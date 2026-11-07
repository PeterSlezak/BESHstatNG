Option Explicit On
Option Strict On
Imports System
Imports System.Linq
Imports System.Globalization
Imports System.Collections.Generic
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports BESHStatNG
Imports BESHStatNG.CausalInference
Imports BESHStatNG.ExcelCommon

<TestClass>
Public Class Graphics34LoveTests
    <DataTestMethod><DataRow("")><DataRow("sk-SK")>
    Public Sub FiniteAdmissionThresholdAndAcceptedOrdering(culture As String)
        Dim old = CultureInfo.CurrentCulture
        Try
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(culture)
            Assert.IsFalse(PsmLoveSeriesPolicy.HasAnyFinite(Nothing))
            Assert.IsFalse(PsmLoveSeriesPolicy.HasAnyFinite(New Double() {}))
            Assert.IsFalse(PsmLoveSeriesPolicy.HasAnyFinite(New Double() {Double.NaN, Double.PositiveInfinity, Double.NegativeInfinity}))
            Assert.IsTrue(PsmLoveSeriesPolicy.HasAnyFinite(New Double() {-0.0, Double.NaN}))
            Dim source As New List(Of PsmLovePlotRow) From {
                New PsmLovePlotRow With {.VariableName = "B", .SmdBefore = -0.3, .SmdAfterMatching = 0.1, .SmdAfterWeighting = Double.NaN, .Threshold = 0.2},
                New PsmLovePlotRow With {.VariableName = "A", .SmdBefore = -0.3, .SmdAfterMatching = 0.2, .SmdAfterWeighting = Double.PositiveInfinity, .Threshold = 0.1}}
            Dim accepted As New PsmLovePlotProjection(source)
            CollectionAssert.AreEqual(New String() {"A", "B"}, accepted.Labels)
            Dim reference = PsmLoveSeriesPolicy.ThresholdReference(accepted.Threshold, accepted.Count)
            CollectionAssert.AreEqual(New Double() {accepted.Threshold, accepted.Threshold}, reference.XValues)
            CollectionAssert.AreEqual(New Double() {0.5, 2.5}, reference.YValues)
            Assert.IsFalse(PsmLoveSeriesPolicy.HasAnyFinite(accepted.AfterWeighting))
            Dim x = reference.XValues, y = reference.YValues
            x(0) = 99 : y(0) = 99
            Assert.AreEqual(accepted.Threshold, reference.XValues(0))
            Assert.AreEqual(0.5R, reference.YValues(0))
            source.Clear()
            Assert.AreEqual(2, accepted.Count)
            'No threshold guard is added to rendering. Nonfinite threshold input
            'remains representable exactly, preserving deferred C25 behavior.
            Assert.IsTrue(Double.IsNaN(PsmLoveSeriesPolicy.ThresholdReference(Double.NaN, 0).XValues(0)))
        Finally
            CultureInfo.CurrentCulture = old
        End Try
    End Sub
End Class
