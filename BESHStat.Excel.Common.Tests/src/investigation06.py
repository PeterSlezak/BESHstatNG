"""Read-only source/PE investigation. No host activation, COM or process launch."""
import hashlib
import functools
from pathlib import Path
import re
import struct

ROOT = Path(__file__).resolve().parents[2]


def sha(path):
    return hashlib.sha256(Path(path).read_bytes()).hexdigest()


def active_vb(line):
    """Strip comments outside VB strings; commented Handles/AddHandler is not wiring."""
    result = []; quoted = False; index = 0
    while index < len(line):
        ch = line[index]
        if ch == '"':
            if quoted and index+1 < len(line) and line[index+1] == '"':
                result.extend('""'); index += 2; continue
            quoted = not quoted
        if ch == "'" and not quoted: break
        result.append(ch); index += 1
    return ''.join(result)


@functools.lru_cache(maxsize=32)
def source_lines(path):
    return [(i+1, line, active_vb(line)) for i, line in enumerate(Path(path).read_text(encoding='utf-8-sig').splitlines())]


@functools.lru_cache(maxsize=32)
def member_inventory(path):
    rows = source_lines(path); result = []; current = None
    start = re.compile(r'^\s*(?:(?:Public|Private|Friend|Protected|Shared|Overrides|Overloads|Async|ReadOnly|WriteOnly|Default|Overridable|NotOverridable)\s+)*(Sub|Function|Property)\s+(\w+)\b', re.I)
    for number, raw, code in rows:
        hit = start.match(code)
        if hit:
            current = dict(member=hit[2], kind=hit[1], start=number, end=len(rows), lines=[])
            result.append(current)
        if current is not None:
            current['lines'].append(dict(line=number, text=raw.strip(), active=code.strip()))
            if re.match(r'^\s*End (Sub|Function|Property)\s*$', code, re.I):
                current['end'] = number; current = None
    return result


@functools.lru_cache(maxsize=512)
def control_inventory(form, name):
    designer = ROOT / ('BESHstatNG/src/UI/'+form+'.Designer.vb')
    codebehind = ROOT / ('BESHstatNG/src/UI/'+form+'.vb')
    code = source_lines(codebehind); des = source_lines(designer); members = member_inventory(codebehind)
    token = re.compile(r'\b(?:Me\.)?'+re.escape(name)+r'\b', re.I)
    declarations = [dict(line=n, text=raw.strip()) for n, raw, text in des if re.search(r'\b'+re.escape(name)+r'\s+As\s+', text, re.I)]
    initialization = [dict(line=n, text=raw.strip()) for n, raw, text in des if re.search(r'Me\.'+re.escape(name)+r'\s*=\s*New\b', text, re.I)]
    defaults = [dict(line=n, text=raw.strip()) for n, raw, text in des
                if re.search(r'Me\.'+re.escape(name)+r'\.(Checked|Enabled|Visible|Value|Text|SelectedIndex|Parent)\s*=', text, re.I)]
    parents = []
    for index, (n, raw, text) in enumerate(des):
        if '.Controls.Add' in text:
            block = '\n'.join(t for _, _, t in des[index:index+18]) if '.AddRange(' in text else text
            if token.search(block): parents.append(dict(line=n, text=block.strip()))
    uses = []
    for number, raw, text in code:
        if token.search(text):
            member = next((m for m in members if m['start'] <= number <= m['end']), None)
            uses.append(dict(line=number, text=raw.strip(), member=member['member'] if member else 'field/class scope',
                             member_start=member['start'] if member else number,
                             member_end=member['end'] if member else number))
    events = [u for u in uses if re.search(r'\bHandles\b|\bAddHandler\b|\bRemoveHandler\b|WireHelp\(', active_vb(u['text']), re.I)]
    transitions = [u for u in uses if re.search(r'\.(Checked|Enabled|Visible|Parent|Value|Text|SelectedIndex)\s*=', active_vb(u['text']), re.I)]
    calls = []
    used_members = {u['member'] for u in uses}
    names = {m['member'] for m in members}
    for m in members:
        if m['member'] in used_members:
            body = '\n'.join(l['active'] for l in m['lines'])
            targets = sorted(n for n in names if n != m['member'] and re.search(r'\b'+re.escape(n)+r'\s*\(', body, re.I))
            calls.append(dict(member=m['member'],start=m['start'],end=m['end'],candidate_local_calls=targets))
    return dict(control_id='BESHStatNG.'+form+'.'+name,
                sources=[dict(path=p.relative_to(ROOT).as_posix(),sha256=sha(p)) for p in [designer,codebehind]],
                declarations=declarations, initialization=initialization, designer_defaults=defaults,
                parents=parents, runtime_uses=uses, event_wiring=events, state_assignments=transitions,
                local_call_edges=calls,
                unresolved=['Inherited/runtime defaults require initialized native observation',
                            'Indirect helper/reflection/COM event delivery is not proven by token reachability'],
                classification='SOURCE_INFERRED', native_executed=False, owner_released=False)


