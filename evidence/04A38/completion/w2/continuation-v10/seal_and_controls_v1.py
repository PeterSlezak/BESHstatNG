"""Versioned fail-closed source/body/validation reader and actual adverse controls."""
from common import *
import importlib.util,copy
def module(name,path):
    spec=importlib.util.spec_from_file_location(name,path);value=importlib.util.module_from_spec(spec);spec.loader.exec_module(value);return value
if sys.argv[1]=='seal':
    names=[p.relative_to(HERE).as_posix() for p in HERE.rglob('*') if p.is_file() and '__pycache__' not in p.parts and p.suffix in ['.json','.py','.txt','.tsv','.cs']]
    names.extend('../continuation-v9/'+n for n in ['whole-body-contracts-v1.json','review-seal-v1.json','verify_c9_checkpoint_v1.py','verify_c9_publication_v1.py','publication-manifest-v1.json'])
    for r in json.loads((HERE/'compatibility-packet-v2.json').read_bytes())['results']:
        for k in ['inputs','original','recorder']:names.append(os.path.relpath(ROOT/r[k]['path'],HERE).replace('\\','/'))
    auth={n:sha((HERE/n).read_bytes()) for n in sorted(set(names))};source=(HERE/'verify_c10_checkpoint_template_v5.py').read_text(encoding='utf-8');assert source.count('AUTH={}')==1;source=source.replace('AUTH={}','AUTH='+repr(auth))
    with (HERE/'verify_c10_checkpoint_v1.py').open('x',encoding='utf-8',newline='\n') as f:f.write(source)
    reader=module('c10_final_reader',HERE/'verify_c10_checkpoint_v1.py');result=reader.compute();save('sealed-reader-positive-v1.json',result);save('review-seal-v1.json',dict(status='PARTIAL_W2_NOT_READY',reader=pin(HERE/'verify_c10_checkpoint_v1.py'),template=pin(HERE/'verify_c10_checkpoint_template_v5.py'),authenticated_inputs=len(auth),original_queries=0));print('C10 actual source/CIL/frozen/validation reader PASS_NOT_READY',len(auth))
