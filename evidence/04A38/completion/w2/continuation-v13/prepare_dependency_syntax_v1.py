from common import *
rows=[]
for name in ['source_views.py','proof_kernel_v4.py']:
    source=PREVIOUS/name
    with (HERE/name).open('xb') as stream:stream.write(source.read_bytes())
    rows.append(dict(before=pin(source),after=pin(HERE/name)))
source=PREVIOUS/'dependency_context_v1.py';value=source.read_text(encoding='utf-8');value=value[:value.index("prior=json.loads((PREVIOUS/'current-ownership-v2.json')")]
needle="'BESHStat.Excel.Common/src/Results/UdfTypedReportPreparation38.vb']";assert needle in value
value=value.replace(needle,"'BESHStat.Excel.Common/src/Results/UdfTypedReportPreparation38.vb','BESHStat.Excel.Common/src/Analysis/UdfPlotDataPreparation38.vb','BESHStat.Excel.Common/src/Analysis/UdfClassLabelPreparation38.vb']")
value+="save('dependency-syntax-authority-v1.json',dict(commands=commands,syntax_tool=pin(tool),shape_source=pin(HERE/'DeclarationShapes-v1.cs'),shape_tool=pin(folder/'bin/Census.dll'),return_source=pin(HERE/'ReturnMetadata-v1.cs'),return_tool=pin(exe),original_queries=0,scope='Actual current whole Common/Core syntax, exact native/portable parameters and measured Debug/Release return metadata. Source/CIL graph and ownership reconciliation are separate mandatory proofs.'))\nprint('Full current dependency/parameter/return syntax ready')\n"
target=HERE/'dependency_syntax_v1.py'
with target.open('x',encoding='utf-8',newline='\n') as stream:stream.write(value)
rows.append(dict(before=pin(source),after=pin(target),scope='Preserved exact syntax/compiler recipes; separate identity and graph reconciliation follows instead of an unproved generic callback mapping.'))
save('dependency-syntax-tool-crosswalk-v1.json',dict(rows=rows,original_queries=0))
result=command('dependency-syntax-driver-v1',[sys.executable,'-B',str(target)]);assert result['exit_code']==0
