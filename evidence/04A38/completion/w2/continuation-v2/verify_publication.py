"""Independent partial-packet hash/privacy/ownership gate; READY is prohibited here."""
import hashlib,json,re,os
from pathlib import Path,PurePosixPath
from verify_ownership_v4 import inspect as ownership
from verify_preservation import inspect as preservation
from verify_host_cache_v4 import inspect as generic_cache
from verify_final_domains import inspect as final_domains
ROOT=Path(__file__).resolve().parents[5];HERE=Path(__file__).resolve().parent;PREFIX=HERE.relative_to(ROOT).as_posix()+'/'
def need(v,m):
 if not v:raise ValueError(m)
def sha(b):return hashlib.sha256(b).hexdigest()
def inspect(root=ROOT,manifest_path=None,overrides=None):
 root=Path(root);overrides=overrides or {};manifest_path=manifest_path or PREFIX+'publication-manifest-v1.json'
 def raw(p):return overrides.get(p,(root/p).read_bytes())
 def pairs(items):
  d={}
  for k,v in items:need(k not in d,'Duplicate JSON key');d[k]=v
  return d
 def load(p):return json.loads(raw(p),object_pairs_hook=pairs)
 m=load(manifest_path);need(m['status']=='NOT_READY_04A38','False packet READY');rows=m['entries'];paths=[r['path'] for r in rows]
 need(paths==sorted(paths) and len(paths)==len({p.casefold() for p in paths}),'Duplicate/casefold/unsorted payloads');need(paths.count(manifest_path)==1,'Self entry missing');nulls=0
 for r in rows:
  p=r['path'];pure=PurePosixPath(p);need(not pure.is_absolute() and pure.as_posix()==p and '\\' not in p and ':' not in p,'Unsafe/noncanonical path')
  need(all(x not in {'','.','..'} and not x.endswith((' ','.')) and not re.search(r'[\x00-\x1f<>"|?*]',x) and not re.match(r'(?i)^(CON|PRN|AUX|NUL|COM[1-9]|LPT[1-9])(?:\.|$)',x) for x in p.split('/')),'Reserved/traversal path')
  need((root/p).resolve().is_relative_to(root.resolve()),'Path escapes root')
  if r['bytes'] is None or r['sha256'] is None:
   nulls+=1;need(p==manifest_path and r['bytes'] is None and r['sha256'] is None,'Illegal null/recursive self');continue
  b=raw(p);need((len(b),sha(b))==(r['bytes'],r['sha256']),'Payload mismatch '+p)
  if p.startswith(PREFIX):
   need(not re.search(rb'(?i)[a-z]:[\\/]+users[\\/]+[^\\/\s"\x27]+',b),'Private owner-home path '+p)
   need(not re.search(rb'(?i)[/\\](?:home|Users)[/\\]+[^/\\\s"<>]+',b),'Private home path '+p)
   need(not re.search(rb'-----BEGIN (?:RSA |EC |OPENSSH )?PRIVATE KEY-----',b),'Private key')
   need(not re.search(rb'\b(?:gh[pousr]_[A-Za-z0-9]{20,}|AKIA[A-Z0-9]{16}|sk-[A-Za-z0-9]{24,})',b),'Credential token '+p)
   need(not re.search(rb'https?://[^\s/"<>:@]+:[^\s/"<>@]+@',b),'Credential URL '+p)
   for token in [os.environ.get('USERNAME'),os.environ.get('COMPUTERNAME')]:
    if token:need(not re.search(rb'(?i)(?<![A-Za-z0-9])'+re.escape(token.encode())+rb'(?![A-Za-z0-9])',b),'Private environment identifier '+p)
 need(nulls==1,'Exactly one null self required');need(load(PREFIX+m['inventory'])['paths']==paths,'Inventory omitted/extra artifact')
 ready=load(PREFIX+m['readiness']);need(ready['status']=='NOT_READY_04A38' and ready['W2']=='IN_PROGRESS' and ready['W3']=='NOT_STARTED','Unsupported ready/W2 closure')
 need(ready['new_original_queries']==0 and ready['owner_decision_needed'] is False,'Extra reference authority or query')
 need(ready['gates']==dict(P='NO',N1='NO GO',N2='NO',C2='UNMET',C3='UNAPPROVED',golden='NOT_GOLDEN'),'Scientific/native promotion')
 p=preservation(root,False);o=ownership(root,overrides);v4=generic_cache(root,overrides);final_domains(root);need(len(v4['methods'])==33 and len(v4['initializers'])==21,'Missing full generic-context cache proof');need(o['historical']==ready['ownership']['historical'] and o['current']==ready['ownership']['current'],'Ownership/ready mismatch')
 need(ready['validation']['managed_Debug']==dict(Passed=20653,Failed=14),'Dropped inherited managed nonpasses')
 need(ready['validation']['full_Release']=='NOT_RUN_OWNER_PREFERENCE','Invented full Release run')
 return dict(status='VERIFIED_PARTIAL_CONTINUATION_V2_NOT_READY',hashed_payloads=len(rows)-1,null_self_entries=1,ownership=o,preservation=p,limitations='Included bytes and executable source readers only; copy cannot prove live Git/index/omitted owner documents/private loaded binaries/ignored history; no native/scientific acceptance')
if __name__=='__main__':
 import sys
 try:print(json.dumps(inspect(manifest_path=sys.argv[1] if len(sys.argv)>1 else None)))
 except Exception as e:print(json.dumps(dict(status='NOT_READY_04A38',error=type(e).__name__+': '+str(e))));raise SystemExit(1)
