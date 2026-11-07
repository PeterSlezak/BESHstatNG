Option Explicit On
Option Strict On
Imports System
Imports System.Linq
Imports System.Collections.Generic
Imports System.Globalization
Imports System.Reflection
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports BESHStatNG
Imports BESHStatNG.ExcelCommon
Imports BESHStatNG.TestFixtures34

<TestClass>
Public Class Graphics34AdapterTests
    Private Shared Function InvokeStatic(t As Type, name As String, args As Object()) As Object
        Try
            Return t.GetMethod(name, BindingFlags.Static Or BindingFlags.Public Or BindingFlags.NonPublic).Invoke(Nothing, args)
        Catch ex As TargetInvocationException
            System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(ex.InnerException).Throw()
            Throw
        End Try
    End Function
    <DataTestMethod><DataRow(0)><DataRow(1)><DataRow(2)><DataRow(3)><DataRow(4)><DataRow(5)><DataRow(6)><DataRow(7)><DataRow(8)><DataRow(9)>
    Public Sub ActualNumericWrappersPreserveLegacyExceptionsAndArrays(index As Integer)
        For Each gap In New Double() {-1, 0, 0.5, 20, Double.NaN, Double.PositiveInfinity}
            Dim expected As Double() = Nothing, actual As Double() = Nothing
            Dim oldError = Graphics34Tests.ErrorIdentity(Sub() expected = LegacySpecialty34.SpreadLabelPositions(Graphics34Tests.Inputs(index), -2, 4, gap))
            Dim newError = Graphics34Tests.ErrorIdentity(Sub() actual = DirectCast(InvokeStatic(GetType(LadderPlotExcel), "SpreadLabelPositions", New Object() {Graphics34Tests.Inputs(index), -2.0R, 4.0R, gap}), Double()))
            Assert.AreEqual(oldError, newError)
            If oldError = "None" Then Graphics34Tests.EqualBits(expected, actual)
        Next
        For mode As Integer = 0 To 3
            Dim x = Graphics34Tests.Inputs(index), y = If(x Is Nothing, Nothing, x.Reverse().ToArray())
            If mode = 3 Then y = New Double() {2, 3}
            Dim ex As Double() = Nothing, ey As Double() = Nothing
            Dim minimum As Double = If(mode = 0, -10.0R, 0.0R), maximum As Double = If(mode = 1, 10.0R, 2.0R)
            LegacySpecialty34.ExtendStepToAxisBounds(x, y, minimum, maximum, ex, ey)
            Dim args As Object() = {x, y, minimum, maximum, Nothing, Nothing}
            InvokeStatic(GetType(CumulativeDistributionPlotExcel), "ExtendStepToAxisBounds", args)
            Graphics34Tests.EqualBits(ex, DirectCast(args(4), Double()))
            Graphics34Tests.EqualBits(ey, DirectCast(args(5), Double()))
            If Object.ReferenceEquals(ex, x) Then Assert.AreSame(x, args(4))
            If Object.ReferenceEquals(ey, y) Then Assert.AreSame(y, args(5))
        Next
    End Sub

    <DataTestMethod><DataRow("")><DataRow("sk-SK")>
    Public Sub MeasuredLayoutsMatchExecutedPreEditSlices(culture As String)
        Dim saved = CultureInfo.CurrentCulture
        Try
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(culture)
            Dim bullet = BulletChartCore.ComputeBasic(New Object() {"A", "B"}, New Double() {40, 70}, New Double() {60, 80}, New Double(,) {{50, 80, 100}, {50, 80, 100}})
            Dim sankey = SankeyPlotCore.ComputeStaged(New Object(,) {{"A", "X", "Z"}, {"B", "X", "Y"}}, New Double() {1, 3})
            For Each family In New String() {"Bullet", "Sankey"}
                Dim appearanceType = If(family = "Bullet", GetType(BulletChartAppearance), GetType(SankeyPlotAppearance))
                Dim fixtureType = If(family = "Bullet", GetType(LegacyBulletLayout34), GetType(LegacySankeyLayout34))
                Dim coreType = If(family = "Bullet", GetType(BulletMeasuredGeometryCore), GetType(SankeyMeasuredGeometryCore))
                For mask As Integer = 0 To 31
                    For Each width In New Double() {80, 620}
                        For Each height In New Double() {40, 420}
                            Dim appearance = Activator.CreateInstance(appearanceType)
                            Dim toggles = If(family = "Bullet", New String() {"ShowMeasureLabels", "ShowSubtitles", "ShowScaleLabels", "ShowScaleTickMarks", "ShowTargetValueLabels"}, New String() {"ShowLegend", "ShowNodeLabels", "ShowStageLabels"})
                            For i As Integer = 0 To toggles.Length - 1
                                appearanceType.GetProperty(toggles(i)).SetValue(appearance, (mask And (1 << i)) <> 0, Nothing)
                            Next
                            appearanceType.GetProperty("ChartTitle").SetValue(appearance, If((mask And 16) <> 0, "Title", ""), Nothing)
                            Dim expected As Object = Nothing, actual As Object = Nothing
                            Dim input As Object()
                            If family = "Bullet" Then
                                input = New Object() {width, height, bullet, appearance}
                            Else
                                Dim legends As New List(Of LegacySankeyLayout34.LegendItem)
                                For i As Integer = 1 To 7 : legends.Add(New LegacySankeyLayout34.LegendItem()) : Next
                                input = New Object() {width, height, sankey, appearance, legends}
                            End If
                            Dim oldError = Graphics34Tests.ErrorIdentity(Sub() expected = InvokeStatic(fixtureType, "BuildDrawingLayout", input))
                            Dim method = coreType.GetMethod("ComputeLayout")
                            Dim args As New List(Of Object)
                            For Each parameter In method.GetParameters()
                                Dim name = parameter.Name
                                Select Case name.ToLowerInvariant()
                                    Case "chartwidth" : args.Add(width)
                                    Case "chartheight" : args.Add(height)
                                    Case "itemcount" : args.Add(bullet.ItemCount)
                                    Case "hassubtitles" : args.Add(bullet.HasSubtitles)
                                    Case "stagecount" : args.Add(sankey.StageCount)
                                    Case "legenditemcount" : args.Add(7)
                                    Case Else
                                        If name.StartsWith("input", StringComparison.Ordinal) Then name = name.Substring(5)
                                        args.Add(appearanceType.GetProperty(name).GetValue(appearance, Nothing))
                                End Select
                            Next
                            Dim newError = Graphics34Tests.ErrorIdentity(Sub() actual = InvokeStatic(coreType, "ComputeLayout", args.ToArray()))
                            Assert.AreEqual(oldError, newError, family & " mask=" & mask)
                            If oldError <> "None" Then Continue For
                            For Each field In expected.GetType().GetFields()
                                Dim e = field.GetValue(expected), a = actual.GetType().GetProperty(field.Name).GetValue(actual, Nothing)
                                If TypeOf e Is Double Then
                                    Graphics34Tests.EqualBits(New Double() {CDbl(e)}, New Double() {CDbl(a)})
                                Else
                                    Assert.AreEqual(e, a, field.Name)
                                End If
                            Next
                            If family = "Bullet" Then CompareBulletRows(DirectCast(expected, LegacyBulletLayout34.DrawingLayout), DirectCast(actual, BulletDrawingLayout), DirectCast(appearance, BulletChartAppearance))
                        Next
                    Next
                Next
            Next
        Finally
            CultureInfo.CurrentCulture = saved
        End Try
    End Sub

    Private Shared Sub CompareBulletRows(expectedLayout As LegacyBulletLayout34.DrawingLayout, layout As BulletDrawingLayout, appearance As BulletChartAppearance)
        For row As Integer = 0 To 2
            Dim expected = LegacyBulletLayout34.BuildRowLayout(expectedLayout, appearance, row)
            Dim actual = BulletMeasuredGeometryCore.ComputeRow(layout, row, appearance.ShowScaleLabels, appearance.ShowScaleTickMarks, appearance.ScaleLabelGap, appearance.ScaleTickLength, appearance.ScaleLabelHeight, appearance.ShowTargetValueLabels, appearance.ValueLabelHeight, appearance.ValueLabelGap)
            For Each field In expected.GetType().GetFields()
                Graphics34Tests.EqualBits(New Double() {CDbl(field.GetValue(expected))}, New Double() {CDbl(actual.GetType().GetProperty(field.Name).GetValue(actual, Nothing))})
            Next
        Next
    End Sub

    <TestMethod>
    Public Sub ActualTimingAndLoveAdmissionDelegates()
        For Each milliseconds In New Integer() {Integer.MinValue, -1, 0, 5, 15, 25, Integer.MaxValue}
            Assert.AreEqual(LegacySpecialty34.MsToCentiseconds(milliseconds), CInt(InvokeStatic(GetType(GifAnimator), "MsToCentiseconds", New Object() {milliseconds})))
        Next
        For i As Integer = 0 To 9
            Dim values = Graphics34Tests.Inputs(i)
            Assert.AreEqual(LegacyReference34.HasAnyFinite(values), CBool(InvokeStatic(GetType(graphics.PsmLovePlotExcel), "HasAnyFinite", New Object() {values})))
        Next
    End Sub
End Class
