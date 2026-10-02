Option Explicit On
Option Strict On

Imports BESHStatNG.AppInfrastructure
Imports BESHStatNG.DataManagement

Public Module Helpers

    ''' <summary>
    ''' Resolves the actual pseudo-random seed that will be used for bootstrap resampling.
    ''' </summary>
    ''' <param name="requestedSeed">
    ''' Explicit seed requested by the caller. Use <see cref="Integer.MinValue"/> to indicate that no explicit seed was supplied.
    ''' </param>
    ''' <returns>
    ''' The explicit seed if one was supplied; otherwise the global default seed; otherwise a time-based seed captured from <see cref="Environment.TickCount"/>.
    ''' </returns>
    ''' <remarks>
    ''' Returning the concrete seed value allows the bootstrap run to be reproduced exactly and reported back in the result output.
    ''' </remarks>
    Friend Function ResolveRandomSeed(Optional requestedSeed As Integer = Integer.MinValue) As Integer
        Return CoreServices.AnalysisDefaults.ResolveRandomSeed(requestedSeed, generateWhenMissing:=True)
    End Function

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
        Return RowArrayUtilities.SubsetRowsByIds(data, rIds.Keys)
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
        Return RowArrayUtilities.FindCommonItems(arr1, arr2, comparer)
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
        Return RowArrayUtilities.SubsetItemsByIds(data, rIds.Keys)
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
        RowArrayUtilities.SortRows(arr, Crit, Low, Up)
    End Sub

End Module
