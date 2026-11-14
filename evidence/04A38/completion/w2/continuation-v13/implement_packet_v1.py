"""Exact current prepared body movement; no original source execution or capture."""
from common import *
plan=json.loads((HERE/'implementation-plan-v1.json').read_bytes())
for row in plan['source_before']:assert pin(ROOT/row['live']['path'])==row['live']
for row in plan['new_sources']:assert not (ROOT/row['path']).exists()
multi='BESHstatNG/src/ExcelUDFs/MultivariateAnalysisUDFs.vb';plot='BESHstatNG/src/ExcelUDFs/PlotDataUDFs.vb'
imports='BESHstatNG/src/ExcelUDFs/UDFdataImport/UdfDataImportPlot.vb';report='BESHStat.Excel.Common/src/Results/UdfTypedReportPreparation38.vb'
prepare='BESHStat.Excel.Common/src/Analysis/UdfPlotDataPreparation38.vb'
read=lambda p:(ROOT/p).read_bytes().decode('utf-8-sig').replace('\r\n','\n')
texts={p:read(p) for p in [multi,plot,imports,report]};phases=[]
def segment(source,parent,start,end):
    a=source.index(start,source.index(parent));b=source.index(end,a);return source[a:b]
def move(path,parent,start,end,signature,bindings,call,owner):
    source=texts[path];before=segment(source,parent,start,end);after=before
    for a,b in bindings.items():after=after.replace(a,b)
    method='        Public Shared Function '+signature+'\n'+after+'        End Function\n'
    if owner=='prepared':prepared_methods.append(method)
    else:
        marker='    Public NotInheritable Class '+owner+'\n';at=texts[report].index(marker);endclass=texts[report].index('    End Class',at)
        texts[report]=texts[report][:endclass]+method+'\n'+texts[report][endclass:]
    texts[path]=source.replace(before,call,1)
    phases.append(dict(path=path,parent=parent,owner=owner,portable_signature=signature,original_segment=before,portable_segment=after,bindings=bindings,actual_native_call=call))
prepared_methods=[]
# DA throws remain in the original Windows parents. The portable selectors do
# not normalize Excel inputs or replace missing/sentinel cells.
move(multi,'Private Function FindWrappedDiscriminantTable(', '            For Each t As ResultTable In tables','        End Function',
    'FindWrappedDiscriminantTablePrepared(tables As IEnumerable(Of ResultTable), title As String, startsWith As Boolean, toText As Func(Of Object, String)) As Object(,)',
    {'CellToTrimmedText':'toText'},
    '            Return Global.BESHStatNG.ExcelCommon.MultivariateAnalysisUDFsReports38.FindWrappedDiscriminantTablePrepared(tables, title, startsWith, AddressOf CellToTrimmedText)\n',
    'MultivariateAnalysisUDFsReports38')
