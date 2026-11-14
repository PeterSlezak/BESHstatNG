"""Add pure-reader successor; preserve six earlier ZIPs and both failed copies."""
import sys
from pathlib import Path
sys.path.insert(0,str(Path(__file__).resolve().parent/'continuation-v11'))
from common import *
import importlib.util,zipfile,copy
from publication_privacy_c11_v1 import safe_path,privacy
from publish_parts_v2 import make_zip,verify_zip,index_valid
READER=W2/'continuation-v11-publication-reader-v4.py'
MANIFEST=W2/'continuation-v11-logical-publication-manifest-v3.json'
INDEX=W2/'continuation-v11-review-parts-index-v3.json'

def write(name,doc):
    path=W2/name
    with path.open('x',encoding='utf-8',newline='\n') as stream:json.dump(doc,stream,ensure_ascii=False,indent=2);stream.write('\n')
    return pin(path)

def module():
    spec=importlib.util.spec_from_file_location('c11_complete_reader4',READER)
    value=importlib.util.module_from_spec(spec);spec.loader.exec_module(value);return value

def build():
    before=W2/'continuation-v11-publication-reader-v3.py';text=before.read_text()
    assert text.count('from common import sha,pin')==1
    text=text.replace('from common import sha,pin','import hashlib\ndef sha(data):return hashlib.sha256(data).hexdigest()')
    text=text.replace('continuation-v11-logical-publication-manifest-v2.json','continuation-v11-logical-publication-manifest-v3.json')
    text=text.replace("doc['status']=='NOT_READY_04A38' and doc['original_queries']==0", "(doc['status'],doc['W2'],doc['W3'],doc['standalone'])==('NOT_READY_04A38','IN_PROGRESS','NOT_STARTED','NOT_CREATED') and doc['original_queries']==0")
    text=text.replace('C11_SIX_PART_EXACT_LOGICAL_UNION_PASS_NOT_READY','C11_COMPLETE_PART_EXACT_LOGICAL_UNION_PASS_NOT_READY')
    with READER.open('x',encoding='utf-8',newline='\n') as stream:stream.write(text)
    folder=RUNROOT/'commands/w2c11-copied-default-publication-reader-v2'
    assert json.loads((folder/'command.json').read_bytes())['exit_code']==1
    failure=write('continuation-v11-copy-failure-reconciliation-v2.json',dict(status='TWO_FAILED_COPIES_RETAINED_NOT_RELABELLED',
        first=pin(W2/'continuation-v11-copy-failure-reconciliation-v1.json'),second_command=pin(folder/'command.json'),
        second_stdout=pin(folder/'stdout.log'),second_stderr=pin(folder/'stderr.log'),
        parent_command=pin(RUNROOT/'commands/w2c11-complete-copy-v2/command.json'),
        failed_reader=pin(before),successor_reader=pin(READER),
        reason='Wrapper imported common only for hashing, which imported the command runner and absent scripts/test_suite_rationalization.py. The second copied reader failed before semantic replay.',
        correction='Replace wrapper-only common import with standard-library hashlib; strengthen all outer stage fields. Unchanged sealed source/CIL/validation modules perform the actual semantic replay.',
        copies_retained=['.c11p.tmp','.c11q.tmp'],original_queries=0))
    note=W2/'continuation-v11-publication-supplement-addendum-v2.md'
    with note.open('x',encoding='utf-8',newline='\n') as stream:
        stream.write('# Authoritative complete C11 publication correction\n\nNOT_READY_04A38; W2_IN_PROGRESS; W3_NOT_STARTED; P=NO; N1=NO GO; N2=NO; C2=UNMET; C3=UNAPPROVED; NOT_GOLDEN; 04A39_BLOCKED\n\nThe first copied default reader exited1 on omitted entry authority. The additive sixth ZIP supplied AGENTS.md and both exact pinned master plans. Fifteen optional owner README documents remain omitted; the required protected master plan is included byte-exact. Two preparer attempts failed while distinguishing required context from optional owner reads; sources/raw receipts remain.\n\nThe second copied wrapper exited1 before proof replay because an unnecessary common import loaded a repository command runner outside the review payload. It is retained unchanged in .c11q.tmp, alongside first .c11p.tmp. A seventh ACTIVE authority ZIP supplies continuation-v11-publication-reader-v4.py, using only standard-library hashing plus unchanged sealed proof modules. Neither failed copy is relabelled successful.\n\nThe authoritative complete logical manifest/index are continuation-v11-logical-publication-manifest-v3.json and continuation-v11-review-parts-index-v3.json, covering all seven immutable complementary parts. Read actual continuation-v11-copied-review-reader-v3.json for the independent copied result. Both earlier manifests/indexes, readers, physical parts, attempts and tracebacks are preserved. No production/test/original source, historical assertion, scientific behavior or protected artifact changed. No unit campaign was repeated.\n')
    prior=json.loads((W2/'continuation-v11-review-parts-index-v2.json').read_bytes())
    previous=json.loads((W2/'continuation-v11-logical-publication-manifest-v2.json').read_bytes())
    added=[READER,Path(__file__),W2/failure['path'].split('/')[-1],note,
        W2/'continuation-v11-review-parts-index-v2.json',W2/'continuation-v11-publication-receipt-v2.json']
    rows=[{k:r[k] for k in ['path','bytes','sha256']} for r in prior['files']]+[pin(p) for p in added]
    assert len(rows)==len({r['path'] for r in rows})
    for p in added:privacy(p.relative_to(ROOT).as_posix(),p.read_bytes())
    rows.append(dict(path=MANIFEST.relative_to(ROOT).as_posix(),bytes=None,sha256=None));rows.sort(key=lambda r:r['path'])
    sealed_names={r['path'] for r in json.loads((HERE/'publication-manifest-v1.json').read_bytes())['entries']}
    manifest=dict(previous,entries=rows,supplement_paths=sorted(r['path'] for r in rows if r['path'] not in sealed_names and r['path']!=MANIFEST.relative_to(ROOT).as_posix()),
        complete_previous_manifest=pin(W2/'continuation-v11-logical-publication-manifest-v2.json'))
    write(MANIFEST.name,manifest)
    folder=PRIVATE/'review-parts-v3-authority';folder.mkdir(exist_ok=False)
    path=folder/'BESHstatNG_04A38_W2_v11_ACTIVE_03.zip'
    names={r['path'] for r in prior['files']};payload=[pin(ROOT/r['path']) for r in rows if r['path'] not in names]
    make_zip(path,payload);records=verify_zip(path,payload)
    reproduction=folder/(path.name+'.determinism.zip');make_zip(reproduction,payload)
    assert pin(path)['sha256']==pin(reproduction)['sha256'] and path.stat().st_size<180*1024**2
    part=dict(name=path.name,group='ACTIVE',**pin(path),files=len(payload),deterministic_reproduction=pin(reproduction),CRC='ALL_MEMBERS_VERIFIED')
    files=prior['files']+[dict(r,part=path.name,group='ACTIVE',logical_root_prefix='') for r in records]
    result=dict(prior,manifest=pin(MANIFEST),parts=prior['parts']+[part],files=sorted(files,key=lambda r:r['path']),
        supersedes=pin(W2/'continuation-v11-review-parts-index-v2.json'))
    expected={r['path']:pin(ROOT/r['path']) for r in rows};index_valid(result,expected)
    write(INDEX.name,result);physical=module().inspect_parts(INDEX)
    write('continuation-v11-publication-receipt-v3.json',dict(status='COMPLETE_SEVEN_PART_PHYSICAL_UNION_VERIFIED_NOT_READY',
        manifest=pin(MANIFEST),parts_index=pin(INDEX),parts=result['parts'],files=len(files),physical=physical,
        original_six_parts_unchanged=True,retained_failures=failure,original_queries=0))
    print('Complete seven-part publication PASS_NOT_READY',len(files),flush=True)

