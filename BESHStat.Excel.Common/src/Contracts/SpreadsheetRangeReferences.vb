Option Explicit On
Option Strict On

Imports System
Imports System.Collections.Generic
Imports System.Globalization
Imports System.Text.RegularExpressions

Namespace ExcelCommon

    ''' <summary>
    ''' Identifies how a host-supplied display address is interpreted. ExcelA1 is the
    ''' legacy/default contract; HostNative addresses are never parsed as Excel A1.
    ''' </summary>
    Public Enum SpreadsheetAddressDialect
        Unknown = 0
        ExcelA1 = 1
        HostNative = 2
    End Enum

    ''' <summary>A bounded, rectangular Excel A1 address. No host objects or I/O.</summary>
    Public NotInheritable Class SpreadsheetRangeAddress
        Public Const ExcelMaxRows As Integer = 1048576
        Public Const ExcelMaxColumns As Integer = 16384
        Private Shared ReadOnly CellPattern As New Regex("^\$?([A-Za-z]{1,3})\$?([1-9][0-9]{0,6})$", RegexOptions.CultureInvariant)
        Private Sub New()
        End Sub

        Public Property WorkbookName As String = String.Empty
        Public Property WorksheetName As String = String.Empty
        Public Property Address As String = String.Empty
        Public Property StartRow As Integer
        Public Property StartColumn As Integer
        Public Property EndRow As Integer
        Public Property EndColumn As Integer

        Public ReadOnly Property RowCount As Integer
            Get
                Return EndRow - StartRow + 1
            End Get
        End Property
        Public ReadOnly Property ColumnCount As Integer
            Get
                Return EndColumn - StartColumn + 1
            End Get
        End Property
        Public ReadOnly Property QualifiedAddress As String
            Get
                If WorksheetName.Length = 0 Then Return Address
                Dim qualifier As String = If(WorkbookName.Length = 0, WorksheetName, "[" & WorkbookName & "]" & WorksheetName)
                Return "'" & qualifier.Replace("'", "''") & "'!" & Address
            End Get
        End Property

        Public Shared Function Parse(text As String, Optional defaultWorksheetName As String = Nothing) As SpreadsheetRangeAddress
            If String.IsNullOrWhiteSpace(text) Then Throw New ArgumentException("An A1 range address is required.", NameOf(text))
            Dim expression As String = text.Trim()
            Dim bang As Integer = -1
            Dim inQuotes As Boolean = False
            Dim i As Integer = 0
            While i < expression.Length
                Dim ch As Char = expression(i)
                If ch = "'"c Then
                    If inQuotes AndAlso i + 1 < expression.Length AndAlso expression(i + 1) = "'"c Then
                        i += 1
                    Else
                        inQuotes = Not inQuotes
                    End If
                ElseIf ch = "!"c AndAlso Not inQuotes Then
                    If bang >= 0 Then Throw New ArgumentException("Only one worksheet qualifier is supported.", NameOf(text))
                    bang = i
                End If
                i += 1
            End While
            If inQuotes Then Throw New ArgumentException("Unclosed quoted worksheet name.", NameOf(text))

            Dim sheet As String = If(defaultWorksheetName, String.Empty).Trim()
            Dim book As String = String.Empty
            If bang >= 0 Then
                Dim qualifier As String = expression.Substring(0, bang)
                If qualifier.StartsWith("'", StringComparison.Ordinal) AndAlso qualifier.EndsWith("'", StringComparison.Ordinal) AndAlso qualifier.Length >= 2 Then
                    qualifier = qualifier.Substring(1, qualifier.Length - 2).Replace("''", "'")
                ElseIf qualifier.IndexOf("'"c) >= 0 Then
                    Throw New ArgumentException("Invalid worksheet quoting.", NameOf(text))
                End If
                If qualifier.StartsWith("[", StringComparison.Ordinal) Then
                    Dim closing As Integer = qualifier.IndexOf("]"c)
                    If closing < 2 Then Throw New ArgumentException("Invalid workbook qualifier.", NameOf(text))
                    book = qualifier.Substring(1, closing - 1)
                    qualifier = qualifier.Substring(closing + 1)
                End If
                If String.IsNullOrWhiteSpace(qualifier) Then Throw New ArgumentException("Worksheet name is missing.", NameOf(text))
                If sheet.Length > 0 AndAlso Not String.Equals(sheet, qualifier, StringComparison.OrdinalIgnoreCase) Then
                    Throw New ArgumentException("Worksheet name conflicts with the qualified address.", NameOf(text))
                End If
                sheet = qualifier
                expression = expression.Substring(bang + 1)
            End If
            If expression.IndexOfAny(New Char() {","c, ";"c, "!"c}) >= 0 Then
                Throw New ArgumentException("Disjoint or multi-sheet A1 addresses are not supported; use SpreadsheetRangeSelection.", NameOf(text))
            End If
            Dim pieces As String() = expression.Split(":"c)
            If pieces.Length < 1 OrElse pieces.Length > 2 Then Throw New ArgumentException("A single rectangular A1 address is required.", NameOf(text))
            Dim row1 As Integer, col1 As Integer, row2 As Integer, col2 As Integer
            ParseCell(pieces(0), row1, col1)
            If pieces.Length = 2 Then
                ParseCell(pieces(1), row2, col2)
            Else
                row2 = row1 : col2 = col1
            End If
            If row2 < row1 OrElse col2 < col1 Then Throw New ArgumentException("The A1 range end precedes its start.", NameOf(text))
            Return New SpreadsheetRangeAddress With {
                .WorkbookName = book, .WorksheetName = sheet,
                .Address = ToColumn(col1) & row1.ToString(CultureInfo.InvariantCulture) & ":" & ToColumn(col2) & row2.ToString(CultureInfo.InvariantCulture),
                .StartRow = row1, .StartColumn = col1, .EndRow = row2, .EndColumn = col2
            }
        End Function

        Private Shared Sub ParseCell(text As String, ByRef row As Integer, ByRef column As Integer)
            Dim match As Match = CellPattern.Match(text)
            If Not match.Success Then Throw New ArgumentException("Invalid A1 cell address: " & text)
            column = 0
            For Each c As Char In match.Groups(1).Value.ToUpperInvariant()
                column = column * 26 + (Convert.ToInt32(c) - 64)
            Next
            If column > ExcelMaxColumns OrElse Not Integer.TryParse(match.Groups(2).Value, NumberStyles.None, CultureInfo.InvariantCulture, row) OrElse row > ExcelMaxRows Then
                Throw New ArgumentOutOfRangeException(NameOf(text), "The address exceeds Excel worksheet boundaries.")
            End If
        End Sub

        Private Shared Function ToColumn(index As Integer) As String
            Dim chars As String = String.Empty
            While index > 0
                index -= 1
                chars = Convert.ToChar(65 + (index Mod 26)).ToString() & chars
                index \= 26
            End While
            Return chars
        End Function
    End Class

    ''' <summary>Selection intent and source identity. IDs are host-supplied; blank means unknown.</summary>
    Public NotInheritable Class SpreadsheetRangeReference
        Public Sub New()
            WorkbookId = String.Empty
            WorkbookName = String.Empty
            WorksheetId = String.Empty
            WorksheetName = String.Empty
            Address = String.Empty
        End Sub

        Public Property WorkbookId As String
        Public Property WorkbookName As String
        Public Property WorksheetId As String
        Public Property WorksheetName As String
        ''' <summary>Unqualified rectangular A1 address. Use QualifiedAddress for display.</summary>
        Public Property Address As String
        ''' <summary>Legacy references use Excel A1; other hosts must explicitly declare HostNative.</summary>
        Public Property AddressDialect As SpreadsheetAddressDialect = SpreadsheetAddressDialect.ExcelA1
        Public Property StartRow As Integer
        Public Property StartColumn As Integer
        Public Property RowCount As Integer
        Public Property ColumnCount As Integer
        Public Property SelectionId As String
        Public Property SelectionRevision As String

        Public ReadOnly Property QualifiedAddress As String
            Get
                If AddressDialect = SpreadsheetAddressDialect.HostNative Then Return If(Address, String.Empty)
                If AddressDialect <> SpreadsheetAddressDialect.ExcelA1 Then Return String.Empty
                If String.IsNullOrWhiteSpace(Address) Then Return String.Empty
                Dim parsed As SpreadsheetRangeAddress = SpreadsheetRangeAddress.Parse(Address, WorksheetName)
                If String.IsNullOrWhiteSpace(WorkbookName) Then Return parsed.QualifiedAddress
                Return "'[" & WorkbookName.Replace("'", "''") & "]" & WorksheetName.Replace("'", "''") & "'!" & parsed.Address
            End Get
        End Property

        ''' <summary>
        ''' Constructs a coordinate-based host-native reference. Coordinates are one-based;
        ''' displayAddress is opaque and must not be evaluated as a formula or parsed as A1.
        ''' The acquiring host must validate its own address dialect and grid bounds.
        ''' </summary>
        Public Shared Function FromCoordinates(worksheetName As String, startRow As Integer, startColumn As Integer,
                                               rowCount As Integer, columnCount As Integer,
                                               Optional displayAddress As String = Nothing) As SpreadsheetRangeReference
            If String.IsNullOrWhiteSpace(worksheetName) Then Throw New ArgumentException("Worksheet name is required.", NameOf(worksheetName))
            If startRow < 1 OrElse startColumn < 1 OrElse rowCount < 1 OrElse columnCount < 1 OrElse
               CLng(startRow) + CLng(rowCount) - 1 > Integer.MaxValue OrElse
               CLng(startColumn) + CLng(columnCount) - 1 > Integer.MaxValue Then
                Throw New ArgumentOutOfRangeException(NameOf(startRow), "Source coordinates must describe a positive, representable rectangle.")
            End If
            Return New SpreadsheetRangeReference With {
                .WorksheetName = worksheetName, .AddressDialect = SpreadsheetAddressDialect.HostNative,
                .Address = If(displayAddress, String.Empty), .StartRow = startRow, .StartColumn = startColumn,
                .RowCount = rowCount, .ColumnCount = columnCount
            }
        End Function

        ''' <summary>
        ''' Interprets separate task-pane worksheet/address fields. The worksheet field normally
        ''' contains a sheet name; a full qualified A1 reference pasted there is also accepted.
        ''' Conflicting duplicate addresses fail before any host lookup.
        ''' </summary>
        Public Shared Function FromUserInput(worksheetNameOrAddress As String,
                                             address As String) As SpreadsheetRangeReference
            Dim sheetInput As String = If(worksheetNameOrAddress, String.Empty).Trim()
            Dim rangeInput As String = If(address, String.Empty).Trim()

            Dim bang As Integer = sheetInput.LastIndexOf("!"c)
            Dim hasQualifiedAddress As Boolean = bang >= 0 AndAlso bang < sheetInput.Length - 1 AndAlso
                Regex.IsMatch(sheetInput.Substring(bang + 1), "^\$?[A-Za-z]{1,3}\$?[1-9][0-9]{0,6}(?::\$?[A-Za-z]{1,3}\$?[1-9][0-9]{0,6})?$", RegexOptions.CultureInvariant)
            If hasQualifiedAddress Then
                Dim qualified As SpreadsheetRangeReference = FromAddress(sheetInput)
                If rangeInput.Length > 0 Then
                    Dim repeated As SpreadsheetRangeReference = FromAddress(rangeInput, qualified.WorksheetName, qualified.WorkbookName)
                    If Not String.Equals(qualified.Address, repeated.Address, StringComparison.OrdinalIgnoreCase) Then
                        Throw New ArgumentException("The range in the worksheet field conflicts with the A1 address field.", NameOf(address))
                    End If
                End If
                Return qualified
            End If

            If rangeInput.Length = 0 Then
                Throw New ArgumentException("An A1 address is required when a worksheet is specified.", NameOf(address))
            End If

            If sheetInput.StartsWith("'", StringComparison.Ordinal) OrElse sheetInput.EndsWith("'", StringComparison.Ordinal) Then
                If sheetInput.Length < 2 OrElse Not sheetInput.StartsWith("'", StringComparison.Ordinal) OrElse
                   Not sheetInput.EndsWith("'", StringComparison.Ordinal) Then
                    Throw New ArgumentException("Invalid worksheet-name quoting.", NameOf(worksheetNameOrAddress))
                End If
                sheetInput = sheetInput.Substring(1, sheetInput.Length - 2).Replace("''", "'")
            End If

            Dim result As SpreadsheetRangeReference = FromAddress(rangeInput, sheetInput)
            If String.IsNullOrWhiteSpace(result.WorksheetName) Then
                Throw New ArgumentException("Specify a worksheet name for the addressed range.", NameOf(worksheetNameOrAddress))
            End If
            Return result
        End Function

        Public Shared Function FromAddress(address As String, Optional worksheetName As String = Nothing,
                                           Optional workbookName As String = Nothing) As SpreadsheetRangeReference
            Dim parsed As SpreadsheetRangeAddress = SpreadsheetRangeAddress.Parse(address, worksheetName)
            If parsed.WorkbookName.Length > 0 AndAlso Not String.IsNullOrWhiteSpace(workbookName) AndAlso
               Not String.Equals(parsed.WorkbookName, workbookName, StringComparison.OrdinalIgnoreCase) Then
                Throw New ArgumentException("Workbook name conflicts with the qualified address.", NameOf(workbookName))
            End If
            Return New SpreadsheetRangeReference With {
                .WorkbookName = If(parsed.WorkbookName.Length > 0, parsed.WorkbookName, If(workbookName, String.Empty)),
                .WorksheetName = parsed.WorksheetName, .Address = parsed.Address,
                .StartRow = parsed.StartRow, .StartColumn = parsed.StartColumn,
                .RowCount = parsed.RowCount, .ColumnCount = parsed.ColumnCount
            }
        End Function
    End Class

    ''' <summary>Ordered selection of ranges; ranges can be on different worksheets.</summary>
    Public NotInheritable Class SpreadsheetRangeSelection
        Public Sub New()
            Ranges = New List(Of SpreadsheetRangeReference)()
        End Sub
        Public Property Ranges As List(Of SpreadsheetRangeReference)
        Public Property SelectionId As String
        Public Sub Validate()
            If Ranges Is Nothing OrElse Ranges.Count = 0 Then Throw New ArgumentException("At least one range must be selected.")
            For Each reference As SpreadsheetRangeReference In Ranges
                If reference Is Nothing OrElse String.IsNullOrWhiteSpace(reference.WorksheetName) Then
                    Throw New ArgumentException("Every range must identify a worksheet.")
                End If
                Select Case reference.AddressDialect
                    Case SpreadsheetAddressDialect.ExcelA1
                        Dim parsed As SpreadsheetRangeAddress = SpreadsheetRangeAddress.Parse(reference.Address, reference.WorksheetName)
                        If parsed.RowCount < 1 OrElse parsed.ColumnCount < 1 Then Throw New ArgumentException("Range dimensions must be positive.")
                    Case SpreadsheetAddressDialect.HostNative
                        If reference.StartRow < 1 OrElse reference.StartColumn < 1 OrElse
                           reference.RowCount < 1 OrElse reference.ColumnCount < 1 OrElse
                           CLng(reference.StartRow) + CLng(reference.RowCount) - 1 > Integer.MaxValue OrElse
                           CLng(reference.StartColumn) + CLng(reference.ColumnCount) - 1 > Integer.MaxValue Then
                            Throw New ArgumentException("Host-native ranges require positive, representable coordinates.")
                        End If
                    Case Else
                        Throw New ArgumentException("Unknown range address dialect.")
                End Select
            Next
        End Sub
    End Class

    ''' <summary>Ordered results from an acquisition, not an analysis-domain table.</summary>
    Public NotInheritable Class SpreadsheetRangeSetData
        Inherits SpreadsheetAnalysisInput
        Public Sub New()
            Ranges = New List(Of SpreadsheetRangeData)()
        End Sub
        Public Property Ranges As List(Of SpreadsheetRangeData)
        Public Property SelectionId As String
    End Class
End Namespace
