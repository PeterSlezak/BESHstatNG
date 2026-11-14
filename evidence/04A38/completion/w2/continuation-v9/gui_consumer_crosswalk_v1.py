"""Read-only source and real current CIL shared-engine GUI/UDF context, not GUI closure."""
from common_v2 import *
paths=['BESHstatNG/src/UI/Ui0OneRefeditMulticol.vb','BESHstatNG/src/UI/Ui9ANOVA2nested.vb','BESHstatNG/src/UI/UiGLM.vb','BESHstatNG/src/UI/UiTwoInputRefedits.vb'];source=[]
for path in paths:
    current=(ROOT/path).read_bytes();original=git('cat-file','blob',BASE+':'+path);assert current.decode('utf-8-sig').replace('\r\n','\n')==original.decode('utf-8-sig').replace('\r\n','\n')
    lines=current.decode('utf-8-sig').splitlines();hits=[dict(line=i+1,text=line.strip()) for i,line in enumerate(lines) if ('New Agreement.' in line and any(n in line for n in ['PassinbBablok','WeightedDemingRegression','BlandAltmanAgreement','LinConcordanceCorrelation','WeightedKappaAgreement','IntraclassCorrelation'])) or 'BinaryClassificationReporting.ComposeGlmReport(' in line]
    source.append(dict(current=pin(ROOT/path),original_git_sha256=sha(original),normalization='FULL_UTF8_CRLF_LF_ONLY',calls=hits))
tool=PRIVATE/'gui-consumer-edges-v1';tool.mkdir(exist_ok=False);exe=tool/'ConsumerEdges.exe';r=command('gui-consumer-edges-tool-v1',[compiler('csc.exe'),'/nologo','/target:exe','/reference:System.Core.dll','/reference:System.Web.Extensions.dll','/out:'+str(exe),str(HERE/'ConsumerEdges-v1.cs')]);assert r['exit_code']==0
files=[]
for config in ['Debug','Release']:
    output=HERE/('gui-udf-consumer-edges-'+config+'-v1.json');r=command('gui-consumer-edges-'+config+'-v1',[str(exe),str(PRIVATE/'packet-v1'/config/'lib'),str(output)]);assert r['exit_code']==0;data=json.loads(output.read_bytes());assert any(not row['caller'].startswith('BESHStatNG.WorksheetFunctions.') and row['target_assembly']=='BESHStat.Core' for row in data['rows']);files.append(pin(output))
save('gui-udf-consumer-crosswalk-v1.json',dict(source=source,current_edges=files,recorder=pin(HERE/'ConsumerEdges-v1.cs'),scope='Actual GUI and UDF call operands resolve to the same existing Core agreement/classification implementations. Their original option/input/report conventions remain distinct; no GUI/UDF discrepancy is corrected or equated across different requests. Whole GUI parents/native state are not claimed closed; no GUI/native/Excel execution occurred.',original_queries=0))
