Option Explicit On
Option Strict On

Imports System
Imports System.Collections.Generic

' Option and scalar import helpers shared by worksheet UDF argument readers.
Partial Friend Module UdfDataImport

    ''' <summary>
    ''' Normalizes option tokens for worksheet arguments by trimming, lower-casing, and removing common separators.
    ''' This keeps UDF option parsing independent of case, spaces, hyphens, and underscores.
    ''' </summary>
    Private Function NormalizeOptionKey(value As String) As String
        If value Is Nothing Then Return String.Empty

        Return value.Trim().ToLowerInvariant().Replace(" ", String.Empty).Replace("-", String.Empty).Replace("_", String.Empty)
    End Function

    ''' <summary>
    ''' Converts the histogram bin-rule worksheet argument into the GUI histogram rule label.
    ''' </summary>
    Friend Function GetHistogramBinRule(arg As Object) As String
        Dim token As String = NormalizeOptionKey(Global.BESHStatNG.WorksheetFunctions.ExcelArgReaders.AsString(arg))
        If String.IsNullOrWhiteSpace(token) Then Return "(Sturges)"

        Select Case token
            Case "sturges", "sturge"
                Return "(Sturges)"
            Case "doane", "doan"
                Return "(Doane)"
            Case "scott"
                Return "(Scott)"
            Case "freedmandiaconis", "fd"
                Return "(Freedman-Diaconis)"
            Case Else
                Return "(Sturges)"
        End Select
    End Function

    ''' <summary>
    ''' Parses an optional scalar, row-vector, or column-vector of probability thresholds.
    ''' Blank cells are ignored; values must be finite probabilities in [0, 1].
    ''' Returned thresholds are sorted and de-duplicated using the legacy tolerance.
    ''' </summary>
    Friend Function TryGetOptionalProbabilityThresholds(arg As Object, ByRef thresholds() As Double) As Boolean
        thresholds = Nothing
        If Global.BESHStatNG.WorksheetFunctions.ExcelArgPredicates.IsMissingArg(arg) Then Return True

        Dim scalarValue As Double
        If Global.BESHStatNG.WorksheetFunctions.ExcelArgNumeric.TryGetFiniteDouble(arg, scalarValue) Then
            If Not IsValidProbability(scalarValue) Then Return False
            thresholds = New Double() {ClampProbabilityValue(scalarValue)}
            Return True
        End If

        Dim arr As Object(,) = Get2D(arg)
        If arr Is Nothing Then Return False

        Dim rows As Integer = arr.GetLength(0)
        Dim cols As Integer = arr.GetLength(1)
        If rows <> 1 AndAlso cols <> 1 Then Return False

        Dim values As New List(Of Double)()
        If rows = 1 Then
            For j As Integer = 0 To cols - 1
                If Not TryAppendProbabilityThreshold(arr(0, j), values) Then Return False
            Next
        Else
            For i As Integer = 0 To rows - 1
                If Not TryAppendProbabilityThreshold(arr(i, 0), values) Then Return False
            Next
        End If

        If values.Count = 0 Then Return True

        values.Sort()
        Dim uniqueValues As New List(Of Double)(values.Count)
        For i As Integer = 0 To values.Count - 1
            If i = 0 OrElse Math.Abs(values(i) - values(i - 1)) > 0.000000000001R Then
                uniqueValues.Add(values(i))
            End If
        Next

        thresholds = uniqueValues.ToArray()
        Return True
    End Function

    Private Function TryAppendProbabilityThreshold(cell As Object, values As List(Of Double)) As Boolean
        If Global.BESHStatNG.WorksheetFunctions.ExcelArgPredicates.IsBlankCell(cell) Then Return True

        Dim d As Double
        If Not Global.BESHStatNG.WorksheetFunctions.ExcelArgNumeric.TryGetFiniteDouble(cell, d) Then Return False
        If Not IsValidProbability(d) Then Return False

        values.Add(ClampProbabilityValue(d))
        Return True
    End Function

    Private Function IsValidProbability(value As Double) As Boolean
        Return Not Double.IsNaN(value) AndAlso Not Double.IsInfinity(value) AndAlso value >= 0.0R AndAlso value <= 1.0R
    End Function

    Private Function ClampProbabilityValue(value As Double) As Double
        If value < 0.0R Then Return 0.0R
        If value > 1.0R Then Return 1.0R
        Return value
    End Function

    Friend Function TryGetEquivalenceMargins(lowerArg As Object, upperArg As Object, ByRef lowerValue As Double, ByRef upperValue As Double) As Boolean
        lowerValue = Double.NaN
        upperValue = Double.NaN

        If Not Global.BESHStatNG.WorksheetFunctions.ExcelArgNumeric.TryGetFiniteDouble(lowerArg, lowerValue) Then Return False

        If Global.BESHStatNG.WorksheetFunctions.ExcelArgPredicates.IsMissingArg(upperArg) Then
            Dim m As Double = Math.Abs(lowerValue)
            If m <= 0.0 Then Return False
            lowerValue = -m
            upperValue = m
            Return True
        End If

        If Not Global.BESHStatNG.WorksheetFunctions.ExcelArgNumeric.TryGetFiniteDouble(upperArg, upperValue) Then Return False
        Return True
    End Function

    ''' <summary>
    ''' Attempts to parse and validate an alpha value from an optional Excel argument.
    ''' </summary>
    ''' <param name="arg">
    ''' The Excel argument to parse. May be missing, numeric, or a string representation of a number.
    ''' </param>
    ''' <param name="alpha">
    ''' When this method returns <c>True</c>, contains the parsed alpha value.
    ''' Defaults to <c>0.05</c> when the argument is missing.
    ''' </param>
    ''' <returns>
    ''' <c>True</c> if a valid alpha in the open interval <c>(0, 1)</c> could be obtained;
    ''' otherwise <c>False</c>.
    ''' </returns>
    Friend Function TryParseAlpha(arg As Object, ByRef alpha As Double) As Boolean
        alpha = 0.05
        If Global.BESHStatNG.WorksheetFunctions.ExcelArgPredicates.IsMissingArg(arg) Then Return True

        Try
            If TypeOf arg Is String Then
                Dim s As String = Convert.ToString(arg).Trim()
                If Not Double.TryParse(s, Globalization.NumberStyles.Any, Globalization.CultureInfo.InvariantCulture, alpha) AndAlso
                   Not Double.TryParse(s, alpha) Then
                    Return False
                End If
            Else
                alpha = Convert.ToDouble(arg)
            End If
        Catch
            Return False
        End Try

        If Double.IsNaN(alpha) OrElse Double.IsInfinity(alpha) Then Return False
        If alpha <= 0.0 OrElse alpha >= 1.0 Then Return False

        Return True
    End Function
End Module
