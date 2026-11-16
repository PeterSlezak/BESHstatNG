"""Standalone read-only46 software delivery reader;no extraction,host or native calls."""
import collections,hashlib,io,json,re,sys,unicodedata,zipfile,zlib
from pathlib import Path
PREFIX='evidence/04A46/'
MAIN='ebc4a3e9df41ef8588aa7f6b25363f6d044c41a7'
PRIOR='350ffbc0924b09199249316a3ee2f31bc3dd98e8'
GATES=dict(P='NO',N1='NO GO',N2='NO',C2='UNMET',C3='UNAPPROVED',scientific='NOT_GOLDEN',next='04A47_NOT_AUTHORIZED')
STATUS='READY_FOR_OWNER_04A46_SAFE_GUI_WIRING_REVIEW__NATIVE_GATE_NOT_AUTHORIZED'
def sha(b):return hashlib.sha256(b).hexdigest()
def safe(n):
 assert n and not n.startswith('/') and chr(92) not in n and ':' not in n
 assert all(p not in ('','.','..') and not p.endswith((' ','.')) for p in n.split('/'))
 assert not re.search(r'(?i)(?:^|/)(?:con|prn|aux|nul|com[1-9]|lpt[1-9])(?:\.|/|$)',n)
def verify(archive,receipt=None,private_identifiers=()):
 with zipfile.ZipFile(archive) as z:
  names=z.namelist();assert len(names)==len(set(names))==len({unicodedata.normalize('NFKC',n).casefold() for n in names})
  assert sum(i.file_size for i in z.infolist())<150_000_000
  for i in z.infolist():safe(i.filename);assert not i.is_dir() and ((i.external_attr>>16)&0o170000)!=0o120000
  data={i.filename:z.read(i) for i in z.infolist()}
  def read(p):return json.loads(data[PREFIX+p])
  def pin(r):assert len(data[r['path']])==r['bytes'] and sha(data[r['path']])==r['sha256']
  for n,b in data.items():
   text=unicodedata.normalize('NFKC',n+'\n'+b.decode('utf-8-sig')).casefold()
   for token in private_identifiers:assert not re.search(r'(?<!\w)'+re.escape(unicodedata.normalize('NFKC',token).casefold())+r'(?!\w)',text),'PRIVATE_IDENTITY_POSITIVE_CONTROL'
  index=json.loads(data['.review-index.json'])['files'];assert len(index)==len({r['path'] for r in index})
  assert set(data)=={r['path'] for r in index}|{'.review-index.json'}
  for r in index:pin(r);assert r['crc32']==f'{zlib.crc32(data[r["path"]])&0xffffffff:08x}'
  state=read('final/readiness-v1.json');assert state['status']==STATUS and state['gates']==GATES and state['live_native']=='NOT_RUN' and state['actual_HWND_PID_XLL']=='NOT_VERIFIED' and state['approval']=='NOT_APPROVED' and not state['native_adapter'] and state['original_controls_discharged']==0
  auth=read('entry/authentication-v1.json');assert auth['HEAD']==auth['main']==auth['merge_base']==MAIN and auth['prior']==PRIOR and auth['branch']=='codex/04A-46' and auth['staged_empty'] and not auth['preexisting_owner_edits']
  accepted=read('entry/accepted45-receipt.review.json');assert accepted['status']=='READY_FOR_OWNER_04A45_SAFE_HARNESS_REVIEW__NATIVE_GATE_NOT_AUTHORIZED' and accepted['archive']['sha256']=='bd8f7be2e1190c7f2e177c1a64d76cc076e77fbc63eb8b9423c41ca772dff2ae'
  assert sha(data[PREFIX+'entry/accepted45-receipt.review.json'])=='46df6a9ed9d498555a3588de7625f0a1f70bc5fc5186bfe1329547b8eca893ed'
  freeze=read('matrix/source-freeze-v1.json');assert not freeze['production_changed'] and len(freeze['new_tests'])==8 and len(freeze['source_delta'])==1
  for r in freeze['new_tests']+[r['after'] for r in freeze['source_delta']]:pin(r)
  source=read('final/source-review-v2.json')
  for r in source['shipping_sources']:pin(r);assert r['sha256']==source['entry_source_hashes'][r['path']]
  for folder in ['wave-a','wave-b']:
   projection=read('fixtures/projections-'+folder+'-v1.json')
   bodies='\n'.join(data[PREFIX0].decode('utf-8-sig') for PREFIX0 in ['BESHStatNG.Test/src/GuiWiring46Projections.vb','BESHStatNG.Test/src/GuiWiring46ExecutionProjections.vb'])
   for m in projection['members']:
    pin(m['source']);original=data[m['source']['path']].decode('utf-8-sig').replace('\r\n','\n')
    body=m['original_text'];assert sha(body.encode())==m['source_text_sha256'] and body in original
    for a,b in m['transforms']:body=body.replace(a,b)
    if folder=='wave-b':
     for a,b in read('fixtures/host-aliases-wave-b-v4.json')['edits']:body=body.replace(a,b)
    assert body in bodies,'CHANGED_SHIPPING_HANDLER_PROJECTION'
  numeric=read('fixtures/numeric-source-v2.json');numbers=data['BESHStatNG.Test/src/GuiWiring46Numbers.vb'].decode()
  assert numbers.startswith('Option Strict On')
  for m in numeric['members']:
   pin(m['source']);assert m['text'] in data[m['source']['path']].decode('utf-8-sig').replace('\r\n','\n')
   body=m['text']
   for a,b in numeric['transform']:body=body.replace(a,b)
   assert body in numbers and sha(m['text'].encode())==m['body_sha256']
  fakes=data['BESHStatNG.Test/src/GuiWiring46Fakes.vb'].decode();services=data['BESHStatNG.Test/src/GuiWiring46Services.vb'].decode()
  assert 'If Not Enabled Then Return False' in fakes and 'RaiseEvent Click' in fakes
  assert not re.search(r'DllImport|Process\.Start|ExcelDnaUtil|System\.Windows\.Forms|DoEvents|UIAutomation|GetWindowThreadProcessId',fakes+services)
  assert 'Return False' in data['BESHStatNG.Test/src/WindowsHarness45.vb'].decode() and 'OFFLINE_CONSISTENT_NOT_VERIFIED' in data['BESHStatNG.Test/src/WindowsHarness45.vb'].decode()
  invoke=read('final/inspector-invocation-v1.json');assert invoke['mode']=='REFLECTION_ONLY_METADATA_AND_IL_NOT_RUNTIME_BINDING' and invoke['native_or_product_initializers_invoked'] is False
  family=read('scope/family-review-v2.json');assert len(family['owners'])==14 and len(family['members'])==496 and len(family['fields'])==939 and len(family['edges'])==8706 and family['source_projected_methods']==162
  assert family['counts']=={'OFFLINE_FAKE_EXECUTED':162,'WINDOWS_HOST_NATIVE_PENDING':334} and family['original_discharges']==0 and len(family['family_matrix'])==7
  assert collections.Counter(r[family['member_columns'].index('classification')] for r in family['members'])==family['counts']
  assert family['all45_census']==dict(owners=30,fields=2386,edges=19596,chains=599,callbacks=85)
  required={'UibyID','Ui82x2','Ui9ANOVA2nested','Ui12SampleSizeTtestSingleProp'};assert required<=set(family['owners'])
  scenario=read('fixtures/scenario-contract-v2.json');assert len(scenario['raised_bindings'])==107 and len({tuple(sorted(r.items())) for r in scenario['raised_bindings']})==107
  assert scenario['planning_branches']==13 and scenario['valid_planning_vectors']==scenario['rejected_planning_vectors']==78 and scenario['option_sets_per_planning_branch']==2
  assert scenario['Shapiro_minimum_source']==4 and scenario['homogeneity_masks']==list(range(16))
  assert scenario['categorical_output_priority']==['workbook','worksheet','range'] and scenario['paired_source_roles']==['RefEdit1','RefEdit2']
  assert scenario['nested_source_roles']==[dict(control='RefEdit1_Group',oneColumn=True),dict(control='RefEdit2_nested',oneColumn=True),dict(control='RefEdit3_Data',oneColumn=False)] and scenario['selected_source_row_identity']==[20,20,21,21]
  observer=read('fixtures/observer-v1.json');assert observer['case_count_per_configuration']==22 and observer['methods']==4 and len(observer['results'])==2 and observer['results'][0]['rows']==observer['results'][1]['rows']
  xyz=read('fixtures/xyz-observer-v1.json');assert xyz['cases_per_configuration']==44 and len(xyz['members'])==11 and xyz['results'][0]['rows']==xyz['results'][1]['rows'] and xyz['new_managed_identities']==0 and not xyz['frozen_test_project_changed']
  pin(xyz['projection']);vb=data[xyz['projection']['path']].decode()
  for m in xyz['members']:
   pin(m['source']);assert m['original_text'] in data[m['source']['path']].decode('utf-8-sig').replace('\r\n','\n') and m['original_text'] in vb and sha(m['original_text'].encode())==m['source_text_sha256']
  assert len(scenario['additional_xyz_bindings'])==11 and scenario['additional_xyz_cases_per_configuration']==44
  old=read('final/accepted45-test-identities-v1.json');debug=read('build-and-tests/Debug-identities-v1.json');release=read('build-and-tests/Release-identities-v1.json');key=lambda r:(r['suite'],r['identity_sha256'],r['occurrence'])
  assert len(old['tests'])==21411 and old['historical_scope_status']=='FAIL_UNWAIVED' and len(debug['tests'])==21754 and debug['summary']['outcomes']==dict(Passed=21740,Failed=14) and len(debug['added'])==343
  current={key(r):r for r in debug['tests']};assert len(current)==21754 and all(current[key(r)]['outcome']==r['outcome'] for r in old['tests'])
  previous={key(r):r for r in old['nonpasses']};assert len(debug['nonpasses'])==14
  assert all(r['classification']=='RETAINED_IDENTICAL' and all(r[k]==previous[key(r)][k] for k in ['message_sha256','traceback_sha256']) for r in debug['nonpasses'])
  assert release['summary']['outcomes']==dict(Passed=1682,Failed=1) and len(release['added'])==343 and release['full_Release']=='NOT_RUN'
  disposition=read('build-and-tests/Release-trace-disposition-v1.json');assert disposition['message_exact'] and not disposition['waived'] and disposition['status']=='INHERITED_RELEASE_FAIL_REPRODUCED_UNWAIVED'
  abi=read('abi-cil/UI-and-attribution-v1.json');assert len(abi['rows'])==2 and abi['all_export_items_and_CIL_exact45'] and abi['XLL']=='NOT_BUILT_NOT_LOADED_NOT_AUTHORIZED'
  for c in ['Debug','Release']:
   row=next(r for r in abi['rows'] if r['configuration']==c);assert row['exports']==len(row['export_item_digests'])==268 and row['UI_Ribbon_RefEdit_fields_methods_IL_exact_pre_edit'] and not row['product_method_changes']
   compat=read('compatibility/'+c+'-v1.json');assert compat['cases']==29424 and compat['PRE_EDIT_FROZEN']==28966 and compat['LATE_ARCHIVED_ENTRY_SUPPLEMENT']==458 and compat['original_queries']==0
   assert all(r['original']['sha256']==r['current']['sha256'] and r['original']['bytes']==r['current']['bytes'] and r['exact'] for r in compat['results'])
   assert read('fixtures/'+c+'-44-v2.json')['cases']==591 and read('fixtures/'+c+'-progress-stub-v1.json')['cases']==72
  py=read('python/parent-event-reconciliation-v2.json');assert len(py['parents'])==len({p['id'] for p in py['parents']})==956 and collections.Counter(p['outcome'] for p in py['parents'])==py['summary']['parent_outcomes'] and not py['summary']['unresolved_diagnostic_events']
  d=read('python/dispositions-v1.json');assert d['unresolved']==0 and d['historical_failures_unwaived'] and d['historical04A08_scope_status']=='FAIL_UNWAIVED'
  assert read('matrix/serial-checks-v1.json')['exits']==[1,1,0]
  reg=read('register/interim-delta-v2.json');assert reg['counts']==dict(canonical_entries=1169,original_obligations=1514,restricted_controls=802,mappings=2967) and len(reg['canonical_ids'])==1169 and len(reg['crosswalk'])==2967
  assert len(reg['in_scope_original_controls'])==300 and all(not r['discharged'] and r['classification']=='WINDOWS_HOST_NATIVE_PENDING' and r['canonical_id'] for r in reg['in_scope_original_controls'])
  assert reg['original_controls_discharged']==reg['original_obligations_discharged']==reg['new_canonical_ids']==0 and reg['original48_exit_status'] is None
  inherited=json.loads(data['evidence/04A45/scope/original-obligations-v1.json']);assert len(inherited['rows'])==1514 and inherited['restricted_controls']==802 and inherited['discharged']==0
  preserve=read('preservation/readback-v1.json');assert preserve['entry_tracked']==15073 and preserve['checkpoint_copies']==468 and preserve['historical_paths']==8984 and preserve['raw40_paths']==1906 and preserve['historical_file_identity_drifts']==788 and preserve['continuous_physical_identity']=='NOT_VERIFIED' and not preserve['W3_union']['fresh_full_union_readback']
  gate=read('native-risk/dependency-v1.json');assert gate['approval']=='NOT_APPROVED' and gate['live_excel']=='NOT_RUN' and gate['actual_hwnd_pid_loaded_xll']=='NOT_VERIFIED' and not gate['MayRunNative'] and gate['original_control_discharge']==0
  privacy=read('privacy/publication-policy-v1.json');assert privacy['required_scanners']==['13','14'] and privacy['full_candidate_scope']=='ORIGINAL_UNCHANGED_PATHS_FUNCTIONS' and privacy['physical_scope']=='EVERY_MEMBER_INCLUDING_INDEX' and privacy['waivers']==0
  if receipt is not None:
   r=json.loads(Path(receipt).read_bytes());raw=Path(archive).read_bytes();assert r['archive']['sha256']==sha(raw) and r['archive']['bytes']==len(raw) and r['status']==STATUS
   assert set(data)=={p['path'] for p in r['physical_members']}
   for p in r['physical_members']:pin(p);assert p['crc32']==f'{zlib.crc32(data[p["path"]])&0xffffffff:08x}'
   assert len(r['full_actual_candidate_scans'])==len(r['physical_scans'])==2 and all(p['findings']==p['errors']==0 for p in r['full_actual_candidate_scans']+r['physical_scans'])
   assert len(r['primary_reports'])>=6 and len(r['adverse_controls'])>=25 and all(p['detected'] for p in r['adverse_controls'])
   for p in r['primary_reports']:pin(p)
  return dict(status='PASS_EXTERNAL_AND_SEMANTIC' if receipt else 'PASS_STRUCTURAL_AND_SEMANTIC',members=len(data),readiness=STATUS,native='NOT_RUN',effective_only_with_matching_external_receipt=True)
