Option Explicit On
Option Strict On

Imports System
Imports System.Collections.Generic

Namespace regression

    ''' <summary>
    ''' One univariate Kenward-Roger validation inference row for L*beta.
    ''' </summary>
    ''' <remarks>
    ''' This class is intentionally separated from final user-facing fixed-effect
    ''' inference.  The current implementation provides KR-adjusted standard errors
    ''' and a Kenward-Roger moment-matched denominator DF on the current KR
    ''' parameter scale.
    ''' </remarks>
    Public Class MixedModelKenwardRogerUnivariateInference

        Public Property Label As String = String.Empty
        Public Property Estimate As Double = Double.NaN
        Public Property OrdinaryStdError As Double = Double.NaN
        Public Property AdjustedStdError As Double = Double.NaN
        Public Property OrdinaryVariance As Double = Double.NaN
        Public Property AdjustedVariance As Double = Double.NaN
        Public Property DF As Double = Double.NaN
        Public Property NumDF As Double = 1.0
        Public Property Lambda As Double = 1.0
        Public Property Statistic As Double = Double.NaN
        Public Property PValue As Double = Double.NaN
        Public Property LowerCI As Double = Double.NaN
        Public Property UpperCI As Double = Double.NaN
        Public Property Scaling As Double = 1.0
        Public Property StatisticLabel As String = "t"
        Public Property PValueLabel As String = "Pr(>|t|)"
        Public Property DiagnosticMessage As String = String.Empty

    End Class

    ''' <summary>
    ''' Represents one multi-row linear hypothesis H0: L * beta = 0.
    ''' </summary>
    Public Class MixedModelMultiDfHypothesis

        Public Property Label As String = String.Empty
        Public Property L As Double(,) = Nothing

        Public Sub New()
        End Sub

        Public Sub New(label As String,
                   lMatrix As Double(,))
            Me.Label = If(label, String.Empty)
            Me.L = lMatrix
        End Sub

        Public Function Validate(expectedP As Integer) As Boolean
            Return L IsNot Nothing AndAlso
               L.GetLength(0) > 0 AndAlso
               L.GetLength(1) = expectedP
        End Function

    End Class


    ''' <summary>
    ''' Internal/validation multi-df Kenward-Roger F-test result.
    ''' </summary>
    Public Class MixedModelKenwardRogerMultiDfInference

        Public Property Label As String = String.Empty
        Public Property L As Double(,) = Nothing
        Public Property EstimateVector As Double() = Nothing
        Public Property CovarianceMatrix As Double(,) = Nothing
        Public Property NumDF As Double = Double.NaN
        Public Property DenDF As Double = Double.NaN
        Public Property FStatistic As Double = Double.NaN
        Public Property UnscaledFStatistic As Double = Double.NaN
        Public Property PValue As Double = Double.NaN
        Public Property Scaling As Double = 1.0
        Public Property A1 As Double = Double.NaN
        Public Property A2 As Double = Double.NaN
        Public Property B As Double = Double.NaN
        Public Property EStar As Double = Double.NaN
        Public Property VStar As Double = Double.NaN
        Public Property Rho As Double = Double.NaN
        Public Property RequestedL As Double(,) = Nothing
        Public Property EffectiveL As Double(,) = Nothing
        Public Property RequestedNumDF As Double = Double.NaN
        Public Property Rank As Integer = 0
        Public Property RankReduced As Boolean = False
        Public Property DiagnosticMessage As String = String.Empty

    End Class

    Public Class MixedModelKenwardRogerDfResult
        Public Property NumDF As Integer = 0
        Public Property DenDF As Double = Double.NaN
        Public Property Lambda As Double = Double.NaN
        Public Property A1 As Double = Double.NaN
        Public Property A2 As Double = Double.NaN
        Public Property B As Double = Double.NaN
        Public Property EStar As Double = Double.NaN
        Public Property VStar As Double = Double.NaN
        Public Property Rho As Double = Double.NaN
        Public Property DiagnosticMessage As String = String.Empty

    End Class

    ''' <summary>
    ''' Backend validation helpers for univariate Kenward-Roger inference.
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' This module is the next step after validating KR adjusted Var(beta).  It
    ''' computes univariate <c>L * beta</c> inference using:
    ''' </para>
    ''' <list type="bullet">
    '''   <item><description>the KR-adjusted coefficient covariance matrix for SEs,</description></item>
    '''   <item><description>a KR moment-matched denominator DF computed on the same KR parameter scale,</description></item>
    '''   <item><description>scaling fixed at 1 for univariate t tests.</description></item>
    ''' </list>
    ''' <para>
    ''' The DF formula follows the Kenward-Roger moment-matching calculation used
    ''' for one- and multi-df contrasts, using <c>Phi</c>, <c>P_h</c>, and the
    ''' currently available <c>Cov(theta)</c>.  It should be treated as a
    ''' validation/backend step until checked against <c>lmerTest</c>,
    ''' <c>pbkrtest</c>, SAS, or <c>mmrm</c> for the target model classes.
    ''' </para>
    ''' </remarks>
    Public Module MixedModelKenwardRogerInference

        ''' <summary>
        ''' Resolves the current KR adjusted Var(beta) from the unified inference
        ''' workspace, falling back to the result-level compatibility property.
        ''' </summary>
        Public Function ResolveAdjustedVarBeta(modelResult As MixedModelResult) As Double(,)
            If modelResult Is Nothing Then Return Nothing

            If modelResult.InferenceWorkspace IsNot Nothing AndAlso modelResult.InferenceWorkspace.AdjustedVarBeta IsNot Nothing Then
                Return modelResult.InferenceWorkspace.AdjustedVarBeta
            End If

            Return modelResult.KenwardRogerAdjustedVarBeta
        End Function

        ''' <summary>
        ''' Computes one univariate KR validation inference row for L*beta.
        ''' </summary>
        Public Function TryUnivariateInference(modelResult As MixedModelResult,
                                               label As String,
                                               l() As Double,
                                               ByRef inference As MixedModelKenwardRogerUnivariateInference,
                                               Optional alpha As Double = 0.05,
                                               Optional ByRef diagnostic As String = Nothing) As Boolean
            inference = Nothing
            diagnostic = String.Empty

            If modelResult Is Nothing Then
                diagnostic = "Model result is Nothing."
                Return False
            End If

            If l Is Nothing OrElse modelResult.Beta Is Nothing OrElse l.Length <> modelResult.Beta.Length Then
                diagnostic = "Linear row L is missing or not conformable with beta."
                Return False
            End If

            Dim adjusted(,) As Double = MixedModelKenwardRogerInference.ResolveAdjustedVarBeta(modelResult)
            If adjusted Is Nothing Then
                diagnostic = "KR adjusted Var(beta) is unavailable."
                Return False
            End If

            If adjusted.GetLength(0) <> l.Length OrElse adjusted.GetLength(1) <> l.Length Then
                diagnostic = "KR adjusted Var(beta) dimensions do not match L."
                Return False
            End If

            Dim est As Double = MixedModelInferenceMath.LinearEstimate(l, modelResult.Beta)
            Dim ordinaryVar As Double = MixedModelInferenceMath.LinearVariance(l, modelResult.VarBeta)
            Dim adjustedVar As Double = MixedModelInferenceMath.LinearVariance(l, adjusted)

            If Not AppInfrastructure.IsFinite(adjustedVar) OrElse adjustedVar <= 0.0 Then
                diagnostic = "KR adjusted variance for L*beta is not positive and finite."
                Return False
            End If

            Dim seAdj As Double = Math.Sqrt(adjustedVar)
            Dim seOrd As Double = If(AppInfrastructure.IsFinite(ordinaryVar) AndAlso ordinaryVar >= 0.0,
                                     Math.Sqrt(ordinaryVar),
                                     Double.NaN)

            Dim lMatrix(0, l.Length - 1) As Double
            For j As Integer = 0 To l.Length - 1
                lMatrix(0, j) = l(j)
            Next

            Dim dfInfo As MixedModelKenwardRogerDfResult = Nothing
            Dim dfDiagnostic As String = String.Empty
            Dim hasDf As Boolean = TryComputeKrDegreesOfFreedomAndScaling(modelResult, lMatrix, dfInfo, dfDiagnostic)

            Dim df As Double = Double.NaN
            Dim lambda As Double = 1.0
            If hasDf AndAlso dfInfo IsNot Nothing Then
                df = dfInfo.DenDF
                lambda = dfInfo.Lambda
            End If

            Dim alphaUse As Double = AppInfrastructure.NormalizeAlpha(alpha)
            Dim stat As Double = est / seAdj
            Dim pv As Double = Double.NaN
            Dim lo As Double = Double.NaN
            Dim hi As Double = Double.NaN

            If hasDf Then
                pv = Global.BESHStatNG.distributions.Distributions.T_2T(Math.Abs(stat), df)
                Dim crit As Double = Global.BESHStatNG.distributions.Distributions.T_Inv_2T(alphaUse, df)
                lo = est - crit * seAdj
                hi = est + crit * seAdj
            End If

            inference = New MixedModelKenwardRogerUnivariateInference With {
                .Label = If(label, String.Empty),
                .Estimate = est,
                .OrdinaryStdError = seOrd,
                .AdjustedStdError = seAdj,
                .OrdinaryVariance = ordinaryVar,
                .AdjustedVariance = adjustedVar,
                .DF = df,
                .NumDF = 1.0,
                .Lambda = lambda,
                .Statistic = stat,
                .PValue = pv,
                .LowerCI = lo,
                .UpperCI = hi,
                .Scaling = 1.0,
                .StatisticLabel = "t",
                .PValueLabel = "Pr(>|t|)",
                .DiagnosticMessage = If(hasDf,
                                        "Univariate KR inference computed with KR-adjusted SE and R mmrm-style denominator DF. F-scaling lambda=" & lambda.ToString("G6", System.Globalization.CultureInfo.InvariantCulture) & " is diagnostic only for one-row t tests.",
                                        "KR adjusted SE computed, but denominator DF was unavailable. " & dfDiagnostic)
            }

            diagnostic = inference.DiagnosticMessage
            Return True
        End Function

        ''' <summary>
        ''' Computes a denominator DF for a univariate KR adjusted variance using
        ''' the same Kenward-Roger moment-matching path as the multi-df F test.
        ''' </summary>
        Public Function TryComputeUnivariateDenominatorDF(modelResult As MixedModelResult,
                                                          l() As Double,
                                                          adjustedVariance As Double,
                                                          ByRef df As Double,
                                                          Optional ByRef diagnostic As String = Nothing) As Boolean
            df = Double.NaN
            diagnostic = String.Empty

            If l Is Nothing Then
                diagnostic = "Linear row L is missing."
                Return False
            End If

            If Not AppInfrastructure.IsFinite(adjustedVariance) OrElse adjustedVariance <= 0.0 Then
                diagnostic = "Adjusted variance is not positive and finite."
                Return False
            End If

            Dim lMatrix(0, l.Length - 1) As Double
            For j As Integer = 0 To l.Length - 1
                lMatrix(0, j) = l(j)
            Next

            Dim dfInfo As MixedModelKenwardRogerDfResult = Nothing
            If Not TryComputeKrDegreesOfFreedomAndScaling(modelResult, lMatrix, dfInfo, diagnostic) Then
                Return False
            End If

            df = dfInfo.DenDF
            diagnostic = "Univariate KR denominator DF computed by the R mmrm-style h_kr_df moment-matching path."
            Return True
        End Function


        ''' <summary>
        ''' Builds a validation table for KR univariate inference rows.
        ''' </summary>
        Public Function BuildUnivariateInferenceTable(modelResult As MixedModelResult,
                                                      hypotheses As IEnumerable(Of MixedModelLinearHypothesis),
                                                      Optional alpha As Double = 0.05,
                                                      Optional title As String = "Kenward-Roger univariate inference, validation") As Global.BESHStatNG.ResultTable
            If modelResult Is Nothing OrElse hypotheses Is Nothing Then Return Nothing

            Dim rowLabels As New List(Of String)()
            Dim values As New List(Of MixedModelKenwardRogerUnivariateInference)()

            For Each h As MixedModelLinearHypothesis In hypotheses
                If h Is Nothing OrElse h.L Is Nothing Then Continue For

                Dim one As MixedModelKenwardRogerUnivariateInference = Nothing
                Dim msg As String = Nothing

                If TryUnivariateInference(modelResult, h.Label, h.L, one, alpha, msg) Then
                    rowLabels.Add(If(String.IsNullOrWhiteSpace(h.Label), "L" & (rowLabels.Count + 1).ToString(), h.Label))
                    values.Add(one)
                End If
            Next

            If values.Count = 0 Then Return Nothing

            Dim body(values.Count - 1, 8) As Object

            For i As Integer = 0 To values.Count - 1
                Dim one As MixedModelKenwardRogerUnivariateInference = values(i)

                body(i, 0) = one.Estimate
                body(i, 1) = one.OrdinaryStdError
                body(i, 2) = one.AdjustedStdError
                body(i, 3) = If(AppInfrastructure.IsFinite(one.DF) AndAlso one.DF > 0.0, CType(one.DF, Object), String.Empty)
                body(i, 4) = one.Statistic
                body(i, 5) = one.PValue
                body(i, 6) = one.LowerCI
                body(i, 7) = one.UpperCI
                body(i, 8) = one.DiagnosticMessage
            Next

            Dim levelText As String = Format((1.0 - AppInfrastructure.NormalizeAlpha(alpha)) * 100.0, "0.###") & "% CI"

            Dim t As New Global.BESHStatNG.ResultTable
            t.AddTitle(title)
            t.SetBody(body)
            t.AddHeaderTopRow({"Estimate", "Ordinary SE", "KR adjusted SE", "DF", "t", "Pr(>|t|)",
                               "Lower " & levelText, "Upper " & levelText, "Diagnostic"})
            t.AddHeaderLeftRow(rowLabels.ToArray())
            t.AddPvalueToFormat(6)
            t.AddFootnote("Uses KR-adjusted SE and R mmrm-style one-dimensional denominator DF.")
            t.AddFootnote("KR parameter scale: " & If(modelResult.KenwardRogerWorkspace Is Nothing,
                                                      "unavailable",
                                                      modelResult.KenwardRogerWorkspace.ParameterScale.ToString()) & ".")
            Return t
        End Function

        ''' <summary>
        ''' Computes an internal/validation multi-df Kenward-Roger F test for
        ''' H0: L * beta = 0 using the same denominator-DF and F-scaling path as
        ''' R mmrm h_kr_df().
        ''' </summary>
        ''' <remarks>
        ''' This is the reusable KR F-test entry point for term-level tests and
        ''' future GUI/UDF integration.  It rank-reduces redundant restriction rows
        ''' before inverting L * Var(beta) * L' and before computing KR denominator
        ''' DF/scaling.  For full-rank restriction matrices, the supplied L matrix is
        ''' used unchanged.
        ''' </remarks>
        Public Function TryComputeKrFTest(modelResult As MixedModelResult,
                                          label As String,
                                          lMatrix(,) As Double,
                                          ByRef inference As MixedModelKenwardRogerMultiDfInference,
                                          Optional alpha As Double = 0.05,
                                          Optional ByRef diagnostic As String = Nothing) As Boolean
            inference = Nothing
            diagnostic = String.Empty

            If modelResult Is Nothing Then
                diagnostic = "Model result is Nothing."
                Return False
            End If

            If modelResult.Beta Is Nothing Then
                diagnostic = "Beta vector is unavailable."
                Return False
            End If

            If lMatrix Is Nothing Then
                diagnostic = "L matrix is Nothing."
                Return False
            End If

            Dim requestedQ As Integer = lMatrix.GetLength(0)
            Dim p As Integer = lMatrix.GetLength(1)

            If requestedQ <= 0 OrElse p <> modelResult.Beta.Length Then
                diagnostic = "L matrix is not conformable with beta."
                Return False
            End If

            Dim effectiveL(,) As Double = Nothing
            Dim rankDiagnostic As String = String.Empty
            If Not TryBuildFullRowRankRestriction(lMatrix, effectiveL, rankDiagnostic) Then
                diagnostic = "Could not build a full-row-rank KR restriction matrix: " & rankDiagnostic
                Return False
            End If

            Dim q As Integer = effectiveL.GetLength(0)
            Dim rankReduced As Boolean = (q <> requestedQ)

            Dim adjusted(,) As Double = MixedModelKenwardRogerInference.ResolveAdjustedVarBeta(modelResult)
            If adjusted Is Nothing Then
                diagnostic = "KR adjusted Var(beta) is unavailable."
                Return False
            End If

            If adjusted.GetLength(0) <> p OrElse adjusted.GetLength(1) <> p Then
                diagnostic = "KR adjusted Var(beta) dimensions do not match L."
                Return False
            End If

            Dim est() As Double = MultiplyMatrixVector(effectiveL, modelResult.Beta)
            Dim covL(,) As Double = ComputeLCovLt(effectiveL, adjusted)

            Dim covInv(,) As Double = Nothing
            If Not TryInvertPositiveDefinite(covL, covInv, diagnostic) Then
                diagnostic = "Could not invert L * KRAdjustedVarBeta * L' after rank reduction: " & diagnostic
                Return False
            End If

            Dim qform As Double = MixedModelInferenceMath.QuadraticForm(est, covInv)
            If Not AppInfrastructure.IsFinite(qform) OrElse qform < 0.0 Then
                diagnostic = "Wald quadratic form is not nonnegative and finite."
                Return False
            End If

            Dim unscaledF As Double = qform / CDbl(q)
            Dim scaling As Double = 1.0
            Dim denDf As Double = Double.NaN
            Dim dfDiagnostic As String = String.Empty
            Dim dfInfo As MixedModelKenwardRogerDfResult = Nothing
            Dim hasDf As Boolean = TryComputeKrDegreesOfFreedomAndScaling(modelResult, effectiveL, dfInfo, dfDiagnostic)

            If hasDf AndAlso dfInfo IsNot Nothing Then
                scaling = dfInfo.Lambda
                denDf = dfInfo.DenDF
            Else
                scaling = 1.0
                hasDf = TryApproximateMultiDfDenominatorDF(modelResult, effectiveL, covL, denDf, dfDiagnostic)
                If hasDf Then
                    dfDiagnostic = "R mmrm-style KR F scaling was unavailable; used legacy harmonic-mean denominator DF with scaling=1. " & dfDiagnostic
                End If
            End If

            Dim fStat As Double = scaling * unscaledF
            Dim pVal As Double = Double.NaN
            If hasDf Then
                pVal = Global.BESHStatNG.distributions.Distributions.F_RT(fStat, CDbl(q), denDf)
                If pVal < 0.0 Then pVal = 0.0
                If pVal > 1.0 Then pVal = 1.0
            End If

            Dim rankMessage As String = If(rankReduced,
                                           " Restriction matrix was rank-reduced from " & requestedQ.ToString() & " requested rows to effective rank " & q.ToString() & ". " & rankDiagnostic,
                                           String.Empty)

            inference = New MixedModelKenwardRogerMultiDfInference With {
                .Label = If(label, String.Empty),
                .L = DirectCast(lMatrix.Clone(), Double(,)),
                .RequestedL = DirectCast(lMatrix.Clone(), Double(,)),
                .EffectiveL = DirectCast(effectiveL.Clone(), Double(,)),
                .EstimateVector = est,
                .CovarianceMatrix = covL,
                .RequestedNumDF = CDbl(requestedQ),
                .NumDF = CDbl(q),
                .Rank = q,
                .RankReduced = rankReduced,
                .DenDF = denDf,
                .FStatistic = fStat,
                .UnscaledFStatistic = unscaledF,
                .PValue = pVal,
                .Scaling = scaling,
                .A1 = If(dfInfo Is Nothing, Double.NaN, dfInfo.A1),
                .A2 = If(dfInfo Is Nothing, Double.NaN, dfInfo.A2),
                .B = If(dfInfo Is Nothing, Double.NaN, dfInfo.B),
                .EStar = If(dfInfo Is Nothing, Double.NaN, dfInfo.EStar),
                .VStar = If(dfInfo Is Nothing, Double.NaN, dfInfo.VStar),
                .Rho = If(dfInfo Is Nothing, Double.NaN, dfInfo.Rho),
                .DiagnosticMessage = If(hasDf,
                                        "Multi-df KR F test computed with R mmrm-style F scaling and denominator DF." & rankMessage & " " & dfDiagnostic,
                                        "Multi-df KR adjusted F statistic computed, but denominator DF was unavailable." & rankMessage & " " & dfDiagnostic)
            }

            diagnostic = inference.DiagnosticMessage
            Return True
        End Function

        ''' <summary>
        ''' Builds an internal/validation table for multi-df KR F tests.
        ''' </summary>
        Public Function BuildMultiDfInferenceTable(modelResult As MixedModelResult,
                                           hypotheses As IEnumerable(Of MixedModelMultiDfHypothesis),
                                           Optional alpha As Double = 0.05,
                                           Optional title As String = "Kenward-Roger multi-df F tests") As Global.BESHStatNG.ResultTable
            If modelResult Is Nothing OrElse hypotheses Is Nothing Then Return Nothing

            Dim rowLabels As New List(Of String)()
            Dim values As New List(Of MixedModelKenwardRogerMultiDfInference)()

            For Each h As MixedModelMultiDfHypothesis In hypotheses
                If h Is Nothing OrElse h.L Is Nothing Then Continue For

                Dim one As MixedModelKenwardRogerMultiDfInference = Nothing
                Dim msg As String = Nothing

                If TryComputeKrFTest(modelResult, h.Label, h.L, one, alpha, msg) Then
                    rowLabels.Add(If(String.IsNullOrWhiteSpace(h.Label), "H" & (rowLabels.Count + 1).ToString(), h.Label))
                    values.Add(one)
                End If
            Next

            If values.Count = 0 Then Return Nothing

            Dim body(values.Count - 1, 8) As Object

            For i As Integer = 0 To values.Count - 1
                Dim one As MixedModelKenwardRogerMultiDfInference = values(i)

                body(i, 0) = one.NumDF
                body(i, 1) = If(AppInfrastructure.IsFinite(one.DenDF) AndAlso one.DenDF > 0.0, CType(one.DenDF, Object), String.Empty)
                body(i, 2) = one.FStatistic
                body(i, 3) = one.PValue
                body(i, 4) = one.UnscaledFStatistic
                body(i, 5) = one.Scaling
                body(i, 6) = one.RequestedNumDF
                body(i, 7) = If(one.RankReduced, "Yes", "No")
                body(i, 8) = one.DiagnosticMessage
            Next

            Dim t As New Global.BESHStatNG.ResultTable
            t.AddTitle(title)
            t.SetBody(body)
            t.AddHeaderTopRow({"Num DF", "Den DF", "F", "Pr(>F)", "Unscaled F", "F scaling", "Requested Num DF", "Rank reduced", "Diagnostic"})
            t.AddHeaderLeftRow(rowLabels.ToArray())
            t.AddPvalueToFormat(4)
            t.AddFootnote("Uses KR-adjusted Var(beta) with R mmrm-style F scaling lambda and denominator DF when available.")
            t.AddFootnote("KR parameter scale: " & If(modelResult.KenwardRogerWorkspace Is Nothing,
                                              "unavailable",
                                              modelResult.KenwardRogerWorkspace.ParameterScale.ToString()) & ".")
            Return t
        End Function


        ''' <summary>
        ''' Computes Kenward-Roger F scaling and denominator DF by moment matching for
        ''' a one- or multi-row contrast matrix.
        ''' </summary>
        Public Function TryComputeKrFScalingAndDenominatorDF(modelResult As MixedModelResult,
                                                             lMatrix(,) As Double,
                                                             ByRef scaling As Double,
                                                             ByRef df As Double,
                                                             Optional ByRef diagnostic As String = Nothing) As Boolean
            scaling = Double.NaN
            df = Double.NaN
            diagnostic = String.Empty
            Dim info As MixedModelKenwardRogerDfResult = Nothing
            If Not TryComputeKrDegreesOfFreedomAndScaling(modelResult, lMatrix, info, diagnostic) Then
                Return False
            End If

            scaling = info.Lambda
            df = info.DenDF
            diagnostic = info.DiagnosticMessage
            Return True
        End Function

        Public Function TryComputeKrDegreesOfFreedomAndScaling(modelResult As MixedModelResult,
                                                               lMatrix(,) As Double,
                                                               ByRef info As MixedModelKenwardRogerDfResult,
                                                               Optional ByRef diagnostic As String = Nothing) As Boolean
            info = Nothing
            diagnostic = String.Empty

            If modelResult Is Nothing OrElse modelResult.KenwardRogerWorkspace Is Nothing Then
                diagnostic = "KR workspace is unavailable."
                Return False
            End If

            Dim ws As MixedModelKrWorkspace = modelResult.KenwardRogerWorkspace

            If lMatrix Is Nothing Then
                diagnostic = "L matrix is Nothing."
                Return False
            End If

            Dim q As Integer = lMatrix.GetLength(0)
            Dim p As Integer = lMatrix.GetLength(1)

            If q <= 0 OrElse p <> ws.P Then
                diagnostic = "L matrix is not conformable with KR workspace P."
                Return False
            End If

            Dim effectiveL(,) As Double = Nothing
            Dim rankDiagnostic As String = String.Empty
            If Not TryBuildFullRowRankRestriction(lMatrix, effectiveL, rankDiagnostic) Then
                diagnostic = "Could not build a full-row-rank KR restriction matrix: " & rankDiagnostic
                Return False
            End If

            lMatrix = effectiveL
            q = lMatrix.GetLength(0)

            If ws.VarBeta Is Nothing OrElse ws.Pmats Is Nothing OrElse ws.ThetaCovariance Is Nothing Then
                diagnostic = "Need VarBeta, Pmats, and ThetaCovariance to compute KR denominator DF and scaling."
                Return False
            End If

            If ws.VarBeta.GetLength(0) <> ws.P OrElse ws.VarBeta.GetLength(1) <> ws.P Then
                diagnostic = "KR VarBeta dimension mismatch."
                Return False
            End If

            If ws.Pmats.GetLength(0) <> ws.K OrElse ws.Pmats.GetLength(1) <> ws.P OrElse ws.Pmats.GetLength(2) <> ws.P Then
                diagnostic = "KR Pmats dimension mismatch."
                Return False
            End If

            If ws.ThetaCovariance.GetLength(0) <> ws.K OrElse ws.ThetaCovariance.GetLength(1) <> ws.K Then
                diagnostic = "KR ThetaCovariance dimension mismatch."
                Return False
            End If

            If ws.DfScalingCache Is Nothing Then ws.DfScalingCache = New Dictionary(Of String, MixedModelKenwardRogerDfResult)()
            Dim cacheKey As String = MixedModelNumericalDiagnostics.BuildMatrixSignature(lMatrix, digits:=12)
            If ws.DfScalingCache.ContainsKey(cacheKey) Then
                info = CloneDfResult(ws.DfScalingCache(cacheKey))
                diagnostic = info.DiagnosticMessage & " Reused cached KR DF/scaling trace products."
                info.DiagnosticMessage = diagnostic
                Return True
            End If

            Dim phi(,) As Double = ws.VarBeta
            Dim cov0(,) As Double = ComputeLCovLt(lMatrix, phi)
            Dim cov0Inv(,) As Double = Nothing
            Dim invDiagnostic As String = String.Empty

            If Not TryInvertPositiveDefinite(cov0, cov0Inv, invDiagnostic) Then
                diagnostic = "Could not invert L * VarBeta * L' for KR denominator DF/scaling: " & invDiagnostic
                Return False
            End If

            Dim mMat(,) As Double = BuildRestrictionProjection(lMatrix, cov0Inv)

            Dim traces(ws.K - 1) As Double
            Dim mPhiPphi(ws.K - 1, p - 1, p - 1) As Double

            For h As Integer = 0 To ws.K - 1
                Dim ph(,) As Double = Slice3D(ws.Pmats, h)
                Dim one(,) As Double = Matrix.MatrixMult(Matrix.MatrixMult(Matrix.MatrixMult(mMat, phi), ph), phi)
                traces(h) = Matrix.MatrixTrace(one)

                For r As Integer = 0 To p - 1
                    For c As Integer = 0 To p - 1
                        mPhiPphi(h, r, c) = one(r, c)
                    Next
                Next
            Next

            Dim a1 As Double = 0.0
            Dim a2 As Double = 0.0

            For h As Integer = 0 To ws.K - 1
                For j As Integer = 0 To ws.K - 1
                    Dim whj As Double = ws.ThetaCovariance(h, j)
                    If whj = 0.0 Then Continue For

                    a1 += whj * traces(h) * traces(j)

                    Dim mh(,) As Double = Slice3D(mPhiPphi, h)
                    Dim mj(,) As Double = Slice3D(mPhiPphi, j)
                    a2 += whj * Matrix.MatrixTrace(Matrix.MatrixMult(mh, mj))
                Next
            Next

            If Not AppInfrastructure.IsFinite(a1) OrElse Not AppInfrastructure.IsFinite(a2) Then
                diagnostic = "KR moment components A1/A2 are not finite."
                Return False
            End If

            Dim qD As Double = CDbl(q)
            Const eps As Double = 0.000000000001

            If Math.Abs(a2) <= eps Then
                info = New MixedModelKenwardRogerDfResult With {
                     .NumDF = q,
                     .DenDF = 1000000.0,
                     .Lambda = 1.0,
                     .A1 = a1,
                     .A2 = a2,
                     .B = (a1 + 6.0 * a2) / (2.0 * qD),
                     .EStar = 1.0,
                     .VStar = 2.0 / qD,
                     .Rho = 1.0,
                     .DiagnosticMessage = "KR A2 is approximately zero; using lambda=1 and large denominator DF."
                }
                If ws.DfScalingCache IsNot Nothing Then ws.DfScalingCache(cacheKey) = CloneDfResult(info)
                diagnostic = info.DiagnosticMessage
                Return True
            End If

            Dim b As Double = (a1 + 6.0 * a2) / (2.0 * qD)
            Dim eStar As Double = 1.0 / (1.0 - (a2 / qD))
            Dim g As Double = (((qD + 1.0) * a1) - ((qD + 4.0) * a2)) / ((qD + 2.0) * a2)
            Dim denom As Double = 3.0 * qD + 2.0 - 2.0 * g

            If Math.Abs(denom) <= eps Then
                diagnostic = "KR moment denominator for c1/c2/c3 is approximately zero."
                Return False
            End If

            Dim c1 As Double = g / denom
            Dim c2 As Double = (qD - g) / denom
            Dim c3 As Double = (qD + 2.0 - g) / denom

            If Math.Abs(1.0 - (a2 / qD)) <= eps OrElse Math.Abs(1.0 - c2 * b) <= eps OrElse Math.Abs(1.0 - c3 * b) <= eps Then
                diagnostic = "KR moment denominator is approximately zero."
                Return False
            End If

            Dim vStar As Double = (2.0 / qD) * ((1.0 + c1 * b) / ((1.0 - c2 * b) * (1.0 - c2 * b) * (1.0 - c3 * b)))
            Dim rho As Double = vStar / (2.0 * eStar * eStar)

            If Not AppInfrastructure.IsFinite(eStar) OrElse eStar <= 0.0 OrElse
               Not AppInfrastructure.IsFinite(vStar) OrElse vStar <= 0.0 OrElse
               Not AppInfrastructure.IsFinite(rho) Then
                diagnostic = "KR moment-matched E*, V*, or rho is invalid."
                Return False
            End If

            Dim dfDenom As Double = qD * rho - 1.0
            If Math.Abs(dfDenom) <= eps Then
                diagnostic = "KR denominator-DF moment denominator is approximately zero."
                Return False
            End If

            Dim df As Double = 4.0 + (qD + 2.0) / dfDenom

            If Not AppInfrastructure.IsFinite(df) OrElse df <= 2.0 Then
                diagnostic = "Computed KR denominator DF is not greater than 2 and finite."
                Return False
            End If

            Dim lambda As Double = df / (eStar * (df - 2.0))

            If Not AppInfrastructure.IsFinite(lambda) OrElse lambda <= 0.0 Then
                diagnostic = "Computed KR F scaling lambda is not positive and finite."
                Return False
            End If

            df = Math.Max(1.0, Math.Min(1000000.0, df))
            info = New MixedModelKenwardRogerDfResult With {
                .NumDF = q,
                .DenDF = df,
                .Lambda = lambda,
                .A1 = a1,
                .A2 = a2,
                .B = b,
                .EStar = eStar,
                .VStar = vStar,
                .Rho = rho,
                .DiagnosticMessage = "KR denominator DF and F scaling lambda computed by R mmrm-style h_kr_df moment matching on " & ws.ParameterScale.ToString() & " parameter scale."
            }
            If ws.DfScalingCache IsNot Nothing Then ws.DfScalingCache(cacheKey) = CloneDfResult(info)
            diagnostic = info.DiagnosticMessage

            Return True
        End Function


        ''' <summary>
        ''' Approximates denominator DF for a multi-row hypothesis by the harmonic mean
        ''' of the row-wise univariate KR DFs.
        ''' </summary>
        Public Function TryApproximateMultiDfDenominatorDF(modelResult As MixedModelResult,
                                                   lMatrix(,) As Double,
                                                   adjustedCovarianceForL(,) As Double,
                                                   ByRef df As Double,
                                                   Optional ByRef diagnostic As String = Nothing) As Boolean
            df = Double.NaN
            diagnostic = String.Empty

            If modelResult Is Nothing OrElse lMatrix Is Nothing OrElse adjustedCovarianceForL Is Nothing Then
                diagnostic = "Model result, L matrix, or L covariance is missing."
                Return False
            End If

            Dim q As Integer = lMatrix.GetLength(0)
            Dim p As Integer = lMatrix.GetLength(1)

            If q <= 0 OrElse adjustedCovarianceForL.GetLength(0) <> q OrElse adjustedCovarianceForL.GetLength(1) <> q Then
                diagnostic = "L covariance dimensions do not match L rows."
                Return False
            End If

            Dim sumInvDf As Double = 0.0
            Dim used As Integer = 0

            For r As Integer = 0 To q - 1
                Dim lRow() As Double = Matrix.rowFromArray(lMatrix, r)
                Dim v As Double = adjustedCovarianceForL(r, r)

                Dim oneDf As Double = Double.NaN
                Dim oneMsg As String = Nothing

                If TryComputeUnivariateDenominatorDF(modelResult, lRow, v, oneDf, oneMsg) Then
                    If AppInfrastructure.IsFinite(oneDf) AndAlso oneDf > 0.0 Then
                        sumInvDf += 1.0 / oneDf
                        used += 1
                    End If
                End If
            Next

            If used = 0 OrElse sumInvDf <= 0.0 Then
                diagnostic = "No row-wise univariate KR denominator DFs were available."
                Return False
            End If

            df = CDbl(used) / sumInvDf

            If Not AppInfrastructure.IsFinite(df) OrElse df <= 0.0 Then
                diagnostic = "Computed harmonic-mean denominator DF is not positive and finite."
                Return False
            End If

            df = Math.Max(1.0, Math.Min(1000000.0, df))
            diagnostic = "Multi-df denominator DF approximated by harmonic mean of " & used.ToString() & " row-wise univariate KR DFs."
            Return True
        End Function

        Private Function TryBuildFullRowRankRestriction(lMatrix(,) As Double,
                                                        ByRef effectiveL(,) As Double,
                                                        ByRef diagnostic As String) As Boolean
            effectiveL = Nothing
            diagnostic = String.Empty

            If lMatrix Is Nothing Then
                diagnostic = "L matrix is Nothing."
                Return False
            End If

            Dim q As Integer = lMatrix.GetLength(0)
            Dim p As Integer = lMatrix.GetLength(1)

            If q <= 0 OrElse p <= 0 Then
                diagnostic = "L matrix must have at least one row and one column."
                Return False
            End If

            Dim maxRowNorm As Double = 0.0
            For r As Integer = 0 To q - 1
                Dim nrm As Double = 0.0
                For c As Integer = 0 To p - 1
                    nrm += lMatrix(r, c) * lMatrix(r, c)
                Next
                maxRowNorm = Math.Max(maxRowNorm, Math.Sqrt(nrm))
            Next

            If maxRowNorm <= 0.0 OrElse Not AppInfrastructure.IsFinite(maxRowNorm) Then
                diagnostic = "All rows in L are numerically zero."
                Return False
            End If

            Dim tol As Double = Math.Max(0.000000000001, 0.0000000001 * CDbl(Math.Max(q, p)) * maxRowNorm)
            Dim basis As New List(Of Double())()

            For r As Integer = 0 To q - 1
                Dim work(p - 1) As Double
                For c As Integer = 0 To p - 1
                    work(c) = lMatrix(r, c)
                Next

                For Each basisRow As Double() In basis
                    Dim projection As Double = Matrix.DotProduct(work, basisRow)
                    For c As Integer = 0 To p - 1
                        work(c) -= projection * basisRow(c)
                    Next
                Next

                Dim norm As Double = Matrix.VectorNorm(work)
                If norm > tol AndAlso AppInfrastructure.IsFinite(norm) Then
                    For c As Integer = 0 To p - 1
                        work(c) /= norm
                    Next
                    basis.Add(work)
                End If
            Next

            If basis.Count = 0 Then
                diagnostic = "L has numerical row rank zero."
                Return False
            End If

            If basis.Count = q Then
                effectiveL = DirectCast(lMatrix.Clone(), Double(,))
                diagnostic = "Restriction matrix is full row rank."
                Return True
            End If

            Dim reduced(basis.Count - 1, p - 1) As Double
            For r As Integer = 0 To basis.Count - 1
                For c As Integer = 0 To p - 1
                    reduced(r, c) = basis(r)(c)
                Next
            Next

            effectiveL = reduced
            diagnostic = "Restriction matrix had " & q.ToString() & " requested rows and numerical row rank " & basis.Count.ToString() & "."
            Return True
        End Function

        Private Function MultiplyMatrixVector(a(,) As Double, x() As Double) As Double()
            Dim n As Integer = a.GetLength(0)
            Dim p As Integer = a.GetLength(1)
            Dim out(n - 1) As Double

            For i As Integer = 0 To n - 1
                Dim s As Double = 0.0

                For j As Integer = 0 To p - 1
                    s += a(i, j) * x(j)
                Next

                out(i) = s
            Next

            Return out
        End Function


        Private Function BuildRestrictionProjection(lMatrix(,) As Double, lPhiLtInv(,) As Double) As Double(,)
            Dim p As Integer = lMatrix.GetLength(1)
            Dim q As Integer = lMatrix.GetLength(0)
            Dim out(p - 1, p - 1) As Double

            For r As Integer = 0 To p - 1
                For c As Integer = 0 To p - 1
                    Dim s As Double = 0.0

                    For a As Integer = 0 To q - 1
                        For b As Integer = 0 To q - 1
                            s += lMatrix(a, r) * lPhiLtInv(a, b) * lMatrix(b, c)
                        Next
                    Next

                    out(r, c) = s
                Next
            Next

            MixedModelEngine.SymmetrizeInPlace(out)
            Return out
        End Function





        Private Function ComputeLCovLt(lMatrix(,) As Double, covariance(,) As Double) As Double(,)
            Dim q As Integer = lMatrix.GetLength(0)
            Dim p As Integer = lMatrix.GetLength(1)
            Dim temp(q - 1, p - 1) As Double

            For r As Integer = 0 To q - 1
                For c As Integer = 0 To p - 1
                    Dim s As Double = 0.0

                    For h As Integer = 0 To p - 1
                        s += lMatrix(r, h) * covariance(h, c)
                    Next

                    temp(r, c) = s
                Next
            Next

            Dim out(q - 1, q - 1) As Double

            For r As Integer = 0 To q - 1
                For c As Integer = 0 To q - 1
                    Dim s As Double = 0.0

                    For h As Integer = 0 To p - 1
                        s += temp(r, h) * lMatrix(c, h)
                    Next

                    out(r, c) = s
                Next
            Next

            MixedModelEngine.SymmetrizeInPlace(out)
            Return out
        End Function

        Private Function TryInvertPositiveDefinite(a(,) As Double, ByRef inv(,) As Double, ByRef diagnostic As String) As Boolean
            inv = Nothing
            diagnostic = String.Empty

            Dim detail As New MixedModelNumericalInverseResult()
            If Not MixedModelNumericalDiagnostics.TryInvertSymmetric(a,
                                                                     inv,
                                                                     diagnostic,
                                                                     allowPseudoInverse:=True,
                                                                     inverseResult:=detail) Then
                Return False
            End If

            Dim warn As String = MixedModelNumericalDiagnostics.WarningForConditionNumber("KR hypothesis covariance", detail.ConditionNumber)
            If Not String.IsNullOrWhiteSpace(warn) Then
                diagnostic &= " " & warn
            End If

            If detail.UsedPseudoInverse Then
                diagnostic &= " Used SVD pseudoinverse fallback."
            End If
            Return inv IsNot Nothing
        End Function

        Private Function CloneDfResult(source As MixedModelKenwardRogerDfResult) As MixedModelKenwardRogerDfResult
            If source Is Nothing Then Return Nothing
            Return New MixedModelKenwardRogerDfResult With {
                    .NumDF = source.NumDF,
                    .DenDF = source.DenDF,
                    .Lambda = source.Lambda,
                    .A1 = source.A1,
                    .A2 = source.A2,
                    .B = source.B,
                    .EStar = source.EStar,
                    .VStar = source.VStar,
                    .Rho = source.Rho,
                    .DiagnosticMessage = source.DiagnosticMessage
                }
        End Function

    End Module

End Namespace
