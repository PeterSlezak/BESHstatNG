"""Seal versioned fail-closed partial reader and exercise actual adverse predicates."""
from common import *
import importlib.util,copy
def module(name,path):
    spec=importlib.util.spec_from_file_location(name,path);value=importlib.util.module_from_spec(spec);spec.loader.exec_module(value);return value
if sys.argv[1]=='seal':
    proof=json.loads((HERE/'independent-proof-final-v1.json').read_bytes());history=json.loads((HERE/'historical-ownership-v2.json').read_bytes())['members'];new=set(proof['newly_supported_historical']);contracts=json.loads((HERE/'whole-body-contracts-v1.json').read_bytes())['contracts'];rows=[]
    for h in history:
        if not h['proof']:continue
        c=next((c for c in contracts if c['original_id']==h['canonical_original_id']),None)
        callback=h['canonical_original_id'] in json.loads((HERE/'proof-inputs-v3.json').read_bytes())['historical_callback_ids']
        rows.append(dict(original_id=h['canonical_original_id'],current_ids=h['current_ids'],kind='C8_COMPLETE_SOURCE_CIL_BRANCH_INLINE' if h['canonical_original_id'] in new else 'C7_AUTHORITY_RETAINED_EXACT_ENTRY_VIEW_BODY_CIL',prior_proof=h['proof'] if h['canonical_original_id'] not in new else None,final_source_contract=c['current_id'] if c else None,authority='historical-callback-contracts-v1.json' if callback else 'whole-body-contracts-v1.json' if c else '../continuation-v7/proof-authority-crosswalk-v1.json',preservation='source-transitions-final-v1.json',whole_parent_supported=True))
    save('proof-authority-crosswalk-v1.json',dict(status='PARTIAL_C8_AUTHORITY_PRIOR_EVIDENCE_IMMUTABLE',rows=rows,new_current_only=[c['current_id'] for c in contracts if c['new_support'] and c['original_id'] is None],new_current_callbacks=0,historical_callback_reconciliation=1,original_queries=0))
    files=['constructed-context-scope-v1.json','proof-inputs-v3.json','whole-body-contracts-v1.json','callback-proofs-v2.json','historical-callback-contracts-v1.json','current-universe-packet-v1.json','portable-universe-v1.json','portable-api-contracts-v1.json','dependency-source-universe-v1.json','numeric-copy-source-universe-v1.json','new-parent-fragment-crosswalk-v2.json','source-transitions-final-v1.json','implementation-plan-v1.json','transitive-compiled-context-v1.json','source-composition-v1.json','compatibility-packet-v1.json','ownership-summary-v1.json','historical-ownership-v2.json','current-ownership-v2.json','remaining-work-queue-v1.json','original-fragment-parent-crosswalk-v1.json','managed-Debug-identities-v1.json','managed-validation-v1.json','python-discovery-inventory-v1.json','python-effective-identities-v1.json','python-first-failures-v1.json','python-worker-accounting-v1.json','python-worker-receipts-v1.json','node-identities-v1.json','validation-v1.json','readiness-v1.json','verify_c8_validation_v1.py','verify_c8_composition_v2.py','source_views.py','proof-authority-crosswalk-v1.json','sample-size-extraction-v1.json','import-layout-extraction-v1.json','text-frontier-carryforward-v1.json','enum-bridge-source-domain-v1.json','unrelated-field-source-qualification-v1.json','toolchain-inputs-v1.json','python-optimization-authentication-v1.json','parallel_scan_v2.py','parallel-scanner-equivalence-v2.json','python_workers_v2.py','privacy-authority-v2.json','publication_privacy_v2.py','publication-privacy-equivalence-v3.json']
    files.extend('metadata-all/packet-v1-context-v3/'+o+'-'+c+'.json' for o in ['current','original'] for c in ['Debug','Release'])
    files.extend('inspection/packet-v1/Inspect38-'+c+'.json' for c in ['Debug','Release'])
    files.extend('../continuation-v7/'+n for n in ['verify_c7_checkpoint_v1.py','verify_composition_v1.py','verify_validation_v1.py','review-seal-v1.json','whole-body-contracts-v1.json'])
    files.extend(r['current']['path'][len(HERE.relative_to(ROOT).as_posix())+1:] for r in json.loads((HERE/'compatibility-packet-v1.json').read_bytes())['results'])
    auth={name:sha((HERE/name).read_bytes()) for name in sorted(set(files))};source=(HERE/'verify_c8_checkpoint_template_v6.py').read_text(encoding='utf-8');assert source.count('AUTH={}')==1
    source=source.replace('AUTH={}','AUTH='+repr(auth))
    with (HERE/'verify_c8_checkpoint_v1.py').open('x',encoding='utf-8',newline='\n') as f:f.write(source)
    reader=module('c8_final_seal',HERE/'verify_c8_checkpoint_v1.py');checked=reader.compute();save('sealed-reader-positive-v1.json',checked)
    save('review-seal-v1.json',dict(status='PARTIAL_W2_NOT_READY',reader=pin(HERE/'verify_c8_checkpoint_v1.py'),template=pin(HERE/'verify_c8_checkpoint_template_v6.py'),validation_reader=pin(HERE/'verify_c8_validation_v1.py'),composition_reader=pin(HERE/'verify_c8_composition_v2.py'),authenticated_inputs=len(auth),original_queries=0))
    print('C8 partial reader sealed and positive source/CIL/validation proof passed',flush=True)
