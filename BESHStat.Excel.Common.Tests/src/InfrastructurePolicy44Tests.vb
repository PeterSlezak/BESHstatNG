Option Explicit On
Option Strict On
Imports System.Globalization
Imports BESHStatNG.ExcelCommon
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass>
Public Class InfrastructurePolicy44Tests
    <DataTestMethod>
    <DataRow(8, 0, 20, 8)> <DataRow(8, 10, 2, 2)> <DataRow(8, -3, -1, -1)> <DataRow(0, 0, 0, 0)>
    Public Sub EffectiveDecimalUsesSequentialLegacyBounds(value As Integer, minimum As Integer, maximum As Integer, expected As Integer)
        Assert.AreEqual(CDec(expected), PreferenceSnapshot.ClampDisplayDecimal(value, minimum, maximum))
    End Sub

    <DataTestMethod>
    <DataRow("", "1.25", 1.25)> <DataRow("sk-SK", "1,25", 1.25)> <DataRow("en-US", "1,234.5", 1234.5)>
    <DataRow("sk-SK", " -2e3 ", -2000.0)> <DataRow("", "+17", 17.0)>
    Public Sub ExplicitCultureIsRespected(culture As String, text As String, expected As Double)
        Dim value As Double
        Assert.IsTrue(NumberTextPolicy.TryDouble(text, CultureInfo.GetCultureInfo(culture), value))
        Assert.AreEqual(expected, value)
        Assert.AreEqual(expected.ToString("G", CultureInfo.GetCultureInfo(culture)), NumberTextPolicy.FormatGeneral(expected, CultureInfo.GetCultureInfo(culture)))
    End Sub

    <DataTestMethod>
    <DataRow(1.5, False)> <DataRow(2147483648.0, False)> <DataRow(-2147483648.0, True)> <DataRow(0.0, True)>
    Public Sub IntegerConversionRetainsRejectionAndByRef(value As Double, accepted As Boolean)
        Dim result As Integer = 99
        Assert.AreEqual(accepted, NumberTextPolicy.TryIntegerValue(value, result))
        Assert.AreEqual(If(accepted, CInt(value), 0), result)
    End Sub

    <TestMethod>
    Public Sub NonfiniteAndElapsedFormattingRetainInvariantContract()
        For Each d In {Double.NaN, Double.PositiveInfinity, Double.NegativeInfinity}
            Assert.AreEqual("", NumberTextPolicy.FormatProgressDouble(d))
            Assert.AreEqual("", NumberTextPolicy.FormatProgressElapsedMilliseconds(d))
            Dim i As Integer = 99
            Assert.IsFalse(NumberTextPolicy.TryIntegerValue(d, i)) : Assert.AreEqual(0, i)
        Next
        Assert.AreEqual("", NumberTextPolicy.FormatProgressElapsedMilliseconds(-1))
        Assert.AreEqual("1.25 s", NumberTextPolicy.FormatProgressElapsedMilliseconds(1250))
        Assert.AreEqual("1E-5", NumberTextPolicy.FormatProgressDouble(0.00001))
        Assert.AreEqual("0.0001", NumberTextPolicy.FormatProgressDouble(0.0001))
        Assert.AreEqual("1E+6", NumberTextPolicy.FormatProgressDouble(1000000))
    End Sub

    <TestMethod>
    Public Sub SeedSentinelAndSnapshotsRemainIndependent()
        Assert.AreEqual("", PreferenceSnapshot.SeedDisplayText(Integer.MinValue))
        Assert.AreEqual("-173", PreferenceSnapshot.SeedDisplayText(-173))
        Dim snapshot = PreferenceSnapshot.Defaults(), data = snapshot.ToData()
        data.DefaultAlpha = 0.2 : data.PValues.DecimalPlaces = 2
        Assert.AreEqual(0.05, snapshot.DefaultAlpha) : Assert.AreEqual(8, snapshot.DecimalPlaces)
    End Sub

    <DataTestMethod>
    <DataRow(-1, 0)> <DataRow(0, 0)> <DataRow(43, 43)> <DataRow(100, 100)> <DataRow(Integer.MaxValue, 100)>
    Public Sub HostPercentHasNoCoordinatorOrDispatchSideEffect(value As Integer, expected As Integer)
        Assert.AreEqual(expected, ExecutionSnapshot.ClampHostReportedPercent(value))
        Using run As New ExecutionCoordinator()
            run.Start() : run.Report(value, "suppressed free text")
            Assert.IsTrue(run.Snapshot.Percent.Value <= 99)
        End Using
    End Sub

    <DataTestMethod>
    <DataRow(Nothing, "fallback")> <DataRow("", "fallback")> <DataRow(" ", "fallback")> <DataRow(" exact ", " exact ")>
    Public Sub CaptionSelectsOriginalTextWithoutTrimmingOrNewRedaction(title As String, expected As String)
        Assert.AreEqual(expected, UserFaultMapper.PresentationTitle(title, "fallback"))
    End Sub

    <TestMethod>
    Public Sub ExistingFailureAndCancellationPrecedenceRemainsShared()
        Dim fault = UserFaultMapper.FromException(New Exception("controlled confidential detail"))
        Assert.IsFalse((fault.Summary & fault.Action).Contains("controlled confidential detail"))
        Using run As New ExecutionCoordinator()
            run.Start() : run.RequestCancellation()
            Assert.IsTrue(run.TryFail(New Exception("controlled")))
            Assert.AreEqual(ExecutionState.Failed, run.Snapshot.State)
            Assert.IsFalse(run.TryComplete())
        End Using
    End Sub
End Class
