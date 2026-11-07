Option Explicit On
Option Strict On
Option Infer On
Imports System
Imports System.Linq
Imports System.Globalization
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports BESHStatNG
Imports BESHStatNG.ExcelCommon
Imports BESHStatNG.StatisticalProcessControl
Imports Legacy36
<TestClass> Public Class Graphics36PresentationTests
Public Shared Function Snapshot(plan As SpcPanelPresentation36) As Object()
Return plan.Series.Select(Function(v) CType(New Object(){v.Role.ToString(),v.Name,v.Values.Select(Function(x) If(x.HasValue,CType(x.Value,Object),Nothing)).ToArray(),v.LastValueOrdinal,v.ShowLimitLabel,v.HideLegend},Object)).ToArray()
End Function
<DataTestMethod><DataRow("")><DataRow("sk-SK")>
Public Sub AllFrozenSpcOptionsOrderAdmissionAndLastLabels(culture As String)
Dim saved=CultureInfo.CurrentCulture
Try
CultureInfo.CurrentCulture=CultureInfo.GetCultureInfo(culture)
For i=0 To 191
Dim p As SpcPanelSeriesProjection=Nothing
Try
p=OriginalPresentation36V2.Panel(i)
Catch ex As ArgumentException
Assert.AreEqual("points",ex.ParamName)
Assert.IsFalse(Cases36.Points(i).Any(Function(v) v IsNot Nothing),"Frozen Core empty-panel rejection")
Continue For
End Try
Dim spec=OriginalPresentation36V2.Specifications(i),plan As New SpcPanelPresentation36(p,spec,i Mod 4,(i And 4)<>0,(i And 8)<>0,(i And 16)<>0,(i And 32)<>0,(i And 64)<>0)
Dim expected=OriginalPresentation36V2.Series(p,spec,i Mod 4,(i And 4)<>0,(i And 8)<>0,(i And 16)<>0,(i And 32)<>0,(i And 64)<>0)
Assert.AreEqual(Cases36.Snapshot(expected),Cases36.Snapshot(Snapshot(plan)),"profile " & i)
Assert.AreEqual(i Mod 4=2 OrElse i Mod 4=3,plan.IncludesBands)
Dim before=Cases36.Snapshot(Snapshot(plan))
For Each plotted As SpcSeriesIntent36 In plan.Series
Dim values=plotted.Values
If values.Length>0 Then values(0)=99
Next
Assert.AreEqual(before,Cases36.Snapshot(Snapshot(plan)))
Assert.ThrowsException(Of NotSupportedException)(Sub() DirectCast(plan.Series,Collections.Generic.IList(Of SpcSeriesIntent36)).Clear())
Next
Finally
CultureInfo.CurrentCulture=saved
End Try
End Sub
<DataTestMethod><DataRow(0)><DataRow(1)><DataRow(2)><DataRow(3)><DataRow(4)><DataRow(5)><DataRow(6)><DataRow(7)><DataRow(8)><DataRow(9)><DataRow(10)><DataRow(11)><DataRow(12)><DataRow(13)><DataRow(14)>
Public Sub ExistingUnivariateEngineFamiliesFeedPresentation(chart As Integer)
Dim result=SpcEngine.Fit(Spc21Profile.Request(chart,True))
For Each useSequenceFlag As Boolean In New Boolean(){False,True}
Dim projection=SpcUnivariateChartProjectionPolicy.Project(result,useSequenceFlag)
For Each projectedPanel As SpcPanelSeriesProjection In projection.Panels
CheckPanel(projectedPanel,result.Request.SpecificationLimits)
Next:Next
End Sub
Public Shared Sub CheckPanel(panel As SpcPanelSeriesProjection,spec As SpcSpecificationLimits)
For mode=0 To 3
Dim before=Spc23Fixture.Hash(panel)
Dim p As New SpcPanelPresentation36(panel,spec,mode,True,True,True,True,False)
Assert.AreEqual(Cases36.Snapshot(OriginalPresentation36V2.Series(panel,spec,mode,True,True,True,True,False)),Cases36.Snapshot(Snapshot(p)))
Dim legacyOptions As New Spc22LegacyRenderer.SpcControlChartAppearanceOptions With {.ShowSpecificationLimits=True,.ShowTargetLine=True}
Dim fit As New SpcFitResult(Spc22Fixture.Request(),New SpcPanelResult(){panel.Panel})
Dim oldAxis=Spc22LegacyRenderer.BuildSharedAxisContext(fit,legacyOptions)
Dim oldBounds=Spc22LegacyRenderer.ComputeYAxisBounds(panel.Panel,Spc22LegacyRenderer.BuildPanelChartData(panel.Panel,oldAxis),spec,legacyOptions)
Dim bounds=SpcPanelPresentation36.AxisBounds(panel,spec,True,True)
Assert.AreEqual(BitConverter.DoubleToInt64Bits(oldBounds.Minimum),BitConverter.DoubleToInt64Bits(bounds.Min))
Assert.AreEqual(BitConverter.DoubleToInt64Bits(oldBounds.Maximum),BitConverter.DoubleToInt64Bits(bounds.Max))
Assert.AreEqual(before,Spc23Fixture.Hash(panel),"No engine/result/point mutation")
Next
End Sub
<DataTestMethod><DataRow(0)><DataRow(1)><DataRow(2)><DataRow(3)><DataRow(4)><DataRow(5)>
Public Sub ExistingMultivariateEnginesUseSameSharedPresentation(index As Integer)
Dim input=Spc24Fixture.Input(index),request=Spc23Fixture.Prepare(input,True),result=SpcMultivariate.Fit(request)
Dim report As New SpcMultivariateReportProjection(result)
Dim payload=SpcMultivariateChartProjectionPolicy.Prepare(report,New SpcMultivariateChartIntent("", "",report.Family.Caption))
For Each projectedPanel As SpcPanelSeriesProjection In payload.ProjectSeries(index Mod 2=0).Panels:CheckPanel(projectedPanel,payload.RenderingResult.Request.SpecificationLimits):Next
End Sub
<DataTestMethod><DataRow("")><DataRow("sk-SK")>
Public Sub CaptionsRemainLateAndLocalized(culture As String)
Dim saved=CultureInfo.CurrentCulture
Try
CultureInfo.CurrentCulture=CultureInfo.GetCultureInfo(culture)
Dim d=Cases36.KM(19)
For i=0 To d.GroupLabels.Length-1
Assert.AreEqual(d.GroupLabels(i),SurvivalChartPresentation36.SeriesName(d,i,KaplanMeierSeriesRole36.Survival))
Assert.AreEqual("Censored " & d.GroupLabels(i),SurvivalChartPresentation36.SeriesName(d,i,KaplanMeierSeriesRole36.Censor))
Assert.AreEqual($"{100.0*(1.0-d.Alpha):0.##}% CI " & d.GroupLabels(i),SurvivalChartPresentation36.SeriesName(d,i,KaplanMeierSeriesRole36.UpperConfidence))
Next
d.GroupLabels(0)="changed":d.Alpha=0.123
Assert.AreEqual($"{100.0*(1.0-d.Alpha):0.##}% CI changed",SurvivalChartPresentation36.SeriesName(d,0,KaplanMeierSeriesRole36.LowerConfidence))
Assert.AreEqual("Time ()",SurvivalChartPresentation36.KmTimeTitle(Nothing))
Assert.IsTrue(SurvivalChartPresentation36.DeleteLegend(1,True)):Assert.IsFalse(SurvivalChartPresentation36.DeleteLegend(2,True))
Finally
CultureInfo.CurrentCulture=saved
End Try
End Sub
<TestMethod> Public Sub ExistingResidualBlocksRetainOrderAndNoUnsupportedJoin()
Dim intent=Survival13Requests.Cox("Cox|Breslow|base"),ids=intent.Input.Time.RowIds,times=intent.Input.Time.Values
Dim score As Object(,)={{"Score Residuals",Nothing},{"Row ID","x1"},{ids(1),2.5},{ids(0),Double.NaN}}
Dim scalar As Object(,)={{Nothing},{"Martingale Residuals"},{-0.0},{2.0}}
Dim nested As Object()={New Double(){1,-0.0}}
scalar(0,0)=nested
Dim old=CoxReportPolicy.Residuals(intent,New Collections.Generic.List(Of Object(,)) From {score,scalar}),spec=old.ResidualPlotAvailability()
Assert.IsFalse(spec.NativeControlAvailable):Assert.AreEqual(2,spec.Blocks.Count)
Dim a=spec.Blocks(0):Assert.AreEqual("AVAILABLE_DATA_NATIVE_CONTROL_DISABLED",a.PlotAvailability)
Assert.AreEqual("NOT_SUPPORTED_BY_EXISTING_SOURCE",spec.Blocks(1).PlotAvailability)
Dim s=a.Series.Single()
CollectionAssert.AreEqual(New Integer(){ids(1),ids(0)},s.RowIds)
CollectionAssert.AreEqual(New Integer(){1,0},s.ObservationOrdinals)
CollectionAssert.AreEqual(New Double(){times(1),times(0)},s.XValues)
score(2,1)=99:scalar(2,0)=88
Dim raw=a.LegacyValues:raw(2,1)=12
Dim numeric=a.NumericBody:numeric(0,1)=24
Dim x=s.XValues:x(0)=77
Dim y=s.YValues:y(0)=77
Dim rows=s.RowIds:rows(0)=777
Dim predictors=spec.Predictors:predictors(0,0)=777
DirectCast(nested(0),Double())(0)=99
Dim detached=DirectCast(spec.Blocks(1).LegacyValues(0,0),Object())
DirectCast(detached(0),Double())(0)=88
Assert.AreEqual(1.0,DirectCast(DirectCast(spec.Blocks(1).LegacyValues(0,0),Object())(0),Double())(0))
Assert.AreEqual(2.5,s.YValues(0)):Assert.AreEqual(ids(1),s.RowIds(0)):Assert.AreEqual(2.5,CDbl(old.LegacyBlocks()(0)(2,1)))
Assert.AreEqual(Long.MinValue,BitConverter.DoubleToInt64Bits(CDbl(spec.Blocks(1).LegacyValues(2,0))))
End Sub
<TestMethod> Public Sub ActualCoxResidualBlocksAndFirstStratumCurvesRemainAccepted()
Dim intent=Survival13Requests.Cox("Cox|Breslow|strata"),model=intent.CreateCoreModel()
model.Fit(intent.Options.Ties)
Dim blocks=model.wrapResiduals(),data=CoxReportPolicy.Residuals(intent,blocks),before=Cases36.Snapshot(blocks.ToArray()),payload=data.ResidualPlotAvailability()
Assert.AreEqual(blocks.Count,payload.Blocks.Count)
For i=0 To blocks.Count-1
Assert.AreEqual(Cases36.Snapshot(blocks(i)),Cases36.Snapshot(payload.Blocks(i).LegacyValues))
For Each residualSeries As CoxResidualSeries36 In payload.Blocks(i).Series
For row=0 To residualSeries.RowIds.Length-1
Assert.AreEqual(data.RowIds(residualSeries.ObservationOrdinals(row)),residualSeries.RowIds(row))
Assert.AreEqual(BitConverter.DoubleToInt64Bits(data.Time(residualSeries.ObservationOrdinals(row))),BitConverter.DoubleToInt64Bits(residualSeries.XValues(row)))
Next:Next:Next
Assert.AreEqual(before,Cases36.Snapshot(data.LegacyBlocks().ToArray()))
Assert.IsFalse(payload.NativeControlAvailable)
End Sub
End Class
