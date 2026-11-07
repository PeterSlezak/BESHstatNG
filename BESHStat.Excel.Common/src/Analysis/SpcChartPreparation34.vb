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
Public NotInheritable Class SpcChartSettings34
Public ReadOnly Property ChartTitle As String
Public Sub New(Optional inputChartTitle As String = "")
Me.ChartTitle = inputChartTitle
End Sub
End Class
Public Module SpcChartPreparation34


        Public Function NormalizeText(value As String) As String
            Return If(value, String.Empty).Trim()
        End Function
        Public Function NormalizeNumberFormat(value As String) As String
            Dim normalized As String = NormalizeText(value)
            If normalized.Length = 0 Then Return "0.####"
            Return normalized
        End Function
        Public Function ResolveChartTitle(
            result As SpcFitResult,
            panel As SpcPanelResult,
            options As SpcChartSettings34,
            panelCount As Integer) As String

            Dim baseTitle As String = options.ChartTitle
            If baseTitle.Length = 0 Then baseTitle = result.ChartTitle
            If panelCount <= 1 Then Return baseTitle
            Return baseTitle & " - " & panel.DisplayName
        End Function
        Public Function ResolveValueAxisTitle(
            result As SpcFitResult,
            panel As SpcPanelResult) As String
            If panel.ValueAxisTitle.Length > 0 Then Return panel.ValueAxisTitle
            If result.Request.ValueAxisTitle.Length > 0 Then Return result.Request.ValueAxisTitle
            Return panel.DisplayName
        End Function
        Public Function PanelSupportsSpecificationLimits(
            panelType As SpcPanelType) As Boolean
            Select Case panelType
                Case SpcPanelType.Run,
                     SpcPanelType.IndividualValue,
                     SpcPanelType.SubgroupMean,
                     SpcPanelType.Ewma,
                     SpcPanelType.MovingAverage
                    Return True
                Case Else
                    Return False
            End Select
        End Function
End Module
End Namespace
