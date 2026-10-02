Option Explicit On
Option Strict On

Imports System
Imports System.Collections.Generic
Imports BESHStatNG.AppInfrastructure

Namespace DataManagement

    ''' <summary>
    ''' Host-neutral row-selection, intersection, and in-place row-sorting helpers.
    ''' </summary>
    Public Module RowArrayUtilities

        ''' <summary>
        ''' Returns the selected rows from a two-dimensional array in the enumeration order
        ''' of <paramref name="rIds"/>.
        ''' </summary>
        Public Function SubsetRowsByIds(Of T)(data(,) As T, rIds As IEnumerable(Of Integer)) As T(,)
            Dim rowUpperBound As Integer = data.GetUpperBound(0)
            Dim colCount As Integer = data.GetLength(1)
            Dim requestedIds As New List(Of Integer)
            For Each key As Integer In rIds
                requestedIds.Add(key)
            Next

            For Each key As Integer In requestedIds
                If key < 0 OrElse key > rowUpperBound Then
                    CoreServices.Errors.LogAndThrow(
                        New ArgumentOutOfRangeException(NameOf(rIds),
                                                        $"Row index {key} is outside the valid range 0 to {rowUpperBound}."))
                End If
            Next

            If requestedIds.Count = 0 Then
                Return DirectCast(Array.CreateInstance(GetType(T), 0, colCount), T(,))
            End If

            Dim output(requestedIds.Count - 1, colCount - 1) As T

            For outputRow As Integer = 0 To requestedIds.Count - 1
                Dim sourceRow As Integer = requestedIds(outputRow)
                For columnIndex As Integer = 0 To colCount - 1
                    output(outputRow, columnIndex) = data(sourceRow, columnIndex)
                Next
            Next

            Return output
        End Function

        ''' <summary>
        ''' Returns the selected elements from a one-dimensional array in the enumeration
        ''' order of <paramref name="rIds"/>.
        ''' </summary>
        Public Function SubsetItemsByIds(Of T)(data() As T, rIds As IEnumerable(Of Integer)) As T()
            Dim rowUpperBound As Integer = data.GetUpperBound(0)
            Dim requestedIds As New List(Of Integer)
            For Each key As Integer In rIds
                requestedIds.Add(key)
            Next

            For Each key As Integer In requestedIds
                If key < 0 OrElse key > rowUpperBound Then
                    CoreServices.Errors.LogAndThrow(
                        New ArgumentOutOfRangeException(NameOf(rIds),
                                                        $"Row index {key} is outside the valid range 0 to {rowUpperBound}."))
                End If
            Next

            If requestedIds.Count = 0 Then Return Array.Empty(Of T)()

            Dim output(requestedIds.Count - 1) As T
            For i As Integer = 0 To requestedIds.Count - 1
                output(i) = data(requestedIds(i))
            Next

            Return output
        End Function

        ''' <summary>
        ''' Returns all elements of <paramref name="arr1"/> whose values also occur in
        ''' <paramref name="arr2"/>. Dictionary keys are indices in <paramref name="arr1"/>.
        ''' Duplicate matching values in <paramref name="arr1"/> are retained as separate entries.
        ''' </summary>
        Public Function FindCommonItems(Of T)(arr1() As T,
                                              arr2() As T,
                                              Optional comparer As IEqualityComparer(Of T) = Nothing) As Dictionary(Of Integer, T)
            If comparer Is Nothing Then comparer = EqualityComparer(Of T).Default

            Dim set2 As New HashSet(Of T)(arr2, comparer)
            Dim result As New Dictionary(Of Integer, T)

            For i As Integer = 0 To arr1.Length - 1
                Dim value As T = arr1(i)
                If set2.Contains(value) Then result.Add(i, value)
            Next

            Return result
        End Function

        ''' <summary>
        ''' Sorts rows of a two-dimensional array in place using comma-separated
        ''' column/direction pairs such as "0,A,1,D".
        ''' </summary>
        Public Sub SortRows(Of T)(arr(,) As T, criteria As String, low As Integer, up As Integer)
            Dim parts() As String = criteria.Split(","c)
            Dim criterionUpperBound As Integer = (parts.Length - 2) \ 2
            Dim columns(criterionUpperBound) As Integer
            Dim directions(criterionUpperBound) As String

            Dim criterionIndex As Integer = 0
            For i As Integer = 0 To parts.GetUpperBound(0) Step 2
                columns(criterionIndex) = Integer.Parse(parts(i))
                directions(criterionIndex) = parts(i + 1)
                criterionIndex += 1
            Next

            QuickSortRows(arr, columns, directions, low, up)
        End Sub

        Private Sub QuickSortRows(Of T)(values(,) As T,
                                        columns() As Integer,
                                        directions() As String,
                                        low As Integer,
                                        up As Integer)
            Dim lowCursor As Integer = low
            Dim highCursor As Integer = up
            Dim pivot(columns.Length - 1) As T

            For i As Integer = 0 To pivot.Length - 1
                pivot(i) = values((low + up) \ 2, columns(i))
            Next

            Do While lowCursor <= highCursor
                Do While RowComparesBeforePivot(values, lowCursor, columns, directions, pivot) AndAlso lowCursor < up
                    lowCursor += 1
                Loop

                Do While RowComparesAfterPivot(values, highCursor, columns, directions, pivot) AndAlso highCursor > low
                    highCursor -= 1
                Loop

                If lowCursor <= highCursor Then
                    For columnIndex As Integer = 0 To values.GetUpperBound(1)
                        Dim swapValue As T = values(lowCursor, columnIndex)
                        values(lowCursor, columnIndex) = values(highCursor, columnIndex)
                        values(highCursor, columnIndex) = swapValue
                    Next
                    lowCursor += 1
                    highCursor -= 1
                End If
            Loop

            If low < highCursor Then QuickSortRows(values, columns, directions, low, highCursor)
            If lowCursor < up Then QuickSortRows(values, columns, directions, lowCursor, up)
        End Sub

        Private Function RowComparesBeforePivot(Of T)(values(,) As T,
                                                       rowIndex As Integer,
                                                       columns() As Integer,
                                                       directions() As String,
                                                       pivot() As T) As Boolean
            Dim cmp As System.Collections.Generic.Comparer(Of T) = System.Collections.Generic.Comparer(Of T).Default

            For i As Integer = 0 To pivot.Length - 1
                Dim value As T = values(rowIndex, columns(i))
                Dim comparison As Integer = cmp.Compare(value, pivot(i))

                If String.Equals(directions(i), "A", StringComparison.OrdinalIgnoreCase) Then
                    If comparison < 0 Then Return True
                    If comparison > 0 Then Return False
                Else
                    If comparison > 0 Then Return True
                    If comparison < 0 Then Return False
                End If
            Next

            Return False
        End Function

        Private Function RowComparesAfterPivot(Of T)(values(,) As T,
                                                      rowIndex As Integer,
                                                      columns() As Integer,
                                                      directions() As String,
                                                      pivot() As T) As Boolean
            Dim cmp As System.Collections.Generic.Comparer(Of T) = System.Collections.Generic.Comparer(Of T).Default

            For i As Integer = 0 To pivot.Length - 1
                Dim value As T = values(rowIndex, columns(i))
                Dim comparison As Integer = cmp.Compare(pivot(i), value)

                If String.Equals(directions(i), "A", StringComparison.OrdinalIgnoreCase) Then
                    If comparison < 0 Then Return True
                    If comparison > 0 Then Return False
                Else
                    If comparison > 0 Then Return True
                    If comparison < 0 Then Return False
                End If
            Next

            Return False
        End Function

    End Module

End Namespace
