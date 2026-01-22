Option Strict On
Option Explicit On
Imports System.Collections.Generic
Imports Microsoft.Office.Interop.Excel
Imports Microsoft.VisualBasic.Devices
Imports System.Globalization
Imports System.Text

Namespace regression


    ''' <summary>
    ''' Specifies which category is treated as the reference (baseline) category.
    ''' Categories are first sorted in ascending order of their observed values.
    ''' </summary>
    Public Enum ReferenceCategory
        ''' <summary>Use the first (smallest) observed category as baseline.</summary>
        First = 0
        ''' <summary>Use the last (largest) observed category as baseline.</summary>
        Last = 1
    End Enum

    ''' <summary>
    ''' Container for a confusion matrix and common classification diagnostics.
    ''' </summary>
    Public Class ClassificationCrosstab
        Public Categories() As Integer                 ' original category labels in ascending order
        Public Counts(,) As Double                     ' (obs x pred) weighted or unweighted
        Public RowTotals() As Double                   ' obs totals
        Public ColTotals() As Double                   ' pred totals
        Public OverallAccuracy As Double               ' sum(diag)/sum(all)
        Public OverallAccuracyPct As Double
        Public RecallPct() As Double                   ' per observed class: diag/row total * 100
        Public PrecisionPct() As Double                ' per predicted class: diag/col total * 100
        Public ColTotalsPrct() As Double
    End Class

    ''' <summary>
    ''' Identifies the type of residual-related quantity for which column names should be generated.
    ''' </summary>
    Public Enum ResidualColumnType
        ''' <summary>Observed counts y_{ik} (one-hot or weighted counts).</summary>
        Observed = 0
        ''' <summary>Fitted probabilities p_{ik}.</summary>
        FittedProbability = 1
        ''' <summary>Fitted means μ_{ik} = m_i p_{ik}.</summary>
        FittedMean = 2
        ''' <summary>Response (raw) residuals y_{ik} - μ_{ik}.</summary>
        ResponseResidual = 3
        ''' <summary>Pearson residuals (y_{ik}-μ_{ik}) / sqrt(m_i p_{ik} (1-p_{ik})).</summary>
        PearsonResidual = 4
        ''' <summary>Standardized Pearson residuals divided by sqrt(1 - h_i).</summary>
        StdPearsonResidual = 5
    End Enum


    ''' <summary>
    ''' Utility functions for baseline-category multinomial logit computations.
    ''' </summary>
    Public Module CategoricalLogitUtils

        ''' <summary>
        ''' Computes <c>log( exp(0) + sum_k exp(eta_k) )</c> stably, where the baseline category has logit 0.
        ''' </summary>
        ''' <param name="eta">Linear predictors for the non-baseline categories (length K-1).</param>
        ''' <returns>Stable log-sum-exp including the baseline exp(0).</returns>
        Public Function LogSumExpBaselineZero(eta() As Double) As Double
            Dim maxv As Double = 0.0
            For i As Integer = 0 To eta.Length - 1
                If eta(i) > maxv Then maxv = eta(i)
            Next
            Dim s As Double = Math.Exp(-maxv) ' baseline term exp(0-maxv)
            For i As Integer = 0 To eta.Length - 1
                s += Math.Exp(eta(i) - maxv)
            Next
            Return maxv + Math.Log(s)
        End Function

        ''' <summary>
        ''' Multiplies a square matrix by a vector.
        ''' </summary>
        ''' <param name="A">Square matrix (q x q).</param>
        ''' <param name="v">Vector length q.</param>
        ''' <returns>Vector length q equal to A*v.</returns>
        Public Function MatTimesVec(A(,) As Double, v() As Double) As Double()
            Dim q As Integer = UBound(A, 1)
            Dim out(q) As Double
            For i As Integer = 0 To q
                Dim s As Double = 0.0
                For j As Integer = 0 To q
                    s += A(i, j) * v(j)
                Next
                out(i) = s
            Next
            Return out
        End Function

        ''' <summary>
        ''' Returns the maximum absolute value in a vector.
        ''' </summary>
        Public Function MaxAbs(v() As Double) As Double
            Dim m As Double = 0.0R
            For i As Integer = 0 To v.Length - 1
                Dim a As Double = Math.Abs(v(i))
                If a > m Then m = a
            Next
            Return m
        End Function

        ''' <summary>
        ''' Returns index of maximum element (ties optionally break to smallest index).
        ''' </summary>
        Public Function ArgMax(v() As Double, tieBreakToSmallest As Boolean) As Integer
            Dim bestIdx As Integer = 0
            Dim bestVal As Double = v(0)
            For i As Integer = 1 To v.Length - 1
                If v(i) > bestVal Then
                    bestVal = v(i)
                    bestIdx = i
                ElseIf tieBreakToSmallest AndAlso v(i) = bestVal Then
                    ' keep smallest index
                End If
            Next
            Return bestIdx
        End Function
    End Module


    ''' <summary>
    ''' Stores residuals and related per-observation diagnostics for a multinomial logit model.
    ''' </summary>
    ''' <remarks>
    ''' For each observation i and category k:
    ''' <para>
    ''' - Observed "count": y_{ik} (for individual records, y_{ik}=m_i for the observed category and 0 otherwise)
    ''' - Fitted probability: p_{ik}
    ''' - Fitted mean: μ_{ik} = m_i p_{ik}
    ''' - Response residual: r^{(R)}_{ik} = y_{ik} - μ_{ik}
    ''' - Pearson residual: r^{(P)}_{ik} = (y_{ik}-μ_{ik}) / sqrt(m_i p_{ik} (1-p_{ik}))
    ''' </para>
    ''' <para>
    ''' Deviance residual magnitude (per row):
    ''' d_i = sqrt( 2 * Σ_k y_{ik} * log( y_{ik} / μ_{ik} ) ), ignoring terms with y_{ik}=0.
    ''' For one-trial rows, this simplifies to d_i = sqrt( 2*m_i*log(1/p_{i,y_i}) ) and is nonnegative.
    ''' </para>
    ''' <para>
    ''' Standardization uses leverage h_i: r_std = r / sqrt(1 - h_i).
    ''' </para>
    ''' </remarks>
    Public Class MultinomialResiduals
        Public Categories() As Integer
        Public Observed(,) As Double
        Public Probabilities(,) As Double
        Public FittedMeans(,) As Double
        Public ResponseResiduals(,) As Double
        Public PearsonResiduals(,) As Double
        Public Leverage() As Double
        Public StdPearsonResiduals(,) As Double
        Public DevianceResiduals() As Double
        Public StdDevianceResiduals() As Double
        Public DevianceContrib() As Double
    End Class


    ''' <summary>
    ''' Fits a baseline-category multinomial logistic regression model with optional offset and case weights,
    ''' and provides likelihood-based diagnostics, GOF tests, classification accuracy, and residuals.
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' Model (baseline-category logit):
    ''' Let Y_i ∈ {1,…,K}. Choose a baseline category K (internally the last category index).
    ''' For k=1..K-1:
    ''' </para>
    ''' <para>
    ''' η_{ik} = x_i^T β_k + offset_i
    ''' </para>
    ''' <para>
    ''' p_{ik} = exp(η_{ik}) / ( 1 + Σ_{j=1}^{K-1} exp(η_{ij}) ),   p_{iK} = 1 / ( 1 + Σ_{j=1}^{K-1} exp(η_{ij}) )
    ''' </para>
    ''' <para>
    ''' Weighted log-likelihood:
    ''' ℓ(β) = Σ_i w_i * log( p_{i, y_i} )
    ''' </para>
    ''' <para>
    ''' The code uses a Newton step with a line search; the Hessian corresponds to the multinomial Fisher structure.
    ''' </para>
    ''' </remarks>
    Public Class MultinomialLogitModel

        ' ----------------------- Data / inputs -----------------------
        Private pData(,) As Double
        Private pX(,) As Double 'all predictors (without Y) including itercept(s)
        Private pVarNames() As String
        Private pbOffset As Boolean
        Private pOffset() As Double
        Private pbWeights As Boolean
        Private pWeights() As Double
        Private pRowNums() As Integer

        ' ----------------------- Fit control -----------------------
        Private CompTime As Double
        Private pAlpha As Double
        Private pMaxiter As Integer
        Private pEps As Double
        Private pRidge As Double = 0.000000000001
        Private pLastIterLLchange As Double
        Private pIteration As Integer = 0

        ' ----------------------- Fit outputs -----------------------
        Public results As LMresult
        Public startParams() As Double = Nothing
        Public bComputeResiduals As Boolean = False
        Public bReturnCov As Boolean = False
        Public bIterationDetails As Boolean = False

        ' Covariance approximation (inverse information): stored for leverage/residual standardization.
        Private pCov(,) As Double = Nothing

        ' ----------------------- Post-fit stats -----------------------
        Private n As Integer
        Private p As Integer
        Private pPred As Integer
        Private pLL As Double 'Loglikelihood
        Private pLL0 As Double
        Private pModelChi2 As TestResult
        Private pGOF As TestResult
        Private pAIC As Double
        Private pBIC As Double
        Private pCoxSnellR2 As Double
        Private pNagelkerkeR2 As Double
        Private pMcFaddenR2 As Double
        Private pKuse As Integer
        Private pyFit() As Integer
        Private pCats() As Integer
        Private pbaselineValue As Integer
        Private pItInfo(,) As Double

        Private pPredAccuary As ClassificationCrosstab
        Private pResiduals As MultinomialResiduals


        ' ----------------------- Settings / data -----------------------

        ''' <summary>
        ''' Sets solver controls.
        ''' </summary>
        ''' <param name="dAlpha">Unused in current implementation; reserved for future step-size control.</param>
        ''' <param name="lMaxiter">Maximum number of Newton iterations.</param>
        ''' <param name="dEps">Convergence tolerance on step and log-likelihood change.</param>
        Sub settingInputs(dAlpha As Double, lMaxiter As Integer, dEps As Double,
                      Optional ridge As Double = 0.000000000001)
            pAlpha = dAlpha
            pMaxiter = lMaxiter
            pEps = dEps
            pRidge = ridge
        End Sub

        ''' <summary>
        ''' Supplies the input data, variable names, and optional row numbers, offset, and weights.
        ''' </summary>
        ''' <param name="x">
        ''' Data matrix with n rows.
        ''' Column 0 is the categorical outcome; columns 1.. are predictors.
        ''' </param>
        ''' <param name="names">
        ''' Variable names: names(0) is outcome name; names(1..) correspond to predictors in the same order as x columns.
        ''' </param>
        ''' <param name="RowNums">Optional mapping from row index to original row id for reporting.</param>
        ''' <param name="offset">
        ''' Optional offset vector (length n).
        ''' Added to each non-baseline linear predictor η_{ik}.
        ''' </param>
        ''' <param name="weights">
        ''' Optional case weights (length n). For frequency/count data, weights represent the number of trials in that row.
        ''' </param>
        Public Sub data(x(,) As Double, names() As String,
                    Optional RowNums() As Integer = Nothing,
                    Optional offset() As Double = Nothing,
                    Optional weights() As Double = Nothing)
            Me.pData = x
            Me.pVarNames = names
            If RowNums Is Nothing Then
                ReDim pRowNums(UBound(x, 1))
                For i As Integer = 0 To UBound(x, 1)
                    pRowNums(i) = i
                Next
            Else
                pRowNums = RowNums
            End If

            If offset Is Nothing Then
                pbOffset = False
                pOffset = IdentityVect(UBound(x, 1), 0.9)
            Else
                pOffset = offset
                pbOffset = True
            End If

            If weights Is Nothing Then
                pbWeights = False
                pWeights = IdentityVect(UBound(x, 1), 1.0)
            Else
                pWeights = weights
                pbWeights = True
            End If
        End Sub

        Public Function wrapResults(Optional strOffsetVar As String = "",
                                Optional strWeightsVar As String = "") As List(Of ResultTable)
            Dim out As New List(Of ResultTable), t = New ResultTable

            'coefficients, SE table
            t = Me.results.CoeffsZ_toPrint()
            t.AddPvalueToFormat(4)
            If strOffsetVar IsNot Nothing Then t.AddFootnote($"Offset Variable: {strOffsetVar}")
            If strWeightsVar IsNot Nothing Then t.AddFootnote($"Weights Variable: {strWeightsVar}")
            If Me.startParams IsNot Nothing Then t.AddFootnote($"Starting values: {array2str(Me.startParams)}")
            t.AddFootnote($"Computational time: {Me.CompTime} seconds.")
            out.Add(t)

            'Odds rations
            If Me.pPred > 0 Then out.Add(Me.results.OR_toPrint) 'if intercept only then there is nothing to output

            'Model Info
            out.Add(Me.results.getModelDiagnasticTable_toPrint())

            'Classification accuracy
            t = New ResultTable
            Dim o2(Me.pPredAccuary.PrecisionPct.Length, Me.pPredAccuary.PrecisionPct.Length) As Double
            For i = 0 To Me.pPredAccuary.PrecisionPct.Length
                For j = 0 To Me.pPredAccuary.PrecisionPct.Length
                    If i < Me.pPredAccuary.PrecisionPct.Length Then
                        o2(i, j) = If(j = Me.pPredAccuary.PrecisionPct.Length, Me.pPredAccuary.RecallPct(i), Me.pPredAccuary.Counts(i, j))
                    Else
                        o2(i, j) = If(j = Me.pPredAccuary.PrecisionPct.Length, Me.pPredAccuary.OverallAccuracyPct, Me.pPredAccuary.ColTotalsPrct(j))
                    End If
                Next
            Next

            t.SetBody(o2)
            Dim strCats(UBound(Me.pCats)) As String, strCats2(UBound(Me.pCats) + 2) As String
            For i = 0 To UBound(pCats) : strCats(i) = pCats(i).ToString : Next
            strCats2(1) = "Predicted"
            t.AddHeaderTopRow(strCats2)
            t.AddHeaderTopRow(ConcatArrays(ConcatArrays({"Observed"}, strCats), {"Classification Accuracy"}))
            t.AddHeaderLeftRow(ConcatArrays(strCats, {"Overall Percentage"}))
            out.Add(t)

            'iteration info
            If Me.bIterationDetails Then
                t = New ResultTable
                t.SetBody(Me.pItInfo)
                Dim ItLabels(Me.pIteration - 1) As String
                For i = 0 To Me.pIteration - 1 : ItLabels(i) = $"Iteration {i + 1}" : Next
                t.AddHeaderTopRow(ItLabels)
                Dim vars = ConcatArrays(Me.results.varNames, {"LogLikelihood", "LogLikelihood Change"})
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

        Public Function wrapResiduals() As Object(,)
            'call this sub only after we have parameters estimated
            Dim t As New ResultTable, tmp2(n - 1, 2) As Double
            Dim tmp = VerticalStackArrays(Me.pResiduals.FittedMeans, Me.pResiduals.Probabilities)
            tmp = VerticalStackArrays(tmp, Me.pResiduals.ResponseResiduals)
            tmp = VerticalStackArrays(tmp, Me.pResiduals.PearsonResiduals)
            tmp = VerticalStackArrays(tmp, Me.pResiduals.StdPearsonResiduals)

            Dim resnames = ConcatArrays(GetResidualColumnNames(ResidualColumnType.FittedMean),
                                    GetResidualColumnNames(ResidualColumnType.FittedProbability))
            resnames = ConcatArrays(resnames, GetResidualColumnNames(ResidualColumnType.ResponseResidual))
            resnames = ConcatArrays(resnames, GetResidualColumnNames(ResidualColumnType.PearsonResidual))
            resnames = ConcatArrays(resnames, GetResidualColumnNames(ResidualColumnType.StdPearsonResidual))
            resnames = ConcatArrays(resnames, {"DevianceResiduals", "StdDevianceResiduals", "Leverage"})
            For i = 0 To n - 1
                tmp2(i, 0) = Me.pResiduals.DevianceResiduals(i)
                tmp2(i, 1) = Me.pResiduals.StdDevianceResiduals(i)
                tmp2(i, 2) = Me.pResiduals.Leverage(i)
            Next
            t.SetBody(VerticalStackArrays(tmp, tmp2))
            t.AddHeaderTopRow(resnames)

            Return t.returnSelf()
        End Function

        ' ----------------------- Fit -----------------------

        ''' <summary>
        ''' Fits the multinomial logit model by maximizing the weighted log-likelihood.
        ''' </summary>
        ''' <param name="intercept">
        ''' If 1, an intercept column is added to the design matrix.
        ''' Note: multinomial models have category-specific intercepts (one per non-baseline category).
        ''' </param>
        ''' <param name="reference">
        ''' Chooses the baseline category as either the first or last category in sorted order of observed outcome values.
        ''' </param>
        ''' <param name="bStartParams">Reserved. If True, uses <see cref="startParams"/> when provided.</param>
        ''' <param name="progressBar">Optional UI progress bar.</param>
        ''' <param name="progressLbl">Optional UI label for progress text.</param>
        Public Sub Calculate(Optional intercept As Integer = 1,
                         Optional reference As ReferenceCategory = ReferenceCategory.Last,
                         Optional bStartParams As Boolean = False,
                         Optional progressBar As System.Windows.Forms.ProgressBar = Nothing,
                         Optional progressLbl As System.Windows.Forms.Label = Nothing)

            If pData Is Nothing Then BESHstatGlobals.BSerr.LogAndThrow(New ArgumentNullException("Data not set. Call dataInputs(x, ...)."))
            Dim startTime As Double = Microsoft.VisualBasic.DateAndTime.Timer
            Me.n = UBound(pData, 1) + 1
            Dim cols As Integer = UBound(pData, 2) + 1
            If cols < 1 Then BESHstatGlobals.BSerr.LogAndThrow(New ArgumentException("Data must have at least 1 column: Y."))

            ' Intercept-only model is valid only if intercept=1
            If cols = 1 AndAlso intercept <> 1 Then
                BESHstatGlobals.BSerr.LogAndThrow(New ArgumentException("No predictors provided and intercept=0 => model has no parameters. Use intercept=1 or add predictors."))
            End If

            ' ---- categories: unique Y values sorted ascending ----
            Me.pCats = GetSortedCategoriesFromY(n)
            Me.pKuse = pCats.Length
            If pKuse < 2 Then BESHstatGlobals.BSerr.LogAndThrow(New ArgumentException("Need at least 2 categories for multinomial logit."))

            Dim map As New Dictionary(Of Integer, Integer)()
            For i As Integer = 0 To pKuse - 1
                map(pCats(i)) = i
            Next

            ' yIdx: 0..K-1 in ascending category order
            Dim yIdx(n - 1) As Integer
            For i As Integer = 0 To n - 1
                Dim yv As Integer = CInt(Math.Round(pData(i, 0)))
                If Not map.ContainsKey(yv) Then BESHstatGlobals.BSerr.LogAndThrow(New ArgumentException($"Unknown category at row {i}."))
                yIdx(i) = map(yv)
            Next

            ' ---- choose baseline category (reference) ----
            Dim baselineOrigIndex As Integer = If(reference = ReferenceCategory.Last, pKuse - 1, 0)
            Me.pbaselineValue = pCats(baselineOrigIndex)

            ' Internally keep baseline as LAST index to use the same softmax code.
            ReDim Me.pyFit(n - 1)
            If reference = ReferenceCategory.Last Then
                Array.Copy(yIdx, Me.pyFit, n)
            Else
                ' baseline is original index 0 -> move to last; shift others down by 1
                For i As Integer = 0 To n - 1
                    Dim oi As Integer = yIdx(i)
                    Me.pyFit(i) = If(oi = 0, pKuse - 1, oi - 1)
                Next
            End If

            ' ---- build X (add intercept column if requested) ----
            Me.pPred = cols - 1
            Me.p = pPred + If(intercept = 1, 1, 0)
            ReDim Me.pX(n - 1, p - 1)

            For i As Integer = 0 To n - 1
                Dim jj As Integer = 0
                If intercept = 1 Then
                    Me.pX(i, 0) = 1.0
                    jj = 1
                End If
                For j As Integer = 0 To pPred - 1
                    Me.pX(i, j + jj) = pData(i, j + 1)
                Next
            Next

            Dim predNames() As String = BuildPredictorNames(intercept, pPred, cols)

            ' ---- parameters: beta for non-baseline categories 0..K-2 (baseline is K-1) ----
            Dim q As Integer = p * (pKuse - 1)
            Dim b(q - 1) As Double ' init zeros
            If bStartParams Then
                If Me.startParams.Length <> b.Length Then BESHstatGlobals.BSerr.LogAndThrow(New ArgumentException("starting parameter array length <> b length"))
                Me.startParams.CopyTo(b, 0)
            End If

            Dim llPrev As Double = Double.NegativeInfinity
            Dim converged As Boolean = False
            Dim invMinusH(,) As Double = Nothing
            Me.pLL = Double.NaN
            ReDim pItInfo(q + 1, pMaxiter) 'parameters, LL, LLchange

            For pItration = 0 To pMaxiter
                BSlogg.Log($"MultinomialLogit iteration #{pItration}")
                Dim g(q - 1) As Double
                Dim H(q - 1, q - 1) As Double
                Dim ll As Double = 0.0

                For i As Integer = 0 To n - 1

                    Dim wi As Double = If(pbWeights, pWeights(i), 1.0)
                    If wi <= 0.0R Then Continue For

                    ' eta_k for k=0..K-2; baseline has eta=0
                    Dim eta(pKuse - 2) As Double
                    For k As Integer = 0 To pKuse - 2
                        Dim s As Double = 0.0R
                        Dim baseIdx As Integer = k * p
                        For j As Integer = 0 To p - 1
                            s += Me.pX(i, j) * b(baseIdx + j)
                        Next

                        ' OFFSET: add to each non-baseline logit
                        If pbOffset Then s += pOffset(i)

                        eta(k) = s
                    Next

                    ' softmax with baseline=0
                    Dim lse As Double = CategoricalLogitUtils.LogSumExpBaselineZero(eta)
                    Dim pBase As Double = Math.Exp(-lse)
                    Dim pk(pKuse - 1) As Double
                    pk(pKuse - 1) = pBase
                    For k As Integer = 0 To pKuse - 2
                        pk(k) = Math.Exp(eta(k) - lse)
                    Next

                    Dim yi As Integer = Me.pyFit(i)
                    ll += wi * Math.Log(Math.Max(pk(yi), 1.0E-300))

                    ' gradient (weighted)
                    For k As Integer = 0 To pKuse - 2
                        Dim diff As Double = (If(yi = k, 1.0R, 0.0R) - pk(k)) * wi
                        Dim baseIdx As Integer = k * p
                        For j As Integer = 0 To p - 1
                            g(baseIdx + j) += Me.pX(i, j) * diff
                        Next
                    Next

                    ' Hessian blocks (weighted)
                    For k As Integer = 0 To pKuse - 2
                        For l As Integer = 0 To pKuse - 2
                            Dim wkl As Double = -pk(k) * (If(k = l, 1.0R, 0.0R) - pk(l)) * wi
                            Dim bk As Integer = k * p
                            Dim bl As Integer = l * p
                            For j As Integer = 0 To p - 1
                                Dim xj As Double = Me.pX(i, j)
                                For t As Integer = 0 To p - 1
                                    H(bk + j, bl + t) += wkl * xj * Me.pX(i, t)
                                Next
                            Next
                        Next
                    Next
                Next

                ' minusH = -H + ridge*I
                Dim minusH(q - 1, q - 1) As Double
                For r As Integer = 0 To q - 1
                    For c As Integer = 0 To q - 1
                        minusH(r, c) = -H(r, c)
                    Next
                    minusH(r, r) += pRidge
                Next

                invMinusH = MatInv(minusH, "CHOL")
                Dim stepVec() As Double = CategoricalLogitUtils.MatTimesVec(invMinusH, g)

                ' Line search to keep LL non-decreasing
                Dim stepScale As Double = 1.0
                Dim bTry(q - 1) As Double
                Dim llTry As Double
                Do
                    For ii As Integer = 0 To q - 1
                        bTry(ii) = b(ii) + stepScale * stepVec(ii)
                    Next
                    llTry = ComputeLogLikMultinom(Me.pX, Me.pyFit, bTry, p, pKuse)
                    If llTry >= ll OrElse stepScale <= 0.000001 Then Exit Do
                    stepScale *= 0.5
                    BSlogg.Log($"MultinomialLogit step halving stepScale={stepScale}, LogLike={llTry}, params={array2str(bTry)}")
                Loop

                Array.Copy(bTry, b, q)
                Me.pLL = llTry
                pLastIterLLchange = Math.Abs(Me.pLL - llPrev)
                If progressBar IsNot Nothing Then
                    progressBar.Invoke(Sub()
                                           progressBar.Value = CInt(100.0 * (Me.pIteration + 1.0) / (Me.pMaxiter + 1.0))
                                           If progressLbl IsNot Nothing Then progressLbl.Text = $"Elapsed Time: {Math.Round((Microsoft.VisualBasic.DateAndTime.Timer - startTime), 2)}[s]   Iterations: {Me.pIteration + 1}   LogLikelihood change = {pLastIterLLchange}"
                                       End Sub)
                    System.Windows.Forms.Application.DoEvents()
                End If
                BSlogg.Log($"MultinomialLogit iteration loop new esstimates  - LogLike={pLL}, pLastIterLLchange={pLastIterLLchange}, params={array2str(b)}")
                'save iteration info
                For i = 0 To q + 1
                    If i = q Then 'LL
                        pItInfo(i, pItration) = Me.pLL
                    ElseIf i = q + 1 Then 'LL change
                        pItInfo(i, pItration) = pLastIterLLchange
                    Else 'parameters
                        pItInfo(i, pItration) = b(i)
                    End If
                Next

                If CategoricalLogitUtils.MaxAbs(stepVec) * stepScale < pEps OrElse pLastIterLLchange < pEps Then
                    converged = True
                    Exit For
                End If

                llPrev = Me.pLL
            Next pItration
            If pIteration > -1 Then ReDim Preserve pItInfo(UBound(pItInfo, 1), pIteration)
            pIteration += 1
            If Not converged Then BSlogg.Log("Algorithm Is diverging. Convergence not reached.", LogMsgType.Warn)

            ' === Recompute covariance at FINAL coefficients b ===
            ' Observed information: I(b) = -H(b) (plus ridge if you want)
            Dim Hfinal(q - 1, q - 1) As Double

            For i As Integer = 0 To n - 1

                Dim wi As Double = If(pbWeights, pWeights(i), 1.0)
                If wi <= 0.0 Then Continue For

                ' --- compute linear predictors for non-baseline categories ---
                Dim eta(pKuse - 2) As Double
                For cat As Integer = 0 To pKuse - 2
                    Dim s As Double = 0.0
                    Dim baseIdx As Integer = cat * p
                    For col As Integer = 0 To p - 1
                        s += pX(i, col) * b(baseIdx + col)
                    Next
                    If pbOffset Then s += pOffset(i)
                    eta(cat) = s
                Next

                ' --- probabilities including baseline ---
                Dim lse As Double = CategoricalLogitUtils.LogSumExpBaselineZero(eta)
                Dim pk(pKuse - 1) As Double
                pk(pKuse - 1) = Math.Exp(-lse)
                For cat As Integer = 0 To pKuse - 2
                    pk(cat) = Math.Exp(eta(cat) - lse)
                Next

                ' --- accumulate Hessian blocks for non-baseline categories only ---
                For catA As Integer = 0 To pKuse - 2
                    For catB As Integer = 0 To pKuse - 2

                        Dim wAB As Double = -pk(catA) * (If(catA = catB, 1.0, 0.0) - pk(catB)) * wi

                        Dim baseA As Integer = catA * p
                        Dim baseB As Integer = catB * p

                        For u As Integer = 0 To p - 1
                            Dim xu As Double = pX(i, u)
                            Dim rowIdx As Integer = baseA + u
                            For v As Integer = 0 To p - 1
                                Hfinal(rowIdx, baseB + v) += wAB * xu * pX(i, v)
                            Next
                        Next
                    Next
                Next

            Next

            ' Build observed information matrix: minusHfinal = -Hfinal + ridge*I
            Dim minusHfinal(q - 1, q - 1) As Double
            For r As Integer = 0 To q - 1
                For c As Integer = 0 To q - 1
                    minusHfinal(r, c) = -Hfinal(r, c)
                Next
                minusHfinal(r, r) += pRidge  ' set to 0.0R if you want unregularized covariance
            Next

            ' Final covariance approximation at final b
            pCov = MatInv(minusHfinal, "CHOL")
            ' === end covariance recompute ===


            ' SEs from covariance approx inv(-H)
            Dim se(q - 1) As Double
            For i As Integer = 0 To q - 1
                se(i) = Math.Sqrt(Math.Max(0.0, pCov(i, i)))
            Next

            ' Map internal category indices (non-baseline) back to original category values
            Dim nonBaseValues(pKuse - 2) As Integer
            If reference = ReferenceCategory.Last Then
                For k As Integer = 0 To pKuse - 2
                    nonBaseValues(k) = pCats(k)
                Next
            Else
                For k As Integer = 0 To pKuse - 2
                    nonBaseValues(k) = pCats(k + 1)
                Next
            End If

            ' Parameter names
            Dim paramNames(q - 1) As String
            Dim idxName As Integer = 0
            For k As Integer = 0 To pKuse - 2
                For j As Integer = 0 To p - 1
                    paramNames(idxName) = $"cat={nonBaseValues(k)} (ref={Me.pbaselineValue}): {predNames(j)}"
                    idxName += 1
                Next
            Next

            Me.results = New LMresult()
            Me.results.n = n
            Me.results.alpha = pAlpha
            Me.results.bIntercept = False ' multiple intercepts are explicit
            Me.results.varNames = paramNames
            Me.results.Coeffs_est = b
            Me.results.Coeffs_SEs = se
            Me.ComputeFitStatistics()

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

            Me.pPredAccuary = ComputeClassificationCrosstab()
            If Me.bComputeResiduals Then Me.ComputeResiduals()

            Me.CompTime = Microsoft.VisualBasic.DateAndTime.Timer - startTime
            If progressBar IsNot Nothing Then progressBar.Invoke(Sub()
                                                                     progressBar.Value = 100
                                                                 End Sub)
        End Sub

        ' ----------------------- Residuals API -----------------------

        ''' <summary>
        ''' Computes fitted probabilities and a standard suite of residuals.
        ''' </summary>
        ''' <param name="useWeights">
        ''' If True, treats weights as counts m_i and computes y_{ik} and μ_{ik} on that scale.
        ''' If False, uses m_i=1 for all i.
        ''' </param>
        ''' <param name="computeLeverage">
        ''' If True and covariance is available, computes hat diagonals h_i and standardized residuals.
        ''' If False, leverage is returned as NaN and standardized residuals are NaN.
        ''' </param>
        ''' <remarks>
        ''' <para>
        ''' Observed matrix y_{ik} is built from the observed class index. For one-record-per-trial data:
        ''' y_{ik}=m_i for k=y_i and 0 otherwise.
        ''' </para>
        ''' <para>
        ''' Pearson residual denominator uses Var(y_{ik}) ≈ m_i p_{ik}(1-p_{ik}).
        ''' </para>
        ''' <para>
        ''' Deviance contribution per observation:
        ''' D_i = 2 * Σ_k y_{ik} log(y_{ik}/μ_{ik}) (ignore y_{ik}=0 terms).
        ''' Returned deviance residual is sqrt(D_i).
        ''' </para>
        ''' </remarks>
        Private Sub ComputeResiduals(Optional useWeights As Boolean = True,
                                 Optional computeLeverage As Boolean = True)

            If results Is Nothing OrElse results.Coeffs_est Is Nothing Then BESHstatGlobals.BSerr.LogAndThrow(New InvalidOperationException("Fit the model first (call Calculate())."))

            Dim colsK As Integer = pKuse
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
                Dim piAll() As Double = PredictRowProbs(i, results.Coeffs_est) ' length K
                For k As Integer = 0 To colsK - 1
                    out.Probabilities(i, k) = piAll(k)
                Next
            Next

            ' observed/fitted means and residuals
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

                ' Fill observed and fitted means
                For k As Integer = 0 To colsK - 1
                    Dim pik As Double = out.Probabilities(i, k)

                    out.Observed(i, k) = If(obsIdx = k, m_i, 0.0)
                    out.FittedMeans(i, k) = m_i * pik
                    out.ResponseResiduals(i, k) = out.Observed(i, k) - out.FittedMeans(i, k)
                    out.PearsonResiduals(i, k) = out.ResponseResiduals(i, k) / Math.Sqrt(Math.Max(1.0E-300R, m_i * pik * (1.0 - pik)))
                Next

                ' Deviance contribution
                Dim Di As Double = 0.0
                For k As Integer = 0 To colsK - 1
                    Dim yik As Double = out.Observed(i, k)
                    If yik > 0.0 Then
                        Dim muik As Double = Math.Max(1.0E-300R, out.FittedMeans(i, k))
                        Di += 2.0 * yik * Math.Log(yik / muik)
                    End If
                Next
                out.DevianceContrib(i) = Di
                out.DevianceResiduals(i) = Math.Sqrt(Math.Max(0.0, Di))
            Next

            ' leverage + standardized residuals
            If computeLeverage AndAlso pCov IsNot Nothing Then
                Dim h() As Double = ComputeHatDiagonal(useWeights:=useWeights)
                For i As Integer = 0 To n - 1
                    out.Leverage(i) = h(i)
                    Dim adj As Double = Math.Sqrt(Math.Max(0.000000000001, 1.0 - h(i)))

                    For k As Integer = 0 To colsK - 1
                        out.StdPearsonResiduals(i, k) = out.PearsonResiduals(i, k) / adj
                    Next
                    out.StdDevianceResiduals(i) = out.DevianceResiduals(i) / adj
                Next
            Else
                For i As Integer = 0 To n - 1
                    out.Leverage(i) = Double.NaN
                    out.StdDevianceResiduals(i) = Double.NaN
                    For k As Integer = 0 To colsK - 1
                        out.StdPearsonResiduals(i, k) = Double.NaN
                    Next
                Next
            End If

            Me.pResiduals = out
        End Sub

        ''' <summary>
        ''' Generates column names for category-wise residual matrices (Observed, fitted probabilities/means,
        ''' response residuals, Pearson residuals, etc.).
        ''' </summary>
        ''' <param name="resType">The residual-related quantity to name.</param>
        ''' <returns>
        ''' Array of column names in the form "&lt;ResidualName&gt;: cat=&lt;CategoryValue&gt;".
        ''' Category values come from the sorted observed categories (pCats).
        ''' </returns>
        ''' <remarks>
        ''' <para>
        ''' The category label shown is the original category value as observed in Y (sorted ascending).
        ''' This mirrors the style used for coefficient names.
        ''' </para>
        ''' <para>
        ''' Note: Internally, the model may reorder categories so that the reference category is last.
        ''' This method returns labels in the same column order used by the residual matrices:
        ''' categories 0..K-2 are non-baseline logits, and K-1 is the baseline column (if included).
        ''' </para>
        ''' </remarks>
        Public Function GetResidualColumnNames(resType As ResidualColumnType) As String()

            If pCats Is Nothing OrElse pCats.Length = 0 Then BESHstatGlobals.BSerr.LogAndThrow(New InvalidOperationException("Categories are not available. Fit the model first."))
            Dim cols As Integer = pKuse
            Dim names(cols - 1) As String
            Dim prefix As String = ResidualTypePrefix(resType)

            ' Column order must match residual matrices:
            ' internal category indices 0..K-2 are the non-baseline columns, and K-1 is the baseline (if included).
            For k As Integer = 0 To cols - 1
                Dim catValue As Integer = GetOriginalCategoryValueFromInternalIndex(k)
                names(k) = prefix & ": cat=" & catValue.ToString(CultureInfo.InvariantCulture)
            Next

            Return names
        End Function

        ''' <summary>
        ''' Maps a residual column internal index (0..K-1 with baseline last internally)
        ''' to the original category value in ascending order.
        ''' </summary>
        ''' <param name="internalIndex">Internal category index used in fitted probabilities/residual matrices.</param>
        ''' <returns>Original category value from pCats.</returns>
        ''' <remarks>
        ''' If the reference category is the last (default), the mapping is identity: pCats(internalIndex).
        ''' If the reference category is the first, internal index K-1 corresponds to pCats(0), and
        ''' internal index 0..K-2 correspond to pCats(1..K-1).
        ''' </remarks>
        Private Function GetOriginalCategoryValueFromInternalIndex(internalIndex As Integer) As Integer
            If internalIndex < 0 OrElse internalIndex >= pKuse Then
                BESHstatGlobals.BSerr.LogAndThrow(New ArgumentOutOfRangeException(NameOf(internalIndex)))
            End If

            ' If you already store the user's reference choice in a field, use it here.
            ' In the code I provided earlier, you set pbaselineValue and also did mapping in pyFit.
            ' We can infer "reference=First" by checking whether baselineValue equals pCats(0).
            Dim baselineIsFirst As Boolean = (pbaselineValue = pCats(0))

            If Not baselineIsFirst Then
                ' reference = Last
                Return pCats(internalIndex)
            Else
                ' reference = First (internal baseline is last)
                If internalIndex = pKuse - 1 Then
                    Return pCats(0)
                Else
                    Return pCats(internalIndex + 1)
                End If
            End If
        End Function

        ''' <summary>
        ''' Returns the human-readable prefix used in residual column names for a given residual type.
        ''' </summary>
        Private Function ResidualTypePrefix(resType As ResidualColumnType) As String
            Select Case resType
                Case ResidualColumnType.Observed
                    Return "Observed"
                Case ResidualColumnType.FittedProbability
                    Return "FittedProb"
                Case ResidualColumnType.FittedMean
                    Return "FittedMean"
                Case ResidualColumnType.ResponseResidual
                    Return "ResponseResidual"
                Case ResidualColumnType.PearsonResidual
                    Return "PearsonResidual"
                Case ResidualColumnType.StdPearsonResidual
                    Return "StdPearsonResidual"
                Case Else
                    Return "Residual"
            End Select
        End Function

        ' ----------------------- Diagnostics -----------------------

        ''' <summary>
        ''' Computes a confusion matrix and overall percent correctly classified using argmax(p_i).
        ''' </summary>
        Private Function ComputeClassificationCrosstab(Optional useWeights As Boolean = True,
                                              Optional tieBreakToSmallestCategory As Boolean = True) As ClassificationCrosstab
            If results Is Nothing OrElse results.Coeffs_est Is Nothing Then
                BESHstatGlobals.BSerr.LogAndThrow(New InvalidOperationException("Fit the model first (call Calculate())."))
            End If

            Dim out As New ClassificationCrosstab()
            out.Categories = DirectCast(pCats.Clone(), Integer())

            ReDim out.Counts(pKuse - 1, pKuse - 1)
            ReDim out.RowTotals(pKuse - 1)
            ReDim out.ColTotals(pKuse - 1)
            ReDim out.RecallPct(pKuse - 1)
            ReDim out.PrecisionPct(pKuse - 1)
            ReDim out.ColTotalsPrct(pKuse - 1)

            Dim total As Double = 0.0
            Dim correct As Double = 0.0

            For i As Integer = 0 To n - 1

                Dim wi As Double = If(useWeights AndAlso pbWeights, pWeights(i), 1.0R)
                If wi <= 0.0 Then Continue For

                Dim obsIdx As Integer = pyFit(i) ' internal index 0..K-1 (after reference mapping)
                If obsIdx < 0 OrElse obsIdx >= pKuse Then Continue For

                Dim probs() As Double = PredictRowProbs(i, results.Coeffs_est)
                Dim predIdx As Integer = CategoricalLogitUtils.ArgMax(probs, tieBreakToSmallestCategory)

                out.Counts(obsIdx, predIdx) += wi
                out.RowTotals(obsIdx) += wi
                out.ColTotals(predIdx) += wi

                total += wi
                If predIdx = obsIdx Then correct += wi
            Next

            out.OverallAccuracy = If(total > 0.0, correct / total, Double.NaN)
            out.OverallAccuracyPct = 100.0 * out.OverallAccuracy

            ' Recall and precision (%)
            For k As Integer = 0 To pKuse - 1
                Dim diag As Double = out.Counts(k, k)
                out.RecallPct(k) = If(out.RowTotals(k) > 0.0, 100.0 * diag / out.RowTotals(k), Double.NaN)
                out.PrecisionPct(k) = If(out.ColTotals(k) > 0.0, 100.0 * diag / out.ColTotals(k), Double.NaN)
                out.ColTotalsPrct(k) = 100 * out.ColTotals(k) / total
            Next

            Return out
        End Function

        ''' <summary>
        ''' Computes likelihood-based fit statistics and GOF tests.
        ''' </summary>
        Public Sub ComputeFitStatistics()
            If results Is Nothing OrElse results.Coeffs_est Is Nothing Then
                BESHstatGlobals.BSerr.LogAndThrow(New InvalidOperationException("Fit the model first (call Calculate())."))
            End If

            ' Full-model loglik
            Dim ll1 As Double = pLL
            If Double.IsNaN(ll1) Then
                ' As a fallback, recompute LL from stored design
                ll1 = ComputeLogLikMultinom(Me.pX, pyFit, results.Coeffs_est, p, pKuse)
                pLL = ll1
            End If

            ' Null model (intercept-only) loglik
            Me.pLL0 = FitNullModelLogLik()

            ' Parameter counts
            Dim kFull As Integer = results.Coeffs_est.Length
            Dim kNull As Integer = (pKuse - 1) ' intercept-only has one intercept per non-baseline category

            ' Effective N for IC / pseudo-R2 (use sum of weights if provided)
            Dim nEff As Double = If(pbWeights, Me.pWeights.Sum(), CDbl(n))
            If nEff <= 0 Then nEff = CDbl(n)

            ' Information criteria
            Me.pAIC = -2.0 * ll1 + 2.0 * kFull
            Me.pBIC = -2.0 * ll1 + kFull * Math.Log(Math.Max(1.0, nEff))

            ' Pseudo R^2
            ' Cox–Snell: 1 - exp( (2/n) (LL0 - LL1) )
            Me.pCoxSnellR2 = 1.0 - Math.Exp((2.0 / nEff) * (Me.pLL0 - ll1))

            ' Nagelkerke: CS / (1 - exp(2/n * LL0))
            Dim denomNk As Double = 1.0 - Math.Exp((2.0 / nEff) * Me.pLL0)
            If Math.Abs(denomNk) > 0.00000000000001 Then
                Me.pNagelkerkeR2 = Me.pCoxSnellR2 / denomNk
            Else
                Me.pNagelkerkeR2 = Double.NaN
            End If

            ' McFadden: 1 - (LL1 / LL0)
            If Math.Abs(Me.pLL0) > 0.00000000000001 Then
                Me.pMcFaddenR2 = 1.0 - (ll1 / Me.pLL0)
            Else
                Me.pMcFaddenR2 = Double.NaN
            End If

            ' Model LR Chi-square test
            Me.pModelChi2 = New TestResult
            Me.pModelChi2.TestStatistics1 = 2.0 * (ll1 - Me.pLL0)
            Dim df As Integer = kFull - kNull
            If df <= 0 Then
                Me.pModelChi2.DF1 = 0
                Me.pModelChi2.Pvalue = Double.NaN
                ' chisq will be 0 (up to floating error) for intercept-only
            Else
                Me.pModelChi2.DF1 = df
                Me.pModelChi2.Pvalue = 1.0 - distributions.ChiSquareCDF(Me.pModelChi2.TestStatistics1, df)
            End If

            ' Deviance GOF
            ' Saturated LL for single-trial multinomial is 0 (prob=1 for observed category).
            Me.pGOF = ComputeDevianceGoodnessOfFit()
        End Sub

        ''' <summary>
        ''' Computes profile-based (covariate-pattern) deviance goodness-of-fit:
        ''' D = 2(ℓ_sat - ℓ_model) with df = G(K-1) - kFull.
        ''' </summary>
        Private Function ComputeDevianceGoodnessOfFit(Optional includeOffsetInPattern As Boolean = True,
                                                 Optional keyDigits As Integer = 12) As TestResult

            If results Is Nothing OrElse results.Coeffs_est Is Nothing Then
                BESHstatGlobals.BSerr.LogAndThrow(New InvalidOperationException("Fit the model first (call Calculate())."))
            End If
            Dim out As New TestResult
            out.TestStatistics1 = Double.NaN
            out.DF1 = 0
            out.Pvalue = Double.NaN
            Dim b() As Double = results.Coeffs_est
            Dim kFull As Integer = b.Length

            ' 2) Covariate-pattern deviance (collapse identical X (and optionally offset) into cells)
            ' Build group counts y_gk and store one representative row index for fitted probs.
            Dim groups As New Dictionary(Of String, GroupCell)()

            For i As Integer = 0 To n - 1

                Dim wi As Double = If(pbWeights, pWeights(i), 1.0)
                If wi <= 0.0 Then Continue For

                Dim key As String = BuildPatternKey(i, includeOffsetInPattern, keyDigits)
                Dim cell As GroupCell = Nothing

                If Not groups.TryGetValue(key, cell) Then
                    cell = New GroupCell(pKuse, i)
                    groups.Add(key, cell)
                End If

                cell.TotalW += wi
                cell.Counts(pyFit(i)) += wi
            Next

            Dim G As Integer = groups.Count
            If G <= 0 Then Return out

            ' Compute deviance = 2 * (LL_sat - LL_model) on collapsed cells
            Dim llSat As Double = 0.0
            Dim llModel As Double = 0.0

            For Each kv In groups
                Dim cell As GroupCell = kv.Value
                Dim m As Double = cell.TotalW
                If m <= 0.0 Then Continue For

                ' Fitted probs for this cell (use representative row index)
                Dim pi() As Double = PredictRowProbs(cell.RepRow, b)

                ' LL_model: Σ_k y_gk * log(p_k)
                For k As Integer = 0 To pKuse - 1
                    Dim yk As Double = cell.Counts(k)
                    If yk > 0.0 Then llModel += yk * Math.Log(Math.Max(pi(k), 1.0E-300))
                Next

                ' LL_sat: Σ_k y_gk * log(y_gk / m)
                For k As Integer = 0 To pKuse - 1
                    Dim yk As Double = cell.Counts(k)
                    If yk > 0.0 Then llSat += yk * Math.Log(Math.Max(yk / m, 1.0E-300))
                Next
            Next

            out.TestStatistics1 = 2.0 * (llSat - llModel)
            Dim df As Integer = G * (pKuse - 1) - kFull
            out.DF1 = Math.Max(0, df)
            out.Pvalue = If(out.DF1 > 0, 1.0 - distributions.ChiSquareCDF(out.TestStatistics1, out.DF1), Double.NaN)
            Return out
        End Function

        ' ----------------------- Residual helpers -----------------------

        ''' <summary>
        ''' Computes leverage (hat diagonal) for each observation:
        ''' h_i = tr( W_i Z_i Cov Z_i^T ),
        ''' where Z_i = I_{K-1} ⊗ x_i^T and W_i = m_i (diag(p) - p p^T) for the non-baseline categories.
        ''' </summary>
        ''' <param name="useWeights">If True uses m_i = weight_i, else m_i=1.</param>
        ''' <returns>Vector h of length n.</returns>
        ''' <remarks>
        ''' <para>
        ''' This is the standard generalized linear model leverage generalized to the multinomial block structure.
        ''' Cov is the inverse observed information approximation stored from the fit.
        ''' </para>
        ''' </remarks>
        Private Function ComputeHatDiagonal(Optional useWeights As Boolean = True) As Double()
            Dim h(n - 1) As Double
            If pCov Is Nothing Then
                For i As Integer = 0 To n - 1 : h(i) = Double.NaN : Next
                Return h
            End If

            Dim Knon As Integer = pKuse - 1
            Dim q As Integer = p * Knon

            For i As Integer = 0 To n - 1

                Dim m_i As Double = If(useWeights AndAlso pbWeights, pWeights(i), 1.0)
                If m_i <= 0.0 Then
                    h(i) = Double.NaN
                    Continue For
                End If

                ' probabilities
                Dim piAll() As Double = PredictRowProbs(i, results.Coeffs_est)
                Dim pNon(Knon - 1) As Double
                For a As Integer = 0 To Knon - 1
                    pNon(a) = piAll(a)
                Next

                ' W_i = m_i*(diag(pNon) - pNon pNon^T)
                Dim W(Knon - 1, Knon - 1) As Double
                For a As Integer = 0 To Knon - 1
                    For b As Integer = 0 To Knon - 1
                        W(a, b) = -m_i * pNon(a) * pNon(b)
                    Next
                    W(a, a) += m_i * pNon(a)
                Next

                ' A(a,b) = x_i^T Cov_ab x_i where Cov_ab is the p×p block (a,b)
                Dim AA(Knon - 1, Knon - 1) As Double
                For a As Integer = 0 To Knon - 1
                    Dim baseA As Integer = a * p
                    For b As Integer = 0 To Knon - 1
                        Dim baseB As Integer = b * p
                        Dim s As Double = 0.0R
                        For u As Integer = 0 To p - 1
                            Dim xu As Double = pX(i, u)
                            Dim rowIdx As Integer = baseA + u
                            For v As Integer = 0 To p - 1
                                s += xu * pCov(rowIdx, baseB + v) * pX(i, v)
                            Next
                        Next
                        AA(a, b) = s
                    Next
                Next

                ' h_i = trace(W*A) = Σ_{a,b} W(a,b)*A(b,a) (A is symmetric in ideal case)
                Dim hi As Double = 0.0R
                For a As Integer = 0 To Knon - 1
                    For b As Integer = 0 To Knon - 1
                        hi += W(a, b) * AA(b, a)
                    Next
                Next
                h(i) = hi
            Next

            Return h
        End Function

        ''' <summary>
        ''' Helper cell structure for grouped (covariate-pattern) computations.
        ''' </summary>
        Private Class GroupCell
            Public ReadOnly Counts() As Double
            Public TotalW As Double
            Public ReadOnly RepRow As Integer

            ''' <summary>
            ''' Creates a new cell with K categories and a representative row index.
            ''' </summary>
            Public Sub New(K As Integer, repRow As Integer)
                ReDim Counts(K - 1)
                Me.RepRow = repRow
                Me.TotalW = 0.0
            End Sub
        End Class

        ''' <summary>
        ''' Builds a grouping key for covariate-pattern aggregation using the model matrix X
        ''' and optionally the offset. Values are rounded to a fixed number of decimal digits.
        ''' </summary>
        Private Function BuildPatternKey(row As Integer, includeOffset As Boolean, keyDigits As Integer) As String
            ' Key is based on the design row X and optionally the offset.
            ' Use rounding to reduce key instability from floating noise.
            Dim sb As New StringBuilder(128)
            Dim fmt As String = "F" & Math.Max(0, keyDigits).ToString(CultureInfo.InvariantCulture)
            For j As Integer = 0 To UBound(pX, 2)
                sb.Append(pX(row, j).ToString(fmt, CultureInfo.InvariantCulture)).Append("|"c)
            Next
            If includeOffset AndAlso pbOffset Then
                sb.Append(pOffset(row).ToString(fmt, CultureInfo.InvariantCulture)).Append("|"c)
            End If
            Return sb.ToString()
        End Function

        ''' <summary>
        ''' Computes fitted probabilities for one row (length K), including offset (if present).
        ''' </summary>
        ''' <remarks>
        ''' For k=0..K-2 (non-baseline):
        ''' η_{ik} = x_i^T β_k + offset_i
        ''' Baseline has η=0.
        ''' Probabilities follow the softmax with baseline.
        ''' </remarks>
        Private Function PredictRowProbs(row As Integer, b() As Double) As Double()
            Dim nCat As Integer = pKuse
            Dim pCols As Integer = p

            Dim eta(nCat - 2) As Double
            For cat As Integer = 0 To nCat - 2
                Dim s As Double = 0.0
                Dim baseIdx As Integer = cat * pCols
                For col As Integer = 0 To pCols - 1
                    s += pX(row, col) * b(baseIdx + col)
                Next
                If pbOffset Then s += pOffset(row) ' offset added to non-baseline logits
                eta(cat) = s
            Next

            Dim lse As Double = CategoricalLogitUtils.LogSumExpBaselineZero(eta)
            Dim probs(nCat - 1) As Double
            probs(nCat - 1) = Math.Exp(-lse)
            For cat As Integer = 0 To nCat - 2
                probs(cat) = Math.Exp(eta(cat) - lse)
            Next

            Return probs
        End Function

        ' ----------------------- Likelihood helpers -----------------------

        ''' <summary>
        ''' Fits the intercept-only (null) multinomial model and returns its maximized log-likelihood.
        ''' </summary>
        Private Function FitNullModelLogLik() As Double
            ' Null model: category-specific intercepts only (for non-baseline categories).
            ' pNull = 1 (intercept), params length = (K-1)
            Dim pNull As Integer = 1
            Dim qNull As Integer = (Me.pKuse - 1) * pNull
            Dim b0(qNull - 1) As Double ' init zeros

            Dim llPrev As Double = Double.NegativeInfinity
            Dim llFinal As Double = Double.NaN

            For it As Integer = 1 To Math.Min(60, Me.pMaxiter)

                Dim g(qNull - 1) As Double
                Dim H(qNull - 1, qNull - 1) As Double
                Dim ll As Double = 0.0

                For i As Integer = 0 To Me.n - 1

                    Dim wi As Double = If(pbWeights, pWeights(i), 1.0)
                    If wi <= 0.0R Then Continue For

                    ' For null: eta_k = intercept_k + offset_i
                    Dim eta(Me.pKuse - 2) As Double
                    For k As Integer = 0 To Me.pKuse - 2
                        Dim s As Double = b0(k) ' intercept for category k
                        If pbOffset Then s += pOffset(i)
                        eta(k) = s
                    Next

                    Dim lse As Double = CategoricalLogitUtils.LogSumExpBaselineZero(eta)
                    Dim pBase As Double = Math.Exp(-lse)
                    Dim pk(Me.pKuse - 1) As Double
                    pk(Me.pKuse - 1) = pBase
                    For k As Integer = 0 To Me.pKuse - 2
                        pk(k) = Math.Exp(eta(k) - lse)
                    Next

                    Dim yi As Integer = Me.pyFit(i)
                    ll += wi * Math.Log(Math.Max(pk(yi), 1.0E-300))

                    ' gradient/hessian w.r.t intercepts
                    For k As Integer = 0 To Me.pKuse - 2
                        Dim diff As Double = (If(yi = k, 1.0, 0.0) - pk(k)) * wi
                        g(k) += diff
                    Next

                    For k As Integer = 0 To Me.pKuse - 2
                        For l As Integer = 0 To Me.pKuse - 2
                            Dim wkl As Double = -pk(k) * (If(k = l, 1.0, 0.0) - pk(l)) * wi
                            H(k, l) += wkl
                        Next
                    Next
                Next

                ' minusH = -H + ridge*I
                Dim minusH(qNull - 1, qNull - 1) As Double
                For r As Integer = 0 To qNull - 1
                    For c As Integer = 0 To qNull - 1
                        minusH(r, c) = -H(r, c)
                    Next
                    minusH(r, r) += Me.pRidge
                Next

                Dim invMinusH(,) As Double = MatInv(minusH, "CHOL")
                Dim stepVec() As Double = CategoricalLogitUtils.MatTimesVec(invMinusH, g)

                Dim stepScale As Double = 1.0
                Dim bTry(qNull - 1) As Double
                Dim llTry As Double

                Do
                    For ii As Integer = 0 To qNull - 1
                        bTry(ii) = b0(ii) + stepScale * stepVec(ii)
                    Next
                    llTry = ComputeLogLikNull(bTry)
                    If llTry >= ll OrElse stepScale <= 0.000001 Then Exit Do
                    stepScale *= 0.5
                Loop

                Array.Copy(bTry, b0, qNull)
                llFinal = llTry

                If CategoricalLogitUtils.MaxAbs(stepVec) * stepScale < pEps OrElse Math.Abs(llFinal - llPrev) < pEps Then
                    Exit For
                End If
                llPrev = llFinal
            Next

            Return llFinal
        End Function

        ''' <summary>
        ''' Computes null-model log-likelihood for a given null parameter vector.
        ''' </summary>
        Private Function ComputeLogLikNull(b0() As Double) As Double
            Dim ll As Double = 0.0
            For i As Integer = 0 To Me.n - 1
                Dim wi As Double = If(pbWeights, pWeights(i), 1.0)
                If wi <= 0.0 Then Continue For

                Dim eta(Me.pKuse - 2) As Double
                For k As Integer = 0 To Me.pKuse - 2
                    Dim s As Double = b0(k)
                    If pbOffset Then s += pOffset(i)
                    eta(k) = s
                Next

                Dim lse As Double = CategoricalLogitUtils.LogSumExpBaselineZero(eta)
                Dim yi As Integer = Me.pyFit(i)
                If yi = Me.pKuse - 1 Then
                    ll += wi * Math.Log(Math.Max(Math.Exp(-lse), 1.0E-300))
                Else
                    ll += wi * Math.Log(Math.Max(Math.Exp(eta(yi) - lse), 1.0E-300))
                End If
            Next
            Return ll
        End Function

        ''' <summary>
        ''' Extracts and sorts unique observed categories in Y (column 0).
        ''' </summary>
        Private Function GetSortedCategoriesFromY(n As Integer) As Integer()
            Dim setCat As New Dictionary(Of Integer, Boolean)()
            For i As Integer = 0 To n - 1
                Dim v As Integer = CInt(Math.Round(pData(i, 0)))
                If Not setCat.ContainsKey(v) Then setCat(v) = True
            Next
            Dim cats As Integer() = setCat.Keys.ToArray()
            Array.Sort(cats)
            Return cats
        End Function

        ''' <summary>
        ''' Builds predictor names including intercept label if requested.
        ''' </summary>
        Private Function BuildPredictorNames(intercept As Integer, pPred As Integer, cols As Integer) As String()
            Dim p As Integer = pPred + If(intercept = 1, 1, 0)
            Dim out(p - 1) As String
            Dim offset As Integer = 0

            If intercept = 1 Then
                out(0) = "Intercept"
                offset = 1
            End If

            If pVarNames IsNot Nothing AndAlso pVarNames.Length = cols Then
                For j As Integer = 0 To pPred - 1
                    out(j + offset) = pVarNames(j + 1) ' skip Y name
                Next
            Else
                For j As Integer = 0 To pPred - 1
                    out(j + offset) = $"x{j + 1}"
                Next
            End If

            Return out
        End Function

        ''' <summary>
        ''' Computes the weighted log-likelihood for the multinomial model for a given parameter vector.
        ''' </summary>
        ''' <remarks>
        ''' ℓ(β)=Σ_i w_i log(p_{i,y_i}). The baseline category uses η=0.
        ''' </remarks>
        Private Function ComputeLogLikMultinom(X(,) As Double, yFit() As Integer, b() As Double, p As Integer, Kuse As Integer) As Double
            Dim n As Integer = yFit.Length
            Dim ll As Double = 0.0

            For i As Integer = 0 To n - 1
                Dim wi As Double = If(pbWeights, pWeights(i), 1.0)
                If wi <= 0.0 Then Continue For

                Dim eta(Kuse - 2) As Double
                For k As Integer = 0 To Kuse - 2
                    Dim s As Double = 0.0
                    Dim baseIdx As Integer = k * p
                    For j As Integer = 0 To p - 1
                        s += X(i, j) * b(baseIdx + j)
                    Next
                    If pbOffset Then s += pOffset(i)
                    eta(k) = s
                Next

                Dim lse As Double = CategoricalLogitUtils.LogSumExpBaselineZero(eta)
                Dim yi As Integer = yFit(i)
                If yi = Kuse - 1 Then
                    ll += wi * Math.Log(Math.Max(Math.Exp(-lse), 1.0E-300))
                Else
                    ll += wi * Math.Log(Math.Max(Math.Exp(eta(yi) - lse), 1.0E-300))
                End If
            Next

            Return ll
        End Function

    End Class

End Namespace