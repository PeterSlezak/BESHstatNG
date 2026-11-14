from datetime import datetime,timezone
from common import *
for name in ['w2c2-validation-v1-python-scripts','w2c2-predecessor-W1']:
 p=RUNROOT/'commands'/name
 if p.is_dir():
  print(name,'completed',(p/'command.json').is_file())
  log=p/'stderr.log'
  if log.exists():
   lines=log.read_text(encoding='utf-8',errors='replace').splitlines();print('bytes',log.stat().st_size,'last',clean('\n'.join(lines[-3:])))
 else:print(name,'not_started')
print('UTC',datetime.now(timezone.utc).isoformat())
