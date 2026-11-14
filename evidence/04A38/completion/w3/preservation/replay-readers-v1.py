"""Actual accepted read-only readers; redirect only new adverse-control receipt."""
from common import *
code='import sys,runpy;sys.path.insert(0,sys.argv[1]);sys.argv=[sys.argv[2]]+sys.argv[3:];runpy.run_path(sys.argv[0],run_name="__main__")'
results=[]
for name,script,args in [
    ('accepted-checkpoint',ACCEPTED/'verify_postcorrection_v3.py',[]),
    ('complete-v17-proof',W2/'continuation-v17/verify_c17_checkpoint_v2.py',[]),
    ('core-physical-ZIPs',ACCEPTED/'verify_transport_v1.py',[W2/'continuation-v17-postcorrection-v1-review-parts-index-v1.json',RUNROOT/'completion-w2-cont17-postcorrection-v1/review-parts-v1']),
    ('postseal-physical-ZIP',ACCEPTED/'verify_postseal_supplement_v1.py',[W2/'continuation-v17-postcorrection-v1-final-additive-readback-v1.json',RUNROOT/'completion-w2-cont17-postcorrection-v1/BESHstatNG_04A38_W2_v17_postcorrection_v1_POSTSEAL_SUPPLEMENT.zip'])]:
    result=command('reader-'+name,[sys.executable,'-I','-B','-c',code,str(script.parent),str(script),*map(str,args)])
    results.append(dict(name=name,reader=pin(script),result=result))
    save('preservation/reader-'+name+'-v1.json',results[-1])
    assert result['exit_code']==0,'AUTHENTICATED_READER_FAILED '+name
# The original adverse reader's sole public write is redirected to a new W3 file.
controlcode='import sys,runpy,json;from pathlib import Path;sys.path.insert(0,sys.argv[1]);import common;out=Path(sys.argv[3]);common.save=lambda name,value:out.open("x",encoding="utf-8").write(json.dumps(value,indent=2));sys.argv=[sys.argv[2]];runpy.run_path(sys.argv[0],run_name="__main__")'
script=ACCEPTED/'reader_controls_v3.py'
result=command('reader-existing-adverse-controls',[sys.executable,'-I','-B','-c',controlcode,str(ACCEPTED),str(script),str(HERE/'preservation/existing-adverse-controls-v1.json')])
save('preservation/existing-adverse-reader-execution-v1.json',dict(reader=pin(script),result=result))
assert result['exit_code']==0
print('Accepted source/full-body/fragment/physical-publication readers and adverse controls PASS',flush=True)
