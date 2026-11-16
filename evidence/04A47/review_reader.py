"""Standalone read-only47 delivery reader. No extraction,host or native operations."""
import collections,hashlib,io,json,re,sys,unicodedata,warnings,zipfile,zlib
from pathlib import Path
PREFIX='evidence/04A47/'
MAIN='8a8d4ee21f41f1a53072308d4a01faa0b20fdd5a'
PRIOR='ebc4a3e9df41ef8588aa7f6b25363f6d044c41a7'
STATUS='NOT_READY_04A47_SAFE_SOFTWARE'
GATES=dict(P='NO',N1='NO GO',N2='NO',C2='UNMET',C3='UNAPPROVED',scientific='NOT_GOLDEN',next='04A48_NOT_AUTHORIZED')
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
   value=unicodedata.normalize('NFKC',n+'\n'+b.decode('utf-8-sig')).casefold()
   for token in private_identifiers:assert not re.search(r'(?<!\w)'+re.escape(unicodedata.normalize('NFKC',token).casefold())+r'(?!\w)',value),'PRIVATE_IDENTITY_POSITIVE_CONTROL'
  index=json.loads(data['.review-index.json'])['files'];assert len(index)==len({r['path'] for r in index}) and set(data)=={r['path'] for r in index}|{'.review-index.json'}
  for r in index:pin(r);assert r['crc32']==f'{zlib.crc32(data[r["path"]])&0xffffffff:08x}'
  state=read('final/readiness-v1.json');assert state['status']==STATUS and state['gates']==GATES and state['live_native']=='NOT_RUN' and state['actual_HWND_PID_XLL']=='NOT_VERIFIED' and state['approval']=='NOT_APPROVED' and not state['native_adapter'] and state['original_controls_discharged']==0
  assert state['blockers']==['MISSING_HISTORICAL_SPC23_COMPARISON_PAYLOAD']
  auth=read('entry/authentication-v1.json');assert auth['HEAD']==auth['main']==auth['merge_base']==MAIN and auth['prior']==PRIOR and auth['branch']=='codex/04A-47' and auth['staged_empty'] and not auth['preexisting_owner_edits']
  assert sha(data[PREFIX+'entry/accepted46-receipt.review.json'])=='cbac245447da540bfbd905ac123d40710b12a02af7a99351ce122de549d93ec1'
  accepted=read('entry/accepted46-receipt.review.json');assert accepted['archive']['bytes']==4500679 and accepted['archive']['sha256']=='7da236f21672a40c027f9877587a4e1fa99f72d39c53421d9b24595442964b9b'
  freeze=read('matrix/source-freeze-v1.json');assert not freeze['production_changed'] and len(freeze['new_tests'])==6 and len(freeze['source_delta'])==1 and freeze['new_passing_identities']==141
  for r in freeze['new_tests']+[r['after'] for r in freeze['source_delta']]:pin(r)
  source=read('final/source-review-v1.json')
  for r in source['shipping_sources']:pin(r);assert r['sha256']==source['entry_source_hashes'][r['path']]
  code='\n'.join(data['BESHStatNG.Test/src/GuiWiring47Projection'+v+'.vb'].decode('utf-8-sig') for v in ['A','B'])
  selected=[];bindings=[]
  for v in ['a','b']:
   projection=read('fixtures/projections-'+v+'-v2.json');selected+=projection['members'];bindings+=projection['bindings']
   for m in projection['members']:
    pin(m['source']);body=m['original_text'];assert sha(body.encode())==m['source_text_sha256'] and body in data[m['source']['path']].decode('utf-8-sig').replace('\r\n','\n')
    for a,b in m['transforms']:body=body.replace(a,b)
    assert body in code,'ERASED_OR_CHANGED_SHIPPING_METHOD'
  assert len(selected)==101 and len(bindings)==53 and len({(b['owner'],b['handler'],b['control'],b['event']) for b in bindings})==53
  family=read('scope/family-review-v3.json');assert len(family['owners'])==10 and len(family['members'])==863 and len(family['fields'])==1246 and len(family['edges'])==10394 and family['bindings']==bindings and family['raised_binding_cases']==53 and family['actual_live_events']==family['original_discharges']==0
  assert family['counts']==dict(OFFLINE_FAKE_EXECUTED=101,SOURCE_AND_IL_ONLY=1,RESTRICTED_SCIENTIFIC_OR_NATIVE_CORRECTION=5,WINDOWS_HOST_NATIVE_PENDING=756)
  orphan=read('scope/orphan-method-v1.json');assert not orphan['has_control'] and not orphan['has_source_binding'] and not orphan['has_compiled_static_caller'] and orphan['classification']=='SOURCE_AND_IL_ONLY'
  assert collections.Counter(r[family['member_columns'].index('classification')] for r in family['members'])==family['counts']
  for row in family['members']:
   m=dict(zip(family['member_columns'],row));raw=data[m['path']];assert sha(raw)==m['source_sha256'];body='\n'.join(raw.decode('utf-8-sig').splitlines()[m['start']-1:m['end']]);assert sha(body.encode())==m['normalized_line_body_sha256']
  compiled=read('scope/compiled-members-v1.json');assert compiled['mode']=='REFLECTION_METADATA_IL_ONLY_NO_INITIALIZERS_OR_FORM_CONSTRUCTORS' and len(compiled['rows'])==2
  for c in compiled['rows']:
   owners={r['owner']:r for r in c['owners']};assert len(owners)==10
   for b in bindings:assert any(m['name']==b['handler'] for m in owners['BESHStatNG.'+b['owner']]['methods'])
  semantic=read('fixtures/semantic-contract-v1.json')
  expected=dict(lm_requested_alpha=0.1,lm_shipping_effective_alpha=0.05,covariance_Data_header_substitution='GLM_OR_NB2_WITH_COVARIANCE_AND_INTERCEPT_ONLY;LM_UNCHANGED',constant_binomial='ACTIVE_CORE_NOT_CONVERGED_NULL_FITTED_AND_RESIDUALS_NO_FABRICATED_REPORT',C09_strict_guard='UNAPPLIED_RESTRICTED',C12_fitted_family_guard='UNAPPLIED_RESTRICTED',gee_cluster_visit_rows='CURRENT_ORDER_PRESERVED_NOT_NATIVE_PROVENANCE',cox_roles=['duration','censor','group','stratum'],spc_points=2,spc_source_rows=[101,105,118,131],psm_RunPSM_reentrant_snapshot='RESTRICTED_NOT_FIXED',session_cache='GLOBAL_NOT_WORKBOOK_ISOLATED',mid_fit_interruptibility='NOT_PROVEN_BOUNDARY_ONLY',completed_write_priority='COMPLETED_WRITE_WINS_LATE_CANCEL',MayRunNative=False,actual_loaded_XLL='NOT_VERIFIED',native='NOT_RUN',scientific='NOT_GOLDEN')
  assert all(semantic[k]==v for k,v in expected.items())
  observer=read('fixtures/gee-observer-v2.json');assert observer['case_count_per_configuration']==6 and observer['new_managed_identities']==0 and not observer['frozen_test_project_changed'] and observer['results'][0]['rows']==observer['results'][1]['rows']
  assert sum(r['shippingException']=='System.ArgumentException' for r in observer['results'][0]['rows'])==3
  helpers=read('fixtures/helpers-observer-v2.json');assert helpers['cases_per_configuration']==141 and helpers['previously_source_only_selected_methods_executed']==11 and helpers['new_managed_identities']==0 and not helpers['frozen_test_project_changed'] and helpers['results'][0]['rows']==helpers['results'][1]['rows']
  assert any(r.get('rotation')=='Promax' and r['powerEnabled'] for r in helpers['results'][0]['rows'])
  assert 'If Not Enabled Then Return False' in data['BESHStatNG.Test/src/GuiWiring46Fakes.vb'].decode() and 'Return False' in data['BESHStatNG.Test/src/WindowsHarness45.vb'].decode()
  fakes=data['BESHStatNG.Test/src/GuiWiring47Fakes.vb'].decode();assert 'HOST_PENDING_MMRM_REFRESH' in fakes and not re.search(r'DllImport|Process\.Start|ExcelDnaUtil|System\.Windows\.Forms|UIAutomation|GetWindowThreadProcessId',fakes)
  old=read('final/accepted46-test-identities-v1.json');debug=read('build-and-tests/Debug-identities-v1.json');release=read('build-and-tests/Release-identities-v1.json');key=lambda r:(r['suite'],r['identity_sha256'],r['occurrence'])
  assert len(old['tests'])==21754 and old['historical_scope_status']=='FAIL_UNWAIVED' and len(debug['tests'])==21895 and debug['summary']['outcomes']==dict(Passed=21881,Failed=14) and len(debug['added'])==141
  current={key(r):r for r in debug['tests']};assert len(current)==21895 and all(current[key(r)]['outcome']==r['outcome'] for r in old['tests'])
  previous={key(r):r for r in old['nonpasses']};assert len(debug['nonpasses'])==14 and all(r['classification']=='RETAINED_IDENTICAL' and all(r[k]==previous[key(r)][k] for k in ['message_sha256','traceback_sha256']) for r in debug['nonpasses'])
  assert release['summary']['outcomes']==dict(Passed=15885,Failed=10) and len(release['added'])==141 and release['full_Release']=='NOT_RUN'
  disposition=read('build-and-tests/Release-trace-disposition-v2.json');assert disposition['failures']==10 and disposition['all_messages_exact'] and disposition['all_optimized_traces_exact'] and not disposition['waived'] and all(not r['waived'] for r in disposition['nonpasses'])
  abi=read('abi-cil/UI-and-attribution-v1.json');assert len(abi['rows'])==2 and abi['all_export_items_and_CIL_exact45'] and abi['XLL']=='NOT_BUILT_NOT_LOADED_NOT_AUTHORIZED'
  assert read('abi-cil/accepted46-exact-v1.json')['all268_items_exact46']
  for c in ['Debug','Release']:
   row=next(r for r in abi['rows'] if r['configuration']==c);assert row['exports']==len(row['export_item_digests'])==268 and row['UI_Ribbon_RefEdit_fields_methods_IL_exact_pre_edit'] and not row['product_method_changes']
   compat=read('compatibility/'+c+'-v1.json');assert compat['cases']==29424 and compat['PRE_EDIT_FROZEN']==28966 and compat['LATE_ARCHIVED_ENTRY_SUPPLEMENT']==458 and compat['original_queries']==0
   assert all(r['original']['sha256']==r['current']['sha256'] and r['original']['bytes']==r['current']['bytes'] and r['exact'] for r in compat['results'])
   assert read('fixtures/'+c+'-44-v2.json')['cases']==591 and read('fixtures/'+c+'-progress-stub-v1.json')['cases']==72
  py=read('python/parent-event-reconciliation-v2.json');assert len(py['parents'])==len({p['id'] for p in py['parents']})==956 and collections.Counter(p['outcome'] for p in py['parents'])==py['summary']['parent_outcomes'] and not py['summary']['unresolved_diagnostic_events']
  d=read('python/dispositions-v1.json');assert d['unresolved']==0 and d['historical_failures_unwaived'] and d['historical04A08_scope_status']=='FAIL_UNWAIVED'
  assert py['summary']['parent_outcomes']==dict(PASS=833,FAIL=110,ERROR=13) and len(d['changed_parents'])==2 and d['readiness_blocker']=='MISSING_HISTORICAL_SPC23_COMPARISON_PAYLOAD'
  assert read('matrix/serial-checks-v1.json')['exits']==[1,1,0]
  reg=read('register/interim-delta-v2.json');assert reg['counts']==dict(canonical_entries=1169,original_obligations=1514,restricted_controls=802,mappings=2967) and len(reg['canonical_ids'])==1169 and len(reg['crosswalk'])==2967
  assert len(reg['in_scope_original_controls'])==586 and all(not r['discharged'] and r['classification']=='WINDOWS_HOST_NATIVE_PENDING' and r['canonical_id'] for r in reg['in_scope_original_controls'])
  assert reg['original_controls_discharged']==reg['original_obligations_discharged']==reg['new_canonical_ids']==0 and reg['original48_exit_status'] is None
  preserve=read('preservation/readback-v1.json');assert preserve['entry_tracked']==15224 and preserve['checkpoint_copies']==468 and preserve['historical_paths']==8984 and preserve['raw40_paths']==1906 and preserve['historical_file_identity_drifts']==788 and preserve['continuous_physical_identity']=='NOT_VERIFIED' and not preserve['W3_union']['fresh_full_union_readback']
  newline=read('entry/generated-newline-transition-v1.json');assert newline['before_bytes']==224 and newline['current_bytes']==219 and newline['normalized_bytes_exact'] and newline['version_and_BuildDate_unchanged'] and not newline['phase47_edit']
  missing=read('preservation/missing-fixture-search-v1.json');assert missing['missing_in_current_repository'] and not missing['restored'] and not missing['matched_members'] and missing['expected']['bytes']==57758738 and missing['expected']['sha256']=='7f20addb9177a48e95b81330aa55acccde4bb6ed8b808cdbad42e09f2b790643'
  gate=read('native-risk/dependency-v1.json');assert gate['approval']=='NOT_APPROVED' and gate['live_excel']=='NOT_RUN' and gate['actual_hwnd_pid_loaded_xll']=='NOT_VERIFIED' and not gate['MayRunNative'] and gate['original_control_discharge']==0
  invoke=read('final/inspector-invocation-v1.json');assert invoke['mode']=='REFLECTION_ONLY_METADATA_AND_IL_NOT_RUNTIME_BINDING' and not invoke['native_or_product_initializers_invoked']
  privacy=read('privacy/publication-policy-v1.json');assert privacy['required_scanners']==['13','14'] and privacy['full_candidate_scope']=='ORIGINAL_UNCHANGED_PATHS_FUNCTIONS' and privacy['physical_scope']=='EVERY_MEMBER_INCLUDING_INDEX' and privacy['waivers']==0
  if receipt is not None:
   r=json.loads(Path(receipt).read_bytes());raw=Path(archive).read_bytes();assert r['archive']['sha256']==sha(raw) and r['archive']['bytes']==len(raw) and r['status']==STATUS and set(data)=={p['path'] for p in r['physical_members']}
   for p in r['physical_members']:pin(p);assert p['crc32']==f'{zlib.crc32(data[p["path"]])&0xffffffff:08x}'
   assert len(r['full_actual_candidate_scans'])==len(r['physical_scans'])==2 and all(p['findings']==p['errors']==0 for p in r['full_actual_candidate_scans']+r['physical_scans'])
   assert len(r['primary_reports'])>=6 and len(r['adverse_controls'])>=40 and all(p['detected'] for p in r['adverse_controls'])
   for p in r['primary_reports']:pin(p)
  return dict(status='PASS_EXTERNAL_AND_SEMANTIC' if receipt else 'PASS_STRUCTURAL_AND_SEMANTIC',members=len(data),readiness=STATUS,native='NOT_RUN',effective_only_with_matching_external_receipt=True)
