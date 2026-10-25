Option Explicit On
Option Strict On

Imports BESHStatNG.ExcelCommon
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass>
Public Class OfficeDiagnosticCompatibilityTests
    <DataTestMethod>
    <DataRow("output.sheet-name-adjusted", DiagnosticSeverity.Warning)>
    <DataRow("diagnostic.host.unavailable", DiagnosticSeverity.Warning)>
    <DataRow("diagnostic.host.ready", DiagnosticSeverity.Information)>
    <DataRow("diagnostic.host.retry", DiagnosticSeverity.Information)>
    <DataRow("diagnostic.user.cancelled", DiagnosticSeverity.Information)>
    <DataRow("diagnostic.output.consent", DiagnosticSeverity.Information)>
    <DataRow("diagnostic.analysis.completed", DiagnosticSeverity.Information)>
    Public Sub OfficeCodesAreAdditiveAndRedactAllSourceMetadata(code As String, severity As DiagnosticSeverity)
        Dim record = DiagnosticRecord.Create(code, "office", Guid.NewGuid(), "output", metadata:=New Dictionary(Of String, String) From {
            {"host", "office"}, {"RequestedName", "PRIVATE"}, {"ResolvedName", "PRIVATE2"}, {"range", "PRIVATE!A1"}})
        Assert.AreEqual(code, record.EventCode)
        Assert.AreEqual(severity, record.Severity)
        Assert.AreEqual("office", record.Operation)
        Assert.AreEqual(1, record.Metadata.Count)
        Assert.AreEqual("office", record.Metadata("host"))
    End Sub

    <TestMethod>
    Public Sub ExistingUnknownCodesAndOperationsStillFailClosed()
        Dim record = DiagnosticRecord.Create("output.PRIVATE", "PRIVATE", Guid.Empty, "PRIVATE")
        Assert.AreEqual("diagnostic.event.redacted", record.EventCode)
        Assert.AreEqual("redacted", record.Operation)
        Assert.AreEqual("redacted", record.Stage)
    End Sub
End Class
