Option Explicit On
Option Strict On
Imports System
Imports System.Linq
Imports System.Reflection
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports BESHStatNG
Imports Batch30Fixtures

<TestClass>
Public Class Graphics31AdapterTests
    Private Shared Function Field(Of T)(instance As Object, name As String) As T
        Dim flags = BindingFlags.NonPublic Or BindingFlags.Instance Or BindingFlags.Public
        Dim f = instance.GetType().GetField(name, flags)
        If f IsNot Nothing Then Return DirectCast(f.GetValue(instance), T)
        Return DirectCast(instance.GetType().GetProperty(name, flags).GetValue(instance), T)
    End Function

    <TestMethod>
    Public Sub RocActualAdapterSnapshotsSeedStateAndTablesWithoutRendering()
        Dim groups = Graphics31RocTests.Groups(2), before = groups.Select(Function(x) DirectCast(x.Clone(), Double())).ToArray()
        Dim actual As New graphics.ROC(groups, {"positive", "negative"}), old As New LegacyRoc30(groups, {"positive", "negative"})
        Assert.IsNull(actual.PlotSeries().X)
        For Each alpha In {0.05, 0.1, 0.01}
            old.compute(alpha) : actual.compute(alpha)
            Dim series = actual.PlotSeries()
            EqualBits(old.par1minusSpec, series.X) : EqualBits(old.parSensitivity, series.Y)
            EqualBits(old.parCutOff, series.Cutoffs) : EqualBits(old.parSpecificity, series.Specificity)
            EqualBits({old.pAUC, old.pseAUC, old.pPvalue, old.pdelongSE}, {Field(Of Double)(actual, "pAUC"), Field(Of Double)(actual, "pseAUC"), Field(Of Double)(actual, "pPvalue"), Field(Of Double)(actual, "pdelongSE")})
            Assert.AreEqual(2, actual.wrapResults().Count)
            Dim saved = Bits(series.X) : series.X(0) = 999
            Assert.AreEqual(saved, Bits(actual.PlotSeries().X))
            Record("WIN31-ROC-" & alpha.ToString(Globalization.CultureInfo.InvariantCulture), String.Join(";", before.Select(Function(x) Bits(x))), "repeated compute alpha=" & alpha.ToString(Globalization.CultureInfo.InvariantCulture), "X/Y=u+1", old.par1minusSpec.Concat(old.parSensitivity).ToArray(), series.X.Concat(series.Y).ToArray())
        Next
        For i As Integer = 0 To groups.Length - 1
            Assert.AreEqual(Bits(before(i)), Bits(groups(i)))
        Next
        Dim singleton As Double()() = {New Double() {1}, New Double() {2, 3}}
        Dim curveOnly As New graphics.ROC(singleton, Nothing), legacy As New LegacyRoc30(singleton, Nothing)
        Assert.AreEqual(Graphics30Tests.ExceptionType(Sub() legacy.compute()), Graphics30Tests.ExceptionType(Sub() curveOnly.compute()))
        EqualBits(legacy.par1minusSpec, curveOnly.PlotSeries().X)
        Assert.AreEqual(legacy.pAUC, Field(Of Double)(curveOnly, "pAUC"))
        'The current accepted constructor retains the group alias for later recompute.
        groups(0)(0) = 77 : old.compute() : actual.compute()
        EqualBits(old.parCutOff, actual.PlotSeries().Cutoffs)
    End Sub

    <DataTestMethod>
    <DataRow("Blom", "SPSS")><DataRow("Blom", "OLS")><DataRow("Blom", "R")>
    <DataRow("Rankit", "SPSS")><DataRow("Rankit", "OLS")><DataRow("Rankit", "R")>
    <DataRow("Van Der Waerden", "SPSS")><DataRow("Van Der Waerden", "OLS")><DataRow("Van Der Waerden", "R")>
    Public Sub NormalActualAdapterCoordinatesIdentityAndMutableCompatibility(method As String, line As String)
        Dim values = {4.0, 1, 4, 2, 1}, before = DirectCast(values.Clone(), Double())
        Dim actual As New graphics.NormalPlot(values, " A "), legacy As New LegacyNormal30(DirectCast(values.Clone(), Double()), " A ")
        Dim scores = actual.compute(method), expected = legacy.compute(method)
        EqualBits(expected, scores)
        Dim lineValues = actual.computeRefLIne(line), oldLine = legacy.computeRefLIne(line)
        Dim series = actual.PlotSeries()
        EqualBits(legacy.pData, series.X) : EqualBits(legacy.pZ, series.Y) : EqualBits(legacy.pP, series.Probabilities)
        EqualBits(Graphics30Tests.Flat(oldLine), Graphics30Tests.Flat(series.ReferenceLine))
        CollectionAssert.AreEqual({1, 4, 3, 0, 2}, series.SortedToSource)
        Dim points = actual.PlotPointOrder()
        CollectionAssert.AreEqual(series.SortedToSource, points.Select(Function(p) p.ObservationIndex).ToArray())
        Assert.IsTrue(points.All(Function(p) Not p.PhysicalRowId.HasValue AndAlso p.GroupOrdinal = 0))
        Assert.AreEqual(" A ", Field(Of String)(actual, "pvarName"))
        Record("WIN31-QQ-" & method & "-" & line, Bits(before), "score=" & method & ";line=" & line, "observed X/normal Y=n;ref=2x2", legacy.pData.Concat(legacy.pZ).Concat(Graphics30Tests.Flat(oldLine)).ToArray(), series.X.Concat(series.Y).Concat(Graphics30Tests.Flat(series.ReferenceLine)).ToArray())
        'Snapshot isolation and mutable legacy return values are separate contracts.
        Dim saved = Bits(series.Y)
        scores(0) = 7 : expected(0) = 7
        Assert.AreEqual(saved, Bits(series.Y))
        EqualBits(expected, actual.PlotSeries().Y)
        EqualBits(Graphics30Tests.Flat(legacy.computeRefLIne(line)), Graphics30Tests.Flat(actual.computeRefLIne(line)))
        Dim mutableLine = actual.computeRefLIne(line)
        mutableLine(0, 0) = 77
        Assert.AreEqual(77.0, actual.PlotSeries().ReferenceX(0))
        Assert.AreNotEqual(77.0, series.ReferenceX(0))
        EqualBits(before, values) : values(0) = 999
        EqualBits({1, 1, 2, 4, 4}, actual.PlotSeries().X)
        EqualBits(legacy.compute(method), actual.compute(method))
    End Sub

    <TestMethod>
    Public Sub PreScoreCallsErrorsAndPublicDefaultsStayCharacterized()
        For Each line In {"SPSS", "OLS", "R", "unknown", Nothing}
            Dim actual As New graphics.NormalPlot({3, 1, 2}, Nothing), old As New LegacyNormal30({3, 1, 2}, Nothing)
            Assert.IsNull(actual.PlotSeries().Y)
            Assert.AreEqual(Graphics30Tests.ExceptionType(Sub() old.computeRefLIne(line)), Graphics30Tests.ExceptionType(Sub() actual.computeRefLIne(line)))
        Next
        For Each values In New Double()() {New Double() {}, New Double() {1}, New Double() {Double.NaN, 1, Double.PositiveInfinity}}
            For Each line In {"SPSS", "OLS", "R", "unknown"}
                Dim actual As New graphics.NormalPlot(values, Nothing), old As New LegacyNormal30(DirectCast(values.Clone(), Double()), Nothing)
                EqualBits(old.compute("unknown"), actual.compute("unknown"))
                Assert.AreEqual(Graphics30Tests.ExceptionType(Sub() old.computeRefLIne(line)), Graphics30Tests.ExceptionType(Sub() actual.computeRefLIne(line)))
            Next
        Next
        Assert.AreEqual(0.05, CDbl(GetType(graphics.ROC).GetMethod("compute").GetParameters()(0).DefaultValue))
        Assert.AreEqual(GetType(Double(,)), GetType(graphics.NormalPlot).GetMethod("computeRefLIne").ReturnType)
        Assert.IsNotNull(GetType(graphics.ROC).GetMethod("addROCplot"))
        Assert.IsNotNull(GetType(graphics.NormalPlot).GetMethod("addChart"))
    End Sub
End Class
