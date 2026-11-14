"""Restrict lifecycle root mapping to the actual selected roots, not unrelated accessors."""
from common import *
source=HERE/'entry_queue_v2.py';value=source.read_text(encoding='utf-8')
old="root_history={resolve(h)['symbol_id']:h for h in history if h['current_ids'] and len(h['current_ids'])==1}"
new="root_ids={n['symbol_id'] for n in roots}\nroot_history={h['current_ids'][0]:h for h in history if len(h['current_ids'])==1 and h['current_ids'][0] in root_ids}\nassert set(root_history)==root_ids,'SELECTED_ROOT_HISTORY_MISSING_STOP'"
assert old in value;target=HERE/'entry_queue_v3.py'
with target.open('x',encoding='utf-8',newline='\n') as stream:stream.write(value.replace(old,new))
folder=RUNROOT/'commands/w2c13-entry-queue-v2'
save('entry-queue-root-scope-correction-v1.json',dict(before=pin(source),after=pin(target),unsuccessful_attempt={n:pin(folder/n) for n in ['command.json','stdout.log','stderr.log']},
    reason='V2 fully bound the selected24 including both distinct lambdas, then unnecessarily applied the lambda resolver to all704 historical members while constructing the native-root map. An unrelated Lmm accessor correctly rejected that misuse. V3 maps only exact current IDs of selected native parent/lifecycle roots and requires every root. No closure or source identity was changed or guessed.',production_edits=0,original_queries=0))
result=command('entry-queue-v3',[sys.executable,'-B',str(target)])
save('entry-queue-execution-v3.json',clean(result));assert result['exit_code']==0,'ENTRY_QUEUE_NOT_AUTHENTICATED_NO_IMPLEMENTATION'
