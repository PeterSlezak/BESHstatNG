Option Explicit On
Option Strict On
Imports System
Imports System.Reflection
Imports BESHStatNG
Namespace Global.Batch34PolicyFixtures
Public Module CompletionPolicyCases34
    Public Function Arguments(method As MethodInfo, index As Integer) As Object()
        Dim ps = method.GetParameters(), result(ps.Length - 1) As Object
        For i As Integer = 0 To ps.Length - 1
            Dim t = ps(i).ParameterType
            If t Is GetType(Boolean) Then
                result(i) = (index And (1 << i)) <> 0
            ElseIf t Is GetType(Integer) Then
                result(i) = New Integer() {0, 1, 2, -1, 999}(index Mod 5)
            ElseIf t Is GetType(Double) Then
                result(i) = New Double() {100, 40, 10, -0.0R, 0, -2, Double.NaN, Double.PositiveInfinity, Double.NegativeInfinity}(index Mod 9)
            ElseIf t Is GetType(Integer()) Then
                result(i) = If(index Mod 5 = 0, Nothing, If(index Mod 5 = 1, New Integer(){}, New Integer(){3,1,3,0}))
            ElseIf t Is GetType(SankeyNode()) Then
                Dim data = SankeyPlotCore.ComputeStaged(New Object(,) {{"A", "B"}, {"B", "A"}}, New Double() {1, 2})
                Dim nodes = data.Nodes
                result(i) = If(index Mod 5 = 0, Nothing, If(index Mod 5 = 1, New SankeyNode(){}, New SankeyNode(){nodes(2),nodes(0),nodes(0),nodes(1)}))
            Else
                Throw New InvalidOperationException(t.FullName)
            End If
        Next
        Return result
    End Function
End Module
End Namespace
