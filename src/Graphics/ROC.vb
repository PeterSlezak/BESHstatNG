Option Explicit On
Imports System.Drawing
Imports System.Security.Policy
Imports Microsoft.ApplicationInsights.MetricDimensionNames.TelemetryContext
Imports Microsoft.Office
Imports Microsoft.Office.Interop.Excel

Namespace graphics


    ''' <summary>
    ''' Implements ROC (Receiver Operating Characteristic) analysis for a binary
    ''' classifier with one continuous marker.  
    ''' 
    ''' The class:
    ''' <list type="bullet">
    '''   <item><description>Computes ROC curve points (sensitivity vs. 1 − specificity)</description></item>
    '''   <item><description>Computes Wilcoxon (Mann–Whitney) AUC</description></item>
    '''   <item><description>Computes the standard error of AUC</description></item>
    '''   <item><description>Tests H₀: AUC = 0.5 with a normal approximation</description></item>
    '''   <item><description>Builds a (1 − α) AUC confidence interval</description></item>
    '''   <item><description>Prepares cut‑off, sensitivity, and specificity tables</description></item>
    '''   <item><description>Draws ROC curve in Excel</description></item>
    ''' </list>
    ''' 
    ''' Convention:
    ''' <list type="bullet">
    '''   <item><description><c>data(0)</c> = marker values in the “patient”/positive group</description></item>
    '''   <item><description><c>data(1)</c> = marker values in the “control”/negative group</description></item>
    ''' </list>
    ''' 
    ''' AUC and its standard error follow:
    ''' <para>
    ''' Hanley &amp; McNeil (1982), "The Meaning and Use of the Area under a Receiver
    ''' Operating Characteristic (ROC) Curve", Radiology 143:29–36.
    ''' </para>
    ''' 
    ''' External dependencies:
    ''' <list type="bullet">
    '''   <item><description><c>ConcatArrays</c> — concatenates vectors</description></item>
    '''   <item><description><c>PNorm</c>, <c>NormSInv</c> — normal CDF and its inverse</description></item>
    '''   <item><description><c>ConfidenceIntervalResult</c> — container for CI</description></item>
    '''   <item><description><c>ResultTable</c> — tabular output</description></item>
    '''   <item><description>Excel interop (<c>Worksheet</c>, <c>Chart</c>)</description></item>
    ''' </list>
    ''' </summary>
    Public Class ROC
        ''' <summary>
        ''' Input data arrays:
        ''' <list type="bullet">
        '''   <item><description><c>data(0)</c> = patient/positive group</description></item>
        '''   <item><description><c>data(1)</c> = control/negative group</description></item>
        ''' </list>
        ''' </summary>
        Private data()() As Double

        ''' <summary>Optional variable names for labeling.</summary>
        Private varNames() As String

        ''' <summary>Area under the ROC curve (AUC).</summary>
        Private pAUC As Double

        ''' <summary>Standard error of AUC.</summary>
        Private pseAUC As Double

        ''' <summary>Two‑sided p‑value for H₀: AUC = 0.5.</summary>
        Private pPvalue As Double

        ''' <summary>Cut‑off values between distinct marker values.</summary>
        Private parCutOff() As Double

        ''' <summary>Sensitivity at each cut‑off.</summary>
        Private parSensitivity() As Double

        ''' <summary>Specificity at each cut‑off.</summary>
        Private parSpecificity() As Double

        ''' <summary>1 − specificity (false positive rate) for ROC plotting.</summary>
        Private par1minusSpec() As Double

        ''' <summary>Confidence interval for AUC.</summary>
        Private pCI As ConfidenceIntervalResult

        Private Property pdelongSE As Double
        Private Property pdelongCI As ConfidenceIntervalResult

        ''' <summary>
        ''' Initializes the ROC analysis object.
        ''' </summary>
        ''' <param name="x">
        ''' Jagged array of two groups:
        ''' <list type="bullet">
        '''   <item><description><c>x(0)</c> = patient/positive group</description></item>
        '''   <item><description><c>x(1)</c> = control/negative group</description></item>
        ''' </list>
        ''' </param>
        ''' <param name="varNames">Optional variable names for labeling output.</param>
        Sub New(x()() As Double, varNames() As String)
            'id 0 = patient data; id 1 = control group data
            Me.data = x
            Me.varNames = varNames
        End Sub

        ''' <summary>
        ''' Wraps ROC results into formatted tables:
        ''' <list type="bullet">
        '''   <item><description>Overall ROC summary (AUC, CI, SE, p‑value) (Note: DeLong's and Hanley–McNeil based SEs)</description></item>
        '''   <item><description>Cut‑off table (cut‑off, sensitivity, specificity)</description></item>
        ''' </list>
        ''' 
        ''' External dependency:
        ''' <list type="bullet">
        '''   <item><description><c>ResultTable</c></description></item>
        ''' </list>
        ''' </summary>
        ''' <returns>
        ''' A list of <see cref="ResultTable"/> objects:
        ''' <list type="bullet">
        '''   <item><description>First table: AUC summary</description></item>
        '''   <item><description>Second table: cut‑off, sensitivity, specificity</description></item>
        ''' </list>
        ''' </returns>
        Public Function wrapResults() As List(Of ResultTable)
            Dim out = New List(Of ResultTable), t = New ResultTable

            t.SetBody({{"Wilcoxon AUC", Me.pAUC},
                {"DeLong 95% Confidence Interval", Me.pdelongCI.strConfidenceInterval(CIformat.LL_to_UL)},
                {"DeLong Standard error", Me.pdelongSE},
                {"Hanley–McNeil 95% Confidence Interval", Me.pCI.strConfidenceInterval(CIformat.LL_to_UL)},
                {"Hanley–McNeil Standard error", Me.pseAUC},
                {"Two-sided p-value (AUC different from 0.5)", Me.pPvalue}})
            t.AddHeaderTopRow({"Receiver Operating Characteristic (ROC) Curve", ""})
            out.Add(t)

            t = New ResultTable
            Dim cutoffs(Me.parCutOff.Length - 1, 2) As Object
            For i = 0 To Me.parCutOff.Length - 1
                cutoffs(i, 0) = parCutOff(i)
                cutoffs(i, 1) = parSensitivity(i)
                cutoffs(i, 2) = parSpecificity(i)
            Next
            t.SetBody(cutoffs)
            t.AddHeaderTopRow({"Cut-Off", "Sensitivity", "Specificity"})
            out.Add(t)

            Return out
        End Function

        ''' <summary>
        ''' Computes ROC curve, Wilcoxon AUC, its standard error, p‑value for H₀: AUC = 0.5,
        ''' and a (1 − α) confidence interval for AUC.
        ''' 
        ''' Steps:
        ''' <list type="number">
        '''   <item><description>Concatenate patient and control data and sort by marker value.</description></item>
        '''   <item><description>Define cut‑offs as midpoints between successive distinct values
        '''     (last cut‑off above the maximum).</description></item>
        '''   <item><description>For each cut‑off, compute sensitivity and specificity.</description></item>
        '''   <item><description>Compute Wilcoxon AUC using cumulated group counts (Mann–Whitney form).</description></item>
        '''   <item><description>Compute AUC standard error using Hanley–McNeil Q₁, Q₂ formulas.</description></item>
        '''   <item><description>Compute z‑test for H₀: AUC = 0.5 and corresponding p‑value.</description></item>
        '''   <item><description>Construct normal‑approximation CI: AUC ± z_{1−α/2}·SE(AUC).</description></item>
        ''' </list>
        ''' 
        ''' External dependencies:
        ''' <list type="bullet">
        '''   <item><description><c>ConcatArrays</c></description></item>
        '''   <item><description><c>PNorm</c>, <c>NormSInv</c></description></item>
        '''   <item><description><c>ConfidenceIntervalResult</c></description></item>
        ''' </list>
        ''' </summary>
        ''' <param name="alpha">Significance level for the AUC confidence interval (default 0.05).</param>
        Public Sub compute(Optional alpha As Double = 0.05)
            Dim Data12() As Double, arIDs() As Integer
            Dim arPatientsGroupNo() As Double, arControlsGroupNo() As Double, arPatientsCum() As Double 'Wilcoxon AUC calculation
            Dim arContCum() As Double, dQ1SE As Double, dQ2SE As Double  'Wilcoxon AUC and seAUC calculation
            Dim a As Integer, b As Integer, c As Integer, d As Integer 'a, b, c, d are rates for calculating sens. and spec.

            'calculate points for ROC plot and AUC calculation--------------
            Dim n1 As Integer = data(0).Length
            Dim n2 As Integer = data(1).Length
            Dim n As Integer = n1 + n2

            ReDim arIDs(n - 1)

            Data12 = ConcatArrays(data(0), data(1))
            For i = 0 To n - 1
                arIDs(i) = If(i <= (n1 - 1), 1, 2) '1 denote patients
            Next

            Array.Sort(Data12, arIDs)

            Dim arUnique() As Double = Data12.Distinct().ToArray()
            Dim NoUniqueVals As Integer = arUnique.Length

            'NoUniqueVals is now # of all unique data values

            ReDim parCutOff(NoUniqueVals - 1), parSensitivity(0 To NoUniqueVals) '0 because of ploting (1st point come default from 1,1)
            ReDim parSpecificity(NoUniqueVals - 1), par1minusSpec(0 To NoUniqueVals) '0 because of ploting (1st point come default from 1,1)
            ReDim arPatientsGroupNo(NoUniqueVals - 1), arControlsGroupNo(NoUniqueVals - 1)
            ReDim arPatientsCum(NoUniqueVals - 1), arContCum(NoUniqueVals - 1)

            'calculate averages of adjecent unique values, which will be used as cut-off points
            For i = 0 To NoUniqueVals - 2
                parCutOff(i) = (arUnique(i) + arUnique(i + 1)) / 2.0
            Next
            parCutOff(NoUniqueVals - 1) = arUnique(NoUniqueVals - 1) + 1
            'compute senitivity and specificity for all unique values
            'note that Data12 is sorted

            'calculation of AUC and its standard error is according J.A. Hanley, and B.J. McNeil. The Meaning and Use of the Area
            'under a Receiver Operating Characteristic (ROC) Curve RADIOLOGY, Vol. l43. No. l, Pages 29-36, 1982
            Dim j As Integer = 0
            arPatientsCum(0) = n1

            For i = 0 To NoUniqueVals - 1
                Do While Data12(j) < parCutOff(i)
                    If arIDs(j) = 1 Then
                        c += 1
                        arPatientsGroupNo(i) += 1
                    Else
                        d += 1
                        arControlsGroupNo(i) += 1
                    End If
                    j += 1
                    If j = n Then Exit Do
                Loop

                a = n1 - c
                b = n2 - d
                parSensitivity(i + 1) = (a / (a + c))
                parSpecificity(i) = (d / (b + d))
                par1minusSpec(i + 1) = 1.0 - parSpecificity(i)

                'wilcoxon AUC calculation
                If i = 0 Then
                    arPatientsCum(i) -= arPatientsGroupNo(i)
                Else   '1st value is always zero
                    arPatientsCum(i) = arPatientsCum(i - 1) - arPatientsGroupNo(i)
                    arContCum(i) = arContCum(i - 1) + arControlsGroupNo(i - 1)
                End If
                pAUC += (arControlsGroupNo(i) * arPatientsCum(i) + 0.5 * arControlsGroupNo(i) * arPatientsGroupNo(i))
                dQ2SE += (arPatientsGroupNo(i) * (arContCum(i) ^ 2 + arContCum(i) * arControlsGroupNo(i) +
                            1 / 3 * arControlsGroupNo(i) ^ 2))
                dQ1SE += (arControlsGroupNo(i) * (arPatientsCum(i) ^ 2 + arPatientsCum(i) * arPatientsGroupNo(i) +
                            1 / 3 * arPatientsGroupNo(i) ^ 2))
            Next
            parSensitivity(0) = 1.0            'for ploting ROC
            par1minusSpec(0) = 1.0             'for ploting ROC
            parSensitivity(NoUniqueVals) = 0.0 'for ploting ROC
            par1minusSpec(NoUniqueVals) = 0.0  'for ploting ROC

            pAUC = pAUC / (n1 * n2)
            dQ2SE = dQ2SE / (n1 * n2 ^ 2)
            dQ1SE = dQ1SE / (n2 * n1 ^ 2)
            pseAUC = Math.Sqrt((pAUC * (1.0 - pAUC) + (CDbl(n1) - 1) * (dQ1SE - pAUC * pAUC) + (CDbl(n2) - 1) * (dQ2SE - pAUC * pAUC)) / (CDbl(n1) * CDbl(n2)))
            Dim SEforPvalue As Double = Math.Sqrt((0.25 + (n1 + n2 - 2) * (1.0 / 12.0)) / ((CDbl(n1) * CDbl(n2))))
            pPvalue = 2 * distributions.PNorm(-Math.Abs(pAUC - 0.5) / SEforPvalue)

            Me.pCI = New ConfidenceIntervalResult
            Dim q As Double = distributions.NormSInv(1.0 - alpha / 2.0)
            Me.pCI.Estimate = pAUC
            Me.pCI.LowerLimit = pAUC - q * pseAUC
            Me.pCI.UpperLimit = pAUC + q * pseAUC

            Me.DeLongSE(alpha)
        End Sub



        ''' <summary>
        ''' Computes the ROC AUC and DeLong's nonparametric standard error for two independent groups.
        ''' </summary>
        ''' <param name="alpha">Significance level for the AUC confidence interval (default 0.05).</param>
        ''' <returns>
        ''' A <see cref="Double"/> containing the AUC, DeLong variance, and standard error.
        ''' </returns>
        ''' <exception cref="ArgumentNullException">
        ''' Thrown when data for patient/controls is <c>Nothing</c>.
        ''' </exception>
        ''' <exception cref="ArgumentException">
        ''' Thrown when either group has fewer than 2 observations (variance estimation requires at least two per group).
        ''' </exception>
        ''' <remarks>
        ''' <para><b>AUC (Wilcoxon / Mann–Whitney):</b></para>
        ''' <para>
        ''' Let <c>X = {X_i}</c> be patient values (<c>i=1..m</c>) and <c>Y = {Y_j}</c> be control values (<c>j=1..n</c>).
        ''' Define the kernel
        ''' <c>phi(x,y) = 1</c> if <c>x &gt; y</c>, <c>0.5</c> if <c>x = y</c>, and <c>0</c> if <c>x &lt; y</c>.
        ''' Then
        ''' <c>AUC = (1/(m*n)) * Sum_i Sum_j phi(X_i, Y_j)</c>.
        ''' This equals the Mann–Whitney U statistic normalized by <c>m*n</c>.
        ''' </para>
        ''' <para><b>DeLong variance:</b></para>
        ''' <para>
        ''' DeLong's method treats AUC as a U-statistic and estimates its variance via "influence values"
        ''' for each observation:
        ''' <c>V_i = (1/n) * Sum_j phi(X_i, Y_j)</c> for patients, and
        ''' <c>W_j = (1/m) * Sum_i phi(X_i, Y_j)</c> for controls.
        ''' The variance is then
        ''' <c>Var(AUC) = Var(V)/m + Var(W)/n</c>,
        ''' where <c>Var(V)</c> and <c>Var(W)</c> are sample variances of the vectors <c>{V_i}</c> and <c>{W_j}</c>.
        ''' </para>
        ''' <para><b>Ties:</b></para>
        ''' <para>
        ''' This implementation uses midranks (average ranks within tied blocks) when computing
        ''' the equivalent DeLong influence values. This is consistent with the AUC definition
        ''' that credits ties as 0.5 and yields tie-safe variance estimates in practice.
        ''' </para>
        ''' <para><b>Reference:</b> DeLong ER, DeLong DM, Clarke-Pearson DL (1988).
        ''' "Comparing the areas under two or more correlated receiver operating characteristic curves."
        ''' <i>Biometrics</i>.</para>
        ''' </remarks>
        Private Function DeLongSE(Optional alpha As Double = 0.05) As Double
            Dim patients As Double() = data(0)
            Dim controls As Double() = data(1)
            If patients Is Nothing OrElse controls Is Nothing Then Throw New ArgumentNullException()
            Dim m As Integer = patients.Length
            Dim n As Integer = controls.Length
            If m < 2 OrElse n < 2 Then Throw New ArgumentException("Need at least 2 observations per group for DeLong SE.")

            ' Combined scores and labels (1=patient, 0=control)
            Dim total As Integer = m + n
            Dim scores(total - 1) As Double
            Dim isPatient(total - 1) As Boolean

            For i = 0 To m - 1
                scores(i) = patients(i)
                isPatient(i) = True
            Next
            For j = 0 To n - 1
                scores(m + j) = controls(j)
                isPatient(m + j) = False
            Next

            ' Midranks on combined scores (1..total), tie-safe
            Dim ranks As Double() = MidRanks(scores)

            ' Split ranks back into patient/control arrays
            Dim rx(m - 1) As Double
            Dim ry(n - 1) As Double
            Dim ix As Integer = 0, iy As Integer = 0
            For k = 0 To total - 1
                If isPatient(k) Then
                    rx(ix) = ranks(k) : ix += 1
                Else
                    ry(iy) = ranks(k) : iy += 1
                End If
            Next

            ' AUC from ranks (equivalent to Wilcoxon/Mann–Whitney with 0.5 ties)
            ' U = sum(rx) - m(m+1)/2 ; AUC = U / (m n)
            Dim sumRx As Double = 0
            For i = 0 To m - 1 : sumRx += rx(i) : Next
            Dim U As Double = sumRx - (m * (m + 1)) / 2.0
            Dim auc As Double = U / (m * n)

            ' DeLong "placement values":
            ' V10_i = (rx_i - i) / n    where rx_i are ranks among combined and i is rank among X when sorted by score
            ' V01_j = (ry_j - j) / m
            ' With ties, we need patient-ranks among patients and control-ranks among controls using midranks too.
            ' We compute within-group midranks on each group, then apply the standard DeLong formulation:
            '   v_i = (R_xi - R_xi_within) / n
            '   w_j = (R_yj - R_yj_within) / m
            Dim rxWithin As Double() = MidRanks(patients)
            Dim ryWithin As Double() = MidRanks(controls)

            ' Important: MidRanks(patients) returns ranks in the ORIGINAL ORDER of patients array
            ' Same for controls. rx and ry are ranks in combined order; we need combined ranks per original obs.
            ' Easiest: compute combined ranks directly per original obs positions.
            ' We'll rebuild combined ranks in original order:
            Dim rxCombined(m - 1) As Double
            Dim ryCombined(n - 1) As Double
            ' Use same MidRanks(scores) but map back:
            For i = 0 To m - 1
                rxCombined(i) = ranks(i)
            Next
            For j = 0 To n - 1
                ryCombined(j) = ranks(m + j)
            Next

            Dim v(m - 1) As Double
            For i = 0 To m - 1
                v(i) = (rxCombined(i) - rxWithin(i)) / n
            Next

            Dim w(n - 1) As Double
            For j = 0 To n - 1
                w(j) = (ryCombined(j) - ryWithin(j)) / m
            Next

            Dim meanV As Double = 0, meanW As Double = 0
            For i = 0 To m - 1 : meanV += v(i) : Next
            For j = 0 To n - 1 : meanW += w(j) : Next
            meanV /= m : meanW /= n

            ' Sample variances of v and w
            Dim sV As Double = 0
            For i = 0 To m - 1
                Dim d As Double = v(i) - meanV
                sV += d * d
            Next
            sV /= (m - 1)

            Dim sW As Double = 0
            For j = 0 To n - 1
                Dim d As Double = w(j) - meanW
                sW += d * d
            Next
            sW /= (n - 1)

            Dim varAuc As Double = (sV / m) + (sW / n)
            If varAuc < 0 Then varAuc = 0 ' numerical guard
            Me.pdelongSE = Math.Sqrt(varAuc)

            Me.pdelongCI = New ConfidenceIntervalResult
            Dim q As Double = distributions.NormSInv(1.0 - alpha / 2.0)
            Me.pdelongCI.Estimate = auc
            Me.pdelongCI.LowerLimit = auc - q * Me.pdelongSE
            Me.pdelongCI.UpperLimit = auc + q * Me.pdelongSE

            Return Me.pdelongSE
        End Function

        ''' <summary>
        ''' Computes tie-corrected midranks (average ranks) for a numeric vector.
        ''' </summary>
        ''' <param name="values">Input values.</param>
        ''' <returns>
        ''' An array of ranks in the <b>original input order</b>, using 1-based ranks with ties assigned
        ''' the average of their occupied ranks (midrank).
        ''' </returns>
        ''' <remarks>
        ''' <para>
        ''' If values are sorted and a tie block occupies positions <c>k..l</c> (1-based),
        ''' each tied value receives rank <c>(k+l)/2</c>.
        ''' </para>
        ''' <para>
        ''' Midranks provide a tie-safe way to compute Mann–Whitney/Wilcoxon quantities and are used
        ''' by DeLong-type algorithms to ensure ties contribute <c>0.5</c> in AUC computations.
        ''' </para>
        ''' </remarks>
        Friend Shared Function MidRanks(values As Double()) As Double()
            Dim n As Integer = values.Length
            Dim idx(n - 1) As Integer
            For i = 0 To n - 1 : idx(i) = i : Next

            Array.Sort(idx, Function(a, b) values(a).CompareTo(values(b)))

            Dim ranks(n - 1) As Double
            Dim iPos As Integer = 0
            While iPos < n
                Dim jPos As Integer = iPos
                Dim v As Double = values(idx(iPos))
                While jPos + 1 < n AndAlso values(idx(jPos + 1)) = v
                    jPos += 1
                End While

                ' Average rank for tie block [iPos..jPos], ranks are 1-based
                Dim rankLo As Double = iPos + 1
                Dim rankHi As Double = jPos + 1
                Dim avg As Double = (rankLo + rankHi) / 2.0

                For k = iPos To jPos
                    ranks(idx(k)) = avg
                Next

                iPos = jPos + 1
            End While

            Return ranks
        End Function


        ''' <summary>
        ''' Creates an ROC curve plot in Excel based on previously computed
        ''' sensitivity and 1 − specificity arrays.
        ''' 
        ''' The chart includes:
        ''' <list type="bullet">
        '''   <item><description>ROC curve (sensitivity vs. 1 − specificity)</description></item>
        '''   <item><description>Diagonal reference line from (0,0) to (1,1)</description></item>
        '''   <item><description>Proper axis scaling (0 to 1 on both axes)</description></item>
        '''   <item><description>Axis titles and chart title</description></item>
        ''' </list>
        ''' 
        ''' External dependency:
        ''' <list type="bullet">
        '''   <item><description>Excel interop (<c>Worksheet</c>, <c>Chart</c>)</description></item>
        ''' </list>
        ''' </summary>
        ''' <param name="ws">Worksheet where the ROC plot will be created.</param>
        Public Sub addROCplot(ws As Worksheet)

            With ws.Shapes.AddChart(Width:=300, Height:=270)
                With .Chart
                    .ChartType = XlChartType.xlXYScatterLines

                    'delete extra series
                    Do Until .SeriesCollection.Count = 0
                        .SeriesCollection(1).Delete
                    Loop

                    .SeriesCollection.NewSeries
                    With .SeriesCollection(1)
                        .XValues = par1minusSpec
                        .Values = parSensitivity
                        .Name = "ROC 1"
                        .Format.Line.Weight = 1.5
                        .MarkerStyle = 8
                        .MarkerSize = 5
                        .Border.Color = RGB(100, 100, 100)
                        .MarkerForegroundColor = RGB(100, 100, 100)
                        .MarkerBackgroundColor = RGB(100, 100, 100)
                    End With
                    With .Axes(XlAxisType.xlValue)
                        .MinimumScale = 0
                        .MaximumScale = 1
                        .CrossesAt = 0
                        .MajorUnit = 0.2
                        .MajorGridlines.Delete
                    End With
                    With .Axes(XlAxisType.xlCategory)
                        .MaximumScale = 1
                        .MinimumScale = 0
                        .CrossesAt = 0
                        .MajorUnit = 0.2
                        .MajorGridlines.Delete
                    End With

                    .Legend.Delete()
                    Try
                        .Axes(XlAxisType.xlValue, XlAxisGroup.xlPrimary).HasTitle = False
                        .Axes(XlAxisType.xlValue, XlAxisGroup.xlPrimary).HasTitle = True
                        .Axes(XlAxisType.xlValue, XlAxisGroup.xlPrimary).AxisTitle.text = "Sensitivity"
                        .Axes(XlAxisType.xlCategory, XlAxisGroup.xlPrimary).HasTitle = False
                        .Axes(XlAxisType.xlCategory, XlAxisGroup.xlPrimary).HasTitle = True
                        .Axes(XlAxisType.xlCategory, XlAxisGroup.xlPrimary).AxisTitle.text = "1 - Specificity"
                        .HasTitle = False
                        .HasTitle = True
                        .ChartTitle.Text = "ROC curve"
                    Catch
                    End Try

                    'add and plot reference line
                    .SeriesCollection.NewSeries
                    With .SeriesCollection(2)
                        .XValues = {0, 1}
                        .Values = {0, 1}
                        .MarkerStyle = -4142
                        .Border.Color = RGB(0, 0, 0)
                        With .Format.Line
                            .Visible = True
                            .Weight = 1.25
                        End With
                        .Name = "Reference Line"
                        .Format.Fill.Visible = False
                    End With
                End With
            End With
        End Sub

    End Class
End Namespace