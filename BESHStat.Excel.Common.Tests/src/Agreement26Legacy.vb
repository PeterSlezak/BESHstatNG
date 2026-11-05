Option Explicit On
Option Strict On
Option Infer On
Imports System
Imports BESHStatNG.Agreement
Imports BESHStatNG.ExcelCommon

''' <summary>Inert original Windows option/dispatch derivative. No estimator or native object.</summary>
Public NotInheritable Class Agreement26Legacy
    Public Class Deming
        Private ReadOnly opts As DemingOptions
        Public Sub New(parsed As DemingOptions, percentile As Boolean, bca As Boolean, closed As Boolean, linnet As Boolean)
            opts = parsed
            If percentile Then
                opts.CiMethod = AgreementCiMethod.BootstrapPercentile
            ElseIf bca Then
                opts.CiMethod = AgreementCiMethod.BootstrapBCa
            ElseIf closed OrElse linnet Then
                opts.CiMethod = AgreementCiMethod.Analytical
            Else
                opts.CiMethod = AgreementCiMethod.Jackknife
            End If
            If opts.CiMethod = AgreementCiMethod.Analytical AndAlso linnet Then
                Execution = DemingExecutionIntent.AnalyticalMcr
            ElseIf opts.CiMethod = AgreementCiMethod.Analytical Then
                Execution = DemingExecutionIntent.AnalyticalClosedForm
            ElseIf opts.CiMethod = AgreementCiMethod.Jackknife Then
                Execution = DemingExecutionIntent.Jackknife
            Else
                Execution = DemingExecutionIntent.Bootstrap
            End If
        End Sub
        Public ReadOnly Property Execution As DemingExecutionIntent
        Public Function CreateOptions() As DemingOptions
            Return opts
        End Function
    End Class
    Public Class Lin
        Private ReadOnly opts As LinConcordanceOptions
        Public Sub New(parsed As LinConcordanceOptions, analytical As Boolean, percentile As Boolean, bca As Boolean)
            opts = parsed
            If analytical Then
                opts.CiMethod = AgreementCiMethod.Analytical
            ElseIf percentile Then
                opts.CiMethod = AgreementCiMethod.BootstrapPercentile
            ElseIf bca Then
                opts.CiMethod = AgreementCiMethod.BootstrapBCa
            Else
                opts.CiMethod = AgreementCiMethod.Analytical
            End If
        End Sub
        Public Function CreateOptions() As LinConcordanceOptions
            Return opts
        End Function
    End Class
    Public Class Kappa
        Private ReadOnly opts As KappaOptions
        Public Sub New(parsed As KappaOptions, weighting As Integer, analytical As Boolean, jackknife As Boolean, percentile As Boolean, bca As Boolean)
            opts = parsed
            Select Case weighting
                Case 0 : opts.Weighting = KappaWeightingScheme.Unweighted
                Case 1 : opts.Weighting = KappaWeightingScheme.Linear
                Case 2 : opts.Weighting = KappaWeightingScheme.Quadratic
                Case 3 : opts.Weighting = KappaWeightingScheme.CicchettiAllison
                Case 4 : opts.Weighting = KappaWeightingScheme.FleissCohen
                Case Else : opts.Weighting = KappaWeightingScheme.Unweighted
            End Select
            If analytical Then
                opts.CiMethod = AgreementCiMethod.Analytical
            ElseIf jackknife Then
                opts.CiMethod = AgreementCiMethod.Jackknife
            ElseIf percentile Then
                opts.CiMethod = AgreementCiMethod.BootstrapPercentile
            ElseIf bca Then
                opts.CiMethod = AgreementCiMethod.BootstrapBCa
            Else
                opts.CiMethod = AgreementCiMethod.Analytical
            End If
        End Sub
        Public Function CreateOptions() As KappaOptions
            Return opts
        End Function
    End Class
    Public Class Bland
        Private ReadOnly opts As BlandAltmanOptions
        Public Sub New(parsed As BlandAltmanOptions, xAxis As Integer, plot As Integer)
            opts = parsed
            Select Case xAxis
                Case 1 : opts.XAxisMode = BlandAltmanXAxisMode.ReferenceMethod
                Case 2 : opts.XAxisMode = BlandAltmanXAxisMode.TestMethod
                Case Else : opts.XAxisMode = BlandAltmanXAxisMode.MeanOfMethods
            End Select
            Select Case plot
                Case 1 : opts.PlotMode = RepeatedBlandAltmanPlotMode.SubjectMeansOnly
                Case 2 : opts.PlotMode = RepeatedBlandAltmanPlotMode.AllObservationsAndSubjectMeans
                Case Else : opts.PlotMode = RepeatedBlandAltmanPlotMode.AllObservations
            End Select
            If opts.BootstrapReplicates <= 0 Then opts.BootstrapReplicates = 2000
        End Sub
        Public Function CreateOptions() As BlandAltmanOptions
            Return opts
        End Function
    End Class
    Public Class Icc
        Public Sub New(a As Boolean, b As Boolean, c As Boolean, d As Boolean, e As Boolean, f As Boolean, alpha As Double, repeatability As Boolean)
            Dim flags = {a, b, c, d, e, f}, index = Array.FindIndex(flags, Function(v) v)
            Choice = CType(index, IccEstimatorChoice) : Me.Alpha = alpha : Me.Repeatability = repeatability
        End Sub
        Public ReadOnly Property Choice As IccEstimatorChoice
        Public ReadOnly Property Alpha As Double
        Public ReadOnly Property Repeatability As Boolean
        Public Function Label(k As Integer) As String
            Select Case Choice
                Case IccEstimatorChoice.OneOne : Return "ICC(1,1)"
                Case IccEstimatorChoice.OneAverage : Return "ICC(1,k)"
                Case IccEstimatorChoice.TwoOne : Return "ICC(2,1)"
                Case IccEstimatorChoice.TwoAverage : Return $"ICC(2,{k})"
                Case IccEstimatorChoice.ThreeOne : Return "ICC(3,1)"
                Case IccEstimatorChoice.ThreeAverage : Return $"ICC(3,{k})"
                Case Else : Return String.Empty
            End Select
        End Function
    End Class
End Class
