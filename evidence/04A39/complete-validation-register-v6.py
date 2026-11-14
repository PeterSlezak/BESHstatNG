"""Add frozen W3 validation/gate obligations and exact original member signatures; no source/test repair."""
from common39 import *
from collections import Counter, defaultdict
import csv
import re

source_lock()
d=load(HERE/'register/defects-v5.json');by_id={x['canonical_id']:x for x in d['issues']}
original_path=W3/'final/unresolved-exceptions-v2.json';records=load(original_path)['entries']
assert len(records)==len({x['id'] for x in records})==163
source_files=git('ls-files').decode().splitlines()+git('ls-files','--others','--exclude-standard').decode().splitlines()
inventory={x['id']:x for x in load(HERE/'python-node-scanners/campaign-v2/python-discovery-inventory-v1.json')['entries']}
old_events=load(W3/'python-node-scanners/python-identities-v1.json')['events']
old_events_by_parent=defaultdict(list)
for e in old_events:old_events_by_parent[e['identity']].append(e)
current=load(HERE/'python-node-scanners/python-identities-v1.json')
current_parents={x['id']:x for x in current['parents']}
template=next(x for x in d['issues'] if x['canonical_id']=='ISSUE:A39-PAIRED-SOURCE-POSITION-01')

def new_entry(key,kind):
    row=json.loads(json.dumps(template))
    for field in ['aliases','source_locations','original_current_evidence','affected_tests','original_obligations','original_controls',
        'obligation_facets','report_occurrences','current_declaration_candidates']:
        row[field]=[]
    row.update(canonical_id=key,record_kind=kind,status='INHERITED_VALIDATION_NONPASS_NOT_WAIVED',
        observed_vs_suspected='ACTUAL_W3_NONPASS;CURRENT_OUTCOME_SEPARATELY_BOUND',description='Unwaived validation or restricted gate record.',
        discovery_provenance=[pin(original_path)],reproducer=dict(source='Exact original validation record/test source; source command receipts retained.'),
        current_behavior='Inherited nonpass is not waived; current39 scope and outcome are stated separately.',proposed_action='Investigate exact source/test/policy condition and obtain any required separate owner decision.',
        authorization='REPORTING_AND_INVESTIGATION_ONLY;NO_WAIVER_OR_SOURCE_REPAIR',dependency_phase=['04A.39 reporting','04A.40–48 applicable owner'],
        host_owners=['Test/source-family maintainer; owner'],responsible_future_owner=['Test/source-family maintainer; owner'],
        unknown_blocked_reason='Underlying scientific/native obligation is not discharged by software outcomes.',
        historical_validation_records=[],current_event_records=[],original_event_records=[])
    return row

for record in records:
    old_id=record['id']
    if old_id.startswith('MANAGED:'):
        parts=old_id.split(':');key='VALIDATION:MANAGED:'+':'.join(parts[2:]);kind='MANAGED_NONPASS'
    elif old_id.startswith('PYTHON_PARENT:'):key='VALIDATION:'+old_id;kind='PYTHON_PARENT_NONPASS'
    elif old_id in ['STRICT_NORMALPLOT','PROFILE']:key='VALIDATION:'+old_id;kind='SCANNER_NONPASS'
    else:key='GATE:'+old_id;kind='RESTRICTED_GATE'
    if key not in by_id:by_id[key]=new_entry(key,kind)
    row=by_id[key];row['aliases'].append(old_id)
    row['historical_validation_records'].append(dict(record=record,record_sha256=sha(json.dumps(record,sort_keys=True,ensure_ascii=False).encode())))
    row['description']=record.get('identity',old_id)
    row['original_current_evidence'].append(pin(original_path))
    if kind=='PYTHON_PARENT_NONPASS':
        identity=old_id[len('PYTHON_PARENT:'):]
        row['affected_tests']=[identity];row['current_parent']=current_parents[identity]
        row['original_event_records']=old_events_by_parent[identity]
        row['current_event_records']=[x for x in current['events'] if x['identity']==identity]
        name=identity.split('.')[0]+'.py'
        paths=[p for p in source_files if Path(p).name==name and any(p.startswith(s) for s in ['scripts/tests/','BESHstatNG/tools/tests/','BESHStatNG_Help_MkDocs/tests/','BESHStat.Excel.Common.Tests/src/'])]
        assert len(paths)==1,(identity,paths)
        path=paths[0];method=identity.rsplit('.',1)[-1];lines=(ROOT/path).read_text(encoding='utf-8-sig').splitlines()
        line=next((i for i,t in enumerate(lines,1) if re.match(r'\s*(?:async\s+)?def\s+'+re.escape(method)+r'\s*\(',t)),None)
        assert line
        row['source_locations'].append(dict(preimage=pin(path),member_signature=lines[line-1].strip(),line=line,
            branch='Actual Python parent; individual subtests/messages/traces retained in event records.'))
    elif kind=='MANAGED_NONPASS':
        identity=record['identity'];row['affected_tests']=[identity]
        method=identity.rsplit('.',1)[-1];class_name=identity.rsplit('.',2)[-2]
        paths=[p for p in source_files if p.endswith('.vb') and Path(p).stem==class_name and not p.startswith(('evidence/','TestResults/'))]
        for path in paths:
            lines=(ROOT/path).read_text(encoding='utf-8-sig').splitlines()
            found=[i for i,t in enumerate(lines,1) if re.search(r'\b(?:Sub|Function)\s+'+re.escape(method)+r'\s*\(',t)]
            for line in found:row['source_locations'].append(dict(preimage=pin(path),member_signature=lines[line-1].strip(),line=line,
                branch='Test definition; a failed test is not automatically adjudicated as a statistical defect.'))
        assert row['source_locations'],identity
    elif kind=='SCANNER_NONPASS':
        path='scripts/windows-migration-audit/'+('Program.cs' if old_id=='STRICT_NORMALPLOT' else 'owner_closeout.py')
        row['source_locations']=[dict(preimage=pin(path),member_signature='Audit.Classify' if old_id=='STRICT_NORMALPLOT' else 'check(history=False,index=False) / main()',
            branch='Actual strict ambiguity' if old_id=='STRICT_NORMALPLOT' else 'CLI reports opaque INTEGRITY_FAILURE; exact originating assertion not exposed. No successful profile claim.')]
    else:
        row['source_locations']=[dict(preimage=pin(HERE/'entry/merge-preflight-v1.json'),member_signature='Owner assertion and independently authenticated merge/EOL gate',branch='Gate authority, not product inference.')]
        row['status']='REQUIRES_OWNER_DECISION'
        if old_id=='04A39':
            row['status']='OWNER_AUTHORIZED_04A39_ONLY'
            row['current_behavior']='Old W3 downstream block preserved historically; current owner39 prompt plus accepted38 merge and explicit EOL approval permits39 only. No04A40 authority.'
    d['crosswalk'].append(dict(kind='W3_VALIDATION_GATE_RECORD',original_id=old_id,canonical_id=key,
        original_record_sha256=sha(json.dumps(record,sort_keys=True,ensure_ascii=False).encode())))

