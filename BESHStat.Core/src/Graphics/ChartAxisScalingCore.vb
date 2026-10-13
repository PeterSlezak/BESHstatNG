Option Explicit On
Option Strict On
Option Infer On

Imports System

''' <summary>
''' Portable numerical axis limits and major-unit computations for statistical charts.
''' No application-specific renderer or platform dependency is required.
''' </summary>
Public NotInheritable Class ChartAxisScaleResult
    Public ReadOnly Property Min As Double
    Public ReadOnly Property Max As Double
    Public ReadOnly Property Scale As Double

    Public Sub New(minimum As Double, maximum As Double, majorUnit As Double)
        Me.Min = minimum
        Me.Max = maximum
        Me.Scale = majorUnit
    End Sub
End Class

''' <summary>
''' Reproduces the legacy ChartingFunc.ChartScaling calculation, including its
''' handling of reversed or degenerate limits and its round-down behavior.
''' The source algorithm is described in Professional Excel Development (2nd ed.).
''' Do not add validation or normalize the outputs here: existing Excel plots
''' depend on the legacy numerical behavior.
''' </summary>
Public Module ChartAxisScalingCore
    Public Function Compute(dMin As Double, dMax As Double) As ChartAxisScaleResult
        Dim dPower As Double
        Dim dScale As Double
        Dim localDMin As Double = dMin
        Dim localDMax As Double = dMax

        'Retain the legacy order of operations (the two one-percent adjustments
        'are intentionally asymmetric, because the second sees the updated max).
        If localDMax = localDMin Then
            dScale = localDMax
            localDMax = localDMax * 1.01R
            localDMin = localDMin * 0.99R
        End If

        If localDMax < localDMin Then
            dScale = localDMax
            localDMax = localDMin
            localDMin = dScale
        End If

        If localDMax > 0.0R Then
            localDMax = localDMax + (localDMax - localDMin) * 0.01R
        Else
            localDMax = localDMax - (localDMax - localDMin) * 0.01R
        End If
        If localDMin > 0.0R Then
            localDMin = localDMin + (localDMax - localDMin) * 0.01R
        Else
            localDMin = localDMin - (localDMax - localDMin) * 0.01R
        End If

        If localDMax = 0.0R AndAlso localDMin = 0.0R Then localDMax = 1.0R

        dPower = Math.Log(localDMax - localDMin) / Math.Log(10.0R)
        dScale = Math.Pow(10.0R, dPower - Math.Floor(dPower))

        Select Case dScale
            Case 0.0R To 2.5R
                dScale = 0.2R
            Case 2.5R To 5.0R
                dScale = 0.5R
            Case 5.0R To 7.5R
                dScale = 1.0R
            Case Else
                dScale = 2.0R
        End Select

        dScale = dScale * Math.Pow(10.0R, Math.Floor(dPower))

        'VB Int(Double) rounds down, not toward zero. Floor preserves that
        'behavior for negative limits; truncation here would change chart axes.
        Return New ChartAxisScaleResult(
            dScale * Math.Floor(localDMin / dScale),
            dScale * (Math.Floor(localDMax / dScale) + 1.0R),
            dScale)
    End Function
End Module
