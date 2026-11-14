"""Live protection and exact approved increment before immutable manifest sealing."""
from common import *
os.environ['GIT_OPTIONAL_LOCKS']='0'
r=command('prepublication-live-preservation-v1',[sys.executable,'-B',str(HERE/'verify_c8_preservation_v1.py')],env=dict(GIT_OPTIONAL_LOCKS='0'))
save('prepublication-live-preservation-command-v1.json',clean(r));assert r['exit_code']==0
result=json.loads((RUNROOT/'commands/w2c8-prepublication-live-preservation-v1/stdout.log').read_bytes())
save('prepublication-live-preservation-v1.json',result)
entry=json.loads((HERE/'entry-v2.json').read_bytes())
save('prepublication-scope-v1.json',dict(status='LIVE_PROTECTION_PASS_NO_STAGE',source_transitions=pin(HERE/'source-transitions-final-v1.json'),source_validation=pin(HERE/'final-source-validation-binding-v1.json'),Git=dict(branch=git('branch','--show-current').decode().strip(),HEAD=git('rev-parse','HEAD').decode().strip(),main=git('rev-parse','main').decode().strip(),merge_base=git('merge-base','HEAD','main').decode().strip(),index=git('diff','--cached','--name-only').decode()),status=git('status','--short').decode(),entry=pin(HERE/'entry-v2.json'),limitations='All accepted pre-existing dirty paths protected by live entry-to-current transition graph. Exact new implementation increment is six source/test paths plus C8 append-only evidence. No ignored historical tree traversal.'))
print('Live protection, source increment and Git prepublication checks passed')
