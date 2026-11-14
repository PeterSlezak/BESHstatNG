"""Record concrete C9 production/test preimages before the coherent packet."""
from common import *
source=(HERE/'common.py').read_text(encoding='utf-8')
source=source.replace("row=pin(path);return dict(name=Path(path).name,bytes=row['bytes'],sha256=row['sha256'])","path=Path(path)\n    with path.open('rb') as f:digest=hashlib.file_digest(f,'sha256').hexdigest()\n    return dict(name=path.name,bytes=path.stat().st_size,sha256=digest)")
with (HERE/'common_v2.py').open('x',encoding='utf-8',newline='\n') as f:f.write(source)
old=['BESHstatNG/src/ExcelUDFs/AgreementMethodUDFs.vb','BESHstatNG/src/ExcelUDFs/ContingencyTableUDFs.vb','BESHstatNG/src/RegModels/BinaryClassificationReporting.vb','BESHStatNG.Test/src/UDFs/UdfPreparation38AdapterTests.vb']
new=['BESHStat.Excel.Common/src/Results/UdfAgreementReportPreparation38.vb','BESHStat.Excel.Common/src/Results/UdfClassificationReportPreparation38.vb','BESHStat.Excel.Common/src/Analysis/UdfContingencyPreparation38.vb','BESHStat.Excel.Common.Tests/src/UdfContinuation9Tests.vb']
rows=[]
for path in old:
    target=HERE/'source-before'/(path+'.txt');target.parent.mkdir(parents=True,exist_ok=True)
    with target.open('xb') as f:f.write((ROOT/path).read_bytes())
    rows.append(dict(live=pin(ROOT/path),preimage=pin(target)))
for path in new:assert not (ROOT/path).exists()
save('implementation-plan-v1.json',dict(status='AUTHENTICATED_PREIMAGES_BEFORE_C9_IMPLEMENTATION',source_paths=old+new,source_before=rows,new_sources=[dict(path=p,entry_state='ABSENT') for p in new],entry=pin(HERE/'entry-v1.json'),tooling=dict(entry_common=pin(HERE/'common.py'),implementation_common=pin(HERE/'common_v2.py'),qualification='v2 makes basename tool pins valid for the owner Python/VS executable outside the repository; entry functions and prior entry module remain unchanged.'),scope='Shared report/data preparation only. Core algorithms, native acquisition/conversion/error materialization, exports and catches remain original. Whole-body closure conditional on full source/CIL/branch/inline proof.',original_queries=0))
for name in ['source_views.py','MethodMetadata-v11.cs','native-metadata-owners-v2.txt']:
    with (HERE/name).open('xb') as f:f.write((PREVIOUS/name).read_bytes())
save('tooling-carryforward-v1.json',dict(files=[dict(before=pin(PREVIOUS/n),after=pin(HERE/n)) for n in ['source_views.py','MethodMetadata-v11.cs','native-metadata-owners-v2.txt']],original_activity='PASSIVE_RETAINED_DLL_METADATA_ONLY_NO_INITIALIZERS_OR_METHOD_EXECUTION',original_queries=0))
print('C9 immediate preimages/absent-new sources and unchanged metadata tools recorded')
