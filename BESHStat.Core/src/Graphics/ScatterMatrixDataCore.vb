Option Explicit On
Option Strict On

Imports System
Imports System.Linq

''' <summary>Legacy n-by-p panel normalization, including nonfinite constant-span results.
''' Caller chooses margin; the Windows adapter deliberately still supplies its overwritten 0.1.
''' Correlation/regression/cell geometry and -100 breaks are not changed here.</summary>
Public Module ScatterMatrixDataCore

    Public Function Normalize(input As Double(,), margin As Double) As ScatterMatrixNormalizedData
        Dim data = DirectCast(input.Clone(), Double(,))
        Dim n = data.GetLength(0), p = data.GetLength(1)
        Dim tmp() As Double
        Dim min_old(p - 1) As Double, max_old(p - 1) As Double, range_old(p - 1) As Double, range_fract(p - 1) As Double
        Dim ScaledData(n - 1, p - 1) As Double, x_var_labels(p - 1) As Double
        Dim min_new As Double =(1 / p) * margin
        Dim max_new As Double =(1 / p) * (1 - margin)
        Dim range_new As Double = max_new - min_new
        For j = 1 To p ' loop over all variables/columns
            tmp = DataManagement.ArrayUtilities.GetColumn(data, j - 1)
            min_old(j - 1) = tmp.Min()
            max_old(j - 1) = tmp.Max()
            range_old(j - 1) = max_old(j - 1) - min_old(j - 1)
            range_fract(j - 1) = range_new / range_old(j - 1)
            x_var_labels(j - 1) =(j - 1) * (1 / p) + (1 / p) / 2
        Next

        For i = 0 To n - 1
            For j = 1 To p
                ScaledData(i, j - 1) =((j - 1.0) * (1.0 / p)) + (data(i, j - 1) - min_old(j - 1)) * range_fract(j - 1) + min_new
            Next
        Next

        Return New ScatterMatrixNormalizedData(ScaledData, x_var_labels)
    End Function
End Module

Public NotInheritable Class ScatterMatrixNormalizedData

    Private ReadOnly data(,) As Double, labels() As Double

    Friend Sub New(values As Double(,), labelX As Double())
        data = DirectCast(values.Clone(), Double(,)) : labels = DirectCast(labelX.Clone(), Double())
    End Sub

    Public ReadOnly Property Values As Double(,)
        Get
            Return DirectCast(data.Clone(), Double(,))
        End Get
    End Property

    Public ReadOnly Property DiagonalLabelX As Double()
        Get
            Return DirectCast(labels.Clone(), Double())
        End Get
    End Property
End Class

