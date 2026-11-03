Option Explicit On
Option Strict On
Imports System
Imports System.Collections.Generic
Imports System.Globalization
Imports System.Linq
Imports System.Reflection
Imports BESHStatNG
Imports BESHStatNG.ExcelCommon
Imports BESHStatNG.regression
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass>
Public Class Mixed14AdapterTests
    Private NotInheritable Class LoggerProbe
        Implements AppInfrastructure.IAppLogger
        Public Messages As Integer
        Public Sub Message(value As String) Implements AppInfrastructure.IAppLogger.Info, AppInfrastructure.IAppLogger.Warn,
            AppInfrastructure.IAppLogger.Debug, AppInfrastructure.IAppLogger.Trace, AppInfrastructure.IAppLogger.WriteError, AppInfrastructure.IAppLogger.Error
            Messages += 1
        End Sub
        Public Sub Failure(ex As Exception, Optional message As String = Nothing) Implements AppInfrastructure.IAppLogger.Error
            Messages += 1
        End Sub
        Public Sub WriteFailure(ex As Exception, message As String) Implements AppInfrastructure.IAppLogger.WriteError
            Messages += 1
        End Sub
        Public Sub Dispose() Implements IDisposable.Dispose
        End Sub
    End Class
    Private NotInheritable Class MemoryWriter
        Inherits ResultTableWriterBase
        Public ReadOnly Blocks As New List(Of ResultTableOutputBlock)
        Protected Overrides Sub WriteOutputBlock(block As ResultTableOutputBlock)
            Blocks.Add(block)
        End Sub
    End Class
    Private Shared Function Raw(f As Mixed14Fixture) As DataObj
        Dim first = f.Rows.Min(), last = f.Rows.Max(), values(last - first, 3) As Object
        For i = 0 To f.Y.Length - 1
            Dim row = f.Rows(i) - first
            values(row, 0) = f.Subjects(i) : values(row, 1) = f.Y(i)
            values(row, 2) = If(f.Visit Is Nothing, f.X(i, 1), f.Visit(i)) : values(row, 3) = f.X(i, 1)
        Next
        Dim data As New DataObj
        data.DataImportRawMatrix(values, {"subject", "response", "visit", "x"}, firstSourceRow:=first, CharCols:=0)
        Return data
    End Function
    Public Shared Function Capture(key As String, f As Mixed14Fixture, blocks As MixedModelBlockData, raw As DataObj) As MixedModelWorkflowRequest
        Dim o = Mixed14Requests.Options(key)
        If key.StartsWith("MMRM|", StringComparison.Ordinal) Then
            Return WindowsMixedModelWorkflowAdapter.CaptureMMRM(raw, blocks, f.Subjects, "response | VarB", "subject | VarA", "visit | VarC",
                f.FixedNames, Mixed14Requests.Effects(True), "visit", o)
        End If
        Return WindowsMixedModelWorkflowAdapter.CaptureLMM(raw, blocks, f.Subjects, "response | VarB", "subject | VarA", If(f.Visit Is Nothing, "", "visit | VarC"),
            f.FixedNames, f.RandomNames, Mixed14Requests.Effects(True), Mixed14Requests.Effects(True, f.RandomNames.Length = 1),
            "visit", If(f.RandomNames.Length = 1, "Random intercept only", "visit"), o)
    End Function

    <DataTestMethod><DynamicData(NameOf(Mixed14Fixture.Cases), GetType(Mixed14Fixture), DynamicDataSourceType.Method)>
    Public Sub ActualShippingAdaptersCreateEquivalentCoreAndWriterPayloads(key As String)
        Dim saved = CultureInfo.CurrentCulture
        Try
            For Each culture In {"en-US", "sk-SK"}
                CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(culture)
                Dim f = Mixed14Fixture.Load(key), original = Mixed14Fixture.LegacyRequest(key, f), data = Raw(f)
                CollectionAssert.AreEqual(f.Rows, data.RowIds)
                Dim workflow = Capture(key, f, original.Data, data)
                Assert.IsTrue(workflow.Validation.IsValid, String.Join(";", workflow.Validation.Diagnostics.Select(Function(d) d.Code)))
                Assert.AreEqual("response | VarB", workflow.SourceResponseKey)
                Assert.AreEqual("subject | VarA", workflow.SourceSubjectKey)
                Assert.AreEqual(If(f.Visit Is Nothing, "", "visit | VarC"), workflow.SourceVisitKey)
                If workflow.Kind = MixedModelWorkflowKind.LMM Then
                    Dim emptyOptions = workflow.Options : emptyOptions.RandomIntercept = False
                    emptyOptions.RandomStructure = "Variance Components (VC/Diag)"
                    Dim emptyRandom As New MixedModelWorkflowRequest(workflow.Kind, workflow.Data, workflow.ResponseKey, workflow.SubjectKey, workflow.VisitKey,
                        workflow.FixedNames, workflow.RandomNames, workflow.FixedEffects, New RegressionEffectModel(includeIntercept:=False),
                        workflow.FixedText, "", emptyOptions)
                    Assert.IsFalse(emptyRandom.Validation.IsValid)
                    Assert.IsTrue(emptyRandom.Validation.Diagnostics.Any(Function(d) d.Code = "mixed.random.empty"))
                    Assert.IsNotNull(emptyRandom.CreateAcquiredCoreRequest(original.ResidualStruct, original.RandomStruct), "No new acquired host refusal.")
                End If
                For Each badSubject As Object In New Object() {Double.NaN, Double.PositiveInfinity, Single.NegativeInfinity}
                    Dim bad = MixedModelWorkflowData.FromArrays(MixedModelWorkflowKind.MMRM, {1.0}, New Double(,) {{1}}, {badSubject}, {0.0}, {101})
                    Assert.IsNull(bad.CoreData)
                    Assert.IsTrue(bad.Diagnostics.Any(Function(d) d.Code = "mixed.subject.missing"))
                Next
                Assert.AreSame(original.Data, workflow.Data.CoreData, "No additional X/Z block snapshot.")
                Assert.IsNull(workflow.Data.Provenance, "Raw arrays must not invent worksheet source coordinates.")
                CollectionAssert.AreEqual(f.Rows, workflow.Data.InputRowIds)
                Dim req = workflow.CreateAcquiredCoreRequest(original.ResidualStruct, original.RandomStruct)
                Assert.AreEqual(original.FitMethod, req.FitMethod) : Assert.AreEqual(original.FixedInferenceMethod, req.FixedInferenceMethod)
                Assert.AreEqual(original.FixedFormulaText, req.FixedFormulaText) : Assert.AreEqual(original.RandomFormulaText, req.RandomFormulaText)
                Assert.AreEqual(original.Control.Epsilon, req.Control.Epsilon) : Assert.AreEqual(original.Control.Trace, req.Control.Trace)
                Dim result = Mixed14Fixture.Fit(key, req)
                Dim originalTables = result.wrapResults(0.1, True, workflow.Kind = MixedModelWorkflowKind.LMM, False)
                Dim report = MixedModelReportPolicy.Project(workflow.Kind, result, New MixedModelReportOptions With {.Alpha = 0.1, .IterationDetails = True, .Diagnostics = False})
                Dim before As New MemoryWriter, after As New MemoryWriter
                Dim p As New ProcessListofResultTables(originalTables), q As New ProcessListofResultTables(report.LegacyTables())
                If workflow.Kind = MixedModelWorkflowKind.MMRM Then
                    p.writeToSheet(before, True) : q.writeToSheet(after, True)
                Else
                    p.writeToSheet(before) : q.writeToSheet(after)
                End If
                Assert.AreEqual(before.Blocks.Count, after.Blocks.Count)
                For i = 0 To before.Blocks.Count - 1
                    Assert.AreEqual(before.Blocks(i).StartRow, after.Blocks(i).StartRow)
                    Assert.AreEqual(before.Blocks(i).StartColumn, after.Blocks(i).StartColumn)
                    CollectionAssert.AreEqual(before.Blocks(i).Model.Values, after.Blocks(i).Model.Values)
                    CollectionAssert.AreEqual(before.Blocks(i).Model.PvalueColumns, after.Blocks(i).Model.PvalueColumns)
                    CollectionAssert.AreEqual(before.Blocks(i).Model.PvalueCells, after.Blocks(i).Model.PvalueCells)
                Next
                result.ReleaseLargePostEstimationWorkspaces()
                Assert.AreEqual(0, result.AdditionalResultTables.Count)
            Next
        Finally
            CultureInfo.CurrentCulture = saved
        End Try
    End Sub

    <TestMethod>
    Public Sub PhysicalSubjectResponseVisitAreasRetainExactOriginalProvenance()
        Dim s As Object(,) = {{"z"}, {"A"}, {"z"}, {"A"}}, y As Object(,) = {{2.0}, {Nothing}, {3.0}, {4.0}}, v As Object(,) = {{1.0}, {0.0}, {0.0}, {1.0}}
        Dim table = ExcelDnaDataImporter.ImportPhysicalSelection("'Data'!A101:A104,D101:D104,F101:F104", {"A101:A104", "D101:D104", "F101:F104"},
            Function(addresses)
                Dim selection As New SpreadsheetRangeSelection With {.SelectionId = "mixed14-physical"}
                For Each address In addresses : selection.Ranges.Add(SpreadsheetRangeReference.FromAddress(address, "Data", "Synthetic.xlsx")) : Next
                Return ExcelDnaSharedApplicationAdapter.SnapshotSelection(selection,
                    Function(reference) ExcelDnaSharedApplicationAdapter.SnapshotValues(reference,
                        If(reference.StartColumn = 1, s, If(reference.StartColumn = 4, y, v)), Function(row, column) "General", Nothing, SpreadsheetDateSystem.Excel1900))
            End Function)
        Dim raw As New DataObj : raw.LoadCoreDataTable(table, CharCols:=0, cloneTable:=False)
        CollectionAssert.AreEqual({101, 103, 104}, raw.RowIds)
        Dim blocks = MixedModelBlockData.FromArrays({2.0, 3.0, 4.0}, New Double(,) {{1, 1}, {1, 0}, {1, 1}}, {"z", "z", "A"}, Nothing, {1.0, 0.0, 1.0}, True, raw.RowIds)
        Dim request = WindowsMixedModelWorkflowAdapter.CaptureMMRM(raw, blocks, {"z", "z", "A"}, "response", "subject", "visit", {"Intercept", "visit"}, Mixed14Requests.Effects(True), "visit", New MixedModelWorkflowOptions With {.ResidualStructure = "CS"})
        Assert.IsNotNull(request.Data.Provenance)
        Assert.AreEqual(4, request.Data.Provenance.SourceAt(0, 1).WorksheetColumn)
        Assert.AreEqual(6, request.Data.Provenance.SourceAt(0, 2).WorksheetColumn)
        Assert.AreEqual(101, request.Data.Provenance.SourceAt(0, 1).WorksheetRow)
        CollectionAssert.AreEqual({104, 103, 101}, request.Data.BlockRowIds)
    End Sub

    <DataTestMethod><DataRow("unknown")><DataRow("Kenward-Roger")><DataRow("AI/Fisher scoring (default)")><DataRow("BFGS")>
    Public Sub RealWindowsHelperDelegationMatchesFrozenOriginal(token As String)
        Dim helper = GetType(WindowsMixedModelWorkflowAdapter).Assembly.GetType("BESHStatNG.regression.MixedModelFrontEndHelpers", True)
        For Each method In {"ParseFitMethod", "ParseInferenceMethod", "ParseCovarianceOptimizerMode", "ParseCovarianceGradientMode"}
            Dim functionInfo = helper.GetMethod(method, BindingFlags.Public Or BindingFlags.NonPublic Or BindingFlags.Static)
            Dim fallback As Object
            Select Case method
                Case "ParseFitMethod" : fallback = MixedModelFitMethod.REML
                Case "ParseInferenceMethod" : fallback = MixedModelFixedInferenceMethod.KenwardRoger
                Case "ParseCovarianceOptimizerMode" : fallback = MixedModelCovarianceOptimizerMode.AverageInformationReml
                Case Else : fallback = MixedModelCovarianceGradientMode.Auto
            End Select
            Dim original = GetType(LegacyMixed14Options).GetMethod(method).Invoke(Nothing, {CType(token, Object), fallback})
            Assert.AreEqual(original, functionInfo.Invoke(Nothing, {CType(token, Object), fallback}))
        Next
    End Sub

    <TestMethod>
    Public Sub UdfAlignedArraysMatchCleanGuiValuesWithoutInventingWorksheetRows()
        Dim y As Object(,) = {{2.0}, {3.0}, {Nothing}, {4.0}}, x As Object(,) = {{1.0}, {0.0}, {1.0}, {1.0}}
        Dim subjects As Object(,) = {{"z"}, {"A"}, {"z"}, {"A"}}, visits As Object(,) = {{1.0}, {0.0}, {0.0}, {1.0}}
        Dim moduleType = GetType(WindowsMixedModelWorkflowAdapter).Assembly.GetType("BESHStatNG.UdfDataImport", True)
        Dim method = moduleType.GetMethod("TryGetMmrmAlignedInputs", BindingFlags.NonPublic Or BindingFlags.Public Or BindingFlags.Static)
        Dim args As Object() = {y, x, subjects, visits, Nothing, Nothing, Nothing, Nothing, Nothing, Nothing, 0, Nothing, "", "relative", Nothing}
        Assert.IsTrue(CBool(method.Invoke(Nothing, args)), CStr(args(11)))
        CollectionAssert.AreEqual({2.0, 3.0, 4.0}, DirectCast(args(4), Double()))
        CollectionAssert.AreEqual(New Object() {"z", "A", "A"}, DirectCast(args(6), Object()))
        Assert.AreEqual(1, CInt(args(10)))
        ' No coordinate/RowId output is supplied by this positional helper.
        ' UDF row identity and default inference/controls are documented separately.
        Dim identity = MixedModelOptionPolicy.ParseInferenceMethod(Nothing)
        Assert.AreEqual(MixedModelFixedInferenceMethod.KenwardRoger, identity)
        Assert.AreEqual(MixedModelFixedInferenceMethod.WaldNormal, MixedModelWorkflowPolicy.DecideFit(MixedModelWorkflowKind.LMM, Nothing, Nothing).EffectiveInference)
        Dim pure = Mixed14Requests.Request("LMM|Random Intercept|Large-sample normal|REML|toy")
        Dim savedLogger = AppInfrastructure.CoreServices.Logger, probe As New LoggerProbe
        Try
            AppInfrastructure.CoreServices.Logger = probe
            Assert.IsTrue(MixedModelWorkflowPolicy.Validate(pure).IsValid)
            Dim badOptions = pure.Options : badOptions.ResidualStructure = "unknown" : badOptions.RandomStructure = "unknown"
            Dim invalid As New MixedModelWorkflowRequest(pure.Kind, pure.Data, pure.ResponseKey, pure.SubjectKey, pure.VisitKey,
                pure.FixedNames, pure.RandomNames, pure.FixedEffects, pure.RandomEffects, pure.FixedText, pure.RandomText, badOptions)
            Assert.IsFalse(invalid.Validation.IsValid)
            Assert.AreEqual(0, probe.Messages, "Pure validation must not call logging Core factories or duplicate host trace events.")
        Finally
            AppInfrastructure.CoreServices.Logger = savedLogger
        End Try

    End Sub
End Class