def copied():
    doc=json.loads(INDEX.read_bytes());physical=module().inspect_parts(INDEX)
    target=ROOT/'.c11r.tmp';target.mkdir(exist_ok=False);expected={r['path']:r for r in doc['files']};seen=set()
    assert max(len(str(target/p)) for p in expected)<260
    for part in doc['parts']:
        with zipfile.ZipFile(ROOT/part['path']) as archive:
            for info in archive.infolist():
                name=info.filename;safe_path(name);assert name not in seen;seen.add(name)
                data=archive.read(name);row=expected[name];assert (len(data),sha(data))==(row['bytes'],row['sha256'])
                path=target/name;assert path.resolve().is_relative_to(target.resolve());path.parent.mkdir(parents=True,exist_ok=True)
                with path.open('xb') as stream:stream.write(data)
    assert seen==set(expected)
    result=command('copied-default-publication-reader-v3',[sys.executable,'-B',str(target/READER.relative_to(ROOT))],cwd=target)
    assert result['exit_code']==0,'Preserve any failed successor; never infer acceptance'
    raw=json.loads((RUNROOT/'commands/w2c11-copied-default-publication-reader-v3/stdout.log').read_bytes())
    write('continuation-v11-copied-review-reader-v3.json',dict(status='C11_COMPLETE_SEVEN_PART_COPIED_EXECUTABLE_READER_PASS_NOT_READY',
        parts_index=pin(INDEX),physical=physical,extracted_files=len(seen),copied_executable=clean(result),copied_result=raw,
        retained_failures=pin(W2/'continuation-v11-copy-failure-reconciliation-v2.json'),original_queries=0))
    print('Complete seven-part independent copied reader PASS_NOT_READY',flush=True)

