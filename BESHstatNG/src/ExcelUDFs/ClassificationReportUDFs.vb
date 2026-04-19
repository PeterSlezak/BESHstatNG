Option Explicit On
Option Strict On

Imports System
Imports System.Collections.Generic
Imports System.Globalization
Imports ExcelDna.Integration

Namespace BESHStatNG.WorksheetFunctions

    ''' <summary>
    ''' Generic worksheet functions for threshold-based reporting of binary classifiers.
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' These worksheet functions score observed binary outcomes against supplied predicted probabilities without requiring
    ''' a stored model handle. They are intended for validation and reporting workflows in which probabilities may come
    ''' from fitted BESHStatNG models, from predictions spilled by <c>BESH.REGR.GLM_PRED</c> or <c>BESH.REGR.GEE_PRED</c>,
    ''' from external holdout datasets, or from models produced outside BESHStatNG.
    ''' </para>
    ''' <para>
    ''' The functions assume a binary outcome <c>y_i ∈ {0,1}</c> and estimated event probabilities <c>p_i ∈ [0,1]</c>.
    ''' At a chosen cutoff <c>c</c>, the predicted class is defined by <c>ŷ_i = 1</c> when <c>p_i ≥ c</c> and by
    ''' <c>ŷ_i = 0</c> otherwise. The returned summaries then report threshold-based measures such as the confusion matrix,
    ''' sensitivity, specificity, precision, negative predictive value, accuracy, balanced accuracy, and Youden's J.
    ''' </para>
    ''' <para>
    ''' Because these functions operate directly on worksheet ranges, they are useful for scoring holdout sets, external
    ''' validation samples, and manually assembled prediction tables. They complement the handle-based model reporting UDFs
    ''' and provide a clean long-term interface for classifier validation in Excel.
    ''' </para>
    ''' </remarks>
    Public Module ClassificationReportUDFs

        ''' <summary>
        ''' Returns a threshold-based confusion-matrix report for observed binary outcomes and predicted probabilities.
        ''' </summary>
        ''' <param name="y">
        ''' Observed binary outcomes coded as <c>0</c> and <c>1</c>.
        ''' Supply a single worksheet row or single worksheet column. A leading text header is allowed and is ignored.
        ''' </param>
        ''' <param name="probabilities">
        ''' Predicted event probabilities corresponding row-by-row to <paramref name="y"/>.
        ''' Supply a single worksheet row or single worksheet column. Values must lie in <c>[0,1]</c>.
        ''' A leading text header is allowed and is ignored.
        ''' </param>
        ''' <param name="threshold">
        ''' Optional single classification cutoff in <c>[0,1]</c>. The default is <c>0.5</c>.
        ''' The predicted class is defined by <c>ŷ_i = 1</c> when <c>p_i ≥ threshold</c>.
        ''' </param>
        ''' <param name="weights">
        ''' Optional nonnegative case weights aligned with <paramref name="y"/> and <paramref name="probabilities"/>.
        ''' Supply a single worksheet row or single worksheet column. When omitted, all observations receive unit weight.
        ''' </param>
        ''' <param name="includeHeader">
        ''' TRUE to include a descriptive header row in the returned table (default TRUE).
        ''' </param>
        ''' <returns>
        ''' A worksheet table containing the 2×2 confusion matrix and selected threshold-based summary measures.
        ''' The layout mirrors the classifier-report output used by the fitted-model UDFs so that generic and handle-based
        ''' reports can be compared directly.
        ''' </returns>
        ''' <remarks>
        ''' <para>
        ''' This function is useful when predicted probabilities are already available in the worksheet and only the reporting
        ''' layer is needed. Typical use cases include scoring a holdout set, evaluating external validation data, or reusing
        ''' probabilities generated earlier by <c>BESH.REGR.GLM_PRED</c> or <c>BESH.REGR.GEE_PRED</c>.
        ''' </para>
        ''' <para>
        ''' The report is threshold-dependent. Changing <paramref name="threshold"/> changes the predicted classes and therefore
        ''' the resulting confusion counts and derived measures. For a threshold sweep over many cutoffs, use
        ''' <c>BESH.CLASS.THRESH</c>.
        ''' </para>
        ''' <para>
        ''' Example:
        ''' <c>=BESH.CLASS.CONFUSION(A2:A101,B2:B101)</c>
        ''' or
        ''' <c>=BESH.CLASS.CONFUSION(A2:A101,B2:B101,0.35,C2:C101,TRUE)</c>
        ''' </para>
        ''' </remarks>
        <ExcelFunction(
            Name:="BESH.CLASS.CONFUSION",
            Category:="BESHStatNG - Regression Models",
            Description:="Returns a threshold-based confusion-matrix report for observed binary outcomes and predicted probabilities.",
            HelpTopic:=HelpLinks.FallbackBaseUrl & "/udf/regression-models/"
        )>
        Public Function CLASS_CONFUSION(
            <ExcelArgument(AllowReference:=True, Name:="y", Description:="Observed binary outcomes coded as 0/1, supplied as a single row or single column. A leading text header is allowed.")> y As Object,
            <ExcelArgument(AllowReference:=True, Name:="probabilities", Description:="Predicted event probabilities in [0,1], supplied as a single row or single column and aligned with y. A leading text header is allowed.")> probabilities As Object,
            <ExcelArgument(Name:="threshold", Description:="Optional single classification cutoff in [0,1]. Default = 0.5.")> Optional threshold As Object = Nothing,
            <ExcelArgument(AllowReference:=True, Name:="weights", Description:="Optional nonnegative case weights aligned with y and probabilities, supplied as a single row or single column.")> Optional weights As Object = Nothing,
            <ExcelArgument(Name:="includeHeader", Description:="TRUE to include a header row in the returned table. Default = TRUE.")> Optional includeHeader As Object = Nothing
        ) As Object
            Try
                Dim observed() As Double = Nothing
                If Not TryReadNumericVectorArgument(y, observed) Then Return ExcelError.ExcelErrorValue

                Dim probs() As Double = Nothing
                If Not TryReadNumericVectorArgument(probabilities, probs) Then Return ExcelError.ExcelErrorValue

                Dim cutoff As Double = 0.5R
                If Not TryGetSingleThresholdFromArg(threshold, cutoff, 0.5R) Then Return ExcelError.ExcelErrorValue

                Dim w() As Double = Nothing
                If Not TryReadOptionalNumericVectorArgument(weights, w) Then Return ExcelError.ExcelErrorValue

                Dim hdr As Boolean = UDFhelpers.GetOptionalBool(includeHeader, True)
                Dim summary As regression.BinaryClassificationSummary =
                    regression.BinaryClassificationReporting.ComputeBinarySummary(observed, probs, cutoff, w)

                Return UDFhelpers.BuildBinaryCrosstabOutput(summary, hdr)
            Catch ex As Exception
                Return LoggedUdfExceptionText("BESH.CLASS.CONFUSION", ex)
            End Try
        End Function

        ''' <summary>
        ''' Returns a threshold-performance table for observed binary outcomes and predicted probabilities.
        ''' </summary>
        ''' <param name="y">
        ''' Observed binary outcomes coded as <c>0</c> and <c>1</c>.
        ''' Supply a single worksheet row or single worksheet column. A leading text header is allowed and is ignored.
        ''' </param>
        ''' <param name="probabilities">
        ''' Predicted event probabilities corresponding row-by-row to <paramref name="y"/>.
        ''' Supply a single worksheet row or single worksheet column. Values must lie in <c>[0,1]</c>.
        ''' A leading text header is allowed and is ignored.
        ''' </param>
        ''' <param name="thresholds">
        ''' Optional scalar or vector of one or more thresholds in <c>[0,1]</c>.
        ''' Supply a single worksheet row, a single worksheet column, or a scalar.
        ''' If omitted, the default threshold grid is built from the sorted unique predicted probabilities.
        ''' </param>
        ''' <param name="weights">
        ''' Optional nonnegative case weights aligned with <paramref name="y"/> and <paramref name="probabilities"/>.
        ''' Supply a single worksheet row or single worksheet column. When omitted, all observations receive unit weight.
        ''' </param>
        ''' <param name="includeHeader">
        ''' TRUE to include a header row in the returned table (default TRUE).
        ''' </param>
        ''' <returns>
        ''' A worksheet table with one row per threshold and the columns:
        ''' threshold, TP, FP, TN, FN, sensitivity, specificity, precision, recall, NPV,
        ''' accuracy, balanced accuracy, Youden's J, and F1.
        ''' Percentage-based measures are returned on the 0–100 scale.
        ''' </returns>
        ''' <remarks>
        ''' <para>
        ''' This function evaluates classifier performance over many candidate decision thresholds.
        ''' It is useful for selecting an operating point after probabilities have been produced, for comparing cutoffs,
        ''' or for complementing ROC-based summaries with concrete confusion counts and predictive values.
        ''' </para>
        ''' <para>
        ''' When <paramref name="thresholds"/> is omitted, the function uses the sorted unique predicted probabilities.
        ''' This gives an exact threshold sweep over the observed fitted values rather than only a coarse fixed grid.
        ''' </para>
        ''' <para>
        ''' Example:
        ''' <c>=BESH.CLASS.THRESH(A2:A101,B2:B101)</c>
        ''' or
        ''' <c>=BESH.CLASS.THRESH(A2:A101,B2:B101,{0.1,0.2,0.3,0.4,0.5},,TRUE)</c>
        ''' </para>
        ''' </remarks>
        <ExcelFunction(
            Name:="BESH.CLASS.THRESH",
            Category:="BESHStatNG - Regression Models",
            Description:="Returns a threshold-performance table for observed binary outcomes and predicted probabilities.",
            HelpTopic:=HelpLinks.FallbackBaseUrl & "/udf/regression-models/"
        )>
        Public Function CLASS_THRESH(
            <ExcelArgument(AllowReference:=True, Name:="y", Description:="Observed binary outcomes coded as 0/1, supplied as a single row or single column. A leading text header is allowed.")> y As Object,
            <ExcelArgument(AllowReference:=True, Name:="probabilities", Description:="Predicted event probabilities in [0,1], supplied as a single row or single column and aligned with y. A leading text header is allowed.")> probabilities As Object,
            <ExcelArgument(Name:="thresholds", Description:="Optional scalar or row/column vector of thresholds in [0,1]. If omitted, the default threshold grid from the supplied probabilities is used.")> Optional thresholds As Object = Nothing,
            <ExcelArgument(AllowReference:=True, Name:="weights", Description:="Optional nonnegative case weights aligned with y and probabilities, supplied as a single row or single column.")> Optional weights As Object = Nothing,
            <ExcelArgument(Name:="includeHeader", Description:="TRUE to include a header row in the returned table. Default = TRUE.")> Optional includeHeader As Object = Nothing
        ) As Object
            Try
                Dim observed() As Double = Nothing
                If Not TryReadNumericVectorArgument(y, observed) Then Return ExcelError.ExcelErrorValue

                Dim probs() As Double = Nothing
                If Not TryReadNumericVectorArgument(probabilities, probs) Then Return ExcelError.ExcelErrorValue

                Dim thresholdVector() As Double = Nothing
                If Not UDFhelpers.TryGetOptionalThresholdVector(thresholds, thresholdVector) Then Return ExcelError.ExcelErrorValue

                Dim w() As Double = Nothing
                If Not TryReadOptionalNumericVectorArgument(weights, w) Then Return ExcelError.ExcelErrorValue

                Dim hdr As Boolean = UDFhelpers.GetOptionalBool(includeHeader, True)
                Dim rows As List(Of regression.BinaryThresholdRow) =
                    regression.BinaryClassificationReporting.BuildThresholdTable(observed, probs, thresholdVector, w)

                Return UDFhelpers.BuildThresholdTableOutput(rows, hdr)
            Catch ex As Exception
                Return LoggedUdfExceptionText("BESH.CLASS.THRESH", ex)
            End Try
        End Function

        ''' <summary>
        ''' Returns calibration-plot data for observed binary outcomes and predicted probabilities.
        ''' </summary>
        ''' <param name="y">
        ''' Observed binary outcomes coded as <c>0</c> and <c>1</c>.
        ''' Supply a single worksheet row or single worksheet column. A leading text header is allowed and is ignored.
        ''' </param>
        ''' <param name="probabilities">
        ''' Predicted event probabilities corresponding row-by-row to <paramref name="y"/>.
        ''' Supply a single worksheet row or single worksheet column. Values must lie in <c>[0,1]</c>.
        ''' A leading text header is allowed and is ignored.
        ''' </param>
        ''' <param name="bins">
        ''' Optional positive integer giving the number of calibration bins. The default is <c>10</c>.
        ''' The current implementation requires at least <c>2</c> bins.
        ''' </param>
        ''' <param name="method">
        ''' Optional calibration binning method.
        ''' Accepted values are <c>"quantile"</c> (default) and <c>"equalwidth"</c>.
        ''' Quantile binning creates groups with approximately equal numbers of observations, while equal-width binning
        ''' partitions the probability scale into equal intervals.
        ''' </param>
        ''' <param name="weights">
        ''' Optional nonnegative case weights aligned with <paramref name="y"/> and <paramref name="probabilities"/>.
        ''' Supply a single worksheet row or single worksheet column. When omitted, all observations receive unit weight.
        ''' </param>
        ''' <param name="includeHeader">
        ''' TRUE to include a header row in the returned table (default TRUE).
        ''' </param>
        ''' <returns>
        ''' A worksheet table with one row per calibration bin and the columns:
        ''' bin index, number of observations, mean predicted probability, observed event rate,
        ''' and lower/upper confidence limits for the observed event rate.
        ''' The probability-scale columns are returned on the 0–1 scale so they can be plotted directly in Excel.
        ''' </returns>
        ''' <remarks>
        ''' <para>
        ''' This function is intended to support calibration plots and simple calibration diagnostics after a set of event probabilities
        ''' has been produced. It groups observations by predicted probability and compares the mean predicted risk in each bin with the
        ''' empirical event rate observed in that bin.
        ''' </para>
        ''' <para>
        ''' The returned table can be plotted directly in Excel by using <c>MeanPredicted</c> on the x-axis and <c>ObservedRate</c>
        ''' on the y-axis, optionally with the confidence limits as error bars for the observed rate.
        ''' </para>
        ''' <para>
        ''' Example:
        ''' <c>=BESH.CLASS.CALIB(A2:A101,B2:B101)</c>
        ''' or
        ''' <c>=BESH.CLASS.CALIB(A2:A101,B2:B101,10,"quantile",,TRUE)</c>
        ''' </para>
        ''' </remarks>
        <ExcelFunction(
            Name:="BESH.CLASS.CALIB",
            Category:="BESHStatNG - Regression Models",
            Description:="Returns calibration-plot data for observed binary outcomes and predicted probabilities.",
            HelpTopic:=HelpLinks.FallbackBaseUrl & "/udf/regression-models/"
        )>
        Public Function CLASS_CALIB(
            <ExcelArgument(AllowReference:=True, Name:="y", Description:="Observed binary outcomes coded as 0/1, supplied as a single row or single column. A leading text header is allowed.")> y As Object,
            <ExcelArgument(AllowReference:=True, Name:="probabilities", Description:="Predicted event probabilities in [0,1], supplied as a single row or single column and aligned with y. A leading text header is allowed.")> probabilities As Object,
            <ExcelArgument(Name:="bins", Description:="Optional positive integer specifying the number of calibration bins. Default = 10.")> Optional bins As Object = Nothing,
            <ExcelArgument(Name:="method", Description:="Optional calibration binning method: 'quantile' (default) or 'equalwidth'.")> Optional method As Object = Nothing,
            <ExcelArgument(AllowReference:=True, Name:="weights", Description:="Optional nonnegative case weights aligned with y and probabilities, supplied as a single row or single column.")> Optional weights As Object = Nothing,
            <ExcelArgument(Name:="includeHeader", Description:="TRUE to include a header row in the returned table. Default = TRUE.")> Optional includeHeader As Object = Nothing
        ) As Object
            Try
                Dim observed() As Double = Nothing
                If Not TryReadNumericVectorArgument(y, observed) Then Return ExcelError.ExcelErrorValue

                Dim probs() As Double = Nothing
                If Not TryReadNumericVectorArgument(probabilities, probs) Then Return ExcelError.ExcelErrorValue

                Dim binCount As Integer = 10
                If Not UDFhelpers.TryGetOptionalPositiveInteger(bins, binCount, 10, 2) Then Return ExcelError.ExcelErrorValue

                Dim methodName As String = UDFhelpers.ParseCalibrationMethod(method, "quantile")

                Dim w() As Double = Nothing
                If Not TryReadOptionalNumericVectorArgument(weights, w) Then Return ExcelError.ExcelErrorValue

                Dim hdr As Boolean = UDFhelpers.GetOptionalBool(includeHeader, True)
                Dim rows As List(Of regression.CalibrationBinSummary) =
                    regression.BinaryClassificationReporting.BuildCalibrationBins(observed, probs, binCount, w, methodName)

                Return UDFhelpers.BuildCalibrationTableOutput(rows, hdr)
            Catch ex As Exception
                Return LoggedUdfExceptionText("BESH.CLASS.CALIB", ex)
            End Try
        End Function

        ''' <summary>
        ''' Returns the Brier score for observed binary outcomes and predicted probabilities.
        ''' </summary>
        ''' <param name="y">
        ''' Observed binary outcomes coded as <c>0</c> and <c>1</c>.
        ''' Supply a single worksheet row or single worksheet column. A leading text header is allowed and is ignored.
        ''' </param>
        ''' <param name="probabilities">
        ''' Predicted event probabilities corresponding row-by-row to <paramref name="y"/>.
        ''' Supply a single worksheet row or single worksheet column. Values must lie in <c>[0,1]</c>.
        ''' A leading text header is allowed and is ignored.
        ''' </param>
        ''' <param name="weights">
        ''' Optional nonnegative case weights aligned with <paramref name="y"/> and <paramref name="probabilities"/>.
        ''' Supply a single worksheet row or single worksheet column. When omitted, all observations receive unit weight.
        ''' </param>
        ''' <param name="includeHeader">
        ''' TRUE to include a header row in the returned table (default TRUE).
        ''' </param>
        ''' <returns>
        ''' A small worksheet table containing the Brier score and, for convenience, the sample size and event rate.
        ''' </returns>
        ''' <remarks>
        ''' <para>
        ''' For observed binary outcomes <c>y_i</c> and predicted probabilities <c>p_i</c>, the Brier score is the mean squared
        ''' probability error. In the unweighted case it is <c>(1/n) Σ (y_i - p_i)^2</c>; when observation weights are present,
        ''' the corresponding weighted mean is returned.
        ''' </para>
        ''' <para>
        ''' This summary is threshold-free and complements the threshold-based reports returned by
        ''' <c>BESH.CLASS.CONFUSION</c> and <c>BESH.CLASS.THRESH</c>.
        ''' </para>
        ''' <para>
        ''' Example:
        ''' <c>=BESH.CLASS.BRIER(A2:A101,B2:B101)</c>
        ''' or
        ''' <c>=BESH.CLASS.BRIER(A2:A101,B2:B101,C2:C101,TRUE)</c>
        ''' </para>
        ''' </remarks>
        <ExcelFunction(
            Name:="BESH.CLASS.BRIER",
            Category:="BESHStatNG - Regression Models",
            Description:="Returns the Brier score for observed binary outcomes and predicted probabilities.",
            HelpTopic:=HelpLinks.FallbackBaseUrl & "/udf/regression-models/"
        )>
        Public Function CLASS_BRIER(
            <ExcelArgument(AllowReference:=True, Name:="y", Description:="Observed binary outcomes coded as 0/1, supplied as a single row or single column. A leading text header is allowed.")> y As Object,
            <ExcelArgument(AllowReference:=True, Name:="probabilities", Description:="Predicted event probabilities in [0,1], supplied as a single row or single column and aligned with y. A leading text header is allowed.")> probabilities As Object,
            <ExcelArgument(AllowReference:=True, Name:="weights", Description:="Optional nonnegative case weights aligned with y and probabilities, supplied as a single row or single column.")> Optional weights As Object = Nothing,
            <ExcelArgument(Name:="includeHeader", Description:="TRUE to include a header row in the returned table. Default = TRUE.")> Optional includeHeader As Object = Nothing
        ) As Object
            Try
                Dim observed() As Double = Nothing
                If Not TryReadNumericVectorArgument(y, observed) Then Return ExcelError.ExcelErrorValue

                Dim probs() As Double = Nothing
                If Not TryReadNumericVectorArgument(probabilities, probs) Then Return ExcelError.ExcelErrorValue

                Dim w() As Double = Nothing
                If Not TryReadOptionalNumericVectorArgument(weights, w) Then Return ExcelError.ExcelErrorValue

                regression.BinaryClassificationReporting.ValidateBinaryInputs(observed, probs, w)

                Dim hdr As Boolean = UDFhelpers.GetOptionalBool(includeHeader, True)
                Dim brier As Double = regression.BinaryClassificationReporting.ComputeBrierScore(observed, probs, w)
                Dim eventRate As Double = ComputeWeightedEventRate(observed, w)
                Dim nObs As Double = If(w Is Nothing, observed.Length, w.Sum())

                Return UDFhelpers.BuildNamedScalarOutput("BrierScore", brier, nObs, eventRate, hdr)
            Catch ex As Exception
                Return LoggedUdfExceptionText("BESH.CLASS.BRIER", ex)
            End Try
        End Function

        Private Function TryGetSingleThresholdFromArg(arg As Object, ByRef threshold As Double,
                                                      Optional defaultValue As Double = 0.5R) As Boolean
            threshold = defaultValue
            If UDFhelpers.IsMissingArg(arg) Then Return True

            Dim d As Double? = UDFhelpers.TryGetDouble(arg)
            If Not d.HasValue Then Return False
            If d.Value < 0.0R OrElse d.Value > 1.0R Then Return False

            threshold = d.Value
            Return True
        End Function

        Private Function TryReadOptionalNumericVectorArgument(arg As Object,
                                                              ByRef values() As Double) As Boolean
            values = Nothing
            If UDFhelpers.IsMissingArg(arg) Then Return True
            Return TryReadNumericVectorArgument(arg, values)
        End Function

        Private Function TryReadNumericVectorArgument(arg As Object, ByRef values() As Double) As Boolean
            values = Nothing

            If UDFhelpers.IsMissingArg(arg) Then Return False

            Dim scalar As Double? = UDFhelpers.TryGetDouble(arg)
            If scalar.HasValue Then
                ReDim values(0)
                values(0) = scalar.Value
                Return True
            End If

            Dim arr As Object(,) = UDFhelpers.Get2D(arg)
            If arr Is Nothing Then Return False

            Dim rows As Integer = arr.GetLength(0)
            Dim cols As Integer = arr.GetLength(1)
            If rows < 1 OrElse cols < 1 Then Return False
            If rows > 1 AndAlso cols > 1 Then Return False

            If rows = 1 Then
                Return TryReadNumericVectorFromSingleRow(arr, values)
            Else
                Return TryReadNumericVectorFromSingleColumn(arr, values)
            End If
        End Function

        Private Function TryReadNumericVectorFromSingleColumn(arr As Object(,), ByRef values() As Double) As Boolean
            values = Nothing
            Dim rows As Integer = arr.GetLength(0)
            If rows < 1 Then Return False

            Dim last As Integer = rows - 1
            While last >= 0 AndAlso UDFhelpers.IsBlankCell(arr(last, 0))
                last -= 1
            End While
            If last < 0 Then Return False

            Dim start As Integer = 0
            If Not UDFhelpers.TryGetDouble(arr(0, 0)).HasValue Then
                If last = 0 Then Return False
                start = 1
            End If
            If start > last Then Return False

            Dim out As New List(Of Double)
            For i = start To last
                If UDFhelpers.IsBlankCell(arr(i, 0)) Then Return False
                Dim d As Double? = UDFhelpers.TryGetDouble(arr(i, 0))
                If Not d.HasValue Then Return False
                out.Add(d.Value)
            Next

            If out.Count = 0 Then Return False
            values = out.ToArray()
            Return True
        End Function

        Private Function TryReadNumericVectorFromSingleRow(arr As Object(,), ByRef values() As Double) As Boolean
            values = Nothing
            Dim cols As Integer = arr.GetLength(1)
            If cols < 1 Then Return False

            Dim last As Integer = cols - 1
            While last >= 0 AndAlso UDFhelpers.IsBlankCell(arr(0, last))
                last -= 1
            End While
            If last < 0 Then Return False

            Dim start As Integer = 0
            If Not UDFhelpers.TryGetDouble(arr(0, 0)).HasValue Then
                If last = 0 Then Return False
                start = 1
            End If
            If start > last Then Return False

            Dim out As New List(Of Double)
            For j = start To last
                If UDFhelpers.IsBlankCell(arr(0, j)) Then Return False
                Dim d As Double? = UDFhelpers.TryGetDouble(arr(0, j))
                If Not d.HasValue Then Return False
                out.Add(d.Value)
            Next

            If out.Count = 0 Then Return False
            values = out.ToArray()
            Return True
        End Function

        Friend Function ComputeWeightedEventRate(y() As Double, weights() As Double) As Double
            If y Is Nothing OrElse y.Length = 0 Then Return Double.NaN
            Dim sumY As Double = 0.0

            If weights Is Nothing Then
                For i = 0 To y.Length - 1
                    sumY += If(y(i) >= 0.5, 1.0, 0.0)
                Next
                Return sumY / y.Length
            End If

            Dim sumW As Double = 0.0
            sumY = 0.0
            For i = 0 To y.Length - 1
                sumW += weights(i)
                sumY += weights(i) * If(y(i) >= 0.5, 1.0, 0.0)
            Next

            If sumW <= 0.0 Then Return Double.NaN
            Return sumY / sumW
        End Function

    End Module
End Namespace
