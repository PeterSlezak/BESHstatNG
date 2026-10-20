Option Explicit On
Option Strict On

Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Text.RegularExpressions

Namespace ExcelCommon
    Public Enum CompositeOutputKind
        Table = 1
        Diagnostic = 2
        PlotPlaceholder = 3
        Note = 4
        Link = 5
    End Enum

    ''' <summary>Additive opt-in intent; does not extend OutputArtifactKind or its v1 family.</summary>
    Public NotInheritable Class CompositeOutputItem
        Private Sub New(id As String, artifactId As String, kind As CompositeOutputKind,
                        rows As Integer, columns As Integer, text As String, relatedId As String, url As String)
            CompositeText.Check(id, False)
            Me.Id = id
            Me.ArtifactId = artifactId
            Me.Kind = kind
            RowCount = rows
            ColumnCount = columns
            Me.Text = text
            RelatedArtifactId = relatedId
            LinkTarget = url
        End Sub
        Public ReadOnly Property Id As String
        Public ReadOnly Property ArtifactId As String
        Public ReadOnly Property Kind As CompositeOutputKind
        Public ReadOnly Property RowCount As Integer
        Public ReadOnly Property ColumnCount As Integer
        Public ReadOnly Property Text As String
        Public ReadOnly Property RelatedArtifactId As String
        Public ReadOnly Property LinkTarget As String

        ''' <summary>Tables use captured matrix dimensions. Other artifacts need explicit positive
        ''' dimensions to become visible; 0 x 0 explicitly retains a nonspatial entry.</summary>
        Public Shared Function FromArtifact(artifact As OutputArtifact, Optional rows As Integer = 0,
                                           Optional columns As Integer = 0) As CompositeOutputItem
            If artifact Is Nothing Then Throw New ArgumentNullException(NameOf(artifact))
            Dim kind As CompositeOutputKind
            Dim text As String = String.Empty
            If TypeOf artifact Is OutputTableArtifact Then
                If rows <> 0 OrElse columns <> 0 Then Throw New ArgumentException("Table dimensions come from its matrix.")
                Dim table = DirectCast(artifact, OutputTableArtifact)
                rows = table.RowCount : columns = table.ColumnCount
                kind = CompositeOutputKind.Table
            ElseIf TypeOf artifact Is OutputDiagnosticArtifact Then
                Dim diagnostic = DirectCast(artifact, OutputDiagnosticArtifact)
                kind = CompositeOutputKind.Diagnostic
                text = diagnostic.Severity.ToString().ToUpperInvariant() & " [" & diagnostic.Code & "] " &
                    diagnostic.Message & If(diagnostic.FieldPath.Length = 0, "", " (" & diagnostic.FieldPath & ")")
            Else
                Dim plot = DirectCast(artifact, OutputPlotArtifact)
                kind = CompositeOutputKind.PlotPlaceholder
                Dim newline = Convert.ToChar(10).ToString()
                text = "CHART PLACEHOLDER (not rendered): " & plot.Purpose & newline &
                    plot.Caption & newline & plot.Description & newline & "Logical reference: " & plot.ReferenceKey
            End If
            CheckDimensions(rows, columns, kind <> CompositeOutputKind.Table)
            CompositeText.Check(text, True)
            Return New CompositeOutputItem(artifact.Id, artifact.Id, kind, rows, columns, text, Nothing, Nothing)
        End Function

        Public Shared Function Note(id As String, text As String, rows As Integer, columns As Integer,
                                    Optional relatedArtifactId As String = Nothing) As CompositeOutputItem
            CheckDimensions(rows, columns, False)
            If rows = 0 OrElse columns = 0 Then Throw New ArgumentException("Notes require positive dimensions.")
            CompositeText.Check(text, False)
            Return New CompositeOutputItem(id, Nothing, CompositeOutputKind.Note, rows, columns, text, relatedArtifactId, Nothing)
        End Function

        ''' <summary>HTTPS only. Hosts currently degrade to explicit literal linked text; never a formula.</summary>
        Public Shared Function Link(id As String, displayText As String, target As String, rows As Integer,
                                    columns As Integer, Optional relatedArtifactId As String = Nothing) As CompositeOutputItem
            ValidateLink(target)
            Dim noteItem = Note(id, displayText, rows, columns, relatedArtifactId)
            CompositeText.Check(displayText & " [link not active: " & target & "]", False)
            Return New CompositeOutputItem(noteItem.Id, Nothing, CompositeOutputKind.Link, rows, columns,
                displayText, relatedArtifactId, target)
        End Function

        Public Shared Sub ValidateLink(target As String)
            CompositeText.Check(target, False)
            Dim parsed As Uri = Nothing
            If target.Length > 2048 OrElse Not Regex.IsMatch(target, "\Ahttps://[A-Za-z0-9.-]+(?:[/?#][^\s\\]*)?\z") OrElse target.Contains("@") OrElse
               target.Any(Function(c) Char.IsWhiteSpace(c) OrElse Char.IsControl(c) OrElse c = "\"c) OrElse
               Not Uri.TryCreate(target, UriKind.Absolute, parsed) OrElse parsed.Scheme <> Uri.UriSchemeHttps OrElse
               String.IsNullOrEmpty(parsed.Host) OrElse parsed.UserInfo.Length <> 0 OrElse Not parsed.IsDefaultPort Then
                Throw New ArgumentException("Link target must be an absolute HTTPS URL without credentials, whitespace or custom ports.")
            End If
            If parsed.Host.Length > 253 OrElse Not Regex.IsMatch(parsed.Host,
                "\A(?:[A-Za-z0-9](?:[A-Za-z0-9-]{0,61}[A-Za-z0-9])?\.)+[A-Za-z0-9](?:[A-Za-z0-9-]{0,61}[A-Za-z0-9])?\z") Then
                Throw New ArgumentException("Link target requires a bounded DNS hostname with at least two labels.")
            End If
        End Sub

        Private Shared Sub CheckDimensions(rows As Integer, columns As Integer, paired As Boolean)
            If rows < 0 OrElse columns < 0 OrElse (paired AndAlso ((rows = 0) <> (columns = 0))) Then
                Throw New ArgumentException("Invalid composite dimensions; nonspatial intent is exactly 0 x 0.")
            End If
        End Sub
    End Class

    Friend NotInheritable Class CompositeText
        Public Shared Sub Check(value As String, allowEmpty As Boolean)
            If value Is Nothing OrElse value.Length > 32767 OrElse (Not allowEmpty AndAlso String.IsNullOrWhiteSpace(value)) OrElse
               value.Any(Function(c) Char.IsControl(c) AndAlso c <> Convert.ToChar(10) AndAlso c <> Convert.ToChar(13) AndAlso c <> Convert.ToChar(9)) Then
                Throw New ArgumentException("Composite text is missing, contains control characters or exceeds 32767 UTF-16 units.")
            End If
        End Sub
    End Class

    Public NotInheritable Class CompositeOutputPlacement
        Friend Sub New(item As CompositeOutputItem, index As Integer, sourceIndex As Integer?, bounds As OutputRectangle)
            Me.Item = item : Me.Index = index : Me.SourceIndex = sourceIndex : Me.Bounds = bounds
        End Sub
        Public ReadOnly Property Item As CompositeOutputItem
        Public ReadOnly Property Index As Integer
        Public ReadOnly Property SourceIndex As Integer?
        ''' <summary>Positive-area owned cells, including visible placeholders; Nothing means nonspatial.</summary>
        Public ReadOnly Property Bounds As OutputRectangle
        Public ReadOnly Property State As String
            Get
                Return If(Bounds Is Nothing, "nonspatial", If(Item.Kind = CompositeOutputKind.PlotPlaceholder,
                    "placeholder-not-rendered", If(Item.Kind = CompositeOutputKind.Link, "linked-text-only", "visible")))
            End Get
        End Property
    End Class

    Public NotInheritable Class CompositeOutputPlan
        Friend Sub New(id As String, source As OutputDocument, decision As OutputTargetDecision,
                       options As OutputLayoutOptions, items As List(Of CompositeOutputPlacement),
                       used As OutputRectangle, reservations As List(Of OutputRectangle))
            DocumentId = id : SourceDocument = source : Target = decision : Me.Options = options
            Me.Items = items.AsReadOnly() : UsedBounds = used : Me.Reservations = reservations.AsReadOnly()
            OccupiedRectangles = items.Where(Function(p) p.Bounds IsNot Nothing).Select(Function(p) p.Bounds).ToList().AsReadOnly()
        End Sub
        Public ReadOnly Property DocumentId As String
        Public ReadOnly Property SourceDocument As OutputDocument
        Public ReadOnly Property Target As OutputTargetDecision
        Public ReadOnly Property Options As OutputLayoutOptions
        Public ReadOnly Property Items As IReadOnlyList(Of CompositeOutputPlacement)
        Public ReadOnly Property UsedBounds As OutputRectangle
        Public ReadOnly Property Reservations As IReadOnlyList(Of OutputRectangle)
        ''' <summary>Use these rectangles for host checks, never the envelope containing gaps.</summary>
        Public ReadOnly Property OccupiedRectangles As IReadOnlyList(Of OutputRectangle)
    End Class

    Public NotInheritable Class CompositeOutputPlanner
        Public Shared Function Create(documentId As String, document As OutputDocument, items As IEnumerable(Of CompositeOutputItem),
                                      decision As OutputTargetDecision, Optional options As OutputLayoutOptions = Nothing,
                                      Optional reservations As IEnumerable(Of OutputRectangle) = Nothing) As CompositeOutputPlan
            CompositeText.Check(documentId, False)
            If document Is Nothing OrElse items Is Nothing OrElse decision Is Nothing Then Throw New ArgumentNullException("composite")
            decision.EnsureSupported()
            Dim limits = If(options, New OutputLayoutOptions())
            If limits.MaxRows > 1048576 OrElse limits.MaxColumns > 16384 OrElse
               decision.StartRow > limits.MaxRows OrElse decision.StartColumn > limits.MaxColumns Then Throw New ArgumentException("Composite grid exceeds Excel or target limits.")
            limits = New OutputLayoutOptions(limits.BlankRowsBetweenTables, Math.Min(limits.MaxRows, decision.Facts.MaxRows), Math.Min(limits.MaxColumns, decision.Facts.MaxColumns))
            Dim occupied As New List(Of OutputRectangle)()
            If reservations IsNot Nothing Then
                For Each rectangle In reservations
                    If rectangle Is Nothing OrElse rectangle.EndRow > limits.MaxRows OrElse rectangle.EndColumn > limits.MaxColumns Then Throw New ArgumentException("Invalid reservation.")
                    occupied.Add(rectangle)
                Next
            End If
            Dim ids As New HashSet(Of String)(StringComparer.Ordinal)
            Dim placed As New List(Of CompositeOutputPlacement)()
            Dim sourceIndex As Integer = 0
            Dim previous As Long = 0
            Dim right As Integer = decision.StartColumn
            For Each item In items
                If item Is Nothing Then Throw New ArgumentException("Null composite item.")
                If Not ids.Add(item.Id) Then Throw New ArgumentException("Duplicate composite item ID.")
                If item.RowCount > limits.MaxRows OrElse item.ColumnCount > limits.MaxColumns Then Throw New ArgumentOutOfRangeException("dimensions", "Composite dimensions exceed the configured grid.")
                Dim referenceIndex As Integer? = Nothing
                If item.ArtifactId IsNot Nothing Then
                    If sourceIndex >= document.Artifacts.Count OrElse document.Artifacts(sourceIndex).Id <> item.ArtifactId Then Throw New ArgumentException("Missing/reordered artifact reference.")
                    Dim expected = CompositeOutputItem.FromArtifact(document.Artifacts(sourceIndex),
                        If(item.Kind = CompositeOutputKind.Table, 0, item.RowCount), If(item.Kind = CompositeOutputKind.Table, 0, item.ColumnCount))
                    If item.Kind <> expected.Kind OrElse item.Text <> expected.Text OrElse item.RowCount <> expected.RowCount OrElse
                       item.ColumnCount <> expected.ColumnCount Then Throw New ArgumentException("Stale composite source reference.")
                    referenceIndex = sourceIndex : sourceIndex += 1
                ElseIf item.RelatedArtifactId IsNot Nothing AndAlso Not document.Artifacts.Any(Function(a) a.Id = item.RelatedArtifactId) Then
                    Throw New ArgumentException("Missing annotation anchor.")
                End If
                Dim bounds As OutputRectangle = Nothing
                If item.RowCount > 0 AndAlso item.ColumnCount > 0 Then
                    bounds = OutputSpatialGeometry.NextBounds(decision.StartRow, decision.StartColumn, previous,
                        item.RowCount, item.ColumnCount, limits, occupied)
                    previous = bounds.EndRow : right = Math.Max(right, bounds.EndColumn)
                End If
                placed.Add(New CompositeOutputPlacement(item, placed.Count, referenceIndex, bounds))
            Next
            If sourceIndex <> document.Artifacts.Count Then Throw New ArgumentException("Every source artifact must have an explicit composite entry.")
            Dim used As OutputRectangle = Nothing
            If previous > 0 Then used = New OutputRectangle(decision.StartRow, decision.StartColumn, CInt(previous), right)
            Return New CompositeOutputPlan(documentId, document, decision, limits, placed, used, occupied)
        End Function
    End Class

    ''' <summary>Shared stack arithmetic for default table layouts and opt-in composite layouts.</summary>
    Friend NotInheritable Class OutputSpatialGeometry
        Public Shared Function NextBounds(startRow As Integer, startColumn As Integer, previousEnd As Long,
                                          rows As Integer, columns As Integer, limits As OutputLayoutOptions,
                                          reservations As IEnumerable(Of OutputRectangle)) As OutputRectangle
            Dim first As Long = If(previousEnd = 0, CLng(startRow), previousEnd + 1L + limits.BlankRowsBetweenTables)
            Dim last As Long = first + CLng(rows) - 1L
            Dim right As Long = CLng(startColumn) + CLng(columns) - 1L
            If rows <= 0 OrElse columns <= 0 OrElse last > limits.MaxRows OrElse right > limits.MaxColumns Then Throw New ArgumentOutOfRangeException("dimensions", "Output rectangle exceeds the configured grid.")
            Dim bounds As New OutputRectangle(CInt(first), startColumn, CInt(last), CInt(right))
            For Each reservation In reservations
                If bounds.Intersects(reservation) Then Throw New InvalidOperationException("Output rectangle collides with reservation: " & bounds.ToString())
            Next
            Return bounds
        End Function
    End Class
End Namespace
