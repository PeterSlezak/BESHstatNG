Option Explicit On
Imports BESHStatNG.AppInfrastructure

Public Enum PredictorScale
    Continuous = 0
    Categorical = 1
End Enum

Public Class TermSpec
    Public Property Kind As String 'MainEffect | Polynomial | Interaction
    Public Property BaseVarKeys As List(Of String)
    Public Property Degree As Integer
    Public Property DisplayNameForCoef As String '(e.g. "Age^2", "Age:BMI")
    Public Property Order As Integer 'position in the result output. It should be identical to the input combobox item position
    Public Property Scale As PredictorScale = PredictorScale.Continuous
    Public Property ReferenceValue As Nullable(Of Double) = Nothing 'optional: if Nothing, use smallest numeric level as reference
End Class

Public Module RegressionDesignCore

    Public Const CATEGORICAL_EFFECT_PREFIX As String = "[Cat] "

    Public Function GetCoefBaseName(varKey As String) As String
        If String.IsNullOrEmpty(varKey) Then Return String.Empty

        Dim s As String = varKey.Trim()
        Dim token As String = " | Var"

        Dim idx As Integer = s.IndexOf(token, StringComparison.Ordinal)
        If idx >= 0 Then
            Return s.Substring(0, idx).Trim()
        End If

        Return s
    End Function

    ''' <summary>
    ''' Constructs a standardized polynomial effect key using a quoted base term
    ''' and an integer exponent. The resulting format is: "baseKey"^degree.
    ''' </summary>
    ''' <param name="baseKey">
    ''' The underlying effect name to be wrapped in double quotes.
    ''' </param>
    ''' <param name="degree">
    ''' The polynomial degree to append after the caret symbol.
    ''' </param>
    ''' <returns>
    ''' A string in the form "baseKey"^degree, suitable for use as a
    ''' polynomial effect identifier.
    ''' </returns>
    Public Function MakePolynomialEffectKey(baseKey As String, degree As Integer) As String
        Return """" & baseKey & """" & "^" & CStr(degree)
    End Function

    Public Function MakeCategoricalEffectKey(baseKey As String) As String
        Return CATEGORICAL_EFFECT_PREFIX & baseKey
    End Function

    ''' <summary>
    ''' Builds a standardized interaction-effect key by quoting each base term
    ''' and joining them with a colon separator. The resulting format is:
    ''' "A":"B":... for multiway interactions.
    ''' </summary>
    ''' <param name="baseKeys">
    ''' A sequence of effect names that will be individually wrapped in
    ''' double quotes and combined into a single interaction key.
    ''' </param>
    ''' <returns>
    ''' A colon‑delimited string of quoted effect names, suitable for use as
    ''' an interaction-effect identifier.
    ''' </returns>
    ''' <remarks>
    ''' The function does not validate or alter the internal content of each
    ''' key; it only trims, quotes, and concatenates them.
    ''' </remarks>
    Public Function MakeInteractionEffectKey(baseKeys As IEnumerable(Of String)) As String
        Dim quoted As New List(Of String)
        For Each k As String In baseKeys
            quoted.Add("""" & k & """")
        Next
        Return String.Join(":", quoted)
    End Function

    ''' <summary>
    ''' Creates a standardized coefficient-name key for an interaction term by
    ''' converting each base key into its coefficient-safe form and joining them
    ''' with a colon separator.
    ''' </summary>
    ''' <param name="baseKeys">
    ''' A sequence of effect base names that will be transformed using
    ''' <c>GetCoefBaseName</c> and combined into a single interaction
    ''' coefficient identifier.
    ''' </param>
    ''' <returns>
    ''' A colon‑delimited string of coefficient‑safe names representing the
    ''' interaction term.
    ''' </returns>
    ''' <remarks>
    ''' This function delegates the normalization of each individual key to
    ''' <c>GetCoefBaseName</c>, ensuring consistent naming across all effect types.
    ''' </remarks>
    Public Function MakeInteractionCoefName(baseKeys As IEnumerable(Of String)) As String
        Dim names As New List(Of String)
        For Each k As String In baseKeys
            names.Add(GetCoefBaseName(k))
        Next
        Return String.Join(":", names)
    End Function

    Private Function MakeResolvedInteractionDisplayName(baseKeys As IEnumerable(Of String),
                                                    baseDisplayNames As Dictionary(Of String, String)) As String
        Dim names As New List(Of String)

        For Each bk As String In baseKeys
            If baseDisplayNames.ContainsKey(bk) Then
                names.Add(baseDisplayNames(bk))
            Else
                names.Add(GetCoefBaseName(bk))
            End If
        Next

        Return String.Join(":", names)
    End Function

    ''' <summary>
    ''' Returns the ordered list of RAW worksheet variable keys required to construct the selected effects.
    ''' Uses TermSpecs when available; otherwise falls back to parsing effect strings:
    '''   Polynomial:   "A | VarX"^k
    '''   Interaction:  "A | VarX":"B | VarY":...
    ''' </summary>
    Public Function GetRequiredRawVarKeys(effectItems As IEnumerable, termSpecs As Dictionary(Of String, TermSpec)) As List(Of String)
        Dim raw As New List(Of String)

        If effectItems Is Nothing Then Return raw

        For Each obj As Object In effectItems
            Dim effKey As String = CStr(obj)

            '1) Prefer TermSpecs mapping
            If termSpecs IsNot Nothing AndAlso termSpecs.ContainsKey(effKey) Then
                Dim spec As TermSpec = termSpecs(effKey)
                If spec IsNot Nothing AndAlso spec.BaseVarKeys IsNot Nothing AndAlso spec.BaseVarKeys.Count > 0 Then
                    For Each baseKey As String In spec.BaseVarKeys
                        If Not raw.Contains(baseKey) Then raw.Add(baseKey)
                    Next
                    Continue For
                End If
            End If

            '2) Fallback parse: polynomial or interaction formatted strings
            Dim baseKeys As List(Of String) = ExtractBaseKeysFromEffectText(effKey)
            If baseKeys.Count > 0 Then
                For Each k As String In baseKeys
                    If Not raw.Contains(k) Then raw.Add(k)
                Next
            Else
                If Not raw.Contains(effKey) Then raw.Add(effKey)
            End If
        Next

        Return raw
    End Function

    ''' <summary>
    ''' Extract base variable keys from an effect text if it matches polynomial/interaction UI formats.
    ''' Returns empty list if it looks like a simple main effect.
    ''' </summary>
    Public Function ExtractBaseKeysFromEffectText(effectText As String) As List(Of String)
        Dim out As New List(Of String)
        Dim s As String = If(effectText, String.Empty).Trim()

        If s = String.Empty Then Return out

        'Categorical main effect: [Cat] Age | VarA
        If s.StartsWith(CATEGORICAL_EFFECT_PREFIX, StringComparison.Ordinal) Then
            out.Add(s.Substring(CATEGORICAL_EFFECT_PREFIX.Length).Trim())
            Return out
        End If

        'Polynomial: "<base>"^k or <base>^k
        Dim pCaret As Integer = InStrRev(s, "^")
        If pCaret > 0 AndAlso pCaret < Len(s) Then
            Dim expStr As String = Mid$(s, pCaret + 1)
            Dim expVal As Integer
            If Integer.TryParse(expStr, expVal) Then
                Dim baseKey As String = Left$(s, pCaret - 1).Trim()
                out.Add(StripOuterDoubleQuotesPublic(baseKey))
                Return out
            End If
        End If

        'Interaction: "A":"B":"C"...
        If InStr(s, ":") > 0 AndAlso InStr(s, ChrW(34)) > 0 Then
            Dim parts() As String = Split(s, ":")
            For Each p As String In parts
                Dim k As String = StripOuterDoubleQuotesPublic(p.Trim())
                If k <> String.Empty Then out.Add(k)
            Next
            If out.Count >= 2 Then Return out
            out.Clear()
        End If

        'Not a derived effect format
        Return out
    End Function

    ''' <summary>
    ''' Removes a single pair of leading and trailing double quotes from the
    ''' provided string, if such quotes are present. Inner content is left
    ''' unchanged.
    ''' </summary>
    ''' <param name="s">
    ''' The input string to process. May be quoted, unquoted, or null.
    ''' </param>
    ''' <returns>
    ''' The trimmed string with one outer pair of double quotes removed when
    ''' applicable; otherwise, the trimmed original value.
    ''' </returns>
    ''' <remarks>
    ''' Only strips quotes when both the first and last characters are
    ''' double quotes. Does not modify or interpret any internal characters.
    ''' </remarks>
    Public Function StripOuterDoubleQuotesPublic(s As String) As String
        Dim t As String = If(s, String.Empty).Trim()
        If Len(t) >= 2 AndAlso Left$(t, 1) = ChrW(34) AndAlso Right$(t, 1) = ChrW(34) Then
            t = Mid$(t, 2, Len(t) - 2)
        End If
        Return t.Trim()
    End Function

    ''' <summary>
    ''' Updates the <c>Order</c> property of each <c>TermSpec</c> entry based on
    ''' the sequence of items provided in <paramref name="effectItems"/>.
    ''' </summary>
    ''' <param name="effectItems">
    ''' An ordered collection whose elements represent effect keys. Each item is
    ''' converted to a string and used to look up the corresponding entry in
    ''' <paramref name="termSpecs"/>.
    ''' </param>
    ''' <param name="termSpecs">
    ''' A dictionary mapping effect keys to their associated <c>TermSpec</c>
    ''' objects. Only keys present in this dictionary are updated.
    ''' </param>
    ''' <remarks>
    ''' The method assigns incremental order values starting at zero, following
    ''' the enumeration order of <paramref name="effectItems"/>. Items not found
    ''' in <paramref name="termSpecs"/> are ignored.
    ''' </remarks>
    Public Sub RefreshTermSpecOrders(effectItems As IEnumerable, termSpecs As Dictionary(Of String, TermSpec))
        If effectItems Is Nothing OrElse termSpecs Is Nothing Then Exit Sub

        Dim i As Integer = 0
        For Each obj As Object In effectItems
            Dim k As String = CStr(obj)
            If termSpecs.ContainsKey(k) Then termSpecs(k).Order = i
            i += 1
        Next
    End Sub

    ''' <summary>
    ''' Computes an integer power of a floating‑point value using simple
    ''' repeated multiplication. Supports non‑negative integer exponents.
    ''' </summary>
    ''' <param name="x">
    ''' The base value to be raised to a power.
    ''' </param>
    ''' <param name="degree">
    ''' The non‑negative integer exponent.  
    ''' A value of 0 returns 1.0; a value of 1 returns <paramref name="x"/>.
    ''' </param>
    ''' <returns>
    ''' The value of <paramref name="x"/> raised to the specified integer power.
    ''' </returns>
    ''' <remarks>
    ''' This implementation uses iterative multiplication and does not perform
    ''' any overflow checking or handle negative exponents.
    ''' </remarks>
    Private Function PowInt(x As Double, degree As Integer) As Double
        If degree = 0 Then Return 1.0
        If degree = 1 Then Return x

        Dim r As Double = 1.0
        For k As Integer = 1 To degree
            r *= x
        Next
        Return r
    End Function

    Private Function GetSortedDistinctLevels(rawMat(,) As Double, col As Integer, nRows As Integer) As List(Of Double)
        Dim hs As New SortedSet(Of Double)

        For i As Integer = 0 To nRows - 1
            Dim v As Double = rawMat(i, col)
            If Double.IsNaN(v) OrElse Double.IsInfinity(v) Then
                AppGlobals.BSerr.LogAndThrow(New ArgumentException("Categorical variable contains invalid numeric values."))
            End If
            hs.Add(v)
        Next

        Return hs.ToList()
    End Function

    Private Function GetReferenceLevel(levels As List(Of Double), spec As TermSpec) As Double
        If levels Is Nothing OrElse levels.Count = 0 Then
            AppGlobals.BSerr.LogAndThrow(New ArgumentException("No observed factor levels were found."))
        End If

        If spec IsNot Nothing AndAlso spec.ReferenceValue.HasValue Then
            If Not levels.Contains(spec.ReferenceValue.Value) Then
                AppGlobals.BSerr.LogAndThrow(New ArgumentException($"Reference level {spec.ReferenceValue.Value} is not present in the data."))
            End If
            Return spec.ReferenceValue.Value
        End If

        'default: smallest observed level
        Return levels(0)
    End Function

    Private Function IsBaseVariableCategorical(baseKey As String,
                                           termSpecs As Dictionary(Of String, TermSpec)) As Boolean
        If termSpecs Is Nothing Then Return False

        For Each kvp In termSpecs
            Dim spec = kvp.Value
            If spec Is Nothing OrElse spec.BaseVarKeys Is Nothing Then Continue For

            If spec.Scale = PredictorScale.Categorical AndAlso
           spec.BaseVarKeys.Any(Function(x) String.Equals(x, baseKey, StringComparison.Ordinal)) Then
                Return True
            End If
        Next

        Return False
    End Function

    ''' <summary>
    ''' Build expanded LM data matrix: [Y | expanded X], where expanded X includes
    ''' polynomial and interaction columns based on TermSpecs/effectItems.
    ''' varNames returned includes Y at index 0 and expanded predictors thereafter.
    ''' </summary>
    Public Sub BuildExpandedLmDataMatrix(raw As glmData, yKey As String, effectItems As IEnumerable,
                                     termSpecs As Dictionary(Of String, TermSpec),
                                     includeIntercept As Boolean,
                                     ByRef outData(,) As Double,
                                     ByRef outVarNames() As String,
                                     ByRef outTermGroups As Dictionary(Of String, Integer()))

        If raw Is Nothing Then AppGlobals.BSerr.LogAndThrow(New ArgumentNullException(NameOf(raw)))
        If effectItems Is Nothing Then AppGlobals.BSerr.LogAndThrow(New ArgumentNullException(NameOf(effectItems)))

        Dim effects As New List(Of String)
        For Each obj As Object In effectItems
            effects.Add(CStr(obj))
        Next

        Dim nRows As Integer = raw.nRows
        Dim rawMat(,) As Double = raw.DataDbl

        'rawMat col 0 = Y; cols 1.. = imported raw predictors
        Dim rawXKeys As List(Of String) = GetRequiredRawVarKeys(effectItems, termSpecs)
        Dim baseDisplayNames As Dictionary(Of String, String) = BuildLmBaseDisplayNameMap(effectItems, termSpecs)
        Dim rawIndex As New Dictionary(Of String, Integer)(StringComparer.Ordinal)
        For j As Integer = 0 To rawXKeys.Count - 1
            rawIndex(rawXKeys(j)) = j + 1
        Next

        Dim predictorCols As New List(Of Double())
        Dim predictorNames As New List(Of String)

        Dim groups As New Dictionary(Of String, List(Of Integer))(StringComparer.Ordinal)
        If includeIntercept Then
            groups("Intercept") = New List(Of Integer) From {0}
        End If

        Dim nextLmXCol As Integer = If(includeIntercept, 1, 0)

        For Each effKey As String In effects
            Dim kind As String = "MainEffect"
            Dim baseKeys As List(Of String) = Nothing
            Dim degree As Integer = 1
            Dim coefName As String = Nothing
            Dim scale As PredictorScale = PredictorScale.Continuous
            Dim spec As TermSpec = Nothing

            If termSpecs IsNot Nothing AndAlso termSpecs.ContainsKey(effKey) AndAlso termSpecs(effKey) IsNot Nothing Then
                spec = termSpecs(effKey)
                kind = If(spec.Kind, "MainEffect")
                baseKeys = spec.BaseVarKeys
                degree = spec.Degree
                coefName = spec.DisplayNameForCoef
                scale = spec.Scale
            End If

            If baseKeys Is Nothing OrElse baseKeys.Count = 0 Then
                baseKeys = New List(Of String) From {effKey}
            End If

            If String.Equals(kind, "Polynomial", StringComparison.OrdinalIgnoreCase) Then
                Dim bk As String = baseKeys(0)

                If scale = PredictorScale.Categorical OrElse IsBaseVariableCategorical(bk, termSpecs) Then
                    AppGlobals.BSerr.LogAndThrow(New ArgumentException($"Polynomial term '{effKey}' cannot be used with a categorical predictor."))
                End If

                If degree < 2 Then
                    AppGlobals.BSerr.LogAndThrow(New ArgumentException($"Polynomial degree must be >=2 for term '{effKey}'."))
                End If

                If Not rawIndex.ContainsKey(bk) Then
                    AppGlobals.BSerr.LogAndThrow(New ArgumentException($"Missing raw predictor '{bk}' required by polynomial term '{effKey}'."))
                End If

                Dim col As Integer = rawIndex(bk)
                Dim newCol(nRows - 1) As Double
                For i As Integer = 0 To nRows - 1
                    newCol(i) = PowInt(rawMat(i, col), degree)
                Next

                predictorCols.Add(newCol)
                Dim displayBase As String = If(baseDisplayNames.ContainsKey(bk), baseDisplayNames(bk), GetCoefBaseName(bk))
                predictorNames.Add(If(String.IsNullOrWhiteSpace(coefName), displayBase & "^" & CStr(degree), coefName))

                Dim gName As String = displayBase
                If Not groups.ContainsKey(gName) Then groups(gName) = New List(Of Integer)
                groups(gName).Add(nextLmXCol)
                nextLmXCol += 1

            ElseIf String.Equals(kind, "Interaction", StringComparison.OrdinalIgnoreCase) Then
                If baseKeys.Count < 2 Then
                    AppGlobals.BSerr.LogAndThrow(New ArgumentException($"Interaction term '{effKey}' must have at least 2 base variables."))
                End If

                For Each bk As String In baseKeys
                    If IsBaseVariableCategorical(bk, termSpecs) Then
                        AppGlobals.BSerr.LogAndThrow(New ArgumentException($"Interaction term '{effKey}' uses a categorical predictor. This is not implemented yet."))
                    End If
                Next

                Dim cols As New List(Of Integer)
                For Each bk As String In baseKeys
                    If Not rawIndex.ContainsKey(bk) Then
                        AppGlobals.BSerr.LogAndThrow(New ArgumentException($"Missing raw predictor '{bk}' required by interaction term '{effKey}'."))
                    End If
                    cols.Add(rawIndex(bk))
                Next

                Dim newCol(nRows - 1) As Double
                For i As Integer = 0 To nRows - 1
                    Dim prod As Double = 1.0
                    For Each c As Integer In cols
                        prod *= rawMat(i, c)
                    Next
                    newCol(i) = prod
                Next

                predictorCols.Add(newCol)
                Dim resolvedInteractionName As String = MakeResolvedInteractionDisplayName(baseKeys, baseDisplayNames)
                predictorNames.Add(If(String.IsNullOrWhiteSpace(coefName), resolvedInteractionName, coefName))

                Dim gName As String = If(String.IsNullOrWhiteSpace(coefName), resolvedInteractionName, coefName)
                If Not groups.ContainsKey(gName) Then groups(gName) = New List(Of Integer)
                groups(gName).Add(nextLmXCol)
                nextLmXCol += 1

            Else
                'Main effect
                Dim bk As String = baseKeys(0)

                If Not rawIndex.ContainsKey(bk) Then
                    AppGlobals.BSerr.LogAndThrow(New ArgumentException($"Missing raw predictor '{bk}' required by term '{effKey}'."))
                End If

                Dim col As Integer = rawIndex(bk)
                Dim baseName As String = If(String.IsNullOrWhiteSpace(coefName),
                                            If(baseDisplayNames.ContainsKey(bk), baseDisplayNames(bk), GetCoefBaseName(bk)),
                                            coefName)
                If Not groups.ContainsKey(baseName) Then groups(baseName) = New List(Of Integer)

                If scale = PredictorScale.Categorical Then
                    Dim levels As List(Of Double) = GetSortedDistinctLevels(rawMat, col, nRows)
                    If levels.Count < 2 Then
                        AppGlobals.BSerr.LogAndThrow(New ArgumentException($"Categorical predictor '{bk}' has fewer than 2 observed levels."))
                    End If

                    Dim refVal As Double = GetReferenceLevel(levels, spec)

                    For Each lev As Double In levels
                        If includeIntercept AndAlso lev = refVal Then Continue For

                        Dim newCol(nRows - 1) As Double
                        For i As Integer = 0 To nRows - 1
                            newCol(i) = If(rawMat(i, col) = lev, 1.0, 0.0)
                        Next

                        predictorCols.Add(newCol)
                        predictorNames.Add($"{baseName}[{lev}]")
                        groups(baseName).Add(nextLmXCol)
                        nextLmXCol += 1
                    Next

                Else
                    Dim newCol(nRows - 1) As Double
                    For i As Integer = 0 To nRows - 1
                        newCol(i) = rawMat(i, col)
                    Next

                    predictorCols.Add(newCol)
                    predictorNames.Add(baseName) 'predictorNames.Add(If(String.IsNullOrEmpty(coefName), baseName, coefName))
                    groups(baseName).Add(nextLmXCol)
                    nextLmXCol += 1
                End If
            End If
        Next

        'Materialize final [Y | expanded X]
        Dim pExpanded As Integer = predictorCols.Count
        ReDim outData(nRows - 1, pExpanded)
        ReDim outVarNames(pExpanded)

        outVarNames(0) = GetCoefBaseName(yKey)
        For i As Integer = 0 To nRows - 1
            outData(i, 0) = rawMat(i, 0)
        Next

        For j As Integer = 0 To predictorCols.Count - 1
            outVarNames(j + 1) = predictorNames(j)
            For i As Integer = 0 To nRows - 1
                outData(i, j + 1) = predictorCols(j)(i)
            Next
        Next

        outTermGroups = New Dictionary(Of String, Integer())(StringComparer.Ordinal)
        For Each kvp In groups
            outTermGroups(kvp.Key) = kvp.Value.Distinct().OrderBy(Function(z) z).ToArray()
        Next
    End Sub

    ''' <summary>
    ''' Extracts the variable‑suffix portion of a base effect key. Supports keys
    ''' that optionally contain a descriptive prefix followed by " | ".
    ''' </summary>
    ''' <param name="baseKey">
    ''' The full effect key from which the suffix should be extracted. May be
    ''' a simple name (e.g., "VarC") or a composite form (e.g., "Age | VarA").
    ''' </param>
    ''' <returns>
    ''' The suffix portion of the key.  
    ''' If the key contains the token " | ", the substring after the token is
    ''' returned; otherwise, the trimmed key itself is returned.  
    ''' Returns "var" when <paramref name="baseKey"/> is null or empty.
    ''' </returns>
    ''' <remarks>
    ''' This function does not validate the semantic meaning of the suffix; it
    ''' simply parses based on the presence of the " | " delimiter.
    ''' </remarks>
    Private Function ExtractVarSuffix(baseKey As String) As String
        If String.IsNullOrEmpty(baseKey) Then Return "var"

        'Base key examples:
        '  "Age | VarA"  -> suffix "VarA"
        '  "VarC"        -> suffix "VarC"
        Dim token As String = " | "
        Dim idx As Integer = baseKey.IndexOf(token, StringComparison.Ordinal)
        If idx >= 0 AndAlso idx + token.Length < baseKey.Length Then
            Return baseKey.Substring(idx + token.Length).Trim()
        End If

        Return baseKey.Trim()
    End Function

    Private Function BuildLmBaseDisplayNameMap(effectItems As IEnumerable,
                                           termSpecs As Dictionary(Of String, TermSpec)) As Dictionary(Of String, String)

        Dim out As New Dictionary(Of String, String)(StringComparer.Ordinal)
        Dim rawBaseKeys As List(Of String) = GetRequiredRawVarKeys(effectItems, termSpecs)

        Dim grouped = rawBaseKeys.GroupBy(Function(bk) GetCoefBaseName(bk), StringComparer.Ordinal)

        For Each grp In grouped
            Dim baseName As String = grp.Key
            Dim keys As List(Of String) = grp.ToList()

            If keys.Count = 1 Then
                out(keys(0)) = baseName
            Else
                Dim used As New HashSet(Of String)(StringComparer.Ordinal)

                For Each bk As String In keys
                    Dim candidate As String = baseName & " (" & ExtractVarSuffix(bk) & ")"
                    Dim finalName As String = candidate
                    Dim k As Integer = 2

                    While used.Contains(finalName)
                        finalName = candidate & " (" & k & ")"
                        k += 1
                    End While

                    used.Add(finalName)
                    out(bk) = finalName
                Next
            End If
        Next

        Return out
    End Function

    ''' <summary>
    ''' Materializes an arbitrary enumerable of effect keys into an ordered string list.
    ''' </summary>
    ''' <param name="items">
    ''' The source sequence whose items will be converted to strings in enumeration order.
    ''' </param>
    ''' <returns>
    ''' A list of string keys preserving the original enumeration order.
    ''' </returns>
    Private Function MaterializeStringList(items As IEnumerable) As List(Of String)
        Dim out As New List(Of String)

        If items Is Nothing Then Return out

        For Each obj As Object In items
            out.Add(CStr(obj))
        Next

        Return out
    End Function

    ''' <summary>
    ''' Extracts the raw predictor matrix from a regression-style data object that stores
    ''' response in column 0 and imported raw predictor columns in columns 1..p.
    ''' </summary>
    ''' <param name="raw">
    ''' The imported regression data object containing response and raw predictors.
    ''' </param>
    ''' <param name="rawXKeys">
    ''' The ordered raw predictor keys expected to be present after the response column.
    ''' </param>
    ''' <returns>
    ''' A two-dimensional matrix containing only the raw predictor columns.
    ''' Returns <see langword="Nothing"/> when no raw predictors are required.
    ''' </returns>
    ''' <exception cref="ArgumentNullException">
    ''' Thrown when <paramref name="raw"/> is <see langword="Nothing"/>.
    ''' </exception>
    ''' <exception cref="ArgumentException">
    ''' Thrown when the imported column count does not match the expected
    ''' response-plus-raw-predictor layout.
    ''' </exception>
    Private Function ExtractRawPredictorMatrixFromRegressionData(raw As DataObj,
                                                                 rawXKeys As IEnumerable) As Double(,)
        If raw Is Nothing Then
            AppGlobals.BSerr.LogAndThrow(New ArgumentNullException(NameOf(raw)))
        End If

        Dim rawKeyList As List(Of String) = MaterializeStringList(rawXKeys)
        Dim pRaw As Integer = rawKeyList.Count

        If pRaw = 0 Then Return Nothing

        If raw.nCols <> pRaw + 1 Then
            AppGlobals.BSerr.LogAndThrow(
                New ArgumentException(
                    $"Regression data object contains {raw.nCols - 1} raw predictor column(s), but {pRaw} were expected."))
        End If

        Dim allData(,) As Double = raw.DataDbl
        Dim out(raw.nRows - 1, pRaw - 1) As Double

        For i As Integer = 0 To raw.nRows - 1
            For j As Integer = 0 To pRaw - 1
                out(i, j) = allData(i, j + 1)
            Next
        Next

        Return out
    End Function

    ''' <summary>
    ''' Builds the expanded predictor definitions and numeric columns for a regression model.
    ''' </summary>
    ''' <param name="rawX">
    ''' The raw imported predictor matrix only. Column order must match <paramref name="rawXKeys"/>.
    ''' </param>
    ''' <param name="rawXKeys">
    ''' The ordered raw predictor keys corresponding to the columns of <paramref name="rawX"/>.
    ''' </param>
    ''' <param name="effectItems">
    ''' The ordered authored effect list (for example, a ListBox item collection).
    ''' </param>
    ''' <param name="termSpecs">
    ''' The effect-specification dictionary describing main effects, polynomial terms,
    ''' interactions, and categorical predictors.
    ''' </param>
    ''' <param name="omitCategoricalReference">
    ''' If <see langword="True"/>, categorical predictors omit their reference level
    ''' when expanded; otherwise all observed levels are materialized.
    ''' </param>
    ''' <param name="outPredictorCols">
    ''' Returns the expanded predictor columns in model-matrix order.
    ''' </param>
    ''' <param name="outPredictorNames">
    ''' Returns the expanded predictor names aligned to <paramref name="outPredictorCols"/>.
    ''' </param>
    ''' <remarks>
    ''' This helper is regression-agnostic and intentionally does not prepend a response
    ''' column or create LinearModel-specific term groups.
    ''' </remarks>
    Private Sub BuildExpandedPredictorDefinitionLists(rawX(,) As Double,
                                                      rawXKeys As IEnumerable,
                                                      effectItems As IEnumerable,
                                                      termSpecs As Dictionary(Of String, TermSpec),
                                                      omitCategoricalReference As Boolean,
                                                      ByRef outPredictorCols As List(Of Double()),
                                                      ByRef outPredictorNames As List(Of String))

        If effectItems Is Nothing Then
            AppGlobals.BSerr.LogAndThrow(New ArgumentNullException(NameOf(effectItems)))
        End If

        Dim effects As List(Of String) = MaterializeStringList(effectItems)
        Dim rawKeyList As List(Of String) = MaterializeStringList(rawXKeys)

        Dim nRows As Integer = 0
        Dim rawColCount As Integer = 0

        If rawX IsNot Nothing Then
            nRows = UBound(rawX, 1) + 1
            rawColCount = UBound(rawX, 2) + 1
        End If

        If rawColCount <> rawKeyList.Count Then
            AppGlobals.BSerr.LogAndThrow(
                New ArgumentException(
                    $"Raw predictor matrix contains {rawColCount} column(s), but {rawKeyList.Count} raw key(s) were provided."))
        End If

        Dim baseDisplayNames As Dictionary(Of String, String) = BuildLmBaseDisplayNameMap(effects, termSpecs)
        Dim rawIndex As New Dictionary(Of String, Integer)(StringComparer.Ordinal)

        For j As Integer = 0 To rawKeyList.Count - 1
            rawIndex(rawKeyList(j)) = j
        Next

        outPredictorCols = New List(Of Double())
        outPredictorNames = New List(Of String)

        For Each effKey As String In effects
            Dim kind As String = "MainEffect"
            Dim baseKeys As List(Of String) = Nothing
            Dim degree As Integer = 1
            Dim coefName As String = Nothing
            Dim scale As PredictorScale = PredictorScale.Continuous
            Dim spec As TermSpec = Nothing

            If termSpecs IsNot Nothing AndAlso termSpecs.ContainsKey(effKey) AndAlso termSpecs(effKey) IsNot Nothing Then
                spec = termSpecs(effKey)
                kind = If(spec.Kind, "MainEffect")
                baseKeys = spec.BaseVarKeys
                degree = spec.Degree
                coefName = spec.DisplayNameForCoef
                scale = spec.Scale
            End If

            If baseKeys Is Nothing OrElse baseKeys.Count = 0 Then
                baseKeys = New List(Of String) From {effKey}
            End If

            If String.Equals(kind, "Polynomial", StringComparison.OrdinalIgnoreCase) Then
                Dim bk As String = baseKeys(0)

                If scale = PredictorScale.Categorical OrElse IsBaseVariableCategorical(bk, termSpecs) Then
                    AppGlobals.BSerr.LogAndThrow(
                        New ArgumentException($"Polynomial term '{effKey}' cannot be used with a categorical predictor."))
                End If

                If degree < 2 Then
                    AppGlobals.BSerr.LogAndThrow(
                        New ArgumentException($"Polynomial degree must be >=2 for term '{effKey}'."))
                End If

                If Not rawIndex.ContainsKey(bk) Then
                    AppGlobals.BSerr.LogAndThrow(
                        New ArgumentException($"Missing raw predictor '{bk}' required by polynomial term '{effKey}'."))
                End If

                Dim col As Integer = rawIndex(bk)
                Dim newCol(nRows - 1) As Double

                For i As Integer = 0 To nRows - 1
                    newCol(i) = PowInt(rawX(i, col), degree)
                Next

                outPredictorCols.Add(newCol)

                Dim displayBase As String = If(baseDisplayNames.ContainsKey(bk),
                                               baseDisplayNames(bk),
                                               GetCoefBaseName(bk))
                outPredictorNames.Add(If(String.IsNullOrWhiteSpace(coefName), displayBase & "^" & CStr(degree), coefName))

            ElseIf String.Equals(kind, "Interaction", StringComparison.OrdinalIgnoreCase) Then
                If baseKeys.Count < 2 Then
                    AppGlobals.BSerr.LogAndThrow(
                        New ArgumentException($"Interaction term '{effKey}' must have at least 2 base variables."))
                End If

                For Each bk As String In baseKeys
                    If IsBaseVariableCategorical(bk, termSpecs) Then
                        AppGlobals.BSerr.LogAndThrow(
                            New ArgumentException($"Interaction term '{effKey}' uses a categorical predictor. This is not implemented yet."))
                    End If
                Next

                Dim cols As New List(Of Integer)

                For Each bk As String In baseKeys
                    If Not rawIndex.ContainsKey(bk) Then
                        AppGlobals.BSerr.LogAndThrow(
                            New ArgumentException($"Missing raw predictor '{bk}' required by interaction term '{effKey}'."))
                    End If

                    cols.Add(rawIndex(bk))
                Next

                Dim newCol(nRows - 1) As Double

                For i As Integer = 0 To nRows - 1
                    Dim prod As Double = 1.0

                    For Each c As Integer In cols
                        prod *= rawX(i, c)
                    Next

                    newCol(i) = prod
                Next

                outPredictorCols.Add(newCol)

                Dim resolvedInteractionName As String = MakeResolvedInteractionDisplayName(baseKeys, baseDisplayNames)
                outPredictorNames.Add(If(String.IsNullOrWhiteSpace(coefName), resolvedInteractionName, coefName))

            Else
                Dim bk As String = baseKeys(0)

                If Not rawIndex.ContainsKey(bk) Then
                    AppGlobals.BSerr.LogAndThrow(
                        New ArgumentException($"Missing raw predictor '{bk}' required by term '{effKey}'."))
                End If

                Dim col As Integer = rawIndex(bk)
                Dim baseName As String = If(String.IsNullOrWhiteSpace(coefName),
                                            If(baseDisplayNames.ContainsKey(bk), baseDisplayNames(bk), GetCoefBaseName(bk)),
                                            coefName)

                If scale = PredictorScale.Categorical Then
                    Dim levels As List(Of Double) = GetSortedDistinctLevels(rawX, col, nRows)

                    If levels.Count < 2 Then
                        AppGlobals.BSerr.LogAndThrow(
                            New ArgumentException($"Categorical predictor '{bk}' has fewer than 2 observed levels."))
                    End If

                    Dim refVal As Double = GetReferenceLevel(levels, spec)

                    For Each lev As Double In levels
                        If omitCategoricalReference AndAlso lev = refVal Then Continue For

                        Dim newCol(nRows - 1) As Double
                        For i As Integer = 0 To nRows - 1
                            newCol(i) = If(rawX(i, col) = lev, 1.0, 0.0)
                        Next

                        outPredictorCols.Add(newCol)
                        outPredictorNames.Add($"{baseName}[{lev}]")
                    Next

                Else
                    Dim newCol(nRows - 1) As Double
                    For i As Integer = 0 To nRows - 1
                        newCol(i) = rawX(i, col)
                    Next

                    outPredictorCols.Add(newCol)
                    outPredictorNames.Add(baseName)
                End If
            End If
        Next
    End Sub

    ''' <summary>
    ''' Builds an expanded predictor matrix from imported raw predictor columns and
    ''' authored effect specifications.
    ''' </summary>
    ''' <param name="rawX">
    ''' The raw imported predictor matrix only. Column order must match <paramref name="rawXKeys"/>.
    ''' </param>
    ''' <param name="rawXKeys">
    ''' The ordered raw predictor keys corresponding to the columns of <paramref name="rawX"/>.
    ''' </param>
    ''' <param name="effectItems">
    ''' The ordered authored effect list.
    ''' </param>
    ''' <param name="termSpecs">
    ''' The term-specification dictionary describing how each effect should expand.
    ''' </param>
    ''' <param name="omitCategoricalReference">
    ''' If <see langword="True"/>, categorical predictors omit their reference level.
    ''' </param>
    ''' <param name="outX">
    ''' Returns the expanded predictor matrix, or <see langword="Nothing"/> when no
    ''' predictors are produced.
    ''' </param>
    ''' <param name="outPredictorNames">
    ''' Returns the expanded predictor names aligned to <paramref name="outX"/>.
    ''' </param>
    Public Sub BuildExpandedPredictorMatrix(rawX(,) As Double,
                                            rawXKeys As IEnumerable,
                                            effectItems As IEnumerable,
                                            termSpecs As Dictionary(Of String, TermSpec),
                                            omitCategoricalReference As Boolean,
                                            ByRef outX(,) As Double,
                                            ByRef outPredictorNames() As String)

        Dim predictorCols As List(Of Double()) = Nothing
        Dim predictorNames As List(Of String) = Nothing

        BuildExpandedPredictorDefinitionLists(rawX:=rawX,
                                              rawXKeys:=rawXKeys,
                                              effectItems:=effectItems,
                                              termSpecs:=termSpecs,
                                              omitCategoricalReference:=omitCategoricalReference,
                                              outPredictorCols:=predictorCols,
                                              outPredictorNames:=predictorNames)

        outPredictorNames = predictorNames.ToArray()

        If predictorCols.Count = 0 Then
            outX = Nothing
            Exit Sub
        End If

        Dim nRows As Integer = predictorCols(0).Length
        Dim pExpanded As Integer = predictorCols.Count

        ReDim outX(nRows - 1, pExpanded - 1)

        For j As Integer = 0 To pExpanded - 1
            For i As Integer = 0 To nRows - 1
                outX(i, j) = predictorCols(j)(i)
            Next
        Next
    End Sub

    ''' <summary>
    ''' Builds a regression data matrix in the form [Y | expanded X] from a data object
    ''' that already contains the response in column 0 and only raw predictors thereafter.
    ''' </summary>
    ''' <param name="raw">
    ''' The imported regression data object.
    ''' </param>
    ''' <param name="yKey">
    ''' The response variable key to use when naming the first output column.
    ''' </param>
    ''' <param name="effectItems">
    ''' The ordered authored effect list.
    ''' </param>
    ''' <param name="termSpecs">
    ''' The term-specification dictionary describing effect expansion.
    ''' </param>
    ''' <param name="omitCategoricalReference">
    ''' If <see langword="True"/>, categorical predictors omit their reference level.
    ''' </param>
    ''' <param name="outData">
    ''' Returns the materialized matrix [Y | expanded X].
    ''' </param>
    ''' <param name="outVarNames">
    ''' Returns the variable names aligned to <paramref name="outData"/>.
    ''' </param>
    Public Sub BuildExpandedRegressionDataMatrix(raw As DataObj,
                                                 yKey As String,
                                                 effectItems As IEnumerable,
                                                 termSpecs As Dictionary(Of String, TermSpec),
                                                 omitCategoricalReference As Boolean,
                                                 ByRef outData(,) As Double,
                                                 ByRef outVarNames() As String)

        If raw Is Nothing Then
            AppGlobals.BSerr.LogAndThrow(New ArgumentNullException(NameOf(raw)))
        End If

        Dim rawKeyList As List(Of String) = GetRequiredRawVarKeys(effectItems, termSpecs)
        Dim rawX(,) As Double = ExtractRawPredictorMatrixFromRegressionData(raw, rawKeyList)
        Dim expandedX(,) As Double = Nothing
        Dim expandedNames() As String = Nothing
        Dim rawMat(,) As Double = raw.DataDbl
        Dim nRows As Integer = raw.nRows

        BuildExpandedPredictorMatrix(rawX:=rawX,
                                     rawXKeys:=rawKeyList,
                                     effectItems:=effectItems,
                                     termSpecs:=termSpecs,
                                     omitCategoricalReference:=omitCategoricalReference,
                                     outX:=expandedX,
                                     outPredictorNames:=expandedNames)

        Dim pExpanded As Integer = If(expandedNames Is Nothing, 0, expandedNames.Length)

        ReDim outData(nRows - 1, pExpanded)
        ReDim outVarNames(pExpanded)

        outVarNames(0) = GetCoefBaseName(yKey)

        For i As Integer = 0 To nRows - 1
            outData(i, 0) = rawMat(i, 0)
        Next

        If pExpanded > 0 Then
            For j As Integer = 0 To pExpanded - 1
                outVarNames(j + 1) = expandedNames(j)
                For i As Integer = 0 To nRows - 1
                    outData(i, j + 1) = expandedX(i, j)
                Next
            Next
        End If
    End Sub

    ''' <summary>
    ''' Returns the expanded predictor names for a raw predictor matrix and authored effect set.
    ''' </summary>
    ''' <param name="rawX">
    ''' The raw imported predictor matrix only.
    ''' </param>
    ''' <param name="rawXKeys">
    ''' The ordered raw predictor keys corresponding to the columns of <paramref name="rawX"/>.
    ''' </param>
    ''' <param name="effectItems">
    ''' The ordered authored effect list.
    ''' </param>
    ''' <param name="termSpecs">
    ''' The term-specification dictionary describing effect expansion.
    ''' </param>
    ''' <param name="omitCategoricalReference">
    ''' If <see langword="True"/>, categorical predictors omit their reference level.
    ''' </param>
    ''' <returns>
    ''' The expanded predictor names in model-matrix order.
    ''' </returns>
    Public Function GetExpandedPredictorNames(rawX(,) As Double,
                                              rawXKeys As IEnumerable,
                                              effectItems As IEnumerable,
                                              termSpecs As Dictionary(Of String, TermSpec),
                                              omitCategoricalReference As Boolean) As String()

        Dim expandedX(,) As Double = Nothing
        Dim expandedNames() As String = Nothing

        BuildExpandedPredictorMatrix(rawX:=rawX,
                                     rawXKeys:=rawXKeys,
                                     effectItems:=effectItems,
                                     termSpecs:=termSpecs,
                                     omitCategoricalReference:=omitCategoricalReference,
                                     outX:=expandedX,
                                     outPredictorNames:=expandedNames)

        Return If(expandedNames, New String() {})
    End Function

    ''' <summary>
    ''' Returns the expanded predictor names for a regression-style data object that
    ''' stores response in column 0 and raw predictors thereafter.
    ''' </summary>
    ''' <param name="raw">
    ''' The imported regression data object.
    ''' </param>
    ''' <param name="effectItems">
    ''' The ordered authored effect list.
    ''' </param>
    ''' <param name="termSpecs">
    ''' The term-specification dictionary describing effect expansion.
    ''' </param>
    ''' <param name="omitCategoricalReference">
    ''' If <see langword="True"/>, categorical predictors omit their reference level.
    ''' </param>
    ''' <returns>
    ''' The expanded predictor names in model-matrix order.
    ''' </returns>
    Public Function GetExpandedPredictorNames(raw As DataObj,
                                              effectItems As IEnumerable,
                                              termSpecs As Dictionary(Of String, TermSpec),
                                              omitCategoricalReference As Boolean) As String()

        If raw Is Nothing Then
            AppGlobals.BSerr.LogAndThrow(New ArgumentNullException(NameOf(raw)))
        End If

        Dim rawKeyList As List(Of String) = GetRequiredRawVarKeys(effectItems, termSpecs)
        Dim rawX(,) As Double = ExtractRawPredictorMatrixFromRegressionData(raw, rawKeyList)

        Return GetExpandedPredictorNames(rawX:=rawX,
                                         rawXKeys:=rawKeyList,
                                         effectItems:=effectItems,
                                         termSpecs:=termSpecs,
                                         omitCategoricalReference:=omitCategoricalReference)
    End Function


    ''' <summary>
    ''' Build custom term groups for LinearModel.Fit() term-wise ANOVA.
    ''' Column indices refer to the design matrix X inside LinearModel (i.e. include intercept shift).
    ''' 
    ''' Grouping rule:
    '''  - MainEffect/Polynomial: grouped by base variable (GetCoefBaseName(baseKey)), so Age + Age^2 -> one term.
    '''  - Interaction: one term per interaction (DisplayNameForCoef by default).
    ''' </summary>
    Public Function BuildCustomTermGroupsForLm(effectItems As IEnumerable,
                                           termSpecs As Dictionary(Of String, TermSpec),
                                           includeIntercept As Boolean,
                                           Optional rawBaseDisplayNames As IDictionary(Of String, String) = Nothing) As Dictionary(Of String, Integer())

        Dim groups As New Dictionary(Of String, List(Of Integer))(StringComparer.Ordinal)
        Dim colOffset As Integer = If(includeIntercept, 1, 0)

        'Optional: include intercept group
        If includeIntercept Then
            groups("Intercept") = New List(Of Integer) From {0}
        End If

        'For interaction-name collision handling
        Dim usedGroupNames As New HashSet(Of String)(StringComparer.Ordinal)
        If includeIntercept Then usedGroupNames.Add("Intercept")

        Dim effects As New List(Of String)
        For Each obj As Object In effectItems
            effects.Add(CStr(obj))
        Next

        Dim baseDisplayNames As New Dictionary(Of String, String)(StringComparer.Ordinal)
        If rawBaseDisplayNames IsNot Nothing Then
            For Each kvp As KeyValuePair(Of String, String) In rawBaseDisplayNames
                Dim resolvedName As String = If(kvp.Value, String.Empty).Trim()
                If resolvedName = String.Empty Then resolvedName = GetCoefBaseName(kvp.Key)
                baseDisplayNames(kvp.Key) = resolvedName
            Next
        End If

        For e As Integer = 0 To effects.Count - 1
            Dim effKey As String = effects(e)
            Dim xCol As Integer = e + colOffset

            Dim kind As String = "MainEffect"
            Dim baseKeys As List(Of String) = Nothing
            Dim coefName As String = Nothing

            If termSpecs IsNot Nothing AndAlso termSpecs.ContainsKey(effKey) AndAlso termSpecs(effKey) IsNot Nothing Then
                Dim spec As TermSpec = termSpecs(effKey)
                kind = If(spec.Kind, "MainEffect")
                baseKeys = spec.BaseVarKeys
                coefName = spec.DisplayNameForCoef
            End If

            If baseKeys Is Nothing OrElse baseKeys.Count = 0 Then
                baseKeys = New List(Of String) From {effKey}
            End If

            Dim groupName As String

            If String.Equals(kind, "Interaction", StringComparison.OrdinalIgnoreCase) Then
                'Interaction term name for ANOVA
                If String.IsNullOrEmpty(coefName) Then
                    coefName = MakeResolvedInteractionDisplayName(baseKeys, baseDisplayNames)
                End If
                groupName = coefName

                'Ensure unique group name
                If usedGroupNames.Contains(groupName) Then
                    Dim k As Integer = 2
                    Dim candidate As String = $"{groupName} ({k})"
                    While usedGroupNames.Contains(candidate)
                        k += 1
                        candidate = $"{groupName} ({k})"
                    End While
                    groupName = candidate
                End If

            Else
                'MainEffect or Polynomial -> group by the user-facing base variable name (so poly joins main)
                Dim baseKey As String = baseKeys(0)
                Dim baseDisplayName As String = Nothing

                If baseDisplayNames.ContainsKey(baseKey) Then
                    baseDisplayName = baseDisplayNames(baseKey)
                ElseIf Not String.IsNullOrWhiteSpace(coefName) AndAlso Not String.Equals(kind, "Polynomial", StringComparison.OrdinalIgnoreCase) Then
                    baseDisplayName = coefName
                Else
                    baseDisplayName = GetCoefBaseName(baseKey)
                End If

                If String.IsNullOrWhiteSpace(baseDisplayName) Then
                    baseDisplayName = GetCoefBaseName(baseKey)
                End If

                groupName = baseDisplayName
            End If

            usedGroupNames.Add(groupName)

            If Not groups.ContainsKey(groupName) Then groups(groupName) = New List(Of Integer)
            groups(groupName).Add(xCol)
        Next

        'Convert lists to arrays
        Dim out As New Dictionary(Of String, Integer())(StringComparer.Ordinal)
        For Each kvp In groups
            Dim arr As Integer() = kvp.Value.Distinct().OrderBy(Function(z) z).ToArray()
            out(kvp.Key) = arr
        Next

        Return out
    End Function

    Public Function BuildCategoricalReferenceFootnotesForLm(raw As glmData,
                                                    effectItems As IEnumerable,
                                                    termSpecs As Dictionary(Of String, TermSpec),
                                                    includeIntercept As Boolean,
                                                    Optional rawBaseDisplayNames As IDictionary(Of String, String) = Nothing) As List(Of String)

        Dim notes As New List(Of String)

        If raw Is Nothing OrElse effectItems Is Nothing OrElse termSpecs Is Nothing Then
            Return notes
        End If

        Dim rawMat(,) As Double = raw.DataDbl
        Dim rawXKeys As List(Of String) = GetRequiredRawVarKeys(effectItems, termSpecs)
        Dim rawIndex As New Dictionary(Of String, Integer)(StringComparer.Ordinal)

        For j As Integer = 0 To rawXKeys.Count - 1
            rawIndex(rawXKeys(j)) = j + 1
        Next

        Dim baseDisplayNames As New Dictionary(Of String, String)(StringComparer.Ordinal)
        If rawBaseDisplayNames IsNot Nothing Then
            For Each kvp As KeyValuePair(Of String, String) In rawBaseDisplayNames
                Dim resolvedName As String = If(kvp.Value, String.Empty).Trim()
                If resolvedName = String.Empty Then resolvedName = GetCoefBaseName(kvp.Key)
                baseDisplayNames(kvp.Key) = resolvedName
            Next
        End If
        If baseDisplayNames.Count = 0 Then
            baseDisplayNames = BuildLmBaseDisplayNameMap(effectItems, termSpecs)
        End If
        Dim seen As New HashSet(Of String)(StringComparer.Ordinal)

        For Each obj As Object In effectItems
            Dim effKey As String = CStr(obj)

            If Not termSpecs.ContainsKey(effKey) Then Continue For
            Dim spec As TermSpec = termSpecs(effKey)
            If spec Is Nothing Then Continue For
            If spec.Scale <> PredictorScale.Categorical Then Continue For
            If spec.BaseVarKeys Is Nothing OrElse spec.BaseVarKeys.Count = 0 Then Continue For

            Dim bk As String = spec.BaseVarKeys(0)
            If seen.Contains(bk) Then Continue For
            seen.Add(bk)

            If Not rawIndex.ContainsKey(bk) Then Continue For

            Dim displayName As String = If(baseDisplayNames.ContainsKey(bk), baseDisplayNames(bk), GetCoefBaseName(bk))
            Dim levels As List(Of Double) = GetSortedDistinctLevels(rawMat, rawIndex(bk), raw.nRows)

            If includeIntercept Then
                Dim refVal As Double = GetReferenceLevel(levels, spec)
                notes.Add($"Categorical predictor (Factor): {displayName}; reference level = {refVal}.")
            Else
                notes.Add($"Categorical predictor (Factor): {displayName}; model fit without intercept, so no reference level was omitted.")
            End If
        Next

        Return notes
    End Function

End Module
