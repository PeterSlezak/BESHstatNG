"""Additive final transport; main ZIPs and all earlier attempts stay immutable."""
from common import *
import zipfile,zlib,importlib.util,concurrent.futures
sys.path.append(str(ROOT/'evidence/04A38'));from baseline_report import clean
sys.path.append(str(W2/'continuation-v6'));import publication_policy as policy

main=load(HERE/'publication/transport-manifest-v1.json')
prior=set(r['path'] for r in main['files'])
excluded={r['path'] for r in load(HERE/'publication/privacy-and-exclusions-v1.json')['excluded_authorities']}
selected=[p for p in HERE.rglob('*') if p.is_file() and p.relative_to(ROOT).as_posix() not in prior|excluded]
rows=[];new_excluded=[];derivatives=[]
for p in sorted(selected):
    name=p.relative_to(ROOT).as_posix();data=p.read_bytes();raw=pin(p)
    try:findings=policy.privacy(name,data)
    except ValueError as error:
        assert 'Unattested privacy finding' in str(error)
        try:review=(json.dumps(clean(json.loads(data)),indent=2,ensure_ascii=False)+'\n').encode()
        except (ValueError,UnicodeError):review=clean(data.decode('utf-8-sig',errors='replace')).encode()
        out=HERE/'publication/postseal-derivatives'/(name+'.review.json')
        try:findings=policy.privacy(out.relative_to(ROOT).as_posix(),review)
        except ValueError:
            review=(json.dumps(dict(raw=raw,review_only='Values omitted from transport only; local authority retained.'),indent=2)+'\n').encode()
            out=HERE/'publication/postseal-derivatives'/(name+'.omitted-values.review.json')
            findings=policy.privacy(out.relative_to(ROOT).as_posix(),review)
        out.parent.mkdir(parents=True,exist_ok=True)
        with out.open('xb') as f:f.write(review)
        new_excluded.append(raw);derivatives.append(dict(raw=raw,derivative=pin(out)));p=out;data=review
    rows.append(dict(pin(p),crc32=format(zlib.crc32(data)&0xffffffff,'08x'),privacy_findings=findings))
assert len(rows)==len({r['path'].casefold() for r in rows})
doc=dict(schema='W3_POSTSEAL_SUPPLEMENT_V1',files=rows,private_raw_included=False,binary_volumes=False,
    main_manifest=pin(HERE/'publication/transport-manifest-v1.json'),exclusions=new_excluded,derivatives=derivatives,
    purpose='Final readiness, Git/index, complete post-publication preservation and publication receipts; main packages are unchanged.')
save('publication/postseal-manifest-v1.json',doc)
metadata=(HERE/'publication/postseal-manifest-v1.json').read_bytes()
path=PRIVATE/'review-parts-v1/BESHstatNG_04A38_W3_POSTSEAL_v1.zip'
with zipfile.ZipFile(path,'x',compression=zipfile.ZIP_DEFLATED,compresslevel=6) as z:
    z.writestr('.w3-postseal-manifest.json',metadata)
    for r in rows:z.write(ROOT/r['path'],r['path'])
assert path.stat().st_size<180*1024**2
with zipfile.ZipFile(path) as z:
    assert set(z.namelist())=={r['path'] for r in rows}|{'.w3-postseal-manifest.json'}
    assert len(z.namelist())==len(set(z.namelist()))==len({n.casefold() for n in z.namelist()})
    assert z.read('.w3-postseal-manifest.json')==metadata
    for r in rows:
        data=z.read(r['path']);info=z.getinfo(r['path'])
        assert (len(data),sha(data),format(zlib.crc32(data)&0xffffffff,'08x'))==(r['bytes'],r['sha256'],r['crc32'])
        assert info.file_size==r['bytes'] and format(info.CRC,'08x')==r['crc32']
save('publication/postseal-physical-readback-v1.json',dict(status='POSTSEAL_PHYSICAL_SHA_AND_ALL_PAYLOAD_SHA_SIZE_CRC_PASS',
    archive=pin(path),manifest=pin(HERE/'publication/postseal-manifest-v1.json'),files=len(rows),
    metadata_sha256=sha(metadata),main_parts=12,final_complementary_parts=13,
    independent_archive=True,binary_volumes=False,external_upload='NOT_PERFORMED'))
combined=main['files']+[dict(r,part=path.name) for r in rows]
assert len(combined)==len({r['path'] for r in combined})==len({r['path'].casefold() for r in combined})
save('publication/final-logical-inventory-v1.json',dict(files=combined,
    main=pin(HERE/'publication/transport-manifest-v1.json'),main_physical=pin(HERE/'publication/parts-index-v1.json'),
    postseal=pin(HERE/'publication/postseal-physical-readback-v1.json'),complementary_parts=13,
    scope='Common path-level logical union; metadata self-members excluded to avoid recursive hashes; local private authorities remain outside derivative transport.'))
# Recheck source and Git after the additive supplement. This is not relabelled
# as another full private audit; that audit precedes this small additive packet.
spec=importlib.util.spec_from_file_location('entry_reader',HERE/'entry/authenticate-entry-v2.py')
entry=importlib.util.module_from_spec(spec);spec.loader.exec_module(entry)
sources=load(ACCEPTED/'production-test-freeze-v1.json')['sources']
with concurrent.futures.ThreadPoolExecutor(max_workers=4) as pool:checks=list(pool.map(entry.check,sources))
assert all(r['status']=='VERIFIED_PASS' for r in checks)
git=entry.git_receipt();save('final/git-after-postseal-v2.json',git)
assert git['index']['sha256']=='2036821003b1e1fb1077e72183e0b6c2fa7d22cdb2e784b760637e6d6fe9bc2c'
assert not git['commands']['staged']['stdout'] and git['commands']['diff-check']['exit_code']==0
save('final/postseal-source-lock-v1.json',dict(source_files=len(checks),changed=[],
    Git=pin(HERE/'final/git-after-postseal-v2.json'),postseal=pin(HERE/'publication/postseal-physical-readback-v1.json'),
    preservation_scope='Full31910 inherited audit after main publication; small postseal source/Git and actual new ZIP payload readback only, never described as second full preservation audit.'))
print('Final supplemental archive',path.stat().st_size,'bytes;',len(rows),'files; all13 complementary archives accounted',flush=True)
