Option Explicit On
Option Strict Off
Imports System
Imports System.Linq
Imports BESHStatNG
Imports BESHStatNG.StatFunc
Imports Matrix = Batch30Fixtures.LegacyMatrix30
Namespace Global.Batch30Fixtures
    'The unchanged Windows Matrix methods are direct forwarding facades.
    Public Module LegacyMatrix30
        Public Function GetColumnFrom2Darray(Of T)(x(,) As T, nCol As Integer) As T()
            Return BESHStatNG.DataManagement.ArrayUtilities.GetColumn(x, nCol)
        End Function
        Public Function CorrelMatrix(data(,) As Double, kind As String) As Double(,)
            'Friend Core provider: invoke the same forwarding target without a host assembly.
            Dim provider = GetType(BESHStatNG.DescriptiveStat).Assembly.GetType("BESHStatNG.Matrix.CorrelationMatrixCore", True)
            Return DirectCast(provider.GetMethod("Compute", System.Reflection.BindingFlags.Static Or System.Reflection.BindingFlags.Public).Invoke(Nothing, {data, kind}), Double(,))
        End Function
    End Module
    Public Class LegacyRoc30
        Friend data()() As Double

        Friend varNames() As String

        Friend pAUC As Double

        Friend pseAUC As Double

        Friend pPvalue As Double

        Friend parCutOff() As Double

        Friend parSensitivity() As Double

        Friend parSpecificity() As Double

        Friend par1minusSpec() As Double

        Friend pCI As ConfidenceIntervalResult

        Friend Property pdelongSE As Double
        Friend Property pdelongCI As ConfidenceIntervalResult

        Sub New(x()() As Double, varNames() As String)
            Me.data = x
            Me.varNames = varNames
        End Sub
        Public Sub compute(Optional alpha As Double = 0.05)
            Dim Data12() As Double, arIDs() As Integer
            Dim arPatientsGroupNo() As Double, arControlsGroupNo() As Double, arPatientsCum() As Double 'Wilcoxon AUC calculation
            Dim arContCum() As Double, dQ1SE As Double, dQ2SE As Double  'Wilcoxon AUC and seAUC calculation
            Dim a As Integer, b As Integer, c As Integer, d As Integer 'a, b, c, d are rates for calculating sens. and spec.

            Dim n1 As Integer = data(0).Length
            Dim n2 As Integer = data(1).Length
            Dim n As Integer = n1 + n2

            ReDim arIDs(n - 1)

            Data12 = Global.BESHStatNG.DataManagement.ArrayUtilities.ConcatArrays(data(0), data(1))
            For i = 0 To n - 1
                arIDs(i) = If(i <= (n1 - 1), 1, 2) '1 denote patients
            Next

            Array.Sort(Data12, arIDs)

            Dim arUnique() As Double = Data12.Distinct().ToArray()
            Dim NoUniqueVals As Integer = arUnique.Length


            ReDim parCutOff(NoUniqueVals - 1), parSensitivity(0 To NoUniqueVals) '0 because of ploting (1st point come default from 1,1)
            ReDim parSpecificity(NoUniqueVals - 1), par1minusSpec(0 To NoUniqueVals) '0 because of ploting (1st point come default from 1,1)
            ReDim arPatientsGroupNo(NoUniqueVals - 1), arControlsGroupNo(NoUniqueVals - 1)
            ReDim arPatientsCum(NoUniqueVals - 1), arContCum(NoUniqueVals - 1)

            For i = 0 To NoUniqueVals - 2
                parCutOff(i) = (arUnique(i) + arUnique(i + 1)) / 2.0
            Next
            parCutOff(NoUniqueVals - 1) = arUnique(NoUniqueVals - 1) + 1

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
            Me.pCI.alpha = alpha
            Dim q As Double = distributions.NormSInv(1.0 - alpha / 2.0)
            Me.pCI.Estimate = pAUC
            Me.pCI.LowerLimit = pAUC - q * pseAUC
            Me.pCI.UpperLimit = pAUC + q * pseAUC

            Me.DeLongSE(alpha)
        End Sub
        Friend Function DeLongSE(Optional alpha As Double = 0.05) As Double
            Dim patients As Double() = data(0)
            Dim controls As Double() = data(1)
            If patients Is Nothing OrElse controls Is Nothing Then Throw New ArgumentNullException()
            Dim m As Integer = patients.Length
            Dim n As Integer = controls.Length
            If m < 2 OrElse n < 2 Then Throw New ArgumentException("Need at least 2 observations per group for DeLong SE.")

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

            Dim ranks As Double() = MidRanks(scores)

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

            Dim sumRx As Double = 0
            For i = 0 To m - 1 : sumRx += rx(i) : Next
            Dim U As Double = sumRx - (m * (m + 1)) / 2.0
            Dim auc As Double = U / (m * n)

            Dim rxWithin As Double() = MidRanks(patients)
            Dim ryWithin As Double() = MidRanks(controls)

            Dim rxCombined(m - 1) As Double
            Dim ryCombined(n - 1) As Double
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
            Me.pdelongCI.alpha = alpha
            Me.pdelongCI.LowerLimit = auc - q * Me.pdelongSE
            Me.pdelongCI.UpperLimit = auc + q * Me.pdelongSE

            Return Me.pdelongSE
        End Function
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
    End Class
    Public Class LegacyNormal30
        Friend pData() As Double

        Friend pvarName As String

        Friend n As Integer

        Friend pZ() As Double

        Friend pP() As Double

        Friend pRefLine(1, 1) As Double

        Friend pstrNormalScores As String

        Sub New(arrData() As Double, varName As String)
            Array.Sort(arrData)
            pData = arrData
            Me.pvarName = varName
            n = pData.Length
        End Sub
        Public Function compute(strMethod As String) As Double()
            pstrNormalScores = strMethod

            ReDim pZ(n - 1), pP(n - 1)
            Dim ranks = nonparametric.ComputeAvgRanks(pData)

            If pstrNormalScores = "Blom" Then
                For i = 0 To n - 1
                    pP(i) = (ranks(i) - 0.375) / (n + 0.25)
                    pZ(i) = distributions.NormSInv(pP(i))
                Next
            ElseIf pstrNormalScores = "Rankit" Then
                For i = 0 To n - 1
                    pP(i) = (ranks(i) - 0.5) / n
                    pZ(i) = distributions.NormSInv(pP(i))
                Next
            ElseIf pstrNormalScores = "Van Der Waerden" Then
                For i = 0 To n - 1
                    pP(i) = ranks(i) / (n + 1)
                    pZ(i) = distributions.NormSInv(pP(i))
                Next
            End If

            Return pZ
        End Function
        Public Function computeRefLIne(strMethod As String) As Double(,)
            Dim dSlope As Double, dIntercept As Double

            If strMethod = "SPSS" Then
                Dim dMean As Double = pData.Average()
                Dim dSD As Double = stDev(pData)

                pRefLine(0, 0) = distributions.QNorm(pP.Min(), dMean, dSD)
                pRefLine(1, 0) = distributions.QNorm(pP.Max(), dMean, dSD)
                pRefLine(0, 1) = pZ.Min()
                pRefLine(1, 1) = pZ.Max()

                dSlope = Slope(Matrix.GetColumnFrom2Darray(pRefLine, 1), Matrix.GetColumnFrom2Darray(pRefLine, 0))
                dIntercept = Intercept(Matrix.GetColumnFrom2Darray(pRefLine, 1), Matrix.GetColumnFrom2Darray(pRefLine, 0))
            ElseIf strMethod = "OLS" Then
                dSlope = Slope(pZ, pData)
                dIntercept = Intercept(pZ, pData)
            ElseIf strMethod = "R" Then
                Dim dRankQ = {(RoundDown((0.25 * (n + 1)), 0) + RoundUp((0.25 * (n + 1)), 0)) / 2,
                              (RoundDown((0.75 * (n + 1)), 0) + RoundUp((0.75 * (n + 1)), 0)) / 2}

                Dim Quartiles = QuartilesComp(pData)
                Dim dQ() = {Quartiles.Q1, Quartiles.Q3}
                Dim ZQ() As Double = Nothing
                If pstrNormalScores = "Blom" Then
                    ZQ = {distributions.NormSInv((dRankQ(0) - 0.375) / (n + 0.25)), distributions.NormSInv((dRankQ(1) - 0.375) / (n + 0.25))}
                ElseIf pstrNormalScores = "Rankit" Then
                    ZQ = {distributions.NormSInv((dRankQ(0) - 0.5) / n), distributions.NormSInv((dRankQ(1) - 0.5) / n)}
                ElseIf pstrNormalScores = "Van Der Waerden" Then
                    ZQ = {distributions.NormSInv(dRankQ(0) / (n + 1)), distributions.NormSInv(dRankQ(1) / (n + 1))}
                End If

                dSlope = Slope(ZQ, dQ)
                dIntercept = Intercept(ZQ, dQ)
            End If

            pRefLine(0, 1) = dIntercept + dSlope * pData.Min()
            pRefLine(1, 1) = dIntercept + dSlope * pData.Max()
            pRefLine(0, 0) = pData.Min()
            pRefLine(1, 0) = pData.Max()

            Return pRefLine
        End Function
    End Class
