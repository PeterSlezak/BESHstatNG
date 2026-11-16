"""Read-only standalone delivery reader; no extraction, native API or product invocation."""
import collections,copy,hashlib,io,json,re,sys,unicodedata,zipfile,zlib
from pathlib import Path
PREFIX='evidence/04A45/'
MAIN='350ffbc0924b09199249316a3ee2f31bc3dd98e8'
GATES=dict(P='NO',N1='NO GO',N2='NO',C2='UNMET',C3='UNAPPROVED',scientific='NOT_GOLDEN',next='04A46_NOT_AUTHORIZED')
READY='READY_FOR_OWNER_04A45_SAFE_HARNESS_REVIEW__NATIVE_GATE_NOT_AUTHORIZED'
def sha(b):return hashlib.sha256(b).hexdigest()
def safe(n):
 assert n and not n.startswith('/') and chr(92) not in n and ':' not in n
 assert all(p not in ('','.','..') and not p.endswith((' ','.')) for p in n.split('/'))
 assert not re.search(r'(?i)(?:^|/)(?:con|prn|aux|nul|com[1-9]|lpt[1-9])(?:\.|/|$)',n)
def verify(archive,receipt=None,private_identifiers=()):
 with zipfile.ZipFile(archive) as z:
  names=z.namelist();assert len(names)==len(set(names))==len({unicodedata.normalize('NFKC',n).casefold() for n in names})
  assert sum(i.file_size for i in z.infolist())<200_000_000
  for i in z.infolist():safe(i.filename);assert not i.is_dir() and ((i.external_attr>>16)&0o170000)!=0o120000
  files={i.filename:z.read(i) for i in z.infolist()}
  # Optional independently supplied identity guard. Original13/14 owner-local
  # full scans remain mandatory;this neutral replay is a separate positive control.
  for n,b in files.items():
   text=unicodedata.normalize('NFKC',n+'\n'+b.decode('utf-8-sig')).casefold()
   for token in private_identifiers:assert not re.search(r'(?<!\w)'+re.escape(unicodedata.normalize('NFKC',token).casefold())+r'(?!\w)',text),'PRIVATE_IDENTITY_CONTROL'
  def read(p):return json.loads(files[PREFIX+p])
  def pinned(r):assert len(files[r['path']])==r['bytes'] and sha(files[r['path']])==r['sha256']
  index=json.loads(files['.review-index.json']);rows=index['files'];assert len(rows)==len({r['path'] for r in rows})
  assert set(files)=={r['path'] for r in rows}|{'.review-index.json'}
  for r in rows:pinned(r);assert r['crc32']==f'{zlib.crc32(files[r["path"]])&0xffffffff:08x}'
  state=read('final/readiness-v1.json');assert state['status']==READY and state['gates']==GATES and state['live_native']=='NOT_RUN' and state['actual_HWND_PID_XLL']=='NOT_VERIFIED' and state['approval']=='NOT_APPROVED' and not state['native_adapter'] and state['original_controls_discharged']==0
  auth=read('entry/authentication-v1.json');assert auth['HEAD']==auth['main']==auth['merge_base']==MAIN and auth['branch']=='codex/04A-45' and auth['staged_empty'] and not auth['preexisting_owner_edits']
  freeze=read('matrix/source-freeze-v1.json');assert not freeze['production_changed'] and len(freeze['source_delta'])==1 and len(freeze['new_tests'])==5
  for r in freeze['new_tests']+[r['after'] for r in freeze['source_delta']]:pinned(r)
  source=read('final/source-review-v1.json')
  for r in source['shipping_sources']:pinned(r);assert r['sha256']==source['entry_source_hashes'][r['path']]
  for r in source['shipping_sources']:assert not r['path'].startswith(('BESHStat.Core/','BESHStat.Excel.Common/'))
  ribbon=read('scope/ribbon-v1.json');assert len(ribbon['callbacks'])==85 and ribbon['action_occurrences']==84 and ribbon['distinct_actions']==81
  ids=[r['id'] for r in ribbon['controls']];assert len(ids)==len(set(ids))
  assert all(r['classification']==('WINDOWS_HOST_ONLY_NATIVE_PENDING' if r['attribute']=='loadImage' else 'FAKE_HOST_EXECUTED') for r in ribbon['callbacks'])
  xml=files['BESHstatNG/Resources/RibbonXML.xml'].decode('utf-8-sig')
  import xml.etree.ElementTree as ET
  root=ET.fromstring(xml);actual=[(e.attrib.get('id','customUI'),k,v) for e in root.iter() for k,v in e.attrib.items() if k.startswith(('on','get')) or k=='loadImage']
  assert actual==[(r['control'],r['attribute'],r['callback']) for r in ribbon['callbacks']]
  projection=read('fixtures/ribbon-projection-final-v1.json');pinned(projection['source']);pinned(projection['projection'])
  original=files[projection['source']['path']].decode('utf-8-sig').replace('\r\n','\n');original=original[original.index('Public Class Ribbon'):]
  assert sha(original.encode())==projection['exact_class_slice_sha256']
  for a,b in projection['substitutions']:original=original.replace(a,b)
  fake=files[projection['projection']['path']].decode();assert fake[fake.index('Friend Class RibbonProjection45'):]==original+'\nEnd Namespace\n'
  assert not re.search(r'AppGlobals\.|ExcelDnaUtil\.|ExcelAsyncUtil\.|System\.Windows\.Forms|Process\.Start|DllImport|DoEvents',fake)
  option=read('fixtures/option-projection-final-v1.json');pinned(option['source']);pinned(option['projection'])
  original=files[option['source']['path']].decode('utf-8-sig').replace('\r\n','\n');fake=files[option['projection']['path']].decode()
  for m in option['members']:
   b=re.search(r'(?ms)^    Private Sub '+m['method']+r'\(.*?^    End Sub',original).group();assert sha(b.encode())==m['source_text_sha256'] and b in fake
  support=files['BESHStatNG.Test/src/WindowsHarness45.vb'].decode();assert 'Return False' in support and 'OFFLINE_CONSISTENT_NOT_VERIFIED' in support
  assert not re.search(r'DllImport|Process\.Start|ExcelDnaUtil|System\.Windows\.Forms|DoEvents|UIAutomation|GetWindowThreadProcessId',support)
  inspector=files[PREFIX+'fixtures/InspectWiring45.cs'].decode();assert 'ReflectionOnlyLoadFrom' in inspector and 'GetMethodBody' in inspector and not re.search(r'Process\.Start|DllImport|\.Invoke\(',inspector)
  mode=read('final/inspector-invocation-v1.json');assert mode['mode']=='REFLECTION_ONLY_METADATA_AND_IL_NOT_RUNTIME_BINDING' and mode['arguments_contract']=='LIBRARY_DIRECTORY_AND_NEW_JSON_ONLY' and not mode['native_or_product_methods_invoked']
  events=read('scope/event-map-review-v1.json');assert len(events['rows'])==19596 and not events['unresolved'] and events['counts'].get('UNRESOLVED_WIRING',0)==0 and len(events['compiled_boundaries'])==82
  assert len({r[0] for r in events['rows']})==len(events['rows'])
  controls=read('scope/controls-v2.json');assert len(controls['fields'])==2386 and len(controls['forms'])==30 and controls['non_designer_forms']==['BESHStatNG.UiUpdateAvailable']
  obligations=read('scope/original-obligations-v1.json');assert len(obligations['rows'])==1514 and len({r['obligation_id'] for r in obligations['rows']})==1514 and obligations['discharged']==0 and obligations['restricted_controls']==802
  assert all(not r['native_executed'] and not r['discharged'] for r in obligations['rows'])
  delta=read('register/interim-delta-v1.json');assert delta['counts']==dict(canonical_entries=1169,original_obligations=1514,restricted_controls=802,mappings=2967) and len(delta['canonical_ids'])==1169 and len(delta['crosswalk'])==2967
  assert delta['original_controls_discharged']==delta['original_obligations_discharged']==delta['new_canonical_ids']==0 and delta['original48_exit_status'] is None
  prior=read('final/accepted44-test-identities-v1.json');assert prior['historical_scope_status']=='FAIL_UNWAIVED' and len(prior['tests'])==21266
  debug=read('build-and-tests/Debug-identities-v1.json');release=read('build-and-tests/Release-identities-v1.json')
  assert debug['summary']['outcomes']==dict(Passed=21397,Failed=14) and len(debug['tests'])==21411 and len(debug['added'])==145
  key=lambda r:(r['suite'],r['identity_sha256'],r['occurrence'])
  current={key(r):r for r in debug['tests']};assert len(current)==21411
  assert all(current[key(r)]['outcome']==r['outcome'] for r in prior['tests'])
  assert len(debug['nonpasses'])==14 and all(r['classification']=='RETAINED_IDENTICAL' for r in debug['nonpasses'])
  old={key(r):r for r in prior['nonpasses']}
  assert all(all(r[k]==old[key(r)][k] for k in ['message_sha256','traceback_sha256']) for r in debug['nonpasses'])
  assert release['summary']['outcomes']==dict(Passed=1230) and release['full_Release']=='NOT_RUN'
  fail=read('final/failed-attempts-v1.json');assert fail['retained'] and fail['corrected_wave_A_failed_tests']==6 and fail['deleted_or_overwritten_protected_evidence']==0
  abi=read('abi-cil/compiled-ui-and-build-attribution-v1.json');assert len(abi['rows'])==2
  for c in ['Debug','Release']:
   row=next(r for r in abi['rows'] if r['configuration']==c);assert row['exports']==len(row['item_digests'])==268 and row['UI_Ribbon_event_fields_methods_IL_exact_accepted44'] and row['xll']=='NOT_BUILT_NOT_LOADED_NOT_AUTHORIZED'
   proof=read('abi-cil/'+c+'-proof-v1.json');assert not proof['changed_methods'] and not proof['unexpected'] and proof['exports']==268
   compat=read('compatibility/'+c+'-v1.json');assert compat['cases']==29424 and compat['PRE_EDIT_FROZEN']==28966 and compat['LATE_ARCHIVED_ENTRY_SUPPLEMENT']==458 and compat['original_queries']==0
   assert all(r['original']['sha256']==r['current']['sha256'] and r['original']['bytes']==r['current']['bytes'] and r['exact'] for r in compat['results'])
   assert read('fixtures/'+c+'-44-v2.json')['cases']==591 and read('fixtures/'+c+'-progress-stub-v1.json')['cases']==72
  py=read('python/parent-event-reconciliation-v2.json');assert len(py['parents'])==len({p['id'] for p in py['parents']})==956
  assert collections.Counter(p['outcome'] for p in py['parents'])==py['summary']['parent_outcomes'] and not py['summary']['unresolved_diagnostic_events']
  disposition=read('python/dispositions-v1.json');assert disposition['unresolved']==0 and disposition['historical_failures_unwaived'] and disposition['historical04A08_scope_status']=='FAIL_UNWAIVED'
  assert read('matrix/serial-checks-v1.json')['exits']==[1,1,0]
  preservation=read('preservation/readback-v1.json');assert preservation['entry_tracked']==14931 and preservation['checkpoint_copies']==468 and preservation['historical_paths']==8984 and preservation['raw40_paths']==1906
  assert preservation['continuous_physical_identity']=='NOT_VERIFIED' and preservation['historical_file_identity_drifts']==788 and not preservation['W3_union']['fresh_full_union_readback']
  privacy=read('privacy/publication-policy-v1.json');assert privacy['required_scanners']==['13','14'] and privacy['waivers']==0 and privacy['full_candidate_scope']=='ORIGINAL_UNCHANGED_PATHS_FUNCTIONS' and privacy['physical_scope']=='EVERY_MEMBER_INCLUDING_INDEX'
  gate=read('native-risk/attribution-contract-v1.json');assert not gate['native_may_run'] and gate['approval']=='NOT_APPROVED' and gate['live_native']=='NOT_RUN' and gate['actual_HWND_PID_XLL_attribution']=='NOT_VERIFIED'
  if receipt is not None:
   external=json.loads(Path(receipt).read_bytes());data=Path(archive).read_bytes();assert len(data)==external['archive']['bytes'] and sha(data)==external['archive']['sha256'] and external['status']==READY
   assert set(files)=={r['path'] for r in external['physical_members']}
   for r in external['physical_members']:pinned(r);assert r['crc32']==f'{zlib.crc32(files[r["path"]])&0xffffffff:08x}'
   assert len(external['full_actual_candidate_scans'])==len(external['physical_scans'])==2
   assert all(r['findings']==r['errors']==0 for r in external['full_actual_candidate_scans']+external['physical_scans'])
   assert len(external['adverse_controls'])>=20 and all(r['detected'] for r in external['adverse_controls'])
   for r in external['primary_reports']:pinned(r)
  return dict(status='PASS_EXTERNAL_AND_SEMANTIC' if receipt else 'PASS_STRUCTURAL_AND_SEMANTIC',members=len(files),native='NOT_RUN',readiness=READY,effective_only_with_matching_external_receipt=True)

