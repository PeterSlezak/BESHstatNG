Option Explicit On
Option Strict On
Option Infer On
Imports System
Imports System.Linq
Imports System.Globalization
Imports System.Collections.Generic
Imports BESHStatNG
Imports BESHStatNG.StatisticalProcessControl
Namespace ExcelCommon.Chart34
Public NotInheritable Class PolarChartSettings34
Public ReadOnly Property SeriesName As String
Public Sub New(Optional inputSeriesName As String = "Data")
Me.SeriesName = inputSeriesName
End Sub
End Class
Public Module PolarChartPreparation34


    Public Function ResolveSeriesName(group As PolarPlotGroupSeries,
                                              groupIndex As Integer,
                                              result As PolarPlotResult,
                                              appearance As PolarChartSettings34) As String
        If result.HasGrouping Then
            If Not String.IsNullOrWhiteSpace(group.Name) Then Return group.Name
            Return "Group " & (groupIndex + 1).ToString(Globalization.CultureInfo.CurrentCulture)
        End If
        Return If(String.IsNullOrWhiteSpace(appearance.SeriesName),
                  "Data",
                  appearance.SeriesName.Trim())
    End Function
End Module
End Namespace
