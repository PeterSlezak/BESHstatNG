"""Version current-only measurement tools; reuse authenticated preserved original metadata."""
from common import *
for name in ['inspect_stage_v1.py','inspect_metadata_v5.py','return_metadata_v1.py','declaration_shapes_v1.py']:
    source=(PREVIOUS/name).read_text(encoding='utf-8').replace('from common_v2 import *','from common import *')
    source=source.replace('packet-v1','packet-v2')
    if name=='inspect_metadata_v5.py':
        source=source.replace("PREVIOUS/'metadata-all/packet-v2-context-v3'","PREVIOUS/'metadata-all/packet-v1-context-v5'")
    if name=='return_metadata_v1.py':
        source=source.replace("'BESHStatNG.ExcelCommon.UdfAgreementReportPreparation38'","'BESHStatNG.ExcelCommon.UdfMultivariateReportPreparation38','BESHStatNG.ExcelCommon.UdfAgreementReportPreparation38'")
    if name=='declaration_shapes_v1.py':source=source.replace("paths.append('BESHstatNG/src/RegModels/BinaryClassificationReporting.vb')", "paths=list(dict.fromkeys(paths))")
    with (HERE/name).open('x',encoding='utf-8',newline='\n') as f:f.write(source)
save('measurement-tool-crosswalk-v1.json',dict(files=[dict(before=pin(PREVIOUS/n),after=pin(HERE/n)) for n in ['inspect_stage_v1.py','inspect_metadata_v5.py','return_metadata_v1.py','declaration_shapes_v1.py']],stage='packet-v2',original_queries=0,original_activity='Original source parsing and byte-exact reuse of authenticated metadata only; no original DLL loading, compilation or execution.'))
print('Current Debug/Release measurement tools versioned')
