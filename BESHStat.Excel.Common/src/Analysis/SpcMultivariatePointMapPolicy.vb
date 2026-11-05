Option Explicit On
Option Strict On
Option Infer On

Imports System
Imports System.Collections.Generic
Imports BESHStatNG.StatisticalProcessControl

Namespace ExcelCommon
    ''' <summary>One logical point, its acquired row ordinals and physical source identities.</summary>
    Public NotInheritable Class SpcMultivariateSourcePoint
        Friend Sub New(index As Integer, id As String, rows As Integer(), sources As Integer())
            PointIndex = index
            SubgroupId = id
            InputRowIndices = Array.AsReadOnly(rows)
            SourceRowIndices = Array.AsReadOnly(sources)
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

    Public NotInheritable Class SpcMultivariatePointMap
        Friend Sub New(sources As Integer(), grouped As Boolean, ids As String())
            Dim pointIndices(sources.Length - 1) As Integer
            Dim groups As New Dictionary(Of String, Integer)(StringComparer.Ordinal)
            Dim members As New List(Of List(Of Integer))
            Dim keys As New List(Of String)
            For row As Integer = 0 To sources.Length - 1
                Dim point As Integer
                Dim id As String = String.Empty
                If grouped Then
                    id = ids(row).Trim()
                    If id.Length = 0 Then Throw New ArgumentException("A subgroup ID is blank at worksheet row " & sources(row).ToString() & ".")
                    If Not groups.TryGetValue(id, point) Then
                        point = groups.Count
                        groups.Add(id, point)
                        members.Add(New List(Of Integer))
                        keys.Add(id)
                    End If
                Else
                    point = row
                    members.Add(New List(Of Integer))
                    keys.Add(id)
                End If
                pointIndices(row) = point
                members(point).Add(row)
            Next
            Dim points As New List(Of SpcMultivariateSourcePoint)
            For i As Integer = 0 To members.Count - 1
                Dim rows = members(i).ToArray()
                Dim physical(rows.Length - 1) As Integer
                For j As Integer = 0 To rows.Length - 1
                    physical(j) = sources(rows(j))
                Next
                points.Add(New SpcMultivariateSourcePoint(i, keys(i), rows, physical))
            Next
            Me.IsGrouped = grouped
            Me.SourceRowIndices = Array.AsReadOnly(CType(sources.Clone(), Integer()))
            Me.PointIndexByRow = Array.AsReadOnly(pointIndices)
            Me.Points = points.AsReadOnly()
        End Sub
        Public ReadOnly Property IsGrouped As Boolean
        Public ReadOnly Property SourceRowIndices As IReadOnlyList(Of Integer)
        Public ReadOnly Property PointIndexByRow As IReadOnlyList(Of Integer)
        Public ReadOnly Property Points As IReadOnlyList(Of SpcMultivariateSourcePoint)
        Public ReadOnly Property PointCount As Integer
            Get
                Return Points.Count
            End Get
        End Property
        Public ReadOnly Property RowCount As Integer
            Get
                Return SourceRowIndices.Count
            End Get
        End Property

        ''' <summary>Legacy trim/ordinal agreement, in logical point then source row order.</summary>
        Public Function CollapsedText(values As String(), point As Integer, label As String) As String
            Dim found As String = Nothing
            For Each row In Points(point).InputRowIndices
                Dim current = values(row).Trim()
                If found Is Nothing Then
                    found = current
                ElseIf Not String.Equals(found, current, StringComparison.Ordinal) Then
                    Throw New ArgumentException("Rows belonging to chart point " & (point + 1).ToString() & " contain inconsistent " & label & " values.")
                End If
            Next
            Return If(found, String.Empty)
        End Function

        ''' <summary>Mutates only synchronous borrowed host arrays, exactly labels before sequences.</summary>
        Public Sub NormalizeGroupedMetadata(ByRef labels As String(), ByRef sequences As Double())
            If labels IsNot Nothing Then
                Dim collapsed(PointCount - 1) As String
                For point As Integer = 0 To PointCount - 1
                    collapsed(point) = CollapsedText(labels, point, "label")
                Next
                For row As Integer = 0 To labels.Length - 1
                    labels(row) = collapsed(PointIndexByRow(row))
                Next
            End If
            If sequences IsNot Nothing Then
                Dim collapsed(PointCount - 1) As Double
                For point As Integer = 0 To PointCount - 1
                    Dim found As Double? = Nothing
                    For Each row In Points(point).InputRowIndices
                        If Double.IsNaN(sequences(row)) Then Continue For
                        If found.HasValue AndAlso sequences(row) <> found.Value Then
                            Throw New ArgumentException("Rows belonging to subgroup point " & (point + 1).ToString() & " contain inconsistent sequence values.")
                        End If
                        found = sequences(row)
                    Next
                    collapsed(point) = If(found.HasValue, found.Value, Double.NaN)
                Next
                For row As Integer = 0 To sequences.Length - 1
                    sequences(row) = collapsed(PointIndexByRow(row))
                Next
            End If
        End Sub

        Public Function Expand(assignments As SpcMultivariatePointAssignments, exclusions As SpcMultivariatePointExclusions) As SpcMultivariateRowMetadata
            Dim phases(RowCount - 1) As SpcPhase
            Dim stages(RowCount - 1) As String
            Dim scopes(RowCount - 1) As SpcExclusionScope
            Dim reasons(RowCount - 1) As String
            Dim sources(RowCount - 1) As Integer
            For row As Integer = 0 To RowCount - 1
                Dim point = PointIndexByRow(row)
                phases(row) = assignments.Phases(point)
                stages(row) = assignments.StageIds(point)
                scopes(row) = exclusions.Scopes(point)
                reasons(row) = exclusions.Reasons(point)
                sources(row) = SourceRowIndices(row)
            Next
            Return New SpcMultivariateRowMetadata(phases, stages, scopes, reasons, sources)
        End Function
    End Class

    Public NotInheritable Class SpcMultivariateRowMetadata
        Friend Sub New(phases As SpcPhase(), stages As String(), scopes As SpcExclusionScope(), reasons As String(), sources As Integer())
            Me.Phases = Array.AsReadOnly(phases)
            Me.StageIds = Array.AsReadOnly(stages)
            Me.ExclusionScopes = Array.AsReadOnly(scopes)
            Me.ExclusionReasons = Array.AsReadOnly(reasons)
            Me.SourceRowIndices = Array.AsReadOnly(sources)
        End Sub
        Public ReadOnly Property Phases As IReadOnlyList(Of SpcPhase)
        Public ReadOnly Property StageIds As IReadOnlyList(Of String)
        Public ReadOnly Property ExclusionScopes As IReadOnlyList(Of SpcExclusionScope)
        Public ReadOnly Property ExclusionReasons As IReadOnlyList(Of String)
        Public ReadOnly Property SourceRowIndices As IReadOnlyList(Of Integer)
    End Class

    Public NotInheritable Class SpcMultivariatePointMapPolicy
        Private Sub New()
        End Sub
        Public Shared Function FromWorksheetRows(firstRow As Integer, lastRow As Integer, grouped As Boolean, Optional subgroupIds As String() = Nothing) As SpcMultivariatePointMap
            Dim sources(lastRow - firstRow) As Integer
            For row As Integer = 0 To sources.Length - 1
                sources(row) = firstRow + row
            Next
            Return FromRows(sources, grouped, subgroupIds)
        End Function
        Public Shared Function FromRows(sourceRows As Integer(), grouped As Boolean, Optional subgroupIds As String() = Nothing) As SpcMultivariatePointMap
            If sourceRows Is Nothing Then Throw New ArgumentNullException(NameOf(sourceRows))
            If grouped AndAlso (subgroupIds Is Nothing OrElse subgroupIds.Length <> sourceRows.Length) Then Throw New ArgumentException("Subgroup IDs must align with the acquired rows.", NameOf(subgroupIds))
            Return New SpcMultivariatePointMap(sourceRows, grouped, subgroupIds)
        End Function
    End Class
End Namespace
