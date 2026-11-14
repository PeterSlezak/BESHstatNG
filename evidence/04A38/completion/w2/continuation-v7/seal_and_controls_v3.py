"""Version and exercise executable partial proof; never promote W2/W3."""
from common import *
import importlib.util,copy

def module(name,path):
    spec=importlib.util.spec_from_file_location(name,path);value=importlib.util.module_from_spec(spec);spec.loader.exec_module(value);return value

if sys.argv[1]=='seal':
    proof=json.loads((HERE/'independent-proof-final-v2.json').read_bytes())
    history=json.loads((HERE/'historical-ownership-v1.json').read_bytes())['members']
    new=set(proof['newly_supported_historical'])
    contracts=json.loads((HERE/'whole-body-contracts-v1.json').read_bytes())['contracts']
    rows=[]
    for h in history:
        if not h['proof']:continue
        contract=next((c for c in contracts if c['original_id']==h['canonical_original_id']),None)
        rows.append(dict(original_id=h['canonical_original_id'],current_ids=h['current_ids'],kind='C7_NEW_COMPLETE_SOURCE_CIL_BRANCH_INLINE' if h['canonical_original_id'] in new else 'C6_AUTHORITY_RETAINED_EXACT_ENTRY_VIEW_AND_UNCHANGED_BODY_BYTES_CIL',prior_proof=h['proof'] if h['canonical_original_id'] not in new else None,final_source_contract=contract['current_id'] if contract else None,authority='whole-body-contracts-v1.json' if contract else '../continuation-v6/proof-authority-crosswalk-v2.json',preservation='source-transitions-final-v1.json',whole_parent_supported=True))
    save('proof-authority-crosswalk-v1.json',dict(status='PARTIAL_FINAL_C7_AUTHORITY_PRIOR_EVIDENCE_IMMUTABLE',rows=rows,existing_callbacks_newly_supported=proof['newly_supported_existing_callbacks'],new_current_only=[c['current_id'] for c in contracts if c['new_support'] and c['original_id'] is None],original_queries=0))
    files=['constructed-context-scope-v1.json','proof-inputs-v1.json','whole-body-contracts-v1.json','callback-proofs-v1.json','current-universe-packet-v1.json','source-transitions-final-v1.json','transitive-compiled-context-v1.json','source-composition-v1.json','compatibility-packet-v1.json','ownership-summary-v1.json','historical-ownership-v1.json','current-ownership-v1.json','remaining-work-queue-v1.json','original-fragment-parent-crosswalk-v1.json','managed-Debug-identities-v1.json','managed-environment-correction-v1.json','python-discovery-inventory-v1.json','python-effective-identities-v1.json','python-first-failures-v1.json','python-worker-accounting-v1.json','node-identities-v1.json','validation-v1.json','readiness-v1.json','verify_validation_v1.py','verify_composition_v1.py','source_views.py','proof-authority-crosswalk-v1.json']
    files.extend('metadata-all/packet-v1-context-v3/'+origin+'-'+config+'.json' for origin in ['current','original'] for config in ['Debug','Release'])
    files.extend('inspection/packet-v1/Inspect38-'+c+'.json' for c in ['Debug','Release'])
    files.extend('../continuation-v6/'+name for name in ['verify_checkpoint_v5.py','verify_checkpoint_v2.py','verify_checkpoint_v1.py','verify_source_composition_v2.py','verify_text_domains_v1.py','whole-body-contracts-v1.json','review-seal-v5.json'])
    files.extend(r['current']['path'][len(HERE.relative_to(ROOT).as_posix())+1:] for r in json.loads((HERE/'compatibility-packet-v1.json').read_bytes())['results'])
    auth={name:sha((HERE/name).read_bytes()) for name in sorted(set(files))}
    source=(HERE/'verify_checkpoint_template_v5.py').read_text(encoding='utf-8');assert source.count('AUTH={}')==1
    source=source.replace('AUTH={}','AUTH='+repr(auth))
    with (HERE/'verify_c7_checkpoint_v1.py').open('x',encoding='utf-8',newline='\n') as stream:stream.write(source)
    validation=module('c7_validation_seal',HERE/'verify_validation_v1.py')
    load=lambda path:json.loads((ROOT/path).read_bytes())
    checked=validation.inspect(load,HERE.relative_to(ROOT).as_posix()+'/',lambda path:(ROOT/path).read_bytes())
    save('validation-independent-recount-v1.json',checked)
    save('review-seal-v1.json',dict(status='PARTIAL_W2_NOT_READY',reader=pin(HERE/'verify_c7_checkpoint_v1.py'),validation_reader=pin(HERE/'verify_validation_v1.py'),source_composition_reader=pin(HERE/'verify_composition_v1.py'),authenticated_inputs=len(auth),original_queries=0))
