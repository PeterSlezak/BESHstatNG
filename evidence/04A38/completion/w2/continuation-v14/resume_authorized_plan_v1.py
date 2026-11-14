"""Additive owner-authorized plan transition; preserve all stopped evidence."""
from common import *
stop=json.loads((HERE/'preservation-stop-v1.json').read_bytes())
assert pin(ROOT/stop['current']['path'])==stop['current']
for key in ['entry_snapshot','current_snapshot','delta']:assert pin(ROOT/stop[key]['path'])==stop[key]
for r in json.loads((HERE/'production-test-freeze-v1.json').read_bytes())['sources']:assert pin(ROOT/r['path'])==r
assert git('branch','--show-current').decode().strip()=='codex/04A-38' and not git('diff','--cached','--name-only')
assert all(git(*args).decode().strip()==BASE for args in [('rev-parse','HEAD'),('rev-parse','main'),('merge-base','HEAD','main')])
assert subprocess.run(['git','-c','safe.directory='+ROOT.as_posix(),'--no-optional-locks','diff','--check'],capture_output=True).returncode==0
save('owner-plan-authorized-transition-v1.json',dict(status='OWNER_AUTHORIZED_ADDITIVE_DOCUMENT_TRANSITION',owner_message='BESHStatNG_CrossPlatform_MasterPlan.md change (udpdate to version 2.14) is expected and authorized.',authority_scope='Only the exact changed owner-plan bytes; preserve the authenticated entry bytes, every prior seal and historical test assertions. No new original/reference execution, scientific or stage authority.',before=stop['entry'],after=stop['current'],preimage=stop['entry_snapshot'],snapshot=stop['current_snapshot'],delta=stop['delta'],stopped_receipt=pin(HERE/'preservation-stop-v1.json'),original_queries=0))
def write(name,text):
 with (HERE/name).open('x',encoding='utf-8',newline='\n') as f:f.write(text)
v=(HERE/'postprocess_v1.py').read_text();needle="    elif row['identity'] in scanids:"
assert v.count(needle)==1
v=v.replace(needle,"""    elif row['identity'] in {'test_capture_contract04a02.CaptureContracts.test_packet_covers_all_original_cases_and_six_gates','test_04a23_multivariate_spc.Spc23Contracts.test_actual_merged_base_and_protected_owner_plan'}:
        transition=json.loads((HERE/'owner-plan-authorized-transition-v1.json').read_bytes())
        assert transition['before']['sha256'] in previous['message'] and transition['after']['sha256'] in row['message'] and pin(ROOT/transition['after']['path'])==transition['after']
        delta.update(classification='RETAINED_OWNER_DOCUMENT_PIN_FAIL_OWNER_AUTHORIZED_PLAN_HASH_DELTA_NOT_WAIVED',transition=pin(HERE/'owner-plan-authorized-transition-v1.json'))
"""+needle)
write('postprocess_v2.py',v)
v=(HERE/'verify_c14_preservation_v1.py').read_text();needle=' checked={};resolved=[]'
assert v.count(needle)==1
v=v.replace(needle,""" plan=load(root/(C14+'owner-plan-authorized-transition-v1.json'));need(plan['status']=='OWNER_AUTHORIZED_ADDITIVE_DOCUMENT_TRANSITION' and plan['original_queries']==0,'Wrong owner plan authority')
 need(plan['before']==dict(path='planning/BESHStatNG_CrossPlatform_MasterPlan.md',bytes=78245,sha256='436b66c4307bd1571649c2604f8d90b5f0b5d843df3567b918698392fe5d6e7c') and plan['after']==dict(path=plan['before']['path'],bytes=84627,sha256='0ee45eae70aa35d6c87c636d82c3f33e105b8c12e6e5a4badb69e38fe7ad69dd'),'Unapproved owner plan transition')
 for key in ['preimage','snapshot']:
  data=(root/plan[key]['path']).read_bytes();need((len(data),sha(data))==(plan[key]['bytes'],plan[key]['sha256']),'Owner document retained bytes changed')
 need((plan['preimage']['bytes'],plan['preimage']['sha256'])==(plan['before']['bytes'],plan['before']['sha256']) and (plan['snapshot']['bytes'],plan['snapshot']['sha256'])==(plan['after']['bytes'],plan['after']['sha256']),'Owner document transition mismatch')
"""+needle)
needle="  if checked[p]==expected:return"
v=v.replace(needle,needle+"""
  if p==plan['before']['path'] and expected==(plan['before']['bytes'],plan['before']['sha256']):
   need(checked[p]==(plan['after']['bytes'],plan['after']['sha256']),'Owner plan successor changed');resolved.append(dict(path=p,former_sha256=row['sha256'],retained=plan['preimage']['path'],current_sha256=plan['after']['sha256'],authority=C14+'owner-plan-authorized-transition-v1.json'));return""")
write('verify_c14_preservation_v2.py',v)
v=(HERE/'verify_c14_checkpoint_template_v3.py').read_text();needle=" u=load(PREFIX+'current-universe-packet-v1.json');"
assert v.count(needle)==1
v=v.replace(needle,""" plan=load(PREFIX+'owner-plan-authorized-transition-v1.json');need(plan['status']=='OWNER_AUTHORIZED_ADDITIVE_DOCUMENT_TRANSITION' and plan['before']['sha256']=='436b66c4307bd1571649c2604f8d90b5f0b5d843df3567b918698392fe5d6e7c' and plan['after']['sha256']=='0ee45eae70aa35d6c87c636d82c3f33e105b8c12e6e5a4badb69e38fe7ad69dd','Wrong owner plan authority')
 for key in ['preimage','snapshot','after']:check(plan[key])
 need(raw(plan['preimage']['path'])!=raw(plan['after']['path']) and raw(plan['snapshot']['path'])==raw(plan['after']['path']),'Owner plan exact byte transition missing');projected[plan['before']['path']]=raw(plan['preimage']['path'])
"""+needle)
write('verify_c14_checkpoint_template_v4.py',v)
write('seal_controls_v3.py',(HERE/'seal_controls_v2.py').read_text().replace('verify_c14_checkpoint_template_v3.py','verify_c14_checkpoint_template_v4.py'))
v=(HERE/'verify_c14_publication_v1.py').read_text().replace('verify_c14_preservation_v1','verify_c14_preservation_v2').replace('C13_','C14_')
write('verify_c14_publication_v2.py',v)
for name in ['publication_adverse_v1.py','copy_review_v1.py','finalize_live_v1.py','finalize_external_v1.py']:
 v=(HERE/name).read_text().replace('verify_c14_publication_v1','verify_c14_publication_v2').replace('verify_c14_preservation_v1','verify_c14_preservation_v2')
 write(name.replace('_v1.py','_v2.py'),v)
steps=[('nonpass-source-deltas-v2','postprocess_v2.py'),('shipping-binding-v1','bind_current_validation_v1.py'),('fixture-crosswalk-v1','fixture_crosswalk_v1.py'),('independent-validation-v1','final_validation_v1.py'),('final-source-binding-v1','bind_final_validation_v1.py'),('publication-authority-v1','prepare_publication_authority_v1.py')]
write('finish_proof_v2.py',"from common import *\nsteps="+repr(steps)+"\nfor label,name in steps:\n r=command(label,[sys.executable,'-B',str(HERE/name)]);print(label,r['exit_code'],flush=True);assert r['exit_code']==0,'Preserve failed attempt '+label\n")
print('Owner-authorized exact plan transition recorded; versioned successor readers retain all old assertions/seals')
