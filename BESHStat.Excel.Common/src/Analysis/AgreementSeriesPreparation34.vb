Option Explicit On
Option Strict On
Option Infer On
Imports System
Imports System.Linq
Imports System.Globalization
Imports System.Collections.Generic
Imports BESHStatNG
Namespace ExcelCommon.Chart34.AgreementSeries
Public Module AgreementSeriesPreparation34
Public Function BaseSpan(values As Double()) As ChartAxisScaleResult
Dim xMin As Double = values.Min()
Dim xMax As Double = values.Max()
If xMin = xMax Then
xMin -= 0.5
xMax += 0.5
End If
Return New ChartAxisScaleResult(xMin,xMax,0.0R)
End Function

End Module
End Namespace
