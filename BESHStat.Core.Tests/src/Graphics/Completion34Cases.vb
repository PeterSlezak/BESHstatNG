Option Explicit On
Option Strict On
Imports System
Imports System.Linq
Imports System.Reflection
Imports System.Globalization
Namespace Global.Batch34CompletionFixtures
Public Module Completion34Cases
    Public Function Arguments(method As MethodInfo, index As Integer) As Object()
        Dim parameters = method.GetParameters()
        Dim result(parameters.Length - 1) As Object
        For i As Integer = 0 To parameters.Length - 1
            Dim p = parameters(i), name = p.Name.ToLowerInvariant()
            If p.ParameterType Is GetType(Double()) Then
                result(i) = New Double() {10, 210, 410}
            ElseIf p.ParameterType Is GetType(Integer) Then
                result(i) = If(name.Contains("stageindex"), 1, 3)
            Else
                Dim value As Double = 0.5R
                If name.Contains("width") Then value = 100
                If name.Contains("height") Then value = If(name.Contains("fraction"), 0.5R, 20.0R)
                If name = "stageleft" OrElse name = "plotleft" Then value = 10
                If name = "stageright" Then value = 410
                If name = "plottop" Then value = 20
                If name = "plotheight" Then value = 300
                If name.Contains("centrey") Then value = 150
                If name.Contains("font") Then value = 10
                If name.Contains("gap") Then value = 4
                If name = "targetnodex" Then value = 0.9R
                If name = "sourcenodex" Then value = 0.1R
                result(i) = value
            End If
        Next
        If index > 0 Then
            Dim slot As Integer = (index - 1) Mod parameters.Length
            Dim mode As Integer = ((index - 1) \ parameters.Length) Mod 8
            Dim kind = parameters(slot).ParameterType
            If kind Is GetType(Double()) Then
                result(slot) = If(mode = 0, Nothing, If(mode = 1, New Double() {}, New Double() {0, 0, 1}))
            ElseIf kind Is GetType(Integer) Then
                result(slot) = New Integer() {-2, -1, 0, 1, 2, 5, Integer.MinValue, Integer.MaxValue}(mode)
            Else
                result(slot) = New Double() {-0.0R, 0.0R, -2.0R, 2.0R, Double.NaN, Double.PositiveInfinity, Double.NegativeInfinity, 1.0E-150R}(mode)
            End If
        End If
        Return result
    End Function
    Public Function Snapshot(value As Object) As String
        If value Is Nothing Then Return "Nothing"
        If TypeOf value Is Double Then Return BitConverter.DoubleToInt64Bits(CDbl(value)).ToString("X16", CultureInfo.InvariantCulture)
        If TypeOf value Is Boolean OrElse TypeOf value Is Integer Then Return Convert.ToString(value, CultureInfo.InvariantCulture)
        If TypeOf value Is String Then Return CStr(value)
        If TypeOf value Is Array Then Return "[" & String.Join("|", DirectCast(value, Array).Cast(Of Object)().Select(Function(v) Snapshot(v))) & "]"
        Return String.Join("|", value.GetType().GetProperties().OrderBy(Function(p) p.Name, StringComparer.Ordinal).Select(Function(p) p.Name & "=" & Snapshot(p.GetValue(value, Nothing))))
    End Function
    Public Function Invoke(method As MethodInfo, args As Object(), ByRef exception As String) As Object
        exception = "None"
        Try
            Return method.Invoke(Nothing, args)
        Catch ex As TargetInvocationException
            Dim arg = TryCast(ex.InnerException, ArgumentException)
            exception = ex.InnerException.GetType().FullName & ":" & If(arg Is Nothing, "", arg.ParamName)
            Return Nothing
        End Try
    End Function
End Module
End Namespace
