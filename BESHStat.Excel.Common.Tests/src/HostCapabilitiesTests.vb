Option Explicit On
Option Strict On

Imports System
Imports System.Collections.Generic
Imports System.Text.Json
Imports BESHStatNG.ExcelCommon
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass>
Public Class HostCapabilitiesTests

    Private Shared Function Target(destination As SpreadsheetOutputDestination) As SpreadsheetOutputTarget
        Return New SpreadsheetOutputTarget With {
            .Destination = destination,
            .StartRow = 2,
            .StartColumn = 3,
            .WorksheetName = "Results"
        }
    End Function

    <TestMethod>
    Public Sub WindowsHost_AllThreeOutputDestinationsSupported()
        Dim host As HostCapabilitySnapshot = HostCapabilityProfiles.WindowsExcelDna()
        Assert.AreEqual("ExcelDna", host.Host)
        Assert.IsTrue(host.Supports(HostCapabilityIds.ReadSelectedRange))
        Assert.IsTrue(host.Supports(HostCapabilityIds.WriteResultTables))
        For Each destination As SpreadsheetOutputDestination In New SpreadsheetOutputDestination() {
            SpreadsheetOutputDestination.CurrentWorksheet,
            SpreadsheetOutputDestination.NewWorksheet,
            SpreadsheetOutputDestination.NewWorkbook
        }
            Assert.IsTrue(HostCapabilityPreflight.CheckRunAndWrite(AnalysisIds.OneWayAnova, host, Target(destination)).IsSupported)
        Next
    End Sub

    <TestMethod>
    Public Sub OfficeHost_SupportsOnlyCurrentAndNewWorksheet()
        Dim host As HostCapabilitySnapshot = HostCapabilityProfiles.OfficeJsExcel("Web")
        Assert.IsTrue(HostCapabilityPreflight.CheckRunAndWrite(AnalysisIds.KruskalWallis, host,
            Target(SpreadsheetOutputDestination.CurrentWorksheet)).IsSupported)
        Assert.IsTrue(HostCapabilityPreflight.CheckRunAndWrite(AnalysisIds.KruskalWallis, host,
            Target(SpreadsheetOutputDestination.NewWorksheet)).IsSupported)
        Dim check As CapabilityPreflightResult = HostCapabilityPreflight.CheckRunAndWrite(AnalysisIds.OneWayAnova,
            host, Target(SpreadsheetOutputDestination.NewWorkbook))
        Assert.IsFalse(check.IsSupported)
        Assert.AreEqual(CapabilityPreflightCodes.UnsupportedDestination, check.Code)
        Assert.AreEqual(HostCapabilityIds.NewWorkbook, check.MissingCapability)
        Dim ex As HostCapabilityException = Assert.ThrowsException(Of HostCapabilityException)(Sub() check.EnsureSupported())
        Assert.AreEqual(check.Code, ex.Code)
        Assert.AreEqual(check.MissingCapability, ex.MissingCapability)
    End Sub

    <TestMethod>
    Public Sub CapabilityCheck_IsCaseSensitive_AndDoesNotModifyHostSnapshot()
        Dim host As HostCapabilitySnapshot = HostCapabilityProfiles.OfficeJsExcel("Web")
        Dim originalCount As Integer = host.Capabilities.Count
        Assert.IsFalse(host.Supports("OUTPUT.NEW-WORKSHEET"))
        Assert.IsFalse(host.Supports(Nothing))
        Assert.IsTrue(HostCapabilityPreflight.CheckOutput(host, Target(SpreadsheetOutputDestination.NewWorksheet)).IsSupported)
        Assert.AreEqual(originalCount, host.Capabilities.Count)
        Assert.IsFalse(host.Supports(HostCapabilityIds.NewWorkbook))
    End Sub

    <TestMethod>
    Public Sub Preflight_MissingReadCapabilityRejectedBeforeCalculation()
        Dim host As HostCapabilitySnapshot = HostCapabilityProfiles.OfficeJsExcel("Web")
        host.Capabilities.Remove(HostCapabilityIds.ReadSelectedRange)
        Dim check As CapabilityPreflightResult = HostCapabilityPreflight.CheckRunAndWrite(AnalysisIds.OneWayAnova,
            host, Target(SpreadsheetOutputDestination.NewWorksheet))
        Assert.IsFalse(check.IsSupported)
        Assert.AreEqual(CapabilityPreflightCodes.MissingCapability, check.Code)
        Assert.AreEqual(HostCapabilityIds.ReadSelectedRange, check.MissingCapability)
        Assert.IsTrue(HostCapabilityPreflight.CheckRunAndWrite(AnalysisIds.OneWayAnova,
            host, Target(SpreadsheetOutputDestination.NewWorksheet), False).IsSupported)
    End Sub

    <TestMethod>
    Public Sub Preflight_MissingWriteCapabilityRejected()
        Dim host As HostCapabilitySnapshot = HostCapabilityProfiles.OfficeJsExcel("Web")
        host.Capabilities.Remove(HostCapabilityIds.WriteResultTables)
        Dim check As CapabilityPreflightResult = HostCapabilityPreflight.CheckOutput(host,
            Target(SpreadsheetOutputDestination.CurrentWorksheet))
        Assert.IsFalse(check.IsSupported)
        Assert.AreEqual(CapabilityPreflightCodes.MissingCapability, check.Code)
        Assert.AreEqual(HostCapabilityIds.WriteResultTables, check.MissingCapability)
    End Sub

    <TestMethod>
    Public Sub Preflight_NonExcelAndNullHostsFailClosed()
        Dim check As CapabilityPreflightResult = HostCapabilityPreflight.CheckSelection(Nothing)
        Assert.AreEqual(CapabilityPreflightCodes.HostUnavailable, check.Code)
        Dim blank As New HostCapabilitySnapshot()
        blank.Capabilities.Add(HostCapabilityIds.ReadSelectedRange)
        Assert.IsFalse(blank.Supports(HostCapabilityIds.ReadSelectedRange))
        Assert.AreEqual(CapabilityPreflightCodes.HostUnavailable, HostCapabilityPreflight.CheckSelection(blank).Code)
        Assert.AreEqual(CapabilityPreflightCodes.HostUnavailable,
            HostCapabilityPreflight.CheckOutput(blank, Target(SpreadsheetOutputDestination.CurrentWorksheet)).Code)
    End Sub

    <TestMethod>
    Public Sub Preflight_RejectsInvalidTargetsWithoutMutatingThem()
        Dim host As HostCapabilitySnapshot = HostCapabilityProfiles.WindowsExcelDna()
        Assert.AreEqual(CapabilityPreflightCodes.InvalidTarget, HostCapabilityPreflight.CheckOutput(host, Nothing).Code)
        Dim targetValue As SpreadsheetOutputTarget = Target(SpreadsheetOutputDestination.NewWorksheet)
        targetValue.StartRow = 0
        Assert.AreEqual(CapabilityPreflightCodes.InvalidTarget, HostCapabilityPreflight.CheckOutput(host, targetValue).Code)
        Assert.AreEqual(0, targetValue.StartRow)
        targetValue.StartRow = 2
        targetValue.StartColumn = -1
        Assert.AreEqual(CapabilityPreflightCodes.InvalidTarget, HostCapabilityPreflight.CheckOutput(host, targetValue).Code)
        targetValue.StartColumn = 1
        targetValue.Destination = CType(999, SpreadsheetOutputDestination)
        Assert.AreEqual(CapabilityPreflightCodes.InvalidTarget, HostCapabilityPreflight.CheckOutput(host, targetValue).Code)
    End Sub

    <TestMethod>
    Public Sub Preflight_RejectsUnknownAnalysisWithoutInvokingHandler()
        Dim host As HostCapabilitySnapshot = HostCapabilityProfiles.WindowsExcelDna()
        Dim check As CapabilityPreflightResult = HostCapabilityPreflight.CheckRunAndWrite("not.registered", host,
            Target(SpreadsheetOutputDestination.NewWorksheet))
        Assert.IsFalse(check.IsSupported)
        Assert.AreEqual(CapabilityPreflightCodes.UnknownAnalysis, check.Code)
        Assert.AreEqual(CapabilityPreflightCodes.UnknownAnalysis,
            HostCapabilityPreflight.CheckAnalysis(Nothing, host).Code)
    End Sub

    <TestMethod>
    Public Sub DescriptorRequirements_RemainPureComputationCapabilities()
        For Each descriptor As IAnalysisDescriptor In AnalysisCatalog.All
            Assert.AreEqual(0, descriptor.RequiredCapabilities.Count,
                "Neither of the existing algorithms intrinsically requires a host operation.")
            Assert.IsTrue(HostCapabilityPreflight.CheckAnalysis(descriptor.Id, HostCapabilityProfiles.OfficeJsExcel("Web")).IsSupported)
        Next
    End Sub

    <TestMethod>
    Public Sub HostSnapshot_OfficeJsJsonRoundTrip()
        Dim host As HostCapabilitySnapshot = HostCapabilityProfiles.OfficeJsExcel("Mac")
        Dim json As String = JsonSerializer.Serialize(host)
        Dim restored As HostCapabilitySnapshot = JsonSerializer.Deserialize(Of HostCapabilitySnapshot)(json)
        Assert.IsNotNull(restored)
        Assert.AreEqual("OfficeJs", restored.Host)
        Assert.AreEqual("Mac", restored.Platform)
        Assert.IsTrue(restored.Supports(HostCapabilityIds.NewWorksheet))
        Assert.IsFalse(restored.Supports(HostCapabilityIds.NewWorkbook))
        Assert.AreEqual(host.Capabilities.Count, restored.Capabilities.Count)
    End Sub

    <TestMethod>
    Public Sub MissingCapabilityList_FailsClosed()
        Dim host As New HostCapabilitySnapshot With {
            .Host = "OfficeJs", .Platform = "Unknown", .IsExcel = True, .Capabilities = Nothing
        }
        Assert.IsFalse(host.Supports(HostCapabilityIds.ReadSelectedRange))
        Dim check As CapabilityPreflightResult = HostCapabilityPreflight.CheckSelection(host)
        Assert.AreEqual(CapabilityPreflightCodes.MissingCapability, check.Code)
    End Sub
End Class
