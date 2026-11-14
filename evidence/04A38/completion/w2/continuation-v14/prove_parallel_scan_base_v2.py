"""Original function versus ordered process cache; synthetic/adverse controls."""
from common import *
from parallel_scan_v1 import original_module,authority,run,validate,batches,function_ast,POLICY
import base64,copy,collections
work=PRIVATE/'parallel-scan-proof-v2';work.mkdir(exist_ok=False);fixture=work/'fixtures';fixture.mkdir()
texts=['plain public text',('C:'+chr(92)+'Users'+chr(92)+'synthetic-owner'+chr(92)+'file'),'\\\\example-server\\share','S-1-5-21-1-2-3-4','reader@example.invalid','ghp_'+'x'*24,'sk-'+'x'*30,('https'+'://'+'synthetic'+':'+'example'+'@'+'example.invalid'),'password=synthetic_example_only','https://example.internal/path','prefix\n'+base64.b64encode(b'password=synthetic_example_only').decode(),'\uff48\uff54\uff54\uff50://example.local']
assert os.environ.get('PYTHONHASHSEED')=='38'
known=[p.name for p in ROOT.parents if p.parent.name.casefold()=='users']
if os.environ.get('COMPUTERNAME'):known.append(os.environ['COMPUTERNAME'])
texts.append(' '.join(known)) # Private fixture only; bind exact ordered token findings.
paths=[]
for i,text in enumerate(texts):
    target=fixture/(str(i)+'.txt');target.write_text(text,encoding='utf-8');paths.append(target.relative_to(ROOT).as_posix())
target=fixture/'escaped.json';target.write_text(json.dumps(dict(value=('C:'+chr(92)+'Users'+chr(92)+'synthetic-owner'+chr(92)+'file')),ensure_ascii=True),encoding='utf-8');paths.append(target.relative_to(ROOT).as_posix())
paths+=['BESHStat.Excel.Common/src/Analysis/UdfSampleSizePreparation38.vb','BESHstatNG/src/ExcelUDFs/SampleSizeUDFs.vb']
rows=[dict(index=i,**pin(ROOT/p)) for i,p in enumerate(paths)]
module=original_module();expected=module.scan(paths)
actual,receipts=run(rows,work/'execution',5)
def combine(values):return ([f for r in values for f in r['findings']],sorted({tuple(v) for r in values for v in r['reviewed']}))
assert combine(actual)==expected
controls=[dict(name='Exact original ordered full function versus five process per-file evaluation',status='PASS',files=len(paths))]
for label,indices in [('reverse',list(reversed(range(len(paths))))),('duplicate',[0,4,4,1,0])]:
    chosen=[paths[i] for i in indices];reference=module.scan(chosen);computed=combine([actual[i] for i in indices]);assert computed==reference
    controls.append(dict(name=label+' candidate multiset and finding/reviewed order preserved',status='PASS'))
for row in actual:
    assert (row['findings'],[tuple(v) for v in row['reviewed']])==module.scan([row['path']])
    controls.append(dict(name='Original per-file equality '+str(row['index']),status='PASS',input=pin(ROOT/row['path'])))
plan=dict(authority=authority(),rows=rows);report=dict(authority=authority(),rows=actual,complete=True,exit_code=0)
def reject(name,mutate):
    changed=copy.deepcopy(report);mutate(changed)
    try:validate(changed,plan)
    except (AssertionError,KeyError):controls.append(dict(name=name,status='REJECTED'));return
    raise AssertionError('Parallel protocol mutation accepted '+name)
reject('Dropped scanner helper file',lambda r:r['rows'].pop())
reject('Duplicate scanner file occurrence',lambda r:r['rows'].append(r['rows'][0]))
reject('Changed content key',lambda r:r['rows'][0].update(sha256='0'*64))
reject('Wrong source AST authority',lambda r:r['authority'].update(scan_ast_sha256='0'*64))
reject('Changed ROOT',lambda r:r['authority'].update(root='synthetic-other-root'))
reject('Changed hash-seed authority',lambda r:r['authority'].update(hash_seed='different'))
reject('Changed environment',lambda r:r['authority'].update(computername='synthetic-other-environment'))
reject('Abnormal child exit',lambda r:r.update(exit_code=2,complete=False))
old=actual[0]
before=pin(ROOT/old['path']);preimage=work/'preimages/0-before.txt';preimage.parent.mkdir(exist_ok=False)
with preimage.open('xb') as stream:stream.write((ROOT/old['path']).read_bytes())
assert pin(preimage)['sha256']==before['sha256']
(ROOT/old['path']).write_text('changed private proof fixture',encoding='utf-8')
try:validate(report,plan)
except AssertionError:controls.append(dict(name='Actual changed fixture invalidates stale content cache',status='REJECTED'))
else:raise AssertionError('Stale content cache accepted')
newrow=dict(index=0,**pin(ROOT/old['path']));rerun,new_receipts=run([newrow],work/'invalidated-execution',1)
assert combine(rerun)==module.scan([old['path']]);controls.append(dict(name='Changed input evaluates original positive fallback again',status='PASS'))
observed_before=next(r['plan'] for r in receipts if any(v['index']==0 for v in json.loads((ROOT/r['plan']['path']).read_bytes())['rows']))
save('private-proof-transition-v1.json',dict(status='CURRENT_SYNTHETIC_CONTROL_PREIMAGE_CAPTURED_BEFORE_DELIBERATE_MUTATION',
    rows=[dict(before=before,preimage=pin(preimage),after=pin(ROOT/old['path']),generator=pin(HERE/'prove_parallel_scan_base_v2.py'),observed_before=observed_before,observed_after=new_receipts[0]['plan'])],
    scope='New C14 stale-cache synthetic control only. Actual before bytes captured before mutation, never an original statistical reference or pre-edit production capture.',original_queries=0))
assert function_ast(POLICY,'scan')==function_ast(ROOT/'scripts/verify-04a14-publication.py','scan')
assert function_ast(POLICY,'git')==function_ast(ROOT/'scripts/verify-04a14-publication.py','git')
save('parallel-scanner-base-equivalence-v1.json',dict(status='ORIGINAL_FUNCTION_FILESET_ORDER_FINDINGS_REVIEWED_AND_ADVERSE_CONTROLS_PASS',reader=pin(HERE/'parallel_scan_v1.py'),proof=pin(HERE/'prove_parallel_scan_base_v2.py'),authority=clean(authority()),controls=controls,receipts=receipts+new_receipts,original_function_source_unchanged=True,assertions_changed=0,law='C6 already proves scan(list) equals ordered concatenation of original scan([path]) findings plus sorted reviewed union under the exact shared13/14 AST and git/environment. Independent helpers evaluate that same deterministic function for each content-keyed actual path. Restore reviewed tuple types, source/order/multiset and SHA-check every file before/after. Parent reuses the unchanged C6 cache/concatenation function. No candidate traversal or test assertion changes.',scheduler='Run non-scanner lanes first; only logical lane1 may launch five read-only helpers. Parent plus helpers <=6 worker processes. No other test lane is active during that phase.',retained_regex_controls=12068,regex_positive_fallback='Unchanged C6 RegexProxy; helper skip/fallback counters and exact raw reports retained',original_queries=0))
print(json.dumps(dict(controls=len(controls),all_pass=True,parallel_helpers=5,original_scan_assertions_changed=0)),flush=True)
