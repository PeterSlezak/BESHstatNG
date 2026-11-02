Option Explicit On
Option Strict On
Imports System
Imports System.Collections.Generic
Imports BESHStatNG
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass>
Public Class Completion05CoreTests
    Public Shared Function Cases() As IEnumerable(Of Object())
        Return Completion05References.Cases()
    End Function
    <DataTestMethod> <DynamicData(NameOf(Cases),DynamicDataSourceType.Method)>
    Public Sub IndependentCandidates(name As String,n As Integer,b As Integer,c As Integer,a As Integer,alpha As Double,
                                    estimate As Double,lower As Double,upper As Double,p As Double,orLower As Double,orUpper As Double)
        Dim ci=contingencytable.PairedProportions(n,b,c,a,alpha)
        Assert.IsTrue(Completion05References.Close(estimate,ci.Estimate,5.0E-15,0),name & " estimate")
        Assert.IsTrue(Completion05References.Close(lower,ci.LowerLimit,2.0E-9,2.0E-8),name & " lower")
        Assert.IsTrue(Completion05References.Close(upper,ci.UpperLimit,2.0E-9,2.0E-8),name & " upper")
        Assert.AreEqual(alpha,ci.alpha)
        Assert.IsTrue(ci.LowerLimit>=-1.0000000001 AndAlso ci.UpperLimit<=1.0000000001)
        Dim exact=contingencytable.Liddell_McNemar(New Integer(,) {{n-a-b-c,b},{c,a}},alpha)
        Assert.IsTrue(Math.Abs(p-exact.Item1.Pvalue)<=Completion05References.PTolerance(p),name & " independent binomial exact p")
        Assert.IsTrue(exact.Item1.Pvalue>=0 AndAlso exact.Item1.Pvalue<=1)
        Assert.IsTrue(Completion05References.Close(orLower,exact.Item2.LowerLimit,2.0E-7,2.0E-7),name & " independent odds lower")
        Assert.IsTrue(Completion05References.Close(orUpper,exact.Item2.UpperLimit,2.0E-7,2.0E-7),name & " independent odds upper")
        If b+c=0 Then
            Assert.AreEqual(1.0,exact.Item1.Pvalue)
            Assert.IsTrue(Double.IsNaN(exact.Item2.Estimate))
            StringAssert.Contains(exact.Item2.strConfidenceInterval,"Not estimable")
        ElseIf c=0 Then
            Assert.IsTrue(Double.IsPositiveInfinity(exact.Item2.Estimate))
            Assert.IsTrue(Double.IsPositiveInfinity(exact.Item2.UpperLimit))
        End If
    End Sub

    ' Supplemental Decimal65 candidates from the same independent formula evaluator.
    ' The local ci-only-candidates.json records full precision and provenance; these are not P goldens.
    <DataTestMethod>
    <DataRow(200000,50000,50000,0,-0.0030989602807552495,0.0030989602807552495)>
    <DataRow(1000,400,350,50,-0.0036533778701332391,0.1033206532294795)>
    <DataRow(2147483647,1000000000,1000000000,0,-0.0000408162614425991,0.0000408162614425991)>
    Public Sub LargeNegativeCorrelationProducts(n As Integer,b As Integer,c As Integer,a As Integer,lower As Double,upper As Double)
        Dim ci=contingencytable.PairedProportions(n,b,c,a,0.05)
        Assert.IsTrue(Completion05References.Close(lower,ci.LowerLimit,2E-9,2E-8))
        Assert.IsTrue(Completion05References.Close(upper,ci.UpperLimit,2E-9,2E-8))
    End Sub
End Class
