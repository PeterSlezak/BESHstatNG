"""Explicit inherited15166 pins/85 archives plus bounded current preservation.

Only explicitly listed historical files are opened. Ignored TestResults is never walked.
"""
import sys
from pathlib import Path
sys.path.insert(0,str(Path(__file__).resolve().parent/'continuation-v15'))
from common import *
TARGET=W2/'continuation-v15-retained-private-preservation-v2.json'
CACHE={}

def check(row,overrides=None):
    if overrides is not None and row['path'] in overrides:
        data=overrides[row['path']]
        if data is None:raise FileNotFoundError(row['path'])
        actual=dict(path=row['path'],bytes=len(data),sha256=sha(data))
    else:
        if row['path'] not in CACHE:CACHE[row['path']]=pin(ROOT/row['path'])
        actual=CACHE[row['path']]
    assert actual=={k:row[k] for k in ['path','bytes','sha256']},'Preservation mismatch '+row['path']

def phase():
    path=HERE/'private-proof-transition-v1.json';doc=json.loads(path.read_bytes())
    assert doc['status']=='CURRENT_SYNTHETIC_CONTROL_PREIMAGE_CAPTURED_BEFORE_DELIBERATE_MUTATION' and doc['original_queries']==0 and len(doc['rows'])==1
    for row in doc['rows']:
        for key in ['preimage','after','generator','observed_before','observed_after']:check(row[key])
        assert (row['before']['bytes'],row['before']['sha256'])==(row['preimage']['bytes'],row['preimage']['sha256'])
        before=next(r for r in json.loads((ROOT/row['observed_before']['path']).read_bytes())['rows'] if r['path']==row['before']['path'])
        after=json.loads((ROOT/row['observed_after']['path']).read_bytes())['rows'][0]
        assert {k:before[k] for k in ['path','bytes','sha256']}==row['before'] and {k:after[k] for k in ['path','bytes','sha256']}==row['after']
    return pin(path)

def inherited():
    priorpath=W2/'continuation-v14-retained-private-preservation-v1.json';prior=json.loads(priorpath.read_bytes())
    reader=W2/'continuation-v14-retained-private-reader-v2.py';assert pin(reader)==prior['reader']
    assert prior['private_pin_count']==len(prior['private_pins'])==15166 and len(prior['retained_prior_physical_archives'])==85
    entry=json.loads((HERE/'entry-authentication-v2.json').read_bytes())
    assert entry['status']=='ENTRY_GATES_PASS'
    gates=entry['executions']
    for label in ['c14-private-v2','c13-private-v2']:
        row=next(r for r in gates if r['label']==label);assert row['exit_code']==0
        for key in ['reader','receipt','stdout','stderr']:check(row[key])
    return priorpath,prior,reader

def generate():
    priorpath,prior,reader=inherited();rows={r['path']:r for r in prior['private_pins']};assert len(rows)==15166
    current=set();roots=[PRIVATE]
    for kind in ['commands','builds','executions']:roots.extend(sorted((RUNROOT/kind).glob('w2c15-*')))
    for base in roots:
        assert base.resolve().is_relative_to(RUNROOT.resolve())
        for path in sorted(base.rglob('*')):
            if path.is_file():
                row=pin(path);assert rows.setdefault(row['path'],row)==row;current.add(row['path'])
    archives={r['path']:{k:r[k] for k in ['path','bytes','sha256']} for r in prior['retained_prior_physical_archives']}
    indexpath=W2/'continuation-v15-review-parts-index-v2.json';index=json.loads(indexpath.read_bytes())
    for part in index['parts']:
        for row in [part,part['deterministic_reproduction']]:archives[row['path']]={k:row[k] for k in ['path','bytes','sha256']}
    excluded=json.loads((HERE/'review-exclusions-v1.json').read_bytes())
    retained_raw=excluded['current_raw_excluded']
    giant=prior['retained_interrupted_aggregation']
    assert (giant['bytes'],giant['sha256'])==(4213129746,'3ed32a23d2a6ee59a64c80ce3977ca89b6dcc8572453222c957af1596ca302eb')
    for row in list(rows.values())+list(archives.values())+retained_raw+[giant]:check(row)
    current_phase=phase();small=next(r for r in rows.values() if 0<r['bytes']<1024);data=(ROOT/small['path']).read_bytes();controls=[]
    for name,value in [('changed private bytes',data+b'changed'),('missing private file',None)]:
        try:check(small,{small['path']:value})
        except (AssertionError,OSError):controls.append(dict(name=name,status='REJECTED'))
        else:raise AssertionError('Invalid private evidence accepted')
    doc=dict(status='C15_EXACT_PRIOR_CURRENT_PRIVATE_PRESERVATION_PASS_NOT_READY',reader=pin(Path(__file__)),previous=pin(priorpath),prior_reader=pin(reader),prior_actual_replay=pin(HERE/'entry-authentication-review-v1.json'),common_source=pin(HERE/'common.py'),current_synthetic_phase=current_phase,
        private_pins=[rows[k] for k in sorted(rows)],private_pin_count=len(rows),inherited_private_count=15166,new_current_private_count=len(current-set(r['path'] for r in prior['private_pins'])),
        retained_prior_physical_archives=list(archives.values()),inherited_physical_archives=85,retained_interrupted_aggregation=giant,parts_index=pin(indexpath),current_review_excluded_raw=retained_raw,controls=controls,original_queries=0,
        scope='Exact inherited pins/archives remain untouched. Current enumeration is bounded to new C15 private run/build/execution/command roots only. Review-copy derivatives are verified by the complementary union, not substituted as raw test/original authority. Historical ignored TestResults never traversed.')
    with TARGET.open('x',encoding='utf-8',newline='\n') as f:json.dump(doc,f,indent=2,ensure_ascii=False);f.write('\n')
    print(json.dumps(dict(receipt=pin(TARGET),pins=len(rows),archives=len(archives),inherited_pins=15166,inherited_archives=85)))

def verify():
    doc=json.loads(TARGET.read_bytes());assert pin(Path(__file__))==doc['reader']
    assert len(doc['private_pins'])==doc['private_pin_count']==len({r['path'] for r in doc['private_pins']}) and doc['inherited_private_count']==15166
    prior=json.loads((ROOT/doc['previous']['path']).read_bytes());got={r['path']:r for r in doc['private_pins']}
    assert all(got.get(r['path'])==r for r in prior['private_pins'])
    old_archives={r['path']:r for r in doc['retained_prior_physical_archives']}
    assert all(old_archives.get(r['path'])==r for r in prior['retained_prior_physical_archives'])
    for row in doc['private_pins']+doc['retained_prior_physical_archives']+doc['current_review_excluded_raw']+[doc['retained_interrupted_aggregation']]:check(row)
    for key in ['previous','prior_reader','parts_index','common_source','prior_actual_replay']:check(doc[key])
    assert phase()==doc['current_synthetic_phase']
    print(json.dumps(dict(status='C15_EXPLICIT_PRIVATE_REPLAY_PASS_NOT_READY',pins=doc['private_pin_count'],archives=len(doc['retained_prior_physical_archives']),inherited_pins=15166,inherited_archives=85)))

if __name__=='__main__':verify() if '--verify' in sys.argv else generate()
