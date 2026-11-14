"""Actual final reader rejects altered indexes; no test campaign or scan."""
import sys,json,copy
from pathlib import Path
W2=Path(__file__).resolve().parent;HERE=W2/'continuation-v15'
sys.path.insert(0,str(HERE))
from common import *
from verify_c15_publication_v3 import inspect
folder=PRIVATE/'publication-adverse-v1';folder.mkdir(exist_ok=False)
source=W2/'continuation-v15-review-parts-index-v2.json';original=json.loads(source.read_bytes())
old=original['files'][0]['path'];runtime='scripts/test_suite_rationalization.py'
controls=[]
for label,mutate,message in [
 ('missing retained payload',lambda d:d.update(files=[r for r in d['files'] if r['path']!=old]),'Prior payload omitted or changed'),
 ('changed retained archive',lambda d:d['parts'][0].update(sha256='0'*64),'Original physical archives changed or supplement missing'),
 ('missing runtime dependency',lambda d:d.update(files=[r for r in d['files'] if r['path']!=runtime]),'Duplicate/missing logical path'),
 ('changed retained member SHA',lambda d:d['files'][0].update(sha256='0'*64),'Prior payload omitted or changed'),
 ('changed manifest binding',lambda d:d['manifest'].update(sha256='0'*64),'External index/manifest authority differs'),
 ('changed prior index authority',lambda d:d['previous_index'].update(sha256='0'*64),'Prior index changed'),
]:
 bad=copy.deepcopy(original);mutate(bad);path=folder/(str(len(controls)+1)+'.json')
 with path.open('x',encoding='utf-8') as f:json.dump(bad,f)
 try:inspect(ROOT,path,physical=False,check_checkpoint=False,check_preservation=False)
 except ValueError as e:
  assert str(e)==message,(label,str(e),message)
  controls.append(dict(name=label,status='REJECTED_BY_ACTUAL_FINAL_READER',message=str(e),index=pin(path)))
 else:raise AssertionError('Invalid publication accepted '+label)
target=W2/'continuation-v15-publication-adverse-reader-controls-v1.json'
with target.open('x',encoding='utf-8',newline='\n') as f:
 json.dump(dict(status='ALL_FINAL_PUBLICATION_READER_ADVERSE_CONTROLS_REJECTED',reader=pin(HERE/'verify_c15_publication_v3.py'),index=pin(source),controls=controls,original_queries=0),f,indent=2);f.write('\n')
print('Actual final publication reader adverse controls PASS',len(controls),flush=True)
