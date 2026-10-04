Option Explicit On
Option Strict On

Imports System.Linq
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass>
Public Class AgreementPlotPayloadTests

    <TestMethod>
    Public Sub PassingBablok_PlotPayload_ContainsFitAndRawPairs()
        Dim x As Double() = {1, 2, 3, 4, 5}
        Dim y As Double() = {1.2, 2.1, 3.2, 4.1, 5.3}
        Dim fit As New BESHStatNG.Agreement.PassinbBablok(x, y, "Reference", "Test")
        fit.PassingBablokCI()

        Dim payload = fit.GetPlotData()
        Assert.AreEqual(x.Length, payload.XValues.Length)
        Assert.AreEqual(y.Length, payload.YValues.Length)
        Assert.AreEqual("Reference", payload.XName)
        Assert.AreEqual("Test", payload.YName)
        Assert.IsTrue(Double.IsFinite(payload.Intercept))
        Assert.IsTrue(Double.IsFinite(payload.Slope))
    End Sub

    <TestMethod>
    Public Sub BlandAltman_PlotPayload_ContainsAgreementReferenceLines()
        Dim x As Double() = {10, 12, 14, 16, 18}
        Dim y As Double() = {10.5, 11.5, 14.4, 15.7, 18.2}
        Dim fit As New BESHStatNG.Agreement.BlandAltmanAgreement(x, y, "Reference", "Test")
        Dim result = fit.Fit()

        Dim payload = fit.GetPlotData()
        Assert.AreEqual(result.PlotX.Length, payload.XValues.Length)
        Assert.AreEqual(result.BiasCI.Estimate, payload.Bias, 0.0)
        Assert.AreEqual(result.LowerLoACI.Estimate, payload.LowerLoA, 0.0)
        Assert.AreEqual(result.UpperLoACI.Estimate, payload.UpperLoA, 0.0)
    End Sub

    <TestMethod>
    Public Sub LinConcordance_PlotPayload_UsesCommonAxisRange()
        Dim x As Double() = {1, 2, 3, 4, 5}
        Dim y As Double() = {1.1, 2.2, 2.9, 4.2, 5.1}
        Dim fit As New BESHStatNG.Agreement.LinConcordanceCorrelation(x, y, "Reference", "Test")
        fit.Fit()

        Dim payload = fit.GetPlotData()
        Assert.AreEqual(Math.Min(x.Min(), y.Min()), payload.MinValue, 0.0)
        Assert.AreEqual(Math.Max(x.Max(), y.Max()), payload.MaxValue, 0.0)
    End Sub

    <TestMethod>
    Public Sub Deming_PlotPayload_ContainsRegressionLineParameters()
        Dim x As Double() = {1, 2, 3, 4, 5, 6}
        Dim y As Double() = {1.2, 2.1, 3.1, 4.3, 5.0, 6.2}
        Dim opts As New BESHStatNG.Agreement.DemingOptions With {.CiMethod = BESHStatNG.Agreement.AgreementCiMethod.Analytical}
        Dim fit As New BESHStatNG.Agreement.WeightedDemingRegression(x, y, "Reference", "Test", opts)
        Dim result = fit.Fit()

        Dim payload = fit.GetPlotData()
        Assert.AreEqual(result.InterceptCI.Estimate, payload.Intercept, 0.0)
        Assert.AreEqual(result.SlopeCI.Estimate, payload.Slope, 0.0)
        Assert.AreEqual(x.Min(), payload.MinX, 0.0)
        Assert.AreEqual(x.Max(), payload.MaxX, 0.0)
    End Sub

End Class
