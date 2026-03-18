Option Strict On
Option Explicit On

Imports System.Collections.Generic
Imports System.Globalization
Imports System.Resources.ResXFileRef
Imports System.Text
Imports BESHStatNG.AppInfrastructure

Namespace regression


    ''' <summary>
    ''' Fits an ordinal (ordered categorical) proportional-odds logistic regression model
    ''' with optional offset and case weights, and provides post-fit likelihood statistics,
    ''' profile-based deviance goodness-of-fit, classification crosstab, and residuals.
    ''' </summary>
    ''' <remarks>
    ''' <para><b>Model.</b> Let Y take ordered values {c0 &lt; c1 &lt; ... &lt; c_{K-1}}. Define K-1 thresholds (cutpoints)
    ''' α_1,...,α_{K-1} and a linear predictor η_i = x_i^T β + offset_i.</para>
    '''
    ''' <para><b>Proportional odds (cumulative logit):</b></para>
    ''' <para>
    ''' logit( P(Y_i ≤ c_k) ) = α_{k+1} - η_i,   for k = 0..K-2
    ''' </para>
    ''' <para>
    ''' with logistic CDF F(t)=1/(1+exp(-t)). Define g_{ik}=F(α_k - η_i), k=1..K-1.
    ''' Category probabilities:
    ''' </para>
    ''' <para>
    ''' p_{i1} = g_{i1},
    ''' p_{ik} = g_{ik} - g_{i,k-1}  (k=2..K-1),
    ''' p_{iK} = 1 - g_{i,K-1}.
    ''' </para>
    '''
    ''' <para><b>Weighted log-likelihood:</b> ℓ(β,α) = Σ_i w_i * log(p_{i,y_i}).</para>
    '''
    ''' <para><b>Identifiability note:</b> A separate intercept inside η_i is not identifiable because shifting
    ''' η_i by a constant can be absorbed into all α_k. This implementation therefore does not estimate a
    ''' separate intercept column in xβ. Thresholds serve as intercepts.</para>
    ''' </remarks>
    Public Class OrdinalLogitModel

        Public bIterationDetails As Boolean = False
        Public startParams() As Double = Nothing

        ' ----------------------- Data / inputs -----------------------
        Private pData(,) As Double
        Private pX(,) As Double ' predictors only (no intercept column)
        Private pVarNames() As String

        Private pbOffset As Boolean
        Private pOffset() As Double

        Private pbWeights As Boolean
        Private pWeights() As Double

        Private pRowNums() As Integer
        ' Direction / reference choice for the ordinal scale
        Private pReference As ReferenceCategory = ReferenceCategory.Last

        ' ----------------------- Fit control -----------------------
        Private pMaxiter As Integer = 50
        Private pEps As Double = 0.0000000001
        Private pRidge As Double = 0.0000000001
        Private pItInfo(,) As Double
        Private pLastIterLLchange As Double
        Private pIteration As Integer
        Private pAlpha As Double
        Private CompTime As Double

        ' ----------------------- Category mapping -----------------------
        Private n As Integer
        Private p As Integer
        Private pK As Integer
        Private pCats() As Integer
        Private pyFit() As Integer ' 0..K-1 in ascending category order

        ' ----------------------- Fit outputs -----------------------
        ''' <summary>
        ''' Regression output container consistent with your project (coefficients + SEs + names).
        ''' Coefficients are stored in the order: β (predictors), then thresholds θ (α_1..α_{K-1}).
        ''' </summary>
        Public results As LMresult

        ''' <summary>
        ''' If True, computes residuals after fitting.
        ''' </summary>
        Public bComputeResiduals As Boolean = False

        ''' <summary>
        ''' If True, stores covariance matrix (inverse information) for leverage/standardization.
        ''' </summary>
        Public bReturnCov As Boolean = False

        ' Covariance approximation (inverse observed information, with ridge).
        Private pCov(,) As Double = Nothing

        ' ----------------------- Post-fit stats -----------------------
        Private pLL As Double = Double.NaN
        Private pLL0 As Double = Double.NaN

        Private pAIC As Double = Double.NaN
        Private pBIC As Double = Double.NaN
        Private pCoxSnellR2 As Double = Double.NaN
        Private pNagelkerkeR2 As Double = Double.NaN
        Private pMcFaddenR2 As Double = Double.NaN

        Private pModelChi2 As TestResult = Nothing
        Private pGOF As TestResult = Nothing

        Private pPredAccuracy As ClassificationCrosstab = Nothing
        Private pResiduals As MultinomialResiduals = Nothing

        ' ----------------------- Public properties -----------------------

        ''' <summary>Final maximized log-likelihood ℓ(β,α).</summary>
        Public ReadOnly Property LogLikelihood As Double
            Get
                Return pLL
            End Get
        End Property

        ''' <summary>Null (threshold-only) log-likelihood ℓ0.</summary>
        Public ReadOnly Property NullLogLikelihood As Double
            Get
                Return pLL0
            End Get
        End Property

        ''' <summary>AIC = -2ℓ + 2k where k is number of estimated parameters.</summary>
        Public ReadOnly Property AIC As Double
            Get
                Return pAIC
            End Get
        End Property

        ''' <summary>BIC = -2ℓ + log(nobs)*k where nobs = sum(weights) if weights are provided, else n.</summary>
        Public ReadOnly Property BIC As Double
            Get
                Return pBIC
            End Get
        End Property

        ''' <summary>Cox–Snell pseudo R² = 1 - exp((2/n)(ℓ0-ℓ)).</summary>
        Public ReadOnly Property CoxSnellR2 As Double
            Get
                Return pCoxSnellR2
            End Get
        End Property

        ''' <summary>Nagelkerke pseudo R² = CoxSnell / (1 - exp((2/n)ℓ0)).</summary>
        Public ReadOnly Property NagelkerkeR2 As Double
            Get
                Return pNagelkerkeR2
            End Get
        End Property

        ''' <summary>McFadden pseudo R² = 1 - (ℓ/ℓ0).</summary>
        Public ReadOnly Property McFaddenR2 As Double
            Get
                Return pMcFaddenR2
            End Get
        End Property

        ''' <summary>Likelihood-ratio “Model Chi-Square” test result (global null: all slopes = 0).</summary>
        Public ReadOnly Property ModelChi2 As TestResult
            Get
                Return pModelChi2
            End Get
        End Property

        ''' <summary>Profile-based deviance goodness-of-fit test result.</summary>
        Public ReadOnly Property GOF As TestResult
            Get
                Return pGOF
            End Get
        End Property

        ''' <summary>Classification crosstab based on argmax_k p_{ik}.</summary>
        Public ReadOnly Property Classification As ClassificationCrosstab
            Get
                Return pPredAccuracy
            End Get
        End Property

        ''' <summary>Latest computed residuals (same container as multinomial residuals).</summary>
        Public ReadOnly Property Residuals As MultinomialResiduals
            Get
                Return pResiduals
            End Get
        End Property

        ' ----------------------- Configuration -----------------------

        ''' <summary>
        ''' Sets solver controls.
        ''' </summary>
        ''' <param name="maxIter">Maximum number of iterations.</param>
        ''' <param name="eps">Convergence tolerance (step norm and LL change).</param>
        ''' <param name="ridge">Ridge added to information matrix diagonal before inversion.</param>
        Public Sub SettingInputs(dAlpha As Double,
                             Optional maxIter As Integer = 50,
                             Optional eps As Double = 0.0000000001,
                             Optional ridge As Double = 0.000000000001)
            pAlpha = dAlpha
            pMaxiter = maxIter
            pEps = eps
            pRidge = ridge
        End Sub

        ''' <summary>
        ''' Supplies data and optional offset/weights.
        ''' </summary>
        ''' <param name="x">
        ''' Data matrix with n rows. Column 0 is the ordinal outcome. Columns 1.. are predictors.
        ''' </param>
        ''' <param name="names">
        ''' Variable names: names(0)=outcome name; names(1..)=predictor names in the same order as x.
        ''' </param>
        ''' <param name="RowNums">Optional mapping from row index to original row id.</param>
        ''' <param name="offset">Optional offset vector (length n), added to η_i.</param>
        ''' <param name="weights">Optional case weights (length n). For frequency data, treat as replicates.</param>
        Public Sub Data(x(,) As Double, names() As String,
                    Optional RowNums() As Integer = Nothing,
                    Optional offset() As Double = Nothing,
                    Optional weights() As Double = Nothing)

            pData = x
            pVarNames = names

            Dim nRows As Integer = UBound(x, 1) + 1

            If RowNums Is Nothing Then
                ReDim pRowNums(nRows - 1)
                For i As Integer = 0 To nRows - 1
                    pRowNums(i) = i
                Next
            Else
                pRowNums = RowNums
            End If

            If offset Is Nothing Then
                pbOffset = False
                ReDim pOffset(nRows - 1)
                For i As Integer = 0 To nRows - 1
                    pOffset(i) = 0.0
                Next
            Else
                pbOffset = True
                pOffset = offset
            End If

            If weights Is Nothing Then
                pbWeights = False
                ReDim pWeights(nRows - 1)
                For i As Integer = 0 To nRows - 1
                    pWeights(i) = 1.0
                Next
            Else
                pbWeights = True
                pWeights = weights
            End If
        End Sub

        Public Function wrapResiduals() As Object(,)
            'call this sub only after we have parameters estimated
            Dim t As New ResultTable, tmp2(n - 1, 2) As Double
            Dim tmp = Matrix.VerticalStackArrays(Me.pResiduals.FittedMeans, Me.pResiduals.Probabilities)
            tmp = Matrix.VerticalStackArrays(tmp, Me.pResiduals.ResponseResiduals)
            tmp = Matrix.VerticalStackArrays(tmp, Me.pResiduals.PearsonResiduals)
            tmp = Matrix.VerticalStackArrays(tmp, Me.pResiduals.StdPearsonResiduals)

            Dim resnames = Matrix.ConcatArrays(GetResidualColumnNames(ResidualColumnType.FittedMean),
                                    GetResidualColumnNames(ResidualColumnType.FittedProbability))
            resnames = Matrix.ConcatArrays(resnames, GetResidualColumnNames(ResidualColumnType.ResponseResidual))
            resnames = Matrix.ConcatArrays(resnames, GetResidualColumnNames(ResidualColumnType.PearsonResidual))
            resnames = Matrix.ConcatArrays(resnames, GetResidualColumnNames(ResidualColumnType.StdPearsonResidual))
            resnames = Matrix.ConcatArrays(resnames, {"DevianceResiduals", "StdDevianceResiduals", "Leverage"})
            For i = 0 To n - 1
                tmp2(i, 0) = Me.pResiduals.DevianceResiduals(i)
                tmp2(i, 1) = Me.pResiduals.StdDevianceResiduals(i)
                tmp2(i, 2) = Me.pResiduals.Leverage(i)
            Next
            t.SetBody(Matrix.VerticalStackArrays(tmp, tmp2))
            t.AddHeaderTopRow(resnames)

            Return t.returnSelf()
        End Function

        Public Function wrapResults(Optional strOffsetVar As String = "",
                                Optional strWeightsVar As String = "") As List(Of ResultTable)
            Dim out As New List(Of ResultTable), t = New ResultTable

            'coefficients, SE table
            t = Me.results.CoeffsZ_toPrint()
            t.AddPvalueToFormat(4)
            If strOffsetVar IsNot Nothing Then t.AddFootnote($"Offset Variable: {strOffsetVar}")
            If strWeightsVar IsNot Nothing Then t.AddFootnote($"Weights Variable: {strWeightsVar}")
            If Me.startParams IsNot Nothing Then t.AddFootnote($"Starting values: {Matrix.array2str(Me.startParams)}")
            t.AddFootnote($"Reference category = {pCats(pCats.Length - 1)}")
            t.AddFootnote($"Computational time: {Me.CompTime} seconds.")
            out.Add(t)

            'Odds rations
            If Me.p > 0 Then out.Add(Me.results.OR_toPrint) 'if intercept only then there is nothing to output

            'Model Info
            out.Add(Me.results.getModelDiagnasticTable_toPrint())

            'Classification accuracy
            t = New ResultTable
            Dim o2(Me.pPredAccuracy.PrecisionPct.Length, Me.pPredAccuracy.PrecisionPct.Length) As Double
            For i = 0 To Me.pPredAccuracy.PrecisionPct.Length
                For j = 0 To Me.pPredAccuracy.PrecisionPct.Length
                    If i < Me.pPredAccuracy.PrecisionPct.Length Then
                        o2(i, j) = If(j = Me.pPredAccuracy.PrecisionPct.Length, Me.pPredAccuracy.RecallPct(i), Me.pPredAccuracy.Counts(i, j))
                    Else
                        o2(i, j) = If(j = Me.pPredAccuracy.PrecisionPct.Length, Me.pPredAccuracy.OverallAccuracyPct, Me.pPredAccuracy.ColTotalsPrct(j))
                    End If
                Next
            Next

            t.SetBody(o2)
            Dim strCats(UBound(Me.pCats)) As String, strCats2(UBound(Me.pCats) + 2) As String
            For i = 0 To UBound(pCats) : strCats(i) = pCats(i).ToString : Next
            strCats2(1) = "Predicted"
            t.AddHeaderTopRow(strCats2)
            t.AddHeaderTopRow(Matrix.ConcatArrays(Matrix.ConcatArrays({"Observed"}, strCats), {"Classification Accuracy"}))
            t.AddHeaderLeftRow(Matrix.ConcatArrays(strCats, {"Overall Percentage"}))
            out.Add(t)

            'iteration info
            If Me.bIterationDetails Then
                t = New ResultTable
                t.SetBody(Me.pItInfo)
                Dim ItLabels(Me.pIteration - 1) As String
                For i = 0 To Me.pIteration - 1 : ItLabels(i) = $"Iteration {i + 1}" : Next
                t.AddHeaderTopRow(ItLabels)
                Dim vars = Matrix.ConcatArrays(Me.results.varNames, {"LogLikelihood", "LogLikelihood Change"})
                t.AddHeaderLeftRow(vars)
                out.Add(t)
            End If

            'Return covariance
            If Me.bReturnCov Then
                t = New ResultTable
                t.SetBody(Me.pCov)
                Dim h(Me.results.varNames.Length) As String
                h(0) = "Covariance matrix of parameters"
                t.AddHeaderTopRow(h)
                t.AddHeaderTopRow(Me.results.varNames)
                t.AddHeaderLeftRow(Me.results.varNames)
                out.Add(t)
            End If

            Return out
        End Function
        ' ----------------------- Fit -----------------------

        ''' <summary>
        ''' Fits the proportional-odds ordinal logistic model by maximizing the weighted log-likelihood.
        ''' </summary>
        Public Sub Fit(Optional reference As ReferenceCategory = ReferenceCategory.Last,
                         Optional bStartParams As Boolean = False,
                         Optional progressBar As System.Windows.Forms.ProgressBar = Nothing,
                         Optional progressLbl As System.Windows.Forms.Label = Nothing)

            If pData Is Nothing Then AppGlobals.BSerr.LogAndThrow(New InvalidOperationException("Data not set. Call Data(...)."))
            Dim startTime As Double = Microsoft.VisualBasic.DateAndTime.Timer
            Me.n = UBound(pData, 1) + 1
            Dim cols As Integer = UBound(pData, 2) + 1
            If cols < 1 Then AppGlobals.BSerr.LogAndThrow(New ArgumentException("Data must have at least 1 column: Y."))

            ' categories and mapping
            Dim catsAsc() As Integer = GetSortedCategoriesFromY()   ' always ascending
            pK = catsAsc.Length
            If pK < 2 Then AppGlobals.BSerr.LogAndThrow(New ArgumentException("Ordinal model requires at least 2 ordered categories."))

            Me.pReference = reference

            ' Internal category order depends on reference:
            ' Last  -> ascending (usual)
            ' First -> descending (reversed order)
            ReDim pCats(pK - 1)
            If reference = ReferenceCategory.Last Then
                Array.Copy(catsAsc, pCats, pK)
            Else
                For i As Integer = 0 To pK - 1
                    pCats(i) = catsAsc(pK - 1 - i)
                Next
            End If

            ' map original category value -> internal index 0..K-1
            Dim map As New Dictionary(Of Integer, Integer)()
            For i As Integer = 0 To pK - 1
                map(pCats(i)) = i
            Next

            ReDim pyFit(n - 1)
            For i As Integer = 0 To n - 1
                Dim yv As Integer = CInt(Math.Round(pData(i, 0)))
                If Not map.ContainsKey(yv) Then AppGlobals.BSerr.LogAndThrow(New ArgumentException($"Unknown category at row {i}."))
                pyFit(i) = map(yv)
            Next

            ' predictors (no intercept column)
            ' predictors (no intercept column) - allow p=0 (intercept-only via thresholds)
            p = cols - 1
            If p <= 0 Then
                ReDim pX(n - 1, 0)  ' dummy, never indexed because loops use 0..p-1
            Else
                ReDim pX(n - 1, p - 1)
                For i As Integer = 0 To n - 1
                    For j As Integer = 0 To p - 1
                        pX(i, j) = pData(i, j + 1)
                    Next
                Next
            End If


            ' parameter vector: [beta (p), alpha (K-1)]
            Dim q As Integer = p + (pK - 1)
            Dim b(q - 1) As Double

            ' Initialize beta = 0 and alpha = logit of cumulative proportions (weighted)
            InitStartParams(b)

            Dim llPrev As Double = Double.NegativeInfinity
            Dim invInfo(,) As Double = Nothing
            pLL = Double.NaN
            ReDim pItInfo(q + 1, pMaxiter) 'parameters, LL, LLchange
            Dim converged As Boolean = False

            For pIteration = 0 To pMaxiter

                Dim g(q - 1) As Double
                Dim H(q - 1, q - 1) As Double ' Hessian of loglik
                Dim ll As Double = 0.0

                For i As Integer = 0 To n - 1
                    Dim wi As Double = If(pbWeights, pWeights(i), 1.0)
                    If wi <= 0.0 Then Continue For

                    Dim eta As Double = LinPred(i, b) ' xβ + offset

                    Dim alpha() As Double = ExtractAlpha(b) ' length K-1
                    If Not IsStrictlyIncreasing(alpha) Then
                        ll = Double.NegativeInfinity
                        Exit For
                    End If

                    Dim gk(pK - 2) As Double, fk(pK - 2) As Double, sk(pK - 2) As Double
                    For k As Integer = 0 To pK - 2
                        Dim t As Double = alpha(k) - eta
                        Dim cdf As Double = regression.Logit.LogisticStable(t)          ' F(t)
                        gk(k) = cdf
                        Dim pdf As Double = cdf * (1.0 - cdf)      ' f(t) = F(1-F)
                        fk(k) = pdf
                        sk(k) = 1.0 - 2.0 * cdf                   ' 1 - 2F
                    Next

                    Dim yi As Integer = pyFit(i)
                    Dim py As Double
                    Dim dp_deta As Double, d2p_deta2 As Double
                    Dim dp_da() As Double = Nothing
                    Dim d2p_da2() As Double = Nothing
                    Dim d2p_deta_da() As Double = Nothing
                    Dim involvedA() As Integer = Nothing

                    GetCategoryDerivatives(yi, gk, fk, sk,
                                       py,
                                       dp_deta, d2p_deta2,
                                       involvedA, dp_da, d2p_da2, d2p_deta_da)

                    If Double.IsNaN(py) Then
                        ll = Double.NegativeInfinity
                        Exit For
                    End If

                    Dim pySafe As Double = Math.Max(py, 1.0E-300R)

                    ll += wi * Math.Log(pySafe)

                    ' ---- gradient ----
                    Dim dlogp_deta As Double = dp_deta / pySafe
                    For col As Integer = 0 To p - 1
                        g(col) += wi * pX(i, col) * dlogp_deta
                    Next

                    ' thresholds gradient (alpha-space)
                    Dim dlogp_da_alpha(pK - 2) As Double ' mostly zeros
                    For tIdx As Integer = 0 To involvedA.Length - 1
                        Dim aIdx As Integer = involvedA(tIdx)
                        dlogp_da_alpha(aIdx) = dp_da(tIdx) / pySafe
                    Next
                    For aIdx As Integer = 0 To pK - 2
                        g(p + aIdx) += wi * dlogp_da_alpha(aIdx)
                    Next

                    ' ---- Hessian ----
                    Dim d2logp_deta2 As Double = (d2p_deta2 / pySafe) - (dp_deta * dp_deta) / (pySafe * pySafe)

                    ' beta-beta block
                    For u As Integer = 0 To p - 1
                        Dim xu As Double = pX(i, u)
                        For v As Integer = 0 To p - 1
                            H(u, v) += wi * d2logp_deta2 * xu * pX(i, v)
                        Next
                    Next

                    ' alpha-alpha & beta-alpha blocks (only for involved thresholds)
                    ' First build the small alpha Hessian contributions in alpha-space.
                    Dim aH(pK - 2, pK - 2) As Double ' sparse but small K; keep simple here
                    For a1 As Integer = 0 To pK - 2
                        If dlogp_da_alpha(a1) <> 0.0 Then
                            ' diagonal term uses d2p/da^2 (if the threshold is involved); otherwise 0.
                            Dim d2p As Double = 0.0
                            For tIdx As Integer = 0 To involvedA.Length - 1
                                If involvedA(tIdx) = a1 Then
                                    d2p = d2p_da2(tIdx)
                                    Exit For
                                End If
                            Next
                            Dim diag As Double = (d2p / pySafe) - (dpFromAlphaIndex(involvedA, dp_da, a1) ^ 2) / (pySafe * pySafe)
                            aH(a1, a1) = wi * diag
                        End If
                    Next
                    ' off-diagonal between the (at most) two involved thresholds:
                    If involvedA.Length = 2 Then
                        Dim a0 As Integer = involvedA(0)
                        Dim a1 As Integer = involvedA(1)
                        Dim off As Double = -wi * (dp_da(0) * dp_da(1)) / (pySafe * pySafe) ' since mixed second derivative of p is 0
                        aH(a0, a1) = off
                        aH(a1, a0) = off
                    End If

                    ' add alpha-alpha block
                    For a1 As Integer = 0 To pK - 2
                        For a2 As Integer = 0 To pK - 2
                            If aH(a1, a2) <> 0.0 Then
                                H(p + a1, p + a2) += aH(a1, a2)
                            End If
                        Next
                    Next

                    ' beta-alpha cross: d2logp/deta/da = (d2p/deta/da)/p - (dp_deta*dp_da)/p^2
                    For tIdx As Integer = 0 To involvedA.Length - 1
                        Dim aIdx As Integer = involvedA(tIdx)
                        Dim d2logp_deta_da As Double =
                    (d2p_deta_da(tIdx) / pySafe) - (dp_deta * dp_da(tIdx)) / (pySafe * pySafe)

                        For col As Integer = 0 To p - 1
                            Dim v As Double = wi * d2logp_deta_da * pX(i, col)
                            H(col, p + aIdx) += v
                            H(p + aIdx, col) += v
                        Next
                    Next

                Next

                If Double.IsNegativeInfinity(ll) Then
                    AppGlobals.BSerr.LogAndThrow(New ApplicationException("OrdinalLogit: invalid step (probability <= 0 or thresholds not increasing)."))
                End If

                ' information matrix = -H + ridge*I
                Dim info(q - 1, q - 1) As Double
                For r As Integer = 0 To q - 1
                    For c As Integer = 0 To q - 1
                        info(r, c) = -H(r, c)
                    Next
                    info(r, r) += pRidge
                Next

                invInfo = Matrix.MatInv(info, "CHOL")
                Dim stepVec() As Double = CategoricalLogitUtils.MatTimesVec(invInfo, g)

                ' line search on b + s*step
                Dim stepScale As Double = 1.0
                Dim bTry(q - 1) As Double
                Dim llTry As Double

                Do
                    For j As Integer = 0 To q - 1
                        bTry(j) = b(j) + stepScale * stepVec(j)
                    Next
                    llTry = ComputeLogLik(bTry)
                    If llTry >= ll OrElse stepScale <= 0.000001 Then Exit Do
                    stepScale *= 0.5
                Loop

                Array.Copy(bTry, b, q)
                pLL = llTry
                pLastIterLLchange = Math.Abs(pLL - llPrev)

                If progressBar IsNot Nothing Then
                    progressBar.Invoke(Sub()
                                           progressBar.Value = CInt(100.0 * (Me.pIteration + 1.0) / (Me.pMaxiter + 1.0))
                                           If progressLbl IsNot Nothing Then progressLbl.Text = $"Elapsed Time: {Math.Round((Microsoft.VisualBasic.DateAndTime.Timer - startTime), 2)}[s]   Iterations: {Me.pIteration + 1}   LogLikelihood change = {pLastIterLLchange}"
                                       End Sub)
                    System.Windows.Forms.Application.DoEvents()
                End If

                'save iteration info
                For i = 0 To q + 1
                    If i = q Then 'LL
                        pItInfo(i, pIteration) = Me.pLL
                    ElseIf i = q + 1 Then 'LL change
                        pItInfo(i, pIteration) = pLastIterLLchange
                    Else 'parameters
                        pItInfo(i, pIteration) = b(i)
                    End If
                Next

                Dim stepNorm As Double = CategoricalLogitUtils.MaxAbs(stepVec) * stepScale
                If stepNorm < pEps OrElse pLastIterLLchange < pEps Then
                    converged = True
                    Exit For
                End If

                llPrev = pLL
            Next pIteration
            If pIteration > -1 Then ReDim Preserve pItInfo(UBound(pItInfo, 1), pIteration)
            pIteration += 1
            If Not converged Then AppGlobals.BSlogg.Log("Algorithm Is diverging. Convergence not reached.", AppGlobals.LogMsgType.Warn)

            ' store covariance at final b (one-pass behind is usually tiny at convergence,
            ' but we set pCov here to the last invInfo computed in-loop)
            pCov = invInfo

            ' build coefficient names and SEs
            Dim se(q - 1) As Double
            If invInfo IsNot Nothing Then
                For i As Integer = 0 To q - 1
                    se(i) = Math.Sqrt(Math.Max(0.0, invInfo(i, i)))
                Next
            End If

            Dim coefNames(q - 1) As String
            For j As Integer = 0 To p - 1
                coefNames(j) = "β: " & GetPredictorName(j)
            Next
            For thr As Integer = 0 To pK - 2
                Dim boundaryValue As Integer = pCats(thr)

                If pReference = ReferenceCategory.Last Then
                    ' Usual ascending order: cumulative probability up to boundary
                    coefNames(p + thr) = $"α{thr + 1}: cutpoint for P(Y ≤ {boundaryValue})"
                Else
                    ' Reversed order: interpretation flips; this cutpoint corresponds to upper tail on original scale
                    coefNames(p + thr) = $"α{thr + 1}: cutpoint (reversed scale) for P(Y ≥ {boundaryValue})"
                End If
            Next

            results = New LMresult()
            results.n = n
            Me.results.alpha = pAlpha
            results.bIntercept = False
            results.varNames = coefNames
            results.Coeffs_est = b
            results.Coeffs_SEs = se

            ComputeFitStatistics(b)
            pPredAccuracy = ComputeClassificationCrosstab(b)

            Me.results.ModelTableLabels = {"Null Log Likelihood", "Final Log Likelihood",
                "# observations", "Likelihood Ratio Test chisq", "Deviance Goodnes-of-Fit chisq",
                "Pseudo(Cox and Snell) R²", "Pseudo(Nagelkerke) R²", "Pseudo(McFadden) R²", "AIC", "BIC",
                "Number of Iterations", "Relative Log - Likelihood Change", "Converged?"}

            Me.results.ModelTableVals = {{Me.pLL0, "", ""},
                                     {Me.pLL, "", ""},
                                     {Me.n, "", ""},
                                     {Me.pModelChi2.TestStatistics1, Me.pModelChi2.DF1, Me.pModelChi2.Pvalue},
                                     {Me.pGOF.TestStatistics1, Me.pGOF.DF1, Me.pGOF.Pvalue},
                                     {Me.pCoxSnellR2, "", ""},
                                     {Me.pNagelkerkeR2, "", ""},
                                     {Me.pMcFaddenR2, "", ""},
                                     {Me.pAIC, Me.results.Coeffs_est.Length, ""},
                                     {Me.pBIC, Me.results.Coeffs_est.Length, ""},
                                     {Me.pIteration, "", ""},
                                     {Me.pLastIterLLchange, "", ""},
                                     {CStr(converged), "", ""}}

            If bComputeResiduals Then
                pResiduals = ComputeResiduals(b, useWeights:=True, includeAllCategories:=True)
            End If

            Me.CompTime = Microsoft.VisualBasic.DateAndTime.Timer - startTime
            If progressBar IsNot Nothing Then progressBar.Invoke(Sub()
                                                                     progressBar.Value = 100
                                                                 End Sub)
        End Sub

        ' ----------------------- Residuals -----------------------

        ''' <summary>
        ''' Computes fitted probabilities and standard residuals (same shapes as in your multinomial model).
        ''' </summary>
        ''' <param name="b">Parameter vector [β, α].</param>
        ''' <param name="useWeights">If True uses m_i=weights(i) as counts; else m_i=1.</param>
        ''' <param name="includeAllCategories">If True returns K columns; if False returns K-1 (drops last category).</param>
        ''' <remarks>
        ''' <para>
        ''' Observed y_{ik} is one-hot (or weighted by m_i). μ_{ik}=m_i p_{ik}.
        ''' Pearson residual uses Var(y_{ik}) ≈ m_i p_{ik}(1-p_{ik}).
        ''' </para>
        ''' <para>
        ''' Deviance contribution per observation (grouped-multinomial form):
        ''' D_i = 2 Σ_k y_{ik} log(y_{ik}/μ_{ik}), ignoring terms with y_{ik}=0.
        ''' Deviance residual returned is sqrt(D_i) (nonnegative for one-trial records).
        ''' </para>
        ''' <para>
        ''' Leverage here is computed as h_i = tr(I_i * Cov), where I_i is the per-observation observed-information
        ''' contribution (a q×q matrix) and Cov ≈ I^{-1}. This is a common generalized leverage proxy; it may exceed 1.
        ''' Standardization uses sqrt(1-h_i) when h_i&lt;1; otherwise standardized residuals are returned as NaN.
        ''' </para>
        ''' </remarks>
        Private Function ComputeResiduals(b() As Double,
                                  Optional useWeights As Boolean = True,
                                  Optional includeAllCategories As Boolean = True) As MultinomialResiduals
            Dim K As Integer = pK
            Dim colsK As Integer = If(includeAllCategories, K, K - 1)
            Dim out As New MultinomialResiduals()
            out.Categories = DirectCast(pCats.Clone(), Integer())

            ReDim out.Observed(n - 1, colsK - 1)
            ReDim out.Probabilities(n - 1, colsK - 1)
            ReDim out.FittedMeans(n - 1, colsK - 1)
            ReDim out.ResponseResiduals(n - 1, colsK - 1)
            ReDim out.PearsonResiduals(n - 1, colsK - 1)
            ReDim out.StdPearsonResiduals(n - 1, colsK - 1)
            ReDim out.DevianceContrib(n - 1)
            ReDim out.DevianceResiduals(n - 1)
            ReDim out.StdDevianceResiduals(n - 1)
            ReDim out.Leverage(n - 1)

            ' probabilities
            For i As Integer = 0 To n - 1
                Dim pi() As Double = PredictRowProbs(i, b) ' length K
                For cat As Integer = 0 To colsK - 1
                    out.Probabilities(i, cat) = pi(cat)
                Next
            Next

            ' residuals
            For i As Integer = 0 To n - 1
                Dim m_i As Double = If(useWeights AndAlso pbWeights, pWeights(i), 1.0)
                If m_i <= 0.0 Then
                    out.DevianceContrib(i) = 0.0
                    out.DevianceResiduals(i) = 0.0
                    out.StdDevianceResiduals(i) = Double.NaN
                    out.Leverage(i) = Double.NaN
                    Continue For
                End If

                Dim obsIdx As Integer = pyFit(i)

                For cat As Integer = 0 To colsK - 1
                    Dim pik As Double = out.Probabilities(i, cat)

                    out.Observed(i, cat) = If(obsIdx = cat, m_i, 0.0)
                    out.FittedMeans(i, cat) = m_i * pik
                    out.ResponseResiduals(i, cat) = out.Observed(i, cat) - out.FittedMeans(i, cat)
                    out.PearsonResiduals(i, cat) = out.ResponseResiduals(i, cat) / Math.Sqrt(Math.Max(1.0E-300R, m_i * pik * (1.0 - pik)))
                Next

                Dim Di As Double = 0.0
                For cat As Integer = 0 To colsK - 1
                    Dim yik As Double = out.Observed(i, cat)
                    If yik > 0.0 Then
                        Dim muik As Double = Math.Max(1.0E-300R, out.FittedMeans(i, cat))
                        Di += 2.0 * yik * Math.Log(yik / muik)
                    End If
                Next
                out.DevianceContrib(i) = Di
                out.DevianceResiduals(i) = Math.Sqrt(Math.Max(0.0, Di))
            Next

            ' leverage + standardization
            If pCov IsNot Nothing Then
                Dim h() As Double = ComputeGeneralizedLeverage(b)
                For i As Integer = 0 To n - 1
                    out.Leverage(i) = h(i)

                    If h(i) < 1.0 AndAlso h(i) >= 0.0 Then
                        Dim adj As Double = Math.Sqrt(Math.Max(0.000000000001, 1.0 - h(i)))
                        For cat As Integer = 0 To colsK - 1
                            out.StdPearsonResiduals(i, cat) = out.PearsonResiduals(i, cat) / adj
                        Next
                        out.StdDevianceResiduals(i) = out.DevianceResiduals(i) / adj
                    Else
                        For cat As Integer = 0 To colsK - 1
                            out.StdPearsonResiduals(i, cat) = Double.NaN
                        Next
                        out.StdDevianceResiduals(i) = Double.NaN
                    End If
                Next
            End If

            Return out
        End Function

        ' ----------------------- Residual column names -----------------------

        ''' <summary>
        ''' Generates residual column names in the same category order as residual matrices:
        ''' categories are in ascending order (pCats), columns correspond to internal indices 0..K-1.
        ''' </summary>
        Public Function GetResidualColumnNames(resType As ResidualColumnType) As String()

            If pCats Is Nothing OrElse pCats.Length = 0 Then AppGlobals.BSerr.LogAndThrow(New InvalidOperationException("Categories not available. Fit the model first."))

            Dim cols As Integer = pK
            Dim names(cols - 1) As String
            Dim prefix As String = ResidualTypePrefix(resType)
            For i As Integer = 0 To cols - 1
                names(i) = prefix & ": cat=" & pCats(i).ToString(CultureInfo.InvariantCulture)
            Next

            Return names
        End Function

        ''' <summary>Returns the prefix used for residual column naming.</summary>
        Private Function ResidualTypePrefix(resType As ResidualColumnType) As String
            Select Case resType
                Case ResidualColumnType.Observed : Return "Observed"
                Case ResidualColumnType.FittedProbability : Return "FittedProb"
                Case ResidualColumnType.FittedMean : Return "FittedMean"
                Case ResidualColumnType.ResponseResidual : Return "ResponseResidual"
                Case ResidualColumnType.PearsonResidual : Return "PearsonResidual"
                Case ResidualColumnType.StdPearsonResidual : Return "StdPearsonResidual"
                Case Else : Return "Residual"
            End Select
        End Function

        ' ----------------------- Post-fit statistics -----------------------

        ''' <summary>
        ''' Computes LL0, AIC/BIC, pseudo-R², LR model χ², and profile-based deviance GOF.
        ''' </summary>
        Private Sub ComputeFitStatistics(b() As Double)

            Dim ll1 As Double = ComputeLogLik(b)
            pLL = ll1

            Dim ll0 As Double = FitNullLogLik()
            pLL0 = ll0

            Dim kFull As Integer = b.Length
            Dim kNull As Integer = (pK - 1)
            Dim nobs As Double = Math.Max(1.0, If(pbWeights, SumWeightsPositive(), CDbl(n)))

            pAIC = -2.0 * ll1 + 2.0 * kFull
            pBIC = -2.0 * ll1 + Math.Log(nobs) * kFull

            pCoxSnellR2 = 1.0 - Math.Exp((2.0 / nobs) * (ll0 - ll1))
            Dim denomNk As Double = 1.0 - Math.Exp((2.0 / nobs) * ll0)
            pNagelkerkeR2 = If(Math.Abs(denomNk) > 0.00000000000001R, pCoxSnellR2 / denomNk, Double.NaN)
            pMcFaddenR2 = If(Math.Abs(ll0) > 0.00000000000001R, 1.0 - (ll1 / ll0), Double.NaN)

            pModelChi2 = New TestResult()
            pModelChi2.TestStatistics1 = 2.0 * (ll1 - ll0)
            Dim df As Integer = kFull - kNull
            If df <= 0 Then
                Me.pModelChi2.DF1 = 0
                Me.pModelChi2.Pvalue = Double.NaN
                ' chisq will be 0 (up to floating error) for intercept-only
            Else
                Me.pModelChi2.DF1 = df
                Me.pModelChi2.Pvalue = 1.0 - distributions.ChiSquareCDF(Me.pModelChi2.TestStatistics1, df)
            End If

            pGOF = ComputeProfileDevianceGOF(b)
        End Sub

        ''' <summary>
        ''' Profile-based deviance goodness-of-fit:
        ''' D = 2(ℓ_sat - ℓ_model) with df = G(K-1) - kFull,
        ''' where G is the number of unique covariate patterns (and offset if included).
        ''' </summary>
        Private Function ComputeProfileDevianceGOF(b() As Double,
                                               Optional keyDigits As Integer = 12) As TestResult

            Dim out As New TestResult With {.TestStatistics1 = Double.NaN, .DF1 = 0, .Pvalue = Double.NaN}
            Dim kFull As Integer = b.Length

            Dim groups As New Dictionary(Of String, GroupCell)()

            For i As Integer = 0 To n - 1
                Dim wi As Double = If(pbWeights, pWeights(i), 1.0)
                If wi <= 0.0R Then Continue For

                Dim key As String = BuildPatternKey(i, keyDigits)
                Dim cell As GroupCell = Nothing
                If Not groups.TryGetValue(key, cell) Then
                    cell = New GroupCell(pK, i)
                    groups.Add(key, cell)
                End If
                cell.TotalW += wi
                cell.Counts(pyFit(i)) += wi
            Next

            Dim G As Integer = groups.Count
            If G <= 0 Then Return out

            Dim llSat As Double = 0.0
            Dim llModel As Double = 0.0

            For Each kv In groups
                Dim cell As GroupCell = kv.Value
                Dim m As Double = cell.TotalW
                If m <= 0.0 Then Continue For

                Dim pi() As Double = PredictRowProbs(cell.RepRow, b)

                For k As Integer = 0 To pK - 1
                    Dim yk As Double = cell.Counts(k)
                    If yk > 0.0 Then
                        llModel += yk * Math.Log(Math.Max(pi(k), 1.0E-300R))
                        llSat += yk * Math.Log(Math.Max(yk / m, 1.0E-300R))
                    End If
                Next
            Next

            out.TestStatistics1 = 2.0 * (llSat - llModel)
            out.DF1 = Math.Max(1, G * (pK - 1) - kFull)
            out.Pvalue = 1.0 - distributions.ChiSquareCDF(out.TestStatistics1, out.DF1)
            Return out
        End Function

        ' ----------------------- Classification -----------------------

        ''' <summary>
        ''' Builds a confusion matrix (observed x predicted) using argmax_k p_{ik}.
        ''' </summary>
        Private Function ComputeClassificationCrosstab(b() As Double,
                                               Optional useWeights As Boolean = True,
                                               Optional tieBreakToSmallestIndex As Boolean = True) As ClassificationCrosstab

            Dim out As New ClassificationCrosstab()
            out.Categories = DirectCast(pCats.Clone(), Integer())

            ReDim out.Counts(pK - 1, pK - 1)
            ReDim out.RowTotals(pK - 1)
            ReDim out.ColTotals(pK - 1)
            ReDim out.RecallPct(pK - 1)
            ReDim out.PrecisionPct(pK - 1)
            ReDim out.ColTotalsPrct(pK - 1)

            Dim total As Double = 0.0
            Dim correct As Double = 0.0

            For i As Integer = 0 To n - 1
                Dim wi As Double = If(useWeights AndAlso pbWeights, pWeights(i), 1.0)
                If wi <= 0.0 Then Continue For

                Dim obs As Integer = pyFit(i)
                Dim pi() As Double = PredictRowProbs(i, b)
                Dim pred As Integer = CategoricalLogitUtils.ArgMax(pi, tieBreakToSmallestIndex)

                out.Counts(obs, pred) += wi
                out.RowTotals(obs) += wi
                out.ColTotals(pred) += wi

                total += wi
                If obs = pred Then correct += wi
            Next

            out.OverallAccuracy = If(total > 0.0, correct / total, Double.NaN)
            out.OverallAccuracyPct = 100.0 * out.OverallAccuracy

            For k As Integer = 0 To pK - 1
                Dim diag As Double = out.Counts(k, k)
                out.RecallPct(k) = If(out.RowTotals(k) > 0.0, 100.0 * diag / out.RowTotals(k), Double.NaN)
                out.PrecisionPct(k) = If(out.ColTotals(k) > 0.0, 100.0 * diag / out.ColTotals(k), Double.NaN)
                out.ColTotalsPrct(k) = If(total > 0.0, 100.0 * out.ColTotals(k) / total, Double.NaN)
            Next

            Return out
        End Function

        ' ----------------------- Core probability helpers -----------------------

        ''' <summary>
        ''' Computes p(Y=k) for all categories for a given row i.
        ''' </summary>
        ''' <param name="row">Row index i.</param>
        ''' <param name="b">Parameters [β, α].</param>
        ''' <returns>Probability vector length K.</returns>
        Private Function PredictRowProbs(row As Integer, b() As Double) As Double()
            Dim eta As Double = LinPred(row, b)
            Dim alpha() As Double = ExtractAlpha(b)
            Dim probs(pK - 1) As Double
            Dim prev As Double = 0.0

            For k As Integer = 0 To pK - 2
                Dim Fk As Double = regression.Logit.LogisticStable(alpha(k) - eta)
                Dim pk As Double = If(k = 0, Fk, Fk - prev)
                probs(k) = Math.Max(0.0R, pk)
                prev = Fk
            Next

            probs(pK - 1) = Math.Max(0.0, 1.0 - prev)

            ' Normalize defensively (can be slightly off due to rounding)
            Dim s As Double = 0.0
            For k As Integer = 0 To pK - 1 : s += probs(k) : Next
            If s > 0.0 Then
                For k As Integer = 0 To pK - 1 : probs(k) /= s : Next
            End If

            Return probs
        End Function

        ''' <summary>Linear predictor η_i = x_i^T β + offset_i.</summary>
        Private Function LinPred(i As Integer, b() As Double) As Double
            Dim s As Double = 0.0
            For col As Integer = 0 To p - 1
                s += pX(i, col) * b(col)
            Next
            If pbOffset Then s += pOffset(i)
            Return s
        End Function

        ''' <summary>
        ''' Extracts thresholds α_1..α_{K-1} from parameter vector b (stored as the last K-1 elements).
        ''' </summary>
        Private Function ExtractAlpha(b() As Double) As Double()
            Dim alpha(pK - 2) As Double
            For k As Integer = 0 To pK - 2
                alpha(k) = b(p + k)
            Next
            Return alpha
        End Function

        ''' <summary>Returns True if alpha(0) &lt; alpha(1) &lt; ... &lt; alpha(K-2).</summary>
        Private Function IsStrictlyIncreasing(alpha() As Double) As Boolean
            For k As Integer = 1 To alpha.Length - 1
                If Not (alpha(k) > alpha(k - 1)) Then Return False
            Next
            Return True
        End Function


        ' ----------------------- Derivative helpers (per category) -----------------------

        ''' <summary>
        ''' Computes p_y and first/second derivatives of p_y with respect to η and the involved thresholds.
        ''' Only up to two thresholds affect a given category probability.
        ''' </summary>
        ''' <param name="yIdx">Observed category index (0..K-1).</param>
        ''' <param name="gk">gk(j)=F(α_{j+1}-η), length K-1.</param>
        ''' <param name="fk">fk(j)=gk(j)(1-gk(j)), length K-1.</param>
        ''' <param name="sk">sk(j)=1-2*gk(j), length K-1.</param>
        ''' <param name="py">Output p(Y=y).</param>
        ''' <param name="dp_deta">Output ∂p/∂η.</param>
        ''' <param name="d2p_deta2">Output ∂²p/∂η².</param>
        ''' <param name="involvedA">Output threshold indices involved (0..K-2).</param>
        ''' <param name="dp_da">Output ∂p/∂α for involved thresholds (same order as involvedA).</param>
        ''' <param name="d2p_da2">Output ∂²p/∂α² for involved thresholds.</param>
        ''' <param name="d2p_deta_da">Output mixed ∂²p/∂η∂α for involved thresholds.</param>
        Private Sub GetCategoryDerivatives(yIdx As Integer, gk() As Double, fk() As Double, sk() As Double,
                                       ByRef py As Double,
                                       ByRef dp_deta As Double, ByRef d2p_deta2 As Double,
                                       ByRef involvedA() As Integer,
                                       ByRef dp_da() As Double,
                                       ByRef d2p_da2() As Double,
                                       ByRef d2p_deta_da() As Double)

            Dim Km1 As Integer = pK - 1
            If involvedA Is Nothing Then involvedA = Array.Empty(Of Integer)()
            If dp_da Is Nothing Then dp_da = Array.Empty(Of Double)()
            If d2p_da2 Is Nothing Then d2p_da2 = Array.Empty(Of Double)()
            If d2p_deta_da Is Nothing Then d2p_deta_da = Array.Empty(Of Double)()

            If yIdx = 0 Then
                ' p1 = g1
                py = gk(0)
                dp_deta = -fk(0)
                d2p_deta2 = fk(0) * sk(0)

                involvedA = New Integer() {0}
                dp_da = New Double() {fk(0)}
                d2p_da2 = New Double() {fk(0) * sk(0)}
                d2p_deta_da = New Double() {-fk(0) * sk(0)}
                Return
            End If

            If yIdx = pK - 1 Then
                ' pK = 1 - g_{K-1}
                Dim j As Integer = Km1 - 1
                py = 1.0 - gk(j)
                dp_deta = fk(j)
                d2p_deta2 = -fk(j) * sk(j)

                involvedA = New Integer() {j}
                dp_da = New Double() {-fk(j)}
                d2p_da2 = New Double() {-fk(j) * sk(j)}
                d2p_deta_da = New Double() {fk(j) * sk(j)}
                Return
            End If

            ' middle category: p = g_y - g_{y-1} (with 1-based y)
            Dim k As Integer = yIdx ' 0-based category yIdx corresponds to boundary index yIdx for gk
            ' Example: yIdx=1 (2nd category) uses g1-g0 -> indices 1 and 0
            py = gk(k) - gk(k - 1)
            dp_deta = -(fk(k) - fk(k - 1))
            d2p_deta2 = fk(k) * sk(k) - fk(k - 1) * sk(k - 1)

            involvedA = New Integer() {k, k - 1}
            dp_da = New Double() {fk(k), -fk(k - 1)}
            d2p_da2 = New Double() {fk(k) * sk(k), -fk(k - 1) * sk(k - 1)}
            d2p_deta_da = New Double() {-fk(k) * sk(k), fk(k - 1) * sk(k - 1)}
        End Sub

        ''' <summary>
        ''' Returns dp/da for a given alpha index (used in alpha diagonal Hessian term).
        ''' </summary>
        Private Function dpFromAlphaIndex(involvedA() As Integer, dp_da() As Double, aIdx As Integer) As Double
            For t As Integer = 0 To involvedA.Length - 1
                If involvedA(t) = aIdx Then Return dp_da(t)
            Next
            Return 0.0
        End Function

        ' ----------------------- Likelihood -----------------------

        ''' <summary>
        ''' Computes the weighted log-likelihood ℓ for parameters b.
        ''' </summary>
        Private Function ComputeLogLik(b() As Double) As Double
            Dim alpha() As Double = ExtractAlpha(b)
            If Not IsStrictlyIncreasing(alpha) Then Return Double.NegativeInfinity

            Dim ll As Double = 0.0
            For i As Integer = 0 To n - 1
                Dim wi As Double = If(pbWeights, pWeights(i), 1.0)
                If wi <= 0.0 Then Continue For

                Dim pi() As Double = PredictRowProbs(i, b)
                Dim yi As Integer = pyFit(i)
                Dim pyi As Double = pi(yi)
                If pyi <= 0.0 Then Return Double.NegativeInfinity

                ll += wi * Math.Log(Math.Max(pyi, 1.0E-300R))
            Next
            Return ll
        End Function

        ''' <summary>
        ''' Fits the null (threshold-only) model and returns ℓ0.
        ''' </summary>
        Private Function FitNullLogLik() As Double
            ' Parameters: alpha only (K-1). We optimize alpha with beta=0.
            Dim q0 As Integer = pK - 1
            Dim a(q0 - 1) As Double
            InitAlphaOnly(a)

            Dim llPrev As Double = Double.NegativeInfinity
            Dim llFinal As Double = Double.NaN

            For it As Integer = 1 To Math.Min(60, pMaxiter)

                Dim g(q0 - 1) As Double
                Dim H(q0 - 1, q0 - 1) As Double
                Dim ll As Double = 0.0

                For i As Integer = 0 To n - 1
                    Dim wi As Double = If(pbWeights, pWeights(i), 1.0)
                    If wi <= 0.0 Then Continue For

                    Dim eta As Double = If(pbOffset, pOffset(i), 0.0)

                    If Not IsStrictlyIncreasing(a) Then
                        ll = Double.NegativeInfinity
                        Exit For
                    End If

                    Dim gk(pK - 2) As Double, fk(pK - 2) As Double, sk(pK - 2) As Double
                    For k As Integer = 0 To pK - 2
                        Dim cdf As Double = regression.Logit.LogisticStable(a(k) - eta)
                        gk(k) = cdf
                        Dim pdf As Double = cdf * (1.0 - cdf)
                        fk(k) = pdf
                        sk(k) = 1.0 - 2.0 * cdf
                    Next

                    Dim yi As Integer = pyFit(i)
                    Dim py As Double
                    Dim dp_deta As Double, d2p_deta2 As Double
                    Dim involvedA() As Integer = Nothing, dp_da() As Double = Nothing, d2p_da2() As Double = Nothing, d2p_deta_da() As Double = Nothing
                    GetCategoryDerivatives(yi, gk, fk, sk,
                                       py, dp_deta, d2p_deta2,
                                       involvedA, dp_da, d2p_da2, d2p_deta_da)

                    If Double.IsNaN(py) Then
                        ll = Double.NegativeInfinity
                        Exit For
                    End If

                    Dim pySafe As Double = Math.Max(py, 1.0E-300R)

                    ll += wi * Math.Log(pySafe)

                    ' gradient and Hessian for alpha-only
                    ' alpha gradient: dp/da / p
                    Dim dlogp_da(q0 - 1) As Double
                    For tIdx As Integer = 0 To involvedA.Length - 1
                        dlogp_da(involvedA(tIdx)) = dp_da(tIdx) / pySafe
                    Next
                    For j As Integer = 0 To q0 - 1
                        g(j) += wi * dlogp_da(j)
                    Next

                    ' diagonal terms
                    For tIdx As Integer = 0 To involvedA.Length - 1
                        Dim j As Integer = involvedA(tIdx)
                        Dim d2log As Double =
                    (d2p_da2(tIdx) / pySafe) - (dp_da(tIdx) * dp_da(tIdx)) / (pySafe * pySafe)
                        H(j, j) += wi * d2log
                    Next

                    ' off-diagonal between the two thresholds if present
                    If involvedA.Length = 2 Then
                        Dim j0 As Integer = involvedA(0)
                        Dim j1 As Integer = involvedA(1)
                        Dim off As Double = -wi * (dp_da(0) * dp_da(1)) / (pySafe * pySafe)
                        H(j0, j1) += off
                        H(j1, j0) += off
                    End If
                Next

                If Double.IsNegativeInfinity(ll) Then Exit For

                ' information = -H + ridge I
                Dim info(q0 - 1, q0 - 1) As Double
                For r As Integer = 0 To q0 - 1
                    For c As Integer = 0 To q0 - 1
                        info(r, c) = -H(r, c)
                    Next
                    info(r, r) += pRidge
                Next

                Dim invInfo(,) As Double = Matrix.MatInv(info, "CHOL")
                Dim stepVec() As Double = CategoricalLogitUtils.MatTimesVec(invInfo, g)

                Dim stepScale As Double = 1.0
                Dim aTry(q0 - 1) As Double, llTry As Double

                Do
                    For j As Integer = 0 To q0 - 1
                        aTry(j) = a(j) + stepScale * stepVec(j)
                    Next
                    llTry = ComputeLogLikNullAlphaOnly(aTry)
                    If llTry >= ll OrElse stepScale <= 0.000001 Then Exit Do
                    stepScale *= 0.5
                Loop

                Array.Copy(aTry, a, q0)
                llFinal = llTry

                Dim stepNorm As Double = CategoricalLogitUtils.MaxAbs(stepVec) * stepScale
                If stepNorm < pEps OrElse Math.Abs(llFinal - llPrev) < pEps Then Exit For
                llPrev = llFinal
            Next

            Return llFinal
        End Function

        ''' <summary>
        ''' Log-likelihood for null model given alpha only (beta=0, eta=offset).
        ''' </summary>
        Private Function ComputeLogLikNullAlphaOnly(a() As Double) As Double
            If Not IsStrictlyIncreasing(a) Then Return Double.NegativeInfinity

            Dim ll As Double = 0.0
            For i As Integer = 0 To n - 1
                Dim wi As Double = If(pbWeights, pWeights(i), 1.0)
                If wi <= 0.0 Then Continue For

                Dim eta As Double = If(pbOffset, pOffset(i), 0.0)

                Dim probs(pK - 1) As Double
                Dim prev As Double = 0.0
                For k As Integer = 0 To pK - 2
                    Dim Fk As Double = regression.Logit.LogisticStable(a(k) - eta)
                    probs(k) = If(k = 0, Fk, Fk - prev)
                    prev = Fk
                Next
                probs(pK - 1) = 1.0 - prev

                Dim yi As Integer = pyFit(i)
                Dim pyi As Double = probs(yi)
                If pyi <= 0.0 Then Return Double.NegativeInfinity
                ll += wi * Math.Log(Math.Max(pyi, 1.0E-300R))
            Next

            Return ll
        End Function

        ' ----------------------- Leverage (generalized) -----------------------

        ''' <summary>
        ''' Computes generalized leverage proxy h_i = tr(I_i * Cov), where I_i is the per-observation
        ''' observed information contribution and Cov ≈ I^{-1}.
        ''' </summary>
        Private Function ComputeGeneralizedLeverage(b() As Double) As Double()
            Dim q As Integer = b.Length
            Dim h(n - 1) As Double

            For i As Integer = 0 To n - 1
                Dim wi As Double = If(pbWeights, pWeights(i), 1.0)
                If wi <= 0.0 Then
                    h(i) = Double.NaN
                    Continue For
                End If

                ' Build per-observation Hessian contribution Hi (loglik Hessian)
                Dim Hi(q - 1, q - 1) As Double
                BuildObsHessianContribution(i, b, Hi)

                ' Information contribution I_i = -wi * Hi
                Dim trace As Double = 0.0
                For r As Integer = 0 To q - 1
                    Dim s As Double = 0.0
                    For c As Integer = 0 To q - 1
                        s += (-wi * Hi(r, c)) * pCov(c, r)
                    Next
                    trace += s
                Next
                h(i) = trace
            Next

            Return h
        End Function

        ''' <summary>
        ''' Builds the per-observation log-likelihood Hessian contribution (without multiplying by weight).
        ''' </summary>
        Private Sub BuildObsHessianContribution(i As Integer, b() As Double, ByRef Hobs(,) As Double)
            Dim q As Integer = b.Length
            For r As Integer = 0 To q - 1
                For c As Integer = 0 To q - 1
                    Hobs(r, c) = 0.0
                Next
            Next

            Dim eta As Double = LinPred(i, b)
            Dim alpha() As Double = ExtractAlpha(b)
            If Not IsStrictlyIncreasing(alpha) Then Exit Sub

            Dim gk(pK - 2) As Double, fk(pK - 2) As Double, sk(pK - 2) As Double
            For cat As Integer = 0 To pK - 2
                Dim cdf As Double = regression.Logit.LogisticStable(alpha(cat) - eta)   ' F(t)
                gk(cat) = cdf
                Dim pdf As Double = cdf * (1.0 - cdf)                      ' f(t)=F(1-F)
                fk(cat) = pdf
                sk(cat) = 1.0 - 2.0 * cdf
            Next


            Dim yi As Integer = pyFit(i)
            Dim py As Double
            Dim dp_deta As Double, d2p_deta2 As Double
            Dim involvedA() As Integer = Nothing, dp_da() As Double = Nothing, d2p_da2() As Double = Nothing, d2p_deta_da() As Double = Nothing
            GetCategoryDerivatives(yi, gk, fk, sk,
                               py, dp_deta, d2p_deta2,
                               involvedA, dp_da, d2p_da2, d2p_deta_da)
            If Double.IsNaN(py) Then Exit Sub
            Dim pySafe As Double = Math.Max(py, 1.0E-300R)

            Dim d2logp_deta2 As Double = (d2p_deta2 / pySafe) - (dp_deta * dp_deta) / (pySafe * pySafe)

            ' beta-beta block
            For u As Integer = 0 To p - 1
                Dim xu As Double = pX(i, u)
                For v As Integer = 0 To p - 1
                    Hobs(u, v) += d2logp_deta2 * xu * pX(i, v)
                Next
            Next

            ' beta-alpha and alpha-alpha (involved thresholds only)
            For tIdx As Integer = 0 To involvedA.Length - 1
                Dim aIdx As Integer = involvedA(tIdx)
                Dim d2logp_deta_da As Double =
            (d2p_deta_da(tIdx) / pySafe) - (dp_deta * dp_da(tIdx)) / (pySafe * pySafe)

                For col As Integer = 0 To p - 1
                    Dim v As Double = d2logp_deta_da * pX(i, col)
                    Hobs(col, p + aIdx) += v
                    Hobs(p + aIdx, col) += v
                Next

                Dim d2logp_da2 As Double =
            (d2p_da2(tIdx) / pySafe) - (dp_da(tIdx) * dp_da(tIdx)) / (pySafe * pySafe)

                Hobs(p + aIdx, p + aIdx) += d2logp_da2
            Next

            If involvedA.Length = 2 Then
                Dim a0 As Integer = involvedA(0)
                Dim a1 As Integer = involvedA(1)
                Dim off As Double = -(dp_da(0) * dp_da(1)) / (pySafe * pySafe)
                Hobs(p + a0, p + a1) += off
                Hobs(p + a1, p + a0) += off
            End If
        End Sub

        ' ----------------------- Initialization -----------------------

        ''' <summary>
        ''' Initializes starting parameters b = [β, α] using β=0 and α_k = logit(P(Y ≤ c_{k-1})) (weighted).
        ''' </summary>
        Private Sub InitStartParams(ByRef b() As Double)
            If Me.startParams IsNot Nothing Then
                If Me.startParams.Length <> b.Length Then AppGlobals.BSerr.LogAndThrow(New ArgumentException("starting parameter array length <> b length"))
                Me.startParams.CopyTo(b, 0)
            Else
                For j As Integer = 0 To p - 1
                    b(j) = 0.0
                Next

                Dim alpha(pK - 2) As Double
                InitAlphaOnly(alpha)

                For k As Integer = 0 To pK - 2
                    b(p + k) = alpha(k)
                Next
            End If
        End Sub

        ''' <summary>
        ''' Initializes alpha only using weighted cumulative proportions of the observed outcome.
        ''' </summary>
        Private Sub InitAlphaOnly(ByRef alpha() As Double)
            ' cumulative counts
            Dim counts(pK - 1) As Double
            Dim total As Double = 0.0
            For i As Integer = 0 To n - 1
                Dim wi As Double = If(pbWeights, pWeights(i), 1.0)
                If wi <= 0.0 Then Continue For
                counts(pyFit(i)) += wi
                total += wi
            Next
            If total <= 0.0 Then total = CDbl(n)

            Dim cum As Double = 0.0
            For k As Integer = 0 To pK - 2
                cum += counts(k)
                Dim pCum As Double = cum / total
                pCum = Math.Min(1.0 - 0.000001, Math.Max(0.000001, pCum))
                alpha(k) = Math.Log(pCum / (1.0 - pCum))
                If k > 0 AndAlso alpha(k) <= alpha(k - 1) Then
                    alpha(k) = alpha(k - 1) + 0.5 ' enforce increasing start
                End If
            Next
        End Sub

        ' ----------------------- Misc helpers -----------------------

        Private Function GetPredictorName(j As Integer) As String
            If pVarNames IsNot Nothing AndAlso pVarNames.Length >= (p + 1) Then
                Return pVarNames(j + 1)
            End If
            Return $"x{j + 1}"
        End Function

        Private Function GetSortedCategoriesFromY() As Integer()
            Dim setCat As New Dictionary(Of Integer, Boolean)()
            For i As Integer = 0 To n - 1
                Dim v As Integer = CInt(Math.Round(pData(i, 0)))
                If Not setCat.ContainsKey(v) Then setCat(v) = True
            Next
            Dim cats As Integer() = setCat.Keys.ToArray()
            Array.Sort(cats)
            Return cats
        End Function

        Private Function SumWeightsPositive() As Double
            If Not pbWeights OrElse pWeights Is Nothing Then Return CDbl(n)
            Dim s As Double = 0.0
            For i As Integer = 0 To n - 1
                If pWeights(i) > 0.0 Then s += pWeights(i)
            Next
            Return s
        End Function

        ''' <summary>
        ''' Builds a covariate-pattern key from X and optionally offset, rounding to keyDigits decimals.
        ''' </summary>
        Private Function BuildPatternKey(row As Integer, keyDigits As Integer) As String
            Dim sb As New StringBuilder(128)
            Dim fmt As String = "F" & Math.Max(0, keyDigits).ToString(CultureInfo.InvariantCulture)

            For j As Integer = 0 To UBound(pX, 2)
                sb.Append(pX(row, j).ToString(fmt, CultureInfo.InvariantCulture)).Append("|"c)
            Next

            If pbOffset Then sb.Append(pOffset(row).ToString(fmt, CultureInfo.InvariantCulture)).Append("|"c)

            Return sb.ToString()
        End Function

        ''' <summary>Group cell for GOF aggregation (counts by category).</summary>
        Private Class GroupCell
            Public ReadOnly Counts() As Double
            Public TotalW As Double
            Public ReadOnly RepRow As Integer

            Public Sub New(K As Integer, repRow As Integer)
                ReDim Counts(K - 1)
                Me.RepRow = repRow
                Me.TotalW = 0.0
            End Sub
        End Class

    End Class

End Namespace