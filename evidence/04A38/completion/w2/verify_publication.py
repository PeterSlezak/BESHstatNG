"""Independent bounded W2 publication reader; unresolved ownership forbids READY."""
import re
from pathlib import Path,PurePosixPath
from verify_profile_slice import inspect as profile,ROOT,W,load,need,digest
from verify_delegated_bodies_v2 import inspect as delegated
from verify_preservation import inspect as protection
SELF=W+'publication-manifest-v1.json'

def inspect(root=ROOT):
 root=Path(root);manifest=load(root/SELF);rows=manifest['entries'];paths=[r['path'] for r in rows]
 need(manifest['status']=='NOT_READY_04A38','False READY promotion')
 need(paths==sorted(paths) and len(paths)==len(set(p.casefold() for p in paths)),'Duplicate/casefold/unsorted publication paths')
 need(paths.count(SELF)==1,'Missing self entry')
 for row in rows:
  p=row['path'];pure=PurePosixPath(p)
  need(not pure.is_absolute() and pure.as_posix()==p and '\\' not in p and ':' not in p,'Unsafe/noncanonical path')
  need(all(x not in {'','.','..'} and not x.endswith((' ','.')) and not re.search(r'[\x00-\x1f<>"|?*]',x) and not re.match(r'(?i)^(CON|PRN|AUX|NUL|COM[1-9]|LPT[1-9])(?:\.|$)',x) for x in p.split('/')),'Reserved/escaping path')
  need((root/p).resolve().is_relative_to(root.resolve()),'Symlink escape')
  if p==SELF:need(row['bytes'] is None and row['sha256'] is None,'Recursive self hash')
  else:
   data=(root/p).read_bytes();need(len(data)==row['bytes'] and digest(data)==row['sha256'],'Publication payload changed '+p)
   if p.startswith(W):
    need(not re.search(rb'(?i)[a-z]:[\\/]+users[\\/]+[^\\/\s"\x27]+',data),'Private owner home path in W2 publication')
    need(not re.search(rb'-----BEGIN (?:RSA |EC |OPENSSH )?PRIVATE KEY-----',data),'Private key in W2 publication')
 need(load(root/(W+'file-inventory.json'))['paths']==paths,'Publication inventory mismatch')
 protected=protection(root,False);p=profile(root);d=delegated(root)
 need(p==load(root/(W+'independent-profile-proof.json')) and d==load(root/(W+'independent-delegated-proof-v2.json')),'Source/IL proof differs from independently recomputed result')
 ledger=load(root/(W+'historical-ownership-checkpoint.json'));expected={r['canonical_original_id'] for r in p['targets']}|{r['original_id'] for r in d['rows'] if not r['status'].startswith('OPEN')}
 original=load(root/'evidence/04A38/completion/w1/member-behavior-evidence-target-matrix-v3.json')['members']
 need(len(ledger['members'])==704 and {r['canonical_original_id'] for r in ledger['members']}=={r['canonical_original_id'] for r in original},'Historical identity omitted/extra')
 need({r['canonical_original_id'] for r in ledger['members'] if r['disposition']=='VERIFIED_PORTABLE_BODY'}==expected and ledger['closed']==47 and ledger['OPEN']==657,'Unsupported whole-body closure')
 ready=load(root/(W+'readiness.json'))
 need(ready['status']=='NOT_READY_04A38' and ready['W2']=='IN_PROGRESS' and ready['W3']=='NOT_STARTED' and ready['historical_OPEN']==657,'Incomplete work promoted to READY')
 need(ready['gates']==dict(P='NO',N1='NO GO',N2='NO',C2='UNMET',C3='UNAPPROVED',golden='NOT_GOLDEN'),'Scientific/native promotion')
 for config in ['Debug','Release']:
  a=load(root/(W+'A-compatibility-'+config+'.json'))
  need((root/a['original']['path']).read_bytes()==(root/a['current']['path']).read_bytes() and a['unique_cases']==432,'Approval A comparison changed')
 return dict(status='VALID_W2_PARTIAL_PUBLICATION_NOT_READY',hashed_payloads=len(rows)-1,historical_proof_supported=47,historical_OPEN=657,source_and_IL_proof='RECOMPUTED_FROM_PUBLISHED_SOURCE_AND_METADATA',binary_boundary='Original/current compilation and CIL were executed locally; private loaded binaries/package cache are not in this compact publication and cannot be independently loaded from it',protection=protected)
if __name__=='__main__':
 import json
 print(json.dumps(inspect()))
