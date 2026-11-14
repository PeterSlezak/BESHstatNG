"""Additive runtime dependency publication; immutable seven-part union retained."""
from common import *
import copy
from publish_parts_v15_v2 import make_zip, verify_zip, external
import publication_privacy_c15_v1 as policy

PREFIX=HERE.relative_to(ROOT).as_posix()+'/'
priorpath=W2/'continuation-v15-review-parts-index-v1.json'
prior=json.loads(priorpath.read_bytes())
oldmanifest=json.loads((HERE/'publication-manifest-v1.json').read_bytes())
oldrows={r['path']:r for r in oldmanifest['entries']}
assert len(prior['files'])==len(oldrows)==9724 and len(prior['parts'])==7
names={r['path'] for r in prior['files']}
names.update([
 'scripts/test_suite_rationalization.py',
 PREFIX+'verify_c15_publication_v3.py',PREFIX+'postseal_verify_c15_v2.py',
 PREFIX+'copy_c15_review_union_v3.py',PREFIX+'publish_dependency_supplement_v1.py',
 PREFIX+'REVIEW-ARCHIVES-addendum-v2.md',PREFIX+'publication-manifest-v2.json',
 PREFIX+'file-inventory-v2.json',
])
names.update(p.relative_to(ROOT).as_posix() for p in W2.glob('continuation-v15-*') if p.is_file() and p.suffix in ['.py','.json','.md'])
assert len(names)==len({p.casefold() for p in names})
save('file-inventory-v2.json',dict(status='ADDITIVE_RUNTIME_DEPENDENCY_COMPLETE_UNION',paths=sorted(names),previous_index=pin(priorpath),original_queries=0))
rows=[]
for name in sorted(names):
 policy.safe_path(name)
 if name==PREFIX+'publication-manifest-v2.json':
  rows.append(dict(path=name,bytes=None,sha256=None));continue
 row=pin(ROOT/name)
 if name in oldrows and oldrows[name]['sha256'] is not None:
  old=oldrows[name];assert (row['bytes'],row['sha256'])==(old['bytes'],old['sha256'])
  decision=old['privacy_decision'];assert decision['content_sha256']==row['sha256'] and decision['policy_sha256']==policy.base['sha256']
 else:
  findings=policy.privacy(name,(ROOT/name).read_bytes())
  decision=dict(status='POLICY_EVALUATED',policy_sha256=policy.base['sha256'],content_sha256=row['sha256'],findings=findings)
 rows.append(dict(row,privacy_decision=decision))
save('publication-manifest-v2.json',dict(status='NOT_READY_04A38',W2='IN_PROGRESS',W3='NOT_STARTED',entries=rows,privacy_policy=policy.base,privacy_checkpoint=pin(HERE/'publication-privacy-decisions-v1.json'),previous_index=pin(priorpath),correction='Additive dependency ZIP; no old manifest, archive, payload or checkpoint seal rewritten.',self_entry='One null self-entry; physical bytes bound by external index.',original_queries=0))
policy.privacy(PREFIX+'publication-manifest-v2.json',(HERE/'publication-manifest-v2.json').read_bytes())
oldnames={r['path'] for r in prior['files']}
added=[pin(ROOT/name) for name in sorted(names-oldnames)]
folder=PRIVATE/'review-parts-supplement-v1';folder.mkdir(exist_ok=False)
archive=folder/'BESHstatNG_04A38_W2_v15_DEPENDENCY_01.zip'
make_zip(archive,added);records=verify_zip(archive,added)
assert archive.stat().st_size<180*1024**2
repro=folder/(archive.name+'.determinism.zip');make_zip(repro,added)
assert (pin(archive)['bytes'],pin(archive)['sha256'])==(pin(repro)['bytes'],pin(repro)['sha256'])
part=dict(name=archive.name,group='DEPENDENCY',**pin(archive),files=len(added),deterministic_reproduction=pin(repro),CRC='ALL_MEMBERS_VERIFIED')
files=sorted(prior['files']+[dict(r,part=part['name'],group='DEPENDENCY',logical_root_prefix='') for r in records],key=lambda r:r['path'])
doc=dict(status='ADDITIVE_COMPLEMENTARY_LOGICAL_PHYSICAL_HASH_CRC_PASS_NOT_READY',manifest=pin(HERE/'publication-manifest-v2.json'),previous_index=pin(priorpath),parts=prior['parts']+[part],files=files,exclusions=prior['exclusions'],privacy_checkpoint=prior['privacy_checkpoint'],standalone='NOT_CREATED',original_queries=0)

def validate(d):
 mapping={r['path']:r for r in d['files']}
 assert len(mapping)==len(d['files'])==len({p.casefold() for p in mapping}) and set(mapping)==names
 assert all(mapping.get(r['path'])==r for r in prior['files'])
 assert d['parts'][:7]==prior['parts'] and len(d['parts'])==8
 assert 'scripts/test_suite_rationalization.py' in mapping
 for r in d['files']:
  policy.safe_path(r['path']);assert r['logical_root_prefix']==''
  assert r['part'] in {p['name'] for p in d['parts']}
validate(doc)
controls=[]
for label,mutate in [('missing runtime dependency',lambda d:d.update(files=[r for r in d['files'] if r['path']!='scripts/test_suite_rationalization.py'])),('changed prior payload SHA',lambda d:d['files'][0].update(sha256='0'*64)),('changed old archive',lambda d:d['parts'][0].update(sha256='0'*64))]:
 bad=copy.deepcopy(doc);mutate(bad)
 try:validate(bad)
 except (AssertionError,ValueError):controls.append(dict(name=label,status='REJECTED'))
 else:raise AssertionError('Invalid additive union accepted')
doc['controls']=controls
index=external('continuation-v15-review-parts-index-v2.json',doc)
external('continuation-v15-publication-receipt-v2.json',dict(status='ADDITIVE_EIGHT_PART_REVIEW_BYTES_VERIFIED_NOT_READY',manifest=doc['manifest'],parts_index=index,previous_index=doc['previous_index'],parts=doc['parts'],files=len(files),added_files=len(added),controls=controls,privacy_checkpoint=doc['privacy_checkpoint'],reviewer_upload='NOT_UPLOADED_BY_AGENT;LOCAL_ACTUAL_BYTES_HASHED',scope='Eight independent complementary ZIPs, all below180MiB. Seven prior ZIPs and all9724 prior payloads remain byte-identical. Final copied reader replay is separately required. No binary split volumes, no historical scanner/test rerun.',original_queries=0))
print('Additive dependency publication',len(files),'files',len(added),'added',pin(archive),flush=True)
