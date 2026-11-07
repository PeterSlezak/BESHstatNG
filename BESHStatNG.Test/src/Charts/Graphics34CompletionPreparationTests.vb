Option Explicit On
Option Strict On
Imports System
Imports System.Linq
Imports System.Globalization
Imports System.Reflection
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports BESHStatNG
Imports Batch34PreparationFixtures
<TestClass>
Public Class Graphics34CompletionPreparationTests
    Private Shared Function Host(family As String) As Type
        Dim names As New Collections.Generic.Dictionary(Of String, String) From {{"Bullet", "BulletChartExcel"}, {"Sankey", "SankeyPlotExcel"}, {"Cdf", "CumulativeDistributionPlotExcel"}, {"Dumbbell", "DumbbellPlotExcel"}, {"Ladder", "LadderPlotExcel"}, {"Hull", "ConvexHullPlotExcel"}, {"Polar", "PolarPlotExcel"}, {"Spc", "graphics.SpcControlChartExcel"}}
        Return GetType(CumulativeDistributionPlotExcel).Assembly.GetType("BESHStatNG." & names(family), True)
    End Function
    Private Shared Function Invoke(method As MethodInfo, args As Object(), ByRef errorName As String) As Object
        errorName = "None"
        Try
            Return method.Invoke(Nothing, args)
        Catch ex As TargetInvocationException
            Dim arg = TryCast(ex.InnerException, ArgumentException)
            errorName = ex.InnerException.GetType().FullName & ":" & If(arg Is Nothing, "", arg.ParamName)
            Return Nothing
        End Try
    End Function
    Public Shared Sub EqualResult(expected As Object, actual As Object)
        If expected Is Nothing Then
            Assert.IsNull(actual) : Return
        End If
        Assert.IsNotNull(actual)
        If TypeOf expected Is String OrElse TypeOf expected Is Boolean OrElse TypeOf expected Is Integer Then
            Assert.AreEqual(expected, actual)
        ElseIf TypeOf expected Is Double Then
            Graphics34Tests.EqualBits(New Double() {CDbl(expected)}, New Double() {CDbl(actual)})
        Else
            Dim fields = expected.GetType().GetFields(BindingFlags.Public Or BindingFlags.Instance)
            Assert.IsTrue(fields.Length > 0, "No scalar projection fields compared")
            For Each f In fields
                Dim actualField = actual.GetType().GetField(f.Name, BindingFlags.Public Or BindingFlags.NonPublic Or BindingFlags.Instance)
                Dim obtained = If(actualField IsNot Nothing, actualField.GetValue(actual), actual.GetType().GetProperty(f.Name).GetValue(actual, Nothing))
                EqualResult(f.GetValue(expected), obtained)
            Next
        End If
    End Sub
    <DataTestMethod><DataRow("Bullet", "")><DataRow("Bullet", "sk-SK")><DataRow("Sankey", "")><DataRow("Sankey", "sk-SK")><DataRow("Cdf", "")><DataRow("Cdf", "sk-SK")><DataRow("Dumbbell", "")><DataRow("Dumbbell", "sk-SK")><DataRow("Ladder", "")><DataRow("Ladder", "sk-SK")><DataRow("Hull", "")><DataRow("Hull", "sk-SK")><DataRow("Polar", "")><DataRow("Polar", "sk-SK")><DataRow("Spc", "")><DataRow("Spc", "sk-SK")>
    Public Sub ActualAdaptersMatchFrozenAxisTextMembers(family As String, culture As String)
        Dim saved = CultureInfo.CurrentCulture
        Try
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(culture)
            Dim legacy = GetType(CompletionPreparationCases34).Assembly.GetType("Batch34PreparationFixtures.Legacy" & family & "Preparation34", True)
            Dim adapter = Host(family)
            For Each method In legacy.GetMethods(BindingFlags.Public Or BindingFlags.Static).Where(Function(m) Not m.Name.StartsWith("IsFinite", StringComparison.Ordinal))
                Dim owner = If(family = "Spc" AndAlso (method.Name = "NormalizeText" OrElse method.Name = "NormalizeNumberFormat"), adapter.Assembly.GetType("BESHStatNG.graphics.SpcControlChartAppearanceOptions", True), adapter)
                Dim current = owner.GetMethod(method.Name, BindingFlags.Public Or BindingFlags.NonPublic Or BindingFlags.Static)
                Assert.IsNotNull(current, family & method.Name)
                For index As Integer = 0 To 159
                    Dim expectedError As String = Nothing, actualError As String = Nothing
                    Dim expected = Invoke(method, CompletionPreparationCases34.Arguments(method, index), expectedError)
                    Dim actual = Invoke(current, CompletionPreparationCases34.Arguments(method, index), actualError)
                    Assert.AreEqual(expectedError, actualError, family & method.Name & index)
                    If expectedError = "None" Then EqualResult(expected, actual)
                Next
            Next
        Finally
            CultureInfo.CurrentCulture = saved
        End Try
    End Sub
    <DataTestMethod><DataRow("Bullet")><DataRow("Sankey")><DataRow("Cdf")><DataRow("Dumbbell")><DataRow("Ladder")><DataRow("Hull")><DataRow("Polar")><DataRow("Spc")>
    Public Sub CurrentAppearanceIsSnapshottedPerCall(family As String)
        Dim hostType = Host(family), snapshot = hostType.GetMethod("Snapshot" & family & "Settings34", BindingFlags.Public Or BindingFlags.NonPublic Or BindingFlags.Static)
        If snapshot Is Nothing Then Return
        Dim appearance = Activator.CreateInstance(snapshot.GetParameters()(0).ParameterType)
        Dim before = snapshot.Invoke(Nothing, New Object() {appearance})
        For Each p In before.GetType().GetProperties()
            Assert.IsNull(p.GetSetMethod(), "Public setting setters are forbidden")
            Dim original = p.GetValue(before, Nothing), native = appearance.GetType().GetProperty(p.Name)
            If native.PropertyType Is GetType(String) Then native.SetValue(appearance, "changed", Nothing)
            If native.PropertyType Is GetType(Boolean) Then native.SetValue(appearance, Not CBool(original), Nothing)
            Assert.AreEqual(original, p.GetValue(before, Nothing), p.Name)
        Next
        Dim after = snapshot.Invoke(Nothing, New Object() {appearance})
        Assert.AreNotSame(before, after)
    End Sub
End Class
