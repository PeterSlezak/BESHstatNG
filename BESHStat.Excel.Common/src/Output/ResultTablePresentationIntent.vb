Option Explicit On
Option Strict On

Imports System
Imports System.Collections.Generic

Namespace ExcelCommon
    ''' <summary>
    ''' Small, detached presentation metadata for an already assembled Core block.
    ''' Coordinates are one-based and table-relative unless explicitly sheet-relative.
    ''' Values are not copied, converted, formatted or recomputed. Raw annotations retain
    ''' order, duplicates and invalid indices; the consuming host retains its error boundary.
    ''' This compatibility intent deliberately does not impose OutputTableArtifact validation
    ''' on legacy scalar/object writers. Strict documents continue to use that existing API.
    ''' </summary>
    Public NotInheritable Class ResultTablePresentationIntent
        Public Sub New(headerTopRows As Integer, headerLeftColumns As Integer,
                       footerRows As Integer, titleRows As Integer)
            Me.HeaderTopRows = headerTopRows
            Me.HeaderLeftColumns = headerLeftColumns
            Me.FooterRows = footerRows
            Me.TitleRows = titleRows
            PvalueColumns = New List(Of Integer)().AsReadOnly()
            PvalueCells = New List(Of Global.BESHStatNG.ResultTableCellAddress)().AsReadOnly()
        End Sub

        Public Shared Function Capture(block As Global.BESHStatNG.ResultTableOutputBlock,
                                       Optional artifactId As String = "legacy-table",
                                       Optional sourceOrdinal As Integer? = Nothing) As ResultTablePresentationIntent
            If block Is Nothing OrElse block.Model Is Nothing Then Throw New ArgumentNullException(NameOf(block))
            Return New ResultTablePresentationIntent(block, artifactId, sourceOrdinal)
        End Function

        Private Sub New(block As Global.BESHStatNG.ResultTableOutputBlock, artifactId As String, sourceOrdinal As Integer?)
            Me.New(block.Model.HeaderTopRows, block.Model.HeaderLeftColumns, block.Model.FooterRows, block.Model.TitleRows)
            RowCount = block.Model.RowCount : ColumnCount = block.Model.ColumnCount
            StartRow = block.StartRow : StartColumn = block.StartColumn
            Me.ArtifactId = artifactId : Me.SourceOrdinal = sourceOrdinal
            IsResultTable = block.Model.IsResultTable
            PvalueColumns = New List(Of Integer)(block.Model.PvalueColumns).AsReadOnly()
            PvalueCells = New List(Of Global.BESHStatNG.ResultTableCellAddress)(block.Model.PvalueCells).AsReadOnly()
        End Sub

        Public ReadOnly Property HeaderTopRows As Integer
        Public ReadOnly Property HeaderLeftColumns As Integer
        Public ReadOnly Property FooterRows As Integer
        Public ReadOnly Property TitleRows As Integer
        Public ReadOnly Property RowCount As Integer
        Public ReadOnly Property ColumnCount As Integer
        Public ReadOnly Property StartRow As Integer = 1
        Public ReadOnly Property StartColumn As Integer = 1
        Public ReadOnly Property ArtifactId As String
        Public ReadOnly Property SourceOrdinal As Integer?
        Public ReadOnly Property IsResultTable As Boolean
        Public ReadOnly Property PvalueColumns As IReadOnlyList(Of Integer)
        Public ReadOnly Property PvalueCells As IReadOnlyList(Of Global.BESHStatNG.ResultTableCellAddress)

        ' Keep arithmetic lazy: native writers call these at their original operation
        ' positions, preserving checked-overflow and partial-formatting boundaries.
        Public ReadOnly Property HasTopBorder As Boolean
            Get
                Return TitleRows = 0
            End Get
        End Property
        Public Function TopHeaderRow(index As Integer) As Integer
            Return index + TitleRows
        End Function
        Public ReadOnly Property LegacyLastLeftHeaderColumn As Integer
            Get
                ' Historical formatting subtracts footers here; this is not a correction.
                Return HeaderLeftColumns - FooterRows
            End Get
        End Property
        Public Function LastBodyRow(rows As Integer) As Integer
            Return rows - FooterRows
        End Function
        Public Function FirstFooterRow(rows As Integer) As Integer
            Return rows - FooterRows + 1
        End Function
        Public ReadOnly Property FirstBodyRow As Integer
            Get
                Return 1 + HeaderTopRows + TitleRows
            End Get
        End Property
        Public Function PvalueRow(bodyRow As Integer) As Integer
            Return FirstBodyRow + bodyRow - 1
        End Function
        Public Function PvalueColumn(bodyColumn As Integer) As Integer
            Return bodyColumn + HeaderLeftColumns
        End Function
        Public Function SheetRow(tableRow As Integer) As Integer
            Return StartRow + tableRow - 1
        End Function
        Public Function SheetColumn(tableColumn As Integer) As Integer
            Return StartColumn + tableColumn - 1
        End Function
        ''' <summary>Canonical semantic role, distinct from historical native font actions.</summary>
        Public Function RoleAt(tableRow As Integer, tableColumn As Integer) As OutputStyleRole
            If tableRow < 1 OrElse tableRow > RowCount OrElse tableColumn < 1 OrElse tableColumn > ColumnCount Then Throw New ArgumentOutOfRangeException()
            If tableRow <= TitleRows Then Return OutputStyleRole.Title
            If tableRow <= TitleRows + HeaderTopRows Then Return OutputStyleRole.TopColumnHeader
            If tableRow > RowCount - FooterRows Then Return OutputStyleRole.Footer
            If tableColumn <= HeaderLeftColumns Then Return OutputStyleRole.LeftRowHeader
            Return OutputStyleRole.Body
        End Function
    End Class
End Namespace
