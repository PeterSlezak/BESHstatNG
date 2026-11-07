Option Explicit On
Option Strict On
Imports System
Imports System.Linq
Imports System.Reflection
Imports System.Globalization
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports BESHStatNG
Imports Batch34SeriesFixtures
Imports Batch34CompletionFixtures
<TestClass>
Public Class Graphics34CompletionSeriesTests
    Private Shared Sub Equal(expected As Object, actual As Object)
        If expected Is Nothing Then
            Assert.IsNull(actual) : Return
        End If
        Assert.IsNotNull(actual)
        If TypeOf expected Is Double Then
            If Double.IsNaN(CDbl(expected)) Then
                Assert.IsTrue(Double.IsNaN(CDbl(actual)))
            Else
                Assert.AreEqual(BitConverter.DoubleToInt64Bits(CDbl(expected)), BitConverter.DoubleToInt64Bits(CDbl(actual)))
            End If
        ElseIf TypeOf expected Is String OrElse TypeOf expected Is Boolean OrElse TypeOf expected Is Integer Then
            Assert.AreEqual(expected, actual)
        ElseIf TypeOf expected Is Array Then
            Dim es = DirectCast(expected, Array).Cast(Of Object)().ToArray(), ac = DirectCast(actual, Array).Cast(Of Object)().ToArray()
            Assert.AreEqual(es.Length, ac.Length)
            For i As Integer = 0 To es.Length - 1 : Equal(es(i), ac(i)) : Next
        Else
            For Each prop In expected.GetType().GetProperties()
                Equal(prop.GetValue(expected, Nothing), actual.GetType().GetProperty(prop.Name).GetValue(actual, Nothing))
                Assert.IsNull(actual.GetType().GetProperty(prop.Name).GetSetMethod())
            Next
        End If
    End Sub
    <DataTestMethod><DataRow("Dumbbell", "")><DataRow("Dumbbell", "sk-SK")><DataRow("Ladder", "")><DataRow("Ladder", "sk-SK")><DataRow("Agreement", "")><DataRow("Agreement", "sk-SK")><DataRow("Comparison", "")><DataRow("Comparison", "sk-SK")><DataRow("Cdf", "")><DataRow("Cdf", "sk-SK")>
    Public Sub FrozenSeriesAndReferenceCoordinates(family As String, culture As String)
        Dim saved = CultureInfo.CurrentCulture
        Try
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(culture)
            Dim legacy = GetType(CompletionSeriesCases34).Assembly.GetType("Batch34SeriesFixtures.Legacy" & family & "Series34", True)
            Dim target = GetType(ExcelCommon.Chart34.CdfChartPreparation34).Assembly.GetType("BESHStatNG.ExcelCommon.Chart34." & family & "Series." & family & "SeriesPreparation34", True)
            For Each method In legacy.GetMethods(BindingFlags.Public Or BindingFlags.Static)
                For index As Integer = 0 To 159
                    Dim expectedError As String = Nothing, actualError As String = Nothing
                    Dim expected = Completion34Cases.Invoke(method, CompletionSeriesCases34.Arguments(method, index), expectedError)
                    Dim args = CompletionSeriesCases34.Arguments(method, index)
                    Dim actual = Completion34Cases.Invoke(target.GetMethod(method.Name), args, actualError)
                    Assert.AreEqual(expectedError, actualError, family & method.Name & index)
                    If expectedError <> "None" Then Continue For
                    Equal(expected, actual)
                    If actual Is Nothing Then Continue For
                    If TypeOf actual Is String OrElse actual.GetType().IsValueType Then Continue For
                    Dim frozen = Completion34Cases.Snapshot(actual)
                    For Each p In actual.GetType().GetProperties()
                        Dim obtained = p.GetValue(actual, Nothing)
                        If TypeOf obtained Is Double() AndAlso DirectCast(obtained, Double()).Length > 0 Then DirectCast(obtained, Double())(0) = 999
                        If TypeOf obtained Is Integer() AndAlso DirectCast(obtained, Integer()).Length > 0 Then DirectCast(obtained, Integer())(0) = 999
                        If TypeOf obtained Is String() AndAlso DirectCast(obtained, String()).Length > 0 Then DirectCast(obtained, String())(0) = "changed"
                    Next
                    For Each arg In args
                        If TypeOf arg Is Array AndAlso DirectCast(arg, Array).Length > 0 Then DirectCast(arg, Array).SetValue(Nothing, 0)
                    Next
                    Assert.AreEqual(frozen, Completion34Cases.Snapshot(actual))
                Next
            Next
        Finally
            CultureInfo.CurrentCulture = saved
        End Try
    End Sub
    <TestMethod>
    Public Sub ConstructorCopiesAndOrdinalIdentity()
        Dim x As Double() = {1, 2}, y As Double() = {3, 4}, amount As Double() = {2, 2}, labels As String() = {"same", "same"}, ordinals As Integer() = {8, 2}
        Dim actual As New ExcelCommon.Chart34.DumbbellSeries.SeriesPayload34(x, y, amount, labels, ordinals)
        x(0) = 99 : y(0) = 99 : amount(0) = 99 : labels(0) = "changed" : ordinals(0) = 99
        CollectionAssert.AreEqual(New Double() {1, 2}, actual.XValues)
        CollectionAssert.AreEqual(New Double() {3, 4}, actual.YValues)
        CollectionAssert.AreEqual(New Double() {2, 2}, actual.Amounts)
        CollectionAssert.AreEqual(New String() {"same", "same"}, actual.Labels)
        CollectionAssert.AreEqual(New Integer() {8, 2}, actual.SourceOrdinals)
        Dim data = DumbbellPlotCore.Compute(New String() {"A", "B"}, New Double() {5, 1}, New Double() {2, 3})
        Dim first = ExcelCommon.Chart34.DumbbellSeries.DumbbellSeriesPreparation34.Endpoints(data.Categories, True)
        Dim second = ExcelCommon.Chart34.DumbbellSeries.DumbbellSeriesPreparation34.Endpoints(data.Categories, False)
        CollectionAssert.AreEqual(data.Categories.Select(Function(c) c.SourceIndex).ToArray(), first.SourceOrdinals)
        CollectionAssert.AreEqual(New Double() {5, 1}, first.XValues)
        CollectionAssert.AreEqual(New Double() {2, 3}, second.XValues)
        Dim reference = ExcelCommon.Chart34.CdfSeries.CdfSeriesPreparation34.PercentileReference(2, 25, -1, 0)
        CollectionAssert.AreEqual(New Double() {-1, 2, 2}, reference.XValues)
        CollectionAssert.AreEqual(New Double() {25, 25, 0}, reference.YValues)
        Assert.IsNull(ExcelCommon.Chart34.CdfSeries.CdfSeriesPreparation34.PercentileReference(Double.NaN, 25, -1, 0))
    End Sub
End Class
