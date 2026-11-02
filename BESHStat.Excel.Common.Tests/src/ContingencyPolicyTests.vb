Option Explicit On
Option Strict On
Imports System
Imports System.Collections.Generic
Imports System.Globalization
Imports System.Linq
Imports System.Threading
Imports BESHStatNG
Imports BESHStatNG.ExcelCommon
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass>
Public Class ContingencyPolicyTests
    Friend Shared Sub SameTables(before As List(Of ResultTable), after As ContingencyReportResult)
        Assert.IsTrue(after.Success, String.Join(";", after.Validation.Issues.Select(Function(i) i.Code & ":" & i.Message)))
        Assert.AreEqual(before.Count, after.Tables.Count)
        For i = 0 To before.Count - 1
            Dim a = before(i).ToOutputModel(), b = after.Tables(i).ToCoreTable().ToOutputModel()
            CollectionAssert.AreEqual(a.Values.Cast(Of Object)().ToArray(), b.Values.Cast(Of Object)().ToArray(), "Table " & i.ToString())
            Assert.AreEqual(a.RowCount, b.RowCount)
            Assert.AreEqual(a.ColumnCount, b.ColumnCount)
            Assert.AreEqual(a.HeaderTopRows, b.HeaderTopRows)
            Assert.AreEqual(a.HeaderLeftColumns, b.HeaderLeftColumns)
            CollectionAssert.AreEqual(a.PvalueColumns.ToArray(), b.PvalueColumns.ToArray())
            CollectionAssert.AreEqual(a.PvalueCells.ToArray(), b.PvalueCells.ToArray())
        Next
    End Sub
    Friend Shared Function Counts(values As Double(,)) As ContingencyCountSelection
        Dim result = ContingencyInputPolicy.Parse(ContingencyInputPolicy.NumericSnapshot(values), New SpreadsheetContingencyCountParseOptions With {.HasColumnHeaders = False, .HasRowLabels = False})
        Assert.IsTrue(result.Success)
        Return result.Data
    End Function

    <DataTestMethod> <DataRow("")> <DataRow("sk-SK")>
    Public Sub TwoByTwo_AllSelected_ExactLegacySemanticSnapshot(culture As String)
        Dim saved = Thread.CurrentThread.CurrentCulture
        Try
            Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo(culture)
            Dim input = ContingencyInputPolicy.FromDialog(12, 5, 7, 19).Data
            For Each alpha In {0.05, 0.1, 0.075}
                Dim options As New TwoByTwoReportOptions With {.Alpha = alpha, .Fisher = True, .ChiSquare = True, .Association = True, .Liddell = True, .OddsRatio = True, .RiskRatio = True}
                Dim report = ContingencyReportPolicy.TwoByTwo(input, options)
                SameTables(ContingencyLegacyCharacterization.TwoByTwo(input.CoreCounts, options), report)
                Assert.AreEqual(7, report.Tables.Count)
                Dim odds = contingencytable.OddsRatio(input.CoreCounts, alpha), risk = contingencytable.RiskRatio(input.CoreCounts, alpha)
                Assert.AreEqual(odds.Item1.LowerLimit, report.Metrics.Single(Function(m) m.Name = "or.woolf.lower").Value)
                Assert.AreEqual(risk.UpperLimit, report.Metrics.Single(Function(m) m.Name = "rr.upper").Value)
                Assert.IsTrue(Math.Abs(odds.Item1.Estimate - contingencytable.OddsRatio(New Integer(,) {{7,19},{12,5}}, alpha).Item1.Estimate) > 1)
                Assert.IsTrue(Math.Abs(risk.Estimate - contingencytable.RiskRatio(New Integer(,) {{12,7},{5,19}}, alpha).Estimate) > 0.01)
            Next
        Finally
            Thread.CurrentThread.CurrentCulture = saved
        End Try
    End Sub

    <DataTestMethod> <DataRow(0)> <DataRow(1)> <DataRow(2)> <DataRow(3)> <DataRow(4)> <DataRow(5)> <DataRow(6)>
    Public Sub TwoByTwo_IndividualOptions_AndAssociationImpliesChi2(optionIndex As Integer)
        Dim options As New TwoByTwoReportOptions With {.Fisher = optionIndex = 0, .ChiSquare = optionIndex = 1, .Association = optionIndex = 2,
            .Liddell = optionIndex = 3, .OddsRatio = optionIndex = 4, .RiskRatio = optionIndex = 5}
        Dim input = ContingencyInputPolicy.FromDialog(12, 5, 7, 19).Data
        Dim report = ContingencyReportPolicy.TwoByTwo(input, options)
        If optionIndex = 6 Then
            Assert.IsFalse(report.Success) : Assert.AreEqual(0, report.Tables.Count)
        Else
            SameTables(ContingencyLegacyCharacterization.TwoByTwo(input.CoreCounts, options), report)
        End If
    End Sub

    <DataTestMethod> <DataRow(1000, False)> <DataRow(1001, False)> <DataRow(1001, True)>
    Public Sub FisherGuardAndNoPartialResult(n As Integer, chi As Boolean)
        Dim input = ContingencyInputPolicy.FromDialog(n - 3, 1, 1, 1).Data
        Dim result = ContingencyReportPolicy.TwoByTwo(input, New TwoByTwoReportOptions With {.Fisher = True, .ChiSquare = chi})
        Assert.AreEqual(n = 1000 OrElse chi, result.Success)
        Assert.AreEqual(n > 1000, result.Validation.Issues.Any(Function(i) i.Code = "contingency.fisher.too-large"))
        If Not result.Success Then Assert.AreEqual(0, result.Tables.Count)
    End Sub

    <DataTestMethod> <DataRow("fraction")> <DataRow("negative")> <DataRow("nan")> <DataRow("infinity")> <DataRow("blank")> <DataRow("error")> <DataRow("boolean")> <DataRow("text")> <DataRow("int32")> <DataRow("precision")>
    Public Sub InvalidCountHasCoordinatesAndNoCoreInput(kind As String)
        Dim f = ContingencyInputPolicy.NumericSnapshot(New Double(,) {{12,5},{7,19}})
        f.StartRow = 20 : f.StartColumn = 3
        Select Case kind
            Case "fraction" : f.Rows(0)(1) = SpreadsheetCellValue.NumberCell(1.5)
            Case "negative" : f.Rows(0)(1) = SpreadsheetCellValue.NumberCell(-1)
            Case "nan" : f.Rows(0)(1) = SpreadsheetCellValue.NumberCell(Double.NaN)
            Case "infinity" : f.Rows(0)(1) = SpreadsheetCellValue.NumberCell(Double.PositiveInfinity)
            Case "blank" : f.Rows(0)(1) = SpreadsheetCellValue.BlankCell()
            Case "error" : f.Rows(0)(1) = SpreadsheetCellValue.ErrorCell("#NUM!", "2036")
            Case "boolean" : f.Rows(0)(1) = SpreadsheetCellValue.BooleanCell(True)
            Case "text" : f.Rows(0)(1) = SpreadsheetCellValue.TextCell("1,5")
            Case "int32" : f.Rows(0)(1) = SpreadsheetCellValue.NumberCell(CDbl(Integer.MaxValue) + 1)
            Case "precision" : f.Rows(0)(1) = SpreadsheetCellValue.NumberCell(9007199254740992.0)
        End Select
        Dim result = ContingencyInputPolicy.Parse(f, New SpreadsheetContingencyCountParseOptions With {.HasColumnHeaders = False, .HasRowLabels = False}, True, {40, 80})
        Assert.IsFalse(result.Success)
        Assert.IsNull(result.Data)
        Assert.IsTrue(result.Validation.Issues.Any(Function(i) i.SelectedRow = 1 AndAlso i.SelectedColumn = 2 AndAlso i.WorksheetRow = 40 AndAlso i.WorksheetColumn = 4))
    End Sub

    <TestMethod>
    Public Sub ExplicitLabels_OrderTypesCaseAndHeadersRemainDistinct()
        Dim f As New SpreadsheetRangeData With {.RowCount = 3, .ColumnCount = 4, .StartRow = 10, .StartColumn = 7}
        f.Rows.Add(New List(Of SpreadsheetCellValue) From {Nothing, SpreadsheetCellValue.TextCell("0"), SpreadsheetCellValue.NumberCell(0), SpreadsheetCellValue.TextCell("z")})
        f.Rows.Add(New List(Of SpreadsheetCellValue) From {SpreadsheetCellValue.TextCell("b"), SpreadsheetCellValue.NumberCell(12), SpreadsheetCellValue.NumberCell(5), SpreadsheetCellValue.NumberCell(1)})
        f.Rows.Add(New List(Of SpreadsheetCellValue) From {SpreadsheetCellValue.TextCell("B"), SpreadsheetCellValue.NumberCell(7), SpreadsheetCellValue.NumberCell(19), SpreadsheetCellValue.NumberCell(2)})
        Dim parsed = ContingencyInputPolicy.Parse(f, New SpreadsheetContingencyCountParseOptions)
        Assert.IsTrue(parsed.Success)
        CollectionAssert.AreEqual(New String() {"b", "B"}, parsed.Data.Table.RowLevels.Select(Function(l) l.DisplayText).ToArray())
        Assert.AreNotEqual(parsed.Data.Table.ColumnLevels(0).Key, parsed.Data.Table.ColumnLevels(1).Key)
        Assert.AreEqual(11, parsed.Data.Sources(0).WorksheetRow)
        Assert.AreEqual(8, parsed.Data.Sources(0).WorksheetColumn)
        Assert.IsFalse(ContingencyInputPolicy.Parse(f, New SpreadsheetContingencyCountParseOptions With {.IgnoreTextCase = True}).Success)
        Assert.IsFalse(ContingencyInputPolicy.Parse(f, New SpreadsheetContingencyCountParseOptions With {.HasColumnHeaders = False}).Success)
    End Sub

    <DataTestMethod> <DataRow(0)> <DataRow(1)> <DataRow(2)>
    Public Sub RxC_LegacyOrdinalNominalAndTrendSnapshots(shape As Integer)
        Dim values As Double(,) = If(shape = 0, New Double(,) {{4,5,6,7},{1,2,4,8},{9,6,4,2}}, If(shape = 1, New Double(,) {{12,5,7},{3,9,19}}, New Double(,) {{12,3},{5,9},{7,19}}))
        Dim input = Counts(values)
        For Each alpha In {0.05, 0.1}
            Dim options As New RxCReportOptions With {.Alpha = alpha, .Nominal = True, .Ordinal = True, .Trend = shape <> 0}
            Dim report = ContingencyReportPolicy.RxC(input, options)
            SameTables(ContingencyLegacyCharacterization.RxC(input.CoreCounts, options), report)
            Dim ordinal = contingencytable.cTableORDINALassoc(input.CoreCounts, alpha)
            Assert.AreEqual(ordinal.Item4.TestStatistics1, CDbl(report.Tables(3).Body(12,1)), 0.000000000001)
            CollectionAssert.AreEqual(New Integer() {4,8,12,16}, report.Tables(3).ToCoreTable().ToOutputModel().PvalueCells.Select(Function(p) p.BodyRow).ToArray())
        Next
    End Sub

    <TestMethod>
    Public Sub RxC_ExactRealSuccessAndInjectedFailurePreserveFallback()
        Dim input = Counts(New Double(,) {{1,2,3},{3,2,1}})
        Dim options As New RxCReportOptions With {.Exact = True}
        SameTables(ContingencyLegacyCharacterization.RxC(input.CoreCounts, options), ContingencyReportPolicy.RxC(input, options))
        Dim failed = ContingencyReportPolicy.RxC(input, options, Function(tab) ThrowExact())
        Assert.IsTrue(failed.Success)
        Assert.AreEqual(".error. not possible to compute", failed.Tables(2).Body(0,1))
        Assert.AreEqual(1, failed.Tables(2).ToCoreTable().ToOutputModel().PvalueCells(0).BodyRow)
    End Sub
    Private Shared Function ThrowExact() As Double
        Throw New InvalidOperationException("Injected exact-engine failure; not a native test")
    End Function

    <TestMethod>
    Public Sub InvalidShapesMissingTotalsAndCopiesAreSafe()
        Assert.IsFalse(ContingencyInputPolicy.Parse(Nothing, Nothing).Success)
        Assert.IsFalse(ContingencyInputPolicy.FromDialog(0,0,0,0).Success)
        Assert.IsFalse(ContingencyInputPolicy.Parse(ContingencyInputPolicy.NumericSnapshot(New Double(,) {{1,2,3},{4,5,6}}), New SpreadsheetContingencyCountParseOptions With {.HasColumnHeaders=False,.HasRowLabels=False}, True).Success)
        Assert.IsFalse(ContingencyInputPolicy.FromDialog(Integer.MaxValue, 1, 0, 0).Success)
        Dim input = ContingencyInputPolicy.FromDialog(12,5,7,19).Data
        Dim matrix = input.CoreCounts : matrix(0,0) = 999
        Dim copy = input.Table.Counts : copy(0)(0) = 999
        Assert.AreEqual(12, input.CoreCounts(0,0))
        Dim report = ContingencyReportPolicy.TwoByTwo(input, New TwoByTwoReportOptions With {.ChiSquare=True})
        Dim body = report.Tables(0).Body : body(0,0) = 999
        Dim artifact = report.Tables(0).ToArtifact(0) : artifact.Model.Values(0,0) = 999
        Assert.AreEqual(12, report.Tables(0).Body(0,0))
        Assert.IsFalse(ContingencyReportPolicy.TwoByTwo(Nothing, New TwoByTwoReportOptions With {.ChiSquare=True}).Success)
        Assert.IsFalse(ContingencyReportPolicy.TwoByTwo(input, New TwoByTwoReportOptions With {.ChiSquare=True,.Alpha=Double.NaN}).Success)
        Assert.IsFalse(ContingencyReportPolicy.TwoByTwo(input, Nothing).Success)
        Assert.IsTrue(ContingencyReportPolicy.RxC(input, New RxCReportOptions With {.Trend=True}).Validation.HasWarnings)
        Dim oneRow=ContingencyInputPolicy.Parse(ContingencyInputPolicy.NumericSnapshot(New Double(,) {{1,2}}),New SpreadsheetContingencyCountParseOptions With {.HasColumnHeaders=False,.HasRowLabels=False,.MinimumRows=1}).Data
        Dim invalidReport=ContingencyReportPolicy.RxC(oneRow,New RxCReportOptions)
        Assert.IsFalse(invalidReport.Success)
        Assert.AreEqual("contingency.shape",invalidReport.Validation.Issues(0).Code)
    End Sub

    <DataTestMethod> <DataRow(0,5,7,19)> <DataRow(12,0,7,19)> <DataRow(12,5,0,19)>
    Public Sub ZeroCellsPreserveCoreUndefinedCIStringsWithoutParsing(a As Integer,b As Integer,c As Integer,d As Integer)
        Dim input=ContingencyInputPolicy.FromDialog(a,b,c,d).Data
        Dim options As New TwoByTwoReportOptions With {.OddsRatio=True,.RiskRatio=True}
        SameTables(ContingencyLegacyCharacterization.TwoByTwo(input.CoreCounts,options),ContingencyReportPolicy.TwoByTwo(input,options))
    End Sub

    <TestMethod>
    Public Sub SourceRowMismatchAndInt32EdgesAreExplicit()
        Dim f=ContingencyInputPolicy.NumericSnapshot(New Double(,) {{12,5},{7,19}})
        f.WorkbookId="book-identity" : f.WorksheetId="sheet-identity" : f.WorksheetName="Study" : f.Address="opaque-selected-area"
        Dim options As New SpreadsheetContingencyCountParseOptions With {.HasColumnHeaders=False,.HasRowLabels=False}
        Assert.IsFalse(ContingencyInputPolicy.Parse(f,options,True,{20,20}).Success)
        Assert.IsFalse(ContingencyInputPolicy.Parse(f,options,True,{20}).Success)
        Dim selected=ContingencyInputPolicy.Parse(f,options).Data
        f.WorkbookId="mutated"
        Assert.AreEqual("book-identity",selected.WorkbookId)
        Assert.AreEqual("sheet-identity",selected.WorksheetId)
        Assert.AreEqual("Study",selected.WorksheetName)
        Assert.AreEqual("opaque-selected-area",selected.SourceAddress)
        Assert.IsTrue(ContingencyInputPolicy.FromDialog(Integer.MaxValue,0,0,0).Success)
        Dim input=ContingencyInputPolicy.FromDialog(1000000,1,1,1000).Data
        Assert.IsTrue(ContingencyReportPolicy.TwoByTwo(input,New TwoByTwoReportOptions With {.ChiSquare=True}).Success)
    End Sub
End Class
