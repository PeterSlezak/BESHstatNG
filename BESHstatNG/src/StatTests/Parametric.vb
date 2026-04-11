Option Explicit On
Imports System.IO
Imports System.Windows.Forms.AxHost
Imports System.Windows.Forms.VisualStyles.VisualStyleElement.ToolTip
Imports BESHStatNG.AppInfrastructure
Imports Microsoft.Office.Interop.Excel

Namespace parametric


    Public Module Parametric

        Friend Function BuildMcpCiFootnote(alpha As Double) As String
            Return "Confidence interval level = " & ((1.0 - alpha) * 100.0).ToString("0.##") & "% (alpha = " & alpha.ToString("0.####") & ")."
        End Function

        Friend Sub ValidateAlpha(alpha As Double)
            If Double.IsNaN(alpha) OrElse alpha <= 0.0 OrElse alpha >= 1.0 Then
                AppGlobals.BSerr.LogAndThrow(New ArgumentOutOfRangeException(NameOf(alpha), "alpha must be in (0,1)."))
            End If
        End Sub

        ''' <summary>
        ''' Implements a two‑way nested ANOVA model of the form:
        ''' 
        '''     Y_ijk = μ + A_i + B_j(i) + ε_ijk
        ''' 
        ''' where factor B is nested within factor A. The class computes:
        ''' <list type="bullet">
        '''   <item><description>Sum of squares for Groups (A)</description></item>
        '''   <item><description>Sum of squares for Subgroups within Groups (B(A))</description></item>
        '''   <item><description>Residual sum of squares</description></item>
        '''   <item><description>Degrees of freedom, mean squares, F‑tests</description></item>
        '''   <item><description>Satterthwaite‑adjusted F‑test (optional)</description></item>
        '''   <item><description>Variance‑component estimates and percentages</description></item>
        ''' </list>
        ''' 
        ''' The class also detects whether the design is balanced and returns
        ''' formatted <c>ResultTable</c> objects for reporting.
        ''' </summary>
        Public Class TwoWayNestedANOVA
            Private data(,) As Object
            Private varNames() As String
            Private parGroupS() As String
            Private parSubGroupS() As String
            Private parRes() As Double
            Private parGroupID() As String 'unique group categories
            Private parGroupFrq() As Integer 'group category counts
            Private parSubGroupID() As String 'unique subgroup categories
            Private parSubGroupFrq() As Integer 'subgroup category counts
            Private pANOVAtable(3, 5) As Object
            Private pANOVAtabSW(3, 4) As Object
            Private pbBalanced As Boolean = False

            ''' <summary>
            ''' Indicates whether the nested ANOVA design is balanced. A design is
            ''' considered balanced when:
            ''' <list type="bullet">
            '''   <item><description>All groups have equal sample sizes</description></item>
            '''   <item><description>All subgroups have equal sample sizes</description></item>
            '''   <item><description>All subgroup‑within‑group cells have equal frequencies</description></item>
            ''' </list>
            ''' </summary>
            Public ReadOnly Property balancedDesign() As Boolean
                Get
                    Return Me.pbBalanced
                End Get
            End Property

            ''' <summary>
            ''' Initializes the nested ANOVA model using a 3‑column data matrix:
            ''' <list type="bullet">
            '''   <item><description>Column 0: Group factor (A)</description></item>
            '''   <item><description>Column 1: Subgroup factor nested within A (B(A))</description></item>
            '''   <item><description>Column 2: Response variable</description></item>
            ''' </list>
            ''' The constructor:
            ''' <list type="bullet">
            '''   <item><description>Sorts the data by group and subgroup</description></item>
            '''   <item><description>Extracts factor levels and response values</description></item>
            '''   <item><description>Computes frequency distributions for A and B(A)</description></item>
            ''' </list>
            ''' </summary>
            ''' <param name="x">Data matrix (n × 3).</param>
            ''' <param name="varNames">Variable names for reporting.</param>
            Public Sub New(x(,) As Object, varNames() As String)

                Me.data = x
                Me.varNames = varNames
                QuickSort2D(Me.data, "0,A,1,A", 0, UBound(Me.data, 1)) 'sort by 1st and 2nd column
                ReDim parGroupS(UBound(Me.data, 1)), parSubGroupS(UBound(Me.data, 1)), parRes(UBound(Me.data, 1))

                For i = 0 To UBound(data, 1)
                    Me.parGroupS(i) = data(i, 0)
                    Me.parSubGroupS(i) = data(i, 1)
                    Me.parRes(i) = data(i, 2)
                Next

                Me.freq(Me.parGroupS, Me.parGroupID, Me.parGroupFrq)
                Me.freq(Me.parSubGroupS, Me.parSubGroupID, Me.parSubGroupFrq) 'get subgroup categories
            End Sub

            ''' <summary>
            ''' Wraps the computed ANOVA results into a list of <c>ResultTable</c> objects.
            ''' Produces:
            ''' <list type="bullet">
            '''   <item><description>Main ANOVA table with variance‑component percentages</description></item>
            '''   <item><description>Satterthwaite‑adjusted ANOVA table (if applicable)</description></item>
            ''' </list>
            ''' </summary>
            ''' <returns>A list of formatted <c>ResultTable</c> instances.</returns>
            Public Function wrapResults() As List(Of ResultTable)
                Dim out = New List(Of ResultTable)
                Dim t = New ResultTable
                t.SetBody(Me.pANOVAtable)
                t.AddHeaderLeftRow({"Between Groups", "Subgroups within Groups", "Within SubGroups", "Total"})
                t.AddHeaderTopRow({"Source of Variation", "SS", "df", "MS", "F", "P-value", "Variance Compontent %"})
                out.Add(t)

                Dim t2 = New ResultTable
                If pANOVAtabSW(0, 3) = -99 Then
                    t2.SetBody({{"Not Applicable"}})
                Else
                    t2.SetBody(Me.pANOVAtabSW)
                    t2.AddHeaderLeftRow({"Between Groups", "Subgroups within Groups", "Within SubGroups", "Total"})
                    t2.AddHeaderTopRow({"Source of Variation", "SS", "df", "MS", "F", "P-value"})
                End If
                out.Add(t2)

                Return out
            End Function

            ''' <summary>
            ''' Computes distinct category values and their frequencies for a factor.
            ''' Uses LINQ grouping to extract:
            ''' <list type="bullet">
            '''   <item><description>Distinct factor levels</description></item>
            '''   <item><description>Counts per level</description></item>
            ''' </list>
            ''' </summary>
            ''' <param name="x">Factor vector.</param>
            ''' <param name="DistinctValues">Output array of unique factor levels.</param>
            ''' <param name="counts">Output array of frequencies.</param>
            Private Sub freq(x() As String, ByRef DistinctValues() As String, ByRef counts() As Integer)
                'get the frequency of the 1st column
                Dim grouped = From itm In x
                              Group itm By itm Into Group
                              Select itm, count = Group.Count()

                ' Convert to arrays
                DistinctValues = grouped.Select(Function(g) g.itm).ToArray()
                counts = grouped.Select(Function(g) g.count).ToArray()
            End Sub

            ''' <summary>
            ''' Performs the full two‑way nested ANOVA computation, including:
            ''' <list type="bullet">
            '''   <item><description>Grand mean and total sum of squares</description></item>
            '''   <item><description>Group (A) means and SS_A</description></item>
            '''   <item><description>Subgroup‑within‑group means and SS_B(A)</description></item>
            '''   <item><description>Residual sum of squares</description></item>
            '''   <item><description>Degrees of freedom and mean squares</description></item>
            '''   <item><description>F‑tests for A and B(A)</description></item>
            '''   <item><description>Balanced‑design detection</description></item>
            '''   <item><description>Satterthwaite approximation for mixed‑model F‑tests</description></item>
            '''   <item><description>Variance‑component estimation:
            '''       <list type="bullet">
            '''         <item><description>σ²_A (between groups)</description></item>
            '''         <item><description>σ²_B(A) (between subgroups within groups)</description></item>
            '''         <item><description>σ²_Error (within subgroups)</description></item>
            '''       </list>
            '''     </description></item>
            '''   <item><description>Percentage contribution of each variance component</description></item>
            ''' </list>
            ''' </summary>
            ''' <returns>
            ''' A 2D Object array representing the main ANOVA table:
            ''' <c>{SS, df, MS, F, p‑value, variance‑component %}</c>.
            ''' </returns>
            Public Function compute() As Object(,)

                'ANOVA table
                Dim SSgroup As Double
                'variance components
                Dim var_raw_group As Double, var_raw_subwgroup As Double, var_raw_subgroup As Double, var_tot As Double
                'Satterthwaite approximation
                Dim tmp As Double, n0 As Double, n0p As Double, w2 As Double, w1 As Double, r As Double, c As Double

                Dim arAll(data.GetUpperBound(0), 2) As Double
                Dim grandMean As Double = Me.parRes.Average()
                Dim SStot As Double = DevSq(Me.parRes)

                Dim bMeans = New Dictionary(Of String, Double)
                Dim aMeans = New Dictionary(Of String, Double)
                ' Compute means per B
                For Each b In Me.parSubGroupID
                    Dim indices = Me.parSubGroupS.Select(Function(s, i) If(s = b, i, -1)).Where(Function(i) i >= 0)
                    bMeans(b) = indices.Average(Function(i) Me.parRes(i))
                Next

                ' Compute means per A
                For Each a In Me.parGroupID
                    Dim indices = Me.parGroupS.Select(Function(g, i) If(g = a, i, -1)).Where(Function(i) i >= 0)
                    aMeans(a) = indices.Average(Function(i) Me.parRes(i))
                Next
                ' Compute Group SS - SS_A
                For Each a In Me.parGroupID
                    Dim n = Me.parGroupS.Where(Function(kk) kk = a).Count()
                    SSgroup += n * (aMeans(a) - grandMean) ^ 2
                Next

                ' Compute SS_B(A)
                Dim ssBA As Double = 0
                Dim dfBA As Integer = 0
                For Each b In Me.parSubGroupID
                    Dim indices = Me.parSubGroupS.Select(Function(s, i) If(s = b, i, -1)).Where(Function(i) i >= 0).ToList()
                    Dim n = indices.Count
                    Dim a = Me.parGroupS(indices(0)) ' get parent A
                    Dim diff = bMeans(b) - aMeans(a)
                    ssBA += n * diff * diff
                    dfBA += 1
                Next

                ' Compute MS_B(A)
                dfBA -= Me.parGroupID.Length
                Dim msBA As Double = ssBA / dfBA

                ' Compute SS_Error
                Dim ssError As Double = 0
                For i = 0 To Me.parRes.Length - 1
                    Dim b = Me.parSubGroupS(i)
                    ssError += (Me.parRes(i) - bMeans(b)) ^ 2
                Next

                ' Compute MS_Error
                Dim dfError As Integer = Me.parRes.Length - Me.parSubGroupID.Length
                Dim msError As Double = ssError / dfError

                ' Compute F = MS_BA / MS_Error
                Dim F As Double = msBA / msError

                ' Build crosstab dictionary: (group, subgroup) → count
                Dim crosstab = New Dictionary(Of String, Dictionary(Of String, Integer))()
                For i = 0 To Me.parGroupS.Length - 1
                    Dim g = Me.parGroupS(i)
                    Dim s = Me.parSubGroupS(i)
                    If Not crosstab.ContainsKey(g) Then crosstab(g) = New Dictionary(Of String, Integer)()
                    If Not crosstab(g).ContainsKey(s) Then crosstab(g)(s) = 0
                    crosstab(g)(s) += 1
                Next
                Dim outFrq2D(crosstab.Sum(Function(g) g.Value.Count) - 1) As Integer
                Dim ii As Integer = 0
                For Each g In crosstab.Keys.OrderBy(Function(k) k)
                    For Each s In crosstab(g).Keys.OrderBy(Function(k) k)
                        outFrq2D(ii) = crosstab(g)(s)
                        ii += 1
                    Next
                Next

                If outFrq2D.GetLength(0) <> parSubGroupID.GetLength(0) Then
                    AppGlobals.BSerr.LogAndThrow(New ApplicationException("Error: Factor is not nested. The same 'nested' factor category occured in multiple group factor categories."))
                End If

                'test if balanced design
                If outFrq2D.Min() = outFrq2D.Max() And Me.parGroupFrq.Min() = Me.parGroupFrq.Max() And Me.parSubGroupFrq.Min() = Me.parSubGroupFrq.Max() Then
                    Me.pbBalanced = True
                End If

                Dim SSsubwgroup As Double = SStot - SSgroup - ssError
                Dim DFgroup As Integer = UBound(Me.parGroupID, 1)
                Dim DFtot As Integer = Me.parSubGroupFrq.Sum() - 1
                Dim MSgroup As Double = SSgroup / DFgroup

                'sum of squares
                pANOVAtable(0, 0) = SSgroup : pANOVAtable(1, 0) = SSsubwgroup : pANOVAtable(2, 0) = ssError : pANOVAtable(3, 0) = SStot
                pANOVAtabSW(0, 0) = SSgroup : pANOVAtabSW(1, 0) = SSsubwgroup : pANOVAtabSW(2, 0) = ssError : pANOVAtabSW(3, 0) = SStot
                'DF
                pANOVAtable(0, 1) = DFgroup : pANOVAtable(1, 1) = dfBA : pANOVAtable(2, 1) = dfError : pANOVAtable(3, 1) = DFtot
                pANOVAtabSW(0, 1) = DFgroup : pANOVAtabSW(1, 1) = dfBA : pANOVAtabSW(2, 1) = dfError : pANOVAtabSW(3, 1) = DFtot
                'mean squares
                pANOVAtable(0, 2) = MSgroup : pANOVAtable(1, 2) = msBA : pANOVAtable(2, 2) = msError
                pANOVAtabSW(0, 2) = MSgroup : pANOVAtabSW(1, 2) = msBA : pANOVAtabSW(2, 2) = msError
                'F statistics
                pANOVAtable(0, 3) = MSgroup / msBA : pANOVAtable(1, 3) = F
                pANOVAtabSW(0, 3) = -99 : pANOVAtabSW(1, 3) = F
                'p-values
                pANOVAtable(0, 4) = distributions.F_RT(pANOVAtable(0, 3), DFgroup, dfBA) : pANOVAtable(1, 4) = distributions.F_RT(F, dfBA, dfError)
                pANOVAtabSW(0, 4) = -9 : pANOVAtabSW(1, 4) = distributions.F_RT(F, dfBA, dfError)

                'Satterthwaite approximation compute only when DF subgroups within groups dfBA <= 100 and DFsubwgroup < 2*dfError (within subgroup)
                'D. W. Gaylor and F. N. Hopper, Estimating the Degrees of Freedom for Linear Combinations of Mean Squares by Satterthwaite's Formula. Technometrics, Vol. 11, No. 4 (Nov., 1969), pp. 691-706
                For Each g In crosstab.Keys.OrderBy(Function(k) k)
                    Dim tmp2 As Double = 0
                    tmp = 0
                    For Each s In crosstab(g).Keys.OrderBy(Function(k) k)
                        'Debug.Print($"Group {g}, Subgroup {s}: {crosstab(g)(s)}")
                        tmp += crosstab(g)(s) * crosstab(g)(s)
                        tmp2 += crosstab(g)(s)
                    Next
                    n0 += (tmp / tmp2)
                Next


                n0p = (n0 - (SumSq(Me.parSubGroupFrq) / Me.parGroupFrq.Sum())) / DFgroup
                n0 = (Me.parGroupFrq.Sum() - n0) / dfBA
                If n0p <> n0 Then
                    r = (n0p / (n0p - n0)) * (F)
                    c = distributions.F_Inv(0.025, dfError, dfBA) * distributions.F_Inv(0.5, dfError, dfBA)
                End If


                If dfBA <= 100 And dfBA < 2 * dfError Then
                    If r > c Then 'it's safe to use approximation
                        w2 = n0p / n0
                        w1 = 1.0 - w2

                        Dim SW_MS As Double = w1 * msError + w2 * msBA
                        Dim SW_DF As Double = (SW_MS * SW_MS) / (((w1 * msError) ^ 2 / dfError) + ((w2 * msBA) ^ 2 / dfBA))
                        Dim SW_F As Double = MSgroup / SW_MS

                        pANOVAtabSW(1, 1) = SW_DF
                        pANOVAtabSW(1, 2) = SW_MS
                        pANOVAtabSW(0, 3) = SW_F

                        'Excel Fdist truncates non-integer DF values so approximate F distribution by beta distribution.
                        pANOVAtabSW(0, 4) = 1.0 - distributions.F_CDF(pANOVAtabSW(0, 3), pANOVAtabSW(0, 1), pANOVAtabSW(1, 1))
                    End If
                End If

                'variance components
                var_raw_group = (MSgroup - msError - n0p * msBA) / ((Me.parGroupFrq.Sum() - SumSq(Me.parGroupFrq) / Me.parGroupFrq.Sum()) / DFgroup)
                var_raw_subwgroup = (msBA - msError) / n0
                var_raw_subgroup = msError
                If var_raw_group <= 0.0 Then var_raw_group = 0.0
                If var_raw_subwgroup <= 0.0 Then var_raw_subwgroup = 0.0
                If var_raw_subgroup <= 0.0 Then var_raw_subgroup = 0.0
                var_tot = var_raw_group + var_raw_subwgroup + var_raw_subgroup

                pANOVAtable(0, 5) = Math.Round(100 * var_raw_group / var_tot, 2)
                pANOVAtable(1, 5) = Math.Round(100 * var_raw_subwgroup / var_tot, 2)
                pANOVAtable(2, 5) = Math.Round(100 * var_raw_subgroup / var_tot, 2)
                pANOVAtable(3, 5) = 100

                Return pANOVAtable
            End Function

        End Class


        ''' <summary>
        ''' Implements a classical one‑way ANOVA model for comparing means across
        ''' multiple independent groups:
        ''' 
        '''     Y_ij = μ + A_i + ε_ij
        ''' 
        ''' where A_i is the fixed effect of group i. The class computes:
        ''' <list type="bullet">
        '''   <item><description>Between‑group and within‑group sums of squares</description></item>
        '''   <item><description>Degrees of freedom, mean squares, F‑statistic, p‑value</description></item>
        '''   <item><description>Welch’s heteroscedastic ANOVA (optional)</description></item>
        '''   <item><description>Multiple‑comparison procedures:
        '''       <list type="bullet">
        '''         <item><description>Fisher’s LSD</description></item>
        '''         <item><description>Bonferroni‑adjusted LSD</description></item>
        '''         <item><description>Tukey–Kramer</description></item>
        '''         <item><description>Games–Howell</description></item>
        '''       </list>
        '''     </description></item>
        ''' </list>
        ''' 
        ''' The class outputs formatted <c>ResultTable</c> objects suitable for reporting.
        ''' </summary>
        Public Class OneWayANOVA
            Private data()() As Double
            Private varNames() As String
            Private pNs() As Integer
            Private pNoGroups As Integer
            Private ANOVAtable(,) As Object
            Private WANOVA As TestResult = Nothing
            Private MCP_LSD(,) As Object = Nothing
            Private MCP_Bonferroni(,) As Object = Nothing
            Private MCP_Tukey(,) As Object = Nothing
            Private MCP_GamesHowell(,) As Object = Nothing
            Private MCP_LSD_Alpha As Double = 0.05
            Private MCP_Bonferroni_Alpha As Double = 0.05
            Private MCP_Tukey_Alpha As Double = 0.05
            Private MCP_GamesHowell_Alpha As Double = 0.05

            ''' <summary>
            ''' Initializes the one‑way ANOVA model with grouped numeric data.
            ''' </summary>
            ''' <param name="x">
            ''' A jagged array where each element <c>x(i)</c> contains all observations
            ''' from group i.
            ''' </param>
            ''' <param name="varNames">Names of the groups for reporting.</param>
            Public Sub New(x()() As Double, varNames() As String)
                Me.data = x
                Me.varNames = varNames
                If x.GetLength(0) <> varNames.Length Then AppGlobals.BSerr.LogAndThrow(New ArgumentException("Number of groups and variable names should be the same."))
                If x.GetLength(0) < 2 Then AppGlobals.BSerr.LogAndThrow(New ArgumentException("At least two groups are expected."))

                pNoGroups = x.Length
                ReDim pNs(pNoGroups - 1)
                For i = 0 To pNoGroups - 1
                    pNs(i) = x(i).Length
                Next
            End Sub

            ''' <summary>
            ''' Wraps the ANOVA results and all available multiple‑comparison procedures
            ''' into a list of <c>ResultTable</c> objects. 
            ''' </summary>
            ''' <remarks>
            ''' <para>
            ''' If Welch’s ANOVA has been computed, the main ANOVA table is expanded to
            ''' include Welch’s adjusted degrees of freedom, F‑statistic, and p‑value.
            ''' </para>
            ''' <para>
            ''' Additional tables are included only if their corresponding MCP matrices
            ''' (<c>MCP_LSD</c>, <c>MCP_Bonferroni</c>, <c>MCP_Tukey</c>,
            ''' <c>MCP_GamesHowell</c>) have been computed. MCP tables that report confidence intervals also include a footnote
            ''' indicating the confidence interval level derived from the alpha used
            ''' when that MCP table was created.
            ''' </para>
            ''' </remarks>
            ''' <returns>A list of formatted <c>ResultTable</c> objects.</returns>
            Public Function wrapResults() As List(Of ResultTable)
                Dim out = New List(Of ResultTable)
                Dim t = New ResultTable
                Dim anTable = New ResultTable

                If Me.WANOVA Is Nothing Then
                    anTable.SetBody(Me.ANOVAtable)
                    anTable.AddHeaderLeftRow({"Between Groups", "Within Groups", "Total"})
                    anTable.AddHeaderTopRow({"Source of Variation", "SS", "df", "MS", "F", "P-value"})
                Else
                    anTable.SetBody(Matrix.VerticalStackArrays(Me.ANOVAtable,
                                                        {{Me.WANOVA.DF1, Me.WANOVA.TestStatistics1, Me.WANOVA.Pvalue},
                                                         {"", "", ""},
                                                         {"", "", ""}}))
                    anTable.AddHeaderLeftRow({"Between Groups", "Within Groups", "Total"})
                    anTable.AddHeaderTopRow({"Source of Variation", "SS", "df", "MS", "F", "P-value", "Welch DF Error", "Welch F", "Welch P-value"})
                End If
                out.Add(anTable)

                If MCP_LSD IsNot Nothing Then
                    t = New ResultTable
                    t.SetBody(Me.MCP_LSD)
                    t.AddHeaderTopRow({"Fisher's LSD multiple comparisons", "Mean difference (CI)", "t", "P-value"})
                    t.AddFootnote(BuildMcpCiFootnote(Me.MCP_LSD_Alpha))
                    out.Add(t)
                End If

                If MCP_Bonferroni IsNot Nothing Then
                    t = New ResultTable
                    t.SetBody(Me.MCP_Bonferroni)
                    t.AddHeaderTopRow({"Bonferroni adjusted multiple comparisons", "Mean difference (CI)", "t", "P-value"})
                    t.AddFootnote(BuildMcpCiFootnote(Me.MCP_Bonferroni_Alpha) & " Bonferroni-adjusted critical values were used.")
                    out.Add(t)
                End If

                If MCP_Tukey IsNot Nothing Then
                    t = New ResultTable
                    t.SetBody(Me.MCP_Tukey)
                    t.AddHeaderTopRow({"Tukey-Kramer multiple comparisons", "Mean difference (CI)", "q", "P-value"})
                    t.AddFootnote(BuildMcpCiFootnote(Me.MCP_Tukey_Alpha))
                    out.Add(t)
                End If

                If MCP_GamesHowell IsNot Nothing Then
                    t = New ResultTable
                    t.SetBody(Me.MCP_GamesHowell)
                    t.AddHeaderTopRow({"Games-Howell multiple comparisons", "Mean difference (CI)", "q", "DF", "P-value"})
                    t.AddFootnote(BuildMcpCiFootnote(Me.MCP_GamesHowell_Alpha))
                    out.Add(t)
                End If

                Return out
            End Function

            ''' <summary>
            ''' Performs the classical one‑way ANOVA computation, including:
            ''' <list type="bullet">
            '''   <item><description>Total sum of squares</description></item>
            '''   <item><description>Between‑group sum of squares</description></item>
            '''   <item><description>Within‑group (error) sum of squares</description></item>
            '''   <item><description>Degrees of freedom and mean squares</description></item>
            '''   <item><description>F‑statistic and p‑value</description></item>
            ''' </list>
            ''' </summary>
            ''' <returns>
            ''' A 2D Object array representing the ANOVA table:
            ''' <c>{SS, df, MS, F, p‑value}</c>.
            ''' </returns>
            Public Function compute() As Object(,)
                Dim out(2, 4) As Object
                Dim SSb As Double
                Dim n As Integer = pNs.Sum()
                Dim arData(n - 1) As Double

                'rewrite data into 1D array and compute MSerr
                Dim ii As Integer = 0
                For i = 0 To pNoGroups - 1
                    Dim arTemp() As Double = data(i)
                    For j = 0 To pNs(i) - 1
                        arData(ii) = data(i)(j)
                        ii += 1
                    Next
                    SSb += (arTemp.Sum() ^ 2) / pNs(i)
                Next

                'Total information MS-mean squares, SS - sum of squares, DF - # degrees-of-freedom
                Dim MStot As Double = variance(arData)
                Dim DFtot As Integer = n - 1
                Dim SStot As Double = MStot * DFtot

                'Between groups information
                SSb -= (arData.Sum() ^ 2) / n
                Dim DFb As Integer = pNoGroups - 1
                Dim MSb As Double = SSb / DFb

                'Error (within group) information
                Dim SSerr As Double = SStot - SSb
                Dim DFerr As Integer = n - pNoGroups
                Dim MSerr As Double = SSerr / DFerr

                'Test statistic and P-value
                Dim F As Double = MSb / MSerr
                Dim Pvalue As Double = distributions.F_RT(F, CDbl(DFb), CDbl(DFerr))

                'output
                out(0, 0) = SSb : out(0, 1) = DFb : out(0, 2) = MSb : out(0, 3) = F : out(0, 4) = Pvalue 'Between groups
                out(1, 0) = SSerr : out(1, 1) = DFerr : out(1, 2) = MSerr 'Within groups
                out(2, 0) = SStot : out(2, 1) = DFtot 'Total
                ANOVAtable = out
                Return out
            End Function

            ''' <summary>
            ''' Computes Welch’s heteroscedastic one‑way ANOVA, which does not assume
            ''' equal variances across groups. This method estimates:
            ''' <list type="bullet">
            '''   <item><description>Group means and variances</description></item>
            '''   <item><description>Welch weights</description></item>
            '''   <item><description>Adjusted degrees of freedom</description></item>
            '''   <item><description>Welch F‑statistic</description></item>
            '''   <item><description>Corresponding p‑value</description></item>
            ''' </list>
            ''' </summary>
            ''' <returns>
            ''' A <c>TestResult</c> object containing Welch’s F‑statistic, degrees of
            ''' freedom, and p‑value.
            ''' </returns>
            Public Function WelshANOVA() As TestResult
                Dim arMean(pNoGroups - 1) As Double, arVariance(pNoGroups - 1) As Double, arWeight(pNoGroups - 1) As Double
                Dim grandMean As Double, b As Double, Num As Double
                Dim n As Integer = pNs.Sum()
                Dim arData(n - 1) As Double

                'rewrite data into 1D array and compute Means and Variances for each group
                Dim ii As Integer = 0
                For i = 0 To pNoGroups - 1
                    Dim arTemp() As Double = data(i)
                    For j = 0 To pNs(i) - 1
                        arData(ii) = data(i)(j)
                        ii += 1
                    Next
                    If pNs(i) > 1 Then arVariance(i) = variance(arTemp)
                    arMean(i) = arTemp.Average()
                    If arVariance(i) <> 0 Then arWeight(i) = pNs(i) / arVariance(i)
                    grandMean += arMean(i) * arWeight(i)
                Next
                grandMean = grandMean / arWeight.Sum()

                For i = 0 To pNoGroups - 1
                    Num += (arWeight(i) * ((arMean(i) - grandMean) ^ 2))
                    b += (1.0 / (pNs(i) - 1)) * (1.0 - (arWeight(i) / arWeight.Sum())) ^ 2
                Next
                Dim DFb As Integer = pNoGroups - 1
                Dim DFerr As Double = ((pNoGroups * pNoGroups) - 1) / (3.0 * b)
                Dim MSerr As Double = Num / DFb
                Dim a As Double = (2 * (pNoGroups - 2)) / ((pNoGroups * pNoGroups) - 1)

                'Test statistic and P-value
                Dim F As Double = MSerr / (1.0 + a * b)
                Dim Pvalue As Double = 1.0 - distributions.F_CDF(F, DFb, DFerr)

                'Prepare outputs
                WANOVA = New TestResult
                WANOVA.DF1 = DFerr
                WANOVA.TestStatistics1 = F
                WANOVA.Pvalue = Pvalue
                Return WANOVA
            End Function

            ''' <summary>
            ''' Performs Fisher’s Least Significant Difference (LSD) post-hoc test for
            ''' all pairwise group comparisons. Optionally applies a Bonferroni
            ''' adjustment to the pairwise p-values and to the critical value used for
            ''' the reported confidence intervals.
            ''' </summary>
            ''' <param name="bBonferroni">
            ''' If True, applies Bonferroni-adjusted p-values and Bonferroni-adjusted
            ''' critical values for the reported confidence intervals.
            ''' </param>
            ''' <param name="alpha">
            ''' Optional two-sided significance level used to construct the reported confidence intervals.
            ''' The default is <c>0.05</c>, corresponding to a 95% confidence interval.
            ''' </param>
            ''' <returns>
            ''' A 2D Object array containing, in natural pair-generation order:
            ''' <list type="bullet">
            '''   <item><description>Group comparison label</description></item>
            '''   <item><description>Mean difference with confidence interval</description></item>
            '''   <item><description>t-statistic</description></item>
            '''   <item><description>Adjusted or unadjusted p-value</description></item>
            ''' </list>
            ''' </returns>
            Public Function FisherLSD(Optional bBonferroni As Boolean = False,
                                      Optional alpha As Double = 0.05) As Object(,)
                ' Fisher's least significant difference (LSD) post hoc test after 1-way ANOVA
                ' or Bonferroni correction if bBonferroni = True
                ValidateAlpha(alpha)
                Dim out(,) As Object
                Dim nContrasts As Integer = (pNoGroups * (pNoGroups - 1)) / 2
                ReDim out(nContrasts - 1, 3)

                Dim arMean(pNoGroups - 1) As Double
                Dim arDiffs(nContrasts - 1, 7) As Object
                Dim DFerr As Double = CDbl(Me.ANOVAtable(1, 1))
                Dim MSerr As Double = CDbl(Me.ANOVAtable(1, 2))

                For i = 0 To pNoGroups - 1
                    arMean(i) = data(i).Average()
                Next

                Dim ii As Integer = 0
                For i = 0 To pNoGroups - 1
                    For j = i + 1 To pNoGroups - 1
                        Dim diff As Double = arMean(i) - arMean(j)
                        Dim se As Double = Math.Sqrt(MSerr * (1.0 / pNs(i) + 1.0 / pNs(j)))
                        Dim tStat As Double = diff / se
                        Dim rawP As Double = distributions.T_2T(Math.Abs(tStat), DFerr)
                        Dim pVal As Double
                        Dim tCrit As Double

                        If bBonferroni Then
                            pVal = rawP * nContrasts
                            If pVal > 1.0 Then pVal = 1.0
                            tCrit = distributions.T_Inv_2T(alpha / nContrasts, DFerr)
                        Else
                            pVal = rawP
                            tCrit = distributions.T_Inv_2T(alpha, DFerr)
                        End If

                        arDiffs(ii, 0) = se
                        arDiffs(ii, 1) = diff
                        arDiffs(ii, 2) = tStat
                        arDiffs(ii, 3) = i
                        arDiffs(ii, 4) = j
                        arDiffs(ii, 5) = pVal
                        arDiffs(ii, 6) = diff - tCrit * se
                        arDiffs(ii, 7) = diff + tCrit * se
                        ii += 1
                    Next
                Next

                For i = 0 To ii - 1
                    out(i, 0) = varNames(CInt(arDiffs(i, 3))) & " vs. " & varNames(CInt(arDiffs(i, 4)))
                    out(i, 1) = CSng(arDiffs(i, 1)) & " (" & CSng(arDiffs(i, 6)) & " to " & CSng(arDiffs(i, 7)) & ")"
                    out(i, 2) = CStr(CSng(arDiffs(i, 2)))
                    out(i, 3) = CStr(CSng(arDiffs(i, 5)))
                Next

                If bBonferroni Then
                    Me.MCP_Bonferroni = out
                    Me.MCP_Bonferroni_Alpha = alpha
                Else
                    Me.MCP_LSD = out
                    Me.MCP_LSD_Alpha = alpha
                End If

                Return out
            End Function

            ''' <summary>
            ''' Performs the Tukey–Kramer multiple-comparison test for unequal sample
            ''' sizes.
            ''' </summary>
            ''' <param name="alpha">
            ''' Optional two-sided significance level used to construct the reported confidence intervals.
            ''' The default is <c>0.05</c>, corresponding to a 95% confidence interval.
            ''' </param>
            ''' <returns>
            ''' A 2D Object array containing comparison labels, mean differences with
            ''' confidence intervals, Q-statistics, and p-values.
            ''' </returns>
            ''' <remarks>
            ''' Pairwise comparisons are returned in natural pair-generation order and
            ''' are not sorted by effect size or p-value.
            ''' </remarks>
            Public Function TukeyKramer(Optional alpha As Double = 0.05) As Object(,)
                ' Tukey-Kramer post hoc test after 1-way ANOVA
                ValidateAlpha(alpha)
                Dim out(,) As Object
                Dim iFault As Integer = 0
                Dim nContrasts As Integer = (pNoGroups * (pNoGroups - 1)) / 2
                ReDim out(nContrasts - 1, 3)

                Dim arMean(pNoGroups - 1) As Double
                Dim arDiffs(nContrasts - 1, 7) As Object
                Dim MSerr As Double = CDbl(Me.ANOVAtable(1, 2))

                For i = 0 To pNoGroups - 1
                    arMean(i) = data(i).Average()
                Next

                Dim df As Integer = pNs.Sum() - pNoGroups
                Dim Qcrit As Double = distributions.QTRNG(1.0 - alpha, CDbl(df), CDbl(pNoGroups), iFault)

                Dim ii As Integer = 0
                For i = 0 To pNoGroups - 1
                    For j = i + 1 To pNoGroups - 1
                        Dim diff As Double = arMean(i) - arMean(j)
                        Dim qStat As Double = Math.Abs(diff) / Math.Sqrt(0.5 * MSerr * (1.0 / pNs(i) + 1.0 / pNs(j)))
                        Dim pVal As Double = 1.0 - distributions.PRTRNG(qStat, CDbl(df), CDbl(pNoGroups), iFault)
                        Dim margin As Double = (Qcrit / Math.Sqrt(2.0)) * Math.Sqrt(MSerr) * Math.Sqrt(1.0 / pNs(i) + 1.0 / pNs(j))

                        arDiffs(ii, 1) = diff
                        arDiffs(ii, 2) = qStat
                        arDiffs(ii, 3) = i
                        arDiffs(ii, 4) = j
                        arDiffs(ii, 5) = pVal
                        arDiffs(ii, 6) = diff - margin
                        arDiffs(ii, 7) = diff + margin
                        ii += 1
                    Next
                Next

                For i = 0 To ii - 1
                    out(i, 0) = varNames(CInt(arDiffs(i, 3))) & " vs. " & varNames(CInt(arDiffs(i, 4)))
                    out(i, 1) = CSng(arDiffs(i, 1)) & " (" & CSng(arDiffs(i, 6)) & " to " & CSng(arDiffs(i, 7)) & ")"
                    out(i, 2) = CStr(CSng(arDiffs(i, 2)))
                    out(i, 3) = CStr(CSng(arDiffs(i, 5)))
                Next

                Me.MCP_Tukey = out
                Me.MCP_Tukey_Alpha = alpha
                Return out
            End Function

            ''' <summary>
            ''' Performs the Games–Howell post-hoc test, which is robust to unequal
            ''' variances and unequal sample sizes.
            ''' </summary>
            ''' <param name="alpha">
            ''' Optional two-sided significance level used to construct the reported confidence intervals.
            ''' The default is <c>0.05</c>, corresponding to a 95% confidence interval.
            ''' </param>
            ''' <returns>
            ''' A 2D Object array containing comparison labels, mean differences with
            ''' confidence intervals, Q-statistics, degrees of freedom, and p-values.
            ''' </returns>
            ''' <remarks>
            ''' Pairwise comparisons are returned in natural pair-generation order and
            ''' are not sorted by effect size or p-value.
            ''' </remarks>
            Public Function GamesHowell(Optional alpha As Double = 0.05) As Object(,)
                ' Games-Howell post hoc test after 1-way ANOVA
                ValidateAlpha(alpha)
                Dim out(,) As Object
                Dim iFault As Integer = 0
                Dim arTemp() As Double
                Dim nContrasts As Integer = (pNoGroups * (pNoGroups - 1)) / 2
                ReDim out(nContrasts - 1, 4)

                Dim arMean(pNoGroups - 1) As Double
                Dim arVars(pNoGroups - 1) As Double
                Dim arDiffs(nContrasts - 1, 8) As Object

                For i = 0 To pNoGroups - 1
                    arTemp = data(i)
                    arMean(i) = arTemp.Average()
                    arVars(i) = variance(arTemp)
                Next

                Dim ii As Integer = 0
                For i = 0 To pNoGroups - 1
                    Dim VarNi As Double = arVars(i) / pNs(i)

                    For j = i + 1 To pNoGroups - 1
                        Dim VarNj As Double = arVars(j) / pNs(j)
                        Dim diff As Double = arMean(i) - arMean(j)
                        Dim se As Double = Math.Sqrt(0.5 * (VarNi + VarNj))
                        Dim qStat As Double = Math.Abs(diff) / se
                        Dim df As Double = ((VarNi + VarNj) ^ 2) / (((VarNi ^ 2) / (pNs(i) - 1)) + ((VarNj ^ 2) / (pNs(j) - 1)))
                        Dim pVal As Double = 1.0 - distributions.PRTRNG(qStat, df, CDbl(pNoGroups), iFault)
                        Dim qCrit As Double = distributions.QTRNG(1.0 - alpha, df, CDbl(pNoGroups), iFault)
                        Dim margin As Double = qCrit * se

                        arDiffs(ii, 1) = diff
                        arDiffs(ii, 2) = qStat
                        arDiffs(ii, 3) = i
                        arDiffs(ii, 4) = j
                        arDiffs(ii, 5) = pVal
                        arDiffs(ii, 6) = diff - margin
                        arDiffs(ii, 7) = diff + margin
                        arDiffs(ii, 8) = df
                        ii += 1
                    Next
                Next

                For i = 0 To ii - 1
                    out(i, 0) = varNames(CInt(arDiffs(i, 3))) & " vs. " & varNames(CInt(arDiffs(i, 4)))
                    out(i, 1) = CSng(arDiffs(i, 1)) & " (" & CSng(arDiffs(i, 6)) & " to " & CSng(arDiffs(i, 7)) & ")"
                    out(i, 2) = CStr(CSng(arDiffs(i, 2)))
                    out(i, 3) = CStr(CSng(arDiffs(i, 8)))
                    out(i, 4) = CStr(CSng(arDiffs(i, 5)))
                Next

                Me.MCP_GamesHowell = out
                Me.MCP_GamesHowell_Alpha = alpha
                Return out
            End Function
        End Class


        ''' <summary>
        ''' Implements a one‑way repeated‑measures ANOVA (RM‑ANOVA) model of the form:
        ''' 
        '''     Y_ij = μ + A_j + S_i + ε_ij
        ''' 
        ''' where:
        ''' <list type="bullet">
        '''   <item><description>A_j is the fixed effect of treatment j (columns)</description></item>
        '''   <item><description>S_i is the random effect of subject/block i (rows)</description></item>
        '''   <item><description>ε_ij is the residual error</description></item>
        ''' </list>
        ''' 
        ''' The class computes:
        ''' <list type="bullet">
        '''   <item><description>Between‑treatments SS, df, MS, F, p‑value</description></item>
        '''   <item><description>Between‑subjects SS, df, MS, F, p‑value</description></item>
        '''   <item><description>Residual SS, df, MS</description></item>
        '''   <item><description>Greenhouse–Geisser and Huynh–Feldt sphericity corrections</description></item>
        '''   <item><description>Tukey–Kramer RM post‑hoc tests (with and without sphericity)</description></item>
        ''' </list>
        ''' 
        ''' Results are returned as formatted <c>ResultTable</c> objects.
        ''' </summary>
        Public Class OneWayRmANOVA

            Private data(,) As Double
            Private varNames() As String
            Private NoGroups As Integer
            Private NoBlocks As Integer
            Private ANOVAtable(,) As Object
            Private HuyhnFeldtTest As TestResult = Nothing
            Private GreenhouseGeisserTest As TestResult = Nothing
            Private TuekyRM2(,) As Object = Nothing
            Private TukeyOut(,) As Object = Nothing
            Private TuekyRM2Alpha As Double = 0.05
            Private TukeyOutAlpha As Double = 0.05

            ''' <summary>
            ''' Initializes the repeated‑measures ANOVA model.
            ''' </summary>
            ''' <param name="x">
            ''' A 2D matrix where:
            ''' <list type="bullet">
            '''   <item><description>Rows = subjects/blocks</description></item>
            '''   <item><description>Columns = repeated‑measure conditions</description></item>
            ''' </list>
            ''' </param>
            ''' <param name="strNames">Names of the repeated‑measure conditions.</param>
            Public Sub New(x(,) As Double, strNames() As String)
                Me.data = x
                Me.varNames = strNames
                Me.NoBlocks = data.GetLength(0) ' number of blocks (rows)
                Me.NoGroups = data.GetLength(1) ' number of treatments (columns)
            End Sub

            ''' <summary>
            ''' Wraps the RM‑ANOVA results and all available sphericity‑corrected
            ''' statistics and post‑hoc tests into a list of <c>ResultTable</c> objects.
            ''' MCP tables that report confidence intervals also include a footnote
            ''' indicating the confidence interval level derived from the alpha used
            ''' when that MCP table was created.
            ''' </summary>
            ''' <remarks>
            ''' <para>
            ''' Depending on which corrections were computed (Greenhouse–Geisser,
            ''' Huynh–Feldt, or both), the ANOVA table is expanded to include:
            ''' <list type="bullet">
            '''   <item><description>Epsilon estimates</description></item>
            '''   <item><description>Corrected p‑values</description></item>
            ''' </list>
            ''' </para>
            ''' 
            ''' <para>
            ''' Post‑hoc tables are included only if their corresponding matrices
            ''' (<c>TuekyRM2</c>, <c>TukeyOut</c>) have been computed.
            ''' </para>
            ''' </remarks>
            ''' <returns>A list of formatted <c>ResultTable</c> objects.</returns>
            Public Function wrapResults() As List(Of ResultTable)
                Dim out = New List(Of ResultTable)
                Dim t = New ResultTable
                Dim anTable = New ResultTable

                If Me.HuyhnFeldtTest Is Nothing And Me.GreenhouseGeisserTest Is Nothing Then
                    anTable.SetBody(Me.ANOVAtable)
                    anTable.AddHeaderLeftRow({"Between Groups(columns)", "Between Subjects(rows)", "Residual(error)", "Total"})
                    anTable.AddHeaderTopRow({"Source of Variation", "SS", "df", "MS", "F", "P-value"})
                ElseIf Me.HuyhnFeldtTest IsNot Nothing And Me.GreenhouseGeisserTest IsNot Nothing Then
                    anTable.SetBody(Matrix.VerticalStackArrays(Me.ANOVAtable,
                                                        {{Me.GreenhouseGeisserTest.TestStatistics1, Me.GreenhouseGeisserTest.Pvalue, Me.HuyhnFeldtTest.TestStatistics1, Me.HuyhnFeldtTest.Pvalue},
                                                         {"", "", "", ""}, {"", "", "", ""}, {"", "", "", ""}}))
                    anTable.AddHeaderLeftRow({"Between Groups(columns)", "Between Subjects(rows)", "Residual(error)", "Total"})
                    anTable.AddHeaderTopRow({"Source of Variation", "SS", "df", "MS", "F", "P-value", "Epsilon Greenhouse - Geisser", "P-value GG", "Epsilon Huyhn-Feldt", "P-value HF"})
                ElseIf Me.HuyhnFeldtTest IsNot Nothing Then
                    anTable.SetBody(Matrix.VerticalStackArrays(Me.ANOVAtable,
                                                        {{Me.HuyhnFeldtTest.TestStatistics1, Me.HuyhnFeldtTest.Pvalue},
                                                         {"", ""}, {"", ""}, {"", ""}}))
                    anTable.AddHeaderLeftRow({"Between Groups(columns)", "Between Subjects(rows)", "Residual(error)", "Total"})
                    anTable.AddHeaderTopRow({"Source of Variation", "SS", "df", "MS", "F", "P-value", "Epsilon Huyhn-Feldt", "P-value HF"})
                ElseIf Me.GreenhouseGeisserTest IsNot Nothing Then
                    anTable.SetBody(Matrix.VerticalStackArrays(Me.ANOVAtable,
                                                        {{Me.GreenhouseGeisserTest.TestStatistics1, Me.GreenhouseGeisserTest.Pvalue},
                                                         {"", ""}, {"", ""}, {"", ""}}))
                    anTable.AddHeaderLeftRow({"Between Groups(columns)", "Between Subjects(rows)", "Residual(error)", "Total"})
                    anTable.AddHeaderTopRow({"Source of Variation", "SS", "df", "MS", "F", "P-value", "Epsilon Greenhouse - Geisser", "P-value GG"})
                End If
                out.Add(anTable)

                If TuekyRM2 IsNot Nothing Then
                    t = New ResultTable
                    t.SetBody(Me.TuekyRM2)
                    t.AddHeaderTopRow({"Tukey-Kramer multiple comparisons not assuming sphericity. Recommended", "", "", ""})
                    t.AddHeaderTopRow({"Comparison", "Mean difference (CI)", "q", "P-value"})
                    t.AddFootnote(BuildMcpCiFootnote(Me.TuekyRM2Alpha))
                    out.Add(t)
                End If

                If TukeyOut IsNot Nothing Then
                    t = New ResultTable
                    t.SetBody(Me.TukeyOut)
                    t.AddHeaderTopRow({"Tukey-Kramer multiple comparisons assuming sphericity (using single pooled variance)", "", "", ""})
                    t.AddHeaderTopRow({"Comparison", "Mean difference (CI)", "q", "P-value"})
                    t.AddFootnote(BuildMcpCiFootnote(Me.TukeyOutAlpha))
                    out.Add(t)
                End If

                Return out
            End Function

            ''' <summary>
            ''' Performs the classical one‑way repeated‑measures ANOVA, computing:
            ''' <list type="bullet">
            '''   <item><description>Total sum of squares</description></item>
            '''   <item><description>Between‑treatments SS (columns)</description></item>
            '''   <item><description>Between‑subjects SS (rows)</description></item>
            '''   <item><description>Residual SS</description></item>
            '''   <item><description>Degrees of freedom and mean squares</description></item>
            '''   <item><description>F‑tests and p‑values for treatments and subjects</description></item>
            ''' </list>
            ''' </summary>
            ''' <returns>
            ''' A 2D Object array representing the ANOVA table:
            ''' <c>{SS, df, MS, F, p‑value}</c>.
            ''' </returns>
            Public Function compute() As Object(,)

                Dim SStot As Double, SSb As Double, SSsub As Double
                Dim Fb As Double, Fsub As Double
                Dim arSMeans() As Double, arGMeans() As Double, arTemp() As Double

                ReDim arSMeans(NoBlocks - 1), arGMeans(NoGroups - 1)
                ReDim Me.ANOVAtable(3, 4)

                Dim n As Integer = data.Length()
                Dim DFtot As Integer = n - 1
                Dim DFb As Integer = NoGroups - 1
                Dim DFsub As Integer = NoBlocks - 1
                Dim DFerr As Integer = DFtot - DFb - DFsub
                Dim MeanTot As Double = data.Average2D()

                'compute groups means, between groups sum-of-squares, and totoal sum-of-sqares
                For i = 0 To NoGroups - 1
                    arTemp = Matrix.GetColumnFrom2Darray(data, i)
                    For j = 0 To NoBlocks - 1
                        SStot += (arTemp(j) - MeanTot) ^ 2
                    Next
                    arGMeans(i) = arTemp.Average()
                    SSb += (arGMeans(i) - MeanTot) ^ 2
                Next
                SSb = SSb * NoBlocks

                'compute subject means and sum-of-squares
                For j = 0 To NoBlocks - 1
                    arTemp = Matrix.rowFromArray(data, j)
                    arSMeans(j) = arTemp.Average()
                    SSsub += (arSMeans(j) - MeanTot) ^ 2
                Next
                SSsub = SSsub * NoGroups
                Dim SSerr As Double = SStot - SSb - SSsub

                'compute mean squares
                Dim MSb As Double = SSb / DFb
                Dim MSsub As Double = SSsub / DFsub
                Dim MSerr As Double = SSerr / DFerr

                'Test statistic and P-value
                If MSerr > 0 Then Fb = MSb / MSerr
                If MSerr > 0 Then Fsub = MSsub / MSerr

                Dim Pb As Double = distributions.F_RT(Fb, CDbl(DFb), CDbl(DFerr))
                Dim Psub As Double = distributions.F_RT(Fsub, CDbl(DFsub), CDbl(DFerr))

                'output
                ANOVAtable(0, 0) = SSb : ANOVAtable(0, 1) = DFb : ANOVAtable(0, 2) = MSb : ANOVAtable(0, 3) = Fb : ANOVAtable(0, 4) = Pb 'Between groups
                ANOVAtable(1, 0) = SSsub : ANOVAtable(1, 1) = DFsub : ANOVAtable(1, 2) = MSsub : ANOVAtable(1, 3) = Fsub : ANOVAtable(1, 4) = Psub 'Subjects
                ANOVAtable(2, 0) = SSerr : ANOVAtable(2, 1) = DFerr : ANOVAtable(2, 2) = MSerr 'error
                ANOVAtable(3, 0) = SStot : ANOVAtable(3, 1) = DFtot 'Total

                Return ANOVAtable
            End Function

            ''' <summary>
            ''' Computes the Greenhouse–Geisser sphericity correction for RM‑ANOVA.
            ''' </summary>
            ''' <remarks>
            ''' <para>
            ''' The method:
            ''' <list type="bullet">
            '''   <item><description>Computes the sample covariance matrix</description></item>
            '''   <item><description>Double‑centers it to estimate the population covariance</description></item>
            '''   <item><description>Extracts eigenvalues</description></item>
            '''   <item><description>Computes the GG epsilon</description></item>
            '''   <item><description>Adjusts numerator and denominator degrees of freedom</description></item>
            '''   <item><description>Computes the corrected p‑value</description></item>
            ''' </list>
            ''' </para>
            ''' </remarks>
            ''' <returns>A <c>TestResult</c> containing epsilon and corrected p‑value.</returns>
            Public Function GreenhouseGeisser() As TestResult
                Dim Num As Double, Den As Double
                GreenhouseGeisserTest = New TestResult

                Dim VarCovar(,) As Double = Matrix.MatCovar(data) 'create variance-covariance matrix
                'double center sample var-covar matrix to estimate population var-covar matrix
                Dim PopVarCovar(,) As Double = Matrix.MatDoubleCenter(VarCovar)
                Dim eig = Matrix.EIGEN_JK(PopVarCovar) 'calculate eigenvector and eigenvalues
                Dim Eigenval() As Double = eig.Item1

                For i = 0 To UBound(Eigenval) - 1
                    Num += Eigenval(i) 'numerator
                    Den += Eigenval(i) * Eigenval(i) 'denominator
                Next
                Num = Num * Num
                Dim V As Double = Num / Den
                Dim Epsilon As Double = V / (NoGroups - 1)
                Dim DFb As Double = Epsilon * ANOVAtable(0, 1)
                Dim DFerr As Double = Epsilon * ANOVAtable(2, 1)

                'Excel Fdist truncates non-integer DF values so approximate F distribution by beta distribution.
                Dim Pvalue As Double = 1.0 - distributions.F_CDF(ANOVAtable(0, 3), DFb, DFerr)

                'output
                Me.GreenhouseGeisserTest.Pvalue = Pvalue
                Me.GreenhouseGeisserTest.TestStatistics1 = Epsilon
                Return GreenhouseGeisserTest
            End Function

            ''' <summary>
            ''' Computes the Huynh–Feldt sphericity correction for RM‑ANOVA.
            ''' </summary>
            ''' <remarks>
            ''' <para>
            ''' Uses the Greenhouse–Geisser epsilon to compute the HF epsilon:
            ''' 
            '''     ε_HF = (N(k−1)ε_GG − 2) / ((k−1)(N − 1 − (k−1)ε_GG))
            ''' 
            ''' where:
            ''' <list type="bullet">
            '''   <item><description>N = number of subjects</description></item>
            '''   <item><description>k = number of repeated‑measure conditions</description></item>
            ''' </list>
            ''' </para>
            ''' </remarks>
            ''' <returns>A <c>TestResult</c> containing epsilon and corrected p‑value.</returns>
            Public Function HuyhnFeldt() As TestResult
                Dim Num As Double, Den As Double
                HuyhnFeldtTest = New TestResult

                Dim VarCovar(,) As Double = Matrix.MatCovar(data) 'create variance-covariance matrix
                'double center sample var-covar matrix to estimate population var-covar matrix
                Dim PopVarCovar(,) As Double = Matrix.MatDoubleCenter(VarCovar)
                Dim eig = Matrix.EIGEN_JK(PopVarCovar) 'calculate eigenvector and eigenvalues
                Dim Eigenval() As Double = eig.Item1

                For i = 0 To UBound(Eigenval, 1) - 1
                    Num += Eigenval(i) 'numerator
                    Den += Eigenval(i) * Eigenval(i) 'denominator
                Next
                Num = Num * Num

                Dim V As Double = Num / Den
                Dim EpsilonGG As Double = V / (NoGroups - 1)
                Dim EpsilonHF As Double = (NoBlocks * (NoGroups - 1) * EpsilonGG - 2) / ((NoGroups - 1) * (NoBlocks - 1 - (NoGroups - 1) * EpsilonGG))
                Dim DFb As Double = EpsilonHF * ANOVAtable(0, 1)
                Dim DFerr As Double = EpsilonHF * ANOVAtable(2, 1)
                Dim Pvalue As Double = 1.0 - distributions.F_CDF(ANOVAtable(0, 3), DFb, DFerr)

                'output
                Me.HuyhnFeldtTest.Pvalue = Pvalue
                Me.HuyhnFeldtTest.TestStatistics1 = EpsilonHF

                Return HuyhnFeldtTest
            End Function

            ''' <summary>
            ''' Performs the Tukey–Kramer post-hoc test for repeated-measures ANOVA
            ''' without assuming sphericity. This is the recommended RM post-hoc test.
            ''' </summary>
            ''' <param name="alpha">
            ''' Optional two-sided significance level used to construct the reported confidence intervals.
            ''' The default is <c>0.05</c>, corresponding to a 95% confidence interval.
            ''' </param>
            ''' <returns>
            ''' A 2D Object array containing comparison labels, mean differences with
            ''' confidence intervals, Q-statistics, and p-values.
            ''' </returns>
            ''' <remarks>
            ''' Pairwise comparisons are returned in natural pair-generation order and
            ''' are not sorted by effect size or p-value.
            ''' </remarks>
            Public Function TukeyKramerRM2(Optional alpha As Double = 0.05) As Object(,)

                ValidateAlpha(alpha)
                Dim iFault As Integer = 0
                ReDim Me.TuekyRM2(((NoGroups * (NoGroups - 1)) / 2 - 1), 3)

                Dim arDiffs(((NoGroups * (NoGroups - 1)) / 2 - 1), 7) As Object
                Dim df As Integer = NoBlocks - 1
                Dim Qcrit As Double = distributions.QTRNG(1.0 - alpha, CDbl(df), CDbl(NoGroups), iFault)

                Dim ii As Integer = 0
                Dim arTemp(NoBlocks - 1) As Double

                For i = 0 To NoGroups - 1
                    For j = i + 1 To NoGroups - 1
                        For jj = 0 To NoBlocks - 1
                            arTemp(jj) = data(jj, i) - data(jj, j)
                        Next

                        Dim diff As Double = arTemp.Average()
                        Dim seDiff As Double = stDev(arTemp) / Math.Sqrt(NoBlocks)
                        Dim qStat As Double = Math.Abs(diff) / ((1.0 / Math.Sqrt(2.0)) * seDiff)
                        Dim pVal As Double = 1.0 - distributions.PRTRNG(qStat, CDbl(df), CDbl(NoGroups), iFault)
                        Dim margin As Double = (Qcrit / Math.Sqrt(2.0)) * seDiff

                        arDiffs(ii, 0) = qStat
                        arDiffs(ii, 1) = diff
                        arDiffs(ii, 3) = i
                        arDiffs(ii, 4) = j
                        arDiffs(ii, 5) = pVal
                        arDiffs(ii, 6) = diff - margin
                        arDiffs(ii, 7) = diff + margin
                        ii += 1
                    Next
                Next

                For i = 0 To ii - 1
                    TuekyRM2(i, 0) = varNames(CInt(arDiffs(i, 3))) & " vs. " & varNames(CInt(arDiffs(i, 4)))
                    TuekyRM2(i, 1) = CSng(arDiffs(i, 1)) & " (" & CSng(arDiffs(i, 6)) & " to " & CSng(arDiffs(i, 7)) & ")"
                    TuekyRM2(i, 2) = CStr(CSng(arDiffs(i, 0)))
                    TuekyRM2(i, 3) = CStr(CSng(arDiffs(i, 5)))
                Next

                Me.TuekyRM2Alpha = alpha
                Return TuekyRM2
            End Function

            ''' <summary>
            ''' Performs the Tukey–Kramer post-hoc test for repeated-measures ANOVA
            ''' under the assumption of sphericity (single pooled residual variance).
            ''' </summary>
            ''' <param name="alpha">
            ''' Optional two-sided significance level used to construct the reported confidence intervals.
            ''' The default is <c>0.05</c>, corresponding to a 95% confidence interval.
            ''' </param>
            ''' <returns>
            ''' A 2D Object array containing comparison labels, mean differences with
            ''' confidence intervals, Q-statistics, and p-values.
            ''' </returns>
            ''' <remarks>
            ''' Pairwise comparisons are returned in natural pair-generation order and
            ''' are not sorted by effect size or p-value.
            ''' </remarks>
            Public Function Tukey(Optional alpha As Double = 0.05) As Object(,)
                ' Tukey-Kramer post hoc test assuming sphericity
                ValidateAlpha(alpha)
                Dim out(,) As Object
                Dim iFault As Integer = 0
                Dim arTemp() As Double
                Dim nContrasts As Integer = (NoGroups * (NoGroups - 1)) / 2
                ReDim out(nContrasts - 1, 3)

                Dim arMean(NoGroups - 1) As Double
                Dim arDiffs(nContrasts - 1, 7) As Object
                Dim MSerr As Double = CDbl(Me.ANOVAtable(2, 2)) ' residual/error MS CDbl(Me.ANOVAtable(1, 2))

                For i = 0 To NoGroups - 1
                    arTemp = Matrix.GetColumnFrom2Darray(data, i)
                    arMean(i) = arTemp.Average()
                Next

                Dim df As Integer = (NoGroups * NoBlocks) + 1 - NoGroups - NoBlocks
                Dim Qcrit As Double = distributions.QTRNG(1.0 - alpha, CDbl(df), CDbl(NoGroups), iFault)

                Dim ii As Integer = 0
                For i = 0 To NoGroups - 1
                    For j = i + 1 To NoGroups - 1
                        Dim diff As Double = arMean(i) - arMean(j)
                        Dim qStat As Double = Math.Abs(diff) / Math.Sqrt(0.5 * MSerr * (1.0 / NoBlocks + 1.0 / NoBlocks))
                        Dim pVal As Double = 1.0 - distributions.PRTRNG(qStat, CDbl(df), CDbl(NoGroups), iFault)
                        Dim margin As Double = (Qcrit / Math.Sqrt(2.0)) * Math.Sqrt(MSerr) * Math.Sqrt(1.0 / NoBlocks + 1.0 / NoBlocks)

                        arDiffs(ii, 1) = diff
                        arDiffs(ii, 2) = qStat
                        arDiffs(ii, 3) = i
                        arDiffs(ii, 4) = j
                        arDiffs(ii, 5) = pVal
                        arDiffs(ii, 6) = diff - margin
                        arDiffs(ii, 7) = diff + margin
                        ii += 1
                    Next
                Next

                For i = 0 To ii - 1
                    out(i, 0) = varNames(CInt(arDiffs(i, 3))) & " vs. " & varNames(CInt(arDiffs(i, 4)))
                    out(i, 1) = CSng(arDiffs(i, 1)) & " (" & CSng(arDiffs(i, 6)) & " to " & CSng(arDiffs(i, 7)) & ")"
                    out(i, 2) = CStr(CSng(arDiffs(i, 2)))
                    out(i, 3) = CStr(CSng(arDiffs(i, 5)))
                Next

                Me.TukeyOut = out
                Me.TukeyOutAlpha = alpha
                Return out
            End Function
        End Class



        ''' <summary>
        ''' Implements the classical two‑sample (unpaired) t‑test for comparing the
        ''' means of two independent groups. Computes:
        ''' <list type="bullet">
        '''   <item><description>Pooled‑variance t‑test (equal variances assumed)</description></item>
        '''   <item><description>Welch’s t‑test (unequal variances)</description></item>
        '''   <item><description>Degrees of freedom for both tests</description></item>
        '''   <item><description>Two‑sided p‑values</description></item>
        '''   <item><description>Mean‑difference confidence intervals</description></item>
        '''   <item><description>F‑test for equality of variances</description></item>
        ''' </list>
        ''' 
        ''' Results are returned as formatted <c>ResultTable</c> objects suitable for reporting.
        ''' </summary>
        Public Class UnpairedTtest
            Private data()() As Double
            Private varNames() As String
            Private TtestRes As TestResult
            Private diffCI As ConfidenceIntervalResult
            Private diffCIunq As ConfidenceIntervalResult
            Private SE As Double
            Private SEunq As Double

            ''' <summary>
            ''' Initializes the unpaired t‑test with two independent samples.
            ''' </summary>
            ''' <param name="x">
            ''' A jagged array where:
            ''' <list type="bullet">
            '''   <item><description><c>x(0)</c> contains all observations from group 1</description></item>
            '''   <item><description><c>x(1)</c> contains all observations from group 2</description></item>
            ''' </list>
            ''' </param>
            ''' <param name="varNames">Names of the two groups for reporting.</param>
            Public Sub New(x()() As Double, varNames() As String)
                If x Is Nothing OrElse x.Length <> 2 Then AppGlobals.BSerr.LogAndThrow(New ArgumentException("Two groups are expected for the Unpaired t-test"))
                If x(0) Is Nothing OrElse x(0).Length < 2 Then AppGlobals.BSerr.LogAndThrow(New ArgumentException("At least two values are expected in group 1"))
                If x(1) Is Nothing OrElse x(1).Length < 2 Then AppGlobals.BSerr.LogAndThrow(New ArgumentException("At least two values are expected in group 2"))

                Me.data = x
                Me.varNames = varNames
            End Sub

            ''' <summary>
            ''' Wraps the results of the unpaired t-test into two <c>ResultTable</c> objects:
            ''' one assuming equal variances (pooled t-test) and one assuming unequal
            ''' variances (Welch’s t-test).
            ''' </summary>
            ''' <remarks>
            ''' <para>
            ''' The first table contains:
            ''' <list type="bullet">
            '''   <item><description>Pooled standard error</description></item>
            '''   <item><description>Pooled t-statistic</description></item>
            '''   <item><description>Pooled degrees of freedom</description></item>
            '''   <item><description>Two-sided p-value</description></item>
            '''   <item><description>Mean difference with a confidence interval at the selected level</description></item>
            ''' </list>
            ''' </para>
            ''' 
            ''' <para>
            ''' The second table contains:
            ''' <list type="bullet">
            '''   <item><description>Welch standard error</description></item>
            '''   <item><description>Welch t-statistic</description></item>
            '''   <item><description>Welch degrees of freedom</description></item>
            '''   <item><description>Two-sided p-value</description></item>
            '''   <item><description>Mean difference with a confidence interval at the selected level</description></item>
            '''   <item><description>F-test p-value for equality of variances</description></item>
            ''' </list>
            ''' </para>
            ''' </remarks>
            ''' <returns>A list of <c>ResultTable</c> objects.</returns>
            Public Function wrapResults() As List(Of ResultTable)
                Dim out = New List(Of ResultTable)
                Dim t = New ResultTable
                t.SetBody({{"Combined SE", Me.SE},
                        {"t", Me.TtestRes.TestStatistics1},
                        {"df", Me.TtestRes.DF1},
                        {"Two sided p-value", Me.TtestRes.Pvalue},
                        {"mean diff (" & Me.diffCI.CIlabel & ")", Me.diffCI.strConfidenceInterval}})
                t.AddHeaderTopRow({"Unpaired T-test", ""})
                t.AddHeaderTopRow({"Assuming equal variance", ""})
                out.Add(t)

                t = New ResultTable
                t.SetBody({{"Combined SE", Me.SEunq},
                        {"t", Me.TtestRes.TestStatistics2},
                        {"df", Me.TtestRes.DF2},
                        {"Two sided p-value", Me.TtestRes.Pvalue2},
                        {"mean diff (" & Me.diffCIunq.CIlabel & ")", Me.diffCIunq.strConfidenceInterval},
                        {"F test p-value", FTest(data(0), data(1))}})
                t.AddHeaderTopRow({"Assuming unequal variance", ""})
                out.Add(t)
                Return out
            End Function

            ''' <summary>
            ''' Performs the two-sample unpaired t-test, computing:
            ''' <list type="bullet">
            '''   <item><description>Group means</description></item>
            '''   <item><description>Pooled standard error (equal variances)</description></item>
            '''   <item><description>Welch standard error (unequal variances)</description></item>
            '''   <item><description>Pooled t-statistic and degrees of freedom</description></item>
            '''   <item><description>Welch t-statistic and degrees of freedom</description></item>
            '''   <item><description>Two-sided p-values for both tests</description></item>
            '''   <item><description>Mean-difference confidence intervals (equal and unequal variances)</description></item>
            ''' </list>
            ''' </summary>
            ''' <param name="alpha">
            ''' Optional two-sided significance level used for both mean-difference confidence intervals.
            ''' The default is <c>0.05</c>, corresponding to a 95% confidence interval.
            ''' </param>
            ''' <returns>
            ''' A <c>TestResult</c> object containing:
            ''' <list type="bullet">
            '''   <item><description><c>TestStatistics1</c> — pooled t-statistic</description></item>
            '''   <item><description><c>TestStatistics2</c> — Welch t-statistic</description></item>
            '''   <item><description><c>Pvalue</c> — pooled two-sided p-value</description></item>
            '''   <item><description><c>Pvalue2</c> — Welch two-sided p-value</description></item>
            '''   <item><description><c>DF1</c> — pooled degrees of freedom</description></item>
            '''   <item><description><c>DF2</c> — Welch degrees of freedom</description></item>
            ''' </list>
            ''' </returns>
            Public Function compute(Optional alpha As Double = 0.05) As TestResult
                Dim out = New TestResult
                Dim n1 As Integer = data(0).Length
                Dim n2 As Integer = data(1).Length
                Dim mean1 As Double = data(0).Average()
                Dim mean2 As Double = data(1).Average()
                Dim diff As Double = mean1 - mean2
                Me.SE = Math.Sqrt((DevSq(data(0)) + DevSq(data(1))) / (n1 + n2 - 2.0) * (1.0 / n1 + 1.0 / n2))
                Dim s1 As Double = variance(data(0))
                Dim s2 As Double = variance(data(1))
                Me.SEunq = Math.Sqrt(s1 / n1 + s2 / n2)
                Dim df_unq As Double = (SEunq) ^ 4 / (((s1 / n1) ^ 2 / (n1 - 1.0)) + ((s2 / n2) ^ 2 / (n2 - 1.0)))

                out.TestStatistics1 = diff / SE
                out.TestStatistics2 = diff / SEunq
                out.Pvalue = distributions.T_2T(Math.Abs(diff / SE), n1 + n2 - 2)
                out.Pvalue2 = distributions.T_2T(Math.Abs(diff / SEunq), df_unq)
                out.DF1 = n1 + n2 - 2
                out.DF2 = df_unq

                Me.diffCI = New ConfidenceIntervalResult With {
                        .alpha = alpha,
                        .Estimate = diff,
                        .LowerLimit = diff - (distributions.T_Inv_2T(alpha, out.DF1) * SE),
                        .UpperLimit = diff + (distributions.T_Inv_2T(alpha, out.DF1) * SE)
                    }

                Me.diffCIunq = New ConfidenceIntervalResult With {
                        .alpha = alpha,
                        .Estimate = diff,
                        .LowerLimit = diff - (distributions.T_Inv_2T(alpha, out.DF2) * SEunq),
                        .UpperLimit = diff + (distributions.T_Inv_2T(alpha, out.DF2) * SEunq)
                    }

                Me.TtestRes = out
                Return out
            End Function
        End Class



        ''' <summary>
        ''' Implements the classical paired‑samples t‑test for comparing the means of
        ''' two dependent measurements taken on the same subjects (e.g., before/after,
        ''' left/right, matched pairs). Computes:
        ''' <list type="bullet">
        '''   <item><description>Pairwise differences</description></item>
        '''   <item><description>Mean difference</description></item>
        '''   <item><description>Standard deviation and standard error of differences</description></item>
        '''   <item><description>t‑statistic and degrees of freedom</description></item>
        '''   <item><description>Two‑sided p‑value</description></item>
        ''' </list>
        ''' 
        ''' Results are returned as a formatted <c>ResultTable</c> suitable for reporting.
        ''' </summary>
        Public Class PairedTtest
            Private data(,) As Double
            Private varNames() As String
            Private pDifferences() As Double
            Private TtestRes As TestResult
            Private SE As Double
            Private pDiffMean As Double

            ''' <summary>
            ''' Initializes the paired t‑test with paired observations.
            ''' </summary>
            ''' <param name="x">
            ''' A 2D matrix where each row represents a subject and:
            ''' <list type="bullet">
            '''   <item><description><c>x(i,0)</c> is the first measurement</description></item>
            '''   <item><description><c>x(i,1)</c> is the second measurement</description></item>
            ''' </list>
            ''' </param>
            ''' <param name="varNames">Names of the two paired variables.</param>
            Public Sub New(x(,) As Double, varNames() As String)
                Me.data = x
                Me.varNames = varNames
            End Sub

            ''' <summary>
            ''' Returns the vector of paired differences:
            '''     d_i = x_i − y_i
            ''' computed during <c>compute()</c>.
            ''' </summary>
            Public ReadOnly Property Differences() As Double()
                Get
                    Return pDifferences
                End Get
            End Property

            ''' <summary>
            ''' Wraps the paired t‑test results into a <c>ResultTable</c> object containing:
            ''' <list type="bullet">
            '''   <item><description>Number of valid data pairs</description></item>
            '''   <item><description>Mean of differences</description></item>
            '''   <item><description>Standard deviation of differences</description></item>
            '''   <item><description>Standard error of the mean difference</description></item>
            '''   <item><description>Degrees of freedom</description></item>
            '''   <item><description>t‑statistic</description></item>
            '''   <item><description>Two‑sided p‑value</description></item>
            ''' </list>
            ''' </summary>
            ''' <returns>A list containing one formatted <c>ResultTable</c>.</returns>
            Public Function wrapResults() As List(Of ResultTable)
                Dim out = New List(Of ResultTable)
                Dim t = New ResultTable
                t.SetBody({{"Number of valid data pairs", Me.pDifferences.Length},
                        {"Mean of differences", Me.pDiffMean},
                        {"Standard deviation", Me.SE * Math.Sqrt(Me.pDifferences.Length)},
                        {"Standard error", Me.SE},
                        {"df", Me.TtestRes.DF1},
                        {"t", Me.TtestRes.TestStatistics1},
                        {"Two-sided p-value", Me.TtestRes.Pvalue}
                       })
                t.AddHeaderTopRow({"Paired T-test", ""})
                out.Add(t)
                Return out
            End Function

            ''' <summary>
            ''' Performs the paired‑samples t‑test by computing:
            ''' <list type="bullet">
            '''   <item><description>Pairwise differences d_i = x_i − y_i</description></item>
            '''   <item><description>Mean difference</description></item>
            '''   <item><description>Variance and standard error of differences</description></item>
            '''   <item><description>t‑statistic:  t = mean(d) / (sd(d) / √n)</description></item>
            '''   <item><description>Degrees of freedom: n − 1</description></item>
            '''   <item><description>Two‑sided p‑value</description></item>
            ''' </list>
            ''' </summary>
            ''' <returns>
            ''' A <c>TestResult</c> containing the t‑statistic, degrees of freedom,
            ''' and two‑sided p‑value.
            ''' </returns>
            Public Function compute() As TestResult
                Dim out = New TestResult
                Dim n As Integer = data.GetLength(0)
                ReDim pDifferences(n - 1)
                For i = 0 To n - 1
                    pDifferences(i) = data(i, 0) - data(i, 1)
                Next

                pDiffMean = pDifferences.Average()
                Dim var As Double = variance(pDifferences)
                Me.SE = Math.Sqrt(var) / Math.Sqrt(n)
                out.DF1 = n - 1
                out.TestStatistics1 = pDiffMean / Math.Sqrt(var / n)
                out.Pvalue = distributions.T_2T(Math.Abs(out.TestStatistics1), CDbl(out.DF1))

                Me.TtestRes = out
                Return out
            End Function
        End Class


        ''' <summary>
        ''' Implements Hotelling’s two‑sample T² test for comparing the multivariate
        ''' means of two independent groups. Supports:
        ''' <list type="bullet">
        '''   <item><description>Equal‑covariance Hotelling’s T² test</description></item>
        '''   <item><description>Unequal‑covariance (generalized) Hotelling’s T² test</description></item>
        '''   <item><description>Simultaneous confidence intervals for mean differences</description></item>
        '''   <item><description>Standard errors for each variable</description></item>
        '''   <item><description>Multivariate F‑test conversion and p‑values</description></item>
        ''' </list>
        ''' 
        ''' Results are returned as formatted <c>ResultTable</c> objects suitable for
        ''' multivariate reporting.
        ''' </summary>
        Public Class HotelingsT_independent

            Private pMeans() As Double = Nothing
            Private pSE() As Double = Nothing
            Private pCIs As List(Of String) = Nothing
            Private data1(,) As Double
            Private data2(,) As Double
            Private pVarNames() As String
            Private pAlpha As Double = 0.05
            Private HT_eq As TestResult = Nothing
            Private HT_uneq As TestResult = Nothing
            Public ReadOnly CIs As New List(Of ConfidenceIntervalResult)

            ''' <summary>
            ''' Initializes the Hotelling’s T² test with two independent multivariate samples.
            ''' </summary>
            ''' <param name="x1">First dataset (n₁ × p matrix).</param>
            ''' <param name="x2">Second dataset (n₂ × p matrix).</param>
            ''' <param name="varNames">Names of the p variables.</param>
            Public Sub New(x1(,) As Double, x2(,) As Double, varNames() As String)
                Me.data1 = x1
                Me.data2 = x2
                Me.pVarNames = varNames
            End Sub


            ''' <summary>
            ''' Wraps the Hotelling’s T² results into two <c>ResultTable</c> objects:
            ''' <list type="bullet">
            '''   <item><description>Simultaneous confidence intervals for each variable</description></item>
            '''   <item><description>Equal‑covariance Hotelling’s T² test</description></item>
            '''   <item><description>Unequal‑covariance Hotelling’s T² test</description></item>
            ''' </list>
            ''' </summary>
            ''' <remarks>
            ''' <para>
            ''' The first table contains per‑variable summaries:
            ''' <list type="bullet">
            '''   <item><description>Null mean differences (0)</description></item>
            '''   <item><description>Observed mean differences</description></item>
            '''   <item><description>Standard errors</description></item>
            '''   <item><description>Simultaneous confidence intervals at the selected level</description></item>
            ''' </list>
            ''' </para>
            ''' 
            ''' <para>
            ''' The second table contains multivariate test results for both:
            ''' <list type="bullet">
            '''   <item><description>Equal covariance assumption</description></item>
            '''   <item><description>Unequal covariance assumption</description></item>
            ''' </list>
            ''' </para>
            ''' </remarks>
            ''' <returns>A list of formatted <c>ResultTable</c> objects.</returns>
            Public Function wrapResults() As List(Of ResultTable)
                Dim out = New List(Of ResultTable)
                Dim t = New ResultTable
                Dim ciLabel As String = $"{(1.0 - Me.pAlpha) * 100.0:0.##}% CI (Simultaneous)"

                If Me.pCIs Is Nothing Then Me.CI(Me.pAlpha) 'if no CIs then calculate them
                Dim o(3, Me.pVarNames.Length - 1) As Object
                For i = 0 To Me.pVarNames.Length - 1
                    o(0, i) = 0
                    o(1, i) = pMeans(i)
                    o(2, i) = pSE(i)
                    o(3, i) = Me.pCIs(i)
                Next
                t.SetBody(o)
                t.AddHeaderTopRow(Me.pVarNames)
                t.AddHeaderLeftRow({"H0 Mean Diffs", "Mean of Differences", "StdErr", ciLabel})
                out.Add(t)

                'Test result
                If Me.HT_eq Is Nothing Then Me.HT_eq = Me.calculate(True)
                If Me.HT_uneq Is Nothing Then Me.HT_uneq = Me.calculate(False)
                t = New ResultTable
                t.SetBody({{data1.GetLength(0)}, {data2.GetLength(0)}, {data1.GetLength(1)},
                           {Me.HT_eq.TestStatistics1}, {Me.HT_eq.Pvalue}, {Me.pAlpha}, {""},
                           {Me.HT_uneq.TestStatistics1}, {Me.HT_uneq.DF1}, {Me.HT_uneq.Pvalue}})
                t.AddHeaderTopRow({"Two independent samples Hotelling's T-squared", "Equal Covariance Structure Assumed"})
                t.AddHeaderLeftRow({"Number of records Grp1", "Number of records Grp2", "Number of Variables",
                                   "T2", "Two-sided p-value", "Alpha", "",
                                   "T2", "Df2", "Two-sided p-value"})
                out.Add(t)

                Return out
            End Function

            ''' <summary>
            ''' Computes Hotelling’s two‑sample T² statistic under either:
            ''' <list type="bullet">
            '''   <item><description>Equal covariance matrices (pooled covariance)</description></item>
            '''   <item><description>Unequal covariance matrices (generalized T²)</description></item>
            ''' </list>
            ''' </summary>
            ''' <param name="bCovEqual">
            ''' If True, uses pooled covariance and classical Hotelling’s T².
            ''' If False, uses separate covariance matrices and adjusted degrees of freedom.
            ''' </param>
            ''' <returns>
            ''' A <c>TestResult</c> containing:
            ''' <list type="bullet">
            '''   <item><description><c>TestStatistics1</c> — Hotelling’s T² value</description></item>
            '''   <item><description><c>Pvalue</c> — multivariate F‑test p‑value</description></item>
            '''   <item><description><c>DF1</c> — adjusted degrees of freedom (unequal covariance only)</description></item>
            ''' </list>
            ''' </returns>
            ''' <remarks>
            ''' <para><b>Equal covariance case:</b></para>
            ''' <para>
            '''     T² = (x̄₁ − x̄₂)' S_p⁻¹ (x̄₁ − x̄₂)
            '''     F = ((n₁ + n₂ − 1 − p) / (p (n₁ + n₂ − 2))) T²
            ''' </para>
            ''' 
            ''' <para><b>Unequal covariance case:</b></para>
            ''' <para>
            ''' Uses the generalized T² statistic with adjusted degrees of freedom based on
            ''' the method of Nel and Van der Merwe (1986).
            ''' </para>
            ''' </remarks>
            Public Function calculate(bCovEqual As Boolean) As TestResult
                Dim n1 As Integer = data1.GetLength(0)
                Dim n2 As Integer = data2.GetLength(0)
                Dim p As Integer = data1.GetLength(1)
                If p <> data2.GetLength(1) Then
                    AppGlobals.BSerr.LogAndThrow(New ArgumentException("Error: Hotelling's T-squared requied The same number of columns in the Input Datasets."))
                End If

                If pMeans Is Nothing Then
                    ReDim pMeans(p - 1)
                    For i = 0 To p - 1
                        Dim tmp1 = Matrix.GetColumnFrom2Darray(data1, i)
                        Dim tmp2 = Matrix.GetColumnFrom2Darray(data2, i)
                        pMeans(i) = tmp1.Average() - tmp2.Average()
                    Next
                End If

                'Convariance MatrixType of 1st Group
                Dim covar1(,) As Double = Matrix.MatCovar(data1)
                If bCovEqual Then
                    covar1 = Matrix.MatrixMult(covar1, n1 - 1)
                Else
                    covar1 = Matrix.MatrixMult(covar1, 1 / n1)
                End If

                'Convariance MatrixType of 2nd Group
                Dim covar2(,) As Double = Matrix.MatCovar(data2)
                If bCovEqual Then
                    covar2 = Matrix.MatrixMult(covar2, n2 - 1)
                Else
                    covar2 = Matrix.MatrixMult(covar2, 1 / n2)
                End If

                'Pooled Convariance MatrixType
                Dim covar(,) As Double = Matrix.M_ADD(covar1, covar2)
                Dim tot_covar(p - 1, p - 1) As Double
                If bCovEqual Then
                    tot_covar = Matrix.MatrixMult(covar, (1.0 / n1 + 1.0 / n2) * (1.0 / (n1 + n2 - 2)))
                Else
                    tot_covar = covar
                End If

                Dim covarinv(,) As Double = Matrix.MatInv(tot_covar, "CHOL")
                Dim H(,) As Double = Matrix.MatrixMult(pMeans, Matrix.MatrixMult(covarinv, pMeans))
                Dim out As New TestResult
                If bCovEqual Then
                    out.TestStatistics1 = H(0, 0)
                    out.Pvalue = distributions.F_RT((n1 + n2 - 1 - p) * out.TestStatistics1 / (p * (n1 + n2 - 2)), CDbl(p), n1 + n2 - 1 - p)
                    Me.HT_eq = out
                Else
                    'compute adjusted DF
                    Dim k1(,) As Double = Matrix.MatrixMult(pMeans, covarinv)
                    Dim k2(,) As Double = Matrix.MatrixMult(covarinv, pMeans)
                    Dim h1(,) As Double = Matrix.MatrixMult(k1, Matrix.MatrixMult(covar1, k2))
                    Dim h2(,) As Double = Matrix.MatrixMult(k1, Matrix.MatrixMult(covar2, k2))
                    H = Matrix.MatrixMult(k1, pMeans) 're-using the h array

                    Dim df As Double = 1.0 / ((h1(0, 0) / H(0, 0)) ^ 2 / (n1 - 1) + (h2(0, 0) / H(0, 0)) ^ 2 / (n2 - 1))
                    out.DF1 = df
                    out.TestStatistics1 = H(0, 0)
                    out.Pvalue = distributions.F_RT((n1 + n2 - 1 - p) * out.TestStatistics1 / (p * (n1 + n2 - 2)), CDbl(p), df)
                    Me.HT_uneq = out
                End If

                Return out
            End Function

            ''' <summary>
            ''' Computes simultaneous (1 − α) confidence intervals for the vector of mean
            ''' differences between two independent multivariate samples using Hotelling’s
            ''' T² methodology.
            ''' </summary>
            ''' <remarks>
            ''' <para>
            ''' For p variables, the simultaneous confidence intervals are based on the
            ''' multivariate F‑distribution:
            ''' </para>
            ''' 
            ''' <para>
            '''     CI_j = d_j ± sqrt( c * Var(d_j) )
            ''' </para>
            ''' 
            ''' <para>
            ''' where:
            ''' <list type="bullet">
            '''   <item><description><c>d_j</c> is the mean difference for variable j</description></item>
            '''   <item><description><c>Var(d_j)</c> is the j‑th diagonal element of the covariance matrix
            '''       of the mean differences</description></item>
            '''   <item><description><c>c</c> is the simultaneous critical value derived from Hotelling’s T²:
            '''       <br/>c = (p (n₁ + n₂ − 2) / (n₁ + n₂ − p − 1)) F_{p, n₁+n₂−p−1}(1 − α)</description></item>
            ''' </list>
            ''' </para>
            ''' 
            ''' <para>
            ''' The method populates:
            ''' <list type="bullet">
            '''   <item><description><c>pMeans()</c> — mean differences</description></item>
            '''   <item><description><c>pSE()</c> — standard errors for each variable</description></item>
            '''   <item><description><c>pCIs</c> — formatted confidence‑interval strings</description></item>
            ''' </list>
            ''' </para>
            ''' 
            ''' <para>
            ''' These intervals are simultaneous across all variables and therefore control
            ''' the family‑wise error rate at the specified α level.
            ''' </para>
            ''' </remarks>
            ''' <param name="alpha">
            ''' Two-sided significance level used to construct the simultaneous confidence intervals.
            ''' Must satisfy <c>0 &lt; alpha &lt; 1</c>.
            ''' The default convention <c>alpha = 0.05</c> corresponds to simultaneous 95% confidence intervals.
            ''' </param>
            Public Function CI(alpha As Double) As List(Of ConfidenceIntervalResult)
                Me.pAlpha = alpha
                Dim n1 As Integer = data1.GetLength(0)
                Dim n2 As Integer = data2.GetLength(0)
                Dim p As Integer = data1.GetLength(1)
                If p <> data2.GetLength(1) Then
                    AppGlobals.BSerr.LogAndThrow(New ArgumentException("Error: Hotelling's T-squared requied The same number of columns in the Input Datasets."))
                End If
                If alpha < 0.0 Or alpha > 1.0 Then
                    AppGlobals.BSerr.LogAndThrow(New ArgumentException("Error: Independent samples version of Hotelling's T-squared alpha must be (0 to 1)."))
                End If
                Dim diffs(p - 1) As Double
                Me.pCIs = New List(Of String)
                ReDim pMeans(p - 1), pSE(p - 1)

                Dim Tcrit As Double = Math.Sqrt(distributions.F_Inv_RT(alpha, CDbl(p), n1 + n2 - 1 - p) * p * (n1 + n2 - 2) / (n1 + n2 - 1 - p))

                For i = 0 To p - 1
                    Dim tmp1 = Matrix.GetColumnFrom2Darray(data1, i)
                    Dim tmp2 = Matrix.GetColumnFrom2Darray(data2, i)
                    pMeans(i) = tmp1.Average() - tmp2.Average()
                    pSE(i) = Math.Sqrt(((n1 - 1) * variance(tmp1) + (n2 - 1) * variance(tmp2)) / (n1 + n2 - 2)) * Math.Sqrt(1.0 / n1 + 1.0 / n2)

                    Dim CIres As New ConfidenceIntervalResult With {
                            .alpha = alpha,
                            .Estimate = pMeans(i),
                            .LowerLimit = pMeans(i) - Tcrit * pSE(i),
                            .UpperLimit = pMeans(i) + Tcrit * pSE(i)
                        }
                    Me.pCIs.Add(CIres.strConfidenceInterval(CIformat.LL_to_UL))
                    Me.CIs.Add(CIres)
                Next

                Return Me.CIs

            End Function

        End Class


        ''' <summary>
        ''' Implements Hotelling’s one‑sample T² test for assessing whether the
        ''' multivariate mean vector of a single sample differs from a specified
        ''' null mean vector H₀. Supports:
        ''' <list type="bullet">
        '''   <item><description>Hotelling’s T² statistic</description></item>
        '''   <item><description>Multivariate F‑test conversion and p‑value</description></item>
        '''   <item><description>Simultaneous confidence intervals for each variable</description></item>
        '''   <item><description>Individual univariate t‑tests for comparison</description></item>
        ''' </list>
        ''' 
        ''' Results are returned as formatted <c>ResultTable</c> objects suitable for
        ''' multivariate reporting.
        ''' </summary>
        Public Class HotelingsT_single

            Private data(,) As Double
            Private H0() As Double
            Private pVarNames() As String
            Private pMeans() As Double = Nothing
            Private pSE() As Double = Nothing
            Private pCIs As List(Of String)
            Private pAlpha As Double = 0.05
            Private pHT As TestResult = Nothing
            Public ReadOnly CIs As New List(Of ConfidenceIntervalResult)

            ''' <summary>
            ''' Initializes the one‑sample Hotelling’s T² test.
            ''' </summary>
            ''' <param name="x">Data matrix (n × p), where rows are observations and columns are variables.</param>
            ''' <param name="H0">Null mean vector of length p.</param>
            ''' <param name="varNames">Names of the p variables.</param>
            Public Sub New(x(,) As Double, H0() As Double, varNames() As String)
                Me.data = x
                Me.H0 = H0
                Me.pVarNames = varNames
            End Sub

            ''' <summary>
            ''' Wraps the Hotelling’s T² results into two <c>ResultTable</c> objects:
            ''' <list type="bullet">
            '''   <item><description>Per‑variable summaries (mean differences, SE, individual t‑tests, simultaneous CIs)</description></item>
            '''   <item><description>Overall Hotelling’s T² test result</description></item>
            ''' </list>
            ''' </summary>
            ''' <param name="bPaired">
            ''' If True, labels the output as “Paired Samples Hotelling’s T‑squared”.
            ''' Otherwise, labels it as “Single Sample Hotelling’s T‑squared”.
            ''' </param>
            ''' <returns>A list of formatted <c>ResultTable</c> objects.</returns>
            Public Function wrapResults(Optional bPaired As Boolean = False) As List(Of ResultTable)
                Dim out = New List(Of ResultTable)
                Dim t = New ResultTable
                Dim ciLabel As String = $"{(1.0 - Me.pAlpha) * 100.0:0.##}% CI (Simultaneous)"

                If Me.pCIs Is Nothing Then Me.CI(Me.pAlpha) 'if no CIs then calculate them
                Dim o(5, Me.pVarNames.Length - 1) As Object, n As Integer = UBound(Me.data, 1) + 1
                For i = 0 To Me.pVarNames.Length - 1
                    o(0, i) = H0(i)
                    o(1, i) = pMeans(i)
                    o(2, i) = pSE(i)
                    o(3, i) = (pMeans(i) - H0(i)) / pSE(i) 'Individual T-test test statistic
                    o(4, i) = distributions.T_2T(Math.Abs(CDbl(o(3, i))), n - 1) 'p-value of Individual T-test
                    o(5, i) = Me.pCIs(i)
                Next

                t.SetBody(o)
                t.AddHeaderTopRow(Me.pVarNames)
                t.AddHeaderLeftRow({"H0 Mean Diffs", "Mean of Differences", "StdErr", "Individual T-test", "T-test two-sided p-value", ciLabel})
                out.Add(t)

                'Test result
                If Me.pHT Is Nothing Then Me.pHT = Me.calculate()
                t = New ResultTable
                t.SetBody({{UBound(Me.data) + 1}, {UBound(Me.data, 2) + 1},
                           {Me.pHT.TestStatistics1}, {Me.pHT.Pvalue}, {Me.pAlpha}})
                Dim strT As String = If(bPaired, "Paired Samples Hotelling's T-squared", "Single Sample Hotelling's T-squared")
                t.AddHeaderTopRow({strT, ""})
                t.AddHeaderLeftRow({"Number of records", "Number of Variables", "T2", "Two-sided p-value", "Alpha"})
                out.Add(t)
                Return out
            End Function

            ''' <summary>
            ''' Computes Hotelling’s one‑sample T² statistic:
            ''' 
            '''     T² = n ( x̄ − H₀ )' S⁻¹ ( x̄ − H₀ )
            ''' 
            ''' where:
            ''' <list type="bullet">
            '''   <item><description><c>x̄</c> is the sample mean vector</description></item>
            '''   <item><description><c>S</c> is the sample covariance matrix</description></item>
            '''   <item><description><c>n</c> is the sample size</description></item>
            ''' </list>
            ''' 
            ''' The statistic is converted to an F‑value using:
            ''' 
            '''     F = ((n − p) / (p (n − 1))) T²
            ''' 
            ''' with df₁ = p and df₂ = n − p.
            ''' </summary>
            ''' <returns>
            ''' A <c>TestResult</c> containing Hotelling’s T² statistic and its
            ''' corresponding multivariate F‑test p‑value.
            ''' </returns>
            Public Function calculate() As TestResult
                'computed according to https://www.ncss.com/wp-content/themes/ncss/pdf/Procedures/NCSS/Hotellings_One-Sample_T2.pdf
                Dim n As Integer = data.GetLength(0)
                Dim p As Integer = data.GetLength(1)
                If p <> H0.Length Then
                    AppGlobals.BSerr.LogAndThrow(New ArgumentException("Error: Single sample version of Hotelling's T-squared requied The same number of columns in the Input Datasets."))
                End If
                Dim diffs(p - 1) As Double, Data_(n - 1, p - 1) As Double
                ReDim pMeans(p - 1)

                For i = 0 To p - 1
                    Dim tmp = Matrix.GetColumnFrom2Darray(data, i)
                    pMeans(i) = tmp.Average()
                    diffs(i) = pMeans(i) - H0(i)
                Next

                For i = 0 To n - 1
                    For j = 0 To p - 1
                        Data_(i, j) = data(i, j) - pMeans(j)
                    Next
                Next

                Dim covar(,) As Double = Matrix.MatrixMult(Matrix.trans(Data_), Data_)
                covar = Matrix.MatrixMult(covar, 1 / (n - 1))
                Dim covarinv(,) As Double = Matrix.MatInv(covar, "CHOL")
                Dim H(,) As Double = Matrix.MatrixMult(Matrix.MatrixMult(diffs, covarinv), diffs)
                Dim out As New TestResult
                out.TestStatistics1 = H(0, 0) * n
                out.Pvalue = distributions.F_RT((n - p) * out.TestStatistics1 / (p * (n - 1)), CDbl(p), n - p)
                Me.pHT = out
                Return out
            End Function

            ''' <summary>
            ''' Computes simultaneous (1 − α) confidence intervals for each component
            ''' of the mean vector using Hotelling’s T² methodology.
            ''' </summary>
            ''' <remarks>
            ''' <para>
            ''' For each variable j, the simultaneous confidence interval is:
            ''' </para>
            ''' 
            ''' <para>
            '''     CI_j = x̄_j ± Tcrit · SE_j
            ''' </para>
            ''' 
            ''' <para>
            ''' where:
            ''' <list type="bullet">
            '''   <item><description><c>x̄_j</c> is the sample mean of variable j</description></item>
            '''   <item><description><c>SE_j = s_j / √n</c> is the standard error</description></item>
            '''   <item><description><c>Tcrit</c> is the Hotelling simultaneous critical value:</description></item>
            ''' </list>
            ''' </para>
            ''' 
            ''' <para>
            '''     Tcrit = √( p (n − 1) / (n − p) · F_{p, n−p}(1 − α) )
            ''' </para>
            ''' 
            ''' <para>
            ''' These intervals control the family‑wise error rate across all p variables.
            ''' </para>
            ''' </remarks>
            ''' <param name="alpha">
            ''' Two-sided significance level used to construct the simultaneous confidence intervals.
            ''' Must satisfy <c>0 &lt; alpha &lt; 1</c>.
            ''' The default convention <c>alpha = 0.05</c> corresponds to simultaneous 95% confidence intervals.
            ''' </param>
            ''' <returns>
            ''' A list of <see cref="ConfidenceIntervalResult"/> objects, one per variable,
            ''' each carrying the estimate, interval bounds, and the supplied alpha level.
            ''' </returns>
            Public Function CI(alpha As Double) As List(Of ConfidenceIntervalResult)
                'computes simultaneous confidence intervals for Single sample Hotelling's T-squared test
                Me.pAlpha = alpha
                Dim n As Integer = data.GetLength(0)
                Dim p As Integer = data.GetLength(1)
                If p <> H0.Length Then
                    AppGlobals.BSerr.LogAndThrow(New ArgumentException("Error: Single sample version of Hotelling's T-squared requied The same number of columns in the Input Datasets."))
                End If
                If alpha < 0.0 Or alpha > 1.0 Then
                    AppGlobals.BSerr.LogAndThrow(New ArgumentException("Error: Single sample version of Hotelling's T-squared alpha must be (0 to 1)."))
                End If

                Dim diffs(p - 1) As Double
                ReDim pMeans(p - 1), pSE(p - 1)

                Dim Tcrit As Double = Math.Sqrt(p * (n - 1) / (n - p) * distributions.F_Inv_RT(alpha, CDbl(p), n - p))
                Me.pCIs = New List(Of String)
                For i = 0 To p - 1
                    Dim tmp = Matrix.GetColumnFrom2Darray(data, i)
                    pMeans(i) = tmp.Average()
                    pSE(i) = stDev(tmp) / Math.Sqrt(n)
                    diffs(i) = pMeans(i) - H0(i)

                    Dim CIres As New ConfidenceIntervalResult With {
                            .alpha = alpha,
                            .Estimate = pMeans(i),
                            .LowerLimit = pMeans(i) - Tcrit * pSE(i),
                            .UpperLimit = pMeans(i) + Tcrit * pSE(i)
                        }
                    Me.pCIs.Add(CIres.strConfidenceInterval(CIformat.LL_to_UL))
                    Me.CIs.Add(CIres)
                Next

                Return Me.CIs
            End Function

        End Class


        ''' <summary>
        ''' Implements Hotelling’s paired‑samples T² test for comparing the multivariate
        ''' means of two dependent datasets. This test evaluates whether the mean vector
        ''' of paired differences differs from a null vector of zeros.
        ''' </summary>
        ''' <remarks>
        ''' <para>
        ''' The paired Hotelling’s T² test is equivalent to:
        ''' </para>
        ''' 
        ''' <para>
        '''     • Computing the difference matrix D = X₁ − X₂  
        '''     • Applying the one‑sample Hotelling’s T² test to D with H₀ = 0  
        ''' </para>
        ''' 
        ''' <para>
        ''' This class is a thin wrapper around <c>HotelingsT_single</c>, automatically
        ''' constructing the difference matrix and zero null vector.
        ''' </para>
        ''' </remarks>
        Public Class HotelingsT_paired

            Private data1(,) As Double
            Private data2(,) As Double
            Private pVarNames() As String
            Private pHt As HotelingsT_single

            ''' <summary>
            ''' Initializes the paired Hotelling’s T² test with two multivariate datasets.
            ''' </summary>
            ''' <param name="x1">First dataset (n × p matrix).</param>
            ''' <param name="x2">Second dataset (n × p matrix).</param>
            ''' <param name="varNames">Names of the p variables.</param>
            ''' <exception cref="ArgumentException">
            ''' Thrown if the datasets do not have the same number of rows or columns.
            ''' </exception>
            Public Sub New(x1(,) As Double, x2(,) As Double, varNames() As String)
                Me.data1 = x1
                Me.data2 = x2
                Me.pVarNames = varNames
            End Sub

            ''' <summary>
            ''' Returns formatted <c>ResultTable</c> objects containing:
            ''' <list type="bullet">
            '''   <item><description>Per‑variable mean differences, SEs, individual t‑tests, and simultaneous CIs</description></item>
            '''   <item><description>Overall paired Hotelling’s T² statistic and p‑value</description></item>
            ''' </list>
            ''' </summary>
            ''' <returns>A list of <c>ResultTable</c> objects.</returns>
            Public Function wrapResults() As List(Of ResultTable)
                Return Me.pHt.wrapResults(True)
            End Function

            ''' <summary>
            ''' Computes the paired‑samples Hotelling’s T² statistic by:
            ''' <list type="bullet">
            '''   <item><description>Constructing the paired difference matrix D = X₁ − X₂</description></item>
            '''   <item><description>Passing D to <c>HotelingsT_single</c> with H₀ = 0</description></item>
            '''   <item><description>Returning the resulting T² statistic and p‑value</description></item>
            ''' </list>
            ''' </summary>
            ''' <returns>A <c>TestResult</c> containing T² and its multivariate p‑value.</returns>
            Public Function calculate() As TestResult
                If Me.pHt Is Nothing Then getHT()
                Return Me.pHt.calculate
            End Function

            ''' <summary>
            ''' Computes simultaneous (1 − α) confidence intervals for each variable’s
            ''' paired mean difference using Hotelling’s T² methodology.
            ''' </summary>
            ''' <param name="dAlpha">Significance level α.</param>
            ''' <returns>A list of ConfidenceIntervalResult objects.</returns>
            Public Function CI(dAlpha As Double) As List(Of ConfidenceIntervalResult)
                If Me.pHt Is Nothing Then getHT()
                Return Me.pHt.CI(dAlpha)
            End Function

            ''' <summary>
            ''' Constructs the internal <c>HotelingsT_single</c> instance by:
            ''' <list type="bullet">
            '''   <item><description>Validating that both datasets have identical dimensions</description></item>
            '''   <item><description>Computing the paired difference matrix D = X₁ − X₂</description></item>
            '''   <item><description>Creating a zero null vector H₀ of length p</description></item>
            '''   <item><description>Initializing <c>HotelingsT_single</c> with (D, H₀)</description></item>
            ''' </list>
            ''' </summary>
            ''' <exception cref="ArgumentException">
            ''' Thrown if the datasets differ in number of rows or columns.
            ''' </exception>
            Private Sub getHT()
                If Me.pHt Is Nothing Then
                    Dim n As Integer = data1.GetLength(0)
                    Dim p As Integer = data1.GetLength(1)
                    If n <> data2.GetLength(0) Then
                        AppGlobals.BSerr.LogAndThrow(New ArgumentException("Error: Paired version of Hotelling's T-squared requied The same number of rows in the Input Datasets."))
                    End If
                    If p <> data2.GetLength(1) Then
                        AppGlobals.BSerr.LogAndThrow(New ArgumentException("Error: Paired version of Hotelling's T-squared requied The same number of columns in the Input Datasets."))
                    End If

                    Dim zeros() As Double = Matrix.IdentityVect(p - 1, 0)
                    Dim diff(,) As Double = Matrix.M_SUB(data1, data2)

                    Me.pHt = New HotelingsT_single(diff, zeros, Me.pVarNames)
                End If
            End Sub
        End Class

    End Module
End Namespace
