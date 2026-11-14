Option Explicit On
Option Strict On

Imports System
Imports System.Collections
Imports System.Collections.Generic
Imports System.Globalization
Imports System.Linq
Imports BESHStatNG.regression
Imports ExcelDna.Integration

Namespace WorksheetFunctions

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
                If Not Global.BESHStatNG.UdfDataImport.TryGetGroupedNumericColumns(data, groups, names) Then Return ExcelError.ExcelErrorValue
                If groups Is Nothing OrElse names Is Nothing Then Return ExcelError.ExcelErrorNum

                Dim resolvedRule As String = Global.BESHStatNG.UdfDataImport.GetHistogramBinRule(binRule)
                Dim parts As List(Of Object(,)) = Global.BESHStatNG.ExcelCommon.PlotDataUDFsReports38.BuildHistogramPartsPrepared(groups, names, resolvedRule, AddressOf graphics.HistogramBinsComputation)
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
                If Not Global.BESHStatNG.UdfDataImport.TryGetGroupedNumericColumns(data, groups, names) Then Return ExcelError.ExcelErrorValue
                If groups Is Nothing OrElse names Is Nothing Then Return ExcelError.ExcelErrorNum

                Dim resolvedRule As String = Global.BESHStatNG.UdfDataImport.GetHistogramBinRule(binRule)
                Dim parts As List(Of Object(,)) = Global.BESHStatNG.ExcelCommon.PlotDataUDFsReports38.BuildHistogramNormalPartsPrepared(groups, names, resolvedRule, AddressOf graphics.HistogramBinsComputation, AddressOf graphics.GaussOverlayComputation)
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
                If Not Global.BESHStatNG.UdfDataImport.TryGetRocInput(marker, status, positiveClass, direction, rocInput, markerName, positiveLabel, isLowerDirection) Then
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
                If Not Global.BESHStatNG.UdfDataImport.TryGetRocInput(marker, status, positiveClass, direction, rocInput, markerName, positiveLabel, isLowerDirection) Then
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
                If Not Global.BESHStatNG.UdfDataImport.TryGetSurvivalRecords(time, status, group, records) Then Return ExcelError.ExcelErrorValue
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

                If Not Global.BESHStatNG.UdfDataImport.TryGetNumericColumn(y, yVals) Then Return ExcelError.ExcelErrorValue
                If Not Global.BESHStatNG.UdfDataImport.TryGetNumericColumn(probabilities, pVals) Then Return ExcelError.ExcelErrorValue
                If yVals Is Nothing OrElse pVals Is Nothing Then Return ExcelError.ExcelErrorValue
                If yVals.Count <> pVals.Count OrElse yVals.Count = 0 Then Return ExcelError.ExcelErrorNum

                Dim w() As Double = Nothing
                If Not IsMissingArg(weights) Then
                    If Not Global.BESHStatNG.UdfDataImport.TryGetNumericColumn(weights, wVals) Then Return ExcelError.ExcelErrorValue
                    If wVals Is Nothing OrElse wVals.Count <> yVals.Count Then Return ExcelError.ExcelErrorNum
                    w = wVals.ToArray()
                End If

                Dim binCount As Integer
                If Not TryGetOptionalPositiveInteger(bins, binCount, 10, 1) Then Return ExcelError.ExcelErrorNum

                Dim methodName As String = BinaryClassificationReporting.ParseCalibrationMethod(method, "quantile")
                Dim yy() As Double = yVals.ToArray()
                Dim pp() As Double = pVals.ToArray()

                Global.BESHStatNG.regression.BinaryClassificationCore.ValidateBinaryInputs(yy, pp, w)
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
            ' The original mixed Enum/Double If expression coerced ExcelErrorNA to Double.
            Return DirectCast(MaterializePortableReport38(Global.BESHStatNG.ExcelCommon.PlotDataUDFsReports38.BuildCalibrationPointsTable(rows, CDbl(ExcelError.ExcelErrorNA))), Object(,))
        End Function

        Private Function BuildRocPointsTable(cutoffTable As Object(,), isLowerDirection As Boolean) As Object(,)
            Return Global.BESHStatNG.ExcelCommon.PlotDataUDFsReports38.BuildRocPointsTable(cutoffTable, isLowerDirection, AddressOf TryGetFiniteDoubleFlexible)
        End Function

        Private Function BuildRocStatsTable(summaryTable As Object(,)) As Object(,)
            If summaryTable Is Nothing OrElse summaryTable.GetLength(0) < 2 Then
                Return Global.BESHStatNG.ExcelCommon.PlotDataUDFsReports38.BuildRocStatsTable(summaryTable)
            End If
            Return PreparePValueRowsForUdf(Global.BESHStatNG.ExcelCommon.PlotDataUDFsReports38.BuildRocStatsTable(summaryTable))
        End Function

        Private Function BuildKaplanMeierStepTable(rawCurves As Object()) As Object(,)
            Return Global.BESHStatNG.ExcelCommon.PlotDataUDFsReports38.BuildKaplanMeierStepTablePrepared(
                rawCurves, Function(x) CType(x, survival.SurvivalTableRecord),
                Function(x) CType(x, survival.SurvivalTableRecord), AddressOf BuildSimpleNoteTable)
        End Function

        Private Function StackHomogeneousTables(tables As List(Of Object(,))) As Object(,)
            If tables Is Nothing OrElse tables.Count = 0 Then
                Dim empty(0, 0) As Object
                empty(0, 0) = ExcelError.ExcelErrorNA
                Return empty
            End If

            Return Global.BESHStatNG.ExcelCommon.PlotDataUDFsReports38.StackHomogeneousTablesPrepared(tables)
        End Function

    End Module
End Namespace
