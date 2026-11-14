"""One C8 review derivative, verified copy and deterministic reproduction."""
from common import *
import shutil,zipfile,zlib
from verify_c8_publication_v3 import inspect,inspect_zip
manifest=HERE/'publication-manifest-v1.json'
if sys.argv[1]=='inventory':
    previous=json.loads((PREVIOUS/'publication-manifest-v1.json').read_bytes())
    paths={row['path'] for row in previous['entries']}
    paths.update(json.loads((HERE/'implementation-plan-v1.json').read_bytes())['source_paths'])
    paths.update((W2/name).relative_to(ROOT).as_posix() for name in ['continuation-v7-publication-receipt-v1.json','continuation-v7-final-git-preservation-v1.json'])
    # Keep every append-only C8 source/proof version; private runtime outputs stay private.
    paths.update(path.relative_to(ROOT).as_posix() for path in HERE.rglob('*') if path.is_file() and '__pycache__' not in path.parts)
    paths.update((HERE/name).relative_to(ROOT).as_posix() for name in ['file-inventory-v1.json','publication-manifest-v1.json'])
    ordered=sorted(paths)
    save('file-inventory-v1.json',dict(status='SELECTED_C8_REVIEW_INVENTORY_NOT_AUTHORITATIVE_REPO',paths=ordered,excluded=['Private raw command streams/TRX/caches/compiled DLLs','The16 unchanged protected owner documents excluded by inherited policy','Ignored retained TestResults and other unlisted private historical sources'],limitations='Omission is not deletion; separate live Git/protection and raw-stream receipts are authoritative for excluded material.'))
    rows=[dict(path=path,bytes=None,sha256=None) if path==manifest.relative_to(ROOT).as_posix() else pin(ROOT/path) for path in ordered]
    save('publication-manifest-v1.json',dict(status='NOT_READY_04A38',W2='IN_PROGRESS',W3='NOT_STARTED',inventory='file-inventory-v1.json',entries=rows,self_entry='Exactly one nonrecursive null self-entry; ZIP bytes/hash/CRC recorded in external post-seal receipt',original_queries=0))
    print('Manifest sealed',len(rows),flush=True)
