Option Explicit On
Option Strict On
Option Infer On
Imports System
Imports System.Linq
Imports System.Globalization
Imports System.Collections.Generic
Imports BESHStatNG
Namespace Global.Batch34CompletionFixtures
Public Module LegacySpcBasis34
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
End Module
End Namespace
