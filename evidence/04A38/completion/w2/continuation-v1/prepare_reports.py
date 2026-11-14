"""Move report layouts; native error and flexible numeric readers stay adapters."""
from prepare_p1 import read,write,member
from common import *
paths=['BESHstatNG/src/ExcelUDFs/PlotDataUDFs.vb','BESHStat.Excel.Common/src/Results/UdfTypedReportPreparation38.vb']
if __name__=='__main__':
 rows=[]
 for p in paths:
  dst=HERE/'source-before/p1-reports'/(p+'.txt');dst.parent.mkdir(parents=True,exist_ok=True)
  with dst.open('xb') as f:f.write((ROOT/p).read_bytes())
  rows.append(dict(live=pin(ROOT/p),preimage=pin(dst)))
 save('source-before-p1-reports.json',dict(files=rows,plan='Calibration/ROC report preparation, existing Core bins, unchanged host interpretation',new_original_queries=0))
 host=read(paths[0]);common=read(paths[1]);added=[]
 for name in ['BuildCalibrationPointsTable','BuildRocPointsTable','BuildRocStatsTable']:
  old=member(host,name);body=old[old.index('\n')+1:]
  if name=='BuildCalibrationPointsTable':
   moved=old.replace('Private Function','Public Shared Function').replace(' As Object(,)\n',' As UdfReportProjection38\n',1).replace('regression.CalibrationBinSummary','Global.BESHStatNG.regression.CalibrationBinSummary')
   moved=moved.replace('ExcelError.ExcelErrorNA','CType(OutputCell.SpreadsheetError("#N/A"), Object)').replace('Return PrepareResultTableForUdf(out)','Return New UdfReportProjection38(New Global.BESHStatNG.ResultTableOutputModel(out), UdfReportPreparation38.NormalizeBlanks)')
   wrapper=old[:old.index('\n')]+'''\n            Return DirectCast(MaterializePortableReport38(Global.BESHStatNG.ExcelCommon.PlotDataUDFsReports38.BuildCalibrationPointsTable(rows)), Object(,))
        End Function'''
  else:
   moved=old.replace('Private Function','Public Shared Function')
   if name=='BuildRocPointsTable':
    moved=moved.replace('isLowerDirection As Boolean) As Object(,)','isLowerDirection As Boolean, tryFinite As UdfModelProfilePreparation38.FiniteDoubleReader) As Object(,)').replace('TryGetFiniteDoubleFlexible(thresholdObj, threshold)','tryFinite(thresholdObj, threshold)')
    wrapper=old[:old.index('\n')]+'''\n            Return Global.BESHStatNG.ExcelCommon.PlotDataUDFsReports38.BuildRocPointsTable(cutoffTable, isLowerDirection, AddressOf TryGetFiniteDoubleFlexible)
        End Function'''
   else:
    moved=moved.replace('Return PreparePValueRowsForUdf(out)','Return out')
    # Preserve the no-summary note's original lack of pvalue-row preparation.
    wrapper=old[:old.index('\n')]+'''\n            If summaryTable Is Nothing OrElse summaryTable.GetLength(0) < 2 Then
                Return Global.BESHStatNG.ExcelCommon.PlotDataUDFsReports38.BuildRocStatsTable(summaryTable)
            End If
            Return PreparePValueRowsForUdf(Global.BESHStatNG.ExcelCommon.PlotDataUDFsReports38.BuildRocStatsTable(summaryTable))
        End Function'''
   moved=moved.replace('Return BuildSimpleNoteTable(', 'Return UdfOutputTablesReports38.BuildSimpleNoteTable(')
   # These notes contain strings only. Current Core model retains their original 2x2 layout.
   for text in ['No ROC cut-off table was returned.','No ROC summary table was returned.']:
    moved=moved.replace('"'+text+'")','"'+text+'").ToOutputModel().Values')
  added.append(moved);host=host.replace(old,wrapper)
 common=common.replace('Namespace ExcelCommon','Namespace ExcelCommon\n    Public NotInheritable Class PlotDataUDFsReports38\n        Private Sub New()\n        End Sub\n\n'+'\n\n'.join(added)+'\n    End Class\n',1)
 write(paths[0],host);write(paths[1],common)
 print('P1 report source transitions prepared')
