Option Explicit On
Option Strict On
Imports System
Imports System.Linq
Imports System.Globalization
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports BESHStatNG
Imports BESHStatNG.ExcelCommon.Chart34
<TestClass>
Public Class Graphics34CompletionPolicyTests
    <DataTestMethod><DataRow("")><DataRow("sk-SK")>
    Public Sub CultureFormattingAxisBoundsAndImmutableSettings(culture As String)
        Dim saved = CultureInfo.CurrentCulture
        Try
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(culture)
            Assert.AreEqual(0.0R.ToString("0.0", CultureInfo.CurrentCulture), BulletChartPreparation34.FormatNumeric(-0.0R, "0.0"))
            Assert.AreEqual("0.###E+0", BulletChartPreparation34.BuildAutomaticNumberFormat(1.0E-10R, 1.0R, Nothing))
            Assert.AreEqual("0.###", BulletChartPreparation34.BuildAutomaticNumberFormat(Double.NaN, 1, Nothing))
            Assert.AreEqual("0.0##", CdfChartPreparation34.AutomaticNumberFormat(0.1R))
            Dim values As Double() = {0, 1, 2, 3}
            Dim result = CumulativeDistributionPlotCore.Compute(values, "A")
            Dim settings As New CdfChartSettings34()
            Dim projection = CdfChartPreparation34.ResolveAxisContext(New CumulativeDistributionPlotResult() {result}, settings)
            Assert.AreEqual(0.0R, projection.YMinimum)
            Assert.AreEqual(100.0R, projection.YMaximum)
            Assert.AreEqual("Percent", projection.YAxisTitle)
            Dim minimum = projection.XMinimum
            values(0) = 999
            Assert.AreEqual(minimum, projection.XMinimum)
            For Each p In projection.GetType().GetProperties() : Assert.IsNull(p.GetSetMethod()) : Next
            For Each p In settings.GetType().GetProperties() : Assert.IsNull(p.GetSetMethod()) : Next
            Assert.AreEqual("A - ECDF", CdfChartPreparation34.ResolveEmpiricalSeriesName(result, New CdfChartSettings34(inputLegendMode:=CumulativeDistributionLegendMode.AllCurves), 1))
        Finally
            CultureInfo.CurrentCulture = saved
        End Try
    End Sub
End Class
