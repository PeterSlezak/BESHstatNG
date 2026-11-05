Option Explicit On
Option Strict On
Option Infer On
Imports System
Imports System.Collections.Generic
Imports System.Globalization
Imports System.Linq
Imports System.Text
Imports System.Xml.Linq
Imports BESHStatNG.ExcelCommon
Imports BESHStatNG.StatisticalProcessControl
Imports Microsoft.VisualStudio.TestTools.UnitTesting

' Same Core snapshot -> frozen entry renderer and shipping Common projection.
' These are presentation comparisons, never independent scientific goldens.
Public NotInheritable Class Spc22Fixture
    Public Shared Function OptionsSnapshot(request As SpcFitRequest) As XElement
        Dim settings = request.AnalysisOptions, limits = settings.ControlLimits, rules = settings.Rules
        Return New XElement("options", New XAttribute("layout", request.DataLayout), New XAttribute("missing", settings.MissingValuePolicy),
            New XAttribute("parameter_source", limits.ParameterSource), New XAttribute("limit_method", limits.Method), New XAttribute("sigma", Spc21Fixture.Text(limits.SigmaMultiplier)),
            New XAttribute("estimator", limits.WithinSigmaEstimator), New XAttribute("natural_limits", limits.NaturalLimitPolicy), New XAttribute("moving_range_length", limits.MovingRangeLength), New XAttribute("bias_correction", limits.UseBiasCorrection),
            New XAttribute("rule_preset", rules.Preset), New XAttribute("rule_phase_scope", rules.PhaseScope), New XAttribute("gap", rules.GapBehavior), New XAttribute("marking", rules.MarkingMode),
            New XElement("data_sha256", Spc21Fixture.Hash(request.Data)), New XElement("custom_rules_sha256", Spc21Fixture.Hash(rules.CustomRules)), New XElement("history_sha256", Spc21Fixture.Hash(request.HistoricalParameters)),
            New XElement("chart_parameters", New XAttribute("ewma_lambda", Spc21Fixture.Text(request.ChartParameters.EwmaLambda)), New XAttribute("steady_state", request.ChartParameters.UseSteadyStateLimits), New XAttribute("cusum_k", Spc21Fixture.Text(request.ChartParameters.CusumReferenceValue)), New XAttribute("cusum_h", Spc21Fixture.Text(request.ChartParameters.CusumDecisionInterval)), New XAttribute("head_start", Spc21Fixture.Text(request.ChartParameters.HeadStart)), New XAttribute("ma_span", Spc21Fixture.Text(request.ChartParameters.MovingAverageSpan))),
            New XElement("stages", request.Stages.Select(Function(s) New XElement("stage", New XAttribute("id", s.StageId), New XAttribute("first", s.FirstPointIndex), New XAttribute("last", s.LastPointIndex), New XAttribute("phase", s.Phase), New XAttribute("mode", s.LimitMode), New XAttribute("reference", s.ReferenceStageId)))),
            New XElement("exclusions", request.Exclusions.Select(Function(e) New XElement("exclusion", New XAttribute("point", e.PointIndex), New XAttribute("scope", e.Scope), e.Reason))))
    End Function
    Public Shared Function Request() As SpcFitRequest
        Return New SpcFitRequest(SpcChartType.Individuals, SpcInputData.FromIndividualSequence({1.0R, 2.0R, 3.0R}))
    End Function
    Public Shared Function Point(index As Integer, Optional value As Double = 2.0R, Optional stage As String = "A", Optional phase As SpcPhase = SpcPhase.PhaseI, Optional sequence As Double? = Nothing, Optional rows As Integer() = Nothing, Optional excluded As Boolean = False, Optional reason As String = "", Optional rules As Integer() = Nothing) As SpcPointResult
        Return New SpcPointResult(index, value, 2, -1, 5, label:="same Ω", stageId:=stage, phase:=phase, sequenceValue:=sequence, lowerOneSigmaLimit:=1, upperOneSigmaLimit:=3, lowerTwoSigmaLimit:=0, upperTwoSigmaLimit:=4, sourceRowIndices:=rows, includedInParameterEstimation:=Not excluded, exclusionScope:=If(excluded, SpcExclusionScope.ParameterEstimation, SpcExclusionScope.None), exclusionReason:=reason, signalRuleNumbers:=rules)
    End Function
    Public Shared Sub SameSample(expected As Object, actual As Double?)
        Assert.AreEqual(expected IsNot Nothing, actual.HasValue)
        If actual.HasValue Then Assert.AreEqual(BitConverter.DoubleToInt64Bits(CDbl(expected)), BitConverter.DoubleToInt64Bits(actual.Value), "Exact Double bits including signed zero")
    End Sub
    Private Shared Sub SameLabels(expected As List(Of Spc22LegacyLabel), actual As IReadOnlyList(Of SpcChartLabelIntent))
        Assert.AreEqual(expected.Count, actual.Count)
        For i = 0 To expected.Count - 1
            Assert.AreEqual(expected(i).Index, actual(i).SeriesIndex)
            Assert.AreEqual(expected(i).Text, actual(i).Text)
        Next
    End Sub
    Public Shared Function Compare(result As SpcFitResult, useSequence As Boolean) As XElement
        Dim projection = SpcUnivariateChartProjectionPolicy.Project(result, useSequence)
        Dim options As New Spc22LegacyRenderer.SpcControlChartAppearanceOptions With {.UseSequenceValuesForHorizontalAxis = useSequence}
        Dim axis = Spc22LegacyRenderer.BuildSharedAxisContext(result, options)
        Assert.AreEqual(result.ChartType, projection.ChartType)
        Assert.AreEqual(result.ChartFamily, projection.ChartFamily)
        Assert.AreEqual(axis.PointIndices.Length, projection.Axis.Count)
        Dim record As New XElement("projection", New XAttribute("sequence", useSequence), New XAttribute("chart", result.ChartType), New XAttribute("family", result.ChartFamily))
        For i = 0 To axis.PointIndices.Length - 1
            Dim p = projection.Axis(i), category As Object = p.Point.Label
            If p.IsSequenceCategory Then category = p.Point.SequenceValue.Value
            Assert.AreEqual(axis.PointIndices(i), p.Point.PointIndex)
            Assert.AreEqual(axis.StageIds(i), p.Point.StageId)
            Assert.AreEqual(axis.Categories(i).GetType(), category.GetType())
            Assert.AreEqual(axis.Categories(i), category)
            record.Add(New XElement("axis", New XAttribute("slot", i), New XAttribute("point", p.Point.PointIndex), New XAttribute("display", p.Point.PointIndex + 1), New XAttribute("category_type", category.GetType().FullName), New XAttribute("category", Spc21Fixture.Text(category)), New XAttribute("stage", p.Point.StageId), New XAttribute("phase", p.Point.Phase), New XElement("source_rows", p.Point.SourceRowIndices.Select(Function(r) New XElement("row", r)))))
        Next
        Dim boundaryOrdinal = 0
        For i = 1 To axis.StageIds.Length - 1
            If Not String.Equals(axis.StageIds(i - 1), axis.StageIds(i), StringComparison.OrdinalIgnoreCase) Then
                Dim b = projection.StageBoundaries(boundaryOrdinal)
                Assert.AreEqual(i, b.Ordinal) : Assert.AreEqual(axis.StageIds.Length, b.Count)
                Assert.AreEqual(BitConverter.DoubleToInt64Bits(CDbl(i) / CDbl(axis.StageIds.Length)), BitConverter.DoubleToInt64Bits(b.Fraction))
                Assert.AreEqual("Stage boundary: " & axis.StageIds(i - 1) & " to " & axis.StageIds(i), b.AlternativeText)
                record.Add(New XElement("boundary", New XAttribute("i", i), New XAttribute("n", b.Count), New XAttribute("fraction", b.Fraction.ToString("R", CultureInfo.InvariantCulture)), b.AlternativeText))
                boundaryOrdinal += 1
            End If
        Next
        Assert.AreEqual(boundaryOrdinal, projection.StageBoundaries.Count)
        Dim panels = result.Panels
        Assert.AreEqual(panels.Length, projection.Panels.Count)
        For panelIndex = 0 To panels.Length - 1
            Dim before = Spc22LegacyRenderer.BuildPanelChartData(panels(panelIndex), axis), after = projection.Panels(panelIndex)
            Assert.AreSame(panels(panelIndex), after.Panel)
            Dim oldChannels As Object()() = {before.StatisticValues, before.CenterLineValues, before.LowerControlLimitValues, before.UpperControlLimitValues, before.LowerOneSigmaValues, before.UpperOneSigmaValues, before.LowerTwoSigmaValues, before.UpperTwoSigmaValues, before.SignalValues, before.ExcludedValues}
            Dim panelNode As New XElement("panel", New XAttribute("order", panelIndex), New XAttribute("type", after.Panel.PanelType), New XAttribute("name", after.Panel.DisplayName), New XAttribute("value_axis", after.Panel.ValueAxisTitle), New XAttribute("signal_occurrences", after.Panel.Signals.Length))
            For c = 0 To 9
                Dim channel As New XElement("channel", New XAttribute("kind", CType(c, SpcChartChannel)))
                For i = 0 To axis.PointIndices.Length - 1
                    SameSample(oldChannels(c)(i), after.Channels(c)(i))
                    channel.Add(New XElement("v", If(after.Channels(c)(i).HasValue, BitConverter.DoubleToInt64Bits(after.Channels(c)(i).GetValueOrDefault()).ToString("X16", CultureInfo.InvariantCulture), "BLANK")))
                Next
                panelNode.Add(channel)
            Next
            For i = 0 To before.Points.Length - 1
                Assert.AreSame(before.Points(i), after.Points(i))
                If after.Points(i) IsNot Nothing Then
                    Dim point = after.Points(i)
                    panelNode.Add(New XElement("identity", New XAttribute("slot", i), New XAttribute("point", point.PointIndex), New XAttribute("phase", point.Phase), New XAttribute("stage", point.StageId), New XAttribute("finite", point.HasFiniteValue), New XAttribute("estimation", point.IncludedInParameterEstimation), New XAttribute("rules", point.IncludedInRuleEvaluation), New XAttribute("scope", point.ExclusionScope), New XAttribute("reason", point.ExclusionReason), New XAttribute("signal_rules", String.Join(",", point.SignalRuleNumbers)), New XElement("source_rows", point.SourceRowIndices.Select(Function(r) New XElement("row", r)))))
                End If
            Next
            For Each signal In after.Panel.Signals
                panelNode.Add(New XElement("occurrence", New XAttribute("panel", signal.PanelType), New XAttribute("stage", signal.StageId), New XAttribute("rule", signal.RuleNumber), New XAttribute("terminal", signal.TerminalPointIndex), New XAttribute("first", signal.WindowStartPointIndex), New XAttribute("last", signal.WindowEndPointIndex), New XElement("contributors", signal.ContributingPointIndices.Select(Function(v) New XElement("i", v))), New XElement("marked", signal.MarkedPointIndices.Select(Function(v) New XElement("i", v)))))
            Next
            Dim labels As New List(Of Spc22LegacyLabel)
            Spc22LegacyRenderer.AddPointLabels(labels, before.Points, options) : SameLabels(labels, after.PointLabels)
            labels.Clear() : Spc22LegacyRenderer.AddSignalLabels(labels, before.Points, options) : SameLabels(labels, after.SignalLabels)
            labels.Clear() : Spc22LegacyRenderer.AddExclusionLabels(labels, before.Points, options) : SameLabels(labels, after.ExclusionLabels)
            panelNode.Add(New XElement("point_labels", after.PointLabels.Select(Function(l) New XElement("label", New XAttribute("slot", l.SeriesIndex), l.Text))), New XElement("signal_labels", after.SignalLabels.Select(Function(l) New XElement("label", New XAttribute("slot", l.SeriesIndex), l.Text))), New XElement("exclusion_labels", after.ExclusionLabels.Select(Function(l) New XElement("label", New XAttribute("slot", l.SeriesIndex), l.Text))))
            record.Add(panelNode)
        Next
        Return record
    End Function
    Public Shared Sub Emit(record As XElement)
        Console.WriteLine("04A22_XML|" & Convert.ToBase64String(Encoding.UTF8.GetBytes(record.ToString(SaveOptions.DisableFormatting))))
    End Sub
