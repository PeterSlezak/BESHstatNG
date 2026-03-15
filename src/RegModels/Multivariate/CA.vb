Option Explicit On
Imports Microsoft.Office.Interop.Excel

Namespace Multivariate

    ''' <summary>
    ''' Performs Simple Correspondence Analysis (CA) on a two-way contingency table and
    ''' Multiple Correspondence Analysis (MCA) on multivariate categorical data.
    ''' </summary>
    ''' <remarks>
    ''' <para><b>What this class does</b></para>
    ''' <para>
    ''' After loading data via <see cref="data"/> (CA) or <see cref="DataMultiple"/> (MCA),
    ''' call <see cref="Calculate"/> to compute factor scores (principal coordinates), inertias (eigenvalues),
    ''' and standard CA/MCA diagnostics (chi-square distances, cos², contributions, angles, quality).
    ''' </para>
    '''
    ''' <para><b>Mathematics — Simple CA</b></para>
    ''' <para>
    ''' Let <c>N</c> be an <c>R×C</c> contingency table of counts and <c>n = Σᵢⱼ Nᵢⱼ</c>.
    ''' Define proportions <c>P = N / n</c>, row masses <c>rᵢ = Σⱼ Pᵢⱼ</c>, and column masses
    ''' <c>cⱼ = Σᵢ Pᵢⱼ</c>. The independence model is <c>r cᵀ</c>. This implementation uses the
    ''' centered, standardized residual matrix:
    ''' </para>
    ''' <code>
    ''' S = D_r^{-1/2} (P − r cᵀ) D_c^{-1/2}
    ''' </code>
    ''' <para>
    ''' with <c>D_r = diag(r)</c>, <c>D_c = diag(c)</c>. An SVD <c>S = U diag(σ) Vᵀ</c> yields
    ''' singular values <c>σₖ</c> and axes. Principal inertias (eigenvalues) are <c>λₖ = σₖ²</c>.
    ''' Principal coordinates are:
    ''' </para>
    ''' <code>
    ''' F = D_r^{-1/2} U diag(σ)   (rows)
    ''' G = D_c^{-1/2} V diag(σ)   (columns)
    ''' </code>
    '''
    ''' <para><b>Mathematics — MCA in this implementation</b></para>
    ''' <para>
    ''' MCA starts from raw categorical observations (N individuals, Q variables). Internally, the class constructs:
    ''' </para>
    ''' <list type="bullet">
    '''   <item><description><b>Indicator/design matrix</b> <c>Z</c> (size <c>N×K</c>) where K is the total number of categories across all variables; each individual has one active category per variable.</description></item>
    '''   <item><description><b>Burt table</b> <c>B = Zᵀ Z</c> (size <c>K×K</c>) for compact presentation of all pairwise cross-tabs.</description></item>
    ''' </list>
    ''' <para>
    ''' The default computation path is CA on the indicator matrix <c>Z</c>.
    ''' (Many R implementations compute from the Burt table for efficiency and then report “indicator” inertias; see below.)
    ''' </para>
    '''
    ''' <para><b>Axis sign</b></para>
    ''' <para>
    ''' Each CA/MCA axis is defined up to a sign: if (<c>F</c>, <c>G</c>) is a solution, then
    ''' (−<c>F</c>, −<c>G</c>) is also a solution with identical inertias, distances, cos² and contributions.
    ''' Therefore factor signs may differ from other software or from earlier runs.
    ''' </para>
    '''
    ''' <para><b>Comparison with R (practical expectations)</b></para>
    ''' <list type="bullet">
    '''   <item><description><c>MASS::corresp</c> performs CA and explicitly notes that axis signs may vary by platform.</description></item>
    '''   <item><description><c>ca::mjca</c> computes MCA from an eigen-decomposition of the Burt matrix; it offers <c>lambda="indicator"</c> (indicator inertias), <c>"Burt"</c>, <c>"adjusted"</c>, <c>"JCA"</c> variants.</description></item>
    '''   <item><description><c>FactoMineR::MCA</c> supports indicator- and Burt-based strategies and often treats missing values as an additional category level.</description></item>
    ''' </list>
    ''' <para>
    ''' This class’ CA formulation (centered residual matrix + SVD) matches standard textbook CA.
    ''' For MCA, it currently follows the indicator-matrix route (conceptually closest to <c>mjca(..., lambda="indicator")</c>),
    ''' while still building a Burt table for reporting.
    ''' </para>
    '''
    ''' <para><b>Implementation notes</b></para>
    ''' <list type="bullet">
    '''   <item><description><b>Axis indexing:</b> factor arrays are zero-based (Factor 1 = index 0).</description></item>
    '''   <item><description><b>Sorting:</b> axes are sorted by descending singular value (thus descending eigenvalue/inertia).</description></item>
    '''   <item><description><b>Excel plotting:</b> <see cref="plot"/> and <see cref="contribPlot"/> use Excel Interop.</description></item>
    '''   <item><description><b>Helper dependencies:</b> this class expects helper functions such as <c>SVD_decomp</c>, <c>MatrixMult</c>, <c>trans</c>, <c>Sum2D</c>, <c>GetColumnFrom2Darray</c>, and logging utilities to exist elsewhere in your project.</description></item>
    ''' </list>
    ''' </remarks>
    Public Class CA
        'Simple and Multiple Correspondence analysis class

        Private pData(,) As Integer 'Input contingency table. When Multiple CA then it is the Design MatrixType
        Private pDataMultiple(,) As String 'Input Raw data for Multiple Correspondence Analysis. We need to create Burt table out of it first
        Private pVarNames() As String 'variable names. Used only for Multiple CA
        Private pVarNamesToPresent() As String
        Private pRowNames() As String
        Private pColNames() As String
        Private pR As Integer 'Number of rows
        Private pC As Integer 'number of columns
        Private pDim As Integer 'number of eigenvalues dimenstions
        Private pEigenvalues() As Double
        Private pRowTot() As Double
        Private pColTot() As Double
        Private pRowFactors(,) As Double
        Private pColFactors(,) As Double
        Private pRowDistance() As Double
        Private pColDistance() As Double
        Private pRowInertia() As Double
        Private pColInertia() As Double
        Private pRowCorr(,) As Double
        Private pColCorr(,) As Double
        Private pRowContribution(,) As Double
        Private pColContribution(,) As Double
        Private pColContributionSigned(,) As Double
        Private pRowAngle(,) As Double
        Private pColAngle(,) As Double
        Private pRowEigencontrib(,) As Double
        Private pColEigencontrib(,) As Double
        Private pRowQuality() As Double
        Private pColQuality() As Double
        Private pbMultiple As Boolean
        Private pCrossTab As New List(Of Dictionary(Of String, Integer)) 'Used for MCA - freqID outputs of all input variables
        Private pDesignMatrix(,) As Integer
        'Global category index for each individual and variable: [ind, var] -> globalCatIdx
        Private pIndCatIdx(,) As Integer

        Private pBurtTable(,) As Integer
        Private pCatTots() As Integer
        Private pLevels As New List(Of String()) ' ordered category levels per variable

        ''' <summary>
        ''' Loads a contingency table for Simple Correspondence Analysis (CA).
        ''' </summary>
        ''' <param name="x">
        ''' Contingency table of non-negative counts (<c>R×C</c>). Each element represents the count for a
        ''' row category i and column category j.
        ''' </param>
        ''' <param name="rows">
        ''' Optional row labels. If <c>Nothing</c>, default labels (<c>"Row 1"</c>, <c>"Row 2"</c>, ...) are generated.
        ''' </param>
        ''' <param name="cols">
        ''' Optional column labels. If <c>Nothing</c>, default labels (<c>"Col 1"</c>, <c>"Col 2"</c>, ...) are generated.
        ''' </param>
        ''' <remarks>
        ''' <para>
        ''' This method sets the internal count matrix (<c>pData</c>) and initializes dimensions (<c>pR</c>, <c>pC</c>, <c>pDim</c>).
        ''' It does not run CA; call <see cref="Calculate"/> afterwards.
        ''' </para>
        ''' <para>
        ''' <b>Validation:</b> this method assumes counts are non-negative and that the table total is &gt; 0.
        ''' </para>
        ''' </remarks>
        Public Sub data(x(,) As Integer, Optional rows() As String = Nothing, Optional cols() As String = Nothing)

            pData = x
            pbMultiple = False
            pR = UBound(pData, 1) + 1
            pC = UBound(pData, 2) + 1
            pDim = Math.Min(pR, pC) - 2

            If rows Is Nothing Then
                ReDim pRowNames(pR - 1)
                For i As Integer = 1 To pR : pRowNames(i - 1) = "Row " & CStr(i) : Next i
            Else
                pRowNames = rows
                If pRowNames.Length <> pR Then
                    BESHstatGlobals.BSerr.LogAndThrow(New ArgumentException("Number of Contingency table rows and Row labels don't match!"))
                End If
            End If

            If cols Is Nothing Then
                ReDim pColNames(pC - 1)
                For i As Integer = 1 To pC : pColNames(i - 1) = "Col " & CStr(i) : Next i
            Else
                pColNames = cols
                If pColNames.Length <> pC Then
                    BESHstatGlobals.BSerr.LogAndThrow(New ArgumentException("Number of Contingency table columns And Column labels don't match!"))
                End If
            End If
        End Sub

        ''' <summary>
        ''' Loads multivariate categorical observations and prepares MCA inputs.
        ''' </summary>
        ''' <param name="x">
        ''' Categorical data matrix of size <c>N×Q</c> (N individuals/records, Q variables).
        ''' </param>
        ''' <param name="strVarnames">
        ''' Names of the Q variables. Used for Burt-table block headers and for presentation.
        ''' </param>
        ''' <remarks>
        ''' <para>
        ''' This method performs the preprocessing required for MCA:
        ''' </para>
        ''' <list type="number">
        '''   <item><description><see cref="CreateCrossTab"/>: determine unique category levels and their order per variable.</description></item>
        '''   <item><description><see cref="CreateBurtTable"/>: build the Burt table <c>B = ZᵀZ</c> for reporting/diagnostics.</description></item>
        '''   <item><description><see cref="CreateDesignMatrix"/>: build the indicator matrix <c>Z</c> used by the default computation path.</description></item>
        ''' </list>
        ''' <para>
        ''' After this method, call <see cref="Calculate"/> to compute MCA results (CA on <c>Z</c>).
        ''' </para>
        ''' <para>
        ''' <b>Missing values:</b> values are trimmed; <c>Nothing</c> becomes <c>""</c>. If you want explicit missing categories,
        ''' recode missing values to a sentinel category (e.g., "(Missing)") before calling this method.
        ''' </para>
        ''' </remarks>
        Public Sub DataMultiple(x(,) As String, strVarnames() As String)
            pbMultiple = True
            pDataMultiple = x
            pVarNames = strVarnames
            CreateCrossTab()
            CreateBurtTable()
            CreateDesignMatrix()
            pData = pDesignMatrix
            pR = UBound(pDesignMatrix, 1) + 1
            pC = UBound(pDesignMatrix, 2) + 1
        End Sub

        'Get Values
        ''' <summary>Gets the Burt table <c>B</c> (MCA only).</summary>
        ''' <remarks>
        ''' <para>Size: <c>K×K</c> where K is the total number of category levels across all variables.</para>
        ''' <para>Diagonal blocks contain per-variable category frequencies; off-diagonals are pairwise cross-tabs.</para>
        ''' </remarks>
        ReadOnly Property BurtTable() As Integer(,)
            Get
                Return pBurtTable
            End Get
        End Property

        ''' <summary>Gets the indicator/design matrix <c>Z</c> used for MCA computation.</summary>
        ''' <remarks>
        ''' <para>Size: <c>N×K</c> (N individuals, K categories). Each row has one active category per variable.</para>
        ''' <para>Built by <see cref="CreateDesignMatrix"/> and stored as <c>Integer(,)</c> with entries 0/1.</para>
        ''' </remarks>
        ReadOnly Property DesignMatrix() As Integer(,)
            Get
                Return pDesignMatrix
            End Get
        End Property

        ''' <summary>Gets variable names aligned with the stacked category list for Burt-table presentation.</summary>
        ''' <remarks>
        ''' Each category row/column can be associated back to its originating variable using this expanded name array.
        ''' </remarks>
        ReadOnly Property BurtVarNames() As String()
            Get
                Return pVarNamesToPresent
            End Get
        End Property

        ''' <summary>Gets labels for columns (CA columns or MCA categories).</summary>
        ReadOnly Property ColumNames() As String()
            Get
                Return pColNames
            End Get
        End Property

        ''' <summary>Gets labels for rows (CA rows or MCA individuals).</summary>
        ReadOnly Property rowNames() As String()
            Get
                Return pRowNames
            End Get
        End Property

        ''' <summary>Gets principal inertias (eigenvalues) for each axis, sorted descending.</summary>
        ''' <remarks>
        ''' <para>Computed as <c>λₖ = σₖ²</c>, where <c>σₖ</c> are singular values of the standardized residual matrix.</para>
        ''' <para>Total inertia is <c>Σₖ λₖ</c>, equal to the chi-square statistic divided by the grand total.</para>
        ''' </remarks>
        ReadOnly Property Eigenvalues() As Double()
            Get
                Return pEigenvalues
            End Get
        End Property

        ''' <summary>Gets percent and cumulative percent inertia per axis.</summary>
        ''' <remarks>
        ''' <para>Column 0: <c>100·λₖ/Σλ</c>. Column 1: cumulative sum of Column 0.</para>
        ''' </remarks>
        ReadOnly Property Percents() As Double(,)
            Get
                Dim tmp(pDim, 1) As Double
                Dim tot As Double = pEigenvalues.Sum()
                For i As Integer = 0 To pDim
                    tmp(i, 0) = 100.0 * (pEigenvalues(i) / tot)

                    If i = 0 Then
                        tmp(i, 1) = tmp(i, 0)
                    Else
                        tmp(i, 1) = tmp(i - 1, 1) + tmp(i, 0)
                    End If
                Next
                Return tmp
            End Get
        End Property

        ''' <summary>Gets row masses <c>r</c> (row marginal proportions).</summary>
        ''' <remarks>For CA, <c>rᵢ = Σⱼ Pᵢⱼ</c>.</remarks>
        ReadOnly Property RowMass() As Double()
            Get
                Return pRowTot
            End Get
        End Property

        ''' <summary>Gets column masses <c>c</c> (column marginal proportions).</summary>
        ''' <remarks>For CA, <c>cⱼ = Σᵢ Pᵢⱼ</c>.</remarks>
        ReadOnly Property ColMass() As Double()
            Get
                Return pColTot
            End Get
        End Property

        ''' <summary>Gets principal row coordinates (factor scores) for an axis.</summary>
        ''' <param name="id">Axis index (0-based). If <c>-1</c>, returns Factor 1 (axis 0).</param>
        ''' <returns>Vector of coordinates aligned to <see cref="rowNames"/>.</returns>
        ''' <remarks>
        ''' Principal coordinates are computed as <c>F = D_r^{-1/2} U diag(σ)</c>. Axis signs are arbitrary.
        ''' </remarks>
        ReadOnly Property RowFactors(Optional id As Integer = -1) As Double()
            Get
                If id = -1 Then
                    Return Matrix.GetColumnFrom2Darray(pRowFactors, 0)
                Else
                    Return Matrix.GetColumnFrom2Darray(pRowFactors, id)
                End If
            End Get
        End Property

        ''' <summary>Gets principal column coordinates (factor scores) for an axis.</summary>
        ''' <param name="id">Axis index (0-based). If <c>-1</c>, returns Factor 1 (axis 0).</param>
        ''' <returns>Vector of coordinates aligned to <see cref="ColumNames"/>.</returns>
        ''' <remarks>
        ''' Principal coordinates are computed as <c>G = D_c^{-1/2} V diag(σ)</c>. Axis signs are arbitrary.
        ''' </remarks>
        ReadOnly Property ColFactors(Optional id As Integer = -1) As Double()
            Get
                If id = -1 Then
                    Return Matrix.GetColumnFrom2Darray(pColFactors, 0)
                Else
                    Return Matrix.GetColumnFrom2Darray(pColFactors, id)
                End If
            End Get
        End Property

        ''' <summary>Gets chi-square distances of rows to the centroid.</summary>
        ''' <remarks>
        ''' Distance is computed from row profiles. Rows with larger distances are further from independence.
        ''' </remarks>
        ReadOnly Property RowDistance() As Double()
            Get
                Return pRowDistance
            End Get
        End Property

        ''' <summary>Gets chi-square distances of columns to the centroid.</summary>
        ReadOnly Property ColDistance() As Double()
            Get
                Return pColDistance
            End Get
        End Property

        ''' <summary>Gets row inertias (mass × distance).</summary>
        ''' <remarks>Row inertia: <c>Iᵢ = rᵢ · dᵢ²</c>.</remarks>
        ReadOnly Property RowInertia() As Double()
            Get
                Return pRowInertia
            End Get
        End Property

        ''' <summary>Gets column inertias (mass × distance).</summary>
        ''' <remarks>Column inertia: <c>Iⱼ = cⱼ · dⱼ²</c>.</remarks>
        ReadOnly Property ColInertia() As Double()
            Get
                Return pColInertia
            End Get
        End Property

        ''' <summary>Gets row quality of representation (sum of cos² over computed axes).</summary>
        ReadOnly Property RowQuality() As Double()
            Get
                Return pRowQuality
            End Get
        End Property

        ''' <summary>Gets column quality of representation (sum of cos² over computed axes).</summary>
        ReadOnly Property ColQuality() As Double()
            Get
                Return pColQuality
            End Get
        End Property

        ''' <summary>Gets row cos² (squared correlations) with an axis.</summary>
        ''' <param name="id">Axis index (0-based). If <c>-1</c>, returns cos² for Factor 1.</param>
        ''' <remarks>
        ''' <para><c>cos²ᵢₖ = fᵢₖ² / dᵢ²</c>.</para>
        ''' </remarks>
        ReadOnly Property RowCorr(Optional id As Integer = -1) As Double()
            Get
                If id = -1 Then
                    Return Matrix.GetColumnFrom2Darray(pRowCorr, 0)
                Else
                    Return Matrix.GetColumnFrom2Darray(pRowCorr, id)
                End If
            End Get
        End Property

        ''' <summary>Gets column cos² (squared correlations) with an axis.</summary>
        ''' <param name="id">Axis index (0-based). If <c>-1</c>, returns cos² for Factor 1.</param>
        ReadOnly Property ColCorr(Optional id As Integer = -1) As Double()
            Get
                If id = -1 Then
                    Return Matrix.GetColumnFrom2Darray(pColCorr, 0)
                Else
                    Return Matrix.GetColumnFrom2Darray(pColCorr, id)
                End If
            End Get
        End Property

        ''' <summary>Gets row contributions to an axis.</summary>
        ''' <param name="id">Axis index (0-based). If <c>-1</c>, returns contributions for Factor 1.</param>
        ''' <remarks>
        ''' <para><c>ctrᵢₖ = rᵢ fᵢₖ² / λₖ</c>.</para>
        ''' </remarks>
        ReadOnly Property RowContribution(Optional id As Integer = -1) As Double()
            Get
                If id = -1 Then
                    Return Matrix.GetColumnFrom2Darray(pRowContribution, 0)
                Else
                    Return Matrix.GetColumnFrom2Darray(pRowContribution, id)
                End If
            End Get
        End Property

        ''' <summary>Gets signed column contributions to an axis.</summary>
        ''' <param name="id">Axis index (0-based). If <c>-1</c>, returns signed contributions for Factor 1.</param>
        ''' <remarks>
        ''' Magnitude equals the standard contribution; sign equals the sign of the column coordinate on that axis.
        ''' </remarks>
        ReadOnly Property ColContributionSigned(Optional id As Integer = -1) As Double()
            Get
                If id = -1 Then
                    Return Matrix.GetColumnFrom2Darray(pColContributionSigned, 0)
                Else
                    Return Matrix.GetColumnFrom2Darray(pColContributionSigned, id)
                End If
            End Get
        End Property

        ''' <summary>Gets column contributions to an axis.</summary>
        ''' <param name="id">Axis index (0-based). If <c>-1</c>, returns contributions for Factor 1.</param>
        ''' <remarks><para><c>ctrⱼₖ = cⱼ gⱼₖ² / λₖ</c>.</para></remarks>
        ReadOnly Property ColContribution(Optional id As Integer = -1) As Double()
            Get
                If id = -1 Then
                    Return Matrix.GetColumnFrom2Darray(pColContribution, 0)
                Else
                    Return Matrix.GetColumnFrom2Darray(pColContribution, id)
                End If
            End Get
        End Property

        ''' <summary>Gets row angles (degrees) to an axis.</summary>
        ''' <param name="id">Axis index (0-based). If <c>-1</c>, returns angles for Factor 1.</param>
        ''' <remarks>Angle is <c>acos(sqrt(cos²))</c> expressed in degrees.</remarks>
        ReadOnly Property RowAngle(Optional id As Integer = -1) As Double()
            Get
                If id = -1 Then
                    Return Matrix.GetColumnFrom2Darray(pRowAngle, 0)
                Else
                    Return Matrix.GetColumnFrom2Darray(pRowAngle, id)
                End If
            End Get
        End Property

        ''' <summary>Gets column angles (degrees) to an axis.</summary>
        ''' <param name="id">Axis index (0-based). If <c>-1</c>, returns angles for Factor 1.</param>
        ReadOnly Property ColAngle(Optional id As Integer = -1) As Double()
            Get
                If id = -1 Then
                    Return Matrix.GetColumnFrom2Darray(pColAngle, 0)
                Else
                    Return Matrix.GetColumnFrom2Darray(pColAngle, id)
                End If
            End Get
        End Property

        ''' <summary>Gets row eigenvalue-scaled contributions (ctr × λ) for an axis.</summary>
        ''' <param name="id">Axis index (0-based). If <c>-1</c>, returns values for Factor 1.</param>
        ReadOnly Property RowEigenvalueContrib(Optional id As Integer = -1) As Double()
            Get
                If id = -1 Then
                    Return Matrix.GetColumnFrom2Darray(pRowEigencontrib, 0)
                Else
                    Return Matrix.GetColumnFrom2Darray(pRowEigencontrib, id)
                End If
            End Get
        End Property

        ''' <summary>Gets column eigenvalue-scaled contributions (ctr × λ) for an axis.</summary>
        ''' <param name="id">Axis index (0-based). If <c>-1</c>, returns values for Factor 1.</param>
        ReadOnly Property ColEigenvalueContrib(Optional id As Integer = -1) As Double()
            Get
                If id = -1 Then
                    Return Matrix.GetColumnFrom2Darray(pColEigencontrib, 0)
                Else
                    Return Matrix.GetColumnFrom2Darray(pColEigencontrib, id)
                End If
            End Get
        End Property

        ''' <summary>
        ''' Produces presentation-ready result tables for the current analysis.
        ''' </summary>
        ''' <returns>
        ''' A list of <see cref="ResultTable"/> objects containing:
        ''' <list type="bullet">
        '''   <item><description>Eigenvalues / percent inertia / cumulative inertia</description></item>
        '''   <item><description>Principal coordinates (row and column/category factor scores)</description></item>
        '''   <item><description>Cos² (quality/correlation) and contributions by axis</description></item>
        '''   <item><description>Optional MCA tables such as the Burt table</description></item>
        ''' </list>
        ''' </returns>
        ''' <remarks>
        ''' <para>
        ''' Call <see cref="Calculate"/> before calling this method.
        ''' The returned <see cref="ResultTable"/> objects are designed to be stacked/merged and exported (e.g., to Excel).
        ''' </para>
        ''' <para>
        ''' <b>Axis indexing:</b> table headings are human-readable (“Factor 1”, “Factor 2”, …) while internal arrays are 0-based.
        ''' </para>
        ''' </remarks>
        Public Function wrapResults() As List(Of ResultTable)
            Dim out As New List(Of ResultTable)
            Dim t = New ResultTable
            Dim axesAvail As Integer = UBound(Me.Eigenvalues) + 1

            If Me.pbMultiple Then
                'Burt table
                t.AddTitle("Burt Table")
                t.AddHeaderTopRow(Matrix.ConcatArrays({"Variable", ""}, Me.BurtVarNames))
                t.AddHeaderTopRow(Matrix.ConcatArrays({"", "Category"}, Me.rowNames))
                t.AddHeaderLeftRow(Me.BurtVarNames)
                t.AddHeaderLeftRow(Me.rowNames)
                t.SetBody(Matrix.Array2objArray(Me.BurtTable))
                out.Add(t)
                Debug.Print(Matrix.array2str(t.returnSelf))

                'Eigenvalues
                t = New ResultTable
                t.AddHeaderTopRow({"Factor #", "Eigenvalue", "Percent", "Percent Cumulative"})
                Dim o(UBound(Me.Eigenvalues, 1), 3) As Object
                For i = 0 To UBound(Me.Eigenvalues, 1)
                    o(i, 0) = i + 1
                    o(i, 1) = Me.Eigenvalues(i)
                    o(i, 2) = Me.Percents(i, 0)
                    o(i, 3) = Me.Percents(i, 1)
                Next
                t.AddTitle("Eigenvalues")
                t.SetBody(o)
                out.Add(t)

                'Principal Coordinates for Columns/Rows
                t = New ResultTable
                t.AddTitle("Principal Coordinates for Columns/Rows")
                t.AddHeaderLeftRow(Me.BurtVarNames)
                t.AddHeaderLeftRow(Me.rowNames)
                t.AddHeaderTopRow({"Variable", "Category", "Quality", "Mass", "Distance", "Inertia"})
                Dim tmp(Me.pC - 1, 3) As Object
                For i = 0 To Me.pC - 1
                    tmp(i, 0) = Me.ColQuality(i)
                    tmp(i, 1) = Me.ColMass(i)
                    tmp(i, 2) = Me.ColDistance(i)
                    tmp(i, 3) = Me.ColInertia(i)
                Next
                t.SetBody(tmp)
                out.Add(t)

                'Axis 1 - Rows
                If axesAvail >= 1 Then
                    t = New ResultTable
                    t.AddTitle("Axis 1")
                    t.AddHeaderLeftRow(Me.BurtVarNames)
                    t.AddHeaderLeftRow(Me.rowNames)
                    t.AddHeaderTopRow({"Variable", "Category", "Factor", "Correlation", "Contribution", "Angle", "Eigenvalue"})
                    Dim tmp2(Me.pC - 1, 4) As Object
                    For i = 0 To Me.pC - 1
                        tmp2(i, 0) = Me.ColFactors(0)(i)
                        tmp2(i, 1) = Me.ColCorr(0)(i)
                        tmp2(i, 2) = Me.ColContribution(0)(i)
                        tmp2(i, 3) = Me.ColAngle(0)(i)
                        tmp2(i, 4) = Me.ColEigenvalueContrib(0)(i)
                    Next
                    t.SetBody(tmp2)
                    out.Add(t)
                End If

                'Axis 2 - Columns
                If axesAvail >= 2 Then
                    t = New ResultTable
                    t.AddTitle("Axis 2")
                    t.AddHeaderLeftRow(Me.BurtVarNames)
                    t.AddHeaderLeftRow(Me.rowNames)
                    t.AddHeaderTopRow({"Variable", "Category", "Factor", "Correlation", "Contribution", "Angle", "Eigenvalue"})
                    Dim tmp6(Me.pC - 1, 4) As Object
                    For i = 0 To Me.pC - 1
                        tmp6(i, 0) = Me.ColFactors(1)(i)
                        tmp6(i, 1) = Me.ColCorr(1)(i)
                        tmp6(i, 2) = Me.ColContribution(1)(i)
                        tmp6(i, 3) = Me.ColAngle(1)(i)
                        tmp6(i, 4) = Me.ColEigenvalueContrib(1)(i)
                    Next
                    t.SetBody(tmp6)
                    out.Add(t)
                End If

            Else

                'Eigenvalues
                t.AddHeaderTopRow({"Factor #", "Eigenvalue", "Percent", "Percent Cumulative"})
                Dim o(UBound(Me.Eigenvalues, 1), 3) As Object
                For i = 0 To UBound(Me.Eigenvalues, 1)
                    o(i, 0) = i + 1
                    o(i, 1) = Me.Eigenvalues(i)
                    o(i, 2) = Me.Percents(i, 0)
                    o(i, 3) = Me.Percents(i, 1)
                Next
                t.AddTitle("Eigenvalues")
                t.SetBody(o)
                out.Add(t)

                'Principal Coordinates for Rows
                t = New ResultTable
                t.AddTitle("Principal Coordinates for Rows")
                t.AddHeaderLeftRow(Me.rowNames)
                t.AddHeaderTopRow({"Row Name", "Quality", "Mass", "Distance", "Inertia"})
                Dim tmp(Me.pR - 1, 3) As Object
                For i = 0 To Me.pR - 1
                    tmp(i, 0) = Me.RowQuality(i)
                    tmp(i, 1) = Me.RowMass(i)
                    tmp(i, 2) = Me.RowDistance(i)
                    tmp(i, 3) = Me.RowInertia(i)
                Next
                t.SetBody(tmp)
                out.Add(t)

                'Axis 1 - Rows
                If axesAvail >= 1 Then
                    t = New ResultTable
                    t.AddTitle("Axis 1")
                    t.AddHeaderLeftRow(Me.rowNames)
                    t.AddHeaderTopRow({"Row Name", "Factor", "Correlation", "Contribution", "Angle", "Eigenvalue"})
                    Dim tmp2(Me.pR - 1, 4) As Object
                    For i = 0 To Me.pR - 1
                        tmp2(i, 0) = Me.RowFactors(0)(i)
                        tmp2(i, 1) = Me.RowCorr(0)(i)
                        tmp2(i, 2) = Me.RowContribution(0)(i)
                        tmp2(i, 3) = Me.RowAngle(0)(i)
                        tmp2(i, 4) = Me.RowEigenvalueContrib(0)(i)
                    Next
                    t.SetBody(tmp2)
                    out.Add(t)
                End If

                If axesAvail >= 2 Then
                    'Axis 2 - Rows
                    t = New ResultTable
                    t.AddTitle("Axis 2")
                    t.AddHeaderLeftRow(Me.rowNames)
                    t.AddHeaderTopRow({"Row Name", "Factor", "Correlation", "Contribution", "Angle", "Eigenvalue"})
                    Dim tmp3(Me.pR - 1, 4) As Object
                    For i = 0 To Me.pR - 1
                        tmp3(i, 0) = Me.RowFactors(1)(i)
                        tmp3(i, 1) = Me.RowCorr(1)(i)
                        tmp3(i, 2) = Me.RowContribution(1)(i)
                        tmp3(i, 3) = Me.RowAngle(1)(i)
                        tmp3(i, 4) = Me.RowEigenvalueContrib(1)(i)
                    Next
                    t.SetBody(tmp3)
                    out.Add(t)
                End If

                'Principal Coordinates for Columns
                t = New ResultTable
                t.AddTitle("Principal Coordinates for Columns")
                t.AddHeaderLeftRow(Me.ColumNames)
                t.AddHeaderTopRow({"Column Name", "Quality", "Mass", "Distance", "Inertia"})
                Dim tmp4(Me.pC - 1, 3) As Object
                For i = 0 To Me.pC - 1
                    tmp4(i, 0) = Me.ColQuality(i)
                    tmp4(i, 1) = Me.ColMass(i)
                    tmp4(i, 2) = Me.ColDistance(i)
                    tmp4(i, 3) = Me.ColInertia(i)
                Next
                t.SetBody(tmp4)
                out.Add(t)

                'Axis 1 - Columns
                If axesAvail >= 1 Then
                    t = New ResultTable
                    t.AddTitle("Axis 1")
                    t.AddHeaderLeftRow(Me.ColumNames)
                    t.AddHeaderTopRow({"Column Name", "Factor", "Correlation", "Contribution", "Angle", "Eigenvalue"})
                    Dim tmp5(Me.pC - 1, 4) As Object
                    For i = 0 To Me.pC - 1
                        tmp5(i, 0) = Me.ColFactors(0)(i)
                        tmp5(i, 1) = Me.ColCorr(0)(i)
                        tmp5(i, 2) = Me.ColContribution(0)(i)
                        tmp5(i, 3) = Me.ColAngle(0)(i)
                        tmp5(i, 4) = Me.ColEigenvalueContrib(0)(i)
                    Next
                    t.SetBody(tmp5)
                    out.Add(t)
                End If

                'Axis 2 - Columns
                If axesAvail >= 2 Then
                    t = New ResultTable
                    t.AddTitle("Axis 2")
                    t.AddHeaderLeftRow(Me.ColumNames)
                    t.AddHeaderTopRow({"Column Name", "Factor", "Correlation", "Contribution", "Angle", "Eigenvalue"})
                    Dim tmp6(Me.pC - 1, 4) As Object
                    For i = 0 To Me.pC - 1
                        tmp6(i, 0) = Me.ColFactors(1)(i)
                        tmp6(i, 1) = Me.ColCorr(1)(i)
                        tmp6(i, 2) = Me.ColContribution(1)(i)
                        tmp6(i, 3) = Me.ColAngle(1)(i)
                        tmp6(i, 4) = Me.ColEigenvalueContrib(1)(i)
                    Next
                    t.SetBody(tmp6)
                    out.Add(t)
                End If
            End If

            Return out
        End Function

        ''' <summary>
        ''' Computes CA/MCA factors, inertias, and diagnostics for the currently loaded data.
        ''' </summary>
        ''' <remarks>
        ''' <para><b>Computation steps</b></para>
        ''' <list type="number">
        '''   <item><description>Compute proportions <c>P = N / ΣN</c> from the current count matrix (<c>pData</c>).</description></item>
        '''   <item><description>Compute masses <c>r</c> and <c>c</c> (row/column marginals).</description></item>
        '''   <item><description>Center against independence and standardize: <c>S = D_r^{-1/2}(P - r cᵀ)D_c^{-1/2}</c>.</description></item>
        '''   <item><description>SVD: <c>S = U diag(σ) Vᵀ</c>, sorted in descending <c>σ</c>.</description></item>
        '''   <item><description>Coordinates: <c>F = D_r^{-1/2}U diag(σ)</c>, <c>G = D_c^{-1/2}V diag(σ)</c>.</description></item>
        '''   <item><description>Eigenvalues: <c>λₖ = σₖ²</c>. Percent inertia is <c>100·λₖ/Σλ</c>.</description></item>
        '''   <item><description>Distances (chi-square): based on deviations of row/column profiles from the marginals.</description></item>
        '''   <item><description>Diagnostics:
        '''     <list type="bullet">
        '''       <item><description><b>Cos²</b> (a.k.a. squared correlations): <c>cos²ᵢₖ = fᵢₖ² / dᵢ²</c></description></item>
        '''       <item><description><b>Contributions</b>: <c>ctrᵢₖ = rᵢ fᵢₖ² / λₖ</c> (analogous for columns)</description></item>
        '''       <item><description><b>Angles</b>: <c>acos(sqrt(cos²))</c> in degrees</description></item>
        '''       <item><description><b>Quality</b>: sum of cos² over the axes computed (here, the first two where available)</description></item>
        '''     </list>
        '''   </description></item>
        ''' </list>
        '''
        ''' <para><b>Axis indexing</b></para>
        ''' <para>
        ''' Internally, axes are 0-based: Factor 1 is index 0, Factor 2 is index 1, etc.
        ''' Public accessors such as <see cref="RowFactors"/> and <see cref="ColFactors"/> follow the same convention.
        ''' </para>
        '''
        ''' <para><b>Numerical notes</b></para>
        ''' <list type="bullet">
        '''   <item><description>Zero or extremely small masses (rare categories) can cause unstable scaling via <c>D^{-1/2}</c>.</description></item>
        '''   <item><description>Axis signs are arbitrary (see class remarks). A sign flip does not change any squared diagnostic.</description></item>
        ''' </list>
        ''' </remarks>
        ''' <exception cref="InvalidOperationException">
        ''' Thrown if there are not enough dimensions to compute at least one axis.
        ''' </exception>
        Public Sub Calculate()
            Dim axesToCompute As Integer = Math.Min(2, pDim + 1)  'pDim+1 = number of eigenvalues stored
            If axesToCompute < 1 Then
                BESHstatGlobals.BSerr.LogAndThrow(New InvalidOperationException("Not enough dimensions for correspondence analysis."))
            End If

            Dim prop(pR - 1, pC - 1) As Double, Dr(pR - 1, pR - 1) As Double, Dc(pC - 1, pC - 1) As Double
            ReDim pRowTot(pR - 1), pColTot(pC - 1)
            Dim tot As Double = Sum2D(pData)
            For i As Integer = 0 To pR - 1
                For j As Integer = 0 To pC - 1
                    prop(i, j) = pData(i, j) / tot
                    pRowTot(i) = pRowTot(i) + prop(i, j)
                    pColTot(j) = pColTot(j) + prop(i, j)
                Next
            Next

            For i As Integer = 0 To pR - 1
                Dr(i, i) = 1.0 / Math.Sqrt(pRowTot(i))
            Next
            For j As Integer = 0 To pC - 1
                Dc(j, j) = 1.0 / Math.Sqrt(pColTot(j))
            Next

            'Compute residuals from independence: (P - r c^T)
            Dim resid(pR - 1, pC - 1) As Double
            For i As Integer = 0 To pR - 1
                For j As Integer = 0 To pC - 1
                    resid(i, j) = prop(i, j) - (pRowTot(i) * pColTot(j))
                Next
            Next

            Dim scaledMat(,) As Double = Matrix.MatrixMult(Matrix.MatrixMult(Dr, resid), Dc)

            'Do the SVD
            'Given a matrix a(1:m,1:n), this routine computes its singular value decomposition, A = U * W * V ^t .
            'The matrix U replaces A on output. The diagonal matrix of singular values W is output as a vector w(1:n).
            'The matrix V (not the transpose V T ) is output as V(1:n,1:n).

            'Do the computation for Rows ------------------------------------------------------------------
            Dim svd As Matrix.SVDoutput = Matrix.SVD_decomp(scaledMat) 'W is the square root of eigenvalues
            Dim tmp2(,) As Double = Matrix.MatrixMult(Matrix.MatrixMult(Dr, svd.U), svd.Wmat) 'is the CA row factor matrix

            'We need to reorder Factor based on the Eigenvalues (largest -> smallest)
            Dim Ordr(svd.Wvect.Length - 1) As Integer
            For k As Integer = 0 To Ordr.Length - 1
                Ordr(k) = k
            Next

            Dim Wtemp() As Double = CType(svd.Wvect.Clone(), Double())
            Array.Sort(Wtemp, Ordr)   'ascending
            Array.Reverse(Wtemp)      'descending
            Array.Reverse(Ordr)

            '0-based axes: axis 0 stored in column 0
            ReDim pRowFactors(pR - 1, pDim)
            For k As Integer = 0 To pDim
                For r As Integer = 0 To pR - 1
                    pRowFactors(r, k) = tmp2(r, Ordr(k))
                Next
            Next


            'Get Eigenvalues (standard CA on the centered matrix): eigenvalue_k = singularValue_k^2
            ReDim pEigenvalues(pDim)
            For k As Integer = 0 To pDim
                pEigenvalues(k) = Wtemp(k) * Wtemp(k)
            Next


            'Compute Distance
            ReDim pRowDistance(pR - 1), pRowInertia(pR - 1), pRowCorr(pR - 1, axesToCompute - 1), pRowQuality(pR - 1), pRowContribution(pR - 1, axesToCompute - 1)
            ReDim pRowAngle(pR - 1, axesToCompute - 1), pRowEigencontrib(pR - 1, axesToCompute - 1)
            For i As Integer = 0 To pR - 1
                For j As Integer = 0 To pC - 1
                    pRowDistance(i) += (1.0 / pColTot(j)) * (prop(i, j) / pRowTot(i) - pColTot(j)) ^ 2
                Next
            Next
            'Compute Inertia

            Dim totInertia As Double
            For i As Integer = 0 To pR - 1
                pRowInertia(i) = pRowTot(i) * pRowDistance(i)
                totInertia += pRowInertia(i)
                For j As Integer = 0 To axesToCompute - 1
                    Dim f As Double = pRowFactors(i, j)

                    Dim corr As Double
                    pRowAngle(i, j) = CorrAndAngleFromFactorAndDistance(f, pRowDistance(i), corr)
                    pRowCorr(i, j) = corr

                    pRowQuality(i) += corr
                    pRowContribution(i, j) = (pRowTot(i) * f * f) / pEigenvalues(j)
                    pRowEigencontrib(i, j) = pRowContribution(i, j) * pEigenvalues(j)
                Next
            Next

            For i As Integer = 0 To pR - 1
                pRowInertia(i) /= totInertia
            Next


            'Do the computation for Columns ---------------------------------------------------------------
            tmp2 = Matrix.MatrixMult(Matrix.MatrixMult(Dc, svd.V), Matrix.trans(svd.Wmat)) 'is the CA column factor matrix
            'ReDim factors the same way as rows: axis 0 stored in column 0
            ReDim pColFactors(pC - 1, pDim)
            For k As Integer = 0 To pDim
                For j As Integer = 0 To pC - 1
                    pColFactors(j, k) = tmp2(j, Ordr(k))
                Next
            Next


            'Compute Distance
            ReDim pColDistance(pC - 1), pColInertia(pC - 1), pColCorr(pC - 1, axesToCompute - 1), pColQuality(pC - 1), pColContribution(pC - 1, axesToCompute - 1)
            ReDim pColAngle(pC - 1, axesToCompute - 1), pColEigencontrib(pC - 1, axesToCompute - 1), pColContributionSigned(pC - 1, axesToCompute - 1)
            For i As Integer = 0 To pC - 1
                For j As Integer = 0 To pR - 1
                    pColDistance(i) += (1.0 / pRowTot(j)) * (prop(j, i) / pColTot(i) - pRowTot(j)) ^ 2
                Next
            Next

            'Compute Inertia
            totInertia = 0
            For i As Integer = 0 To pC - 1
                pColInertia(i) = pColTot(i) * pColDistance(i)
                totInertia += pColInertia(i)
                For j As Integer = 0 To axesToCompute - 1
                    Dim f As Double = pColFactors(i, j)

                    Dim corr As Double
                    pColAngle(i, j) = CorrAndAngleFromFactorAndDistance(f, pColDistance(i), corr)
                    pColCorr(i, j) = corr

                    pColQuality(i) += corr
                    pColContribution(i, j) = (pColTot(i) * f * f) / pEigenvalues(j)
                    pColEigencontrib(i, j) = pColContribution(i, j) * pEigenvalues(j)

                    pColContributionSigned(i, j) = pColContribution(i, j) * Math.Sign(f)
                Next
            Next


            For i As Integer = 0 To pC - 1
                pColInertia(i) /= totInertia
            Next
        End Sub

        ''' <summary>
        ''' Creates the MCA indicator (design) matrix <c>Z</c> using one-hot encoding.
        ''' </summary>
        ''' <remarks>
        ''' <para>
        ''' Produces a 0/1 matrix of size <c>N×K</c> where K is the total number of categories across variables.
        ''' Columns are grouped by variable, and category order within each variable is the deterministic order built by
        ''' <see cref="CreateCrossTab"/> (stored in <c>pLevels</c>).
        ''' </para>
        ''' <para><b>Correctness constraints</b></para>
        ''' <list type="bullet">
        '''   <item><description>Each row should contain exactly Q ones (one active category per variable) if there are no missing values.</description></item>
        '''   <item><description>Global column indices are computed using cumulative offsets (<c>pCatTots</c>).</description></item>
        ''' </list>
        ''' <para><b>Performance note</b></para>
        ''' <para>
        ''' The indicator matrix is typically very sparse. For very large N, consider computing MCA from the Burt table
        ''' and projecting individuals (common in R packages) to avoid materializing <c>Z</c>.
        ''' </para>
        ''' </remarks>
        ''' <exception cref="ArgumentException">
        ''' Thrown if an observed category value is not present in the level map for its variable.
        ''' </exception>
        Private Sub CreateDesignMatrix()
            Dim r As Integer = UBound(pDataMultiple, 1) + 1 ' records
            Dim p As Integer = UBound(pDataMultiple, 2) + 1 ' variables

            If pLevels Is Nothing OrElse pLevels.Count <> p Then
                Throw New InvalidOperationException("CreateDesignMatrix: category levels were not built correctly.")
            End If

            Dim totalCats As Integer = pCatTots(p - 1)
            ReDim pDesignMatrix(r - 1, totalCats - 1)

            'Build per-variable map: category -> local index
            Dim arDic As New List(Of Dictionary(Of String, Integer))(p)
            For varIdx As Integer = 0 To p - 1
                Dim dic As New Dictionary(Of String, Integer)(StringComparer.Ordinal)
                Dim levels = pLevels(varIdx)
                For j As Integer = 0 To levels.Length - 1
                    dic(levels(j)) = j
                Next
                arDic.Add(dic)
            Next

            'Fill design matrix
            For i As Integer = 0 To r - 1
                For varIdx As Integer = 0 To p - 1
                    Dim raw As String = pDataMultiple(i, varIdx)
                    If raw IsNot Nothing Then raw = raw.Trim() Else raw = ""

                    Dim localIdx As Integer
                    If Not arDic(varIdx).TryGetValue(raw, localIdx) Then
                        Throw New ArgumentException(
                    $"Unknown category '{raw}' in row {i}, variable '{pVarNames(varIdx)}'.")
                    End If

                    Dim globalIdx As Integer =
                If(varIdx = 0, localIdx, pCatTots(varIdx - 1) + localIdx)

                    pDesignMatrix(i, globalIdx) = 1
                Next
            Next
        End Sub

        ''' <summary>
        ''' Computes per-variable category frequencies and a deterministic level ordering (MCA preprocessing).
        ''' </summary>
        ''' <remarks>
        ''' <para>
        ''' For each variable column in <c>pDataMultiple</c>, values are trimmed and grouped to produce
        ''' a frequency dictionary (<c>pCrossTab</c>). The ordered unique values are stored in <c>pLevels</c>.
        ''' </para>
        ''' <para>
        ''' The cumulative category offsets <c>pCatTots</c> are computed so that variable blocks can be addressed
        ''' in the design matrix and Burt table.
        ''' </para>
        ''' </remarks>
        Private Sub CreateCrossTab()
            pCrossTab.Clear()
            pLevels.Clear()

            Dim p As Integer = UBound(pVarNames) + 1 ' number of variables
            ReDim pCatTots(p - 1)

            'pre-allocate row names (will shrink later)
            ReDim pRowNames((UBound(pDataMultiple, 1) + 1) * (UBound(pDataMultiple, 2) + 1) - 1)

            pR = 0
            Dim nameIdx As Integer = 0

            For varIdx As Integer = 0 To p - 1
                Dim col() As String = Matrix.GetColumnFrom2Darray(pDataMultiple, varIdx)

                'normalize (optional, but reduces "same category with trailing spaces" issues)
                For i As Integer = 0 To col.Length - 1
                    If col(i) IsNot Nothing Then col(i) = col(i).Trim()
                Next

                Dim frequencies As Dictionary(Of String, Integer) = col.GroupBy(Function(s) If(s, "")).
                                                                    ToDictionary(Function(g) g.Key, Function(g) g.Count())

                'stable order (important for consistent Burt/DesignMatrix/labels)
                Dim levels() As String = frequencies.Keys.OrderBy(Function(s) s).ToArray()

                pCrossTab.Add(frequencies)
                pLevels.Add(levels)

                For Each key In levels
                    pRowNames(nameIdx) = key
                    nameIdx += 1
                Next

                If varIdx = 0 Then
                    pCatTots(varIdx) = levels.Length
                Else
                    pCatTots(varIdx) = pCatTots(varIdx - 1) + levels.Length
                End If

                pR += levels.Length
            Next

            pC = pR
            pDim = pR - 1

            ReDim Preserve pRowNames(pR - 1)
            pColNames = pRowNames
        End Sub

        ''' <summary>
        ''' Builds the Burt table <c>B</c> (MCA summary of all pairwise cross-tabulations).
        ''' </summary>
        ''' <remarks>
        ''' <para>
        ''' The Burt table is a symmetric <c>K×K</c> matrix where each diagonal block contains univariate category
        ''' counts for a variable, and each off-diagonal block contains the contingency table between two variables’ categories.
        ''' </para>
        ''' <para>
        ''' Current implementation fills off-diagonal blocks by repeatedly calling <see cref="ComputeCrossCount"/>,
        ''' which is O(N) per category-pair. For many variables/categories, a faster approach is to compute each
        ''' block using a single pass over individuals with a dictionary accumulator.
        ''' </para>
        ''' </remarks>
        Private Sub CreateBurtTable()

            Dim p As Integer = UBound(pVarNames)
            ReDim pVarNamesToPresent(pR - 1), pBurtTable(pR - 1, pC - 1) 'It's a square matrix

            'populate cross tab counts for the same variables
            Dim i As Integer = 0
            For idx As Integer = 0 To p 'number of variables
                For Each key In pLevels(idx)
                    pBurtTable(i, i) = pCrossTab(idx)(key)
                    pVarNamesToPresent(i) = pVarNames(idx)
                    i += 1
                Next
            Next idx

            'get cross tab counts for different variables
            For idx As Integer = 0 To p 'variables
                For idx2 As Integer = idx + 1 To p
                    Dim baseI As Integer = If(idx = 0, 0, pCatTots(idx - 1))
                    Dim baseJ As Integer = If(idx2 = 0, 0, pCatTots(idx2 - 1))

                    i = 0
                    For Each keyA In pLevels(idx)
                        Dim j As Integer = 0
                        For Each keyB In pLevels(idx2)
                            Dim v As Integer = ComputeCrossCount(keyA, keyB, idx, idx2)
                            pBurtTable(baseI + i, baseJ + j) = v
                            pBurtTable(baseJ + j, baseI + i) = v
                            j += 1
                        Next
                        i += 1
                    Next
                Next
            Next
        End Sub

        ''' <summary>
        ''' Counts the number of individuals having (<paramref name="strCat1"/>, <paramref name="strCat2"/>) for two variables.
        ''' </summary>
        ''' <param name="strCat1">Category value for variable <paramref name="varID1"/>.</param>
        ''' <param name="strCat2">Category value for variable <paramref name="varID2"/>.</param>
        ''' <param name="varID1">Zero-based variable index for <paramref name="strCat1"/>.</param>
        ''' <param name="varID2">Zero-based variable index for <paramref name="strCat2"/>.</param>
        ''' <returns>Co-occurrence count across all individuals.</returns>
        ''' <remarks>
        ''' Complexity is O(N) over individuals. Used for Burt off-diagonal blocks.
        ''' </remarks>
        Private Function ComputeCrossCount(strCat1 As String, strCat2 As String, varID1 As Integer, varID2 As Integer) As Integer
            'returns frequency for specified categories from two variables

            Dim tot As Integer
            Dim r As Integer = UBound(pDataMultiple, 1)
            For i As Integer = 0 To r
                If CStr(pDataMultiple(i, varID1)) = strCat1 And CStr(pDataMultiple(i, varID2)) = strCat2 Then tot += 1
            Next
            Return tot
        End Function

        ''' <summary>
        ''' Builds a label vector for plotting, optionally splitting labels across lines.
        ''' </summary>
        ''' <param name="bRow">If true, uses <see cref="rowNames"/>; otherwise uses <see cref="ColumNames"/>.</param>
        ''' <param name="splitChar">Character(s) used to split labels (e.g., <c>vbNewLine</c>).</param>
        ''' <returns>Label array aligned with the plotted point set.</returns>
        Private Function PlotLabels(bRow As Boolean, splitChar As String) As String()

            Dim CatNames() As String, Xlabels() As String

            If bRow Then CatNames = pRowNames Else CatNames = pColNames

            If pbMultiple Then
                ReDim Xlabels(UBound(pVarNamesToPresent))
            Else
                ReDim Xlabels(UBound(CatNames))
            End If

            For i As Integer = 0 To UBound(Xlabels)
                If pbMultiple Then
                    Xlabels(i) = pVarNamesToPresent(i) & ":" & splitChar & CatNames(i)
                Else
                    Xlabels(i) = CatNames(i)
                End If
            Next
            Return Xlabels
        End Function

        ''' <summary>
        ''' Computes the squared cosine (cos²) and the angular representation of a point on a given CA/MCA axis,
        ''' using the point’s factor coordinate and its chi-square distance to the origin.
        ''' </summary>
        ''' <param name="f">
        ''' The factor coordinate of the point on a single axis (e.g., a principal coordinate on axis <c>j</c>).
        ''' In this implementation the factor coordinate is taken from <c>pRowFactors(*, j+1)</c> /
        ''' <c>pColFactors(*, j+1)</c> where axis 1 is stored in column 1.
        ''' </param>
        ''' <param name="dist">
        ''' The (squared) chi-square distance of the point to the origin used by the class (e.g., <c>pRowDistance(i)</c> or
        ''' <c>pColDistance(i)</c>). In standard CA, the distance relates to the profile deviation from independence:
        ''' <para/>
        ''' <c>d_i^2 = Σ_k ( (p_{ik}/r_i - c_k)^2 / c_k )</c> for rows (analogous for columns).
        ''' <para/>
        ''' This class stores a distance-like scalar that is consistent with the internal coordinate scaling and is used
        ''' to compute cos² as <c>cos² = f² / dist</c>.
        ''' </param>
        ''' <param name="corr">
        ''' Output parameter that receives the squared cosine (cos²), sometimes called "quality of representation"
        ''' on this axis. For a point with factor coordinate <c>f</c> and distance <c>dist</c>:
        ''' <para/>
        ''' <c>corr = cos² = f² / dist</c>.
        ''' <para/>
        ''' For numerical stability, <c>corr</c> is clamped to the interval [0,1].
        ''' </param>
        ''' <returns>
        ''' The angle (in degrees) between the point and the axis in the CA/MCA factor space, computed as:
        ''' <para/>
        ''' <c>angle = acos(sqrt(corr)) * 180 / π</c>.
        ''' <para/>
        ''' Special cases:
        ''' <list type="bullet">
        ''' <item>
        ''' <description>
        ''' If <paramref name="dist"/> is zero or non-finite (NaN/∞), the point is effectively at the origin for the
        ''' purposes of cos²/angle. In that case this function defines <paramref name="corr"/> = 0 and returns 0°.
        ''' </description>
        ''' </item>
        ''' <item>
        ''' <description>
        ''' If floating-point rounding produces a slightly out-of-range cos² (e.g., 1.0000000002), the value is clamped
        ''' into [0,1] so that <c>sqrt</c> and <c>acos</c> remain in-domain and never return NaN.
        ''' </description>
        ''' </item>
        ''' </list>
        ''' </returns>
        ''' <remarks>
        ''' <para>
        ''' <b>Mathematical context (CA/MCA).</b>
        ''' In correspondence analysis, the squared cosine of a point on an axis measures the proportion of the point’s
        ''' squared distance to the origin that is explained by that axis. It is commonly used as a quality-of-display measure:
        ''' <c>cos²_{i,j} = (coord_{i,j}²) / (d_i²)</c>.
        ''' </para>
        ''' <para>
        ''' <b>Why this helper exists.</b>
        ''' The raw formula can produce NaN in practice when:
        ''' <list type="bullet">
        ''' <item><description><paramref name="dist"/> is exactly 0 (point at the origin) leading to division by zero.</description></item>
        ''' <item><description><paramref name="dist"/> is extremely small causing overflow/∞.</description></item>
        ''' <item><description>Rounding produces a cos² slightly outside [0,1], making <c>acos(sqrt(cos²))</c> invalid.</description></item>
        ''' </list>
        ''' This method enforces a deterministic, non-NaN output by applying explicit guards and clamping.
        ''' </para>
        ''' <para>
        ''' <b>Implementation note.</b>
        ''' This helper is typically called inside the row/column loops in <c>Fit()</c> to populate
        ''' <c>pRowCorr</c>/<c>pColCorr</c> and <c>pRowAngle</c>/<c>pColAngle</c>.
        ''' </para>
        ''' </remarks>
        Private Shared Function CorrAndAngleFromFactorAndDistance(f As Double, dist As Double, ByRef corr As Double) As Double
            ' Returns angle in degrees. Sets corr (cos²).
            ' If dist is 0 (point at origin), define corr=0 and angle=0.
            If dist <= 0 OrElse Double.IsNaN(dist) OrElse Double.IsInfinity(dist) Then
                corr = 0.0
                Return 0.0
            End If

            corr = (f * f) / dist

            ' Clamp due to floating point error (avoid sqrt/acos domain issues)
            If Double.IsNaN(corr) OrElse Double.IsInfinity(corr) Then corr = 0.0
            If corr < 0.0 Then corr = 0.0
            If corr > 1.0 Then corr = 1.0

            Return 180.0 * Math.Acos(Math.Sqrt(corr)) / Math.PI
        End Function


        ''' <summary>
        ''' Creates an Excel chart of contributions to a selected axis.
        ''' </summary>
        ''' <param name="lAxis">Axis index (0-based: 0 = Factor 1).</param>
        ''' <param name="bRow">If true, plots row contributions; otherwise plots column contributions.</param>
        ''' <param name="ws">Optional Excel worksheet to host the chart. If <c>Nothing</c>, uses the active chart.</param>
        ''' <remarks>
        ''' <para>
        ''' Contributions are computed as <c>mass × coordinate² / eigenvalue</c>.
        ''' Signed contributions (columns) additionally multiply by the sign of the coordinate to indicate direction.
        ''' </para>
        ''' <para>
        ''' Requires Excel Interop (STA is recommended depending on your host).
        ''' </para>
        ''' </remarks>
        Public Sub contribPlot(lAxis As Integer, bRow As Boolean, Optional ws As Worksheet = Nothing)

            Dim figure As Chart, seriesID As Integer, Contrib() As Double

            If bRow Then
                Contrib = RowContribution(lAxis)
            Else
                Contrib = ColContribution(lAxis)
            End If

            If ws Is Nothing Then
                BESHstatGlobals.app.Charts.Add()
                figure = BESHstatGlobals.app.ActiveWorkbook.ActiveChart
            Else
                figure = ws.Shapes.AddChart.Chart
            End If

            With figure
                .ChartType = XlChartType.xlColumnClustered
                .HasTitle = False
                .HasTitle = True
                .ChartTitle.Text = "Contribution Plot: Axis " & CStr(lAxis + 1)
                If pbMultiple Then .Name = "Contribution" & CStr(lAxis + 1)
                .HasLegend = False

                'delete extra series
                Do Until .SeriesCollection.Count = 0
                    .SeriesCollection(1).Delete
                Loop

                seriesID = 1
                .SeriesCollection.NewSeries
                With .SeriesCollection(seriesID)
                    .XValues = PlotLabels(bRow, vbNewLine)
                    .Values = Contrib
                End With

                Try
                    .Axes(XlAxisType.xlValue, XlAxisGroup.xlPrimary).HasTitle = False
                    .Axes(XlAxisType.xlValue, XlAxisGroup.xlPrimary).HasTitle = True
                    .Axes(XlAxisType.xlValue, XlAxisGroup.xlPrimary).AxisTitle.text = "Contribution"
                    .ChartTitle.Font.Bold = True
                Catch
                End Try
            End With
        End Sub

        ''' <summary>
        ''' Creates a 2D correspondence map (Factor 1 vs Factor 2) in Excel.
        ''' </summary>
        ''' <param name="ws">Optional Excel worksheet to host the chart. If <c>Nothing</c>, uses the active chart.</param>
        ''' <remarks>
        ''' <para>
        ''' X-axis uses Factor 1 coordinates (<see cref="RowFactors"/>(0), <see cref="ColFactors"/>(0));
        ''' Y-axis uses Factor 2 coordinates (<see cref="RowFactors"/>(1), <see cref="ColFactors"/>(1)).
        ''' </para>
        ''' <para>
        ''' Ensure that at least two axes are available (i.e., at least two non-trivial dimensions).
        ''' For very small tables, only one axis may exist.
        ''' </para>
        ''' </remarks>
        Public Sub plot(Optional ws As Worksheet = Nothing)
            Dim seriesID As Integer
            Dim figure As Chart

            If UBound(pRowFactors, 2) = 0 Then Exit Sub 'We need two axis to plot (may not be possible for small tables)

            'compute optimal scaling
            Dim udPlotAxisY As graphics.CHARTscale = graphics.ChartScaling(Math.Min(RowFactors(1).Min(), ColFactors(1).Min()), Math.Max(RowFactors(1).Max(), ColFactors(1).Max()))
            Dim udPlotAxisX As graphics.CHARTscale = graphics.ChartScaling(Math.Min(RowFactors(0).Min(), ColFactors(0).Min()), Math.Max(RowFactors(0).Max(), ColFactors(0).Max()))

            If ws Is Nothing Then
                BESHstatGlobals.app.Charts.Add()
                figure = BESHstatGlobals.app.ActiveWorkbook.ActiveChart
            Else
                figure = ws.Shapes.AddChart.Chart
            End If

            With figure
                .ChartType = XlChartType.xlXYScatter
                .ChartStyle = 1
                .HasTitle = False
                .HasTitle = True
                If pbMultiple Then .Name = "CA plot"

                'delete extra series
                Do Until .SeriesCollection.Count = 0
                    .SeriesCollection(1).Delete
                Loop

                .Axes(XlAxisType.xlValue).MajorGridlines.Delete
                .Axes(XlAxisType.xlCategory).MinimumScale = udPlotAxisX.Min
                .Axes(XlAxisType.xlCategory).MaximumScale = udPlotAxisX.Max
                .Axes(XlAxisType.xlCategory).MajorUnit = udPlotAxisX.Scale
                .Axes(XlAxisType.xlValue).CrossesAt = -1.0E+100
                .Axes(XlAxisType.xlCategory).CrossesAt = -1.0E+100
                .Axes(XlAxisType.xlValue).MinimumScale = udPlotAxisY.Min
                .Axes(XlAxisType.xlValue).MaximumScale = udPlotAxisY.Max
                .Axes(XlAxisType.xlValue).MajorUnit = udPlotAxisY.Scale

                seriesID = 1
                .SeriesCollection.NewSeries
                With .SeriesCollection(seriesID)
                    .XValues = ColFactors(0)
                    .Values = ColFactors(1)
                    .Name = "Columns"
                    .MarkerStyle = XlMarkerStyle.xlMarkerStyleCircle
                    .MarkerSize = 6
                    .MarkerForegroundColor = graphics.GetColor(10) 'tomato
                    .MarkerBackgroundColor = graphics.GetColor(10) 'tomato
                    '.Format.Fill.Visible = msoFalse

                    'Attach a label to each data point
                    For i = 1 To pC
                        .points(i).HasDataLabel = True
                        .points(i).DataLabel.text = PlotLabels(False, vbNullString)(i - 1) 'pColNames(i)
                    Next
                End With

                If Not pbMultiple Then 'We analyze design matrix in multiple analysis so are not displaying rows
                    seriesID += 1
                    .SeriesCollection.NewSeries
                    With .SeriesCollection(seriesID)
                        .XValues = RowFactors(0)
                        .Values = RowFactors(1)
                        .Name = "Rows"
                        .MarkerStyle = XlMarkerStyle.xlMarkerStyleCircle
                        .MarkerSize = 6
                        .MarkerForegroundColor = RGB(0, 0, 150)
                        .MarkerBackgroundColor = RGB(0, 0, 150)
                        '.Format.Fill.Visible = msoFalse

                        'Attach a label to each data point
                        For i = 1 To pR
                            .points(i).HasDataLabel = True
                            .points(i).DataLabel.text = PlotLabels(True, vbNullString)(i - 1) 'pRowNames(i)
                        Next
                    End With
                End If

                'add and plot zero lines
                'add zero lines
                .SeriesCollection.NewSeries
                seriesID += 1
                With .SeriesCollection(seriesID)
                    .XValues = {udPlotAxisX.Min, udPlotAxisX.Max}
                    .Values = {0, 0}
                    .Name = "Y Zero Line"
                    .MarkerStyle = XlMarkerStyle.xlMarkerStyleNone
                    .Border.Color = RGB(0, 0, 0)
                    With .Format.Line
                        .Visible = True
                        .Weight = 0.5
                    End With
                End With
                .SeriesCollection.NewSeries
                seriesID += 1
                With .SeriesCollection(seriesID)
                    .XValues = {0, 0}
                    .Values = {udPlotAxisY.Min, udPlotAxisY.Max}
                    .Name = "X Zero Line"
                    .MarkerStyle = XlMarkerStyle.xlMarkerStyleNone
                    .Border.Color = RGB(0, 0, 0)
                    With .Format.Line
                        .Visible = True
                        .Weight = 0.5
                    End With
                End With

                Try
                    If pbMultiple Then
                        .HasLegend = False
                    Else
                        For i = 4 To 3 Step -1
                            .Legend.LegendEntries(i).Delete
                        Next
                    End If
                Catch
                End Try

                Try
                    .Axes(XlAxisType.xlValue, XlAxisGroup.xlPrimary).HasTitle = False
                    .Axes(XlAxisType.xlValue, XlAxisGroup.xlPrimary).HasTitle = True
                    .Axes(XlAxisType.xlValue, XlAxisGroup.xlPrimary).AxisTitle.text = $"Factor 2 [{ Format$(Percents(2, 1), "#0.0#") }%]"
                    .Axes(XlAxisType.xlCategory, XlAxisGroup.xlPrimary).HasTitle = False
                    .Axes(XlAxisType.xlCategory, XlAxisGroup.xlPrimary).HasTitle = True
                    .Axes(XlAxisType.xlCategory, XlAxisGroup.xlPrimary).AxisTitle.text = $"Factor 1 [{ Format$(Percents(1, 1), "#0.0#") }%]"
                    .ChartTitle.Text = "Correspondence Plot"
                    .ChartTitle.Font.Bold = True
                Catch
                End Try
            End With
        End Sub

    End Class
End Namespace