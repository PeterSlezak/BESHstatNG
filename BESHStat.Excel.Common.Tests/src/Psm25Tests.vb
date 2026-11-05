Option Explicit On
Option Strict On
Option Infer On
Imports System
Imports System.Collections.Generic
Imports System.Globalization
Imports System.Linq
Imports System.Text
Imports System.Xml.Linq
Imports BESHStatNG
Imports BESHStatNG.CausalInference
Imports BESHStatNG.ExcelCommon
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass>
Public Class Psm25Tests
    Public Shared Iterator Function Profiles() As IEnumerable(Of Object())
        For Each culture In {"", "sk-SK"}
            For method = 0 To 4
                For estimand = 0 To 3
                    For profile = 0 To 15 : Yield New Object() {method, estimand, profile, culture} : Next
                Next
                For profile = 16 To 47 : Yield New Object() {method, 0, profile, culture} : Next
            Next
        Next
    End Function
    <DataTestMethod, DynamicData(NameOf(Profiles), DynamicDataSourceType.Method)>
    Public Sub SameCoreOptionsAxesMatchesWeightsStrataAndTables(method As Integer, estimand As Integer, profile As Integer, culture As String)
        Dim saved = CultureInfo.CurrentCulture
        Try
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(culture)
            Dim input = Psm25Fixture.Input(profile), rows = Psm25Fixture.SourceRows(input.RowCount)
            Dim raw = Psm25Fixture.Options(method, estimand, profile), legacy = Psm25Fixture.Options(method, estimand, profile)
            Psm25Legacy.NormalizeFitOptionsForRunMethod(legacy)
            Dim selection As New PsmRequestSelection(raw), options = selection.CreateEffectiveOptions()
            Assert.AreEqual(Spc23Fixture.Hash(legacy), Spc23Fixture.Hash(options), "Exact original normalized options")
            Assert.AreEqual(12345, options.StandardOptions.RandomSeed)
            Dim axis As New PsmAlignedInputAxis(input, rows, {"x1", "site"}, {"x1", "site(B)", "x1:site(B)"}, 3)
            CollectionAssert.AreEqual(rows, axis.SourceRowIds())
            For i = 0 To input.RowCount - 1
                Assert.AreEqual(i, axis.Rows(i).Ordinal) : Assert.AreEqual(rows(i), axis.Rows(i).SourceRow.Value)
                Assert.AreEqual(input.GetId(i), axis.Rows(i).EffectiveId)
            Next
            Dim result As PsmComprehensiveResult = Nothing, failure As Exception = Nothing
            Try
                result = PsmComprehensiveBackend.Fit(input, options)
            Catch ex As Exception
                failure = ex
            End Try
            Dim other As PsmComprehensiveResult = Nothing, previousFailure As Exception = Nothing
            Try
                other = PsmComprehensiveBackend.Fit(input, legacy)
            Catch ex As Exception
                previousFailure = ex
            End Try
            Assert.AreEqual(If(previousFailure Is Nothing, "", previousFailure.GetType().FullName), If(failure Is Nothing, "", failure.GetType().FullName))
            Assert.AreEqual(If(previousFailure Is Nothing, "", previousFailure.Message), If(failure Is Nothing, "", failure.Message))
            Dim node As New XElement("comparison", New XAttribute("method", method), New XAttribute("estimand", estimand), New XAttribute("profile", profile), New XAttribute("culture", culture), New XAttribute("runtime", Environment.Version.ToString()), New XAttribute("status", "NOT_GOLDEN"), New XAttribute("purpose", "SAME_CORE_COMPATIBILITY_ONLY"), New XElement("fixture_sha256", Spc23Fixture.Hash(input)), New XElement("row_ids_sha256", Spc23Fixture.Hash(rows)), New XElement("row_ids", String.Join(",", rows)), New XElement("id_labels_sha256", Spc23Fixture.Hash(input.Ids)), New XElement("exact_axis_sha256", Spc23Fixture.Hash(input.ExactGroupLabels)), New XElement("option_snapshot_sha256", Spc23Fixture.Hash(options)), New XElement("seed", options.StandardOptions.RandomSeed))
            node.Add(New XElement("options", options.StandardOptions.GetType().GetProperties().OrderBy(Function(p) p.Name, StringComparer.Ordinal).Select(Function(p) New XElement("option", New XAttribute("name", p.Name), New XAttribute("type", p.PropertyType.FullName), If(TypeOf p.GetValue(options.StandardOptions, Nothing) Is Double, Spc23Fixture.Bits(CDbl(p.GetValue(options.StandardOptions, Nothing))), Convert.ToString(p.GetValue(options.StandardOptions, Nothing), CultureInfo.InvariantCulture))))))
            If failure IsNot Nothing Then
                node.Add(New XAttribute("classification", "PRESERVED_CORE_REJECTION"), New XElement("exception", New XAttribute("type", failure.GetType().FullName), failure.Message))
            Else
                Assert.AreEqual(Spc23Fixture.Hash(other), Spc23Fixture.Hash(result), "Seed/order/scores/weights/effects/balance/warnings exact deterministic same Core")
                Dim presentation As New PsmResultPresentation(input.RowCount, result)
                Assert.AreEqual(result.Result.Matches.Count, presentation.Matches.Count)
                For i = 0 To presentation.Matches.Count - 1
                    Dim link = result.Result.Matches(i), mapped = presentation.Matches(i)
                    CollectionAssert.AreEqual({link.SetId, link.FocalRowIndex, link.MatchedRowIndex, link.TreatedRowIndex, link.ControlRowIndex}, {mapped.SetId, mapped.FocalOrdinal, mapped.MatchedOrdinal, mapped.TreatedOrdinal, mapped.ControlOrdinal})
                Next
                For i = 0 To input.RowCount - 1
                    If result.Result.MatchingWeights IsNot Nothing Then Assert.AreEqual(Spc23Fixture.Bits(result.Result.MatchingWeights(i)), Spc23Fixture.Bits(presentation.Rows(i).MatchingWeight.Value))
                    If result.Result.BalancingWeights IsNot Nothing Then Assert.AreEqual(Spc23Fixture.Bits(result.Result.BalancingWeights(i)), Spc23Fixture.Bits(presentation.Rows(i).BalancingWeight.Value))
                    Dim observation = result.Result.Observations.FirstOrDefault(Function(o) o.RowIndex = i)
                    If observation IsNot Nothing Then
                        Assert.AreEqual(observation.IncludedByCommonSupport, presentation.Rows(i).IncludedBySupport.Value)
                        Assert.AreEqual(observation.IncludedByTrimming, presentation.Rows(i).IncludedByTrimming.Value)
                    End If
                    If result.CoarsenedExactResult IsNot Nothing Then Assert.AreEqual(result.CoarsenedExactResult.RowStratumIds(i), presentation.Rows(i).CemStratum.Value)
                Next
                Assert.AreEqual(result.Result.Balance.Count, presentation.BalanceRowCount)
                If result.CoarsenedExactResult IsNot Nothing Then
                    CollectionAssert.AreEqual(result.CoarsenedExactResult.Strata.Select(Function(s) s.Key).ToArray(), presentation.CemStrata.Select(Function(s) s.Key).ToArray())
                    CollectionAssert.AreEqual(result.CoarsenedExactResult.Strata.Select(Function(s) s.Retained).ToArray(), presentation.CemStrata.Select(Function(s) s.Retained).ToArray())
                End If
                For Each sheet In PsmReportSelectionPolicy.Plan(True, True, True, presentation.HasLoveRows, presentation.HasCem)
                    Dim summary As Object(,) = {{"Item", "Value"}, {"Rows", input.RowCount}}
                    Dim tables = sheet.BuildTables(input, rows, result, options, summary)
                    Assert.AreEqual(Spc23Fixture.Hash(Psm25Fixture.LegacyTables(sheet.Section, input, rows, result, options, summary)), Spc23Fixture.Hash(tables), sheet.SheetName)
                    node.Add(New XElement("report", New XAttribute("sheet", sheet.SheetName), tables.Select(Function(t) New XElement("table_sha256", Spc23Fixture.Hash(t.ToOutputModel())))))
                Next
                Dim plot As New PsmLovePlotProjection(result.LovePlotRows)
                Assert.AreEqual(Spc23Fixture.Hash(Psm25Legacy.BuildPlotDataTable(result.LovePlotRows)), Spc23Fixture.Hash(plot.BuildDataTable()))
                node.Add(New XAttribute("classification", If(result.Result.ScoreModel.Converged, "POSITIVE_PARITY", "NONCONVERGED")), New XElement("matches", result.Result.Matches.Select(Function(m) New XElement("link", New XAttribute("set", m.SetId), New XAttribute("focal", m.FocalRowIndex), New XAttribute("matched", m.MatchedRowIndex), New XAttribute("treated", m.TreatedRowIndex), New XAttribute("control", m.ControlRowIndex)))), New XElement("scores_sha256", Spc23Fixture.Hash(result.Result.ScoreModel.Scores)), New XElement("weights_sha256", Spc23Fixture.Hash(New Object() {result.Result.MatchingWeights, result.Result.BalancingWeights})), New XElement("balance_rows", result.Result.Balance.Count), New XElement("result_sha256", Spc23Fixture.Hash(result)), New XElement("projection_sha256", Spc23Fixture.Hash(presentation)), New XElement("warnings", result.Warnings.Select(Function(w) New XElement("warning", w))))
            End If
            Console.WriteLine("04A25_XML|" & Convert.ToBase64String(Encoding.UTF8.GetBytes(node.ToString(SaveOptions.DisableFormatting))))
        Finally
            CultureInfo.CurrentCulture = saved
        End Try
    End Sub

    Public Shared Iterator Function PlotProfiles() As IEnumerable(Of Object())
        For i = 0 To 79 : Yield New Object() {i} : Next
    End Function
    <DataTestMethod, DynamicData(NameOf(PlotProfiles), DynamicDataSourceType.Method)>
    Public Sub LoveTableAndSeriesMatchFrozenWindowsPurePlanner(id As Integer)
        Dim rows = Psm25Fixture.LoveRows(id), plot As New PsmLovePlotProjection(rows), legacy = Psm25Legacy.Plan(rows)
        Assert.AreEqual(Spc23Fixture.Hash(Psm25Legacy.BuildPlotDataTable(rows)), Spc23Fixture.Hash(plot.BuildDataTable()))
        Assert.AreEqual(Spc23Fixture.Hash(legacy), Spc23Fixture.Hash(New Object() {plot.YValues, plot.Before, plot.AfterMatching, plot.AfterWeighting, plot.Labels, plot.Threshold, plot.MaximumX}))
        Dim hash = Spc23Fixture.Hash(plot.BuildDataTable())
        If rows IsNot Nothing Then
            For Each row In rows
                If row IsNot Nothing Then row.VariableName = "mutated" : row.SmdBefore = 999
            Next
            rows.Clear()
        End If
        Assert.AreEqual(hash, Spc23Fixture.Hash(plot.BuildDataTable()), "No retained mutable Core row")
        Dim labels = plot.Labels
        If labels.Length > 0 Then labels(0) = "changed"
        Assert.AreEqual(hash, Spc23Fixture.Hash(plot.BuildDataTable()))
        Dim node As New XElement("love", New XAttribute("id", id), New XAttribute("status", "NOT_GOLDEN"), New XAttribute("purpose", "SAME_CORE_COMPATIBILITY_ONLY"), New XElement("table_sha256", hash), New XElement("payload_sha256", Spc23Fixture.Hash(legacy)), New XElement("count", plot.Count), New XElement("threshold_bits", Spc23Fixture.Bits(plot.Threshold)), New XElement("maximum_x_bits", Spc23Fixture.Bits(plot.MaximumX)), New XElement("labels", plot.Labels.Select(Function(label) New XElement("label", label))), New XElement("before", plot.Before.Select(Function(v) New XElement("bits", Spc23Fixture.Bits(v)))), New XElement("matching", plot.AfterMatching.Select(Function(v) New XElement("bits", Spc23Fixture.Bits(v)))), New XElement("weighting", plot.AfterWeighting.Select(Function(v) New XElement("bits", Spc23Fixture.Bits(v)))))
        Console.WriteLine("04A25_LOVE_XML|" & Convert.ToBase64String(Encoding.UTF8.GetBytes(node.ToString(SaveOptions.DisableFormatting))))
    End Sub

    <DataTestMethod><DataRow(0)><DataRow(1)><DataRow(2)><DataRow(3)><DataRow(4)><DataRow(5)><DataRow(6)><DataRow(7)><DataRow(8)><DataRow(9)><DataRow(10)><DataRow(11)><DataRow(12)><DataRow(13)><DataRow(14)><DataRow(15)><DataRow(16)><DataRow(17)><DataRow(18)><DataRow(19)><DataRow(20)><DataRow(21)><DataRow(22)><DataRow(23)><DataRow(24)><DataRow(25)><DataRow(26)><DataRow(27)><DataRow(28)><DataRow(29)><DataRow(30)><DataRow(31)>
    Public Sub AllOutputCombinationsRetainPhaseOrderAndFreshLists(bits As Integer)
        Dim diagnostics = (bits And 1) <> 0, matches = (bits And 2) <> 0, love = (bits And 4) <> 0, available = (bits And 8) <> 0, cem = (bits And 16) <> 0
        Dim expected As New List(Of String) From {"Input Data", "Results"}
        If diagnostics Then expected.AddRange({"Diagnostics", "Row Audit"})
        If love AndAlso available Then expected.Add("Love Plot")
        If matches Then expected.AddRange({"Matched Pairs", "Matched Data"})
        If cem Then expected.Add("CEM")
        Dim plan = PsmReportSelectionPolicy.Plan(diagnostics, matches, love, available, cem)
        CollectionAssert.AreEqual(expected.ToArray(), plan.Select(Function(s) s.SheetName).ToArray())
        Dim a = plan(0).BuildTables(Nothing, Nothing, Nothing, New PsmComprehensiveFitOptions, Nothing)
        a.Clear()
        Assert.AreEqual(1, plan(0).BuildTables(Nothing, Nothing, Nothing, New PsmComprehensiveFitOptions, Nothing).Count)
    End Sub

    <TestMethod>
    Public Sub DetachedAxisAndNestedOptionsDoNotAliasOrCorrectCandidateShapes()
        Dim input = Psm25Fixture.Input(0), ids = Psm25Fixture.SourceRows(input.RowCount)
        Dim axis As New PsmAlignedInputAxis(input, ids, {"x", "site"}, {"site(B)", "x:site(B)"}, 2)
        ids(0) = -1 : input.Ids(0) = "changed" : input.ExactGroupLabels(0) = "changed"
        Assert.AreEqual(101, axis.Rows(0).SourceRow.Value)
        Assert.AreNotEqual("changed", axis.Rows(0).Id)
        Assert.AreNotEqual("changed", axis.Rows(0).ExactLabel)
        Dim copied = axis.SourceRowIds() : copied(0) = -2
        Assert.AreEqual(101, axis.SourceRowIds()(0))
        Dim malformed As New PsmAlignedInputAxis(input, {9, 9}, Nothing, Nothing, 0)
        Assert.AreEqual(2, malformed.CandidateDiagnostics.Count)
        Assert.IsFalse(malformed.Rows(2).SourceRow.HasValue)
        Dim options = Psm25Fixture.Options(4, 0, 0)
        options.CoarseningSpec.CutPoints = {New Double() {0.1, 0.5}, Nothing}
        options.CoarseningSpec.BinCounts = {4, 5}
        Dim snapshot As New PsmRequestSelection(options)
        options.CoarseningSpec.CutPoints(0)(0) = 99
        Dim first = snapshot.CreateEffectiveOptions() : first.CoarseningSpec.BinCounts(0) = 100
        Assert.AreEqual(0.1R, snapshot.SelectedOptions().CoarseningSpec.CutPoints(0)(0))
        Assert.AreEqual(4, snapshot.CreateEffectiveOptions().CoarseningSpec.BinCounts(0))
        Assert.AreEqual(0.000001R, New PsmOptions().LogisticRidgePenalty)
        Assert.AreEqual(0.0000001R, first.StandardOptions.LogisticRidgePenalty)
    End Sub

    <DataTestMethod><DataRow(0)><DataRow(1)><DataRow(2)><DataRow(3)><DataRow(4)>
    Public Sub CapabilitiesAreCoreAuthorityAndNoDisabledTextIsParsed(method As Integer)
        Dim run = CType(method, PsmBackendRunMethod)
        CollectionAssert.AreEqual(PsmMethodCapabilities.SupportedEstimands(run), PsmControlIntent.Estimands(run))
        For Each estimand As PsmEstimand In [Enum].GetValues(GetType(PsmEstimand))
            For Each score As PsmScoreMethod In [Enum].GetValues(GetType(PsmScoreMethod))
                Dim intent As New PsmControlIntent(run, estimand, score)
                Assert.AreEqual(PsmMethodCapabilities.UsesMatchingControls(run), intent.Matching)
                Assert.AreEqual(PsmMethodCapabilities.SupportsDoublyRobust(run, estimand), intent.DoublyRobust)
                Assert.AreEqual(score = PsmScoreMethod.Supplied, intent.Supplied)
            Next
        Next
    End Sub

    <TestMethod>
    Public Sub LargeIdentityAndPlotProjectionsKeepAxesWithoutMatrixCopies()
        Dim input = Psm25Fixture.Input(0), n = 20000
        input.Treatment = Enumerable.Range(0, n).Select(Function(i) CDbl(i Mod 2)).ToArray()
        input.Ids = Nothing : input.CovariateNames = {"x"}
        Dim axis As New PsmAlignedInputAxis(input, Enumerable.Range(100, n).ToArray(), {"x"}, {"x"}, 0)
        Assert.AreEqual(n, axis.Rows.Count) : Assert.AreEqual(20099, axis.Rows(n - 1).SourceRow.Value)
        Dim rows = Enumerable.Range(0, n).Select(Function(i) New PsmLovePlotRow With {.VariableName = "x" & i.ToString(CultureInfo.InvariantCulture), .SmdBefore = -i / 1000.0R}).ToList()
        Dim plot As New PsmLovePlotProjection(rows)
        Assert.AreEqual(n, plot.Count) : Assert.AreEqual("x19999", plot.Labels(0))
    End Sub

    <TestMethod>
    Public Sub LegacyNonfiniteThresholdCellAndAllMissingAxisFallbackAreObservedUnchanged()
        Dim rows As New List(Of PsmLovePlotRow) From {New PsmLovePlotRow With {.VariableName = "missing", .SmdBefore = Double.NaN, .SmdAfterMatching = Double.PositiveInfinity, .SmdAfterWeighting = Double.NegativeInfinity, .Threshold = Double.NaN}}
        Dim projection As New PsmLovePlotProjection(rows), table = projection.BuildDataTable()
        Assert.IsNull(table(1, 2)) : Assert.IsNull(table(1, 3)) : Assert.IsNull(table(1, 4))
        'VB's original Double/Nothing If expression boxes Double zero for a nonfinite threshold.
        Assert.AreEqual(0.0R, CDbl(table(1, 5)))
        rows(0).Threshold = 10
        projection = New PsmLovePlotProjection(rows)
        Assert.AreEqual(10.0R, projection.Threshold) : Assert.AreEqual(0.25R, projection.MaximumX)
        Assert.AreEqual(Spc23Fixture.Hash(Psm25Legacy.BuildPlotDataTable(rows)), Spc23Fixture.Hash(projection.BuildDataTable()))
    End Sub

    <TestMethod>
    Public Sub CaliperDefaultIntentAndSelectedValuesRemainDistinctAcrossAllDistancesAndScales()
        For Each distance As PsmDistanceMetric In [Enum].GetValues(GetType(PsmDistanceMetric))
            For Each scale As PsmCaliperScale In [Enum].GetValues(GetType(PsmCaliperScale))
                Assert.AreEqual(distance = PsmDistanceMetric.MahalanobisWithinPropensityCaliper AndAlso scale = PsmCaliperScale.None, PsmControlIntent.NeedsCaliperDefault(distance, scale))
            Next
        Next
        Dim selected = Psm25Fixture.Options(3, 0, 0)
        selected.StandardOptions.MatchingRatio = 5 : selected.StandardOptions.WithReplacement = True
        Dim snapshot As New PsmRequestSelection(selected)
        Assert.AreEqual(5, snapshot.SelectedOptions().StandardOptions.MatchingRatio)
        Assert.IsTrue(snapshot.SelectedOptions().StandardOptions.WithReplacement)
        Assert.AreEqual(1, snapshot.CreateEffectiveOptions().StandardOptions.MatchingRatio)
        Assert.IsFalse(snapshot.CreateEffectiveOptions().StandardOptions.WithReplacement)
    End Sub

    <TestMethod>
    Public Sub PartialStatusRetainsMissingAndNonfiniteWeightsWithoutRejectionOrSyntheticMatches()
        Dim result As New PsmComprehensiveResult With {.RunMethod = PsmBackendRunMethod.WeightingOnly, .Result = New PsmResult With {.MatchingWeights = {0.0R, Double.NaN}, .BalancingWeights = {Double.PositiveInfinity}}}
        result.Result.Matches.Add(New PsmMatchLink With {.SetId = 99, .FocalRowIndex = 0, .MatchedRowIndex = 20, .TreatedRowIndex = 0, .ControlRowIndex = 20})
        Dim status As New PsmResultPresentation(3, result)
        Assert.AreEqual(1, status.CandidateDiagnostics.Count)
        Assert.AreEqual(99, status.Matches(0).SetId)
        Assert.IsTrue(Double.IsNaN(status.Rows(1).MatchingWeight.Value))
        Assert.IsTrue(Double.IsPositiveInfinity(status.Rows(0).BalancingWeight.Value))
        Assert.IsFalse(status.Rows(2).MatchingWeight.HasValue)
        result.Result.MatchingWeights(0) = 999 : result.Result.Matches(0).SetId = 1
        Assert.AreEqual(0.0R, status.Rows(0).MatchingWeight.Value) : Assert.AreEqual(99, status.Matches(0).SetId)
        Dim empty As New PsmResultPresentation(0, Nothing)
        Assert.AreEqual(0, empty.Matches.Count) : Assert.IsFalse(empty.HasCem) : Assert.IsFalse(empty.HasLoveRows)
        Dim input = Psm25Fixture.Input(0)
        Dim blank As New PsmAlignedInputAxis(input, Psm25Fixture.SourceRows(input.RowCount), Nothing, Nothing, 0)
        input.ExactGroupLabels = Nothing
        Dim absent As New PsmAlignedInputAxis(input, Psm25Fixture.SourceRows(input.RowCount), Nothing, Nothing, 0)
        Assert.IsTrue(blank.HasExactGroups) : Assert.IsFalse(absent.HasExactGroups)
    End Sub
End Class
