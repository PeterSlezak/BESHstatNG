Option Explicit On
Option Strict On

''' <summary>
''' Small numerical utilities shared by worksheet UDF modules.
''' </summary>
Friend Module UdfLinearAlgebra

    ''' <summary>
    ''' Inverts a square matrix using the shared regression-model inversion routine.
    ''' </summary>
    ''' <param name="a">The square matrix to invert.</param>
    ''' <param name="inv">On success, receives the inverse matrix.</param>
    ''' <returns>True when inversion succeeds; otherwise, False.</returns>
    Friend Function TryInvertMatrix(a As Double(,), ByRef inv As Double(,)) As Boolean
        inv = Nothing
        If a Is Nothing Then Return False
        If a.Rank <> 2 Then Return False

        Dim nRows As Integer = a.GetLength(0)
        Dim nCols As Integer = a.GetLength(1)
        If nRows <> nCols OrElse nRows = 0 Then Return False

        Try
            Dim iErr As Integer = 0
            inv = Global.BESHStatNG.Matrix.Matrix.MatInv(a, "SVD", iErr, False)

            If iErr <> 0 OrElse inv Is Nothing Then
                inv = Nothing
                Return False
            End If

            If inv.GetLength(0) <> nRows OrElse inv.GetLength(1) <> nCols Then
                inv = Nothing
                Return False
            End If

            Return True
        Catch
            inv = Nothing
            Return False
        End Try
    End Function

End Module
