Option Explicit On
Option Strict On

Imports System
Imports System.Collections.Generic
Imports System.Globalization
Imports System.Linq
Imports System.Threading
Imports BESHStatNG.ExcelCommon
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass>
Public Class UserFaultTests
    Private Const Payload As String = "PRIVATE 'Sheet!A1' C:\people\secret.xlsx <script> stack at JS.write"

    Private NotInheritable Class HostileException
        Inherits Exception
        Public Overrides ReadOnly Property Message As String
            Get
                Throw New InvalidOperationException("Message must never be read by presentation.")
            End Get
        End Property
        Public Overrides ReadOnly Property StackTrace As String
            Get
                Throw New InvalidOperationException("StackTrace must never be read by presentation.")
            End Get
        End Property
        Public Overrides Function ToString() As String
            Throw New InvalidOperationException("ToString must never be read by presentation.")
        End Function
    End Class

    <TestMethod>
    Public Sub UnknownExceptionDoesNotEvenReadNativeDisplayProperties()
        Assert.AreEqual(UserFaultCodes.Unexpected, UserFaultMapper.FromException(New HostileException()).Code)
    End Sub

    <DataTestMethod>
    <DataRow("analysis.invalid-request", UserFaultCategory.Input)>
    <DataRow("analysis.unsupported", UserFaultCategory.Analysis)>
    <DataRow("analysis.execution-failed", UserFaultCategory.Analysis)>
    Public Sub KnownAnalysisCodesRemainStableWithoutNativeMessages(code As String, category As UserFaultCategory)
        Dim original = New AnalysisFault(code, Payload, "options")
        Dim fault = UserFaultMapper.FromAnalysisFault(original)
        Assert.AreEqual(code, fault.Code)
        Assert.AreEqual(code, fault.OriginalCode)
        Assert.AreEqual(category, fault.Category)
        Assert.AreEqual(Payload, original.Message)
        AssertSafe(fault)
        Assert.IsFalse(UserFaultMapper.FormatDisplay(fault).Contains("convergence"))
    End Sub

    <DataTestMethod>
    <DataRow("PRIVATE")> <DataRow("ANALYSIS.INVALID-REQUEST")> <DataRow("diagnostic.run.failed")>
    <DataRow("analysis.future")> <DataRow(" output.host-failed ")>
    Public Sub UnknownAnalysisCodesHaveOneFixedFallback(code As String)
        Dim fault = UserFaultMapper.FromAnalysisFault(New AnalysisFault(code, Payload, Payload))
        Assert.AreEqual(UserFaultCodes.Unexpected, fault.Code)
        Assert.IsNull(fault.OriginalCode)
        Assert.IsNull(fault.FieldPath)
        AssertSafe(fault)
    End Sub

    <DataTestMethod>
    <DataRow(SpreadsheetValidationSeverity.Error, "user.input-invalid")>
    <DataRow(SpreadsheetValidationSeverity.Warning, "user.input-warning")>
    <DataRow(SpreadsheetValidationSeverity.Information, "user.input-information")>
    Public Sub ValidationPreservesSeverityAndSelectedOffsetsOnly(severity As SpreadsheetValidationSeverity, code As String)
        Dim issue = New SpreadsheetValidationIssue("cell.not-numeric", severity, Payload, "observations", 3, 2, 103, 42)
        Dim fault = UserFaultMapper.FromValidation(issue)
        Assert.AreEqual(code, fault.Code)
        Assert.AreEqual("cell.not-numeric", fault.OriginalCode)
        Assert.AreEqual("observations", fault.FieldPath)
        Assert.AreEqual(3, fault.SelectedRow) : Assert.AreEqual(2, fault.SelectedColumn)
        StringAssert.Contains(fault.Action, "numeric observation")
        Dim display = UserFaultMapper.FormatDisplay(fault)
        StringAssert.Contains(display, "Selected row 3")
        Assert.IsFalse(display.Contains("103")) : Assert.IsFalse(display.Contains("42"))
        AssertSafe(fault)
    End Sub

    <DataTestMethod>
    <DataRow("options.invalid", "orientation")> <DataRow("options.long-columns", "orientation")>
    <DataRow("groups.too-few", "two groups")> <DataRow("groups.too-few-observations", "two groups")>
    <DataRow("header.ambiguous", "header")> <DataRow("groups.category-missing", "category")>
    <DataRow("cell.excel-error", "numeric observation")>
    <DataRow("header.excel-error", "header")>
    <DataRow("groups.category-error", "category")>
    <DataRow("cell.missing-skipped", "skipped")> <DataRow("header.inferred", "inferred")>
    Public Sub RecognizedValidationCodesHaveSpecificActions(code As String, expected As String)
        Dim fault = UserFaultMapper.FromValidation(New SpreadsheetValidationIssue(code, SpreadsheetValidationSeverity.Error, Payload, "input"))
        Assert.AreEqual(code, fault.OriginalCode)
        StringAssert.Contains(fault.Action, expected)
        AssertSafe(fault)
    End Sub

    <DataTestMethod>
    <DataRow(Nothing)> <DataRow("")> <DataRow("groups[0]")> <DataRow("groups[16383]")>
    <DataRow("groups[]")> <DataRow("groups[01]")> <DataRow("groups[-1]")> <DataRow("groups[16384]")>
    <DataRow("groups[PRIVATE]")> <DataRow("C:\people\secret.xlsx")> <DataRow("options" & vbLf)>
    <DataRow("rows")> <DataRow("header")> <DataRow("category")> <DataRow("options.alpha")>
    Public Sub FieldPathsAreClosedVocabularyAndBoundedIndexes(field As String)
        Dim fault = UserFaultMapper.FromValidation(New SpreadsheetValidationIssue(Payload, SpreadsheetValidationSeverity.Warning, Payload, field, Integer.MaxValue, Integer.MaxValue))
        Dim known = field = "groups[0]" OrElse field = "groups[16383]" OrElse field = "rows" OrElse
            field = "header" OrElse field = "category" OrElse field = "options.alpha"
        Assert.AreEqual(If(known, field, Nothing), fault.FieldPath)
        Assert.AreEqual(0, fault.SelectedRow) : Assert.AreEqual(0, fault.SelectedColumn)
        Assert.IsNull(fault.OriginalCode)
        Assert.AreEqual(UserFaultCodes.InputWarning, fault.Code)
        AssertSafe(fault)
    End Sub

    <TestMethod>
    Public Sub ReportSnapshotIsReadOnlyAndPrimaryErrorWinsOverWarning()
        Dim source As New List(Of SpreadsheetValidationIssue) From {
            New SpreadsheetValidationIssue("cell.missing-skipped", SpreadsheetValidationSeverity.Warning, Payload, "observations"),
            New SpreadsheetValidationIssue("cell.not-numeric", SpreadsheetValidationSeverity.Error, Payload, "observations", 8, 3)}
        Dim report = New SpreadsheetValidationReport(source)
        Dim faults = UserFaultMapper.FromReport(report)
        source.Clear()
        Assert.AreEqual(2, faults.Count)
        Assert.ThrowsException(Of NotSupportedException)(Sub() DirectCast(faults, IList(Of UserFault)).Clear())
        Dim primary = UserFaultMapper.FromOutcome(Nothing, validation:=report)
        Assert.AreEqual(UserFaultSeverity.Error, primary.Severity)
        Assert.AreEqual(8, primary.SelectedRow)
        Assert.AreEqual(UserFaultCodes.InvalidInput, UserFaultMapper.FromException(New SpreadsheetInputValidationException(report)).Code)
        Assert.IsFalse(GetType(UserFault).GetProperties().Any(Function(p) p.CanWrite))
        Assert.IsFalse(GetType(UserFault).GetConstructors().Any())
    End Sub

    <TestMethod>
    Public Sub WarningOnlyInputStaysValidAndUnknownWarningIsSafe()
        Dim report = New SpreadsheetValidationReport({New SpreadsheetValidationIssue(Payload, SpreadsheetValidationSeverity.Warning, Payload, Payload)})
        Assert.IsTrue(report.IsValid)
        Assert.AreEqual(UserFaultSeverity.Warning, UserFaultMapper.FromOutcome(Nothing, validation:=report).Severity)
        Dim known = UserFaultMapper.FromWarning(New AnalysisWarning(AnalysisWarningCodes.AlphaDefaulted, Payload, "options"))
        Assert.AreEqual(AnalysisWarningCodes.AlphaDefaulted, known.OriginalCode)
        AssertSafe(known)
        Dim unknown = UserFaultMapper.FromWarning(New AnalysisWarning(Payload, Payload, Payload))
        Assert.IsNull(unknown.OriginalCode) : Assert.IsNull(unknown.FieldPath)
        Assert.AreEqual(UserFaultSeverity.Warning, unknown.Severity)
        AssertSafe(unknown)
    End Sub

    <TestMethod>
    Public Sub ExceptionWhitelistDoesNotGuessFromBaseTypesOrInnerExceptions()
        For Each exception As Exception In {New ArgumentException(Payload), New InvalidOperationException(Payload),
            New NotSupportedException(Payload), New ArithmeticException(Payload), New OperationCanceledException(Payload),
            New NullReferenceException(Payload), New Exception(Payload, New ArgumentException(Payload))}
            Dim fault = UserFaultMapper.FromException(exception)
            Assert.AreEqual(UserFaultCodes.Unexpected, fault.Code, exception.GetType().Name)
            AssertSafe(fault)
        Next
    End Sub

    <DataTestMethod>
    <DataRow("host.unavailable", "user.host-unavailable")>
    <DataRow("host.capability-missing", "user.host-unavailable")>
    <DataRow("analysis.unknown", "analysis.unsupported")>
    <DataRow("output.target-invalid", "output.rejected")>
    <DataRow("output.destination-unsupported", "output.rejected")>
    <DataRow("PRIVATE", "user.unexpected")>
    Public Sub OnlyTypedRecognizedHostFailuresAreMapped(code As String, expected As String)
        Dim fault = UserFaultMapper.FromException(New HostCapabilityException(CapabilityPreflightResult.Rejected(code, Payload)))
        Assert.AreEqual(expected, fault.Code)
        AssertSafe(fault)
    End Sub

    <DataTestMethod>
    <DataRow(OutputWriteState.CancelledByUser, "output.cancelled", UserFaultSeverity.Information)>
    <DataRow(OutputWriteState.RejectedBeforeWrite, "output.consent-stale", UserFaultSeverity.Warning)>
    <DataRow(OutputWriteState.RejectedBeforeWrite, "output.occupancy-unverified", UserFaultSeverity.Warning)>
    <DataRow(OutputWriteState.RejectedBeforeWrite, "output.confirmation-required", UserFaultSeverity.Information)>
    <DataRow(OutputWriteState.RejectedBeforeWrite, "output.rejected", UserFaultSeverity.Error)>
    <DataRow(OutputWriteState.FailedBeforeWrite, "output.host-failed", UserFaultSeverity.Error)>
    <DataRow(OutputWriteState.PartiallyWritten, "output.mutation-uncertain", UserFaultSeverity.Warning)>
    Public Sub WriterStateAndSafeActionsArePreserved(state As OutputWriteState, code As String, severity As UserFaultSeverity)
        Dim fault = UserFaultMapper.FromWriteState(state, code)
        Assert.AreEqual(code, fault.Code)
        Assert.AreEqual(severity, fault.Severity)
        AssertSafe(fault)
        If state = OutputWriteState.PartiallyWritten Then
            StringAssert.Contains(fault.Summary, "No rollback")
            StringAssert.Contains(fault.Action, "remaining output")
        End If
    End Sub

    <TestMethod>
    Public Sub WriteStateWinsContradictoryCodeAndUnknownCodesAreNotEchoed()
        Assert.IsNull(UserFaultMapper.FromWriteState(OutputWriteState.Completed, WriteCodes.Cancelled, True))
        Assert.AreEqual(WriteCodes.Cancelled, UserFaultMapper.FromWriteState(OutputWriteState.CancelledByUser, WriteCodes.Failed).Code)
        Assert.AreEqual(WriteCodes.MutationUncertain, UserFaultMapper.FromWriteState(OutputWriteState.PartiallyWritten, WriteCodes.Completed).Code)
        Assert.AreEqual(WriteCodes.MutationUncertain, UserFaultMapper.FromWriteState(OutputWriteState.RejectedBeforeWrite, WriteCodes.Rejected, True).Code)
        Assert.AreEqual(WriteCodes.Rejected, UserFaultMapper.FromWriteState(OutputWriteState.RejectedBeforeWrite, Payload).Code)
        Assert.AreEqual(UserFaultCodes.Unexpected, UserFaultMapper.FromWriteState(CType(999, OutputWriteState), Payload).Code)
    End Sub

    <TestMethod>
    Public Sub RequestAloneIsNotCancelledAndCompletedWriteWinsAllOtherSignals()
        Using execution As New ExecutionCoordinator()
            execution.Start() : execution.RequestCancellation()
            Assert.IsNull(UserFaultMapper.FromOutcome(execution.Snapshot))
            Assert.AreEqual(UserFaultCodes.Unexpected, UserFaultMapper.FromOutcome(execution.Snapshot, failureException:=New ArgumentException(Payload)).Code)
            execution.TryComplete()
            Assert.IsNull(UserFaultMapper.FromOutcome(execution.Snapshot, failureException:=New Exception(Payload)))
            Dim failed = New AnalysisFault(AnalysisFaultCodes.InvalidRequest, Payload)
            Assert.IsNull(UserFaultMapper.FromOutcome(Nothing, CompletedResult(), New Exception(Payload), failed))
        End Using
        Using execution As New ExecutionCoordinator()
            execution.Start() : execution.RequestCancellation()
            Assert.ThrowsException(Of OperationCanceledException)(Sub() execution.ThrowIfCancellationRequested())
            Dim fault = UserFaultMapper.FromOutcome(execution.Snapshot, failureException:=New Exception(Payload))
            Assert.AreEqual(UserFaultCodes.Cancelled, fault.Code)
            Assert.AreEqual(UserFaultSeverity.Information, fault.Severity)
        End Using
    End Sub

    <TestMethod>
    Public Sub NullEmptyAndCorrelationInputsAreSafe()
        Assert.AreEqual(UserFaultCodes.Unexpected, UserFaultMapper.FromException(Nothing).Code)
        Assert.AreEqual(UserFaultCodes.Unexpected, UserFaultMapper.FromAnalysisFault(Nothing).Code)
        Assert.AreEqual(UserFaultCodes.Unexpected, UserFaultMapper.FromValidation(Nothing).Code)
        Assert.AreEqual(0, UserFaultMapper.FromReport(Nothing).Count)
        Assert.AreEqual(0, UserFaultMapper.FromReport(New SpreadsheetValidationReport(Array.Empty(Of SpreadsheetValidationIssue)())).Count)
        Assert.AreEqual(String.Empty, UserFaultMapper.FormatDisplay(Nothing, Payload))
        Dim fault = UserFaultMapper.FromException(New Exception(Payload))
        Dim plain = UserFaultMapper.FormatDisplay(fault)
        Assert.AreEqual(plain, UserFaultMapper.FormatDisplay(fault, Payload))
        Assert.AreEqual(plain, UserFaultMapper.FormatDisplay(fault, Guid.Empty.ToString("N")))
        Dim run = Guid.NewGuid().ToString("N")
        StringAssert.Contains(UserFaultMapper.FormatDisplay(fault, run), "Run " & run)
    End Sub

    <TestMethod>
    Public Sub CodesAndCopyAreCultureIndependentAndUnknownsDeterministic()
        Dim previous = Thread.CurrentThread.CurrentCulture
        Try
            Dim outputs As New List(Of String)()
            For Each culture In {"en-US", "tr-TR", "ar-SA", "sk-SK"}
                Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo(culture)
                Dim fault = UserFaultMapper.FromValidation(New SpreadsheetValidationIssue("cell.not-numeric", SpreadsheetValidationSeverity.Error, Payload, "groups[2]", 12, 4))
                outputs.Add(UserFaultMapper.FormatDisplay(fault))
                Assert.AreEqual("user.input-invalid", fault.Code)
                Assert.AreEqual("groups[2]", fault.FieldPath)
            Next
            Assert.AreEqual(1, outputs.Distinct().Count())
        Finally
            Thread.CurrentThread.CurrentCulture = previous
        End Try
    End Sub

    Private Shared Function CompletedResult() As WriteResult
        Dim intent = New WriteIntent("plan", "target", New SpreadsheetOutputTarget(), Array.Empty(Of WriteRegion)())
        Return New WriteResult(OutputWriteState.Completed, WriteCodes.Completed, Payload, intent, False)
    End Function

    Private Shared Sub AssertSafe(fault As UserFault)
        Dim text = UserFaultMapper.FormatDisplay(fault) & fault.FieldPath & fault.OriginalCode
        For Each secret In {"PRIVATE", "secret.xlsx", "<script>", "JS.write", "Sheet!A1"}
            Assert.IsFalse(text.Contains(secret), text)
        Next
        Assert.IsFalse(String.IsNullOrWhiteSpace(fault.Title))
        Assert.IsFalse(String.IsNullOrWhiteSpace(fault.Action))
        Assert.IsFalse(fault.Code.StartsWith("diagnostic.", StringComparison.Ordinal))
    End Sub
End Class
