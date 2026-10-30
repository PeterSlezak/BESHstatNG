"""C19 source-backed control integrity, additional to the legacy nonempty-field predicate.

Does not qualify controls by type/Designer/Handles. Qualification is individually
authored; this verifier rejects stale or unsupported proof even for nonempty v1 rows.
"""
from __future__ import annotations
import json,hashlib
from pathlib import Path
ROOT=Path(__file__).resolve().parents[2]
EVIDENCE=ROOT/'TestResults/04A-01-continuation-19'
def read(p):return json.loads(p.read_text(encoding='utf-8-sig'))
FIELDS=('control_id','path','line','type_declaration','initialization_defaults','handles_edges')
WITNESS=('symbol_id','path','kind','signature','profile','span_start','span_length','body_sha256','start_line','end_line')
EDGE=('edge_id','path','kind','event_or_control','target','start_line','end_line','owner_id')
def verify(intents,symbols,controls,edges):
    selected=[d for d in intents if d.get('checkpoint_id')=='04A.01-continuation-19']
    if not selected:return []
    targets=read(EVIDENCE/'targets.json');source={s['symbol_id']:s for g in targets for s in g['records']};scope={c['control_id']:c for g in targets for c in g['controls']}
    authored={c['control_id']:c for p in EVIDENCE.glob('group-*-control-evidence.json') for c in read(p)['controls']}
    ss={s['symbol_id']:s for s in symbols};cc={c['control_id']:c for c in controls};ee={e['edge_id']:e for e in edges};errors=[];ids=[];seen={}
    for d in selected:
        ident=d.get('control_id');ids.append(ident);frozen=authored.get(ident);c=cc.get(ident)
        if ident not in scope or frozen is None or frozen.get('intent_completeness')!='qualified':errors.append('Unsupported C19 complete control '+str(ident));continue
        if d!=frozen:errors.append('C19 authored control contract changed '+ident)
        if c is None:errors.append('Missing C19 control '+ident);continue
        try:
            pin=json.loads(d['source_control_pin'])
            if any(pin.get(k)!=c.get(k) or pin.get(k)!=scope[ident].get(k) for k in FIELDS):errors.append('C19 declaration/default/control binding drift '+ident)
            typ=ident.rsplit('.',1)[0];own=[s for s in source.values() if s['containing_type']==typ or s['containing_type'].startswith(typ+'.')]
            required=[s['symbol_id'] for s in own if s['name'] in ('.ctor','InitializeComponent','Dispose')]
            if json.loads(d['required_constructor_initialization_ids'])!=required:errors.append('Missing C19 constructor/initialization/disposal proof '+ident)
            witnesses=json.loads(d['source_witnesses']);actual={s['symbol_id'] for s in witnesses}
            if len(witnesses)!=len(actual) or actual!={s['symbol_id'] for s in own}:errors.append('Missing/duplicate C19 state/handler/delegate body proof '+ident)
            for w in witnesses:
                s=ss.get(w['symbol_id'])
                if s is None or any(w.get(k)!=s.get(k) for k in WITNESS):errors.append('C19 handler/constructor/source span/body drift '+ident)
            bindings=json.loads(d['binding_edge_pins'])
            if not bindings:errors.append('No C19 declaration/event binding proof '+ident)
            for b in bindings:
                e=ee.get(b['edge_id'])
                if e is None or any(b.get(k)!=e.get(k) for k in EDGE):errors.append('C19 event/option/delegate binding drift '+ident)
            deps=json.loads(d['dependency_pins'])
            if not {s['path'] for s in own}<=set(deps):errors.append('Missing C19 owning source dependency '+ident)
            for path,digest in deps.items():
                if path not in seen:
                    p=ROOT/path;seen[path]=hashlib.sha256(p.read_bytes()).hexdigest() if p.is_file() else None
                if seen[path]!=digest:errors.append('C19 invalid control dependency '+ident+' '+path)
        except (KeyError,ValueError,TypeError):errors.append('Malformed C19 control evidence '+str(ident))
    if len(ids)!=len(set(ids)):errors.append('Duplicate C19 complete control')
    return errors