for event in old_events:
    key='VALIDATION:PYTHON_PARENT:'+event['identity']
    event_id=sha(json.dumps([event['identity'],event['outcome'],event.get('subtest'),event['occurrence']],ensure_ascii=False).encode())
    d['crosswalk'].append(dict(kind='W3_PYTHON_EVENT',original_id=event_id,canonical_id=key,
        identity=event['identity'],outcome=event['outcome'],subtest=event.get('subtest'),occurrence=event['occurrence']))

# Map every immutable referenced original member ID to the original exact signature/body/source record.
symbols_path='docs/architecture/WindowsMigrationLedger/symbols.csv'
with (ROOT/symbols_path).open(encoding='utf-8-sig',newline='') as stream:symbols={x['symbol_id']:x for x in csv.DictReader(stream)}
def symbol_ids(value):
    result=set()
    if isinstance(value,dict):
        if isinstance(value.get('symbol_id'),str):result.add(value['symbol_id'])
        for child in value.values():result|=symbol_ids(child)
    elif isinstance(value,list):
        for child in value:result|=symbol_ids(child)
    return result
resolved=set()
for row in by_id.values():
    ids=symbol_ids(row['obligation_facets'])
    assert ids <= symbols.keys()
    row['original_linked_member_signatures']=[{k:symbols[identity].get(k) for k in ['symbol_id','signature_key','signature','path','start_line','end_line',
        'body_sha256','source_sha256','source_profile','source_span_start','source_span_length','source_span_unit']} for identity in sorted(ids)]
    resolved|=ids
assert len(resolved)==759
d['authority_pins'].append(pin(symbols_path));d['authority_pins'].append(pin(original_path))
d['issues']=[by_id[k] for k in sorted(by_id)];d['version']=6;d['schema']='BESHSTAT_CANONICAL_DEFECT_OBLIGATION_REGISTER_04A39_V6'
d['previous']=pin(HERE/'register/defects-v5.json')
d['counts'].update(canonical_entries=len(by_id),original_member_ids_resolved=759,W3_validation_gate_records=163,W3_python_events=198,
    statuses=dict(Counter(x['status'] for x in by_id.values())))
d['postcampaign_derivation']='Reporting-only completeness amendment from already frozen immutable W3 records and unchanged tracked original symbols. No production/test/input/oracle changes; no campaign rerun. All added derivative artifacts get publication privacy/readback controls.'
save('register/defects-v6.json',d)
save('register/current-v4.json',dict(canonical=pin(HERE/'register/defects-v6.json'),version=6,supersedes=pin(HERE/'register/current-v3.json'),
    reason='Explicit163 W3 validation/gate records,198 individual event mappings and759 exact original member signatures; no original record changed or discharged.'))
save('register/validation-record-crosswalk-v1.json',dict(source=pin(original_path),records=163,events=198,original_member_signatures=759,
    mappings=[x for x in d['crosswalk'] if x['kind'] in ['W3_VALIDATION_GATE_RECORD','W3_PYTHON_EVENT']],
    gate04A39='Only historical downstream block superseded by explicit current owner gate;P/N1/N2/C2/C3/NOT_GOLDEN unchanged.'))
index=['# Canonical04A.39 defect and obligations register','',
    'Authority: [current-v4.json](current-v4.json) → [defects-v6.json](defects-v6.json). This supersedes earlier additive versions. '+
    'All original facets/IDs and W3 unwaived validation/gate records remain.759 referenced original member IDs resolve exactly to immutable signatures/body/source hashes. '+
    'Validation records are not automatically statistical defects; no04A.48 exit disposition is assigned.','',
    '| Canonical ID | Kind/status | Original obligations | Aliases |','|---|---|---:|---|']
for row in d['issues']:index.append('| '+row['canonical_id']+' | '+row['status']+' | '+str(len(row['original_obligations']))+' | '+', '.join(row['aliases'])+' |')
(HERE/'register/index-v6.md').write_text('\n'.join(index)+'\n',encoding='utf-8',newline='\n')
print(json.dumps(d['counts']),flush=True)
