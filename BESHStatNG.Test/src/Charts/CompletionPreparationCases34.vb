Option Explicit On
Option Strict On
Imports System
Imports System.Linq
Imports System.Reflection
Imports BESHStatNG
Imports BESHStatNG.StatisticalProcessControl
Namespace Global.Batch34PreparationFixtures
Public Module CompletionPreparationCases34
    Private Function Cdf() As CumulativeDistributionPlotResult
        Return CumulativeDistributionPlotCore.Compute(New Double() {0, 1, 2, 3}, "A")
    End Function
    Private Function Dumbbell() As DumbbellPlotResult
        Return DumbbellPlotCore.Compute(New Object() {"A", "B", "C", "D"}, New Double() {1, 2, 3, 4}, New Double() {2, 1, 4, 3})
    End Function
    Private Function Ladder() As LadderPlotResult
        Return LadderPlotCore.Compute(New Double() {1, 2, 3, 4}, New Double() {2, 1, 4, 3})
    End Function
    Private Function Polar() As PolarPlotResult
        Return New PolarPlotCore(New Double() {1, 2, 3}, New Double() {0, 90, 180}, Nothing, New Object() {"A", "B", "A"}).Compute()
    End Function
    Private Function Hull() As ConvexHullPlotResult
        Return ConvexHullPlotCore.Compute(New Double() {0, 1, 0, 2}, New Double() {0, 0, 1, 2}, Nothing, New Object() {"A", "A", "A", "B"})
    End Function
    Private Function Sankey() As SankeyPlotResult
        Return SankeyPlotCore.ComputeStaged(New Object(,) {{"A", "X", "Z"}, {"B", "X", "Y"}}, New Double() {1, 3})
    End Function
    Private Function Spc() As SpcFitResult
        Dim layout = DirectCast([Enum].GetValues(GetType(SpcDataLayout)).GetValue(0), SpcDataLayout)
        Dim kind = DirectCast([Enum].GetValues(GetType(SpcChartType)).GetValue(0), SpcChartType)
        Dim data As New SpcInputData(layout, measurements:=New Double(,) {{1}, {2}, {3}, {4}})
        Dim request As New SpcFitRequest(kind, data, chartTitle:="Chart", valueAxisTitle:="Axis")
        Dim point As New SpcPointResult(0, 1, 2, 0, 3)
        Dim panel As New SpcPanelResult(SpcPanelType.IndividualValue, "Panel", New SpcPointResult() {point}, "Value")
        Return New SpcFitResult(request, New SpcPanelResult() {panel})
    End Function
    Public Function Arguments(method As MethodInfo, index As Integer) As Object()
        Dim parameters = method.GetParameters(), result(parameters.Length - 1) As Object
        For i As Integer = 0 To parameters.Length - 1
            Dim p = parameters(i), t = p.ParameterType, n = p.Name.ToLowerInvariant()
            Select Case t.Name
                Case "CumulativeDistributionPlotResult[]" : result(i) = New CumulativeDistributionPlotResult() {Cdf(), Cdf()}
                Case "CumulativeDistributionPlotResult" : result(i) = Cdf()
                Case "CumulativeDistributionPercentile"
                    result(i) = Activator.CreateInstance(t, BindingFlags.Public Or BindingFlags.NonPublic Or BindingFlags.Instance, Nothing, New Object() {25.0R, 0.25R, 1.5R}, Globalization.CultureInfo.InvariantCulture)
                Case "DumbbellPlotResult" : result(i) = Dumbbell()
                Case "LadderPlotResult" : result(i) = Ladder()
                Case "LadderPlotObservation" : result(i) = Ladder().Observations(0)
                Case "PolarPlotResult" : result(i) = Polar()
                Case "PolarPlotGroupSeries" : result(i) = Polar().GroupSeries(0)
                Case "ConvexHullPlotGroup" : result(i) = Hull().Groups(0)
                Case "SankeyPlotResult" : result(i) = Sankey()
                Case "SankeyNode" : result(i) = Sankey().Nodes(0)
                Case "SpcFitResult" : result(i) = Spc()
                Case "SpcPanelResult" : result(i) = Spc().Panels(0)
                Case "BulletChartItem" : result(i) = BulletChartCore.ComputeBasic(New Object() {"A"}, New Double() {40}, New Double() {60}, New Double(,) {{50, 80, 100}}).Items(0)
                Case Else
                    If t.Name.EndsWith("Appearance", StringComparison.Ordinal) OrElse t.Name = "SpcControlChartAppearanceOptions" Then
                        result(i) = Activator.CreateInstance(t)
                        Dim fields = t.GetProperties().Where(Function(x) x.CanWrite AndAlso (x.PropertyType Is GetType(String) OrElse x.PropertyType Is GetType(Boolean) OrElse x.PropertyType Is GetType(Double) OrElse x.PropertyType.IsEnum OrElse Nullable.GetUnderlyingType(x.PropertyType) Is GetType(Double))).OrderBy(Function(x) x.Name, StringComparer.Ordinal).ToArray()
                        If index > 0 AndAlso fields.Length > 0 Then
                            Dim field = fields((index - 1) Mod fields.Length), mode = ((index - 1) \ fields.Length) Mod 6, ft = field.PropertyType
                            If ft Is GetType(String) Then
                                field.SetValue(result(i), New String() {Nothing, "", " ", "0.00", "bad[", "Custom"}(mode), Nothing)
                            ElseIf ft Is GetType(Boolean) Then
                                field.SetValue(result(i), mode Mod 2 = 0, Nothing)
                            ElseIf ft.IsEnum Then
                                Dim values = [Enum].GetValues(ft)
                                field.SetValue(result(i), If(mode = 5, [Enum].ToObject(ft, 999), values.GetValue(mode Mod values.Length)), Nothing)
                            Else
                                Dim value As Object = New Double() {-0.0R, 0.0R, -1.0R, 0.5R, Double.NaN, Double.PositiveInfinity}(mode)
                                If Nullable.GetUnderlyingType(ft) IsNot Nothing AndAlso mode = 0 Then value = Nothing
                                field.SetValue(result(i), value, Nothing)
                            End If
                        End If
                    ElseIf t.IsEnum Then
                        Dim values = [Enum].GetValues(t)
                        result(i) = If(index Mod 7 = 6, [Enum].ToObject(t, 999), values.GetValue(index Mod values.Length))
                    ElseIf t Is GetType(Integer) Then
                        result(i) = New Integer() {0, 1, 2, -1, 5}(index Mod 5)
                    ElseIf t Is GetType(Boolean) Then
                        result(i) = index Mod 2 = 0
                    ElseIf t Is GetType(String) Then
                        result(i) = New String() {"0.###", "", " ", Nothing, "0.00", "bad[", "name"}(index Mod 7)
                    ElseIf t Is GetType(Double) Then
                        result(i) = New Double() {0.2R, -0.0R, 0.0R, -2, Double.NaN, Double.PositiveInfinity, Double.NegativeInfinity, 1.0E-150R}(index Mod 8)
                    ElseIf Nullable.GetUnderlyingType(t) Is GetType(Double) Then
                        result(i) = If(index Mod 4 = 0, Nothing, CType(CDbl(index) / 10, Object))
                    Else
                        Throw New InvalidOperationException("Unspecified fixture input: " & t.Name)
                    End If
            End Select
            If index > 0 AndAlso index Mod 17 = 0 AndAlso Not t.IsValueType AndAlso Not t.Name.EndsWith("Appearance", StringComparison.Ordinal) Then result(i) = Nothing
        Next
        Return result
    End Function
    Public Function Snapshot(value As Object) As String
        If value Is Nothing Then Return "Nothing"
        If TypeOf value Is Double Then Return BitConverter.DoubleToInt64Bits(CDbl(value)).ToString("X16", Globalization.CultureInfo.InvariantCulture)
        If TypeOf value Is String Then Return CStr(value)
        If TypeOf value Is Boolean OrElse TypeOf value Is Integer Then Return Convert.ToString(value, Globalization.CultureInfo.InvariantCulture)
        Return String.Join("|", value.GetType().GetFields(BindingFlags.Public Or BindingFlags.NonPublic Or BindingFlags.Instance).OrderBy(Function(f) f.Name, StringComparer.Ordinal).Select(Function(f) f.Name & "=" & Snapshot(f.GetValue(value))))
    End Function
End Module
End Namespace