def pe_imports(path):
    """Bounded PE import-directory inspection; no library loading or DLL execution."""
    data = Path(path).read_bytes()
    def unpack(fmt, offset):
        size = struct.calcsize(fmt)
        if offset < 0 or offset+size > len(data): raise ValueError('truncated PE')
        return struct.unpack_from(fmt, data, offset)
    if data[:2] != b'MZ': raise ValueError('not PE/MZ')
    pe = unpack('<I', 0x3c)[0]
    if data[pe:pe+4] != b'PE\0\0': raise ValueError('bad PE signature')
    machine, count = unpack('<HH', pe+4); optional_size = unpack('<H', pe+20)[0]
    opt = pe+24; magic = unpack('<H', opt)[0]
    if magic not in (0x10b, 0x20b): raise ValueError('optional header type')
    directory = opt+(112 if magic == 0x20b else 96)
    sections = []
    if count > 128: raise ValueError('section count')
    for index in range(count):
        at = opt+optional_size+40*index
        virtual_size, rva, raw_size, raw_offset = unpack('<IIII', at+8)
        sections.append((rva, max(virtual_size, raw_size), raw_offset, raw_size))
    def offset(rva):
        for address, size, raw, raw_size in sections:
            if address <= rva < address+size and rva-address < raw_size:
                return raw+rva-address
        raise ValueError('unmapped RVA')
    def string_at(rva):
        at = offset(rva); end = data.find(b'\0', at, min(len(data), at+4096))
        if end < 0: raise ValueError('unterminated import')
        return data[at:end].decode('ascii')
    imports = []
    rva, size = unpack('<II', directory+8)
    if rva:
        at = offset(rva)
        for i in range(min(1024, size//20 if size else 1024)):
            descriptor = unpack('<IIIII', at+20*i)
            if not any(descriptor): break
            imports.append(string_at(descriptor[3]))
        else: raise ValueError('unbounded import table')
    return dict(machine=hex(machine),bitness=64 if magic == 0x20b else 32,imports=imports,
                limitation='Normal import table only; dynamic LoadLibrary/delay imports/SxS/API-set resolution need separate closure review')


def validate_trace(record, expected_ids):
    rows = record['per_id']; ids = [r['obligation_id'] for r in rows]
    if len(ids) != len(set(ids)) or set(ids) != set(expected_ids): raise ValueError('ID inventory drift')
    groups = {g['group_id'] for g in record['clusters']}
    for row in rows:
        if not row['clusters'] or not set(row['clusters']) <= groups: raise ValueError('unmapped root cause')
        if not row['owner_roles'] or not row['cases'] or not row['source']['declarations']: raise ValueError('missing owner/source/case')
        if row['native_executed'] or row['owner_released'] or row['mitigation_status'] != 'PROPOSED_NOT_REVIEWED':
            raise ValueError('status inflation')
        for pin in row['source']['sources']:
            if sha(ROOT/pin['path']) != pin['sha256']: raise ValueError('source drift')
        form, name = row['control_id'].split('.')[-2:]
        actual = control_inventory(form, name)
        for field in ['declarations', 'initialization', 'designer_defaults', 'parents',
                      'runtime_uses', 'event_wiring', 'state_assignments', 'local_call_edges']:
            if row['source'][field] != actual[field]: raise ValueError('source contract metadata drift: '+field)
    return True
