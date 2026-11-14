"""Exact raw exit Git commands before the final private seal; no Git mutation."""
from common import *
checks=[]
for name,args in [('branch',['branch','--show-current']),('HEAD',['rev-parse','HEAD']),('main',['rev-parse','main']),
 ('merge-base',['merge-base','HEAD','main']),('index',['diff','--cached','--name-status']),
 ('status',['status','--porcelain=v1']),('diff-check',['diff','--check'])]:
    result=command('exit-git-'+name+'-v1',['git','-c','safe.directory='+ROOT.as_posix(),'--no-optional-locks',*args])
    folder=RUNROOT/'commands'/('w2c11-exit-git-'+name+'-v1')
    stdout=(folder/'stdout.log').read_bytes();stderr=(folder/'stderr.log').read_bytes()
    assert result['exit_code']==0
    if name=='branch':assert stdout.decode().strip()=='codex/04A-38'
    if name in ['HEAD','main','merge-base']:assert stdout.decode().strip()==BASE
    if name=='index':assert not stdout
    checks.append(dict(check=name,command=clean(result),stdout=stdout.decode(),stderr=clean(stderr.decode()),
        raw_command=pin(folder/'command.json'),raw_stdout=pin(folder/'stdout.log'),raw_stderr=pin(folder/'stderr.log')))
assert pin(ROOT/'.git/index')['sha256']==json.loads((HERE/'git-entry-v1.json').read_bytes())['index_file']['sha256']
path=PRIVATE/'exit-git-receipt-v1.json'
with path.open('x',encoding='utf-8',newline='\n') as stream:json.dump(dict(checks=checks,index_file=pin(ROOT/'.git/index'),source_validation_freeze=pin(HERE/'production-test-freeze-v1.json')),stream,ensure_ascii=False,indent=2);stream.write('\n')
print('Exact exit Git/index commands PASS, raw receipt retained privately',flush=True)
