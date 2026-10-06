Option Explicit On
Option Strict On

Imports System.Diagnostics
Imports BESHStatNG.AppInfrastructure
Imports BESHStatNG.regression

''' <summary>
''' Zero-Inflated Poisson (ZIP) regression fitted by an EM algorithm combining a Poisson count model and a logistic zero model.
''' </summary>
''' <remarks>
''' <h3>Model</h3>
''' <para>
''' For observation <c>i</c>, let:
''' </para>
''' <list type="bullet">
''' <item><description><c>λᵢ = exp(xᵢᵀ β)</c> be the Poisson mean (log link).</description></item>
''' <item><description><c>πᵢ = logistic(zᵢᵀ γ)</c> be the probability of belonging to the “structural zero” component (logit link).</description></item>
''' </list>
''' <para>
''' The ZIP pmf is:
''' </para>
''' <para>
''' <c>P(Yᵢ=0) = πᵢ + (1−πᵢ)·exp(−λᵢ)</c>
''' </para>
''' <para>
''' <c>P(Yᵢ=k&gt;0) = (1−πᵢ)·exp(−λᵢ)·λᵢ^k / k!</c>
''' </para>
'''
''' <h3>EM algorithm as implemented</h3>
''' <para>
''' Introduce latent indicator <c>Sᵢ</c> where <c>Sᵢ=1</c> means “structural zero” and <c>Sᵢ=0</c> means “Poisson component”.
''' For nonzero counts, <c>P(Sᵢ=1|Yᵢ&gt;0)=0</c>.
''' For <c>Yᵢ=0</c>:
''' </para>
''' <para>
''' <c>τᵢ = P(Sᵢ=1 | Yᵢ=0) = πᵢ / ( πᵢ + (1−πᵢ)·P_Pois(0; λᵢ) )</c>
''' </para>
''' <para>
''' where <c>P_Pois(0; λ)=exp(−λ)</c>.
''' </para>
''' <para>
''' The code stores <c>τᵢ</c> in <c>probi(i)</c> and <c>1−τᵢ</c> in <c>probi1(i)</c>.
''' </para>
''' <para>
''' M-step updates are performed by fitting two GLMs:
''' </para>
''' <list type="bullet">
''' <item><description><b>Poisson</b>: fit on the original counts with observation weights <c>probi1</c>
''' (posterior probability of the Poisson component).</description></item>
''' <item><description><b>Logistic</b>: fit on a “fractional” response column set to <c>probi</c>
''' (posterior probability of structural-zero membership), using a Binomial/logit GLM.</description></item>
''' </list>
'''
''' <h3>Acceleration (over-relaxation) and monotone fallback</h3>
''' <para>
''' After computing the plain EM parameter updates, the code attempts an over-relaxed step:
''' </para>
''' <para><c>θ_try = θ_old + s(θ_new − θ_old)</c> with <c>s=1.2</c></para>
''' <para>
''' and backtracks toward <c>s=1</c> if the observed-data log-likelihood decreases, guaranteeing monotonicity.
''' </para>
''' </remarks>
Public Class ZeroInflatedPoisson

    ''' <summary>
    ''' Optional starting values for the Poisson (count) part coefficients <c>β</c>.
    ''' </summary>
    ''' <remarks>
    ''' Used when calling <see cref="Fit"/> with <c>bStartParamsPois:=True</c>.
    ''' </remarks>
    Public startParamsPois() As Double = Nothing

    ''' <summary>
    ''' Optional starting values for the Logistic (zero) part coefficients <c>γ</c>.
    ''' </summary>
    ''' <remarks>
    ''' Used when calling <see cref="Fit"/> with <c>bStartParamsLog:=True</c>.
    ''' </remarks>
    Public startParamsLog() As Double = Nothing

    ''' <summary>
    ''' If <c>True</c>, ZIP residuals are computed after fitting and exposed via <see cref="AllResiduals"/>.
    ''' </summary>
    Public bComputeResiduals As Boolean = False

    ''' <summary>
    ''' If <c>True</c>, retains EM iteration history and includes it in <see cref="wrapResults"/>.
    ''' </summary>
    Public bIterationDetails As Boolean = False

    ''' <summary>
    ''' If <c>True</c>, includes an additional covariance/diagnostic output table (when available) in <see cref="wrapResults"/>.
    ''' </summary>
    Public bReturnCov As Boolean = False

    ''' <summary>
    ''' Result object for the Poisson (count) component of the ZIP model.
    ''' </summary>
    Public resultsPoisson As LMresult 'Zip model resutls for Poisson/Count part

    ''' <summary>
    ''' Result object for the Logistic (zero) component of the ZIP model.
    ''' </summary>
    Public resultsLogistic As LMresult 'Zip model resutls for Logistic/Zero part

    ''' <summary>
    ''' Returns the final observed-data log-likelihood of the fitted zero-inflated Poisson model.
    ''' </summary>
    Public ReadOnly Property LogLikelihood() As Double
        Get
            Return Me.pLogLikelihood
        End Get
    End Property

    ''' <summary>
    ''' Returns the final residual deviance, defined in this implementation as <c>-2 log L</c>.
    ''' </summary>
    Public ReadOnly Property ResidualDeviance() As Double
        Get
            Return Me.pFinalDeviance
        End Get
    End Property

    ''' <summary>
    ''' Returns the number of outer EM iterations performed during fitting.
    ''' </summary>
    Public ReadOnly Property EMiterations() As Integer
        Get
            Return Me.pEMiterations
        End Get
    End Property

    ''' <summary>
    ''' Returns the final relative log-likelihood change used by the stopping rule.
    ''' </summary>
    Public ReadOnly Property LastRelativeLogLikelihoodChange() As Double
        Get
            Return Me.pLastIterLLchange
        End Get
    End Property

    ''' <summary>
    ''' Returns whether the ZIP fitting algorithm satisfied its convergence criterion.
    ''' </summary>
    Public ReadOnly Property Converged() As Boolean
        Get
            Return Me.pConverged
        End Get
    End Property

    ''' <summary>
    ''' Returns the total computational time in seconds.
    ''' </summary>
    Public ReadOnly Property ComputationalTimeSeconds() As Double
        Get
            Return Me.CompTime
        End Get
    End Property

    ''' <summary>
    ''' Returns the final logistic model used for the zero-inflation component.
    ''' </summary>
    Public ReadOnly Property FinalZeroComponentModel() As GLM
        Get
            Return Me.pFinalZeroModel
        End Get
    End Property

    ''' <summary>
    ''' Returns the raw residual vector <c>y - μ</c>.
    ''' </summary>
    Public ReadOnly Property RawResiduals() As Double()
        Get
            If Me.pRaw_res Is Nothing Then Return Nothing
            Return DirectCast(Me.pRaw_res.Clone(), Double())
        End Get
    End Property

    ''' <summary>
    ''' Returns the Pearson residual vector based on the ZIP variance function.
    ''' </summary>
    Public ReadOnly Property PearsonResiduals() As Double()
        Get
            If Me.pPearsChisq_res Is Nothing Then Return Nothing
            Return DirectCast(Me.pPearsChisq_res.Clone(), Double())
        End Get
    End Property

    Private pAlpha As Double
    Private pMaxEMIter As Integer
    Private pMaxIRLSIter As Integer
    Private pEps As Double
    Private pEMiterations As Integer = 0
    Private CompTime As Double

    Private Data_count(,) As Double 'It is assumed that response varaible is in the 1st column
    Private Data_zero(,) As Double 'It is assumed that response varaible is in the 1st column
    Private pVarNames_count() As String
    Private pVarNames_zero() As String
    Private pRowNums() As Integer

    'Optional offset for the Poisson (count) part only.
    Private pOffset() As Double
    Private pbOffset As Boolean = False
    Private pOffsetVarName As String = Nothing

    Private n As Integer 'number of rows
    Private p_zero As Integer 'number of variables in logistic part
    Private p_count As Integer 'number of variables in poisson part
    Private y() As Double 'response
    Private pConverged As Boolean = False
    Private pLastIterLLchange As Double

    Private pPredicted_Zero() As Double
    Private pPredicted_Count() As Double
    Private pPredicted() As Double 'Zip Predicted values
    Private pLinPred_Zero() As Double
    Private pLinPred_Count() As Double
    Private pHessianMat(,) As Double 'Hessian matrix
    Private pLogLikelihood As Double
    Private pFinalDeviance As Double
    Private pFinalZeroModel As GLM
    Private ZIPmodelInfo As ResultTable
    'residuals
    Private pRaw_res() As Double
    Private pPearsChisq_res() As Double

    Private pY0count As String
    Private pItInfo(,) As Double 'Interation history information

    ''' <summary>
    ''' Initializes a ZIP model with default EM/IRLS controls.
    ''' </summary>
    ''' <remarks>
    ''' Defaults in code:
    ''' <list type="bullet">
    ''' <item><description><c>pEps = 1e-9</c> (log-likelihood change tolerance)</description></item>
    ''' <item><description><c>pAlpha = 0.05</c></description></item>
    ''' <item><description><c>pMaxEMIter = 200</c></description></item>
    ''' <item><description><c>pMaxIRLSIter = 25</c></description></item>
    ''' </list>
    ''' </remarks>
    Public Sub New()
        pEps = 0.000000001
        pAlpha = 0.05 'significance level
        pMaxEMIter = 200
        pMaxIRLSIter = 25
    End Sub

    ''' <summary>
    ''' Supplies the Poisson-part and Logistic-part datasets (and their variable names) for ZIP fitting.
    ''' </summary>
    ''' <param name="arPoisData">Data matrix for the count part: column 0 is response, remaining columns are predictors.</param>
    ''' <param name="arLogisticData">Data matrix for the zero part: column 0 is the same response, remaining columns are predictors.</param>
    ''' <param name="strPoisVarNames">Variable names for the Poisson part (aligned to columns).</param>
    ''' <param name="strLogisticVarNames">Variable names for the Logistic part (aligned to columns).</param>
    ''' <param name="RowNums">Optional mapping back to original row indices; if omitted, uses sequential indices.</param>
    ''' <param name="Offset">Optional offset for the Poisson part only.</param>
    ''' <param name="strOffsetVarName">Optional offset variable name for reporting.</param>
    ''' <remarks>
    ''' Both matrices must have the same number of rows and the same response values in column 0.
    ''' </remarks>
    Public Sub dataInputs(arPoisData(,) As Double, arLogisticData(,) As Double,
                           strPoisVarNames() As String, strLogisticVarNames() As String,
                           Optional RowNums() As Integer = Nothing,
                           Optional Offset() As Double = Nothing,
                           Optional strOffsetVarName As String = Nothing)

        Me.Data_count = arPoisData
        Me.Data_zero = arLogisticData
        Me.pVarNames_count = strPoisVarNames
        Me.pVarNames_zero = strLogisticVarNames

        If RowNums Is Nothing Then
            ReDim Me.pRowNums(Data_count.GetUpperBound(0))
            For i = 1 To Data_count.GetUpperBound(0)
                Me.pRowNums(i) = i
            Next
        Else
            Me.pRowNums = RowNums
        End If

        Me.pbOffset = False
        Me.pOffsetVarName = strOffsetVarName

        If Offset Is Nothing Then
            Me.pOffset = Matrix.MatrixArithmeticCore.ConstantVector(arPoisData.GetUpperBound(0), 0.0)
        Else
            If Offset.Length <> arPoisData.GetLength(0) Then
                CoreServices.Errors.LogAndThrow(New ArgumentException("ZIP offset array length does Not match the number of observations."))
            End If

            Me.pOffset = Offset

            For i = 0 To Offset.GetUpperBound(0)
                If Offset(i) <> 0.0 Then
                    Me.pbOffset = True
                    Exit For
                End If
            Next
        End If
    End Sub

    ''' <summary>
    ''' Sets ZIP fitting controls for EM and its nested IRLS steps.
    ''' </summary>
    ''' <param name="dAlpha">Significance level for output formatting.</param>
    ''' <param name="irlsMaxiter">Maximum IRLS iterations used inside each M-step GLM fit.</param>
    ''' <param name="emMaxiter">Maximum EM iterations.</param>
    ''' <param name="dEps">Convergence tolerance for absolute change in observed-data log-likelihood.</param>
    Public Sub settingInputs(dAlpha As Double, irlsMaxiter As Integer, emMaxiter As Integer, dEps As Double)
        pAlpha = dAlpha
        pMaxEMIter = emMaxiter
        pMaxIRLSIter = irlsMaxiter
        pEps = dEps
    End Sub

    ''' <summary>
    ''' AIC for the fitted ZIP model.
    ''' </summary>
    ''' <remarks>
    ''' The code returns:
    ''' <para><c>AIC = −2·(LL − (p_count + p_zero))</c></para>
    ''' (algebraically equivalent to <c>−2·LL + 2·(p_count+p_zero)</c>).
    ''' </remarks>
    Public ReadOnly Property AIC() As Double
        Get
            Return -2.0 * (pLogLikelihood - (p_count + p_zero))
        End Get
    End Property

    ''' <summary>
    ''' BIC for the fitted ZIP model.
    ''' </summary>
    ''' <remarks>
    ''' Computed as:
    ''' <para><c>BIC = D + log(n)·(p_count+p_zero)</c></para>
    ''' where <c>D = −2·LL</c> is the final deviance stored by the code.
    ''' </remarks>
    Public ReadOnly Property BIC() As Double
        Get
            Return pFinalDeviance + Math.Log(n) * (p_count + p_zero)
        End Get
    End Property

    ''' <summary>
    ''' Small-sample corrected AIC (AICc) for the fitted ZIP model.
    ''' </summary>
    ''' <remarks>
    ''' Computed as:
    ''' <para><c>AICc = D + 2·k·n / (n − k − 1)</c></para>
    ''' where <c>k = p_count + p_zero</c>.
    ''' </remarks>
    Public ReadOnly Property AICc() As Double
        Get
            Return pFinalDeviance + 2.0 * (p_count + p_zero) * (n / (n - (p_count + p_zero) - 1))
        End Get
    End Property

    ''' <summary>
    ''' Returns the ZIP model mean prediction <c>E[Y|x,z] = (1−π)·λ</c> for each observation.
    ''' </summary>
    ''' <remarks>
    ''' With <c>λᵢ = exp(xᵢᵀβ)</c> and <c>πᵢ = logistic(zᵢᵀγ)</c>, the ZIP mean is:
    ''' <para><c>μᵢ = (1−πᵢ)·λᵢ</c></para>
    ''' </remarks>
    Public ReadOnly Property Predicted() As Double()
        Get
            Return Me.pPredicted
        End Get
    End Property

    ''' <summary>
    ''' Returns basic residuals for ZIP: raw and Pearson residuals.
    ''' </summary>
    ''' <remarks>
    ''' As implemented:
    ''' <list type="bullet">
    ''' <item><description><b>Raw</b>: <c>rᵢ = yᵢ − μᵢ</c> where <c>μᵢ</c> is the ZIP mean prediction.</description></item>
    ''' <item><description><b>Pearson</b>: code-stored Pearson-style residual for ZIP (computed post-fit).</description></item>
    ''' </list>
    ''' </remarks>
    Public ReadOnly Property AllResiduals() As Object(,)
        Get
            Dim t = New ResultTable
            Dim o(n - 1, 1) As Double
            For i = 0 To n - 1
                o(i, 0) = Me.pRaw_res(i)
                o(i, 1) = Me.pPearsChisq_res(i)
            Next
            t.SetBody(o)
            t.AddHeaderTopRow({"Raw Resid.", "Pearson Resid."})
            Return t.returnSelf()
        End Get
    End Property

    ''' <summary>
    ''' Produces formatted result tables for the ZIP model (Poisson and Logistic components plus model diagnostics).
    ''' </summary>
    ''' <param name="strOffsetVar">Optional offset variable name (if relevant) added as a footnote.</param>
    ''' <param name="strWeightsVar">Optional weights variable name added as a footnote.</param>
    ''' <returns>A list of <c>ResultTable</c> objects for reporting/UI display.</returns>
    ''' <remarks>
    ''' Typically includes:
    ''' <list type="bullet">
    ''' <item><description>Poisson (count) coefficient table</description></item>
    ''' <item><description>Logistic (zero) coefficient table (with separation warnings if detected)</description></item>
    ''' <item><description>ZIP model info table (LL, deviance, AIC/AICc/BIC, #zeros, etc.)</description></item>
    ''' <item><description>Iteration trace (if enabled)</description></item>
    ''' <item><description>Optional covariance output (if enabled and available)</description></item>
    ''' </list>
    ''' </remarks>
    Public Function wrapResults(Optional strOffsetVar As String = Nothing,
                                Optional strWeightsVar As String = Nothing) As List(Of ResultTable)
        Dim out As New List(Of ResultTable)
        Dim t = New ResultTable

        'Poisson Coefficients, SE table
        t = Me.resultsPoisson.CoeffsZ_toPrint()
        t.AddPvalueToFormat(4)
        t.AddTitle("Poisson Model Estimates")
        If strOffsetVar IsNot Nothing Then t.AddFootnote($"Offset Variable {strOffsetVar}")
        If strWeightsVar IsNot Nothing Then t.AddFootnote($"Weights Variable {strWeightsVar}")
        If Me.startParamsPois IsNot Nothing Then t.AddFootnote($"Starting values {ArrayToString(Me.startParamsPois)}")
        out.Add(t)

        'Logistic Coefficients, SE table
        t = Me.resultsLogistic.CoeffsZ_toPrint()
        t.AddPvalueToFormat(4)
        t.AddTitle("Logistic Model Estimates")
        If Me.pFinalZeroModel.bSeparation Then
            t.AddFootnote("Complete separation of data points. Maximum likelihood estimates may Not exist.")
        ElseIf Me.pFinalZeroModel.bQuasiSeparation Then
            t.AddFootnote("Quasi-separation of the iterative algorithm. Results may be misleading.")
        End If
        If Me.startParamsLog IsNot Nothing Then t.AddFootnote($"Starting values {ArrayToString(Me.startParamsLog)}")
        t.AddFootnote($"Computational time {Me.CompTime} seconds.")
        out.Add(t)

        'Model Info
        out.Add(Me.ZIPmodelInfo)

        'iteration info
        If Me.bIterationDetails Then
            t = New ResultTable
            t.SetBody(Me.pItInfo)
            Dim ItLabels(Me.pEMiterations - 1) As String
            For i = 0 To Me.pEMiterations - 1 : ItLabels(i) = $"Iteration {i + 1}" : Next
            t.AddHeaderTopRow(ItLabels)
            t.AddHeaderLeftRow(ConcatArrays(ConcatArrays(Me.pVarNames_count, Me.pVarNames_zero), {"LogLikelihood", "LogLikelihood Change"}))
            out.Add(t)
        End If

        'Return covariance
        If Me.bReturnCov Then
            t = New ResultTable
            t.SetBody(Me.pHessianMat)
            Dim h(Me.pVarNames_count.Length + Me.pVarNames_zero.Length - 1) As String
            h(0) = "Covariance matrix of parameters"
            t.AddHeaderTopRow(h)
            t.AddHeaderTopRow(ConcatArrays(Me.pVarNames_count, Me.pVarNames_zero))
            t.AddHeaderLeftRow(ConcatArrays(Me.pVarNames_count, Me.pVarNames_zero))
            out.Add(t)
        End If

        Return out
    End Function

    ''' <summary>
    ''' Fits a Zero-Inflated Poisson model by EM, using GLM(IRLS) fits in each M-step.
    ''' </summary>
    ''' <param name="interceptPois">1 to include an intercept in the Poisson part; 0 otherwise.</param>
    ''' <param name="interceptLog">1 to include an intercept in the Logistic part; 0 otherwise.</param>
    ''' <param name="bStartParamsPois">If True, uses <see cref="startParamsPois"/> for Poisson-part initialization.</param>
    ''' <param name="bStartParamsLog">If True, uses <see cref="startParamsLog"/> for Logistic-part initialization.</param>
    ''' <param name="progress">Optional host-neutral progress reporter.</param>
    ''' <remarks>
    ''' <para>
    ''' Preconditions enforced by the code:
    ''' </para>
    ''' <list type="bullet">
    ''' <item><description>The data must contain at least one zero (<c>y0 &gt; 0</c>).</description></item>
    ''' <item><description>Not all observations may be zero (otherwise parameters are not identifiable).</description></item>
    ''' </list>
    ''' <para>
    ''' After convergence, the method:
    ''' </para>
    ''' <list type="bullet">
    ''' <item><description>Stores final log-likelihood and deviance <c>D = −2·LL</c>.</description></item>
    ''' <item><description>Computes component linear predictors and predictions, and the ZIP mean <c>(1−π)·λ</c>.</description></item>
    ''' <item><description>Computes a Hessian-based covariance/SE estimate for both component parameter vectors.</description></item>
    ''' <item><description>Optionally computes residuals if <see cref="bComputeResiduals"/> is True.</description></item>
    ''' </list>
    ''' </remarks>
    Public Sub Fit(interceptPois As Integer, interceptLog As Integer,
                   Optional bStartParamsPois As Boolean = False,
                   Optional bStartParamsLog As Boolean = False,
                   Optional progress As IProgressReporter = Nothing)
        Dim y0 As Integer, LL_old As Double, LL_new As Double
        Dim stopwatch As Stopwatch = Stopwatch.StartNew()
        Me.resultsPoisson = New LMresult
        Me.resultsPoisson.varNames = GLM.SafePredictorNames(Me.pVarNames_count)
        Me.resultsLogistic = New LMresult
        Me.resultsLogistic.varNames = GLM.SafePredictorNames(Me.pVarNames_zero)

        'Set variable number constants for Poisson part
        Dim pi1 = Data_count.GetLength(1) 'Columns of predictor variables and responses
        Me.p_count = pi1 - 1 + interceptPois '# of variables initially in the model
        Me.n = Data_count.GetLength(0) '# of observations

        'Set variable number constants for Logistic part
        'It is assumed that 1st column in both Data_count and Data_zero is the same response variable
        pi1 = Data_zero.GetLength(1) 'Columns of predictor variables and responses
        Me.p_zero = pi1 - 1 + interceptLog '# of variables initially in the model
        If n <> Data_zero.GetLength(0) Then
            CoreServices.Errors.LogAndThrow(New ArgumentException("ERROR Number of records in Poisson And Logistic part of model doesn't match."))
            Exit Sub
        End If

        Dim YX_zero(n - 1, p_zero - interceptLog) As Double
        ReDim y(n - 1), pItInfo(p_zero + p_count + 1, pMaxEMIter) 'stores params estimates, LL at each iteration

        'prepare Logistic data as required in EM algorithm
        y = DataManagement.ArrayUtilities.GetColumn(Data_zero, 0)
        For i = 0 To n - 1
            'Response for initial Logisitc model. Swap Zeros and Ones
            YX_zero(i, 0) = If(y(i) = 0, 1, 0)
            If y(i) = 0 Then y0 += 1
            For j = 1 To p_zero - interceptLog
                YX_zero(i, j) = Data_zero(i, j)
            Next
        Next
        pY0count = $"{y0} ({100.0 * y0 / n:##0.00}%)"
        If y0 = 0 Then
            CoreServices.Errors.LogAndThrow(New ArgumentException("No zero present in the data. ZIP cannot be fitted. Aborting exectution."))
            Exit Sub
        End If

        If y0 = n Then
            CoreServices.Errors.LogAndThrow(New ArgumentException("All observations are zero. ZIP is not identifiable (no positive counts)."))
            Exit Sub
        End If


        'Initial Estimates ------------------------------------------
        'Poisson init: weight = 0 For y=0 rows (equivalent To fitting On y>0 only),
        'but keeps full-length mu (n) available for the first E-step.

        Dim model_count = New GLM(New regression.Poisson, New regression.Log)

        ' Count positives to decide if y>0-only init is stable
        Dim nPos As Integer = 0
        For i = 0 To n - 1
            If y(i) > 0.0 Then nPos += 1
        Next

        ' Rule-of-thumb guard: if too few positives relative to parameters, fall back
        Dim minPos As Integer = Math.Max(30, 2 * Me.p_count)

        Dim wInit(n - 1) As Double
        If (Not bStartParamsPois) AndAlso (nPos >= minPos) Then
            ' Exclude zeros from Poisson init
            For i = 0 To n - 1
                wInit(i) = If(y(i) > 0.0, 1.0, 0.0)
            Next
        Else
            ' Fall back to full-data init (or user provided start params)
            wInit = Matrix.MatrixArithmeticCore.ConstantVector(n - 1, 1.0)
        End If

        With model_count
            .bComputeResiduals = False
            .bIterationDetails = False
            .data(Me.Data_count,, If(Me.pbOffset, Me.pOffset, Nothing), wInit) ' Pass prior weights for init; this keeps Xdata/mu length = n
            .setVarNames(Me.pVarNames_count)
            .settingInputs(pAlpha, pMaxIRLSIter, pEps)
            If bStartParamsPois Then .startParams = Me.startParamsPois
            .Fit(interceptPois, bStartParamsPois)
        End With
        Dim countParam = model_count.results.Coeffs_est


        'Logistic
        Dim model_zero = New GLM(New regression.Binomial, New regression.Logit)
        With model_zero
            .bComputeResiduals = False
            .bHosmerLemeshow = False
            .bIterationDetails = False
            .data(YX_zero)
            .setVarNames(Me.pVarNames_zero)
            .settingInputs(pAlpha, pMaxIRLSIter, pEps)
            If bStartParamsLog Then .startParams = Me.startParamsLog
            .Fit(interceptLog, bStartParamsLog) 'allways calculate intercept
        End With
        Dim zeroParam = model_zero.results.Coeffs_est

        'E Step
        Dim probi1(n - 1) As Double, probi(n - 1) As Double
        Dim mui = model_count.PredictedResponses
        model_zero.PredictedResponses.CopyTo(probi, 0)

        For i = 0 To n - 1
            probi(i) = If(y(i) = 0.0, probi(i) / (probi(i) + (1.0 - probi(i)) * distributions.PoissonPMF(0.0, mui(i))), 0)
            probi1(i) = 1.0 - probi(i)
        Next

        LL_new = loglikfun(model_count.Xdata, countParam, model_zero.Xdata, zeroParam,
                           If(Me.pbOffset, Me.pOffset, Nothing), Me.pbOffset)
        LL_old = 2.0 * LL_new 'multipley by 2 to always run at least 1 iteration
        pLastIterLLchange = Math.Abs(LL_new - LL_old)
        'EM iterations ----------------------------------------------------------------
        Do While pLastIterLLchange > pEps And pEMiterations <= pMaxEMIter
            CoreServices.Regression.ThrowIfCancellationRequested("Zero-inflated Poisson calculation cancelled by user.")
            If pEMiterations > 0 AndAlso CoreServices.Regression.IsInterruptionRequested() Then
                AppInfrastructure.CoreServices.Log("Zero-inflated Poisson calculation interrupted by user; returning latest accepted EM estimates.", AppInfrastructure.LogMsgType.Warn)
                Exit Do
            End If

            LL_old = LL_new

            ' >>> SAVE OLD PARAMS BEFORE M-STEP UPDATES <<<
            Dim countOld() As Double = DirectCast(countParam.Clone(), Double())
            Dim zeroOld() As Double = DirectCast(zeroParam.Clone(), Double())

            'M step - Poisson -------------------------------
            With model_count
                .data(Data_count,, If(Me.pbOffset, Me.pOffset, Nothing), probi1)
                .startParams = countParam
                .Fit(interceptPois, True)
            End With
            countParam = model_count.results.Coeffs_est

            'M step - Logistic -------------------------------
            For i = 0 To n - 1
                YX_zero(i, 0) = probi(i)
            Next
            With model_zero
                .data(YX_zero)
                .startParams = zeroParam
                .Fit(interceptLog, True)
            End With
            zeroParam = model_zero.results.Coeffs_est

            ' ---------- Over-relaxation with monotone fallback ----------
            ' These are the plain EM updates produced by the M-step GLMs
            Dim countNew() As Double = model_count.results.Coeffs_est
            Dim zeroNew() As Double = model_zero.results.Coeffs_est

            ' Base LL at the plain EM update
            Dim LL_em As Double = loglikfun(model_count.Xdata, countNew, model_zero.Xdata, zeroNew,
                                            If(Me.pbOffset, Me.pOffset, Nothing), Me.pbOffset)

            ' Try an over-relaxed step
            Dim s As Double = 1.2
            Const sMin As Double = 1.0
            Dim maxBacktracks As Integer = 6
            Dim countTry(countNew.Length - 1) As Double
            Dim zeroTry(zeroNew.Length - 1) As Double
            Dim LL_try As Double = Double.NegativeInfinity
            Dim accepted As Boolean = False

            For bt As Integer = 0 To maxBacktracks
                ' build trial params: old + s*(new-old)
                For j As Integer = 0 To countNew.Length - 1
                    countTry(j) = countOld(j) + s * (countNew(j) - countOld(j))
                Next
                For j As Integer = 0 To zeroNew.Length - 1
                    zeroTry(j) = zeroOld(j) + s * (zeroNew(j) - zeroOld(j))
                Next

                LL_try = loglikfun(model_count.Xdata, countTry, model_zero.Xdata, zeroTry,
                                   If(Me.pbOffset, Me.pOffset, Nothing), Me.pbOffset)

                If LL_try >= LL_em Then
                    accepted = True
                    Exit For
                End If

                ' backtrack toward s=1 (monotone fallback)
                s = 1.0 + 0.5 * (s - 1.0)
                If s <= sMin + 0.000001 Then Exit For
            Next

            If accepted Then
                countParam = DirectCast(countTry.Clone(), Double())
                zeroParam = DirectCast(zeroTry.Clone(), Double())
                LL_new = LL_try
            Else
                countParam = countNew
                zeroParam = zeroNew
                LL_new = LL_em
            End If
            AppInfrastructure.CoreServices.Log($"ZIP Over-relaxation with monotone fallback Iter {pEMiterations}: accepted={accepted}, s={s:0.###}, LL_new={LL_new}")

            ' ---------- E-step computed from the ACCEPTED params ----------
            ' Compute mu and pi from X matrices + accepted params
            mui = PredictPoissonLogLink(model_count.Xdata, countParam, If(Me.pbOffset, Me.pOffset, Nothing), Me.pbOffset)
            probi = PredictLogisticLogitLink(model_zero.Xdata, zeroParam)

            For i As Integer = 0 To n - 1
                probi(i) = If(y(i) = 0.0, probi(i) / (probi(i) + (1.0 - probi(i)) * distributions.PoissonPMF(0.0, mui(i))), 0.0)
                probi1(i) = 1.0 - probi(i)
            Next

            ' Standard LL change
            pLastIterLLchange = Math.Abs(LL_new - LL_old)

            'save iteration info
            If bIterationDetails Then
                For i = 0 To zeroParam.GetUpperBound(0) + countParam.GetLength(0)
                    pItInfo(i, pEMiterations) = If(i <= zeroParam.GetUpperBound(0), zeroParam(i), countParam(i - 1 - zeroParam.GetUpperBound(0)))
                Next
                pItInfo(pItInfo.GetUpperBound(0) - 1, pEMiterations) = LL_new
                pItInfo(pItInfo.GetUpperBound(0), pEMiterations) = pLastIterLLchange
            End If
            If progress IsNot Nothing Then
                progress.Report(Convert.ToInt32(100.0 * Me.pEMiterations / Me.pMaxEMIter),
                                $"Elapsed Time: {Math.Round(stopwatch.Elapsed.TotalSeconds, 2)}[s]   Iterations: {Me.pEMiterations + 1}   LogLikelihood change = {pLastIterLLchange}")
            End If

            pEMiterations += 1
            If pLastIterLLchange < pEps Then pConverged = True
        Loop

        'Finalize results ------------------------------------------------------------------------------
        ReDim Preserve pItInfo(pItInfo.GetUpperBound(0), pEMiterations - 1)
        Me.pLogLikelihood = LL_new
        Me.pFinalDeviance = -2.0 * LL_new

        'Create results
        Me.pFinalZeroModel = model_zero
        ' Store final coefficients as the ACCEPTED parameters (may differ from GLM internal state)
        Me.resultsPoisson.bIntercept = (interceptPois = 1)
        Me.resultsLogistic.bIntercept = (interceptLog = 1)
        Me.resultsPoisson.Coeffs_est = countParam
        Me.resultsLogistic.Coeffs_est = zeroParam
        ' Recompute linear predictors and predictions from the accepted params
        Me.pLinPred_Count = LinearPredictor(model_count.Xdata, countParam)
        If Me.pbOffset Then
            For i = 0 To Me.pLinPred_Count.GetUpperBound(0)
                Me.pLinPred_Count(i) += Me.pOffset(i)
            Next
        End If

        Me.pPredicted_Count = PredictPoissonLogLink(model_count.Xdata, countParam, If(Me.pbOffset, Me.pOffset, Nothing), Me.pbOffset)  ' Poisson log link
        Me.pLinPred_Zero = LinearPredictor(model_zero.Xdata, zeroParam)
        Me.pPredicted_Zero = PredictLogisticLogitLink(model_zero.Xdata, zeroParam) ' uses stable logistic


        ReDim pPredicted(n - 1)
        For i = 0 To n - 1
            Me.pPredicted(i) = (1.0 - Me.pPredicted_Zero(i)) * Me.pPredicted_Count(i)
        Next

        'Get hessian matrix and update outputs with correct standard errors
        Me.pHessianMat = Hess()

        'update userform label
        If progress IsNot Nothing Then
            progress.Report(100, $"Elapsed Time: {Math.Round(stopwatch.Elapsed.TotalSeconds, 2)}[s]   Finalizing ...")
        End If

        Dim tmp1 = distributions.NormSInv(1.0 - pAlpha / 2.0)
        ReDim Me.resultsPoisson.Coeffs_SEs(Me.p_count - 1), Me.resultsLogistic.Coeffs_SEs(Me.p_zero - 1)
        For i = 0 To p_count - 1
            If pHessianMat(i, i) > 0.0 Then Me.resultsPoisson.Coeffs_SEs(i) = Math.Sqrt(pHessianMat(i, i))
        Next
        For i = 0 To p_zero - 1
            If pHessianMat(p_count + i, p_count + i) > 0.0 Then Me.resultsLogistic.Coeffs_SEs(i) = Math.Sqrt(pHessianMat(p_count + i, p_count + i))
        Next

        If Me.bComputeResiduals Then Me.Residuals()

        ZIPmodelInfo = New ResultTable
        ZIPmodelInfo.AddHeaderLeftRow({"Model", "Poisson Model Link Function", "Logistic Model Link Function", "Residual deviance",
                                      "Log Likelihood", "# observations", "Observations with Y = 0", "AIC", "AICc", "BIC",
                                      "Number of EM Iterations", "Relative Log - Likelihood Change", "Converged?"})
        ZIPmodelInfo.SetBody({{"Zero-Inflated Poisson", ""},
                              {"Log", ""},
                              {"Logit", ""},
                              {Me.pFinalDeviance, ""},
                              {Me.pLogLikelihood, ""},
                              {Me.n, ""},
                              {pY0count, ""},
                              {Me.AIC, ""},
                              {Me.AICc, ""},
                              {Me.BIC, ""},
                              {Me.pEMiterations, ""},
                              {Me.pLastIterLLchange, ""},
                              {Me.pConverged.ToString(), ""}})
        stopwatch.Stop()
        Me.CompTime = stopwatch.Elapsed.TotalSeconds
    End Sub

    Private Shared Function ConcatArrays(Of T)(first() As T, second() As T) As T()
        If first Is Nothing Then Throw New ArgumentNullException(NameOf(first))
        If second Is Nothing Then Throw New ArgumentNullException(NameOf(second))

        Dim output(first.Length + second.Length - 1) As T
        Array.Copy(first, 0, output, 0, first.Length)
        Array.Copy(second, 0, output, first.Length, second.Length)
        Return output
    End Function

    Private Shared Function ArrayToString(Of T)(values() As T) As String
        If values Is Nothing Then Return "[]"

        Dim builder As New System.Text.StringBuilder("[")
        For i As Integer = 0 To values.Length - 1
            If i > 0 Then builder.Append(", ")
            Dim boxed As Object = values(i)
            If boxed IsNot Nothing Then builder.Append(boxed.ToString())
        Next
        builder.Append("]")
        Return builder.ToString()
    End Function

    Private Function LinearPredictor(ByVal X As Double(,), ByVal b As Double()) As Double()
        Dim nLocal As Integer = X.GetLength(0)
        Dim pLocal As Integer = b.Length
        Dim eta(nLocal - 1) As Double

        For i As Integer = 0 To nLocal - 1
            Dim s As Double = 0.0
            For j As Integer = 0 To pLocal - 1
                s += X(i, j) * b(j)
            Next
            eta(i) = s
        Next
        Return eta
    End Function

    Private Function PredictPoissonLogLink(ByVal X As Double(,), ByVal beta As Double(),
                                                 Optional ByVal offset As Double() = Nothing,
                                                 Optional ByVal bOffset As Boolean = False) As Double()
        Dim n As Integer = X.GetLength(0)
        Dim p As Integer = beta.Length
        Dim mu(n - 1) As Double

        If bOffset AndAlso (offset Is Nothing OrElse offset.Length <> n) Then
            CoreServices.Errors.LogAndThrow(New ArgumentException("Offset array is missing or has incorrect length."))
        End If

        For i As Integer = 0 To n - 1
            Dim eta As Double = 0.0
            For j As Integer = 0 To p - 1
                eta += X(i, j) * beta(j)
            Next
            If bOffset Then eta += offset(i)

            ' avoid overflow
            If eta > 700.0 Then
                mu(i) = Math.Exp(700.0)
            ElseIf eta < -700.0 Then
                mu(i) = Math.Exp(-700.0)
            Else
                mu(i) = Math.Exp(eta)
            End If
        Next

        Return mu
    End Function

    Private Function PredictLogisticLogitLink(ByVal X As Double(,), ByVal gamma As Double()) As Double()
        Dim n As Integer = X.GetLength(0)
        Dim p As Integer = gamma.Length
        Dim pi(n - 1) As Double

        For i As Integer = 0 To n - 1
            Dim eta As Double = 0.0
            For j As Integer = 0 To p - 1
                eta += X(i, j) * gamma(j)
            Next
            pi(i) = regression.Logit.LogisticStable(eta)
        Next

        Return pi
    End Function




    ''' <summary>
    ''' Computes the diagonal Hessian element for the count (Poisson) component
    ''' of a zero-inflated Poisson model. This corresponds to the second
    ''' derivative of the log-likelihood with respect to the count parameters.
    ''' </summary>
    ''' <param name="y">Observed response value.</param>
    ''' <param name="mu">Predicted Poisson mean λ.</param>
    ''' <param name="pi">Predicted zero-inflation probability π.</param>
    ''' <returns>
    ''' The Hessian contribution Dbb(i,i). Returned value is negative of the
    ''' second derivative, matching the convention used in Newton–Raphson
    ''' and Fisher scoring.
    ''' </returns>
    ''' <remarks>
    ''' <para>
    ''' For y = 0, the ZIP likelihood mixes the Poisson and zero-inflation
    ''' components, producing a more complex second derivative. This method
    ''' uses a numerically stable formulation involving only exp(-μ), which
    ''' is always safe for large μ.
    ''' </para>
    ''' <para>
    ''' For y > 0, the second derivative reduces to -μ.
    ''' </para>
    ''' </remarks>
    Private Function HessianCountTerm(y As Double, mu As Double, pi As Double) As Double
        Dim result As Double = 0.0

        If y = 0.0 Then
            Dim eMinusMu As Double = Math.Exp(-mu)
            Dim oneMinusPi As Double = 1.0 - pi

            Dim numerator As Double = -eMinusMu * ((1.0 - mu) * pi + oneMinusPi * eMinusMu) * oneMinusPi * mu
            Dim denom As Double = pi + oneMinusPi * eMinusMu
            Dim denomSq As Double = denom * denom

            result = numerator / denomSq
        Else
            result = -mu
        End If

        Return -result
    End Function

    ''' <summary>
    ''' Computes the diagonal Hessian element for the zero-inflation (logistic)
    ''' component of a zero-inflated Poisson model. This corresponds to the
    ''' second derivative of the log-likelihood with respect to the zero
    ''' component parameters.
    ''' </summary>
    ''' <param name="y">Observed response value.</param>
    ''' <param name="mu">Predicted Poisson mean λ.</param>
    ''' <param name="pi">Predicted zero-inflation probability π.</param>
    ''' <returns>
    ''' The Hessian contribution Dgg(i,i). Returned value is negative of the
    ''' second derivative, matching Newton–Raphson conventions.
    ''' </returns>
    ''' <remarks>
    ''' <para>
    ''' The Hessian splits into two parts:
    ''' 
    '''   gg1 — contribution when y = 0 from the ZIP mixture term.
    '''   gg2 — contribution from the logistic link itself.
    ''' 
    ''' gg2 simplifies exactly to:
    ''' 
    '''     gg2 = π (π - 1)
    ''' 
    ''' which is always safe numerically.
    ''' </para>
    ''' </remarks>
    Private Function HessianZeroTerm(y As Double, mu As Double, pi As Double) As Double
        Dim gg1 As Double = 0.0
        Dim gg2 As Double = pi * (pi - 1.0)

        If y = 0 Then
            Dim eMinusMu As Double = Math.Exp(-mu)
            Dim oneMinusPi As Double = 1.0 - pi

            Dim denom As Double = pi + oneMinusPi * eMinusMu
            Dim denomSq As Double = denom * denom

            gg1 = pi * oneMinusPi * eMinusMu / denomSq
        End If

        Return -(gg1 + gg2)
    End Function

    ''' <summary>
    ''' Computes the cross-derivative Hessian element between the count and
    ''' zero-inflation components of a zero-inflated Poisson model.
    ''' </summary>
    ''' <param name="y">Observed response value.</param>
    ''' <param name="mu">Predicted Poisson mean λ.</param>
    ''' <param name="etaZero">Linear predictor for the zero component.</param>
    ''' <returns>
    ''' The Hessian contribution Dgb(i,i). Returned value is negative of the
    ''' mixed second derivative. Returns 0 when y > 0.
    ''' </returns>
    ''' <remarks>
    ''' <para>
    ''' The cross term exists only when y = 0. Using the identity:
    ''' 
    '''     logistic(t) (1 - logistic(t)) = exp(t) / (1 + exp(t))^2
    ''' 
    ''' the expression becomes:
    ''' 
    '''     Dgb = -μ * logistic(μ + η₀) * (1 - logistic(μ + η₀))
    ''' 
    ''' which is fully stable for all μ and η₀.
    ''' </para>
    ''' </remarks>
    Private Function HessianCrossTerm(y As Double, mu As Double, etaZero As Double) As Double
        If y <> 0.0 Then Return 0.0

        Dim t As Double = mu + etaZero
        Dim q As Double = regression.Logit.LogisticStable(t)

        Return -mu * q * (1.0 - q)
    End Function

    ''' <summary>
    ''' Computes the full Hessian matrix of the zero-inflated Poisson (ZIP) 
    ''' log-likelihood with respect to both the count (Poisson) parameters 
    ''' and the zero-inflation (logistic) parameters. The Hessian is assembled 
    ''' as a block matrix using numerically stable second-derivative components.
    ''' </summary>
    ''' 
    ''' <returns>
    ''' A square matrix representing the observed Hessian of the ZIP model. 
    ''' The matrix is returned as the negative second derivative of the 
    ''' log-likelihood, suitable for Newton–Raphson or Fisher scoring updates.
    ''' </returns>
    ''' 
    ''' <remarks>
    ''' <para>
    ''' The ZIP model combines a Poisson count component with a logistic 
    ''' zero-inflation component. The Hessian therefore has a natural 
    ''' block structure:
    ''' </para>
    ''' 
    ''' <code>
    '''     H = [  Dbb   Dgb ]
    '''         [  Dgb   Dgg ]
    ''' </code>
    ''' 
    ''' <para>
    ''' where:
    ''' </para>
    ''' 
    ''' <list type="bullet">
    '''   <item>
    '''     <description>
    '''     <b>Dbb</b> — second derivatives with respect to the Poisson 
    '''     (count) parameters.
    '''     </description>
    '''   </item>
    '''   <item>
    '''     <description>
    '''     <b>Dgg</b> — second derivatives with respect to the zero-inflation 
    '''     (logistic) parameters.
    '''     </description>
    '''   </item>
    '''   <item>
    '''     <description>
    '''     <b>Dgb</b> — cross-derivatives between the count and zero components.
    '''     </description>
    '''   </item>
    ''' </list>
    ''' 
    ''' <para>
    ''' Each diagonal element of these blocks is computed using dedicated, 
    ''' numerically stable helper functions:
    ''' </para>
    ''' 
    ''' <list type="bullet">
    '''   <item><description><c>HessianCountTerm</c> — Poisson curvature.</description></item>
    '''   <item><description><c>HessianZeroTerm</c> — logistic curvature.</description></item>
    '''   <item><description><c>HessianCrossTerm</c> — mixture interaction curvature.</description></item>
    ''' </list>
    ''' 
    ''' <para>
    ''' These helpers avoid overflow by eliminating unstable expressions such 
    ''' as exp(2·η) and by using stable identities involving:
    ''' </para>
    ''' 
    ''' <list type="bullet">
    '''   <item><description>logistic(x) = 1 / (1 + exp(-x))</description></item>
    '''   <item><description>exp(-μ) which is always safe for large μ</description></item>
    '''   <item><description>π = logistic(η₀) to avoid raw exponentials</description></item>
    ''' </list>
    ''' 
    ''' <para>
    ''' The full Hessian is constructed by embedding these diagonal blocks into 
    ''' a larger matrix using the combined design matrix for both model parts. 
    ''' The final Hessian is:
    ''' </para>
    ''' 
    ''' <code>
    '''     H = Xᵀ D X
    ''' </code>
    ''' 
    ''' <para>
    ''' where:
    ''' </para>
    ''' 
    ''' <list type="bullet">
    '''   <item><description><c>X</c> is the combined design matrix for count and zero components.</description></item>
    '''   <item><description><c>D</c> is the block-diagonal matrix of second derivatives.</description></item>
    '''   <item><description><c>Xᵀ</c> is the transpose of X.</description></item>
    ''' </list>
    ''' 
    ''' <para>
    ''' The resulting Hessian is inverted using a Cholesky-based routine 
    ''' (<c>MatInv(..., "CHOL")</c>) to ensure numerical stability and 
    ''' positive-definiteness when appropriate.
    ''' </para>
    ''' 
    ''' <para>
    ''' This function is intended for use in:
    ''' </para>
    ''' 
    ''' <list type="bullet">
    '''   <item><description>Newton–Raphson optimization</description></item>
    '''   <item><description>Fisher scoring</description></item>
    '''   <item><description>Variance–covariance estimation</description></item>
    '''   <item><description>ZIP model diagnostics and inference</description></item>
    ''' </list>
    ''' 
    ''' <para>
    ''' The implementation is designed to match the numerical behavior of 
    ''' professional statistical software such as SAS, Stata, and R, while 
    ''' avoiding overflow and underflow in extreme predictor settings.
    ''' </para>
    ''' </remarks>
    Private Function Hess() As Double(,)
        Dim Dgg(n - 1, n - 1) As Double, Dbb(n - 1, n - 1) As Double, Dgb(n - 1, n - 1) As Double
        Dim xx(2 * n - 1, p_zero - 1 + p_count) As Double, dd(2 * n - 1, 2 * n - 1) As Double

        For i = 0 To n - 1
            ' Build design matrix rows
            xx(i, 0) = 1.0
            xx(i + n, p_count) = 1.0

            For j = 1 To p_count - 1
                xx(i, j) = Data_count(i, j)
            Next

            For j = 1 To p_zero - 1
                xx(n + i, p_count + j) = Data_zero(i, j)
            Next

            ' Shorthands
            Dim mu As Double = pPredicted_Count(i)
            Dim pi As Double = pPredicted_Zero(i)
            Dim etaZero As Double = pLinPred_Zero(i)

            ' Hessian components
            Dbb(i, i) = HessianCountTerm(y(i), mu, pi)
            Dgg(i, i) = HessianZeroTerm(y(i), mu, pi)
            Dgb(i, i) = HessianCrossTerm(y(i), mu, etaZero)
        Next

        ' Assemble block matrix
        For i = 0 To n - 1
            dd(i, i) = Dbb(i, i)
            dd(n + i, i) = Dgb(i, i)
            dd(i, n + i) = Dgb(i, i)
            dd(n + i, n + i) = Dgg(i, i)
        Next

        Dim tmp2 = Matrix.MatrixArithmeticCore.Multiply(Matrix.MatrixArithmeticCore.Multiply(Matrix.MatrixArithmeticCore.Transpose(xx), dd), xx)
        Return Matrix.MatrixDecompositionCore.InvertMatrix(tmp2, "CHOL")
    End Function

    Sub Residuals()
        'call this sub only after we have parameters estimated
        ReDim pRaw_res(n - 1), pPearsChisq_res(n - 1)
        For i = 0 To n - 1

            Dim mu As Double = pPredicted_Count(i)     ' Poisson mean
            Dim pi As Double = pPredicted_Zero(i)      ' structural-zero probability
            Dim meanY As Double = (1.0 - pi) * mu
            Dim varY As Double = meanY * (1.0 + pi * mu)   ' ZIP variance

            pRaw_res(i) = y(i) - meanY
            pPearsChisq_res(i) = pRaw_res(i) / Math.Sqrt(varY)
        Next
    End Sub

    ''' <summary>
    ''' Computes the log-likelihood of a Zero-Inflated Poisson (ZIP) model
    ''' in a numerically stable way. This version avoids overflow and NaN
    ''' values during EM iterations by using stable logistic, log-sum-exp,
    ''' and log-Poisson computations.
    ''' </summary>
    ''' <param name="Xcount">Design matrix for the Poisson component.</param>
    ''' <param name="count_params">Parameter vector for the Poisson component.</param>
    ''' <param name="Xzero">Design matrix for the zero-inflation component.</param>
    ''' <param name="zero_params">Parameter vector for the zero-inflation component.</param>
    ''' <param name="offset">Optional offset for the Poisson component only.</param>
    ''' <param name="bOffset">If <see langword="True"/>, applies <paramref name="offset"/> to the Poisson linear predictor.</param>
    ''' <returns>The total log-likelihood of the ZIP model.</returns>
    Private Function loglikfun(Xcount(,) As Double,
                               count_params() As Double,
                               Xzero(,) As Double,
                               zero_params() As Double,
                               Optional offset() As Double = Nothing,
                               Optional bOffset As Boolean = False) As Double

        Dim loglik As Double = 0.0

        ' === Compute Poisson mean ===
        Dim mu() As Double = PredictPoissonLogLink(Xcount, count_params, offset, bOffset)

        ' === Compute zero-inflation linear predictor and probability ===
        Dim zeroP(p_zero - 1, 0) As Double
        For i = 0 To p_zero - 1
            zeroP(i, 0) = zero_params(i)
        Next

        Dim phiMat(,) As Double = Matrix.MatrixArithmeticCore.Multiply(Xzero, zeroP)
        Dim pi(n - 1) As Double
        For i = 0 To n - 1
            pi(i) = regression.Logit.LogisticStable(phiMat(i, 0))
        Next

        ' === Log-likelihood contributions ===
        For i = 0 To n - 1
            If y(i) = 0 Then
                loglik += LogZIPZeroTerm(pi(i), mu(i))
            Else
                loglik += LogZIPPositiveTerm(pi(i), Convert.ToInt32(y(i)), mu(i))
            End If
        Next

        Return loglik
    End Function

    ''' <summary>
    ''' Computes the log-likelihood contribution for y > 0 in a ZIP model:
    ''' log(1 - π) + log Poisson(y | μ).
    ''' </summary>
    Private Function LogZIPPositiveTerm(pi As Double, y As Integer, mu As Double) As Double
        If pi >= 1.0 Then Return Double.NegativeInfinity
        Return Math.Log(1.0 - pi) + LogPoissonPMF(y, mu)
    End Function

    ''' <summary>
    ''' Computes log( π + (1-π) * exp(-μ) ) using a stable log-sum-exp identity.
    ''' </summary>
    Private Function LogZIPZeroTerm(pi As Double, mu As Double) As Double
        Dim logA As Double = Math.Log(pi)
        Dim logB As Double = Math.Log(1.0 - pi) - mu

        ' log( exp(logA) + exp(logB) )
        Dim m As Double = Math.Max(logA, logB)
        Return m + Math.Log(Math.Exp(logA - m) + Math.Exp(logB - m))
    End Function

    ''' <summary>
    ''' Computes log(Poisson(y | mu)) in a numerically stable way.
    ''' </summary>
    Private Function LogPoissonPMF(y As Integer, mu As Double) As Double
        If mu <= 0.0 Then Return Double.NegativeInfinity
        Return y * Math.Log(mu) - mu - LogFactorial(y)
    End Function

End Class
