"""Save the complete dirty-source entry and concrete bounded implementation plan."""
from common import *
assert json.loads((HERE/'entry-authentication-v2.json').read_bytes())['status']=='ENTRY_GATES_PASS'
assert len(json.loads((HERE/'entry-queue-receipt-v1.json').read_bytes())['rows'])==24
modified=git('diff','--name-only','-z').decode().split('\0')
untracked=git('ls-files','--others','--exclude-standard','-z','--','BESHStat.Core','BESHStat.Excel.Common','BESHStat.Core.Tests','BESHStat.Excel.Common.Tests','BESHstatNG','BESHStatNG.Test').decode().split('\0')
rows=[]
for name in sorted(set(modified+untracked)-{''}):
    source=ROOT/name
    target=HERE/'source-before'/(name+'.txt')
    target.parent.mkdir(parents=True,exist_ok=True)
    with target.open('xb') as stream:stream.write(source.read_bytes())
    rows.append(dict(live=pin(source),preimage=pin(target)))
save('all-edited-source-preimages-v1.json',dict(files=rows,count=len(rows),scope='All tracked diffs plus untracked production/test/project files; no historical TestResults traversal.'))
paths=['BESHstatNG/src/ExcelUDFs/Regression/CoxUDFs.vb',
       'BESHstatNG/src/ExcelUDFs/Regression/GeeUDFs.vb',
       'BESHStat.Excel.Common/src/Analysis/UdfCoxInputPreparation38.vb',
       'BESHStat.Excel.Common/src/Results/UdfCoxGeeReportPreparation38.vb',
       'BESHStat.Excel.Common.Tests/src/UdfContinuation15Tests.vb',
       'BESHStatNG.Test/src/UDFs/UdfPreparation38AdapterTests.vb']
before=[r for r in rows if r['live']['path'] in paths]
new=[dict(path=p,entry_state='ABSENT') for p in paths if not (ROOT/p).exists()]
assert len(before)+len(new)==len(paths)
save('implementation-plan-v1.json',dict(source_paths=paths,source_before=before,new_sources=new,
    entry=pin(HERE/'entry-authentication-v2.json'),queue=pin(HERE/'entry-queue-receipt-v1.json'),
    changes=['Extract pure Cox record preparation using existing Core SurvivalRecord, preserving source row identity, censor/stratum values and covariate allocation order.',
             'Extract Cox/GEE report matrix/header/row preparation, leaving engine fitting/numerics, Excel imports, error policies, handle state, exception logging and materialization in their existing owners.',
             'Reconcile all selected complete bodies, exact Cox lambda and four moved GEE property accessors with actual constructed Debug/Release consumers; add only source-authenticated coherent dependency closures.',
             'Keep all original defaults, evaluation order, exceptional paths, source IDs, frozen references and prior supported bodies; unsupported dependencies remain OPEN.'],
    Release_full='NOT_RUN_OWNER_PREFERENCE',Release_selected='NOT_RUN_OWNER_PREFERENCE',original_queries=0))
print(json.dumps(dict(source_preimages=len(rows),planned_paths=paths,new_paths=[r['path'] for r in new])))
