"""Standalone SHA/CRC, source and semantic reader. No extraction, native calls or tokens."""
import collections,copy,hashlib,io,json,re,sys,unicodedata,zipfile,zlib
from pathlib import Path
PREFIX='evidence/04A43/'
STATUS='READY_FOR_OWNER_04A43_ACCEPTANCE_REVIEW_WITH_DOCUMENTED_RESTRICTIONS'
MAIN='3955a6e9142256df6bc1fd53a924010c58367b51'
GATES=dict(P='NO',N1='NO GO',N2='NO',C2='UNMET',C3='UNAPPROVED',scientific='NOT_GOLDEN',next='04A44_NOT_AUTHORIZED')
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
  assert not state['owner_accepted'] and not state['merged'] and not state['phase04A44_authorized'] and state['scientific_corrections']==state['native_activations']==0
  assert state['cached_model_semantics_changed'] is False and state['live_Excel']=='NOT_RUN'
  auth=read('entry/review-authentication-v1.json');assert auth['HEAD']==auth['main']==auth['merge_base']==MAIN and auth['branch']=='codex/04A-43' and auth['raw40_exclusion_retained']
  source=read('matrix/source-freeze-v1.json');assert len(source['source_delta'])==4 and len(source['new_production'])==1 and len(source['new_tests'])==2
  for r in [source['plan'],source['plan_snapshot']]+source['new_production']+source['new_tests']+[r['after'] for r in source['source_delta']]:pinned(r)
  preimages=read('source/preimages-v1.json')['files'];assert len(preimages)==4
  for r in preimages:
   pinned(r['public_exact_source_copy']);assert r['public_exact_source_copy']['sha256']==r['original']['sha256']
  plan=read('planning/reconciliation-v1.json');assert len(plan['identities'])==len(set(plan['identities']))==226 and plan['revision']=='2.20' and not plan['charter_changed']
  matrix=read('scope/cache-export-matrix-v1.json');assert len(matrix['families'])==matrix['cache_families']==18 and matrix['cache_modules']==12 and len(matrix['exports'])==matrix['handle_exports']==174 and matrix['total_exports']==268 and matrix['changed_public_parents']==8
  assert not matrix['model_storage_moved'] and not matrix['new_workbook_callbacks'] and not matrix['Core_scientific_changes'] and not matrix['public_cached_model_semantics_changed']
  assert all(r['exact'] for r in matrix['FIT_locks'])
  portable=read('scope/portable-ownership-lock-v1.json');assert portable['Common_target']=='netstandard2.0' and portable['Core_source_unchanged'] and not portable['host_dependencies_in_new_Common'] and portable['mutable_Common_cache_instances']==0 and not portable['existing_tests_modified']
  common=z.read(portable['new_Common_source']['path']).decode('utf-8-sig');assert 'Option Strict On' in common
  assert not re.search(r'(?i)ConcurrentDictionary|ExcelDna|System\.Windows\.Forms|System\.IO|Microsoft\.VisualBasic|Shared\s+(?:ReadOnly\s+)?\w+\s+As\s+.*Dictionary',common)
  delta=read('register/issue-delta-v1.json');assert delta['counts']==dict(canonical_entries=1169,original_obligations=1514,restricted_controls=802,mappings=2967) and len(delta['issues'])==3
  assert all(delta[k]==0 for k in ['original_obligations_discharged','original_controls_discharged','new_canonical_ids','scientific_corrections_activated','native_corrections_activated'])
  assert all(not r['scientific_correction_activated'] and not r['native_correction_activated'] and r['original48_exit_status'] is None for r in delta['issues'])
  debug=read('build-and-tests/Debug-identities-v1.json');assert len(debug['tests'])==debug['summary']['identities']==21232 and debug['summary']['outcomes']==dict(Passed=21218,Failed=14)
  assert collections.Counter(r['outcome'] for r in debug['tests'])==debug['summary']['outcomes'] and not debug['summary']['missing'] and len(debug['added'])==55 and all(r['outcome']=='Passed' for r in debug['added'])
  assert len(debug['nonpasses'])==14 and all(r['classification']=='RETAINED_IDENTICAL' for r in debug['nonpasses'])
  release=read('build-and-tests/Release-identities-v1.json');assert release['full_Release']=='NOT_RUN' and release['summary']['outcomes']==dict(Passed=373)
  py=read('python/parent-event-reconciliation-v2.json');assert len(py['parents'])==len({p['id'] for p in py['parents']})==956 and py['summary']['parent_outcomes']==dict(PASS=832,FAIL=113,ERROR=11)
  assert py['summary']['event_outcomes']==dict(FAIL=185,ERROR=11) and not py['summary']['unresolved_diagnostic_events'] and not py['summary']['changed_parents'] and len(py['events'])==196
  assert all(r['classification'] in ['RETAINED_IDENTICAL','RETAINED_CHANGED_MESSAGE'] for r in py['events'])
  disp=read('python/diagnostic-dispositions-v1.json');assert disp['unresolved']==0 and len(disp['rows'])==21 and disp['historical_outcomes_retained']
  guards=read('python/source-guard-dispositions-v1.json')['guards'];assert len(guards)==3 and all(r['current_status']=='FAIL' and not r['waived'] for r in guards)
  privacy=read('privacy/repository-candidate-v1.json');assert len(privacy['reports'])==2 and privacy['rules_ignore_and_scope_unchanged'] and privacy['waivers']==0
  for r in privacy['reports']:
   assert r['findings']==r['unresolved']==0 and r['candidates']==len(r['members']) and r['candidates']>100
   assert r['negative_control']['zero_findings'] and all(c['detected'] for c in r['identity_controls'])
  exports=read('abi-cil/exports-review-v1.json');assert len(exports['exports'])==268 and exports['exact_accepted42']
  for config in ['Debug','Release']:
   abi=read('abi-cil/'+config+'-proof-v1.json');assert abi['exports']==268 and abi['exact_export_order_names_signatures_attributes'] and not abi['unexpected'] and not abi['removed']
   compat=read('compatibility/'+config+'-v1.json');assert compat['cases']==29424 and compat['PRE_EDIT_FROZEN']==28966 and compat['LATE_ARCHIVED_ENTRY_SUPPLEMENT']==458 and compat['original_queries']==0 and compat['exact_typed_order_input_schema_identities'] and all(r['exact'] for r in compat['results'])
   for family,count in [('39',77),('41',390)]:f=read('fixtures/'+config+'-'+family+'-v1.json');assert f['cases']==count and f['exact']
   f=read('fixtures/'+config+'-40-v1.json');assert f['cases']==48 and f['statistical_input_exception_contracts_exact'] and len(f['elapsed_only_cases'])==4
   f=read('fixtures/'+config+'-42-v1.json');assert f['cases']==1802 and f['typed_statistical_shape_error_order_exact'] and len(f['only_excluded_runtime_fields'])==2
   f=read('fixtures/'+config+'-43-v1.json');assert f['operations']==1118 and f['graphs']==36 and f['families']==18 and f['generated_reference_bijection_validated'] and f['raw_tokens_grammar_uniqueness_and_object_association_verified'] and f['nonvolatile_typed_outputs_errors_shapes_exact']
   f=read('fixtures/'+config+'-43-races-v1.json');assert f['before']['rows']==f['after']['rows']==864 and f['identities_exact'] and f['allowed_outcome_invariants_pass'] and not f['fixed_schedule_equality_claimed'] and f['live_Excel']=='NOT_RUN'
   simulation=read('policy/'+config+'-simulation-v1.json')['data'];assert len(simulation['rows'])==15 and simulation['actualWindowsCallbacksInstalled']==0 and simulation['actualWorkbookAttribution']=='NOT_ESTABLISHED' and simulation['liveExcel']=='NOT_RUN'
   values={r['id']:r['actual'] for r in simulation['rows']};assert values['wrong-session']=='WrongVerifiedSession' and values['wrong-workbook']=='WrongVerifiedWorkbook' and values['unknown-workbook-close']=='NativeScopeNotVerified'
   assert values['reference-good'] is True and all(values[k] is False for k in ['reference-alias-substitution','reference-wrong-cache-family','reference-wrong-object'])
  relational=read('fixtures/relational-review-v1.json');assert len(relational['graphs'])==36 and len(relational['operations'])==1118 and relational['reference_graph_validated'] and not relational['model_data_published']
  for g in relational['graphs']:
   assert g['workbookScope']==g['sessionEpoch']=='NOT_ESTABLISHED' and g['liveExcel']=='NOT_RUN' and g['uniqueGeneratedTokens'] and g['distinctCachedObjects'] and g['queryABindsOwnObject'] and g['queryBBindsOwnObject']
   ordinals=[t['valueOrdinal'] for t in g['tokens']];assert len(ordinals)==len(set(ordinals))
   for t in g['tokens']:assert t['boundValueOrdinal']==t['valueOrdinal'] and t['token']==g['context']+'/'+t['symbol']
  pr=read('preservation/readback-v1.json');assert pr['entry_tracked_files']==14634 and pr['checkpoint']['files']==pr['checkpoint']['exact_original_copies']==468 and pr['checkpoint']['live_exact']==464 and pr['historical_protected']['paths']==8984 and pr['newer40_raw']['paths']==1906 and pr['raw41_prior_pins_exact']==6810 and pr['raw42_prior_pins_exact']==11457 and pr['historical_source_entry_identities']==1353
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
 'handle_alias_substitution':('fixtures/relational-review-v1.json',lambda d:d['graphs'][0]['tokens'][0].__setitem__('boundValueOrdinal',2)),
 'wrong_cache_family':('policy/Debug-simulation-v1.json',lambda d:next(r for r in d['data']['rows'] if r['id']=='reference-wrong-cache-family').__setitem__('actual',True)),
 'stale_verified_session':('policy/Debug-simulation-v1.json',lambda d:next(r for r in d['data']['rows'] if r['id']=='wrong-session').__setitem__('actual','Available')),
 'fake_recalc_workbook_scope':('fixtures/relational-review-v1.json',lambda d:d['graphs'][0].__setitem__('workbookScope','VERIFIED_LIVE')),
 'fake_native_callback':('policy/Debug-simulation-v1.json',lambda d:d['data'].__setitem__('actualWindowsCallbacksInstalled',1)),
 'new_export':('abi-cil/exports-review-v1.json',lambda d:d['exports'].append(d['exports'][0])),
 'lost_historic_case':('compatibility/Debug-v1.json',lambda d:d.__setitem__('cases',29423)),
 'greenwash_guard':('python/source-guard-dispositions-v1.json',lambda d:d['guards'][0].__setitem__('current_status','PASS')),
 'historical_FAIL_suppressed':('build-and-tests/Debug-identities-v1.json',lambda d:d['nonpasses'].pop()),
 'privacy_scope_narrowing':('privacy/repository-candidate-v1.json',lambda d:d['reports'][0]['members'].pop()),
 'fake_scientific_PASS':('final/readiness-postseal-v1.json',lambda d:d['gates'].__setitem__('scientific','GOLDEN')),
 'fake_full_Release':('build-and-tests/Release-identities-v1.json',lambda d:d.__setitem__('full_Release','PASS')),
 'next_batch':('final/readiness-postseal-v1.json',lambda d:d.__setitem__('phase04A44_authorized',True)),
 'obligation_discharge':('register/issue-delta-v1.json',lambda d:d.__setitem__('original_obligations_discharged',1)),
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
   n='BESHStat.Excel.Common/src/Analysis/UdfModelHandlePolicy.vb';f[n]+=b'\nImports System.Collections.Concurrent.ConcurrentDictionary\n'
   # Adjust source claims too: semantic dependency guard must reject independently of the hash guard.
   for p in ['matrix/source-freeze-v1.json','scope/portable-ownership-lock-v1.json']:
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
