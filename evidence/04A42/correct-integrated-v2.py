from common42 import *
import re
lock();freeze=load(HERE/'matrix/source-freeze-v1.json');copies=[]
for r in [x['after'] for x in freeze['source_delta']]+freeze['new_tests']:
    exact(r);dst=PRIVATE/'failed-freeze-v1-source'/r['path'];dst.parent.mkdir(parents=True,exist_ok=True);dst.write_bytes((ROOT/r['path']).read_bytes());copies.append(dict(original=r,protected=pin(dst)))
save('matrix/failed-freeze-v1-disposition.json',dict(status='FAILED_SUPERSEDED_BY_CORRECTED_SOURCE_FREEZE;NOT_READY_THIS_ATTEMPT',source=pin(HERE/'matrix/source-freeze-v1.json'),source_copies=copies,Debug=pin(HERE/'build-and-tests/Debug-executed-v1.json'),Release=pin(HERE/'build-and-tests/Release-executed-v1.json'),new_failures=['Continuation14PsmUnknownTableDoesNotReadMissingHandleState','Continuation13ShippingBodiesBindActualPreparedOwners:FindWrappedDiscriminantTable'],reason='Integrated existing adapter tests exposed eager missing-handle read and changed direct prepared-owner dependency. Correct only new extraction;all tests/assertions/history unchanged. New final build/test campaigns justified by demonstrated software regressions.',gates=GATES))
def write(p,s):(ROOT/p).write_bytes(s.replace('\r\n','\n').replace('\n','\r\n').encode())
p='BESHstatNG/src/ExcelUDFs/PropensityScoreUDFs.vb';s=(ROOT/p).read_text(encoding='utf-8-sig');needle='            Return Global.BESHStatNG.ExcelCommon.UdfPsmDataPreparation38.BuildNamedTable(h.Result'
assert s.count(needle)==1;s=s.replace(needle,'''            Dim kind As Global.BESHStatNG.ExcelCommon.PsmUdfTableKind38 = Global.BESHStatNG.ExcelCommon.UdfPsmDataPreparation38.ResolveTableKindPrepared(requested)
            If kind = Global.BESHStatNG.ExcelCommon.PsmUdfTableKind38.Unknown Then Return PsmResult.EmptyTable("Unknown PSM table: " & If(requested, String.Empty))
            Return Global.BESHStatNG.ExcelCommon.UdfPsmDataPreparation38.BuildNamedTable(h.Result''');write(p,s)
p='BESHstatNG/src/ExcelUDFs/MultivariateAnalysisUDFs.vb';s=(ROOT/p).read_text(encoding='utf-8-sig');old=(PRIVATE/'source-preimages'/p).read_text(encoding='utf-8-sig')
def get(text,name):
    a=text.index('        Private Function '+name+'(');b=text.index('        End Function',a)+len('        End Function');return text[a:b]
s=s.replace(get(s,'FindWrappedDiscriminantTable'),get(old,'FindWrappedDiscriminantTable'));write(p,s)
p='BESHStat.Excel.Common/src/Results/UdfMultivariateReportPreparation38.vb';s=(ROOT/p).read_text(encoding='utf-8-sig');a=s.index('        Public Shared Function FindDiscriminantTable(');b=s.index('        End Function',a)+len('        End Function');s=s[:a]+s[b:];write(p,s)
p='BESHStat.Excel.Common.Tests/src/UdfMultivariate42Tests.vb';s=(ROOT/p).read_text(encoding='utf-8-sig');s=s.replace('UdfMultivariateReportPreparation38.FindDiscriminantTable(model, "not present", False,','MultivariateAnalysisUDFsReports38.FindWrappedDiscriminantTablePrepared(model.wrapResults(), "not present", False,');s='\n'.join(line for line in s.splitlines() if 'FindDiscriminantTable(Nothing' not in line)+'\n';write(p,s)
print('Integrated regressions corrected;failed exact source/test receipts preserved;existing tests unchanged',flush=True)
