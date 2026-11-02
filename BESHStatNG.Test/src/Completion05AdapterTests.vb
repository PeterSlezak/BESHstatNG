Option Explicit On
Option Strict On
Imports System
Imports System.Collections.Generic
Imports System.Globalization
Imports System.Linq
Imports System.Threading
Imports BESHStatNG.ExcelCommon
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass>
Public Class Completion05AdapterTests
    Public Shared Function Cases() As IEnumerable(Of Object())
        Return Completion05References.Cases()
    End Function
    <DataTestMethod> <DynamicData(NameOf(Cases),DynamicDataSourceType.Method)>
    Public Sub ShippingAdapterUsesIndependentCandidates(name As String,n As Integer,b As Integer,c As Integer,a As Integer,alpha As Double,
                                                        estimate As Double,lower As Double,upper As Double,p As Double,orLower As Double,orUpper As Double)
        For Each culture In {CultureInfo.InvariantCulture,CultureInfo.GetCultureInfo("sk-SK")}
            Dim saved=Thread.CurrentThread.CurrentCulture
            Try
                Thread.CurrentThread.CurrentCulture=culture
                Dim report=WindowsContingencyInputAdapter.AnalyzeProportions(ProportionsMode.Paired,IndependentProportionsHypothesis.Superiority,n,b,0,c,a,alpha,0)
                Assert.IsTrue(report.Success,name)
                Assert.AreEqual(estimate,CDbl(report.Tables(0).Body(6,1)),5E-15)
                Assert.IsTrue(Math.Abs(p-CDbl(report.Tables(0).Body(8,1)))<=Completion05References.PTolerance(p),name)
                Assert.IsTrue(Completion05References.Close(lower,report.Metrics.Single(Function(m) m.Name="paired.lower").Value,2E-9,2E-8))
                Assert.IsTrue(Completion05References.Close(upper,report.Metrics.Single(Function(m) m.Name="paired.upper").Value,2E-9,2E-8))
                Assert.AreEqual(a+b,CInt(report.Tables(0).Body(1,1)))
                Assert.AreEqual(a+c,CInt(report.Tables(0).Body(3,1)))
                Assert.AreEqual(9,report.ToLegacyTables()(0).ToOutputModel().PvalueCells(0).BodyRow)
            Finally
                Thread.CurrentThread.CurrentCulture=saved
            End Try
        Next
    End Sub

    <DataTestMethod> <DataRow("")> <DataRow("sk-SK")>
    Public Sub BothTwoByTwoInputsAndAll64OptionCombinations(culture As String)
        Dim saved=Thread.CurrentThread.CurrentCulture
        Try
            Thread.CurrentThread.CurrentCulture=CultureInfo.GetCultureInfo(culture)
            Dim imported As New DataObj
            imported.DataImportRawMatrix(New Object(,) {{12,5},{7,19}},{"A","B"},firstSourceRow:=20)
            Dim selected=WindowsContingencyInputAdapter.ParseWorksheet(imported,New SpreadsheetContingencyCountParseOptions With {.HasColumnHeaders=False,.HasRowLabels=False},True)
            Assert.IsTrue(selected.Success)
            For mask=0 To 63
                For Each alpha In {0.05,0.1}
                    Dim options As New TwoByTwoReportOptions With {.Alpha=alpha,.Fisher=(mask And 1)<>0,.ChiSquare=(mask And 2)<>0,
                        .Association=(mask And 4)<>0,.Liddell=(mask And 8)<>0,.OddsRatio=(mask And 16)<>0,.RiskRatio=(mask And 32)<>0}
                    Dim dialog=WindowsContingencyInputAdapter.AnalyzeDialog(12,5,7,19,options)
                    Dim worksheet=ContingencyReportPolicy.TwoByTwo(selected.Data,options)
                    Assert.AreEqual(mask<>0,dialog.Success)
                    Assert.AreEqual(dialog.Success,worksheet.Success)
                    Assert.AreEqual(dialog.Tables.Count,worksheet.Tables.Count)
                    For index=0 To dialog.Tables.Count-1
                        Dim a=dialog.Tables(index).ToCoreTable().ToOutputModel(),b=worksheet.Tables(index).ToCoreTable().ToOutputModel()
                        CollectionAssert.AreEqual(a.Values.Cast(Of Object)().ToArray(),b.Values.Cast(Of Object)().ToArray())
                        CollectionAssert.AreEqual(a.PvalueCells.ToArray(),b.PvalueCells.ToArray())
                        CollectionAssert.AreEqual(a.PvalueColumns.ToArray(),b.PvalueColumns.ToArray())
                    Next
                    If mask=4 Then Assert.AreEqual(3,dialog.Tables.Count,"Association retains Chi-square")
                Next
            Next
        Finally
            Thread.CurrentThread.CurrentCulture=saved
        End Try
    End Sub

    <TestMethod>
    Public Sub StrictFailuresFisherOrderingAndZeroWritesDocumentBoundary()
        For Each raw In {New Object(,) {{1.5,5},{7,19}},New Object(,) {{Nothing,5},{7,19}},New Object(,) {{-1,5},{7,19}},
                         New Object(,) {{Double.NaN,5},{7,19}},New Object(,) {{CDbl(Integer.MaxValue)+1,5},{7,19}},New Object(,) {{1,2,3},{4,5,6}}}
            Dim data As New DataObj
            data.DataImportRawMatrix(raw,Enumerable.Range(0,raw.GetLength(1)).Select(Function(i) "V" & i.ToString()).ToArray(),firstSourceRow:=20)
            Dim selected=WindowsContingencyInputAdapter.ParseWorksheet(data,New SpreadsheetContingencyCountParseOptions With {.HasColumnHeaders=False,.HasRowLabels=False},True)
            Assert.IsFalse(selected.Success)
            Assert.IsNull(selected.Data)
        Next
        Dim none=WindowsContingencyInputAdapter.AnalyzeDialog(12,5,7,19,New TwoByTwoReportOptions)
        Assert.IsFalse(none.Success) : Assert.AreEqual(0,none.ToLegacyTables().Count)
        Dim guard=WindowsContingencyInputAdapter.AnalyzeDialog(998,1,1,1,New TwoByTwoReportOptions With {.Fisher=True})
        Assert.IsFalse(guard.Success) : Assert.AreEqual(0,guard.ToLegacyTables().Count)
        Assert.AreEqual("contingency.fisher.too-large",guard.Validation.Issues(0).Code)
        Assert.AreEqual("contingency.no-result",guard.Validation.Issues(1).Code)
        Assert.IsTrue(WindowsContingencyInputAdapter.AnalyzeDialog(998,1,1,1,New TwoByTwoReportOptions With {.Fisher=True,.ChiSquare=True}).Success)
        Assert.IsFalse(WindowsContingencyInputAdapter.AnalyzeProportions(ProportionsMode.Paired,IndependentProportionsHypothesis.Superiority,100,60,0,50,0,0.05,0).Success)
    End Sub
End Class
