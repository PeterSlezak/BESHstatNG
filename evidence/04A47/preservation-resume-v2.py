from common47 import *
lock();key='BESHstatNG/Generated/VersionAttributes.vb';old=ROOT/'.test-rationalization-04A46.tmp/source-preimages'/key;pre=PRIVATE/'source-preimages'/key
expected=load(ROOT/'evidence/04A46/matrix/source-freeze-v1.json')['source_pins'][key]
assert pin(old)['sha256']==expected and old.read_bytes().replace(b'\r\n',b'\n')==pre.read_bytes()==(ROOT/key).read_bytes()
assert pin(pre)['sha256']==load(HERE/'entry/source-preimages-v1.json')['source_pins'][key]
save('entry/generated-newline-transition-v1.json',dict(source=key,sealed46=pin(old),entry47=pin(pre),current=pin(key),kind='OBSERVED_PRE_ENTRY_GENERATED_SOURCE_CRLF_TO_LF_ONLY',before_bytes=224,current_bytes=219,normalized_bytes_exact=True,version_and_BuildDate_unchanged=True,phase47_edit=False,qualification='Ignored generated compiler source,not an authored tracked merge delta. Current bytes already sealed at entry before any47 edits;exact old source copy retained. This is byte lineage,not native attribution or a new authorization.'))
failed=[]
for p in (PRIVATE/'failed-attempts').glob('*.json'):
 d=load(p)
 if d['recipe']=='preservation-v1.py':assert 'preservation-base-v1.py' in d['traceback'] and 'line 48' in d['traceback'];failed.append(pin(p))
assert len(failed)==1
# The failed recipe completed all physical checkpoint/historical/raw40-44 loops and
# wrote all four current store inventories before its overly strict successor check.
# Resume that exact completed stage;do not overwrite or rerun its expensive readback.
populations=[]
for b,n in [('41',6810),('42',11457),('43',8160),('44',8307)]:
 prior=ROOT/('.test-rationalization-04A'+b+'.tmp/new-raw-inventory-v1.json');inventory=PRIVATE/('prior'+b+'-whole-store-readback-v1.json');a=load(prior)['files'];current=load(inventory)['files'];by={r['path']:r for r in current};assert len(a)==n
 for r in a:assert by[r['path']]==r
 populations.append(dict(batch=b,prior_manifest=pin(prior),prior_exact=n,current_whole_store_files=len(current),current_inventory=pin(inventory),physical_readback_attempt=failed[0],completed_before_failure=True))
for p,h in load(ROOT/'evidence/04A46/matrix/source-freeze-v1.json')['source_pins'].items():
 if p=='BESHStatNG.Test/BESHStatNG.Tests.vbproj':assert pin(PRIVATE/'source-preimages'/p)['sha256']==h
 elif p!=key:assert pin(p)['sha256']==h,p
for r in load(ROOT/'evidence/04A44/entry/source-preimages-v1.json')['files']:assert pin(ROOT/'.test-rationalization-04A44.tmp/source-preimages'/r['path'])['sha256']==r['sha256']
canonical=load(ROOT/'evidence/04A39/register/defects-v7.json');counts=module('register47_resume',ROOT/'evidence/04A39/verify-register-v5.py').verify(ROOT,canonical,check_files=False)
assert (counts['canonical_entries'],counts['original_obligations'],counts['restricted_controls'],len(canonical['crosswalk']))==(1169,1514,802,2967)
base=dict(status='FRESH_LISTED_BYTE_READBACK_PASS_WITH_EXPLICIT_PRE_ENTRY_NEWLINE_SUCCESSOR',entry_tracked=15224,allowed_changes=['BESHStatNG.Test/BESHStatNG.Tests.vbproj','planning/BESHStatNG_CrossPlatform_MasterPlan.md'],all_other_entry_bytes_exact=True,checkpoint_copies=468,checkpoint_live_exact=464,checkpoint_authorized_newline=4,historical_paths=8984,raw40_paths=1906,raw_populations=populations,first_readback_attempt=failed[0],first_readback_failure='Previous46 raw compiler source pin did not allow the already-entry-sealed ignored generated CRLF/LF successor;all protected loops completed before that assertion. No historical byte was edited.',generated_newline_successor=pin(HERE/'entry/generated-newline-transition-v1.json'),accepted46_source_lineage_exact_except_declared_entry_newline=True,original_source_history=1353,historical_file_identity_drifts=788,continuous_physical_identity='NOT_VERIFIED',W3_union=dict(paths=37484,archives=154,fresh_full_union_readback=False,inherited_qualified=True),canonical_counts=counts,mappings=2967,original_privacy_findings_per_parent=1941,W3_privacy_errors='RETAINED_UNWAIVED',old_Debug_FAIL=14,old_04A08_scope_FAIL='FAIL_UNWAIVED',accepted41_Release_FAIL='RETAINED_FRESHLY_REPRODUCED_EXACT_RELEASE_TRACE',accepted42_failed_first_freeze='RETAINED',accepted44_raw_publication_failure='RETAINED',gates=GATES)
save('preservation/readback-base-v2.json',base)
s=(HERE/'preservation-v1.py').read_text();s=s[s.index('def pins'):]
exec(compile(s,str(HERE/'preservation-v1.py'),'exec'),dict(globals(),base=base,populations=[]))
