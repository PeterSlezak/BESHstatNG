Option Explicit On
Option Strict On

Imports System
Imports BESHStatNG.AppInfrastructure

Namespace DataManagement

    ''' <summary>
    ''' Host-neutral one- and two-dimensional array helpers used by statistical data objects.
    ''' </summary>
    Public Module ArrayUtilities

        ''' <summary>
        ''' Returns an inclusive slice of a one-dimensional array. A value of -1 for
        ''' <paramref name="endIndex"/> means the last element. If start is greater than end,
        ''' the existing BESHStatNG behavior is preserved by coercing start to end.
        ''' </summary>
        Public Function Slice(Of T)(values() As T,
                                    Optional startIndex As Integer = 0,
                                    Optional endIndex As Integer = -1) As T()
            If endIndex = -1 Then endIndex = values.GetUpperBound(0)
            If startIndex > endIndex Then startIndex = endIndex

            Dim output(endIndex - startIndex) As T
            Dim outputIndex As Integer = 0

            For sourceIndex As Integer = startIndex To endIndex
                output(outputIndex) = values(sourceIndex)
                outputIndex += 1
            Next

            Return output
        End Function

        ''' <summary>
        ''' Extracts a zero-based column from a two-dimensional array.
        ''' </summary>
        Public Function GetColumn(Of T)(values(,) As T, columnIndex As Integer) As T()
            Dim output(values.GetUpperBound(0)) As T

            If columnIndex > values.GetUpperBound(1) Then
                CoreServices.Errors.LogAndThrow(
                    New ArgumentException("Provided column number is larger than array 2nd dimension."))
            End If

            For rowIndex As Integer = 0 To values.GetUpperBound(0)
                output(rowIndex) = values(rowIndex, columnIndex)
            Next

            Return output
        End Function

        ''' <summary>
        ''' Concatenates two one-dimensional arrays without changing element order.
        ''' </summary>
        Public Function ConcatArrays(Of T)(a1() As T, a2() As T) As T()
            Dim output(a1.Length + a2.Length - 1) As T
            Array.Copy(a1, 0, output, 0, a1.Length)
            Array.Copy(a2, 0, output, a1.Length, a2.Length)
            Return output
        End Function

        ''' <summary>
        ''' Legacy name: stacks two matrices SIDE BY SIDE (appends columns).
        ''' Mismatched row counts require bAppendBlanks=True; padding uses default(T).
        ''' </summary>
        Public Function VerticalStackArrays(Of T)(a1(,) As T,
                                                  a2(,) As T,
                                                  Optional bAppendBlanks As Boolean = False) As T(,)
            If a1.GetLength(0) <> a2.GetLength(0) AndAlso Not bAppendBlanks Then
                CoreServices.Errors.LogAndThrow(New ArgumentException("Invalid input array dimensions"))
            End If

            Dim rowCount As Integer = Math.Max(a1.GetLength(0), a2.GetLength(0))
            Dim colCount As Integer = a1.GetLength(1) + a2.GetLength(1)
            Dim output As T(,) = DirectCast(Array.CreateInstance(GetType(T), rowCount, colCount), T(,))

            For i As Integer = 0 To a1.GetLength(0) - 1
                For j As Integer = 0 To a1.GetLength(1) - 1
                    output(i, j) = a1(i, j)
                Next
            Next
            For i As Integer = 0 To a2.GetLength(0) - 1
                For j As Integer = 0 To a2.GetLength(1) - 1
                    output(i, a1.GetLength(1) + j) = a2(i, j)
                Next
            Next
            Return output
        End Function

        ''' <summary>
        ''' Legacy name: stacks two matrices TOP TO BOTTOM (appends rows).
        ''' Mismatched column counts require bAppendBlanks=True; padding uses default(T).
        ''' </summary>
        Public Function HorizontalStackArrays(Of T)(a1(,) As T,
                                                    a2(,) As T,
                                                    Optional bAppendBlanks As Boolean = False) As T(,)
            If a1.GetLength(1) <> a2.GetLength(1) AndAlso Not bAppendBlanks Then
                CoreServices.Errors.LogAndThrow(New ArgumentException("Invalid input array dimensions"))
            End If

            Dim rowCount As Integer = a1.GetLength(0) + a2.GetLength(0)
            Dim colCount As Integer = Math.Max(a1.GetLength(1), a2.GetLength(1))
            Dim output As T(,) = DirectCast(Array.CreateInstance(GetType(T), rowCount, colCount), T(,))

            For i As Integer = 0 To a1.GetLength(0) - 1
                For j As Integer = 0 To a1.GetLength(1) - 1
                    output(i, j) = a1(i, j)
                Next
            Next
            For i As Integer = 0 To a2.GetLength(0) - 1
                For j As Integer = 0 To a2.GetLength(1) - 1
                    output(a1.GetLength(0) + i, j) = a2(i, j)
                Next
            Next
            Return output
        End Function

        ''' <summary>Uses Object.ToString, with Nothing mapped to an empty string.</summary>
        Public Function Array2strArray(Of T)(x() As T) As String()
            Dim output(x.Length - 1) As String
            For i As Integer = 0 To x.Length - 1
                Dim value As Object = x(i)
                output(i) = If(value Is Nothing, String.Empty, value.ToString())
            Next
            Return output
        End Function

        ''' <summary>Uses Convert.ToString, with Nothing mapped to an empty string.</summary>
        Public Function Array2strArray(Of T)(x(,) As T) As String(,)
            Dim output As String(,) = DirectCast(Array.CreateInstance(GetType(String), x.GetLength(0), x.GetLength(1)), String(,))
            For i As Integer = 0 To x.GetLength(0) - 1
                For j As Integer = 0 To x.GetLength(1) - 1
                    Dim value As Object = x(i, j)
                    output(i, j) = If(value Is Nothing, String.Empty, Convert.ToString(value))
                Next
            Next
            Return output
        End Function

        ''' <summary>Converts using Convert.ToDouble and maps Nothing to zero.</summary>
        Public Function Array2dblArray(Of T)(x() As T) As Double()
            Dim output(x.Length - 1) As Double
            For i As Integer = 0 To x.Length - 1
                Dim value As Object = x(i)
                output(i) = If(value Is Nothing, 0.0R, Convert.ToDouble(value))
            Next
            Return output
        End Function

        ''' <summary>Converts using Convert.ToInt32 (midpoint-to-even) and maps Nothing to zero.</summary>
        Public Function Array2intArray(Of T)(x() As T) As Integer()
            Dim output(x.Length - 1) As Integer
            For i As Integer = 0 To x.Length - 1
                Dim value As Object = x(i)
                output(i) = If(value Is Nothing, 0, Convert.ToInt32(value))
            Next
            Return output
        End Function

        ''' <summary>Converts using Convert.ToInt32 (midpoint-to-even) and maps Nothing to zero.</summary>
        Public Function Array2intArray(Of T)(x(,) As T) As Integer(,)
            Dim output As Integer(,) = DirectCast(Array.CreateInstance(GetType(Integer), x.GetLength(0), x.GetLength(1)), Integer(,))
            For i As Integer = 0 To x.GetLength(0) - 1
                For j As Integer = 0 To x.GetLength(1) - 1
                    Dim value As Object = x(i, j)
                    output(i, j) = If(value Is Nothing, 0, Convert.ToInt32(value))
                Next
            Next
            Return output
        End Function

        ''' <summary>Boxes each element without altering the input matrix dimensions.</summary>
        Public Function Array2objArray(Of T)(x(,) As T) As Object(,)
            Dim output As Object(,) = DirectCast(Array.CreateInstance(GetType(Object), x.GetLength(0), x.GetLength(1)), Object(,))
            For i As Integer = 0 To x.GetLength(0) - 1
                For j As Integer = 0 To x.GetLength(1) - 1
                    output(i, j) = x(i, j)
                Next
            Next
            Return output
        End Function

    End Module

End Namespace
