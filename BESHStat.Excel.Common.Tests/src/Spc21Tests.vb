Option Explicit On
Option Strict On
Imports System
Imports System.Collections.Generic
Imports System.Globalization
Imports System.Linq
Imports System.Xml.Linq
Imports BESHStatNG
Imports BESHStatNG.ExcelCommon
Imports BESHStatNG.StatisticalProcessControl
Imports Microsoft.VisualStudio.TestTools.UnitTesting

Public NotInheritable Class Spc21Profile
    Public Shared Function Request(id As Integer, portable As Boolean) As SpcFitRequest
        Dim chart = Spc21Fixture.Charts(id Mod 15), edge = If(id < 270, -1, (id - 270) \ 15)
        Dim input = Spc21Fixture.Input(chart, edge), count = If(portable, SpcInputPointPolicy.LogicalPointCount(input), Spc21LegacyPreparation.LogicalPointCount(input))
        Dim source = CType(id Mod 3, SpcParameterSource), method = If(id Mod 2 = 0, SpcControlLimitMethod.ShewhartSigma, SpcControlLimitMethod.ExactProbability)
        If Not Spc21LegacyPreparation.IsAttributeChart(chart) Then method = SpcControlLimitMethod.ShewhartSigma
        Dim sigma = If(edge = 8, Double.NaN, If(edge = 9, 0.0R, 3.0R))
        Dim estimator = CType((id \ 15) Mod 8, SpcWithinSigmaEstimator), natural = CType(id Mod 2, SpcNaturalLimitPolicy)
        Dim mr = If(edge = 10, 1, 2 + id Mod 2), bias = id Mod 2 = 0
        Dim preset = CType((id \ 15) Mod 6, SpcRulePreset)
        If Spc21LegacyPreparation.IsTimeWeightedChart(chart) Then preset = SpcRulePreset.None
        Dim phaseScope = CType(id Mod 4, SpcRulePhaseScope), gap = CType(id Mod 2, SpcSequenceGapBehavior), marking = CType((id \ 2) Mod 2, SpcSignalMarkingMode)
        Dim custom = If(preset = SpcRulePreset.Custom, SpcRuleCatalog.GetRules(SpcRulePreset.Nelson), Array.Empty(Of SpcRuleDefinition)())
        If edge = 11 AndAlso preset = SpcRulePreset.Custom Then custom = {custom(0), custom(0)}
        Dim missing = CType((id \ 90) Mod 3, SpcMissingValuePolicy)
        Dim sources = input.SourceRowIndices, ids = input.SubgroupIds
        Dim indices = Spc21LegacyPreparation.PointIndices(input.Layout, sources, ids)
        Dim map As SpcSourcePointMap = Nothing
        If portable Then
            map = SpcInputPointPolicy.FromRows(input.Layout, sources, ids, rejectBlank:=False)
            CollectionAssert.AreEqual(indices, map.PointIndexByRow.ToArray())
            Assert.AreEqual(count, map.PointCount)
        End If
        Dim rows As New List(Of SpcStageRowIntent)
        If count > 0 Then
            If id Mod 4 = 0 Then
                Dim stages = indices.Select(Function(i) If(i < count \ 2, "Baseline", "Monitoring")).ToArray()
                Dim phases = indices.Select(Function(i) If(i < count \ 2, "Phase I", "Phase II")).ToArray()
                If portable Then
                    rows.AddRange(SpcPhaseStagePolicy.ImportedRows(SpcPhaseStagePolicy.Collapse(map, stages, phases), source))
                Else
                    Dim cleared As Boolean
                    Spc21LegacyPreparation.Imported(indices, count, stages, phases, source, rows, cleared)
                End If
            Else
                rows.AddRange(If(portable, SpcPhaseStagePolicy.Quick(count, count = 1, Math.Max(1, count \ 2), source), Spc21LegacyPreparation.Quick(count, count = 1, Math.Max(1, count \ 2), source)))
            End If
        End If
        Dim stagesCore = rows.Select(Function(r) If(portable, SpcPhaseStagePolicy.Manual(r, count), Spc21LegacyPreparation.ManualStage(r, count))).ToArray()
        Dim flags = indices.Select(Function(i) i = If(count > 18, 17, 0)).ToArray()
        Dim reasons = indices.Select(Function(i) If(i Mod 2 = 0, " reason Ω ", "note")).ToArray()
        Dim exclusions As New List(Of SpcExclusionDefinition)
        If portable Then
            For Each item In SpcExclusionPolicy.Import(map, flags, reasons)
                exclusions.Add(New SpcExclusionDefinition(SpcExclusionPolicy.CoreIndex(item.DisplayPoint, count), CType(1 + id Mod 3, SpcExclusionScope), item.Reason))
            Next
        Else
            For Each item In Spc21LegacyPreparation.Exclusions(indices, count, flags, reasons)
                exclusions.Add(New SpcExclusionDefinition(item.Key - 1, CType(1 + id Mod 3, SpcExclusionScope), item.Value))
            Next
        End If
        If edge = 16 AndAlso exclusions.Count > 0 Then exclusions.Add(exclusions(0))
        Dim history As SpcHistoricalParameters() = {}
        If source <> SpcParameterSource.EstimateFromPhaseI Then
            Dim mean As Double? = If(edge = 17, CType(Double.NaN, Double?), 50.0R), sd As Double? = If(edge = 18, CType(-1.0R, Double?), 0.25R)
            Dim value = If(portable, SpcRequestPreparation.HistoricalRow("", mean, sd, 0.02R, 2.0R, 0.02R), New SpcHistoricalParameters(processMean:=mean, processSigma:=sd, nonconformingProportion:=0.02R, meanDefectCount:=2.0R, meanDefectRate:=0.02R))
            history = {value}
        End If
        Dim spec As New SpcSpecificationLimits()
        If Spc21LegacyPreparation.ChartCanUseSpecificationLines(chart) Then spec = New SpcSpecificationLimits(48.0R, 50.0R, 52.0R)
        Dim parameters As SpcChartParameters
        Select Case chart
            Case SpcChartType.Ewma : parameters = New SpcChartParameters(ewmaLambda:=If(edge = 19, 0.0R, 0.2R), useSteadyStateLimits:=id Mod 2 = 0)
            Case SpcChartType.Cusum : parameters = New SpcChartParameters(cusumReferenceValue:=0.5R, cusumDecisionInterval:=5.0R, headStart:=0.0R)
            Case SpcChartType.MovingAverage : parameters = New SpcChartParameters(movingAverageSpan:=3)
            Case Else : parameters = New SpcChartParameters()
        End Select
        Dim analysis As SpcAnalysisOptions
        If portable Then
            Dim limits As New SpcLimitRequest(source, method, sigma, estimator, natural, mr, bias)
            Dim rules As New SpcRuleRequest(preset, phaseScope, gap, marking, custom)
            analysis = SpcRequestPreparation.Analysis(missing, limits, rules, chart, SpcPhaseStagePolicy.FinishManual(stagesCore, source), exclusions.ToArray())
            Return SpcRequestPreparation.Create(chart, input, analysis, history, spec, parameters, "SPC fixture", " chart Ω ".Trim(), " value ".Trim())
        End If
        Dim oldLimits As New SpcControlLimitOptions With {.ParameterSource = source, .Method = method, .SigmaMultiplier = sigma, .WithinSigmaEstimator = estimator, .NaturalLimitPolicy = natural, .MovingRangeLength = mr, .UseBiasCorrection = bias}
        Dim oldRules As New SpcRuleOptions With {.Preset = preset, .CustomRules = custom, .PhaseScope = phaseScope, .GapBehavior = gap, .MarkingMode = marking}
        analysis = New SpcAnalysisOptions With {.MissingValuePolicy = missing, .ControlLimits = oldLimits, .Rules = oldRules, .Stages = stagesCore, .Exclusions = exclusions.ToArray()}
        Return New SpcFitRequest(chart, input, analysis, history, spec, parameters, "SPC fixture", "chart Ω", "value")
    End Function
    Public Shared Sub Compare(id As Integer)
        Dim oldRequest As SpcFitRequest = Nothing, request As SpcFitRequest = Nothing
        Dim phase = "Preparation", a = Spc21Fixture.CatchAction(Sub() oldRequest = Spc21Profile.Request(id, False)), b = Spc21Fixture.CatchAction(Sub() request = Spc21Profile.Request(id, True))
        Dim root As New XElement("profile", New XAttribute("id", id), New XAttribute("status", "NOT_GOLDEN"), New XAttribute("validation", "SAME_CORE_COMPATIBILITY_ONLY"), New XAttribute("culture", CultureInfo.CurrentCulture.Name), New XAttribute("runtime", Environment.Version.ToString()), New XAttribute("dataset_sha256", Spc21Fixture.DatasetSha), New XAttribute("chart", Spc21Fixture.Charts(id Mod 15)))
        Dim before As SpcFitResult = Nothing, after As SpcFitResult = Nothing
        If a Is Nothing AndAlso b Is Nothing Then
            Assert.AreEqual(Spc21Fixture.Hash(oldRequest), Spc21Fixture.Hash(request), "Complete existing request fields and data.")
            root.Add(New XElement("request_sha256", Spc21Fixture.Hash(request)), New XElement("options", Spc21Fixture.Hash(request.AnalysisOptions)), New XAttribute("layout", request.DataLayout),
                New XElement("source_rows", request.Data.SourceRowIndices.Select(Function(r) New XElement("row", r))),
                New XElement("stages", request.Stages.Select(Function(s) New XElement("stage", New XAttribute("id", s.StageId), New XAttribute("first", s.FirstPointIndex), New XAttribute("last", s.LastPointIndex), New XAttribute("phase", s.Phase), New XAttribute("limit_mode", s.LimitMode), New XAttribute("reference", s.ReferenceStageId)))),
                New XElement("exclusions", request.Exclusions.Select(Function(e) New XElement("exclusion", New XAttribute("point", e.PointIndex), New XAttribute("scope", e.Scope), e.Reason))))
            Dim settings = request.AnalysisOptions, limits = settings.ControlLimits, rules = settings.Rules
            Dim map = SpcInputPointPolicy.FromRows(request.DataLayout, request.Data.SourceRowIndices, request.Data.SubgroupIds, rejectBlank:=False)
            root.Add(New XElement("fixture_sha256", Spc21Fixture.Hash(request.Data)),
                New XElement("option_snapshot", New XAttribute("missing", settings.MissingValuePolicy), New XAttribute("parameter_source", limits.ParameterSource), New XAttribute("limit_method", limits.Method),
                    New XAttribute("sigma", Spc21Fixture.Text(limits.SigmaMultiplier)), New XAttribute("estimator", limits.WithinSigmaEstimator), New XAttribute("natural", limits.NaturalLimitPolicy), New XAttribute("moving_range", limits.MovingRangeLength), New XAttribute("bias", limits.UseBiasCorrection),
                    New XAttribute("preset", rules.Preset), New XAttribute("phase_scope", rules.PhaseScope), New XAttribute("gap", rules.GapBehavior), New XAttribute("marking", rules.MarkingMode), New XElement("custom_rules_sha256", Spc21Fixture.Hash(rules.CustomRules)),
                    New XElement("historical_sha256", Spc21Fixture.Hash(request.HistoricalParameters)), New XElement("specification_sha256", Spc21Fixture.Hash(request.SpecificationLimits)), New XElement("chart_parameters_sha256", Spc21Fixture.Hash(request.ChartParameters))),
                New XElement("point_map", map.Points.Select(Function(p) New XElement("point", New XAttribute("index", p.PointIndex), New XAttribute("display", p.DisplayPoint), New XAttribute("group", If(p.SubgroupId, "")), New XElement("input_rows", p.InputRowIndices.Select(Function(r) New XElement("row", r))), New XElement("source_rows", p.SourceRowIndices.Select(Function(r) New XElement("row", r)))))),
                New XElement("labels_sha256", Spc21Fixture.Hash(request.Data.Labels)), New XElement("sequence_sha256", Spc21Fixture.Hash(request.Data.SequenceValues)))
            phase = "Validate"
            a = Spc21Fixture.CatchAction(Sub() SpcEngine.Validate(oldRequest)) : b = Spc21Fixture.CatchAction(Sub() SpcEngine.Validate(request))
            If a Is Nothing AndAlso b Is Nothing Then
                phase = "Fit"
                a = Spc21Fixture.CatchAction(Sub() before = SpcEngine.Fit(oldRequest)) : b = Spc21Fixture.CatchAction(Sub() after = SpcEngine.Fit(request))
            End If
        End If
        root.Add(New XAttribute("phase", phase))
        If a IsNot Nothing OrElse b IsNot Nothing Then
            Spc21Fixture.SameException(a, b)
            root.Add(New XAttribute("classification", "EXISTING_REJECTION"), New XElement("exception", New XAttribute("type", b.GetType().FullName), b.Message))
        Else
            Assert.IsNotNull(before) : Assert.IsNotNull(after)
            Assert.AreEqual(Spc21Fixture.Hash(before.Panels), Spc21Fixture.Hash(after.Panels), "All point, limit, source/stage/exclusion/signal metadata.")
            Dim expected = Spc21Fixture.TableHash(Spc21Fixture.Tables(before)), actual = Spc21Fixture.TableHash(Spc21Fixture.Tables(after))
            Assert.AreEqual(expected, actual, "All report cells/types/formatting/order except three named volatile execution fields.")
            root.Add(New XAttribute("classification", "CORE_RETURNED"), New XElement("panels_sha256", Spc21Fixture.Hash(after.Panels)), New XElement("tables_sha256", actual), New XElement("signal_count", after.SignalCount),
                New XElement("panels", after.Panels.Select(Function(p) New XElement("panel", New XAttribute("kind", p.PanelType), New XAttribute("points", p.PointCount), New XAttribute("signals", p.SignalCount), New XAttribute("sha256", Spc21Fixture.Hash(p))))),
                New XElement("tables", Spc21Fixture.Tables(after).Select(Function(t) New XElement("table", New XAttribute("rows", t.ToOutputModel().RowCount), New XAttribute("columns", t.ToOutputModel().ColumnCount), CStr(t.ToOutputModel().Values(0, 0))))))
        End If
        Spc21Fixture.Emit(root)
    End Sub
