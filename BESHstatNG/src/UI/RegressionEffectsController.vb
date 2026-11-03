Option Explicit On
Option Strict On

Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Windows.Forms
Imports BESHStatNG.ExcelCommon

''' <summary>Windows snapshot, projection and message boundary for the portable authoring policy.</summary>
Friend Class RegressionEffectsController
    Private ReadOnly _selectedVariables As ListBox
    Private ReadOnly _selectedEffects As ListBox
    Private _termSpecs As Dictionary(Of String, TermSpec)
    Friend Property PresentMessage As Action(Of String) = Sub(message) MsgBox(message, vbExclamation, "Input Error!")

    Public Sub New(selectedVariables As ListBox, selectedEffects As ListBox,
                   Optional existingTermSpecs As Dictionary(Of String, TermSpec) = Nothing)
        If selectedVariables Is Nothing Then Throw New ArgumentNullException(NameOf(selectedVariables))
        If selectedEffects Is Nothing Then Throw New ArgumentNullException(NameOf(selectedEffects))
        _selectedVariables = selectedVariables : _selectedEffects = selectedEffects
        TermSpecs = existingTermSpecs
    End Sub
    Public Property TermSpecs As Dictionary(Of String, TermSpec)
        Get
            Return _termSpecs
        End Get
        Set(value As Dictionary(Of String, TermSpec))
            _termSpecs = If(value, New Dictionary(Of String, TermSpec)(StringComparer.Ordinal))
        End Set
    End Property
    Public Sub AddMainEffectsFromSelectedVars()
        Apply(RegressionEffectOperation.ContinuousMain)
    End Sub
    Public Sub AddCategoricalEffectsFromSelectedVars()
        Apply(RegressionEffectOperation.CategoricalMain)
    End Sub
    Public Sub AddPolynomialEffectsFromSelectedVars(degree As Integer)
        Apply(RegressionEffectOperation.Polynomial, degree)
    End Sub
    Public Sub AddTwoWayInteractionsFromSelectedVars()
        Apply(RegressionEffectOperation.PairwiseInteractions)
    End Sub
    Public Sub AddCustomInteractionFromSelectedVars()
        Apply(RegressionEffectOperation.CustomInteraction)
    End Sub
    Private Sub Apply(operation As RegressionEffectOperation, Optional degree As Integer = 2)
        Dim catalog = _selectedVariables.Items.Cast(Of String)().ToArray()
        Dim indices = _selectedVariables.SelectedIndices.Cast(Of Integer)().ToArray()
        Dim result = RegressionEffectAuthoringPolicy.Apply(catalog, indices, Snapshot(_selectedEffects, TermSpecs), operation, degree)
        For Each issue In result.Diagnostics
            If issue.PresentationSnapshot IsNot Nothing Then Project(_selectedEffects, TermSpecs, issue.PresentationSnapshot)
            PresentMessage.Invoke(issue.Message)
        Next
        Project(_selectedEffects, TermSpecs, result.Model)
    End Sub
    Friend Shared Function Snapshot(effects As ListBox, specs As Dictionary(Of String, TermSpec)) As RegressionEffectModel
        Return RegressionEffectModel.FromProjection(effects.Items.Cast(Of String)().ToArray(), specs)
    End Function
    Friend Shared Sub Project(effects As ListBox, specs As Dictionary(Of String, TermSpec), model As RegressionEffectModel)
        Dim keys = model.Effects.Select(Function(e) e.Key).ToArray()
        If Not effects.Items.Cast(Of String)().SequenceEqual(keys, StringComparer.Ordinal) Then
            Dim selected = effects.SelectedItems.Cast(Of String)().ToArray()
            effects.BeginUpdate()
            Try
                For i = effects.Items.Count - 1 To 0 Step -1
                    If Not keys.Contains(CStr(effects.Items(i)), StringComparer.Ordinal) Then effects.Items.RemoveAt(i)
                Next
                ' Ordinary additions/removals preserve surviving item/selection state.
                If Not effects.Items.Cast(Of String)().SequenceEqual(keys.Take(effects.Items.Count), StringComparer.Ordinal) Then effects.Items.Clear()
                For Each key In keys.Skip(effects.Items.Count)
                    effects.Items.Add(key)
                Next
                For i = 0 To effects.Items.Count - 1
                    If selected.Contains(CStr(effects.Items(i)), StringComparer.Ordinal) Then effects.SetSelected(i, True)
                Next
            Finally
                effects.EndUpdate()
            End Try
        End If
        specs.Clear()
        For Each entry In model.ToTermSpecs()
            specs.Add(entry.Key, entry.Value)
        Next
    End Sub
    Friend Shared Sub Remove(effects As ListBox, specs As Dictionary(Of String, TermSpec), clear As Boolean)
        Dim result = RegressionEffectAuthoringPolicy.Remove(Snapshot(effects, specs), effects.SelectedItems.Cast(Of String)().ToArray(), clear)
        Project(effects, specs, result.Model)
        For Each issue In result.Diagnostics
            MsgBox(issue.Message, vbExclamation, "Input Error!")
        Next
    End Sub
End Class