def adverse(archive):
 with zipfile.ZipFile(archive) as z:original={n:z.read(n) for n in z.namelist()}
 def mutate(f,p,action):d=json.loads(f[PREFIX+p]);action(d);f[PREFIX+p]=json.dumps(d).encode()
 actions={
 'forged_PASS':('final/readiness-v1.json',lambda d:d.__setitem__('status','PASS_NATIVE')),
 'false_native':('final/readiness-v1.json',lambda d:d.__setitem__('live_native','PASS')),
 'local_approval_boolean':('native-risk/attribution-contract-v1.json',lambda d:d.__setitem__('native_may_run',True)),
 'inspector_invocation_altered':('final/inspector-invocation-v1.json',lambda d:d.__setitem__('arguments_contract','LIVE_HOST')),
 'skipped_callback':('scope/ribbon-v1.json',lambda d:d['callbacks'].pop()),
 'added_control':('scope/ribbon-v1.json',lambda d:d['controls'].append(d['controls'][0])),
 'mutated_event_map':('scope/event-map-review-v1.json',lambda d:d['rows'].pop()),
 'missing_old_control':('scope/original-obligations-v1.json',lambda d:d['rows'].pop()),
 'duplicated_handler':('scope/event-map-review-v1.json',lambda d:d['rows'].append(d['rows'][0])),
 'scope_FAIL_suppressed':('final/accepted44-test-identities-v1.json',lambda d:d.__setitem__('historical_scope_status','PASS')),
 'lost_previous_test':('final/accepted44-test-identities-v1.json',lambda d:d['tests'].pop()),
 'lost_failure':('build-and-tests/Debug-identities-v1.json',lambda d:d['nonpasses'].pop()),
 'fake_full_Release':('build-and-tests/Release-identities-v1.json',lambda d:d.__setitem__('full_Release','PASS')),
 'fake_scientific_PASS':('final/readiness-v1.json',lambda d:d['gates'].__setitem__('scientific','GOLDEN')),
 'compatibility_lost':('compatibility/Debug-v1.json',lambda d:d.__setitem__('cases',29423)),
 'original_discharge':('register/interim-delta-v1.json',lambda d:d.__setitem__('original_controls_discharged',1)),
 'unresolved_hidden':('scope/event-map-review-v1.json',lambda d:d['unresolved'].append('hidden')),
 'privacy_narrowed':('privacy/publication-policy-v1.json',lambda d:d.__setitem__('physical_scope','SELECTED_MEMBERS')),
 }
 labels=list(actions)+['missing','extra','unsafe','casefold_collision','NFKC_collision','duplicate_member','changed_handler_source','changed_error_cancel_priority','private_token_leak']
 result=[]
 for label in labels:
  f=dict(original);reindex=True;identities=[]
  if label in actions:mutate(f,*actions[label])
  elif label=='missing':f.pop(PREFIX+'scope/ribbon-v1.json');reindex=False
  elif label=='extra':f['extra.txt']=b'extra';reindex=False
  elif label=='unsafe':f['../escape.txt']=b'unsafe'
  elif label=='casefold_collision':f[(PREFIX+'review_reader.py').upper()]=f[PREFIX+'review_reader.py']
  elif label=='NFKC_collision':f[PREFIX+'\uff52eview_reader.py']=f[PREFIX+'review_reader.py']
  elif label=='changed_handler_source':f['BESHStatNG.Test/src/RibbonProjection45.vb']+=b'\nchanged\n'
  elif label=='changed_error_cancel_priority':f['BESHstatNG/src/AppInfrastructure/WindowsExecutionRun.vb']+=b'\nchanged\n'
  elif label=='private_token_leak':
   import uuid
   token='review'+uuid.uuid4().hex;identities=[token];f['identity-positive-control.txt']=token.encode()
  if reindex:f['.review-index.json']=json.dumps(dict(files=[dict(path=n,bytes=len(b),sha256=sha(b),crc32=f'{zlib.crc32(b)&0xffffffff:08x}') for n,b in sorted(f.items()) if n!='.review-index.json'])).encode()
  stream=io.BytesIO()
  with zipfile.ZipFile(stream,'w',zipfile.ZIP_STORED) as z:
   for n,b in f.items():z.writestr(n,b)
   if label=='duplicate_member':z.writestr('.review-index.json',f['.review-index.json'])
  stream.seek(0)
  try:verify(stream,private_identifiers=identities)
  except (AssertionError,KeyError,ValueError,zipfile.BadZipFile):result.append(dict(control=label,detected=True,method='Altered physical in-memory ZIP;semantic/hash/name reader executed'))
  else:raise AssertionError('MISSED_ADVERSE_CONTROL:'+label)
 return result
if __name__=='__main__':print(json.dumps(verify(sys.argv[1],sys.argv[2] if len(sys.argv)>2 else None),sort_keys=True))
