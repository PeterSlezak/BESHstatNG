from common40_v3 import *
from collections import Counter
lock();run=runner();result=run.build('Release','a40-final-v2')
save(PRIVATE/'Release-build-v2.json',result);assert not any(result['exit_codes']) and result['source_unchanged']
selection='|'.join('FullyQualifiedName~'+x for x in ['Regression40','Regression08','Regression09','Regression10','Regression11','Gee12','Survival13','Mixed14','Mixed15','Mixed16',
 'UdfRegression','UdfCox','UdfGee','UdfLmm','UdfMmrm','UdfW2Completion','UdfPreparation38','UdfInputPolicy39','UdfInputBoundary39'])
parts=[];counts=Counter()
for suite in ['Common','Legacy']:
    name='a40-final2-Release-'+suite+'-targeted';test=run.test('Release','a40-final-v2',suite,name,selection)
    save(PRIVATE/(name+'.json'),test);counts.update(test['outcomes'])
    parts.append(dict(suite=suite,part='targeted',selection=selection,name=name,outcomes=test['outcomes'],
        cases=pin(PRIVATE/'executions'/name/'cases.json'),TRX=pin(PRIVATE/'executions'/name/'results'/(suite+'.trx')),raw=pin(PRIVATE/(name+'.json')),command_exit=test['command']['exit_code']))
save('build-and-tests/Release-targeted-executed-v2.json',dict(configuration='Release',mode='TARGETED_FRESH',parts=parts,outcomes=dict(counts),identities=sum(counts.values()),
    source_unchanged=result['source_pins']==run.__class__.build.__globals__['source_pins'](),full_Release='NOT_RUN;targeted family tests plus per-config ABI/CIL,77 input fixtures and29424 compatibility cases; no concrete release-only risk requires full campaign.'))
lock();print('Release build/targeted completed',sum(counts.values()),dict(counts),flush=True)
