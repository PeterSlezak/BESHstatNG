Option Explicit On
Option Strict On

Imports System
Imports System.Collections.Generic
Imports System.Globalization
Imports System.Linq
Imports System.Text
Imports System.Text.RegularExpressions
Imports ExcelDna.Integration

Namespace WorksheetFunctions

    ''' <summary>
    ''' Worksheet functions for validating regression-model formula strings before fitting a model.
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' The functions in this module validate the same formula grammar and addressing rules used by the worksheet regression fit UDFs
    ''' that support a <c>formula</c> parameter, such as <c>BESH.SURV.COX_FIT</c> and <c>BESH.REGR.ORDLOGIT_FIT</c>.
    ''' </para>
    ''' <para>
    ''' Validation is performed against the supplied raw predictor matrix <c>x</c>, predictor names, and formula-addressing mode.
    ''' This means the checker can catch not only pure syntax errors, but also semantic issues such as unknown variables,
    ''' invalid factor specifications, unsupported interaction patterns, and addressing-mode mismatches.
    ''' </para>
    ''' </remarks>
    Public Module RegressionFormulaUDFs

        ''' <summary>
        ''' Validates a regression-model formula string against the raw predictor matrix and returns TRUE when validation succeeds.
        ''' </summary>
        ''' <param name="formula">
        ''' The right-hand-side model formula to validate.
        ''' Supported syntax currently includes additive terms (<c>A + B</c>), polynomial terms (<c>A^2</c>),
        ''' continuous-continuous interactions (<c>A:B</c>, <c>A:B:C</c>), categorical main effects such as
        ''' <c>factor(C)</c> or <c>factor(C, ref=2)</c>, categorical-continuous interactions such as
        ''' <c>factor(C):A</c> or <c>factor(C, ref=1):A</c>, and categorical-categorical interactions such as
        ''' <c>factor(C):factor(D)</c>.
        ''' Blank text is considered valid and corresponds to the default design that uses all predictor columns as continuous main effects.
        ''' </param>
        ''' <param name="x">
        ''' The raw predictor matrix that would be supplied to the corresponding regression fit UDF.
        ''' The validator uses this matrix to determine the number of raw predictors and, when needed, the absolute worksheet column letters.
        ''' </param>
        ''' <param name="varNames">
        ''' Optional raw predictor names.
        ''' This may be supplied as a comma-separated text string or as a one-row or one-column range containing one name per raw predictor column.
        ''' These names are used when <paramref name="formulaAddressing"/> is set to <c>names</c>.
        ''' </param>
        ''' <param name="formulaAddressing">
        ''' Optional formula-addressing mode that controls how bare column-letter tokens are interpreted.
        ''' Accepted values are <c>relative</c> (default), <c>absolute</c>, and <c>names</c>.
        ''' In <c>relative</c> mode, bare letters such as <c>A</c> and <c>B</c> refer to columns 1 and 2 of <paramref name="x"/>.
        ''' In <c>absolute</c> mode, bare letters refer to worksheet columns of the supplied <paramref name="x"/> range.
        ''' In <c>names</c> mode, bare letters are disabled and variables should be referenced using single-quoted names such as <c>'dose'</c>.
        ''' Single quotes inside names are escaped by doubling them, e.g. <c>'Children''s dose'</c>.
        ''' </param>
        ''' <returns>
        ''' TRUE when validation succeeds.
        ''' If validation fails, returns a descriptive text message that includes the parser or design-build error,
        ''' a best-effort indication of the offending fragment, and context about the active addressing mode and available predictor references.
        ''' </returns>
        ''' <remarks>
        ''' <para>
        ''' This function validates formulas by using the same parser and design-matrix infrastructure that the supported regression fit UDFs use internally.
        ''' As a result, a formula that returns TRUE here is expected to satisfy the formula grammar and addressing rules during model fitting as well,
        ''' provided that the same <paramref name="x"/>, <paramref name="varNames"/>, and <paramref name="formulaAddressing"/> inputs are used.
        ''' </para>
        ''' <para>
        ''' The formula grammar supports interactions involving <c>factor(...)</c>.  Polynomial subterms inside interactions
        ''' and repeated variables inside one interaction term remain unsupported; write polynomial terms separately and then
        ''' interact the raw variables only when needed.
        ''' </para>
        ''' <para>
        ''' When <c>formulaAddressing="absolute"</c> is used, the <paramref name="x"/> argument must be passed as a direct worksheet range so that the validator
        ''' can determine the absolute worksheet column letters that are available to the formula.
        ''' </para>
        ''' </remarks>
        ''' <example>
        ''' <code>
        ''' =BESH.REGR.FORMULA_VALIDATE("A + A^2 + factor(C, ref=1) + B:D", C2:F101, "prison,dose,stage,treat")
        ''' =BESH.REGR.FORMULA_VALIDATE("factor(stage, ref=1) + dose + factor(stage, ref=1):dose + factor(stage):factor(treat)", C2:F101, "prison,dose,stage,treat", "names")
        ''' =BESH.REGR.FORMULA_VALIDATE("factor(E, ref=1):C", C2:F101, "prison,dose,stage,treat", "absolute")
        ''' </code>
        ''' </example>
        <ExcelFunction(
            Name:="BESH.REGR.FORMULA_VALIDATE",
            Category:="BESHStatNG - Regression Models",
            Description:="Validates a regression-model formula string and returns TRUE or a descriptive validation message.",
            HelpTopic:=HelpLinks.FallbackBaseUrl & "/udf/regression-formula-syntax/"
        )>
        Public Function FORMULA_VALIDATE(
            <ExcelArgument(Name:="formula", Description:="Formula text to validate, e.g. ""A + factor(C) + factor(C):B"". Blank text means all predictors as continuous main effects.")> formula As Object,
            <ExcelArgument(AllowReference:=True, Name:="x", Description:="Raw predictor matrix that defines the available formula variables.")> x As Object,
            <ExcelArgument(Name:="varNames", Description:="Optional predictor names as a comma-separated list or a one-row/one-column range.")> Optional varNames As Object = Nothing,
            <ExcelArgument(Name:="formulaAddressing", Description:="Addressing mode: ""relative"" (default), ""absolute"", or ""names"".")> Optional formulaAddressing As Object = Nothing
        ) As Object

            If ExcelDnaUtil.IsInFunctionWizard() Then
                Return "FORMULA_VALIDATE (editing...)"
            End If

            Try
                Dim xMat As Double(,) = Nothing
                Dim rowCount As Integer = 0
                Dim colCount As Integer = 0
                If Not Global.BESHStatNG.UdfDataImport.TryGetNumericMatrix(x, xMat, rowCount, colCount) Then
                    Return "Validation failed: the raw predictor matrix x could not be read as a numeric matrix. Ensure x matches the range you plan to use when fitting the model."
                End If
                If colCount < 1 Then
                    Return "Validation failed: the raw predictor matrix x must contain at least one predictor column."
                End If

                Dim formulaText As String = AsString(formula)
                If formulaText Is Nothing Then formulaText = String.Empty

                Dim rawVarNames As String() = Global.BESHStatNG.UdfDataImport.GetVariableNames(varNames, colCount)
                Dim addressingMode As String = Global.BESHStatNG.UdfDataImport.GetFormulaAddressingMode(formulaAddressing, "relative")

                Dim allowRelativeColumnLetters As Boolean = False
                Dim allowAbsoluteColumnLetters As Boolean = False
                Dim allowQuotedVariableNames As Boolean = True

                Select Case addressingMode
                    Case "absolute"
                        allowAbsoluteColumnLetters = True
                    Case "names"
                        allowQuotedVariableNames = True
                    Case Else
                        allowRelativeColumnLetters = True
                        addressingMode = "relative"
                End Select

                Dim absoluteColumnLetters As String() = Nothing
                If allowAbsoluteColumnLetters AndAlso Not String.IsNullOrWhiteSpace(formulaText) Then
                    If Not Global.BESHStatNG.UdfDataImport.TryGetAbsoluteColumnLetters(x, colCount, absoluteColumnLetters) Then
                        Return "Validation failed: formulaAddressing='absolute' requires x to be passed as a direct worksheet range with " &
                               colCount.ToString(CultureInfo.InvariantCulture) &
                               " predictor column(s), so absolute worksheet column letters can be determined."
                    End If
                End If

                Dim designBuild As RegressionFormulaMatrixBuildResult = Nothing
                Dim designErr As String = Nothing
                If RegressionFormulaDesignService.TryBuildExpandedPredictorMatrixFromFormula(rawX:=xMat,
                                                                                           result:=designBuild,
                                                                                           errorMessage:=designErr,
                                                                                           predictorNames:=rawVarNames,
                                                                                           formulaText:=formulaText,
                                                                                           absoluteColumnLetters:=absoluteColumnLetters,
                                                                                           allowRelativeColumnLetters:=allowRelativeColumnLetters,
                                                                                           allowAbsoluteColumnLetters:=allowAbsoluteColumnLetters,
                                                                                           allowQuotedVariableNames:=allowQuotedVariableNames,
                                                                                           omitCategoricalReference:=True) Then
                    Return True
                End If

                Return BuildValidationFailureMessage(formulaText:=formulaText,
                                                     addressingMode:=addressingMode,
                                                     rawVarNames:=rawVarNames,
                                                     absoluteColumnLetters:=absoluteColumnLetters,
                                                     parserOrBuilderMessage:=designErr)

            Catch ex As Exception
                Return LoggedUdfError("BESH.REGR.FORMULA_VALIDATE", ex, Nothing, "Validation failed: ")
            End Try
        End Function

        ''' <summary>
        ''' Builds a user-facing validation message from a lower-level parser or design-build error.
        ''' </summary>
        ''' <param name="formulaText">The original formula text supplied by the user.</param>
        ''' <param name="addressingMode">The resolved addressing mode used for validation.</param>
        ''' <param name="rawVarNames">The raw predictor names aligned with the columns of x.</param>
        ''' <param name="absoluteColumnLetters">The absolute worksheet column letters aligned with x, when available.</param>
        ''' <param name="parserOrBuilderMessage">The original parser or design-build error message.</param>
        ''' <returns>A descriptive validation message suitable for display in Excel.</returns>
        Private Function BuildValidationFailureMessage(formulaText As String,
                                                       addressingMode As String,
                                                       rawVarNames As String(),
                                                       absoluteColumnLetters As String(),
                                                       parserOrBuilderMessage As String) As String

            Dim sb As New StringBuilder()
            Dim msg As String = If(parserOrBuilderMessage, "Unknown validation error.").Trim()

            sb.Append("Validation failed: ")
            sb.Append(msg)

            Dim fragment As String = Nothing
            Dim fragmentPos As Integer = -1
            If TryLocateOffendingFragment(formulaText, msg, fragment, fragmentPos) Then
                sb.Append(" Offending fragment: ")
                sb.Append(fragment)
                If fragmentPos >= 0 Then
                    sb.Append(" (starting near character ")
                    sb.Append((fragmentPos + 1).ToString(CultureInfo.InvariantCulture))
                    sb.Append(").")
                Else
                    sb.Append(".")
                End If
            End If

            If Not String.IsNullOrWhiteSpace(formulaText) Then
                sb.Append(" Formula: ")
                sb.Append(formulaText.Trim())
                sb.Append(".")
            End If

            sb.Append(" Addressing mode: ")
            sb.Append(addressingMode)
            sb.Append(".")

            Dim refs As String = BuildAvailableReferenceSummary(addressingMode, rawVarNames, absoluteColumnLetters)
            If refs <> String.Empty Then
                sb.Append(" ")
                sb.Append(refs)
            End If

            Return sb.ToString()
        End Function

        ''' <summary>
        ''' Attempts to find the formula fragment that most likely caused a validation error.
        ''' </summary>
        ''' <param name="formulaText">The original formula text.</param>
        ''' <param name="errorMessage">The parser or design-build error message.</param>
        ''' <param name="fragment">On success, receives the likely offending fragment.</param>
        ''' <param name="position">On success, receives the zero-based position of the fragment in the original formula when it can be found.</param>
        ''' <returns>True when a likely offending fragment can be identified; otherwise, False.</returns>
        Private Function TryLocateOffendingFragment(formulaText As String,
                                                    errorMessage As String,
                                                    ByRef fragment As String,
                                                    ByRef position As Integer) As Boolean

            fragment = Nothing
            position = -1

            Dim formula As String = If(formulaText, String.Empty)
            Dim err As String = If(errorMessage, String.Empty)
            If formula = String.Empty OrElse err = String.Empty Then Return False

            Dim candidates As New List(Of String)()

            For Each m As Match In Regex.Matches(err, "'((?:[^']|'')*)'")
                If m.Success Then
                    Dim body As String = m.Groups(1).Value
                    If body <> String.Empty Then candidates.Add(body)
                End If
            Next

            For Each candidate As String In candidates.Distinct(StringComparer.Ordinal)
                Dim exactQuoted As String = "'" & candidate.Replace("'", "''") & "'"
                Dim idx As Integer = formula.IndexOf(exactQuoted, StringComparison.OrdinalIgnoreCase)
                If idx >= 0 Then
                    fragment = exactQuoted
                    position = idx
                    Return True
                End If

                idx = formula.IndexOf(candidate, StringComparison.OrdinalIgnoreCase)
                If idx >= 0 Then
                    fragment = candidate
                    position = idx
                    Return True
                End If
            Next

            Dim factorIx As Integer = err.IndexOf("factor(", StringComparison.OrdinalIgnoreCase)
            If factorIx >= 0 Then
                Dim factorFormulaIx As Integer = formula.IndexOf("factor(", StringComparison.OrdinalIgnoreCase)
                If factorFormulaIx >= 0 Then
                    fragment = ExtractBalancedFunctionCall(formula, factorFormulaIx)
                    position = factorFormulaIx
                    Return fragment <> String.Empty
                End If
            End If

            If err.IndexOf("interaction", StringComparison.OrdinalIgnoreCase) >= 0 AndAlso formula.IndexOf(":"c) >= 0 Then
                fragment = formula.Trim()
                position = formula.IndexOf(":"c)
                Return True
            End If

            If err.IndexOf("polynomial", StringComparison.OrdinalIgnoreCase) >= 0 AndAlso formula.IndexOf("^"c) >= 0 Then
                fragment = formula.Trim()
                position = formula.IndexOf("^"c)
                Return True
            End If

            Return False
        End Function

        ''' <summary>
        ''' Extracts a balanced function call such as factor(...), starting from the supplied index.
        ''' </summary>
        ''' <param name="text">The source text containing the function call.</param>
        ''' <param name="startIndex">The zero-based index of the first character of the function name.</param>
        ''' <returns>The balanced function-call text when it can be extracted; otherwise, an empty string.</returns>
        Private Function ExtractBalancedFunctionCall(text As String, startIndex As Integer) As String
            Dim s As String = If(text, String.Empty)
            If startIndex < 0 OrElse startIndex >= s.Length Then Return String.Empty

            Dim depth As Integer = 0
            Dim inQuotes As Boolean = False
            Dim i As Integer = startIndex

            While i < s.Length
                Dim ch As Char = s(i)

                If ch = "'"c Then
                    If inQuotes Then
                        If i + 1 < s.Length AndAlso s(i + 1) = "'"c Then
                            i += 2
                            Continue While
                        Else
                            inQuotes = False
                        End If
                    Else
                        inQuotes = True
                    End If
                ElseIf Not inQuotes Then
                    If ch = "("c Then
                        depth += 1
                    ElseIf ch = ")"c Then
                        depth -= 1
                        If depth = 0 Then
                            Return s.Substring(startIndex, i - startIndex + 1).Trim()
                        End If
                    End If
                End If

                i += 1
            End While

            Return s.Substring(startIndex).Trim()
        End Function

        ''' <summary>
        ''' Builds a short summary of the variable references that are available under the active addressing mode.
        ''' </summary>
        ''' <param name="addressingMode">The resolved formula-addressing mode.</param>
        ''' <param name="rawVarNames">The predictor names aligned with x.</param>
        ''' <param name="absoluteColumnLetters">The absolute worksheet column letters aligned with x, when available.</param>
        ''' <returns>A concise user-facing reference summary.</returns>
        Private Function BuildAvailableReferenceSummary(addressingMode As String,
                                                        rawVarNames As String(),
                                                        absoluteColumnLetters As String()) As String

            Dim names As List(Of String) = If(rawVarNames, Array.Empty(Of String)()).Select(Function(x) If(x, String.Empty).Trim()).Where(Function(x) x <> String.Empty).ToList()
            Dim p As Integer = names.Count
            If p = 0 Then Return String.Empty

            Select Case addressingMode
                Case "absolute"
                    If absoluteColumnLetters IsNot Nothing AndAlso absoluteColumnLetters.Length = p Then
                        Return "Available absolute worksheet columns: " & FormatSampleList(absoluteColumnLetters) & "."
                    End If
                    Return "Available predictors: " & FormatSampleList(names.Select(Function(n) "'" & n.Replace("'", "''") & "'").ToArray()) & "."

                Case "names"
                    Return "Available variable names: " & FormatSampleList(names.Select(Function(n) "'" & n.Replace("'", "''") & "'").ToArray()) & "."

                Case Else
                    Dim rel As String() = Enumerable.Range(1, p).Select(Function(i) RegressionVariableCatalog.NumberToLetters(i)).ToArray()
                    Return "Available relative X-columns: " & FormatSampleList(rel) & "."
            End Select
        End Function

        ''' <summary>
        ''' Formats the first few values of a list for concise display.
        ''' </summary>
        ''' <param name="items">The values to format.</param>
        ''' <returns>A concise comma-separated list with truncation when needed.</returns>
        Private Function FormatSampleList(items As IEnumerable(Of String)) As String
            Dim vals As List(Of String) = If(items, Array.Empty(Of String)()).Where(Function(x) Not String.IsNullOrWhiteSpace(x)).ToList()
            If vals.Count = 0 Then Return String.Empty

            Const maxShown As Integer = 8
            If vals.Count <= maxShown Then
                Return String.Join(", ", vals)
            End If

            Return String.Join(", ", vals.Take(maxShown)) & ", ..."
        End Function

    End Module
End Namespace
