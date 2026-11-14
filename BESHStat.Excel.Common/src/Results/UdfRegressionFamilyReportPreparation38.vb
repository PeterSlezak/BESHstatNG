Option Explicit On
Option Strict On
Option Infer On

Imports System
Imports System.Globalization

Namespace ExcelCommon
    Public NotInheritable Class UdfRegressionFamilyReportPreparation38
        Private Sub New()
        End Sub

        Public Shared Function CreateGeeLsmEstimateTable(labelCount As Integer, alphaValue As Double) As Object(,)
            Dim hdr() As String = {"Label", "Estimate", "Std. Error", "Z", "P-value",
                                   ((100.0R * (1.0R - alphaValue)).ToString("0.##", CultureInfo.InvariantCulture) & "% CI Lower"),
                                   ((100.0R * (1.0R - alphaValue)).ToString("0.##", CultureInfo.InvariantCulture) & "% CI Upper"),
                                   "Scale", "Covariance"}
            Dim out(labelCount, hdr.Length - 1) As Object
            For j As Integer = 0 To hdr.Length - 1
                out(0, j) = hdr(j)
            Next
            Return out
        End Function

        Public Shared Sub AppendGeeLsmEstimateRow(out As Object(,), r As Integer, label As String, estOut As Double, seOut As Double, z As Double, pValue As Double, loOut As Double, hiOut As Double, scaleLabel As String, covName As String, safeNumber As Func(Of Double, Object))
            out(r + 1, 0) = label
            out(r + 1, 1) = safeNumber(estOut)
            out(r + 1, 2) = safeNumber(seOut)
            out(r + 1, 3) = safeNumber(z)
            out(r + 1, 4) = safeNumber(pValue)
            out(r + 1, 5) = safeNumber(loOut)
            out(r + 1, 6) = safeNumber(hiOut)
            out(r + 1, 7) = scaleLabel
            out(r + 1, 8) = covName
        End Sub

        Public Shared Function BuildGlmCoefficientTable(beta As Double(), se As Double(), names As String(), hdr As Boolean, alphaValue As Double, interceptIncluded As Boolean) As Object(,)
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
                out(r0 + i, 1) = If(interceptIncluded AndAlso i = 0, "Intercept", "Slope")
                out(r0 + i, 2) = beta(i)
                out(r0 + i, 3) = se(i)
                out(r0 + i, 4) = z
                out(r0 + i, 5) = pv
                out(r0 + i, 6) = beta(i) - zCrit * se(i)
                out(r0 + i, 7) = beta(i) + zCrit * se(i)
            Next
            Return out
        End Function

        Public Shared Function BuildRegressionTestsPrefix(labels As String(), vals As Object(,), hdr As Boolean, extraRows As Integer, ByRef r As Integer) As Object(,)
            Dim outRows As Integer = If(hdr, labels.Length + extraRows + 1, labels.Length + extraRows)
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
                out(r, 2) = vals(i, 1)
                out(r, 3) = vals(i, 2)
                r += 1
            Next
            Return out
        End Function

        Public Shared Function CompleteRegressionParameterNames(out As String(), nextIndex As Integer, varNames As String()) As String()
            Dim predictorNames() As String = If(varNames, New String() {})
            For i As Integer = 0 To predictorNames.Length - 1
                If nextIndex + i <= (out.Length - 1) Then
                    out(nextIndex + i) = predictorNames(i)
                End If
            Next

            For i As Integer = 0 To (out.Length - 1)
                If String.IsNullOrWhiteSpace(out(i)) Then
                    out(i) = "Param" & (i + 1).ToString(CultureInfo.InvariantCulture)
                End If
            Next

            Return out
        End Function

        Public Shared Function BuildAllGlmResidualTable(raw As Double(), dev As Double(), pear As Double(), leverage As Double(), sdev As Double(), spear As Double(), cook As Double(), includeHeader As Boolean) As Object(,)
            Dim n As Integer = raw.Length
            Dim outRows As Integer = If(includeHeader, n + 1, n)
            Dim out(outRows - 1, 6) As Object
            Dim r0 As Integer = 0

            If includeHeader Then
                out(0, 0) = "RawResidual"
                out(0, 1) = "DevianceResidual"
                out(0, 2) = "PearsonResidual"
                out(0, 3) = "Leverage"
                out(0, 4) = "StdDevianceResidual"
                out(0, 5) = "StdPearsonResidual"
                out(0, 6) = "CookDistance"
                r0 = 1
            End If

            For i As Integer = 0 To n - 1
                out(r0 + i, 0) = raw(i)
                out(r0 + i, 1) = dev(i)
                out(r0 + i, 2) = pear(i)
                out(r0 + i, 3) = leverage(i)
                out(r0 + i, 4) = sdev(i)
                out(r0 + i, 5) = spear(i)
                out(r0 + i, 6) = cook(i)
            Next

            Return out
        End Function

        Public Shared Function BuildLmCoefficientTable(model As regression.LinearModel, coef As Double(), se As Double(), dfResid As Double, q As Integer, hdr As Boolean, noteCount As Integer, alphaValue As Double) As Object(,)
            Dim outRows As Integer = q + If(hdr, 1, 0) + noteCount
            Dim out(outRows - 1, 7) As Object

            Dim r0 As Integer = 0
            If hdr Then
                out(0, 0) = "Parameter"
                out(0, 1) = "Type"
                out(0, 2) = "Coef"
                out(0, 3) = "SE"
                out(0, 4) = "T"
                out(0, 5) = "P-value"
                out(0, 6) = $"{100.0 * (1.0 - alphaValue):0.##}% CI LCL"
                out(0, 7) = $"{100.0 * (1.0 - alphaValue):0.##}% CI UCL"
                r0 = 1
            End If

            Dim tCrit As Double = distributions.T_Inv_2T(alphaValue, dfResid)
            For i As Integer = 0 To q - 1
                Dim pname As String = If(model.results.bIntercept AndAlso i = 0,
                                         "Intercept",
                                         model.results.varNames(i - If(model.results.bIntercept, 1, 0)))
                Dim tVal As Double = If(se(i) > 0.0, coef(i) / se(i), Double.NaN)
                Dim pVal As Double = If(se(i) > 0.0, distributions.T_2T(Math.Abs(tVal), dfResid), Double.NaN)

                out(r0 + i, 0) = pname
                out(r0 + i, 1) = If(model.results.bIntercept AndAlso i = 0, "Intercept", "Slope")
                out(r0 + i, 2) = coef(i)
                out(r0 + i, 3) = se(i)
                out(r0 + i, 4) = tVal
                out(r0 + i, 5) = pVal
                out(r0 + i, 6) = coef(i) - tCrit * se(i)
                out(r0 + i, 7) = coef(i) + tCrit * se(i)
            Next
            Return out
        End Function

        Public Shared Function BuildLmTestsTable(labels As String(), vals As Object(,), n As Integer, hdr As Boolean, noteCount As Integer) As Object(,)
            Dim outRows As Integer = n + If(hdr, 1, 0) + noteCount
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

        Public Shared Function DropTopRows(arr As Object(,), rowsToDrop As Integer) As Object(,)
            If arr Is Nothing Then Return Nothing
            If rowsToDrop <= 0 Then Return arr

            Dim rows As Integer = arr.GetLength(0)
            Dim cols As Integer = arr.GetLength(1)
            If rowsToDrop >= rows Then
                Dim emptyOut(0, Math.Max(0, cols - 1)) As Object
                Return emptyOut
            End If

            Dim out(rows - rowsToDrop - 1, cols - 1) As Object
            For i As Integer = rowsToDrop To rows - 1
                For j As Integer = 0 To cols - 1
                    out(i - rowsToDrop, j) = arr(i, j)
                Next
            Next
            Return out
        End Function

        Public Shared Function AppendFootnotes(arr As Object(,), notes As String()) As Object(,)
            If arr Is Nothing Then Return Nothing
            If notes Is Nothing OrElse notes.Length = 0 Then Return arr

            Dim rows As Integer = arr.GetLength(0)
            Dim cols As Integer = arr.GetLength(1)
            Dim out(rows + notes.Length - 1, cols - 1) As Object

            For i As Integer = 0 To rows - 1
                For j As Integer = 0 To cols - 1
                    out(i, j) = arr(i, j)
                Next
            Next

            For i As Integer = 0 To notes.Length - 1
                out(rows + i, 0) = notes(i)
            Next

            Return out
        End Function

        Public Shared Sub AppendFootnotesInPlace(arr As Object(,), startRow As Integer, notes As String())
            If arr Is Nothing OrElse notes Is Nothing Then Exit Sub
            For i As Integer = 0 To notes.Length - 1
                arr(startRow + i, 0) = notes(i)
            Next
        End Sub

        Public Shared Function BuildLmResidualColumn(fullTable As Object(,), hdr As Boolean, colIndex As Integer, colName As String) As Object(,)
            Dim startRow As Integer = If(hdr, 0, 1)
            Dim rowCount As Integer = fullTable.GetLength(0) - startRow
            Dim out(rowCount - 1, 0) As Object

            Dim r As Integer = 0
            If hdr Then
                out(0, 0) = colName
                r = 1
            End If

            For i As Integer = 1 To fullTable.GetLength(0) - 1
                out(r, 0) = fullTable(i, colIndex)
                r += 1
            Next
            Return out
        End Function

        Public Shared Function CreateLmPredictionTable(nRows As Integer, hdr As Boolean, ByRef r0 As Integer) As Object(,)
            Dim outRows As Integer = If(hdr, nRows + 1, nRows)
            Dim out(outRows - 1, 0) As Object

            r0 = 0
            If hdr Then
                out(0, 0) = "PredictedY"
                r0 = 1
            End If
            Return out
        End Function
    End Class
End Namespace
