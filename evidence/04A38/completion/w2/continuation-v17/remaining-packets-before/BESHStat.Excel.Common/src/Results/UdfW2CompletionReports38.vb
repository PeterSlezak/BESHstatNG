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

        Public Shared Function BuildMultinomialResidualOutput(res As regression.MultinomialResiduals, categories() As Integer, includeHeader As Boolean, unavailable As Object) As Object
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

        Public Shared Function BuildOrdinalResidualOutput(res As regression.MultinomialResiduals, includeHeader As Boolean, unavailable As Object) As Object
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
    End Class
End Namespace
