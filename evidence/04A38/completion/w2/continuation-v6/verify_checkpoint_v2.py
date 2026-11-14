"""Independent fail-closed C6 source/CIL/ownership reader. Partial W2, never READY."""
import hashlib,json,re,sys
from collections import Counter,defaultdict
from pathlib import Path
from source_views import body as source_body
ROOT=Path(__file__).resolve().parents[5];HERE=Path(__file__).resolve().parent
PREFIX=HERE.relative_to(ROOT).as_posix()+'/'
V5='evidence/04A38/completion/w2/continuation-v5/'
W1='evidence/04A38/completion/w1/'
AUTH={'whole-body-contracts-v1.json': 'e947d923890422a5f53be79708954175cc09e33ed2184c88cadf6a9e194951af', 'callback-proofs-v1.json': 'ca5d2378c783905390488221bdff6a9427458791d5ba429d29e2bf55ac7b9112', 'current-universe-families-v2.json': '9c3eaef09b4ed633e8ab109a3a67e8772fffde2c21e7497b1085a56781897ae2', 'source-transitions-final-v1.json': '7bb1db43a7e37ec752f92d7fd6b25284e2b5950c5fff4e910c1e2a63984995c6', 'compiled-context-and-consumers.json': 'b3b3ce80eb4cf7d0373c5887828d95ece236a89498c4057e8759b70101fae9ba', 'full-type-field-context.json': '26e8b1110d9cb6d0d18ac73ecd7a4c2317662c2e7c37e46f770b125986901851'}
ALIASES={'BESHStatNG.ExcelCommon.UdfNumericImportPreparation38+_Closure$__8-0':'BESHStatNG.ExcelCommon.UdfNumericImportPreparation38+_Closure$__9-0'}
MODULES={'ExcelArgPredicates','ExcelArgReaders','ExcelArgNumeric','ParametricUDFs','NonparametricUDFs'}
def need(value,message):
 if not value:raise ValueError(message)
def sha(data):return hashlib.sha256(data).hexdigest()
def semantic(row):return {k:v for k,v in row.items() if k!='raw_IL_sha256'}
def normalize(row):
 text=json.dumps(row,sort_keys=True,ensure_ascii=False)
 for before,after in ALIASES.items():text=text.replace(before,after)
 return json.loads(text)
def tokens(text):return [x for x in re.findall(r'"(?:""|[^"])*"|\x27[^\n]*|[A-Za-z_][A-Za-z_0-9]*|\d+(?:\.\d+)?|[^\s]',text) if not x.startswith("'")]
def complete_map(rows):
 result={r['method']:r for r in rows};need(len(result)==len(rows),'Duplicate complete CLR method identity');return result
def expected_edges():
 n='BESHStatNG.UdfDataImport';p='BESHStatNG.WorksheetFunctions.ParametricUDFs';r='BESHStatNG.WorksheetFunctions.NonparametricUDFs'
 num='BESHStatNG.ExcelCommon.UdfNumericImportPreparation38';inf='BESHStatNG.ExcelCommon.UdfInferencePreparation38';core='BESHStatNG.WorksheetCompatibility.UdfNumericPreparation38'
 result={}
 for name,target in [('TryGetOptionalProbabilityThresholds','TryReadProbabilityThresholdsPrepared'),('TryGetSingleNumericColumn','ReadFiniteColumn'),('TryReadIndependentNumericColumns','TryReadIndependentNumericColumnsPrepared'),('ExtractCompleteNumericMatrixCompleteCases','ExtractCompleteNumericMatrixCompleteCasesPrepared'),('TryReadCompleteNumericMatrixWithHeaders','TryReadCompleteNumericMatrixWithHeadersPrepared'),('ExtractNumericColumnIgnoringNonNumeric','ReadFiniteColumn')]:result[n,name]=num,target
 for name in ['TryReadPairedNumericColumns','ExtractPairedNumericColumnsIgnoringNonNumeric']:result[n,name]=core,'RowsToMatrix'
 for name,target in [('RMANOVA1','ApplyRepeatedCorrection'),('ANOVA2_NESTED','NestedTableSelection'),('RMANOVA1_MCP','BuildRepeatedMcpTables'),('TTEST_UNPAIRED','IndependentTTableSelection'),('TTEST_UNPAIRED_NI','MeanNonInferiorityBody'),('TTEST_UNPAIRED_EQUIV','MeanEquivalenceBody')]:result[p,name]=inf,target
 for name in ['SPEARMAN_RHO','SPEARMAN_P','KENDALL_TAU','KENDALL_P']:result[r,name]=inf,'SplitCorrelationColumns'
 for name in ['KW_STAT','KW_P']:result[r,name]=inf,'KruskalGroupNames'
 for name in ['FRIEDMAN_STAT','FRIEDMAN_P']:result[r,name]=inf,'FriedmanGroupNames'
 result[r,'KW_MCP']=inf,'BuildKruskalMcpTables';result[r,'FRIEDMAN_MCP']=inf,'BuildFriedmanMcpTables'
 return result
