"""Isolated offline builds, focused new tests and one final Debug managed census."""
from common39 import *
from collections import Counter
import shutil

source_lock()
config,mode=sys.argv[1:]
assert config in ['Debug','Release']
label='a39v4'
run=runner()
if mode=='build':
    raw=PRIVATE/'builds'/label/config/'build.json'
    if raw.is_file():
        assert config=='Debug', 'Only preserved completed Debug report recovery is scheduled'
        result=load(raw)
        assert result['source_pins']==run.__class__.build.__globals__['source_pins']()
        save('build-and-tests/Debug-report-recovery-v1.json',dict(raw=pin(raw),reason='Append-only report name collision; all nine build commands already completed0. No compilation repeated.'))
    else:result=run.build(config,label)
    save('build-and-tests/'+config+'-build-v4.json',result)
    assert not any(result['exit_codes']) and result['source_unchanged']
    folder=PRIVATE/'shipping-final'/config;folder.mkdir(parents=True,exist_ok=False)
    for name in ['BESHStatNG.dll','BESHStat.Core.dll','BESHStat.Excel.Common.dll','ExcelDna.Integration.dll']:
        shutil.copy2(PRIVATE/'builds'/label/config/'legacy'/name,folder/name)
    save('abi-cil/'+config+'-shipping-v4.json',dict(assemblies=[pin(p) for p in sorted(folder.glob('*.dll'))]))
elif mode in ['focused','full']:
    build=load(HERE/('build-and-tests/'+config+'-build-v4.json'))
    assert not any(build['exit_codes']) and build['source_pins']==run.__class__.build.__globals__['source_pins']()
    if mode=='full':
        assert config=='Debug'
        suites=[('Core','all',None),('Common','main','FullyQualifiedName!~MigrationSemantic04A02Tests'),
            ('Common','semantic','FullyQualifiedName~MigrationSemantic04A02Tests'),('Legacy','all',None),
            ('ControlMitigation','all',None),('OfficeAddin','all',None),('OfficeExecution','all',None)]
    elif config=='Debug':
        suites=[('Common','new','FullyQualifiedName~UdfInputPolicy39Tests'),
                ('Legacy','new','FullyQualifiedName~UdfInputPolicy39Tests|FullyQualifiedName~UdfInputBoundary39Tests')]
    else:
        selection='|'.join('FullyQualifiedName~'+x for x in ['UdfInputPolicy39Tests','UdfInputBoundary39Tests',
            'PairedBlockPolicyTests','PairedBlockAdapterTests','PhysicalImporter27Tests','PhysicalImporter27ColumnNameTests',
            'UdfImportFacadeTests','UdfPreparation38AdapterTests','UdfW2CompletionPreparation38'])
        suites=[('Common','targeted',selection),('Legacy','targeted',selection)]
    parts=[];counts=Counter()
    for suite,part,selection in suites:
        name='a39f'+config[0]+'-'+mode+'-'+suite+'-'+part
        result=run.test(config,label,suite,name,selection)
        record=dict(suite=suite,part=part,selection=selection,name=name,outcomes=result['outcomes'],
            cases=pin(PRIVATE/'executions'/name/'cases.json'),TRX=pin(PRIVATE/'executions'/name/'results'/(suite+'.trx')),
            command=result['command'])
        save('build-and-tests/'+config+'-'+mode+'-'+suite+'-'+part+'-v4.json',record)
        parts.append(record);counts.update(result['outcomes'])
    save('build-and-tests/'+config+'-'+mode+'-executed-v4.json',dict(configuration=config,mode=mode,parts=parts,
        outcomes=dict(counts),identities=sum(counts.values()),source_unchanged=build['source_pins']==run.__class__.build.__globals__['source_pins']()))
    if mode=='focused':assert counts.get('Failed',0)==0 and set(counts)=={'Passed'},'NEW_FOCUSED_TEST_FAILURE'
else:raise ValueError(mode)
source_lock()
