"""External post-seal exact private pins; no directory traversal or original run."""
import hashlib,json,sys
from pathlib import Path
ROOT=Path(__file__).resolve().parents[4];W2=Path(__file__).resolve().parent;HERE=W2/'continuation-v8'
def sha(data):return hashlib.sha256(data).hexdigest()
def pin(path):
    path=Path(path)
    with path.open('rb') as f:digest=hashlib.file_digest(f,'sha256').hexdigest()
    return dict(path=path.relative_to(ROOT).as_posix(),bytes=path.stat().st_size,sha256=digest)
def check(row,override=None):
    path=ROOT/row['path'];assert path.resolve().is_relative_to(ROOT.resolve())
    if override is None:actual=pin(path)
    else:
        if row['path'] not in override:actual=pin(path)
        else:
            data=override[row['path']]
            if data is None:raise FileNotFoundError('Explicit missing private control')
            actual=dict(path=row['path'],bytes=len(data),sha256=sha(data))
    assert actual==row,'Retained private pin changed '+row['path']
def run():
    publication=W2/'continuation-v8-publication-receipt-v1.json';pub=json.loads(publication.read_bytes());assert pub['status']=='C8_PARTIAL_W2_NOT_READY_LIVE_COPY_DETERMINISTIC_PUBLICATION_VERIFIED'
    inputs=[W2/'continuation-v7'/n for n in ['v6-raw-command-preservation-v1.json','v6-explicit-private-and-seal-pins-v1.json','v6-private-diagnostic-entry-v1.json']]
    inputs += [HERE/n for n in ['v7-private-command-preservation-v1.json','command-receipts-v1.json','python-worker-accounting-v1.json','managed-Debug-identities-v1.json','publication-import-controls-v1.json']]
    rows={}
    def collect(value):
        if isinstance(value,dict):
            if all(k in value for k in ['path','bytes','sha256']) and isinstance(value['path'],str) and value['path'].startswith('.test-rationalization-04A38.tmp/') and value['bytes'] is not None and value['sha256'] is not None:
                row={k:value[k] for k in ['path','bytes','sha256']};old=rows.setdefault(row['path'],row);assert old==row,'Conflicting private source pin'
            for v in value.values():collect(v)
        elif isinstance(value,list):
            for v in value:collect(v)
    for path in inputs:collect(json.loads(path.read_bytes()))
    # The final copied-reader command has its own post-inventory raw streams.
    cmd=ROOT/'.test-rationalization-04A38.tmp/commands/w2c8-publication-copied-reader-v1'
    record=json.loads((cmd/'command.json').read_bytes());assert record['exit_code']==0
    for name in ['stdout','stderr']:
        row=pin(cmd/(name+'.log'));assert row['sha256']==record[name+'_sha256'];rows[row['path']]=row
    row=pin(cmd/'command.json');rows[row['path']]=row
    for label in ['positive','wrong']:
        folder=ROOT/('.test-rationalization-04A38.tmp/commands/w2c8-publication-import-'+label+'-v1');record=json.loads((folder/'command.json').read_bytes());assert record['exit_code']==0
        for name in ['stdout','stderr']:
            row=pin(folder/(name+'.log'));assert row['sha256']==record[name+'_sha256'];rows[row['path']]=row
        row=pin(folder/'command.json');rows[row['path']]=row
    for row in rows.values():check(row)
    archives=[]
    for input_path in [HERE/'v7-publication-entry-v1.json',W2/'continuation-v7/v6-publication-entry-v1.json']:
        doc=json.loads(input_path.read_bytes());archive=doc['archive']
        if 'path' not in archive:
            archive=dict(archive,path='.test-rationalization-04A38.tmp/completion-w2-cont6/publication-v1/'+archive['name'])
        targets=[('main',archive)]
        if 'deterministic_reproduction' in archive:targets.append(('reproduction',archive['deterministic_reproduction']))
        for label,item in targets:
            row={k:item[k] for k in ['path','bytes','sha256']};check(row);archives.append(dict(role=label,**row))
    for item in [pub['archive'],pub['archive']['deterministic_reproduction']]:check({k:item[k] for k in ['path','bytes','sha256']})
    controls=[];row=next(r for r in rows.values() if r['bytes']<10000);raw=(ROOT/row['path']).read_bytes()
    for label,value in [('altered raw private bytes',raw+b'changed'),('missing raw private file',None)]:
        try:check(row,{row['path']:value})
        except (AssertionError,OSError):controls.append(dict(name=label,status='REJECTED'))
        else:raise AssertionError('Private preservation mutation accepted')
    result=dict(status='POST_PUBLICATION_EXACT_RETAINED_PRIVATE_HASH_PASS',reader=pin(Path(__file__)),inputs=[pin(p) for p in inputs],private_pins=[rows[k] for k in sorted(rows)],private_pin_count=len(rows),retained_prior_physical_archives=archives,current_publication=pin(publication),controls=controls,scope='Explicit existing file pins only. No broad ignored-directory traversal, no rewrite of raw transcripts, no original source execution. Does not assert every unlisted ignored byte was independently hashed. External to the immutable selected review manifest; final live Git receipt follows this check.',original_queries=0)
    output=W2/'continuation-v8-retained-private-preservation-v1.json'
    with output.open('x',encoding='utf-8',newline='\n') as f:json.dump(result,f,ensure_ascii=False,indent=2);f.write('\n')
    print(json.dumps(dict(receipt=pin(output),private_pins=len(rows),retained_archives=len(archives),controls=len(controls))))
if __name__=='__main__':run()
