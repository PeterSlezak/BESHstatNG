Option Explicit On
Imports System.Drawing
Imports System.Security.Cryptography
Imports System.Windows.Forms.VisualStyles.VisualStyleElement.TrackBar
Imports Microsoft.Office.Interop.Excel
Imports System.Linq

Public Class GEE

    Private pLink As regression.Link
    Private pFamily As regression.Family
    Private pCovStruct As regression.GEEcovStruct

    Private pData(,) As Double 'It is assumed that response varaible is in the 1st column
    Private pOffset() As Double ', pOffsetUntrunsformed() As Double
    Private pbOffset As Boolean
    Private pWeights() As Double
    Private pbWeights As Boolean
    Private pVarNames() As String
    Private pClusterIdVarName As String = String.Empty
    Private pTimeVarName As String = String.Empty
    Private pOffsetVarName As String = String.Empty
    Private pWeightsVarName As String = String.Empty
    Private pRowNums() As Integer
    Private pRepeats() As Object 'variable data specifying repeats (e.g. subjid in longitudinal data)
    Private pTimeRaw() As Double 'time variable (i.e. within cluster ordering variable)
    Private pbMissingTime As Boolean = False '.True. if time variable was not provided
    Private pNoGroup As Integer
    Private pGroupLabels() As String
    Private pEndogLi As New List(Of Double()) 'list of dependent variable arrays corresponding to the cluster structure
    Private pExogLi As New List(Of Double(,)) 'list of independent variable(s) arrays corresponding to the cluster structure
    Private pOffsetLi As New List(Of Double()) 'list of offset variable arrays corresponding to the cluster structure
    Private pTimeLi As New List(Of Double())
    Private pCachedMeans As New List(Of (Double(), Double(,)))
    Private pClusterSize() As Integer

    Private pEps As Double = 0.00000001
    Private pMaxiter As Integer = 20
    Private pAlpha As Double = 0.05 'significance level
    Private pbUseP As Boolean = False
    Private pScalingFactor As Double = 1.0
    Private pStdErrType As String = "Robust"
    Private pScaleType As Integer = 0 '0 means None. Otherwise provide a value of the scale parameter
    Private CompTime As Double
    Private pUniqueTimesDict As New Dictionary(Of Double, Integer)
    Private pGroupIndices As New Dictionary(Of String, Integer())

    Private n As Integer
    Private p As Integer 'number of independent variables/Exogs including intercept
    Private pDFmodel As Integer
    Private pDFresid As Integer
    Private pScale As Double
    Private pQL As Double 'The quasi-likelihood value
    Private pQIC As Double 'A QIC that can be used to compare the mean and covariance structures of the model.
    Private pQICu As Double 'A simplified QIC that can be used to compare mean structures but not covariance structures
    Private pIndependenceNaiveVarCovar(,) As Double 'needed for QIC
    Private pCovRobust(,) As Double
    Private pCovNaive(,) As Double
    Private pCovBiasCorr(,) As Double
    Private pItInfo(,) As Double
    Private pConverged As Boolean = False
    Private pItration As Integer = 0

    Public results As LMresult
    Public bIterationDetails As Boolean = False
    Public bComputeResiduals As Boolean = False
    Public startParams() As Double = Nothing 'Starting parameter values

    Private Const VAR_EPS As Double = 0.000000000001

    'Residuals
    ''' <summary>Raw (response) residuals: y − μ.</summary>
    Private pRawRes() As Double
    ''' <summary>
    ''' Pearson residuals: (y − μ) / sqrt(V(μ)).
    ''' If weights are requested, returns sqrt(w) * (y − μ) / sqrt(V(μ)).
    ''' </summary>
    Private pPearsonRes() As Double
    ''' <summary>
    ''' Scaled Pearson residuals: Pearson / sqrt(φ) where φ is the model scale.
    ''' </summary>
    Private pPearsonScaledRes() As Double
    ''' <summary>
    ''' Deviance residuals: sign(y − μ) * sqrt(Dᵢ).
    ''' If weights are requested, returns sign(y − μ) * sqrt(w * Dᵢ).
    ''' </summary>
    Private pDevianceRes() As Double
    ''' <summary>
    ''' Scaled deviance residuals: Deviance / sqrt(φ) where φ is the model scale.
    ''' </summary>
    Private pDevianceScaledRes() As Double
    ''' <summary>
    ''' Working residuals: (y − μ) / (dμ/dη).
    ''' If weights are requested, returns sqrt(w) * (y − μ) / (dμ/dη).
    ''' </summary>
    Private pWorkingRes() As Double

    Public Sub New(fam As regression.Family, lin As regression.Link, covStr As regression.GEEcovStruct, Optional strSEtype As String = "Robust") ' make sure these object are created and ready at the very beginning
        Me.pFamily = fam
        Me.pLink = lin
        Me.pCovStruct = covStr
        Me.pStdErrType = strSEtype
    End Sub

    Public Sub settingInputs(dAlpha As Double, lMaxiter As Long, dEps As Double, bUseP As Boolean)
        pAlpha = dAlpha
        pMaxiter = lMaxiter
        pEps = dEps
        pbUseP = bUseP
    End Sub

    Public Sub data(data(,) As Double, repeat() As Object,
             Optional RowNums() As Integer = Nothing,
             Optional Offset() As Double = Nothing,
             Optional Weights() As Double = Nothing,
             Optional time() As Double = Nothing)
        pData = data 'it is assumed that dependent variable is in the first column
        pRepeats = repeat

        Me.n = UBound(pRepeats) + 1
        Me.p = UBound(pData, 2) + 1 '# of independent vars + intercept (pData 1st column is dependent var it should be equal to dims itself)
        Me.pDFmodel = p - 1
        Me.pDFresid = n - p

        If n <= p Then
            BESHstatGlobals.BSerr.LogAndThrow(New ArgumentException($"Not enough observations to fit model: n={n}, parameters={p}. Need n > p."))
        End If

        If RowNums Is Nothing Then
            ReDim pRowNums(Me.n - 1)
            For i = 0 To Me.n - 1
                pRowNums(i) = i
            Next
        Else
            Me.pRowNums = RowNums
        End If

        If Offset Is Nothing Then
            ReDim pOffset(n - 1) 'it automaticaly assign zeros
            Me.pbOffset = False
        Else
            Me.pbOffset = True
            pOffset = Offset
        End If

        If Weights Is Nothing Then
            Me.pWeights = IdentityVect(Me.n - 1, 1) 'it automaticaly assign zeros
            Me.pbWeights = False
        Else
            Me.pbWeights = True
            pWeights = Weights
        End If

        If time Is Nothing Then
            Me.pbMissingTime = True
            ReDim pTimeRaw(n - 1)
        Else
            pTimeRaw = time
        End If

        'get data into the required format
        PreProcessData()
    End Sub

    Public Sub setVarNames(names() As String, strClusterIDname As String,
                    Optional strOffsetName As String = Nothing,
                    Optional strWeightsName As String = Nothing,
                    Optional strTimeName As String = Nothing)
        Me.pVarNames = names
        Me.pClusterIdVarName = strClusterIDname
        Me.pOffsetVarName = strOffsetName
        Me.pWeightsVarName = strWeightsName
        Me.pTimeVarName = strTimeName
    End Sub

    Public ReadOnly Property PredictedResponses() As Double()
        Get
            Dim mu(n - 1) As Double
            Dim k As Integer = 0
            For i = 0 To pNoGroup - 1
                For j = 0 To UBound(pCachedMeans(i).Item1)
                    mu(k) = pCachedMeans(i).Item1(j)
                    k += 1
                Next
            Next
            Return mu
        End Get
    End Property

    Public ReadOnly Property AllResiduals() As Object(,)
        Get
            Dim t = New ResultTable
            Dim o(n - 1, 5) As Double
            For i = 0 To n - 1
                o(i, 0) = Me.pRawRes(i)
                o(i, 1) = Me.pDevianceRes(i)
                o(i, 2) = Me.pPearsonRes(i)
                o(i, 3) = Me.pDevianceScaledRes(i)
                o(i, 4) = Me.pPearsonScaledRes(i)
                o(i, 5) = Me.pWorkingRes(i)
            Next
            t.SetBody(o)
            t.AddHeaderTopRow({"Raw Resid.", "Deviance Resid.", "Pearson Resid.", "Std Deviance Resid.", "Std Pearson Resid.", "Working Resid."})
            Return t.returnSelf()
        End Get
    End Property

    Public ReadOnly Property TimesDict() As Dictionary(Of Double, Integer)
        Get
            Return Me.pUniqueTimesDict
        End Get
    End Property

    Public ReadOnly Property TimeClustered() As List(Of Double())
        Get
            Return Me.pTimeLi
        End Get
    End Property

    Public ReadOnly Property CachedMeans() As List(Of (Double(), Double(,)))
        Get
            Return Me.pCachedMeans
        End Get
    End Property

    Public ReadOnly Property EndogClustered() As List(Of Double())
        Get
            Return Me.pEndogLi
        End Get
    End Property

    Public ReadOnly Property UniqueTimesDict() As Dictionary(Of Double, Integer)
        Get
            Return Me.pUniqueTimesDict
        End Get
    End Property

    Public ReadOnly Property Family() As regression.Family
        Get
            Return Me.pFamily
        End Get
    End Property

    Public ReadOnly Property NoGroup() As Integer
        Get
            Return Me.pNoGroup
        End Get
    End Property

    Public ReadOnly Property Nparams() As Integer
        Get
            Return Me.p
        End Get
    End Property

    Public ReadOnly Property Nobs() As Integer
        Get
            Return Me.n
        End Get
    End Property

    Public ReadOnly Property UseP() As Boolean
        Get
            Return Me.pbUseP
        End Get
    End Property

    Public ReadOnly Property hasTime() As Boolean
        Get
            Return Not Me.pbMissingTime
        End Get
    End Property

    Public ReadOnly Property DFresid() As Integer
        Get
            Return Me.pDFresid
        End Get
    End Property

    Public Function wrapResults() As List(Of ResultTable)
        Dim out As New List(Of ResultTable)
        Dim t = New ResultTable

        'coefficients, SE table
        t = Me.results.CoeffsZ_toPrint()
        t.AddPvalueToFormat(4)
        If Me.pOffsetVarName IsNot Nothing Then t.AddFootnote($"Offset Variable: {Me.pOffsetVarName}")
        If Me.pWeightsVarName IsNot Nothing Then t.AddFootnote($"Weights Variable: {Me.pWeightsVarName}")
        If Me.startParams IsNot Nothing Then t.AddFootnote($"Starting values: {array2str(Me.startParams)}")
        'If Me.bSeparation Then
        '    t.AddFootnote("Complete separation of data points. Maximum likelihood estimates may not exist.")
        'ElseIf Me.bQuasiSeparation Then
        '    t.AddFootnote("Quasi-separation of the iterative algorithm. Results may be misleading.")
        'End If
        t.AddFootnote($"Computational time: {Me.CompTime} seconds.")
        out.Add(t)

        'Model Info
        out.Add(Me.results.getModelDiagnasticTable_toPrint())

        'Working correlation matrix
        t = New ResultTable
        t.SetBody(Me.pCovStruct.DepParams(Me))
        t.AddTitle("Working Correlation Matrix")
        out.Add(t)

        'Covariance Matrix - Model based (Naive)
        Dim strVars() As String = ConcatArrays({"Intercept"}, BESHStatNG.SubsetArray(pVarNames, 1))
        t = New ResultTable
        t.SetBody(Me.pCovNaive)
        t.AddHeaderLeftRow(strVars)
        t.AddHeaderTopRow(strVars)
        t.AddTitle("Covariance Matrix - Model based (Naive)")
        out.Add(t)

        'Covariance Matrix - Model based (Naive)
        t = New ResultTable
        t.SetBody(Me.pCovRobust)
        t.AddHeaderLeftRow(strVars)
        t.AddHeaderTopRow(strVars)
        t.AddTitle("Covariance Matrix - Empirical (Robust)")
        out.Add(t)

        'Covariance Matrix - Bias Reduced
        If Me.pStdErrType = "Bias Reduced" Then
            t = New ResultTable
            t.SetBody(Me.pCovBiasCorr)
            t.AddHeaderLeftRow(strVars)
            t.AddHeaderTopRow(strVars)
            t.AddTitle("Covariance Matrix - Bias Reduced")
            out.Add(t)
        End If

        'iteration info
        If Me.bIterationDetails Then
            t = New ResultTable
            t.SetBody(Me.pItInfo)
            Dim ItLabels(Me.pItration) As String
            For i = 0 To Me.pItration : ItLabels(i) = $"Iteration {i + 1}" : Next
            t.AddHeaderTopRow(ItLabels)
            t.AddHeaderLeftRow(ConcatArrays(Me.pVarNames, {"Parameter Change"}))
            out.Add(t)
        End If

        Return out
    End Function
    Public Sub Calculate(bStartParams As Boolean,
                         Optional scalingFactor As Double = 1.0#,
                         Optional progressBar As System.Windows.Forms.ProgressBar = Nothing,
                         Optional progressLbl As System.Windows.Forms.Label = Nothing)
        BSlogg.Log("proc started: gee.Calculate")
        Dim update() As Double = Nothing, score() As Double = Nothing, del_params As Double, strTmpTrace As String = String.Empty
        Dim startTime As Double = Microsoft.VisualBasic.DateAndTime.Timer
        Me.pScalingFactor = scalingFactor
        Me.results = New LMresult
        Me.results.varNames = BESHStatNG.SubsetArray(pVarNames, 1)
        ReDim pItInfo(p, pMaxiter)

        'starting parameters
        Dim meanParams() As Double = If(bStartParams, Me.startParams, GetStartParams())
        Me.UpdateCachedMeans(meanParams)

        Dim prevParams() As Double = CType(meanParams.Clone(), Double())
        Dim consecOK As Integer = 0
        Const SAS_REL_THRESH As Double = 0.08   'SAS GENMOD GEE threshold :contentReference[oaicite:1]{index=1}

        For pItration = 0 To Me.pMaxiter

            'Compute step (same as your current code)
            Me.updateMeanParams(update, score)
            BSlogg.Log($"Iteration={pItration + 1} update:{array2str(update)} score: {array2str(score)}")

            'Apply step (same as your current code)
            meanParams = M_ADD(meanParams, update)
            Me.UpdateCachedMeans(meanParams)

            '--- SAS-style convergence criterion: max change in beta (abs or relative) ---
            del_params = 0.0
            For j = 0 To UBound(meanParams)
                Dim d As Double = Math.Abs(meanParams(j) - prevParams(j))
                If Math.Abs(meanParams(j)) > SAS_REL_THRESH Then
                    d = d / Math.Abs(meanParams(j))   'relative change
                End If
                If d > del_params Then del_params = d
            Next

            'two successive iterations required
            If del_params < pEps Then
                consecOK += 1
            Else
                consecOK = 0
            End If

            BSlogg.Log($"Iteration={pItration + 1} meanParams:{array2str(meanParams)} sas_del={del_params} consecOK={consecOK}")

            'save iteration info (store criterion in last row, like before)
            For i = 0 To p
                pItInfo(i, pItration) = If(i = p, del_params, meanParams(i))
            Next

            'check for convergence (SAS: two successive iterations)
            'keep your rule: don't exit until assoc updated at least once
            If (consecOK >= 2) AndAlso (pItration > 0) Then
                pConverged = True
                Exit For
            End If

            'Update dependence structure
            pCovStruct.updateAssoc(Me, strTmpTrace)
            If strTmpTrace <> String.Empty Then BSlogg.Log($"strTmpTrace= {strTmpTrace}")

            'UI progress
            If progressBar IsNot Nothing Then
                progressBar.Invoke(Sub()
                                       progressBar.Value = 100 * (Me.pItration + 1) / (Me.pMaxiter + 1)
                                       If progressLbl IsNot Nothing Then
                                           progressLbl.Text = $"Elapsed Time: {Math.Round((Microsoft.VisualBasic.DateAndTime.Timer - startTime), 2)}[s]  Iter {Me.pItration + 1}   Last convergence crit. value = {del_params}"
                                       End If
                                   End Sub)
                System.Windows.Forms.Application.DoEvents()
            End If

            'update prevParams for next iteration
            prevParams = CType(meanParams.Clone(), Double())

        Next pItration
        '-----------------------------------------------

        If Not pConverged Then BSlogg.Log($"Iteration limit reached prior to convergence", LogMsgType.Warn)
        If pItration > -1 Then ReDim Preserve pItInfo(UBound(pItInfo, 1), pItration)

        Me.pScale = EstimateScale(True)
        Me.ComputeCovMat()
        If pStdErrType = "Bias Reduced" Then Me.pCovBiasCorr = ComputeBScovMat(pCovNaive)

        'Save results
        Me.results.alpha = pAlpha
        Me.results.Coeffs_est = meanParams
        ReDim Me.results.Coeffs_SEs(Me.p - 1)
        For i = 0 To Me.p - 1
            If pStdErrType = "Bias Reduced" Then
                Me.results.Coeffs_SEs(i) = Math.Sqrt(Me.pCovBiasCorr(i, i))
            ElseIf pStdErrType = "Robust" Then
                Me.results.Coeffs_SEs(i) = Math.Sqrt(Me.pCovRobust(i, i))
            ElseIf pStdErrType = "Naive" Then
                Me.results.Coeffs_SEs(i) = Math.Sqrt(Me.pCovNaive(i, i))
            End If
        Next

        'quasi information criterion
        Me.EstimateQIC()
        If Me.bComputeResiduals Then Me.ComputeResiduals()

        Me.results.ModelTableLabels = {"Dep.Variable", "Family", "Link Function", "Dependence Structure", "Covariance Type",
                "# observations", "# clusters", "Min. Cluster Size", "Max. Cluster Size", "Mean Cluster Size",
                "Correlation Matrix Dimension", "Scale", "QIC", "QICu", "Quasi Likelihood", "Number of Iterations",
                "Relative Parameter Values Change", "Converged?"}
        Me.results.ModelTableTopRow = {"Model Analysis", "", "df"}
        Me.results.ModelTableVals = {{Me.pVarNames(0), ""},
                                     {Me.pFamily.ToString(), ""},
                                     {Me.pLink.ToString(), ""},
                                     {Me.pCovStruct.ToString(), ""},
                                     {Me.pStdErrType, ""},
                                     {Me.n, ""},
                                     {Me.pNoGroup, ""},
                                     {Me.pClusterSize.Min(), ""},
                                     {Me.pClusterSize.Max(), ""},
                                     {Me.pClusterSize.Average(), ""},
                                     {Me.pUniqueTimesDict.Count, ""},
                                     {Me.pScale, ""},
                                     {Me.pQIC, Me.p},
                                     {Me.pQICu, Me.p},
                                     {Me.pQL, ""},
                                     {Me.pItration, ""},
                                     {del_params, ""},
                                     {CStr(Me.pConverged), ""}}

        Me.CompTime = Microsoft.VisualBasic.DateAndTime.Timer - startTime
        If progressBar IsNot Nothing Then progressBar.Invoke(Sub()
                                                                 progressBar.Value = 100
                                                             End Sub)
    End Sub

    Private Function SafeStDevFromMu(mu As Double) As Double
        Dim v As Double = pFamily.Variance(mu)
        If Double.IsNaN(v) Then Return Double.NaN
        If v < VAR_EPS Then v = VAR_EPS
        Return Math.Sqrt(v)
    End Function

    Private Function ComputeBScovMat(cnaive(,) As Double) As Double(,)
        'Calculate the bias-corrected sandwich estimate of Mancl and DeRouen.
        BSlogg.Log("proc started: gee.ComputeBScovMat")

        Dim strTmpTrace As String = String.Empty, srt() As Double = Nothing

        cnaive = MatrixMult(cnaive, 1.0 / pScalingFactor)
        If pScale = 0 Then pScale = EstimateScale()

        Dim bcm(p - 1, p - 1) As Double
        For i = 0 To pNoGroup - 1
            Dim expval = pCachedMeans(i).Item1
            Dim lin_pred = pCachedMeans(i).Item2
            Dim endog = pEndogLi(i)
            Dim exog = pExogLi(i)

            Dim sdev(UBound(expval)) As Double ', resid(1 To UBound(expval))
            Dim resid = M_SUB(endog, expval)
            Dim dmat = MeanDeriv(exog, lin_pred, i, False)
            For j = 0 To UBound(expval)
                sdev(j) = SafeStDevFromMu(expval(j))
            Next

            Dim vinv_d(,) As Double = Nothing, vinv_resid() As Double = Nothing
            pCovStruct.covarianceMatrixSolve(expval, i, Me, sdev, dmat, resid, vinv_d, vinv_resid, strTmpTrace) ' vinv_d, vinv_resid - are results
            If strTmpTrace <> String.Empty Then BSlogg.Log($"strTmpTrace= {strTmpTrace}")

            vinv_d = MatrixMult(vinv_d, 1.0 / pScale)
            Dim hmat(,) As Double = MatrixMult(MatrixMult(vinv_d, cnaive), trans(dmat))
            hmat = trans(hmat)

            Dim tmp2 = M_SUB(IdentityMat(UBound(resid)), hmat)
            Dim tmp = Cholesky(tmp2)
            Dim aresid = CholSolve(tmp, resid)
            strTmpTrace = String.Empty
            pCovStruct.covarianceMatrixSolve(expval, i, Me, sdev, dmat, aresid, tmp2, srt, strTmpTrace) ' tmp2, srt - are results (reusing tmp2)
            If strTmpTrace <> String.Empty Then BSlogg.Log($"strTmpTrace= {strTmpTrace}")

            srt = GetColumnFrom2Darray(MatrixMult(trans(dmat), srt), 0)
            For j = 0 To UBound(srt)
                srt(j) /= pScale
            Next
            bcm = M_ADD(bcm, M_OUTERPRODUCT(srt, srt))
        Next

        ReDim pCovBiasCorr(p - 1, p - 1)
        Me.pCovBiasCorr = MatrixMult(cnaive, MatrixMult(bcm, cnaive))
        Me.pCovBiasCorr = MatrixMult(Me.pCovBiasCorr, pScalingFactor)

        Return pCovBiasCorr
    End Function

    Private Sub ComputeCovMat()
        'Returns the sampling covariance matrix of the regression parameters and related quantities.
        BSlogg.Log("proc started: gee.ComputeCovMat")
        Dim strTmpTrace As String = String.Empty

        Dim bmat(p - 1, p - 1) As Double, cmat(p - 1, p - 1) As Double
        For i = 0 To pNoGroup - 1
            Dim expval = pCachedMeans(i).Item1
            Dim lin_pred = pCachedMeans(i).Item2
            Dim endog = pEndogLi(i)
            Dim exog = pExogLi(i)

            Dim resid(UBound(expval)) As Double, sdev(UBound(expval)) As Double
            resid = M_SUB(endog, expval)
            Dim dmat = MeanDeriv(exog, lin_pred, i, False)
            For j = 0 To UBound(expval)
                sdev(j) = SafeStDevFromMu(expval(j))
            Next

            Dim wresid = resid
            Dim wdmat = dmat
            Dim vinv_d(,) As Double = Nothing, vinv_resid() As Double = Nothing
            pCovStruct.covarianceMatrixSolve(expval, i, Me, sdev, wdmat, wresid, vinv_d, vinv_resid, strTmpTrace) ' vinv_d, vinv_resid - are results
            If strTmpTrace <> String.Empty Then BSlogg.Log($"strTmpTrace= {strTmpTrace}")
            bmat = M_ADD(bmat, MatrixMult(trans(dmat), vinv_d))
            Dim dvinv_resid = MatrixMult(trans(dmat), vinv_resid)
            cmat = M_ADD(cmat, M_OUTERPRODUCT(GetColumnFrom2Darray(dvinv_resid, 0), GetColumnFrom2Darray(dvinv_resid, 0)))
        Next

        If pScale = 0 Then pScale = EstimateScale()
        BSlogg.Log($"bmatfull={array2str(bmat)}")
        ReDim pCovNaive(p - 1, p - 1), pCovRobust(p - 1, p - 1)
        'compute matrix inversion

        Dim bmatInv(,) As Double = MatInv(bmat, "CHOL",, bPseudInverse:=True)
        'Dim tmp(,) As Double = Cholesky(bmat, iErr, False)
        ''Debug.Print(array2str(tmp))
        'If iErr = 2 Then 'Matrix not positive-definite. Compute pseudoinverse
        '    BSlogg.Log($"WARNING: CHOLESKY. bmat not positive-definite. Calling pseudoInverse. bmat={array2str(bmat)}", LogMsgType.Warn)
        '    bmatInv = pseudoInverse(bmat)
        '    BSlogg.Log($"NOTE: pseudoInverse output ={array2str(bmatInv)}")
        'Else
        '    bmatInv = CholInv(tmp)
        'End If
        'Debug.Print(array2str(bmatInv))
        Me.pCovRobust = MatrixMult(bmatInv, MatrixMult(cmat, bmatInv))

        For i = 0 To p - 1
            For j = 0 To p - 1
                pCovNaive(i, j) = bmatInv(i, j) * pScale * pScalingFactor
                pCovRobust(i, j) = pCovRobust(i, j) * pScalingFactor
            Next
        Next

    End Sub

    Function EstimateScale(Optional bForce As Boolean = False) As Double
        'The scale parameter is estimated as the sum of squared Pearson residuals divided by

        BSlogg.Log("proc started: gee.estimateScale")
        If bForce Then GoTo 1

        If pScaleType = 0 And (TypeOf pFamily Is regression.Binomial Or TypeOf pFamily Is regression.Poisson Or TypeOf pFamily Is regression.NegativeBinomial) Then
            Return 1.0
        Else
1:          Dim estScale As Double = 0.0
            Dim fSum As Double = 0.0
            For i = 0 To pNoGroup - 1
                Dim expval = pCachedMeans(i).Item1
                Dim endog = pEndogLi(i)
                Dim ResId(UBound(expval)) As Double, sdev(UBound(expval)) As Double

                For j = 0 To UBound(expval)
                    sdev(j) = SafeStDevFromMu(expval(j))
                Next

                ' If any NaN stdev appears, bail out safely
                If sdev.Any(Function(z) Double.IsNaN(z)) Then Return Double.NaN

                ResId = M_DIV(M_SUB(endog, expval), sdev)

                For j = 0 To UBound(ResId)
                    estScale += ResId(j) * ResId(j)
                Next
                fSum += ResId.Length()
            Next

            estScale /= If(Me.pbUseP, (fSum * (n - p) / n), fSum)
            Return estScale
        End If

    End Function

    Private Sub updateMeanParams(ByRef update() As Double, ByRef score() As Double)
        'update and score is the output

        Dim strTmpTrace As String, bmat(p - 1, p - 1) As Double, score_(p - 1, 0) As Double
        BSlogg.Log("proc started: gee.updateMeanParams")
        For i = 0 To p - 1 : score_(i, 0) = 0 : Next

        For i = 0 To pNoGroup - 1
            Dim expval = pCachedMeans(i).Item1
            Dim lin_pred = pCachedMeans(i).Item2
            Dim endog = pEndogLi(i)
            Dim exog = pExogLi(i)

            Dim resid(UBound(expval)) As Double, sdev(UBound(expval)) As Double
            resid = M_SUB(endog, expval)
            Dim dmat(,) As Double = MeanDeriv(exog, lin_pred, i)
            For j = 0 To UBound(expval)
                sdev(j) = SafeStDevFromMu(expval(j))
            Next

            Dim wresid = resid
            Dim wdmat = dmat
            Dim vinv_d(,) As Double = Nothing, vinv_resid() As Double = Nothing
            strTmpTrace = String.Empty
            pCovStruct.covarianceMatrixSolve(expval, i, Me, sdev, wdmat, wresid, vinv_d, vinv_resid, strTmpTrace) ' vinv_d, vinv_resid - are results
            If strTmpTrace <> String.Empty Then BSlogg.Log($"strTmpTrace= {strTmpTrace}")

            bmat = M_ADD(bmat, MatrixMult(trans(dmat), vinv_d))
            score_ = M_ADD(score_, MatrixMult(trans(dmat), vinv_resid))
        Next
        'Debug.Print(array2str(score_))
        'Debug.Print(array2str(bmat))
        score = GetColumnFrom2Darray(score_, 0)
        BSlogg.Log($"bmatfull= {array2str(bmat)} scorefull={array2str(score)}")


        Dim tmp = MatInv(bmat, "CHOL",, bPseudInverse:=True)
        update = GetColumnFrom2Darray(MatrixMult(tmp, score_), 0)
        'Dim tmp = Cholesky(bmat, iErr, False) 'reusing tmp again
        'If iErr = 2 Then 'Matrix not positive-definite. Compute pseudoinverse
        '    BSlogg.Log($"WARNING: CHOLESKY. bmat not positive-definite. Calling pseudoInverse. bmat={array2str(bmat)}", LogMsgType.Warn)
        '    tmp = pseudoInverse(bmat)
        '    BSlogg.Log($"NOTE: pseudoInverse output ={array2str(tmp)}")
        '    update = GetColumnFrom2Darray(MatrixMult(tmp, score_), 0)
        'Else
        '    update = CholSolve(tmp, score)
        'End If

    End Sub

    Private Function MeanDeriv(exog(,) As Double, lin_pred(,) As Double, idx As Integer,
                               Optional bUseOffset As Boolean = True) As Double(,)
        'Returns: The value of the derivative of the expected endog with respect to the parameter vector.
        'Notes: If there is exposure, it should be added to lin_pred prior to calling this function.

        Dim idl(UBound(lin_pred)) As Double, dmat(UBound(exog), UBound(exog, 2)) As Double
        For i = 0 To UBound(lin_pred)
            If pbOffset And bUseOffset Then lin_pred(i, 0) += pOffsetLi(idx)(i)
            idl(i) = pLink.inverseDeriv(lin_pred(i, 0))
        Next

        For i = 0 To UBound(exog)
            For j = 0 To UBound(exog, 2)
                dmat(i, j) = exog(i, j) * idl(i)
            Next
        Next

        Return dmat
    End Function

    Private Sub UpdateCachedMeans(mean_params() As Double)
        'pCachedMeans should always contain the most recent calculation of the group-wise mean vectors. This sub should be
        'called every time the regression parameters are changed, to keep the cached means up to date.
        BSlogg.Log("proc started: gee.updateCachedMeans")

        Dim bFirstCall As Boolean = If(pCachedMeans.Count = 0, True, False)

        For i = 0 To pNoGroup - 1
            'Debug.Print(array2str(pExogLi(i)))
            Dim tmpExog(,) As Double = pExogLi(i)
            Dim lin_pred(,) As Double = MatrixMult(tmpExog, mean_params)

            Dim expval(UBound(tmpExog, 1)) As Double
            For j = 0 To UBound(tmpExog, 1)
                If pbOffset Then lin_pred(j, 0) = lin_pred(j, 0) + pOffsetLi(i)(j)
                expval(j) = pLink.inverse(lin_pred(j, 0))
            Next

            If bFirstCall Then
                pCachedMeans.Add((expval, lin_pred))
            Else
                pCachedMeans(i) = (expval, lin_pred)
            End If
        Next
    End Sub

    Private Function GetStartParams() As Double()
        'estimate starting parameters using the GLM fit

        BSlogg.Log("proc started: gee.getStartParams")

        Dim glm As New GLM(pFamily, pLink)
        With glm
            If pbOffset Then
                .data(Me.pData, Me.pRowNums, Me.pOffset)
            Else
                .data(Me.pData, Me.pRowNums)
            End If
            .bHosmerLemeshow = False
            .settingInputs(pAlpha, pMaxiter, pEps)
            .setVarNames(Me.pVarNames)
            .Calculate(1)
            pIndependenceNaiveVarCovar = .VarCovar
        End With
        BSlogg.Log($"start params: {array2str(glm.results.Coeffs_est)}")

        Return glm.results.Coeffs_est
    End Function

    Private Sub EstimateQIC()
        'Returns quasi-information criteria and quasi-likelihood values.
        'W. Pan (2001).  Akaike's information criterion in generalized estimating equations.  Biometrics (57) 1.
        Dim Trace As Double
        BSlogg.Log("proc started: gee.estimateQIC")

        For i = 0 To pNoGroup - 1
            Dim expval = pCachedMeans(i).Item1
            For j = 0 To UBound(expval)
                pQL += pFamily.geeQuasiLike(CDbl(pEndogLi(i)(j)), expval(j))
            Next
        Next

        Dim NaiveInv(,) As Double = MatInv(pIndependenceNaiveVarCovar)
        Dim tmp = MatrixMult(NaiveInv, pCovRobust)

        For i = 0 To UBound(tmp)
            Trace += tmp(i, i)
        Next
        pQICu = -2.0 * pQL + 2.0 * Me.p
        pQIC = -2.0 * pQL + 2.0 * Trace
    End Sub

    Private Sub PreProcessData()

        BSlogg.Log("proc started: Extracted Information:")
        Dim uniqueTimesColl = New Dictionary(Of Double, String)
        'data should be already sorted by repeats (clusetr/subject id) and within cluster order variable (time)
        Dim tmpGrp As Dictionary(Of Object, Integer) = Me.pRepeats.GroupBy(Function(x) x).
                                                                   ToDictionary(Function(g) g.Key,
                                                                                Function(g) g.Count())
        pNoGroup = tmpGrp.Count

        ReDim pGroupLabels(pNoGroup - 1), pClusterSize(pNoGroup - 1)

        Dim timeDim As Integer = 0
        Dim i As Integer = 0
        For Each grp In tmpGrp.Keys
            pGroupLabels(i) = grp
            pClusterSize(i) = tmpGrp(grp)
            Dim indices(pClusterSize(i) - 1) As Integer, tmpEndog(pClusterSize(i) - 1) As Double, tmpExog(pClusterSize(i) - 1, Me.p - 1) As Double
            Dim tmpOffset(pClusterSize(i) - 1) As Double, tmpTime(pClusterSize(i) - 1) As Double

            Dim k As Integer = 0
            For j = 0 To n - 1
                If pGroupLabels(i) = pRepeats(j).ToString() Then
                    indices(k) = j
                    tmpEndog(k) = pData(j, 0) 'Response/Endog variable should always be in the first column
                    tmpOffset(k) = pOffset(j)
                    If Not pbMissingTime Then
                        tmpTime(k) = pTimeRaw(j)
                        If Not uniqueTimesColl.ContainsKey(CDbl(pTimeRaw(j))) Then
                            uniqueTimesColl.Add(CDbl(pTimeRaw(j)), CStr(pTimeRaw(j)))
                        End If
                    End If

                    For ii = 0 To p - 1
                        If ii = 0 Then
                            tmpExog(k, ii) = 1 'intercept
                        Else
                            tmpExog(k, ii) = pData(j, ii) 'independent variables
                        End If
                    Next
                    k += 1
                End If
            Next j

            pGroupIndices.Add(pGroupLabels(i), indices)
            pEndogLi.Add(tmpEndog)
            pExogLi.Add(tmpExog)
            pOffsetLi.Add(tmpOffset)
            If pbMissingTime Then
                For k = 0 To pClusterSize(i) - 1
                    tmpTime(k) = CDbl(k)
                    Try
                        uniqueTimesColl.Add(CDbl(k), CStr(k))
                    Catch
                    End Try
                Next
            End If
            pTimeLi.Add(tmpTime)

            If timeDim < pClusterSize(i) Then timeDim = pClusterSize(i)
            i += 1
        Next

        ' set the covariance matrix dimensions (needed for the unstructureded or AR1 covariance structure types)
        ' the call will be ignored iternaly for other covariance structures
        Dim UniqueTimes() As Double = uniqueTimesColl.Keys.ToArray()

        Array.Sort(UniqueTimes)
        For i = 0 To uniqueTimesColl.Count - 1
            pUniqueTimesDict.Add(UniqueTimes(i), i)
        Next

        BSlogg.Log($"pGroupLabels={array2str(pGroupLabels)}")
        BSlogg.Log($"# of unique times: {uniqueTimesColl.Count}; UniqueTimes={array2str(UniqueTimes)}")
    End Sub


    ' Add this inside Class GEE
    ''' <summary>
    ''' Computes common GLM-style residuals for a fitted GEE mean model (marginal residuals).
    ''' </summary>
    ''' <param name="tol">
    ''' Small positive number used to guard divisions by near-zero values (variance, derivatives, scale).
    ''' </param>
    ''' <param name="useWeights">
    ''' If True and your model has weights, residuals are multiplied by sqrt(weight) where appropriate.
    ''' Note: your current EstimateScale() ignores weights, so leaving this False matches your scale logic.
    ''' </param>
    ''' <param name="scaleResiduals">
    ''' If True, also returns scaled variants (dividing Pearson/Deviance by sqrt(φ)).
    ''' </param>
    Private Sub ComputeResiduals(Optional tol As Double = 0.000000000001,
                                 Optional useWeights As Boolean = False,
                                 Optional scaleResiduals As Boolean = True)

        ' Ensure cached means correspond to the final fitted parameters (if available)
        If Me.results IsNot Nothing AndAlso Me.results.Coeffs_est IsNot Nothing Then Me.UpdateCachedMeans(Me.results.Coeffs_est)

        Dim Yresponse(Me.n - 1) As Double, Fitted(Me.n - 1) As Double, LinearPredictor(Me.n - 1) As Double
        ReDim pRawRes(Me.n - 1), pPearsonRes(Me.n - 1), pPearsonScaledRes(Me.n - 1)
        ReDim pDevianceRes(Me.n - 1), pDevianceScaledRes(Me.n - 1), pWorkingRes(Me.n - 1)

        Dim phi As Double = 1.0
        If scaleResiduals Then
            phi = GetResidualScale()
            If phi < tol Then phi = 1.0
        End If

        For g As Integer = 0 To pNoGroup - 1

            Dim mu() As Double = pCachedMeans(g).Item1
            Dim eta(,) As Double = pCachedMeans(g).Item2
            Dim y() As Double = pEndogLi(g)
            ' Original row indices for this cluster
            Dim idx() As Integer = pGroupIndices(pGroupLabels(g))

            For j As Integer = 0 To idx.Length - 1
                Dim row As Integer = idx(j)
                Dim yi As Double = y(j)
                Dim mui As Double = mu(j)
                Dim etai As Double = eta(j, 0)
                Dim wi As Double = 1.0
                If useWeights AndAlso pbWeights Then
                    wi = pWeights(row)
                    If wi < 0.0 Then wi = 0.0
                End If

                Dim ri As Double = yi - mui
                Dim vmu As Double = pFamily.Variance(mui)
                Dim dmu_deta As Double = pLink.inverseDeriv(etai)

                Yresponse(row) = yi
                Fitted(row) = mui
                LinearPredictor(row) = etai
                Me.pRawRes(row) = ri

                ' Pearson residual
                If vmu > tol Then
                    Dim pr As Double = ri / Math.Sqrt(vmu)
                    If useWeights Then pr *= Math.Sqrt(wi)
                    Me.pPearsonRes(row) = pr
                    Me.pPearsonScaledRes(row) = If(scaleResiduals, pr / Math.Sqrt(phi), pr)
                Else
                    Me.pPearsonRes(row) = Double.NaN
                    Me.pPearsonScaledRes(row) = Double.NaN
                End If

                ' Deviance residual: sign(y-mu)*sqrt(D_i)
                ' Family implements residDev_(y, mu) as the deviance contribution D_i.
                Dim Di As Double = pFamily.residDev_(yi, mui)
                If Di >= 0 AndAlso Not Double.IsNaN(Di) Then
                    Dim dr As Double = Math.Sign(ri) * Math.Sqrt(Di)
                    If useWeights Then dr *= Math.Sqrt(wi)
                    Me.pDevianceRes(row) = dr
                    Me.pDevianceScaledRes(row) = If(scaleResiduals, dr / Math.Sqrt(phi), dr)
                Else
                    Me.pDevianceRes(row) = Double.NaN
                    Me.pDevianceScaledRes(row) = Double.NaN
                End If

                ' Working residual: (y - mu)/(dmu/deta)
                If Math.Abs(dmu_deta) > tol Then
                    Dim wr As Double = ri / dmu_deta
                    If useWeights Then wr *= Math.Sqrt(wi)
                    Me.pWorkingRes(row) = wr
                Else
                    Me.pWorkingRes(row) = Double.NaN
                End If

            Next
        Next
    End Sub

    ''' <summary>
    ''' Returns the scale parameter φ to use for scaled residuals, matching your EstimateScale convention.
    ''' </summary>
    Private Function GetResidualScale() As Double
        ' Match EstimateScale(): for Binomial/Poisson/NegBin with pScaleType=0 => phi = 1
        If pScaleType = 0 AndAlso (TypeOf pFamily Is regression.Binomial OrElse
                                   TypeOf pFamily Is regression.Poisson OrElse
                                   TypeOf pFamily Is regression.NegativeBinomial) Then
            Return 1.0
        End If

        ' Otherwise use current pScale if set; else estimate it
        If pScale > 0 Then Return pScale
        Return EstimateScale(True)
    End Function

End Class
