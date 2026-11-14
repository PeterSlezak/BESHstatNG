"""Join every syntax identity/branch to original evidence and real CIL metadata.

This is a read-only adequacy matrix, never a scientific oracle or an ownership
completion producer. Member entry is explicitly weaker than a branch witness.
"""
import base64,csv,hashlib,json,re,sys
from collections import Counter,defaultdict
from pathlib import Path
HERE=Path(__file__).resolve().parent;ROOT=HERE.parents[3];V6=ROOT/'evidence/04A38/late-archived-entry-supplement/v6-mmrm-intercept-group'
def load(p):return json.loads(p.read_bytes())
def sha(b):return hashlib.sha256(b).hexdigest()
def pin(p):b=p.read_bytes();return dict(path=p.relative_to(ROOT).as_posix(),bytes=len(b),sha256=sha(b))
def save(n,v):
 with (HERE/n).open('x',encoding='utf-8',newline='\n') as f:json.dump(v,f,indent=2,ensure_ascii=False);f.write('\n')
def table(n,rows):
 with (HERE/n).open('x',encoding='utf-8',newline='') as f:w=csv.DictWriter(f,fieldnames=list(rows[0]),lineterminator='\n');w.writeheader();w.writerows(rows)
def main():
 assert load(V6/'MMRM_INTERCEPT_GROUP_GATE_PASS.json')['status']=='MMRM_INTERCEPT_GROUP_GATE_PASS'
 original=load(HERE/'original-universe.json');current=load(HERE/'current-universe.json');entry=load(ROOT/'evidence/04A38/entry-members.json')
 entry_bykey=defaultdict(list)
 for m in entry['members']:entry_bykey[m['signature_key']].append(m)
 snapshots={}
 for row in load(ROOT/'evidence/04A38/preedit/entry-source-snapshot.json')['files']:
  with (ROOT/row['snapshot']['path']).open(encoding='utf-8-sig',newline='') as stream:snapshots[row['source']['path']]=stream.read()
 def normalized_span(text,start):return len(text.encode('utf-16-le')[:start*2].decode('utf-16-le').replace('\r\n','\n').encode('utf-16-le'))//2
 original_map={};crosswalk=[]
 for m in original['members']:
  matches=[e for e in entry_bykey[m['signature_key']] if e['body_token_sha256']==m['body_token_sha256']]
  if len(matches)>1:
   matches=[e for e in matches if normalized_span(snapshots[e['path']],e['span_start'])==m['span_start']]
  assert len(matches)==1,('Ambiguous original identity',m['symbol_id'],len(matches));old=matches[0];original_map[m['symbol_id']]=old['symbol_id']
  crosswalk.append(dict(original_git_member_id=m['symbol_id'],canonical_archived_id=old['symbol_id'],signature_key=m['signature_key'],body_token_sha256=m['body_token_sha256'],original_git_start_line=m['start_line'],archived_start_line=old['start_line'],relation='SAME_TOKENS; independently recorded rawLF/CRLF spans'))
 assert len(crosswalk)==len(entry['members']) and len({r['canonical_archived_id'] for r in crosswalk})==len(entry['members'])
 table('original-identity-crosswalk.csv',crosswalk)
 with (ROOT/'evidence/04A38/udf-member-preparation-plan-v1.csv').open(encoding='utf-8-sig',newline='') as f:plan={r['current_member_id']:r for r in csv.DictReader(f)}
 with (ROOT/'evidence/04A38/ownership-progress-validation-v1-v2.csv').open(encoding='utf-8-sig',newline='') as f:progress={r['original_member_id']:r for r in csv.DictReader(f)}
 config_members={};error_witnesses=defaultdict(lambda:defaultdict(set));pins=[];case_index={};all_cases=0
 corpora=[('original-v1','frozen-v2',24732),('public-options-v1','attempts/public-options-v1',2508),('public-primitives-v1','attempts/public-primitives-v1',226),('model-inputs-v2','attempts/model-inputs-v2',882),('math-edges-v1','attempts/math-edges-v1',618)]
 for config in ['Debug','Release']:
  reached=defaultdict(set);case_index[config]={}
  for name,relative,count in corpora:
   base=ROOT/'evidence/04A38/preedit'/relative;cases=list(map(json.loads,(base/'cases.jsonl').read_bytes().splitlines()));assert len(cases)==count
   trace=ROOT/'evidence/04A38/preedit/attempts/capture-v10'/('capture-'+config+'-trace-v1.tsv.reachability.jsonl') if name=='original-v1' else base/('capture-'+config+'-trace-v1.tsv.reachability.jsonl')
   hits={r['case_id']:r['reached'] for r in map(json.loads,trace.read_bytes().splitlines())}
   output=base/('original-'+config+'.tsv' if name=='original-v1' else 'capture-'+config+'-original-v1.tsv')
   with output.open(encoding='utf-8',newline='') as f:rows={r['case_id']:r for r in csv.DictReader(f,delimiter='\t')}
   assert len(rows)==len(hits)==count and set(rows)==set(hits)
   for c in cases:
    key=c['case_id'];r=rows[key];case_index[config][key]=dict(corpus=name,culture=c['culture'],target=c.get('target'),outcome=r['outcome'],input_sha256=sha(r['byref_arguments'].encode()),result_sha256=sha(r['result'].encode()))
    for member in hits[key]:reached[member].add(key)
    if r['outcome']=='RETURN_TEXT':
     text=base64.b64decode(r['result'][1:]).decode()
     for member in hits[key]:error_witnesses[member][text].add(config+'/'+key)
   pins.extend([pin(base/'cases.jsonl'),pin(trace),pin(output)]);all_cases+=count
  config_members[config]={k:sorted(v) for k,v in reached.items()}
 assert all(len(v)==28966 for v in case_index.values())
 save('genuine-preedit-case-index.json',dict(cases=case_index,inputs=pins,qualification='Recorded member reachability and outcomes only; not automatically branch coverage'))
 save('member-preedit-witness-sets.json',dict(configurations=config_members))
 late=[]
 for version,configcount in [('v1',432),('v2-gee-profile',12),('v3-mmrm-headered-profile',2),('v4-mmrm-duplicate-guards',4),('v5-mmrm-unmatched-profile',2),('v6-mmrm-intercept-group',2)]:
  folder=ROOT/'evidence/04A38/late-archived-entry-supplement'/version
  for config in ['Debug','Release']:
   cases=list(map(json.loads,(folder/('inputs-'+config+'.jsonl')).read_bytes().splitlines()));assert len(cases)==configcount
   info=load(folder/'original-coverage.json')
   late.append(dict(packet=version,configuration=config,unique_cases=configcount,cases=[dict(case_id=c['case_id'],culture=c['culture'],targets=[o['target'] for o in c['operations']]) for c in cases],coverage=pin(folder/'original-coverage.json'),typed_inputs=pin(folder/('inputs-'+config+'.jsonl')),qualification='Source-specific branch evidence remains in immutable packet; no broad coverage inferred from a public target'))
 save('authorized-late-index.json',dict(configurations=['Debug','Release'],late_per_configuration=454,original_separate_per_configuration=28966,packets=late))
 compiled=load(HERE/'compiled-current.json');accepted=load(ROOT/'evidence/04A38/preedit/compiled-Debug.json')
 old_exports={e['effective_excel_name']:e for e in accepted['exports']};new_exports={e['effective_excel_name']:e for e in compiled['exports']}
 assert len(old_exports)==len(new_exports)==268 and old_exports==new_exports,'Actual compiled export/default/attribute drift'
 old_syntax={e['name']:e for e in original['exports']};new_syntax={e['name']:e for e in current['exports']}
 assert len(old_syntax)==len(new_syntax)==268
 for name,old in old_syntax.items():assert all(old[k]==new_syntax[name][k] for k in ['name','signature','attribute','arguments']),name
 save('export-comparison.json',dict(status='268_SOURCE_AND_COMPILED_CONTRACTS_EXACT',compiled_current=pin(HERE/'compiled-current.json'),compiled_original=pin(ROOT/'evidence/04A38/preedit/compiled-Debug.json'),source_current=pin(HERE/'current-universe.json'),source_original=pin(HERE/'original-universe.json'),names=sorted(new_exports),native_registration='NOT_RUN'))
 graph=load(HERE/'compiled-current-closure.json');methods={r['method']:r for r in graph['methods']};byname=defaultdict(list);consumers=defaultdict(set)
 for method,row in methods.items():byname[method.split('(')[0].replace('+','.')].append(row)
 for e in compiled['exports']:
  todo=[e['method']];seen=set()
  while todo:
   key=todo.pop()
   if key in seen:continue
   seen.add(key);consumers[key].add(e['effective_excel_name']);todo.extend(c['target'] for c in methods.get(key,{}).get('calls',[]) if c['assembly'] in ['BESHStatNG','BESHStat.Core','BESHStat.Excel.Common'])
 matrix=[];current_bykey=defaultdict(list)
 for c in current['members']:current_bykey[c['signature_key']].append(c)
 for m in original['members']:
  oldid=original_map[m['symbol_id']];p=plan[oldid];matches=current_bykey[m['signature_key']];binding=json.loads(progress[oldid]['windows_bindings'])
  live=[methods[k] for k in binding if k in methods];edges=[dict(windows_method=r['method'],**c) for r in live for c in r['calls'] if c['assembly'] in ['BESHStat.Core','BESHStat.Excel.Common']]
  matrix.append(dict(canonical_original_id=oldid,git_original_id=m['symbol_id'],path=m['path'],name=m['name'],kind=m['kind'],original_start_line=m['start_line'],original_end_line=m['end_line'],original_body_token_sha256=m['body_token_sha256'],current_ids=[c['symbol_id'] for c in matches],current_bodies_token_sha256=[c['body_token_sha256'] for c in matches],body_tokens_unchanged=len(matches)==1 and matches[0]['body_token_sha256']==m['body_token_sha256'],public_callers=sorted(set().union(*(consumers[r['method']] for r in live))) if live else [],compiled_windows_bindings=binding,actual_portable_edges=edges,
   preedit_witness_set=oldid,preedit_entered_counts={c:len(config_members[c].get(oldid,[])) for c in ['Debug','Release']},late_witness_index='authorized-late-index.json; case/member/path must be proved by its packet',target_owner_plan=p['planned_disposition'],planned_boundary=p['planned_scope'],inherited_progress=progress[oldid]['progress'],whole_body_disposition='NOT_CLOSED_W1_PLANNING_ONLY'))
 save('member-behavior-evidence-target-matrix.json',dict(status='FULL_MEMBER_UNIVERSE_JOINED_NOT_OWNERSHIP_CLOSED',members=matrix))
 fragments=[];candidates=[];orig_byid={m['symbol_id']:m for m in original['members']}
 for f in original['fragments']:
  oldid=original_map.get(f['member_id']);m=orig_byid.get(f['member_id']);literal_matches=[]
  if oldid and f['is_branch']:
   for literal in f['arm_string_literals']:
    if len(literal)<18:continue
    for text,ids in error_witnesses[oldid].items():
     if literal in text:literal_matches.append(dict(source_literal=literal,witness_case_ids=sorted(ids),qualification='Entered member plus matching text is a review candidate, not an instrumented branch PASS'))
  row=dict(fragment_id=f['fragment_id'],canonical_original_member_id=oldid,path=f['path'],kind=f['kind'],start_line=f['start_line'],end_line=f['end_line'],span_start=f['span_start'],span_length=f['span_length'],is_branch=f['is_branch'],is_inline=f['is_inline'],token_sha256=f['token_sha256'],code=f['code'],preedit_witness_set=oldid,literal_path_review_candidates=literal_matches,adequacy='REQUIRES_SOURCE_BRANCH_REVIEW' if f['is_branch'] else 'STRUCTURAL_STATEMENT_OR_INLINE_ACCOUNTED;W2_DISPOSITION_PENDING',planned_target_owner=plan[oldid]['planned_disposition'] if oldid else 'SUPPORT_DECLARATION_REVIEW',native_scientific='NOT_RUN_NOT_GOLDEN')
  fragments.append(row)
  if f['is_branch'] and oldid and not literal_matches and any(len(s)>18 and re.search(r'(?i)must|cannot|not |requires|missing|invalid|exactly|finite|duplicate',s) for s in f['arm_string_literals']):
   candidates.append(dict(fragment_id=f['fragment_id'],member_id=oldid,path=f['path'],member=m['name'],line=f['start_line'],condition=f['code'],arm_literals=f['arm_string_literals'],entered_counts={c:len(config_members[c].get(oldid,[])) for c in ['Debug','Release']},status='CANDIDATE_REQUIRES_ADEQUACY_REVIEW_NOT_AUTOMATIC_REFERENCE_REQUEST'))
 save('branch-statement-inline-matrix.json',dict(status='FULL_SYNTAX_POPULATION;ADEQUACY_REVIEW_NOT_YET_COMPLETE',fragments=fragments))
 save('unresolved-branch-review-frontier.json',dict(status='READ_ONLY_CANDIDATES_NOT_APPROVED_OR_MATERIALIZED',candidates=candidates,new_reference_executions=0,new_input_packets=0))
 stats=dict(original_behavioral_members=len(original['members']),current_behavioral_members=len(current['members']),historical_ledger=703,additional_accepted_native_callback=1,
  original_support_raw=len(original['supports']),original_support_overlap=sum(r['already_in_primary_population'] for r in original['supports']),original_support_distinct=sum(not r['already_in_primary_population'] for r in original['supports']),
  current_support_raw=len(current['supports']),current_support_overlap=sum(r['already_in_primary_population'] for r in current['supports']),current_support_distinct=sum(not r['already_in_primary_population'] for r in current['supports']),
  original_fragments=len(original['fragments']),current_fragments=len(current['fragments']),original_branches=sum(r['is_branch'] for r in original['fragments']),current_branches=sum(r['is_branch'] for r in current['fragments']),original_inline=sum(r['is_inline'] for r in original['fragments']),current_inline=sum(r['is_inline'] for r in current['fragments']),
  original_sources=len(original['sources']),current_sources=len(current['sources']),exports=268,source_member_matrix_rows=len(matrix),unreviewed_branch_candidates=len(candidates),whole_body_closed=0,whole_body_open=len(matrix),new_current_signature_keys=sum(k not in entry_bykey for k in current_bykey),original_dropped_signature_keys=sum(not current_bykey[m['signature_key']] for m in original['members']))
 save('census-summary.json',dict(status='W1_FULL_UNIVERSE_ENUMERATED;BRANCH_ADEQUACY_AND_SCHEDULE_REVIEW_IN_PROGRESS',counts=stats,original_per_configuration=28966,late_per_configuration=454,new_original_reference_cases_beyond_F=0,new_production_changes=0))
 print(json.dumps(stats))
if __name__=='__main__':main()
