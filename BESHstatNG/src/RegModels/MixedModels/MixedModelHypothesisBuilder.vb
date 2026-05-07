Option Explicit On
Option Strict On

Imports System
Imports System.Collections.Generic

Namespace regression

    ''' <summary>
    ''' Reusable helpers for constructing linear and multi-df hypotheses from
    ''' mixed-model fixed-effect coefficient names.
    ''' </summary>
    ''' <remarks>
    ''' This module is intentionally independent of MMRM/LMM/UI details.  It can be
    ''' reused by:
    '''
    '''   - internal KR term-level F-test validation,
    '''   - future UDFs,
    '''   - future GUI term-level tests,
    '''   - post-estimation and reference-grid utilities.
    '''
    ''' The grouping convention is coefficient-name based:
    '''
    '''   visit=2, visit=3             -> term "visit"
    '''   visit[2], visit[3]           -> term "visit"
    '''   trt:visit=2, trt:visit=3     -> term "trt:visit"
    '''
    ''' Interaction matching is order-insensitive:
    '''
    '''   "visit:trt" and "trt:visit" are treated as the same term key.
    ''' </remarks>
    Public Module MixedModelHypothesisBuilder

        Private Const INTERCEPT_TERM As String = "(Intercept)"


        ''' <summary>
        ''' Normalizes a coefficient or term name to a canonical term key.
        ''' </summary>
        Public Function NormalizeTermKey(termName As String) As String
            Dim raw As String = If(termName, String.Empty).Trim()
            If raw.Length = 0 Then Return String.Empty

            Dim parts() As String = raw.Split(":"c)
            Dim atoms As New List(Of String)()

            For Each part As String In parts
                Dim atom As String = NormalizeTermAtom(part)
                If String.IsNullOrWhiteSpace(atom) Then Continue For
                atoms.Add(atom)
            Next

            If atoms.Count = 0 Then Return String.Empty
            If atoms.Count = 1 Then Return atoms(0)

            atoms.Sort(StringComparer.OrdinalIgnoreCase)
            Return String.Join(":", atoms.ToArray())
        End Function


        ''' <summary>
        ''' Returns the unique fixed-effect term names represented by the coefficient
        ''' vector, in first-appearance order.
        ''' </summary>
        Public Function GetTermNames(fixedEffectNames() As String,
                                     Optional includeIntercept As Boolean = False) As String()
            Dim out As New List(Of String)()

            If fixedEffectNames Is Nothing Then Return out.ToArray()

            For Each coefName As String In fixedEffectNames
                Dim key As String = NormalizeTermKey(coefName)

                If String.IsNullOrWhiteSpace(key) Then Continue For
                If IsInterceptTerm(key) AndAlso Not includeIntercept Then Continue For

                If Not ContainsIgnoreCase(out, key) Then out.Add(key)
            Next

            Return out.ToArray()
        End Function


        ''' <summary>
        ''' Builds one multi-df hypothesis per fixed-effect term.
        ''' </summary>
        Public Function BuildTermHypotheses(fixedEffectNames() As String,
                                            Optional includeIntercept As Boolean = False) As List(Of MixedModelMultiDfHypothesis)
            Dim out As New List(Of MixedModelMultiDfHypothesis)()

            If fixedEffectNames Is Nothing OrElse fixedEffectNames.Length = 0 Then Return out

            Dim keyOrder As New List(Of String)()
            Dim groups As New Dictionary(Of String, List(Of Integer))(StringComparer.OrdinalIgnoreCase)

            For j As Integer = 0 To fixedEffectNames.Length - 1
                Dim key As String = NormalizeTermKey(fixedEffectNames(j))

                If String.IsNullOrWhiteSpace(key) Then Continue For
                If IsInterceptTerm(key) AndAlso Not includeIntercept Then Continue For

                If Not groups.ContainsKey(key) Then
                    groups(key) = New List(Of Integer)()
                    keyOrder.Add(key)
                End If

                groups(key).Add(j)
            Next

            For Each key As String In keyOrder
                Dim h As MixedModelMultiDfHypothesis = Nothing

                If TryBuildHypothesisFromCoefficientIndices(key,
                                                            fixedEffectNames.Length,
                                                            groups(key).ToArray(),
                                                            h) Then
                    out.Add(h)
                End If
            Next

            Return out
        End Function


        ''' <summary>
        ''' Builds a multi-df hypothesis for one named fixed-effect term.
        ''' </summary>
        Public Function TryBuildTermHypothesis(fixedEffectNames() As String,
                                               termName As String,
                                               ByRef hypothesis As MixedModelMultiDfHypothesis,
                                               Optional includeIntercept As Boolean = True,
                                               Optional ByRef diagnostic As String = Nothing) As Boolean
            hypothesis = Nothing
            diagnostic = String.Empty

            If fixedEffectNames Is Nothing OrElse fixedEffectNames.Length = 0 Then
                diagnostic = "Fixed-effect names are missing."
                Return False
            End If

            Dim targetKey As String = NormalizeTermKey(termName)

            If String.IsNullOrWhiteSpace(targetKey) Then
                diagnostic = "Term name is empty."
                Return False
            End If

            If IsInterceptTerm(targetKey) AndAlso Not includeIntercept Then
                diagnostic = "Intercept term was requested but includeIntercept=False."
                Return False
            End If

            Dim indices As New List(Of Integer)()

            For j As Integer = 0 To fixedEffectNames.Length - 1
                Dim key As String = NormalizeTermKey(fixedEffectNames(j))

                If String.Equals(key, targetKey, StringComparison.OrdinalIgnoreCase) Then
                    indices.Add(j)
                End If
            Next

            If indices.Count = 0 Then
                diagnostic = "No fixed-effect coefficients matched term '" & termName & "'."
                Return False
            End If

            Return TryBuildHypothesisFromCoefficientIndices(targetKey,
                                                           fixedEffectNames.Length,
                                                           indices.ToArray(),
                                                           hypothesis,
                                                           diagnostic)
        End Function


        ''' <summary>
        ''' Builds one one-row linear hypothesis per coefficient.
        ''' </summary>
        Public Function BuildCoefficientLinearHypotheses(fixedEffectNames() As String,
                                                         Optional includeIntercept As Boolean = True) As List(Of MixedModelLinearHypothesis)
            Dim out As New List(Of MixedModelLinearHypothesis)()

            If fixedEffectNames Is Nothing OrElse fixedEffectNames.Length = 0 Then Return out

            For j As Integer = 0 To fixedEffectNames.Length - 1
                Dim key As String = NormalizeTermKey(fixedEffectNames(j))

                If IsInterceptTerm(key) AndAlso Not includeIntercept Then Continue For

                Dim l(fixedEffectNames.Length - 1) As Double
                l(j) = 1.0

                out.Add(New MixedModelLinearHypothesis(If(fixedEffectNames(j), "b" & j.ToString()), l))
            Next

            Return out
        End Function


        ''' <summary>
        ''' Builds a one-row linear hypothesis for one exact coefficient name.
        ''' </summary>
        Public Function TryBuildCoefficientLinearHypothesis(fixedEffectNames() As String,
                                                           coefficientName As String,
                                                           ByRef hypothesis As MixedModelLinearHypothesis,
                                                           Optional ByRef diagnostic As String = Nothing) As Boolean
            hypothesis = Nothing
            diagnostic = String.Empty

            If fixedEffectNames Is Nothing OrElse fixedEffectNames.Length = 0 Then
                diagnostic = "Fixed-effect names are missing."
                Return False
            End If

            For j As Integer = 0 To fixedEffectNames.Length - 1
                If String.Equals(If(fixedEffectNames(j), String.Empty).Trim(),
                                 If(coefficientName, String.Empty).Trim(),
                                 StringComparison.OrdinalIgnoreCase) Then

                    Dim l(fixedEffectNames.Length - 1) As Double
                    l(j) = 1.0
                    hypothesis = New MixedModelLinearHypothesis(fixedEffectNames(j), l)
                    Return True
                End If
            Next

            diagnostic = "No fixed-effect coefficient matched '" & coefficientName & "'."
            Return False
        End Function


        ''' <summary>
        ''' Convenience wrapper that builds term hypotheses from a fitted result and
        ''' returns a multi-df KR validation table.
        ''' </summary>
        Public Function BuildTermMultiDfInferenceTable(modelResult As MixedModelResult,
                                                       Optional includeIntercept As Boolean = False,
                                                       Optional alpha As Double = 0.05,
                                                       Optional title As String = "Kenward-Roger term-level F tests") As Global.BESHStatNG.ResultTable
            If modelResult Is Nothing OrElse modelResult.FixedEffectNames Is Nothing Then Return Nothing

            Dim hyps As List(Of MixedModelMultiDfHypothesis) =
                BuildTermHypotheses(modelResult.FixedEffectNames, includeIntercept)

            If hyps Is Nothing OrElse hyps.Count = 0 Then Return Nothing

            Return MixedModelKenwardRogerInference.BuildMultiDfInferenceTable(modelResult,
                                                                              hyps,
                                                                              alpha,
                                                                              title)
        End Function


        ''' <summary>
        ''' Builds a hypothesis matrix with selector rows for the supplied coefficient
        ''' indices.
        ''' </summary>
        Public Function TryBuildHypothesisFromCoefficientIndices(label As String,
                                                                 coefficientCount As Integer,
                                                                 coefficientIndices() As Integer,
                                                                 ByRef hypothesis As MixedModelMultiDfHypothesis,
                                                                 Optional ByRef diagnostic As String = Nothing) As Boolean
            hypothesis = Nothing
            diagnostic = String.Empty

            If coefficientCount <= 0 Then
                diagnostic = "Coefficient count must be positive."
                Return False
            End If

            If coefficientIndices Is Nothing OrElse coefficientIndices.Length = 0 Then
                diagnostic = "Coefficient index list is empty."
                Return False
            End If

            Dim l(coefficientIndices.Length - 1, coefficientCount - 1) As Double

            For r As Integer = 0 To coefficientIndices.Length - 1
                Dim j As Integer = coefficientIndices(r)

                If j < 0 OrElse j >= coefficientCount Then
                    diagnostic = "Coefficient index " & j.ToString() & " is outside the valid range."
                    Return False
                End If

                l(r, j) = 1.0
            Next

            hypothesis = New MixedModelMultiDfHypothesis(If(label, String.Empty), l)
            Return True
        End Function


        Private Function NormalizeTermAtom(rawAtom As String) As String
            Dim atom As String = If(rawAtom, String.Empty).Trim()

            If atom.Length = 0 Then Return String.Empty

            If atom.StartsWith("'", StringComparison.Ordinal) AndAlso
               atom.EndsWith("'", StringComparison.Ordinal) AndAlso
               atom.Length >= 2 Then
                atom = atom.Substring(1, atom.Length - 2).Trim()
            End If

            If IsInterceptTerm(atom) OrElse String.Equals(atom, "1", StringComparison.OrdinalIgnoreCase) Then
                Return INTERCEPT_TERM
            End If

            Dim lb As Integer = atom.LastIndexOf("["c)
            Dim rb As Integer = atom.LastIndexOf("]"c)

            If lb > 0 AndAlso rb = atom.Length - 1 AndAlso rb > lb Then
                atom = atom.Substring(0, lb).Trim()
            End If

            Dim eqPos As Integer = atom.IndexOf("="c)

            If eqPos > 0 Then
                atom = atom.Substring(0, eqPos).Trim()
            End If

            If IsInterceptTerm(atom) OrElse String.Equals(atom, "1", StringComparison.OrdinalIgnoreCase) Then
                Return INTERCEPT_TERM
            End If

            Return atom
        End Function


        Private Function IsInterceptTerm(termKey As String) As Boolean
            Dim s As String = If(termKey, String.Empty).Trim()
            Return String.Equals(s, INTERCEPT_TERM, StringComparison.OrdinalIgnoreCase) OrElse
                   String.Equals(s, "Intercept", StringComparison.OrdinalIgnoreCase) OrElse
                   String.Equals(s, "(Intercept)", StringComparison.OrdinalIgnoreCase)
        End Function


        Private Function ContainsIgnoreCase(values As List(Of String),
                                            value As String) As Boolean
            If values Is Nothing Then Return False

            For Each existing As String In values
                If String.Equals(existing, value, StringComparison.OrdinalIgnoreCase) Then Return True
            Next

            Return False
        End Function

    End Module

End Namespace
