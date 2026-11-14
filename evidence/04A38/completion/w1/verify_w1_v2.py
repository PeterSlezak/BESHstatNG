"""Independent stdlib source/span/population/export and gap-stop verifier.

No producer or census executable is imported. Rechecks real source bytes and
frozen case/output/reachability records; never executes a proposed reference.
"""
import base64,csv,hashlib,json,re,sys
from collections import Counter
from pathlib import Path
HERE=Path(__file__).resolve().parent;ROOT=HERE.parents[3]
V6_PREFIX='evidence/04A38/late-archived-entry-supplement/v6-mmrm-intercept-group/'
sys.path.insert(0,str(ROOT/V6_PREFIX))
from verify_supplement import inspect as F,InvalidEvidence,need,digest,check_pin
W='evidence/04A38/completion/w1/'
def load(p):return json.loads(p.read_bytes())
def text(data):return data.decode('utf-8-sig')
def utf16_slice(s,start,length):return s.encode('utf-16-le')[start*2:(start+length)*2].decode('utf-16-le')
def original_bytes(root,path,expected):
 index=load(root/'evidence/04A38/preedit/entry-source-snapshot.json')['files'];row=next(r for r in index if r['source']['path']==path)
 raw=(root/row['snapshot']['path']).read_bytes();variants={raw,raw.replace(b'\r\n',b'\n'),raw.replace(b'\r\n',b'\n').replace(b'\n',b'\r\n')}
 matches=[b for b in variants if len(b)==expected['bytes'] and digest(b)==expected['sha256']];need(len(matches)==1,'Original source normalization ambiguous/mismatched');return matches[0]
