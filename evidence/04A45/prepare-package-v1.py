from common45 import *
import zipfile,zlib
lock()
paths=[]
def add(p):
 p=str(p).replace('\\','/');assert (ROOT/p).is_file();paths.append(p)
for r in load(HERE/'matrix/source-freeze-v1.json')['new_tests']:add(r['path'])
add('BESHStatNG.Test/BESHStatNG.Tests.vbproj')
for r in load(HERE/'final/source-review-v1.json')['shipping_sources']:add(r['path'])
for p in ['docs/architecture/batch-04A-45-owner-review.md','planning/BESHStatNG_CrossPlatform_MasterPlan.md','planning/BESHStatNG_CrossPlatform_MasterPlan2.21.md','planning/BESHStatNG_CrossPlatform_MasterPlan2.12.md','scripts/verify-04a13-publication.py','scripts/verify-04a14-publication.py','evidence/04A39/physical-witness-v1/dependencies/privacy_regex_guard.py']:add(p)
for folder in ['entry','matrix','native-risk','planning','register','preservation','final','compatibility','build-and-tests','fixtures','waves','source','python','privacy']:
 for p in sorted((HERE/folder).rglob('*')):
  if p.is_file():add(p.relative_to(ROOT))
for name in ['ribbon-v1.json','controls-v2.json','event-map-review-v1.json','execution-chains-v1.json','original-obligations-v1.json','carry-over-v2.json','census-tool-provenance-v3.json']:
 add('evidence/04A45/scope/'+name)
for name in ['Debug-proof-v1.json','Release-proof-v1.json','compiled-ui-and-build-attribution-v1.json','dependency-resource-boundary-v1.json']:add('evidence/04A45/abi-cil/'+name)
for name in ['review_reader.py','neutral_privacy_replay.py','common45.py','run45.py','prepare-wave-a-v1.py','prepare-wave-b-v1.py','publication45.py']:
 add('evidence/04A45/'+name)
paths=sorted(set(paths));manifest_path='evidence/04A45/final/package-members-v1.json'
assert manifest_path not in paths;paths.append(manifest_path);paths.sort()
save('final/package-members-v1.json',dict(members=paths,scope='Small textual software/evidence review;no binary/TRX/private raw/session/native/workbook artifacts',external_receipt='Separate ignored publication directory;not part of this ZIP',owner_private_raw=True))
archive=PRIVATE/'review-dry-run-v1.zip';contents={n:(ROOT/n).read_bytes() for n in paths};contents['.review-index.json']=(json.dumps(dict(files=[dict(path=n,bytes=len(b),sha256=sha(b),crc32=f'{zlib.crc32(b)&0xffffffff:08x}') for n,b in sorted(contents.items())]),indent=2)+'\n').encode()
with zipfile.ZipFile(archive,'x',zipfile.ZIP_DEFLATED,compresslevel=6) as z:
 for n,b in sorted(contents.items()):z.writestr(n,b)
reader=module('dry_reader45',HERE/'review_reader.py');print(reader.verify(archive),flush=True)
controls=reader.adverse(archive);print('Executed reader adverse controls',len(controls),flush=True)
neutral=module('dry_neutral45',HERE/'neutral_privacy_replay.py');result=neutral.replay(archive)
save(PRIVATE/'dry-review-v1.json',dict(archive=pin(archive),adverse=controls,neutral=result))
print('Dry ZIP/readback/adverse/neutral privacy replay succeeded',flush=True)
