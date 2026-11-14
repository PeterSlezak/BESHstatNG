Option Explicit On
Option Strict On

Imports System
Imports System.Globalization
Imports ExcelDna.Integration

''' <summary>
''' Regression-oriented worksheet output helpers used by model UDF modules.
''' </summary>
Friend Module UdfRegressionOutput

    ''' <summary>
    ''' Returns <c>exp(exponent)</c> formatted for Excel output, with overflow rendered as "Inf"
    ''' and severe underflow rendered as 0.
    ''' </summary>
    Public Function ExpForDisplay(exponent As Double) As Object
        Global.BESHStatNG.UdfTrace38.Hit("BESHstatNG/src/ExcelUDFs/Regression/UdfRegressionOutput.vb|BESHStatNG.UdfRegressionOutput|FunctionBlock|3918beb544898603@449:700")
        If Double.IsNaN(exponent) Then Return ExcelError.ExcelErrorNum
        If Double.IsPositiveInfinity(exponent) Then Return "Inf"
        If Double.IsNegativeInfinity(exponent) Then Return 0.0R

        Dim maxLog As Double = Math.Log(Double.MaxValue)
        Dim minLog As Double = Math.Log(Double.Epsilon)

        If exponent > maxLog Then Return "Inf"
        If exponent < minLog Then Return 0.0R

        Dim value As Double = Math.Exp(exponent)
        If Double.IsPositiveInfinity(value) Then Return "Inf"
        If Double.IsNaN(value) Then Return ExcelError.ExcelErrorNum
        Return value
    End Function

    ''' <summary>
    ''' Wraps a residual or leverage vector in a spilled-object array.
    ''' </summary>
    ''' <param name="vec">Vector of per-observation values.</param>
    ''' <param name="header">Column label to use when <paramref name="includeHeader"/> is True.</param>
    ''' <param name="includeHeader">Whether to include a header row.</param>
    ''' <returns>A spilled-object array containing the requested vector.</returns>
    Friend Function BuildResidualVectorOutput(vec() As Double, header As String, includeHeader As Boolean) As Object
        Global.BESHStatNG.UdfTrace38.Hit("BESHstatNG/src/ExcelUDFs/Regression/UdfRegressionOutput.vb|BESHStatNG.UdfRegressionOutput|FunctionBlock|13906656dcaff16f@1604:585")
        If vec Is Nothing Then Return ExcelError.ExcelErrorNA

        Dim n As Integer = vec.Length
        Dim outRows As Integer = If(includeHeader, n + 1, n)
        Dim out(outRows - 1, 0) As Object
        Dim r0 As Integer = 0

        If includeHeader Then
            out(0, 0) = header
            r0 = 1
        End If

        For i As Integer = 0 To n - 1
            out(r0 + i, 0) = vec(i)
        Next

        Return out
    End Function

    Friend Function ComputeLinearPredictor(expandedX(,) As Double,
                                            rowIndex As Integer,
                                            beta() As Double,
                                            includeIntercept As Boolean,
                                            offsetVals() As Double) As Double
        Global.BESHStatNG.UdfTrace38.Hit("BESHstatNG/src/ExcelUDFs/Regression/UdfRegressionOutput.vb|BESHStatNG.UdfRegressionOutput|FunctionBlock|70d7310463f2da36@2197:1017")
        Dim eta As Double = 0.0R
        Dim startBeta As Integer = 0

        If includeIntercept AndAlso beta IsNot Nothing AndAlso beta.Length > 0 Then
            eta = beta(0)
            startBeta = 1
        End If

        If expandedX IsNot Nothing Then
            Dim p As Integer = expandedX.GetLength(1)
            For j As Integer = 0 To p - 1
                eta += expandedX(rowIndex, j) * beta(startBeta + j)
            Next
        End If

        If offsetVals IsNot Nothing AndAlso rowIndex >= 0 AndAlso rowIndex < offsetVals.Length Then
            eta += offsetVals(rowIndex)
        End If

        Return eta
    End Function

    Friend Function SafeExcelNumber(value As Double) As Object
        Global.BESHStatNG.UdfTrace38.Hit("BESHstatNG/src/ExcelUDFs/Regression/UdfRegressionOutput.vb|BESHStatNG.UdfRegressionOutput|FunctionBlock|340153636b4d0020@3222:199")
        If Double.IsNaN(value) OrElse Double.IsInfinity(value) Then Return ExcelError.ExcelErrorNum
        Return value
    End Function

    ''' <summary>
    ''' Builds category-specific column headers for residual or probability outputs.
    ''' </summary>
    ''' <param name="prefix">Prefix describing the quantity shown in each category column.</param>
    ''' <param name="categories">Outcome categories in model order.</param>
    ''' <returns>An array of column labels aligned with the category-specific matrix.</returns>
    Friend Function CategoryHeaders(prefix As String, categories() As Integer) As String()
        Global.BESHStatNG.UdfTrace38.Hit("BESHstatNG/src/ExcelUDFs/Regression/UdfRegressionOutput.vb|BESHStatNG.UdfRegressionOutput|FunctionBlock|10a60d5f1f4a8061@3828:401")
        If categories Is Nothing Then Return New String() {}
        Dim out(categories.Length - 1) As String
        For i As Integer = 0 To categories.Length - 1
            out(i) = prefix & "(" & categories(i).ToString(CultureInfo.InvariantCulture) & ")"
        Next
        Return out
    End Function

    ''' <summary>
    ''' Wraps a category-specific residual matrix in a spilled-object array.
    ''' </summary>
    ''' <param name="mat">Residual matrix with one row per observation and one column per category.</param>
    ''' <param name="headers">Column headers aligned with <paramref name="mat"/>.</param>
    ''' <param name="includeHeader">Whether to include a header row.</param>
    ''' <returns>A spilled-object array containing the requested residual matrix.</returns>
    Friend Function BuildResidualMatrixOutput(mat(,) As Double, headers() As String, includeHeader As Boolean) As Object
        Global.BESHStatNG.UdfTrace38.Hit("BESHstatNG/src/ExcelUDFs/Regression/UdfRegressionOutput.vb|BESHStatNG.UdfRegressionOutput|FunctionBlock|5d7894c9e5b6b5a8@4725:781")
        If mat Is Nothing Then Return ExcelError.ExcelErrorNA

        Dim n As Integer = mat.GetLength(0)
        Dim p As Integer = mat.GetLength(1)
        Dim outRows As Integer = If(includeHeader, n + 1, n)
        Dim out(outRows - 1, p - 1) As Object
        Dim r0 As Integer = 0

        If includeHeader Then
            For j As Integer = 0 To p - 1
                out(0, j) = headers(j)
            Next
            r0 = 1
        End If

        For i As Integer = 0 To n - 1
            For j As Integer = 0 To p - 1
                out(r0 + i, j) = mat(i, j)
            Next
        Next

        Return out
    End Function
End Module