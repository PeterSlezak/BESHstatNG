Option Explicit On

Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports BESHStatNG.AppInfrastructure
Imports Microsoft.Office.Interop.Excel

Namespace Agreement

    ''' <summary>
    ''' Implements ordinary or repeated-measures Bland–Altman agreement analysis for two paired measurement methods.
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' When <see cref="BlandAltmanOptions.SubjectIds"/> is <c>Nothing</c>, the class performs the standard paired
    ''' Bland–Altman analysis.
    ''' </para>
    ''' <para>
    ''' When <see cref="BlandAltmanOptions.SubjectIds"/> is supplied, the class attempts a repeated-measures analysis
    ''' using the wide paired input format: one row per paired observation with aligned <c>SubjectID</c>,
    ''' <c>Reference</c>, and <c>Test</c> values.
    ''' </para>
    ''' <para>
    ''' In repeated-measures mode, the bias is still the overall mean of the transformed paired differences, while the
    ''' agreement limits are based on the pooled within-subject standard deviation of the differences:
    ''' </para>
    ''' <para>
    ''' <c>s_w² = ΣΣ(d_ij − d̄_i)² / Σ(n_i − 1)</c>
    ''' </para>
    ''' <para>
    ''' and the repeated-measures limits of agreement are reported as:
    ''' </para>
    ''' <para>
    ''' <c>bias ± 1.96 · s_w</c>
    ''' </para>
    ''' <para>
    ''' This provides a practical repeated-measures Bland–Altman implementation while preserving the existing
    ''' <see cref="BlandAltmanOptions"/> and <see cref="BlandAltmanResult"/> types already used by the project.
    ''' Additional repeated-measures details are exposed through the returned <see cref="BlandAltmanResult"/> object,
    ''' reporting tables, plotting fields, And the <see cref="BlandAltmanResult.Notes"/> field.
    ''' </para>
    ''' </remarks>
    Public Class BlandAltmanAgreement

        Private Const DefaultLoAMultiplier As Double = 1.96

        Private ReadOnly pVarX As String
        Private ReadOnly pVarY As String
        Private ReadOnly pReferenceData As Double()
        Private ReadOnly pTestData As Double()

        Private pOptions As BlandAltmanOptions
        Private pResult As BlandAltmanResult
        Private pIsFitted As Boolean = False
        Private pPlotXLabel As String = String.Empty
        Private pPlotYLabel As String = String.Empty
        Private pScaleNote As String = String.Empty
        Private pDroppedPairCount As Integer = 0

        Private pUsedRepeatedModel As Boolean = False
        Private pSubjectCount As Integer = 0
        Private pExcludedSubjectCount As Integer = 0
        Private pWithinSubjectSD As Double = Double.NaN
        Private pSubjectMeanPlotX As Double() = Nothing
        Private pSubjectMeanPlotY As Double() = Nothing
        Private pSubjectLabels As Object() = Nothing

        Private pUsedBootstrapCi As Boolean = False
        Private pBootstrapSeedUsed As Integer = Integer.MinValue

        ''' <summary>
        ''' Initializes a new Bland–Altman agreement-analysis object for two paired numeric variables.
        ''' </summary>
        ''' <param name="dataX">Numeric observations for the reference method.</param>
        ''' <param name="dataY">Numeric observations for the test method.</param>
        ''' <param name="varX">Display name for the reference method.</param>
        ''' <param name="varY">Display name for the test method.</param>
        ''' <param name="opts">Optional Bland–Altman configuration. If <c>Nothing</c>, a new <see cref="BlandAltmanOptions"/> instance is used.</param>
        Public Sub New(dataX As Double(),
                       dataY As Double(),
                       varX As String,
                       varY As String,
                       Optional opts As BlandAltmanOptions = Nothing)

            If dataX Is Nothing Then AppGlobals.BSerr.LogAndThrow(New ArgumentNullException(NameOf(dataX)))
            If dataY Is Nothing Then AppGlobals.BSerr.LogAndThrow(New ArgumentNullException(NameOf(dataY)))
            If dataX.Length <> dataY.Length Then
                AppGlobals.BSerr.LogAndThrow(New ArgumentException("Reference and test arrays must have the same length."))
            End If
            If dataX.Length < 3 Then
                AppGlobals.BSerr.LogAndThrow(New ArgumentException("At least 3 paired observations are required for Bland–Altman analysis."))
            End If

            Me.pReferenceData = CType(dataX.Clone(), Double())
            Me.pTestData = CType(dataY.Clone(), Double())
            Me.pVarX = If(String.IsNullOrWhiteSpace(varX), "Reference", varX)
            Me.pVarY = If(String.IsNullOrWhiteSpace(varY), "Test", varY)
            Me.pOptions = NormalizeOptions(opts)
        End Sub

        ''' <summary>
        ''' Gets or sets the options currently used by the agreement object.
        ''' </summary>
        Public Property Options As BlandAltmanOptions
            Get
                Return pOptions
            End Get
            Set(value As BlandAltmanOptions)
                pOptions = NormalizeOptions(value)
                ResetFitState()
            End Set
        End Property

        ''' <summary>
        ''' Gets a value indicating whether the last successful fit used repeated-measures Bland–Altman logic.
        ''' </summary>
        Public ReadOnly Property UsedRepeatedModel As Boolean
            Get
                Return pUsedRepeatedModel
            End Get
        End Property

        ''' <summary>
        ''' Gets the number of distinct subjects used in the last repeated-measures fit.
        ''' </summary>
        Public ReadOnly Property SubjectCount As Integer
            Get
                Return pSubjectCount
            End Get
        End Property

        ''' <summary>
        ''' Gets the number of subjects excluded from repeated-measures variance estimation in the last fit.
        ''' </summary>
        Public ReadOnly Property ExcludedSubjectCount As Integer
            Get
                Return pExcludedSubjectCount
            End Get
        End Property

        ''' <summary>
        ''' Gets the pooled within-subject standard deviation used in the last repeated-measures fit.
        ''' </summary>
        Public ReadOnly Property WithinSubjectSD As Double
            Get
                Return pWithinSubjectSD
            End Get
        End Property

        ''' <summary>
        ''' Gets the last computed Bland–Altman result.
        ''' </summary>
        Public ReadOnly Property Result As BlandAltmanResult
            Get
                Return pResult
            End Get
        End Property

        ''' <summary>
        ''' Fits ordinary or repeated-measures Bland–Altman analysis and returns the computed result object.
        ''' </summary>
        ''' <returns>
        ''' A <see cref="BlandAltmanResult"/> containing the bias estimate, limits of agreement, confidence intervals,
        ''' plotting coordinates, and an optional proportional-bias test result.
        ''' </returns>
        ''' <remarks>
        ''' <para>
        ''' The analysis mode is controlled by <see cref="BlandAltmanOptions.Mode"/>:
        ''' </para>
        ''' <list type="bullet">
        '''   <item><description><see cref="RepeatedBlandAltmanMode.SimplePairs"/> always uses ordinary Bland–Altman.</description></item>
        '''   <item><description><see cref="RepeatedBlandAltmanMode.RepeatedBySubject"/> requires repeated-measures analysis using subject IDs.</description></item>
        '''   <item><description><see cref="RepeatedBlandAltmanMode.Auto"/> uses repeated-measures analysis when subject IDs are supplied, otherwise ordinary Bland–Altman.</description></item>
        ''' </list>
        ''' <para>
        ''' If repeated-measures requirements are not met, the method either falls back to ordinary Bland–Altman or throws,
        ''' depending on <see cref="BlandAltmanOptions.AllowFallbackToSimple"/>.
        ''' </para>
        ''' <para>
        ''' When repeated-measures analysis is active and a bootstrap confidence-interval method is requested,
        ''' this implementation uses a clustered bootstrap that resamples whole subjects with replacement rather than
        ''' individual rows, preserving within-subject dependence.
        ''' </para>
        ''' </remarks>
        Public Function Fit(Optional randomSeed As Integer = Integer.MinValue) As BlandAltmanResult
            Dim opts As BlandAltmanOptions = NormalizeOptions(Me.Options)
            ValidateOptions(opts)
            ResetFitState()

            If opts.CiMethod = AgreementCiMethod.BootstrapPercentile OrElse opts.CiMethod = AgreementCiMethod.BootstrapBCa Then
                pUsedBootstrapCi = True
                pBootstrapSeedUsed = Helpers.ResolveRandomSeed(randomSeed)
            End If

            Dim requireSubjectIds As Boolean
            Select Case opts.Mode
                Case RepeatedBlandAltmanMode.RepeatedBySubject
                    requireSubjectIds = True
                Case RepeatedBlandAltmanMode.Auto
                    requireSubjectIds = (opts.SubjectIds IsNot Nothing)
                Case Else
                    requireSubjectIds = False
            End Select

            Dim filtered = FilterFinitePairsAndSubjects(pReferenceData, pTestData, opts.SubjectIds, requireSubjectIds)
            Dim x As Double() = filtered.Reference
            Dim y As Double() = filtered.Test
            Dim subjectIds As Object() = filtered.SubjectIds
            pDroppedPairCount = filtered.DroppedCount

            If x.Length < 3 Then
                AppGlobals.BSerr.LogAndThrow(New ArgumentException("Fewer than 3 complete finite pairs remain after filtering."))
            End If

            Dim transformed = TransformPairsForAnalysis(x, y, opts, pVarX, pVarY)
            pPlotXLabel = transformed.XAxisLabel
            pPlotYLabel = transformed.DifferenceLabel
            pScaleNote = transformed.ScaleNote

            Dim d As Double() = transformed.Differences
            Dim plotX As Double() = transformed.PlotX
            Dim n As Integer = d.Length
            Dim bias As Double = d.Average()
            Dim sdForLoA As Double
            Dim repeatedBetweenSubjectSD As Double = Double.NaN
            Dim repeatedSubjectMeanXAxis As Double() = Nothing
            Dim repeatedSubjectMeanDifferences As Double() = Nothing
            Dim repeatedSubjectObservationCounts As Integer() = Nothing

            Dim noteParts As New List(Of String)
            If Not String.IsNullOrWhiteSpace(pScaleNote) Then noteParts.Add(pScaleNote)
            noteParts.Add($"n = {n} complete finite pairs.")
            If pDroppedPairCount > 0 Then noteParts.Add($"Dropped {pDroppedPairCount} pair(s) with missing/non-finite values.")

            Dim requestedRepeated As Boolean
            Select Case opts.Mode
                Case RepeatedBlandAltmanMode.SimplePairs
                    requestedRepeated = False
                Case RepeatedBlandAltmanMode.RepeatedBySubject
                    requestedRepeated = True
                Case Else
                    requestedRepeated = (subjectIds IsNot Nothing)
            End Select

            If requestedRepeated Then
                If subjectIds Is Nothing Then
                    Dim msg As String = "Repeated Bland–Altman was requested, but no subject IDs were supplied."
                    If opts.AllowFallbackToSimple Then
                        sdForLoA = StatFunc.stDev(d)
                        noteParts.Add(msg & " Ordinary Bland–Altman was used.")
                    Else
                        AppGlobals.BSerr.LogAndThrow(New InvalidOperationException(msg))
                        Return Nothing
                    End If
                Else
                    Dim repeated = ComputeRepeatedStatistics(subjectIds, plotX, d, opts)

                    If repeated.UseRepeated Then
                        pUsedRepeatedModel = True
                        pSubjectCount = repeated.SubjectCount
                        pExcludedSubjectCount = repeated.ExcludedSubjectCount
                        pWithinSubjectSD = repeated.WithinSubjectSD
                        pSubjectMeanPlotX = repeated.SubjectMeanPlotX
                        pSubjectMeanPlotY = repeated.SubjectMeanPlotY
                        pSubjectLabels = repeated.SubjectLabels

                        repeatedBetweenSubjectSD = repeated.BetweenSubjectSD
                        repeatedSubjectMeanXAxis = repeated.SubjectMeanXAxis
                        repeatedSubjectMeanDifferences = repeated.SubjectMeanDifferences
                        repeatedSubjectObservationCounts = repeated.SubjectObservationCounts

                        sdForLoA = repeated.WithinSubjectSD
                        noteParts.Add($"Repeated-measures Bland–Altman used with {pSubjectCount} subject(s).")
                        If pExcludedSubjectCount > 0 Then noteParts.Add($"Excluded {pExcludedSubjectCount} subject(s) from within-subject SD estimation.")
                        noteParts.Add($"Within-subject SD(diff) = {Math.Round(pWithinSubjectSD, 6)}.")
                        If Not String.IsNullOrWhiteSpace(repeated.Note) Then noteParts.Add(repeated.Note)

                    ElseIf opts.AllowFallbackToSimple Then
                        sdForLoA = StatFunc.stDev(d)
                        If Not String.IsNullOrWhiteSpace(repeated.Note) Then
                            noteParts.Add(repeated.Note)
                        Else
                            noteParts.Add("Repeated-measures requirements were not met; ordinary Bland–Altman was used.")
                        End If

                    Else
                        Dim msg As String = If(String.IsNullOrWhiteSpace(repeated.Note),
                               "Repeated-measures requirements were not met.",
                               repeated.Note)
                        AppGlobals.BSerr.LogAndThrow(New InvalidOperationException(msg))
                        Return Nothing
                    End If
                End If
            Else
                sdForLoA = StatFunc.stDev(d)
                If subjectIds IsNot Nothing AndAlso opts.Mode = RepeatedBlandAltmanMode.SimplePairs Then
                    noteParts.Add("Subject IDs were supplied, but the analysis mode was explicitly set to SimplePairs, so ordinary Bland–Altman was used.")
                End If
            End If

            Dim biasCI As ConfidenceIntervalResult
            Dim loaCI As (Lower As ConfidenceIntervalResult, Upper As ConfidenceIntervalResult)

            If pUsedRepeatedModel Then
                Select Case opts.CiMethod
                    Case AgreementCiMethod.BootstrapPercentile, AgreementCiMethod.BootstrapBCa
                        Dim clusteredBoot = ComputeRepeatedBootstrapConfidenceIntervals(subjectIds, plotX, d, bias, pWithinSubjectSD, opts, pBootstrapSeedUsed)
                        biasCI = clusteredBoot.BiasCI
                        loaCI = (clusteredBoot.LowerLoACI, clusteredBoot.UpperLoACI)
                        If opts.CiMethod = AgreementCiMethod.BootstrapBCa Then
                            noteParts.Add("Repeated-measures BCa bootstrap is not yet implemented separately; clustered percentile bootstrap limits were used.")
                        Else
                            noteParts.Add("Repeated-measures bootstrap CIs use clustered resampling at the subject level.")
                        End If
                    Case AgreementCiMethod.Jackknife
                        Dim clusteredJack = ComputeRepeatedJackknifeConfidenceIntervals(subjectIds, plotX, d, bias, pWithinSubjectSD, opts)
                        biasCI = clusteredJack.BiasCI
                        loaCI = (clusteredJack.LowerLoACI, clusteredJack.UpperLoACI)
                        noteParts.Add("Repeated-measures jackknife CIs use leave-one-subject-out resampling.")
                    Case Else
                        biasCI = ComputeBiasConfidenceInterval(d, bias, sdForLoA, opts, pBootstrapSeedUsed)
                        loaCI = ComputeLimitsOfAgreementConfidenceIntervals(n, bias, sdForLoA, opts, pBootstrapSeedUsed)
                End Select
            Else
                If opts.CiMethod = AgreementCiMethod.Jackknife Then
                    Dim jack = ComputeJackknifeConfidenceIntervalsSimple(d, bias, sdForLoA, opts)
                    biasCI = jack.BiasCI
                    loaCI = (jack.LowerLoACI, jack.UpperLoACI)
                    noteParts.Add("Jackknife CIs use leave-one-pair-out resampling.")
                Else
                    biasCI = ComputeBiasConfidenceInterval(d, bias, sdForLoA, opts, pBootstrapSeedUsed)
                    loaCI = ComputeLimitsOfAgreementConfidenceIntervals(n, bias, sdForLoA, opts, pBootstrapSeedUsed)
                    If pUsedBootstrapCi AndAlso opts.CiMethod = AgreementCiMethod.BootstrapBCa Then
                        noteParts.Add("BCa bootstrap is not yet implemented separately; percentile bootstrap limits were used.")
                    End If
                End If
            End If

            If pUsedBootstrapCi Then noteParts.Add($"Bootstrap seed = {pBootstrapSeedUsed}.")

            Dim trend As TestResult = Nothing
            If opts.CheckProportionalBias Then trend = ComputeProportionalBiasTrend(plotX, d)

            pResult = New BlandAltmanResult With {
                .UsedRepeatedModel = pUsedRepeatedModel,
                .BiasCI = biasCI,
                .LowerLoACI = loaCI.Lower,
                .UpperLoACI = loaCI.Upper,
                .SdDifference = sdForLoA,
                .WithinSubjectSD = If(pUsedRepeatedModel, pWithinSubjectSD, Double.NaN),
                .BetweenSubjectSD = repeatedBetweenSubjectSD,
                .RepeatabilityCoefficient = DefaultLoAMultiplier * sdForLoA,
                .ObservationCount = n,
                .SubjectCount = If(pUsedRepeatedModel, pSubjectCount, 0),
                .ExcludedSubjectCount = If(pUsedRepeatedModel, pExcludedSubjectCount, 0),
                .PlotX = CType(plotX.Clone(), Double()),
                .PlotY = CType(d.Clone(), Double()),
                .SubjectMeanPlotX = If(pSubjectMeanPlotX Is Nothing, Nothing, CType(pSubjectMeanPlotX.Clone(), Double())),
                .SubjectMeanPlotY = If(pSubjectMeanPlotY Is Nothing, Nothing, CType(pSubjectMeanPlotY.Clone(), Double())),
                .SubjectLabels = If(pSubjectLabels Is Nothing, Nothing, CType(pSubjectLabels.Clone(), Object())),
                .SubjectObservationCounts = If(repeatedSubjectObservationCounts Is Nothing, Nothing, CType(repeatedSubjectObservationCounts.Clone(), Integer())),
                .SubjectMeanDifferences = If(repeatedSubjectMeanDifferences Is Nothing, Nothing, CType(repeatedSubjectMeanDifferences.Clone(), Double())),
                .SubjectMeanXAxis = If(repeatedSubjectMeanXAxis Is Nothing, Nothing, CType(repeatedSubjectMeanXAxis.Clone(), Double())),
                .ProportionalBias = trend,
                .MethodName = If(pUsedRepeatedModel, "Repeated-measures Bland–Altman agreement analysis", "Bland–Altman agreement analysis"),
                .Notes = String.Join(" ", noteParts.Where(Function(s) Not String.IsNullOrWhiteSpace(s)))
            }

            pIsFitted = True
            Return pResult
        End Function


        ''' <summary>
        ''' Wraps the fitted Bland–Altman output into report-ready <see cref="ResultTable"/> instances.
        ''' </summary>
        Public Function wrapResults() As List(Of ResultTable)
            If Not pIsFitted OrElse pResult Is Nothing Then Fit()

            Dim out As New List(Of ResultTable)()
            Dim ciLabel As String = If(pResult.BiasCI Is Nothing, "Confidence Interval", pResult.BiasCI.CIlabel)

            Dim tSummary As New ResultTable()
            tSummary.AddTitle("Method Comparison Summary")
            tSummary.SetBody(New Object(,) {{"Reference method", pVarX},
                                    {"Test method", pVarY},
                                    {"Complete finite pairs", CStr(pResult.ObservationCount)},
                                    {"Dropped non-finite pairs", CStr(pDroppedPairCount)},
                                    {"Requested mode", Me.Options.Mode.ToString()},
                                    {"Model used", If(pResult.UsedRepeatedModel, "Repeated Bland–Altman", "Simple Bland–Altman")},
                                    {"Scale", GetScaleDisplayText(Me.Options.Scale)},
                                    {"X-axis", GetXAxisDisplayText(Me.Options.XAxisMode)},
                                    {"CI method", GetCiMethodDisplayText(Me.Options.CiMethod)},
                                    {"Plot mode", Me.Options.PlotMode.ToString()}})

            If Not String.IsNullOrWhiteSpace(pScaleNote) Then tSummary.AddFootnote(pScaleNote)
            out.Add(tSummary)

            Dim tAgreement As New ResultTable()
            tAgreement.AddTitle("Bland–Altman Agreement")
            tAgreement.AddHeaderTopRow({"Estimate", ciLabel, "Interpretation"})
            tAgreement.AddHeaderLeftRow({"Bias", "Lower LoA", "Upper LoA", "SD(diff)", "Repeatability coefficient"})
            tAgreement.SetBody(New Object(,) {
                       {pResult.BiasCI.Estimate, pResult.BiasCI.strConfidenceInterval(CIformat.LL_to_UL), "Average signed difference (test minus reference)."},
                       {pResult.LowerLoACI.Estimate, pResult.LowerLoACI.strConfidenceInterval(CIformat.LL_to_UL), "Lower limit of agreement."},
                       {pResult.UpperLoACI.Estimate, pResult.UpperLoACI.strConfidenceInterval(CIformat.LL_to_UL), "Upper limit of agreement."},
                       {pResult.SdDifference, "", If(pResult.UsedRepeatedModel, "Pooled within-subject SD of transformed paired differences.", "Sample SD of transformed paired differences.")},
                       {pResult.RepeatabilityCoefficient, "", "Half-width of the classical LoA band (1.96 × SD(diff))."}})

            If Not String.IsNullOrWhiteSpace(pResult.Notes) Then tAgreement.AddFootnote(pResult.Notes)
            out.Add(tAgreement)

            If pResult.UsedRepeatedModel Then
                Dim tRepeated As New ResultTable()
                tRepeated.AddTitle("Repeated-Measurements Summary")
                tRepeated.SetBody(New Object(,) {
                                    {"Subjects used for repeated model", pResult.SubjectCount},
                                    {"Subjects excluded", pResult.ExcludedSubjectCount},
                                    {"Within-subject SD(diff)", pResult.WithinSubjectSD},
                                    {"Between-subject SD(mean diff)", pResult.BetweenSubjectSD},
                                    {"Subject means available for plot", If(pResult.SubjectMeanPlotX Is Nothing, 0, pResult.SubjectMeanPlotX.Length)}})
                out.Add(tRepeated)

                If pResult.SubjectMeanDifferences IsNot Nothing AndAlso pResult.SubjectMeanDifferences.Length > 0 Then
                    Dim tSubjects As New ResultTable()
                    tSubjects.AddTitle("Subject-Level Summary")
                    tSubjects.AddHeaderTopRow({"Subject", "Pairs", "Subject mean X", "Subject mean diff"})
                    Dim n As Integer = pResult.SubjectMeanDifferences.Length
                    Dim body(n - 1, 3) As Object
                    For i As Integer = 0 To n - 1
                        body(i, 0) = pResult.SubjectLabels(i)
                        body(i, 1) = pResult.SubjectObservationCounts(i)
                        body(i, 2) = pResult.SubjectMeanXAxis(i)
                        body(i, 3) = pResult.SubjectMeanDifferences(i)
                    Next
                    tSubjects.SetBody(body)
                    out.Add(tSubjects)
                End If
            End If

            If pResult.ProportionalBias IsNot Nothing Then
                Dim tTrend As New ResultTable()
                tTrend.AddTitle("Proportional Bias Check")
                tTrend.AddHeaderTopRow({"Value", "Meaning"})
                tTrend.AddHeaderLeftRow({"Slope", "t statistic", "df", "Two-sided p-value"})
                tTrend.SetBody(New Object(,) {
                       {pResult.ProportionalBias.TestStatistics1, "Slope from regression of differences on the selected x-axis quantity."},
                       {pResult.ProportionalBias.TestStatistics2, "Student-t statistic for the slope."},
                       {pResult.ProportionalBias.DF1, "Residual degrees of freedom for the trend model."},
                       {pResult.ProportionalBias.Pvalue, "Two-sided p-value for testing slope = 0."}})
                If Not String.IsNullOrWhiteSpace(pResult.ProportionalBias.strSpecialInformation) Then
                    tTrend.AddFootnote(pResult.ProportionalBias.strSpecialInformation)
                End If
                out.Add(tTrend)
            End If

            Return out
        End Function

        ''' <summary> 
        ''' Adds a Bland–Altman plot to an Excel worksheet. 
        ''' </summary> 
        ''' <param name="ws">Target worksheet where the chart will be created.</param> 
        ''' <remarks> 
        ''' <para> 
        ''' The plot always includes the bias, lower limit of agreement, and upper limit of agreement reference lines. 
        ''' </para> 
        ''' <para> 
        ''' In repeated-measures mode, the displayed points are controlled by <see cref="RepeatedBlandAltmanPlotMode"/>: 
        ''' </para> 
        ''' <list type="bullet"> 
        ''' <item><description><see cref="RepeatedBlandAltmanPlotMode.AllObservations"/> shows all observations only</description></item> 
        ''' <item><description><see cref="RepeatedBlandAltmanPlotMode.SubjectMeansOnly"/> shows subject means only</description></item> 
        ''' <item><description><see cref="RepeatedBlandAltmanPlotMode.AllObservationsAndSubjectMeans"/> shows both</description></item> 
        ''' </list> 
        ''' </remarks> 
        Public Sub AddPlot(ws As Worksheet)
            If ws Is Nothing Then AppGlobals.BSerr.LogAndThrow(New ArgumentNullException(NameOf(ws)))
            If Not pIsFitted OrElse pResult Is Nothing Then Fit()

            Dim mainX As Double() = pResult.PlotX
            Dim mainY As Double() = pResult.PlotY
            Dim chartTitle As String = pResult.MethodName

            If pResult.UsedRepeatedModel AndAlso Me.Options.PlotMode = RepeatedBlandAltmanPlotMode.SubjectMeansOnly AndAlso pResult.SubjectMeanPlotX IsNot Nothing AndAlso pResult.SubjectMeanPlotY IsNot Nothing AndAlso pResult.SubjectMeanPlotX.Length > 0 Then
                mainX = pResult.SubjectMeanPlotX
                mainY = pResult.SubjectMeanPlotY
                chartTitle &= " (subject means)"
            End If

            Dim ch As Chart = graphics.GeneralScatterPlot(mainX, mainY, pPlotYLabel, pPlotXLabel, ws, chartTitle)
            Dim xMin As Double = mainX.Min()
            Dim xMax As Double = mainX.Max()

            If xMin = xMax Then
                xMin -= 0.5
                xMax += 0.5
            End If

            Dim lineX As Double() = {xMin, xMax}
            AddHorizontalReferenceLine(ch, lineX, pResult.BiasCI.Estimate, "Bias", RGB(31, 119, 180))
            AddHorizontalReferenceLine(ch, lineX, pResult.LowerLoACI.Estimate, "Lower LoA", RGB(214, 39, 40))
            AddHorizontalReferenceLine(ch, lineX, pResult.UpperLoACI.Estimate, "Upper LoA", RGB(214, 39, 40))

            If pResult.UsedRepeatedModel AndAlso Me.Options.PlotMode = RepeatedBlandAltmanPlotMode.AllObservationsAndSubjectMeans AndAlso pResult.SubjectMeanPlotX IsNot Nothing AndAlso pResult.SubjectMeanPlotY IsNot Nothing AndAlso pResult.SubjectMeanPlotX.Length > 0 Then
                ch.SeriesCollection.NewSeries()
                With ch.SeriesCollection(ch.SeriesCollection.Count)
                    .XValues = pResult.SubjectMeanPlotX
                    .Values = pResult.SubjectMeanPlotY
                    .Name = "Subject means"
                    .MarkerStyle = XlMarkerStyle.xlMarkerStyleDiamond
                    .MarkerSize = 7
                    .Format.Line.Visible = False
                End With
            End If
        End Sub

        ''' <summary>
        ''' Returns a normalized options instance, creating a new default object when needed.
        ''' </summary>
        Friend Shared Function NormalizeOptions(opts As BlandAltmanOptions) As BlandAltmanOptions
            If opts Is Nothing Then Return New BlandAltmanOptions()
            Return opts
        End Function

        ''' <summary>
        ''' Validates a <see cref="BlandAltmanOptions"/> object.
        ''' </summary>
        Friend Shared Sub ValidateOptions(opts As BlandAltmanOptions)
            If opts Is Nothing Then AppGlobals.BSerr.LogAndThrow(New ArgumentNullException(NameOf(opts)))
            If opts.Alpha <= 0.0 OrElse opts.Alpha >= 1.0 OrElse Double.IsNaN(opts.Alpha) Then
                AppGlobals.BSerr.LogAndThrow(New ArgumentOutOfRangeException(NameOf(opts.Alpha), "Alpha must be in the open interval (0, 1)."))
            End If
            If (opts.CiMethod = AgreementCiMethod.BootstrapPercentile OrElse opts.CiMethod = AgreementCiMethod.BootstrapBCa) AndAlso opts.BootstrapReplicates < 200 Then
                AppGlobals.BSerr.LogAndThrow(New ArgumentOutOfRangeException(NameOf(opts.BootstrapReplicates), "At least 200 bootstrap replicates are recommended for bootstrap confidence intervals."))
            End If
            If opts.SubjectIds IsNot Nothing AndAlso opts.SubjectIds.Length = 0 Then
                opts.SubjectIds = Nothing
            End If
        End Sub

        ''' <summary>
        ''' Filters paired observations by removing any pair containing a non-finite value and, when repeated analysis is actually being attempted, rows with missing subject identifiers.
        ''' </summary>
        ''' <param name="reference">Reference-method observations.</param>
        ''' <param name="test">Test-method observations.</param>
        ''' <param name="subjectIds">Optional subject identifiers aligned with the paired observations.</param>
        ''' <param name="requireSubjectIds">
        ''' If <c>True</c>, rows with missing subject IDs are removed; if <c>False</c>, subject IDs are passed through without affecting ordinary Bland–Altman row retention.
        ''' </param>
        Friend Shared Function FilterFinitePairsAndSubjects(reference As Double(),
                                                    test As Double(),
                                                    subjectIds As Object(),
                                                    requireSubjectIds As Boolean) As (Reference As Double(), Test As Double(), SubjectIds As Object(), DroppedCount As Integer)
            If reference Is Nothing Then AppGlobals.BSerr.LogAndThrow(New ArgumentNullException(NameOf(reference)))
            If test Is Nothing Then AppGlobals.BSerr.LogAndThrow(New ArgumentNullException(NameOf(test)))
            If reference.Length <> test.Length Then
                AppGlobals.BSerr.LogAndThrow(New ArgumentException("Reference and test arrays must have the same length."))
            End If
            If subjectIds IsNot Nothing AndAlso subjectIds.Length <> reference.Length Then
                AppGlobals.BSerr.LogAndThrow(New ArgumentException("SubjectIds must have the same length as the paired measurements."))
            End If

            Dim xr As New List(Of Double)(reference.Length)
            Dim yt As New List(Of Double)(test.Length)
            Dim sid As List(Of Object) = If(subjectIds Is Nothing, Nothing, New List(Of Object)(reference.Length))
            Dim dropped As Integer = 0

            For i As Integer = 0 To reference.Length - 1
                Dim keep As Boolean = IsFinite(reference(i)) AndAlso IsFinite(test(i))
                If keep AndAlso requireSubjectIds Then
                    keep = (subjectIds IsNot Nothing AndAlso Not IsMissingSubjectId(subjectIds(i)))
                End If

                If keep Then
                    xr.Add(reference(i))
                    yt.Add(test(i))
                    If sid IsNot Nothing Then
                        sid.Add(If(IsMissingSubjectId(subjectIds(i)), Nothing, NormalizeSubjectId(subjectIds(i))))
                    End If
                Else
                    dropped += 1
                End If
            Next

            Return (xr.ToArray(), yt.ToArray(), If(sid Is Nothing, Nothing, sid.ToArray()), dropped)
        End Function

        ''' <summary>
        ''' Converts the paired measurements into the Bland–Altman analysis scale and plotting coordinates.
        ''' </summary>
        Friend Shared Function TransformPairsForAnalysis(reference As Double(),
                                                         test As Double(),
                                                         opts As BlandAltmanOptions,
                                                         referenceName As String,
                                                         testName As String) As (Differences As Double(), PlotX As Double(), XAxisLabel As String, DifferenceLabel As String, ScaleNote As String)
            If reference Is Nothing Then AppGlobals.BSerr.LogAndThrow(New ArgumentNullException(NameOf(reference)))
            If test Is Nothing Then AppGlobals.BSerr.LogAndThrow(New ArgumentNullException(NameOf(test)))
            If opts Is Nothing Then AppGlobals.BSerr.LogAndThrow(New ArgumentNullException(NameOf(opts)))
            If reference.Length <> test.Length Then
                AppGlobals.BSerr.LogAndThrow(New ArgumentException("Reference and test arrays must have the same length."))
            End If

            Dim n As Integer = reference.Length
            Dim d(n - 1) As Double
            Dim plotX(n - 1) As Double
            Dim xLabel As String = GetXAxisLabel(referenceName, testName, opts.XAxisMode)
            Dim yLabel As String = String.Empty
            Dim scaleNote As String = String.Empty

            For i As Integer = 0 To n - 1
                Dim x As Double = reference(i)
                Dim y As Double = test(i)
                plotX(i) = ComputeXAxisValue(x, y, opts.XAxisMode)

                Select Case opts.Scale
                    Case BlandAltmanScale.RawDifference
                        d(i) = y - x
                        yLabel = $"Difference ({testName} − {referenceName})"
                        scaleNote = $"Differences are analysed on the raw scale as {testName} − {referenceName}."

                    Case BlandAltmanScale.PercentOfMean
                        Dim denom As Double = 0.5 * (x + y)
                        If denom = 0.0 Then AppGlobals.BSerr.LogAndThrow(New InvalidOperationException("Percent-of-mean Bland–Altman analysis is undefined when the paired mean equals zero."))
                        d(i) = 100.0 * (y - x) / denom
                        yLabel = $"Difference (%) relative to mean({referenceName},{testName})"
                        scaleNote = "Differences are expressed as 100 × (test − reference) / paired mean."

                    Case BlandAltmanScale.PercentOfReference
                        If x = 0.0 Then AppGlobals.BSerr.LogAndThrow(New InvalidOperationException("Percent-of-reference Bland–Altman analysis is undefined when the reference value equals zero."))
                        d(i) = 100.0 * (y - x) / x
                        yLabel = $"Difference (%) relative to {referenceName}"
                        scaleNote = $"Differences are expressed as 100 × ({testName} − {referenceName}) / {referenceName}."

                    Case BlandAltmanScale.PercentOfTest
                        If y = 0.0 Then AppGlobals.BSerr.LogAndThrow(New InvalidOperationException("Percent-of-test Bland–Altman analysis is undefined when the test value equals zero."))
                        d(i) = 100.0 * (y - x) / y
                        yLabel = $"Difference (%) relative to {testName}"
                        scaleNote = $"Differences are expressed as 100 × ({testName} − {referenceName}) / {testName}."

                    Case BlandAltmanScale.LogRatio
                        If x <= 0.0 OrElse y <= 0.0 Then AppGlobals.BSerr.LogAndThrow(New InvalidOperationException("Log-ratio Bland–Altman analysis requires strictly positive paired values."))
                        d(i) = Math.Log(y / x)
                        yLabel = $"Log ratio ln({testName}/{referenceName})"
                        scaleNote = "Differences are analysed on the natural-log ratio scale. Exponentiation converts estimates back to ratio form."

                    Case Else
                        AppGlobals.BSerr.LogAndThrow(New ArgumentOutOfRangeException(NameOf(opts.Scale), "Unsupported Bland–Altman scale."))
                End Select
            Next

            Return (d, plotX, xLabel, yLabel, scaleNote)
        End Function

        ''' <summary>
        ''' Computes the confidence interval for the Bland–Altman bias estimate.
        ''' </summary>
        Friend Shared Function ComputeBiasConfidenceInterval(differences As Double(),
                                                     bias As Double,
                                                     sdDiff As Double,
                                                     opts As BlandAltmanOptions,
                                                     Optional randomSeed As Integer = Integer.MinValue) As ConfidenceIntervalResult
            Dim n As Integer = differences.Length
            Dim se As Double = sdDiff / Math.Sqrt(n)

            If opts.CiMethod = AgreementCiMethod.Analytical Then
                Dim crit As Double = If(opts.UseTDistribution,
                                distributions.T_Inv(1.0 - opts.Alpha / 2.0, n - 1),
                                distributions.NormSInv(1.0 - opts.Alpha / 2.0))
                Return New ConfidenceIntervalResult With {
                        .Estimate = bias,
                        .alpha = opts.Alpha,
                        .StdErr = se,
                        .LowerLimit = bias - crit * se,
                        .UpperLimit = bias + crit * se
                    }
            End If

            Return BootstrapBiasConfidenceInterval(differences, opts, randomSeed)
        End Function

        ''' <summary>
        ''' Computes approximate confidence intervals for the lower and upper Bland–Altman limits of agreement.
        ''' </summary>
        Friend Shared Function ComputeLimitsOfAgreementConfidenceIntervals(n As Integer,
                                                                   bias As Double,
                                                                   sdDiff As Double,
                                                                   opts As BlandAltmanOptions,
                                                                   Optional randomSeed As Integer = Integer.MinValue) As (Lower As ConfidenceIntervalResult, Upper As ConfidenceIntervalResult)
            Dim lowerEstimate As Double = bias - DefaultLoAMultiplier * sdDiff
            Dim upperEstimate As Double = bias + DefaultLoAMultiplier * sdDiff

            If opts.CiMethod = AgreementCiMethod.Analytical Then
                Dim seLoA As Double = sdDiff * Math.Sqrt((1.0 / n) + ((DefaultLoAMultiplier * DefaultLoAMultiplier) / (2.0 * Math.Max(1.0, n - 1.0))))
                Dim crit As Double = If(opts.UseTDistribution,
                                distributions.T_Inv(1.0 - opts.Alpha / 2.0, Math.Max(1, n - 1)),
                                distributions.NormSInv(1.0 - opts.Alpha / 2.0))

                Dim lower As New ConfidenceIntervalResult With {
                        .Estimate = lowerEstimate,
                        .alpha = opts.Alpha,
                        .StdErr = seLoA,
                        .LowerLimit = lowerEstimate - crit * seLoA,
                        .UpperLimit = lowerEstimate + crit * seLoA
                    }

                Dim upper As New ConfidenceIntervalResult With {
                        .Estimate = upperEstimate,
                        .alpha = opts.Alpha,
                        .StdErr = seLoA,
                        .LowerLimit = upperEstimate - crit * seLoA,
                        .UpperLimit = upperEstimate + crit * seLoA
                    }

                Return (lower, upper)
            End If

            Return BootstrapLoAConfidenceIntervals(n, bias, sdDiff, opts, randomSeed)
        End Function

        ''' <summary>
        ''' Fits a simple linear trend model of paired differences on the selected x-axis quantity.
        ''' </summary>

        Friend Shared Function ComputeJackknifeConfidenceIntervalsSimple(differences As Double(),
                                                                          observedBias As Double,
                                                                          observedSdDiff As Double,
                                                                          opts As BlandAltmanOptions) As (BiasCI As ConfidenceIntervalResult,
                                                                                                          LowerLoACI As ConfidenceIntervalResult,
                                                                                                          UpperLoACI As ConfidenceIntervalResult)
            If differences Is Nothing Then AppGlobals.BSerr.LogAndThrow(New ArgumentNullException(NameOf(differences)))
            If opts Is Nothing Then AppGlobals.BSerr.LogAndThrow(New ArgumentNullException(NameOf(opts)))
            If differences.Length < 3 Then
                AppGlobals.BSerr.LogAndThrow(New ArgumentException("At least 3 paired observations are required for jackknife Bland–Altman confidence intervals."))
            End If

            Dim n As Integer = differences.Length
            Dim biasJK(n - 1) As Double
            Dim lowerJK(n - 1) As Double
            Dim upperJK(n - 1) As Double

            For i As Integer = 0 To n - 1
                Dim sample As Double() = AgreementHelpers.ExcludeIndex(differences, i)
                Dim meanB As Double = sample.Average()
                Dim sdB As Double = StatFunc.stDev(sample)
                biasJK(i) = meanB
                lowerJK(i) = meanB - DefaultLoAMultiplier * sdB
                upperJK(i) = meanB + DefaultLoAMultiplier * sdB
            Next

            Dim biasCI = BuildJackknifeConfidenceInterval(observedBias, biasJK, opts.Alpha, opts.UseTDistribution)
            Dim lowerObserved As Double = observedBias - DefaultLoAMultiplier * observedSdDiff
            Dim upperObserved As Double = observedBias + DefaultLoAMultiplier * observedSdDiff
            Dim lowerCI = BuildJackknifeConfidenceInterval(lowerObserved, lowerJK, opts.Alpha, opts.UseTDistribution)
            Dim upperCI = BuildJackknifeConfidenceInterval(upperObserved, upperJK, opts.Alpha, opts.UseTDistribution)
            Return (biasCI, lowerCI, upperCI)
        End Function

        Private Shared Function ComputeRepeatedJackknifeConfidenceIntervals(subjectIds As Object(),
                                                                            plotX As Double(),
                                                                            differences As Double(),
                                                                            observedBias As Double,
                                                                            observedWithinSubjectSD As Double,
                                                                            opts As BlandAltmanOptions) As (BiasCI As ConfidenceIntervalResult,
                                                                                                            LowerLoACI As ConfidenceIntervalResult,
                                                                                                            UpperLoACI As ConfidenceIntervalResult)
            If subjectIds Is Nothing Then AppGlobals.BSerr.LogAndThrow(New ArgumentNullException(NameOf(subjectIds)))
            If plotX Is Nothing Then AppGlobals.BSerr.LogAndThrow(New ArgumentNullException(NameOf(plotX)))
            If differences Is Nothing Then AppGlobals.BSerr.LogAndThrow(New ArgumentNullException(NameOf(differences)))
            If opts Is Nothing Then AppGlobals.BSerr.LogAndThrow(New ArgumentNullException(NameOf(opts)))
            If subjectIds.Length <> plotX.Length OrElse subjectIds.Length <> differences.Length Then
                AppGlobals.BSerr.LogAndThrow(New ArgumentException("subjectIds, plotX, and differences must have the same length."))
            End If

            Dim grouped As Dictionary(Of String, List(Of Integer)) = BuildSubjectIndexGroups(subjectIds)
            If grouped.Count < 2 Then
                AppGlobals.BSerr.LogAndThrow(New InvalidOperationException("At least two subjects are required for repeated-measures jackknife Bland–Altman confidence intervals."))
            End If

            Dim jackBias As New List(Of Double)(grouped.Count)
            Dim jackLower As New List(Of Double)(grouped.Count)
            Dim jackUpper As New List(Of Double)(grouped.Count)

            Dim jkOpts As BlandAltmanOptions = CloneOptions(opts)
            jkOpts.MinSubjects = 1

            For Each key As String In grouped.Keys
                Dim reduced = ExcludeSubject(subjectIds, plotX, differences, grouped(key))
                If reduced.SubjectIds.Length < 2 Then Continue For

                Dim repeated = ComputeRepeatedStatistics(reduced.SubjectIds, reduced.PlotX, reduced.Differences, jkOpts)
                If repeated.UseRepeated AndAlso Not Double.IsNaN(repeated.WithinSubjectSD) AndAlso repeated.WithinSubjectSD > 0.0 Then
                    Dim biasJ As Double = reduced.Differences.Average()
                    Dim lowerJ As Double = biasJ - DefaultLoAMultiplier * repeated.WithinSubjectSD
                    Dim upperJ As Double = biasJ + DefaultLoAMultiplier * repeated.WithinSubjectSD
                    jackBias.Add(biasJ)
                    jackLower.Add(lowerJ)
                    jackUpper.Add(upperJ)
                End If
            Next

            If jackBias.Count < 2 Then
                AppGlobals.BSerr.LogAndThrow(New InvalidOperationException("Too few successful leave-one-subject-out replicates were obtained for repeated-measures jackknife Bland–Altman confidence intervals."))
            End If

            Dim biasCI = BuildJackknifeConfidenceInterval(observedBias, jackBias.ToArray(), opts.Alpha, opts.UseTDistribution)
            Dim lowerObserved As Double = observedBias - DefaultLoAMultiplier * observedWithinSubjectSD
            Dim upperObserved As Double = observedBias + DefaultLoAMultiplier * observedWithinSubjectSD
            Dim lowerCI = BuildJackknifeConfidenceInterval(lowerObserved, jackLower.ToArray(), opts.Alpha, opts.UseTDistribution)
            Dim upperCI = BuildJackknifeConfidenceInterval(upperObserved, jackUpper.ToArray(), opts.Alpha, opts.UseTDistribution)
            Return (biasCI, lowerCI, upperCI)
        End Function

        Private Shared Function BuildJackknifeConfidenceInterval(observedEstimate As Double,
                                                                 leaveOneOutEstimates As Double(),
                                                                 alpha As Double,
                                                                 useTDistribution As Boolean) As ConfidenceIntervalResult
            If leaveOneOutEstimates Is Nothing Then AppGlobals.BSerr.LogAndThrow(New ArgumentNullException(NameOf(leaveOneOutEstimates)))
            If leaveOneOutEstimates.Length < 2 Then
                AppGlobals.BSerr.LogAndThrow(New ArgumentException("At least two leave-one-out estimates are required for a jackknife confidence interval."))
            End If

            Dim n As Integer = leaveOneOutEstimates.Length
            Dim meanTheta As Double = leaveOneOutEstimates.Average()
            Dim ss As Double = 0.0
            For i As Integer = 0 To n - 1
                Dim d As Double = leaveOneOutEstimates(i) - meanTheta
                ss += d * d
            Next
            Dim se As Double = Math.Sqrt(((n - 1.0) / n) * ss)
            Dim crit As Double = If(useTDistribution,
                                    distributions.T_Inv(1.0 - alpha / 2.0, Math.Max(1, n - 1)),
                                    distributions.NormSInv(1.0 - alpha / 2.0))
            Return New ConfidenceIntervalResult With {
                .Estimate = observedEstimate,
                .alpha = alpha,
                .StdErr = se,
                .LowerLimit = observedEstimate - crit * se,
                .UpperLimit = observedEstimate + crit * se
            }
        End Function

        Private Shared Function BuildSubjectIndexGroups(subjectIds As Object()) As Dictionary(Of String, List(Of Integer))
            Dim grouped As New Dictionary(Of String, List(Of Integer))(StringComparer.Ordinal)
            For i As Integer = 0 To subjectIds.Length - 1
                Dim key As String = Convert.ToString(subjectIds(i), Globalization.CultureInfo.InvariantCulture)
                If Not grouped.ContainsKey(key) Then grouped.Add(key, New List(Of Integer))
                grouped(key).Add(i)
            Next
            Return grouped
        End Function

        Private Shared Function ExcludeSubject(subjectIds As Object(),
                                               plotX As Double(),
                                               differences As Double(),
                                               rowsToExclude As List(Of Integer)) As (SubjectIds As Object(), PlotX As Double(), Differences As Double())
            Dim skip As New HashSet(Of Integer)(rowsToExclude)
            Dim outIds As New List(Of Object)(Math.Max(0, subjectIds.Length - skip.Count))
            Dim outX As New List(Of Double)(Math.Max(0, plotX.Length - skip.Count))
            Dim outD As New List(Of Double)(Math.Max(0, differences.Length - skip.Count))
            For i As Integer = 0 To subjectIds.Length - 1
                If skip.Contains(i) Then Continue For
                outIds.Add(subjectIds(i))
                outX.Add(plotX(i))
                outD.Add(differences(i))
            Next
            Return (outIds.ToArray(), outX.ToArray(), outD.ToArray())
        End Function

        Private Shared Function CloneOptions(opts As BlandAltmanOptions) As BlandAltmanOptions
            Return New BlandAltmanOptions With {
                .Alpha = opts.Alpha,
                .Scale = opts.Scale,
                .XAxisMode = opts.XAxisMode,
                .CiMethod = opts.CiMethod,
                .UseTDistribution = opts.UseTDistribution,
                .BootstrapReplicates = opts.BootstrapReplicates,
                .SubjectIds = opts.SubjectIds,
                .Mode = opts.Mode,
                .ExcludeSingletonSubjects = opts.ExcludeSingletonSubjects,
                .MinSubjects = opts.MinSubjects,
                .MinPairsPerSubject = opts.MinPairsPerSubject,
                .CheckProportionalBias = opts.CheckProportionalBias,
                .PlotMode = opts.PlotMode,
                .AllowFallbackToSimple = opts.AllowFallbackToSimple
            }
        End Function

        Friend Shared Function ComputeProportionalBiasTrend(plotX As Double(), differences As Double()) As TestResult
            If plotX Is Nothing Then AppGlobals.BSerr.LogAndThrow(New ArgumentNullException(NameOf(plotX)))
            If differences Is Nothing Then AppGlobals.BSerr.LogAndThrow(New ArgumentNullException(NameOf(differences)))
            If plotX.Length <> differences.Length Then AppGlobals.BSerr.LogAndThrow(New ArgumentException("plotX and differences must have the same length."))
            If plotX.Length < 3 Then AppGlobals.BSerr.LogAndThrow(New ArgumentException("At least 3 points are required for the proportional-bias trend check."))

            Dim n As Integer = plotX.Length
            Dim meanX As Double = plotX.Average()
            Dim meanY As Double = differences.Average()
            Dim sxx As Double = 0.0
            Dim sxy As Double = 0.0
            For i As Integer = 0 To n - 1
                Dim dx As Double = plotX(i) - meanX
                Dim dy As Double = differences(i) - meanY
                sxx += dx * dx
                sxy += dx * dy
            Next

            If sxx <= 0.0 Then
                Return New TestResult With {
                    .Pvalue = Double.NaN,
                    .TestStatistics1 = Double.NaN,
                    .TestStatistics2 = Double.NaN,
                    .DF1 = n - 2,
                    .strSpecialInformation = "Proportional-bias trend not computed because the x-axis values have zero variance."
                }
            End If

            Dim slope As Double = sxy / sxx
            Dim intercept As Double = meanY - slope * meanX
            Dim sse As Double = 0.0
            For i As Integer = 0 To n - 1
                Dim fitted As Double = intercept + slope * plotX(i)
                Dim resid As Double = differences(i) - fitted
                sse += resid * resid
            Next

            Dim df As Integer = n - 2
            Dim mse As Double = sse / Math.Max(1, df)
            Dim seSlope As Double = Math.Sqrt(mse / sxx)
            Dim tValue As Double = slope / seSlope
            Dim cdf As Double = distributions.T_CDF(Math.Abs(tValue), Math.Max(1, df))
            Dim pTwoSided As Double = 2.0 * (1.0 - cdf)

            Return New TestResult With {
                .Pvalue = pTwoSided,
                .TestStatistics1 = slope,
                .TestStatistics2 = tValue,
                .DF1 = df,
                .strSpecialInformation = $"OLS trend of differences on the selected x-axis quantity. Intercept = {CSng(intercept)}, SE(slope) = {CSng(seSlope)}."
            }
        End Function

        ''' <summary> 
        ''' Computes repeated-measures Bland–Altman summary statistics from subject-grouped paired observations. 
        ''' </summary> 
        ''' <param name="subjectIds">Subject identifiers aligned with <paramref name="plotX"/> and <paramref name="differences"/>.</param> 
        ''' <param name="plotX">The x-axis values used for plotting.</param> 
        ''' <param name="differences">The transformed paired differences.</param> 
        ''' <param name="opts">Bland–Altman options controlling repeated-measures eligibility and fallback behavior.</param> 
        ''' <returns> 
        ''' A tuple indicating whether repeated-measures analysis should be used, together with within-subject SD, 
        ''' subject summaries, and any explanatory note. 
        ''' </returns> 
        ''' <remarks> 
        ''' <para> 
        ''' Subjects are first grouped by identifier. A subject is eligible for repeated-measures variance estimation only if 
        ''' it has at least <see cref="BlandAltmanOptions.MinPairsPerSubject"/> usable pairs, with an enforced minimum of 2 
        ''' because within-subject variance cannot be estimated from a single observation. 
        ''' </para> 
        ''' <para> 
        ''' Subject means returned for plotting depend on <see cref="BlandAltmanOptions.ExcludeSingletonSubjects"/>: 
        ''' when <c>True</c>, only variance-eligible subjects are returned for subject-mean plotting; otherwise all subjects are returned. 
        ''' </para> 
        ''' </remarks> 
        Private Shared Function ComputeRepeatedStatistics(subjectIds As Object(),
                                                  plotX As Double(),
                                                  differences As Double(),
                                                  opts As BlandAltmanOptions) As (UseRepeated As Boolean,
                                                                                  WithinSubjectSD As Double,
                                                                                  BetweenSubjectSD As Double,
                                                                                  SubjectCount As Integer,
                                                                                  ExcludedSubjectCount As Integer,
                                                                                  SubjectMeanPlotX As Double(),
                                                                                  SubjectMeanPlotY As Double(),
                                                                                  SubjectLabels As Object(),
                                                                                  SubjectObservationCounts As Integer(),
                                                                                  SubjectMeanDifferences As Double(),
                                                                                  SubjectMeanXAxis As Double(),
                                                                                  Note As String)

            Dim groups As New Dictionary(Of String, SubjectAccumulator)(StringComparer.Ordinal)
            For i As Integer = 0 To subjectIds.Length - 1
                Dim key As String = Convert.ToString(subjectIds(i), Globalization.CultureInfo.InvariantCulture)
                If Not groups.ContainsKey(key) Then
                    groups.Add(key, New SubjectAccumulator(subjectIds(i)))
                End If
                groups(key).Add(plotX(i), differences(i))
            Next

            Dim minPairs As Integer = Math.Max(2, opts.MinPairsPerSubject)
            Dim minSubjects As Integer = Math.Max(2, opts.MinSubjects)
            Dim eligible As New List(Of SubjectAccumulator)
            Dim displaySubjects As New List(Of SubjectAccumulator)
            Dim excluded As Integer = 0

            For Each kv As KeyValuePair(Of String, SubjectAccumulator) In groups
                Dim subj As SubjectAccumulator = kv.Value
                Dim qualifies As Boolean = (subj.Count >= minPairs)

                If qualifies Then
                    eligible.Add(subj)
                Else
                    excluded += 1
                End If

                If opts.ExcludeSingletonSubjects Then
                    If qualifies Then displaySubjects.Add(subj)
                Else
                    displaySubjects.Add(subj)
                End If
            Next

            If eligible.Count < minSubjects Then
                Return (False, Double.NaN, Double.NaN,
                        eligible.Count,
                        excluded,
                        Nothing, Nothing, Nothing, Nothing, Nothing, Nothing,
                        $"Subject IDs were supplied, but fewer than {minSubjects} subject(s) had at least {minPairs} usable pair(s); ordinary Bland–Altman was used.")
            End If

            Dim ssWithin As Double = 0.0
            Dim dfWithin As Integer = 0

            For Each s In eligible
                Dim meanD As Double = s.DifferenceMean
                For Each dVal As Double In s.Differences
                    ssWithin += (dVal - meanD) * (dVal - meanD)
                Next
                dfWithin += s.Count - 1
            Next

            If dfWithin <= 0 Then
                Return (False, Double.NaN, Double.NaN,
                        eligible.Count,
                        excluded,
                        Nothing, Nothing, Nothing, Nothing, Nothing, Nothing,
                        "Repeated-measures variance could not be estimated; ordinary Bland–Altman was used.")
            End If

            Dim sw As Double = Math.Sqrt(ssWithin / dfWithin)
            Dim eligibleMeanDiffs As Double() = eligible.Select(Function(s) s.DifferenceMean).ToArray()
            Dim betweenSubjectSD As Double = If(eligibleMeanDiffs.Length >= 2, StatFunc.stDev(eligibleMeanDiffs), Double.NaN)

            ' Only materialize subject-mean arrays when the selected plot mode can actually use them.
            If opts.PlotMode = RepeatedBlandAltmanPlotMode.AllObservations Then
                Return (True,
                        sw,
                        betweenSubjectSD,
                        eligible.Count,
                        excluded,
                        Nothing, Nothing, Nothing, Nothing, Nothing, Nothing,
                        String.Empty)
            End If

            Dim plotSubjects As List(Of SubjectAccumulator) = displaySubjects

            Dim subjectMeanPlotX As Double() = plotSubjects.Select(Function(s) s.XMean).ToArray()
            Dim subjectMeanPlotY As Double() = plotSubjects.Select(Function(s) s.DifferenceMean).ToArray()
            Dim subjectLabelsOut As Object() = plotSubjects.Select(Function(s) s.SubjectLabel).ToArray()
            Dim subjectObservationCounts As Integer() = plotSubjects.Select(Function(s) s.Count).ToArray()
            Dim subjectMeanDifferences As Double() = plotSubjects.Select(Function(s) s.DifferenceMean).ToArray()
            Dim subjectMeanXAxis As Double() = plotSubjects.Select(Function(s) s.XMean).ToArray()

            Return (True,
                    sw,
                    betweenSubjectSD,
                    eligible.Count,
                    excluded,
                    subjectMeanPlotX,
                    subjectMeanPlotY,
                    subjectLabelsOut,
                    subjectObservationCounts,
                    subjectMeanDifferences,
                    subjectMeanXAxis,
                    String.Empty)
        End Function

        ''' <summary>
        ''' Computes clustered bootstrap confidence intervals for repeated-measures Bland–Altman analysis by resampling whole subjects with replacement.
        ''' </summary>
        ''' <param name="subjectIds">Subject identifiers aligned with <paramref name="plotX"/> and <paramref name="differences"/>.</param>
        ''' <param name="plotX">The Bland–Altman x-axis values for each repeated paired observation.</param>
        ''' <param name="differences">The transformed paired differences for each repeated paired observation.</param>
        ''' <param name="observedBias">The observed repeated-measures bias estimate from the original data.</param>
        ''' <param name="observedWithinSubjectSD">The observed pooled within-subject SD from the original data.</param>
        ''' <param name="opts">Bland–Altman options controlling bootstrap size and repeated-measures eligibility.</param>
        ''' <returns>
        ''' A tuple containing the clustered-bootstrap confidence intervals for the bias, lower LoA, and upper LoA.
        ''' </returns>
        ''' <remarks>
        ''' <para>
        ''' This function resamples subjects rather than individual rows. Each selected subject contributes all of its repeated
        ''' observations to the bootstrap sample. When a subject is sampled more than once, each copy is assigned a new synthetic
        ''' subject ID so that the repeated-measures grouping logic treats duplicated draws as separate clusters.
        ''' </para>
        ''' <para>
        ''' The returned intervals are percentile-bootstrap intervals. When <see cref="AgreementCiMethod.BootstrapBCa"/> is requested,
        ''' the current implementation still uses the clustered percentile engine.
        ''' </para>
        ''' </remarks>
        Private Shared Function ComputeRepeatedBootstrapConfidenceIntervals(subjectIds As Object(),
                                                            plotX As Double(),
                                                            differences As Double(),
                                                            observedBias As Double,
                                                            observedWithinSubjectSD As Double,
                                                            opts As BlandAltmanOptions,
                                                            Optional randomSeed As Integer = Integer.MinValue) As (BiasCI As ConfidenceIntervalResult,
                                                                                                                    LowerLoACI As ConfidenceIntervalResult,
                                                                                                                    UpperLoACI As ConfidenceIntervalResult)
            If subjectIds Is Nothing Then AppGlobals.BSerr.LogAndThrow(New ArgumentNullException(NameOf(subjectIds)))
            If plotX Is Nothing Then AppGlobals.BSerr.LogAndThrow(New ArgumentNullException(NameOf(plotX)))
            If differences Is Nothing Then AppGlobals.BSerr.LogAndThrow(New ArgumentNullException(NameOf(differences)))
            If subjectIds.Length <> plotX.Length OrElse subjectIds.Length <> differences.Length Then
                AppGlobals.BSerr.LogAndThrow(New ArgumentException("subjectIds, plotX, and differences must have the same length."))
            End If
            If opts Is Nothing Then AppGlobals.BSerr.LogAndThrow(New ArgumentNullException(NameOf(opts)))

            Dim biasEstimates As New List(Of Double)(opts.BootstrapReplicates)
            Dim lowerEstimates As New List(Of Double)(opts.BootstrapReplicates)
            Dim upperEstimates As New List(Of Double)(opts.BootstrapReplicates)
            Dim rng = AppGlobals.CreateRandom(randomSeed)
            Dim maxAttempts As Integer = Math.Max(opts.BootstrapReplicates * 20, 1000)
            Dim attempts As Integer = 0

            While biasEstimates.Count < opts.BootstrapReplicates AndAlso attempts < maxAttempts
                attempts += 1
                Dim boot = ResampleSubjectsWithReplacement(subjectIds, plotX, differences, rng, attempts)
                Dim repeated = ComputeRepeatedStatistics(boot.SubjectIds, boot.PlotX, boot.Differences, opts)

                If repeated.UseRepeated AndAlso Not Double.IsNaN(repeated.WithinSubjectSD) AndAlso repeated.WithinSubjectSD > 0.0 Then
                    Dim biasB As Double = boot.Differences.Average()
                    Dim lowerB As Double = biasB - DefaultLoAMultiplier * repeated.WithinSubjectSD
                    Dim upperB As Double = biasB + DefaultLoAMultiplier * repeated.WithinSubjectSD
                    biasEstimates.Add(biasB)
                    lowerEstimates.Add(lowerB)
                    upperEstimates.Add(upperB)
                End If
            End While

            If biasEstimates.Count < Math.Max(100, CInt(Math.Ceiling(opts.BootstrapReplicates * 0.5))) Then
                AppGlobals.BSerr.LogAndThrow(New InvalidOperationException("Too few successful clustered bootstrap replicates were obtained for repeated-measures Bland–Altman confidence intervals."))
            End If

            Dim biasCI As ConfidenceIntervalResult = BuildPercentileConfidenceInterval(biasEstimates.ToArray(), observedBias, opts.Alpha)
            Dim lowerCI As ConfidenceIntervalResult = BuildPercentileConfidenceInterval(lowerEstimates.ToArray(), observedBias - DefaultLoAMultiplier * observedWithinSubjectSD, opts.Alpha)
            Dim upperCI As ConfidenceIntervalResult = BuildPercentileConfidenceInterval(upperEstimates.ToArray(), observedBias + DefaultLoAMultiplier * observedWithinSubjectSD, opts.Alpha)

            biasCI.StdErr = StatFunc.stDev(biasEstimates.ToArray())
            lowerCI.StdErr = StatFunc.stDev(lowerEstimates.ToArray())
            upperCI.StdErr = StatFunc.stDev(upperEstimates.ToArray())

            Return (biasCI, lowerCI, upperCI)
        End Function

        ''' <summary>
        ''' Builds one clustered bootstrap sample by resampling whole subjects with replacement.
        ''' </summary>
        ''' <param name="subjectIds">Original subject identifiers.</param>
        ''' <param name="plotX">Original x-axis values.</param>
        ''' <param name="differences">Original transformed paired differences.</param>
        ''' <param name="rng">Random-number generator used for sampling.</param>
        ''' <param name="replicateIndex">Index of the current bootstrap replicate, used only to construct unique synthetic subject IDs.</param>
        ''' <returns>
        ''' A tuple containing bootstrap-sample subject IDs, x-axis values, and differences.
        ''' </returns>
        ''' <remarks>
        ''' If a subject is sampled multiple times in the same bootstrap replicate, each sampled copy receives a new synthetic
        ''' subject identifier so that repeated-measures grouping is preserved without merging duplicated draws.
        ''' </remarks>
        Private Shared Function ResampleSubjectsWithReplacement(subjectIds As Object(),
                                                        plotX As Double(),
                                                        differences As Double(),
                                                        rng As Random,
                                                        replicateIndex As Integer) As (SubjectIds As Object(), PlotX As Double(), Differences As Double())
            Dim grouped As New Dictionary(Of String, List(Of Integer))(StringComparer.Ordinal)
            For i As Integer = 0 To subjectIds.Length - 1
                Dim key As String = Convert.ToString(subjectIds(i), Globalization.CultureInfo.InvariantCulture)
                If Not grouped.ContainsKey(key) Then grouped.Add(key, New List(Of Integer))
                grouped(key).Add(i)
            Next

            Dim keys As List(Of String) = grouped.Keys.ToList()
            Dim outIds As New List(Of Object)(subjectIds.Length)
            Dim outX As New List(Of Double)(plotX.Length)
            Dim outD As New List(Of Double)(differences.Length)

            For drawIndex As Integer = 0 To keys.Count - 1
                Dim pickedKey As String = keys(rng.Next(0, keys.Count))
                Dim syntheticId As String = $"{pickedKey}__bs{replicateIndex}_{drawIndex}"
                For Each rowIndex As Integer In grouped(pickedKey)
                    outIds.Add(syntheticId)
                    outX.Add(plotX(rowIndex))
                    outD.Add(differences(rowIndex))
                Next
            Next

            Return (outIds.ToArray(), outX.ToArray(), outD.ToArray())
        End Function

        Private Shared Function BootstrapBiasConfidenceInterval(differences As Double(),
                                                        opts As BlandAltmanOptions,
                                                        Optional randomSeed As Integer = Integer.MinValue) As ConfidenceIntervalResult
            Dim n As Integer = differences.Length
            Dim estimates(opts.BootstrapReplicates - 1) As Double
            Dim rng = AppGlobals.CreateRandom(randomSeed)

            For b As Integer = 0 To opts.BootstrapReplicates - 1
                Dim sample(n - 1) As Double
                For i As Integer = 0 To n - 1
                    sample(i) = differences(rng.Next(0, n))
                Next
                estimates(b) = sample.Average()
            Next

            Dim observed As Double = differences.Average()
            Dim ci = BuildPercentileConfidenceInterval(estimates, observed, opts.Alpha)
            ci.StdErr = StatFunc.stDev(estimates)
            Return ci
        End Function

        Private Shared Function BootstrapLoAConfidenceIntervals(n As Integer,
                                                        bias As Double,
                                                        sdDiff As Double,
                                                        opts As BlandAltmanOptions,
                                                        Optional randomSeed As Integer = Integer.MinValue) As (Lower As ConfidenceIntervalResult,
                                                                                                                Upper As ConfidenceIntervalResult)
            Dim b As Integer = opts.BootstrapReplicates
            Dim lowerEst(b - 1) As Double
            Dim upperEst(b - 1) As Double
            Dim rng = AppGlobals.CreateRandom(randomSeed)

            For r As Integer = 0 To b - 1
                ' Parametric bootstrap around the estimated BA model.
                Dim sample(n - 1) As Double
                For i As Integer = 0 To n - 1
                    sample(i) = bias + sdDiff * distributions.NormSInv(rng.NextDouble())
                Next
                Dim meanB As Double = sample.Average()
                Dim sdB As Double = StatFunc.stDev(sample)
                lowerEst(r) = meanB - DefaultLoAMultiplier * sdB
                upperEst(r) = meanB + DefaultLoAMultiplier * sdB
            Next

            Dim lower As ConfidenceIntervalResult = BuildPercentileConfidenceInterval(lowerEst, bias - DefaultLoAMultiplier * sdDiff, opts.Alpha)
            Dim upper As ConfidenceIntervalResult = BuildPercentileConfidenceInterval(upperEst, bias + DefaultLoAMultiplier * sdDiff, opts.Alpha)
            lower.StdErr = StatFunc.stDev(lowerEst)
            upper.StdErr = StatFunc.stDev(upperEst)
            Return (lower, upper)
        End Function

        Private Shared Function BuildPercentileConfidenceInterval(samples As Double(), observed As Double, alpha As Double) As ConfidenceIntervalResult
            Dim sorted As Double() = CType(samples.Clone(), Double())
            Array.Sort(sorted)
            Dim lower As Double = PercentileFromSorted(sorted, alpha / 2.0)
            Dim upper As Double = PercentileFromSorted(sorted, 1.0 - alpha / 2.0)
            Return New ConfidenceIntervalResult With {
                .Estimate = observed,
                .alpha = alpha,
                .LowerLimit = lower,
                .UpperLimit = upper
            }
        End Function

        Private Shared Function PercentileFromSorted(sorted As Double(), p As Double) As Double
            If sorted Is Nothing OrElse sorted.Length = 0 Then Return Double.NaN
            If p <= 0.0 Then Return sorted(0)
            If p >= 1.0 Then Return sorted(sorted.Length - 1)

            Dim h As Double = (sorted.Length - 1) * p
            Dim i As Integer = CInt(Math.Floor(h))
            Dim frac As Double = h - i
            If i >= sorted.Length - 1 Then Return sorted(sorted.Length - 1)
            Return sorted(i) + frac * (sorted(i + 1) - sorted(i))
        End Function

        Private Shared Sub AddHorizontalReferenceLine(ch As Chart,
                                                      lineX As Double(),
                                                      yValue As Double,
                                                      seriesName As String,
                                                      lineColor As Integer)
            If ch Is Nothing Then Exit Sub
            Dim lineY As Double() = {yValue, yValue}
            ch.SeriesCollection.NewSeries()
            With ch.SeriesCollection(ch.SeriesCollection.Count)
                .XValues = lineX
                .Values = lineY
                .Name = seriesName
                .MarkerStyle = XlMarkerStyle.xlMarkerStyleNone
                .Format.Line.Visible = True
                .Format.Line.ForeColor.RGB = lineColor
                .Format.Line.Weight = 1.5
            End With
        End Sub

        Private Shared Function IsMissingSubjectId(value As Object) As Boolean
            If value Is Nothing OrElse Convert.IsDBNull(value) Then Return True
            If TypeOf value Is String Then Return String.IsNullOrWhiteSpace(CStr(value))
            Return False
        End Function

        Private Shared Function NormalizeSubjectId(value As Object) As Object
            If TypeOf value Is String Then Return CStr(value).Trim()
            Return value
        End Function

        Private Shared Function ComputeXAxisValue(reference As Double,
                                                  test As Double,
                                                  mode As BlandAltmanXAxisMode) As Double
            Select Case mode
                Case BlandAltmanXAxisMode.MeanOfMethods : Return 0.5 * (reference + test)
                Case BlandAltmanXAxisMode.ReferenceMethod : Return reference
                Case BlandAltmanXAxisMode.TestMethod : Return test
                Case Else
                    AppGlobals.BSerr.LogAndThrow(New ArgumentOutOfRangeException(NameOf(mode), "Unsupported Bland–Altman x-axis mode."))
                    Return Double.NaN
            End Select
        End Function

        Private Shared Function GetXAxisLabel(referenceName As String,
                                              testName As String,
                                              mode As BlandAltmanXAxisMode) As String
            Select Case mode
                Case BlandAltmanXAxisMode.MeanOfMethods : Return $"Mean of methods ({referenceName}, {testName})"
                Case BlandAltmanXAxisMode.ReferenceMethod : Return referenceName
                Case BlandAltmanXAxisMode.TestMethod : Return testName
                Case Else : Return "Method average"
            End Select
        End Function

        Private Shared Function GetScaleDisplayText(scale As BlandAltmanScale) As String
            Select Case scale
                Case BlandAltmanScale.RawDifference : Return "Raw difference"
                Case BlandAltmanScale.PercentOfMean : Return "Percent of paired mean"
                Case BlandAltmanScale.PercentOfReference : Return "Percent of reference"
                Case BlandAltmanScale.PercentOfTest : Return "Percent of test"
                Case BlandAltmanScale.LogRatio : Return "Log ratio"
                Case Else : Return "Unknown"
            End Select
        End Function

        Private Shared Function GetXAxisDisplayText(mode As BlandAltmanXAxisMode) As String
            Select Case mode
                Case BlandAltmanXAxisMode.MeanOfMethods : Return "Mean of methods"
                Case BlandAltmanXAxisMode.ReferenceMethod : Return "Reference method"
                Case BlandAltmanXAxisMode.TestMethod : Return "Test method"
                Case Else : Return "Unknown"
            End Select
        End Function

        Private Shared Function GetCiMethodDisplayText(method As AgreementCiMethod) As String
            Select Case method
                Case AgreementCiMethod.Analytical : Return "Analytical"
                Case AgreementCiMethod.Jackknife : Return "Jackknife"
                Case AgreementCiMethod.BootstrapPercentile : Return "Bootstrap percentile"
                Case AgreementCiMethod.BootstrapBCa : Return "Bootstrap BCa (currently uses percentile engine)"
                Case Else : Return "Unknown"
            End Select
        End Function

        Private Sub ResetFitState()
            pResult = Nothing
            pIsFitted = False
            pPlotXLabel = String.Empty
            pPlotYLabel = String.Empty
            pScaleNote = String.Empty
            pDroppedPairCount = 0
            pUsedRepeatedModel = False
            pSubjectCount = 0
            pExcludedSubjectCount = 0
            pWithinSubjectSD = Double.NaN
            pSubjectMeanPlotX = Nothing
            pSubjectMeanPlotY = Nothing
            pSubjectLabels = Nothing
            pUsedBootstrapCi = False
            pBootstrapSeedUsed = Integer.MinValue
        End Sub

        Private NotInheritable Class SubjectAccumulator
            Public ReadOnly SubjectLabel As Object
            Public ReadOnly XValues As New List(Of Double)
            Public ReadOnly Differences As New List(Of Double)

            Public Sub New(label As Object)
                SubjectLabel = label
            End Sub

            Public Sub Add(x As Double, d As Double)
                XValues.Add(x)
                Differences.Add(d)
            End Sub

            Public ReadOnly Property Count As Integer
                Get
                    Return Differences.Count
                End Get
            End Property

            Public ReadOnly Property XMean As Double
                Get
                    Return If(XValues.Count = 0, Double.NaN, XValues.Average())
                End Get
            End Property

            Public ReadOnly Property DifferenceMean As Double
                Get
                    Return If(Differences.Count = 0, Double.NaN, Differences.Average())
                End Get
            End Property
        End Class

    End Class

End Namespace
