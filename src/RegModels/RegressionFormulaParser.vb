Option Explicit On
Option Strict On

Imports System
Imports System.Collections.Generic
Imports System.Globalization
Imports System.Linq

''' <summary>
''' Identifies the addressing mode used to resolve a formula variable reference.
''' </summary>
Public Enum RegressionFormulaTokenKind
    RelativeColumnLetter = 0
    AbsoluteColumnLetter = 1
    VariableName = 2
End Enum

''' <summary>
''' Represents one predictor available for formula parsing and design-matrix construction.
''' </summary>
Public Class RegressionVariableCatalogEntry
    Public Property BaseKey As String
    Public Property DisplayName As String
    Public Property RelativeColumnIndex As Integer
    Public Property RelativeColumnLetter As String
    Public Property AbsoluteColumnLetter As String
    Public Property VariableName As String
End Class

''' <summary>
''' Stores predictor metadata and resolves formula variable references to raw predictor columns.
''' </summary>
Public Class RegressionVariableCatalog

    Public Property AllowRelativeColumnLetters As Boolean = True
    Public Property AllowAbsoluteColumnLetters As Boolean = False
    Public Property AllowQuotedVariableNames As Boolean = True

    Public ReadOnly Property Variables As List(Of RegressionVariableCatalogEntry)

    ''' <summary>
    ''' Initializes a new instance of the containing type.
    ''' </summary>
    Public Sub New()
        Me.Variables = New List(Of RegressionVariableCatalogEntry)()
    End Sub

    ''' <summary>
    ''' Builds a variable catalog for formula parsing and design-matrix construction.
    ''' </summary>
    ''' <param name="varNames">Predictor display names in raw-column order.</param>
    ''' <param name="baseKeys">Internal base keys associated with each raw predictor column.</param>
    ''' <param name="absoluteColumnLetters">Absolute worksheet column letters aligned with the raw predictor columns.</param>
    ''' <param name="allowRelativeColumnLetters">Whether relative X-column letters such as A, B, or AA are allowed.</param>
    ''' <param name="allowAbsoluteColumnLetters">Whether absolute worksheet column letters are allowed.</param>
    ''' <param name="allowQuotedVariableNames">Whether quoted variable-name references are allowed.</param>
    ''' <returns>A populated variable catalog.</returns>
    Public Shared Function Build(varNames As IEnumerable(Of String),
                                 Optional baseKeys As IEnumerable(Of String) = Nothing,
                                 Optional absoluteColumnLetters As IEnumerable(Of String) = Nothing,
                                 Optional allowRelativeColumnLetters As Boolean = True,
                                 Optional allowAbsoluteColumnLetters As Boolean = False,
                                 Optional allowQuotedVariableNames As Boolean = True) As RegressionVariableCatalog

        Dim catalog As New RegressionVariableCatalog With {
            .AllowRelativeColumnLetters = allowRelativeColumnLetters,
            .AllowAbsoluteColumnLetters = allowAbsoluteColumnLetters,
            .AllowQuotedVariableNames = allowQuotedVariableNames
        }

        Dim names As List(Of String) = If(varNames, Enumerable.Empty(Of String)()).ToList()
        Dim keys As List(Of String) = If(baseKeys, Enumerable.Empty(Of String)()).ToList()
        Dim absCols As List(Of String) = If(absoluteColumnLetters, Enumerable.Empty(Of String)()).ToList()

        Dim p As Integer = names.Count
        If keys.Count > 0 AndAlso keys.Count <> p Then
            Throw New ArgumentException("baseKeys count must match varNames count.")
        End If
        If absCols.Count > 0 AndAlso absCols.Count <> p Then
            Throw New ArgumentException("absoluteColumnLetters count must match varNames count.")
        End If

        For i As Integer = 0 To p - 1
            Dim relLetter As String = NumberToLetters(i + 1)
            Dim nm As String = If(names(i), String.Empty).Trim()
            If nm = String.Empty Then nm = "X" & (i + 1).ToString(CultureInfo.InvariantCulture)

            Dim entry As New RegressionVariableCatalogEntry With {
                .BaseKey = If(keys.Count > 0, keys(i), relLetter),
                .DisplayName = nm,
                .RelativeColumnIndex = i + 1,
                .RelativeColumnLetter = relLetter,
                .AbsoluteColumnLetter = If(absCols.Count > 0, NormalizeLetters(absCols(i)), String.Empty),
                .VariableName = nm
            }

            catalog.Variables.Add(entry)
        Next

        Return catalog
    End Function

    ''' <summary>
    ''' Attempts to resolve a formula token to a variable-catalog entry.
    ''' </summary>
    ''' <param name="tokenText">The token text to resolve.</param>
    ''' <param name="entry">On success, receives the resolved variable-catalog entry.</param>
    ''' <param name="tokenKind">On success, receives the addressing mode used to resolve the token.</param>
    ''' <param name="errorMessage">On failure, receives a human-readable error message.</param>
    ''' <returns>True when the token resolves successfully; otherwise, False.</returns>
    Public Function TryResolveToken(tokenText As String,
                                    ByRef entry As RegressionVariableCatalogEntry,
                                    ByRef tokenKind As RegressionFormulaTokenKind,
                                    ByRef errorMessage As String) As Boolean

        entry = Nothing
        tokenKind = RegressionFormulaTokenKind.RelativeColumnLetter
        errorMessage = Nothing

        Dim token As String = If(tokenText, String.Empty).Trim()
        If token = String.Empty Then
            errorMessage = "Empty variable reference."
            Return False
        End If

        If IsQuotedToken(token) Then
            If Not Me.AllowQuotedVariableNames Then
                errorMessage = "Single-quoted variable names are not enabled in this variable catalog."
                Return False
            End If

            Dim variableName As String = StripOuterQuotes(token)
            Dim matches = Me.Variables.Where(Function(v) String.Equals(If(v.VariableName, String.Empty).Trim(),
                                                                       variableName.Trim(),
                                                                       StringComparison.OrdinalIgnoreCase)).ToList()
            If matches.Count = 0 Then
                errorMessage = "Unknown variable name '" & variableName & "'."
                Return False
            End If
            If matches.Count > 1 Then
                errorMessage = "Variable name '" & variableName & "' is ambiguous in the current variable catalog."
                Return False
            End If

            entry = matches(0)
            tokenKind = RegressionFormulaTokenKind.VariableName
            Return True
        End If

        Dim bare As String = NormalizeLetters(token)
        Dim relMatch As RegressionVariableCatalogEntry = Nothing
        Dim absMatch As RegressionVariableCatalogEntry = Nothing

        If Me.AllowRelativeColumnLetters Then
            relMatch = Me.Variables.FirstOrDefault(Function(v) String.Equals(v.RelativeColumnLetter, bare, StringComparison.OrdinalIgnoreCase))
        End If
        If Me.AllowAbsoluteColumnLetters Then
            absMatch = Me.Variables.FirstOrDefault(Function(v) String.Equals(v.AbsoluteColumnLetter, bare, StringComparison.OrdinalIgnoreCase))
        End If

        If relMatch IsNot Nothing AndAlso absMatch IsNot Nothing Then
            If Object.ReferenceEquals(relMatch, absMatch) OrElse String.Equals(relMatch.BaseKey, absMatch.BaseKey, StringComparison.Ordinal) Then
                entry = relMatch
                tokenKind = RegressionFormulaTokenKind.RelativeColumnLetter
                Return True
            End If

            errorMessage = "Bare token '" & bare & "' is ambiguous because it matches both a relative X-column and an absolute worksheet column. Use a single-quoted variable name or disable one addressing mode."
            Return False
        End If

        If relMatch IsNot Nothing Then
            entry = relMatch
            tokenKind = RegressionFormulaTokenKind.RelativeColumnLetter
            Return True
        End If

        If absMatch IsNot Nothing Then
            entry = absMatch
            tokenKind = RegressionFormulaTokenKind.AbsoluteColumnLetter
            Return True
        End If

        errorMessage = "Unknown variable reference '" & token & "'. Use relative column letters (A, B, ...), enabled absolute worksheet letters, or a single-quoted variable name."
        Return False
    End Function

    ''' <summary>
    ''' Normalizes a column-letter token for case-insensitive comparison.
    ''' </summary>
    ''' <param name="s">The text value to inspect.</param>
    ''' <returns>The normalized column-letter text.</returns>
    Private Shared Function NormalizeLetters(s As String) As String
        Return If(s, String.Empty).Trim().ToUpperInvariant()
    End Function

    ''' <summary>
    ''' Converts a 1-based column index to Excel-style letters.
    ''' </summary>
    ''' <param name="columnIndex1Based">The 1-based column index to convert.</param>
    ''' <returns>The Excel-style column letters for the supplied index.</returns>
    Public Shared Function NumberToLetters(columnIndex1Based As Integer) As String
        If columnIndex1Based <= 0 Then Throw New ArgumentOutOfRangeException(NameOf(columnIndex1Based))

        Dim n As Integer = columnIndex1Based
        Dim chars As New List(Of Char)()
        While n > 0
            n -= 1
            chars.Add(ChrW(AscW("A"c) + (n Mod 26)))
            n \= 26
        End While
        chars.Reverse()
        Return New String(chars.ToArray())
    End Function

    ''' <summary>
    ''' Determines whether a token is enclosed in single quotes.
    ''' </summary>
    ''' <param name="tokenText">The token text to resolve.</param>
    ''' <returns>True when the token is enclosed in single quotes; otherwise, False.</returns>
    Public Shared Function IsQuotedToken(tokenText As String) As Boolean
        Dim t As String = If(tokenText, String.Empty).Trim()
        Return t.Length >= 2 AndAlso t(0) = "'"c AndAlso t(t.Length - 1) = "'"c
    End Function

    ''' <summary>
    ''' Removes a single pair of outer single quotes from a token when present and unescapes doubled apostrophes inside the token body.
    ''' </summary>
    ''' <param name="tokenText">The token text to resolve.</param>
    ''' <returns>The token text without a single outer pair of single quotes.</returns>
    Public Shared Function StripOuterQuotes(tokenText As String) As String
        Dim t As String = If(tokenText, String.Empty).Trim()
        If IsQuotedToken(t) Then
            Return t.Substring(1, t.Length - 2).Replace("''", "'")
        End If
        Return t
    End Function
