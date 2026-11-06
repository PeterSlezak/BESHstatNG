Option Explicit On
Option Strict On

Imports System
Imports System.Linq

''' <summary>Source-faithful ROC numerical data. No chart objects or input aliases.
''' ComputeCurve retains the explicit accumulator seed solely for the Windows
''' repeated-compute compatibility path; ordinary callers use its zero default.
''' No new finite filtering, alpha policy, terminal cutoff or CI definition.</summary>
Public NotInheritable Class RocDataCore

    Private Sub New()
    End Sub

    Private Shared Function CopyGroups(groups As Double()()) As Double()()
        If groups Is Nothing Then Return Nothing
        Dim copy(groups.Length - 1)() As Double
        For i As Integer = 0 To groups.Length - 1
            copy(i) = If(groups(i) Is Nothing, Nothing, DirectCast(groups(i).Clone(), Double()))
        Next

        Return copy
    End Function

    Public Shared Function Compute(groups As Double()(), Optional alpha As Double = 0.05) As RocDataResult
        Dim state As New Work(CopyGroups(groups), 0)
        state.compute(alpha)
        state.DeLongSE(alpha)
        Return state.Snapshot()
    End Function

    Public Shared Function ComputeCurve(groups As Double()(), Optional alpha As Double = 0.05, Optional legacyAucSeed As Double = 0) As RocDataResult
        Dim state As New Work(CopyGroups(groups), legacyAucSeed)
        state.compute(alpha)
        Return state.Snapshot()
    End Function

    Public Shared Function ComputeDeLong(groups As Double()(), Optional alpha As Double = 0.05) As RocDataResult
        Dim state As New Work(CopyGroups(groups), 0)
        state.DeLongSE(alpha)
        Return state.Snapshot()
    End Function

    Public Shared Function MidRanks(values As Double()) As Double()
        Return Work.MidRanks(values)
    End Function

    Private NotInheritable Class Work

        Private data()() As Double

        Private pAUC As Double, pseAUC As Double, pPvalue As Double

        Private parCutOff() As Double, parSensitivity() As Double, parSpecificity() As Double, par1minusSpec() As Double

        Private pCI As ConfidenceIntervalResult, pdelongCI As ConfidenceIntervalResult

        Private pdelongSE As Double

        Public Sub New(groups As Double()(), seed As Double)
            data = groups : pAUC = seed
        End Sub

        Public Function Snapshot() As RocDataResult
            Return New RocDataResult(pAUC, pseAUC, pPvalue, pCI, pdelongSE, pdelongCI, parCutOff, parSensitivity, parSpecificity, par1minusSpec)
        End Function

        Public Sub compute(Optional alpha As Double = 0.05)
            Dim Data12() As Double, arIDs() As Integer
            Dim arPatientsGroupNo() As Double, arControlsGroupNo() As Double, arPatientsCum() As Double 'Wilcoxon AUC calculation
            Dim arContCum() As Double, dQ1SE As Double, dQ2SE As Double 'Wilcoxon AUC and seAUC calculation
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
                parCutOff(i) =(arUnique(i) + arUnique(i + 1)) / 2.0
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
                parSensitivity(i + 1) =(a / (a + c))
                parSpecificity(i) =(d / (b + d))
                par1minusSpec(i + 1) = 1.0 - parSpecificity(i)
                If i = 0 Then
                    arPatientsCum(i) -= arPatientsGroupNo(i)
                Else '1st value is always zero
                    arPatientsCum(i) = arPatientsCum(i - 1) - arPatientsGroupNo(i)
                    arContCum(i) = arContCum(i - 1) + arControlsGroupNo(i - 1)
                End If

                pAUC +=(arControlsGroupNo(i) * arPatientsCum(i) + 0.5 * arControlsGroupNo(i) * arPatientsGroupNo(i))
                dQ2SE +=(arPatientsGroupNo(i) * (arContCum(i) ^ 2 + arContCum(i) * arControlsGroupNo(i) + 1 / 3 * arControlsGroupNo(i) ^ 2))
                dQ1SE +=(arControlsGroupNo(i) * (arPatientsCum(i) ^ 2 + arPatientsCum(i) * arPatientsGroupNo(i) + 1 / 3 * arPatientsGroupNo(i) ^ 2))
            Next

            parSensitivity(0) = 1.0 'for ploting ROC
            par1minusSpec(0) = 1.0 'for ploting ROC
            parSensitivity(NoUniqueVals) = 0.0 'for ploting ROC
            par1minusSpec(NoUniqueVals) = 0.0 'for ploting ROC
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
        End Sub

        Public Function DeLongSE(Optional alpha As Double = 0.05) As Double
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
                v(i) =(rxCombined(i) - rxWithin(i)) / n
            Next

            Dim w(n - 1) As Double
            For j = 0 To n - 1
                w(j) =(ryCombined(j) - ryWithin(j)) / m
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

            sV /=(m - 1)
            Dim sW As Double = 0
            For j = 0 To n - 1
                Dim d As Double = w(j) - meanW
                sW += d * d
            Next

            sW /=(n - 1)
            Dim varAuc As Double =(sV / m) + (sW / n)
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

        Public Shared Function MidRanks(values As Double()) As Double()
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
                Dim avg As Double =(rankLo + rankHi) / 2.0
                For k = iPos To jPos
                    ranks(idx(k)) = avg
                Next

                iPos = jPos + 1
            End While

            Return ranks
        End Function
    End Class
