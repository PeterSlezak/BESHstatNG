"""Fail-closed inherited5661 plus bounded current explicit private preservation."""
import sys
from pathlib import Path
sys.path.insert(0,str(Path(__file__).resolve().parent/'continuation-v12'))
from common import *
TARGET=W2/'continuation-v12-retained-private-preservation-v1.json'

def check(row,overrides=None):
    if overrides is not None and row['path'] in overrides:
        data=overrides[row['path']]
        if data is None:raise FileNotFoundError(row['path'])
        actual=dict(path=row['path'],bytes=len(data),sha256=sha(data))
    else:actual=pin(ROOT/row['path'])
    assert actual=={key:row[key] for key in ['path','bytes','sha256']},'Private preservation mismatch '+row['path']

def phase():
    path=HERE/'private-proof-transition-v1.json';doc=json.loads(path.read_bytes())
    assert doc['status']=='CURRENT_SYNTHETIC_CONTROL_PREIMAGE_CAPTURED_BEFORE_DELIBERATE_MUTATION' and doc['original_queries']==0 and len(doc['rows'])==1
    for row in doc['rows']:
        for key in ['preimage','after','generator','observed_before','observed_after']:check(row[key])
        assert (row['before']['bytes'],row['before']['sha256'])==(row['preimage']['bytes'],row['preimage']['sha256'])
        before=next(r for r in json.loads((ROOT/row['observed_before']['path']).read_bytes())['rows'] if r['path']==row['before']['path'])
        after=json.loads((ROOT/row['observed_after']['path']).read_bytes())['rows'][0]
        assert {key:before[key] for key in ['path','bytes','sha256']}==row['before']
        assert {key:after[key] for key in ['path','bytes','sha256']}==row['after']
    return pin(path)

def generate():
    priorpath=W2/'continuation-v11-retained-private-preservation-v4.json';prior=json.loads(priorpath.read_bytes())
    reader=W2/'continuation-v11-retained-private-reader-v4.py';assert pin(reader)==prior['reader'] and prior['private_pin_count']==len(prior['private_pins'])==5661
    # Execute the actual documented reader with fresh isolated C11 imports.
    folder=PRIVATE/'preservation-prior-v11-v1';folder.mkdir(exist_ok=False)
    bootstrap='import sys,runpy;sys.path.insert(0,sys.argv[1]);sys.argv=[sys.argv[2],"--verify"];runpy.run_path(sys.argv[0],run_name="__main__")'
    args=[sys.executable,'-I','-B','-c',bootstrap,str(PREVIOUS),str(reader)];begin=time.monotonic();result=subprocess.run(args,cwd=ROOT,capture_output=True)
    for name in ['stdout','stderr']:
        with (folder/(name+'.log')).open('xb') as stream:stream.write(getattr(result,name))
    with (folder/'command.json').open('x',encoding='utf-8',newline='\n') as stream:json.dump(dict(command=args,cwd=str(ROOT),exit_code=result.returncode,seconds=time.monotonic()-begin,reader=pin(reader),prior=pin(priorpath)),stream,indent=2)
    assert result.returncode==0,'Actual prior private reader failed; preserve and STOP'
    current_phase=phase();rows={r['path']:r for r in prior['private_pins']};assert len(rows)==5661
    roots=[PRIVATE]
    for kind in ['commands','builds','executions']:roots.extend(sorted((RUNROOT/kind).glob('w2c12-*')))
    current=set()
    for root in roots:
        assert root.resolve().is_relative_to(RUNROOT.resolve())
        for path in sorted(root.rglob('*')):
            if path.is_file():
                row=pin(path);assert rows.setdefault(row['path'],row)==row;current.add(row['path'])
    archives={r['path']:{key:r[key] for key in ['path','bytes','sha256']} for r in prior['retained_prior_physical_archives']}
    indexpath=W2/'continuation-v12-review-parts-index-v1.json';parts=json.loads(indexpath.read_bytes())
    for part in parts['parts']:
        for record in [part,part['deterministic_reproduction']]:archives[record['path']]={key:record[key] for key in ['path','bytes','sha256']}
    for row in archives.values():check(row)
    giant=prior['retained_interrupted_aggregation'];check(giant)
    assert (giant['bytes'],giant['sha256'])==(4213129746,'3ed32a23d2a6ee59a64c80ce3977ca89b6dcc8572453222c957af1596ca302eb')
    for row in rows.values():check(row)
    small=next(r for r in rows.values() if 0<r['bytes']<1024);data=(ROOT/small['path']).read_bytes();controls=[]
    for name,value in [('changed private bytes',data+b'changed'),('missing private pin',None)]:
        try:check(small,{small['path']:value})
        except (AssertionError,OSError):controls.append(dict(name=name,status='REJECTED'))
        else:raise AssertionError('Private mutation accepted')
    doc=dict(status='C12_EXACT_PRIOR_CURRENT_PRIVATE_PRESERVATION_PASS_NOT_READY',reader=pin(Path(__file__)),previous=pin(priorpath),prior_reader=pin(reader),
        prior_actual_replay=dict(command=pin(folder/'command.json'),stdout=pin(folder/'stdout.log'),stderr=pin(folder/'stderr.log')),
        common_source=pin(HERE/'common.py'),current_synthetic_phase=current_phase,
        private_pins=[rows[key] for key in sorted(rows)],private_pin_count=len(rows),inherited_private_count=5661,
        new_current_private_count=len(current-{r['path'] for r in prior['private_pins']}),retained_prior_physical_archives=list(archives.values()),
        retained_interrupted_aggregation=giant,parts_index=pin(indexpath),controls=controls,original_queries=0,
        scope='All5661 inherited pins plus explicit bounded C12 run/command/build/execution files; old/new physical archives and giant retained. Historical ignored TestResults never traversed. Separately extracted review derivatives are indexed by their physical/logical union, not claimed as new authoritative test evidence.')
    with TARGET.open('x',encoding='utf-8',newline='\n') as stream:json.dump(doc,stream,ensure_ascii=False,indent=2);stream.write('\n')
    print(json.dumps(dict(receipt=pin(TARGET),pins=len(rows),new=doc['new_current_private_count'],archives=len(archives))))

def verify():
    doc=json.loads(TARGET.read_bytes());assert pin(Path(__file__))==doc['reader']
    assert len(doc['private_pins'])==doc['private_pin_count'] and len({r['path'] for r in doc['private_pins']})==doc['private_pin_count'] and doc['inherited_private_count']==5661
    for row in doc['private_pins']+doc['retained_prior_physical_archives']+[doc['retained_interrupted_aggregation']]:check(row)
    for key in ['previous','prior_reader','parts_index','common_source']:check(doc[key])
    assert phase()==doc['current_synthetic_phase']
    print(json.dumps(dict(status='C12_EXPLICIT_PRIVATE_REPLAY_PASS_NOT_READY',pins=doc['private_pin_count'],archives=len(doc['retained_prior_physical_archives']))))
if __name__=='__main__':verify() if '--verify' in sys.argv else generate()
