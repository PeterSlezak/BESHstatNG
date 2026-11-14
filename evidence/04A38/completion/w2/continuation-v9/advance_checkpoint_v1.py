"""Serial postscan bookkeeping/verification; no production/test edits or rerun campaigns."""
from common_v2 import *
phase=sys.argv[1]
tasks={
 'reports':[('report-validation-v2','report_validation_v2.py',[]),('investigate-nonpass-v1','report_nonpass_investigation_v1.py',[])],
 'tools':[('prepare-final-validation-v1','prepare_final_validation_v1.py',[]),('prepare-preservation-v1','prepare_preservation_v1.py',[]),('prepare-publication-v1','prepare_publication_v1.py',[]),('publication-privacy-proof-v1','prove_publication_privacy_v1.py',[])],
 'proof':[('finish-checkpoint-inputs-v1','finish_checkpoint_inputs_v1.py',[]),('fixture-crosswalk-v1','fixture_crosswalk_v1.py',[])],
 'seal':[('checkpoint-seal-v1','seal_and_controls_v1.py',['seal']),('checkpoint-controls-v1','seal_and_controls_v1.py',['controls'])],
 'review':[('write-owner-review-v1','write_owner_review_v1.py',[]),('publication-import-controls-v1','prove_publication_import_v1.py',[]),('record-receipts-v1','record_receipts_v1.py',[])],
 'preflight':[('prepublication-privacy-v1','preflight_publication_v1.py',[])],
 'publish':[('publication-inventory-v1','publish_v1.py',['inventory']),('publication-export-v1','publish_v1.py',['export'])],
 'private':[('retained-private-check-v1','../continuation-v9-retained-private-reader-v1.py',[])],
 'final':[('final-live-git-preservation-v1','postseal_v1.py',[])]
}
for label,name,args in tasks[phase]:
    print('START',label,flush=True);r=command(label,[sys.executable,'-B',str(HERE/name),*args]);print('END',label,'exit',r['exit_code'],'seconds',r['seconds'],flush=True)
    if r['exit_code']!=0:raise SystemExit(r['exit_code'])
