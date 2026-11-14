from common import *
book=PRIVATE/'bookkeeping-code-v1'
def write(name,value):
 with (book/name).open('x',encoding='utf-8',newline='\n') as f:f.write(value)
write('run_composition_v1.py',"from common import *\nfrom verify_c14_composition_v2 import inspect\nresult=inspect(ROOT);save('source-composition-proof-v1.json',result);print('Complete whole-body source composites verified')\n")
write('final_validation_v1.py',"from common import *\nfrom verify_c14_validation_v1 import inspect\nr=inspect(lambda p:json.loads((ROOT/p).read_bytes()),lambda p:(ROOT/p).read_bytes(),HERE.relative_to(ROOT).as_posix()+'/');save('validation-independent-replay-v1.json',r);print(json.dumps(r))\n")
write('prepare_publication_authority_v1.py',"""from common import *
save('publication-privacy-authority-v1.json',dict(base_policy=pin(W2/'continuation-v6/publication_policy.py'),C10_publication_receipt=pin(W2/'continuation-v10-publication-receipt-v1.json'),C13_complete_manifest=pin(PREVIOUS/'publication-manifest-v1.json'),C13_final_git=pin(W2/'continuation-v13-final-git-preservation-v1.json'),law='Unchanged base policy; exact path/size/SHA/policy decisions from the authenticated complete C13 manifest or C10 physical receipt only. All changed/new selected bytes get fresh unchanged policy evaluation once at checkpoint.',original_queries=0))
""")
s=PREVIOUS/'bind_final_validation_v1.py';v=s.read_text().replace('C13','C14').replace('new_historical=26,new_current=29,new_support=1','new_historical=35,new_current=35,new_support=3')
v=v.replace("'open-byref-alias-defect-v1.json'","'roc-compatibility-repair-final-v1.json'").replace("'original-callback-delegate-return-abi-v1.json'","'property-body-contracts-v1.json'").replace("'new-parent-fragment-crosswalk-v2.json'","'new-parent-fragment-crosswalk-v2.json'")
v=v.replace("'python-scan-input-freeze-v1.json'","'python-scan-input-freeze-v2.json'").replace("blocked=['GuessPositiveClass','TryGetRocInput','ROC_POINTS','ROC_STATS']","blocked=[]")
write('bind_final_validation_v1.py',v)
v=(PREVIOUS/'seal_controls_v1.py').read_text().replace('proof_kernel_v6','proof_kernel_v8').replace('c13','c14').replace('C13','C14').replace('verify_c14_checkpoint_template_v4.py','verify_c14_checkpoint_template_v2.py').replace('continuation-v14-retained-private-reader-v2.py','continuation-v14-retained-private-reader-v1.py').replace("W2/'continuation-v12-retained-private-preservation-v1.json'","W2/'continuation-v13-retained-private-preservation-v1.json'")
v=v.replace("('Swapped repeated callback role','callback-identity-context-v1.json',lambda d:d['rows'][0].update(parameter_slot=2))","('Changed actual callback target','callback-proofs-v1.json',lambda d:d['rows'][0]['configurations'][0].update(target='unsupported-target'))")
v=v.replace("('BESHStatNG.ExcelCommon.UdfPlotDataPreparation38','CompleteSurvivalRecordsPrepared')","('BESHStatNG.ExcelCommon.UdfClassLabelPreparation38','GuessPositiveClassAfterConversion')")
write('seal_controls_v1.py',v)
write('install_postscan_tools_v1.py',"""from common import *
assert json.loads((HERE/'python-frozen-input-postcheck-v1.json').read_bytes())['status']=='EXACT_FROZEN_INPUTS_UNCHANGED'
book=PRIVATE/'bookkeeping-code-v1';rows=[]
for p in sorted(book.glob('*.py')):
 target=W2/p.name if p.name.startswith('continuation-v14-') else HERE/p.name
 assert not target.exists(),'Never overwrite existing source '+target.name
 with target.open('xb') as f:f.write(p.read_bytes())
 rows.append(dict(private_source=pin(p),published_source=pin(target)))
for p in sorted(book.glob('*-correction-*.json')):
 target=HERE/p.name;assert not target.exists()
 with target.open('xb') as f:f.write(p.read_bytes())
 rows.append(dict(private_source=pin(p),published_source=pin(target)))
save('postscan-bookkeeping-source-crosswalk-v1.json',dict(files=rows,scope='Privately developed tools installed append-only only after exact frozen-input postcheck. Draft outputs are not silently promoted; official source/CIL/body/census artifacts are regenerated and independently verified.',original_queries=0))
""")
write('roc_repair_receipt_v1.py',"""from common import *
from proof_kernel_v8 import definitions,semantic
from source_views import body
pc=json.loads((HERE/'portable-api-contracts-v1.json').read_bytes())['contracts'];oldapi=next(p for p in pc if p['source_record']['owner']=='BESHStatNG.ExcelCommon.UdfClassLabelPreparation38' and p['source_record']['name']=='GuessPositiveClass');newapi=next(p for p in pc if p['source_record']['name']=='GuessPositiveClassAfterConversion');records=[]
for cfg in ['Debug','Release']:
 meta=json.loads((HERE/('metadata-all/packet-v1-context-v5/current-'+cfg+'.json')).read_bytes());prior=json.loads((PREVIOUS/('metadata-all/packet-v1-context-v5/current-'+cfg+'.json')).read_bytes());defs=definitions(meta);old=definitions(prior);existing=next(r for r in oldapi['compiled'] if r['configuration']==cfg)['method'];added=next(r for r in newapi['compiled'] if r['configuration']==cfg)['method'];assert semantic(existing)==semantic(old[existing['method']]) and not added['exceptions'];callers=[m for m in meta['methods'] if any(i.get('operand')=='METHOD:'+added['method'] for i in m['instructions'])];assert len(callers)==1 and callers[0]['declaring_type']=='BESHStatNG.UdfDataImport' and callers[0]['member_name']=='GuessPositiveClass';records.append(dict(configuration=cfg,unchanged_existing_API=existing,new_postconversion_API=added,actual_Windows_consumers=callers))
tests=json.loads((HERE/'managed-Debug-identities-v1.json').read_bytes())['new_tests'];edges=[r for r in tests if 'Continuation14ActualNativeAliasIsRetainedOnFalse' in r['test_name'] or 'Continuation14NativeFailure' in r['test_name'] or 'Continuation14NativeStatusConversion' in r['test_name'] or 'OldResettingContract' in r['test_name']];assert edges and all(r['outcome']=='Passed' for r in edges)
save('roc-compatibility-repair-final-v1.json',dict(status='COMPLETE_ORIGINAL_SOURCE_CURRENT_CIL_AND_CURRENT_ONLY_DIAGNOSTIC_PARITY_PROOF',original_source_authority=pin(PREVIOUS/'open-byref-alias-defect-v1.json'),decision=pin(HERE/'roc-compatibility-decision-v1.json'),source_reconstruction=pin(HERE/'source-composition-proof-v1.json'),fragments=pin(HERE/'new-parent-fragment-crosswalk-v2.json'),configurations=records,current_only_diagnostic_identities=edges,original_False_converted='SOURCE_DERIVED_PREDICTION_NOT_EXECUTED_ORIGINAL_EVIDENCE',current_False_converted='MEASURED_CURRENT_ONLY_MANAGED_DIAGNOSTICS',existing_API_contract='UNCHANGED_RESETTING_FALSE_NOTHING_FOR_ALL_OTHER_CALLERS',original_queries=0,new_reference_inputs=0,new_reference_captures=0,native_Office='NOT_RUN',scientific_truth='NOT_GOLDEN'))
""")
write('finish_proof_v1.py',"""from common import *
steps=[('contracts-final-v1','build_contracts_v5.py',[]),('scope-final-v1','source_scope_v1.py',[]),('source-composition-final-v1','run_composition_v1.py',[]),('constructed-final-v1','constructed_context_v1.py',[]),('fragments-final-v1','fragment_correspondence_v2.py',[]),('ownership-final-v1','build_ownership_v1.py',[]),('roc-repair-final-v1','roc_repair_receipt_v1.py',[]),('test-identity-report-v1','report_validation_v1.py',[]),('nonpass-source-deltas-v1','postprocess_v1.py',[]),('shipping-binding-v1','bind_current_validation_v1.py',[]),('fixture-crosswalk-v1','fixture_crosswalk_v1.py',[]),('independent-validation-v1','final_validation_v1.py',[]),('final-source-binding-v1','bind_final_validation_v1.py',[]),('publication-authority-v1','prepare_publication_authority_v1.py',[])]
for label,filename,args in steps:
 r=command(label,[sys.executable,'-B',str(HERE/filename),*args]);print(label,r['exit_code'],flush=True);assert r['exit_code']==0,'Retain unsuccessful attempt '+label
""")
print('Final source/repair/validation/authority/seal/install/orchestration tools prepared privately')
