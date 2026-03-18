Option Explicit On
Imports System.Drawing
Imports System.Linq
Imports System.Security.Cryptography
Imports System.Windows.Forms.VisualStyles.VisualStyleElement.TrackBar
Imports BESHStatNG.AppInfrastructure
Imports Microsoft.Office.Interop.Excel

''' <summary>
''' Fits a marginal regression model for clustered/longitudinal data using
''' <b>Generalized Estimating Equations (GEE)</b>.
''' </summary>
''' <remarks>
''' <para>
''' This class estimates regression parameters <c>β</c> for a generalized linear mean model while allowing
''' for within-cluster correlation via a user-supplied working covariance structure (<see cref="regression.GEEcovStruct"/>).
''' </para>
'''
''' <h3>Mean model</h3>
''' <para>
''' For observation <c>j</c> in cluster <c>i</c>:
''' </para>
''' <para>
''' <c>ηᵢⱼ = xᵢⱼᵀ β + oᵢⱼ</c>  (optionally includes offset <c>oᵢⱼ</c>)
''' </para>
''' <para>
''' <c>μᵢⱼ = g⁻¹(ηᵢⱼ)</c> where <c>g</c> is the link (<see cref="regression.Link"/>).
''' </para>
'''
''' <h3>Working covariance</h3>
''' <para>
''' Let <c>yᵢ</c> and <c>μᵢ</c> be the response and mean vectors for cluster <c>i</c> with size <c>mᵢ</c>.
''' The working covariance is typically of the form:
''' </para>
''' <para>
''' <c>Vᵢ = φ Aᵢ^(1/2) R(α) Aᵢ^(1/2)</c>
''' </para>
''' <para>
''' where <c>Aᵢ</c> is diagonal with entries <c>Var(μᵢⱼ)</c> (as defined by the family),
''' <c>R(α)</c> is the working correlation matrix parameterized by association parameters <c>α</c>,
''' and <c>φ</c> is a scale/dispersion parameter.
''' </para>
'''
''' <h3>Estimating equation (score)</h3>
''' <para>
''' The GEE score is:
''' </para>
''' <para>
''' <c>U(β) = Σᵢ Dᵢᵀ Vᵢ⁻¹ (yᵢ − μᵢ) = 0</c>
''' </para>
''' <para>
''' with derivative matrix:
''' </para>
''' <para>
''' <c>Dᵢ = ∂μᵢ/∂β = diag(dμ/dη) Xᵢ</c>.
''' </para>
'''
''' <h3>Parameter update used here</h3>
''' <para>
''' This implementation uses a Fisher-scoring / IRLS-like step:
''' </para>
''' <para>
''' <c>β(new) = β(old) + (B)⁻¹ U</c>
''' </para>
''' <para>
''' where <c>B = Σᵢ Dᵢᵀ Vᵢ⁻¹ Dᵢ</c> and <c>U = Σᵢ Dᵢᵀ Vᵢ⁻¹ (yᵢ − μᵢ)</c>.
''' </para>
'''
''' <h3>Covariance of β</h3>
''' <para>
''' Model-based (naive): <c>Var_naive(β̂) = φ B⁻¹</c>
''' </para>
''' <para>
''' Robust (sandwich): <c>Var_robust(β̂) = B⁻¹ C B⁻¹</c>, where
''' <c>C = Σᵢ uᵢ uᵢᵀ</c> and <c>uᵢ = Dᵢᵀ Vᵢ⁻¹ (yᵢ − μᵢ)</c>.
''' </para>
''' <para>
''' Optional bias-reduced sandwich covariance is also available (Mancl–DeRouen-style),
''' matching your <c>ComputeBScovMat</c> implementation.
''' </para>
'''
''' <h3>Data layout expectations</h3>
''' <para>
''' <see cref="data"/> assumes the response variable is stored in column 0 of <c>pData</c>.
''' Columns 1..(p−1) are predictors. An intercept column is added internally.
''' Clusters are defined by the <c>repeat()</c> array (e.g., subject id).
''' </para>
''' </remarks>
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

    ''' <summary>
    ''' Holds fitted model results (coefficients, standard errors, diagnostics tables, etc.).
    ''' Populated after <see cref="Fit"/> completes.
    ''' </summary>
    ''' <remarks>
    ''' This instance is created inside <see cref="Fit"/> and then filled with coefficient estimates,
    ''' standard errors (according to the selected covariance type), convergence metadata, QIC/QICu,
    ''' and other model diagnostics.
    ''' </remarks>
    Public results As LMresult

    ''' <summary>
    ''' If <c>True</c>, <see cref="wrapResults"/> will include an iteration trace table
    ''' (parameter values and the convergence criterion per iteration).
    ''' </summary>
    Public bIterationDetails As Boolean = False

    ''' <summary>
    ''' If <c>True</c>, <see cref="Fit"/> will compute residual arrays via <c>ComputeResiduals</c>.
    ''' </summary>
    Public bComputeResiduals As Boolean = False

    ''' <summary>
    ''' Optional starting parameter vector <c>β₀</c> used when <see cref="Fit"/> is called with
    ''' <c>bStartParams:=True</c>.
    ''' </summary>
    ''' <remarks>
    ''' Expected length is <see cref="Nparams"/> (including the intercept).
    ''' If not provided (or if <c>bStartParams</c> is False), starting values are obtained by fitting
    ''' a corresponding GLM under independence via <c>GetStartParams</c>.
    ''' </remarks>
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

    ''' <summary>
    ''' Initializes a new GEE model with the given family, link, and working covariance structure.
    ''' </summary>
    ''' <param name="fam">
    ''' The exponential-family object providing at minimum:
    ''' variance function <c>Var(μ)</c>, deviance contributions, and quasi-likelihood pieces used for QIC.
    ''' </param>
    ''' <param name="lin">
    ''' The link function <c>g</c> with inverse <c>g⁻¹</c> and derivative of the inverse <c>dμ/dη</c>.
    ''' </param>
    ''' <param name="covStr">
    ''' Working covariance structure used to define/solve with <c>Vᵢ</c> (and to update association parameters).
    ''' </param>
    ''' <param name="strSEtype">
    ''' Standard error / covariance estimator type. Expected values in this implementation:
    ''' <c>"Robust"</c>, <c>"Naive"</c>, or <c>"Bias Reduced"</c>.
    ''' </param>
    ''' <remarks>
    ''' You must call <see cref="data"/> before <see cref="Fit"/>.
    ''' </remarks>
    Public Sub New(fam As regression.Family, lin As regression.Link, covStr As regression.GEEcovStruct, Optional strSEtype As String = "Robust") ' make sure these object are created and ready at the very beginning
        Me.pFamily = fam
        Me.pLink = lin
        Me.pCovStruct = covStr
        Me.pStdErrType = strSEtype
    End Sub

    ''' <summary>
    ''' Sets general fitting controls used by the iterative GEE solver.
    ''' </summary>
    ''' <param name="dAlpha">Significance level used for p-values / confidence interpretation in result formatting.</param>
    ''' <param name="lMaxiter">Maximum number of GEE iterations allowed.</param>
    ''' <param name="dEps">
    ''' Convergence tolerance for the SAS-style “max parameter change” criterion used in <see cref="Fit"/>.
    ''' </param>
    ''' <param name="bUseP">
    ''' If <c>True</c>, the scale estimate divisor is adjusted by a degrees-of-freedom factor as implemented in <see cref="EstimateScale"/>.
    ''' </param>
    ''' <remarks>
    ''' <para>
    ''' Convergence criterion used in <see cref="Fit"/>:
    ''' </para>
    ''' <para>
    ''' For each parameter <c>βⱼ</c>, compute absolute change <c>|βⱼ(new) − βⱼ(old)|</c>.
    ''' If <c>|βⱼ(new)|</c> is larger than a fixed threshold (0.08 in the code), use relative change
    ''' <c>|βⱼ(new) − βⱼ(old)| / |βⱼ(new)|</c>. The iteration’s criterion is the maximum over <c>j</c>.
    ''' </para>
    ''' <para>
    ''' The fit is declared converged only after this criterion is below <c>dEps</c> for two consecutive iterations,
    ''' and at least one association-parameter update has occurred.
    ''' </para>
    ''' </remarks>
    Public Sub settingInputs(dAlpha As Double, lMaxiter As Long, dEps As Double, bUseP As Boolean)
        pAlpha = dAlpha
        pMaxiter = lMaxiter
        pEps = dEps
        pbUseP = bUseP
    End Sub

    ''' <summary>
    ''' Supplies the raw observation-level dataset and cluster identifiers to the model and performs preprocessing.
    ''' </summary>
    ''' <param name="data">
    ''' Rectangular array of shape (n × k) in which column 0 is the response <c>y</c>.
    ''' Columns 1..(k−1) are predictors. An intercept is added internally, so the fitted parameter count is:
    ''' <c>p = k</c> (intercept + (k−1) predictors).
    ''' </param>
    ''' <param name="repeat">
    ''' Cluster/subject identifier for each row (length n). Observations with the same <c>repeat</c> value form one cluster.
    ''' </param>
    ''' <param name="RowNums">
    ''' Optional mapping of model-row indices to original row numbers. If omitted, uses 0..n−1.
    ''' This is used primarily for reporting / alignment back to the original dataset.
    ''' </param>
    ''' <param name="Offset">
    ''' Optional offset vector <c>o</c> (length n) added to the linear predictor:
    ''' <c>η = Xβ + o</c>. If omitted, offsets are treated as 0 and <c>pbOffset</c> is False.
    ''' </param>
    ''' <param name="Weights">
    ''' Optional nonnegative weights (length n). If omitted, all weights are 1 and <c>pbWeights</c> is False.
    ''' <b>Note:</b> in your current implementation, weights are not incorporated into the scale estimate
    ''' and are not explicitly applied in the core estimating equation unless the covariance structure uses them.
    ''' </param>
    ''' <param name="time">
    ''' Optional within-cluster ordering variable (length n). If omitted, synthetic times 0..(mᵢ−1) are generated per cluster.
    ''' Used to build the “unique times” dictionary required by certain correlation structures (e.g., AR(1), unstructured).
    ''' </param>
    ''' <exception cref="ArgumentException">
    ''' Thrown if the number of observations <c>n</c> is not greater than the number of parameters <c>p</c>.
    ''' </exception>
    ''' <remarks>
    ''' <para>
    ''' This method calls <c>PreProcessData()</c> which:
    ''' </para>
    ''' <list type="bullet">
    ''' <item><description>Groups rows by <paramref name="repeat"/> into clusters <c>i = 1..G</c>.</description></item>
    ''' <item><description>Builds per-cluster arrays <c>yᵢ</c>, <c>Xᵢ</c>, offsets, and times.</description></item>
    ''' <item><description>Adds an intercept column to each <c>Xᵢ</c>.</description></item>
    ''' <item><description>Builds <see cref="UniqueTimesDict"/> (sorted) needed for correlation matrix dimensioning.</description></item>
    ''' </list>
    ''' </remarks>
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
            AppGlobals.BSerr.LogAndThrow(New ArgumentException($"Not enough observations to fit model: n={n}, parameters={p}. Need n > p."))
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
            Me.pWeights = Matrix.IdentityVect(Me.n - 1, 1) 'it automaticaly assign zeros
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

    ''' <summary>
    ''' Stores variable names for reporting (tables/footnotes) and optional special-variable labels.
    ''' </summary>
    ''' <param name="names">
    ''' Array of variable names where index 0 corresponds to the dependent variable name,
    ''' and indices 1.. correspond to predictor names (excluding the intercept).
    ''' </param>
    ''' <param name="strClusterIDname">Name of the cluster id variable (for reporting/footnotes).</param>
    ''' <param name="strOffsetName">Optional name of the offset variable (for reporting/footnotes).</param>
    ''' <param name="strWeightsName">Optional name of the weights variable (for reporting/footnotes).</param>
    ''' <param name="strTimeName">Optional name of the within-cluster time/order variable (for reporting/footnotes).</param>
    ''' <remarks>
    ''' These names do not affect estimation; they are used in <see cref="wrapResults"/> and result tables.
    ''' </remarks>
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

    ''' <summary>
    ''' Returns fitted marginal means <c>μ</c> for all observations in (cluster-concatenated) row order.
    ''' </summary>
    ''' <value>
    ''' A length-<c>n</c> vector containing the most recently cached fitted means.
    ''' </value>
    ''' <remarks>
    ''' Values are taken from <see cref="CachedMeans"/> which is updated whenever <c>β</c> changes
    ''' (see <c>UpdateCachedMeans</c>). The ordering follows the preprocessing cluster loop used by the class.
    ''' </remarks>
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

    ''' <summary>
    ''' Returns a table-like object containing multiple residual types per observation.
    ''' </summary>
    ''' <value>
    ''' An object matrix shaped (n × 6) in the following column order:
    ''' Raw, Deviance, Pearson, Std Deviance, Std Pearson, Working.
    ''' </value>
    ''' <remarks>
    ''' Residuals are computed only if <see cref="bComputeResiduals"/> is True (and <see cref="Fit"/> ran to completion).
    ''' Definitions used:
    ''' <list type="bullet">
    ''' <item><description><b>Raw</b>: <c>r = y − μ</c></description></item>
    ''' <item><description><b>Pearson</b>: <c>r / sqrt(Var(μ))</c></description></item>
    ''' <item><description><b>Scaled Pearson</b>: <c>(Pearson) / sqrt(φ)</c></description></item>
    ''' <item><description><b>Deviance</b>: <c>sign(y−μ) * sqrt(Dᵢ)</c>, with <c>Dᵢ</c> from the family deviance contribution</description></item>
    ''' <item><description><b>Scaled Deviance</b>: <c>(Deviance) / sqrt(φ)</c></description></item>
    ''' <item><description><b>Working</b>: <c>(y−μ) / (dμ/dη)</c></description></item>
    ''' </list>
    ''' </remarks>
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

    ''' <summary>
    ''' Dictionary of unique time values to contiguous indices (0..T−1) used by certain covariance structures.
    ''' </summary>
    ''' <remarks>
    ''' Built in <c>PreProcessData()</c> by sorting unique time values within the dataset (or synthetic times if missing).
    ''' </remarks>
    Public ReadOnly Property TimesDict() As Dictionary(Of Double, Integer)
        Get
            Return Me.pUniqueTimesDict
        End Get
    End Property

    ''' <summary>
    ''' Per-cluster time arrays aligned with clustered endog/exog arrays.
    ''' </summary>
    ''' <remarks>
    ''' If no time is supplied, synthetic times 0..(mᵢ−1) are assigned per cluster.
    ''' </remarks>
    Public ReadOnly Property TimeClustered() As List(Of Double())
        Get
            Return Me.pTimeLi
        End Get
    End Property

    ''' <summary>
    ''' Cached mean vectors and linear predictors per cluster.
    ''' </summary>
    ''' <value>
    ''' A list of tuples <c>(μᵢ, ηᵢ)</c> for clusters <c>i</c>, where:
    ''' <c>μᵢ</c> is a vector of fitted means and <c>ηᵢ</c> is an (mᵢ × 1) array of linear predictors.
    ''' </value>
    ''' <remarks>
    ''' Updated by <c>UpdateCachedMeans(β)</c> after each parameter update and before covariance computations.
    ''' </remarks>
    Public ReadOnly Property CachedMeans() As List(Of (Double(), Double(,)))
        Get
            Return Me.pCachedMeans
        End Get
    End Property

    ''' <summary>
    ''' Clustered dependent-variable vectors <c>yᵢ</c>.
    ''' </summary>
    Public ReadOnly Property EndogClustered() As List(Of Double())
        Get
            Return Me.pEndogLi
        End Get
    End Property

    ''' <summary>
    ''' Alias for <see cref="TimesDict"/> (unique time → index mapping).
    ''' </summary>
    Public ReadOnly Property UniqueTimesDict() As Dictionary(Of Double, Integer)
        Get
            Return Me.pUniqueTimesDict
        End Get
    End Property

    ''' <summary>
    ''' Gets the family used to define the mean/variance relationship and deviance/quasi-likelihood calculations.
    ''' </summary>
    Public ReadOnly Property Family() As regression.Family
        Get
            Return Me.pFamily
        End Get
    End Property

    ''' <summary>
    ''' Number of clusters (groups) discovered in preprocessing.
    ''' </summary>
    Public ReadOnly Property NoGroup() As Integer
        Get
            Return Me.pNoGroup
        End Get
    End Property

    ''' <summary>
    ''' Number of regression parameters <c>p</c> including intercept.
    ''' </summary>
    Public ReadOnly Property Nparams() As Integer
        Get
            Return Me.p
        End Get
    End Property

    ''' <summary>
    ''' Number of observations <c>n</c>.
    ''' </summary>
    Public ReadOnly Property Nobs() As Integer
        Get
            Return Me.n
        End Get
    End Property

    ''' <summary>
    ''' Gets whether the scale estimator applies the additional degrees-of-freedom adjustment implemented in <see cref="EstimateScale"/>.
    ''' </summary>
    Public ReadOnly Property UseP() As Boolean
        Get
            Return Me.pbUseP
        End Get
    End Property

    ''' <summary>
    ''' Indicates whether a time/order variable was supplied (<c>True</c>) or synthesized (<c>False</c>).
    ''' </summary>
    Public ReadOnly Property hasTime() As Boolean
        Get
            Return Not Me.pbMissingTime
        End Get
    End Property

    ''' <summary>
    ''' Residual degrees of freedom <c>n − p</c>.
    ''' </summary>
    Public ReadOnly Property DFresid() As Integer
        Get
            Return Me.pDFresid
        End Get
    End Property

    ''' <summary>
    ''' Wraps the fitted model output into a list of <c>ResultTable</c> objects for presentation.
    ''' </summary>
    ''' <returns>
    ''' A list that typically includes:
    ''' (1) coefficient table with standard errors and p-values,
    ''' (2) model diagnostics table,
    ''' (3) working correlation matrix,
    ''' (4) covariance matrix (naive),
    ''' (5) covariance matrix (robust),
    ''' and optionally (6) bias-reduced covariance and (7) iteration trace.
    ''' </returns>
    ''' <remarks>
    ''' <para>
    ''' This method assumes <see cref="Fit"/> has already populated:
    ''' <c>results</c>, <c>pCovStruct.DepParams</c>, <c>pCovNaive</c>, <c>pCovRobust</c>,
    ''' and (if selected) <c>pCovBiasCorr</c>.
    ''' </para>
    ''' <para>
    ''' Variable labels used in tables are based on <see cref="setVarNames"/>:
    ''' predictor names are reported without the dependent variable name and with an explicit "Intercept".
    ''' </para>
    ''' </remarks>
    Public Function wrapResults() As List(Of ResultTable)
        Dim out As New List(Of ResultTable)
        Dim t = New ResultTable

        'coefficients, SE table
        t = Me.results.CoeffsZ_toPrint()
        t.AddPvalueToFormat(4)
        If Me.pOffsetVarName IsNot Nothing Then t.AddFootnote($"Offset Variable: {Me.pOffsetVarName}")
        If Me.pWeightsVarName IsNot Nothing Then t.AddFootnote($"Weights Variable: {Me.pWeightsVarName}")
        If Me.startParams IsNot Nothing Then t.AddFootnote($"Starting values: {Matrix.array2str(Me.startParams)}")
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
        t.AddTitle("Working Correlation MatrixType")
        out.Add(t)

        'Covariance MatrixType - Model based (Naive)
        Dim strVars() As String = Matrix.ConcatArrays({"Intercept"}, Matrix.SubsetArray(pVarNames, 1))
        t = New ResultTable
        t.SetBody(Me.pCovNaive)
        t.AddHeaderLeftRow(strVars)
        t.AddHeaderTopRow(strVars)
        t.AddTitle("Covariance MatrixType - Model based (Naive)")
        out.Add(t)

        'Covariance MatrixType - Model based (Naive)
        t = New ResultTable
        t.SetBody(Me.pCovRobust)
        t.AddHeaderLeftRow(strVars)
        t.AddHeaderTopRow(strVars)
        t.AddTitle("Covariance MatrixType - Empirical (Robust)")
        out.Add(t)

        'Covariance MatrixType - Bias Reduced
        If Me.pStdErrType = "Bias Reduced" Then
            t = New ResultTable
            t.SetBody(Me.pCovBiasCorr)
            t.AddHeaderLeftRow(strVars)
            t.AddHeaderTopRow(strVars)
            t.AddTitle("Covariance MatrixType - Bias Reduced")
            out.Add(t)
        End If

        'iteration info
        If Me.bIterationDetails Then
            t = New ResultTable
            t.SetBody(Me.pItInfo)
            Dim ItLabels(Me.pItration) As String
            For i = 0 To Me.pItration : ItLabels(i) = $"Iteration {i + 1}" : Next
            t.AddHeaderTopRow(ItLabels)
            t.AddHeaderLeftRow(Matrix.ConcatArrays(Me.pVarNames, {"Parameter Change"}))
            out.Add(t)
        End If

        Return out
    End Function

    ''' <summary>
    ''' Fits the GEE mean model by iterating between mean-parameter updates and association-parameter updates.
    ''' </summary>
    ''' <param name="bStartParams">
    ''' If <c>True</c>, uses <see cref="startParams"/> as the initial <c>β</c>.
    ''' Otherwise obtains starting values from an independence GLM fit (<c>GetStartParams</c>).
    ''' </param>
    ''' <param name="scalingFactor">
    ''' Multiplicative scaling applied to the covariance matrices after computation.
    ''' This class stores it as <c>pScalingFactor</c> and applies it in <c>ComputeCovMat</c>/<c>ComputeBScovMat</c>.
    ''' </param>
    ''' <param name="progressBar">Optional UI progress bar updated during fitting.</param>
    ''' <param name="progressLbl">Optional UI label updated with iteration/timing and last criterion value.</param>
    ''' <remarks>
    ''' <h3>Iteration structure</h3>
    ''' <para>
    ''' For iteration <c>t</c>:
    ''' </para>
    ''' <list type="number">
    ''' <item>
    ''' <description>
    ''' Compute mean step: obtain <c>U(β)</c> and <c>B(β)</c> and update
    ''' <c>β ← β + B⁻¹U</c>.
    ''' </description>
    ''' </item>
    ''' <item>
    ''' <description>
    ''' Update cached means <c>μᵢ</c> and <c>ηᵢ</c> for all clusters.
    ''' </description>
    ''' </item>
    ''' <item>
    ''' <description>
    ''' Check convergence using max(abs/relative) change in <c>β</c>.
    ''' Requires two consecutive iterations below <c>pEps</c>.
    ''' </description>
    ''' </item>
    ''' <item>
    ''' <description>
    ''' Update working association parameters <c>α</c> via <c>pCovStruct.updateAssoc(Me, ...)</c>.
    ''' </description>
    ''' </item>
    ''' </list>
    '''
    ''' <h3>After convergence / max iterations</h3>
    ''' <para>
    ''' The method computes:
    ''' </para>
    ''' <list type="bullet">
    ''' <item><description>Scale <c>φ</c> via <see cref="EstimateScale"/>.</description></item>
    ''' <item><description>Naive and robust covariance matrices via <c>ComputeCovMat</c>.</description></item>
    ''' <item><description>Optional bias-reduced covariance via <c>ComputeBScovMat</c> when <c>pStdErrType="Bias Reduced"</c>.</description></item>
    ''' <item><description>QIC and QICu via <c>EstimateQIC</c>.</description></item>
    ''' <item><description>Optional residual arrays via <c>ComputeResiduals</c>.</description></item>
    ''' </list>
    ''' </remarks>
    Public Sub Fit(bStartParams As Boolean,
                         Optional scalingFactor As Double = 1.0#,
                         Optional progressBar As System.Windows.Forms.ProgressBar = Nothing,
                         Optional progressLbl As System.Windows.Forms.Label = Nothing)
        AppGlobals.BSlogg.Log("proc started: gee.Fit")
        Dim update() As Double = Nothing, score() As Double = Nothing, del_params As Double, strTmpTrace As String = String.Empty
        Dim startTime As Double = Microsoft.VisualBasic.DateAndTime.Timer
        Me.pScalingFactor = scalingFactor
        Me.results = New LMresult
        Me.results.varNames = Matrix.SubsetArray(pVarNames, 1)
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
            AppGlobals.BSlogg.Log($"Iteration={pItration + 1} update:{Matrix.array2str(update)} score: {Matrix.array2str(score)}")

            'Apply step (same as your current code)
            meanParams = Matrix.M_ADD(meanParams, update)
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

            AppGlobals.BSlogg.Log($"Iteration={pItration + 1} meanParams:{Matrix.array2str(meanParams)} sas_del={del_params} consecOK={consecOK}")

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
            If strTmpTrace <> String.Empty Then AppGlobals.BSlogg.Log($"strTmpTrace= {strTmpTrace}")

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

        If Not pConverged Then AppGlobals.BSlogg.Log($"Iteration limit reached prior to convergence", AppGlobals.LogMsgType.Warn)
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
                "Correlation MatrixType Dimension", "Scale", "QIC", "QICu", "Quasi Likelihood", "Number of Iterations",
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

    ''' <summary>
    ''' Returns <c>sqrt(Var(μ))</c> with a small lower bound for numerical stability.
    ''' </summary>
    ''' <param name="mu">Mean value <c>μ</c> at which to evaluate the family variance function.</param>
    ''' <returns>
    ''' <c>sqrt(max(Var(μ), VAR_EPS))</c>, or <see cref="Double.NaN"/> if the variance evaluation returns NaN.
    ''' </returns>
    ''' <remarks>
    ''' Many algorithms in this class divide by <c>sqrt(Var(μ))</c>. Bounding away from zero avoids blow-ups when
    ''' the mean approaches boundaries where variance becomes extremely small.
    ''' </remarks>
    Private Function SafeStDevFromMu(mu As Double) As Double
        Dim v As Double = pFamily.Variance(mu)
        If Double.IsNaN(v) Then Return Double.NaN
        If v < VAR_EPS Then v = VAR_EPS
        Return Math.Sqrt(v)
    End Function

    ''' <summary>
    ''' Computes the bias-corrected (bias-reduced) sandwich covariance estimator following Mancl–DeRouen style logic.
    ''' </summary>
    ''' <param name="cnaive">
    ''' The model-based covariance matrix used as the baseline, typically <c>φ B⁻¹</c> (possibly scaled).
    ''' </param>
    ''' <returns>Bias-reduced covariance matrix for <c>β̂</c>.</returns>
    ''' <remarks>
    ''' <para>
    ''' Robust sandwich covariance can be downward biased when the number of clusters is small.
    ''' Bias-reduced methods adjust cluster contributions using a leverage-type correction.
    ''' </para>
    ''' <para>
    ''' In broad terms, the correction uses a cluster “hat” matrix:
    ''' </para>
    ''' <para>
    ''' <c>Hᵢ = Dᵢ B⁻¹ Dᵢᵀ Vᵢ⁻¹</c>
    ''' </para>
    ''' <para>
    ''' and forms adjusted residual-like quantities involving <c>(I − Hᵢ)</c>.
    ''' Your code constructs a Cholesky factor of <c>(I − Hᵢ)</c> and transforms residuals accordingly before
    ''' recomputing cluster score contributions, and finally returns:
    ''' </para>
    ''' <para>
    ''' <c>Var_bc(β̂) = B⁻¹ (Σᵢ uᵢ* uᵢ*ᵀ) B⁻¹</c>
    ''' </para>
    ''' <para>
    ''' where <c>uᵢ*</c> are the adjusted cluster score contributions produced by the transformation.
    ''' </para>
    ''' <para>
    ''' The implementation also respects the class scaling factor (<c>pScalingFactor</c>).
    ''' </para>
    ''' </remarks>
    Private Function ComputeBScovMat(cnaive(,) As Double) As Double(,)
        'Fit the bias-corrected sandwich estimate of Mancl and DeRouen.
        AppGlobals.BSlogg.Log("proc started: gee.ComputeBScovMat")

        Dim strTmpTrace As String = String.Empty, srt() As Double = Nothing

        cnaive = Matrix.MatrixMult(cnaive, 1.0 / pScalingFactor)
        If pScale = 0 Then pScale = EstimateScale()

        Dim bcm(p - 1, p - 1) As Double
        For i = 0 To pNoGroup - 1
            Dim expval = pCachedMeans(i).Item1
            Dim lin_pred = pCachedMeans(i).Item2
            Dim endog = pEndogLi(i)
            Dim exog = pExogLi(i)

            Dim sdev(UBound(expval)) As Double ', resid(1 To UBound(expval))
            Dim resid = Matrix.M_SUB(endog, expval)
            Dim dmat = MeanDeriv(exog, lin_pred, i, False)
            For j = 0 To UBound(expval)
                sdev(j) = SafeStDevFromMu(expval(j))
            Next

            Dim vinv_d(,) As Double = Nothing, vinv_resid() As Double = Nothing
            pCovStruct.covarianceMatrixSolve(expval, i, Me, sdev, dmat, resid, vinv_d, vinv_resid, strTmpTrace) ' vinv_d, vinv_resid - are results
            If strTmpTrace <> String.Empty Then AppGlobals.BSlogg.Log($"strTmpTrace= {strTmpTrace}")

            vinv_d = Matrix.MatrixMult(vinv_d, 1.0 / pScale)
            Dim hmat(,) As Double = Matrix.MatrixMult(Matrix.MatrixMult(vinv_d, cnaive), Matrix.trans(dmat))
            hmat = Matrix.trans(hmat)

            Dim tmp2 = Matrix.M_SUB(Matrix.IdentityMat(UBound(resid)), hmat)
            Dim tmp = Matrix.Cholesky(tmp2)
            Dim aresid = Matrix.CholSolve(tmp, resid)
            strTmpTrace = String.Empty
            pCovStruct.covarianceMatrixSolve(expval, i, Me, sdev, dmat, aresid, tmp2, srt, strTmpTrace) ' tmp2, srt - are results (reusing tmp2)
            If strTmpTrace <> String.Empty Then AppGlobals.BSlogg.Log($"strTmpTrace= {strTmpTrace}")

            srt = Matrix.GetColumnFrom2Darray(Matrix.MatrixMult(Matrix.trans(dmat), srt), 0)
            For j = 0 To UBound(srt)
                srt(j) /= pScale
            Next
            bcm = Matrix.M_ADD(bcm, Matrix.M_OUTERPRODUCT(srt, srt))
        Next

        ReDim pCovBiasCorr(p - 1, p - 1)
        Me.pCovBiasCorr = Matrix.MatrixMult(cnaive, Matrix.MatrixMult(bcm, cnaive))
        Me.pCovBiasCorr = Matrix.MatrixMult(Me.pCovBiasCorr, pScalingFactor)

        Return pCovBiasCorr
    End Function

    ''' <summary>
    ''' Computes both naive (model-based) and robust (sandwich) covariance matrices for <c>β̂</c>.
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' Let <c>B = Σᵢ Dᵢᵀ Vᵢ⁻¹ Dᵢ</c> and <c>uᵢ = Dᵢᵀ Vᵢ⁻¹ (yᵢ − μᵢ)</c>.
    ''' This method computes:
    ''' </para>
    ''' <para>
    ''' <c>Var_naive = φ B⁻¹</c>
    ''' </para>
    ''' <para>
    ''' <c>Var_robust = B⁻¹ (Σᵢ uᵢ uᵢᵀ) B⁻¹</c>
    ''' </para>
    ''' <para>
    ''' Numerically, your code:
    ''' </para>
    ''' <list type="bullet">
    ''' <item><description>accumulates <c>B</c> and <c>C = Σ uᵢuᵢᵀ</c> cluster-by-cluster,</description></item>
    ''' <item><description>computes <c>B⁻¹</c> via Cholesky-based inversion with pseudoinverse fallback,</description></item>
    ''' <item><description>sets <c>Var_robust = B⁻¹ C B⁻¹</c>,</description></item>
    ''' <item><description>sets <c>Var_naive = (B⁻¹) * φ</c>, then applies <c>pScalingFactor</c>.</description></item>
    ''' </list>
    ''' </remarks>
    Private Sub ComputeCovMat()
        'Returns the sampling covariance matrix of the regression parameters and related quantities.
        AppGlobals.BSlogg.Log("proc started: gee.ComputeCovMat")
        Dim strTmpTrace As String = String.Empty

        Dim bmat(p - 1, p - 1) As Double, cmat(p - 1, p - 1) As Double
        For i = 0 To pNoGroup - 1
            Dim expval = pCachedMeans(i).Item1
            Dim lin_pred = pCachedMeans(i).Item2
            Dim endog = pEndogLi(i)
            Dim exog = pExogLi(i)

            Dim resid(UBound(expval)) As Double, sdev(UBound(expval)) As Double
            resid = Matrix.M_SUB(endog, expval)
            Dim dmat = MeanDeriv(exog, lin_pred, i, False)
            For j = 0 To UBound(expval)
                sdev(j) = SafeStDevFromMu(expval(j))
            Next

            Dim wresid = resid
            Dim wdmat = dmat
            Dim vinv_d(,) As Double = Nothing, vinv_resid() As Double = Nothing
            pCovStruct.covarianceMatrixSolve(expval, i, Me, sdev, wdmat, wresid, vinv_d, vinv_resid, strTmpTrace) ' vinv_d, vinv_resid - are results
            If strTmpTrace <> String.Empty Then AppGlobals.BSlogg.Log($"strTmpTrace= {strTmpTrace}")
            bmat = Matrix.M_ADD(bmat, Matrix.MatrixMult(Matrix.trans(dmat), vinv_d))
            Dim dvinv_resid = Matrix.MatrixMult(Matrix.trans(dmat), vinv_resid)
            cmat = Matrix.M_ADD(cmat, Matrix.M_OUTERPRODUCT(Matrix.GetColumnFrom2Darray(dvinv_resid, 0), Matrix.GetColumnFrom2Darray(dvinv_resid, 0)))
        Next

        If pScale = 0 Then pScale = EstimateScale()
        AppGlobals.BSlogg.Log($"bmatfull={Matrix.array2str(bmat)}")
        ReDim pCovNaive(p - 1, p - 1), pCovRobust(p - 1, p - 1)
        'compute matrix inversion

        Dim bmatInv(,) As Double = Matrix.MatInv(bmat, "CHOL",, bPseudInverse:=True)
        'Dim tmp(,) As Double = Cholesky(bmat, iErr, False)
        ''Debug.Print(array2str(tmp))
        'If iErr = 2 Then 'MatrixType not positive-definite. Compute pseudoinverse
        '    BSlogg.Log($"WARNING: CHOLESKY. bmat not positive-definite. Calling pseudoInverse. bmat={array2str(bmat)}", LogMsgType.Warn)
        '    bmatInv = pseudoInverse(bmat)
        '    BSlogg.Log($"NOTE: pseudoInverse output ={array2str(bmatInv)}")
        'Else
        '    bmatInv = CholInv(tmp)
        'End If
        'Debug.Print(array2str(bmatInv))
        Me.pCovRobust = Matrix.MatrixMult(bmatInv, Matrix.MatrixMult(cmat, bmatInv))

        For i = 0 To p - 1
            For j = 0 To p - 1
                pCovNaive(i, j) = bmatInv(i, j) * pScale * pScalingFactor
                pCovRobust(i, j) = pCovRobust(i, j) * pScalingFactor
            Next
        Next

    End Sub

    ''' <summary>
    ''' Estimates the scale/dispersion parameter <c>φ</c> used in covariance scaling and scaled residuals.
    ''' </summary>
    ''' <param name="bForce">
    ''' If <c>True</c>, forces recomputation even when the family implies <c>φ = 1</c> under the current configuration.
    ''' </param>
    ''' <returns>The estimated scale parameter <c>φ</c>.</returns>
    ''' <remarks>
    ''' <para>
    ''' For many canonical count/binary families, the code returns <c>1.0</c> when <c>pScaleType = 0</c>:
    ''' Binomial, Poisson, and Negative Binomial.
    ''' </para>
    ''' <para>
    ''' Otherwise, this implementation computes Pearson-residual scale:
    ''' </para>
    ''' <para>
    ''' Let <c>rᵢⱼ = (yᵢⱼ − μᵢⱼ) / sqrt(Var(μᵢⱼ))</c>.
    ''' Then <c>φ</c> is proportional to the average of <c>rᵢⱼ²</c> over all observations.
    ''' </para>
    ''' <para>
    ''' In code:
    ''' </para>
    ''' <para>
    ''' <c>φ = (Σ r²) / denom</c>,
    ''' where <c>denom = fSum</c> (total observation count) by default, and if <c>UseP</c> is True:
    ''' <c>denom = fSum * (n − p)/n</c>.
    ''' </para>
    ''' <para>
    ''' This matches your current logic and may differ from other common conventions (e.g., dividing by <c>n − p</c>).
    ''' </para>
    ''' </remarks>
    Function EstimateScale(Optional bForce As Boolean = False) As Double
        'The scale parameter is estimated as the sum of squared Pearson residuals divided by

        AppGlobals.BSlogg.Log("proc started: gee.estimateScale")
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

                ResId = Matrix.M_DIV(Matrix.M_SUB(endog, expval), sdev)

                For j = 0 To UBound(ResId)
                    estScale += ResId(j) * ResId(j)
                Next
                fSum += ResId.Length()
            Next

            estScale /= If(Me.pbUseP, (fSum * (n - p) / n), fSum)
            Return estScale
        End If

    End Function

    ''' <summary>
    ''' Computes the GEE score vector and Newton/Fisher-scoring update step for the mean parameters <c>β</c>.
    ''' </summary>
    ''' <param name="update">
    ''' Output: the parameter step vector <c>Δβ = B⁻¹U</c> used by <see cref="Fit"/>.
    ''' </param>
    ''' <param name="score">
    ''' Output: the score vector <c>U(β) = Σ Dᵀ V⁻¹ (y − μ)</c>.
    ''' </param>
    ''' <remarks>
    ''' For each cluster <c>i</c> this method obtains from the covariance structure solver:
    ''' <c>Vᵢ⁻¹ Dᵢ</c> and <c>Vᵢ⁻¹ (yᵢ − μᵢ)</c>, then accumulates
    ''' <c>B += Dᵢᵀ (Vᵢ⁻¹ Dᵢ)</c> and <c>U += Dᵢᵀ (Vᵢ⁻¹ (yᵢ − μᵢ))</c>.
    ''' Finally, it returns <c>Δβ = B⁻¹ U</c>.
    ''' </remarks>
    Private Sub updateMeanParams(ByRef update() As Double, ByRef score() As Double)
        'update and score is the output

        Dim strTmpTrace As String, bmat(p - 1, p - 1) As Double, score_(p - 1, 0) As Double
        AppGlobals.BSlogg.Log("proc started: gee.updateMeanParams")
        For i = 0 To p - 1 : score_(i, 0) = 0 : Next

        For i = 0 To pNoGroup - 1
            Dim expval = pCachedMeans(i).Item1
            Dim lin_pred = pCachedMeans(i).Item2
            Dim endog = pEndogLi(i)
            Dim exog = pExogLi(i)

            Dim resid(UBound(expval)) As Double, sdev(UBound(expval)) As Double
            resid = Matrix.M_SUB(endog, expval)
            Dim dmat(,) As Double = MeanDeriv(exog, lin_pred, i)
            For j = 0 To UBound(expval)
                sdev(j) = SafeStDevFromMu(expval(j))
            Next

            Dim wresid = resid
            Dim wdmat = dmat
            Dim vinv_d(,) As Double = Nothing, vinv_resid() As Double = Nothing
            strTmpTrace = String.Empty
            pCovStruct.covarianceMatrixSolve(expval, i, Me, sdev, wdmat, wresid, vinv_d, vinv_resid, strTmpTrace) ' vinv_d, vinv_resid - are results
            If strTmpTrace <> String.Empty Then AppGlobals.BSlogg.Log($"strTmpTrace= {strTmpTrace}")

            bmat = Matrix.M_ADD(bmat, Matrix.MatrixMult(Matrix.trans(dmat), vinv_d))
            score_ = Matrix.M_ADD(score_, Matrix.MatrixMult(Matrix.trans(dmat), vinv_resid))
        Next

        score = Matrix.GetColumnFrom2Darray(score_, 0)
        AppGlobals.BSlogg.Log($"bmatfull= {Matrix.array2str(bmat)} scorefull={Matrix.array2str(score)}")


        Dim tmp = Matrix.MatInv(bmat, "CHOL",, bPseudInverse:=True)
        update = Matrix.GetColumnFrom2Darray(Matrix.MatrixMult(tmp, score_), 0)

    End Sub

    ''' <summary>
    ''' Computes <c>D = ∂μ/∂β</c> for one cluster, i.e. the derivative of the mean vector with respect to <c>β</c>.
    ''' </summary>
    ''' <param name="exog">Cluster design matrix <c>Xᵢ</c> including intercept (shape mᵢ × p).</param>
    ''' <param name="lin_pred">
    ''' Cluster linear predictor array <c>ηᵢ</c> (shape mᵢ × 1). This may be modified to include the offset depending on flags.
    ''' </param>
    ''' <param name="idx">Cluster index used to retrieve the matching offset vector.</param>
    ''' <param name="bUseOffset">
    ''' If <c>True</c> and an offset was supplied, adds the offset to <paramref name="lin_pred"/> before evaluating <c>dμ/dη</c>.
    ''' </param>
    ''' <returns>
    ''' MatrixType <c>Dᵢ</c> where row <c>j</c> is <c>xᵢⱼ * (dμ/dη)(ηᵢⱼ)</c>.
    ''' </returns>
    ''' <remarks>
    ''' Using the inverse link derivative:
    ''' <c>dμ/dη = (g⁻¹)'(η)</c>.
    ''' Then
    ''' <c>Dᵢ = diag(dμ/dη) Xᵢ</c>.
    ''' </remarks>
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

    ''' <summary>
    ''' Updates the cached per-cluster mean vectors <c>μᵢ</c> and linear predictors <c>ηᵢ</c> for a given parameter vector <c>β</c>.
    ''' </summary>
    ''' <param name="mean_params">Current regression parameter vector (including intercept).</param>
    ''' <remarks>
    ''' For each cluster <c>i</c>:
    ''' <c>ηᵢ = Xᵢ β (+ offsetᵢ)</c>, then <c>μᵢ = g⁻¹(ηᵢ)</c>.
    ''' These cached arrays are used by score, covariance, scale, QIC, and residual computations.
    ''' </remarks>
    Private Sub UpdateCachedMeans(mean_params() As Double)
        'pCachedMeans should always contain the most recent calculation of the group-wise mean vectors. This sub should be
        'called every time the regression parameters are changed, to keep the cached means up to date.
        AppGlobals.BSlogg.Log("proc started: gee.updateCachedMeans")

        Dim bFirstCall As Boolean = If(pCachedMeans.Count = 0, True, False)

        For i = 0 To pNoGroup - 1
            'Debug.Print(array2str(pExogLi(i)))
            Dim tmpExog(,) As Double = pExogLi(i)
            Dim lin_pred(,) As Double = Matrix.MatrixMult(tmpExog, mean_params)

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

    ''' <summary>
    ''' Estimates starting values for <c>β</c> by fitting a corresponding independence GLM.
    ''' </summary>
    ''' <returns>Initial coefficient vector suitable for starting the GEE iteration.</returns>
    ''' <remarks>
    ''' This method fits a GLM using the same family/link (and offset, if present),
    ''' stores the resulting independence (naive) covariance for later QIC calculations,
    ''' and returns the GLM coefficient estimates as the initial <c>β</c>.
    ''' </remarks>
    Private Function GetStartParams() As Double()
        'estimate starting parameters using the GLM fit

        AppGlobals.BSlogg.Log("proc started: gee.getStartParams")

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
            .Fit(1)
            pIndependenceNaiveVarCovar = .VarCovar
        End With
        AppGlobals.BSlogg.Log($"start params: {Matrix.array2str(glm.results.Coeffs_est)}")

        Return glm.results.Coeffs_est
    End Function

    ''' <summary>
    ''' Computes quasi-likelihood and Pan’s QIC / QICu information criteria for the fitted model.
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' The class accumulates a quasi-likelihood-like quantity:
    ''' <c>QL = Σᵢ Σⱼ Q(yᵢⱼ, μᵢⱼ)</c> using <c>pFamily.geeQuasiLike</c>.
    ''' </para>
    ''' <para>
    ''' QICu (mean-structure comparison): <c>QICu = −2 QL + 2 p</c>.
    ''' </para>
    ''' <para>
    ''' QIC (mean + covariance comparison): <c>QIC = −2 QL + 2 trace(Ω_I⁻¹ V̂)</c>,
    ''' where <c>Ω_I</c> is the independence-model covariance (from the GLM start fit) and <c>V̂</c> is the robust covariance.
    ''' </para>
    ''' <para>
    ''' Your code computes <c>trace(Ω_I⁻¹ V̂)</c> by multiplying <c>inv(Ω_I)</c> with <c>V̂</c> and summing diagonal elements.
    ''' </para>
    ''' </remarks>
    Private Sub EstimateQIC()
        'Returns quasi-information criteria and quasi-likelihood values.
        'W. Pan (2001).  Akaike's information criterion in generalized estimating equations.  Biometrics (57) 1.
        Dim Trace As Double
        AppGlobals.BSlogg.Log("proc started: gee.estimateQIC")

        For i = 0 To pNoGroup - 1
            Dim expval = pCachedMeans(i).Item1
            For j = 0 To UBound(expval)
                pQL += pFamily.geeQuasiLike(CDbl(pEndogLi(i)(j)), expval(j))
            Next
        Next

        Dim NaiveInv(,) As Double = Matrix.MatInv(pIndependenceNaiveVarCovar)
        Dim tmp = Matrix.MatrixMult(NaiveInv, pCovRobust)

        For i = 0 To UBound(tmp)
            Trace += tmp(i, i)
        Next
        pQICu = -2.0 * pQL + 2.0 * Me.p
        pQIC = -2.0 * pQL + 2.0 * Trace
    End Sub

    ''' <summary>
    ''' Converts row-wise inputs into per-cluster arrays and builds time/correlation indexing helpers.
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' This method:
    ''' </para>
    ''' <list type="bullet">
    ''' <item><description>Groups observations by cluster id (<c>pRepeats</c>).</description></item>
    ''' <item><description>Builds <c>pEndogLi</c> (yᵢ), <c>pExogLi</c> (Xᵢ with intercept), <c>pOffsetLi</c>, and <c>pTimeLi</c>.</description></item>
    ''' <item><description>Builds <c>pGroupIndices</c> to map cluster-local positions back to original row indices.</description></item>
    ''' <item><description>Constructs and sorts unique times, then assigns contiguous indices in <see cref="UniqueTimesDict"/>.</description></item>
    ''' </list>
    ''' <para>
    ''' The time dictionary is required by covariance structures that depend on a shared set of time points
    ''' (e.g., unstructured correlation, AR(1) with a common index mapping).
    ''' </para>
    ''' </remarks>
    Private Sub PreProcessData()

        AppGlobals.BSlogg.Log("proc started: Extracted Information:")
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

        AppGlobals.BSlogg.Log($"pGroupLabels={Matrix.array2str(pGroupLabels)}")
        AppGlobals.BSlogg.Log($"# of unique times: {uniqueTimesColl.Count}; UniqueTimes={Matrix.array2str(UniqueTimes)}")
    End Sub


    ''' <summary>
    ''' Computes common GLM-style residuals for a fitted GEE mean model (marginal residuals).
    ''' </summary>
    ''' <param name="tol">
    ''' Small positive number used to guard divisions by near-zero values (variance, derivatives, scale).
    ''' </param>
    ''' <param name="useWeights">
    ''' If True and weights exist, residuals are multiplied by sqrt(weight) where appropriate.
    ''' <b>Note:</b> this does not change the parameter fit itself; it affects only residual outputs.
    ''' </param>
    ''' <param name="scaleResiduals">
    ''' If True, also returns scaled variants (dividing Pearson/Deviance by sqrt(φ)).
    ''' </param>
    ''' <remarks>
    ''' <para>
    ''' Residual definitions for observation i:
    ''' </para>
    ''' <list type="bullet">
    ''' <item><description><b>Raw</b>: <c>r = y − μ</c></description></item>
    ''' <item><description><b>Pearson</b>: <c>r / sqrt(Var(μ))</c></description></item>
    ''' <item><description><b>Deviance</b>: <c>sign(r) * sqrt(D)</c>, where <c>D</c> is the family deviance contribution</description></item>
    ''' <item><description><b>Working</b>: <c>r / (dμ/dη)</c></description></item>
    ''' </list>
    ''' <para>
    ''' Scaled variants divide Pearson/Deviance residuals by <c>sqrt(φ)</c> where <c>φ</c> is obtained from <c>GetResidualScale()</c>.
    ''' </para>
    ''' </remarks>
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
    ''' Returns the scale parameter <c>φ</c> to use for scaled residuals, matching <see cref="EstimateScale"/> conventions.
    ''' </summary>
    ''' <returns>
    ''' <c>1</c> for certain families when <c>pScaleType = 0</c>; otherwise <c>pScale</c> if already computed,
    ''' else recomputes via <see cref="EstimateScale"/>.
    ''' </returns>
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
