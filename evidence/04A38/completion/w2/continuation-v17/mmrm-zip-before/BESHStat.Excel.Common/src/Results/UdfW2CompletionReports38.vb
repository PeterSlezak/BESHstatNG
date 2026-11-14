Option Explicit On
Option Strict On
Option Infer On
Imports System
Imports System.Collections.Generic
Imports System.Globalization
Imports System.Linq
Namespace ExcelCommon
    Public NotInheritable Class UdfW2CompletionReports38
        Private Sub New()
        End Sub

        Public Shared Function BuildMultinomialResidualOutput(res As Global.BESHStatNG.regression.MultinomialResiduals, categories() As Integer, includeHeader As Boolean, unavailable As Object) As Object
            If res Is Nothing Then Return unavailable
            
            Dim cats() As Integer = categories
            Dim n As Integer = res.FittedMeans.GetLength(0)
            Dim k As Integer = cats.Length
            
            Dim headers As New List(Of String)()
            headers.AddRange(UdfRegressionOutputReports38.CategoryHeaders("Observed", cats))
            headers.AddRange(UdfRegressionOutputReports38.CategoryHeaders("FittedMean", cats))
            headers.AddRange(UdfRegressionOutputReports38.CategoryHeaders("Prob", cats))
            headers.AddRange(UdfRegressionOutputReports38.CategoryHeaders("Response", cats))
            headers.AddRange(UdfRegressionOutputReports38.CategoryHeaders("Pearson", cats))
            headers.AddRange(UdfRegressionOutputReports38.CategoryHeaders("StdPearson", cats))
            headers.Add("DevianceResidual")
            headers.Add("StdDevianceResidual")
            headers.Add("Leverage")
            
            Dim outRows As Integer = If(includeHeader, n + 1, n)
            Dim outCols As Integer = headers.Count
            Dim out(outRows - 1, outCols - 1) As Object
            Dim r0 As Integer = 0
            
            If includeHeader Then
                For j As Integer = 0 To outCols - 1
                    out(0, j) = headers(j)
                Next
                r0 = 1
            End If
            
            For i As Integer = 0 To n - 1
                Dim c As Integer = 0
            
                For j As Integer = 0 To k - 1
                    out(r0 + i, c) = res.Observed(i, j) : c += 1
                Next
                For j As Integer = 0 To k - 1
                    out(r0 + i, c) = res.FittedMeans(i, j) : c += 1
                Next
                For j As Integer = 0 To k - 1
                    out(r0 + i, c) = res.Probabilities(i, j) : c += 1
                Next
                For j As Integer = 0 To k - 1
                    out(r0 + i, c) = res.ResponseResiduals(i, j) : c += 1
                Next
                For j As Integer = 0 To k - 1
                    out(r0 + i, c) = res.PearsonResiduals(i, j) : c += 1
                Next
                For j As Integer = 0 To k - 1
                    out(r0 + i, c) = res.StdPearsonResiduals(i, j) : c += 1
                Next
            
                out(r0 + i, c) = res.DevianceResiduals(i) : c += 1
                out(r0 + i, c) = res.StdDevianceResiduals(i) : c += 1
                out(r0 + i, c) = res.Leverage(i)
            Next
            
            Return out
        End Function

        Public Shared Function BuildOrdinalResidualOutput(res As Global.BESHStatNG.regression.MultinomialResiduals, includeHeader As Boolean, unavailable As Object) As Object
            If res Is Nothing Then Return unavailable
            
            Dim cats() As Integer = res.Categories
            Dim n As Integer = res.FittedMeans.GetLength(0)
            Dim k As Integer = cats.Length
            
            Dim headers As New List(Of String)()
            headers.AddRange(UdfRegressionOutputReports38.CategoryHeaders("FittedMean", cats))
            headers.AddRange(UdfRegressionOutputReports38.CategoryHeaders("Prob", cats))
            headers.AddRange(UdfRegressionOutputReports38.CategoryHeaders("Response", cats))
            headers.AddRange(UdfRegressionOutputReports38.CategoryHeaders("Pearson", cats))
            headers.AddRange(UdfRegressionOutputReports38.CategoryHeaders("StdPearson", cats))
            headers.Add("DevianceResidual")
            headers.Add("StdDevianceResidual")
            headers.Add("Leverage")
            
            Dim outRows As Integer = If(includeHeader, n + 1, n)
            Dim outCols As Integer = headers.Count
            Dim out(outRows - 1, outCols - 1) As Object
            Dim r0 As Integer = 0
            
            If includeHeader Then
                For j As Integer = 0 To outCols - 1
                    out(0, j) = headers(j)
                Next
                r0 = 1
            End If
            
            For i As Integer = 0 To n - 1
                Dim c As Integer = 0
            
                For j As Integer = 0 To k - 1
                    out(r0 + i, c) = res.FittedMeans(i, j) : c += 1
                Next
                For j As Integer = 0 To k - 1
                    out(r0 + i, c) = res.Probabilities(i, j) : c += 1
                Next
                For j As Integer = 0 To k - 1
                    out(r0 + i, c) = res.ResponseResiduals(i, j) : c += 1
                Next
                For j As Integer = 0 To k - 1
                    out(r0 + i, c) = res.PearsonResiduals(i, j) : c += 1
                Next
                For j As Integer = 0 To k - 1
                    out(r0 + i, c) = res.StdPearsonResiduals(i, j) : c += 1
                Next
            
                out(r0 + i, c) = res.DevianceResiduals(i) : c += 1
                out(r0 + i, c) = res.StdDevianceResiduals(i) : c += 1
                out(r0 + i, c) = res.Leverage(i)
            Next
            
            Return out
        End Function

        Public Shared Sub AppendZipSummaryRows(rows As List(Of Object()), componentLabel As String, predictorNames() As String, coeffs() As Double, ses() As Double, interceptIncluded As Boolean, ciAlpha As Double)
            If rows Is Nothing OrElse coeffs Is Nothing OrElse ses Is Nothing Then Exit Sub
            If coeffs.Length <> ses.Length Then Exit Sub
            
            Dim zCrit As Double = distributions.ZCritTwoSided(ciAlpha)
            For i As Integer = 0 To coeffs.Length - 1
                Dim z As Double = If(ses(i) > 0.0R, coeffs(i) / ses(i), Double.NaN)
                Dim pv As Double = If(ses(i) > 0.0R, 2.0R * distributions.PNorm(-Math.Abs(z)), Double.NaN)
                rows.Add(New Object() {
                    componentLabel,
                    predictorNames(i),
                    If(interceptIncluded AndAlso i = 0, "Intercept", "Slope"),
                    coeffs(i),
                    ses(i),
                    z,
                    pv,
                    coeffs(i) - zCrit * ses(i),
                    coeffs(i) + zCrit * ses(i)
                })
            Next
        End Sub

        Public Shared Function MaterializeZipRows(rows As List(Of Object()), includeHeader As Boolean, headers() As String) As Object
            Dim bodyCols As Integer = If(headers Is Nothing, If(rows.Count > 0, rows(0).Length, 1), headers.Length)
            Dim outRows As Integer = rows.Count + If(includeHeader, 1, 0)
            If outRows < 1 Then outRows = 1
            Dim out(outRows - 1, bodyCols - 1) As Object
            Dim r0 As Integer = 0
            
            If includeHeader AndAlso headers IsNot Nothing Then
                For j As Integer = 0 To headers.Length - 1
                    out(0, j) = headers(j)
                Next
                r0 = 1
            End If
            
            For i As Integer = 0 To rows.Count - 1
                Dim rowVals() As Object = rows(i)
                For j As Integer = 0 To Math.Min(bodyCols, rowVals.Length) - 1
                    out(r0 + i, j) = rowVals(j)
                Next
            Next
            
            Return out
        End Function
        Public Shared Function BuildMultinomialSummary(result As LMresult, names() As String, predictorCount As Integer, hdr As Boolean, alphaValue As Double, expForDisplay As Func(Of Double, Object)) As Object(,)
                Dim q As Integer = result.Coeffs_est.Length
                Dim outRows As Integer = If(hdr, q + 1, q)
                Dim out(outRows - 1, 8) As Object

                Dim zCrit As Double = distributions.ZCritTwoSided(alphaValue)
                Dim r0 As Integer = 0

                If hdr Then
                    out(0, 0) = "Parameter"
                    out(0, 1) = "Type"
                    out(0, 2) = "Coef"
                    out(0, 3) = "SE"
                    out(0, 4) = "Z"
                    out(0, 5) = "P-value"
                    out(0, 6) = "OR"
                    out(0, 7) = "OR " & $"{100.0 * (1.0 - alphaValue):0.##}%" & " LCL"
                    out(0, 8) = "OR " & $"{100.0 * (1.0 - alphaValue):0.##}%" & " UCL"
                    r0 = 1
                End If

                For i As Integer = 0 To q - 1
                    Dim beta As Double = result.Coeffs_est(i)
                    Dim se As Double = result.Coeffs_SEs(i)
                    Dim z As Double = If(se > 0.0, beta / se, Double.NaN)
                    Dim pv As Double = If(se > 0.0, 2.0 * distributions.PNorm(-Math.Abs(z)), Double.NaN)
                    Dim pname As String = names(i)
                    Dim isInterceptParam As Boolean = pname IsNot Nothing AndAlso pname.Trim().EndsWith(": Intercept", StringComparison.OrdinalIgnoreCase)

                    out(r0 + i, 0) = pname
                    out(r0 + i, 1) = If(isInterceptParam, "Intercept", "Slope")
                    out(r0 + i, 2) = beta
                    out(r0 + i, 3) = se
                    out(r0 + i, 4) = z
                    out(r0 + i, 5) = pv

                    If isInterceptParam Then
                        out(r0 + i, 6) = ""
                        out(r0 + i, 7) = ""
                        out(r0 + i, 8) = ""
                    Else
                        out(r0 + i, 6) = expForDisplay(beta)
                        out(r0 + i, 7) = expForDisplay(beta - zCrit * se)
                        out(r0 + i, 8) = expForDisplay(beta + zCrit * se)
                    End If
                Next

                Return out
        End Function

        Public Shared Function BuildMultinomialTests(n As Integer, labels() As String, vals(,) As Object, hdr As Boolean) As Object(,)
                Dim outRows As Integer = If(hdr, n + 1, n)
                Dim out(outRows - 1, 3) As Object

                Dim r0 As Integer = 0
                If hdr Then
                    out(0, 0) = "Item"
                    out(0, 1) = "Value"
                    out(0, 2) = "df"
                    out(0, 3) = "P-value"
                    r0 = 1
                End If

                For i As Integer = 0 To n - 1
                    out(r0 + i, 0) = labels(i)
                    out(r0 + i, 1) = vals(i, 0)
                    out(r0 + i, 2) = vals(i, 1)
                    out(r0 + i, 3) = vals(i, 2)
                Next

                Return out
        End Function

        Public Shared Function BuildMultinomialClassification(cls As Global.BESHStatNG.regression.ClassificationCrosstab, cats() As Integer, k As Integer, hdr As Boolean) As Object(,)
                Dim outRows As Integer = If(hdr, k + 2, k + 1)
                Dim outCols As Integer = k + 2
                Dim out(outRows - 1, outCols - 1) As Object
                Dim r0 As Integer = 0

                If hdr Then
                    out(0, 0) = "Observed \ Predicted"
                    For j As Integer = 0 To k - 1
                        out(0, j + 1) = cats(j)
                    Next
                    out(0, k + 1) = "Recall %"
                    r0 = 1
                End If

                For i As Integer = 0 To k - 1
                    out(r0 + i, 0) = cats(i)
                    For j As Integer = 0 To k - 1
                        out(r0 + i, j + 1) = cls.Counts(i, j)
                    Next
                    out(r0 + i, k + 1) = cls.RecallPct(i)
                Next

                out(r0 + k, 0) = "Precision % / Overall"
                For j As Integer = 0 To k - 1
                    out(r0 + k, j + 1) = cls.PrecisionPct(j)
                Next
                out(r0 + k, k + 1) = cls.OverallAccuracyPct

                Return out
        End Function

        Public Shared Function BuildMultinomialPrediction(expandedX(,) As Double, b() As Double, cats() As Integer, offsetVals() As Double, nRows As Integer, hdr As Boolean, equationParameterCount As Integer, interceptIncluded As Boolean, baselineCategory As Integer, k As Integer) As Object
                Dim outRows As Integer = If(hdr, nRows + 1, nRows)
                Dim outCols As Integer = 1 + (k - 1) + k
                Dim out(outRows - 1, outCols - 1) As Object

                Dim r0 As Integer = 0
                If hdr Then
                    out(0, 0) = "PredictedCategory"
                    Dim c As Integer = 1
                    For j As Integer = 0 To k - 2
                        out(0, c) = "Eta(" & cats(j).ToString(CultureInfo.InvariantCulture) & " vs " & baselineCategory.ToString(CultureInfo.InvariantCulture) & ")"
                        c += 1
                    Next
                    For j As Integer = 0 To k - 1
                        out(0, c) = "P(Y=" & cats(j).ToString(CultureInfo.InvariantCulture) & ")"
                        c += 1
                    Next
                    r0 = 1
                End If

                For i As Integer = 0 To nRows - 1
                    Dim eta() As Double = Global.BESHStatNG.WorksheetCompatibility.UdfNumericPreparation38.ComputeMultinomialEtas(expandedX, i, b, equationParameterCount, interceptIncluded, offsetVals, cats.Length)
                    Dim probs() As Double = Global.BESHStatNG.WorksheetCompatibility.UdfNumericPreparation38.ComputeMultinomialProbabilities(eta)
                    Dim predIdx As Integer = Global.BESHStatNG.regression.CategoricalLogitUtils.ArgMax(probs, True)

                    out(r0 + i, 0) = cats(predIdx)
                    Dim c As Integer = 1
                    For j As Integer = 0 To eta.Length - 1
                        out(r0 + i, c) = eta(j)
                        c += 1
                    Next
                    For j As Integer = 0 To probs.Length - 1
                        out(r0 + i, c) = probs(j)
                        c += 1
                    Next
                Next

                Return out
        End Function

        Public Shared Function BuildOrdinalSummary(result As LMresult, names() As String, predictorCount As Integer, hdr As Boolean, alphaValue As Double, expForDisplay As Func(Of Double, Object)) As Object(,)
                Dim q As Integer = result.Coeffs_est.Length
                Dim outRows As Integer = If(hdr, q + 1, q)
                Dim out(outRows - 1, 8) As Object

                Dim zCrit As Double = distributions.ZCritTwoSided(alphaValue)
                Dim r0 As Integer = 0

                If hdr Then
                    out(0, 0) = "Parameter"
                    out(0, 1) = "Type"
                    out(0, 2) = "Coef"
                    out(0, 3) = "SE"
                    out(0, 4) = "Z"
                    out(0, 5) = "P-value"
                    out(0, 6) = "OR"
                    out(0, 7) = "OR " & $"{100.0 * (1.0 - alphaValue):0.##}%" & " LCL"
                    out(0, 8) = "OR " & $"{100.0 * (1.0 - alphaValue):0.##}%" & " UCL"
                    r0 = 1
                End If

                For i As Integer = 0 To q - 1
                    Dim beta As Double = result.Coeffs_est(i)
                    Dim se As Double = result.Coeffs_SEs(i)
                    Dim z As Double = If(se > 0.0, beta / se, Double.NaN)
                    Dim pv As Double = If(se > 0.0, 2.0 * distributions.PNorm(-Math.Abs(z)), Double.NaN)

                    Dim isSlope As Boolean = (i < predictorCount)

                    out(r0 + i, 0) = names(i)
                    out(r0 + i, 1) = If(isSlope, "Slope", "Threshold")
                    out(r0 + i, 2) = beta
                    out(r0 + i, 3) = se
                    out(r0 + i, 4) = z
                    out(r0 + i, 5) = pv

                    If isSlope Then
                        out(r0 + i, 6) = expForDisplay(beta)
                        out(r0 + i, 7) = expForDisplay(beta - zCrit * se)
                        out(r0 + i, 8) = expForDisplay(beta + zCrit * se)
                    Else
                        out(r0 + i, 6) = ""
                        out(r0 + i, 7) = ""
                        out(r0 + i, 8) = ""
                    End If
                Next

                Return out
        End Function

        Public Shared Function BuildOrdinalTests(n As Integer, labels() As String, vals(,) As Object, hdr As Boolean) As Object(,)
                Dim outRows As Integer = If(hdr, n + 1, n)
                Dim out(outRows - 1, 3) As Object

                Dim r0 As Integer = 0
                If hdr Then
                    out(0, 0) = "Item"
                    out(0, 1) = "Value"
                    out(0, 2) = "df"
                    out(0, 3) = "P-value"
                    r0 = 1
                End If

                For i As Integer = 0 To n - 1
                    out(r0 + i, 0) = labels(i)
                    out(r0 + i, 1) = vals(i, 0)
                    out(r0 + i, 2) = vals(i, 1)
                    out(r0 + i, 3) = vals(i, 2)
                Next

                Return out
        End Function

        Public Shared Function BuildOrdinalClassification(cls As Global.BESHStatNG.regression.ClassificationCrosstab, cats() As Integer, k As Integer, hdr As Boolean) As Object(,)
                Dim outRows As Integer = If(hdr, k + 3, k + 2)
                Dim outCols As Integer = k + 2
                Dim out(outRows - 1, outCols - 1) As Object

                Dim r As Integer = 0
                If hdr Then
                    out(0, 0) = "Observed \ Predicted"
                    For j As Integer = 0 To k - 1
                        out(0, j + 1) = cls.Categories(j)
                    Next
                    out(0, outCols - 1) = "Recall %"
                    r = 1
                End If

                For i As Integer = 0 To k - 1
                    out(r + i, 0) = cls.Categories(i)
                    For j As Integer = 0 To k - 1
                        out(r + i, j + 1) = cls.Counts(i, j)
                    Next
                    out(r + i, outCols - 1) = cls.RecallPct(i)
                Next

                out(r + k, 0) = "Precision %"
                For j As Integer = 0 To k - 1
                    out(r + k, j + 1) = cls.PrecisionPct(j)
                Next
                out(r + k, outCols - 1) = ""

                out(r + k + 1, 0) = "Overall accuracy %"
                out(r + k + 1, 1) = cls.OverallAccuracyPct
                For j As Integer = 2 To outCols - 1
                    out(r + k + 1, j) = ""
                Next

                Return out
        End Function

        Public Shared Function BuildOrdinalPrediction(expandedX(,) As Double, b() As Double, cats() As Integer, offsetVals() As Double, nRows As Integer, hdr As Boolean, predictorCount As Integer, invalid As Object) As Object
                Dim outRows As Integer = If(hdr, nRows + 1, nRows)
                Dim outCols As Integer = cats.Length + 2
                Dim out(outRows - 1, outCols - 1) As Object

                Dim r0 As Integer = 0
                If hdr Then
                    out(0, 0) = "PredictedCategory"
                    out(0, 1) = "LinearPredictor"
                    For k As Integer = 0 To cats.Length - 1
                        out(0, k + 2) = "P(Y=" & cats(k).ToString(CultureInfo.InvariantCulture) & ")"
                    Next
                    r0 = 1
                End If

                For i As Integer = 0 To nRows - 1
                    Dim eta As Double = 0.0
                    For j As Integer = 0 To predictorCount - 1
                        Dim xv As Double = expandedX(i, j)
                        If Double.IsNaN(xv) OrElse Double.IsInfinity(xv) Then Return invalid
                        eta += xv * b(j)
                    Next
                    If offsetVals IsNot Nothing Then
                        eta += offsetVals(i)
                    End If

                    Dim probs() As Double = Global.BESHStatNG.WorksheetCompatibility.UdfNumericPreparation38.ComputeOrdinalProbabilities(b, predictorCount, cats.Length, eta)
                    Dim predIdx As Integer = Global.BESHStatNG.regression.CategoricalLogitUtils.ArgMax(probs, True)

                    out(r0 + i, 0) = cats(predIdx)
                    out(r0 + i, 1) = eta
                    For k As Integer = 0 To cats.Length - 1
                        out(r0 + i, k + 2) = probs(k)
                    Next
                Next

                Return out
        End Function

        Public Shared Function BuildKaplanMeierTable(outObj As Object(), noRows As Object) As Object
                Dim totalRows As Integer = 0
                For Each g In outObj
                    If g Is Nothing Then Continue For
                    Dim lst = TryCast(g, System.Collections.IEnumerable)
                    If lst Is Nothing Then Continue For
                    totalRows += lst.Cast(Of Object)().Count()
                Next

                If totalRows = 0 Then Return noRows

                Dim result(totalRows - 1, 6) As Object

                Dim r As Integer = 0
                For Each g In outObj
                    If g Is Nothing Then Continue For
                    Dim lst = TryCast(g, System.Collections.IEnumerable)
                    If lst Is Nothing Then Continue For

                    For Each itemObj In lst
                        Dim rec As survival.SurvivalTableRecord = CType(itemObj, survival.SurvivalTableRecord)
                        result(r, 0) = rec.strGroup
                        result(r, 1) = rec.Time
                        result(r, 2) = rec.AtRisk
                        result(r, 3) = rec.Prob
                        result(r, 4) = rec.SE
                        result(r, 5) = rec.ProbCILL
                        result(r, 6) = rec.ProbCIUL
                        r += 1
                    Next
                Next

                Return result
        End Function
    End Class
End Namespace
