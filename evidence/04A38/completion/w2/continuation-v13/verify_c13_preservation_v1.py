"""Independent live protection with explicit old/new source transition graph.

Old manifests are immutable. A former live source pin is satisfied only by an
exact retained preimage AND the recorded current successor, never by rewriting
the former manifest or substituting an arbitrary file.
"""
import hashlib,json,subprocess,re
from pathlib import Path
ROOT=Path(__file__).resolve().parents[5];HERE=Path(__file__).resolve().parent
W='evidence/04A38/completion/w2/';C=W+'continuation-v1/';C2=W+'continuation-v2/';C3=W+'continuation-v3/';C4=W+'continuation-v4/';C5=W+'continuation-v5/';C6=W+'continuation-v6/';C7=W+'continuation-v7/';C8=W+'continuation-v8/';C9=W+'continuation-v9/';C10=W+'continuation-v10/';C11=W+'continuation-v11/';C12=W+'continuation-v12/';C13=W+'continuation-v13/'
BASE='ba1636f5356ac761509c2cfa16fc66b92430935d'
ANCHOR='73711dba3aa1f12cd32ff0757b9948086391a5b405dd6087112ca04fbffb7d85'
def need(v,m):
 if not v:raise ValueError(m)
def load(p):return json.loads(p.read_bytes())
def sha(b):return hashlib.sha256(b).hexdigest()
def inspect(root=ROOT,live=True):
 root=Path(root);old=(root/(W+'publication-manifest-closeout-v2.json')).read_bytes();need(sha(old)==ANCHOR,'Old W2 manifest changed')
 c6trans=load(root/(C6+'source-transitions-final-v1.json'))['files'];c7trans=load(root/(C7+'source-transitions-final-v1.json'))['files'];c8trans=load(root/(C8+'source-transitions-final-v1.json'))['files'];c9trans=load(root/(C9+'source-transitions-final-v1.json'))['files'];c10trans=load(root/(C10+'source-transitions-final-v1.json'))['files'];c11trans=load(root/(C11+'source-transitions-final-v1.json'))['files'];c12trans=load(root/(C12+'source-transitions-final-v1.json'))['files'];transitions=load(root/(C13+'source-transitions-final-v1.json'))['files'];c5trans=load(root/(C5+'source-transitions-v4.json'))['files'];c4trans=load(root/(C4+'source-transitions.json'))['files'];c3trans=load(root/(C3+'source-transitions-final.json'))['files'];c2trans=load(root/(C2+'source-transitions-final.json'))['files'];final={r['after']['path']:r for r in transitions};versions={}
 for r in c12trans+c11trans+c10trans+c9trans+c8trans+c7trans+c6trans+c5trans+c4trans+c3trans+c2trans:
  if r['after']['path'] not in final:final[r['after']['path']]=r
 for r in load(root/(C+'source-transitions-p1-v5.json'))['files']:
  if r['after']['path'] not in final:final[r['after']['path']]=r
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
 # Every former live C1 source version must have an exact retained C2 preimage.
 for r in c12trans+c11trans+c10trans+c9trans+c8trans+c7trans+c6trans+c5trans+c4trans+c3trans+c2trans+transitions:
  if r['before']:version(r['path'],dict(r['preimage'],bytes=r['before']['bytes'],sha256=r['before']['sha256']))
  version(r['path'],r['snapshot'])
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
 predecessor=(root/(C+'publication-manifest-v4.json')).read_bytes();need(sha(predecessor)=='726f684cae59a2ace90bfeccd109c2443d1f43a610a51330f34cb891497301d8','C1 manifest changed')
 c1rows=json.loads(predecessor)['entries'];need(len(c1rows)==2645,'C1 count changed')
 for row in c1rows:
  if row['sha256'] is not None:check(row)
 
 c2manifest=(root/(C2+'publication-manifest-v3.json')).read_bytes();need(sha(c2manifest)=='6428ab41d51fabf062db6a451a51a350428d7cd392f0c70147054ff635f4be86','C2 final manifest changed')
 c2rows=json.loads(c2manifest)['entries'];need(len(c2rows)==3125,'C2 payload count changed')
 for row in c2rows:
  if row['sha256'] is not None:check(row)
 c3manifest=(root/(C3+'publication-manifest-v2.json')).read_bytes();need(sha(c3manifest)=='6c1be0d2bc3e6fbe00968ed1385ad599cab28ad49306756de6085358f98a45dc','C3 final manifest changed')
 c3rows=json.loads(c3manifest)['entries'];need(len(c3rows)==3425,'C3 payload cardinality changed')
 for row in c3rows:
  if row['sha256'] is not None:check(row)
 c4manifest=(root/(C4+'publication-manifest-v3.json')).read_bytes();need(sha(c4manifest)=='2b947493165401dfffc405846ac0ff68e902b5dc567486e9db38c0290471b0a3','C4 final manifest changed')
 c4rows=json.loads(c4manifest)['entries'];need(len(c4rows)==3778,'C4 payload cardinality changed')
 for row in c4rows:
  if row['sha256'] is not None:check(row)
 c5manifest=(root/(C5+'publication-manifest-v1.json')).read_bytes();need(sha(c5manifest)=='b8d6537e5abfc5a7585ed43d7f4c5df0bd37ae79b3550fbf22443d095f5b0cf4','C5 final manifest changed')
 c5rows=json.loads(c5manifest)['entries'];need(len(c5rows)==4493,'C5 payload cardinality changed')
 for row in c5rows:
  if row['sha256'] is not None:check(row)
 c6manifest=(root/(C6+'publication-manifest-v1.json')).read_bytes();need(sha(c6manifest)=='9b5f1ef88e5b4c2e951baaa92d74065d8c127971bc7ca54994b243d4c69b1478','C6 final manifest changed')
 c6rows=json.loads(c6manifest)['entries'];need(len(c6rows)==5640,'C6 payload cardinality changed')
 for row in c6rows:
  if row['sha256'] is not None:check(row)
 c7manifest=(root/(C7+'publication-manifest-v1.json')).read_bytes();need(sha(c7manifest)=='b0c0668625181881e532abf882bcf0bb4653f8695d519f9ebf2970c86aae8109','C7 final manifest changed')
 c7rows=json.loads(c7manifest)['entries'];need(len(c7rows)==5915,'C7 payload cardinality changed')
 for row in c7rows:
  if row['sha256'] is not None:check(row)
 c8manifest=(root/(C8+'publication-manifest-v2.json')).read_bytes();need(sha(c8manifest)=='9e92673350e948868d74e80585f730aeea74607f893a4647c2d4c6ea8eb7bd13','C8 final manifest changed')
 c8rows=json.loads(c8manifest)['entries'];need(len(c8rows)==6306,'C8 publication population changed')
 for row in c8rows:
  if row['sha256'] is not None:check(row)
 c9manifest=(root/(C9+'publication-manifest-v1.json')).read_bytes();need(sha(c9manifest)=='d9dc9c20e18f501e75efc2bfe3894170f084c167efad46535963a38cfc964992','C9 final manifest changed')
 c9rows=json.loads(c9manifest)['entries'];need(len(c9rows)==6698,'C9 publication population changed')
 for row in c9rows:
  if row['sha256'] is not None:check(row)
 c10manifest=(root/(C10+'publication-manifest-v1.json')).read_bytes();need(sha(c10manifest)=='b84ad070d4f5e70063167ee61712f86f007c032ebcae4e6635291f02002420b2','C10 final manifest changed')
 c10rows=json.loads(c10manifest)['entries'];need(len(c10rows)==7075,'C10 publication population changed')
 for row in c10rows:
  if row['sha256'] is not None:check(row)
 data=(root/'evidence/04A38/completion/w2/continuation-v11/publication-manifest-v1.json').read_bytes();need(sha(data)=='5a8399cad912a422fc54bc9f122f6d58c45e762d9d407790d8246bdc7a793ca2','C11 immutable manifest changed')
 ledger=json.loads(data)['entries'];need(len(ledger)==7563,'C11 logical population changed')
 for row in ledger:
  if row['sha256'] is not None:check(row)
 data=(root/'evidence/04A38/completion/w2/continuation-v11-logical-publication-manifest-v2.json').read_bytes();need(sha(data)=='98ed2f43fbca43943164284d673777fa525ae34aa903382885eae93d5aa3a043','C11 immutable manifest changed')
 ledger=json.loads(data)['entries'];need(len(ledger)==7577,'C11 logical population changed')
 for row in ledger:
  if row['sha256'] is not None:check(row)
 data=(root/'evidence/04A38/completion/w2/continuation-v11-logical-publication-manifest-v3.json').read_bytes();need(sha(data)=='c47616719fa8eaa1908f542747e0d330161dc9b49d2afa6266f04ca17b16cbc9','C11 immutable manifest changed')
 ledger=json.loads(data)['entries'];need(len(ledger)==7584,'C11 logical population changed')
 for row in ledger:
  if row['sha256'] is not None:check(row)
 data=(root/'evidence/04A38/completion/w2/continuation-v12/publication-manifest-v1.json').read_bytes();need(sha(data)=='3b3832a087e6a7d6eb0a24e3302f9f1dbd773c10da0e593589dab31eb7cb9949','C12 immutable complete manifest changed')
 ledger=json.loads(data)['entries'];need(len(ledger)==8015,'C12 logical population changed')
 for row in ledger:
  if row['sha256'] is not None:check(row)
 private_manifest=load(root/(W+'continuation-v12-retained-private-preservation-v1.json'))
 private_rows={r['path']:r for r in private_manifest['private_pins']}
 for row in load(root/(C13+'entry-authority-pins-v1.json'))['authorities']:
  if row['path'].startswith('.test-rationalization-04A38.tmp/') and not live:
   need(row==private_rows.get(row['path']),'Unattested omitted raw private authority')
  else:check(row,transition=False)
 for row in load(root/(C12+'entry-authority-pins-v1.json'))['authorities']:check(row,transition=False)
 for row in load(root/(C11+'entry-authority-pins-v1.json'))['authorities']:check(row,transition=False)
 for row in load(root/(C10+'entry-v1.json'))['anchors']+load(root/(C10+'entry-v1.json'))['external_receipts']:check(row,transition=False)
 for row in load(root/(C9+'entry-v1.json'))['external_receipts']:check(row,transition=False)
 if live:
  retained=load(root/(C9+'v8-private-entry-v1.json'))
  for row in retained['pins']+retained['prior_archives']:check(row,transition=False)
  oldzip=load(root/(C9+'v8-publication-entry-v1.json'))['archive'];check(oldzip,transition=False);check(oldzip['deterministic_reproduction'],transition=False)
 protection=load(root/'evidence/04A38/preservation-validation-v2.json')
 for row in [r['current'] for r in protection['lineage_pins']]:check(row)
 for row in protection['protected_mixed_model_captures']:check(row,transition=False)
 owner_checked=0
 for row in protection['owner_documents_excluded']:
  if live or (root/row['path']).is_file():check(row,transition=False);owner_checked+=1
 originals=load(root/'evidence/04A38/preedit/entry-source-snapshot.json')['files'];need(len(originals)==41,'Original snapshot count')
 for row in originals:check(row['snapshot'],transition=False)
 result=dict(status='PRESERVATION_AND_SOURCE_TRANSITION_GRAPH_PASS_NOT_READY',prior_W2_hashed_payloads=2134,prior_C1_hashed_payloads=2644,prior_C2_hashed_payloads=3124,prior_C3_hashed_payloads=3424,prior_C4_hashed_payloads=3777,prior_C5_hashed_payloads=4492,prior_C6_hashed_payloads=5639,prior_C7_hashed_payloads=5914,prior_C8_hashed_payloads=6305,prior_C9_hashed_payloads=6697,prior_C10_hashed_payloads=7074,prior_C11_complete_logical_payloads=7584,prior_C12_complete_logical_payloads=8015,continuation_source_transitions=len(transitions),prior_manifest_sha256=ANCHOR,old_manifest_bytes='PRESERVED',original_snapshots=41,lineage_pins=len(protection['lineage_pins']),protected_captures=len(protection['protected_mixed_model_captures']),owner_documents_in_ledger=len(protection['owner_documents_excluded']),owner_documents_checked=owner_checked,owner_copy_scope='LIVE_ALL16' if live else 'ONLY_INCLUDED_PAYLOADS;OMITTED_OWNER_DOCUMENTS_REQUIRE_LIVE_RECEIPT',source_transitions=resolved,original_cases_per_configuration=28966,authorized_separate_late_per_configuration=458,original_queries=0,Git='NOT_CHECKED_IN_COPY',TestResults='Only explicitly protected/tracked files; ignored tree not traversed')
 if live:
  def git(*args):return subprocess.check_output(['git','-c','safe.directory='+root.as_posix(),'--no-optional-locks',*args],cwd=root)
  tracked=[p for p in git('ls-files','-z').decode().split('\0') if p];hidden={'<PRIVATE_PROTECTED_PATH:'+sha(p.encode())[:24]+'>':p for p in tracked}
  baseline=load(root/'evidence/04A38/late-archived-entry-supplement/v1/preservation.json')['records'];need(len(tracked)==len(baseline)==4695,'Historical tracked path population changed')
  for row in baseline:check(row['current'],hidden.get(row['path'],row['path']))
  need(not git('diff','--cached','--name-only'),'Index not empty');need(git('branch','--show-current').decode().strip()=='codex/04A-38','Branch changed')
  need(all(git(*args).decode().strip()==BASE for args in [('rev-parse','HEAD'),('rev-parse','main'),('merge-base','HEAD','main')]),'Commit/main/merge-base changed')
  diff=subprocess.run(['git','-c','safe.directory='+root.as_posix(),'--no-optional-locks','diff','--check'],cwd=root,capture_output=True);need(diff.returncode==0,'Diff whitespace error')
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
