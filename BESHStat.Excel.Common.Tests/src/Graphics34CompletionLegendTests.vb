Option Explicit On
Option Strict On
Imports System
Imports System.Linq
Imports System.Globalization
Imports System.Reflection
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports BESHStatNG
Imports BESHStatNG.ExcelCommon.Chart34
Imports Batch34PolicyFixtures
Imports Batch34CompletionFixtures
<TestClass>
Public Class Graphics34CompletionLegendTests
    <DataTestMethod><DataRow("")><DataRow("sk-SK")>
    Public Sub OrderedLegendPoliciesPreserveSourceSemantics(culture As String)
        Dim saved = CultureInfo.CurrentCulture
        Try
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(culture)
            For Each method In GetType(LegacyPolicies34).GetMethods(BindingFlags.Static Or BindingFlags.Public).Where(Function(m) m.Name <> "PolarSquare")
                Dim current = GetType(SpecialtyLegendPolicy34).GetMethod(method.Name)
                For i As Integer = 0 To 79
                    Dim expectedError As String = Nothing, actualError As String = Nothing
                    Dim expected = Completion34Cases.Invoke(method, CompletionPolicyCases34.Arguments(method, i), expectedError)
                    Dim actual = Completion34Cases.Invoke(current, CompletionPolicyCases34.Arguments(method, i), actualError)
                    Assert.AreEqual(expectedError, actualError, method.Name & i)
                    If expectedError = "None" Then Assert.AreEqual(Completion34Cases.Snapshot(expected), Completion34Cases.Snapshot(actual))
                Next
            Next
            Dim keys As Integer() = {3, 1, 3, 0}
            Dim order = SpecialtyLegendPolicy34.SankeyLegendOrder(keys)
            CollectionAssert.AreEqual(New Integer() {3, 1, 0, 2}, order)
            keys(0) = -999
            CollectionAssert.AreEqual(New Integer() {3, 1, 0, 2}, order)
            For mode As Integer = -1 To 3
                Dim choice = SpecialtyLegendPolicy34.LadderLegendChoice(True, mode, 0)
                Assert.AreEqual(mode = 2, choice.RecordGroup)
                If mode <> 2 Then Assert.IsTrue(choice.Keep)
            Next
            Dim groupLegend = SpecialtyLegendPolicy34.CdfLegendChoice(True, False, True, True)
            Assert.IsFalse(groupLegend.KeepEmpirical)
            Assert.IsTrue(groupLegend.KeepFitted)
            Assert.IsTrue(groupLegend.RenameRepresentative)
            Dim curveLegend = SpecialtyLegendPolicy34.CdfLegendChoice(True, True, True, True)
            Assert.IsTrue(curveLegend.KeepEmpirical AndAlso curveLegend.KeepFitted)
            Assert.IsFalse(curveLegend.RenameRepresentative)
        Finally
            CultureInfo.CurrentCulture = saved
        End Try
    End Sub
End Class
