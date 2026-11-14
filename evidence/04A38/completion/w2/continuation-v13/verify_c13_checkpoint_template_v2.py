"""Independent complete W2 source/compiled/ownership reader. Always NOT_READY."""
import hashlib,json,posixpath,sys
from pathlib import Path
from collections import Counter
from source_views import body
from proof_kernel_v6 import need,definitions,semantic,bind,closure,map_current
from verify_c13_composition_v3 import inspect as composition,tokens
ROOT=Path(__file__).resolve().parents[5];HERE=Path(__file__).resolve().parent
PREFIX=HERE.relative_to(ROOT).as_posix()+'/';OLD=PREFIX.replace('v13/','v12/')
AUTH={}

def compute(root=ROOT,overrides=None,check_validation=True):
 root=Path(root);overrides=overrides or {};cache={}
 def raw(p):
  p=posixpath.normpath(p)
  if p not in cache:cache[p]=overrides[p] if p in overrides else (root/p).read_bytes()
  return cache[p]
 load=lambda p:json.loads(raw(p))
 def check(r):need((len(raw(r['path'])),hashlib.sha256(raw(r['path'])).hexdigest())==(r['bytes'],r['sha256']),'Changed bytes '+r['path'])
 for p,h in AUTH.items():need(hashlib.sha256(raw(p)).hexdigest()==h,'Sealed C13 input changed '+p)
 ts=load(PREFIX+'source-transitions-final-v1.json')['files'];need(len(ts)==len({t['path'] for t in ts})==7,'Source transition set changed')
 projected=dict(overrides)
 for t in ts:
  check(t['after']);check(t['snapshot']);need(raw(t['path'])==raw(t['snapshot']['path']),'Snapshot mismatch')
  if t['before']:
   check(t['preimage']);need((t['before']['bytes'],t['before']['sha256'])==(t['preimage']['bytes'],t['preimage']['sha256']),'Wrong preimage');projected[t['path']]=raw(t['preimage']['path'])
  else:need(t['entry_state']=='ABSENT','New source lacks absent-entry proof')
 saved=list(sys.path);sys.path.insert(0,str(root/OLD))
 try:
  from verify_c12_checkpoint_v2 import compute as prior_compute
  prior=prior_compute(root,projected)
 finally:sys.path[:]=saved
 need(prior['historical']==dict(total=704,closed=435,OPEN=269) and prior['current']==dict(total=726,closed=465,OPEN=261),'Prior sealed ownership drift')
 u=load(PREFIX+'current-universe-packet-v1.json');old=load(OLD+'current-ownership-v2.json');oh=load(OLD+'historical-ownership-v2.json')['members'];o=load('evidence/04A38/completion/w1/original-universe.json')
 mapping=map_current(old['members'],u['members'],load(PREFIX+'callback-identity-context-v1.json')['mapping']);members={n['symbol_id']:n for n in u['members']}
 need(len(members)==len(mapping)==726,'Native universe changed');inherited={mapping[n['symbol_id']]['symbol_id'] for n in old['members'] if not n['review_status'].startswith('OPEN')}
 need(len(inherited)==465 and all(mapping[n['symbol_id']]['body_sha256']==n['body_sha256'] for n in old['members'] if not n['review_status'].startswith('OPEN')),'Inherited supported body changed')
 cs=load(PREFIX+'whole-body-contracts-v1.json')['contracts'];cb=load(PREFIX+'callback-proofs-v1.json')['rows'];pc=load(PREFIX+'portable-api-contracts-v1.json')['contracts']
 need(len(cs)==96 and len(cb)==19 and len({c['current_id'] for c in cs+cb})==115,'Complete executable cohort changed')
 shapes={r['symbol_id']:r for r in load(PREFIX+'declaration-shapes-v1.json')['rows']};om={n['symbol_id']:n for n in o['members']};hm={h['canonical_original_id']:h for h in oh}
 snapshots={r['source']['path']:r['snapshot'] for r in load('evidence/04A38/preedit/entry-source-snapshot.json')['files']}
 for c in cs+cb:
  n=members[c['current_id']];need(n==c['current_record'],'Wrong complete current identity');check(c.get('current_source',c.get('source')))
  text,v=body(raw(n['path']),n);need(text==c.get('current_whole_body',c.get('whole_body')) and v==c.get('current_source_view',c.get('source_view')),'Current source/body hash/span mismatch')
  if c['original_id']:
   a=om[hm[c['original_id']]['git_original_id']];need(a==c['original_record'] and snapshots[a['path']]==c['original_source'],'Original lineage identity changed');check(c['original_source'])
   text,v=body(raw(c['original_source']['path']),a);need(text==c['original_whole_body'] and v==c['original_source_view'],'Original body/normalization mismatch')
 eligible=[c for c in cs if c['proof_status']!='OPEN_BYREF_PARITY_DEPENDENCY']+cb
 blocked={c['current_record']['name'] for c in cs if c['proof_status']=='OPEN_BYREF_PARITY_DEPENDENCY'}
 need(blocked=={'GuessPositiveClass','TryGetRocInput','ROC_POINTS','ROC_STATS'},'ByRef mismatch waived')
 closed=inherited|{c['current_id'] for c in eligible};newhist={c['original_id'] for c in eligible if c['new_support'] and c['original_id']}
 ph=load(PREFIX+'portable-historical-callback-proof-v1.json');newhist.add(ph['original_id'])
 need(len(newhist)==26 and len(closed-inherited)==29,'New historical/current cohort changed')
 diag=load(PREFIX+'cohort-diagnostic-v1.json');inputs=load(PREFIX+'proof-inputs-v1.json');deltas=load(PREFIX+'compiled-method-deltas-v1.json')['configurations'];retention=[]
 for config in ['Debug','Release']:
  meta=load(PREFIX+'metadata-all/packet-v1-context-v5/current-'+config+'.json');orig=load(PREFIX+'metadata-all/packet-v1-context-v5/original-'+config+'.json');before=load(OLD+'metadata-all/packet-v1-context-v5/current-'+config+'.json')
  need(raw(PREFIX+'metadata-all/packet-v1-context-v5/original-'+config+'.json')==raw(OLD+'metadata-all/packet-v1-context-v5/original-'+config+'.json') and orig['original_semantic_calls']==0,'Frozen original CIL/provenance changed')
  defs=definitions(meta);od=definitions(orig);bd=definitions(before);ret=load(PREFIX+'return-metadata-'+config+'-v1.json')['methods']
  graph=closure(meta,[bind(members[k],meta,shapes)['method'] for k in inputs['roots']]);need(graph==diag['graphs'][config] and len(graph['methods'])==730,'Missing executable dependency')
  need({n['symbol_id'] for n in u['members'] if n['parent_member'] is None and (n['owner'],n['name']) in {(defs[k]['declaring_type'],defs[k]['member_name']) for k in graph['methods']}}=={c['current_id'] for c in cs},'Native dependency omitted')
  for c in cs:
   method=bind(c['current_record'],meta,shapes);entry=next(r for r in c['compiled'] if r['configuration']==config)
   need(entry['method']==method['method'] and entry['transitive']==closure(meta,[method['method']]) and entry['source_parameters']==shapes[c['current_id']],'Compiled consumer/parameter graph forged')
   need(entry['return_declaration'] in ret,'Actual return boundary missing')
   if c['original_id']:
    b=entry['original_method'];need(b==od[b['method']] and b['parameter_types']==method['parameter_types'],'Original CIL/parameters changed')
    need(entry['original_return'] in load('evidence/04A38/preedit/compiled-'+config+'.json')['methods'] and entry['original_return']['return_type']==entry['return_declaration']['return_type'],'Original return ABI mismatch')
  targets={}
  for c in cb:
   n=c['current_record'];parent=members[n['parent_member']];key=bind(parent,meta,shapes)['method'] if parent['parent_member'] is None else targets[parent['symbol_id']]
   siblings=sorted([n for n in u['members'] if n['parent_member']==parent['symbol_id']],key=lambda n:n['span_start']);actual=[x['operand'][7:] for x in defs[key]['instructions'] if x['op']=='ldftn' and x['operand'][7:] in defs and defs[x['operand'][7:]]['member_name'].startswith('_Lambda$__')]
   need(len(siblings)==len(actual),'Callback count/ldftn mismatch');target=actual[siblings.index(n)];entry=next(r for r in c['configurations'] if r['configuration']==config)
   need(entry['target']==target and entry['parent_method']==key and entry['complete_method']==defs[target] and entry['return_declaration'] in ret,'Callback identity/CIL/return mismatch')
   need(entry['declaring_type']==next(t for t in meta['types'] if t['type']==defs[target]['declaring_type']) and entry['constructors']==[m for m in meta['methods'] if m['declaring_type']==defs[target]['declaring_type'] and m['member_name'] in ['.ctor','.cctor']],'Callback field/constructor mismatch')
   if c['original_id']:
    b=entry['original_binding'];need(b['target']==od[b['target']['method']] and b['parent'] in od and semantic(b['target'])==semantic(defs[target]),'Original callback CIL changed')
   targets[n['symbol_id']]=target
  for p in pc:
   text,v=body(raw(p['source_record']['path']),p['source_record']);need(text==p['whole_body'] and v==p['source_view'],'Portable body mismatch')
   entry=next(r for r in p['compiled'] if r['configuration']==config);need(bind(p['source_record'],meta,shapes)==entry['method'] and not entry['context']['fields'],'Portable actual compiled implementation missing')
  changed={k for k in bd if k in defs and semantic(bd[k])!=semantic(defs[k])};added=set(defs)-set(bd);d=next(r for r in deltas if r['configuration']==config)
  need(not set(bd)-set(defs) and changed=={r['method'] for r in d['changed']} and all(r['original']==bd[r['method']] and r['current']==defs[r['method']] for r in d['changed']) and {r['method'] for r in d['added']}==added and len(changed)==8 and len(added)==17,'Unexpected compiled transition')
  for r in next(r for r in load(PREFIX+'constructed-owner-proofs-v1.json')['configurations'] if r['configuration']==config)['contexts']:
   need(r['type']==next(t for t in meta['types'] if t['type']==r['owner']) and r['constructors_and_initializers']==[m for m in meta['methods'] if m['declaring_type']==r['owner'] and m['member_name'] in ['.ctor','.cctor']],'Constructed owner context omitted')
  exp=load(PREFIX+'inspection/packet-v1/Inspect38-'+config+'.json')['exports'];need(len(exp)==268 and sorted(exp,key=lambda r:r['effective_excel_name'])==sorted(load('evidence/04A38/preedit/compiled-'+config+'.json')['exports'],key=lambda r:r['effective_excel_name']),'Public export ABI changed')
  # Original lambda return ABI is fixed by actual archived closed generic delegate constructors, not a fabricated reflection row.
  p=next(p for p in ph['compiled'] if p['configuration']==config);need(p['original_callback']==od[p['original_callback']['method']] and p['current_callback']==defs[p['current_callback']['method']],'Portable historical callback target changed')
  content=lambda s:'\n'.join(line.strip() for line in s.splitlines()[1:] if line.strip())
  need(content(p['original_normalizer_source'])==content(p['current_normalizer_source']),'Portable callback normalizer source mismatch')
  a=p['original_callback'];b=p['current_callback'];left=p['original_normalizer']['method'];right=p['current_normalizer']['method']
  need([dict(i,operand='METHOD:'+right) if i.get('operand')=='METHOD:'+left else i for i in a['instructions']]==b['instructions'] and all(a[k]==b[k] for k in ['parameter_types','locals','exceptions']),'Portable callback full CIL normalization mismatch')
  need(a['method'] in [i['operand'][7:] for i in od[p['original_parent']]['instructions'] if i['op']=='ldftn'] and b['method'] in [i['operand'][7:] for i in defs[p['current_parent']]['instructions'] if i['op']=='ldftn'],'Portable historical callback actual construction missing')
  for r in load(PREFIX+'callback-identity-context-v1.json')['rows']:
   if r['configuration']!=config:continue
   need(r['complete_callback']==defs[r['actual_ldftn']] and r['actual_ldftn'] in [i['operand'][7:] for i in defs[r['current_parent']]['instructions'] if i['op']=='ldftn'],'Separate KM callback mismatch')
   method=defs[r['portable_method']['method']];index=r['portable_select_instruction'];need(method==r['portable_method'] and any(i['op']=='ldarg.'+str(r['parameter_slot']) for i in method['instructions'][max(0,index-5):index]),'Wrong KM count/fill selector argument slot')
  for owner,suffix in [('BESHStatNG.ExcelCommon.UdfPlotFiniteDoubleReader38','System.Double&'),('BESHStatNG.ExcelCommon.UdfPlotStatusReader38','System.Int32&')]:
   methods=[m for m in meta['methods'] if m['declaring_type']==owner];need(len(methods)==4 and all(m['implementation_flags']=='CodeTypeMask' and not m['instructions'] and not m['exceptions'] for m in methods),'CLR delegate implementation boundary changed')
   invoke=next(m for m in methods if m['member_name']=='Invoke');need(invoke['parameter_types']==['System.Object',suffix] and any(r['declaring_type']==owner and r['member_name']=='Invoke' and r['return_type']=='System.Boolean' for r in ret),'Typed ByRef delegate return ABI changed')
  retention.append(dict(configuration=config,reached=730,exports=268,changed_native=8,added=17))
 from verify_c13_host_contexts_v1 import inspect as host_check
 host_check(load,raw,PREFIX,o,u,{c:load(PREFIX+'metadata-all/packet-v1-context-v5/current-'+c+'.json') for c in ['Debug','Release']})
 need(composition(root,overrides)==load(PREFIX+'source-composition-proof-v1.json'),'Whole-body composite changed')
 hist=load(PREFIX+'historical-ownership-v2.json')['members'];cur=load(PREFIX+'current-ownership-v2.json')
 need([r['canonical_original_id'] for r in hist]==[r['canonical_original_id'] for r in oh] and len(hist)==704,'Historical universe changed')
 for h,b in zip(hist,oh):
  need(bool(h['proof'])==(bool(b['proof']) or h['canonical_original_id'] in newhist),'Unsupported historical closure')
  if h['canonical_original_id'] not in newhist:need(h['proof']==b['proof'] and h['current_ids']==[mapping[k]['symbol_id'] for k in b['current_ids']],'Retained historical proof changed')
 need(Counter(n['symbol_id'] for n in cur['members'])==Counter(members.keys()) and all((not n['review_status'].startswith('OPEN'))==(n['symbol_id'] in closed) for n in cur['members']),'Current OPEN complement changed')
 olds={(s['path'],s['kind'],s['owner'],s['declaration']):s for s in old['support']};need(Counter(s['support_id'] for s in cur['support'])==Counter(s['support_id'] for s in u['supports']),'Support universe changed')
 for s in cur['support']:
  before=olds[s['path'],s['kind'],s['owner'],s['declaration']];expected=before['fully_reconciled'] or s['declaration']=='Public Module MultivariateAnalysisUDFs'
  need(s['fully_reconciled']==expected,'Unsupported support closure')
  if not before['fully_reconciled'] and expected:
   base=s['owner']+'.'+s['declaration'].split()[-1];need(all(n['symbol_id'] in closed for n in u['members'] if n['path']==s['path'] and (n['owner']==base or n['owner'].startswith(base+'.'))),'Module has unclosed child')
 hclosed={h['git_original_id'] for h in hist if h['proof']};headers=load(PREFIX+'support-fragment-crosswalk-v1.json')
 def actual_headers(universe):
  found={};keys={(s['path'],s['kind'],s['owner'],s['declaration']) for s in cur['support'] if s['fully_reconciled']}
  for s in universe['supports']:
   if (s['path'],s['kind'],s['owner'],s['declaration']) not in keys:continue
   if s['kind']=='ModuleBlock':kinds=['ModuleStatement','EndModuleStatement']
   elif s['kind']=='ClassBlock' and s['owner']=='BESHStatNG.WorksheetFunctions.MultivariateAnalysisUDFs' and s['declaration'].startswith('Private Class '):kinds=['ClassStatement','EndClassStatement']
   elif s['kind']=='PropertyStatement' and s['owner'].startswith('BESHStatNG.WorksheetFunctions.MultivariateAnalysisUDFs.'):kinds=['PropertyStatement']
   else:continue
   for f in universe['fragments']:
    if f['member_id'] is None and f['path']==s['path'] and f['kind'] in kinds and s['span_start']<=f['span_start'] and f['span_start']+f['span_length']<=s['span_start']+s['span_length']:found[f['fragment_id']]=s['support_id']
  return found
 need(headers==dict(current=actual_headers(u),original=actual_headers(o)),'Support fragment/header authority forged')
 measured_fragments={}
 for population,universe,rows in [('current',u,cur['fragments']),('original',o,load(PREFIX+'original-fragment-parent-crosswalk-v2.json')['rows'])]:
  need(Counter(f['fragment_id'] for f in rows)==Counter(f['fragment_id'] for f in universe['fragments']),'Missing fragment')
  fm={f['fragment_id']:f for f in universe['fragments']}
  for f in rows:
   src=fm[f['fragment_id']];covered=src['member_id'] in (closed if population=='current' else hclosed) or src['fragment_id'] in headers[population]
   need(all(f[k]==v for k,v in src.items()) and ((f['review_status']!='OPEN') if population=='current' else f['covered'])==covered,'Wrong fragment disposition')
  predicate=lambda f:f['member_id'] in (closed if population=='current' else hclosed) or f['fragment_id'] in headers[population]
  measured_fragments[population]=dict(total=len(rows),covered=sum(predicate(f) for f in rows),branches=sum(f['is_branch'] for f in rows),branches_covered=sum(f['is_branch'] and predicate(f) for f in rows),inline=sum(f['is_inline'] for f in rows),inline_covered=sum(f['is_inline'] and predicate(f) for f in rows))
 cross=load(PREFIX+'new-parent-fragment-crosswalk-v2.json');newgit={hm[k]['git_original_id'] for k in newhist};need(Counter(r['fragment_id'] for r in cross['rows'])==Counter(f['fragment_id'] for f in o['fragments'] if f['member_id'] in newgit) and not cross['unmatched_branch_inline'],'New genuine fragment population incomplete')
 summary=load(PREFIX+'ownership-summary-v2.json');need(summary['historical']==dict(total=704,closed=461,OPEN=243) and summary['current']==dict(total=726,closed=494,OPEN=232) and summary['support']['distinct_closed']==253,'Census differs from exact closures')
 need(summary['fragments']==measured_fragments,'Fragment denominator/coverage forged')
 need(len(load(PREFIX+'new-closures-final-v1.json')['rows'])==26,'New closure ledger changed')
 q=load(PREFIX+'remaining-work-queue-v1.json');expected=[r for r in load(OLD+'remaining-work-queue-v1.json')['members'] if r['original_id'] not in newhist]
 need(q['members']==expected and q['first_members']==expected[:24] and Counter(r['current_id'] for r in q['current_OPEN'])==Counter(set(members)-closed),'Remaining queue/order changed')
 r=load(PREFIX+'readiness-v1.json');need((r['status'],r['W2'],r['W3'],r['P'],r['N1'],r['N2'],r['C2'],r['C3'],r['science'],r['phase04A39'])==('NOT_READY_04A38','IN_PROGRESS','NOT_STARTED','NO','NO GO','NO','UNMET','UNAPPROVED','NOT_GOLDEN','BLOCKED'),'False readiness/stage')
 comparisons=load(PREFIX+'compatibility-packet-v1.json')['results'];priorcmp={(r['configuration'],r['corpus']):r for r in load(OLD+'compatibility-packet-v1.json')['results']};totals=Counter();seen=set()
 for r in comparisons:
  k=r['configuration'],r['corpus'];need(k in priorcmp and k not in seen,'Duplicate frozen corpus');seen.add(k)
  need(all(r[n]==priorcmp[k][n] for n in ['original','inputs','recorder','cases']),'Altered frozen provenance')
  for n in ['original','inputs','recorder','current']:check(r[n])
  need(r['exact'] and raw(r['original']['path'])==raw(r['current']['path']) and r['current']['path'].startswith(PREFIX+'captures/packet-v1/'),'Stale/nonexact current behavior')
  totals[r['configuration'],'late' if r['corpus'] in list('ABCDEFG') else 'preedit']+=r['cases']
 need(seen==set(priorcmp) and dict(totals)=={(c,p):n for c in ['Debug','Release'] for p,n in [('preedit',28966),('late',458)]},'Compatibility population changed')
 provenance=load(PREFIX+'frozen-provenance-v1.json');need(provenance['new_original_queries']==0 and provenance['scientific_truth']=='NOT_GOLDEN','Unauthorized provenance');check(provenance['comparison'])
 if check_validation:
  from verify_c13_validation_v1 import inspect
  summary['validation']=inspect(load,raw,PREFIX)
 summary.update(compiled_retention=retention,original_queries=0,reader_sealed=bool(AUTH),qualification='Complete static source/CIL/managed proof for supported identities. Dynamic native arms not inferred executed. Four ByRef-dependent parents remain OPEN. W3/native/scientific acceptance unmet.')
 return summary

if __name__=='__main__':
 try:print(json.dumps(compute(check_validation='--static' not in sys.argv)))
 except Exception as e:print(json.dumps(dict(status='NOT_READY_04A38',error=type(e).__name__+': '+str(e))));raise SystemExit(1)
