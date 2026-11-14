"""Preserve first copied-reader failure; add missing pinned entry context only."""
import sys
from pathlib import Path
sys.path.insert(0,str(Path(__file__).resolve().parent/'continuation-v11'))
from common import *
import importlib.util,copy,zipfile,zlib
from publication_privacy_c11_v1 import safe_path,privacy
from verify_c11_preservation_v1 import inspect as preservation
from publish_parts_v2 import make_zip,verify_zip,index_valid
READER=W2/'continuation-v11-publication-reader-v3.py'
MANIFEST=W2/'continuation-v11-logical-publication-manifest-v2.json'
INDEX=W2/'continuation-v11-review-parts-index-v2.json'

def write(name,doc):
    path=W2/name
    with path.open('x',encoding='utf-8',newline='\n') as stream:json.dump(doc,stream,ensure_ascii=False,indent=2);stream.write('\n')
    return pin(path)

def module():
    spec=importlib.util.spec_from_file_location('c11_supplemented_publication',READER)
    value=importlib.util.module_from_spec(spec);spec.loader.exec_module(value);return value

def prepare():
    old=json.loads((HERE/'publication-manifest-v1.json').read_bytes());names={r['path'] for r in old['entries']}
    context=[r for r in json.loads((HERE/'entry-authority-pins-v1.json').read_bytes())['authorities'] if r['path'] not in names]
    assert {r['path'] for r in context}=={'AGENTS.md','planning/BESHStatNG_CrossPlatform_MasterPlan.md','planning/BESHStatNG_CrossPlatform_MasterPlan2.12.md'}
    for row in context:assert pin(ROOT/row['path'])==row
    # Observe every actual preservation file read, using unchanged semantics and real bytes.
    reads=set();original=Path.read_bytes
    def observed(path):
        reads.add(path.resolve());return original(path)
    Path.read_bytes=observed
    try:proof=preservation(ROOT,False)
    finally:Path.read_bytes=original
    missing=sorted(p.relative_to(ROOT).as_posix() for p in reads if p.relative_to(ROOT).as_posix() not in names)
    owners={r['path'] for r in json.loads((ROOT/'evidence/04A38/preservation-validation-v2.json').read_bytes())['owner_documents_excluded']}
    required={r['path'] for r in context}
    assert set(missing)-owners<=required and required<=set(missing),missing
    folder=RUNROOT/'commands/w2c11-copied-default-publication-reader-v1'
    execution=json.loads((folder/'command.json').read_bytes())
    assert execution['exit_code']==1
    failure=write('continuation-v11-copy-failure-reconciliation-v1.json',dict(status='FIRST_COPIED_READER_FAIL_RETAINED_NOT_RELABELLED',
        execution=pin(folder/'command.json'),stdout=pin(folder/'stdout.log'),stderr=pin(folder/'stderr.log'),
        first_copy='.c11p.tmp',first_copy_retained_unchanged=True,missing_entry_context=context,
        actual_preservation_reads=len(reads),failed_preparer_sources=[pin(W2/name) for name in ['continuation-v11-supplement-publication-v1.py','continuation-v11-supplement-publication-v2.py']],second_failure_command=pin(RUNROOT/'commands/w2c11-supplement-preparation-v2/command.json'),failed_preparer_reproduction_command=pin(RUNROOT/'commands/w2c11-supplement-preparation-failure-reproduction-v1/command.json'),actual_missing_paths=missing,optional_excluded_owner_paths=sorted((set(missing)&owners)-required),live_preservation=proof,
        remedy='Additive sixth ACTIVE ZIP and versioned complete logical manifest/index/reader; original five ZIPs, original manifest and failed reader/copy remain unchanged.',original_queries=0))
    note=W2/'continuation-v11-publication-supplement-addendum-v1.md'
    with note.open('x',encoding='utf-8',newline='\n') as stream:
        stream.write('# C11 additive publication correction\n\nNOT_READY_04A38; W2_IN_PROGRESS; W3_NOT_STARTED; 04A39_BLOCKED\n\nThe first copied default reader exited1 because AGENTS.md was absent from the original selected review. Independently tracing every read of the unchanged preservation reader found exactly three missing entry authorities: AGENTS.md and both pinned cross-platform master plans. The complete source/CIL proof and live preservation had passed; copied acceptance was not claimed. The failed .c11p.tmp copy and all original five ZIPs remain untouched.\n\nThe authoritative complete logical union is continuation-v11-logical-publication-manifest-v2.json and continuation-v11-review-parts-index-v2.json: the same five immutable parts plus one ACTIVE context/authority supplement. continuation-v11-publication-reader-v3.py verifies that exact union and replays the unchanged sealed source/CIL/validation/preservation reader. Read the actual copied-reader-v2 receipt for acceptance; the first exit1 remains a separate failure. No production source, test assertion, original reference, scientific result, or protected artifact changed.\n')
    added=[ROOT/r['path'] for r in context]+[READER,Path(__file__),W2/'continuation-v11-supplement-publication-v1.py',W2/'continuation-v11-supplement-publication-v2.py',W2/'continuation-v11-correct-supplement-preparer-v2.py',W2/'continuation-v11-correct-supplement-preparer-v3.py',note,W2/failure['path'].split('/')[-1],
        W2/'continuation-v11-review-parts-index-v1.json',W2/'continuation-v11-publication-receipt-v1.json']
    rows=[pin(ROOT/r['path']) for r in old['entries']]+[pin(p) for p in added]
    assert len(rows)==len({r['path'] for r in rows})
    for row in rows:
        if row['path'] not in names:privacy(row['path'],(ROOT/row['path']).read_bytes())
    rows.append(dict(path=MANIFEST.relative_to(ROOT).as_posix(),bytes=None,sha256=None));rows.sort(key=lambda r:r['path'])
    write(MANIFEST.name,dict(status='NOT_READY_04A38',W2='IN_PROGRESS',W3='NOT_STARTED',entries=rows,
        sealed_manifest=pin(HERE/'publication-manifest-v1.json'),required_context=context,
        supplement_paths=[p.relative_to(ROOT).as_posix() for p in added],
        original_sealed_files=len(old['entries']),standalone='NOT_CREATED',original_queries=0))
    print('Complete additive manifest prepared',len(rows),flush=True)

