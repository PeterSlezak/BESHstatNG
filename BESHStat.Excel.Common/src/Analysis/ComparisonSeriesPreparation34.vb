Option Explicit On
Option Strict On
Option Infer On
Imports System
Imports System.Linq
Imports System.Globalization
Imports System.Collections.Generic
Imports BESHStatNG
Namespace ExcelCommon.Chart34.ComparisonSeries
Public Module ComparisonSeriesPreparation34
        Public Function SuggestedHeight(data As parametric.MultipleComparisonPlotData) As Double
            If data Is Nothing OrElse data.Estimates Is Nothing Then Return 320.0
            Return Math.Max(320.0, Math.Min(720.0, 150.0 + 32.0 * data.Estimates.Length))
        End Function
End Module
End Namespace
