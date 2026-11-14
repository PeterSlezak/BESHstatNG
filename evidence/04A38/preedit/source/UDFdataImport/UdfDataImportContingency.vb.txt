Option Explicit On
Option Strict On

' Contingency-table data import helpers for worksheet UDFs.
' Keeps table UDF modules routed through the shared UdfDataImport facade instead of local range parsing.
Partial Friend Module UdfDataImport

    ''' <summary>
    ''' Imports a numeric matrix as a non-negative integer contingency table.
    ''' A single top header row containing non-numeric labels is accepted by the underlying numeric matrix reader.
    ''' </summary>
    Friend Function TryGetContingencyTable(input As Object,
                                           ByRef table(,) As Integer) As Boolean
        table = Nothing

        Dim mat(,) As Double = Nothing
        Dim rows As Integer = 0
        Dim cols As Integer = 0
        If Not TryGetNumericMatrix(input, mat, rows, cols) Then Return False
        If rows < 1 OrElse cols < 1 Then Return False

        ReDim table(rows - 1, cols - 1)
        For i As Integer = 0 To rows - 1
            For j As Integer = 0 To cols - 1
                Dim x As Double = mat(i, j)
                If Double.IsNaN(x) OrElse Double.IsInfinity(x) OrElse x < 0.0R Then Return False
                Dim rounded As Double = Math.Round(x)
                If Math.Abs(x - rounded) > 0.0000001R Then Return False
                table(i, j) = CInt(rounded)
            Next
        Next

        Return True
    End Function

End Module
