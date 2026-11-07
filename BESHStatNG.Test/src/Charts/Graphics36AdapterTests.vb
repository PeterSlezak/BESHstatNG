Option Explicit On
Option Strict On
Option Infer On
Imports System
Imports System.Linq
Imports System.Reflection
Imports System.Reflection.Emit
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports BESHStatNG
Imports BESHStatNG.ExcelCommon
Imports Legacy36
<TestClass> Public Class Graphics36AdapterTests
Private Shared Function Calls(method As MethodBase) As Collections.Generic.List(Of MethodBase)
Dim opcodes=GetType(OpCodes).GetFields(BindingFlags.Public Or BindingFlags.Static).Where(Function(f) f.FieldType Is GetType(OpCode)).Select(Function(f) DirectCast(f.GetValue(Nothing),OpCode)).ToDictionary(Function(o) o.Value)
Dim bytes=method.GetMethodBody().GetILAsByteArray(),i=0,result As New Collections.Generic.List(Of MethodBase)
While i<bytes.Length
Dim value As Short=CShort(bytes(i)):i+=1
If value=&HFE Then value=CShort(-512+CInt(bytes(i))):i+=1
Dim op=opcodes(value),size=0
Select Case op.OperandType
Case OperandType.InlineNone:size=0
Case OperandType.ShortInlineBrTarget,OperandType.ShortInlineI,OperandType.ShortInlineVar:size=1
Case OperandType.InlineVar:size=2
Case OperandType.InlineI8,OperandType.InlineR:size=8
Case OperandType.InlineSwitch:size=4+4*BitConverter.ToInt32(bytes,i)
Case Else:size=4
End Select
If op.OperandType=OperandType.InlineMethod Then result.Add(method.Module.ResolveMethod(BitConverter.ToInt32(bytes,i)))
i+=size
End While
Return result
End Function
<TestMethod> Public Sub ShippingILConsumesKMAndCoxAtOriginalPhases()
Dim km=GetType(graphics.KaplanMeierPlotExcel).GetMethods().Where(Function(m) m.Name="Plot").ToArray()
Dim payload=km.Single(Function(m) m.GetParameters()(0).ParameterType Is GetType(survival.KaplanMeierPlotData))
Dim methods=Calls(payload)
For Each name In New String(){"Survival","Censor","Confidence"}
Assert.IsTrue(methods.Any(Function(m) m.DeclaringType Is GetType(KaplanMeierSeriesGeometry36) AndAlso m.Name=name))
Next
Assert.IsTrue(methods.Any(Function(m) m.DeclaringType Is GetType(SurvivalChartPresentation36)))
Dim analysis=km.Single(Function(m) m.GetParameters()(0).ParameterType Is GetType(survival.Survival_KM_LR))
Assert.IsTrue(Calls(analysis).Any(Function(m) m.Equals(payload)))
Assert.IsTrue(Calls(GetType(survival.KaplanMeierPlotExcelCompatibility).GetMethod("AddKMplot")).Any(Function(m) m.Equals(analysis)))
Dim cox=Calls(GetType(graphics.CoxSurvivalPlotExcel).GetMethod("PlotCox"))
For Each name In New String(){"Validate","Axis","Snapshot"}
Assert.IsTrue(cox.Any(Function(m) m.DeclaringType Is GetType(CoxCurveGeometry36) AndAlso m.Name=name))
Next
Assert.AreEqual(2,cox.Where(Function(m) m.DeclaringType Is GetType(CoxCurveGeometry36) AndAlso m.Name="Snapshot").Count())
End Sub
<TestMethod> Public Sub ActualSharedSpcPreparationAndBlankMarshalling()
Dim host=GetType(graphics.SpcControlChartExcel),flags=BindingFlags.Static Or BindingFlags.Public Or BindingFlags.NonPublic
Dim prepare=host.GetMethod("PreparePresentation36",flags)
For i=0 To 191
Dim panel As SpcPanelSeriesProjection=Nothing
Try
panel=OriginalPresentation36V2.Panel(i)
Catch ex As ArgumentException
Assert.AreEqual("points",ex.ParamName)
Assert.IsFalse(Cases36.Points(i).Any(Function(v) v IsNot Nothing))
Continue For
End Try
Dim spec=OriginalPresentation36V2.Specifications(i)
Dim options As New graphics.SpcControlChartAppearanceOptions With {.ZoneDisplay=CType(i Mod 4,graphics.SpcZoneDisplayMode),.ShowSpecificationLimits=(i And 4)<>0,.ShowTargetLine=(i And 8)<>0,.ShowExcludedPoints=(i And 16)<>0,.ShowLimitLabels=(i And 32)<>0,.ShowZoneSeriesInLegend=(i And 64)<>0}
Dim plan=DirectCast(prepare.Invoke(Nothing,New Object(){panel,spec,options}),SpcPanelPresentation36)
Assert.AreEqual(Cases36.Snapshot(OriginalPresentation36V2.Series(panel,spec,i Mod 4,options.ShowSpecificationLimits,options.ShowTargetLine,options.ShowExcludedPoints,options.ShowLimitLabels,options.ShowZoneSeriesInLegend)),Cases36.Snapshot(Graphics36PresentationTests.Snapshot(plan)))
For Each intent In plan.Series
Dim actual=DirectCast(host.GetMethod("MarshalChartValues",flags).Invoke(Nothing,New Object(){intent.Values}),Object())
Assert.AreEqual(Cases36.Snapshot(intent.Values.Select(Function(v) If(v.HasValue,CType(v.Value,Object),Nothing)).ToArray()),Cases36.Snapshot(actual))
Next
Next
Assert.IsTrue(Calls(host.GetMethod("RenderPanel",flags)).Any(Function(m) m.Equals(prepare)))
Assert.IsTrue(Calls(host.GetMethod("RenderPanel",flags)).Any(Function(m) m.DeclaringType Is GetType(SpcZoneBands36) AndAlso m.IsConstructor))
End Sub
<TestMethod> Public Sub PreCreationNullWorksheetAndAnalysisErrorsRemain()
For Each method In GetType(graphics.KaplanMeierPlotExcel).GetMethods().Where(Function(m) m.Name="Plot")
Dim args(method.GetParameters().Length-1) As Object
args(2)=False:args(3)=False:args(4)="":args(5)=""
If args.Length=7 Then args(6)=0.05
Dim ex=Assert.ThrowsException(Of TargetInvocationException)(Sub() method.Invoke(Nothing,args))
Assert.AreEqual(If(args.Length=7,"analysis","ws"),DirectCast(ex.InnerException,ArgumentNullException).ParamName)
Next
Dim cox=Assert.ThrowsException(Of TargetInvocationException)(Sub() GetType(graphics.CoxSurvivalPlotExcel).GetMethod("PlotCox").Invoke(Nothing,New Object(){Nothing,Nothing,Nothing,100L,100L}))
Assert.AreEqual("ws",DirectCast(cox.InnerException,ArgumentNullException).ParamName)
End Sub
End Class
