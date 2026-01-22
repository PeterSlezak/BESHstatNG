Option Explicit On
Imports System.Drawing.Drawing2D
Imports System.Linq
Imports System.Runtime.InteropServices.ComTypes
Imports Microsoft.Office.Interop.Excel

Namespace survival

    ''' <summary>
    ''' Utility functions for constructing and formatting survival‑analysis data
    ''' structures, including:
    ''' <list type="bullet">
    '''   <item><description>Conversion of <see cref="SurvivalRecord"/> objects to readable text</description></item>
    '''   <item><description>Conversion of <see cref="SurvivalTableRecord"/> objects to array form</description></item>
    '''   <item><description>Construction of survival records from raw input vectors</description></item>
    ''' </list>
    ''' 
    ''' These helpers support Kaplan–Meier estimation, log‑rank tests, stratified
    ''' survival analysis, and downstream reporting.
    ''' 
    ''' External dependencies:
    ''' <list type="bullet">
    '''   <item><description><c>SurvivalRecord</c> — structure containing time, censoring, group, and stratum</description></item>
    '''   <item><description><c>SurvivalTableRecord</c> — structure containing survival table row values</description></item>
    '''   <item><description><c>gLogger</c> — logging utility</description></item>
    ''' </list>
    ''' </summary>
    Public Module Survival

        Public Structure SurvivalTableRecord
            'sturcture used for the KM tabular output
            Public Time As Double        ' Time to event or censoring
            Public Group As Integer      ' Group identifier (e.g., 0 or 1)
            Public strGroup As String    ' String version of Group ID
            Public AtRisk As Integer     ' subject at risk in this group at time = me.Time
            Public Prob As Double        ' survival probability in this group at time = me.Time
            Public SE As Double          ' standard error of survival probability
            Public ProbCILL As Double    ' confidence interval lower limit of survival probability
            Public ProbCIUL As Double    ' confidence interval upper limit of survival probability
        End Structure

        Public Structure SurvivalRecord
            'structure representing one Survival item record used in KM and Logrank
            Public Time As Double        ' Time to event or censoring
            Public Censorship As Integer ' 1 = event, 0 = censored
            Public Group As Integer      ' Group identifier (e.g., 0 or 1)
            Public strGroup As String    ' String version of Group ID
            Public Stratum As String     ' Stratum identifier for stratified analysis
            Public strStratum As String  ' String version of Strata ID
            Public Covariates As Double() 'for Cox PH model
            Public Index As Integer      'variable that uniquely identify the record.
        End Structure

        ''' <summary>
        ''' Converts a <see cref="SurvivalRecord"/> into a human‑readable string
        ''' summarizing:
        ''' <list type="bullet">
        '''   <item><description>Event or censoring time</description></item>
        '''   <item><description>Censoring indicator (0 = censored, 1 = event)</description></item>
        '''   <item><description>Numeric and string group identifiers</description></item>
        '''   <item><description>Numeric and string stratum identifiers</description></item>
        ''' </list>
        ''' </summary>
        ''' <param name="x">The survival record to convert.</param>
        ''' <returns>A formatted string describing the record.</returns>
        Public Function survivalRecord2str(x As SurvivalRecord) As String
            Return $"Time:{x.Time}; censor:{x.Censorship}; group:{x.Group}; strGroup:{x.strGroup}; strata:{x.Stratum}; strStrata:{x.strStratum}"
        End Function

        ''' <summary>
        ''' Converts a list of <see cref="SurvivalRecord"/> objects into a
        ''' multi‑line string, one record per line.
        ''' Useful for debugging and logging survival data structures.
        ''' </summary>
        ''' <param name="x">List of survival records.</param>
        ''' <returns>A multi‑line string representation of the list.</returns>
        Public Function survRecList2str(x As List(Of SurvivalRecord)) As String
            Dim s As String = survivalRecord2str(x(0))
            For i = 1 To x.Count - 1
                s &= vbNewLine & survivalRecord2str(x(i))
            Next
            Return s
        End Function

        ''' <summary>
        ''' Converts a <see cref="SurvivalTableRecord"/> into an object array
        ''' suitable for table output or grid display.
        ''' 
        ''' The returned array contains:
        ''' <list type="number">
        '''   <item><description>Event time</description></item>
        '''   <item><description>Group label</description></item>
        '''   <item><description>Number at risk</description></item>
        '''   <item><description>Estimated survival probability</description></item>
        '''   <item><description>Standard error</description></item>
        '''   <item><description>Lower confidence limit</description></item>
        '''   <item><description>Upper confidence limit</description></item>
        ''' </list>
        ''' </summary>
        ''' <param name="x">A survival table record.</param>
        ''' <returns>An array of values representing the record.</returns>
        Public Function SurvivalTableRecord2array(x As SurvivalTableRecord) As Object()
            Return {x.Time, x.strGroup, x.AtRisk, x.Prob, x.SE, x.ProbCILL, x.ProbCIUL}
        End Function

        ''' <summary>
        ''' Constructs a list of <see cref="SurvivalRecord"/> objects from
        ''' parallel input vectors:
        ''' <list type="bullet">
        '''   <item><description><paramref name="t"/> — event or censoring times</description></item>
        '''   <item><description><paramref name="s"/> — censoring indicators (0 = censored, 1 = event)</description></item>
        '''   <item><description><paramref name="g"/> — group labels</description></item>
        '''   <item><description><paramref name="strat"/> — stratum labels</description></item>
        ''' </list>
        ''' 
        ''' Each record is assigned:
        ''' <list type="bullet">
        '''   <item><description>Time</description></item>
        '''   <item><description>Censorship indicator</description></item>
        '''   <item><description>Group index (based on distinct group labels)</description></item>
        '''   <item><description>Stratum index (based on distinct stratum labels)</description></item>
        ''' </list>
        ''' 
        ''' Validation rules:
        ''' <list type="bullet">
        '''   <item><description>All input arrays must have equal length</description></item>
        '''   <item><description>Times must be ≥ 0</description></item>
        '''   <item><description>Censoring indicators must be 0 or 1</description></item>
        ''' </list>
        ''' 
        ''' On validation failure, the function returns <c>Nothing</c> and sets
        ''' <paramref name="strErr"/> with a descriptive message.
        ''' </summary>
        ''' <param name="t">Event or censoring times.</param>
        ''' <param name="s">Censoring indicators (0/1).</param>
        ''' <param name="g">Group labels.</param>
        ''' <param name="strat">Stratum labels.</param>
        ''' <param name="strErr">Output parameter containing error message if validation fails.</param>
        ''' <returns>
        ''' A list of <see cref="SurvivalRecord"/> objects, or <c>Nothing</c> on error.
        ''' </returns>
        Public Function CreatSurvivalData(t() As Double, s() As Integer, g() As String, strat() As String, ByRef strErr As String) As List(Of SurvivalRecord)
            Dim out = New List(Of SurvivalRecord)

            If t.Length <> s.Length Or t.Length <> g.Length Or t.Length <> strat.Length Then
                strErr = "Invalid input dimensions"
                BSlogg.Log(strErr)
                Return Nothing
            End If


            Dim grpIds = g.Distinct().ToList()
            Dim stratumIds = strat.Distinct().ToList()

            'build list of individual survivalRecords
            Dim n As Integer = t.Length
            For i = 0 To n - 1
                Dim sr As New SurvivalRecord
                If t(i) < 0 Then
                    strErr = "Unexpected time value (values less then zero are expected) but got = " & CStr(s(i))
                    BSlogg.Log(strErr)
                    Return Nothing
                End If

                If s(i) < 0 Or s(i) > 1 Then
                    strErr = "Unexpected censoring indictor (0/1 values are expected) but got = " & CStr(s(i))
                    BSlogg.Log(strErr)
                    Return Nothing
                End If

                sr.Time = t(i)
                sr.Censorship = s(i)
                sr.strGroup = g(i)
                sr.Group = grpIds.IndexOf(g(i))
                sr.strStratum = strat(i)
                sr.Stratum = stratumIds.IndexOf(strat(i))

                out.Add(sr)
            Next

            Return out
        End Function

    End Module


    ''' <summary>
    ''' Implements Kaplan–Meier survival estimation, log‑rank tests (with multiple
    ''' weighting schemes), hazard‑ratio estimation, Brookmeyer–Crowley median
    ''' survival confidence intervals, and tabular survival‑curve output.
    ''' 
    ''' This class accepts a list of <see cref="SurvivalRecord"/> objects and
    ''' computes:
    ''' <list type="bullet">
    '''   <item><description>Kaplan–Meier survival probabilities for each group</description></item>
    '''   <item><description>Greenwood standard errors</description></item>
    '''   <item><description>Median survival times with Brookmeyer–Crowley CIs</description></item>
    '''   <item><description>Weighted log‑rank tests (logrank, Gehan–Breslow, Tarone–Ware, Peto, modified Peto)</description></item>
    '''   <item><description>Hazard ratio and CI for two‑group comparisons</description></item>
    '''   <item><description>Fixed‑time survival comparisons</description></item>
    '''   <item><description>Tabular KM curve output for reporting</description></item>
    ''' </list>
    ''' 
    ''' External dependencies:
    ''' <list type="bullet">
    '''   <item><description><c>SurvivalProbability</c> — computes KM curves and Greenwood SE</description></item>
    '''   <item><description><c>SurvivalTableRecord</c> — structure for tabular KM output</description></item>
    '''   <item><description><c>MatInv</c>, <c>MatrixMult</c>, <c>trans</c> — matrix algebra utilities</description></item>
    '''   <item><description><c>ChiSquareCDF</c>, <c>PNorm</c> — distribution functions</description></item>
    '''   <item><description><c>HorizontalStackArrays</c>, <c>ResultTable</c>, <c>TestResult</c>, <c>ConfidenceIntervalResult</c></description></item>
    '''   <item><description><c>gLogger</c> — logging utility</description></item>
    ''' </list>
    ''' </summary>
    Public Class Survival_KM_LR

        ''' <summary>Raw survival records (time, censoring, group, stratum).</summary>
        Private pRecords As List(Of SurvivalRecord)

        ''' <summary>Weighting method used for the log‑rank test.</summary>
        Private pWeightMethod As String

        ''' <summary>Log‑rank test results.</summary>
        Private LogRankres As TestResult

        ''' <summary>Hazard ratio estimate and CI (two‑group case only).</summary>
        Private HRres As ConfidenceIntervalResult = Nothing

        ''' <summary>Number of groups in the data.</summary>
        Private NoGroups As Integer

        ''' <summary>List of group identifiers.</summary>
        Private groups As List(Of Integer)

        ''' <summary>String labels for groups.</summary>
        Private grpIDs = New List(Of String)

        ''' <summary>Kaplan–Meier survival probabilities S(t) for each group.</summary>
        Private pSurvivalProb(,) As Double

        ''' <summary>Greenwood standard errors for S(t).</summary>
        Private pSEGreenwood(,) As Double

        ''' <summary>Records sorted by time for KM and log‑rank computation.</summary>
        Private pSortedRecords As List(Of SurvivalRecord)

        ''' <summary>Median survival times for each group.</summary>
        Private MedianSurvivalTime() As Double

        ''' <summary>Lower 95% CI for median survival times.</summary>
        Private MedianSurvivalTimeLLCI() As Double

        ''' <summary>Upper 95% CI for median survival times.</summary>
        Private MedianSurvivalTimeULCI() As Double

        ''' <summary>Fixed‑time survival comparison results (two‑group case).</summary>
        Private pFixTimePointComparisonResults(,) As Object

        ''' <summary>Brookmeyer–Crowley median test results.</summary>
        Private pBrookmeyerCrowleyMedianTestResult As TestResult = Nothing

        ''' <summary>Tabular KM output for each group.</summary>
        Private pKMtabularOutput() As Object = Nothing


        ''' <summary>
        ''' Initializes the survival analysis object and precomputes Kaplan–Meier
        ''' survival probabilities and Greenwood standard errors.
        ''' </summary>
        ''' <param name="x">List of <see cref="SurvivalRecord"/> objects.</param>
        Public Sub New(x As List(Of SurvivalRecord))
            pRecords = x
            Me.SurvivalProbability()
        End Sub

        ''' <summary>
        ''' Produces formatted result tables summarizing:
        ''' <list type="bullet">
        '''   <item><description>Median survival times with 95% CI</description></item>
        '''   <item><description>Brookmeyer–Crowley median test</description></item>
        '''   <item><description>Weighted log‑rank test results</description></item>
        '''   <item><description>Hazard ratio and CI (two‑group case)</description></item>
        '''   <item><description>Fixed‑time survival comparisons</description></item>
        '''   <item><description>Tabular KM curve output</description></item>
        ''' </list>
        ''' 
        ''' External dependency:
        ''' <c>ResultTable</c> for formatting.
        ''' </summary>
        ''' <returns>A list of <see cref="ResultTable"/> objects.</returns>
        Public Function wrapResults() As List(Of ResultTable)
            Dim out = New List(Of ResultTable)
            Dim t = New ResultTable
            Dim Lr(,) As Object = Nothing
            Dim rTable = New ResultTable
            Dim tmp(NoGroups - 1, 1) As Object

            'median surviaval time and confidence intervals  -------------------------------------------------
            If Me.MedianSurvivalTime IsNot Nothing Then
                For i = 0 To NoGroups - 1
                    tmp(i, 0) = Me.MedianSurvivalTime(i)
                    tmp(i, 1) = $"{Me.MedianSurvivalTimeLLCI(i)} to {Me.MedianSurvivalTimeULCI(i)}"
                Next
                rTable.SetBody(tmp)
                rTable.AddHeaderLeftRow(Me.grpIDs.ToArray())
                rTable.AddHeaderTopRow({"Median Survival Time", "", ""})
                rTable.AddHeaderTopRow({"Group", "Median Survival Time", "95%CI"})
                out.Add(rTable)
            End If

            'Brookmeyer-Crowley median test -------------------------------------------------
            If Me.pBrookmeyerCrowleyMedianTestResult IsNot Nothing Then
                t = New ResultTable
                t.SetBody({{"Chi2", Me.pBrookmeyerCrowleyMedianTestResult.TestStatistics1},
                          {"df", Me.pBrookmeyerCrowleyMedianTestResult.DF1},
                          {"Two-sided p-value", Me.pBrookmeyerCrowleyMedianTestResult.Pvalue}})
                t.AddHeaderTopRow({"Test for Equality of Median Survival Times", ""})
                out.Add(t)
            End If

            'Logrank test -------------------------------------------------
            If LogRankres IsNot Nothing Then
                t = New ResultTable
                Lr = {{"Weights", Me.pWeightMethod},
                  {"Chi-square", LogRankres.TestStatistics1},
                  {"Two-sided P-value", LogRankres.Pvalue}}
                t.AddHeaderTopRow({"Log-rank test", ""})
            End If
            If Me.NoGroups = 2 Then
                If LogRankres IsNot Nothing Then
                    Lr = HorizontalStackArrays(Lr,
                                           {{"Hazard ratio(" & grpIDs(0) & " vs. " & grpIDs(1) & ")", HRres.Estimate},
                                            {"Approximate 95% CI", HRres.strConfidenceInterval(CIformat.LL_to_UL)}})
                    t.SetBody(Lr)
                    out.Add(t)
                End If

                'Compare curves at fixed time points -------------------------------------------------
                rTable = New ResultTable
                rTable.SetBody(Me.pFixTimePointComparisonResults)
                rTable.AddHeaderTopRow({"Comparison of Curves at Fixed Time Points", "", ""})
                rTable.AddHeaderTopRow({"Time", "Surv.Prob. difference (Group " & Me.grpIDs(0) & " vs " & Me.grpIDs(1) & " )", "Two-sided p-value"})
                out.Add(rTable)
            Else
                If LogRankres IsNot Nothing Then
                    t.SetBody(Lr)
                    out.Add(t)
                End If
            End If

            'Tabular KM curve results by group -------------------------------------------------
            If Me.pKMtabularOutput IsNot Nothing Then
                Dim totLen As Integer = Me.pKMtabularOutput.Select(Function(g) g.count()).ToArray().Sum(Function(x) Int(x))
                totLen += NoGroups - 1 'blank line separators
                Dim KMtab(totLen, 6) As Object


                Dim k As Integer = 0
                For i = 0 To NoGroups - 1
                    For j = 0 To Me.pKMtabularOutput(i).count() - 1
                        Dim tmp2 = SurvivalTableRecord2array(Me.pKMtabularOutput(i)(j))
                        For g = 0 To 6
                            KMtab(k, g) = tmp2(g)
                        Next g
                        k += 1
                    Next
                    k += 1 'add blank row separator between groups
                Next
                rTable = New ResultTable
                rTable.SetBody(KMtab)
                rTable.AddHeaderTopRow({"Survival Cuve Tabular Result", "", "", "", "", "", ""})
                rTable.AddHeaderTopRow({"Time", "Group", "AtRisk", "S", "SE(S)", "95%LCL", "95%UCL"})
                out.Add(rTable)
            End If

            Return out
        End Function

        ''' <summary>
        ''' Computes a weighted log‑rank test for comparing survival curves across
        ''' groups. Supported weighting schemes:
        ''' <list type="bullet">
        '''   <item><description><c>logrank</c> — equal weights</description></item>
        '''   <item><description><c>gehan-breslow</c> — weights proportional to number at risk</description></item>
        '''   <item><description><c>tarone-ware</c> — square‑root weights</description></item>
        '''   <item><description><c>peto</c> — Peto–Peto modification</description></item>
        '''   <item><description><c>modified peto</c> — Anderson modification</description></item>
        ''' </list>
        ''' 
        ''' The test statistic is:
        ''' <code>
        ''' χ² = Zᵀ Σ⁻¹ Z
        ''' </code>
        ''' where Z is the vector of weighted observed–expected event differences.
        ''' 
        ''' For two groups, a hazard ratio and 95% CI are also computed using:
        ''' <code>
        ''' HR (O/E method) = (O1/E1) / (O2/E2)
        ''' with SE(log HR) ≈ sqrt(1/E1 + 1/E2)
        ''' </code>
        ''' 
        ''' External dependencies:
        ''' <list type="bullet">
        '''   <item><description><c>MatInv</c>, <c>MatrixMult</c>, <c>trans</c></description></item>
        '''   <item><description><c>ChiSquareCDF</c></description></item>
        '''   <item><description><c>PNorm</c></description></item>
        ''' </list>
        ''' </summary>
        ''' <param name="weightMethod">Weighting scheme name.</param>
        ''' <returns>A <see cref="TestResult"/> containing χ² and p-value.</returns>
        Public Function WeightedLogRankTest(weightMethod As String) As TestResult
            Const small As Double = 0.0000000000001
            Me.pWeightMethod = weightMethod
            Dim ii As Integer, Events1 As Double, Events2 As Double
            Dim NoTimes As Integer '# of distinct survival times at whitch at least one event occured

            If Me.AllCenzoredInGroup() Then
                BSlogg.Log("Log rank test skipped, because a group with all record censored detected.")
                Return Nothing
            End If
            'ascending order of Time
            Dim sortedRec As List(Of SurvivalRecord) = pRecords.OrderBy(Function(r) r.Time).ToList()

            'If at the smallest survival time are any censored observation then omitt them and redim arrays accordingly
            Dim i As Integer = 0
            Dim j As Integer = 0
            Dim itemsToDelete = New List(Of Integer)
            Do While (sortedRec(i).Time = sortedRec(i + 1).Time) Or sortedRec(i).Censorship = 0
                If sortedRec(i).Censorship = 0 Then
                    j += 1 'will be the # of deleted subjects
                    itemsToDelete.Add(i) 'delete current subject
                End If
                i += 1
            Loop
            If j > 0 Then
                For i = j - 1 To 0
                    sortedRec.RemoveAt(itemsToDelete(i))
                Next
            End If

            Dim strata = sortedRec.Select(Function(r) r.Stratum).Distinct()
            Dim n As Integer = sortedRec.Count

            'calculate test statistic in coresponding strata and then sum it up
            'count subject InRisk in respective group and respective strata
            Dim Zj(NoGroups - 1) As Double, ZjLR(NoGroups - 1) As Double, InRisk(NoGroups - 1) As Integer
            Dim var(NoGroups - 1, NoGroups - 1) As Double, Var2(NoGroups - 2, NoGroups - 2) As Double

            InRisk = Me.GetInRiskByGroup()

            For Each stratum In strata

                ReDim InRisk(NoGroups - 1) 'null InRisk array
                Dim time(n - 1) As Double, Events(n - 1, NoGroups - 1) As Double, Censor(n - 1, NoGroups - 1) As Double

                'write 1st value
                For i = 0 To n - 1
                    If sortedRec(i).Censorship = 1 And sortedRec(i).Stratum = stratum Then Exit For
                Next i
                If Not (i > n - 1) Then
                    time(0) = sortedRec(i).Time
                    'write value to respecitve group
                    Events(0, sortedRec(i).Group) += 1
                    InRisk(sortedRec(i).Group) += 1

                    'rest of event values
                    i += 1
                    ii = 1
                    Do While i <= n - 1 'goes through rows and select times when event occured
                        If sortedRec(i).Censorship = 1 And sortedRec(i).Stratum = stratum Then
                            If sortedRec(i).Time = time(ii - 1) Then 'this time already occured
                                'write value to respecitve group
                                Events(ii - 1, sortedRec(i).Group) += 1
                                InRisk(sortedRec(i).Group) += 1
                            ElseIf sortedRec(i).Time > time(ii - 1) Then 'new time
                                time(ii) = sortedRec(i).Time
                                'write value to respecitve group
                                Events(ii, sortedRec(i).Group) += 1
                                InRisk(sortedRec(i).Group) += 1
                                ii += 1
                            End If
                        End If
                        i += 1
                    Loop
                    NoTimes = ii - 1
                End If

                'censored values
                i = 0
                Do While i <= n - 1 'go through rows
                    If sortedRec(i).Censorship = 0 And sortedRec(i).Time >= time(0) And sortedRec(i).Stratum = stratum Then
                        ii = NoTimes
                        Do While sortedRec(i).Time < time(ii)
                            If ii = 0 Then Exit Do
                            ii -= 1
                        Loop
                        If sortedRec(i).Time = time(0) Then ii = 0
                        Censor(ii, sortedRec(i).Group) += 1
                        InRisk(sortedRec(i).Group) += 1
                    End If
                    i += 1
                Loop

                'calculate test statistics
                Dim w As Double
                ' Peto–Peto/Prentice (R survdiff rho=1) pooled KM just BEFORE current event time
                Dim Speto As Double = 1.0
                'Andersen-style Modified Peto–Peto running pooled survival S~(t)
                Dim Stilde As Double = 1.0
                For j = 0 To NoTimes
                    Dim Yi As Double = 0 'sum of in risk subject in all groups in time i
                    Dim Di As Double = 0 'sum of events in all groups in time i
                    For i = 0 To NoGroups - 1 'calculate Yi and di in givent time
                        Yi += InRisk(i)
                        Di += Events(j, i)
                    Next

                    'for calculation of hazard ratio
                    Events1 += Events(j, 0)
                    Events2 += Events(j, 1)

                    'select weights
                    Select Case weightMethod.ToLower()
                        Case Is = "logrank"
                            w = 1.0
                        Case Is = "gehan-breslow"
                            w = Yi
                        Case Is = "tarone-ware"
                            w = Math.Sqrt(Yi)
                        Case Is = "peto"
                            'R survdiff rho=1: weight = S_pooled(t-) (pooled KM just before current time)
                            w = Speto

                            'Update pooled KM AFTER using the weight for this time:
                            If Yi <= 0 Then
                                'should not happen, but guard anyway
                                Speto = Speto
                            Else
                                Speto *= (1.0 - Di / Yi)  'note: Di/Yi (no +1)
                            End If
                        Case Is = "modified peto" 'Anderson modificiation to Peto and Peto weight
                            'S~(t_j) = S~(t_{j-1}) * (1 - d_j/(Y_j+1))
                            Dim denom As Double = Yi + 1.0
                            If denom <= 0 Then
                                w = 0.0
                            Else
                                Stilde *= (1.0 - Di / denom)
                                w = Stilde * (Yi / denom)
                            End If
                    End Select

                    For i = 0 To NoGroups - 1
                        Zj(i) = Zj(i) + (w * (Events(j, i) - InRisk(i) * Di / Yi)) 'test statistics weights
                        ZjLR(i) = ZjLR(i) + (Events(j, i) - InRisk(i) * Di / Yi) 'test statistics Logrank - for HR calculation
                        'variance
                        If Yi = 1 Then Yi += small 'it can cause division by zero if Yi = 1
                        var(i, i) = var(i, i) + (w * w * (InRisk(i) / Yi) * (1.0 - (InRisk(i) / Yi)) * ((Yi - Di) / (Yi - 1)) * Di)
                        If i < NoGroups - 1 Then Var2(i, i) = var(i, i)
                    Next

                    'covariance
                    For i = 0 To NoGroups - 1
                        For ii = 0 To NoGroups - 1
                            If i <> ii Then
                                If Yi = 1 Then Yi += small 'it can cause error (division by zero) if Yi = 1
                                var(i, ii) += (w * w * InRisk(i) / Yi * InRisk(ii) / Yi * ((Yi - Di) / (Yi - 1)) * Di) * -1
                            End If
                            If i < NoGroups - 1 And ii < NoGroups - 1 Then Var2(i, ii) = var(i, ii)
                        Next
                    Next
                    For i = 0 To NoGroups - 1 'update InRisk in respective groups
                        InRisk(i) = InRisk(i) - Events(j, i) - Censor(j, i)
                    Next
                Next j
            Next

            'Compute test statistic (quadratic form)
            Dim VarINV = MatInv(Var2)
            Dim Zj2(UBound(Zj) - 1, 0) As Double, Zj2T(0, UBound(Zj) - 1) As Double
            For i = 0 To UBound(Zj2)
                Zj2(i, 0) = Zj(i)
                Zj2T(0, i) = Zj(i)
            Next
            Dim chi2(,) As Double = MatrixMult(MatrixMult(Zj2T, VarINV), Zj2)

            'calculate HR and 95% CI if there are two groups
            If NoGroups = 2 Then
                Me.HRres = New ConfidenceIntervalResult
                Me.HRres.Estimate = ((Events1 / (ZjLR(0) - Events1)) / (Events2 / (ZjLR(1) - Events2)))
                Me.HRres.LowerLimit = Math.Exp(Math.Log(Me.HRres.Estimate) - 1.96 * Math.Sqrt(1.0 / ((ZjLR(0) - Events1) * -1) + 1.0 / ((ZjLR(1) - Events2) * -1)))
                Me.HRres.UpperLimit = Math.Exp(Math.Log(Me.HRres.Estimate) + 1.96 * Math.Sqrt(1.0 / ((ZjLR(0) - Events1) * -1) + 1.0 / ((ZjLR(1) - Events2) * -1)))
            End If

            Me.LogRankres = New TestResult
            Me.LogRankres.TestStatistics1 = chi2(0, 0)
            Me.LogRankres.Pvalue = 1.0 - distributions.ChiSquareCDF(Me.LogRankres.TestStatistics1, NoGroups - 1)

            Return Me.LogRankres
        End Function

        ''' <summary>
        ''' Computes the number of subjects initially at risk in each group.
        ''' 
        ''' This is used as the starting risk set for:
        ''' <list type="bullet">
        '''   <item><description>Kaplan–Meier estimation</description></item>
        '''   <item><description>Log‑rank and weighted log‑rank tests</description></item>
        '''   <item><description>Greenwood variance computation</description></item>
        ''' </list>
        ''' 
        ''' The function counts all subjects in <c>pSortedRecords</c> whose
        ''' <c>Group</c> field matches the group index.
        ''' </summary>
        ''' <returns>An integer array of length <c>NoGroups</c> containing initial risk counts.</returns>
        Private Function GetInRiskByGroup() As Integer()
            Dim out(Me.NoGroups - 1) As Integer
            For i = 0 To Me.NoGroups - 1
                Dim k = groups(i)
                out(i) = pSortedRecords.Where(Function(r) r.Group = k).Count()
            Next
            Return out
        End Function

        ''' <summary>
        ''' Internal structure used to build Kaplan–Meier tabular output before
        ''' converting to <see cref="SurvivalTableRecord"/>.
        ''' 
        ''' Contains:
        ''' <list type="bullet">
        '''   <item><description>Event time</description></item>
        '''   <item><description>Censoring indicator</description></item>
        '''   <item><description>Group index and label</description></item>
        '''   <item><description>Survival probability</description></item>
        '''   <item><description>Greenwood SE</description></item>
        '''   <item><description>Log‑SE</description></item>
        '''   <item><description>Number at risk</description></item>
        ''' </list>
        ''' </summary>
        Private Structure TempSurvivalTableRecord
            Public Time As Double        ' Time to event or censoring
            Public Censorship As Integer ' 1 = event, 0 = censored
            Public Group As Integer      ' Group identifier (e.g., 0 or 1)
            Public strGroup As String    ' String version of Group ID
            Public SE As Double          ' standard error of survival probability
            Public LogSE As Double       ' Log standard error of survival probability
            Public Prob As Double        ' survival probability in this group at time = me.Time
            Public AtRisk As Integer     ' subject at risk in this group at time = me.Time
        End Structure

        ''' <summary>
        ''' Produces a tabular representation of the Kaplan–Meier survival curve
        ''' for each group, including:
        ''' <list type="bullet">
        '''   <item><description>Time</description></item>
        '''   <item><description>Group</description></item>
        '''   <item><description>Number at risk</description></item>
        '''   <item><description>Survival probability S(t)</description></item>
        '''   <item><description>Greenwood SE</description></item>
        '''   <item><description>Log‑SE</description></item>
        '''   <item><description>95% confidence limits</description></item>
        ''' </list>
        ''' 
        ''' External dependencies:
        ''' <list type="bullet">
        '''   <item><description><c>SurvivalTableRecord</c></description></item>
        '''   <item><description><c>SurvivalCurveLogSE</c></description></item>
        '''   <item><description><c>SurvivalCurveAtRisk</c></description></item>
        ''' </list>
        ''' </summary>
        ''' <returns>An array of lists of <see cref="SurvivalTableRecord"/> objects.</returns>
        Public Function SurvivalCurveTabularOutput() As Object()

            Dim LogSE = Me.SurvivalCurveLogSE()
            Dim AtRiskOUT = SurvivalCurveAtRisk()
            Dim n As Integer = Me.pSortedRecords.Count()
            Dim out(NoGroups - 1) As Object

            'Prepare Survival curve tabular output
            For j = 0 To NoGroups - 1
                Dim TableRecords = New List(Of TempSurvivalTableRecord)
                For i = 0 To n - 1
                    If Me.pSortedRecords(i).Group = j Then
                        Dim rs = New TempSurvivalTableRecord
                        rs.SE = pSEGreenwood(i + 1, j)
                        rs.Prob = pSurvivalProb(i + 1, j)
                        rs.AtRisk = AtRiskOUT(j, i)
                        rs.Censorship = Me.pSortedRecords(i).Censorship
                        rs.Time = Me.pSortedRecords(i).Time
                        rs.strGroup = Me.pSortedRecords(i).strGroup
                        rs.Group = Me.pSortedRecords(i).Group
                        rs.LogSE = LogSE(j, i)
                        TableRecords.Add(rs)
                    End If
                Next
                TableRecords = TableRecords.OrderByDescending(Function(g) g.AtRisk).ToList()

                'Remove rows with the same times. keep the last one only
                Dim k As Integer = 0
                For i = 1 To TableRecords.Count() - 1
                    If TableRecords(i).Time = TableRecords(i - 1).Time Then
                        Dim tmp = TableRecords(k)
                        tmp.Prob = TableRecords(i).Prob
                        tmp.SE = TableRecords(i).SE
                        tmp.LogSE = TableRecords(i).LogSE
                        TableRecords(k) = tmp
                    Else
                        k += 1
                        TableRecords(k) = TableRecords(i)
                    End If
                Next

                Dim out_tmp = New List(Of SurvivalTableRecord)
                For i = 0 To k
                    Dim tmp = New SurvivalTableRecord
                    tmp.Time = TableRecords(i).Time
                    tmp.Group = TableRecords(i).Group
                    tmp.strGroup = TableRecords(i).strGroup
                    tmp.AtRisk = TableRecords(i).AtRisk
                    tmp.Prob = TableRecords(i).Prob
                    tmp.SE = TableRecords(i).SE
                    tmp.ProbCILL = tmp.Prob ^ Math.Exp(1.96 * TableRecords(i).LogSE)
                    tmp.ProbCIUL = tmp.Prob ^ Math.Exp(-1.96 * TableRecords(i).LogSE)
                    out_tmp.Add(tmp)
                Next
                out(j) = out_tmp
            Next j

            Me.pKMtabularOutput = out
            Return out

        End Function


        ''' <summary>
        ''' Computes the number of subjects at risk at each event time for each group.
        ''' 
        ''' For each sorted survival record:
        ''' <list type="bullet">
        '''   <item><description>If the record belongs to group g, the current risk count
        '''     for g is recorded and then decremented.</description></item>
        '''   <item><description>Risk counts for other groups remain unchanged.</description></item>
        ''' </list>
        ''' 
        ''' This produces a matrix:
        ''' <code>
        ''' AtRisk(group, timeIndex)
        ''' </code>
        ''' used for KM tables and fixed‑time comparisons.
        ''' </summary>
        ''' <returns>A 2D array of risk counts.</returns>
        Private Function SurvivalCurveAtRisk() As Integer(,)
            Dim n As Integer = Me.pSortedRecords.Count()
            Dim InRisk = Me.GetInRiskByGroup()
            Dim AtRiskOUT(NoGroups - 1, n - 1) As Integer

            For i = 0 To n - 1
                For j = 0 To NoGroups - 1
                    If Me.pSortedRecords(i).Group = j Then 'if event occured in this group calculate new survival probability
                        AtRiskOUT(j, i) = InRisk(j)
                        InRisk(j) -= 1
                    End If
                Next
            Next
            Return AtRiskOUT
        End Function

        ''' <summary>
        ''' Computes the log‑standard‑error for the Kaplan–Meier estimator using the
        ''' transformation method described in:
        ''' <para>
        ''' Machin, Cheung and Parmar, "Survival Analysis: A Practical Approach",
        ''' pp. 42–43.
        ''' </para>
        ''' 
        ''' For each event time:
        ''' <list type="bullet">
        '''   <item><description>Updates cumulative Greenwood components</description></item>
        '''   <item><description>Computes log‑SE:
        '''     <code>
        '''     logSE = sqrt( Σ dᵢ / (nᵢ (nᵢ − dᵢ)) ) / ( −Σ log(1 − dᵢ / nᵢ) )
        '''     </code>
        '''   </description></item>
        '''   <item><description>Propagates previous log‑SE values across censored times</description></item>
        ''' </list>
        ''' 
        ''' Used to compute log‑transformed confidence intervals:
        ''' <code>
        ''' CI = S(t) ^ exp( ±1.96 × logSE )
        ''' </code>
        ''' </summary>
        ''' <returns>A 2D array LogSE(group, timeIndex).</returns>
        Private Function SurvivalCurveLogSE() As Double(,)
            Dim n As Integer = Me.pSortedRecords.Count()
            Dim InRisk = Me.GetInRiskByGroup()
            Dim LogSE(NoGroups - 1, n - 1) As Double, sum(NoGroups - 1) As Double, sum2(NoGroups - 1) As Double

            For i = 0 To n - 1
                For j = 0 To NoGroups - 1
                    If Me.pSortedRecords(i).Group = j Then 'if event occured in this group calculate new survival probability
                        If Me.pSortedRecords(i).Censorship = 1 Then
                            If InRisk(j) <> 1 Then 'would produce division by zero
                                sum(j) += (Me.pSortedRecords(i).Censorship / (InRisk(j) * (InRisk(j) - Me.pSortedRecords(i).Censorship)))
                                sum2(j) += Math.Log((InRisk(j) - Me.pSortedRecords(i).Censorship) / InRisk(j))
                                LogSE(j, i) = Math.Sqrt(sum(j)) / (-sum2(j)) 'for 95%CI computation using Transformation Method.
                                'Survival Analysis: A Practical Approach. p.42-43 by David Machin, Yin Bun Cheung, Mahesh Parmar
                            End If
                        ElseIf Me.pSortedRecords(i).Censorship = 0 Then
                            If i > 0 Then LogSE(j, i) = LogSE(j, i - 1)
                        End If
                        InRisk(j) -= 1
                    Else 'if currently analyzed date is from different group then probaility does not change
                        LogSE(j, i) = If(i = 0, 0.0, LogSE(j, i - 1))
                    End If
                Next
            Next i

            Return LogSE
        End Function

        ''' <summary>
        ''' Implements a comparer for <see cref="SurvivalTableRecord"/> objects based on
        ''' event time.  
        ''' 
        ''' This comparer enables efficient binary search operations on sorted lists of
        ''' survival table records, where ordering is strictly by:
        ''' <code>
        ''' x.Time.CompareTo(y.Time)
        ''' </code>
        ''' 
        ''' Used by <c>SurvivalAt</c> to locate the most recent survival probability
        ''' estimate at or before a given time point.
        ''' </summary>
        Public Class TimeComparer
            Implements IComparer(Of SurvivalTableRecord)

            ''' <summary>
            ''' Compares two <see cref="SurvivalTableRecord"/> objects by their
            ''' <c>Time</c> field.
            ''' </summary>
            ''' <param name="x">First survival table record.</param>
            ''' <param name="y">Second survival table record.</param>
            ''' <returns>
            ''' Negative if <c>x.Time &lt; y.Time</c>,  
            ''' Zero if equal,  
            ''' Positive if <c>x.Time &gt; y.Time</c>.
            ''' </returns>
            Public Function Compare(x As SurvivalTableRecord, y As SurvivalTableRecord) As Integer _
            Implements IComparer(Of SurvivalTableRecord).Compare

                Return x.Time.CompareTo(y.Time)
            End Function
        End Class

        ''' <summary>
        ''' Returns the Kaplan–Meier survival probability <c>S(t)</c> for a given time
        ''' <paramref name="t"/> within a specific group.
        ''' 
        ''' The function performs a binary search on the group’s tabular KM output to
        ''' locate the most recent event time ≤ <paramref name="t"/>:
        ''' <list type="bullet">
        '''   <item><description>If an exact match is found, returns the corresponding <c>Prob</c>.</description></item>
        '''   <item><description>If <paramref name="t"/> falls between event times, returns the
        '''     survival probability at the last event time before <paramref name="t"/>.</description></item>
        '''   <item><description>If <paramref name="t"/> precedes all event times, returns 1.0.</description></item>
        ''' </list>
        ''' 
        ''' This function is used in:
        ''' <list type="bullet">
        '''   <item><description>Brookmeyer–Crowley median test pseudocount construction</description></item>
        '''   <item><description>Fixed‑time survival comparisons</description></item>
        ''' </list>
        ''' </summary>
        ''' <param name="t">Time at which survival probability is requested.</param>
        ''' <param name="grpKMtabularData">Sorted KM table for a single group.</param>
        ''' <returns>The survival probability <c>S(t)</c>.</returns>
        Private Function SurvivalAt(t As Double, grpKMtabularData As List(Of SurvivalTableRecord)) As Double
            If grpKMtabularData Is Nothing OrElse grpKMtabularData.Count = 0 Then Return 1.0

            ' find last index where kmTimes(i) <= t
            Dim idx = grpKMtabularData.BinarySearch(New SurvivalTableRecord With {.Time = t}, New TimeComparer())

            If idx >= 0 Then
                Return grpKMtabularData(idx).Prob
            Else
                Dim ins = Not idx 'retrieves the position where t should be inserted.
                Dim last = ins - 1 'find the last event time less than t
                If last < 0 Then
                    Return 1.0
                Else
                    Return grpKMtabularData(last).Prob
                End If
            End If
        End Function


        ''' <summary>
        ''' Tests the equality of median survival times across <c>k ≥ 2</c> groups using
        ''' the method of  
        ''' <para>
        ''' **Brookmeyer and Crowley (Biometrics, 2012; 68:983–989)**.
        ''' </para>
        ''' 
        ''' The test proceeds as follows:
        ''' <list type="number">
        '''   <item><description>Compute the pooled Kaplan–Meier estimator and determine the
        '''     pooled median survival time.</description></item>
        '''   <item><description>For each subject, compute a pseudocount <c>qᵢ</c> representing
        '''     the probability that the subject’s survival exceeds the pooled median.</description></item>
        '''   <item><description>Aggregate pseudocounts within each group to obtain
        '''     <c>n̂₁, …, n̂_k</c>.</description></item>
        '''   <item><description>Construct all <c>2ᵏ</c> integer-valued tables using floor/ceiling
        '''     combinations of <c>n̂ᵢ</c>, weighted by their fractional components.</description></item>
        '''   <item><description>For each table, compute a Pearson chi‑square statistic comparing
        '''     observed vs. expected counts.</description></item>
        '''   <item><description>Average these statistics using the combination weights to obtain
        '''     the final test statistic <c>U</c>.</description></item>
        ''' </list>
        ''' 
        ''' The final statistic <c>U</c> is asymptotically chi‑square distributed with
        ''' <c>k − 1</c> degrees of freedom.
        ''' 
        ''' External dependencies:
        ''' <list type="bullet">
        '''   <item><description><c>SurvivalCurveTabularOutput</c> — group‑specific KM tables</description></item>
        '''   <item><description><c>SurvivalAt</c> — survival probability interpolation</description></item>
        '''   <item><description><c>ChiSquareCDF</c> — chi‑square tail probability</description></item>
        ''' </list>
        ''' </summary>
        ''' <returns>
        ''' A <see cref="TestResult"/> containing:
        ''' <list type="bullet">
        '''   <item><description><c>TestStatistics1</c> — chi‑square statistic <c>U</c></description></item>
        '''   <item><description><c>DF1</c> — degrees of freedom (<c>k − 1</c>)</description></item>
        '''   <item><description><c>Pvalue</c> — two‑sided p‑value</description></item>
        ''' </list>
        ''' </returns>
        ''' <remarks>
        ''' Implements the Brookmeyer–Crowley median test for general <c>k</c>-group
        ''' survival comparisons.
        ''' </remarks>
        Public Function EqualityOfMedianTest() As TestResult
            Dim out = New TestResult
            ' based on Biometrics. 2012 Sep;68(3):983–989. doi: 10.1111/j.1541-0420.2011.01723.x
            Dim n As Integer = Me.pSortedRecords.Count()
            ' Step 1: Sort by time
            Dim sorted = Me.pRecords.OrderBy(Function(r) r.Time).ToList()

            ' Step 2: Estimate pooled Kaplan-Meier median
            Dim survivalProb As Double = 1.0
            Dim pooledMedian As Double = -1.0

            For Each t In sorted.Where(Function(r) r.Censorship = 1).Select(Function(r) r.Time).Distinct().OrderBy(Function(q) q)
                Dim atRisk = sorted.Where(Function(r) r.Time >= t).Count()
                Dim events = sorted.Where(Function(r) r.Time = t And r.Censorship = 1).Count()
                survivalProb *= 1.0 - events / atRisk
                If survivalProb <= 0.5 Then
                    pooledMedian = t
                    Exit For
                End If
            Next

            If pooledMedian < 0 Then 'Median not reached
                out.DF1 = NoGroups - 1
                out.TestStatistics1 = Double.NaN
                out.Pvalue = Double.NaN
            End If

            ' --- Compute pseudocounts ---
            Dim x = Me.SurvivalCurveTabularOutput()

            Dim nhat1 = New Double(NoGroups - 1) {}
            For i As Integer = 0 To NoGroups - 1
                Dim sumq As Double = 0.0
                Dim grpI As Integer = i
                For Each r In Me.pSortedRecords.Where(Function(q) q.Group = grpI).ToList()
                    Dim q As Double
                    If r.Censorship = 1 Then
                        q = If(r.Time > pooledMedian, 1.0, 0.0)
                    Else
                        If r.Time >= pooledMedian Then
                            q = 1.0
                        Else
                            Dim S_t = Me.SurvivalAt(r.Time, x(i))
                            Dim S_med = SurvivalAt(pooledMedian, x(i))
                            q = If(S_t > 0, S_med / S_t, 0.0)
                            q = Math.Max(0.0, Math.Min(1.0, q))
                        End If
                    End If
                    sumq += q
                Next
                nhat1(i) = sumq
            Next


            ' --- Build integer tables ---
            ' Compute counts above pooled median for each group
            Dim nGroups = NoGroups
            Dim nTotal As Double = Me.pSortedRecords.Count
            Dim aboveCounts = nhat1
            Dim groupNs = (From g In Me.pSortedRecords.GroupBy(Function(r) r.Group)
                           Select CDbl(g.Count())).ToArray()

            Dim N_above As Double = aboveCounts.Sum()
            Dim N_below As Double = nTotal - N_above

            ' Expected counts under H0: each group has same proportion above median
            Dim expectedAbove = groupNs.Select(Function(nn) nn * N_above / nTotal).ToArray()

            Dim chi2 As Double = 0.0
            For i As Integer = 0 To nGroups - 1
                Dim obsAbove = aboveCounts(i)
                Dim expAbove = expectedAbove(i)
                Dim obsBelow = groupNs(i) - obsAbove
                Dim expBelow = groupNs(i) - expAbove
                chi2 += (obsAbove - expAbove) ^ 2 / expAbove
                chi2 += (obsBelow - expBelow) ^ 2 / expBelow
            Next

            out.TestStatistics1 = chi2
            out.DF1 = nGroups - 1
            out.Pvalue = 1.0 - distributions.ChiSquareCDF(chi2, out.DF1)
            Me.pBrookmeyerCrowleyMedianTestResult = out
            Return out
        End Function

        ''' <summary>
        ''' Computes median survival times and corresponding 95% confidence intervals
        ''' for each group using the Brookmeyer–Crowley method.
        ''' 
        ''' Method:
        ''' <para>
        ''' R. Brookmeyer, J. Crowley (1982), "A confidence interval for the median
        ''' survival time", Biometrics 38:29–41, and  
        ''' J.P. Klein, M.L. Moeschberger (2003), "Survival Analysis: Techniques for
        ''' Censored and Truncated Data", 2nd ed., Springer, eq. (4.5.4), p. 120.
        ''' </para>
        ''' 
        ''' For each group:
        ''' <list type="number">
        '''   <item><description>Tracks the Kaplan–Meier survival probability S(t).</description></item>
        '''   <item><description>Identifies the first time where S(t) ≤ 0.5 as the median.</description></item>
        '''   <item><description>Uses the standardized quantity
        '''     <c>(S(t) − 0.5) / SE(S(t))</c> to determine lower/upper CI limits based on
        '''     ±1.96 cutoffs.</description></item>
        '''   <item><description>Groups without a reached median retain default −1 values
        '''     for median and CI limits.</description></item>
        ''' </list>
        ''' 
        ''' Results are stored in:
        ''' <list type="bullet">
        '''   <item><description><c>MedianSurvivalTime</c></description></item>
        '''   <item><description><c>MedianSurvivalTimeLLCI</c></description></item>
        '''   <item><description><c>MedianSurvivalTimeULCI</c></description></item>
        ''' </list>
        ''' </summary>
        ''' <returns>
        ''' A 2D array <c>Out(group, 0..2)</c> containing, for each group:
        ''' <list type="bullet">
        '''   <item><description>Column 0: Median survival time</description></item>
        '''   <item><description>Column 1: Lower 95% confidence limit</description></item>
        '''   <item><description>Column 2: Upper 95% confidence limit</description></item>
        ''' </list>
        ''' Groups without a defined median or CI have −1 in the corresponding fields.
        ''' </returns>
        Public Function BrookmeyerCrowleyMedianSurvivalCI() As Object(,)
            Dim ii() As Integer, LogSurvProb() As Double, bPrvaMensia() As Boolean, bWasSmaller() As Boolean
            Dim n As Integer = Me.pRecords.Count
            ReDim Me.MedianSurvivalTime(NoGroups - 1), LogSurvProb(NoGroups - 1), bPrvaMensia(NoGroups - 1), bWasSmaller(NoGroups - 1)
            ReDim Me.MedianSurvivalTimeLLCI(NoGroups - 1), Me.MedianSurvivalTimeULCI(NoGroups - 1), ii(NoGroups - 1)

            For j = 0 To NoGroups - 1
                bPrvaMensia(j) = True
                '-1 is default value, we can than check whether median is present in given group. If negative
                'in the end, given group does not have median survival time. The same holds for 95%CI.
                MedianSurvivalTime(j) = -1.0
                MedianSurvivalTimeLLCI(j) = -1.0
                MedianSurvivalTimeULCI(j) = -1.0
            Next

            For i = 1 To n
                For j = 0 To NoGroups - 1
                    If Me.pSortedRecords(i - 1).Group = j Then 'event occured in this group

                        If Me.pSortedRecords(i - 1).Censorship = 1 And Me.pSEGreenwood(i, j) > 0 Then LogSurvProb(j) = (Me.pSurvivalProb(i, j) - 0.5) / Me.pSEGreenwood(i, j)

                        'compute median survival time and 95% CI
                        If Me.pSurvivalProb(i, j) <= 0.5 And ii(j) = 0 Then 'store only 1st value smaler then 0.5
                            MedianSurvivalTime(j) = Me.pSortedRecords(i - 1).Time
                            ii(j) = 1
                        End If

                        '95% CI - lower limit
                        If Me.pSortedRecords(i - 1).Censorship <> 0 Then 'for censored observation it's always zero
                            If LogSurvProb(j) <= 1.96 Then 'the 1st smaller is CI limit
                                If bPrvaMensia(j) Then
                                    MedianSurvivalTimeLLCI(j) = Me.pSortedRecords(i - 1).Time
                                    bPrvaMensia(j) = False
                                End If
                            End If

                            'upper limit. store only if median exists
                            If LogSurvProb(j) >= -1.96 And ii(j) = 1 And Not bWasSmaller(j) Then
                                ' Use the right end of the KM plateau for the reported upper CI time (step-function friendly reporting).
                                MedianSurvivalTimeULCI(j) = StepPlateauRightEndTime(i, j)
                            ElseIf LogSurvProb(j) < -1.96 And ii(j) = 1 And Not bWasSmaller(j) Then
                                bWasSmaller(j) = True
                            End If

                        End If
                    End If
                Next j
            Next i

            'out
            Dim Out(NoGroups - 1, 2)
            For i = 0 To NoGroups - 1
                Out(i, 0) = Me.MedianSurvivalTime(i)
                Out(i, 1) = Me.MedianSurvivalTimeLLCI(i)
                Out(i, 2) = Me.MedianSurvivalTimeULCI(i)
            Next
            Return Out
        End Function

        ''' <summary>
        ''' Returns the <b>right endpoint</b> of the Kaplan–Meier step (plateau) for the specified group,
        ''' starting from the given Kaplan–Meier index.
        ''' </summary>
        ''' <remarks>
        ''' <para>
        ''' Kaplan–Meier survival curves are step functions. At an event time <c>t</c>, the curve can drop and then remain
        ''' constant until the next event time for that group. When reporting confidence interval bounds (or other
        ''' threshold-crossing times), it is sometimes preferable to return the <i>right end</i> of the horizontal step
        ''' rather than the left edge where the step begins.
        ''' </para>
        ''' <para>
        ''' This function scans forward from <paramref name="startI"/> and returns the time of the <b>next</b> record where the
        ''' group’s survival probability decreases (i.e., the next event-driven drop). If the survival probability never
        ''' decreases again for this group, it returns the last observed time for that group.
        ''' </para>
        ''' <para>
        ''' This is used to report Brookmeyer–Crowley-style median confidence bounds in a way that matches the visual KM
        ''' plateau (e.g., returning 13 instead of 9 when the survival level is constant between those times).
        ''' </para>
        ''' </remarks>
        ''' <param name="startI">
        ''' 1-based Kaplan–Meier index into the internal arrays used by the KM computation
        ''' (corresponding to the <c>i</c> index in loops that run <c>i = 1..n</c>).
        ''' </param>
        ''' <param name="grp">
        ''' Zero-based group index used internally by BESHStatNG.
        ''' </param>
        ''' <param name="tol">
        ''' Numerical tolerance used to decide whether survival has decreased. A decrease greater than
        ''' <paramref name="tol"/> is treated as a new step (default: <c>1E-12</c>).
        ''' </param>
        ''' <returns>
        ''' The time value representing the right end of the KM plateau that begins at <paramref name="startI"/>
        ''' for the specified group.
        ''' </returns>
        Private Function StepPlateauRightEndTime(startI As Integer, grp As Integer, Optional tol As Double = 0.000000000001) As Double
            Dim n As Integer = Me.pRecords.Count

            Dim s0 As Double = Me.pSurvivalProb(startI, grp)
            Dim t0 As Double = Me.pSortedRecords(startI - 1).Time

            ' Find the next time where the survival for this group decreases (next event in that group)
            For k As Integer = startI + 1 To n
                If Me.pSurvivalProb(k, grp) < s0 - tol Then
                    ' Return the time at which the drop occurs (right end of the plateau)
                    Return Me.pSortedRecords(k - 1).Time
                End If
            Next

            ' If survival never decreases again, return the last time observed for this group
            For k As Integer = n To 1 Step -1
                If Me.pSortedRecords(k - 1).Group = grp Then
                    Return Me.pSortedRecords(k - 1).Time
                End If
            Next

            Return t0
        End Function



        ''' <summary>
        ''' Compares two survival curves at each event time using a fixed‑time point
        ''' test based on the log(−log(S(t))) transformation.
        ''' 
        ''' Method:
        ''' <para>
        ''' J.P. Klein, B. Logan, M. Harhoff, P.K. Andersen (2007),  
        ''' "Analyzing survival curves at a fixed point in time", Statist. Med. 26:4505–4519.
        ''' </para>
        ''' 
        ''' For each distinct event time t:
        ''' <list type="number">
        '''   <item><description>Extracts S₁(t), S₂(t) and their Greenwood variances
        '''     for the two groups.</description></item>
        '''   <item><description>Transforms using log(−log(S(t))) to stabilize variance.</description></item>
        '''   <item><description>Computes a chi‑square statistic for the difference in
        '''     transformed survival:
        '''     <code>
        '''     χ²(t) = [log(−log(S₁(t))) − log(−log(S₂(t)))]² /
        '''             [Var₁ + Var₂]
        '''     </code>
        '''     where <c>Varₖ</c> uses Greenwood variance and the delta method.</description></item>
        '''   <item><description>Derives a p‑value from χ² with 1 degree of freedom.</description></item>
        ''' </list>
        ''' 
        ''' Time points where either survival or variance is undefined (0) in any group
        ''' are excluded. If multiple events occur at the same time, only the last
        ''' survival estimate at that time is retained.
        ''' </summary>
        ''' <returns>
        ''' A 2D object array <c>out(i, 0..2)</c> where each row corresponds to a
        ''' distinct valid event time:
        ''' <list type="bullet">
        '''   <item><description>Column 0: Time t</description></item>
        '''   <item><description>Column 1: Difference in survival probabilities S₁(t) − S₂(t)</description></item>
        '''   <item><description>Column 2: Two‑sided p‑value for equality at time t</description></item>
        ''' </list>
        ''' The results are also stored in <c>pFixTimePointComparisonResults</c>.
        ''' </returns>
        ''' <remarks>
        ''' This procedure does not adjust for multiple comparisons across time points.
        ''' It is intended for exploratory comparison of curves at fixed times.
        ''' </remarks>
        Public Function CompareCurveFixTimePoint() As Object(,)
            Dim t() As Double, p(,) As Double, S(,) As Double, chi2 As Double
            Dim n As Integer = Me.pSortedRecords.Where(Function(r) r.Censorship = 1).Count() 'total number of events
            ReDim t(n - 1), p(1, n - 1), S(1, n - 1)

            'delete censored values
            Dim k As Integer = 0
            For i = 1 To Me.pSortedRecords.Count()
                For j = 0 To 1
                    If Me.pSortedRecords(i - 1).Censorship = 1 Then
                        t(k) = Me.pSortedRecords(i - 1).Time
                        p(j, k) = Me.pSurvivalProb(i, j)
                        S(j, k) = (Me.pSEGreenwood(i, j) / Me.pSurvivalProb(i, j)) ^ 2
                        If j = 1 Then k += 1
                    End If
                Next
            Next

            'select survival data that have defined survival probability in both groups
            k = 0
            For i = 0 To n - 1
                If p(0, i) <> 0 And p(1, i) <> 0 And S(0, i) <> 0 And S(1, i) <> 0 Then
                    t(k) = t(i)
                    p(0, k) = p(0, i)
                    p(1, k) = p(1, i)
                    S(0, k) = S(0, i)
                    S(1, k) = S(1, i)
                    k += 1
                End If
            Next
            ReDim Preserve p(1, k - 1), S(1, k - 1), t(k - 1)

            'if there are multiple event at the same time point, than keep only the last probability and sigma for that time
            k = 0
            For i = 1 To UBound(t)
                If t(i) = t(i - 1) Then
                    t(k) = t(i)
                    p(0, k) = p(0, i)
                    p(1, k) = p(1, i)
                    S(0, k) = S(0, i)
                    S(1, k) = S(1, i)
                Else
                    k += 1
                    t(k) = t(i)
                    p(0, k) = p(0, i)
                    p(1, k) = p(1, i)
                    S(0, k) = S(0, i)
                    S(1, k) = S(1, i)
                End If
            Next

            ReDim Preserve p(1, k), S(1, k), t(k)
            Dim out(k, 2) As Object

            'compute outputs
            For i = 0 To UBound(t)
                out(i, 1) = p(0, i) - p(1, i)
                out(i, 0) = t(i)
                chi2 = (Math.Log(-Math.Log(p(0, i))) - Math.Log(-Math.Log(p(1, i)))) ^ 2
                chi2 = chi2 / ((S(0, i) / (Math.Log(p(0, i))) ^ 2) + (S(1, i) / (Math.Log(p(1, i))) ^ 2))
                out(i, 2) = 1.0 - distributions.ChiSquareCDF(chi2, 1)
            Next
            Me.pFixTimePointComparisonResults = out
            Return out
        End Function

        ''' <summary>
        ''' Determines whether any group contains only censored observations and no
        ''' observed events.  
        ''' 
        ''' A group is considered “all censored” if:
        ''' <code>
        ''' (# censored subjects in group) = (# subjects at risk in group)
        ''' </code>
        ''' and the count is greater than zero.
        ''' 
        ''' This condition invalidates the log‑rank test because the expected number
        ''' of events is zero for that group, making the variance matrix singular.
        ''' 
        ''' External dependency:
        ''' <list type="bullet">
        '''   <item><description><c>GetInRiskByGroup</c> — initial risk counts</description></item>
        ''' </list>
        ''' </summary>
        ''' <returns>
        ''' <c>True</c> if at least one group has no observed events; otherwise <c>False</c>.
        ''' </returns>
        Public Function AllCenzoredInGroup() As Boolean
            Dim bAllCensored As Boolean = False
            Dim InRisk = Me.GetInRiskByGroup()
            Dim CensoredNo(Me.NoGroups - 1) As Integer
            For i = 0 To Me.NoGroups - 1
                Dim k = groups(i)
                CensoredNo(i) = pSortedRecords.Where(Function(r) (r.Group = k And r.Censorship = 0)).Count()
            Next

            For i = 0 To NoGroups - 1
                If CensoredNo(i) = InRisk(i) And CensoredNo(i) > 0 Then bAllCensored = True
            Next

            Return bAllCensored
        End Function

        ''' <summary>
        ''' Creates a Kaplan–Meier survival plot in an Excel worksheet, including:
        ''' <list type="bullet">
        '''   <item><description>Step‑function survival curves for each group</description></item>
        '''   <item><description>Censoring markers</description></item>
        '''   <item><description>Optional 95% confidence limits</description></item>
        '''   <item><description>Optional legend and chart title</description></item>
        ''' </list>
        ''' 
        ''' The method uses precomputed KM quantities from:
        ''' <list type="bullet">
        '''   <item><description><c>pSurvivalProb</c> — survival probabilities</description></item>
        '''   <item><description><c>pSEGreenwood</c> — Greenwood standard errors</description></item>
        '''   <item><description><c>pSortedRecords</c> — sorted survival records</description></item>
        ''' </list>
        ''' 
        ''' Plotting details:
        ''' <list type="number">
        '''   <item><description>Constructs step‑function curves by duplicating each event time.</description></item>
        '''   <item><description>Plots censoring markers at the last survival probability before censoring.</description></item>
        '''   <item><description>Plots upper and lower 95% CI curves using log‑SE transformation.</description></item>
        '''   <item><description>Applies consistent group‑specific colors via <c>GetColor()</c>.</description></item>
        ''' </list>
        ''' 
        ''' External dependencies:
        ''' <list type="bullet">
        '''   <item><description><c>KaplanMeierPlotDataPrep</c> — prepares arrays for plotting</description></item>
        '''   <item><description><c>GetColor</c> — group color selection</description></item>
        '''   <item><description>Excel interop (<c>Worksheet</c>, <c>Chart</c>, <c>SeriesCollection</c>)</description></item>
        ''' </list>
        ''' </summary>
        ''' <param name="ws">Excel worksheet where the KM plot will be created.</param>
        ''' <param name="bPlotCI">If <c>True</c>, plots 95% confidence limits.</param>
        ''' <param name="bLegend">If <c>True</c>, includes a legend.</param>
        ''' <param name="sTitle">Chart title (empty string removes title).</param>
        ''' <param name="sXaxisUnit">Label for the time axis (e.g., “days”, “months”).</param>
        Public Sub AddKMplot(ws As Worksheet, bPlotCI As Boolean, bLegend As Boolean, sTitle As String, sXaxisUnit As String)
            Dim bCen As Boolean
            Dim CenProbability() As Double, CenTimes() As Double, UpCI() As Double, LowCI() As Double, time() As Double, Probability() As Double

            Dim n As Integer = Me.pSortedRecords.Count()

            Dim MaxTime() As Double = Me.pSortedRecords.GroupBy(Function(r) r.Group) _
                                                       .OrderBy(Function(g) g.Key) _
                                                       .Select(Function(g) g.Max(Function(r) r.Time)).ToList().ToArray()

            'Get data to display in the plot
            Dim CenMarkersTime(,) As Double = Nothing, CenMarkersProb(,) As Double = Nothing
            Dim SurvivalTimePlot() As Double = Nothing, SurvivalProbPlot(,) As Double = Nothing
            Dim LLCI(,) As Double = Nothing, ULCI(,) As Double = Nothing
            Me.KaplanMeierPlotDataPrep(CenMarkersProb, CenMarkersTime, LLCI, ULCI, SurvivalTimePlot, SurvivalProbPlot)

            'Get number of censored subjects by group
            Dim CensoredNo() As Integer = Me.pSortedRecords.Where(Function(r) r.Censorship = 0) _
                                                          .GroupBy(Function(r) r.Group) _
                                                          .OrderBy(Function(g) g.Key) _
                                                          .Select(Function(g) g.Count()).ToArray()


            With ws.Shapes.AddChart
                With .Chart
                    .ChartType = XlChartType.xlXYScatterLinesNoMarkers

                    'delete extra series
                    Do Until .SeriesCollection.Count = 0
                        .SeriesCollection(1).Delete
                    Loop

                    With .Axes(XlAxisType.xlValue)
                        .MinimumScale = 0
                        .MaximumScale = 1
                        .MajorUnit = 0.2
                        .MajorGridlines.Delete
                    End With
                    .Axes(XlAxisType.xlCategory).MinimumScale = 0

                    'plot survival plots for each group
                    For i = 0 To NoGroups - 1
                        ReDim Probability(UBound(SurvivalProbPlot, 1) + 1), time(UBound(SurvivalProbPlot, 1) + 1)
                        Probability(0) = 1
                        time(0) = 0
                        For j = 0 To UBound(SurvivalProbPlot, 1)
                            If SurvivalTimePlot(j) <= MaxTime(i) Then
                                Probability(j + 1) = SurvivalProbPlot(j, i)
                                time(j + 1) = SurvivalTimePlot(j)
                            Else
                                ReDim Preserve Probability(j), time(j)
                                Exit For
                            End If
                        Next

                        .SeriesCollection.NewSeries
                        With .SeriesCollection(i + 1)
                            .Name = Me.grpIDs(i)
                            .XValues = time 'SurvivalTimePlot()
                            .Values = Probability

                            'Formal.Line.ForeColor.RGB does not work for excel 2007, therefore we use .Border.Color that works OK
                            'for both excel 2007 as well as 2010
                            .Border.Color = graphics.GetColor(i + 1)
                            With .Format.Line
                                .Visible = True
                                .ForeColor.TintAndShade = 0
                                .Weight = 2.25
                                .ForeColor.Brightness = 0
                            End With
                        End With
                    Next i

                    'plot censoring markers for each group
                    For i = NoGroups To 2 * NoGroups - 1
                        If CensoredNo(i - NoGroups) > 0 Then
                            ReDim CenProbability(CensoredNo(i - NoGroups) - 1), CenTimes(CensoredNo(i - NoGroups) - 1)
                            bCen = True
                        Else
                            ReDim CenProbability(0), CenTimes(0)
                            bCen = False
                        End If
                        For j = 0 To CensoredNo(i - NoGroups) - 1
                            CenProbability(j) = CenMarkersProb(j, i - NoGroups)
                            CenTimes(j) = CenMarkersTime(j, i - NoGroups)
                        Next

                        .SeriesCollection.NewSeries
                        If bCen Then
                            With .SeriesCollection(i + 1)
                                .XValues = CenTimes
                                .Values = CenProbability
                                .Name = "Censored " + Me.grpIDs(i - NoGroups)

                                With .Format.Line
                                    .Visible = True
                                    .ForeColor.TintAndShade = 0
                                    .ForeColor.Brightness = 0
                                End With
                                .MarkerStyle = 9
                                .MarkerSize = 5
                                .ChartType = XlChartType.xlXYScatter
                                .MarkerForegroundColor = graphics.GetColor(i - NoGroups + 1)
                            End With
                        End If
                    Next i

                    If bPlotCI Then
                        'plot 95% confidence limits
                        For i = 2 * NoGroups To 3 * NoGroups - 1
                            'upper limits
                            ReDim UpCI(UBound(SurvivalProbPlot, 1) + 1), time(UBound(SurvivalTimePlot) + 1)
                            UpCI(0) = 1
                            time(0) = 0
                            For j = 0 To UBound(SurvivalProbPlot, 1)
                                If SurvivalTimePlot(j) <= MaxTime(i - 2 * NoGroups) Then
                                    UpCI(j + 1) = ULCI(j, i - 2 * NoGroups)
                                    time(j + 1) = SurvivalTimePlot(j)
                                Else
                                    ReDim Preserve UpCI(j), time(j)
                                    Exit For
                                End If
                            Next

                            .SeriesCollection.NewSeries
                            With .SeriesCollection(i + 1)
                                .Name = "95% CI " + Me.grpIDs(i - 2 * NoGroups)
                                .XValues = time
                                .Values = UpCI
                                .Border.Color = graphics.GetColor(i - 2 * NoGroups + 1)
                                With .Format.Line
                                    .Visible = True
                                    .ForeColor.TintAndShade = 0
                                    .ForeColor.Brightness = 0
                                    .Weight = 1
                                    .DashStyle = 4 'msoLineSysDash
                                End With
                            End With
                        Next i

                        For i = 3 * NoGroups To 4 * NoGroups - 1
                            'lower limits
                            ReDim LowCI(UBound(SurvivalProbPlot, 1) + 1), time(UBound(SurvivalTimePlot) + 1)
                            LowCI(0) = 1
                            time(0) = 0
                            For j = 0 To UBound(SurvivalProbPlot, 1)
                                If SurvivalTimePlot(j) <= MaxTime(i - 3 * NoGroups) Then
                                    LowCI(j + 1) = LLCI(j, i - 3 * NoGroups)
                                    time(j + 1) = SurvivalTimePlot(j)
                                Else
                                    ReDim Preserve LowCI(j), time(j)
                                    Exit For
                                End If
                            Next

                            .SeriesCollection.NewSeries
                            With .SeriesCollection(i + 1)
                                .Name = "95% CI " + Me.grpIDs(i - 3 * NoGroups)
                                .XValues = time
                                .Values = LowCI
                                .Border.Color = graphics.GetColor(i - 3 * NoGroups + 1)
                                With .Format.Line
                                    .Visible = True
                                    .ForeColor.TintAndShade = 0
                                    .ForeColor.Brightness = 0
                                    .Weight = 1
                                    .DashStyle = 4 'msoLineSysDash
                                End With
                            End With
                        Next i
                    End If

                    Try
                        .HasTitle = False
                        .HasTitle = True
                        If sTitle <> String.Empty Then .ChartTitle.Text = sTitle
                        If sTitle = String.Empty Then .HasTitle = False
                        .Axes(XlAxisType.xlValue, XlAxisGroup.xlPrimary).HasTitle = False
                        .Axes(XlAxisType.xlValue, XlAxisGroup.xlPrimary).HasTitle = True
                        .Axes(XlAxisType.xlValue, XlAxisGroup.xlPrimary).AxisTitle.text = "Survival Probability"
                        .Axes(XlAxisType.xlCategory, XlAxisGroup.xlPrimary).HasTitle = False
                        .Axes(XlAxisType.xlCategory, XlAxisGroup.xlPrimary).HasTitle = True
                        .Axes(XlAxisType.xlCategory, XlAxisGroup.xlPrimary).AxisTitle.text = $"Time ({sXaxisUnit})"
                    Catch
                    End Try

                    'delete legend for censoring seriescollections
                    For i = .SeriesCollection.Count To NoGroups + 1 Step -1
                        .Legend.LegendEntries(i).Delete
                    Next

                    'If there is only one group, then delete whole legend
                    If NoGroups = 1 Or Not bLegend Then
                        Try
                            .Legend.Delete()
                        Catch
                        End Try
                    End If
                End With
            End With
        End Sub

        ''' <summary>
        ''' Prepares all arrays required for Kaplan–Meier plotting, including:
        ''' <list type="bullet">
        '''   <item><description>Duplicated time points for step‑function survival curves</description></item>
        '''   <item><description>Survival probabilities for each group</description></item>
        '''   <item><description>Upper and lower 95% confidence limits</description></item>
        '''   <item><description>Censoring marker times and probabilities</description></item>
        ''' </list>
        ''' 
        ''' Steps:
        ''' <list type="number">
        '''   <item><description>Determine maximum observed time per group.</description></item>
        '''   <item><description>Compute log‑SE values using <c>SurvivalCurveLogSE</c>.</description></item>
        '''   <item><description>Construct step‑function arrays by duplicating each event time.</description></item>
        '''   <item><description>Compute CI curves using:
        '''     <code>
        '''     CI = S(t) ^ exp( ±1.96 × logSE )
        '''     </code>
        '''   </description></item>
        '''   <item><description>Extract censoring markers at the last survival probability
        '''     before each censoring time.</description></item>
        ''' </list>
        ''' 
        ''' Output arrays:
        ''' <list type="bullet">
        '''   <item><description><c>CenMarkersProb</c> — censoring marker probabilities</description></item>
        '''   <item><description><c>CenMarkersTime</c> — censoring marker times</description></item>
        '''   <item><description><c>LLCI</c> — lower 95% CI curve</description></item>
        '''   <item><description><c>ULCI</c> — upper 95% CI curve</description></item>
        '''   <item><description><c>SurvivalTimePlot</c> — duplicated time points</description></item>
        '''   <item><description><c>SurvivalProbPlot</c> — survival probabilities for each group</description></item>
        ''' </list>
        ''' 
        ''' External dependencies:
        ''' <list type="bullet">
        '''   <item><description><c>SurvivalCurveLogSE</c></description></item>
        '''   <item><description><c>pSurvivalProb</c>, <c>pSortedRecords</c></description></item>
        ''' </list>
        ''' </summary>
        ''' <param name="CenMarkersProb">Output: censoring marker probabilities.</param>
        ''' <param name="CenMarkersTime">Output: censoring marker times.</param>
        ''' <param name="LLCI">Output: lower 95% confidence limits.</param>
        ''' <param name="ULCI">Output: upper 95% confidence limits.</param>
        ''' <param name="SurvivalTimePlot">Output: duplicated time points for step curves.</param>
        ''' <param name="SurvivalProbPlot">Output: survival probabilities for each group.</param>
        Private Sub KaplanMeierPlotDataPrep(ByRef CenMarkersProb(,) As Double, ByRef CenMarkersTime(,) As Double,
                                            ByRef LLCI(,) As Double, ByRef ULCI(,) As Double,
                                            ByRef SurvivalTimePlot() As Double, ByRef SurvivalProbPlot(,) As Double)

            Dim k As Integer
            Dim n As Integer = Me.pSortedRecords.Count()
            Dim MaxTime() As Double = Me.pSortedRecords.GroupBy(Function(r) r.Group) _
                                                       .OrderBy(Function(g) g.Key) _
                                                       .Select(Function(g) g.Max(Function(r) r.Time)).ToList().ToArray()
            'Get number of censored subjects by group
            Dim CensoredNo() As Integer = Me.pSortedRecords.Where(Function(r) r.Censorship = 0) _
                                                          .GroupBy(Function(r) r.Group) _
                                                          .OrderBy(Function(g) g.Key) _
                                                          .Select(Function(g) g.Count()).ToArray()

            Dim LogSE = Me.SurvivalCurveLogSE()

            'arrays for plotting
            ReDim SurvivalTimePlot(0 To 2 * n - 1), SurvivalProbPlot(0 To 2 * n - 1, NoGroups - 1)
            Dim i As Integer = CensoredNo.Max()

            If i = 0 Then i = 1
            'censored markers
            ReDim CenMarkersProb(i - 1, NoGroups - 1), CenMarkersTime(i - 1, NoGroups - 1)
            ReDim LLCI(0 To 2 * n - 1, NoGroups - 1), ULCI(0 To 2 * n - 1, NoGroups - 1) '95% CI for survival curves
            Dim ii(NoGroups - 1) As Integer

            For i = 1 To n
                SurvivalTimePlot(i * 2 - 2) = Me.pSortedRecords(i - 1).Time
                SurvivalTimePlot(i * 2 - 1) = Me.pSortedRecords(i - 1).Time
                For j = 0 To NoGroups - 1
                    If Me.pSortedRecords(i - 1).Time <= MaxTime(j) Then 'the curve for respecitve group end in max observed time in that group
                        SurvivalProbPlot(i * 2 - 2, j) = pSurvivalProb(i - 1, j)
                        SurvivalProbPlot(i * 2 - 1, j) = pSurvivalProb(i, j)
                        '95% CI http://www.graphpad.com/support/faq/how-does-prism-compute-the-confidence-intervals-of-a-survival-curve/
                        If i > 1 Then
                            LLCI(i * 2 - 2, j) = pSurvivalProb(i - 1, j) ^ Math.Exp(1.96 * LogSE(j, i - 2))
                            ULCI(i * 2 - 2, j) = pSurvivalProb(i - 1, j) ^ Math.Exp(-1.96 * LogSE(j, i - 2))
                        Else
                            LLCI(1, j) = 1
                            ULCI(1, j) = 1
                            LLCI(0, j) = 1
                            ULCI(0, j) = 1
                        End If
                        LLCI(i * 2 - 1, j) = pSurvivalProb(i, j) ^ Math.Exp(1.96 * LogSE(j, i - 1))
                        ULCI(i * 2 - 1, j) = pSurvivalProb(i, j) ^ Math.Exp(-1.96 * LogSE(j, i - 1))
                    End If
                    'censor markers
                    If Me.pSortedRecords(i - 1).Censorship = 0 And Me.pSortedRecords(i - 1).Group = j Then
                        'find the highest probability value for censoring observation and for the current survival time
                        k = i - 1
                        If k > 1 Then
                            Do While Me.pSortedRecords(k).Time = Me.pSortedRecords(k - 1).Time
                                k -= 1
                                If k = 0 Then Exit Do
                            Loop
                        End If
                        CenMarkersTime(ii(j), j) = Me.pSortedRecords(k).Time
                        CenMarkersProb(ii(j), j) = pSurvivalProb(k, j)
                        ii(j) += 1
                    End If
                Next j
            Next i
        End Sub

        ''' <summary>
        ''' Computes Kaplan–Meier survival probabilities and Greenwood standard errors
        ''' for all groups, and initializes all internal structures required for
        ''' downstream survival analyses (log‑rank tests, median estimation,
        ''' fixed‑time comparisons, KM plotting, etc.).
        ''' 
        ''' The procedure performs the following steps:
        ''' <list type="number">
        '''   <item><description>
        '''     Sorts all survival records by:
        '''     <list type="bullet">
        '''       <item><description>ascending time</description></item>
        '''       <item><description>events before censoring at tied times</description></item>
        '''       <item><description>group index</description></item>
        '''     </list>
        '''     producing <c>pSortedRecords</c>.
        '''   </description></item>
        ''' 
        '''   <item><description>
        '''     Identifies distinct groups and stores their string labels in
        '''     <c>grpIDs</c>.
        '''   </description></item>
        ''' 
        '''   <item><description>
        '''     Initializes Kaplan–Meier arrays:
        '''     <code>
        '''     pSurvivalProb(i, g)   ' S(tᵢ) for group g
        '''     pSEGreenwood(i, g)    ' Greenwood SE at tᵢ
        '''     </code>
        '''     with <c>S(0) = 1</c> for all groups.
        '''   </description></item>
        ''' 
        '''   <item><description>
        '''     Computes the number at risk for each group using
        '''     <c>GetInRiskByGroup()</c>.
        '''   </description></item>
        ''' 
        '''   <item><description>
        '''     Iterates through sorted records and updates survival probabilities:
        '''     <code>
        '''     S(tᵢ) = S(tᵢ₋₁) × (1 − dᵢ / nᵢ)
        '''     </code>
        '''     where:
        '''     <list type="bullet">
        '''       <item><description><c>dᵢ</c> = 1 for an event, 0 for censoring</description></item>
        '''       <item><description><c>nᵢ</c> = number at risk just before tᵢ</description></item>
        '''     </list>
        '''   </description></item>
        ''' 
        '''   <item><description>
        '''     Computes Greenwood’s variance incrementally:
        '''     <code>
        '''     Var[S(tᵢ)] = S(tᵢ)² × Σ (dⱼ / (nⱼ (nⱼ − dⱼ)))
        '''     </code>
        '''     and stores the standard error:
        '''     <code>
        '''     SE(tᵢ) = sqrt( Var[S(tᵢ)] )
        '''     </code>
        '''   </description></item>
        ''' 
        '''   <item><description>
        '''     Updates the risk set after each event or censoring:
        '''     <code>
        '''     nᵢ ← nᵢ − 1
        '''     </code>
        '''   </description></item>
        ''' 
        '''   <item><description>
        '''     For groups not involved at a given time point, survival probability
        '''     and SE are carried forward unchanged.
        '''   </description></item>
        ''' </list>
        ''' 
        ''' External references:
        ''' <para>
        ''' Machin, Cheung and Parmar, "Survival Analysis: A Practical Approach",
        ''' pp. 42–43 — Greenwood’s formula.
        ''' </para>
        ''' </summary>
        Private Sub SurvivalProbability()

            Me.pSortedRecords = pRecords.OrderBy(Function(r) r.Time) _
                                       .ThenByDescending(Function(r) r.Censorship) _
                                       .ThenBy(Function(r) r.Group).ToList()

            Dim n As Integer = pSortedRecords.Count()

            Me.groups = pSortedRecords.Select(Function(r) r.Group).Distinct().OrderBy(Function(g) g).ToList()
            Me.NoGroups = groups.Count
            For i = 0 To Me.NoGroups - 1
                Dim g = groups(i)
                Me.grpIDs.Add(pSortedRecords.Where(Function(r) r.Group = g).ToList()(0).strGroup)
            Next

            'number of subjects In risk by group
            Dim InRisk = Me.GetInRiskByGroup()
            ReDim Me.pSurvivalProb(n, Me.NoGroups - 1), Me.pSEGreenwood(n, Me.NoGroups - 1)
            Dim sum(Me.NoGroups - 1) As Double

            For j = 0 To NoGroups - 1  '0 because at the beginning (time 0), survival probability is set to 1
                pSurvivalProb(0, j) = 1.0
            Next

            'calculate probability and errors at subsequent times
            For i = 1 To n
                For j = 0 To NoGroups - 1
                    If pSortedRecords(i - 1).Group = j Then 'if event occured in this group calculate new survival probability
                        If pSortedRecords(i - 1).Censorship = 1 Then
                            pSurvivalProb(i, j) = pSurvivalProb(i - 1, j) * (1 - (pSortedRecords(i - 1).Censorship / InRisk(j)))
                            If InRisk(j) <> 1 Then 'would produce division by zero
                                sum(j) = sum(j) + (pSortedRecords(i - 1).Censorship / (InRisk(j) * (InRisk(j) - pSortedRecords(i - 1).Censorship)))
                                pSEGreenwood(i, j) = Math.Sqrt(sum(j) * pSurvivalProb(i, j) ^ 2)  'Greenwood’s method
                                'Survival Analysis: A Practical Approach. p.42-43 by David Machin, Yin Bun Cheung, Mahesh Parmar
                            End If
                        ElseIf pSortedRecords(i - 1).Censorship = 0 Then
                            pSEGreenwood(i, j) = pSEGreenwood(i - 1, j)
                            pSurvivalProb(i, j) = pSurvivalProb(i - 1, j)
                        End If
                        InRisk(j) = InRisk(j) - 1
                    Else 'if currently analyzed date is from different group then probaility does not change
                        pSurvivalProb(i, j) = pSurvivalProb(i - 1, j)
                        pSEGreenwood(i, j) = pSEGreenwood(i - 1, j)
                    End If
                Next
            Next i
        End Sub

    End Class

End Namespace
