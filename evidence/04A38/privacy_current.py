"""Scan proposed batch artifacts, never raw private run logs or TestResults."""
from collections import Counter
import os,re
from pathlib import Path
from preflight import ROOT,HERE,git,save,sha

def proposed():
 tracked=git('diff','--name-only','-z').decode().split('\0')
 untracked=git('ls-files','--others','--exclude-standard','-z').decode().split('\0')
 # Debug/Release directory ignore rules also match structured capture folders.
 # The explicit packet inventory must include these, even though Git's default
 # untracked listing omits them. No ignore rule or index state is changed.
 batch=[p.relative_to(ROOT).as_posix() for p in HERE.rglob('*') if p.is_file()]
 return sorted(set(p for p in tracked+untracked+batch if p and (ROOT/p).is_file()))

def inspect(paths):
 patterns=[('PRIVATE_HOME',re.compile(rb'[A-Za-z]:[\\/]+Users[\\/]+[^\\/\s"\']+',re.I)),
           ('UNIX_HOME',re.compile(rb'/(?:home|Users)/[^/\s"\']+')),
           ('PRIVATE_SID',re.compile(rb'S-1-5-\d+(?:-\d+)*'))]
 for key in ['USERNAME','COMPUTERNAME']:
  value=os.environ.get(key,'')
  if value:patterns.append((key,re.compile(rb'(?<![A-Za-z0-9_])'+re.escape(value.encode())+rb'(?![A-Za-z0-9_])',re.I)))
 findings=[]
 for path in paths:
  data=(ROOT/path).read_bytes()
  for category,pattern in patterns:
   for match in pattern.finditer(data):findings.append(dict(path=path,category=category,offset=match.start(),redacted_digest=sha(match.group())))
  if Path(path).suffix.lower() in {'.dll','.exe','.pdb','.pfx','.key','.snk','.zip'}:findings.append(dict(path=path,category='BINARY_OR_SECRET_MATERIAL_NOT_ALLOWED'))
 return findings

if __name__=='__main__':
 paths=proposed();findings=inspect(paths)
 save('privacy-validation-v3.json',dict(status='PASS' if not findings else 'NOT_READY_04A38',artifacts=len(paths),findings=findings,
  scope='Proposed changed/untracked artifacts only; no private raw test/trace logs, package/signing material, old TestResults traversal or external publication'))
 print('Privacy scan',len(paths),'artifacts;',len(findings),'redacted findings')
 raise SystemExit(bool(findings))