def adverse(archive):
 with zipfile.ZipFile(archive) as z:original={n:z.read(n) for n in z.namelist()}
 def change(f,p,action):d=json.loads(f[PREFIX+p]);action(d);f[PREFIX+p]=json.dumps(d).encode()
 mutations={
 'forged_READY_despite_missing_payload':('final/readiness-v1.json',lambda d:d.__setitem__('status','READY_FOR_OWNER_04A47_SAFE_LONGRUN_GUI_WIRING_REVIEW__NATIVE_GATE_NOT_AUTHORIZED')),
 'fake_native_PASS':('final/readiness-v1.json',lambda d:d.__setitem__('live_native','PASS')),
 'orphan_PSM_interrupt_forged_wired':('scope/orphan-method-v1.json',lambda d:d.__setitem__('has_control',True)),
 'wrong_Git_provenance':('entry/authentication-v1.json',lambda d:d.__setitem__('main',PRIOR)),
 'wrong_LM_alpha':('fixtures/semantic-contract-v1.json',lambda d:d.__setitem__('lm_shipping_effective_alpha',0.1)),
 'altered_covariance_Data_header':('fixtures/semantic-contract-v1.json',lambda d:d.__setitem__('covariance_Data_header_substitution','CORRECTED')),
 'constant_binomial_bypass':('fixtures/semantic-contract-v1.json',lambda d:d.__setitem__('constant_binomial','SUCCESS')),
 'dormant_C09_claimed_active':('fixtures/semantic-contract-v1.json',lambda d:d.__setitem__('C09_strict_guard','ACTIVE')),
 'dormant_C12_claimed_active':('fixtures/semantic-contract-v1.json',lambda d:d.__setitem__('C12_fitted_family_guard','ACTIVE')),
 'changed_GEE_cluster_visit_axis':('fixtures/semantic-contract-v1.json',lambda d:d.__setitem__('gee_cluster_visit_rows','POSITIONALLY_EQUIVALENT')),
 'wrong_Cox_censor_role':('fixtures/semantic-contract-v1.json',lambda d:d['cox_roles'].reverse()),
 'SPC_logical_points_lost':('fixtures/semantic-contract-v1.json',lambda d:d.__setitem__('spc_points',4)),
 'stale_PSM_run_accepted':('fixtures/semantic-contract-v1.json',lambda d:d.__setitem__('psm_RunPSM_reentrant_snapshot','FIXED')),
 'false_mid_fit_cancel':('fixtures/semantic-contract-v1.json',lambda d:d.__setitem__('mid_fit_interruptibility','VERIFIED')),
 'completed_write_fault_priority_swapped':('fixtures/semantic-contract-v1.json',lambda d:d.__setitem__('completed_write_priority','CANCEL_WINS')),
 'fake_workbook_isolation':('fixtures/semantic-contract-v1.json',lambda d:d.__setitem__('session_cache','WORKBOOK_ISOLATED')),
 'duplicate_Handles_subscriber':('fixtures/projections-a-v2.json',lambda d:d['bindings'].append(d['bindings'][0])),
 'missing_previous_test':('final/accepted46-test-identities-v1.json',lambda d:d['tests'].pop()),
 'missing_inherited_FAIL':('build-and-tests/Debug-identities-v1.json',lambda d:d['nonpasses'].pop()),
 'suppressed_scope_FAIL':('python/dispositions-v1.json',lambda d:d.__setitem__('historical04A08_scope_status','PASS')),
 'forged_compiled_member':('scope/compiled-members-v1.json',lambda d:d['rows'][0]['owners'][0].__setitem__('methods',[])),
 'stale_loaded_XLL_claim':('fixtures/semantic-contract-v1.json',lambda d:d.__setitem__('actual_loaded_XLL','VERIFIED')),
 'fake_native_approval':('native-risk/dependency-v1.json',lambda d:d.__setitem__('MayRunNative',True)),
 'fake_scientific_PASS':('fixtures/semantic-contract-v1.json',lambda d:d.__setitem__('scientific','GOLDEN')),
 'changed_export_metadata':('abi-cil/UI-and-attribution-v1.json',lambda d:d['rows'][0].__setitem__('exports',267)),
 'compatibility_corpus_deleted':('compatibility/Debug-v1.json',lambda d:d.__setitem__('cases',29423)),
 'privacy_scope_reduced':('privacy/publication-policy-v1.json',lambda d:d.__setitem__('full_candidate_scope','SELECTED')),
 'unauthorized_issue_discharge':('register/interim-delta-v2.json',lambda d:d['in_scope_original_controls'][0].__setitem__('discharged',True)),
 'fake_full_Release':('build-and-tests/Release-identities-v1.json',lambda d:d.__setitem__('full_Release','PASS')),
 'waived_Release_failure':('build-and-tests/Release-trace-disposition-v2.json',lambda d:d.__setitem__('waived',True)),
 'altered_inspector_invocation':('final/inspector-invocation-v1.json',lambda d:d.__setitem__('mode','LIVE_ENUMERATOR')),
 }
 for label,owner in [('skipped_GEE_handler','Ui13GEE'),('skipped_mixed_handler','Ui18MMRM'),('skipped_PSM_handler','Ui20PropensityScoreMatching'),('skipped_SPC_handler','Ui21ControlCharts'),('skipped_Cox_handler','Ui4Cox')]:
  path='fixtures/projections-a-v2.json' if owner in ['Ui13GEE','Ui18MMRM'] else 'fixtures/projections-b-v2.json'
  mutations[label]=(path,lambda d,owner=owner:d.__setitem__('bindings',[b for b in d['bindings'] if b['owner']!=owner]))
 labels=list(mutations)+['skipped_Help_handler','inverted_Run_enable','source_method_erased','forged_source_hash','disabled_action_executes','unsafe_name','extra','missing','casefold_collision','NFKC_collision','duplicate_member','private_token_leak'];results=[]
 for label in labels:
  f=dict(original);identifiers=[];reindex=True
  if label in mutations:change(f,*mutations[label])
  elif label=='skipped_Help_handler':f['BESHStatNG.Test/src/GuiWiring47ContractTests.vb']=f['BESHStatNG.Test/src/GuiWiring47ContractTests.vb'].replace(b'HelpIntentResolvesMetadataOnlyAndUnknownDestinationFailsClosed',b'ErasedHelpIntent')
  elif label=='inverted_Run_enable':f['BESHStatNG.Test/src/GuiWiring47ProjectionB.vb']=f['BESHStatNG.Test/src/GuiWiring47ProjectionB.vb'].replace(b'Me.btCalculate.Enabled = False',b'Me.btCalculate.Enabled = True')
  elif label=='source_method_erased':
   p='BESHStatNG.Test/src/GuiWiring47ProjectionA.vb';target=b'RefreshLinkOptionsForSelectedFamily(GetCanonicalLinkFromDisplayName(Me.cbFamily.SelectedItem.ToString()))';assert target in f[p];f[p]=f[p].replace(target,b"'erased source call",1)
  elif label=='forged_source_hash':change(f,'final/source-review-v1.json',lambda d:d['shipping_sources'][0].__setitem__('sha256','0'*64))
  elif label=='disabled_action_executes':f['BESHStatNG.Test/src/GuiWiring46Fakes.vb']=f['BESHStatNG.Test/src/GuiWiring46Fakes.vb'].replace(b'If Not Enabled Then Return False',b'If False Then Return False')
  elif label=='unsafe_name':f['../unsafe.txt']=b'unsafe'
  elif label=='extra':f['extra.txt']=b'extra';reindex=False
  elif label=='missing':f.pop(PREFIX+'scope/family-review-v3.json');reindex=False
  elif label=='casefold_collision':f[(PREFIX+'review_reader.py').upper()]=f[PREFIX+'review_reader.py']
  elif label=='NFKC_collision':f[PREFIX+'\uff52eview_reader.py']=f[PREFIX+'review_reader.py']
  elif label=='private_token_leak':
   import uuid
   token='review'+uuid.uuid4().hex;identifiers=[token];f['identity-positive-control.txt']=token.encode()
  if reindex:f['.review-index.json']=json.dumps(dict(files=[dict(path=n,bytes=len(b),sha256=sha(b),crc32=f'{zlib.crc32(b)&0xffffffff:08x}') for n,b in sorted(f.items()) if n!='.review-index.json'])).encode()
  stream=io.BytesIO()
  with warnings.catch_warnings():
   warnings.filterwarnings('ignore',message='Duplicate name:')
   with zipfile.ZipFile(stream,'w',zipfile.ZIP_STORED) as z:
    for n,b in f.items():z.writestr(n,b)
    if label=='duplicate_member':z.writestr('.review-index.json',f['.review-index.json'])
  stream.seek(0)
  try:verify(stream,private_identifiers=identifiers)
  except (AssertionError,KeyError,ValueError,zipfile.BadZipFile):results.append(dict(control=label,detected=True,method='Altered physical in-memory ZIP through standalone reader;index recomputed except explicit membership controls'))
  else:raise AssertionError('MISSED_ADVERSE:'+label)
 return results
if __name__=='__main__':print(json.dumps(verify(sys.argv[1],sys.argv[2] if len(sys.argv)>2 else None),sort_keys=True))