elif sys.argv[1]=='controls':
    reader=module('c8_control_reader',HERE/'verify_c8_checkpoint_v1.py');baseline=reader.compute();rows=[]
    def rejected(name,action):
        try:action()
        except (ValueError,AssertionError,OSError,KeyError) as e:rows.append(dict(name=name,status='REJECTED',reason=clean(str(e))));return
        raise AssertionError('Mutation accepted '+name)
    def mutation(name,file,change):
        value=json.loads((HERE/file).read_bytes());change(value);rejected(name,lambda:reader.compute(overrides={reader.PREFIX+file:json.dumps(value,ensure_ascii=False).encode()}))
    mutation('False READY','ownership-summary-v1.json',lambda d:d.update(status='READY_04A38'))
    mutation('Dropped historical identity','historical-ownership-v2.json',lambda d:d['members'].pop())
    mutation('Duplicate current identity','current-ownership-v2.json',lambda d:d['members'].append(d['members'][0]))
    mutation('Missing nested callback','callback-proofs-v2.json',lambda d:d['rows'].pop())
    mutation('Wrong callback immediate caller','callback-proofs-v2.json',lambda d:d['rows'][-1]['configurations'][0].update(parent_method='invented'))
    mutation('Changed callback field','callback-proofs-v2.json',lambda d:d['rows'][0]['configurations'][0]['declaring_type']['fields'].append(dict(name='newState')))
    mutation('Dropped historical callback proof','historical-callback-contracts-v1.json',lambda d:d['rows'].clear())
    mutation('Dropped full source contract','whole-body-contracts-v1.json',lambda d:d['contracts'].pop())
    mutation('Forged original canonical ID','proof-inputs-v3.json',lambda d:d['new_historical_ids'].append('invented'))
    mutation('Dropped original fragment','original-fragment-parent-crosswalk-v1.json',lambda d:d['rows'].pop())
    mutation('Lost branch source correspondence','new-parent-fragment-crosswalk-v2.json',lambda d:d['rows'].pop())
    mutation('False unresolved container closure','current-ownership-v2.json',lambda d:next(s for s in d['support'] if not s['fully_reconciled']).update(fully_reconciled=True))
    mutation('Wrong export default','inspection/packet-v1/Inspect38-Debug.json',lambda d:d['exports'][0]['arguments'][0].update(optional=True))
    mutation('Changed frozen input authority','compatibility-packet-v1.json',lambda d:d['results'][0]['inputs'].update(sha256='0'*64))
    mutation('Changed real consumer','metadata-all/packet-v1-context-v3/current-Debug.json',lambda d:next(m for m in d['methods'] if m['declaring_type']=='BESHStatNG.WorksheetFunctions.SampleSizeUDFs' and m['member_name']=='SSIZE_TTEST_UNPAIRED').update(instructions=[]))
    mutation('Changed nonpass identity','python-first-failures-v1.json',lambda d:d['records'][0].update(identity='invented'))
    mutation('Dropped Python phase','python-worker-accounting-v1.json',lambda d:d['executions'].pop())
    mutation('Skipped Python method','python-effective-identities-v1.json',lambda d:d['entries'][0].update(outcome='SKIP'))
    mutation('Relabelled retained C6 source-pin fail','python-first-failures-v1.json',lambda d:next(r for r in d['records'] if r['identity'].endswith('test_original_member_ids_and_body_pins_not_fabricated')).update(outcome='PASS'))
    mutation('Dropped portable declaration','portable-api-contracts-v1.json',lambda d:d['contracts'].pop())
    for path,name in [('BESHstatNG/src/ExcelUDFs/SampleSizeUDFs.vb','Actual native source body'),('BESHStat.Excel.Common/src/Analysis/UdfSampleSizePreparation38.vb','Actual prepared source body')]:
        rejected(name+' changed',lambda p=path:reader.compute(overrides={p:(ROOT/p).read_bytes()+b'changed'}))
    t=next(t for t in json.loads((HERE/'source-transitions-final-v1.json').read_bytes())['files'] if t['before'] is not None);path=t['preimage']['path'];rejected('Altered authentic immediate preimage',lambda:reader.compute(overrides={path:(ROOT/path).read_bytes()+b'changed'}))
    path='evidence/04A38/completion/w2/continuation-v7/whole-body-contracts-v1.json';rejected('Missing old mandatory evidence',lambda:reader.compute(overrides={path:b'{}'}))
    replay=json.loads((HERE/'compatibility-packet-v1.json').read_bytes())['results'][0];path=replay['inputs']['path'];lines=(ROOT/path).read_bytes().splitlines();case=json.loads(lines[0]);case['culture']='C8_MUTATION';lines[0]=json.dumps(case).encode();changed=b'\n'.join(lines)+b'\n';rejected('Actual frozen typed input changed',lambda:reader.compute(overrides={path:changed}))
    path='.test-rationalization-04A38.tmp/completion-w2-cont8/packet-v1/Debug/lib/BESHStat.Excel.Common.dll';rejected('Actual compiled consumer DLL changed',lambda:reader.compute(overrides={path:(ROOT/path).read_bytes()+b'changed'}))
    meta=json.loads((HERE/'metadata-all/packet-v1-context-v3/current-Debug.json').read_bytes());target=next(m['method'] for m in meta['methods'] if m['declaring_type']=='BESHStatNG.ExcelCommon.UdfSampleSizePreparation38' and m['member_name']=='PrepareUnpaired');caller=next(m['method'] for m in meta['methods'] if m['declaring_type']=='BESHStatNG.WorksheetFunctions.SampleSizeUDFs' and m['member_name']=='SSIZE_TTEST_UNPAIRED')
    missing=dict(meta,methods=[m for m in meta['methods'] if m['method']!=target]);rejected('Pure graph missing actual preparation target',lambda:reader.closure(missing,[caller]))
    import verify_c8_composition_v2 as composition
    path='BESHStat.Excel.Common/src/Analysis/UdfSampleSizePreparation38.vb';raw=(ROOT/path).read_bytes();changed=raw.replace(b'If m.Value <= 0 Then',b'If m.Value < 0 Then',1);assert changed!=raw
    rejected('Pure full-source margin guard changed',lambda:composition.inspect(overrides={path:changed}))
    validator=module('c8_control_validation',HERE/'verify_c8_validation_v1.py');prefix=reader.PREFIX;load=lambda p:json.loads((ROOT/p).read_bytes());raw=lambda p:(ROOT/p).read_bytes()
    d=load(prefix+'python-worker-accounting-v1.json');d['executions'].pop();rejected('Pure validation dropped worker',lambda:validator.inspect(lambda p:d if p==prefix+'python-worker-accounting-v1.json' else load(p),prefix,raw))
    d=load(prefix+'python-first-failures-v1.json');d['records'][0]['identity']='invented';rejected('Pure validation changed failure identity',lambda:validator.inspect(lambda p:d if p==prefix+'python-first-failures-v1.json' else load(p),prefix,raw))
    rows.append(dict(name='Actual source/CIL/frozen/validation positive',status='PASS_NOT_READY',historical=baseline['historical'],current=baseline['current'],support=baseline['support']))
    save('independent-controls-v1.json',dict(controls=rows,passed=len(rows),failed=0,physical_protected_mutations=0,original_queries=0));print('C8 independent controls PASS',len(rows),flush=True)
else:raise ValueError(sys.argv[1])
