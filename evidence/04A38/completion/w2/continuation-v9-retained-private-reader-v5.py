"""Explicit append-only private preservation after physical C9 publication."""
import hashlib,json
from pathlib import Path
ROOT=Path(__file__).resolve().parents[4];W2=Path(__file__).resolve().parent;HERE=W2/'continuation-v9'
def sha(data):return hashlib.sha256(data).hexdigest()
def pin(path):
    path=Path(path)
    with path.open('rb') as f:digest=hashlib.file_digest(f,'sha256').hexdigest()
    return dict(path=path.relative_to(ROOT).as_posix(),bytes=path.stat().st_size,sha256=digest)
def check(row,overrides=None):
    path=ROOT/row['path'];assert path.resolve().is_relative_to(ROOT.resolve())
    if overrides is None or row['path'] not in overrides:actual=pin(path)
    else:
        data=overrides[row['path']]
        if data is None:raise FileNotFoundError('Explicit missing private adverse control')
        actual=dict(path=row['path'],bytes=len(data),sha256=sha(data))
    assert actual==row,'Retained private pin changed '+row['path']

EXPECTED_PHASE={'status': 'EXACT_INTENTIONAL_SYNTHETIC_ADVERSE_INPUT_TRANSITION', 'proof': {'path': 'evidence/04A38/completion/w2/continuation-v9/parallel-scanner-equivalence-v1.json', 'bytes': 17872, 'sha256': '2f2d2fa09d449241ec971e04b7cae97909348d9429b396411af92a0bc51c8982'}, 'generator': {'path': 'evidence/04A38/completion/w2/continuation-v9/prove_parallel_scan_v1.py', 'bytes': 5735, 'sha256': 'ec2b1f7f32a9b3e94c2c33a4c159ea072f39ad7275e33286aa1a087518c28367'}, 'before': {'path': '.test-rationalization-04A38.tmp/completion-w2-cont9/parallel-scan-proof-v1/fixtures/0.txt', 'bytes': 17, 'sha256': '630b101c31ede722739b64407966997ecffc113af9715559e2240390c30fabd6'}, 'after': {'path': '.test-rationalization-04A38.tmp/completion-w2-cont9/parallel-scan-proof-v1/fixtures/0.txt', 'bytes': 29, 'sha256': '862340991bdbc503da2f9433b77067c206f05c4c8d4c26db79c111ad09c310a2'}, 'recovered_before': {'path': '.test-rationalization-04A38.tmp/completion-w2-cont9/parallel-scan-proof-v1/retained-before-adverse-0-v1.txt', 'bytes': 17, 'sha256': '630b101c31ede722739b64407966997ecffc113af9715559e2240390c30fabd6'}, 'recovery_provenance': 'SYNTHETIC_PRIVATE_ADVERSE_INPUT_PRE_STATE_RECOVERED_FROM_PINNED_GENERATOR_AND_OBSERVED_PRE_CONTROL_SHA_NOT_A_CAPTURED_SOURCE_PREIMAGE', 'after_observation': {'path': '.test-rationalization-04A38.tmp/completion-w2-cont9/parallel-scan-proof-v1/invalidated-execution/1/raw-result.json', 'bytes': 1687, 'sha256': '84e7f0a317c896fdc48091ecea7b846d40802eeae16c6326230768a7e47de2dd'}, 'first_private_preservation_attempt': {'path': '.test-rationalization-04A38.tmp/commands/w2c9-retained-private-check-v3/command.json', 'bytes': 597, 'sha256': '1a43f6f85a4b5abada84c6d2bae90c8c020673ddca077d5ff95868a9120a6060'}, 'first_stderr': {'path': '.test-rationalization-04A38.tmp/commands/w2c9-retained-private-check-v3/stderr.log', 'bytes': 838, 'sha256': '3ffdc98cce30eb28b2c2b0c578066673521867af91c593ee88db72afc7a013f6'}, 'scope': 'Exactly one new scratch fixture intentionally overwritten by authenticated cache invalidation control before its fresh fallback scan. Preserve actual current bytes, original observed hash, recovered synthetic constant and both raw scan phases. No historical protected file, source body, oracle or test assertion change.', 'original_queries': 0}
def validate_phase(document):
    assert document==EXPECTED_PHASE,'Unapproved synthetic fixture transition'
    proof=json.loads((ROOT/document['proof']['path']).read_bytes());assert pin(ROOT/document['proof']['path'])==document['proof'] and proof['proof']==document['generator']
    observed=next(r['input'] for r in proof['controls'] if r.get('input',{}).get('path')==document['before']['path']);assert observed==document['before']
    raw=json.loads((ROOT/document['after_observation']['path']).read_bytes());assert pin(ROOT/document['after_observation']['path'])==document['after_observation']
    after={k:raw['rows'][0][k] for k in ['path','bytes','sha256']};assert after==document['after'] and pin(ROOT/after['path'])==after
    assert pin(ROOT/document['recovered_before']['path'])==document['recovered_before']

