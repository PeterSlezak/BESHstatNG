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
Public NotInheritable Class HullChartSettings34
Public ReadOnly Property SeriesName As String
Public Sub New(Optional inputSeriesName As String = "Data")
Me.SeriesName = inputSeriesName
End Sub
End Class
Public Module HullChartPreparation34


    Public Function ResolveSeriesName(group As ConvexHullPlotGroup,
                                              groupIndex As Integer,
                                              hasGrouping As Boolean,
                                              appearance As HullChartSettings34) As String
        If hasGrouping Then
            If Not String.IsNullOrWhiteSpace(group.Name) Then Return group.Name
            Return "Group " & (groupIndex + 1).ToString(CultureInfo.CurrentCulture)
        End If
        Return If(String.IsNullOrWhiteSpace(appearance.SeriesName), "Data", appearance.SeriesName.Trim())
    End Function
End Module
End Namespace