End Class

''' <summary>Immutable value snapshot; every vector and mutable legacy CI is copied on access.</summary>
Public NotInheritable Class RocDataResult

    Private ReadOnly ci As ConfidenceIntervalResult, dci As ConfidenceIntervalResult

    Private ReadOnly cut() As Double, sens() As Double, spec() As Double, fpr() As Double

    Friend Sub New(auc As Double, se As Double, p As Double, interval As ConfidenceIntervalResult, deLong As Double, deLongInterval As ConfidenceIntervalResult, cutoffs() As Double, sensitivity() As Double, specificity() As Double, falsePositive() As Double)
        Me.Auc = auc : Me.StandardError = se : Me.PValue = p : Me.DeLongStandardError = deLong
        ci = CopyInterval(interval) : dci = CopyInterval(deLongInterval)
        cut = CopyVector(cutoffs) : sens = CopyVector(sensitivity) : spec = CopyVector(specificity) : fpr = CopyVector(falsePositive)
    End Sub

    Private Shared Function CopyVector(x() As Double) As Double()
        Return If(x Is Nothing, Nothing, DirectCast(x.Clone(), Double()))
    End Function

    Private Shared Function CopyInterval(x As ConfidenceIntervalResult) As ConfidenceIntervalResult
        If x Is Nothing Then Return Nothing
        Return New ConfidenceIntervalResult With {.Estimate = x.Estimate, .LowerLimit = x.LowerLimit, .UpperLimit = x.UpperLimit, .alpha = x.alpha, .StdErr = x.StdErr}
    End Function

    Public ReadOnly Property Auc As Double

    Public ReadOnly Property StandardError As Double

    Public ReadOnly Property PValue As Double

    Public ReadOnly Property DeLongStandardError As Double

    Public ReadOnly Property Interval As ConfidenceIntervalResult
        Get
            Return CopyInterval(ci)
        End Get
    End Property

    Public ReadOnly Property DeLongInterval As ConfidenceIntervalResult
        Get
            Return CopyInterval(dci)
        End Get
    End Property

    Public ReadOnly Property Cutoffs As Double()
        Get
            Return CopyVector(cut)
        End Get
    End Property

    Public ReadOnly Property Sensitivity As Double()
        Get
            Return CopyVector(sens)
        End Get
    End Property

    Public ReadOnly Property Specificity As Double()
        Get
            Return CopyVector(spec)
        End Get
    End Property

    Public ReadOnly Property FalsePositiveRate As Double()
        Get
            Return CopyVector(fpr)
        End Get
    End Property
End Class

