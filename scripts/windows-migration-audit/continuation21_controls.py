"""C21-only static control proofs; C20 checker and immutable proof remain untouched."""
from pathlib import Path
import json,hashlib,functools
ROOT=Path(__file__).resolve().parents[2];EVIDENCE=ROOT/'TestResults/04A-01-continuation-21'
def read(p):return json.loads(p.read_text(encoding='utf-8-sig'))
CONTROL=('control_id','path','line','type_declaration','initialization_defaults','handles_edges')
WITNESS=('symbol_id','path','kind','signature','profile','span_start','span_length','body_sha256','start_line','end_line')
EDGE=('edge_id','path','kind','event_or_control','target','start_line','end_line','owner_id')

@functools.lru_cache(maxsize=1)
def authored_controls():
 authored={c['control_id']:c for p in EVIDENCE.glob('group-*-control-evidence.json') for c in read(p)['controls']}
 p=EVIDENCE/'control-source-refinements.json'
 if p.exists():
  for change in read(p):
   ident=change['control_id'];assert authored[ident]==change['before'],'Unbound C21 control refinement'
   before,after=change['before'],change['after'];assert {k for k in set(before)|set(after) if before.get(k)!=after.get(k)}<={'binding_edge_pins','inherited_defaults_evidence'}
   authored[ident]=after
 return authored
def verify(intents,symbols,controls,edges):
 selected=[d for d in intents if d.get('checkpoint_id')=='04A.01-continuation-21']
 groups=read(EVIDENCE/'targets.json');scope={c['control_id']:c for g in groups for c in g['controls']};source={s['symbol_id']:s for g in groups for s in g['records']}
 authored=authored_controls();ss={s['symbol_id']:s for s in symbols};cc={c['control_id']:c for c in controls};ee={e['edge_id']:e for e in edges};errors=[];seen={};ids=[]
 expected={i for i,c in authored.items() if c.get('intent_completeness')=='qualified'}
 if {d.get('control_id') for d in selected}!=expected:errors.append('Missing or foreign qualified C21 control IDs')
 for d in selected:
  ident=d.get('control_id');ids.append(ident);frozen=authored.get(ident);c=cc.get(ident)
  if ident not in scope or not frozen or frozen.get('intent_completeness')!='qualified':errors.append('Unsupported C21 control promotion '+str(ident));continue
  if d!=frozen:errors.append('Changed authored C21 intent/default/mode/output/lifetime '+ident)
  if c is None:errors.append('Missing C21 declaration '+ident);continue
  try:
   pin=json.loads(d['source_control_pin'])
   if any(pin.get(k)!=c.get(k) or pin.get(k)!=scope[ident].get(k) for k in CONTROL):errors.append('C21 declaration/default/binding drift '+ident)
   typ=ident.rsplit('.',1)[0];own=[s for s in source.values() if s['containing_type']==typ or s['containing_type'].startswith(typ+'.')]
   required=[s['symbol_id'] for s in own if s['name'] in ('.ctor','InitializeComponent','Dispose')]
   if json.loads(d['required_constructor_initialization_ids'])!=required:errors.append('C21 missing constructor/InitializeComponent/Dispose proof '+ident)
   witnesses=json.loads(d['source_witnesses']);wids=[s['symbol_id'] for s in witnesses]
   if len(wids)!=len(set(wids)) or set(wids)!={s['symbol_id'] for s in own}:errors.append('C21 missing handler/state/validation/output/lifecycle body '+ident)
   for w in witnesses:
    actual=ss.get(w['symbol_id'])
    if actual is None or any(actual.get(k)!=w.get(k) for k in WITNESS):errors.append('C21 source witness drift '+ident)
   flow=json.loads(d['control_flow_pins'])
   if not flow or any(i not in wids or i not in ss for i in flow):errors.append('C21 missing control validation/output/state dependency '+ident)
   lifecycle=json.loads(d['form_lifecycle_method_ids']);expected=[s['symbol_id'] for s in own if 'FormClosing' in s['name'] or s['name'] in ('BeginRegressionComputation','EndRegressionComputation')]
   if lifecycle!=expected or not d.get('lifetime_contract'):errors.append('C21 missing closure/cancel lifecycle '+ident)
   bindings=json.loads(d['binding_edge_pins'])
   if not bindings:errors.append('C21 missing declaration/delegate binding '+ident)
   for e in bindings:
    actual=ee.get(e['edge_id'])
    if actual is None or any(actual.get(k)!=e.get(k) for k in EDGE):errors.append('C21 handler/AddHandler/lifecycle edge drift '+ident)
   deps=json.loads(d['dependency_pins'])
   if not {s['path'] for s in own}<=set(deps):errors.append('C21 missing source dependency '+ident)
   for path,digest in deps.items():
    if path not in seen:
     p=ROOT/path;seen[path]=hashlib.sha256(p.read_bytes()).hexdigest() if p.is_file() else None
    if seen[path]!=digest:errors.append('C21 dependency bytes changed '+ident+' '+path)
  except (KeyError,ValueError,TypeError):errors.append('Malformed C21 proof '+str(ident))
 if len(ids)!=len(set(ids)):errors.append('Duplicate C21 control ID')
 return errors
