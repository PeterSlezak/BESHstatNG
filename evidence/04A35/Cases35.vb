Option Explicit On
Option Strict On
Option Infer On
Imports System
Imports System.Linq
Imports System.Collections.Generic
Imports System.Globalization
Imports BESHStatNG
Namespace Global.Legacy35
Public Module Cases35
Public Function Numbers(index As Integer) As Double()
Return New Double(){-0.0,0,1,-1,2,Double.NaN,Double.PositiveInfinity,Double.NegativeInfinity,1.0E-150,1.0E150}.Skip(index Mod 10).Take(3).ToArray()
End Function
Public Function Data(index As Integer) As parametric.MultipleComparisonPlotData
If index=0 Then Return Nothing
Dim n=index Mod 6
Dim result As New parametric.MultipleComparisonPlotData With {.ProcedureName=New String(){Nothing,"Tukey-Kramer","Bonferroni","Games-Howell","Other","tukey-kramer","Žluťoučký"}(index Mod 7),.Alpha=New Double(){0.05,0.1,-0.0,1,-1,Double.NaN,Double.PositiveInfinity}(index Mod 7),.Estimates=Enumerable.Range(0,n).Select(Function(i) Numbers(index+i)(0)).ToArray(),.LowerLimits=Enumerable.Range(0,n).Select(Function(i) Numbers(index+i+2)(0)).ToArray(),.UpperLimits=Enumerable.Range(0,n).Select(Function(i) Numbers(index+i+4)(0)).ToArray(),.Labels=Enumerable.Range(0,n).Select(Function(i) If(i Mod 2=0,"α – beta",CStr(Nothing))).ToArray()}
If index Mod 11=0 Then result.Estimates=Nothing
If index Mod 13=0 Then result.Labels=Nothing
If index Mod 17=0 Then result.LowerLimits=Nothing
If index Mod 19=0 Then result.UpperLimits=Nothing
If index Mod 23=0 Then result.LowerLimits=New Double(){}
Return result
End Function
Public Function Rows(index As Integer) As IList(Of CausalInference.PsmLovePlotRow)
If index=0 Then Return Nothing
Dim result As New List(Of CausalInference.PsmLovePlotRow)
For i As Integer=0 To index Mod 7-1
If i=2 AndAlso index Mod 3=0 Then
result.Add(Nothing):Continue For
End If
Dim values=Numbers(index+i)
result.Add(New CausalInference.PsmLovePlotRow With {.VariableName=New String(){Nothing," ","a","A","B","Ž","a"}((i+index) Mod 7),.SmdBefore=values(0),.SmdAfterMatching=If(values.Length>1,values(1),Double.NaN),.SmdAfterWeighting=If(values.Length>2,values(2),Double.NaN),.Threshold=New Double(){Double.NaN,0,-0.0,Double.PositiveInfinity,0.1,-1}(index Mod 6),.Flag="flag"})
Next
Return result
End Function
Public Function Snapshot(value As Object) As String
If value Is Nothing Then Return "Nothing"
If TypeOf value Is Double Then Return BitConverter.DoubleToInt64Bits(CDbl(value)).ToString("X16",CultureInfo.InvariantCulture)
If TypeOf value Is Array Then
Dim a=DirectCast(value,Array)
Return "[" & String.Join("x",Enumerable.Range(0,a.Rank).Select(Function(i) a.GetLength(i).ToString(CultureInfo.InvariantCulture))) & ":" & String.Join("|",a.Cast(Of Object).Select(Function(v) Snapshot(v))) & "]"
End If
Return Convert.ToString(value,CultureInfo.InvariantCulture)
End Function
End Module
End Namespace
