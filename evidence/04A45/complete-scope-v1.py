from common45 import *
import csv,re,collections
lock();target=PRIVATE/'syntax-current-v3'
def table(n):return list(csv.DictReader((target/n).open(encoding='utf-8-sig',newline='')))
controls=load(HERE/'scope/controls-v1.json');types=table('types.csv');symbols=table('symbols.csv');refs=table('references.csv')
for form in controls['forms']:
 form['parts']=[dict(path=t['path'],start=int(t['start_line']),end=int(t['end_line']),source=pin(t['path'])) for t in types if t['qualified_type']==form['owner'] and t['kind']=='ClassBlock']
 form['non_designer']=not any(p['path'].endswith('.Designer.vb') for p in form['parts'])
controls['non_designer_forms']=[f['owner'] for f in controls['forms'] if f['non_designer']]
save('scope/controls-v2.json',controls)
events=load(HERE/'scope/event-map-v3.json')
for e in events['edges']:
 if 'compiled_evidence' in e:e['evidence']='evidence/04A45/scope/compiled-wiring-Debug-v2.json;Release counterpart'
# Compact exact source crosswalk: omit repeated explanatory strings, retain identity and status.
columns=['edge_id','path','owner','owner_id','kind','event_or_control','target_ids','target_expression_sha256','start_line','end_line','source_sha256','classification']
save('scope/event-map-review-v1.json',dict(columns=columns,rows=[[e.get(k) for k in columns] for e in events['edges']],counts=events['counts'],parser_ambiguities=82,compiled_boundaries=[dict(edge_id=e['edge_id'],proofs=e['compiled_evidence']) for e in events['edges'] if 'compiled_evidence' in e],unresolved=events['remaining_unresolved'],classification_rules=dict(FAKE_HOST_EXECUTED='Exact source handler compiled against fake events and executed;native pending',STATIC_SOURCE_VERIFIED='Roslyn declaration/configuration/candidate only;not actual delivery',WINDOWS_HOST_ONLY_NATIVE_PENDING='Compiled host/delegate/COM boundary;instance subscription and native delivery unproved',UNRESOLVED_WIRING='Software wiring review blocker;never counted as successful coverage'),full_owner_local_map=pin(HERE/'scope/event-map-v3.json'),native='NOT_RUN'))
by_owner=collections.defaultdict(list)
for r in refs:
 if r['path'].startswith(('BESHstatNG/src/UI/','BESHstatNG/src/AppInfrastructure/')) or r['path']=='BESHstatNG/BESHStatAddIn.vb':by_owner[r['owner_id']].append(r)
owners={e['owner_id'] for e in events['edges']};owners.update(x for e in events['edges'] for x in e['target_ids'].split('; ') if x)
chains=[]
for s in symbols:
 if s['symbol_id'] not in owners:continue
 calls=[]
 for r in by_owner[s['symbol_id']]:
  target=r['reference'];host=bool(re.search(r'(?i)DoEvents|(?:Begin)?Invoke|Process\.Start|MsgBox|\.Show|\.Workbooks|\.ActiveSheet|ExcelDna|Write|Output|Import|RefEdit|ExcelConnector',target))
  calls.append(dict(line=int(r['line']),kind=r['kind'],target=target,arguments_sha256=sha(r['arguments'].encode()),boundary='HOST_OR_WRITER_CANDIDATE' if host else 'REQUEST_VALIDATION_RESULT_CANDIDATE' if re.search(r'(?i)Request|Validate|checkInput|Analysis|Execute|Build|Result|Cancel|Checkpoint',target) else 'SOURCE_CALL_CANDIDATE'))
 chains.append(dict(member_id=s['symbol_id'],owner=s['containing_type'],name=s['name'],signature=s['signature'],path=s['path'],line=int(s['start_line']),body_sha256=s['body_sha256'],source_sha256=s['source_sha256'],calls=calls,classification='STATIC_SOURCE_VERIFIED',reason='Source control/event->member->invocation chain;overload/late-bound request and side-effect semantics remain candidates unless covered by compiled/fake proof',evidence='evidence/04A45/scope/event-map-review-v1.json',next_gate='Native input acquisition/validation/output and46/47 family contracts remain separately restricted'))
save('scope/execution-chains-v1.json',dict(chains=chains,source_kind='CURRENT_MAIN_PRE_EDIT_04A45_UNCHANGED_PRODUCTION',limits='No inferred full interprocedural call graph or request validation claim. Exact source reached Ribbon/option/lease and existing managed execution tests provide software evidence only.',native='NOT_RUN'))
print('Compact event map and',len(chains),'source event/request/host candidate chains;',len(controls['non_designer_forms']),'non-designer forms',flush=True)
