Option Explicit On
Option Strict On
Imports System
Imports System.Collections.Generic
Imports System.Globalization
Imports System.Linq
Imports System.Threading
Imports System.Threading.Tasks
Imports BESHStatNG
Imports BESHStatNG.ExcelCommon
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass>
Public Class AnovaWorkflow07Tests
    Public Shared Function Groups() As AssumptionReportInput
        Return New AssumptionReportInput({New Double() {7, 1, 3, 4}, New Double() {9, 2, 6, 8, 3}, New Double() {11, 4, 5}}, {"Z'eta", "0", "alpha"})
    End Function
    Public Shared Function OptionCases() As IEnumerable(Of Object())
        Return From bits In Enumerable.Range(0, 32) From culture In New String() {"", "sk-SK"} Select New Object() {bits, culture}
    End Function
    Public Shared Function RepeatedMatrix() As Double(,)
        Return New Double(,) {{1, 1.2, 1.1}, {0.9, 1.1, 1}, {1.1, 1.3, 1.25}, {1.05, 1.15, 1.2}, {0.95, 1.05, 1}, {1.2, 1.25, 1.3}}
    End Function
    Public Shared Function NestedMatrix(Optional unbalanced As Boolean = False, Optional duplicateChild As Boolean = False) As Object(,)
        Dim rows As New List(Of Object()) From {New Object() {"B", "b2", 10.0}, New Object() {"A", "a1", 1.0}, New Object() {"A", "a2", 7.0},
            New Object() {"B", "b1", 2.0}, New Object() {"A", "a1", 3.0}, New Object() {"B", "b2", 8.0}, New Object() {"A", "a2", 5.0}, New Object() {"B", "b1", 4.0}}
        If unbalanced Then rows.Add(New Object() {"A", "a1", 2.0})
        Dim result(rows.Count - 1, 2) As Object
        For r = 0 To rows.Count - 1
            For c = 0 To 2
                result(r, c) = rows(r)(c)
            Next
            If duplicateChild AndAlso CStr(result(r, 1)) = "b1" Then result(r, 1) = "a1"
        Next
        Return result
    End Function

    <DataTestMethod> <DynamicData(NameOf(OptionCases), DynamicDataSourceType.Method)>
    Public Sub EveryOneWayOptionCombinationUsesExistingDispatcherOnceAndStablePlots(bits As Integer, culture As String)
        Dim saved = Thread.CurrentThread.CurrentCulture
        Try
            Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo(culture)
            Dim input = Groups(), original = input.Groups
            Dim options As New AnovaGroupedWorkflowOptions(SpreadsheetAnalysisKind.OneWayAnova, welch:=(bits And 1) <> 0,
                lsd:=(bits And 2) <> 0, bonferroni:=(bits And 4) <> 0, tukey:=(bits And 8) <> 0, gamesHowell:=(bits And 16) <> 0)
            Dim result = AnovaWorkflowPolicy.RunGrouped(input, options)
            Assert.IsTrue(result.Success, String.Join(";", result.Validation.Issues.Select(Function(i) i.Message)))
            Dim legacy = OneWayAnovaApplicationService.Run(New GroupedNumericData(input.Names, input.Groups), DirectCast(options.ToAnalysisOptions(), OneWayAnovaAnalysisOptions))
            Assert.AreEqual(legacy.FStatistic, result.LegacyAnalysis.Metrics(AnalysisMetricNames.FStatistic), 1.0E-12)
            Assert.AreEqual(legacy.PValue, result.LegacyAnalysis.Metrics(AnalysisMetricNames.PValue), 1.0E-12)
            Assert.AreEqual(legacy.Tables.Count, result.LegacyAnalysis.Tables.Count)
            Assert.AreEqual(legacy.HasWelch, (bits And 1) <> 0)
            If legacy.HasWelch Then Assert.AreEqual(legacy.WelchDenominatorDf, result.LegacyAnalysis.Metrics(AnalysisMetricNames.WelchDenominatorDf), 1.0E-12)
            Dim expectedPlots = New parametric.MultipleComparisonPlotData() {legacy.FisherLsdPlotData, legacy.BonferroniPlotData, legacy.TukeyKramerPlotData, legacy.GamesHowellPlotData}.Where(Function(p) p IsNot Nothing).ToArray()
            Assert.AreEqual(expectedPlots.Length, result.ComparisonPlots.Count)
            Assert.AreEqual(1 + expectedPlots.Length, result.LegacyAnalysis.Tables.Count)
            For p = 0 To expectedPlots.Length - 1
                Assert.AreEqual(expectedPlots(p).ProcedureName, result.ComparisonPlots(p).ProcedureName)
                CollectionAssert.AreEqual(expectedPlots(p).Labels, result.ComparisonPlots(p).Labels)
                CollectionAssert.AreEqual(expectedPlots(p).Estimates, result.ComparisonPlots(p).Estimates)
                CollectionAssert.AreEqual(expectedPlots(p).LowerLimits, result.ComparisonPlots(p).LowerLimits)
                CollectionAssert.AreEqual(expectedPlots(p).UpperLimits, result.ComparisonPlots(p).UpperLimits)
            Next
            If result.ComparisonPlots.Count > 0 Then
                Dim changed = result.ComparisonPlots(0)
                changed.Labels(0) = "changed" : changed.Estimates(0) = 1000
                Assert.AreNotEqual("changed", result.ComparisonPlots(0).Labels(0))
            End If
            For g = 0 To original.Length - 1
                CollectionAssert.AreEqual(original(g), input.Groups(g))
            Next
            CollectionAssert.AreEqual(New String() {"Z'eta", "0", "alpha"}, input.Names)
        Finally
            Thread.CurrentThread.CurrentCulture = saved
        End Try
    End Sub

    <DataTestMethod> <DataRow("C30-SYN-ANOVA")> <DataRow("C30-SYN-KW")>
    Public Sub C30CandidatesRetainDistinctMetricsAndOriginalSyntheticRows(id As String)
        Dim fixture = MigrationFixture04A02.Load(id)
        MigrationFixture04A02.Validate(fixture)
        Dim grouped = MigrationFixture04A02.Grouped(fixture)
        Dim sources As New List(Of UnivariateObservationSource)
        For g = 0 To grouped.Groups.Length - 1
            For r = 0 To grouped.Groups(g).Length - 1
                Dim row = fixture("dataset")("row_ids")(g)(r).GetValue(Of Integer)()
                sources.Add(New UnivariateObservationSource(g, r, r + 1, g + 1, row, 0, "", "", "synthetic fixture", ""))
            Next
        Next
        Dim input As New AssumptionReportInput(grouped.Groups, grouped.GroupNames, sources)
        Dim result = AnovaWorkflowPolicy.RunGrouped(input, New AnovaGroupedWorkflowOptions(If(id = "C30-SYN-ANOVA", SpreadsheetAnalysisKind.OneWayAnova, SpreadsheetAnalysisKind.KruskalWallis)))
        Assert.IsTrue(result.Success)
        CollectionAssert.AreEqual(sources.Select(Function(s) s.WorksheetRow).ToArray(), result.Input.Sources.Select(Function(s) s.WorksheetRow).ToArray())
        If id = "C30-SYN-ANOVA" Then
            'Retained owner-qualified R candidates, NOT_GOLDEN. No new numerical oracle is generated.
            Assert.AreEqual(2.3333333333333335, result.LegacyAnalysis.Metrics(AnalysisMetricNames.FStatistic), 1.0E-12)
            Assert.AreEqual(0.177978515625, result.LegacyAnalysis.Metrics(AnalysisMetricNames.PValue), 1.0E-12)
            Dim values = result.LegacyAnalysis.PrimaryTable.Values
            Assert.AreEqual(14.0, CDbl(values(1, 1)), 1.0E-12)
            Assert.AreEqual(18.0, CDbl(values(2, 1)), 1.0E-12)
            Assert.AreEqual(32.0, CDbl(values(3, 1)), 1.0E-12)
        Else
            Assert.AreEqual(4.622222222222227, result.LegacyAnalysis.Metrics(AnalysisMetricNames.HStatistic), 1.0E-12)
            Assert.AreEqual(4.781609195402297, result.LegacyAnalysis.Metrics(AnalysisMetricNames.TieCorrectedHStatistic), 1.0E-12)
            Assert.AreEqual(0.09915102252982504, result.LegacyAnalysis.Metrics(AnalysisMetricNames.PValue), 1.0E-12)
            Assert.AreEqual(0.09155598849604304, result.LegacyAnalysis.Metrics(AnalysisMetricNames.TieCorrectedPValue), 1.0E-12)
        End If
    End Sub

    <TestMethod>
    Public Sub ExistingWelchReferenceKeepsDenominatorDfInItsNamedField()
        Dim input As New AssumptionReportInput({New Double() {1, 2, 3}, New Double() {2, 3, 4}, New Double() {5, 6, 7}}, {"G1", "G2", "G3"})
        Dim result = AnovaWorkflowPolicy.RunGrouped(input, New AnovaGroupedWorkflowOptions(SpreadsheetAnalysisKind.OneWayAnova, welch:=True))
        Assert.IsTrue(result.Success)
        Assert.AreEqual(4.0, result.LegacyAnalysis.Metrics(AnalysisMetricNames.WelchDenominatorDf), 1.0E-12)
        Assert.AreEqual(11.142857142857142, result.LegacyAnalysis.Metrics(AnalysisMetricNames.WelchFStatistic), 1.0E-9)
        Assert.AreEqual(0.023156899810964086, result.LegacyAnalysis.Metrics(AnalysisMetricNames.WelchPValue), 5.0E-8)
    End Sub

    <DataTestMethod> <DataRow(False)> <DataRow(True)>
    Public Sub DunnIsOptionalAtApiAndRemainsBonferroni(dunn As Boolean)
        Dim result = AnovaWorkflowPolicy.RunGrouped(Groups(), New AnovaGroupedWorkflowOptions(SpreadsheetAnalysisKind.KruskalWallis, dunn:=dunn))
        Assert.IsTrue(result.Success)
        Assert.AreEqual(If(dunn, 2, 1), result.LegacyAnalysis.Tables.Count)
        If dunn Then Assert.IsTrue(result.LegacyAnalysis.Tables(1).Values.Cast(Of Object)().OfType(Of String)().Any(Function(s) s.IndexOf("Dunn", StringComparison.OrdinalIgnoreCase) >= 0))
    End Sub

    Public Shared Function SupplementCases() As IEnumerable(Of Object())
        Return From kind In New SpreadsheetAnalysisKind() {SpreadsheetAnalysisKind.OneWayAnova, SpreadsheetAnalysisKind.KruskalWallis}
               From flags In Enumerable.Range(0, 8) Select New Object() {kind, flags}
    End Function
    <DataTestMethod> <DynamicData(NameOf(SupplementCases), DynamicDataSourceType.Method)>
    Public Sub SupplementOrderingIsAnalysisSpecific(kind As SpreadsheetAnalysisKind, flags As Integer)
        Dim options As New AnovaGroupedWorkflowOptions(kind, homogeneity:=If((flags And 1) <> 0, HomogeneityMethods.All, HomogeneityMethods.None),
            descriptive:=(flags And 2) <> 0, boxSummary:=(flags And 4) <> 0)
        Dim result = AnovaWorkflowPolicy.RunGrouped(Groups(), options)
        Assert.IsTrue(result.Success)
        Dim expected As New List(Of AnovaWorkflowStage) From {AnovaWorkflowStage.PrimaryTables}
        If kind = SpreadsheetAnalysisKind.OneWayAnova Then
            If (flags And 1) <> 0 Then expected.Add(AnovaWorkflowStage.Homogeneity)
            If (flags And 2) <> 0 Then expected.Add(AnovaWorkflowStage.Descriptive)
        Else
            If (flags And 2) <> 0 Then expected.Add(AnovaWorkflowStage.Descriptive)
            If (flags And 1) <> 0 Then expected.Add(AnovaWorkflowStage.Homogeneity)
        End If
        If (flags And 4) <> 0 Then expected.Add(AnovaWorkflowStage.WindowsBoxSummary)
        CollectionAssert.AreEqual(expected.ToArray(), result.Plan.Stages.ToArray())
        Dim report = AnovaWorkflowPolicy.BuildSupplements(Groups(), options)
        Assert.IsTrue(report.Success)
        Assert.AreEqual(If((flags And 1) <> 0, 1, 0) + If((flags And 2) <> 0, 1, 0), report.Tables.Count)
        Assert.AreEqual(1, result.Plan.BlankRowsBetweenTables)
    End Sub

    <DataTestMethod> <DataRow(0)> <DataRow(1)> <DataRow(2)> <DataRow(3)> <DataRow(4)> <DataRow(5)> <DataRow(6)> <DataRow(7)>
    <DataRow(8)> <DataRow(9)> <DataRow(10)> <DataRow(11)> <DataRow(12)> <DataRow(13)> <DataRow(14)> <DataRow(15)>
    Public Sub EveryRepeatedFlagPreservesCoreCallAndTableOrder(bits As Integer)
        Dim matrix = RepeatedMatrix(), copy = DirectCast(matrix.Clone(), Double(,))
        Dim options As New RepeatedAnovaWorkflowOptions(greenhouseGeisser:=(bits And 1) <> 0, huynhFeldt:=(bits And 2) <> 0, tukey:=(bits And 4) <> 0, mauchly:=(bits And 8) <> 0)
        Dim result = AnovaWorkflowPolicy.RunRepeated(matrix, {"A", "B", "C"}, options)
        Assert.IsTrue(result.Success, String.Join(";", result.Validation.Issues.Select(Function(i) i.Message)))
        Dim expected As New List(Of String) From {"compute"}
        If (bits And 1) <> 0 Then expected.Add("GreenhouseGeisser")
        If (bits And 2) <> 0 Then expected.Add("HuyhnFeldt")
        If (bits And 4) <> 0 Then expected.AddRange({"TukeyKramerRM2", "Tukey"})
        expected.Add("wrapResults")
        If (bits And 8) <> 0 Then expected.Add("MauchlyTest")
        CollectionAssert.AreEqual(expected.ToArray(), result.Calls.ToArray())
        Assert.AreEqual(1 + If((bits And 4) <> 0, 2, 0) + If((bits And 8) <> 0, 1, 0), result.Tables.Count)
        CollectionAssert.AreEqual(copy, matrix)
        If (bits And 8) <> 0 Then
            Dim model = result.Tables.Last().Artifact.Model
            Assert.AreEqual("Mauchly 's Test of Sphericity", model.Values(0, 0))
            Assert.AreEqual(1.7236099959966962, CDbl(model.Values(1, 1)), 1.0E-9)
            Assert.AreEqual(0.63169757298113693, CDbl(model.Values(2, 1)), 1.0E-9)
            Assert.AreEqual(2, model.PvalueCells.Single().BodyRow)
        End If
    End Sub

    <TestMethod>
    Public Sub ExistingRepeatedReferenceAndOutputRoundTripHaveExactMetadata()
        Dim matrix As Double(,) = {{1, 2, 3}, {2, 2, 4}, {3, 4, 5}, {4, 3, 6}}
        Dim result = AnovaWorkflowPolicy.RunRepeated(matrix, {"C1", "C2", "C3"}, New RepeatedAnovaWorkflowOptions(greenhouseGeisser:=True, huynhFeldt:=True, tukey:=True))
        Assert.IsTrue(result.Success)
        Dim model = result.Tables(0).Artifact.Model
        Assert.AreEqual(9.5, CDbl(model.Values(1, 1)), 1.0E-12)
        Assert.AreEqual(15.545454545454524, CDbl(model.Values(1, 4)), 1.0E-9)
        Assert.AreEqual(0.0042330297170771669, CDbl(model.Values(1, 5)), 5.0E-8)
        Assert.AreEqual(0.5, CDbl(model.Values(1, 6)), 1.0E-9)
        For Each table In result.Tables
            Dim old = table.Artifact.Model, roundTrip = table.ToLegacyTable().ToOutputModel()
            CollectionAssert.AreEqual(old.Values, roundTrip.Values)
            CollectionAssert.AreEqual(old.PvalueColumns, roundTrip.PvalueColumns)
            Assert.AreEqual(old.FooterRows, roundTrip.FooterRows)
        Next
    End Sub

    <DataTestMethod> <DataRow(False)> <DataRow(True)>
    Public Sub NestedBalancedMetadataAndIndependentSumOfSquares(unbalanced As Boolean)
        Dim matrix = NestedMatrix(unbalanced), original = DirectCast(matrix.Clone(), Object(,))
        Dim result = AnovaWorkflowPolicy.RunNested(matrix, {"Group", "Nested", "Value"})
        Assert.IsTrue(result.Success, String.Join(";", result.Validation.Issues.Select(Function(i) i.Message)))
        Assert.AreEqual(Not unbalanced, result.BalancedDesign.Value)
        Assert.AreEqual("Two-Way Nested ANOVA", result.Banner(0, 0))
        Assert.AreEqual("Balanced design = " & (Not unbalanced).ToString(), result.Banner(1, 0))
        Assert.AreEqual("Satterthwaite approximation", result.Separators(0)(1, 0))
        Assert.AreEqual(2, result.Tables.Count)
        If Not unbalanced Then
            'Independent small arithmetic fixture, NOT_GOLDEN: grand mean 5, SS 8+52+8=68.
            Dim values = result.Tables(0).Artifact.Model.Values
            Assert.AreEqual(8.0, CDbl(values(1, 1)), 1.0E-12)
            Assert.AreEqual(52.0, CDbl(values(2, 1)), 1.0E-12)
            Assert.AreEqual(8.0, CDbl(values(3, 1)), 1.0E-12)
            Assert.AreEqual(68.0, CDbl(values(4, 1)), 1.0E-12)
        End If
        CollectionAssert.AreEqual(original, matrix)
        Dim mutated = result.Banner : mutated(0, 0) = "bad"
        Assert.AreEqual("Two-Way Nested ANOVA", result.Banner(0, 0))
    End Sub

    <TestMethod>
    Public Sub RepeatedGlobalNestedLabelIsRejectedWithoutScientificRecoding()
        Dim result = AnovaWorkflowPolicy.RunNested(NestedMatrix(duplicateChild:=True), {"Group", "Nested", "Value"})
        Assert.IsFalse(result.Success)
        Assert.AreEqual(0, result.Tables.Count)
        Assert.IsTrue(result.Validation.Issues.Any(Function(i) i.Code = "ANOVA_NESTED_GLOBAL_LABEL"))
        Assert.IsTrue(result.Validation.HasErrors)
        Assert.IsFalse(result.BalancedDesign.HasValue)
    End Sub

    <DataTestMethod> <DataRow(0)> <DataRow(1)> <DataRow(2)> <DataRow(3)> <DataRow(4)> <DataRow(5)>
    Public Sub InvalidGroupedInputsAndCancellationHaveNoAcceptedPrimary(which As Integer)
        Dim input = Groups(), options As New AnovaGroupedWorkflowOptions(SpreadsheetAnalysisKind.OneWayAnova)
        Select Case which
            Case 0 : input = Nothing
            Case 1 : input = New AssumptionReportInput({New Double() {1, 2}}, {"only"})
            Case 2 : input = New AssumptionReportInput({New Double() {1, Double.NaN}, New Double() {2, 3}}, {"A", "B"})
            Case 3 : input = New AssumptionReportInput({New Double() {1, Double.PositiveInfinity}, New Double() {2, 3}}, {"A", "B"})
            Case 4 : options = New AnovaGroupedWorkflowOptions(CType(99, SpreadsheetAnalysisKind))
        End Select
        Dim result = AnovaWorkflowPolicy.RunGrouped(input, options, New CancellationToken(which = 5))
        Assert.IsFalse(result.Success)
        Assert.IsNull(result.LegacyAnalysis)
        Assert.AreEqual(0, result.ComparisonPlots.Count)
        Assert.IsTrue(result.Validation.HasErrors)
    End Sub

    <DataTestMethod> <DataRow(0.0)> <DataRow(1.0)> <DataRow(-0.01)> <DataRow(Double.NaN)> <DataRow(Double.PositiveInfinity)>
    Public Sub GroupedAlphaDefaultWarningAndRepeatedStrictOptionBoundaryAreExplicit(alpha As Double)
        For Each kind In {SpreadsheetAnalysisKind.OneWayAnova, SpreadsheetAnalysisKind.KruskalWallis}
            Dim result = AnovaWorkflowPolicy.RunGrouped(Groups(), New AnovaGroupedWorkflowOptions(kind, alpha))
            Assert.IsTrue(result.Success)
            Assert.IsTrue(result.Validation.Issues.Any(Function(i) i.Code = AnalysisWarningCodes.AlphaDefaulted))
        Next
        Dim invalid = AnovaWorkflowPolicy.RunRepeated(RepeatedMatrix(), {"A", "B", "C"}, New RepeatedAnovaWorkflowOptions(alpha))
        Assert.IsFalse(invalid.Success)
        Assert.AreEqual(0, invalid.Tables.Count)
    End Sub

    <TestMethod>
    Public Sub LegacyIdsDefaultsAndFreshConcurrentRequestsStayIndependent()
        Assert.AreEqual(1, CInt(SpreadsheetAnalysisKind.OneWayAnova))
        Assert.AreEqual(2, CInt(SpreadsheetAnalysisKind.KruskalWallis))
        Assert.IsTrue(New KruskalWallisAnalysisOptions().IncludeDunnComparisons)
        Assert.IsFalse(New OneWayAnovaAnalysisOptions().IncludeWelch)
        Dim tasks = Enumerable.Range(1, 4).Select(Function(i) Task.Run(Function() AnovaWorkflowPolicy.RunGrouped(Groups(), New AnovaGroupedWorkflowOptions(SpreadsheetAnalysisKind.OneWayAnova)))).ToArray()
        Task.WaitAll(tasks)
        For Each task In tasks
            Assert.IsTrue(task.Result.Success)
            Assert.AreEqual(tasks(0).Result.LegacyAnalysis.Metrics(AnalysisMetricNames.FStatistic), task.Result.LegacyAnalysis.Metrics(AnalysisMetricNames.FStatistic))
        Next
    End Sub

    <DataTestMethod> <DataRow(False)> <DataRow(True)>
    Public Sub ConstantAndNearConstantGroupsKeepCoreOutcomeWithoutInventingFiniteResults(nearConstant As Boolean)
        Dim delta = If(nearConstant, 1.0E-12, 0.0)
        Dim input As New AssumptionReportInput({New Double() {1, 1 + delta, 1 + 2 * delta}, New Double() {1, 1 + delta, 1 + 2 * delta}}, {"same", "same"})
        For Each kind In {SpreadsheetAnalysisKind.OneWayAnova, SpreadsheetAnalysisKind.KruskalWallis}
            Dim result = AnovaWorkflowPolicy.RunGrouped(input, New AnovaGroupedWorkflowOptions(kind))
            If result.Success Then
                Dim hasUndefined = result.LegacyAnalysis.Tables.Any(Function(t) t.Values.Cast(Of Object)().OfType(Of Double)().Any(Function(v) Double.IsNaN(v) OrElse Double.IsInfinity(v)))
                If hasUndefined Then Assert.IsTrue(result.Validation.Issues.Any(Function(i) i.Code = "ANOVA_UNDEFINED_RESULT"))
            Else
                Assert.IsNull(result.LegacyAnalysis)
                Assert.IsTrue(result.Validation.HasErrors)
            End If
        Next
    End Sub

    <DataTestMethod> <DataRow(False)> <DataRow(True)>
    Public Sub TypedCompleteBlocksKeepNoncontiguousAndReversedPhysicalOrder(reversed As Boolean)
        Dim matrix As Double(,) = {{1, 3, 2}, {4, 7, 2}, {6, 3, 7}}
        Dim rows = If(reversed, New Integer() {107, 103, 101}, New Integer() {101, 103, 107})
        Dim blocks As New List(Of IEnumerable(Of SpreadsheetNumericObservation))
        For r = 0 To 2
            Dim cells As New List(Of SpreadsheetNumericObservation)
            For c = 0 To 2
                cells.Add(New SpreadsheetNumericObservation(matrix(r, c), r + 1, c + 1, rows(r), c + 2))
            Next
            blocks.Add(cells)
        Next
        Dim input As New SpreadsheetNumericBlocks({"B", "A", "0"}, blocks)
        Dim result = AnovaWorkflowPolicy.RunRepeated(input, New RepeatedAnovaWorkflowOptions())
        Assert.IsTrue(result.Success)
        CollectionAssert.AreEqual(rows, result.Blocks.Blocks.Select(Function(b) b(0).WorksheetRow).ToArray())
        CollectionAssert.AreEqual(matrix, input.Matrix)
    End Sub

    <DataTestMethod> <DataRow(0)> <DataRow(1)> <DataRow(2)>
    Public Sub DuplicateOrMisalignedBlockEvidenceIsRejectedBeforeCore(which As Integer)
        Dim matrix As Double(,) = {{1, 3, 2}, {4, 7, 2}, {6, 3, 7}}
        Dim blocks As New List(Of IEnumerable(Of SpreadsheetNumericObservation))
        For r = 0 To 2
            Dim cells As New List(Of SpreadsheetNumericObservation)
            For c = 0 To 2
                cells.Add(New SpreadsheetNumericObservation(matrix(r, c) + If(which = 1 AndAlso r = 0 AndAlso c = 0, 1, 0), r + 1, c + 1,
                    If(which = 0, 101, 101 + r) + If(which = 2 AndAlso c = 1, 10, 0), c + 2))
            Next
            blocks.Add(cells)
        Next
        Dim result = AnovaWorkflowPolicy.RunRepeated(matrix, {"A", "B", "C"}, New RepeatedAnovaWorkflowOptions(), New SpreadsheetNumericBlocks({"A", "B", "C"}, blocks))
        Assert.IsFalse(result.Success)
        Assert.AreEqual(0, result.Tables.Count)
        Assert.AreEqual(0, result.Calls.Count)
        Assert.IsTrue(result.Validation.Issues.Any(Function(i) i.Code = If(which = 0, "ANOVA_DUPLICATE_BLOCK", "ANOVA_SOURCE_MAP")))
    End Sub

    <DataTestMethod> <DataRow(0)> <DataRow(1)> <DataRow(2)> <DataRow(3)> <DataRow(4)>
    Public Sub InvalidNestedRowsAndCancellationReturnNoBannerOrTables(which As Integer)
        Dim matrix = NestedMatrix()
        Select Case which
            Case 0 : matrix(0, 0) = Nothing
            Case 1 : matrix(0, 1) = ""
            Case 2 : matrix(0, 2) = Double.PositiveInfinity
            Case 3 : matrix = New Object(,) {{"A", 1}}
        End Select
        Dim result = AnovaWorkflowPolicy.RunNested(matrix, {"Group", "Child", "Value"}, cancellation:=New CancellationToken(which = 4))
        Assert.IsFalse(result.Success)
        Assert.AreEqual(0, result.Tables.Count)
        Assert.IsNull(result.Banner)
    End Sub

    <DataTestMethod> <DataRow(0)> <DataRow(1)> <DataRow(2)> <DataRow(3)>
    Public Sub RepeatedInvalidShapesAndCellsAbortBeforeCore(which As Integer)
        Dim matrix = RepeatedMatrix(), names As String() = {"A", "B", "C"}
        Select Case which
            Case 0 : matrix = Nothing
            Case 1 : matrix = New Double(0, 0) {} : names = {"only"}
            Case 2 : matrix(0, 0) = Double.NaN
            Case 3 : matrix(0, 0) = Double.NegativeInfinity
        End Select
        Dim result = AnovaWorkflowPolicy.RunRepeated(matrix, names, New RepeatedAnovaWorkflowOptions())
        Assert.IsFalse(result.Success)
        Assert.AreEqual(0, result.Calls.Count)
        Assert.AreEqual(0, result.Tables.Count)
    End Sub

    <DataTestMethod> <DataRow(False)> <DataRow(True)>
    Public Sub CompleteBlockPolicyReusedForMissingRowsAndDuplicateTreatmentLabels(rejectMissing As Boolean)
        Dim source As New SpreadsheetRangeData With {.RowCount = 4, .ColumnCount = 3, .StartRow = 101, .StartColumn = 2}
        source.Rows.Add(New List(Of SpreadsheetCellValue) From {SpreadsheetCellValue.NumberCell(1), SpreadsheetCellValue.NumberCell(3), SpreadsheetCellValue.NumberCell(2)})
        source.Rows.Add(New List(Of SpreadsheetCellValue) From {SpreadsheetCellValue.NumberCell(4), SpreadsheetCellValue.BlankCell(), SpreadsheetCellValue.NumberCell(8)})
        source.Rows.Add(New List(Of SpreadsheetCellValue) From {SpreadsheetCellValue.NumberCell(4), SpreadsheetCellValue.NumberCell(7), SpreadsheetCellValue.NumberCell(2)})
        source.Rows.Add(New List(Of SpreadsheetCellValue) From {SpreadsheetCellValue.NumberCell(6), SpreadsheetCellValue.NumberCell(3), SpreadsheetCellValue.NumberCell(7)})
        Dim parsed = PairedBlockInputPolicy.ParseBlocks(source, New SpreadsheetBlockParseOptions With {.Missing = If(rejectMissing, SpreadsheetIncompleteObservationPolicy.Reject, SpreadsheetIncompleteObservationPolicy.Skip)},
            {101, 102, 103, 107}, {"same", "same", "0"})
        Assert.AreEqual(Not rejectMissing, parsed.Success)
        If rejectMissing Then
            Assert.IsTrue(parsed.Validation.HasErrors)
        Else
            Assert.IsTrue(parsed.Validation.HasWarnings)
            Dim result = AnovaWorkflowPolicy.RunRepeated(parsed.Data, New RepeatedAnovaWorkflowOptions())
            Assert.IsTrue(result.Success)
            CollectionAssert.AreEqual(New Integer() {101, 103, 107}, result.Blocks.Blocks.Select(Function(b) b(0).WorksheetRow).ToArray())
            CollectionAssert.AreEqual(New String() {"same", "same", "0"}, result.Blocks.Names.ToArray())
        End If
    End Sub

    <TestMethod>
    Public Sub NestedTypedInputKeepsOrderedLabelsAndCoordinates()
        Dim matrix = NestedMatrix(), snapshot As New SpreadsheetRangeData With {.RowCount = matrix.GetLength(0), .ColumnCount = 3, .StartRow = 201, .StartColumn = 1}
        For r = 0 To matrix.GetLength(0) - 1
            snapshot.Rows.Add(New List(Of SpreadsheetCellValue) From {SpreadsheetCellValue.TextCell(CStr(matrix(r, 0))), SpreadsheetCellValue.TextCell(CStr(matrix(r, 1))), SpreadsheetCellValue.NumberCell(CDbl(matrix(r, 2)))})
        Next
        Dim parsed = PairedBlockInputPolicy.ParseNested(snapshot, Nothing)
        Assert.IsTrue(parsed.Success)
        Dim report = AnovaWorkflowPolicy.RunNested(parsed.Data)
        Assert.IsTrue(report.Success)
        Assert.AreEqual("B", report.Nested.GroupLabels(0))
        Assert.AreEqual(201, report.Nested.Responses.Blocks(0)(0).WorksheetRow)
        Assert.AreEqual(10.0, report.Nested.Responses.Blocks(0)(0).Value)
        Assert.AreEqual("A", report.LegacyWorkingObjects(0, 0))
    End Sub
End Class
