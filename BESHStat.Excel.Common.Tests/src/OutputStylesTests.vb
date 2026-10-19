Option Explicit On
Option Strict On

Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports BESHStatNG
Imports BESHStatNG.ExcelCommon
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass>
Public Class OutputStylesTests
    <TestMethod>
    Public Sub Sections_HaveDeterministicRolesAndPValueOverlayKeepsBodyRole()
        Dim model As New ResultTableOutputModel(New Object(,) {
            {"Title one", Nothing, Nothing}, {"Title two", Nothing, Nothing},
            {Nothing, "Estimate", "P"}, {Nothing, "Second", "P"},
            {"A", 2, 0.04}, {"B", 3, 0.1}, {"Foot one", Nothing, Nothing}, {"Foot two", Nothing, Nothing}},
            titleRows:=2, headerTopRows:=2, headerLeftColumns:=1, footerRows:=2,
            pvalueColumns:=New Integer() {2}, pvalueCells:=New ResultTableCellAddress() {New ResultTableCellAddress(1, 2)})
        Dim plan As OutputSemanticPlan = OutputSemanticPlanner.Create(New OutputTableArtifact("mixed", model), Nothing, 0.05)
        Dim cells As IReadOnlyList(Of OutputCellStyle) = plan.Artifacts(0).Cells
        Assert.AreEqual(24, cells.Count)
        For index As Integer = 0 To cells.Count - 1
            Dim cell As OutputCellStyle = cells(index)
            Dim expected As OutputStyleRole = If(cell.Row < 2, OutputStyleRole.Title,
                If(cell.Row < 4, OutputStyleRole.TopColumnHeader,
                If(cell.Row >= 6, OutputStyleRole.Footer,
                If(cell.Column = 0, OutputStyleRole.LeftRowHeader, OutputStyleRole.Body))))
            Assert.AreEqual(expected, cell.Role)
            Assert.AreEqual(cell.Row * 3 + cell.Column, index)
            Assert.AreEqual(cell.Row >= 4 AndAlso cell.Row < 6 AndAlso cell.Column = 2, cell.IsPValue)
        Next
        Assert.IsTrue(cells(14).IsSignificant)
        Assert.IsTrue(cells(14).StrongEmphasis)
        Assert.AreEqual("significant", cells(14).AccessibilityIndicator)
        Assert.AreEqual(OutputStyleRole.Body, cells(14).Role)
        Assert.IsFalse(cells(17).IsSignificant)
        Assert.AreEqual(OutputNumberFormatKind.PValue, cells(14).NumberFormat.Kind)
        Assert.AreEqual(OutputNumberFormatKind.General, cells(13).NumberFormat.Kind)
        Assert.AreEqual("4:2", cells(14).LocalId)
        Assert.AreEqual("mixed", cells(14).ArtifactId)
    End Sub

    <TestMethod>
    Public Sub SemanticNumberFormats_ValidateTokensAndPrecision()
        For Each kind As OutputNumberFormatKind In New OutputNumberFormatKind() {
            OutputNumberFormatKind.Numeric, OutputNumberFormatKind.Percentage, OutputNumberFormatKind.Scientific, OutputNumberFormatKind.PValue}
            Dim intent As New OutputNumberFormatIntent(kind, 4)
            Assert.AreEqual(kind, intent.Kind)
            Assert.AreEqual(4, intent.DecimalPlaces.Value)
        Next
        Assert.ThrowsException(Of ArgumentOutOfRangeException)(Function() New OutputNumberFormatIntent(CType(99, OutputNumberFormatKind)))
        Assert.ThrowsException(Of ArgumentException)(Function() New OutputNumberFormatIntent(OutputNumberFormatKind.General, 2))
        Assert.ThrowsException(Of ArgumentOutOfRangeException)(Function() New OutputNumberFormatIntent(OutputNumberFormatKind.Numeric))
        Assert.ThrowsException(Of ArgumentOutOfRangeException)(Function() New OutputNumberFormatIntent(OutputNumberFormatKind.Scientific, -1))
        Assert.ThrowsException(Of ArgumentOutOfRangeException)(Function() New OutputNumberFormatIntent(OutputNumberFormatKind.Percentage, 17))
        Assert.ThrowsException(Of ArgumentOutOfRangeException)(Function() New OutputNumberFormatIntent(OutputNumberFormatKind.PValue, 1))
        Assert.AreEqual(0, New OutputNumberFormatIntent(OutputNumberFormatKind.Numeric, 0).DecimalPlaces.Value)
    End Sub

    <TestMethod>
    Public Sub EmptyAndHeaderOnlyTables_HaveNoPValuesOrInvalidIndexing()
        For Each values As Object(,) In New List(Of Object(,)) From {New Object(-1, -1) {}, New Object(-1, 2) {}, New Object(2, -1) {}}
            Dim artifact As New OutputTableArtifact("empty", New ResultTableOutputModel(values))
            Assert.AreEqual(0, OutputSemanticPlanner.Create(artifact, Nothing, 0.05).Artifacts(0).Cells.Count)
        Next
        Dim model As New ResultTableOutputModel(New Object(,) {{"Title"}, {"Header"}, {"Foot"}},
            titleRows:=1, headerTopRows:=1, footerRows:=1, pvalueColumns:=New Integer() {1},
            pvalueCells:=New ResultTableCellAddress() {New ResultTableCellAddress(1, 1)})
        Dim plan As OutputSemanticPlan = OutputSemanticPlanner.Create(New OutputTableArtifact("zero-body", model), Nothing, 0.05)
        Assert.IsTrue(plan.Artifacts(0).Cells.All(Function(c) Not c.IsPValue))
        Assert.AreEqual(2, plan.Diagnostics.Count)
        Dim noBodyColumns As New ResultTableOutputModel(New Object(,) {{0.04}}, headerLeftColumns:=1, pvalueColumns:=New Integer() {1})
        Assert.IsFalse(OutputSemanticPlanner.Create(New OutputTableArtifact("left-only", noBodyColumns), Nothing, 0.05).Artifacts(0).Cells(0).IsPValue)
    End Sub

    <TestMethod>
    Public Sub GenericArtifacts_KeepOrderSeverityAndImmutableCollections()
        Dim artifacts As New List(Of OutputArtifact) From {
            New OutputTableArtifact("first", New ResultTableOutputModel(New Object(,) {{1}}, isResultTable:=False)),
            New OutputDiagnosticArtifact("warn", OutputDiagnosticSeverity.Warning, "warning.test", "Warning"),
            New OutputPlotArtifact("plot", "logical-ref", "Preview"),
            New OutputTableArtifact("second", New ResultTableOutputModel(New Object(,) {{2}})),
            New OutputDiagnosticArtifact("fault", OutputDiagnosticSeverity.Fault, "fault.test", "Fault")}
        Dim document As New OutputDocument(artifacts)
        Dim plan As OutputSemanticPlan = OutputSemanticPlanner.Create(document, Nothing, 0.05)
        artifacts.Clear()
        CollectionAssert.AreEqual(New String() {"first", "warn", "plot", "second", "fault"}, plan.Artifacts.Select(Function(a) a.ArtifactId).ToArray())
        Assert.AreEqual(OutputStyleRole.DiagnosticWarning, plan.Artifacts(1).DiagnosticRole.Value)
        Assert.AreEqual("warning", plan.Artifacts(1).AccessibilityIndicator)
        Assert.AreEqual(OutputStyleRole.DiagnosticFault, plan.Artifacts(4).DiagnosticRole.Value)
        Assert.AreEqual("fault", plan.Artifacts(4).AccessibilityIndicator)
        Assert.IsNull(plan.Artifacts(2).DiagnosticRole)
        Assert.AreEqual(0, plan.Artifacts(2).Cells.Count)
        Assert.ThrowsException(Of NotSupportedException)(Sub() DirectCast(plan.Artifacts, IList(Of OutputArtifactStylePlan)).Clear())
        Assert.ThrowsException(Of NotSupportedException)(Sub() DirectCast(plan.Diagnostics, IList(Of OutputPlanDiagnostic)).Clear())
        Assert.ThrowsException(Of NotSupportedException)(Sub() DirectCast(plan.Artifacts(0).Cells, IList(Of OutputCellStyle)).Clear())
        For Each type As Type In New Type() {GetType(OutputCell), GetType(OutputCellStyle), GetType(OutputNumberFormatIntent), GetType(OutputPValuePolicy), GetType(OutputSemanticPlan), GetType(OutputArtifactStylePlan)}
            Assert.IsTrue(type.GetProperties().All(Function(p) Not p.CanWrite), type.Name)
        Next
    End Sub
End Class
