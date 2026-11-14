Option Explicit On
Option Strict On
Option Infer On

Imports System
Imports System.Collections.Generic
Imports System.Globalization

Namespace ExcelCommon
    Public NotInheritable Class UdfCoxGeeReportPreparation38
        Private Sub New()
        End Sub

        Public Shared Function BuildCoxSummaryTable(hdr As Boolean, p As Integer, varNames As String(), result As CoxResult, robust As Boolean, zCrit As Double, ciPct As String, expForDisplay As Func(Of Double, Object)) As Object(,)
            Dim rows As Integer = If(hdr, p + 1, p)
            Dim out(rows - 1, 7) As Object

            Dim r0 As Integer = 0
            If hdr Then
                out(0, 0) = "Variable"
                out(0, 1) = "Coef"
                out(0, 2) = "SE"
                out(0, 3) = "Z"
                out(0, 4) = "P-value"
                out(0, 5) = "HR"
                out(0, 6) = "HR " & ciPct & " LCL"
                out(0, 7) = "HR " & ciPct & " UCL"
                r0 = 1
            End If

            For i As Integer = 0 To p - 1
                Dim beta As Double = result.Coefficients(i)
                Dim se As Double
                If robust AndAlso result.VarCovRobust IsNot Nothing AndAlso result.VarCovRobust.GetLength(0) = p Then
                    se = Math.Sqrt(result.VarCovRobust(i, i))
                Else
                    se = Math.Sqrt(result.VarCov(i, i))
                End If

                Dim z As Double = beta / se
                Dim pv As Double = 2.0 * distributions.PNorm(-Math.Abs(z))
                Dim hr As Object = expForDisplay(beta)
                Dim lcl As Object = expForDisplay(beta - zCrit * se)
                Dim ucl As Object = expForDisplay(beta + zCrit * se)

                out(r0 + i, 0) = varNames(i)
                out(r0 + i, 1) = beta
                out(r0 + i, 2) = se
                out(r0 + i, 3) = z
                out(r0 + i, 4) = pv
                out(r0 + i, 5) = hr
                out(r0 + i, 6) = lcl
                out(r0 + i, 7) = ucl
            Next
            Return out
        End Function

        Public Shared Function BuildCoxTestsTable(hdr As Boolean, p As Integer, result As CoxResult, lr As Object, lrP As Object, wald As Object, waldP As Object, unavailable As Object) As Object(,)
            Dim nRows As Integer = If(hdr, 7, 5)
            Dim out(nRows - 1, 3) As Object
            Dim r As Integer = 0

            If hdr Then
                out(0, 0) = "Item"
                out(0, 1) = "Value"
                out(0, 2) = "df"
                out(0, 3) = "P-value"
                r = 1
            End If

            out(r + 0, 0) = "Likelihood-ratio chi-square"
            out(r + 0, 1) = lr
            out(r + 0, 2) = p
            out(r + 0, 3) = lrP

            out(r + 1, 0) = "Wald chi-square"
            out(r + 1, 1) = wald
            out(r + 1, 2) = p
            out(r + 1, 3) = waldP

            out(r + 2, 0) = "Log-likelihood (null)"
            out(r + 2, 1) = If(Not Double.IsNaN(result.LogLikelihoodNull) AndAlso Not Double.IsInfinity(result.LogLikelihoodNull), CType(result.LogLikelihoodNull, Object), unavailable)
            out(r + 2, 2) = ""
            out(r + 2, 3) = ""

            out(r + 3, 0) = "Log-likelihood (fitted)"
            out(r + 3, 1) = If(Not Double.IsNaN(result.LogLikelihood) AndAlso Not Double.IsInfinity(result.LogLikelihood), CType(result.LogLikelihood, Object), unavailable)
            out(r + 3, 2) = ""
            out(r + 3, 3) = ""

            out(r + 4, 0) = "Iterations"
            out(r + 4, 1) = result.Iterations
            out(r + 4, 2) = ""
            out(r + 4, 3) = ""

            If hdr Then
                out(r + 5, 0) = "Converged"
                out(r + 5, 1) = result.Converged
                out(r + 5, 2) = ""
                out(r + 5, 3) = ""
            End If
            Return out
        End Function

        Public Shared Function BuildCoxResidualTable(d As Dictionary(Of Integer, Double()), keys As List(Of Integer), width As Integer, varNames As String(), rt As ResidualType) As Object(,)
            Dim out(keys.Count, width) As Object
            out(0, 0) = "Row ID"
            If width = 1 Then
                out(0, 1) = CoxUDFsMetadata38.ResidualLabel(rt)
            Else
                For j As Integer = 0 To width - 1
                    out(0, j + 1) = If(j < varNames.Length, varNames(j), "V" & (j + 1).ToString())
                Next
            End If
            For i As Integer = 0 To keys.Count - 1
                out(i + 1, 0) = keys(i)
                Dim vals = d(keys(i))
                For j As Integer = 0 To vals.Length - 1
                    out(i + 1, j + 1) = vals(j)
                Next
            Next
            Return out
        End Function

        Public Shared Function CreateCoxBaselineTable(total As Integer, tp As String) As Object(,)
            Dim cols As Integer = If(LegacyUdfText38.EqualText(tp, "survival") OrElse LegacyUdfText38.EqualText(tp, "cumhaz"), 3, 4)
            Dim out(total - 1, cols - 1) As Object
            If cols = 4 Then
                out(0, 0) = "Stratum" : out(0, 1) = "Time" : out(0, 2) = "Survival" : out(0, 3) = "CumHazard"
            Else
                out(0, 0) = "Stratum" : out(0, 1) = "Time" : out(0, 2) = If(LegacyUdfText38.EqualText(tp, "survival"), "Survival", "CumHazard")
            End If
            Return out
        End Function

        Public Shared Sub AppendCoxBaselineRows(out As Object(,), arr As Double(,), key As Object, tp As String, ByRef r As Integer)
            For i As Integer = 0 To arr.GetLength(0) - 1
                out(r, 0) = Convert.ToString(key)
                out(r, 1) = arr(i, 0)
                If LegacyUdfText38.EqualText(tp, "survival") Then
                    out(r, 2) = arr(i, 1)
                ElseIf LegacyUdfText38.EqualText(tp, "cumhaz") OrElse LegacyUdfText38.EqualText(tp, "hazard") Then
                    out(r, 2) = arr(i, 2)
                Else
                    out(r, 2) = arr(i, 1)
                    out(r, 3) = arr(i, 2)
                End If
                r += 1
            Next
        End Sub

        Public Shared Function BuildGeeSummaryTable(beta As Double(), se As Double(), names As String(), hdr As Boolean, alphaValue As Double) As Object(,)
            Dim outRows As Integer = If(hdr, beta.Length + 1, beta.Length)
            Dim out(outRows - 1, 7) As Object
            Dim zCrit As Double = distributions.ZCritTwoSided(alphaValue)
            Dim ciLabel As String = (100.0R * (1.0R - alphaValue)).ToString("0.##", CultureInfo.InvariantCulture) & "%"
            Dim r0 As Integer = 0

            If hdr Then
                out(0, 0) = "Parameter"
                out(0, 1) = "Type"
                out(0, 2) = "Coef"
                out(0, 3) = "SE"
                out(0, 4) = "Z"
                out(0, 5) = "P-value"
                out(0, 6) = ciLabel & " CI Lower"
                out(0, 7) = ciLabel & " CI Upper"
                r0 = 1
            End If

            For i As Integer = 0 To beta.Length - 1
                Dim z As Double = If(se(i) > 0.0R, beta(i) / se(i), Double.NaN)
                Dim pv As Double = If(se(i) > 0.0R, 2.0R * distributions.PNorm(-Math.Abs(z)), Double.NaN)

                out(r0 + i, 0) = names(i)
                out(r0 + i, 1) = If(i = 0, "Intercept", "Slope")
                out(r0 + i, 2) = beta(i)
                out(r0 + i, 3) = se(i)
                out(r0 + i, 4) = z
                out(r0 + i, 5) = pv
                out(r0 + i, 6) = beta(i) - zCrit * se(i)
                out(r0 + i, 7) = beta(i) + zCrit * se(i)
            Next
            Return out
        End Function

        Public Shared Function BuildGeeTestsPrefix(labels As String(), vals As Object(,), hdr As Boolean, ByRef r As Integer) As Object(,)
            Dim outRows As Integer = If(hdr, labels.Length + 2, labels.Length + 1)
            Dim out(outRows - 1, 3) As Object
            r = 0

            If hdr Then
                out(0, 0) = "Item"
                out(0, 1) = "Value"
                out(0, 2) = "df"
                out(0, 3) = "P-value"
                r = 1
            End If

            For i As Integer = 0 To labels.Length - 1
                out(r, 0) = labels(i)
                out(r, 1) = vals(i, 0)
                out(r, 2) = If(vals.GetLength(1) > 1, vals(i, 1), "")
                out(r, 3) = If(vals.GetLength(1) > 2, vals(i, 2), "")
                r += 1
            Next
            Return out
        End Function

        Public Shared Sub AppendGeeResidualRows(rows As List(Of Object()), raw As Double(), dev As Double(), pear As Double(), sdev As Double(), spear As Double(), work As Double())
            For i As Integer = 0 To raw.Length - 1
                rows.Add(New Object() {raw(i), dev(i), pear(i), sdev(i), spear(i), work(i)})
            Next
        End Sub

        Public Shared Function CreateGeePredictionTable(nRows As Integer, hdr As Boolean, ByRef r0 As Integer) As Object(,)
            Dim outRows As Integer = If(hdr, nRows + 1, nRows)
            Dim out(outRows - 1, 1) As Object
            r0 = 0

            If hdr Then
                out(0, 0) = "PredictedResponse"
                out(0, 1) = "LinearPredictor"
                r0 = 1
            End If
            Return out
        End Function

        Public Shared Function MaterializeGeeRows(rows As List(Of Object()), includeHeader As Boolean, headers As String(), unavailable As Object) As Object
            If rows Is Nothing Then Return unavailable

            Dim colCount As Integer = If(headers Is Nothing, 0, headers.Length)
            If colCount = 0 AndAlso rows.Count > 0 Then
                colCount = rows(0).Length
            End If
            If colCount < 1 Then Return unavailable

            Dim outRows As Integer = rows.Count + If(includeHeader, 1, 0)
            Dim out(outRows - 1, colCount - 1) As Object
            Dim r0 As Integer = 0

            If includeHeader Then
                For j As Integer = 0 To colCount - 1
                    out(0, j) = headers(j)
                Next
                r0 = 1
            End If

            For i As Integer = 0 To rows.Count - 1
                Dim row() As Object = rows(i)
                For j As Integer = 0 To colCount - 1
                    out(r0 + i, j) = If(j < row.Length, row(j), Nothing)
                Next
            Next

            Return out
        End Function
    End Class
End Namespace
