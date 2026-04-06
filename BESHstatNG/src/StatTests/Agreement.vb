Option Explicit On

Imports System
Imports System.Collections.Generic
Imports BESHStatNG.AppInfrastructure
Imports Microsoft.Office.Interop.Excel


Namespace Agreement

    Public Module Agreement

        ''' <summary>
        ''' Passing–Bablok regression with asymptotic confidence intervals for slope and intercept.
        ''' </summary>
        ''' <remarks>
        ''' <para>
        ''' Passing–Bablok regression is designed for <b>method comparison</b> (agreement) problems where <c>x</c> and <c>y</c>
        ''' are two measurement procedures applied to the same samples, and both are subject to random measurement error.
        ''' </para>
        '''
        ''' <para>
        ''' The estimated relationship is:
        ''' </para>
        ''' <para>
        ''' <c>y = a + b x</c>
        ''' </para>
        '''
        ''' <h3>Estimator definition (slope and intercept)</h3>
        ''' <para>
        ''' For all pairs <c>i &lt; j</c>, define the pairwise slopes:
        ''' </para>
        ''' <para>
        ''' <c>sᵢⱼ = (yᵢ − yⱼ) / (xᵢ − xⱼ)</c>.
        ''' </para>
        '''
        ''' <para>
        ''' This implementation follows the common Passing–Bablok construction:
        ''' </para>
        ''' <list type="bullet">
        '''   <item>
        '''     <description>
        '''       Pairs with identical points (<c>xᵢ=xⱼ</c> and <c>yᵢ=yⱼ</c>) are ignored.
        '''     </description>
        '''   </item>
        '''   <item>
        '''     <description>
        '''       Pairs with <c>xᵢ=xⱼ</c> but <c>yᵢ≠yⱼ</c> contribute slopes of <c>±∞</c>, preserving the ordering information.
        '''     </description>
        '''   </item>
        '''   <item>
        '''     <description>
        '''       Pairwise slopes exactly equal to <c>−1</c> are excluded (as in the classic Passing–Bablok algorithmic form).
        '''     </description>
        '''   </item>
        ''' </list>
        '''
        ''' <para>
        ''' Let <c>S</c> be the sorted list of all retained slopes and let <c>N</c> be its length. Let
        ''' <c>K</c> be the number of slopes with <c>s &lt; −1</c>. The slope estimate <c>b</c> is computed as a
        ''' <b>shifted median</b>:
        ''' </para>
        ''' <list type="bullet">
        '''   <item>
        '''     <description>
        '''       If <c>N</c> is odd: <c>b = S[((N+1)/2) + K]</c> (1-based indexing; shifted by <c>K</c>).
        '''     </description>
        '''   </item>
        '''   <item>
        '''     <description>
        '''       If <c>N</c> is even: <c>b = ( S[(N/2)+K] + S[(N/2)+K+1] ) / 2</c> (1-based indexing).
        '''     </description>
        '''   </item>
        ''' </list>
        '''
        ''' <para>
        ''' After estimating <c>b</c>, the intercept is:
        ''' </para>
        ''' <para>
        ''' <c>a = median( yᵢ − b xᵢ )</c>.
        ''' </para>
        '''
        ''' <h3>Asymptotic confidence intervals</h3>
        ''' <para>
        ''' The CI construction implemented here is the rank-based asymptotic approach traditionally associated with Passing–Bablok.
        ''' Let <c>z = Φ⁻¹(1 − α/2)</c> be the standard normal quantile (computed by <c>NormSInv</c>), and define:
        ''' </para>
        ''' <para>
        ''' <c>σ = sqrt( n(n−1)(2n+5) / 18 )</c>,
        ''' </para>
        ''' <para>
        ''' where <c>n</c> is the number of observations (not the number of slopes).
        ''' Then <c>Cγ = z·σ</c>.
        ''' </para>
        '''
        ''' <para>
        ''' Let <c>M1 = round( (N − Cγ) / 2 )</c> and <c>M2 = N − M1 + 1</c>.
        ''' (This code uses banker's rounding: <c>MidpointRounding.ToEven</c>, matching NumPy's default <c>round</c> behavior.)
        ''' The slope CI is taken from the ordered slopes:
        ''' </para>
        ''' <para>
        ''' <c>b_L = S[M1 + K]</c>, <c>b_U = S[M2 + K]</c> (conceptually 1-based; see code for 0-based index adjustment).
        ''' </para>
        '''
        ''' <para>
        ''' The intercept CI is constructed by recomputing the median residual term at the CI endpoints:
        ''' </para>
        ''' <para>
        ''' <c>a_L = median( yᵢ − b_U xᵢ )</c>, and <c>a_U = median( yᵢ − b_L xᵢ )</c>.
        ''' </para>
        '''
        ''' <h3>Interpretation in method comparison</h3>
        ''' <para>
        ''' In typical method-comparison interpretation:
        ''' </para>
        ''' <list type="bullet">
        '''   <item><description>If <c>0</c> lies in the CI for <c>a</c>, there is no evidence of a constant (systematic) bias.</description></item>
        '''   <item><description>If <c>1</c> lies in the CI for <c>b</c>, there is no evidence of a proportional bias.</description></item>
        ''' </list>
        '''
        ''' <h3>References</h3>
        ''' <list type="bullet">
        '''   <item><description>Passing &amp; Bablok (1983), Part I (definition, estimation, CI concept).</description></item>
        '''   <item><description>Passing &amp; Bablok (1984), Part II (properties and comparisons; sample-size considerations).</description></item>
        '''   <item><description>Bilić-Zulle (2011) (practical guide and interpretation).</description></item>
        '''   <item><description>CLSI EP09-A3 (2013) (method-comparison practice guidance; interpretation context).</description></item>
        ''' </list>
        ''' </remarks>
        Public Class PassinbBablok

            ''' <summary>
            ''' X measurements (method/procedure 1).
            ''' </summary>
            ''' <remarks>
            ''' Must have the same length as <see cref="y"/>. Both variables are treated as measured with error.
            ''' </remarks>
            Private x As Double()

            ''' <summary>
            ''' Y measurements (method/procedure 2).
            ''' </summary>
            ''' <remarks>
            ''' Must have the same length as <see cref="x"/>. Both variables are treated as measured with error.
            ''' </remarks>
            Private y As Double()

            Private groups As Object() = Nothing
            Private pInterceptCI As New ConfidenceIntervalResult
            Private pSlopeCI As New ConfidenceIntervalResult
            Private pVarX As String
            Private pVarY As String
            Private pVarGrp As String = String.Empty
            Private pNoGroups As Integer
            Private pMinGroupSize As Integer
            Private pMaxGroupSize As Integer
            Private pMeanGroupSize As Double

            ''' <summary>
            ''' Two-sided significance level used to construct the reported confidence intervals.
            ''' </summary>
            ''' <remarks>
            ''' The reported interval level is <c>100 × (1 − alpha)%</c>.
            ''' The default is <c>0.05</c>, corresponding to a 95% confidence interval.
            ''' </remarks>
            Public alpha As Double = 0.05

            ''' <summary>
            ''' Initializes a Passing–Bablok regression instance using paired measurement vectors.
            ''' </summary>
            ''' <param name="dataX">X measurement vector (method 1).</param>
            ''' <param name="dataY">Y measurement vector (method 2).</param>
            ''' <exception cref="ArgumentNullException">Thrown if <paramref name="dataX"/> or <paramref name="dataY"/> is <c>Nothing</c>.</exception>
            ''' <exception cref="ArgumentException">Thrown if lengths differ or fewer than 2 observations are provided.</exception>
            ''' <remarks>
            ''' The constructor validates that <paramref name="dataX"/> and <paramref name="dataY"/> are non-null, same length,
            ''' and contain at least two paired observations.
            ''' </remarks>
            Public Sub New(dataX() As Double, dataY() As Double, varX As String, varY As String,
                           Optional dataGroups As Object() = Nothing, Optional varGrp As String = "")
                Me.x = dataX
                Me.y = dataY
                Me.pVarX = varX
                Me.pVarY = varY
                Me.pVarGrp = varGrp
                Me.groups = dataGroups
                Me.ValidateXY(x, y)
                If dataGroups IsNot Nothing Then
                    If groups.Length <> x.Length Then Throw New ArgumentException("groups must have the same length as x and y.", NameOf(groups))
                    'get gourps information
                    Me.GetGroupCounts(Me.groups)
                End If
            End Sub

            Private Sub GetGroupCounts(arr() As Object)
                Dim gg = arr.GroupBy(Function(x) x)
                Dim counts = gg.Select(Function(g) g.Count()).ToList()
                If counts.Count = 0 Then AppGlobals.BSerr.LogAndThrow(New InvalidOperationException("No groups found"))

                Me.pNoGroups = counts.Count
                Me.pMinGroupSize = counts.Min()
                Me.pMaxGroupSize = counts.Max()
                Me.pMeanGroupSize = counts.Average(Function(c) CDbl(c))
            End Sub

            Public Function wrapResults() As List(Of ResultTable)
                Dim out = New List(Of ResultTable)
                Dim t = New ResultTable
                t.SetBody({{"Test method", Me.pVarY},
                           {"Reference method", Me.pVarX},
                           {"Sample size", Me.x.Length}})
                t.AddTitle("Method Comparison Regression")
                out.Add(t)


                t = New ResultTable
                t.AddTitle("Passing-Bablok")
                t.SetBody({{Me.pSlopeCI.Estimate, Me.pSlopeCI.strConfidenceInterval(CIformat.LL_to_UL), "Proportional differences"},
                           {Me.pInterceptCI.Estimate, Me.pInterceptCI.strConfidenceInterval(CIformat.LL_to_UL), "Systematic differences"}})
                t.AddHeaderLeftRow({"Slope", "Intercept"})
                t.AddHeaderTopRow({"Estimate", Me.pSlopeCI.CIlabel, "Meaning"})
                If Me.groups IsNot Nothing Then t.AddFootnote($"Group var = {Me.pVarGrp}")
                out.Add(t)

                If Me.groups IsNot Nothing Then
                    t = New ResultTable
                    t.AddTitle("Groups Information")
                    t.SetBody({{"Number of Groups", Me.pNoGroups},
                               {"Min Group Size", Me.pMinGroupSize},
                               {"Max Group Size", Me.pMaxGroupSize},
                               {"Mean Group Size", Me.pMeanGroupSize}})
                    out.Add(t)
                End If

                Return out
            End Function

            ''' <summary>
            ''' Computes Passing–Bablok point estimates and asymptotic confidence intervals for intercept and slope.
            ''' </summary>
            ''' <returns>
            ''' A tuple <c>(InterceptCI, SlopeCI)</c> where:
            ''' <list type="bullet">
            '''   <item><description><c>InterceptCI</c> contains <c>a</c>, <c>a_L</c>, <c>a_U</c></description></item>
            '''   <item><description><c>SlopeCI</c> contains <c>b</c>, <c>b_L</c>, <c>b_U</c></description></item>
            ''' </list>
            ''' </returns>
            ''' <exception cref="InvalidOperationException">Thrown if no valid pairwise slopes can be computed.</exception>
            ''' <remarks>
            ''' <para>
            ''' This is the ungrouped Passing–Bablok procedure (all pairs <c>i&lt;j</c> are eligible).
            ''' See the class-level remarks for the full mathematical definition and CI construction.
            ''' </para>
            ''' <para>
            ''' Numerical notes:
            ''' </para>
            ''' <list type="bullet">
            '''   <item><description>Slopes are stored in a list and sorted; complexity is <c>O(n² log n)</c> in this straightforward implementation.</description></item>
            '''   <item><description><c>±Infinity</c> slopes can occur when <c>xᵢ=xⱼ</c>; these influence the order statistic-based median.</description></item>
            '''   <item><description>The CI index calculations use banker's rounding to match NumPy’s default rounding behavior.</description></item>
            ''' </list>
            ''' </remarks>
            Public Function PassingBablokCI() As (InterceptCI As ConfidenceIntervalResult, SlopeCI As ConfidenceIntervalResult)

                Dim slopes As List(Of Double) = BuildSlopes(x, y, groups:=Nothing, useGroups:=False)
                If slopes Is Nothing OrElse slopes.Count = 0 Then Throw New InvalidOperationException("No valid pairwise slopes could be computed.")

                slopes.Sort()
                Dim S() As Double = slopes.ToArray()
                Dim N As Integer = S.Length

                ' K = number of slopes < -1
                Dim K As Integer = 0
                For i As Integer = 0 To N - 1
                    If S(i) < -1.0 Then
                        K += 1
                    End If
                Next

                ' Shifted median slope 
                Dim b As Double
                If (N Mod 2) <> 0 Then
                    ' odd: idx = (N+1)/2 + K  (1-based), then -1 for 0-based
                    Dim idx0 As Integer = ((N + 1) \ 2) + K - 1
                    b = S(idx0)
                Else
                    ' even: idx = N/2 + K (1-based), then -1 for 0-based; average idx and idx+1
                    Dim idx0 As Integer = (N \ 2) + K - 1
                    b = 0.5 * (S(idx0) + S(idx0 + 1))
                End If

                ' a = median(y - b*x)
                Dim a As Double = MedianOfResiduals(y, x, b)

                ' w = z_{1 - alpha/2}
                Dim q As Double = 1.0 - (alpha / 2.0)
                Dim w As Double = distributions.NormSInv(q)

                ' sigma = sqrt(n(n-1)(2n+5)/18)
                Dim nObs As Integer = x.Length
                Dim sigma As Double = Math.Sqrt((nObs * (nObs - 1.0) * (2.0 * nObs + 5.0)) / 18.0)

                Dim Cgamma As Double = w * sigma

                ' M1 = round((N - Cgamma)/2), M2 = N - M1 + 1    (pb.py / numpy.round uses bankers rounding)
                Dim M1 As Double = Math.Round((N - Cgamma) / 2.0, 0, MidpointRounding.ToEven)
                Dim M2 As Double = N - M1 + 1.0

                ' Indices for CI bounds (pb.py: S[int(M1)+K-1], S[int(M2)+K-1])
                Dim idxL As Integer = CInt(Math.Truncate(M1)) + K - 1
                Dim idxU As Integer = CInt(Math.Truncate(M2)) + K - 1

                If idxL < 0 OrElse idxL >= N OrElse idxU < 0 OrElse idxU >= N Then
                    Throw New InvalidOperationException("Computed CI indices are out of range. Check inputs / alpha.")
                End If

                Dim bL As Double = S(idxL)
                Dim bU As Double = S(idxU)

                ' a_L = median(y - b_U*x), a_U = median(y - b_L*x)
                Dim aL As Double = MedianOfResiduals(y, x, bU)
                Dim aU As Double = MedianOfResiduals(y, x, bL)

                Me.pInterceptCI = New ConfidenceIntervalResult With {
                    .Estimate = a,
                    .alpha = Me.alpha,
                    .LowerLimit = aL,
                    .UpperLimit = aU}

                Me.pSlopeCI = New ConfidenceIntervalResult With {
                    .Estimate = b,
                    .alpha = Me.alpha,
                    .LowerLimit = bL,
                    .UpperLimit = bU}

                Return (Me.pInterceptCI, Me.pSlopeCI)
            End Function

            ''' <summary>
            ''' Computes grouped (block) Passing–Bablok asymptotic confidence intervals by excluding within-group pairs.
            ''' </summary>
            ''' <returns>
            ''' A tuple <c>(InterceptCI, SlopeCI)</c> where:
            ''' <list type="bullet">
            '''   <item><description><c>InterceptCI</c> contains <c>a</c>, <c>a_L</c>, <c>a_U</c></description></item>
            '''   <item><description><c>SlopeCI</c> contains <c>b</c>, <c>b_L</c>, <c>b_U</c></description></item>
            ''' </list>
            ''' </returns>
            ''' <exception cref="ArgumentNullException">Thrown if groups is <c>Nothing</c>.</exception>
            ''' <exception cref="InvalidOperationException">Thrown if no valid cross-group slopes can be computed.</exception>
            ''' <remarks>
            ''' <para>
            ''' This method implements a <b>block/grouped</b> variant by computing pairwise slopes only between observations belonging to
            ''' <b>different</b> groups (e.g., repeated measurements per patient/sample). This is motivated by the grouped-data setting where
            ''' within-group pairs can introduce bias if both variables are noisy and the repeated measurements are not independent.
            ''' </para>
            '''
            ''' <para>
            ''' The slope and intercept estimators are computed exactly as in the ungrouped case, but with the slope set restricted to
            ''' cross-group pairs. The confidence interval construction in this implementation mirrors the same rank-based asymptotic CI structure
            ''' used in the ungrouped procedure (see <see cref="PassingBablokCI"/>).
            ''' </para>
            '''
            ''' <h3>Reference</h3>
            ''' <para>
            ''' For the grouped-data theory and the term <i>Block–Passing–Bablok</i>, see:
            ''' </para>
            ''' <para>
            ''' Baumdicker, F. et al. (2020) / preprint (2019), <i>Passing–Bablok regression for grouped data with errors in both variables</i>.
            ''' </para>
            ''' </remarks>
            Public Function GroupedBlockPassingBablok() As (InterceptCI As ConfidenceIntervalResult, SlopeCI As ConfidenceIntervalResult)

                If groups Is Nothing Then Throw New ArgumentNullException(NameOf(groups))
                Dim slopes As List(Of Double) = BuildSlopes(x, y, groups, useGroups:=True)
                If slopes.Count = 0 Then Throw New InvalidOperationException("No valid cross-group pairwise slopes could be computed.")

                slopes.Sort()
                Dim S() As Double = slopes.ToArray()
                Dim N As Integer = S.Length

                Dim K As Integer = 0
                For i As Integer = 0 To N - 1
                    If S(i) < -1.0 Then
                        K += 1
                    End If
                Next

                Dim b As Double
                If (N Mod 2) <> 0 Then
                    Dim idx0 As Integer = ((N + 1) \ 2) + K - 1
                    b = S(idx0)
                Else
                    Dim idx0 As Integer = (N \ 2) + K - 1
                    b = 0.5 * (S(idx0) + S(idx0 + 1))
                End If

                Dim a As Double = MedianOfResiduals(y, x, b)

                Dim q As Double = 1.0 - (alpha / 2.0)
                Dim w As Double = distributions.NormSInv(q)

                ' --- Grouped (Block) Passing–Bablok: variance correction for C~ (conservative) ---
                ' Baumdicker & Hölker (2020), Eq. (7):
                '   V[C~] = (1/18) * ( n(n-1)(2n+5) - Σ_k p_k(p_k-1)(2p_k+5) )
                ' This is exact for non-overlapping groups on the x-axis and conservative when groups overlap.

                Dim nObs As Integer = x.Length

                ' group sizes p_k
                Dim groupCounts As Integer() = groups.GroupBy(Function(g) g).Select(Function(grp) grp.Count()).ToArray()

                Dim sumWithin As Double = 0.0
                For Each pk As Integer In groupCounts
                    sumWithin += pk * (pk - 1.0) * (2.0 * pk + 5.0)
                Next

                Dim varC As Double = (nObs * (nObs - 1.0) * (2.0 * nObs + 5.0) - sumWithin) / 18.0

                ' guard against tiny negative values due to floating point rounding
                If varC < 0.0 Then varC = 0.0

                Dim sigma As Double = Math.Sqrt(varC)
                ' -------------------------------------------------------------------------------


                Dim Cgamma As Double = w * sigma
                Dim M1 As Double = Math.Round((N - Cgamma) / 2.0, 0, MidpointRounding.ToEven)
                Dim M2 As Double = N - M1 + 1.0

                Dim idxL As Integer = CInt(Math.Truncate(M1)) + K - 1
                Dim idxU As Integer = CInt(Math.Truncate(M2)) + K - 1

                If idxL < 0 OrElse idxL >= N OrElse idxU < 0 OrElse idxU >= N Then
                    Throw New InvalidOperationException("Computed CI indices are out of range. Check inputs / alpha.")
                End If

                Dim bL As Double = S(idxL)
                Dim bU As Double = S(idxU)

                Dim aL As Double = MedianOfResiduals(y, x, bU)
                Dim aU As Double = MedianOfResiduals(y, x, bL)

                Me.pInterceptCI = New ConfidenceIntervalResult With {
                    .Estimate = a,
                    .alpha = Me.alpha,
                    .LowerLimit = aL,
                    .UpperLimit = aU}

                Me.pSlopeCI = New ConfidenceIntervalResult With {
                    .Estimate = b,
                    .alpha = Me.alpha,
                    .LowerLimit = bL,
                    .UpperLimit = bU}

                Return (Me.pInterceptCI, Me.pSlopeCI)
            End Function

            Public Sub AddPlot(ws As Worksheet)
                Dim ch = graphics.GeneralScatterPlot(Me.x, Me.y, Me.pVarY, Me.pVarX, ws, "Passing-Bablok Regression")
                Dim dMinX As Double = Me.x.Min()
                Dim dMaxX As Double = Me.x.Max()

                If Me.pInterceptCI Is Nothing Then Me.PassingBablokCI()

                With ch
                    .HasLegend = False
                    .HasLegend = True

                    'add and plot fit line
                    .SeriesCollection.NewSeries
                    With .SeriesCollection(2)
                        .XValues = {dMinX, dMaxX}
                        .Values = {Me.pInterceptCI.Estimate + Me.pSlopeCI.Estimate * dMinX,
                                   Me.pInterceptCI.Estimate + Me.pSlopeCI.Estimate * dMaxX}
                        .Name = "Regression line"
                        .MarkerStyle = -4142
                        .Border.Color = RGB(255, 0, 0)
                        With .Format.Line
                            .Visible = True
                            .Weight = 1.5
                        End With
                    End With

                    'Zero x=y refline
                    .SeriesCollection.NewSeries
                    With .SeriesCollection(3)
                        .XValues = {dMinX, dMaxX}
                        .Values = {dMinX, dMaxX}
                        .Name = "Unity line (y = x)"
                        .MarkerStyle = -4142 'no marker
                        .Border.Color = RGB(0, 0, 255)
                        With .Format.Line
                            .Visible = True
                            .DashStyle = 4 'msoLineDash
                            .Weight = 0.5
                        End With
                    End With
                End With
            End Sub

            ' ----------------------------
            ' Helpers (internal)
            ' ----------------------------
            ''' <summary>
            ''' Validates that <paramref name="x"/> and <paramref name="y"/> are non-null, have equal length,
            ''' and contain at least two paired observations.
            ''' </summary>
            ''' <param name="x">X measurement vector.</param>
            ''' <param name="y">Y measurement vector.</param>
            ''' <exception cref="ArgumentNullException">Thrown if <paramref name="x"/> or <paramref name="y"/> is <c>Nothing</c>.</exception>
            ''' <exception cref="ArgumentException">Thrown if lengths differ or fewer than 2 observations are provided.</exception>
            Private Sub ValidateXY(x As Double(), y As Double())
                If x Is Nothing Then Throw New ArgumentNullException(NameOf(x))
                If y Is Nothing Then Throw New ArgumentNullException(NameOf(y))
                If x.Length <> y.Length Then Throw New ArgumentException("x and y must have the same length.")
                If x.Length < 2 Then Throw New ArgumentException("x and y must contain at least two observations.")
            End Sub

            ''' <summary>
            ''' Builds the list of pairwise slopes used by Passing–Bablok.
            ''' </summary>
            ''' <param name="x">X vector.</param>
            ''' <param name="y">Y vector.</param>
            ''' <param name="groups">
            ''' Optional group labels. Used only when <paramref name="useGroups"/> is <c>True</c>.
            ''' </param>
            ''' <param name="useGroups">
            ''' If <c>True</c>, includes only cross-group pairs where <c>groups(i)≠groups(j)</c>.
            ''' If <c>False</c>, includes all pairs <c>i&lt;j</c>.
            ''' </param>
            ''' <returns>
            ''' A list of slopes <c>sᵢⱼ</c> for eligible pairs, including <c>±Infinity</c> for <c>x</c>-ties, excluding
            ''' identical points and excluding slopes exactly equal to <c>−1</c>.
            ''' </returns>
            ''' <remarks>
            ''' <para>
            ''' For eligible pairs <c>i&lt;j</c>:
            ''' </para>
            ''' <para>
            ''' <c>sᵢⱼ = (yᵢ − yⱼ) / (xᵢ − xⱼ)</c>.
            ''' </para>
            ''' <para>
            ''' If <c>xᵢ = xⱼ</c> and <c>yᵢ ≠ yⱼ</c>, the slope is represented as <c>+∞</c> if <c>yᵢ &gt; yⱼ</c>,
            ''' else <c>−∞</c>. This preserves ordering information when sorting the slopes.
            ''' </para>
            ''' <para>
            ''' Slopes exactly equal to <c>−1</c> are excluded to match the classic Passing–Bablok algorithmic specification.
            ''' </para>
            ''' </remarks>
            Private Function BuildSlopes(x As Double(), y As Double(),
                                     groups As Object(), useGroups As Boolean) As List(Of Double)

                Dim S As New List(Of Double)(capacity:=Math.Max(16, x.Length * (x.Length - 1) \ 2))
                Dim n As Integer = x.Length
                For i As Integer = 0 To n - 2
                    For j As Integer = i + 1 To n - 1

                        If useGroups Then
                            If Object.Equals(groups(i), groups(j)) Then
                                Continue For
                            End If
                        End If

                        Dim yi As Double = y(i)
                        Dim yj As Double = y(j)
                        Dim xi As Double = x(i)
                        Dim xj As Double = x(j)

                        ' Ignore identical points
                        If (yi = yj) AndAlso (xi = xj) Then
                            Continue For
                        End If

                        ' Avoid division by zero: x ties become ±Infinity depending on y order (pb.py)
                        If xi = xj Then
                            If yi > yj Then
                                S.Add(Double.PositiveInfinity)
                            Else
                                S.Add(Double.NegativeInfinity)
                            End If
                            Continue For
                        End If

                        Dim gradient As Double = (yi - yj) / (xi - xj)

                        ' Ignore gradient exactly equal to -1 (pb.py)
                        If gradient = -1.0 Then
                            Continue For
                        End If

                        S.Add(gradient)
                    Next
                Next

                Return S
            End Function

            ''' <summary>
            ''' Computes the intercept estimate as the median of <c>yᵢ − b xᵢ</c> for a given slope <paramref name="b"/>.
            ''' </summary>
            ''' <param name="y">Y vector.</param>
            ''' <param name="x">X vector.</param>
            ''' <param name="b">Slope value.</param>
            ''' <returns><c>median( yᵢ − b xᵢ )</c>.</returns>
            ''' <remarks>
            ''' This function builds a new residual-like array and then computes its median without mutating the original inputs.
            ''' </remarks>
            Private Function MedianOfResiduals(y As Double(), x As Double(), b As Double) As Double
                Dim n As Integer = y.Length
                Dim r(n - 1) As Double
                For i As Integer = 0 To n - 1
                    r(i) = y(i) - b * x(i)
                Next
                Return MedianCopy(r)
            End Function

            ''' <summary>
            ''' Returns the median of the provided array, sorting a copy (leaving the input unchanged).
            ''' </summary>
            ''' <param name="values">Input values.</param>
            ''' <returns>
            ''' The sample median: for odd <c>n</c>, the middle order statistic; for even <c>n</c>, the average of the two middle order statistics.
            ''' </returns>
            ''' <remarks>
            ''' Median definition:
            ''' <list type="bullet">
            '''   <item><description>If <c>n</c> is odd: <c>median = x[(n+1)/2]</c> (1-based).</description></item>
            '''   <item><description>If <c>n</c> is even: <c>median = (x[n/2] + x[n/2+1]) / 2</c> (1-based).</description></item>
            ''' </list>
            ''' </remarks>
            Private Function MedianCopy(values As Double()) As Double
                If values Is Nothing Then Throw New ArgumentNullException(NameOf(values))
                If values.Length = 0 Then Return Double.NaN

                Dim tmp As Double() = CType(values.Clone(), Double())
                Array.Sort(tmp)

                Dim n As Integer = tmp.Length
                If (n Mod 2) <> 0 Then
                    Return tmp(n \ 2)
                Else
                    Dim j As Integer = n \ 2
                    Return 0.5 * (tmp(j - 1) + tmp(j))
                End If
            End Function

        End Class





        ''' <summary>
        ''' Deming regression (errors-in-variables linear regression) for method-comparison studies.
        ''' Fits y = a + b x when both x and y are measured with error.
        ''' </summary>
        ''' <remarks>
        ''' <para>
        ''' Deming regression assumes measurement errors in both variables with known (or assumed) variance ratio
        ''' λ = σy² / σx². For λ = 1 it becomes orthogonal regression.
        ''' </para>
        ''' <para>
        ''' Point estimates (a,b) are computed using the classic closed-form Deming solution.
        ''' Standard errors and confidence intervals in this class are computed by the jackknife (leave-one-out) method.
        ''' This corresponds to MCR's method.ci="jackknife" workflow and is commonly recommended for Deming-type CIs.
        ''' </para>
        ''' </remarks>
        Public Class DemingRegression

            Private ReadOnly x As Double()
            Private ReadOnly y As Double()

            ' --- Results storage (optional, similar to your PB class style) ---
            Private pInterceptCI As ConfidenceIntervalResult
            Private pSlopeCI As ConfidenceIntervalResult
            Private pInterceptSE As Double = Double.NaN
            Private pSlopeSE As Double = Double.NaN
            Private pVarX As String
            Private pVarY As String
            Private pCItype As String = Nothing

            ''' <summary>
            ''' Two-sided significance level used to construct the reported confidence intervals.
            ''' The default is <c>0.05</c>, corresponding to a 95% confidence interval.
            ''' </summary>
            Public Property alpha As Double = 0.05

            ''' <summary>
            ''' Error variance ratio λ = σx² / σy² (reference over test).
            ''' This matches the <c>error.ratio</c> argument used by the R package <c>mcr</c>.
            ''' Default λ = 1 (orthogonal regression).
            ''' </summary>
            ''' <remarks>
            ''' Many textbooks write Deming regression in terms of δ = σy²/σx².
            ''' This implementation uses λ = 1/δ internally: δ = 1 / Lambda.
            ''' </remarks>
            Public Property Lambda As Double = 1.0


            ''' <summary>Last computed intercept CI (if <see cref="FitJackknifeCI"/> has been called).</summary>
            Public ReadOnly Property InterceptCI As ConfidenceIntervalResult
                Get
                    Return pInterceptCI
                End Get
            End Property

            ''' <summary>Last computed slope CI (if <see cref="FitJackknifeCI"/> has been called).</summary>
            Public ReadOnly Property SlopeCI As ConfidenceIntervalResult
                Get
                    Return pSlopeCI
                End Get
            End Property

            ''' <summary>Jackknife SE of intercept (computed by <see cref="FitJackknifeCI"/>).</summary>
            Public ReadOnly Property InterceptSE As Double
                Get
                    Return pInterceptSE
                End Get
            End Property

            ''' <summary>Jackknife SE of slope (computed by <see cref="FitJackknifeCI"/>).</summary>
            Public ReadOnly Property SlopeSE As Double
                Get
                    Return pSlopeSE
                End Get
            End Property

            Public Sub New(dataX As Double(), dataY As Double(), varX As String, varY As String)
                If dataX Is Nothing Then AppGlobals.BSerr.LogAndThrow(New ArgumentNullException(NameOf(dataX)))
                If dataY Is Nothing Then AppGlobals.BSerr.LogAndThrow(New ArgumentNullException(NameOf(dataY)))
                If dataX.Length <> dataY.Length Then AppGlobals.BSerr.LogAndThrow(New ArgumentException("x and y must have the same length."))
                If dataX.Length < 3 Then AppGlobals.BSerr.LogAndThrow(New ArgumentException("At least 3 paired observations are required."))

                x = CType(dataX.Clone(), Double())
                y = CType(dataY.Clone(), Double())
                Me.pVarX = varX
                Me.pVarY = varY
            End Sub

            Public Function wrapResults() As List(Of ResultTable)
                Dim out = New List(Of ResultTable)
                Dim t = New ResultTable
                t.SetBody({{"Test method", Me.pVarY},
                           {"Reference method", Me.pVarX},
                           {"Sample size", Me.x.Length},
                           {"Error Ratio", Me.Lambda}})
                t.AddTitle("Method Comparison Regression")
                out.Add(t)


                t = New ResultTable
                t.AddTitle("Deming Regression")
                t.SetBody({{Me.pSlopeCI.Estimate, Me.pSlopeCI.strConfidenceInterval(CIformat.LL_to_UL), "Proportional differences"},
                           {Me.pInterceptCI.Estimate, Me.pInterceptCI.strConfidenceInterval(CIformat.LL_to_UL), "Systematic differences"}})
                t.AddHeaderLeftRow({"Slope", "Intercept"})
                t.AddHeaderTopRow({"Estimate", Me.pSlopeCI.CIlabel, "Meaning"})
                t.AddFootnote($"SE type = {Me.pCItype}")
                out.Add(t)

                Return out
            End Function

            ''' <summary>
            ''' Computes Deming point estimates (a,b) for the current data and <see cref="Lambda"/>.
            ''' </summary>
            Public Function FitPointEstimate() As (Intercept As Double, Slope As Double)
                Return ComputeDemingCoefficients(x, y, Lambda)
            End Function

            ''' <summary>
            ''' Fits Deming regression and returns (InterceptCI, SlopeCI) using jackknife SE and t-based CI.
            ''' </summary>
            ''' <param name="dfForTCrit">
            ''' Degrees of freedom used for the t critical value. If omitted, uses n-2 (common in linear regression-style reporting).
            ''' </param>
            ''' <returns>Tuple(InterceptCI, SlopeCI).</returns>
            Public Function FitJackknifeCI(Optional dfForTCrit As Integer? = Nothing) As (InterceptCI As ConfidenceIntervalResult, SlopeCI As ConfidenceIntervalResult)

                If Lambda <= 0.0 OrElse Double.IsNaN(Lambda) OrElse Double.IsInfinity(Lambda) Then AppGlobals.BSerr.LogAndThrow(New ArgumentOutOfRangeException(NameOf(Lambda), "Lambda must be finite and > 0."))
                If alpha <= 0.0 OrElse alpha >= 1.0 Then AppGlobals.BSerr.LogAndThrow(New ArgumentOutOfRangeException(NameOf(alpha), "alpha must be in (0,1)."))

                Dim n As Integer = x.Length

                ' --- Global estimates ---
                Dim glob = ComputeDemingCoefficients(x, y, Lambda)
                Dim aHat As Double = glob.Intercept
                Dim bHat As Double = glob.Slope

                ' --- Jackknife leave-one-out estimates ---
                Dim aJack(n - 1) As Double, bJack(n - 1) As Double

                Dim xx(n - 2) As Double, yy(n - 2) As Double

                For i As Integer = 0 To n - 1
                    ' Build leave-one-out sample
                    Dim t As Integer = 0
                    For k As Integer = 0 To n - 1
                        If k = i Then Continue For
                        xx(t) = x(k)
                        yy(t) = y(k)
                        t += 1
                    Next

                    Dim est = ComputeDemingCoefficients(xx, yy, Lambda)
                    aJack(i) = est.Intercept
                    bJack(i) = est.Slope
                Next

                ' --- Jackknife SE: sqrt((n-1)/n * Σ (θ_i - θ_dot)^2 ) ---
                Dim aBar As Double = aJack.Average()
                Dim bBar As Double = bJack.Average()

                Dim ssa As Double = 0.0
                Dim ssb As Double = 0.0
                For i As Integer = 0 To n - 1
                    Dim da As Double = aJack(i) - aBar
                    Dim db As Double = bJack(i) - bBar
                    ssa += da * da
                    ssb += db * db
                Next

                Dim seA As Double = Math.Sqrt(((n - 1.0) / n) * ssa)
                Dim seB As Double = Math.Sqrt(((n - 1.0) / n) * ssb)

                pInterceptSE = seA
                pSlopeSE = seB

                ' --- t critical ---
                Dim df As Integer = If(dfForTCrit.HasValue, dfForTCrit.Value, n - 2)
                If df < 1 Then df = 1

                Dim tCrit As Double = distributions.T_Inv(1.0 - alpha / 2.0, df)

                Dim aL As Double = aHat - tCrit * seA
                Dim aU As Double = aHat + tCrit * seA
                Dim bL As Double = bHat - tCrit * seB
                Dim bU As Double = bHat + tCrit * seB

                pInterceptCI = New ConfidenceIntervalResult With {.Estimate = aHat, .alpha = Me.alpha, .StdErr = seA, .LowerLimit = aL, .UpperLimit = aU}
                pSlopeCI = New ConfidenceIntervalResult With {.Estimate = bHat, .alpha = Me.alpha, .StdErr = seB, .LowerLimit = bL, .UpperLimit = bU}
                Me.pCItype = "Jackknife"

                Return (pInterceptCI, pSlopeCI)
            End Function

            ''' <summary>
            ''' Computes Deming-regression slope/intercept and the corresponding "analytical" (Wald) confidence intervals
            ''' in the same spirit as the MCR R package's <c>method.ci="analytical"</c>.
            ''' </summary>
            ''' <returns>
            ''' Tuple of <see cref="ConfidenceIntervalResult"/>:
            ''' <list type="bullet">
            ''' <item><description>Item1: intercept CI result</description></item>
            ''' <item><description>Item2: slope CI result</description></item>
            ''' </list>
            ''' Each <see cref="ConfidenceIntervalResult"/> contains <c>Estimate</c>, <c>LowerLimit</c>, <c>UpperLimit</c>.
            ''' </returns>
            ''' <remarks>
            ''' <para>
            ''' <b>Deming regression model.</b><br/>
            ''' We assume a linear relationship between the true values <c>X*</c>, <c>Y*</c>:
            ''' <c>Y* = β0 + β1 X*</c>.
            ''' Observed values contain measurement error in both axes:
            ''' <c>x = X* + εx</c>, <c>y = Y* + εy</c>,
            ''' with <c>Var(εx) = σx²</c>, <c>Var(εy) = σy²</c>, and known ratio <c>λ = σy²/σx²</c>.
            ''' </para>
            '''
            ''' <para>
            ''' <b>Point estimates (closed form).</b><br/>
            ''' Let <c>x̄</c>, <c>ȳ</c> be sample means and define sample (co)variances:
            ''' <c>Sxx = Σ(xi−x̄)²/(n−1)</c>, <c>Syy = Σ(yi−ȳ)²/(n−1)</c>, <c>Sxy = Σ(xi−x̄)(yi−ȳ)/(n−1)</c>.
            ''' The Deming slope is
            ''' <c>
            ''' β1̂ = ( Syy − λ Sxx + √( (Syy − λ Sxx)² + 4 λ Sxy² ) ) / ( 2 Sxy )
            ''' </c>
            ''' (choose the "+" root to obtain the solution with the same sign as <c>Sxy</c> in typical method-comparison use).
            ''' Intercept: <c>β0̂ = ȳ − β1̂ x̄</c>.
            ''' </para>
            '''
            ''' <para>
            ''' <b>Objective function.</b><br/>
            ''' Deming regression minimizes
            ''' <c>Q(β0,β1) = Σ ri² / (λ + β1²)</c> where <c>ri = yi − β0 − β1 xi</c>.
            ''' This is equivalent to nonlinear least squares of <c>ui = ri / √(λ + β1²)</c>, i.e. <c>Q = Σ ui²</c>.
            ''' </para>
            '''
            ''' <para>
            ''' <b>Analytical standard errors.</b><br/>
            ''' We estimate the residual variance as <c>s² = Q(β0̂,β1̂)/(n−2)</c>.
            ''' Let the Jacobian row for observation i be <c>Ji = [∂ui/∂β0, ∂ui/∂β1]</c>:
            ''' <c>∂ui/∂β0 = −1/√D</c>,
            ''' <c>∂ui/∂β1 = −xi/√D − (β1 ri)/D^(3/2)</c>,
            ''' where <c>D = λ + β1²</c>.
            ''' Then <c>A = JᵀJ</c> (2×2) and the covariance matrix is approximated by
            ''' <c>Cov(β̂) ≈ s² · A⁻¹</c>.
            ''' Hence <c>SE(β0̂)=√Cov00</c>, <c>SE(β1̂)=√Cov11</c>.
            ''' </para>
            '''
            ''' <para>
            ''' <b>"Analytical" confidence intervals (MCR style).</b><br/>
            ''' MCR uses Wald intervals with the Student-t critical value:
            ''' <c>β̂ ± t_(1−α/2, n−2) · SE</c>. :contentReference[oaicite:1]{index=1}
            ''' </para>
            '''
            ''' <para>
            ''' <b>References (core).</b><br/>
            ''' <list type="bullet">
            ''' <item>
            ''' <description>
            ''' Deming, W. E. (1943). <i>Statistical Adjustment of Data</i>. Wiley.
            ''' </description>
            ''' </item>
            ''' <item>
            ''' <description>
            ''' Linnet, K. (1993). Evaluation of Regression Procedures for Methods Comparison Studies.
            ''' <i>Clinical Chemistry</i>, 39(3), 424–432.
            ''' </description>
            ''' </item>
            ''' <item>
            ''' <description>
            ''' Fuller, W. A. (1987). <i>Measurement Error Models</i>. Wiley.
            ''' </description>
            ''' </item>
            ''' <item>
            ''' <description>
            ''' MCR package source: <c>mc.analytical.ci</c> uses <c>qt(1−α/2, n−2)</c> for Wald CIs. :contentReference[oaicite:2]{index=2}
            ''' </description>
            ''' </item>
            ''' </list>
            ''' </para>
            ''' </remarks>
            Public Function DemingAnalyticalCI() As (InterceptCI As ConfidenceIntervalResult, SlopeCI As ConfidenceIntervalResult)

                Dim n As Integer = x.Length
                If n < 3 Then AppGlobals.BSerr.LogAndThrow(New ArgumentException("At least 3 observations are required (n>=3)."))
                If Double.IsNaN(alpha) OrElse alpha <= 0 OrElse alpha >= 1 Then AppGlobals.BSerr.LogAndThrow(New ArgumentOutOfRangeException(NameOf(alpha), "alpha must be in (0,1)."))

                ' MCR-compatible meaning:
                ' Lambda = error.ratio = σx²/σy²  (reference/test)
                If Lambda <= 0 OrElse Double.IsNaN(Lambda) OrElse Double.IsInfinity(Lambda) Then
                    AppGlobals.BSerr.LogAndThrow(New ArgumentOutOfRangeException(NameOf(Lambda), "Error ratio (Lambda) must be finite and > 0."))
                End If

                Me.pCItype = "Analytical (closed form / linearization)"

                ' Convert to Deming variance ratio used in common formulas:
                ' δ = σy²/σx² = 1 / error.ratio
                Dim delta As Double = 1.0 / Lambda

                ' Means
                Dim xbar As Double = x.Average()
                Dim ybar As Double = y.Average()

                ' Sample covariances (divide by n-1)
                Dim Sxx As Double = 0.0, Syy As Double = 0.0, Sxy As Double = 0.0
                For i As Integer = 0 To n - 1
                    Dim dx As Double = x(i) - xbar
                    Dim dy As Double = y(i) - ybar
                    Sxx += dx * dx
                    Syy += dy * dy
                    Sxy += dx * dy
                Next
                Sxx /= (n - 1)
                Syy /= (n - 1)
                Sxy /= (n - 1)

                If Sxy = 0.0 Then
                    AppGlobals.BSerr.LogAndThrow(New InvalidOperationException("Deming slope is undefined because Sxy = 0 (degenerate association)."))
                End If

                ' Deming slope (closed form, using δ = σy²/σx²)
                ' b = (Syy - δ Sxx + sign(Sxy)*sqrt((Syy - δ Sxx)^2 + 4 δ Sxy^2)) / (2 Sxy)
                Dim A As Double = Syy - delta * Sxx
                Dim disc As Double = A * A + 4.0 * delta * Sxy * Sxy
                Dim root As Double = Math.Sqrt(disc)
                Dim sgn As Double = If(Sxy >= 0.0, 1.0, -1.0)

                Dim slope As Double = (A + sgn * root) / (2.0 * Sxy)
                Dim intercept As Double = ybar - slope * xbar

                ' Objective Q = Σ ri² / (δ + slope²), where ri = yi - a - b xi
                Dim D As Double = delta + slope * slope
                Dim sseOverD As Double = 0.0
                Dim r() As Double = New Double(n - 1) {}
                For i As Integer = 0 To n - 1
                    r(i) = y(i) - intercept - slope * x(i)
                    sseOverD += (r(i) * r(i)) / D
                Next

                ' Residual variance estimate
                Dim s2 As Double = sseOverD / (n - 2)

                ' Build Jacobian J of ui = ri / sqrt(D)
                ' ∂ui/∂a = -1/sqrt(D)
                ' ∂ui/∂b = -xi/sqrt(D) - (b*ri)/D^(3/2)
                Dim invSqrtD As Double = 1.0 / Math.Sqrt(D)
                Dim invD32 As Double = 1.0 / (D * Math.Sqrt(D))

                Dim a11 As Double = 0.0, a12 As Double = 0.0, a22 As Double = 0.0

                For i As Integer = 0 To n - 1
                    Dim j1 As Double = -invSqrtD
                    Dim j2 As Double = -(x(i) * invSqrtD) - (slope * r(i)) * invD32

                    a11 += j1 * j1
                    a12 += j1 * j2
                    a22 += j2 * j2
                Next

                ' Invert 2x2 matrix A = J'J
                Dim det As Double = a11 * a22 - a12 * a12
                If det <= 0 OrElse Double.IsNaN(det) OrElse Double.IsInfinity(det) Then
                    AppGlobals.BSerr.LogAndThrow(New InvalidOperationException("Failed to invert information matrix for analytical SE."))
                End If

                Dim inv00 As Double = a22 / det
                Dim inv11 As Double = a11 / det

                Dim varIntercept As Double = s2 * inv00
                Dim varSlope As Double = s2 * inv11

                Dim seIntercept As Double = Math.Sqrt(Math.Max(0.0, varIntercept))
                Dim seSlope As Double = Math.Sqrt(Math.Max(0.0, varSlope))

                Me.pInterceptSE = seIntercept
                Me.pSlopeSE = seSlope

                ' MCR-style: Wald CI with t critical value, df = n-2
                Dim df As Integer = n - 2
                Dim tCrit As Double = distributions.T_Inv(1.0 - alpha / 2.0, df)

                Me.pInterceptCI = New ConfidenceIntervalResult With {
                    .Estimate = intercept,
                    .alpha = Me.alpha,
                    .StdErr = seIntercept,
                    .LowerLimit = intercept - tCrit * seIntercept,
                    .UpperLimit = intercept + tCrit * seIntercept}

                Me.pSlopeCI = New ConfidenceIntervalResult With {
                    .Estimate = slope,
                    .alpha = Me.alpha,
                    .StdErr = seSlope,
                    .LowerLimit = slope - tCrit * seSlope,
                    .UpperLimit = slope + tCrit * seSlope}

                Return (pInterceptCI, pSlopeCI)
            End Function

            ''' <summary>
            ''' Computes Deming regression parameter confidence intervals using an MCR-compatible
            ''' "analytical" Wald/t approach with Linnet jackknife standard errors.
            ''' </summary>
            ''' <returns>
            ''' A tuple <c>(InterceptCI, SlopeCI)</c> where:
            ''' <list type="bullet">
            '''   <item><description><c>InterceptCI</c> is the confidence interval for the intercept <c>b0</c>.</description></item>
            '''   <item><description><c>SlopeCI</c> is the confidence interval for the slope <c>b1</c>.</description></item>
            ''' </list>
            ''' Each <see cref="ConfidenceIntervalResult"/> contains:
            ''' <c>Estimate</c>, <c>LowerLimit</c>, and <c>UpperLimit</c>.
            ''' </returns>
            ''' <exception cref="ArgumentException">
            ''' Thrown when fewer than 3 paired observations are available (because the t-based Wald CI uses <c>df = n − 2</c>).
            ''' </exception>
            ''' <remarks>
            ''' <h3>Purpose and correspondence to MCR</h3>
            ''' <para>
            ''' This routine reproduces the common method-comparison workflow used in the R package <c>mcr</c> for Deming regression:
            ''' </para>
            ''' <list type="bullet">
            '''   <item>
            '''     <description>
            '''       Confidence intervals are computed as Wald/t intervals:
            '''       <c>estimate ± t_(1−α/2, n−2) · SE</c>.
            '''     </description>
            '''   </item>
            '''   <item>
            '''     <description>
            '''       Standard errors for Deming parameters are estimated using Linnet's jackknife pseudo-value approach.
            '''       (Hence the internal label <c>"Analytical - Linnet"</c>.)
            '''     </description>
            '''   </item>
            ''' </list>
            '''
            ''' <h3>Model and assumptions</h3>
            ''' <para>
            ''' Deming regression is an errors-in-variables linear model for method comparison when both axes are measured with error.
            ''' Let the true (unobserved) values satisfy:
            ''' </para>
            ''' <para>
            ''' <c>Y* = b0 + b1 · X*</c>.
            ''' </para>
            ''' <para>
            ''' Observations are contaminated by measurement errors:
            ''' </para>
            ''' <para>
            ''' <c>x = X* + εx</c>, <c>y = Y* + εy</c>,
            ''' </para>
            ''' <para>
            ''' with <c>E[εx]=E[εy]=0</c> and error-variance ratio
            ''' <c>δ = Var(εy) / Var(εx) = σy² / σx²</c>. The ratio δ is treated as known (or fixed by assumption).
            ''' </para>
            '''
            ''' <h3>Error ratio parameterization used here</h3>
            ''' <para>
            ''' This implementation stores <see cref="Lambda"/> using the convention commonly found in method-comparison tools (incl. MCR):
            ''' <c>Lambda = σx² / σy²</c> (reference over test).
            ''' </para>
            ''' <para>
            ''' The classic Deming formula is usually expressed in terms of:
            ''' <c>δ = σy² / σx²</c>.
            ''' Therefore, this routine converts:
            ''' </para>
            ''' <para>
            ''' <c>δ = 1 / Lambda</c>.
            ''' </para>
            '''
            ''' <h3>Point estimates (Deming coefficients)</h3>
            ''' <para>
            ''' The Deming slope and intercept are computed by <c>ComputeDemingCoefficients(x, y, δ)</c>.
            ''' Let <c>x̄</c>, <c>ȳ</c> be sample means and let centered sums be:
            ''' </para>
            ''' <para>
            ''' <c>Sxx = Σ(xi − x̄)²</c>, <c>Syy = Σ(yi − ȳ)²</c>, <c>Sxy = Σ(xi − x̄)(yi − ȳ)</c>.
            ''' </para>
            ''' <para>
            ''' A common closed-form Deming slope is:
            ''' </para>
            ''' <para>
            ''' <c>
            ''' b1 = (Syy − δ·Sxx + sign(Sxy)·√((Syy − δ·Sxx)² + 4δ·Sxy²)) / (2·Sxy),
            ''' </c>
            ''' </para>
            ''' <para>
            ''' and intercept:
            ''' <c>b0 = ȳ − b1·x̄</c>.
            ''' </para>
            ''' <para>
            ''' (The sign convention ensures the slope follows the direction of association in typical method-comparison usage.)
            ''' </para>
            '''
            ''' <h3>Jackknife standard errors (Linnet pseudo-values)</h3>
            ''' <para>
            ''' Deming regression does not have a universally adopted, simple closed-form standard error in everyday software.
            ''' In laboratory medicine/method comparison practice, Linnet recommended jackknife-based uncertainty estimation.
            ''' </para>
            ''' <para>
            ''' This routine uses leave-one-out fits:
            ''' for each observation i, refit Deming regression on the dataset with point i removed to obtain
            ''' <c>θ_(i)</c> (where θ is b0 or b1).
            ''' </para>
            ''' <para>
            ''' Linnet’s jackknife pseudo-values are then formed as:
            ''' </para>
            ''' <para>
            ''' <c>p_i = n·θ_full − (n−1)·θ_(i)</c>,
            ''' </para>
            ''' <para>
            ''' where <c>θ_full</c> is the full-sample estimate and <c>n</c> is the sample size.
            ''' </para>
            ''' <para>
            ''' Let <c>sd(p)</c> be the sample standard deviation of the pseudo-values. The jackknife standard error is:
            ''' </para>
            ''' <para>
            ''' <c>SE(θ) = sd(p) / √n</c>.
            ''' </para>
            ''' <para>
            ''' In code, <c>StatFunc.stDev</c> supplies <c>sd(p)</c> (sample SD), and the scaling <c>/ √n</c> yields SE.
            ''' </para>
            '''
            ''' <h3>Wald/t confidence intervals</h3>
            ''' <para>
            ''' With the standard error estimates above, confidence intervals are computed using a t critical value with
            ''' degrees of freedom <c>df = n − 2</c> (mirroring common linear-regression reporting and MCR’s analytical CI helper):
            ''' </para>
            ''' <para>
            ''' <c>CI(θ) = θ̂ ± t_(1−α/2, n−2) · SE(θ)</c>,
            ''' </para>
            ''' <para>
            ''' where <c>θ̂</c> is either <c>b0</c> or <c>b1</c>.
            ''' </para>
            ''' <para>
            ''' Here <c>tCrit = distributions.T_Inv(1−α/2, n−2)</c>.
            ''' </para>
            '''
            ''' <h3>Interpretation in method comparison</h3>
            ''' <para>
            ''' In typical method-comparison interpretation:
            ''' </para>
            ''' <list type="bullet">
            '''   <item><description>If <c>0</c> lies in the CI for <c>b0</c>, there is no evidence of a constant bias.</description></item>
            '''   <item><description>If <c>1</c> lies in the CI for <c>b1</c>, there is no evidence of a proportional bias.</description></item>
            ''' </list>
            '''
            ''' <h3>Algorithmic complexity</h3>
            ''' <para>
            ''' The jackknife requires <c>n</c> Deming fits, each <c>O(n)</c> for the closed-form sums, giving overall <c>O(n²)</c>.
            ''' For typical method-comparison sample sizes this is acceptable and aligns with standard implementations.
            ''' </para>
            '''
            ''' <h3>References</h3>
            ''' <list type="bullet">
            '''   <item>
            '''     <description>
            '''       Deming, W. E. (1943). <i>Statistical Adjustment of Data</i>. Wiley.
            '''       (Foundational text on errors-in-variables and Deming-type adjustments.)
            '''     </description>
            '''   </item>
            '''   <item>
            '''     <description>
            '''       Linnet, K. (1990). <i>Estimation of the linear relationship between two methods of measurement</i>.
            '''       (Introduces/advocates jackknife-based uncertainty estimation in method comparison contexts.)
            '''     </description>
            '''   </item>
            '''   <item>
            '''     <description>
            '''       Linnet, K. (1993). <i>Evaluation of regression procedures for methods comparison studies</i>, <i>Clinical Chemistry</i>.
            '''       (Comparative evaluation; commonly cited in method comparison software including MCR.)
            '''     </description>
            '''   </item>
            '''   <item>
            '''     <description>
            '''       Passing, H. &amp; Bablok, W. (1983). <i>J. Clin. Chem. Clin. Biochem.</i>
            '''       (Contextual reference for method-comparison regression procedures.)
            '''     </description>
            '''   </item>
            '''   <item>
            '''     <description>
            '''       MCR R package documentation/source: analytical CIs computed as Wald/t intervals with <c>df = n − 2</c>.
            '''       (See MCR helper <c>mc.analytical.ci</c>.)
            '''     </description>
            '''   </item>
            ''' </list>
            ''' </remarks>
            Public Function DemingAnalyticalCI_MCR() As (InterceptCI As ConfidenceIntervalResult, SlopeCI As ConfidenceIntervalResult)


                Dim n As Integer = x.Length
                If n < 3 Then AppGlobals.BSerr.LogAndThrow(New ArgumentException("Deming analytical CI requires at least 3 observations (df = n-2)."))

                ' Error ratio convention used throughout this project:
                '   Lambda = σx²/σy²  (reference over test)
                ' ComputeDemingCoefficients expects this same convention.
                Dim errorRatioXoverY As Double = Me.Lambda

                ' Fit on full data
                Dim fullFit = ComputeDemingCoefficients(x, y, errorRatioXoverY)
                Dim b0 As Double = fullFit.Intercept
                Dim b1 As Double = fullFit.Slope

                ' ---- Jackknife SE (Linnet pseudo-values) ----
                Dim jackB0(n - 1) As Double
                Dim jackB1(n - 1) As Double

                For i As Integer = 0 To n - 1
                    ' Build leave-one-out sample
                    Dim xLOO(n - 2) As Double
                    Dim yLOO(n - 2) As Double
                    Dim k As Integer = 0
                    For j As Integer = 0 To n - 1
                        If j = i Then Continue For
                        xLOO(k) = x(j)
                        yLOO(k) = y(j)
                        k += 1
                    Next

                    Dim fitI = ComputeDemingCoefficients(xLOO, yLOO, errorRatioXoverY)
                    jackB0(i) = fitI.Intercept
                    jackB1(i) = fitI.Slope
                Next

                ' Linnet jackknife pseudo-values:
                ' p_i = n*theta_full - (n-1)*theta_(i)
                Dim pvB0(n - 1) As Double
                Dim pvB1(n - 1) As Double
                For i As Integer = 0 To n - 1
                    pvB0(i) = n * b0 - (n - 1) * jackB0(i)
                    pvB1(i) = n * b1 - (n - 1) * jackB1(i)
                Next

                Dim seB0 As Double = StatFunc.stDev(pvB0) / Math.Sqrt(n)
                Dim seB1 As Double = StatFunc.stDev(pvB1) / Math.Sqrt(n)

                ' Wald/t CI with df = n - 2 (matches mcr::mc.analytical.ci)
                Dim tCrit As Double = distributions.T_Inv(1.0 - alpha / 2.0, n - 2)

                Me.pInterceptCI = New ConfidenceIntervalResult With {
                    .Estimate = b0,
                    .alpha = Me.alpha,
                    .StdErr = seB0,
                    .LowerLimit = b0 - tCrit * seB0,
                    .UpperLimit = b0 + tCrit * seB0}

                Me.pSlopeCI = New ConfidenceIntervalResult With {
                    .Estimate = b1,
                    .alpha = Me.alpha,
                    .StdErr = seB1,
                    .LowerLimit = b1 - tCrit * seB1,
                    .UpperLimit = b1 + tCrit * seB1}

                Me.pCItype = "Analytical – Linnet (jackknife pseudo-values)"

                Return (InterceptCI, SlopeCI)
            End Function

            Public Sub AddPlot(ws As Worksheet)
                Dim ch = graphics.GeneralScatterPlot(Me.x, Me.y, Me.pVarY, Me.pVarX, ws, "Deming Regression")
                Dim dMinX As Double = Me.x.Min()
                Dim dMaxX As Double = Me.x.Max()

                If Me.pInterceptCI Is Nothing Then Me.DemingAnalyticalCI()

                With ch
                    .HasLegend = False
                    .HasLegend = True

                    'add and plot fit line
                    .SeriesCollection.NewSeries
                    With .SeriesCollection(2)
                        .XValues = {dMinX, dMaxX}
                        .Values = {Me.pInterceptCI.Estimate + Me.pSlopeCI.Estimate * dMinX,
                                   Me.pInterceptCI.Estimate + Me.pSlopeCI.Estimate * dMaxX}
                        .Name = "Regression line"
                        .MarkerStyle = -4142
                        .Border.Color = RGB(255, 0, 0)
                        With .Format.Line
                            .Visible = True
                            .Weight = 1.5
                        End With
                    End With

                    'Zero x=y refline
                    .SeriesCollection.NewSeries
                    With .SeriesCollection(3)
                        .XValues = {dMinX, dMaxX}
                        .Values = {dMinX, dMaxX}
                        .Name = "Unity line (y = x)"
                        .MarkerStyle = -4142 'no marker
                        .Border.Color = RGB(0, 0, 255)
                        With .Format.Line
                            .Visible = True
                            .DashStyle = 4 'msoLineDash
                            .Weight = 0.5
                        End With
                    End With
                End With
            End Sub

            ' ==========================================================
            ' Core Deming coefficient computation
            ' ==========================================================
            ''' <summary>
            ''' Computes Deming regression coefficients (a,b) for y = a + b x with variance ratio λ.
            ''' </summary>
            ''' <remarks>
            ''' Let x̄, ȳ be sample means and define centered sums:
            ''' Sxx = Σ (xi - x̄)^2,  Syy = Σ (yi - ȳ)^2,  Sxy = Σ (xi - x̄)(yi - ȳ).
            ''' Then the Deming slope is:
            ''' b = (Syy - λ Sxx + sign(Sxy)*sqrt( (Syy - λ Sxx)^2 + 4 λ Sxy^2 )) / (2 Sxy),
            ''' and intercept a = ȳ - b x̄.
            ''' </remarks>
            Private Shared Function ComputeDemingCoefficients(x As Double(), y As Double(), errorRatio As Double) As (Intercept As Double, Slope As Double)

                Dim n As Integer = x.Length
                If n <> y.Length Then AppGlobals.BSerr.LogAndThrow(New ArgumentException("x and y must have the same length."))
                If n < 2 Then Return (Double.NaN, Double.NaN)
                If errorRatio <= 0.0 OrElse Double.IsNaN(errorRatio) OrElse Double.IsInfinity(errorRatio) Then Return (Double.NaN, Double.NaN)

                ' errorRatio = σx²/σy² (MCR)
                ' delta = σy²/σx²
                Dim delta As Double = 1.0 / errorRatio

                Dim xBar As Double = x.Average()
                Dim yBar As Double = y.Average()

                Dim Sxx As Double = 0.0
                Dim Syy As Double = 0.0
                Dim Sxy As Double = 0.0

                For i As Integer = 0 To n - 1
                    Dim dx As Double = x(i) - xBar
                    Dim dy As Double = y(i) - yBar
                    Sxx += dx * dx
                    Syy += dy * dy
                    Sxy += dx * dy
                Next

                If Sxy = 0.0 Then Return (Double.NaN, Double.NaN)

                Dim A As Double = Syy - delta * Sxx
                Dim B As Double = 2.0 * Sxy
                Dim disc As Double = A * A + 4.0 * delta * Sxy * Sxy
                Dim root As Double = Math.Sqrt(disc)

                Dim sgn As Double = If(Sxy >= 0.0, 1.0, -1.0)
                Dim slope As Double = (A + sgn * root) / B
                Dim intercept As Double = yBar - slope * xBar

                Return (intercept, slope)
            End Function

        End Class




        ''' <summary>
        ''' Provides methods for computing intraclass correlation coefficients (ICC) and corresponding confidence intervals.
        ''' </summary>
        ''' <remarks>
        ''' <para>
        ''' This class implements commonly used ICC forms as described in the classical reliability literature
        ''' (e.g., Shrout &amp; Fleiss) and later summaries (e.g., McGraw &amp; Wong).
        ''' </para>
        '''
        ''' <para>
        ''' Supported ICC families:
        ''' </para>
        ''' <list type="bullet">
        '''   <item>
        '''     <description>
        '''     <b>ICC(1,·)</b> (one-way random effects): computed from a one-way ANOVA where "groups" are the targets/subjects.
        '''     </description>
        '''   </item>
        '''   <item>
        '''     <description>
        '''     <b>ICC(2,·)</b> (two-way random effects, absolute agreement): computed from a two-way ANOVA without replication
        '''     (targets × raters), treating both targets and raters as random.
        '''     </description>
        '''   </item>
        '''   <item>
        '''     <description>
        '''     <b>ICC(3,·)</b> (two-way mixed effects, consistency): computed from a two-way ANOVA without replication
        '''     (targets × raters), treating targets as random and raters as fixed.
        '''     </description>
        '''   </item>
        ''' </list>
        '''
        ''' <para>
        ''' <b>Input layout</b>
        ''' </para>
        ''' <list type="bullet">
        '''   <item>
        '''     <description>
        '''     For ICC(1,·): data are provided as a jagged array <c>x()()</c>, where each inner array is one target/group and
        '''     contains repeated measurements/ratings for that target.
        '''     </description>
        '''   </item>
        '''   <item>
        '''     <description>
        '''     For ICC(2,·) and ICC(3,·): data are provided as a rectangular matrix <c>x(,)</c> where rows are targets/subjects
        '''     and columns are raters/judges/measurement methods. The matrix must be complete (no missing cells).
        '''     </description>
        '''   </item>
        ''' </list>
        '''
        ''' <para>
        ''' <b>Confidence intervals</b>
        ''' </para>
        ''' <para>
        ''' Confidence intervals are computed using an F-distribution method by applying bounds on mean-square ratios and then
        ''' transforming to ICC limits. This approach yields generally asymmetric intervals and may produce negative lower bounds.
        ''' </para>
        '''
        ''' <para>
        ''' <b>Important implementation note:</b>
        ''' All CI calculations assume that <c>distributions.F_Inv(p, df1, df2)</c> returns the <i>lower-tail</i> quantile
        ''' (i.e., <c>P(F &lt;= x) = p</c>). If your implementation returns an upper-tail critical value instead, you must convert
        ''' probabilities accordingly; otherwise CI bounds will be incorrect.
        ''' </para>
        '''
        ''' <para>
        ''' <b>Assumptions and limitations</b>
        ''' </para>
        ''' <list type="bullet">
        '''   <item><description>Observations are numeric and finite (no NaN/Infinity).</description></item>
        '''   <item><description>Targets are assumed independent.</description></item>
        '''   <item><description>For ANOVA-based ICC, homoscedasticity (equal within-target variance) is typically assumed.</description></item>
        '''   <item><description>For ICC(2,·) / ICC(3,·), the design must be balanced and complete (one score per target×rater cell).</description></item>
        '''   <item><description>
        '''     For ICC(1,·) with unbalanced group sizes, the point estimate may use an effective group size n0; F-based CIs are exact
        '''     in balanced designs and are commonly used as approximations in unbalanced designs.
        '''   </description></item>
        ''' </list>
        ''' </remarks>
        Public Class IntraclassCorrelation

            Private pRepeatabilityCoefficient As ConfidenceIntervalResult = Nothing

            ''' <summary>
            ''' Computes ICC(1,1): one-way random effects intraclass correlation for a single measurement,
            ''' including an F-based confidence interval.
            ''' </summary>
            ''' <param name="x">
            ''' Jagged array of observations grouped by target/subject.
            ''' Each <c>x(i)</c> is the set of repeated measurements (e.g., ratings) for target i.
            ''' </param>
            ''' <param name="alpha">
            ''' Optional two-sided significance level used for the confidence interval. 
            ''' The default is <c>0.05</c>, corresponding to a 95% confidence interval.
            ''' </param>
            ''' <returns>
            ''' A <see cref="ConfidenceIntervalResult"/> containing:
            ''' <list type="bullet">
            '''   <item><description><c>Estimate</c>: ICC(1,1) point estimate.</description></item>
            '''   <item><description><c>LowerLimit</c> / <c>UpperLimit</c>: CI bounds.</description></item>
            '''   <item><description><c>alpha</c>: the supplied alpha.</description></item>
            ''' </list>
            ''' </returns>
            ''' <remarks>
            ''' <para>
            ''' ICC(1,1) is based on the one-way random effects model:
            ''' <c>y_ij = μ + u_i + e_ij</c>, where targets i are random and repeated measurements j are exchangeable.
            ''' </para>
            '''
            ''' <para>
            ''' The method computes a one-way ANOVA using <c>parametric.OneWayANOVA</c> to obtain mean squares:
            ''' <c>MSB</c> (between targets) and <c>MSW</c> (within targets/error).
            ''' </para>
            '''
            ''' <para>
            ''' For unbalanced group sizes, an effective group size <c>n0</c> may be used in the ICC formula:
            ''' <c>ICC(1,1) = (MSB - MSW) / (MSB + (n0 - 1) MSW)</c>.
            ''' For balanced data, <c>n0</c> equals the number of measurements per target.
            ''' </para>
            '''
            ''' <para>
            ''' The confidence interval is computed using an F-distribution method on the ratio <c>F = MSB/MSW</c>
            ''' and then transformed to the ICC scale. The interval is generally asymmetric and may include negative values.
            ''' </para>
            '''
            ''' <para>
            ''' CI correctness depends on <c>distributions.F_Inv</c> returning a lower-tail quantile.
            ''' </para>
            ''' </remarks>
            Public Function ICC11(x()() As Double, Optional alpha As Double = 0.05) As ConfidenceIntervalResult
                Dim vars(x.Length - 1) As String
                Dim anova = New parametric.OneWayANOVA(x, vars)
                Dim atab = anova.compute()

                Dim MSb As Double = CDbl(atab(0, 2))
                Dim MSw As Double = CDbl(atab(1, 2))
                Dim F As Double = CDbl(atab(0, 3))
                Dim df1 As Integer = atab(0, 1)
                Dim df2 As Integer = atab(1, 1)
                Dim n0 As Double = EffectiveGroupSizeN0_ICC11(x)
                Dim ICC As Double = (F - 1) / (F + (n0 - 1))

                Dim out As New ConfidenceIntervalResult
                out.Estimate = ICC
                out.alpha = alpha
                Dim Fl As Double = F / distributions.F_Inv(1.0 - alpha / 2.0, df1, df2)
                Dim Fu As Double = F * distributions.F_Inv(1.0 - alpha / 2.0, df2, df1)
                out.LowerLimit = (Fl - 1) / (Fl + (n0 - 1))
                out.UpperLimit = (Fu - 1) / (Fu + (n0 - 1))

                Return out
            End Function

            ''' <summary>
            ''' Computes ICC(1,k): one-way random effects intraclass correlation for the mean of k measurements,
            ''' including an F-based confidence interval.
            ''' </summary>
            ''' <param name="x">
            ''' Jagged array of observations grouped by target/subject.
            ''' Each <c>x(i)</c> is the set of repeated measurements (e.g., ratings) for target i.
            ''' </param>
            ''' <param name="alpha">
            ''' Optional two-sided significance level used for the confidence interval. 
            ''' The default is <c>0.05</c>, corresponding to a 95% confidence interval.
            ''' </param>
            ''' <returns>
            ''' A <see cref="ConfidenceIntervalResult"/> containing the ICC(1,k) estimate and CI.
            ''' </returns>
            ''' <remarks>
            ''' <para>
            ''' ICC(1,k) represents the reliability of the average of k exchangeable measurements per target.
            ''' In balanced designs, k equals the number of measurements per target. In unbalanced designs, an
            ''' effective k (often denoted n0) may be used by some implementations.
            ''' </para>
            '''
            ''' <para>
            ''' A common formula is:
            ''' <c>ICC(1,k) = (MSB - MSW) / MSB</c>
            ''' which is equivalent to transforming ICC(1,1) via:
            ''' <c>ICC(1,k) = (k·ICC(1,1)) / (1 + (k-1)·ICC(1,1))</c>
            ''' in balanced designs.
            ''' </para>
            '''
            ''' <para>
            ''' Confidence limits are typically obtained by transforming the ICC(1,1) F-based limits to the k-average scale.
            ''' </para>
            ''' </remarks>
            Public Function ICC1k(x()() As Double, Optional alpha As Double = 0.05) As ConfidenceIntervalResult
                Dim vars(x.Length - 1) As String
                Dim anova = New parametric.OneWayANOVA(x, vars)
                Dim atab = anova.compute()

                Dim MSb As Double = CDbl(atab(0, 2))
                Dim MSw As Double = CDbl(atab(1, 2))
                Dim ICC As Double = (MSb - MSw) / MSb
                Dim out As New ConfidenceIntervalResult
                out.Estimate = ICC
                out.alpha = alpha
                Dim icc11 = Me.ICC11(x, alpha)

                Dim L1 As Double = icc11.LowerLimit
                Dim U1 As Double = icc11.UpperLimit
                Dim n0 As Double = EffectiveGroupSizeN0_ICC11(x)

                out.LowerLimit = (n0 * L1) / (1 + (n0 - 1) * L1)
                out.UpperLimit = (n0 * U1) / (1 + (n0 - 1) * U1)

                Return out
            End Function

            ''' <summary>
            ''' Computes ICC(2,1): two-way random effects intraclass correlation for absolute agreement of a single rater/measurement,
            ''' including an F-based confidence interval.
            ''' </summary>
            ''' <param name="x">
            ''' Rectangular data matrix where rows are targets/subjects and columns are raters/judges/measurement methods.
            ''' The matrix must be complete (no missing values) with one observation per target×rater cell.
            ''' </param>
            ''' <param name="alpha">
            ''' Optional two-sided significance level used for the confidence interval. 
            ''' The default is <c>0.05</c>, corresponding to a 95% confidence interval.
            ''' </param>
            ''' <returns>
            ''' A <see cref="ConfidenceIntervalResult"/> containing the ICC(2,1) estimate and CI.
            ''' </returns>
            ''' <remarks>
            ''' <para>
            ''' ICC(2,1) corresponds to a two-way random effects model with absolute agreement, where both targets and raters are
            ''' considered random samples from larger populations.
            ''' </para>
            '''
            ''' <para>
            ''' Mean squares are obtained using <c>parametric.OneWayRmANOVA</c>, interpreted as a two-way ANOVA without replication:
            ''' </para>
            ''' <list type="bullet">
            '''   <item><description><c>MSR</c>: mean square for rows/targets.</description></item>
            '''   <item><description><c>MSC</c>: mean square for columns/raters.</description></item>
            '''   <item><description><c>MSE</c>: residual mean square (interaction + error).</description></item>
            ''' </list>
            '''
            ''' <para>
            ''' Point estimate:
            ''' <c>ICC(2,1) = (MSR - MSE) / (MSR + (k-1)MSE + k(MSC - MSE)/n)</c>,
            ''' where n is number of targets and k number of raters.
            ''' </para>
            '''
            ''' <para>
            ''' The confidence interval is computed by placing F-distribution bounds on <c>F = MSR/MSE</c> and transforming to ICC limits.
            ''' This yields an asymmetric interval. CI correctness depends on <c>distributions.F_Inv</c> returning a lower-tail quantile.
            ''' </para>
            ''' </remarks>
            Public Function ICC21(x(,) As Double, Optional alpha As Double = 0.05) As ConfidenceIntervalResult

                ' x: rows = targets/subjects, columns = raters/judges (balanced, complete)

                Dim n As Integer = x.GetLength(0) ' number of targets
                Dim k As Integer = x.GetLength(1) ' number of raters

                If n < 2 Then AppGlobals.BSerr.LogAndThrow(New ArgumentException("ICC(2,1) requires at least 2 targets (rows)."))
                If k < 2 Then AppGlobals.BSerr.LogAndThrow(New ArgumentException("ICC(2,1) requires at least 2 raters (columns)."))

                Dim vars(k - 1) As String
                Dim anova = New parametric.OneWayRmANOVA(x, vars)
                Dim atab = anova.compute()

                ' OneWayRmANOVA table:
                ' 0 = Between groups (columns/raters): SS, df, MS, F, p
                ' 1 = Subjects (rows/targets):         SS, df, MS, F, p
                ' 2 = Error:                           SS, df, MS
                Dim MSC As Double = CDbl(atab(0, 2)) ' columns (raters)
                Dim MSR As Double = CDbl(atab(1, 2)) ' rows (targets)
                Dim MSE As Double = CDbl(atab(2, 2)) ' residual

                Dim dfR As Integer = CInt(atab(1, 1)) ' n - 1
                Dim dfE As Integer = CInt(atab(2, 1)) ' (n - 1)(k - 1)

                If MSE <= 0 Then AppGlobals.BSerr.LogAndThrow(New ArgumentException("MSE <= 0; ICC(2,1) is undefined (check data)."))

                ' ICC(2,1): two-way random, absolute agreement, single measure
                ' ICC = (MSR - MSE) / (MSR + (k-1)MSE + k*(MSC - MSE)/n)
                Dim ICC As Double = (MSR - MSE) / (MSR + (k - 1) * MSE + (k * (MSC - MSE) / n))

                ' For CI we transform the CI for F = MSR/MSE while holding the extra term fixed.
                Dim Fobs As Double = MSR / MSE
                Dim c As Double = (k * (MSC - MSE)) / (n * MSE) ' dimensionless extra term in denominator

                Dim out As New ConfidenceIntervalResult
                out.Estimate = ICC
                out.alpha = alpha

                ' NOTE: This assumes distributions.F_Inv(p, df1, df2) returns the LOWER-tail quantile: P(F <= x) = p.
                Dim Fl As Double = Fobs / distributions.F_Inv(1.0 - alpha / 2.0, dfR, dfE)
                Dim Fu As Double = Fobs * distributions.F_Inv(1.0 - alpha / 2.0, dfE, dfR)

                out.LowerLimit = (Fl - 1.0) / (Fl + (k - 1.0) + c)
                out.UpperLimit = (Fu - 1.0) / (Fu + (k - 1.0) + c)

                Return out
            End Function

            ''' <summary>
            ''' Computes ICC(2,k): two-way random effects intraclass correlation for absolute agreement of the mean of k raters/measurements,
            ''' including an F-based confidence interval.
            ''' </summary>
            ''' <param name="x">
            ''' Rectangular data matrix where rows are targets/subjects and columns are raters/judges/measurement methods.
            ''' The matrix must be complete (no missing values) with one observation per target×rater cell.
            ''' </param>
            ''' <param name="alpha">
            ''' Optional two-sided significance level used for the confidence interval. 
            ''' The default is <c>0.05</c>, corresponding to a 95% confidence interval.
            ''' </param>
            ''' <returns>
            ''' A <see cref="ConfidenceIntervalResult"/> containing the ICC(2,k) estimate and CI.
            ''' </returns>
            ''' <remarks>
            ''' <para>
            ''' ICC(2,k) is the absolute-agreement reliability of the average of k random raters/measurements.
            ''' It is appropriate when raters are considered randomly sampled and you wish to generalize to other raters.
            ''' </para>
            '''
            ''' <para>
            ''' Mean squares are obtained using <c>parametric.OneWayRmANOVA</c> (two-way ANOVA without replication),
            ''' giving <c>MSR</c>, <c>MSC</c>, and <c>MSE</c>.
            ''' </para>
            '''
            ''' <para>
            ''' Point estimate:
            ''' <c>ICC(2,k) = (MSR - MSE) / (MSR + (MSC - MSE)/n)</c>.
            ''' </para>
            '''
            ''' <para>
            ''' The confidence interval is computed using F-distribution bounds on <c>F = MSR/MSE</c> and transforming to ICC limits.
            ''' CI correctness depends on <c>distributions.F_Inv</c> returning a lower-tail quantile.
            ''' </para>
            ''' </remarks>
            Public Function ICC2k(x(,) As Double, Optional alpha As Double = 0.05) As ConfidenceIntervalResult
                ' ICC(2,k): two-way random effects, absolute agreement, average of k raters
                ' x: rows = targets/subjects, columns = raters/judges (balanced & complete)

                Dim n As Integer = x.GetLength(0) ' targets
                Dim k As Integer = x.GetLength(1) ' raters

                If n < 2 Then AppGlobals.BSerr.LogAndThrow(New ArgumentException("ICC(2,k) requires at least 2 targets (rows)."))
                If k < 2 Then AppGlobals.BSerr.LogAndThrow(New ArgumentException("ICC(2,k) requires at least 2 raters (columns)."))

                Dim vars(k - 1) As String
                Dim anova = New parametric.OneWayRmANOVA(x, vars)
                Dim atab = anova.compute()

                ' OneWayRmANOVA table layout in your code:
                ' 0 = Between groups (columns/raters): SS, df, MS, F, p
                ' 1 = Subjects (rows/targets):         SS, df, MS, F, p
                ' 2 = Error:                           SS, df, MS
                Dim MSC As Double = CDbl(atab(0, 2)) ' columns (raters)
                Dim MSR As Double = CDbl(atab(1, 2)) ' rows (targets)
                Dim MSE As Double = CDbl(atab(2, 2)) ' residual

                Dim dfR As Integer = CInt(atab(1, 1)) ' n - 1
                Dim dfE As Integer = CInt(atab(2, 1)) ' (n - 1)(k - 1)

                If MSE <= 0 Then AppGlobals.BSerr.LogAndThrow(New ArgumentException("MSE <= 0; ICC(2,k) is undefined (check data)."))

                ' ICC(2,k) formula (absolute agreement, average measures):
                ' ICC(2,k) = (MSR - MSE) / (MSR + (MSC - MSE)/n)
                Dim ICC As Double = (MSR - MSE) / (MSR + (MSC - MSE) / n)

                ' CI via F-based bounds on F = MSR/MSE, then transform.
                ' Write ICC(2,k) in terms of F and c:
                ' F = MSR/MSE
                ' c = (MSC - MSE)/(n*MSE)
                ' ICC = (F - 1) / (F + c)
                Dim Fobs As Double = MSR / MSE
                Dim c As Double = (MSC - MSE) / (n * MSE)

                Dim out As New ConfidenceIntervalResult
                out.Estimate = ICC
                out.alpha = alpha

                ' NOTE: This assumes distributions.F_Inv(p, df1, df2) returns the LOWER-tail quantile:  P(F <= x) = p.
                Dim q1 As Double = distributions.F_Inv(1.0 - alpha / 2.0, dfR, dfE)
                Dim q2 As Double = distributions.F_Inv(1.0 - alpha / 2.0, dfE, dfR)

                Dim Fl As Double = Fobs / q1
                Dim Fu As Double = Fobs * q2

                out.LowerLimit = (Fl - 1.0) / (Fl + c)
                out.UpperLimit = (Fu - 1.0) / (Fu + c)

                Return out
            End Function

            ''' <summary>
            ''' Computes ICC(3,1): two-way mixed effects intraclass correlation for consistency of a single rater/measurement,
            ''' including an F-based confidence interval.
            ''' </summary>
            ''' <param name="x">
            ''' Rectangular data matrix where rows are targets/subjects and columns are raters/judges/measurement methods.
            ''' The matrix must be complete (no missing values) with one observation per target×rater cell.
            ''' </param>
            ''' <param name="alpha">
            ''' Optional two-sided significance level used for the confidence interval. 
            ''' The default is <c>0.05</c>, corresponding to a 95% confidence interval.
            ''' </param>
            ''' <returns>
            ''' A <see cref="ConfidenceIntervalResult"/> containing the ICC(3,1) estimate and CI.
            ''' </returns>
            ''' <remarks>
            ''' <para>
            ''' ICC(3,1) corresponds to a two-way mixed effects model (targets random, raters fixed) and measures <i>consistency</i>,
            ''' not absolute agreement. It is appropriate when you care only about the specific raters included in the study
            ''' (i.e., you do not generalize to a wider rater population).
            ''' </para>
            '''
            ''' <para>
            ''' Mean squares are obtained using <c>parametric.OneWayRmANOVA</c> and interpreted as:
            ''' <c>MSR</c> (rows/targets) and <c>MSE</c> (residual).
            ''' </para>
            '''
            ''' <para>
            ''' Point estimate:
            ''' <c>ICC(3,1) = (MSR - MSE) / (MSR + (k-1)MSE)</c>.
            ''' </para>
            '''
            ''' <para>
            ''' The confidence interval is computed via F-distribution bounds on <c>F = MSR/MSE</c> and transformation to ICC limits.
            ''' CI correctness depends on <c>distributions.F_Inv</c> returning a lower-tail quantile.
            ''' </para>
            ''' </remarks>
            Public Function ICC31(x(,) As Double, Optional alpha As Double = 0.05) As ConfidenceIntervalResult
                ' ICC(3,1): two-way mixed effects, consistency, single measure
                ' x: rows = targets/subjects, columns = raters/judges (balanced & complete)

                Dim n As Integer = x.GetLength(0) ' number of targets
                Dim k As Integer = x.GetLength(1) ' number of raters

                If n < 2 Then AppGlobals.BSerr.LogAndThrow(New ArgumentException("ICC(3,1) requires at least 2 targets (rows)."))
                If k < 2 Then AppGlobals.BSerr.LogAndThrow(New ArgumentException("ICC(3,1) requires at least 2 raters (columns)."))

                Dim vars(k - 1) As String
                Dim anova = New parametric.OneWayRmANOVA(x, vars)
                Dim atab = anova.compute()

                ' OneWayRmANOVA table layout in your code:
                ' 0 = Between groups (columns/raters): SS, df, MS, F, p
                ' 1 = Subjects (rows/targets):         SS, df, MS, F, p
                ' 2 = Error:                           SS, df, MS
                Dim MSR As Double = CDbl(atab(1, 2)) ' rows (targets)
                Dim MSE As Double = CDbl(atab(2, 2)) ' residual (interaction + error)

                Dim dfR As Integer = CInt(atab(1, 1)) ' n - 1
                Dim dfE As Integer = CInt(atab(2, 1)) ' (n - 1)(k - 1)

                If MSE <= 0 Then AppGlobals.BSerr.LogAndThrow(New ArgumentException("MSE <= 0; ICC(3,1) is undefined (check data)."))

                ' ICC(3,1) point estimate:
                ' ICC(3,1) = (MSR - MSE) / (MSR + (k - 1)MSE)
                Dim ICC As Double = (MSR - MSE) / (MSR + (k - 1) * MSE)

                Dim out As New ConfidenceIntervalResult With {
                    .Estimate = ICC,
                    .alpha = alpha}

                ' F-based CI via F = MSR/MSE (assumes distributions.F_Inv is LOWER-tail quantile)
                Dim Fobs As Double = MSR / MSE

                Dim q1 As Double = distributions.F_Inv(1.0 - alpha / 2.0, dfR, dfE)
                Dim q2 As Double = distributions.F_Inv(1.0 - alpha / 2.0, dfE, dfR)

                Dim Fl As Double = Fobs / q1
                Dim Fu As Double = Fobs * q2

                out.LowerLimit = (Fl - 1.0) / (Fl + (k - 1.0))
                out.UpperLimit = (Fu - 1.0) / (Fu + (k - 1.0))

                Return out
            End Function

            ''' <summary>
            ''' Computes ICC(3,k): two-way mixed effects intraclass correlation for consistency of the mean of k raters/measurements,
            ''' including an F-based confidence interval.
            ''' </summary>
            ''' <param name="x">
            ''' Rectangular data matrix where rows are targets/subjects and columns are raters/judges/measurement methods.
            ''' The matrix must be complete (no missing values) with one observation per target×rater cell.
            ''' </param>
            ''' <param name="alpha">
            ''' Optional two-sided significance level used for the confidence interval. 
            ''' The default is <c>0.05</c>, corresponding to a 95% confidence interval.
            ''' </param>
            ''' <returns>
            ''' A <see cref="ConfidenceIntervalResult"/> containing the ICC(3,k) estimate and CI.
            ''' </returns>
            ''' <remarks>
            ''' <para>
            ''' ICC(3,k) measures the reliability (consistency) of the average of k <i>fixed</i> raters/measurements.
            ''' It is appropriate when the raters in the study are the only raters of interest (no generalization).
            ''' </para>
            '''
            ''' <para>
            ''' Mean squares are obtained using <c>parametric.OneWayRmANOVA</c>.
            ''' Point estimate:
            ''' <c>ICC(3,k) = (MSR - MSE) / MSR</c>,
            ''' which is equivalent to transforming ICC(3,1) to the average-measures scale.
            ''' </para>
            '''
            ''' <para>
            ''' The confidence interval is computed using F-distribution bounds on <c>F = MSR/MSE</c>.
            ''' Since <c>ICC(3,k) = 1 - 1/F</c>, bounds can be obtained by transforming F bounds directly:
            ''' <c>Lower = (F_L - 1)/F_L</c>, <c>Upper = (F_U - 1)/F_U</c>.
            ''' </para>
            '''
            ''' <para>
            ''' CI correctness depends on <c>distributions.F_Inv</c> returning a lower-tail quantile.
            ''' </para>
            ''' </remarks>
            Public Function ICC3k(x(,) As Double, Optional alpha As Double = 0.05) As ConfidenceIntervalResult
                ' ICC(3,k): two-way mixed effects, consistency, average of k raters
                ' x: rows = targets/subjects, columns = raters/judges (balanced & complete)

                Dim n As Integer = x.GetLength(0) ' number of targets
                Dim k As Integer = x.GetLength(1) ' number of raters

                If n < 2 Then AppGlobals.BSerr.LogAndThrow(New ArgumentException("ICC(3,k) requires at least 2 targets (rows)."))
                If k < 2 Then AppGlobals.BSerr.LogAndThrow(New ArgumentException("ICC(3,k) requires at least 2 raters (columns)."))

                Dim vars(k - 1) As String
                Dim anova = New parametric.OneWayRmANOVA(x, vars)
                Dim atab = anova.compute()

                ' OneWayRmANOVA layout:
                ' 0 = Between groups (columns/raters): SS, df, MS, F, p
                ' 1 = Subjects (rows/targets):         SS, df, MS, F, p
                ' 2 = Error:                           SS, df, MS
                Dim MSR As Double = CDbl(atab(1, 2)) ' rows (targets)
                Dim MSE As Double = CDbl(atab(2, 2)) ' residual (interaction + error)

                Dim dfR As Integer = CInt(atab(1, 1)) ' n - 1
                Dim dfE As Integer = CInt(atab(2, 1)) ' (n - 1)(k - 1)

                If MSE <= 0 Then AppGlobals.BSerr.LogAndThrow(New ArgumentException("MSE <= 0; ICC(3,k) is undefined (check data)."))
                If MSR <= 0 Then AppGlobals.BSerr.LogAndThrow(New ArgumentException("MSR <= 0; ICC(3,k) is undefined (check data)."))

                ' ICC(3,k) point estimate:
                ' ICC(3,k) = (MSR - MSE) / MSR
                Dim ICC As Double = (MSR - MSE) / MSR

                Dim out As New ConfidenceIntervalResult With {
                    .Estimate = ICC,
                    .alpha = alpha}

                ' CI via F = MSR/MSE (assumes distributions.F_Inv is LOWER-tail quantile: P(F <= x) = p)
                Dim Fobs As Double = MSR / MSE

                Dim q1 As Double = distributions.F_Inv(1.0 - alpha / 2.0, dfR, dfE)
                Dim q2 As Double = distributions.F_Inv(1.0 - alpha / 2.0, dfE, dfR)

                Dim Fl As Double = Fobs / q1
                Dim Fu As Double = Fobs * q2

                ' Since ICC(3,k) = 1 - 1/F, transform bounds directly:
                out.LowerLimit = (Fl - 1.0) / Fl
                out.UpperLimit = (Fu - 1.0) / Fu

                Return out
            End Function


            Public Function wrapResults(ic As ConfidenceIntervalResult, type As String) As List(Of ResultTable)
                Dim out = New List(Of ResultTable)
                Dim t = New ResultTable

                If pRepeatabilityCoefficient Is Nothing Then
                    t.SetBody({{$"{type}", ic.Estimate},
                              {ic.CIlabel, ic.strConfidenceInterval(CIformat.LL_to_UL)}})
                Else
                    t.SetBody({{$"{type}", ic.Estimate},
                               {ic.CIlabel, ic.strConfidenceInterval(CIformat.LL_to_UL)},
                               {$"Repeatability Coefficient (for alpha = {pRepeatabilityCoefficient.alpha})", pRepeatabilityCoefficient.Estimate},
                               {pRepeatabilityCoefficient.CIlabel, pRepeatabilityCoefficient.strConfidenceInterval(CIformat.LL_to_UL)},
                               {"SEM (standard error of measurement)", pRepeatabilityCoefficient.StdErr}})
                End If

                t.AddHeaderTopRow({"Intraclass correlation coefficient", ""})
                out.Add(t)

                Return out
            End Function

            ''' <summary>
            ''' Computes the effective group size n0 used in some one-way ICC(1,·) formulas for unbalanced designs.
            ''' </summary>
            ''' <param name="x">
            ''' Jagged array grouped by target/subject; each <c>x(i)</c> contains measurements for target i.
            ''' </param>
            ''' <returns>
            ''' Effective group size n0 defined as:
            ''' <c>n0 = (1/(g-1)) * (n - (Σ n_i^2)/n)</c>,
            ''' where g is the number of targets/groups and n is the total number of observations.
            ''' </returns>
            ''' <remarks>
            ''' <para>
            ''' For balanced data (all n_i equal), n0 equals the common group size.
            ''' </para>
            ''' <para>
            ''' n0 is used to adjust ICC(1,1) formulas to accommodate unequal group sizes.
            ''' </para>
            ''' </remarks>
            Private Shared Function EffectiveGroupSizeN0_ICC11(x()() As Double) As Double
                Dim k As Integer = x.Length
                If k < 2 Then AppGlobals.BSerr.LogAndThrow(New ArgumentException("At least two groups are required."))

                Dim n As Integer = 0
                Dim sumNiSq As Double = 0.0

                For i As Integer = 0 To k - 1
                    Dim ni As Integer = x(i).Length
                    If ni = 0 Then AppGlobals.BSerr.LogAndThrow(New ArgumentException($"Group {i} is empty."))

                    n += ni
                    sumNiSq += ni * ni
                Next

                Return (n - sumNiSq / n) / (k - 1)
            End Function

            ''' <summary>
            ''' Repeatability Coefficient (RC) / SEM based on one-way ANOVA within-target variance (MSW),
            ''' consistent with ICC(1,1) (single) and ICC(1,k) (average-measures) as implemented in this class.
            '''
            ''' For ICC(1,1):
            '''   SEM = sqrt(MSW)
            ''' For ICC(1,k):
            '''   SEM = sqrt(MSW / n0) where n0 is the effective group size used by ICC(1,k)
            '''
            ''' RC = z_{1-α/2} * sqrt(2) * SEM
            ''' </summary>
            ''' <param name="x">
            ''' Jagged array of observations grouped by target/subject. Each x(i) contains repeated measurements for target i.
            ''' </param>
            ''' <param name="averageMeasures">
            ''' False => ICC(1,1) style (single measurement).
            ''' True  => ICC(1,k) style (mean of measurements) using effective group size n0.
            ''' </param>
            ''' <param name="alpha"> 
            ''' Optional two-sided significance level used for the confidence interval. 
            ''' The default is <c>0.05</c>, corresponding to a 95% confidence interval. 
            ''' </param>
            Public Function RepeatabilityCoefficient_OneWay(x()() As Double, averageMeasures As Boolean,
                                                            Optional alpha As Double = 0.05) As ConfidenceIntervalResult

                If x Is Nothing OrElse x.Length < 2 Then AppGlobals.BSerr.LogAndThrow(New ArgumentException("At least 2 targets/groups are required."))

                Dim vars(x.Length - 1) As String
                Dim anova = New parametric.OneWayANOVA(x, vars)
                Dim atab = anova.compute()

                Dim msw As Double = CDbl(atab(1, 2))   ' within/error MS
                Dim dfw As Integer = CInt(atab(1, 1))  ' within df = n_tot - g

                If dfw <= 0 Then AppGlobals.BSerr.LogAndThrow(New ArgumentException("Invalid within degrees of freedom."))
                If msw < 0 Then AppGlobals.BSerr.LogAndThrow(New ArgumentException("MSw < 0 is invalid."))

                ' Effective k used for average-measures ICC(1,k)
                Dim kEff As Double = 1.0
                If averageMeasures Then
                    kEff = EffectiveGroupSizeN0_ICC11(x)
                    If kEff <= 0 Then AppGlobals.BSerr.LogAndThrow(New ArgumentException("Effective group size n0 <= 0 is invalid."))
                End If

                ' For average-measures: Var(mean) = MSW / kEff
                Dim varUsed As Double = msw / kEff
                If varUsed <= 0 Then AppGlobals.BSerr.LogAndThrow(New ArgumentException("Computed variance <= 0; cannot compute SEM/RC."))

                Dim sem As Double = Math.Sqrt(varUsed)
                Dim z As Double = distributions.NormSInv(1.0 - alpha / 2.0)
                Dim rc As Double = z * Math.Sqrt(2.0) * sem

                ' CI for MSW via chi-square on variance, then apply the same /kEff scaling
                Dim chiUpper As Double = distributions.ChiSquareInv(1.0 - alpha / 2.0, dfw)
                Dim chiLower As Double = distributions.ChiSquareInv(alpha / 2.0, dfw)

                Dim mswL As Double = (dfw * msw) / chiUpper
                Dim mswU As Double = (dfw * msw) / chiLower

                Dim semL As Double = Math.Sqrt(mswL / kEff)
                Dim semU As Double = Math.Sqrt(mswU / kEff)

                pRepeatabilityCoefficient = New ConfidenceIntervalResult With {
                    .Estimate = rc,
                    .LowerLimit = z * Math.Sqrt(2.0) * semL,
                    .UpperLimit = z * Math.Sqrt(2.0) * semU,
                    .StdErr = sem,   ' SEM (within-target SD for the selected unit)
                    .alpha = alpha}

                Return pRepeatabilityCoefficient
            End Function


            ''' <summary>
            ''' Model-consistent Repeatability Coefficient (RC) / SEM for two-way ICC designs.
            ''' Use this for ICC(2,·) (two-way random, absolute agreement) and ICC(3,·) (two-way mixed, consistency).
            '''
            ''' The function derives variance components from the same ANOVA quantities used by ICC(2,·)/ICC(3,·):
            ''' MSC (columns/raters), MSR (rows/targets), MSE (residual).
            '''
            ''' Definitions:
            '''   Lambda/ICC family choice determines whether rater variance is included (agreement) or not (consistency).
            '''   SEM = sqrt(Var(measurement error for one observation or for mean-of-k observations))
            '''   RC  = z_{1-α/2} * sqrt(2) * SEM
            '''
            ''' For ICC(3,·) consistency: Var = σ_e^2 ≈ MSE
            ''' For ICC(2,·) agreement:   Var = σ_e^2 + σ_r^2,  where σ_r^2 ≈ max((MSC - MSE)/n, 0)
            '''
            ''' For average-measures (·,k): Var is divided by k (mean of k raters/replicates).
            '''
            ''' CI: Uses a chi-square CI for the variance with an effective df (Satterthwaite approximation).
            ''' When only MSE is used (consistency), df_eff = dfE and the CI reduces to the usual chi-square variance CI.
            ''' </summary>
            ''' <param name="x">MatrixType: rows = targets, columns = raters (complete, balanced).</param>
            ''' <param name="includeRaterVariance">
            ''' True for ICC(2,·) absolute agreement (includes σ_r^2), False for ICC(3,·) consistency (excludes σ_r^2).
            ''' </param>
            ''' <param name="averageMeasures">
            ''' True for (·,k) (mean of k raters), False for (·,1) (single rater).
            ''' </param>
            ''' <param name="alpha"> 
            ''' Optional two-sided significance level used for the confidence interval. 
            ''' The default is <c>0.05</c>, corresponding to a 95% confidence interval. 
            ''' </param>
            Public Function RepeatabilityCoefficient_TwoWay(x(,) As Double,
                                                            includeRaterVariance As Boolean,
                                                            averageMeasures As Boolean,
                                                            Optional alpha As Double = 0.05) As ConfidenceIntervalResult

                If x Is Nothing Then AppGlobals.BSerr.LogAndThrow(New ArgumentNullException(NameOf(x)))

                Dim n As Integer = x.GetLength(0) ' targets
                Dim k As Integer = x.GetLength(1) ' raters
                If n < 2 Then AppGlobals.BSerr.LogAndThrow(New ArgumentException("At least 2 targets (rows) are required."))
                If k < 2 Then AppGlobals.BSerr.LogAndThrow(New ArgumentException("At least 2 raters (columns) are required."))

                ' Two-way ANOVA without replication (same as ICC(2,·)/ICC(3,·) in this class)
                Dim vars(k - 1) As String
                Dim anova = New parametric.OneWayRmANOVA(x, vars)
                Dim atab = anova.compute()

                Dim MSC As Double = CDbl(atab(0, 2)) ' columns (raters)
                Dim MSR As Double = CDbl(atab(1, 2)) ' rows (targets)
                Dim MSE As Double = CDbl(atab(2, 2)) ' residual (interaction + error)

                Dim dfC As Integer = CInt(atab(0, 1)) ' k - 1
                Dim dfE As Integer = CInt(atab(2, 1)) ' (n - 1)(k - 1)

                If dfE <= 0 Then AppGlobals.BSerr.LogAndThrow(New ArgumentException("Invalid residual degrees of freedom."))
                If MSE <= 0 Then AppGlobals.BSerr.LogAndThrow(New ArgumentException("MSE <= 0; repeatability/SEM is undefined."))

                ' Variance components (classical two-way random/mixed decomposition)
                ' σ_e^2 estimated by MSE
                Dim sigmaE2 As Double = MSE

                ' σ_r^2 estimated by (MSC - MSE)/n, truncated at 0 (can be negative in finite samples)
                Dim sigmaR2 As Double = 0.0
                If includeRaterVariance Then
                    sigmaR2 = (MSC - MSE) / n
                    If sigmaR2 < 0.0 Then sigmaR2 = 0.0
                End If

                ' Measurement error variance for a single score or average of k raters
                Dim varSingle As Double = sigmaE2 + sigmaR2
                Dim varUsed As Double = If(averageMeasures, varSingle / k, varSingle)

                If varUsed <= 0 Then AppGlobals.BSerr.LogAndThrow(New ArgumentException("Computed variance <= 0; cannot compute SEM/RC."))

                ' SEM and RC
                Dim sem As Double = Math.Sqrt(varUsed)
                Dim z As Double = distributions.NormSInv(1.0 - alpha / 2.0)
                Dim rc As Double = z * Math.Sqrt(2.0) * sem

                ' --- Confidence interval for RC ---
                ' Use chi-square CI for variance with an effective df (Satterthwaite approximation).
                ' For consistency (sigmaR2 = 0): df_eff = dfE (exact chi-square CI for σ_e^2).
                ' For agreement: approximate df_eff using Var(MSE) and Var((MSC-MSE)/n); assumes independence (approx).
                Dim varVarSingle As Double

                If (Not includeRaterVariance) OrElse sigmaR2 = 0.0 Then
                    ' Exact variance CI based on MSE only
                    varVarSingle = 2.0 * sigmaE2 * sigmaE2 / dfE
                Else
                    ' Approximate variance of sigmaR2 = (MSC - MSE)/n:
                    ' Var(MSC) ≈ 2*MSC^2/dfC, Var(MSE) ≈ 2*MSE^2/dfE
                    Dim varMSC As Double = 2.0 * MSC * MSC / Math.Max(dfC, 1)
                    Dim varMSE As Double = 2.0 * MSE * MSE / dfE
                    Dim varSigmaR2 As Double = (varMSC + varMSE) / (n * n)

                    ' Var(sigmaE2 + sigmaR2) ≈ Var(MSE) + Var(sigmaR2)  (cov neglected)
                    varVarSingle = varMSE + varSigmaR2
                End If

                ' Satterthwaite df for varSingle
                Dim dfEff As Double = 2.0 * varSingle * varSingle / Math.Max(varVarSingle, 1.0E-30)

                ' CI on varSingle using chi-square with dfEff
                Dim chiUpper As Double = distributions.ChiSquareInv(1.0 - alpha / 2.0, dfEff)
                Dim chiLower As Double = distributions.ChiSquareInv(alpha / 2.0, dfEff)

                Dim varSingleL As Double = (dfEff * varSingle) / chiUpper
                Dim varSingleU As Double = (dfEff * varSingle) / chiLower

                ' Apply average-measures scaling if needed
                Dim varUsedL As Double = If(averageMeasures, varSingleL / k, varSingleL)
                Dim varUsedU As Double = If(averageMeasures, varSingleU / k, varSingleU)

                Dim rcL As Double = z * Math.Sqrt(2.0) * Math.Sqrt(varUsedL)
                Dim rcU As Double = z * Math.Sqrt(2.0) * Math.Sqrt(varUsedU)

                pRepeatabilityCoefficient = New ConfidenceIntervalResult With {
                    .Estimate = rc,
                    .LowerLimit = rcL,
                    .UpperLimit = rcU,
                    .StdErr = sem,
                    .alpha = alpha}

                Return pRepeatabilityCoefficient
            End Function


        End Class
    End Module

End Namespace