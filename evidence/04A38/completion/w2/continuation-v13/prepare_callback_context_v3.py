from common import *
source=HERE/'callback_context_v2.py';value=source.read_text(encoding='utf-8');assert value.count("['exception_regions']")==2
target=HERE/'callback_context_v3.py'
with target.open('x',encoding='utf-8',newline='\n') as stream:stream.write(value.replace("['exception_regions']","['exceptions']"))
folder=RUNROOT/'commands/w2c13-callback-context-v2'
save('callback-proof-metadata-schema-correction-v1.json',dict(before=pin(source),after=pin(target),unsuccessful_attempt={n:pin(folder/n) for n in ['command.json','stdout.log','stderr.log']},
    reason='Recorded MethodMetadata uses the exact key exceptions. V2 had already compared the complete callback instructions/parameters/locals but requested a nonexistent exception_regions field. V3 compares the actual complete exceptions arrays; no skipped or inferred metadata and no production/test change.',original_queries=0))
result=command('callback-context-v3',[sys.executable,'-B',str(target)])
save('callback-context-execution-v3.json',clean(result));assert result['exit_code']==0