Public Class LegacyScatter30
        Friend pData(,) As Double, n As Integer, p As Integer, pMargin As Double = 0.1
        Friend ScaledData(,) As Double, x_corr_labels() As Double, y_corr_labels() As Double, x_var_labels() As Double
        Friend CorrMatrix(,) As Double, x_regline() As Double, y_regline() As Double
        Friend pbDisplayCorrCoef As Boolean, pbDisplayRegLines As Boolean
        Friend Const gToDeleteGridLineValue As Double = -100
        Sub transformData()
            Dim tmp() As Double, tmp2() As Double

            Dim min_old(p - 1) As Double, max_old(p - 1) As Double, range_old(p - 1) As Double, range_fract(p - 1) As Double
            ReDim ScaledData(n - 1, p - 1), x_corr_labels(p * p - 1), y_corr_labels(p * p - 1), x_var_labels(p - 1)
            Dim intercepts_scaled(p - 1, p - 1) As Double, slopes_scaled(p - 1, p - 1) As Double
            ReDim x_regline((p * (p - 1) * 3) - 1), y_regline((p * (p - 1) * 3) - 1)

            Dim min_new As Double = (1 / p) * pMargin
            Dim max_new As Double = (1 / p) * (1 - pMargin)
            Dim range_new As Double = max_new - min_new

            For j = 1 To p  ' loop over all variables/columns
                tmp = Matrix.GetColumnFrom2Darray(pData, j - 1)
                min_old(j - 1) = tmp.Min()
                max_old(j - 1) = tmp.Max()
                range_old(j - 1) = max_old(j - 1) - min_old(j - 1)
                range_fract(j - 1) = range_new / range_old(j - 1)
                x_var_labels(j - 1) = (j - 1) * (1 / p) + (1 / p) / 2
            Next

            For i = 0 To n - 1
                For j = 1 To p
                    ScaledData(i, j - 1) = ((j - 1.0) * (1.0 / p)) + (pData(i, j - 1) - min_old(j - 1)) * range_fract(j - 1) + min_new
                Next
            Next

            If pbDisplayCorrCoef Then CorrMatrix = Matrix.CorrelMatrix(pData, "r") 'compute pearson correlation coefficient to display on chart

            'compute regression lines to display on the chart
            If pbDisplayRegLines Then
                For i = 0 To p - 1
                    tmp = Matrix.GetColumnFrom2Darray(ScaledData, i)
                    For j = 0 To p - 1
                        tmp2 = Matrix.GetColumnFrom2Darray(ScaledData, j)
                        intercepts_scaled(i, j) = Intercept(tmp2, tmp)
                        slopes_scaled(i, j) = Slope(tmp2, tmp)
                    Next
                Next
            End If

            Dim ii As Integer = 0
            Dim iii As Integer = 0
            For i = 1 To p
                For j = 1 To p
                    'coordinates for correlation coefficient labels displayed on chart
                    x_corr_labels(ii) = (i - 1) * (1 / p)
                    y_corr_labels(ii) = (j - 1) * (1 / p) + ((1 / p) * (pMargin / 2))
                    ii += 1

                    If pbDisplayRegLines Then
                        'coordinates for regression lines
                        If i <> j Then
                            For jj = 1 To 3
                                If jj = 1 Then
                                    x_regline(iii) = (i - 1) * (1 / p) + ((1 / p) * pMargin)
                                    y_regline(iii) = slopes_scaled(i - 1, j - 1) * x_regline(iii) + intercepts_scaled(i - 1, j - 1)
                                ElseIf jj = 2 Then
                                    x_regline(iii) = i * (1 / p) - ((1 / p) * pMargin)
                                    y_regline(iii) = slopes_scaled(i - 1, j - 1) * x_regline(iii) + intercepts_scaled(i - 1, j - 1)
                                Else 'indication to set line segment to invisible
                                    x_regline(iii) = gToDeleteGridLineValue
                                    y_regline(iii) = gToDeleteGridLineValue
                                End If

                                If i = j Then
                                    x_regline(iii) = gToDeleteGridLineValue
                                    y_regline(iii) = gToDeleteGridLineValue
                                End If
                                iii += 1
                            Next jj
                        End If
                    End If
                Next
            Next i
        End Sub
End Class
Public Class LegacyXyz30
        Friend n As Integer, x_raw() As Double, y_raw() As Double, z_raw() As Double
        Friend raw_mins_() As Double, raw_ranges_() As Double, axis_dir_() As Double
        Friend x_norm() As Double, y_norm() As Double, z_norm() As Double
        Friend Sub getnorm()
            ReDim x_norm(n - 1), y_norm(n - 1), z_norm(n - 1)
            For i = 0 To n - 1
                x_norm(i) = ((x_raw(i) - raw_mins_(0)) / raw_ranges_(0) - 0.5) * axis_dir_(0)
                y_norm(i) = ((y_raw(i) - raw_mins_(1)) / raw_ranges_(1) - 0.5) * axis_dir_(1)
                z_norm(i) = ((z_raw(i) - raw_mins_(2)) / raw_ranges_(2) - 0.5) * axis_dir_(2)
            Next
        End Sub
End Class
End Namespace