def authorities():
    source=W2/'continuation-v11-retained-private-reader-v3.py';text=source.read_text()
    text=text.replace('continuation-v11-retained-private-preservation-v3.json','continuation-v11-retained-private-preservation-v4.json')
    text=text.replace('continuation-v11-review-parts-index-v2.json','continuation-v11-review-parts-index-v3.json')
    with (W2/'continuation-v11-retained-private-reader-v4.py').open('x',encoding='utf-8',newline='\n') as stream:stream.write(text)
    text=(W2/'continuation-v11-finalize-live-v5.py').read_text()
    for a,b in [('continuation-v11-retained-private-preservation-v3.json','continuation-v11-retained-private-preservation-v4.json'),
        ('continuation-v11-retained-private-reader-v3.py','continuation-v11-retained-private-reader-v4.py'),
        ('continuation-v11-review-parts-index-v2.json','continuation-v11-review-parts-index-v3.json'),
        ('continuation-v11-publication-receipt-v2.json','continuation-v11-publication-receipt-v3.json'),
        ('continuation-v11-copied-review-reader-v2.json','continuation-v11-copied-review-reader-v3.json'),
        ('continuation-v11-publication-reader-v3.py','continuation-v11-publication-reader-v4.py'),
        ('continuation-v11-logical-publication-manifest-v2.json','continuation-v11-logical-publication-manifest-v3.json'),
        ('continuation-v11-copy-failure-reconciliation-v1.json','continuation-v11-copy-failure-reconciliation-v2.json'),
        ('continuation-v11-publication-supplement-addendum-v1.md','continuation-v11-publication-supplement-addendum-v2.md')]:text=text.replace(a,b)
    text=text.replace('complete six-part index','complete seven-part index').replace('earlier private reader-v1/v2 sources','earlier private reader-v1/v2/v3 sources')
    text=text.replace('The first copied default reader failed on omitted entry context;', 'The first copied default reader failed on omitted entry context; the second copied wrapper failed before semantic replay because common unnecessarily imported the repository command runner. Both failed copies and actual exit1 traces remain. Reader-v4 replaces that wrapper import with standard-library hashing;')
    text=text.replace("'continuation-v11-retained-private-reader-v2.py']],", "'continuation-v11-retained-private-reader-v2.py','continuation-v11-retained-private-reader-v3.py','continuation-v11-finalize-live-v5.py','continuation-v11-complete-publication-v4.py']],")
    with (W2/'continuation-v11-finalize-live-v6.py').open('x',encoding='utf-8',newline='\n') as stream:stream.write(text)
    print('Private-v4 / finalizer-v6 additive sources prepared',flush=True)

if __name__=='__main__':{'build':build,'copy':copied,'authorities':authorities}[sys.argv[1]]()