def run():
    pubpath=W2/'continuation-v9-publication-receipt-v1.json';pub=json.loads(pubpath.read_bytes());assert pub['status']=='C9_PARTIAL_W2_NOT_READY_LIVE_COPY_DETERMINISTIC_PUBLICATION_VERIFIED'
    priorpath=W2/'continuation-v8-retained-private-preservation-v1.json';prior=json.loads(priorpath.read_bytes());rows={r['path']:r for r in prior['private_pins']}
    def collect(value):
        if isinstance(value,dict):
            if all(k in value for k in ['path','bytes','sha256']) and isinstance(value['path'],str) and value['path'].startswith('.test-rationalization-04A38.tmp/') and value['bytes'] is not None and value['sha256'] is not None:
                r={k:value[k] for k in ['path','bytes','sha256']};assert rows.setdefault(r['path'],r)==r,'Conflicting exact private pins'
            for item in value.values():collect(item)
        elif isinstance(value,list):
            for item in value:collect(item)
    # Only the bounded current evidence directory and explicit command names are enumerated.
    inputs=[p for p in sorted(HERE.glob('*.json')) if p.name!='python-first-failures-v1.json']
    for path in inputs:collect(json.loads(path.read_bytes()))
    commands=[]
    for folder in sorted((ROOT/'.test-rationalization-04A38.tmp/commands').glob('w2c9-*')):
        recordpath=folder/'command.json'
        if not recordpath.is_file():continue
        record=json.loads(recordpath.read_bytes());commands.append(pin(recordpath))
        for name in ['stdout','stderr']:
            r=pin(folder/(name+'.log'));assert r['sha256']==record[name+'_sha256'];assert rows.setdefault(r['path'],r)==r
        r=pin(recordpath);assert rows.setdefault(r['path'],r)==r
    transition_path=W2/'continuation-v9-private-proof-transition-v1.json';transition=json.loads(transition_path.read_bytes());assert transition['status']=='EXACT_INTENTIONAL_SYNTHETIC_ADVERSE_INPUT_TRANSITION';validate_phase(transition)
    for item in [transition['proof'],transition['generator'],transition['after_observation'],transition['after'],transition['recovered_before']]:check(item)
    p=transition['before']['path'];assert p=='.test-rationalization-04A38.tmp/completion-w2-cont9/parallel-scan-proof-v1/fixtures/0.txt' and rows[p]==transition['before']
    assert transition['recovered_before']['bytes']==transition['before']['bytes'] and transition['recovered_before']['sha256']==transition['before']['sha256']
    rows[p]=transition['after'];rows[transition['recovered_before']['path']]=transition['recovered_before']
    partial=json.loads((HERE/'failed-aggregation-attempt-v1.json').read_bytes())['partial'];check(partial)
    for r in rows.values():check(r)
    archives=[]
    for old in prior['retained_prior_physical_archives']:
        r={k:old[k] for k in ['path','bytes','sha256']};check(r);archives.append(r)
    for oldpub in [json.loads((W2/'continuation-v8-publication-receipt-v1.json').read_bytes()),pub]:
        for item in [oldpub['archive'],oldpub['archive']['deterministic_reproduction']]:
            r={k:item[k] for k in ['path','bytes','sha256']};check(r);archives.append(r)
    controls=[];r=next(r for r in rows.values() if 0<r['bytes']<10000);data=(ROOT/r['path']).read_bytes()
    for label,value in [('Changed exact private bytes',data+b'changed'),('Missing exact private file',None)]:
        try:check(r,{r['path']:value})
        except (AssertionError,OSError):controls.append(dict(name=label,status='REJECTED'))
        else:raise AssertionError('Private preservation adverse accepted')
    import copy
    for label,section,field,value in [('Forged original synthetic phase','before','path','forged'),('Forged after digest','after','sha256','0'*64),('Fake fallback observation root','after_observation','path','forged')]:
        changed=copy.deepcopy(transition);changed[section][field]=value
        try:validate_phase(changed)
        except (AssertionError,OSError):controls.append(dict(name=label,status='REJECTED'))
        else:raise AssertionError('Synthetic transition forgery accepted')
    result=dict(status='POST_PUBLICATION_EXACT_RETAINED_PRIVATE_HASH_PASS',reader=pin(Path(__file__)),previous=pin(priorpath),inputs=[pin(p) for p in inputs],private_pins=[rows[k] for k in sorted(rows)],private_pin_count=len(rows),retained_prior_physical_archives=archives,current_publication=pin(pubpath),current_command_records=commands,retained_interrupted_aggregation=partial,intentional_synthetic_fixture_transition=pin(transition_path),controls=controls,original_queries=0,scope='Explicit exact file pins only; no historical TestResults traversal. Includes prior exact private population and current source-bound raw command/TRX/worker/helper records. Does not claim every unlisted ignored byte was independently hashed. External to selected publication; final live Git check follows.')
    target=W2/'continuation-v9-retained-private-preservation-v1.json'
    with target.open('x',encoding='utf-8',newline='\n') as f:json.dump(result,f,ensure_ascii=False,indent=2);f.write('\n')
    print(json.dumps(dict(receipt=pin(target),private_pins=len(rows),archives=len(archives),controls=len(controls))))
if __name__=='__main__':run()
