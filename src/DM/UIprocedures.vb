Option Explicit On
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

    Public Class TermSpec
        Public Property Kind As String 'MainEffect | Polynomial | Interaction
        Public Property BaseVarKeys As List(Of String)
        Public Property Degree As Integer
        Public Property DisplayNameForCoef As String '(e.g. "Age^2", "Age:BMI")
        Public Property Order As Integer 'position in the result output. It should be identical to the input combobox item position

    End Class

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
        app = BESHstatGlobals.app

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
    Sub Remove_Item(lbox As Object, Optional sWhich As String = "all")

        'Test if there is a variable in the listbox
        If lbox.Items.Count() > 0 Then
            If sWhich = "all" Then
                lbox.Items.Clear()
            ElseIf sWhich = "selected" Then 'Check that a variable(s) have been selected

                If lbox.SelectedItems.count = 0 Then
                    MsgBox("Please select variable(s) to remove.", vbExclamation, "Input Error!")
                Else
                    For i As Integer = lbox.Items.Count - 1 To 0 Step -1
                        If lbox.SelectedItems.Contains(lbox.Items(i)) Then
                            If Not (InStr(lbox.Items(i), "''log(time)''") > 0 Or
                                    InStr(lbox.Items(i), "''time''") > 0 Or
                                    InStr(lbox.Items(i), "''heaviside(") > 0) Or
                               Not (Right$(lbox.Items(i), 2) = "^2" Or InStr(lbox.Items(i), "''x''")) Then
                                'not a main effect = (ie not (COX time interactions or 2nd order polynomial))

                                'it is probably a main effect, so we neet to remove polynomial/2-way interaction as well
                                For j As Integer = lbox.Items.Count - 1 To 0 Step -1

                                    Dim maineffect1 As String = String.Empty
                                    Dim maineffect2 As String = String.Empty

                                    If InStr(lbox.Items(j), "''x''") Then '2-way intractions
                                        Dim PartString() As String = Split(lbox.Items(j), "''x''")
                                        maineffect1 = Mid$(PartString(0), 3)
                                        maineffect2 = Left$(PartString(1), Len(PartString(1)) - 2)
                                        If maineffect1 = lbox.Items(i) Or maineffect2 = lbox.Items(i) Then
                                            'it is a interaction of the main effect we are removing so remove it as well
                                            lbox.Items.RemoveAt(j)
                                        End If
                                    ElseIf Right$(lbox.Items(j), 2) = "^2" Then ' 2nd order polynomial
                                        maineffect1 = Left$(lbox.Items(j), Len(lbox.Items(j)) - 2)
                                        If maineffect1 = lbox.Items(i) Then
                                            'it is a 2nd order polynomial of the main effect we are removing so remove it as well
                                            lbox.Items.RemoveAt(j)
                                        End If
                                    ElseIf InStr(lbox.Items(j), "''log(time)''") Then ' COX time intraction
                                        If Mid$(lbox.Items(j), 3, Len(lbox.Items(j)) - 17) = lbox.Items(i) Then
                                            'it is a time interaction of the main effect we are removing so remove it as well
                                            lbox.Items.RemoveAt(j)
                                        End If
                                    ElseIf InStr(lbox.Items(j), "''time''") Then ' COX time intraction
                                        If Mid$(lbox.Items(j), 3, Len(lbox.Items(j)) - 12) = lbox.Items(i) Then
                                            lbox.Items.RemoveAt(j)
                                        End If
                                    ElseIf InStr(lbox.Items(j), "''heaviside(") Then ' COX time intraction
                                        If Mid$(lbox.Items(j), 3, InStr(lbox.Items(j), "''''heaviside(") - 3) = lbox.Items(i) Then
                                            lbox.Items.RemoveAt(j)
                                        End If
                                    End If
                                Next
                            End If

                            'remove selected item
                            lbox.Items.RemoveAt(i)
                        End If
                    Next i
                End If
            End If
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
        Dim t As String = If(s, String.Empty).Trim()
        If Len(t) >= 2 AndAlso Left$(t, 1) = ChrW(34) AndAlso Right$(t, 1) = ChrW(34) Then
            t = Mid$(t, 2, Len(t) - 2)
        End If
        Return t.Trim()
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
            Try
                out(i) = CDbl(list(i))
            Catch
                bErr = True
            End Try
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
            MsgBox("The Reference Range is NULL. Please select some data.", vbExclamation, BESHstatGlobals.gsAPP_TITLE)
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
            MsgBox("The Range is not valid!", vbExclamation, BESHstatGlobals.gsAPP_TITLE)
            Return True
        End If
        If bOneColumn And bIsRange Then
            rRange = ws.Range(refEditValue)
            If rRange.Columns.Count <> 1 Then
                MsgBox("The Range have to have one column!", vbExclamation, BESHstatGlobals.gsAPP_TITLE)
                Return True
            End If
        End If

        Return False
    End Function

    ''' <summary>
    ''' Clears a RefEdit control and returns focus to it.
    ''' </summary>
    ''' <param name="objRefEdit">The RefEdit control.</param>
    Sub RefEditReset(objRefEdit As Excel2007RefEdit)
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

        ElseIf IsNumeric(txtData.text) Then 'numeric, so set the return values
            dResult = CDbl(txtData.text)
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

    ''' <summary>
    ''' Extract a stable coefficient base name from a UI variable key (e.g. "Age | VarA" -> "Age").
    ''' If no header is present, returns the whole string (e.g. "VarB").
    ''' </summary>
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

    ''' <summary>
    ''' Build expanded LM data matrix: [Y | expanded X], where expanded X includes
    ''' polynomial and interaction columns based on TermSpecs/effectItems.
    ''' varNames returned includes Y at index 0 and expanded predictors thereafter.
    ''' </summary>
    Public Sub BuildExpandedLmDataMatrix(raw As glmData, yKey As String, effectItems As IEnumerable,
                                         termSpecs As Dictionary(Of String, TermSpec),
                                         ByRef outData(,) As Double,
                                         ByRef outVarNames() As String)

        If raw Is Nothing Then BESHstatGlobals.BSerr.LogAndThrow(New ArgumentNullException(NameOf(raw)))
        If effectItems Is Nothing Then BESHstatGlobals.BSerr.LogAndThrow(New ArgumentNullException(NameOf(effectItems)))

        'Materialize effects in display/order sequence (ListBox order)
        Dim effects As New List(Of String)
        For Each obj As Object In effectItems
            effects.Add(CStr(obj))
        Next

        Dim nRows As Integer = raw.nRows
        Dim pExpanded As Integer = effects.Count 'expanded predictors count (1 column per effect)

        'Raw imported numeric matrix includes Y at col 0 and raw X in subsequent columns
        Dim rawMat(,) As Double = raw.DataDbl

        'Map raw variable keys -> column index in rawMat
        'rawMat col 0 = Y; col 1.. = raw predictors in the order GetRequiredRawVarKeys produced
        Dim rawXKeys As List(Of String) = GetRequiredRawVarKeys(effectItems, termSpecs)
        Dim rawIndex As New Dictionary(Of String, Integer)(StringComparer.Ordinal)

        For j As Integer = 0 To rawXKeys.Count - 1
            rawIndex(rawXKeys(j)) = j + 1
        Next

        'Allocate output: columns = 1 (Y) + pExpanded predictors
        ReDim outData(nRows - 1, pExpanded)
        ReDim outVarNames(pExpanded) '0..pExpanded

        'Y name
        outVarNames(0) = GetCoefBaseName(yKey)

        'Copy Y
        For i As Integer = 0 To nRows - 1
            outData(i, 0) = rawMat(i, 0)
        Next

        'Build expanded predictors in effects order
        For e As Integer = 0 To effects.Count - 1
            Dim effKey As String = effects(e)

            Dim kind As String = "MainEffect"
            Dim baseKeys As List(Of String) = Nothing
            Dim degree As Integer = 1
            Dim coefName As String = Nothing

            If termSpecs IsNot Nothing AndAlso termSpecs.ContainsKey(effKey) AndAlso termSpecs(effKey) IsNot Nothing Then
                Dim spec As TermSpec = termSpecs(effKey)
                kind = If(spec.Kind, "MainEffect")
                baseKeys = spec.BaseVarKeys
                degree = spec.Degree
                coefName = spec.DisplayNameForCoef
            End If

            'Fallbacks when no spec exists
            If baseKeys Is Nothing OrElse baseKeys.Count = 0 Then
                baseKeys = New List(Of String) From {effKey}
            End If
            If String.IsNullOrEmpty(coefName) Then
                'If user didn’t store DisplayNameForCoef, infer from base key
                If String.Equals(kind, "Polynomial", StringComparison.OrdinalIgnoreCase) Then
                    coefName = GetCoefBaseName(baseKeys(0)) & "^" & CStr(degree)
                ElseIf String.Equals(kind, "Interaction", StringComparison.OrdinalIgnoreCase) Then
                    Dim tmp As New List(Of String)
                    For Each bk As String In baseKeys
                        tmp.Add(GetCoefBaseName(bk))
                    Next
                    coefName = String.Join(":", tmp)
                Else
                    coefName = GetCoefBaseName(baseKeys(0))
                End If
            End If

            outVarNames(e + 1) = coefName

            'Compute column values
            If String.Equals(kind, "Polynomial", StringComparison.OrdinalIgnoreCase) Then
                If degree < 2 Then BESHstatGlobals.BSerr.LogAndThrow(New ArgumentException($"Polynomial degree must be >=2 for term '{effKey}'."))

                Dim bk As String = baseKeys(0)
                If Not rawIndex.ContainsKey(bk) Then
                    BESHstatGlobals.BSerr.LogAndThrow(New ArgumentException($"Missing raw predictor '{bk}' required by polynomial term '{effKey}'."))
                End If
                Dim col As Integer = rawIndex(bk)

                For i As Integer = 0 To nRows - 1
                    outData(i, e + 1) = PowInt(rawMat(i, col), degree)
                Next

            ElseIf String.Equals(kind, "Interaction", StringComparison.OrdinalIgnoreCase) Then
                If baseKeys.Count < 2 Then
                    BESHstatGlobals.BSerr.LogAndThrow(New ArgumentException($"Interaction term '{effKey}' must have at least 2 base variables."))
                End If

                'validate all base keys exist
                Dim cols As New List(Of Integer)
                For Each bk As String In baseKeys
                    If Not rawIndex.ContainsKey(bk) Then
                        BESHstatGlobals.BSerr.LogAndThrow(New ArgumentException($"Missing raw predictor '{bk}' required by interaction term '{effKey}'."))
                    End If
                    cols.Add(rawIndex(bk))
                Next

                For i As Integer = 0 To nRows - 1
                    Dim prod As Double = 1.0
                    For Each c As Integer In cols
                        prod *= rawMat(i, c)
                    Next
                    outData(i, e + 1) = prod
                Next

            Else
                'Main effect (or unknown kind treated as main effect)
                Dim bk As String = baseKeys(0)
                If Not rawIndex.ContainsKey(bk) Then
                    BESHstatGlobals.BSerr.LogAndThrow(New ArgumentException($"Missing raw predictor '{bk}' required by term '{effKey}'."))
                End If
                Dim col As Integer = rawIndex(bk)

                For i As Integer = 0 To nRows - 1
                    outData(i, e + 1) = rawMat(i, col)
                Next
            End If
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
                                               includeIntercept As Boolean) As Dictionary(Of String, Integer())

        Dim groups As New Dictionary(Of String, List(Of Integer))(StringComparer.Ordinal)
        Dim colOffset As Integer = If(includeIntercept, 1, 0)

        'Optional: include intercept group
        If includeIntercept Then
            groups("Intercept") = New List(Of Integer) From {0}
        End If

        'For base-name disambiguation across duplicate headers
        Dim baseNameToKeys As New Dictionary(Of String, List(Of String))(StringComparer.Ordinal)
        Dim baseKeyToGroupName As New Dictionary(Of String, String)(StringComparer.Ordinal)

        'For interaction-name collision handling
        Dim usedGroupNames As New HashSet(Of String)(StringComparer.Ordinal)
        If includeIntercept Then usedGroupNames.Add("Intercept")

        Dim effects As New List(Of String)
        For Each obj As Object In effectItems
            effects.Add(CStr(obj))
        Next

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
                    coefName = MakeInteractionCoefName(baseKeys)
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
                'MainEffect or Polynomial -> group by base variable (so poly joins main)
                Dim baseKey As String = baseKeys(0)
                Dim baseName As String = GetCoefBaseName(baseKey)

                If Not baseNameToKeys.ContainsKey(baseName) Then
                    baseNameToKeys(baseName) = New List(Of String)
                End If

                Dim list As List(Of String) = baseNameToKeys(baseName)

                If Not list.Contains(baseKey) Then
                    list.Add(baseKey)

                    'If this baseName now maps to multiple different baseKeys, disambiguate ALL
                    If list.Count >= 2 Then
                        For Each bk As String In list
                            Dim desired As String = baseName & " (" & ExtractVarSuffix(bk) & ")"

                            If baseKeyToGroupName.ContainsKey(bk) Then
                                Dim oldName As String = baseKeyToGroupName(bk)
                                'Rename existing group if it used the ambiguous baseName
                                If oldName = baseName AndAlso groups.ContainsKey(oldName) Then
                                    groups(desired) = groups(oldName)
                                    groups.Remove(oldName)
                                End If
                                baseKeyToGroupName(bk) = desired
                            Else
                                baseKeyToGroupName(bk) = desired
                            End If
                        Next
                    End If
                End If

                'Pick groupName for this baseKey
                If baseKeyToGroupName.ContainsKey(baseKey) Then
                    groupName = baseKeyToGroupName(baseKey)
                Else
                    groupName = baseName
                    baseKeyToGroupName(baseKey) = groupName
                End If

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

End Module
