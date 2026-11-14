"""Versioned final partial reader and actual adversarial predicate controls."""
from common_v2 import *
import importlib.util
def module(name,path):
    spec=importlib.util.spec_from_file_location(name,path);value=importlib.util.module_from_spec(spec);spec.loader.exec_module(value);return value
if sys.argv[1]=='seal':
    names=['proof-inputs-v1.json','whole-body-contracts-v1.json','callback-proofs-v1.json','historical-callback-contracts-v1.json','portable-api-contracts-v1.json','current-universe-packet-v1.json','portable-universe-v1.json','dependency-source-universe-v1.json','declaration-shapes-v1.json','source-composition-v1.json','new-parent-fragment-crosswalk-v1.json','source-transitions-final-v1.json','implementation-plan-v1.json','transitive-compiled-context-v1.json','historical-ownership-v2.json','current-ownership-v2.json','ownership-summary-v2.json','new-closures-v3.json','remaining-work-queue-v1.json','original-fragment-parent-crosswalk-v2.json','compatibility-packet-v1.json','frozen-provenance-v1.json','readiness-v1.json','managed-Debug-identities-v1.json','managed-validation-v1.json','python-discovery-inventory-v1.json','python-worker-accounting-v1.json','python-worker-receipts-v1.json','python-effective-identities-v1.json','python-first-failures-v2.json','python-nonpass-investigation-v1.json','python-message-delta-investigation-v1.json','python-frozen-input-postcheck-v1.json','node-identities-v1.json','validation-v1.json','verify_c9_validation_v2.py','verify_c9_composition_v1.py','proof_kernel_v1.py','source_views.py','proof-authority-crosswalk-v1.json','constructed-context-scope-v1.json','return-metadata-Debug-v1.json','return-metadata-Release-v1.json','source-static-freeze-v1.json','gui-udf-consumer-crosswalk-v1.json','python-optimization-authentication-v1.json','parallel_scan_v1.py','parallel-scanner-equivalence-v1.json','python_workers_v1.py']
    names.extend(['privacy-findings-delta-v1.json','python-additional-message-source-proof-v1.json','failed-aggregation-attempt-v1.json','aggregation-correction-crosswalk-v1.json','publication-privacy-authority-v1.json','publication_privacy_c9_v1.py','publication-privacy-equivalence-v1.json','verify_c9_preservation_v1.py','verify_c9_publication_v1.py','final-source-validation-binding-v1.json','compatibility-fixture-crosswalk-v1.json'])
    names.extend('metadata-all/packet-v1-context-v5/'+o+'-'+c+'.json' for o in ['current','original'] for c in ['Debug','Release']);names.extend('inspection/packet-v1/Inspect38-'+c+'.json' for c in ['Debug','Release'])
    names.extend('../continuation-v8/'+n for n in ['verify_c8_checkpoint_v1.py','verify_c8_publication_v4.py','whole-body-contracts-v1.json','review-seal-v1.json','publication-manifest-v2.json'])
    names.extend(r['current']['path'][len(HERE.relative_to(ROOT).as_posix())+1:] for r in json.loads((HERE/'compatibility-packet-v1.json').read_bytes())['results'])
    auth={n:sha((HERE/n).read_bytes()) for n in sorted(set(names))};source=(HERE/'verify_c9_checkpoint_template_v3.py').read_text(encoding='utf-8');assert source.count('AUTH={}')==1;source=source.replace('AUTH={}','AUTH='+repr(auth))
    with (HERE/'verify_c9_checkpoint_v1.py').open('x',encoding='utf-8',newline='\n') as f:f.write(source)
    reader=module('c9_final_reader',HERE/'verify_c9_checkpoint_v1.py');result=reader.compute();save('sealed-reader-positive-v1.json',result);save('review-seal-v1.json',dict(status='PARTIAL_W2_NOT_READY',reader=pin(HERE/'verify_c9_checkpoint_v1.py'),template=pin(HERE/'verify_c9_checkpoint_template_v3.py'),authenticated_inputs=len(auth),original_queries=0));print('C9 sealed reader positive source/CIL/frozen/nonpass proof PASS_NOT_READY')
