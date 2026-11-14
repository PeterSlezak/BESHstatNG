"""Freeze all inherited dirty source and the exact bounded two-packet plan."""
from common import *
assert json.loads((HERE/'entry-authentication-v1.json').read_bytes())['status']=='ENTRY_GATES_PASS'
assert len(json.loads((HERE/'entry-queue-receipt-v1.json').read_bytes())['rows'])==58
modified=git('diff','--name-only','-z').decode().split('\0')
untracked=git('ls-files','--others','--exclude-standard','-z','--','BESHStat.Core','BESHStat.Excel.Common','BESHStat.Core.Tests','BESHStat.Excel.Common.Tests','BESHstatNG','BESHStatNG.Test').decode().split('\0')
rows=[]
for name in sorted(set(modified+untracked)-{''}):
 source=ROOT/name;target=HERE/'source-before'/(name+'.txt');target.parent.mkdir(parents=True,exist_ok=True)
 with target.open('xb') as f:f.write(source.read_bytes())
 rows.append(dict(live=pin(source),preimage=pin(target)))
save('all-edited-source-preimages-v1.json',dict(files=rows,count=len(rows),scope='All tracked diffs plus explicit untracked production/test/project paths only; no ignored historical TestResults traversal.'))
native=['BESHstatNG/src/ExcelUDFs/Regression/'+n+'.vb' for n in ['GeeUDFs','GlmNbUDFs','GlmUDFs','LinearModelUDFs','LmmUDFs']]
paths=native+['BESHStat.Excel.Common/src/Results/UdfRegressionFamilyReportPreparation38.vb','BESHStat.Excel.Common/src/Analysis/UdfLmmInputPreparation38.vb','BESHStat.Excel.Common/src/Results/UdfLmmReportPreparation38.vb','BESHStat.Excel.Common.Tests/src/UdfContinuation16Tests.vb','BESHStatNG.Test/src/UDFs/UdfPreparation38AdapterTests.vb']
before=[];absent=[]
for path in paths:
 if not (ROOT/path).exists():absent.append(dict(path=path,entry_state='ABSENT'));continue
 match=next((r for r in rows if r['live']['path']==path),None)
 if match is None:
  source=ROOT/path;target=HERE/'source-before'/(path+'.txt');target.parent.mkdir(parents=True,exist_ok=True)
  with target.open('xb') as f:f.write(source.read_bytes())
  match=dict(live=pin(source),preimage=pin(target))
 before.append(match)
save('implementation-plan-v1.json',dict(source_paths=paths,source_before=before,new_sources=absent,entry=pin(HERE/'entry-authentication-v1.json'),queue=pin(HERE/'entry-queue-receipt-v1.json'),packet_A=native[:4]+[paths[5],paths[8],paths[9]],packet_B=[native[4],paths[6],paths[7],paths[8],paths[9]],changes=['Packet A: source-bound GEE LS-means table preparation; shared GLM/NB coefficient/diagnostic/residual/name preparation, reuse existing identical prediction table helper; LM table/footnote/selection preparation. No scientific engine/formula changes.','Packet B: typed complete-row LMM preparation with unchanged native conversion callbacks, existing portable formula-design orchestration, fitted/residual tables and copy-safe result-name projection. Keep imports/cache/registration/errors/exception materialization in Windows.','Full first58 plus genuine dependency original/current/fragment/callback/Debug CLR proofs. Preserve N/Get distinction and authentic original-only Common GEE lambda. No MMRM family expansion merely for coverage.'],development_tests='NOT_RUN_UNTIL_COMBINED_FREEZE',Debug_comprehensive_campaigns=1,Release_compilation=DEFERRED,Release_unit_suites=DEFERRED,Release_ABI_CIL_exports=DEFERRED,Release_frozen_compatibility=DEFERRED,original_queries=0))
print('Entry preimages',len(rows),'bounded planned paths',len(paths),'new files',len(absent),flush=True)
