Option Explicit On
Option Strict On
Option Infer On
Imports System
Imports System.Linq
Imports System.Collections.Generic

Namespace ExcelCommon
Public Module PsmLoveSeriesPolicy
Public Function HasAnyFinite(values As IEnumerable(Of Double)) As Boolean
            If values Is Nothing Then Return False
            Return values.Any(Function(v) Not Double.IsNaN(v) AndAlso Not Double.IsInfinity(v))
        End Function
    Public Function ThresholdReference(threshold As Double, count As Integer) As ChartReferenceSeries
        Return ChartReferenceSeries.Vertical(threshold, 0.5R, count + 0.5R)
    End Function
End Module
End Namespace