elif sys.argv[1]=='controls':
    reader=module('c10_control_reader',HERE/'verify_c10_checkpoint_v1.py');rows=[]
    def reject(name,action):
        try:action()
        except (ValueError,AssertionError,OSError,KeyError,TypeError) as e:rows.append(dict(name=name,status='REJECTED',reason=clean(str(e)),exception=type(e).__name__));return
        raise AssertionError('Adverse accepted '+name)
    def mutate(name,path,change):
        value=json.loads((HERE/path).read_bytes());change(value);reject(name,lambda:reader.compute(overrides={reader.PREFIX+path:json.dumps(value,ensure_ascii=False).encode()}))
    for name,path,change in [
        ('false READY','readiness-v1.json',lambda d:d.update(status='READY_04A38')),
        ('native approval','readiness-v1.json',lambda d:d.update(N1='GO')),
        ('forged totals','ownership-summary-v2.json',lambda d:d['historical'].update(closed=704,OPEN=0)),
        ('duplicate historical','historical-ownership-v2.json',lambda d:d['members'].append(d['members'][0])),
        ('missing current','current-ownership-v2.json',lambda d:d['members'].pop()),
        ('unproved container','current-ownership-v2.json',lambda d:next(s for s in d['support'] if not s['fully_reconciled']).update(fully_reconciled=True)),
        ('dropped parent','whole-body-contracts-v1.json',lambda d:d['contracts'].pop()),
        ('dropped callback','callback-proofs-v1.json',lambda d:d['rows'].pop()),
        ('fake callback root','callback-proofs-v1.json',lambda d:d['rows'][0]['configurations'][0].update(parent_method='forged')),
        ('changed host field','host-cache-contexts-v1.json',lambda d:d['rows'][0]['configurations'][0]['type'].update(fields=[])),
        ('dropped portable body','portable-api-contracts-v1.json',lambda d:d['contracts'].pop()),
        ('lost branch','new-parent-fragment-crosswalk-v1.json',lambda d:d['rows'].pop()),
        ('wrong queue','remaining-work-queue-v1.json',lambda d:d['first_members'].reverse()),
        ('new original query','proof-inputs-v1.json',lambda d:d.update(original_queries=1)),
        ('late relabelled preedit','frozen-provenance-v1.json',lambda d:d['configurations'][0]['late'][0].update(classification='PRE_EDIT_FROZEN')),
        ('missing frozen pair','compatibility-packet-v2.json',lambda d:d['results'].pop()),
        ('wrong typed input','compatibility-packet-v2.json',lambda d:d['results'][0]['inputs'].update(sha256='0'*64)),
        ('wrong export default','inspection/packet-v2/Inspect38-Debug.json',lambda d:d['exports'][0]['arguments'][0].update(optional=True)),
        ('erased managed FAIL','managed-Debug-identities-v1.json',lambda d:d['nonpasses'].pop()),
        ('dropped worker','python-worker-accounting-v1.json',lambda d:d['executions'].pop()),
        ('changed failure ID','python-first-failures-v2.json',lambda d:d['records'][0].update(identity='forged')),
        ('source-pin waived','python-first-failures-v2.json',lambda d:next(r for r in d['records'] if r['identity'].endswith('test_original_member_ids_and_body_pins_not_fabricated')).update(outcome='PASS')),
        ('skipped Python parent','python-effective-identities-v1.json',lambda d:d['entries'][0].update(outcome='SKIP'))]:mutate(name,path,change)
    for path in ['BESHstatNG/src/ExcelUDFs/ContingencyTableUDFs.vb','BESHStat.Excel.Common/src/Results/UdfMultivariateReportPreparation38.vb']:
        reject('actual production bytes '+path,lambda p=path:reader.compute(overrides={p:(ROOT/p).read_bytes()+b'changed'}))
    old='evidence/04A38/completion/w2/continuation-v9/whole-body-contracts-v1.json';reject('missing protected prior authority',lambda:reader.compute(overrides={old:None}))
    t=next(t for t in json.loads((HERE/'source-transitions-final-v1.json').read_bytes())['files'] if t['before']);path=t['preimage']['path'];reject('changed preimage',lambda:reader.compute(overrides={path:(ROOT/path).read_bytes()+b'changed'}))
    r=json.loads((HERE/'compatibility-packet-v2.json').read_bytes())['results'][0];path=r['inputs']['path'];reject('actual frozen input changed',lambda:reader.compute(overrides={path:(ROOT/path).read_bytes()+b'changed'}))
    meta=json.loads((HERE/'metadata-all/packet-v2-context-v5/current-Debug.json').read_bytes());caller=next(m['method'] for m in meta['methods'] if (m['declaring_type'],m['member_name'])==('BESHStatNG.WorksheetFunctions.ContingencyTableUDFs','CT_MCNEMAR_EXACT'));target=next(m['method'] for m in meta['methods'] if (m['declaring_type'],m['member_name'])==('BESHStatNG.ExcelCommon.UdfContingencyPreparation38','McNemarBody'));missing=dict(meta,methods=[r for r in meta['methods'] if r['method']!=target]);reject('pure graph real target removed',lambda:reader.closure(missing,[caller]))
    from verify_c10_composition_v1 import inspect as compose
    path='BESHStat.Excel.Common/src/Analysis/UdfContingencyPreparation38.vb';data=(ROOT/path).read_bytes();changed=data.replace(b'OrElse res.Item2 Is Nothing OrElse Not IsFinite(res.Item1.Pvalue)',b'OrElse res.Item2 Is Nothing AndAlso Not IsFinite(res.Item1.Pvalue)',1);assert changed!=data;reject('pure source short circuit changed',lambda:compose(overrides={path:changed}))
    from verify_c10_validation_v1 import inspect as validate
    prefix=reader.PREFIX;load=lambda p:json.loads((ROOT/p).read_bytes());raw=lambda p:(ROOT/p).read_bytes();altered=load(prefix+'python-worker-accounting-v1.json');altered['executions'].pop();reject('pure validation dropped worker',lambda:validate(lambda p:altered if p==prefix+'python-worker-accounting-v1.json' else load(p),prefix,raw))
    save('independent-controls-v1.json',dict(controls=rows,passed=len(rows),failed=0,physical_protected_mutations=0,original_queries=0));print('C10 controls rejected',len(rows))
else:raise ValueError(sys.argv[1])
