Option Explicit On
Option Strict On
Imports System
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports BESHStatNG
Imports Batch34CompletionFixtures
Imports Batch34PolicyFixtures
<TestClass>
Public Class Graphics34CompletionBoundsTests
    <TestMethod>
    Public Sub PolarSquareAndCentreInvariants()
        Dim method = GetType(LegacyPolicies34).GetMethod("PolarSquare")
        For i As Integer = 0 To 79
            Dim args = CompletionPolicyCases34.Arguments(method, i)
            Dim expected = DirectCast(method.Invoke(Nothing, args), Batch34PolicyFixtures.PolarSquareLayout34)
            Dim actual = PolarMeasuredLayoutCore.PolarSquare(CDbl(args(0)), CDbl(args(1)), CDbl(args(2)), CDbl(args(3)))
            Graphics34Tests.EqualBits(New Double() {expected.Left, expected.Top, expected.Side}, New Double() {actual.Left, actual.Top, actual.Side})
        Next
        Dim square = PolarMeasuredLayoutCore.PolarSquare(100, 40, 10, 20)
        Assert.AreEqual(40.0R, square.Side)
        Assert.AreEqual(60.0R, square.Left + square.Side / 2)
        Assert.AreEqual(40.0R, square.Top + square.Side / 2)
    End Sub
    <TestMethod>
    Public Sub SpcOptionalExtentsConstantVectorsAndPanelPlacement()
        For Each value As Double? In New Double?() {Nothing, -0.0R, -2, 0, 2, Double.NaN, Double.NegativeInfinity, Double.PositiveInfinity}
            Dim expected = LegacySpcBasis34.SpcOptionalExtent(value, -1, 1)
            Dim actual = SpcAxisBoundsCore.SpcOptionalExtent(value, -1, 1)
            Graphics34Tests.EqualBits(New Double() {expected.Min, expected.Max}, New Double() {actual.Min, actual.Max})
        Next
        For Each count As Integer In New Integer() {-2, -1, 0, 1, 5}
            For Each value As Double In New Double() {-0.0R, 0, 1, Double.NaN, Double.PositiveInfinity}
                Dim expected As Double() = Nothing, actual As Double() = Nothing
                Assert.AreEqual(Graphics34Tests.ErrorIdentity(Sub() expected = LegacySpcBasis34.SpcConstantReference(count, value)), Graphics34Tests.ErrorIdentity(Sub() actual = SpcAxisBoundsCore.SpcConstantReference(count, value)))
                If expected IsNot Nothing Then
                    Graphics34Tests.EqualBits(expected, actual)
                    If actual.Length > 0 Then actual(0) = 999
                    Graphics34Tests.EqualBits(expected, SpcAxisBoundsCore.SpcConstantReference(count, value))
                End If
            Next
        Next
        For i As Integer = 0 To 3
            Graphics34Tests.EqualBits(New Double() {LegacySpcBasis34.SpcPanelTop(10, i, 200, 20)}, New Double() {SpcAxisBoundsCore.SpcPanelTop(10, i, 200, 20)})
        Next
    End Sub
End Class
