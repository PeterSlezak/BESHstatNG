Option Explicit On
Option Strict On

Imports System.Diagnostics
Imports BESHStatNG.AppInfrastructure
Imports BESHStatNG.regression

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
            AppInfrastructure.CoreServices.Log("Theta iteration limit reached", AppInfrastructure.LogMsgType.Warn)
            Me.strError += " Theta iteration limit reached"
        End If

        If th < 0.0 Then
            AppInfrastructure.CoreServices.Log("Theta estimate truncated at zero", AppInfrastructure.LogMsgType.Warn)
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
    ''' <param name="progress">Optional host-neutral progress reporter.</param>
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
                             Optional progress As IProgressReporter = Nothing)
        'replicating the [R] MASS package glm.nb algorithm
        Dim stopwatch As Stopwatch = Stopwatch.StartNew()
        'Set defaults
        Me.pbConverged = False
        Me.pbIntercept = If(intercept = 1, True, False)

        'Set variable number constants
        Dim pi1 As Integer = pData.GetLength(1) 'Columns of predictor variables and responses
        Me.p = pi1 - 1 + intercept '# of variables initially in the model.
        Me.n = pData.GetLength(0)   '# of observations

        If Me.p <= 0 Then
            CoreServices.Errors.LogAndThrow(New ArgumentException("Model has no parameters (no intercept And no predictors)."))
            Me.strError += " Model has no parameters (no intercept And no predictors)."
            Exit Sub
        End If

        If Me.n <= pi1 Then
            CoreServices.Errors.LogAndThrow(New ArgumentException("Insufficient observations to complete analysis."))
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
            CoreServices.Regression.ThrowIfCancellationRequested("Negative binomial calculation cancelled by user.")
            If pIRLSiterations > 0 AndAlso CoreServices.Regression.IsInterruptionRequested() Then
                Me.strError += " Calculation interrupted by user; latest accepted NB2 estimates returned."
                AppInfrastructure.CoreServices.Log("Negative binomial calculation interrupted by user; returning latest accepted estimates.", AppInfrastructure.LogMsgType.Warn)
                Exit Do
            End If

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
                CoreServices.Errors.LogAndThrow(New ArgumentException("Error in inner loop while re-estimating Negative binomial fit with New dispension parameter. " & pNBglm.strError))
                Exit Do
            End If

            Dim t0 As Double = th
            th = theta_ml(pNBglm, If(pbWeigts, Me.pWeights, Nothing))
            del = t0 - th
            lm0 = lm
            lm = pNBglm.LogLikelihood
            pLastIterDispersionChange = Math.Abs(lm0 - lm) / d1 + Math.Abs(del) / d2
            If progress IsNot Nothing Then
                progress.Report(Convert.ToInt32(100.0 * (Me.pIRLSiterations + 1) / Me.pMaxiter),
                                $"Elapsed Time: {Math.Round(stopwatch.Elapsed.TotalSeconds, 2)}[s]   Iterations: {Me.pIRLSiterations + 1}   Relative Deviance + Dispersion Change = {pLastIterDispersionChange}")
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
            AppInfrastructure.CoreServices.Log("Algorithm Is diverging.", AppInfrastructure.LogMsgType.Warn)
        Else
            pbConverged = True
            If pIRLSiterations > 0 Then ReDim Preserve pItInfo(pItInfo.GetLength(0) - 1, pIRLSiterations - 1) Else ReDim Preserve pItInfo(pItInfo.GetLength(0) - 1, 0)
        End If

        stopwatch.Stop()
        Me.CompTime = stopwatch.Elapsed.TotalSeconds
        If progress IsNot Nothing Then progress.Report(100, $"Elapsed Time: {stopwatch.Elapsed.TotalSeconds:#####0.00} [s] Finalizing ...")

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
                                     {$"u+({Convert.ToSingle(pNBglm.pFamily.pdAlpha)})u^2", "", ""},
                                     {Me.pIRLSiterations, "", ""},
                                     {pLastIterDispersionChange, "", ""},
                                     {Me.pbConverged.ToString(), "", ""}}
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
