Option Explicit On
Option Strict On
Option Infer On
Imports System
Imports System.Collections.Generic
Imports System.Globalization
Imports System.Linq
Imports System.Xml.Linq
Imports BESHStatNG.ExcelCommon
Imports BESHStatNG.StatisticalProcessControl
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass>
Public Class Spc23Tests
    Public Shared Function Failure(action As Action) As Exception
        Try
            action()
            Return Nothing
        Catch ex As AssertFailedException
            Throw
        Catch ex As Exception
            Return ex
        End Try
    End Function
    Public Shared Sub SameFailure(before As Exception, after As Exception)
        Assert.AreEqual(before Is Nothing, after Is Nothing)
        If before Is Nothing Then Return
        Assert.AreEqual(before.GetType(), after.GetType())
        Assert.AreEqual(before.Message, after.Message)
        If TypeOf before Is ArgumentException Then Assert.AreEqual(DirectCast(before, ArgumentException).ParamName, DirectCast(after, ArgumentException).ParamName)
    End Sub
    Public Shared Iterator Function Profiles() As IEnumerable(Of Object())
        For Each culture In {"", "sk-SK"}
            For id = 0 To 359
                Yield New Object() {id, culture}
            Next
        Next
    End Function

    <DataTestMethod, DynamicData(NameOf(Profiles), DynamicDataSourceType.Method)>
    Public Sub CapturedUiRequestAndSameCoreResultRemainCompatible(id As Integer, culture As String)
        Dim saved = CultureInfo.CurrentCulture
        Try
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(culture)
            Dim input = Spc23Fixture.InputFor(id)
            Dim node As New XElement("comparison", New XAttribute("id", id), New XAttribute("culture", culture), New XAttribute("runtime", Environment.Version.ToString()), New XAttribute("chart", input.Chart), New XAttribute("classification", "NOT_GOLDEN"), New XAttribute("purpose", "SAME_CORE_COMPATIBILITY_ONLY"), New XElement("dataset_sha256", Spc23Fixture.Hash(input.Data)), New XElement("options_sha256", Spc23Fixture.Hash(input.Options)), New XElement("names", input.Names.Select(Function(n) New XElement("name", n))))
            node.Add(New XElement("requested_options", input.Options.GetType().GetFields().OrderBy(Function(f) f.Name, StringComparer.Ordinal).Select(Function(f) New XElement("option", New XAttribute("name", f.Name), New XAttribute("type", f.FieldType.FullName), Spc21Fixture.Text(f.GetValue(input.Options))))))
            Dim oldMap As Spc23LegacyPreparation.InputRowContext = Nothing, newMap As SpcMultivariatePointMap = Nothing
            Dim first = input.FirstRow, last = first + input.Data.GetLength(0) - 1
            Dim before = Failure(Sub() oldMap = Spc23LegacyPreparation.Map(first, last, input.Grouped, input.Ids))
            Dim after = Failure(Sub() newMap = SpcMultivariatePointMapPolicy.FromWorksheetRows(first, last, input.Grouped, input.Ids))
            SameFailure(before, after)
            Dim stage = "point-map", request As SpcMultivariateRequest = Nothing, legacyRequest As SpcMultivariateRequest = Nothing
            If before Is Nothing Then
                CollectionAssert.AreEqual(oldMap.PointIndexByRow, newMap.PointIndexByRow.ToArray())
                Assert.AreEqual(oldMap.PointCount, newMap.PointCount)
                input.Context = oldMap : input.Map = newMap
                Spc23Fixture.FillGrids(input, id \ 6)
                For Each p In newMap.Points
                    node.Add(New XElement("source_point", New XAttribute("index", p.PointIndex), New XAttribute("display", p.DisplayPoint), New XAttribute("group", If(p.SubgroupId, "")), New XElement("input_rows", String.Join(",", p.InputRowIndices)), New XElement("physical_rows", String.Join(",", p.SourceRowIndices))))
                Next
                stage = "request"
                before = Failure(Sub() legacyRequest = Spc23Fixture.Prepare(input, False))
                after = Failure(Sub() request = Spc23Fixture.Prepare(input, True))
                SameFailure(before, after)
                If before Is Nothing Then
                    Assert.AreEqual(Spc23Fixture.Hash(legacyRequest), Spc23Fixture.Hash(request), "All typed request fields and Double bits")
                    node.Add(New XElement("request_sha256", Spc23Fixture.Hash(request)))
                    node.Add(New XElement("request_rows", New XAttribute("phases", String.Join(",", request.Phases)), New XAttribute("stages", String.Join(",", request.StageIds)), New XAttribute("scopes", String.Join(",", request.ExclusionScopes)), New XAttribute("sources", String.Join(",", request.SourceRowIndices)), New XElement("metadata_sha256", Spc23Fixture.Hash(New Object() {request.Labels, request.SequenceValues, request.ExclusionReasons, request.SubgroupIds}))))
                    stage = "core-fit"
                    Dim result As SpcMultivariateFitResult = Nothing
                    before = Failure(Sub() result = SpcMultivariate.Fit(request))
                    If before Is Nothing Then
                        node.Add(Spc23Fixture.CompareResult(result))
                        node.Add(New XAttribute("outcome", "POSITIVE_PARITY"))
                    End If
                End If
            End If
            If before IsNot Nothing Then
                node.Add(New XAttribute("outcome", If(stage = "core-fit", "EXISTING_CORE_REJECTION", "EXISTING_PREPARATION_EXCEPTION")))
                node.Add(New XElement("exception", New XAttribute("stage", stage), New XAttribute("type", before.GetType().FullName), New XAttribute("parameter", If(TryCast(before, ArgumentException)?.ParamName, "")), before.Message))
            End If
            Spc23Fixture.Emit(node)
        Finally
            CultureInfo.CurrentCulture = saved
        End Try
    End Sub

    <DataTestMethod><DataRow(0)><DataRow(1)><DataRow(2)><DataRow(3)><DataRow(4)><DataRow(5)>
    Public Sub CompatibilityRetainsSixChartStructureAndEnabledStatePredicates(id As Integer)
        Dim chart = Spc23Data.Charts(id), c = SpcMultivariateRequestPolicy.Compatibility(chart)
        Assert.AreEqual(chart = SpcChartType.HotellingT2 OrElse chart = SpcChartType.GeneralizedVariance, c.AllowsSubgroups)
        Assert.AreEqual(chart = SpcChartType.GeneralizedVariance, c.RequiresSubgroups)
        Assert.AreEqual(chart = SpcChartType.PcaT2 OrElse chart = SpcChartType.PcaQ, c.IsPca)
        Assert.AreEqual(chart = SpcChartType.Mewma OrElse chart = SpcChartType.Mcusum, c.IsSequential)
        For Each grouped In {False, True}
            Assert.AreEqual(chart = SpcChartType.HotellingT2 AndAlso Not grouped, c.UseLowerHotellingLimitAvailable(grouped))
        Next
        For Each supplied In {False, True}
            For Each gv In {False, True}
                Assert.AreEqual(chart <> SpcChartType.Mcusum AndAlso Not (chart = SpcChartType.Mewma AndAlso supplied) AndAlso Not (chart = SpcChartType.GeneralizedVariance AndAlso gv), c.AlphaEnabled(supplied, gv))
            Next
        Next
    End Sub

    <TestMethod>
    Public Sub GroupedNormalizationPreservesSignedZeroAndFailurePartialMutation()
        Dim map = SpcMultivariatePointMapPolicy.FromRows({101, 105, 118, 131}, True, {"A", "B", "A", "B"})
        Dim oldMap = Spc23LegacyPreparation.Map(101, 104, True, {"A", "B", "A", "B"})
        Dim original = {" A ", "B", "A", "B"}, a = CType(original.Clone(), String()), b = CType(original.Clone(), String())
        Dim negativeZero = BitConverter.Int64BitsToDouble(Long.MinValue)
        Dim s = {0.0R, 1.0R, negativeZero, 2.0R}, t = CType(s.Clone(), Double())
        Dim before = Failure(Sub() Spc23LegacyPreparation.NormalizeGroupedMetadata(a, s, oldMap))
        Dim after = Failure(Sub() map.NormalizeGroupedMetadata(b, t))
        SameFailure(before, after) : CollectionAssert.AreEqual(a, b)
        Assert.AreEqual(Spc23Fixture.Hash(s), Spc23Fixture.Hash(t))
        Assert.AreEqual(0L, BitConverter.DoubleToInt64Bits(t(0)), "Sequence expansion waits until every group validates")
        s(3) = 1 : t(3) = 1
        Spc23LegacyPreparation.NormalizeGroupedMetadata(a, s, oldMap) : map.NormalizeGroupedMetadata(b, t)
        Assert.AreEqual(Long.MinValue, BitConverter.DoubleToInt64Bits(t(0)))
        Assert.AreEqual(Spc23Fixture.Hash(s), Spc23Fixture.Hash(t))
    End Sub

    <DataTestMethod><DataRow(0)><DataRow(1)><DataRow(2)><DataRow(3)><DataRow(4)><DataRow(5)>
    Public Sub SixOrdinaryPublishedDataMethodsProduceAuthoritativeResults(id As Integer)
        Dim input = Spc23Fixture.InputFor(id)
        input.Context = Spc23LegacyPreparation.Map(input.FirstRow, input.FirstRow + 149, input.Grouped, input.Ids)
        Spc23Fixture.FillGrids(input, 0)
        Dim result = SpcMultivariate.Fit(Spc23Fixture.Prepare(input, True))
        Assert.AreEqual(1, result.Panels.Length)
        Assert.IsTrue(result.Panels(0).Points.Length > 0)
        Spc23Fixture.CompareResult(result)
    End Sub

    <DataTestMethod><DataRow(0)><DataRow(1)><DataRow(2)><DataRow(3)><DataRow(4)><DataRow(5)><DataRow(6)>
    Public Sub PublishedPhaseAndExclusionExampleKeepsActualModelAndMasks(id As Integer)
        Dim input = Spc23Fixture.InputFor(If(id = 6, 6, id))
        input.Options = New Spc23LegacyPreparation.RawOptions With {.FixedComponents = True, .Components = 2, .AllowPseudo = input.Chart <> SpcChartType.GeneralizedVariance}
        input.Mean = Nothing : input.Covariance = Nothing
        Dim n = input.Data.GetLength(0), grouped = input.Grouped
        input.Context = Spc23LegacyPreparation.Map(2, 151, grouped, input.Ids) : input.FirstRow = 2
        input.Labels = Enumerable.Range(0, n).Select(Function(i) If(grouped, "Subgroup " & (i \ 5 + 1).ToString("00", CultureInfo.InvariantCulture), "Obs-" & (i + 1).ToString("000", CultureInfo.InvariantCulture))).ToArray()
        input.Sequences = Enumerable.Range(0, n).Select(Function(i) CDbl(If(grouped, i \ 5 + 1, i + 1))).ToArray()
        Dim count = input.Context.PointCount, split = If(grouped, 20, 100)
        input.Legacy.AddStageRow("Baseline", 1, split, SpcPhase.PhaseI)
        input.Legacy.AddStageRow("Monitoring", split + 1, count, SpcPhase.PhaseII)
        For point = If(grouped, 8, 36) To If(grouped, 8, 40)
            input.Legacy.dgvExclusions.Rows.Add(New Spc23LegacyPreparation.DataGridViewRow(point, "Parameter estimation and signal evaluation", "Calibration check: exclude from estimation and signal evaluation"))
        Next
        Dim legacy = Spc23Fixture.Prepare(input, False), request = Spc23Fixture.Prepare(input, True)
        Assert.AreEqual(Spc23Fixture.Hash(legacy), Spc23Fixture.Hash(request))
        Dim result = SpcMultivariate.Fit(request)
        Assert.AreEqual(If(grouped, 19 * 5, 95), result.Model.BaselineObservationCount)
        Spc23Fixture.Emit(New XElement("comparison", New XAttribute("id", "published-" & id.ToString(CultureInfo.InvariantCulture)), New XAttribute("culture", CultureInfo.CurrentCulture.Name), New XAttribute("runtime", Environment.Version.ToString()), New XAttribute("chart", input.Chart), New XAttribute("classification", "NOT_GOLDEN"), New XAttribute("purpose", "SAME_CORE_COMPATIBILITY_ONLY"), New XAttribute("outcome", "POSITIVE_PARITY"), New XElement("request_sha256", Spc23Fixture.Hash(request)), Spc23Fixture.CompareResult(result)))
    End Sub

    <TestMethod>
    Public Sub PhysicalRowsGroupedIdentityAndDefensiveSnapshotsRemainDistinct()
        Dim rows = {101, 105, 118, 131}, ids = {" A ", "B", "A", "B"}
        Dim map = SpcMultivariatePointMapPolicy.FromRows(rows, True, ids)
        rows(0) = 999 : ids(0) = "changed"
        CollectionAssert.AreEqual(New Integer() {0, 1, 0, 1}, map.PointIndexByRow.ToArray())
        CollectionAssert.AreEqual(New Integer() {101, 118}, map.Points(0).SourceRowIndices.ToArray())
        Assert.AreEqual("A", map.Points(0).SubgroupId)
        Assert.AreEqual(1, map.Points(0).DisplayPoint)
        Dim labels = {" Ω ", "same", "Ω", "same"}, sequence = {Double.NaN, -1.0R, 0.0R, -1.0R}
        map.NormalizeGroupedMetadata(labels, sequence)
        CollectionAssert.AreEqual(New String() {"Ω", "same", "Ω", "same"}, labels)
        Assert.AreEqual(0.0R, sequence(0))
        Assert.ThrowsException(Of NotSupportedException)(Sub() DirectCast(map.Points(0).SourceRowIndices, IList(Of Integer))(0) = 12)
        Dim stages As New SpcMultivariateStageBuilder(2), exclusions As New SpcMultivariateExclusionBuilder(2)
        exclusions.IncludeScope(0, SpcExclusionScope.ParameterEstimation)
        exclusions.IncludeReason(0, "first") : exclusions.IncludeReason(0, "second") : exclusions.IncludeReason(0, "first")
        Dim expanded = map.Expand(stages.Finish(), exclusions.Finish())
        CollectionAssert.AreEqual(New String() {"first; second", "", "first; second", ""}, expanded.ExclusionReasons.ToArray())
        CollectionAssert.AreEqual(New Integer() {101, 105, 118, 131}, expanded.SourceRowIndices.ToArray())
        Assert.IsTrue(expanded.Phases.All(Function(p) p = SpcPhase.PhaseI))
    End Sub

    <TestMethod>
    Public Sub HistoricalAxesAreDetachedAndNoCovarianceCorrectionOccurs()
        Dim names = {"Ω", "B"}, mean = {1.0R, 2.0R}, covariance = New Double(,) {{1, 0.2R}, {0.3R, -1}}
        Dim intent As New SpcMultivariateModelIntent(names, SpcMultivariateModelSource.UseHistoricalParameters, mean, covariance)
        names(0) = "changed" : mean(0) = 99 : covariance(0, 0) = 99
        Assert.AreEqual("Ω", intent.VariableNames(0))
        Assert.AreEqual(1.0R, intent.HistoricalMean(0))
        Assert.AreEqual(-1.0R, intent.HistoricalCovariance(1, 1))
        Dim copy = intent.HistoricalCovariance : copy(0, 1) = 99
        Assert.AreEqual(0.2R, intent.HistoricalCovariance(0, 1))
        Assert.AreEqual(0.3R, intent.HistoricalCovariance(1, 0))
    End Sub

    <TestMethod>
    Public Sub ResultContextAndRequestArraysAreDetachedFromCallerMutations()
        Dim input = Spc23Fixture.InputFor(0)
        input.Context = Spc23LegacyPreparation.Map(1, 150, False, Nothing) : Spc23Fixture.FillGrids(input, 0)
        Dim request = Spc23Fixture.Prepare(input, True), requestHash = Spc23Fixture.Hash(request)
        input.Data(0, 0) = 999 : input.Names(0) = "changed" : input.Labels(0) = "changed" : input.Sequences(0) = 999
        Assert.AreEqual(requestHash, Spc23Fixture.Hash(request))
        Dim result = SpcMultivariate.Fit(request), context = SpcMultivariateResultAxisPolicy.Project(result)
        Dim panels = result.Panels, diagnostics = result.Diagnostics, names = result.Request.VariableNames
        panels(0) = Nothing : diagnostics(0) = Nothing : names(0) = "changed"
        Assert.IsNotNull(context.Panels(0)) : Assert.IsNotNull(context.Diagnostics(0)) : Assert.AreEqual("Temperature_C", context.VariableNames(0))
        Assert.ThrowsException(Of NotSupportedException)(Sub() DirectCast(context.Panels, IList(Of SpcPanelResult))(0) = Nothing)
        Assert.AreEqual(Spc23Fixture.Hash(Spc23LegacyPreparation.RenderingInput(context.Panels(0))), Spc23Fixture.Hash(SpcMultivariateResultAxisPolicy.RenderingInput(context.Panels(0))))
    End Sub

    <TestMethod>
    Public Sub SparseRenderingInputKeepsFirstSourceAndCoreIndexFallback()
        Dim points = {New SpcPointResult(3, 1.2345678901234567R, 1, 0, 5, label:="same Ω", sourceRowIndices:={101, 118}, sequenceValue:=-2), New SpcPointResult(9, Double.NaN, 1, 0, 5, label:="same Ω")}
        Dim panel As New SpcPanelResult(SpcPanelType.HotellingT2, "T2", points)
        Assert.AreEqual(Spc23Fixture.Hash(Spc23LegacyPreparation.RenderingInput(panel)), Spc23Fixture.Hash(SpcMultivariateResultAxisPolicy.RenderingInput(panel)))
        Dim wrapped = SpcMultivariateResultAxisPolicy.RenderingInput(panel)
        Assert.AreEqual(Spc23Fixture.Hash(New Integer() {101, 9}), Spc23Fixture.Hash(wrapped.SourceRowIndices))
    End Sub

    <DataTestMethod><DataRow(1)><DataRow(2)><DataRow(50)>
    Public Sub QuickPhaseRowsAndRejectionsMatchCapturedSource(count As Integer)
        For Each mode In {SpcMultivariateQuickPhase.AllPhaseI, SpcMultivariateQuickPhase.PhaseIThenPhaseII, SpcMultivariateQuickPhase.AllPhaseII}
            For Each historical In {False, True}
                For Each splitPoint As Integer In {0, 1, count - 1, count}
                    Dim legacy As New Spc23LegacyPreparation(), rows As IReadOnlyList(Of SpcMultivariateStageRow) = Nothing
                    Dim before = Failure(Sub() legacy.Quick(New Spc23LegacyPreparation.InputRowContext With {.PointCount=count}, mode = SpcMultivariateQuickPhase.AllPhaseI, mode = SpcMultivariateQuickPhase.AllPhaseII, splitPoint, If(historical, SpcMultivariateModelSource.UseHistoricalParameters, SpcMultivariateModelSource.EstimateFromPhaseI)))
                    Dim after = Failure(Sub() rows = SpcMultivariatePhasePolicy.Quick(count, mode, splitPoint, If(historical, SpcMultivariateModelSource.UseHistoricalParameters, SpcMultivariateModelSource.EstimateFromPhaseI)))
                    SameFailure(before, after)
                    If before Is Nothing Then
                        Assert.AreEqual(legacy.dgvStages.Rows.Count, rows.Count)
                        For i = 0 To rows.Count - 1
                            Assert.AreEqual(CStr(legacy.dgvStages.Rows(i).Cells(0).Value), rows(i).StageId)
                            Assert.AreEqual(CInt(legacy.dgvStages.Rows(i).Cells(1).Value), rows(i).FirstPoint)
                            Assert.AreEqual(CInt(legacy.dgvStages.Rows(i).Cells(2).Value), rows(i).LastPoint)
                        Next
                    End If
                Next
            Next
        Next
    End Sub

    <DataTestMethod><DataRow(0)><DataRow(1)><DataRow(2)><DataRow(3)><DataRow(4)><DataRow(5)><DataRow(6)><DataRow(7)>
    Public Sub ImportedPhaseRunsPreserveCollapseAndPartialGridErrors(id As Integer)
        Dim keys = {"A", "B", "A", "B", "C", "D"}
        Dim map = SpcMultivariatePointMapPolicy.FromWorksheetRows(2, 7, True, keys), legacyMap = Spc23LegacyPreparation.Map(2, 7, True, keys)
        Dim phases As String() = {"I", "I", "1", "Phase I", "II", "I"}, stages As String() = Nothing
        Select Case id
            Case 1 : phases = Nothing : stages = {"X", "X", "X", "X", "Y", "Z"}
            Case 2 : stages = {"X", "Y", "X", "Y", "X", "Z"}
            Case 3 : stages = {"X", "x", "X", "x", "Y", "Z"}
            Case 4 : phases(2) = "II"
            Case 5 : phases(0) = ""
            Case 6 : stages = {"", "X", "", "X", "Y", "Z"}
            Case 7 : phases = {"I", "II", "I", "II", "I", "II"}
        End Select
        Dim legacy As New Spc23LegacyPreparation(), rows As New List(Of SpcMultivariateStageRow)
        Dim before = Failure(Sub() legacy.Imported(legacyMap, stages, phases))
        Dim after = Failure(Sub()
                                Dim assignments = SpcMultivariatePhasePolicy.CollapseImported(map, stages, phases)
                                For Each row In SpcMultivariatePhasePolicy.ImportedRows(assignments) : rows.Add(row) : Next
                            End Sub)
        SameFailure(before, after)
        Assert.AreEqual(legacy.dgvStages.Rows.Count, rows.Count, "Lazy enumeration preserves partial successful rows")
        For i = 0 To rows.Count - 1
            Assert.AreEqual(CStr(legacy.dgvStages.Rows(i).Cells(0).Value), rows(i).StageId)
            Assert.AreEqual(CInt(legacy.dgvStages.Rows(i).Cells(1).Value), rows(i).FirstPoint)
            Assert.AreEqual(CInt(legacy.dgvStages.Rows(i).Cells(2).Value), rows(i).LastPoint)
        Next
    End Sub

    <TestMethod>
    Public Sub ExclusionImportUsesOnlyFlaggedRowsReasonsAndOrdinalUniqueness()
        Dim map = SpcMultivariatePointMapPolicy.FromRows({101, 105, 118, 131}, True, {"A", "B", "A", "B"})
        Dim importer As New SpcMultivariateExclusionImport(map), reasons = {"unflagged", "Ω", "first", "Ω"}
        importer.IncludeRow(0, False, reasons) : importer.IncludeRow(1, True, reasons)
        importer.IncludeRow(2, True, reasons) : importer.IncludeRow(3, True, reasons)
        Dim rows = importer.Rows()
        Assert.AreEqual(2, rows.Count) : Assert.AreEqual(1, rows(0).DisplayPoint) : Assert.AreEqual("first", rows(0).Reason)
        Assert.AreEqual("Ω", rows(1).Reason)
    End Sub

    <TestMethod>
    Public Sub ThousandsOfNoncontiguousMembersRetainFirstSeenOrderAcrossRuns()
        Dim keys = Enumerable.Range(0, 12000).Select(Function(i) "G" & (i Mod 3000).ToString(CultureInfo.InvariantCulture)).ToArray()
        Dim first = SpcMultivariatePointMapPolicy.FromWorksheetRows(101, 12100, True, keys)
        Assert.AreEqual(3000, first.PointCount)
        For i = 0 To first.PointCount - 1
            Assert.AreEqual(i, first.Points(i).PointIndex)
            Assert.AreEqual(4, first.Points(i).SourceRowIndices.Count)
        Next
        Dim second = SpcMultivariatePointMapPolicy.FromWorksheetRows(1, 2, False, Nothing)
        Assert.AreEqual(2, second.PointCount) : Assert.AreEqual(1, second.Points(0).SourceRowIndices(0))
        Assert.AreEqual(101, first.Points(0).SourceRowIndices(0))
    End Sub
    Public Shared Iterator Function CovarianceProfiles() As IEnumerable(Of Object())
        For Each culture In {"", "sk-SK"}
            For id = 0 To 35 : Yield New Object() {id, culture} : Next
        Next
    End Function

    <DataTestMethod, DynamicData(NameOf(CovarianceProfiles), DynamicDataSourceType.Method)>
    Public Sub SingularTiedAndTwoVariableModelChoicesKeepOriginalRequests(id As Integer, culture As String)
        Dim saved = CultureInfo.CurrentCulture
        Try
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(culture)
            Dim profileKind = id \ 6, chartIndex = id Mod 6, input = Spc23Fixture.InputFor(chartIndex), n = input.Data.GetLength(0)
            If profileKind < 4 Then
                For row = 0 To n - 1 : input.Data(row, 1) = input.Data(row, 0) : Next
            ElseIf profileKind = 5 Then
                Dim reduced(n - 1, 1) As Double
                For row = 0 To n - 1 : reduced(row, 0) = input.Data(row, 0) : reduced(row, 1) = input.Data(row, 1) : Next
                input.Data = reduced : input.Names = {"Temperature_C", "Pressure_kPa"}
            End If
            Dim historical = profileKind = 3 OrElse profileKind = 4
            input.Options = New Spc23LegacyPreparation.RawOptions With {
                .ModelSource = If(historical, SpcMultivariateModelSource.UseHistoricalParameters, SpcMultivariateModelSource.EstimateFromPhaseI),
                .AllowPseudo = input.Chart <> SpcChartType.GeneralizedVariance AndAlso (profileKind = 1 OrElse profileKind = 3),
                .Ridge = If(profileKind = 2, 0.001R, 0.0R), .FixedComponents = True, .Components = If(profileKind = 5, 1, 2), .Correlation = profileKind Mod 2 = 1}
            input.Mean = If(historical AndAlso input.Chart <> SpcChartType.GeneralizedVariance, New Double() {50, If(profileKind = 3, 50.0R, 100.0R), 20}, Nothing)
            input.Covariance = If(profileKind = 3, New Double(,) {{1, 1, 0}, {1, 1, 0}, {0, 0, 1}}, If(profileKind = 4, New Double(,) {{1, 0, 0}, {0, 1, 0}, {0, 0, 1}}, Nothing))
            input.Context = Spc23LegacyPreparation.Map(input.FirstRow, input.FirstRow + n - 1, input.Grouped, input.Ids)
            Spc23Fixture.FillGrids(input, 0)
            Dim oldRequest As SpcMultivariateRequest = Nothing, request As SpcMultivariateRequest = Nothing
            Dim before = Failure(Sub() oldRequest = Spc23Fixture.Prepare(input, False)), after = Failure(Sub() request = Spc23Fixture.Prepare(input, True))
            SameFailure(before, after)
            Dim node As New XElement("comparison", New XAttribute("id", "rank-" & id.ToString(CultureInfo.InvariantCulture)), New XAttribute("culture", culture), New XAttribute("runtime", Environment.Version.ToString()), New XAttribute("chart", input.Chart), New XAttribute("classification", "NOT_GOLDEN"), New XAttribute("purpose", "SAME_CORE_COMPATIBILITY_ONLY"), New XElement("dataset_sha256", Spc23Fixture.Hash(input.Data)), New XElement("requested_options", input.Options.GetType().GetFields().OrderBy(Function(f) f.Name, StringComparer.Ordinal).Select(Function(f) New XElement("option", New XAttribute("name", f.Name), New XAttribute("type", f.FieldType.FullName), Spc21Fixture.Text(f.GetValue(input.Options))))))
            Dim stage = "request"
            If before Is Nothing Then
                Assert.AreEqual(Spc23Fixture.Hash(oldRequest), Spc23Fixture.Hash(request))
                node.Add(New XElement("request_sha256", Spc23Fixture.Hash(request)))
                Dim result As SpcMultivariateFitResult = Nothing : stage = "core-fit"
                before = Failure(Sub() result = SpcMultivariate.Fit(request))
                If input.Chart = SpcChartType.HotellingT2 AndAlso (profileKind = 1 OrElse profileKind = 3) Then
                    Assert.IsNull(before, "Authoritative supported pseudoinverse fixture")
                    Assert.IsTrue(result.Model.UsedPseudoInverse)
                    Assert.AreEqual(2, result.Model.EffectiveDimension)
                End If
                If before Is Nothing Then
                    Assert.AreEqual(If(profileKind = 5, 2, 3), result.Request.VariableNames.Length)
                    node.Add(New XAttribute("outcome", "POSITIVE_PARITY"), Spc23Fixture.CompareResult(result))
                End If
            End If
            If before IsNot Nothing Then
                node.Add(New XAttribute("outcome", If(stage = "core-fit", "EXISTING_CORE_REJECTION", "EXISTING_PREPARATION_EXCEPTION")), New XElement("exception", New XAttribute("stage", stage), New XAttribute("type", before.GetType().FullName), New XAttribute("parameter", If(TryCast(before, ArgumentException)?.ParamName, "")), before.Message))
            End If
            Spc23Fixture.Emit(node)
        Finally
            CultureInfo.CurrentCulture = saved
        End Try
    End Sub

End Class
