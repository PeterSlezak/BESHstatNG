"""Fail-closed successor for explicit prior/current private preservation only."""
import sys
from pathlib import Path
sys.path.insert(0,str(Path(__file__).resolve().parent/'continuation-v11'))
from common import *
import importlib.util

TARGET=W2/'continuation-v11-retained-private-preservation-v4.json'

def check(row,overrides=None):
    if overrides is not None and row['path'] in overrides:
        data=overrides[row['path']]
        if data is None:raise FileNotFoundError(row['path'])
        actual=dict(path=row['path'],bytes=len(data),sha256=sha(data))
    else:actual=pin(ROOT/row['path'])
    assert actual=={k:row[k] for k in ['path','bytes','sha256']},'Private preservation mismatch '+row['path']

def previous():
    receipt=W2/'continuation-v10-retained-private-preservation-v2.json'
    doc=json.loads(receipt.read_bytes());reader=W2/'continuation-v10-retained-private-reader-v2.py'
    assert pin(reader)==doc['reader'] and len(doc['private_pins'])==doc['private_pin_count']==2545
    spec=importlib.util.spec_from_file_location('c11_authoritative_v10_private',reader)
    module=importlib.util.module_from_spec(spec);spec.loader.exec_module(module)
    for row in doc['private_pins']+doc['retained_prior_physical_archives']+[doc['retained_interrupted_aggregation']]:module.check(row)
    # Retain the actual phase authority, not a recorded PASS label.
    phases=json.loads((HERE/'../continuation-v10/private-proof-transitions-v1.json').read_bytes())['rows']
    for r in phases:
        for name in ['after','recovered_before','generator','observed_before','observed_after']:module.check(r[name])
        before=next(v for v in json.loads((ROOT/r['observed_before']['path']).read_bytes())['rows'] if v['path']==r['before']['path'])
        assert {k:before[k] for k in ['path','bytes','sha256']}==r['before']
        after=json.loads((ROOT/r['observed_after']['path']).read_bytes())['rows'][0]
        assert {k:after[k] for k in ['path','bytes','sha256']}==r['after']
    oldpath=W2/'continuation-v9-retained-private-reader-v6.py'
    spec=importlib.util.spec_from_file_location('c11_prior_phase_reader',oldpath)
    old=importlib.util.module_from_spec(spec);spec.loader.exec_module(old)
    old.validate_phase(json.loads((W2/'continuation-v9-private-proof-transition-v1.json').read_bytes()))
    return receipt,doc


def validate_current_phase():
    path=W2/'continuation-v11-private-proof-transition-v1.json'
    doc=json.loads(path.read_bytes())
    assert len(doc['rows'])==1 and doc['original_queries']==0
    for row in doc['rows']:
        for key in ['after','recovered_before','generator','observed_before','observed_after','proof']:check(row[key])
        assert (row['recovered_before']['bytes'],row['recovered_before']['sha256'])==(row['before']['bytes'],row['before']['sha256'])
        observed=next(r for r in json.loads((ROOT/row['observed_before']['path']).read_bytes())['rows'] if r['index']==0)
        assert {k:observed[k] for k in ['path','bytes','sha256']}==row['before']
        changed=json.loads((ROOT/row['observed_after']['path']).read_bytes())['rows'][0]
        assert {k:changed[k] for k in ['path','bytes','sha256']}==row['after']
        assert row['classification']=='DELIBERATE_CURRENT_SYNTHETIC_CONTROL_PRESTATE_RECOVERED_FROM_GENERATOR_AND_OBSERVED_HASH_NOT_PRE_EDIT'
    return pin(path)

