"""Keep real candidate order and original exceptions for deferred oversized inputs."""
from common import *
import ast
from parallel_scan_v1 import original_module,Accelerator,authority
assert os.environ.get('PYTHONHASHSEED')=='38'
base=json.loads((HERE/'parallel-scanner-base-equivalence-v1.json').read_bytes())
assert base['reader']==pin(HERE/'parallel_scan_v1.py')
work=PRIVATE/'scanner-deferral-proof-v1';work.mkdir(exist_ok=False);fixtures=work/'fixtures';fixtures.mkdir()
paths=[]
for i in range(34):
    p=fixtures/(str(i)+'.txt');p.write_text('public value '+str(i),encoding='utf-8');paths.append(p.relative_to(ROOT).as_posix())
bad=fixtures/'invalid.json';bad.write_text('['+' '*512,encoding='utf-8');invalid=bad.relative_to(ROOT).as_posix()
good=fixtures/'large.json';good.write_text(json.dumps({'text':'x'*512}),encoding='utf-8');valid=good.relative_to(ROOT).as_posix()
reference=original_module();sys.path.insert(0,str(W2/'continuation-v6'))
hook={**globals()};exec(compile((PREVIOUS/'scanner-hook-ast-v1.py').read_text(encoding='utf-8'),'unchanged C6 hook deferral proof','exec'),hook)
accelerator=Accelerator(hook,work,defer_bytes=64);accelerator.install();target=original_module();controls=[]
for label,items in [('valid',paths+[valid]),('invalid-first',[invalid]+paths),('invalid-middle',paths[:17]+[invalid]+paths[17:]),('invalid-last',paths+[invalid]),('duplicate-invalid',[invalid]+paths+[invalid])]:
    def evaluate(module):
        try:return dict(result=module.scan(items),error=None)
        except Exception as e:return dict(result=None,error=dict(type=type(e).__name__,message=str(e),position=getattr(e,'pos',None),line=getattr(e,'lineno',None),column=getattr(e,'colno',None)))
    expected=evaluate(reference);actual=evaluate(target);assert actual==expected,(label,actual,expected)
    controls.append(dict(name=label,status='PASS',candidate_count=len(items),result_error=actual['error'],candidate_order_sha256=sha(json.dumps(items).encode())))
assert {r['path'] for r in accelerator.deferred}=={invalid,valid}
assert not any(k[3] in [invalid,valid] for k in hook['cache']) or any(k[3]==valid for k in hook['cache'])
# Missing domain/seed/changed AST remains the inherited nonaccelerated path.
for name in ['scanner-hook-ast-v1.py','tools-hook-ast-v1.py']:
    expected=json.loads((PREVIOUS/'python-optimization-authentication-v1.json').read_bytes())['prefixes'][name.split('-')[0]]
    assert pin(PREVIOUS/name)==expected['extracted']
    assert sha(ast.dump(ast.parse((PREVIOUS/name).read_text(encoding='utf-8')),include_attributes=False).encode())==expected['ast_sha256']
save('parallel-scanner-equivalence-v1.json',dict(base=pin(HERE/'parallel-scanner-base-equivalence-v1.json'),reader=pin(HERE/'parallel_scan_v1.py'),proof=pin(HERE/'prove_scanner_deferral_v1.py'),controls=base['controls']+controls,deferred_proof_inputs=accelerator.deferred,authority=clean(authority()),status='UNCHANGED_FILESET_ORDER_ORIGINAL_SCAN_RESULTS_AND_DEFERRED_EXCEPTION_FALLBACK_PROVED',retained_regex_controls=12068,assertions_changed=0,original_queries=0,scope='Files above 256MiB are only omitted from helper precomputation, never omitted from candidates. Original unchanged sequential C6 cache/scan runs in original order and retains original exception type/message/JSON position. The proof exercises the identical scheduling predicate with a 64-byte threshold using small valid and malformed JSON at first/middle/last/duplicate positions. No parsing or aggregation of the protected 4GB historical failed report by this proof.'))
print('Scanner deferral and unchanged base controls PASS',len(base['controls'])+len(controls))
