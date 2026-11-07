Option Explicit On
Option Strict On
Imports System
Imports System.Linq
Imports System.Reflection
Imports BESHStatNG
Namespace Global.Batch34SeriesFixtures
Public Module CompletionSeriesCases34
    Public Function Arguments(method As MethodInfo, index As Integer) As Object()
        Dim ps = method.GetParameters(), result(ps.Length - 1) As Object
        For i As Integer = 0 To ps.Length - 1
            Dim t = ps(i).ParameterType
            Select Case t.Name
                Case "DumbbellPlotCategory[]", "DumbbellAuxiliaryObservation[]"
                    Dim data = DumbbellPlotCore.Compute(New Object() {"A", "B", "C"}, New Double() {1, 2, 3}, New Double() {3, 1, 2}, Nothing, New Object() {"B", "A"}, New Double() {1.2, 2.5})
                    result(i) = If(t.Name = "DumbbellPlotCategory[]", CType(data.Categories, Object), CType(data.AuxiliaryObservations, Object))
                Case "LadderPlotObservation[]", "LadderPlotObservation"
                    Dim data = LadderPlotCore.Compute(New Double() {1, 2, 2}, New Double() {3, 1, 2})
                    result(i) = If(t.Name = "LadderPlotObservation[]", CType(data.Observations, Object), CType(data.Observations(0), Object))
                Case "Double[]" : result(i) = New Double() {-0.0, 1, 1, 4}
                Case "MultipleComparisonPlotData"
                    result(i) = New parametric.MultipleComparisonPlotData With {.Estimates = If(index Mod 4 = 0, Nothing, Enumerable.Range(0, index Mod 10).Select(Function(k) CDbl(k)).ToArray())}
                Case "String" : result(i) = New String() {Nothing, "", "0.00", "Q", "0.###", "name"}(index Mod 6)
                Case "Boolean" : result(i) = index Mod 2 = 0
                Case "Double" : result(i) = New Double() {1, -0.0, 0, -1, 100, Double.NaN, Double.PositiveInfinity, Double.NegativeInfinity}(index Mod 8)
                Case Else : Throw New InvalidOperationException(t.FullName)
            End Select
            If index > 0 AndAlso index Mod 17 = 0 AndAlso Not t.IsValueType Then result(i) = Nothing
            If t.IsArray AndAlso index > 0 AndAlso index Mod 19 = 0 Then result(i) = Array.CreateInstance(t.GetElementType(), 0)
            If t.IsArray AndAlso Not t.GetElementType().IsValueType AndAlso index > 0 AndAlso index Mod 23 = 0 Then result(i) = Array.CreateInstance(t.GetElementType(), 1)
        Next
        Return result
    End Function
End Module
End Namespace
