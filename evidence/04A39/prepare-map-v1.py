"""Derive a bounded current-source Roslyn map tool from the accepted syntax reader."""
from common39 import *

source_lock()
tool=HERE/'tools';tool.mkdir(exist_ok=False)
original=ROOT/'evidence/04A38/helpers/Program.cs'
text=original.read_text(encoding='utf-8-sig')
old='Directory.GetFiles("BESHstatNG/src/ExcelUDFs","*.vb",SearchOption.AllDirectories).Order()'
new='File.ReadAllLines(args[1]).Order()'
assert text.count(old)==1
text=text.replace(old,new)
old='if(exports.Count!=268||sources.Count!=41)throw new Exception("Scope count mismatch");'
assert text.count(old)==1
text=text.replace(old,'if(exports.Count!=268 || sources.Count!=File.ReadAllLines(args[1]).Length)throw new Exception("04A39 source/export scope mismatch");')
old='catches=n.DescendantNodes().OfType<CatchStatementSyntax>().Select(a=>Tokens(a)).ToArray(),'
new=old+'\n      statements=n.DescendantNodes().OfType<StatementSyntax>().Select(a=>new{kind=a.Kind().ToString(),line=a.GetLocation().GetLineSpan().StartLinePosition.Line+1,tokens=Tokens(a)}).ToArray(),'
assert text.count(old)==1;text=text.replace(old,new)
(tool/'Program.cs').write_text(text,encoding='utf-8',newline='\n')
(tool/'Inventory39.csproj').write_text((ROOT/'evidence/04A38/helpers/Inventory38.csproj').read_text(),encoding='utf-8',newline='\n')
(tool/'NuGet.Config').write_text('<configuration><packageSources><clear /></packageSources></configuration>\n',encoding='utf-8')
paths={p.as_posix() for p in Path('BESHstatNG/src/ExcelUDFs').rglob('*.vb')}
paths|={p.as_posix() for p in Path('BESHStat.Excel.Common/src').rglob('Udf*38.vb')}
paths|={p.as_posix() for p in Path('BESHStat.Excel.Common/src/Input').glob('*.vb')}
paths|={p.as_posix() for p in Path('BESHStat.Core/src').rglob('Udf*38.vb')}
paths|={'BESHstatNG/src/DM/'+name+'.vb' for name in ['WindowsPairedInputAdapter','WindowsSpreadsheetTransport','ExcelDnaDataImporter','ExcelDnaSharedApplicationAdapter','DataObjContracts','Helpers']}
paths=sorted(paths)
(tool/'source-paths-v1.txt').write_text('\n'.join(paths)+'\n',encoding='utf-8',newline='\n')
save('map/tool-provenance-v1.json',dict(authority=pin(original),derived=pin(tool/'Program.cs'),
    changes=['Explicit bounded source list expands accepted syntax reader to current Common/Core/DM policies.',
             'Same268-export assertion retained; source count bound to explicit list.',
             'Added source statement/branch tokens, not a semantic equivalence assertion.'],
    source_files=[pin(x) for x in paths],framework='net10.0 diagnostic only; no production targets changed'))
out=PRIVATE/'map-tool';obj=PRIVATE/'map-obj'
for label,args in [('map-restore',['dotnet','restore',str(tool/'Inventory39.csproj'),'--configfile',str(tool/'NuGet.Config'),'/p:BaseIntermediateOutputPath='+str(obj)+'\\']),
    ('map-build',['dotnet','build',str(tool/'Inventory39.csproj'),'--no-restore','-o',str(out),'/p:BaseIntermediateOutputPath='+str(obj)+'\\'])]:
    receipt=command(label,args);save('map/'+label+'-v1.json',receipt);assert receipt['exit_code']==0
receipt=command('map-execute',['dotnet',str(out/'Inventory39.dll'),str(HERE/'map/current-source-syntax-v1.json'),str(tool/'source-paths-v1.txt')])
save('map/execute-v1.json',receipt);assert receipt['exit_code']==0
source_lock()