source=texts[multi];old=segment(source,'Public Function DA_COVARIANCE(', '                Dim gs As Multivariate.DiscriminantGroupStatistics = Nothing','                If gs Is Nothing Then Throw')
body=old.replace('h.Model.GroupStatistics','statistics')+'                Return gs\n'
signature='FindDiscriminantGroupPrepared(statistics As IEnumerable(Of Multivariate.DiscriminantGroupStatistics), requested As String) As Multivariate.DiscriminantGroupStatistics'
method='        Public Shared Function '+signature+'\n'+body+'        End Function\n'
at=texts[report].index('    Public NotInheritable Class MultivariateAnalysisUDFsReports38\n');stop=texts[report].index('    End Class',at)
texts[report]=texts[report][:stop]+method+'\n'+texts[report][stop:]
call='                Dim gs As Multivariate.DiscriminantGroupStatistics = Global.BESHStatNG.ExcelCommon.MultivariateAnalysisUDFsReports38.FindDiscriminantGroupPrepared(h.Model.GroupStatistics, requested)\n'
texts[multi]=source.replace(old,call,1);phases.append(dict(path=multi,parent='Public Function DA_COVARIANCE(',owner='MultivariateAnalysisUDFsReports38',portable_signature=signature,original_segment=old,portable_segment=body,bindings={'h.Model.GroupStatistics':'statistics'},actual_native_call=call))
for parent,name,overlay in [('HIST_BINS','BuildHistogramPartsPrepared',False),('HIST_NORMAL','BuildHistogramNormalPartsPrepared',True)]:
    source=texts[plot];old=segment(source,'Public Function '+parent+'(', '                Dim parts As New List(Of Object(,))()','                If parts.Count = 0 Then')
    bindings={'graphics.HistogramBinsComputation':'computeBins'}
    if overlay:bindings['graphics.GaussOverlayComputation']='computeOverlay'
    body=old
    for a,b in bindings.items():body=body.replace(a,b)
    body+='                Return parts\n'
    sig=name+'(groups()() As Double, names() As String, resolvedRule As String, computeBins As Func(Of Double(), String, Object(,))'+(', computeOverlay As Func(Of Double(), Double(), Double(,))' if overlay else '')+') As List(Of Object(,))'
    method='        Public Shared Function '+sig+'\n'+body+'        End Function\n'
    at=texts[report].index('    Public NotInheritable Class PlotDataUDFsReports38\n');stop=texts[report].index('    End Class',at)
    texts[report]=texts[report][:stop]+method+'\n'+texts[report][stop:]
    call='                Dim parts As List(Of Object(,)) = Global.BESHStatNG.ExcelCommon.PlotDataUDFsReports38.'+name+'(groups, names, resolvedRule, AddressOf graphics.HistogramBinsComputation'+(', AddressOf graphics.GaussOverlayComputation' if overlay else '')+')\n'
    texts[plot]=source.replace(old,call,1);phases.append(dict(path=plot,parent=parent,owner='PlotDataUDFsReports38',portable_signature=sig,original_segment=old,portable_segment=body,bindings=bindings,actual_native_call=call))
source=texts[plot];old=segment(source,'Private Function BuildKaplanMeierStepTable(', '            Dim totalRows As Integer = 1','        End Function')
callback='Function(x) CType(x, survival.SurvivalTableRecord)';assert old.count(callback)==2
body=old.replace('.Select('+callback+')','.Select(countRecord)',1).replace('.Select('+callback+')','.Select(fillRecord)',1).replace('Return BuildSimpleNoteTable(','Return noteTable(')
sig='BuildKaplanMeierStepTablePrepared(rawCurves As Object(), countRecord As Func(Of Object, survival.SurvivalTableRecord), fillRecord As Func(Of Object, survival.SurvivalTableRecord), noteTable As Func(Of String, String, Object(,))) As Object(,)'
method='        Public Shared Function '+sig+'\n'+body+'        End Function\n'
at=texts[report].index('    Public NotInheritable Class PlotDataUDFsReports38\n');stop=texts[report].index('    End Class',at)
texts[report]=texts[report][:stop]+method+'\n'+texts[report][stop:]
call='            Return Global.BESHStatNG.ExcelCommon.PlotDataUDFsReports38.BuildKaplanMeierStepTablePrepared(\n                rawCurves, '+callback+',\n                '+callback+', AddressOf BuildSimpleNoteTable)\n'
texts[plot]=source.replace(old,call,1);phases.append(dict(path=plot,parent='BuildKaplanMeierStepTable',owner='PlotDataUDFsReports38',portable_signature=sig,original_segment=old,portable_segment=body,bindings={'.Select('+callback+') occurrences':['.Select(countRecord)','.Select(fillRecord)'],'BuildSimpleNoteTable':'noteTable'},actual_native_call=call,callback_contexts=['first original count-loop projection -> countRecord parameter','second original fill-loop projection -> fillRecord parameter']))
move(plot,'Private Function StackHomogeneousTables(', '            Dim cols As Integer = tables(0).GetLength(1)','        End Function',
    'StackHomogeneousTablesPrepared(tables As List(Of Object(,))) As Object(,)',{},
    '            Return Global.BESHStatNG.ExcelCommon.PlotDataUDFsReports38.StackHomogeneousTablesPrepared(tables)\n','PlotDataUDFsReports38')
