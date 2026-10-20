Option Explicit On
Option Strict On

Imports System
Imports System.Collections.Generic
Imports System.Globalization
Imports System.Numerics
Imports BESHStatNG.AppInfrastructure

Namespace ExcelCommon
    ' Prototype adapter wire version, NOT the frozen Common v1 envelope (02.08).
    ' Serialize with camelCase property naming, including null union fields.
    Public NotInheritable Class OfficeOutputValue
        Private Sub New(kind As String, Optional number As Double? = Nothing,
                        Optional text As String = Nothing, Optional booleanValue As Boolean? = Nothing)
            Me.Kind = kind
            NumberValue = number
            TextValue = text
            Me.BooleanValue = booleanValue
        End Sub
        Public ReadOnly Property Kind As String
        Public ReadOnly Property NumberValue As Double?
        Public ReadOnly Property TextValue As String
        Public ReadOnly Property BooleanValue As Boolean?

        Public Shared Function FromCell(cell As OutputCell) As OfficeOutputValue
            If cell Is Nothing Then Throw New ArgumentNullException(NameOf(cell))
            Select Case cell.Kind
                Case OutputCellKind.Blank : Return New OfficeOutputValue("blank")
                Case OutputCellKind.Text : Return New OfficeOutputValue("text", text:=cell.TextValue)
                Case OutputCellKind.BooleanValue : Return New OfficeOutputValue("boolean", booleanValue:=cell.BooleanValue)
                Case OutputCellKind.Number
                    Dim number As Double
                    If cell.NumberKind = OutputNumberKind.Binary64 Then
                        number = cell.BinaryValue.Value
                    ElseIf cell.NumberKind = OutputNumberKind.ExactDecimal Then
                        Dim exact As Decimal = cell.DecimalValue.Value
                        number = CDbl(exact)
                        If Not ExactlyRepresented(exact, number) Then
                            Throw New ArgumentException("Office output cannot losslessly transport this exact decimal/integer as binary64.")
                        End If
                    Else
                        Throw New ArgumentException("Unknown output number kind.")
                    End If
                    If Double.IsNaN(number) OrElse Double.IsInfinity(number) Then Throw New ArgumentException("Nonfinite Office output number.")
                    Return New OfficeOutputValue("number", number:=number)
                Case Else
                    ' No formula dialect or native error transport is authorized in this adapter.
                    Throw New ArgumentException("Office output does not support error or trusted formula cells.")
            End Select
        End Function

        Private Shared Function ExactlyRepresented(exact As Decimal, number As Double) As Boolean
            Dim parts As Integer() = Decimal.GetBits(exact)
            Dim numerator As BigInteger = New BigInteger(CLng(parts(0)) And &HFFFFFFFFL) +
                (New BigInteger(CLng(parts(1)) And &HFFFFFFFFL) << 32) +
                (New BigInteger(CLng(parts(2)) And &HFFFFFFFFL) << 64)
            If parts(3) < 0 Then numerator = -numerator
            Dim denominator As BigInteger = BigInteger.Pow(10, (parts(3) >> 16) And &HFF)
            Dim bits As Long = BitConverter.DoubleToInt64Bits(number)
            Dim exponentBits As Integer = CInt((bits >> 52) And &H7FFL)
            Dim significand As BigInteger = New BigInteger(bits And &HFFFFFFFFFFFFFL)
            Dim exponent As Integer = -1074
            If exponentBits <> 0 Then
                significand += BigInteger.One << 52
                exponent = exponentBits - 1075
            End If
            If bits < 0 Then significand = -significand
            If exponent >= 0 Then Return numerator = (significand << exponent) * denominator
            Return (numerator << -exponent) = significand * denominator
        End Function
    End Class

    Public NotInheritable Class OfficeOutputCell
        Friend Sub New(style As OutputCellStyle, numberFormat As String)
            Row = style.Row
            Column = style.Column
            Value = OfficeOutputValue.FromCell(style.Cell)
            Role = style.Role.ToString()
            Me.NumberFormat = numberFormat
            IsPValue = style.IsPValue
            IsSignificant = style.IsSignificant
            StrongEmphasis = style.StrongEmphasis
        End Sub
        Public ReadOnly Property Row As Integer
        Public ReadOnly Property Column As Integer
        Public ReadOnly Property Value As OfficeOutputValue
        Public ReadOnly Property Role As String
        Public ReadOnly Property NumberFormat As String
        Public ReadOnly Property IsPValue As Boolean
        Public ReadOnly Property IsSignificant As Boolean
        Public ReadOnly Property StrongEmphasis As Boolean
    End Class

    Public NotInheritable Class OfficeOutputTarget
        Friend Sub New(layout As OutputLayoutPlan)
            Destination = CInt(layout.Destination)
            WorksheetName = layout.WorksheetName
            StartRow = layout.StartRow
            StartColumn = layout.StartColumn
        End Sub
        Public ReadOnly Property Destination As Integer
        Public ReadOnly Property WorksheetName As String
        Public ReadOnly Property StartRow As Integer
        Public ReadOnly Property StartColumn As Integer
    End Class

    Public NotInheritable Class OfficeOutputTable
        Friend Sub New(block As OutputBlockPlacement, ordinal As Integer, cells As IEnumerable(Of OfficeOutputCell), autoFit As Boolean)
            ArtifactId = block.ArtifactId
            DocumentIndex = block.Index
            TableOrdinal = ordinal
            Bounds = block.Bounds
            RowCount = block.RowCount
            ColumnCount = block.ColumnCount
            Me.Cells = New List(Of OfficeOutputCell)(cells).AsReadOnly()
            Me.AutoFit = autoFit
        End Sub
        Public ReadOnly Property ArtifactId As String
        Public ReadOnly Property DocumentIndex As Integer
        Public ReadOnly Property TableOrdinal As Integer
        Public ReadOnly Property Bounds As OutputRectangle
        Public ReadOnly Property RowCount As Integer
        Public ReadOnly Property ColumnCount As Integer
        Public ReadOnly Property Cells As IReadOnlyList(Of OfficeOutputCell)
        Public ReadOnly Property AutoFit As Boolean
    End Class

    Public NotInheritable Class OfficeOutputPlan
        Friend Sub New(layout As OutputLayoutPlan, sourceTableCount As Integer, tables As IEnumerable(Of OfficeOutputTable), format As String)
            Target = New OfficeOutputTarget(layout)
            ArtifactCount = layout.Placements.Count
            Me.SourceTableCount = sourceTableCount
            Me.Tables = New List(Of OfficeOutputTable)(tables).AsReadOnly()
            TableCount = Me.Tables.Count
            UsedBounds = layout.UsedBounds
            PvalueNumberFormat = format
        End Sub
        Public ReadOnly Property SchemaVersion As Integer = 1
        Public ReadOnly Property Target As OfficeOutputTarget
        Public ReadOnly Property ArtifactCount As Integer
        Public ReadOnly Property SourceTableCount As Integer
        Public ReadOnly Property TableCount As Integer
        Public ReadOnly Property Tables As IReadOnlyList(Of OfficeOutputTable)
        Public ReadOnly Property UsedBounds As OutputRectangle
        Public ReadOnly Property PvalueNumberFormat As String
    End Class

    Public NotInheritable Class OfficeOutputPlanAdapter
        Private Sub New()
        End Sub

        Public Shared Function Prepare(document As OutputDocument, semantics As OutputSemanticPlan,
                                       layout As OutputLayoutPlan, Optional autoFit As Boolean = True) As OfficeOutputPlan
            If document Is Nothing OrElse semantics Is Nothing OrElse layout Is Nothing Then Throw New ArgumentNullException("plan")
            If Not Object.ReferenceEquals(document, semantics.SourceDocument) Then Throw New ArgumentException("Stale semantic source document.")
            If layout.Destination <> SpreadsheetOutputDestination.CurrentWorksheet AndAlso
               layout.Destination <> SpreadsheetOutputDestination.NewWorksheet Then Throw New ArgumentException("Unsupported Office output destination.")
            If layout.StartRow < 1 OrElse layout.StartRow > 1048576 OrElse layout.StartColumn < 1 OrElse layout.StartColumn > 16384 Then
                Throw New ArgumentException("Office output target is outside the worksheet grid.")
            End If
            If document.Artifacts.Count <> semantics.Artifacts.Count OrElse document.Artifacts.Count <> layout.Placements.Count Then
                Throw New ArgumentException("Output plan artifact count mismatch.")
            End If
            ' Reuse the prototype's existing 01.10 bounds as adapter memory guards, not a new product output quota.
            Dim limits As New InputSizeLimits()
            If document.Artifacts.Count > limits.MaxRanges Then Throw New ArgumentException("Office output adapter artifact limit exceeded.")
            Dim totalCells As Long = 0
            Dim estimatedBytes As Long = 0
            If layout.WorksheetName Is Nothing OrElse layout.WorksheetName.Length > 32767 Then Throw New ArgumentException("Invalid Office worksheet name transport.")
            Dim tables As New List(Of OfficeOutputTable)()
            Dim ordinal As Integer = 0
            Dim policy = semantics.Policy
            Dim format As String = Global.BESHStatNG.PValuePresentation.BuildExcelNumberFormat(
                New PValuePresentationSettings With {.DecimalPlaces = policy.DecimalPlaces,
                    .SmallValueDisplay = policy.SmallValueDisplay, .UseUpperBound = policy.UseUpperBound})
            For index As Integer = 0 To document.Artifacts.Count - 1
                Dim artifact = document.Artifacts(index)
                If artifact.Id.Length > 32767 Then Throw New ArgumentException("Office artifact ID is too long.")
                Dim block = layout.Placements(index)
                Dim styled = semantics.Artifacts(index)
                Dim table = TryCast(artifact, OutputTableArtifact)
                Dim rows As Integer = If(table Is Nothing, 0, table.RowCount)
                Dim columns As Integer = If(table Is Nothing, 0, table.ColumnCount)
                If Not Object.ReferenceEquals(block.Artifact, artifact) OrElse block.Index <> index OrElse
                   block.ArtifactId <> styled.ArtifactId OrElse block.Kind <> styled.Kind OrElse
                   block.RowCount <> rows OrElse block.ColumnCount <> columns OrElse
                   styled.RowCount <> rows OrElse styled.ColumnCount <> columns OrElse
                   styled.Cells.Count <> CLng(rows) * columns Then Throw New ArgumentException("Output identity/order/shape mismatch.")
                If table Is Nothing Then
                    If block.Bounds IsNot Nothing Then Throw New ArgumentException("Nonspatial artifact has worksheet bounds.")
                    Continue For
                End If
                Dim tableOrdinal As Integer = ordinal
                ordinal += 1
                If rows = 0 OrElse columns = 0 Then
                    If block.Bounds IsNot Nothing Then Throw New ArgumentException("Zero-area table has worksheet bounds.")
                    Continue For
                End If
                totalCells += CLng(rows) * columns
                If totalCells > limits.MaxTotalCells Then Throw New ArgumentException("Office output adapter cell limit exceeded.")
                Dim bounds = block.Bounds
                If bounds Is Nothing OrElse bounds.EndRow > 1048576 OrElse bounds.EndColumn > 16384 OrElse
                   bounds.EndRow - bounds.StartRow + 1 <> rows OrElse bounds.EndColumn - bounds.StartColumn + 1 <> columns OrElse
                   layout.UsedBounds Is Nothing OrElse Not layout.UsedBounds.Contains(bounds) Then Throw New ArgumentException("Output rectangle mismatch.")
                For Each prior In tables
                    If prior.Bounds.Intersects(bounds) Then Throw New ArgumentException("Overlapping output rectangles.")
                Next
                Dim cells As New List(Of OfficeOutputCell)()
                Dim canonical As New OutputCellTable(table)
                For cellIndex As Integer = 0 To styled.Cells.Count - 1
                    Dim style = styled.Cells(cellIndex)
                    Dim row As Integer = cellIndex \ columns
                    Dim column As Integer = cellIndex Mod columns
                    Dim expected = canonical.CellAt(row, column)
                    If style.ArtifactId <> artifact.Id OrElse style.Row <> row OrElse style.Column <> column OrElse
                       Not SameCell(expected, style.Cell) Then Throw New ArgumentException("Stale or reordered semantic cell.")
                    Dim resolvedFormat As String
                    Select Case style.NumberFormat.Kind
                        Case OutputNumberFormatKind.General : resolvedFormat = "General"
                        Case OutputNumberFormatKind.PValue : resolvedFormat = format
                        Case Else : Throw New ArgumentException("Unsupported Office number format intent.")
                    End Select
                    Dim projected As New OfficeOutputCell(style, resolvedFormat)
                    Dim text As String = projected.Value.TextValue
                    If text IsNot Nothing AndAlso text.Length > 32767 Then Throw New ArgumentException("Office text exceeds the worksheet cell limit.")
                    estimatedBytes += 1024L + If(text Is Nothing, 0L, (CLng(text.Length) + 1L) * 2L) + CLng(resolvedFormat.Length) * 2L
                    If estimatedBytes > limits.MaxEstimatedBytes Then Throw New ArgumentException("Office output adapter byte limit exceeded.")
                    cells.Add(projected)
                Next
                tables.Add(New OfficeOutputTable(block, tableOrdinal, cells, autoFit))
            Next
            If (tables.Count = 0) <> (layout.UsedBounds Is Nothing) Then Throw New ArgumentException("Output envelope mismatch.")
            Return New OfficeOutputPlan(layout, ordinal, tables, format)
        End Function

        Private Shared Function SameCell(left As OutputCell, right As OutputCell) As Boolean
            If right Is Nothing OrElse left.Kind <> right.Kind OrElse left.NumberKind <> right.NumberKind Then Return False
            Select Case left.Kind
                Case OutputCellKind.Blank : Return True
                Case OutputCellKind.Text : Return String.Equals(left.TextValue, right.TextValue, StringComparison.Ordinal)
                Case OutputCellKind.BooleanValue : Return left.BooleanValue.Value = right.BooleanValue.Value
                Case OutputCellKind.Number
                    If left.NumberKind = OutputNumberKind.ExactDecimal Then Return left.DecimalValue.Value = right.DecimalValue.Value
                    Return BitConverter.DoubleToInt64Bits(left.BinaryValue.Value) = BitConverter.DoubleToInt64Bits(right.BinaryValue.Value)
                Case Else : Throw New ArgumentException("Unsupported Office output scalar.")
            End Select
        End Function
    End Class
End Namespace