End Class

<TestClass>
Public Class Spc21Tests
    Public Shared Iterator Function Profiles() As IEnumerable(Of Object())
        For id = 0 To 569 : Yield New Object() {id} : Next
    End Function
    <DataTestMethod><DynamicData(NameOf(Profiles), DynamicDataSourceType.Method)>
    Public Sub LegacyAndPortableRequestFitTablesRemainSameCore(id As Integer)
        Dim culture = CultureInfo.CurrentCulture
        Try
            For Each name In New String() {"", "sk-SK"}
                CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(name)
                Spc21Profile.Compare(id)
            Next
        Finally
            CultureInfo.CurrentCulture = culture
        End Try
    End Sub
    <DataTestMethod><DataRow(0)><DataRow(1)><DataRow(2)><DataRow(3)>
    Public Sub FourLayoutsKeepInputSourceAndLogicalAxesApart(layout As Integer)
        Dim sources = {101, 105, 118, 131}, ids = {" A ", "a", "A", "a"}, kind = CType(layout, SpcDataLayout)
        Dim map = SpcInputPointPolicy.FromRows(kind, sources, ids)
        CollectionAssert.AreEqual(Spc21LegacyPreparation.PointIndices(kind, sources, ids), map.PointIndexByRow.ToArray())
        sources(0) = -1 : ids(0) = "changed"
        Assert.AreEqual(101, map.SourceRowIndices(0))
        If kind = SpcDataLayout.StackedObservations Then
            Assert.AreEqual(2, map.PointCount) : Assert.AreEqual("A", map.Points(0).SubgroupId)
            CollectionAssert.AreEqual(New Integer() {101, 118}, map.Points(0).SourceRowIndices.ToArray())
        Else
            Assert.AreEqual(4, map.PointCount)
        End If
        Assert.ThrowsException(Of NotSupportedException)(Sub() DirectCast(map.PointIndexByRow, IList(Of Integer))(0) = -1)
    End Sub
    Public Shared Iterator Function QuickCases() As IEnumerable(Of Object())
        For Each n In {1, 2, 10}
            For Each last In (New Integer() {0, 1, n - 1, n}).Distinct()
                For Each source In {SpcParameterSource.EstimateFromPhaseI, SpcParameterSource.UseHistoricalParameters, SpcParameterSource.DefinedByStage}
                    Yield New Object() {n, last, source}
                Next
            Next
        Next
    End Function
    <DataTestMethod><DynamicData(NameOf(QuickCases), DynamicDataSourceType.Method)>
    Public Sub QuickPhaseRangeAndFailureMessagesStayLegacy(n As Integer, last As Integer, source As SpcParameterSource)
        For Each singleI In {False, True}
            Dim a As List(Of SpcStageRowIntent) = Nothing, b As List(Of SpcStageRowIntent) = Nothing
            Dim x = Spc21Fixture.CatchAction(Sub() a = Spc21LegacyPreparation.Quick(n, singleI, last, source)), y = Spc21Fixture.CatchAction(Sub() b = SpcPhaseStagePolicy.Quick(n, singleI, last, source).ToList())
            If x IsNot Nothing OrElse y IsNot Nothing Then Spc21Fixture.SameException(x, y) Else Assert.AreEqual(Spc21Fixture.Hash(a), Spc21Fixture.Hash(b))
        Next
    End Sub
    Public Shared Iterator Function ImportedCases() As IEnumerable(Of Object())
        For id = 0 To 25 : Yield New Object() {id} : Next
    End Function
    <DataTestMethod><DynamicData(NameOf(ImportedCases), DynamicDataSourceType.Method)>
    Public Sub StageImportCollapseCaseContinuityAndPartialGridTimingMatch(id As Integer)
        Dim indices = {0, 1, 0, 1, 2, 3, 4, 5}, map = SpcInputPointPolicy.FromRows(SpcDataLayout.StackedObservations, Enumerable.Range(2, 8).ToArray(), New String() {"A", "B", "A", "B", "C", "D", "E", "F"})
        Dim stages = New String() {"A", "A", "A", "A", "B", "C", "D", "D"}, phases = New String() {"Phase I", "I", "1", "Phase I", "II", "I", "II", "2"}
        Select Case id Mod 13
            Case 0 : stages = Nothing
            Case 1 : phases = Nothing
            Case 2 : stages(0) = " "
            Case 3 : stages(2) = "a"
            Case 4 : phases(2) = "II"
            Case 5 : phases(0) = "bad"
            Case 6 : phases = Enumerable.Repeat("II", 8).ToArray()
            Case 7 : stages(7) = "A"
            Case 8 : stages(7) = "a"
            Case 9 : stages(6) = " Ω " : stages(7) = "Ω"
            Case 10 : stages = Nothing : phases = {"I", "II", "1", "2", "I", "II", "I", "II"}
            Case 11 : stages = Nothing : phases = Enumerable.Repeat(" Phase  I ", 8).ToArray()
            Case 12 : stages(5) = "b"
        End Select
        Dim source = If(id >= 13, SpcParameterSource.UseHistoricalParameters, SpcParameterSource.EstimateFromPhaseI)
        Dim a As New List(Of SpcStageRowIntent), b As New List(Of SpcStageRowIntent), aclear As Boolean, bclear As Boolean
        Dim x = Spc21Fixture.CatchAction(Sub() Spc21LegacyPreparation.Imported(indices, 6, stages, phases, source, a, aclear))
        Dim y = Spc21Fixture.CatchAction(Sub()
            Dim captured = SpcPhaseStagePolicy.Collapse(map, stages, phases)
            b.Clear() : bclear = True
            For Each row In SpcPhaseStagePolicy.ImportedRows(captured, source) : b.Add(row) : Next
        End Sub)
        Assert.AreEqual(aclear, bclear) : Assert.AreEqual(Spc21Fixture.Hash(a), Spc21Fixture.Hash(b))
        If x IsNot Nothing OrElse y IsNot Nothing Then Spc21Fixture.SameException(x, y)
    End Sub
    <DataTestMethod><DataRow(0)><DataRow(1)><DataRow(2)><DataRow(3)><DataRow(4)><DataRow(5)><DataRow(6)><DataRow(7)><DataRow(8)>
    Public Sub ManualStageBoundsCoreConstructorAndEmptyDefinedSourceKeepOrder(id As Integer)
        Dim row As New SpcStageRowIntent(If(id = 0, "", "Ω"), "", If(id = 1, 0, 1), If(id = 2, 0, If(id = 3, 20, 10)), SpcPhase.PhaseI, If(id >= 4 AndAlso id <= 6, SpcStageLimitMode.UseReferenceStage, SpcStageLimitMode.EstimateFromStageData), If(id = 5, "Ω", If(id = 6, "missing", "")))
        Dim a As SpcStageDefinition = Nothing, b As SpcStageDefinition = Nothing
        Dim x = Spc21Fixture.CatchAction(Sub() a = Spc21LegacyPreparation.ManualStage(row, 10)), y = Spc21Fixture.CatchAction(Sub() b = SpcPhaseStagePolicy.Manual(row, 10))
        If x IsNot Nothing OrElse y IsNot Nothing Then Spc21Fixture.SameException(x, y) Else Assert.AreEqual(Spc21Fixture.Hash(a), Spc21Fixture.Hash(b))
        Assert.ThrowsException(Of ArgumentException)(Sub() SpcPhaseStagePolicy.FinishManual({}, SpcParameterSource.DefinedByStage))
        Assert.AreEqual(0, SpcPhaseStagePolicy.FinishManual({}, SpcParameterSource.EstimateFromPhaseI).Length)
    End Sub
    <DataTestMethod><DataRow(1)><DataRow(2)><DataRow(3)>
    Public Sub ImportedExclusionsUseAnyFlagAndAllReasonsWithOriginalScope(scope As Integer)
        Dim map = SpcInputPointPolicy.FromRows(SpcDataLayout.StackedObservations, {101, 105, 118, 131}, {"A", "B", "A", "B"}), flags = {True, False, False, True}, reasons = {" true Ω ", "false reason", "false Ω", "false reason"}
        Dim old = Spc21LegacyPreparation.Exclusions(map.PointIndexByRow.ToArray(), 2, flags, reasons), actual = SpcExclusionPolicy.Import(map, flags, reasons)
        Assert.AreEqual(old.Count, actual.Count)
        For i = 0 To old.Count - 1
            Assert.AreEqual(old(i).Key, actual(i).DisplayPoint) : Assert.AreEqual(old(i).Value, actual(i).Reason)
            Dim e As New SpcExclusionDefinition(SpcExclusionPolicy.CoreIndex(actual(i).DisplayPoint, 2), CType(scope, SpcExclusionScope), actual(i).Reason)
            Assert.AreEqual(CType(scope, SpcExclusionScope), e.Scope)
        Next
        flags(0) = False : reasons(0) = "mutated"
        StringAssert.Contains(actual(0).Reason, "true Ω") : StringAssert.Contains(actual(0).Reason, "false Ω")
        Assert.ThrowsException(Of ArgumentException)(Sub() SpcExclusionPolicy.CoreIndex(0, 2))
        Assert.ThrowsException(Of ArgumentException)(Sub() SpcExclusionPolicy.CoreIndex(3, 2))
    End Sub
    <TestMethod>
    Public Sub RawRowCountValidationVersusGroupedPointFitRejectionIsCharacterized()
        Dim input = SpcInputData.FromStackedObservations({1.0R, 2.0R, 3.0R, 4.0R}, {"A", "B", "A", "B"})
        Dim options As New SpcAnalysisOptions With {.Exclusions = {New SpcExclusionDefinition(2, SpcExclusionScope.RuleEvaluation)}, .Rules = New SpcRuleOptions With {.Preset = SpcRulePreset.None}}
        Dim request As New SpcFitRequest(SpcChartType.XBar, input, options)
        SpcEngine.Validate(request)
        Dim ex = Spc21Fixture.CatchAction(Sub() SpcEngine.Fit(request))
        Assert.IsNotNull(ex) : Assert.IsInstanceOfType(ex, GetType(ArgumentException))
        Spc21Fixture.Emit(New XElement("profile", New XAttribute("id", "raw-vs-point"), New XAttribute("status", "NOT_GOLDEN"), New XAttribute("classification", "EXISTING_TWO_LEVEL_REJECTION"), New XAttribute("culture", CultureInfo.CurrentCulture.Name), New XAttribute("runtime", Environment.Version.ToString()), New XAttribute("chart", "XBar"), New XAttribute("layout", "StackedObservations"), New XAttribute("dataset_sha256", Spc21Fixture.Hash(New Double() {1, 2, 3, 4})), New XElement("raw_rows", 4), New XElement("logical_points", 2), New XElement("exception", New XAttribute("type", ex.GetType().FullName), ex.Message)))
        options.Exclusions = {}
        options.Stages = {New SpcStageDefinition("raw-extent", 0, 3, SpcPhase.PhaseI, SpcStageLimitMode.EstimateFromStageData)}
        request = New SpcFitRequest(SpcChartType.XBar, input, options)
        SpcEngine.Validate(request)
        ex = Spc21Fixture.CatchAction(Sub() SpcEngine.Fit(request))
        Assert.IsNotNull(ex) : Assert.IsInstanceOfType(ex, GetType(ArgumentException))
        Spc21Fixture.Emit(New XElement("profile", New XAttribute("id", "raw-stage-vs-point"), New XAttribute("status", "NOT_GOLDEN"), New XAttribute("classification", "EXISTING_TWO_LEVEL_REJECTION"), New XAttribute("culture", CultureInfo.CurrentCulture.Name), New XAttribute("runtime", Environment.Version.ToString()), New XAttribute("chart", "XBar"), New XAttribute("layout", "StackedObservations"), New XAttribute("dataset_sha256", Spc21Fixture.Hash(New Double() {1, 2, 3, 4})), New XElement("raw_rows", 4), New XElement("logical_points", 2), New XElement("exception", New XAttribute("type", ex.GetType().FullName), ex.Message)))
    End Sub
    <TestMethod>
    Public Sub StackedFirstSeenLabelsSequenceAndEverySourceRowMatchCore()
        Dim source = {101, 105, 118, 131}, ids = {" A ", "B", "A", "B"}, map = SpcInputPointPolicy.FromRows(SpcDataLayout.StackedObservations, source, ids)
        Dim input = SpcInputData.FromStackedObservations({1.0R, 2.0R, 3.0R, 4.0R}, ids, {"firstA", "firstB", "lastA", "lastB"}, {11.0R, 12.0R, 99.0R, 100.0R}, source)
        Dim result = SpcEngine.Fit(New SpcFitRequest(SpcChartType.XBar, input, New SpcAnalysisOptions With {.Rules = New SpcRuleOptions With {.Preset = SpcRulePreset.None}}))
        Assert.AreEqual(2, result.ChartPointCount)
        Dim points = result.Panels(0).Points
        Assert.AreEqual("firstA", points(0).Label) : Assert.AreEqual("firstB", points(1).Label)
        CollectionAssert.AreEqual(map.Points(0).SourceRowIndices.ToArray(), points(0).SourceRowIndices)
        CollectionAssert.AreEqual(map.Points(1).SourceRowIndices.ToArray(), points(1).SourceRowIndices)
        Assert.AreEqual(11.0R, points(0).SequenceValue.Value) : Assert.AreEqual(12.0R, points(1).SequenceValue.Value)
    End Sub
    <TestMethod>
    Public Sub SnapshotsAreDetachedAndMappingScalesWithoutQuadraticPointSearch()
        Dim n = 40000, ids = Enumerable.Range(0, n).Select(Function(i) "group" & (i Mod 2000).ToString(CultureInfo.InvariantCulture)).ToArray(), sources = Enumerable.Range(101, n).ToArray()
        Dim map = SpcInputPointPolicy.FromRows(SpcDataLayout.StackedObservations, sources, ids)
        Assert.AreEqual(2000, map.PointCount) : Assert.AreEqual(20, map.Points(0).InputRowIndices.Count)
        ids(0) = "mutated" : sources(0) = -1 : Assert.AreEqual("group0", map.Points(0).SubgroupId) : Assert.AreEqual(101, map.Points(0).SourceRowIndices(0))
        Dim custom = SpcRuleCatalog.GetRules(SpcRulePreset.Nelson), rules As New SpcRuleRequest(SpcRulePreset.Custom, SpcRulePhaseScope.PhaseII, SpcSequenceGapBehavior.SkipPointAndContinue, SpcSignalMarkingMode.EntirePattern, custom)
        custom(0) = Nothing : Dim a = rules.ToCore(SpcChartType.Individuals), b = rules.ToCore(SpcChartType.Individuals)
        Assert.IsNotNull(a.CustomRules(0)) : a.CustomRules(0) = Nothing : Assert.IsNotNull(b.CustomRules(0))
        Assert.AreEqual(SpcRulePreset.None, rules.ToCore(SpcChartType.Ewma).Preset)
        Assert.AreEqual(SpcSequenceGapBehavior.SkipPointAndContinue, rules.ToCore(SpcChartType.Ewma).GapBehavior)
        Dim limits As New SpcLimitRequest(SpcParameterSource.EstimateFromPhaseI, SpcControlLimitMethod.ShewhartSigma, 3, SpcWithinSigmaEstimator.Automatic, SpcNaturalLimitPolicy.ClipToFeasibleRange, 2, True)
        a = rules.ToCore(SpcChartType.Individuals) : Dim l = limits.ToCore() : l.SigmaMultiplier = 100 : Assert.AreEqual(3.0R, limits.ToCore().SigmaMultiplier)
    End Sub
    <TestMethod>
    Public Sub NewNullShapeContractsAndCoreCancellationStayExplicit()
        Assert.ThrowsException(Of ArgumentNullException)(Sub() SpcInputPointPolicy.FromRows(SpcDataLayout.IndividualSequence, Nothing))
        Assert.ThrowsException(Of ArgumentException)(Sub() SpcInputPointPolicy.FromRows(SpcDataLayout.StackedObservations, {1, 2}, {"A"}))
        Assert.ThrowsException(Of ArgumentException)(Sub() SpcInputPointPolicy.FromWorksheetRows(SpcDataLayout.IndividualSequence, 2, 1))
        Assert.AreEqual(2, SpcInputPointPolicy.FirstDataRow({False, True, False}))
        Dim request = Spc21Profile.Request(0, True)
        Assert.ThrowsException(Of OperationCanceledException)(Sub() SpcEngine.Fit(request, Function() True))
    End Sub
    Public Shared Iterator Function StageEdgeCases() As IEnumerable(Of Object())
        For id = 0 To 15 : Yield New Object() {id} : Next
    End Function
    <DataTestMethod><DynamicData(NameOf(StageEdgeCases), DynamicDataSourceType.Method)>
    Public Sub StageCoverageReferenceOrderingAndExclusionScopesRemainCoreDecisions(id As Integer)
        Dim rows As New List(Of SpcStageRowIntent) From {
            New SpcStageRowIntent("A", "baseline", 1, 30, SpcPhase.PhaseI, SpcStageLimitMode.EstimateFromStageData, ""),
            New SpcStageRowIntent("B", "monitor", 31, 60, SpcPhase.PhaseII, SpcStageLimitMode.UseReferenceStage, "A")}
        Select Case id
            Case 0 : rows(1) = New SpcStageRowIntent("B", "", 30, 60, SpcPhase.PhaseII, SpcStageLimitMode.UseReferenceStage, "A")
            Case 1 : rows(1) = New SpcStageRowIntent("B", "", 35, 60, SpcPhase.PhaseII, SpcStageLimitMode.UseReferenceStage, "A")
            Case 2 : rows(1) = New SpcStageRowIntent("a", "", 31, 60, SpcPhase.PhaseI, SpcStageLimitMode.EstimateFromStageData, "")
            Case 3 : rows(1) = New SpcStageRowIntent("B", "", 31, 60, SpcPhase.PhaseII, SpcStageLimitMode.UseReferenceStage, "missing")
            Case 4 : rows(1) = New SpcStageRowIntent("B", "", 31, 60, SpcPhase.PhaseII, SpcStageLimitMode.UseReferenceStage, "B")
            Case 5 : rows.Reverse()
            Case 6 : rows(0) = New SpcStageRowIntent("A", "", 1, 30, SpcPhase.PhaseII, SpcStageLimitMode.UseReferenceStage, "B")
            Case 7 : rows.Clear()
        End Select
        Dim a As SpcFitRequest = Nothing, b As SpcFitRequest = Nothing
        Dim build As Func(Of Boolean, SpcFitRequest) = Function(portable)
            Dim stages = rows.Select(Function(r) If(portable, SpcPhaseStagePolicy.Manual(r, 60), Spc21LegacyPreparation.ManualStage(r, 60))).ToArray()
            Dim ex As SpcExclusionDefinition() = {}
            If id >= 8 Then
                ex = {New SpcExclusionDefinition(0, SpcExclusionScope.ParameterEstimation), New SpcExclusionDefinition(0, If(id Mod 2 = 0, SpcExclusionScope.ParameterEstimation, SpcExclusionScope.RuleEvaluation))}
            End If
            Dim limits As New SpcLimitRequest(SpcParameterSource.EstimateFromPhaseI, SpcControlLimitMethod.ShewhartSigma, 3, SpcWithinSigmaEstimator.Automatic, SpcNaturalLimitPolicy.ClipToFeasibleRange, 2, True)
            Dim rules As New SpcRuleRequest(SpcRulePreset.Nelson, CType(id Mod 4, SpcRulePhaseScope), CType(id Mod 2, SpcSequenceGapBehavior), CType(id Mod 2, SpcSignalMarkingMode))
            Dim options = If(portable, SpcRequestPreparation.Analysis(SpcMissingValuePolicy.Reject, limits, rules, SpcChartType.Individuals, stages, ex), New SpcAnalysisOptions With {.ControlLimits = New SpcControlLimitOptions(), .Rules = New SpcRuleOptions With {.Preset = SpcRulePreset.Nelson, .PhaseScope = CType(id Mod 4, SpcRulePhaseScope), .GapBehavior = CType(id Mod 2, SpcSequenceGapBehavior), .MarkingMode = CType(id Mod 2, SpcSignalMarkingMode)}, .Stages = stages, .Exclusions = ex})
            Return New SpcFitRequest(SpcChartType.Individuals, Spc21Fixture.Input(SpcChartType.Individuals, -1), options)
        End Function
        Dim before As SpcFitResult = Nothing, after As SpcFitResult = Nothing
        Dim x = Spc21Fixture.CatchAction(Sub()
            a = build(False) : SpcEngine.Validate(a) : before = SpcEngine.Fit(a)
        End Sub)
        Dim y = Spc21Fixture.CatchAction(Sub()
            b = build(True) : SpcEngine.Validate(b) : after = SpcEngine.Fit(b)
        End Sub)
        If x IsNot Nothing OrElse y IsNot Nothing Then
            Spc21Fixture.SameException(x, y)
        Else
            Assert.AreEqual(Spc21Fixture.Hash(before.Panels), Spc21Fixture.Hash(after.Panels))
            Assert.AreEqual(Spc21Fixture.TableHash(Spc21Fixture.Tables(before)), Spc21Fixture.TableHash(Spc21Fixture.Tables(after)))
        End If
        Spc21Fixture.Emit(New XElement("profile", New XAttribute("id", "stage-edge-" & id.ToString(CultureInfo.InvariantCulture)), New XAttribute("status", "NOT_GOLDEN"), New XAttribute("classification", If(y Is Nothing, "CORE_RETURNED", "EXISTING_REJECTION")), New XAttribute("culture", CultureInfo.CurrentCulture.Name), New XAttribute("runtime", Environment.Version.ToString()), New XAttribute("chart", "Individuals"), New XAttribute("layout", "IndividualSequence"), New XAttribute("dataset_sha256", Spc21Fixture.DatasetSha), New XElement("row_intent_sha256", Spc21Fixture.Hash(rows)), New XElement("exception", If(y Is Nothing, "", y.GetType().FullName & ": " & y.Message)), New XElement("panels_sha256", If(after Is Nothing, "", Spc21Fixture.Hash(after.Panels)))))
    End Sub
    Public Shared Iterator Function CustomCases() As IEnumerable(Of Object())
        For Each kind As SpcRuleKind In [Enum].GetValues(GetType(SpcRuleKind))
            For Each side As SpcRuleSide In [Enum].GetValues(GetType(SpcRuleSide))
                For Each scope In New SpcRuleScope() {SpcRuleScope.LocationPanels, SpcRuleScope.DispersionPanels, SpcRuleScope.AttributePanels, SpcRuleScope.TimeWeightedPanels, SpcRuleScope.LocationAndAttributePanels, SpcRuleScope.AllShewhartPanels, SpcRuleScope.All}
                    For Each minimum As Integer In New Integer() {2, 4}
                        Yield New Object() {kind, side, scope, minimum}
                    Next
                Next
            Next
        Next
    End Function
    <DataTestMethod><DynamicData(NameOf(CustomCases), DynamicDataSourceType.Method)>
    Public Sub CustomRuleKindsSidesScopesDelegateCoreWithoutReevaluation(kind As SpcRuleKind, side As SpcRuleSide, scope As SpcRuleScope, minimum As Integer)
        Dim rules = {New SpcRuleDefinition("C21", 99, kind, 4, minimum, 1.0R, side, scope, "test Ω", "tag description")}
        Dim old As New SpcRuleOptions With {.Preset = SpcRulePreset.Custom, .CustomRules = rules, .PhaseScope = SpcRulePhaseScope.All, .GapBehavior = SpcSequenceGapBehavior.SkipPointAndContinue, .MarkingMode = SpcSignalMarkingMode.EntirePattern}
        Dim intent As New SpcRuleRequest(SpcRulePreset.Custom, old.PhaseScope, old.GapBehavior, old.MarkingMode, rules)
        Dim input = Spc21Fixture.Input(SpcChartType.Individuals, -1), a As SpcFitResult = Nothing, b As SpcFitResult = Nothing
        Dim x = Spc21Fixture.CatchAction(Sub() a = SpcEngine.Fit(New SpcFitRequest(SpcChartType.Individuals, input, New SpcAnalysisOptions With {.Rules = old})))
        Dim y = Spc21Fixture.CatchAction(Sub() b = SpcEngine.Fit(New SpcFitRequest(SpcChartType.Individuals, input, New SpcAnalysisOptions With {.Rules = intent.ToCore(SpcChartType.Individuals)})))
        If x IsNot Nothing OrElse y IsNot Nothing Then
            Spc21Fixture.SameException(x, y)
        Else
            Assert.AreEqual(Spc21Fixture.Hash(a.Panels), Spc21Fixture.Hash(b.Panels))
            Assert.AreEqual(Spc21Fixture.TableHash(Spc21Fixture.Tables(a)), Spc21Fixture.TableHash(Spc21Fixture.Tables(b)))
        End If
        Assert.AreEqual(Spc21Fixture.Hash(old), Spc21Fixture.Hash(intent.ToCore(SpcChartType.Individuals)))
        Spc21Fixture.Emit(New XElement("profile", New XAttribute("id", "custom-" & kind.ToString() & "-" & side.ToString() & "-" & scope.ToString() & "-" & minimum.ToString(CultureInfo.InvariantCulture)), New XAttribute("status", "NOT_GOLDEN"), New XAttribute("classification", If(y Is Nothing, "CORE_RETURNED", "EXISTING_REJECTION")), New XAttribute("culture", CultureInfo.CurrentCulture.Name), New XAttribute("runtime", Environment.Version.ToString()), New XAttribute("chart", "Individuals"), New XAttribute("layout", "IndividualSequence"), New XAttribute("dataset_sha256", Spc21Fixture.DatasetSha), New XElement("rule_sha256", Spc21Fixture.Hash(rules)), New XElement("exception", If(y Is Nothing, "", y.GetType().FullName & ": " & y.Message)), New XElement("panels_sha256", If(b Is Nothing, "", Spc21Fixture.Hash(b.Panels)))))
        Assert.ThrowsException(Of ArgumentException)(Sub() SpcRuleRequest.FinishCustom({}, False))
        Assert.AreEqual(0, SpcRuleRequest.FinishCustom({}, True).Length)
    End Sub
    <DataTestMethod><DataRow(0)><DataRow(1)><DataRow(2)><DataRow(3)><DataRow(4)><DataRow(5)>
    Public Sub HistoricalOptionalZeroEmptyAndFiniteGuardsRemainCoreOwned(id As Integer)
        Dim name = If(id = 0, "", "Stage Ω"), mean As Double? = Nothing, sd As Double? = Nothing
        If id = 2 Then mean = 0.0R
        If id = 3 Then sd = -1.0R
        If id = 4 Then mean = Double.NaN
        If id = 5 Then mean = Double.PositiveInfinity
        Dim value As SpcHistoricalParameters = Nothing
        Dim ex = Spc21Fixture.CatchAction(Sub() value = SpcRequestPreparation.HistoricalRow(name, mean, sd, Nothing, Nothing, Nothing))
        If id = 0 Then
            Assert.IsNull(ex) : Assert.IsNull(value)
        ElseIf id = 1 Then
            Assert.IsInstanceOfType(ex, GetType(ArgumentException))
            StringAssert.Contains(ex.Message, "contains no parameter relevant")
        Else
            Dim legacy As SpcHistoricalParameters = Nothing
            Dim expected = Spc21Fixture.CatchAction(Sub() legacy = New SpcHistoricalParameters(stageId:=name, processMean:=mean, processSigma:=sd))
            If expected IsNot Nothing OrElse ex IsNot Nothing Then Spc21Fixture.SameException(expected, ex) Else Assert.AreEqual(Spc21Fixture.Hash(legacy), Spc21Fixture.Hash(value))
        End If
    End Sub
End Class
