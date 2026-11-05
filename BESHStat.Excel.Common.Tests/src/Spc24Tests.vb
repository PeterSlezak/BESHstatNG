Option Explicit On
Option Strict On
Option Infer On
Imports System
Imports System.Collections.Generic
Imports System.Globalization
Imports System.Linq
Imports System.Xml.Linq
Imports BESHStatNG
Imports BESHStatNG.ExcelCommon
Imports BESHStatNG.StatisticalProcessControl
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass>
Public Class Spc24Tests
    Public Shared Iterator Function Profiles() As IEnumerable(Of Object())
        For Each culture In {"", "sk-SK"}
            For id = 0 To 359 : Yield New Object() {id, culture} : Next
        Next
    End Function
    <DataTestMethod, DynamicData(NameOf(Profiles), DynamicDataSourceType.Method)>
    Public Sub SameCorePostFitTablesWarningsWrapperAndChannelsMatchEntry(id As Integer, culture As String)
        Dim saved = CultureInfo.CurrentCulture
        Try
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(culture)
            Dim input = Spc23Fixture.InputFor(id), request As SpcMultivariateRequest = Nothing, result As SpcMultivariateFitResult = Nothing
            Dim failure = Spc23Tests.Failure(Sub()
                                                input.Context = Spc23LegacyPreparation.Map(input.FirstRow, input.FirstRow + input.Data.GetLength(0) - 1, input.Grouped, input.Ids)
                                                Spc23Fixture.FillGrids(input, id \ 6)
                                                request = Spc23Fixture.Prepare(input, True)
                                                result = SpcMultivariate.Fit(request)
                                            End Sub)
            CheckAndEmit(id.ToString(CultureInfo.InvariantCulture), culture, input, request, result, failure, (id \ 6) Mod 3, (id \ 6) Mod 2 = 1)
        Finally
            CultureInfo.CurrentCulture = saved
        End Try
    End Sub
    Public Shared Iterator Function RankProfiles() As IEnumerable(Of Object())
        For Each culture In {"", "sk-SK"}
            For id = 0 To 35 : Yield New Object() {id, culture} : Next
        Next
    End Function
    <DataTestMethod, DynamicData(NameOf(RankProfiles), DynamicDataSourceType.Method)>
    Public Sub SingularHistoricalTiedAndTwoVariableReportsMatchEntry(id As Integer, culture As String)
        Dim saved = CultureInfo.CurrentCulture
        Try
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(culture)
            Dim input = Spc24Fixture.RankInput(id), request As SpcMultivariateRequest = Nothing, result As SpcMultivariateFitResult = Nothing
            Dim failure = Spc23Tests.Failure(Sub()
                                                request = Spc23Fixture.Prepare(input, True)
                                                result = SpcMultivariate.Fit(request)
                                            End Sub)
            CheckAndEmit("rank-" & id.ToString(CultureInfo.InvariantCulture), culture, input, request, result, failure, (id \ 6) Mod 3, (id \ 6) Mod 2 = 1)
        Finally
            CultureInfo.CurrentCulture = saved
        End Try
    End Sub
    Public Shared Sub CheckAndEmit(id As String, culture As String, input As Spc23Fixture.Input, request As SpcMultivariateRequest, result As SpcMultivariateFitResult, failure As Exception, view As Integer, signalOnly As Boolean)
        Dim tfm = If(Environment.Version.Major = 4, "net48", "net10.0"), expected As String() = Nothing
        Dim negative = Spc24ExpectedRejections.Cases().TryGetValue(tfm & "|" & id & "|" & culture, expected)
        Dim node As New XElement("comparison", New XAttribute("id", id), New XAttribute("chart", input.Chart), New XAttribute("culture", culture), New XAttribute("runtime", Environment.Version.ToString()), New XAttribute("status", "NOT_GOLDEN"), New XAttribute("purpose", "SAME_CORE_COMPATIBILITY_ONLY"), New XElement("dataset_sha256", Spc23Fixture.Hash(input.Data)), New XElement("fixture_source_sha256", Spc23Data.DatasetSha), New XElement("requested_options_sha256", Spc23Fixture.Hash(input.Options)))
        node.Add(New XElement("requested_options", input.Options.GetType().GetFields().OrderBy(Function(f) f.Name, StringComparer.Ordinal).Select(Function(f) New XElement("option", New XAttribute("name", f.Name), New XAttribute("type", f.FieldType.FullName), Spc21Fixture.Text(f.GetValue(input.Options))))))
        If negative Then
            Assert.IsNotNull(failure, "A prior rejection must not become success")
            Assert.AreEqual(expected(0), failure.GetType().FullName)
            'Accepted XML payload text normalized CRLF to LF. Rehydrate the
            'Framework message separator, then compare the actual raw message.
            Dim expectedMessage = If(tfm = "net48", expected(1).Replace(vbLf, vbCrLf), expected(1))
            Assert.AreEqual(expectedMessage, failure.Message)
            node.Add(New XAttribute("outcome", "PRESERVED_REJECTION"), New XElement("exception", New XAttribute("type", failure.GetType().FullName), New XAttribute("raw_message_sha256", Spc23Fixture.Hash(failure.Message)), failure.Message))
        Else
            Assert.IsNull(failure, If(failure Is Nothing, "", failure.ToString()))
            Assert.IsNotNull(result)
            node.Add(New XAttribute("outcome", "POSITIVE_PARITY"), New XElement("request_sha256", Spc23Fixture.Hash(request)), Spc24Fixture.Compare(result, view, signalOnly))
        End If
        Spc24Fixture.Emit(node)
    End Sub

    <DataTestMethod><DataRow(0)><DataRow(1)><DataRow(2)><DataRow(3)><DataRow(4)><DataRow(5)>
    Public Sub EveryFamilyRetainsAllAndSignalOnlyOptionalDiagnosticSections(id As Integer)
        Dim input = Spc24Fixture.Input(id), request = Spc23Fixture.Prepare(input, True), result = SpcMultivariate.Fit(request)
        For Each signalOnly In {False, True}
            Spc24Fixture.Compare(result, 2, signalOnly)
        Next
    End Sub

    <TestMethod>
    Public Sub WarningUnionRetainsFirstSeenOrdinalCaseWhitespaceAndEmptySemantics()
        Dim top = {"A", "", " A ", "A", "a"}, panel = New String() {"B", "A", "a", "b", " "}
        Dim warning = SpcMultivariateWarningProjection.FromWarnings(top, {panel, New String() {"C", "B"}})
        top(0) = "changed" : panel(0) = "changed"
        CollectionAssert.AreEqual(New String() {"A", " A ", "a", "B", "b", " ", "C"}, warning.Values.ToArray())
        Assert.ThrowsException(Of NotSupportedException)(Sub() DirectCast(warning.Values, IList(Of String))(0) = "changed")
        Assert.AreEqual(0, SpcMultivariateWarningProjection.FromWarnings(New String() {}, New IEnumerable(Of String)() {}).Values.Count)
    End Sub

    <TestMethod>
    Public Sub AllSevenOutputToggleCombinationsKeepDefinitionOrderAndNoImplicitSelection()
        Dim names = {"Multivariate Summary", "Model Details", "Multivariate Chart", "Chart Data", "Signals", "Diagnostics", "Settings and Audit"}
        Dim definitions = SpcMultivariateSections.OrderedDefinitions()
        CollectionAssert.AreEqual(names, definitions.Select(Function(d) d.SheetName).ToArray())
        For mask = 0 To 127
            Dim selectionMask = mask
            Dim selected As New List(Of String)
            For i = 0 To 6
                If (mask And (1 << i)) <> 0 Then selected.Add(SpcMultivariateSections.Definition(CType(i, SpcMultivariateReportSection)).SheetName)
            Next
            CollectionAssert.AreEqual(names.Where(Function(n, i) (selectionMask And (1 << i)) <> 0).ToArray(), selected.ToArray())
        Next
        Assert.ThrowsException(Of NotSupportedException)(Sub() DirectCast(definitions, IList(Of SpcMultivariateSectionDefinition))(0) = Nothing)
    End Sub

    <DataTestMethod><DataRow(0)><DataRow(1)><DataRow(2)><DataRow(3)><DataRow(4)><DataRow(5)>
    Public Sub ReentrantFreshTablesAndImmutableFamilyDoNotRetainCallerMutations(id As Integer)
        Dim input = Spc24Fixture.Input(id), request = Spc23Fixture.Prepare(input, True), result = SpcMultivariate.Fit(request)
        Dim report As New SpcMultivariateReportProjection(result)
        Dim original = Spc23Fixture.Hash(report.BuildModelTables().Select(Function(t) Spc24Fixture.Model(t)).ToArray())
        Dim table = report.BuildModelTables()(0), names = result.Request.VariableNames, covariance = result.Model.ProcessCovariance
        names(0) = "changed" : covariance(0, 0) = 999 : table.AddTitle("changed")
        input.Data(0, 0) = 999
        Assert.AreEqual(original, Spc23Fixture.Hash(report.BuildModelTables().Select(Function(t) Spc24Fixture.Model(t)).ToArray()))
        Assert.AreEqual("Temperature_C", report.Family.InputVariables(0))
        Assert.AreSame(report.Context, report.Context) : Assert.AreSame(report.Warnings, report.Warnings)
        Assert.ThrowsException(Of NotSupportedException)(Sub() DirectCast(report.Family.InputVariables, IList(Of String))(0) = "changed")
    End Sub

    <TestMethod>
    Public Sub StandaloneNullGuardsDoNotInvokeCoreFitOrNativeObjects()
        Assert.ThrowsException(Of ArgumentNullException)(Sub() NewReport(Nothing))
        Assert.ThrowsException(Of ArgumentNullException)(Sub() SpcMultivariateChartProjectionPolicy.Prepare(Nothing, New SpcMultivariateChartIntent("", "", "")))
    End Sub

    <DataTestMethod><DataRow(0)><DataRow(1)><DataRow(2)><DataRow(3)><DataRow(4)><DataRow(5)><DataRow(6)>
    Public Sub PublishedMonitoringAndExclusionsRetainEverySelectedReport(id As Integer)
        Dim input = Spc23Fixture.InputFor(id)
        input.Options = New Spc23LegacyPreparation.RawOptions With {.FixedComponents = True, .Components = 2, .AllowPseudo = input.Chart <> SpcChartType.GeneralizedVariance}
        input.Mean = Nothing : input.Covariance = Nothing : input.FirstRow = 2
        Dim n = input.Data.GetLength(0), grouped = input.Grouped
        input.Context = Spc23LegacyPreparation.Map(2, 151, grouped, input.Ids)
        input.Labels = Enumerable.Range(0, n).Select(Function(i) If(grouped, "Subgroup " & (i \ 5 + 1).ToString("00", CultureInfo.InvariantCulture), "Obs-" & (i + 1).ToString("000", CultureInfo.InvariantCulture))).ToArray()
        input.Sequences = Enumerable.Range(0, n).Select(Function(i) CDbl(If(grouped, i \ 5 + 1, i + 1))).ToArray()
        Dim split = If(grouped, 20, 100)
        input.Legacy.AddStageRow("Baseline", 1, split, SpcPhase.PhaseI)
        input.Legacy.AddStageRow("Monitoring", split + 1, input.Context.PointCount, SpcPhase.PhaseII)
        For point = If(grouped, 8, 36) To If(grouped, 8, 40)
            input.Legacy.dgvExclusions.Rows.Add(New Spc23LegacyPreparation.DataGridViewRow(point, "Parameter estimation and signal evaluation", "Calibration check: exclude from estimation and signal evaluation"))
        Next
        Dim request = Spc23Fixture.Prepare(input, True), result = SpcMultivariate.Fit(request)
        Assert.AreEqual(95, result.Model.BaselineObservationCount)
        CheckAndEmit("published-" & id.ToString(CultureInfo.InvariantCulture), CultureInfo.CurrentCulture.Name, input, request, result, Nothing, 2, id Mod 2 = 1)
    End Sub

    <TestMethod>
    Public Sub SyntheticSparseMultiPanelWrapperRetainsFirstRepresentativeAndEmptyDiagnostics()
        Dim input = Spc24Fixture.Input(0), original = SpcMultivariate.Fit(Spc23Fixture.Prepare(input, True))
        Dim kind = original.Panels(0).PanelType
        Dim first = New SpcPanelResult(kind, "first", {
            Spc22Fixture.Point(0, -0.0R, "A", rows:={101, 118}, sequence:=3),
            Spc22Fixture.Point(2, Double.NaN, "a", SpcPhase.PhaseII, rows:={105}),
            Spc22Fixture.Point(5, 8, "B", rows:={131}, excluded:=True)})
        Assert.ThrowsException(Of ArgumentOutOfRangeException)(Sub() Spc22Fixture.Point(1, Double.PositiveInfinity))
        Dim second = New SpcPanelResult(SpcPanelType.PcaQ, "second", {Spc22Fixture.Point(1, Double.NaN, "different", rows:={111}), Spc22Fixture.Point(2, 4, "other", sequence:=-9, rows:={222})})
        'Only a synthetic API characterization: no additional production panel is enabled.
        Dim constructor = GetType(SpcMultivariateFitResult).GetConstructors(Reflection.BindingFlags.NonPublic Or Reflection.BindingFlags.Instance).Single()
        Dim result = DirectCast(constructor.Invoke(New Object() {original.Request, original.Model, New SpcPanelResult() {first, second}, New SpcMultivariatePointDiagnostic() {}, New String() {"one", "one", "One"}, 0.0R}), SpcMultivariateFitResult)
        Dim report As New SpcMultivariateReportProjection(result), payload = SpcMultivariateChartProjectionPolicy.Prepare(report, New SpcMultivariateChartIntent("", "", "Hotelling T-squared"))
        CollectionAssert.AreEqual(New Integer() {0, 1, 2, 5}, payload.ProjectSeries(True).Axis.Select(Function(a) a.Point.PointIndex).ToArray())
        CollectionAssert.AreEqual(New Integer() {105}, payload.ProjectSeries(True).Axis(2).Point.SourceRowIndices.ToArray())
        Spc24Fixture.Emit(New XElement("comparison", New XAttribute("id", "synthetic-sparse"), New XAttribute("outcome", "SYNTHETIC_API_PRESENTATION"), New XAttribute("status", "NOT_GOLDEN"), Spc24Fixture.Compare(result, 2, True)))
    End Sub

    <TestMethod>
    Public Sub ThousandsOfPointsKeepPhysicalRowsAndRepeatedProjectionIndependent()
        Const n As Integer = 3000
        Dim data(n - 1, 1) As Double, rows(n - 1) As Integer
        For i = 0 To n - 1
            data(i, 0) = Math.Sin(i * 0.7R) : data(i, 1) = Math.Cos(i * 0.31R) : rows(i) = 101 + i * 3
        Next
        Dim request As New SpcMultivariateRequest(SpcChartType.Mewma, data, sourceRowIndices:=rows)
        Dim result = SpcMultivariate.Fit(request), report As New SpcMultivariateReportProjection(result)
        rows(0) = 999 : data(0, 0) = 999
        Dim payload = SpcMultivariateChartProjectionPolicy.Prepare(report, New SpcMultivariateChartIntent("", "", "MEWMA"))
        Dim first = payload.ProjectSeries(False), second = payload.ProjectSeries(True)
        Assert.AreEqual(n, first.Axis.Count) : Assert.AreEqual(101, first.Axis(0).Point.SourceRowIndices(0))
        Assert.AreEqual(101 + (n - 1) * 3, second.Axis(n - 1).Point.SourceRowIndices(0))
        Assert.AreEqual(Spc23Fixture.Hash(first), Spc23Fixture.Hash(payload.ProjectSeries(False)))
        Dim legacy As New Spc24LegacyProjection
        Assert.AreEqual(Spc23Fixture.Hash(legacy.BuildChartDataTable(result).ToOutputModel()), Spc23Fixture.Hash(report.BuildChartDataTable().ToOutputModel()))
    End Sub
    Private Shared Sub NewReport(result As SpcMultivariateFitResult)
        Dim report As New SpcMultivariateReportProjection(result)
    End Sub
End Class
