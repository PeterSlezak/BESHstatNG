"""Serial bounded postscan finalization; interrupted aggregate remains retained."""
from common_v2 import *
tasks={
 'reports':[('report-validation-v4','report_validation_v4.py',[]),('investigate-nonpass-v2','report_nonpass_investigation_v2.py',[])],
 'tools':[('prepare-final-validation-v2','prepare_final_validation_v2.py',[]),('prepare-preservation-v1','prepare_preservation_v1.py',[]),('prepare-publication-v2','prepare_publication_v2.py',[]),('publication-privacy-proof-v1','prove_publication_privacy_v1.py',[])],
 'proof':[('finish-checkpoint-inputs-v2','finish_checkpoint_inputs_v2.py',[]),('fixture-crosswalk-v1','fixture_crosswalk_v1.py',[])],
 'seal':[('checkpoint-seal-v2','seal_and_controls_v2.py',['seal']),('checkpoint-controls-v2','seal_and_controls_v2.py',['controls'])],
 'review':[('write-owner-review-v2','write_owner_review_v2.py',[]),('publication-import-controls-v1','prove_publication_import_v1.py',[]),('record-receipts-v2','record_receipts_v2.py',[])],
 'preflight':[('prepublication-privacy-v3','preflight_publication_v3.py',[])],
 'publish':[('publication-inventory-v1','publish_v1.py',['inventory']),('publication-export-v1','publish_v1.py',['export'])],
 'private':[('retained-private-check-v2','../continuation-v9-retained-private-reader-v2.py',[])],
 'final':[('final-live-git-preservation-v1','postseal_v1.py',[])]
}
for label,name,args in tasks[sys.argv[1]]:
    print('START',label,flush=True);r=command(label,[sys.executable,'-B',str(HERE/name),*args]);print('END',label,'exit',r['exit_code'],'seconds',r['seconds'],flush=True)
    if r['exit_code']!=0:raise SystemExit(r['exit_code'])
