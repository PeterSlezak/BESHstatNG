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

    End Module

End Namespace
