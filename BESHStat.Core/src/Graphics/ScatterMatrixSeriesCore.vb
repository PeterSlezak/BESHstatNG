Option Explicit On
Option Strict On
Imports System
Imports System.Linq
Public Module ScatterMatrixSeriesCore
    Public Function Correlations(data(,) As Double) As Double(,)
        Return Matrix.CorrelationMatrixCore.Compute(DirectCast(data.Clone(), Double(,)), "r")
    End Function
    Public Function GridPosition(index As Integer, variables As Integer) As Double
        Return index * 1 / variables
    End Function
    Public Function PanelValues(data(,) As Double, variableOrdinal As Integer) As Double()
        Return DataManagement.ArrayUtilities.GetColumn(DirectCast(data.Clone(), Double(,)), variableOrdinal)
    End Function
    Public Function PanelCoordinates(input(,) As Double, margin As Double, regression As Boolean) As ScatterMatrixPanelData
        Dim ScaledData = DirectCast(input.Clone(), Double(,)), p = input.GetLength(1)
        Dim pMargin As Double = margin, pbDisplayRegLines As Boolean = regression
        Dim tmp(), tmp2() As Double
        Dim intercepts_scaled(p - 1, p - 1), slopes_scaled(p - 1, p - 1) As Double
        Dim x_corr_labels(p * p - 1), y_corr_labels(p * p - 1) As Double
        Dim x_regline(p * (p - 1) * 3 - 1), y_regline(p * (p - 1) * 3 - 1) As Double
        Const gToDeleteGridLineValue As Double = -100
            If pbDisplayRegLines Then
                For i = 0 To p - 1
                    tmp = DataManagement.ArrayUtilities.GetColumn(ScaledData, i)
                    For j = 0 To p - 1
                        tmp2 = DataManagement.ArrayUtilities.GetColumn(ScaledData, j)
                        intercepts_scaled(i, j) = Intercept(tmp2, tmp)
                        slopes_scaled(i, j) = Slope(tmp2, tmp)
                    Next
                Next
            End If

            Dim ii As Integer = 0
            Dim iii As Integer = 0
            For i = 1 To p
                For j = 1 To p
                    x_corr_labels(ii) = (i - 1) * (1 / p)
                    y_corr_labels(ii) = (j - 1) * (1 / p) + ((1 / p) * (pMargin / 2))
                    ii += 1

                    If pbDisplayRegLines Then
                        If i <> j Then
                            For jj = 1 To 3
                                If jj = 1 Then
                                    x_regline(iii) = (i - 1) * (1 / p) + ((1 / p) * pMargin)
                                    y_regline(iii) = slopes_scaled(i - 1, j - 1) * x_regline(iii) + intercepts_scaled(i - 1, j - 1)
                                ElseIf jj = 2 Then
                                    x_regline(iii) = i * (1 / p) - ((1 / p) * pMargin)
                                    y_regline(iii) = slopes_scaled(i - 1, j - 1) * x_regline(iii) + intercepts_scaled(i - 1, j - 1)
                                Else 'indication to set line segment to invisible
                                    x_regline(iii) = gToDeleteGridLineValue
                                    y_regline(iii) = gToDeleteGridLineValue
                                End If

                                If i = j Then
                                    x_regline(iii) = gToDeleteGridLineValue
                                    y_regline(iii) = gToDeleteGridLineValue
                                End If
                                iii += 1
                            Next jj
                        End If
                    End If
                Next
            Next i
        Return New ScatterMatrixPanelData(ScaledData, x_corr_labels, y_corr_labels, x_regline, y_regline)
    End Function
End Module
