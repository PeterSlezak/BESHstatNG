Option Explicit On
Option Strict On

Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Text.Json
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports BESHStatNG.ExcelCommon

<TestClass>
Public Class WriteResultTests
    Private Shared Function Intent(Optional destination As SpreadsheetOutputDestination = SpreadsheetOutputDestination.CurrentWorksheet) As WriteIntent
        Return New WriteIntent("payload-v1", "target-instance", New SpreadsheetOutputTarget With {
            .WorksheetName = "Results", .Destination = destination, .StartRow = 6, .StartColumn = 3},
            {New WriteRegion("table-0", New OutputRectangle(6, 3, 7, 5)), New WriteRegion("table-1", New OutputRectangle(9, 3, 11, 4))})
    End Function

    Private Shared Function Observation(scope As WriteIntent, occupied As Boolean, Optional token As String = "snapshot") As WriteInspection
        Return New WriteInspection(scope, If(occupied, OutputOccupancy.Occupied, OutputOccupancy.Empty), token,
            If(occupied, New String() {"table-1"}, New String() {}), hasValues:=occupied)
    End Function

    <TestMethod>
    Public Sub Intent_DefensiveCopiesAndOneBasedExactRegions()
        Dim target As New SpreadsheetOutputTarget With {.WorksheetName = "Results", .StartRow = 6, .StartColumn = 3}
        Dim regions As New List(Of WriteRegion) From {New WriteRegion("table", New OutputRectangle(6, 3, 7, 5)), New WriteRegion("zero", Nothing)}
        Dim reserved As New List(Of OutputRectangle) From {New OutputRectangle(8, 3, 8, 6)}
        Dim scope As New WriteIntent("plan", "target", target, regions, reserved)
        regions.Clear() : reserved.Clear() : target.StartRow = 99 : target.WorksheetName = "changed"
        Assert.AreEqual(6, scope.StartRow) : Assert.AreEqual("Results", scope.WorksheetName)
        Assert.AreEqual(2, scope.Regions.Count) : Assert.IsNull(scope.Regions(1).Bounds)
        Assert.AreEqual("R6C3:R7C5", scope.Regions(0).Bounds.ToString())
        Assert.AreEqual("R8C3:R8C6", scope.Reservations(0).ToString())
        Assert.ThrowsException(Of NotSupportedException)(Sub() DirectCast(scope.Regions, IList(Of WriteRegion)).Clear())
    End Sub

    <DataTestMethod>
    <DataRow("overlap")> <DataRow("duplicate")> <DataRow("null")> <DataRow("grid")>
    <DataRow("reservation")> <DataRow("identity")> <DataRow("too-many")> <DataRow("target")>
    Public Sub InvalidIntent_RejectsBeforeExecution(reason As String)
        Dim target As New SpreadsheetOutputTarget With {.WorksheetName = "Results"}
        Dim regions As New List(Of WriteRegion) From {New WriteRegion("first", New OutputRectangle(1, 1, 2, 2))}
        Dim reservations As OutputRectangle() = Nothing
        Dim id As String = "plan"
        Assert.ThrowsException(Of ArgumentException)(Sub()
            Select Case reason
                Case "overlap" : regions.Add(New WriteRegion("second", New OutputRectangle(2, 2, 3, 3)))
                Case "duplicate" : regions.Add(New WriteRegion("first", Nothing))
                Case "null" : regions.Add(Nothing)
                Case "grid" : regions.Add(New WriteRegion("second", New OutputRectangle(1048577, 1, 1048577, 1)))
                Case "reservation" : reservations = {New OutputRectangle(1, 1, 1, 1)}
                Case "identity" : id = New String("x"c, 257)
                Case "too-many" : regions.AddRange(Enumerable.Range(1, 256).Select(Function(i) New WriteRegion(i.ToString(), Nothing)))
                Case "target" : target.StartRow = 0
            End Select
            Dim invalid = New WriteIntent(id, "target", target, regions, reservations)
        End Sub)
    End Sub

    <DataTestMethod>
    <DataRow(False, False, 0)> <DataRow(False, True, 0)> <DataRow(True, False, 1)> <DataRow(True, True, 0)>
    Public Sub Consent_OnlyOccupiedPromptsAndDecliningNeverExecutes(occupied As Boolean, approved As Boolean, expectedState As Integer)
        Dim scope = Intent(), calls As Integer = 0, prompts As Integer = 0, probes As Integer = 0
        Dim writes As Action() = {Sub() calls += 1, Sub() calls += 1}
        Dim result = SafeWrite.Execute(scope, Function()
                                                  probes += 1
                                                  Return Observation(scope, occupied)
                                              End Function,
            Function(o)
                prompts += 1
                Return approved
            End Function, writes)
        Assert.AreEqual(CType(expectedState, OutputWriteState), result.State)
        Assert.AreEqual(If(occupied, 1, 0), prompts)
        Assert.AreEqual(If(expectedState = 1, 0, 2), calls)
        Assert.AreEqual(If(expectedState = 1, 1, 2), probes)
        Assert.IsFalse(result.UndoSupported) : Assert.IsFalse(result.RollbackAttempted) : Assert.IsFalse(result.RollbackSucceeded)
    End Sub

    <DataTestMethod>
    <DataRow(2)> <DataRow(3)>
    Public Sub UnknownOrRejected_NeverPromptsOrWrites(state As Integer)
        Dim scope = Intent(), calls As Integer = 0
        Dim result = SafeWrite.Execute(scope, Function() New WriteInspection(scope, CType(state, OutputOccupancy), "unknown", Nothing),
            Function(o)
                Assert.Fail("Unverified occupancy must not request permission.")
                Return True
            End Function, New Action() {Sub() calls += 1, Sub() calls += 1})
        Assert.AreEqual(OutputWriteState.RejectedBeforeWrite, result.State)
        Assert.AreEqual(If(state = 2, WriteCodes.Unverified, WriteCodes.Rejected), result.Code) : Assert.AreEqual(0, calls)
    End Sub

    <DataTestMethod>
    <DataRow("plan")> <DataRow("target")> <DataRow("observation")> <DataRow("declined")> <DataRow("formulas")>
    Public Sub Consent_IsExactScopeAndObservation(reason As String)
        Dim scope = Intent(), initial = Observation(scope, True)
        Dim consent As New WriteConsent(initial, reason <> "declined")
        Dim current = Observation(If(reason = "plan" OrElse reason = "target", Intent(), scope), True,
            If(reason = "observation", "changed", "snapshot"))
        If reason = "formulas" Then current = New WriteInspection(scope, OutputOccupancy.Occupied, "snapshot", {"table-1"}, hasFormulas:=True)
        Assert.IsFalse(consent.Authorizes(current))
    End Sub

    <DataTestMethod>
    <DataRow(False)> <DataRow(True)>
    Public Sub DestinationChangesAfterInspection_RejectBeforeWrites(occupied As Boolean)
        Dim scope = Intent(), probe As Integer = 0, writes As Integer = 0
        Dim result = SafeWrite.Execute(scope, Function()
                                                  probe += 1
                                                  Return Observation(scope, occupied, probe.ToString())
                                              End Function, Function(o) True, New Action() {Sub() writes += 1, Sub() writes += 1})
        Assert.AreEqual(OutputWriteState.RejectedBeforeWrite, result.State)
        Assert.AreEqual(WriteCodes.Changed, result.Code) : Assert.AreEqual(0, writes)
    End Sub

    <DataTestMethod>
    <DataRow(-1, 3, 0)> <DataRow(0, 4, 0)> <DataRow(1, 4, 1)>
    Public Sub HostFailures_ReportPrewriteVersusMutationUncertain(failAt As Integer, expected As Integer, confirmed As Integer)
        Dim scope = Intent()
        Dim calls As Integer = -1
        Dim write As Action = Sub()
                                  calls += 1
                                  If calls = failAt Then Throw New InvalidOperationException("PRIVATE WORKBOOK DATA")
                              End Sub
        Dim result = SafeWrite.Execute(scope, Function()
                                                  If failAt = -1 Then Throw New InvalidOperationException("PRIVATE WORKBOOK DATA")
                                                  Return Observation(scope, False)
                                              End Function, Function(o) True, New Action() {write, write})
        Assert.AreEqual(CType(expected, OutputWriteState), result.State)
        Assert.AreEqual(confirmed, result.ConfirmedWritten.Count)
        Assert.AreEqual(expected = 4, result.MutationMayHaveOccurred)
        Assert.IsFalse(result.Message.Contains("PRIVATE"))
    End Sub

    <DataTestMethod>
    <DataRow(0)> <DataRow(1)> <DataRow(2)>
    Public Sub DestinationParity_CompletedEnvelopeRoundTrip(destination As Integer)
        Dim scope = Intent(CType(destination, SpreadsheetOutputDestination))
        Dim result = SafeWrite.Execute(scope, Function() Observation(scope, False), Function(o) True, New Action() {Sub()
                                                                                                                  End Sub, Sub()
                                                                                                                           End Sub})
        Using json = JsonDocument.Parse(JsonSerializer.Serialize(result))
            Assert.AreEqual(1, json.RootElement.GetProperty("SchemaVersion").GetInt32())
            Assert.AreEqual(destination, json.RootElement.GetProperty("Intent").GetProperty("Destination").GetInt32())
            Assert.AreEqual(2, json.RootElement.GetProperty("ConfirmedWritten").GetArrayLength())
            Assert.IsFalse(json.RootElement.GetProperty("UndoSupported").GetBoolean())
        End Using
    End Sub

    <TestMethod>
    Public Sub InvalidResults_RejectFalsePartialRollbackAndFalseCompletion()
        Dim scope = Intent()
        For Each state In {OutputWriteState.CancelledByUser, OutputWriteState.RejectedBeforeWrite, OutputWriteState.FailedBeforeWrite}
            Assert.ThrowsException(Of ArgumentException)(Sub()
                                                            Dim invalid = New WriteResult(state, "code", "message", scope, True)
                                                        End Sub)
        Next
        Assert.ThrowsException(Of ArgumentException)(Sub()
                                                        Dim invalid = New WriteResult(OutputWriteState.PartiallyWritten, "code", "message", scope, False)
                                                    End Sub)
        Assert.ThrowsException(Of ArgumentException)(Sub()
                                                        Dim invalid = New WriteResult(OutputWriteState.Completed, "code", "message", scope, True, {"table-1"})
                                                    End Sub)
        Assert.ThrowsException(Of ArgumentException)(Sub()
                                                        Dim invalid = New WriteResult(OutputWriteState.PartiallyWritten, "code", "message", scope, True, {"table-1", "table-0"})
                                                    End Sub)
        Assert.ThrowsException(Of ArgumentException)(Sub()
                                                        Dim invalid = New WriteInspection(scope, OutputOccupancy.Empty, "token", {"table-1"})
                                                    End Sub)
    End Sub

    <TestMethod>
    Public Sub CompositeC10_SixRegionSnapshotConsentAndSeparators()
        Dim decision = TargetPlanner.Create(New SpreadsheetOutputTarget With {.Destination = SpreadsheetOutputDestination.NewWorksheet,
            .WorksheetName = "Composite", .StartRow = 10, .StartColumn = 3}, HostCapabilityProfiles.OfficeJsExcel("OfficeOnline"))
        Dim plan = CompositeOutputExample.Create(decision)
        Dim scope As New WriteIntent(plan.DocumentId, "new-sheet", decision.ToTarget(),
            plan.Items.Select(Function(p) New WriteRegion(p.Item.Id, p.Bounds)), plan.Reservations)
        CollectionAssert.AreEqual(New String() {"R10C3:R13C8", "R15C3:R15C8", "R17C3:R24C8", "R26C3:R26C8", "R28C3:R28C8", "R30C3:R31C5"},
            scope.Regions.Select(Function(r) r.Bounds.ToString()).ToArray())
        For Each row In {14, 16, 25, 27, 29}
            Assert.IsFalse(scope.Regions.Any(Function(r) r.Bounds.Contains(row, 3)))
        Next
        Dim ids = scope.Regions.Select(Function(r) r.ArtifactId).ToList()
        Dim observed As New WriteInspection(scope, OutputOccupancy.Occupied, "snapshot", ids, hasFormulas:=True)
        ids.Clear()
        Assert.AreEqual(6, observed.OccupiedRegionIds.Count)
        Dim writes As Integer = 0
        Dim actions = scope.Regions.Select(Function(r) CType(Sub() writes += 1, Action)).ToArray()
        Dim result = SafeWrite.Execute(scope, Function() observed, Function(o) False, actions)
        Assert.AreEqual(OutputWriteState.CancelledByUser, result.State) : Assert.AreEqual(0, writes)
    End Sub

    <TestMethod>
    Public Sub NonspatialAndZeroArea_NeverInvokeWriteOrClaimMutation()
        Dim scope As New WriteIntent("logical", "target", New SpreadsheetOutputTarget With {.WorksheetName = "Results"},
            {New WriteRegion("zero-area", Nothing), New WriteRegion("unrendered-plot", Nothing)})
        Dim result = SafeWrite.Execute(scope, Function() New WriteInspection(scope, OutputOccupancy.Empty, "empty", Nothing),
            Function(o) True, New Action() {Sub() Assert.Fail("zero-area write"), Sub() Assert.Fail("logical plot write")})
        Assert.AreEqual(OutputWriteState.Completed, result.State) : Assert.IsFalse(result.MutationMayHaveOccurred)
        Assert.AreEqual(0, result.ConfirmedWritten.Count)
    End Sub

    <TestMethod>
    Public Sub CreatedDestinationThenInspectionFailure_IsPartialEvenWithNoBlockWritten()
        Dim scope = Intent(SpreadsheetOutputDestination.NewWorksheet)
        Dim result = SafeWrite.Execute(scope, Function()
                                                 Throw New InvalidOperationException("host error")
                                                 Return Nothing
                                             End Function, Function(o) True,
            New Action() {Sub() Assert.Fail(), Sub() Assert.Fail()}, destinationCreated:=True)
        Assert.AreEqual(OutputWriteState.PartiallyWritten, result.State)
        Assert.IsTrue(result.MutationMayHaveOccurred) : Assert.AreEqual(0, result.ConfirmedWritten.Count)
        Assert.IsFalse(result.RollbackAttempted)
    End Sub
End Class