def inspect(root=ROOT):
 root=Path(root);here=root/W;fproof=F(root)
 need(all(r['exact']==2 for r in fproof['configurations'].values()),'F not exact in both configurations')
 universes={role:load(here/(role+'-universe.json')) for role in ['original','current']};verified_fragments={}
 for role,u in universes.items():
  expected_members=704 if role=='original' else 719;expected_fragments=20683 if role=='original' else 16946
  need(len(u['sources'])==41 and len(u['members'])==expected_members and len(u['fragments'])==expected_fragments and len(u['exports'])==268,'Omitted/extra source/member/fragment/export')
  need(len({m['symbol_id'] for m in u['members']})==expected_members and len({f['fragment_id'] for f in u['fragments']})==expected_fragments,'Duplicate source identity')
  need(len(u['supports'])==347 and sum(s['already_in_primary_population'] for s in u['supports'])==30,'Supporting declaration population/overlap wrong')
  sources={}
  for row in u['sources']:
   path=row['path'];need(path.startswith('BESHstatNG/src/ExcelUDFs/') and '..' not in path and '\\' not in path,'Invalid source path')
   data=original_bytes(root,path,row) if role=='original' else (root/path).read_bytes()
   need(len(data)==row['bytes'] and digest(data)==row['sha256'],'Changed source bytes');sources[path]=text(data)
  ids={m['symbol_id'] for m in u['members']}
  for m in u['members']:
   piece=utf16_slice(sources[m['path']],m['span_start'],m['span_length']);need(digest(piece.encode())==m['body_sha256'],'Member span/body not sourced')
   need(m['parent_member'] is None or m['parent_member'] in ids,'Orphan inline member')
  for s in u['supports']:
   piece=utf16_slice(sources[s['path']],s['span_start'],s['span_length']);need(s['declaration'] in piece and digest(s['declaration'].encode())==s['declaration_sha256'],'Unsourced support declaration')
  for frag in u['fragments']:
   piece=utf16_slice(sources[frag['path']],frag['span_start'],frag['span_length']);need(piece==frag['code'],'Statement/branch/inline span differs from actual source')
   need(frag['member_id'] is None or frag['member_id'] in ids,'Orphan statement ownership')
  verified_fragments[role]=len(u['fragments'])
 original,current=universes['original'],universes['current'];need(sum(f['is_branch'] for f in original['fragments'])==5511 and sum(f['is_branch'] for f in current['fragments'])==4006,'Branch census mismatch')
 compiled=load(here/'compiled-current.json');baseline=load(root/'evidence/04A38/preedit/compiled-Debug.json')
 need({e['effective_excel_name']:e for e in compiled['exports']}=={e['effective_excel_name']:e for e in baseline['exports']},'Compiled export/default/attribute change')
 a={e['name']:e for e in original['exports']};b={e['name']:e for e in current['exports']};need(set(a)==set(b) and len(a)==268,'Source export changed')
 for name in a:need(all(a[name][k]==b[name][k] for k in ['signature','attribute','arguments']),'Source signature/default/attribute change')
 matrix=load(here/'member-behavior-evidence-target-matrix-v3.json')['members'];entry=load(root/'evidence/04A38/entry-members.json')['members'];entries={m['name']:m for m in entry}
 need(len(matrix)==704 and {m['canonical_original_id'] for m in matrix}=={m['symbol_id'] for m in entry},'Historical identity omitted/duplicated')
 review=load(here/'complete-branch-adequacy-matrix.json')['branches'];need(len(review)==5511 and {f['fragment_id'] for f in review}=={f['fragment_id'] for f in original['fragments'] if f['is_branch']},'Omitted/extra branch review')
 need(all(r['whole_body_closed'] is False for r in review),'Unimplemented source declared ownership closed')
 current_plan=load(here/'current-member-support-inline-plan.json');need(len(current_plan['members'])==719 and len(current_plan['support_declarations'])==347 and len(current_plan['current_fragments'])==16946,'Current/support/inline scope omitted')
 need(all(m['status']=='W2_WHOLE_BODY_DISPOSITION_REQUIRED' for m in current_plan['members']),'Unsupported W2 completion')
 schedule=load(here/'w2-implementation-schedule.json');need({s['path'] for s in schedule['sources']}=={s['path'] for s in current['sources']},'UDF family missing from schedule')
 graph=load(here/'compiled-current-closure.json');methods={m['method']:m for m in graph['methods']}
 from collections import defaultdict
 consumers=defaultdict(set)
 for export in compiled['exports']:
  todo=[export['method']];visited=set()
  while todo:
   method=todo.pop()
   if method in visited:continue
   visited.add(method);consumers[method].add(export['effective_excel_name'])
   todo.extend(c['target'] for c in methods.get(method,{}).get('calls',[]) if c['assembly'] in ['BESHStatNG','BESHStat.Core','BESHStat.Excel.Common'])
 with (root/'evidence/04A38/ownership-progress-validation-v1-v2.csv').open(encoding='utf-8-sig',newline='') as stream:ledger={r['original_member_id']:r for r in csv.DictReader(stream)}
 for row in matrix:
  bindings=json.loads(ledger[row['canonical_original_id']]['windows_bindings'])
  need(row['compiled_windows_bindings']==bindings,'Invented Windows binding')
  actual=[methods[m] for m in bindings if m in methods]
  edges=[dict(windows_method=m['method'],**c) for m in actual for c in m['calls'] if c['assembly'] in ['BESHStat.Core','BESHStat.Excel.Common']]
  need(row['actual_portable_edges']==edges,'Fake or missing portable consumer edge')
  callers=sorted(set().union(*(consumers[m['method']] for m in actual))) if actual else []
  need(row['public_callers']==callers,'Invented or omitted registered call path')
  need(row['whole_body_disposition']=='NOT_CLOSED_W1_PLANNING_ONLY','Edge/host label passed off as whole-body closure')
 fragments={f['fragment_id']:f for f in original['fragments']}
 for row in review:
  original_fragment=fragments[row['fragment_id']]
  need(row['source_token_sha256']==original_fragment['token_sha256'] and row['condition']==original_fragment['code'],'Fabricated branch-source evidence')
 gaps=load(here/'consolidated-reference-gaps.json');need(gaps['status']=='NOT_READY_04A38' and gaps['new_reference_executions']==gaps['new_runtime_input_packets']==gaps['new_production_changes']==0,'Unauthorized capture/materialization/production/READY')
 need(gaps['W2']=='NOT_STARTED_MANDATORY_AUTHORITY_STOP' and gaps['W3']=='NOT_STARTED' and gaps['proposed_cases_per_configuration']==4 and gaps['proposed_configuration_culture_results']==8,'Wrong wave/scope')
 expected={'GEE_SPEC_INTERCEPT_ROLE':('TryGetGeeLsmEstimateSpec',[1506,1507,1508,1509],'the intercept column should not be used as a profile column; it is supplied automatically by the fitted design.'),'MMRM_WIDE_MULTIROW_REJECTION':('TryGetMmrmLsmEstimateAtSpec',[592,593,594],'wide-form at ranges must contain exactly one nonblank data row. Use name/value form for multiple AT settings by row.')}
 need({g['gap_id'] for g in gaps['essential_gaps']}==set(expected),'Lost/extra consolidated gap')
 observations={}
 folders=['frozen-v2','attempts/public-options-v1','attempts/public-primitives-v1','attempts/model-inputs-v2','attempts/math-edges-v1']
 for g in gaps['essential_gaps']:
  member,lines,message=expected[g['gap_id']];need(g['member_id']==entries[member]['symbol_id'] and g['original_lines']==lines,'Wrong canonical gap/lines')
  need(g['expected_output']=='UNKNOWN_NOT_CAPTURED' and g['state']=='NOT_AUTHORIZED_NOT_MATERIALIZED_NOT_EXECUTED' and g['cases_per_configuration']==2 and g['cultures']==['Invariant','sk-SK'] and g['configurations']==['Debug','Release'],'Fabricated outcome/authority/case scope')
  original_source=next(s for s in original['sources'] if s['path']==g['path']);source=text(original_bytes(root,g['path'],original_source)).splitlines()
  need(message in source[lines[-2]-1],'Wrong original guard assignment')
  counts={}
  for config in ['Debug','Release']:
   entered=0;outputs=0
   for relative in folders:
    folder=root/'evidence/04A38/preedit'/relative;trace=root/'evidence/04A38/preedit/attempts/capture-v10'/('capture-'+config+'-trace-v1.tsv.reachability.jsonl') if relative=='frozen-v2' else folder/('capture-'+config+'-trace-v1.tsv.reachability.jsonl')
    hits={r['case_id']:r['reached'] for r in map(json.loads,trace.read_bytes().splitlines())}
    out=folder/('original-'+config+'.tsv' if relative=='frozen-v2' else 'capture-'+config+'-original-v1.tsv')
    with out.open(encoding='utf-8',newline='') as f:
     for r in csv.DictReader(f,delimiter='\t'):
      if g['member_id'] not in hits[r['case_id']]:continue
      entered+=1
      for surface in ['result','error','caught_error_events']:
       for match in re.finditer(r'(?:^|[\{|])S([A-Za-z0-9+/]*={0,2})(?=[|\}]|$)',r[surface]):
        try:decoded=base64.b64decode(match[1],validate=True).decode()
        except (ValueError,UnicodeDecodeError):continue
        if message in decoded:outputs+=1
   need(entered==(56 if member=='TryGetGeeLsmEstimateSpec' else 26) and outputs==0,'New/omitted genuine branch witness')
   for version in ['v1','v2-gee-profile','v3-mmrm-headered-profile','v4-mmrm-duplicate-guards','v5-mmrm-unmatched-profile','v6-mmrm-intercept-group']:
    packet=root/'evidence/04A38/late-archived-entry-supplement'/version
    paths=list((packet/'original-results'/config).glob('**/results.tsv'))
    for p in paths:
     # Original error is an encoded public String; its complete fixed source
     # literal can never be introduced by scientific numeric normalization.
     encoded=base64.b64encode(('BESH.REGR.'+('GEE_LSMESTIMATE' if member=='TryGetGeeLsmEstimateSpec' else 'MMRM_LSMESTIMATE')+' error: '+message).encode())
     need(encoded not in p.read_bytes(),'Omitted late original witness')
   counts[config]=dict(genuine_helper_entries=entered,genuine_target_guard_outputs=outputs)
  observations[g['gap_id']]=counts
 return dict(status='VALID_W1_F_PASS_FULL_UNIVERSE_CONSOLIDATED_STOP',F_exact_each_configuration='2/2',source_files=41,exports_exact=268,original_members=704,current_members=719,support_distinct=317,source_fragments_verified=verified_fragments,original_branch_adequacy_rows=5511,whole_body_closed=0,whole_body_open=704,current_members_W2_pending=719,gap_observations=observations,proposed_cases_per_configuration=4,W2='NOT_STARTED',W3='NOT_STARTED',new_reference_queries=0,new_production_changes=0)
if __name__=='__main__':print(json.dumps(inspect()))
