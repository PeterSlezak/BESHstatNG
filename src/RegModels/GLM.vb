Option Explicit On
Imports System.Drawing
Imports System.IO
Imports System.Reflection
Imports System.Resources.ResXFileRef
Imports System.Runtime.InteropServices
Imports Microsoft.Office.Interop.Excel
Imports NLog

''' <summary>
''' Generalized Linear Model (GLM) fitted by Iteratively Reweighted Least Squares (IRLS).
''' </summary>
''' <remarks>
''' <para>
''' This implementation fits regression coefficients <c>β</c> in the mean model:
''' </para>
''' <para><c>ηᵢ = xᵢᵀ β + oᵢ</c> (linear predictor with optional offset <c>oᵢ</c>)</para>
''' <para><c>μᵢ = g⁻¹(ηᵢ)</c> where <c>g</c> is the link function.</para>
''' <para>
''' The response distribution is represented via an exponential-family-like object (<see cref="regression.Family"/>)
''' providing at least a variance function <c>Var(μ)</c>, deviance <c>D(y, μ)</c>, and a log-likelihood routine.
''' </para>
'''
''' <h3>IRLS / Fisher scoring update used in this code</h3>
''' <para>
''' At iteration <c>t</c>, with current <c>μ</c> and <c>η</c>, define:
''' </para>
''' <list type="bullet">
''' <item><description><c>dη/dμ = g'(μ)</c> (this code uses <c>pLink.deriv(μ)</c>).</description></item>
''' <item><description>Working weights
''' <c>wᵢ = wBaseᵢ / ( (dη/dμ)² · Var(μᵢ) )</c>,
''' where <c>wBaseᵢ</c> is an optional user weight (<c>pWeights</c>).</description></item>
''' <item><description>Working response
''' <c>zᵢ = ηᵢ + (yᵢ − μᵢ)(dη/dμ) − oᵢ</c>.
''' (The subtraction of <c>oᵢ</c> is required because <c>ηᵢ</c> stored in the code already includes the offset.)</description></item>
''' </list>
''' <para>
''' Then the updated coefficients are obtained by weighted least squares:
''' </para>
''' <para><c>β(new) = argmin_β Σᵢ wᵢ (zᵢ − xᵢᵀβ)²</c></para>
''' <para>
''' i.e. <c>β(new) = (Xᵀ W X)⁻¹ Xᵀ W z</c>.
''' </para>
'''
''' <h3>Deviance, convergence, and step-halving</h3>
''' <para>
''' After each update, the fitted means are recomputed and the model deviance <c>D(y, μ)</c> is evaluated.
''' If the deviance increases or fitted means become invalid (e.g., out of bounds), the code performs step-halving:
''' </para>
''' <para><c>β ← (β + β_old)/2</c> repeatedly (up to <c>pInnerLoopMaxIter</c>) until the issue is resolved.</para>
''' <para>
''' Convergence is declared when the absolute change in deviance is below <c>pEps</c>:
''' <c>|D_t − D_{t−1}| &lt; pEps</c>.
''' </para>
'''
''' <h3>Scale / dispersion and standard errors</h3>
''' <para>
''' The scale factor returned by <see cref="ScaleSECoef"/> is:
''' </para>
''' <list type="bullet">
''' <item><description><c>1</c> for Binomial, Poisson, Negative Binomial.</description></item>
''' <item><description>For other families, either Pearson-based or Deviance-based as selected by <c>pScaleEstimation</c>.</description></item>
''' </list>
''' <para>
''' Pearson dispersion is computed as <see cref="DispestionParameterPhi"/> = <c>X² / (n − p)</c>,
''' where <c>X² = Σ (y−μ)²/Var(μ)</c>.
''' </para>
''' <para>
''' Parameter covariance is computed as <c>(Xᵀ W X)⁻¹</c> (see <see cref="VarCovar"/>).
''' Standard errors reported in <c>results</c> are scaled in the code as:
''' <c>SE = SE_WLS / sqrt(phi) * sqrt(ScaleSECoef)</c>.
''' </para>
''' </remarks>
Public Class GLM

    ''' <summary>
    ''' Optional starting values for the coefficient vector <c>β</c> (including intercept if used).
    ''' </summary>
    ''' <remarks>
    ''' Used when calling <see cref="Fit"/> with <c>bStartParams:=True</c>.
    ''' Length must match the number of fitted parameters <c>p</c>.
    ''' </remarks>
    Public startParams() As Double = Nothing 'Starting parameter values

    ''' <summary>
    ''' If <c>True</c>, residuals, leverage, standardized residuals, and Cook’s distance are computed after fitting.
    ''' </summary>
    ''' <remarks>
    ''' Residual outputs are exposed via <see cref="AllResiduals"/>.
    ''' </remarks>
    Public bComputeResiduals As Boolean = False

    ''' <summary>
    ''' Populated after a successful <see cref="Fit"/> with coefficient estimates, standard errors, and model tables.
    ''' </summary>
    Public results As LMresult = Nothing

    ''' <summary>
    ''' If <c>True</c>, <see cref="wrapResults"/> includes the covariance matrix table for the fitted parameters.
    ''' </summary>
    ''' <remarks>
    ''' The covariance matrix is computed by <see cref="VarCovar"/> (ultimately <c>(XᵀWX)⁻¹</c>).
    ''' </remarks>
    Public bReturnCov As Boolean = False

    ''' <summary>
    ''' If <c>True</c>, iteration history (coefficients, deviance, and deviance change per iteration) is retained
    ''' and included in <see cref="wrapResults"/>.
    ''' </summary>
    Public bIterationDetails As Boolean = False

    ''' <summary>
    ''' Indicates that complete separation was detected for Binomial/logistic-like models.
    ''' </summary>
    ''' <remarks>
    ''' Separation detection is based on the proportion of extreme fitted probabilities near 0 or 1 during IRLS.
    ''' If complete separation is detected, maximum likelihood estimates may not exist and results can be unstable.
    ''' </remarks>
    Public bSeparation As Boolean = False

    ''' <summary>
    ''' Indicates that quasi-separation was detected for Binomial/logistic-like models.
    ''' </summary>
    ''' <remarks>
    ''' Quasi-separation warns that the IRLS iterates produce a nontrivial fraction of near-0/near-1 fitted probabilities.
    ''' The code may prompt the user to continue.
    ''' </remarks>
    Public bQuasiSeparation As Boolean = False

    ''' <summary>
    ''' If <c>True</c> and the family is Binomial, computes the Hosmer–Lemeshow goodness-of-fit test after fitting.
    ''' </summary>
    ''' <remarks>
    ''' The test is computed by binning predicted probabilities (typically deciles, collapsed if ties occur),
    ''' then comparing observed vs expected successes and failures within bins.
    ''' </remarks>
    Public bHosmerLemeshow As Boolean = True


    ''' <summary>
    ''' Initializes a GLM with a specified family and link function.
    ''' </summary>
    ''' <param name="f">Distribution/family object providing variance, deviance, and log-likelihood routines.</param>
    ''' <param name="l">Link function mapping <c>μ → η</c> with inverse <c>η → μ</c> and derivative <c>dη/dμ</c>.</param>
    ''' <remarks>
    ''' Default controls set here:
    ''' <list type="bullet">
    ''' <item><description><c>pEps = 1e-8</c> (deviance-change tolerance)</description></item>
    ''' <item><description><c>pMaxiter = 20</c> (IRLS maximum iterations)</description></item>
    ''' <item><description><c>pInnerLoopMaxIter = 100</c> (step-halving cap)</description></item>
    ''' <item><description><c>pAlpha = 0.05</c> (for CIs / p-values in output formatting)</description></item>
    ''' <item><description><c>pScaleEstimation = "Pearson chisq"</c> (scale selection for non-canonical families)</description></item>
    ''' </list>
    ''' </remarks>
    Public Sub New(f As regression.Family, l As regression.Link)
        pLink = l
        pFamily = f

        pEps = 0.00000001
        pMaxiter = 20
        pInnerLoopMaxIter = 100
        pAlpha = 0.05 'significance level
        pScaleEstimation = "Pearson chisq"
    End Sub

    Protected Friend CompTime As Double
    Protected Friend pAlpha As Double
    Protected Friend pMaxiter As Integer
    Protected Friend pEps As Double
    Protected Friend pInnerLoopMaxIter As Integer
    Protected Friend pIRLSiterations As Integer
    Protected Friend strError As String

    Protected Friend pLink As regression.Link
    Protected Friend pFamily As regression.Family
    Protected Friend pRowNums() As Integer
    Protected Friend pData(,) As Double 'It is assumed that response varaible is in the 1st column
    Protected Friend pVarNames() As String
    Protected Friend pOffset() As Double
    Protected Friend pbOffset As Boolean
    Protected Friend pbWeigts As Boolean
    Protected Friend pWeights() As Double
    Protected Friend pFinalWeights() As Double
    Protected Friend pbConverged As Boolean
    Protected Friend p As Integer 'number of parameters
    Protected Friend n As Integer 'number of pRecords
    Protected Friend y() As Double
    Protected Friend x(,) As Double               'predictor variables including intercept in the 1st column
    Protected Friend pItInfo(,) As Double
    Protected Friend pOdds(,) As Double
    Protected Friend pSuccess As Integer
    Protected Friend pFail As Integer
    Protected Friend pNullDeviance As Double
    Protected Friend pFinalDeviance As Double
    Protected Friend pNullLogLikelihood As Double
    Protected Friend pLastIterLLchange As Double
    Protected Friend mu() As Double
    Protected Friend pLin_pred(,) As Double
    Protected Friend pScaleEstimation As String
    Protected Friend pbIntercept As Boolean

    Protected Friend pRaw_res() As Double          'Raw residuals
    Protected Friend pPearsChisq_res() As Double   'Pearson Chi-square residuals
    Protected Friend pDeviance_res() As Double     'Deviance residuals
    Protected Friend pStPearsChisq_res() As Double 'Standardized Pearson Chi-square residuals
    Protected Friend pStDeviance_res() As Double   'Standardized Deviance residuals
    Protected Friend pLeverage() As Double
    Protected Friend pCookDistance() As Double
    Protected Friend pbVarCovarComputed As Boolean
    Protected Friend pVarCovar(,) As Double 'variance convariance matrix
    Private pHosmerLemeshowTab(,) As Double
    Private pHosmerLemeshowTest As TestResult = New TestResult

    ''' <summary>
    ''' Returns a per-observation residual table: raw, deviance, Pearson, leverage, standardized residuals, and Cook’s distance.
    ''' </summary>
    ''' <value>
    ''' An <c>Object(,)</c> table with columns:
    ''' Raw Residual, Deviance Residual, Pearson Residual, Leverage, Std Deviance Residual, Std Pearson Residual, Cook’s D.
    ''' </value>
    ''' <remarks>
    ''' Definitions as implemented:
    ''' <list type="bullet">
    ''' <item><description><b>Raw</b>: <c>rᵢ = yᵢ − μᵢ</c></description></item>
    ''' <item><description><b>Pearson</b>: <c>rᵢ / sqrt(Var(μᵢ))</c></description></item>
    ''' <item><description><b>Deviance</b>: <c>pFamily.residDev(yᵢ, μᵢ)</c> (family-specific)</description></item>
    ''' <item><description><b>Leverage</b> (<c>hᵢ</c>): diagonal of the hat matrix computed from
    ''' <c>X_v = diag(sqrt(w)) X</c> and <c>VarCovar = (Xᵀ W X)⁻¹</c>:
    ''' <c>H = X_v VarCovar X_vᵀ</c>, so <c>hᵢ = Hᵢᵢ</c>.</description></item>
    ''' <item><description><b>Standardized Pearson</b>: <c>r_P / sqrt(1 − hᵢ)</c></description></item>
    ''' <item><description><b>Standardized Deviance</b>: <c>r_D / sqrt(1 − hᵢ)</c></description></item>
    ''' <item><description><b>Cook’s distance</b> (as coded):
    ''' <c>Dᵢ = ( (1/p) · (hᵢ/(1−hᵢ)) · (StdPearsonᵢ)² ) / ScaleSECoef</c>.</description></item>
    ''' </list>
    ''' </remarks>
    Public Overridable ReadOnly Property AllResiduals() As Object(,)
        Get
            Dim t = New ResultTable
            Dim o(n - 1, 6) As Double
            For i = 0 To n - 1
                o(i, 0) = Me.pRaw_res(i)
                o(i, 1) = Me.pDeviance_res(i)
                o(i, 2) = Me.pPearsChisq_res(i)
                o(i, 3) = Me.pLeverage(i)
                o(i, 4) = Me.pStDeviance_res(i)
                o(i, 5) = Me.pStPearsChisq_res(i)
                o(i, 6) = Me.pCookDistance(i)
            Next
            t.SetBody(o)
            t.AddHeaderTopRow({"Raw Resid.", "Deviance Resid.", "Pearson Resid.", "Laverage", "Std Deviance Resid.", "Std Pearson Resid.", "Cook Distance"})
            Return t.returnSelf()
        End Get
    End Property

    ''' <summary>
    ''' Returns the (unscaled) covariance matrix of the coefficient estimates: <c>(Xᵀ W X)⁻¹</c>.
    ''' </summary>
    ''' <remarks>
    ''' The code constructs the weighted design matrix via <c>X_v(i,j) = sqrt(wᵢ) X(i,j)</c>
    ''' and computes:
    ''' <para><c>VarCovar = (Xᵀ W X)⁻¹</c></para>
    ''' where <c>W = diag(wᵢ)</c> uses the final IRLS weights stored in <c>pFinalWeights</c>.
    ''' <para>
    ''' If <c>pbVarCovarComputed</c> is False, this property triggers computation.
    ''' </para>
    ''' </remarks>
    Public ReadOnly Property VarCovar() As Double(,)
        Get
            If Me.pbVarCovarComputed Then Return pVarCovar Else Return computeVarCovar()
        End Get
    End Property

    ''' <summary>
    ''' Pearson dispersion estimate <c>φ</c> computed as <c>X² / (n − p)</c>.
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' Here <c>X²</c> is the Pearson goodness-of-fit statistic:
    ''' <c>X² = Σ (yᵢ − μᵢ)² / Var(μᵢ)</c>,
    ''' and <c>n − p</c> is the residual degrees of freedom.
    ''' </para>
    ''' <para>
    ''' This property is primarily used when <see cref="ScaleSECoef"/> selects Pearson scaling for non-canonical families.
    ''' </para>
    ''' </remarks>
    Public ReadOnly Property DispestionParameterPhi() As Double
        Get
            Return Me.PearsonGOFchisq / Me.DFresid
        End Get
    End Property

    ''' <summary>
    ''' Residual degrees of freedom: <c>n − p</c>.
    ''' </summary>
    Public ReadOnly Property DFresid() As Integer
        Get
            Return Me.n - Me.p
        End Get
    End Property

    ''' <summary>
    ''' Pearson goodness-of-fit statistic <c>X²</c>.
    ''' </summary>
    ''' <remarks>
    ''' Computed as:
    ''' <para><c>X² = Σᵢ (yᵢ − μᵢ)² / Var(μᵢ)</c></para>
    ''' using the family variance function <c>Var(μ)</c>.
    ''' </remarks>
    Public ReadOnly Property PearsonGOFchisq() As Double
        Get
            Dim sum As Double = 0.0
            For i As Integer = 0 To Me.n - 1
                sum += ((Me.y(i) - Me.mu(i)) ^ 2 / Me.pFamily.Variance(Me.mu(i)))
            Next
            Return sum
        End Get
    End Property

    ''' <summary>
    ''' Upper-tail p-value for the Pearson goodness-of-fit statistic using a chi-square reference distribution.
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' Uses <c>df = n − p</c> and returns:
    ''' </para>
    ''' <para><c>p = 1 − F_{χ²(df)}(X²)</c>.</para>
    ''' <para>
    ''' Returns <see cref="Double.NaN"/> if <c>df ≤ 0</c>.
    ''' </para>
    ''' </remarks>
    Public ReadOnly Property PearsonGOFpvalue() As Double
        Get
            If Me.DFresid <= 0 Then Return Double.NaN
            Return 1.0 - distributions.ChiSquareCDF(Me.PearsonGOFchisq, Me.DFresid)
        End Get
    End Property

    ''' <summary>
    ''' Likelihood-ratio / deviance reduction statistic (G²) comparing the fitted model to the null model.
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' Computed as:
    ''' <c>G² = (D_null − D_model) / ScaleSECoef</c>,
    ''' where <c>D</c> is deviance and <see cref="ScaleSECoef"/> applies scale correction when appropriate.
    ''' </para>
    ''' </remarks>
    Public ReadOnly Property  DevianceG2chisq() As Double
        Get
            Return (Me.pNullDeviance - Me.pFinalDeviance) / Me.ScaleSECoef
        End Get
    End Property

    ''' <summary>
    ''' Degrees of freedom for the deviance reduction test (G²).
    ''' </summary>
    ''' <remarks>
    ''' If an intercept is included, the null model uses an intercept-only fit, and the df is <c>p − 1</c>.
    ''' Otherwise, df is <c>p</c>.
    ''' </remarks>
    Public ReadOnly Property DevianceG2df() As Integer
        Get
            If Me.pbIntercept Then
                Return Me.p - 1
            Else
                Return Me.p
            End If
        End Get
    End Property

    ''' <summary>
    ''' Upper-tail p-value for the deviance reduction test statistic (G²) using a chi-square reference distribution.
    ''' </summary>
    ''' <remarks>
    ''' Returns <c>1 − F_{χ²(df)}(G²)</c>, with <c>df</c> from <see cref="DevianceG2df"/>.
    ''' </remarks>
    Public ReadOnly Property DevianceG2pvalue() As Double
        Get
            Dim df As Integer = Me.DevianceG2df
            If df <= 0 Then Return Double.NaN
            Return 1.0 - distributions.ChiSquareCDF(Me.DevianceG2chisq, df)
        End Get
    End Property

    ''' <summary>
    ''' Deviance goodness-of-fit statistic: <c>D_model / ScaleSECoef</c>.
    ''' </summary>
    ''' <remarks>
    ''' Often compared to <c>χ²(n − p)</c> as an approximate GOF check.
    ''' </remarks>
    Public ReadOnly Property DevianceGOFchisq() As Double
        Get 'deviance goodnes of fit
            Return Me.pFinalDeviance / Me.ScaleSECoef
        End Get
    End Property

    ''' <summary>
    ''' Upper-tail p-value for the deviance goodness-of-fit statistic using a chi-square reference distribution.
    ''' </summary>
    ''' <remarks>
    ''' Returns <c>1 − F_{χ²(n−p)}(D_model/ScaleSECoef)</c>. Returns NaN if <c>n − p ≤ 0</c>.
    ''' </remarks>
    Public ReadOnly Property DevianceGOFpvalue() As Double
        Get
            If Me.DFresid <= 0 Then Return Double.NaN
            Return 1.0 - distributions.ChiSquareCDF(Me.DevianceGOFchisq, Me.DFresid)
        End Get
    End Property

    ''' <summary>
    ''' Scaled log-likelihood of the fitted model: <c>loglike(y, μ, scale)</c>.
    ''' </summary>
    ''' <remarks>
    ''' This calls <c>pFamily.loglike(y, μ, ScaleSECoef)</c>.
    ''' Depending on the family implementation, <c>scale</c> may affect the likelihood (e.g., Gaussian).
    ''' </remarks>
    Public ReadOnly Property LogLikelihood() As Double
        Get
            Return pFamily.loglike(Me.y, Me.mu, Me.ScaleSECoef)
        End Get
    End Property

    ''' <summary>
    ''' Unscaled log-likelihood of the fitted model: <c>loglike(y, μ, 1)</c>.
    ''' </summary>
    ''' <remarks>
    ''' This is the log-likelihood used by AIC/BIC/AICc properties in this code.
    ''' </remarks>
    Public ReadOnly Property LogLikelihoodUnscaled() As Double
        Get
            Return pFamily.loglike(Me.y, Me.mu, 1.0)
        End Get
    End Property

    ''' <summary>
    ''' Akaike Information Criterion (AIC) using the unscaled log-likelihood.
    ''' </summary>
    ''' <remarks>
    ''' Computed as <c>AIC = −2·LL + 2p</c>, where <c>LL</c> is <see cref="LogLikelihoodUnscaled"/>.
    ''' </remarks>
    Public Overridable ReadOnly Property AIC() As Double
        Get
            Return -2.0 * Me.LogLikelihoodUnscaled + 2.0 * Me.p
        End Get
    End Property

    ''' <summary>
    ''' Bayesian Information Criterion (BIC) using the unscaled log-likelihood.
    ''' </summary>
    ''' <remarks>
    ''' Computed as <c>BIC = −2·LL + log(n)·p</c>, where <c>LL</c> is <see cref="LogLikelihoodUnscaled"/>.
    ''' </remarks>
    Public Overridable ReadOnly Property BIC() As Double
        Get
            Return -2.0 * Me.LogLikelihoodUnscaled + Math.Log(Me.n) * Me.p
        End Get
    End Property

    ''' <summary>
    ''' Small-sample corrected AIC (AICc) using the unscaled log-likelihood.
    ''' </summary>
    ''' <remarks>
    ''' Computed here as:
    ''' <para><c>AICc = −2·LL + 2·p·n / ( (n−p) − 1 )</c></para>
    ''' Returns NaN if the denominator is non-positive.
    ''' </remarks>
    Public Overridable ReadOnly Property AICc() As Double
        Get
            Dim denom As Double = Me.DFresid - 1.0
            If denom <= 0.0 Then Return Double.NaN
            Return -2.0 * Me.LogLikelihoodUnscaled + (2.0 * Me.p * Me.n / denom)
        End Get
    End Property

    ''' <summary>
    ''' Pseudo R² based on the deviance ratio: <c>1 − D_model / D_null</c>.
    ''' </summary>
    ''' <remarks>
    ''' This is the quantity returned by the code (and labeled in output as pseudo R²).
    ''' It is deviance-based:
    ''' <para><c>R²_pseudo = 1 − D(β̂) / D_null</c>.</para>
    ''' If <c>D_null</c> is non-positive or not finite, returns 0.
    ''' </remarks>
    Public ReadOnly Property PseudoR2() As Double
        Get
            If Me.pNullDeviance <= 0.0 OrElse Double.IsNaN(Me.pNullDeviance) OrElse Double.IsInfinity(Me.pNullDeviance) Then
                Return 0.0
            End If
            Return 1.0 - Me.pFinalDeviance / Me.pNullDeviance
        End Get
    End Property

    ''' <summary>
    ''' Scale factor used for scaling deviance-based statistics and (optionally) standard errors.
    ''' </summary>
    ''' <remarks>
    ''' Returns:
    ''' <list type="bullet">
    ''' <item><description><c>1</c> for Binomial, Poisson, Negative Binomial.</description></item>
    ''' <item><description>If <c>pScaleEstimation="Pearson chisq"</c>: <see cref="DispestionParameterPhi"/>.</description></item>
    ''' <item><description>If <c>pScaleEstimation="Deviance"</c>: <c>D_model/(n−p)</c>.</description></item>
    ''' </list>
    ''' </remarks>
    Public ReadOnly Property ScaleSECoef() As Double
        Get
            If TypeOf pFamily Is regression.Binomial Or TypeOf pFamily Is regression.Poisson Or TypeOf pFamily Is regression.NegativeBinomial Then
                Return 1.0
            Else
                If pScaleEstimation = "Pearson chisq" Then
                    Return DispestionParameterPhi
                ElseIf pScaleEstimation = "Deviance" Then
                    Return pFinalDeviance / (Me.DFresid)
                ElseIf pScaleEstimation = "Maximum Likelihood" Then
                    BESHstatGlobals.BSerr.LogAndThrow(New NotImplementedException("Scale coeficient using Maximum likelihood method is not implemented yet."))
                    Return Nothing
                Else
                    Return 1.0
                End If
            End If
        End Get
    End Property

    ''' <summary>
    ''' Returns fitted means <c>μ</c> for each observation.
    ''' </summary>
    ''' <remarks>
    ''' Ordering matches the input rows passed to <see cref="data"/>.
    ''' </remarks>
    Public ReadOnly Property PredictedResponses() As Double()
        Get
            Return Me.mu
        End Get
    End Property

    ''' <summary>
    ''' Returns the design matrix <c>X</c> used in fitting (including intercept column if selected).
    ''' </summary>
    Public ReadOnly Property Xdata() As Double(,)
        Get  'predictor variables including intercept in the 1st column
            Return Me.x
        End Get
    End Property

    ''' <summary>
    ''' Returns the fitted linear predictor <c>η</c> for each observation (including the offset).
    ''' </summary>
    ''' <remarks>
    ''' <para><c>η = Xβ + offset</c></para>
    ''' Ordering matches the input rows passed to <see cref="data"/>.
    ''' </remarks>
    Public ReadOnly Property LinPred() As Double()
        Get
            Return GetColumnFrom2Darray(Me.pLin_pred, 0)
        End Get
    End Property

    ''' <summary>
    ''' Indicates whether the IRLS algorithm met the convergence criterion.
    ''' </summary>
    ''' <remarks>
    ''' Convergence is based on the absolute deviance change falling below <c>pEps</c>.
    ''' </remarks>
    Public ReadOnly Property Converged() As Boolean
        Get
            Return Me.pbConverged
        End Get
    End Property

    ''' <summary>
    ''' Supplies the observation-level dataset and optional offset/weights to the model.
    ''' </summary>
    ''' <param name="x">
    ''' Rectangular array where column 0 is the response <c>y</c> and remaining columns are predictors.
    ''' The intercept column is handled in <see cref="Fit"/> intercept argument.
    ''' </param>
    ''' <param name="RowNums">
    ''' Optional mapping back to original row indices; if omitted, uses <c>0..n−1</c>.
    ''' </param>
    ''' <param name="Offset">
    ''' Optional offset vector <c>o</c> added to the linear predictor:
    ''' <c>η = Xβ + o</c>. If omitted, a zero vector is used.
    ''' </param>
    ''' <param name="Weights">
    ''' Optional nonnegative weights <c>wBase</c>. If omitted, a vector of ones is used.
    ''' These weights enter IRLS as the multiplicative factor in the working weights:
    ''' <c>wᵢ = wBaseᵢ / ( (dη/dμ)² · Var(μᵢ) )</c>.
    ''' </param>
    ''' <remarks>
    ''' Offsets are treated as “present” (<c>pbOffset=True</c>) only if at least one offset element is nonzero.
    ''' </remarks>
    Public Sub data(x(,) As Double,
         Optional RowNums() As Integer = Nothing,
         Optional Offset() As Double = Nothing,
         Optional Weights() As Double = Nothing)

        pData = x

        ' Offsets are additive in eta (as used throughout GLM.Fit).
        ' Passing an all-zero offset should behave exactly like having no offset.
        pbOffset = False
        If Offset Is Nothing Then
            pOffset = BESHStatNG.IdentityVect(pData.GetUpperBound(0), 0)   ' length = n (rows)
        Else
            pOffset = Offset
            For i As Integer = 0 To Offset.GetUpperBound(0)
                If Offset(i) <> 0.0 Then
                    pbOffset = True
                    Exit For
                End If
            Next
        End If

        pbWeigts = (Weights IsNot Nothing)
        If Weights Is Nothing Then
            pWeights = BESHStatNG.IdentityVect(pData.GetUpperBound(0), 1)     ' length = n (rows)
        Else
            pWeights = Weights
        End If

        If RowNums Is Nothing Then
            ReDim pRowNums(x.GetUpperBound(0))
            For i As Integer = 0 To x.GetUpperBound(0)
                pRowNums(i) = i
            Next
        Else
            pRowNums = RowNums
        End If
    End Sub

    ''' <summary>
    ''' Sets general fitting controls (alpha, iteration limit, and convergence tolerance).
    ''' </summary>
    ''' <param name="dAlpha">Significance level used for intervals and p-values in output formatting.</param>
    ''' <param name="lMaxiter">Maximum IRLS iterations.</param>
    ''' <param name="dEps">Convergence tolerance for deviance change.</param>
    Public Sub settingInputs(dAlpha As Double, lMaxiter As Integer, dEps As Double)
        pAlpha = dAlpha
        pMaxiter = lMaxiter
        pEps = dEps
    End Sub

    ''' <summary>
    ''' Stores variable names used in reporting (tables/headers).
    ''' </summary>
    ''' <param name="names">
    ''' Names aligned to the data columns: index 0 is the response name; subsequent names are predictor names.
    ''' </param>
    ''' <remarks>
    ''' These labels do not affect estimation; they are used by <see cref="wrapResults"/> and by <c>LMresult</c>.
    ''' </remarks>
    Public Sub setVarNames(names() As String)
        Me.pVarNames = names
    End Sub

    ''' <summary>
    ''' Produces a list of formatted result tables (coefficients, model info, diagnostics, iteration history, etc.).
    ''' </summary>
    ''' <param name="strOffsetVar">Optional offset variable name to include as a footnote.</param>
    ''' <param name="strWeightsVar">Optional weights variable name to include as a footnote.</param>
    ''' <returns>A list of <c>ResultTable</c> objects suitable for UI/report rendering.</returns>
    ''' <remarks>
    ''' Typically includes:
    ''' <list type="bullet">
    ''' <item><description>Coefficient table with z/t statistics and p-values.</description></item>
    ''' <item><description>Model summary table (family/link, deviance, GOF tests, AIC/AICc/BIC, etc.).</description></item>
    ''' <item><description>Hosmer–Lemeshow table for Binomial (if enabled).</description></item>
    ''' <item><description>Iteration trace (if <see cref="bIterationDetails"/> True).</description></item>
    ''' <item><description>Covariance matrix table (if <see cref="bReturnCov"/> True).</description></item>
    ''' </list>
    ''' </remarks>
    Public Function wrapResults(Optional strOffsetVar As String = "",
                                Optional strWeightsVar As String = "") As List(Of ResultTable)
        Dim out As New List(Of ResultTable)
        Dim t = New ResultTable

        'coefficients, SE table
        t = Me.results.CoeffsZ_toPrint()
        t.AddPvalueToFormat(4)
        If strOffsetVar IsNot Nothing Then t.AddFootnote($"Offset Variable: {strOffsetVar}")
        If strWeightsVar IsNot Nothing Then t.AddFootnote($"Weights Variable: {strWeightsVar}")
        If Me.startParams IsNot Nothing Then t.AddFootnote($"Starting values: {array2str(Me.startParams)}")
        If Me.bSeparation Then
            t.AddFootnote("Complete separation of data points. Maximum likelihood estimates may not exist.")
        ElseIf Me.bQuasiSeparation Then
            t.AddFootnote("Quasi-separation of the iterative algorithm. Results may be misleading.")
        End If
        t.AddFootnote($"Computational time: {Me.CompTime} seconds.")
        out.Add(t)

        'Model Info
        out.Add(Me.results.getModelDiagnasticTable_toPrint())

        If TypeOf pFamily Is regression.Binomial Then
            t = New ResultTable
            t.SetBody({{Me.pSuccess}, {Me.pFail}})
            t.AddHeaderLeftRow({"Cases where Y>0", "Cases where Y=0"})
            out.Add(t)

            ' Odds ratios only make sense when there are slope parameters
            Dim nSlopes As Integer = Me.p - If(Me.pbIntercept, 1, 0)
            If nSlopes > 0 Then out.Add(Me.results.OR_toPrint)

            If bHosmerLemeshow Then
                'Hosmer Lemeshow Test
                t = New ResultTable
                t.SetBody(Me.pHosmerLemeshowTab)
                t.AddHeaderTopRow({"Group", "Cut Point", "Resp.>0 Obs", "Resp.>0 Exp", "Resp.= 0 Obs", "Resp.= 0 Exp", "Total"})
                t.AddTitle("Contingency table for Hosmer and Lemeshow test")
                t.AddFootnote($"Chi2={Me.pHosmerLemeshowTest.TestStatistics1}, DF={Me.pHosmerLemeshowTest.DF1}, P-value={Me.pHosmerLemeshowTest.Pvalue}")
                out.Add(t)
            End If
        End If

        'iteration info
        If Me.bIterationDetails Then
            t = New ResultTable
            t.SetBody(Me.pItInfo)
            Dim ItLabels(Me.pIRLSiterations - 1) As String
            For i = 0 To Me.pIRLSiterations - 1 : ItLabels(i) = $"Iteration {i + 1}" : Next
            t.AddHeaderTopRow(ItLabels)
            Dim vars = ConcatArrays(Me.pVarNames, {"LogLikelihood", "LogLikelihood Change"})
            If Me.pbIntercept Then vars(0) = "Intercept"
            t.AddHeaderLeftRow(vars)
            out.Add(t)
        End If

        'Return covariance
        If Me.bReturnCov Then
            t = New ResultTable
            t.SetBody(Me.computeVarCovar())
            Dim h(Me.pVarNames.Length - 1) As String
            h(0) = "Covariance matrix of parameters"
            t.AddHeaderTopRow(h)
            Dim vars = Me.pVarNames
            If Me.pbIntercept Then vars(0) = "Intercept"
            t.AddHeaderTopRow(vars)
            t.AddHeaderLeftRow(vars)
            out.Add(t)
        End If

        Return out
    End Function

    Shared Function SafePredictorNames(names() As String) As String()
        If names Is Nothing OrElse names.Length <= 1 Then
            Return New String() {} ' no predictors; intercept only model
        End If
        Return BESHStatNG.SubsetArray(names, 1)
    End Function

    ''' <summary>
    ''' Fits the GLM by IRLS and populates <see cref="results"/> and diagnostic properties.
    ''' </summary>
    ''' <param name="intercept">
    ''' 1 to include an intercept term; 0 to fit without an intercept.
    ''' </param>
    ''' <param name="bStartParams">
    ''' If <c>True</c>, uses <see cref="startParams"/> as initial <c>β</c>. Otherwise uses family-specific starting means.
    ''' </param>
    ''' <param name="progressBar">Optional UI progress bar updated during IRLS.</param>
    ''' <param name="progressLbl">Optional UI label updated with iteration count and deviance-change metric.</param>
    ''' <remarks>
    ''' <para>
    ''' The null deviance is computed “R-compatibly”:
    ''' </para>
    ''' <list type="bullet">
    ''' <item><description>If intercept is included, fits an intercept-only model by IRLS using the same offset and uses its deviance.</description></item>
    ''' <item><description>If no intercept, uses <c>η = offset</c> and computes deviance directly.</description></item>
    ''' </list>
    ''' <para>
    ''' For Binomial models, the implementation applies numerical safeguards:
    ''' </para>
    ''' <list type="bullet">
    ''' <item><description>Clamps <c>η</c> to avoid overflow in inverse link evaluation.</description></item>
    ''' <item><description>Keeps <c>μ</c> away from 0 and 1 to prevent exploding weights.</description></item>
    ''' <item><description>Uses step-halving when <c>μ</c> leaves the valid domain or when deviance increases.</description></item>
    ''' </list>
    ''' <para>
    ''' When <see cref="bComputeResiduals"/> is True, calls the internal residual routine to compute the table returned by
    ''' <see cref="AllResiduals"/>. When <see cref="bHosmerLemeshow"/> is True and the family is Binomial, computes the
    ''' Hosmer–Lemeshow test table and p-value.
    ''' </para>
    ''' </remarks>
    Public Overridable Sub Fit(intercept As Integer,
                               Optional bStartParams As Boolean = False,
                               Optional progressBar As System.Windows.Forms.ProgressBar = Nothing,
                               Optional progressLbl As System.Windows.Forms.Label = Nothing)
        'Intercept = 1 if Yes; 0 if No
        Dim j As Integer, pi1 As Integer, dev As Double, params(,) As Double, hold As Double, y_mean As Double
        Dim ii As Integer, weights() As Double, old_params() As Double, wlsendog() As Double
        'For logistic regression only
        Dim Sep As Double, StdErr() As Double
        Const LL As Double = 0.00000001, UL As Double = 0.99999999
        ' Only these Binomial links naturally produce mu in (0,1).
        ' For Binomial+Log and Binomial+Identity we MUST NOT clamp; we must step-half back in-bounds instead.
        Dim boundedBinomialLink As Boolean = (TypeOf pFamily Is regression.Binomial) AndAlso (TypeOf pLink Is regression.Logit OrElse TypeOf pLink Is regression.Probit)

        Dim startTime As Double = Microsoft.VisualBasic.DateAndTime.Timer
        Me.results = New LMresult

        'Set defaults
        pbConverged = False
        Me.pbIntercept = If(intercept = 1, True, False)

        'Set variable number constants
        pi1 = pData.GetLength(1)  'Columns of predictor variables and responses
        Me.p = pi1 - 1 + intercept '# of variables initially in the model.
        Me.n = pData.GetLength(0)     '# of observations

        If Me.p <= 0 Then
            BSerr.LogAndThrow(New ArgumentException("Model has no parameters (no intercept and no predictors)."))
            Me.strError += " Model has no parameters (no intercept and no predictors)."
            Exit Sub
        End If

        ReDim pItInfo(Me.p + 1, pMaxiter + 1) 'stores params estimates, LL at each iteration
        'Test for insufficient observations
        If n <= pi1 Then
            BSlogg.Log("Insufficient observations to complete analysis.", BESHstatGlobals.LogMsgType.Warn)
            Me.strError += " Insufficient observations to complete analysis."
            Exit Sub
        End If

        'Initialize arrays and format input data
        If TypeOf pFamily Is regression.Binomial Then
            Dim nSlopes As Integer = Me.p - intercept  ' number of non-intercept coefficients
            If nSlopes <= 0 Then
                ReDim pOdds(0, 4)  'dummy (not used)
            Else
                ReDim pOdds(nSlopes - 1, 4)
            End If
        End If

        Me.results.bIntercept = Me.pbIntercept
        Me.results.alpha = Me.pAlpha
        Me.results.varNames = SafePredictorNames(pVarNames)
        ReDim Me.y(Me.n - 1), x(Me.n - 1, Me.p - 1), StdErr(Me.p - 1), Me.results.Coeffs_est(Me.p - 1), Me.results.Coeffs_SEs(Me.p - 1), Me.results.Coeffs_SEsT(Me.p - 1)
        ReDim Me.mu(Me.n - 1), pLin_pred(Me.n - 1, 0), weights(Me.n - 1), wlsendog(Me.n - 1), old_params(Me.p - 1)

        For i = 0 To Me.n - 1
            Me.y(i) = pData(i, 0)  'It is assumed that response varaible is in the 1st column
            If Me.y(i) > 0 Then pSuccess += 1 'For logistic regression
        Next

        If CheckResponse(Me.y) Then Exit Sub
        pFail = n - pSuccess
        If intercept = 1 Then
            For i = 0 To n - 1
                x(i, 0) = 1.0
            Next
        End If
        For j = 1 To pi1 - 1 'all variables except Y
            For i = 0 To Me.n - 1
                If intercept = 1 Then
                    x(i, j) = pData(i, j)
                Else
                    x(i, j - 1) = pData(i, j) 'no itercept so j-1
                End If
            Next
        Next

        ' null deviance (match R: fit intercept-only model with the SAME offset in eta)
        Dim mu0(Me.n - 1) As Double

        If Me.pbIntercept Then
            pNullDeviance = ComputeNullDevianceByIRLS(mu0)
        Else
            ' No-intercept "null" model: eta = offset only
            For i = 0 To Me.n - 1
                Dim eta0 As Double = If(Me.pbOffset, Me.pOffset(i), 0.0)
                mu0(i) = pLink.inverse(eta0)
            Next
            pNullDeviance = pFamily.Deviance(Me.y, mu0)
        End If

        ' Use the same loglike routine as the fitted model (unscaled, like your AIC uses)
        pNullLogLikelihood = pFamily.loglike(Me.y, mu0, 1.0)


        'initial values
        y_mean = y.Average()
        If Not bStartParams Then
            For i = 0 To Me.n - 1
                Me.mu(i) = pFamily.startingMu(Me.y(i), y_mean)
                pLin_pred(i, 0) = pLink.transform(mu(i))
            Next
            ' Add offset once (offset is zeros when pbOffset=False)
            If Me.pbOffset Then pLin_pred = M_ADD(pLin_pred, pOffset)
        Else
            pLin_pred = MatrixMult(x, startParams)
            pLin_pred = M_ADD(pLin_pred, pOffset)
            For i = 0 To Me.n - 1
                Me.mu(i) = pLink.inverse(CDbl(pLin_pred(i, 0)))
            Next
        End If


        'Do IRLS iterations
        For pIRLSiterations = 0 To pMaxiter
            BSlogg.Log($"IRLS iteration #{pIRLSiterations}")
            For i = 0 To Me.n - 1
                weights(i) = pWeights(i) * 1.0 / (pLink.deriv(mu(i)) ^ 2 * pFamily.Variance(mu(i))) 'eim (expected information (hassian) matrix
                wlsendog(i) = pLin_pred(i, 0) + ((Me.y(i) - mu(i)) * pLink.deriv(mu(i))) - pOffset(i)
            Next

            params = MinimalWLS(wlsendog, x, weights)

            For i = 0 To UBound(params, 1)
                Me.results.Coeffs_est(i) = params(i, 0)
                StdErr(i) = params(i, 1)
            Next

            pLin_pred = MatrixMult(x, Me.results.Coeffs_est)
            pLin_pred = M_ADD(pLin_pred, pOffset)

            If TypeOf pFamily Is regression.Binomial Then
                Sep = 0
                For i = 0 To Me.n - 1
                    Dim eta As Double = CDbl(pLin_pred(i, 0))

                    ' avoid exp overflow
                    If eta < -700.0 Then eta = -700.0
                    If eta > 700.0 Then eta = 700.0

                    Me.mu(i) = pLink.inverse(eta)
                Next

                ' Separation diagnostics only make sense for bounded links (logit/probit).
                If (TypeOf pLink Is regression.Logit OrElse TypeOf pLink Is regression.Probit) Then
                    For i = 0 To Me.n - 1
                        If Me.mu(i) < LL Then
                            Me.mu(i) = LL
                            Sep += 1.0
                        ElseIf Me.mu(i) > UL Then
                            Me.mu(i) = UL
                            Sep += 1.0
                        End If
                    Next

                    If pIRLSiterations > 4 Then
                        If QSEP(Sep, n) Then Exit For
                    End If
                End If
            Else
                For i = 0 To Me.n - 1
                    Dim eta As Double = CDbl(pLin_pred(i, 0))

                    ' Prevent inverse-link blowups (mu = 1/eta) when eta approaches 0
                    If TypeOf pLink Is regression.Inverse OrElse
                        (TypeOf pLink Is regression.Power AndAlso CType(pLink, regression.Power).pwr < 0) Then
                        If Math.Abs(eta) < 0.000000000001 Then eta = If(eta >= 0.0, 0.000000000001, -0.000000000001)
                    End If

                    Me.mu(i) = pLink.inverse(eta)
                Next
            End If

            ' For non-binomial families, mu can still become NaN/Inf (e.g., inverse link / extreme eta).
            ' If so, do step-halving toward the previous iteration's parameters until mu is finite.
            If HasNonFinite(Me.mu) Then
                ii = 0
                BSlogg.Log("step size truncated: non-finite mu", LogMsgType.Warn)

                Do While HasNonFinite(Me.mu)
                    If (ii > pInnerLoopMaxIter) Then Exit Do
                    ii += 1

                    For k As Integer = 0 To UBound(Me.results.Coeffs_est)
                        Me.results.Coeffs_est(k) = (Me.results.Coeffs_est(k) + old_params(k)) / 2.0
                    Next

                    pLin_pred = MatrixMult(x, Me.results.Coeffs_est)
                    pLin_pred = M_ADD(pLin_pred, pOffset)

                    For r As Integer = 0 To Me.n - 1
                        Dim eta2 As Double = CDbl(pLin_pred(r, 0))
                        If TypeOf pLink Is regression.Inverse Then
                            If Math.Abs(eta2) < 0.000000000001 Then eta2 = If(eta2 >= 0.0, 0.000000000001, -0.000000000001)
                        End If
                        Me.mu(r) = pLink.inverse(eta2)
                    Next
                Loop

                If HasNonFinite(Me.mu) Then
                    BSlogg.Log("IRLS - Step size truncated: non-finite mu. Cannot correct step size.", LogMsgType.Warn)
                    Me.strError += " IRLS - Step size truncated: non-finite mu. Cannot correct step size."
                    Exit Sub
                End If
            End If

            'TODO: update the checkresponse function and do step halving when fit is outside of meaningfull range
            If CheckMu(mu) And TypeOf pFamily Is regression.Binomial Then

                If pIRLSiterations = 0 Then
                    BSlogg.Log("IRLS algorithm. No valid set of coefficients has been found: please supply starting values.", LogMsgType.Warn)
                    'Me.strError += " IRLS algorithm. No valid set of coefficients has been found: please supply starting values."
                Else

                    ii = 0
                    BSlogg.Log("step size truncated: out of bounds")
                    Do While (CheckMu(mu))
                        If (ii > pInnerLoopMaxIter) Then Exit Do
                        ii += 1
                        For i = 0 To UBound(Me.results.Coeffs_est)
                            Me.results.Coeffs_est(i) = (Me.results.Coeffs_est(i) + old_params(i)) / 2.0
                        Next
                        pLin_pred = MatrixMult(x, Me.results.Coeffs_est)
                        pLin_pred = M_ADD(pLin_pred, pOffset)
                        For i = 0 To Me.n - 1
                            Dim eta As Double = CDbl(pLin_pred(i, 0))
                            If eta < -700.0 Then eta = -700.0
                            If eta > 700.0 Then eta = 700.0

                            Me.mu(i) = pLink.inverse(eta)

                            If boundedBinomialLink Then
                                If Me.mu(i) < LL Then Me.mu(i) = LL
                                If Me.mu(i) > UL Then Me.mu(i) = UL
                            End If
                        Next
                    Loop
                    If (ii > pInnerLoopMaxIter) Then
                        BSlogg.Log("IRLS - Step size truncated: out of bounds. Inner loop 2; cannot correct step size.", LogMsgType.Warn)
                        Me.strError += " IRLS - Step size truncated: out of bounds. Inner loop 2; cannot correct step size."
                    Else
                        dev = pFamily.Deviance(y, mu)
                        BSlogg.Log($" Step halved: new deviance ={dev} ii ={ii}")
                    End If
                End If
            End If

            ' For binomial models (ALL links), keep mu away from 0 and 1 to avoid exploding weights.
            ' This matches R's glm behavior (mu is forced into (eps, 1-eps) each iteration).
            If TypeOf pFamily Is regression.Binomial Then
                For i = 0 To Me.n - 1
                    If Me.mu(i) < LL Then Me.mu(i) = LL
                    If Me.mu(i) > UL Then Me.mu(i) = UL
                Next
            End If

            dev = pFamily.Deviance(y, mu)
            If (((dev - hold) / (0.1 + Math.Abs(dev)) >= 0.00000001) And (pIRLSiterations > 0)) Then
                ii = 0

                BSlogg.Log(" step size truncated due to increasing deviance")
                Do While ((dev - hold) / (0.1 + Math.Abs(dev))) > 0.00000001

                    If (ii > pInnerLoopMaxIter) Then Exit Do
                    ii += 1
                    Debug.Print(array2str(Me.results.Coeffs_est))

                    For i = 0 To UBound(Me.results.Coeffs_est)
                        Me.results.Coeffs_est(i) = (Me.results.Coeffs_est(i) + old_params(i)) / 2.0
                    Next
                    pLin_pred = MatrixMult(x, Me.results.Coeffs_est)
                    pLin_pred = M_ADD(pLin_pred, pOffset)

                    For i = 0 To Me.n - 1
                        Dim eta As Double = CDbl(pLin_pred(i, 0))
                        If eta < -700.0 Then eta = -700.0
                        If eta > 700.0 Then eta = 700.0

                        Me.mu(i) = pLink.inverse(eta)

                        If boundedBinomialLink Then
                            If Me.mu(i) < LL Then Me.mu(i) = LL
                            If Me.mu(i) > UL Then Me.mu(i) = UL
                        End If
                    Next

                    dev = pFamily.Deviance(y, mu)
                    BSlogg.Log($"inner loop 3; ii={ii} dev={dev} hold={hold}")
                Loop
                If (ii > pInnerLoopMaxIter) Then
                    BSlogg.Log("IRLS - Step size truncated due to increasing deviance. Inner loop 3; cannot correct step size.", LogMsgType.Warn)
                    Me.strError += " IRLS - Step size truncated due to increasing deviance. Inner loop 3; cannot correct step size."
                Else
                    BSlogg.Log($" Step halved: new deviance ={dev} ii ={ii}")
                End If
            End If

            'save iteration info
            If Me.bIterationDetails Then
                For i = 0 To Me.p
                    pItInfo(i, pIRLSiterations) = If(i = p, dev, Me.results.Coeffs_est(i))
                Next
            End If

            BSlogg.Log($" eps={pEps} Abs(Abs(dev) - Abs(hold))={Math.Abs(Math.Abs(dev) - Math.Abs(hold))}")
            If pIRLSiterations > 0 Then

                pLastIterLLchange = Math.Abs(Math.Abs(dev) - Math.Abs(hold))
                pItInfo(Me.p + 1, pIRLSiterations) = pLastIterLLchange
                If progressBar IsNot Nothing Then
                    progressBar.Invoke(Sub()
                                           progressBar.Value = 100 * (Me.pIRLSiterations + 1) / (Me.pMaxiter + 1)
                                           If progressLbl IsNot Nothing Then progressLbl.Text = $"Elapsed Time: {Math.Round((Microsoft.VisualBasic.DateAndTime.Timer - startTime), 2)}[s]   Iterations: {Me.pIRLSiterations + 1}   LogLikelihood change = {pLastIterLLchange}"
                                       End Sub)
                    System.Windows.Forms.Application.DoEvents()
                End If

                If pLastIterLLchange < pEps Then 'AndAlso (maxDelta < pEps) Then
                    pbConverged = True
                    Exit For
                End If
            End If

            'save data for next iteration
            Me.results.Coeffs_est.CopyTo(old_params, 0)
            hold = dev
        Next pIRLSiterations

        Me.pFinalDeviance = dev
        ReDim Me.pFinalWeights(UBound(weights))
        For i = 0 To UBound(weights) : Me.pFinalWeights(i) = weights(i) : Next

        'Test for convergence or divergence and warn user
        If pIRLSiterations >= pMaxiter + 1 Then 'Too many iterations
            BSlogg.Log("Algorithm failed To converge. Results may be misleading. Excessive iterations Of IRLS algorithm In .Fit. ", LogMsgType.Warn)
            Me.strError += " Algorithm failed To converge. Results may be misleading. Excessive iterations Of IRLS algorithm In .Fit. "
        ElseIf Not pbConverged Then
            BSlogg.Log("Algorithm Is diverging. Failure Of IRLS algorithm In .Fit.", LogMsgType.Warn)
            Me.strError += " Algorithm Is diverging. Failure Of IRLS algorithm In .Fit."
        End If
        If Me.bIterationDetails Then
            If pIRLSiterations > 0 Then ReDim Preserve pItInfo(UBound(pItInfo, 1), pIRLSiterations) Else ReDim Preserve pItInfo(UBound(pItInfo, 1), 0)
        End If
        pIRLSiterations += 1

        'Fit model coefficient estimates, standard errors, pZ and Chi2
        'statistics, and upper and lower confidence intervals for parameters
        For i = 0 To Me.p - 1
            Me.results.Coeffs_SEs(i) = (StdErr(i) / Math.Sqrt(DispestionParameterPhi)) * Math.Sqrt(ScaleSECoef) ': SE
            Me.results.Coeffs_SEsT(i) = StdErr(i)                             ': SE
        Next

        BSlogg.Log($"pCoefs{array2str(Me.results.CoeffsZ_vals)} {MethodBase.GetCurrentMethod().Name}")

        If Me.bComputeResiduals Then Me.Residuals()
        If Me.bHosmerLemeshow And TypeOf pFamily Is regression.Binomial Then Me.HosmerLemeshowTest()

        Me.results.ModelTableLabels = {"Family", "Link Function", "Null deviance", "Residual deviance", "Log Likelihood",
                "# observations", "Deviance G² (likelihood ratio) chisq", "Deviance goodness of fit chisq",
                "Pearson goodness of fit chisq", "Pseudo(McFadden) R²", "AIC", "AICc", "BIC", "Scale",
                "Number of Iterations", "Relative Log - Likelihood Change", "Converged?"}
        Me.results.ModelTableVals = {{Me.pFamily.ToString(), "", ""},
                                    {Me.pLink.ToString(), "", ""},
                                    {Me.pNullDeviance, "", ""},
                                    {Me.pFinalDeviance, "", ""},
                                    {Me.LogLikelihood(), "", ""},
                                    {Me.n, "", ""},
                                    {Me.DevianceG2chisq, Me.DevianceG2df, Me.DevianceG2pvalue},
                                    {Me.DevianceGOFchisq, Me.DFresid, Me.DevianceGOFpvalue},
                                    {Me.PearsonGOFchisq, Me.DFresid, Me.PearsonGOFpvalue},
                                    {Me.PseudoR2, "", ""},
                                    {Me.AIC, Me.p, ""},
                                    {Me.AICc, Me.p, ""},
                                    {Me.BIC, Me.p, ""},
                                    {Me.ScaleSECoef, "", ""},
                                    {Me.pIRLSiterations, "", ""},
                                    {Me.pLastIterLLchange, "", ""},
                                    {CStr(Me.pbConverged), "", ""}}

        Me.CompTime = Microsoft.VisualBasic.DateAndTime.Timer - startTime
        If progressBar IsNot Nothing Then progressBar.Invoke(Sub()
                                                                 progressBar.Value = 100
                                                             End Sub)
    End Sub


    ' Fits the intercept-only GLM via IRLS using the SAME offset in eta (R-compatible null model).
    ' Returns the null deviance and outputs mu0 (fitted means under the null).
    ' Fits the intercept-only GLM via IRLS using the SAME offset in eta (R-compatible null model).
    ' Returns the null deviance and outputs mu0 (fitted means under the null).
    Private Function ComputeNullDevianceByIRLS(ByRef mu0() As Double) As Double

        Dim n As Integer = Me.n
        ReDim mu0(n - 1)

        ' --- constants/flags matching the main IRLS routine ---
        Const LL As Double = 0.00000001
        Const UL As Double = 0.99999999

        ' Only these Binomial links naturally produce mu in (0,1).
        Dim boundedBinomialLink As Boolean =
        (TypeOf pFamily Is regression.Binomial) AndAlso
        (TypeOf pLink Is regression.Logit OrElse TypeOf pLink Is regression.Probit)

        ' Design matrix for intercept-only model
        Dim x0(n - 1, 0) As Double
        For i As Integer = 0 To n - 1
            x0(i, 0) = 1.0
        Next

        Dim eta0(n - 1, 0) As Double
        Dim weights0(n - 1) As Double
        Dim wlsendog0(n - 1) As Double

        ' Starting values (same approach as the full model)
        Dim yMean As Double = Me.y.Average()
        For i As Integer = 0 To n - 1
            mu0(i) = pFamily.startingMu(Me.y(i), yMean)
            eta0(i, 0) = pLink.transform(mu0(i))
        Next

        ' Add offset once (offset is all-zeros if pbOffset=False; but guard anyway)
        If Me.pbOffset AndAlso Me.pOffset IsNot Nothing Then
            eta0 = M_ADD(eta0, Me.pOffset)
        End If

        Dim devOld As Double = Double.PositiveInfinity
        Dim devNew As Double = Double.PositiveInfinity
        Dim betaOld As Double = 0.0
        Dim betaNew As Double = 0.0

        For iter As Integer = 0 To Me.pMaxiter

            ' --- Build IRLS working weights and response (same form as main fit) ---
            For i As Integer = 0 To n - 1
                Dim off As Double = If(Me.pbOffset AndAlso Me.pOffset IsNot Nothing, Me.pOffset(i), 0.0)
                weights0(i) = Me.pWeights(i) * 1.0 / (pLink.deriv(mu0(i)) ^ 2 * pFamily.Variance(mu0(i)))
                wlsendog0(i) = eta0(i, 0) + ((Me.y(i) - mu0(i)) * pLink.deriv(mu0(i))) - off
            Next

            Dim params0 As Double(,) = MinimalWLS(wlsendog0, x0, weights0)
            betaNew = params0(0, 0)

            ' --- Update eta and mu under the null: eta = beta0 + offset ---
            For i As Integer = 0 To n - 1
                Dim off As Double = If(Me.pbOffset AndAlso Me.pOffset IsNot Nothing, Me.pOffset(i), 0.0)
                Dim eta As Double = betaNew + off

                If TypeOf pFamily Is regression.Binomial Then
                    ' avoid exp overflow
                    If eta < -700.0 Then eta = -700.0
                    If eta > 700.0 Then eta = 700.0

                    Dim muVal As Double = pLink.inverse(eta)

                    ' Separation diagnostics not needed for null fit; only keep bounded links in-range
                    If boundedBinomialLink Then
                        If muVal < LL Then muVal = LL
                        If muVal > UL Then muVal = UL
                    End If

                    mu0(i) = muVal
                Else
                    ' Prevent inverse-link blowups (mu = 1/eta) when eta approaches 0
                    If TypeOf pLink Is regression.Inverse OrElse
                   (TypeOf pLink Is regression.Power AndAlso CType(pLink, regression.Power).pwr < 0) Then
                        If Math.Abs(eta) < 0.000000000001 Then
                            eta = If(eta >= 0.0, 0.000000000001, -0.000000000001)
                        End If
                    End If

                    mu0(i) = pLink.inverse(eta)
                End If

                eta0(i, 0) = eta
            Next

            ' --- Non-finite mu safeguard (matches main fit behavior) ---
            If HasNonFinite(mu0) Then
                Dim ii As Integer = 0
                Do While HasNonFinite(mu0) AndAlso ii < Me.pInnerLoopMaxIter
                    ii += 1
                    betaNew = (betaNew + betaOld) / 2.0

                    For i As Integer = 0 To n - 1
                        Dim off As Double = If(Me.pbOffset AndAlso Me.pOffset IsNot Nothing, Me.pOffset(i), 0.0)
                        Dim eta As Double = betaNew + off

                        If TypeOf pFamily Is regression.Binomial Then
                            If eta < -700.0 Then eta = -700.0
                            If eta > 700.0 Then eta = 700.0
                            Dim muVal As Double = pLink.inverse(eta)
                            If boundedBinomialLink Then
                                If muVal < LL Then muVal = LL
                                If muVal > UL Then muVal = UL
                            End If
                            mu0(i) = muVal
                        Else
                            If TypeOf pLink Is regression.Inverse OrElse
                           (TypeOf pLink Is regression.Power AndAlso CType(pLink, regression.Power).pwr < 0) Then
                                If Math.Abs(eta) < 0.000000000001 Then
                                    eta = If(eta >= 0.0, 0.000000000001, -0.000000000001)
                                End If
                            End If
                            mu0(i) = pLink.inverse(eta)
                        End If

                        eta0(i, 0) = eta
                    Next
                Loop
            End If

            ' --- Binomial out-of-bounds safeguard (same pattern as main fit) ---
            If (TypeOf pFamily Is regression.Binomial) AndAlso CheckMu(mu0) Then
                Dim ii As Integer = 0
                Do While CheckMu(mu0) AndAlso ii < Me.pInnerLoopMaxIter
                    ii += 1
                    betaNew = (betaNew + betaOld) / 2.0

                    For i As Integer = 0 To n - 1
                        Dim off As Double = If(Me.pbOffset AndAlso Me.pOffset IsNot Nothing, Me.pOffset(i), 0.0)
                        Dim eta As Double = betaNew + off

                        If eta < -700.0 Then eta = -700.0
                        If eta > 700.0 Then eta = 700.0

                        Dim muVal As Double = pLink.inverse(eta)

                        If boundedBinomialLink Then
                            If muVal < LL Then muVal = LL
                            If muVal > UL Then muVal = UL
                        End If

                        mu0(i) = muVal
                        eta0(i, 0) = eta
                    Next
                Loop
            End If

            ' For binomial models (ALL links), keep mu away from 0 and 1 (matches main fit)
            If TypeOf pFamily Is regression.Binomial Then
                For i As Integer = 0 To n - 1
                    If mu0(i) < LL Then mu0(i) = LL
                    If mu0(i) > UL Then mu0(i) = UL
                Next
            End If

            devNew = pFamily.Deviance(Me.y, mu0)

            ' --- Step-halving if deviance increases (same style as main fit) ---
            If (iter > 0) AndAlso (((devNew - devOld) / (0.1 + Math.Abs(devNew))) > 0.00000001) Then
                Dim ii As Integer = 0
                Do While ((devNew - devOld) / (0.1 + Math.Abs(devNew))) > 0.00000001 AndAlso ii < Me.pInnerLoopMaxIter
                    ii += 1
                    betaNew = (betaNew + betaOld) / 2.0

                    For i As Integer = 0 To n - 1
                        Dim off As Double = If(Me.pbOffset AndAlso Me.pOffset IsNot Nothing, Me.pOffset(i), 0.0)
                        Dim eta As Double = betaNew + off

                        If TypeOf pFamily Is regression.Binomial Then
                            If eta < -700.0 Then eta = -700.0
                            If eta > 700.0 Then eta = 700.0

                            Dim muVal As Double = pLink.inverse(eta)
                            If boundedBinomialLink Then
                                If muVal < LL Then muVal = LL
                                If muVal > UL Then muVal = UL
                            End If
                            mu0(i) = muVal
                        Else
                            If TypeOf pLink Is regression.Inverse OrElse
                           (TypeOf pLink Is regression.Power AndAlso CType(pLink, regression.Power).pwr < 0) Then
                                If Math.Abs(eta) < 0.000000000001 Then
                                    eta = If(eta >= 0.0, 0.000000000001, -0.000000000001)
                                End If
                            End If
                            mu0(i) = pLink.inverse(eta)
                        End If

                        eta0(i, 0) = eta
                    Next

                    If TypeOf pFamily Is regression.Binomial Then
                        For i As Integer = 0 To n - 1
                            If mu0(i) < LL Then mu0(i) = LL
                            If mu0(i) > UL Then mu0(i) = UL
                        Next
                    End If

                    devNew = pFamily.Deviance(Me.y, mu0)
                Loop
            End If

            ' Convergence: deviance change (same criterion as main fit)
            If Math.Abs(devOld - devNew) < Me.pEps Then
                devOld = devNew
                Exit For
            End If

            devOld = devNew
            betaOld = betaNew
        Next

        Return devOld
    End Function



    Private Shared Function HasNonFinite(vals() As Double) As Boolean
        If vals Is Nothing Then Return True
        For i As Integer = 0 To vals.GetUpperBound(0)
            Dim v As Double = vals(i)
            If Double.IsNaN(v) OrElse Double.IsInfinity(v) Then Return True
        Next
        Return False
    End Function

    Private Function CheckResponse(vals() As Double) As Boolean
        ' Returns TRUE if values are OUTSIDE the valid range (i.e., response check FAILED).
        ' Does NOT throw. Throwing should be handled by the caller after step-halving attempts.
        If vals Is Nothing OrElse vals.Length = 0 Then Return True

        If TypeOf pFamily Is regression.Binomial Then
            For i As Integer = 0 To vals.GetUpperBound(0)
                Dim v As Double = vals(i)
                If Double.IsNaN(v) OrElse Double.IsInfinity(v) OrElse v < 0.0 OrElse v > 1.0 Then
                    Return True
                End If
            Next
            Return False
        End If

        If TypeOf pFamily Is regression.Poisson OrElse TypeOf pFamily Is regression.NegativeBinomial Then
            For i As Integer = 0 To vals.GetUpperBound(0)
                Dim v As Double = vals(i)
                If Double.IsNaN(v) OrElse Double.IsInfinity(v) OrElse v < 0.0 Then
                    Return True
                End If
            Next
            Return False
        End If

        ' Gaussian: any finite value is acceptable
        Return HasNonFinite(vals)
    End Function

    ' Validates FITTED VALUES (mu), not the raw response y.
    ' For binomial, mu must be strictly inside (0,1) (R behaves this way).
    Private Function CheckMu(vals() As Double) As Boolean
        If vals Is Nothing OrElse vals.Length = 0 Then Return True

        If TypeOf pFamily Is regression.Binomial Then
            Const epsMu As Double = 0.000000000001
            For i As Integer = 0 To vals.GetUpperBound(0)
                Dim v As Double = vals(i)
                If Double.IsNaN(v) OrElse Double.IsInfinity(v) Then Return True
                If v <= epsMu OrElse v >= 1.0 - epsMu Then Return True
            Next
            Return False
        End If

        If TypeOf pFamily Is regression.Poisson OrElse TypeOf pFamily Is regression.NegativeBinomial Then
            For i As Integer = 0 To vals.GetUpperBound(0)
                Dim v As Double = vals(i)
                If Double.IsNaN(v) OrElse Double.IsInfinity(v) OrElse v < 0.0 Then Return True
            Next
            Return False
        End If

        ' Gaussian etc: only finite is required
        Return HasNonFinite(vals)
    End Function


    Private Function QSEP(ByRef Sep As Double, ByRef n As Integer) As Boolean
        'Function tests for complete and quasi-separation for an IRLS logistic regression
        'Arguements:
        ' Sep = proportion of times an extreme fitted value (near 0 or 1) is observed in the current iteration
        ' n = # of observations in the data.
        QSEP = False

        If Sep / CDbl(n) >= 0.0001 Then 'Complete separation
            BSlogg.Log("Complete separation of data points. Maximum likelihood estimates may not exist. Ending Computation.", LogMsgType.Warn)
            Me.strError += " Complete separation of data points. Maximum likelihood estimates may not exist. Ending Computation."
            QSEP = True
            Me.bSeparation = True
            Me.bQuasiSeparation = True
            Exit Function
        ElseIf Sep / CDbl(n) >= 0.05 Then 'Quasi-separation
            BSlogg.Log("Quasi-separation of the iterative algorithm.", LogMsgType.Warn)
            Me.bQuasiSeparation = True
            If MsgBox(Prompt:="Quasi-separation of the iterative algorithm." & vbCr & vbCr & "Results may be misleading.", Title:="Continue?") = vbNo Then
                QSEP = True
                Me.strError += " Quasi-separation of the iterative algorithm."
                Exit Function
            End If
        End If
    End Function

    Protected Friend Function computeVarCovar(Optional bForceRecalculate As Boolean = False) As Double(,)
        'Call this once IRLS is done
        Dim xv(n - 1, p - 1) As Double

        If Not pbVarCovarComputed Or bForceRecalculate Then
            For j = 0 To p - 1
                For i = 0 To n - 1
                    xv(i, j) = Math.Sqrt(Me.pFinalWeights(i)) * Me.x(i, j)
                Next
            Next
            pVarCovar = hessian(x, Me.pFinalWeights)
            pbVarCovarComputed = True
            Return pVarCovar
        Else
            Return pVarCovar
        End If
    End Function

    Private Function hessian(x(,) As Double, V() As Double) As Double(,)
        'compute hessian matrix X'VX for regression analysis
        Dim tmp1 As Double
        Dim n As Integer = x.GetLength(0)
        Dim p As Integer = x.GetLength(1) 'number of regression parameters
        Dim XtWX(p - 1, p - 1) As Double

        'Compute X'VX
        For i = 0 To p - 1
            For j = i To p - 1
                tmp1 = 0.0
                For k = 0 To n - 1
                    tmp1 += x(k, j) * V(k) * x(k, i)
                Next
                XtWX(i, j) = tmp1
                XtWX(j, i) = tmp1
            Next
        Next
        Return MatInv(XtWX, "CHOL")
    End Function

    Protected Friend Sub Residuals()
        'call this sub only after we have parameters estimated
        Dim xv(n - 1, p - 1) As Double
        ReDim pRaw_res(n - 1), pPearsChisq_res(n - 1), pDeviance_res(n - 1), pCookDistance(n - 1)
        ReDim pLeverage(n - 1), pStPearsChisq_res(n - 1), pStDeviance_res(n - 1)

        For i = 0 To n - 1
            pRaw_res(i) = Me.y(i) - Me.mu(i)
            pPearsChisq_res(i) = pRaw_res(i) / Math.Sqrt(pFamily.Variance(mu(i)))
            pDeviance_res(i) = pFamily.residDev(Me.y(i), mu(i))
        Next

        'Compute Leverage - diagonals of the Hat matrix H
        For j = 0 To p - 1
            For i = 0 To n - 1
                xv(i, j) = Math.Sqrt(Me.pFinalWeights(i)) * x(i, j)
            Next
        Next
        Dim temp_db2 = MatrixMult(MatrixMult(xv, VarCovar), trans(xv))
        For i = 0 To n - 1
            pLeverage(i) = temp_db2(i, i) 'Leverage
            pStPearsChisq_res(i) = pPearsChisq_res(i) / Math.Sqrt(1.0 - pLeverage(i)) 'Std Pearson
            pStDeviance_res(i) = pDeviance_res(i) / Math.Sqrt(1.0 - pLeverage(i)) 'Std Deviance
            pCookDistance(i) = ((1.0 / p) * (pLeverage(i) / (1.0 - pLeverage(i))) * pStPearsChisq_res(i) ^ 2) / ScaleSECoef
        Next
    End Sub

    Private Sub HosmerLemeshowTest()
        'Fit Hosmer Lemeshow Goodness of Fit test
        'Computed as described in Hosmer, Lemeshow Applied Logistic Regression, 3rd ed. page 170 STATA
        Dim temp(9) As Double

        ' 1) Compute deciles of predicted probabilities -------------------------
        With app.WorksheetFunction
            For i = 0 To 9
                temp(i) = .Percentile(mu, (i + 1) / 10)
            Next
        End With
        Dim percentiles() As Double = temp.Distinct().ToArray() 'if some percentiles are identical then collaps them
        Dim bins = percentiles.Length - 1

        ' 2) Combine y and mu into a single sequence ----------------------------
        Dim data = Enumerable.Range(0, n).Select(Function(i) New With {.y = y(i), .mu = mu(i)})

        ' 3) Local helper to assign bin index
        Dim getBin = Function(muVal As Double) As Integer
                         Dim idx As Integer = Array.FindIndex(percentiles, Function(p) muVal <= p)
                         Return If(idx = -1, percentiles.Length - 1, idx)
                     End Function

        ' 4) Group by bin index
        Dim grouped = data.GroupBy(Function(obs) getBin(obs.mu)).OrderBy(Function(g) g.Key).ToList()

        ' 5) Aggregate per bin
        Dim results = grouped.Select(Function(g, k) New With {
                                        .Bin = k + 1,
                                        .Cut = percentiles(k),
                                        .SuccessObs = g.Count(Function(o) o.y > 0),
                                        .FailureObs = g.Count(Function(o) o.y = 0),
                                        .SuccessExp = g.Sum(Function(o) o.mu),
                                        .FailureExp = g.Count() - g.Sum(Function(o) o.mu)}).ToList()

        ' 6) HL chi-square
        pHosmerLemeshowTest.TestStatistics1 = results.Sum(
            Function(r)
                Dim s As Double = 0.0
                If r.SuccessExp > 0 Then s += (r.SuccessObs - r.SuccessExp) ^ 2 / r.SuccessExp
                If r.FailureExp > 0 Then s += (r.FailureObs - r.FailureExp) ^ 2 / r.FailureExp
                Return s
            End Function)

        ' 7) DF and p-value
        pHosmerLemeshowTest.DF1 = Math.Max(0, results.Count - 2)
        If pHosmerLemeshowTest.DF1 > 0 Then
            pHosmerLemeshowTest.Pvalue = 1.0 - distributions.ChiSquareCDF(pHosmerLemeshowTest.TestStatistics1, CDbl(pHosmerLemeshowTest.DF1))
        Else
            pHosmerLemeshowTest.Pvalue = Double.NaN
        End If

        'Create table For Hosmer Lemeshow test (see Hosmer Lemeshow, Applied Logistic regression table 5.1
        ' 8) Output table
        ReDim pHosmerLemeshowTab(results.Count - 1, 6)
        For i = 0 To results.Count - 1
            Dim r = results(i)
            pHosmerLemeshowTab(i, 0) = r.Bin
            pHosmerLemeshowTab(i, 1) = r.Cut
            pHosmerLemeshowTab(i, 2) = r.SuccessObs
            pHosmerLemeshowTab(i, 3) = r.SuccessExp
            pHosmerLemeshowTab(i, 4) = r.FailureObs
            pHosmerLemeshowTab(i, 5) = r.FailureExp
            pHosmerLemeshowTab(i, 6) = r.SuccessObs + r.FailureObs
        Next
    End Sub
