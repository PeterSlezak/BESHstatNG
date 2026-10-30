"""C20-only static control proofs; C19 checker and immutable proof remain untouched."""
from pathlib import Path
import json,hashlib
ROOT=Path(__file__).resolve().parents[2];EVIDENCE=ROOT/'TestResults/04A-01-continuation-20'
def read(p):return json.loads(p.read_text(encoding='utf-8-sig'))
CONTROL=('control_id','path','line','type_declaration','initialization_defaults','handles_edges')
WITNESS=('symbol_id','path','kind','signature','profile','span_start','span_length','body_sha256','start_line','end_line')
EDGE=('edge_id','path','kind','event_or_control','target','start_line','end_line','owner_id')
def verify(intents,symbols,controls,edges):
 selected=[d for d in intents if d.get('checkpoint_id')=='04A.01-continuation-20']
 if not selected:return []
 groups=read(EVIDENCE/'targets.json');scope={c['control_id']:c for g in groups for c in g['controls']};source={s['symbol_id']:s for g in groups for s in g['records']}
 authored={c['control_id']:c for p in EVIDENCE.glob('group-*-control-evidence.json') for c in read(p)['controls']};ss={s['symbol_id']:s for s in symbols};cc={c['control_id']:c for c in controls};ee={e['edge_id']:e for e in edges};errors=[];seen={};ids=[]
 for d in selected:
  ident=d.get('control_id');ids.append(ident);frozen=authored.get(ident);c=cc.get(ident)
  if ident not in scope or not frozen or frozen.get('intent_completeness')!='qualified':errors.append('Unsupported C20 control promotion '+str(ident));continue
  if d!=frozen:errors.append('Changed authored C20 intent/default/mode/output/lifetime '+ident)
  if c is None:errors.append('Missing C20 declaration '+ident);continue
  try:
   pin=json.loads(d['source_control_pin'])
   if any(pin.get(k)!=c.get(k) or pin.get(k)!=scope[ident].get(k) for k in CONTROL):errors.append('C20 declaration/default/binding drift '+ident)
   typ=ident.rsplit('.',1)[0];own=[s for s in source.values() if s['containing_type']==typ or s['containing_type'].startswith(typ+'.')]
   required=[s['symbol_id'] for s in own if s['name'] in ('.ctor','InitializeComponent','Dispose')]
   if json.loads(d['required_constructor_initialization_ids'])!=required:errors.append('C20 missing constructor/InitializeComponent/Dispose proof '+ident)
   witnesses=json.loads(d['source_witnesses']);wids=[s['symbol_id'] for s in witnesses]
   if len(wids)!=len(set(wids)) or set(wids)!={s['symbol_id'] for s in own}:errors.append('C20 missing handler/state/validation/output/lifecycle body '+ident)
   for w in witnesses:
    actual=ss.get(w['symbol_id'])
    if actual is None or any(actual.get(k)!=w.get(k) for k in WITNESS):errors.append('C20 source witness drift '+ident)
   flow=json.loads(d['control_flow_pins'])
   if not flow or any(i not in wids or i not in ss for i in flow):errors.append('C20 missing control validation/output/state dependency '+ident)
   lifecycle=json.loads(d['form_lifecycle_method_ids']);expected=[s['symbol_id'] for s in own if 'FormClosing' in s['name'] or s['name'] in ('BeginRegressionComputation','EndRegressionComputation')]
   if lifecycle!=expected or not d.get('lifetime_contract'):errors.append('C20 missing closure/cancel lifecycle '+ident)
   bindings=json.loads(d['binding_edge_pins'])
   if not bindings:errors.append('C20 missing declaration/delegate binding '+ident)
   for e in bindings:
    actual=ee.get(e['edge_id'])
    if actual is None or any(actual.get(k)!=e.get(k) for k in EDGE):errors.append('C20 handler/AddHandler/lifecycle edge drift '+ident)
   deps=json.loads(d['dependency_pins'])
   if not {s['path'] for s in own}<=set(deps):errors.append('C20 missing source dependency '+ident)
   for path,digest in deps.items():
    if path not in seen:
     p=ROOT/path;seen[path]=hashlib.sha256(p.read_bytes()).hexdigest() if p.is_file() else None
    if seen[path]!=digest:errors.append('C20 dependency bytes changed '+ident+' '+path)
  except (KeyError,ValueError,TypeError):errors.append('Malformed C20 proof '+str(ident))
 if len(ids)!=len(set(ids)):errors.append('Duplicate C20 control ID')
 return errors
