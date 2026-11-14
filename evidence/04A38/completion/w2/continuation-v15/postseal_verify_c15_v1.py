"""Actual final read-only gates; raw postseal streams use a new explicit root.

No test suite, original/reference code, restore, build, native or production gate.
"""
from common import *
post=RUNROOT/'postseal-w2c15-v1';post.mkdir(exist_ok=False)
bootstrap='import sys,runpy;sys.path.insert(0,sys.argv[1]);sys.argv=[sys.argv[2]]+sys.argv[3:];runpy.run_path(sys.argv[0],run_name="__main__")'
records=[]

def execute(label,args):
 folder=post/label;folder.mkdir();start=time.monotonic();result=subprocess.run(args,cwd=ROOT,capture_output=True)
 for name in ['stdout','stderr']:
  with (folder/(name+'.log')).open('xb') as f:f.write(getattr(result,name))
 raw=dict(command=args,cwd=str(ROOT),exit_code=result.returncode,seconds=time.monotonic()-start)
 with (folder/'command.json').open('x',encoding='utf-8') as f:json.dump(raw,f,indent=2)
 row=dict(label=label,command=pin(folder/'command.json'),stdout=pin(folder/'stdout.log'),stderr=pin(folder/'stderr.log'),result=clean(raw));records.append(row)
 assert result.returncode==0,'Actual final gate failed; preserve and STOP: '+label
 return result

def python_gate(label,path,*args):return execute(label,[sys.executable,'-I','-B','-c',bootstrap,str(HERE),str(path),*map(str,args)])

private=python_gate('retained-private-reader',W2/'continuation-v15-retained-private-reader-v2.py','--verify')
physical=python_gate('physical-publication-reader',HERE/'verify_c15_publication_v2.py','--index',W2/'continuation-v15-review-parts-index-v1.json','--parts','--physical-only')
protected=python_gate('live-preservation-reader',HERE/'verify_c15_preservation_v3.py')
baseargs=['git','-c','safe.directory='+ROOT.as_posix(),'--no-optional-locks']
branch=execute('git-branch',baseargs+['branch','--show-current']).stdout.decode().strip();assert branch=='codex/04A-38'
anchors=execute('git-head-main',baseargs+['rev-parse','HEAD','main']).stdout.decode().splitlines();assert anchors==[BASE,BASE]
merge=execute('git-merge-base',baseargs+['merge-base','HEAD','main']).stdout.decode().strip();assert merge==BASE
assert not execute('git-staged-index',baseargs+['diff','--cached','--name-only']).stdout
execute('git-diff-check',baseargs+['diff','--check']);status=execute('git-status-short',baseargs+['status','--short'])
index=pin(ROOT/'.git/index');expected=next(r for r in json.loads((HERE/'entry-authentication-v2.json').read_bytes())['authorities'] if r['path']=='.git/index');assert index==expected

def external(name,value):
 path=W2/name
 with path.open('x',encoding='utf-8',newline='\n') as f:json.dump(value,f,indent=2,ensure_ascii=False);f.write('\n')
 return pin(path)

git_receipt=external('continuation-v15-final-git-preservation-v1.json',dict(status='EXACT_GIT_ANCHORS_INDEX_AND_DIFF_CHECK_PASS_NOT_READY',branch=branch,HEAD=BASE,main=BASE,merge_base=merge,index=index,staged='EMPTY',diff_check_exit=0,status_sha256=sha(status.stdout),raw_commands=[r for r in records if r['label'].startswith('git-')],original_queries=0))
verification=external('continuation-v15-final-verification-v1.json',dict(status='ACTUAL_FINAL_PRIVATE_PHYSICAL_AND_LIVE_READERS_PASS_NOT_READY',private=clean(json.loads(private.stdout)),physical=clean(json.loads(physical.stdout)),preservation=clean(json.loads(protected.stdout)),commands=records,git=git_receipt,original_queries=0))
pins=[pin(p) for p in sorted(post.rglob('*')) if p.is_file()]
external('continuation-v15-postseal-private-addendum-v1.json',dict(status='ADDITIVE_EXPLICIT_POSTSEAL_RAW_PINS_NO_PRIOR_REWRITE',base=pin(W2/'continuation-v15-retained-private-preservation-v1.json'),postseal_private_pins=pins,count=len(pins),verification=verification,scope='New explicit postseal raw-command root only; every raw stream/pin remains local. All earlier15166 inherited and bounded current private pins remain bound by the unchanged base private seal. No ignored historical TestResults traversal.',original_queries=0))
print('Actual final postseal gates PASS',verification,flush=True)
