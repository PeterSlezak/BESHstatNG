from common import *
assert json.loads((HERE/'entry-executable-authentication-v1.json').read_bytes())['status']=='AUTHENTICATED'
assert json.loads((HERE/'entry-queue-execution-v3.json').read_bytes())['exit_code']==0
paths=['BESHstatNG/src/ExcelUDFs/MultivariateAnalysisUDFs.vb','BESHstatNG/src/ExcelUDFs/PlotDataUDFs.vb',
    'BESHstatNG/src/ExcelUDFs/UDFdataImport/UdfDataImportPlot.vb','BESHStat.Excel.Common/src/Results/UdfTypedReportPreparation38.vb',
    'BESHStat.Excel.Common/src/Analysis/UdfPlotDataPreparation38.vb','BESHStatNG.Test/src/UDFs/UdfPreparation38AdapterTests.vb',
    'BESHStat.Excel.Common.Tests/src/UdfContinuation13Tests.vb']
rows=[];new=[]
for name in paths:
    source=ROOT/name
    if source.is_file():
        target=HERE/'source-before'/(name+'.txt')
        if target.exists():assert target.read_bytes()==source.read_bytes()
        else:
            target.parent.mkdir(parents=True,exist_ok=True)
            with target.open('xb') as stream:stream.write(source.read_bytes())
        rows.append(dict(live=pin(source),preimage=pin(target)))
    else:new.append(dict(path=name,entry_state='ABSENT'))
save('implementation-plan-v1.json',dict(status='AUTHENTICATED_BEFORE_C13_PRODUCTION_EDITS',source_paths=paths,source_before=rows,new_sources=new,
    queue=pin(HERE/'entry-queue-receipt-v1.json'),entry=pin(HERE/'v12-independent-entry-reconciliation-v1.json'),
    changes=['Move DA wrapped-table scan and ordinal group lookup into existing Common report owners; retain FIT/model/cache, host coercion, invalid-group/source throws, materialization and exception logging.',
        'Move histogram parts, complete KM step layout and nonempty homogeneous table stacking into existing Common PlotData reports. Keep actual Windows histogram/overlay engine delegates; preserve both native KM cast lambdas as separate count/fill policy arguments with full source/CIL slot proof.',
        'Move already-acquired ROC row classification and survival-record preparation into a typed Common preparation class with explicit native numeric/status/text/normalization delegates. Keep acquisition, header alignment, positive-label resolution, ByRef reset/partial outputs and exception policies native.',
        'Add focused Common and actual shipping adapter tests; retain every previous assertion and source preimage. Full Debug once only after freeze; Release full/selected unit suites NOT_RUN_OWNER_PREFERENCE.'],
    potential_module_support=['MultivariateAnalysisUDFs','PlotDataUDFs'],module_support_rule='Only close if all original/current child bodies, support declarations, callbacks, fields and complete compiled initializers/owner contexts actually reconcile. Otherwise OPEN.',
    repeated_callback_rule='No generic ordinal fallback. Exact old parent source, actual original/current ldftn argument positions and explicit count/fill composition must prove each canonical KM callback separately.',
    restrictions='No statistical formula/Core engine, framework/export/project ABI, scientific/native/manual work, original execution or reference capture.',original_queries=0))
print('Seven planned paths sealed; five existing preimages and two verified absent Common sources')
