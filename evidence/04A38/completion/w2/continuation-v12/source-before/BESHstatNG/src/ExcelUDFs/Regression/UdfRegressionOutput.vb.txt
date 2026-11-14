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
        Return MaterializePortableReport38(Global.BESHStatNG.ExcelCommon.UdfRegressionOutputReports38.BuildResidualVectorOutput(vec, header, includeHeader))
    End Function

    Friend Function ComputeLinearPredictor(expandedX(,) As Double,
                                            rowIndex As Integer,
                                            beta() As Double,
                                            includeIntercept As Boolean,
                                            offsetVals() As Double) As Double
        Return Global.BESHStatNG.WorksheetCompatibility.UdfNumericPreparation38.ComputeLinearPredictor(expandedX, rowIndex, beta, includeIntercept, offsetVals)
    End Function

    Friend Function SafeExcelNumber(value As Double) As Object
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
        Return Global.BESHStatNG.ExcelCommon.UdfRegressionOutputReports38.CategoryHeaders(prefix, categories)
    End Function

    ''' <summary>
    ''' Wraps a category-specific residual matrix in a spilled-object array.
    ''' </summary>
    ''' <param name="mat">Residual matrix with one row per observation and one column per category.</param>
    ''' <param name="headers">Column headers aligned with <paramref name="mat"/>.</param>
    ''' <param name="includeHeader">Whether to include a header row.</param>
    ''' <returns>A spilled-object array containing the requested residual matrix.</returns>
    Friend Function BuildResidualMatrixOutput(mat(,) As Double, headers() As String, includeHeader As Boolean) As Object
        Return MaterializePortableReport38(Global.BESHStatNG.ExcelCommon.UdfRegressionOutputReports38.BuildResidualMatrixOutput(mat, headers, includeHeader))
    End Function
End Module