Option Explicit On
Option Strict On
Option Infer On
Imports System
Imports System.Linq
Imports System.Collections.Generic
Imports BESHStatNG.StatisticalProcessControl

Public Module SpcAxisBoundsCore
Public Function SpcOptionalExtent(value As Nullable(Of Double), minimum As Double, maximum As Double) As ChartAxisScaleResult
If Not value.HasValue Then Return New ChartAxisScaleResult(minimum,maximum,0.0R)
If value.Value < minimum Then minimum = value.Value
If value.Value > maximum Then maximum = value.Value
Return New ChartAxisScaleResult(minimum,maximum,0.0R)
End Function
Public Function SpcConstantReference(count As Integer, value As Double) As Double()
Dim values(count - 1) As Double
For i As Integer = 0 To values.Length - 1
values(i) = value
Next
Return values
End Function
Public Function SpcPanelTop(top As Double, panelIndex As Integer, panelHeight As Double, panelSpacing As Double) As Double
Return top + panelIndex * (panelHeight + panelSpacing)
End Function

    Public Function FromExtents(minimum As Double, maximum As Double, panelType As SpcPanelType) As ChartAxisScaleResult
            If Double.IsPositiveInfinity(minimum) OrElse
               Double.IsNegativeInfinity(maximum) Then
                Return New ChartAxisScaleResult(0.0R, 1.0R, 0.0R)
            End If

            If minimum = maximum Then
                Dim scale As Double = Math.Max(1.0R, Math.Abs(minimum))
                minimum -= 0.1R * scale
                maximum += 0.1R * scale
            Else
                Dim padding As Double = 0.07R * (maximum - minimum)
                minimum -= padding
                maximum += padding
            End If

            If HasNaturalZero(panelType) AndAlso minimum >= -0.15R * Math.Max(1.0R, maximum) Then
                minimum = 0.0R
            End If
            If panelType = SpcPanelType.Proportion AndAlso
               maximum > 1.0R AndAlso maximum <= 1.1R Then
                maximum = 1.0R
            End If
            If maximum <= minimum Then maximum = minimum + 1.0R

            Return New ChartAxisScaleResult(minimum, maximum, 0.0R)
    End Function
Private Function HasNaturalZero(panelType As SpcPanelType) As Boolean
            Select Case panelType
                Case SpcPanelType.MovingRange,
                     SpcPanelType.SubgroupRange,
                     SpcPanelType.SubgroupStandardDeviation,
                     SpcPanelType.Proportion,
                     SpcPanelType.NumberNonconforming,
                     SpcPanelType.DefectCount,
                     SpcPanelType.DefectRate,
                     SpcPanelType.EventsBetweenOccurrences,
                     SpcPanelType.TimeBetweenOccurrences,
                     SpcPanelType.UpperCusum,
                     SpcPanelType.LowerCusum,
                     SpcPanelType.HotellingT2,
                     SpcPanelType.GeneralizedVariance,
                     SpcPanelType.PcaT2,
                     SpcPanelType.PcaQ,
                     SpcPanelType.Mewma,
                     SpcPanelType.Mcusum
                    Return True
                Case Else
                    Return False
            End Select
        End Function
End Module
