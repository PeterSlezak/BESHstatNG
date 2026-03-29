Option Explicit On
Imports System.Math
Imports BESHStatNG.AppInfrastructure
Imports Microsoft.Office.Interop.Excel

Namespace regression


    Public Module FamilyUtils
        Public Function createFamily(type As String, Optional DispersionParam As Double = 1) As regression.Family
            Dim f As Family
            If type.ToLower = "binomial" Then
                f = New regression.Binomial
            ElseIf type.ToLower = "poisson" Then
                f = New regression.Poisson
            ElseIf type.ToLower = "negativebinomial" Then
                f = New regression.NegativeBinomial(DispersionParam)
            ElseIf type.ToLower = "gaussian" Then
                f = New regression.Gaussian
            ElseIf type.ToLower = "gamma" Then
                f = New regression.Gamma
            Else
                AppGlobals.BSerr.LogAndThrow(New ApplicationException("Unsupported family type = " & type))
                f = Nothing
            End If
            Return f
        End Function

        Public Function GetCanonicalLinkFromDisplayName(familyDisplayName As String) As String
            Select Case familyDisplayName
                Case "Binomial"
                    Return "Logit"
                Case "Poisson"
                    Return "Log"
                Case "Negative Binomial"
                    Return "Log"
                Case "Gaussian"
                    Return "Identity"
                Case "Gamma"
                    Return "Inverse"
                Case Else
                    Return String.Empty
            End Select
        End Function
    End Module

    ''' <summary>
    ''' Abstract base class defining the distributional family used in
    ''' Generalized Linear Models (GLM) and Generalized Estimating Equations (GEE).
    ''' 
    ''' A <c>Family</c> specifies:
    ''' <list type="bullet">
    '''   <item><description>The variance function V(μ)</description></item>
    '''   <item><description>The derivative of the variance function V'(μ)</description></item>
    '''   <item><description>The deviance contribution for each observation</description></item>
    '''   <item><description>The per‑observation log‑likelihood</description></item>
    '''   <item><description>Validation rules for the response variable</description></item>
    '''   <item><description>Compatibility with link functions</description></item>
    ''' </list>
    ''' 
    ''' Concrete subclasses implement the behavior for specific exponential‑family
    ''' distributions such as Binomial, Poisson, Negative Binomial, Gaussian, and Gamma.
    ''' </summary>
    Public MustInherit Class Family

        '----------------------------------------------------------------------
        ' Numerical safety helpers
        '----------------------------------------------------------------------

        ' Small positive number to avoid NaN from Log(0) or division by zero in
        ' deviance/likelihood-style calculations. (Families that require μ>0 use this.)
        Protected Const MU_EPS As Double = 0.000000000000001

        ' Clip μ to a safe positive value for families that require μ > 0.
        Protected Shared Function ClipPositiveMu(mu As Double) As Double
            If Double.IsNaN(mu) Then Return MU_EPS
            If Double.IsInfinity(mu) Then Return mu
            If mu <= MU_EPS Then Return MU_EPS
            Return mu
        End Function

        ''' <summary>
        ''' Human‑readable list of supported GLM/GEE families.
        ''' </summary>
        Public Shared FamiliesList() As String = {"Binomial", "Poisson", "Negative Binomial", "Gaussian", "Gamma"}

        ''' <summary>
        ''' Internal codes corresponding to <see cref="FamiliesList"/> entries.
        ''' Used for programmatic selection and dispatch.
        ''' </summary>
        Public Shared FamiliesCodes() As String = {"Binomial", "Poisson", "NegativeBinomial", "Gaussian", "Gamma"}

        ''' <summary>
        ''' The dispersion/shape parameter α for the Negative Binomial family.
        ''' For other families this value is ignored.
        ''' </summary>
        Public pdAlpha As Double = 1.0#

        ' -------------------------------------------------------------------------
        '  STARTING VALUES
        ' -------------------------------------------------------------------------

        ''' <summary>
        ''' Computes an initial estimate of the mean response μ for iterative GLM/GEE fitting.
        ''' 
        ''' Default implementation returns the midpoint between the observed value and
        ''' the sample mean:
        ''' <code>
        ''' μ₀ = (y + y_mean) / 2
        ''' </code>
        ''' Subclasses may override this for families requiring special initialization.
        ''' </summary>
        ''' <param name="y">Observed response value.</param>
        ''' <param name="y_mean">Mean of the response variable.</param>
        ''' <returns>An initial estimate of μ.</returns>
        Public Overridable Function startingMu(y As Double, y_mean As Double) As Double
            Return (y + y_mean) / 2.0#
        End Function

        ' -------------------------------------------------------------------------
        '  VARIANCE FUNCTION
        ' -------------------------------------------------------------------------

        ''' <summary>
        ''' Computes the variance function V(μ) for the family.
        ''' 
        ''' Examples:
        ''' <list type="bullet">
        '''   <item><description>Gaussian: V(μ) = 1</description></item>
        '''   <item><description>Poisson: V(μ) = μ</description></item>
        '''   <item><description>Binomial: V(μ) = μ(1 − μ)</description></item>
        '''   <item><description>Gamma: V(μ) = μ²</description></item>
        ''' </list>
        ''' </summary>
        ''' <param name="mu">The mean response μ.</param>
        ''' <returns>The variance V(μ).</returns>
        Public MustOverride Function Variance(mu As Double) As Double

        ''' <summary>
        ''' Computes the derivative of the variance function V'(μ).
        ''' Required for IRLS and GEE working‑correlation updates.
        ''' </summary>
        ''' <param name="mu">The mean response μ.</param>
        ''' <returns>The derivative V'(μ).</returns>
        Public MustOverride Function varianceDeriv(mu As Double) As Double

        ' -------------------------------------------------------------------------
        '  VALIDATION
        ' -------------------------------------------------------------------------

        ''' <summary>
        ''' Validates whether a response value is admissible for the family.
        ''' 
        ''' Default implementation returns <c>True</c>.  
        ''' Subclasses should override for constraints such as:
        ''' <list type="bullet">
        '''   <item><description>Binomial: y ∈ {0,1} or y ∈ [0,1] for proportions</description></item>
        '''   <item><description>Poisson: y ≥ 0 and integer</description></item>
        '''   <item><description>Gamma: y > 0</description></item>
        ''' </list>
        ''' </summary>
        ''' <param name="val">Response value to validate.</param>
        ''' <returns><c>True</c> if valid; otherwise <c>False</c>.</returns>
        Public Overridable Function validata(val As Double) As Boolean
            'check if response variable value is applicable for the Family
            validata = True
        End Function

        ' -------------------------------------------------------------------------
        '  GEE QUASI-LIKELIHOOD
        ' -------------------------------------------------------------------------

        ''' <summary>
        ''' Computes the quasi‑likelihood contribution for a single observation,
        ''' used in GEE estimation where only the mean‑variance relationship is required.
        ''' </summary>
        ''' <param name="y">Observed response.</param>
        ''' <param name="mu">Mean response μ.</param>
        ''' <returns>The quasi‑likelihood contribution.</returns>
        Public MustOverride Function geeQuasiLike(y As Double, mu As Double) As Double

        ' -------------------------------------------------------------------------
        '  LINK COMPATIBILITY
        ' -------------------------------------------------------------------------

        ''' <summary>
        ''' Tests whether a given link function is valid for this family.
        ''' 
        ''' Examples:
        ''' <list type="bullet">
        '''   <item><description>Binomial: logit, probit, cloglog</description></item>
        '''   <item><description>Poisson: log, identity</description></item>
        '''   <item><description>Gamma: inverse, log</description></item>
        ''' </list>
        ''' </summary>
        ''' <param name="strLink">Name of the link function.</param>
        ''' <returns><c>True</c> if the link is supported.</returns>
        Public MustOverride Function testLink(strLink As String) As Boolean

        ' -------------------------------------------------------------------------
        '  PER-OBSERVATION LOG-LIKELIHOOD & DEVIANCE
        ' -------------------------------------------------------------------------

        ''' <summary>
        ''' Computes the per‑observation log‑likelihood contribution for the family.
        ''' Used in GLM maximum‑likelihood estimation.
        ''' </summary>
        ''' <param name="y">Observed response.</param>
        ''' <param name="mu">Mean response μ.</param>
        ''' <param name="scaleCoef">Scale/dispersion parameter.</param>
        ''' <returns>The log‑likelihood contribution.</returns>
        MustOverride Function loglike_obs(y As Double, mu As Double, scaleCoef As Double) As Double

        ''' <summary>
        ''' Computes the deviance contribution Dᵢ for a single observation.
        ''' 
        ''' The deviance is defined as:
        ''' <code>
        ''' D = Σ Dᵢ
        ''' </code>
        ''' where Dᵢ depends on the exponential‑family form of the distribution.
        ''' </summary>
        ''' <param name="y">Observed response.</param>
        ''' <param name="mu">Mean response μ.</param>
        ''' <returns>The deviance contribution Dᵢ.</returns>
        MustOverride Function residDev_(y As Double, mu As Double) As Double

        ' -------------------------------------------------------------------------
        '  AGGREGATE METHODS
        ' -------------------------------------------------------------------------

        ''' <summary>
        ''' Computes the total deviance for a vector of observations.
        ''' </summary>
        ''' <param name="y">Observed responses.</param>
        ''' <param name="mu">Mean responses μ.</param>
        ''' <returns>The total deviance.</returns>
        Function Deviance(y() As Double, mu() As Double) As Double
            'The deviance function evaluated at (endog, mu, var_weights, freq_weights, scale) for the distribution.
            'Deviance is usually defined as twice the loglikelihood ratio.
            'pY: The endogenous response variable
            'mu: The inverse of the link function at the linear predicted values.
            Dim sum As Double

            For i = 0 To UBound(y)
                sum += Me.residDev_(y(i), mu(i))
            Next
            Return sum
        End Function

        ''' <summary>
        ''' Computes the deviance residual for a single observation:
        ''' <code>
        ''' rᵢ = sign(yᵢ − μᵢ) * sqrt(Dᵢ)
        ''' </code>
        ''' where Dᵢ is the deviance contribution.
        ''' </summary>
        ''' <param name="y">Observed response.</param>
        ''' <param name="mu">Mean response μ.</param>
        ''' <returns>The deviance residual.</returns>
        Function residDev(y As Double, mu As Double) As Double
            'The deviance residuals are defined by the contribution D_i of  observation i to the deviance as
            ' resid\_dev_i = sign(y_i-\mu_i) \sqrt{D_i}
            ' D_i is calculated from the _residDev method in each family.
            residDev = Math.Sign(y - mu) * Math.Sqrt(residDev_(y, mu)) 'should be the same for all families.
        End Function

        ''' <summary>
        ''' Computes the total log‑likelihood for a vector of observations.
        ''' </summary>
        ''' <param name="y">Observed responses.</param>
        ''' <param name="mu">Mean responses μ.</param>
        ''' <param name="scaleCoef">Scale/dispersion parameter.</param>
        ''' <returns>The total log‑likelihood.</returns>
        Function loglike(y() As Double, mu() As Double, Optional scaleCoef As Double = 1.0#) As Double
            'The log-likelihood function in terms of the fitted mean response.
            'pY: Usually the endogenous response variable.
            'mu: Usually but not always the fitted mean response variable.
            ' Return LL defined as: ll = \sum(ll_i * freq\_weights_i)
            Dim ll_obs As Double

            For i = 0 To UBound(y)
                ll_obs += loglike_obs(y(i), mu(i), scaleCoef)
            Next
            Return ll_obs
        End Function
    End Class


    ''' <summary>
    ''' Poisson family for GLM and GEE models.
    ''' 
    ''' Models count data with mean equal to the variance:
    ''' <c>V(μ) = μ</c>.
    ''' Supports canonical log link and alternative identity/sqrt/power links.
    ''' </summary>
    Public Class Poisson
        Inherits Family

        ''' <summary>
        ''' Returns the family name.
        ''' </summary>
        Public Overrides Function tostring() As String
            Return "Poisson"
        End Function

        ''' <summary>
        ''' Variance function for the Poisson distribution:
        ''' <c>V(μ) = μ</c>.
        ''' </summary>
        Public Overrides Function Variance(mu As Double) As Double
            Return mu
        End Function

        ''' <summary>
        ''' Derivative of the variance function.
        ''' For Poisson, <c>V'(μ) = 1</c>,
        ''' note: derivative is not used in IRLS for Poisson.
        ''' </summary>
        Public Overrides Function varianceDeriv(mu As Double) As Double
            Return 1.0
        End Function


        ''' <summary>
        ''' Per‑observation log‑likelihood for the Poisson distribution:
        ''' <c>ℓ = y log μ − μ − log(y!)</c>.
        ''' </summary>
        Public Overrides Function loglike_obs(y As Double, mu As Double, scaleCoef As Double) As Double
            If scaleCoef <= 0.0 Then Return Double.NegativeInfinity
            If y < 0.0 Then Return Double.NegativeInfinity

            ' Domain for Poisson mean is mu > 0.
            ' Limit cases:
            '  - y=0, mu=0 => loglik = 0
            '  - y>0, mu=0 => loglik = -Infinity
            If mu <= 0.0 Then
                If y = 0.0 Then Return 0.0
                Return Double.NegativeInfinity
            End If

            Dim ll As Double = y * Math.Log(mu) - mu - LogGamma(y + 1.0)
            Return ll / scaleCoef
        End Function


        ''' <summary>
        ''' Deviance contribution for a single observation:
        ''' <c>Dᵢ = 2 [ y log(y/μ) − (y − μ) ]</c>.
        ''' </summary>
        Public Overrides Function residDev_(y As Double, mu As Double) As Double
            ' Poisson deviance contribution:
            '   D_i = 2 * [ y*log(y/mu) - (y - mu) ]
            ' with the convention 0*log(0/mu)=0 and D_i=0 when y=mu=0.
            If y <= 0.0 Then
                ' y = 0: D_i = 2*mu, and should be 0 when mu=0.
                If mu <= 0.0 Then Return 0.0
                Return 2.0 * mu
            End If

            mu = ClipPositiveMu(mu)
            Return 2.0 * (y * Math.Log(y / mu) - (y - mu))
        End Function


        ''' <summary>
        ''' Tests whether a link function is valid for Poisson.
        ''' </summary>
        Public Overrides Function testLink(strLink As String) As Boolean
            Dim ret As Boolean
            ret = False
            If strLink = "Log" Or strLink = "Identity" Then 'these make sense with all Families
                ret = True
            ElseIf strLink = "Sqrt" Or strLink = "Power" Then
                ret = True
            End If
            Return ret
        End Function

        ''' <summary>
        ''' Quasi‑likelihood for GEE:
        ''' <c>Q = y log μ − μ</c>.
        ''' </summary>
        Public Overrides Function geeQuasiLike(y As Double, mu As Double) As Double
            Return y * Math.Log(mu) - mu
        End Function

        ''' <summary>
        ''' Validates Poisson response values (must be non‑negative).
        ''' </summary>
        Public Overrides Function validata(val As Double) As Boolean
            validata = True
            If val < 0.0 Then validata = False
        End Function
    End Class


    ''' <summary>
    ''' Binomial family for GLM and GEE models.
    ''' 
    ''' Models binary or proportion data with variance:
    ''' <c>V(μ) = μ(1 − μ)</c>.
    ''' Supports logit, probit, cloglog, and other monotone links.
    ''' </summary>
    Public Class Binomial
        Inherits Family

        Private Const eps As Double = 0.000000000001

        ''' <summary>
        ''' Returns the family name.
        ''' </summary>
        Public Overrides Function tostring() As String
            Return "Binomial"
        End Function

        ''' <summary>
        ''' Starting value for μ using a stabilized midpoint.
        ''' </summary>
        Public Shadows Function startingMu(y As Double) As Double
            Return (y + 0.5) / 2.0
        End Function

        ''' <summary>
        ''' Variance function <c>V(μ) = μ(1 − μ)</c>.
        ''' </summary>
        Public Overrides Function Variance(mu As Double) As Double
            Return mu * (1.0 - mu)
        End Function

        ''' <summary>
        ''' Derivative <c>V'(μ) = 1 − 2μ</c>.
        ''' </summary>
        Public Overrides Function varianceDeriv(mu As Double) As Double
            Return 1.0 - 2.0 * mu
        End Function


        ''' <summary>
        ''' GEE quasi‑likelihood for binomial responses.
        ''' </summary>
        Public Overrides Function geeQuasiLike(y As Double, mu As Double) As Double
            Return y * Math.Log(mu / (1.0 - mu)) + Math.Log(1.0 - mu)
        End Function


        ''' <summary>
        ''' Tests whether a link function is valid for the binomial family.
        ''' </summary>
        Public Overrides Function testLink(strLink As String) As Boolean
            Dim ret As Boolean
            ret = False
            If strLink = "Log" Or strLink = "Identity" Then 'these make sense with all Families
                ret = True
            ElseIf strLink = "Logit" Or strLink = "Probit" Or strLink = "LogLog" Then
                ret = True
            End If
            testLink = ret
        End Function

        ''' <summary>
        ''' Per‑observation log‑likelihood:
        ''' <c>ℓ = y log μ + (1 − y) log(1 − μ)</c>.
        ''' </summary>
        Public Overrides Function loglike_obs(y As Double, mu As Double, scaleCoef As Double) As Double
            mu = clipMu(mu)
            Return y * Math.Log(mu) + (1.0 - y) * Math.Log(1.0 - mu)
        End Function

        ''' <summary>
        ''' Deviance contribution:
        ''' <c>Dᵢ = −2 [ y log μ + (1 − y) log(1 − μ) ]</c>.
        ''' </summary>
        Public Overrides Function residDev_(y As Double, mu As Double) As Double
            mu = clipMu(mu)
            Return -2.0 * (y * Math.Log(mu) + (1.0 - y) * Math.Log(1.0 - mu))
        End Function

        ''' <summary>
        ''' Validates binomial responses (must lie in [0,1]).
        ''' </summary>
        Public Overrides Function validata(val As Double) As Boolean
            validata = True
            If val < 0.0 Or val > 1.0 Then validata = False
        End Function

        Private Function clipMu(mu As Double) As Double
            If mu < eps Then
                mu = eps
            ElseIf mu > 1.0 - eps Then
                mu = 1.0 - eps
            End If
            Return mu
        End Function
    End Class


    ''' <summary>
    ''' Gamma family for GLM and GEE models.
    ''' 
    ''' Models positive continuous data with variance:
    ''' <c>V(μ) = μ²</c>.
    ''' Supports log, inverse, sqrt, and power links.
    ''' </summary>
    Public Class Gamma
        Inherits Family

        ''' <summary>
        ''' Returns the family name.
        ''' </summary>
        Public Overrides Function tostring() As String
            Return "Gamma"
        End Function

        ''' <summary>
        ''' Variance function <c>V(μ) = μ²</c>.
        ''' </summary>
        Public Overrides Function Variance(mu As Double) As Double
            Return Math.Abs(mu) ^ 2
        End Function

        ''' <summary>
        ''' Derivative <c>V'(μ) = 2μ</c>.
        ''' </summary>
        Public Overrides Function varianceDeriv(mu As Double) As Double
            Return 2.0 * Math.Abs(mu)
        End Function

        ''' <summary>
        ''' GEE quasi‑likelihood for Gamma responses.
        ''' </summary>
        Public Overrides Function geeQuasiLike(y As Double, mu As Double) As Double
            Return -(y / mu + Math.Log(mu))
        End Function


        ''' <summary>
        ''' Tests whether a link function is valid for Gamma.
        ''' </summary>
        Public Overrides Function testLink(strLink As String) As Boolean
            Dim ret As Boolean
            ret = False
            If strLink = "Log" Or strLink = "Identity" Then 'these make sense with all Families
                ret = True
            ElseIf strLink = "Inverse" Or strLink = "Sqrt" Or strLink = "Power" Then
                ret = True
            End If
            Return ret
        End Function

        ''' <summary>
        ''' Per‑observation log‑likelihood for Gamma.
        ''' </summary>
        Public Overrides Function loglike_obs(y As Double, mu As Double, scaleCoef As Double) As Double
            Dim ll_obs As Double
            If mu <> 0.0 Then ll_obs = 1.0 / scaleCoef * Math.Log(1.0 / scaleCoef * y / mu) - (1.0 / scaleCoef * y / mu)
            If y <> 0.0 Then ll_obs = ll_obs - LogGamma(1.0 / scaleCoef) - Math.Log(y)
            Return ll_obs
        End Function

        ''' <summary>
        ''' Deviance contribution:
        ''' <c>Dᵢ = 2 [ −log(y/μ) + (y − μ)/μ ]</c>.
        ''' </summary>
        Public Overrides Function residDev_(y As Double, mu As Double) As Double
            ' Gamma deviance contribution:
            '   D_i = 2 * [ -log(y/mu) + (y - mu)/mu ]
            ' Domain: y>0, mu>0. We return +Infinity for invalid y (except y=mu=0 -> 0).
            If y <= 0.0 Then
                If y = 0.0 AndAlso mu <= 0.0 Then Return 0.0
                Return Double.PositiveInfinity
            End If

            mu = ClipPositiveMu(mu)
            Return 2.0 * (-Math.Log(y / mu) + (y - mu) / mu)
        End Function


        ''' <summary>
        ''' Validates Gamma responses (must be positive).
        ''' </summary>
        Public Overrides Function validata(val As Double) As Boolean
            validata = True
            If val < 0.0 Then validata = False
        End Function
    End Class


    ''' <summary>
    ''' Gaussian (normal) family for GLM and GEE models.
    ''' 
    ''' Models continuous data with constant variance:
    ''' <c>V(μ) = 1</c>.
    ''' Supports identity, log, inverse, sqrt, and power links.
    ''' </summary>
    Public Class Gaussian
        Inherits Family

        ''' <summary>
        ''' Returns the family name.
        ''' </summary>
        Public Overrides Function tostring() As String
            Return "Gaussian"
        End Function

        ''' <summary>
        ''' Variance function <c>V(μ) = 1</c>.
        ''' </summary>
        Public Overrides Function Variance(mu As Double) As Double
            Return 1.0
        End Function

        ''' <summary>
        ''' Derivative of the variance function (zero for Gaussian).
        ''' </summary>
        Public Overrides Function varianceDeriv(mu As Double) As Double
            Return 0.0
        End Function

        ''' <summary>
        ''' GEE quasi‑likelihood for Gaussian responses.
        ''' </summary>
        Public Overrides Function geeQuasiLike(y As Double, mu As Double) As Double
            Return -0.5 * (y - mu) ^ 2
        End Function


        ''' <summary>
        ''' Tests whether a link function is valid for Gaussian.
        ''' </summary>
        Public Overrides Function testLink(strLink As String) As Boolean
            Dim ret As Boolean
            ret = False
            If strLink = "Log" Or strLink = "Identity" Then 'these make sense with all Families
                ret = True
            ElseIf strLink = "Inverse" Or strLink = "Sqrt" Or strLink = "Power" Then
                ret = True
            End If
            testLink = ret
        End Function

        ''' <summary>
        ''' Per‑observation log‑likelihood for Gaussian.
        ''' </summary>
        Public Overrides Function loglike_obs(y As Double, mu As Double, scaleCoef As Double) As Double
            'based on SPSS algorithms ver.22 page 452
            Return -(residDev_(y, mu) / scaleCoef + Math.Log(scaleCoef)) / 2.0 - Math.Log(2.0 * Math.PI) / 2.0
        End Function

        ''' <summary>
        ''' Deviance contribution <c>Dᵢ = (y − μ)²</c>.
        ''' </summary>
        Public Overrides Function residDev_(y As Double, mu As Double) As Double
            Return (y - mu) ^ 2
        End Function
    End Class


    ''' <summary>
    ''' Negative Binomial family for GLM and GEE models.
    ''' 
    ''' Models overdispersed count data with variance:
    ''' <c>V(μ) = μ + α μ²</c>.
    ''' The dispersion parameter α is stored in <see cref="pdAlpha"/>.
    ''' </summary>
    Public Class NegativeBinomial
        Inherits Family

        ''' <summary>
        ''' Returns the family name.
        ''' </summary>
        Public Overrides Function tostring() As String
            Return "Negative Binomial"
        End Function

        ''' <summary>
        ''' Initializes the family with a dispersion parameter α.
        ''' </summary>
        Sub New(Optional alpha As Double = 1.0)
            pdAlpha = alpha
        End Sub

        ''' <summary>
        ''' Variance function <c>V(μ) = μ + α μ²</c>.
        ''' </summary>
        Public Overrides Function Variance(mu As Double) As Double
            Return mu + Me.pdAlpha * (mu ^ 2)
        End Function

        ''' <summary>
        ''' Derivative <c>V'(μ) = 1 + 2α μ</c>.
        ''' </summary>
        Public Overrides Function varianceDeriv(mu As Double) As Double
            Return 1.0 + 2.0 * Me.pdAlpha * mu
        End Function

        ''' <summary>
        ''' GEE quasi‑likelihood for Negative Binomial responses.
        ''' </summary>
        Public Overrides Function geeQuasiLike(y As Double, mu As Double) As Double
            Dim tmpqq As Double
            tmpqq = LogGamma(y + 1.0 / Me.pdAlpha)
            tmpqq -= LogGamma(1.0 / Me.pdAlpha)
            tmpqq += (y * Math.Log((Me.pdAlpha * mu) / (1.0 + Me.pdAlpha * mu)))
            tmpqq += (1.0 / Me.pdAlpha * Math.Log(1.0 / (1.0 + Me.pdAlpha * mu)))
            Return tmpqq
        End Function


        ''' <summary>
        ''' Tests whether a link function is valid for Negative Binomial.
        ''' </summary>
        Public Overrides Function testLink(strLink As String) As Boolean
            Dim ret As Boolean
            ret = False
            If strLink = "Log" Or strLink = "Identity" Then 'these make sense with all Families
                ret = True
            ElseIf strLink = "Power" Then
                ret = True
            End If
            testLink = ret
        End Function

        ''' <summary>
        ''' Per‑observation log‑likelihood for Negative Binomial.
        ''' </summary>
        Public Overrides Function loglike_obs(y As Double, mu As Double, scaleCoef As Double) As Double
            ' Numerically safe per-observation log-likelihood for NB2 (alpha parameterization).
            ' Avoids NaN from 0 * Log(0) when y=0 and mu=0.

            Dim alpha As Double = Me.pdAlpha
            If alpha <= 0.0 OrElse scaleCoef <= 0.0 Then Return Double.NegativeInfinity

            ' Domain: y>=0, mu>=0
            If y < 0.0 OrElse mu < 0.0 Then Return Double.NegativeInfinity

            ' If mu=0 and y>0 then log(alpha*mu)=log(0) => -Inf (valid limiting behavior)
            If mu = 0.0 AndAlso y > 0.0 Then Return Double.NegativeInfinity

            Dim ll_obs As Double = 0.0

            ' First term: y * log(alpha*mu). For y=0, this term is exactly 0 by limit.
            If y > 0.0 Then ll_obs = y * Math.Log(alpha * mu)

            ' Second term: -(y + 1/alpha) * log(1 + alpha*mu) requires 1+alpha*mu > 0
            Dim denom As Double = 1.0 + alpha * mu
            If denom <= 0.0 OrElse Double.IsNaN(denom) Then Return Double.NegativeInfinity
            ll_obs -= (y + 1.0 / alpha) * Math.Log(denom)

            ' Remaining gamma terms (defined for y>=0, alpha>0)
            ll_obs += LogGamma(y + 1.0 / alpha)
            ll_obs -= LogGamma(1.0 / alpha)
            ll_obs -= LogGamma(y + 1.0)

            Return ll_obs / scaleCoef
        End Function


        ''' <summary>
        ''' Deviance contribution based on the NB2 formulation.
        ''' </summary>
        Public Overrides Function residDev_(y As Double, mu As Double) As Double
            'https://www.ncss.com/wp-content/themes/ncss/pdf/Procedures/NCSS/Negative_Binomial_Regression.pdf
            ' Negative Binomial deviance contribution (NB2 parameterization with alpha):
            ' Uses a numerically safe convention for y=0 to avoid 0*log(1/mu) -> NaN.
            Dim dev1 As Double, dev2 As Double

            If y <= 0.0 Then
                ' y=0 => dev1 is exactly 0 (limit of y*log(y/mu) as y->0)
                dev1 = 0.0
            Else
                mu = ClipPositiveMu(mu)
                dev1 = y * Math.Log(y / mu)
            End If

            ' Ensure mu is positive for the dev2 term as well.
            mu = ClipPositiveMu(mu)

            ' Guard against invalid alpha or log arguments; if invalid, deviance is effectively infinite.
            If Me.pdAlpha <= 0.0 Then Return Double.PositiveInfinity

            Dim denom As Double = 1.0 + Me.pdAlpha * mu
            If denom <= 0.0 OrElse Double.IsNaN(denom) Then Return Double.PositiveInfinity

            Dim numer As Double = 1.0 + Me.pdAlpha * Math.Max(y, 0.0)
            If numer <= 0.0 OrElse Double.IsNaN(numer) Then Return Double.PositiveInfinity

            dev2 = (Math.Max(y, 0.0) + 1.0 / Me.pdAlpha) * Math.Log(numer / denom)
            Return 2.0 * (dev1 - dev2)
        End Function


        ''' <summary>
        ''' Validates Negative Binomial responses (must be non‑negative).
        ''' </summary>
        Public Overrides Function validata(val As Double) As Boolean
            validata = True
            If val < 0.0 Then validata = False
        End Function
    End Class

End Namespace