Option Explicit On
Option Strict On
Option Infer On
Imports System
Imports System.Collections.Generic
Imports System.Globalization
Imports System.Reflection
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports BESHStatNG
Imports Batch34NativeStateFixtures
<TestClass>
Public Class Graphics34CompletionNativeStateTests
    <DataTestMethod><DataRow("")><DataRow("sk-SK")>
    Public Sub LegendStateAndNativeDateFormattingRemainCompatible(culture As String)
        Dim saved = CultureInfo.CurrentCulture
        Try
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(culture)
            Dim observations = LadderPlotCore.Compute(New Double(){1,2,3,4},New Double(){4,3,2,1},New Object(){"A","B","A","B"}).Observations
            Dim current = GetType(LadderPlotExcel).GetMethod("ShouldKeepLegendEntry",BindingFlags.NonPublic Or BindingFlags.Static)
            For Each show In New Boolean(){False,True}
                For mode As Integer = -1 To 3
                    Dim appearance As New LadderPlotAppearance With {.ShowLegend=show,.ColorMode=CType(mode,LadderColorMode)}
                    Dim expectedGroups As New HashSet(Of Integer), actualGroups As New HashSet(Of Integer)
                    For repeat As Integer = 0 To 1
                        For i As Integer = 0 To observations.Length-1
                            Dim expected = LegacyNativeState34.ShouldKeepLegendEntry(observations(i),i,appearance,expectedGroups)
                            Dim actual = CBool(current.Invoke(Nothing,New Object(){observations(i),i,appearance,actualGroups}))
                            Assert.AreEqual(expected,actual)
                            CollectionAssert.AreEquivalent(New List(Of Integer)(expectedGroups),New List(Of Integer)(actualGroups))
                        Next
                    Next
                Next
            Next
            Dim dateMethod = GetType(DumbbellPlotExcel).GetMethod("FormatEndpointValue",BindingFlags.NonPublic Or BindingFlags.Static)
            Dim oldMethod = GetType(LegacyNativeState34).GetMethod("FormatEndpointValue")
            For Each kind In [Enum].GetValues(dateMethod.GetParameters()(1).ParameterType)
                For Each value As Double In New Double(){-0.0,1,45000.25,-1000000,Double.NaN,Double.PositiveInfinity}
                    For Each requestedFormat As String In New String(){Nothing,"","0.00","yyyy-MM-dd","Q"}
                        Dim oldError As String=Nothing,newError As String=Nothing
                        Dim args As Object()={value,kind,requestedFormat}
                        Dim expected=Batch34CompletionFixtures.Completion34Cases.Invoke(oldMethod,args,oldError)
                        Dim actual=Batch34CompletionFixtures.Completion34Cases.Invoke(dateMethod,args,newError)
                        Assert.AreEqual(oldError,newError)
                        Assert.AreEqual(expected,actual)
                    Next
                Next
            Next
        Finally
            CultureInfo.CurrentCulture=saved
        End Try
    End Sub
End Class
