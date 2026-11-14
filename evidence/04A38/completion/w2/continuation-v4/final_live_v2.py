"""Post-manifest live state and previous privacy-policy check; no repo mutation."""
import zipfile,re
from common import *
from verify_preservation import inspect
folder=RUNROOT/'exports';stem='BESHstatNG_04A38_W2_continuation_v4_20261110_';manifest=HERE/'publication-manifest-v3.json';zip_path=folder/(stem+'review_v2.zip');prefix=HERE.relative_to(ROOT).as_posix()+'/'
# Preserve the former current-prefix environment-identifier privacy check too.
tokens=[os.environ.get(k) for k in ['USERNAME','COMPUTERNAME'] if os.environ.get(k)];violations=[];checked=0
with zipfile.ZipFile(zip_path) as z:
 for i in z.infolist():
  if not i.filename.startswith(prefix):continue
  checked+=1;b=z.read(i.filename)
  for token in tokens:
   if re.search(rb'(?i)(?<![A-Za-z0-9])'+re.escape(token.encode())+rb'(?![A-Za-z0-9])',b):violations.append(dict(path=i.filename,identifier_sha256=sha(token.encode())))
assert not violations,'Current-prefix privacy policy failure'
privacy=dict(status='PRIOR_CURRENT_PREFIX_ENVIRONMENT_IDENTIFIER_POLICY_PASS',checked=checked,identifier_hashes=[sha(t.encode()) for t in tokens],violations=violations,scope='Actual entire current-prefix ZIP membership; supplementary to all-entry all encoded privacy patterns checks')
p=inspect(ROOT,True);old=json.loads((HERE/'entry.json').read_bytes());paths={json.loads(s[3:]) if s[3:].startswith('"') else s[3:] for s in old['git_status'].splitlines() if s};now=git('status','--porcelain=v1','-uall','-z').decode().split('\0');unexpected=[s[3:] for s in now if s and s[3:] not in paths and not s[3:].startswith(prefix)];assert not unexpected and all((ROOT/s).exists() for s in paths)
assert not git('diff','--cached','--name-only');check=subprocess.run(['git','-c','safe.directory='+ROOT.as_posix(),'diff','--check'],cwd=ROOT,capture_output=True);assert check.returncode==0
state=dict(status='FINAL_LIVE_GIT_PRESERVATION_PASS_NOT_READY',preservation=p,branch=git('branch','--show-current').decode().strip(),HEAD=git('rev-parse','HEAD').decode().strip(),main=git('rev-parse','main').decode().strip(),merge_base=git('merge-base','HEAD','main').decode().strip(),index='EMPTY',diff_check_exit=0,git_status=clean(git('status','--short','-uall').decode()),preexisting_paths_retained=len(paths),unexpected_new_paths=unexpected,source_transitions=pin(HERE/'source-transitions.json'),manifest=pin(manifest),publication_actions='NONE',Phase04A39='NOT_STARTED')
assert state['branch']=='codex/04A-38' and state['HEAD']==state['main']==state['merge_base']==BASE
receipts={}
for suffix,obj in [('prior-policy-privacy',privacy),('live-closeout',state)]:
 path=folder/(stem+suffix+'.json')
 with path.open('x',encoding='utf-8') as f:json.dump(clean(obj),f,indent=2)
 receipts[suffix]=dict(name=path.name,bytes=path.stat().st_size,sha256=sha(path.read_bytes()))
export=json.loads((folder/(stem+'export-receipt.json')).read_bytes());cmds=[dict(label=r.parent.name,command=clean(json.loads(r.read_bytes()))) for r in sorted((RUNROOT/'commands').glob('w2c4-*/command.json'))]
result=dict(status='NOT_READY_04A38',W2='IN_PROGRESS',W3='NOT_STARTED',manifest=pin(manifest),export={k:export[k] for k in ['filename','bytes','sha256','entries','CRC']},external_receipts=receipts,privacy=privacy,live=state,completed_runner_commands=cmds,scope='Post-manifest live/export receipt outside ZIP to avoid recursive ZIP hashing',gates='P=NO;N1=NO GO;N2=NO;C2=UNMET;C3=UNAPPROVED;NOT_GOLDEN')
with (folder/(stem+'FINAL_RECEIPT.json')).open('x',encoding='utf-8') as f:json.dump(result,f,indent=2)
print(json.dumps(dict(status='NOT_READY_04A38',export=result['export'],index=state['index'],diff_check=state['diff_check_exit'],manifest=result['manifest'])))