End Class

<TestClass>
Public Class Spc22Tests
    <TestMethod>
    Public Sub ExistingSharedRendererAcceptsGenericCorePanelSnapshotsWithoutFamilyDispatch()
        'Ui22 already wraps its panels in SpcFitResult for the public renderer,
        'retaining its chart type. Exercise immutable models, never multivariate Fit.
        Dim kinds = {SpcPanelType.HotellingT2, SpcPanelType.GeneralizedVariance, SpcPanelType.PcaT2, SpcPanelType.PcaQ, SpcPanelType.Mewma, SpcPanelType.Mcusum}
        Dim charts = {SpcChartType.HotellingT2, SpcChartType.GeneralizedVariance, SpcChartType.PcaT2, SpcChartType.PcaQ, SpcChartType.Mewma, SpcChartType.Mcusum}
        For i = 0 To kinds.Length - 1
            Dim kind = kinds(i)
            Dim request As New SpcFitRequest(charts(i), SpcInputData.FromIndividualSequence({1.0R, 2.0R, 3.0R}))
            Dim panel As New SpcPanelResult(kind, kind.ToString(), {Spc22Fixture.Point(0, rows:={101, 118}), Spc22Fixture.Point(2, stage:="B", sequence:=7)})
            Dim result As New SpcFitResult(request, {panel})
            Spc22Fixture.Compare(result, False)
            Spc22Fixture.Compare(result, True)
            Assert.AreEqual(SpcChartFamily.Multivariate, SpcUnivariateChartProjectionPolicy.Project(result).ChartFamily)
        Next
    End Sub
    Public Shared Iterator Function SupplementalCases() As IEnumerable(Of Object())
        For id = 0 To 23 : Yield New Object() {id} : Next
    End Function
    <DataTestMethod><DynamicData(NameOf(SupplementalCases), DynamicDataSourceType.Method)>
    Public Sub TimeWeightedAttributeAndInterleavedSourceEdges(id As Integer)
        If id = 7 OrElse id = 18 Then
            Dim rejected = Spc21Fixture.CatchAction(Sub()
                If id = 7 Then
                    Dim invalid = New SpcChartParameters(movingAverageSpan:=1)
                Else
                    Dim invalid = SpcInputData.FromIndividualSequence({50.0R, Double.PositiveInfinity, 51.0R})
                End If
            End Sub)
            Assert.IsInstanceOfType(rejected, GetType(ArgumentOutOfRangeException), "Preserve rejection at Core parameter/input construction before Fit.")
            Spc22Fixture.Emit(New XElement("case", New XAttribute("id", "supplemental-" & id.ToString(CultureInfo.InvariantCulture)), New XAttribute("status", "NOT_GOLDEN"), New XAttribute("classification", "CORE_OR_REQUEST_REJECTION_NOT_PARITY_SUCCESS"), New XAttribute("phase", "Core construction before Fit"), New XElement("exception", New XAttribute("type", rejected.GetType().FullName), rejected.Message)))
            Return
        End If
        Dim chart = SpcChartType.Ewma, data = SpcInputData.FromIndividualSequence(Spc21Fixture.Individuals())
        Dim parameters As New SpcChartParameters()
        Dim settings As New SpcAnalysisOptions With {.MissingValuePolicy = SpcMissingValuePolicy.OmitPoint, .ControlLimits = New SpcControlLimitOptions With {.ParameterSource = SpcParameterSource.UseHistoricalParameters}, .Rules = New SpcRuleOptions With {.Preset = SpcRulePreset.None, .GapBehavior = CType(id Mod 2, SpcSequenceGapBehavior)}}
        Select Case id
            Case 0 : parameters = New SpcChartParameters(ewmaLambda:=0.000001R)
            Case 1 : parameters = New SpcChartParameters(ewmaLambda:=0.999999R)
            Case 2 : parameters = New SpcChartParameters(ewmaLambda:=0.2R, useSteadyStateLimits:=True)
            Case 3 : data = SpcInputData.FromIndividualSequence({50.0R, 51.0R}) : parameters = New SpcChartParameters(ewmaLambda:=0.2R)
            Case 4 : chart = SpcChartType.Cusum : parameters = New SpcChartParameters(headStart:=2.0R)
            Case 5 : chart = SpcChartType.Cusum : parameters = New SpcChartParameters(cusumReferenceValue:=0.25R, cusumDecisionInterval:=1.0R)
            Case 6 : chart = SpcChartType.Cusum : data = SpcInputData.FromIndividualSequence({50.0R, Double.NaN, 52.0R, 49.0R})
            Case 8 : chart = SpcChartType.MovingAverage : parameters = New SpcChartParameters(movingAverageSpan:=100)
            Case 9 : chart = SpcChartType.MovingAverage : parameters = New SpcChartParameters(movingAverageSpan:=61)
            Case 10 : chart = SpcChartType.MovingAverage : parameters = New SpcChartParameters(movingAverageSpan:=3) : data = SpcInputData.FromIndividualSequence({50.0R, Double.NaN, 52.0R, 49.0R})
            Case 11 : chart = SpcChartType.NpChart : data = SpcInputData.FromAggregatedCounts({1.0R, 2.0R, 3.0R}, sampleSizes:={10.0R, 11.0R, 12.0R})
            Case 12 : chart = SpcChartType.NpChart : data = SpcInputData.FromAggregatedCounts({1.0R, -1.0R, 3.0R}, sampleSizes:={10.0R, 10.0R, 10.0R})
            Case 13 : chart = SpcChartType.PChart : data = SpcInputData.FromAggregatedCounts({1.0R, Double.MaxValue, 3.0R}, sampleSizes:={10.0R, 10.0R, 10.0R})
            Case 14 : chart = SpcChartType.PChart : data = SpcInputData.FromAggregatedCounts({1.0R, 1.25R, 3.0R}, sampleSizes:={10.0R, 10.0R, 10.0R})
            Case 15 : chart = SpcChartType.PChart : data = SpcInputData.FromAggregatedCounts({1.0R, 1.0R, 3.0R}, sampleSizes:={10.0R, 0.0R, 10.0R})
            Case 16 : chart = SpcChartType.UChart : data = SpcInputData.FromAggregatedCounts({1.0R, 1.0R, 3.0R}, exposures:={10.0R, 0.0R, 10.0R})
            Case 17 : chart = SpcChartType.CChart : data = SpcInputData.FromAggregatedCounts({1.0R, 1.25R, 3.0R})
            Case 19
                chart = SpcChartType.XBarR
                data = SpcInputData.FromStackedObservations({49.0R, 51.0R, 49.5R, 51.5R}, {"A", "B", "A", "B"}, labels:={"first A", "first B", "last A", "last B"}, sequenceValues:={7.0R, 8.0R, 9.0R, 10.0R}, sourceRowIndices:={101, 105, 118, 131})
            Case 20, 21, 22
                chart = SpcChartType.XBarR
                Dim wide = Spc21Fixture.Wide() : wide(5, 2) = Double.NaN : data = SpcInputData.FromWideSubgroups(wide)
                settings.MissingValuePolicy = If(id = 20, SpcMissingValuePolicy.UseAvailableMeasurements, If(id = 21, SpcMissingValuePolicy.OmitPoint, SpcMissingValuePolicy.Reject))
            Case 23 : chart = SpcChartType.CChart : data = SpcInputData.FromAggregatedCounts({0.0R, 0.0R, 0.0R})
        End Select
        Dim history As New SpcHistoricalParameters(processMean:=50, processSigma:=0.25R, nonconformingProportion:=0.02R, meanDefectCount:=2, meanDefectRate:=0.02R)
        Dim request As New SpcFitRequest(chart, data, settings, {history}, chartParameters:=parameters)
        Dim result As SpcFitResult = Nothing
        Dim rejection = Spc21Fixture.CatchAction(Sub() result = SpcEngine.Fit(request))
        Dim record As New XElement("case", New XAttribute("id", "supplemental-" & id.ToString(CultureInfo.InvariantCulture)), New XAttribute("status", "NOT_GOLDEN"), New XAttribute("chart", chart), New XAttribute("runtime", Environment.Version.ToString()), New XElement("request_sha256", Spc21Fixture.Hash(request)), Spc22Fixture.OptionsSnapshot(request))
        If rejection Is Nothing Then
            record.Add(New XAttribute("classification", "PRESENTATION_PARITY"), Spc22Fixture.Compare(result, False), Spc22Fixture.Compare(result, True))
            If id = 19 Then
                CollectionAssert.AreEqual(New Integer() {101, 118}, result.Panels(0).Points(0).SourceRowIndices)
                CollectionAssert.AreEqual(New Integer() {105, 131}, result.Panels(0).Points(1).SourceRowIndices)
            End If
        Else
            record.Add(New XAttribute("classification", "CORE_OR_REQUEST_REJECTION_NOT_PARITY_SUCCESS"), New XElement("exception", New XAttribute("type", rejection.GetType().FullName), rejection.Message))
        End If
        If id = 11 OrElse id = 12 OrElse id = 13 OrElse id = 14 OrElse id = 17 OrElse id = 18 OrElse id = 22 Then Assert.IsNotNull(rejection, "Preserve current Core negative admission.")
        If id = 19 OrElse id = 20 OrElse id = 21 Then Assert.IsNull(rejection, "Ordinary wide/interleaved inputs retain their source identities.")
        Spc22Fixture.Emit(record)
    End Sub
    Public Shared Iterator Function ChartChoices() As IEnumerable(Of Object())
        For id = 0 To 14 : Yield New Object() {id} : Next
    End Function
    <DataTestMethod><DynamicData(NameOf(ChartChoices), DynamicDataSourceType.Method)>
    Public Sub AllFifteenWindowsChoicesKeepActualCorePanelOrder(id As Integer)
        Dim expected As SpcPanelType()() = {
            New SpcPanelType() {SpcPanelType.IndividualValue},
            New SpcPanelType() {SpcPanelType.MovingRange},
            New SpcPanelType() {SpcPanelType.IndividualValue, SpcPanelType.MovingRange},
            New SpcPanelType() {SpcPanelType.SubgroupMean},
            New SpcPanelType() {SpcPanelType.SubgroupRange},
            New SpcPanelType() {SpcPanelType.SubgroupStandardDeviation},
            New SpcPanelType() {SpcPanelType.SubgroupMean, SpcPanelType.SubgroupRange},
            New SpcPanelType() {SpcPanelType.SubgroupMean, SpcPanelType.SubgroupStandardDeviation},
            New SpcPanelType() {SpcPanelType.Proportion},
            New SpcPanelType() {SpcPanelType.NumberNonconforming},
            New SpcPanelType() {SpcPanelType.DefectCount},
            New SpcPanelType() {SpcPanelType.DefectRate},
            New SpcPanelType() {SpcPanelType.UpperCusum, SpcPanelType.LowerCusum},
            New SpcPanelType() {SpcPanelType.Ewma},
            New SpcPanelType() {SpcPanelType.MovingAverage}}
        Dim result = SpcEngine.Fit(Spc21Profile.Request(id, True))
        Dim p = SpcUnivariateChartProjectionPolicy.Project(result)
        CollectionAssert.AreEqual(expected(id), p.Panels.Select(Function(v) v.Panel.PanelType).ToArray())
        Spc22Fixture.Emit(Spc22Fixture.Compare(result, True))
    End Sub
    Public Shared Iterator Function Profiles() As IEnumerable(Of Object())
        For id = 0 To 569 : Yield New Object() {id} : Next
    End Function
    <DataTestMethod><DynamicData(NameOf(Profiles), DynamicDataSourceType.Method)>
    Public Sub SameCoreEntryRendererAndShippingProjection(id As Integer)
        Dim prior = CultureInfo.CurrentCulture
        Try
            For Each culture In {"", "sk-SK"}
                CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(culture)
                Dim request As SpcFitRequest = Nothing, result As SpcFitResult = Nothing
                Dim record As New XElement("case", New XAttribute("id", id), New XAttribute("culture", culture), New XAttribute("runtime", Environment.Version.ToString()), New XAttribute("chart", Spc21Fixture.Charts(id Mod 15)), New XAttribute("status", "NOT_GOLDEN"), New XAttribute("validation", "SAME_CORE_PRESENTATION_COMPATIBILITY_ONLY"), New XAttribute("dataset_sha256", Spc21Fixture.DatasetSha))
                Dim phase = "Request preparation/Core construction before Fit"
                Dim rejection = Spc21Fixture.CatchAction(Sub()
                    request = Spc21Profile.Request(id, True)
                    phase = "Core Fit"
                    result = SpcEngine.Fit(request)
                End Sub)
                If request IsNot Nothing Then record.Add(Spc22Fixture.OptionsSnapshot(request))
                If rejection IsNot Nothing Then
                    record.Add(New XAttribute("classification", "CORE_OR_REQUEST_REJECTION_NOT_PARITY_SUCCESS"), New XAttribute("phase", phase), New XElement("exception", New XAttribute("type", rejection.GetType().FullName), rejection.Message))
                Else
                    record.Add(New XAttribute("classification", "PRESENTATION_PARITY"), New XElement("request_sha256", Spc21Fixture.Hash(request)), New XElement("settings_sha256", Spc21Fixture.Hash(request.AnalysisOptions)), New XElement("tables_sha256", Spc21Fixture.TableHash(Spc21Fixture.Tables(result))), Spc22Fixture.Compare(result, False), Spc22Fixture.Compare(result, True))
                End If
                Spc22Fixture.Emit(record)
            Next
        Finally
            CultureInfo.CurrentCulture = prior
        End Try
    End Sub

    <TestMethod>
    Public Sub SparseUnionFirstRepresentativeTypedCategoriesAndPhaseOnlyTransition()
        Dim first = New SpcPanelResult(SpcPanelType.IndividualValue, "I", {Spc22Fixture.Point(0, sequence:=7, rows:={101, 118}), Spc22Fixture.Point(4, stage:="a", phase:=SpcPhase.PhaseII, rows:={105, 131})})
        Dim later = New SpcPanelResult(SpcPanelType.MovingRange, "MR", {Spc22Fixture.Point(0, stage:="different", sequence:=-8), Spc22Fixture.Point(2, stage:="B", sequence:=7), Spc22Fixture.Point(4, value:=Double.NaN, stage:="different")})
        Dim fit As New SpcFitResult(Spc22Fixture.Request(), {first, later})
        Spc22Fixture.Emit(Spc22Fixture.Compare(fit, True))
        Dim p = SpcUnivariateChartProjectionPolicy.Project(fit, True)
        CollectionAssert.AreEqual(New Integer() {0, 2, 4}, p.Axis.Select(Function(x) x.Point.PointIndex).ToArray())
        Assert.AreEqual("A", p.Axis(0).Point.StageId) : Assert.AreEqual(7.0R, p.Axis(0).Point.SequenceValue.Value)
        Assert.IsNull(p.Panels(0).Points(1)) : Assert.IsNotNull(p.Panels(1).Points(2))
        Assert.IsFalse(p.Panels(1).Channels(0)(2).HasValue)
        Assert.AreEqual(2, p.StageBoundaries.Count)
        Dim sameStage As New SpcFitResult(Spc22Fixture.Request(), {New SpcPanelResult(SpcPanelType.IndividualValue, "I", {Spc22Fixture.Point(0), Spc22Fixture.Point(1, stage:="a", phase:=SpcPhase.PhaseII)})})
        Assert.AreEqual(0, SpcUnivariateChartProjectionPolicy.Project(sameStage).StageBoundaries.Count)
    End Sub

    <TestMethod>
    Public Sub ImmutableSnapshotsSourceRowsChannelsAndRepeatedRuns()
        Dim rows = {101, 118}, points = {Spc22Fixture.Point(0, value:=BitConverter.Int64BitsToDouble(Long.MinValue), rows:=rows), Spc22Fixture.Point(1, excluded:=True)}
        Dim panel As New SpcPanelResult(SpcPanelType.IndividualValue, "I", points)
        Dim fit As New SpcFitResult(Spc22Fixture.Request(), {panel})
        Dim p = SpcUnivariateChartProjectionPolicy.Project(fit)
        rows(0) = -1 : points(0) = Spc22Fixture.Point(3)
        Dim returned = p.Axis(0).Point.SourceRowIndices : returned(0) = 99
        Assert.AreEqual(101, p.Axis(0).Point.SourceRowIndices(0))
        Assert.AreEqual(Long.MinValue, BitConverter.DoubleToInt64Bits(p.Panels(0).Channels(0)(0).Value))
        Assert.ThrowsException(Of NotSupportedException)(Sub() DirectCast(p.Axis, IList(Of SpcChartAxisPoint))(0) = Nothing)
        Assert.ThrowsException(Of NotSupportedException)(Sub() DirectCast(p.Panels(0).Channels(0), IList(Of Double?))(0) = 99)
        Assert.AreEqual("Excluded", p.Panels(0).ExclusionLabels(0).Text)
        Assert.AreEqual(Spc21Fixture.Hash(p), Spc21Fixture.Hash(SpcUnivariateChartProjectionPolicy.Project(fit)))
        Assert.IsFalse(p.UseSequenceValues) : Assert.IsTrue(SpcUnivariateChartProjectionPolicy.Project(fit, True).UseSequenceValues)
    End Sub

    <TestMethod>
    Public Sub OccurrencesContributorsAndMarkersRemainDistinctAndRuleOrderIsCoreOrder()
        Dim rules = SpcRuleCatalog.GetRules(SpcRulePreset.Nelson)
        Dim points = {Spc22Fixture.Point(0), Spc22Fixture.Point(1, excluded:=True, reason:="dôvod Ω", rules:={2, 1})}
        Dim signals = {New SpcSignalResult(SpcPanelType.IndividualValue, "A", rules(1), 1, 0, 1, contributingPointIndices:={0, 1}, markedPointIndices:={1}), New SpcSignalResult(SpcPanelType.IndividualValue, "A", rules(0), 1, 0, 1, contributingPointIndices:={0, 1}, markedPointIndices:={1})}
        Dim fit As New SpcFitResult(Spc22Fixture.Request(), {New SpcPanelResult(SpcPanelType.IndividualValue, "I", points, signals:=signals)})
        Spc22Fixture.Emit(Spc22Fixture.Compare(fit, False))
        Dim p = SpcUnivariateChartProjectionPolicy.Project(fit).Panels(0)
        Assert.AreEqual(2, p.Panel.Signals.Length) : Assert.AreEqual(1, p.SignalLabels.Count)
        CollectionAssert.AreEqual(New Integer() {1, 2}, points(1).SignalRuleNumbers, "Core normalizes the constructed result's rule enumeration.")
        Assert.AreEqual("R1,2", p.SignalLabels(0).Text, "Render the enumeration returned by Core without another normalization.")
        Assert.IsFalse(p.Channels(CInt(SpcChartChannel.Signal))(0).HasValue)
        Assert.IsTrue(p.Channels(CInt(SpcChartChannel.Signal))(1).HasValue)
        Assert.IsTrue(p.Channels(CInt(SpcChartChannel.Excluded))(1).HasValue)
        Dim clean As New SpcFitResult(Spc22Fixture.Request(), {New SpcPanelResult(SpcPanelType.IndividualValue, "I", {Spc22Fixture.Point(0)})})
        Assert.AreEqual(0, SpcUnivariateChartProjectionPolicy.Project(clean).Panels(0).SignalLabels.Count)
    End Sub

    <DataTestMethod><DataRow(0)><DataRow(1)><DataRow(2)><DataRow(3)><DataRow(4)><DataRow(5)><DataRow(6)><DataRow(7)>
    Public Sub EachSourceChannelPreservesNaNAndFiniteBits(channel As Integer)
        Dim values = New Double() {2, 2, -1, 5, 1, 3, 0, 4}
        values(channel) = Double.NaN
        Dim point As New SpcPointResult(0, values(0), values(1), values(2), values(3), lowerOneSigmaLimit:=values(4), upperOneSigmaLimit:=values(5), lowerTwoSigmaLimit:=values(6), upperTwoSigmaLimit:=values(7))
        Dim fit As New SpcFitResult(Spc22Fixture.Request(), {New SpcPanelResult(SpcPanelType.IndividualValue, "I", {point})})
        Spc22Fixture.Compare(fit, False)
        Assert.IsFalse(SpcUnivariateChartProjectionPolicy.Project(fit).Panels(0).Channels(channel)(0).HasValue)
    End Sub

    <TestMethod>
    Public Sub ImpossibleResultInfinityAndInvalidZonesRemainCoreRejections()
        Assert.ThrowsException(Of ArgumentException)(Sub()
            Dim ignored = New SpcFitResult(Spc22Fixture.Request(), {})
        End Sub)
        Assert.ThrowsException(Of ArgumentException)(Sub()
            Dim ignored = New SpcPanelResult(SpcPanelType.IndividualValue, "I", {})
        End Sub)
        For Each value In {Double.PositiveInfinity, Double.NegativeInfinity}
            Assert.ThrowsException(Of ArgumentOutOfRangeException)(Sub() Spc22Fixture.Point(0, value))
        Next
        Assert.ThrowsException(Of ArgumentException)(Sub()
            Dim ignored = New SpcPointResult(0, 2, 2, -1, 5, lowerTwoSigmaLimit:=3, lowerOneSigmaLimit:=1)
        End Sub)
        Assert.ThrowsException(Of ArgumentNullException)(Sub() SpcUnivariateChartProjectionPolicy.Project(Nothing))
    End Sub

    <TestMethod>
    Public Sub ThousandsOfPointsAndPanelsKeepSparseLinearAlignment()
        Dim points = Enumerable.Range(0, 5000).Select(Function(i) Spc22Fixture.Point(i * 2, stage:=If(i < 2500, "A", "B"))).ToArray()
        Dim panels = {New SpcPanelResult(SpcPanelType.IndividualValue, "I", points), New SpcPanelResult(SpcPanelType.MovingRange, "MR", points.Where(Function(point) point.PointIndex Mod 4 = 0).ToArray()), New SpcPanelResult(SpcPanelType.SubgroupMean, "X", points)}
        Dim p = SpcUnivariateChartProjectionPolicy.Project(New SpcFitResult(Spc22Fixture.Request(), panels))
        Assert.AreEqual(5000, p.Axis.Count) : Assert.AreEqual(3, p.Panels.Count)
        Assert.AreEqual(0.5R, p.StageBoundaries(0).Fraction)
        Assert.IsNull(p.Panels(1).Points(1)) : Assert.AreEqual(9998, p.Axis(4999).Point.PointIndex)
    End Sub
End Class
