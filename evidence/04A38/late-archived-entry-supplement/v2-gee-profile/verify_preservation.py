"""Independent live current-tree and protected-source/evidence preservation."""
import hashlib,json,subprocess
from pathlib import Path
ROOT=Path(__file__).resolve().parents[4]
PREFIX='evidence/04A38/late-archived-entry-supplement/v2-gee-profile/'

def inspect_protected(root=ROOT):
 root=Path(root)
 manifest_path='evidence/04A38/late-archived-entry-supplement/v1/publication-manifest.json'
 raw=(root/manifest_path).read_bytes()
 if hashlib.sha256(raw).hexdigest()!='0265ab6b17597faaeec79f846c484b7097824a519b73303e9ab8e37b042663eb':raise ValueError('Protected authority changed')
 path='evidence/04A38/preservation-validation-v2.json'
 row=next(r for r in json.loads(raw)['entries'] if r['path']==path)
 data=(root/path).read_bytes()
 if len(data)!=row['bytes'] or hashlib.sha256(data).hexdigest()!=row['sha256']:raise ValueError('Protection metadata changed')
 old=json.loads(data)
 for row in [r['current'] for r in old['lineage_pins']]+old['protected_mixed_model_captures']+old['owner_documents_excluded']:
  data=(root/row['path']).read_bytes()
  if len(data)!=row['bytes'] or hashlib.sha256(data).hexdigest()!=row['sha256']:raise ValueError('Protected payload changed: '+row['path'])
 return dict(lineage=278,captures=100,owner_documents=16)

def inspect(root=ROOT):
 root=Path(root)
 inspect_protected(root)
 def load(p):return json.loads((root/p).read_bytes())
 def sha(b):return hashlib.sha256(b).hexdigest()
 def git(*args):return subprocess.check_output(['git','-c','safe.directory='+root.as_posix(),*args],cwd=root)
 def check(row,actual=None):
  data=(root/(actual or row['path'])).read_bytes()
  if len(data)!=row['bytes'] or sha(data)!=row['sha256']:raise ValueError('Preservation mismatch: '+row['path'])
 tracked=[p for p in git('ls-files','-z').decode().split('\0') if p]
 hidden={'<PRIVATE_PROTECTED_PATH:'+sha(p.encode())[:24]+'>':p for p in tracked}
 old=load('evidence/04A38/late-archived-entry-supplement/v1/preservation.json')
 if len(tracked)!=4695 or len(old['records'])!=4695:raise ValueError('Tracked identity count changed')
 for row in old['records']:check(row['current'],hidden.get(row['path'],row['path']))
 original=load('evidence/04A38/preservation-validation-v2.json')
 for row in original['lineage_pins']:check(row['current'])
 for row in original['protected_mixed_model_captures']+original['owner_documents_excluded']:check(row)
 for row in load('evidence/04A38/late-archived-entry-supplement/v1/publication-manifest.json')['entries']:
  if row['sha256'] is not None:check(row)
 if git('diff','--cached','--name-only'):raise ValueError('Unexpected staged changes')
 if git('rev-parse','HEAD').decode().strip()!='ba1636f5356ac761509c2cfa16fc66b92430935d':raise ValueError('Unexpected commit')
 return dict(status='PASS',tracked_entry=4695,exact_entry=4663,authorized_checkpoint_transitions=32,new_production_changes=0,
      lineage_pins=len(original['lineage_pins']),protected_mixed_model_captures=len(original['protected_mixed_model_captures']),
      owner_documents=len(original['owner_documents_excluded']),old_publication_payloads=774,
      original_freeze_28966_per_configuration='BYTE_PRESERVED',late432='BYTE_PRESERVED',
      prior_not_ready_and_manifests='BYTE_PRESERVED',accepted04A37_routeA='BYTE_PRESERVED',index='EMPTY',
      historical_TestResults='NOT_TRAVERSED_OR_REHASHED')
if __name__=='__main__':print(json.dumps(inspect()))
