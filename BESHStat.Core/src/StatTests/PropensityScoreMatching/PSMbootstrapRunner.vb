Option Explicit On
Option Strict On
Option Infer On

Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports BESHStatNG.Resampling

Namespace CausalInference

    ''' <summary>
    ''' Simple non-parametric bootstrap wrapper for backend effect estimates.
    ''' The bootstrap refits the score model in each replicate when options.ScoreMethod is logistic regression.
    ''' </summary>
    Public NotInheritable Class PsmBootstrapRunner
        Private Sub New()
        End Sub

        Public Shared Function BootstrapEffect(input As PsmInputData, options As PsmOptions, target As PsmBootstrapTarget, Optional replications As Integer = 200, Optional seed As Integer = 12345) As PsmBootstrapResult
            If input Is Nothing Then Throw New ArgumentNullException("input")
            If options Is Nothing Then options = New PsmOptions()
            If replications < 2 Then Throw New ArgumentOutOfRangeException("replications", "At least two bootstrap replications are required.")

            Dim original As Double = EstimateTarget(input, options, target)
            Dim rng As New Random(seed)
            Dim estimates As New List(Of Double)()
            Dim warnings As New List(Of String)()

            Dim replicateNumber As Integer = 0
            For Each indices As Integer() In ResamplingBootstrap.BootstrapIndices(input.RowCount, replications, rng)
                replicateNumber += 1
                Try
                    Dim bootInput As PsmInputData = ResampleInput(input, indices)
                    Dim estimate As Double = EstimateTarget(bootInput, options, target)
                    If AppInfrastructure.IsFinite(estimate) Then estimates.Add(estimate)
                Catch ex As Exception
                    If warnings.Count < 10 Then warnings.Add("Bootstrap replicate " & replicateNumber.ToString() & " failed: " & ex.Message)
                End Try
            Next

            Dim arr As Double() = estimates.ToArray()
            Dim result As New PsmBootstrapResult With {
                .Target = target,
                .ReplicationsRequested = replications,
                .ReplicationsUsed = arr.Length,
                .EstimateOriginal = original,
                .ReplicateEstimates = arr,
                .Warnings = warnings
            }

            If arr.Length > 0 Then
                result.BootstrapMean = arr.Average()
                result.BootstrapStandardError = If(arr.Length > 1, Math.Sqrt(PsmMath.Variance(arr, True)), Double.NaN)
                result.PercentileLower95 = PsmMath.Quantile(arr, 0.025)
                result.PercentileUpper95 = PsmMath.Quantile(arr, 0.975)
            Else
                result.BootstrapMean = Double.NaN
                result.BootstrapStandardError = Double.NaN
                result.PercentileLower95 = Double.NaN
                result.PercentileUpper95 = Double.NaN
                result.Warnings.Add("No valid bootstrap estimates were produced.")
            End If
            Return result
        End Function

        Private Shared Function EstimateTarget(input As PsmInputData, options As PsmOptions, target As PsmBootstrapTarget) As Double
            Dim fit As PsmResult = PsmBackend.Fit(input, options)
            Select Case target
                Case PsmBootstrapTarget.WeightedEffect
                    Return If(fit.WeightedEffect Is Nothing, Double.NaN, fit.WeightedEffect.Estimate)
                Case PsmBootstrapTarget.MatchedEffect
                    Return If(fit.MatchedEffect Is Nothing, Double.NaN, fit.MatchedEffect.Estimate)
                Case PsmBootstrapTarget.SubclassificationEffect
                    Return If(fit.SubclassificationEffect Is Nothing, Double.NaN, fit.SubclassificationEffect.Estimate)
                Case PsmBootstrapTarget.DoublyRobustAipw
                    Dim dr As PsmDoublyRobustResult = PsmDoublyRobustEstimator.EstimateAipw(input, fit.ScoreModel.Scores, options)
                    Return If(dr Is Nothing OrElse dr.Effect Is Nothing, Double.NaN, dr.Effect.Estimate)
                Case Else
                    Return Double.NaN
            End Select
        End Function

        Private Shared Function ResampleInput(input As PsmInputData, indices As Integer()) As PsmInputData
            If indices Is Nothing Then Throw New ArgumentNullException("indices")
            Dim n As Integer = indices.Length
            Dim p As Integer = input.CovariateCount
            Dim ids(n - 1) As String
            Dim treatment(n - 1) As Double
            Dim outcome As Double() = Nothing
            If input.Outcome IsNot Nothing Then outcome = New Double(n - 1) {}
            Dim covariates(n - 1, p - 1) As Double
            Dim scores As Double() = Nothing
            If input.SuppliedPropensityScores IsNot Nothing Then scores = New Double(n - 1) {}
            Dim groups As String() = Nothing
            If input.ExactGroupLabels IsNot Nothing Then groups = New String(n - 1) {}

            For i As Integer = 0 To n - 1
                Dim src As Integer = indices(i)
                ids(i) = input.GetId(src) & ":b" & (i + 1).ToString()
                treatment(i) = input.Treatment(src)
                If outcome IsNot Nothing Then outcome(i) = input.Outcome(src)
                For j As Integer = 0 To p - 1
                    covariates(i, j) = input.Covariates(src, j)
                Next
                If scores IsNot Nothing Then scores(i) = input.SuppliedPropensityScores(src)
                If groups IsNot Nothing Then groups(i) = If(input.ExactGroupLabels(src), "")
            Next

            Return New PsmInputData With {
                .Ids = ids,
                .Treatment = treatment,
                .Outcome = outcome,
                .Covariates = covariates,
                .CovariateNames = input.CovariateNames,
                .SuppliedPropensityScores = scores,
                .ExactGroupLabels = groups
            }
        End Function
    End Class

End Namespace