move(imports,'Friend Function TryGetRocInput(', '        Dim pos As New List(Of Double)()','    End Function',
    'CompleteRocInputPrepared(markerCol As Object(,), statusCol As Object(,), positiveLabel As String, isLowerDirection As Boolean, finiteDouble As UdfPlotFiniteDoubleReader38, status01 As UdfPlotStatusReader38, toText As Func(Of Object, String), normalize As Func(Of String, String), ByRef rocInput()() As Double) As Boolean',
    {'Global.BESHStatNG.WorksheetFunctions.ExcelArgNumeric.TryGetFiniteDoubleFlexible':'finiteDouble','Global.BESHStatNG.WorksheetFunctions.ExcelArgNumeric.TryGetStatus01Flexible':'status01','Global.BESHStatNG.WorksheetFunctions.ExcelArgReaders.CellToTrimmedText':'toText','NormalizeOptionKey':'normalize'},
    '        Return Global.BESHStatNG.ExcelCommon.UdfPlotDataPreparation38.CompleteRocInputPrepared(markerCol, statusCol, positiveLabel, isLowerDirection,\n            AddressOf Global.BESHStatNG.WorksheetFunctions.ExcelArgNumeric.TryGetFiniteDoubleFlexible,\n            AddressOf Global.BESHStatNG.WorksheetFunctions.ExcelArgNumeric.TryGetStatus01Flexible,\n            AddressOf Global.BESHStatNG.WorksheetFunctions.ExcelArgReaders.CellToTrimmedText, AddressOf NormalizeOptionKey, rocInput)\n','prepared')
move(imports,'Friend Function TryGetSurvivalRecords(', '        Dim tList As New List(Of Double)()','    End Function',
    'CompleteSurvivalRecordsPrepared(timeCol As Object(,), statusCol As Object(,), groupCol As Object(,), hasGroup As Boolean, finiteDouble As UdfPlotFiniteDoubleReader38, status01 As UdfPlotStatusReader38, toText As Func(Of Object, String), ByRef records As List(Of survival.SurvivalRecord)) As Boolean',
    {'Global.BESHStatNG.WorksheetFunctions.ExcelArgNumeric.TryGetFiniteDoubleFlexible':'finiteDouble','Global.BESHStatNG.WorksheetFunctions.ExcelArgNumeric.TryGetStatus01Flexible':'status01','Global.BESHStatNG.WorksheetFunctions.ExcelArgReaders.CellToTrimmedText':'toText'},
    '        Return Global.BESHStatNG.ExcelCommon.UdfPlotDataPreparation38.CompleteSurvivalRecordsPrepared(timeCol, statusCol, groupCol, hasGroup,\n            AddressOf Global.BESHStatNG.WorksheetFunctions.ExcelArgNumeric.TryGetFiniteDoubleFlexible,\n            AddressOf Global.BESHStatNG.WorksheetFunctions.ExcelArgNumeric.TryGetStatus01Flexible,\n            AddressOf Global.BESHStatNG.WorksheetFunctions.ExcelArgReaders.CellToTrimmedText, records)\n','prepared')
texts[report]=texts[report].replace('Imports System\n','Imports System\nImports System.Collections\n',1)
prepared='Option Explicit On\nOption Strict On\n\nImports System\nImports System.Collections.Generic\nImports BESHStatNG\n\nNamespace ExcelCommon\n    Public Delegate Function UdfPlotFiniteDoubleReader38(cell As Object, ByRef value As Double) As Boolean\n    Public Delegate Function UdfPlotStatusReader38(cell As Object, ByRef value As Integer) As Boolean\n\n    Public NotInheritable Class UdfPlotDataPreparation38\n        Private Sub New()\n        End Sub\n\n'+('\n'.join(prepared_methods))+'    End Class\nEnd Namespace\n'
for path,value in texts.items():write_source(path,value)
with (ROOT/prepare).open('x',encoding='utf-8',newline='\n') as stream:stream.write(prepared)
save('implementation-source-composition-v1.json',dict(phases=phases,source_after=[pin(ROOT/p) for p in [multi,plot,imports,report,prepare]],
    scope='Eight exact prepared body movements, two typed native policy delegates, unchanged model engines. No closure until full source/ABI/CIL/callback/current compatibility proof. Both canonical KM callbacks remain source declarations in their original Windows parent with explicit count/fill parameter ownership.',original_queries=0))
print('Eight prepared body segments moved; native acquisition/cache/throws, exact KM callbacks and Core engines retained')