elif sys.argv[1]=='export':
    # Nothing in the selected publication set is written after inventory sealing.
    live=inspect();print('Live complete manifest/source/CIL/validation/privacy PASS_NOT_READY',flush=True)
    rows=json.loads(manifest.read_bytes())['entries'];folder=PRIVATE/'publication-v1';folder.mkdir(exist_ok=False)
    total=sum(r['bytes'] or 0 for r in rows);need_space=total+2*1024**3
    assert shutil.disk_usage(ROOT).free>need_space,'Insufficient space; preserve all originals and incomplete attempts'
    def build(path):
        with zipfile.ZipFile(path,'x',compression=zipfile.ZIP_DEFLATED,compresslevel=6,allowZip64=True) as archive:
            for row in rows:
                info=zipfile.ZipInfo(row['path'],date_time=(1980,1,1,0,0,0));info.compress_type=zipfile.ZIP_DEFLATED;info.create_system=3;info.external_attr=0o100644<<16
                archive.writestr(info,(ROOT/row['path']).read_bytes(),compress_type=zipfile.ZIP_DEFLATED,compresslevel=6)
        with path.open('rb') as stream:return hashlib.file_digest(stream,'sha256').hexdigest()
    archive=folder/'BESHstatNG_04A38_W2_continuation_v8_review.zip';digest=build(archive)
    physical=inspect_zip(archive);print('All physical ZIP SHA/CRC/privacy PASS',flush=True)
    reproduction=folder/'determinism-reproduction-v1.zip';repeat=build(reproduction);assert repeat==digest and reproduction.stat().st_size==archive.stat().st_size
    print('Independent deterministic ZIP rebuild byte hash exact',flush=True)
    copied=ROOT/'.c8-review-copy-v1.tmp';assert max(len(str(copied/r['path'])) for r in rows)<260;copied.mkdir(exist_ok=False)
    with zipfile.ZipFile(archive) as source:
        for item in source.infolist():
            target=copied/item.filename;assert target.resolve().is_relative_to(copied.resolve());target.parent.mkdir(parents=True,exist_ok=True)
            with target.open('xb') as stream:stream.write(source.read(item.filename))
    result=command('publication-copied-reader-v1',[sys.executable,'-B',str(copied/HERE.relative_to(ROOT)/'verify_c8_publication_v3.py')],cwd=copied)
    assert result['exit_code']==0,'Failed copied reader is retained, never interpreted as verified'
    copy_result=json.loads((RUNROOT/'commands/w2c8-publication-copied-reader-v1/stdout.log').read_bytes())
    controls=[]
    def reject(name,changes):
        try:inspect(overrides=changes,semantic=False)
        except (ValueError,OSError,KeyError) as error:controls.append(dict(name=name,status='REJECTED',reason=clean(str(error))));return
        raise AssertionError('Publication mutation accepted '+name)
    key=manifest.relative_to(ROOT).as_posix();value=json.loads(manifest.read_bytes());value['entries'].append(value['entries'][0]);reject('Overlapping publication identity',{key:json.dumps(value).encode()})
    value=json.loads(manifest.read_bytes());next(r for r in value['entries'] if r['sha256'] is None).update(sha256='0'*64,bytes=1);reject('Recursive self hash substitution',{key:json.dumps(value).encode()})
    value=json.loads(manifest.read_bytes());value['status']='READY_04A38';reject('False READY publication',{key:json.dumps(value).encode()})
    row=next(r for r in rows if r['sha256'] is not None);reject('Altered stored payload',{row['path']:(ROOT/row['path']).read_bytes()+b'changed'})
    # A real ZIP with an unmanifested raw-diagnostic-shaped member must fail.
    tiny=folder/'negative-extra-raw-member-v1.zip'
    with zipfile.ZipFile(tiny,'x',compression=zipfile.ZIP_DEFLATED) as output:
        for name,data in [(key,manifest.read_bytes()),('full-managed-Debug-v99.json',b'{}')]:
            info=zipfile.ZipInfo(name,date_time=(1980,1,1,0,0,0));info.compress_type=zipfile.ZIP_DEFLATED;output.writestr(info,data)
    try:inspect_zip(tiny)
    except (ValueError,KeyError,zipfile.BadZipFile) as error:controls.append(dict(name='Actual extra raw diagnostic ZIP member',status='REJECTED',reason=clean(str(error))))
    else:raise AssertionError('Raw diagnostic ZIP accepted')
    receipt=dict(status='C8_PARTIAL_W2_NOT_READY_LIVE_COPY_DETERMINISTIC_PUBLICATION_VERIFIED',manifest=pin(manifest),review=pin(HERE/'owner-review.md'),next_wave=pin(HERE/'NEXT_WAVE_HANDOVER.md'),live=live,physical_zip=physical,copied_executable=clean(result),copied_result=copy_result,archive=dict(name=archive.name,path=archive.relative_to(ROOT).as_posix(),bytes=archive.stat().st_size,sha256=digest,deterministic_reproduction=dict(path=reproduction.relative_to(ROOT).as_posix(),bytes=reproduction.stat().st_size,sha256=repeat,byte_exact=True),policy='Sorted paths, fixed1980timestamps/mode, DEFLATE6, actual complete second serialization exact'),mutations=controls,restrictions='Review derivative only; no authoritatively omitted file was deleted. Live .git/owner documents/private raw diagnostics/ignored historical payloads require separate live receipt. No XLL packaging, signing, deployment, stage/commit/merge/rebase/push/reset/clean.',original_queries=0)
    path=W2/'continuation-v8-publication-receipt-v1.json'
    with path.open('x',encoding='utf-8',newline='\n') as stream:json.dump(receipt,stream,ensure_ascii=False,indent=2);stream.write('\n')
    print(json.dumps(dict(archive=receipt['archive'],receipt=pin(path))),flush=True)
else:raise ValueError(sys.argv[1])
