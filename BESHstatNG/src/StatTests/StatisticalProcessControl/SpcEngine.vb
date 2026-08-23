Option Explicit On
Option Strict On
Option Infer On

Imports System
Imports System.Collections.Generic
Imports System.Diagnostics
Imports System.Reflection

Namespace StatisticalProcessControl

    ''' <summary>
    ''' Contract implemented by host-neutral SPC chart calculators.
    ''' </summary>
    ''' <remarks>
    ''' Implementations should contain calculations only. Request validation,
    ''' cancellation checks, timing, and result construction are coordinated by
    ''' <see cref="SpcEngine"/>.
    ''' </remarks>
    Public Interface ISpcChartCalculator
        Function CanCalculate(chartType As SpcChartType) As Boolean

        Function Calculate(request As SpcFitRequest,
                           cancellationRequested As Func(Of Boolean)) As SpcCalculationResult
    End Interface

    ''' <summary>
    ''' Calculation payload returned by an <see cref="ISpcChartCalculator"/>.
    ''' </summary>
    Public NotInheritable Class SpcCalculationResult
        Private ReadOnly _panels As SpcPanelResult()
        Private ReadOnly _warnings As String()

        Public Sub New(panels As SpcPanelResult(),
                       Optional warnings As String() = Nothing)
            If panels Is Nothing OrElse panels.Length = 0 Then
                Throw New ArgumentException(
                    "A chart calculator must return at least one panel.", NameOf(panels))
            End If

            _panels = CType(panels.Clone(), SpcPanelResult())
            For i As Integer = 0 To _panels.Length - 1
                If _panels(i) Is Nothing Then
                    Throw New ArgumentException(
                        "Calculated panels must not contain null entries.", NameOf(panels))
                End If
            Next

            If warnings Is Nothing Then
                _warnings = Array.Empty(Of String)()
            Else
                _warnings = CType(warnings.Clone(), String())
                For i As Integer = 0 To _warnings.Length - 1
                    _warnings(i) = If(_warnings(i), String.Empty).Trim()
                Next
            End If
        End Sub

        Public ReadOnly Property Panels As SpcPanelResult()
            Get
                Return CType(_panels.Clone(), SpcPanelResult())
            End Get
        End Property

        Public ReadOnly Property Warnings As String()
            Get
                Return CType(_warnings.Clone(), String())
            End Get
        End Property
    End Class

    ''' <summary>
    ''' Validates and dispatches host-neutral statistical-process-control fits.
    ''' </summary>
    Public NotInheritable Class SpcEngine
        Private Shared ReadOnly _defaultEngine As New Lazy(Of SpcEngine)(
            AddressOf CreateDefaultEngine, True)

        Private ReadOnly _calculators As ISpcChartCalculator()

        ''' <summary>
        ''' Creates an engine with an explicit calculator collection.
        ''' </summary>
        ''' <remarks>
        ''' This constructor is useful for deterministic unit tests. Normal callers
        ''' should use the shared <see cref="Fit"/> method, which discovers bundled
        ''' calculators in the BESHStatNG assembly once and caches the registry.
        ''' </remarks>
        Public Sub New(calculators As IEnumerable(Of ISpcChartCalculator))
            If calculators Is Nothing Then Throw New ArgumentNullException(NameOf(calculators))

            Dim values As New List(Of ISpcChartCalculator)()
            For Each calculator As ISpcChartCalculator In calculators
                If calculator Is Nothing Then
                    Throw New ArgumentException(
                        "The calculator collection must not contain null entries.",
                        NameOf(calculators))
                End If
                values.Add(calculator)
            Next
            _calculators = values.ToArray()
        End Sub

        ''' <summary>Fits a chart using the bundled calculator registry.</summary>
        Public Shared Function Fit(request As SpcFitRequest,
                                   Optional cancellationRequested As Func(Of Boolean) = Nothing) As SpcFitResult
            Return _defaultEngine.Value.FitCore(request, cancellationRequested)
        End Function

        ''' <summary>Fits a chart using this engine's calculator registry.</summary>
        Public Function FitWithRegisteredCalculators(
            request As SpcFitRequest,
            Optional cancellationRequested As Func(Of Boolean) = Nothing) As SpcFitResult

            Return FitCore(request, cancellationRequested)
        End Function

        ''' <summary>
        ''' Validates a request without performing a calculation.
        ''' </summary>
        Public Shared Sub Validate(request As SpcFitRequest)
            If request Is Nothing Then Throw New ArgumentNullException(NameOf(request))

            ValidateChartAndLayout(request)
            ValidateOptions(request)
            ValidateStages(request)
            ValidateExclusions(request)
            ValidateHistoricalParameters(request)
            ValidateDataValues(request)
        End Sub

        ''' <summary>Returns whether a bundled calculator supports a chart type.</summary>
        Public Shared Function IsImplemented(chartType As SpcChartType) As Boolean
            ValidateEnum(chartType, NameOf(chartType))
            Return _defaultEngine.Value.FindCalculator(chartType) IsNot Nothing
        End Function

        ''' <summary>Returns chart types supported by the bundled calculator registry.</summary>
        Public Shared Function GetImplementedChartTypes() As SpcChartType()
            Dim values As New List(Of SpcChartType)()
            For Each item As SpcChartType In [Enum].GetValues(GetType(SpcChartType))
                If _defaultEngine.Value.FindCalculator(item) IsNot Nothing Then values.Add(item)
            Next
            Return values.ToArray()
        End Function

        ''' <summary>Maps a chart type to its broad statistical family.</summary>
        Public Shared Function GetChartFamily(chartType As SpcChartType) As SpcChartFamily
            ValidateEnum(chartType, NameOf(chartType))

            Dim numericValue As Integer = CInt(chartType)
            If chartType = SpcChartType.RunChart Then Return SpcChartFamily.Run
            If numericValue >= 100 AndAlso numericValue < 200 Then Return SpcChartFamily.ShewhartVariables
            If numericValue >= 200 AndAlso numericValue < 220 Then Return SpcChartFamily.ShewhartAttributes
            If numericValue >= 220 AndAlso numericValue < 300 Then Return SpcChartFamily.RareEvent
            If numericValue >= 300 AndAlso numericValue < 400 Then Return SpcChartFamily.TimeWeighted
            If numericValue >= 400 AndAlso numericValue < 500 Then Return SpcChartFamily.Multivariate
            Return SpcChartFamily.Specialized
        End Function

        Private Function FitCore(request As SpcFitRequest,
                                 cancellationRequested As Func(Of Boolean)) As SpcFitResult
            Validate(request)
            ThrowIfCancellationRequested(cancellationRequested)

            Dim calculator As ISpcChartCalculator = FindCalculator(request.ChartType)
            If calculator Is Nothing Then
                Throw New NotSupportedException(
                    "The " & request.ChartType.ToString() &
                    " chart is part of the SPC public model but does not yet have a registered calculator.")
            End If

            Dim startedUtc As DateTime = DateTime.UtcNow
            Dim timer As Stopwatch = Stopwatch.StartNew()
            Dim calculated As SpcCalculationResult =
                calculator.Calculate(request, NormalizeCancellation(cancellationRequested))

            If calculated Is Nothing Then
                Throw New InvalidOperationException(
                    "The SPC chart calculator returned no calculation result.")
            End If
            calculated = SpcRuleEvaluator.EvaluateCalculation(calculated,
                                                              request.AnalysisOptions.Rules,
                                                              NormalizeCancellation(cancellationRequested))

            timer.Stop()
            Dim completedUtc As DateTime = DateTime.UtcNow

            ThrowIfCancellationRequested(cancellationRequested)

            Return New SpcFitResult(request,
                                    calculated.Panels,
                                    calculated.Warnings,
                                    timer.Elapsed.TotalMilliseconds,
                                    startedUtc,
                                    completedUtc)
        End Function

        Private Function FindCalculator(chartType As SpcChartType) As ISpcChartCalculator
            Dim found As ISpcChartCalculator = Nothing
            For i As Integer = 0 To _calculators.Length - 1
                If _calculators(i).CanCalculate(chartType) Then
                    If found IsNot Nothing Then
                        Throw New InvalidOperationException(
                            "More than one SPC calculator is registered for " & chartType.ToString() & ".")
                    End If
                    found = _calculators(i)
                End If
            Next
            Return found
        End Function

        Private Shared Function CreateDefaultEngine() As SpcEngine
            Dim calculators As New List(Of ISpcChartCalculator)()
            Dim contract As Type = GetType(ISpcChartCalculator)
            Dim assemblyTypes As Type()

            Try
                assemblyTypes = contract.Assembly.GetTypes()
            Catch ex As ReflectionTypeLoadException
                Dim available As New List(Of Type)()
                For Each candidate As Type In ex.Types
                    If candidate IsNot Nothing Then available.Add(candidate)
                Next
                assemblyTypes = available.ToArray()
            End Try

            Array.Sort(assemblyTypes,
                       Function(left As Type, right As Type) As Integer
                           Return StringComparer.Ordinal.Compare(left.FullName, right.FullName)
                       End Function)

            For Each candidate As Type In assemblyTypes
                If candidate Is contract OrElse
                   candidate.IsAbstract OrElse
                   candidate.IsInterface OrElse
                   Not contract.IsAssignableFrom(candidate) Then
                    Continue For
                End If

                Dim constructor As ConstructorInfo = candidate.GetConstructor(
                    BindingFlags.Instance Or BindingFlags.Public Or BindingFlags.NonPublic,
                    binder:=Nothing,
                    types:=Type.EmptyTypes,
                    modifiers:=Nothing)
                If constructor Is Nothing Then Continue For

                Dim instance As Object = constructor.Invoke(Array.Empty(Of Object)())
                calculators.Add(DirectCast(instance, ISpcChartCalculator))
            Next

            Return New SpcEngine(calculators)
        End Function

        Private Shared Sub ValidateChartAndLayout(request As SpcFitRequest)
            ValidateEnum(request.ChartType, NameOf(request.ChartType))
            ValidateEnum(request.DataLayout, NameOf(request.DataLayout))

            Select Case request.ChartType
                Case SpcChartType.PChart, SpcChartType.NpChart,
                     SpcChartType.CChart, SpcChartType.UChart,
                     SpcChartType.LaneyPPrime, SpcChartType.LaneyUPrime
                    If request.DataLayout <> SpcDataLayout.AggregatedCounts Then
                        Throw New ArgumentException(
                            "Attribute charts require the aggregated-counts data layout.")
                    End If

                Case SpcChartType.Individuals, SpcChartType.MovingRange,
                     SpcChartType.IndividualsMovingRange, SpcChartType.RunChart,
                     SpcChartType.Cusum, SpcChartType.Ewma, SpcChartType.MovingAverage,
                     SpcChartType.GChart, SpcChartType.TChart
                    If request.DataLayout <> SpcDataLayout.IndividualSequence Then
                        Throw New ArgumentException(
                            request.ChartType.ToString() &
                            " requires the individual-sequence data layout.")
                    End If

                Case SpcChartType.XBar, SpcChartType.SubgroupRange,
                     SpcChartType.SubgroupStandardDeviation,
                     SpcChartType.XBarR, SpcChartType.XBarS
                    If request.DataLayout <> SpcDataLayout.WideSubgroups AndAlso
                       request.DataLayout <> SpcDataLayout.StackedObservations Then
                        Throw New ArgumentException(
                            request.ChartType.ToString() &
                            " requires wide-subgroup or stacked-observation data.")
                    End If
            End Select

            If request.DataLayout = SpcDataLayout.StackedObservations Then
                Dim subgroupIds As String() = request.Data.SubgroupIds
                If subgroupIds Is Nothing Then
                    Throw New ArgumentException(
                        "Stacked-observation data require subgroup identifiers.")
                End If
            End If
        End Sub

        Private Shared Sub ValidateOptions(request As SpcFitRequest)
            Dim options As SpcAnalysisOptions = request.AnalysisOptions
            ValidateEnum(options.MissingValuePolicy, "MissingValuePolicy")
            If options.ControlLimits Is Nothing Then
                Throw New ArgumentException("Control-limit options are required.")
            End If
            If options.Rules Is Nothing Then Throw New ArgumentException("Rule options are required.")

            Dim limits As SpcControlLimitOptions = options.ControlLimits
            ValidateEnum(limits.ParameterSource, "ParameterSource")
            ValidateEnum(limits.Method, "ControlLimitMethod")
            ValidateEnum(limits.WithinSigmaEstimator, "WithinSigmaEstimator")
            ValidateEnum(limits.NaturalLimitPolicy, "NaturalLimitPolicy")
            If Double.IsNaN(limits.SigmaMultiplier) OrElse
               Double.IsInfinity(limits.SigmaMultiplier) OrElse
               limits.SigmaMultiplier <= 0.0 Then
                Throw New ArgumentOutOfRangeException(
                    "SigmaMultiplier", "The sigma multiplier must be finite and positive.")
            End If
            If limits.MovingRangeLength < 2 Then
                Throw New ArgumentOutOfRangeException(
                    "MovingRangeLength", "The moving-range length must be at least two.")
            End If

            Dim rules As SpcRuleOptions = options.Rules
            ValidateEnum(rules.Preset, "RulePreset")
            ValidateEnum(rules.PhaseScope, "RulePhaseScope")
            ValidateEnum(rules.GapBehavior, "SequenceGapBehavior")
            ValidateEnum(rules.MarkingMode, "SignalMarkingMode")
            If rules.Preset = SpcRulePreset.Custom AndAlso
               (rules.CustomRules Is Nothing OrElse rules.CustomRules.Length = 0) Then
                Throw New ArgumentException(
                    "At least one custom rule is required when the custom preset is selected.")
            End If
        End Sub

        Private Shared Sub ValidateStages(request As SpcFitRequest)
            Dim stages As SpcStageDefinition() = request.Stages
            If stages.Length = 0 Then Return

            Dim ids As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
            Dim covered(request.Data.RowCount - 1) As Boolean
            For Each stage As SpcStageDefinition In stages
                If stage Is Nothing Then Throw New ArgumentException("Stage entries must not be null.")
                If Not ids.Add(stage.StageId) Then
                    Throw New ArgumentException("Stage identifiers must be unique, ignoring case.")
                End If
                If stage.LastPointIndex >= request.Data.RowCount Then
                    Throw New ArgumentOutOfRangeException(
                        "Stages", "A stage extends beyond the available input rows.")
                End If
                For pointIndex As Integer = stage.FirstPointIndex To stage.LastPointIndex
                    If covered(pointIndex) Then
                        Throw New ArgumentException("Stage point ranges must not overlap.")
                    End If
                    covered(pointIndex) = True
                Next
            Next

            For Each stage As SpcStageDefinition In stages
                If stage.LimitMode = SpcStageLimitMode.UseReferenceStage AndAlso
                   Not ids.Contains(stage.ReferenceStageId) Then
                    Throw New ArgumentException(
                        "Reference stage '" & stage.ReferenceStageId & "' was not defined.")
                End If
            Next
        End Sub

        Private Shared Sub ValidateExclusions(request As SpcFitRequest)
            Dim seen As New HashSet(Of String)(StringComparer.Ordinal)
            For Each exclusion As SpcExclusionDefinition In request.Exclusions
                If exclusion Is Nothing Then
                    Throw New ArgumentException("Exclusion entries must not be null.")
                End If
                If exclusion.PointIndex >= request.Data.RowCount Then
                    Throw New ArgumentOutOfRangeException(
                        "Exclusions", "An exclusion refers to a point beyond the available input rows.")
                End If
                Dim key As String = exclusion.PointIndex.ToString(Globalization.CultureInfo.InvariantCulture) &
                                    ":" & CInt(exclusion.Scope).ToString(Globalization.CultureInfo.InvariantCulture)
                If Not seen.Add(key) Then
                    Throw New ArgumentException(
                        "Duplicate exclusions for the same point and scope are not permitted.")
                End If
            Next
        End Sub

        Private Shared Sub ValidateHistoricalParameters(request As SpcFitRequest)
            Dim history As SpcHistoricalParameters() = request.HistoricalParameters
            Dim stageIds As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
            Dim hasDefault As Boolean = False
            For Each item As SpcHistoricalParameters In history
                If item.AppliesToAllStages Then
                    If hasDefault Then
                        Throw New ArgumentException(
                            "Only one default historical-parameter entry is permitted.")
                    End If
                    hasDefault = True
                ElseIf Not stageIds.Add(item.StageId) Then
                    Throw New ArgumentException(
                        "Only one historical-parameter entry is permitted per stage.")
                End If
            Next

            Dim options As SpcAnalysisOptions = request.AnalysisOptions
            If options.ControlLimits.ParameterSource = SpcParameterSource.UseHistoricalParameters AndAlso
               history.Length = 0 Then
                Throw New ArgumentException(
                    "Historical parameters are required by the selected parameter source.")
            End If
        End Sub

        Private Shared Sub ValidateDataValues(request As SpcFitRequest)
            Dim data As SpcInputData = request.Data
            Dim counts As Double() = data.Counts
            Dim sampleSizes As Double() = data.SampleSizes
            Dim exposures As Double() = data.Exposures

            If counts IsNot Nothing Then
                For i As Integer = 0 To counts.Length - 1
                    If Not Double.IsNaN(counts(i)) AndAlso counts(i) < 0.0 Then
                        Throw New ArgumentOutOfRangeException("Counts", "Counts must be nonnegative.")
                    End If
                Next
            End If
            If sampleSizes IsNot Nothing Then
                For i As Integer = 0 To sampleSizes.Length - 1
                    If Not Double.IsNaN(sampleSizes(i)) AndAlso sampleSizes(i) <= 0.0 Then
                        Throw New ArgumentOutOfRangeException(
                            "SampleSizes", "Sample sizes must be positive.")
                    End If
                Next
            End If
            If exposures IsNot Nothing Then
                For i As Integer = 0 To exposures.Length - 1
                    If Not Double.IsNaN(exposures(i)) AndAlso exposures(i) <= 0.0 Then
                        Throw New ArgumentOutOfRangeException(
                            "Exposures", "Exposure values must be positive.")
                    End If
                Next
            End If

            Select Case request.ChartType
                Case SpcChartType.PChart, SpcChartType.NpChart, SpcChartType.LaneyPPrime
                    RequireVector(counts, "Counts are required for p and np charts.")
                    RequireVector(sampleSizes, "Sample sizes are required for p and np charts.")
                    ValidateIntegerVector(counts, "Count")
                    ValidateIntegerVector(sampleSizes, "Sample size")
                    For i As Integer = 0 To counts.Length - 1
                        If Not Double.IsNaN(counts(i)) AndAlso
                           Not Double.IsNaN(sampleSizes(i)) AndAlso
                           counts(i) > sampleSizes(i) Then
                            Throw New ArgumentException(
                                "A nonconforming count must not exceed its sample size.")
                        End If
                    Next

                Case SpcChartType.CChart
                    RequireVector(counts, "Counts are required for a c chart.")
                    ValidateIntegerVector(counts, "Count")

                Case SpcChartType.UChart, SpcChartType.LaneyUPrime
                    RequireVector(counts, "Counts are required for a u chart.")
                    RequireVector(exposures, "Exposure values are required for a u chart.")
                    ValidateIntegerVector(counts, "Count")
            End Select

            If request.ChartType = SpcChartType.NpChart AndAlso sampleSizes IsNot Nothing Then
                Dim expected As Double = Double.NaN
                For Each value As Double In sampleSizes
                    If Double.IsNaN(value) Then Continue For
                    If Double.IsNaN(expected) Then
                        expected = value
                    ElseIf value <> expected Then
                        Throw New ArgumentException("An np chart requires constant sample size.")
                    End If
                Next
            End If
        End Sub

        Private Shared Sub RequireVector(values As Double(), message As String)
            If values Is Nothing Then Throw New ArgumentException(message)
        End Sub

        Private Shared Sub ValidateIntegerVector(values As Double(), valueName As String)
            For i As Integer = 0 To values.Length - 1
                Dim value As Double = values(i)
                If Not Double.IsNaN(value) AndAlso value <> Math.Truncate(value) Then
                    Throw New ArgumentException(
                        valueName & " at point " & (i + 1).ToString() & " must be an integer.")
                End If
            Next
        End Sub

        Private Shared Sub ValidateEnum(Of T As Structure)(value As T, parameterName As String)
            If Not [Enum].IsDefined(GetType(T), value) Then
                Throw New ArgumentOutOfRangeException(parameterName)
            End If
        End Sub

        Private Shared Function NormalizeCancellation(
            cancellationRequested As Func(Of Boolean)) As Func(Of Boolean)

            If cancellationRequested Is Nothing Then Return Function() False
            Return cancellationRequested
        End Function

        Friend Shared Sub ThrowIfCancellationRequested(
            cancellationRequested As Func(Of Boolean))

            If cancellationRequested IsNot Nothing AndAlso cancellationRequested() Then
                Throw New OperationCanceledException("The SPC calculation was cancelled.")
            End If
        End Sub
    End Class

End Namespace
