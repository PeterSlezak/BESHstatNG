"""One planned six-suite full Debug run. Release unit suites deliberately absent."""
from common import *

LABEL='w2c7-checkpoint-v1'
class CheckpointRunner(m.Runner):
    def command(self,name,args,cwd=ROOT,env=None):
        if name==LABEL+'-Debug-serial-Common':
            env=dict(env or {},BESHSTAT_04A02_OBSERVATIONS=str(RUNROOT/'executions'/name/'observations'))
        return super().command(name,args,cwd,env)

runner=CheckpointRunner(RUNROOT);before=m.source_pins();start=time.monotonic()
build=runner.build('Debug',LABEL)
assert not any(build['exit_codes']),'Retain failed build; no implicit restore'
results=[]
for suite in ['Core','Common','Legacy','ControlMitigation','OfficeAddin','OfficeExecution']:
    result=runner.test('Debug',LABEL,suite,LABEL+'-Debug-serial-'+suite)
    results.append(result)
    save('managed-executed-'+suite+'-v1.json',dict(suite=suite,outcomes=result['outcomes'],trx_sha256=result['sha256'],command=clean(result['command']),raw_cases=pin(RUNROOT/'executions'/(LABEL+'-Debug-serial-'+suite)/'cases.json')))
raw=PRIVATE/'full-managed-Debug-v1.json'
with raw.open('x',encoding='utf-8',newline='\n') as stream:
    json.dump(dict(build=clean(build),results=clean(results),source_unchanged=before==m.source_pins()),stream,ensure_ascii=False,indent=2);stream.write('\n')
save('managed-full-execution-v1.json',dict(raw=pin(raw),private=True,seconds=time.monotonic()-start,source_unchanged=before==m.source_pins(),Release_full='NOT_RUN_OWNER_PREFERENCE',Release_selected='NOT_RUN_OWNER_PREFERENCE',Common_observations='The full Debug Common run emits its existing semantic candidates to its isolated execution directory, avoiding a duplicate semantic subset run.'))
assert before==m.source_pins()
print('Full six-suite Debug run completed; compare every identity/nonpass before acceptance',flush=True)
