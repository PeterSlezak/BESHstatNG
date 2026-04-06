Option Explicit On

Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Windows.Forms

''' <summary>
''' Provides shared authoring logic for regression effect terms backed by the
''' selected-variables list box, the selected-effects list box, and a
''' <see cref="TermSpec"/> dictionary.
''' </summary>
''' <remarks>
''' This controller is intended to centralize the effect-construction rules that
''' are currently implemented in the linear model UI so that the same behavior
''' can later be reused by GLM, GEE, Cox, and other regression forms.
''' </remarks>
Friend Class RegressionEffectsController

    Private ReadOnly _selectedVariables As ListBox
    Private ReadOnly _selectedEffects As ListBox
    Private _termSpecs As Dictionary(Of String, TermSpec)

    ''' <summary>
    ''' Initializes a new instance of the <see cref="RegressionEffectsController"/> class.
    ''' </summary>
    ''' <param name="selectedVariables">
    ''' The list box containing the available or selected raw variables that can be used
    ''' to construct model effects.
    ''' </param>
    ''' <param name="selectedEffects">
    ''' The list box containing the authored model effects.
    ''' </param>
    ''' <param name="existingTermSpecs">
    ''' An optional existing term-specification dictionary to reuse. If omitted,
    ''' a new empty dictionary is created.
    ''' </param>
    ''' <exception cref="ArgumentNullException">
    ''' Thrown when <paramref name="selectedVariables"/> or
    ''' <paramref name="selectedEffects"/> is <see langword="Nothing"/>.
    ''' </exception>
    Public Sub New(selectedVariables As ListBox,
                   selectedEffects As ListBox,
                   Optional existingTermSpecs As Dictionary(Of String, TermSpec) = Nothing)

        If selectedVariables Is Nothing Then Throw New ArgumentNullException(NameOf(selectedVariables))
        If selectedEffects Is Nothing Then Throw New ArgumentNullException(NameOf(selectedEffects))

        _selectedVariables = selectedVariables
        _selectedEffects = selectedEffects
        _termSpecs = If(existingTermSpecs,
                        New Dictionary(Of String, TermSpec)(StringComparer.Ordinal))
    End Sub

    ''' <summary>
    ''' Gets or sets the term-specification dictionary keyed by effect key.
    ''' </summary>
    ''' <value>
    ''' A dictionary containing one <see cref="TermSpec"/> entry per
    ''' authored effect.
    ''' </value>
    Public Property TermSpecs As Dictionary(Of String, TermSpec)
        Get
            If _termSpecs Is Nothing Then
                _termSpecs = New Dictionary(Of String, TermSpec)(StringComparer.Ordinal)
            End If
            Return _termSpecs
        End Get
        Set(value As Dictionary(Of String, TermSpec))
            _termSpecs = If(value,
                            New Dictionary(Of String, TermSpec)(StringComparer.Ordinal))
        End Set
    End Property

    ''' <summary>
    ''' Ensures that the backing term-specification dictionary has been initialized.
    ''' </summary>
    Private Sub EnsureTermSpecs()
        If _termSpecs Is Nothing Then
            _termSpecs = New Dictionary(Of String, TermSpec)(StringComparer.Ordinal)
        End If
    End Sub

    ''' <summary>
    ''' Determines whether the specified base variable already has a categorical
    ''' main-effect entry in the term-specification dictionary.
    ''' </summary>
    ''' <param name="baseKey">
    ''' The raw variable key to test.
    ''' </param>
    ''' <returns>
    ''' <see langword="True"/> if a categorical main effect exists for the variable;
    ''' otherwise, <see langword="False"/>.
    ''' </returns>
    Private Function HasCategoricalMainEffect(baseKey As String) As Boolean
        If _termSpecs Is Nothing Then Return False

        Dim catKey As String = RegressionDesignCore.MakeCategoricalEffectKey(baseKey)
        Return _termSpecs.ContainsKey(catKey)
    End Function

    ''' <summary>
    ''' Determines whether the specified base variable is already referenced by any
    ''' authored term in the current term-specification dictionary.
    ''' </summary>
    ''' <param name="baseKey">
    ''' The raw variable key to search for.
    ''' </param>
    ''' <returns>
    ''' <see langword="True"/> if the variable appears in any term's
    ''' <c>BaseVarKeys</c> collection; otherwise, <see langword="False"/>.
    ''' </returns>
    Private Function BaseVarUsedInAnyOtherTerm(baseKey As String) As Boolean
        If _termSpecs Is Nothing Then Return False

        For Each kvp In _termSpecs
            Dim spec = kvp.Value
            If spec Is Nothing OrElse spec.BaseVarKeys Is Nothing Then Continue For

            If spec.BaseVarKeys.Any(Function(x) String.Equals(x, baseKey, StringComparison.Ordinal)) Then
                Return True
            End If
        Next

        Return False
    End Function

    ''' <summary>
    ''' Determines whether any currently selected raw variables are already present
    ''' in the model as categorical effects.
    ''' </summary>
    ''' <returns>
    ''' <see langword="True"/> if at least one selected variable has a categorical
    ''' main-effect entry; otherwise, <see langword="False"/>.
    ''' </returns>
    Private Function SelectedVarsContainCategoricalUsage() As Boolean
        If _termSpecs Is Nothing Then Return False

        For Each it As Object In _selectedVariables.SelectedItems
            Dim bk As String = CStr(it)
            If HasCategoricalMainEffect(bk) Then Return True
        Next

        Return False
    End Function

    ''' <summary>
    ''' Adds continuous main effects for all currently selected variables.
    ''' </summary>
    ''' <remarks>
    ''' A variable is skipped if it is already present as a categorical effect or
    ''' if the exact continuous main effect already exists in the selected-effects list.
    ''' </remarks>
    Public Sub AddMainEffectsFromSelectedVars()
        If _selectedVariables.SelectedItems.Count = 0 Then
            MsgBox("Please select variable(s).", vbExclamation, "Input Error!")
            Exit Sub
        End If

        EnsureTermSpecs()

        For Each it As Object In _selectedVariables.SelectedItems
            Dim baseKey As String = CStr(it)

            If HasCategoricalMainEffect(baseKey) Then
                MsgBox("Variable '" & RegressionDesignCore.GetCoefBaseName(baseKey) &
                       "' is already marked as categorical. Remove it first if you want to use it as continuous.",
                       vbExclamation, "Input Error!")
                Continue For
            End If

            If _selectedEffects.Items.Contains(baseKey) Then Continue For

            _selectedEffects.Items.Add(baseKey)

            Dim spec As New TermSpec With {
                .Kind = "MainEffect",
                .BaseVarKeys = New List(Of String) From {baseKey},
                .Degree = 1,
                .DisplayNameForCoef = RegressionDesignCore.GetCoefBaseName(baseKey),
                .Order = _selectedEffects.Items.Count - 1,
                .Scale = PredictorScale.Continuous
            }

            _termSpecs(baseKey) = spec
        Next
    End Sub

    ''' <summary>
    ''' Adds categorical main effects for all currently selected variables.
    ''' </summary>
    ''' <remarks>
    ''' A variable is rejected when it is already used by another authored term and
    ''' does not already have its own categorical main-effect entry.
    ''' </remarks>
    Public Sub AddCategoricalEffectsFromSelectedVars()
        If _selectedVariables.SelectedItems.Count = 0 Then
            MsgBox("Please select variable(s).", vbExclamation, "Input Error!")
            Exit Sub
        End If

        EnsureTermSpecs()

        For Each it As Object In _selectedVariables.SelectedItems
            Dim baseKey As String = CStr(it)
            Dim termKey As String = RegressionDesignCore.MakeCategoricalEffectKey(baseKey)

            If BaseVarUsedInAnyOtherTerm(baseKey) AndAlso Not _termSpecs.ContainsKey(termKey) Then
                MsgBox("Variable '" & RegressionDesignCore.GetCoefBaseName(baseKey) &
                       "' is already used in the model. Remove its existing term(s) before adding it as categorical.",
                       vbExclamation, "Input Error!")
                Continue For
            End If

            If _selectedEffects.Items.Contains(termKey) Then Continue For

            _selectedEffects.Items.Add(termKey)

            Dim spec As New TermSpec With {
                .Kind = "MainEffect",
                .BaseVarKeys = New List(Of String) From {baseKey},
                .Degree = 1,
                .DisplayNameForCoef = RegressionDesignCore.GetCoefBaseName(baseKey),
                .Order = _selectedEffects.Items.Count - 1,
                .Scale = PredictorScale.Categorical
            }

            _termSpecs(termKey) = spec
        Next
    End Sub

    ''' <summary>
    ''' Adds polynomial effects of the specified degree for all currently selected variables.
    ''' </summary>
    ''' <param name="degree">
    ''' The polynomial degree to apply. Must be greater than or equal to 2.
    ''' </param>
    ''' <remarks>
    ''' Polynomial effects are only allowed for continuous predictors. Variables that
    ''' are currently marked as categorical are rejected.
    ''' </remarks>
    Public Sub AddPolynomialEffectsFromSelectedVars(degree As Integer)
        If degree < 2 Then
            MsgBox("Polynomial degree must be >= 2.", vbExclamation, "Input Error!")
            Exit Sub
        End If

        If _selectedVariables.SelectedItems.Count = 0 Then
            MsgBox("Please select variable(s).", vbExclamation, "Input Error!")
            Exit Sub
        End If

        EnsureTermSpecs()

        For Each it As Object In _selectedVariables.SelectedItems
            Dim baseKey As String = CStr(it)

            If HasCategoricalMainEffect(baseKey) Then
                MsgBox("Polynomial terms are not supported for categorical predictors.", vbExclamation, "Input Error!")
                Continue For
            End If

            Dim termKey As String = RegressionDesignCore.MakePolynomialEffectKey(baseKey, degree)

            If _selectedEffects.Items.Contains(termKey) Then Continue For

            _selectedEffects.Items.Add(termKey)

            Dim spec As New TermSpec With {
                .Kind = "Polynomial",
                .BaseVarKeys = New List(Of String) From {baseKey},
                .Degree = degree,
                .DisplayNameForCoef = RegressionDesignCore.GetCoefBaseName(baseKey) & "^" & CStr(degree),
                .Order = _selectedEffects.Items.Count - 1,
                .Scale = PredictorScale.Continuous
            }

            _termSpecs(termKey) = spec
        Next
    End Sub

    ''' <summary>
    ''' Adds all pairwise interaction effects for the currently selected variables.
    ''' </summary>
    ''' <remarks>
    ''' This method currently supports continuous-by-continuous interactions only.
    ''' If any selected variable is already used as categorical, the operation is rejected.
    ''' </remarks>
    Public Sub AddTwoWayInteractionsFromSelectedVars()
        If _selectedVariables.SelectedItems.Count < 2 Then
            MsgBox("Please select at least two variables.", vbExclamation, "Input Error!")
            Exit Sub
        End If

        If SelectedVarsContainCategoricalUsage() Then
            MsgBox("Interactions involving categorical predictors are not implemented yet.", vbExclamation, "Input Error!")
            Exit Sub
        End If

        EnsureTermSpecs()

        Dim idxs As New List(Of Integer)
        For Each ix As Integer In _selectedVariables.SelectedIndices
            idxs.Add(ix)
        Next
        idxs.Sort()

        For a As Integer = 0 To idxs.Count - 2
            Dim v1 As String = CStr(_selectedVariables.Items(idxs(a)))

            For b As Integer = a + 1 To idxs.Count - 1
                Dim v2 As String = CStr(_selectedVariables.Items(idxs(b)))

                Dim termKey As String = RegressionDesignCore.MakeInteractionEffectKey(New List(Of String) From {v1, v2})

                If _selectedEffects.Items.Contains(termKey) Then Continue For

                _selectedEffects.Items.Add(termKey)

                Dim spec As New TermSpec With {
                    .Kind = "Interaction",
                    .BaseVarKeys = New List(Of String) From {v1, v2},
                    .Degree = 1,
                    .DisplayNameForCoef = RegressionDesignCore.MakeInteractionCoefName(New List(Of String) From {v1, v2}),
                    .Order = _selectedEffects.Items.Count - 1,
                    .Scale = PredictorScale.Continuous
                }

                _termSpecs(termKey) = spec
            Next
        Next
    End Sub

    ''' <summary>
    ''' Adds a single interaction effect containing all currently selected variables.
    ''' </summary>
    ''' <remarks>
    ''' This method creates one multi-way interaction term spanning the entire current
    ''' selection. As with pairwise interactions, categorical involvement is currently
    ''' rejected.
    ''' </remarks>
    Public Sub AddCustomInteractionFromSelectedVars()
        If _selectedVariables.SelectedItems.Count < 2 Then
            MsgBox("Please select at least two variables.", vbExclamation, "Input Error!")
            Exit Sub
        End If

        If SelectedVarsContainCategoricalUsage() Then
            MsgBox("Interactions involving categorical predictors are not implemented yet.", vbExclamation, "Input Error!")
            Exit Sub
        End If

        EnsureTermSpecs()

        Dim idxs As New List(Of Integer)
        For Each ix As Integer In _selectedVariables.SelectedIndices
            idxs.Add(ix)
        Next
        idxs.Sort()

        Dim baseKeys As New List(Of String)
        For Each ix As Integer In idxs
            Dim v As String = CStr(_selectedVariables.Items(ix))
            baseKeys.Add(v)
        Next

        Dim termKey As String = RegressionDesignCore.MakeInteractionEffectKey(baseKeys)

        If _selectedEffects.Items.Contains(termKey) Then Exit Sub

        _selectedEffects.Items.Add(termKey)

        Dim spec As New TermSpec With {
            .Kind = "Interaction",
            .BaseVarKeys = baseKeys,
            .Degree = 1,
            .DisplayNameForCoef = RegressionDesignCore.MakeInteractionCoefName(baseKeys),
            .Order = _selectedEffects.Items.Count - 1,
            .Scale = PredictorScale.Continuous
        }

        _termSpecs(termKey) = spec
    End Sub

End Class