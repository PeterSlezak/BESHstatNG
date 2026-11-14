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
    result=dict(status='POST_PUBLICATION_EXACT_RETAINED_PRIVATE_HASH_PASS',reader=pin(Path(__file__)),previous=pin(priorpath),inputs=[pin(p) for p in inputs],private_pins=[rows[k] for k in sorted(rows)],private_pin_count=len(rows),retained_prior_physical_archives=archives,current_publication=pin(pubpath),current_command_records=commands,retained_interrupted_aggregation=partial,controls=controls,original_queries=0,scope='Explicit exact file pins only; no historical TestResults traversal. Includes prior exact private population and current source-bound raw command/TRX/worker/helper records. Does not claim every unlisted ignored byte was independently hashed. External to selected publication; final live Git check follows.')
    target=W2/'continuation-v9-retained-private-preservation-v1.json'
    with target.open('x',encoding='utf-8',newline='\n') as f:json.dump(result,f,ensure_ascii=False,indent=2);f.write('\n')
    print(json.dumps(dict(receipt=pin(target),private_pins=len(rows),archives=len(archives),controls=len(controls))))
if __name__=='__main__':run()
