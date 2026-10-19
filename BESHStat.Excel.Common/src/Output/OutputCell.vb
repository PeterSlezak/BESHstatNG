Option Explicit On
Option Strict On

Imports System
Imports System.Collections.Generic
Imports System.Globalization
Imports System.Numerics

Namespace ExcelCommon
    Public Enum OutputCellKind
        Blank = 0
        Number = 1
        Text = 2
        BooleanValue = 3
        [Error] = 4
        Formula = 5
    End Enum

    Public Enum OutputNumberKind
        Binary64 = 1
        ExactDecimal = 2
    End Enum

    Public Enum OutputErrorKind
        Unknown = 0
        NullIntersection = 1
        DivisionByZero = 2
        InvalidValue = 3
        InvalidReference = 4
        UnknownName = 5
        InvalidNumber = 6
        NotAvailable = 7
        GettingData = 8
        NonFinite = 9
    End Enum

    ''' <summary>
    ''' Immutable scalar union. Only the fields selected by Kind/NumberKind are populated.
    ''' ExactDecimal retains every CLR integer/Decimal without binary rounding. Binary64
    ''' retains finite Single/Double values, including the sign bit of negative zero.
    ''' </summary>
    Public NotInheritable Class OutputCell
        Private Sub New(kind As OutputCellKind, Optional numberKind As OutputNumberKind? = Nothing,
                        Optional binaryValue As Double? = Nothing, Optional decimalValue As Decimal? = Nothing,
                        Optional text As String = Nothing, Optional booleanValue As Boolean? = Nothing,
                        Optional errorKind As OutputErrorKind? = Nothing, Optional errorCode As String = Nothing,
                        Optional originalError As String = Nothing, Optional nativeCode As Integer? = Nothing,
                        Optional safeDetail As String = Nothing, Optional formulaExpression As String = Nothing,
                        Optional formulaLanguage As String = Nothing)
            Me.Kind = kind
            Me.NumberKind = numberKind
            Me.BinaryValue = binaryValue
            Me.DecimalValue = decimalValue
            Me.TextValue = text
            Me.BooleanValue = booleanValue
            Me.ErrorKind = errorKind
            Me.ErrorCode = errorCode
            Me.OriginalError = originalError
            Me.NativeErrorCode = nativeCode
            Me.SafeErrorDetail = safeDetail
            Me.FormulaExpression = formulaExpression
            Me.FormulaLanguage = formulaLanguage
        End Sub

        Public ReadOnly Property Kind As OutputCellKind
        Public ReadOnly Property NumberKind As OutputNumberKind?
        Public ReadOnly Property BinaryValue As Double?
        Public ReadOnly Property DecimalValue As Decimal?
        Public ReadOnly Property TextValue As String
        Public ReadOnly Property BooleanValue As Boolean?
        Public ReadOnly Property ErrorKind As OutputErrorKind?
        Public ReadOnly Property ErrorCode As String
        Public ReadOnly Property OriginalError As String
        ''' <summary>Only a code supplied by the caller; no native codes are synthesized.</summary>
        Public ReadOnly Property NativeErrorCode As Integer?
        Public ReadOnly Property SafeErrorDetail As String
        Public ReadOnly Property FormulaExpression As String
        Public ReadOnly Property FormulaLanguage As String

        Public Shared Function BlankCell() As OutputCell
            Return New OutputCell(OutputCellKind.Blank)
        End Function

        Public Shared Function NumberCell(value As Double) As OutputCell
            If Double.IsNaN(value) OrElse Double.IsInfinity(value) Then Throw New ArgumentOutOfRangeException(NameOf(value), "Number cells must be finite.")
            Return New OutputCell(OutputCellKind.Number, OutputNumberKind.Binary64, binaryValue:=value)
        End Function

        Public Shared Function DecimalCell(value As Decimal) As OutputCell
            Return New OutputCell(OutputCellKind.Number, OutputNumberKind.ExactDecimal, decimalValue:=value)
        End Function

        Public Shared Function TextCell(value As String) As OutputCell
            If value Is Nothing Then Throw New ArgumentNullException(NameOf(value))
            Return New OutputCell(OutputCellKind.Text, text:=value)
        End Function

        Public Shared Function BooleanCell(value As Boolean) As OutputCell
            Return New OutputCell(OutputCellKind.BooleanValue, booleanValue:=value)
        End Function

        ''' <summary>
        ''' Explicit standard token mapping, ordinal and case sensitive. Unknown tokens retain
        ''' their original spelling and optional supplied native code. Strings in FromRaw never
        ''' enter this mapping. safeDetail must already be safe for downstream display.
        ''' </summary>
        Public Shared Function SpreadsheetError(token As String, Optional nativeCode As Integer? = Nothing,
                                                Optional safeDetail As String = Nothing) As OutputCell
            If token Is Nothing Then Throw New ArgumentNullException(NameOf(token))
            Dim kind As OutputErrorKind
            Dim code As String
            Select Case token
                Case "#NULL!" : kind = OutputErrorKind.NullIntersection : code = "spreadsheet.null"
                Case "#DIV/0!" : kind = OutputErrorKind.DivisionByZero : code = "spreadsheet.division-by-zero"
                Case "#VALUE!" : kind = OutputErrorKind.InvalidValue : code = "spreadsheet.value"
                Case "#REF!" : kind = OutputErrorKind.InvalidReference : code = "spreadsheet.reference"
                Case "#NAME?" : kind = OutputErrorKind.UnknownName : code = "spreadsheet.name"
                Case "#NUM!" : kind = OutputErrorKind.InvalidNumber : code = "spreadsheet.number"
                Case "#N/A" : kind = OutputErrorKind.NotAvailable : code = "spreadsheet.not-available"
                Case "#GETTING_DATA" : kind = OutputErrorKind.GettingData : code = "spreadsheet.getting-data"
                Case Else : kind = OutputErrorKind.Unknown : code = "spreadsheet.unknown"
            End Select
            Return New OutputCell(OutputCellKind.Error, errorKind:=kind, errorCode:=code,
                                  originalError:=token, nativeCode:=nativeCode, safeDetail:=safeDetail)
        End Function

        ''' <summary>
        ''' Caller attests trusted intent. Language is an explicit logical dialect identifier;
        ''' no grammar parsing, reference translation or evaluation occurs. Future adapters
        ''' must explicitly support the dialect before executing. Never inferred from text.
        ''' </summary>
        Public Shared Function TrustedFormula(expression As String, language As String) As OutputCell
            If String.IsNullOrWhiteSpace(expression) Then Throw New ArgumentException("Formula intent is required.", NameOf(expression))
            If String.IsNullOrWhiteSpace(language) Then Throw New ArgumentException("Formula language is required.", NameOf(language))
            For Each ch As Char In expression & language
                If Char.IsControl(ch) Then Throw New ArgumentException("Control characters are not supported in formula intent.")
            Next
            Return New OutputCell(OutputCellKind.Formula, formulaExpression:=expression, formulaLanguage:=language)
        End Function

        ''' <summary>
        ''' Explicit bridge for the 02.01 whitelist. Unsupported inputs throw, never ToString.
        ''' Nonfinite floats become portable error codes (not Excel errors/native codes).
        ''' Integers and decimals remain exact; future binary-only adapters must check loss.
        ''' </summary>
        Public Shared Function FromRaw(value As Object) As OutputCell
            If value Is Nothing Then Return BlankCell()
            Dim valueType As Type = value.GetType()
            If Not valueType.IsEnum Then
                Select Case Type.GetTypeCode(valueType)
                    Case TypeCode.String : Return TextCell(DirectCast(value, String))
                    Case TypeCode.Boolean : Return BooleanCell(DirectCast(value, Boolean))
                    Case TypeCode.SByte, TypeCode.Byte, TypeCode.Int16, TypeCode.UInt16,
                         TypeCode.Int32, TypeCode.UInt32, TypeCode.Int64, TypeCode.UInt64, TypeCode.Decimal
                        Return DecimalCell(Convert.ToDecimal(value, CultureInfo.InvariantCulture))
                    Case TypeCode.Single, TypeCode.Double
                        Dim number As Double = Convert.ToDouble(value, CultureInfo.InvariantCulture)
                        If Double.IsNaN(number) OrElse Double.IsInfinity(number) Then
                            Dim code As String = If(Double.IsNaN(number), "number.nan",
                                If(number > 0, "number.positive-infinity", "number.negative-infinity"))
                            Return New OutputCell(OutputCellKind.Error, errorKind:=OutputErrorKind.NonFinite, errorCode:=code)
                        End If
                        Return NumberCell(number)
                End Select
            End If
            Throw New ArgumentException("Unsupported output cell payload type: " & valueType.FullName & ".", NameOf(value))
        End Function

        ''' <summary>
        ''' Probability-only preview projection to Core's Double policy. Out-of-[0,1] exact decimals
        ''' are rejected before conversion, so rounding cannot make them valid probabilities.
        ''' The exact primary value is never replaced by this projection.
        ''' </summary>
        Public Function TryGetProbability(ByRef value As Double) As Boolean
            value = 0
            If Kind <> OutputCellKind.Number Then Return False
            If NumberKind.Value = OutputNumberKind.ExactDecimal Then
                If DecimalValue.Value < 0D OrElse DecimalValue.Value > 1D Then Return False
                value = CDbl(DecimalValue.Value)
            Else
                value = BinaryValue.Value
            End If
            Return value >= 0 AndAlso value <= 1
        End Function

        ''' <summary>
        ''' Single significance predicate: a typed probability <= finite alpha in (0,1).
        ''' Exact decimals compare as rational numbers against the actual binary64 alpha;
        ''' preview rounding to Double cannot turn a value just above alpha significant.
        ''' </summary>
        Public Function IsSignificantAt(alpha As Double) As Boolean
            If Double.IsNaN(alpha) OrElse Double.IsInfinity(alpha) OrElse alpha <= 0 OrElse alpha >= 1 Then
                Throw New ArgumentOutOfRangeException(NameOf(alpha))
            End If
            Dim probability As Double
            If Not TryGetProbability(probability) Then Return False
            If NumberKind.Value = OutputNumberKind.Binary64 Then Return probability <= alpha

            ' Decimal is a 96-bit integer / 10^scale. Alpha is significand * 2^exponent.
            ' Cross multiplication uses unbounded integers, avoiding overflow and rounding.
            Dim parts As Integer() = Decimal.GetBits(DecimalValue.Value)
            Dim magnitude As BigInteger = New BigInteger(CLng(parts(0)) And &HFFFFFFFFL) +
                (New BigInteger(CLng(parts(1)) And &HFFFFFFFFL) << 32) +
                (New BigInteger(CLng(parts(2)) And &HFFFFFFFFL) << 64)
            Dim denominator As BigInteger = BigInteger.Pow(10, (parts(3) >> 16) And &HFF)
            Dim alphaBits As Long = BitConverter.DoubleToInt64Bits(alpha)
            Dim exponentBits As Integer = CInt((alphaBits >> 52) And &H7FFL)
            Dim significand As Long = alphaBits And &HFFFFFFFFFFFFFL
            Dim exponent As Integer = -1074
            If exponentBits <> 0 Then
                significand = significand Or &H10000000000000L
                exponent = exponentBits - 1075
            End If
            ' Alpha < 1 guarantees a negative exponent, including subnormal alpha.
            Return (magnitude << -exponent) <= New BigInteger(significand) * denominator
        End Function
    End Class

    ''' <summary>Independent canonical matrix snapshot; coordinates are zero-based array coordinates.</summary>
    Public NotInheritable Class OutputCellTable
        Private ReadOnly _cells As OutputCell(,)

        Public Sub New(artifact As OutputTableArtifact)
            If artifact Is Nothing Then Throw New ArgumentNullException(NameOf(artifact))
            ArtifactId = artifact.Id
            RowCount = artifact.RowCount
            ColumnCount = artifact.ColumnCount
            _cells = New OutputCell(RowCount - 1, ColumnCount - 1) {}
            Dim raw As Object(,) = artifact.Model.Values
            For row As Integer = 0 To RowCount - 1
                For column As Integer = 0 To ColumnCount - 1
                    _cells(row, column) = OutputCell.FromRaw(raw(row, column))
                Next
            Next
        End Sub

        Public ReadOnly Property ArtifactId As String
        Public ReadOnly Property RowCount As Integer
        Public ReadOnly Property ColumnCount As Integer

        Public Function CellAt(row As Integer, column As Integer) As OutputCell
            If row < 0 OrElse row >= RowCount Then Throw New ArgumentOutOfRangeException(NameOf(row))
            If column < 0 OrElse column >= ColumnCount Then Throw New ArgumentOutOfRangeException(NameOf(column))
            Return _cells(row, column)
        End Function
    End Class
End Namespace
