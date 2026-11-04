Option Explicit On
Option Strict On
Imports System
Imports System.Collections.Generic
Imports System.Globalization
Imports System.Linq
Imports BESHStatNG.StatisticalProcessControl

Namespace ExcelCommon
    Public NotInheritable Class SpcSourcePoint
        Friend Sub New(index As Integer, subgroupId As String, rows As IEnumerable(Of Integer), sources As IEnumerable(Of Integer))
            PointIndex = index : Me.SubgroupId = subgroupId
            InputRowIndices = rows.ToList().AsReadOnly() : SourceRowIndices = sources.ToList().AsReadOnly()
        End Sub
        Public ReadOnly Property PointIndex As Integer
        Public ReadOnly Property DisplayPoint As Integer
            Get
                Return PointIndex + 1
            End Get
        End Property
        Public ReadOnly Property SubgroupId As String
        Public ReadOnly Property InputRowIndices As IReadOnlyList(Of Integer)
        Public ReadOnly Property SourceRowIndices As IReadOnlyList(Of Integer)
    End Class
    Public NotInheritable Class SpcSourcePointMap
        Friend Sub New(layout As SpcDataLayout, sources() As Integer, ids() As String, rejectBlank As Boolean)
            Me.Layout = layout
            Dim groups As New Dictionary(Of String, Integer)(StringComparer.Ordinal)
            Dim rows As New List(Of List(Of Integer)), sourceGroups As New List(Of List(Of Integer)), keys As New List(Of String)
            Dim indices(sources.Length - 1) As Integer
            For row = 0 To sources.Length - 1
                Dim point As Integer
                Dim id As String = Nothing
                If layout = SpcDataLayout.StackedObservations Then
                    id = If(ids(row), String.Empty).Trim()
                    If rejectBlank AndAlso id.Length = 0 Then Throw New ArgumentException("A subgroup ID is blank at worksheet row " & sources(row).ToString() & ".")
                    If Not groups.TryGetValue(id, point) Then
                        point = groups.Count : groups.Add(id, point)
                    End If
                Else
                    point = row
                End If
                If point = rows.Count Then
                    rows.Add(New List(Of Integer)) : sourceGroups.Add(New List(Of Integer)) : keys.Add(id)
                End If
                rows(point).Add(row) : sourceGroups(point).Add(sources(row)) : indices(row) = point
            Next
            SourceRowIndices = Array.AsReadOnly(CType(sources.Clone(), Integer()))
            PointIndexByRow = Array.AsReadOnly(indices)
            Points = rows.Select(Function(v, i) New SpcSourcePoint(i, keys(i), v, sourceGroups(i))).ToList().AsReadOnly()
        End Sub
        Public ReadOnly Property Layout As SpcDataLayout
        Public ReadOnly Property SourceRowIndices As IReadOnlyList(Of Integer)
        Public ReadOnly Property PointIndexByRow As IReadOnlyList(Of Integer)
        Public ReadOnly Property Points As IReadOnlyList(Of SpcSourcePoint)
        Public ReadOnly Property PointCount As Integer
            Get
                Return Points.Count
            End Get
        End Property
    End Class
    Public NotInheritable Class SpcInputPointPolicy
        Private Sub New()
        End Sub
        Public Shared Function FirstDataRow(requiredHeaders As IEnumerable(Of Boolean)) As Integer
            If requiredHeaders Is Nothing Then Throw New ArgumentNullException(NameOf(requiredHeaders))
            Return If(requiredHeaders.Any(Function(v) v), 2, 1)
        End Function
        Public Shared Function FromWorksheetRows(layout As SpcDataLayout, firstRow As Integer, lastRow As Integer, Optional subgroupIds() As String = Nothing) As SpcSourcePointMap
            If lastRow < firstRow Then Throw New ArgumentException("The selected columns contain no data rows.")
            Return FromRows(layout, Enumerable.Range(firstRow, lastRow - firstRow + 1).ToArray(), subgroupIds, True)
        End Function
        Public Shared Function FromRows(layout As SpcDataLayout, sources() As Integer, Optional subgroupIds() As String = Nothing, Optional rejectBlank As Boolean = True) As SpcSourcePointMap
            If sources Is Nothing Then Throw New ArgumentNullException(NameOf(sources))
            If layout = SpcDataLayout.StackedObservations AndAlso (subgroupIds Is Nothing OrElse subgroupIds.Length <> sources.Length) Then Throw New ArgumentException("Subgroup identities must align with source rows.", NameOf(subgroupIds))
            Return New SpcSourcePointMap(layout, sources, subgroupIds, rejectBlank)
        End Function
        Public Shared Function LogicalPointCount(input As SpcInputData) As Integer
            If input Is Nothing Then Throw New ArgumentNullException(NameOf(input))
            If input.Layout <> SpcDataLayout.StackedObservations Then Return input.RowCount
            Dim distinct As New HashSet(Of String)(StringComparer.Ordinal)
            For Each id In input.SubgroupIds
                distinct.Add(If(id, String.Empty).Trim())
            Next
            Return distinct.Count
        End Function
    End Class
End Namespace
