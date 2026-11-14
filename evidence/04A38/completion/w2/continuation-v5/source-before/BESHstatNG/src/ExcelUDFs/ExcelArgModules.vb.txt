Option Explicit On
Option Strict On

Imports System
Imports System.Collections.Concurrent
Imports System.Collections.Generic
Imports System.Globalization
Imports ExcelDna.Integration

Namespace WorksheetFunctions

    ''' <summary>
    ''' Lightweight predicates for classifying Excel worksheet arguments.
    ''' </summary>
    Friend Module ExcelArgPredicates

        Friend Function IsMissingArg(v As Object) As Boolean
            Return v Is Nothing OrElse TypeOf v Is ExcelMissing OrElse TypeOf v Is ExcelEmpty
        End Function

        Friend Function IsBlankCell(cell As Object) As Boolean
            If cell Is Nothing OrElse TypeOf cell Is ExcelEmpty OrElse TypeOf cell Is ExcelMissing Then Return True
            If TypeOf cell Is String Then Return String.IsNullOrWhiteSpace(CStr(cell))
            Return False
        End Function

    End Module

    ''' <summary>
    ''' Shared worksheet-argument readers that normalize raw Excel values into text.
    ''' </summary>
    Friend Module ExcelArgReaders

        Public Function AsString(v As Object) As String
            If v Is Nothing OrElse TypeOf v Is ExcelEmpty OrElse TypeOf v Is ExcelMissing Then Return Nothing
            Return Convert.ToString(v).Trim()
        End Function

        Friend Function CellToTrimmedText(v As Object) As String
            If ExcelArgPredicates.IsMissingArg(v) Then Return ""

            If TypeOf v Is String Then
                Return CStr(v).Trim()
            End If

            If TypeOf v Is Double OrElse
               TypeOf v Is Single OrElse
               TypeOf v Is Decimal OrElse
               TypeOf v Is Integer OrElse
               TypeOf v Is Long OrElse
               TypeOf v Is Short Then

                Dim d As Double = Convert.ToDouble(v, CultureInfo.InvariantCulture)
                If Double.IsNaN(d) OrElse Double.IsInfinity(d) Then Return ""

                If Math.Abs(d - Math.Round(d)) < 0.000000000001R Then
                    Return CLng(Math.Round(d)).ToString(CultureInfo.InvariantCulture)
                End If

                Return d.ToString(CultureInfo.InvariantCulture)
            End If

            Dim s As String = Convert.ToString(v, CultureInfo.InvariantCulture)
            If s Is Nothing Then Return ""
            Return s.Trim()
        End Function

    End Module

    ''' <summary>
    ''' Shared worksheet-argument numeric parsers and small validation helpers.
    ''' </summary>
    Public Module ExcelArgNumeric

        Public Function TryGetDouble(v As Object) As Double?
            If v Is Nothing OrElse TypeOf v Is ExcelEmpty OrElse TypeOf v Is ExcelMissing Then
                Return Nothing
            End If
            If TypeOf v Is ExcelError OrElse TypeOf v Is Boolean OrElse TypeOf v Is String Then
                Return Nothing
            End If
            Try
                Dim d As Double = Convert.ToDouble(v)
                If Double.IsNaN(d) OrElse Double.IsInfinity(d) Then Return Nothing
                Return d
            Catch
                Return Nothing
            End Try
        End Function

        Public Function TryGetFiniteDouble(arg As Object, ByRef value As Double) As Boolean
            Dim parsed As Double? = ExcelArgNumeric.TryGetDouble(arg)
            If Not parsed.HasValue Then
                value = Double.NaN
                Return False
            End If
            value = parsed.Value
            Return Not Double.IsNaN(value) AndAlso Not Double.IsInfinity(value)
        End Function

        Public Function GetOptionalBool(v As Object, defaultValue As Boolean) As Boolean
            If v Is Nothing OrElse TypeOf v Is ExcelEmpty OrElse TypeOf v Is ExcelMissing Then Return defaultValue
            If TypeOf v Is Boolean Then Return CBool(v)
            Dim s As String = Convert.ToString(v).Trim()
            Return Global.BESHStatNG.ExcelCommon.UdfScalarChoice38.OptionalBoolean(s, defaultValue)
        End Function

        Public Function GetOptionalInt(v As Object, defaultValue As Integer) As Integer
            If v Is Nothing OrElse TypeOf v Is ExcelEmpty OrElse TypeOf v Is ExcelMissing Then Return defaultValue
            Dim d As Double? = ExcelArgNumeric.TryGetDouble(v)
            Return Global.BESHStatNG.ExcelCommon.UdfScalarChoice38.OptionalInteger(d, defaultValue)
        End Function

        Public Function GetOptionalDouble(v As Object, defaultValue As Double) As Double
            If v Is Nothing OrElse TypeOf v Is ExcelEmpty OrElse TypeOf v Is ExcelMissing Then Return defaultValue
            Dim d As Double? = ExcelArgNumeric.TryGetDouble(v)
            Return Global.BESHStatNG.ExcelCommon.UdfScalarChoice38.OptionalDouble(d, defaultValue)
        End Function

        Friend Function TryGetBinary01(cell As Object, ByRef value As Integer) As Boolean
            value = -1

            If cell Is Nothing OrElse TypeOf cell Is ExcelEmpty OrElse TypeOf cell Is ExcelMissing Then
                Return False
            End If

            Dim d As Double? = ExcelArgNumeric.TryGetDouble(cell)
            Dim logicalValue As Boolean? = Nothing
            If TypeOf cell Is Boolean Then logicalValue = CBool(cell)
            Return Global.BESHStatNG.ExcelCommon.UdfScalarChoice38.TryBinary(d, logicalValue, value)
        End Function

        Friend Function TryGetWholeNumber(v As Object, ByRef value As Integer) As Boolean
            value = 0

            Dim d As Double? = ExcelArgNumeric.TryGetDouble(v)
            Return Global.BESHStatNG.ExcelCommon.UdfScalarChoice38.TryWholeNumber(d, value)
        End Function

        Friend Function TryGetFiniteDoubleFlexible(v As Object, ByRef x As Double) As Boolean
            x = 0.0R

            Dim d As Double? = ExcelArgNumeric.TryGetDouble(v)
            If d.HasValue Then
                x = d.Value
                Return True
            End If

            If ExcelArgPredicates.IsMissingArg(v) OrElse TypeOf v Is ExcelError OrElse TypeOf v Is Boolean Then Return False

            Dim s As String = ExcelArgReaders.CellToTrimmedText(v)
            Return Global.BESHStatNG.ExcelCommon.UdfScalarChoice38.TryFiniteText(s, x)
        End Function

        Friend Function TryGetStatus01Flexible(v As Object, ByRef value As Integer) As Boolean
            value = 0

            If ExcelArgNumeric.TryGetBinary01(v, value) Then Return True

            Dim x As Double
            If Not ExcelArgNumeric.TryGetFiniteDoubleFlexible(v, x) Then Return False

            If x = 0.0R Then
                value = 0
                Return True
            End If

            If x = 1.0R Then
                value = 1
                Return True
            End If

            Return False
        End Function

        Friend Function TryGetOptionalThresholdVector(arg As Object, ByRef thresholds() As Double) As Boolean
            Return Global.BESHStatNG.UdfDataImport.TryGetOptionalProbabilityThresholds(arg, thresholds)
        End Function

        Friend Function TryGetSingleThresholdFromArg(arg As Object, ByRef threshold As Double,
                                                     Optional defaultValue As Double = 0.5R) As Boolean
            threshold = defaultValue
            Dim thresholds() As Double = Nothing
            If Not ExcelArgNumeric.TryGetOptionalThresholdVector(arg, thresholds) Then Return False
            If thresholds Is Nothing OrElse thresholds.Length = 0 Then Return True
            If thresholds.Length <> 1 Then Return False

            threshold = thresholds(0)
            Return True
        End Function

        Friend Function TryGetOptionalPositiveInteger(arg As Object,
                                                      ByRef value As Integer,
                                                      Optional defaultValue As Integer = 10,
                                                      Optional minValue As Integer = 1) As Boolean
            value = defaultValue
            If ExcelArgPredicates.IsMissingArg(arg) Then Return True

            Dim d As Double
            If Not ExcelArgNumeric.TryGetFiniteDouble(arg, d) Then Return False
            Return Global.BESHStatNG.ExcelCommon.UdfScalarChoice38.TryPositiveInteger(d, False, value, defaultValue, minValue)
        End Function

    End Module

    ''' <summary>
    ''' Shared helpers for resolving worksheet model handles from cache dictionaries.
    ''' </summary>
    Friend Module UdfCacheHelpers

        Friend Function TryGetHandleKey(handle As Object, ByRef key As String) As Boolean
            key = ExcelArgReaders.AsString(handle)
            Return Not String.IsNullOrWhiteSpace(key)
        End Function

        Friend Function TryGetCachedHandle(Of T As Class)(handle As Object,
                                                          cache As ConcurrentDictionary(Of String, T),
                                                          ByRef value As T) As Boolean
            value = Nothing

            Dim key As String = Nothing
            If Not TryGetHandleKey(handle, key) Then Return False

            Return cache.TryGetValue(key, value)
        End Function

    End Module

End Namespace
