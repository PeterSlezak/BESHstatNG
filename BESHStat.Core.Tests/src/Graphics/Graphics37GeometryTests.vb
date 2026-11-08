Option Explicit On
Option Strict On
Option Infer On
Imports System
Imports System.Linq
Imports System.Threading.Tasks
Imports BESHStatNG
Imports Frozen37
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass>
Public Class Graphics37GeometryTests
    <DataTestMethod><DataRow(0.0)><DataRow(0.25)><DataRow(0.5)><DataRow(0.75)><DataRow(1.0)><DataRow(Double.NaN)>
    Public Sub BiplotUsesOriginalEigenvalueScaling(c As Double)
        Dim model = Cases37.PcaModel(3)
        Dim expected = DirectCast(Original37.PcaBiplot(model, c, Nothing, Nothing), Object())
        Dim actual = MultivariatePlotGeometry37.Biplot(model.ReducedDataset, model.GetLoadings, model.Eigenvalues, model.ObservationCount, model.VariableCount, c)
        Assert.AreEqual(Cases37.Snapshot(expected.Take(4).ToArray()), Cases37.Snapshot(New Object(){actual.ScoreX, actual.ScoreY, actual.LoadingX, actual.LoadingY}))
        Dim axes = MultivariatePlotGeometry37.CombinedAxes(actual.ScoreX, actual.ScoreY, actual.LoadingX, actual.LoadingY)
        Assert.AreEqual(Cases37.Snapshot(expected.Skip(8).ToArray()), Cases37.Snapshot(New Object(){axes.X.Min, axes.X.Max, axes.X.Scale, axes.Y.Min, axes.Y.Max, axes.Y.Scale}))
    End Sub

    <TestMethod>
    Public Sub FactorPercentRetainsPatternTimesStructureAndTrace()
        Dim model = Cases37.FactorModel(2)
        Dim expected = DirectCast(Original37.FactorLoadingPlot2D(model), Object())
        Assert.AreEqual(Cases37.Snapshot(expected(3)), Cases37.Snapshot(MultivariatePlotGeometry37.FactorPercent(model.PatternMatrix, model.StructureMatrix, model.WorkingMatrix)))
        Dim scree = DirectCast(Original37.FactorScreePlot(model), Object())
        Assert.AreEqual(Cases37.Snapshot(scree), Cases37.Snapshot(New Object(){MultivariatePlotGeometry37.ComponentAxis(model.InitialEigenvalues.Length), MultivariatePlotGeometry37.InitialFactorPercent(model.InitialEigenvalues, model.WorkingMatrix)}))
    End Sub

    <DataTestMethod><DataRow(0)><DataRow(1)>
    Public Sub DendrogramBoundsUseExistingPathsCutsAndPadding(source As Integer)
        For id As Integer = 0 To 127
            Dim layout = If(source = 0, Cases37.Layout(id), OriginalSeries37.FittedLayout(id))
            Dim geometry As New DendrogramChartGeometry37(layout)
            Try
                Dim expected = Original37.DendrogramBounds(layout)
                Dim actual = geometry.Bounds()
                Assert.AreEqual(Cases37.Snapshot(expected), Cases37.Snapshot(New Object(){actual.XMin, actual.XMax, actual.YMin, actual.YMax}), "case " & id)
            Catch ex As InvalidOperationException
                Assert.ThrowsException(Of InvalidOperationException)(Function() geometry.Bounds())
            End Try
        Next
    End Sub

    <TestMethod>
    Public Sub DetachedVectorsAndNestedLayoutArraysAreRepeatableAndConcurrent()
        Dim model = Cases37.PcaModel(3), before = Cases37.Snapshot(model.ReducedDataset), layout = Cases37.Layout(3)
        Dim geometry As New DendrogramChartGeometry37(layout), expected = Cases37.Snapshot(geometry.PolylineX)
        Dim segment = geometry.ClusterPolylineX : segment(0)(0) = 999 : segment.Clear()
        Dim labels = geometry.LeafLabels : labels(0) = "mutated"
        layout.PolylineX(0) = 999
        Parallel.For(0, 16, Sub(i)
            Dim b = MultivariatePlotGeometry37.Biplot(model.ReducedDataset, model.GetLoadings, model.Eigenvalues, model.ObservationCount, model.VariableCount, 0.5)
            Dim a = b.ScoreX : a(0) = 999
            Assert.AreNotEqual(999.0, b.ScoreX(0))
            Dim x = geometry.PolylineX : x(0) = 999
            Assert.AreEqual(expected, Cases37.Snapshot(geometry.PolylineX))
            Assert.AreNotEqual(999.0, geometry.ClusterPolylineX(0)(0))
            Assert.AreNotEqual("mutated", geometry.LeafLabels(0))
        End Sub)
        Assert.AreEqual(before, Cases37.Snapshot(model.ReducedDataset))
        Dim column = MultivariatePlotGeometry37.Column(model.ReducedDataset, 0) : column(0) = 999
        Assert.AreEqual(before, Cases37.Snapshot(model.ReducedDataset))
    End Sub

    <TestMethod>
    Public Sub ArrowAndZeroReferenceTypesAndSignedZerosArePreserved()
        Dim minusZero = BitConverter.Int64BitsToDouble(Long.MinValue)
        Assert.AreEqual("System.Double[0:2]{D0000000000000000|D8000000000000000}", Cases37.Snapshot(MultivariatePlotGeometry37.LoadingVector(minusZero)))
        Assert.IsInstanceOfType(MultivariatePlotGeometry37.ZeroPair(), GetType(Integer()))
        Dim axes = MultivariatePlotGeometry37.SymmetricAxes({-0.2, 0.7}, {-0.8, 0.1})
        Assert.AreEqual(Cases37.Snapshot(ChartAxisScalingCore.Compute(-0.7, 0.7).Min), Cases37.Snapshot(axes.X.Min))
        Assert.AreEqual(Cases37.Snapshot(ChartAxisScalingCore.Compute(-0.8, 0.8).Max), Cases37.Snapshot(axes.Y.Max))
    End Sub
End Class
