Option Explicit On
Option Strict On

Imports System
Imports System.Collections.Generic
Imports System.Globalization
Imports System.Linq

Namespace ExcelCommon
    ''' <summary>
    ''' Existing shared-table plus supplementary-table geometry. It consumes canonical
    ''' layout/regions and preserves the legacy one-unused-row and next-cursor rules.
    ''' It neither creates destinations nor obtains consent, writes cells or rolls back.
    ''' </summary>
    Public NotInheritable Class ResultTableSequencePolicy
        Private Sub New()
        End Sub
        Public Shared Function PreflightBounds(layout As OutputLayoutPlan,
                                               supplementary As IEnumerable(Of Global.BESHStatNG.ResultTableOutputModel),
                                               maxRows As Integer, maxColumns As Integer) As OutputRectangle
            If layout Is Nothing Then Throw New ArgumentNullException(NameOf(layout))
            Dim used = layout.UsedBounds
            If used Is Nothing Then Throw New ArgumentException("This UI workflow requires occupied shared tables.")
            Dim rowLimit As Integer = Math.Min(maxRows, layout.Options.MaxRows)
            Dim columnLimit As Integer = Math.Min(maxColumns, layout.Options.MaxColumns)
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
        Public Shared Function OrderedRegions(layout As OutputLayoutPlan,
                                              supplementary As IEnumerable(Of Global.BESHStatNG.ResultTableOutputModel),
                                              chartIds As IEnumerable(Of String),
                                              maxRows As Integer, maxColumns As Integer) As IReadOnlyList(Of WriteRegion)
            If layout Is Nothing OrElse layout.UsedBounds Is Nothing Then Throw New ArgumentException("Shared output requires spatial tables.")
            Dim regions = layout.Placements.Where(Function(p) p.IsPlaced).Select(Function(p) New WriteRegion(p.ArtifactId, p.Bounds)).ToList()
            Dim lastRow As Integer = layout.UsedBounds.EndRow
            If supplementary IsNot Nothing Then
                Dim index As Integer = 0
                For Each model In supplementary
                    If model Is Nothing OrElse model.RowCount < 1 OrElse model.ColumnCount < 1 Then Throw New ArgumentException("Invalid supplementary table.")
                    Dim start As Long = CLng(lastRow) + 2
                    Dim finish As Long = start + model.RowCount - 1L
                    Dim right As Long = CLng(layout.StartColumn) + model.ColumnCount - 1L
                    If finish >= maxRows OrElse right > maxColumns Then Throw New ArgumentException("Supplementary output exceeds grid.")
                    regions.Add(New WriteRegion("supplementary-" & index.ToString(CultureInfo.InvariantCulture),
                        New OutputRectangle(CInt(start), layout.StartColumn, CInt(finish), CInt(right))))
                    lastRow = CInt(finish) : index += 1
                Next
            End If
            If chartIds IsNot Nothing Then
                For Each id In chartIds
                    regions.Add(New WriteRegion(id, Nothing, nonCellMutation:=True))
                Next
            End If
            Return regions.AsReadOnly()
        End Function
    End Class
End Namespace
