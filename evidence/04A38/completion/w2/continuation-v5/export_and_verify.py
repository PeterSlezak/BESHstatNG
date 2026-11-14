"""Verify live and copied evidence, physical ZIP/CRC, and adversarial mutations."""
from common import *
from verify_publication import inspect,inspect_zip
import zipfile
manifest=(HERE/'publication-manifest-v1.json').relative_to(ROOT).as_posix()
live=inspect()
print('Live manifest/source/proofs/privacy verified',flush=True)
rows=json.loads((ROOT/manifest).read_bytes())['entries']
folder=PRIVATE/'publication-v1';folder.mkdir()
archive=folder/'BESHstatNG_04A38_W2_continuation_v5_review.zip'
with zipfile.ZipFile(archive,'x',compression=zipfile.ZIP_DEFLATED,compresslevel=6,allowZip64=True) as output:
    for row in rows:
        info=zipfile.ZipInfo(row['path'],date_time=(1980,1,1,0,0,0))
        info.compress_type=zipfile.ZIP_DEFLATED;info.create_system=3;info.external_attr=0o100644<<16
        output.writestr(info,(ROOT/row['path']).read_bytes(),compress_type=zipfile.ZIP_DEFLATED,compresslevel=6)
physical=inspect_zip(archive,manifest)
print('Complete physical ZIP hash/CRC/privacy verified',flush=True)
copy=folder/'copied-root';copy.mkdir()
with zipfile.ZipFile(archive) as source:
    for item in source.infolist():
        path=copy/item.filename
        assert path.resolve().is_relative_to(copy.resolve())
        path.parent.mkdir(parents=True,exist_ok=True)
        with path.open('xb') as stream:stream.write(source.read(item.filename))
    assert source.testzip() is None
r=command('publication-copied-reader-v1',[sys.executable,'-B',str(copy/HERE.relative_to(ROOT)/'verify_publication.py'),manifest],cwd=copy)
assert r['exit_code']==0,'Preserve failed copy; never call it verified'
copied=json.loads((RUNROOT/'commands/w2c5-publication-copied-reader-v1/stdout.log').read_bytes())
controls=[]
def reject(name,changes,expected):
    try:inspect(overrides=changes,semantic=False)
    except (ValueError,OSError) as error:
        assert expected in str(error),(name,str(error));controls.append(dict(name=name,status='REJECTED',reason=str(error)))
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
    mutations=controls,restrictions='W3 NOT_STARTED; no owner acceptance; copied payload cannot prove omitted live/index/private DLL/raw diagnostics/owner documents/ignored history',
    original_queries=0)
dest=W2/'continuation-v5-publication-receipt-v1.json'
with dest.open('x',encoding='utf-8',newline='\n') as stream:json.dump(receipt,stream,ensure_ascii=False,indent=2);stream.write('\n')
print('Live/copy/publication controls PASS; external post-seal receipt recorded',flush=True)
