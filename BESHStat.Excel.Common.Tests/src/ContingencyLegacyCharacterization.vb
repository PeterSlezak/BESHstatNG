Option Explicit On
Option Strict On
Imports System
Imports System.Collections.Generic
Imports BESHStatNG
Imports BESHStatNG.ExcelCommon

' TEST-ONLY mechanically frozen 8490252 form report bodies. No form/COM constructed.
' Scientific reference status: characterization only, NOT P goldens.
Friend NotInheritable Class ContingencyLegacyCharacterization
    Public Shared Function TwoByTwo(table As Integer(,), options As TwoByTwoReportOptions) As List(Of ResultTable)
        Dim R1C1 = table(0,0), R1C2 = table(0,1), R2C1 = table(1,0), R2C2 = table(1,1)
        Dim alphaValue = options.Alpha
        Dim res As New List(Of ResultTable), t As New ResultTable
            t.SetBody(table)
            t.AddHeaderTopRow({"Analyzed 2x2 table", ""})
            res.Add(t)

            'do the analysis
            If options.Fisher Then
                If R1C1 + R1C2 + R2C1 + R2C2 > 1000 Then
                    Throw New InvalidOperationException("Test oracle excludes oversized exact work")
                Else
                    Dim Fisher = contingencytable.FisherExact2x2(R1C1, R1C2, R2C1, R2C2)
                    t = New ResultTable
                    t.AddHeaderTopRow({"Fisher's Exact Test", ""})
                    t.SetBody({{"one-sided P-value", Fisher.PvalueLowerSide},
                               {"two-sided P-value", Fisher.Pvalue},
                               {"one-sided mid P-value", Fisher.pValueExactLowerSide},
                               {"two-sided mid P-value", Fisher.Pvalue2}})
                    t.AddPvalueToFormat(2)
                    res.Add(t)
                End If
            End If

            If options.ChiSquare Or options.Association Then
                Dim chi2indep = contingencytable.Chi2TESTindependence(table)
                t = New ResultTable
                t.AddHeaderTopRow({"Pearson's Chi-squared Test", ""})
                t.SetBody({{"Chi-Square", chi2indep.Item1.TestStatistics1}, {"two-sided P-value", chi2indep.Item1.Pvalue}})
                t.AddPvalueCellToFormat(2, 2)
                res.Add(t)

                t = New ResultTable
                t.AddHeaderTopRow({"Measures of Nominal Association", ""})
                t.SetBody({{"Cramer's V", chi2indep.Item2},
                           {"Pearson's contingency coefficient", chi2indep.Item3},
                           {"Phi", chi2indep.Item4}})
                res.Add(t)
            End If
            If options.Liddell Then
                Dim Liddel = contingencytable.Liddell_McNemar(table, alphaValue)
                t = New ResultTable
                t.AddHeaderTopRow({"Liddell's Test", ""})
                t.SetBody({{"P-value", Liddel.Item1.Pvalue},
                            {"Risk ratio", Liddel.Item2.Estimate},
                            {Liddel.Item2.CIlabel, Liddel.Item2.strConfidenceInterval(CIformat.LL_to_UL)}})
                t.AddPvalueCellToFormat(1, 2)
                res.Add(t)
            End If
            If options.OddsRatio Then
                Dim Odds = contingencytable.OddsRatio(table, alphaValue)
                t = New ResultTable
                t.AddHeaderTopRow({"Odds Ratio", ""})
                t.SetBody({{"OR", Odds.Item1.Estimate},
                          {Odds.Item1.CIlabel & " (Woolf)", Odds.Item1.strConfidenceInterval(CIformat.LL_to_UL)},
                          {Odds.Item2.CIlabel & " (Cornfield)", Odds.Item2.strConfidenceInterval(CIformat.LL_to_UL)}})
                res.Add(t)
            End If
            If options.RiskRatio Then
                Dim Risk = contingencytable.RiskRatio(table, alphaValue)
                t = New ResultTable
                t.AddHeaderTopRow({"Risk Ratio", ""})
                t.SetBody({{"RR", Risk.Estimate},
                            {Risk.CIlabel, Risk.strConfidenceInterval(CIformat.LL_to_UL)}})
                res.Add(t)
            End If

        Return res
    End Function
    Public Shared Function RxC(tab As Integer(,), options As RxCReportOptions) As List(Of ResultTable)
        Dim res = New List(Of ResultTable), t = New ResultTable
        Dim head(UBound(tab, 2)) As String
        head(0) = "Analyzed table"
        t.AddHeaderTopRow(head)
        t.SetBody(tab)
        res.Add(t)

        'compute outputs
        Dim chi2indep = contingencytable.Chi2TESTindependence(tab)
        t = New ResultTable
        t.AddHeaderTopRow({"Pearson's Chi-squared Test", ""})
        t.SetBody({{"Chi-Square", chi2indep.Item1.TestStatistics1}, {"two-sided P-value", chi2indep.Item1.Pvalue}})
        t.AddPvalueCellToFormat(2, 2)
        res.Add(t)

        If options.Nominal Then
            t = New ResultTable
            t.AddHeaderTopRow({"Measures of Nominal Association", ""})
            t.SetBody({{"Cramer's V", chi2indep.Item2},
                           {"Pearson's contingency coefficient", chi2indep.Item3},
                           {"Phi", chi2indep.Item4}})
            res.Add(t)
        End If

        If options.Exact Then
            Dim fisherP As Object
            Try
                Dim fexact = New contingencytable.FisherExactEngine(tab)
                fexact.Run()
                fisherP = fexact.PValue
            Catch
                fisherP = ".error. not possible to compute"
            End Try

            t = New ResultTable
            t.AddHeaderTopRow({"Fisher-Freeman-Halton Exact Test", ""})
            t.SetBody({{"two-sided P-value", fisherP}})
            t.AddPvalueCellToFormat(1, 2)
            res.Add(t)
        End If

        If options.Ordinal Then
            Dim alphaValue As Double = options.Alpha
            Dim ciLabel As String = $"{(1.0 - alphaValue) * 100.0:0.##}% CI"
            Dim ordinal = contingencytable.cTableORDINALassoc(tab, alphaValue)

            t = New ResultTable
            t.AddHeaderTopRow({"Measures of Ordinal Association", ""})
            t.SetBody({{"Kendall's tau-b", ordinal.Item1.TestStatistics1},
                       {"Std.Err.", ordinal.Item1.DF1},
                       {ciLabel, ordinal.Item1.strSpecialInformation},
                       {"two-sided P-value", ordinal.Item1.Pvalue},
                       {"Kendall's tau-c", ordinal.Item2.TestStatistics1},
                       {"Std.Err.", ordinal.Item2.DF1},
                       {ciLabel, ordinal.Item2.strSpecialInformation},
                       {"two-sided P-value", ordinal.Item2.Pvalue},
                       {"Goodman-Kruskal's Gamma", ordinal.Item3.TestStatistics1},
                       {"Std.Err.", ordinal.Item3.DF1},
                       {ciLabel, ordinal.Item3.strSpecialInformation},
                       {"two-sided P-value", ordinal.Item3.Pvalue},
                       {"Somers' D (columns as dependent var.)", ordinal.Item4.TestStatistics1},
                       {"Std.Err.", ordinal.Item4.DF1},
                       {ciLabel, ordinal.Item4.strSpecialInformation},
                       {"two-sided P-value", ordinal.Item4.Pvalue}})
            t.AddPvalueCellToFormat(4, 2)
            t.AddPvalueCellToFormat(8, 2)
            t.AddPvalueCellToFormat(12, 2)
            t.AddPvalueCellToFormat(16, 2)
            res.Add(t)
        End If

        If options.Trend Then
            Dim Cochran As TestResult = Nothing
            '1st chcek for proper table dimensions.
            'One of the dimensions have to be 2 and table must be larger then 2x2
            Dim rows As Integer = UBound(tab, 1) + 1
            Dim cols As Integer = UBound(tab, 2) + 1
            If Not ((rows = 2) Or (cols = 2)) Or (rows + cols < 5) Then
                Throw New InvalidOperationException("Test oracle excludes invalid trend shape")
            ElseIf rows = 2 Then
                Cochran = contingencytable.CochranArmitage(Transpose(tab))
            Else
                Cochran = contingencytable.CochranArmitage(tab)
            End If

            If Cochran IsNot Nothing Then
                t = New ResultTable
                t.AddHeaderTopRow({"Cochran-Armitage Test for Linear Trend", ""})
                t.SetBody({{"Chi2 for Linear Trend", Cochran.TestStatistics1},
                       {"two-sided P-value for Linear Trend", Cochran.Pvalue},
                       {"Chi2 for Departure from Linear Trend", Cochran.TestStatistics2},
                       {"two-sided P-value for Linear Trend", Cochran.Pvalue2}})
                t.AddPvalueCellToFormat(2, 2)
                t.AddPvalueCellToFormat(4, 2)
                res.Add(t)
            End If
        End If

        Return res
    End Function
    Public Shared Function Proportions(request As ProportionsRequest) As List(Of ResultTable)
        Dim TotalN1 = request.Total1, RespondersN1 = request.Responders1, TotalN2 = request.Total2, RespondersN2 = request.Responders2
        Dim RespondersBoth = request.Both, nres As Integer
        Dim alphaValue = request.Alpha
        Dim ciLabel As String = $"{100.0 * (1.0 - alphaValue):0.##}% CI"
        Dim res As New List(Of ResultTable), t As New ResultTable
            'do the calculations
            If request.Mode = ProportionsMode.SingleSample Then

                If TotalN1 < RespondersN1 Then 'total have to be less then # of responders
                    Throw New InvalidOperationException("Test oracle excludes invalid counts")

                    Return Nothing
                End If
                Dim SingleProp = contingencytable.SingleProportion(RespondersN1, TotalN1, alphaValue)
                If RespondersN1 * 2 > TotalN1 Then
                    nres = TotalN1 - RespondersN1
                Else
                    nres = RespondersN1
                End If
                Dim PtwoTAIL As Double = distributions.BinomDist(nres, TotalN1, 0.5, True) * 2.0
                If PtwoTAIL > 1 Then PtwoTAIL = 1.0

                t.AddHeaderTopRow({"Single proportion", ""})
                t.SetBody({{"Total Number of Subjects", TotalN1},
                            {"Number of Responders", RespondersN1},
                            {"Proportion", SingleProp.Estimate},
                            {ciLabel, SingleProp.strConfidenceInterval(CIformat.LL_to_UL)},
                            {"two-sided P-value", PtwoTAIL}})
                t.AddPvalueCellToFormat(5, 2)
                res.Add(t)

            ElseIf request.Mode = ProportionsMode.Independent Then

                If TotalN1 < RespondersN1 Then
                    Throw New InvalidOperationException("Test oracle excludes invalid counts")

                    Return Nothing
                End If
                If TotalN2 < RespondersN2 Then
                    Throw New InvalidOperationException("Test oracle excludes invalid counts")

                    Return Nothing
                End If

                Select Case request.Hypothesis
                        Case IndependentProportionsHypothesis.Noninferiority
                            Dim margin As Double = request.Margin
                        Dim ni = equivalencetests.TestIndependentProportionsNonInferiority(
                                        controlResponders:=RespondersN1,
                                        controlTotal:=TotalN1,
                                        experimentalResponders:=RespondersN2,
                                        experimentalTotal:=TotalN2,
                                        nonInferiorityMargin:=margin,
                                        alphaOneSided:=alphaValue)

                        Dim niCiLabel As String = $"{100.0 * (1.0 - 2.0 * alphaValue):0.##}% CI for proportion difference"
                            t.AddHeaderTopRow({"Two independent proportions - Noninferiority", ""})
                        t.SetBody({
                                        {"Control / Reference sample size", ni.NumberOfControls},
                                        {"Control / Reference responders", ni.ControlResponders},
                                        {"Control / Reference proportion", ni.ControlProportion},
                                        {"Experimental / Test sample size", ni.NumberOfExperimental},
                                        {"Experimental / Test responders", ni.ExperimentalResponders},
                                        {"Experimental / Test proportion", ni.ExperimentalProportion},
                                        {"Difference (Experimental - Control)", ni.DifferenceExperimentalMinusControl},
                                        {"Noninferiority margin", ni.NonInferiorityMargin},
                                        {"Noninferiority limit", ni.NonInferiorityLimit},
                                        {niCiLabel, ni.TwoSidedEquivalentConfidenceInterval.strConfidenceInterval(CIformat.LL_to_UL)},
                                        {"Lower one-sided confidence limit", ni.LowerOneSidedConfidenceLimit},
                                        {"Z statistic", ni.ZStatistic},
                                        {"One-sided P-value", ni.PValue},
                                        {"Conclusion", ni.Conclusion}
                                    })
                        t.AddPvalueCellToFormat(13, 2)
                        res.Add(t)

                        Case IndependentProportionsHypothesis.Equivalence
                            Dim margin As Double = request.Margin
                        Dim eq = equivalencetests.TestIndependentProportionsEquivalence(
                                    controlResponders:=RespondersN1,
                                    controlTotal:=TotalN1,
                                    experimentalResponders:=RespondersN2,
                                    experimentalTotal:=TotalN2,
                                    lowerMargin:=-margin,
                                    upperMargin:=margin,
                                    alphaOneSided:=alphaValue)

                        Dim eqCiLabel As String = $"{100.0 * (1.0 - 2.0 * alphaValue):0.##}% CI for proportion difference"
                            t.AddHeaderTopRow({"Two independent proportions - Equivalence", ""})
                        t.SetBody({
                                        {"Control / Reference sample size", eq.NumberOfControls},
                                        {"Control / Reference responders", eq.ControlResponders},
                                        {"Control / Reference proportion", eq.ControlProportion},
                                        {"Experimental / Test sample size", eq.NumberOfExperimental},
                                        {"Experimental / Test responders", eq.ExperimentalResponders},
                                        {"Experimental / Test proportion", eq.ExperimentalProportion},
                                        {"Difference (Experimental - Control)", eq.DifferenceExperimentalMinusControl},
                                        {"Lower equivalence margin", eq.LowerMargin},
                                        {"Upper equivalence margin", eq.UpperMargin},
                                        {eqCiLabel, eq.EquivalentConfidenceInterval.strConfidenceInterval(CIformat.LL_to_UL)},
                                        {"Lower TOST Z statistic", eq.LowerComponentStatistic},
                                        {"Lower TOST one-sided P-value", eq.LowerComponentPValue},
                                        {"Upper TOST Z statistic", eq.UpperComponentStatistic},
                                        {"Upper TOST one-sided P-value", eq.UpperComponentPValue},
                                        {"TOST P-value", eq.TostPValue},
                                        {"Conclusion", eq.Conclusion}
                                    })
                        t.AddPvalueCellToFormat(12, 2)
                        t.AddPvalueCellToFormat(14, 2)
                        t.AddPvalueCellToFormat(15, 2)
                        res.Add(t)

                        Case Else
                            Dim TwoIdependent = contingencytable.TwoIndependentProportions(RespondersN1, TotalN1, RespondersN2, TotalN2, alphaValue)

                            ' get counts in format required by fisherexact
                            Dim R1C1 As Integer = RespondersN1
                            Dim R2C1 As Integer = TotalN1 - RespondersN1
                            Dim R1C2 As Integer = RespondersN2
                            Dim R2C2 As Integer = TotalN2 - RespondersN2
                            Dim Fisher = contingencytable.FisherExact2x2(R1C1, R1C2, R2C1, R2C2)

                            t.AddHeaderTopRow({"Two independent proportions", ""})
                        t.SetBody({{"Total Number of Subjects in Sample 1", TotalN1},
                                        {"Number of Responders in Sample 1", RespondersN1},
                                        {"Proportion in Sample 1", RespondersN1 / TotalN1},
                                        {"Total Number of Subjects in Sample 2", TotalN2},
                                        {"Number of Responders in Sample 2", RespondersN2},
                                        {"Proportion in Sample 2", RespondersN2 / TotalN2},
                                        {"Proportions Difference", TwoIdependent.Estimate},
                                        {ciLabel, TwoIdependent.strConfidenceInterval(CIformat.LL_to_UL)},
                                        {"Exact two-sided P-value", Fisher.Pvalue},
                                        {"Exact Mid two-sided P-value", Fisher.Pvalue2}})
                        t.AddPvalueCellToFormat(9, 2)
                        t.AddPvalueCellToFormat(10, 2)
                        res.Add(t)
                    End Select

                ElseIf request.Mode = ProportionsMode.Paired Then

                    If Math.Max(Math.Max(RespondersN1, RespondersN2), RespondersBoth) > TotalN1 Or
                   TotalN1 < RespondersN1 + RespondersBoth Or TotalN1 < RespondersN2 + RespondersBoth Then 'total have to be less then # of responders
                    Throw New InvalidOperationException("Test oracle excludes invalid counts")

                    Return Nothing
                End If
                Dim PairedProp = contingencytable.PairedProportions(TotalN1, RespondersN1, RespondersN2, RespondersBoth, alphaValue)

                'get counts in format required by liddel
                Dim Total As Integer = TotalN1
                Dim R1C1 As Integer = RespondersN1 - RespondersBoth
                Dim R2C1 As Integer = Total - RespondersN1 - RespondersN2 + RespondersBoth
                Dim R1C2 As Integer = RespondersBoth
                Dim R2C2 As Integer = RespondersN2 - RespondersBoth

                Dim table = {{R1C1, R1C2}, {R2C1, R2C2}}
                Dim Liddell = contingencytable.Liddell_McNemar(table)

                t.AddHeaderTopRow({"Two paired proportions", ""})
                t.SetBody({{"Total Number of Subjects", TotalN1},
                            {"Number of Responders in the 1st Category", RespondersN1},
                            {"Proportion in the 1st Category", (RespondersN1 + RespondersBoth) / TotalN1},
                            {"Total Number of Subjects in the 2nd Category", RespondersN2},
                            {"Number of Responders in the 2nd Category", (RespondersN2 + RespondersBoth) / TotalN1},
                            {"Number of Responders in Both Categories", RespondersBoth},
                            {"Proportions Difference", PairedProp.Estimate},
                            {ciLabel, PairedProp.strConfidenceInterval(CIformat.LL_to_UL)},
                            {"Two-sided P-value", Liddell.Item1.Pvalue}})
                t.AddPvalueCellToFormat(9, 2)
                res.Add(t)
            End If

        Return res
    End Function
    Private Shared Function Transpose(tab As Integer(,)) As Integer(,)
        Dim result(tab.GetLength(1) - 1, tab.GetLength(0) - 1) As Integer
        For r = 0 To tab.GetLength(0) - 1
            For c = 0 To tab.GetLength(1) - 1
                result(c, r) = tab(r, c)
            Next
        Next
        Return result
    End Function
End Class