def generate():
    phase=validate_current_phase();priorpath,prior=previous();rows={r['path']:r for r in prior['private_pins']}
    roots=[PRIVATE]
    roots.extend(sorted((RUNROOT/'commands').glob('w2c11-*')))
    roots.extend(sorted((RUNROOT/'builds').glob('w2c11-*')))
    roots.extend(sorted((RUNROOT/'executions').glob('w2c11-*')))
    # The extracted review copy is an already indexed byte-exact derivative;
    # its payload union is checked by the copied reader/ZIP index, not counted
    # thousands of times as new authoritative private test output.
    skip=ROOT/'.c11p.tmp'
    current=[]
    for folder in roots:
        assert folder.resolve().is_relative_to(RUNROOT.resolve())
        for path in sorted(folder.rglob('*')):
            if path.is_file() and not path.is_relative_to(skip):
                row=pin(path);assert rows.setdefault(row['path'],row)==row;current.append(row['path'])
    archive_rows={r['path']:{k:r[k] for k in ['path','bytes','sha256']} for r in prior['retained_prior_physical_archives']}
    parts=json.loads((W2/'continuation-v11-review-parts-index-v3.json').read_bytes())
    for part in parts['parts']:
        for row in [part,part['deterministic_reproduction']]:archive_rows[row['path']]={k:row[k] for k in ['path','bytes','sha256']}
    for row in archive_rows.values():check(row)
    giant=prior['retained_interrupted_aggregation'];check(giant)
    assert (giant['bytes'],giant['sha256'])==(4213129746,'3ed32a23d2a6ee59a64c80ce3977ca89b6dcc8572453222c957af1596ca302eb')
    for row in rows.values():check(row)
    small=next(r for r in rows.values() if 0<r['bytes']<1024);data=(ROOT/small['path']).read_bytes();controls=[]
    for name,value in [('changed private bytes',data+b'changed'),('missing private pin',None)]:
        try:check(small,{small['path']:value})
        except (AssertionError,OSError):controls.append(dict(name=name,status='REJECTED'))
        else:raise AssertionError('Private mutation accepted')
    doc=dict(status='C11_EXACT_PRIOR_CURRENT_PRIVATE_PRESERVATION_PASS_NOT_READY',reader=pin(Path(__file__)),
        previous=pin(priorpath),prior_reader=prior['reader'],current_synthetic_phase=phase,common_source=pin(HERE/'common.py'),private_pins=[rows[k] for k in sorted(rows)],
        private_pin_count=len(rows),inherited_private_count=2545,new_current_private_count=len(set(current)-{r['path'] for r in prior['private_pins']}),
        retained_prior_physical_archives=list(archive_rows.values()),retained_interrupted_aggregation=giant,
        parts_index=pin(W2/'continuation-v11-review-parts-index-v3.json'),controls=controls,original_queries=0,
        scope='Explicit prior2545 plus bounded C11 run/command/build/execution files only. All old archives and giant retained. Historical root TestResults never traversed. Exact extracted review union is independently checked by the part index/copied reader; no claim to exhaustive unlisted ignored preservation.')
    with TARGET.open('x',encoding='utf-8',newline='\n') as stream:json.dump(doc,stream,ensure_ascii=False,indent=2);stream.write('\n')
    print(json.dumps(dict(receipt=pin(TARGET),pins=len(rows),new=doc['new_current_private_count'],archives=len(archive_rows))))

def verify():
    doc=json.loads(TARGET.read_bytes());assert pin(Path(__file__))==doc['reader']
    assert len(doc['private_pins'])==doc['private_pin_count'] and doc['inherited_private_count']==2545
    for row in doc['private_pins']+doc['retained_prior_physical_archives']+[doc['retained_interrupted_aggregation']]:check(row)
    check(doc['parts_index']);check(doc['previous']);check(doc['prior_reader']);check(doc['common_source']);assert validate_current_phase()==doc['current_synthetic_phase']
    print(json.dumps(dict(status='C11_EXPLICIT_PRIVATE_PIN_REPLAY_PASS_NOT_READY',pins=doc['private_pin_count'],archives=len(doc['retained_prior_physical_archives']))))

if __name__=='__main__':
    if '--verify' in sys.argv:verify()
    else:generate()
