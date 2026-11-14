"""Capture every planned immediate preimage before authorized implementation."""
from common import *
assert json.loads((HERE/'entry-executable-authentication-v1.json').read_bytes())['status']=='AUTHENTICATED'
paths=['BESHstatNG/src/ExcelUDFs/MultivariateAnalysisUDFs.vb',
    'BESHstatNG/src/ExcelUDFs/UDFdataImport/UdfDataImportMultivariate.vb',
    'BESHStat.Excel.Common/src/Results/UdfMultivariateReportPreparation38.vb',
    'BESHStat.Excel.Common/src/Analysis/UdfModelImportPreparation38.vb',
    'BESHStatNG.Test/src/UDFs/UdfPreparation38AdapterTests.vb',
    'BESHStat.Excel.Common.Tests/src/UdfContinuation12Tests.vb']
rows=[];new=[]
for name in paths:
    path=ROOT/name
    if path.is_file():
        target=HERE/'source-before'/(name+'.txt')
        if target.exists():assert target.read_bytes()==path.read_bytes()
        else:
            target.parent.mkdir(parents=True,exist_ok=True)
            with target.open('xb') as stream:stream.write(path.read_bytes())
        rows.append(dict(live=pin(path),preimage=pin(target)))
    else:new.append(dict(path=name,entry_state='ABSENT'))
save('implementation-plan-v1.json',dict(status='AUTHENTICATED_BEFORE_C12_PRODUCTION_EDITS',source_paths=paths,source_before=rows,new_sources=new,
    first_members=json.loads((PREVIOUS/'remaining-work-queue-v1.json').read_bytes())['first_members'],queue=pin(HERE/'entry-queue-receipt-v1.json'),
    changes=['Move MCA summary layout to the existing Common report preparation class, retaining eigen/header access and host materialization.',
        'Move DA summary rows0-13 and final label into Common; retain the late native validation-mode select, formatting, header coercion and materialization.',
        'Move categorical prepared array fill and string/numeric prepared vector iteration into the existing Common model-import preparation class. Preserve native acquisition, missing/default/coercion policies and delimited-string paths; bind existing host policies with AddressOf.',
        'Add focused Common and shipping adapter boundary tests; preserve all old assertions byte-for-byte.'],
    restrictions='No Core formulas, model FIT/cache ownership, public exports/framework/project metadata, scientific/native/manual validation, original execution or supplemental reference capture.',original_queries=0))
print('Six planned source/test paths; five existing immediate preimages and one absent new test source sealed')
