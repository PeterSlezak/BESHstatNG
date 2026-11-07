Option Explicit On
Option Strict On
Option Infer On
Imports System
Imports System.Linq
Imports System.Collections.Generic
Imports BESHStatNG
Imports BESHStatNG.StatisticalProcessControl

Namespace Global.BESHStatNG.TestFixtures34
Public Module LegacyReference34
Public Function ComparisonBasis(estimates As Double(), lower As Double(), upper As Double()) As Double()()
            Dim n As Integer = estimates.Length
            Dim y(n - 1) As Double
            Dim minCI As Double = lower(0)
            Dim maxCI As Double = upper(0)

            'Reverse the numeric y positions so the first comparison is shown at the top.
            For i As Integer = 0 To n - 1
                y(i) = n - i
                minCI = Math.Min(minCI, lower(i))
                maxCI = Math.Max(maxCI, upper(i))
            Next

            'Always include zero in the visible range because it is the null reference.
            minCI = Math.Min(minCI, 0.0)
            maxCI = Math.Max(maxCI, 0.0)
Return New Double()(){y,New Double(){minCI,maxCI}}
End Function
Public Function SpcBounds(minimum As Double, maximum As Double, panelType As SpcPanelType) As ChartAxisScaleResult
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
Public Function FitLine(minimum As Double, maximum As Double, intercept As Double, slope As Double) As Double()()
Return New Double()(){New Double(){minimum,maximum},New Double(){intercept + slope * minimum,intercept + slope * maximum}}
End Function
Public Function HasAnyFinite(values As IEnumerable(Of Double)) As Boolean
            If values Is Nothing Then Return False
            Return values.Any(Function(v) Not Double.IsNaN(v) AndAlso Not Double.IsInfinity(v))
        End Function
End Module
End Namespace
