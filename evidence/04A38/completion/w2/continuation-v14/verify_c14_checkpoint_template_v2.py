"""Independent C14 exact source/CIL/whole-body/fragment/ownership reader; never READY."""
import hashlib,json,posixpath,sys
from pathlib import Path
from collections import Counter
from source_views import body
from proof_kernel_v8 import bind,definitions,semantic,closure,map_current,need
from verify_c14_composition_v2 import inspect as composition
from verify_c14_fragments_v1 import inspect as fragment_check
ROOT=Path(__file__).resolve().parents[5] if 'evidence' in Path(__file__).parts else Path.cwd()
PREFIX='evidence/04A38/completion/w2/continuation-v14/';OLD=PREFIX.replace('v14/','v13/')
AUTH={}

def compute(root=ROOT,overrides=None,check_validation=True,check_predecessor=True):
 root=Path(root);overrides=overrides or {};cache={}
 def raw(path):
  path=posixpath.normpath(path)
  if path not in cache:cache[path]=overrides[path] if path in overrides else (root/path).read_bytes()
  return cache[path]
 load=lambda path:json.loads(raw(path))
 def check(r):need((len(raw(r['path'])),hashlib.sha256(raw(r['path'])).hexdigest())==(r['bytes'],r['sha256']),'Changed authority bytes '+r['path'])
 for path,digest in AUTH.items():need(hashlib.sha256(raw(path)).hexdigest()==digest,'Sealed C14 input changed '+path)
 ts=load(PREFIX+'source-transitions-final-v1.json')['files'];need(len(ts)==len({t['path'] for t in ts})==7,'Missing/duplicate source transition');projected=dict(overrides)
 for t in ts:
  check(t['after']);check(t['snapshot']);need(raw(t['path'])==raw(t['snapshot']['path']),'Final source/snapshot mismatch')
  if t['before']:check(t['preimage']);need((t['before']['bytes'],t['before']['sha256'])==(t['preimage']['bytes'],t['preimage']['sha256']),'Wrong immediate preimage');projected[t['path']]=raw(t['preimage']['path'])
  else:need(t['entry_state']=='ABSENT','New source lacks absent-entry proof')
 u=load(PREFIX+'current-universe-packet-v1.json');original=load('evidence/04A38/completion/w1/original-universe.json');old=load(OLD+'current-ownership-v2.json');oh=load(OLD+'historical-ownership-v2.json')['members'];mapping=map_current(old['members'],u['members']);members={n['symbol_id']:n for n in u['members']};om={n['symbol_id']:n for n in original['members']};hm={h['canonical_original_id']:h for h in oh};oldm={n['symbol_id']:n for n in old['members']};reverse={n['symbol_id']:k for k,n in mapping.items()}
 need(len(members)==len(mapping)==726 and len(oh)==704,'Native/historical universe changed')
 inherited={mapping[n['symbol_id']]['symbol_id'] for n in old['members'] if not n['review_status'].startswith('OPEN')};need(len(inherited)==494 and all(mapping[n['symbol_id']]['body_sha256']==n['body_sha256'] for n in old['members'] if not n['review_status'].startswith('OPEN')),'Inherited supported body changed')
 cs=load(PREFIX+'whole-body-contracts-v1.json')['contracts'];props=load(PREFIX+'property-body-contracts-v1.json')['contracts'];callbacks=load(PREFIX+'callback-proofs-v1.json')['rows'];pc=load(PREFIX+'portable-api-contracts-v1.json')['contracts'];diag=load(PREFIX+'cohort-diagnostic-v1.json');shapes={s['symbol_id']:s for s in load(PREFIX+'declaration-shapes-v1.json')['rows']}
 need(len(cs)==79 and len(props)==1 and len(callbacks)==14 and len({c['current_id'] for c in cs+props+callbacks})==94,'Whole dependency/property/callback cohort missing')
 snapshots={r['source']['path']:r['snapshot'] for r in load('evidence/04A38/preedit/entry-source-snapshot.json')['files']}
 for c in cs+props:
  n=members[c['current_id']];need(n==c['current_record'],'Current source identity forged');text,v=body(raw(n['path']),n);need(text==c['current_whole_body'] and v==c['current_source_view'],'Current whole body/view changed');check(c['current_source'])
  if c['original_id']:
   o=om[hm[c['original_id']]['git_original_id']];need(o==c['original_record'] and c['original_source']==snapshots[o['path']],'Original source/span authority changed');check(c['original_source']);text,v=body(raw(c['original_source']['path']),o);need(text==c['original_whole_body'] and v==c['original_source_view'],'Original whole body/normalization forged')
  need(c['new_support']==oldm[reverse[n['symbol_id']]]['review_status'].startswith('OPEN'),'Invented new/retained source classification')
 new={c['original_id']:c for c in cs+props if c['new_support']};newcurrent={c['current_id'] for c in new.values()};need(len(new)==len(newcurrent)==35 and None not in new,'Missing/ambiguous historical/current new closure')
 retention=[];contexts={}
 for cfg in ['Debug','Release']:
  meta=load(PREFIX+'metadata-all/packet-v1-context-v5/current-'+cfg+'.json');orig=load(PREFIX+'metadata-all/packet-v1-context-v5/original-'+cfg+'.json');prior=load(OLD+'metadata-all/packet-v1-context-v5/current-'+cfg+'.json');need(raw(PREFIX+'metadata-all/packet-v1-context-v5/original-'+cfg+'.json')==raw(OLD+'metadata-all/packet-v1-context-v5/original-'+cfg+'.json') and orig['original_semantic_calls']==0,'Altered original compiled authority/activity');defs=definitions(meta);od=definitions(orig);bd=definitions(prior);ret=load(PREFIX+'return-metadata-'+cfg+'-v1.json')['methods'];roots=[]
  for c in cs:
   m=bind(c['current_record'],meta,shapes);r=next(r for r in c['compiled'] if r['configuration']==cfg);need(m==r['complete_method'] and m['method']==r['method'] and r['source_parameters']==shapes[c['current_id']] and r['transitive']==closure(meta,[m['method']]),'Actual whole parent/signature/dependency/CIL mismatch');need(r['return_declaration'] in ret,'Current return ABI missing');roots.append(m['method'])
   if c['original_id']:need(r['original_method']==od[r['original_method']['method']] and r['original_return'] in load('evidence/04A38/preedit/compiled-'+cfg+'.json')['methods'] and r['original_return']['return_type']==r['return_declaration']['return_type'],'Original return/CIL authority missing')
  for c in props:
   r=next(r for r in c['compiled'] if r['configuration']==cfg);owner='BESHStatNG.WorksheetFunctions.PropensityScoreUDFs+PsmUdfHandle';ms=[m for m in meta['methods'] if m['declaring_type']==owner and m['member_name'] in ['get_Alpha','set_Alpha','.ctor']];need(r['current_methods']==ms and len(ms)==3 and all(semantic(m)==semantic(od[m['method']]) for m in ms),'Alpha real getter/setter/default constructor mismatch');need(all(m in ret for m in r['actual_returns']),'Alpha current return reflection missing');roots.extend(m['method'] for m in ms)
  graph=closure(meta,roots);need(roots==diag['roots'][cfg] and graph==diag['graphs'][cfg] and len(graph['methods'])==1581,'Missing actual complete executable dependency');targets={}
  expected_children=set();pending=[c['current_id'] for c in cs]
  while pending:
   p=pending.pop()
   for n in u['members']:
    if n['parent_member']==p and n['symbol_id'] not in expected_children:expected_children.add(n['symbol_id']);pending.append(n['symbol_id'])
  need(expected_children=={c['current_id'] for c in callbacks},'Actual child callback omitted')
  for c in callbacks:
   n=members[c['current_id']];parent=members[n['parent_member']];key=bind(parent,meta,shapes)['method'] if parent['parent_member'] is None else targets[parent['symbol_id']];siblings=sorted([n for n in u['members'] if n['parent_member']==parent['symbol_id']],key=lambda n:n['span_start']);actual=[i['operand'][7:] for i in defs[key]['instructions'] if i['op']=='ldftn' and i['operand'][7:] in defs and defs[i['operand'][7:]]['member_name'].startswith('_Lambda$__')];need(len(actual)==len(siblings),'Callback source/actual ldftn count mismatch');target=actual[siblings.index(n)];r=next(r for r in c['configurations'] if r['configuration']==cfg);need(r['parent_method']==key and r['target']==target and r['complete_method']==defs[target] and semantic(defs[target])==semantic(bd[target]) and r['return_declaration'] in ret,'Callback target/context/return changed');need(r['declaring_type']==next(t for t in meta['types'] if t['type']==defs[target]['declaring_type']) and r['constructors']==[m for m in meta['methods'] if m['declaring_type']==defs[target]['declaring_type'] and m['member_name'] in ['.ctor','.cctor']],'Callback declaring fields/constructors missing');text,v=body(raw(n['path']),n);need(text==c['whole_body'] and v==c['source_view'] and n['body_sha256']==oldm[reverse[n['symbol_id']]]['body_sha256'],'Retained callback body changed');targets[n['symbol_id']]=target
  for p in pc:
   n=p['source_record'];text,v=body(raw(n['path']),n);need(text==p['whole_body'] and v==p['source_view'],'Portable whole body/span mismatch');r=next(r for r in p['compiled'] if r['configuration']==cfg);need(bind(n,meta,shapes)==r['method'] and not r['context']['fields'],'Portable actual compiled implementation/state missing');need(n['name']=='.ctor' or r['return_declaration'] in ret,'Portable return boundary missing')
  enum=load(PREFIX+'portable-api-contracts-v1.json')['enum_declaration'];data=raw(enum['path']).decode('utf-8-sig');part=data.encode('utf-16-le')[enum['span_start']*2:(enum['span_start']+enum['span_length'])*2].decode('utf-16-le');need(part==enum['complete_declaration'],'Portable enum source declaration changed');names=[s.strip() for s in part.splitlines()[1:-1] if s.strip()];t=next(t for t in meta['types'] if t['type']=='BESHStatNG.ExcelCommon.PsmUdfTableKind38');need({f['name']:f['literal'] for f in t['fields'] if f['is_literal']}=={n:i for i,n in enumerate(names)},'Wrong actual enum literal/order values')
  delta=next(r for r in load(PREFIX+'compiled-method-deltas-v1.json')['configurations'] if r['configuration']==cfg);changed={k for k in bd if k in defs and semantic(bd[k])!=semantic(defs[k])};added=set(defs)-set(bd);need(not set(bd)-set(defs) and changed=={r['method'] for r in delta['changed']} and len(changed)==5 and len(added)==8 and {r['method'] for r in delta['added']}==added,'Unapproved CIL transition');need(all(r['original']==bd[r['method']] and r['current']==defs[r['method']] for r in delta['changed']),'Compiled delta bytes forged')
  ownerproof=next(r for r in load(PREFIX+'constructed-owner-proofs-v1.json')['configurations'] if r['configuration']==cfg);need({r['owner'] for r in ownerproof['contexts']}=={defs[k]['declaring_type'] for k in graph['methods']},'Constructed owner omitted')
  for r in ownerproof['contexts']:need(r['type']==next(t for t in meta['types'] if t['type']==r['owner']) and r['constructors_and_initializers']==[m for m in meta['methods'] if m['declaring_type']==r['owner'] and m['member_name'] in ['.ctor','.cctor']],'Constructed state/initializer proof forged')
  ph=next(r for r in load(OLD+'portable-historical-callback-proof-v1.json')['compiled'] if r['configuration']==cfg);need(semantic(defs[ph['current_callback']['method']])==semantic(ph['current_callback']),'Inherited portable historical callback changed')
  exports=load(PREFIX+'inspection/packet-v1/Inspect38-'+cfg+'.json')['exports'];need(len(exports)==268 and sorted(exports,key=lambda r:r['effective_excel_name'])==sorted(load('evidence/04A38/preedit/compiled-'+cfg+'.json')['exports'],key=lambda r:r['effective_excel_name']),'Public268 export/attribute/signature/default/help ABI changed');contexts[cfg]=meta;retention.append(dict(configuration=cfg,reached=1581,exports=268,changed_native=5,added_portable_API=7,added_private_ctor=1))
 need(composition(root,overrides,load)==load(PREFIX+'source-composition-proof-v1.json'),'Whole-source behavioral composite changed');fragment_result=fragment_check(load,PREFIX)
 history=load(PREFIX+'historical-ownership-v2.json')['members'];cur=load(PREFIX+'current-ownership-v2.json');closed=inherited|newcurrent;need(len(history)==704 and [h['canonical_original_id'] for h in history]==[h['canonical_original_id'] for h in oh],'Historical universe/order changed')
 for h,b in zip(history,oh):
  need(bool(h['proof'])==(bool(b['proof']) or h['canonical_original_id'] in new),'Unsupported historical closure')
  if h['canonical_original_id'] not in new:need(h['proof']==b['proof'] and h['current_ids']==[mapping[k]['symbol_id'] for k in b['current_ids']],'Retained historical proof changed')
  else:need(h['current_ids']==[new[h['canonical_original_id']]['current_id']],'Wrong historical/current identity counterpart')
 need(Counter(n['symbol_id'] for n in cur['members'])==Counter(members.keys()) and all((not n['review_status'].startswith('OPEN'))==(n['symbol_id'] in closed) for n in cur['members']),'Current OPEN universe/complement changed')
 ss=cur['support'];before={(s['path'],s['kind'],s['owner'],s['declaration']):s for s in old['support']};need(Counter(s['support_id'] for s in ss)==Counter(s['support_id'] for s in u['supports']),'Support declaration universe changed')
 for s in ss:
  key=s['path'],s['kind'],s['owner'],s['declaration'];expected=before[key]['fully_reconciled'] or s['declaration'] in ['Private Class PsmUdfHandle','Public Module PropensityScoreUDFs','Public Module PlotDataUDFs'];need(s['fully_reconciled']==expected,'Unsupported support closure')
  if s['fully_reconciled'] and not before[key]['fully_reconciled'] and s['kind']=='ModuleBlock':
   owner=s['owner']+'.'+s['declaration'].split()[-1];need(all(n['symbol_id'] in closed for n in u['members'] if n['path']==s['path'] and (n['owner']==owner or n['owner'].startswith(owner+'.'))),'Module has unclosed member')
 hc={h['git_original_id'] for h in history if h['proof']};headers=load(PREFIX+'support-fragment-crosswalk-v1.json');keys={(s['path'],s['kind'],s['owner'],s['declaration']) for s in ss if s['fully_reconciled']};classes={(s['owner'],s['declaration']) for s in ss if s['kind']=='ClassBlock' and s['owner'] in ['BESHStatNG.WorksheetFunctions.MultivariateAnalysisUDFs','BESHStatNG.WorksheetFunctions.PropensityScoreUDFs'] and s['fully_reconciled']};owners={owner+'.'+decl.split()[-1] for owner,decl in classes}
 def make_headers(universe):
  found={}
  for s in universe['supports']:
   if (s['path'],s['kind'],s['owner'],s['declaration']) not in keys:continue
   if s['kind']=='ModuleBlock':kinds=['ModuleStatement','EndModuleStatement']
   elif s['kind']=='ClassBlock' and (s['owner'],s['declaration']) in classes:kinds=['ClassStatement','EndClassStatement']
   elif s['kind']=='PropertyStatement' and s['owner'] in owners:kinds=['PropertyStatement']
   else:continue
   for f in universe['fragments']:
    if f['member_id'] is None and f['path']==s['path'] and f['kind'] in kinds and s['span_start']<=f['span_start'] and f['span_start']+f['span_length']<=s['span_start']+s['span_length']:found[f['fragment_id']]=s['support_id']
  return found
 need(headers==dict(current=make_headers(u),original=make_headers(original)),'Support/header fragment coverage forged');counts={}
 for pop,universe,rows in [('current',u,cur['fragments']),('original',original,load(PREFIX+'original-fragment-parent-crosswalk-v2.json')['rows'])]:
  fm={f['fragment_id']:f for f in universe['fragments']};need(Counter(f['fragment_id'] for f in rows)==Counter(fm.keys()),'Missing/duplicate fragment')
  pred=lambda f:f['member_id'] in (closed if pop=='current' else hc) or f['fragment_id'] in headers[pop]
  for f in rows:need(all(f[k]==v for k,v in fm[f['fragment_id']].items()) and ((f['review_status']!='OPEN') if pop=='current' else f['covered'])==pred(f),'Wrong actual fragment OPEN/covered status')
  counts[pop]=dict(total=len(rows),covered=sum(pred(f) for f in rows),branches=sum(f['is_branch'] for f in rows),branches_covered=sum(f['is_branch'] and pred(f) for f in rows),inline=sum(f['is_inline'] for f in rows),inline_covered=sum(f['is_inline'] and pred(f) for f in rows))
 summary=load(PREFIX+'ownership-summary-v2.json');need(summary['historical']==dict(total=704,closed=496,OPEN=208) and summary['current']==dict(total=726,closed=529,OPEN=197) and summary['support']['distinct_closed']==256 and summary['fragments']==counts,'Census/fragment denominator forged')
 queue=load(PREFIX+'remaining-work-queue-v1.json');remaining=[r for r in load(OLD+'remaining-work-queue-v1.json')['members'] if r['original_id'] not in new];need(queue['members']==remaining and queue['first_members']==remaining[:24] and Counter(r['current_id'] for r in queue['current_OPEN'])==Counter(set(members)-closed),'Remaining queue/order/complement forged')
 ready=load(PREFIX+'readiness-v1.json');need((ready['status'],ready['W2'],ready['W3'],ready['P'],ready['N1'],ready['N2'],ready['C2'],ready['C3'],ready['science'],ready['phase04A39'])==('NOT_READY_04A38','IN_PROGRESS','NOT_STARTED','NO','NO GO','NO','UNMET','UNAPPROVED','NOT_GOLDEN','BLOCKED'),'False readiness/stage')
 comparisons=load(PREFIX+'compatibility-packet-v1.json')['results'];past={(r['configuration'],r['corpus']):r for r in load(OLD+'compatibility-packet-v1.json')['results']};seen=set();totals=Counter()
 for r in comparisons:
  key=r['configuration'],r['corpus'];need(key in past and key not in seen,'Unknown/duplicate frozen corpus');seen.add(key);need(all(r[k]==past[key][k] for k in ['original','inputs','recorder','cases']),'Changed original/reference provenance')
  for k in ['original','inputs','recorder','current']:check(r[k])
  need(r['exact'] and raw(r['original']['path'])==raw(r['current']['path']) and r['current']['path'].startswith(PREFIX+'captures/packet-v1/'),'Stale or nonexact current behavior');totals[r['configuration'],'late' if r['corpus'] in list('ABCDEFG') else 'preedit']+=r['cases']
 need(seen==set(past) and dict(totals)=={(c,p):n for c in ['Debug','Release'] for p,n in [('preedit',28966),('late',458)]},'Compatibility population/provenance changed')
 if check_validation:
  from verify_c14_validation_v1 import inspect as validate
  summary['validation']=validate(load,raw,PREFIX)
 if check_predecessor:
  saved=list(sys.path);sys.path.insert(0,str(root/OLD))
  try:
   from verify_c13_checkpoint_v2 import compute as prior_compute
   previous=prior_compute(root,projected)
  finally:sys.path[:]=saved
  need(previous['historical']==dict(total=704,closed=461,OPEN=243) and previous['current']==dict(total=726,closed=494,OPEN=232),'Prior sealed source/ownership drift')
 summary.update(compiled_retention=retention,fragments_new=fragment_result,original_queries=0,reader_sealed=bool(AUTH),qualification='Static complete body/CIL and current-only managed diagnostics; native/scientific/dynamic execution not inferred. Incomplete W2, no W3 or04A39 authority.')
 return summary

if __name__=='__main__':
 try:print(json.dumps(compute(check_validation='--static' not in sys.argv)))
 except Exception as e:print(json.dumps(dict(status='NOT_READY_04A38',error=type(e).__name__+': '+str(e))));raise SystemExit(1)
