Option Explicit On
Option Strict On

Imports System.Diagnostics
Imports System.Reflection
Imports BESHStatNG.AppInfrastructure
Imports BESHStatNG.regression

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

    Private pWlsRidgePenalty As Double = 0.0

    ''' <summary>
    ''' Optional ridge penalty applied to the weighted least-squares normal equations used inside IRLS.
    ''' </summary>
    ''' <remarks>
    ''' The default value of 0 preserves the original GLM behaviour. When positive, the IRLS WLS step solves
    ''' <c>(XᵀWX + Λ)β = XᵀWz</c>, where Λ is diagonal and contains this value for penalized columns.
    ''' By default the intercept is not penalized when an intercept is present. This is mainly intended for
    ''' numerically stabilizing binomial/logit fits used by propensity-score models.
    ''' </remarks>
    Public Property WlsRidgePenalty As Double
        Get
            Return pWlsRidgePenalty
        End Get
        Set(value As Double)
            If Double.IsNaN(value) OrElse Double.IsInfinity(value) OrElse value < 0.0 Then
                Throw New ArgumentOutOfRangeException(NameOf(WlsRidgePenalty), "WLS ridge penalty must be a finite non-negative value.")
            End If
            pWlsRidgePenalty = value
        End Set
    End Property

    ''' <summary>
    ''' If True, <see cref="WlsRidgePenalty"/> is not applied to the intercept column when the model includes an intercept.
    ''' </summary>
    Public Property WlsRidgeExcludeIntercept As Boolean = True


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

    Public ReadOnly Property ObservedResponses() As Double()
        Get
            Return Me.y
        End Get
    End Property

    Public ReadOnly Property ObservationWeights() As Double()
        Get
            Return Me.pWeights
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
    Public ReadOnly Property DevianceG2chisq() As Double
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
                    CoreServices.Errors.LogAndThrow(New NotImplementedException("Scale coeficient using Maximum likelihood method is not implemented yet."))
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
            Return DataManagement.ArrayUtilities.GetColumn(Me.pLin_pred, 0)
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
        CoreServices.Logger.Trace($"GLM.data start. rows={x.GetLength(0)}; cols={x.GetLength(1)}; rowNumsProvided={RowNums IsNot Nothing}; offsetProvided={Offset IsNot Nothing}; weightsProvided={Weights IsNot Nothing}")

        ' Offsets are additive in eta (as used throughout GLM.Fit).
        ' Passing an all-zero offset should behave exactly like having no offset.
        pbOffset = False
        If Offset Is Nothing Then
            pOffset = Matrix.MatrixArithmeticCore.ConstantVector(pData.GetUpperBound(0), 0.0)   ' length = n (rows)
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
            pWeights = Matrix.MatrixArithmeticCore.ConstantVector(pData.GetUpperBound(0), 1.0)     ' length = n (rows)
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

        CoreServices.Logger.Trace($"GLM.data completed. pbOffset={pbOffset}; pbWeights={pbWeigts}; n={pData.GetLength(0)}")
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
        If Me.startParams IsNot Nothing Then t.AddFootnote($"Starting values: {ArrayToString(Me.startParams)}")
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

    Private Shared Function ArrayToString(Of T)(values(,) As T) As String
        If values Is Nothing Then Return "[]"

        Dim builder As New System.Text.StringBuilder("[")
        For i As Integer = 0 To values.GetLength(0) - 1
            builder.Append("[")
            For j As Integer = 0 To values.GetLength(1) - 1
                If j > 0 Then builder.Append(", ")
                Dim boxed As Object = values(i, j)
                If boxed Is Nothing Then
                    builder.Append("<nothing>")
                Else
                    builder.Append(boxed.ToString())
                End If
            Next
            builder.Append("]")
            If i < values.GetLength(0) - 1 Then builder.Append("; ")
        Next
        builder.Append("]")
        Return builder.ToString()
    End Function

    Shared Function SafePredictorNames(names() As String) As String()
        If names Is Nothing OrElse names.Length <= 1 Then
            Return New String() {} ' no predictors; intercept only model
        End If
        Return DataManagement.ArrayUtilities.Slice(names, 1)
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
    ''' <param name="progress">Optional host-neutral progress reporter.</param>
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
                               Optional progress As IProgressReporter = Nothing)
        CoreServices.Logger.Debug($"GLM.Fit start. family={pFamily.GetType().Name}; link={pLink.GetType().Name}; intercept={intercept}; startParams={bStartParams}; maxIter={pMaxiter}; eps={pEps}; dataShape={pData.GetLength(0)}x{pData.GetLength(1)}; offset={pbOffset}; weights={pbWeigts}; wlsRidge={Me.WlsRidgePenalty}; ridgeExcludeIntercept={Me.WlsRidgeExcludeIntercept}")
        'Intercept = 1 if Yes; 0 if No
        Dim j As Integer, pi1 As Integer, dev As Double, params(,) As Double, hold As Double, y_mean As Double
        Dim ii As Integer, weights() As Double, old_params() As Double, wlsendog() As Double
        'For logistic regression only
        Dim Sep As Double, StdErr() As Double
        Const LL As Double = 0.00000001, UL As Double = 0.99999999
        ' Only these Binomial links naturally produce mu in (0,1).
        ' For Binomial+Log and Binomial+Identity we MUST NOT clamp; we must step-half back in-bounds instead.
        Dim boundedBinomialLink As Boolean = (TypeOf pFamily Is regression.Binomial) AndAlso (TypeOf pLink Is regression.Logit OrElse TypeOf pLink Is regression.Probit)

        Dim stopwatch As Stopwatch = Stopwatch.StartNew()
        Me.results = New LMresult

        'Set defaults
        pbConverged = False
        Me.pbIntercept = If(intercept = 1, True, False)

        'Set variable number constants
        pi1 = pData.GetLength(1)  'Columns of predictor variables and responses
        Me.p = pi1 - 1 + intercept '# of variables initially in the model.
        Me.n = pData.GetLength(0)     '# of observations

        If Me.p <= 0 Then
            CoreServices.Errors.LogAndThrow(New ArgumentException("Model has no parameters (no intercept and no predictors)."))
            Me.strError += " Model has no parameters (no intercept and no predictors)."
            Exit Sub
        End If

        ReDim pItInfo(Me.p + 1, pMaxiter + 1) 'stores params estimates, LL at each iteration
        'Test for insufficient observations
        If n <= pi1 Then
            AppInfrastructure.CoreServices.Log("Insufficient observations to complete analysis.", AppInfrastructure.LogMsgType.Warn)
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
            If Me.pbOffset Then pLin_pred = Matrix.MatrixArithmeticCore.Add(pLin_pred, pOffset)
        Else
            pLin_pred = Matrix.MatrixArithmeticCore.Multiply(x, startParams)
            pLin_pred = Matrix.MatrixArithmeticCore.Add(pLin_pred, pOffset)
            For i = 0 To Me.n - 1
                Me.mu(i) = pLink.inverse(pLin_pred(i, 0))
            Next
        End If


        'Do IRLS iterations
        For pIRLSiterations = 0 To pMaxiter
            CoreServices.Regression.ThrowIfCancellationRequested("GLM calculation cancelled by user.")
            If progress IsNot Nothing AndAlso pIRLSiterations > 0 AndAlso CoreServices.Regression.IsInterruptionRequested() Then
                Me.strError += " Calculation interrupted by user; latest accepted IRLS estimates returned."
                AppInfrastructure.CoreServices.Log("GLM calculation interrupted by user; returning latest accepted IRLS estimates.", AppInfrastructure.LogMsgType.Warn)
                Exit For
            End If

            AppInfrastructure.CoreServices.Log($"IRLS iteration #{pIRLSiterations}")
            For i = 0 To Me.n - 1
                weights(i) = pWeights(i) * 1.0 / (pLink.deriv(mu(i)) ^ 2 * pFamily.Variance(mu(i))) 'eim (expected information (hassian) matrix
                wlsendog(i) = pLin_pred(i, 0) + ((Me.y(i) - mu(i)) * pLink.deriv(mu(i))) - pOffset(i)
            Next

            params = FitIrlsWeightedLeastSquares(wlsendog, x, weights)

            For i = 0 To params.GetLength(0) - 1
                Me.results.Coeffs_est(i) = params(i, 0)
                StdErr(i) = params(i, 1)
            Next

            pLin_pred = Matrix.MatrixArithmeticCore.Multiply(x, Me.results.Coeffs_est)
            pLin_pred = Matrix.MatrixArithmeticCore.Add(pLin_pred, pOffset)

            If TypeOf pFamily Is regression.Binomial Then
                Sep = 0
                For i = 0 To Me.n - 1
                    Dim eta As Double = pLin_pred(i, 0)

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
                    Dim eta As Double = pLin_pred(i, 0)

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
                AppInfrastructure.CoreServices.Log("step size truncated: non-finite mu", AppInfrastructure.LogMsgType.Warn)

                Do While HasNonFinite(Me.mu)
                    If (ii > pInnerLoopMaxIter) Then Exit Do
                    ii += 1

                    For k As Integer = 0 To Me.results.Coeffs_est.Length - 1
                        Me.results.Coeffs_est(k) = (Me.results.Coeffs_est(k) + old_params(k)) / 2.0
                    Next

                    pLin_pred = Matrix.MatrixArithmeticCore.Multiply(x, Me.results.Coeffs_est)
                    pLin_pred = Matrix.MatrixArithmeticCore.Add(pLin_pred, pOffset)

                    For r As Integer = 0 To Me.n - 1
                        Dim eta2 As Double = pLin_pred(r, 0)
                        If TypeOf pLink Is regression.Inverse Then
                            If Math.Abs(eta2) < 0.000000000001 Then eta2 = If(eta2 >= 0.0, 0.000000000001, -0.000000000001)
                        End If
                        Me.mu(r) = pLink.inverse(eta2)
                    Next
                Loop

                If HasNonFinite(Me.mu) Then
                    AppInfrastructure.CoreServices.Log("IRLS - Step size truncated: non-finite mu. Cannot correct step size.", AppInfrastructure.LogMsgType.Warn)
                    Me.strError += " IRLS - Step size truncated: non-finite mu. Cannot correct step size."
                    Exit Sub
                End If
            End If

            'TODO: update the checkresponse function and do step halving when fit is outside of meaningfull range
            If CheckMu(mu) And TypeOf pFamily Is regression.Binomial Then

                If pIRLSiterations = 0 Then
                    AppInfrastructure.CoreServices.Log("IRLS algorithm. No valid set of coefficients has been found: please supply starting values.", AppInfrastructure.LogMsgType.Warn)
                    'Me.strError += " IRLS algorithm. No valid set of coefficients has been found: please supply starting values."
                Else

                    ii = 0
                    AppInfrastructure.CoreServices.Log("step size truncated: out of bounds")
                    Do While (CheckMu(mu))
                        If (ii > pInnerLoopMaxIter) Then Exit Do
                        ii += 1
                        For i = 0 To Me.results.Coeffs_est.Length - 1
                            Me.results.Coeffs_est(i) = (Me.results.Coeffs_est(i) + old_params(i)) / 2.0
                        Next
                        pLin_pred = Matrix.MatrixArithmeticCore.Multiply(x, Me.results.Coeffs_est)
                        pLin_pred = Matrix.MatrixArithmeticCore.Add(pLin_pred, pOffset)
                        For i = 0 To Me.n - 1
                            Dim eta As Double = pLin_pred(i, 0)
                            If eta < -700.0 Then eta = -700.0
                            If eta > 700.0 Then eta = 700.0

                            Me.mu(i) = pLink.inverse(eta)

                            If boundedBinomialLink Then
                                If Me.mu(i) < LL Then Me.mu(i) = LL
                                If Me.mu(i) > UL Then Me.mu(i) = UL
                            End If
                        Next
                    Loop
                    If ii > pInnerLoopMaxIter Then
                        AppInfrastructure.CoreServices.Log("IRLS - Step size truncated: out of bounds. Inner loop 2; cannot correct step size.", AppInfrastructure.LogMsgType.Warn)
                        Me.strError += " IRLS - Step size truncated: out of bounds. Inner loop 2; cannot correct step size."
                    Else
                        dev = pFamily.Deviance(y, mu)
                        AppInfrastructure.CoreServices.Log($" Step halved: new deviance ={dev} ii ={ii}")
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
            If ((dev - hold) / (0.1 + Math.Abs(dev)) >= 0.00000001) And (pIRLSiterations > 0) Then
                ii = 0

                AppInfrastructure.CoreServices.Log(" step size truncated due to increasing deviance")
                Do While ((dev - hold) / (0.1 + Math.Abs(dev))) > 0.00000001

                    If (ii > pInnerLoopMaxIter) Then Exit Do
                    ii += 1

                    For i = 0 To Me.results.Coeffs_est.Length - 1
                        Me.results.Coeffs_est(i) = (Me.results.Coeffs_est(i) + old_params(i)) / 2.0
                    Next
                    pLin_pred = Matrix.MatrixArithmeticCore.Multiply(x, Me.results.Coeffs_est)
                    pLin_pred = Matrix.MatrixArithmeticCore.Add(pLin_pred, pOffset)

                    For i = 0 To Me.n - 1
                        Dim eta As Double = pLin_pred(i, 0)
                        If eta < -700.0 Then eta = -700.0
                        If eta > 700.0 Then eta = 700.0

                        Me.mu(i) = pLink.inverse(eta)

                        If boundedBinomialLink Then
                            If Me.mu(i) < LL Then Me.mu(i) = LL
                            If Me.mu(i) > UL Then Me.mu(i) = UL
                        End If
                    Next

                    dev = pFamily.Deviance(y, mu)
                    AppInfrastructure.CoreServices.Log($"inner loop 3; ii={ii} dev={dev} hold={hold}")
                Loop
                If ii > pInnerLoopMaxIter Then
                    AppInfrastructure.CoreServices.Log("IRLS - Step size truncated due to increasing deviance. Inner loop 3; cannot correct step size.", AppInfrastructure.LogMsgType.Warn)
                    Me.strError += " IRLS - Step size truncated due to increasing deviance. Inner loop 3; cannot correct step size."
                Else
                    AppInfrastructure.CoreServices.Log($" Step halved: new deviance ={dev} ii ={ii}")
                End If
            End If

            'save iteration info
            If Me.bIterationDetails Then
                For i = 0 To Me.p
                    pItInfo(i, pIRLSiterations) = If(i = p, dev, Me.results.Coeffs_est(i))
                Next
            End If

            AppInfrastructure.CoreServices.Log($" eps={pEps} Abs(Abs(dev) - Abs(hold))={Math.Abs(Math.Abs(dev) - Math.Abs(hold))}")
            If pIRLSiterations > 0 Then

                pLastIterLLchange = Math.Abs(Math.Abs(dev) - Math.Abs(hold))
                pItInfo(Me.p + 1, pIRLSiterations) = pLastIterLLchange
                If progress IsNot Nothing Then
                    progress.Report(Convert.ToInt32(100.0 * (Me.pIRLSiterations + 1) / (Me.pMaxiter + 1)),
                                    $"Elapsed Time: {Math.Round(stopwatch.Elapsed.TotalSeconds, 2)}[s]   Iterations: {Me.pIRLSiterations + 1}   LogLikelihood change = {pLastIterLLchange}")
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
        ReDim Me.pFinalWeights(weights.Length - 1)
        For i = 0 To weights.Length - 1 : Me.pFinalWeights(i) = weights(i) : Next

        'Test for convergence or divergence and warn user
        If pIRLSiterations >= pMaxiter + 1 Then 'Too many iterations
            AppInfrastructure.CoreServices.Log("Algorithm failed To converge. Results may be misleading. Excessive iterations Of IRLS algorithm In .Fit. ", AppInfrastructure.LogMsgType.Warn)
            Me.strError += " Algorithm failed To converge. Results may be misleading. Excessive iterations Of IRLS algorithm In .Fit. "
        ElseIf Not pbConverged Then
            AppInfrastructure.CoreServices.Log("Algorithm Is diverging. Failure Of IRLS algorithm In .Fit.", AppInfrastructure.LogMsgType.Warn)
            Me.strError += " Algorithm Is diverging. Failure Of IRLS algorithm In .Fit."
        End If
        If Me.bIterationDetails Then
            If pIRLSiterations > 0 Then ReDim Preserve pItInfo(pItInfo.GetLength(0) - 1, pIRLSiterations) Else ReDim Preserve pItInfo(pItInfo.GetLength(0) - 1, 0)
        End If
        pIRLSiterations += 1

        'Fit model coefficient estimates, standard errors, pZ and Chi2
        'statistics, and upper and lower confidence intervals for parameters
        For i = 0 To Me.p - 1
            Me.results.Coeffs_SEs(i) = (StdErr(i) / Math.Sqrt(DispestionParameterPhi)) * Math.Sqrt(ScaleSECoef) ': SE
            Me.results.Coeffs_SEsT(i) = StdErr(i)                             ': SE
        Next

        AppInfrastructure.CoreServices.Log($"pCoefs{ArrayToString(Me.results.CoeffsZ_vals)} {MethodBase.GetCurrentMethod().Name}")

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
                                    {Me.pbConverged.ToString(), "", ""}}

        stopwatch.Stop()
        Me.CompTime = stopwatch.Elapsed.TotalSeconds
        CoreServices.Logger.Debug($"GLM.Fit completed. converged={Me.pbConverged}; iterations={Me.pIRLSiterations}; finalDeviance={Me.pFinalDeviance}; logLikelihood={Me.LogLikelihood()}; compTime={Me.CompTime}")
        If progress IsNot Nothing Then progress.Report(100)
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
            eta0 = Matrix.MatrixArithmeticCore.Add(eta0, Me.pOffset)
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

            Dim params0 As Double(,) = FitIrlsWeightedLeastSquares(wlsendog0, x0, weights0)
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

        If Sep / Convert.ToDouble(n) >= 0.0001 Then 'Complete separation
            AppInfrastructure.CoreServices.Log("Complete separation of data points. Maximum likelihood estimates may Not exist. Ending Computation.", AppInfrastructure.LogMsgType.Warn)
            Me.strError += " Complete separation of data points. Maximum likelihood estimates may Not exist. Ending Computation."
            QSEP = True
            Me.bSeparation = True
            Me.bQuasiSeparation = True
            Exit Function
        ElseIf Sep / Convert.ToDouble(n) >= 0.05 Then 'Quasi-separation
            AppInfrastructure.CoreServices.Log("Quasi-separation of the iterative algorithm.", AppInfrastructure.LogMsgType.Warn)
            Me.bQuasiSeparation = True
            If Not regression.GLMHostInteraction.ShouldContinueAfterQuasiSeparation() Then
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
        'compute inverse of X'VX, or inverse of the penalized information matrix when ridge is enabled
        Dim xtwx As Double(,) = BuildWeightedCrossProduct(x, V)
        ApplyWlsRidgePenalty(xtwx)
        Return Matrix.MatrixDecompositionCore.InvertMatrix(xtwx, "CHOL")
    End Function

    Private Function FitIrlsWeightedLeastSquares(endog() As Double, exog(,) As Double, weights() As Double) As Double(,)
        If Me.WlsRidgePenalty <= 0.0 Then Return Matrix.MatrixStatisticsCore.FitWeightedLeastSquares(endog, exog, weights)

        Dim xtwx As Double(,) = BuildWeightedCrossProduct(exog, weights)
        Dim xtwy() As Double = BuildWeightedCrossProductResponse(exog, endog, weights)
        ApplyWlsRidgePenalty(xtwx)

        Dim ifault As Integer = 0
        Dim chol As Double(,) = Matrix.MatrixFactorizationCore.Cholesky(xtwx, ifault, False)
        If ifault <> 0 Then
            AppInfrastructure.CoreServices.Log("Ridge WLS normal equations were Not positive definite; retrying with a larger numerical ridge.", AppInfrastructure.LogMsgType.Warn)
            For j As Integer = 0 To xtwx.GetUpperBound(0)
                xtwx(j, j) += Math.Max(Me.WlsRidgePenalty, 0.00000001) * 100.0
            Next
            ifault = 0
            chol = Matrix.MatrixFactorizationCore.Cholesky(xtwx, ifault, True)
        End If

        Dim beta() As Double = Matrix.MatrixFactorizationCore.CholeskySolve(chol, xtwy)
        Dim invInfo As Double(,) = Matrix.MatrixDecompositionCore.InvertMatrix(xtwx, "CHOL")
        Dim out(beta.Length - 1, 1) As Double
        For j As Integer = 0 To beta.Length - 1
            out(j, 0) = beta(j)
            out(j, 1) = Math.Sqrt(Math.Max(0.0, invInfo(j, j)))
        Next
        Return out
    End Function

    Private Function BuildWeightedCrossProduct(exog(,) As Double, weights() As Double) As Double(,)
        Dim nRows As Integer = exog.GetLength(0)
        Dim nCols As Integer = exog.GetLength(1)
        Dim xtwx(nCols - 1, nCols - 1) As Double

        For j As Integer = 0 To nCols - 1
            For k As Integer = j To nCols - 1
                Dim s As Double = 0.0
                For i As Integer = 0 To nRows - 1
                    Dim w As Double = If(weights(i) > 0.0, weights(i), 0.0)
                    s += exog(i, j) * w * exog(i, k)
                Next
                xtwx(j, k) = s
                xtwx(k, j) = s
            Next
        Next
        Return xtwx
    End Function

    Private Function BuildWeightedCrossProductResponse(exog(,) As Double, endog() As Double, weights() As Double) As Double()
        Dim nRows As Integer = exog.GetLength(0)
        Dim nCols As Integer = exog.GetLength(1)
        Dim xtwy(nCols - 1) As Double

        For j As Integer = 0 To nCols - 1
            Dim s As Double = 0.0
            For i As Integer = 0 To nRows - 1
                Dim w As Double = If(weights(i) > 0.0, weights(i), 0.0)
                s += exog(i, j) * w * endog(i)
            Next
            xtwy(j) = s
        Next

        Return xtwy
    End Function

    Private Sub ApplyWlsRidgePenalty(ByRef xtwx(,) As Double)
        If Me.WlsRidgePenalty <= 0.0 OrElse xtwx Is Nothing Then Return

        For j As Integer = 0 To xtwx.GetUpperBound(0)
            If ShouldApplyWlsRidgeToColumn(j) Then xtwx(j, j) += Me.WlsRidgePenalty
        Next
    End Sub

    Private Function ShouldApplyWlsRidgeToColumn(columnIndex As Integer) As Boolean
        If Me.WlsRidgePenalty <= 0.0 Then Return False
        If Me.WlsRidgeExcludeIntercept AndAlso Me.pbIntercept AndAlso columnIndex = 0 Then Return False
        Return True
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
        Dim temp_db2 = Matrix.MatrixArithmeticCore.Multiply(Matrix.MatrixArithmeticCore.Multiply(xv, VarCovar), Matrix.MatrixArithmeticCore.Transpose(xv))
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
        Dim sharedBins = regression.BinaryClassificationCore.BuildPercentileCutpointBins(mu, 10)

        Dim results = sharedBins.Select(Function(b) New With {.Bin = b.BinIndex,
                                                              .Cut = b.CutUpper,
                                                              .SuccessObs = b.Indices.Count(Function(idx) y(idx) > 0),
                                                              .FailureObs = b.Indices.Count(Function(idx) y(idx) = 0),
                                                              .SuccessExp = b.Indices.Sum(Function(idx) mu(idx)),
                                                              .FailureExp = b.Indices.Count() - b.Indices.Sum(Function(idx) mu(idx))}).ToList()

        pHosmerLemeshowTest.TestStatistics1 = results.Sum(
                    Function(r)
                        Dim s As Double = 0.0
                        If r.SuccessExp > 0 Then s += (r.SuccessObs - r.SuccessExp) ^ 2 / r.SuccessExp
                        If r.FailureExp > 0 Then s += (r.FailureObs - r.FailureExp) ^ 2 / r.FailureExp
                        Return s
                    End Function)

        ' DF and p-value
        pHosmerLemeshowTest.DF1 = Math.Max(0, results.Count - 2)
        If pHosmerLemeshowTest.DF1 > 0 Then
            pHosmerLemeshowTest.Pvalue = 1.0 - distributions.ChiSquareCDF(pHosmerLemeshowTest.TestStatistics1, Convert.ToDouble(pHosmerLemeshowTest.DF1))
        Else
            pHosmerLemeshowTest.Pvalue = Double.NaN
        End If

        'Create table For Hosmer Lemeshow test (see Hosmer Lemeshow, Applied Logistic regression table 5.1
        ReDim pHosmerLemeshowTab(results.Count - 1, 6)
        For i As Integer = 0 To results.Count - 1
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
