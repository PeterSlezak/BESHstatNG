Option Explicit On
Option Strict On
Imports System
Imports System.Globalization
Imports System.Reflection
Imports BESHStatNG
Imports BESHStatNG.AppInfrastructure
Imports BESHStatNG.ExcelCommon
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass, DoNotParallelize>
Public Class InfrastructurePolicy44AdapterTests
    <DataTestMethod>
    <DataRow("")> <DataRow("en-US")> <DataRow("sk-SK")>
    Public Sub WindowsNumberReadersRetainCultureFallbackAndErrors(name As String)
        Dim previous = CultureInfo.CurrentCulture
        Try
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(name)
            For Each sample As String In {"1.25", "1,25", " -2e3 ", "+17", "bad", "", Nothing}
                Dim a As Double = 99, b As Double = 99, s = If(sample, "").Trim(), expected As Boolean
                If s = "" Then
                    b = 0 : expected = False
                Else
                    expected = NumberTextPolicy.TryDouble(s, UIprocedures.GetExcelNumberCulture(), b) OrElse NumberTextPolicy.TryDouble(s, CultureInfo.CurrentCulture, b) OrElse NumberTextPolicy.TryDouble(s, CultureInfo.InvariantCulture, b)
                End If
                Assert.AreEqual(expected, UIprocedures.TryParseUiDouble(sample, a)) : Assert.AreEqual(b, a)
            Next
            Dim failure = Assert.ThrowsException(Of FormatException)(Sub() UIprocedures.ParseUiInteger("bad", "test field"))
            Assert.AreEqual("Cannot parse test field: ""bad"".", failure.Message)
            Assert.AreEqual("", UIprocedures.FormatProgressElapsedSeconds(Nothing))
            Assert.AreEqual("0.00 s", UIprocedures.FormatProgressElapsedSeconds(New Diagnostics.Stopwatch()))
            Assert.AreEqual("1E-5", UIprocedures.FormatProgressDouble(0.00001))
            Assert.AreEqual(1.25.ToString("G", UIprocedures.GetExcelNumberCulture()), UIprocedures.FormatUiDouble(1.25))
        Finally
            CultureInfo.CurrentCulture = previous
        End Try
    End Sub

    <TestMethod>
    Public Sub EffectiveSettingsAreAcquiredAtCallAndRejectedSaveDoesNotApply()
        Dim previous = AppGlobals.gSettings, runtime = CoreServices.AnalysisDefaults, trace = CoreServices.TraceExecutionLoggingEnabled
        Try
            AppGlobals.ApplySettings(New BeshStatNgSettings With {.DefaultAlpha = 0.01, .DefaultRandomSeed = 173})
            Assert.AreEqual(0.01D, AppGlobals.GetDefaultAlphaDecimal(0, 1))
            Assert.AreEqual("173", AppGlobals.GetDefaultRandomSeedText())
            AppGlobals.ApplySettings(New BeshStatNgSettings With {.DefaultAlpha = 0.2, .DefaultRandomSeed = Integer.MinValue})
            Assert.AreEqual(0.1D, AppGlobals.GetDefaultAlphaDecimal(0, 0.1D))
            Assert.AreEqual(2D, AppGlobals.GetPValueDecimalPlacesDecimal(10, 2))
            Assert.AreEqual("", AppGlobals.GetDefaultRandomSeedText())
            Dim active = AppGlobals.gSettings
            Dim rejected = AppGlobals.SavePreferences(New BeshStatNgSettings With {.Version = 99, .DefaultAlpha = 0.3})
            Assert.AreEqual(PreferenceStatus.UnsupportedVersion, rejected.Status)
            Assert.AreSame(active, AppGlobals.gSettings) : Assert.AreEqual(0.2, AppGlobals.DefaultAlpha)
        Finally
            AppGlobals.gSettings = previous : CoreServices.AnalysisDefaults = runtime : CoreServices.TraceExecutionLoggingEnabled = trace
        End Try
    End Sub

    <TestMethod>
    Public Sub MissingProgressTargetDoesNotPumpOrDisplay()
        Dim reporter As New WinFormsProgressReporter(Nothing, Nothing)
        reporter.Report(Integer.MaxValue, "controlled")
        ' Actual target-null return executes;no control/HWND/event pump is created.
    End Sub

    <DataTestMethod>
    <DataRow(Nothing, "BESH-Stat-NG")> <DataRow("", "BESH-Stat-NG")> <DataRow(" ", "BESH-Stat-NG")> <DataRow("custom", "custom")>
    Public Sub ActualMessageAdapterConstructorDelegatesCaptionOnly(title As String, expected As String)
        Dim service As New WinFormsUserMessageService(title)
        Dim field = GetType(WinFormsUserMessageService).GetField("_applicationTitle", BindingFlags.NonPublic Or BindingFlags.Instance)
        Assert.AreEqual(expected, CStr(field.GetValue(service)))
        Assert.AreEqual(expected, UserFaultMapper.PresentationTitle(Nothing, expected))
        ' ShowError/MsgBox is intentionally not executed.
    End Sub
End Class
