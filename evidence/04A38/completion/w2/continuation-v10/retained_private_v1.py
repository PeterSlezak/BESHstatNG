"""Independent post-publication private pins; explicit phase-aware synthetic controls."""
from common import *
import importlib.util

def check(row,overrides=None):
    if overrides and row['path'] in overrides:
        data=overrides[row['path']]
        if data is None:raise FileNotFoundError(row['path'])
        actual=dict(path=row['path'],bytes=len(data),sha256=sha(data))
    else:actual=pin(ROOT/row['path'])
    assert actual=={k:row[k] for k in ['path','bytes','sha256']},'Private preservation changed '+row['path']

def run():
    priorpath=W2/'continuation-v9-retained-private-preservation-v2.json';prior=json.loads(priorpath.read_bytes());rows={r['path']:r for r in prior['private_pins']}
    spec=importlib.util.spec_from_file_location('c10_authoritative_prior_private',W2/'continuation-v9-retained-private-reader-v6.py');old=importlib.util.module_from_spec(spec);spec.loader.exec_module(old)
    assert pin(ROOT/prior['reader']['path'])==prior['reader'];old.validate_phase(json.loads((W2/'continuation-v9-private-proof-transition-v1.json').read_bytes()))
    for r in rows.values():old.check(r)
    phases=json.loads((HERE/'private-proof-transitions-v1.json').read_bytes())['rows'];phase_by={r['before']['path']:r for r in phases}
    for r in phases:
        check(r['after']);check(r['recovered_before']);check(r['generator']);check(r['observed_before']);check(r['observed_after'])
        before=next(v for v in json.loads((ROOT/r['observed_before']['path']).read_bytes())['rows'] if v['path']==r['before']['path']);assert {k:before[k] for k in ['path','bytes','sha256']}==r['before']
        after=json.loads((ROOT/r['observed_after']['path']).read_bytes())['rows'][0];assert {k:after[k] for k in ['path','bytes','sha256']}==r['after']
    conflicts=[]
    def collect(value):
        if isinstance(value,dict):
            if all(k in value for k in ['path','bytes','sha256']) and isinstance(value['path'],str) and value['path'].startswith('.test-rationalization-04A38.tmp/') and value['bytes'] is not None and value['sha256'] is not None:
                r={k:value[k] for k in ['path','bytes','sha256']}
                if r['path'] in phase_by:
                    phase=phase_by[r['path']];assert r in [phase['before'],phase['after']];conflicts.append(dict(observed=r,current=phase['after']));r=phase['after']
                assert rows.setdefault(r['path'],r)==r,'Conflicting exact private pin'
            for v in value.values():collect(v)
        elif isinstance(value,list):
            for v in value:collect(v)
    inputs=sorted(HERE.glob('*.json'))
    for p in inputs:collect(json.loads(p.read_bytes()))
    for r in phases:rows[r['recovered_before']['path']]=r['recovered_before']
    commands=[]
    for folder in sorted((RUNROOT/'commands').glob('w2c10-*')):
        recordpath=folder/'command.json'
        if not recordpath.is_file():continue
        record=json.loads(recordpath.read_bytes());commands.append(pin(recordpath))
        for name in ['stdout','stderr']:
            row=pin(folder/(name+'.log'));assert row['sha256']==record[name+'_sha256'];rows[row['path']]=row
        row=pin(recordpath);rows[row['path']]=row
    # Exact shipping DLL artifacts used by both compiled proofs and frozen replay.
    for config in ['Debug','Release']:
        for folder in ['lib','udf']:
            for name in ['BESHStatNG.dll','BESHStat.Core.dll','BESHStat.Excel.Common.dll','ExcelDna.Integration.dll']:
                row=pin(PRIVATE/'packet-v2'/config/folder/name);assert rows.setdefault(row['path'],row)==row
    for row in rows.values():check(row)
    partial=prior['retained_interrupted_aggregation'];check(partial)
    pubpath=W2/'continuation-v10-publication-receipt-v1.json';pub=json.loads(pubpath.read_bytes());parts_path=W2/'continuation-v10-review-parts-index-v1.json';parts=json.loads(parts_path.read_bytes());archives={r['path']:{k:r[k] for k in ['path','bytes','sha256']} for r in prior['retained_prior_physical_archives']}
    for r in [pub['archive'],pub['archive']['deterministic_reproduction']]+[p for r in parts['parts'] for p in [r,r['deterministic_reproduction']]]:archives[r['path']]={k:r[k] for k in ['path','bytes','sha256']}
    for row in archives.values():check(row)
    controls=[];small=next(r for r in rows.values() if 0<r['bytes']<10000);data=(ROOT/small['path']).read_bytes()
    for label,value in [('changed private bytes',data+b'changed'),('missing private file',None)]:
        try:check(small,{small['path']:value})
        except (AssertionError,OSError):controls.append(dict(name=label,status='REJECTED'))
        else:raise AssertionError('Private mutation accepted')
    target=W2/'continuation-v10-retained-private-preservation-v1.json'
    doc=dict(status='POST_PUBLICATION_EXACT_RETAINED_PRIVATE_HASH_PASS',reader=pin(Path(__file__)),previous=pin(priorpath),prior_reader=prior['reader'],private_pins=[rows[k] for k in sorted(rows)],private_pin_count=len(rows),inherited_private_count=len(prior['private_pins']),retained_prior_physical_archives=list(archives.values()),retained_interrupted_aggregation=partial,synthetic_phases=pin(HERE/'private-proof-transitions-v1.json'),phase_observations=conflicts,current_publication=pin(pubpath),parts_index=pin(parts_path),current_command_records=commands,inputs=[pin(p) for p in inputs],controls=controls,original_queries=0,scope='Explicit pinned files only; preserve prior1964 plus current raw diagnostics/streams/TRX/worker/helper/compile evidence. No claim of exhaustive ignored preservation; historical TestResults not traversed. C9 reader-v6/receipt-v2 phase authority remains unchanged. Two new synthetic cache-control prestates are honestly recovered constants, not source preimages or original statistical captures.')
    with target.open('x',encoding='utf-8',newline='\n') as f:json.dump(doc,f,ensure_ascii=False,indent=2);f.write('\n')
    print(json.dumps(dict(receipt=pin(target),private_pins=len(rows),archives=len(archives))))
if __name__=='__main__':run()
