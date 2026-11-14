from common import *
from collections import Counter
load=lambda p:json.loads(Path(p).read_bytes())
# These are post-campaign evidence operations, not additional validation campaigns.
for label,args in [('reader-controls-v1',bootstrap(HERE/'reader_controls_v3.py')),('owner-dossier-v1',bootstrap(HERE/'write_dossier_v5.py')),('private-inventory-base-v1',bootstrap(HERE/'private_preservation_v2.py','prepare','1'))]:
 r=command(label,args);assert r['exit_code']==0,label
files=[pin(p) for p in sorted(HERE.rglob('*')) if p.is_file() and '__pycache__' not in p.parts]
save('review-seal-v1.json',dict(status='FINAL_CORRECTED_SOURCE_AND_TWO_REPLACEMENT_CAMPAIGNS_SEALED',files=files,authority=pin(HERE/'owner-authorization-v1.json'),source_freeze=pin(HERE/'production-test-freeze-v1.json'),Release=DEFERRED,original_queries=0,scope='Append-only post-correction evidence; later publication/preservation/final raw bindings are external additive authorities.'))
r=command('final-checkpoint-reader-v1',bootstrap(HERE/'verify_postcorrection_v3.py'));save('checkpoint-verification-v1.json',dict(command=clean(r),reader=pin(HERE/'verify_postcorrection_v3.py'),seal=pin(HERE/'review-seal-v1.json'),pass_status=r['exit_code']==0));assert r['exit_code']==0
for mode in ['checkpoint','inventory','export']:
 r=command('publication-'+mode+'-v1',bootstrap(HERE/'publish_review_v3.py',mode));assert r['exit_code']==0,mode
index=W2/'continuation-v17-postcorrection-v1-review-parts-index-v1.json'
r=command('publication-physical-reader-v1',bootstrap(HERE/'verify_transport_v1.py',index,PRIVATE/'review-parts-v1'));assert r['exit_code']==0
save('publication-physical-reader-receipt-v1.json',dict(command=clean(r),index=pin(index),reader=pin(HERE/'verify_transport_v1.py'),external_upload='NOT_PERFORMED;PRIVATE_OLD_EIGHT_ZIPS_NOT_UPLOADED',status='ACTUAL_ALL_NEW_PHYSICAL_PART_SHA_AND_ALL_LOGICAL_PAYLOAD_SHA_SIZE_CRC_PASS'))
raw=[]
for label,args in [('branch',['branch','--show-current']),('HEAD',['rev-parse','HEAD']),('main',['rev-parse','main']),('merge-base',['merge-base','HEAD','main']),('staged',['diff','--cached','--name-only']),('diff-check',['diff','--check']),('status',['status','--short'])]:
 r=command('final-git-'+label,['git','--no-optional-locks',*args]);assert r['exit_code']==0;raw.append(dict(label=label,command=clean(r),raw=[pin(RUNROOT/'commands'/('w2c17pc1-final-git-'+label)/n) for n in ['command.json','stdout.log','stderr.log']]))
