Option Explicit On
Option Strict On
Imports System
Imports System.Globalization
Imports System.Threading
Imports BESHStatNG
Imports Microsoft.VisualStudio.TestTools.UnitTesting

''' <summary>Pre/post observation only. Exceptions are recorded, never labeled numerical qualification.</summary>
<TestClass>
Public Class Completion05CoreProbeTests
    Public Property TestContext As TestContext
    <DataTestMethod>
    <DataRow(100,20,15,40)> <DataRow(1000,100,80,300)> <DataRow(1000,400,350,50)>
    <DataRow(100,0,0,40)> <DataRow(10,1,1,0)> <DataRow(1000,1000,0,0)>
    Public Sub ObserveBeforeAfter(n As Integer, b As Integer, c As Integer, a As Integer)
        Dim saved=Thread.CurrentThread.CurrentCulture
        Try
            For Each name In {"","sk-SK"}
                Thread.CurrentThread.CurrentCulture=CultureInfo.GetCultureInfo(name)
                Try
                    Dim ci=contingencytable.PairedProportions(n,b,c,a,0.05)
                    TestContext.WriteLine("CI {0}/{1}/{2}/{3} culture={4} estimate={5:R} lower={6:R} upper={7:R}",n,b,c,a,name,ci.Estimate,ci.LowerLimit,ci.UpperLimit)
                Catch ex As ArithmeticException
                    TestContext.WriteLine("CI {0}/{1}/{2}/{3} culture={4} EXCEPTION {5}",n,b,c,a,name,ex.GetType().FullName)
                End Try
                Dim result=contingencytable.Liddell_McNemar(New Integer(,) {{n-a-b-c,b},{c,a}},0.05)
                TestContext.WriteLine("EXACT b={0} c={1} p={2:R} OR={3:R} lower={4:R} upper={5:R}",b,c,result.Item1.Pvalue,result.Item2.Estimate,result.Item2.LowerLimit,result.Item2.UpperLimit)
            Next
        Finally
            Thread.CurrentThread.CurrentCulture=saved
        End Try
    End Sub
End Class
