Option Explicit On

Imports System
Imports System.Collections.Generic

Namespace regression

    ''' <summary>
    ''' Summary metrics for binary classification at a single threshold.
    ''' Framework compatibility facade plus Excel/UDF/ROC host integration for binary classification reporting.
    ''' </summary>
    Public Module BinaryClassificationReporting

        ''' <summary>
        ''' Host-neutral sentinel used in portable tabular outputs to indicate that a cell
        ''' should be rendered as a missing/not-available value by the caller. Excel UDF
        ''' wrappers convert this sentinel to <c>ExcelErrorNA</c>; non-Excel callers can
        ''' map it to <c>null</c>, <c>DBNull</c>, JSON <c>null</c>, or another host-specific
        ''' missing-value representation.
        ''' </summary>
        Public ReadOnly MissingOutputValue As Object = New BinaryClassificationMissingOutputValue()

        ''' <summary>
        ''' Returns <c>True</c> when <paramref name="value"/> is the host-neutral missing-output sentinel.
        ''' </summary>
        Public Function IsMissingOutputValue(value As Object) As Boolean
            Return Object.ReferenceEquals(value, MissingOutputValue)
        End Function

        Private NotInheritable Class BinaryClassificationMissingOutputValue
            Public Overrides Function ToString() As String
                Return "N/A"
            End Function
        End Class

        Public Sub ValidateBinaryInputs(y() As Double,
                                        p() As Double,
                                        Optional weights() As Double = Nothing,
                                        Optional allowMissing As Boolean = False)
            BinaryClassificationCore.ValidateBinaryInputs(y, p, weights, allowMissing)
        End Sub

        Public Function ComputeBinarySummary(y() As Double,
                                             p() As Double,
                                             Optional threshold As Double = 0.5,
                                             Optional weights() As Double = Nothing) As BinaryClassificationSummary
            Return BinaryClassificationCore.ComputeBinarySummary(y, p, threshold, weights)
        End Function

        Public Function BuildThresholdTable(y() As Double,
                                            p() As Double,
                                            Optional thresholds() As Double = Nothing,
                                            Optional weights() As Double = Nothing,
                                            Optional maxRows As Integer = 500) As List(Of BinaryThresholdRow)
            Return BinaryClassificationCore.BuildThresholdTable(y, p, thresholds, weights, maxRows)
        End Function

        Public Function ComputeBrierScore(y() As Double, p() As Double, Optional weights() As Double = Nothing) As Double
            Return BinaryClassificationCore.ComputeBrierScore(y, p, weights)
        End Function

        Public Function BuildCalibrationBins(y() As Double,
                                             p() As Double,
                                             Optional bins As Integer = 10,
                                             Optional weights() As Double = Nothing,
                                             Optional method As String = "quantile") As List(Of CalibrationBinSummary)
            Return BinaryClassificationCore.BuildCalibrationBins(y, p, bins, weights, method)
        End Function

        ''' <summary>
        ''' Parses and normalizes a calibration-binning method supplied from Excel.
        ''' Supported return values are <c>quantile</c> and <c>equalwidth</c>.
        ''' </summary>
        Public Function ParseCalibrationMethod(arg As Object,
                                               Optional defaultValue As String = "quantile") As String
            If BESHStatNG.WorksheetFunctions.ExcelArgPredicates.IsMissingArg(arg) Then Return defaultValue

            Dim s As String = BESHStatNG.WorksheetFunctions.ExcelArgReaders.AsString(arg)
            If String.IsNullOrWhiteSpace(s) Then Return defaultValue

            Select Case s.Trim().ToLowerInvariant()
                Case "quantile", "quantiles", "decile", "deciles"
                    Return "quantile"
                Case "equalwidth", "equal-width", "equal_width", "equal width"
                    Return "equalwidth"
                Case Else
                    Return defaultValue
            End Select
        End Function

        Public Function GetDefaultThresholds(p() As Double, Optional maxRows As Integer = 500,
                                             Optional includeEndpoints As Boolean = True) As Double()
            Return BinaryClassificationCore.GetDefaultThresholds(p, maxRows, includeEndpoints)
        End Function

        Public Function BuildPercentileCutpointBins(p() As Double,
                                                  Optional requestedBins As Integer = 10) As List(Of ProbabilityCutpointBin)
            Return BinaryClassificationCore.BuildPercentileCutpointBins(p, requestedBins)
        End Function

        Public Function WrapResults(summary As BinaryClassificationSummary,
                                    Optional thresholdRows As IList(Of BinaryThresholdRow) = Nothing,
                                    Optional calibrationRows As IList(Of CalibrationBinSummary) = Nothing,
                                    Optional brierScore As Double = Double.NaN,
                                    Optional eventRate As Double = Double.NaN,
                                    Optional analysisLabel As String = "Binary Classification") As List(Of ResultTable)
            Return BinaryClassificationCore.WrapResults(summary, thresholdRows, calibrationRows, brierScore, eventRate, analysisLabel)
        End Function


        ''' <summary>
        ''' Builds an Excel spill range for a binary confusion-matrix style crosstab.
        ''' </summary>
        Public Function BuildBinaryCrosstabUdfOutput(summary As BinaryClassificationSummary,
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
            out(rowOffset + 0, 3) = PercentOrMissingValue(summary.Specificity)

            out(rowOffset + 1, 0) = 1
            out(rowOffset + 1, 1) = summary.FN
            out(rowOffset + 1, 2) = summary.TP
            out(rowOffset + 1, 3) = PercentOrMissingValue(summary.Sensitivity)

            out(rowOffset + 2, 0) = "Precision % / Overall"
            out(rowOffset + 2, 1) = PercentOrMissingValue(summary.NPV)
            out(rowOffset + 2, 2) = PercentOrMissingValue(summary.Precision)
            out(rowOffset + 2, 3) = PercentOrMissingValue(summary.Accuracy)

            out(rowOffset + 3, 0) = "Threshold / Balanced accuracy"
            out(rowOffset + 3, 1) = summary.Threshold
            out(rowOffset + 3, 2) = PercentOrMissingValue(summary.BalancedAccuracy)
            out(rowOffset + 3, 3) = PercentOrMissingValue(summary.YoudenJ)

            Return PrepareResultTableForUdfLocal(out)
        End Function

        ''' <summary>
        ''' Builds an Excel spill range for threshold-performance rows.
        ''' </summary>
        Public Function BuildThresholdTableUdfOutput(rows As IList(Of BinaryThresholdRow),
                                                     Optional includeHeader As Boolean = True) As Object
            If rows Is Nothing OrElse rows.Count = 0 Then Return MissingOutputValue

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
                out(rr, 5) = PercentOrMissingValue(r.Sensitivity)
                out(rr, 6) = PercentOrMissingValue(r.Specificity)
                out(rr, 7) = PercentOrMissingValue(r.Precision)
                out(rr, 8) = PercentOrMissingValue(r.Recall)
                out(rr, 9) = PercentOrMissingValue(r.NPV)
                out(rr, 10) = PercentOrMissingValue(r.Accuracy)
                out(rr, 11) = PercentOrMissingValue(r.BalancedAccuracy)
                out(rr, 12) = PercentOrMissingValue(r.YoudenJ)
                out(rr, 13) = PercentOrMissingValue(r.F1)
            Next

            Return PrepareResultTableForUdfLocal(out)
        End Function

        ''' <summary>
        ''' Builds an Excel spill range for calibration-bin rows.
        ''' </summary>
        Public Function BuildCalibrationTableUdfOutput(rows As IList(Of CalibrationBinSummary),
                                                       Optional includeHeader As Boolean = True) As Object
            If rows Is Nothing OrElse rows.Count = 0 Then Return MissingOutputValue

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

            Return PrepareResultTableForUdfLocal(out)
        End Function

        ''' <summary>
        ''' Builds an Excel spill range for chart-ready calibration points.
        ''' </summary>
        Public Function BuildCalibrationPointsUdfOutput(rows As IList(Of CalibrationBinSummary)) As Object
            If rows Is Nothing OrElse rows.Count = 0 Then Return MissingOutputValue

            Dim out(rows.Count, 7) As Object
            out(0, 0) = "Bin"
            out(0, 1) = "N"
            out(0, 2) = "MeanPredicted"
            out(0, 3) = "ObservedRate"
            out(0, 4) = "LowerCI"
            out(0, 5) = "UpperCI"
            out(0, 6) = "ErrorMinus"
            out(0, 7) = "ErrorPlus"

            For i As Integer = 0 To rows.Count - 1
                Dim r As CalibrationBinSummary = rows(i)
                out(i + 1, 0) = r.BinIndex
                out(i + 1, 1) = r.N
                out(i + 1, 2) = r.MeanPredicted
                out(i + 1, 3) = r.ObservedRate
                out(i + 1, 4) = r.LowerCI
                out(i + 1, 5) = r.UpperCI
                out(i + 1, 6) = If(Double.IsNaN(r.LowerCI), MissingOutputValue, r.ObservedRate - r.LowerCI)
                out(i + 1, 7) = If(Double.IsNaN(r.UpperCI), MissingOutputValue, r.UpperCI - r.ObservedRate)
            Next

            Return PrepareResultTableForUdfLocal(out)
        End Function

        ''' <summary>
        ''' Builds an Excel spill range for the Brier score and optional supporting scalars.
        ''' </summary>
        Public Function BuildBrierScoreUdfOutput(score As Double,
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

            Return PrepareResultTableForUdfLocal(out)
        End Function

        ''' <summary>
        ''' Builds the two-group numeric arrays required by <see cref="graphics.ROC"/> from
        ''' binary outcomes and fitted probabilities.
        '''
        ''' Group layout expected by <see cref="graphics.ROC"/>:
        ''' <list type="bullet">
        '''   <item><description><c>x(0)</c> = positive / event group (<c>y = 1</c>)</description></item>
        '''   <item><description><c>x(1)</c> = negative / non-event group (<c>y = 0</c>)</description></item>
        ''' </list>
        '''
        ''' Each group contains the predicted probabilities for the corresponding observations.
        ''' The ROC class then interprets those probabilities as marker values.
        '''
        ''' <para>
        ''' Returns <c>Nothing</c> when the input does not contain at least one event and one
        ''' non-event observation, because an ROC curve cannot be computed in that case.
        ''' </para>
        ''' </summary>
        ''' <param name="y">Observed binary outcomes encoded as 0/1.</param>
        ''' <param name="p">Predicted probabilities on the interval [0,1].</param>
        ''' <returns>
        ''' A jagged array with two elements:
        ''' <list type="bullet">
        '''   <item><description><c>result(0)</c> = probabilities for observations with <c>y = 1</c></description></item>
        '''   <item><description><c>result(1)</c> = probabilities for observations with <c>y = 0</c></description></item>
        ''' </list>
        ''' or <c>Nothing</c> if an ROC curve cannot be constructed.
        ''' </returns>
        Public Function BuildRocInputFromBinaryProbabilities(y() As Double, p() As Double) As Double()()
            If y Is Nothing OrElse p Is Nothing Then Return Nothing
            If y.Length <> p.Length OrElse y.Length = 0 Then Return Nothing

            Dim positive As New List(Of Double)()
            Dim negative As New List(Of Double)()

            For i As Integer = 0 To y.Length - 1
                If y(i) >= 0.5R Then
                    positive.Add(p(i))
                Else
                    negative.Add(p(i))
                End If
            Next

            If positive.Count = 0 OrElse negative.Count = 0 Then Return Nothing

            Return New Double()() {positive.ToArray(), negative.ToArray()}
        End Function

        ''' <summary>
        ''' Computes an ROC analysis object from observed binary outcomes and fitted probabilities.
        ''' </summary>
        ''' <param name="y">Observed binary outcomes encoded as 0/1.</param>
        ''' <param name="p">Predicted probabilities on the interval [0,1].</param>
        ''' <param name="alphaValue">
        ''' Two-sided significance level used for the ROC confidence intervals and AUC test.
        ''' </param>
        ''' <returns>
        ''' A computed <see cref="graphics.ROC"/> instance, or <c>Nothing</c> if an ROC curve
        ''' cannot be constructed from the supplied data.
        ''' </returns>
        Private Function BuildRocFromBinaryProbabilities(y() As Double,
                                                 p() As Double,
                                                 alphaValue As Double) As graphics.ROC
            Dim rocInput As Double()() = BuildRocInputFromBinaryProbabilities(y, p)
            If rocInput Is Nothing Then Return Nothing

            Dim varNames() As String = {"Event (y = 1)", "Non-event (y = 0)"}
            Dim roc As New graphics.ROC(rocInput, varNames)
            roc.compute(alphaValue)
            Return roc
        End Function

        ''' <summary>
        ''' Computes an ROC curve from observed binary outcomes and fitted probabilities,
        ''' writes the tabular ROC results to the current classification worksheet, and
        ''' adds the ROC chart below the written tables.
        '''
        ''' The tabular results are taken directly from <see cref="graphics.ROC.wrapResults"/>
        ''' and therefore include:
        ''' <list type="bullet">
        '''   <item><description>overall ROC summary (AUC, confidence intervals, standard errors, and p-value)</description></item>
        '''   <item><description>cut-off table (cut-off, sensitivity, specificity)</description></item>
        ''' </list>
        '''
        ''' <para>
        ''' The ROC chart is then added to the same worksheet and positioned below the last
        ''' written ROC table, using the writer’s updated row pointer.
        ''' </para>
        ''' </summary>
        ''' <param name="writer">
        ''' <see cref="ExcelDnaResultWriter"/> instance already pointing at the classification worksheet.
        ''' The current row pointer is used as the insertion location for the ROC tables.
        ''' </param>
        ''' <param name="y">Observed binary outcomes encoded as 0/1.</param>
        ''' <param name="p">Predicted probabilities on the interval [0,1].</param>
        ''' <param name="alphaValue">
        ''' Two-sided significance level used for ROC confidence intervals and the AUC test.
        ''' The default is <c>0.05</c>.
        ''' </param>
        ''' <returns>
        ''' <c>True</c> if the ROC results were successfully computed and written; otherwise <c>False</c>.
        ''' </returns>
        Public Function AddRocResultsAndPlotToClassificationSheet(writer As ExcelDnaResultWriter, y() As Double, p() As Double,
                                                                  Optional alphaValue As Double = 0.05R) As Boolean
            If writer Is Nothing OrElse writer.ws Is Nothing Then Return False

            Dim roc As graphics.ROC = BuildRocFromBinaryProbabilities(y, p, alphaValue)
            If roc Is Nothing Then Return False

            Dim rocRes As List(Of ResultTable) = roc.wrapResults()
            If rocRes IsNot Nothing AndAlso rocRes.Count > 0 Then
                writer.shiftRowPointer() 'blank row separator before ROC output
                Dim rrRoc As New ProcessListofResultTables(rocRes)
                rrRoc.writeToSheet(writer, True)
            End If

            roc.addROCplot(writer.ws)

            Return True
        End Function

        Private Function PercentOrMissingValue(value As Double) As Object
            If Double.IsNaN(value) OrElse Double.IsInfinity(value) Then Return MissingOutputValue
            Return 100.0R * value
        End Function

        Private Function PrepareResultTableForUdfLocal(table As Object(,)) As Object(,)
            If table Is Nothing Then Return Nothing

            Dim rows As Integer = table.GetLength(0)
            Dim cols As Integer = table.GetLength(1)
            Dim out(rows - 1, cols - 1) As Object

            For r As Integer = 0 To rows - 1
                For c As Integer = 0 To cols - 1
                    Dim v As Object = table(r, c)

                    If v Is Nothing OrElse TypeOf v Is DBNull Then
                        out(r, c) = String.Empty
                    Else
                        out(r, c) = v
                    End If
                Next
            Next

            Return out
        End Function

    End Module
End Namespace
