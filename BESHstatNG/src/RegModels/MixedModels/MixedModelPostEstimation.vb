Option Explicit On
Option Strict On

Namespace regression

    ''' <summary>
    ''' Reusable post-estimation helpers for mixed models.
    ''' </summary>
    ''' <remarks>
    ''' This module intentionally contains no UI dependencies.  It is designed to be
    ''' reused by Ui18MMRM worksheet output, future MMRM/LMM UDF extractors, and
    ''' unit tests for LS-means and contrasts.
    ''' </remarks>
    Public Module MixedModelPostEstimation

        Public Function UniqueSortedFiniteValues(values() As Double) As Double()
            If values Is Nothing Then Return Array.Empty(Of Double)()

            Dim list As New List(Of Double)

            For Each v As Double In values
                If Not AppInfrastructure.IsFinite(v) Then Continue For

                Dim exists As Boolean = False
                For Each old As Double In list
                    If NearlyEqual(old, v) Then
                        exists = True
                        Exit For
                    End If
                Next

                If Not exists Then list.Add(v)
            Next

            list.Sort()
            Return list.ToArray()
        End Function


        Public Function NearlyEqual(a As Double, b As Double) As Boolean
            If Not AppInfrastructure.IsFinite(a) OrElse Not AppInfrastructure.IsFinite(b) Then Return False

            Dim tol As Double = 0.000000001 * Math.Max(1.0, Math.Max(Math.Abs(a), Math.Abs(b)))
            Return Math.Abs(a - b) <= tol
        End Function


        Public Function FormatProfileValue(v As Double) As String
            If Not AppInfrastructure.IsFinite(v) Then Return String.Empty
            If Math.Abs(v - Math.Round(v)) < 0.000000001 Then Return CStr(CLng(Math.Round(v)))
            Return v.ToString("G6", System.Globalization.CultureInfo.InvariantCulture)
        End Function


        Public Function AverageDesignRowForProfile(x(,) As Double,
                                                   visit() As Double,
                                                   groupValues() As Double,
                                                   targetVisit As Double,
                                                   targetGroup As Double,
                                                   rowMask() As Boolean) As Double()
            If x Is Nothing OrElse visit Is Nothing Then Return Nothing

            Dim n As Integer = x.GetLength(0)
            Dim p As Integer = x.GetLength(1)
            If visit.Length <> n Then Return Nothing
            If groupValues IsNot Nothing AndAlso groupValues.Length <> n Then Return Nothing

            Dim out(p - 1) As Double
            Dim count As Integer = 0

            For i As Integer = 0 To n - 1
                If rowMask IsNot Nothing AndAlso (i >= rowMask.Length OrElse Not rowMask(i)) Then Continue For
                If Not NearlyEqual(visit(i), targetVisit) Then Continue For
                If groupValues IsNot Nothing AndAlso Not NearlyEqual(groupValues(i), targetGroup) Then Continue For

                For j As Integer = 0 To p - 1
                    out(j) += x(i, j)
                Next

                count += 1
            Next

            If count <= 0 Then Return Nothing

            For j As Integer = 0 To p - 1
                out(j) /= CDbl(count)
            Next

            Return out
        End Function


        Public Function CountProfileRows(visit() As Double,
                                         groupValues() As Double,
                                         targetVisit As Double,
                                         targetGroup As Double,
                                         rowMask() As Boolean) As Integer
            If visit Is Nothing Then Return 0
            If groupValues IsNot Nothing AndAlso groupValues.Length <> visit.Length Then Return 0

            Dim count As Integer = 0

            For i As Integer = 0 To visit.Length - 1
                If rowMask IsNot Nothing AndAlso (i >= rowMask.Length OrElse Not rowMask(i)) Then Continue For
                If Not NearlyEqual(visit(i), targetVisit) Then Continue For
                If groupValues IsNot Nothing AndAlso Not NearlyEqual(groupValues(i), targetGroup) Then Continue For
                count += 1
            Next

            Return count
        End Function


        Public Function MakeDirectedDifference(lTreatment() As Double,
                                               lControl() As Double,
                                               direction As String,
                                               treatmentMinusControlText As String,
                                               controlMinusTreatmentText As String) As Double()
            If String.Equals(direction, controlMinusTreatmentText, StringComparison.OrdinalIgnoreCase) Then
                Return Matrix.M_SUB(lControl, lTreatment)
            End If

            Return Matrix.M_SUB(lTreatment, lControl)
        End Function


        Public Function DirectedComparisonLabel(groupBaseName As String,
                                                treatmentLevel As Double,
                                                controlLevel As Double,
                                                direction As String,
                                                treatmentMinusControlText As String,
                                                controlMinusTreatmentText As String) As String
            If String.Equals(direction, controlMinusTreatmentText, StringComparison.OrdinalIgnoreCase) Then
                Return groupBaseName & "=" & FormatProfileValue(controlLevel) & " - " &
                       groupBaseName & "=" & FormatProfileValue(treatmentLevel)
            End If

            Return groupBaseName & "=" & FormatProfileValue(treatmentLevel) & " - " &
                   groupBaseName & "=" & FormatProfileValue(controlLevel)
        End Function


        Public Function LinearEstimate(l() As Double, beta() As Double) As Double
            If l Is Nothing OrElse beta Is Nothing OrElse l.Length <> beta.Length Then Return Double.NaN

            Dim s As Double = 0.0
            For i As Integer = 0 To l.Length - 1
                s += l(i) * beta(i)
            Next

            Return s
        End Function


        Public Function LinearCombinationVariance(l() As Double, varBeta(,) As Double) As Double
            If l Is Nothing OrElse varBeta Is Nothing Then Return Double.NaN

            Dim p As Integer = l.Length
            If varBeta.GetLength(0) <> p OrElse varBeta.GetLength(1) <> p Then Return Double.NaN

            Dim v As Double = 0.0
            For i As Integer = 0 To p - 1
                For j As Integer = 0 To p - 1
                    v += l(i) * varBeta(i, j) * l(j)
                Next
            Next

            Return v
        End Function

        Public Function ResolveLinearEstimateDF(result As MixedModelResult, Optional linearRow() As Double = Nothing) As Double
            If result Is Nothing Then Return Double.NaN
            If result.FixedInferenceMethod = MixedModelFixedInferenceMethod.WaldNormal Then Return Double.NaN

            If result.FixedInferenceMethod = MixedModelFixedInferenceMethod.Satterthwaite AndAlso
                  linearRow IsNot Nothing AndAlso result.InferenceWorkspace IsNot Nothing Then

                Dim dfSat As Double = Double.NaN

                If MixedModelInferenceMath.TrySatterthwaiteDF(linearRow, result.InferenceWorkspace, dfSat) Then Return dfSat
            End If

            If result.FixedInferenceMethod = MixedModelFixedInferenceMethod.KenwardRoger AndAlso linearRow IsNot Nothing Then

                Dim krInf As MixedModelKenwardRogerUnivariateInference = Nothing
                Dim krMsg As String = Nothing

                If MixedModelKenwardRogerInference.TryUnivariateInference(result,
                                                                  "L*beta",
                                                                  linearRow,
                                                                  krInf,
                                                                  alpha:=0.05,
                                                                  diagnostic:=krMsg) Then
                    If krInf IsNot Nothing AndAlso AppInfrastructure.IsFinite(krInf.DF) AndAlso krInf.DF > 0.0 Then
                        Return krInf.DF
                    End If
                End If
            End If

            If result.BetaDF Is Nothing OrElse result.BetaDF.Length = 0 Then Return Double.NaN

            Dim minDf As Double = Double.PositiveInfinity

            For Each df As Double In result.BetaDF
                If AppInfrastructure.IsFinite(df) AndAlso df > 0.0 Then
                    If df < minDf Then minDf = df
                End If
            Next

            If Double.IsInfinity(minDf) Then Return Double.NaN
            Return minDf
        End Function

        ''' <summary>
        ''' Computes inference for a single linear estimate or contrast row.
        ''' </summary>
        ''' <remarks>
        ''' This is the common path for observed-grid LS-means, reference-grid LS-means,
        ''' contrasts, future UDFs, and tests.  When fixed inference is Kenward-Roger and
        ''' KR adjusted covariance is available, it uses
        ''' <see cref="MixedModelKenwardRogerInference.TryUnivariateInference"/>.
        ''' Otherwise it falls back to the ordinary model covariance and the selected
        ''' denominator-DF method.
        ''' </remarks>
        Public Function TryLinearInference(result As MixedModelResult,
                                   label As String,
                                   linearRow() As Double,
                                   alpha As Double,
                                   ByRef estimate As Double,
                                   ByRef standardError As Double,
                                   ByRef df As Double,
                                   ByRef statistic As Double,
                                   ByRef pValue As Double,
                                   ByRef lowerCI As Double,
                                   ByRef upperCI As Double,
                                   Optional ByRef diagnostic As String = Nothing) As Boolean
            estimate = Double.NaN
            standardError = Double.NaN
            df = Double.NaN
            statistic = Double.NaN
            pValue = Double.NaN
            lowerCI = Double.NaN
            upperCI = Double.NaN
            diagnostic = String.Empty

            If result Is Nothing OrElse linearRow Is Nothing OrElse result.Beta Is Nothing Then
                diagnostic = "Result, linear row, or beta is missing."
                Return False
            End If

            If linearRow.Length <> result.Beta.Length Then
                diagnostic = "Linear row length does not match beta length."
                Return False
            End If

            If result.FixedInferenceMethod = MixedModelFixedInferenceMethod.KenwardRoger Then
                Dim krInf As MixedModelKenwardRogerUnivariateInference = Nothing
                Dim krMsg As String = Nothing

                If MixedModelKenwardRogerInference.TryUnivariateInference(result, label, linearRow, krInf, alpha, krMsg) Then
                    estimate = krInf.Estimate
                    standardError = krInf.AdjustedStdError
                    df = krInf.DF
                    statistic = krInf.Statistic
                    pValue = krInf.PValue
                    lowerCI = krInf.LowerCI
                    upperCI = krInf.UpperCI
                    diagnostic = krInf.DiagnosticMessage
                    Return True
                End If

                diagnostic = "Kenward-Roger linear inference unavailable; ordinary covariance fallback used. " & krMsg
            End If

            Dim alphaUse As Double = AppInfrastructure.NormalizeAlpha(alpha)

            estimate = LinearEstimate(linearRow, result.Beta)

            Dim varEst As Double = LinearCombinationVariance(linearRow, result.VarBeta)
            standardError = If(varEst >= 0.0 AndAlso AppInfrastructure.IsFinite(varEst), Math.Sqrt(varEst), Double.NaN)

            df = ResolveLinearEstimateDF(result, linearRow)
            ComputeLinearInference(estimate, standardError, df, alphaUse, statistic, pValue, lowerCI, upperCI)

            Return AppInfrastructure.IsFinite(estimate) AndAlso AppInfrastructure.IsFinite(standardError)
        End Function

        Public Function BuildLinearEstimateResultTable(title As String,
                                               rowLabels() As String,
                                               lRows As List(Of Double()),
                                               counts As List(Of Integer),
                                               result As MixedModelResult,
                                               alpha As Double,
                                               footnote As String) As Global.BESHStatNG.ResultTable
            If rowLabels Is Nothing OrElse lRows Is Nothing OrElse result Is Nothing Then Return Nothing
            If rowLabels.Length <> lRows.Count Then Return Nothing

            Dim alphaUse As Double = AppInfrastructure.NormalizeAlpha(alpha)
            Dim n As Integer = lRows.Count
            Dim body(n - 1, 7) As Object

            Dim statLabel As String = If(String.IsNullOrWhiteSpace(result.BetaStatisticLabel), "z", result.BetaStatisticLabel)
            Dim pLabel As String = If(String.IsNullOrWhiteSpace(result.BetaPValueLabel), "Pr(>|z|)", result.BetaPValueLabel)
            Dim levelText As String = Format((1.0 - alphaUse) * 100.0, "0.###") & "% CI"

            For i As Integer = 0 To n - 1
                Dim est As Double = Double.NaN
                Dim se As Double = Double.NaN
                Dim df As Double = Double.NaN
                Dim stat As Double = Double.NaN
                Dim pv As Double = Double.NaN
                Dim lo As Double = Double.NaN
                Dim hi As Double = Double.NaN
                Dim diag As String = Nothing

                TryLinearInference(result,
                           If(rowLabels IsNot Nothing AndAlso i < rowLabels.Length, rowLabels(i), "L" & (i + 1).ToString()),
                           lRows(i),
                           alphaUse,
                           est,
                           se,
                           df,
                           stat,
                           pv,
                           lo,
                           hi,
                           diag)

                body(i, 0) = If(counts IsNot Nothing AndAlso i < counts.Count, counts(i), Nothing)
                body(i, 1) = est
                body(i, 2) = se
                body(i, 3) = If(AppInfrastructure.IsFinite(df) AndAlso df > 0.0, CType(df, Object), String.Empty)
                body(i, 4) = stat
                body(i, 5) = pv
                body(i, 6) = lo
                body(i, 7) = hi
            Next

            Dim t As New Global.BESHStatNG.ResultTable
            t.AddTitle(title)
            t.SetBody(body)
            t.AddHeaderTopRow({"N", "Estimate", "Std. Error", "DF", statLabel, pLabel, "Lower " & levelText, "Upper " & levelText})
            t.AddHeaderLeftRow(rowLabels)
            t.AddPvalueToFormat(6)

            If Not String.IsNullOrWhiteSpace(footnote) Then t.AddFootnote(footnote)
            AddLinearEstimateFootnote(t, result, isContrast:=False)

            Return t
        End Function

        Public Function BuildLinearContrastResultTable(title As String,
                                               rowLabels() As String,
                                               lRows As List(Of Double()),
                                               result As MixedModelResult,
                                               alpha As Double,
                                               footnote As String) As Global.BESHStatNG.ResultTable
            If rowLabels Is Nothing OrElse lRows Is Nothing OrElse result Is Nothing Then Return Nothing
            If rowLabels.Length <> lRows.Count Then Return Nothing

            Dim alphaUse As Double = AppInfrastructure.NormalizeAlpha(alpha)
            Dim n As Integer = lRows.Count
            Dim body(n - 1, 6) As Object

            Dim statLabel As String = If(String.IsNullOrWhiteSpace(result.BetaStatisticLabel), "z", result.BetaStatisticLabel)
            Dim pLabel As String = If(String.IsNullOrWhiteSpace(result.BetaPValueLabel), "Pr(>|z|)", result.BetaPValueLabel)
            Dim levelText As String = Format((1.0 - alphaUse) * 100.0, "0.###") & "% CI"

            For i As Integer = 0 To n - 1
                Dim est As Double = Double.NaN
                Dim se As Double = Double.NaN
                Dim df As Double = Double.NaN
                Dim stat As Double = Double.NaN
                Dim pv As Double = Double.NaN
                Dim lo As Double = Double.NaN
                Dim hi As Double = Double.NaN
                Dim diag As String = Nothing

                TryLinearInference(result,
                           If(rowLabels IsNot Nothing AndAlso i < rowLabels.Length, rowLabels(i), "L" & (i + 1).ToString()),
                           lRows(i),
                           alphaUse,
                           est,
                           se,
                           df,
                           stat,
                           pv,
                           lo,
                           hi,
                           diag)

                body(i, 0) = est
                body(i, 1) = se
                body(i, 2) = If(AppInfrastructure.IsFinite(df) AndAlso df > 0.0, CType(df, Object), String.Empty)
                body(i, 3) = stat
                body(i, 4) = pv
                body(i, 5) = lo
                body(i, 6) = hi
            Next

            Dim t As New Global.BESHStatNG.ResultTable
            t.AddTitle(title)
            t.SetBody(body)
            t.AddHeaderTopRow({"Estimate", "Std. Error", "DF", statLabel, pLabel, "Lower " & levelText, "Upper " & levelText})
            t.AddHeaderLeftRow(rowLabels)
            t.AddPvalueToFormat(5)

            If Not String.IsNullOrWhiteSpace(footnote) Then t.AddFootnote(footnote)
            AddLinearEstimateFootnote(t, result, isContrast:=True)

            Return t
        End Function

        Private Sub ComputeLinearInference(est As Double,
                                           se As Double,
                                           df As Double,
                                           alpha As Double,
                                           ByRef stat As Double,
                                           ByRef pv As Double,
                                           ByRef lo As Double,
                                           ByRef hi As Double)
            stat = Double.NaN
            pv = Double.NaN
            lo = Double.NaN
            hi = Double.NaN

            If se > 0.0 AndAlso AppInfrastructure.IsFinite(se) AndAlso AppInfrastructure.IsFinite(est) Then
                stat = est / se

                If AppInfrastructure.IsFinite(df) AndAlso df > 0.0 Then
                    pv = Global.BESHStatNG.distributions.Distributions.T_2T(Math.Abs(stat), df)
                    Dim crit As Double = Global.BESHStatNG.distributions.Distributions.T_Inv_2T(alpha, df)
                    lo = est - crit * se
                    hi = est + crit * se
                Else
                    pv = 2.0 * (1.0 - Global.BESHStatNG.distributions.Distributions.PNorm(Math.Abs(stat)))
                    Dim crit As Double = Global.BESHStatNG.distributions.Distributions.NormSInv(1.0 - alpha / 2.0)
                    lo = est - crit * se
                    hi = est + crit * se
                End If
            End If

            If AppInfrastructure.IsFinite(pv) Then
                If pv < 0.0 Then pv = 0.0
                If pv > 1.0 Then pv = 1.0
            End If
        End Sub


        Private Sub AddLinearEstimateFootnote(t As Global.BESHStatNG.ResultTable,
                                              result As MixedModelResult,
                                              isContrast As Boolean)
            If t Is Nothing OrElse result Is Nothing Then Exit Sub

            Select Case result.FixedInferenceMethod
                Case MixedModelFixedInferenceMethod.WaldNormal
                    t.AddFootnote("Inference uses the large-sample normal approximation for these " &
                                  If(isContrast, "linear contrasts.", "linear estimates."))

                Case MixedModelFixedInferenceMethod.Satterthwaite
                    t.AddFootnote("DF for these " & If(isContrast, "contrasts", "LS-means/linear estimates") &
                                  " uses a row-specific first-order Satterthwaite approximation for L*beta.")

                Case MixedModelFixedInferenceMethod.KenwardRoger
                    t.AddFootnote("Inference for these " & If(isContrast, "contrasts", "LS-means/linear estimates") &
                                  " uses KR-adjusted standard errors and R mmrm-style one-dimensional Kenward-Roger denominator DF.")

                Case Else
                    t.AddFootnote("DF for these " & If(isContrast, "linear contrasts", "linear estimates") &
                                  " uses the fitted model's selected fixed-effect denominator-DF method.")
            End Select
        End Sub

    End Module

End Namespace