End Class


''' <summary>
''' Negative Binomial GLM fitted by alternating between GLM coefficient updates and dispersion (theta/alpha) updates.
''' </summary>
''' <remarks>
''' <para>
''' This class inherits <see cref="GLM"/> but implements a <see cref="Fit"/> procedure modeled after
''' the MASS::glm.nb algorithm in R (iterating between:
''' </para>
''' <list type="bullet">
''' <item><description>fitting a Negative Binomial GLM for fixed dispersion, and</description></item>
''' <item><description>re-estimating dispersion by (approximate) maximum likelihood given the fitted means.</description></item>
''' </list>
''' <para>
''' Parameterization used in code:
''' <c>alpha = 1/theta</c>, exposed by <see cref="NBalpha"/>.
''' </para>
''' </remarks>
Public Class GLM_NB
    Inherits GLM

    Private pLastIterDispersionChange As Double
    Private pNBglm As GLM = Nothing

    ''' <summary>
    ''' Initializes a Negative Binomial GLM with the specified link function.
    ''' </summary>
    ''' <param name="l">Link function for the mean model (commonly log link).</param>
    ''' <remarks>
    ''' Internally sets the family to <c>regression.NegativeBinomial</c>.
    ''' </remarks>
    Public Sub New(l As regression.Link)
        MyBase.New(New regression.NegativeBinomial, l)
    End Sub

    ''' <summary>
    ''' Returns the Negative Binomial dispersion parameter <c>alpha</c> (where <c>alpha = 1/theta</c>).
    ''' </summary>
    ''' <remarks>
    ''' In NB2 form: <c>Var(Y|μ) = μ + alpha·μ²</c> (typical convention).
    ''' The exact variance form depends on your <c>regression.NegativeBinomial</c> implementation.
    ''' </remarks>
    Public ReadOnly Property NBalpha() As Double
        'negative binomial dispersion parameter (1 / theta)
        Get
            Return Me.pNBglm.pFamily.pdAlpha
        End Get
    End Property

    ''' <summary>
    ''' AIC for Negative Binomial GLM counting the dispersion parameter as an additional fitted parameter.
    ''' </summary>
    ''' <remarks>
    ''' Computed as:
    ''' <para><c>AIC = −2·LL + 2·(p + 1)</c></para>
    ''' where <c>LL</c> is the unscaled log-likelihood of the fitted NB model and <c>p</c> is the number of mean parameters.
    ''' </remarks>
    Public Overloads ReadOnly Property AIC() As Double
        'here we have number of parameters + 1 because of the alpha (NB dispersion) parameter estimation
        Get
            Return -2.0 * Me.pNBglm.LogLikelihoodUnscaled + 2.0 * (Me.p + 1)
        End Get
    End Property

    ''' <summary>
    ''' BIC for Negative Binomial GLM counting the dispersion parameter as an additional fitted parameter.
    ''' </summary>
    ''' <remarks>
    ''' Computed as:
    ''' <para><c>BIC = −2·LL + log(n)·(p + 1)</c></para>
    ''' </remarks>
    Public Overloads ReadOnly Property BIC() As Double
        'here we have number of parameters + 1 because of the alpha (NB dispersion) parameter estimation
        Get
            Return -2.0 * Me.pNBglm.LogLikelihoodUnscaled + Math.Log(Me.n) * (Me.p + 1)
        End Get
    End Property

    ''' <summary>
    ''' Small-sample corrected AIC (AICc) for Negative Binomial GLM counting the dispersion parameter.
    ''' </summary>
    ''' <remarks>
    ''' Computed here as:
    ''' <para><c>AICc = −2·LL + 2·(p + 1)·n / (n − p)</c></para>
    ''' Returns NaN if <c>n − p ≤ 0</c>.
    ''' </remarks>
    Public Overloads ReadOnly Property AICc() As Double
        'here we have number of parameters + 1 because of the alpha (NB dispersion) parameter estimation
        Get
            Dim denom As Double = (Me.n - Me.p)
            If denom <= 0 Then Return Double.NaN
            Return -2.0 * Me.pNBglm.LogLikelihoodUnscaled + (2.0 * (Me.p + 1) * Me.n / denom)
        End Get
    End Property

    ''' <summary>
    ''' Returns residual diagnostics from the internally fitted NB GLM.
    ''' </summary>
    ''' <remarks>
    ''' This override forces residual computation on the internal GLM instance and returns its <see cref="GLM.AllResiduals"/>.
    ''' </remarks>
    Public Overrides ReadOnly Property AllResiduals() As Object(,)
        Get
            Me.pNBglm.bComputeResiduals = True
            Me.pNBglm.Residuals()
            Return Me.pNBglm.AllResiduals()
        End Get
    End Property

    ''' <summary>
    ''' Estimates the Negative Binomial dispersion parameter (<c>theta</c>) by (approximately) maximizing
    ''' the Negative Binomial log-likelihood given the current fitted means <c>μ</c>.
    ''' </summary>
    ''' <param name="nb_fit">
    ''' A fitted <see cref="GLM"/> instance representing the current Negative Binomial mean-model fit
    ''' (i.e., holding the current coefficient estimates and fitted means).
    ''' The routine treats <c>μ</c> from <paramref name="nb_fit"/> as fixed while optimizing <c>theta</c>.
    ''' </param>
    ''' <param name="w">
    ''' Optional nonnegative observation weights. If omitted, all weights are treated as 1.
    ''' When provided, they are applied as multiplicative weights in the log-likelihood and its derivatives:
    ''' each observation contributes <c>wᵢ</c> times its usual contribution.
    ''' </param>
    ''' <returns>
    ''' The maximum-likelihood estimate of the NB dispersion parameter <c>theta</c> (also called “size” in some software).
    ''' </returns>
    ''' <remarks>
    ''' <h3>Model and parameterization</h3>
    ''' <para>
    ''' This routine assumes the NB2 mean/variance relationship (common in GLM software):
    ''' </para>
    ''' <para><c>E[Yᵢ|μᵢ] = μᵢ</c>, and <c>Var(Yᵢ|μᵢ) = μᵢ + μᵢ² / theta</c>.</para>
    ''' <para>
    ''' Equivalently, with <c>alpha = 1/theta</c>, <c>Var(Yᵢ|μᵢ) = μᵢ + alpha·μᵢ²</c>.
    ''' </para>
    ''' <para>
    ''' The fitted mean <c>μᵢ</c> is taken from <paramref name="nb_fit"/> and is not updated inside this function.
    ''' </para>
    '''
    ''' <h3>Log-likelihood optimized</h3>
    ''' <para>
    ''' For a single observation <c>yᵢ</c> with mean <c>μᵢ</c> and dispersion <c>theta</c>, the NB log-likelihood is:
    ''' </para>
    ''' <para>
    ''' <c>
    ''' ℓᵢ(theta) =
    ''' log Γ(yᵢ + theta) − log Γ(theta) − log(yᵢ!)
    ''' + theta·log(theta) + yᵢ·log(μᵢ)
    ''' − (yᵢ + theta)·log(theta + μᵢ).
    ''' </c>
    ''' </para>
    ''' <para>
    ''' With weights <c>wᵢ</c>, the objective is:
    ''' <c>ℓ(theta) = Σᵢ wᵢ · ℓᵢ(theta)</c>.
    ''' </para>
    '''
    ''' <h3>Score equation and Newton update (typical for glm.nb)</h3>
    ''' <para>
    ''' The derivative (score) with respect to <c>theta</c> (holding μ fixed) can be written using the digamma function
    ''' <c>ψ(·)</c>:
    ''' </para>
    ''' <para>
    ''' <c>
    ''' ∂ℓᵢ/∂theta =
    ''' ψ(yᵢ + theta) − ψ(theta)
    ''' + log(theta) + 1
    ''' − log(theta + μᵢ) − (yᵢ + theta)/(theta + μᵢ).
    ''' </c>
    ''' </para>
    ''' <para>
    ''' And the second derivative uses the trigamma function <c>ψ₁(·)</c>.
    ''' Many implementations (including MASS::glm.nb) solve <c>Σ ∂ℓᵢ/∂theta = 0</c>
    ''' by Newton–Raphson:
    ''' </para>
    ''' <para><c>theta(new) = theta(old) − score / information</c></para>
    ''' <para>
    ''' with step control to keep <c>theta &gt; 0</c>.
    ''' </para>
    ''' <para>
    ''' This function follows that same principle: it iterates updates for <c>theta</c> until convergence,
    ''' using the current <c>μ</c> from <paramref name="nb_fit"/>.
    ''' </para>
    '''
    ''' <h3>Numerical stability / constraints</h3>
    ''' <para>
    ''' The dispersion parameter must satisfy <c>theta &gt; 0</c>. The implementation enforces positivity
    ''' (e.g., via truncation, guarded updates, or step-halving) so that intermediate iterates do not
    ''' cross into invalid values.
    ''' </para>
    ''' <para>
    ''' Because the log-likelihood involves <c>log(theta)</c>, <c>log(theta + μ)</c>, and gamma-function terms,
    ''' extremely small <c>theta</c> can cause instability; similarly, very large <c>theta</c> approximates Poisson.
    ''' </para>
    '''
    ''' <h3>Relationship to the outer <see cref="GLM_NB.Fit"/> loop</h3>
    ''' <para>
    ''' <see cref="GLM_NB.Fit"/> alternates between:
    ''' </para>
    ''' <list type="bullet">
    ''' <item><description>Updating <c>β</c> by fitting an NB GLM for a fixed <c>theta</c> (mean step), and</description></item>
    ''' <item><description>Updating <c>theta</c> by maximizing <c>ℓ(theta)</c> with μ fixed (dispersion step), via this function.</description></item>
    ''' </list>
    ''' <para>
    ''' Convergence of the outer loop typically depends on the change in log-likelihood and/or <c>theta</c>.
    ''' </para>
    ''' </remarks>
    Private Function theta_ml(nb_fit As GLM, Optional w As Double() = Nothing) As Double
        ' Re-estimate theta given NB parameters; returns alpha = 1/theta (NB2 dispersion)
        Dim th As Double = 0.0, info As Double, score As Double
        Dim sumW As Double = 0.0

        ' --- initial moment estimate for theta ---
        For i = 0 To n - 1
            Dim wi As Double = If(w Is Nothing, 1.0, w(i))
            sumW += wi
            Dim r As Double = (nb_fit.y(i) / nb_fit.mu(i) - 1.0)
            th += wi * (r * r)
        Next

        If th <= 0.0 OrElse sumW <= 0.0 Then Return nb_fit.pFamily.pdAlpha ' fallback: keep current alpha

        th = sumW / th   ' theta start

        ' --- Newton iterations for theta ---
        Dim it As Integer = 0
        Dim del As Double = 1.0

        Do While (it < Me.pMaxiter And Math.Abs(del) > Me.pEps)
            th = Math.Abs(th)
            info = 0.0
            score = 0.0

            For i = 0 To n - 1
                Dim wi As Double = If(w Is Nothing, 1.0, w(i))

                info += wi * (-trigamma(th + nb_fit.y(i)) + trigamma(th) - 1.0 / th +
                          2.0 / (nb_fit.mu(i) + th) - (nb_fit.y(i) + th) / (nb_fit.mu(i) + th) ^ 2)

                score += wi * (digamma(th + nb_fit.y(i)) - digamma(th) + Math.Log(th) + 1.0 -
                           Math.Log(th + nb_fit.mu(i)) - (nb_fit.y(i) + th) / (nb_fit.mu(i) + th))
            Next

            del = score / info
            th += del
            it += 1
        Loop

        If it >= Me.pMaxiter Then
            BSlogg.Log("Theta iteration limit reached", LogMsgType.Warn)
            Me.strError += " Theta iteration limit reached"
        End If

        If th < 0.0 Then
            BSlogg.Log("Theta estimate truncated at zero", LogMsgType.Warn)
            Me.strError += " Theta estimate truncated at zero"
            Return 0.0
        End If

        Return 1.0 / th  ' alpha
    End Function

    ''' <summary>
    ''' Fits a Negative Binomial GLM by iterating between mean-parameter IRLS updates and dispersion updates.
    ''' </summary>
    ''' <param name="intercept">1 to include an intercept; 0 otherwise.</param>
    ''' <param name="bStartParams">If True, uses <see cref="GLM.startParams"/> as initial mean parameters.</param>
    ''' <param name="progressBar">Optional progress bar.</param>
    ''' <param name="progressLbl">Optional progress label.</param>
    ''' <remarks>
    ''' <para>
    ''' High-level algorithm (glm.nb style):
    ''' </para>
    ''' <list type="number">
    ''' <item><description>Fit an initial Poisson GLM to obtain starting <c>β</c> and <c>μ</c>.</description></item>
    ''' <item><description>Initialize dispersion (<c>theta</c> / <c>alpha</c>).</description></item>
    ''' <item><description>Repeat until convergence or max iterations:
    ''' <list type="bullet">
    ''' <item><description>Fit an NB GLM with current dispersion to update <c>β</c>.</description></item>
    ''' <item><description>Update dispersion by maximizing NB log-likelihood w.r.t. <c>theta</c> (implemented by <c>theta_ml</c>).</description></item>
    ''' <item><description>Check convergence using the change in log-likelihood / dispersion metric maintained by the class.</description></item>
    ''' </list>
    ''' </description></item>
    ''' </list>
    ''' <para>
    ''' The result object (<see cref="GLM.results"/>) is populated from the final NB fit.
    ''' </para>
    ''' </remarks>
    Public Overrides Sub Fit(intercept As Integer,
                                   Optional bStartParams As Boolean = False,
                                   Optional progressBar As System.Windows.Forms.ProgressBar = Nothing,
                                   Optional progressLbl As System.Windows.Forms.Label = Nothing)
        'replicating the [R] MASS package glm.nb algorithm
        Dim startTime As Double = Microsoft.VisualBasic.DateAndTime.Timer
        'Set defaults
        Me.pbConverged = False
        Me.pbIntercept = If(intercept = 1, True, False)

        'Set variable number constants
        Dim pi1 As Integer = pData.GetLength(1) 'Columns of predictor variables and responses
        Me.p = pi1 - 1 + intercept '# of variables initially in the model.
        Me.n = pData.GetLength(0)   '# of observations

        If Me.p <= 0 Then
            BSerr.LogAndThrow(New ArgumentException("Model has no parameters (no intercept and no predictors)."))
            Me.strError += " Model has no parameters (no intercept and no predictors)."
            Exit Sub
        End If

        If Me.n <= pi1 Then
            BSerr.LogAndThrow(New ArgumentException("Insufficient observations to complete analysis."))
            Me.strError += " Insufficient observations to complete analysis."
            Exit Sub
        End If

        ReDim pItInfo(p + 2, pMaxiter) 'stores params estimates, LL, Alpha at each iteration

        'Initial Estimates from Poisson model -------------------------------------
        Dim poisson_glm = New GLM(New regression.Poisson, Me.pLink)
        With poisson_glm
            .data(Me.pData,
                  Me.pRowNums,
                  If(pbOffset, Me.pOffset, Nothing),
                  If(pbWeigts, Me.pWeights, Nothing))
            .setVarNames(Me.pVarNames)
            .settingInputs(pAlpha, pMaxiter, pEps)
            .startParams = Me.startParams
            .Fit(intercept, bStartParams)
        End With
        Dim PoissonParams = poisson_glm.results.Coeffs_est

        'Fit negative binomial with fixed dispersion parameter
        Dim th As Double = theta_ml(poisson_glm, If(pbWeigts, Me.pWeights, Nothing)) 'initial estimate (weighted)
        Dim d1 As Double = Math.Sqrt(2.0 * Math.Max(1, poisson_glm.DFresid))
        Dim d2 As Double = 1.0
        Dim del As Double = 1.0
        Dim lm As Double = poisson_glm.LogLikelihood
        Dim lm0 As Double = lm + 2.0 * d1
        pLastIterDispersionChange = 1.0
        pIRLSiterations = 0
        pNBglm = New GLM(New regression.NegativeBinomial, Me.pLink)

        Do While (pIRLSiterations < pMaxiter And pLastIterDispersionChange > pEps)
            With pNBglm
                .data(Me.pData,
                      Me.pRowNums,
                      If(pbOffset, Me.pOffset, Nothing),
                      If(pbWeigts, Me.pWeights, Nothing))
                .setVarNames(Me.pVarNames)
                .settingInputs(Me.pAlpha, Me.pMaxiter, Me.pEps)
                .pFamily.pdAlpha = th  'dispersion parameter initial value
                .startParams = PoissonParams
                .Fit(intercept, True)
            End With

            If pNBglm.strError <> String.Empty Then
                BSerr.LogAndThrow(New ArgumentException("Error in inner loop while re-estimating Negative binomial fit with new dispension parameter. " & pNBglm.strError))
                Exit Do
            End If

            Dim t0 As Double = th
            th = theta_ml(pNBglm, If(pbWeigts, Me.pWeights, Nothing))
            del = t0 - th
            lm0 = lm
            lm = pNBglm.LogLikelihood
            pLastIterDispersionChange = Math.Abs(lm0 - lm) / d1 + Math.Abs(del) / d2
            If progressBar IsNot Nothing Then
                progressBar.Invoke(Sub()
                                       progressBar.Value = 100 * (Me.pIRLSiterations + 1) / Me.pMaxiter
                                       If progressLbl IsNot Nothing Then progressLbl.Text = $"Elapsed Time: {Math.Round((Microsoft.VisualBasic.DateAndTime.Timer - startTime), 2)}[s]   Iterations: {Me.pIRLSiterations + 1}   Relative Deviance + Dispersion Change = {pLastIterDispersionChange}"
                                   End Sub)
                System.Windows.Forms.Application.DoEvents()
            End If

            'save iteration info
            For i As Integer = 0 To p - 1
                pItInfo(i, pIRLSiterations) = pNBglm.results.Coeffs_est(i)
            Next
            pItInfo(p, pIRLSiterations) = th
            pItInfo(p + 1, pIRLSiterations) = pNBglm.LogLikelihood
            pItInfo(p + 2, pIRLSiterations) = pLastIterDispersionChange

            pIRLSiterations += 1
        Loop

        If pIRLSiterations > pMaxiter Then
            pbConverged = False
            BSlogg.Log("Algorithm is diverging.", LogMsgType.Warn)
        Else
            pbConverged = True
            If pIRLSiterations > 0 Then ReDim Preserve pItInfo(UBound(pItInfo, 1), pIRLSiterations - 1) Else ReDim Preserve pItInfo(UBound(pItInfo, 1), 0)
        End If

        Me.CompTime = Microsoft.VisualBasic.DateAndTime.Timer - startTime
        If progressBar IsNot Nothing Then progressBar.Invoke(Sub()
                                                                 progressBar.Value = 100
                                                             End Sub)
        If progressLbl IsNot Nothing Then progressLbl.Text = $"Elapsed Time: {Format$((Timer - startTime), "#####0.00")} [s] Finalizing ..."

        'Fit model coefficient estimates, standard errors, pZ and Chi2
        'statistics, and upper and lower confidence intervals for parameters
        Me.results = pNBglm.results
        Me.pNullDeviance = pNBglm.pNullDeviance
        Me.pFinalDeviance = pNBglm.pFinalDeviance
        Me.pLin_pred = pNBglm.pLin_pred
        Me.mu = pNBglm.mu
        Me.pFinalWeights = Me.pNBglm.pFinalWeights
        If Me.bReturnCov Then
            Me.pVarCovar = Me.pNBglm.computeVarCovar()
            Me.pbVarCovarComputed = True
        End If

        'If bComputeResiduals Then pNBglm.Residuals() 'Compute residuals if requested

        Me.results.ModelTableLabels = {"Family", "Link Function", "Null deviance", "Residual deviance", "Log Likelihood",
                "# observations", "Deviance G² (likelihood ratio) chisq", "Deviance goodness of fit chisq",
                "Pearson goodness of fit chisq", "Pseudo(McFadden) R²", "AIC", "AICc", "BIC", "Scale", "Dispersion", "Variance function V(u)=",
                "Number of Iterations", "Last Relative Deviance + Dispersion Change", "Converged?"}
        Me.results.ModelTableVals = {{pNBglm.pFamily.ToString(), "", ""},
                                     {pNBglm.pLink.ToString(), "", ""},
                                     {pNBglm.pNullDeviance, "", ""},
                                     {pNBglm.pFinalDeviance, "", ""},
                                     {pNBglm.LogLikelihood(), "", ""},
                                     {Me.n, "", ""},
                                     {pNBglm.DevianceG2chisq, pNBglm.DevianceG2df, pNBglm.DevianceG2pvalue},
                                     {pNBglm.DevianceGOFchisq, pNBglm.DFresid, pNBglm.DevianceGOFpvalue},
                                     {pNBglm.PearsonGOFchisq, pNBglm.DFresid, pNBglm.PearsonGOFpvalue},
                                     {pNBglm.PseudoR2, "", ""},
                                     {Me.AIC, Me.p + 1, ""},
                                     {Me.AICc, Me.p + 1, ""},
                                     {Me.BIC, Me.p + 1, ""},
                                     {pNBglm.ScaleSECoef, "", ""},
                                     {pNBglm.DispestionParameterPhi, "", ""},
                                     {$"u+({CSng(pNBglm.pFamily.pdAlpha)})u^2", "", ""},
                                     {Me.pIRLSiterations, "", ""},
                                     {pLastIterDispersionChange, "", ""},
                                     {CStr(Me.pbConverged), "", ""}}
    End Sub
End Class


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
    ''' <remarks>
    ''' Both matrices must have the same number of rows and the same response values in column 0.
    ''' </remarks>
    Public Sub dataInputs(arPoisData(,) As Double, arLogisticData(,) As Double,
                   strPoisVarNames() As String, strLogisticVarNames() As String,
                   Optional RowNums() As Integer = Nothing)
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
        If strOffsetVar IsNot Nothing Then t.AddFootnote($"Offset Variable: {strOffsetVar}")
        If strWeightsVar IsNot Nothing Then t.AddFootnote($"Weights Variable: {strWeightsVar}")
        If Me.startParamsPois IsNot Nothing Then t.AddFootnote($"Starting values: {array2str(Me.startParamsPois)}")
        out.Add(t)

        'Logistic Coefficients, SE table
        t = Me.resultsLogistic.CoeffsZ_toPrint()
        t.AddPvalueToFormat(4)
        t.AddTitle("Logistic Model Estimates")
        If Me.pFinalZeroModel.bSeparation Then
            t.AddFootnote("Complete separation of data points. Maximum likelihood estimates may not exist.")
        ElseIf Me.pFinalZeroModel.bQuasiSeparation Then
            t.AddFootnote("Quasi-separation of the iterative algorithm. Results may be misleading.")
        End If
        If Me.startParamsLog IsNot Nothing Then t.AddFootnote($"Starting values: {array2str(Me.startParamsLog)}")
        t.AddFootnote($"Computational time: {Me.CompTime} seconds.")
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
    ''' <param name="progressBar">Optional progress bar updated during EM.</param>
    ''' <param name="progressLbl">Optional progress label updated with iteration count and log-likelihood change.</param>
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
                         Optional progressBar As System.Windows.Forms.ProgressBar = Nothing,
                         Optional progressLbl As System.Windows.Forms.Label = Nothing)
        Dim y0 As Integer, LL_old As Double, LL_new As Double
        Dim startTime As Double = Microsoft.VisualBasic.DateAndTime.Timer
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
            BSerr.LogAndThrow(New ArgumentException("ERROR: Number of records in Poisson and Logistic part of model doesn't match."))
            Exit Sub
        End If

        Dim YX_zero(n - 1, p_zero - interceptLog) As Double
        ReDim y(n - 1), pItInfo(p_zero + p_count + 1, pMaxEMIter) 'stores params estimates, LL at each iteration

        'prepare Logistic data as required in EM algorithm
        y = GetColumnFrom2Darray(Data_zero, 0)
        For i = 0 To n - 1
            'Response for initial Logisitc model. Swap Zeros and Ones
            YX_zero(i, 0) = If(y(i) = 0, 1, 0)
            If y(i) = 0 Then y0 += 1
            For j = 1 To p_zero - interceptLog
                YX_zero(i, j) = Data_zero(i, j)
            Next
        Next
        pY0count = $"{y0} ({Format$((100 * y0 / n), "##0.00")}%)"
        If y0 = 0 Then
            BESHstatGlobals.BSerr.LogAndThrow(New ArgumentException("No zero present in the data. ZIP cannot be fitted. Aborting exectution."))
            Exit Sub
        End If

        If y0 = n Then
            BESHstatGlobals.BSerr.LogAndThrow(New ArgumentException("All observations are zero. ZIP is not identifiable (no positive counts)."))
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
            wInit = IdentityVect(n - 1, 1.0)
        End If

        With model_count
            .bComputeResiduals = False
            .bIterationDetails = False
            .data(Me.Data_count,,, wInit) ' Pass prior weights for init; this keeps Xdata/mu length = n
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

        LL_new = loglikfun(model_count.Xdata, countParam, model_zero.Xdata, zeroParam)
        LL_old = 2.0 * LL_new 'multipley by 2 to always run at least 1 iteration
        pLastIterLLchange = Math.Abs(LL_new - LL_old)
        'EM iterations ----------------------------------------------------------------
        Do While pLastIterLLchange > pEps And pEMiterations <= pMaxEMIter
            LL_old = LL_new

            ' >>> SAVE OLD PARAMS BEFORE M-STEP UPDATES <<<
            Dim countOld() As Double = CType(countParam.Clone(), Double())
            Dim zeroOld() As Double = CType(zeroParam.Clone(), Double())

            'M step - Poisson -------------------------------
            With model_count
                .data(Data_count,,, probi1)
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
            Dim LL_em As Double = loglikfun(model_count.Xdata, countNew, model_zero.Xdata, zeroNew)

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

                LL_try = loglikfun(model_count.Xdata, countTry, model_zero.Xdata, zeroTry)

                If LL_try >= LL_em Then
                    accepted = True
                    Exit For
                End If

                ' backtrack toward s=1 (monotone fallback)
                s = 1.0 + 0.5 * (s - 1.0)
                If s <= sMin + 0.000001 Then Exit For
            Next

            If accepted Then
                countParam = CType(countTry.Clone(), Double())
                zeroParam = CType(zeroTry.Clone(), Double())
                LL_new = LL_try
            Else
                countParam = countNew
                zeroParam = zeroNew
                LL_new = LL_em
            End If
            BSlogg.Log($"ZIP Over-relaxation with monotone fallback Iter {pEMiterations}: accepted={accepted}, s={s:0.###}, LL_new={LL_new}")

            ' ---------- E-step computed from the ACCEPTED params ----------
            ' Compute mu and pi from X matrices + accepted params
            mui = PredictPoissonLogLink(model_count.Xdata, countParam)
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
            If progressBar IsNot Nothing Then
                progressBar.Invoke(Sub()
                                       progressBar.Value = 100 * Me.pEMiterations / Me.pMaxEMIter
                                       If progressLbl IsNot Nothing Then progressLbl.Text = $"Elapsed Time: {Math.Round((Microsoft.VisualBasic.DateAndTime.Timer - startTime), 2)}[s]   Iterations: {Me.pEMiterations + 1}   LogLikelihood change = {pLastIterLLchange}"
                                   End Sub)
                System.Windows.Forms.Application.DoEvents()
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
        Me.pPredicted_Count = MuFromEtaLogLink(Me.pLinPred_Count)  ' Poisson log link
        Me.pLinPred_Zero = LinearPredictor(model_zero.Xdata, zeroParam)
        Me.pPredicted_Zero = PredictLogisticLogitLink(model_zero.Xdata, zeroParam) ' uses stable logistic


        ReDim pPredicted(n - 1)
        For i = 0 To n - 1
            Me.pPredicted(i) = (1.0 - Me.pPredicted_Zero(i)) * Me.pPredicted_Count(i)
        Next

        'Get hessian matrix and update outputs with correct standard errors
        Me.pHessianMat = Hess()

        'update userform label
        If progressBar IsNot Nothing Then
            progressBar.Invoke(Sub()
                                   progressBar.Value = 100
                                   If progressLbl IsNot Nothing Then progressLbl.Text = $"Elapsed Time: {Math.Round((Microsoft.VisualBasic.DateAndTime.Timer - startTime), 2)}[s]   Finalizing ..."
                               End Sub)
            System.Windows.Forms.Application.DoEvents()
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
                              {CStr(Me.pConverged), ""}})
        Me.CompTime = Microsoft.VisualBasic.DateAndTime.Timer - startTime
    End Sub

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

    Private Function MuFromEtaLogLink(ByVal eta As Double()) As Double()
        Dim nLocal As Integer = eta.Length
        Dim mu(nLocal - 1) As Double
        For i As Integer = 0 To nLocal - 1
            mu(i) = Math.Exp(eta(i))
        Next
        Return mu
    End Function

    Private Function PredictPoissonLogLink(ByVal X As Double(,), ByVal beta As Double(),
                                                 Optional ByVal offset As Double() = Nothing,
                                                 Optional ByVal bOffset As Boolean = False) As Double()
        Dim n As Integer = X.GetLength(0)
        Dim p As Integer = beta.Length
        Dim mu(n - 1) As Double

        If bOffset AndAlso (offset Is Nothing OrElse offset.Length <> n) Then
            BESHStatNG.BSerr.LogAndThrow(New ArgumentException("Offset array is missing or has incorrect length."))
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

        Dim tmp2 = MatrixMult(MatrixMult(trans(xx), dd), xx)
        Return MatInv(tmp2, "CHOL")
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
    ''' <returns>The total log-likelihood of the ZIP model.</returns>
    Private Function loglikfun(Xcount(,) As Double,
                           count_params() As Double,
                           Xzero(,) As Double,
                           zero_params() As Double) As Double

        Dim loglik As Double = 0.0

        ' === Compute Poisson linear predictor and mean ===
        Dim countP(p_count - 1, 0) As Double
        For i = 0 To p_count - 1
            countP(i, 0) = count_params(i)
        Next

        Dim muMat(,) As Double = MatrixMult(Xcount, countP)
        Dim mu(n - 1) As Double
        For i = 0 To n - 1
            mu(i) = Math.Exp(muMat(i, 0))   ' safe: exp(η) only
        Next

        ' === Compute zero-inflation linear predictor and probability ===
        Dim zeroP(p_zero - 1, 0) As Double
        For i = 0 To p_zero - 1
            zeroP(i, 0) = zero_params(i)
        Next

        Dim phiMat(,) As Double = MatrixMult(Xzero, zeroP)
        Dim pi(n - 1) As Double
        For i = 0 To n - 1
            pi(i) = regression.Logit.LogisticStable(phiMat(i, 0))
        Next

        ' === Log-likelihood contributions ===
        For i = 0 To n - 1
            If y(i) = 0 Then
                loglik += LogZIPZeroTerm(pi(i), mu(i))
            Else
                loglik += LogZIPPositiveTerm(pi(i), y(i), mu(i))
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