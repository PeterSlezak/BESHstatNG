from common import *
result=command('entry-queue-v2',[sys.executable,'-B',str(HERE/'entry_queue_v2.py')])
save('entry-queue-execution-v2.json',clean(result));assert result['exit_code']==0,'ENTRY_QUEUE_NOT_AUTHENTICATED_NO_IMPLEMENTATION'
