Option Explicit On
Option Strict On
Imports System
Imports System.Collections.Generic
Imports BESHStatNG.regression

Namespace ExcelCommon
    ''' <summary>Lossless covariance selection. Metadata uses the existing validation-only
    ''' Core projections; construction deliberately calls the original factory exactly once,
    ''' preserving its logging, exceptions and aliases. No covariance matrix is retained.</summary>
    Public NotInheritable Class MixedModelCovarianceSelection
        Private ReadOnly _residual As MixedModelRStruct
        Private ReadOnly _random As MixedModelGStruct
        Friend Sub New(token As String, randomSide As Boolean)
            RequestedToken = token
            IsRandomSide = randomSide
            If randomSide Then
                _random = MixedModelOptionPolicy.RandomForValidation(token)
                IsSupported = _random IsNot Nothing
                If IsSupported Then
                    EffectiveCoreType = _random.GetType().Name
                    EffectiveName = _random.ToString()
                    IsZeroRandom = _random.IsDegenerateZeroG()
                End If
            Else
                _residual = MixedModelOptionPolicy.ResidualForValidation(token)
                IsSupported = _residual IsNot Nothing
                If IsSupported Then
                    EffectiveCoreType = _residual.GetType().Name
                    EffectiveName = _residual.ToString()
                    UsesVisitIndex = _residual.UsesVisitIndex()
                End If
            End If
        End Sub
        Public ReadOnly Property RequestedToken As String
        Public ReadOnly Property IsRandomSide As Boolean
        Public ReadOnly Property IsSupported As Boolean
        Public ReadOnly Property EffectiveCoreType As String
        Public ReadOnly Property EffectiveName As String
        Public ReadOnly Property UsesVisitIndex As Boolean
        Public ReadOnly Property IsZeroRandom As Boolean

        Public Function CreateResidualStructure() As MixedModelRStruct
            If IsRandomSide Then Throw New InvalidOperationException("R-side selection required.")
            Return MixedModelFittingOptions.CreateResidualStructure(RequestedToken)
        End Function
        Public Function CreateRandomStructure() As MixedModelGStruct
            If Not IsRandomSide Then Throw New InvalidOperationException("G-side selection required.")
            Return MixedModelFittingOptions.CreateRandomStructure(RequestedToken)
        End Function

        ''' <summary>Delegate shape/parameter metadata to Core, including its Q restrictions.
        ''' Nothing means unsupported selection/dimension, not a zero-parameter covariance.
        ''' Does not allocate a covariance matrix or parameter vector.</summary>
        Public Function ParameterCount(data As MixedModelBlockData) As Nullable(Of Integer)
            If Not IsSupported OrElse data Is Nothing Then Return Nothing
            If IsRandomSide Then Return RandomParameterCount(data.Q)
            Try
                Return _residual.ParamCount(data)
            Catch ex As OverflowException
                Return Nothing
            Catch ex As ApplicationException
                Return Nothing
            Catch ex As ArgumentException
                Return Nothing
            End Try
        End Function
        Public Function RandomParameterCount(q As Integer) As Nullable(Of Integer)
            If Not IsRandomSide OrElse Not IsSupported OrElse q < 0 Then Return Nothing
            Try
                Return _random.ParamCount(q)
            Catch ex As OverflowException
                Return Nothing
            Catch ex As ApplicationException
                Return Nothing
            Catch ex As ArgumentException
                Return Nothing
            End Try
        End Function
    End Class

    ''' <summary>Portable covariance and fitting specialization for the GUI workflow.
    ''' Keeps requested strings and effective Core controls distinct. Core remains responsible
    ''' for runtime AI/gradient fallback, numerical fitting and convergence. This selection
    ''' never fits, invokes callbacks, promotes random designs or rejects an acquired request.</summary>
    Public NotInheritable Class MixedModelFittingOptions
        Private ReadOnly _requested As MixedModelWorkflowOptions
        Private ReadOnly _optimizerFallback, _gradientFallback As Boolean
        Public Sub New(kind As MixedModelWorkflowKind, options As MixedModelWorkflowOptions)
            Me.Kind = kind
            _requested = If(options, MixedModelWorkflowOptions.Defaults(kind)).Copy()
            Residual = SelectResidual(_requested.ResidualStructure)
            ' MMRM has no G design. The unused requested token remains in RequestedOptions.
            Random = SelectRandom(If(kind = MixedModelWorkflowKind.MMRM, "None", _requested.RandomStructure))
            Decision = MixedModelWorkflowPolicy.DecideFit(kind, _requested.FitMethod, _requested.Inference)
            Dim control As New MixedModelControl
            ProjectOptimizer(kind, control, _requested.Optimizer, _requested.Gradient)
            OptimizerMode = control.CovarianceOptimizerMode
            GradientMode = control.CovarianceGradientMode
            Dim text = If(String.IsNullOrWhiteSpace(_requested.Optimizer), "AI/Fisher scoring (default)", _requested.Optimizer)
            _optimizerFallback = MixedModelOptionPolicy.ParseCovarianceOptimizerMode(text, CType(-1, MixedModelCovarianceOptimizerMode)) = CType(-1, MixedModelCovarianceOptimizerMode) AndAlso
                Not String.Equals(text, "Projected BFGS (finite-difference gradient)", StringComparison.OrdinalIgnoreCase)
            Dim gradientToken = MixedModelOptionPolicy.NormalizeToken(_requested.Gradient)
            _gradientFallback = gradientToken <> String.Empty AndAlso gradientToken <> "AUTO" AndAlso gradientToken <> "AUTOANALYTICWHEREAVAILABLE" AndAlso
                MixedModelOptionPolicy.ParseCovarianceGradientMode(_requested.Gradient, CType(-1, MixedModelCovarianceGradientMode)) = CType(-1, MixedModelCovarianceGradientMode)
        End Sub
        Public ReadOnly Property Kind As MixedModelWorkflowKind
        Public ReadOnly Property Residual As MixedModelCovarianceSelection
        Public ReadOnly Property Random As MixedModelCovarianceSelection
        Public ReadOnly Property Decision As MixedModelFitDecision
        Public ReadOnly Property OptimizerMode As MixedModelCovarianceOptimizerMode
        Public ReadOnly Property GradientMode As MixedModelCovarianceGradientMode
        Public ReadOnly Property RequestedOptions As MixedModelWorkflowOptions
            Get
                Return _requested.Copy()
            End Get
        End Property
        Public Shared Function SelectResidual(token As String) As MixedModelCovarianceSelection
            Return New MixedModelCovarianceSelection(token, False)
        End Function
        Public Shared Function SelectRandom(token As String) As MixedModelCovarianceSelection
            Return New MixedModelCovarianceSelection(token, True)
        End Function
        Public Shared Function CreateResidualStructure(token As String) As MixedModelRStruct
            Return MixedModelRStructUtils.createMixedModelRStruct(token)
        End Function
        Public Shared Function CreateRandomStructure(token As String) As MixedModelGStruct
            Return MixedModelGStructUtils.createMixedModelGStruct(token)
        End Function
        Public Shared Function ResidualChoices() As IReadOnlyList(Of String)
            Return Array.AsReadOnly(DirectCast(MixedModelRStruct.RStructsList.Clone(), String()))
        End Function
        Public Shared Function RandomChoices() As IReadOnlyList(Of String)
            Return Array.AsReadOnly(DirectCast(MixedModelGStruct.GStructsList.Clone(), String()))
        End Function

        ''' <summary>Modify only the original GUI-owned control fields. Other Core defaults,
        ''' restart/scaling settings and callbacks stay with their existing owners.</summary>
        Public Sub ApplyControl(ByRef control As MixedModelControl)
            control.MaxIter = _requested.MaximumIterations
            control.Epsilon = _requested.Epsilon
            control.StepTolerance = _requested.Epsilon
            control.FunctionTolerance = _requested.Epsilon
            control.CovarianceOptimizerMode = OptimizerMode
            control.CovarianceGradientMode = GradientMode
            control.Trace = _requested.Trace OrElse _requested.IterationDetails
        End Sub

        ' The 04A.14 public ApplyOptimizer seam delegates here. Preserve its exact
        ' model-specific unknown fallback and exact-label gradient overrides.
        Friend Shared Sub ProjectOptimizer(kind As MixedModelWorkflowKind, ByRef control As MixedModelControl, optimizer As String, gradient As String)
            Dim text = If(String.IsNullOrWhiteSpace(optimizer), "AI/Fisher scoring (default)", optimizer)
            control.CovarianceOptimizerMode = MixedModelOptionPolicy.ParseCovarianceOptimizerMode(text,
                If(kind = MixedModelWorkflowKind.MMRM, MixedModelCovarianceOptimizerMode.ProjectedBfgs, MixedModelCovarianceOptimizerMode.AverageInformationReml))
            control.CovarianceGradientMode = MixedModelOptionPolicy.ParseCovarianceGradientMode(gradient, MixedModelCovarianceGradientMode.Auto)
            If String.Equals(text, "Projected BFGS (analytic gradient)", StringComparison.OrdinalIgnoreCase) Then
                control.CovarianceOptimizerMode = MixedModelCovarianceOptimizerMode.ProjectedBfgsAnalyticGradient
                control.CovarianceGradientMode = MixedModelCovarianceGradientMode.AnalyticScore
            ElseIf String.Equals(text, "Projected BFGS (finite-difference gradient)", StringComparison.OrdinalIgnoreCase) Then
                control.CovarianceOptimizerMode = MixedModelCovarianceOptimizerMode.ProjectedBfgs
                control.CovarianceGradientMode = MixedModelCovarianceGradientMode.NumericalFiniteDifference
            End If
        End Sub

        ''' <summary>Additional portable diagnostics only. Existing workflow validation owns
        ''' roles, authoring, finite inputs and missing visit-role diagnostics. Acquired Windows
        ''' construction intentionally does not enforce either diagnostic set.</summary>
        Public Function DesignDiagnostics(data As MixedModelBlockData) As IReadOnlyList(Of RegressionDiagnostic)
            Dim issues As New List(Of RegressionDiagnostic)
            If _optimizerFallback Then MixedModelWorkflowPolicy.Issue(issues, "optimizer.fallback", "Unknown optimizer retains the model-specific GUI fallback.", False)
            If _gradientFallback Then MixedModelWorkflowPolicy.Issue(issues, "gradient.fallback", "Unknown gradient retains Auto selection.", False)
            If Not String.Equals(_requested.FitMethod, "ML", StringComparison.OrdinalIgnoreCase) AndAlso Not String.Equals(_requested.FitMethod, "REML", StringComparison.OrdinalIgnoreCase) Then
                MixedModelWorkflowPolicy.Issue(issues, "fit.fallback", "Unknown GUI fit method retains REML.", False)
            End If
            If data IsNot Nothing Then
                If Residual.IsSupported AndAlso Not Residual.ParameterCount(data).HasValue Then MixedModelWorkflowPolicy.Issue(issues, "residual.dimension", "Core cannot describe R parameters for this design.")
                If Kind = MixedModelWorkflowKind.LMM AndAlso Random.IsSupported AndAlso Not Random.IsZeroRandom AndAlso Not Random.ParameterCount(data).HasValue Then
                    MixedModelWorkflowPolicy.Issue(issues, "random.dimension", "Core G structure does not support the expanded Z column count.")
                End If
            End If
            Return issues.AsReadOnly()
        End Function
    End Class
End Namespace
