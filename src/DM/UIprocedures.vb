Option Explicit On
Imports BESHStatNG.AppInfrastructure
Imports ExcelDna.Integration 'for the excelmissing constant
Imports Microsoft.Office.Interop.Excel

''' <summary>
''' A collection of user‑interface helper procedures for Excel‑based statistical
''' tools. Provides utilities for:
''' <list type="bullet">
'''   <item><description>Extracting variable names from worksheet ranges</description></item>
'''   <item><description>Managing ListBox selections and preventing duplicates</description></item>
'''   <item><description>Validating numeric input and reference ranges</description></item>
'''   <item><description>Checking worksheet areas for emptiness before writing results</description></item>
'''   <item><description>Parsing numeric lists from text boxes</description></item>
'''   <item><description>Synchronizing UI controls (ListBoxes, TextBoxes, RefEdits)</description></item>
''' </list>
''' These procedures are used throughout the UI layer of the statistical add‑in.
''' </summary>
Public Module UIprocedures

    ''' <summary>
    ''' Describes a worksheet column as presented to the UI (typically in a ListBox).
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' Instances of this class are commonly produced by <c>VarNamesToLBox</c> and stored in a dictionary
    ''' keyed by <see cref="DisplayText"/>. The <see cref="DisplayText"/> value is intended to match the
    ''' exact string shown in the UI (e.g. <c>"Age | VarA"</c> or <c>"VarB"</c>) and should be stable and
    ''' unique per worksheet column.
    ''' </para>
    ''' <para>
    ''' Use <see cref="ColumnNumber"/> and <see cref="ColumnLetter"/> to map the UI item back to the
    ''' underlying Excel column for range/reference creation.
    ''' </para>
    ''' </remarks>
    Public Class VarColumnInfo

        ''' <summary>
        ''' Gets or sets the 1-based Excel column number (e.g. 1 for column A).
        ''' </summary>
        ''' <remarks>
        ''' This value corresponds to <c>Range.Column</c> for the header cell in the worksheet.
        ''' </remarks>
        Public Property ColumnNumber As Integer

        ''' <summary>
        ''' Gets or sets the Excel column letter(s) (e.g. <c>A</c>, <c>B</c>, <c>AA</c>).
        ''' </summary>
        ''' <remarks>
        ''' This is useful for creating column references such as <c>$A:$A</c> without COM calls.
        ''' </remarks>
        Public Property ColumnLetter As String

        ''' <summary>
        ''' Gets or sets the raw header text for the column.
        ''' </summary>
        ''' <remarks>
        ''' When the worksheet header is missing or invalid, this may be a placeholder such as <c>"VarA"</c>.
        ''' This value does not include the <c>" | VarX"</c> suffix used in <see cref="DisplayText"/>.
        ''' </remarks>
        Public Property HeaderText As String

        ''' <summary>
        ''' Gets or sets a value indicating whether <see cref="HeaderText"/> came from the worksheet header row.
        ''' </summary>
        ''' <remarks>
        ''' <para>
        ''' If <c>True</c>, <see cref="HeaderText"/> reflects the value found in the header cell (typically row 1).
        ''' If <c>False</c>, <see cref="HeaderText"/> is a generated placeholder (e.g. <c>"VarA"</c>).
        ''' </para>
        ''' </remarks>
        Public Property HasHeader As Boolean

        ''' <summary>
        ''' Gets or sets the index of this item in the ListBox at the time it was populated.
        ''' </summary>
        ''' <remarks>
        ''' This can be used to restore selections or correlate UI items with their metadata.
        ''' Note that indices may change if the ListBox is rebuilt or items are sorted.
        ''' </remarks>
        Public Property ListBoxIndex As Integer

        ''' <summary>
        ''' Gets or sets the exact text displayed in the ListBox for this column.
        ''' </summary>
        ''' <remarks>
        ''' <para>
        ''' This is typically either:
        ''' </para>
        ''' <list type="bullet">
        '''   <item><description><c>"{HeaderText} | Var{ColumnLetter}"</c> when the header exists</description></item>
        '''   <item><description><c>"Var{ColumnLetter}"</c> when a placeholder name is used</description></item>
        ''' </list>
        ''' <para>
        ''' This value is commonly used as the dictionary key for column metadata lookups.
        ''' </para>
        ''' </remarks>
        Public Property DisplayText As String

    End Class



    ''' <summary>
    ''' Extracts variable names from an Excel range and populates a ListBox with
    ''' human‑readable labels. Returns a dictionary mapping column numbers to
    ''' metadata describing each variable.
    ''' </summary>
    ''' <param name="VarRng">The Excel range containing variable headers.</param>
    ''' <param name="MaxRows">Maximum number of rows to scan for numeric data.</param>
    ''' <param name="lbox">The ListBox to populate.</param>
    ''' <param name="bNumeric_only">
    ''' If True, only columns containing numeric data are included.
    ''' </param>
    ''' <returns>
    ''' A dictionary where each key is a column index and each value is an array:
    ''' <list type="bullet">
    '''   <item><description><c>(0)</c> — variable name or placeholder</description></item>
    '''   <item><description><c>(1)</c> — Boolean: True if name came from header</description></item>
    '''   <item><description><c>(2)</c> — ListBox index</description></item>
    '''   <item><description><c>(3)</c> — final display text</description></item>
    ''' </list>
    ''' </returns>
    Public Function VarNamesToLBox(VarRng As Range,
                                   MaxRows As Integer,
                                   lbox As System.Windows.Forms.ListBox,
                                   Optional bNumeric_only As Boolean = True) As Dictionary(Of String, VarColumnInfo)

        Dim cell As Object, app As Application
        Dim ws As Worksheet = VarRng.Parent
        app = AppGlobals.app

        Dim out As New Dictionary(Of String, VarColumnInfo)(StringComparer.Ordinal)

        Dim i As Integer = 0
        For Each cell In VarRng
            Dim colLetter As String = ColName(cell)
            Dim displayText As String = String.Empty
            Dim headerText As String = String.Empty
            Dim hasHeader As Boolean = False

            If app.WorksheetFunction.IsNA(cell) Or TypeOf cell.value Is ExcelError _
               Or TypeOf cell.value Is ExcelEmpty Or TypeOf cell.value Is ExcelMissing _
               Or cell.value Is Nothing Then

                If CountNonmissing(ws.Range(ws.Cells(1, cell.Column), ws.Cells(MaxRows, cell.Column)), bNumeric_only) > 0 Then
                    headerText = "Var" & colLetter
                    displayText = headerText
                    hasHeader = False
                End If

            ElseIf CountNonmissing(ws.Range(ws.Cells(1, cell.Column), ws.Cells(MaxRows, cell.Column)), bNumeric_only) > 0 Then
                headerText = CStr(cell.value)
                displayText = headerText & " | Var" & colLetter
                hasHeader = True

            Else
                If CountNonmissing(ws.Range(ws.Cells(1, cell.Column), ws.Cells(MaxRows, cell.Column)), bNumeric_only) > 0 Then
                    headerText = "Var" & colLetter
                    displayText = headerText
                    hasHeader = False
                End If
            End If

            If displayText <> String.Empty Then
                lbox.Items.Add(displayText)

                Dim info As New VarColumnInfo With {.ColumnNumber = cell.Column,
                                                    .ColumnLetter = colLetter,
                                                    .HeaderText = headerText,
                                                    .HasHeader = hasHeader,
                                                    .ListBoxIndex = i,
                                                    .DisplayText = displayText}
                out.Add(displayText, info)
                i += 1
            End If
        Next cell

        Return out
    End Function


    ''' <summary>
    ''' Adds a single selected item from one ListBox to another, ensuring that:
    ''' <list type="bullet">
    '''   <item><description>Only one item is selected</description></item>
    '''   <item><description>The destination ListBox is empty</description></item>
    '''   <item><description>The item is not already selected in any of the provided check‑ListBoxes</description></item>
    ''' </list>
    ''' Displays MsgBox warnings when invalid selections occur.
    ''' </summary>
    ''' <param name="lbox_to">Destination ListBox.</param>
    ''' <param name="lbox_from">Source ListBox.</param>
    ''' <param name="lbox_tocheck">Optional ListBoxes to check for duplicates.</param>
    Sub AddItemToListbox(lbox_to As Object, lbox_from As Object, ParamArray lbox_tocheck() As Object)
        'add item to lbox_to listbox from the lbox_from listbox, lbox_to should contain only 1 variable if
        ' lbox_tocheck is provided then it's checked whether variable was already selected in these listboxes

        'Test for multiple selections
        If lbox_from.SelectedItems.count > 1 Then
            MsgBox("Please select a single variable.", vbExclamation, "Input Error!")
            Exit Sub
        Else 'Check that the lbox_to listbox is empty
            If lbox_to.Items.Count = 0 Then 'Test to see if a selection has been made
                If lbox_from.SelectedItems.count = 1 Then 'some X response variables has been already selected
                    For jj As Integer = 0 To lbox_tocheck.GetUpperBound(0)
                        For ii As Integer = 0 To lbox_tocheck(jj).Items.Count - 1
                            'this variable has been already selected as predictor
                            If lbox_from.Items(lbox_from.SelectedIndex) = lbox_tocheck(jj).Items(ii) Then
                                MsgBox("Variable has already been selected!", vbExclamation, "Input Error!")
                                Exit Sub
                            End If
                        Next
                    Next
                    lbox_to.Items.Add(lbox_from.Items(lbox_from.SelectedIndex))
                Else
                    MsgBox("Please select a variable.", vbExclamation, "Input Error!")
                End If
            Else
                MsgBox("Variable has already been chosen.", vbExclamation, "Input Error!")
            End If
        End If
    End Sub

    Private Function IsDerivedEffectKey(effectText As String) As Boolean
        Dim s As String = If(effectText, String.Empty).Trim()
        If s = String.Empty Then Return False

        'Categorical effect is a main effect, not a derived term
        If s.StartsWith(RegressionDesignCore.CATEGORICAL_EFFECT_PREFIX, StringComparison.Ordinal) Then
            Return False
        End If

        'Polynomial: "<base>"^k or <base>^k
        Dim pCaret As Integer = InStrRev(s, "^")
        If pCaret > 0 AndAlso pCaret < Len(s) Then
            Dim expStr As String = Mid$(s, pCaret + 1)
            Dim expVal As Integer
            If Integer.TryParse(expStr, expVal) Then
                Return True
            End If
        End If

        'Interaction: "A":"B":...
        If InStr(s, ":") > 0 AndAlso InStr(s, ChrW(34)) > 0 Then
            Dim keys As List(Of String) = RegressionDesignCore.ExtractBaseKeysFromEffectText(s)
            If keys.Count >= 2 Then Return True
        End If

        Return False
    End Function

    Private Function ContainsAnyBaseKey(baseKeys As List(Of String),
                                    removedBaseKeys As HashSet(Of String)) As Boolean
        If baseKeys Is Nothing Then Return False

        For Each bk As String In baseKeys
            If removedBaseKeys.Contains(bk) Then Return True
        Next

        Return False
    End Function

    Private Sub SyncTermSpecsToListBoxItems(lbox As Object,
                                        termSpecs As Dictionary(Of String, TermSpec))
        If termSpecs Is Nothing Then Exit Sub

        Dim keep As New HashSet(Of String)(StringComparer.Ordinal)
        For Each it As Object In lbox.Items
            keep.Add(CStr(it))
        Next

        Dim stale As New List(Of String)
        For Each k As String In termSpecs.Keys
            If Not keep.Contains(k) Then stale.Add(k)
        Next

        For Each k As String In stale
            termSpecs.Remove(k)
        Next

        RegressionDesignCore.RefreshTermSpecOrders(lbox.Items, termSpecs)
    End Sub

    ''' <summary>
    ''' Removes items from a ListBox. Supports removing:
    ''' <list type="bullet">
    '''   <item><description>All items</description></item>
    '''   <item><description>Only selected items</description></item>
    ''' </list>
    ''' When removing a main effect, associated interaction terms or polynomial
    ''' terms are also removed automatically.
    ''' </summary>
    ''' <param name="lbox">The ListBox to modify.</param>
    ''' <param name="sWhich">
    ''' Either <c>"all"</c> or <c>"selected"</c>.
    ''' </param>
    Public Sub Remove_Item(lbox As Object, Optional sWhich As String = "all",
                           Optional termSpecs As Dictionary(Of String, TermSpec) = Nothing)

        If lbox Is Nothing OrElse lbox.Items.Count = 0 Then Exit Sub

        If String.Equals(sWhich, "all", StringComparison.OrdinalIgnoreCase) Then
            lbox.Items.Clear()

            If termSpecs IsNot Nothing Then
                termSpecs.Clear()
                RegressionDesignCore.RefreshTermSpecOrders(lbox.Items, termSpecs)
            End If

            Exit Sub
        End If

        If Not String.Equals(sWhich, "selected", StringComparison.OrdinalIgnoreCase) Then
            Exit Sub
        End If

        If lbox.SelectedItems.Count = 0 Then
            MsgBox("Please select variable(s) to remove.", vbExclamation, "Input Error!")
            Exit Sub
        End If

        Dim selectedKeys As New HashSet(Of String)(StringComparer.Ordinal)
        For Each it As Object In lbox.SelectedItems
            selectedKeys.Add(CStr(it))
        Next

        'If a main effect is removed, also remove all dependent derived terms
        Dim removedBaseKeys As New HashSet(Of String)(StringComparer.Ordinal)
        For Each effKey As String In selectedKeys
            If Not IsDerivedEffectKey(effKey) Then
                Dim baseKeys As List(Of String) = ExtractBaseKeysForSubset(effKey, True)
                For Each bk As String In baseKeys
                    If bk <> String.Empty Then removedBaseKeys.Add(bk)
                Next
            End If
        Next

        Dim keysToRemove As New HashSet(Of String)(selectedKeys, StringComparer.Ordinal)

        If removedBaseKeys.Count > 0 Then
            For Each it As Object In lbox.Items
                Dim effKey As String = CStr(it)
                If keysToRemove.Contains(effKey) Then Continue For

                Dim baseKeys As List(Of String) = ExtractBaseKeysForSubset(effKey, True)
                If ContainsAnyBaseKey(baseKeys, removedBaseKeys) Then
                    keysToRemove.Add(effKey)
                End If
            Next
        End If

        For i As Integer = lbox.Items.Count - 1 To 0 Step -1
            Dim effKey As String = CStr(lbox.Items(i))
            If keysToRemove.Contains(effKey) Then
                lbox.Items.RemoveAt(i)
            End If
        Next

        If termSpecs IsNot Nothing Then
            SyncTermSpecsToListBoxItems(lbox, termSpecs)
        End If
    End Sub

    ''' <summary>
    ''' Adds multiple selected items from one ListBox to another, preventing:
    ''' <list type="bullet">
    '''   <item><description>Duplicate entries</description></item>
    '''   <item><description>Conflicts with variables already selected as responses</description></item>
    ''' </list>
    ''' Displays MsgBox warnings when invalid selections occur.
    ''' </summary>
    ''' <param name="lbox_to">Destination ListBox.</param>
    ''' <param name="lbox_from">Source ListBox.</param>
    ''' <param name="lbox_tocheck">Optional ListBoxes to check for conflicts.</param>
    Sub AddItemsToListbox(lbox_to As Object, lbox_from As Object, ParamArray lbox_tocheck() As Object)
        ' The purpose of this subroutine is to copy predictor variables chosen from the lbox_from
        ' into the lbox_to listbox. Mutliple selections are accepted. If lbox_tocheck is provided
        ' then it is checked whether variable was already selected in these listboxes If the
        ' selection is already present, it is not added to the lbox_to.

        'Test for whether predictor variables are present
        If lbox_from.SelectedItems.count >= 1 Then 'If there has been a selection, load into lbox_to
            For i As Integer = 0 To lbox_from.Items.Count - 1
                'Do not add if selection is already present
                Dim bPresent As Boolean = False
                For j As Integer = 0 To lbox_to.Items.Count - 1
                    If lbox_to.Items(j) = lbox_from.Items(i) Then
                        bPresent = True
                        Exit For
                    End If
                Next
                'check whether this Variable has alredy been selected as respons variable
                For jj As Integer = 0 To lbox_tocheck.GetUpperBound(0)
                    For ii As Integer = 0 To lbox_tocheck(jj).Items.Count - 1
                        If lbox_tocheck(jj).Items(ii) = lbox_from.Items(i) And lbox_from.SelectedItems.Contains(lbox_from.Items(i)) Then
                            MsgBox("Variable " & lbox_from.Items(i) & " has already been selected.", vbExclamation, "Input Error!")
                            Exit Sub
                        End If
                    Next
                Next

                'Cannot reselect the response variable
                If lbox_from.SelectedItems.Contains(lbox_from.Items(i)) And Not bPresent Then
                    lbox_to.Items.Add(lbox_from.Items(i))
                End If
            Next
        Else 'No selection has been made
            MsgBox("Please select variable(s).", vbExclamation, "Input Error!")
        End If
    End Sub

    ''' <summary>
    ''' Determines whether two ListBoxes contain identical items, regardless of order.
    ''' </summary>
    ''' <param name="ListBox1">First ListBox.</param>
    ''' <param name="ListBox2">Second ListBox.</param>
    ''' <returns>True if both contain the same items; otherwise False.</returns>
    Function IsEqualListBox(ListBox1 As Object, ListBox2 As Object) As Boolean
        'return true if two listboxes have identical items (items does not need to be in the same order)
        Dim dict As New Dictionary(Of String, Integer)

        'no items, so they match
        If ListBox1.Items.Count = 0 And ListBox2.Items.Count = 0 Then Return True

        '# of items don't match
        If ListBox1.Items.Count <> ListBox2.Items.Count Then Return False

        For i As Integer = 0 To ListBox1.Items.Count - 1
            dict.Add(ListBox1.Items(i), 1)
        Next

        For i As Integer = 0 To ListBox2.Items.Count - 1
            If Not dict.ContainsKey(ListBox2.Items(i)) Then Return False
        Next

        Return True
    End Function


    ''' <summary>
    ''' Determines whether all items in ListBox2 are present in ListBox1.
    ''' Optionally considers only main‑effect variable names by stripping
    ''' polynomial and interaction notation.
    ''' </summary>
    ''' <param name="ListBox1">The superset ListBox.</param>
    ''' <param name="ListBox2">The subset ListBox.</param>
    ''' <param name="bOnlyMain">
    ''' If True, reduces items to main‑effect names before comparison.
    ''' </param>
    ''' <returns>True if ListBox2 is a subset of ListBox1.</returns>
    Function IsSubsetListBox(ListBox1 As Object, ListBox2 As Object, Optional bOnlyMain As Boolean = False) As Boolean
        Dim dict As New Dictionary(Of String, Integer)(StringComparer.Ordinal)

        If ListBox1.Items.Count = 0 And ListBox2.Items.Count = 0 Then Return True

        'Build dict from ListBox1
        For i As Integer = 0 To ListBox1.Items.Count - 1
            Dim keys As List(Of String) = ExtractBaseKeysForSubset(CStr(ListBox1.Items(i)), bOnlyMain)
            For Each k As String In keys
                If Not dict.ContainsKey(k) Then dict.Add(k, 1)
            Next
        Next

        'Check all required keys from ListBox2 exist in dict
        For i As Integer = 0 To ListBox2.Items.Count - 1
            Dim keys As List(Of String) = ExtractBaseKeysForSubset(CStr(ListBox2.Items(i)), bOnlyMain)
            For Each k As String In keys
                If Not dict.ContainsKey(k) Then Return False
            Next
        Next

        Return True
    End Function

    ''' <summary>
    ''' Extracts one or more "base keys" from an effect expression.  
    ''' The function supports several expression formats, including:
    ''' polynomial expressions using the caret operator, quoted interaction
    ''' formats, and simple main-effect strings.
    ''' </summary>
    ''' <param name="effectText">
    ''' The raw effect expression to analyze. May contain polynomial notation
    ''' (e.g., "X^2"), quoted interaction notation (e.g., "A":"B"), or a simple
    ''' main-effect term.
    ''' </param>
    ''' <param name="bOnlyMain">
    ''' If True, only the main/base portion of the effect is returned.  
    ''' If False, the entire trimmed effectText is returned as a single key.
    ''' </param>
    ''' <returns>
    ''' A list of extracted base keys.  
    ''' 
    ''' <para>
    ''' Behavior by format:
    ''' </para>
    ''' <list type="bullet">
    '''   <item>
    '''     <description>
    '''     If <paramref name="bOnlyMain"/> is False, the function returns a list
    '''     containing the full trimmed effectText.
    '''     </description>
    '''   </item>
    '''   <item>
    '''     <description>
    '''     Polynomial format ("base"^k or base^k):  
    '''     If the exponent is valid, returns the base portion with outer quotes removed.
    '''     </description>
    '''   </item>
    '''   <item>
    '''     <description>
    '''     Quoted interaction format ("A":"B"):  
    '''     Splits on ":" and returns each quoted component as a separate key.
    '''     </description>
    '''   </item>
    '''   <item>
    '''     <description>
    '''     If no special format is detected, returns the trimmed effectText as-is.
    '''     </description>
    '''   </item>
    ''' </list>
    ''' </returns>
    Private Function ExtractBaseKeysForSubset(effectText As String, bOnlyMain As Boolean) As List(Of String)
        Dim out As New List(Of String)
        Dim s As String = If(effectText, String.Empty).Trim()

        If Not bOnlyMain Then
            out.Add(s)
            Return out
        End If

        '1) Polynomial: "<base>"^k or <base>^k
        Dim pCaret As Integer = InStrRev(s, "^")
        If pCaret > 0 AndAlso pCaret < Len(s) Then
            Dim expStr As String = Mid$(s, pCaret + 1)
            Dim expVal As Integer
            If Integer.TryParse(expStr, expVal) Then
                Dim baseKey As String = Left$(s, pCaret - 1).Trim()
                out.Add(StripOuterDoubleQuotes(baseKey))
                Return out
            End If
        End If

        '2) New interaction format:  "A":"B"  (also supports multiway in future)
        If InStr(s, ":") > 0 AndAlso InStr(s, ChrW(34)) > 0 Then
            Dim parts() As String = Split(s, ":")
            For Each p As String In parts
                Dim k As String = StripOuterDoubleQuotes(p.Trim())
                If k <> String.Empty Then out.Add(k)
            Next
            If out.Count >= 2 Then Return out
            out.Clear()
        End If

        '3) Categorical predictor
        If s.StartsWith(RegressionDesignCore.CATEGORICAL_EFFECT_PREFIX, StringComparison.Ordinal) Then
            out.Add(s.Substring(RegressionDesignCore.CATEGORICAL_EFFECT_PREFIX.Length).Trim())
            Return out
        End If

        '3) Main effect as-is
        out.Add(s)
        Return out
    End Function

    ''' <summary>
    ''' Removes a single pair of outer double quotes from the supplied string,
    ''' if such quotes are present. Inner content is preserved unchanged.
    ''' </summary>
    ''' <param name="s">
    ''' The input string to process. May be quoted, unquoted, or null.
    ''' </param>
    ''' <returns>
    ''' The trimmed string with one pair of leading and trailing double quotes
    ''' removed if present; otherwise, the trimmed original value.
    ''' </returns>
    ''' <remarks>
    ''' Only removes quotes when both the first and last characters are
    ''' double quotes. Does not attempt to unescape or modify internal content.
    ''' </remarks>
    Private Function StripOuterDoubleQuotes(s As String) As String
        Return RegressionDesignCore.StripOuterDoubleQuotesPublic(s)
    End Function

    Public Function GetExcelNumberCulture() As Globalization.CultureInfo
        Dim ci As Globalization.CultureInfo =
        DirectCast(Globalization.CultureInfo.CurrentCulture.Clone(), Globalization.CultureInfo)

        Try
            If AppGlobals.app IsNot Nothing Then
                Dim decSep As String = CStr(AppGlobals.app.DecimalSeparator)
                Dim grpSep As String = CStr(AppGlobals.app.ThousandsSeparator)

                If decSep <> String.Empty Then
                    ci.NumberFormat.NumberDecimalSeparator = decSep
                    ci.NumberFormat.CurrencyDecimalSeparator = decSep
                End If

                If grpSep <> String.Empty Then
                    ci.NumberFormat.NumberGroupSeparator = grpSep
                    ci.NumberFormat.CurrencyGroupSeparator = grpSep
                End If
            End If
        Catch
            'fall back to current culture
        End Try

        Return ci
    End Function

    Public Function TryParseUiDouble(text As String, ByRef value As Double) As Boolean
        Dim s As String = If(text, String.Empty).Trim()

        If s = String.Empty Then
            value = 0
            Return False
        End If

        Dim styles As Globalization.NumberStyles =
        Globalization.NumberStyles.Float Or Globalization.NumberStyles.AllowThousands

        Return Double.TryParse(s, styles, GetExcelNumberCulture(), value) OrElse
           Double.TryParse(s, styles, Globalization.CultureInfo.CurrentCulture, value) OrElse
           Double.TryParse(s, styles, Globalization.CultureInfo.InvariantCulture, value)
    End Function

    Public Function ParseUiDouble(text As String,
                              Optional fieldName As String = "numeric value") As Double
        Dim value As Double
        If Not TryParseUiDouble(text, value) Then
            Throw New FormatException($"Cannot parse {fieldName}: ""{text}"".")
        End If
        Return value
    End Function

    Public Function TryParseUiInteger(text As String, ByRef value As Integer) As Boolean
        Dim d As Double
        If Not TryParseUiDouble(text, d) Then
            value = 0
            Return False
        End If

        If d < Integer.MinValue OrElse d > Integer.MaxValue OrElse d <> Math.Truncate(d) Then
            value = 0
            Return False
        End If

        value = CInt(d)
        Return True
    End Function

    Public Function ParseUiInteger(text As String,
                               Optional fieldName As String = "integer value") As Integer
        Dim value As Integer
        If Not TryParseUiInteger(text, value) Then
            Throw New FormatException($"Cannot parse {fieldName}: ""{text}"".")
        End If
        Return value
    End Function

    Public Function FormatUiDouble(value As Double) As String
        Return value.ToString("G", GetExcelNumberCulture())
    End Function

    ''' <summary>
    ''' Parses a space‑separated string of numbers into a Double array.
    ''' Used for reading initial parameter values in regression dialogs.
    ''' </summary>
    ''' <param name="tb">The input string.</param>
    ''' <param name="bErr">Output flag indicating whether any value failed to parse.</param>
    ''' <returns>An array of parsed numeric values.</returns>
    Function GetNumbersFromStrList(tb As String, ByRef bErr As Boolean) As Double()
        'tb - space separated list of numbers. Convert them to array of numbers. Used for initial parameter values in regression estimation.
        Dim tmp As String, out() As Double
        bErr = False

        If tb = String.Empty Then
            ReDim out(0)
            Return out
        End If

        Do
            tmp = tb
            tb = Replace(tb, "  ", " ") 'remove multiple white spaces
        Loop Until tmp = tb

        Dim list() As String = Split(Trim$(tmp), " ")
        ReDim out(list.Length - 1)
        For i = 0 To list.Length - 1
            If Not TryParseUiDouble(list(i), out(i)) Then
                bErr = True
            End If
        Next
        Return out
    End Function

    ''' <summary>
    ''' Sets the background color and tooltip text for a TextBox‑like control.
    ''' </summary>
    ''' <param name="tb">The TextBox or ComboBox control.</param>
    ''' <param name="bkColor">Background color to apply.</param>
    ''' <param name="tiptext">Tooltip text to assign.</param>
    Sub setTextBoxProperties(ByRef tb As Object, bkColor As Object, tiptext As String)
        Dim window As Object
        Try
            tb.BackColor = bkColor
            window = tb.FindForm()
            window.ToolTip1.SetToolTip(tb, tiptext)
        Catch
        End Try
    End Sub

    ''' <summary>
    ''' Validates a RefEdit control’s address string. Ensures that:
    ''' <list type="bullet">
    '''   <item><description>The address is a valid Excel range</description></item>
    '''   <item><description>The referenced worksheet exists</description></item>
    '''   <item><description>If <c>bOneColumn</c> is True, the range contains exactly one column</description></item>
    ''' </list>
    ''' Displays MsgBox warnings when invalid.
    ''' </summary>
    ''' <param name="refEditValue">The RefEdit text.</param>
    ''' <param name="bOneColumn">If True, requires a single‑column range.</param>
    ''' <returns>True if invalid; False if valid.</returns>
    Function CheckRefEdit(refEditValue As String, Optional bOneColumn As Boolean = False) As Boolean
        'check validity of refedit.value

        Dim bIsRange As Boolean = False, rRange As Range
        Dim ws As Worksheet = Nothing

        If refEditValue = String.Empty Then
            MsgBox("The Reference Range is NULL. Please select some data.", vbExclamation, AppGlobals.gsAPP_TITLE)
            Return True
        End If

        Try
            ws = WorksheetFromRefAdress(refEditValue)
            rRange = ws.Range(refEditValue) 'Use IsObject to find out if the string is a valid address.
            bIsRange = True
        Catch
            bIsRange = False
        End Try

        If Not bIsRange Then
            MsgBox("The Range is not valid!", vbExclamation, AppGlobals.gsAPP_TITLE)
            Return True
        End If
        If bOneColumn And bIsRange Then
            rRange = ws.Range(refEditValue)
            If rRange.Columns.Count <> 1 Then
                MsgBox("The Range have to have one column!", vbExclamation, AppGlobals.gsAPP_TITLE)
                Return True
            End If
        End If

        Return False
    End Function

    ''' <summary>
    ''' Clears a RefEdit control and returns focus to it.
    ''' </summary>
    ''' <param name="objRefEdit">The RefEdit control.</param>
    Sub RefEditReset(objRefEdit As Global.BESHStatNG.Excel2007RefEdit)
        Try
            objRefEdit.Address = String.Empty
            objRefEdit.Select()
        Catch
        End Try
    End Sub

    ''' <summary>
    ''' Checks whether a rectangular worksheet region contains any non‑empty cells.
    ''' Used to prevent overwriting user data when writing results.
    ''' </summary>
    ''' <param name="lStartRow">Starting row.</param>
    ''' <param name="lStartColumn">Starting column.</param>
    ''' <param name="NumberOfRows">Height of the region.</param>
    ''' <param name="NumberOfColumns">Width of the region.</param>
    ''' <param name="ws">Worksheet to inspect.</param>
    ''' <returns>True if any cell is non‑empty; otherwise False.</returns>
    Function AreaCheck(lStartRow As Integer, lStartColumn As Integer, NumberOfRows As Integer, NumberOfColumns As Integer, ws As Worksheet) As Boolean
        For i As Integer = lStartRow To lStartRow + NumberOfRows - 1
            For j As Integer = lStartColumn To lStartColumn + NumberOfColumns - 1
                If ws.Cells(i, j).value IsNot Nothing Then
                    If TypeOf ws.Cells(i, j).value IsNot ExcelEmpty Or TypeOf ws.Cells(i, j).value IsNot ExcelMissing Then
                        Return True
                    End If
                End If
            Next
        Next

        Return False
    End Function

    ''' <summary>
    ''' Validates that a TextBox or ComboBox contains a numeric value.
    ''' Updates background color and returns parsed numeric output.
    ''' </summary>
    ''' <param name="txtData">The UI control containing text.</param>
    ''' <param name="dResult">Parsed numeric value (output).</param>
    ''' <param name="sError">Error message if parsing fails.</param>
    ''' <returns>True if numeric or empty; False if invalid.</returns>
    Public Function CheckNumeric(ByRef txtData As Object,
                             Optional ByRef dResult As Double = Double.NaN,
                             Optional ByRef sError As String = "") As Boolean

        'check for valid entry
        If txtData.text = String.Empty Then 'allow empty
            dResult = 0
            sError = String.Empty
            CheckNumeric = True

            'and give the text box its usual background
            txtData.BackColor = System.Drawing.Color.White

        ElseIf TryParseUiDouble(CStr(txtData.text), dResult) Then
            sError = String.Empty
            CheckNumeric = True

            'and give the text box its usual background
            txtData.BackColor = System.Drawing.Color.White

        Else 'not numeric
            dResult = 0
            sError = "Entry is not a number."
            CheckNumeric = False

            'give the text box a red background
            txtData.BackColor = System.Drawing.Color.Red
            'add the error message to the tooltip
            'txtData.ControlTipText = txtData.ControlTipText + sErr + sError

        End If
    End Function

End Module