elif sys.argv[1]=='controls':
    reader=module('c9_control_reader',HERE/'verify_c9_checkpoint_v1.py');positive=reader.compute();rows=[]
    def reject(name,action):
        try:action()
        except (ValueError,AssertionError,OSError,KeyError) as error:rows.append(dict(name=name,status='REJECTED',reason=clean(str(error))));return
        except TypeError as error:
            if name!='Missing protected earlier reference':raise
            rows.append(dict(name=name,status='REJECTED',reason='Inherited C8 reader hashes the explicitly missing None input and raises TypeError; reader fails closed. First control-harness attempt retained.',exception_type=type(error).__name__));return
        raise AssertionError('Adverse control accepted '+name)
    def mutate(name,path,change):
        value=json.loads((HERE/path).read_bytes());change(value);reject(name,lambda:reader.compute(overrides={reader.PREFIX+path:json.dumps(value,ensure_ascii=False).encode()}))
    mutate('False READY','readiness-v1.json',lambda d:d.update(status='READY_04A38'))
    mutate('Native gate approval injection','readiness-v1.json',lambda d:d.update(N1='GO'))
    mutate('Forged supported total','ownership-summary-v2.json',lambda d:d['historical'].update(closed=704,OPEN=0))
    mutate('Duplicate historical identity','historical-ownership-v2.json',lambda d:d['members'].append(d['members'][0]))
    mutate('Missing current body','current-ownership-v2.json',lambda d:d['members'].pop())
    mutate('Unproved container closure','current-ownership-v2.json',lambda d:next(s for s in d['support'] if not s['fully_reconciled']).update(fully_reconciled=True))
    mutate('Dropped full parent','whole-body-contracts-v1.json',lambda d:d['contracts'].pop())
    mutate('Dropped callback','callback-proofs-v1.json',lambda d:d['rows'].pop())
    mutate('Wrong callback root','callback-proofs-v1.json',lambda d:d['rows'][0]['configurations'][0].update(parent_method='forged'))
    mutate('Changed callback field','callback-proofs-v1.json',lambda d:d['rows'][0]['configurations'][0]['declaring_type'].update(fields=[]))
    mutate('Dropped portable declaration','portable-api-contracts-v1.json',lambda d:d['contracts'].pop())
    mutate('Lost original branch','new-parent-fragment-crosswalk-v1.json',lambda d:d['rows'].pop())
    mutate('Lost original fragment universe','original-fragment-parent-crosswalk-v2.json',lambda d:d['rows'].pop())
    mutate('Wrong queue order','remaining-work-queue-v1.json',lambda d:d['first_members'].reverse())
    mutate('Forged original activity','proof-inputs-v1.json',lambda d:d.update(original_queries=1))
    mutate('Late recategorized pre-edit','frozen-provenance-v1.json',lambda d:d['configurations'][0]['late'][0].update(classification='PRE_EDIT_FROZEN'))
    mutate('Missing late authority','frozen-provenance-v1.json',lambda d:d['configurations'][0]['late'].pop())
    mutate('Missing frozen pair','compatibility-packet-v1.json',lambda d:d['results'].pop())
    mutate('Wrong frozen typed-input pin','compatibility-packet-v1.json',lambda d:d['results'][0]['inputs'].update(sha256='0'*64))
    mutate('Wrong export default','inspection/packet-v1/Inspect38-Debug.json',lambda d:d['exports'][0]['arguments'][0].update(optional=True))
    mutate('Erased managed FAIL','managed-Debug-identities-v1.json',lambda d:d['nonpasses'].pop())
    mutate('Dropped Python phase','python-worker-accounting-v1.json',lambda d:d['executions'].pop())
    mutate('Changed Python failure identity','python-first-failures-v2.json',lambda d:d['records'][0].update(identity='forged'))
    mutate('Source pin relabelled PASS','python-first-failures-v2.json',lambda d:next(r for r in d['records'] if r['identity'].endswith('test_original_member_ids_and_body_pins_not_fabricated')).update(outcome='PASS'))
    mutate('Skipped Python method','python-effective-identities-v1.json',lambda d:d['entries'][0].update(outcome='SKIP'))
    for path in ['BESHstatNG/src/ExcelUDFs/AgreementMethodUDFs.vb','BESHStat.Excel.Common/src/Analysis/UdfContingencyPreparation38.vb']:
        reject('Actual source altered '+path,lambda p=path:reader.compute(overrides={p:(ROOT/p).read_bytes()+b'changed'}))
    path='evidence/04A38/completion/w2/continuation-v8/whole-body-contracts-v1.json';reject('Missing protected earlier reference',lambda:reader.compute(overrides={path:None}))
    t=next(t for t in json.loads((HERE/'source-transitions-final-v1.json').read_bytes())['files'] if t['before']);path=t['preimage']['path'];reject('Altered immediate source preimage',lambda:reader.compute(overrides={path:(ROOT/path).read_bytes()+b'changed'}))
    path='.test-rationalization-04A38.tmp/completion-w2-cont9/packet-v1/Debug/lib/BESHStat.Excel.Common.dll';reject('Actual compiled consumer altered',lambda:reader.compute(overrides={path:(ROOT/path).read_bytes()+b'changed'}))
    r=json.loads((HERE/'compatibility-packet-v1.json').read_bytes())['results'][0];path=r['inputs']['path'];reject('Actual frozen input bytes changed',lambda:reader.compute(overrides={path:(ROOT/path).read_bytes()+b'changed'}))
    meta=json.loads((HERE/'metadata-all/packet-v1-context-v5/current-Debug.json').read_bytes());caller=next(r['method'] for r in meta['methods'] if (r['declaring_type'],r['member_name'])==('BESHStatNG.WorksheetFunctions.ContingencyTableUDFs','CT_CHI2'));target=next(r['method'] for r in meta['methods'] if (r['declaring_type'],r['member_name'])==('BESHStatNG.ExcelCommon.UdfContingencyPreparation38','ChiSquaredBody'));missing=dict(meta,methods=[r for r in meta['methods'] if r['method']!=target]);reject('Pure graph actual target removed',lambda:reader.closure(missing,[caller]))
    from verify_c9_composition_v1 import inspect as compose
    path='BESHStat.Excel.Common/src/Analysis/UdfContingencyPreparation38.vb';data=(ROOT/path).read_bytes();changed=data.replace(b'OrElse Not IsFinite',b'AndAlso Not IsFinite',1);assert changed!=data;reject('Pure source finite-guard connective changed',lambda:compose(overrides={path:changed}))
    from verify_c9_validation_v2 import inspect as validate
    prefix=reader.PREFIX;load=lambda p:json.loads((ROOT/p).read_bytes());raw=lambda p:(ROOT/p).read_bytes();changed=load(prefix+'python-worker-accounting-v1.json');changed['executions'].pop();reject('Pure validation worker removed',lambda:validate(lambda p:changed if p==prefix+'python-worker-accounting-v1.json' else load(p),prefix,raw))
    rows.append(dict(name='Actual source/CIL/frozen/identity positive',status='PASS_NOT_READY',historical=positive['historical'],current=positive['current'],support=positive['support']));save('independent-controls-v1.json',dict(controls=rows,passed=len(rows),failed=0,physical_protected_mutations=0,original_queries=0));print('C9 independent controls PASS',len(rows))
else:raise ValueError(sys.argv[1])
