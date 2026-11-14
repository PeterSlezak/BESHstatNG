from common import *
assert (HERE/'entry.json').is_file() and (HERE/'python-nonpass-investigation.json').is_file()
paths=['BESHstatNG/src/ExcelUDFs/UDFdataImport/UdfDataImportRangeReaders.vb',
 'BESHstatNG/src/ExcelUDFs/UDFdataImport/UdfDataImport.vb',
 'BESHstatNG/src/ExcelUDFs/UDFdataImport/UdfDataImportOptions.vb',
 'BESHstatNG/src/ExcelUDFs/ParametricUDFs.vb','BESHstatNG/src/ExcelUDFs/NonparametricUDFs.vb',
 'BESHStat.Excel.Common/src/Analysis/UdfNumericImportPreparation38.vb',
 'BESHStat.Excel.Common.Tests/src/UdfPreparation38Tests.vb',
 'BESHStatNG.Test/src/UDFs/UdfPreparation38AdapterTests.vb']
rows=[]
for path in paths:
 p=ROOT/path;target=HERE/'source-before'/(path+'.txt');target.parent.mkdir(parents=True,exist_ok=True)
 with target.open('xb') as f:f.write(p.read_bytes())
 rows.append(dict(live=pin(p),preimage=pin(target)))
save('implementation-plan.json',dict(status='PREIMAGES_BEFORE_SOURCE_EDITS',source_before=rows,
 new_sources=['BESHStat.Excel.Common/src/Results/UdfInferencePreparation38.vb'],
 groups=['Ordered independent/paired/complete-matrix/single-column and probability-vector dependencies',
 'Complete repeated/nested ANOVA and unpaired/paired/NI/equivalence t-test parents',
 'Complete rank-test families and applicable shared output parents/support modules'],
 source_of_queue=pin(PREVIOUS/'remaining-work-queue-v3.json'),original_queries=0))
review=json.loads((PREVIOUS/'body-contracts-v3.json').read_bytes())['contracts']
history=json.loads((PREVIOUS/'historical-ownership-v3.json').read_bytes())['members'];byid={r['canonical_original_id']:r for r in history}
anova=[]
for name in ['ANOVA1','ANOVA1_WELCH','ANOVA1_MCP']:
 r=next(r for r in review if r['name']==name)
 assert all(byid[i]['proof'] for i in r['dependency_ids'])
 assert all(pin(ROOT/p['source_pin']['path'])==p['source_pin'] for p in r['portable_bodies'])
 anova.append(dict(name=name,original_id=r['original_id'],source_contract=r,
    dependencies=[dict(id=i,proof=byid[i]['proof']) for i in r['dependency_ids']],
    compiled_proof=pin(PREVIOUS/'production-consumer-edges-v3.json'),
    engine_result_settings_full_context=[pin(PREVIOUS/('dependency-metadata-v9/current-'+c+'.json')) for c in ['Debug','Release']],
    original_context=[pin(PREVIOUS/('dependency-metadata-v7/original-'+c+'.json')) for c in ['Debug','Release']],
    reassessment='SUPPORTED: complete ordered source composition and native real catch/logging, import/names/alpha/format/stack dependencies individually supported, unchanged entire engine/result/settings context and exact frozen fixtures. No reopen warranted by this review.'))
save('v5-anova-parent-reassessment.json',dict(parents=anova,independent_reader=pin(HERE/'v5-ownership-recomputed.json'),reopened=[],original_queries=0))
print('Eight exact preimages retained; v5 ANOVA parent dependencies reassessed')
