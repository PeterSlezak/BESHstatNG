Option Strict On
Option Explicit On

Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports Microsoft.Office.Interop.Excel

Namespace regression


    '==========================================================
    '  GENERAL LINEAR MODEL (Gaussian/Identity) - WLS/OLS
    '==========================================================
    ''' <summary>
    ''' Specifies the sum-of-squares convention used for term-wise ANOVA tables.
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' This enumeration controls how the contribution of each <em>term</em> (a group of one or more design-matrix columns)
    ''' is tested in the presence of other terms.
    ''' </para>
    ''' <list type="bullet">
    '''   <item>
    '''     <description>
    '''       <see cref="TermSumOfSquaresType.TypeI"/>: sequential (a.k.a. "Type I") sums of squares. Each term is added
    '''       in the provided order and tested by the reduction in residual sum of squares (RSS/SSE) relative to the previous model.
    '''     </description>
    '''   </item>
    '''   <item>
    '''     <description>
    '''       <see cref="TermSumOfSquaresType.TypeIII"/>: partial (a.k.a. "Type III") sums of squares. Each term is tested
    '''       by comparing the full model to a reduced model with that term's columns removed.
    '''     </description>
    '''   </item>
    ''' </list>
    ''' <para>
    ''' The definition of a "term" is provided by <c>customTermGroups</c> in <see cref="LinearModel.Fit"/>; 
    ''' for example, a multi-level factor or  an interaction may span multiple columns. Interaction columns must be precomputed by 
    ''' the caller and included in the design matrix.
    ''' </para>
    ''' </remarks>

    Public Enum TermSumOfSquaresType
        TypeI = 1
        TypeIII = 3
    End Enum

    ''' <summary>
    ''' Fits a (weighted) Gaussian linear regression model using ordinary/weighted least squares (OLS/WLS) and produces
    ''' commonly used diagnostics and tabular outputs.
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' Model form (with optional intercept):
    ''' </para>
    ''' <para>
    ''' <c>y = X β + ε</c>,
    ''' where <c>y</c> is an <c>n×1</c> response vector, <c>X</c> is an <c>n×p</c> design matrix, <c>β</c> is a <c>p×1</c> coefficient vector,
    ''' and <c>ε</c> are errors.
    ''' </para>
    ''' <para>
    ''' Estimation is performed by minimizing the weighted sum of squared residuals
    ''' </para>
    ''' <para>
    ''' <c>SSE = Σ_i w_i (y_i − x_i' β)^2</c>,
    ''' </para>
    ''' <para>
    ''' which yields the normal-equation solution:
    ''' </para>
    ''' <para>
    ''' <c>β̂ = (X' W X)^{-1} X' W y</c>,
    ''' where <c>W</c> is diagonal with entries <c>w_i</c>.
    ''' </para>
    ''' <para>
    ''' Implementation details:
    ''' </para>
    ''' <list type="bullet">
    '''   <item><description>
    '''     Coefficients are estimated using <see cref="Matrix.MinimalWLS"/>, which applies the
    '''     usual WLS transformation and solves the transformed OLS problem.
    '''   </description></item>
    '''   <item><description>
    '''     The (scaled) coefficient covariance is computed as <c>Var(β̂) = MSE · (X'WX)^{-1}</c> with
    '''     <c>MSE = SSE / (n − p)</c>. The matrix inverse is obtained via <see cref="Matrix.MatInv"/> ("CHOL").
    '''   </description></item>
    '''   <item><description>
    '''     T-based inference (p-values and confidence intervals) uses the Student t distribution functions in
    '''     <see cref="Distributions"/> (e.g., <see cref="Distributions.T_CDF"/> and <see cref="Distributions.T_Inv"/>).
    '''     Overall model F-tests use <see cref="Distributions.F_CDF"/>.
    '''   </description></item>
    '''   <item><description>
    '''     Tabular outputs are returned as <see cref="ResultTable"/> instances for consistent printing/export within the project.
    '''   </description></item>
    ''' </list>
    ''' <para>
    ''' AIC/BIC: this implementation computes the Gaussian log-likelihood with the ML variance estimator
    ''' <c>σ̂² = SSE / n</c> and then uses:
    ''' </para>
    ''' <para>
    ''' <c>logLik = −(n/2) [ log(2πσ̂²) + 1 ]</c>,
    ''' <c>AIC = −2·logLik + 2p</c>,
    ''' <c>BIC = −2·logLik + log(n)·p</c>.
    ''' </para>
    ''' <para>
    ''' Some software (e.g., SAS PROC REG) reports <c>AIC* = n·log(SSE/n) + 2p</c>, which differs by the constant
    ''' <c>n·(log(2π)+1)</c>; model comparisons on the same data are unaffected by this constant offset.
    ''' For weighted fits, the likelihood is a Gaussian working likelihood using the weighted SSE; comparisons are most meaningful
    ''' when the same weights are used across the compared models.
    ''' </para>
    ''' </remarks>
    ''' <seealso cref="LMresult"/>
    ''' <seealso cref="LMresult.getModelDiagnasticTable_toPrint"/>
    ''' <seealso cref="LMresult.CoeffsT_toPrint"/>
    ''' <seealso cref="ResultTable"/>
    Public Class LinearModel

        '' <summary>When <c>True</c>, computes residual diagnostics (leverage, standardized residuals, Cook's distance, jackknife/PRESS residuals).</summary>
        Public bComputeResiduals As Boolean = True

        ''' <summary>When <c>True</c>, computes and stores the coefficient covariance matrix <c>Var(β̂)</c>.</summary>
        Public bReturnCov As Boolean = True

        ''' <summary>Significance level used for coefficient confidence intervals and hypothesis tests (default 0.05).</summary>
        ''' <remarks>
        ''' Used for two-sided t-based confidence intervals: <c>β̂ ± t_{1−α/2, df}·SE</c>.
        ''' See <see cref="Distributions.T_Inv"/>.
        ''' </remarks>
        Public Alpha As Double = 0.05

        'Primary results container used across your project (GLM/LM/etc.)
        ''' <summary>
        ''' Holds the fitted model results (coefficients, standard errors, test statistics, and model summary metrics).
        ''' </summary>
        ''' <remarks>
        ''' The <see cref="LMresult"/> object is populated by <see cref="Fit"/> and is used to render coefficient and model diagnostic tables
        ''' via <see cref="LMresult.CoeffsT_toPrint"/> and <see cref="LMresult.getModelDiagnasticTable_toPrint"/>.
        ''' </remarks>
        Public results As LMresult

        'Tabular outputs (ResultTable)
        Private pAnovaOverall As ResultTable
        Private pAnovaTypeI As ResultTable
        Private pAnovaTypeIII As ResultTable
        Private pVIF As ResultTable

        'Raw vectors/matrices (kept for downstream usage)
        Private pFitted() As Double
        Private pResiduals() As Double
        Private pLeverage() As Double
        Private pStdResidual() As Double
        Private pJackknifeResidual() As Double
        Private pCooksD() As Double
        Private pCovariance(,) As Double

        'Model scalars
        Private pSSE As Double
        Private pSST As Double
        Private pSSR As Double
        Private pMSE As Double
        Private pRMSE As Double
        Private pRSquared As Double
        Private pAdjRSquared As Double
        Private pLogLik As Double
        Private pAIC As Double
        Private pBIC As Double
        Private pDFModel As Integer
        Private pDFResid As Integer
        Private pDFTotal As Integer
        Private pIncludeIntercept As Boolean

        'Input storage
        Private pData(,) As Double
        Private pVarNames() As String
        Private pWeights() As Double
        Private pRowNums() As Integer

        'Working arrays
        Private n As Integer
        Private p As Integer
        Private y() As Double
        Private X(,) As Double
        Private w() As Double

        'Key = term name, Value = X-column indices (0..p-1) after intercept handling
        Private termGroups As Dictionary(Of String, Integer())

        '==========================
        ' Input
        '==========================
        ''' <summary>
        ''' Loads the response and predictor data (and optional weights/labels) into the model object.
        ''' </summary>
        ''' <param name="dataMatrix">
        ''' A rectangular <c>n×(k+1)</c> matrix where column 0 is the response <c>y</c> and columns 1..k are predictors.
        ''' Interaction terms must be computed by the caller and included as additional predictor columns.
        ''' </param>
        ''' <param name="varNames">
        ''' Optional column names for <paramref name="dataMatrix"/>. When provided, this array must have the same length as the
        ''' number of columns in <paramref name="dataMatrix"/> and must include the response name at index 0.
        ''' The response name is discarded internally; only predictor names (columns 1..k) are stored.
        ''' </param>
        ''' <param name="RowNums">
        ''' Optional row identifiers (length <c>n</c>). When omitted, row indices 0..n-1 are used.
        ''' These are surfaced in diagnostic tables to help map results back to the original dataset.
        ''' </param>
        ''' <param name="weights">
        ''' Optional WLS weights (length <c>n</c>). If omitted, all weights are 1 (OLS).
        ''' Weights must be finite and strictly positive. The fitted coefficients minimize
        ''' <c>Σ w_i (y_i − x_i'β)^2</c>.
        ''' </param>
        ''' <remarks>
        ''' <para>
        ''' This method stores the provided inputs; estimation is performed by <see cref="Fit"/>.
        ''' </para>
        ''' <seealso cref="Fit"/>
        ''' <seealso cref="BESHstatGlobals.BSerr.LogAndThrow(Exception,Boolean,Boolean)"/>
        ''' <seealso cref="Matrix.IdentityVect(Integer, Double)"/>
        ''' </remarks>
        Public Sub Data(dataMatrix(,) As Double,
                    Optional varNames() As String = Nothing,
                    Optional RowNums() As Integer = Nothing,
                    Optional weights() As Double = Nothing)

            If dataMatrix Is Nothing Then BESHStatNG.BSerr.LogAndThrow(New ArgumentNullException(NameOf(dataMatrix)))
            Me.pData = dataMatrix

            Me.n = UBound(pData, 1) + 1
            If n <= 1 Then BESHStatNG.BSerr.LogAndThrow(New ArgumentException("Data matrix must have at least 2 rows."))

            '--- varNames incoming includes Y at index 0; store predictors only (drop index 0) ---
            If varNames IsNot Nothing Then
                Dim expectedCols As Integer = UBound(pData, 2) + 1
                If varNames.Length <> expectedCols Then
                    BESHStatNG.BSerr.LogAndThrow(New ArgumentException("varNames length must match number of columns in dataMatrix (including Y at index 0)."))
                End If

                Dim pPredictors As Integer = expectedCols - 1
                If pPredictors = 0 Then 'Intercept-only model: no predictor names
                    Me.pVarNames = New String() {}
                Else
                    ReDim Me.pVarNames(pPredictors - 1)
                    Array.Copy(varNames, 1, Me.pVarNames, 0, pPredictors) 'skip Y name
                End If

            Else
                Me.pVarNames = Nothing
            End If

            If RowNums Is Nothing Then
                ReDim pRowNums(n - 1)
                For i As Integer = 0 To n - 1
                    pRowNums(i) = i
                Next
            Else
                If RowNums.Length <> n Then BESHStatNG.BSerr.LogAndThrow(New ArgumentException("RowNums length must match #rows."))
                Me.pRowNums = CType(RowNums.Clone(), Integer())
            End If

            If weights Is Nothing Then
                ' IdentityVect expects last index, so n-1 gives length n
                Me.pWeights = Matrix.IdentityVect(n - 1, 1.0)
            Else
                If weights.Length <> n Then BESHStatNG.BSerr.LogAndThrow(New ArgumentException("weights length must match #rows."))
                Me.pWeights = CType(weights.Clone(), Double())
            End If
        End Sub

        '==========================
        ' Read-only accessors
        '==========================
        ''' <summary>Gets the fitted values ŷ for each observation after <see cref="Fit"/>.</summary>
        ''' <remarks>Computed as <c>ŷ = X β̂</c>.</remarks>
        Public ReadOnly Property Fitted() As Double()
            Get
                Return pFitted
            End Get
        End Property

        ''' <summary>Gets the raw residuals <c>e = y − ŷ</c> after <see cref="Fit"/>.</summary>
        Public ReadOnly Property Residuals() As Double()
            Get
                Return pResiduals
            End Get
        End Property

        ''' <summary>
        ''' Gets the estimated coefficient covariance matrix <c>Var(β̂)</c>.
        ''' </summary>
        ''' <remarks>
        ''' Computed as <c>MSE · (X'WX)^{-1}</c>, where <c>MSE = SSE/(n−p)</c>.
        ''' See also <see cref="Matrix.MatInv"/>.
        ''' </remarks>
        Public ReadOnly Property Covariance() As Double(,)
            Get
                Return pCovariance
            End Get
        End Property

        ''' <summary>Gets the overall ANOVA table (Model/Residuals/Total) as a <see cref="ResultTable"/>.</summary>
        ''' <remarks>
        ''' The overall F-test uses <see cref="Distributions.F_CDF"/>. For intercept-only models, df_model = 0 and F is undefined.
        ''' </remarks>
        Public ReadOnly Property AnovaOverall_toPrint() As ResultTable
            Get
                Return pAnovaOverall
            End Get
        End Property

        ''' <summary>Gets the Type I (sequential) term-wise ANOVA table as a <see cref="ResultTable"/>.</summary>
        ''' <remarks>
        ''' Requires term definitions supplied to <see cref="Fit"/> via <c>customTermGroups</c>, or defaults to one term per predictor column.
        ''' </remarks>
        Public ReadOnly Property AnovaTypeI_toPrint() As ResultTable
            Get
                Return pAnovaTypeI
            End Get
        End Property

        ''' <summary>Gets the Type III (partial) term-wise ANOVA table as a <see cref="ResultTable"/>.</summary>
        ''' <remarks>
        ''' Type III tests each term by comparing the full model to a reduced model with that term removed.
        ''' </remarks>
        Public ReadOnly Property AnovaTypeIII_toPrint() As ResultTable
            Get
                Return pAnovaTypeIII
            End Get
        End Property

        ''' <summary>Gets the variance inflation factor (VIF) table as a <see cref="ResultTable"/>.</summary>
        ''' <remarks>
        ''' VIFs are computed from the diagonal of the inverse weighted correlation matrix among predictors (excluding intercept).
        ''' </remarks>
        Public ReadOnly Property VIF_toPrint() As ResultTable
            Get
                Return pVIF
            End Get
        End Property

        ''' <summary>
        ''' Gets a combined residual diagnostics table suitable for printing/export (raw residuals, leverage, standardized residuals,
        ''' Cook's distance, and jackknife residuals).
        ''' </summary>
        ''' <returns>
        ''' A 2D object array as returned by <see cref="ResultTable.returnSelf"/>, containing headers and data.
        ''' </returns>
        ''' <remarks>
        ''' This table is built using <see cref="ResultTable"/> formatting helpers such as
        ''' <see cref="ResultTable.SetBody"/>, <see cref="ResultTable.AddHeaderTopRow"/>, and
        ''' <see cref="ResultTable.AddHeaderLeftRow"/>.
        ''' </remarks>
        Public ReadOnly Property AllResiduals_toPrint() As Object(,)
            Get
                If pResiduals Is Nothing Then Return Nothing
                Dim t As New ResultTable
                Dim body(n - 1, 5) As Object
                For i As Integer = 0 To n - 1
                    body(i, 0) = pFitted(i)
                    body(i, 1) = pResiduals(i)
                    body(i, 2) = If(pLeverage Is Nothing, Nothing, pLeverage(i))
                    body(i, 3) = If(pStdResidual Is Nothing, Nothing, pStdResidual(i))
                    body(i, 4) = If(pCooksD Is Nothing, Nothing, pCooksD(i))
                    body(i, 5) = If(pJackknifeResidual Is Nothing, Nothing, pJackknifeResidual(i))
                Next
                t.SetBody(body)
                t.AddHeaderTopRow({"Fitted", "Residual", "Leverage", "Std. Residual", "Cook's D", "Jackknife Residual"})
                Return t.returnSelf()
            End Get
        End Property

        ''' <summary>
        ''' Returns a set of standard output tables for the fitted model (coefficients, model diagnostics, ANOVA, VIF, and residual diagnostics).
        ''' </summary>
        ''' <returns>A list of <see cref="ResultTable"/> objects, in a presentation-friendly order.</returns>
        ''' <remarks>
        ''' This method mirrors the project pattern used by the GLM implementation: it returns preformatted tables intended for UI/reporting.
        ''' Coefficient and model-diagnostic tables are provided by <see cref="LMresult"/>.
        ''' <seealso cref="ProcessListofResultTables.writeToSheet"/>
        ''' <seealso cref="WriteResults"/>
        ''' </remarks>
        Public Function wrapResults() As List(Of ResultTable)

            If results Is Nothing Then BESHStatNG.BSerr.LogAndThrow(New InvalidOperationException("Model is not fitted."))

            Dim out As New List(Of ResultTable)

            Dim tCoef As ResultTable = results.CoeffsT_toPrint
            tCoef.AddPvalueToFormat(4)
            out.Add(tCoef)

            out.Add(results.getModelDiagnasticTable_toPrint())

            If pAnovaOverall IsNot Nothing Then out.Add(pAnovaOverall)

            If pAnovaTypeI IsNot Nothing Then out.Add(pAnovaTypeI)
            If pAnovaTypeIII IsNot Nothing Then out.Add(pAnovaTypeIII)

            out.Add(pVIF)

            'Return covariance
            If Me.bReturnCov Then
                Dim t As New ResultTable
                t.SetBody(Me.pCovariance)
                Dim vars = Me.pVarNames
                If Me.pIncludeIntercept Then vars = ConcatArrays({"Intercept"}, Me.pVarNames)

                Dim h(vars.Length) As String
                h(0) = "Covariance Matrix of Parameters"
                t.AddHeaderTopRow(h)
                t.AddHeaderTopRow(vars)
                t.AddHeaderLeftRow(vars)
                out.Add(t)
            End If

            Return out
        End Function

        ''' <summary>
        ''' Fits the linear model using OLS/WLS and computes coefficients, inference, diagnostics, ANOVA tables, and VIFs.
        ''' </summary>
        ''' <param name="includeIntercept">
        ''' When <c>True</c>, an intercept column of ones is prepended to the design matrix.
        ''' </param>
        ''' <param name="customTermGroups">
        ''' Optional mapping from term name to design-matrix column indices (0-based indices in the <em>final</em> <c>X</c> used for fitting,
        ''' i.e., after adding the intercept when <paramref name="includeIntercept"/> is <c>True</c>).
        ''' This mapping controls term-wise ANOVA tables (Type I/III).
        ''' If omitted, each predictor column is treated as its own term (and the intercept, if present, is treated separately).
        ''' </param>
        ''' <param name="computeTermAnova">
        ''' Controls whether and which term-wise ANOVA table to compute.
        ''' Set to <see cref="TermSumOfSquaresType.TypeIII"/> (default) or <see cref="TermSumOfSquaresType.TypeI"/>.
        ''' If you do not need term-wise ANOVA, you may skip it by not reading the corresponding output properties.
        ''' </param>
        ''' <remarks>
        ''' <para><b>Estimation</b></para>
        ''' <para>
        ''' The fitted coefficients solve:
        ''' <c>β̂ = argmin_β Σ_i w_i (y_i − x_i'β)^2</c>.
        ''' This is computed by <see cref="Matrix.MinimalWLS"/>.
        ''' </para>
        ''' <para><b>Sums of squares</b></para>
        ''' <para>
        ''' Residual (error) sum of squares:
        ''' <c>SSE = Σ_i w_i e_i^2</c>, where <c>e_i = y_i − ŷ_i</c>.
        ''' </para>
        ''' <para>
        ''' Total sum of squares:
        ''' if an intercept is included, <c>SST = Σ_i w_i (y_i − ȳ_w)^2</c> where <c>ȳ_w</c> is the weighted mean;
        ''' otherwise, the uncentered definition is used: <c>SST = Σ_i w_i y_i^2</c>.
        ''' </para>
        ''' <para>
        ''' Regression sum of squares: <c>SSR = SST − SSE</c>.
        ''' </para>
        ''' <para><b>Overall ANOVA / F test</b></para>
        ''' <para>
        ''' Degrees of freedom: <c>df_resid = n − p</c>, <c>df_model = p−1</c> when an intercept is present (else <c>df_model = p</c>).
        ''' Mean squares: <c>MSE = SSE/df_resid</c>, <c>MSR = SSR/df_model</c>.
        ''' Overall F statistic: <c>F = MSR/MSE</c> with p-value <c>1 − F_CDF(F; df_model, df_resid)</c>.
        ''' For intercept-only models, <c>df_model = 0</c> and the F-test is not defined (reported as NaN/blank).
        ''' </para>
        ''' <para><b>Coefficient inference</b></para>
        ''' <para>
        ''' <c>Var(β̂) = MSE · (X'WX)^{-1}</c>. Standard errors are <c>SE_j = sqrt(Var(β̂)_{jj})</c>.
        ''' T-statistics: <c>t_j = β̂_j / SE_j</c>.
        ''' Two-sided p-values: <c>p_j = 2·(1 − T_CDF(|t_j|; df_resid))</c>.
        ''' Confidence intervals: <c>β̂_j ± t_{1−α/2, df_resid}·SE_j</c>.
        ''' See <see cref="Distributions.T_CDF"/> and <see cref="Distributions.T_Inv"/>.
        ''' </para>
        ''' <para><b>Diagnostics</b></para>
        ''' <para>
        ''' Leverage (hat diagonal) for WLS:
        ''' <c>h_ii = w_i · x_i' (X'WX)^{-1} x_i</c>.
        ''' Standardized residuals:
        ''' <c>r_i = sqrt(w_i)·e_i / sqrt(MSE·(1 − h_ii))</c>.
        ''' Jackknife (deleted/PRESS) residuals:
        ''' <c>e_(i) = e_i / (1 − h_ii)</c>.
        ''' Cook's distance (WLS analogue as implemented):
        ''' <c>D_i = (w_i e_i^2 / (p·MSE)) · (h_ii / (1−h_ii)^2)</c>.
        ''' </para>
        ''' <para><b>Information criteria</b></para>
        ''' <para>
        ''' Gaussian log-likelihood with ML variance <c>σ̂² = SSE/n</c>:
        ''' <c>logLik = −(n/2)[log(2πσ̂²)+1]</c>.
        ''' Then <c>AIC = −2·logLik + 2p</c> and <c>BIC = −2·logLik + log(n)·p</c>.
        ''' Some software reports <c>n·log(SSE/n)+2p</c>, which differs only by an additive constant.
        ''' </para>
        ''' <para><b>VIF</b></para>
        ''' <para>
        ''' Variance inflation factors are computed from the inverse of the weighted predictor correlation matrix (excluding the intercept):
        ''' <c>VIF_j = (R^{-1})_{jj}</c>. The inversion uses <see cref="Matrix.MatInv"/>.
        ''' If the correlation matrix is singular or not positive definite, VIFs are reported as <c>+∞</c>.
        ''' </para>
        ''' </remarks>
        ''' <seealso cref="Data"/>
        ''' <seealso cref="AnovaOverall_toPrint"/>
        ''' <seealso cref="AnovaTypeI_toPrint"/>
        ''' <seealso cref="AnovaTypeIII_toPrint"/>
        ''' <seealso cref="VIF_toPrint"/>
        ''' <seealso cref="AllResiduals_toPrint"/>
        Public Sub Fit(Optional includeIntercept As Boolean = True,
                   Optional customTermGroups As Dictionary(Of String, Integer()) = Nothing,
                   Optional computeTermAnova As TermSumOfSquaresType = TermSumOfSquaresType.TypeIII)

            If pData Is Nothing Then BESHStatNG.BSerr.LogAndThrow(New InvalidOperationException("Call Data(...) first."))

            Dim lastCol As Integer = UBound(pData, 2)  '0 means only Y column
            If lastCol < 0 Then
                BESHStatNG.BSerr.LogAndThrow(New ArgumentException("Data matrix must contain at least one column (Y)."))
            End If
            Dim pPredictors As Integer = lastCol '0.. => number of predictor columns (since col0 is Y)
            Me.pIncludeIntercept = includeIntercept

            'y
            ReDim y(n - 1)
            For i As Integer = 0 To n - 1
                y(i) = pData(i, 0)
            Next

            'weights
            w = CType(pWeights.Clone(), Double())
            For i As Integer = 0 To n - 1
                If Double.IsNaN(w(i)) OrElse Double.IsInfinity(w(i)) OrElse w(i) <= 0 Then
                    BESHStatNG.BSerr.LogAndThrow(New ArgumentException($"Invalid weight at row {i}: {w(i)}. Weights must be finite and > 0."))
                End If
            Next

            'X
            Me.p = pPredictors + If(includeIntercept, 1, 0)
            ReDim X(n - 1, p - 1)

            Dim colOffset As Integer = 0
            If includeIntercept Then
                For i As Integer = 0 To n - 1 : X(i, 0) = 1.0 : Next
                colOffset = 1
            End If

            For j As Integer = 0 To pPredictors - 1
                For i As Integer = 0 To n - 1
                    X(i, j + colOffset) = pData(i, j + 1)
                Next
            Next

            'term groups (used only for term-wise ANOVA)
            termGroups = BuildDefaultTermGroups(includeIntercept, pPredictors, customTermGroups)

            '=== Fit via MinimalWLS ===
            Dim params As Double(,) = Matrix.MinimalWLS(y, X, w)

            Dim beta(p - 1) As Double
            Dim seFromMinimal(p - 1) As Double
            For j As Integer = 0 To p - 1
                beta(j) = params(j, 0)
                seFromMinimal(j) = params(j, 1)
            Next

            'Predicted / residuals
            Dim yhat() As Double = Predict1D(X, beta)
            Dim resid() As Double = Matrix.M_SUB(y, yhat)
            Me.pFitted = yhat
            Me.pResiduals = resid

            'SSE
            Dim sse As Double = 0.0
            For i As Integer = 0 To n - 1
                sse += w(i) * resid(i) * resid(i)
            Next

            'SST (centered if intercept present; uncentered if no intercept)
            Dim sst As Double
            If includeIntercept Then
                Dim ybarW As Double = WeightedMean(y, w)
                sst = 0.0
                For i As Integer = 0 To n - 1
                    Dim dy As Double = y(i) - ybarW
                    sst += w(i) * dy * dy
                Next
            Else
                'uncentered total SS
                sst = 0.0
                For i As Integer = 0 To n - 1
                    sst += w(i) * y(i) * y(i)
                Next
            End If

            Dim ssr As Double = sst - sse
            Dim dfResid As Integer = n - p
            If dfResid <= 0 Then BESHStatNG.BSerr.LogAndThrow(New InvalidOperationException("Insufficient degrees of freedom: n - p <= 0."))
            Dim dfModel As Integer = If(includeIntercept, p - 1, p)
            Dim dfTotal As Integer = If(includeIntercept, n - 1, n) 'common convention for uncentered total

            Dim mse As Double = sse / dfResid
            Dim rmse As Double = Math.Sqrt(mse)

            Dim r2 As Double = If(sst > 0, 1.0 - (sse / sst), 0.0)
            Dim adjR2 As Double = If(dfTotal > 0, 1.0 - (1.0 - r2) * (dfTotal / dfResid), Double.NaN)

            Me.pSSE = sse : Me.pSST = sst : Me.pSSR = ssr
            Me.pMSE = mse : Me.pRMSE = rmse
            Me.pRSquared = r2 : Me.pAdjRSquared = adjR2
            Me.pDFModel = dfModel : Me.pDFResid = dfResid : Me.pDFTotal = dfTotal

            'Covariance: mse*(X'WX)^-1 using your MatInv
            Dim cov(,) As Double = Nothing
            If bReturnCov OrElse bComputeResiduals Then
                Dim invXtWX As Double(,) = InvertXtWX_UsingMatrixVB(X, w)
                cov = Matrix.MatrixMult(invXtWX, mse) 'ScaleMatrix(invXtWX, mse)
            End If
            Me.pCovariance = cov

            'Inference (use covariance diagonal)
            Dim se(p - 1) As Double
            For j As Integer = 0 To p - 1
                se(j) = If(cov Is Nothing, params(j, 1), Math.Sqrt(Math.Max(0.0, cov(j, j))))
            Next

            'Overall F-test (also shown in overall ANOVA)
            Dim msr As Double = If(dfModel > 0, ssr / dfModel, Double.NaN)
            Dim fStat As Double = If(dfModel > 0 AndAlso mse > 0, msr / mse, Double.NaN)
            Dim pStat As Double = If(dfModel > 0 AndAlso mse > 0, 1.0 - Distributions.F_CDF(fStat, CDbl(dfModel), CDbl(dfResid)), Double.NaN)

            'Gaussian LL/AIC/BIC (common convention)
            Dim sigma2ML As Double = Math.Max(sse / n, 1.0E-300R)
            Dim ll As Double = -0.5 * n * (Math.Log(2.0 * Math.PI * sigma2ML) + 1.0)
            Dim aic As Double = -2.0 * ll + 2.0 * p
            Dim bic As Double = -2.0 * ll + Math.Log(n) * p

            Me.pLogLik = ll : Me.pAIC = aic : Me.pBIC = bic

            'Diagnostics
            Dim leverage() As Double = Nothing
            Dim stdRes() As Double = Nothing
            Dim cooks() As Double = Nothing
            If bComputeResiduals Then
                If cov Is Nothing Then BESHStatNG.BSerr.LogAndThrow(New InvalidOperationException("Covariance is required for diagnostics."))
                ComputeDiagnostics(X, w, cov, mse)
            End If

            '==== Build ResultTables ====
            pAnovaOverall = BuildOverallAnovaTable(ssr, sse, sst, dfModel, dfResid, dfTotal, mse, fStat, pStat)
            pVIF = ComputeVIFTable_toPrint(X, w, includeIntercept) 'VIF

            'Optional term ANOVA (Type I and Type III only)
            If computeTermAnova = TermSumOfSquaresType.TypeI Then
                pAnovaTypeI = BuildTermAnova_toPrint(TermSumOfSquaresType.TypeI, includeIntercept, mse, dfResid)
            Else
                pAnovaTypeI = Nothing
            End If
            If computeTermAnova = TermSumOfSquaresType.TypeIII Then
                pAnovaTypeIII = BuildTermAnova_toPrint(TermSumOfSquaresType.TypeIII, includeIntercept, mse, dfResid)
            Else
                pAnovaTypeIII = Nothing
            End If

            '==== Populate LMresult (coefficients + model summary) ====
            Dim predictorNamesOnly As String() = BuildPredictorNames(pPredictors) 'no intercept
            Me.results = New LMresult With {
                .alpha = Me.Alpha,
                .bIntercept = includeIntercept,
                .varNames = predictorNamesOnly,
                .n = n,
                .dfResid = dfResid,
                .Coeffs_est = beta,
                .Coeffs_SEsT = se,
                .Coeffs_SEs = se}

            results.ModelTableTopRow = {"Linear Model", "", "df", "p-value"}
            results.ModelTableLabels = {
            "# observations",
            "Parameters (p)",
            "DF model",
            "DF residual",
            "R²",
            "Adj. R²",
            "Overall F",
            "Log Likelihood",
            "AIC",
            "BIC"}

            results.ModelTableVals = New Object(,) {
            {n, "", ""},
            {p, "", ""},
            {dfModel, "", ""},
            {dfResid, "", ""},
            {r2, "", ""},
            {adjR2, "", ""},
            {fStat, $"{dfModel}, {dfResid}", pStat},
            {ll, "", ""},
            {aic, p, ""},
            {bic, p, ""}}
        End Sub

        ''' <summary>
        ''' Builds the overall (model/residual/total) ANOVA table for the fitted model.
        ''' </summary>
        ''' <param name="ssr">Model (regression) sum of squares, <c>SSR = SST − SSE</c>.</param>
        ''' <param name="sse">Residual (error) sum of squares, <c>SSE = Σ wᵢ (yᵢ − ŷᵢ)²</c>.</param>
        ''' <param name="sst">
        ''' Total sum of squares. If an intercept is included, <c>SST = Σ wᵢ (yᵢ − ȳ_w)²</c> with weighted mean
        ''' <c>ȳ_w = (Σ wᵢ yᵢ)/(Σ wᵢ)</c>. Without intercept, an uncentered total <c>SST = Σ wᵢ yᵢ²</c> is used.
        ''' </param>
        ''' <param name="dfModel">Model degrees of freedom (<c>p − 1</c> with intercept, otherwise <c>p</c>).</param>
        ''' <param name="dfResid">Residual degrees of freedom <c>n − p</c>.</param>
        ''' <param name="dfTotal">Total degrees of freedom (typically <c>n − 1</c> with intercept).</param>
        ''' <param name="mse">Mean squared error, <c>MSE = SSE / dfResid</c>.</param>
        ''' <param name="fStat">Overall F statistic, <c>F = (SSR/dfModel) / MSE</c> (undefined when <c>dfModel = 0</c>).</param>
        ''' <param name="pStat">Overall model p-value computed from <see cref="Distributions.F_CDF"/>.</param>
        ''' <returns>A <see cref="ResultTable"/> containing the overall ANOVA decomposition and F-test.</returns>
        ''' <remarks>
        ''' <para>
        ''' This is the standard Gaussian linear-model ANOVA decomposition adapted to WLS by using the weighted sums of squares.
        ''' For an intercept-only model, <paramref name="dfModel"/> is zero and the model F-test is not defined; the table still
        ''' reports SSE/MSE and the total sum of squares.
        ''' </para>
        ''' </remarks>
        Private Function BuildOverallAnovaTable(ssr As Double, sse As Double, sst As Double,
                                           dfModel As Integer, dfResid As Integer, dfTotal As Integer,
                                           mse As Double, fStat As Double, pStat As Double) As ResultTable

            Dim t As New ResultTable
            t.AddTitle("ANOVA (Overall fit)")
            t.AddHeaderTopRow({"SS", "df", "MS", "F", "p"})

            Dim left() As String = {"Model", "Residuals", "Total"}
            t.AddHeaderLeftRow(left)

            Dim body(2, 4) As Object

            Dim msr As Object = If(dfModel > 0, ssr / dfModel, Nothing)

            body(0, 0) = ssr
            body(0, 1) = dfModel
            body(0, 2) = msr
            body(0, 3) = fStat
            body(0, 4) = pStat

            body(1, 0) = sse
            body(1, 1) = dfResid
            body(1, 2) = mse
            body(1, 3) = Nothing
            body(1, 4) = Nothing

            body(2, 0) = sst
            body(2, 1) = dfTotal
            body(2, 2) = Nothing
            body(2, 3) = Nothing
            body(2, 4) = Nothing

            t.SetBody(body)
            t.AddPvalueToFormat(5)
            Return t
        End Function

        ''' <summary>
        ''' Creates a default mapping from term names to design-matrix column indices.
        ''' </summary>
        ''' <param name="includeIntercept">Whether the design matrix includes an intercept column at index 0.</param>
        ''' <param name="pPredictors">Number of predictor columns supplied by the caller (excluding the response column).</param>
        ''' <param name="custom">
        ''' Optional custom mapping. When provided, it is cloned and returned directly; otherwise a default mapping is built
        ''' where each predictor column is treated as its own term.
        ''' </param>
        ''' <returns>
        ''' A dictionary where keys are term names and values are arrays of column indices into <c>X</c> (after intercept insertion).
        ''' </returns>
        ''' <remarks>
        ''' <para>
        ''' Term groups are used only for term-wise ANOVA (Type I/III) and allow multi-column terms, e.g. dummy-coded factors or
        ''' precomputed interaction blocks. Interaction columns must be computed by the caller and included in the design matrix.
        ''' </para>
        ''' </remarks>
        Private Function BuildDefaultTermGroups(includeIntercept As Boolean,
                                           pPredictors As Integer,
                                           custom As Dictionary(Of String, Integer())) As Dictionary(Of String, Integer())

            If custom IsNot Nothing Then Return New Dictionary(Of String, Integer())(custom)

            Dim groups As New Dictionary(Of String, Integer())()
            Dim colOffset As Integer = If(includeIntercept, 1, 0)

            If includeIntercept Then groups("Intercept") = New Integer() {0}

            Dim namesOK As Boolean = (pVarNames IsNot Nothing AndAlso pVarNames.Length = pPredictors)
            For j As Integer = 0 To pPredictors - 1
                Dim nm As String = If(namesOK, pVarNames(j), $"X{j + 1}")
                groups(nm) = New Integer() {j + colOffset}
            Next

            Return groups
        End Function

        ''' <summary>
        ''' Builds the list of predictor (parameter) names in the same order as the predictor columns in the input matrix.
        ''' </summary>
        ''' <param name="pPredictors">Number of predictor columns in the input data (excluding the response column).</param>
        ''' <returns>
        ''' An array of predictor names. If <c>pVarNames</c> is not set or has an unexpected length, generic names (<c>X1</c>, <c>X2</c>, …)
        ''' are returned.
        ''' </returns>
        ''' <remarks>
        ''' This helper is used to populate <see cref="LMresult.varNames"/> so that coefficient tables are labeled consistently.
        ''' </remarks>
        Private Function BuildPredictorNames(pPredictors As Integer) As String()
            Dim out As New List(Of String)()
            Dim namesOK As Boolean = (pVarNames IsNot Nothing AndAlso pVarNames.Length = pPredictors)
            For j As Integer = 0 To pPredictors - 1
                out.Add(If(namesOK, pVarNames(j), $"X{j + 1}"))
            Next
            Return out.ToArray()
        End Function


        ''' <summary>
        ''' Builds a term-wise ANOVA table (Type I or Type III) by refitting reduced models and comparing residual sums of squares.
        ''' </summary>
        ''' <param name="ssType">The sums-of-squares convention (Type I sequential or Type III partial).</param>
        ''' <param name="includeIntercept">Whether the fitted design matrix includes an intercept column.</param>
        ''' <param name="mseFull">Full-model mean squared error <c>MSE = SSE/(n − p)</c>.</param>
        ''' <param name="dfResidFull">Full-model residual degrees of freedom <c>n − p</c>.</param>
        ''' <returns>A <see cref="ResultTable"/> with per-term df, SS, MS, F and p-values.</returns>
        ''' <remarks>
        ''' <para>
        ''' Each term corresponds to a set of columns in <c>X</c> as specified by <c>termGroups</c>.
        ''' For each term, a reduced model is formed by including/excluding columns:
        ''' </para>
        ''' <list type="bullet">
        '''   <item>
        '''     <description><b>Type I:</b> terms are added sequentially; SS for a term is <c>SSE(previous) − SSE(new)</c>.</description>
        '''   </item>
        '''   <item>
        '''     <description><b>Type III:</b> each term is tested by dropping its columns from the full model; SS is <c>SSE(reduced) − SSE(full)</c>.</description>
        '''   </item>
        ''' </list>
        ''' <para>
        ''' Reduced-model fits are performed using <see cref="Matrix.MinimalWLS(Double(), Double(,), Double())"/> inside <see cref="SSEForDesign"/>.
        ''' F-tests use <see cref="Distributions.F_CDF"/>.
        ''' </para>
        ''' </remarks>
        Private Function BuildTermAnova_toPrint(ssType As TermSumOfSquaresType, includeIntercept As Boolean,
                                           mseFull As Double, dfResidFull As Integer) As ResultTable

            Dim termKeys As List(Of String) = termGroups.Keys.
            Where(Function(k) Not String.Equals(k, "Intercept", StringComparison.OrdinalIgnoreCase)).ToList()

            Dim t As New ResultTable
            t.AddTitle($"ANOVA ({ssType})")
            t.AddHeaderTopRow({"df", "SS", "MS", "F", "p"})

            Dim sseFull As Double = SSEForDesign(X)

            Dim rows As New List(Of String)()
            Dim vals As New List(Of Object())()

            Select Case ssType

                Case TermSumOfSquaresType.TypeI
                    Dim includedCols As New List(Of Integer)
                    If includeIntercept Then includedCols.Add(0)

                    Dim ssePrev As Double = SSEForColumns(includedCols.ToArray())
                    Dim dfPrev As Integer = includedCols.Count

                    For Each term As String In termKeys
                        Dim colsThis As Integer() = termGroups(term)
                        Dim newCols As New List(Of Integer)(includedCols)
                        newCols.AddRange(colsThis.Where(Function(c) Not newCols.Contains(c)))

                        Dim sseNew As Double = SSEForColumns(newCols.ToArray())
                        Dim dfNew As Integer = newCols.Count

                        Dim ssTerm As Double = ssePrev - sseNew
                        Dim dfTerm As Integer = dfNew - dfPrev
                        Dim msTerm As Double = If(dfTerm > 0, ssTerm / dfTerm, Double.NaN)
                        Dim fTerm As Double = If(dfTerm > 0, msTerm / mseFull, Double.NaN)
                        Dim pTerm As Double = If(dfTerm > 0, 1.0 - Distributions.F_CDF(fTerm, CDbl(dfTerm), CDbl(dfResidFull)), Double.NaN)

                        rows.Add(term)
                        vals.Add(New Object() {dfTerm, ssTerm, msTerm, fTerm, pTerm})

                        includedCols = newCols
                        ssePrev = sseNew
                        dfPrev = dfNew
                    Next

                Case TermSumOfSquaresType.TypeIII
                    Dim fullCols As Integer() = Enumerable.Range(0, p).ToArray()

                    For Each term As String In termKeys
                        Dim dropCols As Integer() = termGroups(term)
                        Dim keepCols As Integer() = fullCols.Where(Function(c) Not dropCols.Contains(c)).ToArray()

                        Dim sseReduced As Double = SSEForColumns(keepCols)
                        Dim ssTerm As Double = sseReduced - sseFull
                        Dim dfTerm As Integer = dropCols.Length
                        Dim msTerm As Double = ssTerm / dfTerm
                        Dim fTerm As Double = msTerm / mseFull
                        Dim pTerm As Double = 1.0 - Distributions.F_CDF(fTerm, CDbl(dfTerm), CDbl(dfResidFull))

                        rows.Add(term)
                        vals.Add(New Object() {dfTerm, ssTerm, msTerm, fTerm, pTerm})
                    Next

                Case Else
                    BESHStatNG.BSerr.LogAndThrow(New ArgumentOutOfRangeException(NameOf(ssType)))
            End Select

            'Add residual row
            rows.Add("Residuals")
            vals.Add(New Object() {dfResidFull, sseFull, mseFull, Nothing, Nothing})

            t.AddHeaderLeftRow(rows.ToArray())

            Dim body(rows.Count - 1, 4) As Object
            For i As Integer = 0 To rows.Count - 1
                Dim r = vals(i)
                For j As Integer = 0 To 4
                    body(i, j) = r(j)
                Next
            Next

            t.SetBody(body)
            t.AddPvalueToFormat(5)
            Return t
        End Function

        ''' <summary>
        ''' Computes the weighted residual sum of squares (SSE) for a submodel defined by a subset of columns of <c>X</c>.
        ''' </summary>
        ''' <param name="cols">Indices of columns to include from the full design matrix <c>X</c>.</param>
        ''' <returns>The weighted SSE for the submodel.</returns>
        ''' <remarks>
        ''' This method forms a submatrix <c>X_sub</c> and calls <see cref="SSEForDesign"/> to refit via WLS and compute SSE.
        ''' </remarks>
        Private Function SSEForColumns(cols As Integer()) As Double
            Dim Xsub(,) As Double = SubMatrixColumns(X, cols)
            Return SSEForDesign(Xsub)
        End Function

        ''' <summary>
        ''' Fits a WLS/OLS model for a given design matrix and returns its weighted residual sum of squares (SSE).
        ''' </summary>
        ''' <param name="Xdesign">Design matrix to fit (may be the full matrix or a reduced submatrix).</param>
        ''' <returns>Weighted residual sum of squares, <c>SSE = Σ wᵢ (yᵢ − ŷᵢ)²</c>.</returns>
        ''' <remarks>
        ''' <para>
        ''' Coefficients are computed using <see cref="Matrix.MinimalWLS(Double(), Double(,), Double())"/> and predictions using <see cref="Predict1D"/>.
        ''' This function is the computational core used by term-wise ANOVA (Type I/III), where many reduced models are fit.
        ''' </para>
        ''' </remarks>
        Private Function SSEForDesign(Xdesign(,) As Double) As Double
            Dim params As Double(,) = Matrix.MinimalWLS(y, Xdesign, w)
            Dim betaLocal() As Double = Matrix.GetColumnFrom2Darray(params, 0)
            Dim yhat() As Double = Predict1D(Xdesign, betaLocal)

            Dim sse As Double = 0.0
            For i As Integer = 0 To n - 1
                Dim r As Double = y(i) - yhat(i)
                sse += w(i) * r * r
            Next
            Return sse
        End Function


        ''' <summary>
        ''' Computes observation-level diagnostics: leverage, standardized residuals, Cook's distance, and jackknife (PRESS) residuals.
        ''' </summary>
        ''' <param name="Xfull">Full design matrix used in the final fit.</param>
        ''' <param name="wvec">Weights vector <c>w</c> (must be positive).</param>
        ''' <param name="covBeta">Coefficient covariance matrix <c>Var(β̂) = MSE · (X'WX)^{-1}</c>.</param>
        ''' <param name="mse">Mean squared error <c>MSE = SSE/(n − p)</c>.</param>
        ''' <remarks>
        ''' <para>
        ''' The diagonal of the WLS hat matrix is computed as:
        ''' <c>h_ii = w_i · x_i' (X'WX)^{-1} x_i</c>.
        ''' Standardized residuals are computed as:
        ''' <c>r_i = e_i · sqrt(w_i) / sqrt(MSE · (1 − h_ii))</c>.
        ''' Jackknife/deleted (PRESS) residuals are:
        ''' <c>e_(i) = e_i / (1 − h_ii)</c>.
        ''' Cook's distance uses the common form:
        ''' <c>D_i = (e_i² w_i / (p·MSE)) · (h_ii / (1 − h_ii)²)</c>.
        ''' </para>
        ''' <para>
        ''' The inverse information <c>(X'WX)^{-1}</c> is obtained by un-scaling <paramref name="covBeta"/>:
        ''' <c>(X'WX)^{-1} = Var(β̂) / MSE</c>.
        ''' </para>
        ''' </remarks>
        Private Sub ComputeDiagnostics(Xfull(,) As Double, wvec() As Double, covBeta(,) As Double, mse As Double)

            Dim invXtWX As Double(,) = Matrix.MatrixMult(covBeta, 1.0 / mse) 'undo sigma^2 scaling
            Dim nLocal As Integer = UBound(Xfull, 1) + 1
            Dim pLocal As Integer = UBound(Xfull, 2) + 1

            ReDim Me.pLeverage(nLocal - 1), Me.pStdResidual(nLocal - 1), Me.pCooksD(nLocal - 1), Me.pJackknifeResidual(nLocal - 1)

            For i As Integer = 0 To nLocal - 1
                Dim tmp As Double = 0.0
                For a As Integer = 0 To pLocal - 1
                    For b As Integer = 0 To pLocal - 1
                        tmp += Xfull(i, a) * invXtWX(a, b) * Xfull(i, b)
                    Next
                Next

                pLeverage(i) = wvec(i) * tmp
                pLeverage(i) = Math.Min(0.999999, Math.Max(0.0, pLeverage(i)))
                Dim oneMinus As Double = Math.Max(0.000000000001, 1.0 - pLeverage(i))

                pJackknifeResidual(i) = Me.pResiduals(i) / oneMinus ' Jackknife / deleted (PRESS) residual

                Dim denom As Double = Math.Sqrt(Math.Max(1.0E-300R, mse * oneMinus))
                pStdResidual(i) = Me.pResiduals(i) * Math.Sqrt(wvec(i)) / denom
                pCooksD(i) = (Me.pResiduals(i) * Me.pResiduals(i) * wvec(i) / (pLocal * mse)) * (pLeverage(i) / (oneMinus * oneMinus))
            Next
        End Sub

        ''' <summary>
        ''' Computes variance inflation factors (VIF) for each predictor (excluding the intercept) and returns them as a <see cref="ResultTable"/>.
        ''' </summary>
        ''' <param name="Xfull">Full design matrix (including intercept if present).</param>
        ''' <param name="wvec">Weights vector.</param>
        ''' <param name="includeIntercept">If <c>True</c>, the first column is treated as intercept and excluded from VIF computations.</param>
        ''' <returns>A <see cref="ResultTable"/> with one VIF value per predictor column.</returns>
        ''' <remarks>
        ''' <para>
        ''' This implementation computes the (weighted) predictor correlation matrix <c>R</c> (excluding the intercept) and returns
        ''' <c>VIF_j = (R^{-1})_{jj}</c>.
        ''' Weighted covariance is computed using the weighted mean and
        ''' <c>Cov(a,b) = Σ w_i (a_i − ā_w)(b_i − b̄_w) / (Σ w_i − 1)</c>.
        ''' The correlation matrix is formed by normalizing covariances by standard deviations. The inverse is obtained via
        ''' <see cref="Matrix.MatInv"/> with Cholesky ("CHOL").
        ''' </para>
        ''' <para>
        ''' If the predictor correlation matrix is singular or not positive definite (perfect/multi-collinearity), the inversion may fail.
        ''' In that case the calling code may choose to report infinite/undefined VIFs.
        ''' </para>
        ''' </remarks>
        Private Function ComputeVIFTable_toPrint(Xfull(,) As Double, wvec() As Double, includeIntercept As Boolean) As ResultTable
            Dim t As New ResultTable
            t.AddTitle("VIF")

            Dim pLocal As Integer = UBound(Xfull, 2) + 1
            Dim startCol As Integer = If(includeIntercept, 1, 0)
            Dim m As Integer = pLocal - startCol
            If m <= 0 Then Return t

            ' Extract predictor matrix Z (skip intercept)
            Dim Z(n - 1, m - 1) As Double
            For i As Integer = 0 To n - 1
                For j As Integer = 0 To m - 1
                    Z(i, j) = Xfull(i, j + startCol)
                Next
            Next

            ' Weighted means
            Dim mean(m - 1) As Double
            For j As Integer = 0 To m - 1
                Dim col() As Double = Matrix.GetColumnFrom2Darray(Z, j)
                mean(j) = WeightedMean(col, wvec)
            Next

            ' Weighted covariance matrix
            Dim wSum As Double = wvec.Sum()
            Dim covMat(m - 1, m - 1) As Double

            For a As Integer = 0 To m - 1
                For b As Integer = a To m - 1
                    Dim acc As Double = 0.0
                    For i As Integer = 0 To n - 1
                        Dim da As Double = Z(i, a) - mean(a)
                        Dim db As Double = Z(i, b) - mean(b)
                        acc += wvec(i) * da * db
                    Next

                    acc /= Math.Max(1.0, wSum - 1.0)
                    covMat(a, b) = acc
                    covMat(b, a) = acc
                Next
            Next

            ' Correlation matrix R
            Dim R(m - 1, m - 1) As Double
            For a As Integer = 0 To m - 1
                For b As Integer = 0 To m - 1
                    Dim denom As Double = Math.Sqrt(Math.Max(1.0E-300R, covMat(a, a) * covMat(b, b)))
                    R(a, b) = If(denom > 0, covMat(a, b) / denom, 0.0)
                Next
            Next

            ' Invert correlation matrix -> VIF = diag(invR)
            Dim invR As Double(,)
            Dim namesOK As Boolean = (pVarNames IsNot Nothing AndAlso pVarNames.Length = m)

            Dim rowNames As New List(Of String)
            Dim body(m - 1, 0) As Object

            Try
                invR = Matrix.MatInv(R, "CHOL")
                For j As Integer = 0 To m - 1
                    rowNames.Add(If(namesOK, pVarNames(j), $"X{j + 1}"))
                    body(j, 0) = invR(j, j)
                Next
            Catch
                For j As Integer = 0 To m - 1
                    rowNames.Add(If(namesOK, pVarNames(j), $"X{j + 1}"))
                    body(j, 0) = Double.PositiveInfinity
                Next
            End Try

            t.AddHeaderTopRow({"VIF"})
            t.AddHeaderLeftRow(rowNames.ToArray())
            t.SetBody(body)

            Return t
        End Function


        ''' <summary>
        ''' Computes and inverts the weighted cross-product matrix <c>X' W X</c>.
        ''' </summary>
        ''' <param name="Xfull">Design matrix <c>X</c>.</param>
        ''' <param name="wvec">Weights vector <c>w</c>.</param>
        ''' <returns>
        ''' The inverse matrix <c>(X' W X)^{-1}</c>.
        ''' </returns>
        ''' <remarks>
        ''' <para>
        ''' The matrix <c>X'WX</c> is computed element-wise as
        ''' <c>(X'WX)_{ij} = Σ_k X_{k,i} · w_k · X_{k,j}</c>.
        ''' The inverse is computed by <see cref="Matrix.MatInv"/> using a Cholesky-based method ("CHOL").
        ''' </para>
        ''' </remarks>
        Private Function InvertXtWX_UsingMatrixVB(Xfull(,) As Double, wvec() As Double) As Double(,)
            Dim nR As Integer = UBound(Xfull, 1) + 1
            Dim nC As Integer = UBound(Xfull, 2) + 1

            Dim XtWX(nC - 1, nC - 1) As Double
            For i As Integer = 0 To nC - 1
                For j As Integer = i To nC - 1
                    Dim acc As Double = 0.0
                    For k As Integer = 0 To nR - 1
                        acc += Xfull(k, i) * wvec(k) * Xfull(k, j)
                    Next
                    XtWX(i, j) = acc
                    XtWX(j, i) = acc
                Next
            Next

            Return Matrix.MatInv(XtWX, "CHOL")
        End Function

        '==========================================================
        ' Glue helpers
        '==========================================================
        ''' <summary>
        ''' Computes fitted values <c>ŷ = X β</c> for a given design matrix and coefficient vector.
        ''' </summary>
        ''' <param name="Xdesign">Design matrix <c>X</c>.</param>
        ''' <param name="beta">Coefficient vector <c>β</c>.</param>
        ''' <returns>Vector of fitted values <c>ŷ</c>.</returns>
        ''' <remarks>
        ''' This method converts <paramref name="beta"/> into a <c>p×1</c> matrix and multiplies using
        ''' <see cref="Matrix.MatrixMult(Double(,), Double(,))"/>, then extracts the single output column using
        ''' <see cref="Matrix.GetColumnFrom2Darray"/>.
        ''' </remarks>
        Private Function Predict1D(Xdesign(,) As Double, beta() As Double) As Double()
            Dim beta2D(UBound(beta), 0) As Double
            For j As Integer = 0 To UBound(beta)
                beta2D(j, 0) = beta(j)
            Next

            Dim yhat2D As Double(,) = Matrix.MatrixMult(Xdesign, beta2D)
            Return Matrix.GetColumnFrom2Darray(yhat2D, 0)
        End Function

        ''' <summary>
        ''' Extracts a submatrix containing only the specified columns from a given 2D array.
        ''' </summary>
        ''' <param name="A">Source matrix.</param>
        ''' <param name="cols">Zero-based column indices to extract.</param>
        ''' <returns>A matrix with the same number of rows as <paramref name="A"/> and columns given by <paramref name="cols"/>.</returns>
        ''' <remarks>
        ''' Used to construct reduced design matrices for term-wise ANOVA refits.
        ''' </remarks>
        Private Function SubMatrixColumns(A(,) As Double, cols As Integer()) As Double(,)
            Dim nR As Integer = UBound(A, 1) + 1
            Dim nC As Integer = cols.Length
            Dim out(nR - 1, nC - 1) As Double
            For i As Integer = 0 To nR - 1
                For j As Integer = 0 To nC - 1
                    out(i, j) = A(i, cols(j))
                Next
            Next
            Return out
        End Function

        ''' <summary>
        ''' Computes the weighted mean of a vector.
        ''' </summary>
        ''' <param name="x">Values.</param>
        ''' <param name="wvec">Weights (must be non-negative; typically positive).</param>
        ''' <returns>
        ''' The weighted mean <c>ȳ_w = (Σ wᵢ xᵢ)/(Σ wᵢ)</c>. If the sum of weights is not positive, returns 0.
        ''' </returns>
        ''' <remarks>
        ''' Used for computing centered total sum of squares and for weighted covariance/correlation in VIF.
        ''' </remarks>
        Private Function WeightedMean(x() As Double, wvec() As Double) As Double
            Dim sw As Double = 0.0, sx As Double = 0.0
            For i As Integer = 0 To x.Length - 1
                sw += wvec(i)
                sx += wvec(i) * x(i)
            Next
            If sw <= 0 Then Return 0.0
            Return sx / sw
        End Function

    End Class
End Namespace