Option Explicit On
Option Strict On
Option Infer On
Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Reflection
Imports BESHStatNG.ExcelCommon
Imports BESHStatNG.StatisticalProcessControl
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass>
Public Class Spc24AdapterTests
    Private Shared Function Calls(method As MethodInfo) As List(Of MethodBase)
        Dim bytes = method.GetMethodBody().GetILAsByteArray(), values As New List(Of MethodBase)
        For offset = 0 To bytes.Length - 5
            If bytes(offset) <> &H28 AndAlso bytes(offset) <> &H6F AndAlso bytes(offset) <> &H73 Then Continue For
            Try
                values.Add(method.Module.ResolveMethod(BitConverter.ToInt32(bytes, offset + 1)))
            Catch ex As ArgumentException
                'Only actual metadata-token instructions can resolve.
            End Try
        Next
        Return values
    End Function
    <DataTestMethod><DataRow("BuildSummaryTables")><DataRow("BuildModelTables")><DataRow("BuildChartDataTable")><DataRow("BuildSignalsTable")><DataRow("BuildDiagnosticTables")><DataRow("BuildAuditTables")>
    Public Sub CompiledShippingReportWrappersConsumePortableBuilders(name As String)
        Dim method = GetType(BESHStatNG.Ui22MultivariateControlCharts).GetMethod(name, BindingFlags.Instance Or BindingFlags.NonPublic)
        Assert.IsNotNull(method)
        Assert.IsTrue(Calls(method).Any(Function(m) m.DeclaringType = GetType(SpcMultivariateReportProjection) AndAlso m.Name = name))
    End Sub
    <TestMethod>
    Public Sub CompiledShippingWorkbookUsesOneProjectionAndFourArgumentRendererSeam()
        Dim owner = GetType(BESHStatNG.Ui22MultivariateControlCharts)
        Dim writer = Calls(owner.GetMethod("WriteResultsToNewWorkbook", BindingFlags.Instance Or BindingFlags.NonPublic))
        Assert.AreEqual(1, writer.Where(Function(m) m.DeclaringType = GetType(SpcMultivariateReportProjection) AndAlso m.IsConstructor).Count())
        Assert.IsTrue(writer.Any(Function(m) m.DeclaringType = GetType(SpcMultivariateChartPayload) AndAlso m.Name = "ProjectSeries"))
        Assert.IsTrue(writer.Any(Function(m) m.DeclaringType = GetType(BESHStatNG.graphics.SpcControlChartExcel) AndAlso m.Name = "AddCharts" AndAlso m.GetParameters().Length = 4))
        Dim chart = Calls(owner.GetMethod("BuildChartRenderingResult", BindingFlags.Instance Or BindingFlags.NonPublic))
        Assert.IsTrue(chart.Any(Function(m) m.DeclaringType = GetType(SpcMultivariateChartProjectionPolicy) AndAlso m.Name = "Prepare"))
    End Sub
    <DataTestMethod><DataRow(0)><DataRow(1)><DataRow(2)><DataRow(3)><DataRow(4)><DataRow(5)>
    Public Sub ActualNativeMarshallingConsumesPreparedCategoriesAndTenChannelsWithoutChartObjects(id As Integer)
        Dim input = Spc24Fixture.Input(id), fit = SpcMultivariate.Fit(Spc23Fixture.Prepare(input, True))
        Dim payload = SpcMultivariateChartProjectionPolicy.Prepare(New SpcMultivariateReportProjection(fit), New SpcMultivariateChartIntent("", " axis ", Spc24LegacyProjection.ChartTypeText(input.Chart)))
        Dim flags = BindingFlags.Static Or BindingFlags.NonPublic, renderer = GetType(BESHStatNG.graphics.SpcControlChartExcel)
        For Each useSequence As Boolean In {False, True}
            Dim projection = payload.ProjectSeries(useSequence)
            Dim actual = DirectCast(renderer.GetMethod("MarshalChartCategories", flags).Invoke(Nothing, New Object() {projection}), Object())
            Dim axis = Spc22LegacyRenderer.BuildSharedAxisContext(payload.RenderingResult, New Spc22LegacyRenderer.SpcControlChartAppearanceOptions With {.UseSequenceValuesForHorizontalAxis = useSequence})
            Assert.AreEqual(Spc23Fixture.Hash(axis.Categories), Spc23Fixture.Hash(actual))
            For Each panel In projection.Panels
                For Each channel In panel.Channels
                    Dim values = DirectCast(renderer.GetMethod("MarshalChartValues", flags).Invoke(Nothing, New Object() {channel}), Object())
                    For i = 0 To values.Length - 1
                        Assert.AreEqual(channel(i).HasValue, values(i) IsNot Nothing)
                        If channel(i).HasValue Then Assert.AreEqual(Spc23Fixture.Bits(channel(i).Value), Spc23Fixture.Bits(CDbl(values(i))))
                    Next
                Next
            Next
        Next
    End Sub
End Class
