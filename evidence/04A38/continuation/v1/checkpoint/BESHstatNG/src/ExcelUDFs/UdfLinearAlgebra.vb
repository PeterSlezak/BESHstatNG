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
        Return Global.BESHStatNG.WorksheetCompatibility.UdfNumericPreparation38.TryInvertMatrix(a, inv)
    End Function

End Module
