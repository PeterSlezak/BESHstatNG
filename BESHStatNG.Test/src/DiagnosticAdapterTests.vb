Option Explicit On
Option Strict On

Imports System.Collections.Generic
Imports System.Linq
Imports BESHStatNG.AppInfrastructure
Imports BESHStatNG.ExcelCommon
Imports Microsoft.VisualStudio.TestTools.UnitTesting

Friend Class DiagnosticRecordingLogger
    Implements IAppLogger
    Friend ReadOnly Lines As New List(Of String)
    Friend ReadOnly Levels As New List(Of String)
    Friend ThrowOnWrite As Boolean
    Friend Attempts As Integer
    Private Sub Add(level As String, message As String)
        Attempts += 1
        If ThrowOnWrite Then Throw New ObjectDisposedException("Payroll!A1 =SECRET()")
        Levels.Add(level)
        Lines.Add(message)
    End Sub
    Public Sub Info(message As String) Implements IAppLogger.Info
        Add("info", message)
    End Sub
    Public Sub Warn(message As String) Implements IAppLogger.Warn
        Add("warning", message)
    End Sub
    Public Sub Debug(message As String) Implements IAppLogger.Debug
        Add("debug", message)
    End Sub
    Public Sub Trace(message As String) Implements IAppLogger.Trace
        Add("trace", message)
    End Sub
    Public Sub WriteError(message As String) Implements IAppLogger.WriteError
        Add("error", message)
    End Sub
    Public Sub WriteError(ex As Exception, message As String) Implements IAppLogger.WriteError
        Assert.Fail("The safe bridge must never pass an exception to the legacy logger.")
    End Sub
    Public Sub [Error](message As String) Implements IAppLogger.Error
        Add("error", message)
    End Sub
    Public Sub [Error](ex As Exception, Optional message As String = Nothing) Implements IAppLogger.Error
        Assert.Fail("The safe bridge must never pass an exception to the legacy logger.")
    End Sub
    Public Sub Dispose() Implements IDisposable.Dispose
        ThrowOnWrite = True
    End Sub
End Class

<TestClass>
Public Class DiagnosticAdapterTests
    <TestMethod>
    Public Sub EarlyValidationIsCorrelatedAndTraceOffRetainsEssentialEvents()
        Dim logger As New DiagnosticRecordingLogger()
        Dim before = CoreServices.TraceExecutionLoggingEnabled
        Dim id As String
        Using run As New WindowsDiagnosticRun(AnalysisIds.OneWayAnova, False, New CoreLoggerDiagnosticSink(logger))
            id = run.RunId
            run.EnterStage("validation")
            run.Reject()
            StringAssert.Contains(run.FailureDisplay, id)
            StringAssert.Contains(run.FailureDisplay, "diagnostic.validation.rejected | validation")
        End Using
        Assert.AreEqual(3, logger.Lines.Count)
        Assert.IsTrue(logger.Lines.All(Function(line) line.Contains("run=" & id)))
        CollectionAssert.AreEqual(New String() {"info", "warning", "info"}, logger.Levels)
        Assert.AreEqual(before, CoreServices.TraceExecutionLoggingEnabled)
    End Sub

    <DataTestMethod>
    <DataRow(False)> <DataRow(True)>
    Public Sub SecretsInOutputExceptionsNeverReachLoggerAndLoggingCannotReplaceFailure(throwing As Boolean)
        Dim logger As New DiagnosticRecordingLogger With {.ThrowOnWrite = throwing}
        Dim original As New ArgumentException("=SECRET(Payroll!A1)" & Environment.NewLine & "C:\Users\private\salary.xlsx 秘密")
        Dim session As New ExcelDnaSharedOutputRun()
        Using run As New WindowsDiagnosticRun(AnalysisIds.KruskalWallis, True, New CoreLoggerDiagnosticSink(logger))
            session.Diagnostics = run
            run.OutputAttempted()
            session.LogFailure(original)
            session.LogFailure(original) ' repeated inner/outer catches must not recurse/re-log
            Assert.AreSame(original, session.Failure)
            Assert.IsFalse(run.FailureDisplay.Contains("Payroll"))
            StringAssert.Contains(run.FailureDisplay, "diagnostic.run.failed | output")
        End Using
        Assert.AreEqual(4, logger.Attempts)
        Assert.IsFalse(String.Join("", logger.Lines).Contains("SECRET"))
        Assert.IsFalse(String.Join("", logger.Lines).Contains("private"))
        Assert.IsFalse(String.Join("", logger.Lines).Contains("秘密"))
    End Sub

    <TestMethod>
    Public Sub SeparateInvocationsAndDisposedLoggerAreIsolated()
        Dim logger As New DiagnosticRecordingLogger()
        Dim id As String
        Using first As New WindowsDiagnosticRun(AnalysisIds.OneWayAnova, True, New CoreLoggerDiagnosticSink(logger))
            id = first.RunId
            logger.Dispose()
            first.Reject()
        End Using
        Using second As New WindowsDiagnosticRun(AnalysisIds.OneWayAnova, True, New CoreLoggerDiagnosticSink(logger))
            Assert.AreNotEqual(id, second.RunId)
            second.OutputAttempted()
            second.OutputFinished(OutputWriteState.Completed)
        End Using
        Assert.AreEqual(1, logger.Lines.Count)
    End Sub

    <TestMethod>
    Public Sub UnsafeStageIsRedactedInUiAndLogs()
        Dim logger As New DiagnosticRecordingLogger()
        Using run As New WindowsDiagnosticRun("C:\private.xlsx", True, New CoreLoggerDiagnosticSink(logger))
            run.EnterStage("Payroll!A1")
            run.Reject()
            Assert.IsFalse(run.FailureDisplay.Contains("Payroll"))
        End Using
        Assert.IsTrue(logger.Lines.All(Function(line) Not line.Contains("private") AndAlso Not line.Contains("Payroll")))
    End Sub
End Class
