Option Explicit On
Option Strict On
Imports System
Imports System.Linq
Imports System.Reflection
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports BESHStatNG
Imports Batch32Fixtures

<TestClass>
Public Class Graphics32AdapterTests
    Private Shared Function CreateBox(data As Object, labels() As String) As graphics.BoxPlot
        Return DirectCast(Activator.CreateInstance(GetType(graphics.BoxPlot), BindingFlags.Instance Or BindingFlags.Public Or BindingFlags.NonPublic,
            Nothing, New Object() {data, labels, Nothing, ""}, Nothing), graphics.BoxPlot)
    End Function
    Private Shared Function Field(Of T)(instance As Object, name As String) As T
        Return DirectCast(instance.GetType().GetField(name, BindingFlags.Instance Or BindingFlags.NonPublic).GetValue(instance), T)
    End Function
    <TestMethod>
    Public Sub ActualBoxBothConstructorsAliasesRepeatedAndFailureState()
        Dim groups As Double()() = {New Double() {4, 1, 4, 2, 1}, New Double() {-5, -1, 0, 3, 6}}
        Dim labels = {" A ", " A "}, before = groups.Select(Function(x) DirectCast(x.Clone(), Double())).ToArray()
        Dim actual = CreateBox(groups, labels), old As New LegacyBox32(groups)
        For repeat As Integer = 0 To 1
            old.Calculate() : old.CalcForPlotting() : actual.Calculate() : actual.CalcForPlotting()
            Graphics31Evidence.EqualBits(old.pQ1, actual.Q1) : Graphics31Evidence.EqualBits(old.pQ3, actual.Q3)
            Graphics31Evidence.EqualBits(old.pWhiskerQ1, Field(Of Double())(actual, "pWhiskerQ1"))
            Graphics31Evidence.EqualBits(old.pWhiskerQ3, Field(Of Double())(actual, "pWhiskerQ3"))
            CollectionAssert.AreEqual(old.pArNOutliersBig, actual.BigOutliers)
            Assert.IsNotNull(actual.wrapResults())
            Assert.AreSame(actual.Q1, actual.Q1) 'Legacy return-vector alias stays supported.
            Dim q = actual.Q1 : q(0) = 77 : Assert.AreEqual(77.0, actual.Q1(0))
            groups(0)(0) += 1
        Next
        Assert.AreEqual(4, Field(Of Microsoft.VisualBasic.Collection)(actual, "pDSCollection").Count)
        Dim matrix(,) As Double = {{4, -2}, {1, 0}, {3, 1}}, m = CreateBox(matrix, {"", "duplicate"})
        m.Calculate() : m.CalcForPlotting()
        Dim expected = BoxPlotDataCore.Compute(matrix, {"", "duplicate"})
        Graphics31Evidence.EqualBits(expected.Groups.Select(Function(g) g.Aggregate.LQuartile).ToArray(), m.Q1)
        Dim input = {4.0, 1, 3}, copy = DirectCast(input.Clone(), Double()), b = CreateBox(New Double()() {input}, {"a"})
        b.Calculate() : b.CalcForPlotting() : Graphics31Evidence.EqualBits(copy, input)
        Dim invalid = CreateBox(New Double()() {New Double() {}}, {"a"})
        Dim invalidLegacy As New LegacyBox32({New Double() {}})
        Assert.AreEqual(Graphics30Tests.ExceptionType(Sub() invalidLegacy.Calculate()), Graphics30Tests.ExceptionType(Sub() invalid.Calculate()))
        Assert.ThrowsException(Of InvalidOperationException)(Sub() b.AddBoxPlot()) 'No worksheet/native object.
    End Sub
    <TestMethod>
    Public Sub ActualHistogramTogglesPayloadAndReturnedBinAlias()
        Dim values = {8.0, 1, 4, 2, 9, 3, 6, 5}, before = Graphics31Evidence.Bits(values)
        Dim actual As New graphics.Histogram(values), old As New LegacyHistogram32(values)
        For Each overlay In {True, False, True}
            For Each box In {True, False}
                Dim expected = old.Compute(overlay, "(Sturges)", box), obtained = actual.compute(overlay, "(Sturges)", box)
                CollectionAssert.AreEqual(expected, obtained)
                Dim p = actual.PlotData(box)
                Graphics31Evidence.EqualBits(old.arXi, p.NormalX) : Graphics31Evidence.EqualBits(old.arGauss, p.NormalY)
                Graphics31Evidence.EqualBits(old.arOutliers, p.Outliers)
                Graphics31Evidence.EqualBits(old.Axes(box), Graphics32Tests.AxisVector(p.Axis))
                Assert.AreEqual(old.pHasBoxSummary, p.HasBoxSummary)
                obtained(0, 0) = 999.0 : Assert.AreNotEqual(999.0, actual.PlotData(box).Bins(0, 0))
                Graphics32Tests.MutateArrayGetters(p)
                Assert.AreEqual(before, Graphics31Evidence.Bits(values))
            Next
        Next
        values(0) = Double.NaN
        Dim e = Graphics30Tests.ExceptionType(Sub() old.Compute(True, "(Scott)", True)), a = Graphics30Tests.ExceptionType(Sub() actual.compute(True, "(Scott)", True))
        Assert.AreEqual(e, a)
        Graphics31Evidence.EqualBits(old.arBinMidVal, actual.PlotData().Midpoints)
        Graphics31Evidence.EqualBits(old.arGauss, actual.PlotData().NormalY)
    End Sub
    <TestMethod>
    Public Sub ActualViolinAxisDelegationAndNoNativeEntry()
        Dim result = ViolinPlotCore.Compute({4, 1, 4, 2, 1}, {"a", "a", "a", "a", "a"})
        Dim appearance As New ViolinPlotAppearance With {.YAxisMinimum = -10, .YAxisMaximum = 10, .YAxisMajorUnit = 2}
        Dim method = GetType(ViolinPlotExcel).GetMethod("ResolveAxisContext", BindingFlags.NonPublic Or BindingFlags.Static)
        Dim axis = method.Invoke(Nothing, {result, appearance}), projected = New ViolinPlotDomainData(result, -10, 10, 2)
        For Each name In {"XMinimum", "XMaximum", "YMinimum", "YMaximum"}
            Dim v = CDbl(axis.GetType().GetField(name, BindingFlags.NonPublic Or BindingFlags.Instance).GetValue(axis))
            Assert.AreEqual(CDbl(projected.GetType().GetProperty(name).GetValue(projected, Nothing)), v)
        Next
        Dim errorResult = Assert.ThrowsException(Of TargetInvocationException)(Sub() GetType(ViolinPlotExcel).GetMethod("AddChart").Invoke(Nothing, {Nothing, result, appearance, 20.0, 20.0, 720.0, 440.0}))
        Assert.IsInstanceOfType(errorResult.InnerException, GetType(ArgumentNullException))
    End Sub
End Class