elif sys.argv[1]=='controls':
    reader=module('c7_controls_reader',HERE/'verify_c7_checkpoint_v1.py');baseline=reader.compute();rows=[]
    def rejected(name,action):
        try:action()
        except (ValueError,AssertionError,OSError,KeyError) as error:
            rows.append(dict(name=name,status='REJECTED',reason=clean(str(error))));return
        raise AssertionError('Mutation was accepted: '+name)
    def mutation(name,file,change):
        value=json.loads((HERE/file).read_bytes());change(value)
        rejected(name,lambda:reader.compute(overrides={reader.PREFIX+file:json.dumps(value,ensure_ascii=False).encode()}))
    mutation('False READY','ownership-summary-v1.json',lambda d:d.update(status='READY_04A38'))
    mutation('Dropped historical identity','historical-ownership-v1.json',lambda d:d['members'].pop())
    mutation('Duplicate current identity','current-ownership-v1.json',lambda d:d['members'].append(d['members'][0]))
    mutation('Missing callback','callback-proofs-v1.json',lambda d:d['rows'].pop())
    mutation('Changed callback captured field','callback-proofs-v1.json',lambda d:d['rows'][0]['configurations'][0]['declaring_type']['fields'].append(dict(name='unsupportedState')))
    mutation('Removed full source body contract','whole-body-contracts-v1.json',lambda d:d['contracts'].pop())
    mutation('Forged original canonical ID','proof-inputs-v1.json',lambda d:d['new_historical_ids'].append('invented'))
    mutation('Dropped original fragment','original-fragment-parent-crosswalk-v1.json',lambda d:d['rows'].pop())
    mutation('False module closure','current-ownership-v1.json',lambda d:next(r for r in d['support'] if not r['fully_reconciled']).update(fully_reconciled=True))
    mutation('Wrong export default','inspection/packet-v1/Inspect38-Debug.json',lambda d:d['exports'][0]['arguments'][0].update(optional=True))
    mutation('Changed frozen input record','compatibility-packet-v1.json',lambda d:d['results'][0]['inputs'].update(sha256='0'*64))
    mutation('Changed actual consumer context','metadata-all/packet-v1-context-v3/current-Debug.json',lambda d:next(r for r in d['methods'] if r['declaring_type']=='BESHStatNG.WorksheetFunctions.AssumptionsUDFs' and r['member_name']=='BOX_M').update(instructions=[]))
    mutation('Changed nonpass identity','python-first-failures-v1.json',lambda d:d['records'][0].update(identity='invented'))
    mutation('Dropped Python worker','python-worker-accounting-v1.json',lambda d:d['initial'].pop())
    mutation('Skipped Python method','python-effective-identities-v1.json',lambda d:d['entries'][0].update(outcome='SKIP'))
    mutation('Relabelled C6-introduced source-pin failure','python-first-failures-v1.json',lambda d:next(r for r in d['records'] if r['identity'].endswith('test_original_member_ids_and_body_pins_not_fabricated')).update(outcome='PASS'))
    transitions=json.loads((HERE/'source-transitions-final-v1.json').read_bytes())['files'];source=next(r for r in transitions if r['path'].endswith('UdfDataImportRangeReaders.vb'))
    path=source['path'];changed=(ROOT/path).read_bytes().replace(b'Return True',b'Return False',1)
    rejected('Actual current body byte mutation',lambda:reader.compute(overrides={path:changed}))
    path=source['preimage']['path'];rejected('Altered genuine C7 source preimage',lambda:reader.compute(overrides={path:(ROOT/path).read_bytes()+b'changed'}))
    path='evidence/04A38/completion/w2/continuation-v6/whole-body-contracts-v1.json';rejected('Missing old evidence',lambda:reader.compute(overrides={path:b'{}'}))
    replay=json.loads((HERE/'compatibility-packet-v1.json').read_bytes())['results'][0]
    path=replay['inputs']['path'];lines=(ROOT/path).read_bytes().splitlines();case=json.loads(lines[0]);case['culture']='C7_MUTATED_INPUT';lines[0]=json.dumps(case).encode()
    rejected('Actual frozen typed input mutation',lambda:reader.compute(overrides={path:b'\n'.join(lines)+b'\n'}))
    path='.test-rationalization-04A38.tmp/completion-w2-cont7/packet-v1/Debug/lib/BESHStat.Excel.Common.dll'
    rejected('Actual compiled consumer DLL byte mutation',lambda:reader.compute(overrides={path:(ROOT/path).read_bytes()+b'changed'}))
    metadata=json.loads((HERE/'metadata-all/packet-v1-context-v3/current-Debug.json').read_bytes())
    target=next(r['method'] for r in metadata['methods'] if r['declaring_type']=='BESHStatNG.WorksheetCompatibility.UdfNumericPreparation38' and r['member_name']=='TryBuildBoxCovariances')
    caller=next(r['method'] for r in metadata['methods'] if r['declaring_type']=='BESHStatNG.WorksheetFunctions.AssumptionsUDFs' and r['member_name']=='BOX_M')
    missing=dict(metadata,methods=[r for r in metadata['methods'] if r['method']!=target])
    rejected('Pure complete call graph missing real portable target',lambda:reader.closure(missing,[caller]))
    callback=json.loads((HERE/'callback-proofs-v1.json').read_bytes())['rows'][0]['configurations'][0]
    missing=dict(metadata,methods=[r for r in metadata['methods'] if r['method']!=callback['target']])
    rejected('Pure actual ldftn callback target missing',lambda:reader.closure(missing,[callback['parent_method']]))
    import verify_composition_v1 as composition
    path='BESHStat.Core/src/WorksheetFunctions/UdfNumericPreparation38.vb';raw=(ROOT/path).read_bytes();changed=raw.replace(b'If n < 2 Then Return False',b'If n < 3 Then Return False')
    assert changed!=raw
    rejected('Pure original parent source composition threshold mutation',lambda:composition.inspect(overrides={path:changed}))
    validator=module('c7_control_validation',HERE/'verify_validation_v1.py');prefix=reader.PREFIX
    load=lambda path:json.loads((ROOT/path).read_bytes());raw=lambda path:(ROOT/path).read_bytes()
    d=load(prefix+'python-worker-accounting-v1.json');d['initial'].pop()
    rejected('Pure validation reader dropped worker',lambda:validator.inspect(lambda p:d if p==prefix+'python-worker-accounting-v1.json' else load(p),prefix,raw))
    d=load(prefix+'python-first-failures-v1.json');d['records'][0]['identity']='invented'
    rejected('Pure validation reader changed nonpass identity',lambda:validator.inspect(lambda p:d if p==prefix+'python-first-failures-v1.json' else load(p),prefix,raw))
    rows.append(dict(name='Actual partial source/CIL/frozen/validation positive',status='PASS_NOT_READY',historical=baseline['historical'],current=baseline['current'],support=baseline['support']))
    save('independent-controls-v1.json',dict(controls=rows,passed=len(rows),failed=0,physical_protected_mutations=0,original_queries=0,scope='Sealed executable tamper controls plus direct semantic full-call-graph, whole-source-composition and independent validation-reader mutations. First attempts and source versions retained.'))
    print('C7 independent controls PASS',len(rows),flush=True)
else:raise ValueError(sys.argv[1])
