"""One isolated build/test packet per configuration; no retries or source edits."""
from common import *
import shutil
from collections import Counter

lock()
config,mode=sys.argv[1:]
assert config in ['Debug','Release']
assert load(HERE/'matrix/verification-matrix-v1.json')['status']=='LOCKED_BEFORE_EXECUTION'
runner=m.Runner(RUNROOT)
if mode=='build':
    result=runner.build(config,LABEL)
    save('build-and-tests/'+config+'-build-v1.json',dict(result=result,raw=pin(RUNROOT/'builds'/LABEL/config/'build.json'),
        source_freeze=pin(ACCEPTED/'production-test-freeze-v1.json'),campaigns=1))
    assert not any(result['exit_codes']) and result['source_unchanged'],'BUILD_STOP'
    folder=PRIVATE/config/'lib'
    folder.mkdir(parents=True,exist_ok=False)
    for n in ['BESHStatNG.dll','BESHStat.Core.dll','BESHStat.Excel.Common.dll','ExcelDna.Integration.dll']:
        shutil.copy2(RUNROOT/'builds'/LABEL/config/'legacy'/n,folder/n)
    save('abi-cil/'+config+'-shipping-v1.json',dict(configuration=config,assemblies=[pin(p) for p in sorted(folder.glob('*.dll'))],
        build=pin(HERE/('build-and-tests/'+config+'-build-v1.json'))))
elif mode=='tests':
    result=load(HERE/('build-and-tests/'+config+'-build-v1.json'))['result']
    assert not any(result['exit_codes']) and result['source_unchanged']
    suites=[('Core','all',None),('Common','main','FullyQualifiedName!~MigrationSemantic04A02Tests'),
        ('Common','semantic','FullyQualifiedName~MigrationSemantic04A02Tests'),('Legacy','all',None),
        ('ControlMitigation','all',None),('OfficeAddin','all',None),('OfficeExecution','all',None)]
    parts=[];totals=Counter()
    for suite,part,selection in suites:
        name='w3'+config[0]+'-'+suite+('-'+part if suite=='Common' else '')
        test=runner.test(config,LABEL,suite,name,selection)
        row=dict(suite=suite,part=part,name=name,outcomes=test['outcomes'],
            cases=pin(RUNROOT/'executions'/name/'cases.json'),
            TRX=pin(RUNROOT/'executions'/name/'results'/(suite+'.trx')),command=test['command'])
        save('build-and-tests/'+config+'-'+suite+'-'+part+'-v1.json',row)
        parts.append(row);totals.update(test['outcomes'])
    save('build-and-tests/'+config+'-executed-v1.json',dict(configuration=config,parts=parts,
        outcomes=dict(totals),identities=sum(totals.values()),campaigns=1,source_unchanged=result['source_pins']==m.source_pins()))
else: raise ValueError(mode)
lock()
print(config,mode,'packet complete',flush=True)
