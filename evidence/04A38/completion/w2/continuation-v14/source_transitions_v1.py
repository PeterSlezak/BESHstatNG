from common import *
plan=json.loads((HERE/'implementation-plan-v1.json').read_bytes());rows=[]
for before in plan['source_before']:
 path=before['live']['path'];target=HERE/'source-after/final-v1'/(path+'.txt');target.parent.mkdir(parents=True,exist_ok=True)
 with target.open('xb') as f:f.write((ROOT/path).read_bytes())
 rows.append(dict(path=path,before=before['live'],preimage=before['preimage'],after=pin(ROOT/path),snapshot=pin(target)))
for new in plan['new_sources']:
 path=new['path'];target=HERE/'source-after/final-v1'/(path+'.txt');target.parent.mkdir(parents=True,exist_ok=True)
 with target.open('xb') as f:f.write((ROOT/path).read_bytes())
 rows.append(dict(path=path,before=None,preimage=None,after=pin(ROOT/path),snapshot=pin(target),entry_state='ABSENT'))
for pinrow in json.loads((HERE/'production-test-freeze-v1.json').read_bytes())['sources']:assert pin(ROOT/pinrow['path'])==pinrow
save('source-transitions-final-v1.json',dict(files=rows,Core_changes=0,original_queries=0))
