Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Resources.ResXFileRef
Imports System.Windows.Forms
Imports BESHStatNG.AppInfrastructure
Imports Microsoft.Office.Interop.Excel


''' <summary>
''' Container for fitting results.
''' </summary>
Public Class CoxResult
    ' Result container
    Public Property Coefficients As Double()
    Public Property VarCov As Double(,)
    Public Property VarCovRobust As Double(,)
    Public Property LogLikelihood As Double
    Public Property LogLikelihoodNull As Double
    Public Property Iterations As Integer
    Public Property Converged As Boolean
End Class

Public Enum TieMethod
    Breslow = 0
    Efron = 1
    Exact = 2
End Enum

Public Enum ResidualType
    Score
    Martingale
    Deviance
    Schoenfeld
    SchoenfeldScaled
    Dfbeta
    Dfbetas
    CoxSnell
End Enum

''' <summary>
''' Implements the Cox proportional hazards regression model, including:
'''   • Newton–Raphson fitting with step-halving
'''   • Breslow and Efron tie handling
'''   • Log partial likelihood evaluation
'''   • Score vector and Hessian computation
'''   • Model-based covariance (inverse Hessian)
'''   • Robust (sandwich) covariance estimator
'''   • Comprehensive residuals:
'''        – Score residuals
'''        – Martingale residuals
'''        – Deviance residuals
'''        – Anscombe residuals
'''        – Schoenfeld residuals
'''        – Scaled Schoenfeld residuals (as in R's cox.zph)
'''        – dfbeta and dfbetas influence diagnostics
'''        – Cumulative hazard residuals
'''   • Stratification support
'''   • Storage of detailed fitting results (loglik, variance, convergence)
'''
''' This class is designed to closely match the behavior of the widely used
''' “coxph” function in R’s survival package in:
'''   • parameter estimation,
'''   • log-likelihood values,
'''   • score/Hessian calculations,
'''   • tie-handling behavior,
'''   • variance computation,
'''   • and all residual types.
''' 
''' The class expects:
'''   • A collection of SurvivalRecord objects containing:
'''        – Time
'''        – Event indicator
'''        – Covariates
'''        – Optional stratum indicator
'''        – A unique Index for each subject
'''   • A choice of tie-handling method: Breslow (default) or Efron
''' 
''' The fitting procedure follows:
'''   1. Initialize β = 0 or provided starting values
'''   2. Iterate Newton–Raphson updates:
'''        β_{new} = β − H^{-1}(β) * Score(β)
'''   3. Apply step-halving if the log partial likelihood decreases
'''   4. Stop when:
'''        – log-likelihood change .lt. tolerance, or
'''        – parameter step .lt. tolerance, or
'''        – maximum iterations reached
''' 
''' After fitting, users may query:
'''   • Estimated coefficients β
'''   • Model-based covariance matrix V = (−H)^{-1}
'''   • Robust covariance matrix V_robust
'''   • Log partial likelihood
'''   • Number of iterations and convergence status
'''   • All available residuals for diagnostic analysis
''' 
''' This class provides the numerical foundation for:
'''   • proportional hazards tests (using scaled Schoenfeld residuals)
'''   • influence diagnostics (dfbeta / dfbetas)
'''   • goodness-of-fit evaluation through martingale/deviance residuals
''' 
''' </summary>
Public Class CoxPH
    Private pRecords As List(Of survival.SurvivalRecord)
    Private pVarNames As String()
    Private pmaxIter As Integer
    Private pEps As Double
    Private pMethod As TieMethod
    Private CompTime As Double = Nothing
    Private pIterationDetails(,) As Double

    Private pCoefficients As Double()
    Private pVarCov As Double(,)
    Private pVarCovRobust As Double(,)
    Private pLogLikelihood As Double
    Private pLogLikelihoodNull As Double
    Private pIterations As Integer
    Private pConverged As Boolean
    Private pScoreStat As TestResult

    'Residuals
    Private pScoreResiduals As Dictionary(Of Integer, Double()) = Nothing
    Private pMartingaleResiduals As Dictionary(Of Integer, Double()) = Nothing
    Private pDevianceResiduals As Dictionary(Of Integer, Double()) = Nothing
    Private pSchoenfeldResiduals As Dictionary(Of Integer, Double()) = Nothing
    Private pScaledSchoenfeldResiduals As Dictionary(Of Integer, Double()) = Nothing
    Private pDfbeta As Dictionary(Of Integer, Double()) = Nothing 'scaled score residuals (delata-betas)
    Private pScaledDfbeta As Dictionary(Of Integer, Double()) = Nothing 'scaled Dfbeta
    Private pCoxSnell As Dictionary(Of Integer, Double()) = Nothing
    Private pLikelihoodDisplacement As Dictionary(Of Integer, Double()) = Nothing

    'PH assumtpion test results
    Private pPHtestIdentity As List(Of TestResult) = Nothing
    Private pPHtestLog As List(Of TestResult) = Nothing
    Private pPHtestRank As List(Of TestResult) = Nothing

    Public bRobustVariance As Boolean = False
    Public bComputeAllResiduals As Boolean = False
    Public bReturnCov As Boolean = False
    Public bComputePHScoreTest As Boolean = False
    Public bIterationDetails As Boolean = False
    Public bTrace As Boolean = False

    ''' <summary>
    ''' Optional user-supplied starting parameter values for the Newton–Raphson optimizer.
    ''' </summary>
    ''' <remarks>
    ''' These values affect only the starting point of the optimization.
    ''' They do not change the null-model log-likelihood, the likelihood-ratio
    ''' comparison against the null model, or the score test, all of which remain
    ''' defined at β = 0.
    ''' </remarks>
    Public startParams() As Double = Nothing

    Public Sub New(x As List(Of survival.SurvivalRecord), varnames() As String, Optional maxIter As Integer = 100, Optional eps As Double = 0.00000001)
        Me.pRecords = x
        Me.pVarNames = varnames
        Me.pmaxIter = If(maxIter < 1, 100, maxIter)
        Me.pEps = If(eps < 0, 0.00000001, eps)
    End Sub

    Public Function wrapResiduals() As List(Of Object(,))
        Dim d(,) As Object, out = New List(Of Object(,))

        If Me.pScoreResiduals IsNot Nothing Then
            ReDim d(Me.pScoreResiduals.Count + 1, Me.pVarNames.Length)
            d(0, 0) = "Score Residuals"
            d(1, 0) = "Row ID"
            For j = 0 To Me.pVarNames.Length - 1 : d(1, j + 1) = Me.pVarNames(j) : Next 'variable names in the 1st row
            Dim i As Integer = 0
            For Each key In Me.pScoreResiduals.Keys
                Dim x = Me.pScoreResiduals(key)
                For j = 0 To x.Length - 1
                    If j = 0 Then d(i + 2, 0) = key
                    d(i + 2, j + 1) = x(j)
                Next
                i += 1
            Next
            out.Add(d)
        End If

        If pMartingaleResiduals IsNot Nothing Then
            'there is only one per subject
            ReDim d(Me.pMartingaleResiduals.Count + 1, 0)
            d(1, 0) = "Martingale Residuals"
            Dim i As Integer = 0
            For Each key In Me.pMartingaleResiduals.Keys
                Dim x = Me.pMartingaleResiduals(key)
                For j = 0 To x.Length - 1
                    d(i + 2, j) = x(j)
                Next
                i += 1
            Next
            out.Add(d)
        End If

        If Me.pDevianceResiduals IsNot Nothing Then
            'there is only one per subject
            ReDim d(Me.pDevianceResiduals.Count + 1, 0)
            d(1, 0) = "Deviance Residuals"
            Dim i As Integer = 0
            For Each key In Me.pDevianceResiduals.Keys
                Dim x = Me.pDevianceResiduals(key)
                For j = 0 To x.Length - 1
                    d(i + 2, j) = x(j)
                Next
                i += 1
            Next
            out.Add(d)
        End If

        If Me.pCoxSnell IsNot Nothing Then
            'there is only one per subject
            ReDim d(Me.pCoxSnell.Count + 1, 0)
            d(1, 0) = "Cox-Snell Residuals"
            Dim i As Integer = 0
            For Each key In Me.pCoxSnell.Keys
                Dim x = Me.pCoxSnell(key)
                For j = 0 To x.Length - 1
                    d(i + 2, j) = x(j)
                Next
                i += 1
            Next
            out.Add(d)
        End If

        If Me.pLikelihoodDisplacement IsNot Nothing Then
            'there is only one per subject
            ReDim d(Me.pLikelihoodDisplacement.Count + 1, 0)
            d(1, 0) = "Likelihood Displacement"
            Dim i As Integer = 0
            For Each key In Me.pLikelihoodDisplacement.Keys
                Dim x = Me.pLikelihoodDisplacement(key)
                For j = 0 To x.Length - 1
                    d(i + 2, j) = x(j)
                Next
                i += 1
            Next
            out.Add(d)
        End If

        If Me.pSchoenfeldResiduals IsNot Nothing Then
            ReDim d(Me.pSchoenfeldResiduals.Count + 1, Me.pVarNames.Length - 1)
            d(0, 0) = "Schoenfeld Residuals"
            d(1, 0) = "Row ID"
            For j = 0 To Me.pVarNames.Length - 1 : d(1, j) = Me.pVarNames(j) : Next 'variable names in the 1st row
            Dim i As Integer = 0
            For Each key In Me.pSchoenfeldResiduals.Keys
                Dim x = Me.pSchoenfeldResiduals(key)
                For j = 0 To x.Length - 1
                    d(i + 2, j) = If(Double.IsNaN(x(j)), "", x(j))
                Next
                i += 1
            Next
            out.Add(d)
        End If

        If Me.pScaledSchoenfeldResiduals IsNot Nothing Then
            ReDim d(Me.pScaledSchoenfeldResiduals.Count + 1, Me.pVarNames.Length - 1)
            d(0, 0) = "Scaled Schoenfeld Residuals"
            d(1, 0) = "Row ID"
            For j = 0 To Me.pVarNames.Length - 1 : d(1, j) = Me.pVarNames(j) : Next 'variable names in the 1st row
            Dim i As Integer = 0
            For Each key In Me.pScaledSchoenfeldResiduals.Keys
                Dim x = Me.pScaledSchoenfeldResiduals(key)
                For j = 0 To x.Length - 1
                    d(i + 2, j) = If(Double.IsNaN(x(j)), "", x(j))
                Next
                i += 1
            Next
            out.Add(d)
        End If

        If Me.pDfbeta IsNot Nothing Then
            ReDim d(Me.pDfbeta.Count + 1, Me.pVarNames.Length - 1)
            d(0, 0) = "Scaled Score Residuals (delta-betas)"
            d(1, 0) = "Row ID"
            For j = 0 To Me.pVarNames.Length - 1 : d(1, j) = Me.pVarNames(j) : Next 'variable names in the 1st row
            Dim i As Integer = 0
            For Each key In Me.pDfbeta.Keys
                Dim x = Me.pDfbeta(key)
                For j = 0 To x.Length - 1
                    d(i + 2, j) = If(Double.IsNaN(x(j)), "", x(j))
                Next
                i += 1
            Next
            out.Add(d)
        End If

        If Me.pScaledDfbeta IsNot Nothing Then
            ReDim d(Me.pDfbeta.Count + 1, Me.pVarNames.Length - 1)
            d(0, 0) = "Standardized (delta-betas) Residuals"
            d(1, 0) = "Row ID"
            For j = 0 To Me.pVarNames.Length - 1 : d(1, j) = Me.pVarNames(j) : Next 'variable names in the 1st row
            Dim i As Integer = 0
            For Each key In Me.pScaledDfbeta.Keys
                Dim x = Me.pScaledDfbeta(key)
                For j = 0 To x.Length - 1
                    d(i + 2, j) = If(Double.IsNaN(x(j)), "", x(j))
                Next
                i += 1
            Next
            out.Add(d)
        End If

        Return out
    End Function

    ''' <summary>
    ''' Builds formatted result tables for a fitted Cox proportional hazards model.
    ''' </summary>
    ''' <param name="strStrataVar">
    ''' Optional name of the strata variable, included in the output footnotes when supplied.
    ''' </param>
    ''' <param name="alpha">
    ''' Optional two-sided significance level used for hazard-ratio confidence intervals.
    ''' The default is <c>0.05</c>, corresponding to a 95% confidence interval.
    ''' </param>
    ''' <returns>
    ''' A list of <see cref="ResultTable"/> objects containing:
    ''' <list type="bullet">
    ''' <item><description>Coefficient table with beta, standard error, Wald z, p-value, hazard ratio, and confidence limits.</description></item>
    ''' <item><description>Model-fit summary table with likelihood-ratio test, score test, log-likelihoods, convergence, tie method, and computational time.</description></item>
    ''' </list>
    ''' </returns>
    Public Function wrapResults(Optional strStrataVar As String = Nothing, Optional alpha As Double = 0.05) As List(Of ResultTable)
        Dim out = New List(Of ResultTable)
        Dim zCrit As Double = distributions.ZCritTwoSided(alpha)
        Dim ciPct As String = $"{100.0 * (1.0 - alpha)}% CI"

        'coefficients, SE table
        Dim t = New ResultTable
        Dim o(UBound(Me.pVarNames, 1), 6) As Double
        For i = 0 To UBound(Me.pVarNames, 1)
            o(i, 0) = Me.pCoefficients(i)
            If Me.bRobustVariance Then
                o(i, 1) = Math.Sqrt(Me.pVarCovRobust(i, i))
            Else
                o(i, 1) = Math.Sqrt(Me.pVarCov(i, i))
            End If
            o(i, 2) = o(i, 0) / o(i, 1)
            o(i, 3) = 2.0 * distributions.PNorm(-Math.Abs(o(i, 2)))
            o(i, 4) = Math.Exp(Me.pCoefficients(i))
            o(i, 5) = Math.Exp(Me.pCoefficients(i) - zCrit * o(i, 1))
            o(i, 6) = Math.Exp(Me.pCoefficients(i) + zCrit * o(i, 1))
        Next
        t.SetBody(o)
        t.AddPvalueToFormat(4)
        t.AddHeaderTopRow({"Variable", "Coefficient", "Std. Error", "Z", "P-value", "HR", ciPct & " Lower Limit", ciPct & " Upper Limit"})
        t.AddHeaderLeftRow(Me.pVarNames)
        If bRobustVariance Then t.AddFootnote("Standard Errors are based on Lin–Wei–Ying robust sandwich variance.")
        If strStrataVar IsNot Nothing Then t.AddFootnote($"Strata Variable: {strStrataVar}")
        If Me.startParams IsNot Nothing Then t.AddFootnote($"Starting values: {Matrix.array2str(Me.startParams)}")
        t.AddFootnote($"Computational time: {Me.CompTime} seconds.")
        out.Add(t)

        'Model Info
        Dim strName As String = [Enum].GetName(GetType(TieMethod), Me.pMethod)
        t = New ResultTable
        Dim chi2 As Double, chi2p As Double
        Try
            chi2 = -2.0 * (Me.pLogLikelihoodNull - Me.pLogLikelihood)
            chi2p = 1.0 - distributions.ChiSquareCDF(chi2, Me.pVarNames.Length)
        Catch
        End Try
        t.SetBody({{chi2, $"p-value={chi2p}"},
                   {Me.pScoreStat.TestStatistics1, $"p-value={Me.pScoreStat.Pvalue}"},
                   {Me.pLogLikelihoodNull, ""},
                   {Me.pLogLikelihood, ""},
                   {Me.pIterations, ""},
                   {Me.pConverged, ""},
                   {Me.pRecords.Count, ""},
                   {Me.pRecords.Where(Function(c) c.Censorship = 1).Count, ""},
                   {strName, ""}})
        t.AddHeaderTopRow({"Model Info", ""})
        t.AddHeaderLeftRow({"Chi2(Null model - final solution)", "Chi2 Score Test", "Log likelihood with no covariates",
                            "Final Log likelihood", "Number of iterations", "Converged?", "N", "Events", "Method"})
        out.Add(t)

        'Return covariance
        If Me.bReturnCov Then
            t = New ResultTable
            If Me.bRobustVariance Then
                t.SetBody(Me.pVarCovRobust)
                t.AddFootnote("Note: Robust Convariance is presented.")
            Else
                t.SetBody(Me.pVarCov)
            End If
            Dim h(Me.pVarNames.Length - 1) As String
            h(0) = "Covariance matrix of parameters"
            t.AddHeaderTopRow(h)
            t.AddHeaderTopRow(Me.pVarNames)
            t.AddHeaderLeftRow(Me.pVarNames)
            out.Add(t)
        End If

        'PH assumption tests
        If Me.bComputePHScoreTest Then
            t = New ResultTable
            Dim body(Me.pVarNames.Length, 5) As Double
            For i = 0 To Me.pVarNames.Length 'the last one is Global test
                body(i, 0) = Me.pPHtestIdentity(i).TestStatistics1
                body(i, 1) = Me.pPHtestIdentity(i).Pvalue
                body(i, 2) = Me.pPHtestLog(i).TestStatistics1
                body(i, 3) = Me.pPHtestLog(i).Pvalue
                body(i, 4) = Me.pPHtestRank(i).TestStatistics1
                body(i, 5) = Me.pPHtestRank(i).Pvalue
            Next
            t.SetBody(body)
            t.AddHeaderTopRow({"Score Test of Proportionality Assumption", "", "", "", "", ""})
            t.AddHeaderTopRow({"Time", "", "Log(Time)", "", "Rank(Time)", ""})
            t.AddHeaderTopRow({"chi2", "p -value", "chi2", "p -value", "chi2", "p -value"})
            t.AddHeaderLeftRow(Matrix.ConcatArrays(Me.pVarNames, {"Global Test"}))
            out.Add(t)
        End If

        'iteration info
        If Me.bIterationDetails Then
            t = New ResultTable
            t.SetBody(Me.pIterationDetails)
            Dim ItLabels(Me.pIterations - 1) As String
            For i = 0 To Me.pIterations - 1 : ItLabels(i) = $"Iteration {i + 1}" : Next
            t.AddHeaderTopRow(ItLabels)
            t.AddHeaderLeftRow(Matrix.ConcatArrays(Me.pVarNames, {"LogLikelihood", "LogLikelihood Change"}))
            out.Add(t)
        End If

        Return out
    End Function

    ''' <summary>
    ''' Fits the Cox proportional hazards regression model using 
    ''' Newton–Raphson optimization of the partial log-likelihood.
    '''
    ''' <para>
    ''' This method performs the complete model-fitting procedure, including:
    ''' </para>
    ''' 
    ''' <list type="bullet">
    '''   <item><description>Initialization of coefficients.</description></item>
    '''   <item><description>Risk-set construction for each distinct event time (Time ≥ t).</description></item>
    '''   <item><description>Efficient computation of exp(η) values.</description></item>
    '''   <item><description>Construction of the score vector and Hessian matrix.</description></item>
    '''   <item><description>Newton–Raphson coefficient updates.</description></item>
    '''   <item><description>Step-halving when the log-likelihood decreases.</description></item>
    '''   <item><description>Convergence checks based on parameters and log-likelihood.</description></item>
    '''   <item><description>Recording fit diagnostics such as iterations, log-likelihood, and convergence flags.</description></item>
    ''' </list>
    '''
    ''' <h2>Tie-Handling Methods</h2>
    ''' <para>
    ''' The method supports the three standard approaches to resolving tied event times:
    ''' </para>
    ''' 
    ''' <list type="bullet">
    '''   <item>
    '''     <description>
    '''     <b>Breslow</b>: simplest approximation; exact when ties are rare.
    '''     Matches <c>coxph(..., ties="breslow")</c>.
    '''     </description>
    '''   </item>
    ''' 
    '''   <item>
    '''     <description>
    '''     <b>Efron</b>: improves accuracy for moderate tie sizes by averaging
    '''     over hypothetical ordering of tied failures.
    '''     Matches <c>coxph(..., ties="efron")</c>.
    '''     </description>
    '''   </item>
    ''' 
    '''   <item>
    '''     <description>
    '''     <b>Exact</b>: uses full or dynamic-programming exact likelihood for
    '''     tied failures, producing mathematically exact estimates.
    '''     Matches <c>coxph(..., ties="exact")</c>.
    '''     </description>
    '''   </item>
    ''' </list>
    '''
    ''' <h2>Newton–Raphson Optimization</h2>
    ''' <para>
    ''' Each iteration solves the system:
    ''' </para>
    '''
    ''' <code>
    '''   β_{new} = β − H(β)^{-1} * U(β)
    ''' </code>
    '''
    ''' <para>
    ''' where:
    ''' </para>
    ''' 
    ''' <list type="bullet">
    '''   <item><description><b>U(β)</b> is the score vector.</description></item>
    '''   <item><description><b>H(β)</b> is the observed information (negative Hessian).</description></item>
    ''' </list>
    '''
    ''' <para>
    ''' The update is repeatedly halved (step-halving) whenever it fails to 
    ''' increase the partial log-likelihood, ensuring monotone ascent.
    ''' </para>
    '''
    ''' <h2>Convergence Criteria</h2>
    ''' <para>
    ''' The algorithm stops when any of the following conditions are met:
    ''' </para>
    ''' 
    ''' <list type="bullet">
    '''   <item><description>Change in log-likelihood below tolerance.</description></item>
    '''   <item><description>Maximum absolute change in β below tolerance.</description></item>
    '''   <item><description>Maximum number of iterations reached.</description></item>
    ''' </list>
    '''
    ''' <h2>Output</h2>
    ''' <para>
    ''' After fitting, this method populates the model with:
    ''' </para>
    ''' 
    ''' <list type="bullet">
    '''   <item><description>The final coefficient estimates β.</description></item>
    '''   <item><description>The observed information matrix H(β).</description></item>
    '''   <item><description>The model-based covariance matrix (−H)^{-1}.</description></item>
    '''   <item><description>The log partial likelihood for each iteration.</description></item>
    '''   <item><description>Convergence status and number of iterations used.</description></item>
    ''' </list>
    '''
    ''' <h2>Compatibility</h2>
    ''' <para>
    ''' When the same risk-set definition (Time ≥ t), tie-handling method,
    ''' and numerical tolerances are used, this function produces estimates,
    ''' standard errors, and log-likelihood values that match R's 
    ''' <c>coxph</c> to numerical precision.
    ''' </para>
    '''
    ''' </summary>
    ''' <param name="method">
    ''' Tie-handling method: Breslow, Efron, or Exact.
    ''' </param>
    ''' <param name="progressBar">
    ''' Progress Bar control from windows form GUI
    ''' </param>
    ''' <param name="progressLbl">
    ''' Label control from windows form GUI
    ''' </param>
    ''' <returns>
    ''' Returns <c>True</c> if convergence was achieved, <c>False</c> otherwise.
    ''' Fitted model parameters and diagnostics are always stored in the class.
    ''' </returns>
    Public Function Fit(Optional method As TieMethod = TieMethod.Breslow,
                        Optional progressBar As System.Windows.Forms.ProgressBar = Nothing,
                        Optional progressLbl As System.Windows.Forms.Label = Nothing) As CoxResult
        Me.pMethod = method
        Dim startTime As Double = Microsoft.VisualBasic.DateAndTime.Timer
        If Me.pRecords.Count = 0 Then AppGlobals.BSerr.LogAndThrow(New ArgumentException("Empty data"))

        Dim p As Integer = Me.pRecords(0).Covariates.Length
        Dim beta(p - 1) As Double

        'The null-model log-likelihood must always be evaluated at β = 0,
        'regardless of any user-supplied starting values.
        Dim betaNull(p - 1) As Double
        Me.pLogLikelihoodNull = ComputeLogLikelihood(betaNull)

        'Initialize the optimizer either at the null vector or at the supplied
        'starting values. This affects optimization only, not null-model statistics.
        If Me.startParams IsNot Nothing Then
            If Me.startParams.Length <> p Then
                AppGlobals.BSerr.LogAndThrow(
                    New ArgumentException($"Starting parameter array length ({Me.startParams.Length}) does not match the number of Cox predictors ({p})."))
            End If

            Me.startParams.CopyTo(beta, 0)
            Me.pLogLikelihood = ComputeLogLikelihood(beta)

            If Double.IsNaN(Me.pLogLikelihood) OrElse Double.IsInfinity(Me.pLogLikelihood) Then
                AppGlobals.BSerr.LogAndThrow(
                    New ArgumentException("Provided starting values lead to an invalid initial Cox partial log-likelihood. Please provide a different set of starting values."))
            End If
        Else
            Me.pLogLikelihood = Me.pLogLikelihoodNull
        End If

        ' Group by stratum
        Dim strataGroups = Me.pRecords.GroupBy(Function(r) r.Stratum).ToList()
        Me.pConverged = False
        Dim info(p - 1, p - 1) As Double   ' will hold Hessian at final iteration
        ReDim pIterationDetails(p + 1, Me.pmaxIter - 1) 'parameters + LL + LL change

        For Me.pIterations = 0 To Me.pmaxIter - 1
            Dim score(p - 1) As Double
            info = New Double(p - 1, p - 1) {}   ' reset Hessian accumulator

            For Each sg In strataGroups

                Dim group = sg.OrderBy(Function(r) r.Time).ToList()

                ' Group events by time
                Dim eventsByTime = group.Where(Function(r) r.Censorship = 1).GroupBy(Function(r) r.Time)

                ' For each time with events
                For Each eventGroup In eventsByTime

                    Dim t As Double = eventGroup.Key
                    Dim events = eventGroup.OrderBy(Function(r) r.Index).ToList()
                    Dim d As Integer = events.Count

                    ' Risk set: subjects with time >= t
                    Dim riskSet = group.Where(Function(r) r.Time >= t).ToList()

                    ' Precompute exp(η) for risk set
                    Dim exbRisk(riskSet.Count - 1) As Double
                    For i = 0 To riskSet.Count - 1
                        exbRisk(i) = Math.Exp(Matrix.DotProduct(riskSet(i).Covariates, beta))
                    Next

                    ' Precompute exp(η) for tied events
                    Dim exbEvents(d - 1) As Double
                    For i = 0 To d - 1
                        exbEvents(i) = Math.Exp(Matrix.DotProduct(events(i).Covariates, beta))
                    Next

                    ' Handle ties depending on method
                    Select Case Me.pMethod
                        Case TieMethod.Breslow
                            UpdateBreslow(riskSet, events, exbRisk, exbEvents, beta, score, info)
                        Case TieMethod.Efron
                            UpdateEfron(riskSet, events, exbRisk, exbEvents, beta, score, info)
                        Case TieMethod.Exact
                            UpdateExact(riskSet, events, exbRisk, exbEvents, beta, score, info)
                    End Select

                Next
            Next

            ' Solve for delta
            Dim delta = SolveLinearSystem(info, score)

            ' Update beta, do the step halving if needed
            ' step-halving line search on log-likelihood
            Dim stepSize As Double = 1.0
            Dim betaNew(p - 1) As Double
            Dim logLikNew As Double

            Do
                For j = 0 To p - 1
                    betaNew(j) = beta(j) - stepSize * delta(j)
                Next

                logLikNew = ComputeLogLikelihood(betaNew)
                If (Not Double.IsNaN(logLikNew) AndAlso logLikNew >= Me.pLogLikelihood) OrElse stepSize < 0.00000001 Then
                    Exit Do
                Else
                    AppGlobals.BSlogg.Log($"Step halving. Current stepSize={stepSize}; logLikNew={logLikNew}; old logLike={Me.pLogLikelihood}")
                    stepSize /= 2.0
                End If
            Loop

            ' update beta and check convergence
            Dim llDiff As Double = Math.Abs(logLikNew - Me.pLogLikelihood)
            beta = CType(betaNew.Clone(), Double())
            Me.pLogLikelihood = logLikNew

            If Me.bTrace Then AppGlobals.BSlogg.Log($"betaNew = {Matrix.array2str(betaNew)}; logLikNew = {logLikNew}; llDiff = {llDiff}")

            'save iteration info
            For jj = 0 To p + 1
                If jj = p Then
                    Me.pIterationDetails(jj, pIterations) = Me.pLogLikelihood
                ElseIf jj = p + 1 Then
                    Me.pIterationDetails(jj, pIterations) = llDiff
                Else
                    Me.pIterationDetails(jj, pIterations) = beta(jj)
                End If
            Next
            If llDiff <= Me.pEps Then
                Me.pConverged = True
                Exit For
            End If

            If progressBar IsNot Nothing Then
                progressBar.Invoke(Sub()
                                       progressBar.Value = 100 * (Me.pIterations + 1) / Me.pmaxIter
                                       If progressLbl IsNot Nothing Then progressLbl.Text = $"Elapsed Time: {Math.Round((Microsoft.VisualBasic.DateAndTime.Timer - startTime), 2)}[s]   Iterations: {Me.pIterations + 1}   LogLikelihood change = {llDiff}"
                                   End Sub)
                System.Windows.Forms.Application.DoEvents()
            End If
        Next
        If Me.pConverged Then ReDim Preserve Me.pIterationDetails(p + 1, Me.pIterations)
        Me.pIterations += 1 'because it starts from zero

        ' one more evaluation of Hessian at final beta for covariance
        Dim finalScore(p - 1) As Double
        info = New Double(p - 1, p - 1) {}

        For Each sg In strataGroups
            Dim group = sg.OrderBy(Function(r) r.Time).ToList()
            Dim eventsByTime = group.Where(Function(r) r.Censorship = 1).GroupBy(Function(r) r.Time)

            For Each eventGroup In eventsByTime
                Dim t = eventGroup.Key
                Dim events = eventGroup.OrderBy(Function(r) r.Index).ToList()
                Dim d = events.Count

                Dim riskSet = group.Where(Function(r) r.Time >= t).ToList()

                Dim nRisk As Integer = riskSet.Count
                Dim exbRisk(nRisk - 1) As Double
                For i = 0 To nRisk - 1
                    exbRisk(i) = Math.Exp(Matrix.DotProduct(riskSet(i).Covariates, beta))
                Next

                Dim exbEvents(d - 1) As Double
                For i = 0 To d - 1
                    exbEvents(i) = Math.Exp(Matrix.DotProduct(events(i).Covariates, beta))
                Next

                Select Case method
                    Case TieMethod.Breslow
                        UpdateBreslow(riskSet, events, exbRisk, exbEvents, beta, finalScore, info)
                    Case TieMethod.Efron
                        UpdateEfron(riskSet, events, exbRisk, exbEvents, beta, finalScore, info)
                    Case TieMethod.Exact
                        UpdateExact(riskSet, events, exbRisk, exbEvents, beta, finalScore, info)
                End Select
            Next
        Next

        ' info currently ~ Hessian H; model-based var = (-H)^(-1)
        Me.pVarCov = InvertNegHessian(info)
        Me.pCoefficients = beta

        ' robust (sandwich) variance
        If Me.bRobustVariance Then Me.pVarCovRobust = ComputeSandwichVariance(info)

        'score test
        Try
            Me.pScoreStat = Me.ComputeScoreTest()
        Catch
            Me.pScoreStat = Nothing
        End Try

        'Residuals
        If Me.bComputeAllResiduals Then
            Me.pScoreResiduals = Me.Residuals(ResidualType.Score)
            Me.pSchoenfeldResiduals = Me.Residuals(ResidualType.Schoenfeld)
            Me.pScaledSchoenfeldResiduals = Me.Residuals(ResidualType.SchoenfeldScaled)
            Me.pMartingaleResiduals = Me.Residuals(ResidualType.Martingale)
            Me.pDevianceResiduals = Me.Residuals(ResidualType.Deviance)
            Me.pCoxSnell = Me.Residuals(ResidualType.CoxSnell)
            Me.pDfbeta = Me.Residuals(ResidualType.Dfbeta)
            Me.pScaledDfbeta = Me.Residuals(ResidualType.Dfbetas)
            Me.pLikelihoodDisplacement = Me.ComputeLikelihoodDisplacement()
        End If

        If Me.bComputePHScoreTest Then
            Me.pPHtestIdentity = Me.ComputePHScoreTest("identity")
            Me.pPHtestLog = Me.ComputePHScoreTest("log")
            Me.pPHtestRank = Me.ComputePHScoreTest()
        End If

        If progressBar IsNot Nothing Then progressBar.Invoke(Sub()
                                                                 progressBar.Value = 100
                                                             End Sub)

        Me.CompTime = Microsoft.VisualBasic.DateAndTime.Timer - startTime
        Return New CoxResult With {
                .Coefficients = Me.pCoefficients,
                .VarCov = Me.pVarCov,
                .VarCovRobust = Me.pVarCovRobust,
                .LogLikelihood = Me.pLogLikelihood,
                .LogLikelihoodNull = Me.pLogLikelihoodNull,
                .Iterations = Me.pIterations,
                .Converged = Me.pConverged
            }
    End Function

    ''' <summary>
    ''' Computes the classical Cox Score Test (also known as Logrank Test),
    ''' evaluated at the null model β = 0.
    '''
    ''' This reproduces the score test reported in R's summary(coxph()).
    ''' 
    ''' The score test statistic is:
    '''     S = U(0)^T * I(0)^{-1} * U(0)
    ''' where:
    '''   U(0) = score vector at β = 0  (first derivative of log-partial likelihood)
    '''   I(0) = observed information matrix at β = 0 (− Hessian)
    ''' 
    ''' This is a test of the global null hypothesis:
    '''     H0: all coefficients β = 0
    ''' 
    ''' It does NOT use the fitted coefficients; it uses β = 0.
    ''' That is why this test must be computed separately AFTER the model converges.
    ''' 
    ''' This function respects the tie-handling method used in the model:
    ''' Breslow, Efron, or Exact.
    '''
    ''' Returned value:
    '''     A ScoreTestResult structure containing:
    '''         Statistic       – Score test statistic (chi-square)
    '''         DF              – Degrees of freedom
    '''         PValue          – Chi-square p-value
    ''' 
    ''' </summary>
    Private Function ComputeScoreTest() As TestResult
        Dim out = New TestResult
        Dim p As Integer = Me.pCoefficients.Length

        ' β = 0 for the null model
        Dim beta0(p - 1) As Double

        ' Score and Hessian at β = 0
        Dim score0(p - 1) As Double
        Dim info0(p - 1, p - 1) As Double

        ' Group by stratum
        Dim strataGroups = Me.pRecords.GroupBy(Function(r) r.Stratum).ToList()

        ' ================================================
        ' Build U(0) and I(0) using the same update logic
        ' ================================================
        For Each sg In strataGroups

            Dim group = sg.OrderBy(Function(r) r.Time).ToList()

            ' Group events by event time only
            Dim eventsByTime = group.Where(Function(r) r.Censorship = 1).GroupBy(Function(r) r.Time)

            For Each eventGroup In eventsByTime

                Dim t As Double = eventGroup.Key
                Dim events = eventGroup.OrderBy(Function(r) r.Index).ToList()
                Dim d As Integer = events.Count

                ' Risk set: subjects with observed time >= t  (matches R)
                Dim riskSet = group.Where(Function(r) r.Time >= t).ToList()

                Dim nRisk As Integer = riskSet.Count

                ' exp(η) = 1 at β = 0, but we keep general formula
                Dim exbRisk(nRisk - 1) As Double
                For i = 0 To nRisk - 1
                    exbRisk(i) = Math.Exp(Matrix.DotProduct(riskSet(i).Covariates, beta0))
                Next

                Dim exbEvents(d - 1) As Double
                For i = 0 To d - 1
                    exbEvents(i) = Math.Exp(Matrix.DotProduct(events(i).Covariates, beta0))
                Next

                ' Apply chosen tie method
                Select Case Me.pMethod
                    Case TieMethod.Breslow
                        UpdateBreslow(riskSet, events, exbRisk, exbEvents, beta0, score0, info0)

                    Case TieMethod.Efron
                        UpdateEfron(riskSet, events, exbRisk, exbEvents, beta0, score0, info0)

                    Case TieMethod.Exact
                        UpdateExact(riskSet, events, exbRisk, exbEvents, beta0, score0, info0)
                End Select
            Next
        Next

        ' ================================
        ' Invert -I(0) → variance under null
        ' ================================
        Dim var0(,) As Double = InvertNegHessian(info0)

        ' ===========================
        ' Score statistic: U^T V U
        ' ===========================
        Dim S As Double = 0.0
        For i = 0 To p - 1
            For j = 0 To p - 1
                S += score0(i) * var0(i, j) * score0(j)
            Next
        Next

        out.TestStatistics1 = S
        ' Degrees of freedom = number of coefficients
        out.DF1 = p
        ' Chi-square p-value
        out.Pvalue = 1.0 - distributions.ChiSquareCDF(out.TestStatistics1, out.DF1)

        Return out
    End Function

    ' ---------------------------
    ' Breslow tie handling
    ' ---------------------------
    ''' <summary>
    ''' Updates the score vector and observed information (Hessian) matrix
    ''' for the Cox proportional hazards model using the
    ''' <b>Breslow approximation</b> for tied event times.
    '''
    ''' <para>
    ''' The Breslow method is the simplest tie-handling approach. It assumes
    ''' that all tied events occur in an infinitesimally small interval and
    ''' share the same risk set without further adjustment.
    ''' </para>
    '''
    ''' <para>
    ''' For a tied block of d events at time t, the Breslow expected value is:
    ''' </para>
    '''
    ''' <code>
    '''     E[X]  = Σ_{i∈R(t)} exp(η_i) X_i  /  Σ_{i∈R(t)} exp(η_i)
    ''' </code>
    '''
    ''' <code>
    '''     E[XX] = Σ exp(η_i) X_i X_i^T / Σ exp(η_i)
    ''' </code>
    '''
    ''' <para>
    ''' Score and Hessian updates are then:
    ''' </para>
    '''
    ''' <code>
    '''   Score  += Σ_events X_i − d * E[X]
    '''   Hessian -= d * ( E[XX] − E[X]E[X]^T )
    ''' </code>
    '''
    ''' <para>
    ''' Breslow typically works well for continuous event times but may be
    ''' biased when many ties occur. For such cases, the Efron method provides
    ''' a more accurate approximation.
    ''' </para>
    '''
    ''' <para>
    ''' Arguments:
    ''' </para>
    '''
    ''' <list type="bullet">
    ''' <item>
    ''' <description><paramref name="riskSet"/> — List of individuals at risk at the event time.</description>
    ''' </item>
    ''' <item>
    ''' <description><paramref name="events"/> — List of tied events occurring at the same time.</description>
    ''' </item>
    ''' <item>
    ''' <description><paramref name="exbRisk"/> — precomputed exp(η) for risk-set individuals.</description>
    ''' </item>
    ''' <item>
    ''' <description><paramref name="exbEvents"/> — exp(η) for event individuals (unused for Breslow but kept for consistency).</description>
    ''' </item>
    ''' <item>
    ''' <description><paramref name="beta"/> — current coefficient vector β.</description>
    ''' </item>
    ''' <item>
    ''' <description><paramref name="score"/> — score vector updated in-place.</description>
    ''' </item>
    ''' <item>
    ''' <description><paramref name="info"/> — Hessian matrix updated in-place.</description>
    ''' </item>
    ''' </list>
    '''
    ''' <para>
    ''' Matches the behavior of:<br/>
    '''     <c>coxph(..., ties = "breslow")</c> in R.
    ''' </para>
    ''' </summary>
    Private Sub UpdateBreslow(riskSet As List(Of survival.SurvivalRecord),
                          events As List(Of survival.SurvivalRecord),
                          exbRisk() As Double,
                          exbEvents() As Double,
                          beta() As Double,
                          ByRef score() As Double,
                          ByRef info(,) As Double)

        Dim p = beta.Length
        Dim d = events.Count

        ' Compute weighted sums over risk set
        Dim sumExp = 0.0
        Dim sumExpX(p - 1) As Double
        Dim sumExpXX(p - 1, p - 1) As Double

        For i = 0 To riskSet.Count - 1
            Dim r = riskSet(i)
            Dim w = exbRisk(i)

            sumExp += w

            For j = 0 To p - 1
                Dim xj = r.Covariates(j)
                sumExpX(j) += w * xj

                For m = 0 To p - 1
                    sumExpXX(j, m) += w * xj * r.Covariates(m)
                Next
            Next
        Next

        ' Score contribution: sum over events
        For Each e In events
            For j = 0 To p - 1
                score(j) += e.Covariates(j) - sumExpX(j) / sumExp
            Next
        Next

        ' Information matrix contribution: d * expected second derivative
        For j = 0 To p - 1
            For m = 0 To p - 1
                info(j, m) -= d * (sumExpXX(j, m) / sumExp - (sumExpX(j) * sumExpX(m)) / (sumExp * sumExp))
            Next
        Next

    End Sub

    ' ---------------------------
    ' Efron tie handling
    ' ---------------------------
    ''' <summary>
    ''' Updates the score vector and observed information (Hessian) for the 
    ''' Cox proportional hazards model using the
    ''' <b>Efron approximation</b> for handling tied event times.
    '''
    ''' <para>
    ''' Efron's method improves upon the Breslow approximation by accounting
    ''' for the fact that tied events reduce the risk set gradually rather
    ''' than all at once. It approximates the average over all d! possible
    ''' orderings of the tied failures.
    ''' </para>
    '''
    ''' <para>The Efron denominator at sub-step l = 0,…,d−1 is:</para>
    '''
    ''' <code>
    '''     denom_l = Σ exp(η_i) − (l/d) * Σ_events exp(η_i)
    ''' </code>
    '''
    ''' <para>
    ''' Expected covariate values are adjusted similarly at each sub-step.
    ''' For each tied event block:
    ''' </para>
    '''
    ''' <code>
    '''   Score  += X_event(l) − E_l[X]
    '''   Hessian -= ( E_l[XX] − E_l[X]E_l[X]^T )
    ''' </code>
    '''
    ''' <para>
    ''' This produces near-exact results for moderate tie sizes and is the
    ''' default in most modern Cox model implementations.
    ''' </para>
    '''
    ''' <para>Arguments:</para>
    '''
    ''' <list type="bullet">
    ''' <item><description><paramref name="riskSet"/> — subjects at risk at the event time.</description></item>
    ''' <item><description><paramref name="events"/> — tied event records.</description></item>
    ''' <item><description><paramref name="exbRisk"/> — exp(η) for risk-set individuals.</description></item>
    ''' <item><description><paramref name="exbEvents"/> — exp(η) for event individuals.</description></item>
    ''' <item><description><paramref name="beta"/> — current coefficient vector.</description></item>
    ''' <item><description><paramref name="score"/> — score contributions added in-place.</description></item>
    ''' <item><description><paramref name="info"/> — Hessian updated in-place.</description></item>
    ''' </list>
    '''
    ''' <para>
    ''' Matches the behavior of:<br/>
    '''     <c>coxph(..., ties = "efron")</c> in R's <c>survival</c> package.
    ''' </para>
    ''' </summary>
    Private Sub UpdateEfron(riskSet As List(Of survival.SurvivalRecord),
                        events As List(Of survival.SurvivalRecord),
                        exbRisk() As Double,
                        exbEvents() As Double,
                        beta() As Double,
                        ByRef score() As Double,
                        ByRef info(,) As Double)

        Dim p = beta.Length
        Dim d = events.Count

        ' Totals over the full risk set
        Dim totalExp = exbRisk.Sum()

        Dim totalExpX(p - 1) As Double
        Dim totalExpXX(p - 1, p - 1) As Double

        For i = 0 To riskSet.Count - 1
            Dim r = riskSet(i)
            Dim w = exbRisk(i)

            For j = 0 To p - 1
                Dim xj = r.Covariates(j)
                totalExpX(j) += w * xj

                For m = 0 To p - 1
                    totalExpXX(j, m) += w * xj * r.Covariates(m)
                Next
            Next
        Next

        ' Sums for tied events
        Dim totalEventExp = exbEvents.Sum()

        Dim totalEventX(p - 1) As Double
        Dim totalEventXX(p - 1, p - 1) As Double

        For e = 0 To d - 1
            Dim w = exbEvents(e)
            Dim obs = events(e)

            For j = 0 To p - 1
                Dim xj = obs.Covariates(j)
                totalEventX(j) += w * xj

                For m = 0 To p - 1
                    totalEventXX(j, m) += w * xj * obs.Covariates(m)
                Next
            Next
        Next

        ' Efron approximation loop: l = number of events already removed
        For l = 0 To d - 1
            Dim frac = l / CDbl(d)

            ' Denominator for this sub-step
            Dim denom = totalExp - frac * totalEventExp

            ' Adjusted first moment
            Dim adjX(p - 1) As Double
            For j = 0 To p - 1
                adjX(j) = totalExpX(j) - frac * totalEventX(j)
            Next

            ' Adjusted second moment
            Dim adjXX(p - 1, p - 1) As Double
            For j = 0 To p - 1
                For m = 0 To p - 1
                    adjXX(j, m) = totalExpXX(j, m) - frac * totalEventXX(j, m)
                Next
            Next

            ' Score contribution from event l
            Dim ev = events(l)
            For j = 0 To p - 1
                score(j) += ev.Covariates(j) - adjX(j) / denom
            Next

            ' Information matrix contribution
            For j = 0 To p - 1
                For m = 0 To p - 1
                    info(j, m) -= (adjXX(j, m) / denom) - (adjX(j) * adjX(m)) / (denom * denom)
                Next
            Next
        Next

    End Sub

    ' ---------------------------
    ' Exact tie handling (discrete)
    ' ---------------------------
    ''' <summary>
    ''' Updates the score vector and Hessian for the Cox proportional hazards
    ''' model using the <b>exact partial likelihood</b> for tied event times.
    '''
    ''' <para>
    ''' The exact method computes the conditional likelihood of observing the
    ''' specific subset of <c>d</c> tied failures out of the risk set by summing 
    ''' over <i>all</i> possible combinations S of size d:
    ''' </para>
    '''
    ''' <code>
    '''   Z      = Σ_{|S|=d} exp( Σ_{i∈S} η_i )
    '''   ZX(j)  = Σ exp(η_S) * Σ_{i∈S} X_{ij}
    '''   ZXX(j,m) = Σ exp(η_S) * [Σ_{i∈S} X_{ij}] [Σ_{i∈S} X_{im}]
    ''' </code>
    '''
    ''' <para>
    ''' The expected covariate sums under the exact distribution are:
    ''' </para>
    '''
    ''' <code>
    '''   E[ Σ X ]   = ZX / Z
    '''   E[ (Σ X)(Σ X)^T ] = ZXX / Z
    ''' </code>
    '''
    ''' <para>
    ''' Score and Hessian updates:
    ''' </para>
    '''
    ''' <code>
    '''   Score  += Σ_events X_i − E[Σ X]
    '''   Hessian -= ( E[XX] − E[X]E[X]^T )
    ''' </code>
    '''
    ''' <para>
    ''' Direct enumeration of all subsets is combinatorial and infeasible for
    ''' large risk sets. This implementation uses the <b>dynamic programming</b>
    ''' algorithm described by Terry Therneau, also used in the R
    ''' <c>survival</c> package, to compute the exact quantities efficiently.
    ''' </para>
    '''
    ''' <para>
    ''' Complexity: O(nRisk × d × p²), enabling exact likelihood evaluation
    ''' for typical tie sizes (d ≤ 10).
    ''' </para>
    '''
    ''' <para>Arguments:</para>
    '''
    ''' <list type="bullet">
    ''' <item><description><paramref name="riskSet"/> — individuals at risk at time t.</description></item>
    ''' <item><description><paramref name="events"/> — tied failures.</description></item>
    ''' <item><description><paramref name="exbRisk"/> — exp(η) for risk-set individuals.</description></item>
    ''' <item><description><paramref name="exbEvents"/> — exp(η) for event individuals (not needed by exact).</description></item>
    ''' <item><description><paramref name="beta"/> — current parameter vector.</description></item>
    ''' <item><description><paramref name="score"/> — accumulated score, updated in-place.</description></item>
    ''' <item><description><paramref name="info"/> — accumulated Hessian, updated in-place.</description></item>
    ''' </list>
    '''
    ''' <para>
    ''' This implementation matches the numerical behavior of:<br/>
    '''     <c>coxph(..., ties = "exact")</c>
    ''' </para>
    ''' </summary>
    Private Sub UpdateExact(riskSet As List(Of survival.SurvivalRecord),
                        events As List(Of survival.SurvivalRecord),
                        exbRisk() As Double,
                        exbEvents() As Double,
                        beta() As Double,
                        ByRef score() As Double,
                        ByRef info(,) As Double)

        Dim p As Integer = beta.Length
        Dim d As Integer = events.Count
        Dim nRisk As Integer = riskSet.Count

        ' Edge cases: if only 1 event, exact == standard Cox
        If d <= 1 Then
            ' Degenerates to standard single-event update:
            ' risk set already defined; treat like Breslow with d=1.
            Dim sumExp As Double = 0.0
            Dim sumExpX(p - 1) As Double
            Dim sumExpXX(p - 1, p - 1) As Double

            For i = 0 To nRisk - 1
                Dim r = riskSet(i)
                Dim w = exbRisk(i)
                sumExp += w
                For j = 0 To p - 1
                    Dim xj = r.Covariates(j)
                    sumExpX(j) += w * xj
                    For m = 0 To p - 1
                        sumExpXX(j, m) += w * xj * r.Covariates(m)
                    Next
                Next
            Next

            Dim ev = events(0)
            For j = 0 To p - 1
                score(j) += ev.Covariates(j) - sumExpX(j) / sumExp
            Next

            For j = 0 To p - 1
                For m = 0 To p - 1
                    info(j, m) -= sumExpXX(j, m) / sumExp - (sumExpX(j) * sumExpX(m)) / (sumExp * sumExp)
                Next
            Next

            Return
        End If

        ' ---- Dynamic programming arrays ----
        ' We use:
        '   a(k)        = Σ exp(η_S) over all subsets S of size k
        '   a2(k,j)     = Σ exp(η_S) * Σ_{i∈S} X_{ij}
        '   a3(k,j,m)   = Σ exp(η_S) * [Σ_{i∈S} X_{ij}] [Σ_{i∈S} X_{im}]
        '
        ' At the end we only need k = d.

        Dim a(d) As Double
        Dim a2(d, p - 1) As Double
        Dim a3(d, p - 1, p - 1) As Double

        ' Base case: k = 0 -> empty subset
        a(0) = 1.0
        ' a2(0,*) and a3(0,*,*) are already 0 by default

        ' Pre-extract X to avoid repeated property access
        Dim Xrisk(nRisk - 1)() As Double
        For i = 0 To nRisk - 1
            Xrisk(i) = riskSet(i).Covariates
        Next

        ' ---- DP over risk set ----
        For i = 0 To nRisk - 1
            Dim w As Double = exbRisk(i)
            Dim x() As Double = Xrisk(i)

            ' maximum subset size we can form with (i+1) items is min(i+1, d)
            Dim maxK As Integer = Math.Min(i + 1, d)

            ' update k from maxK down to 1 to avoid overwriting needed states
            For k As Integer = maxK To 1 Step -1

                Dim akm1 As Double = a(k - 1)
                If akm1 = 0.0 Then
                    Continue For
                End If

                Dim temp As Double = w * akm1

                ' update a(k)
                a(k) += temp

                ' update a2(k,*)
                For j As Integer = 0 To p - 1
                    Dim sumPrev As Double = a2(k - 1, j)
                    ' new sum for this subset level includes:
                    '   w * previous sum + temp * xj
                    a2(k, j) += w * sumPrev + temp * x(j)
                Next

                ' update a3(k,*,*)
                For j As Integer = 0 To p - 1
                    Dim sumPrev_j As Double = a2(k - 1, j)
                    Dim xj As Double = x(j)

                    For m As Integer = 0 To p - 1
                        Dim sumPrev_m As Double = a2(k - 1, m)
                        Dim x_m As Double = x(m)

                        Dim sumPrev_jm As Double = a3(k - 1, j, m)

                        ' expansion:
                        ' new contribution for k from this subject is:
                        '   w * sumPrev_jm
                        ' + w * sumPrev_j * x_m
                        ' + w * sumPrev_m * xj
                        ' + temp * xj * x_m
                        a3(k, j, m) += w * sumPrev_jm + w * sumPrev_j * x_m + w * sumPrev_m * xj + temp * xj * x_m
                    Next
                Next
            Next
        Next

        ' ---- Extract k = d (the number of tied events) ----
        Dim Z As Double = a(d)
        If Z <= 0.0 Then
            ' Should not happen unless underflow; fallback to Breslow-like
            ' or simply return.
            Return
        End If

        ' E[ sum X ] = a2(d,*) / Z
        ' E[ (sum X)(sum X)^T ] = a3(d,*,*) / Z

        Dim E_sumX(p - 1) As Double
        Dim E_sumXX(p - 1, p - 1) As Double

        For j As Integer = 0 To p - 1
            E_sumX(j) = a2(d, j) / Z
            For m As Integer = 0 To p - 1
                E_sumXX(j, m) = a3(d, j, m) / Z
            Next
        Next

        ' ---- Score contribution: Σ_events X_i − E[ Σ_S X_i ] ----
        ' Add observed X for each tied event
        For Each ev In events
            For j As Integer = 0 To p - 1
                score(j) += ev.Covariates(j)
            Next
        Next

        ' Subtract exact expected sum of covariates over all d-sized failure subsets
        For j As Integer = 0 To p - 1
            score(j) -= E_sumX(j)
        Next

        ' ---- Hessian contribution: −( E[SS^T] − E[S]E[S]^T ) ----
        For j As Integer = 0 To p - 1
            For m As Integer = 0 To p - 1
                Dim cov_jm As Double = E_sumXX(j, m) - E_sumX(j) * E_sumX(m)
                info(j, m) -= cov_jm
            Next
        Next

    End Sub

    Private Function InvertNegHessian(info(,) As Double) As Double(,)
        ' A = -info (positive definite)
        Dim A(,) As Double = Matrix.MatrixMult(info, -1.0)
        Return Matrix.MatInv(A, "CHOL")
    End Function


    ''' <summary>
    ''' Computes the Lin–Wei–Ying robust (sandwich) variance estimator.
    '''
    ''' Var_robust = (H⁻¹) * ( Σ_i U_i U_iᵀ ) * (H⁻¹)
    '''
    ''' where:
    '''   • H = observed information matrix (negative Hessian)
    '''   • U_i = score residual for subject i (length p)
    '''
    ''' Matches: coxph(..., robust=TRUE)$var
    '''
    ''' </summary>
    Public Function ComputeSandwichVariance(info As Double(,)) As Double(,)
        Dim p As Integer = info.GetLength(0)

        ' (1) Model-based covariance = (-H)^(-1)
        Dim Hinv As Double(,) = InvertNegHessian(info)  ' dimension p×p

        ' (2) Get per-subject score residuals U_i  (each: p-vector)
        Dim score As Dictionary(Of Integer, Double()) = ComputeScoreResiduals()

        ' (3) Compute meat = Σ_i U_i U_iᵀ
        Dim meat(p - 1, p - 1) As Double

        For Each r In Me.pRecords
            Dim Ui As Double() = score(r.Index)

            If Ui.Length <> p Then AppGlobals.BSerr.LogAndThrow(New ArgumentException($"Score residual vector length {Ui.Length} does not match p={p}"))

            For i = 0 To p - 1
                For j = 0 To p - 1
                    meat(i, j) += Ui(i) * Ui(j)
                Next j
            Next i
        Next

        ' (4) Robust variance = Hinv * meat * Hinv
        Dim temp(p - 1, p - 1) As Double
        Dim robust(p - 1, p - 1) As Double

        ' First: temp = Hinv * meat
        For i = 0 To p - 1
            For j = 0 To p - 1
                Dim s As Double = 0
                For k = 0 To p - 1
                    s += Hinv(i, k) * meat(k, j)
                Next
                temp(i, j) = s
            Next
        Next

        ' Next: robust = temp * Hinvᵀ
        For i = 0 To p - 1
            For j = 0 To p - 1
                Dim s As Double = 0
                For k = 0 To p - 1
                    s += temp(i, k) * Hinv(j, k)   ' note: Hinvᵀ
                Next
                robust(i, j) = s
            Next
        Next

        Return robust
    End Function


    Public Function Residuals(resType As ResidualType) As Dictionary(Of Integer, Double())
        Select Case resType
            Case ResidualType.Score
                Return ComputeScoreResiduals()
            Case ResidualType.Martingale
                Return ComputeMartingaleResiduals()
            Case ResidualType.Deviance
                Return ComputeDevianceResiduals()
            Case ResidualType.Schoenfeld
                Return ComputeSchoenfeldResiduals()
            Case ResidualType.SchoenfeldScaled
                Return ComputeScaledSchoenfeld()
            Case ResidualType.Dfbeta
                Return ComputeDfbetaResiduals()
            Case ResidualType.Dfbetas
                Return ComputeDfbetas()
            Case ResidualType.CoxSnell
                Return ComputeCoxSnellResiduals()
            Case Else
                AppGlobals.BSerr.LogAndThrow(New ArgumentException("Unknown residual type."))
                Return Nothing
        End Select
    End Function

    ''' <summary>
    ''' Computes deviance residuals for the Cox proportional hazards model.
    '''
    ''' Deviance residuals (D_i) transform martingale residuals into a 
    ''' more symmetric distribution, improving diagnostic interpretation.
    '''
    ''' They are defined as:
    ''' 
    '''   If δ_i = 1:
    '''       D_i = sign(−M_i) * sqrt( −2 * ( M_i + log(1 + M_i) ) )
    '''   If δ_i = 0:
    '''       D_i = sign(−M_i) * sqrt( −2 * M_i )
    '''
    ''' where M_i is the martingale residual.
    '''
    ''' Corresponds to residuals(fit, type="deviance") in R.
    ''' </summary>
    ''' <returns>
    ''' A dictionary mapping subject Index → Double(0), containing deviance 
    ''' residuals for each subject.
    ''' </returns>

    Private Function ComputeDevianceResiduals() As Dictionary(Of Integer, Double())
        Dim mart As Dictionary(Of Integer, Double())
        If Me.pMartingaleResiduals Is Nothing Then
            mart = ComputeMartingaleResiduals()
        Else
            mart = Me.pMartingaleResiduals
        End If
        Dim res As New Dictionary(Of Integer, Double())

        For Each r In Me.pRecords
            Dim m As Double = mart(r.Index)(0)
            Dim inside As Double

            If r.Censorship = 1 Then ' event: m + log(1 - m)
                inside = m + Math.Log(1.0 - m)
            Else ' censored: m  (since r.Censorship = 0 so log term = 0)
                inside = m
            End If

            ' deviance = sign(m) * sqrt(-2 * inside)
            Dim d As Double = Math.Sign(m) * Math.Sqrt(Math.Max(0.0, -2.0 * inside))

            res(r.Index) = {d}
        Next

        Return res
    End Function

    ''' <summary>
    ''' Computes Schoenfeld residuals for the Cox model.
    '''
    ''' Schoenfeld residuals are defined only for subjects who experience 
    ''' events (δ_i = 1), and for each event time t they satisfy:
    ''' 
    '''     r_i = X_i − E[ X | event time = t ]
    ''' 
    ''' These residuals measure the deviation of the observed covariate 
    ''' at the event time from its risk-set expectation.
    '''
    ''' They are used extensively for:
    '''   • proportional hazards assumption diagnostics
    '''   • generating the scaled Schoenfeld residuals in cox.zph
    '''
    ''' Non-event subjects receive residuals of NaN.
    '''
    ''' Matches R's residuals(fit, type="schoenfeld").
    ''' </summary>
    ''' <returns>
    ''' Dictionary mapping subject Index → Double(p-1), or NaN for censored subjects.
    ''' </returns>
    Private Function ComputeSchoenfeldResiduals() As Dictionary(Of Integer, Double())
        Dim p As Integer = Me.pCoefficients.Length
        Dim result As New Dictionary(Of Integer, Double())()

        ' initialize all rows to zero for censored subjects
        Dim tmp(p - 1) As Double
        For i = 0 To p - 1 : tmp(i) = Double.NaN : Next
        For Each r In pRecords
            result(r.Index) = tmp
        Next

        ' Precompute exp(η)
        Dim exb As New Dictionary(Of Integer, Double)()
        For Each r In pRecords
            exb(r.Index) = Math.Exp(Matrix.DotProduct(r.Covariates, Me.pCoefficients))
        Next

        Dim strataGroups = pRecords.GroupBy(Function(r) r.Stratum)

        For Each sg In strataGroups

            ' Sort by time
            Dim group = sg.OrderBy(Function(r) r.Time).ToList()

            ' Precompute groupIndex lookup
            Dim gIndex As New Dictionary(Of Integer, Integer)
            For i = 0 To group.Count - 1
                gIndex(group(i).Index) = i
            Next

            ' Event groups
            Dim eventsByTime = group.Where(Function(r) r.Censorship = 1).
                                     GroupBy(Function(r) r.Time).
                                     OrderBy(Function(g) g.Key)

            For Each evGroup In eventsByTime

                Dim t = evGroup.Key
                Dim events = evGroup.ToList()
                Dim d = events.Count

                ' Risk set (same definition used in Fit)
                Dim risk = group.Where(Function(r) r.Time >= t).ToList()

                ' Compute ∑exp(η) and ∑x exp(η) over the risk set
                Dim denom As Double = 0
                Dim num(p - 1) As Double

                For Each rr In risk
                    Dim ei = exb(rr.Index)
                    denom += ei
                    For k = 0 To p - 1
                        num(k) += rr.Covariates(k) * ei
                    Next
                Next

                If pMethod = TieMethod.Breslow Or pMethod = TieMethod.Exact Then

                    ' Expected x̄(t)
                    Dim xbar(p - 1) As Double
                    For k = 0 To p - 1
                        xbar(k) = num(k) / denom
                    Next

                    ' For each event subject: Schoenfeld = x_i - x̄(t)
                    For Each ev In events
                        Dim vec = New Double(p - 1) {}
                        For k = 0 To p - 1
                            vec(k) = ev.Covariates(k) - xbar(k)
                        Next

                        result(ev.Index) = vec
                    Next

                ElseIf pMethod = TieMethod.Efron Then

                    ' Compute event sums
                    Dim sumExpEvents As Double = 0
                    Dim sumXExpEvents(p - 1) As Double

                    For Each ev In events
                        Dim ei = exb(ev.Index)
                        sumExpEvents += ei
                        For k = 0 To p - 1
                            sumXExpEvents(k) += ev.Covariates(k) * ei
                        Next
                    Next

                    ' For each tied event l = 0..d-1, apply fractional Efron risk
                    For l = 0 To d - 1
                        Dim ev = events(l)
                        Dim frac As Double = l / CDbl(d)

                        Dim denomL As Double = denom - frac * sumExpEvents

                        Dim xbarL(p - 1) As Double
                        For k = 0 To p - 1
                            Dim numL = num(k) - frac * sumXExpEvents(k)
                            xbarL(k) = numL / denomL
                        Next

                        Dim vec = New Double(p - 1) {}
                        For k = 0 To p - 1
                            vec(k) = ev.Covariates(k) - xbarL(k)
                        Next

                        result(ev.Index) = vec
                    Next
                End If
            Next
        Next
        Return result
    End Function

    ''' <summary>
    ''' Computes scaled Schoenfeld residuals for Cox PH, matching the output
    ''' used in R's cox.zph proportional hazards test.
    '''
    ''' Scaled Schoenfeld residuals are defined as:
    ''' 
    '''     r_i* = V(β)^{-1} * r_i
    ''' 
    ''' where:
    '''     V(β) = covariance matrix of β (model-based)
    '''     r_i = unscaled Schoenfeld residual
    ''' 
    ''' These residuals are used as dependent variables in the PH test.
    ''' </summary>
    ''' <returns>
    ''' Dictionary mapping subject Index → Double(p-1), containing scaled 
    ''' Schoenfeld residuals for event subjects; NaN for censored subjects.
    ''' </returns>
    Private Function ComputeScaledSchoenfeld() As Dictionary(Of Integer, Double())
        Dim sch As Dictionary(Of Integer, Double())
        If Me.pSchoenfeldResiduals Is Nothing Then
            sch = ComputeSchoenfeldResiduals()
        Else
            sch = Me.pSchoenfeldResiduals
        End If

        Dim p = Me.pCoefficients.Length
        Dim res As New Dictionary(Of Integer, Double())

        ' Total number of events
        Dim nEvents As Integer = Me.pRecords.Where(Function(r) r.Censorship = 1).Count

        For Each r In Me.pRecords

            Dim idx As Integer = r.Index
            Dim v As Double() = sch(idx)   ' Schoenfeld residual for this subject

            If r.Censorship = 0 Then
                ' Censored: no Schoenfeld residual → NaNs (R behavior)
                res(idx) = Enumerable.Repeat(Double.NaN, p).ToArray()
            Else
                ' Event: scaled = (nEvents * Var(β̂)) * schoenfeld
                Dim scaled(p - 1) As Double

                For i = 0 To p - 1
                    Dim s As Double = 0.0
                    For j = 0 To p - 1
                        s += nEvents * Me.pVarCov(i, j) * v(j)
                    Next
                    scaled(i) = s
                Next
                res(idx) = scaled
            End If
        Next
        Return res
    End Function

    ''' <summary>
    ''' Computes dfbeta residuals for each subject and coefficient. 
    ''' It's scaled score residuals called also as delta-betas.
    ''' 
    ''' dfbeta_i,j is the approximate change in coefficient β_j due to
    ''' subject i (i.e. β̂_j − β̂_j(−i)).
    ''' 
    ''' This matches residuals(fit, type = "dfbeta") from R's survival::coxph,
    ''' when using the same tie method and risk-set definition.
    ''' 
    ''' Mathematically:
    '''   dfbeta_i ≈ Var(β̂) %*% score_residual_i
    ''' where Var(β̂) = (−H(β̂))^{-1} = Me.pVarCov and score_residual_i is the
    ''' individual score residual vector for subject i.
    ''' </summary>
    ''' <returns>
    ''' Dictionary mapping subject Index → Double() (length p) dfbeta vector.
    ''' </returns>
    Private Function ComputeDfbetaResiduals() As Dictionary(Of Integer, Double())
        Dim p As Integer = Me.pCoefficients.Length
        If Me.pScoreResiduals Is Nothing Then Me.pScoreResiduals = ComputeScoreResiduals()

        Dim res As New Dictionary(Of Integer, Double())()

        ' dfbeta_i = Var(β̂) %*% score_residual_i
        For Each r In Me.pRecords
            Dim idx As Integer = r.Index
            Dim s As Double() = Me.pScoreResiduals(idx)   ' score residual vector for subject i

            Dim dfb(p - 1) As Double

            For j As Integer = 0 To p - 1
                Dim v As Double = 0.0
                For k As Integer = 0 To p - 1
                    v += Me.pVarCov(j, k) * s(k)
                Next
                dfb(j) = v
            Next

            res(idx) = dfb
        Next

        Return res
    End Function

    ''' <summary>
    ''' Computes standardized dfbeta residuals ("dfbetas") for Cox PH.
    '''
    ''' dfbetas are defined as:
    ''' 
    '''     dfbetas_i,j = dfbeta_i,j / SE_j
    ''' 
    ''' where:
    '''     SE = sqrt(diag( V(β) ))
    ''' 
    ''' These represent the influence of observation i on coefficient j
    ''' measured in standard-error units.
    '''
    ''' Equivalent to R's residuals(fit, type="dfbetas").
    ''' </summary>
    ''' <returns>
    ''' Dictionary(Index → Double(p-1)): standardized influence measures.
    ''' </returns>
    Private Function ComputeDfbetas() As Dictionary(Of Integer, Double())

        If Me.pDfbeta Is Nothing Then Me.pDfbeta = ComputeDfbetaResiduals()
        Dim p = Me.pCoefficients.Length

        ' Std errors = sqrt(diag(cov))
        Dim se(p - 1) As Double
        For j = 0 To p - 1
            se(j) = Math.Sqrt(Me.pVarCov(j, j))
        Next

        Dim res As New Dictionary(Of Integer, Double())

        For Each r In Me.pRecords
            Dim idx = r.Index
            Dim v = Me.pDfbeta(idx)
            Dim scaled(p - 1) As Double
            For j = 0 To p - 1
                scaled(j) = v(j) / se(j)
            Next
            res(idx) = scaled
        Next

        Return res
    End Function

    ''' <summary>
    ''' Computes score residuals for the Cox proportional hazards model.
    '''
    ''' Score residuals are the individual contributions to the Cox partial 
    ''' likelihood score vector U(β). They satisfy:
    '''     U(β) = Σ_i U_i(β)
    '''
    ''' These residuals are:
    '''   • p-dimensional (one component per covariate)
    '''   • used to construct the robust (sandwich) variance estimator
    '''   • identical to residuals(fit, type="score") in R's survival package
    '''
    ''' Tie handling (Breslow or Efron) follows the same logic used in
    ''' the main model fitting so results match R exactly.
    ''' </summary>
    ''' <returns>
    ''' A dictionary mapping subject Index → Double(p-1), containing the 
    ''' p-dimensional score residual for each subject.
    ''' </returns>
    Private Function ComputeScoreResiduals() As Dictionary(Of Integer, Double())
        Dim n As Integer = Me.pRecords.Count
        Dim p As Integer = Me.pCoefficients.Length

        Dim beta = Me.pCoefficients
        Dim method = Me.pMethod

        ' --------------------------------------------------------
        ' Map record.Index (arbitrary, not 0..n-1) -> global position in pRecords
        ' --------------------------------------------------------
        Dim idxToPos As New Dictionary(Of Integer, Integer)(n)
        For pos As Integer = 0 To n - 1
            idxToPos(Me.pRecords(pos).Index) = pos
        Next

        ' --------------------------------------------------------
        ' Precompute exp(η) for all records in global order
        ' --------------------------------------------------------
        Dim exb(n - 1) As Double
        For pos As Integer = 0 To n - 1
            exb(pos) = Math.Exp(Matrix.DotProduct(Me.pRecords(pos).Covariates, beta))
        Next

        ' --------------------------------------------------------
        ' Initialize result dictionary with zero vectors
        ' --------------------------------------------------------
        Dim result As New Dictionary(Of Integer, Double())()
        For Each r In Me.pRecords
            result(r.Index) = New Double(p - 1) {}
        Next

        ' --------------------------------------------------------
        ' Process strata separately (risk sets and hazards are stratum-specific)
        ' --------------------------------------------------------
        Dim strataGroups = Me.pRecords.GroupBy(Function(r) r.Stratum)

        For Each sg In strataGroups

            ' Sort by time within stratum
            Dim group = sg.OrderBy(Function(r) r.Time).ToList()

            ' Quick map: record.Index -> index in this stratum's group list
            Dim groupIndex As New Dictionary(Of Integer, Integer)(group.Count)
            For gi As Integer = 0 To group.Count - 1
                groupIndex(group(gi).Index) = gi
            Next

            ' All event-time blocks in this stratum
            Dim eventsByTime =
            group.Where(Function(r) r.Censorship = 1).
                  GroupBy(Function(r) r.Time).
                  OrderBy(Function(g) g.Key)

            ' ----------------------------------------------------
            ' Loop over event times and accumulate score residual contributions
            ' ----------------------------------------------------
            For Each evGroup In eventsByTime

                Dim t As Double = evGroup.Key
                Dim events = evGroup.ToList()
                Dim d As Integer = events.Count

                ' Risk set: all subjects with Time >= t
                Dim risk As List(Of survival.SurvivalRecord) = group.Where(Function(r) r.Time >= t).ToList()

                ' Sum r_i = exp(η_i) and r_i * x_i over risk set
                Dim denom As Double = 0.0
                Dim num(p - 1) As Double

                For Each rr In risk
                    Dim globalPos As Integer = idxToPos(rr.Index)
                    Dim w As Double = exb(globalPos)
                    denom += w
                    For k As Integer = 0 To p - 1
                        num(k) += rr.Covariates(k) * w
                    Next
                Next

                If denom <= 0.0 Then Continue For ' Should never happen; skip this time point if it does

                ' Expected covariate vector E[X | R(t)]
                Dim xbar(p - 1) As Double
                For k As Integer = 0 To p - 1
                    xbar(k) = num(k) / denom
                Next

                ' Baseline hazard increment ΔΛ0(t) in this stratum at time t
                Dim dH As Double = 0.0

                If method = TieMethod.Efron Then
                    ' Efron increment: sum over d pseudo-steps
                    Dim sumExpEvents As Double = 0.0
                    For Each ev In events
                        Dim gpos As Integer = idxToPos(ev.Index)
                        sumExpEvents += exb(gpos)
                    Next

                    For l As Integer = 0 To d - 1
                        Dim frac As Double = l / CDbl(d)
                        Dim denomL As Double = denom - frac * sumExpEvents
                        dH += 1.0 / denomL
                    Next
                Else
                    ' Breslow-style increment (used for Breslow and as a reasonable
                    ' approximation for Exact baseline)
                    dH = d / denom
                End If

                ' Prepare a quick lookup for which subjects fail at time t
                Dim eventIds As New HashSet(Of Integer)(
                events.Select(Function(ev) ev.Index))

                ' --------------------------------------------------------
                ' Now update score residuals for ALL subjects in the risk set
                ' Score increment at t:
                '   ΔU_i = (x_i − x̄(t)) * ΔM_i
                ' where ΔM_i = I(i fails at t) − r_i * ΔΛ0(t)
                ' --------------------------------------------------------
                For Each rr In risk
                    Dim rid As Integer = rr.Index
                    Dim globalPos As Integer = idxToPos(rid)
                    Dim r_i As Double = exb(globalPos)

                    Dim dN_i As Double = If(eventIds.Contains(rid), 1.0, 0.0)
                    Dim dM_i As Double = dN_i - r_i * dH

                    If dM_i = 0.0 Then Continue For

                    Dim resVec = result(rid)
                    For k As Integer = 0 To p - 1
                        resVec(k) += (rr.Covariates(k) - xbar(k)) * dM_i
                    Next
                Next
            Next ' event time
        Next ' stratum

        Return result
    End Function


    ''' <summary>
    ''' Computes martingale residuals for each subject in the Cox model.
    '''
    ''' Martingale residuals are defined as:
    '''     M_i = δ_i − Λ̂_i(t_i)
    ''' where:
    '''     δ_i = event indicator (1 = event, 0 = censored)
    '''     Λ̂_i(t_i) = estimated cumulative hazard for subject i at its observed time.
    '''
    ''' Properties:
    '''   • Mean ≈ 0 at convergence
    '''   • Highly skewed, especially for censored observations
    '''   • Basis for deviance and Anscombe residuals
    '''   • Matches R's residuals(fit, type="martingale")
    '''
    ''' Computation uses the Breslow estimator for the baseline hazard,
    ''' following R's implementation.
    ''' </summary>
    ''' <returns>
    ''' A dictionary mapping subject Index → Double(0), containing one 
    ''' martingale residual per subject.
    ''' </returns>
    Private Function ComputeMartingaleResiduals() As Dictionary(Of Integer, Double())

        Dim n As Integer = pRecords.Count
        Dim beta = pCoefficients

        ' Get baseline cumulative hazard H0(t) per stratum
        Dim baseline = ComputeBaseline(False)   ' must match R's basehaz()

        Dim res As New Dictionary(Of Integer, Double())

        For Each r In pRecords

            ' compute exp(eta)
            Dim eta As Double = Matrix.DotProduct(r.Covariates, beta)
            Dim exb As Double = Math.Exp(eta)

            ' lookup H0 at THIS subject's observed time
            Dim H0 As Double = 0.0
            Dim bl = baseline(r.Stratum)

            ' R uses H0(t_i), where t_i may be censored
            ' find last H0(t_k) where t_k <= t_i OR exact match
            For i = 0 To UBound(bl, 1)
                If bl(i, 0) <= r.Time Then
                    H0 = bl(i, 2) 'cumulative hazard
                Else
                    Exit For
                End If
            Next

            ' Martingale residual = delta_i - H0(t_i) * exp(eta_i)
            Dim M As Double = r.Censorship - H0 * exb
            res(r.Index) = {M}
        Next

        Return res

    End Function

    ''' <summary>
    ''' Computes Cox–Snell residuals for all subjects.
    '''
    ''' Cox–Snell residual for subject i is:
    '''     r_i = H0(t_i) * exp(eta_i)
    ''' where:
    '''     H0(t) is baseline cumulative hazard at time t (matching R's basehaz)
    '''     eta_i = x_i · beta
    '''
    ''' Valid for events and censored subjects.
    ''' Works for Breslow, Efron, and Exact tie-handling.
    ''' </summary>
    ''' <returns>
    ''' Dictionary mapping subject Index → Cox–Snell residual value.
    ''' </returns>
    Private Function ComputeCoxSnellResiduals() As Dictionary(Of Integer, Double())
        Dim beta = Me.pCoefficients
        Dim res As New Dictionary(Of Integer, Double())

        ' 1) Get baseline hazard per stratum (match R basehaz / fitted Cox model)
        'Dim baseline = ComputeBaseline()   ' Dictionary(stratum → List(Of BaselinePoint))
        Dim baseline = ComputeBaseline(bZeroBetas:=False)

        ' 2) For each subject: r_i = H0(t_i) * exp(eta)
        For Each r In Me.pRecords

            Dim eta As Double = Matrix.DotProduct(r.Covariates, beta)
            Dim risk As Double = Math.Exp(eta)

            Dim H0 As Double = 0.0

            ' Find baseline cumulative hazard at subject's time
            Dim basePts = baseline(r.Stratum)

            ' Binary search not required; list is sorted by time
            For i = 0 To UBound(basePts)
                If basePts(i, 0) <= r.Time Then
                    H0 = basePts(i, 2)
                Else
                    Exit For
                End If
            Next

            Dim cs As Double = H0 * risk
            res(r.Index) = {cs}
        Next
        Return res
    End Function

    '------------------
    ' QR-based SolveLinearSystem wrapper for CoxPH
    '------------------
    ''' <summary>
    ''' Solves a linear system of equations A * x = b using the QR decomposition method.
    ''' This implementation leverages user-provided QR routines:
    ''' QRdecomp() to compute the QR decomposition of A, and QRsolve() to solve R * x = Q^T * b.
    ''' 
    ''' Steps:
    ''' 1. Converts the 1D vector b into a column matrix for compatibility with QRsolve.
    ''' 2. Computes the QR decomposition of the input square matrix A.
    ''' 3. Solves for x using the decomposition (R x = Q^T b).
    ''' 4. Converts the resulting column vector back into a 1D array.
    '''  
    ''' This method replaces the previous Gaussian elimination-based solver, providing
    ''' improved numerical stability and robustness for Newton-Raphson updates in Cox regression.
    ''' </summary>
    ''' <param name="A">Input square matrix representing the Hessian (n x n).</param>
    ''' <param name="b">Right-hand side vector (length n) representing the score vector.</param>
    ''' <returns>Output vector (length n) to store the solution.</returns>
    Function SolveLinearSystem(A As Double(,), b As Double()) As Double()
        ' Convert b to column vector (n x 1)
        Dim n = b.Length
        Dim bCol(n - 1, 0) As Double
        For i = 0 To n - 1
            bCol(i, 0) = b(i)
        Next

        ' Compute QR decomposition
        Dim QR As Matrix.QRout = Matrix.QRdecomp(A)
        ' Solve R x = Q^T b using user QRsolve routine
        Dim xCol(,) As Double = Matrix.QRsolve(QR, bCol)

        ' Convert column vector back to 1D array
        Dim x = Matrix.GetColumnFrom2Darray(xCol, 0)
        Return x
    End Function

    ''' <summary>
    ''' Computes the Cox partial log-likelihood for the current coefficient
    ''' vector β, using the selected tie-handling method (Breslow, Efron, or Exact).
    '''
    ''' <para>
    ''' The Cox partial likelihood is defined as:
    ''' </para>
    ''' 
    ''' <code>
    '''   ℓ(β) = Σ_{events i} [ η_i − log( Σ_{j ∈ R(t_i)} exp(η_j) ) ]
    ''' </code>
    ''' 
    ''' <para>
    ''' In the presence of tied failures at a common event time t, the log-likelihood
    ''' differs depending on the tie-handling method:
    ''' </para>
    ''' 
    ''' <list type="bullet">
    '''   <item>
    '''     <description>
    '''     <b>Breslow</b>: subtracts d·log(Σ exp(η)) once per tied block.
    '''     </description>
    '''   </item>
    '''
    '''   <item>
    '''     <description>
    '''     <b>Efron</b>: averages the denominators over d substeps, producing:
    '''     </description>
    '''     <code>
    '''       ℓ += Σ η_event − Σ_{l=0}^{d−1} log( Σ exp(η) − (l/d) Σ_event exp(η) )
    '''     </code>
    '''   </item>
    '''
    '''   <item>
    '''     <description>
    '''     <b>Exact</b>: uses the exact conditional likelihood:
    '''     </description>
    '''     <code>
    '''       ℓ += log( Σ_{|S|=d} exp(Σ_{i∈S} η_i) ) − d·log( Σ_{risk} exp(η) )
    '''     </code>
    '''     <para>
    '''     where the sum is over all subsets S of size d from the risk set.
    '''     Dynamic programming is used to evaluate the numerator efficiently.
    '''     </para>
    '''   </item>
    ''' </list>
    '''
    ''' <h2>Risk Set Definition</h2>
    ''' <para>
    ''' This function uses left-continuous risk sets as in R’s <c>coxph</c>:
    ''' </para>
    ''' 
    ''' <code>
    '''   R(t) = { i : Time_i ≥ t }
    ''' </code>
    '''
    ''' <para>
    ''' This ensures numerical equivalence with the hazard convention used in
    ''' the <c>survival</c> package.
    ''' </para>
    '''
    ''' <h2>Purpose</h2>
    ''' <para>
    ''' The log-likelihood is used to:
    ''' </para>
    ''' 
    ''' <list type="bullet">
    '''   <item><description>Monitor improvement during Newton–Raphson iterations.</description></item>
    '''   <item><description>Control step-halving when updates decrease ℓ(β).</description></item>
    '''   <item><description>Compute likelihood-based statistics (AIC, deviance, etc.).</description></item>
    ''' </list>
    '''
    ''' <h2>Compatibility</h2>
    ''' <para>
    ''' When the same tie-handling method and risk-set definition are used,
    ''' this implementation yields log-likelihood values numerically identical
    ''' to those from:
    ''' </para>
    ''' 
    ''' <code>
    '''   coxph(..., ties = "breslow" | "efron" | "exact")
    ''' </code>
    ''' 
    ''' <para>
    ''' in the R <c>survival</c> package.
    ''' </para>
    '''
    ''' </summary>
    ''' <param name="beta">
    ''' The coefficient vector β at which the partial log-likelihood is evaluated.
    ''' </param>
    ''' <returns>
    ''' The scalar partial log-likelihood ℓ(β) for the Cox model under the chosen tie method.
    ''' </returns>
    Function ComputeLogLikelihood(beta() As Double) As Double

        Dim loglik As Double = 0.0
        Dim p As Integer = beta.Length

        ' Group data by stratum
        Dim strataGroups = pRecords.GroupBy(Function(r) r.Stratum).ToList()

        For Each sg In strataGroups

            ' Sort within stratum by time
            Dim group = sg.OrderBy(Function(r) r.Time).ToList()

            ' Precompute exp(η) for all subjects in this stratum
            Dim exb(group.Count - 1) As Double
            For i = 0 To group.Count - 1
                exb(i) = Math.Exp(Matrix.DotProduct(group(i).Covariates, beta))
            Next

            ' Event times (only where Censorship = 1)
            Dim eventsByTime = group.Where(Function(r) r.Censorship = 1).
                                     GroupBy(Function(r) r.Time).
                                     ToDictionary(Function(g) g.Key, Function(g) g.ToList())

            ' Iterate through event times
            For Each kvp In eventsByTime

                Dim t As Double = kvp.Key
                Dim events = kvp.Value
                Dim d As Integer = events.Count

                ' Risk set indices: Time >= t (matches R's coxph)
                Dim riskSetIdx = group.Select(Function(r, idx) New With {.Rec = r, .Idx = idx}).
                                       Where(Function(x) x.Rec.Time >= t).
                                       Select(Function(x) x.Idx).ToList()

                ' Sum exp(η) over risk set
                Dim sumRiskExp As Double = 0.0
                For Each idx In riskSetIdx
                    sumRiskExp += exb(idx)
                Next

                ' Sum η and exp(η) over events
                Dim sumEtaEvents As Double = 0.0
                Dim sumExpEvents As Double = 0.0

                For Each ev In events
                    Dim idx As Integer = group.IndexOf(ev)
                    sumEtaEvents += Math.Log(exb(idx))  ' η_i = log(exp(η_i))
                    sumExpEvents += exb(idx)
                Next

                Select Case Me.pMethod

                ' ----------------------------------------------------------
                ' BRESLOW
                ' ----------------------------------------------------------
                    Case TieMethod.Breslow
                        loglik += sumEtaEvents - d * Math.Log(sumRiskExp)

                ' ----------------------------------------------------------
                ' EFRON
                ' ----------------------------------------------------------
                    Case TieMethod.Efron

                        loglik += sumEtaEvents
                        For l = 0 To d - 1
                            Dim frac As Double = l / CDbl(d)
                            Dim denom As Double = sumRiskExp - frac * sumExpEvents
                            loglik -= Math.Log(denom)
                        Next

                ' ----------------------------------------------------------
                ' EXACT (Dynamic Programming)
                ' ----------------------------------------------------------
                    Case TieMethod.Exact

                        Dim nRisk As Integer = riskSetIdx.Count
                        Dim exbRisk(nRisk - 1) As Double
                        For i As Integer = 0 To nRisk - 1
                            exbRisk(i) = exb(riskSetIdx(i))
                        Next

                        ' DP arrays:
                        ' a(k) = Σ exp(η_S) over subsets S of size k
                        Dim a(d) As Double
                        a(0) = 1.0  ' base case: empty subset

                        ' Dynamic programming over risk set
                        For i As Integer = 0 To nRisk - 1
                            Dim w As Double = exbRisk(i)
                            Dim maxK As Integer = Math.Min(d, i + 1)
                            For k As Integer = maxK To 1 Step -1
                                a(k) += w * a(k - 1)
                            Next
                        Next

                        Dim Z As Double = a(d)  ' exact numerator
                        If Z <= 0.0 OrElse Double.IsNaN(Z) Then
                            Z = Double.Epsilon
                        End If

                        ' exact log-likelihood:
                        ' ℓ_t(β) = Σ_{events} η_i − log( Σ_{|S|=d} exp(Σ_{j∈S} η_j) )
                        loglik += sumEtaEvents - Math.Log(Z)
                End Select
            Next
        Next

        Return loglik
    End Function

    ''' <summary>
    ''' Computes approximate likelihood displacement for each subject,
    ''' based on the subject-specific score residuals and the model-based
    ''' variance-covariance matrix of the coefficients.
    '''
    ''' Formally, for subject i:
    ''' 
    '''   LD_i ≈ U_i^T * Var(β̂) * U_i
    ''' 
    ''' where:
    '''   U_i      = score residual vector for subject i
    '''   Var(β̂)  = (−H(β̂))^{-1}  (model-based covariance matrix)
    ''' 
    ''' This quantity is a first-order approximation to
    '''   2 * [ ℓ(β̂) − ℓ(β̂_(−i)) ]
    ''' and serves as a likelihood–based influence measure, analogous
    ''' to Cook's distance in GLMs.
    ''' 
    ''' It relies on:
    '''   • The final fitted coefficients (Me.pCoefficients)
    '''   • The model-based covariance matrix (Me.pVarCov)
    '''   • Score residuals computed via ComputeScoreResiduals.
    ''' 
    ''' Returned dictionary:
    '''   Key   = subject Index
    '''   Value = approximate likelihood displacement for that subject.
    ''' </summary>
    Private Function ComputeLikelihoodDisplacement() As Dictionary(Of Integer, Double())
        Dim p As Integer = Me.pCoefficients.Length

        ' Score residuals U_i (vector per subject), already matching R
        If Me.pScoreResiduals Is Nothing Then Me.pScoreResiduals = ComputeScoreResiduals()

        Dim result As New Dictionary(Of Integer, Double())

        For Each r In Me.pRecords

            Dim idx As Integer = r.Index
            Dim u As Double() = Me.pScoreResiduals(idx)   ' length p

            ' LD_i ≈ U_i^T * Var(β̂) * U_i
            Dim ld As Double = 0.0
            For j As Integer = 0 To p - 1
                Dim tmp As Double = 0.0
                For k As Integer = 0 To p - 1
                    tmp += Me.pVarCov(j, k) * u(k)
                Next
                ld += u(j) * tmp
            Next
            result(idx) = {ld}
        Next
        Return result
    End Function

    ''' <summary>
    ''' Tests the proportional hazards assumption using a score test
    ''' based on scaled Schoenfeld residuals (Grambsch–Therneau/cox.zph style).
    '''
    ''' For each covariate j:
    '''   - Extract scaled Schoenfeld residuals z_{ij} at each event time t_i
    '''   - Apply a time transform g(t_i)  (default: "rank")
    '''   - Compute the correlation ρ_j between z_{ij} and g(t_i)
    '''   - Test H0: slope = 0  via χ² = m * ρ_j²  with 1 df,
    '''     where m is the number of events.
    '''
    ''' A global test is formed by summing the per-variable χ² statistics,
    ''' with df equal to the number of covariates.
    '''
    ''' Notes:
    '''   • This uses the existing ComputeScaledSchoenfeld() method.
    '''   • Small numerical differences vs R's cox.zph are expected because
    '''     we use a simpler time transform ("rank") instead of "km".
    ''' </summary>
    ''' <param name="timeTransform">
    '''   Time transform to use: "rank", "log", or "identity".
    '''   Default = "rank".
    ''' </param>
    ''' <returns>PhScoreTestResult with per-variable and global tests.</returns>
    Private Function ComputePHScoreTest(Optional timeTransform As String = "rank") As List(Of TestResult)

        If Me.pScaledSchoenfeldResiduals Is Nothing Then Me.pScaledSchoenfeldResiduals = Me.ComputeScaledSchoenfeld()
        Dim events = pRecords.Where(Function(rr) rr.Censorship = 1).ToList()
        Dim p As Integer = pCoefficients.Length
        Dim m As Integer = events.Count

        ' Extract event times and marked scaled residuals
        Dim times(m - 1) As Double
        Dim r(m - 1, p - 1) As Double

        For i = 0 To m - 1
            times(i) = events(i).Time
            Dim v = Me.pScaledSchoenfeldResiduals(events(i).Index)   ' scaled Schoenfeld residual
            For k = 0 To p - 1
                r(i, k) = v(k)
            Next
        Next

        ' === Apply R's time transform ===
        Dim xt(m - 1) As Double
        Select Case timeTransform.ToLower()
            Case "identity"
                For i = 0 To m - 1
                    xt(i) = times(i)
                Next
            Case "rank"
                Dim ord = times.Select(Function(t, i) New With {.t = t, .i = i}).OrderBy(Function(x) x.t).ToList()
                For rank = 0 To m - 1
                    xt(ord(rank).i) = rank + 1
                Next
            Case "log"
                For i = 0 To m - 1
                    xt(i) = Math.Log(times(i))
                Next
            Case Else
                AppGlobals.BSerr.LogAndThrow(New ArgumentException("Unknown transform"))
        End Select

        ' === Center x (R centers the transformed times) ===
        Dim xbar As Double = xt.Average()

        ' === Compute test statistic for each covariate ===
        Dim entries = New List(Of TestResult)
        Dim globalChiSq As Double = 0
        For k = 0 To p - 1
            Dim num As Double = 0
            Dim den As Double = 0

            For i = 0 To m - 1
                Dim xc = xt(i) - xbar
                num += xc * r(i, k)
                den += xc * xc
            Next

            Dim chi = (num * num) / (den * Me.pVarCov(k, k) * m)
            Dim t = New TestResult
            t.TestStatistics1 = chi
            t.Pvalue = 1.0 - distributions.ChiSquareCDF(chi, 1.0)
            t.DF1 = 1.0
            t.strSpecialInformation = pVarNames(k)
            entries.Add(t)

            globalChiSq += chi
        Next

        Dim gt = New TestResult
        gt.TestStatistics1 = globalChiSq
        gt.Pvalue = 1.0 - distributions.ChiSquareCDF(globalChiSq, p)
        gt.DF1 = p
        gt.strSpecialInformation = "Global test"
        entries.Add(gt)
        Return entries
    End Function




    ''' <summary>
    ''' Holds baseline cumulative hazard and survival at a given time point.
    ''' </summary>
    Public Structure BaselinePoint
        ''' <summary>Event time.</summary>
        Public Time As Double
        ''' <summary>Cumulative baseline hazard H₀(t) up to this time.</summary>
        Public CumHazard As Double
        ''' <summary>Baseline survival S₀(t) = exp(−H₀(t)) at this time.</summary>
        Public Survival As Double
    End Structure

    ''' <summary>
    ''' Computes the baseline cumulative hazard H₀(t) and baseline survival
    ''' S₀(t) for each stratum, using the fitted coefficients and the chosen
    ''' tie-handling method.
    '''
    ''' <para>
    ''' The baseline hazard is estimated from the fitted Cox model via:
    ''' </para>
    ''' <para>
    '''   H₀(t) = Σ_k ΔH₀(t_k),  where t_k are distinct event times.
    ''' </para>
    '''
    ''' <para>
    ''' For Breslow ties, at each event time t with d events:
    ''' </para>
    ''' <code>
    '''   ΔH₀(t) = d / Σ_{i∈R(t)} exp(η_i)
    ''' </code>
    '''
    ''' <para>
    ''' For Efron ties, we use:
    ''' </para>
    ''' <code>
    '''   ΔH₀(t) = Σ_{l=0}^{d−1} 1 /
    '''                [ Σ_{i∈R(t)} exp(η_i) − (l/d) Σ_{events at t} exp(η_i) ]
    ''' </code>
    '''
    ''' <para>
    ''' For Exact ties, the baseline hazard is approximated by the Breslow
    ''' increment formula. This is also a common choice in software where the
    ''' exact likelihood is used for β, but the baseline hazard is derived
    ''' using the Breslow-style increment.
    ''' </para>
    '''
    ''' <para>
    ''' The result is returned as a dictionary:
    ''' </para>
    ''' <list type="bullet">
    '''   <item>
    '''     <description>
    '''     Key: stratum identifier (the value of <c>Stratum</c> in the data).
    '''     </description>
    '''   </item>
    '''   <item>
    '''     <description>
    '''     Value: ordered list of <see cref="BaselinePoint"/> containing
    '''     event time, cumulative hazard H₀(t), and baseline survival S₀(t).
    '''     </description>
    '''   </item>
    ''' </list>
    '''
    ''' <para>
    ''' This method assumes the model has already been fitted and that
    ''' <c>pCoefficients</c> contains the final β estimates. Provided output is at βs = 0
    ''' </para>
    ''' </summary>
    ''' <returns>
    ''' Dictionary mapping each stratum to a time-ordered list of baseline
    ''' cumulative hazard and survival values.
    ''' </returns>
    Public Function ComputeBaseline(Optional bZeroBetas As Boolean = True) As Dictionary(Of Object, Double(,))
        Dim result As New Dictionary(Of Object, Double(,))()

        Dim beta(Me.pCoefficients.Length - 1) As Double 'estimate at betas equal zero
        If Not bZeroBetas Then beta = Me.pCoefficients

        Dim strataGroups = Me.pRecords.GroupBy(Function(r) r.Stratum).ToList()
        Dim maxTimeByStratum = Me.pRecords.GroupBy(Function(r) r.Stratum).
                                           ToDictionary(Function(g) g.Key,
                                                        Function(g) g.Max(Function(r) r.Time))

        For Each sg In strataGroups

            Dim stratumId = sg.Key
            Dim group = sg.OrderBy(Function(r) r.Time).ToList()
            Dim n = group.Count

            ' Precompute exp(η)
            Dim exb(n - 1) As Double
            For i = 0 To n - 1
                exb(i) = Math.Exp(Matrix.DotProduct(group(i).Covariates, beta))
            Next

            ' Collect event times
            Dim eventsByTime = group.Where(Function(r) r.Censorship = 1).
                                     GroupBy(Function(r) r.Time).
                                     OrderBy(Function(g) g.Key).ToList()

            Dim baseline As New List(Of BaselinePoint)()
            Dim cumHaz As Double = 0.0

            For Each evGroup In eventsByTime

                Dim t = evGroup.Key
                Dim events = evGroup.ToList()
                Dim d = events.Count

                ' Correct R-style risk set: everyone with Time >= t
                Dim riskIdx = group.Select(Function(r, idx) New With {.Rec = r, .Idx = idx}).
                                    Where(Function(x) x.Rec.Time >= t).
                                    Select(Function(x) x.Idx).ToList()

                ' Sum exp(η) over risk set
                Dim sumRisk As Double = 0.0
                For Each idx In riskIdx
                    sumRisk += exb(idx)
                Next

                Dim dH As Double = 0.0

                If Me.pMethod = TieMethod.Efron Then

                    Dim sumExpEvents As Double = 0.0
                    For Each ev In events
                        Dim idx = group.IndexOf(ev)
                        sumExpEvents += exb(idx)
                    Next

                    For l = 0 To d - 1
                        dH += 1.0 / (sumRisk - (l / CDbl(d)) * sumExpEvents)
                    Next

                Else
                    ' Breslow increment used by R for both Breslow AND Exact fits
                    dH = d / sumRisk
                End If

                cumHaz += dH
                Dim surv = Math.Exp(-cumHaz)

                baseline.Add(New BaselinePoint With {.Time = t, .CumHazard = cumHaz, .Survival = surv})
            Next
            'add point for the very last time if not already present
            If baseline(baseline.Count - 1).Time <> maxTimeByStratum(stratumId) Then
                Dim bp = New BaselinePoint With {.Time = maxTimeByStratum(stratumId),
                                                 .CumHazard = baseline(baseline.Count - 1).CumHazard,
                                                 .Survival = baseline(baseline.Count - 1).Survival}
                baseline.Add(bp)
            End If


            Dim xx(baseline.Count - 1, 2) As Double
            For i = 0 To baseline.Count - 1
                xx(i, 0) = baseline(i).Time 'time
                xx(i, 1) = baseline(i).Survival 'survival
                xx(i, 2) = baseline(i).CumHazard 'hazard
            Next

            result(stratumId) = xx
        Next

        Return result
    End Function

    ''' <summary>
    ''' Input is a 2D array of survival time, porbability, and hazard.
    ''' </summary>
    ''' <returns>
    ''' Array of survival time, porbability, and hazard formated for step plot output.
    ''' </returns>
    Public Function BaseSurvivalForPloting(inX(,) As Double) As Double(,)
        Dim n As Integer = UBound(inX, 1)
        Dim inX_(n + 1, 2)
        inX_(0, 1) = 1 'Probability at time 0
        For i = 0 To n
            For j = 0 To UBound(inX, 2)
                inX_(i + 1, j) = inX(i, j)
            Next
        Next
        n += 1
        Dim out(0 To 2 * n, 2) As Double
        out(0, 1) = 1 'Probability at time 0 to create 1st step
        For i = 1 To n
            'Probability
            out(i * 2 - 1, 1) = inX_(i - 1, 1)
            out(i * 2, 1) = inX_(i, 1)
            'Time
            out(i * 2 - 1, 0) = inX_(i, 0)
            out(i * 2, 0) = inX_(i, 0)
            'Hazard
            out(i * 2 - 1, 2) = inX_(i - 1, 2)
            out(i * 2, 2) = inX_(i, 2)
        Next
        Return out
    End Function

    Public Sub PlotCox(ws As Worksheet, SurvTime() As Double, SurvProb() As Double, Optional lTop As Long = 100, Optional lLeft As Long = 100)

        'subrutine add Adjusted survival and Cumulative hazards plots
        'inputs:
        '   SurvTime()      - array of distinct survival times
        '   SurvProb()      - corresponding array of survival probabilities
        '   lTop            - top coordiante of the plot
        '   lLeft           - left coordinate of the plot

        'compute optimal scaling
        Dim udPlotAxisX = graphics.ChartScaling(0, SurvTime.Max())
        Dim Ymax As Double = 1.0
        Dim MJunit As Double = 0.2

        With ws.Shapes.AddChart(Left:=lLeft, Top:=lTop)
            With .Chart
                .ChartType = XlChartType.xlXYScatterLinesNoMarkers

                'delete extra series
                Do Until .SeriesCollection.Count = 0
                    .SeriesCollection(1).Delete
                Loop

                With .Axes(XlAxisType.xlValue)
                    .MinimumScale = 0
                    .MaximumScale = Ymax
                    .MajorUnit = MJunit
                    .MajorGridlines.Delete
                End With
                .Axes(XlAxisType.xlCategory).MinimumScale = 0
                .Axes(XlAxisType.xlCategory).MaximumScale = udPlotAxisX.Max

                .SeriesCollection.NewSeries
                With .SeriesCollection(1)
                    .Name = "Baseline"
                    .XValues = SurvTime
                    .Values = SurvProb
                    .Border.Color = RGB(155, 0, 0)
                    With .Format.Line
                        .Visible = True
                        .ForeColor.TintAndShade = 0
                        .ForeColor.Brightness = 0
                    End With
                End With

                '.Legend.Delete()

                Try 'add title and axis labels
                    .HasTitle = False
                    .HasTitle = True
                    .ChartTitle.Text = "Cox - Survival plot"
                    .Axes(XlAxisType.xlValue, XlAxisGroup.xlPrimary).HasTitle = False
                    .Axes(XlAxisType.xlValue, XlAxisGroup.xlPrimary).HasTitle = True
                    .Axes(XlAxisType.xlValue, XlAxisGroup.xlPrimary).AxisTitle.text = "Survival Probability"
                    .Axes(XlAxisType.xlCategory, XlAxisGroup.xlPrimary).HasTitle = False
                    .Axes(XlAxisType.xlCategory, XlAxisGroup.xlPrimary).HasTitle = True
                    .Axes(XlAxisType.xlCategory, XlAxisGroup.xlPrimary).AxisTitle.text = "Time"
                Catch
                End Try
            End With
        End With
    End Sub
End Class
