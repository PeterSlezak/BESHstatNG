"""Post-publication source/Git/readback receipt, no Git mutation."""
from common39 import *
source_lock()
publication=load(HERE/'publication/physical-readback-v1.json')
assert pin(publication['archive']['path'])==publication['archive']
commands=[]
for args in [('branch','--show-current'),('rev-parse','HEAD','main'),('merge-base','HEAD','main'),('status','--short','--untracked-files=all'),
    ('diff','--cached','--name-status'),('diff','--check')]:
    raw=git(*args);commands.append(dict(arguments=args,utc=utc(),stdout=raw.decode(),stdout_sha256=sha(raw),exit_code=0))
index=pin('.git/index');assert index==load(HERE/'entry/merge-preflight-v1.json')['index']
changed=git('diff','--name-only').decode().splitlines()
assert set(changed)=={'BESHStatNG.Test/BESHStatNG.Tests.vbproj','planning/BESHStatNG_CrossPlatform_MasterPlan.md'}
untracked=git('ls-files','--others','--exclude-standard').decode().splitlines()
assert all(p.startswith('evidence/04A39/') or p in ['BESHStat.Excel.Common.Tests/src/UdfInputPolicy39Tests.vb',
    'BESHStatNG.Test/src/UDFs/UdfInputBoundary39Tests.vb','planning/BESHStatNG_CrossPlatform_MasterPlan2.14.md'] for p in untracked)
save('final/git-final-v1.json',dict(main=MAIN,branch='codex/04A-39',tip=MAIN,commands=commands,index=index,
    staged_empty=True,tracked_modified=changed,untracked=untracked,diff_check_exit=0,commit_created=False,merge_performed=False))
receipt=dict(status=load(HERE/'final/readiness.json')['status'],utc=utc(),main=MAIN,branch='codex/04A-39',tip=MAIN,
    owner_review=pin(HERE/'final/owner-review.md'),readiness=pin(HERE/'final/readiness.json'),canonical_register=pin(HERE/'register/current-v3.json'),
    Git=pin(HERE/'final/git-final-v1.json'),archive=publication['archive'],physical_readback=pin(HERE/'publication/physical-readback-v1.json'),
    logical_manifest=pin(HERE/'publication/payload-manifest-v1.json'),preservation=pin(HERE/'preservation/new-pins-v1.json'),
    inherited_file_pins=37484,inherited_archive_records=154,new_review_archives=1,
    physical_scope='Incremental39 audit/readback; no new whole inherited union/private upload claim.',
    omitted_postpublication_bookkeeping='This final Git/receipt follows the main ZIP to avoid self-hash cycles; provide these two JSONs beside the ZIP.',
    external_upload='NOT_PERFORMED',gates=GATES,next_batch='04A40_NOT_AUTHORIZED')
save('final/delivery-receipt-v1.json',receipt)
source_lock();assert pin('.git/index')==index
print(json.dumps(dict(status=receipt['status'],archive=receipt['archive'],Git=receipt['Git'])),flush=True)
