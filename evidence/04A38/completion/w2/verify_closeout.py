"""New independent closeout layer; the v1 publication remains immutable."""
import re
from pathlib import Path,PurePosixPath
from verify_publication import ROOT,W,load,need,digest,inspect as base
SELF=W+'publication-manifest-closeout-v2.json'
def inspect(root=ROOT):
 root=Path(root);m=load(root/SELF);rows=m['entries'];paths=[r['path'] for r in rows]
 need(m['status']=='NOT_READY_04A38' and paths==sorted(paths),'False status or unsorted closeout')
 need(len(paths)==len(set(p.casefold() for p in paths)) and paths.count(SELF)==1,'Duplicate/casefold/self-entry error')
 for row in rows:
  p=row['path'];pure=PurePosixPath(p)
  need(not pure.is_absolute() and pure.as_posix()==p and ':' not in p and '\\' not in p and all(x not in {'','.','..'} and not x.endswith((' ','.')) and not re.search(r'[\x00-\x1f<>"|?*]',x) and not re.match(r'(?i)^(CON|PRN|AUX|NUL|COM[1-9]|LPT[1-9])(?:\.|$)',x) for x in p.split('/')),'Unsafe publication path')
  need((root/p).resolve().is_relative_to(root.resolve()),'Escaping symlink')
  if p==SELF:need(row['bytes'] is None and row['sha256'] is None,'Nonnull/recursive self entry')
  else:
   b=(root/p).read_bytes();need(len(b)==row['bytes'] and digest(b)==row['sha256'],'Changed closeout payload '+p)
   if p.startswith(W):need(not re.search(rb'(?i)[a-z]:[\\/]+users[\\/]+[^\\/\s"\x27]+',b),'Private home identifier in closeout')
 need(load(root/(W+'file-inventory-closeout-v2.json'))['paths']==paths,'Closeout inventory mismatch')
 need(all(r['path'] in paths for r in load(root/(W+'publication-manifest-v1.json'))['entries']),'Missing prior publication')
 proof=base(root)
 return dict(status='VALID_W2_CLOSEOUT_NOT_READY',hashed_payloads=len(rows)-1,historical_closed=47,historical_OPEN=657,base=proof)
if __name__=='__main__':
 import json
 print(json.dumps(inspect()))
