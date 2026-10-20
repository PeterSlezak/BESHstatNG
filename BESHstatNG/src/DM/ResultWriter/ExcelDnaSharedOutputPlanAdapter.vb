Option Explicit On
Option Strict On

Imports System
Imports System.Collections.Generic
Imports System.Globalization
Imports System.Linq
Imports BESHStatNG.ExcelCommon

' Bounded Windows bridge: no COM, live occupancy, or workbook-creation policy here.
Friend NotInheritable Class ExcelDnaSharedWriteCommand
    Private ReadOnly snapshot As OutputTableArtifact

    Friend Sub New(placement As OutputBlockPlacement, ordinal As Integer, request As ResultTableWriteRequest)
        ArtifactId = placement.ArtifactId
        SourceOrdinal = ordinal
        DocumentIndex = placement.Index
        Bounds = placement.Bounds
        Dim values(placement.RowCount - 1, placement.ColumnCount - 1) As Object
        For row As Integer = 0 To placement.RowCount - 1
            For column As Integer = 0 To placement.ColumnCount - 1
                values(row, column) = request.Values(row)(column)
            Next
        Next
        snapshot = New OutputTableArtifact(ArtifactId, New ResultTableOutputModel(values,
            request.HeaderTopRows, request.HeaderLeftColumns, request.FooterRows, request.PvalueColumns,
            request.TitleRows, request.IsResultTable,
            request.PvalueCells.Select(Function(p) New ResultTableCellAddress(p.BodyRow, p.BodyColumn))))
        PvalueNumberFormat = request.PvalueNumberFormat
        PvalueHighlightAlpha = request.PvalueHighlightAlpha
        AutoFit = request.AutoFit
    End Sub

    Public ReadOnly Property ArtifactId As String
    Public ReadOnly Property SourceOrdinal As Integer
    Public ReadOnly Property DocumentIndex As Integer
    Public ReadOnly Property Bounds As OutputRectangle
    Public ReadOnly Property PvalueNumberFormat As String
    Public ReadOnly Property PvalueHighlightAlpha As Double
    Public ReadOnly Property AutoFit As Boolean
    Public ReadOnly Property Model As ResultTableOutputModel
        Get
            Return snapshot.Model
        End Get
    End Property
End Class

Friend NotInheritable Class ExcelDnaPreparedSharedOutput
    Friend Sub New(layout As OutputLayoutPlan, commands As List(Of ExcelDnaSharedWriteCommand))
        Me.Layout = layout
        Me.Commands = New List(Of ExcelDnaSharedWriteCommand)(commands).AsReadOnly()
    End Sub

    Public ReadOnly Property Layout As OutputLayoutPlan
    Public ReadOnly Property Commands As IReadOnlyList(Of ExcelDnaSharedWriteCommand)

    ' The cursor marks the attempted block if execution fails. Prior successes stay written;
    ' even the failing block may have written values before formatting threw. No rollback.
    Public Sub Execute(target As SpreadsheetOutputTarget, worksheetName As String,
                       write As Action(Of ExcelDnaSharedWriteCommand), setRow As Action(Of Integer))
        ExcelDnaSharedOutputPlanAdapter.ValidateTarget(Layout, target, worksheetName)
        If write Is Nothing Then Throw New ArgumentNullException(NameOf(write))
        If setRow Is Nothing Then Throw New ArgumentNullException(NameOf(setRow))
        For Each command As ExcelDnaSharedWriteCommand In Commands
            setRow(command.Bounds.StartRow)
            write(command)
            setRow(command.Bounds.EndRow + 1)
        Next
    End Sub
End Class

