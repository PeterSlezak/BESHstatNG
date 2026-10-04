Option Explicit On
Option Strict On
Imports BESHStatNG.AppInfrastructure

Namespace Agreement
    Public Enum AgreementCiMethod
        Analytical = 0
        Jackknife = 1
        BootstrapPercentile = 2
        BootstrapBCa = 3
    End Enum

    Public Enum BlandAltmanScale
        RawDifference = 0
        PercentOfMean = 1
        PercentOfReference = 2
        PercentOfTest = 3
        LogRatio = 4
    End Enum

    Public Enum BlandAltmanXAxisMode
        MeanOfMethods = 0
        ReferenceMethod = 1
        TestMethod = 2
    End Enum

    Public Enum RepeatedBlandAltmanMode
        Auto = 0
        SimplePairs = 1
        RepeatedBySubject = 2
    End Enum

    Public Enum RepeatedBlandAltmanPlotMode
        AllObservations = 0
        SubjectMeansOnly = 1
        AllObservationsAndSubjectMeans = 2
    End Enum

    Public Enum KappaWeightingScheme
        Unweighted = 0
        Linear = 1
        Quadratic = 2
        CicchettiAllison = 3
        FleissCohen = 4
        Custom = 5
    End Enum

    Public Enum DemingVarianceModel
        ConstantLambda = 0
        KnownPointwiseSD = 1
        ConstantCV = 2
    End Enum

    ''' <summary>
    ''' Options controlling ordinary or repeated-measures Bland–Altman agreement analysis.
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' When <see cref="Mode"/> is <see cref="RepeatedBlandAltmanMode.Auto"/>, the analysis behaves as follows:
    ''' </para>
    ''' <list type="bullet">
    '''   <item><description>if <see cref="SubjectIds"/> is <c>Nothing</c>, ordinary Bland–Altman is used</description></item>
    '''   <item><description>if <see cref="SubjectIds"/> is supplied, repeated-measures Bland–Altman is used</description></item>
    ''' </list>
    ''' </remarks>
    Public Class BlandAltmanOptions

        ''' <summary>
        ''' Gets or sets whether the analysis should run as simple paired Bland–Altman, repeated Bland–Altman, or automatic detection.
        ''' </summary>
        Public Property Mode As RepeatedBlandAltmanMode = RepeatedBlandAltmanMode.Auto

        ''' <summary>
        ''' Gets or sets the two-sided significance level used for confidence intervals.
        ''' </summary>
        Public Property Alpha As Double = 0.05

        ''' <summary>
        ''' Gets or sets the difference scale.
        ''' </summary>
        Public Property Scale As BlandAltmanScale = BlandAltmanScale.RawDifference

        ''' <summary>
        ''' Gets or sets the x-axis quantity used in the Bland–Altman plot.
        ''' </summary>
        Public Property XAxisMode As BlandAltmanXAxisMode = BlandAltmanXAxisMode.MeanOfMethods

        ''' <summary>
        ''' Gets or sets the confidence-interval method.
        ''' </summary>
        Public Property CiMethod As AgreementCiMethod = AgreementCiMethod.Analytical

        ''' <summary>
        ''' Gets or sets a value indicating whether analytical intervals should use the Student-t critical value rather than the normal critical value.
        ''' </summary>
        Public Property UseTDistribution As Boolean = True

        ''' <summary>
        ''' Gets or sets the number of bootstrap replicates when a bootstrap CI method is requested.
        ''' </summary>
        Public Property BootstrapReplicates As Integer = 2000

        ''' <summary>
        ''' Gets or sets optional subject identifiers.
        ''' </summary>
        ''' <remarks>
        ''' <para>
        ''' The array must be aligned row-by-row with the reference and test vectors.
        ''' </para>
        ''' <para>
        ''' When <c>Nothing</c>, the analysis uses ordinary Bland–Altman unless <see cref="Mode"/> explicitly requires repeated analysis.
        ''' </para>
        ''' </remarks>
        Public Property SubjectIds As Object() = Nothing

        ''' <summary>
        ''' Gets or sets a value indicating whether subjects with only one usable pair should be excluded from repeated-measures variance estimation.
        ''' </summary>
        Public Property ExcludeSingletonSubjects As Boolean = True

        ''' <summary>
        ''' Gets or sets the minimum number of distinct subjects required for repeated-measures Bland–Altman.
        ''' </summary>
        Public Property MinSubjects As Integer = 2

        ''' <summary>
        ''' Gets or sets the minimum number of usable pairs per subject required for a subject to contribute to repeated-measures variance estimation.
        ''' </summary>
        Public Property MinPairsPerSubject As Integer = 2

        ''' <summary>
        ''' Gets or sets a value indicating whether proportional bias should be checked.
        ''' </summary>
        Public Property CheckProportionalBias As Boolean = True

        ''' <summary>
        ''' Gets or sets how plot coordinates should be returned for repeated-measures output.
        ''' </summary>
        Public Property PlotMode As RepeatedBlandAltmanPlotMode = RepeatedBlandAltmanPlotMode.AllObservationsAndSubjectMeans

        ''' <summary>
        ''' Gets or sets a value indicating whether the class should silently fall back to ordinary Bland–Altman when repeated-measures requirements are not met.
        ''' </summary>
        ''' <remarks>
        ''' If <c>False</c>, the class should throw instead of silently downgrading the analysis.
        ''' </remarks>
        Public Property AllowFallbackToSimple As Boolean = True

    End Class

    Public Class LinConcordanceOptions
        Public Property Alpha As Double = 0.05
        Public Property CiMethod As AgreementCiMethod = AgreementCiMethod.Analytical
        Public Property BootstrapReplicates As Integer = 2000
        Public Property NullConcordance As Double = 0.0
        Public Property SubjectIds As Object() = Nothing   ' optional future extension
    End Class

    Public Class KappaOptions
        Public Property Alpha As Double = 0.05
        Public Property Weighting As KappaWeightingScheme = KappaWeightingScheme.Quadratic
        Public Property CustomWeights As Double(,) = Nothing
        Public Property CiMethod As AgreementCiMethod = AgreementCiMethod.Analytical
        Public Property BootstrapReplicates As Integer = 2000
        Public Property Categories As Object() = Nothing
    End Class

    Public Class DemingOptions
        Public Property Alpha As Double = 0.05
        Public Property CiMethod As AgreementCiMethod = AgreementCiMethod.Jackknife
        Public Property VarianceModel As DemingVarianceModel = DemingVarianceModel.ConstantLambda
        Public Property Lambda As Double = 1.0
        Public Property SDx As Double() = Nothing
        Public Property SDy As Double() = Nothing
        Public Property CVx As Double = Double.NaN
        Public Property CVy As Double = Double.NaN
        Public Property BootstrapReplicates As Integer = 2000
        Public Property FitIntercept As Boolean = True
    End Class

    Public Class MethodComparisonFitResult
        Public Property InterceptCI As ConfidenceIntervalResult
        Public Property SlopeCI As ConfidenceIntervalResult
        Public Property ResidualSD As Double
        Public Property MethodName As String
        Public Property Notes As String = String.Empty
    End Class



    ''' <summary>
    ''' Result object for ordinary or repeated-measures Bland–Altman analysis.
    ''' </summary>
    Public Class BlandAltmanResult

        ''' <summary> 
        ''' Gets a value indicating whether the last successful fit used repeated-measures Bland–Altman logic. 
        ''' </summary> 
        ''' <remarks> 
        ''' This property is meaningful only after <see cref="BlandAltmanAgreement.Fit"/> has been called. It is <c>False</c> for ordinary Bland–Altman fits. 
        ''' </remarks> 
        Public Property UsedRepeatedModel As Boolean = False

        ''' <summary>
        ''' Gets or sets the fitted mean difference (bias) and its confidence interval.
        ''' </summary>
        Public Property BiasCI As ConfidenceIntervalResult

        ''' <summary>
        ''' Gets or sets the lower limit of agreement and its confidence interval.
        ''' </summary>
        Public Property LowerLoACI As ConfidenceIntervalResult

        ''' <summary>
        ''' Gets or sets the upper limit of agreement and its confidence interval.
        ''' </summary>
        Public Property UpperLoACI As ConfidenceIntervalResult

        ''' <summary>
        ''' Gets or sets the sample standard deviation of the analyzed differences.
        ''' </summary>
        ''' <remarks>
        ''' For ordinary Bland–Altman this is the SD of all paired differences.
        ''' For repeated Bland–Altman this may be the within-subject SD proxy depending on the estimation method used.
        ''' </remarks>
        Public Property SdDifference As Double = Double.NaN

        ''' <summary> 
        ''' Gets the pooled within-subject standard deviation used in the last repeated-measures fit. 
        ''' </summary> 
        ''' <remarks> 
        ''' This property is meaningful only after <see cref="BlandAltmanAgreement.Fit"/> has been called. It returns <see cref="Double.NaN"/> for ordinary Bland–Altman fits. 
        ''' </remarks> 
        Public Property WithinSubjectSD As Double = Double.NaN

        ''' <summary>
        ''' Gets or sets the between-subject standard deviation of subject mean differences.
        ''' </summary>
        Public Property BetweenSubjectSD As Double = Double.NaN

        ''' <summary>
        ''' Gets or sets the repeatability coefficient.
        ''' </summary>
        Public Property RepeatabilityCoefficient As Double = Double.NaN

        ''' <summary>
        ''' Gets or sets the total number of valid paired observations used in the analysis.
        ''' </summary>
        Public Property ObservationCount As Integer = 0

        ''' <summary> 
        ''' Gets the number of subjects that contributed to repeated-measures variance estimation in the last fit. 
        ''' </summary> 
        ''' <remarks> 
        ''' This property is meaningful only after <see cref="BlandAltmanAgreement.Fit"/> has been called. It is 0 for ordinary Bland–Altman fits. 
        ''' </remarks> 
        Public Property SubjectCount As Integer = 0

        ''' <summary> 
        ''' Gets the number of subjects excluded from repeated-measures variance estimation in the last fit. 
        ''' </summary> 
        ''' <remarks> 
        ''' This property is meaningful only after <see cref="BlandAltmanAgreement.Fit"/> has been called. It is 0 for ordinary Bland–Altman fits. 
        ''' </remarks> 
        Public Property ExcludedSubjectCount As Integer = 0

        ''' <summary>
        ''' Gets or sets the full x-axis coordinates for the Bland–Altman scatter plot.
        ''' </summary>
        Public Property PlotX As Double() = Nothing

        ''' <summary>
        ''' Gets or sets the full y-axis coordinates for the Bland–Altman scatter plot.
        ''' </summary>
        Public Property PlotY As Double() = Nothing

        ''' <summary>
        ''' Gets or sets subject-level mean x-axis values.
        ''' </summary>
        ''' <remarks>
        ''' These are useful for repeated-measures plots that overlay subject means.
        ''' </remarks>
        Public Property SubjectMeanPlotX As Double() = Nothing

        ''' <summary>
        ''' Gets or sets subject-level mean difference values.
        ''' </summary>
        Public Property SubjectMeanPlotY As Double() = Nothing

        ''' <summary>
        ''' Gets or sets the ordered subject labels corresponding to the subject-level summaries.
        ''' </summary>
        Public Property SubjectLabels As Object() = Nothing

        ''' <summary>
        ''' Gets or sets the number of usable observations contributed by each subject.
        ''' </summary>
        Public Property SubjectObservationCounts As Integer() = Nothing

        ''' <summary>
        ''' Gets or sets the subject-level mean difference values.
        ''' </summary>
        Public Property SubjectMeanDifferences As Double() = Nothing

        ''' <summary>
        ''' Gets or sets the subject-level mean x-axis values used for summary plotting.
        ''' </summary>
        Public Property SubjectMeanXAxis As Double() = Nothing

        ''' <summary>
        ''' Gets or sets the proportional-bias test result.
        ''' </summary>
        Public Property ProportionalBias As TestResult = Nothing

        ''' <summary>
        ''' Gets or sets the method name for reporting.
        ''' </summary>
        Public Property MethodName As String = String.Empty

        ''' <summary>
        ''' Gets or sets free-form notes about the fit, fallbacks, exclusions, or CI behavior.
        ''' </summary>
        Public Property Notes As String = String.Empty

    End Class


    Public Class LinConcordanceResult
        Public Property ConcordanceCI As ConfidenceIntervalResult
        Public Property PearsonR As Double
        Public Property BiasCorrectionFactor As Double
        Public Property LocationShift As Double
        Public Property ScaleShift As Double
        Public Property Accuracy As Double
        Public Property Precision As Double
        Public Property HypothesisTest As TestResult
    End Class

    Public Class KappaResult
        Public Property KappaCI As ConfidenceIntervalResult
        Public Property ObservedAgreement As Double
        Public Property ExpectedAgreement As Double
        Public Property WeightedObservedAgreement As Double
        Public Property WeightedExpectedAgreement As Double
        Public Property HypothesisTest As TestResult
        Public Property Categories As Object()
        Public Property ConfusionMatrix As Double(,)
        Public Property WeightMatrix As Double(,)
    End Class

    Public Module AgreementHelpers

        Friend Function ExcludeIndex(values As Double(), indexToExclude As Integer) As Double()
            Dim out(values.Length - 2) As Double
            Dim t As Integer = 0
            For i As Integer = 0 To values.Length - 1
                If i = indexToExclude Then Continue For
                out(t) = values(i)
                t += 1
            Next
            Return out
        End Function

        ''' <summary>
        ''' Filters paired numeric inputs by removing any pair that contains a non-finite value.
        ''' </summary>
        ''' <param name="reference">Reference-method values.</param>
        ''' <param name="test">Test-method values.</param>
        ''' <returns>
        ''' A tuple containing:
        ''' <list type="bullet">
        '''   <item><description>filtered reference values</description></item>
        '''   <item><description>filtered test values</description></item>
        '''   <item><description>the number of dropped pairs</description></item>
        ''' </list>
        ''' </returns>
        ''' <remarks>
        ''' A pair is retained only when both values are finite.
        ''' </remarks>
        Friend Function FilterFinitePairs(reference As Double(),
                                          test As Double()) As (Reference As Double(), Test As Double(), DroppedCount As Integer)

            If reference Is Nothing Then CoreServices.Errors.LogAndThrow(New ArgumentNullException(NameOf(reference)))
            If test Is Nothing Then CoreServices.Errors.LogAndThrow(New ArgumentNullException(NameOf(test)))
            If reference.Length <> test.Length Then
                CoreServices.Errors.LogAndThrow(New ArgumentException("Reference and test arrays must have the same length."))
            End If

            Dim xr As New List(Of Double)(reference.Length)
            Dim yt As New List(Of Double)(test.Length)
            Dim dropped As Integer = 0

            For i As Integer = 0 To reference.Length - 1
                If IsFinite(reference(i)) AndAlso IsFinite(test(i)) Then
                    xr.Add(reference(i))
                    yt.Add(test(i))
                Else
                    dropped += 1
                End If
            Next

            Return (xr.ToArray(), yt.ToArray(), dropped)
        End Function

        ''' <summary>
        ''' Filters paired numeric inputs by removing any pair that contains a non-finite value and also returns the original kept row indices.
        ''' </summary>
        ''' <param name="reference">Reference-method values.</param>
        ''' <param name="test">Test-method values.</param>
        ''' <returns>
        ''' A tuple containing:
        ''' <list type="bullet">
        '''   <item><description>filtered reference values</description></item>
        '''   <item><description>filtered test values</description></item>
        '''   <item><description>indices of the original rows that were kept</description></item>
        '''   <item><description>the number of dropped pairs</description></item>
        ''' </list>
        ''' </returns>
        ''' <remarks>
        ''' This helper is useful when additional per-row arrays must be aligned to the filtered paired data.
        ''' </remarks>
        Friend Function FilterFinitePairsWithIndices(reference As Double(),
                                                     test As Double()) As (Reference As Double(), Test As Double(), KeptIndices As Integer(), DroppedCount As Integer)

            If reference Is Nothing Then CoreServices.Errors.LogAndThrow(New ArgumentNullException(NameOf(reference)))
            If test Is Nothing Then CoreServices.Errors.LogAndThrow(New ArgumentNullException(NameOf(test)))
            If reference.Length <> test.Length Then CoreServices.Errors.LogAndThrow(New ArgumentException("Reference and test arrays must have the same length."))

            Dim xr As New List(Of Double)(reference.Length)
            Dim yt As New List(Of Double)(test.Length)
            Dim kept As New List(Of Integer)(reference.Length)
            Dim dropped As Integer = 0

            For i As Integer = 0 To reference.Length - 1
                If IsFinite(reference(i)) AndAlso IsFinite(test(i)) Then
                    xr.Add(reference(i))
                    yt.Add(test(i))
                    kept.Add(i)
                Else
                    dropped += 1
                End If
            Next

            Return (xr.ToArray(), yt.ToArray(), kept.ToArray(), dropped)
        End Function

    End Module
End Namespace



