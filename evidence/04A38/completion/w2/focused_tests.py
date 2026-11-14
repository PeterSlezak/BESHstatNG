"""Compile/test only new profile seams in isolated Debug/Release lanes."""
import sys,json
from pathlib import Path
HERE=Path(__file__).resolve().parent;ROOT=HERE.parents[3]
G=ROOT/'evidence/04A38/late-archived-entry-supplement/v7-consolidated-gee-spec-mmrm-wide';sys.path.insert(0,str(G))
from common import RUNROOT,m,clean

def main():
 config,attempt=sys.argv[1:3];run=RUNROOT/'completion-w2'/'profiles-v3'/config;output=run/'lib';r=m.Runner(RUNROOT);records=[]
 for suite,project in [('Core','BESHStat.Core.Tests/BESHStat.Core.Tests.vbproj'),('Common','BESHStat.Excel.Common.Tests/BESHStat.Excel.Common.Tests.vbproj')]:
  args=['dotnet','msbuild',str(ROOT/project),'/t:Build','/p:Configuration='+config,'/p:BuildProjectReferences=false','/p:ProduceReferenceAssembly=false',
    '/p:OutputPath='+str(output)+'\\','/p:IntermediateOutputPath='+str(run/'obj'/('tests-'+attempt+'-'+suite))+'\\','/m:4','/v:minimal','/nr:false']
  b=r.command('w2-'+attempt+'-'+config+'-build-tests-'+suite,args);records.append(clean(b))
  if b['exit_code']!=0:break
  dest=run/('results-'+attempt+'-'+suite);dest.mkdir()
  dll=output/('BESHStat.'+('Core' if suite=='Core' else 'Excel.Common')+'.Tests.dll')
  t=r.command('w2-'+attempt+'-'+config+'-focused-'+suite,['dotnet','vstest',str(dll),'/TestCaseFilter:FullyQualifiedName~UdfModelProfilePreparation38Tests|FullyQualifiedName~UdfRowMaskPreparation38Tests','/Logger:trx;LogFileName=focused.trx','/ResultsDirectory:'+str(dest),'/Settings:'+str(ROOT/'test.runsettings')])
  record=dict(command=clean(t))
  if (dest/'focused.trx').exists():record['tests']=clean(m.trx(dest/'focused.trx'))
  records.append(record)
 with (HERE/('focused-'+attempt+'-'+config+'.json')).open('x',encoding='utf-8') as f:json.dump(dict(configuration=config,records=records),f,indent=2)
 assert len(records)==4 and all((a.get('command') or a)['exit_code']==0 for a in records),'Focused tests or compilation failed; preserve receipt'
 print('Focused profile/row-mask tests PASS',config)
if __name__=='__main__':main()
