Option Explicit On
Option Strict On
Option Infer On
Imports System
Imports System.Collections.Generic
Imports System.Globalization
Imports System.Linq
Imports System.Reflection
Imports BESHStatNG
Imports BESHStatNG.ExcelCommon
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass>
Public Class Agreement26AdapterTests
    Private Shared Function Calls(method As MethodInfo) As List(Of MethodBase)
        Dim bytes = method.GetMethodBody().GetILAsByteArray(), values As New List(Of MethodBase)
        For offset = 0 To bytes.Length - 5
            If bytes(offset) <> &H28 AndAlso bytes(offset) <> &H6F AndAlso bytes(offset) <> &H73 Then Continue For
            Try : values.Add(method.Module.ResolveMethod(BitConverter.ToInt32(bytes, offset + 1))) : Catch ex As ArgumentException : End Try
        Next
        Return values
    End Function
    <DataTestMethod><DataRow("RunPTtest")><DataRow("RunUPTtest")><DataRow("RunSingleProp")><DataRow("RunIndependentProp")><DataRow("RunLogRank")><DataRow("RunCox")><DataRow("RunICC")><DataRow("RunBlandAltman")>
    Public Sub AllEightShippingPlanningHandlersConsumeTypedRequestAndProjection(name As String)
        Dim body = Calls(GetType(Ui12SampleSizeTtestSingleProp).GetMethod(name, BindingFlags.Instance Or BindingFlags.NonPublic))
        Assert.IsTrue(body.Any(Function(m) m.DeclaringType = GetType(SampleSizePlanningRequest) AndAlso m.IsConstructor))
        Assert.IsTrue(body.Any(Function(m) m.DeclaringType = GetType(SampleSizePlanningPolicy) AndAlso m.Name = "Calculate"))
        Assert.IsTrue(body.Any(Function(m) m.DeclaringType = GetType(SampleSizePlanningReport) AndAlso m.Name = "get_Text"))
        Assert.IsFalse(body.Any(Function(m) m.DeclaringType.FullName.Contains("SampleSizeCalculator")))
        Assert.IsTrue(body.Any(Function(m) m.Name = "AppendText"))
    End Sub
    <DataTestMethod><DataRow("RunDeming")><DataRow("RunLinsCCC")><DataRow("RunCohensKappa")>
    Public Sub ActualTwoInputMethodsUseOptionInputAndReportPolicies(name As String)
        Dim body = Calls(GetType(UiTwoInputRefedits).GetMethod(name, BindingFlags.Instance Or BindingFlags.NonPublic))
        Dim selection = If(name = "RunDeming", GetType(DemingRequestSelection), If(name = "RunLinsCCC", GetType(LinRequestSelection), GetType(KappaRequestSelection)))
        Assert.IsTrue(body.Any(Function(m) m.DeclaringType = selection AndAlso m.Name = "CreateOptions"))
        Assert.IsTrue(body.Any(Function(m) m.DeclaringType = GetType(AgreementReportPlan) AndAlso m.Name = "TablesForWriter"))
        Assert.IsTrue(body.Any(Function(m) m.DeclaringType = GetType(AgreementReportPlan) AndAlso m.Name = "NativeRowSpan"))
        Assert.IsTrue(body.Any(Function(m) m.DeclaringType = GetType(AgreementInputAxis)))
    End Sub
    <TestMethod>
    Public Sub ActualBlandAndIccConsumePlansAndRetainCoreDispatch()
        Dim flags = BindingFlags.Instance Or BindingFlags.NonPublic
        Dim bland = Calls(GetType(Ui9ANOVA2nested).GetMethod("RunBlandAltman", flags))
        For Each owner In {GetType(BlandRequestSelection), GetType(AgreementInputAxis), GetType(AgreementReportPlan)}
            Assert.IsTrue(bland.Any(Function(m) m.DeclaringType = owner))
        Next
        Dim icc = Calls(GetType(Ui0OneRefeditMulticol).GetMethod("RunICC", flags))
        Assert.IsTrue(icc.Any(Function(m) m.DeclaringType = GetType(IccRequestIntent)))
        For Each name In {"ICC11", "ICC1k", "ICC21", "ICC2k", "ICC31", "ICC3k"}
            Assert.AreEqual(1, icc.Where(Function(m) m.Name = name).Count())
        Next
        Assert.IsTrue(icc.Any(Function(m) m.DeclaringType = GetType(AgreementReportPlan) AndAlso m.Name = "NativeRowSpan"))
    End Sub
    <DataTestMethod><DataRow("")><DataRow("sk-SK")>
    Public Sub NativeNumericParsingRemainsCultureOwned(culture As String)
        Dim saved = CultureInfo.CurrentCulture
        Try
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(culture)
            Dim value = UIprocedures.ParseUiDouble(0.2R.ToString(CultureInfo.CurrentCulture), "beta")
            Dim request As New SampleSizePlanningRequest(SampleSizePlanningMethod.PairedTTest, 0.05R, value, {5.0R, 10.0R})
            Assert.AreEqual(0.2R, request.Beta)
            Assert.AreEqual(2, CInt(UIprocedures.ParseUiDouble("2", "observations")))
        Finally
            CultureInfo.CurrentCulture = saved
        End Try
    End Sub
End Class
