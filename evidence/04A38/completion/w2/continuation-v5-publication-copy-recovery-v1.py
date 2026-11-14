"""Post-seal copy recovery: retain the archive and use Windows extended paths.

The first exporter verified live payloads and physical ZIP hashes/privacy/CRC,
then encountered MAX_PATH while extracting. No sealed payload is changed here.
Existing extracted bytes must match the ZIP; only missing members are created.
"""
import sys
from pathlib import Path
CHECKPOINT=Path(__file__).resolve().parent/'continuation-v5'
sys.path.insert(0,str(CHECKPOINT))
from common import *
from verify_publication_final import inspect,inspect_zip
import zipfile

manifest=(HERE/'publication-manifest-v1.json').relative_to(ROOT).as_posix()
folder=PRIVATE/'publication-v1'
archive=folder/'BESHstatNG_04A38_W2_continuation_v5_review.zip'
ordinary_copy=folder/'copied-root'
copy=Path('\\\\?\\'+str(ordinary_copy.resolve()))
assert str(copy).startswith('\\\\?\\') and ordinary_copy.resolve().is_relative_to(ROOT.resolve())
failure=dict(status='RETAINED_POST_SEAL_COPY_ATTEMPT',exit_code=1,
    completed=['Live manifest/source/proofs/privacy verification','Physical ZIP hashes/CRC/privacy verification'],
    failure='FileNotFoundError [Errno2] during exclusive creation of historical continuation-v4 source-after/parametric-v4/BESHStat.Excel.Common/src/Analysis/UdfNumericImportPreparation38.vb.txt under the long isolated copied-root path',
    cause='Windows MAX_PATH; ordinary extracted target exceeded260 characters',
    original_exporter=pin(HERE/'export_and_verify_final.py'),archive_sha256=sha(archive.read_bytes()),
    disposition='Archive and every partial extracted file retained. Resume through extended-length Windows namespace; verify existing bytes, never overwrite.',
    sealed_payload_changes=0,original_queries=0)
failure_path=W2/'continuation-v5-publication-copy-attempt-v1.json'
with failure_path.open('x',encoding='utf-8',newline='\n') as stream:json.dump(failure,stream,indent=2);stream.write('\n')

physical=inspect_zip(archive,manifest)
existing=created=0
with zipfile.ZipFile(archive) as source:
    for item in source.infolist():
        path=copy/item.filename
        assert path.resolve().is_relative_to(copy.resolve())
        data=source.read(item.filename)
        if path.exists():
            assert path.read_bytes()==data,'Partial copy bytes changed'
            existing+=1
        else:
            path.parent.mkdir(parents=True,exist_ok=True)
            with path.open('xb') as stream:stream.write(data)
            created+=1
    assert source.testzip() is None
print('All ZIP members copied with exact existing-byte checks and extended Windows paths',flush=True)

r=command('publication-copied-reader-v1',[sys.executable,'-B',str(copy/HERE.relative_to(ROOT)/'verify_publication_final.py'),manifest],cwd=copy)
assert r['exit_code']==0,'Preserve failed extended-path copy attempt'
copied=json.loads((RUNROOT/'commands/w2c5-publication-copied-reader-v1/stdout.log').read_bytes())
print('Copied executable source/ownership/preservation/privacy reader PASS',flush=True)

# An independent live semantic read can reuse the copied privacy observations
# only through actual same-byte hashes, but retain a fresh cold live read here.
live=inspect()
controls=[]
def reject(name,changes,expected):
    try:inspect(overrides=changes,semantic=False)
    except (ValueError,OSError) as error:
        assert expected in str(error),(name,str(error))
        controls.append(dict(name=name,status='REJECTED',reason=str(error)))
    else:raise AssertionError('Mutation accepted '+name)
def mutate(name,change,expected):
    data=json.loads((ROOT/manifest).read_bytes());change(data)
    reject(name,{manifest:json.dumps(data).encode()},expected)
mutate('False READY',lambda d:d.update(status='READY_FOR_OWNER_04A38_ACCEPTANCE_REVIEW'),'False READY')
mutate('Duplicate path',lambda d:d['entries'].append(d['entries'][0]),'Duplicate/casefold')
mutate('Missing null self',lambda d:d['entries'].pop(next(i for i,r in enumerate(d['entries']) if r['path']==manifest)),'Self missing')
mutate('Additional null payload',lambda d:d['entries'][0].update(bytes=None,sha256=None),'Illegal recursive/null')
protected='evidence/04A38/preedit/entry-source-snapshot.json'
reject('Changed protected evidence',{protected:(ROOT/protected).read_bytes()+b'changed'},'Payload hash mismatch')
tiny=folder/'adverse-extra-raw.zip'
with zipfile.ZipFile(tiny,'x') as output:
    output.writestr(manifest,(ROOT/manifest).read_bytes())
    output.writestr('evidence/04A38/completion/w2/continuation-v5/full-managed-Debug-v3.json',b'forbidden synthetic diagnostic')
try:inspect_zip(tiny,manifest)
except ValueError as error:
    assert 'Forbidden raw diagnostic/cache' in str(error)
    controls.append(dict(name='Actual unmanifested raw diagnostic ZIP member',status='REJECTED',reason=str(error)))
else:raise AssertionError('Physical raw diagnostic accepted')

receipt=dict(status='C5_PARTIAL_W2_NOT_READY_LIVE_COPY_PUBLICATION_VERIFIED',manifest=pin(ROOT/manifest),
    live=live,physical_zip=physical,copied_executable=clean(r),copied_result=copied,
    archive=dict(name=archive.name,bytes=archive.stat().st_size,sha256=sha(archive.read_bytes()),deterministic='Sorted paths, fixed1980timestamps, fixed mode, DEFLATE6'),
    extraction=dict(existing_byte_exact=existing,new_exclusive_members=created,path_policy='Extended-length Windows namespace; all names canonical and confined to the original isolated copied-root',overwritten=0),
    failed_attempt=pin(failure_path),recovery_reader=pin(Path(__file__)),mutations=controls,
    restrictions='W3 NOT_STARTED; no owner acceptance; copy cannot prove omitted live/index/private DLL/raw diagnostics/owner documents/ignored history',original_queries=0)
dest=W2/'continuation-v5-publication-receipt-v1.json'
with dest.open('x',encoding='utf-8',newline='\n') as stream:json.dump(receipt,stream,ensure_ascii=False,indent=2);stream.write('\n')
print('Live/copy/publication controls PASS; external post-seal receipt recorded',flush=True)
