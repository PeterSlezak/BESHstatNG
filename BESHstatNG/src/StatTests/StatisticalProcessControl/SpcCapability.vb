Option Explicit On
Option Strict On
Option Infer On

Imports System
Imports System.Collections.Generic
Imports System.Globalization

Namespace StatisticalProcessControl

    ''' <summary>Specifies the method used for a continuous capability analysis.</summary>
    Public Enum SpcContinuousCapabilityMethod
        ''' <summary>Normal-distribution capability using within and overall standard deviations.</summary>
        Normal = 0

        ''' <summary>NIST-style distribution-free indices based on the 0.135th and 99.865th percentiles.</summary>
        NonnormalPercentile = 1
    End Enum

    ''' <summary>Identifies the probability model used for an attribute capability analysis.</summary>
    Public Enum SpcAttributeCapabilityModel
        Binomial = 0
        Poisson = 1
    End Enum

    ''' <summary>Identifies the source of a reported nonconformance estimate.</summary>
    Public Enum SpcCapabilityPerformanceBasis
        Observed = 0
        WithinNormal = 1
        OverallNormal = 2
    End Enum

    ''' <summary>
    ''' Immutable input for normal or percentile-based continuous capability analysis.
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' At least one specification limit is required. When subgroup identifiers are
    ''' supplied, the automatic within-sigma estimator is the pooled subgroup standard
    ''' deviation. Without subgroup identifiers, the automatic estimator is the average
    ''' moving range of length two.
    ''' </para>
    ''' <para>
    ''' A supplied process mean, within sigma, or overall sigma is treated as a
    ''' historical value. Confidence intervals that depend on a supplied sigma are
    ''' returned only when its degrees of freedom are also supplied.
    ''' </para>
    ''' </remarks>
    Public NotInheritable Class SpcContinuousCapabilityRequest
        Private ReadOnly _values As Double()
        Private ReadOnly _subgroupIds As String()
        Private ReadOnly _specifications As SpcSpecificationLimits
        Private ReadOnly _method As SpcContinuousCapabilityMethod
        Private ReadOnly _withinSigmaEstimator As SpcWithinSigmaEstimator
        Private ReadOnly _movingRangeLength As Integer
        Private ReadOnly _useBiasCorrection As Boolean
        Private ReadOnly _omitMissing As Boolean
        Private ReadOnly _processMean As Nullable(Of Double)
        Private ReadOnly _withinProcessSigma As Nullable(Of Double)
        Private ReadOnly _withinSigmaDegreesOfFreedom As Nullable(Of Double)
        Private ReadOnly _overallProcessSigma As Nullable(Of Double)
        Private ReadOnly _overallSigmaDegreesOfFreedom As Nullable(Of Double)
        Private ReadOnly _confidenceLevel As Double
        Private ReadOnly _lowerPercentileProbability As Double
        Private ReadOnly _upperPercentileProbability As Double
        Private ReadOnly _requestLabel As String

        Public Sub New(values As Double(),
                       specifications As SpcSpecificationLimits,
                       Optional method As SpcContinuousCapabilityMethod = SpcContinuousCapabilityMethod.Normal,
                       Optional subgroupIds As String() = Nothing,
                       Optional withinSigmaEstimator As SpcWithinSigmaEstimator = SpcWithinSigmaEstimator.Automatic,
                       Optional movingRangeLength As Integer = 2,
                       Optional useBiasCorrection As Boolean = True,
                       Optional omitMissing As Boolean = True,
                       Optional processMean As Nullable(Of Double) = Nothing,
                       Optional withinProcessSigma As Nullable(Of Double) = Nothing,
                       Optional withinSigmaDegreesOfFreedom As Nullable(Of Double) = Nothing,
                       Optional overallProcessSigma As Nullable(Of Double) = Nothing,
                       Optional overallSigmaDegreesOfFreedom As Nullable(Of Double) = Nothing,
                       Optional confidenceLevel As Double = 0.95,
                       Optional lowerPercentileProbability As Double = 0.00135,
                       Optional upperPercentileProbability As Double = 0.99865,
                       Optional requestLabel As String = Nothing)

            If values Is Nothing Then Throw New ArgumentNullException(NameOf(values))
            If values.Length = 0 Then
                Throw New ArgumentException("At least one measurement is required.", NameOf(values))
            End If
            If specifications Is Nothing Then Throw New ArgumentNullException(NameOf(specifications))
            If Not specifications.LowerSpecificationLimit.HasValue AndAlso
               Not specifications.UpperSpecificationLimit.HasValue Then
                Throw New ArgumentException(
                    "At least one lower or upper specification limit is required.",
                    NameOf(specifications))
            End If
            If Not [Enum].IsDefined(GetType(SpcContinuousCapabilityMethod), method) Then
                Throw New ArgumentOutOfRangeException(NameOf(method))
            End If
            If Not [Enum].IsDefined(GetType(SpcWithinSigmaEstimator), withinSigmaEstimator) Then
                Throw New ArgumentOutOfRangeException(NameOf(withinSigmaEstimator))
            End If
            If movingRangeLength < 2 OrElse movingRangeLength > 25 Then
                Throw New ArgumentOutOfRangeException(
                    NameOf(movingRangeLength),
                    "The moving-range length must be from 2 through 25.")
            End If
            If confidenceLevel <= 0.0 OrElse confidenceLevel >= 1.0 OrElse
               Double.IsNaN(confidenceLevel) OrElse Double.IsInfinity(confidenceLevel) Then
                Throw New ArgumentOutOfRangeException(
                    NameOf(confidenceLevel),
                    "The confidence level must be in the open interval (0, 1).")
            End If
            ValidatePercentileProbabilities(lowerPercentileProbability,
                                            upperPercentileProbability)
            ValidateOptionalPositive(processMean, NameOf(processMean), allowAnyFinite:=True)
            ValidateOptionalPositive(withinProcessSigma, NameOf(withinProcessSigma))
            ValidateOptionalPositive(withinSigmaDegreesOfFreedom,
                                     NameOf(withinSigmaDegreesOfFreedom))
            ValidateOptionalPositive(overallProcessSigma, NameOf(overallProcessSigma))
            ValidateOptionalPositive(overallSigmaDegreesOfFreedom,
                                     NameOf(overallSigmaDegreesOfFreedom))

            If subgroupIds IsNot Nothing AndAlso subgroupIds.Length <> values.Length Then
                Throw New ArgumentException(
                    "The subgroup identifier vector must have one entry per measurement.",
                    NameOf(subgroupIds))
            End If

            Dim copiedValues As Double() = CType(values.Clone(), Double())
            Dim copiedIds As String() = Nothing
            If subgroupIds IsNot Nothing Then
                copiedIds = New String(subgroupIds.Length - 1) {}
            End If

            Dim finiteCount As Integer = 0
            For i As Integer = 0 To copiedValues.Length - 1
                Dim value As Double = copiedValues(i)
                If Double.IsInfinity(value) Then
                    Throw New ArgumentException(
                        "Measurements must not contain infinity.", NameOf(values))
                End If
                If Double.IsNaN(value) Then
                    If Not omitMissing Then
                        Throw New ArgumentException(
                            "Measurements must not contain missing values when OmitMissing is false.",
                            NameOf(values))
                    End If
                Else
                    finiteCount += 1
                End If

                If copiedIds IsNot Nothing Then
                    copiedIds(i) = NormalizeText(subgroupIds(i))
                    If Not Double.IsNaN(value) AndAlso copiedIds(i).Length = 0 Then
                        Throw New ArgumentException(
                            "Every finite measurement must have a nonblank subgroup identifier.",
                            NameOf(subgroupIds))
                    End If
                End If
            Next
            If finiteCount < 2 Then
                Throw New ArgumentException(
                    "At least two finite measurements are required.", NameOf(values))
            End If

            _values = copiedValues
            _subgroupIds = copiedIds
            _specifications = specifications
            _method = method
            _withinSigmaEstimator = withinSigmaEstimator
            _movingRangeLength = movingRangeLength
            _useBiasCorrection = useBiasCorrection
            _omitMissing = omitMissing
            _processMean = processMean
            _withinProcessSigma = withinProcessSigma
            _withinSigmaDegreesOfFreedom = withinSigmaDegreesOfFreedom
            _overallProcessSigma = overallProcessSigma
            _overallSigmaDegreesOfFreedom = overallSigmaDegreesOfFreedom
            _confidenceLevel = confidenceLevel
            _lowerPercentileProbability = lowerPercentileProbability
            _upperPercentileProbability = upperPercentileProbability
            _requestLabel = NormalizeText(requestLabel)
        End Sub

        Public ReadOnly Property Values As Double()
            Get
                Return CType(_values.Clone(), Double())
            End Get
        End Property

        Public ReadOnly Property SubgroupIds As String()
            Get
                If _subgroupIds Is Nothing Then Return Nothing
                Return CType(_subgroupIds.Clone(), String())
            End Get
        End Property

        Public ReadOnly Property HasSubgroups As Boolean
            Get
                Return _subgroupIds IsNot Nothing
            End Get
        End Property

        Public ReadOnly Property Specifications As SpcSpecificationLimits
            Get
                Return _specifications
            End Get
        End Property

        Public ReadOnly Property Method As SpcContinuousCapabilityMethod
            Get
                Return _method
            End Get
        End Property

        Public ReadOnly Property WithinSigmaEstimator As SpcWithinSigmaEstimator
            Get
                Return _withinSigmaEstimator
            End Get
        End Property

        Public ReadOnly Property MovingRangeLength As Integer
            Get
                Return _movingRangeLength
            End Get
        End Property

        Public ReadOnly Property UseBiasCorrection As Boolean
            Get
                Return _useBiasCorrection
            End Get
        End Property

        Public ReadOnly Property OmitMissing As Boolean
            Get
                Return _omitMissing
            End Get
        End Property

        Public ReadOnly Property ProcessMean As Nullable(Of Double)
            Get
                Return _processMean
            End Get
        End Property

        Public ReadOnly Property WithinProcessSigma As Nullable(Of Double)
            Get
                Return _withinProcessSigma
            End Get
        End Property

        Public ReadOnly Property WithinSigmaDegreesOfFreedom As Nullable(Of Double)
            Get
                Return _withinSigmaDegreesOfFreedom
            End Get
        End Property

        Public ReadOnly Property OverallProcessSigma As Nullable(Of Double)
            Get
                Return _overallProcessSigma
            End Get
        End Property

        Public ReadOnly Property OverallSigmaDegreesOfFreedom As Nullable(Of Double)
            Get
                Return _overallSigmaDegreesOfFreedom
            End Get
        End Property

        Public ReadOnly Property ConfidenceLevel As Double
            Get
                Return _confidenceLevel
            End Get
        End Property

        Public ReadOnly Property LowerPercentileProbability As Double
            Get
                Return _lowerPercentileProbability
            End Get
        End Property

        Public ReadOnly Property UpperPercentileProbability As Double
            Get
                Return _upperPercentileProbability
            End Get
        End Property

        Public ReadOnly Property RequestLabel As String
            Get
                Return _requestLabel
            End Get
        End Property

        Private Shared Sub ValidatePercentileProbabilities(lowerProbability As Double,
                                                           upperProbability As Double)
            If Double.IsNaN(lowerProbability) OrElse Double.IsInfinity(lowerProbability) OrElse
               Double.IsNaN(upperProbability) OrElse Double.IsInfinity(upperProbability) OrElse
               lowerProbability <= 0.0 OrElse lowerProbability >= 0.5 OrElse
               upperProbability <= 0.5 OrElse upperProbability >= 1.0 OrElse
               lowerProbability >= upperProbability Then
                Throw New ArgumentOutOfRangeException(
                    NameOf(lowerProbability),
                    "Percentile probabilities must satisfy 0 < lower < 0.5 < upper < 1.")
            End If
        End Sub

        Private Shared Sub ValidateOptionalPositive(value As Nullable(Of Double),
                                                    parameterName As String,
                                                    Optional allowAnyFinite As Boolean = False)
            If Not value.HasValue Then Return
            If Double.IsNaN(value.Value) OrElse Double.IsInfinity(value.Value) Then
                Throw New ArgumentOutOfRangeException(parameterName, "The value must be finite.")
            End If
            If Not allowAnyFinite AndAlso value.Value <= 0.0 Then
                Throw New ArgumentOutOfRangeException(parameterName, "The value must be positive.")
            End If
        End Sub

        Private Shared Function NormalizeText(value As String) As String
            Return If(value, String.Empty).Trim()
        End Function
    End Class

    ''' <summary>Immutable input for binomial capability analysis.</summary>
    Public NotInheritable Class SpcBinomialCapabilityRequest
        Private ReadOnly _nonconformingCounts As Integer()
        Private ReadOnly _sampleSizes As Integer()

        Public Sub New(nonconformingCounts As Integer(),
                       sampleSizes As Integer(),
                       Optional confidenceLevel As Double = 0.95,
                       Optional requestLabel As String = Nothing)
            ValidateConfidenceLevel(confidenceLevel)
            If nonconformingCounts Is Nothing Then
                Throw New ArgumentNullException(NameOf(nonconformingCounts))
            End If
            If sampleSizes Is Nothing Then Throw New ArgumentNullException(NameOf(sampleSizes))
            If nonconformingCounts.Length = 0 OrElse
               nonconformingCounts.Length <> sampleSizes.Length Then
                Throw New ArgumentException(
                    "Counts and sample sizes must be nonempty vectors of equal length.")
            End If
            For i As Integer = 0 To nonconformingCounts.Length - 1
                If sampleSizes(i) <= 0 Then
                    Throw New ArgumentOutOfRangeException(
                        NameOf(sampleSizes), "Every sample size must be positive.")
                End If
                If nonconformingCounts(i) < 0 OrElse
                   nonconformingCounts(i) > sampleSizes(i) Then
                    Throw New ArgumentOutOfRangeException(
                        NameOf(nonconformingCounts),
                        "Each nonconforming count must be from zero through its sample size.")
                End If
            Next
            _nonconformingCounts = CType(nonconformingCounts.Clone(), Integer())
            _sampleSizes = CType(sampleSizes.Clone(), Integer())
            Me.ConfidenceLevel = confidenceLevel
            Me.RequestLabel = If(requestLabel, String.Empty).Trim()
        End Sub

        Public ReadOnly Property NonconformingCounts As Integer()
            Get
                Return CType(_nonconformingCounts.Clone(), Integer())
            End Get
        End Property

        Public ReadOnly Property SampleSizes As Integer()
            Get
                Return CType(_sampleSizes.Clone(), Integer())
            End Get
        End Property

        Public ReadOnly Property ConfidenceLevel As Double
        Public ReadOnly Property RequestLabel As String

        Private Shared Sub ValidateConfidenceLevel(value As Double)
            If Double.IsNaN(value) OrElse Double.IsInfinity(value) OrElse
               value <= 0.0 OrElse value >= 1.0 Then
                Throw New ArgumentOutOfRangeException(
                    NameOf(value), "The confidence level must be in the open interval (0, 1).")
            End If
        End Sub
    End Class

    ''' <summary>Immutable input for Poisson capability analysis.</summary>
    Public NotInheritable Class SpcPoissonCapabilityRequest
        Private ReadOnly _defectCounts As Integer()
        Private ReadOnly _exposures As Double()

        Public Sub New(defectCounts As Integer(),
                       exposures As Double(),
                       Optional confidenceLevel As Double = 0.95,
                       Optional requestLabel As String = Nothing)
            ValidateConfidenceLevel(confidenceLevel)
            If defectCounts Is Nothing Then Throw New ArgumentNullException(NameOf(defectCounts))
            If exposures Is Nothing Then Throw New ArgumentNullException(NameOf(exposures))
            If defectCounts.Length = 0 OrElse defectCounts.Length <> exposures.Length Then
                Throw New ArgumentException(
                    "Defect counts and exposures must be nonempty vectors of equal length.")
            End If
            For i As Integer = 0 To defectCounts.Length - 1
                If defectCounts(i) < 0 Then
                    Throw New ArgumentOutOfRangeException(
                        NameOf(defectCounts), "Defect counts must be nonnegative integers.")
                End If
                If Double.IsNaN(exposures(i)) OrElse Double.IsInfinity(exposures(i)) OrElse
                   exposures(i) <= 0.0 Then
                    Throw New ArgumentOutOfRangeException(
                        NameOf(exposures), "Every exposure must be finite and positive.")
                End If
            Next
            _defectCounts = CType(defectCounts.Clone(), Integer())
            _exposures = CType(exposures.Clone(), Double())
            Me.ConfidenceLevel = confidenceLevel
            Me.RequestLabel = If(requestLabel, String.Empty).Trim()
        End Sub

        Public ReadOnly Property DefectCounts As Integer()
            Get
                Return CType(_defectCounts.Clone(), Integer())
            End Get
        End Property

        Public ReadOnly Property Exposures As Double()
            Get
                Return CType(_exposures.Clone(), Double())
            End Get
        End Property

        Public ReadOnly Property ConfidenceLevel As Double
        Public ReadOnly Property RequestLabel As String

        Private Shared Sub ValidateConfidenceLevel(value As Double)
            If Double.IsNaN(value) OrElse Double.IsInfinity(value) OrElse
               value <= 0.0 OrElse value >= 1.0 Then
                Throw New ArgumentOutOfRangeException(
                    NameOf(value), "The confidence level must be in the open interval (0, 1).")
            End If
        End Sub
    End Class

    ''' <summary>One continuous capability index and its optional confidence interval.</summary>
    Public NotInheritable Class SpcCapabilityIndexResult
        Public Sub New(name As String,
                       displayName As String,
                       estimate As Double,
                       basis As String,
                       Optional lowerConfidenceLimit As Nullable(Of Double) = Nothing,
                       Optional upperConfidenceLimit As Nullable(Of Double) = Nothing,
                       Optional confidenceLevel As Nullable(Of Double) = Nothing)
            If String.IsNullOrWhiteSpace(name) Then
                Throw New ArgumentException("An index name is required.", NameOf(name))
            End If
            If String.IsNullOrWhiteSpace(displayName) Then
                Throw New ArgumentException("An index display name is required.", NameOf(displayName))
            End If
            If String.IsNullOrWhiteSpace(basis) Then
                Throw New ArgumentException("An index basis is required.", NameOf(basis))
            End If
            ValidateFinite(estimate, NameOf(estimate))
            ValidateOptionalFinite(lowerConfidenceLimit, NameOf(lowerConfidenceLimit))
            ValidateOptionalFinite(upperConfidenceLimit, NameOf(upperConfidenceLimit))
            If lowerConfidenceLimit.HasValue Xor upperConfidenceLimit.HasValue Then
                Throw New ArgumentException(
                    "Both confidence limits must be supplied together.")
            End If
            If lowerConfidenceLimit.HasValue AndAlso
               lowerConfidenceLimit.Value > upperConfidenceLimit.Value Then
                Throw New ArgumentException(
                    "The lower confidence limit must not exceed the upper confidence limit.")
            End If
            If lowerConfidenceLimit.HasValue Then
                If Not confidenceLevel.HasValue OrElse confidenceLevel.Value <= 0.0 OrElse
                   confidenceLevel.Value >= 1.0 OrElse
                   Double.IsNaN(confidenceLevel.Value) OrElse
                   Double.IsInfinity(confidenceLevel.Value) Then
                    Throw New ArgumentOutOfRangeException(
                        NameOf(confidenceLevel),
                        "A confidence level in (0, 1) is required when limits are supplied.")
                End If
            ElseIf confidenceLevel.HasValue Then
                Throw New ArgumentException(
                    "A confidence level must not be supplied without confidence limits.",
                    NameOf(confidenceLevel))
            End If

            Me.Name = name.Trim()
            Me.DisplayName = displayName.Trim()
            Me.Estimate = estimate
            Me.Basis = basis.Trim()
            Me.LowerConfidenceLimit = lowerConfidenceLimit
            Me.UpperConfidenceLimit = upperConfidenceLimit
            Me.ConfidenceLevel = confidenceLevel
        End Sub

        Public ReadOnly Property Name As String
        Public ReadOnly Property DisplayName As String
        Public ReadOnly Property Estimate As Double
        Public ReadOnly Property Basis As String
        Public ReadOnly Property LowerConfidenceLimit As Nullable(Of Double)
        Public ReadOnly Property UpperConfidenceLimit As Nullable(Of Double)
        Public ReadOnly Property ConfidenceLevel As Nullable(Of Double)

        Public ReadOnly Property HasConfidenceInterval As Boolean
            Get
                Return LowerConfidenceLimit.HasValue
            End Get
        End Property

        Private Shared Sub ValidateFinite(value As Double, parameterName As String)
            If Double.IsNaN(value) OrElse Double.IsInfinity(value) Then
                Throw New ArgumentOutOfRangeException(parameterName, "The value must be finite.")
            End If
        End Sub

        Private Shared Sub ValidateOptionalFinite(value As Nullable(Of Double),
                                                 parameterName As String)
            If value.HasValue Then ValidateFinite(value.Value, parameterName)
        End Sub
    End Class

    ''' <summary>Observed or normally predicted nonconformance performance.</summary>
    Public NotInheritable Class SpcCapabilityPerformanceResult
        Public Sub New(basis As SpcCapabilityPerformanceBasis,
                       belowLowerSpecification As Nullable(Of Double),
                       aboveUpperSpecification As Nullable(Of Double))
            If Not [Enum].IsDefined(GetType(SpcCapabilityPerformanceBasis), basis) Then
                Throw New ArgumentOutOfRangeException(NameOf(basis))
            End If
            ValidateOptionalProbability(belowLowerSpecification,
                                        NameOf(belowLowerSpecification))
            ValidateOptionalProbability(aboveUpperSpecification,
                                        NameOf(aboveUpperSpecification))
            If Not belowLowerSpecification.HasValue AndAlso
               Not aboveUpperSpecification.HasValue Then
                Throw New ArgumentException("At least one performance tail is required.")
            End If
            Dim total As Double = If(belowLowerSpecification, 0.0) +
                                  If(aboveUpperSpecification, 0.0)
            If total > 1.0 + 0.000000000001 Then
                Throw New ArgumentException(
                    "The sum of nonconforming tail probabilities must not exceed one.")
            End If

            Me.Basis = basis
            Me.BelowLowerSpecification = belowLowerSpecification
            Me.AboveUpperSpecification = aboveUpperSpecification
            Me.TotalNonconforming = Math.Min(1.0, total)
        End Sub

        Public ReadOnly Property Basis As SpcCapabilityPerformanceBasis
        Public ReadOnly Property BelowLowerSpecification As Nullable(Of Double)
        Public ReadOnly Property AboveUpperSpecification As Nullable(Of Double)
        Public ReadOnly Property TotalNonconforming As Double

        Public ReadOnly Property BelowLowerPpm As Nullable(Of Double)
            Get
                If Not BelowLowerSpecification.HasValue Then Return Nothing
                Return BelowLowerSpecification.Value * 1000000.0
            End Get
        End Property

        Public ReadOnly Property AboveUpperPpm As Nullable(Of Double)
            Get
                If Not AboveUpperSpecification.HasValue Then Return Nothing
                Return AboveUpperSpecification.Value * 1000000.0
            End Get
        End Property

        Public ReadOnly Property TotalPpm As Double
            Get
                Return TotalNonconforming * 1000000.0
            End Get
        End Property

        Private Shared Sub ValidateOptionalProbability(value As Nullable(Of Double),
                                                       parameterName As String)
            If Not value.HasValue Then Return
            If Double.IsNaN(value.Value) OrElse Double.IsInfinity(value.Value) OrElse
               value.Value < 0.0 OrElse value.Value > 1.0 Then
                Throw New ArgumentOutOfRangeException(parameterName,
                    "A probability must be from zero through one.")
            End If
        End Sub
    End Class

    ''' <summary>Complete immutable result of a continuous capability analysis.</summary>
    Public NotInheritable Class SpcContinuousCapabilityResult
        Private ReadOnly _indices As SpcCapabilityIndexResult()
        Private ReadOnly _performance As SpcCapabilityPerformanceResult()
        Private ReadOnly _warnings As String()

        Friend Sub New(request As SpcContinuousCapabilityRequest,
                       sampleCount As Integer,
                       sampleMean As Double,
                       processMean As Double,
                       median As Double,
                       minimum As Double,
                       maximum As Double,
                       overallSigma As Double,
                       overallSigmaDegreesOfFreedom As Nullable(Of Double),
                       withinSigma As Nullable(Of Double),
                       withinSigmaDegreesOfFreedom As Nullable(Of Double),
                       withinSigmaMethod As String,
                       lowerPercentile As Nullable(Of Double),
                       upperPercentile As Nullable(Of Double),
                       indices As SpcCapabilityIndexResult(),
                       performance As SpcCapabilityPerformanceResult(),
                       warnings As String())
            If request Is Nothing Then Throw New ArgumentNullException(NameOf(request))
            If sampleCount < 2 Then Throw New ArgumentOutOfRangeException(NameOf(sampleCount))
            ValidateFinite(sampleMean, NameOf(sampleMean))
            ValidateFinite(processMean, NameOf(processMean))
            ValidateFinite(median, NameOf(median))
            ValidateFinite(minimum, NameOf(minimum))
            ValidateFinite(maximum, NameOf(maximum))
            ValidatePositive(overallSigma, NameOf(overallSigma))
            If withinSigma.HasValue Then ValidatePositive(withinSigma.Value, NameOf(withinSigma))
            ValidateOptionalPositive(overallSigmaDegreesOfFreedom,
                                     NameOf(overallSigmaDegreesOfFreedom))
            ValidateOptionalPositive(withinSigmaDegreesOfFreedom,
                                     NameOf(withinSigmaDegreesOfFreedom))
            ValidateOptionalFinite(lowerPercentile, NameOf(lowerPercentile))
            ValidateOptionalFinite(upperPercentile, NameOf(upperPercentile))
            If lowerPercentile.HasValue Xor upperPercentile.HasValue Then
                Throw New ArgumentException("Both empirical percentiles must be supplied together.")
            End If
            If lowerPercentile.HasValue AndAlso
               lowerPercentile.Value >= upperPercentile.Value Then
                Throw New ArgumentException("The lower empirical percentile must be smaller.")
            End If
            If indices Is Nothing OrElse indices.Length = 0 Then
                Throw New ArgumentException("At least one capability index is required.", NameOf(indices))
            End If
            If performance Is Nothing OrElse performance.Length = 0 Then
                Throw New ArgumentException("At least one performance result is required.", NameOf(performance))
            End If

            _indices = CType(indices.Clone(), SpcCapabilityIndexResult())
            _performance = CType(performance.Clone(), SpcCapabilityPerformanceResult())
            _warnings = CopyWarnings(warnings)
            For Each item As SpcCapabilityIndexResult In _indices
                If item Is Nothing Then Throw New ArgumentException("Indices must not contain null entries.", NameOf(indices))
            Next
            For Each item As SpcCapabilityPerformanceResult In _performance
                If item Is Nothing Then Throw New ArgumentException("Performance results must not contain null entries.", NameOf(performance))
            Next

            Me.Request = request
            Me.SampleCount = sampleCount
            Me.SampleMean = sampleMean
            Me.ProcessMean = processMean
            Me.Median = median
            Me.Minimum = minimum
            Me.Maximum = maximum
            Me.OverallSigma = overallSigma
            Me.OverallSigmaDegreesOfFreedom = overallSigmaDegreesOfFreedom
            Me.WithinSigma = withinSigma
            Me.WithinSigmaDegreesOfFreedom = withinSigmaDegreesOfFreedom
            Me.WithinSigmaMethod = If(withinSigmaMethod, String.Empty).Trim()
            Me.LowerPercentile = lowerPercentile
            Me.UpperPercentile = upperPercentile
        End Sub

        Public ReadOnly Property Request As SpcContinuousCapabilityRequest
        Public ReadOnly Property Method As SpcContinuousCapabilityMethod
            Get
                Return Request.Method
            End Get
        End Property
        Public ReadOnly Property SampleCount As Integer
        Public ReadOnly Property SampleMean As Double
        Public ReadOnly Property ProcessMean As Double
        Public ReadOnly Property Median As Double
        Public ReadOnly Property Minimum As Double
        Public ReadOnly Property Maximum As Double
        Public ReadOnly Property OverallSigma As Double
        Public ReadOnly Property OverallSigmaDegreesOfFreedom As Nullable(Of Double)
        Public ReadOnly Property WithinSigma As Nullable(Of Double)
        Public ReadOnly Property WithinSigmaDegreesOfFreedom As Nullable(Of Double)
        Public ReadOnly Property WithinSigmaMethod As String
        Public ReadOnly Property LowerPercentile As Nullable(Of Double)
        Public ReadOnly Property UpperPercentile As Nullable(Of Double)

        Public ReadOnly Property Indices As SpcCapabilityIndexResult()
            Get
                Return CType(_indices.Clone(), SpcCapabilityIndexResult())
            End Get
        End Property

        Public ReadOnly Property Performance As SpcCapabilityPerformanceResult()
            Get
                Return CType(_performance.Clone(), SpcCapabilityPerformanceResult())
            End Get
        End Property

        Public ReadOnly Property Warnings As String()
            Get
                Return CType(_warnings.Clone(), String())
            End Get
        End Property

        Public Function GetIndex(name As String) As SpcCapabilityIndexResult
            If String.IsNullOrWhiteSpace(name) Then Return Nothing
            For Each item As SpcCapabilityIndexResult In _indices
                If String.Equals(item.Name, name.Trim(), StringComparison.OrdinalIgnoreCase) Then
                    Return item
                End If
            Next
            Return Nothing
        End Function

        Private Shared Function CopyWarnings(values As String()) As String()
            If values Is Nothing Then Return Array.Empty(Of String)()
            Dim result As New List(Of String)(values.Length)
            For Each value As String In values
                Dim text As String = If(value, String.Empty).Trim()
                If text.Length > 0 Then result.Add(text)
            Next
            Return result.ToArray()
        End Function

        Private Shared Sub ValidateFinite(value As Double, parameterName As String)
            If Double.IsNaN(value) OrElse Double.IsInfinity(value) Then
                Throw New ArgumentOutOfRangeException(parameterName, "The value must be finite.")
            End If
        End Sub

        Private Shared Sub ValidatePositive(value As Double, parameterName As String)
            ValidateFinite(value, parameterName)
            If value <= 0.0 Then Throw New ArgumentOutOfRangeException(parameterName)
        End Sub

        Private Shared Sub ValidateOptionalFinite(value As Nullable(Of Double),
                                                 parameterName As String)
            If value.HasValue Then ValidateFinite(value.Value, parameterName)
        End Sub

        Private Shared Sub ValidateOptionalPositive(value As Nullable(Of Double),
                                                   parameterName As String)
            If Not value.HasValue Then Return
            ValidatePositive(value.Value, parameterName)
        End Sub
    End Class

    ''' <summary>Complete immutable result of binomial or Poisson capability analysis.</summary>
    Public NotInheritable Class SpcAttributeCapabilityResult
        Private ReadOnly _warnings As String()

        Friend Sub New(model As SpcAttributeCapabilityModel,
                       requestLabel As String,
                       rowCount As Integer,
                       totalEvents As Long,
                       totalOpportunity As Double,
                       rate As Double,
                       lowerConfidenceLimit As Double,
                       upperConfidenceLimit As Double,
                       confidenceLevel As Double,
                       yieldProbability As Double,
                       lowerYieldConfidenceLimit As Double,
                       upperYieldConfidenceLimit As Double,
                       zBench As Nullable(Of Double),
                       lowerZBenchConfidenceLimit As Nullable(Of Double),
                       upperZBenchConfidenceLimit As Nullable(Of Double),
                       warnings As String())
            If Not [Enum].IsDefined(GetType(SpcAttributeCapabilityModel), model) Then
                Throw New ArgumentOutOfRangeException(NameOf(model))
            End If
            If rowCount < 1 Then Throw New ArgumentOutOfRangeException(NameOf(rowCount))
            If totalEvents < 0L Then Throw New ArgumentOutOfRangeException(NameOf(totalEvents))
            ValidatePositive(totalOpportunity, NameOf(totalOpportunity))
            ValidateNonnegative(rate, NameOf(rate))
            ValidateNonnegative(lowerConfidenceLimit, NameOf(lowerConfidenceLimit))
            ValidateNonnegative(upperConfidenceLimit, NameOf(upperConfidenceLimit))
            If lowerConfidenceLimit > rate OrElse rate > upperConfidenceLimit Then
                Throw New ArgumentException("The attribute rate must lie within its confidence interval.")
            End If
            ValidateProbability(confidenceLevel, NameOf(confidenceLevel), openInterval:=True)
            ValidateProbability(yieldProbability, NameOf(yieldProbability))
            ValidateProbability(lowerYieldConfidenceLimit, NameOf(lowerYieldConfidenceLimit))
            ValidateProbability(upperYieldConfidenceLimit, NameOf(upperYieldConfidenceLimit))
            If lowerYieldConfidenceLimit > yieldProbability OrElse
               yieldProbability > upperYieldConfidenceLimit Then
                Throw New ArgumentException("The yield must lie within its confidence interval.")
            End If
            ValidateOptionalFinite(zBench, NameOf(zBench))
            ValidateOptionalFinite(lowerZBenchConfidenceLimit,
                                   NameOf(lowerZBenchConfidenceLimit))
            ValidateOptionalFinite(upperZBenchConfidenceLimit,
                                   NameOf(upperZBenchConfidenceLimit))
            If lowerZBenchConfidenceLimit.HasValue AndAlso
               upperZBenchConfidenceLimit.HasValue AndAlso
               lowerZBenchConfidenceLimit.Value > upperZBenchConfidenceLimit.Value Then
                Throw New ArgumentException("The Z.Bench confidence limits are reversed.")
            End If

            Me.Model = model
            Me.RequestLabel = If(requestLabel, String.Empty).Trim()
            Me.RowCount = rowCount
            Me.TotalEvents = totalEvents
            Me.TotalOpportunity = totalOpportunity
            Me.Rate = rate
            Me.LowerConfidenceLimit = lowerConfidenceLimit
            Me.UpperConfidenceLimit = upperConfidenceLimit
            Me.ConfidenceLevel = confidenceLevel
            Me.YieldProbability = yieldProbability
            Me.LowerYieldConfidenceLimit = lowerYieldConfidenceLimit
            Me.UpperYieldConfidenceLimit = upperYieldConfidenceLimit
            Me.ZBench = zBench
            Me.LowerZBenchConfidenceLimit = lowerZBenchConfidenceLimit
            Me.UpperZBenchConfidenceLimit = upperZBenchConfidenceLimit
            _warnings = CopyWarnings(warnings)
        End Sub

        Public ReadOnly Property Model As SpcAttributeCapabilityModel
        Public ReadOnly Property RequestLabel As String
        Public ReadOnly Property RowCount As Integer
        Public ReadOnly Property TotalEvents As Long
        Public ReadOnly Property TotalOpportunity As Double
        Public ReadOnly Property Rate As Double
        Public ReadOnly Property LowerConfidenceLimit As Double
        Public ReadOnly Property UpperConfidenceLimit As Double
        Public ReadOnly Property ConfidenceLevel As Double
        Public ReadOnly Property YieldProbability As Double
        Public ReadOnly Property LowerYieldConfidenceLimit As Double
        Public ReadOnly Property UpperYieldConfidenceLimit As Double
        Public ReadOnly Property ZBench As Nullable(Of Double)
        Public ReadOnly Property LowerZBenchConfidenceLimit As Nullable(Of Double)
        Public ReadOnly Property UpperZBenchConfidenceLimit As Nullable(Of Double)

        Public ReadOnly Property PartsPerMillion As Double
            Get
                Return Rate * 1000000.0
            End Get
        End Property

        Public ReadOnly Property LowerPartsPerMillionConfidenceLimit As Double
            Get
                Return LowerConfidenceLimit * 1000000.0
            End Get
        End Property

        Public ReadOnly Property UpperPartsPerMillionConfidenceLimit As Double
            Get
                Return UpperConfidenceLimit * 1000000.0
            End Get
        End Property

        Public ReadOnly Property Warnings As String()
            Get
                Return CType(_warnings.Clone(), String())
            End Get
        End Property

        Private Shared Function CopyWarnings(values As String()) As String()
            If values Is Nothing Then Return Array.Empty(Of String)()
            Dim result As New List(Of String)(values.Length)
            For Each value As String In values
                Dim text As String = If(value, String.Empty).Trim()
                If text.Length > 0 Then result.Add(text)
            Next
            Return result.ToArray()
        End Function

        Private Shared Sub ValidatePositive(value As Double, parameterName As String)
            If Double.IsNaN(value) OrElse Double.IsInfinity(value) OrElse value <= 0.0 Then
                Throw New ArgumentOutOfRangeException(parameterName)
            End If
        End Sub

        Private Shared Sub ValidateNonnegative(value As Double, parameterName As String)
            If Double.IsNaN(value) OrElse Double.IsInfinity(value) OrElse value < 0.0 Then
                Throw New ArgumentOutOfRangeException(parameterName)
            End If
        End Sub

        Private Shared Sub ValidateProbability(value As Double,
                                              parameterName As String,
                                              Optional openInterval As Boolean = False)
            If Double.IsNaN(value) OrElse Double.IsInfinity(value) OrElse
               value < 0.0 OrElse value > 1.0 OrElse
               (openInterval AndAlso (value <= 0.0 OrElse value >= 1.0)) Then
                Throw New ArgumentOutOfRangeException(parameterName)
            End If
        End Sub

        Private Shared Sub ValidateOptionalFinite(value As Nullable(Of Double),
                                                 parameterName As String)
            If value.HasValue AndAlso
               (Double.IsNaN(value.Value) OrElse Double.IsInfinity(value.Value)) Then
                Throw New ArgumentOutOfRangeException(parameterName)
            End If
        End Sub
    End Class

    ''' <summary>
    ''' Host-neutral calculations for continuous, binomial, and Poisson process capability.
    ''' </summary>
    Public NotInheritable Class SpcCapability
        Private Sub New()
        End Sub

        ''' <summary>Calculates a continuous capability request.</summary>
        Public Shared Function Analyze(
            request As SpcContinuousCapabilityRequest,
            Optional cancellationRequested As Func(Of Boolean) = Nothing) As SpcContinuousCapabilityResult

            Return AnalyzeContinuous(request, cancellationRequested)
        End Function

        ''' <summary>Calculates a binomial capability request.</summary>
        Public Shared Function Analyze(
            request As SpcBinomialCapabilityRequest,
            Optional cancellationRequested As Func(Of Boolean) = Nothing) As SpcAttributeCapabilityResult

            Return AnalyzeBinomial(request, cancellationRequested)
        End Function

        ''' <summary>Calculates a Poisson capability request.</summary>
        Public Shared Function Analyze(
            request As SpcPoissonCapabilityRequest,
            Optional cancellationRequested As Func(Of Boolean) = Nothing) As SpcAttributeCapabilityResult

            Return AnalyzePoisson(request, cancellationRequested)
        End Function

        ''' <summary>Calculates normal or percentile-based continuous capability.</summary>
        Public Shared Function AnalyzeContinuous(
            request As SpcContinuousCapabilityRequest,
            Optional cancellationRequested As Func(Of Boolean) = Nothing) As SpcContinuousCapabilityResult

            If request Is Nothing Then Throw New ArgumentNullException(NameOf(request))
            CheckCancellation(cancellationRequested)

            Dim rawValues As Double() = request.Values
            Dim rawSubgroupIds As String() = request.SubgroupIds
            Dim data As ContinuousData = BuildContinuousData(rawValues,
                                                             rawSubgroupIds,
                                                             request.OmitMissing,
                                                             cancellationRequested)
            Dim descriptive As SpcSubgroupStatistics =
                SpcStatistics.CalculateSubgroup(data.Values)
            If descriptive.SampleStandardDeviation <= 0.0 AndAlso
               Not request.OverallProcessSigma.HasValue Then
                Throw New InvalidOperationException(
                    "Overall process variation is zero; capability indices cannot be estimated.")
            End If

            Dim warnings As New List(Of String) From {
                "Capability indices describe a stable process. Assess statistical control before interpreting capability."
            }
            If data.Values.Length < 50 Then
                warnings.Add(
                    "Fewer than 50 finite measurements were available; capability estimates and confidence intervals may be imprecise.")
            End If
            If data.OmittedCount > 0 Then
                warnings.Add(
                    data.OmittedCount.ToString(CultureInfo.InvariantCulture) &
                    If(data.OmittedCount = 1, " missing measurement was", " missing measurements were") &
                    " omitted from the capability analysis.")
            End If

            Dim sampleMean As Double = descriptive.Mean
            Dim processMean As Double = If(request.ProcessMean, sampleMean)
            Dim overallSigma As Double = If(request.OverallProcessSigma,
                                            descriptive.SampleStandardDeviation)
            If overallSigma <= 0.0 Then
                Throw New InvalidOperationException(
                    "The overall process standard deviation must be positive.")
            End If
            Dim overallDf As Nullable(Of Double) = request.OverallSigmaDegreesOfFreedom
            If Not overallDf.HasValue AndAlso Not request.OverallProcessSigma.HasValue Then
                overallDf = CDbl(data.Values.Length - 1)
            End If

            Dim sorted As Double() = CType(data.Values.Clone(), Double())
            Array.Sort(sorted)
            Dim median As Double =
                Global.BESHStatNG.Resampling.ResamplingResults.QuantileSorted(sorted, 0.5)
            If request.Method = SpcContinuousCapabilityMethod.NonnormalPercentile Then
                If request.ProcessMean.HasValue Then
                    warnings.Add(
                        "The supplied process mean is not used by percentile capability; the empirical median is the location estimate.")
                End If
                processMean = median
            End If

            Dim indices As New List(Of SpcCapabilityIndexResult)()
            Dim performance As New List(Of SpcCapabilityPerformanceResult)()
            performance.Add(BuildObservedPerformance(data.Values,
                                                     request.Specifications,
                                                     cancellationRequested))

            Dim withinSigma As Nullable(Of Double) = Nothing
            Dim withinDf As Nullable(Of Double) = Nothing
            Dim withinMethod As String = String.Empty
            Dim lowerPercentile As Nullable(Of Double) = Nothing
            Dim upperPercentile As Nullable(Of Double) = Nothing

            Select Case request.Method
                Case SpcContinuousCapabilityMethod.Normal
                    Dim within As SigmaResolution = ResolveWithinSigma(request,
                                                                      rawValues,
                                                                      data,
                                                                      cancellationRequested)
                    withinSigma = within.Value
                    withinDf = within.DegreesOfFreedom
                    withinMethod = within.Method
                    If Not withinDf.HasValue Then
                        warnings.Add(
                            "Within-capability confidence intervals were omitted because the within-process sigma has no degrees of freedom.")
                    End If
                    If Not overallDf.HasValue Then
                        warnings.Add(
                            "Overall-capability confidence intervals were omitted because the overall standard deviation has no degrees of freedom.")
                    End If
                    If data.Values.Length < 25 Then
                        warnings.Add(
                            "The normal-approximation intervals for Cpk, Ppk, and one-sided indices are unreliable with fewer than 25 observations.")
                    End If

                    BuildNormalIndices(request,
                                       processMean,
                                       within,
                                       overallSigma,
                                       overallDf,
                                       data.Values.Length,
                                       indices)
                    If request.OverallProcessSigma.HasValue AndAlso
                       request.Specifications.Target.HasValue AndAlso
                       request.Specifications.LowerSpecificationLimit.HasValue AndAlso
                       request.Specifications.UpperSpecificationLimit.HasValue Then
                        warnings.Add(
                            "The Cpm confidence interval was omitted because the overall process sigma was supplied rather than estimated from these data.")
                    End If
                    performance.Add(BuildNormalPerformance(
                        SpcCapabilityPerformanceBasis.WithinNormal,
                        processMean,
                        within.Value,
                        request.Specifications))
                    performance.Add(BuildNormalPerformance(
                        SpcCapabilityPerformanceBasis.OverallNormal,
                        processMean,
                        overallSigma,
                        request.Specifications))

                Case SpcContinuousCapabilityMethod.NonnormalPercentile
                    If request.WithinProcessSigma.HasValue OrElse
                       request.WithinSigmaDegreesOfFreedom.HasValue OrElse
                       request.WithinSigmaEstimator <> SpcWithinSigmaEstimator.Automatic Then
                        warnings.Add(
                            "Within-sigma settings do not affect empirical percentile capability indices.")
                    End If
                    If request.OverallProcessSigma.HasValue OrElse
                       request.OverallSigmaDegreesOfFreedom.HasValue Then
                        warnings.Add(
                            "Overall-sigma settings are descriptive only and do not affect empirical percentile capability indices.")
                    End If
                    lowerPercentile =
                        Global.BESHStatNG.Resampling.ResamplingResults.QuantileSorted(
                            sorted, request.LowerPercentileProbability)
                    upperPercentile =
                        Global.BESHStatNG.Resampling.ResamplingResults.QuantileSorted(
                            sorted, request.UpperPercentileProbability)
                    If upperPercentile.Value <= lowerPercentile.Value Then
                        Throw New InvalidOperationException(
                            "The empirical capability percentile width is zero.")
                    End If
                    BuildNonnormalIndices(request,
                                          median,
                                          lowerPercentile.Value,
                                          upperPercentile.Value,
                                          indices)
                    warnings.Add(
                        "Confidence intervals are not reported for empirical percentile capability indices; bootstrap intervals belong in a later resampling layer.")
                    Dim tailRequirement As Double =
                        1.0 / Math.Min(request.LowerPercentileProbability,
                                       1.0 - request.UpperPercentileProbability)
                    If CDbl(data.Values.Length) < tailRequirement Then
                        warnings.Add(
                            "The requested extreme empirical percentiles extend beyond one expected tail observation and therefore depend strongly on the sample extremes.")
                    End If

                Case Else
                    Throw New ArgumentOutOfRangeException(NameOf(request.Method))
            End Select

            CheckCancellation(cancellationRequested)
            Return New SpcContinuousCapabilityResult(
                request,
                data.Values.Length,
                sampleMean,
                processMean,
                median,
                descriptive.Minimum,
                descriptive.Maximum,
                overallSigma,
                overallDf,
                withinSigma,
                withinDf,
                withinMethod,
                lowerPercentile,
                upperPercentile,
                indices.ToArray(),
                performance.ToArray(),
                warnings.ToArray())
        End Function

        ''' <summary>Calculates binomial nonconformance capability with an exact Clopper-Pearson interval.</summary>
        Public Shared Function AnalyzeBinomial(
            request As SpcBinomialCapabilityRequest,
            Optional cancellationRequested As Func(Of Boolean) = Nothing) As SpcAttributeCapabilityResult

            If request Is Nothing Then Throw New ArgumentNullException(NameOf(request))
            Dim counts As Integer() = request.NonconformingCounts
            Dim sizes As Integer() = request.SampleSizes
            Dim events As Long = 0L
            Dim opportunities As Long = 0L

            For i As Integer = 0 To counts.Length - 1
                CheckCancellationPeriodically(i, cancellationRequested)
                events = SafeAdd(events, counts(i), NameOf(request.NonconformingCounts))
                opportunities = SafeAdd(opportunities, sizes(i), NameOf(request.SampleSizes))
            Next

            Dim total As Double = CDbl(opportunities)
            Dim rate As Double = CDbl(events) / total
            Dim alpha As Double = 1.0 - request.ConfidenceLevel
            Dim lower As Double
            Dim upper As Double
            If events = 0L Then
                lower = 0.0
            Else
                lower = Global.BESHStatNG.distributions.Distributions.InverseRegularizedIncompleteBeta(
                        alpha / 2.0,
                        CDbl(events),
                        CDbl(opportunities - events + 1L))
            End If
            If events = opportunities Then
                upper = 1.0
            Else
                upper = Global.BESHStatNG.distributions.Distributions.InverseRegularizedIncompleteBeta(
                        1.0 - alpha / 2.0,
                        CDbl(events + 1L),
                        CDbl(opportunities - events))
            End If

            Dim yield As Double = 1.0 - rate
            Dim lowerYield As Double = 1.0 - upper
            Dim upperYield As Double = 1.0 - lower
            Dim warnings As New List(Of String) From {
                "Binomial capability assumes independent trials with a constant nonconforming probability."
            }
            Dim z As Nullable(Of Double) = ProbabilityToZ(yield)
            Dim lowerZ As Nullable(Of Double) = ProbabilityToZ(lowerYield)
            Dim upperZ As Nullable(Of Double) = ProbabilityToZ(upperYield)
            AddBoundaryZWarning(z, warnings)

            Return New SpcAttributeCapabilityResult(
                SpcAttributeCapabilityModel.Binomial,
                request.RequestLabel,
                counts.Length,
                events,
                total,
                rate,
                lower,
                upper,
                request.ConfidenceLevel,
                yield,
                lowerYield,
                upperYield,
                z,
                lowerZ,
                upperZ,
                warnings.ToArray())
        End Function

        ''' <summary>Calculates Poisson defect-rate capability with an exact chi-square interval.</summary>
        Public Shared Function AnalyzePoisson(
            request As SpcPoissonCapabilityRequest,
            Optional cancellationRequested As Func(Of Boolean) = Nothing) As SpcAttributeCapabilityResult

            If request Is Nothing Then Throw New ArgumentNullException(NameOf(request))
            Dim counts As Integer() = request.DefectCounts
            Dim exposures As Double() = request.Exposures
            Dim events As Long = 0L
            Dim totalExposure As Double = 0.0
            Dim compensation As Double = 0.0

            For i As Integer = 0 To counts.Length - 1
                CheckCancellationPeriodically(i, cancellationRequested)
                events = SafeAdd(events, counts(i), NameOf(request.DefectCounts))
                CompensatedAdd(totalExposure, compensation, exposures(i))
            Next
            If Double.IsInfinity(totalExposure) OrElse totalExposure <= 0.0 Then
                Throw New InvalidOperationException("The total Poisson exposure is invalid.")
            End If
            If events = Long.MaxValue Then
                Throw New OverflowException(
                    "The aggregated Poisson count is too large to construct an exact confidence interval.")
            End If

            Dim rate As Double = CDbl(events) / totalExposure
            Dim alpha As Double = 1.0 - request.ConfidenceLevel
            Dim lower As Double
            If events = 0L Then
                lower = 0.0
            Else
                lower = 0.5 * Global.BESHStatNG.distributions.Distributions.ChiSquareInv(
                    alpha / 2.0, 2.0 * CDbl(events)) / totalExposure
            End If
            Dim upper As Double =
                0.5 * Global.BESHStatNG.distributions.Distributions.ChiSquareInv(
                    1.0 - alpha / 2.0,
                    2.0 * CDbl(events + 1L)) / totalExposure

            Dim yield As Double = Math.Exp(-rate)
            Dim lowerYield As Double = Math.Exp(-upper)
            Dim upperYield As Double = Math.Exp(-lower)
            Dim warnings As New List(Of String) From {
                "Poisson capability assumes independent defect occurrences and a constant rate per unit of exposure.",
                "Poisson yield is the probability of zero defects, exp(-rate), for one unit of the supplied exposure scale."
            }
            Dim z As Nullable(Of Double) = ProbabilityToZ(yield)
            Dim lowerZ As Nullable(Of Double) = ProbabilityToZ(lowerYield)
            Dim upperZ As Nullable(Of Double) = ProbabilityToZ(upperYield)
            AddBoundaryZWarning(z, warnings)

            Return New SpcAttributeCapabilityResult(
                SpcAttributeCapabilityModel.Poisson,
                request.RequestLabel,
                counts.Length,
                events,
                totalExposure,
                rate,
                lower,
                upper,
                request.ConfidenceLevel,
                yield,
                lowerYield,
                upperYield,
                z,
                lowerZ,
                upperZ,
                warnings.ToArray())
        End Function

