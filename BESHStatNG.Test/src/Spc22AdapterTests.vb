Option Explicit On
Option Strict On
Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Reflection
Imports BESHStatNG.ExcelCommon
Imports BESHStatNG.StatisticalProcessControl
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass>
Public Class Spc22AdapterTests
    <DataTestMethod><DataRow(0)><DataRow(1)><DataRow(2)><DataRow(3)><DataRow(4)><DataRow(5)><DataRow(6)><DataRow(7)>
    Public Sub RetainedNativeBoundsAndZoneEligibilityMatchFrozenSource(id As Integer)
        Dim values As Double() = {2, 2, -1, 5, 1, 3, 0, 4}
        Select Case id
            Case 1 : values = Enumerable.Repeat(2.0R, 8).ToArray()
            Case 2 : values = Enumerable.Repeat(Double.NaN, 8).ToArray()
            Case 3 : values = {0.5R, 0.5R, 0, 1, 0.3R, 0.7R, 0.1R, 0.9R}
            Case 5 : values(0) = -0.5R
            Case 6 : values = Enumerable.Repeat(0.0R, 8).ToArray()
            Case 7 : values(6) = Double.NaN
        End Select
        Dim point As New SpcPointResult(0, values(0), values(1), values(2), values(3), lowerOneSigmaLimit:=values(4), upperOneSigmaLimit:=values(5), lowerTwoSigmaLimit:=values(6), upperTwoSigmaLimit:=values(7))
        Dim kind = If(id = 3, SpcPanelType.Proportion, If(id = 5, SpcPanelType.LowerCusum, SpcPanelType.IndividualValue))
        Dim panel As New SpcPanelResult(kind, "Panel", {point})
        Dim fit As New SpcFitResult(Spc22Fixture.Request(), {panel})
        Dim projection = SpcUnivariateChartProjectionPolicy.Project(fit)
        Dim options As New BESHStatNG.graphics.SpcControlChartAppearanceOptions With {.ShowSpecificationLimits = id = 4, .ShowTargetLine = id = 4}
        Dim oldOptions As New Spc22LegacyRenderer.SpcControlChartAppearanceOptions With {.ShowSpecificationLimits = id = 4, .ShowTargetLine = id = 4}
        Dim specs As New SpcSpecificationLimits(-20, 9, 30)
        Dim axis = Spc22LegacyRenderer.BuildSharedAxisContext(fit, oldOptions)
        Dim expected = Spc22LegacyRenderer.ComputeYAxisBounds(panel, Spc22LegacyRenderer.BuildPanelChartData(panel, axis), specs, oldOptions)
        Dim renderer = GetType(BESHStatNG.graphics.SpcControlChartExcel)
        Dim flags = BindingFlags.Static Or BindingFlags.NonPublic
        Dim data = renderer.GetMethod("MarshalPanel", flags).Invoke(Nothing, New Object() {projection.Panels(0)})
        Dim actual = renderer.GetMethod("ComputeYAxisBounds", flags).Invoke(Nothing, New Object() {panel, data, specs, options})
        Assert.AreEqual(BitConverter.DoubleToInt64Bits(expected.Minimum), BitConverter.DoubleToInt64Bits(CDbl(actual.GetType().GetField("Minimum").GetValue(actual))))
        Assert.AreEqual(BitConverter.DoubleToInt64Bits(expected.Maximum), BitConverter.DoubleToInt64Bits(CDbl(actual.GetType().GetField("Maximum").GetValue(actual))))
        Assert.AreEqual(Spc22LegacyRenderer.HasValidZones(point), CBool(renderer.GetMethod("HasValidZones", flags).Invoke(Nothing, New Object() {point})))
    End Sub
    Private Shared Function Bridge(name As String, argument As Object) As Object()
        Dim method = GetType(BESHStatNG.graphics.SpcControlChartExcel).GetMethod(name, BindingFlags.Static Or BindingFlags.NonPublic)
        Assert.IsNotNull(method, "Real shipping renderer bridge")
        Return DirectCast(method.Invoke(Nothing, New Object() {argument}), Object())
    End Function

    <TestMethod>
    Public Sub ActualNativeBridgeBoxesDoubleAndMarshalsMissingAsNothing()
        Dim values As Double?() = {Nothing, BitConverter.Int64BitsToDouble(Long.MinValue), 1.2345678901234567R, -9.0R, Nothing}
        Dim boxed = Bridge("MarshalChartValues", Array.AsReadOnly(values))
        Assert.AreEqual(values.Length, boxed.Length)
        For i = 0 To values.Length - 1
            Assert.AreEqual(values(i).HasValue, boxed(i) IsNot Nothing)
            If values(i).HasValue Then
                Assert.AreEqual(GetType(Double), boxed(i).GetType())
                Assert.AreEqual(BitConverter.DoubleToInt64Bits(values(i).Value), BitConverter.DoubleToInt64Bits(CDbl(boxed(i))))
            End If
        Next
        boxed(2) = 999
        Assert.AreEqual(1.2345678901234567R, values(2).Value)
    End Sub

    <DataTestMethod><DataRow(False)><DataRow(True)>
    Public Sub ActualNativeBridgeKeepsMixedSequenceAndLabelCategories(useSequence As Boolean)
        Dim panel As New SpcPanelResult(SpcPanelType.IndividualValue, "I", {Spc22Fixture.Point(0, sequence:=7), Spc22Fixture.Point(2), Spc22Fixture.Point(5, sequence:=-7)})
        Dim result As New SpcFitResult(Spc22Fixture.Request(), {panel})
        Dim projection = SpcUnivariateChartProjectionPolicy.Project(result, useSequence)
        Dim actual = Bridge("MarshalChartCategories", projection)
        Dim expected = Spc22LegacyRenderer.BuildSharedAxisContext(result, New Spc22LegacyRenderer.SpcControlChartAppearanceOptions With {.UseSequenceValuesForHorizontalAxis = useSequence}).Categories
        For i = 0 To expected.Length - 1
            Assert.AreEqual(expected(i).GetType(), actual(i).GetType())
            Assert.AreEqual(expected(i), actual(i))
        Next
    End Sub

    <DataTestMethod><DataRow(6)><DataRow(8)><DataRow(9)><DataRow(12)><DataRow(13)>
    Public Sub ActualNativeBridgeConsumesEveryChannelOfReferenceChartModes(chartOrdinal As Integer)
        Dim request = Spc21Profile.Request(chartOrdinal, True)
        Dim result = SpcEngine.Fit(request)
        Dim projection = SpcUnivariateChartProjectionPolicy.Project(result)
        Dim axis = Spc22LegacyRenderer.BuildSharedAxisContext(result, New Spc22LegacyRenderer.SpcControlChartAppearanceOptions())
        For Each panel In projection.Panels
            Dim original = Spc22LegacyRenderer.BuildPanelChartData(panel.Panel, axis)
            Dim expected As Object()() = {original.StatisticValues, original.CenterLineValues, original.LowerControlLimitValues, original.UpperControlLimitValues, original.LowerOneSigmaValues, original.UpperOneSigmaValues, original.LowerTwoSigmaValues, original.UpperTwoSigmaValues, original.SignalValues, original.ExcludedValues}
            For c = 0 To 9
                Dim actual = Bridge("MarshalChartValues", panel.Channels(c))
                For i = 0 To actual.Length - 1
                    Spc22Fixture.SameSample(expected(c)(i), If(actual(i) Is Nothing, CType(Nothing, Double?), CDbl(actual(i))))
                Next
            Next
        Next
    End Sub
End Class
