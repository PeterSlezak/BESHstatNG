Option Explicit On
Option Strict On

Imports BESHStatNG
Imports BESHStatNG.ExcelCommon
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass>
Public Class OutputTargetPlannerTests
    Private Shared Function Target(Optional destination As Integer = 0, Optional row As Integer = 10,
                                   Optional column As Integer = 3) As SpreadsheetOutputTarget
        Return New SpreadsheetOutputTarget With {.Destination = CType(destination, SpreadsheetOutputDestination),
            .WorksheetName = "Requested", .StartRow = row, .StartColumn = column}
    End Function

    <DataTestMethod>
    <DataRow(False, 0, True)> <DataRow(False, 1, True)> <DataRow(False, 2, True)>
    <DataRow(True, 0, True)> <DataRow(True, 1, True)> <DataRow(True, 2, False)>
    Public Sub Matrix_NoImplicitFallback(office As Boolean, destination As Integer, supported As Boolean)
        Dim host = If(office, HostCapabilityProfiles.OfficeJsExcel("OfficeOnline"), HostCapabilityProfiles.WindowsExcelDna())
        Dim decision = TargetPlanner.Create(Target(destination), host)
        Assert.AreEqual(supported, decision.Result.IsSupported)
        Assert.AreEqual(destination, CInt(decision.EffectiveDestination))
        Assert.AreEqual(HostCapabilityIds.ForDestination(CType(destination, SpreadsheetOutputDestination)), decision.SelectedCapability)
        If Not supported Then
            Assert.AreEqual("output.destination-unsupported", decision.Result.Code)
            Assert.ThrowsException(Of HostCapabilityException)(Function() decision.ToTarget())
        End If
    End Sub

    <DataTestMethod>
    <DataRow("missing-host", "host.unavailable")> <DataRow("nonspreadsheet", "host.unavailable")>
    <DataRow("unnamed", "host.unavailable")> <DataRow("write", "host.capability-missing")>
    <DataRow("destination", "output.destination-unsupported")> <DataRow("claims", "output.destination-unsupported")>
    <DataRow("unknown", "output.target-invalid")> <DataRow("null-target", "output.target-invalid")>
    Public Sub InvalidCapabilities_HaveStableActionableFailures(reason As String, code As String)
        Dim host = HostCapabilityProfiles.OfficeJsExcel("OfficeOnline")
        Dim requested = Target()
        Select Case reason
            Case "missing-host" : host = Nothing
            Case "nonspreadsheet" : host.IsExcel = False : host.IsSpreadsheetHost = False
            Case "unnamed" : host.Host = " "
            Case "write" : host.Capabilities.Remove(HostCapabilityIds.WriteResultTables)
            Case "destination" : host.Capabilities.Remove(HostCapabilityIds.CurrentWorksheet)
            Case "claims" : requested.Destination = SpreadsheetOutputDestination.NewWorkbook : host.Capabilities.Add(HostCapabilityIds.NewWorkbook)
            Case "unknown" : requested.Destination = CType(99, SpreadsheetOutputDestination)
            Case "null-target" : requested = Nothing
        End Select
        Dim decision = TargetPlanner.Create(requested, host)
        Assert.AreEqual(code, decision.Result.Code)
        Assert.IsFalse(String.IsNullOrWhiteSpace(decision.Result.Message))
    End Sub

    <DataTestMethod>
    <DataRow(0, 3)> <DataRow(-1, 3)> <DataRow(10, 0)> <DataRow(10, -1)>
    <DataRow(1048577, 3)> <DataRow(10, 16385)> <DataRow(Integer.MaxValue, 3)>
    Public Sub IllegalStart_IsNeverRepaired(row As Integer, column As Integer)
        Dim requested = Target(row:=row, column:=column)
        Dim decision = TargetPlanner.Create(requested, HostCapabilityProfiles.WindowsExcelDna())
        Assert.AreEqual("output.target-invalid", decision.Result.Code)
        Assert.AreEqual(row, requested.StartRow)
        Assert.AreEqual(column, requested.StartColumn)
        Assert.AreEqual(row, decision.StartRow)
        Assert.AreEqual(column, decision.StartColumn)
    End Sub

    <DataTestMethod>
    <DataRow("workbook", "output.target-invalid")> <DataRow("sheet", "output.target-invalid")>
    <DataRow("ambiguous", "output.target-invalid")> <DataRow("name", "output.target-invalid")>
    <DataRow("collision", "output.sheet-name-collision")> <DataRow("protected", "output.target-unwritable")>
    <DataRow("writable", "output.target-unwritable")> <DataRow("create", "output.target-unwritable")>
    <DataRow("merged", "output.target-merged")> <DataRow("grid", "output.target-invalid")>
    Public Sub KnownHostFacts_RejectWithoutConfusingNamesWithOccupancy(reason As String, code As String)
        Dim facts As OutputTargetFacts = Nothing
        Dim requested = Target()
        Select Case reason
            Case "workbook" : facts = New OutputTargetFacts(workbookAvailable:=False)
            Case "sheet" : facts = New OutputTargetFacts(sheetExists:=False)
            Case "ambiguous" : facts = New OutputTargetFacts(identityUnambiguous:=False, resolvedWorksheetName:="Requested")
            Case "name" : facts = New OutputTargetFacts(worksheetNameValid:=False)
            Case "collision" : requested.Destination = SpreadsheetOutputDestination.NewWorksheet : facts = New OutputTargetFacts(newWorksheetNameCollision:=True)
            Case "protected" : facts = New OutputTargetFacts(protectedSheet:=True)
            Case "writable" : facts = New OutputTargetFacts(writable:=False)
            Case "create" : requested.Destination = SpreadsheetOutputDestination.NewWorksheet : facts = New OutputTargetFacts(canCreateWorksheet:=False)
            Case "merged" : facts = New OutputTargetFacts(mergedConflict:=True)
            Case "grid" : facts = New OutputTargetFacts(maxColumns:=2)
        End Select
        Assert.AreEqual(code, TargetPlanner.Create(requested, HostCapabilityProfiles.WindowsExcelDna(), facts).Result.Code)
    End Sub

    <TestMethod>
    Public Sub UnknownFactsAndResolvedNames_AreNotClaimsOfSafetyOrIdentity()
        Dim decision = TargetPlanner.Create(Target(), HostCapabilityProfiles.WindowsExcelDna())
        Assert.IsTrue(decision.Result.IsSupported)
        Assert.IsFalse(decision.Facts.Writable.HasValue)
        Assert.IsFalse(decision.Facts.SheetExists.HasValue)
        Assert.IsFalse(decision.Facts.IdentityUnambiguous.HasValue)
        Dim resolved = TargetPlanner.Create(Target(1), HostCapabilityProfiles.WindowsExcelDna(),
            New OutputTargetFacts(resolvedWorksheetName:="Requested_2", newWorksheetNameCollision:=False))
        Assert.AreEqual("Requested", resolved.RequestedWorksheetName)
        Assert.AreEqual("Requested_2", resolved.WorksheetName)
        Assert.IsFalse(resolved.Facts.IdentityUnambiguous.HasValue)
    End Sub

    <TestMethod>
    Public Sub Snapshot_IsIsolatedFromRequestHostAndReturnedDtoMutation()
        Dim requested = Target(1)
        Dim host = HostCapabilityProfiles.OfficeJsExcel("OfficeOnline")
        Dim decision = TargetPlanner.Create(requested, host)
        requested.StartRow = 1 : requested.StartColumn = 1 : requested.WorksheetName = "Changed"
        host.Host = "Changed" : host.Capabilities.Clear()
        Dim exported = decision.ToTarget()
        exported.StartColumn = 9
        Assert.AreEqual(10, decision.StartRow)
        Assert.AreEqual(3, decision.ToTarget().StartColumn)
        Assert.AreEqual("Requested", decision.WorksheetName)
        Assert.AreEqual("OfficeJs", decision.HostName)
    End Sub

    <DataTestMethod>
    <DataRow(10, 3)> <DataRow(10, 1)> <DataRow(1048576, 16384)>
    Public Sub Coordinates_SurvivePlannerLayoutAndOfficeAdapter(row As Integer, column As Integer)
        Dim decision = TargetPlanner.Create(Target(1, row, column), HostCapabilityProfiles.OfficeJsExcel("OfficeOnline"))
        Dim document = OutputDocument.FromModel(New ResultTableOutputModel(New Object(,) {{0.01R}}))
        Dim semantics = OutputSemanticPlanner.Create(document, Nothing, 0.05)
        Dim layout = OutputLayoutPlanner.Create(document, decision.ToTarget(), semanticPlan:=semantics)
        Dim wire = OfficeOutputPlanAdapter.Prepare(document, semantics, layout)
        Assert.AreEqual(row, wire.Target.StartRow)
        Assert.AreEqual(column, wire.Target.StartColumn)
        Assert.AreEqual(row, wire.Tables(0).Bounds.StartRow)
        Assert.AreEqual(column, wire.Tables(0).Bounds.StartColumn)
    End Sub
End Class
