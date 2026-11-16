Option Strict On
Option Explicit On
Imports System
Imports BESHStatNG.Test.Harness45
Imports BESHStatNG.AppInfrastructure
Imports BESHStatNG.ExcelCommon
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass, DoNotParallelize>
Public Class WindowsHarness45FormTests
    <DataTestMethod>
    <DataRow(0)> <DataRow(1)> <DataRow(2)>
    Public Sub OutputRadioEventsReachOriginalHandlerAndNestedSelection(mode As Integer)
        Dim form As New OptionProjection45()
        form.RefEditOutput.Enabled = False
        form.optOutputRange.FireClick()
        Assert.IsTrue(form.RefEditOutput.Enabled)
        Assert.AreEqual(1, form.RefEditOutput.txtAddress.Selections)
        If mode = 0 Then form.optWorksheet.FireClick()
        If mode = 1 Then form.optWorkbook.FireClick()
        If mode = 2 Then form.optOutputRange.FireClick()
        Assert.AreEqual(mode = 2, form.RefEditOutput.Enabled)
        form.optOutputRange.FireClick()
        Assert.IsTrue(form.RefEditOutput.Enabled)
    End Sub

    <DataTestMethod>
    <DataRow("NormalityTests")> <DataRow("ViolinPlot")> <DataRow("CDFplot")> <DataRow("CategoricalHistogram")> <DataRow("MannWhitneyTest")> <DataRow("ROCCurve")> <DataRow("UnpairedTTest")>
    Public Sub GroupingEventsRetainLabelsResetFocusAndReenable(topicName As String)
        Dim form As New OptionProjection45 With {.Tag = CType([Enum].Parse(GetType(HelpTopic), topicName), HelpTopic)}
        form.RefEdit1.txtAddress.Text = "synthetic-selection"
        form.optByColumn.Checked = True : form.optByColumn.FireClick()
        Assert.AreEqual("", form.RefEdit1.txtAddress.Text)
        Dim twoInputs = form.Tag = HelpTopic.MannWhitneyTest OrElse form.Tag = HelpTopic.UnpairedTTest OrElse form.Tag = HelpTopic.ROCCurve
        Assert.AreEqual(twoInputs, form.RefEdit1.Enabled)
        If twoInputs Then Assert.IsTrue(form.RefEdit1.txtAddress.Selections > 0) Else Assert.IsTrue(form.RefEdit2.txtAddress.Selections > 0)
        form.optByColumn.Checked = False : form.optByID.Checked = True : form.optByID.FireClick()
        Assert.IsTrue(form.RefEdit1.Enabled)
        If topicName <> "NormalityTests" Then Assert.AreEqual("Group ID:", form.lblRefedit1.Text)
        form.optByID.FireClick()
        Assert.IsTrue(form.RefEdit1.txtAddress.Selections >= 2)
    End Sub

    <TestMethod>
    Public Sub ActualRefEditLeasePreventsOldSelectionAfterCancelAndReset()
        Dim lease As New RefEditEventLease(Of String), oldCallback As Action(Of String) = Nothing, newCallback As Action(Of String) = Nothing, selected As String = Nothing, detached As Integer
        lease.Rebind(Function(cb)
                         oldCallback = cb
                         Return Sub() detached += 1
                     End Function, Sub(value) selected = value)
        oldCallback("synthetic-first") : Assert.AreEqual("synthetic-first", selected)
        lease.Invalidate() : oldCallback("synthetic-after-escape") : Assert.AreEqual("synthetic-first", selected)
        lease.Rebind(Function(cb)
                         newCallback = cb
                         Return Sub() detached += 1
                     End Function, Sub(value) selected = value)
        oldCallback("synthetic-stale") : Assert.AreEqual("synthetic-first", selected)
        newCallback("synthetic-second") : Assert.AreEqual("synthetic-second", selected)
        lease.Dispose() : newCallback("synthetic-after-close")
        Assert.AreEqual("synthetic-second", selected) : Assert.AreEqual(2, detached)
    End Sub

    <DataTestMethod>
    <DataRow(False)> <DataRow(True)>
    Public Sub ExecutionCancelAndOutputFailureRetainPriority(afterFit As Boolean)
        Dim dispatcher As New WindowsExecutionDispatcher()
        Using run = dispatcher.TryBegin()
            run.Execution.Start()
            If afterFit Then
                Assert.ThrowsException(Of OperationCanceledException)(Sub() run.InvokeStage(ExecutionStage.Analysis, Function()
                                                                                                                       run.Execution.RequestCancellation()
                                                                                                                       Return 45
                                                                                                                   End Function))
            Else
                run.Execution.RequestCancellation()
            End If
            Dim writes = 0
            Assert.ThrowsException(Of OperationCanceledException)(Sub() run.Write(Function()
                                                                                     writes += 1
                                                                                     Return OutputWriteState.Completed
                                                                                 End Function))
            Assert.AreEqual(0, writes)
            Assert.AreEqual(ExecutionState.Cancelled, run.Execution.Snapshot.State)
        End Using
    End Sub

    <TestMethod>
    Public Sub PostWriteCancelCannotUndoCompletedOutput()
        Dim dispatcher As New WindowsExecutionDispatcher()
        Using run = dispatcher.TryBegin()
            run.Execution.Start()
            Assert.AreEqual(OutputWriteState.Completed, run.Write(Function()
                                                                     run.Execution.RequestCancellation()
                                                                     Return OutputWriteState.Completed
                                                                 End Function))
            Assert.AreEqual(ExecutionState.Completed, run.Execution.Snapshot.State)
        End Using
    End Sub
End Class
