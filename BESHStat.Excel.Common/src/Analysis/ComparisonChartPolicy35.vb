Option Explicit On
Option Strict On
Option Infer On
Option Compare Text
Imports System
Imports System.Linq
Imports System.Collections.Generic
Imports BESHStatNG
Namespace ExcelCommon
Public NotInheritable Class PlotPointLabel35
Public Sub New(orderedOrdinal As Integer,text As String)
Me.OrderedOrdinal=orderedOrdinal:ExcelPointIndex=ComparisonChartPolicy35.PointIndex(orderedOrdinal):Me.Text=text
End Sub
Public ReadOnly Property OrderedOrdinal As Integer
Public ReadOnly Property ExcelPointIndex As Integer
Public ReadOnly Property Text As String
End Class
Public Module ComparisonChartPolicy35

        Public Sub ValidatePlotData(data As parametric.MultipleComparisonPlotData)
            If data Is Nothing Then Throw New ArgumentNullException(NameOf(data))
            If data.Labels Is Nothing OrElse data.Estimates Is Nothing OrElse
               data.LowerLimits Is Nothing OrElse data.UpperLimits Is Nothing Then
                Throw New ArgumentException("Multiple-comparison plot data are incomplete.", NameOf(data))
            End If
            Dim n As Integer = data.Estimates.Length
            If n = 0 OrElse data.Labels.Length <> n OrElse data.LowerLimits.Length <> n OrElse
               data.UpperLimits.Length <> n Then
                Throw New ArgumentException("Multiple-comparison plot arrays must have equal non-zero lengths.", NameOf(data))
            End If
        End Sub

        Public Function BuildTitle(data As parametric.MultipleComparisonPlotData) As String
            Dim confidenceLevel As Double = (1.0 - data.Alpha) * 100.0
            Dim qualifier As String
            Select Case data.ProcedureName
                Case "Tukey-Kramer", "Bonferroni", "Games-Howell"
                    qualifier = " Simultaneous "
                Case Else
                    qualifier = " "
            End Select
            Return data.ProcedureName & qualifier & confidenceLevel.ToString("0.##") & "% CIs" & Convert.ToChar(10).ToString() &
                   "Differences of Means"
        End Function
Public Function LabelOrdinals(pointCount As Integer,labelCount As Integer) As Integer()
Dim result As New List(Of Integer)
For i As Integer=1 To Math.Min(pointCount,labelCount)
result.Add(i-1)
Next
Return result.ToArray()
End Function
Public Function LabelAt(labels As String(),orderedOrdinal As Integer) As PlotPointLabel35
Return New PlotPointLabel35(orderedOrdinal,labels(orderedOrdinal))
End Function
Public Function PointIndex(orderedOrdinal As Integer) As Integer
Return orderedOrdinal+1
End Function
Public Function ValueAxisMaximum(count As Integer) As Double
Return count+0.8
End Function
End Module
End Namespace
