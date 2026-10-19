Option Explicit On
Option Strict On

Imports System
Imports System.Collections.Generic
Imports System.Globalization

Namespace ExcelCommon
    ''' <summary>Positive-area worksheet rectangle; all coordinates are one-based, ends inclusive.</summary>
    Public NotInheritable Class OutputRectangle
        Public Sub New(startRow As Integer, startColumn As Integer, endRow As Integer, endColumn As Integer)
            If startRow < 1 Then Throw New ArgumentOutOfRangeException(NameOf(startRow))
            If startColumn < 1 Then Throw New ArgumentOutOfRangeException(NameOf(startColumn))
            If endRow < startRow Then Throw New ArgumentOutOfRangeException(NameOf(endRow), "End row must include the start row.")
            If endColumn < startColumn Then Throw New ArgumentOutOfRangeException(NameOf(endColumn), "End column must include the start column.")
            Me.StartRow = startRow
            Me.StartColumn = startColumn
            Me.EndRow = endRow
            Me.EndColumn = endColumn
        End Sub

        Public ReadOnly Property StartRow As Integer
        Public ReadOnly Property StartColumn As Integer
        Public ReadOnly Property EndRow As Integer
        Public ReadOnly Property EndColumn As Integer

        Public Function Intersects(other As OutputRectangle) As Boolean
            If other Is Nothing Then Throw New ArgumentNullException(NameOf(other))
            Return StartRow <= other.EndRow AndAlso other.StartRow <= EndRow AndAlso
                StartColumn <= other.EndColumn AndAlso other.StartColumn <= EndColumn
        End Function

        Public Function Contains(row As Integer, column As Integer) As Boolean
            Return row >= StartRow AndAlso row <= EndRow AndAlso column >= StartColumn AndAlso column <= EndColumn
        End Function

        Public Function Contains(other As OutputRectangle) As Boolean
            If other Is Nothing Then Throw New ArgumentNullException(NameOf(other))
            Return Contains(other.StartRow, other.StartColumn) AndAlso Contains(other.EndRow, other.EndColumn)
        End Function

        Public Overrides Function ToString() As String
            Return String.Format(CultureInfo.InvariantCulture, "R{0}C{1}:R{2}C{3}", StartRow, StartColumn, EndRow, EndColumn)
        End Function
    End Class

    ''' <summary>Immutable vertical-stack options. Gaps are unused rows, never occupied rectangles.</summary>
    Public NotInheritable Class OutputLayoutOptions
        Public Sub New(Optional blankRowsBetweenTables As Integer = 1,
                       Optional maxRows As Integer = 1048576, Optional maxColumns As Integer = 16384)
            If maxRows < 1 Then Throw New ArgumentOutOfRangeException(NameOf(maxRows), "Grid row limit must be positive.")
            If maxColumns < 1 Then Throw New ArgumentOutOfRangeException(NameOf(maxColumns), "Grid column limit must be positive.")
            If blankRowsBetweenTables < 0 OrElse blankRowsBetweenTables >= maxRows Then
                Throw New ArgumentOutOfRangeException(NameOf(blankRowsBetweenTables), "Gap must be nonnegative and smaller than the grid row limit.")
            End If
            Me.BlankRowsBetweenTables = blankRowsBetweenTables
            Me.MaxRows = maxRows
            Me.MaxColumns = maxColumns
        End Sub
        Public ReadOnly Property BlankRowsBetweenTables As Integer
        Public ReadOnly Property MaxRows As Integer
        Public ReadOnly Property MaxColumns As Integer
    End Class

    Public NotInheritable Class OutputBlockPlacement
        Friend Sub New(artifact As OutputArtifact, index As Integer, rows As Integer, columns As Integer,
                       bounds As OutputRectangle, targetRow As Integer, targetColumn As Integer, blankRowsBefore As Integer)
            Me.Artifact = artifact
            Me.Index = index
            RowCount = rows
            ColumnCount = columns
            Me.Bounds = bounds
            If bounds IsNot Nothing Then
                RowOffset = CInt(CLng(bounds.StartRow) - targetRow)
                ColumnOffset = CInt(CLng(bounds.StartColumn) - targetColumn)
            End If
            Me.BlankRowsBefore = blankRowsBefore
        End Sub
        ''' <summary>Original immutable artifact, retaining all metadata and source identity.</summary>
        Public ReadOnly Property Artifact As OutputArtifact
        Public ReadOnly Property ArtifactId As String
            Get
                Return Artifact.Id
            End Get
        End Property
        Public ReadOnly Property Kind As OutputArtifactKind
            Get
                Return Artifact.Kind
            End Get
        End Property
        ''' <summary>Zero-based original document position, including all unplaced artifacts.</summary>
        Public ReadOnly Property Index As Integer
        Public ReadOnly Property RowCount As Integer
        Public ReadOnly Property ColumnCount As Integer
        ''' <summary>Nothing for zero-area tables and nonspatial artifacts; no fabricated cell.</summary>
        Public ReadOnly Property Bounds As OutputRectangle
        Public ReadOnly Property IsPlaced As Boolean
            Get
                Return Bounds IsNot Nothing
            End Get
        End Property
        ''' <summary>Zero-based offsets from the requested start; Nothing when not placed.</summary>
        Public ReadOnly Property RowOffset As Integer?
        Public ReadOnly Property ColumnOffset As Integer?
        ''' <summary>Unused rows preceding this block; zero for the first or an unplaced entry.</summary>
        Public ReadOnly Property BlankRowsBefore As Integer
    End Class

    ''' <summary>
    ''' Immutable in-memory geometry in one requested logical scope. Does not assert that any
    ''' workbook/sheet exists, a destination is supported, or live cells are safe to overwrite.
    ''' </summary>
    Public NotInheritable Class OutputLayoutPlan
        Friend Sub New(target As SpreadsheetOutputTarget, options As OutputLayoutOptions,
                       placements As IEnumerable(Of OutputBlockPlacement), usedBounds As OutputRectangle,
                       reservations As IEnumerable(Of OutputRectangle))
            Destination = target.Destination
            WorksheetName = target.WorksheetName
            StartRow = target.StartRow
            StartColumn = target.StartColumn
            Me.Options = options
            Me.Placements = New List(Of OutputBlockPlacement)(placements).AsReadOnly()
            Me.UsedBounds = usedBounds
            Me.Reservations = New List(Of OutputRectangle)(reservations).AsReadOnly()
        End Sub
        Public ReadOnly Property Destination As SpreadsheetOutputDestination
        Public ReadOnly Property WorksheetName As String
        Public ReadOnly Property StartRow As Integer
        Public ReadOnly Property StartColumn As Integer
        Public ReadOnly Property Options As OutputLayoutOptions
        Public ReadOnly Property Placements As IReadOnlyList(Of OutputBlockPlacement)
        ''' <summary>Envelope only, including interior gaps; Nothing when no block has positive area.</summary>
        Public ReadOnly Property UsedBounds As OutputRectangle
        ''' <summary>Caller-supplied reservations, all in this plan's logical scope; never live occupancy.</summary>
        Public ReadOnly Property Reservations As IReadOnlyList(Of OutputRectangle)
    End Class

    Public NotInheritable Class OutputLayoutPlanner
        Private Sub New()
        End Sub

        ''' <summary>
        ''' Stacks complete matrices vertically in document order. Zero-area/nonspatial entries
        ''' consume neither rows nor gaps. All reservations refer to the same resolved logical
        ''' destination; no sheet-name comparisons establish physical identity. Invalid inputs
        ''' throw ArgumentException; collisions throw InvalidOperationException before return.
        ''' Semantic identity/shape validation is optional and never recomputes cell semantics.
        ''' Inputs must not be mutated concurrently while this snapshot is being constructed.
        ''' </summary>
        Public Shared Function Create(document As OutputDocument, Optional target As SpreadsheetOutputTarget = Nothing,
                                      Optional options As OutputLayoutOptions = Nothing,
                                      Optional occupiedRectangles As IEnumerable(Of OutputRectangle) = Nothing,
                                      Optional semanticPlan As OutputSemanticPlan = Nothing) As OutputLayoutPlan
            If document Is Nothing Then Throw New ArgumentNullException(NameOf(document))
            Dim requested As SpreadsheetOutputTarget = If(target, New SpreadsheetOutputTarget())
            Dim resolved As New SpreadsheetOutputTarget With {
                .Destination = requested.Destination,
                .WorksheetName = If(String.IsNullOrWhiteSpace(requested.WorksheetName), "BESHStat_Result", requested.WorksheetName),
                .StartRow = requested.StartRow, .StartColumn = requested.StartColumn}
            If Not [Enum].IsDefined(GetType(SpreadsheetOutputDestination), resolved.Destination) Then
                Throw New ArgumentOutOfRangeException(NameOf(target), "Unknown requested output destination.")
            End If
            Dim limits As OutputLayoutOptions = If(options, New OutputLayoutOptions())
            If resolved.StartRow < 1 OrElse resolved.StartRow > limits.MaxRows OrElse
               resolved.StartColumn < 1 OrElse resolved.StartColumn > limits.MaxColumns Then
                Throw New ArgumentOutOfRangeException(NameOf(target), "Requested one-based start cell is outside the configured grid.")
            End If
            Dim reservations As New List(Of OutputRectangle)()
            If occupiedRectangles IsNot Nothing Then
                For Each rectangle As OutputRectangle In occupiedRectangles
                    If rectangle Is Nothing Then Throw New ArgumentException("Null occupied rectangle.", NameOf(occupiedRectangles))
                    CheckGrid(rectangle, limits, NameOf(occupiedRectangles))
                    reservations.Add(rectangle)
                Next
            End If
            If semanticPlan IsNot Nothing AndAlso semanticPlan.Artifacts.Count <> document.Artifacts.Count Then
                Throw New ArgumentException("Semantic artifact count does not match the document.", NameOf(semanticPlan))
            End If
            Dim placements As New List(Of OutputBlockPlacement)()
            Dim placedBounds As New List(Of OutputRectangle)()
            Dim previousEndRow As Long = 0
            Dim rightmost As Integer = resolved.StartColumn
            For index As Integer = 0 To document.Artifacts.Count - 1
                Dim artifact As OutputArtifact = document.Artifacts(index)
                Dim table As OutputTableArtifact = TryCast(artifact, OutputTableArtifact)
                Dim rows As Integer = If(table Is Nothing, 0, table.RowCount)
                Dim columns As Integer = If(table Is Nothing, 0, table.ColumnCount)
                If rows < 0 OrElse columns < 0 Then Throw New ArgumentException("Negative table dimensions: " & artifact.Id, NameOf(document))
                If semanticPlan IsNot Nothing Then
                    Dim styled As OutputArtifactStylePlan = semanticPlan.Artifacts(index)
                    If Not String.Equals(styled.ArtifactId, artifact.Id, StringComparison.Ordinal) OrElse
                       styled.Kind <> artifact.Kind OrElse styled.RowCount <> rows OrElse styled.ColumnCount <> columns Then
                        Throw New ArgumentException("Semantic identity/order/shape mismatch at artifact: " & artifact.Id, NameOf(semanticPlan))
                    End If
                End If
                Dim bounds As OutputRectangle = Nothing
                Dim gap As Integer = 0
                If rows > 0 AndAlso columns > 0 Then
                    gap = If(placedBounds.Count = 0, 0, limits.BlankRowsBetweenTables)
                    ' Every operand is Int32-bounded; these Int64 sums cannot overflow Int64.
                    ' Check grid limits before narrowing to Int32. No worksheet-sized allocation.
                    Dim startRow As Long = If(placedBounds.Count = 0, CLng(resolved.StartRow), previousEndRow + 1L + gap)
                    Dim endRow As Long = startRow + CLng(rows) - 1L
                    Dim endColumn As Long = CLng(resolved.StartColumn) + CLng(columns) - 1L
                    If endRow > limits.MaxRows OrElse endColumn > limits.MaxColumns Then
                        Throw New ArgumentOutOfRangeException(NameOf(document), String.Format(CultureInfo.InvariantCulture,
                            "Artifact '{0}' needs R{1}C{2}:R{3}C{4}, beyond grid {5}x{6}.",
                            artifact.Id, startRow, resolved.StartColumn, endRow, endColumn, limits.MaxRows, limits.MaxColumns))
                    End If
                    bounds = New OutputRectangle(CInt(startRow), resolved.StartColumn, CInt(endRow), CInt(endColumn))
                    For reservationIndex As Integer = 0 To reservations.Count - 1
                        If bounds.Intersects(reservations(reservationIndex)) Then
                            Throw New InvalidOperationException(String.Format(CultureInfo.InvariantCulture,
                                "Artifact '{0}' at {1} collides with reservation[{2}] at {3}.", artifact.Id, bounds, reservationIndex, reservations(reservationIndex)))
                        End If
                    Next
                    For Each prior As OutputRectangle In placedBounds
                        If bounds.Intersects(prior) Then Throw New InvalidOperationException("Internal layout overlap: " & artifact.Id & " at " & bounds.ToString())
                    Next
                    placedBounds.Add(bounds)
                    previousEndRow = endRow
                    rightmost = Math.Max(rightmost, bounds.EndColumn)
                End If
                placements.Add(New OutputBlockPlacement(artifact, index, rows, columns, bounds, resolved.StartRow, resolved.StartColumn, gap))
            Next
            Dim used As OutputRectangle = Nothing
            If placedBounds.Count > 0 Then used = New OutputRectangle(resolved.StartRow, resolved.StartColumn, CInt(previousEndRow), rightmost)
            Return New OutputLayoutPlan(resolved, limits, placements, used, reservations)
        End Function

        Private Shared Sub CheckGrid(rectangle As OutputRectangle, options As OutputLayoutOptions, parameter As String)
            If rectangle.EndRow > options.MaxRows OrElse rectangle.EndColumn > options.MaxColumns Then
                Throw New ArgumentOutOfRangeException(parameter, "Occupied rectangle " & rectangle.ToString() & " exceeds the configured grid.")
            End If
        End Sub
    End Class
End Namespace
