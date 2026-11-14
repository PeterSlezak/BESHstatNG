"""Syntax-only declaration/fragment census of new portable and required source files."""
from common_v2 import *
tool=RUNROOT/'completion-w2-cont8/portable-census-v1/bin/Census.dll';assert tool.is_file()
portable=[p['path'] for p in json.loads((HERE/'implementation-plan-v1.json').read_bytes())['new_sources'] if p['path'].startswith('BESHStat.Excel.Common/src/')]
paths=['BESHstatNG/src/RegModels/BinaryClassificationReporting.vb','BESHstatNG/src/DM/WindowsSpreadsheetTransport.vb','BESHStat.Core/src/StatTests/StatisticalResultContracts.vb','BESHStat.Core/src/RegModels/BinaryClassificationCore.vb','BESHStat.Core/src/StatTests/FisherExact.vb','BESHStat.Core/src/StatTests/ContingencyTable.vb']
# Locate the existing named contingency module without a broad ignored-tree walk.
paths=[p for p in paths if (ROOT/p).is_file()]
paths.extend((ROOT/'BESHStat.Core/src/StatTests/Agreement'/n).relative_to(ROOT).as_posix() for n in ['Agreement.vb','AgreementHelpers.vb','AgreementWeightedDeming.vb','AgreementBlandAltman.vb','AgreementLinConcordance.vb','AgreementWeightedKappa.vb','AgreementPlotData.vb'])
for name,files in [('portable-universe-v1.json',portable),('dependency-source-universe-v1.json',paths)]:
    r=command(name[:-5]+'-syntax',['dotnet',str(tool),str(ROOT),str(HERE/name),*files]);assert r['exit_code']==0
save('dependency-census-authority-v1.json',dict(tool=pin(tool),source=pin(PREVIOUS/'PortableCensus-v1.cs'),syntax_only=True,original_source_compiler_runs=0,files=paths,portable=portable))
raw=git('cat-file','blob',BASE+':BESHstatNG/src/RegModels/BinaryClassificationReporting.vb');target=HERE/'source-original/BinaryClassificationReporting.vb.txt';target.parent.mkdir(exist_ok=True)
with target.open('xb') as f:f.write(raw)
old=next(r for r in json.loads((HERE/'implementation-plan-v1.json').read_bytes())['source_before'] if r['live']['path']=='BESHstatNG/src/RegModels/BinaryClassificationReporting.vb')
assert raw.decode('utf-8-sig').replace('\r\n','\n')==(ROOT/old['preimage']['path']).read_text(encoding='utf-8-sig')
save('classification-original-source-authority-v1.json',dict(git_object=BASE+':BESHstatNG/src/RegModels/BinaryClassificationReporting.vb',snapshot=pin(target),immediate_preimage=old['preimage'],relationship='FULL_UTF8_CRLF_LF_NORMALIZED_IDENTITY_BEFORE_C9; no code compiled or executed',original_queries=0))
print('New portable/required dependency syntax censuses and exact original report source pinned')
