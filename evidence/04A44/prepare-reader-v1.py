from common44 import *
s=(ROOT/'evidence/04A43/review_reader.py').read_text().replace('04A43','04A44').replace('04A-43','04A-44').replace('04A44_NOT_AUTHORIZED','04A45_NOT_AUTHORIZED').replace("MAIN='3955a6e9142256df6bc1fd53a924010c58367b51'","MAIN='243b6dcd654767ffedb8fe3643b310c0c56cd6ac'")
s=s.replace('phase04A44_authorized','phase04A45_authorized').replace("len(source['source_delta'])==4","len(source['source_delta'])==7").replace('len(preimages)==4','len(preimages)==7').replace("plan['revision']=='2.20'","plan['revision']=='2.21'")
start=s.index("  matrix=read('scope/cache-export-matrix-v1.json')");end=s.index("  debug=read(",start)
s=s[:start]+'''  matrix=read('scope/member-ownership-v1.json');assert len(matrix['members'])==matrix['count']==276 and matrix['core_algorithms_unchanged'] and not matrix['Windows_store_UI_Help_updater_actions_changed'] and not matrix['new_capability_claims']
  boundary=read('scope/host-boundary-v1.json');assert boundary['Common_target']=='netstandard2.0' and not boundary['Core_algorithms_changed'] and not boundary['XML_store_schema_changed'] and not boundary['Office_DTO_JS_changed'] and not boundary['Help_generated_metadata_allowlist_resolver_changed'] and boundary['native']=='NOT_RUN'
  common=z.read(source['new_production'][0]['path']).decode('utf-8-sig');assert 'Option Strict On' in common and not re.search(r'(?i)ConcurrentDictionary|ExcelDna|System\\.Windows|System\\.IO|Microsoft\\.VisualBasic|HttpClient|Registry|Process\\.Start|DoEvents',common)
  refs=read('abi-cil/compiled-host-boundary-v1.json');assert len(refs['rows'])==4 and all(r['exact_entry_references'] and not r['forbidden_host_references'] for r in refs['rows'])
  delta=read('register/issue-delta-v1.json');assert delta['counts']==dict(canonical_entries=1169,original_obligations=1514,restricted_controls=802,mappings=2967) and len(delta['issues'])==4
  assert all(delta[k]==0 for k in ['original_obligations_discharged','original_controls_discharged','new_canonical_ids','scientific_corrections_activated','native_corrections_activated'])
  assert all(not r['scientific_correction'] and not r['native_correction'] and r['original48_exit_status'] is None for r in delta['issues'])
'''+s[end:]
s=s.replace('21232','21266').replace('21218','21252').replace("len(debug['added'])==55","len(debug['added'])==34").replace('dict(Passed=373)','dict(Passed=1041)')
start=s.index("  py=read(");end=s.index("  privacy=read(",start)
s=s[:start]+'''  py=read('python/parent-event-reconciliation-v2.json');assert len(py['parents'])==len({p['id'] for p in py['parents']})==956
  assert collections.Counter(p['outcome'] for p in py['parents'])==py['summary']['parent_outcomes'] and not py['summary']['unresolved_diagnostic_events']
  disp=read('python/diagnostic-dispositions-v1.json');assert disp['unresolved']==0 and disp['historical_outcomes_retained'] and disp['every_parent_transition_explained']
  guards=read('python/source-guard-dispositions-v1.json')['guards'];assert len(guards)==3 and all(r['historical43_status']=='FAIL' and not r['waived'] for r in guards)
'''+s[end:]
s=s.replace("exports['exact_accepted42']","exports['exact_accepted43']")
start=s.index("   f=read('fixtures/'+config+'-43-v1.json')");end=s.index("  pr=read(",start)
s=s[:start]+'''   f=read('fixtures/'+config+'-43-v2.json');assert f['operations']==1118 and f['graphs']==36 and f['families']==18 and f['generated_reference_bijection_validated'] and f['nonvolatile_typed_outputs_errors_shapes_exact']
   f=read('fixtures/'+config+'-43-races-v2.json');assert f['before']['rows']==f['after']['rows']==864 and f['identities_exact'] and f['allowed_outcome_invariants_pass'] and not f['fixed_schedule_equality_claimed'] and f['live_Excel']=='NOT_RUN'
   f=read('fixtures/'+config+'-43-policy-v2.json');assert f['cases']==15 and f['exact']
   f=read('fixtures/'+config+'-44-v2.json');assert f['cases']==591 and f['exact'] and not f['excluded_runtime_fields']
   f=read('fixtures/'+config+'-progress-stub-v1.json');assert f['cases']==72 and f['exact'] and f['expected_event_order_verified'] and f['live_Excel']=='NOT_RUN' and 'SIMULATION' in f['kind']
  current=read('fixtures/current-main-review-v1.json');assert current['cases']==len(current['rows'])==591 and not current['excluded_fields']
'''+s[end:]
s=s.replace("pr['entry_tracked_files']==14634","pr['entry_tracked_files']==14769").replace("and pr['historical_source_entry_identities']==1353","and pr['raw43_prior_pins_exact']==8160 and pr['historical_source_entry_identities']==1353")
start=s.index(' actions={');end=s.index(" labels=list(actions)",start)
s=s[:start]+''' actions={
 'settings_behavior_substitution':('fixtures/Debug-44-v2.json',lambda d:d.__setitem__('exact',False)),
 'fake_native_progress':('fixtures/Debug-progress-stub-v1.json',lambda d:d.__setitem__('live_Excel','PASS')),
 'new_export':('abi-cil/exports-review-v1.json',lambda d:d['exports'].append(d['exports'][0])),
 'lost_historic_case':('compatibility/Debug-v1.json',lambda d:d.__setitem__('cases',29423)),
 'greenwash_guard':('python/source-guard-dispositions-v1.json',lambda d:d['guards'][0].__setitem__('historical43_status','PASS')),
 'historical_FAIL_suppressed':('build-and-tests/Debug-identities-v1.json',lambda d:d['nonpasses'].pop()),
 'privacy_scope_narrowing':('privacy/repository-candidate-v1.json',lambda d:d['reports'][0]['members'].pop()),
 'fake_scientific_PASS':('final/readiness-postseal-v1.json',lambda d:d['gates'].__setitem__('scientific','GOLDEN')),
 'fake_full_Release':('build-and-tests/Release-identities-v1.json',lambda d:d.__setitem__('full_Release','PASS')),
 'next_batch':('final/readiness-postseal-v1.json',lambda d:d.__setitem__('phase04A45_authorized',True)),
 'obligation_discharge':('register/issue-delta-v1.json',lambda d:d.__setitem__('original_obligations_discharged',1)),
 'host_IO_claim':('scope/host-boundary-v1.json',lambda d:d.__setitem__('XML_store_schema_changed',True)),
 'fake_Office_wiring':('scope/host-boundary-v1.json',lambda d:d.__setitem__('Office_DTO_JS_changed',True)),
 'compiled_host_reference':('abi-cil/compiled-host-boundary-v1.json',lambda d:d['rows'][0]['forbidden_host_references'].append('System.Windows.Forms')),
 }
'''+s[end:]
s=s.replace("n='BESHStat.Excel.Common/src/Analysis/UdfModelHandlePolicy.vb'","n='BESHStat.Excel.Common/src/Infrastructure/NumberTextPolicy.vb'").replace("['matrix/source-freeze-v1.json','scope/portable-ownership-lock-v1.json']","['matrix/source-freeze-v1.json']")
with (HERE/'review_reader.py').open('x',encoding='utf-8',newline='\n') as f:f.write(s)
s=(ROOT/'evidence/04A43/publication_helpers.py').read_text().replace('common43','common44').replace('final43','final44').replace('04A43_ACCEPTANCE','04A44_ACCEPTANCE')
with (HERE/'publication_helpers.py').open('x',encoding='utf-8',newline='\n') as f:f.write(s)
