"""One new ACTIVE ZIP plus eleven unchanged W2 reference ZIPs; no recompression of history."""
from common import *
import copy,zipfile,zlib,stat
sys.path.append(str(ROOT/'evidence/04A38'))
from baseline_report import clean
sys.path.append(str(W2/'continuation-v6'))
import publication_policy as policy
sys.path.append(str(ACCEPTED))
from verify_transport_v1 import safe_path

META='.w3-review-manifest.json'


def main():
    core=load(W2/'continuation-v17-postcorrection-v1-review-parts-index-v1.json')
    post=load(W2/'continuation-v17-postcorrection-v1-final-additive-readback-v1.json')
    prior={r['path']:copy.deepcopy(r) for r in core['files']}
    parts=[dict(name=p['name'],path=p['path'],bytes=p['bytes'],sha256=p['sha256'],metadata='.review-manifest.json',metadata_sha256=core['embedded_manifest_sha256'],role='UNCHANGED_W2_CORE_REFERENCE') for p in core['parts']]
    supp=post['postseal_supplement']
    parts.append(dict(name=Path(supp['path']).name,**supp,metadata='.postseal-manifest.json',metadata_sha256=post['supplement_manifest_sha256'],role='UNCHANGED_W2_POSTSEAL_REFERENCE'))
    for row in post['supplement_files']:
        p={k:row[k] for k in ['path','bytes','sha256','crc32']};p['part']=Path(supp['path']).name
        assert p['path'] not in prior;prior[p['path']]=p
    # New passive metadata is an exact derivative of private compiler output;
    # copy only these named files, never the private execution tree.
    metadata=[]
    for config in ['Debug','Release']:
        for name in ['current.json','exports.json']:
            source=PRIVATE/config/'metadata'/name
            dest=HERE/'abi-cil/review-metadata'/config/name
            dest.parent.mkdir(parents=True,exist_ok=True)
            with dest.open('xb') as f:f.write(source.read_bytes())
            assert dest.read_bytes()==source.read_bytes()
            metadata.append(dict(raw=pin(source),derivative=pin(dest),exact=True))
    save('publication/compiled-metadata-crosswalk-v1.json',dict(entries=metadata))
    sources=load(ACCEPTED/'production-test-freeze-v1.json')['sources']
    selected={p.relative_to(ROOT).as_posix() for p in HERE.rglob('*') if p.is_file() and '__pycache__' not in p.parts}
    selected.update(r['path'] for r in sources if r['path'] not in prior)
    selected.update(p.relative_to(ROOT).as_posix() for p in W2.glob('continuation-v17-postcorrection-v1-*') if p.is_file() and p.relative_to(ROOT).as_posix() not in prior)
    selected.discard((HERE/'publication/transport-manifest-v1.json').relative_to(ROOT).as_posix())
    active=[];exclusions=[];derivatives=[];decisions=[]
    zipname='BESHstatNG_04A38_W3_ACTIVE_v1.zip'
    for name in sorted(selected):
        assert name not in prior
        safe_path(name);p=ROOT/name;data=p.read_bytes();raw=pin(p)
        try:findings=policy.privacy(name,data)
        except ValueError as error:
            assert 'Unattested privacy finding' in str(error)
            try:
                obj=json.loads(data);review=(json.dumps(clean(obj),indent=2,ensure_ascii=False)+'\n').encode()
            except (ValueError,UnicodeError):review=clean(data.decode('utf-8-sig',errors='replace')).encode()
            suffix='.review.json' if name.endswith('.json') else '.review.txt'
            out=HERE/'publication/derivatives'/(name+suffix)
            try:findings=policy.privacy(out.relative_to(ROOT).as_posix(),review)
            except ValueError:
                # Authenticated omission is transport-only, never a privacy waiver
                # or a replacement of the authoritative local source/diagnostic.
                review=(json.dumps(dict(raw=raw,review_only='Values omitted from derivative transport; local authoritative bytes retained. This file does not supply executable source or raw diagnostic proof.'),indent=2)+'\n').encode()
                out=HERE/'publication/derivatives'/(name+'.omitted-values.review.json')
                findings=policy.privacy(out.relative_to(ROOT).as_posix(),review)
            out.parent.mkdir(parents=True,exist_ok=True)
            with out.open('xb') as f:f.write(review)
            exclusions.append(raw);derivatives.append(dict(raw=raw,derivative=pin(out)))
            name=out.relative_to(ROOT).as_posix();data=review;p=out
        row=pin(p);row.update(crc32=format(zlib.crc32(data)&0xffffffff,'08x'),part=zipname)
        active.append(row);decisions.append(dict(path=name,sha256=row['sha256'],findings=findings,policy_sha256=pin(W2/'continuation-v6/publication_policy.py')['sha256']))
    save('publication/privacy-and-exclusions-v1.json',dict(decisions=decisions,excluded_authorities=exclusions,derivatives=derivatives,
        repository_privacy='NOT_VERIFIED_CLEAN;TWO_HISTORICAL_UNICODE_ERROR_PARENTS_UNWAIVED',
        private_raw='NOT_INCLUDED;PIN_AND_MESSAGE_LEDGERS_ONLY',giant='NOT_PARSED_OR_PACKAGED',privacy_waivers_changed=False,
        source_count=1350,source_transport_limitations=[r for r in derivatives if r['raw']['path'] in {s['path'] for s in sources}]))
    # This policy receipt is itself clean bookkeeping and gets a final direct check.
    p=HERE/'publication/privacy-and-exclusions-v1.json';data=p.read_bytes();policy.privacy(p.relative_to(ROOT).as_posix(),data)
    active.append(dict(pin(p),crc32=format(zlib.crc32(data)&0xffffffff,'08x'),part=zipname))
    rows=sorted([*prior.values(),*active],key=lambda r:r['path'])
    paths=[safe_path(r['path']) for r in rows]
    assert len(paths)==len(set(paths))==len({p.casefold() for p in paths})
    manifest=dict(schema='W3_COMPLEMENTARY_TRANSPORT_V1',scope='GIT_FREE_REVIEW_DERIVATIVE;NOT_A_LIVE_CHECKOUT',
        files=rows,reference_parts=parts,new_active_name=zipname,private_raw_included=False,
        prior_manifest=pin(W2/'continuation-v17-postcorrection-v1-review-parts-index-v1.json'),
        postseal_manifest=pin(W2/'continuation-v17-postcorrection-v1-final-additive-readback-v1.json'),
        exclusions=pin(HERE/'publication/privacy-and-exclusions-v1.json'),
        source_freeze=pin(ACCEPTED/'production-test-freeze-v1.json'),
        status='SOFTWARE_REVIEW_DERIVATIVE;NO_ACCEPTANCE_OR_NATIVE_SCIENTIFIC_RELEASE',
        original_queries=0,binary_volumes=False)
    save('publication/transport-manifest-v1.json',manifest)
    metadata_bytes=(HERE/'publication/transport-manifest-v1.json').read_bytes()
    folder=PRIVATE/'review-parts-v1';folder.mkdir(exist_ok=False)
    path=folder/zipname
    with zipfile.ZipFile(path,'x',compression=zipfile.ZIP_DEFLATED,compresslevel=6,allowZip64=True) as z:
        z.writestr(META,metadata_bytes)
        for row in active:
            assert pin(ROOT/row['path'])=={k:row[k] for k in ['path','bytes','sha256']}
            z.write(ROOT/row['path'],row['path'])
    assert path.stat().st_size<180*1024**2,'PHYSICAL_REVIEW_LIMIT_EXCEEDED;KEEP_STOPPED_ARCHIVE'
    save('publication/parts-index-v1.json',dict(manifest=pin(HERE/'publication/transport-manifest-v1.json'),
        new_active=pin(path),reference_parts=parts,required_parts=len(parts)+1,
        notes='Upload this ACTIVE plus ten unchanged W2 core ZIPs and separate unchanged POSTSEAL_SUPPLEMENT. Existing reference ZIPs are not rebuilt or claimed newly uploaded. One common logical manifest joins all twelve parts; each physical archive has its own exact hash.',
        external_upload='NOT_PERFORMED',new_active_files=len(active),logical_payloads=len(rows)))
    print('New ACTIVE bytes',path.stat().st_size,'logical payloads',len(rows),'required complementary archives',len(parts)+1,flush=True)


if __name__=='__main__':main()
