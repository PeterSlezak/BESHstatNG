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
Imports BESHStatNG.ExcelCommon
Imports BESHStatNG.StatisticalProcessControl
Imports Microsoft.VisualStudio.TestTools.UnitTesting

Public NotInheritable Class Spc24Fixture
    Public Shared Function Audit(legacy As Spc24LegacyProjection) As SpcMultivariateAuditView
        Return New SpcMultivariateAuditView(legacy.tbChartTitle.Text.Trim(), legacy.tbValueAxisTitle.Text.Trim(), legacy.tbHorizontalAxisTitle.Text.Trim(), legacy.chkUseSequenceValuesForHorizontalAxis.Checked, legacy.spinChartWidth.Value, legacy.spinChartHeight.Value)
    End Function
    Public Shared Function Model(table As ResultTable) As ResultTableOutputModel
        Dim m = table.ToOutputModel(), a = CType(m.Values.Clone(), Object(,))
        For row = 0 To a.GetLength(0) - 1
            If a.GetLength(1) > 1 AndAlso TypeOf a(row, 0) Is String AndAlso CStr(a(row, 0)) = "Execution time (ms)" Then a(row, 1) = "VOLATILE:System.Double"
        Next
        Return New ResultTableOutputModel(a, m.HeaderTopRows, m.HeaderLeftColumns, m.FooterRows, m.PvalueColumns, m.TitleRows, m.IsResultTable, m.PvalueCells)
    End Function
    Public Shared Function Cell(value As Object) As XElement
        Dim typeName = If(value Is Nothing, "NULL", value.GetType().FullName)
        Return New XElement("cell", New XAttribute("type", typeName), If(TypeOf value Is Double, Spc23Fixture.Bits(CDbl(value)), Spc21Fixture.Text(value)))
    End Function
    Public Shared Function Tables(before As IList(Of ResultTable), after As IList(Of ResultTable), section As String) As XElement
        Assert.AreEqual(before.Count, after.Count, section)
        Dim node As New XElement("section", New XAttribute("name", section))
        For i = 0 To before.Count - 1
            'No tolerance and no timing normalization in the actual compatibility assertion.
            Assert.AreEqual(Spc23Fixture.Hash(before(i).ToOutputModel()), Spc23Fixture.Hash(after(i).ToOutputModel()), section & " table " & i.ToString())
            Dim m = Model(after(i)), values = m.Values
            Dim t As New XElement("table", New XAttribute("rows", values.GetLength(0)), New XAttribute("columns", values.GetLength(1)), New XAttribute("header_rows", m.HeaderTopRows), New XAttribute("header_left", m.HeaderLeftColumns), New XAttribute("footer_rows", m.FooterRows), New XAttribute("sha256", Spc23Fixture.Hash(m)), New XElement("metadata_sha256", Spc23Fixture.Hash(New Object() {m.PvalueColumns, m.TitleRows, m.IsResultTable, m.PvalueCells})))
            For row = 0 To values.GetLength(0) - 1
                Dim cells(values.GetLength(1) - 1) As Object
                For col = 0 To cells.Length - 1 : cells(col) = values(row, col) : Next
                t.Add(New XElement("row_hash", New XAttribute("index", row), Spc23Fixture.Hash(cells)))
                'Full small/model sections; large point tables have all row hashes,
                'all header/footer cells and representative first/last body cells.
                If values.GetLength(0) <= 35 OrElse row < 4 OrElse row = values.GetLength(0) - 1 Then t.Add(New XElement("row", New XAttribute("index", row), cells.Select(Function(v) Cell(v))))
            Next
            node.Add(t)
        Next
        Return node
    End Function
    Public Shared Function Compare(result As SpcMultivariateFitResult, viewCase As Integer, signalOnly As Boolean) As XElement
        Dim report As New SpcMultivariateReportProjection(result)
        Dim legacy As New Spc24LegacyProjection With {.SignalOnly = signalOnly, .DefaultCaption = Spc24LegacyProjection.ChartTypeText(result.Request.ChartType)}
        legacy.tbChartTitle.Text = If(viewCase = 0, "", If(viewCase = 1, "   ", " chart Ω "))
        legacy.tbValueAxisTitle.Text = If(viewCase = 0, "", If(viewCase = 1, "  ", " axis μ "))
        legacy.tbHorizontalAxisTitle.Text = " sequence Ω "
        legacy.chkUseSequenceValuesForHorizontalAxis.Checked = viewCase Mod 2 = 1
        legacy.spinChartWidth.Value = 761.25D : legacy.spinChartHeight.Value = 359.5D
        Dim node As New XElement("presentation", New XAttribute("view", viewCase), New XAttribute("signal_only", signalOnly), New XAttribute("caption", report.Family.Caption), New XAttribute("grouped", report.Family.HasSubgroups), New XAttribute("correlation", report.Family.UsesCorrelationScale), New XAttribute("rank", report.Family.Model.EffectiveDimension), New XAttribute("pseudo", report.Family.Model.UsedPseudoInverse), New XAttribute("retained", report.Family.Model.RetainedComponentCount), New XAttribute("model_source", report.Family.Model.Source), New XElement("variables", report.Family.InputVariables.Select(Function(n) New XElement("name", n))))
        CollectionAssert.AreEqual(result.Request.VariableNames, report.Family.InputVariables.ToArray())
        CollectionAssert.AreEqual(Spc24LegacyProjection.CollectWarnings(result).ToArray(), report.Warnings.Values.ToArray())
        node.Add(New XElement("warnings", report.Warnings.Values.Select(Function(w) New XElement("warning", w))))
        node.Add(Tables(legacy.BuildSummaryTables(result), report.BuildSummaryTables(), "Summary"))
        node.Add(Tables(legacy.BuildModelTables(result), report.BuildModelTables(), "Model"))
        node.Add(Tables(New List(Of ResultTable) From {legacy.BuildChartDataTable(result)}, New List(Of ResultTable) From {report.BuildChartDataTable()}, "ChartData"))
        node.Add(Tables(New List(Of ResultTable) From {legacy.BuildSignalsTable(result)}, New List(Of ResultTable) From {report.BuildSignalsTable()}, "Signals"))
        node.Add(Tables(legacy.BuildDiagnosticTables(result), report.BuildDiagnosticTables(signalOnly), "Diagnostics"))
        node.Add(Tables(legacy.BuildAuditTables(result), report.BuildAuditTables(Audit(legacy)), "Audit"))
        Dim intent As New SpcMultivariateChartIntent(legacy.tbChartTitle.Text, legacy.tbValueAxisTitle.Text, legacy.DefaultCaption)
        Dim oldWrapper = legacy.BuildChartRenderingResult(result), payload = SpcMultivariateChartProjectionPolicy.Prepare(report, intent)
        Assert.AreEqual(Spc23Fixture.Hash(oldWrapper), Spc23Fixture.Hash(payload.RenderingResult), "Complete wrapper types/values including elapsed time")
        For Each useSequence In {False, True}
            Dim expected = SpcUnivariateChartProjectionPolicy.Project(oldWrapper, useSequence), actual = payload.ProjectSeries(useSequence)
            Assert.AreEqual(Spc23Fixture.Hash(expected), Spc23Fixture.Hash(actual))
            'The accepted source-pinned renderer oracle independently checks all ten
            'channels, typed categories, label eligibility and stage fractions.
            node.Add(Spc22Fixture.Compare(payload.RenderingResult, useSequence))
        Next
        node.Add(New XElement("core_model_sha256", Spc23Fixture.Hash(result.Model)), New XElement("core_diagnostics_sha256", Spc23Fixture.Hash(result.Diagnostics)), New XElement("wrapper_sha256", Spc23Fixture.Hash(New Object() {payload.RenderingResult.Request, payload.RenderingResult.Panels, payload.RenderingResult.Warnings})))
        Return node
    End Function
    Public Shared Function Input(id As Integer) As Spc23Fixture.Input
        Dim inputValue = Spc23Fixture.InputFor(id)
        inputValue.Context = Spc23LegacyPreparation.Map(inputValue.FirstRow, inputValue.FirstRow + inputValue.Data.GetLength(0) - 1, inputValue.Grouped, inputValue.Ids)
        Spc23Fixture.FillGrids(inputValue, id \ 6)
        Return inputValue
    End Function
    Public Shared Sub Emit(root As XElement)
        Console.WriteLine("04A24_XML|" & Convert.ToBase64String(Encoding.UTF8.GetBytes(root.ToString(SaveOptions.DisableFormatting))))
    End Sub
    Public Shared Function RankInput(id As Integer) As Spc23Fixture.Input
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
            Return input
    End Function
End Class
