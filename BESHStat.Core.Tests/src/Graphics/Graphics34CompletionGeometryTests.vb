Option Explicit On
Option Strict On
Imports System
Imports System.Linq
Imports System.Reflection
Imports System.Globalization
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports BESHStatNG
Imports Batch34CompletionFixtures
<TestClass>
Public Class Graphics34CompletionGeometryTests
    Public Shared Sub EqualValue(expected As Object, actual As Object)
        If expected Is Nothing Then
            Assert.IsNull(actual) : Return
        End If
        Assert.IsNotNull(actual)
        If TypeOf expected Is Double Then
            Graphics34Tests.EqualBits(New Double() {CDbl(expected)}, New Double() {CDbl(actual)})
        ElseIf TypeOf expected Is Boolean OrElse TypeOf expected Is Integer OrElse TypeOf expected Is String Then
            Assert.AreEqual(expected, actual)
        ElseIf TypeOf expected Is Array Then
            Dim e = DirectCast(expected, Array).Cast(Of Object)().ToArray(), a = DirectCast(actual, Array).Cast(Of Object)().ToArray()
            Assert.AreEqual(e.Length, a.Length)
            For i As Integer = 0 To e.Length - 1 : EqualValue(e(i), a(i)) : Next
        Else
            For Each p In expected.GetType().GetProperties()
                EqualValue(p.GetValue(expected, Nothing), actual.GetType().GetProperty(p.Name).GetValue(actual, Nothing))
                Assert.IsNull(actual.GetType().GetProperty(p.Name).GetSetMethod(), "Public geometry setters are forbidden")
            Next
        End If
    End Sub
    <DataTestMethod><DataRow("Bullet", "", "Shapes")><DataRow("Bullet", "sk-SK", "Shapes")><DataRow("Sankey", "", "Shapes")><DataRow("Sankey", "sk-SK", "Shapes")><DataRow("Bullet", "", "Extras")><DataRow("Bullet", "sk-SK", "Extras")><DataRow("Sankey", "", "Extras")><DataRow("Sankey", "sk-SK", "Extras")>
    Public Sub FrozenShapeArithmeticAndInputOwnership(family As String, culture As String, section As String)
        Dim saved = CultureInfo.CurrentCulture
        Try
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(culture)
            Dim legacy = GetType(LegacyBulletShapes34).Assembly.GetType("Batch34CompletionFixtures.Legacy" & family & section & "34", True)
            Dim current = If(family = "Bullet", GetType(BulletMeasuredGeometryCore), GetType(SankeyMeasuredGeometryCore))
            For Each oldMethod In legacy.GetMethods(BindingFlags.Public Or BindingFlags.Static)
                Dim target = current.GetMethod(family & oldMethod.Name)
                Assert.IsNotNull(target)
                For index As Integer = 0 To oldMethod.GetParameters().Length * 8
                    Dim oldArgs = Completion34Cases.Arguments(oldMethod, index), newArgs = Completion34Cases.Arguments(oldMethod, index)
                    Dim before = Completion34Cases.Snapshot(newArgs)
                    Dim oldError As String = Nothing, newError As String = Nothing
                    Dim expected = Completion34Cases.Invoke(oldMethod, oldArgs, oldError)
                    Dim actual = Completion34Cases.Invoke(target, newArgs, newError)
                    Assert.AreEqual(oldError, newError, family & oldMethod.Name & index)
                    If oldError = "None" Then
                        EqualValue(expected, actual)
                        Dim frozen = Completion34Cases.Snapshot(actual)
                        For Each arg In newArgs
                            If TypeOf arg Is Double() AndAlso DirectCast(arg, Double()).Length > 0 Then DirectCast(arg, Double())(0) = 9999
                        Next
                        Assert.AreEqual(frozen, Completion34Cases.Snapshot(actual))
                        Dim againError As String = Nothing
                        EqualValue(actual, Completion34Cases.Invoke(target, Completion34Cases.Arguments(oldMethod, index), againError))
                        Assert.AreEqual("None", againError)
                    Else
                        Assert.AreEqual(before, Completion34Cases.Snapshot(newArgs))
                    End If
                Next
            Next
        Finally
            CultureInfo.CurrentCulture = saved
        End Try
    End Sub
    <TestMethod>
    Public Sub IndependentBarAndBezierEndpointInvariants()
        Dim bar = BulletMeasuredGeometryCore.BulletActual(0.5, 20, 0.5, 100, 200)
        Assert.AreEqual(100.0R, bar.Width)
        Assert.AreEqual(10.0R, bar.Height)
        Assert.AreEqual(95.0R, bar.Top)
        Dim pair = BulletMeasuredGeometryCore.BulletPairedLabels(100, 10, 8)
        Graphics34Tests.EqualBits(New Double() {pair.Top + pair.MainHeight}, New Double() {pair.SubtitleTop})
        Assert.IsNull(BulletMeasuredGeometryCore.BulletActual(0, 20, 0.5, 100, 200))
        Dim ribbon = SankeyMeasuredGeometryCore.SankeyRibbon(0, 1, 20, 0.1, 0.4, 0.2, 0.5, 10, 410, 20, 300)
        Assert.AreEqual(20.0R, ribbon.SourceX)
        Assert.AreEqual(400.0R, ribbon.TargetX)
        Assert.AreEqual(210.0R, ribbon.C1X)
        Assert.AreEqual(210.0R, ribbon.C2X)
        Assert.AreEqual(50.0R, ribbon.SourceTop)
        Graphics34Tests.EqualBits(New Double() {20.0R + (0.4R + 0.2R) * 300.0R}, New Double() {ribbon.TargetBottom})
        Assert.IsNull(SankeyMeasuredGeometryCore.SankeyRibbon(1, 0, 20, 0, 0, 0.1, 0.5, 10, 410, 20, 300))
    End Sub
End Class
