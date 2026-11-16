from common45 import *
import re
lock();records=[]
for p in sorted((PRIVATE/'commands').glob('*/command.json')):
 d=load(p);args=[]
 for i,a in enumerate(d['command']):
  s=str(a).replace(str(ROOT),'<repo>').replace(ROOT.as_posix(),'<repo>').replace(str(Path(sys.executable)),'<existing-python>')
  if i==0 and re.match(r'^[A-Za-z]:',s):s='<installed-tool>/'+Path(a).name
  if re.search(r'(?i)\b[a-z]:[/\\]',s):
   prefix=s.split(':',1)[0]+':' if s.startswith('/') else ''
   s=prefix+'<installed-reference>/'+Path(a).name
  args.append(s.replace('\\','/'))
 records.append(dict(label=p.parent.name,command=args,exit_code=d['exit_code'],seconds=d['seconds'],raw=pin(p),stdout_sha256=d['stdout_sha256'],stderr_sha256=d['stderr_sha256']))
save('matrix/actual-commands-review-v1.json',dict(scope='Actual execution commands with path-independent installed-tool/repo aliases;exact argv/cwd/stdout/stderr retained privately under pinned receipts. No private raw arguments published.',commands=records,unsafe_fixed_path_wrappers='NOT_RUN;verify-beshstatng.ps1 and -Legacy would overwrite protected TRX paths. Unchanged isolated runner used instead.',native='NOT_RUN',node='INHERITED_916_PASS;NOT_RERUN',gates=GATES))
