"""Standalone SHA/CRC, source and semantic reader. No extraction, native calls or tokens."""
import collections,copy,hashlib,io,json,re,sys,unicodedata,zipfile,zlib
from pathlib import Path
PREFIX='evidence/04A44/'
STATUS='READY_FOR_OWNER_04A44_ACCEPTANCE_REVIEW_WITH_DOCUMENTED_RESTRICTIONS'
MAIN='243b6dcd654767ffedb8fe3643b310c0c56cd6ac'
GATES=dict(P='NO',N1='NO GO',N2='NO',C2='UNMET',C3='UNAPPROVED',scientific='NOT_GOLDEN',next='04A45_NOT_AUTHORIZED')
def sha(b):return hashlib.sha256(b).hexdigest()
def safe(n):
 assert n and not n.startswith('/') and chr(92) not in n and ':' not in n
 assert all(p not in ('','.','..') and not p.endswith((' ','.')) for p in n.split('/'))
 assert not re.search(r'(?i)(?:^|/)(?:con|prn|aux|nul|com[1-9]|lpt[1-9])(?:\.|/|$)',n)
def verify(archive,external=None):
 with zipfile.ZipFile(archive) as z:
  names=z.namelist();assert len(names)==len(set(names))==len({unicodedata.normalize('NFKC',n).casefold() for n in names})
  for n in names:safe(n)
  assert z.testzip() is None
  index=json.loads(z.read('.review-index.json'));files={r['path']:r for r in index['files']}
  assert len(files)==len(index['files']) and set(names)==set(files)|{'.review-index.json'}
  def pinned(r):
   data=z.read(r['path']);assert len(data)==r['bytes'] and sha(data)==r['sha256']
  for n,r in files.items():
   pinned(r);i=z.getinfo(n);assert not i.is_dir() and ((i.external_attr>>16)&0o170000)!=0o120000
   assert f'{zlib.crc32(z.read(n))&0xffffffff:08x}'==r['crc32']==f'{i.CRC:08x}'
  read=lambda p:json.loads(z.read(PREFIX+p))
  state=read('final/readiness-postseal-v1.json');assert state['status']==STATUS and state['gates']==GATES and state['effective_only_with_matching_external_receipt']
  assert not state['owner_accepted'] and not state['merged'] and not state['phase04A45_authorized'] and state['scientific_corrections']==state['native_activations']==0
  assert state['cached_model_semantics_changed'] is False and state['live_Excel']=='NOT_RUN'
  auth=read('entry/review-authentication-v1.json');assert auth['HEAD']==auth['main']==auth['merge_base']==MAIN and auth['branch']=='codex/04A-44' and auth['raw40_exclusion_retained']
  source=read('matrix/source-freeze-v1.json');assert len(source['source_delta'])==7 and len(source['new_production'])==1 and len(source['new_tests'])==2
  for r in [source['plan'],source['plan_snapshot']]+source['new_production']+source['new_tests']+[r['after'] for r in source['source_delta']]:pinned(r)
  preimages=read('source/preimages-v1.json')['files'];assert len(preimages)==7
  for r in preimages:
   pinned(r['public_exact_source_copy']);assert r['public_exact_source_copy']['sha256']==r['original']['sha256']
  plan=read('planning/reconciliation-v1.json');assert len(plan['identities'])==len(set(plan['identities']))==226 and plan['revision']=='2.21' and not plan['charter_changed']
  matrix=read('scope/member-ownership-v1.json');assert len(matrix['members'])==matrix['count']==276 and matrix['core_algorithms_unchanged'] and not matrix['Windows_store_UI_Help_updater_actions_changed'] and not matrix['new_capability_claims']
  boundary=read('scope/host-boundary-v1.json');assert boundary['Common_target']=='netstandard2.0' and not boundary['Core_algorithms_changed'] and not boundary['XML_store_schema_changed'] and not boundary['Office_DTO_JS_changed'] and not boundary['Help_generated_metadata_allowlist_resolver_changed'] and boundary['native']=='NOT_RUN'
  common=z.read(source['new_production'][0]['path']).decode('utf-8-sig');assert 'Option Strict On' in common and not re.search(r'(?i)ConcurrentDictionary|ExcelDna|System\.Windows|System\.IO|Microsoft\.VisualBasic|HttpClient|Registry|Process\.Start|DoEvents',common)
  refs=read('abi-cil/compiled-host-boundary-v1.json');assert len(refs['rows'])==4 and all(r['exact_entry_references'] and not r['forbidden_host_references'] for r in refs['rows'])
  delta=read('register/issue-delta-v1.json');assert delta['counts']==dict(canonical_entries=1169,original_obligations=1514,restricted_controls=802,mappings=2967) and len(delta['issues'])==4
  assert all(delta[k]==0 for k in ['original_obligations_discharged','original_controls_discharged','new_canonical_ids','scientific_corrections_activated','native_corrections_activated'])
  assert all(not r['scientific_correction'] and not r['native_correction'] and r['original48_exit_status'] is None for r in delta['issues'])
  debug=read('build-and-tests/Debug-identities-v1.json');assert len(debug['tests'])==debug['summary']['identities']==21266 and debug['summary']['outcomes']==dict(Passed=21252,Failed=14)
  assert collections.Counter(r['outcome'] for r in debug['tests'])==debug['summary']['outcomes'] and not debug['summary']['missing'] and len(debug['added'])==34 and all(r['outcome']=='Passed' for r in debug['added'])
  assert len(debug['nonpasses'])==14 and all(r['classification']=='RETAINED_IDENTICAL' for r in debug['nonpasses'])
  release=read('build-and-tests/Release-identities-v1.json');assert release['full_Release']=='NOT_RUN' and release['summary']['outcomes']==dict(Passed=1041)
  py=read('python/parent-event-reconciliation-v2.json');assert len(py['parents'])==len({p['id'] for p in py['parents']})==956
  assert collections.Counter(p['outcome'] for p in py['parents'])==py['summary']['parent_outcomes'] and not py['summary']['unresolved_diagnostic_events']
  disp=read('python/diagnostic-dispositions-v1.json');assert disp['unresolved']==0 and disp['historical_outcomes_retained'] and disp['every_parent_transition_explained']
  guards=read('python/source-guard-dispositions-v1.json')['guards'];assert len(guards)==3 and all(r['historical43_status']=='FAIL' and not r['waived'] for r in guards)
  privacy=read('privacy/repository-candidate-v1.json');assert len(privacy['reports'])==2 and privacy['rules_ignore_and_scope_unchanged'] and privacy['waivers']==0
  for r in privacy['reports']:
   assert r['findings']==r['unresolved']==0 and r['candidates']==len(r['members']) and r['candidates']>100
   assert r['negative_control']['zero_findings'] and all(c['detected'] for c in r['identity_controls'])
  exports=read('abi-cil/exports-review-v1.json');assert len(exports['exports'])==268 and exports['exact_accepted43']
  for config in ['Debug','Release']:
   abi=read('abi-cil/'+config+'-proof-v1.json');assert abi['exports']==268 and abi['exact_export_order_names_signatures_attributes'] and not abi['unexpected'] and not abi['removed']
   compat=read('compatibility/'+config+'-v1.json');assert compat['cases']==29424 and compat['PRE_EDIT_FROZEN']==28966 and compat['LATE_ARCHIVED_ENTRY_SUPPLEMENT']==458 and compat['original_queries']==0 and compat['exact_typed_order_input_schema_identities'] and all(r['exact'] for r in compat['results'])
   for family,count in [('39',77),('41',390)]:f=read('fixtures/'+config+'-'+family+'-v1.json');assert f['cases']==count and f['exact']
   f=read('fixtures/'+config+'-40-v1.json');assert f['cases']==48 and f['statistical_input_exception_contracts_exact'] and len(f['elapsed_only_cases'])==4
   f=read('fixtures/'+config+'-42-v1.json');assert f['cases']==1802 and f['typed_statistical_shape_error_order_exact'] and len(f['only_excluded_runtime_fields'])==2
   f=read('fixtures/'+config+'-43-v2.json');assert f['operations']==1118 and f['graphs']==36 and f['families']==18 and f['generated_reference_bijection_validated'] and f['nonvolatile_typed_outputs_errors_shapes_exact']
   f=read('fixtures/'+config+'-43-races-v2.json');assert f['before']['rows']==f['after']['rows']==864 and f['identities_exact'] and f['allowed_outcome_invariants_pass'] and not f['fixed_schedule_equality_claimed'] and f['live_Excel']=='NOT_RUN'
   f=read('fixtures/'+config+'-43-policy-v2.json');assert f['cases']==15 and f['exact']
   f=read('fixtures/'+config+'-44-v2.json');assert f['cases']==591 and f['exact'] and not f['excluded_runtime_fields']
   f=read('fixtures/'+config+'-progress-stub-v1.json');assert f['cases']==72 and f['exact'] and f['expected_event_order_verified'] and f['live_Excel']=='NOT_RUN' and 'SIMULATION' in f['kind']
  current=read('fixtures/current-main-review-v1.json');assert current['cases']==len(current['rows'])==591 and not current['excluded_fields']
  pr=read('preservation/readback-v1.json');assert pr['entry_tracked_files']==14769 and pr['checkpoint']['files']==pr['checkpoint']['exact_original_copies']==468 and pr['checkpoint']['live_exact']==464 and pr['historical_protected']['paths']==8984 and pr['newer40_raw']['paths']==1906 and pr['raw41_prior_pins_exact']==6810 and pr['raw42_prior_pins_exact']==11457 and pr['raw43_prior_pins_exact']==8160 and pr['historical_source_entry_identities']==1353
  assert pr['continuous_physical_identity'].startswith('NOT_VERIFIED') and not pr['W3_union']['fresh_full_union_audit'] and pr['Node']['outcome']=='INHERITED_916_PASS;NOT_RERUN'
  for raw in ['evidence/04A40/baseline/Debug-build-v1.json','evidence/04A40/entry/tool-runtime-final-v1.json']:assert raw not in names
  assert read('matrix/serial-checks-v1.json')['exits']==[1,1,0]
  g=read('final/git-final-v1.json');assert g['staged_empty'] and g['diff_check_exit']==0 and g['HEAD']==g['main']==g['merge_base']==MAIN
  if external is not None:
   r=json.loads(Path(external).read_bytes());b=Path(archive).read_bytes();assert len(b)==r['archive']['bytes'] and sha(b)==r['archive']['sha256'] and r['status']==STATUS
   assert set(names)=={p['path'] for p in r['physical_members']}
   for p in r['physical_members']:pinned(p);assert p['crc32']==f'{zlib.crc32(z.read(p["path"]))&0xffffffff:08x}'
   assert len(r['physical_scans'])==len(r['full_actual_candidate_scans'])==2 and all(p['findings']==p['unresolved']==0 for p in r['physical_scans']+r['full_actual_candidate_scans'])
   assert len(r['primary_reports'])==6
   for p in r['primary_reports']:pinned(p)
   assert len(r['adverse_controls'])>=16 and all(p['detected'] for p in r['adverse_controls'])
  return dict(status='PASS_EXTERNAL_AND_SEMANTIC' if external else 'PASS_STRUCTURAL_AND_SEMANTIC',physical_members=len(names),SHA_size_CRC='PASS',normalized_names='PASS',missing_extra='NONE',readiness=STATUS,effective_only_with_matching_external_receipt=True)

