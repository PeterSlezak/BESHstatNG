Option Explicit On

Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports BESHStatNG.AppInfrastructure
Imports BESHStatNG.Resampling
Imports Microsoft.Office.Interop.Excel

Namespace Agreement

    ''' <summary>
    ''' Computes Lin's concordance correlation coefficient (CCC) for two paired measurement methods.
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' Lin's concordance correlation coefficient quantifies both precision and accuracy of agreement
    ''' between paired numeric measurements. It combines Pearson correlation with a bias-correction term:
    ''' </para>
    ''' <para>
    ''' <c>ρ_c = ρ × C_b</c>
    ''' </para>
    ''' <para>
    ''' where:
    ''' </para>
    ''' <list type="bullet">
    '''   <item><description><c>ρ</c> is the Pearson correlation coefficient (precision component)</description></item>
    '''   <item><description><c>C_b</c> is Lin's bias-correction factor (accuracy component)</description></item>
    ''' </list>
    ''' <para>
    ''' The coefficient can also be written directly in moment form as:
    ''' </para>
    ''' <para>
    ''' <c>ρ_c = 2 s_xy / (s_x² + s_y² + (x̄ − ȳ)²)</c>
    ''' </para>
    ''' <para>
    ''' where <c>s_xy</c> is the sample covariance, <c>s_x²</c> and <c>s_y²</c> are the sample variances,
    ''' and <c>x̄</c>, <c>ȳ</c> are the sample means.
    ''' </para>
    ''' <para>
    ''' This implementation follows the current BESHStatNG architecture:
    ''' </para>
    ''' <list type="bullet">
    '''   <item><description>finite pair filtering before analysis</description></item>
    '''   <item><description>results returned through <see cref="LinConcordanceResult"/></description></item>
    '''   <item><description>formatted output through <see cref="ResultTable"/></description></item>
    '''   <item><description>optional Excel scatter plot with identity line</description></item>
    ''' </list>
    ''' <para>
    ''' The first implementation supports pairwise-complete independent measurements. Repeated-measure or
    ''' clustered variants based on <see cref="LinConcordanceOptions.SubjectIds"/> are intentionally deferred.
    ''' If subject identifiers are supplied, the class throws <see cref="NotSupportedException"/> so that the
    ''' limitation is explicit.
    ''' </para>
    ''' <para>
    ''' For analytical confidence intervals and hypothesis tests this implementation uses a Fisher z-style
    ''' approximation applied to the observed concordance coefficient. This is convenient and practical for a
    ''' first implementation, but it should be documented as an approximation rather than an exact small-sample
    ''' procedure.
    ''' </para>
    ''' </remarks>
    Public Class LinConcordanceCorrelation

        Private ReadOnly pVarX As String
        Private ReadOnly pVarY As String
        Private ReadOnly pReferenceData As Double()
        Private ReadOnly pTestData As Double()

        Private pOptions As LinConcordanceOptions
        Private pResult As LinConcordanceResult
        Private pIsFitted As Boolean = False
        Private pDroppedPairCount As Integer = 0
        Private pFilteredReference As Double() = Nothing
        Private pFilteredTest As Double() = Nothing
        Private pComputationNotes As New List(Of String)
        Private pBootstrapRunInfo As ResamplingRunInfo = Nothing

        ''' <summary>
        ''' Initializes a new Lin concordance-correlation analysis object.
        ''' </summary>
        ''' <param name="dataX">
        ''' Numeric observations for the reference method.
        ''' </param>
        ''' <param name="dataY">
        ''' Numeric observations for the test method.
        ''' </param>
        ''' <param name="varX">Display name of the reference method.</param>
        ''' <param name="varY">Display name of the test method.</param>
        ''' <param name="opts">
        ''' Optional concordance options. If <c>Nothing</c>, a new <see cref="LinConcordanceOptions"/> instance is used.
        ''' </param>
        ''' <remarks>
        ''' <para>
        ''' The constructor stores copies of the supplied arrays and performs only structural validation.
        ''' Full preprocessing and numerical estimation are performed by <see cref="Fit"/>.
        ''' </para>
        ''' </remarks>
        Public Sub New(dataX As Double(),
                       dataY As Double(),
                       varX As String,
                       varY As String,
                       Optional opts As LinConcordanceOptions = Nothing)

            If dataX Is Nothing Then AppGlobals.BSerr.LogAndThrow(New ArgumentNullException(NameOf(dataX)))
            If dataY Is Nothing Then AppGlobals.BSerr.LogAndThrow(New ArgumentNullException(NameOf(dataY)))
            If dataX.Length <> dataY.Length Then
                AppGlobals.BSerr.LogAndThrow(New ArgumentException("Reference and test arrays must have the same length."))
            End If
            If dataX.Length < 3 Then
                AppGlobals.BSerr.LogAndThrow(New ArgumentException("At least 3 paired observations are required for Lin concordance analysis."))
            End If

            Me.pReferenceData = DirectCast(dataX.Clone(), Double())
            Me.pTestData = DirectCast(dataY.Clone(), Double())
            Me.pVarX = If(String.IsNullOrWhiteSpace(varX), "Reference", varX.Trim())
            Me.pVarY = If(String.IsNullOrWhiteSpace(varY), "Test", varY.Trim())
            Me.pOptions = If(opts, New LinConcordanceOptions())

            ValidateOptions(Me.pOptions)
        End Sub

        ''' <summary>
        ''' Gets or sets the concordance-analysis options used by the class.
        ''' </summary>
        ''' <remarks>
        ''' <para>
        ''' Replacing the options invalidates any previously fitted result, so the analysis is recomputed the next time
        ''' <see cref="Fit"/> is called.
        ''' </para>
        ''' </remarks>
        Public Property Options As LinConcordanceOptions
            Get
                Return Me.pOptions
            End Get
            Set(value As LinConcordanceOptions)
                If value Is Nothing Then AppGlobals.BSerr.LogAndThrow(New ArgumentNullException(NameOf(value)))
                ValidateOptions(value)
                Me.pOptions = value
                Me.pIsFitted = False
                Me.pResult = Nothing
            End Set
        End Property

        ''' <summary>
        ''' Gets the fitted concordance result.
        ''' </summary>
        ''' <returns>
        ''' The current <see cref="LinConcordanceResult"/> instance if the model has been fitted; otherwise <c>Nothing</c>.
        ''' </returns>
        Public ReadOnly Property Result As LinConcordanceResult
            Get
                Return Me.pResult
            End Get
        End Property

        ''' <summary>
        ''' Fits Lin's concordance correlation coefficient and associated summaries.
        ''' </summary>
        ''' <returns>
        ''' A populated <see cref="LinConcordanceResult"/> containing the concordance estimate, decomposition,
        ''' confidence interval, and hypothesis-test summary.
        ''' </returns>
        ''' <remarks>
        ''' <para>
        ''' The fit process performs these steps:
        ''' </para>
        ''' <list type="number">
        '''   <item><description>remove any paired observation containing a non-finite value</description></item>
        '''   <item><description>compute sample means, standard deviations, covariance, Pearson correlation, and CCC</description></item>
        '''   <item><description>compute Lin's bias-correction factor together with location and scale shift summaries</description></item>
        '''   <item><description>construct a confidence interval using the configured method</description></item>
        '''   <item><description>compute an approximate hypothesis test for <c>H0: ρ_c = ρ_0</c></description></item>
        ''' </list>
        ''' </remarks>
        Public Function Fit(Optional progressBar As System.Windows.Forms.ProgressBar = Nothing,
                            Optional randomSeed As Integer = Integer.MinValue) As LinConcordanceResult
            If Me.pIsFitted AndAlso Me.pResult IsNot Nothing Then Return Me.pResult

            ValidateOptions(Me.pOptions)
            Me.pComputationNotes.Clear()
            Me.pBootstrapRunInfo = Nothing

            If Me.pOptions.SubjectIds IsNot Nothing Then
                AppGlobals.BSerr.LogAndThrow(New NotSupportedException("Repeated-measures/clustered Lin concordance is not implemented in this first version. Supply independent paired measurements only."))
            End If

            Dim filtered = AgreementHelpers.FilterFinitePairs(Me.pReferenceData, Me.pTestData)
            Me.pFilteredReference = filtered.Reference
            Me.pFilteredTest = filtered.Test
            Me.pDroppedPairCount = filtered.DroppedCount

            If Me.pFilteredReference.Length < 3 Then
                AppGlobals.BSerr.LogAndThrow(New InvalidOperationException("Fewer than 3 finite paired observations remain after filtering."))
            End If

            Dim core = ComputeLinConcordanceCore(Me.pFilteredReference, Me.pFilteredTest)
            Dim ci As ConfidenceIntervalResult
            If Me.pOptions.CiMethod = AgreementCiMethod.BootstrapPercentile OrElse Me.pOptions.CiMethod = AgreementCiMethod.BootstrapBCa Then
                Dim boot As ScalarResamplingResult = BootstrapConcordanceResamplingResult(Me.pFilteredReference,
                                                                               Me.pFilteredTest,
                                                                               Me.pOptions,
                                                                               progressBar,
                                                                               randomSeed)
                Me.pBootstrapRunInfo = boot.RunInfo
                If Me.pOptions.CiMethod = AgreementCiMethod.BootstrapBCa Then
                    Dim jk As ScalarResamplingResult = JackknifeConcordanceResamplingResult(Me.pFilteredReference, Me.pFilteredTest, Me.pOptions)
                    ci = boot.ToBcaConfidenceInterval(pOptions.Alpha, jk.ResampledStatistics)
                    ResamplingCore.AppendNote(Me.pBootstrapRunInfo, $"BCa acceleration derived from {jk.ReplicateCount} leave-one-out jackknife replicates.")
                Else
                    ci = boot.ToPercentileConfidenceInterval(pOptions.Alpha)
                End If
            Else
                ci = ComputeConcordanceConfidenceInterval(core.Concordance, Me.pFilteredReference.Length, Me.pOptions)
            End If
            Dim ht = ComputeHypothesisTest(core.Concordance, Me.pFilteredReference.Length, Me.pOptions)

            If Me.pDroppedPairCount > 0 Then
                Me.pComputationNotes.Add($"Dropped {Me.pDroppedPairCount} non-finite pair(s) by pairwise complete-case filtering.")
            End If
            If Me.pOptions.CiMethod = AgreementCiMethod.Jackknife Then
                Me.pComputationNotes.Add("Jackknife CI is not yet implemented separately; the current version uses the analytical Fisher z-style approximation.")
            End If
            If Me.pBootstrapRunInfo IsNot Nothing Then
                Me.pComputationNotes.Add($"Bootstrap seed = {Me.pBootstrapRunInfo.SeedUsed}.")
                Me.pComputationNotes.Add($"Bootstrap replicates used = {Me.pBootstrapRunInfo.ReplicatesUsed}/{Me.pBootstrapRunInfo.ReplicatesRequested}.")
                If Me.pBootstrapRunInfo.FailedReplicates > 0 Then
                    Me.pComputationNotes.Add($"Bootstrap failed replicates = {Me.pBootstrapRunInfo.FailedReplicates}.")
                End If
                If Me.pBootstrapRunInfo.Notes IsNot Nothing Then
                    For Each note As String In Me.pBootstrapRunInfo.Notes
                        If Not String.IsNullOrWhiteSpace(note) Then Me.pComputationNotes.Add(note)
                    Next
                End If
            End If

            Me.pResult = New LinConcordanceResult With {
                .ConcordanceCI = ci,
                .PearsonR = core.PearsonR,
                .BiasCorrectionFactor = core.BiasCorrectionFactor,
                .LocationShift = core.LocationShift,
                .ScaleShift = core.ScaleShift,
                .Accuracy = core.BiasCorrectionFactor,
                .Precision = core.PearsonR,
                .HypothesisTest = ht
            }

            If progressBar IsNot Nothing Then
                progressBar.Invoke(Sub() progressBar.Value = 100)
            End If

            Me.pIsFitted = True
            Return Me.pResult
        End Function

        ''' <summary>
        ''' Creates a collection of formatted result tables for worksheet/report output.
        ''' </summary>
        ''' <returns>
        ''' A list of <see cref="ResultTable"/> objects summarizing the fitted Lin concordance analysis.
        ''' </returns>
        ''' <remarks>
        ''' <para>
        ''' If the object has not been fitted yet, <see cref="Fit"/> is called automatically.
        ''' </para>
        ''' </remarks>
        Public Function wrapResults() As List(Of ResultTable)
            If Not Me.pIsFitted OrElse Me.pResult Is Nothing Then Me.Fit()

            Dim out As New List(Of ResultTable)

            Dim t As New ResultTable
            t.AddTitle("Lin Concordance Correlation Coefficient")
            Dim summaryRows = {{"Reference method", Me.pVarX},
                               {"Test method", Me.pVarY},
                               {"Number of valid data pairs", Me.pFilteredReference.Length},
                               {"Dropped non-finite pairs", Me.pDroppedPairCount}}
            t.SetBody(summaryRows)
            out.Add(t)

            t = New ResultTable
            t.AddTitle("Concordance Summary")
            t.SetBody({
                {Me.pResult.ConcordanceCI.Estimate, Me.pResult.ConcordanceCI.strConfidenceInterval(CIformat.LL_to_UL), "Overall concordance combining precision and accuracy."},
                {Me.pResult.Precision, "", "Precision component: Pearson correlation coefficient."},
                {Me.pResult.Accuracy, "", "Accuracy component: bias-correction factor (Cb)."}
            })
            t.AddHeaderLeftRow({"Lin CCC", "Pearson r", "Bias-correction factor"})
            t.AddHeaderTopRow({"Estimate", Me.pResult.ConcordanceCI.CIlabel, "Meaning"})
            out.Add(t)

            t = New ResultTable
            t.AddTitle("Bias Decomposition")
            t.SetBody({
                {Me.pResult.LocationShift, "Standardized mean difference between methods. 0 indicates no location shift."},
                {Me.pResult.ScaleShift, "Ratio of standard deviations (reference/test). 1 indicates no scale shift."}
            })
            t.AddHeaderLeftRow({"Location shift", "Scale shift"})
            t.AddHeaderTopRow({"Estimate", "Meaning"})
            out.Add(t)

            t = New ResultTable
            t.AddTitle("Approximate Hypothesis Test")
            t.SetBody({
                {Me.pOptions.NullConcordance, Me.pResult.HypothesisTest.TestStatistics1, Me.pResult.HypothesisTest.Pvalue}
            })
            t.AddHeaderTopRow({"Null concordance", "z statistic", "Two-sided p-value"})
            If Not String.IsNullOrWhiteSpace(Me.pResult.HypothesisTest.strSpecialInformation) Then
                t.AddFootnote(Me.pResult.HypothesisTest.strSpecialInformation)
            End If
            out.Add(t)

            If Me.pComputationNotes.Count > 0 Then
                t = New ResultTable
                t.AddTitle("Computation Notes")
                Dim body(Me.pComputationNotes.Count - 1, 0) As Object
                For i As Integer = 0 To Me.pComputationNotes.Count - 1
                    body(i, 0) = Me.pComputationNotes(i)
                Next
                t.SetBody(body)
                out.Add(t)
            End If

            Return out
        End Function

        ''' <summary>
        ''' Adds a concordance scatter plot to an Excel worksheet.
        ''' </summary>
        ''' <param name="ws">Target worksheet that will receive the chart.</param>
        ''' <param name="chartTitle">Optional chart title.</param>
        ''' <returns>The created Excel chart object.</returns>
        ''' <remarks>
        ''' <para>
        ''' The plot contains the paired data and a 45-degree identity line. Axis ranges are synchronized so that the
        ''' identity line visually represents perfect agreement.
        ''' </para>
        ''' </remarks>
        Public Function AddPlot(ws As Worksheet,
                                Optional chartTitle As String = "Lin concordance plot") As Chart
            If ws Is Nothing Then AppGlobals.BSerr.LogAndThrow(New ArgumentNullException(NameOf(ws)))
            If Not Me.pIsFitted OrElse Me.pResult Is Nothing Then Me.Fit()

            Dim chartObj As Chart = graphics.GeneralScatterPlot(Me.pFilteredReference,
                                                                Me.pFilteredTest,
                                                                Me.pVarY,
                                                                Me.pVarX,
                                                                ws,
                                                                chartTitle)

            Dim minVal As Double = Math.Min(Me.pFilteredReference.Min(), Me.pFilteredTest.Min())
            Dim maxVal As Double = Math.Max(Me.pFilteredReference.Max(), Me.pFilteredTest.Max())
            If minVal = maxVal Then
                minVal -= 0.5
                maxVal += 0.5
            End If

            With chartObj
                .Axes(XlAxisType.xlCategory).MinimumScale = minVal
                .Axes(XlAxisType.xlCategory).MaximumScale = maxVal
                .Axes(XlAxisType.xlValue).MinimumScale = minVal
                .Axes(XlAxisType.xlValue).MaximumScale = maxVal

                .SeriesCollection.NewSeries()
                With .SeriesCollection(.SeriesCollection.Count)
                    .Name = "Identity"
                    .XValues = New Double() {minVal, maxVal}
                    .Values = New Double() {minVal, maxVal}
                    .MarkerStyle = XlMarkerStyle.xlMarkerStyleNone
                    .Format.Line.Visible = True
                End With
            End With

            Return chartObj
        End Function

        ''' <summary>
        ''' Validates Lin concordance options.
        ''' </summary>
        ''' <param name="opts">Options to validate.</param>
        ''' <remarks>
        ''' Validation currently checks:
        ''' <list type="bullet">
        '''   <item><description><c>alpha</c> must lie strictly between 0 and 1</description></item>
        '''   <item><description><c>BootstrapReplicates</c> must be positive when bootstrap intervals are requested</description></item>
        '''   <item><description><c>NullConcordance</c> must lie strictly between -1 and 1 for Fisher z-style testing</description></item>
        ''' </list>
        ''' </remarks>
        Friend Shared Sub ValidateOptions(opts As LinConcordanceOptions)
            If opts Is Nothing Then AppGlobals.BSerr.LogAndThrow(New ArgumentNullException(NameOf(opts)))
            If Double.IsNaN(opts.Alpha) OrElse opts.Alpha <= 0.0 OrElse opts.Alpha >= 1.0 Then
                AppGlobals.BSerr.LogAndThrow(New ArgumentOutOfRangeException(NameOf(opts.Alpha), "Alpha must lie in the open interval (0, 1)."))
            End If
            If (opts.CiMethod = AgreementCiMethod.BootstrapPercentile OrElse opts.CiMethod = AgreementCiMethod.BootstrapBCa) AndAlso opts.BootstrapReplicates < 200 Then
                AppGlobals.BSerr.LogAndThrow(New ArgumentOutOfRangeException(NameOf(opts.BootstrapReplicates), "At least 200 bootstrap replicates are recommended for bootstrap confidence intervals."))
            End If
            If Double.IsNaN(opts.NullConcordance) OrElse opts.NullConcordance <= -1.0 OrElse opts.NullConcordance >= 1.0 Then
                AppGlobals.BSerr.LogAndThrow(New ArgumentOutOfRangeException(NameOf(opts.NullConcordance), "Null concordance must lie strictly between -1 and 1 for Fisher z-style inference."))
            End If
        End Sub

        Friend Shared Function BootstrapConcordanceResamplingResult(reference As Double(),
                                                            test As Double(),
                                                            opts As LinConcordanceOptions,
                                                            Optional progressBar As System.Windows.Forms.ProgressBar = Nothing,
                                                            Optional randomSeed As Integer = Integer.MinValue) As ScalarResamplingResult
            If reference Is Nothing Then AppGlobals.BSerr.LogAndThrow(New ArgumentNullException(NameOf(reference)))
            If test Is Nothing Then AppGlobals.BSerr.LogAndThrow(New ArgumentNullException(NameOf(test)))
            If reference.Length <> test.Length Then
                AppGlobals.BSerr.LogAndThrow(New ArgumentException("Reference and test arrays must have the same length."))
            End If
            If opts Is Nothing Then AppGlobals.BSerr.LogAndThrow(New ArgumentNullException(NameOf(opts)))

            Dim bootOpts As New BootstrapOptions With {
                .Alpha = opts.Alpha,
                .Replicates = opts.BootstrapReplicates,
                .RandomSeed = randomSeed,
                .MaxFailures = Math.Max(1000, opts.BootstrapReplicates)
            }

            Dim progressCallback As Action(Of Integer, Integer) = Nothing
            If progressBar IsNot Nothing Then
                progressBar.Invoke(Sub() progressBar.Value = 0)
                progressCallback = Sub(completed As Integer, total As Integer)
                                       Dim progressValue As Integer = CInt(Math.Min(100.0, Math.Round(100.0 * completed / Math.Max(1, total))))
                                       progressBar.Invoke(Sub() progressBar.Value = progressValue)
                                   End Sub
            End If

            Dim result As ScalarResamplingResult = ResamplingBootstrapRunner.RunScalarBootstrap(
                reference.Length,
                Function(idx As Integer())
                    Dim sampled = ResamplingBootstrap.TakeByIndices(reference, test, idx)
                    Return ComputeLinConcordanceCore(sampled.Values1, sampled.Values2).Concordance
                End Function,
                bootOpts,
                "Lin concordance correlation",
                "Lin CCC bootstrap",
                50,
                progressCallback)

            Return result
        End Function

        Friend Shared Function JackknifeConcordanceResamplingResult(reference As Double(),
                                                            test As Double(),
                                                            opts As LinConcordanceOptions) As ScalarResamplingResult
            If reference Is Nothing Then AppGlobals.BSerr.LogAndThrow(New ArgumentNullException(NameOf(reference)))
            If test Is Nothing Then AppGlobals.BSerr.LogAndThrow(New ArgumentNullException(NameOf(test)))
            If reference.Length <> test.Length Then
                AppGlobals.BSerr.LogAndThrow(New ArgumentException("Reference and test arrays must have the same length."))
            End If
            If opts Is Nothing Then AppGlobals.BSerr.LogAndThrow(New ArgumentNullException(NameOf(opts)))

            Dim jkOpts As New JackknifeOptions With {.Alpha = opts.Alpha}
            Dim result As ScalarResamplingResult = ResamplingJackknifeRunner.RunScalarJackknife(
                reference.Length,
                Function(idx As Integer())
                    Dim sampled = ResamplingBootstrap.TakeByIndices(reference, test, idx)
                    Return ComputeLinConcordanceCore(sampled.Values1, sampled.Values2).Concordance
                End Function,
                jkOpts,
                "Lin concordance correlation",
                "Lin CCC jackknife",
                2)

            result.ObservedStatistic = ComputeLinConcordanceCore(reference, test).Concordance
            Return result
        End Function

        ''' <summary>
        ''' Computes Lin's concordance coefficient and its decomposition from paired numeric vectors.
        ''' </summary>
        ''' <param name="reference">Filtered finite reference-method values.</param>
        ''' <param name="test">Filtered finite test-method values.</param>
        ''' <returns>
        ''' A tuple containing Lin's CCC, Pearson's r, the bias-correction factor, location shift, scale shift,
        ''' and selected summary moments.
        ''' </returns>
        ''' <remarks>
        ''' <para>
        ''' The returned decomposition is based on:
        ''' </para>
        ''' <para>
        ''' <c>C_b = 2 / (v + 1/v + u²)</c>
        ''' </para>
        ''' <para>
        ''' where <c>u = (x̄ − ȳ) / √(s_x s_y)</c> is the standardized location shift and
        ''' <c>v = s_x / s_y</c> is the scale-shift ratio.
        ''' </para>
        ''' </remarks>
        Friend Shared Function ComputeLinConcordanceCore(reference As Double(),
                                                         test As Double()) As (Concordance As Double,
                                                                                PearsonR As Double,
                                                                                BiasCorrectionFactor As Double,
                                                                                LocationShift As Double,
                                                                                ScaleShift As Double,
                                                                                MeanReference As Double,
                                                                                MeanTest As Double,
                                                                                SDReference As Double,
                                                                                SDTest As Double)
            If reference Is Nothing Then AppGlobals.BSerr.LogAndThrow(New ArgumentNullException(NameOf(reference)))
            If test Is Nothing Then AppGlobals.BSerr.LogAndThrow(New ArgumentNullException(NameOf(test)))
            If reference.Length <> test.Length Then
                AppGlobals.BSerr.LogAndThrow(New ArgumentException("Reference and test arrays must have the same length."))
            End If
            If reference.Length < 3 Then
                AppGlobals.BSerr.LogAndThrow(New ArgumentException("At least 3 paired observations are required."))
            End If

            Dim n As Integer = reference.Length
            Dim meanX As Double = reference.Average()
            Dim meanY As Double = test.Average()
            Dim varX As Double = variance(reference)
            Dim varY As Double = variance(test)
            Dim sdX As Double = Math.Sqrt(varX)
            Dim sdY As Double = Math.Sqrt(varY)

            If varX <= 0.0 OrElse varY <= 0.0 Then
                AppGlobals.BSerr.LogAndThrow(New InvalidOperationException("Lin concordance is undefined when either method has zero sample variance."))
            End If

            Dim covXY As Double = 0.0
            For i As Integer = 0 To n - 1
                covXY += (reference(i) - meanX) * (test(i) - meanY)
            Next
            covXY /= (n - 1)

            Dim pearsonR As Double = covXY / (sdX * sdY)
            Dim denominator As Double = varX + varY + (meanX - meanY) * (meanX - meanY)
            If denominator <= 0.0 Then
                AppGlobals.BSerr.LogAndThrow(New InvalidOperationException("Lin concordance denominator is non-positive; check the supplied data."))
            End If

            Dim rhoC As Double = (2.0 * covXY) / denominator
            rhoC = ClampToOpenUnitInterval(rhoC)

            Dim u As Double = (meanX - meanY) / Math.Sqrt(sdX * sdY)
            Dim v As Double = sdX / sdY
            Dim cb As Double = 2.0 / (v + (1.0 / v) + (u * u))

            Return (rhoC, pearsonR, cb, u, v, meanX, meanY, sdX, sdY)
        End Function

        ''' <summary>
        ''' Computes a confidence interval for Lin's concordance coefficient.
        ''' </summary>
        ''' <param name="concordance">Observed concordance estimate.</param>
        ''' <param name="n">Number of valid paired observations.</param>
        ''' <param name="opts">Analysis options controlling interval construction.</param>
        ''' <returns>A <see cref="ConfidenceIntervalResult"/> for Lin's CCC.</returns>
        ''' <remarks>
        ''' <para>
        ''' The analytical interval uses a Fisher z-style approximation:
        ''' </para>
        ''' <para>
        ''' <c>z = atanh(ρ_c)</c>, <c>SE(z) ≈ 1/√(n−3)</c></para>
        ''' <para>
        ''' followed by back-transformation with <c>tanh</c>.
        ''' </para>
        ''' <para>
        ''' This is a convenient approximate interval for a first implementation, not a dedicated exact Lin-CCC interval.
        ''' </para>
        ''' </remarks>
        Friend Shared Function ComputeConcordanceConfidenceInterval(concordance As Double,
                                                                    n As Integer,
                                                                    opts As LinConcordanceOptions) As ConfidenceIntervalResult
            If n < 3 Then
                AppGlobals.BSerr.LogAndThrow(New ArgumentOutOfRangeException(NameOf(n), "At least 3 observations are required for concordance inference."))
            End If
            If opts Is Nothing Then AppGlobals.BSerr.LogAndThrow(New ArgumentNullException(NameOf(opts)))

            If opts.CiMethod = AgreementCiMethod.Analytical OrElse opts.CiMethod = AgreementCiMethod.Jackknife Then
                Dim out As New ConfidenceIntervalResult With {
                    .Estimate = concordance,
                    .alpha = opts.Alpha
                }

                If n <= 3 Then
                    out.StdErr = Double.NaN
                    out.LowerLimit = Double.NaN
                    out.UpperLimit = Double.NaN
                    Return out
                End If

                Dim z As Double = Atanh(ClampToOpenUnitInterval(concordance))
                Dim se As Double = 1.0 / Math.Sqrt(n - 3.0)
                Dim crit As Double = distributions.NormSInv(1.0 - opts.Alpha / 2.0)

                out.StdErr = se
                out.LowerLimit = Math.Tanh(z - crit * se)
                out.UpperLimit = Math.Tanh(z + crit * se)
                Return out
            End If

            AppGlobals.BSerr.LogAndThrow(New InvalidOperationException("Bootstrap concordance intervals require paired data arrays. Use the overload that takes the paired vectors."))
            Return Nothing
        End Function


        ''' <summary>
        ''' Computes an approximate hypothesis test for the null concordance value.
        ''' </summary>
        ''' <param name="concordance">Observed concordance estimate.</param>
        ''' <param name="n">Number of valid paired observations.</param>
        ''' <param name="opts">Options providing the null concordance value.</param>
        ''' <returns>
        ''' A <see cref="TestResult"/> whose key fields are used as follows:
        ''' <list type="bullet">
        '''   <item><description><see cref="TestResult.TestStatistics1"/> = z statistic</description></item>
        '''   <item><description><see cref="TestResult.TestStatistics2"/> = observed concordance</description></item>
        '''   <item><description><see cref="TestResult.DF1"/> = sample size</description></item>
        '''   <item><description><see cref="TestResult.Pvalue"/> = two-sided p-value</description></item>
        ''' </list>
        ''' </returns>
        Friend Shared Function ComputeHypothesisTest(concordance As Double,
                                                     n As Integer,
                                                     opts As LinConcordanceOptions) As TestResult
            If opts Is Nothing Then AppGlobals.BSerr.LogAndThrow(New ArgumentNullException(NameOf(opts)))

            If n <= 3 Then
                Return New TestResult With {
                    .TestStatistics1 = Double.NaN,
                    .TestStatistics2 = concordance,
                    .DF1 = n,
                    .Pvalue = Double.NaN,
                    .strSpecialInformation = "Approximate Fisher z-style hypothesis test requires at least 4 paired observations."
                }
            End If

            Dim zObs As Double = Atanh(ClampToOpenUnitInterval(concordance))
            Dim zNull As Double = Atanh(ClampToOpenUnitInterval(opts.NullConcordance))
            Dim zStat As Double = (zObs - zNull) * Math.Sqrt(n - 3.0)
            Dim p As Double = 2.0 * (1.0 - distributions.PNorm(Math.Abs(zStat)))

            Return New TestResult With {
                .TestStatistics1 = zStat,
                .TestStatistics2 = concordance,
                .DF1 = n,
                .Pvalue = p,
                .strSpecialInformation = $"Approximate Fisher z-style test of H0: concordance = {CSng(opts.NullConcordance)}."
            }
        End Function

        ''' <summary>
        ''' Clamps a concordance-like coefficient to the open interval (-1, 1).
        ''' </summary>
        ''' <param name="x">Value to clamp.</param>
        ''' <returns>
        ''' The original value when it already lies strictly between -1 and 1; otherwise a numerically safe boundary value.
        ''' </returns>
        Friend Shared Function ClampToOpenUnitInterval(x As Double) As Double
            Const eps As Double = 0.000000000001
            If x <= -1.0 Then Return -1.0 + eps
            If x >= 1.0 Then Return 1.0 - eps
            Return x
        End Function
    End Class

End Namespace
