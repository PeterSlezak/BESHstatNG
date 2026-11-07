"""Add focused cases after the frozen fresh-main run; no old test edits."""
from work import *
COMMON='''Option Explicit On
Option Strict On
Option Infer On
Imports System
Imports System.Linq
Imports System.Globalization
Imports System.Reflection
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports BESHStatNG
Imports BESHStatNG.ExcelCommon
Imports Legacy35
<TestClass>
Public Class Graphics35Tests
Public Shared Function Comparison(data As parametric.MultipleComparisonPlotData) As Object()
ComparisonChartPolicy35.ValidatePlotData(data)
Dim geometry As New MultipleComparisonGeometry35(data.Estimates,data.LowerLimits,data.UpperLimits)
Return New Object(){geometry.YValues,geometry.PlusError,geometry.MinusError,geometry.MinimumX,geometry.MaximumX,ComparisonChartPolicy35.BuildTitle(data),ExcelCommon.Chart34.ComparisonSeries.ComparisonSeriesPreparation34.SuggestedHeight(data)}
End Function
Public Shared Function Series(values As Collections.Generic.IReadOnlyList(Of AgreementPlotSeries35)) As Object()
Return values.Select(Function(v) CType(New Object(){v.Name,v.XValues,v.YValues},Object)).ToArray()
End Function
Public Shared Function Love(rows As Collections.Generic.IList(Of CausalInference.PsmLovePlotRow)) As Object()
Dim projection As New PsmLovePlotProjection(rows),spec As New PsmLoveChartSpec35(projection)
Dim values As New Collections.Generic.List(Of Object)
If spec.IncludesChannel(0) Then values.Add(spec.Before)
If spec.IncludesChannel(1) Then values.Add(spec.AfterMatching)
If spec.IncludesChannel(2) Then values.Add(spec.AfterWeighting)
Return New Object(){spec.Names,values.ToArray(),spec.YValues,spec.Labels,spec.LabelSeriesOrdinal,spec.Threshold,spec.MaximumX,projection.BuildDataTable()}
End Function
Public Shared Function Result(action As Func(Of Object)) As String
Try
Return Cases35.Snapshot(action())
Catch ex As Exception
Dim arg=TryCast(ex,ArgumentException)
Return "ERROR:" & ex.GetType().FullName & ":" & If(arg Is Nothing,"",arg.ParamName)
End Try
End Function
<DataTestMethod><DataRow("")><DataRow("sk-SK")>
Public Sub IndependentFrozenComparisonAndValidation(culture As String)
Dim saved=CultureInfo.CurrentCulture
Try
CultureInfo.CurrentCulture=CultureInfo.GetCultureInfo(culture)
For i As Integer=0 To 279
Dim input=Cases35.Data(i)
Assert.AreEqual(Result(Function() Original35.Comparison(input)),Result(Function() Comparison(input)),"comparison" & i)
Assert.AreEqual(Result(Function() Original35.BuildTitle(input)),Result(Function() ComparisonChartPolicy35.BuildTitle(input)),"title" & i)
Dim labels As String()=If(i Mod 11=0,Nothing,New String(){Nothing,"a","α","a"})
Dim pointCount=i Mod 9-2
Dim actual As Func(Of Object)=Function() ComparisonChartPolicy35.LabelOrdinals(pointCount,labels.Length).Select(Function(ordinal) CType(New Object(){ordinal,ComparisonChartPolicy35.LabelAt(labels,ordinal).ExcelPointIndex,ComparisonChartPolicy35.LabelAt(labels,ordinal).Text},Object)).ToArray()
Assert.AreEqual(Result(Function() Original35.LabelMap(labels,pointCount)),Result(actual),"labels" & i)
Next
Finally
CultureInfo.CurrentCulture=saved
End Try
End Sub
<DataTestMethod><DataRow("")><DataRow("sk-SK")>
Public Sub IndependentFrozenAgreementEndpointsAndOrder(culture As String)
Dim saved=CultureInfo.CurrentCulture
Try
CultureInfo.CurrentCulture=CultureInfo.GetCultureInfo(culture)
For i As Integer=0 To 279
Dim nums=Cases35.Numbers(i),a=nums(0),b=If(i Mod 3=0,a,3.0),intercept=If(i Mod 2=0,-0.0,2.0),slope=If(i Mod 3=0,Double.NaN,-1.2)
Assert.AreEqual(Result(Function() Original35.Regression(a,b,intercept,slope)),Result(Function() Series(AgreementPlotPolicy35.Regression(a,b,intercept,slope))))
Assert.AreEqual(Result(Function() Original35.Identity(a,b)),Result(Function() Series(AgreementPlotPolicy35.Identity(a,b))))
Dim x As Double()=If(i Mod 17=0,Nothing,If(i Mod 13=0,New Double(){},nums)),sx As Double()=If(i Mod 5=0,Nothing,nums),sy As Double()=If(i Mod 7=0,Nothing,New Double(){1.0})
Assert.AreEqual(Result(Function() Original35.Bland(x,a,b,-b,i Mod 2=0,sx,sy)),Result(Function() Series(AgreementPlotPolicy35.Bland(x,a,b,-b,i Mod 2=0,sx,sy))),"Bland" & i)
Next
Finally
CultureInfo.CurrentCulture=saved
End Try
End Sub
<DataTestMethod><DataRow("")><DataRow("sk-SK")>
Public Sub IndependentFrozenLoveProjectionAndChartAdmission(culture As String)
Dim saved=CultureInfo.CurrentCulture
Try
CultureInfo.CurrentCulture=CultureInfo.GetCultureInfo(culture)
For i As Integer=0 To 279
Assert.AreEqual(Result(Function() Original35.Love(Cases35.Rows(i))),Result(Function() Love(Cases35.Rows(i))),"Love" & i)
Next
Finally
CultureInfo.CurrentCulture=saved
End Try
End Sub
<TestMethod>
Public Sub DetachedIntervalsReferencesAndLabels()
Dim x As Double()={-2,3},lo As Double()={-4,2},hi As Double()={-1,9}
Dim geometry As New MultipleComparisonGeometry35(x,lo,hi)
x(0)=99:lo(0)=99:hi(0)=99
CollectionAssert.AreEqual(New Double(){1,6},geometry.PlusError)
CollectionAssert.AreEqual(New Double(){2,1},geometry.MinusError)
Dim returned=geometry.PlusError:returned(0)=999
Assert.AreEqual(1.0,geometry.PlusError(0))
CollectionAssert.AreEqual(New Double(){2,1},geometry.YValues)
Assert.AreEqual(-4.0,geometry.MinimumX):Assert.AreEqual(9.0,geometry.MaximumX)
CollectionAssert.AreEqual(New Double(){0,0},geometry.ZeroReference.XValues)
CollectionAssert.AreEqual(New Double(){0.5,2.5},geometry.ZeroReference.YValues)
Dim lines=AgreementPlotPolicy35.Bland(New Double(){2,2},0,-1,1,True,New Double(){1},New Double(){2})
CollectionAssert.AreEqual(New String(){"Bias","Lower LoA","Upper LoA","Subject means"},lines.Select(Function(v) v.Name).ToArray())
Dim values=lines(0).XValues:values(0)=99
CollectionAssert.AreEqual(New Double(){1.5,2.5},lines(0).XValues)
Dim source As Double()={1},other As Double()={2},series As New AgreementPlotSeries35("x",source,other)
source(0)=99:other(0)=99
Assert.AreEqual(1.0,series.XValues(0)):Assert.AreEqual(2.0,series.YValues(0))
Dim labels As String()={"first","second"},label=ComparisonChartPolicy35.LabelAt(labels,1)
labels(1)="changed"
Assert.AreEqual("second",label.Text):Assert.AreEqual(1,label.OrderedOrdinal):Assert.AreEqual(2,label.ExcelPointIndex)
End Sub
<TestMethod>
Public Sub LoveSourceOrdinalsSnapshotsAndLastDataSeries()
Dim rows As New Collections.Generic.List(Of CausalInference.PsmLovePlotRow) From {Nothing,New CausalInference.PsmLovePlotRow With {.VariableName=" ",.SmdBefore=9},New CausalInference.PsmLovePlotRow With {.VariableName="b",.SmdBefore=0.2,.SmdAfterMatching=Double.NaN,.SmdAfterWeighting=Double.NaN,.Threshold=Double.NaN},New CausalInference.PsmLovePlotRow With {.VariableName="A",.SmdBefore=-0.2,.SmdAfterMatching=Double.NaN,.SmdAfterWeighting=Double.NaN,.Threshold=0},New CausalInference.PsmLovePlotRow With {.VariableName="a",.SmdBefore=0.2,.SmdAfterMatching=Double.NaN,.SmdAfterWeighting=Double.NaN,.Threshold=0}}
Dim projection As New PsmLovePlotProjection(rows),spec As New PsmLoveChartSpec35(projection)
CollectionAssert.AreEqual(New Integer(){3,4,2},projection.SourceOrdinals)
CollectionAssert.AreEqual(New Integer(){3,4,2},spec.SourceOrdinals)
CollectionAssert.AreEqual(New String(){"Before"},spec.Names)
Assert.AreEqual(0,spec.LabelSeriesOrdinal):Assert.IsTrue(spec.ReverseY)
rows(3).VariableName="changed":rows(3).SmdBefore=9:rows.Clear()
Assert.AreEqual("A",spec.Labels(0)):Assert.AreEqual(0.2,spec.Before(0))
Dim source=projection.SourceOrdinals:source(0)=999
Dim labels=spec.Labels:labels(0)="changed"
Dim values=spec.Before:values(0)=999
Assert.AreEqual(3,projection.SourceOrdinals(0)):Assert.AreEqual("A",spec.Labels(0)):Assert.AreEqual(0.2,spec.Before(0))
Dim table=projection.BuildDataTable():table(1,1)="changed"
Assert.AreEqual("A",projection.BuildDataTable()(1,1))
Assert.AreEqual(0.0,CDbl(projection.BuildDataTable()(1,5)))
Assert.AreEqual(0.0,CDbl(projection.BuildDataTable()(3,5)))
Assert.IsNull(projection.BuildDataTable()(1,3))
Dim missing As New PsmLoveChartSpec35(New PsmLovePlotProjection(New Collections.Generic.List(Of CausalInference.PsmLovePlotRow) From {New CausalInference.PsmLovePlotRow With {.VariableName="x",.SmdBefore=Double.NaN,.SmdAfterMatching=Double.PositiveInfinity,.SmdAfterWeighting=Double.NaN,.Threshold=Double.NaN}}))
Assert.AreEqual(-1,missing.LabelSeriesOrdinal):Assert.AreEqual(0.25,missing.MaximumX)
End Sub
<TestMethod>
Public Sub AcceptedReportPoliciesRemainActualOwners()
For i As Integer=0 To 31
Dim plan=PsmReportSelectionPolicy.Plan((i And 1)<>0,(i And 2)<>0,(i And 4)<>0,(i And 8)<>0,(i And 16)<>0)
Assert.AreEqual(PsmReportSection.InputData,plan(0).Section):Assert.AreEqual(PsmReportSection.Results,plan(1).Section)
Assert.AreEqual(If((i And 4)<>0 AndAlso (i And 8)<>0,1,0),plan.Where(Function(p) p.Section=PsmReportSection.LovePlot).Count())
Next
For Each method As AgreementMethod In [Enum].GetValues(GetType(AgreementMethod))
Dim tables As New Collections.Generic.List(Of ResultTable) From {New ResultTable,New ResultTable}
Dim plan As New AgreementReportPlan(method,tables,Nothing)
Assert.AreEqual(If(method=AgreementMethod.BlandAltman,13,11),plan.NativeRowSpan(10))
Dim returned=plan.TablesForWriter():returned.Clear()
Assert.AreEqual(2,plan.TablesForWriter().Count)
Assert.AreSame(tables(0),plan.TablesForWriter()(0))
Next
End Sub
End Class
'''
CORE='''Option Explicit On
Option Strict On
Imports System
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports BESHStatNG
<TestClass> Public Class Graphics35GeometryTests
<DataTestMethod><DataRow(-2.0,-4.0,1.0)><DataRow(2.0,4.0,1.0)><DataRow(0.0,0.0,0.0)>
Public Sub ClampedArmsAndZeroBounds(estimate As Double,lower As Double,upper As Double)
Dim x As Double()={estimate},lo As Double()={lower},hi As Double()={upper}
Dim p As New MultipleComparisonGeometry35(x,lo,hi)
Assert.AreEqual(Math.Max(0,upper-estimate),p.PlusError(0)):Assert.AreEqual(Math.Max(0,estimate-lower),p.MinusError(0))
Assert.IsTrue(p.MinimumX<=0):Assert.IsTrue(p.MaximumX>=0)
x(0)=999:lo(0)=999:hi(0)=999
Assert.AreEqual(estimate,p.Estimates(0)):Assert.AreEqual(lower,p.LowerLimits(0)):Assert.AreEqual(upper,p.UpperLimits(0))
Dim values=p.YValues:values(0)=999:Assert.AreEqual(1.0,p.YValues(0))
End Sub
<TestMethod> Public Sub ExplicitNewGeometryInputContract()
Assert.ThrowsException(Of ArgumentNullException)(Sub() Create(Nothing,New Double(){1},New Double(){1}))
Assert.ThrowsException(Of ArgumentException)(Sub() Create(New Double(){},New Double(){},New Double(){}))
Assert.ThrowsException(Of ArgumentException)(Sub() Create(New Double(){1},New Double(){},New Double(){1}))
End Sub
Private Shared Function Create(x As Double(),lower As Double(),upper As Double()) As MultipleComparisonGeometry35
Return New MultipleComparisonGeometry35(x,lower,upper)
End Function
End Class
'''
ADAPTER='''Option Explicit On
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
'''
if __name__=='__main__':
    assert (ROOT/'BESHStat.Core/src/Graphics/MultipleComparisonGeometry35.vb').exists()
    p=ROOT/'BESHStat.Excel.Common.Tests/src/Legacy35.vb'
    original=p.read_bytes();(HERE/'entry-source/Legacy35-pre-capture.vb.txt').write_bytes(original)
    write(p.relative_to(ROOT),p.read_text(encoding='utf-8-sig').replace('Option Infer On','Option Infer On\nOption Compare Text'))
    write('BESHStat.Excel.Common.Tests/src/Cases35.vb',(HERE/'Cases35.vb').read_text(encoding='utf-8'))
    write('BESHStat.Excel.Common.Tests/src/Graphics35Tests.vb',COMMON)
    write('BESHStat.Core.Tests/src/Graphics/Graphics35GeometryTests.vb',CORE)
    write('BESHStatNG.Test/src/Charts/Graphics35AdapterTests.vb',ADAPTER)
    project=ROOT/'BESHStatNG.Test/BESHStatNG.Tests.vbproj';s=project.read_text(encoding='utf-8-sig');a,b=s.rsplit('</Project>',1)
    includes=['BESHStat.Excel.Common.Tests/src/'+n+'.vb' for n in ['Legacy35','LegacyLoveProjection35','Cases35','Graphics35Tests']]+['BESHStat.Core.Tests/src/Graphics/Graphics35GeometryTests.vb']
    a+='  <ItemGroup>\n'+'\n'.join('    <Compile Include="..\\'+p.replace('/','\\')+'"><Link>Charts35\\'+Path(p).name+'</Link></Compile>' for p in includes)+'\n    <Compile Include="src\\Charts\\Graphics35AdapterTests.vb"/>\n  </ItemGroup>\n'
    write(project.relative_to(ROOT),a+'</Project>'+b)
    print('Installed new35 tests; old tests unchanged; test-only OptionCompare binding explicit')
