Option Explicit On
Option Strict On
Imports System
Imports System.Linq
Imports System.Reflection
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports BESHStatNG
Imports Batch30Fixtures

<TestClass>
Public Class Graphics30AdapterTests
    Private Shared Function Value(Of T)(instance As Object, name As String) As T
        Dim flags = BindingFlags.NonPublic Or BindingFlags.Instance Or BindingFlags.Public
        Dim field = instance.GetType().GetField(name, flags)
        If field IsNot Nothing Then Return DirectCast(field.GetValue(instance), T)
        Return DirectCast(instance.GetType().GetProperty(name, flags).GetValue(instance), T)
    End Function
    <TestMethod>
    Public Sub RocWindowsAdapterRetainsFirstAndRepeatedStateAndReportShape()
        Dim groups As Double()() = {New Double() {5, 1, 3, 3}, New Double() {0, 2, 3}}
        Dim old As New LegacyRoc30(groups, {"a", "b"}), actual As New graphics.ROC(groups, {"a", "b"})
        For Each alpha In {0.05, 0.1}
            old.compute(alpha) : actual.compute(alpha)
            Graphics30Tests.EqualValues({old.pAUC, old.pseAUC, old.pPvalue, old.pdelongSE},
                {Value(Of Double)(actual, "pAUC"), Value(Of Double)(actual, "pseAUC"), Value(Of Double)(actual, "pPvalue"), Value(Of Double)(actual, "pdelongSE")})
            For Each name In {"parCutOff", "parSensitivity", "parSpecificity", "par1minusSpec"}
                Graphics30Tests.EqualValues(Value(Of Double())(old, name), Value(Of Double())(actual, name))
            Next
            Assert.AreEqual(2, actual.wrapResults().Count)
            Graphics30Tests.Record("WINDOWS-ROC-" & alpha, {old.pAUC, old.pseAUC, old.pPvalue, old.pdelongSE}, {Value(Of Double)(actual, "pAUC"), Value(Of Double)(actual, "pseAUC"), Value(Of Double)(actual, "pPvalue"), Value(Of Double)(actual, "pdelongSE")})
        Next
        Dim invalid As Double()() = {New Double() {1}, New Double() {2, 3}}
        Dim oldError As New LegacyRoc30(invalid, Nothing), newError As New graphics.ROC(invalid, Nothing)
        Assert.AreEqual(Graphics30Tests.ExceptionType(Sub() oldError.compute()), Graphics30Tests.ExceptionType(Sub() newError.compute()))
        Assert.AreEqual(oldError.pAUC, Value(Of Double)(newError, "pAUC")) 'Partial state survives DeLong failure.
    End Sub

    <TestMethod>
    Public Sub NormalWindowsAdapterMatchesSourceWithIntentionalCopyBoundary()
        For Each line In {"SPSS", "OLS", "R", "unknown"}
            Dim legacy As New LegacyNormal30({3, 1, 2}, Nothing), wrapper As New graphics.NormalPlot({3, 1, 2}, Nothing)
            Assert.AreEqual(Graphics30Tests.ExceptionType(Sub() legacy.computeRefLIne(line)), Graphics30Tests.ExceptionType(Sub() wrapper.computeRefLIne(line)))
        Next
        For Each method In {"Blom", "Rankit", "Van Der Waerden"}
            For Each line In {"SPSS", "OLS", "R"}
                Dim values = {9.0, 1, 3, 3, 5, 2}, before = DirectCast(values.Clone(), Double())
                Dim old As New LegacyNormal30(DirectCast(values.Clone(), Double()), " A "), actual As New graphics.NormalPlot(values, " A ")
                Graphics30Tests.EqualValues(old.compute(method), actual.compute(method))
                Graphics30Tests.EqualValues(Graphics30Tests.Flat(old.computeRefLIne(line)), Graphics30Tests.Flat(actual.computeRefLIne(line)))
                Graphics30Tests.EqualValues(before, values)
                Assert.AreEqual(" A ", Value(Of String)(actual, "pvarName"))
                Graphics30Tests.Record("WINDOWS-QQ-" & method & "-" & line, Graphics30Tests.Flat(old.computeRefLIne(line)), Graphics30Tests.Flat(actual.computeRefLIne(line)))
                'Retain the Windows returned-vector alias contract while Core snapshots are immutable.
                Dim oldScores = old.compute(method), newScores = actual.compute(method)
                oldScores(0) = 7 : newScores(0) = 7
                Graphics30Tests.EqualValues(Graphics30Tests.Flat(old.computeRefLIne(line)), Graphics30Tests.Flat(actual.computeRefLIne(line)))
            Next
        Next
    End Sub

    <TestMethod>
    Public Sub BoxHistogramAndLabelWindowsConsumers()
        Dim groups As Double()() = {New Double() {-5, -1, 0, 4}, New Double() {1, 2, 7}}
        Dim labels = {"same ", "same "}
        Dim box = DirectCast(Activator.CreateInstance(GetType(graphics.BoxPlot), BindingFlags.Instance Or BindingFlags.Public Or BindingFlags.NonPublic,
            Nothing, New Object() {groups, labels, Nothing, ""}, Nothing), graphics.BoxPlot)
        box.Calculate() : box.CalcForPlotting()
        CollectionAssert.AreEqual(labels, Value(Of String())(box, "pGroupNames"))
        labels(0) = "latest label"
        Dim snapshot = DirectCast(box.GetType().GetMethod("SnapshotGroupNames", BindingFlags.NonPublic Or BindingFlags.Instance).Invoke(box, Nothing), String())
        Assert.AreEqual("latest label", snapshot(0))
        snapshot(0) = "changed copy"
        Assert.AreEqual("latest label", Value(Of String())(box, "pGroupNames")(0))
        Assert.IsNotNull(box.wrapResults())
        For i As Integer = 0 To groups.Length - 1
            Dim segment = BoxSegmentsCore.Compute(box.Q1(i), box.Medians(i), box.Q3(i))
            Graphics30Tests.EqualValues({segment.Blank, segment.Median, segment.Q3, segment.Q3Minus, segment.MedianMinus},
                {Value(Of Double())(box, "pPlotBlank")(i), Value(Of Double())(box, "pPlotMedian")(i), Value(Of Double())(box, "pPlotQ3")(i), Value(Of Double())(box, "pPlotQ3Minus")(i), Value(Of Double())(box, "pPlotMedianMinus")(i)})
        Next
        Dim values = {8.0, 1, 4, 2, 9, 3, 6, 5}, before = DirectCast(values.Clone(), Double())
        Dim hist As New graphics.Histogram(values)
        CollectionAssert.AreEqual(HistogramDataCore.BinsCopy(values, "(Freedman-Diaconis)"), hist.compute(True, "(Freedman-Diaconis)", True))
        Graphics30Tests.EqualValues(before, values)
        hist.compute(True, "(Sturges)", False) : Graphics30Tests.EqualValues(before, values)
    End Sub

    <TestMethod>
    Public Sub ScatterXyzAndAxisWindowsConsumers()
        Dim data As Double(,) = {{3, 9}, {1, 4}, {7, 3}}
        Dim scatter = DirectCast(Activator.CreateInstance(GetType(graphics.ScatterPlotMatrix), BindingFlags.Instance Or BindingFlags.Public Or BindingFlags.NonPublic,
            Nothing, New Object() {data, New String() {"a", "a"}, Nothing, 0.4}, Nothing), graphics.ScatterPlotMatrix)
        scatter.settingInputs(False, True) : scatter.transformData()
        Dim old As New LegacyScatter30 With {.pData = data, .n = 3, .p = 2, .pbDisplayRegLines = True}
        old.transformData()
        Assert.AreEqual(0.1, Value(Of Double)(scatter, "pMargin"))
        Graphics30Tests.EqualValues(Graphics30Tests.Flat(old.ScaledData), Graphics30Tests.Flat(Value(Of Double(,))(scatter, "ScaledData")))
        Graphics30Tests.EqualValues(old.x_regline, Value(Of Double())(scatter, "x_regline"))
        Graphics30Tests.EqualValues(old.y_regline, Value(Of Double())(scatter, "y_regline"))
        Dim xyz As New graphics.XYZscatter()
        Dim empty As New graphics.XYZscatter()
        empty.normalizedData()
        Assert.AreEqual(0, Value(Of Double())(empty, "x_norm").Length)
        xyz.dataInputs({1, 3, 5}, {0, 4, 8}, {-3, 1, 5})
        xyz.SetAxisLimitsX(-1, 7) : xyz.AxisDirectionInputs(True, False, True)
        xyz.GetType().GetMethod("getnorm", BindingFlags.NonPublic Or BindingFlags.Instance).Invoke(xyz, Nothing)
        Dim mins = Value(Of Double())(xyz, "raw_mins_"), spans = Value(Of Double())(xyz, "raw_ranges_"), directions = Value(Of Double())(xyz, "axis_dir_")
        Graphics30Tests.EqualValues(XyzDataCore.NormalizeAxis({1, 3, 5}, mins(0), spans(0), directions(0), 3), Value(Of Double())(xyz, "x_norm"))
        For Each bounds In New Double()() {New Double() {0, 0}, New Double() {-3, 8}, New Double() {8, -3}, New Double() {0.000001, 0.000009}, New Double() {1.0E+12, 9.0E+12}, New Double() {-0.0, 0.0}}
            Dim expected = ChartAxisScalingCore.Compute(bounds(0), bounds(1)), adapter = graphics.ChartingFunc.ChartScaling(bounds(0), bounds(1))
            Graphics30Tests.EqualValues({expected.Min, expected.Max, expected.Scale}, {adapter.Min, adapter.Max, adapter.Scale})
        Next
    End Sub
End Class
