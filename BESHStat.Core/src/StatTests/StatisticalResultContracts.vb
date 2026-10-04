Option Explicit On
Option Strict On
Option Infer On

Imports System
Imports System.Globalization

''' <summary>
''' Represents the collection of test statistics, p-values, and auxiliary
''' information returned by a statistical hypothesis test.
'''
''' This structure is used across multiple procedures (e.g., Mann–Whitney,
''' Wilcoxon, Grubbs, GLM/GEE diagnostics) and supports both exact and
''' asymptotic inference.
''' </summary>
Public Class TestResult

    ''' <summary>
    ''' Two‑sided p‑value based on the primary test statistic
    ''' (<see cref="TestStatistics1"/>).
    ''' For symmetric distributions, this is typically:
    ''' <code>
    ''' p = 2 * min( P(T ≤ t_obs), P(T ≥ t_obs) )
    ''' </code>
    ''' </summary>
    Public Pvalue As Double

    ''' <summary>
    ''' One‑sided lower‑tail p‑value:
    ''' <c>P(T ≤ t_obs)</c>.
    ''' </summary>
    Public PvalueLowerSide As Double

    ''' <summary>
    ''' One‑sided upper‑tail p‑value:
    ''' <c>P(T ≥ t_obs)</c>.
    ''' </summary>
    Public PvalueUpperSide As Double

    ''' <summary>
    ''' Optional second two‑sided p‑value associated with
    ''' <see cref="TestStatistics2"/> when a test produces two
    ''' related statistics (e.g., U₁ and U₂ in Mann–Whitney).
    ''' </summary>
    Public Pvalue2 As Double

    ''' <summary>
    ''' Exact two‑sided p‑value computed from the exact sampling
    ''' distribution of the statistic (when available).
    ''' Used for small samples or discrete distributions.
    ''' </summary>
    Public PvalueExact As Double

    ''' <summary>
    ''' Exact lower‑tail p‑value:
    ''' <c>P(T ≤ t_obs)</c> from the exact distribution.
    ''' </summary>
    Public pValueExactLowerSide As Double

    ''' <summary>
    ''' Exact upper‑tail p‑value:
    ''' <c>P(T ≥ t_obs)</c> from the exact distribution.
    ''' </summary>
    Public pValueExactUpperSide As Double

    ''' <summary>
    ''' Primary test statistic.
    ''' Examples:
    ''' <list type="bullet">
    '''   <item><description>Mann–Whitney U₁</description></item>
    '''   <item><description>t‑statistic</description></item>
    '''   <item><description>Z‑score</description></item>
    '''   <item><description>χ² statistic</description></item>
    ''' </list>
    ''' </summary>
    Public TestStatistics1 As Double

    ''' <summary>
    ''' Secondary test statistic when applicable.
    ''' For example, Mann–Whitney U₂ = n₁n₂ − U₁.
    ''' </summary>
    Public TestStatistics2 As Double

    ''' <summary>
    ''' Degrees of freedom associated with <see cref="TestStatistics1"/>.
    ''' </summary>
    Public DF1 As Double

    ''' <summary>
    ''' Degrees of freedom associated with <see cref="TestStatistics2"/>,
    ''' if the test produces a second statistic requiring its own df.
    ''' </summary>
    Public DF2 As Double

    ''' <summary>
    ''' Optional textual information returned by the test.
    ''' Used for procedures that require additional interpretation,
    ''' such as:
    ''' <list type="bullet">
    '''   <item><description>Grubbs outlier test (identifies which point is an outlier)</description></item>
    '''   <item><description>Normality tests (notes on ties or continuity corrections)</description></item>
    '''   <item><description>Warnings about small‑sample adjustments</description></item>
    ''' </list>
    ''' </summary>
    Public strSpecialInformation As String

    ''' <summary>
    ''' Indicates whether exact p‑values were computed and are available.
    ''' </summary>
    Public bExactAvailable As Boolean = False
End Class

Public Enum CIformat
    E_p_LL_to_UL_p = 0
    LL_to_UL = 1
    p_LL_to_UL_p = 2
End Enum

''' <summary>
''' Represents the result of a confidence interval computation, typically for
''' an effect size, parameter estimate, or distributional shift.
''' </summary>
Public Class ConfidenceIntervalResult

    Private pstrConfidenceInterval As String = String.Empty

    ''' <summary>
    ''' Point estimate of the parameter of interest (e.g., mean difference,
    ''' Hodges–Lehmann shift, effect size).
    ''' </summary>
    Public Estimate As Double = Nothing

    ''' <summary>
    ''' Upper bound of the confidence interval.
    ''' </summary>
    Public UpperLimit As Double = Nothing

    ''' <summary>
    ''' Lower bound of the confidence interval.
    ''' </summary>
    Public LowerLimit As Double = Nothing

    ''' <summary>
    ''' Standard error that is used to derive interval (used as applicable).
    ''' </summary>
    Public StdErr As Double = Nothing

    ''' <summary>
    ''' 100 * (1 - alpha) confidence interval is created.
    ''' </summary>
    Public alpha As Double = 0.05

    ''' <summary>
    ''' Preformatted textual representation of the confidence interval,
    ''' typically in the form "estimate (lower to upper)" for reporting.
    ''' </summary>
    Public Property strConfidenceInterval(Optional format As CIformat = CIformat.E_p_LL_to_UL_p) As String
        Get
            If pstrConfidenceInterval = String.Empty Then
                Dim estText As String = FormatDoubleForDisplay(Estimate)
                Dim llText As String = FormatDoubleForDisplay(LowerLimit)
                Dim ulText As String = FormatDoubleForDisplay(UpperLimit)

                If format = CIformat.E_p_LL_to_UL_p Then
                    pstrConfidenceInterval = $"{estText} ({llText} to {ulText})"
                ElseIf format = CIformat.LL_to_UL Then
                    pstrConfidenceInterval = $"{llText} to {ulText}"
                ElseIf format = CIformat.p_LL_to_UL_p Then
                    pstrConfidenceInterval = $"({llText} to {ulText})"
                End If
            End If
            Return pstrConfidenceInterval
        End Get
        Set(value As String)
            pstrConfidenceInterval = value
        End Set
    End Property

    Private Shared Function FormatDoubleForDisplay(x As Double) As String
        If Double.IsNaN(x) Then Return "#N/A"
        If Double.IsPositiveInfinity(x) Then Return "#Pinf"
        If Double.IsNegativeInfinity(x) Then Return "#Ninf"
        Return Convert.ToSingle(x).ToString(CultureInfo.CurrentCulture)
    End Function

    Public ReadOnly Property CIlabel As String
        Get
            Return $"{100.0 * (1.0 - alpha)}% Confidence Interval"
        End Get
    End Property

End Class