def adverse(archive):
 with zipfile.ZipFile(archive) as z:original={n:z.read(n) for n in z.namelist()}
 def alter(f,p,change):d=json.loads(f[PREFIX+p]);change(d);f[PREFIX+p]=json.dumps(d).encode()
 actions={
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
 labels=list(actions)+['missing','extra','case_collision','unsafe','changed_preimage','Common_host_cache']
 controls=[]
 for label in labels:
  f=copy.deepcopy(original);reindex=True
  if label in actions:alter(f,*actions[label])
  elif label=='missing':f.pop(PREFIX+'final/readiness-postseal-v1.json');reindex=False
  elif label=='extra':f['extra.txt']=b'extra';reindex=False
  elif label=='case_collision':f[(PREFIX+'review_reader.py').upper()]=f[PREFIX+'review_reader.py'];reindex=False
  elif label=='unsafe':f['../unsafe.txt']=b'unsafe'
  elif label=='changed_preimage':n=next(n for n in f if '/source/preimages-v1/' in n);f[n]+=b'changed'
  elif label=='Common_host_cache':
   n='BESHStat.Excel.Common/src/Infrastructure/NumberTextPolicy.vb';f[n]+=b'\nImports System.Collections.Concurrent.ConcurrentDictionary\n'
   # Adjust source claims too: semantic dependency guard must reject independently of the hash guard.
   for p in ['matrix/source-freeze-v1.json']:
    d=json.loads(f[PREFIX+p])
    def update(x):
     if isinstance(x,dict):
      if x.get('path')==n and 'sha256' in x:x.update(bytes=len(f[n]),sha256=sha(f[n]))
      for v in x.values():update(v)
     elif isinstance(x,list):
      for v in x:update(v)
    update(d);f[PREFIX+p]=json.dumps(d).encode()
  if reindex:f['.review-index.json']=json.dumps(dict(files=[dict(path=n,bytes=len(b),sha256=sha(b),crc32=f'{zlib.crc32(b)&0xffffffff:08x}') for n,b in f.items() if n!='.review-index.json'])).encode()
  out=io.BytesIO()
  with zipfile.ZipFile(out,'w',compression=zipfile.ZIP_STORED) as z:
   for n,b in f.items():z.writestr(n,b)
  out.seek(0)
  try:verify(out)
  except (AssertionError,KeyError,ValueError,zipfile.BadZipFile):controls.append(dict(control=label,detected=True,method='Executed altered in-memory physical archive through independent reader'))
  else:raise AssertionError('MISSED_READER_CONTROL:'+label)
 return controls
if __name__=='__main__':print(json.dumps(verify(sys.argv[1],sys.argv[2] if len(sys.argv)>2 else None),sort_keys=True))
