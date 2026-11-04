Option Explicit On
Option Strict On
Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports BESHStatNG
Imports BESHStatNG.ExcelCommon
Imports BESHStatNG.regression
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass>
Public Class Mixed15AdapterTests
    Private NotInheritable Class LoggerProbe
        Implements AppInfrastructure.IAppLogger
        Public ReadOnly Messages As New List(Of String)
        Public Sub Message(value As String) Implements AppInfrastructure.IAppLogger.Info, AppInfrastructure.IAppLogger.Warn,
            AppInfrastructure.IAppLogger.Debug, AppInfrastructure.IAppLogger.Trace, AppInfrastructure.IAppLogger.WriteError, AppInfrastructure.IAppLogger.Error
            Messages.Add(value)
        End Sub
        Public Sub Failure(ex As Exception, Optional message As String = Nothing) Implements AppInfrastructure.IAppLogger.Error
            Messages.Add(If(message, "") & ex.GetType().Name)
        End Sub
        Public Sub WriteFailure(ex As Exception, message As String) Implements AppInfrastructure.IAppLogger.WriteError
            Messages.Add(If(message, "") & ex.GetType().Name)
        End Sub
        Public Sub Dispose() Implements IDisposable.Dispose
        End Sub
    End Class

    <TestMethod>
    Public Sub RealAdapterFactoriesPreserveExactlyOneOriginalFactorySideEffect()
        Dim saved = AppInfrastructure.CoreServices.Logger, probe As New LoggerProbe
        Try
            AppInfrastructure.CoreServices.Logger = probe
            For Each row In Mixed15FittingTests.Aliases().Concat({New Object() {"R", "unknown"}, New Object() {"G", "unknown"}, New Object() {"R", Nothing}, New Object() {"G", Nothing}})
                Dim side = CStr(row(0)), token = CStr(row(1)), originalError As String = Nothing, currentError As String = Nothing
                Dim originalType As Type = Nothing, currentType As Type = Nothing
                probe.Messages.Clear()
                Try
                    originalType = If(side = "R", MixedModelRStructUtils.createMixedModelRStruct(token).GetType(), MixedModelGStructUtils.createMixedModelGStruct(token).GetType())
                Catch ex As ApplicationException
                    originalError = ex.Message
                End Try
                Dim expectedMessages = probe.Messages.ToArray()
                probe.Messages.Clear()
                Try
                    currentType = If(side = "R", WindowsMixedModelWorkflowAdapter.CreateResidualStructure(token).GetType(), WindowsMixedModelWorkflowAdapter.CreateRandomStructure(token).GetType())
                Catch ex As ApplicationException
                    currentError = ex.Message
                End Try
                Assert.AreEqual(originalError, currentError)
                Assert.AreEqual(originalType, currentType)
                CollectionAssert.AreEqual(expectedMessages, probe.Messages.ToArray())
                Assert.AreEqual(1, probe.Messages.Count)
            Next
            probe.Messages.Clear()
            Dim selection As New MixedModelFittingOptions(MixedModelWorkflowKind.LMM,
                New MixedModelWorkflowOptions With {.ResidualStructure = "unknown", .RandomStructure = "unknown", .Optimizer = "unknown"})
            selection.DesignDiagnostics(Nothing)
            Assert.AreEqual(0, probe.Messages.Count, "Selection and portable diagnostics must remain silent.")
        Finally
            AppInfrastructure.CoreServices.Logger = saved
        End Try
    End Sub

    <DataTestMethod><DataRow("MMRM|AR1|Large-sample normal|REML|missing")><DataRow("LMM|Random Intercept|Large-sample normal|REML|novisit")><DataRow("LMM|Random Intercept + Slope|Satterthwaite|REML|sleep-unbalanced")>
    Public Sub ShippingCovarianceObjectsAreReusedThroughAcquiredRequest(key As String)
        Dim f = Mixed14Fixture.Load(key), original = Mixed14Fixture.LegacyRequest(key, f)
        Dim raw As New DataObj, values(f.Y.Length - 1, 0) As Object
        For i = 0 To f.Y.Length - 1
            values(i, 0) = f.Y(i)
        Next
        raw.DataImportRawMatrix(values, {"response"}, firstSourceRow:=101)
        ' Existing accepted capture entry uses actual Windows caption projection.
        Dim workflow = Mixed14AdapterTests.Capture(key, f, original.Data, raw)
        Dim residual = WindowsMixedModelWorkflowAdapter.CreateResidualStructure(workflow.Options.ResidualStructure)
        Dim random = If(workflow.Kind = MixedModelWorkflowKind.LMM, WindowsMixedModelWorkflowAdapter.CreateRandomStructure(workflow.Options.RandomStructure), Nothing)
        Dim req = workflow.CreateAcquiredCoreRequest(residual, random)
        Assert.AreSame(residual, req.ResidualStruct)
        If random IsNot Nothing Then Assert.AreSame(random, req.RandomStruct)
        Assert.AreSame(original.Data, req.Data)
        CollectionAssert.AreEqual(original.Data.Blocks.SelectMany(Function(b) b.RowIndices).ToArray(), req.Data.Blocks.SelectMany(Function(b) b.RowIndices).ToArray())
        Dim report = Mixed14Fixture.Fit(key, req), expected = Mixed14Fixture.Fit(key, original)
        Regression10Fixture.AssertCapture(Mixed14Fixture.Capture(key, original, expected), Mixed14Fixture.Capture(key, req, report), key)
    End Sub
End Class
