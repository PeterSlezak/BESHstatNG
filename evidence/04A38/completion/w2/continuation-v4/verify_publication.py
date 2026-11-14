"""Independent complete-inventory and physical-ZIP verifier; never a READY gate.

Historical adverse fixture exceptions bind exact immutable whole-file hashes.
They do not permit raw diagnostics, new URI values, changed files or extra ZIP
members. No production/reference bytes are sanitized by this reader.
"""
import hashlib,json,re,zipfile,stat
from pathlib import Path,PurePosixPath
from collections import Counter
from verify_ownership_v2 import inspect as ownership
from verify_preservation import inspect as preservation
from verify_fragment_scope import inspect as scope_addendum
ROOT=Path(__file__).resolve().parents[5];HERE=Path(__file__).resolve().parent;PREFIX=HERE.relative_to(ROOT).as_posix()+'/'
FIXTURES={
 'evidence/04A38/completion/w2/continuation-v1/test_publication.py':('77db10fd2989f6f8b688f603e89ed280d1582fe397f82aa31f8ba0b460f174f1',{'home'}),
 'evidence/04A38/managed-test-identities-entry.csv':('7bec5946a791584dcc548bcfc5adb9550c9ecb7d3f92d004414e6b4e31b9d2d3',{'credential_uri'}),
 'BESHStat.Excel.Common.Tests/src/CompositeOutputTests.vb':('78ef6a5e623e69cd0196fa1da48eb36f223b4de7ed509c2eff358c17d7d24f4d',{'credential_uri'}),
 'BESHStat.Excel.Common.Tests/src/UserFaultTests.vb':('68903d3e6efea9a31983b78bc04fc5ceac7055dcf6bbb0379ce95a519ec4bb94',{'home'})}
PATTERNS={
 'home':re.compile(r'(?i)(?:[a-z]:[\\/]+users[\\/]+[^\\/\s"\x27]+|[/\\](?:home|Users)[/\\]+[^/\\\s"<>]+)'),
 'private_key':re.compile(r'-----BEGIN (?:RSA |EC |OPENSSH )?PRIVATE KEY-----'),
 'credential_token':re.compile(r'\b(?:gh[pousr]_[A-Za-z0-9]{20,}|AKIA[A-Z0-9]{16}|sk-[A-Za-z0-9]{24,})'),
 'credential_uri':re.compile(r'https?://[^\s/"<>:@]+:[^\s/"<>@]+@')}
def need(v,m):
 if not v:raise ValueError(m)
def sha(b):return hashlib.sha256(b).hexdigest()
def forbidden(p):return '__pycache__' in p.split('/') or p.lower().endswith(('.pyc','.pyo','/full-managed-debug-v1.json'))
def safe_path(p):
 need(isinstance(p,str) and p and '\\' not in p and ':' not in p and PurePosixPath(p).as_posix()==p and not p.startswith('/'),'Noncanonical path')
 need(all(x not in {'','.','..'} and not x.endswith((' ','.')) and not re.search(r'[\x00-\x1f<>"|?*]',x) and not re.match(r'(?i)^(CON|PRN|AUX|NUL|COM[1-9]|LPT[1-9])(?:\.|$)',x) for x in p.split('/')),'Unsafe/reserved path')
 need(not forbidden(p),'Forbidden raw diagnostic/cache '+p)
_PRIVACY_CACHE={}
def privacy(p,b):
 key=(p,sha(b))
 if key in _PRIVACY_CACHE:return _PRIVACY_CACHE[key]
 views={'raw':b.decode('utf-8-sig',errors='replace').replace('\\\\','\\')}
 if b.startswith((b'\xff\xfe',b'\xfe\xff')):views['utf16']=b.decode('utf-16').replace('\\\\','\\')
 # Raw view already unfolds escaped backslashes; only Unicode/slash escapes
 # can hide the remaining pattern characters. Parse those JSON values too.
 if p.endswith('.json') and (b'\\u' in b or b'\\/' in b or b.startswith((b'\xff\xfe',b'\xfe\xff'))):
  try:
   obj=json.loads(b)
   def strings(x):
    if isinstance(x,str):yield x
    elif isinstance(x,list):
     for v in x:yield from strings(v)
    elif isinstance(x,dict):
     for k,v in x.items():yield k;yield from strings(v)
   views['json_strings']='\n'.join(strings(obj))
  except (ValueError,UnicodeError):pass
 findings=[]
 for view,text in views.items():
  for kind,pattern in PATTERNS.items():
   hits=pattern.findall(text)
   if not hits:continue
   allowed=FIXTURES.get(p)
   need(allowed and sha(b)==allowed[0] and kind in allowed[1],'Unattested privacy finding '+p+' ['+kind+'/'+view+']')
   findings.append(dict(path=p,kind=kind,view=view,whole_file_sha256=sha(b),occurrences=len(hits),fragment_sha256_counts=dict(Counter(sha(x.encode()) for x in hits)),classification='EXACT_INHERITED_SYNTHETIC_ADVERSE_FIXTURE;NOT_MACHINE_OR_REAL_CREDENTIAL'))
 _PRIVACY_CACHE[key]=findings
 return findings
