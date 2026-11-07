Option Explicit On
Option Strict On
Option Infer On
Imports System
Imports System.Linq
Imports System.Globalization
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports BESHStatNG
Imports Legacy36
<TestClass> Public Class Graphics36GeometryTests
Public Shared Function Result(action As Func(Of Object)) As String
Try
Return Cases36.Snapshot(action())
Catch ex As Exception
Dim a=TryCast(ex,ArgumentException)
Return ex.GetType().FullName & ":" & If(a Is Nothing,"",a.ParamName)
End Try
End Function
Public Shared Function Project(data As survival.KaplanMeierPlotData,group As Integer,role As Integer) As KaplanMeierSeries36
Select Case role
Case 0:Return KaplanMeierSeriesGeometry36.Survival(data,group)
Case 1:Return KaplanMeierSeriesGeometry36.Censor(data,group)
Case Else:Return KaplanMeierSeriesGeometry36.Confidence(data,group,role=2)
End Select
End Function
<DataTestMethod><DataRow(0)><DataRow(1)><DataRow(2)><DataRow(3)>
Public Sub FrozenOriginalKMLoopsAndExceptions(role As Integer)
Dim old=GetType(Original36).GetMethod("KM" & role)
For i=0 To 151
For group=0 To 3
Dim data=Cases36.KM(i)
Dim expected=Result(Function()
Try
Return old.Invoke(Nothing,New Object(){data,group})
Catch ex As Reflection.TargetInvocationException
Throw ex.InnerException
End Try
End Function)
Dim actual=Result(Function()
Dim p=Project(data,group,role)
Return New Object(){p.XValues,p.YValues,p.AssignValues}
End Function)
Assert.AreEqual(expected,actual,"case " & i & " group " & group & " role " & role)
Next:Next
End Sub
<DataTestMethod><DataRow(0)><DataRow(1)><DataRow(2)><DataRow(3)>
Public Sub EveryKMVectorSnapshotAndLiveNextRead(role As Integer)
Dim data=Cases36.KM(19), p=Project(data,0,role)
Dim before=Cases36.Snapshot(New Object(){p.XValues,p.YValues,p.XSourceSlots,p.YSourceSlots})
For Each prop In p.GetType().GetProperties().Where(Function(v) v.PropertyType.IsArray)
Dim a=DirectCast(prop.GetValue(p,Nothing),Array)
If a.Length>0 Then a.SetValue(Convert.ChangeType(99,a.GetType().GetElementType()),0)
Next
data.SurvivalProbability(0,0)=99:data.SurvivalTime(0)=77:data.UpperConfidenceLimit(0,0)=88:data.CensorMarkerProbability(0,0)=66
Assert.AreEqual(before,Cases36.Snapshot(New Object(){p.XValues,p.YValues,p.XSourceSlots,p.YSourceSlots}))
Assert.AreEqual(0,p.SourceGroupOrdinal)
End Sub
<TestMethod> Public Sub LegacyOriginCensorBlankAndUnassignedCITrailingSlots()
Dim d=Cases36.KM(19)
d.CensoredCountByGroup=New Integer(){0,1}:d.SurvivalTime=d.SurvivalTime.Concat(New Double(){10}).ToArray()
Dim curve=KaplanMeierSeriesGeometry36.Survival(d,0),ci=KaplanMeierSeriesGeometry36.Confidence(d,0,True),censor=KaplanMeierSeriesGeometry36.Censor(d,0)
Assert.AreEqual(0.0,curve.XValues(0)):Assert.AreEqual(1.0,curve.YValues(0)):Assert.AreEqual(-1,curve.XSourceSlots(0))
Assert.AreEqual(curve.XValues.Length+1,ci.XValues.Length)
Assert.AreEqual(-2,ci.XSourceSlots.Last())
Assert.IsFalse(censor.AssignValues):Assert.AreEqual(1,censor.XValues.Length):Assert.AreEqual(-2,censor.XSourceSlots(0))
End Sub
<TestMethod> Public Sub SparseCensorCountFailureStaysAtCensorPhase()
Dim d=Cases36.KM(20)
d.CensoredCountByGroup=New Integer(){1}
KaplanMeierSeriesGeometry36.Survival(d,1)
Assert.ThrowsException(Of IndexOutOfRangeException)(Sub() KaplanMeierSeriesGeometry36.Censor(d,1))
End Sub
<TestMethod> Public Sub CoxOriginalScalingVectorsAndValidation()
For i=0 To 151
Dim x=Cases36.Numbers(i),y=Cases36.Numbers(If(i Mod 3=0,i+1,i))
Assert.AreEqual(Result(Function() Original36.Cox(x,y)),Result(Function()
CoxCurveGeometry36.Validate(x,y)
If x.Length=0 Then Return Nothing
Dim scale=CoxCurveGeometry36.Axis(x)
Return New Object(){CoxCurveGeometry36.Snapshot(x),CoxCurveGeometry36.Snapshot(y),scale.Min,scale.Max,scale.Scale,0.0,1.0,0.2,"Baseline","Cox - Survival plot","Survival Probability","Time"}
End Function),"Cox " & i)
Next
Dim input As Double()={-0.0,1},p=CoxCurveGeometry36.Snapshot(input)
input(0)=42:Assert.AreEqual(Long.MinValue,BitConverter.DoubleToInt64Bits(p(0)))
End Sub
<TestMethod> Public Sub FrozenOriginalZonesAndDeepCopies()
For i=0 To 151
Dim points=Cases36.Points(i),minimum=New Double(){-5,-0.0,0,Double.NaN,Double.NegativeInfinity}(i Mod 5), bands As New SpcZoneBands36(points,minimum)
Dim expected=Original36.Bands(points,minimum),actual As New Collections.Generic.List(Of Object) From {bands.ValidPointCount}
For Each vector In bands.Values:actual.Add(vector.Select(Function(v) If(v.HasValue,CType(v.Value,Object),Nothing)).ToArray()):Next
Assert.AreEqual(Cases36.Snapshot(expected),Cases36.Snapshot(actual.ToArray()),"bands " & i)
Dim before=Cases36.Snapshot(bands.Values),first=bands.Values
If first(0).Length>0 Then first(0)(0)=99:first(1)=Nothing
If points.Length>0 Then points(0)=Nothing
Assert.AreEqual(before,Cases36.Snapshot(bands.Values))
Next
End Sub
<TestMethod> Public Sub RepeatedConcurrentGettersAreDeterministic()
Dim points=Cases36.Points(19),bands As New SpcZoneBands36(points,-5)
Dim d=Cases36.KM(19),curve=KaplanMeierSeriesGeometry36.Survival(d,0)
Dim expected=Cases36.Snapshot(New Object(){bands.Values,curve.XValues,curve.YValues,curve.XSourceSlots})
Threading.Tasks.Parallel.For(0,32,Sub(i)
Assert.AreEqual(expected,Cases36.Snapshot(New Object(){bands.Values,curve.XValues,curve.YValues,curve.XSourceSlots}))
End Sub)
End Sub
End Class
