"""Independent stdlib publication reader. Cannot approve whole-batch READY."""
import re
from pathlib import Path,PurePosixPath
from verify_inputs import ROOT,PREFIX,BASE,need,load,digest,check_pin
from verify_supplement import PREVIOUS,inspect as supplement
SELF=PREFIX+'publication-manifest.json'

def inspect(root=ROOT,manifest_name=SELF,inventory_name=PREFIX+'file-inventory.json'):
 root=Path(root);manifest=load(root/manifest_name);rows=manifest['entries'];paths=[r['path'] for r in rows]
 need(manifest['status']=='NOT_READY_04A38','This bounded reader cannot grant batch READY')
 need(paths==sorted(paths) and len(set(p.casefold() for p in paths))==len(paths),'Duplicate/casefold/reordered manifest')
 need(paths.count(manifest_name)==1,'Missing/duplicate manifest self entry')
 for row in rows:
  p=row['path'];pure=PurePosixPath(p)
  need(not pure.is_absolute() and pure.as_posix()==p and '\\' not in p and ':' not in p,'Unsafe/absolute/noncanonical manifest path')
  need(all(v not in {'','.','..'} and not v.endswith((' ','.')) and not re.search(r'[\x00-\x1f<>"|?*]',v) and not re.match(r'(?i)^(CON|PRN|AUX|NUL|COM[1-9]|LPT[1-9])(?:\.|$)',v) for v in p.split('/')),'Escaping/reserved manifest path')
  need((root/p).resolve().is_relative_to(root.resolve()),'Symlink path escape')
  need(not p.startswith(('.git/','.test-rationalization-','packages/','TestResults/')),'Private raw path in publication')
  need(p!='evidence/04A38/late-archived-entry-supplement/v2-gee-profile/entry-inspection.json','Private predecessor raw diagnostic republished')
  if p==manifest_name:need(row['bytes'] is None and row['sha256'] is None,'Recursive/nonnull self entry')
  else:
   need(type(row['bytes']) is int and row['bytes']>=0 and bool(re.fullmatch('[0-9a-f]{64}',row['sha256'] or '')),'Malformed size/digest')
   check_pin(root,row)
   if p.startswith(PREFIX):
    data=(root/p).read_bytes()
    need(not re.search(rb'(?i)[a-z]:[\\/]+users[\\/]+[^\\/\s"\x27]+',data),'Private owner-machine path in public evidence')
    need(not re.search(rb'-----BEGIN (?:RSA |EC |OPENSSH )?PRIVATE KEY-----',data),'Private key in public evidence')
 need(load(root/inventory_name)['paths']==paths,'Missing payload/inventory mismatch')
 for row in load(root/PREVIOUS)['entries']:
  need(row['path'] in paths,'Lost predecessor payload')
  if row['sha256'] is not None:need(next(r for r in rows if r['path']==row['path'])==row,'Changed old manifest assertion')
 git=load(root/(PREFIX+'git-gate.json'))
 need(git['branch']=='codex/04A-38' and git['head']==git['main']==git['merge_base']==BASE and git['index']=='EMPTY' and git['diff_check_exit']==0,'Staging/commit/branch/whitespace changed')
 ready=load(root/(PREFIX+'readiness.json'))
 need(ready['status']=='NOT_READY_04A38' and ready['new_production_changes']==0 and ready['gates']=={'P':'NO','N1':'NO GO','N2':'NO','C2':'UNMET','C3':'UNAPPROVED'},'False readiness/native/scientific promotion')
 return dict(status='VALID_NOT_READY_MMRM_PUBLICATION',external_files=len(rows)-1)
if __name__=='__main__':
 import json
 supplement();print(json.dumps(inspect()))