def inspect(root=ROOT,manifest_path=None,overrides=None,semantic=True):
 root=Path(root);overrides=overrides or {};manifest_path=manifest_path or PREFIX+'publication-manifest-v1.json'
 def raw(p):return overrides[p] if p in overrides else (root/p).read_bytes()
 def pairs(items):
  d={}
  for k,v in items:need(k not in d,'Duplicate JSON key');d[k]=v
  return d
 def load(p):return json.loads(raw(p),object_pairs_hook=pairs)
 m=load(manifest_path);need(m['status']=='NOT_READY_04A38','False READY');rows=m['entries'];paths=[r['path'] for r in rows];need(paths==sorted(paths) and len(paths)==len({p.casefold() for p in paths}),'Duplicate/casefold/unsorted paths');need(paths.count(manifest_path)==1,'Self missing')
 nulls=0;findings=[]
 for r in rows:
  p=r['path'];safe_path(p);need((root/p).resolve().is_relative_to(root.resolve()),'Escaping path')
  if r['bytes'] is None or r['sha256'] is None:
   nulls+=1;need(p==manifest_path and r['bytes'] is None and r['sha256'] is None,'Illegal null/recursive self')
  else:
   need(type(r['bytes']) is int and r['bytes']>=0 and bool(re.fullmatch('[0-9a-f]{64}',r['sha256'])),'Malformed pin');b=raw(p);need((len(b),sha(b))==(r['bytes'],r['sha256']),'Payload mismatch '+p)
  findings+=privacy(p,raw(p))
 need(nulls==1,'Exactly one nonrecursive null self')
 need(load(PREFIX+m['inventory'])['paths']==paths,'Inventory mismatch')
 need(set(FIXTURES).issubset(paths),'Missing immutable source/fixture exception provenance')
 ready=load(PREFIX+'readiness.json');need(ready['status']=='NOT_READY_04A38' and ready['W2']=='IN_PROGRESS' and ready['W3']=='NOT_STARTED','Unsupported closure')
 need(ready['new_original_queries']==0 and ready['owner_decision_needed'] is False,'Extra oracle authority')
 need(ready['gates']==dict(P='NO',N1='NO GO',N2='NO',C2='UNMET',C3='UNAPPROVED',golden='NOT_GOLDEN'),'Restricted gate promotion')
 v=load(PREFIX+'validation.json');need(ready['validation']==v,'Stale readiness validation');need(v['managed_Debug']['outcomes']=={'Passed':20674,'Failed':14},'Inherited managed nonpasses lost')
 need(v['new_original_queries']==0 and any('Full managed Release' in s for s in v['NOT_RUN']),'Invented original/full Release execution')
 # Reversal proves the public diagnostic changed only declared display labels.
 d=load(PREFIX+'diagnostic-public-derivative.json');public=load(d['public']['path'])
 for change in d['changes']:
  loc=public
  for step in change['pointer'][:-1]:loc=loc[step]
  need(sha(loc[change['pointer'][-1]].encode())==change['public_label_sha256'],'Public label changed')
 need(d['excluded_raw_retained_locally'] and not forbidden(d['public']['path']),'Raw provenance missing')
 result=dict(status='VERIFIED_PARTIAL_C4_NOT_READY',hashed_payloads=len(rows)-1,null_self_entries=1,privacy_findings=findings,physical_zip='VERIFY_SEPARATELY',limitations='Included bytes only; no live index/omitted owner docs/private binaries/ignored history or native/scientific proof')
 if semantic:
  result['ownership']=ownership(root,overrides);result['preservation']=preservation(root,False)
  result['scope_addendum']=scope_addendum(root,overrides)
  need(result['ownership']['historical']==ready['ownership']['historical'] and result['ownership']['current']==ready['ownership']['current'],'Ownership readiness mismatch')
 return result
def inspect_zip(path,manifest_path):
 # Inspect every physical ZIP member, including anything outside the manifest.
 with zipfile.ZipFile(path) as z:
  info=z.infolist();names=[i.filename for i in info]
  need(len(names)==len(set(names))==len({p.casefold() for p in names}),'Physical duplicate/casefold paths')
  for i in info:
   safe_path(i.filename);need(not i.is_dir() and not i.flag_bits&1 and not stat.S_ISLNK(i.external_attr>>16),'Directory/encrypted/symlink member')
  need(manifest_path in names,'Missing physical manifest');m=json.loads(z.read(manifest_path));mp=[r['path'] for r in m['entries']]
  need(len(mp)==len(set(mp))==len({p.casefold() for p in mp}),'Duplicate physical manifest entry')
  need(sum(r['bytes'] is None or r['sha256'] is None for r in m['entries'])==1,'Illegal ZIP null self')
  need(set(names)==set(mp),'Physical ZIP extra/missing payload');findings=[]
  for r in m['entries']:
   b=z.read(r['path']);findings+=privacy(r['path'],b)
   if r['path']==manifest_path:need(r['sha256'] is None and r['bytes'] is None,'Recursive self')
   else:need(type(r['bytes']) is int and (len(b),sha(b))==(r['bytes'],r['sha256']),'ZIP payload mismatch')
  need(z.testzip() is None,'ZIP CRC failure')
 return dict(status='COMPLETE_PHYSICAL_ZIP_HASH_CRC_PRIVACY_PASS',entries=len(names),privacy_findings=findings,scope='All actual members, not manifest subset')
if __name__=='__main__':
 import sys
 try:
  result=inspect_zip(sys.argv[2],sys.argv[1]) if len(sys.argv)>2 else inspect(manifest_path=sys.argv[1] if len(sys.argv)>1 else None)
  print(json.dumps(result))
 except Exception as e:print(json.dumps(dict(status='NOT_READY_04A38',error=type(e).__name__+': '+str(e))));raise SystemExit(1)
