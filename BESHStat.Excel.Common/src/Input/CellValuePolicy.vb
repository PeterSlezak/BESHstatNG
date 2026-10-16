Option Explicit On
Option Strict On

Imports System
Imports System.Globalization

Namespace ExcelCommon

    ''' <summary>Excel workbook date basis. Unknown never silently assumes 1900.</summary>
    Public Enum SpreadsheetDateSystem
        Unknown = 0
        Excel1900 = 1
        Excel1904 = 2
    End Enum

    Public Enum SpreadsheetMissingTextPolicy
        BlankOnly = 0
        BlankOrEmptyText = 1
        BlankOrWhitespaceText = 2
    End Enum

    Public Enum SpreadsheetErrorPolicy
        Reject = 0
        Preserve = 1
        TreatAsMissing = 2
    End Enum

    Public Enum SpreadsheetNumericTextPolicy
        Reject = 0
        Invariant = 1
        ExplicitCulture = 2
    End Enum

    ''' <summary>
    ''' An explicit opt-in set of conversion rules. Defaults preserve Stage 3.04 numeric
    ''' input semantics: only Number is numeric, only Blank is missing, errors are rejected.
    ''' </summary>
    Public NotInheritable Class SpreadsheetCellPolicyOptions
        Public Property MissingText As SpreadsheetMissingTextPolicy = SpreadsheetMissingTextPolicy.BlankOnly
        Public Property Errors As SpreadsheetErrorPolicy = SpreadsheetErrorPolicy.Reject
        Public Property NumericText As SpreadsheetNumericTextPolicy = SpreadsheetNumericTextPolicy.Reject
        Public Property NumericCultureName As String = "en-US"
        Public Property AllowBooleanNumbers As Boolean
        Public Property AllowDateSerialNumbers As Boolean

        Public Function GetNumericCulture() As CultureInfo
            Select Case NumericText
                Case SpreadsheetNumericTextPolicy.Invariant
                    Return CultureInfo.InvariantCulture
                Case SpreadsheetNumericTextPolicy.ExplicitCulture
                    If String.IsNullOrWhiteSpace(NumericCultureName) Then
                        Throw New ArgumentException("An explicit culture name is required.", NameOf(NumericCultureName))
                    End If
                    Return CultureInfo.GetCultureInfo(NumericCultureName)
                Case Else
                    Return CultureInfo.InvariantCulture
            End Select
        End Function
    End Class

    ''' <summary>Explicit Excel error value, never confused with a text cell containing '#N/A'.</summary>
    Public NotInheritable Class SpreadsheetExcelErrorValue
        Public Sub New(rawText As String)
            RawText = If(rawText, String.Empty)
            Code = SpreadsheetExcelErrors.Canonicalize(RawText)
        End Sub
        Public ReadOnly Property RawText As String
        Public ReadOnly Property Code As String
    End Class

    Public NotInheritable Class SpreadsheetExcelErrors
        Private Sub New()
        End Sub

        Public Shared Function Canonicalize(value As String) As String
            Dim code As String = If(value, String.Empty).Trim().ToUpperInvariant()
            Select Case code
                Case "#NULL!", "#DIV/0!", "#VALUE!", "#REF!", "#NAME?", "#NUM!", "#N/A", "#GETTING_DATA", "#SPILL!", "#CALC!", "#FIELD!", "#BLOCKED!", "#CONNECT!", "#BUSY!", "#UNKNOWN!", "#PYTHON!", "#CIRC!"
                    Return code
                Case Else
                    Return String.Empty
            End Select
        End Function
    End Class

    Public NotInheritable Class SpreadsheetCellEvaluation
        Public Property IsMissing As Boolean
        Public Property IsError As Boolean
        Public Property [Error] As SpreadsheetExcelErrorValue
        Public Property HasNumericValue As Boolean
        Public Property NumericValue As Double
        Public Property HasDateValue As Boolean
        Public Property DateValue As DateTime
    End Class

    ''' <summary>
    ''' Applies canonical interpretation without rewriting the source cell. No implicit
    ''' Boolean/text/date coercion, locale guesswork, or Excel error-string detection.
    ''' </summary>
    Public NotInheritable Class SpreadsheetCellValuePolicy
        Private Sub New()
        End Sub

        Public Shared Function Evaluate(cell As SpreadsheetCellValue,
                                        Optional options As SpreadsheetCellPolicyOptions = Nothing,
                                        Optional dateSystem As SpreadsheetDateSystem = SpreadsheetDateSystem.Unknown,
                                        Optional interpretNumberAsDate As Boolean = False) As SpreadsheetCellEvaluation
            Dim policy As SpreadsheetCellPolicyOptions = If(options, New SpreadsheetCellPolicyOptions())
            Dim answer As New SpreadsheetCellEvaluation()
            If cell Is Nothing OrElse cell.Kind = SpreadsheetCellKind.Blank Then
                answer.IsMissing = True
                Return answer
            End If
            Select Case cell.Kind
                Case SpreadsheetCellKind.CellError
                    answer.IsError = True
                    answer.Error = New SpreadsheetExcelErrorValue(cell.TextValue)
                    answer.IsMissing = policy.Errors = SpreadsheetErrorPolicy.TreatAsMissing
                    Return answer

                Case SpreadsheetCellKind.Number
                    If Double.IsNaN(cell.NumberValue) OrElse Double.IsInfinity(cell.NumberValue) Then
                        Throw New ArgumentException("Non-finite spreadsheet numbers are not valid input.", NameOf(cell))
                    End If
                    answer.NumericValue = cell.NumberValue
                    answer.HasNumericValue = Not interpretNumberAsDate OrElse policy.AllowDateSerialNumbers
                    If interpretNumberAsDate Then
                        Dim d As DateTime
                        If Not ExcelDateSerialConverter.TryToDateTime(cell.NumberValue, dateSystem, d) Then
                            Throw New ArgumentException("Invalid date serial or unverified workbook date system.", NameOf(cell))
                        End If
                        answer.HasDateValue = True
                        answer.DateValue = d
                    End If

                Case SpreadsheetCellKind.Text
                    Dim value As String = If(cell.TextValue, String.Empty)
                    If policy.MissingText = SpreadsheetMissingTextPolicy.BlankOrEmptyText AndAlso value.Length = 0 Then
                        answer.IsMissing = True
                    ElseIf policy.MissingText = SpreadsheetMissingTextPolicy.BlankOrWhitespaceText AndAlso String.IsNullOrWhiteSpace(value) Then
                        answer.IsMissing = True
                    ElseIf policy.NumericText <> SpreadsheetNumericTextPolicy.Reject Then
                        Dim result As Double
                        ' Float allows a leading sign, decimals and exponent; thousands separators are not inferred.
                        If Double.TryParse(value, NumberStyles.Float, policy.GetNumericCulture(), result) AndAlso
                            Not Double.IsNaN(result) AndAlso Not Double.IsInfinity(result) Then
                            answer.HasNumericValue = True
                            answer.NumericValue = result
                        End If
                    End If

                Case SpreadsheetCellKind.Bool
                    If policy.AllowBooleanNumbers Then
                        answer.HasNumericValue = True
                        answer.NumericValue = If(cell.BooleanValue, 1.0R, 0.0R)
                    End If

                Case Else
                    Throw New ArgumentOutOfRangeException(NameOf(cell), "Unknown spreadsheet cell kind.")
            End Select
            Return answer
        End Function

        Public Shared Function ReadNumeric(cell As SpreadsheetCellValue,
                                           Optional options As SpreadsheetCellPolicyOptions = Nothing) As Double
            Dim policy As SpreadsheetCellPolicyOptions = If(options, New SpreadsheetCellPolicyOptions())
            Dim result As SpreadsheetCellEvaluation = Evaluate(cell, policy)
            If result.IsError AndAlso policy.Errors = SpreadsheetErrorPolicy.Reject Then
                Throw New ArgumentException("An Excel error cell cannot be used as a numeric observation.", NameOf(cell))
            End If
            If Not result.HasNumericValue OrElse result.IsMissing Then
                Throw New ArgumentException("The cell is not a permitted numeric observation.", NameOf(cell))
            End If
            Return result.NumericValue
        End Function
    End Class

    ''' <summary>Excel date serial conversion with the historical phantom 1900-02-29 rejected.</summary>
    Public NotInheritable Class ExcelDateSerialConverter
        Private Sub New()
        End Sub

        Public Shared Function TryToDateTime(serial As Double, basis As SpreadsheetDateSystem, ByRef value As DateTime) As Boolean
            value = DateTime.MinValue
            If Double.IsNaN(serial) OrElse Double.IsInfinity(serial) OrElse serial < 0.0R Then Return False
            If basis <> SpreadsheetDateSystem.Excel1900 AndAlso basis <> SpreadsheetDateSystem.Excel1904 Then Return False
            If basis = SpreadsheetDateSystem.Excel1900 AndAlso serial >= 60.0R AndAlso serial < 61.0R Then Return False
            Dim oaValue As Double = If(basis = SpreadsheetDateSystem.Excel1904, serial + 1462.0R,
                                       If(serial < 60.0R, serial + 1.0R, serial))
            Try
                value = DateTime.FromOADate(oaValue)
                Return True
            Catch ex As ArgumentException
                Return False
            End Try
        End Function

        Public Shared Function ToSerial(value As DateTime, basis As SpreadsheetDateSystem) As Double
            If basis <> SpreadsheetDateSystem.Excel1900 AndAlso basis <> SpreadsheetDateSystem.Excel1904 Then
                Throw New ArgumentException("The workbook date system must be known.", NameOf(basis))
            End If
            Dim oa As Double = value.ToOADate()
            Dim serial As Double = If(basis = SpreadsheetDateSystem.Excel1904, oa - 1462.0R,
                                      If(oa < 61.0R, oa - 1.0R, oa))
            If serial < 0.0R Then Throw New ArgumentOutOfRangeException(NameOf(value), "Excel dates before the workbook epoch are unsupported.")
            Return serial
        End Function
    End Class
End Namespace