#Region "Continuous capability calculations"

        Private Shared Sub BuildNormalIndices(request As SpcContinuousCapabilityRequest,
                                             mean As Double,
                                             within As SigmaResolution,
                                             overallSigma As Double,
                                             overallDf As Nullable(Of Double),
                                             sampleCount As Integer,
                                             output As List(Of SpcCapabilityIndexResult))
            Dim specs As SpcSpecificationLimits = request.Specifications
            Dim confidenceLevel As Double = request.ConfidenceLevel
            Dim includeMeanUncertainty As Boolean = Not request.ProcessMean.HasValue

            Dim cpl As Nullable(Of Double) = Nothing
            Dim cpu As Nullable(Of Double) = Nothing
            If specs.LowerSpecificationLimit.HasValue Then
                cpl = (mean - specs.LowerSpecificationLimit.Value) / (3.0 * within.Value)
                output.Add(CreateApproximateIndex(
                    "CPL", "Lower potential capability", cpl.Value,
                    "Within-process sigma", sampleCount, within.DegreesOfFreedom,
                    confidenceLevel, includeMeanUncertainty))
            End If
            If specs.UpperSpecificationLimit.HasValue Then
                cpu = (specs.UpperSpecificationLimit.Value - mean) / (3.0 * within.Value)
                output.Add(CreateApproximateIndex(
                    "CPU", "Upper potential capability", cpu.Value,
                    "Within-process sigma", sampleCount, within.DegreesOfFreedom,
                    confidenceLevel, includeMeanUncertainty))
            End If
            If specs.LowerSpecificationLimit.HasValue AndAlso
               specs.UpperSpecificationLimit.HasValue Then
                Dim cp As Double =
                    (specs.UpperSpecificationLimit.Value -
                     specs.LowerSpecificationLimit.Value) / (6.0 * within.Value)
                output.Insert(0, CreateSpreadIndex(
                    "Cp", "Potential capability", cp, "Within-process sigma",
                    within.DegreesOfFreedom, confidenceLevel))
            End If
            Dim cpk As Double = MinimumAvailable(cpl, cpu)
            output.Add(CreateApproximateIndex(
                "Cpk", "Minimum potential capability", cpk,
                "Within-process sigma", sampleCount, within.DegreesOfFreedom,
                confidenceLevel, includeMeanUncertainty))

            Dim ppl As Nullable(Of Double) = Nothing
            Dim ppu As Nullable(Of Double) = Nothing
            If specs.LowerSpecificationLimit.HasValue Then
                ppl = (mean - specs.LowerSpecificationLimit.Value) / (3.0 * overallSigma)
                output.Add(CreateApproximateIndex(
                    "PPL", "Lower overall capability", ppl.Value,
                    "Overall standard deviation", sampleCount, overallDf,
                    confidenceLevel, includeMeanUncertainty))
            End If
            If specs.UpperSpecificationLimit.HasValue Then
                ppu = (specs.UpperSpecificationLimit.Value - mean) / (3.0 * overallSigma)
                output.Add(CreateApproximateIndex(
                    "PPU", "Upper overall capability", ppu.Value,
                    "Overall standard deviation", sampleCount, overallDf,
                    confidenceLevel, includeMeanUncertainty))
            End If
            If specs.LowerSpecificationLimit.HasValue AndAlso
               specs.UpperSpecificationLimit.HasValue Then
                Dim pp As Double =
                    (specs.UpperSpecificationLimit.Value -
                     specs.LowerSpecificationLimit.Value) / (6.0 * overallSigma)
                output.Add(CreateSpreadIndex(
                    "Pp", "Overall capability", pp, "Overall standard deviation",
                    overallDf, confidenceLevel))
            End If
            Dim ppk As Double = MinimumAvailable(ppl, ppu)
            output.Add(CreateApproximateIndex(
                "Ppk", "Minimum overall capability", ppk,
                "Overall standard deviation", sampleCount, overallDf,
                confidenceLevel, includeMeanUncertainty))

            If specs.LowerSpecificationLimit.HasValue AndAlso
               specs.UpperSpecificationLimit.HasValue AndAlso specs.Target.HasValue Then
                Dim targetDistance As Double = mean - specs.Target.Value
                Dim denominator As Double =
                    6.0 * Math.Sqrt(overallSigma * overallSigma +
                                    targetDistance * targetDistance)
                Dim cpm As Double =
                    (specs.UpperSpecificationLimit.Value -
                     specs.LowerSpecificationLimit.Value) / denominator
                Dim a As Double = targetDistance / overallSigma
                If request.OverallProcessSigma.HasValue Then
                    output.Add(New SpcCapabilityIndexResult(
                        "Cpm", "Target capability", cpm,
                        "Supplied overall variation and target deviation"))
                Else
                    Dim cpmDf As Double =
                        CDbl(sampleCount) * Math.Pow(1.0 + a * a, 2.0) /
                        (1.0 + 2.0 * a * a)
                    output.Add(CreateSpreadIndex(
                        "Cpm", "Target capability", cpm,
                        "Overall variation and target deviation (approximate interval)",
                        cpmDf, confidenceLevel))
                End If
            End If
        End Sub

        Private Shared Sub BuildNonnormalIndices(request As SpcContinuousCapabilityRequest,
                                                median As Double,
                                                lowerPercentile As Double,
                                                upperPercentile As Double,
                                                output As List(Of SpcCapabilityIndexResult))
            Dim specs As SpcSpecificationLimits = request.Specifications
            Dim percentileWidth As Double = upperPercentile - lowerPercentile
            Dim halfWidth As Double = percentileWidth / 2.0
            Dim cnpl As Nullable(Of Double) = Nothing
            Dim cnpu As Nullable(Of Double) = Nothing

            If specs.LowerSpecificationLimit.HasValue Then
                cnpl = (median - specs.LowerSpecificationLimit.Value) / halfWidth
                output.Add(New SpcCapabilityIndexResult(
                    "Cnpl", "Lower nonnormal capability", cnpl.Value,
                    "Empirical percentile width"))
            End If
            If specs.UpperSpecificationLimit.HasValue Then
                cnpu = (specs.UpperSpecificationLimit.Value - median) / halfWidth
                output.Add(New SpcCapabilityIndexResult(
                    "Cnpu", "Upper nonnormal capability", cnpu.Value,
                    "Empirical percentile width"))
            End If
            If specs.LowerSpecificationLimit.HasValue AndAlso
               specs.UpperSpecificationLimit.HasValue Then
                Dim cnp As Double =
                    (specs.UpperSpecificationLimit.Value -
                     specs.LowerSpecificationLimit.Value) / percentileWidth
                output.Insert(0, New SpcCapabilityIndexResult(
                    "Cnp", "Nonnormal potential capability", cnp,
                    "Empirical percentile width"))
            End If
            output.Add(New SpcCapabilityIndexResult(
                "Cnpk", "Minimum nonnormal capability",
                MinimumAvailable(cnpl, cnpu), "Empirical percentile width"))

            If specs.LowerSpecificationLimit.HasValue AndAlso
               specs.UpperSpecificationLimit.HasValue AndAlso specs.Target.HasValue Then
                Dim robustSigma As Double = percentileWidth / 6.0
                Dim targetDistance As Double = median - specs.Target.Value
                Dim cnpm As Double =
                    (specs.UpperSpecificationLimit.Value -
                     specs.LowerSpecificationLimit.Value) /
                    (6.0 * Math.Sqrt(robustSigma * robustSigma +
                                     targetDistance * targetDistance))
                output.Add(New SpcCapabilityIndexResult(
                    "Cnpm", "Nonnormal target capability", cnpm,
                    "Empirical percentile width and median target deviation"))
            End If
        End Sub

        Private Shared Function CreateSpreadIndex(name As String,
                                                 displayName As String,
                                                 estimate As Double,
                                                 basis As String,
                                                 degreesOfFreedom As Nullable(Of Double),
                                                 confidenceLevel As Double) As SpcCapabilityIndexResult
            If Not degreesOfFreedom.HasValue Then
                Return New SpcCapabilityIndexResult(name, displayName, estimate, basis)
            End If
            Dim alpha As Double = 1.0 - confidenceLevel
            Dim lower As Double = estimate * Math.Sqrt(
                Global.BESHStatNG.distributions.Distributions.ChiSquareInv(
                    alpha / 2.0, degreesOfFreedom.Value) / degreesOfFreedom.Value)
            Dim upper As Double = estimate * Math.Sqrt(
                Global.BESHStatNG.distributions.Distributions.ChiSquareInv(
                    1.0 - alpha / 2.0, degreesOfFreedom.Value) /
                degreesOfFreedom.Value)
            Return New SpcCapabilityIndexResult(
                name, displayName, estimate, basis,
                lower, upper, confidenceLevel)
        End Function

        Private Shared Function CreateApproximateIndex(
            name As String,
            displayName As String,
            estimate As Double,
            basis As String,
            sampleCount As Integer,
            degreesOfFreedom As Nullable(Of Double),
            confidenceLevel As Double,
            includeMeanUncertainty As Boolean) As SpcCapabilityIndexResult

            If Not degreesOfFreedom.HasValue Then
                Return New SpcCapabilityIndexResult(name, displayName, estimate, basis)
            End If
            Dim alpha As Double = 1.0 - confidenceLevel
            Dim z As Double =
                Global.BESHStatNG.distributions.Distributions.NormSInv(
                    1.0 - alpha / 2.0)
            Dim standardError As Double = Math.Sqrt(
                If(includeMeanUncertainty,
                   1.0 / (9.0 * CDbl(sampleCount)),
                   0.0) +
                estimate * estimate / (2.0 * degreesOfFreedom.Value))
            Return New SpcCapabilityIndexResult(
                name, displayName, estimate, basis,
                estimate - z * standardError,
                estimate + z * standardError,
                confidenceLevel)
        End Function

        Private Shared Function BuildObservedPerformance(
            values As Double(),
            specifications As SpcSpecificationLimits,
            cancellationRequested As Func(Of Boolean)) As SpcCapabilityPerformanceResult

            Dim below As Integer = 0
            Dim above As Integer = 0
            For i As Integer = 0 To values.Length - 1
                CheckCancellationPeriodically(i, cancellationRequested)
                If specifications.LowerSpecificationLimit.HasValue AndAlso
                   values(i) < specifications.LowerSpecificationLimit.Value Then
                    below += 1
                End If
                If specifications.UpperSpecificationLimit.HasValue AndAlso
                   values(i) > specifications.UpperSpecificationLimit.Value Then
                    above += 1
                End If
            Next
            Dim belowRate As Nullable(Of Double) = Nothing
            Dim aboveRate As Nullable(Of Double) = Nothing
            If specifications.LowerSpecificationLimit.HasValue Then
                belowRate = CDbl(below) / CDbl(values.Length)
            End If
            If specifications.UpperSpecificationLimit.HasValue Then
                aboveRate = CDbl(above) / CDbl(values.Length)
            End If
            Return New SpcCapabilityPerformanceResult(
                SpcCapabilityPerformanceBasis.Observed, belowRate, aboveRate)
        End Function

        Private Shared Function BuildNormalPerformance(
            basis As SpcCapabilityPerformanceBasis,
            mean As Double,
            sigma As Double,
            specifications As SpcSpecificationLimits) As SpcCapabilityPerformanceResult

            Dim below As Nullable(Of Double) = Nothing
            Dim above As Nullable(Of Double) = Nothing
            If specifications.LowerSpecificationLimit.HasValue Then
                below = Global.BESHStatNG.distributions.Distributions.PNorm(
                    specifications.LowerSpecificationLimit.Value, mean, sigma)
            End If
            If specifications.UpperSpecificationLimit.HasValue Then
                ' Symmetry avoids subtracting a probability close to one.
                above = Global.BESHStatNG.distributions.Distributions.PNorm(
                    mean - specifications.UpperSpecificationLimit.Value, 0.0, sigma)
            End If
            Return New SpcCapabilityPerformanceResult(basis, below, above)
        End Function

        Private Shared Function ResolveWithinSigma(
            request As SpcContinuousCapabilityRequest,
            rawValues As Double(),
            data As ContinuousData,
            cancellationRequested As Func(Of Boolean)) As SigmaResolution

            If request.WithinProcessSigma.HasValue Then
                Return New SigmaResolution(
                    request.WithinProcessSigma.Value,
                    request.WithinSigmaDegreesOfFreedom,
                    "Supplied within-process standard deviation",
                    request.WithinSigmaEstimator)
            End If

            Dim selected As SpcWithinSigmaEstimator = request.WithinSigmaEstimator
            If selected = SpcWithinSigmaEstimator.Automatic Then
                selected = If(data.SubgroupIds Is Nothing,
                              SpcWithinSigmaEstimator.MovingRange,
                              SpcWithinSigmaEstimator.PooledStandardDeviation)
            End If

            Dim estimate As SpcSigmaEstimate
            Dim degrees As Nullable(Of Double) = Nothing
            Select Case selected
                Case SpcWithinSigmaEstimator.MovingRange,
                     SpcWithinSigmaEstimator.MedianMovingRange
                    estimate = SpcStatistics.EstimateSigmaFromIndividuals(
                        rawValues,
                        selected,
                        request.MovingRangeLength,
                        request.UseBiasCorrection)
                    Dim ranges As Double() = SpcStatistics.MovingRanges(
                        rawValues, request.MovingRangeLength)
                    degrees = CDbl(ranges.Length)

                Case SpcWithinSigmaEstimator.SampleStandardDeviation,
                     SpcWithinSigmaEstimator.MedianAbsoluteDeviation
                    estimate = SpcStatistics.EstimateSigmaFromIndividuals(
                        data.Values,
                        selected,
                        request.MovingRangeLength,
                        request.UseBiasCorrection)
                    If selected = SpcWithinSigmaEstimator.SampleStandardDeviation Then
                        degrees = CDbl(data.Values.Length - 1)
                    End If

                Case SpcWithinSigmaEstimator.AverageRange,
                     SpcWithinSigmaEstimator.AverageStandardDeviation,
                     SpcWithinSigmaEstimator.PooledStandardDeviation
                    If data.SubgroupIds Is Nothing Then
                        Throw New ArgumentException(
                            "The selected within-sigma estimator requires subgroup identifiers.",
                            NameOf(request))
                    End If
                    Dim subgroups As SpcSubgroupStatistics() = BuildSubgroupStatistics(
                        data.Values, data.SubgroupIds, cancellationRequested)
                    estimate = SpcStatistics.EstimateSigmaFromSubgroups(
                        subgroups, selected, request.UseBiasCorrection)
                    degrees = EstimateSubgroupDegreesOfFreedom(subgroups, selected)

                Case Else
                    Throw New ArgumentOutOfRangeException(
                        NameOf(request.WithinSigmaEstimator),
                        "The selected within-sigma estimator is not supported for capability analysis.")
            End Select

            If estimate.Value <= 0.0 Then
                Throw New InvalidOperationException(
                    "The estimated within-process standard deviation is zero.")
            End If
            If request.WithinSigmaDegreesOfFreedom.HasValue Then
                degrees = request.WithinSigmaDegreesOfFreedom
            End If
            Return New SigmaResolution(estimate.Value,
                                       degrees,
                                       estimate.Method,
                                       estimate.Estimator)
        End Function

        Private Shared Function EstimateSubgroupDegreesOfFreedom(
            subgroups As SpcSubgroupStatistics(),
            estimator As SpcWithinSigmaEstimator) As Nullable(Of Double)

            Dim totalDegrees As Double = 0.0
            Select Case estimator
                Case SpcWithinSigmaEstimator.PooledStandardDeviation
                    For Each subgroup As SpcSubgroupStatistics In subgroups
                        totalDegrees += CDbl(Math.Max(0, subgroup.Count - 1))
                    Next

                Case SpcWithinSigmaEstimator.AverageRange
                    For Each subgroup As SpcSubgroupStatistics In subgroups
                        totalDegrees += 0.9 * CDbl(Math.Max(0, subgroup.Count - 1))
                    Next

                Case SpcWithinSigmaEstimator.AverageStandardDeviation
                    For Each subgroup As SpcSubgroupStatistics In subgroups
                        totalDegrees += StandardDeviationDfFactor(subgroup.Count) *
                                        CDbl(Math.Max(0, subgroup.Count - 1))
                    Next

                Case Else
                    Return Nothing
            End Select
            If totalDegrees <= 0.0 Then Return Nothing
            Return totalDegrees
        End Function

        Private Shared Function StandardDeviationDfFactor(subgroupSize As Integer) As Double
            If subgroupSize <= 1 Then Return 0.0
            If subgroupSize = 2 Then Return 0.88
            If subgroupSize = 3 Then Return 0.92
            If subgroupSize = 4 Then Return 0.94
            If subgroupSize = 5 Then Return 0.95
            If subgroupSize <= 7 Then Return 0.96
            If subgroupSize <= 9 Then Return 0.97
            If subgroupSize <= 17 Then Return 0.98
            If subgroupSize <= 64 Then Return 0.99
            Return 1.0
        End Function

        Private Shared Function BuildSubgroupStatistics(
            values As Double(),
            subgroupIds As String(),
            cancellationRequested As Func(Of Boolean)) As SpcSubgroupStatistics()

            Dim order As New List(Of String)()
            Dim groups As New Dictionary(Of String, List(Of Double))(
                StringComparer.OrdinalIgnoreCase)
            For i As Integer = 0 To values.Length - 1
                CheckCancellationPeriodically(i, cancellationRequested)
                Dim id As String = subgroupIds(i)
                Dim groupValues As List(Of Double) = Nothing
                If Not groups.TryGetValue(id, groupValues) Then
                    groupValues = New List(Of Double)()
                    groups.Add(id, groupValues)
                    order.Add(id)
                End If
                groupValues.Add(values(i))
            Next

            Dim result As New List(Of SpcSubgroupStatistics)(order.Count)
            For Each id As String In order
                Dim groupValues As Double() = groups(id).ToArray()
                If groupValues.Length < 2 Then
                    Throw New ArgumentException(
                        "Every subgroup used to estimate within-process sigma must contain at least two measurements. Subgroup '" &
                        id & "' contains only one.", NameOf(subgroupIds))
                End If
                result.Add(SpcStatistics.CalculateSubgroup(groupValues))
            Next
            Return result.ToArray()
        End Function

        Private Shared Function BuildContinuousData(
            values As Double(),
            subgroupIds As String(),
            omitMissing As Boolean,
            cancellationRequested As Func(Of Boolean)) As ContinuousData

            Dim cleanValues As New List(Of Double)(values.Length)
            Dim cleanIds As List(Of String) = Nothing
            If subgroupIds IsNot Nothing Then cleanIds = New List(Of String)(values.Length)
            Dim omitted As Integer = 0
            For i As Integer = 0 To values.Length - 1
                CheckCancellationPeriodically(i, cancellationRequested)
                If Double.IsNaN(values(i)) Then
                    If Not omitMissing Then
                        Throw New ArgumentException("Measurements contain missing values.", NameOf(values))
                    End If
                    omitted += 1
                    Continue For
                End If
                cleanValues.Add(values(i))
                If cleanIds IsNot Nothing Then cleanIds.Add(subgroupIds(i))
            Next
            If cleanValues.Count < 2 Then
                Throw New ArgumentException("At least two finite measurements are required.", NameOf(values))
            End If
            Dim resultIds As String() = Nothing
            If cleanIds IsNot Nothing Then resultIds = cleanIds.ToArray()
            Return New ContinuousData(cleanValues.ToArray(), resultIds, omitted)
        End Function

        Private Shared Function MinimumAvailable(first As Nullable(Of Double),
                                                second As Nullable(Of Double)) As Double
            If first.HasValue AndAlso second.HasValue Then
                Return Math.Min(first.Value, second.Value)
            End If
            If first.HasValue Then Return first.Value
            If second.HasValue Then Return second.Value
            Throw New InvalidOperationException("At least one specification-side index is required.")
        End Function

