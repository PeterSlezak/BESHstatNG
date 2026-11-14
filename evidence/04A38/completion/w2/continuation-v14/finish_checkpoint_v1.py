"""Final reader/negative controls, one inventory, physical union, live and Git."""
from common import *
def execute(label,name,*args):
 r=command(label,[sys.executable,'-B',str(HERE/name),*args]);print(label,r['exit_code'],flush=True);assert r['exit_code']==0,'Retain failure and version any corrective successor: '+label
 return clean(r)
prior=RUNROOT/'commands/w2c14-default-reader-unsealed-v1/command.json';assert json.loads(prior.read_bytes())['exit_code']==0
controls=execute('seal-and-controls-v1','seal_controls_v4.py')
physical=execute('publication-negative-v1','publication_adverse_v2.py')
attempts=[]
for folder in sorted((RUNROOT/'commands').glob('w2c14-*')):
 p=folder/'command.json'
 if p.is_file():
  r=json.loads(p.read_bytes())
  if r['exit_code']:attempts.append(dict(command=pin(p),stdout=pin(folder/'stdout.log'),stderr=pin(folder/'stderr.log'),exit_code=r['exit_code']))
save('inherited-failure-mutation-ledger-v3.json',dict(nonzero_commands=attempts,previous=pin(HERE/'inherited-failure-mutation-ledger-v2.json'),retained_drafts='All bounded current C14 sources/outputs/raw attempts remain in the new private manifest; all earlier11875 pins remain.',assertions_changed=0,original_queries=0))
save('prepublication-reader-execution-v1.json',dict(sealed_reader_controls=controls,physical_adverse_controls=physical,unsealed_actual_default_command=pin(prior),owner_plan_authority=pin(HERE/'owner-plan-authorized-transition-v1.json'),closure='35 historical /35 current /3 support, partial W2 NOT_READY',original_queries=0))
# Nothing in HERE changes after the exact logical inventory. Later facts are
# external additive receipts or ignored private raw streams, never hidden edits.
execute('publication-inventory-v1','publish_parts_v1.py','inventory')
execute('publication-export-v1','publish_parts_v1.py','export')
execute('copied-union-v1','copy_review_v2.py')
execute('final-live-v1','finalize_live_v2.py')
execute('capture-final-git-v1','capture_exit_git_v1.py')
print('Publication/copied/live/Git complete; private manifest may now be generated directly without a growing Runner log',flush=True)
