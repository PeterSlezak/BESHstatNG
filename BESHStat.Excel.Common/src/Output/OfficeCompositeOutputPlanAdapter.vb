Option Explicit On
Option Strict On

Imports System
Imports System.Collections.Generic
Imports System.Linq

Namespace ExcelCommon
    ''' <summary>Separate composite wire v1. TablePlan remains the unchanged Office table wire v1.</summary>
    Public NotInheritable Class OfficeCompositeOutputItem
        Friend Sub New(placement As CompositeOutputPlacement, source As OutputDocument)
            Dim item = placement.Item
            Id = item.Id : Kind = item.Kind.ToString() : Index = placement.Index
            SourceIndex = placement.SourceIndex : ArtifactId = item.ArtifactId
            RelatedArtifactId = item.RelatedArtifactId : Bounds = placement.Bounds
            RowCount = item.RowCount : ColumnCount = item.ColumnCount
            Text = item.Text : LinkTarget = item.LinkTarget : State = placement.State
            If SourceIndex.HasValue Then
                Dim artifact = source.Artifacts(SourceIndex.Value)
                Caption = artifact.Caption
                If TypeOf artifact Is OutputDiagnosticArtifact Then
                    Dim diagnostic = DirectCast(artifact, OutputDiagnosticArtifact)
                    Severity = diagnostic.Severity.ToString() : Code = diagnostic.Code : FieldPath = diagnostic.FieldPath : Message = diagnostic.Message
                ElseIf TypeOf artifact Is OutputPlotArtifact Then
                    Dim plot = DirectCast(artifact, OutputPlotArtifact)
                    ReferenceKey = plot.ReferenceKey : Purpose = plot.Purpose : Description = plot.Description
                End If
            End If
        End Sub
        Public ReadOnly Property Id As String
        Public ReadOnly Property Kind As String
        Public ReadOnly Property Index As Integer
        Public ReadOnly Property SourceIndex As Integer?
        Public ReadOnly Property ArtifactId As String
        Public ReadOnly Property RelatedArtifactId As String
        Public ReadOnly Property Bounds As OutputRectangle
        Public ReadOnly Property RowCount As Integer
        Public ReadOnly Property ColumnCount As Integer
        Public ReadOnly Property Text As String
        Public ReadOnly Property LinkTarget As String
        Public ReadOnly Property State As String
        Public ReadOnly Property Caption As String
        Public ReadOnly Property Severity As String
        Public ReadOnly Property Code As String
        Public ReadOnly Property FieldPath As String
        Public ReadOnly Property Message As String
        Public ReadOnly Property ReferenceKey As String
        Public ReadOnly Property Purpose As String
        Public ReadOnly Property Description As String
    End Class

    Public NotInheritable Class OfficeCompositeOutputPlan
        Friend Sub New(plan As CompositeOutputPlan, tablePlan As OfficeOutputPlan)
            DocumentId = plan.DocumentId
            Dim layout As New OutputLayoutPlan(plan.Target.ToTarget(), plan.Options, New List(Of OutputBlockPlacement)(), Nothing, plan.Reservations)
            Target = New OfficeOutputTarget(layout)
            Me.TablePlan = tablePlan
            Items = plan.Items.Select(Function(p) New OfficeCompositeOutputItem(p, plan.SourceDocument)).ToList().AsReadOnly()
            Reservations = plan.Reservations : UsedBounds = plan.UsedBounds
        End Sub
        Public ReadOnly Property SchemaVersion As Integer = 1
        Public ReadOnly Property DocumentId As String
        Public ReadOnly Property Target As OfficeOutputTarget
        Public ReadOnly Property Items As IReadOnlyList(Of OfficeCompositeOutputItem)
        Public ReadOnly Property Reservations As IReadOnlyList(Of OutputRectangle)
        Public ReadOnly Property UsedBounds As OutputRectangle
        Public ReadOnly Property TablePlan As OfficeOutputPlan
    End Class

    Public NotInheritable Class OfficeCompositeOutputPlanAdapter
        Public Shared Function Prepare(plan As CompositeOutputPlan, semantics As OutputSemanticPlan) As OfficeCompositeOutputPlan
            If plan Is Nothing Then Throw New ArgumentNullException(NameOf(plan))
            If plan.Target.EffectiveDestination <> SpreadsheetOutputDestination.NewWorksheet Then Throw New ArgumentException("Composite live output supports new worksheets only until 02.08.")
            Dim limits As New InputSizeLimits()
            If plan.Items.Count > limits.MaxRanges OrElse plan.Reservations.Count > limits.MaxRanges Then Throw New ArgumentException("Composite item/reservation limit exceeded.")
            Dim cells As Long = 0
            Dim bytes As Long = 0
            Dim placements As New List(Of OutputBlockPlacement)()
            Dim tableBounds As New List(Of OutputRectangle)()
            For Each entry In plan.Items
                Dim item = entry.Item
                cells += CLng(item.RowCount) * item.ColumnCount
                bytes += 4096L + CLng(item.Text.Length) * 2L
                If cells > limits.MaxTotalCells OrElse bytes > limits.MaxEstimatedBytes Then Throw New ArgumentException("Composite transport memory limit exceeded.")
                If Not entry.SourceIndex.HasValue Then Continue For
                Dim artifact = plan.SourceDocument.Artifacts(entry.SourceIndex.Value)
                Dim table = TryCast(artifact, OutputTableArtifact)
                Dim bounds = If(table Is Nothing, Nothing, entry.Bounds)
                If bounds IsNot Nothing Then tableBounds.Add(bounds)
                placements.Add(New OutputBlockPlacement(artifact, entry.SourceIndex.Value,
                    If(table Is Nothing, 0, table.RowCount), If(table Is Nothing, 0, table.ColumnCount),
                    bounds, plan.Target.StartRow, plan.Target.StartColumn, 0))
            Next
            Dim target = plan.Target.ToTarget()
            Dim used As OutputRectangle = Nothing
            If tableBounds.Count > 0 Then
                used = New OutputRectangle(tableBounds.Min(Function(b) b.StartRow), tableBounds.Min(Function(b) b.StartColumn),
                    tableBounds.Max(Function(b) b.EndRow), tableBounds.Max(Function(b) b.EndColumn))
                target.StartRow = used.StartRow : target.StartColumn = used.StartColumn
            End If
            Dim layout As New OutputLayoutPlan(target, plan.Options, placements, used, plan.Reservations)
            ' Reuses complete scalar/type/provenance/semantic and per-table format validation.
            Dim tables = OfficeOutputPlanAdapter.Prepare(plan.SourceDocument, semantics, layout, autoFit:=False)
            Dim result As New OfficeCompositeOutputPlan(plan, tables)
            For Each item In result.Items
                For Each value In New String() {item.Id, item.Kind, item.ArtifactId, item.RelatedArtifactId, item.Text,
                    item.LinkTarget, item.Caption, item.Severity, item.Code, item.FieldPath, item.Message, item.ReferenceKey, item.Purpose, item.Description}
                    If value Is Nothing Then Continue For
                    CompositeText.Check(value, True)
                    bytes += 1024L + CLng(value.Length) * 2L
                Next
            Next
            For Each table In tables.Tables
                For Each cell In table.Cells
                    bytes += 1024L + CLng(cell.NumberFormat.Length) * 2L + If(cell.Value.TextValue Is Nothing, 0L, CLng(cell.Value.TextValue.Length) * 2L)
                Next
            Next
            If bytes > limits.MaxEstimatedBytes Then Throw New ArgumentException("Composite aggregate byte limit exceeded.")
            Return result
        End Function
    End Class
End Namespace
