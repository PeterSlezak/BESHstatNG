"""Actual sealed-reader rejection controls; no live source/evidence mutation."""
from common import *
import importlib.util,copy
spec=importlib.util.spec_from_file_location('c16_sealed_controls',HERE/'verify_c16_checkpoint_v3.py');reader=importlib.util.module_from_spec(spec);spec.loader.exec_module(reader)
prefix=HERE.relative_to(ROOT).as_posix()+'/'
controls=[]
def reject(name,path,value):
 try:reader.compute(ROOT,{path:value},predecessor=False)
 except (AssertionError,ValueError,OSError,KeyError,TypeError) as e:controls.append(dict(name=name,status='REJECTED',path=path,reason=str(e)))
 else:raise AssertionError('Adverse input accepted '+name)
source='BESHstatNG/src/ExcelUDFs/Regression/LinearModelUDFs.vb'
reject('changed live source',source,(ROOT/source).read_bytes()+b'\n')
reject('missing live source',source,None)
path=prefix+'historical-ownership-v2.json';bad=json.loads((ROOT/path).read_bytes());row=next(r for r in bad['members'] if not r['proof']);row['proof']={'forged':'unsupported closure'}
reject('unsupported historical closure',path,json.dumps(bad).encode())
path=prefix+'validation-v1.json';bad=json.loads((ROOT/path).read_bytes());bad['Release_compilation']='PASS'
reject('deferred Release falsely labelled PASS',path,json.dumps(bad).encode())
compat=json.loads((HERE/'compatibility-combined-v3.json').read_bytes());late=next(r for r in compat['results'] if r['corpus']=='A')
reject('changed frozen late-reference input',late['inputs']['path'],(ROOT/late['inputs']['path']).read_bytes()+b' ')
path=prefix+'compatibility-combined-v3.json';bad=copy.deepcopy(compat);next(r for r in bad['results'] if r['corpus']=='A')['corpus']='preedit-invented'
reject('late provenance relabelled as pre-edit',path,json.dumps(bad).encode())
path='evidence/04A38/completion/w2/continuation-v15/verify_c15_checkpoint_v1.py'
reject('modified predecessor reader',path,(ROOT/path).read_bytes()+b'\n')
from publication_privacy_c16_v1 import safe_path
for path in ['../escape','/absolute','C:/absolute','a/../../escape','a\\escape']:
 try:safe_path(path)
 except (AssertionError,ValueError):controls.append(dict(name='unsafe extraction path',path=path,status='REJECTED'))
 else:raise AssertionError('Unsafe review path accepted '+path)
save('adversarial-reader-controls-v3.json',dict(status='ALL_REAL_READER_AND_SAFE_PATH_CONTROLS_REJECTED',controls=controls,positive='Final actual positive reader command is separately recorded. Negative runs fail before predecessor execution; no test suite or historical scanner repeated.',original_queries=0))
print('Actual sealed rejection controls',len(controls),flush=True)
