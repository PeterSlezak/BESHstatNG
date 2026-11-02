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
Public Class Completion05CommonTests
    Public Shared Function Cases() As IEnumerable(Of Object())
        Return Completion05References.Cases()
    End Function
    <DataTestMethod> <DynamicData(NameOf(Cases),DynamicDataSourceType.Method)>
    Public Sub CorrectedProductionPolicyAndReport(name As String,n As Integer,b As Integer,c As Integer,a As Integer,alpha As Double,
                                                  estimate As Double,lower As Double,upper As Double,p As Double,orLower As Double,orUpper As Double)
        Dim saved=Thread.CurrentThread.CurrentCulture
        Try
            For Each culture In {"","sk-SK"}
                Thread.CurrentThread.CurrentCulture=CultureInfo.GetCultureInfo(culture)
                Dim validated=ProportionsInputPolicy.Validate(ProportionsMode.Paired,IndependentProportionsHypothesis.Superiority,n,b,0,c,a,alpha,0)
                Assert.IsTrue(validated.Success,name)
                CollectionAssert.AreEqual(New Integer() {a,b,c,n-a-b-c},validated.Data.PairedOutcomeTable().Cast(Of Integer)().ToArray())
                CollectionAssert.AreEqual(New Integer() {n-a-b-c,b,c,a},validated.Data.CoreLiddellTable().Cast(Of Integer)().ToArray())
                Dim report=ProportionsReportPolicy.Analyze(validated.Data)
                Assert.IsTrue(report.Success,name & ":" & String.Join(";",report.Validation.Issues.Select(Function(i) i.Message)))
                Dim body=report.Tables(0).Body
                Assert.AreEqual(9,body.GetLength(0))
                Assert.AreEqual("Total Responders in the 1st Category",CStr(body(1,0)))
                Assert.AreEqual(a+b,CInt(body(1,1)))
                Assert.AreEqual((CDbl(a)+b)/n,CDbl(body(2,1)),1E-15)
                Assert.AreEqual("Total Responders in the 2nd Category",CStr(body(3,0)))
                Assert.AreEqual(a+c,CInt(body(3,1)))
                Assert.AreEqual("Proportion in the 2nd Category",CStr(body(4,0)))
                Assert.AreEqual((CDbl(a)+c)/n,CDbl(body(4,1)),1E-15)
                Assert.AreEqual(a,CInt(body(5,1)))
                Assert.AreEqual(estimate,CDbl(body(6,1)),5E-15)
                Assert.AreEqual($"{100*(1-alpha):0.##}% CI",CStr(body(7,0)))
                Assert.IsTrue(Math.Abs(p-CDbl(body(8,1)))<=Completion05References.PTolerance(p),name & " exact p")
                Assert.IsTrue(Completion05References.Close(lower,report.Metrics.Single(Function(m) m.Name="paired.lower").Value,2E-9,2E-8))
                Assert.IsTrue(Completion05References.Close(upper,report.Metrics.Single(Function(m) m.Name="paired.upper").Value,2E-9,2E-8))
                Dim model=report.Tables(0).ToCoreTable().ToOutputModel()
                Assert.AreEqual(1,model.PvalueCells.Count)
                Assert.AreEqual(9,model.PvalueCells(0).BodyRow)
                Assert.AreEqual(2,model.PvalueCells(0).BodyColumn)
                Dim changed=validated.Data.CoreLiddellTable() : changed(0,1)=0
                Assert.AreEqual(b,validated.Data.CoreLiddellTable()(0,1))
            Next
        Finally
            Thread.CurrentThread.CurrentCulture=saved
        End Try
    End Sub

    <DataTestMethod> <DataRow(0.0,0.0,0.0,0.0)> <DataRow(-1.0,0.0,0.0,0.0)> <DataRow(100.0,20.5,15.0,40.0)>
    <DataRow(100.0,60.0,50.0,0.0)> <DataRow(2147483648.0,1.0,1.0,1.0)>
    <DataRow(2147483647.0,2147483647.0,2147483647.0,2147483647.0)>
    Public Sub DisjointAndAggregateArithmeticRejectBeforeCore(n As Double,b As Double,c As Double,a As Double)
        Dim parsed=ProportionsInputPolicy.Validate(ProportionsMode.Paired,IndependentProportionsHypothesis.Superiority,n,b,0,c,a,0.05,0)
        Assert.IsFalse(parsed.Success)
        Assert.IsNull(parsed.Data)
        Assert.AreEqual(0,ProportionsReportPolicy.Analyze(parsed.Data).Tables.Count)
    End Sub

    <TestMethod>
    Public Sub NonfiniteInferenceNeverCreatesPartialTables()
        Dim request=ProportionsInputPolicy.Validate(ProportionsMode.Paired,IndependentProportionsHypothesis.Superiority,100,20,0,15,40,0.05,0).Data
        Dim ci As New ConfidenceIntervalResult With {.Estimate=0.05,.LowerLimit=Double.NaN,.UpperLimit=0.2}
        Dim test As New BESHStatNG.TestResult With {.Pvalue=0.5}
        Dim report=ProportionsReportPolicy.ProjectPaired(request,ci,test)
        Assert.IsFalse(report.Success)
        Assert.AreEqual(0,report.Tables.Count)
        Assert.AreEqual("proportions.paired-nonfinite",report.Validation.Issues(0).Code)
        ci.LowerLimit=-0.1 : ci.UpperLimit=0.2 : test.Pvalue=Double.NaN
        Assert.AreEqual(0,ProportionsReportPolicy.ProjectPaired(request,ci,test).Tables.Count)
    End Sub
End Class
