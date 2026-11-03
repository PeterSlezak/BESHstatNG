Option Explicit On
Option Strict On
Imports System
Imports System.Collections.Generic
Imports System.Globalization
Imports System.Linq
Imports System.Threading
Imports System.Windows.Forms
Imports BESHStatNG.ExcelCommon
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass>
Public Class RegressionEffects08Tests
    Private Shared Sub Sta(action As Action)
        Dim failure As Exception = Nothing
        Dim worker As New Thread(Sub()
                                     Try
                                         action()
                                     Catch ex As Exception
                                         failure = ex
                                     End Try
                                 End Sub)
        worker.SetApartmentState(ApartmentState.STA) : worker.Start() : worker.Join()
        If failure IsNot Nothing Then Throw failure
    End Sub
    <TestMethod>
    Public Sub CanonicalDoubleMetadataMatchesPortableRuntimeContract()
        Dim s As New TermSpec With {.Kind = "MainEffect", .Degree = 1, .BaseVarKeys = New List(Of String) From {"a"}, .Scale = PredictorScale.Categorical,
            .ReferenceValue = BitConverter.Int64BitsToDouble(&H3FD5555555555555L), .ObservedLevels = New List(Of Double) From {BitConverter.Int64BitsToDouble(Long.MinValue)}}
        Dim text = RegressionEffectSerialization.Serialize(New RegressionEffectModel({New RegressionAuthoredEffect("[Cat] a", s)}))
        StringAssert.Contains(text, "<reference>3FD5555555555555</reference>")
        StringAssert.Contains(text, "<value>8000000000000000</value>")
        Dim restored As RegressionEffectModel = Nothing, issue As RegressionEffectDiagnostic = Nothing
        Assert.IsTrue(RegressionEffectSerialization.TryDeserialize(text, restored, issue))
        Assert.AreEqual(Long.MinValue, BitConverter.DoubleToInt64Bits(restored.Effects(0).Spec.ObservedLevels(0)))
        s.ObservedLevels = Nothing : s.DisplayNameForCoef = "a"
        Dim projection = RegressionEffectSerialization.ProjectFormula(New RegressionEffectModel({New RegressionAuthoredEffect("[Cat] a", s)}), {"a"})
        Assert.IsFalse(projection.Representable, "Framework Core normalization uses G formatting; the exact reference must not be silently rounded.")
        StringAssert.Contains(projection.Diagnostics.Single().Message, "C08-FORMULA-REF-01")
    End Sub
    <DataTestMethod> <DataRow(0)> <DataRow(1)> <DataRow(2)> <DataRow(3)> <DataRow(4)> <DataRow(5)>
    Public Sub RemovalAndClearMatchFrozenTextRulesIncludingPunctuation(index As Integer)
        Sta(Sub()
                For Each names In {New String() {"A", "B", "C"}, New String() {"A:B", " B ", "雪"}, New String() {"A""B", "A^2", "[Cat] x"}}
                    Using variables = Box(names, 7), oldBox = Box({}, 0), newBox = Box({}, 0)
                        Dim old As New LegacyRegressionEffects08(variables, oldBox), current As New RegressionEffectsController(variables, newBox)
                        old.AddMainEffectsFromSelectedVars() : current.AddMainEffectsFromSelectedVars()
                        old.AddTwoWayInteractionsFromSelectedVars() : current.AddTwoWayInteractionsFromSelectedVars()
                        oldBox.SetSelected(index, True) : newBox.SetSelected(index, True)
                        Dim removal As New LegacyRegressionRemoval08
                        removal.Remove_Item(oldBox, "selected", old.TermSpecs)
                        UIprocedures.Remove_Item(newBox, "selected", current.TermSpecs)
                        Same(oldBox, newBox, old.TermSpecs, current.TermSpecs)
                        removal.Remove_Item(oldBox, "all", old.TermSpecs)
                        UIprocedures.Remove_Item(newBox, "all", current.TermSpecs)
                        Same(oldBox, newBox, old.TermSpecs, current.TermSpecs)
                    End Using
                Next
            End Sub)
    End Sub
    Private Shared Function Box(keys As String(), mask As Integer) As ListBox
        Dim b As New ListBox With {.SelectionMode = SelectionMode.MultiExtended}
        b.Items.AddRange(keys.Cast(Of Object)().ToArray())
        For i = keys.Length - 1 To 0 Step -1
            If (mask And (1 << i)) <> 0 Then b.SetSelected(i, True)
        Next
        Return b
    End Function
    Private Shared Sub Same(oldBox As ListBox, newBox As ListBox, oldSpecs As Dictionary(Of String, TermSpec), newSpecs As Dictionary(Of String, TermSpec))
        CollectionAssert.AreEqual(oldBox.Items.Cast(Of String)().ToArray(), newBox.Items.Cast(Of String)().ToArray())
        Assert.AreEqual(RegressionEffectSerialization.Serialize(RegressionEffectsController.Snapshot(oldBox, oldSpecs)),
                        RegressionEffectSerialization.Serialize(RegressionEffectsController.Snapshot(newBox, newSpecs)))
        CollectionAssert.AreEqual(RegressionDesignCore.GetRequiredRawVarKeys(oldBox.Items, oldSpecs).ToArray(), RegressionDesignCore.GetRequiredRawVarKeys(newBox.Items, newSpecs).ToArray())
    End Sub
    <DataTestMethod> <DataRow(0)> <DataRow(1)> <DataRow(2)> <DataRow(3)> <DataRow(5)> <DataRow(7)> <DataRow(15)>
    Public Sub FrozenOriginalAllOperationsPartialSuccessAndSourceOrder(mask As Integer)
        Sta(Sub()
                For Each culture In {"en-US", "sk-SK"}
                    CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(culture)
                    For seed = 0 To 2
                        For operation = 0 To 4
                            Dim names = {"Age | VarB", "a | VarC", "A | VarD", "λ'雪:Q | VarE"}
                            Using variables = Box(names, mask), oldBox = Box({}, 0), newBox = Box({}, 0)
                                Dim oldSpecs As New Dictionary(Of String, TermSpec)(StringComparer.Ordinal), newSpecs As New Dictionary(Of String, TermSpec)(StringComparer.Ordinal)
                                Dim old As New LegacyRegressionEffects08(variables, oldBox, oldSpecs)
                                Dim current As New RegressionEffectsController(variables, newBox, newSpecs)
                                Dim messages As New List(Of String), messageModels As New List(Of String)
                                current.PresentMessage = Sub(m)
                                                             messages.Add(m)
                                                             messageModels.Add(RegressionEffectSerialization.Serialize(RegressionEffectsController.Snapshot(newBox, newSpecs)))
                                                         End Sub
                                If seed > 0 Then
                                    variables.ClearSelected() : variables.SetSelected(0, True)
                                    If seed = 1 Then
                                        old.AddCategoricalEffectsFromSelectedVars() : current.AddCategoricalEffectsFromSelectedVars()
                                    Else
                                        old.AddMainEffectsFromSelectedVars() : current.AddMainEffectsFromSelectedVars()
                                    End If
                                    variables.ClearSelected()
                                    For i = 3 To 0 Step -1
                                        If (mask And (1 << i)) <> 0 Then variables.SetSelected(i, True)
                                    Next
                                End If
                                For repeat = 0 To 1
                                    Select Case operation
                                        Case 0
                                            old.AddMainEffectsFromSelectedVars() : current.AddMainEffectsFromSelectedVars()
                                        Case 1
                                            old.AddCategoricalEffectsFromSelectedVars() : current.AddCategoricalEffectsFromSelectedVars()
                                        Case 2
                                            old.AddPolynomialEffectsFromSelectedVars(3) : current.AddPolynomialEffectsFromSelectedVars(3)
                                        Case 3
                                            old.AddTwoWayInteractionsFromSelectedVars() : current.AddTwoWayInteractionsFromSelectedVars()
                                        Case 4
                                            old.AddCustomInteractionFromSelectedVars() : current.AddCustomInteractionFromSelectedVars()
                                    End Select
                                    Same(oldBox, newBox, oldSpecs, newSpecs)
                                    CollectionAssert.AreEqual(old.Messages.ToArray(), messages.ToArray())
                                    CollectionAssert.AreEqual(old.MessageModels.ToArray(), messageModels.ToArray())
                                    Assert.AreSame(newSpecs, current.TermSpecs)
                                Next
                            End Using
                        Next
                    Next
                Next
            End Sub)
    End Sub
    <DataTestMethod> <DataRow("GLM-primary")> <DataRow("GLM-logistic")> <DataRow("GEE")> <DataRow("Cox")>
    <DataRow("LMM-fixed")> <DataRow("LMM-random")> <DataRow("MMRM")> <DataRow("PSM")>
    Public Sub SharedDictionaryRemovalClearRefreshAndIndependentComponents(consumer As String)
        Sta(Sub()
                Using variables = Box({"A", "B", "C"}, 7), effects = Box({}, 0), second = Box({}, 0)
                    Dim specs As New Dictionary(Of String, TermSpec)(StringComparer.Ordinal), otherSpecs As New Dictionary(Of String, TermSpec)(StringComparer.Ordinal)
                    Dim a As New RegressionEffectsController(variables, effects, specs), b As New RegressionEffectsController(variables, second, otherSpecs)
                    a.AddMainEffectsFromSelectedVars() : a.AddTwoWayInteractionsFromSelectedVars()
                    b.AddCustomInteractionFromSelectedVars()
                    effects.SetSelected(0, True)
                    UIprocedures.Remove_Item(effects, "selected", specs)
                    CollectionAssert.AreEqual({"B", "C", """B"":""C"""}, effects.Items.Cast(Of String)().ToArray(), consumer)
                    CollectionAssert.AreEqual({0, 1, 2}, effects.Items.Cast(Of String)().Select(Function(k) specs(k).Order).ToArray())
                    Assert.AreEqual(1, otherSpecs.Count) : Assert.AreSame(specs, a.TermSpecs)
                    UIprocedures.Remove_Item(effects, "all", specs)
                    Assert.AreEqual(0, specs.Count) : Assert.AreEqual(1, second.Items.Count)
                    variables.Items.Clear() : variables.Items.Add("New name | VarF") : variables.SetSelected(0, True)
                    Dim replacement As New Dictionary(Of String, TermSpec)(StringComparer.Ordinal)
                    a.TermSpecs = replacement : a.AddMainEffectsFromSelectedVars()
                    Assert.AreSame(replacement, a.TermSpecs) : Assert.IsTrue(replacement.ContainsKey("New name | VarF"))
                    Dim frozen = RegressionEffectsController.Snapshot(effects, replacement)
                    effects.Items.Clear() : replacement.Values.First().BaseVarKeys.Clear()
                    Assert.AreEqual("New name | VarF", frozen.Effects(0).Spec.BaseVarKeys.Single())
                    a.TermSpecs = Nothing : Assert.IsNotNull(a.TermSpecs)
                End Using
            End Sub)
    End Sub
    <DataTestMethod> <DataRow(0)> <DataRow(1)> <DataRow(2)> <DataRow(Integer.MaxValue)>
    Public Sub PolynomialErrorsAndExtremeDegreeMatchOriginal(degree As Integer)
        Sta(Sub()
                Using variables = Box({"A"}, 1), oldBox = Box({}, 0), newBox = Box({}, 0)
                    Dim old As New LegacyRegressionEffects08(variables, oldBox), current As New RegressionEffectsController(variables, newBox)
                    Dim messages As New List(Of String) : current.PresentMessage = Sub(m) messages.Add(m)
                    old.AddPolynomialEffectsFromSelectedVars(degree) : current.AddPolynomialEffectsFromSelectedVars(degree)
                    Same(oldBox, newBox, old.TermSpecs, current.TermSpecs)
                    CollectionAssert.AreEqual(old.Messages.ToArray(), messages.ToArray())
                End Using
            End Sub)
    End Sub
End Class
