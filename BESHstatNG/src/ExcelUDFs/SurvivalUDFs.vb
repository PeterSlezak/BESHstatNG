Option Explicit On
Imports System.Globalization
Imports System.Windows.Forms.VisualStyles.VisualStyleElement
Imports System.Windows.Forms.VisualStyles.VisualStyleElement.TaskbarClock
Imports ExcelDna.Integration

Namespace BESHStatNG.WorksheetFunctions

    ''' <summary>
    ''' Worksheet functions for basic survival analysis procedures.
    ''' </summary>
    ''' <remarks>
    ''' These functions are designed for direct use in Excel worksheets.
    ''' They accept ranges (columns) as inputs, validate and clean the data,
    ''' and return Excel-friendly numeric outputs or Excel error values when
    ''' inputs are invalid.
    ''' </remarks>
    Public Module SurvivalUDFs

        ' ============
        ' Public UDFs
        ' ============

        ''' <summary>
        ''' Computes the p-value for a (possibly stratified) log-rank family test comparing survival curves across groups.
        ''' </summary>
        ''' <param name="timeRange">
        ''' A single-column range containing follow-up times (time-to-event or time-to-censoring). Values must be ≥ 0.
        ''' </param>
        ''' <param name="statusRange">
        ''' A single-column range containing event indicators: 1 = event occurred, 0 = censored. Other values are invalid.
        ''' </param>
        ''' <param name="groupRange">
        ''' A single-column range containing group identifiers (text or numbers). Each distinct value defines a group.
        ''' </param>
        ''' <param name="strataRange">
        ''' Optional single-column range containing stratum identifiers (text or numbers) for stratified analysis.
        ''' If omitted, all rows are treated as belonging to a single stratum.
        ''' </param>
        ''' <param name="weight">
        ''' Optional weighting scheme for the log-rank family test:
        ''' <list type="bullet">
        '''   <item><description><c>"logrank"</c> — standard log-rank test (equal weights across time).</description></item>
        '''   <item><description><c>"gehan-breslow"</c> — Gehan–Breslow (generalized Wilcoxon) weights emphasize early events.</description></item>
        '''   <item><description><c>"tarone-ware"</c> — Tarone–Ware weights (intermediate emphasis on early events).</description></item>
        '''   <item><description><c>"peto"</c> — Peto–Peto weights based on the pooled Kaplan–Meier estimate just prior to each event time.</description></item>
        '''   <item><description><c>"modified peto"</c> — modified Peto weight with a small-sample adjustment.</description></item>
        ''' </list>
        ''' The comparison is performed by accumulating weighted observed-minus-expected event counts over event times
        ''' (and across strata when provided) and evaluating the resulting chi-square statistic.
        ''' </param>
        ''' <returns>
        ''' The (upper-tail) p-value from a chi-square distribution with degrees of freedom equal to (number of groups − 1).
        ''' Returns <c>#VALUE!</c> for invalid range shapes or unknown weight names, and <c>#NUM!</c> for invalid data.
        ''' </returns>
        ''' <example>
        ''' <code>
        ''' =BESH.SURV.LOGRANK_P(A2:A101, B2:B101, C2:C101)
        ''' =BESH.SURV.LOGRANK_P(A2:A101, B2:B101, C2:C101, D2:D101, "tarone-ware")
        ''' </code>
        ''' </example>
        <ExcelFunction(
            Name:="BESH.SURV.LOGRANK_P",
            Category:="BESHStatNG - Survival",
            Description:="Log-rank family test p-value for comparing survival curves across groups (optionally stratified; supports multiple weight schemes).",
            HelpTopic:=HelpLinks.BaseUrlRoot & "/udf/survival/")>
        Public Function LOGRANK_P(
            <ExcelArgument(Name:="time", Description:="Single-column range of follow-up times (>=0).")> timeRange As Object,
            <ExcelArgument(Name:="status", Description:="Single-column range of event indicators (1=event, 0=censored).")> statusRange As Object,
            <ExcelArgument(Name:="group", Description:="Single-column range of group identifiers (text or numbers).")> groupRange As Object,
            <ExcelArgument(Name:="strata", Description:="Optional single-column range of stratum identifiers (text or numbers).")> Optional strataRange As Object = Nothing,
            <ExcelArgument(Name:="weight", Description:="Optional weighting: logrank, gehan-breslow, tarone-ware, peto, modified peto.")> Optional weight As Object = Nothing
        ) As Object

            Dim res As TestResult = Nothing
            Dim errText As String = Nothing
            Dim ok = TryComputeLogRank(timeRange, statusRange, groupRange, strataRange, weight, res, errText)
            If Not ok Then
                Return ExcelError.ExcelErrorNum
            End If

            If res Is Nothing OrElse Double.IsNaN(res.Pvalue) OrElse Double.IsInfinity(res.Pvalue) Then
                Return ExcelError.ExcelErrorNum
            End If

            Return res.Pvalue
        End Function


        ''' <summary>
        ''' Computes the test statistic for a (possibly stratified) log-rank family test comparing survival curves across groups.
        ''' </summary>
        ''' <param name="timeRange">Single-column range of follow-up times (≥ 0).</param>
        ''' <param name="statusRange">Single-column range of event indicators: 1 = event, 0 = censored.</param>
        ''' <param name="groupRange">Single-column range of group identifiers (text or numbers).</param>
        ''' <param name="strataRange">
        ''' Optional single-column range of stratum identifiers for stratified analysis.
        ''' If omitted, all rows are treated as belonging to a single stratum.
        ''' </param>
        ''' <param name="weight">
        ''' Optional weighting scheme: logrank, gehan-breslow, tarone-ware, peto, modified peto.
        ''' </param>
        ''' <returns>
        ''' The chi-square test statistic with degrees of freedom (number of groups − 1).
        ''' Returns <c>#VALUE!</c> for invalid range shapes or unknown weight names, and <c>#NUM!</c> for invalid data.
        ''' </returns>
        ''' <example>
        ''' <code>
        ''' =BESH.SURV.LOGRANK_STAT(A2:A101, B2:B101, C2:C101, , "logrank")
        ''' </code>
        ''' </example>
        <ExcelFunction(
            Name:="BESH.SURV.LOGRANK_STAT",
            Category:="BESHStatNG - Survival",
            Description:="Log-rank family test chi-square statistic for comparing survival curves across groups (optionally stratified; supports multiple weight schemes).",
            HelpTopic:=HelpLinks.BaseUrlRoot & "/udf/survival/")>
        Public Function LOGRANK_STAT(
            <ExcelArgument(Name:="time", Description:="Single-column range of follow-up times (>=0).")> timeRange As Object,
            <ExcelArgument(Name:="status", Description:="Single-column range of event indicators (1=event, 0=censored).")> statusRange As Object,
            <ExcelArgument(Name:="group", Description:="Single-column range of group identifiers (text or numbers).")> groupRange As Object,
            <ExcelArgument(Name:="strata", Description:="Optional single-column range of stratum identifiers (text or numbers).")> Optional strataRange As Object = Nothing,
            <ExcelArgument(Name:="weight", Description:="Optional weighting: logrank, gehan-breslow, tarone-ware, peto, modified peto.")> Optional weight As Object = Nothing
        ) As Object

            Dim res As TestResult = Nothing
            Dim errText As String = Nothing
            Dim ok = TryComputeLogRank(timeRange, statusRange, groupRange, strataRange, weight, res, errText)
            If Not ok Then
                Return ExcelError.ExcelErrorNum
            End If

            Dim stat As Double = res.TestStatistics1
            If Double.IsNaN(stat) OrElse Double.IsInfinity(stat) Then
                Return ExcelError.ExcelErrorNum
            End If

            Return stat
        End Function

        ''' <summary>
        ''' Computes the Kaplan–Meier median survival time and its Brookmeyer–Crowley confidence interval at level <c>1 - alpha</c>.
        ''' </summary>
        ''' <param name="time">
        ''' A single-column range containing follow-up times (time-to-event or time-to-censoring). Values must be ≥ 0.
        ''' </param>
        ''' <param name="status">
        ''' A single-column range containing event indicators: 1 = event occurred, 0 = censored. Other values are invalid.
        ''' </param>
        ''' <param name="group">
        ''' Optional single-column range of group identifiers (text or numbers).
        ''' If omitted, the median and confidence interval are computed for the whole sample and a single-row result is returned.
        ''' If provided, the result includes one row per group (based on distinct identifiers present in the input), allowing
        ''' quick comparison of group-specific medians.
        ''' </param>
        ''' <param name="alpha">
        ''' Optional two-sided significance level used for the Brookmeyer–Crowley confidence interval.
        ''' The default is <c>0.05</c>, corresponding to a 95% confidence interval.
        ''' </param>
        ''' <returns>
        ''' A 2D array with one row per group and the following columns:
        ''' <list type="bullet">
        '''   <item><description>Column 0: Group identifier (or <c>ALL</c> when no group range is provided)</description></item>
        '''   <item><description>Column 1: Median survival time (Kaplan–Meier estimate)</description></item>
        '''   <item><description>Column 2: confidence interval lower bound</description></item>
        '''   <item><description>Column 3: confidence interval upper bound</description></item>
        ''' </list>
        ''' If the median (or CI bound) is not defined for a group (e.g., heavy censoring and the estimated survival curve never drops to 0.5),
        ''' the corresponding cell is returned as <c>#N/A</c>.
        ''' <para>
        ''' Returns <c>#VALUE!</c> for invalid range shapes (inputs must be single-column and have the same number of rows),
        ''' and <c>#NUM!</c> when there is insufficient valid data.
        ''' </para>
        ''' </returns>
        ''' <example>
        ''' <code>
        ''' =BESH.SURV.MEDIAN_CI(A2:A101, B2:B101)
        ''' =BESH.SURV.MEDIAN_CI(A2:A101, B2:B101, C2:C101)
        ''' =BESH.SURV.MEDIAN_CI(A2:A101, B2:B101, C2:C101, 0.1)
        ''' </code>
        ''' </example>
        <ExcelFunction(
    Name:="BESH.SURV.MEDIAN_CI",
    Category:="BESHStatNG - Survival",
    Description:="Kaplan–Meier median survival time with Brookmeyer–Crowley CI (overall or by group). Returns a 2D table.",
    HelpTopic:=HelpLinks.BaseUrlRoot & "/udf/survival/")>
        Public Function MEDIAN_CI(
    <ExcelArgument(Name:="time", Description:="Single-column range of follow-up times (>=0).")> time As Object,
    <ExcelArgument(Name:="status", Description:="Single-column range of event indicators (1=event, 0=censored).")> status As Object,
    <ExcelArgument(Name:="group", Description:="Optional single-column range of group identifiers. If omitted, computes overall median.")> Optional group As Object = Nothing,
    <ExcelArgument(Name:="alpha", Description:="Optional two-sided alpha for the Brookmeyer-Crowley confidence interval (default 0.05).")> Optional alpha As Object = Nothing
) As Object

            Try
                Dim alphaValue As Double = 0.05
                If Not ParametricUDFs.TryParseAlpha(alpha, alphaValue) Then
                    Return ExcelError.ExcelErrorNum
                End If

                Dim outArr As Object(,) = Nothing
                If Not TryComputeMedianCI(time, status, group, alphaValue, outArr) Then
                    Return ExcelError.ExcelErrorNum
                End If
                Return outArr
            Catch ex As Exception
                Return LoggedUdfError("BESH.SURV.MEDIAN_CI", ex, ExcelError.ExcelErrorValue)
            End Try

        End Function

        ''' <summary>
        ''' Returns a tabular Kaplan–Meier survival curve.
        ''' </summary>
        ''' <remarks>
        ''' <para>
        ''' This function computes the Kaplan–Meier estimate of the survival function <c>S(t)</c>
        ''' for the full sample (when <paramref name="group"/> is omitted) or separately for each group
        ''' (when <paramref name="group"/> is provided).
        ''' </para>
        ''' <para>
        ''' The output includes, for each event/censoring time, the number at risk, the estimated survival
        ''' probability, Greenwood standard error, and a two-sided confidence interval at level <c>1 - alpha</c>.
        ''' Confidence limits are computed on a transformed scale to keep limits within <c>[0,1]</c>.
        ''' </para>
        ''' <para>
        ''' <b>Input rules</b>
        ''' <list type="bullet">
        '''   <item><description><paramref name="time"/> must be a single-column range of non-negative times.</description></item>
        '''   <item><description><paramref name="status"/> must be a single-column range coded as 1=event, 0=censored.</description></item>
        '''   <item><description><paramref name="group"/> is optional; if provided, it must be a single-column range with the same number of rows as <paramref name="time"/>.</description></item>
        '''   <item><description>Rows with missing/non-numeric time or invalid status are ignored. If <paramref name="group"/> is provided, blank group IDs are ignored.</description></item>
        ''' </list>
        ''' </para>
        ''' <para>
        ''' <b>Returned table</b> (no header row):
        ''' <list type="bullet">
        '''   <item><description>Col 1: Group ID</description></item>
        '''   <item><description>Col 2: Time</description></item>
        '''   <item><description>Col 3: At risk</description></item>
        '''   <item><description>Col 4: S(t)</description></item>
        '''   <item><description>Col 5: SE(S(t))</description></item>
        '''   <item><description>Col 6: Lower confidence limit</description></item>
        '''   <item><description>Col 7: Upper confidence limit</description></item>
        ''' </list>
        ''' </para>
        ''' </remarks>
        ''' <param name="time">Single-column range of follow-up times (>= 0).</param>
        ''' <param name="status">Single-column range of event indicators (1=event, 0=censored).</param>
        ''' <param name="group">
        ''' Optional single-column range of group IDs (text or numbers). When omitted, all observations are treated as one group.
        ''' </param>
        ''' <param name="alpha">
        ''' Optional two-sided significance level used for the Kaplan–Meier confidence interval.
        ''' The default is <c>0.05</c>, corresponding to a 95% confidence interval.
        ''' </param>
        ''' <returns>
        ''' A 2D array with one row per time point per group and 7 columns:
        ''' group, time, at risk, survival, SE, lower confidence limit, upper confidence limit.
        ''' </returns>
        ''' <example>
        ''' <code>
        ''' =BESH.SURV.KM_TABLE(A2:A200, B2:B200)
        ''' =BESH.SURV.KM_TABLE(A2:A200, B2:B200, C2:C200)
        ''' =BESH.SURV.KM_TABLE(A2:A200, B2:B200, C2:C200, 0.1)
        ''' </code>
        ''' </example>
        <ExcelFunction(
            Name:="BESH.SURV.KM_TABLE",
            Category:="BESHStatNG - Survival",
            Description:="Kaplan-Meier tabular survival curve: group, time, at-risk, S(t), SE, lower/upper CI.",
            HelpTopic:=HelpLinks.BaseUrlRoot & "/udf/survival/"
        )>
        Public Function KM_TABLE(
            <ExcelArgument(Name:="time", Description:="Follow-up time (single column, >=0).")> time As Object,
            <ExcelArgument(Name:="status", Description:="Event indicator (single column, 1=event, 0=censored).")> status As Object,
            <ExcelArgument(Name:="[group]", Description:="Optional group IDs (single column). When omitted, computes one overall curve.")> Optional group As Object = Nothing,
            <ExcelArgument(Name:="alpha", Description:="Optional two-sided alpha for the Kaplan-Meier confidence interval (default 0.05).")> Optional alpha As Object = Nothing
        ) As Object

            Try
                Dim alphaValue As Double = 0.05
                If Not ParametricUDFs.TryParseAlpha(alpha, alphaValue) Then
                    Return ExcelError.ExcelErrorNum
                End If

                Dim hasGroup As Boolean = (group IsNot Nothing) AndAlso Not TypeOf group Is ExcelMissing

                Dim records As List(Of survival.SurvivalRecord) = BuildSurvivalRecords(time, status, If(hasGroup, group, Nothing))
                If records Is Nothing OrElse records.Count = 0 Then Return ExcelError.ExcelErrorNum

                Dim km As New survival.Survival_KM_LR(records)
                Dim outObj As Object() = km.SurvivalCurveTabularOutput(alphaValue)

                Dim totalRows As Integer = 0
                For Each g In outObj
                    If g Is Nothing Then Continue For
                    Dim lst = TryCast(g, System.Collections.IEnumerable)
                    If lst Is Nothing Then Continue For
                    totalRows += lst.Cast(Of Object)().Count()
                Next

                If totalRows = 0 Then Return ExcelError.ExcelErrorNum

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

            Catch ex As Exception
                Return LoggedUdfError("BESH.SURV.KM_TABLE", ex, ExcelError.ExcelErrorValue)
            End Try

        End Function

        ' =================
        ' Internal helpers
        ' =================
        Private Function BuildSurvivalRecords(time As Object, status As Object, group As Object) As List(Of survival.SurvivalRecord)
            Dim tArr = TryCast(time, Object(,))
            Dim sArr = TryCast(status, Object(,))
            If tArr Is Nothing OrElse sArr Is Nothing Then Return Nothing
            If tArr.GetLength(1) <> 1 OrElse sArr.GetLength(1) <> 1 Then Return Nothing
            If tArr.GetLength(0) <> sArr.GetLength(0) Then Return Nothing

            Dim hasGroup As Boolean = (group IsNot Nothing) AndAlso Not TypeOf group Is ExcelMissing
            Dim gArr As Object(,) = Nothing
            If hasGroup Then
                gArr = TryCast(group, Object(,))
                If gArr Is Nothing Then Return Nothing
                If gArr.GetLength(1) <> 1 Then Return Nothing
                If gArr.GetLength(0) <> tArr.GetLength(0) Then Return Nothing
            End If

            Dim map As New Dictionary(Of String, Integer)(StringComparer.OrdinalIgnoreCase)
            Dim nextId As Integer = 0

            Dim records As New List(Of survival.SurvivalRecord)()

            For i As Integer = 0 To tArr.GetLength(0) - 1
                Dim tVal As Double
                Dim sVal As Integer

                If Not UDFhelpers.TryGetFiniteDoubleFlexible(tArr(i, 0), tVal) Then Continue For
                If tVal < 0 Then Continue For
                If Not UDFhelpers.TryGetStatus01Flexible(sArr(i, 0), sVal) Then Continue For

                Dim gKey As String = "All"
                If hasGroup Then
                    Dim cell = gArr(i, 0)
                    If cell Is Nothing Then Continue For
                    Dim gs As String = Convert.ToString(cell, CultureInfo.InvariantCulture)
                    If String.IsNullOrWhiteSpace(gs) Then Continue For
                    gKey = gs.Trim()
                End If

                Dim gid As Integer
                If Not map.TryGetValue(gKey, gid) Then
                    gid = nextId
                    map(gKey) = gid
                    nextId += 1
                End If

                Dim rec As New survival.SurvivalRecord With {
                        .Time = tVal,
                        .Censorship = sVal,
                        .Group = gid,
                        .strGroup = gKey,
                        .Stratum = "",
                        .strStratum = ""
                    }
                records.Add(rec)
            Next

            Return records
        End Function

        Private Function TryComputeMedianCI(timeRange As Object, statusRange As Object, groupRange As Object,
                                            alpha As Double, ByRef outTable As Object(,)) As Boolean
            outTable = Nothing

            Dim timeArr As Object(,) = Nothing
            Dim statusArr As Object(,) = Nothing
            Dim groupArr As Object(,) = Nothing

            If Not TryGet2D(timeRange, timeArr) Then Return False
            If Not TryGet2D(statusRange, statusArr) Then Return False

            If timeArr.GetLength(1) <> 1 OrElse statusArr.GetLength(1) <> 1 Then
                Return False
            End If

            Dim nRows As Integer = timeArr.GetLength(0)
            If statusArr.GetLength(0) <> nRows Then Return False

            Dim hasGroup As Boolean = False
            If groupRange IsNot Nothing AndAlso Not TypeOf groupRange Is ExcelMissing AndAlso Not TypeOf groupRange Is ExcelEmpty Then
                hasGroup = True
                If Not TryGet2D(groupRange, groupArr) Then Return False
                If groupArr.GetLength(1) <> 1 OrElse groupArr.GetLength(0) <> nRows Then Return False
            End If

            Dim tList As New List(Of Double)()
            Dim sList As New List(Of Integer)()
            Dim gList As New List(Of String)()

            For i = 0 To nRows - 1
                Dim t As Double
                Dim s As Integer

                If Not UDFhelpers.TryGetFiniteDoubleFlexible(timeArr(i, 0), t) Then Continue For
                If Not UDFhelpers.TryGetStatus01Flexible(statusArr(i, 0), s) Then Continue For
                If t < 0 Then Return False

                Dim g As String = "ALL"
                If hasGroup Then
                    g = UDFhelpers.CellToTrimmedText(groupArr(i, 0))
                    If String.IsNullOrWhiteSpace(g) Then Continue For
                    g = g.Trim()
                End If

                tList.Add(t)
                sList.Add(s)
                gList.Add(g)
            Next

            If tList.Count < 3 Then Return False

            Dim err As String = Nothing
            Dim stratAll() As String = Enumerable.Repeat("ALL", tList.Count).ToArray()

            Dim recs = survival.Survival.CreatSurvivalData(
                tList.ToArray(),
                sList.ToArray(),
                gList.ToArray(),
                stratAll,
                err)

            If recs Is Nothing Then Return False

            Dim lr As New survival.Survival_KM_LR(recs)
            Dim mci As Object(,) = lr.BrookmeyerCrowleyMedianSurvivalCI(alpha) ' (group, 0..2) = median, LLCI, ULCI

            If mci Is Nothing OrElse mci.GetLength(0) < 1 Then Return False

            Dim groupIds = gList.Distinct().ToList()

            Dim k As Integer = mci.GetLength(0)

            Dim out As Object(,) = New Object(k - 1, 3) {}

            For j = 0 To k - 1
                Dim med As Double = CDbl(mci(j, 0))
                Dim ll As Double = CDbl(mci(j, 1))
                Dim ul As Double = CDbl(mci(j, 2))

                ' Col 1: group id
                out(j, 0) = groupIds(j)

                ' Col 2: median survival time
                If Double.IsNaN(med) OrElse Double.IsInfinity(med) Then
                    out(j, 1) = ExcelError.ExcelErrorNA
                Else
                    out(j, 1) = med
                End If

                ' Col 3-4: CI lower/upper
                If Double.IsNaN(ll) OrElse Double.IsInfinity(ll) Then
                    out(j, 2) = ExcelError.ExcelErrorNA
                Else
                    out(j, 2) = ll
                End If

                If Double.IsNaN(ul) OrElse Double.IsInfinity(ul) Then
                    out(j, 3) = ExcelError.ExcelErrorNA
                Else
                    out(j, 3) = ul
                End If
            Next

            outTable = out
            Return True
        End Function

        Private Function TryComputeLogRank(timeRange As Object,
                                        statusRange As Object,
                                        groupRange As Object,
                                        strataRange As Object,
                                        weight As Object,
                                        ByRef result As TestResult,
                                        ByRef errText As String) As Boolean

            errText = Nothing
            result = Nothing

            Dim timeArr As Object(,) = Nothing
            Dim statusArr As Object(,) = Nothing
            Dim groupArr As Object(,) = Nothing
            Dim strataArr As Object(,) = Nothing

            If Not TryGet2D(timeRange, timeArr) Then Return False
            If Not TryGet2D(statusRange, statusArr) Then Return False
            If Not TryGet2D(groupRange, groupArr) Then Return False

            If timeArr.GetLength(1) <> 1 OrElse statusArr.GetLength(1) <> 1 OrElse groupArr.GetLength(1) <> 1 Then
                Return False 'must be single-column inputs
            End If

            Dim nRows As Integer = timeArr.GetLength(0)
            If statusArr.GetLength(0) <> nRows OrElse groupArr.GetLength(0) <> nRows Then
                Return False 'must be same number of rows
            End If

            Dim hasStrata As Boolean = False
            If strataRange IsNot Nothing AndAlso Not TypeOf strataRange Is ExcelMissing AndAlso Not TypeOf strataRange Is ExcelEmpty Then
                hasStrata = True
                If Not TryGet2D(strataRange, strataArr) Then Return False
                If strataArr.GetLength(1) <> 1 OrElse strataArr.GetLength(0) <> nRows Then
                    Return False
                End If
            End If

            Dim weightMethod As String = ParseWeightMethod(weight)
            If weightMethod Is Nothing Then
                Return False
            End If

            ' Collect valid rows (pairwise across time/status/group[/strata])
            Dim tList As New List(Of Double)()
            Dim sList As New List(Of Integer)()
            Dim gList As New List(Of String)()
            Dim stratList As New List(Of String)()

            For i = 0 To nRows - 1
                Dim t As Double
                Dim s As Integer
                Dim g As String
                Dim st As String = "ALL"

                If Not UDFhelpers.TryGetFiniteDoubleFlexible(timeArr(i, 0), t) Then Continue For
                If Not UDFhelpers.TryGetStatus01Flexible(statusArr(i, 0), s) Then Continue For

                g = UDFhelpers.CellToTrimmedText(groupArr(i, 0))
                If String.IsNullOrWhiteSpace(g) Then Continue For

                If hasStrata Then
                    st = UDFhelpers.CellToTrimmedText(strataArr(i, 0))
                    If String.IsNullOrWhiteSpace(st) Then st = "ALL"
                End If

                If t < 0 Then Return False

                tList.Add(t)
                sList.Add(s)
                gList.Add(g.Trim())
                stratList.Add(st.Trim())
            Next

            If tList.Count < 3 Then Return False
            If gList.Distinct().Count() < 2 Then Return False

            Dim err As String = Nothing
            Dim recs = survival.Survival.CreatSurvivalData(
                tList.ToArray(),
                sList.ToArray(),
                gList.ToArray(),
                stratList.ToArray(),
                err)

            If recs Is Nothing Then
                errText = err
                Return False
            End If

            Dim lr As New survival.Survival_KM_LR(recs)
            result = lr.WeightedLogRankTest(weightMethod)

            Return result IsNot Nothing
        End Function

        Private Function ParseWeightMethod(weight As Object) As String
            Dim w As String = "logrank"

            If weight Is Nothing OrElse TypeOf weight Is ExcelMissing OrElse TypeOf weight Is ExcelEmpty Then
                Return w
            End If

            Dim s As String = UDFhelpers.CellToTrimmedText(weight)
            If String.IsNullOrWhiteSpace(s) Then Return w

            s = s.Trim().ToLowerInvariant()

            Select Case s
                Case "logrank", "lr"
                    Return "logrank"
                Case "gehan-breslow", "gehan", "breslow", "wilcoxon"
                    Return "gehan-breslow"
                Case "tarone-ware", "tarone", "ware"
                    Return "tarone-ware"
                Case "peto", "peto-peto"
                    Return "peto"
                Case "modified peto", "modifiedpeto", "anderson", "modpeto"
                    Return "modified peto"
                Case Else
                    Return Nothing
            End Select
        End Function

        Private Function TryGet2D(input As Object, ByRef arr As Object(,)) As Boolean
            arr = UDFhelpers.Get2DOrScalar(input)
            Return arr IsNot Nothing
        End Function

    End Module

End Namespace
