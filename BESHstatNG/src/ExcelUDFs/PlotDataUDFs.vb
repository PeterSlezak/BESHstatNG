Option Explicit On
Option Strict On

Imports System
Imports System.Collections
Imports System.Collections.Generic
Imports System.Globalization
Imports System.Linq
Imports ExcelDna.Integration

Namespace BESHStatNG.WorksheetFunctions

    ''' <summary>
    ''' Worksheet functions that return chart-ready tables for common statistical plots.
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' These functions are designed for worksheet users who want to build Excel charts directly from
    ''' UDF spill ranges rather than using the GUI chart writers. Each function returns a labeled 2D table
    ''' that can be used as an input to an Excel chart, PivotChart, or further worksheet formulas.
    ''' </para>
    ''' <para>
    ''' The current module focuses on three common plot-data families:
    ''' </para>
    ''' <list type="bullet">
    '''   <item><description>Histogram bins and optional normal overlays.</description></item>
    '''   <item><description>ROC curve points together with a numerical ROC summary table.</description></item>
    '''   <item><description>Kaplan–Meier survival-curve step coordinates.</description></item>
    ''' </list>
    ''' <para>
    ''' The functions intentionally reuse the shared UDF input helpers so that header detection, row trimming,
    ''' missing-value handling, and text/number coercion stay consistent with the rest of the add-in.
    ''' </para>
    ''' </remarks>
    Public Module PlotDataUDFs

        ''' <summary>
        ''' Returns histogram bin midpoints and frequencies for one or more numeric input columns.
        ''' </summary>
        ''' <param name="data">
        ''' One numeric column or a multi-column numeric range. When multiple columns are supplied, each column is
        ''' treated as a separate series. The first row may contain headers, which are used as group names.
        ''' </param>
        ''' <param name="binRule">
        ''' Optional binning rule. Accepted values include <c>sturges</c>, <c>doane</c>, <c>scott</c>, and
        ''' <c>freedman-diaconis</c>. The default is <c>sturges</c>.
        ''' </param>
        ''' <returns>
        ''' A spill range with columns:
        ''' <list type="bullet">
        '''   <item><description><c>Group</c> — source column or series name.</description></item>
        '''   <item><description><c>BinMidpoint</c> — midpoint of the bin.</description></item>
        '''   <item><description><c>Frequency</c> — number of observations falling in the bin.</description></item>
        ''' </list>
        ''' </returns>
        ''' <remarks>
        ''' <para>
        ''' This function is useful when you want to build a histogram with native Excel charts while keeping the binning
        ''' logic aligned with the GUI histogram feature. The returned table is already in long format, which makes it easy
        ''' to filter by group or build separate series from the same spill range.
        ''' </para>
        ''' <para>
        ''' Each input column is processed independently using the requested automatic bin-width rule.
        ''' </para>
        ''' </remarks>
        <ExcelFunction(
            Name:="BESH.PLOT.HIST_BINS",
            Category:="BESHStatNG - Plot Data",
            Description:="Histogram bin midpoints and frequencies for one or more numeric series.",
            HelpTopic:=HelpLinks.BaseUrlRoot & "/latest/udf/plot-data/")>
        Public Function HIST_BINS(
            <ExcelArgument(AllowReference:=True, Name:="data", Description:="One numeric column or a multi-column numeric range. First row may be headers.")> data As Object,
            <ExcelArgument(Name:="binRule", Description:="Optional rule: sturges | doane | scott | freedman-diaconis.")> Optional binRule As Object = Nothing
            ) As Object
            Try
                Dim groups()() As Double = Nothing
                Dim names() As String = Nothing
                If Not UDFhelpers.TryReadGroupedNumericColumns(data, groups, names) Then Return ExcelError.ExcelErrorValue
                If groups Is Nothing OrElse names Is Nothing Then Return ExcelError.ExcelErrorNum

                Dim resolvedRule As String = ResolveHistogramRule(binRule)
                Dim parts As New List(Of Object(,))()

                For i As Integer = 0 To groups.Length - 1
                    Dim values() As Double = groups(i)
                    If values Is Nothing OrElse values.Length < 1 Then Continue For

                    Dim bins As Object(,) = graphics.HistogramBinsComputation(values, resolvedRule)
                    If bins Is Nothing OrElse bins.GetLength(0) < 1 Then Continue For

                    Dim part(bins.GetLength(0), 2) As Object
                    part(0, 0) = "Group"
                    part(0, 1) = "BinMidpoint"
                    part(0, 2) = "Frequency"
                    For r As Integer = 0 To bins.GetLength(0) - 1
                        part(r + 1, 0) = names(i)
                        part(r + 1, 1) = bins(r, 0)
                        part(r + 1, 2) = bins(r, 1)
                    Next
                    parts.Add(part)
                Next

                If parts.Count = 0 Then Return ExcelError.ExcelErrorNum
                Return StackHomogeneousTables(parts)
            Catch ex As Exception
                Return LoggedUdfError("BESH.PLOT.HIST_BINS", ex, ExcelError.ExcelErrorValue)
            End Try
        End Function

        ''' <summary>
        ''' Returns the coordinates of the normal overlay curve corresponding to the GUI histogram overlay.
        ''' </summary>
        ''' <param name="data">
        ''' One numeric column or a multi-column numeric range. Each column is treated as a separate series.
        ''' The first row may contain headers, which are used as group names.
        ''' </param>
        ''' <param name="binRule">
        ''' Optional automatic binning rule used to determine the effective bin width for scaling the overlay to histogram frequency units.
        ''' Accepted values include <c>sturges</c>, <c>doane</c>, <c>scott</c>, and <c>freedman-diaconis</c>.
        ''' </param>
        ''' <returns>
        ''' A spill range with columns:
        ''' <list type="bullet">
        '''   <item><description><c>Group</c> — source column or series name.</description></item>
        '''   <item><description><c>X</c> — x-coordinate of the overlay curve.</description></item>
        '''   <item><description><c>NormalFrequency</c> — expected histogram-scale frequency for the fitted normal curve.</description></item>
        ''' </list>
        ''' </returns>
        ''' <remarks>
        ''' <para>
        ''' The overlay is not a probability density on a unit area scale. Instead, it is scaled to the histogram frequency axis,
        ''' matching the GUI histogram overlay so that the curve can be drawn directly on top of the histogram bars.
        ''' </para>
        ''' <para>
        ''' The fitted curve uses the same quartile-based center and spread approximation as the GUI histogram overlay.
        ''' </para>
        ''' </remarks>
        <ExcelFunction(
            Name:="BESH.PLOT.HIST_NORMAL",
            Category:="BESHStatNG - Plot Data",
            Description:="Normal-overlay coordinates matched to the GUI histogram frequency scale.",
            HelpTopic:=HelpLinks.BaseUrlRoot & "/latest/udf/plot-data/")>
        Public Function HIST_NORMAL(
            <ExcelArgument(AllowReference:=True, Name:="data", Description:="One numeric column or a multi-column numeric range. First row may be headers.")> data As Object,
            <ExcelArgument(Name:="binRule", Description:="Optional rule: sturges | doane | scott | freedman-diaconis.")> Optional binRule As Object = Nothing
            ) As Object
            Try
                Dim groups()() As Double = Nothing
                Dim names() As String = Nothing
                If Not UDFhelpers.TryReadGroupedNumericColumns(data, groups, names) Then Return ExcelError.ExcelErrorValue
                If groups Is Nothing OrElse names Is Nothing Then Return ExcelError.ExcelErrorNum

                Dim resolvedRule As String = ResolveHistogramRule(binRule)
                Dim parts As New List(Of Object(,))()

                For i As Integer = 0 To groups.Length - 1
                    Dim values() As Double = groups(i)
                    If values Is Nothing OrElse values.Length < 2 Then Continue For

                    Dim bins As Object(,) = graphics.HistogramBinsComputation(values, resolvedRule)
                    If bins Is Nothing OrElse bins.GetLength(0) < 1 Then Continue For

                    Dim mids(bins.GetLength(0) - 1) As Double
                    For r As Integer = 0 To bins.GetLength(0) - 1
                        mids(r) = Convert.ToDouble(bins(r, 0), CultureInfo.InvariantCulture)
                    Next

                    Dim overlay As Double(,) = graphics.GaussOverlayComputation(values, mids)
                    If overlay Is Nothing OrElse overlay.GetLength(0) < 1 Then Continue For

                    Dim part(overlay.GetLength(0), 2) As Object
                    part(0, 0) = "Group"
                    part(0, 1) = "X"
                    part(0, 2) = "NormalFrequency"
                    For r As Integer = 0 To overlay.GetLength(0) - 1
                        part(r + 1, 0) = names(i)
                        part(r + 1, 1) = overlay(r, 0)
                        part(r + 1, 2) = overlay(r, 1)
                    Next
                    parts.Add(part)
                Next

                If parts.Count = 0 Then Return ExcelError.ExcelErrorNum
                Return StackHomogeneousTables(parts)
            Catch ex As Exception
                Return LoggedUdfError("BESH.PLOT.HIST_NORMAL", ex, ExcelError.ExcelErrorValue)
            End Try
        End Function

        ''' <summary>
        ''' Returns chart-ready ROC curve coordinates from a marker column and a binary status column.
        ''' </summary>
        ''' <param name="marker">
        ''' Single-column numeric marker values. Higher values are assumed to indicate the positive class unless
        ''' <paramref name="direction"/> is set to <c>lower</c>.
        ''' </param>
        ''' <param name="status">
        ''' Single-column class labels aligned row-by-row with <paramref name="marker"/>. The column may contain 0/1 values,
        ''' logical binary labels, or any two-category text labels.
        ''' </param>
        ''' <param name="positiveClass">
        ''' Optional label identifying the positive class. When omitted, the function automatically uses 1 for binary 0/1 inputs.
        ''' For text labels, supplying this argument is recommended to avoid ambiguity.
        ''' </param>
        ''' <param name="direction">
        ''' Optional direction flag:
        ''' <list type="bullet">
        '''   <item><description><c>higher</c> — larger marker values indicate the positive class (default).</description></item>
        '''   <item><description><c>lower</c> — smaller marker values indicate the positive class.</description></item>
        ''' </list>
        ''' </param>
        ''' <param name="alpha">
        ''' Optional two-sided significance level used internally for the stored ROC confidence intervals.
        ''' The default is 0.05, corresponding to a 95% confidence interval.
        ''' </param>
        ''' <returns>
        ''' A spill range with columns:
        ''' <list type="bullet">
        '''   <item><description><c>Threshold</c> — decision threshold on the original marker scale.</description></item>
        '''   <item><description><c>Sensitivity</c> — true-positive rate.</description></item>
        '''   <item><description><c>Specificity</c> — true-negative rate.</description></item>
        '''   <item><description><c>FalsePositiveRate</c> — 1 − specificity, the usual ROC x-axis.</description></item>
        '''   <item><description><c>TruePositiveRate</c> — sensitivity, the usual ROC y-axis.</description></item>
        ''' </list>
        ''' </returns>
        ''' <remarks>
        ''' <para>
        ''' The output includes the two ROC end points in addition to the empirical cut-offs computed by the underlying ROC engine.
        ''' This means the returned table can be plotted directly as an XY scatter with straight lines.
        ''' </para>
        ''' <para>
        ''' When smaller marker values indicate the positive class, set <paramref name="direction"/> to <c>lower</c>.
        ''' Internally the function reverses the marker scale, fits the ROC, and then converts thresholds back to the original scale.
        ''' </para>
        ''' </remarks>
        <ExcelFunction(
            Name:="BESH.PLOT.ROC_POINTS",
            Category:="BESHStatNG - Plot Data",
            Description:="ROC thresholds and curve coordinates (false-positive rate vs. true-positive rate).",
            HelpTopic:=HelpLinks.BaseUrlRoot & "/latest/udf/plot-data/")>
        Public Function ROC_POINTS(
            <ExcelArgument(AllowReference:=True, Name:="marker", Description:="Single-column numeric marker values. First cell may be a header.")> marker As Object,
            <ExcelArgument(AllowReference:=True, Name:="status", Description:="Single-column binary status labels aligned row-wise with marker. First cell may be a header.")> status As Object,
            <ExcelArgument(Name:="positiveClass", Description:="Optional label identifying the positive class.")> Optional positiveClass As Object = Nothing,
            <ExcelArgument(Name:="direction", Description:="Optional direction: higher | lower. Default higher.")> Optional direction As Object = Nothing,
            <ExcelArgument(Name:="alpha", Description:="Optional two-sided alpha for the stored ROC confidence intervals. Default 0.05.")> Optional alpha As Object = Nothing
            ) As Object
            Try
                Dim alphaValue As Double = 0.05R
                If Not TryParseAlpha(alpha, alphaValue) Then Return ExcelError.ExcelErrorNum

                Dim rocInput()() As Double = Nothing
                Dim markerName As String = Nothing
                Dim positiveLabel As String = Nothing
                Dim isLowerDirection As Boolean = False
                If Not TryBuildRocInput(marker, status, positiveClass, direction, rocInput, markerName, positiveLabel, isLowerDirection) Then
                    Return ExcelError.ExcelErrorValue
                End If

                Dim mdl As New graphics.ROC(rocInput, New String() {If(String.IsNullOrWhiteSpace(markerName), "Marker", markerName)})
                mdl.compute(alphaValue)
                Dim tables As List(Of ResultTable) = mdl.wrapResults()
                If tables Is Nothing OrElse tables.Count < 2 Then Return ExcelError.ExcelErrorNA

                Dim cutoffTable As Object(,) = tables(1).returnSelf()
                Return BuildRocPointsTable(cutoffTable, isLowerDirection)
            Catch ex As Exception
                Return LoggedUdfError("BESH.PLOT.ROC_POINTS", ex, ExcelError.ExcelErrorValue)
            End Try
        End Function

        ''' <summary>
        ''' Returns a numerical ROC summary table containing AUC, standard errors, confidence intervals, and the test against AUC = 0.5.
        ''' </summary>
        ''' <param name="marker">
        ''' Single-column numeric marker values. Higher values are assumed to indicate the positive class unless
        ''' <paramref name="direction"/> is set to <c>lower</c>.
        ''' </param>
        ''' <param name="status">
        ''' Single-column class labels aligned row-by-row with <paramref name="marker"/>.
        ''' </param>
        ''' <param name="positiveClass">
        ''' Optional label identifying the positive class. For text labels, supplying this argument is recommended.
        ''' </param>
        ''' <param name="direction">
        ''' Optional direction flag: <c>higher</c> (default) or <c>lower</c>.
        ''' </param>
        ''' <param name="alpha">
        ''' Optional two-sided significance level used for the DeLong and Hanley–McNeil confidence intervals.
        ''' </param>
        ''' <returns>
        ''' A two-column spill range with metric names and values, mirroring the numerical summary shown by the GUI ROC output.
        ''' </returns>
        ''' <remarks>
        ''' <para>
        ''' The returned table includes the Wilcoxon AUC estimate, DeLong and Hanley–McNeil confidence intervals,
        ''' the corresponding standard errors, and the two-sided p-value for the null hypothesis that AUC = 0.5.
        ''' </para>
        ''' <para>
        ''' Use this function when you need the numeric ROC summary for reporting while <c>BESH.PLOT.ROC_POINTS</c>
        ''' supplies the chart coordinates for plotting.
        ''' </para>
        ''' </remarks>
        <ExcelFunction(
            Name:="BESH.PLOT.ROC_STATS",
            Category:="BESHStatNG - Plot Data",
            Description:="Numerical ROC summary: AUC, standard errors, confidence intervals, and p-value.",
            HelpTopic:=HelpLinks.BaseUrlRoot & "/latest/udf/plot-data/")>
        Public Function ROC_STATS(
            <ExcelArgument(AllowReference:=True, Name:="marker", Description:="Single-column numeric marker values. First cell may be a header.")> marker As Object,
            <ExcelArgument(AllowReference:=True, Name:="status", Description:="Single-column binary status labels aligned row-wise with marker. First cell may be a header.")> status As Object,
            <ExcelArgument(Name:="positiveClass", Description:="Optional label identifying the positive class.")> Optional positiveClass As Object = Nothing,
            <ExcelArgument(Name:="direction", Description:="Optional direction: higher | lower. Default higher.")> Optional direction As Object = Nothing,
            <ExcelArgument(Name:="alpha", Description:="Optional two-sided alpha for the ROC confidence intervals. Default 0.05.")> Optional alpha As Object = Nothing
            ) As Object
            Try
                Dim alphaValue As Double = 0.05R
                If Not TryParseAlpha(alpha, alphaValue) Then Return ExcelError.ExcelErrorNum

                Dim rocInput()() As Double = Nothing
                Dim markerName As String = Nothing
                Dim positiveLabel As String = Nothing
                Dim isLowerDirection As Boolean = False
                If Not TryBuildRocInput(marker, status, positiveClass, direction, rocInput, markerName, positiveLabel, isLowerDirection) Then
                    Return ExcelError.ExcelErrorValue
                End If

                Dim mdl As New graphics.ROC(rocInput, New String() {If(String.IsNullOrWhiteSpace(markerName), "Marker", markerName)})
                mdl.compute(alphaValue)
                Dim tables As List(Of ResultTable) = mdl.wrapResults()
                If tables Is Nothing OrElse tables.Count < 1 Then Return ExcelError.ExcelErrorNA

                Return BuildRocStatsTable(tables(0).returnSelf())
            Catch ex As Exception
                Return LoggedUdfError("BESH.PLOT.ROC_STATS", ex, ExcelError.ExcelErrorValue)
            End Try
        End Function

        ''' <summary>
        ''' Returns step-ready Kaplan–Meier survival-curve coordinates for one or more groups.
        ''' </summary>
        ''' <param name="time">
        ''' Single-column follow-up times. Values must be non-negative and aligned row-by-row with <paramref name="status"/>.
        ''' The first cell may be a header.
        ''' </param>
        ''' <param name="status">
        ''' Single-column event indicator containing 1 for event and 0 for censoring. The first cell may be a header.
        ''' </param>
        ''' <param name="group">
        ''' Optional single-column group labels. When omitted, all observations are treated as one group.
        ''' </param>
        ''' <param name="alpha">
        ''' Optional two-sided significance level used for the confidence limits. The default is 0.05.
        ''' </param>
        ''' <returns>
        ''' A spill range with columns:
        ''' <list type="bullet">
        '''   <item><description><c>Group</c> — group label.</description></item>
        '''   <item><description><c>PlotOrder</c> — plotting order within the group.</description></item>
        '''   <item><description><c>Time</c> — x-coordinate for the step curve.</description></item>
        '''   <item><description><c>Survival</c> — Kaplan–Meier survival probability.</description></item>
        '''   <item><description><c>LowerCI</c> — lower confidence limit.</description></item>
        '''   <item><description><c>UpperCI</c> — upper confidence limit.</description></item>
        '''   <item><description><c>AtRisk</c> — number at risk reported on the drop point rows; blank on horizontal connector rows.</description></item>
        ''' </list>
        ''' </returns>
        ''' <remarks>
        ''' <para>
        ''' The function converts the tabular Kaplan–Meier output into a step-ready representation by duplicating each event/censor time:
        ''' one row continues the previous plateau and the next row moves vertically to the updated survival level.
        ''' This shape can be plotted directly as an XY scatter with straight lines.
        ''' </para>
        ''' <para>
        ''' The output is intended for plotting rather than formal tabular reporting. For a compact tabular summary at each observed time,
        ''' the existing <c>BESH.SURV.KM_TABLE</c> UDF remains the better choice.
        ''' </para>
        ''' </remarks>
        <ExcelFunction(
            Name:="BESH.PLOT.KM_CURVE",
            Category:="BESHStatNG - Plot Data",
            Description:="Step-ready Kaplan-Meier survival-curve coordinates with optional confidence limits.",
            HelpTopic:=HelpLinks.BaseUrlRoot & "/latest/udf/plot-data/")>
        Public Function KM_CURVE(
            <ExcelArgument(AllowReference:=True, Name:="time", Description:="Single-column follow-up times (>=0). First cell may be a header.")> time As Object,
            <ExcelArgument(AllowReference:=True, Name:="status", Description:="Single-column status values: 1=event, 0=censored. First cell may be a header.")> status As Object,
            <ExcelArgument(AllowReference:=True, Name:="group", Description:="Optional single-column group labels aligned row-wise with time and status.")> Optional group As Object = Nothing,
            <ExcelArgument(Name:="alpha", Description:="Optional two-sided alpha for the confidence limits. Default 0.05.")> Optional alpha As Object = Nothing
            ) As Object
            Try
                Dim alphaValue As Double = 0.05R
                If Not TryParseAlpha(alpha, alphaValue) Then Return ExcelError.ExcelErrorNum

                Dim records As List(Of survival.SurvivalRecord) = Nothing
                If Not TryBuildSurvivalRecords(time, status, group, records) Then Return ExcelError.ExcelErrorValue
                If records Is Nothing OrElse records.Count = 0 Then Return ExcelError.ExcelErrorNum

                Dim km As New survival.Survival_KM_LR(records)
                Dim rawCurves As Object() = km.SurvivalCurveTabularOutput(alphaValue)
                If rawCurves Is Nothing OrElse rawCurves.Length = 0 Then Return ExcelError.ExcelErrorNA

                Return BuildKaplanMeierStepTable(rawCurves)
            Catch ex As Exception
                Return LoggedUdfError("BESH.PLOT.KM_CURVE", ex, ExcelError.ExcelErrorValue)
            End Try
        End Function

        ''' <summary>
        ''' Returns chart-ready calibration-bin coordinates for a binary classifier or probabilistic model.
        ''' </summary>
        ''' <param name="y">
        ''' Single-column observed binary outcomes encoded as 0/1. The first cell may be a header.
        ''' </param>
        ''' <param name="probabilities">
        ''' Single-column predicted probabilities aligned row-by-row with <paramref name="y"/>.
        ''' Values must lie in the closed interval [0,1]. The first cell may be a header.
        ''' </param>
        ''' <param name="bins">
        ''' Optional positive integer specifying the number of calibration bins. The default is 10.
        ''' </param>
        ''' <param name="method">
        ''' Optional calibration-binning method. Supported values are <c>quantile</c> (default) and <c>equalwidth</c>.
        ''' </param>
        ''' <param name="weights">
        ''' Optional single-column nonnegative observation weights aligned row-by-row with <paramref name="y"/>.
        ''' When omitted, all observations receive weight 1.
        ''' </param>
        ''' <returns>
        ''' A spill range with columns:
        ''' <list type="bullet">
        '''   <item><description><c>Bin</c> — bin index starting at 1.</description></item>
        '''   <item><description><c>N</c> — (possibly weighted) bin size.</description></item>
        '''   <item><description><c>MeanPredicted</c> — mean predicted probability in the bin.</description></item>
        '''   <item><description><c>ObservedRate</c> — empirical event rate in the bin.</description></item>
        '''   <item><description><c>LowerCI</c> — lower confidence limit for the observed rate.</description></item>
        '''   <item><description><c>UpperCI</c> — upper confidence limit for the observed rate.</description></item>
        '''   <item><description><c>ErrorMinus</c> — distance from the point to the lower confidence limit.</description></item>
        '''   <item><description><c>ErrorPlus</c> — distance from the point to the upper confidence limit.</description></item>
        ''' </list>
        ''' </returns>
        ''' <remarks>
        ''' <para>
        ''' This function is the plot-data counterpart of the classifier calibration reporting functions.
        ''' It is intended for users who want to build a native Excel scatter plot directly from a worksheet
        ''' spill range instead of using the GUI chart writer.
        ''' </para>
        ''' <para>
        ''' The returned table can be plotted with:
        ''' </para>
        ''' <list type="bullet">
        '''   <item><description>x-axis = <c>MeanPredicted</c></description></item>
        '''   <item><description>y-axis = <c>ObservedRate</c></description></item>
        '''   <item><description>optional custom error bars from <c>ErrorMinus</c> and <c>ErrorPlus</c></description></item>
        ''' </list>
        ''' <para>
        ''' For a GUI-generated chart inside a workbook, pair these data with the new
        ''' <c>graphics.CalibrationPlot</c> class.
        ''' </para>
        ''' </remarks>
        <ExcelFunction(
            Name:="BESH.PLOT.CALIB_POINTS",
            Category:="BESHStatNG - Plot Data",
            Description:="Calibration-bin points for plotting observed event rate vs. mean predicted probability.",
            HelpTopic:=HelpLinks.BaseUrlRoot & "/latest/udf/plot-data/")>
        Public Function CALIB_POINTS(
            <ExcelArgument(AllowReference:=True, Name:="y", Description:="Single-column observed binary outcomes (0/1). First cell may be a header.")> y As Object,
            <ExcelArgument(AllowReference:=True, Name:="probabilities", Description:="Single-column predicted probabilities aligned with y. First cell may be a header.")> probabilities As Object,
            <ExcelArgument(Name:="bins", Description:="Optional positive integer number of calibration bins. Default 10.")> Optional bins As Object = Nothing,
            <ExcelArgument(Name:="method", Description:="Optional binning method: quantile | equalwidth. Default quantile.")> Optional method As Object = Nothing,
            <ExcelArgument(AllowReference:=True, Name:="weights", Description:="Optional single-column nonnegative observation weights aligned with y.")> Optional weights As Object = Nothing
            ) As Object
            Try
                Dim yVals As List(Of Double) = Nothing
                Dim pVals As List(Of Double) = Nothing
                Dim wVals As List(Of Double) = Nothing

                If Not UDFhelpers.TryReadNumericColumn(y, yVals) Then Return ExcelError.ExcelErrorValue
                If Not UDFhelpers.TryReadNumericColumn(probabilities, pVals) Then Return ExcelError.ExcelErrorValue
                If yVals Is Nothing OrElse pVals Is Nothing Then Return ExcelError.ExcelErrorValue
                If yVals.Count <> pVals.Count OrElse yVals.Count = 0 Then Return ExcelError.ExcelErrorNum

                Dim w() As Double = Nothing
                If Not UDFhelpers.IsMissingArg(weights) Then
                    If Not UDFhelpers.TryReadNumericColumn(weights, wVals) Then Return ExcelError.ExcelErrorValue
                    If wVals Is Nothing OrElse wVals.Count <> yVals.Count Then Return ExcelError.ExcelErrorNum
                    w = wVals.ToArray()
                End If

                Dim binCount As Integer
                If Not UDFhelpers.TryGetOptionalPositiveInteger(bins, binCount, 10, 1) Then Return ExcelError.ExcelErrorNum

                Dim methodName As String = UDFhelpers.ParseCalibrationMethod(method, "quantile")
                Dim yy() As Double = yVals.ToArray()
                Dim pp() As Double = pVals.ToArray()

                regression.BinaryClassificationReporting.ValidateBinaryInputs(yy, pp, w)
                Dim rows As List(Of regression.CalibrationBinSummary) = regression.BinaryClassificationReporting.BuildCalibrationBins(yy, pp, binCount, w, methodName)
                If rows Is Nothing OrElse rows.Count = 0 Then Return ExcelError.ExcelErrorNA

                Return BuildCalibrationPointsTable(rows)
            Catch ex As Exception
                Return LoggedUdfError("BESH.PLOT.CALIB_POINTS", ex, ExcelError.ExcelErrorValue)
            End Try
        End Function

        '----------------------------------------------------------------------
        ' Helpers
        '----------------------------------------------------------------------

        ''' <summary>
        ''' Builds a chart-ready calibration-points table from calibration-bin summaries.
        ''' </summary>
        ''' <param name="rows">Calibration-bin summaries returned by the shared reporting engine.</param>
        ''' <returns>
        ''' A spill range with columns Bin, N, MeanPredicted, ObservedRate, LowerCI, UpperCI, ErrorMinus, ErrorPlus.
        ''' </returns>
        Private Function BuildCalibrationPointsTable(rows As IList(Of regression.CalibrationBinSummary)) As Object(,)
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
                Dim r = rows(i)
                out(i + 1, 0) = r.BinIndex
                out(i + 1, 1) = r.N
                out(i + 1, 2) = r.MeanPredicted
                out(i + 1, 3) = r.ObservedRate
                out(i + 1, 4) = r.LowerCI
                out(i + 1, 5) = r.UpperCI
                out(i + 1, 6) = If(Double.IsNaN(r.LowerCI), ExcelError.ExcelErrorNA, r.ObservedRate - r.LowerCI)
                out(i + 1, 7) = If(Double.IsNaN(r.UpperCI), ExcelError.ExcelErrorNA, r.UpperCI - r.ObservedRate)
            Next

            Return PrepareResultTableForUdf(out)
        End Function


        Private Function ResolveHistogramRule(arg As Object) As String
            Dim token As String = UDFhelpers.NormalizeToken(UDFhelpers.AsString(arg))
            If String.IsNullOrWhiteSpace(token) Then Return "(Sturges)"

            Select Case token
                Case "sturges", "sturge"
                    Return "(Sturges)"
                Case "doane", "doan"
                    Return "(Doane)"
                Case "scott"
                    Return "(Scott)"
                Case "freedmandiaconis", "freedman diaconis", "fd", "freedman-diaconis"
                    Return "(Freedman-Diaconis)"
                Case Else
                    Return "(Sturges)"
            End Select
        End Function

        Private Function ParseRocDirection(direction As Object) As Boolean
            Dim token As String = UDFhelpers.NormalizeToken(UDFhelpers.AsString(direction))
            If String.IsNullOrWhiteSpace(token) Then Return False
            Return (token = "lower" OrElse token = "low" OrElse token = "smaller" OrElse token = "decreasing")
        End Function

        Private Function GuessPositiveClass(statusCol(,) As Object, explicitPositive As Object, ByRef positiveLabel As String) As Boolean
            positiveLabel = Nothing
            Dim explicitText As String = UDFhelpers.CellToTrimmedText(explicitPositive)
            If Not String.IsNullOrWhiteSpace(explicitText) Then
                positiveLabel = explicitText
                Return True
            End If

            Dim distinctTokens As New List(Of String)()
            Dim distinctNormalized As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
            Dim allBinary01 As Boolean = True

            For i As Integer = 0 To statusCol.GetLength(0) - 1
                Dim s As String = UDFhelpers.CellToTrimmedText(statusCol(i, 0))
                If String.IsNullOrWhiteSpace(s) Then Continue For

                Dim iv As Integer
                If Not UDFhelpers.TryGetStatus01Flexible(statusCol(i, 0), iv) Then
                    allBinary01 = False
                End If

                Dim norm As String = UDFhelpers.NormalizeToken(s)
                If Not distinctNormalized.Contains(norm) Then
                    distinctNormalized.Add(norm)
                    distinctTokens.Add(s)
                End If
            Next

            If allBinary01 Then
                positiveLabel = "1"
                Return True
            End If

            If distinctTokens.Count = 2 Then
                Dim norms = distinctTokens.Select(Function(x) UDFhelpers.NormalizeToken(x)).ToArray()
                For i As Integer = 0 To norms.Length - 1
                    Select Case norms(i)
                        Case "positive", "pos", "case", "cases", "event", "events", "yes", "true"
                            positiveLabel = distinctTokens(i)
                            Return True
                    End Select
                Next
                positiveLabel = distinctTokens(0)
                Return True
            End If

            Return False
        End Function

        Private Function TryBuildRocInput(marker As Object, status As Object, positiveClass As Object, direction As Object,
                                          ByRef rocInput()() As Double, ByRef markerName As String, ByRef positiveLabel As String,
                                          ByRef isLowerDirection As Boolean) As Boolean
            rocInput = Nothing
            markerName = Nothing
            positiveLabel = Nothing
            isLowerDirection = ParseRocDirection(direction)

            Dim markerCol(,) As Object = Nothing
            Dim statusCol(,) As Object = Nothing
            Dim inferredMarker As String = Nothing
            Dim inferredStatus As String = Nothing

            If Not UDFhelpers.TryGetTrimmedColumnObject(marker, markerCol, inferredMarker, "numeric") Then Return False
            If Not UDFhelpers.TryGetTrimmedColumnObject(status, statusCol, inferredStatus, "binary") Then
                If Not UDFhelpers.TryGetTrimmedColumnObject(status, statusCol, inferredStatus, "text") Then Return False
            End If

            If markerCol.GetLength(0) <> statusCol.GetLength(0) Then
                If statusCol.GetLength(0) = markerCol.GetLength(0) + 1 Then
                    Dim trimmed(statusCol.GetLength(0) - 2, 0) As Object
                    For i As Integer = 1 To statusCol.GetLength(0) - 1
                        trimmed(i - 1, 0) = statusCol(i, 0)
                    Next
                    statusCol = trimmed
                End If
            End If

            If markerCol.GetLength(0) <> statusCol.GetLength(0) Then Return False

            markerName = If(String.IsNullOrWhiteSpace(inferredMarker), "Marker", inferredMarker)
            If Not GuessPositiveClass(statusCol, positiveClass, positiveLabel) Then Return False

            Dim pos As New List(Of Double)()
            Dim neg As New List(Of Double)()
            Dim normalizedPositive As String = UDFhelpers.NormalizeToken(positiveLabel)

            For i As Integer = 0 To markerCol.GetLength(0) - 1
                Dim x As Double
                If Not UDFhelpers.TryGetFiniteDoubleFlexible(markerCol(i, 0), x) Then Continue For

                Dim label As String = UDFhelpers.CellToTrimmedText(statusCol(i, 0))
                If String.IsNullOrWhiteSpace(label) Then Continue For

                Dim isPositive As Boolean = False
                Dim iv As Integer
                If normalizedPositive = "1" AndAlso UDFhelpers.TryGetStatus01Flexible(statusCol(i, 0), iv) Then
                    isPositive = (iv = 1)
                Else
                    isPositive = String.Equals(UDFhelpers.NormalizeToken(label), normalizedPositive, StringComparison.OrdinalIgnoreCase)
                End If

                If isLowerDirection Then x = -x
                If isPositive Then
                    pos.Add(x)
                Else
                    neg.Add(x)
                End If
            Next

            If pos.Count < 1 OrElse neg.Count < 1 Then Return False
            rocInput = New Double()() {pos.ToArray(), neg.ToArray()}
            Return True
        End Function

        Private Function BuildRocPointsTable(cutoffTable As Object(,), isLowerDirection As Boolean) As Object(,)
            If cutoffTable Is Nothing OrElse cutoffTable.GetLength(0) < 2 Then
                Return BuildSimpleNoteTable("ROC", "No ROC cut-off table was returned.")
            End If

            Dim n As Integer = cutoffTable.GetLength(0) - 1
            Dim out(n + 2, 4) As Object
            out(0, 0) = "Threshold"
            out(0, 1) = "Sensitivity"
            out(0, 2) = "Specificity"
            out(0, 3) = "FalsePositiveRate"
            out(0, 4) = "TruePositiveRate"

            out(1, 0) = ""
            out(1, 1) = 1.0R
            out(1, 2) = 0.0R
            out(1, 3) = 1.0R
            out(1, 4) = 1.0R

            For i As Integer = 1 To n
                Dim thresholdObj As Object = cutoffTable(i, 0)
                Dim threshold As Double
                If UDFhelpers.TryGetFiniteDoubleFlexible(thresholdObj, threshold) Then
                    out(i + 1, 0) = If(isLowerDirection, -threshold, threshold)
                Else
                    out(i + 1, 0) = thresholdObj
                End If

                Dim sens As Double = Convert.ToDouble(cutoffTable(i, 1), CultureInfo.InvariantCulture)
                Dim spec As Double = Convert.ToDouble(cutoffTable(i, 2), CultureInfo.InvariantCulture)
                out(i + 1, 1) = sens
                out(i + 1, 2) = spec
                out(i + 1, 3) = 1.0R - spec
                out(i + 1, 4) = sens
            Next

            out(n + 2, 0) = ""
            out(n + 2, 1) = 0.0R
            out(n + 2, 2) = 1.0R
            out(n + 2, 3) = 0.0R
            out(n + 2, 4) = 0.0R
            Return out
        End Function

        Private Function BuildRocStatsTable(summaryTable As Object(,)) As Object(,)
            If summaryTable Is Nothing OrElse summaryTable.GetLength(0) < 2 Then
                Return BuildSimpleNoteTable("ROC", "No ROC summary table was returned.")
            End If

            Dim bodyRows As Integer = summaryTable.GetLength(0) - 1
            Dim out(bodyRows, 1) As Object
            out(0, 0) = "Metric"
            out(0, 1) = "Value"
            For i As Integer = 1 To bodyRows
                out(i, 0) = summaryTable(i, 0)
                out(i, 1) = summaryTable(i, 1)
            Next
            Return out
        End Function

        Private Function TryBuildSurvivalRecords(time As Object, status As Object, group As Object,
                                                 ByRef records As List(Of survival.SurvivalRecord)) As Boolean
            records = Nothing
            Dim timeCol(,) As Object = Nothing
            Dim statusCol(,) As Object = Nothing
            Dim groupCol(,) As Object = Nothing
            Dim timeName As String = Nothing
            Dim statusName As String = Nothing
            Dim groupName As String = Nothing

            If Not UDFhelpers.TryGetTrimmedColumnObject(time, timeCol, timeName, "numeric") Then Return False
            If Not UDFhelpers.TryGetTrimmedColumnObject(status, statusCol, statusName, "binary") Then Return False
            If timeCol.GetLength(0) <> statusCol.GetLength(0) Then Return False

            Dim hasGroup As Boolean = Not UDFhelpers.IsMissingArg(group)
            If hasGroup Then
                If Not UDFhelpers.TryGetTrimmedColumnObject(group, groupCol, groupName, "text") Then Return False
                If groupCol.GetLength(0) <> timeCol.GetLength(0) Then Return False
            End If

            Dim tList As New List(Of Double)()
            Dim sList As New List(Of Integer)()
            Dim gList As New List(Of String)()
            Dim stratList As New List(Of String)()

            For i As Integer = 0 To timeCol.GetLength(0) - 1
                Dim t As Double
                Dim s As Integer
                If Not UDFhelpers.TryGetFiniteDoubleFlexible(timeCol(i, 0), t) Then Continue For
                If t < 0.0R Then Return False
                If Not UDFhelpers.TryGetStatus01Flexible(statusCol(i, 0), s) Then Continue For

                Dim g As String = "ALL"
                If hasGroup Then
                    g = UDFhelpers.CellToTrimmedText(groupCol(i, 0))
                    If String.IsNullOrWhiteSpace(g) Then Continue For
                End If

                tList.Add(t)
                sList.Add(s)
                gList.Add(g)
                stratList.Add("ALL")
            Next

            If tList.Count < 1 Then Return False

            Dim err As String = Nothing
            records = survival.Survival.CreatSurvivalData(tList.ToArray(), sList.ToArray(), gList.ToArray(), stratList.ToArray(), err)
            Return records IsNot Nothing AndAlso records.Count > 0
        End Function

        Private Function BuildKaplanMeierStepTable(rawCurves As Object()) As Object(,)
            Dim totalRows As Integer = 1 'header

            For Each groupObj As Object In rawCurves
                If groupObj Is Nothing Then Continue For
                Dim items As IEnumerable = TryCast(groupObj, IEnumerable)
                If items Is Nothing Then Continue For
                Dim recs As List(Of survival.SurvivalTableRecord) = items.Cast(Of Object)().Select(Function(x) CType(x, survival.SurvivalTableRecord)).ToList()
                If recs.Count = 0 Then Continue For
                totalRows += 1 + 2 * recs.Count
            Next

            If totalRows <= 1 Then
                Return BuildSimpleNoteTable("Kaplan-Meier", "No survival-curve rows were returned.")
            End If

            Dim out(totalRows - 1, 6) As Object
            out(0, 0) = "Group"
            out(0, 1) = "PlotOrder"
            out(0, 2) = "Time"
            out(0, 3) = "Survival"
            out(0, 4) = "LowerCI"
            out(0, 5) = "UpperCI"
            out(0, 6) = "AtRisk"

            Dim row As Integer = 1
            For Each groupObj As Object In rawCurves
                If groupObj Is Nothing Then Continue For
                Dim items As IEnumerable = TryCast(groupObj, IEnumerable)
                If items Is Nothing Then Continue For
                Dim recs As List(Of survival.SurvivalTableRecord) = items.Cast(Of Object)().Select(Function(x) CType(x, survival.SurvivalTableRecord)).ToList()
                If recs.Count = 0 Then Continue For

                Dim groupName As String = recs(0).strGroup
                Dim prevProb As Double = 1.0R
                Dim prevLL As Double = 1.0R
                Dim prevUL As Double = 1.0R
                Dim plotOrder As Integer = 1

                out(row, 0) = groupName
                out(row, 1) = plotOrder
                out(row, 2) = 0.0R
                out(row, 3) = 1.0R
                out(row, 4) = 1.0R
                out(row, 5) = 1.0R
                out(row, 6) = recs(0).AtRisk
                row += 1
                plotOrder += 1

                For Each rec As survival.SurvivalTableRecord In recs
                    out(row, 0) = groupName
                    out(row, 1) = plotOrder
                    out(row, 2) = rec.Time
                    out(row, 3) = prevProb
                    out(row, 4) = prevLL
                    out(row, 5) = prevUL
                    out(row, 6) = ""
                    row += 1
                    plotOrder += 1

                    out(row, 0) = groupName
                    out(row, 1) = plotOrder
                    out(row, 2) = rec.Time
                    out(row, 3) = rec.Prob
                    out(row, 4) = rec.ProbCILL
                    out(row, 5) = rec.ProbCIUL
                    out(row, 6) = rec.AtRisk
                    row += 1
                    plotOrder += 1

                    prevProb = rec.Prob
                    prevLL = rec.ProbCILL
                    prevUL = rec.ProbCIUL
                Next
            Next

            Return out
        End Function

        Private Function StackHomogeneousTables(tables As List(Of Object(,))) As Object(,)
            If tables Is Nothing OrElse tables.Count = 0 Then
                Dim empty(0, 0) As Object
                empty(0, 0) = ExcelError.ExcelErrorNA
                Return empty
            End If

            Dim cols As Integer = tables(0).GetLength(1)
            Dim totalRows As Integer = 1
            For Each tbl As Object(,) In tables
                totalRows += Math.Max(0, tbl.GetLength(0) - 1)
            Next

            Dim out(totalRows - 1, cols - 1) As Object
            For j As Integer = 0 To cols - 1
                out(0, j) = tables(0)(0, j)
            Next

            Dim row As Integer = 1
            For Each tbl As Object(,) In tables
                For i As Integer = 1 To tbl.GetLength(0) - 1
                    For j As Integer = 0 To cols - 1
                        out(row, j) = tbl(i, j)
                    Next
                    row += 1
                Next
            Next

            Return out
        End Function

    End Module
End Namespace