#End Region

#Region "Attribute capability helpers"

        Private Shared Function SafeAdd(total As Long,
                                       value As Integer,
                                       parameterName As String) As Long
            If value < 0 Then Throw New ArgumentOutOfRangeException(parameterName)
            If total > Long.MaxValue - CLng(value) Then
                Throw New OverflowException("The aggregated count exceeds Int64 capacity.")
            End If
            Return total + CLng(value)
        End Function

        Private Shared Sub CompensatedAdd(ByRef total As Double,
                                         ByRef compensation As Double,
                                         value As Double)
            Dim adjusted As Double = value - compensation
            Dim updated As Double = total + adjusted
            compensation = (updated - total) - adjusted
            total = updated
        End Sub

        Private Shared Function ProbabilityToZ(probability As Double) As Nullable(Of Double)
            If probability <= 0.0 OrElse probability >= 1.0 Then Return Nothing
            Dim value As Double =
                Global.BESHStatNG.distributions.Distributions.NormSInv(probability)
            If Double.IsNaN(value) OrElse Double.IsInfinity(value) Then Return Nothing
            Return value
        End Function

        Private Shared Sub AddBoundaryZWarning(value As Nullable(Of Double),
                                              warnings As List(Of String))
            If Not value.HasValue Then
                warnings.Add(
                    "Z.Bench is not finite when the estimated yield is exactly zero or one and is therefore reported as missing.")
            End If
        End Sub

