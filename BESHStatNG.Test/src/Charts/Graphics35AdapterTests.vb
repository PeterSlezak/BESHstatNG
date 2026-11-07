Option Explicit On
Option Strict On
Option Infer On
Imports System
Imports System.Reflection
Imports System.Globalization
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports BESHStatNG
Imports BESHStatNG.ExcelCommon
Imports Legacy35
<TestClass> Public Class Graphics35AdapterTests
Private Shared Function Calls35(method As MethodBase) As Collections.Generic.List(Of MethodBase)
Dim bytes=method.GetMethodBody().GetILAsByteArray(),result As New Collections.Generic.List(Of MethodBase)
For i As Integer=0 To bytes.Length-5
If bytes(i)<>&H28 AndAlso bytes(i)<>&H6F AndAlso bytes(i)<>&H73 Then Continue For
Try
result.Add(method.Module.ResolveMethod(BitConverter.ToInt32(bytes,i+1)))
Catch ex As ArgumentException
End Try
Next
Return result
End Function
<TestMethod>
Public Sub CompiledShippingBodiesConsumeEachNewContract()
Dim comparison=Calls35(GetType(graphics.MultipleComparisonPlotExcel).GetMethod("AddPlot"))
Assert.IsTrue(comparison.Exists(Function(m) m.DeclaringType Is GetType(MultipleComparisonGeometry35) AndAlso m.IsConstructor))
For Each method In GetType(AgreementPlotExcelCompatibility).GetMethods(BindingFlags.Public Or BindingFlags.Static)
If method.Name<>"AddPlot" Then Continue For
Dim calls=Calls35(method)
Assert.IsTrue(calls.Exists(Function(m) m.Name="GetPlotData"))
Assert.IsTrue(calls.Exists(Function(m) m.DeclaringType Is GetType(AgreementPlotPolicy35)))
Next
Dim love=Calls35(GetType(graphics.PsmLovePlotExcel).GetMethod("AddProjectedChart"))
Assert.IsTrue(love.Exists(Function(m) m.DeclaringType Is GetType(PsmLoveChartSpec35) AndAlso m.IsConstructor))
Assert.IsTrue(love.Exists(Function(m) m.DeclaringType Is GetType(PsmLoveChartSpec35) AndAlso m.Name="get_LabelSeriesOrdinal"))
Dim spec=Calls35(GetType(PsmLoveChartSpec35).GetConstructors()(0))
Assert.IsTrue(spec.Exists(Function(m) m.DeclaringType Is GetType(PsmLovePlotProjection) AndAlso m.Name="get_SourceOrdinals"))
For Each owner In New Type(){GetType(graphics.MultipleComparisonPlotExcel),GetType(graphics.PsmLovePlotExcel)}
Dim labelMethod=owner.GetMethod(If(owner Is GetType(graphics.MultipleComparisonPlotExcel),"AddComparisonLabels","AddVariableLabels"),BindingFlags.NonPublic Or BindingFlags.Static)
Assert.IsTrue(Calls35(labelMethod).Exists(Function(m) m.DeclaringType Is GetType(ComparisonChartPolicy35) AndAlso m.Name="PointIndex"))
Next
End Sub
<TestMethod>
Public Sub MutableComparisonInputRemainsLiveAtOriginalLateReads()
Dim data As New parametric.MultipleComparisonPlotData With {.Labels=New String(){"old"},.Estimates=New Double(){1},.LowerLimits=New Double(){0},.UpperLimits=New Double(){3},.Alpha=0.05,.ProcedureName="Tukey-Kramer"}
Dim geometry As New MultipleComparisonGeometry35(data.Estimates,data.LowerLimits,data.UpperLimits)
data.Estimates(0)=9:data.Labels(0)="new":data.Alpha=0.1
Assert.AreEqual(1.0,geometry.Estimates(0))
Dim host=GetType(graphics.MultipleComparisonPlotExcel)
Assert.AreEqual(Original35.BuildTitle(data),CStr(host.GetMethod("BuildTitle",BindingFlags.NonPublic Or BindingFlags.Static).Invoke(Nothing,New Object(){data})))
Assert.AreEqual("new",ComparisonChartPolicy35.LabelAt(data.Labels,0).Text)
End Sub
<DataTestMethod><DataRow("")><DataRow("sk-SK")>
Public Sub ActualTitleValidationAndPreCreationErrorTiming(culture As String)
Dim saved=CultureInfo.CurrentCulture
Try
CultureInfo.CurrentCulture=CultureInfo.GetCultureInfo(culture)
Dim host=GetType(graphics.MultipleComparisonPlotExcel)
For Each name As String In New String(){"BuildTitle","ValidatePlotData"}
Dim old=GetType(Original35).GetMethod(name),current=host.GetMethod(name,BindingFlags.Static Or BindingFlags.NonPublic)
For i As Integer=0 To 279
Dim data=Cases35.Data(i)
Dim run As Func(Of MethodInfo,String)=Function(m) Execute(m,data)
Assert.AreEqual(run(old),run(current),name & i)
Next
Next
Dim noSheet=Assert.ThrowsException(Of TargetInvocationException)(Sub() host.GetMethod("AddPlot").Invoke(Nothing,New Object(){Nothing,Nothing,0.0,0.0,560.0}))
Assert.AreEqual("ws",DirectCast(noSheet.InnerException,ArgumentNullException).ParamName)
For Each method In GetType(AgreementPlotExcelCompatibility).GetMethods(BindingFlags.Public Or BindingFlags.Static)
If method.Name<>"AddPlot" Then Continue For
Dim args(method.GetParameters().Length-1) As Object
If args.Length=3 Then args(2)="title"
Dim ex=Assert.ThrowsException(Of TargetInvocationException)(Sub() method.Invoke(Nothing,args))
Assert.IsInstanceOfType(ex.InnerException,GetType(ArgumentNullException))
Assert.AreEqual("analysis",DirectCast(ex.InnerException,ArgumentNullException).ParamName)
Next
GetType(graphics.PsmLovePlotExcel).GetMethod("AddProjectedChart").Invoke(Nothing,New Object(){Nothing,Nothing,420.0,20.0,720.0,420.0})
Finally
CultureInfo.CurrentCulture=saved
End Try
End Sub
Private Shared Function Execute(method As MethodInfo,data As parametric.MultipleComparisonPlotData) As String
Try
Return Cases35.Snapshot(method.Invoke(Nothing,New Object(){data}))
Catch ex As TargetInvocationException
Dim arg=TryCast(ex.InnerException,ArgumentException)
Return ex.InnerException.GetType().FullName & ":" & If(arg Is Nothing,"",arg.ParamName)
End Try
End Function
End Class