def verify_edges(current):
 methods=complete_map(current['methods']);rows=[]
 for (owner,name),(target_owner,target_name) in expected_edges().items():
  callers=[r for r in methods.values() if r['declaring_type']==owner and r['member_name']==name]
  targets=[r for r in methods.values() if r['declaring_type']==target_owner and r['member_name']==target_name]
  need(len(callers)==len(targets)==1,'Compiled producer/consumer missing')
  target='METHOD:'+targets[0]['method']
  need(sum(i['op'] in ['call','callvirt'] and i['operand']==target for i in callers[0]['instructions'])==1,'Broken actual portable dependency edge')
  rows.append(dict(caller=callers[0]['method'],target=targets[0]['method']))
 return rows
def verify_context(old,current,original):
 current_methods=complete_map(current['methods']);types={t['type']:t for t in current['types']}
 allowed=set(expected_edges())|{('BESHStatNG.ExcelCommon.UdfNumericImportPreparation38',name) for name in ['ExtractCompleteNumericMatrixCompleteCases','TryReadCompleteNumericMatrixWithHeaders']}
 changed=set();retained=0
 for before in old['methods']:
  row=normalize(before);need(row['method'] in current_methods,'Lost retained method/closure')
  after=current_methods[row['method']]
  if semantic(row)==semantic(after):retained+=1
  else:
   pair=row['declaring_type'],row['member_name'];need(pair in allowed,'Unreviewed complete CIL change');changed.add(pair)
 need(changed==allowed,'Authorized CIL transition set differs')
 for before in old['types']:
  row=normalize(before);need(types.get(row['type'])==row,'Changed retained type/field/initializer context')
 counts={}
 for label,test in [('engine',lambda s:s.startswith(('BESHStatNG.parametric.','BESHStatNG.nonparametric.','BESHStatNG.equivalencetests.','BESHStatNG.Matrix.')) or s=='BESHStatNG.ExcelDnaSharedApplicationAdapter'),('typed_Common',lambda s:s.startswith('BESHStatNG.ExcelCommon.') and '38' not in s)]:
  a={r['method']:semantic(r) for r in original['methods'] if test(r['declaring_type'])}
  b={r['method']:semantic(r) for r in current['methods'] if test(r['declaring_type'])}
  need(a==b,'Changed original complete '+label+' methods')
  at={r['type']:r for r in original['types'] if test(r['type'])};bt={r['type']:r for r in current['types'] if test(r['type'])}
  need(at==bt,'Changed original complete '+label+' fields/types')
  counts[label]=dict(methods=len(a),types=len(at))
 return dict(retained_v5_methods=retained,authorized_changes=len(changed),contexts=counts,edges=verify_edges(current))
def declaration_headers(universe,supports):
 rows={}
 for s in supports:
  if s['kind']!='ModuleBlock' or s['declaration'].split()[-1] not in MODULES:continue
  for f in universe['fragments']:
   if f['member_id'] is None and f['path']==s['path'] and f['kind'] in ['ModuleStatement','EndModuleStatement'] and f['span_start']>=s['span_start'] and f['span_start']+f['span_length']<=s['span_start']+s['span_length']:
    rows[f['fragment_id']]=s['support_id']
 return rows