assert git('branch','--show-current').decode().strip()=='codex/04A-38'
for args in [('rev-parse','HEAD'),('rev-parse','main'),('merge-base','HEAD','main')]:assert git(*args).decode().strip()==BASE
assert not git('diff','--cached','--name-only') and not git('diff','--check')
idx=pin(ROOT/'.git/index');assert idx['bytes']==620185 and idx['sha256']=='2036821003b1e1fb1077e72183e0b6c2fa7d22cdb2e784b760637e6d6fe9bc2c'
plan=pin(ROOT/'planning/BESHStatNG_CrossPlatform_MasterPlan.md');assert plan['sha256']=='0ee45eae70aa35d6c87c636d82c3f33e105b8c12e6e5a4badb69e38fe7ad69dd'
save('final-git-preservation-v1.json',dict(branch='codex/04A-38',HEAD=BASE,main=BASE,merge_base=BASE,index=idx,staged_empty=True,diff_check=0,MasterPlan=plan,raw=raw))
r=command('private-inventory-final-v1',bootstrap(HERE/'private_preservation_v2.py','prepare','2'));assert r['exit_code']==0
r=command('final-actual-private-audit-v1',bootstrap(HERE/'private_preservation_v2.py','verify',HERE/'retained-private-preservation-v2.json',PRIVATE/'final-actual-private-preservation-v1.json'));assert r['exit_code']==0
save('actual-private-preservation-receipt-v1.json',dict(command=clean(r),reader=pin(HERE/'private_preservation_v2.py'),actual=load(PRIVATE/'final-actual-private-preservation-v1.json'),raw=pin(PRIVATE/'final-actual-private-preservation-v1.json')))
for row in load(HERE/'production-test-freeze-v1.json')['sources']:assert pin(ROOT/row['path'])==row
# No second reader/scanner campaign. Core checkpoint already ran on this identical source.
validation=load(HERE/'validation-v1.json');preserved=load(HERE/'actual-private-preservation-receipt-v1.json')['actual'];parts=load(index)
postfiles=[pin(p) for p in sorted(HERE.rglob('*')) if p.is_file() and p.relative_to(ROOT).as_posix() not in {r['path'] for r in load(HERE/'review-seal-v1.json')['files']}]
external=W2/'continuation-v17-postcorrection-v1-final-authority-v1.json'
final=dict(status='POST_CORRECTION_COMPLETION_CANDIDATE_FOR_INDEPENDENT_REVIEW;NOT_ACCEPTED',owner_review=pin(HERE/'owner-review.md'),source_freeze=pin(HERE/'production-test-freeze-v1.json'),seal=pin(HERE/'review-seal-v1.json'),checkpoint=pin(HERE/'checkpoint-verification-v1.json'),postseal_files=postfiles,parts_index=pin(index),physical_reader=pin(HERE/'publication-physical-reader-receipt-v1.json'),Git=pin(HERE/'final-git-preservation-v1.json'),private_preservation=pin(HERE/'actual-private-preservation-receipt-v1.json'),validation=validation,ownership=dict(historical_supported=704,historical_OPEN=0,current_supported=726,current_OPEN=0,support_supported=303,support_OPEN=0),new_physical_parts=len(parts['parts']),private_pin_count=preserved['private_pins'],physical_archive_count=preserved['physical_archives'],external_upload='NOT_PERFORMED',W3='NOT_STARTED',original_queries=0,terminal='NOT_READY_04A38; W2_IN_PROGRESS; W3_NOT_STARTED; P=NO; N1=NO GO; N2=NO; C2=UNMET; C3=UNAPPROVED; NOT_GOLDEN; 04A39_BLOCKED')
with external.open('x',encoding='utf-8',newline='\n') as f:json.dump(final,f,indent=2,ensure_ascii=False);f.write('\n')
text="""# Final additive post-correction authority

The two approved replacement campaigns completed at the authenticated, unchanged final corrected v17 source. This record supplements the sealed owner review and publication; no previous seal is rewritten.

"""
text+='Managed Debug: '+str(validation['Debug']['outcomes'])+'; Python parents: '+str(validation['Python']['methods'])+'; Python events: '+str(validation['Python']['nonpass_events'])+'; strict/profile/MkDocs: '+str(validation['strict_profile_MkDocs'])+'.\n\n'
text+='All 29,424 fresh Debug frozen comparisons matched exactly; 268 exports preserved. Candidate ownership 704/704 historical, 726/726 current, 303/303 distinct support; all OPEN complements empty. This validation added no ownership closure and grants no acceptance.\n\n'
text+='Actual final preservation: '+str(preserved['private_pins'])+' file pins and '+str(preserved['physical_archives'])+' physical archives, including all inherited private bytes, identities and malformed v9 aggregation. Historical TestResults was not enumerated. New transport: '+str(len(parts['parts']))+' independent complementary ZIPs, all below180MiB, actual physical SHA plus every payload SHA/CRC verified locally. Older physical eight ZIPs remain private NOT_UPLOADED; no external physical verification is claimed.\n\n'
text+='The MAX_PATH copy failure before Common semantic host launch remains preserved. Completed parts were not repeated; only unstarted parts used shorter paths. Both privacy UnicodeDecodeError ERRORs and all other genuine inherited nonpasses remain unwaived. No production/test changes occurred during either campaign.\n\n'
text+='Release compilation; Release full/selected/focused tests; Release ABI/CIL/signatures/exports; Release frozen compatibility: '+DEFERRED+'. Native/scientific/Excel/COM/UIA/manual/Office/Mac/browser/original-reference/W3/04A39: NOT_RUN.\n\n'
text+='Review index: ['+HERE.name+'/owner-review.md]('+HERE.name+'/owner-review.md), ['+external.name+']('+external.name+'), ['+index.name+']('+index.name+'). Exact hashes, source/Git/private/publication receipts and all observed result deltas are bound in the final authority JSON.\n\n'
text+='Next step: independent review and an explicit owner decision.\n\n'+final['terminal']+'\n'
handover=W2/'continuation-v17-postcorrection-v1-postseal-handover-v1.md'
with handover.open('x',encoding='utf-8',newline='\n') as f:f.write(text)
print('Final post-correction authority PASS',preserved['private_pins'],preserved['physical_archives'],len(parts['parts']),flush=True)
