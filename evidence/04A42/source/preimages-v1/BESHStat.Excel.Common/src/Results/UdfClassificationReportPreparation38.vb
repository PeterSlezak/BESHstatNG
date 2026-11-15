Option Explicit On
Option Strict On

Imports System
Imports System.Collections.Generic
Imports BESHStatNG.regression

Namespace ExcelCommon
    Public NotInheritable Class UdfClassificationReportPreparation38
        Private Sub New()
        End Sub

        Public Shared Function BuildBinaryCrosstabUdfOutput(summary As BinaryClassificationSummary, missingValue As Object,
                                                     Optional includeHeader As Boolean = True) As Object
            Dim rowOffset As Integer = If(includeHeader, 1, 0)
            Dim outRows As Integer = rowOffset + 4
            Dim out(outRows - 1, 3) As Object

            If includeHeader Then
                out(0, 0) = "Observed \ Predicted"
                out(0, 1) = 0
                out(0, 2) = 1
                out(0, 3) = "Recall %"
            End If

            out(rowOffset + 0, 0) = 0
            out(rowOffset + 0, 1) = summary.TN
            out(rowOffset + 0, 2) = summary.FP
            out(rowOffset + 0, 3) = PercentOrMissingValue(summary.Specificity, missingValue)

            out(rowOffset + 1, 0) = 1
            out(rowOffset + 1, 1) = summary.FN
            out(rowOffset + 1, 2) = summary.TP
            out(rowOffset + 1, 3) = PercentOrMissingValue(summary.Sensitivity, missingValue)

            out(rowOffset + 2, 0) = "Precision % / Overall"
            out(rowOffset + 2, 1) = PercentOrMissingValue(summary.NPV, missingValue)
            out(rowOffset + 2, 2) = PercentOrMissingValue(summary.Precision, missingValue)
            out(rowOffset + 2, 3) = PercentOrMissingValue(summary.Accuracy, missingValue)

            out(rowOffset + 3, 0) = "Threshold / Balanced accuracy"
            out(rowOffset + 3, 1) = summary.Threshold
            out(rowOffset + 3, 2) = PercentOrMissingValue(summary.BalancedAccuracy, missingValue)
            out(rowOffset + 3, 3) = PercentOrMissingValue(summary.YoudenJ, missingValue)

            Return UdfSpillPreparation38.NormalizeBlanks(out)
        End Function

        Public Shared Function BuildThresholdTableUdfOutput(rows As IList(Of BinaryThresholdRow), missingValue As Object,
                                                     Optional includeHeader As Boolean = True) As Object
            If rows Is Nothing OrElse rows.Count = 0 Then Return missingValue

            Dim rowOffset As Integer = If(includeHeader, 1, 0)
            Dim outRows As Integer = rowOffset + rows.Count
            Dim out(outRows - 1, 13) As Object

            If includeHeader Then
                out(0, 0) = "Threshold"
                out(0, 1) = "TP"
                out(0, 2) = "FP"
                out(0, 3) = "TN"
                out(0, 4) = "FN"
                out(0, 5) = "Sensitivity %"
                out(0, 6) = "Specificity %"
                out(0, 7) = "Precision %"
                out(0, 8) = "Recall %"
                out(0, 9) = "NPV %"
                out(0, 10) = "Accuracy %"
                out(0, 11) = "BalancedAccuracy %"
                out(0, 12) = "YoudenJ %"
                out(0, 13) = "F1 %"
            End If

            For i As Integer = 0 To rows.Count - 1
                Dim r As BinaryThresholdRow = rows(i)
                Dim rr As Integer = rowOffset + i
                out(rr, 0) = r.Threshold
                out(rr, 1) = r.TP
                out(rr, 2) = r.FP
                out(rr, 3) = r.TN
                out(rr, 4) = r.FN
                out(rr, 5) = PercentOrMissingValue(r.Sensitivity, missingValue)
                out(rr, 6) = PercentOrMissingValue(r.Specificity, missingValue)
                out(rr, 7) = PercentOrMissingValue(r.Precision, missingValue)
                out(rr, 8) = PercentOrMissingValue(r.Recall, missingValue)
                out(rr, 9) = PercentOrMissingValue(r.NPV, missingValue)
                out(rr, 10) = PercentOrMissingValue(r.Accuracy, missingValue)
                out(rr, 11) = PercentOrMissingValue(r.BalancedAccuracy, missingValue)
                out(rr, 12) = PercentOrMissingValue(r.YoudenJ, missingValue)
                out(rr, 13) = PercentOrMissingValue(r.F1, missingValue)
            Next

            Return UdfSpillPreparation38.NormalizeBlanks(out)
        End Function

        Public Shared Function BuildCalibrationTableUdfOutput(rows As IList(Of CalibrationBinSummary), missingValue As Object,
                                                       Optional includeHeader As Boolean = True) As Object
            If rows Is Nothing OrElse rows.Count = 0 Then Return missingValue

            Dim rowOffset As Integer = If(includeHeader, 1, 0)
            Dim outRows As Integer = rowOffset + rows.Count
            Dim out(outRows - 1, 5) As Object

            If includeHeader Then
                out(0, 0) = "Bin"
                out(0, 1) = "N"
                out(0, 2) = "MeanPredicted"
                out(0, 3) = "ObservedRate"
                out(0, 4) = "LowerCI"
                out(0, 5) = "UpperCI"
            End If

            For i As Integer = 0 To rows.Count - 1
                Dim r As CalibrationBinSummary = rows(i)
                Dim rr As Integer = rowOffset + i
                out(rr, 0) = r.BinIndex
                out(rr, 1) = r.N
                out(rr, 2) = r.MeanPredicted
                out(rr, 3) = r.ObservedRate
                out(rr, 4) = r.LowerCI
                out(rr, 5) = r.UpperCI
            Next

            Return UdfSpillPreparation38.NormalizeBlanks(out)
        End Function

        Public Shared Function BuildBrierScoreUdfOutput(score As Double, missingValue As Object,
                                                 Optional n As Double = Double.NaN,
                                                 Optional eventRate As Double = Double.NaN,
                                                 Optional includeHeader As Boolean = True) As Object
            Dim totalRows As Integer = 1
            If Not Double.IsNaN(n) Then totalRows += 1
            If Not Double.IsNaN(eventRate) Then totalRows += 1

            Dim rowOffset As Integer = If(includeHeader, 1, 0)
            Dim outRows As Integer = rowOffset + totalRows
            Dim out(outRows - 1, 1) As Object

            If includeHeader Then
                out(0, 0) = "Item"
                out(0, 1) = "Value"
            End If

            Dim r As Integer = rowOffset
            out(r, 0) = "BrierScore"
            out(r, 1) = score
            r += 1

            If Not Double.IsNaN(n) Then
                out(r, 0) = "N"
                out(r, 1) = n
                r += 1
            End If

            If Not Double.IsNaN(eventRate) Then
                out(r, 0) = "EventRate"
                out(r, 1) = eventRate
            End If

            Return UdfSpillPreparation38.NormalizeBlanks(out)
        End Function

        Private Shared Function PercentOrMissingValue(value As Double, missingValue As Object) As Object
            If Double.IsNaN(value) OrElse Double.IsInfinity(value) Then Return missingValue
            Return 100.0R * value
        End Function
    End Class
End Namespace
