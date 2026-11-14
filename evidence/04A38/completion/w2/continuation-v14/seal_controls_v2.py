"""Seal final exact input bytes; execute real reader and adversarial mutations."""
from common import *
import copy,importlib.util,zipfile
from proof_kernel_v8 import closure
PREFIX=HERE.relative_to(ROOT).as_posix()+'/'
excluded={r['path'] for r in json.loads((HERE/'review-exclusions-v1.json').read_bytes())['excluded']}
paths={p.relative_to(ROOT).as_posix() for p in HERE.rglob('*') if p.is_file() and '__pycache__' not in p.parts and p.relative_to(ROOT).as_posix() not in excluded}
paths.update(json.loads((HERE/'implementation-plan-v1.json').read_bytes())['source_paths'])
auth={n:pin(ROOT/n)['sha256'] for n in sorted(paths)}
template=HERE/'verify_c14_checkpoint_template_v3.py';v=template.read_text();assert v.count('AUTH={}')==1
reader=HERE/'verify_c14_checkpoint_v2.py'
with reader.open('x',encoding='utf-8',newline='\n') as f:f.write(v.replace('AUTH={}','AUTH='+repr(auth)))
save('review-seal-v1.json',dict(status='PARTIAL_W2_NOT_READY',reader=pin(reader),template=pin(template),authenticated_inputs=len(auth),original_queries=0))
r=command('sealed-reader-positive-v1',[sys.executable,'-B',str(reader)]);save('sealed-reader-positive-v1.json',clean(r));assert r['exit_code']==0
import verify_c14_checkpoint_v2 as verify
controls=[]
def reject(name,fn):
 try:fn()
 except (ValueError,AssertionError,KeyError,OSError,zipfile.BadZipFile) as e:controls.append(dict(name=name,status='REJECTED',reason=clean(str(e))));return
 raise AssertionError('Mutation accepted '+name)
source=json.loads((HERE/'source-transitions-final-v1.json').read_bytes())['files'][0]['path']
reject('Changed actual production bytes',lambda:verify.compute(overrides={source:(ROOT/source).read_bytes()+b'changed'},check_validation=False))
reject('Missing actual source',lambda:verify.compute(overrides={source:b''},check_validation=False))
for label,path,edit in [
 ('Missing complete body dependency','whole-body-contracts-v1.json',lambda d:d['contracts'].pop()),
 ('Unsupported historical closure','historical-ownership-v2.json',lambda d:next(r for r in d['members'] if not r['proof']).update(proof=dict(forged=True))),
 ('Changed actual callback target','callback-proofs-v1.json',lambda d:d['rows'][0]['configurations'][0].update(target='unsupported-target')),
 ('Altered frozen original provenance','compatibility-packet-v1.json',lambda d:d['results'][0]['original'].update(sha256='0'*64)),
 ('Changed public exports','inspection/packet-v1/Inspect38-Debug.json',lambda d:d['exports'].pop()),
 ('Unsupported stage/readiness','readiness-v1.json',lambda d:d.update(status='READY_04A38',W3='COMPLETE')),
 ('Dropped genuine fragment','new-parent-fragment-crosswalk-v2.json',lambda d:d['rows'].pop())]:
  p=PREFIX+path;d=json.loads((ROOT/p).read_bytes());edit(d);data=json.dumps(d).encode();reject(label,lambda p=p,data=data:verify.compute(overrides={p:data},check_validation=False))
meta=json.loads((HERE/'metadata-all/packet-v1-context-v5/current-Debug.json').read_bytes());roots=json.loads((HERE/'proof-inputs-v1.json').read_bytes())['roots']
key=next(m['method'] for m in meta['methods'] if (m['declaring_type'],m['member_name'])==('BESHStatNG.ExcelCommon.UdfClassLabelPreparation38','GuessPositiveClassAfterConversion'))
bad=copy.deepcopy(meta);bad['methods']=[m for m in bad['methods'] if m['method']!=key]
reject('Structurally missing actual internal compiled dependency',lambda:closure(bad,roots))
p=W2/'continuation-v13-retained-private-reader-v2.py';spec=importlib.util.spec_from_file_location('c14_private_controls',p);private=importlib.util.module_from_spec(spec);spec.loader.exec_module(private)
prior=json.loads((W2/'continuation-v13-retained-private-preservation-v1.json').read_bytes());small=next(r for r in prior['private_pins'] if 0<r['bytes']<1024);data=(ROOT/small['path']).read_bytes()
reject('Changed inherited private pin',lambda:private.check(small,{small['path']:data+b'changed'}))
reject('Missing inherited private pin',lambda:private.check(small,{small['path']:None}))
save('independent-controls-v1.json',dict(reader=pin(reader),controls=controls,original_queries=0))
print('Sealed reader and mutation controls PASS_NOT_READY',len(controls))
