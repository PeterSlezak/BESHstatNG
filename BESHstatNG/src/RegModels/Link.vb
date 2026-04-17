Option Explicit On
Option Strict On

Imports BESHStatNG.AppInfrastructure
Imports Microsoft.Office.Interop.Excel

Namespace regression


    Public Module LinkUtils
        Public Function createLink(type As String) As regression.Link
            Dim f As regression.Link
            If type.ToLower = "logit" Then
                f = New regression.Logit
            ElseIf type.ToLower = "probit" Then
                f = New regression.Probit
            ElseIf type.ToLower = "log" Then
                f = New regression.Log
            ElseIf type.ToLower = "identity" Then
                f = New regression.Identity
            ElseIf type.ToLower = "sqrt" Then
                f = New regression.Sqrt
            ElseIf type.ToLower = "inverse" Then
                f = New regression.Inverse
            Else
                AppGlobals.BSerr.LogAndThrow(New ApplicationException("Unsupported link type = " & type))
                f = Nothing
            End If
            Return f
        End Function

        Public Function createLink(type As String, pwr As Double) As regression.Link
            Dim f As regression.Link
            If type.ToLower = "power" Then
                f = New regression.Power(pwr)
            Else
                AppGlobals.BSerr.LogAndThrow(New ApplicationException("Unsupported link type = " & type))
                f = Nothing
            End If
            Return f
        End Function
    End Module

    ''' <summary>
    ''' Abstract base class for GLM link functions. A link function defines a
    ''' transformation g(μ) mapping the mean μ of a response distribution to a
    ''' linear predictor η = Xβ. Subclasses implement specific link families
    ''' (logit, probit, log, identity, power, etc.).
    ''' </summary>
    ''' <remarks>
    ''' <para><b>Mathematical Definition</b></para>
    ''' <para>
    ''' A link function g(·) satisfies:
    '''     η = g(μ)
    ''' and its inverse:
    '''     μ = g⁻¹(η)
    ''' </para>
    ''' 
    ''' <para><b>Derivatives</b></para>
    ''' <list type="bullet">
    '''   <item><description><c>deriv(p)</c> computes g′(p)</description></item>
    '''   <item><description><c>deriv2(p)</c> computes g″(p)</description></item>
    '''   <item><description><c>inverseDeriv(p)</c> computes (g⁻¹)′(p)</description></item>
    '''   <item><description><c>inverseDeriv2(p)</c> computes (g⁻¹)″(p)</description></item>
    ''' </list>
    ''' 
    ''' <para>
    ''' These derivatives are required for IRLS algorithms in GLM and GEE models.
    ''' </para>
    ''' </remarks>
    Public MustInherit Class Link
        Protected Friend Const eps As Double = 0.000000000001

        ''' <summary>
        ''' Dictionary of supported link names for general GLM families.
        ''' </summary>
        Public Shared LinkList As New Dictionary(Of Integer, String) _
    From {{0, "Logit"}, {1, "Probit"}, {2, "Log"}, {3, "Identity"}, {4, "Sqrt"}, {5, "Inverse"}, {6, "Power"}}

        ''' <summary>
        ''' Dictionary of supported link names for Poisson GLMs.
        ''' </summary>
        Public Shared PoissonLinkList As New Dictionary(Of Integer, String) From {{0, "Log"}, {1, "Identity"}, {2, "Sqrt"}}

        ''' <summary>
        ''' Dictionary of supported link names for Binomial GLMs.
        ''' </summary>
        Public Shared BinomialLinkList As New Dictionary(Of Integer, String) From {{0, "Logit"}, {1, "Probit"}, {2, "Log"}, {3, "Identity"}}

        ''' <summary>
        ''' Dictionary of supported link names for Gaussian GLMs.
        ''' </summary>
        Public Shared GaussianLinkList As New Dictionary(Of Integer, String) From {{0, "Identity"}, {1, "Log"}, {2, "Inverse"}, {3, "Power"}}

        ''' <summary>
        ''' Computes the link transformation g(p).
        ''' </summary>
        Public MustOverride Function transform(p As Double) As Double

        ''' <summary>
        ''' Computes the inverse link g⁻¹(p).
        ''' </summary>
        Public MustOverride Function inverse(p As Double) As Double

        ''' <summary>
        ''' Computes the first derivative g′(p).
        ''' </summary>
        Public MustOverride Function deriv(p As Double) As Double

        ''' <summary>
        ''' Computes the second derivative g″(p).
        ''' </summary>
        Public MustOverride Function deriv2(p As Double) As Double

        ''' <summary>
        ''' Computes the derivative of the inverse link (g⁻¹)′(p).
        ''' </summary>
        Public MustOverride Function inverseDeriv(p As Double) As Double

        ''' <summary>
        ''' Computes the second derivative of the inverse link (g⁻¹)″(p).
        ''' Default implementation uses:
        '''     (g⁻¹)″(η) = − g″(μ) / (g′(μ))³
        ''' where μ = g⁻¹(η).
        ''' </summary>
        ''' <param name="p">Linear predictor η.</param>
        Public Overridable Function inverseDeriv2(p As Double) As Double
            'Second derivative of the inverse link function g^(-1)(p). p is usually the linear predictor for a GLM or GEE model.
            'General implementation. Can be override for specific links for efficiency.
            Dim iz As Double
            iz = Me.inverse(p)
            inverseDeriv2 = -Me.deriv2(iz) / Me.deriv(iz) ^ 3
        End Function

    End Class



    ''' <summary>
    ''' Logit link function:
    '''     g(μ) = log( μ / (1 − μ) )
    ''' Commonly used for binomial GLMs and logistic regression.
    ''' </summary>
    Public Class Logit
        Inherits Link

        ''' <summary>Returns the name of the link function.</summary>
        Public Overrides Function tostring() As String
            Return "Logit"
        End Function

        ''' <summary> ''' Computes g(μ) = log( μ / (1 − μ) ), with clipping to avoid numerical overflow. ''' </summary>
        Public Overrides Function transform(p As Double) As Double
            p = clipMu(p)
            Return Math.Log(p / (1.0 - p))
        End Function

        ''' <summary>
        ''' Computes the inverse logit:
        '''     μ = 1 / (1 + exp(−η))
        ''' using a numerically stable implementation.
        ''' </summary>
        Public Overrides Function inverse(p As Double) As Double
            'inverse = 1.0# / (1.0# + Math.Exp(-p)) 'g^(-1)(pZ) = exp(p)/(1+exp(p))
            Return LogisticStable(p)
        End Function

        ''' <summary>
        ''' Computes g′(μ) = 1 / ( μ (1 − μ) ).
        ''' </summary>
        Public Overrides Function deriv(p As Double) As Double
            'clipping
            p = clipMu(p)

            Return 1.0 / (p * (1.0 - p))
        End Function

        ''' <summary>
        ''' Computes g″(μ) = (2μ − 1) / ( μ² (1 − μ)² ).
        ''' </summary>
        Public Overrides Function deriv2(p As Double) As Double
            'clipping
            p = clipMu(p)

            Dim v As Double = p * (1.0 - p)
            Return (2.0 * p - 1.0) / v ^ 2
        End Function

        ''' <summary>
        ''' Computes (g⁻¹)′(η) = μ (1 − μ), the derivative of the logistic function.
        ''' </summary>
        Public Overrides Function inverseDeriv(p As Double) As Double
            'inverseDeriv = Math.Exp(p) / (1 + Math.Exp(p)) ^ 2
            Dim mu As Double = Me.inverse(p)
            Return mu * (1.0 - mu)
        End Function

        ''' <summary>
        ''' Computes the logistic function in a numerically stable way:
        ''' 
        '''     logistic(x) = 1 / (1 + exp(-x))
        ''' 
        ''' This implementation avoids overflow for large positive x and
        ''' underflow for large negative x by branching on the sign of x.
        ''' </summary>
        ''' <param name="x">The input value.</param>
        ''' <returns>The logistic transformation of x.</returns>
        Public Shared Function LogisticStable(x As Double) As Double
            If x >= 0 Then
                Dim e As Double = Math.Exp(-x)
                Return 1.0 / (1.0 + e)
            Else
                Dim e As Double = Math.Exp(x)
                Return e / (1.0 + e)
            End If
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
    ''' Log link function:
    '''     g(μ) = log(μ)
    ''' Used in Poisson GLMs and multiplicative models.
    ''' </summary>
    Public Class Log
        Inherits Link

        Private Const MU_EPS As Double = 0.000000000000001
        Private Const EXP_MAX As Double = 700.0
        Private Const EXP_MIN As Double = -745.0

        ''' <summary>Returns "Log".</summary>
        Public Overrides Function tostring() As String
            Return "Log"
        End Function

        ''' <summary>Computes g(μ) = log(μ).</summary>
        Public Overrides Function transform(p As Double) As Double
            ' g(mu) = log(mu), domain mu>0
            If Double.IsNaN(p) Then Return Double.NaN
            If p <= MU_EPS Then p = MU_EPS
            Return Math.Log(p)
        End Function

        ''' <summary>Computes g⁻¹(η) = exp(η).</summary>
        Public Overrides Function inverse(p As Double) As Double
            ' g^-1(eta) = exp(eta), guard overflow/underflow
            If Double.IsNaN(p) Then Return Double.NaN
            If p >= EXP_MAX Then Return Math.Exp(EXP_MAX)
            If p <= EXP_MIN Then Return 0.0
            Return Math.Exp(p)
        End Function

        ''' <summary>Computes g′(μ) = 1 / μ.</summary>
        Public Overrides Function deriv(p As Double) As Double
            ' g'(mu) = 1/mu
            If Double.IsNaN(p) Then Return Double.NaN
            If p <= MU_EPS Then p = MU_EPS
            Return 1.0 / p
        End Function

        ''' <summary>Computes g″(μ) = −1 / μ².</summary>
        Public Overrides Function deriv2(p As Double) As Double
            ' g''(mu) = -1/mu^2
            If Double.IsNaN(p) Then Return Double.NaN
            If p <= MU_EPS Then p = MU_EPS
            Return -1.0 / (p * p)
        End Function

        ''' <summary>Computes (g⁻¹)′(η) = exp(η).</summary>
        Public Overrides Function inverseDeriv(p As Double) As Double
            ' (g^-1)'(eta) = exp(eta)
            Return Me.inverse(p)
        End Function
    End Class



    ''' <summary>
    ''' Power link function:
    '''     g(μ) = μᵖ
    ''' where p is a user‑specified exponent. Includes identity, inverse, and sqrt links.
    ''' </summary>
    Public Class Power
        Inherits Link

        ''' <summary>Returns "Power".</summary>
        Public Overrides Function tostring() As String
            Return "Power"
        End Function
        Public pwr As Double

        ''' <summary>
        ''' Creates a power link with exponent p.
        ''' </summary>
        Public Sub New(ByVal p As Double) 'power exponent
            Me.pwr = p
        End Sub

        ''' <summary>Computes g(μ) = μᵖ.</summary>
        Public Overrides Function transform(p As Double) As Double
            transform = p ^ Me.pwr
        End Function

        ''' <summary>Computes g⁻¹(η) = η^(1/p).</summary>
        Public Overrides Function inverse(p As Double) As Double
            inverse = p ^ (1.0 / Me.pwr)
        End Function

        ''' <summary>Computes g′(μ) = p μ^(p−1).</summary>
        Public Overrides Function deriv(p As Double) As Double
            deriv = Me.pwr * p ^ (Me.pwr - 1)
        End Function

        ''' <summary>Computes g″(μ) = p (p−1) μ^(p−2).</summary>
        Public Overrides Function deriv2(p As Double) As Double
            deriv2 = Me.pwr * (Me.pwr - 1) * p ^ (Me.pwr - 2)
        End Function

        ''' <summary>Computes (g⁻¹)′(η) = η^((1−p)/p) / p.</summary>
        Public Overrides Function inverseDeriv(p As Double) As Double
            If p < eps Then p = eps
            inverseDeriv = p ^ ((1.0 - Me.pwr) / Me.pwr) / Me.pwr
        End Function

        ''' <summary>Computes (g⁻¹)″(η) for the power link.</summary>
        Public Overrides Function inverseDeriv2(p As Double) As Double
            inverseDeriv2 = ((1.0 - Me.pwr) * p ^ ((1 - 2 * Me.pwr) / Me.pwr) / Me.pwr ^ 2)
        End Function
    End Class


    ''' <summary>
    ''' Identity link:
    '''     g(μ) = μ
    ''' Used in Gaussian GLMs.
    ''' </summary>
    Public Class Identity
        Inherits Power

        ''' <summary>Returns "Identity".</summary>
        Public Overrides Function tostring() As String
            Return "Identity"
        End Function
        Sub New()
            MyBase.New(1)
        End Sub

        ''' <summary>Identity transform g(μ) = μ.</summary>
        Public Overrides Function transform(p As Double) As Double
            transform = p
        End Function

        ''' <summary>Derivative g′(μ) = 1.</summary>
        Public Overrides Function deriv(p As Double) As Double
            deriv = 1.0
        End Function

        ''' <summary>Second derivative g″(μ) = 0.</summary>
        Public Overrides Function deriv2(p As Double) As Double
            deriv2 = 0.0
        End Function

        ''' <summary>(g⁻¹)′(η) = 1.</summary>
        Public Overrides Function inverseDeriv(p As Double) As Double
            inverseDeriv = 1.0
        End Function

        ''' <summary>(g⁻¹)″(η) = 0.</summary>
        Public Overrides Function inverseDeriv2(p As Double) As Double
            inverseDeriv2 = 0.0
        End Function
    End Class


    ''' <summary>
    ''' Inverse link:
    '''     g(μ) = 1 / μ
    ''' Equivalent to a power link with exponent −1.
    ''' </summary>
    Public Class Inverse
        Inherits Power

        ''' <summary>Returns "Inverse".</summary>
        Public Overrides Function tostring() As String
            Return "Inverse"
        End Function
        Sub New()
            MyBase.New(-1)
        End Sub
    End Class

    ''' <summary>
    ''' Square‑root link:
    '''     g(μ) = √μ
    ''' Equivalent to a power link with exponent 1/2.
    ''' </summary>
    Public Class Sqrt
        Inherits Power

        ''' <summary>Returns "Sqrt".</summary>
        Public Overrides Function tostring() As String
            Return "Sqrt"
        End Function
        Sub New()
            MyBase.New(0.5)
        End Sub
    End Class

    ''' <summary>
    ''' Probit link:
    '''     g(μ) = Φ⁻¹(μ)
    ''' where Φ is the standard normal CDF. Common in binomial GLMs.
    ''' </summary>
    Public Class Probit
        Inherits Link

        ''' <summary>Returns "Probit".</summary>
        Public Overrides Function tostring() As String
            Return "Probit"
        End Function

        ''' <summary>Computes g(μ) = Φ⁻¹(μ).</summary>
        Public Overrides Function transform(p As Double) As Double
            Return distributions.NormSInv(p)
        End Function

        ''' <summary>Computes g⁻¹(η) = Φ(η).</summary>
        Public Overrides Function inverse(p As Double) As Double
            Return distributions.PNorm(p)
        End Function

        ''' <summary>Computes g′(μ) = 1 / φ(Φ⁻¹(μ)).</summary>
        Public Overrides Function deriv(p As Double) As Double
            Return 1.0 / distributions.DNorm(distributions.NormSInv(p))
        End Function

        ''' <summary>Computes g″(μ) = v / φ(v)², where v = Φ⁻¹(μ).</summary>
        Public Overrides Function deriv2(p As Double) As Double
            Dim v As Double = distributions.NormSInv(p)
            Return v / distributions.DNorm(v) ^ 2
        End Function

        ''' <summary>Computes (g⁻¹)′(η) = φ(η).</summary>
        Public Overrides Function inverseDeriv(p As Double) As Double
            Return distributions.DNorm(p)
        End Function

        ''' <summary>Computes (g⁻¹)″(η) = −η φ(η).</summary>
        Public Overrides Function inverseDeriv2(p As Double) As Double
            inverseDeriv2 = -p * distributions.DNorm(p)
        End Function
    End Class

End Namespace