End Class

''' <summary>
''' Represents one parsed formula term before it is converted into the shared term-spec model.
''' </summary>
Public Class ParsedRegressionFormulaTerm
    Public Property Kind As String
    Public Property BaseVarKeys As List(Of String)
    Public Property Degree As Integer
    Public Property Scale As PredictorScale
    Public Property ReferenceValue As Nullable(Of Double)
    Public Property EffectKey As String
    Public Property DisplayNameForCoef As String
    Public Property NormalizedText As String
    Public Property OriginalText As String
End Class

''' <summary>
''' Stores the normalized parsed formula together with the shared design-matrix term metadata.
''' </summary>
Public Class RegressionFormulaDesignSpec
    Public Property OriginalFormulaText As String
    Public Property NormalizedFormulaText As String
    Public Property EffectItems As List(Of String)
    Public Property TermSpecs As Dictionary(Of String, TermSpec)
    Public Property RequiredRawVarKeys As List(Of String)
    Public Property ParsedTerms As List(Of ParsedRegressionFormulaTerm)

    ''' <summary>
    ''' Initializes a new instance of the containing type.
    ''' </summary>
    Public Sub New()
        Me.EffectItems = New List(Of String)()
        Me.TermSpecs = New Dictionary(Of String, TermSpec)(StringComparer.Ordinal)
        Me.RequiredRawVarKeys = New List(Of String)()
        Me.ParsedTerms = New List(Of ParsedRegressionFormulaTerm)()
    End Sub
End Class

''' <summary>
''' Parses formula text into normalized design specifications that can be consumed by the regression design core.
''' </summary>
Public Module RegressionFormulaParser

    ''' <summary>
    ''' Builds a default design specification that includes every predictor as a continuous main effect.
    ''' </summary>
    ''' <param name="variableCatalog">The variable catalog used to resolve formula references.</param>
    ''' <returns>A design specification containing one continuous main effect per predictor.</returns>
    Public Function BuildDefaultMainEffectsDesignSpec(variableCatalog As RegressionVariableCatalog) As RegressionFormulaDesignSpec
        If variableCatalog Is Nothing Then Throw New ArgumentNullException(NameOf(variableCatalog))

        Dim spec As New RegressionFormulaDesignSpec With {
            .OriginalFormulaText = String.Empty,
            .NormalizedFormulaText = String.Empty
        }

        For Each v In variableCatalog.Variables.OrderBy(Function(x) x.RelativeColumnIndex)
            Dim term As New ParsedRegressionFormulaTerm With {
                .Kind = "MainEffect",
                .BaseVarKeys = New List(Of String) From {v.BaseKey},
                .Degree = 1,
                .Scale = PredictorScale.Continuous,
                .ReferenceValue = Nothing,
                .EffectKey = v.BaseKey,
                .DisplayNameForCoef = v.DisplayName,
                .NormalizedText = v.RelativeColumnLetter,
                .OriginalText = v.RelativeColumnLetter
            }

            Dim appendErr As String = Nothing
            If Not AppendTerm(spec, term, appendErr) Then
                Throw New InvalidOperationException(appendErr)
            End If
        Next

        spec.RequiredRawVarKeys = RegressionDesignCore.GetRequiredRawVarKeys(spec.EffectItems, spec.TermSpecs)
        Return spec
    End Function

    ''' <summary>
    ''' Attempts to parse a formula string into a normalized design specification.
    ''' </summary>
    ''' <param name="formulaText">The formula text to parse. Blank text produces the default main-effects design.</param>
    ''' <param name="variableCatalog">The variable catalog used to resolve formula references.</param>
    ''' <param name="designSpec">On success, receives the parsed design specification.</param>
    ''' <param name="errorMessage">On failure, receives a human-readable error message.</param>
    ''' <returns>True when parsing succeeds; otherwise, False.</returns>
    Public Function TryParseFormulaToDesignSpec(formulaText As String,
                                                variableCatalog As RegressionVariableCatalog,
                                                ByRef designSpec As RegressionFormulaDesignSpec,
                                                ByRef errorMessage As String) As Boolean

        designSpec = Nothing
        errorMessage = Nothing

        If variableCatalog Is Nothing Then
            errorMessage = "Variable catalog is required."
            Return False
        End If

        Dim formula As String = If(formulaText, String.Empty).Trim()
        If formula = String.Empty Then
            designSpec = BuildDefaultMainEffectsDesignSpec(variableCatalog)
            Return True
        End If

        Dim additiveParts As List(Of String) = SplitTopLevel(formula, "+"c)
        If additiveParts.Count = 0 Then
            errorMessage = "Formula does not contain any valid terms."
            Return False
        End If

        Dim spec As New RegressionFormulaDesignSpec With {
            .OriginalFormulaText = formula
        }
        Dim normalizedTerms As New List(Of String)()

        For Each rawTerm As String In additiveParts
            Dim parsed As ParsedRegressionFormulaTerm = Nothing
            Dim parseErr As String = Nothing
            If Not TryParseSingleTerm(rawTerm, variableCatalog, parsed, parseErr) Then
                errorMessage = parseErr
                Return False
            End If

            Dim effectCountBefore As Integer = spec.EffectItems.Count
            If Not AppendTerm(spec, parsed, parseErr) Then
                errorMessage = parseErr
                Return False
            End If

            If spec.EffectItems.Count > effectCountBefore Then
                normalizedTerms.Add(parsed.NormalizedText)
            End If
        Next

        If spec.EffectItems.Count = 0 Then
            errorMessage = "Formula does not contain any valid terms."
            Return False
        End If

        spec.NormalizedFormulaText = String.Join(" + ", normalizedTerms)
        spec.RequiredRawVarKeys = RegressionDesignCore.GetRequiredRawVarKeys(spec.EffectItems, spec.TermSpecs)
        designSpec = spec
        Return True
    End Function

    ''' <summary>
    ''' Parses a formula string into a normalized design specification or throws an exception.
    ''' </summary>
    ''' <param name="formulaText">The formula text to parse. Blank text produces the default main-effects design.</param>
    ''' <param name="variableCatalog">The variable catalog used to resolve formula references.</param>
    ''' <returns>The parsed design specification.</returns>
    Public Function ParseFormulaToDesignSpec(formulaText As String,
                                             variableCatalog As RegressionVariableCatalog) As RegressionFormulaDesignSpec
        Dim spec As RegressionFormulaDesignSpec = Nothing
        Dim err As String = Nothing
        If Not TryParseFormulaToDesignSpec(formulaText, variableCatalog, spec, err) Then
            Throw New ArgumentException(err)
        End If
        Return spec
    End Function

    ''' <summary>
    ''' Appends a parsed term to a design specification when it is not already present.
    ''' If the same effect key is repeated with incompatible semantics, an error is returned.
    ''' </summary>
    ''' <param name="spec">The design specification being updated.</param>
    ''' <param name="parsed">The parsed-term object being produced or appended.</param>
    ''' <param name="errorMessage">On failure, receives a human-readable error message.</param>
    ''' <returns>True when the term is accepted or a compatible duplicate is ignored; otherwise, False.</returns>
    Private Function AppendTerm(spec As RegressionFormulaDesignSpec,
                                parsed As ParsedRegressionFormulaTerm,
                                ByRef errorMessage As String) As Boolean
        If spec Is Nothing Then Throw New ArgumentNullException(NameOf(spec))
        If parsed Is Nothing Then Throw New ArgumentNullException(NameOf(parsed))

        errorMessage = Nothing

        If Not spec.TermSpecs.ContainsKey(parsed.EffectKey) Then
            spec.EffectItems.Add(parsed.EffectKey)
            spec.ParsedTerms.Add(parsed)

            Dim termSpec As New TermSpec With {
                .Kind = parsed.Kind,
                .BaseVarKeys = New List(Of String)(parsed.BaseVarKeys),
                .Degree = parsed.Degree,
                .DisplayNameForCoef = parsed.DisplayNameForCoef,
                .Order = spec.EffectItems.Count - 1,
                .Scale = parsed.Scale,
                .ReferenceValue = parsed.ReferenceValue
            }

            spec.TermSpecs(parsed.EffectKey) = termSpec
            Return True
        End If

        Dim existing As TermSpec = spec.TermSpecs(parsed.EffectKey)
        If Not AreTermSpecsSemanticallyEquivalent(existing, parsed) Then
            errorMessage =
                "Formula contains conflicting duplicate term specifications for '" & parsed.EffectKey & "'." &
                " For example, the same effect cannot be repeated with different factor reference levels or different term kinds."
            Return False
        End If

        Return True
    End Function

    ''' <summary>
    ''' Determines whether an already stored term specification is semantically equivalent to a newly parsed term.
    ''' </summary>
    ''' <param name="existing">The existing stored term specification.</param>
    ''' <param name="parsed">The newly parsed term.</param>
    ''' <returns>True when both describe the same effect semantics; otherwise, False.</returns>
    Private Function AreTermSpecsSemanticallyEquivalent(existing As TermSpec,
                                                        parsed As ParsedRegressionFormulaTerm) As Boolean
        If existing Is Nothing Then Return False
        If parsed Is Nothing Then Return False

        If Not String.Equals(existing.Kind, parsed.Kind, StringComparison.Ordinal) Then Return False
        If existing.Scale <> parsed.Scale Then Return False
        If existing.Degree <> parsed.Degree Then Return False

        Dim existingKeys As List(Of String) = If(existing.BaseVarKeys, New List(Of String)())
        Dim parsedKeys As List(Of String) = If(parsed.BaseVarKeys, New List(Of String)())

        If existingKeys.Count <> parsedKeys.Count Then Return False
        For i As Integer = 0 To existingKeys.Count - 1
            If Not String.Equals(existingKeys(i), parsedKeys(i), StringComparison.Ordinal) Then
                Return False
            End If
        Next

        If existing.ReferenceValue.HasValue <> parsed.ReferenceValue.HasValue Then Return False
        If existing.ReferenceValue.HasValue Then
            If existing.ReferenceValue.Value <> parsed.ReferenceValue.Value Then Return False
        End If

        Return True
    End Function

    ''' <summary>
    ''' Attempts to parse one additive formula term.
    ''' </summary>
    ''' <param name="termText">The formula term text to parse.</param>
    ''' <param name="variableCatalog">The variable catalog used to resolve formula references.</param>
    ''' <param name="parsed">The parsed-term object being produced or appended.</param>
    ''' <param name="errorMessage">On failure, receives a human-readable error message.</param>
    ''' <returns>True when parsing succeeds; otherwise, False.</returns>
    Private Function TryParseSingleTerm(termText As String,
                                        variableCatalog As RegressionVariableCatalog,
                                        ByRef parsed As ParsedRegressionFormulaTerm,
                                        ByRef errorMessage As String) As Boolean

        parsed = Nothing
        errorMessage = Nothing

        Dim s As String = If(termText, String.Empty).Trim()
        If s = String.Empty Then
            errorMessage = "Formula contains an empty term."
            Return False
        End If

        If StartsWithFactor(s) Then
            Return TryParseFactorMainEffect(s, variableCatalog, parsed, errorMessage)
        End If

        Dim interactionParts As List(Of String) = SplitTopLevel(s, ":"c)
        If interactionParts.Count > 1 Then
            Return TryParseInteraction(s, interactionParts, variableCatalog, parsed, errorMessage)
        End If

        Dim caretIndex As Integer = FindTopLevelChar(s, "^"c)
        If caretIndex >= 0 Then
            Return TryParsePolynomial(s, caretIndex, variableCatalog, parsed, errorMessage)
        End If

        Return TryParseContinuousMainEffect(s, variableCatalog, parsed, errorMessage)
    End Function

    ''' <summary>
    ''' Attempts to parse a continuous main-effect term.
    ''' </summary>
    ''' <param name="termText">The formula term text to parse.</param>
    ''' <param name="variableCatalog">The variable catalog used to resolve formula references.</param>
    ''' <param name="parsed">The parsed-term object being produced or appended.</param>
    ''' <param name="errorMessage">On failure, receives a human-readable error message.</param>
    ''' <returns>True when parsing succeeds; otherwise, False.</returns>
    Private Function TryParseContinuousMainEffect(termText As String,
                                                  variableCatalog As RegressionVariableCatalog,
                                                  ByRef parsed As ParsedRegressionFormulaTerm,
                                                  ByRef errorMessage As String) As Boolean

        Dim entry As RegressionVariableCatalogEntry = Nothing
        Dim tokenKind As RegressionFormulaTokenKind
        If Not variableCatalog.TryResolveToken(termText, entry, tokenKind, errorMessage) Then
            Return False
        End If

        parsed = New ParsedRegressionFormulaTerm With {
            .Kind = "MainEffect",
            .BaseVarKeys = New List(Of String) From {entry.BaseKey},
            .Degree = 1,
            .Scale = PredictorScale.Continuous,
            .ReferenceValue = Nothing,
            .EffectKey = entry.BaseKey,
            .DisplayNameForCoef = entry.DisplayName,
            .NormalizedText = NormalizeVariableReference(entry, tokenKind),
            .OriginalText = termText.Trim()
        }
        Return True
    End Function

    ''' <summary>
    ''' Attempts to parse a categorical main-effect term expressed with factor(...).
    ''' </summary>
    ''' <param name="termText">The formula term text to parse.</param>
    ''' <param name="variableCatalog">The variable catalog used to resolve formula references.</param>
    ''' <param name="parsed">The parsed-term object being produced or appended.</param>
    ''' <param name="errorMessage">On failure, receives a human-readable error message.</param>
    ''' <returns>True when parsing succeeds; otherwise, False.</returns>
    Private Function TryParseFactorMainEffect(termText As String,
                                              variableCatalog As RegressionVariableCatalog,
                                              ByRef parsed As ParsedRegressionFormulaTerm,
                                              ByRef errorMessage As String) As Boolean

        Dim entry As RegressionVariableCatalogEntry = Nothing
        Dim tokenKind As RegressionFormulaTokenKind
        Dim refValue As Nullable(Of Double) = Nothing
        If Not TryParseFactorArguments(termText, variableCatalog, entry, tokenKind, refValue, errorMessage) Then
            Return False
        End If

        Dim normalized As String = "factor(" & NormalizeVariableReference(entry, tokenKind)
        If refValue.HasValue Then
            normalized &= ", ref=" & refValue.Value.ToString("G", CultureInfo.InvariantCulture)
        End If
        normalized &= ")"

        parsed = New ParsedRegressionFormulaTerm With {
            .Kind = "MainEffect",
            .BaseVarKeys = New List(Of String) From {entry.BaseKey},
            .Degree = 1,
            .Scale = PredictorScale.Categorical,
            .ReferenceValue = refValue,
            .EffectKey = RegressionDesignCore.MakeCategoricalEffectKey(entry.BaseKey),
            .DisplayNameForCoef = entry.DisplayName,
            .NormalizedText = normalized,
            .OriginalText = termText.Trim()
        }
        Return True
    End Function

    ''' <summary>
    ''' Attempts to parse a polynomial term.
    ''' </summary>
    ''' <param name="termText">The formula term text to parse.</param>
    ''' <param name="caretIndex">The position of the top-level polynomial operator within the term text.</param>
    ''' <param name="variableCatalog">The variable catalog used to resolve formula references.</param>
    ''' <param name="parsed">The parsed-term object being produced or appended.</param>
    ''' <param name="errorMessage">On failure, receives a human-readable error message.</param>
    ''' <returns>True when parsing succeeds; otherwise, False.</returns>
    Private Function TryParsePolynomial(termText As String,
                                        caretIndex As Integer,
                                        variableCatalog As RegressionVariableCatalog,
                                        ByRef parsed As ParsedRegressionFormulaTerm,
                                        ByRef errorMessage As String) As Boolean

        Dim lhs As String = termText.Substring(0, caretIndex).Trim()
        Dim rhs As String = termText.Substring(caretIndex + 1).Trim()
        If lhs = String.Empty OrElse rhs = String.Empty Then
            errorMessage = "Invalid polynomial term '" & termText.Trim() & "'. Use syntax such as A^2."
            Return False
        End If

        If StartsWithFactor(lhs) Then
            errorMessage = "Polynomial terms on factor(...) are not supported yet."
            Return False
        End If

        Dim degree As Integer
        If Not Integer.TryParse(rhs, NumberStyles.Integer, CultureInfo.InvariantCulture, degree) OrElse degree < 2 Then
            errorMessage = "Polynomial degree in '" & termText.Trim() & "' must be an integer greater than or equal to 2."
            Return False
        End If

        Dim entry As RegressionVariableCatalogEntry = Nothing
        Dim tokenKind As RegressionFormulaTokenKind
        If Not variableCatalog.TryResolveToken(lhs, entry, tokenKind, errorMessage) Then
            Return False
        End If

        parsed = New ParsedRegressionFormulaTerm With {
            .Kind = "Polynomial",
            .BaseVarKeys = New List(Of String) From {entry.BaseKey},
            .Degree = degree,
            .Scale = PredictorScale.Continuous,
            .ReferenceValue = Nothing,
            .EffectKey = RegressionDesignCore.MakePolynomialEffectKey(entry.BaseKey, degree),
            .DisplayNameForCoef = entry.DisplayName & "^" & degree.ToString(CultureInfo.InvariantCulture),
            .NormalizedText = NormalizeVariableReference(entry, tokenKind) & "^" & degree.ToString(CultureInfo.InvariantCulture),
            .OriginalText = termText.Trim()
        }
        Return True
    End Function

    ''' <summary>
    ''' Attempts to parse a continuous interaction term.
    ''' </summary>
    ''' <param name="termText">The formula term text to parse.</param>
    ''' <param name="interactionParts">The interaction subterms obtained by splitting the source term on top-level colons.</param>
    ''' <param name="variableCatalog">The variable catalog used to resolve formula references.</param>
    ''' <param name="parsed">The parsed-term object being produced or appended.</param>
    ''' <param name="errorMessage">On failure, receives a human-readable error message.</param>
    ''' <returns>True when parsing succeeds; otherwise, False.</returns>
    Private Function TryParseInteraction(termText As String,
                                         interactionParts As List(Of String),
                                         variableCatalog As RegressionVariableCatalog,
                                         ByRef parsed As ParsedRegressionFormulaTerm,
                                         ByRef errorMessage As String) As Boolean

        If interactionParts Is Nothing OrElse interactionParts.Count < 2 Then
            errorMessage = "Invalid interaction term '" & termText.Trim() & "'."
            Return False
        End If

        Dim resolvedEntries As New List(Of RegressionVariableCatalogEntry)()
        Dim resolvedKinds As New List(Of RegressionFormulaTokenKind)()
        Dim seenBaseKeys As New HashSet(Of String)(StringComparer.Ordinal)

        For Each part As String In interactionParts
            Dim p As String = If(part, String.Empty).Trim()
            If p = String.Empty Then
                errorMessage = "Invalid interaction term '" & termText.Trim() & "'."
                Return False
            End If
            If StartsWithFactor(p) Then
                errorMessage = "Interactions involving factor(...) are not supported yet."
                Return False
            End If
            If FindTopLevelChar(p, "^"c) >= 0 Then
                errorMessage = "Interactions involving polynomial subterms are not supported yet."
                Return False
            End If

            Dim entry As RegressionVariableCatalogEntry = Nothing
            Dim tokenKind As RegressionFormulaTokenKind
            If Not variableCatalog.TryResolveToken(p, entry, tokenKind, errorMessage) Then
                Return False
            End If

            If seenBaseKeys.Contains(entry.BaseKey) Then
                errorMessage = "Interaction term '" & termText.Trim() & "' repeats variable '" &
                               entry.BaseKey &
                               "'. Self-interactions such as A:A or repeated variables within one interaction term are not supported."
                Return False
            End If

            seenBaseKeys.Add(entry.BaseKey)
            resolvedEntries.Add(entry)
            resolvedKinds.Add(tokenKind)
        Next

        Dim paired = resolvedEntries.Select(Function(x, i) New With {.Entry = x, .Kind = resolvedKinds(i)}) _
                                  .OrderBy(Function(x) x.Entry.BaseKey, StringComparer.Ordinal) _
                                  .ToList()

        Dim baseKeys As List(Of String) = paired.Select(Function(x) x.Entry.BaseKey).ToList()
        Dim displayNames As List(Of String) = paired.Select(Function(x) x.Entry.DisplayName).ToList()
        Dim normalizedTokens As List(Of String) = paired.Select(Function(x) NormalizeVariableReference(x.Entry, x.Kind)).ToList()

        parsed = New ParsedRegressionFormulaTerm With {
            .Kind = "Interaction",
            .BaseVarKeys = baseKeys,
            .Degree = 1,
            .Scale = PredictorScale.Continuous,
            .ReferenceValue = Nothing,
            .EffectKey = RegressionDesignCore.MakeInteractionEffectKey(baseKeys),
            .DisplayNameForCoef = String.Join(":", displayNames),
            .NormalizedText = String.Join(":", normalizedTokens),
            .OriginalText = termText.Trim()
        }
        Return True
    End Function

    ''' <summary>
    ''' Attempts to parse the argument list of a factor(...) term.
    ''' </summary>
    ''' <param name="termText">The formula term text to parse.</param>
    ''' <param name="variableCatalog">The variable catalog used to resolve formula references.</param>
    ''' <param name="entry">On success, receives the resolved variable-catalog entry.</param>
    ''' <param name="tokenKind">On success, receives the addressing mode used to resolve the token.</param>
    ''' <param name="referenceValue">On success, receives the requested categorical reference level when one is specified.</param>
    ''' <param name="errorMessage">On failure, receives a human-readable error message.</param>
    ''' <returns>True when parsing succeeds; otherwise, False.</returns>
    Private Function TryParseFactorArguments(termText As String,
                                             variableCatalog As RegressionVariableCatalog,
                                             ByRef entry As RegressionVariableCatalogEntry,
                                             ByRef tokenKind As RegressionFormulaTokenKind,
                                             ByRef referenceValue As Nullable(Of Double),
                                             ByRef errorMessage As String) As Boolean

        entry = Nothing
        tokenKind = RegressionFormulaTokenKind.RelativeColumnLetter
        referenceValue = Nothing
        errorMessage = Nothing

        Dim s As String = If(termText, String.Empty).Trim()
        If Not StartsWithFactor(s) OrElse Not s.EndsWith(")", StringComparison.Ordinal) Then
            errorMessage = "Invalid factor() term '" & s & "'."
            Return False
        End If

        Dim openIndex As Integer = s.IndexOf("("c)
        Dim inner As String = s.Substring(openIndex + 1, s.Length - openIndex - 2).Trim()
        If inner = String.Empty Then
            errorMessage = "factor() requires a variable reference."
            Return False
        End If

        Dim args As List(Of String) = SplitTopLevel(inner, ","c)
        If args.Count < 1 OrElse args.Count > 2 Then
            errorMessage = "factor() currently supports syntax factor(X) or factor(X, ref=2)."
            Return False
        End If

        If Not variableCatalog.TryResolveToken(args(0).Trim(), entry, tokenKind, errorMessage) Then
            Return False
        End If

        If args.Count = 2 Then
            Dim secondArg As String = args(1).Trim()
            Dim eqIndex As Integer = FindTopLevelChar(secondArg, "="c)
            If eqIndex <= 0 Then
                errorMessage = "Invalid factor() reference-level syntax in '" & s & "'. Use factor(X, ref=2)."
                Return False
            End If

            Dim key As String = secondArg.Substring(0, eqIndex).Trim()
            Dim valueText As String = secondArg.Substring(eqIndex + 1).Trim()
            If Not String.Equals(key, "ref", StringComparison.OrdinalIgnoreCase) Then
                errorMessage = "Invalid factor() option '" & key & "'. Only ref=... is supported."
                Return False
            End If

            Dim d As Double
            If Not Double.TryParse(valueText, NumberStyles.Float Or NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, d) Then
                errorMessage = "factor() reference level must be numeric in '" & s & "'."
                Return False
            End If
            If Double.IsNaN(d) OrElse Double.IsInfinity(d) Then
                errorMessage = "factor() reference level must be a finite numeric value in '" & s & "'."
                Return False
            End If

            referenceValue = d
        End If

        Return True
    End Function

    ''' <summary>
    ''' Determines whether a term begins with factor(.
    ''' </summary>
    ''' <param name="s">The text value to inspect.</param>
    ''' <returns>True when the text starts with factor(; otherwise, False.</returns>
    Private Function StartsWithFactor(s As String) As Boolean
        Return If(s, String.Empty).TrimStart().StartsWith("factor(", StringComparison.OrdinalIgnoreCase)
    End Function

    ''' <summary>
    ''' Formats a resolved variable reference back into canonical formula text.
    ''' </summary>
    ''' <param name="entry">On success, receives the resolved variable-catalog entry.</param>
    ''' <param name="tokenKind">On success, receives the addressing mode used to resolve the token.</param>
    ''' <returns>Canonical formula text for the resolved variable reference.</returns>
    Private Function NormalizeVariableReference(entry As RegressionVariableCatalogEntry,
                                                tokenKind As RegressionFormulaTokenKind) As String
        If entry Is Nothing Then Return String.Empty

        Select Case tokenKind
            Case RegressionFormulaTokenKind.VariableName
                Return "'" & If(entry.VariableName, String.Empty).Replace("'", "''") & "'"
            Case RegressionFormulaTokenKind.AbsoluteColumnLetter
                Return entry.AbsoluteColumnLetter
            Case Else
                Return entry.RelativeColumnLetter
        End Select
    End Function

    ''' <summary>
    ''' Splits text by a separator while ignoring separators that appear inside quotes or parentheses.
    ''' </summary>
    ''' <param name="text">The text to inspect.</param>
    ''' <param name="separator">The separator character to split on.</param>
    ''' <returns>A list of split segments.</returns>
    Private Function SplitTopLevel(text As String, separator As Char) As List(Of String)
        Dim parts As New List(Of String)()
        Dim s As String = If(text, String.Empty)
        Dim depth As Integer = 0
        Dim inQuotes As Boolean = False
        Dim startIndex As Integer = 0

        For i As Integer = 0 To s.Length - 1
            Dim ch As Char = s(i)
            If ch = "'"c Then
                inQuotes = Not inQuotes
            ElseIf Not inQuotes Then
                If ch = "("c Then
                    depth += 1
                ElseIf ch = ")"c Then
                    depth -= 1
                    If depth < 0 Then depth = 0
                ElseIf ch = separator AndAlso depth = 0 Then
                    parts.Add(s.Substring(startIndex, i - startIndex).Trim())
                    startIndex = i + 1
                End If
            End If
        Next

        parts.Add(s.Substring(startIndex).Trim())
        Return parts.Where(Function(x) x IsNot Nothing).ToList()
    End Function

    ''' <summary>
    ''' Finds the first occurrence of a character that appears at the top parsing level.
    ''' </summary>
    ''' <param name="text">The text to inspect.</param>
    ''' <param name="target">The character to search for.</param>
    ''' <returns>The zero-based index of the matching character, or -1 when none is found.</returns>
    Private Function FindTopLevelChar(text As String, target As Char) As Integer
        Dim s As String = If(text, String.Empty)
        Dim depth As Integer = 0
        Dim inQuotes As Boolean = False

        For i As Integer = 0 To s.Length - 1
            Dim ch As Char = s(i)
            If ch = "'"c Then
                inQuotes = Not inQuotes
            ElseIf Not inQuotes Then
                If ch = "("c Then
                    depth += 1
                ElseIf ch = ")"c Then
                    depth -= 1
                    If depth < 0 Then depth = 0
                ElseIf ch = target AndAlso depth = 0 Then
                    Return i
                End If
            End If
        Next

        Return -1
    End Function
End Module
