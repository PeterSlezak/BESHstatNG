"""Seal exact reviewed inputs, run the final reader, then genuine byte/structure controls."""
from common import *
import copy
import importlib.util
import zipfile
import warnings
from proof_kernel_v3 import closure,bind

PREFIX=HERE.relative_to(ROOT).as_posix()+'/'
excluded={r['path'] for r in json.loads((HERE/'review-exclusions-v1.json').read_bytes())['excluded']}
paths=set()
for path in HERE.rglob('*'):
    name=path.relative_to(ROOT).as_posix()
    if path.is_file() and '__pycache__' not in path.parts and name not in excluded:
        paths.add(name)
paths.update(json.loads((HERE/'implementation-plan-v1.json').read_bytes())['source_paths'])
# Results/manifest created after sealing have the manifest/physical-index authority,
# rather than a recursive reader-self hash.
paths.discard(PREFIX+'seal_controls_v1.py')
auth={p:pin(ROOT/p)['sha256'] for p in sorted(paths)}
template_path=HERE/'verify_c11_checkpoint_template_v2.py'
template=template_path.read_text(encoding='utf-8')
assert template.count('AUTH={}')==1
reader=HERE/'verify_c11_checkpoint_v1.py'
with reader.open('x',encoding='utf-8',newline='\n') as stream:
    stream.write(template.replace('AUTH={}','AUTH='+repr(auth)))
save('review-seal-v1.json',dict(status='PARTIAL_W2_NOT_READY',reader=pin(reader),
    template=pin(template_path),authenticated_inputs=len(auth),
    raw_review_exclusions=pin(HERE/'review-exclusions-v1.json'),original_queries=0))
result=command('sealed-reader-positive-v1',[sys.executable,'-B',str(reader)])
save('sealed-reader-positive-v1.json',clean(result));assert result['exit_code']==0
sys.path.insert(0,str(HERE));import verify_c11_checkpoint_v1 as check_reader
controls=[]
def reject(name,fn):
    try:fn()
    except (ValueError,AssertionError,KeyError,OSError,zipfile.BadZipFile) as error:
        controls.append(dict(name=name,status='REJECTED',reason=clean(str(error))))
        return
    raise AssertionError('Control accepted '+name)
source=json.loads((HERE/'source-transitions-final-v1.json').read_bytes())['files'][0]['path']
reject('Changed actual production bytes',lambda:check_reader.compute(overrides={source:(ROOT/source).read_bytes()+b'changed'},check_validation=False))
for label,path,edit in [
 ('Missing whole-body dependency','whole-body-contracts-v1.json',lambda d:d['contracts'].pop()),
 ('Altered original frozen provenance','compatibility-packet-v2.json',lambda d:d['results'][0]['original'].update(sha256='0'*64)),
 ('Changed export metadata','inspection/packet-v2/Inspect38-Debug.json',lambda d:d['exports'][0].update(return_type='System.String')),
 ('Unsupported W3/readiness stage','readiness-v1.json',lambda d:d.update(status='READY_04A38',W3='COMPLETE')),
 ('Dropped original branch/inline fragment','new-parent-fragment-crosswalk-v1.json',lambda d:d['rows'].pop())]:
    key=PREFIX+path;doc=json.loads((ROOT/key).read_bytes());edit(doc)
    changed=json.dumps(doc).encode()
    reject(label,lambda key=key,changed=changed:check_reader.compute(overrides={key:changed},check_validation=False))
meta=json.loads((HERE/'metadata-all/packet-v2-context-v5/current-Debug.json').read_bytes())
inputs=json.loads((HERE/'proof-inputs-v1.json').read_bytes())
universe=json.loads((HERE/'current-universe-packet-v2.json').read_bytes());nodes={r['symbol_id']:r for r in universe['members']}
shapes={r['symbol_id']:r for r in json.loads((HERE/'declaration-shapes-v2.json').read_bytes())['rows']}
roots=[bind(nodes[key],meta,shapes)['method'] for key in inputs['roots']]
removed=next(m['method'] for m in meta['methods'] if m['declaring_type']=='BESHStatNG.ExcelCommon.UdfMultivariateReportPreparation38' and m['member_name']=='BuildFactorCommunalities')
mutated=copy.deepcopy(meta);mutated['methods']=[m for m in mutated['methods'] if m['method']!=removed]
reject('Structurally missing actual internal compiled body',lambda:closure(mutated,roots))
private_path=W2/'continuation-v11-retained-private-reader-v1.py'
spec=importlib.util.spec_from_file_location('c11_private_control',private_path);private=importlib.util.module_from_spec(spec);spec.loader.exec_module(private)
prior=json.loads((W2/'continuation-v10-retained-private-preservation-v2.json').read_bytes())
small=next(r for r in prior['private_pins'] if 0<r['bytes']<1024);data=(ROOT/small['path']).read_bytes()
reject('Changed prior private pin',lambda:private.check(small,{small['path']:data+b'changed'}))
reject('Missing prior private pin',lambda:private.check(small,{small['path']:None}))
save('independent-controls-v1.json',dict(status='SOURCE_BODY_PROVENANCE_ABI_STAGE_CONSTRUCTED_DEPENDENCY_PRIVATE_MUTATIONS_REJECTED',
    reader=pin(reader),controls=controls,original_queries=0))
print('Final sealed reader and',len(controls),'source/compiled/private controls PASS_NOT_READY',flush=True)
