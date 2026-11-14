"""Independent complete-inventory and physical-ZIP verifier; never a READY gate.

Historical adverse fixture exceptions bind exact immutable whole-file hashes.
They do not permit raw diagnostics, new URI values, changed files or extra ZIP
members. No production/reference bytes are sanitized by this reader.
"""
import hashlib,json,re,zipfile,stat
from pathlib import Path,PurePosixPath
from collections import Counter
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
def forbidden(p):return '__pycache__' in p.split('/') or p.lower().endswith(('.pyc','.pyo')) or bool(re.search(r'/full-managed-debug-v[0-9]+\.json$',p.lower()))
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
