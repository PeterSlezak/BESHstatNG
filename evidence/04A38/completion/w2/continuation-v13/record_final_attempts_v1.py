from common import *
attempts=[]
for folder in sorted((RUNROOT/'commands').glob('w2c13-*')):
 p=folder/'command.json'
 if p.is_file():
  d=json.loads(p.read_bytes())
  if d['exit_code']:attempts.append(dict(command=pin(p),stdout=pin(folder/'stdout.log'),stderr=pin(folder/'stderr.log'),exit_code=d['exit_code'],args=clean(d['command'])))
save('inherited-failure-mutation-ledger-v2.json',dict(nonzero_commands=attempts,previous=pin(HERE/'inherited-failure-mutation-ledger-v1.json'),historical_assertions_changed=0,original_queries=0,
    shell_bootstrap=pin(HERE/'shell-bootstrap-correction-v1.json'),scope='All completed captured commands up to final seal, including inherited test failures and each proof rejection/corrective attempt. Any later rejection receives an additive external receipt; never overwrite earlier bytes.'))
