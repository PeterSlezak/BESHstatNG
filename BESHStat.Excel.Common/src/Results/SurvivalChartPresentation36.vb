Option Explicit On
Option Strict On
Option Infer On
Imports System
Imports System.Linq
Imports System.Collections.Generic
Imports BESHStatNG.survival
Namespace ExcelCommon
Public NotInheritable Class SurvivalChartPresentation36
    Public Const MaximumProbability As Double=1.0
    Public Const ProbabilityMajorUnit As Double=0.2
    Public Const ProbabilityTitle As String="Survival Probability"
    Public Const CoxTimeTitle As String="Time"
    Public Const CoxTitle As String="Cox - Survival plot"
    Public Const BaselineName As String="Baseline"
    Private Sub New()
    End Sub
    'Called at original name-assignment time, not while preparing earlier curves.
    Public Shared Function SeriesName(data As KaplanMeierPlotData, group As Integer, role As KaplanMeierSeriesRole36) As String
        If role=KaplanMeierSeriesRole36.Survival Then Return data.GroupLabels(group)
        If role=KaplanMeierSeriesRole36.Censor Then Return "Censored " + data.GroupLabels(group)
        Return $"{100.0 * (1.0 - data.Alpha):0.##}% CI " + data.GroupLabels(group)
    End Function
    Public Shared Function KmTimeTitle(unit As String) As String
        Return $"Time ({unit})"
    End Function
    Public Shared Function DeleteLegend(groups As Integer, requested As Boolean) As Boolean
        Return groups=1 OrElse Not requested
    End Function
End Class
End Namespace
