Option Explicit On
Option Strict On
Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports BESHStatNG.regression

Namespace ExcelCommon
    Public Enum MixedModelFitState
        Unavailable
        Converged
        Nonconverged
        Cancelled
        Interrupted
        Failed
    End Enum

    ''' <summary>Detached scalar status; availability flags describe supplied Core state,
    ''' not independent scientific qualification. No matrix/workspace/fit is retained.</summary>
    Public NotInheritable Class MixedModelDiagnosticSnapshot
        Friend Sub New(result As MixedModelResult, alpha As Double)
            ' Metadata projection of the existing internal Core alpha fallback;
            ' actual interval/probability calculations still normalize inside Core.
            RequestedAlpha = alpha : EffectiveAlpha = If(Double.IsNaN(alpha) OrElse Double.IsInfinity(alpha) OrElse alpha <= 0 OrElse alpha >= 1, 0.05, alpha)
            Dim warnings As New List(Of AnalysisWarning)
            If result Is Nothing Then
                State = MixedModelFitState.Unavailable
            Else
                State = If(result.Cancelled, MixedModelFitState.Cancelled, If(result.Interrupted, MixedModelFitState.Interrupted,
                    If(result.Converged, MixedModelFitState.Converged, If(result.Beta Is Nothing OrElse result.Beta.Length = 0, MixedModelFitState.Failed, MixedModelFitState.Nonconverged))))
                FitMethod = result.FitMethod : Inference = result.FixedInferenceMethod
                Iterations = result.Iterations : MaximumIterations = result.ControlMaxIter
                Optimizer = result.ControlCovarianceOptimizerMode : Gradient = result.ControlCovarianceGradientMode
                GradientNorm = result.GradNorm : Message = result.Message
                HasKrWorkspace = result.KenwardRogerWorkspace IsNot Nothing
                HasAdjustedCovariance = result.KenwardRogerAdjustedVarBeta IsNot Nothing
                HasSatterthwaiteWorkspace = result.InferenceWorkspace IsNot Nothing
                HasOptimizerTrace = result.OptimizerTrace IsNot Nothing
                HasSupportDiagnostics = result.VisitSupportDiagnostics IsNot Nothing AndAlso result.VisitSupportDiagnostics.Enabled
                HasRestartDiagnostics = result.RestartDiagnostics IsNot Nothing AndAlso result.RestartDiagnostics.Enabled
                HasKrFiniteDifferenceDiagnostics = HasKrWorkspace AndAlso result.KenwardRogerWorkspace.FiniteDifferenceDiagnostics IsNot Nothing
                KrStatusMessage = result.KenwardRogerStatusMessage
                If result.PerformanceDiagnostics IsNot Nothing Then
                    ActualOptimizer = result.PerformanceDiagnostics.ActualCovarianceOptimizerName
                    ActualGradientProvider = result.PerformanceDiagnostics.ActualCovarianceGradientProviderName
                    AnalyticGradientFallback = result.PerformanceDiagnostics.AnalyticGradientFallbackUsed
                End If
                If State <> MixedModelFitState.Converged Then warnings.Add(New AnalysisWarning("mixed.fit." & State.ToString().ToLowerInvariant(), "Core fit status: " & State.ToString() & "."))
                CoefficientsAvailable = result.Beta IsNot Nothing AndAlso result.Beta.Length > 0
                OrdinaryCovarianceAvailable = result.VarBeta IsNot Nothing
                If Not CoefficientsAvailable Then warnings.Add(New AnalysisWarning("mixed.fit.coefficients.unavailable", "Core coefficients are unavailable."))
                If result.Beta IsNot Nothing AndAlso result.Beta.Any(Function(v) Double.IsNaN(v) OrElse Double.IsInfinity(v)) Then warnings.Add(New AnalysisWarning("mixed.fit.coefficients.nonfinite", "Core coefficients contain unavailable or nonfinite values."))
                If result.VarBeta Is Nothing Then
                    warnings.Add(New AnalysisWarning("mixed.fit.covariance.unavailable", "Ordinary fitted coefficient covariance is unavailable."))
                ElseIf result.VarBeta.Cast(Of Double)().Any(Function(v) Double.IsNaN(v) OrElse Double.IsInfinity(v)) Then
                    warnings.Add(New AnalysisWarning("mixed.fit.covariance.nonfinite", "Core coefficient covariance contains nonfinite values."))
                Else
                    For i = 0 To Math.Min(result.VarBeta.GetLength(0), result.VarBeta.GetLength(1)) - 1
                        If result.VarBeta(i, i) < 0 Then
                            warnings.Add(New AnalysisWarning("mixed.fit.covariance.negative", "Core coefficient covariance has a negative diagonal value.")) : Exit For
                        End If
                    Next
                End If
                If result.UserWarnings IsNot Nothing Then
                    For Each warningText As String In result.UserWarnings
                        If warnings.Count >= MixedModelWorkflowPolicy.MaximumDiagnostics Then Exit For
                        If Not String.IsNullOrWhiteSpace(warningText) Then warnings.Add(New AnalysisWarning("mixed.fit.warning", warningText))
                    Next
                End If
            End If
            If Not alpha.Equals(EffectiveAlpha) AndAlso warnings.Count < MixedModelWorkflowPolicy.MaximumDiagnostics Then warnings.Add(New AnalysisWarning(AnalysisWarningCodes.AlphaDefaulted, "Core uses its existing alpha fallback."))
            Me.Warnings = warnings.AsReadOnly()
        End Sub
        Public ReadOnly Property State As MixedModelFitState
        Public ReadOnly Property RequestedAlpha As Double
        Public ReadOnly Property EffectiveAlpha As Double
        Public ReadOnly Property FitMethod As Nullable(Of MixedModelFitMethod)
        Public ReadOnly Property Inference As Nullable(Of MixedModelFixedInferenceMethod)
        Public ReadOnly Property Iterations As Integer
        Public ReadOnly Property MaximumIterations As Integer
        Public ReadOnly Property Optimizer As Nullable(Of MixedModelCovarianceOptimizerMode)
        Public ReadOnly Property Gradient As Nullable(Of MixedModelCovarianceGradientMode)
        Public ReadOnly Property GradientNorm As Double
        Public ReadOnly Property Message As String
        Public ReadOnly Property KrStatusMessage As String
        Public ReadOnly Property ActualOptimizer As String
        Public ReadOnly Property ActualGradientProvider As String
        Public ReadOnly Property AnalyticGradientFallback As Boolean
        Public ReadOnly Property HasKrWorkspace As Boolean
        Public ReadOnly Property CoefficientsAvailable As Boolean
        Public ReadOnly Property OrdinaryCovarianceAvailable As Boolean
        Public ReadOnly Property HasAdjustedCovariance As Boolean
        Public ReadOnly Property HasSatterthwaiteWorkspace As Boolean
        Public ReadOnly Property HasOptimizerTrace As Boolean
        Public ReadOnly Property HasSupportDiagnostics As Boolean
        Public ReadOnly Property HasRestartDiagnostics As Boolean
        Public ReadOnly Property HasKrFiniteDifferenceDiagnostics As Boolean
        Public ReadOnly Property Warnings As IReadOnlyList(Of AnalysisWarning)
    End Class

    Public NotInheritable Class MixedModelReportSection
        Friend Sub New(index As Integer, label As String, code As String)
            Me.Index = index : Me.Label = label : Me.Code = code
        End Sub
        Public ReadOnly Property Index As Integer
        Public ReadOnly Property Label As String
        Public ReadOnly Property Code As String
    End Class

    Public NotInheritable Class MixedModelDiagnosticPolicy
        Private Sub New()
        End Sub
        Public Shared Function Capture(result As MixedModelResult, alpha As Double) As MixedModelDiagnosticSnapshot
            Return New MixedModelDiagnosticSnapshot(result, alpha)
        End Function
        Public Shared Function DescribeTables(document As AnalysisResultDocument) As IReadOnlyList(Of MixedModelReportSection)
            Dim sections As New List(Of MixedModelReportSection)
            Dim known As New Dictionary(Of String, String)(StringComparer.Ordinal) From {
                {"Fixed effects", "fixed-effects"}, {"Kenward-Roger term-level F tests", "kr-term-tests"}, {"Covariance parameters", "covariance-parameters"},
                {"Fit statistics", "fit-statistics"}, {"Performance diagnostics", "performance"}, {"MMRM support diagnostics", "support"},
                {"Optimizer restart diagnostics", "restart"}, {"KR finite-difference diagnostics", "kr-finite-difference"},
                {"Convergence", "convergence"}, {"Optimizer trace", "optimizer-trace"}, {"BLUPs / random effects", "random-effects"},
                {"Class Level Information", "class-information"}, {"Class level information", "class-information"},
                {"Reference-grid estimated marginal means", "reference-means"}, {"Reference-grid group contrasts", "reference-group-contrasts"}}
            For Each artifact In document.Tables
                Dim model = artifact.Model, label = If(model.RowCount > 0 AndAlso model.ColumnCount > 0, TryCast(model.Values(0, 0), String), Nothing)
                Dim code As String = Nothing
                If label Is Nothing OrElse Not known.TryGetValue(label, code) Then
                    code = "core-or-supplemental"
                    If label IsNot Nothing Then
                        If label = "Estimated marginal means by visit" Then code = "means-by-visit"
                        If label.StartsWith("Estimated marginal means by visit and ", StringComparison.Ordinal) Then code = "means-by-group"
                        If label = "MMRM change from baseline by visit" Then code = "change-by-visit"
                        If label.StartsWith("MMRM change from baseline by visit and ", StringComparison.Ordinal) Then code = "change-by-group"
                        If label = "MMRM pairwise group differences by visit" OrElse label = "MMRM selected group difference by visit" OrElse label = "MMRM group differences vs control by visit" Then code = "group-differences"
                        If label.StartsWith("MMRM difference in change from baseline by ", StringComparison.Ordinal) OrElse label.StartsWith("MMRM selected difference in change from baseline by ", StringComparison.Ordinal) Then code = "difference-in-change"
                    End If
                End If
                sections.Add(New MixedModelReportSection(artifact.Index, If(label, String.Empty), "mixed.section." & code))
            Next
            Return sections.AsReadOnly()
        End Function
    End Class
End Namespace
