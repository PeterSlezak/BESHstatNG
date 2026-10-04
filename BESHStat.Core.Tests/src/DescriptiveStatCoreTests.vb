Option Explicit On
Option Strict On

Imports System
Imports System.Collections.Generic
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass()>
Public Class DescriptiveStatCoreTests

    <TestMethod()>
    Public Sub Compute_WithoutShapiro_MatchesReferenceSummary()
        Dim data() As Double = {1.0R, 2.0R, 3.0R, 4.0R}
        Dim stats As New BESHStatNG.DescriptiveStat(data)

        stats.compute(False)

        Assert.AreEqual(1.5R, stats.LQuartile, 0.000000000001R)
        Assert.AreEqual(2.5R, stats.Median, 0.000000000001R)
        Assert.AreEqual(3.5R, stats.UQuartile, 0.000000000001R)
        Assert.AreEqual(2.0R, stats.IQR, 0.000000000001R)
        Assert.AreEqual(2.5R, stats.Mean, 0.000000000001R)
        Assert.AreEqual(1.0R, stats.Minimum, 0.000000000001R)
        Assert.AreEqual(4.0R, stats.Maximum, 0.000000000001R)

        Dim wrapped(,) As Object = stats.wrapSelf(True)
        Assert.AreEqual(17, wrapped.GetLength(0))
        Assert.AreEqual("Valid Data", Convert.ToString(wrapped(0, 0)))
        Assert.AreEqual(4, Convert.ToInt32(wrapped(0, 1)))
        Assert.AreEqual(2.5R, Convert.ToDouble(wrapped(1, 1)), 0.000000000001R)
        Assert.IsTrue(Double.IsNaN(Convert.ToDouble(wrapped(15, 1))))
        Assert.IsTrue(Double.IsNaN(Convert.ToDouble(wrapped(16, 1))))
    End Sub

    <TestMethod()>
    Public Sub Compute_WithShapiro_UsesCoreAssumptionsEngine()
        Dim data() As Double = {-1.2R, -0.7R, -0.3R, 0.0R, 0.15R, 0.32R, 0.5R, 0.9R, 1.1R, 1.3R,
                                  -0.1R, 0.22R, 0.45R, -0.55R, 0.78R, 1.05R, -0.95R, 0.6R, -0.4R, 0.12R}
        Dim expectedData() As Double = DirectCast(data.Clone(), Double())
        Dim errorText As String = String.Empty
        Dim expected As BESHStatNG.TestResult = BESHStatNG.assumptions.Assumptions.ShapiroWilk(expectedData, errorText)
        Assert.IsNotNull(expected)

        Dim stats As New BESHStatNG.DescriptiveStat(data)
        stats.compute(True)
        Dim wrapped(,) As Object = stats.wrapSelf(True)

        Assert.AreEqual(expected.TestStatistics1, Convert.ToDouble(wrapped(15, 1)), 0.000000000001R)
        Assert.AreEqual(expected.Pvalue, Convert.ToDouble(wrapped(16, 1)), 0.000000000001R)
    End Sub

    <TestMethod()>
    Public Sub WrapSelf_SelectedStatistics_PreservesRequestedOrder()
        Dim stats As New BESHStatNG.DescriptiveStat(New Double() {1.0R, 2.0R, 3.0R, 4.0R})
        stats.compute(False)

        Dim selected As New List(Of String) From {"max", "mean", "q1"}
        Dim wrapped(,) As Object = stats.wrapSelf(True, selected)

        Assert.AreEqual("Maximum", Convert.ToString(wrapped(0, 0)))
        Assert.AreEqual(4.0R, Convert.ToDouble(wrapped(0, 1)), 0.000000000001R)
        Assert.AreEqual("Mean", Convert.ToString(wrapped(1, 0)))
        Assert.AreEqual(2.5R, Convert.ToDouble(wrapped(1, 1)), 0.000000000001R)
        Assert.AreEqual("Q1", Convert.ToString(wrapped(2, 0)))
        Assert.AreEqual(1.5R, Convert.ToDouble(wrapped(2, 1)), 0.000000000001R)
    End Sub

End Class