def export():
    doc=json.loads(MANIFEST.read_bytes());prior=json.loads((W2/'continuation-v11-review-parts-index-v1.json').read_bytes())
    baseline={r['path'] for r in prior['files']};rows=[pin(ROOT/r['path']) for r in doc['entries'] if r['path'] not in baseline]
    folder=PRIVATE/'review-parts-v2-supplement';folder.mkdir(exist_ok=False)
    path=folder/'BESHstatNG_04A38_W2_v11_ACTIVE_02.zip';make_zip(path,rows);records=verify_zip(path,rows)
    repro=folder/(path.name+'.determinism.zip');make_zip(repro,rows);assert pin(path)['sha256']==pin(repro)['sha256']
    assert path.stat().st_size<180*1024**2
    part=dict(name=path.name,group='ACTIVE',**pin(path),files=len(rows),deterministic_reproduction=pin(repro),CRC='ALL_MEMBERS_VERIFIED')
    files=prior['files']+[dict(r,part=path.name,group='ACTIVE',logical_root_prefix='') for r in records]
    result=dict(status=prior['status'],manifest=pin(MANIFEST),parts=prior['parts']+[part],files=sorted(files,key=lambda r:r['path']),
        logical_root='repository root, empty prefix',standalone='NOT_CREATED',exclusions=prior['exclusions'],
        supersedes=pin(W2/'continuation-v11-review-parts-index-v1.json'),original_queries=0)
    expected={r['path']:pin(ROOT/r['path']) for r in doc['entries']};index_valid(result,expected)
    controls=[]
    for name,mutate in [('missing context',lambda d:d['files'].__delitem__(next(i for i,r in enumerate(d['files']) if r['path']=='AGENTS.md'))),
        ('duplicate payload',lambda d:d['files'].append(d['files'][0])),('changed provenance SHA',lambda d:d['files'][0].update(sha256='0'*64))]:
        bad=copy.deepcopy(result);mutate(bad)
        try:index_valid(bad,expected)
        except (AssertionError,ValueError):controls.append(dict(name=name,status='REJECTED'))
        else:raise AssertionError('Invalid supplement accepted')
    result['controls']=controls;write(INDEX.name,result)
    physical=module().inspect_parts(INDEX)
    write('continuation-v11-publication-receipt-v2.json',dict(status='ADDITIVE_COMPLETE_SPLIT_PUBLICATION_VERIFIED_NOT_READY',
        manifest=pin(MANIFEST),parts_index=pin(INDEX),parts=result['parts'],files=len(files),physical=physical,
        original_five_parts_unchanged=True,retained_failure=pin(W2/'continuation-v11-copy-failure-reconciliation-v1.json'),original_queries=0))
    print('Additive six-part physical union PASS_NOT_READY',len(files),flush=True)

def copied():
    doc=json.loads(INDEX.read_bytes());physical=module().inspect_parts(INDEX)
    target=ROOT/'.c11q.tmp';target.mkdir(exist_ok=False);seen=set()
    expected={r['path']:r for r in doc['files']}
    assert max(len(str(target/p)) for p in expected)<260
    for part in doc['parts']:
        with zipfile.ZipFile(ROOT/part['path']) as archive:
            for info in archive.infolist():
                name=info.filename;safe_path(name);assert name not in seen;seen.add(name)
                data=archive.read(name);row=expected[name];assert (len(data),sha(data))==(row['bytes'],row['sha256'])
                path=target/name;assert path.resolve().is_relative_to(target.resolve());path.parent.mkdir(parents=True,exist_ok=True)
                with path.open('xb') as stream:stream.write(data)
    assert seen==set(expected)
    result=command('copied-default-publication-reader-v2',[sys.executable,'-B',str(target/READER.relative_to(ROOT))],cwd=target)
    assert result['exit_code']==0,'Retain failed successor copy; never relabel acceptance'
    raw=json.loads((RUNROOT/'commands/w2c11-copied-default-publication-reader-v2/stdout.log').read_bytes())
    write('continuation-v11-copied-review-reader-v2.json',dict(status='C11_COMPLETE_SIX_PART_UNION_AND_COPIED_EXECUTABLE_READER_PASS_NOT_READY',
        parts_index=pin(INDEX),physical=physical,extracted_files=len(seen),copied_executable=clean(result),copied_result=raw,
        retained_first_failure=pin(W2/'continuation-v11-copy-failure-reconciliation-v1.json'),original_queries=0))
    print('Complete six-part independent copied reader PASS_NOT_READY',flush=True)

if __name__=='__main__':{'prepare':prepare,'export':export,'copy':copied}[sys.argv[1]]()