def adverse(archive):
 with zipfile.ZipFile(archive) as z:original={n:z.read(n) for n in z.namelist()}
 def change(f,p,action):d=json.loads(f[PREFIX+p]);action(d);f[PREFIX+p]=json.dumps(d).encode()
 mutations={
 'fake_native_PASS':('final/readiness-v1.json',lambda d:d.__setitem__('live_native','PASS')),
 'forged_readiness':('final/readiness-v1.json',lambda d:d.__setitem__('status','READY_FULL_NATIVE')),
 'wrong_Git_provenance':('entry/authentication-v1.json',lambda d:d.__setitem__('main',PRIOR)),
 'skipped_grouped_handler':('scope/family-review-v2.json',lambda d:d['members'].pop()),
 'skipped_normality_handler':('scope/family-review-v2.json',lambda d:d['owners'].remove('UibyID')),
 'duplicate_Handles_binding':('fixtures/scenario-contract-v2.json',lambda d:d['raised_bindings'].append(d['raised_bindings'][0])),
 'missing_sample_branch':('fixtures/scenario-contract-v2.json',lambda d:d.__setitem__('planning_branches',12)),
 'swapped_paired_row_source':('fixtures/scenario-contract-v2.json',lambda d:d['paired_source_roles'].reverse()),
 'wrong_nested_role':('fixtures/scenario-contract-v2.json',lambda d:d['nested_source_roles'][2].__setitem__('oneColumn',True)),
 'swapped_output_destinations':('fixtures/scenario-contract-v2.json',lambda d:d['categorical_output_priority'].reverse()),
 'wrong_default_availability':('fixtures/scenario-contract-v2.json',lambda d:d.__setitem__('Shapiro_minimum_source',3)),
 'suppressed_Python_scope_FAIL':('python/dispositions-v1.json',lambda d:d.__setitem__('historical04A08_scope_status','PASS')),
 'missing_previous_test':('final/accepted45-test-identities-v1.json',lambda d:d['tests'].pop()),
 'missing_Debug_failure':('build-and-tests/Debug-identities-v1.json',lambda d:d['nonpasses'].pop()),
 'changed_original_control':('register/interim-delta-v2.json',lambda d:d['in_scope_original_controls'][0].__setitem__('discharged',True)),
 'fake_full_Release':('build-and-tests/Release-identities-v1.json',lambda d:d.__setitem__('full_Release','PASS')),
 'Release_failure_waived':('build-and-tests/Release-trace-disposition-v1.json',lambda d:d.__setitem__('waived',True)),
 'reduced_privacy_candidate_scope':('privacy/publication-policy-v1.json',lambda d:d.__setitem__('full_candidate_scope','SELECTED')),
 'lost_frozen_case':('compatibility/Debug-v1.json',lambda d:d.__setitem__('cases',29423)),
 'altered_inspector_invocation':('final/inspector-invocation-v1.json',lambda d:d.__setitem__('mode','LIVE_ENUMERATOR')),
 'local_approval_boolean':('native-risk/dependency-v1.json',lambda d:d.__setitem__('MayRunNative',True)),
 'fake_scientific_PASS':('final/readiness-v1.json',lambda d:d['gates'].__setitem__('scientific','GOLDEN')),
 }
 labels=list(mutations)+['unsafe_name','extra','missing','casefold_collision','NFKC_collision','duplicate_member','changed_handler_source','wrong_option_enable_state','disabled_action_executes','overwritten_error_no_write_priority','stale_lease_allowed','private_token_leak'];results=[]
 for label in labels:
  f=dict(original);identifiers=[];reindex=True
  if label in mutations:change(f,*mutations[label])
  elif label=='unsafe_name':f['../unsafe.txt']=b'unsafe'
  elif label=='extra':f['extra.txt']=b'extra';reindex=False
  elif label=='missing':f.pop(PREFIX+'scope/family-review-v2.json');reindex=False
  elif label=='casefold_collision':f[(PREFIX+'review_reader.py').upper()]=f[PREFIX+'review_reader.py']
  elif label=='NFKC_collision':f[PREFIX+'\uff52eview_reader.py']=f[PREFIX+'review_reader.py']
  elif label=='changed_handler_source':
   p='BESHStatNG.Test/src/GuiWiring46Projections.vb';assert b' Handles ' in f[p];f[p]=f[p].replace(b' Handles ',b" 'missing Handles ",1)
  elif label=='wrong_option_enable_state':
   p='BESHStatNG.Test/src/GuiWiring46Fakes.vb';assert b'Enabled As Boolean = True' in f[p];f[p]=f[p].replace(b'Enabled As Boolean = True',b'Enabled As Boolean = False',1)
  elif label=='disabled_action_executes':
   p='BESHStatNG.Test/src/GuiWiring46Fakes.vb';assert b'If Not Enabled Then Return False' in f[p];f[p]=f[p].replace(b'If Not Enabled Then Return False',b'If False Then Return False',1)
  elif label=='stale_lease_allowed':
   p='BESHstatNG/src/UI/Refedit/RefEditEventLease.vb';a=b'If disposed OrElse faulted OrElse captured <> generation OrElse armed <> captured Then Return';assert a in f[p];f[p]=f[p].replace(a,b'If False Then Return',1)
  elif label=='overwritten_error_no_write_priority':
   p='BESHStatNG.Test/src/GuiWiring46ExecutionProjections.vb';assert b'If FakeMessages46.Record("Output range not empty! Overwrite?", vbYesNo + vbExclamation, "Overwrite?") = vbNo Then' in f[p]
   f[p]=f[p].replace(b'= vbNo Then',b'= vbYes Then')
  elif label=='private_token_leak':
   import uuid
   token='review'+uuid.uuid4().hex;identifiers=[token];f['identity-positive-control.txt']=token.encode()
  if reindex:f['.review-index.json']=json.dumps(dict(files=[dict(path=n,bytes=len(b),sha256=sha(b),crc32=f'{zlib.crc32(b)&0xffffffff:08x}') for n,b in sorted(f.items()) if n!='.review-index.json'])).encode()
  stream=io.BytesIO()
  with zipfile.ZipFile(stream,'w',zipfile.ZIP_STORED) as z:
   for n,b in f.items():z.writestr(n,b)
   if label=='duplicate_member':z.writestr('.review-index.json',f['.review-index.json'])
  stream.seek(0)
  try:verify(stream,private_identifiers=identifiers)
  except (AssertionError,KeyError,ValueError,zipfile.BadZipFile):results.append(dict(control=label,detected=True,method='Altered physical in-memory ZIP through standalone reader;index recomputed except explicit membership controls'))
  else:raise AssertionError('MISSED_ADVERSE:'+label)
 return results
if __name__=='__main__':print(json.dumps(verify(sys.argv[1],sys.argv[2] if len(sys.argv)>2 else None),sort_keys=True))
