"""Freeze exact review authority, execute sealed reader, reject genuine mutations."""
from common import *
import copy,importlib.util,zipfile
from proof_kernel_v4 import bind,closure
PREFIX=HERE.relative_to(ROOT).as_posix()+'/'
excluded={r['path'] for r in json.loads((HERE/'review-exclusions-v1.json').read_bytes())['excluded']}
paths={p.relative_to(ROOT).as_posix() for p in HERE.rglob('*') if p.is_file() and '__pycache__' not in p.parts and p.relative_to(ROOT).as_posix() not in excluded}
paths.update(json.loads((HERE/'implementation-plan-v1.json').read_bytes())['source_paths']);paths.discard(PREFIX+'seal_controls_v1.py')
auth={name:pin(ROOT/name)['sha256'] for name in sorted(paths)}
template=HERE/'verify_c12_checkpoint_template_v1.py';text=template.read_text(encoding='utf-8');assert text.count('AUTH={}')==1
reader=HERE/'verify_c12_checkpoint_v2.py'
with reader.open('x',encoding='utf-8',newline='\n') as stream:stream.write(text.replace('AUTH={}','AUTH='+repr(auth)))
save('review-seal-v1.json',dict(status='PARTIAL_W2_NOT_READY',reader=pin(reader),template=pin(template),authenticated_inputs=len(auth),raw_review_exclusions=pin(HERE/'review-exclusions-v1.json'),original_queries=0))
result=command('sealed-reader-positive-v1',[sys.executable,'-B',str(reader)]);save('sealed-reader-positive-v1.json',clean(result));assert result['exit_code']==0
import verify_c12_checkpoint_v2 as check_reader
controls=[]
def reject(name,fn):
    try:fn()
    except (ValueError,AssertionError,KeyError,OSError,zipfile.BadZipFile) as error:controls.append(dict(name=name,status='REJECTED',reason=clean(str(error))));return
    raise AssertionError('Mutation accepted '+name)
source=json.loads((HERE/'source-transitions-final-v1.json').read_bytes())['files'][0]['path']
reject('Changed actual production bytes',lambda:check_reader.compute(overrides={source:(ROOT/source).read_bytes()+b'changed'},check_validation=False))
for name,path,edit in [
    ('Missing whole-body dependency','whole-body-contracts-v1.json',lambda d:d['contracts'].pop()),
    ('Altered frozen original provenance','compatibility-packet-v1.json',lambda d:d['results'][0]['original'].update(sha256='0'*64)),
    ('Changed public export ABI','inspection/packet-v1/Inspect38-Debug.json',lambda d:d['exports'][0].update(return_type='System.String')),
    ('Unsupported closure/readiness stage','readiness-v1.json',lambda d:d.update(status='READY_04A38',W3='COMPLETE')),
    ('Dropped genuine original fragment','new-parent-fragment-crosswalk-v1.json',lambda d:d['rows'].pop())]:
    key=PREFIX+path;doc=json.loads((ROOT/key).read_bytes());edit(doc);data=json.dumps(doc).encode()
    reject(name,lambda key=key,data=data:check_reader.compute(overrides={key:data},check_validation=False))
meta=json.loads((HERE/'metadata-all/packet-v1-context-v5/current-Debug.json').read_bytes());inputs=json.loads((HERE/'proof-inputs-v1.json').read_bytes())
universe=json.loads((HERE/'current-universe-packet-v1.json').read_bytes());nodes={n['symbol_id']:n for n in universe['members']}
shapes={r['symbol_id']:r for r in json.loads((HERE/'declaration-shapes-v1.json').read_bytes())['rows']};roots=[bind(nodes[key],meta,shapes)['method'] for key in inputs['roots']]
removed=next(m['method'] for m in meta['methods'] if (m['declaring_type'],m['member_name'])==('BESHStatNG.ExcelCommon.UdfModelImportPreparation38','CompleteCategoricalMatrixPrepared'))
bad=copy.deepcopy(meta);bad['methods']=[m for m in bad['methods'] if m['method']!=removed]
reject('Structurally missing actual internal compiled dependency',lambda:closure(bad,roots))
path=W2/'continuation-v12-retained-private-reader-v1.py';spec=importlib.util.spec_from_file_location('c12_private_controls',path);private=importlib.util.module_from_spec(spec);spec.loader.exec_module(private)
previous=json.loads((W2/'continuation-v11-retained-private-preservation-v4.json').read_bytes());small=next(r for r in previous['private_pins'] if 0<r['bytes']<1024);data=(ROOT/small['path']).read_bytes()
reject('Changed inherited private pin',lambda:private.check(small,{small['path']:data+b'changed'}))
reject('Missing inherited private pin',lambda:private.check(small,{small['path']:None}))
save('independent-controls-v1.json',dict(status='SOURCE_BODY_PROVENANCE_ABI_STAGE_COMPILED_DEPENDENCY_PRIVATE_MUTATIONS_REJECTED',reader=pin(reader),controls=controls,original_queries=0))
print('Sealed reader and genuine mutation controls PASS_NOT_READY',len(controls))
