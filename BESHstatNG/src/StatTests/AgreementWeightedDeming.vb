Option Explicit On

Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports BESHStatNG.AppInfrastructure
Imports Microsoft.Office.Interop.Excel

Namespace Agreement

    ''' <summary>
    ''' Fits classical and generalized weighted Deming regression for method-comparison studies.
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' This class unifies the legacy constant-λ Deming workflow and the newer generalized weighted workflow in one
    ''' back-end object so the front end can call a single implementation.
    ''' </para>
    ''' <para>
    ''' Supported variance models are:
    ''' </para>
    ''' <list type="bullet">
    '''   <item><description><see cref="DemingVarianceModel.ConstantLambda"/>: classical Deming regression with a constant error ratio λ = σx² / σy².</description></item>
    '''   <item><description><see cref="DemingVarianceModel.KnownPointwiseSD"/>: weighted Deming/York-style regression using per-observation standard deviations for both methods.</description></item>
    '''   <item><description><see cref="DemingVarianceModel.ConstantCV"/>: weighted Deming regression under constant coefficients of variation for both methods.</description></item>
    ''' </list>
    ''' <para>
    ''' For the classical constant-λ model with intercept, this class preserves the legacy Deming point estimate,
    ''' closed-form analytical confidence interval, MCR/Linnet analytical interval, and jackknife interval behavior.
    ''' For generalized weighted models it uses a York-style iterative fit and supports jackknife and percentile-bootstrap
    ''' confidence intervals in this first implementation.
    ''' </para>
    ''' </remarks>
    Public Class WeightedDemingRegression

        Private Const DEFAULT_MAX_ITER As Integer = 200
        Private Const DEFAULT_TOLERANCE As Double = 0.000000000001
        Private Const DEFAULT_SD_FLOOR As Double = 0.000000000001

        Private ReadOnly pRawReference As Double()
        Private ReadOnly pRawTest As Double()
        Private ReadOnly pVarX As String
        Private ReadOnly pVarY As String

        Private pOptions As DemingOptions
        Private pResult As MethodComparisonFitResult = Nothing
        Private pInterceptSE As Double = Double.NaN
        Private pSlopeSE As Double = Double.NaN
        Private pCItype As String = String.Empty
        Private pUsedBootstrapCi As Boolean = False
        Private pBootstrapSeedUsed As Integer = Integer.MinValue
        Private pFilteredReference As Double() = Nothing
        Private pFilteredTest As Double() = Nothing
        Private pKeptPairIndices As Integer() = Nothing
        Private pDroppedPairCount As Integer = 0
        Private pIsFitted As Boolean = False
        Private ReadOnly pComputationNotes As New List(Of String)

        ''' <summary>
        ''' Initializes a new weighted/generalized Deming-regression object.
        ''' </summary>
        ''' <param name="dataX">Numeric measurements from the reference method.</param>
        ''' <param name="dataY">Numeric measurements from the test method.</param>
        ''' <param name="varX">Display label for the reference method.</param>
        ''' <param name="varY">Display label for the test method.</param>
        ''' <param name="opts">
        ''' Optional Deming options. If <c>Nothing</c>, a default <see cref="DemingOptions"/> instance is created.
        ''' </param>
        ''' <remarks>
        ''' The constructor validates only the basic paired-array structure. Pairwise finite-case filtering and
        ''' variance-model-specific validation are performed during fitting.
        ''' </remarks>
        Public Sub New(dataX As Double(),
                       dataY As Double(),
                       varX As String,
                       varY As String,
                       Optional opts As DemingOptions = Nothing)

            If dataX Is Nothing Then AppGlobals.BSerr.LogAndThrow(New ArgumentNullException(NameOf(dataX)))
            If dataY Is Nothing Then AppGlobals.BSerr.LogAndThrow(New ArgumentNullException(NameOf(dataY)))
            If dataX.Length <> dataY.Length Then
                AppGlobals.BSerr.LogAndThrow(New ArgumentException("Reference and test arrays must have the same length."))
            End If
            If dataX.Length < 3 Then
                AppGlobals.BSerr.LogAndThrow(New ArgumentException("At least 3 paired observations are required for Deming regression."))
            End If

            Me.pRawReference = DirectCast(dataX.Clone(), Double())
            Me.pRawTest = DirectCast(dataY.Clone(), Double())
            Me.pVarX = If(String.IsNullOrWhiteSpace(varX), "Reference", varX.Trim())
            Me.pVarY = If(String.IsNullOrWhiteSpace(varY), "Test", varY.Trim())
            Me.pOptions = If(opts, New DemingOptions())
            ValidateOptions(Me.pOptions)
        End Sub

        ''' <summary>
        ''' Gets or sets the option object controlling the weighted Deming fit.
        ''' </summary>
        ''' <remarks>
        ''' Assigning a new options object invalidates any previously fitted result.
        ''' </remarks>
        Public Property Options As DemingOptions
            Get
                Return Me.pOptions
            End Get
            Set(value As DemingOptions)
                If value Is Nothing Then AppGlobals.BSerr.LogAndThrow(New ArgumentNullException(NameOf(value)))
                ValidateOptions(value)
                Me.pOptions = value
                InvalidateFit()
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the two-sided significance level used to build reported confidence intervals.
        ''' </summary>
        ''' <remarks>
        ''' This legacy-style alias maps directly to <see cref="DemingOptions.Alpha"/>.
        ''' </remarks>
        Public Property alpha As Double
            Get
                Return Me.pOptions.Alpha
            End Get
            Set(value As Double)
                Me.pOptions.Alpha = value
                ValidateOptions(Me.pOptions)
                InvalidateFit()
            End Set
        End Property

        ''' <summary>
        ''' Gets or sets the constant Deming error ratio λ = σx² / σy².
        ''' </summary>
        ''' <remarks>
        ''' Setting this property forces <see cref="DemingOptions.VarianceModel"/> to
        ''' <see cref="DemingVarianceModel.ConstantLambda"/>.
        ''' </remarks>
        Public Property Lambda As Double
            Get
                Return Me.pOptions.Lambda
            End Get
            Set(value As Double)
                Me.pOptions.Lambda = value
                Me.pOptions.VarianceModel = DemingVarianceModel.ConstantLambda
                ValidateOptions(Me.pOptions)
                InvalidateFit()
            End Set
        End Property

        ''' <summary>
        ''' Gets the most recently fitted result object.
        ''' </summary>
        Public ReadOnly Property Result As MethodComparisonFitResult
            Get
                Return Me.pResult
            End Get
        End Property

        ''' <summary>
        ''' Gets the most recently computed confidence interval for the intercept.
        ''' </summary>
        Public ReadOnly Property InterceptCI As ConfidenceIntervalResult
            Get
                Return If(Me.pResult Is Nothing, Nothing, Me.pResult.InterceptCI)
            End Get
        End Property

        ''' <summary>
        ''' Gets the most recently computed confidence interval for the slope.
        ''' </summary>
        Public ReadOnly Property SlopeCI As ConfidenceIntervalResult
            Get
                Return If(Me.pResult Is Nothing, Nothing, Me.pResult.SlopeCI)
            End Get
        End Property

        ''' <summary>
        ''' Gets the most recently computed standard error of the intercept.
        ''' </summary>
        Public ReadOnly Property InterceptSE As Double
            Get
                Return Me.pInterceptSE
            End Get
        End Property

        ''' <summary>
        ''' Gets the most recently computed standard error of the slope.
        ''' </summary>
        Public ReadOnly Property SlopeSE As Double
            Get
                Return Me.pSlopeSE
            End Get
        End Property

        ''' <summary>
        ''' Computes the Deming point estimate only.
        ''' </summary>
        ''' <returns>
        ''' A tuple containing the fitted intercept and slope.
        ''' </returns>
        Public Function FitPointEstimate() As (Intercept As Double, Slope As Double)
            PrepareFilteredData()
            Dim sd = BuildObservationStandardDeviationsForCurrentData()
            Return ComputeWeightedDemingPointEstimate(Me.pFilteredReference, Me.pFilteredTest, sd.SDx, sd.SDy, Me.pOptions)
        End Function

        ''' <summary>
        ''' Fits weighted/generalized Deming regression using the confidence-interval method configured in <see cref="Options"/>.
        ''' </summary>
        ''' <returns>
        ''' A populated <see cref="MethodComparisonFitResult"/> instance.
        ''' </returns>
        Public Function Fit(Optional progressBar As System.Windows.Forms.ProgressBar = Nothing,
                            Optional randomSeed As Integer = Integer.MinValue) As MethodComparisonFitResult
            If Me.pIsFitted AndAlso Me.pResult IsNot Nothing Then Return Me.pResult

            PrepareFilteredData()
            ValidateOptions(Me.pOptions)
            ResetNotes()
            Me.pUsedBootstrapCi = False
            Me.pBootstrapSeedUsed = Integer.MinValue

            Dim sd = BuildObservationStandardDeviationsForCurrentData()
            Dim pointFit = ComputeWeightedDemingPointEstimate(Me.pFilteredReference, Me.pFilteredTest, sd.SDx, sd.SDy, Me.pOptions)
            Dim res As MethodComparisonFitResult

            Select Case Me.pOptions.CiMethod
                Case AgreementCiMethod.Analytical
                    res = FitAnalyticalCore(Me.pFilteredReference, Me.pFilteredTest, sd.SDx, sd.SDy, pointFit)
                Case AgreementCiMethod.Jackknife
                    res = FitJackknifeCore(Me.pFilteredReference, Me.pFilteredTest, sd.SDx, sd.SDy, pointFit)
                Case AgreementCiMethod.BootstrapPercentile, AgreementCiMethod.BootstrapBCa
                    Me.pUsedBootstrapCi = True
                    Me.pBootstrapSeedUsed = ResolveRandomSeed(randomSeed)
                    res = FitBootstrapCore(Me.pFilteredReference, Me.pFilteredTest, sd.SDx, sd.SDy, pointFit, progressBar, Me.pBootstrapSeedUsed)
                Case Else
                    AppGlobals.BSerr.LogAndThrow(New NotSupportedException($"Unsupported CI method: {Me.pOptions.CiMethod}."))
                    Return Nothing
            End Select

            If progressBar IsNot Nothing Then
                progressBar.Invoke(Sub() progressBar.Value = 100)
            End If
            Return FinalizeFitResult(res)
        End Function

        ''' <summary>
        ''' Fits Deming regression and returns jackknife confidence intervals.
        ''' </summary>
        ''' <param name="dfForTCrit">
        ''' Optional degrees of freedom used for the t critical value. If omitted, <c>n - 2</c> is used.
        ''' </param>
        ''' <returns>
        ''' A tuple containing the intercept and slope confidence intervals.
        ''' </returns>
        Public Overloads Function FitJackknifeCI(Optional dfForTCrit As Integer? = Nothing) As (InterceptCI As ConfidenceIntervalResult, SlopeCI As ConfidenceIntervalResult)
            PrepareFilteredData()
            ValidateOptions(Me.pOptions)
            ResetNotes()

            Dim sd = BuildObservationStandardDeviationsForCurrentData()
            Dim pointFit = ComputeWeightedDemingPointEstimate(Me.pFilteredReference, Me.pFilteredTest, sd.SDx, sd.SDy, Me.pOptions)
            Dim res = FitJackknifeCore(Me.pFilteredReference, Me.pFilteredTest, sd.SDx, sd.SDy, pointFit, dfForTCrit)
            FinalizeFitResult(res)
            Return (res.InterceptCI, res.SlopeCI)
        End Function

        ''' <summary>
        ''' Computes classical closed-form analytical confidence intervals for constant-λ Deming regression.
        ''' </summary>
        ''' <returns>
        ''' A tuple containing the intercept and slope confidence intervals.
        ''' </returns>
        ''' <remarks>
        ''' For generalized weighted variance models the implementation falls back to the jackknife engine and records that choice in the notes.
        ''' </remarks>
        Public Function DemingAnalyticalCI() As (InterceptCI As ConfidenceIntervalResult, SlopeCI As ConfidenceIntervalResult)
            PrepareFilteredData()
            ValidateOptions(Me.pOptions)
            ResetNotes()

            Dim sd = BuildObservationStandardDeviationsForCurrentData()
            Dim pointFit = ComputeWeightedDemingPointEstimate(Me.pFilteredReference, Me.pFilteredTest, sd.SDx, sd.SDy, Me.pOptions)
            Dim res = FitAnalyticalCore(Me.pFilteredReference, Me.pFilteredTest, sd.SDx, sd.SDy, pointFit)
            FinalizeFitResult(res)
            Return (res.InterceptCI, res.SlopeCI)
        End Function

        ''' <summary>
        ''' Computes MCR/Linnet analytical confidence intervals for constant-λ Deming regression.
        ''' </summary>
        ''' <returns>
        ''' A tuple containing the intercept and slope confidence intervals.
        ''' </returns>
        ''' <remarks>
        ''' For generalized weighted variance models the implementation falls back to the jackknife engine and records that choice in the notes.
        ''' </remarks>
        Public Function DemingAnalyticalCI_MCR() As (InterceptCI As ConfidenceIntervalResult, SlopeCI As ConfidenceIntervalResult)
            PrepareFilteredData()
            ValidateOptions(Me.pOptions)
            ResetNotes()

            Dim sd = BuildObservationStandardDeviationsForCurrentData()
            Dim pointFit = ComputeWeightedDemingPointEstimate(Me.pFilteredReference, Me.pFilteredTest, sd.SDx, sd.SDy, Me.pOptions)
            Dim res = FitAnalyticalMcrCore(Me.pFilteredReference, Me.pFilteredTest, sd.SDx, sd.SDy, pointFit)
            FinalizeFitResult(res)
            Return (res.InterceptCI, res.SlopeCI)
        End Function

        ''' <summary>
        ''' Builds formatted result tables for worksheet output.
        ''' </summary>
        ''' <returns>
        ''' A list of <see cref="ResultTable"/> instances summarizing the fitted Deming analysis.
        ''' </returns>
        Public Function wrapResults() As List(Of ResultTable)
            If Not Me.pIsFitted OrElse Me.pResult Is Nothing Then Me.Fit()

            Dim out As New List(Of ResultTable)

            Dim t As New ResultTable
            t.AddTitle("Method Comparison Regression")
            Dim tmp = {{"Reference method", Me.pVarX},
                        {"Test method", Me.pVarY},
                        {"Number of valid data pairs", Me.pFilteredReference.Length},
                        {"Dropped non-finite pairs", Me.pDroppedPairCount},
                        {"Variance model", Me.pOptions.VarianceModel.ToString()},
                        {"Fit intercept", Me.pOptions.FitIntercept},
                        {"CI method", Me.pOptions.CiMethod.ToString()}}

            Select Case Me.pOptions.VarianceModel
                Case DemingVarianceModel.ConstantLambda
                    tmp = Matrix.HorizontalStackArrays(tmp, {{"Error ratio (λ = σx² / σy²)", Me.pOptions.Lambda}})

                Case DemingVarianceModel.ConstantCV
                    tmp = Matrix.HorizontalStackArrays(tmp,
                                                       {{"Reference CV", Me.pOptions.CVx},
                                                        {"Test CV", Me.pOptions.CVy}})

                Case DemingVarianceModel.KnownPointwiseSD
                    tmp = Matrix.HorizontalStackArrays(tmp, {{"Pointwise SD model", "Provided per observation"}})

            End Select
            t.SetBody(tmp)
            out.Add(t)

            t = New ResultTable
            t.AddTitle(Me.pResult.MethodName)
            t.SetBody({
                {Me.pResult.SlopeCI.Estimate, Me.pResult.SlopeCI.strConfidenceInterval(CIformat.LL_to_UL), "Proportional differences"},
                {Me.pResult.InterceptCI.Estimate, Me.pResult.InterceptCI.strConfidenceInterval(CIformat.LL_to_UL), "Systematic differences"}
            })
            t.AddHeaderLeftRow({"Slope", "Intercept"})
            t.AddHeaderTopRow({"Estimate", Me.pResult.SlopeCI.CIlabel, "Meaning"})
            If Not String.IsNullOrWhiteSpace(Me.pCItype) Then t.AddFootnote($"SE/CI type = {Me.pCItype}")
            If Not String.IsNullOrWhiteSpace(Me.pResult.Notes) Then t.AddFootnote(Me.pResult.Notes)
            out.Add(t)

            t = New ResultTable
            t.AddTitle("Fit Diagnostics")
            t.SetBody({
                {"Orthogonal residual SD", Me.pResult.ResidualSD}
            })
            out.Add(t)

            Return out
        End Function

        ''' <summary>
        ''' Adds a scatter plot with fitted regression line and identity line to the supplied worksheet.
        ''' </summary>
        ''' <param name="ws">Target Excel worksheet.</param>
        Public Sub AddPlot(ws As Worksheet)
            If ws Is Nothing Then AppGlobals.BSerr.LogAndThrow(New ArgumentNullException(NameOf(ws)))
            If Not Me.pIsFitted OrElse Me.pResult Is Nothing Then Me.Fit()

            Dim ch = graphics.GeneralScatterPlot(Me.pFilteredReference, Me.pFilteredTest, Me.pVarY, Me.pVarX, ws, Me.BuildMethodName())
            Dim dMinX As Double = Me.pFilteredReference.Min()
            Dim dMaxX As Double = Me.pFilteredReference.Max()

            With ch
                .HasLegend = True

                .SeriesCollection.NewSeries()
                With .SeriesCollection(2)
                    .XValues = {dMinX, dMaxX}
                    .Values = {
                        Me.pResult.InterceptCI.Estimate + Me.pResult.SlopeCI.Estimate * dMinX,
                        Me.pResult.InterceptCI.Estimate + Me.pResult.SlopeCI.Estimate * dMaxX
                    }
                    .Name = "Regression line"
                    .MarkerStyle = -4142
                    .Border.Color = RGB(255, 0, 0)
                    With .Format.Line
                        .Visible = True
                        .Weight = 1.5
                    End With
                End With

                .SeriesCollection.NewSeries()
                With .SeriesCollection(3)
                    .XValues = {dMinX, dMaxX}
                    .Values = {dMinX, dMaxX}
                    .Name = "Unity line (y = x)"
                    .MarkerStyle = -4142
                    .Border.Color = RGB(0, 0, 255)
                    With .Format.Line
                        .Visible = True
                        .DashStyle = 4
                        .Weight = 0.5
                    End With
                End With
            End With
        End Sub

        ''' <summary>
        ''' Computes York-style weighted errors-in-variables coefficients from paired data and observation-level standard deviations.
        ''' </summary>
        ''' <param name="x">Reference-method measurements.</param>
        ''' <param name="y">Test-method measurements.</param>
        ''' <param name="sdX">Observation-level standard deviations for the reference method.</param>
        ''' <param name="sdY">Observation-level standard deviations for the test method.</param>
        ''' <param name="fitIntercept">If <c>True</c>, fit <c>y = a + b x</c>; otherwise fit through the origin.</param>
        ''' <param name="maxIterations">Maximum number of fixed-point iterations.</param>
        ''' <param name="tolerance">Relative tolerance used for slope convergence.</param>
        ''' <returns>
        ''' A tuple containing the fitted intercept and slope.
        ''' </returns>
        Friend Shared Function ComputeYorkWeightedCoefficients(x As Double(),
                                                               y As Double(),
                                                               sdX As Double(),
                                                               sdY As Double(),
                                                               Optional fitIntercept As Boolean = True,
                                                               Optional maxIterations As Integer = DEFAULT_MAX_ITER,
                                                               Optional tolerance As Double = DEFAULT_TOLERANCE) As (Intercept As Double, Slope As Double)
            ValidateCoreInputs(x, y, sdX, sdY)

            Dim wx(x.Length - 1) As Double
            Dim wy(y.Length - 1) As Double
            For i As Integer = 0 To x.Length - 1
                wx(i) = 1.0 / (sdX(i) * sdX(i))
                wy(i) = 1.0 / (sdY(i) * sdY(i))
            Next

            Dim slope As Double = InitialSlopeGuess(x, y, fitIntercept)
            If Double.IsNaN(slope) OrElse Double.IsInfinity(slope) Then slope = 1.0

            For iter As Integer = 1 To maxIterations
                Dim w(x.Length - 1) As Double
                For i As Integer = 0 To x.Length - 1
                    Dim denom As Double = wx(i) + (slope * slope * wy(i))
                    If denom <= 0.0 OrElse Double.IsNaN(denom) OrElse Double.IsInfinity(denom) Then
                        AppGlobals.BSerr.LogAndThrow(New InvalidOperationException("Invalid York weight denominator encountered."))
                    End If
                    w(i) = (wx(i) * wy(i)) / denom
                Next

                Dim slopeNew As Double
                If fitIntercept Then
                    Dim xBar As Double = WeightedMean(x, w)
                    Dim yBar As Double = WeightedMean(y, w)
                    Dim num As Double = 0.0
                    Dim den As Double = 0.0
                    For i As Integer = 0 To x.Length - 1
                        Dim ui As Double = x(i) - xBar
                        Dim vi As Double = y(i) - yBar
                        Dim betaI As Double = w(i) * (ui / wy(i) + slope * vi / wx(i))
                        num += w(i) * betaI * vi
                        den += w(i) * betaI * ui
                    Next
                    If den = 0.0 Then AppGlobals.BSerr.LogAndThrow(New InvalidOperationException("The generalized Deming slope is undefined because the York denominator is zero."))
                    slopeNew = num / den
                Else
                    Dim num As Double = 0.0
                    Dim den As Double = 0.0
                    For i As Integer = 0 To x.Length - 1
                        num += w(i) * x(i) * y(i)
                        den += w(i) * x(i) * x(i)
                    Next
                    If den = 0.0 Then AppGlobals.BSerr.LogAndThrow(New InvalidOperationException("The through-origin generalized Deming slope is undefined because the weighted x sum of squares is zero."))
                    slopeNew = num / den
                End If

                If Math.Abs(slopeNew - slope) <= tolerance * Math.Max(1.0, Math.Abs(slopeNew)) Then
                    slope = slopeNew
                    Exit For
                End If
                slope = slopeNew

                If iter = maxIterations Then
                    AppGlobals.BSerr.LogAndThrow(New InvalidOperationException("York weighted Deming iteration did not converge within the configured iteration limit."))
                End If
            Next

            Dim intercept As Double
            If fitIntercept Then
                Dim w(x.Length - 1) As Double
                For i As Integer = 0 To x.Length - 1
                    Dim wxi As Double = 1.0 / (sdX(i) * sdX(i))
                    Dim wyi As Double = 1.0 / (sdY(i) * sdY(i))
                    w(i) = (wxi * wyi) / (wxi + slope * slope * wyi)
                Next
                intercept = WeightedMean(y, w) - slope * WeightedMean(x, w)
            Else
                intercept = 0.0
            End If

            Return (intercept, slope)
        End Function

        ''' <summary>
        ''' Builds observation-level standard-deviation arrays implied by the configured variance model.
        ''' </summary>
        ''' <param name="x">Reference-method measurements after finite-pair filtering.</param>
        ''' <param name="y">Test-method measurements after finite-pair filtering.</param>
        ''' <param name="opts">Deming options describing the variance model.</param>
        ''' <returns>
        ''' A tuple containing standard-deviation arrays for the x and y methods.
        ''' </returns>
        Friend Shared Function BuildObservationStandardDeviations(x As Double(),
                                                                  y As Double(),
                                                                  opts As DemingOptions) As (SDx As Double(), SDy As Double())
            If x Is Nothing Then AppGlobals.BSerr.LogAndThrow(New ArgumentNullException(NameOf(x)))
            If y Is Nothing Then AppGlobals.BSerr.LogAndThrow(New ArgumentNullException(NameOf(y)))
            If opts Is Nothing Then AppGlobals.BSerr.LogAndThrow(New ArgumentNullException(NameOf(opts)))
            If x.Length <> y.Length Then AppGlobals.BSerr.LogAndThrow(New ArgumentException("x and y must have the same length."))

            Dim n As Integer = x.Length
            Dim sx(n - 1) As Double
            Dim sy(n - 1) As Double

            Select Case opts.VarianceModel
                Case DemingVarianceModel.ConstantLambda
                    If opts.Lambda <= 0.0 OrElse Double.IsNaN(opts.Lambda) OrElse Double.IsInfinity(opts.Lambda) Then
                        AppGlobals.BSerr.LogAndThrow(New ArgumentOutOfRangeException(NameOf(opts.Lambda), "Lambda must be finite and > 0."))
                    End If
                    Dim sdXConst As Double = Math.Sqrt(opts.Lambda)
                    For i As Integer = 0 To n - 1
                        sx(i) = sdXConst
                        sy(i) = 1.0
                    Next

                Case DemingVarianceModel.KnownPointwiseSD
                    If opts.SDx Is Nothing OrElse opts.SDy Is Nothing Then
                        AppGlobals.BSerr.LogAndThrow(New ArgumentException("KnownPointwiseSD requires both SDx and SDy arrays."))
                    End If
                    If opts.SDx.Length <> n OrElse opts.SDy.Length <> n Then
                        AppGlobals.BSerr.LogAndThrow(New ArgumentException("SDx and SDy must have the same length as x and y."))
                    End If
                    For i As Integer = 0 To n - 1
                        sx(i) = SanitizeStandardDeviation(opts.SDx(i))
                        sy(i) = SanitizeStandardDeviation(opts.SDy(i))
                    Next

                Case DemingVarianceModel.ConstantCV
                    If opts.CVx <= 0.0 OrElse Double.IsNaN(opts.CVx) OrElse Double.IsInfinity(opts.CVx) Then
                        AppGlobals.BSerr.LogAndThrow(New ArgumentOutOfRangeException(NameOf(opts.CVx), "CVx must be finite and > 0 for ConstantCV."))
                    End If
                    If opts.CVy <= 0.0 OrElse Double.IsNaN(opts.CVy) OrElse Double.IsInfinity(opts.CVy) Then
                        AppGlobals.BSerr.LogAndThrow(New ArgumentOutOfRangeException(NameOf(opts.CVy), "CVy must be finite and > 0 for ConstantCV."))
                    End If
                    For i As Integer = 0 To n - 1
                        sx(i) = Math.Max(Math.Abs(x(i)) * opts.CVx, DEFAULT_SD_FLOOR)
                        sy(i) = Math.Max(Math.Abs(y(i)) * opts.CVy, DEFAULT_SD_FLOOR)
                    Next

                Case Else
                    AppGlobals.BSerr.LogAndThrow(New NotSupportedException($"Unsupported variance model: {opts.VarianceModel}."))
            End Select

            Return (sx, sy)
        End Function

        Private Function FitAnalyticalCore(x As Double(),
                                           y As Double(),
                                           sdX As Double(),
                                           sdY As Double(),
                                           pointFit As (Intercept As Double, Slope As Double)) As MethodComparisonFitResult
            If Me.pOptions.VarianceModel = DemingVarianceModel.ConstantLambda AndAlso Me.pOptions.FitIntercept Then
                Return FitClassicalAnalyticalClosedFormCore(x, y)
            End If

            Me.pComputationNotes.Add("Exact analytical CIs are not yet implemented for the selected generalized variance model; jackknife CI was used instead.")
            Return FitJackknifeCore(x, y, sdX, sdY, pointFit)
        End Function

        Private Function FitAnalyticalMcrCore(x As Double(),
                                              y As Double(),
                                              sdX As Double(),
                                              sdY As Double(),
                                              pointFit As (Intercept As Double, Slope As Double)) As MethodComparisonFitResult
            If Me.pOptions.VarianceModel = DemingVarianceModel.ConstantLambda AndAlso Me.pOptions.FitIntercept Then
                Return FitClassicalAnalyticalMcrCore(x, y)
            End If

            Me.pComputationNotes.Add("Linnet/MCR analytical CIs are not yet implemented for the selected generalized variance model; jackknife CI was used instead.")
            Return FitJackknifeCore(x, y, sdX, sdY, pointFit)
        End Function

        Private Function FitJackknifeCore(x As Double(),
                                          y As Double(),
                                          sdXin As Double(),
                                          sdYin As Double(),
                                          pointFit As (Intercept As Double, Slope As Double),
                                          Optional dfForTCrit As Integer? = Nothing) As MethodComparisonFitResult
            Dim n As Integer = x.Length
            Dim jackIntercept(n - 1) As Double
            Dim jackSlope(n - 1) As Double

            For i As Integer = 0 To n - 1
                Dim xx = ExcludeIndex(x, i)
                Dim yy = ExcludeIndex(y, i)
                Dim sdx = ExcludeIndex(sdXin, i)
                Dim sdy = ExcludeIndex(sdYin, i)
                Dim fitI = ComputeWeightedDemingPointEstimate(xx, yy, sdx, sdy, Me.pOptions)
                jackIntercept(i) = fitI.Intercept
                jackSlope(i) = fitI.Slope
            Next

            Dim seIntercept As Double = JackknifeStdErr(jackIntercept)
            Dim seSlope As Double = JackknifeStdErr(jackSlope)
            Dim df As Integer = If(dfForTCrit.HasValue, dfForTCrit.Value, n - 2)
            If df < 1 Then df = 1
            Dim tCrit As Double = distributions.T_Inv(1.0 - Me.pOptions.Alpha / 2.0, df)

            Dim interceptCI As New ConfidenceIntervalResult With {
                .Estimate = pointFit.Intercept,
                .alpha = Me.pOptions.Alpha,
                .StdErr = seIntercept,
                .LowerLimit = pointFit.Intercept - tCrit * seIntercept,
                .UpperLimit = pointFit.Intercept + tCrit * seIntercept
            }
            Dim slopeCI As New ConfidenceIntervalResult With {
                .Estimate = pointFit.Slope,
                .alpha = Me.pOptions.Alpha,
                .StdErr = seSlope,
                .LowerLimit = pointFit.Slope - tCrit * seSlope,
                .UpperLimit = pointFit.Slope + tCrit * seSlope
            }

            Me.pInterceptSE = seIntercept
            Me.pSlopeSE = seSlope
            Me.pCItype = "Jackknife"

            Return New MethodComparisonFitResult With {
                .InterceptCI = interceptCI,
                .SlopeCI = slopeCI,
                .MethodName = BuildMethodName(),
                .ResidualSD = ComputeOrthogonalResidualSD(x, y, pointFit.Intercept, pointFit.Slope)
            }
        End Function

        Private Function FitBootstrapCore(x As Double(),
                                          y As Double(),
                                          sdXin As Double(),
                                          sdYin As Double(),
                                          pointFit As (Intercept As Double, Slope As Double),
                                          Optional progressBar As System.Windows.Forms.ProgressBar = Nothing,
                                          Optional randomSeed As Integer = Integer.MinValue) As MethodComparisonFitResult
            Dim b As Integer = Math.Max(200, Me.pOptions.BootstrapReplicates)
            Dim n As Integer = x.Length
            Dim bootIntercept(b - 1) As Double
            Dim bootSlope(b - 1) As Double
            Dim rng = AppGlobals.CreateRandom(randomSeed)

            If progressBar IsNot Nothing Then
                progressBar.Invoke(Sub() progressBar.Value = 0)
            End If

            For r As Integer = 0 To b - 1
                Dim xx(n - 1) As Double
                Dim yy(n - 1) As Double
                Dim sdx_(n - 1) As Double
                Dim sdy_(n - 1) As Double
                For i As Integer = 0 To n - 1
                    Dim idx As Integer = rng.Next(0, n)
                    xx(i) = x(idx)
                    yy(i) = y(idx)
                    sdx_(i) = sdXin(idx)
                    sdy_(i) = sdYin(idx)
                Next
                Dim fitR = ComputeWeightedDemingPointEstimate(xx, yy, sdx_, sdy_, Me.pOptions)
                bootIntercept(r) = fitR.Intercept
                bootSlope(r) = fitR.Slope

                If progressBar IsNot Nothing Then
                    Dim progressValue As Integer = CInt(Math.Min(100.0, Math.Round(100.0 * (r + 1) / b)))
                    progressBar.Invoke(Sub() progressBar.Value = progressValue)
                End If
            Next

            If Me.pOptions.CiMethod = AgreementCiMethod.BootstrapBCa Then
                Me.pComputationNotes.Add("BCa bootstrap is not yet implemented separately for generalized Deming regression; percentile bootstrap limits were used.")
            End If

            Dim interceptCI = PercentileConfidenceInterval(pointFit.Intercept, bootIntercept, Me.pOptions.Alpha)
            Dim slopeCI = PercentileConfidenceInterval(pointFit.Slope, bootSlope, Me.pOptions.Alpha)
            Me.pInterceptSE = interceptCI.StdErr
            Me.pSlopeSE = slopeCI.StdErr
            Me.pCItype = If(Me.pOptions.CiMethod = AgreementCiMethod.BootstrapBCa, "Bootstrap BCa fallback (percentile)", "Bootstrap percentile")

            Return New MethodComparisonFitResult With {
                .InterceptCI = interceptCI,
                .SlopeCI = slopeCI,
                .MethodName = BuildMethodName(),
                .ResidualSD = ComputeOrthogonalResidualSD(x, y, pointFit.Intercept, pointFit.Slope)
            }
        End Function

        Private Function FitClassicalAnalyticalClosedFormCore(x As Double(), y As Double()) As MethodComparisonFitResult
            Dim n As Integer = x.Length
            If n < 3 Then AppGlobals.BSerr.LogAndThrow(New ArgumentException("At least 3 observations are required (n>=3)."))
            If Me.pOptions.Lambda <= 0 OrElse Double.IsNaN(Me.pOptions.Lambda) OrElse Double.IsInfinity(Me.pOptions.Lambda) Then
                AppGlobals.BSerr.LogAndThrow(New ArgumentOutOfRangeException(NameOf(Me.pOptions.Lambda), "Error ratio (Lambda) must be finite and > 0."))
            End If

            Me.pCItype = "Analytical (closed form / linearization)"
            Dim delta As Double = 1.0 / Me.pOptions.Lambda
            Dim xbar As Double = x.Average()
            Dim ybar As Double = y.Average()
            Dim Sxx As Double = 0.0, Syy As Double = 0.0, Sxy As Double = 0.0
            For i As Integer = 0 To n - 1
                Dim dx As Double = x(i) - xbar
                Dim dy As Double = y(i) - ybar
                Sxx += dx * dx
                Syy += dy * dy
                Sxy += dx * dy
            Next
            Sxx /= (n - 1)
            Syy /= (n - 1)
            Sxy /= (n - 1)
            If Sxy = 0.0 Then AppGlobals.BSerr.LogAndThrow(New InvalidOperationException("Deming slope is undefined because Sxy = 0 (degenerate association)."))

            Dim A As Double = Syy - delta * Sxx
            Dim disc As Double = A * A + 4.0 * delta * Sxy * Sxy
            Dim root As Double = Math.Sqrt(disc)
            Dim sgn As Double = If(Sxy >= 0.0, 1.0, -1.0)
            Dim slope As Double = (A + sgn * root) / (2.0 * Sxy)
            Dim intercept As Double = ybar - slope * xbar

            Dim D As Double = delta + slope * slope
            Dim sseOverD As Double = 0.0
            Dim resid(n - 1) As Double
            For i As Integer = 0 To n - 1
                resid(i) = y(i) - intercept - slope * x(i)
                sseOverD += (resid(i) * resid(i)) / D
            Next
            Dim s2 As Double = sseOverD / Math.Max(1, n - 2)

            Dim invSqrtD As Double = 1.0 / Math.Sqrt(D)
            Dim invD32 As Double = 1.0 / (D * Math.Sqrt(D))
            Dim a11 As Double = 0.0, a12 As Double = 0.0, a22 As Double = 0.0
            For i As Integer = 0 To n - 1
                Dim j1 As Double = -invSqrtD
                Dim j2 As Double = -(x(i) * invSqrtD) - (slope * resid(i)) * invD32
                a11 += j1 * j1
                a12 += j1 * j2
                a22 += j2 * j2
            Next

            Dim det As Double = a11 * a22 - a12 * a12
            If det <= 0 OrElse Double.IsNaN(det) OrElse Double.IsInfinity(det) Then
                AppGlobals.BSerr.LogAndThrow(New InvalidOperationException("Failed to invert information matrix for analytical SE."))
            End If
            Dim inv00 As Double = a22 / det
            Dim inv11 As Double = a11 / det
            Dim seIntercept As Double = Math.Sqrt(Math.Max(0.0, s2 * inv00))
            Dim seSlope As Double = Math.Sqrt(Math.Max(0.0, s2 * inv11))
            Dim tCrit As Double = distributions.T_Inv(1.0 - Me.pOptions.Alpha / 2.0, Math.Max(1, n - 2))

            Dim interceptCI As New ConfidenceIntervalResult With {
                .Estimate = intercept,
                .alpha = Me.pOptions.Alpha,
                .StdErr = seIntercept,
                .LowerLimit = intercept - tCrit * seIntercept,
                .UpperLimit = intercept + tCrit * seIntercept
            }
            Dim slopeCI As New ConfidenceIntervalResult With {
                .Estimate = slope,
                .alpha = Me.pOptions.Alpha,
                .StdErr = seSlope,
                .LowerLimit = slope - tCrit * seSlope,
                .UpperLimit = slope + tCrit * seSlope
            }

            Me.pInterceptSE = seIntercept
            Me.pSlopeSE = seSlope
            Return New MethodComparisonFitResult With {
                .InterceptCI = interceptCI,
                .SlopeCI = slopeCI,
                .MethodName = BuildMethodName(),
                .ResidualSD = ComputeOrthogonalResidualSD(x, y, intercept, slope)
            }
        End Function

        Private Function FitClassicalAnalyticalMcrCore(x As Double(), y As Double()) As MethodComparisonFitResult
            Dim n As Integer = x.Length
            If n < 3 Then AppGlobals.BSerr.LogAndThrow(New ArgumentException("Deming analytical CI requires at least 3 observations (df = n-2)."))

            Dim fit = ComputeClassicDemingCoefficients(x, y, Me.pOptions.Lambda)
            Dim b0 As Double = fit.Intercept
            Dim b1 As Double = fit.Slope
            Dim jackB0(n - 1) As Double
            Dim jackB1(n - 1) As Double

            For i As Integer = 0 To n - 1
                Dim xLOO = ExcludeIndex(x, i)
                Dim yLOO = ExcludeIndex(y, i)
                Dim fitI = ComputeClassicDemingCoefficients(xLOO, yLOO, Me.pOptions.Lambda)
                jackB0(i) = fitI.Intercept
                jackB1(i) = fitI.Slope
            Next

            Dim pvB0(n - 1) As Double
            Dim pvB1(n - 1) As Double
            For i As Integer = 0 To n - 1
                pvB0(i) = n * b0 - (n - 1) * jackB0(i)
                pvB1(i) = n * b1 - (n - 1) * jackB1(i)
            Next

            Dim seB0 As Double = StatFunc.stDev(pvB0) / Math.Sqrt(n)
            Dim seB1 As Double = StatFunc.stDev(pvB1) / Math.Sqrt(n)
            Dim tCrit As Double = distributions.T_Inv(1.0 - Me.pOptions.Alpha / 2.0, Math.Max(1, n - 2))

            Dim interceptCI As New ConfidenceIntervalResult With {
                .Estimate = b0,
                .alpha = Me.pOptions.Alpha,
                .StdErr = seB0,
                .LowerLimit = b0 - tCrit * seB0,
                .UpperLimit = b0 + tCrit * seB0
            }
            Dim slopeCI As New ConfidenceIntervalResult With {
                .Estimate = b1,
                .alpha = Me.pOptions.Alpha,
                .StdErr = seB1,
                .LowerLimit = b1 - tCrit * seB1,
                .UpperLimit = b1 + tCrit * seB1
            }

            Me.pInterceptSE = seB0
            Me.pSlopeSE = seB1
            Me.pCItype = "Analytical – Linnet (jackknife pseudo-values)"
            Return New MethodComparisonFitResult With {
                .InterceptCI = interceptCI,
                .SlopeCI = slopeCI,
                .MethodName = BuildMethodName(),
                .ResidualSD = ComputeOrthogonalResidualSD(x, y, b0, b1)
            }
        End Function

        Private Function ComputeWeightedDemingPointEstimate(x As Double(),
                                                            y As Double(),
                                                            sdX As Double(),
                                                            sdY As Double(),
                                                            opts As DemingOptions) As (Intercept As Double, Slope As Double)
            If opts Is Nothing Then AppGlobals.BSerr.LogAndThrow(New ArgumentNullException(NameOf(opts)))

            If opts.VarianceModel = DemingVarianceModel.ConstantLambda AndAlso opts.FitIntercept Then
                Return ComputeClassicDemingCoefficients(x, y, opts.Lambda)
            End If

            Return ComputeYorkWeightedCoefficients(x, y, sdX, sdY, opts.FitIntercept)
        End Function

        Private Function FinalizeFitResult(res As MethodComparisonFitResult) As MethodComparisonFitResult
            If Me.pUsedBootstrapCi Then
                Me.pComputationNotes.Add($"Bootstrap seed = {Me.pBootstrapSeedUsed}.")
            End If
            res.MethodName = BuildMethodName()
            res.Notes = String.Join(Environment.NewLine, Me.pComputationNotes)
            Me.pResult = res
            Me.pIsFitted = True
            Return res
        End Function

        Private Sub InvalidateFit()
            Me.pResult = Nothing
            Me.pIsFitted = False
            Me.pInterceptSE = Double.NaN
            Me.pSlopeSE = Double.NaN
            Me.pCItype = String.Empty
            Me.pUsedBootstrapCi = False
            Me.pBootstrapSeedUsed = Integer.MinValue
        End Sub

        Private Sub ResetNotes()
            Me.pComputationNotes.Clear()
            If Me.pDroppedPairCount > 0 Then
                Me.pComputationNotes.Add($"Dropped {Me.pDroppedPairCount} non-finite pair(s) by pairwise complete-case filtering.")
            End If
        End Sub

        Private Function BuildObservationStandardDeviationsForCurrentData() As (SDx As Double(), SDy As Double())
            If Me.pFilteredReference Is Nothing OrElse Me.pFilteredTest Is Nothing Then PrepareFilteredData()
            If Me.pOptions.VarianceModel <> DemingVarianceModel.KnownPointwiseSD Then
                Return BuildObservationStandardDeviations(Me.pFilteredReference, Me.pFilteredTest, Me.pOptions)
            End If

            If Me.pOptions.SDx Is Nothing OrElse Me.pOptions.SDy Is Nothing Then
                AppGlobals.BSerr.LogAndThrow(New ArgumentException("KnownPointwiseSD requires both SDx and SDy arrays."))
            End If
            If Me.pKeptPairIndices Is Nothing Then
                AppGlobals.BSerr.LogAndThrow(New InvalidOperationException("Internal kept-pair index map is not available."))
            End If
            If Me.pOptions.SDx.Length <> Me.pRawReference.Length OrElse Me.pOptions.SDy.Length <> Me.pRawReference.Length Then
                AppGlobals.BSerr.LogAndThrow(New ArgumentException("For KnownPointwiseSD, SDx and SDy must match the original unfiltered data length."))
            End If

            Dim n As Integer = Me.pKeptPairIndices.Length
            Dim sx(n - 1) As Double
            Dim sy(n - 1) As Double
            For i As Integer = 0 To n - 1
                Dim idx As Integer = Me.pKeptPairIndices(i)
                sx(i) = SanitizeStandardDeviation(Me.pOptions.SDx(idx))
                sy(i) = SanitizeStandardDeviation(Me.pOptions.SDy(idx))
            Next
            Return (sx, sy)
        End Function

        Private Shared Function ComputeClassicDemingCoefficients(x As Double(), y As Double(), errorRatio As Double) As (Intercept As Double, Slope As Double)
            Dim n As Integer = x.Length
            If n <> y.Length Then AppGlobals.BSerr.LogAndThrow(New ArgumentException("x and y must have the same length."))
            If n < 2 Then Return (Double.NaN, Double.NaN)
            If errorRatio <= 0.0 OrElse Double.IsNaN(errorRatio) OrElse Double.IsInfinity(errorRatio) Then Return (Double.NaN, Double.NaN)

            Dim delta As Double = 1.0 / errorRatio
            Dim xBar As Double = x.Average()
            Dim yBar As Double = y.Average()
            Dim Sxx As Double = 0.0
            Dim Syy As Double = 0.0
            Dim Sxy As Double = 0.0

            For i As Integer = 0 To n - 1
                Dim dx As Double = x(i) - xBar
                Dim dy As Double = y(i) - yBar
                Sxx += dx * dx
                Syy += dy * dy
                Sxy += dx * dy
            Next

            If Sxy = 0.0 Then Return (Double.NaN, Double.NaN)
            Dim A As Double = Syy - delta * Sxx
            Dim B As Double = 2.0 * Sxy
            Dim disc As Double = A * A + 4.0 * delta * Sxy * Sxy
            Dim root As Double = Math.Sqrt(disc)
            Dim sgn As Double = If(Sxy >= 0.0, 1.0, -1.0)
            Dim slope As Double = (A + sgn * root) / B
            Dim intercept As Double = yBar - slope * xBar
            Return (intercept, slope)
        End Function

        Private Sub PrepareFilteredData()
            Dim filtered = FilterFinitePairs(Me.pRawReference, Me.pRawTest)
            Me.pFilteredReference = filtered.Reference
            Me.pFilteredTest = filtered.Test
            Me.pKeptPairIndices = filtered.KeptIndices
            Me.pDroppedPairCount = filtered.DroppedCount
            If Me.pFilteredReference.Length < 3 Then
                AppGlobals.BSerr.LogAndThrow(New InvalidOperationException("Fewer than 3 finite paired observations remain after filtering."))
            End If
        End Sub

        Private Shared Function FilterFinitePairs(x As Double(), y As Double()) As (Reference As Double(), Test As Double(), KeptIndices As Integer(), DroppedCount As Integer)
            If x Is Nothing Then AppGlobals.BSerr.LogAndThrow(New ArgumentNullException(NameOf(x)))
            If y Is Nothing Then AppGlobals.BSerr.LogAndThrow(New ArgumentNullException(NameOf(y)))
            If x.Length <> y.Length Then AppGlobals.BSerr.LogAndThrow(New ArgumentException("x and y must have the same length."))

            Dim keepX As New List(Of Double)
            Dim keepY As New List(Of Double)
            Dim keptIdx As New List(Of Integer)
            Dim dropped As Integer = 0
            For i As Integer = 0 To x.Length - 1
                If Double.IsNaN(x(i)) OrElse Double.IsInfinity(x(i)) OrElse Double.IsNaN(y(i)) OrElse Double.IsInfinity(y(i)) Then
                    dropped += 1
                Else
                    keepX.Add(x(i))
                    keepY.Add(y(i))
                    keptIdx.Add(i)
                End If
            Next

            Return (keepX.ToArray(), keepY.ToArray(), keptIdx.ToArray(), dropped)
        End Function

        Private Shared Sub ValidateOptions(opts As DemingOptions)
            If opts Is Nothing Then AppGlobals.BSerr.LogAndThrow(New ArgumentNullException(NameOf(opts)))
            If opts.Alpha <= 0.0 OrElse opts.Alpha >= 1.0 Then
                AppGlobals.BSerr.LogAndThrow(New ArgumentOutOfRangeException(NameOf(opts.Alpha), "Alpha must be in (0,1)."))
            End If
            If opts.BootstrapReplicates < 50 Then
                AppGlobals.BSerr.LogAndThrow(New ArgumentOutOfRangeException(NameOf(opts.BootstrapReplicates), "BootstrapReplicates must be at least 50."))
            End If
            If opts.VarianceModel = DemingVarianceModel.ConstantLambda Then
                If opts.Lambda <= 0.0 OrElse Double.IsNaN(opts.Lambda) OrElse Double.IsInfinity(opts.Lambda) Then
                    AppGlobals.BSerr.LogAndThrow(New ArgumentOutOfRangeException(NameOf(opts.Lambda), "Lambda must be finite and > 0."))
                End If
            End If
        End Sub

        Private Shared Sub ValidateCoreInputs(x As Double(), y As Double(), sdX As Double(), sdY As Double())
            If x Is Nothing Then AppGlobals.BSerr.LogAndThrow(New ArgumentNullException(NameOf(x)))
            If y Is Nothing Then AppGlobals.BSerr.LogAndThrow(New ArgumentNullException(NameOf(y)))
            If sdX Is Nothing Then AppGlobals.BSerr.LogAndThrow(New ArgumentNullException(NameOf(sdX)))
            If sdY Is Nothing Then AppGlobals.BSerr.LogAndThrow(New ArgumentNullException(NameOf(sdY)))
            If x.Length <> y.Length OrElse x.Length <> sdX.Length OrElse x.Length <> sdY.Length Then
                AppGlobals.BSerr.LogAndThrow(New ArgumentException("All arrays must have the same length."))
            End If
            If x.Length < 2 Then
                AppGlobals.BSerr.LogAndThrow(New ArgumentException("At least 2 paired observations are required."))
            End If
            For i As Integer = 0 To sdX.Length - 1
                If sdX(i) <= 0.0 OrElse Double.IsNaN(sdX(i)) OrElse Double.IsInfinity(sdX(i)) Then
                    AppGlobals.BSerr.LogAndThrow(New ArgumentException("All SDx values must be finite and > 0."))
                End If
                If sdY(i) <= 0.0 OrElse Double.IsNaN(sdY(i)) OrElse Double.IsInfinity(sdY(i)) Then
                    AppGlobals.BSerr.LogAndThrow(New ArgumentException("All SDy values must be finite and > 0."))
                End If
            Next
        End Sub

        Private Shared Function InitialSlopeGuess(x As Double(), y As Double(), fitIntercept As Boolean) As Double
            If fitIntercept Then
                Dim xBar As Double = x.Average()
                Dim yBar As Double = y.Average()
                Dim sxx As Double = 0.0
                Dim sxy As Double = 0.0
                For i As Integer = 0 To x.Length - 1
                    Dim dx As Double = x(i) - xBar
                    Dim dy As Double = y(i) - yBar
                    sxx += dx * dx
                    sxy += dx * dy
                Next
                If sxx = 0.0 Then Return Double.NaN
                Return sxy / sxx
            Else
                Dim num As Double = 0.0
                Dim den As Double = 0.0
                For i As Integer = 0 To x.Length - 1
                    num += x(i) * y(i)
                    den += x(i) * x(i)
                Next
                If den = 0.0 Then Return Double.NaN
                Return num / den
            End If
        End Function

        Private Shared Function WeightedMean(values As Double(), weights As Double()) As Double
            Dim sw As Double = 0.0
            Dim swx As Double = 0.0
            For i As Integer = 0 To values.Length - 1
                sw += weights(i)
                swx += weights(i) * values(i)
            Next
            If sw = 0.0 Then AppGlobals.BSerr.LogAndThrow(New InvalidOperationException("Weighted mean is undefined because the total weight is zero."))
            Return swx / sw
        End Function

        Private Shared Function SanitizeStandardDeviation(sd As Double) As Double
            If Double.IsNaN(sd) OrElse Double.IsInfinity(sd) OrElse sd <= 0.0 Then
                AppGlobals.BSerr.LogAndThrow(New ArgumentOutOfRangeException(NameOf(sd), "Standard deviations must be finite and > 0."))
            End If
            Return Math.Max(sd, DEFAULT_SD_FLOOR)
        End Function

        Private Shared Function PercentileConfidenceInterval(estimate As Double,
                                                             bootstrapEstimates As Double(),
                                                             alpha As Double) As ConfidenceIntervalResult
            Dim sorted = DirectCast(bootstrapEstimates.Clone(), Double())
            Array.Sort(sorted)
            Dim lower As Double = QuantileSorted(sorted, alpha / 2.0)
            Dim upper As Double = QuantileSorted(sorted, 1.0 - alpha / 2.0)
            Dim se As Double = StatFunc.stDev(sorted)
            Return New ConfidenceIntervalResult With {
                .Estimate = estimate,
                .alpha = alpha,
                .StdErr = se,
                .LowerLimit = lower,
                .UpperLimit = upper
            }
        End Function

        Private Shared Function QuantileSorted(sortedValues As Double(), p As Double) As Double
            If sortedValues Is Nothing OrElse sortedValues.Length = 0 Then
                AppGlobals.BSerr.LogAndThrow(New ArgumentException("At least one sorted value is required."))
            End If
            Dim pp As Double = Math.Min(1.0, Math.Max(0.0, p))
            Dim h As Double = (sortedValues.Length - 1) * pp
            Dim lo As Integer = CInt(Math.Floor(h))
            Dim hi As Integer = CInt(Math.Ceiling(h))
            If lo = hi Then Return sortedValues(lo)
            Dim frac As Double = h - lo
            Return sortedValues(lo) + frac * (sortedValues(hi) - sortedValues(lo))
        End Function

        Private Shared Function JackknifeStdErr(leaveOneOutEstimates As Double()) As Double
            Dim n As Integer = leaveOneOutEstimates.Length
            Dim meanTheta As Double = leaveOneOutEstimates.Average()
            Dim ss As Double = 0.0
            For i As Integer = 0 To n - 1
                Dim d As Double = leaveOneOutEstimates(i) - meanTheta
                ss += d * d
            Next
            Return Math.Sqrt(((n - 1.0) / n) * ss)
        End Function

        Private Shared Function ExcludeIndex(values As Double(), indexToExclude As Integer) As Double()
            Dim out(values.Length - 2) As Double
            Dim t As Integer = 0
            For i As Integer = 0 To values.Length - 1
                If i = indexToExclude Then Continue For
                out(t) = values(i)
                t += 1
            Next
            Return out
        End Function

        Private Shared Function ComputeOrthogonalResidualSD(x As Double(),
                                                            y As Double(),
                                                            intercept As Double,
                                                            slope As Double) As Double
            Dim n As Integer = x.Length
            Dim p As Integer = If(Double.IsNaN(intercept), 1, 2)
            Dim denom As Integer = Math.Max(1, n - p)
            Dim ss As Double = 0.0
            Dim scale As Double = 1.0 + slope * slope
            For i As Integer = 0 To n - 1
                Dim v As Double = y(i) - intercept - slope * x(i)
                ss += (v * v) / scale
            Next
            Return Math.Sqrt(ss / denom)
        End Function

        Friend Shared Function ResolveRandomSeed(requestedSeed As Integer) As Integer
            If requestedSeed <> Integer.MinValue Then Return requestedSeed

            Dim globalSeed As Integer = AppGlobals.DefaultRandomSeed
            If globalSeed <> Integer.MinValue Then Return globalSeed

            Return Environment.TickCount
        End Function

        Private Function BuildMethodName() As String
            Select Case Me.pOptions.VarianceModel
                Case DemingVarianceModel.ConstantLambda
                    Return If(Me.pOptions.FitIntercept, "Weighted Deming Regression (constant λ)", "Weighted Deming Regression Through Origin (constant λ)")
                Case DemingVarianceModel.KnownPointwiseSD
                    Return If(Me.pOptions.FitIntercept, "Weighted Deming Regression (known pointwise SD)", "Weighted Deming Regression Through Origin (known pointwise SD)")
                Case DemingVarianceModel.ConstantCV
                    Return If(Me.pOptions.FitIntercept, "Weighted Deming Regression (constant CV)", "Weighted Deming Regression Through Origin (constant CV)")
                Case Else
                    Return "Weighted Deming Regression"
            End Select
        End Function

    End Class

    ''' <summary>
    ''' Alias exposing the broader “generalized Deming” name for the weighted Deming back-end.
    ''' </summary>
    Public Class GeneralizedDemingRegression
        Inherits WeightedDemingRegression

        ''' <summary>
        ''' Initializes a generalized Deming-regression object.
        ''' </summary>
        Public Sub New(dataX As Double(),
                       dataY As Double(),
                       varX As String,
                       varY As String,
                       Optional opts As DemingOptions = Nothing)
            MyBase.New(dataX, dataY, varX, varY, opts)
        End Sub
    End Class

End Namespace
