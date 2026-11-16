from common47 import *
import zipfile,zlib
lock()
with (HERE/'final/owner-review-v1.md').open('xb') as f:f.write((ROOT/'docs/architecture/batch-04A-47-owner-review.md').read_bytes())
paths=[]
def add(p):
 p=str(p).replace('\\','/');assert (ROOT/p).is_file();paths.append(p)
for r in load(HERE/'matrix/source-freeze-v1.json')['new_tests']:add(r['path'])
add('BESHStatNG.Test/BESHStatNG.Tests.vbproj')
for r in load(HERE/'final/source-review-v1.json')['shipping_sources']:add(r['path'])
for p in ['docs/architecture/batch-04A-47-owner-review.md','planning/BESHStatNG_CrossPlatform_MasterPlan.md','planning/BESHStatNG_CrossPlatform_MasterPlan2.23.md','planning/BESHStatNG_CrossPlatform_MasterPlan2.12.md','scripts/verify-04a13-publication.py','scripts/verify-04a14-publication.py','scripts/test_suite_rationalization.py','scripts/tests/test_04a23_multivariate_spc.py','scripts/tests/test_04a24_multivariate_payload.py','evidence/04A39/physical-witness-v1/dependencies/privacy_regex_guard.py','evidence/04A45/native-risk/owner-decision-v1.md','evidence/04A46/native-risk/owner-decision-addendum-v1.md','evidence/04A45/scope/original-obligations-v1.json','evidence/04A45/scope/ribbon-v1.json','evidence/04A45/fixtures/InspectWiring45.cs','evidence/04A45/fixtures/ResourceBoundary45.cs']:add(p)
for folder in ['entry','matrix','native-risk','planning','preservation','final','compatibility','build-and-tests','fixtures','waves','source','privacy','abi-cil']:
 for p in sorted((HERE/folder).rglob('*')):
  if p.is_file():add(p.relative_to(ROOT))
for p in ['scope/family-review-v3.json','scope/orphan-method-v1.json','scope/compiled-members-v1.json','scope/accepted-contract-crosswalk-v1.json','scope/execution-chains-v1.json','scope/compile-membership-v1.json','scope/selection-a-v1.json','scope/selection-b-v1.json','register/interim-delta-v2.json','python/discovery-v1.json','python/dispositions-v1.json','python/parent-event-reconciliation-v2.json','python/driver-provenance-v1.json']:add('evidence/04A47/'+p)
for p in sorted(HERE.glob('*.py')):add(p.relative_to(ROOT))
paths=sorted(set(paths));manifest='evidence/04A47/final/package-members-v1.json';paths.append(manifest);paths.sort()
save('final/package-members-v1.json',dict(members=paths,scope='Textual source/software/evidence review;compact maps only,no raw model dumps,TRX,private argv,binaries,workbooks or actual native fingerprints.',external_receipt='Separate owner-local publication directory;not inside ZIP.',status='NOT_READY_04A47_SAFE_SOFTWARE',native='NOT_RUN'))
folder=PRIVATE/'publication-dry-v1';folder.mkdir();archive=folder/'review-dry-v1.zip'
contents={n:(ROOT/n).read_bytes() for n in paths};contents['.review-index.json']=(json.dumps(dict(files=[dict(path=n,bytes=len(b),sha256=sha(b),crc32=f'{zlib.crc32(b)&0xffffffff:08x}') for n,b in sorted(contents.items())]),indent=2)+'\n').encode()
with zipfile.ZipFile(archive,'x',zipfile.ZIP_DEFLATED,compresslevel=6) as z:
 for n,b in sorted(contents.items()):z.writestr(n,b)
reader=module('reader47_dry',HERE/'review_reader.py');verified=reader.verify(archive);print(verified,flush=True)
controls=reader.adverse(archive);print('Executed adverse controls',len(controls),flush=True)
neutral=module('neutral47_dry',HERE/'neutral_privacy_replay.py').replay(archive)
save(folder/'readback-v1.json',dict(archive=pin(archive),reader=verified,adverse=controls,neutral=neutral))
print('Dry ZIP/readback/adverse/neutral replay passed;software status remains NOT_READY',flush=True)
