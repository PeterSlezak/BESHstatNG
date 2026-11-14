"""Independent live protection with explicit old/new source transition graph.

Old manifests are immutable. A former live source pin is satisfied only by an
exact retained preimage AND the recorded current successor, never by rewriting
the former manifest or substituting an arbitrary file.
"""
import hashlib,json,subprocess,re
from pathlib import Path
ROOT=Path(__file__).resolve().parents[5];HERE=Path(__file__).resolve().parent
W='evidence/04A38/completion/w2/';C=W+'continuation-v1/'
BASE='ba1636f5356ac761509c2cfa16fc66b92430935d'
ANCHOR='73711dba3aa1f12cd32ff0757b9948086391a5b405dd6087112ca04fbffb7d85'
def need(v,m):
 if not v:raise ValueError(m)
def load(p):return json.loads(p.read_bytes())
def sha(b):return hashlib.sha256(b).hexdigest()
def inspect(root=ROOT,live=True):
 root=Path(root);old=(root/(W+'publication-manifest-closeout-v2.json')).read_bytes();need(sha(old)==ANCHOR,'Old W2 manifest changed')
 transitions=load(root/(C+'source-transitions-p1-v5.json'))['files'];final={r['after']['path']:r for r in transitions};versions={}
 for r in load(root/(W+'source-transitions.json'))['files']:
  if r['path'] not in final:final[r['path']]=dict(after=r['after'],snapshot=r['after'])
 def version(path,record):versions.setdefault(path,{})[(record['bytes'],record['sha256'])]=record['path']
 for r in load(root/(W+'source-transitions.json'))['files']:
  if r['before']:version(r['path'],r['before'])
  if r['path'] in final:
   row=next(x for x in load(root/(C+'entry.json'))['source_before'] if x['live']['path']==r['path']) if any(x['live']['path']==r['path'] for x in load(root/(C+'entry.json'))['source_before']) else None
   if row:version(r['path'],dict(row['preimage'],bytes=r['after']['bytes'],sha256=r['after']['sha256']))
 for data,key in [('entry.json','source_before'),('source-before-p1.json','files'),('source-before-p1-reports.json','files')]:
  for r in load(root/(C+data))[key]:version(r['live']['path'],r['preimage'])
 for stage in ['p0-v2','p1-v4','p1-v5']:
  for r in load(root/(C+'source-transitions-'+stage+'.json'))['files']:version(r['after']['path'],r['snapshot'])
 checked={};resolved=[]
 def check(row,path=None,transition=True):
  p=path or row['path'];expected=(row['bytes'],row['sha256'])
  if p not in checked:
   b=(root/p).read_bytes();checked[p]=(len(b),sha(b))
  if checked[p]==expected:return
  need(transition and p in final,'Protected byte mismatch '+row['path'])
  need(expected in versions.get(p,{}),'No exact retained source version '+row['path'])
  snapshot=versions[p][expected];raw=(root/snapshot).read_bytes();need((len(raw),sha(raw))==expected,'Source preimage changed '+snapshot)
  current=final[p]['after'];need(checked[p]==(current['bytes'],current['sha256']),'Source successor not recorded '+p)
  resolved.append(dict(path=p,former_sha256=row['sha256'],retained=snapshot,current_sha256=current['sha256']))
 rows=json.loads(old)['entries'];need(len(rows)==2135,'Prior W2 payload cardinality changed')
 for row in rows:
  if row['sha256'] is not None:check(row)
 for row in final.values():check(row['after'],transition=False);check(row['snapshot'],transition=False)
 protection=load(root/'evidence/04A38/preservation-validation-v2.json')
 for row in [r['current'] for r in protection['lineage_pins']]:check(row)
 for row in protection['protected_mixed_model_captures']:check(row,transition=False)
 for row in protection['owner_documents_excluded']:check(row,transition=False)
 originals=load(root/'evidence/04A38/preedit/entry-source-snapshot.json')['files'];need(len(originals)==41,'Original snapshot count')
 for row in originals:check(row['snapshot'],transition=False)
 result=dict(status='PRESERVATION_AND_SOURCE_TRANSITION_GRAPH_PASS_NOT_READY',prior_W2_hashed_payloads=2134,prior_manifest_sha256=ANCHOR,old_manifest_bytes='PRESERVED',original_snapshots=41,lineage_pins=len(protection['lineage_pins']),protected_captures=len(protection['protected_mixed_model_captures']),owner_documents=len(protection['owner_documents_excluded']),source_transitions=resolved,original_cases_per_configuration=28966,authorized_separate_late_per_configuration=458,original_queries=0,Git='NOT_CHECKED_IN_COPY',TestResults='Only explicitly protected/tracked files; ignored tree not traversed')
 if live:
  def git(*args):return subprocess.check_output(['git','-c','safe.directory='+root.as_posix(),*args],cwd=root)
  tracked=[p for p in git('ls-files','-z').decode().split('\0') if p];hidden={'<PRIVATE_PROTECTED_PATH:'+sha(p.encode())[:24]+'>':p for p in tracked}
  baseline=load(root/'evidence/04A38/late-archived-entry-supplement/v1/preservation.json')['records'];need(len(tracked)==len(baseline)==4695,'Historical tracked path population changed')
  for row in baseline:check(row['current'],hidden.get(row['path'],row['path']))
  need(not git('diff','--cached','--name-only'),'Index not empty');need(git('branch','--show-current').decode().strip()=='codex/04A-38','Branch changed')
  need(all(git(*args).decode().strip()==BASE for args in [('rev-parse','HEAD'),('rev-parse','main'),('merge-base','HEAD','main')]),'Commit/main/merge-base changed')
  diff=subprocess.run(['git','-c','safe.directory='+root.as_posix(),'diff','--check'],cwd=root,capture_output=True);need(diff.returncode==0,'Diff whitespace error')
  source=[]
  for row in originals:
   raw=git('cat-file','blob',BASE+':'+row['source']['path']);snapshot=(root/row['snapshot']['path']).read_bytes()
   norm=lambda b:b.replace(b'\r\n',b'\n')
   need(norm(raw)==norm(snapshot),'Pinned Git/original snapshot identity mismatch '+row['source']['path'])
   source.append(dict(path=row['source']['path'],git_bytes=len(raw),git_sha256=sha(raw),snapshot=row['snapshot'],identity='RAW_BYTE_EXACT' if raw==snapshot else 'CRLF_LF_ONLY'))
  result.update(Git='LIVE_PASS',branch='codex/04A-38',HEAD=BASE,main=BASE,merge_base=BASE,index='EMPTY',diff_check_exit=0,tracked=4695,source_authentication=source,authoritative_tracked_protection='Entry hashes plus explicit accepted and W2 successor/preimage graph',status_paths_sha256=sha(git('status','--porcelain=v1')))
 return result
if __name__=='__main__':
 try:print(json.dumps(inspect()))
 except (ValueError,OSError,KeyError,TypeError,subprocess.CalledProcessError) as e:print(json.dumps(dict(status='NOT_READY_04A38',error=str(e))));raise SystemExit(1)
