Option Explicit On
Option Strict On
Imports System
Imports System.Globalization
Imports System.Linq
Imports System.Threading
Imports BESHStatNG.ExcelCommon
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass>
Public Class ContingencyAdapterTests
    Private Shared Function Imported(raw As Object(,), names As String()) As DataObj
        Dim data As New DataObj
        data.DataImportRawMatrix(raw,names,firstSourceRow:=20)
        Return data
    End Function
    Private Shared Function Options() As TwoByTwoReportOptions
        Return New TwoByTwoReportOptions With {.Fisher=True,.ChiSquare=True,.Association=True,.Liddell=True,.OddsRatio=True,.RiskRatio=True}
    End Function

    <TestMethod>
    Public Sub DialogAndRealWorksheetProducerUseIdenticalCountOrientationAndReport()
        Dim data=Imported(New Object(,) {{12,5},{7,19}},{"first","second"})
        Dim parsed=WindowsContingencyInputAdapter.ParseWorksheet(data,New SpreadsheetContingencyCountParseOptions With {.HasColumnHeaders=False,.HasRowLabels=False},True)
        Assert.IsTrue(parsed.Success)
        CollectionAssert.AreEqual(New Integer() {12,5,7,19},parsed.Data.CoreCounts.Cast(Of Integer)().ToArray())
        CollectionAssert.AreEqual(New Integer() {20,20,21,21},parsed.Data.Sources.Select(Function(s) s.WorksheetRow).ToArray())
        Assert.IsTrue(parsed.Data.Sources.All(Function(s) s.WorksheetColumn=0),"Do not invent physical columns from importer defaults")
        Dim fromSheet=ContingencyReportPolicy.TwoByTwo(parsed.Data,Options()), fromDialog=WindowsContingencyInputAdapter.AnalyzeDialog(12,5,7,19,Options())
        Assert.IsTrue(fromSheet.Success)
        Assert.AreEqual(fromSheet.Tables.Count,fromDialog.Tables.Count)
        For i=0 To fromSheet.Tables.Count-1
            CollectionAssert.AreEqual(fromSheet.Tables(i).ToCoreTable().returnSelf().Cast(Of Object)().ToArray(),fromDialog.Tables(i).ToCoreTable().returnSelf().Cast(Of Object)().ToArray())
        Next
        data.CoreTable.ObjectMatrix(0,0)=999
        Assert.AreEqual(12,parsed.Data.CoreCounts(0,0))
    End Sub

    <DataTestMethod> <DataRow("fraction")> <DataRow("missing")> <DataRow("date")> <DataRow("bool")> <DataRow("nonfinite")>
    Public Sub StrictBridgeDoesNotRoundOrSilentlyDropRawCountRows(kind As String)
        Dim raw As Object(,)={{12,5},{7,19}}
        Select Case kind
            Case "fraction": raw(0,0)=1.5
            Case "missing": raw(0,0)=Nothing
            Case "date": raw(0,0)=New DateTime(2026,11,2)
            Case "bool": raw(0,0)=True
            Case "nonfinite": raw(0,0)=Double.NaN
        End Select
        Dim data=Imported(raw,{"A","B"})
        If kind="fraction" Then Assert.AreEqual(2,Matrix.Array2intArray(data.DataDbl)(0,0),"Frozen coercion candidate, not approved strict behavior")
        Dim result=WindowsContingencyInputAdapter.AnalyzeRxC(data,New RxCReportOptions)
        Assert.IsFalse(result.Success)
        Assert.AreEqual(0,result.Tables.Count)
        Assert.IsTrue(result.Validation.Issues.Any(Function(i) i.WorksheetRow=20 AndAlso i.SelectedColumn=1))
    End Sub

    <DataTestMethod> <DataRow("")> <DataRow("sk-SK")>
    Public Sub ProducerNumericTextCultureIsCapturedExplicitly(culture As String)
        Dim saved=Thread.CurrentThread.CurrentCulture
        Try
            Thread.CurrentThread.CurrentCulture=CultureInfo.GetCultureInfo(culture)
            Dim text=If(culture="","12.0","12,0")
            Dim data=Imported(New Object(,) {{text,5},{7,19}},{"A","B"})
            Dim input=WindowsContingencyInputAdapter.ParseWorksheet(data,New SpreadsheetContingencyCountParseOptions With {.HasColumnHeaders=False,.HasRowLabels=False},True)
            Assert.IsTrue(input.Success)
            Assert.AreEqual(12,input.Data.CoreCounts(0,0))
            Dim report=WindowsContingencyInputAdapter.AnalyzeRxC(data,New RxCReportOptions With {.Ordinal=True})
            Assert.IsTrue(report.Success)
            Assert.AreEqual(contingencytable.Chi2TESTindependence(input.Data.CoreCounts).Item1.Pvalue,CDbl(report.Tables(1).Body(1,1)))
        Finally
            Thread.CurrentThread.CurrentCulture=saved
        End Try
    End Sub

    <DataTestMethod> <DataRow(0)> <DataRow(1)> <DataRow(2)>
    Public Sub IndependentRealAdapter_NIHypothesesAndCoreSigns(hypothesis As Integer)
        Dim report=WindowsContingencyInputAdapter.AnalyzeProportions(ProportionsMode.Independent,CType(hypothesis,IndependentProportionsHypothesis),100,45,80,52,0,0.05,0.3)
        Assert.IsTrue(report.Success)
        Assert.AreEqual(If(hypothesis=0,-0.2,0.2),CDbl(report.Tables(0).Body(6,1)),0.000000000001)
        Assert.AreEqual(If(hypothesis=0,10,If(hypothesis=1,14,16)),report.Tables(0).Body.GetLength(0))
    End Sub

    <TestMethod>
    Public Sub PairedCompatibilityEvidenceIsExplicitlyLegacy_NotACorrectedGolden()
        Dim result=WindowsContingencyInputAdapter.AnalyzeProportions(ProportionsMode.Paired,IndependentProportionsHypothesis.Superiority,100,20,0,15,40,0.1,0)
        Assert.IsTrue(result.Success)
        Dim oldTable As Integer(,)={{-20,40},{105,-25}}
        Assert.AreEqual(contingencytable.Liddell_McNemar(oldTable).Item1.Pvalue,CDbl(result.Tables(0).Body(8,1)))
        Assert.AreEqual(contingencytable.PairedProportions(100,20,15,40,0.1).Estimate,CDbl(result.Tables(0).Body(6,1)))
        Assert.IsFalse(WindowsContingencyInputAdapter.AnalyzeProportions(ProportionsMode.Paired,IndependentProportionsHypothesis.Superiority,100,60,0,50,0,0.05,0).Success)
        Assert.IsFalse(WindowsContingencyInputAdapter.AnalyzeProportions(ProportionsMode.Paired,IndependentProportionsHypothesis.Superiority,1000,100,0,80,300,0.05,0).Success)
    End Sub

    <TestMethod>
    Public Sub InvalidAdapterInputsCannotSupplyAnOutputDocument()
        Assert.IsFalse(WindowsContingencyInputAdapter.AnalyzeRxC(Nothing,New RxCReportOptions).Success)
        Assert.IsFalse(WindowsContingencyInputAdapter.AnalyzeDialog(0,0,0,0,Options()).Success)
        Assert.IsFalse(WindowsContingencyInputAdapter.AnalyzeProportions(ProportionsMode.Independent,IndependentProportionsHypothesis.Equivalence,100,45,80,52,0,0.05,0).Success)
        Assert.IsFalse(WindowsContingencyInputAdapter.AnalyzeProportions(ProportionsMode.SingleSample,IndependentProportionsHypothesis.Superiority,0,0,0,0,0,0.05,0).Success)
        Assert.IsTrue(WindowsContingencyInputAdapter.AnalyzeProportions(ProportionsMode.SingleSample,IndependentProportionsHypothesis.Superiority,100,45,0,0,0,0.05,0).Success)
    End Sub
End Class