def compute(root=ROOT,overrides=None,check_ledgers=True):
 root=Path(root);overrides=overrides or {};cache={}
 def raw(path):
  if path in overrides:return overrides[path]
  return (root/path).read_bytes()
 def load(path):
  if path not in cache:cache[path]=json.loads(raw(path))
  return cache[path]
 def check_pin(row):
  data=raw(row['path']);need((len(data),sha(data))==(row['bytes'],row['sha256']),'Source/evidence pin mismatch '+row['path'])
 for path,digest in AUTH.items():need(sha(raw(PREFIX+path))==digest,'Authenticated proof/census changed '+path)
 transitions=load(PREFIX+'source-transitions-final-v1.json')['files']
 projected=dict(overrides)
 for t in transitions:
  check_pin(t['after']);check_pin(t['snapshot']);need(raw(t['after']['path'])==raw(t['snapshot']['path']),'Successor snapshot differs')
  if t['before'] is not None:
   check_pin(t['preimage']);need(t['preimage']['sha256']==t['before']['sha256'] and t['preimage']['bytes']==t['before']['bytes'],'Preimage differs from accepted entry bytes')
   projected[t['path']]=raw(t['preimage']['path'])
 need(len(transitions)==9,'Source transition population changed')
 # Historical reader executes independently against exact authenticated entry bytes.
 sys.path.insert(0,str(root/V5))
 from verify_ownership_final import inspect as v5_ownership
 old_proof=v5_ownership(root,projected)
 need(old_proof['historical']==dict(total=704,closed=199,OPEN=505),'Entry proof population changed')
 current=load(PREFIX+'current-universe-families-v2.json');original=load(W1+'original-universe.json')
 old_history=load(V5+'historical-ownership-v3.json')['members'];old_current=load(V5+'current-ownership-v3.json')
 for source in current['sources']:
  # Census source rows are exact raw worktree files, including BOM/CRLF.
  path=source.get('path',source.get('source'))
  if isinstance(path,dict):path=path['path']
  data=raw(path);need(sha(data)==source['sha256'],'Live current census source differs '+path)
 dossier=load(PREFIX+'whole-body-contracts-v1.json');contracts=dossier['contracts']
 need(len(contracts)==91 and len({c['current_id'] for c in contracts})==91,'Incomplete/duplicate reviewed contracts')
 histories={r['canonical_original_id']:r for r in old_history};members={r['symbol_id']:r for r in current['members']}
 original_members={r['symbol_id']:r for r in original['members']}
 snapshots={r['source']['path']:r['snapshot'] for r in load('evidence/04A38/preedit/entry-source-snapshot.json')['files']}
 newly_supported=set();reviewed=set()
 for c in contracts:
  need(c['current_id'] in members and c['current_record']==members[c['current_id']],'Missing source dependency/current identity')
  n=members[c['current_id']];check_pin(c['current_source'])
  text,policy=source_body(raw(n['path']),n);need(text==c['current_whole_body'] and policy==c['current_source_view'],'Whole current body/source view mismatch')
  need(c['boundary']!='HOST_ONLY' and len(c['review'])>100,'Unsupported host-only/body disposition')
  if c['original_id'] is not None:
   h=histories[c['original_id']];o=original_members[h['git_original_id']]
   need(c['original_record']==o and c['original_source']==snapshots[h['path']],'Original identity/source mismatch')
   check_pin(c['original_source']);old_text,old_policy=source_body(raw(c['original_source']['path']),o)
   need(old_text==c['original_whole_body'] and old_policy==c['original_source_view'],'Whole original body/source view mismatch')
   need(c['new_support']==(not bool(h['proof'])),'Forged historical closure origin')
   if not h['proof']:newly_supported.add(h['canonical_original_id'])
  else:need(n['name']=='MaterializePortableReport38' and c['new_support'],'Invented historical/new-current identity')
  for key,universe,parent in [('current_fragments',current,n['symbol_id']),('original_fragments',original,h['git_original_id'] if c['original_id'] else None)]:
   expected=[dict(id=f['fragment_id'],branch=f['is_branch'],inline=f['is_inline'],token_sha256=f['token_sha256']) for f in universe['fragments'] if parent is not None and f['member_id']==parent]
   need(c[key]==expected,'Missing branch/statement/short-circuit/inline proof')
  reviewed.add(n['symbol_id'])
 need(len(newly_supported)==49,'New whole-body proof population changed')
 contexts={};metadata={}
 for config in ['Debug','Release']:
  old=load(V5+'dependency-metadata-v9/current-'+config+'.json')
  now=load(PREFIX+'metadata-full/families-v2/current-'+config+'.json');before=load(PREFIX+'metadata-full/families-v2/original-'+config+'.json')
  need(now['metadata_only'] and before['metadata_only'] and before['original_semantic_calls']==0,'Original metadata scope exceeded')
  contexts[config]=verify_context(old,now,before);metadata[config]=now
  for label,record in [('current',now),('original',before)]:
   library=root/'.test-rationalization-04A38.tmp'/('completion-w2-cont6/families-v2/'+config+'/lib' if label=='current' else 'consolidated38-v7/original-build/'+config)
   for a in record['assemblies']:
    path=library/a['file_name']
    if path.is_file():need((path.stat().st_size,sha(path.read_bytes()))==(a['bytes'],a['sha256']),'Actual compiled DLL identity changed')
  actual=load(PREFIX+'inspection/families-v2/Inspect38-'+config+'.json')['exports'];frozen=load('evidence/04A38/preedit/compiled-'+config+'.json')['exports']
  need(len(actual)==268 and sorted(actual,key=lambda r:r['effective_excel_name'])==sorted(frozen,key=lambda r:r['effective_excel_name']),'Altered export signature/default/attribute')
 # Every new/current anonymous callback has its own whole source and actual ldftn binding.
 callbacks=load(PREFIX+'callback-proofs-v1.json')['rows'];callback_ids=set()
 for row in callbacks:
  n=members[row['current_id']];need(row['current_record']==n and n['parent_member']==row['parent_id'] and row['parent_id'] in reviewed,'Orphaned/unreviewed callback')
  check_pin(row['source']);text,policy=source_body(raw(n['path']),n);need((text,policy)==(row['whole_body'],row['source_view']),'Callback body/source mismatch')
  parent=next(c for c in contracts if c['current_id']==row['parent_id'])
  siblings=sorted([m for m in current['members'] if m['parent_member']==row['parent_id']],key=lambda r:r['span_start'])
  for config in ['Debug','Release']:
   methods=complete_map(metadata[config]['methods']);binding=next(r for r in row['configurations'] if r['configuration']==config)
   pc=next(r for r in parent['compiled'] if r['configuration']==config)
   need(binding['parent_method'] in pc['current_methods'],'Wrong compiled parent')
   targets=[i['operand'][7:] for i in methods[binding['parent_method']]['instructions'] if i['op']=='ldftn' and methods[i['operand'][7:]]['member_name'].startswith('_Lambda$__')]
   need(len(targets)==len(siblings) and targets[siblings.index(n)]==binding['target'],'Broken actual callback edge/source order')
   need(binding['complete_method']==methods[binding['target']],'Incomplete callback CIL/exception proof')
   need(binding['declaring_type']==next(t for t in metadata[config]['types'] if t['type']==methods[binding['target']]['declaring_type']),'Callback captured-field context differs')
  callback_ids.add(n['symbol_id'])
 need(callback_ids=={n['symbol_id'] for n in current['members'] if n['parent_member'] in reviewed},'Callback source omitted')
 bykey=defaultdict(list)
 for n in current['members']:bykey[n['signature_key']].append(n)
 closed=set(reviewed)|callback_ids
 expected_history=[]
 for h in old_history:
  keys={x.split('@')[0] for x in h['current_ids']}|{h['git_original_id'].split('@')[0]}
  mapped=sorted({n['symbol_id'] for key in keys for n in bykey.get(key,[])})
  supported=bool(h['proof']) or h['canonical_original_id'] in newly_supported
  if supported:closed.update(mapped)
  expected_history.append(dict(id=h['canonical_original_id'],current_ids=mapped,supported=supported))
 for n in old_current['members']:
  if n['review_status'].startswith('OPEN'):continue
  candidates=[r for r in bykey.get(n['signature_key'],[]) if r['body_token_sha256']==n['body_token_sha256'] and (r['parent_member'] or '').split('@')[0]==(n['parent_member'] or '').split('@')[0]]
  need(len(candidates)==1,'Lost or unreviewed changed inherited current body');closed.add(candidates[0]['symbol_id'])
 # Every exact native source call candidate is supported; complete CLR target is retained.
 for c in contracts:
  for edge in c['compiled_dependency_edges']:
   defs=complete_map(metadata[edge['configuration']]['methods'])
   need(edge['caller'] in defs and any(i['op']==edge['op'] and i['operand']=='METHOD:'+edge['target'] for i in defs[edge['caller']]['instructions']),'Forged compiled dependency')
   need(set(edge['current_source_candidates'])<=closed,'Unresolved source dependency in claimed parent')
 # Match all325 raw/303 distinct support declarations; close only complete stateless modules.
 old_support={(s['path'],s['kind'],s['owner'],s['declaration']):s for s in old_current['support']}
 support=[];resolved_modules=[]
 for s in current['supports']:
  prior=old_support[s['path'],s['kind'],s['owner'],s['declaration']]
  status=prior['fully_reconciled'];children=[]
  if s['kind'] in ['ModuleBlock','ClassBlock']:
   children=[n['symbol_id'] for n in current['members'] if n['path']==s['path'] and n['span_start']>=s['span_start'] and n['span_start']+n['span_length']<=s['span_start']+s['span_length']]
   status=False
   if s['kind']=='ModuleBlock' and s['declaration'].split()[-1] in MODULES:
    need(set(children)<=closed and children,'Container children unresolved')
    owners={members[i]['owner'] for i in children};need(len(owners)==1,'Ambiguous module CLR mapping')
    for config in ['Debug','Release']:
     t=next(t for t in metadata[config]['types'] if t['type']==next(iter(owners)))
     need(t['fields']==[],'Unreviewed mutable module state')
    status=True;resolved_modules.append(s['support_id'])
  support.append(dict(id=s['support_id'],supported=status,children=children,distinct=not s['already_in_primary_population']))
 headers=declaration_headers(current,current['supports'])
 old_headers=declaration_headers(original,original['supports'])
 proof_orig={r['git_original_id'] for r in old_history if r['proof'] or r['canonical_original_id'] in newly_supported}
 def counts(fragments,covered):return dict(total=len(fragments),branches=sum(f['is_branch'] for f in fragments),inline=sum(f['is_inline'] for f in fragments),covered=sum(covered(f) for f in fragments),branches_covered=sum(f['is_branch'] and covered(f) for f in fragments),inline_covered=sum(f['is_inline'] and covered(f) for f in fragments))
 fragments=dict(current=counts(current['fragments'],lambda f:f['member_id'] in closed or f['fragment_id'] in headers),original=counts(original['fragments'],lambda f:f['member_id'] in proof_orig or f['fragment_id'] in old_headers))
 hs=dict(total=len(old_history),closed=sum(r['supported'] for r in expected_history),OPEN=sum(not r['supported'] for r in expected_history))
 cs=dict(total=len(current['members']),closed=len(closed),OPEN=len(current['members'])-len(closed))
 ss=dict(raw=len(support),distinct=sum(s['distinct'] for s in support),distinct_closed=sum(s['distinct'] and s['supported'] for s in support),distinct_OPEN=sum(s['distinct'] and not s['supported'] for s in support))
 need(hs['total']==704 and ss['distinct']==303 and hs['OPEN']>0,'Unsupported READY or inventory population change')
 comparisons=load(PREFIX+'compatibility-families-v2.json')['results'];totals=Counter()
 for r in comparisons:
  for key in ['original','current','inputs','recorder']:check_pin(r[key])
  need(raw(r['original']['path'])==raw(r['current']['path']),'Frozen compatibility mismatch')
  need(sum(bool(line.strip()) for line in raw(r['inputs']['path']).splitlines())==r['cases'],'Frozen input case population differs')
  totals[r['configuration'],'late' if r['corpus'] in list('ABCDEFG') else 'preedit']+=r['cases']
 need(len(comparisons)==24 and dict(totals)=={(c,p):n for c in ['Debug','Release'] for p,n in [('preedit',28966),('late',458)]},'Corpus populations/provenance changed')
 result=dict(status='INDEPENDENT_C6_PARTIAL_OWNERSHIP_SOURCE_CIL_COMPATIBILITY_PASS_NOT_READY',historical=hs,current=cs,support=ss,fragments=fragments,
   newly_supported_historical=sorted(newly_supported),reviewed_current=sorted(reviewed),closed_current=sorted(closed),callbacks=sorted(callback_ids),support_dispositions=support,
   header_fragments=headers,original_header_fragments=old_headers,historical_dispositions=expected_history,compiled_context=contexts,
   original_queries=0,W2='IN_PROGRESS',W3='NOT_STARTED')
 if check_ledgers:
  h=load(PREFIX+'historical-ownership-v1.json');c=load(PREFIX+'current-ownership-v1.json');summary=load(PREFIX+'ownership-summary-v1.json')
  need(len(h['members'])==704 and [r['canonical_original_id'] for r in h['members']]==[r['id'] for r in expected_history],'Historical identities omitted/reordered')
  for r,e in zip(h['members'],expected_history):
   need(bool(r['proof'])==e['supported'] and r['current_ids']==e['current_ids'],'OPEN or unsupported historical body disposition')
   if r['proof']:need(r['disposition']=='PROOF_SUPPORTED_REVIEWED_BODY' and r['proof'].get('kind')!='HOST_ONLY','Unsupported host-only classification')
  need(Counter(n['symbol_id'] for n in c['members'])==Counter(members.keys()),'Current population omitted/duplicated')
  need({n['symbol_id'] for n in c['members'] if not n['review_status'].startswith('OPEN')}==closed,'OPEN or unsupported current body disposition')
  need(Counter(s['support_id'] for s in c['support'])==Counter(s['support_id'] for s in current['supports']),'Support inventory omitted/duplicated')
  need({s['support_id'] for s in c['support'] if s['fully_reconciled']}=={s['id'] for s in support if s['supported']},'Unsupported support declaration closure')
  actual_frags={f['fragment_id']:f for f in c['fragments']}
  need(set(actual_frags)=={f['fragment_id'] for f in current['fragments']},'Orphaned/missing fragment')
  for f in current['fragments']:need((actual_frags[f['fragment_id']]['review_status']=='COVERED_BY_REVIEWED_BODY_OR_DECLARATION')==(f['member_id'] in closed or f['fragment_id'] in headers),'Missing branch/statement/inline accounting')
  need(summary['historical']==hs and summary['current']==cs and summary['support']==ss and summary['fragments']==fragments,'Forged ownership/fragment totals')
  need(summary['status']=='NOT_READY_04A38' and summary['W3']=='NOT_STARTED','False READY/W3 promotion')
  queue=load(PREFIX+'remaining-work-queue-v1.json')
  need(Counter(r['original_id'] for r in queue['members'])==Counter(r['id'] for r in expected_history if not r['supported']),'Remaining canonical historical queue incomplete')
  need(Counter(r['current_id'] for r in queue['current_OPEN'])==Counter(set(members)-closed),'Remaining current queue incomplete')
 return result
if __name__=='__main__':
 try:
  result=compute();print(json.dumps({k:result[k] for k in ['status','historical','current','support','fragments','W3']}))
 except Exception as error:print(json.dumps(dict(status='NOT_READY_04A38',error=type(error).__name__+': '+str(error))));raise SystemExit(1)
