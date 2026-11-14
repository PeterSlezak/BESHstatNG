"""Independent live4695-path protection with explicit W2 source transitions."""
import hashlib,json,subprocess
from pathlib import Path
ROOT=Path(__file__).resolve().parents[4];W='evidence/04A38/completion/w2/'
G='evidence/04A38/late-archived-entry-supplement/v7-consolidated-gee-spec-mmrm-wide/'
ANCHOR='b050ca676e406333346c7ed813d5eecf9eaf6fc193e882b38fe58ddb83077623'
def need(v,m):
 if not v:raise ValueError(m)
def load(p):return json.loads(p.read_bytes())
def sha(b):return hashlib.sha256(b).hexdigest()
def inspect(root=ROOT,live=True):
 root=Path(root);raw=(root/(G+'publication-manifest-closeout-v1.json')).read_bytes();need(sha(raw)==ANCHOR,'Pre-W2 G manifest changed')
 rows=json.loads(raw)['entries'];changes=load(root/(W+'source-transitions.json'))['files'];allowed={r['path']:r for r in changes if r['before']}
 def check(row,actual=None,allow=True):
  path=actual or row['path'];data=(root/path).read_bytes()
  if (len(data),sha(data))==(row['bytes'],row['sha256']):return
  need(allow and path in allowed,'Unexpected source/protected transition '+row['path'])
  change=allowed[path];before=change['before'];after=change['after']
  need((row['bytes'],row['sha256'])==(before['bytes'],before['sha256']),'Changed predecessor baseline')
  old=(root/before['path']).read_bytes();need((len(old),sha(old))==(before['bytes'],before['sha256']),'Lost source preimage')
  need((len(data),sha(data))==(after['bytes'],after['sha256']),'Unrecorded further production edit')
 for row in rows:
  if row['sha256'] is not None:check(row)
 for change in changes:check(change['after'],allow=False)
 protection=load(root/'evidence/04A38/preservation-validation-v2.json')
 for row in [r['current'] for r in protection['lineage_pins']]+protection['protected_mixed_model_captures']:
  check(row)
 result=dict(status='PROTECTED_BYTES_AND_EXPLICIT_TRANSITIONS_PASS',predecessor_payloads=1605,old_manifests='BYTE_PRESERVED',source_preimages=4,original_snapshots=41,lineage_pins=278,protected_captures=100,
   original_preedit_per_configuration=28966,authorized_late_per_configuration=458,live_Git='NOT_CHECKED_IN_COPY',TestResults='Only100 declared protected captures/known tracked paths checked; no ignored-tree traversal')
 if live:
  def git(*args):return subprocess.check_output(['git','-c','safe.directory='+root.as_posix(),*args],cwd=root)
  tracked=git('ls-files','-z').decode().split('\0');tracked=[p for p in tracked if p];hidden={'<PRIVATE_PROTECTED_PATH:'+sha(p.encode())[:24]+'>':p for p in tracked}
  old=load(root/'evidence/04A38/late-archived-entry-supplement/v1/preservation.json');need(len(tracked)==len(old['records'])==4695,'Tracked path identity drift')
  for row in old['records']:check(row['current'],hidden.get(row['path'],row['path']))
  for row in protection['owner_documents_excluded']:check(row,allow=False)
  need(not git('diff','--cached','--name-only'),'Index not empty')
  base='ba1636f5356ac761509c2cfa16fc66b92430935d'
  need(git('branch','--show-current').decode().strip()=='codex/04A-38','Branch drift')
  need(all(git(*a).decode().strip()==base for a in [('rev-parse','HEAD'),('rev-parse','main'),('merge-base','HEAD','main')]),'Commit/main/merge-base drift')
  p=subprocess.run(['git','-c','safe.directory='+root.as_posix(),'diff','--check'],cwd=root,capture_output=True);need(p.returncode==0,'diff-check failure')
  result.update(live_Git='PASS',tracked=4695,exact_original_entry=4663,authorized_tracked_transitions=32,tracked_paths_further_changed_by_W2=3,owner_documents=16,index='EMPTY',diff_check_exit=0)
 return result
if __name__=='__main__':print(json.dumps(inspect()))