#End Region

        Private Shared Sub CheckCancellation(cancellationRequested As Func(Of Boolean))
            If cancellationRequested IsNot Nothing AndAlso cancellationRequested() Then
                Throw New OperationCanceledException("SPC capability analysis was cancelled.")
            End If
        End Sub

        Private Shared Sub CheckCancellationPeriodically(
            index As Integer,
            cancellationRequested As Func(Of Boolean))
            If (index And 255) = 0 Then CheckCancellation(cancellationRequested)
        End Sub

        Private NotInheritable Class ContinuousData
            Friend Sub New(values As Double(), subgroupIds As String(), omittedCount As Integer)
                Me.Values = values
                Me.SubgroupIds = subgroupIds
                Me.OmittedCount = omittedCount
            End Sub

            Friend ReadOnly Property Values As Double()
            Friend ReadOnly Property SubgroupIds As String()
            Friend ReadOnly Property OmittedCount As Integer
        End Class

        Private NotInheritable Class SigmaResolution
            Friend Sub New(value As Double,
                           degreesOfFreedom As Nullable(Of Double),
                           method As String,
                           estimator As SpcWithinSigmaEstimator)
                If Double.IsNaN(value) OrElse Double.IsInfinity(value) OrElse value <= 0.0 Then
                    Throw New ArgumentOutOfRangeException(NameOf(value))
                End If
                If degreesOfFreedom.HasValue AndAlso
                   (Double.IsNaN(degreesOfFreedom.Value) OrElse
                    Double.IsInfinity(degreesOfFreedom.Value) OrElse
                    degreesOfFreedom.Value <= 0.0) Then
                    Throw New ArgumentOutOfRangeException(NameOf(degreesOfFreedom))
                End If
                Me.Value = value
                Me.DegreesOfFreedom = degreesOfFreedom
                Me.Method = If(method, String.Empty).Trim()
                Me.Estimator = estimator
            End Sub

            Friend ReadOnly Property Value As Double
            Friend ReadOnly Property DegreesOfFreedom As Nullable(Of Double)
            Friend ReadOnly Property Method As String
            Friend ReadOnly Property Estimator As SpcWithinSigmaEstimator
        End Class

    End Class

End Namespace
