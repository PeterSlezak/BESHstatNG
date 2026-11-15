from common43 import *
lock();assert load(HERE/'fixtures/pre-edit-current-main-v1.json')['real_fit_graphs']==36
edits=[]
for path,before,after,expected in [
('BESHstatNG/src/ExcelUDFs/ExcelArgModules.vb','Return Not String.IsNullOrWhiteSpace(key)','Return Global.BESHStatNG.ExcelCommon.UdfModelHandlePolicy.HasLookupKey(key)',1),
('BESHstatNG/src/ExcelUDFs/MultivariateAnalysisUDFs.vb','If String.IsNullOrWhiteSpace(key) Then Return False','If Not Global.BESHStatNG.ExcelCommon.UdfModelHandlePolicy.HasLookupKey(key) Then Return False',14),
('BESHstatNG/src/ExcelUDFs/PropensityScoreUDFs.vb','If String.IsNullOrWhiteSpace(key) OrElse String.Equals(key, "ALL", StringComparison.OrdinalIgnoreCase) Then','If Global.BESHStatNG.ExcelCommon.UdfModelHandlePolicy.IsClearAllRequest(key, "ALL") Then',1)]:
    p=ROOT/path;s=p.read_text(encoding='utf-8-sig');assert s.count(before)==expected
    original=pin(p);p.write_bytes(s.replace(before,after).replace('\n','\r\n').encode());edits.append(dict(before=original,after=pin(p),replacements=expected))
save('waves/adapter-source-transitions-v1.json',dict(files=edits,storage_ownership='ALL18_DICTIONARIES_UNCHANGED_WINDOWS',FIT_GUID_time_store_query_drop_clear_order='UNCHANGED',new_workbook_callbacks=0,gates=GATES))
print('Wave B only delegates key eligibility and cleanup predicate;all physical cache operations stay Windows',flush=True)