Friend NotInheritable Class ExcelDnaSharedOutputPlanAdapter
    Friend Const MaxRows As Integer = 1048576
    Friend Const MaxColumns As Integer = 16384

    Private Sub New()
    End Sub

    ' Requests have no IDs in the legacy protocol. Production binds source ordinal N to
    ' table-N; custom documents must supply the exact ordered IDs explicitly. Compare the
    ' complete normalized payload/metadata as well, so stale/reordered requests fail closed.
    Public Shared Function Prepare(layout As OutputLayoutPlan, requests As IReadOnlyList(Of ResultTableWriteRequest),
                                   target As SpreadsheetOutputTarget, worksheetName As String,
                                   Optional tableIds As IReadOnlyList(Of String) = Nothing) As ExcelDnaPreparedSharedOutput
        ValidateTarget(layout, target, worksheetName)
        If requests Is Nothing Then Throw New ArgumentNullException(NameOf(requests))
        Dim tables = layout.Placements.Where(Function(p) p.Kind = OutputArtifactKind.Table).ToList()
        If tables.Count <> requests.Count OrElse (tableIds IsNot Nothing AndAlso tableIds.Count <> tables.Count) Then
            Throw New ArgumentException("Shared table/request identity count mismatch.")
        End If
        Dim commands As New List(Of ExcelDnaSharedWriteCommand)()
        Dim ids As New HashSet(Of String)(StringComparer.Ordinal)
        Dim ordinal As Integer = 0
        For index As Integer = 0 To layout.Placements.Count - 1
            Dim placement = layout.Placements(index)
            If placement.Index <> index OrElse Not ids.Add(placement.ArtifactId) Then Throw New ArgumentException("Invalid layout identity/order.")
            Select Case placement.Kind
                Case OutputArtifactKind.Diagnostic, OutputArtifactKind.Plot
                    If placement.IsPlaced Then Throw New ArgumentException("Nonspatial artifact has a cell placement.")
                    Continue For
                Case OutputArtifactKind.Table
                Case Else
                    Throw New ArgumentException("Unsupported output artifact.")
            End Select
            Dim id As String = If(tableIds Is Nothing, "table-" & ordinal.ToString(CultureInfo.InvariantCulture), tableIds(ordinal))
            If Not String.Equals(id, placement.ArtifactId, StringComparison.Ordinal) Then Throw New ArgumentException("Shared table ID/order mismatch.")
            Dim request = requests(ordinal)
            If request Is Nothing Then Throw New ArgumentException("Null shared request.")
            ValidateTarget(layout, request.OutputTarget, worksheetName)
            Dim table = DirectCast(placement.Artifact, OutputTableArtifact)
            Dim expected = ResultTableWriteRequestFactory.Create(table.Model, Nothing, 0.05, target)
            ValidatePayload(expected, request)
            If placement.RowCount > 0 AndAlso placement.ColumnCount > 0 Then
                Dim bounds = placement.Bounds
                If bounds Is Nothing OrElse bounds.StartColumn <> layout.StartColumn OrElse
                   CLng(bounds.EndRow) - bounds.StartRow + 1 <> placement.RowCount OrElse
                   CLng(bounds.EndColumn) - bounds.StartColumn + 1 <> placement.ColumnCount OrElse
                   bounds.EndRow >= MaxRows OrElse bounds.EndColumn > MaxColumns Then
                    ' Deliberately refuse a final-row write before any writes: the legacy cursor
                    ' must remain a valid worksheet row for subsequent tables/chart anchors.
                    Throw New ArgumentException("Shared bounds/shape invalid or no next worksheet row is available.")
                End If
                commands.Add(New ExcelDnaSharedWriteCommand(placement, ordinal, request))
            ElseIf placement.IsPlaced Then
                Throw New ArgumentException("Zero-area table has a cell placement.")
            End If
            ordinal += 1
        Next
        Return New ExcelDnaPreparedSharedOutput(layout, commands)
    End Function

    Friend Shared Sub ValidateTarget(layout As OutputLayoutPlan, target As SpreadsheetOutputTarget, worksheetName As String)
        If layout Is Nothing Then Throw New ArgumentNullException(NameOf(layout))
        If target Is Nothing Then Throw New ArgumentNullException(NameOf(target))
        If String.IsNullOrWhiteSpace(worksheetName) OrElse String.IsNullOrWhiteSpace(target.WorksheetName) OrElse
           Not String.Equals(worksheetName, layout.WorksheetName, StringComparison.Ordinal) OrElse
           Not String.Equals(target.WorksheetName, layout.WorksheetName, StringComparison.Ordinal) OrElse
           target.Destination <> layout.Destination OrElse target.StartRow <> layout.StartRow OrElse
           target.StartColumn <> layout.StartColumn OrElse target.StartRow < 1 OrElse target.StartRow > MaxRows OrElse
           target.StartColumn < 1 OrElse target.StartColumn > MaxColumns OrElse
           layout.Options.MaxRows > MaxRows OrElse layout.Options.MaxColumns > MaxColumns OrElse
           layout.Options.BlankRowsBetweenTables <> 1 Then
            Throw New ArgumentException("Shared output target/start/worksheet changed or is outside the Windows grid policy.")
        End If
    End Sub

    Private Shared Sub ValidatePayload(expected As ResultTableWriteRequest, actual As ResultTableWriteRequest)
        If actual.Values Is Nothing OrElse actual.Values.Count <> expected.Values.Count OrElse
           actual.HeaderTopRows <> expected.HeaderTopRows OrElse actual.HeaderLeftColumns <> expected.HeaderLeftColumns OrElse
           actual.TitleRows <> expected.TitleRows OrElse actual.FooterRows <> expected.FooterRows OrElse
           actual.IsResultTable <> expected.IsResultTable OrElse actual.PvalueColumns Is Nothing OrElse actual.PvalueCells Is Nothing OrElse
           Not actual.PvalueColumns.SequenceEqual(expected.PvalueColumns) OrElse actual.PvalueCells.Count <> expected.PvalueCells.Count Then
            Throw New ArgumentException("Shared request shape/metadata mismatch.")
        End If
        For row As Integer = 0 To expected.Values.Count - 1
            If actual.Values(row) Is Nothing OrElse actual.Values(row).Count <> expected.Values(row).Count Then Throw New ArgumentException("Shared request is not rectangular or has a different shape.")
            For column As Integer = 0 To expected.Values(row).Count - 1
                Dim left = expected.Values(row)(column)
                Dim right = actual.Values(row)(column)
                If Not Object.Equals(left, right) OrElse (left IsNot Nothing AndAlso right IsNot Nothing AndAlso left.GetType() <> right.GetType()) Then
                    Throw New ArgumentException("Shared request value/type/order mismatch.")
                End If
            Next
        Next
        For index As Integer = 0 To expected.PvalueCells.Count - 1
            Dim cell = actual.PvalueCells(index)
            If cell Is Nothing OrElse cell.BodyRow <> expected.PvalueCells(index).BodyRow OrElse cell.BodyColumn <> expected.PvalueCells(index).BodyColumn Then
                Throw New ArgumentException("Shared request body p-value annotation mismatch.")
            End If
        Next
    End Sub

    ' Conservative legacy AreaCheck envelope includes shared gaps and all supplementary
    ' matrices/gaps. Only the shared primary geometry comes from the canonical planner.
    Public Shared Function PreflightBounds(prepared As ExcelDnaPreparedSharedOutput,
                                           supplementary As IEnumerable(Of ResultTableOutputModel)) As OutputRectangle
        If prepared Is Nothing Then Throw New ArgumentNullException(NameOf(prepared))
        Return PreflightBounds(prepared.Layout, supplementary)
    End Function

    Friend Shared Function PreflightBounds(layout As OutputLayoutPlan,
                                           supplementary As IEnumerable(Of ResultTableOutputModel)) As OutputRectangle
        If layout Is Nothing Then Throw New ArgumentNullException(NameOf(layout))
        Dim used = layout.UsedBounds
        If used Is Nothing Then Throw New ArgumentException("This UI workflow requires occupied shared tables.")
        Dim rowLimit As Integer = Math.Min(MaxRows, layout.Options.MaxRows)
        Dim columnLimit As Integer = Math.Min(MaxColumns, layout.Options.MaxColumns)
        If used.EndRow >= rowLimit Then Throw New ArgumentException("Output leaves no next Windows cursor row.")
        Dim endRow As Long = used.EndRow
        Dim endColumn As Long = used.EndColumn
        If supplementary IsNot Nothing Then
            For Each model In supplementary
                If model Is Nothing OrElse model.RowCount < 1 OrElse model.ColumnCount < 1 Then Throw New ArgumentException("Invalid supplementary table.")
                endRow += 1L + model.RowCount
                endColumn = Math.Max(endColumn, CLng(used.StartColumn) + model.ColumnCount - 1L)
                If endRow >= rowLimit OrElse endColumn > columnLimit Then Throw New ArgumentException("Combined output exceeds the Windows grid or leaves no next cursor row.")
            Next
        End If
        Return New OutputRectangle(used.StartRow, used.StartColumn, CInt(endRow), CInt(endColumn))
    End Function

    Public Shared Function ConfirmOverwrite(occupied As Boolean, confirm As Func(Of Boolean)) As Boolean
        If confirm Is Nothing Then Throw New ArgumentNullException(NameOf(confirm))
        Return Not occupied OrElse confirm()
    End Function
End Class
