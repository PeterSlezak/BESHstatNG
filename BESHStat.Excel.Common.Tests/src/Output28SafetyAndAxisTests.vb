Option Explicit On
Option Strict On

Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports BESHStatNG
Imports BESHStatNG.ExcelCommon
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass>
Public Class Output28SafetyAndAxisTests
    Public Shared Iterator Function Outcomes() As IEnumerable(Of Object())
        For destination = 0 To 2
            For Each scenario In {"empty", "value-denied", "formula-denied", "approved", "stale-plan", "stale-sheet", "rename", "deleted", "readonly", "merged", "chart-changed", "partial"}
                Yield New Object() {destination, scenario}
            Next
        Next
    End Function
    <DataTestMethod, DynamicData(NameOf(Outcomes), DynamicDataSourceType.Method)>
    Public Sub ThreeDestinationsRequireExactConsentAndTruthfulPartialWrite(destination As Integer, scenario As String)
        Dim target As New SpreadsheetOutputTarget With {.Destination = CType(destination, SpreadsheetOutputDestination), .WorksheetName = "Requested", .StartRow = 3, .StartColumn = 4}
        Dim decision = TargetPlanner.Create(target, HostCapabilityProfiles.WindowsExcelDna())
        Assert.IsTrue(decision.Result.IsSupported) : Assert.AreEqual(destination, CInt(decision.EffectiveDestination))
        Dim scope As New WriteIntent("plan", "target", target, {New WriteRegion("table", New OutputRectangle(3, 4, 5, 7)), New WriteRegion("chart", Nothing, True)},
            workbookIdentity:="workbook", worksheetIdentity:="sheet")
        Dim reads As Integer = 0, writes As Integer = 0, prompts As Integer = 0
        Dim rejected = {"readonly", "merged", "deleted"}.Contains(scenario)
        Dim occupied = {"value-denied", "formula-denied", "approved"}.Contains(scenario)
        Dim inspect As Func(Of WriteInspection) = Function()
                                                     reads += 1
                                                     Dim token = "observation"
                                                     Dim current = scope
                                                     If reads > 1 Then
                                                         If scenario = "stale-plan" Then current = New WriteIntent("different-plan", "target", target, scope.Regions, workbookIdentity:="workbook", worksheetIdentity:="sheet")
                                                         If {"stale-sheet", "rename", "chart-changed"}.Contains(scenario) Then token = "changed"
                                                     End If
                                                     Return New WriteInspection(current, If(rejected, OutputOccupancy.Rejected, If(occupied, OutputOccupancy.Occupied, OutputOccupancy.Empty)), token,
                                                         If(occupied, New String() {"table"}, Nothing), hasValues:=scenario = "value-denied" OrElse scenario = "approved", hasFormulas:=scenario = "formula-denied")
                                                 End Function
        Dim actions As Action() = {Sub() writes += 1,
                                   Sub()
                                       writes += 1
                                       If scenario = "partial" Then Throw New InvalidOperationException("Synthetic chart failure")
                                   End Sub}
        Dim outcome = SafeWrite.Execute(scope, inspect, Function(observed)
                                                           prompts += 1
                                                           Return scenario = "approved"
                                                       End Function, actions)
        If {"empty", "approved"}.Contains(scenario) Then
            Assert.AreEqual(OutputWriteState.Completed, outcome.State) : Assert.AreEqual(2, writes)
        ElseIf scenario = "partial" Then
            Assert.AreEqual(OutputWriteState.PartiallyWritten, outcome.State)
            Assert.IsTrue(outcome.MutationMayHaveOccurred) : Assert.AreEqual(1, outcome.ConfirmedWritten.Count)
        Else
            Assert.AreEqual(0, writes)
            If occupied Then
                Assert.AreEqual(OutputWriteState.CancelledByUser, outcome.State)
            Else
                Assert.AreEqual(OutputWriteState.RejectedBeforeWrite, outcome.State)
            End If
        End If
        Assert.AreEqual(If(occupied, 1, 0), prompts)
        Assert.IsFalse(outcome.RollbackAttempted) : Assert.IsFalse(outcome.RollbackSucceeded)
        Assert.AreEqual(destination, CInt(scope.Destination)) : Assert.AreEqual("sheet", scope.WorksheetIdentity)
    End Sub

    <TestMethod>
    Public Sub DefaultDestinationAndReservationConflictAreNotSilentlyRepaired()
        Dim target As New SpreadsheetOutputTarget()
        Assert.AreEqual(SpreadsheetOutputDestination.NewWorksheet, target.Destination)
        Dim region As New WriteRegion("table", New OutputRectangle(1, 1, 2, 2))
        Assert.ThrowsException(Of ArgumentException)(Sub()
                                                        Dim ignored = New WriteIntent("plan", "target", target, {region}, {New OutputRectangle(2, 2, 5, 5)})
                                                    End Sub)
        Assert.ThrowsException(Of ArgumentException)(Sub()
                                                        Dim ignored = New WriteIntent("plan", "target", target, {region, New WriteRegion("overlap", New OutputRectangle(1, 2, 3, 4))})
                                                    End Sub)
    End Sub

    Public Shared Iterator Function Facts() As IEnumerable(Of Object())
        For destination As Integer = 0 To 2
            For Each problem In {"valid", "missing-workbook", "missing-sheet", "ambiguous", "readonly", "protected", "merged", "no-create", "collision", "invalid-name", "outside-grid"}
                Yield New Object() {destination, problem}
            Next
        Next
    End Function
    <DataTestMethod, DynamicData(NameOf(Facts), DynamicDataSourceType.Method)>
    Public Sub TargetFactsNeverChangeRequestedDestinationOrUseNameAsIdentity(destination As Integer, problem As String)
        Dim target As New SpreadsheetOutputTarget With {.Destination = CType(destination, SpreadsheetOutputDestination), .WorksheetName = "Requested", .StartRow = If(problem = "outside-grid", 1048577, 3), .StartColumn = 4}
        Dim facts As New OutputTargetFacts(workbookAvailable:=problem <> "missing-workbook", sheetExists:=problem <> "missing-sheet",
            identityUnambiguous:=problem <> "ambiguous", writable:=problem <> "readonly", protectedSheet:=problem = "protected",
            mergedConflict:=problem = "merged", canCreateWorksheet:=problem <> "no-create", resolvedWorksheetName:="Resolved",
            worksheetNameValid:=problem <> "invalid-name", newWorksheetNameCollision:=problem = "collision")
        Dim decision = TargetPlanner.Create(target, HostCapabilityProfiles.WindowsExcelDna(), facts)
        Dim rejected = {"readonly", "protected", "merged", "invalid-name", "outside-grid"}.Contains(problem) OrElse
            (destination <> 2 AndAlso problem = "missing-workbook") OrElse
            (destination = 0 AndAlso {"missing-sheet", "ambiguous"}.Contains(problem)) OrElse
            (destination = 1 AndAlso {"no-create", "collision"}.Contains(problem))
        Assert.AreEqual(Not rejected, decision.Result.IsSupported, problem)
        Assert.AreEqual(destination, CInt(decision.EffectiveDestination))
        Assert.AreEqual("Resolved", decision.WorksheetName)
        target.WorksheetName = "mutation" : target.Destination = SpreadsheetOutputDestination.NewWorkbook
        Assert.AreEqual(destination, CInt(decision.EffectiveDestination))
        Assert.AreEqual("Requested", decision.RequestedWorksheetName)
        If Not rejected Then
            Dim captured = decision.ToTarget() : captured.StartRow = 99
            Assert.AreEqual(3, decision.StartRow)
        End If
    End Sub

    <TestMethod>
    Public Sub LiddellCurrentLabelAndCandidateAxisAreObservedWithoutCorrectingOutput()
        Dim input = ContingencyInputPolicy.FromDialog(12, 8, 5, 20)
        Assert.IsTrue(input.Success)
        Dim report = ContingencyReportPolicy.TwoByTwo(input.Data, New TwoByTwoReportOptions With {.Liddell = True})
        Assert.IsTrue(report.Success)
        Assert.AreEqual(2, report.Tables.Count)
        Dim table = report.ToLegacyTables()(1), model = table.ToOutputModel()
        Assert.AreEqual("Liddell's Test", model.Values(0, 0))
        Assert.AreEqual("P-value", model.Values(1, 0))
        Assert.AreEqual("Risk ratio", model.Values(2, 0), "Observed label retained; candidate odds-ratio label is not activated")
        Assert.AreEqual(1, model.PvalueCells(0).BodyRow)
        Assert.AreEqual(2, model.PvalueCells(0).BodyColumn)
        Dim computed = contingencytable.Liddell_McNemar(input.Data.CoreCounts, 0.05)
        Assert.AreEqual(computed.Item1.Pvalue, CDbl(model.Values(1, 1)), 0.0)
        Assert.AreEqual(computed.Item2.Estimate, CDbl(model.Values(2, 1)), 0.0)
        Assert.AreEqual(computed.Item2.strConfidenceInterval(CIformat.LL_to_UL), model.Values(3, 1))
    End Sub

    <DataTestMethod><DataRow("time")><DataRow("notime")><DataRow("irregular")>
    Public Sub GeeAxisDescriptorKeepsClusteredAndInputIdentitiesDistinctWithoutChangingPolicy(mode As String)
        Dim key = "Gaussian|Identity|Independence|Robust|" & mode
        Dim raw = Gee12Fixture.Raw(key)
        ' Deliberately interleave existing clusters; no fitting or GUI correction occurs.
        Dim original = Gee12PolicyTests.Request(key, raw)
        Dim clusters = original.Cluster.Values
        Dim temporary = clusters(1) : clusters(1) = clusters(clusters.Length - 1) : clusters(clusters.Length - 1) = temporary
        Dim intent = GeeWorkflowPolicy.Project(original.MeanRequest, New GeeClusterVector(original.Cluster.Key, clusters, original.Cluster.RowIds), original.Time, original.Options)
        Dim descriptor = GeePredictionOutputAxis.Describe(intent)
        Dim permutation = intent.Order.ClusteredToInputIndices
        Dim predictions = Enumerable.Range(0, descriptor.Count).Select(Function(i) 100.0 + i).ToArray()
        Dim aligned = GeeReportPolicy.Predictions(intent, predictions)
        For Each row In descriptor
            Assert.AreEqual(permutation(row.PredictionPosition - 1) + 1, row.InputPosition)
            Assert.AreEqual(intent.MeanRequest.Primary.RowIds(row.InputPosition - 1), row.InputRowId)
            Assert.AreEqual(intent.Cluster.Values(row.InputPosition - 1), row.ClusterValue)
            Assert.AreEqual(predictions(row.PredictionPosition - 1), aligned(row.InputPosition - 1))
        Next
        Assert.IsFalse(intent.Order.PredictionOrderMatchesInput)
        raw.RowIds(0) = -999 : clusters(0) = "mutation"
        Assert.AreNotEqual(-999, descriptor(0).InputRowId)
        Assert.ThrowsException(Of NotSupportedException)(Sub() DirectCast(descriptor, IList(Of GeePredictionOutputRow)).Clear())
    End Sub
End Class
