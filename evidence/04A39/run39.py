"""Run one versioned packet with immutable raw command receipts."""
from common39 import *
name,script,*args=sys.argv[1:]
result=command('packet-'+name,bootstrap(script,*args))
save('execution/'+name+'-v1.json',result)
raise SystemExit(result['exit_code'])
