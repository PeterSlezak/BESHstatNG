Option Explicit On
Imports BESHStatNG.AppInfrastructure

Public Module Helpers

    ''' <summary>
    ''' Creates a subset of the input 2D array by selecting specific rows whose indices
    ''' are provided in <paramref name="rIds"/>. The resulting array contains only the
    ''' selected rows, in the order of the dictionary's key enumeration.
    ''' </summary>
    ''' <typeparam name="T">
    ''' The element type of the 2D array.
    ''' </typeparam>
    ''' <param name="data">
    ''' The source 2D array from which rows will be extracted.
    ''' </param>
    ''' <param name="rIds">
    ''' A dictionary whose keys represent the row indices to extract from <paramref name="data"/>.
    ''' The dictionary values are ignored; only the keys are used.
    ''' </param>
    ''' <returns>
    ''' A new 2D array containing only the rows specified by <paramref name="rIds"/>.
    ''' The number of rows equals <c>rIds.Count</c>, and the number of columns matches
    ''' the second dimension of <paramref name="data"/>.
    ''' </returns>
    ''' <exception cref="ArgumentOutOfRangeException">
    ''' Thrown when any key in <paramref name="rIds"/> is outside the valid range of row indices
    ''' for <paramref name="data"/>.
    ''' </exception>
    ''' <remarks>
    ''' This function performs a direct row copy using nested loops.
    ''' Time complexity is <c>O(k * m)</c>, where <c>k</c> is the number of selected rows
    ''' and <c>m</c> is the number of columns.
    ''' </remarks>
    ''' <example>
    ''' <code>
    ''' Dim mat(,) As Integer = {
    '''     {1, 2, 3},
    '''     {4, 5, 6},
    '''     {7, 8, 9}
    ''' }
    '''
    ''' Dim ids As New Dictionary(Of Integer, Integer) From {
    '''     {2, 0},
    '''     {0, 0}
    ''' }
    '''
    ''' Dim subset = SubsetArrayByIds(mat, ids)
    ''' ' subset =
    ''' '   { {7, 8, 9},
    ''' '     {1, 2, 3} }
    ''' </code>
    ''' </example>
    Public Function SubsetArrayByIds(Of T)(data(,) As T, rIds As Dictionary(Of Integer, Integer)) As T(,)
        Dim rowCount As Integer = UBound(data, 1)
        Dim colCount As Integer = UBound(data, 2)

        ' Validate all requested row indices
        For Each key In rIds.Keys
            If key < 0 OrElse key > rowCount Then
                AppGlobals.BSerr.LogAndThrow(New ArgumentOutOfRangeException(NameOf(rIds), $"Row index {key} is outside the valid range 0 to {rowCount}."))
            End If
        Next

        ' Prepare output array
        Dim newRowCount As Integer = rIds.Count - 1
        Dim tmp = data
        ReDim data(newRowCount, colCount)

        ' Copy selected rows
        Dim i As Integer = 0
        For Each key In rIds.Keys
            For j As Integer = 0 To colCount
                data(i, j) = tmp(key, j)
            Next
            i += 1
        Next

        Return data
    End Function


    ''' <summary>
    ''' Identifies all values that appear in both input arrays and returns them in a dictionary
    ''' where each key is the index of the matching value in <paramref name="arr1"/>,
    ''' and each value is the corresponding item of type <typeparamref name="T"/>.
    ''' </summary>
    ''' <typeparam name="T">
    ''' The element type of the input arrays.
    ''' </typeparam>
    ''' <param name="arr1">
    ''' The first array. Its indices are used as dictionary keys for matched items.
    ''' </param>
    ''' <param name="arr2">
    ''' The second array. Its values are used to determine which items from <paramref name="arr1"/> match.
    ''' </param>
    ''' <param name="comparer">
    ''' Optional equality comparer used to determine value equality.  
    ''' If omitted, <see cref="EqualityComparer(Of T).Default"/> is used.
    ''' </param>
    ''' <returns>
    ''' A <see cref="Dictionary(Of Integer, T)"/> containing all items from <paramref name="arr1"/>
    ''' that also appear in <paramref name="arr2"/>.  
    ''' Keys represent indices in <paramref name="arr1"/>, and values represent the matching items.
    ''' </returns>
    ''' <remarks>
    ''' This function uses a <see cref="HashSet(Of T)"/> for efficient membership testing.
    ''' Time complexity is <c>O(n + m)</c>, where <c>n</c> is the length of <paramref name="arr1"/>
    ''' and <c>m</c> is the length of <paramref name="arr2"/>.
    ''' </remarks>
    ''' <example>
    ''' <code>
    ''' Dim a() As String = {"apple", "pear", "banana"}
    ''' Dim b() As String = {"banana", "kiwi"}
    '''
    ''' Dim result = CommonItems(a, b)
    ''' ' result = { {2, "banana"} }
    ''' </code>
    ''' </example>
    Public Function CommonItems(Of T)(arr1() As T, arr2() As T,
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
    ''' Creates a subset of a one-dimensional array by selecting specific elements whose
    ''' indices are provided in <paramref name="rIds"/>. The resulting array contains only
    ''' the selected elements, in the enumeration order of the dictionary's keys.
    ''' </summary>
    ''' <typeparam name="T">
    ''' The element type of the input array.
    ''' </typeparam>
    ''' <param name="data">
    ''' The source one-dimensional array from which elements will be extracted.
    ''' </param>
    ''' <param name="rIds">
    ''' A dictionary whose keys represent the element indices to extract from <paramref name="data"/>.
    ''' The dictionary values are ignored; only the keys are used.
    ''' </param>
    ''' <returns>
    ''' A new one-dimensional array containing the elements of <paramref name="data"/> located at
    ''' the indices specified in <paramref name="rIds"/>.  
    ''' The length of the returned array equals <c>rIds.Count</c>.
    ''' </returns>
    ''' <exception cref="ArgumentOutOfRangeException">
    ''' Thrown when any key in <paramref name="rIds"/> is outside the valid index range
    ''' of <paramref name="data"/>.
    ''' </exception>
    ''' <remarks>
    ''' This function performs direct element copying using a simple loop.  
    ''' Time complexity is <c>O(k)</c>, where <c>k</c> is the number of selected indices.
    ''' </remarks>
    ''' <example>
    ''' <code>
    ''' Dim arr() As String = {"A", "B", "C", "D"}
    ''' Dim ids As New Dictionary(Of Integer, Integer) From {
    '''     {3, 0},
    '''     {1, 0}
    ''' }
    '''
    ''' Dim subset = Subset1DArrayByIds(arr, ids)
    ''' ' subset = {"D", "B"}
    ''' </code>
    ''' </example>
    Public Function Subset1DArrayByIds(Of T)(data() As T, rIds As Dictionary(Of Integer, Integer)) As T()
        Dim rowCount As Integer = UBound(data)

        ' Validate all requested indices
        For Each key In rIds.Keys
            If key < 0 OrElse key > rowCount Then
                AppGlobals.BSerr.LogAndThrow(New ArgumentOutOfRangeException(NameOf(rIds), $"Row index {key} is outside the valid range 0 to {rowCount}."))
            End If
        Next

        ' Prepare output array
        Dim newRowCount As Integer = rIds.Count - 1
        Dim tmp = data
        ReDim data(newRowCount)

        ' Copy selected elements
        Dim i As Integer = 0
        For Each key In rIds.Keys
            data(i) = tmp(key)
            i += 1
        Next

        Return data
    End Function

    ''' <summary>
    ''' Sorts a two-dimensional array of any type using the QuickSort algorithm with multiple column criteria.
    ''' </summary>
    ''' <typeparam name="T">
    ''' The element type of the array (e.g., String, Integer, Double).
    ''' </typeparam>
    ''' <param name="arr">
    ''' The two-dimensional array of type <typeparamref name="T"/> to be sorted.
    ''' </param>
    ''' <param name="Crit">
    ''' A comma-separated string specifying sort criteria.  
    ''' Format: "colIndex1,A|D,colIndex2,A|D,...".  
    ''' Example: "0,A,1,D" sorts first by column 0 ascending, then by column 1 descending.
    ''' </param>
    ''' <param name="Low">
    ''' The lower bound (row index) of the portion of the array to sort.
    ''' </param>
    ''' <param name="Up">
    ''' The upper bound (row index) of the portion of the array to sort.
    ''' </param>
    ''' <remarks>
    ''' - Sorting is performed in-place on <paramref name="arr"/>.  
    ''' - Multiple columns can be specified with ascending ("A") or descending ("D") order.  
    ''' - Uses <see cref="Comparer(Of T).Default"/> for comparisons.  
    ''' </remarks>
    ''' <example>
    ''' ' Example: sort a 2D array of strings by column 0 ascending, then column 1 descending
    ''' Dim data(,) As String = {
    '''     {"Alice", "25"},
    '''     {"Bob", "30"},
    '''     {"Charlie", "25"}
    ''' }
    ''' QuickSort2D(Of String)(data, "0,A,1,D", 0, UBound(data, 1))
    ''' </example>
    Public Sub QuickSort2D(Of T)(arr(,) As T, Crit As String, Low As Integer, Up As Integer)
        Dim Cr() As String = Split(Crit, ",")
        Dim a As Integer = (UBound(Cr) - 1) \ 2
        Dim Col(a) As Integer
        Dim AorD(a) As String

        a = 0
        For i = 0 To UBound(Cr) Step 2
            Col(a) = Integer.Parse(Cr(i))
            AorD(a) = Cr(i + 1)
            a += 1
        Next

        QuicksortCalc(arr, Col, AorD, Low, Up)
    End Sub

    ''' <summary>
    ''' Recursive QuickSort partitioning routine for two-dimensional arrays.
    ''' </summary>
    ''' <param name="varray">The array being sorted.</param>
    ''' <param name="Col">An array of column indices used for sorting.</param>
    ''' <param name="AorD">An array of "A"/"D" flags indicating ascending or descending order for each column.</param>
    ''' <param name="Low">The lower bound (row index) of the current partition.</param>
    ''' <param name="Up">The upper bound (row index) of the current partition.</param>
    ''' <remarks>
    ''' - Chooses a pivot row and partitions the array into two halves.  
    ''' - Recursively sorts each half until the entire array is ordered.  
    ''' - Swaps entire rows when necessary.  
    ''' </remarks>
    Private Sub QuicksortCalc(Of T)(varray(,) As T, Col() As Integer, AorD() As String, Low As Integer, Up As Integer)
        Dim tmpLow As Integer = Low
        Dim tmpHi As Integer = Up
        Dim pval(UBound(Col)) As T

        For i = 0 To UBound(pval)
            pval(i) = varray((Low + Up) \ 2, Col(i))
        Next

        Do While tmpLow <= tmpHi
            Do While Checkstr1(varray, tmpLow, Col, AorD, pval) AndAlso tmpLow < Up
                tmpLow += 1
            Loop
            Do While Checkstr2(varray, tmpHi, Col, AorD, pval) AndAlso tmpHi > Low
                tmpHi -= 1
            Loop

            If tmpLow <= tmpHi Then
                For i = 0 To UBound(varray, 2)
                    Dim vSwap As T = varray(tmpLow, i)
                    varray(tmpLow, i) = varray(tmpHi, i)
                    varray(tmpHi, i) = vSwap
                Next
                tmpLow += 1
                tmpHi -= 1
            End If
        Loop

        If Low < tmpHi Then QuicksortCalc(varray, Col, AorD, Low, tmpHi)
        If tmpLow < Up Then QuicksortCalc(varray, Col, AorD, tmpLow, Up)
    End Sub

    ''' <summary>
    ''' Compares a row against the pivot values to determine if it should move forward in QuickSort.
    ''' </summary>
    ''' <param name="arr">The array being sorted.</param>
    ''' <param name="tmpLow">The current row index being checked.</param>
    ''' <param name="Col">Column indices used for sorting.</param>
    ''' <param name="AorD">Ascending/descending flags for each column.</param>
    ''' <param name="pval">Pivot values for each column.</param>
    ''' <returns>
    ''' <c>True</c> if the row should move forward (less than pivot for ascending, greater for descending).  
    ''' Otherwise <c>False</c>.
    ''' </returns>
    Private Function Checkstr1(Of T)(arr(,) As T, tmpLow As Integer, Col() As Integer, AorD() As String, pval() As T) As Boolean
        Dim cmp As Comparer(Of T) = Comparer(Of T).Default
        For i = 0 To UBound(pval)
            Dim Str1 As T = arr(tmpLow, Col(i))
            If String.Equals(AorD(i), "A", StringComparison.OrdinalIgnoreCase) Then
                'If Str1.GetType() Is pval(i).GetType() Then
                If cmp.Compare(Str1, pval(i)) < 0 Then Return True
                If cmp.Compare(Str1, pval(i)) > 0 Then Return False
                'End If
            Else
                If cmp.Compare(Str1, pval(i)) > 0 Then Return True
                If cmp.Compare(Str1, pval(i)) < 0 Then Return False
            End If
        Next
        Return False
    End Function

    ''' <summary>
    ''' Compares a row against the pivot values to determine if it should move backward in QuickSort.
    ''' </summary>
    ''' <param name="arr">The array being sorted.</param>
    ''' <param name="tmpHi">The current row index being checked.</param>
    ''' <param name="Col">Column indices used for sorting.</param>
    ''' <param name="AorD">Ascending/descending flags for each column.</param>
    ''' <param name="pval">Pivot values for each column.</param>
    ''' <returns>
    ''' <c>True</c> if the row should move backward (greater than pivot for ascending, less for descending).  
    ''' Otherwise <c>False</c>.
    ''' </returns>
    Private Function Checkstr2(Of T)(arr(,) As T, tmpHi As Integer, Col() As Integer, AorD() As String, pval() As T) As Boolean
        Dim cmp As Comparer(Of T) = Comparer(Of T).Default
        For i = 0 To UBound(pval)
            Dim Str1 As T = arr(tmpHi, Col(i))
            If String.Equals(AorD(i), "A", StringComparison.OrdinalIgnoreCase) Then
                If cmp.Compare(pval(i), Str1) < 0 Then Return True
                If cmp.Compare(pval(i), Str1) > 0 Then Return False
            Else
                If cmp.Compare(pval(i), Str1) > 0 Then Return True
                If cmp.Compare(pval(i), Str1) < 0 Then Return False
            End If
        Next
        Return False
    End Function

End Module
