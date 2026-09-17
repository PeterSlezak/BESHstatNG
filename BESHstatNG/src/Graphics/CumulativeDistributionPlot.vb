Option Explicit On
Option Strict On
Option Infer On

Imports System
Imports System.Collections.Generic
Imports System.Globalization
Imports System.Linq

''' <summary>
''' Controls which cumulative-distribution elements are prepared by
''' <see cref="CumulativeDistributionPlot"/>.
''' </summary>
Public Enum CumulativeDistributionPlotMode
    EmpiricalOnly = 0
    EmpiricalWithFittedDistribution = 1
    FittedDistributionOnly = 2
End Enum

''' <summary>
''' Y-axis scaling used by a cumulative distribution plot.
''' </summary>
Public Enum CumulativeDistributionYScale
    Probability = 0
    Percent = 1
End Enum

''' <summary>
''' Controls how the empirical cumulative probabilities are placed.
''' </summary>
Public Enum CumulativeDistributionEmpiricalMethod
    ''' <summary>True empirical CDF: F_n(x) = count(X &lt;= x) / n.</summary>
    ExactEcdf = 0

    ''' <summary>
    ''' Minitab-style median-rank plotting probabilities: (i - 0.3) / (n + 0.4).
    ''' This is useful when reproducing Minitab fitted empirical-CDF charts.
    ''' </summary>
    MinitabMedianRank = 1
End Enum

''' <summary>
''' Continuous distributions supported by the CDF/ECDF backend.
'''
''' The list mirrors the continuous fitted distributions exposed by Minitab for
''' empirical CDF/histogram distribution fits. Threshold variants are included
''' because they are useful when the support does not naturally begin at zero.
''' </summary>
Public Enum CumulativeDistributionKind
    Normal = 0
    Lognormal = 1
    ThreeParameterLognormal = 2
    Gamma = 3
    ThreeParameterGamma = 4
    Exponential = 5
    TwoParameterExponential = 6
    SmallestExtremeValue = 7
    Weibull = 8
    ThreeParameterWeibull = 9
    LargestExtremeValue = 10
    Logistic = 11
    Loglogistic = 12
    ThreeParameterLoglogistic = 13
End Enum

''' <summary>
''' Excel-independent calculation options for <see cref="CumulativeDistributionPlot"/>.
''' Appearance settings (colours, line widths, markers, legend placement, chart size)
''' intentionally belong in the Excel renderer rather than in this class.
''' </summary>
Public Class CumulativeDistributionPlotOptions
    ''' <summary>What should be prepared for display. Default: empirical + fitted CDF.</summary>
    Public Property Mode As CumulativeDistributionPlotMode =
        CumulativeDistributionPlotMode.EmpiricalWithFittedDistribution

    ''' <summary>Distribution used for the fitted CDF. Default: Normal.</summary>
    Public Property Distribution As CumulativeDistributionKind = CumulativeDistributionKind.Normal

    ''' <summary>Y-axis scaling. Default: Percent (0 to 100).</summary>
    Public Property YScale As CumulativeDistributionYScale = CumulativeDistributionYScale.Percent

    ''' <summary>
    ''' Empirical probability convention. ExactEcdf gives the mathematical ECDF and reaches 1.
    ''' MinitabMedianRank reproduces the plotting-probability convention used by Minitab.
    ''' Default: ExactEcdf.
    ''' </summary>
    Public Property EmpiricalMethod As CumulativeDistributionEmpiricalMethod =
        CumulativeDistributionEmpiricalMethod.ExactEcdf

    ''' <summary>
    ''' Number of points used for the smooth fitted CDF. Accepted range: 50 to 2000.
    ''' Default: 241.
    ''' </summary>
    Public Property FittedCurvePointCount As Integer = 241

    ''' <summary>
    ''' Optional sample percentile reference lines, expressed on a 0-100 scale.
    ''' For example {25, 50, 75}. Values must be strictly between 0 and 100.
    ''' The empirical quantile is the generalized inverse ECDF: the smallest x for
    ''' which F_n(x) is at least the requested probability.
    ''' </summary>
    Public Property Percentiles As Double() = Array.Empty(Of Double)()

    ''' <summary>
    ''' Optional explicit x-axis minimum. When Nothing, a renderer-friendly automatic
    ''' bound is prepared from the sample and fitted distribution.
    ''' </summary>
    Public Property XMinimum As Nullable(Of Double) = Nothing

    ''' <summary>
    ''' Optional explicit x-axis maximum. When Nothing, a renderer-friendly automatic
    ''' bound is prepared from the sample and fitted distribution.
    ''' </summary>
    Public Property XMaximum As Nullable(Of Double) = Nothing

    ''' <summary>
    ''' When False (default), a missing/non-numeric/non-finite observation causes validation
    ''' to fail. When True, such observations are omitted and counted in the result.
    ''' </summary>
    Public Property OmitInvalidRows As Boolean = False

    ''' <summary>
    ''' Maximum Nelder-Mead iterations used by distributions that require numerical
    ''' parameter estimation. Default: 2000.
    ''' </summary>
    Public Property MaximumFitIterations As Integer = 2000

    ''' <summary>
    ''' Relative convergence tolerance used by the numerical distribution fitter.
    ''' Default: 1E-9.
    ''' </summary>
    Public Property FitTolerance As Double = 1.0E-9R

    Friend Function Copy() As CumulativeDistributionPlotOptions
        Return New CumulativeDistributionPlotOptions With {
            .Mode = Mode,
            .Distribution = Distribution,
            .YScale = YScale,
            .EmpiricalMethod = EmpiricalMethod,
            .FittedCurvePointCount = FittedCurvePointCount,
            .Percentiles = If(Percentiles Is Nothing,
                              Array.Empty(Of Double)(),
                              DirectCast(Percentiles.Clone(), Double())),
            .XMinimum = XMinimum,
            .XMaximum = XMaximum,
            .OmitInvalidRows = OmitInvalidRows,
            .MaximumFitIterations = MaximumFitIterations,
            .FitTolerance = FitTolerance
        }
    End Function
End Class

''' <summary>
''' One requested sample percentile and its x-coordinate.
''' </summary>
Public NotInheritable Class CumulativeDistributionPercentile
    Private ReadOnly _percent As Double
    Private ReadOnly _probability As Double
    Private ReadOnly _xValue As Double
    Private ReadOnly _yValue As Double

    Friend Sub New(percent As Double, xValue As Double, yValue As Double)
        _percent = percent
        _probability = percent / 100.0R
        _xValue = xValue
        _yValue = yValue
    End Sub

    Public ReadOnly Property Percent As Double
        Get
            Return _percent
        End Get
    End Property

    Public ReadOnly Property Probability As Double
        Get
            Return _probability
        End Get
    End Property

    Public ReadOnly Property XValue As Double
        Get
            Return _xValue
        End Get
    End Property

    ''' <summary>Reference-line Y coordinate already transformed to the requested Y scale.</summary>
    Public ReadOnly Property YValue As Double
        Get
            Return _yValue
        End Get
    End Property
End Class

''' <summary>
''' Parameter estimates for one fitted theoretical distribution.
''' </summary>
Public NotInheritable Class CumulativeDistributionFit
    Private ReadOnly _distribution As CumulativeDistributionKind
    Private ReadOnly _parameterNames() As String
    Private ReadOnly _parameterValues() As Double
    Private ReadOnly _succeeded As Boolean
    Private ReadOnly _converged As Boolean
    Private ReadOnly _iterations As Integer
    Private ReadOnly _logLikelihood As Double
    Private ReadOnly _message As String

    Friend Sub New(distribution As CumulativeDistributionKind,
                   parameterNames() As String,
                   parameterValues() As Double,
                   succeeded As Boolean,
                   converged As Boolean,
                   iterations As Integer,
                   logLikelihood As Double,
                   message As String)
        _distribution = distribution
        _parameterNames = If(parameterNames Is Nothing,
                             Array.Empty(Of String)(),
                             DirectCast(parameterNames.Clone(), String()))
        _parameterValues = If(parameterValues Is Nothing,
                              Array.Empty(Of Double)(),
                              DirectCast(parameterValues.Clone(), Double()))
        _succeeded = succeeded
        _converged = converged
        _iterations = iterations
        _logLikelihood = logLikelihood
        _message = If(message, String.Empty)
    End Sub

    Public ReadOnly Property Distribution As CumulativeDistributionKind
        Get
            Return _distribution
        End Get
    End Property

    Public ReadOnly Property ParameterNames As String()
        Get
            Return DirectCast(_parameterNames.Clone(), String())
        End Get
    End Property

    Public ReadOnly Property ParameterValues As Double()
        Get
            Return DirectCast(_parameterValues.Clone(), Double())
        End Get
    End Property

    Public ReadOnly Property Succeeded As Boolean
        Get
            Return _succeeded
        End Get
    End Property

    Public ReadOnly Property Converged As Boolean
        Get
            Return _converged
        End Get
    End Property

    Public ReadOnly Property Iterations As Integer
        Get
            Return _iterations
        End Get
    End Property

    Public ReadOnly Property LogLikelihood As Double
        Get
            Return _logLikelihood
        End Get
    End Property

    Public ReadOnly Property Message As String
        Get
            Return _message
        End Get
    End Property

    Public Function TryGetParameter(name As String, ByRef value As Double) As Boolean
        If String.IsNullOrWhiteSpace(name) Then Return False
        For i As Integer = 0 To _parameterNames.Length - 1
            If String.Equals(_parameterNames(i), name, StringComparison.OrdinalIgnoreCase) Then
                value = _parameterValues(i)
                Return True
            End If
        Next
        Return False
    End Function
End Class

''' <summary>
''' Immutable calculation result for one sample/group.
''' All arrays are renderer-ready and contain no Excel Interop types.
''' </summary>
Public NotInheritable Class CumulativeDistributionPlotResult
    Private ReadOnly _seriesName As String
    Private ReadOnly _sourceCount As Integer
    Private ReadOnly _validCount As Integer
    Private ReadOnly _omittedCount As Integer
    Private ReadOnly _sortedData() As Double
    Private ReadOnly _empiricalX() As Double
    Private ReadOnly _empiricalY() As Double
    Private ReadOnly _fittedX() As Double
    Private ReadOnly _fittedY() As Double
    Private ReadOnly _percentiles() As CumulativeDistributionPercentile
    Private ReadOnly _fit As CumulativeDistributionFit
    Private ReadOnly _minimum As Double
    Private ReadOnly _maximum As Double
    Private ReadOnly _mean As Double
    Private ReadOnly _sampleStandardDeviation As Double
    Private ReadOnly _xMinimum As Double
    Private ReadOnly _xMaximum As Double
    Private ReadOnly _options As CumulativeDistributionPlotOptions

    Friend Sub New(seriesName As String,
                   sourceCount As Integer,
                   omittedCount As Integer,
                   sortedData() As Double,
                   empiricalX() As Double,
                   empiricalY() As Double,
                   fittedX() As Double,
                   fittedY() As Double,
                   percentiles() As CumulativeDistributionPercentile,
                   fit As CumulativeDistributionFit,
                   minimum As Double,
                   maximum As Double,
                   mean As Double,
                   sampleStandardDeviation As Double,
                   xMinimum As Double,
                   xMaximum As Double,
                   options As CumulativeDistributionPlotOptions)
        _seriesName = If(seriesName, String.Empty)
        _sourceCount = sourceCount
        _validCount = sortedData.Length
        _omittedCount = omittedCount
        _sortedData = DirectCast(sortedData.Clone(), Double())
        _empiricalX = DirectCast(empiricalX.Clone(), Double())
        _empiricalY = DirectCast(empiricalY.Clone(), Double())
        _fittedX = DirectCast(fittedX.Clone(), Double())
        _fittedY = DirectCast(fittedY.Clone(), Double())
        _percentiles = DirectCast(percentiles.Clone(), CumulativeDistributionPercentile())
        _fit = fit
        _minimum = minimum
        _maximum = maximum
        _mean = mean
        _sampleStandardDeviation = sampleStandardDeviation
        _xMinimum = xMinimum
        _xMaximum = xMaximum
        _options = options.Copy()
    End Sub

    Public ReadOnly Property SeriesName As String
        Get
            Return _seriesName
        End Get
    End Property

    Public ReadOnly Property SourceCount As Integer
        Get
            Return _sourceCount
        End Get
    End Property

    Public ReadOnly Property ValidCount As Integer
        Get
            Return _validCount
        End Get
    End Property

    Public ReadOnly Property OmittedCount As Integer
        Get
            Return _omittedCount
        End Get
    End Property

    Public ReadOnly Property SortedData As Double()
        Get
            Return DirectCast(_sortedData.Clone(), Double())
        End Get
    End Property

    ''' <summary>Expanded X coordinates for an XY-scatter step line.</summary>
    Public ReadOnly Property EmpiricalX As Double()
        Get
            Return DirectCast(_empiricalX.Clone(), Double())
        End Get
    End Property

    ''' <summary>Expanded ECDF Y coordinates, already converted to probability or percent.</summary>
    Public ReadOnly Property EmpiricalY As Double()
        Get
            Return DirectCast(_empiricalY.Clone(), Double())
        End Get
    End Property

    Public ReadOnly Property FittedX As Double()
        Get
            Return DirectCast(_fittedX.Clone(), Double())
        End Get
    End Property

    ''' <summary>Fitted theoretical CDF values, already converted to probability or percent.</summary>
    Public ReadOnly Property FittedY As Double()
        Get
            Return DirectCast(_fittedY.Clone(), Double())
        End Get
    End Property

    Public ReadOnly Property Percentiles As CumulativeDistributionPercentile()
        Get
            Return DirectCast(_percentiles.Clone(), CumulativeDistributionPercentile())
        End Get
    End Property

    Public ReadOnly Property Fit As CumulativeDistributionFit
        Get
            Return _fit
        End Get
    End Property

    Public ReadOnly Property Minimum As Double
        Get
            Return _minimum
        End Get
    End Property

    Public ReadOnly Property Maximum As Double
        Get
            Return _maximum
        End Get
    End Property

    Public ReadOnly Property Mean As Double
        Get
            Return _mean
        End Get
    End Property

    Public ReadOnly Property SampleStandardDeviation As Double
        Get
            Return _sampleStandardDeviation
        End Get
    End Property

    Public ReadOnly Property XMinimum As Double
        Get
            Return _xMinimum
        End Get
    End Property

    Public ReadOnly Property XMaximum As Double
        Get
            Return _xMaximum
        End Get
    End Property

    Public ReadOnly Property Options As CumulativeDistributionPlotOptions
        Get
            Return _options.Copy()
        End Get
    End Property
End Class

''' <summary>
''' Result of splitting one numeric variable by a grouping variable.
''' </summary>
Public NotInheritable Class CumulativeDistributionPlotSetResult
    Private ReadOnly _series() As CumulativeDistributionPlotResult

    Friend Sub New(series() As CumulativeDistributionPlotResult)
        _series = DirectCast(series.Clone(), CumulativeDistributionPlotResult())
    End Sub

    Public ReadOnly Property Series As CumulativeDistributionPlotResult()
        Get
            Return DirectCast(_series.Clone(), CumulativeDistributionPlotResult())
        End Get
    End Property

    Public ReadOnly Property Count As Integer
        Get
            Return _series.Length
        End Get
    End Property
End Class

''' <summary>
''' Excel-independent ECDF and fitted-CDF preparation engine.
'''
''' ECDF coordinates are returned as a true right-continuous step function with ties
''' collapsed to one jump. Fitted lines support the same 14 continuous distribution
''' families that Minitab exposes for fitted empirical-CDF lines.
''' </summary>
Public NotInheritable Class CumulativeDistributionPlot
    Private Const EulerGamma As Double = 0.57721566490153287R
    Private Const LogTwoPi As Double = 1.8378770664093453R
    Private Const HugeObjective As Double = 1.0E+290R

    Private Sub New()
    End Sub

    ''' <summary>
    ''' Computes ECDF coordinates, optional fitted distribution coordinates, sample
    ''' percentiles and renderer-friendly x-axis bounds for one sample.
    ''' </summary>
    Public Shared Function Compute(values As Array,
                                   Optional seriesName As String = "",
                                   Optional options As CumulativeDistributionPlotOptions = Nothing) As CumulativeDistributionPlotResult
        If values Is Nothing Then Throw New ArgumentNullException(NameOf(values))
        EnsureVectorShape(values, NameOf(values))

        Dim resolved As CumulativeDistributionPlotOptions = ResolveOptions(options)
        Dim omitted As Integer = 0
        Dim data As Double() = ConvertNumericVector(values, resolved.OmitInvalidRows, omitted, NameOf(values))

        If data.Length = 0 Then
            Throw New ArgumentException("No finite numeric observations are available for the cumulative distribution plot.", NameOf(values))
        End If

        Array.Sort(data)

        Dim n As Integer = data.Length
        Dim minimum As Double = data(0)
        Dim maximum As Double = data(n - 1)
        Dim mean As Double = ComputeMean(data)
        Dim sampleSd As Double = ComputeSampleStandardDeviation(data, mean)

        Dim empiricalX() As Double = Array.Empty(Of Double)()
        Dim empiricalY() As Double = Array.Empty(Of Double)()
        If resolved.Mode <> CumulativeDistributionPlotMode.FittedDistributionOnly Then
            BuildEmpiricalStep(data,
                               resolved.YScale,
                               resolved.EmpiricalMethod,
                               empiricalX,
                               empiricalY)
        End If

        Dim fit As CumulativeDistributionFit = Nothing
        If resolved.Mode <> CumulativeDistributionPlotMode.EmpiricalOnly Then
            fit = FitDistribution(data,
                                  resolved.Distribution,
                                  resolved.MaximumFitIterations,
                                  resolved.FitTolerance)
        End If

        Dim xMinimum As Double
        Dim xMaximum As Double
        DetermineXBounds(data, fit, resolved, xMinimum, xMaximum)

        Dim fittedX() As Double = Array.Empty(Of Double)()
        Dim fittedY() As Double = Array.Empty(Of Double)()
        If fit IsNot Nothing AndAlso fit.Succeeded Then
            BuildFittedCurve(fit,
                             xMinimum,
                             xMaximum,
                             resolved.FittedCurvePointCount,
                             resolved.YScale,
                             fittedX,
                             fittedY)
        End If

        Dim percentileResults() As CumulativeDistributionPercentile =
            BuildPercentiles(data, resolved.Percentiles, resolved.YScale)

        Return New CumulativeDistributionPlotResult(seriesName,
                                                    values.Length,
                                                    omitted,
                                                    data,
                                                    empiricalX,
                                                    empiricalY,
                                                    fittedX,
                                                    fittedY,
                                                    percentileResults,
                                                    fit,
                                                    minimum,
                                                    maximum,
                                                    mean,
                                                    sampleSd,
                                                    xMinimum,
                                                    xMaximum,
                                                    resolved)
    End Function

    ''' <summary>
    ''' Splits one numeric vector by a text or numeric grouping vector and computes one
    ''' independent ECDF/CDF result per group, preserving order of first appearance.
    ''' This is intended for a future Excel renderer that can overlay groups or draw them
    ''' as separate charts without duplicating grouping logic in the UI.
    ''' </summary>
    Public Shared Function ComputeGrouped(values As Array,
                                          groupingValues As Array,
                                          Optional variableName As String = "",
                                          Optional options As CumulativeDistributionPlotOptions = Nothing) As CumulativeDistributionPlotSetResult
        If values Is Nothing Then Throw New ArgumentNullException(NameOf(values))
        If groupingValues Is Nothing Then Throw New ArgumentNullException(NameOf(groupingValues))
        EnsureVectorShape(values, NameOf(values))
        EnsureVectorShape(groupingValues, NameOf(groupingValues))
        If values.Length <> groupingValues.Length Then
            Throw New ArgumentException("Values and grouping variable must contain the same number of rows.")
        End If

        Dim resolved As CumulativeDistributionPlotOptions = ResolveOptions(options)
        Dim order As New List(Of String)()
        Dim labels As New Dictionary(Of String, String)(StringComparer.Ordinal)
        Dim groups As New Dictionary(Of String, List(Of Double))(StringComparer.Ordinal)

        For i As Integer = 0 To values.Length - 1
            Dim groupValue As Object = GetVectorValue(groupingValues, i)
            Dim numericValue As Double

            If IsMissing(groupValue) OrElse Not TryConvertNumeric(GetVectorValue(values, i), numericValue) Then
                If resolved.OmitInvalidRows Then Continue For
                Throw New ArgumentException("Missing/invalid value or grouping level at row " &
                                            (i + 1).ToString(CultureInfo.CurrentCulture) & ".")
            End If

            Dim key As String = BuildGroupKey(groupValue)
            Dim label As String = FormatGroupLabel(groupValue)

            If Not groups.ContainsKey(key) Then
                groups.Add(key, New List(Of Double)())
                labels.Add(key, label)
                order.Add(key)
            End If
            groups(key).Add(numericValue)
        Next

        If order.Count = 0 Then
            Throw New ArgumentException("No complete rows are available after applying the grouping variable.")
        End If

        Dim results(order.Count - 1) As CumulativeDistributionPlotResult
        For i As Integer = 0 To order.Count - 1
            Dim key As String = order(i)
            Dim groupLabel As String = labels(key)
            Dim name As String = If(String.IsNullOrWhiteSpace(variableName),
                                    groupLabel,
                                    variableName & " - " & groupLabel)
            Dim groupData As Double() = groups(key).ToArray()
            results(i) = Compute(groupData, name, resolved)
        Next

        Return New CumulativeDistributionPlotSetResult(results)
    End Function

    ''' <summary>
    ''' Evaluates a fitted CDF at x. Returns NaN when the fit is unavailable.
    ''' </summary>
    Public Shared Function EvaluateFittedCdf(x As Double, fit As CumulativeDistributionFit) As Double
        If fit Is Nothing OrElse Not fit.Succeeded Then Return Double.NaN
        If Not IsFinite(x) Then Return Double.NaN
        Return EvaluateCdf(x, fit.Distribution, fit.ParameterValues)
    End Function

    Private Shared Function ResolveOptions(options As CumulativeDistributionPlotOptions) As CumulativeDistributionPlotOptions
        Dim resolved As CumulativeDistributionPlotOptions = If(options Is Nothing,
                                                               New CumulativeDistributionPlotOptions(),
                                                               options.Copy())

        If resolved.FittedCurvePointCount < 50 OrElse resolved.FittedCurvePointCount > 2000 Then
            Throw New ArgumentOutOfRangeException(NameOf(options), "FittedCurvePointCount must be between 50 and 2000.")
        End If
        If resolved.MaximumFitIterations < 100 OrElse resolved.MaximumFitIterations > 100000 Then
            Throw New ArgumentOutOfRangeException(NameOf(options), "MaximumFitIterations must be between 100 and 100000.")
        End If
        If Not IsFinite(resolved.FitTolerance) OrElse resolved.FitTolerance <= 0.0R OrElse resolved.FitTolerance > 0.01R Then
            Throw New ArgumentOutOfRangeException(NameOf(options), "FitTolerance must be finite, positive, and no greater than 0.01.")
        End If

        If resolved.Percentiles Is Nothing Then resolved.Percentiles = Array.Empty(Of Double)()
        Dim cleanPercentiles As New List(Of Double)()
        For Each p As Double In resolved.Percentiles
            If Not IsFinite(p) OrElse p <= 0.0R OrElse p >= 100.0R Then
                Throw New ArgumentOutOfRangeException(NameOf(options), "Percentiles must be finite and strictly between 0 and 100.")
            End If
            If Not cleanPercentiles.Any(Function(x) Math.Abs(x - p) <= 1.0E-12R) Then cleanPercentiles.Add(p)
        Next
        cleanPercentiles.Sort()
        resolved.Percentiles = cleanPercentiles.ToArray()

        If resolved.XMinimum.HasValue AndAlso Not IsFinite(resolved.XMinimum.Value) Then
            Throw New ArgumentOutOfRangeException(NameOf(options), "XMinimum must be finite when supplied.")
        End If
        If resolved.XMaximum.HasValue AndAlso Not IsFinite(resolved.XMaximum.Value) Then
            Throw New ArgumentOutOfRangeException(NameOf(options), "XMaximum must be finite when supplied.")
        End If
        If resolved.XMinimum.HasValue AndAlso resolved.XMaximum.HasValue AndAlso
           resolved.XMinimum.Value >= resolved.XMaximum.Value Then
            Throw New ArgumentException("XMinimum must be smaller than XMaximum.", NameOf(options))
        End If

        Return resolved
    End Function

    Private Shared Function ConvertNumericVector(source As Array,
                                                 omitInvalid As Boolean,
                                                 ByRef omittedCount As Integer,
                                                 parameterName As String) As Double()
        Dim output As New List(Of Double)(source.Length)
        omittedCount = 0

        For i As Integer = 0 To source.Length - 1
            Dim value As Double
            If TryConvertNumeric(GetVectorValue(source, i), value) Then
                output.Add(value)
            ElseIf omitInvalid Then
                omittedCount += 1
            Else
                Throw New ArgumentException("Row " & (i + 1).ToString(CultureInfo.CurrentCulture) &
                                            " is missing, non-numeric, or non-finite.", parameterName)
            End If
        Next

        Return output.ToArray()
    End Function

    Private Shared Sub EnsureVectorShape(source As Array, parameterName As String)
        If source Is Nothing Then Throw New ArgumentNullException(parameterName)
        If source.Rank = 1 Then Return
        If source.Rank = 2 AndAlso (source.GetLength(0) = 1 OrElse source.GetLength(1) = 1) Then Return
        Throw New ArgumentException("Input must be a one-dimensional array or a single-row/single-column two-dimensional array.", parameterName)
    End Sub

    Private Shared Function GetVectorValue(source As Array, index As Integer) As Object
        If source.Rank = 1 Then Return source.GetValue(index)
        If source.GetLength(1) = 1 Then Return source.GetValue(index, 0)
        Return source.GetValue(0, index)
    End Function

    Private Shared Function TryConvertNumeric(value As Object, ByRef result As Double) As Boolean
        result = Double.NaN
        If IsMissing(value) Then Return False

        Try
            If TypeOf value Is DateTime Then
                result = DirectCast(value, DateTime).ToOADate()
            Else
                result = Convert.ToDouble(value, CultureInfo.InvariantCulture)
            End If
        Catch
            Return False
        End Try

        Return IsFinite(result)
    End Function

    Private Shared Function IsMissing(value As Object) As Boolean
        If value Is Nothing OrElse Convert.IsDBNull(value) Then Return True
        If TypeOf value Is String Then Return String.IsNullOrWhiteSpace(DirectCast(value, String))
        Return False
    End Function

    Private Shared Function IsFinite(value As Double) As Boolean
        Return Not Double.IsNaN(value) AndAlso Not Double.IsInfinity(value)
    End Function

    Private Shared Function ComputeMean(data() As Double) As Double
        Dim sum As Double = 0.0R
        For Each x As Double In data
            sum += x
        Next
        Return sum / CDbl(data.Length)
    End Function

    Private Shared Function ComputeSampleStandardDeviation(data() As Double, mean As Double) As Double
        If data.Length < 2 Then Return Double.NaN
        Dim ss As Double = 0.0R
        For Each x As Double In data
            Dim d As Double = x - mean
            ss += d * d
        Next
        Return Math.Sqrt(ss / CDbl(data.Length - 1))
    End Function

    Private Shared Function ComputePopulationStandardDeviation(data() As Double, mean As Double) As Double
        If data.Length = 0 Then Return Double.NaN
        Dim ss As Double = 0.0R
        For Each x As Double In data
            Dim d As Double = x - mean
            ss += d * d
        Next
        Return Math.Sqrt(ss / CDbl(data.Length))
    End Function

    Private Shared Sub BuildEmpiricalStep(sortedData() As Double,
                                          yScale As CumulativeDistributionYScale,
                                          empiricalMethod As CumulativeDistributionEmpiricalMethod,
                                          ByRef xOut() As Double,
                                          ByRef yOut() As Double)
        Dim xs As New List(Of Double)(2 * sortedData.Length)
        Dim ys As New List(Of Double)(2 * sortedData.Length)
        Dim n As Integer = sortedData.Length
        Dim i As Integer = 0
        Dim cumulative As Integer = 0
        Dim previousProbability As Double = 0.0R

        While i < n
            Dim x As Double = sortedData(i)
            Dim j As Integer = i + 1
            While j < n AndAlso sortedData(j) = x
                j += 1
            End While

            xs.Add(x)
            ys.Add(ScaleProbability(previousProbability, yScale))

            cumulative += (j - i)
            Dim currentProbability As Double
            If empiricalMethod = CumulativeDistributionEmpiricalMethod.MinitabMedianRank Then
                currentProbability = (CDbl(cumulative) - 0.3R) / (CDbl(n) + 0.4R)
            Else
                currentProbability = CDbl(cumulative) / CDbl(n)
            End If
            currentProbability = Clamp(currentProbability, 0.0R, 1.0R)
            xs.Add(x)
            ys.Add(ScaleProbability(currentProbability, yScale))

            previousProbability = currentProbability
            i = j
        End While

        xOut = xs.ToArray()
        yOut = ys.ToArray()
    End Sub

    Private Shared Function BuildPercentiles(sortedData() As Double,
                                             requestedPercentiles() As Double,
                                             yScale As CumulativeDistributionYScale) As CumulativeDistributionPercentile()
        If requestedPercentiles Is Nothing OrElse requestedPercentiles.Length = 0 Then
            Return Array.Empty(Of CumulativeDistributionPercentile)()
        End If

        Dim output(requestedPercentiles.Length - 1) As CumulativeDistributionPercentile
        For i As Integer = 0 To requestedPercentiles.Length - 1
            Dim percent As Double = requestedPercentiles(i)
            Dim probability As Double = percent / 100.0R
            Dim rank As Integer = CInt(Math.Ceiling(probability * sortedData.Length))
            If rank < 1 Then rank = 1
            If rank > sortedData.Length Then rank = sortedData.Length
            Dim x As Double = sortedData(rank - 1)
            output(i) = New CumulativeDistributionPercentile(percent,
                                                              x,
                                                              ScaleProbability(probability, yScale))
        Next
        Return output
    End Function

    Private Shared Function ScaleProbability(probability As Double,
                                             yScale As CumulativeDistributionYScale) As Double
        Dim p As Double = Clamp(probability, 0.0R, 1.0R)
        If yScale = CumulativeDistributionYScale.Percent Then Return 100.0R * p
        Return p
    End Function

    Private Shared Sub DetermineXBounds(data() As Double,
                                       fit As CumulativeDistributionFit,
                                       options As CumulativeDistributionPlotOptions,
                                       ByRef xMinimum As Double,
                                       ByRef xMaximum As Double)
        Dim dataMin As Double = data(0)
        Dim dataMax As Double = data(data.Length - 1)
        Dim range As Double = dataMax - dataMin
        If range <= 0.0R OrElse Not IsFinite(range) Then
            range = Math.Max(1.0R, Math.Abs(dataMin) * 0.2R)
        End If

        xMinimum = dataMin - 0.08R * range
        xMaximum = dataMax + 0.08R * range

        If fit IsNot Nothing AndAlso fit.Succeeded Then
            Dim threshold As Double
            If TryGetThreshold(fit, threshold) Then
                xMinimum = Math.Min(xMinimum, threshold)
            ElseIf HasZeroLowerSupport(fit.Distribution) Then
                xMinimum = Math.Max(0.0R, Math.Min(xMinimum, dataMin * 0.9R))
            End If

            ExpandBoundsForFittedCdf(fit, dataMin, dataMax, xMinimum, xMaximum)
        End If

        If options.XMinimum.HasValue Then xMinimum = options.XMinimum.Value
        If options.XMaximum.HasValue Then xMaximum = options.XMaximum.Value

        If xMaximum <= xMinimum Then
            xMinimum = dataMin - 0.5R
            xMaximum = dataMax + 0.5R
            If xMaximum <= xMinimum Then xMaximum = xMinimum + 1.0R
        End If
    End Sub

    Private Shared Sub ExpandBoundsForFittedCdf(fit As CumulativeDistributionFit,
                                                     dataMinimum As Double,
                                                     dataMaximum As Double,
                                                     ByRef xMinimum As Double,
                                                     ByRef xMaximum As Double)
        Dim span As Double = Math.Max(dataMaximum - dataMinimum,
                                      Math.Max(Math.Abs(dataMinimum) * 0.1R, 1.0R))
        Dim lowerStep As Double = 0.25R * span
        Dim upperStep As Double = 0.25R * span

        Dim lowerSupport As Double = Double.NegativeInfinity
        Dim threshold As Double
        If TryGetThreshold(fit, threshold) Then
            lowerSupport = threshold
        ElseIf HasZeroLowerSupport(fit.Distribution) Then
            lowerSupport = 0.0R
        End If

        For i As Integer = 0 To 29
            Dim p As Double = EvaluateCdf(xMinimum, fit.Distribution, fit.ParameterValues)
            If IsFinite(p) AndAlso p <= 0.002R Then Exit For

            Dim candidate As Double = xMinimum - lowerStep
            If IsFinite(lowerSupport) Then candidate = Math.Max(lowerSupport, candidate)
            If candidate >= xMinimum Then Exit For
            xMinimum = candidate
            lowerStep *= 1.35R
        Next

        For i As Integer = 0 To 29
            Dim p As Double = EvaluateCdf(xMaximum, fit.Distribution, fit.ParameterValues)
            If IsFinite(p) AndAlso p >= 0.998R Then Exit For
            xMaximum += upperStep
            upperStep *= 1.35R
            If Not IsFinite(xMaximum) Then
                xMaximum = dataMaximum + 10.0R * span
                Exit For
            End If
        Next
    End Sub

    Private Shared Sub BuildFittedCurve(fit As CumulativeDistributionFit,
                                        xMinimum As Double,
                                        xMaximum As Double,
                                        pointCount As Integer,
                                        yScale As CumulativeDistributionYScale,
                                        ByRef xOut() As Double,
                                        ByRef yOut() As Double)
        ReDim xOut(pointCount - 1)
        ReDim yOut(pointCount - 1)
        Dim stepSize As Double = (xMaximum - xMinimum) / CDbl(pointCount - 1)

        For i As Integer = 0 To pointCount - 1
            Dim x As Double = If(i = pointCount - 1,
                                 xMaximum,
                                 xMinimum + stepSize * CDbl(i))
            Dim p As Double = EvaluateCdf(x, fit.Distribution, fit.ParameterValues)
            xOut(i) = x
            yOut(i) = ScaleProbability(p, yScale)
        Next
    End Sub

    Private Shared Function FitDistribution(data() As Double,
                                            kind As CumulativeDistributionKind,
                                            maximumIterations As Integer,
                                            tolerance As Double) As CumulativeDistributionFit
        If data.Length < 2 Then
            Return FailedFit(kind, "At least two observations are required to fit a theoretical distribution.")
        End If

        Dim domainMessage As String = ValidateDistributionDomain(data, kind)
        If domainMessage.Length > 0 Then Return FailedFit(kind, domainMessage)

        Select Case kind
            Case CumulativeDistributionKind.Normal
                Return FitNormal(data)
            Case CumulativeDistributionKind.Lognormal
                Return FitLognormal(data)
            Case CumulativeDistributionKind.Exponential
                Return FitExponential(data)
            Case CumulativeDistributionKind.TwoParameterExponential
                Return FitTwoParameterExponential(data)
            Case Else
                Return FitByMaximumLikelihood(data, kind, maximumIterations, tolerance)
        End Select
    End Function

    Private Shared Function FitNormal(data() As Double) As CumulativeDistributionFit
        Dim mean As Double = ComputeMean(data)
        'Minitab's fitted empirical-CDF normal line is described in terms of the
        'sample mean and standard deviation. Keep the same familiar sample-SD estimate.
        Dim sd As Double = ComputeSampleStandardDeviation(data, mean)
        If Not IsFinite(sd) OrElse sd <= 0.0R Then
            Return FailedFit(CumulativeDistributionKind.Normal,
                             "A normal distribution cannot be fitted because the sample has zero variability.")
        End If

        Dim parameters() As Double = {mean, sd}
        Return SuccessfulClosedFormFit(CumulativeDistributionKind.Normal,
                                       {"Mean", "Standard deviation"},
                                       parameters,
                                       ComputeLogLikelihood(data, CumulativeDistributionKind.Normal, parameters))
    End Function

    Private Shared Function FitLognormal(data() As Double) As CumulativeDistributionFit
        Dim logData(data.Length - 1) As Double
        For i As Integer = 0 To data.Length - 1
            logData(i) = Math.Log(data(i))
        Next

        Dim location As Double = ComputeMean(logData)
        Dim scale As Double = ComputeSampleStandardDeviation(logData, location)
        If Not IsFinite(scale) OrElse scale <= 0.0R Then
            Return FailedFit(CumulativeDistributionKind.Lognormal,
                             "A lognormal distribution cannot be fitted because log(data) has zero variability.")
        End If

        Dim parameters() As Double = {location, scale}
        Return SuccessfulClosedFormFit(CumulativeDistributionKind.Lognormal,
                                       {"Location", "Scale"},
                                       parameters,
                                       ComputeLogLikelihood(data, CumulativeDistributionKind.Lognormal, parameters))
    End Function

    Private Shared Function FitExponential(data() As Double) As CumulativeDistributionFit
        Dim scale As Double = ComputeMean(data)
        If Not IsFinite(scale) OrElse scale <= 0.0R Then
            Return FailedFit(CumulativeDistributionKind.Exponential,
                             "The exponential scale estimate must be positive.")
        End If

        Dim parameters() As Double = {scale}
        Return SuccessfulClosedFormFit(CumulativeDistributionKind.Exponential,
                                       {"Scale"},
                                       parameters,
                                       ComputeLogLikelihood(data, CumulativeDistributionKind.Exponential, parameters))
    End Function

    Private Shared Function FitTwoParameterExponential(data() As Double) As CumulativeDistributionFit
        Dim threshold As Double = data.Min()
        Dim sumShifted As Double = 0.0R
        For Each x As Double In data
            sumShifted += x - threshold
        Next
        Dim scale As Double = sumShifted / CDbl(data.Length)

        If Not IsFinite(scale) OrElse scale <= 0.0R Then
            Return FailedFit(CumulativeDistributionKind.TwoParameterExponential,
                             "The two-parameter exponential distribution cannot be fitted to a constant sample.")
        End If

        Dim parameters() As Double = {scale, threshold}
        Return New CumulativeDistributionFit(CumulativeDistributionKind.TwoParameterExponential,
                                             {"Scale", "Threshold"},
                                             parameters,
                                             True,
                                             True,
                                             0,
                                             ComputeLogLikelihood(data,
                                                                  CumulativeDistributionKind.TwoParameterExponential,
                                                                  parameters),
                                             "Threshold estimated at the smallest observation (the maximum-likelihood boundary solution).")
    End Function

    Private Shared Function FitByMaximumLikelihood(data() As Double,
                                                   kind As CumulativeDistributionKind,
                                                   maximumIterations As Integer,
                                                   tolerance As Double) As CumulativeDistributionFit
        Dim starts As List(Of Double()) = BuildStartingValues(data, kind)
        If starts.Count = 0 Then Return FailedFit(kind, "Unable to construct valid starting values for this distribution.")

        Dim best As OptimizationResult = Nothing
        For Each start As Double() In starts
            Dim steps As Double() = BuildInitialSteps(start, kind)
            Dim candidate As OptimizationResult = NelderMead(
                Function(theta As Double()) NegativeLogLikelihoodTheta(data, kind, theta),
                start,
                steps,
                maximumIterations,
                tolerance)

            If candidate IsNot Nothing AndAlso IsFinite(candidate.Objective) AndAlso candidate.Objective < HugeObjective Then
                If best Is Nothing OrElse candidate.Objective < best.Objective Then best = candidate
            End If
        Next

        If best Is Nothing Then Return FailedFit(kind, "Numerical parameter estimation failed to find a finite likelihood.")

        Dim parameters() As Double = DecodeTheta(kind, best.Theta, data.Min())
        If parameters Is Nothing OrElse Not ParametersAreValid(kind, parameters) Then
            Return FailedFit(kind, "Numerical parameter estimation returned invalid distribution parameters.")
        End If

        Dim parameterNames() As String = GetParameterNames(kind)
        Dim message As String = If(best.Converged,
                                   "Maximum-likelihood fit converged.",
                                   "Maximum-likelihood fit returned the best finite iterate but did not satisfy the convergence tolerance.")

        If HasThreshold(kind) Then
            Dim threshold As Double = parameters(parameters.Length - 1)
            Dim range As Double = Math.Max(data.Max() - data.Min(), 1.0R)
            Dim gap As Double = data.Min() - threshold
            If gap <= 1.0E-8R * range Then
                message &= " The threshold estimate is very close to the smallest observation; threshold-family likelihoods can have boundary solutions."
            End If
        End If

        Return New CumulativeDistributionFit(kind,
                                             parameterNames,
                                             parameters,
                                             True,
                                             best.Converged,
                                             best.Iterations,
                                             -best.Objective,
                                             message)
    End Function

    Private Shared Function ValidateDistributionDomain(data() As Double,
                                                       kind As CumulativeDistributionKind) As String
        Select Case kind
            Case CumulativeDistributionKind.Lognormal,
                 CumulativeDistributionKind.Gamma,
                 CumulativeDistributionKind.Weibull,
                 CumulativeDistributionKind.Loglogistic
                For Each x As Double In data
                    If x <= 0.0R Then
                        Return DistributionDisplayName(kind) & " requires all observations to be greater than zero."
                    End If
                Next

            Case CumulativeDistributionKind.Exponential
                For Each x As Double In data
                    If x < 0.0R Then
                        Return "Exponential distribution requires all observations to be non-negative."
                    End If
                Next
        End Select
        Return String.Empty
    End Function

    Private Shared Function SuccessfulClosedFormFit(kind As CumulativeDistributionKind,
                                                    parameterNames() As String,
                                                    parameters() As Double,
                                                    logLikelihood As Double) As CumulativeDistributionFit
        Return New CumulativeDistributionFit(kind,
                                             parameterNames,
                                             parameters,
                                             True,
                                             True,
                                             0,
                                             logLikelihood,
                                             "Parameter estimates calculated directly from the sample.")
    End Function

    Private Shared Function FailedFit(kind As CumulativeDistributionKind,
                                     message As String) As CumulativeDistributionFit
        Return New CumulativeDistributionFit(kind,
                                             GetParameterNames(kind),
                                             Array.Empty(Of Double)(),
                                             False,
                                             False,
                                             0,
                                             Double.NaN,
                                             message)
    End Function

    Private Shared Function BuildStartingValues(data() As Double,
                                                kind As CumulativeDistributionKind) As List(Of Double())
        Dim starts As New List(Of Double())()
        Dim minX As Double = data.Min()
        Dim maxX As Double = data.Max()
        Dim range As Double = maxX - minX
        If range <= 0.0R Then range = Math.Max(Math.Abs(minX) * 0.1R, 1.0R)

        If HasThreshold(kind) Then
            Dim gapFactors() As Double = {0.02R, 0.1R, 0.5R, 2.0R}
            For Each factor As Double In gapFactors
                Dim gap As Double = Math.Max(range * factor,
                                             Math.Max(Math.Abs(minX) * 1.0E-6R, 1.0E-8R))
                Dim threshold As Double = minX - gap
                Dim shifted(data.Length - 1) As Double
                For i As Integer = 0 To data.Length - 1
                    shifted(i) = data(i) - threshold
                Next
                Dim start As Double() = BuildBaseStartForThresholdFamily(shifted, kind, gap)
                If start IsNot Nothing Then starts.Add(start)
            Next
            Return starts
        End If

        Dim start0 As Double() = BuildBaseStart(data, kind)
        If start0 IsNot Nothing Then starts.Add(start0)
        Return starts
    End Function

    Private Shared Function BuildBaseStart(data() As Double,
                                           kind As CumulativeDistributionKind) As Double()
        Dim mean As Double = ComputeMean(data)
        Dim popSd As Double = ComputePopulationStandardDeviation(data, mean)
        Dim safeSd As Double = Math.Max(popSd, Math.Max(Math.Abs(mean) * 1.0E-6R, 1.0E-6R))

        Select Case kind
            Case CumulativeDistributionKind.Gamma
                Dim shape As Double = Math.Max(0.05R, Math.Min(1000.0R, (mean / safeSd) * (mean / safeSd)))
                Dim scale As Double = Math.Max(1.0E-12R, mean / shape)
                Return {Math.Log(shape), Math.Log(scale)}

            Case CumulativeDistributionKind.Weibull
                Dim cv As Double = safeSd / Math.Max(mean, 1.0E-12R)
                Dim shape As Double = Math.Pow(Math.Max(cv, 1.0E-4R), -1.086R)
                shape = Math.Max(0.1R, Math.Min(50.0R, shape))
                Dim gammaValue As Double = Math.Exp(StatFunc.LogGamma(1.0R + 1.0R / shape))
                Dim scale As Double = Math.Max(mean / Math.Max(gammaValue, 1.0E-12R), 1.0E-12R)
                Return {Math.Log(shape), Math.Log(scale)}

            Case CumulativeDistributionKind.SmallestExtremeValue
                Dim scale As Double = Math.Max(safeSd * Math.Sqrt(6.0R) / Math.PI, 1.0E-12R)
                Dim location As Double = mean + EulerGamma * scale
                Return {location, Math.Log(scale)}

            Case CumulativeDistributionKind.LargestExtremeValue
                Dim scale As Double = Math.Max(safeSd * Math.Sqrt(6.0R) / Math.PI, 1.0E-12R)
                Dim location As Double = mean - EulerGamma * scale
                Return {location, Math.Log(scale)}

            Case CumulativeDistributionKind.Logistic
                Dim scale As Double = Math.Max(safeSd * Math.Sqrt(3.0R) / Math.PI, 1.0E-12R)
                Return {MedianOfSorted(data), Math.Log(scale)}

            Case CumulativeDistributionKind.Loglogistic
                Dim logData(data.Length - 1) As Double
                For i As Integer = 0 To data.Length - 1
                    logData(i) = Math.Log(data(i))
                Next
                Dim logMean As Double = ComputeMean(logData)
                Dim logSd As Double = Math.Max(ComputePopulationStandardDeviation(logData, logMean), 1.0E-6R)
                Dim logScale As Double = Math.Max(logSd * Math.Sqrt(3.0R) / Math.PI, 1.0E-12R)
                Array.Sort(logData)
                Return {MedianOfSorted(logData), Math.Log(logScale)}
        End Select

        Return Nothing
    End Function

    Private Shared Function BuildBaseStartForThresholdFamily(shifted() As Double,
                                                             kind As CumulativeDistributionKind,
                                                             gap As Double) As Double()
        Dim logGap As Double = Math.Log(Math.Max(gap, 1.0E-14R))

        Select Case kind
            Case CumulativeDistributionKind.ThreeParameterLognormal
                Dim logData(shifted.Length - 1) As Double
                For i As Integer = 0 To shifted.Length - 1
                    logData(i) = Math.Log(shifted(i))
                Next
                Dim location As Double = ComputeMean(logData)
                Dim scale As Double = Math.Max(ComputePopulationStandardDeviation(logData, location), 1.0E-6R)
                Return {location, Math.Log(scale), logGap}

            Case CumulativeDistributionKind.ThreeParameterGamma
                Dim mean As Double = ComputeMean(shifted)
                Dim sd As Double = Math.Max(ComputePopulationStandardDeviation(shifted, mean), 1.0E-6R)
                Dim shape As Double = Math.Max(0.05R, Math.Min(1000.0R, (mean / sd) * (mean / sd)))
                Dim scale As Double = Math.Max(mean / shape, 1.0E-12R)
                Return {Math.Log(shape), Math.Log(scale), logGap}

            Case CumulativeDistributionKind.ThreeParameterWeibull
                Dim mean As Double = ComputeMean(shifted)
                Dim sd As Double = Math.Max(ComputePopulationStandardDeviation(shifted, mean), 1.0E-6R)
                Dim cv As Double = sd / Math.Max(mean, 1.0E-12R)
                Dim shape As Double = Math.Pow(Math.Max(cv, 1.0E-4R), -1.086R)
                shape = Math.Max(0.1R, Math.Min(50.0R, shape))
                Dim gammaValue As Double = Math.Exp(StatFunc.LogGamma(1.0R + 1.0R / shape))
                Dim scale As Double = Math.Max(mean / Math.Max(gammaValue, 1.0E-12R), 1.0E-12R)
                Return {Math.Log(shape), Math.Log(scale), logGap}

            Case CumulativeDistributionKind.ThreeParameterLoglogistic
                Dim logData(shifted.Length - 1) As Double
                For i As Integer = 0 To shifted.Length - 1
                    logData(i) = Math.Log(shifted(i))
                Next
                Dim loc As Double = ComputeMean(logData)
                Dim sd As Double = Math.Max(ComputePopulationStandardDeviation(logData, loc), 1.0E-6R)
                Dim scale As Double = Math.Max(sd * Math.Sqrt(3.0R) / Math.PI, 1.0E-12R)
                Array.Sort(logData)
                Return {MedianOfSorted(logData), Math.Log(scale), logGap}
        End Select

        Return Nothing
    End Function

    Private Shared Function BuildInitialSteps(start() As Double,
                                              kind As CumulativeDistributionKind) As Double()
        Dim steps(start.Length - 1) As Double
        For i As Integer = 0 To start.Length - 1
            If i = 0 AndAlso IsLocationFirst(kind) Then
                steps(i) = Math.Max(0.05R * (Math.Abs(start(i)) + 1.0R), 0.05R)
            Else
                steps(i) = 0.2R
            End If
        Next
        Return steps
    End Function

    Private Shared Function NegativeLogLikelihoodTheta(data() As Double,
                                                       kind As CumulativeDistributionKind,
                                                       theta() As Double) As Double
        Dim parameters() As Double = DecodeTheta(kind, theta, data.Min())
        If parameters Is Nothing OrElse Not ParametersAreValid(kind, parameters) Then Return HugeObjective

        Dim ll As Double = ComputeLogLikelihood(data, kind, parameters)
        If Not IsFinite(ll) Then Return HugeObjective
        Dim nll As Double = -ll
        If Not IsFinite(nll) OrElse nll > HugeObjective Then Return HugeObjective
        Return nll
    End Function

    Private Shared Function DecodeTheta(kind As CumulativeDistributionKind,
                                        theta() As Double,
                                        minimumDataValue As Double) As Double()
        Try
            Select Case kind
                Case CumulativeDistributionKind.Gamma,
                     CumulativeDistributionKind.Weibull
                    If theta.Length <> 2 Then Return Nothing
                    Return {SafeExp(theta(0)), SafeExp(theta(1))}

                Case CumulativeDistributionKind.SmallestExtremeValue,
                     CumulativeDistributionKind.LargestExtremeValue,
                     CumulativeDistributionKind.Logistic,
                     CumulativeDistributionKind.Loglogistic
                    If theta.Length <> 2 Then Return Nothing
                    Return {theta(0), SafeExp(theta(1))}

                Case CumulativeDistributionKind.ThreeParameterLognormal,
                     CumulativeDistributionKind.ThreeParameterLoglogistic
                    If theta.Length <> 3 Then Return Nothing
                    Dim gap As Double = SafeExp(theta(2))
                    If Not IsFinite(gap) Then Return Nothing
                    Return {theta(0), SafeExp(theta(1)), minimumDataValue - gap}

                Case CumulativeDistributionKind.ThreeParameterGamma,
                     CumulativeDistributionKind.ThreeParameterWeibull
                    If theta.Length <> 3 Then Return Nothing
                    Dim gap As Double = SafeExp(theta(2))
                    If Not IsFinite(gap) Then Return Nothing
                    Return {SafeExp(theta(0)), SafeExp(theta(1)), minimumDataValue - gap}
            End Select
        Catch
            Return Nothing
        End Try
        Return Nothing
    End Function

    Private Shared Function ParametersAreValid(kind As CumulativeDistributionKind,
                                               parameters() As Double) As Boolean
        If parameters Is Nothing Then Return False
        For Each p As Double In parameters
            If Not IsFinite(p) Then Return False
        Next

        Select Case kind
            Case CumulativeDistributionKind.Normal,
                 CumulativeDistributionKind.Lognormal,
                 CumulativeDistributionKind.SmallestExtremeValue,
                 CumulativeDistributionKind.LargestExtremeValue,
                 CumulativeDistributionKind.Logistic,
                 CumulativeDistributionKind.Loglogistic
                Return parameters.Length = 2 AndAlso parameters(1) > 0.0R

            Case CumulativeDistributionKind.Gamma,
                 CumulativeDistributionKind.Weibull
                Return parameters.Length = 2 AndAlso parameters(0) > 0.0R AndAlso parameters(1) > 0.0R

            Case CumulativeDistributionKind.Exponential
                Return parameters.Length = 1 AndAlso parameters(0) > 0.0R

            Case CumulativeDistributionKind.TwoParameterExponential
                Return parameters.Length = 2 AndAlso parameters(0) > 0.0R

            Case CumulativeDistributionKind.ThreeParameterLognormal,
                 CumulativeDistributionKind.ThreeParameterLoglogistic
                Return parameters.Length = 3 AndAlso parameters(1) > 0.0R

            Case CumulativeDistributionKind.ThreeParameterGamma,
                 CumulativeDistributionKind.ThreeParameterWeibull
                Return parameters.Length = 3 AndAlso parameters(0) > 0.0R AndAlso parameters(1) > 0.0R
        End Select
        Return False
    End Function

    Private Shared Function ComputeLogLikelihood(data() As Double,
                                                 kind As CumulativeDistributionKind,
                                                 parameters() As Double) As Double
        Dim total As Double = 0.0R
        For Each x As Double In data
            Dim lp As Double = EvaluateLogPdf(x, kind, parameters)
            If Not IsFinite(lp) Then Return Double.NaN
            total += lp
            If Not IsFinite(total) Then Return Double.NaN
        Next
        Return total
    End Function

    Private Shared Function EvaluateLogPdf(x As Double,
                                           kind As CumulativeDistributionKind,
                                           p() As Double) As Double
        Select Case kind
            Case CumulativeDistributionKind.Normal
                Dim z As Double = (x - p(0)) / p(1)
                Return -Math.Log(p(1)) - 0.5R * LogTwoPi - 0.5R * z * z

            Case CumulativeDistributionKind.Lognormal
                If x <= 0.0R Then Return Double.NaN
                Dim lx As Double = Math.Log(x)
                Dim z As Double = (lx - p(0)) / p(1)
                Return -Math.Log(x) - Math.Log(p(1)) - 0.5R * LogTwoPi - 0.5R * z * z

            Case CumulativeDistributionKind.ThreeParameterLognormal
                Dim y As Double = x - p(2)
                If y <= 0.0R Then Return Double.NaN
                Dim ly As Double = Math.Log(y)
                Dim z As Double = (ly - p(0)) / p(1)
                Return -Math.Log(y) - Math.Log(p(1)) - 0.5R * LogTwoPi - 0.5R * z * z

            Case CumulativeDistributionKind.Gamma
                If x <= 0.0R Then Return Double.NaN
                Return GammaLogPdf(x, p(0), p(1))

            Case CumulativeDistributionKind.ThreeParameterGamma
                Dim y As Double = x - p(2)
                If y <= 0.0R Then Return Double.NaN
                Return GammaLogPdf(y, p(0), p(1))

            Case CumulativeDistributionKind.Exponential
                If x < 0.0R Then Return Double.NaN
                Return -Math.Log(p(0)) - x / p(0)

            Case CumulativeDistributionKind.TwoParameterExponential
                Dim y As Double = x - p(1)
                If y < 0.0R Then Return Double.NaN
                Return -Math.Log(p(0)) - y / p(0)

            Case CumulativeDistributionKind.SmallestExtremeValue
                Dim z As Double = (x - p(0)) / p(1)
                If z > 700.0R Then Return Double.NaN
                Return -Math.Log(p(1)) + z - Math.Exp(z)

            Case CumulativeDistributionKind.LargestExtremeValue
                Dim z As Double = (x - p(0)) / p(1)
                If -z > 700.0R Then Return Double.NaN
                Return -Math.Log(p(1)) - z - Math.Exp(-z)

            Case CumulativeDistributionKind.Weibull
                If x <= 0.0R Then Return Double.NaN
                Return WeibullLogPdf(x, p(0), p(1))

            Case CumulativeDistributionKind.ThreeParameterWeibull
                Dim y As Double = x - p(2)
                If y <= 0.0R Then Return Double.NaN
                Return WeibullLogPdf(y, p(0), p(1))

            Case CumulativeDistributionKind.Logistic
                Dim z As Double = (x - p(0)) / p(1)
                Return LogisticStandardLogPdf(z) - Math.Log(p(1))

            Case CumulativeDistributionKind.Loglogistic
                If x <= 0.0R Then Return Double.NaN
                Dim lx As Double = Math.Log(x)
                Dim z As Double = (lx - p(0)) / p(1)
                Return LogisticStandardLogPdf(z) - Math.Log(p(1)) - Math.Log(x)

            Case CumulativeDistributionKind.ThreeParameterLoglogistic
                Dim y As Double = x - p(2)
                If y <= 0.0R Then Return Double.NaN
                Dim ly As Double = Math.Log(y)
                Dim z As Double = (ly - p(0)) / p(1)
                Return LogisticStandardLogPdf(z) - Math.Log(p(1)) - Math.Log(y)
        End Select

        Return Double.NaN
    End Function

    Private Shared Function GammaLogPdf(x As Double, shape As Double, scale As Double) As Double
        If x <= 0.0R OrElse shape <= 0.0R OrElse scale <= 0.0R Then Return Double.NaN
        Return (shape - 1.0R) * Math.Log(x) - x / scale - StatFunc.LogGamma(shape) - shape * Math.Log(scale)
    End Function

    Private Shared Function WeibullLogPdf(x As Double, shape As Double, scale As Double) As Double
        If x <= 0.0R OrElse shape <= 0.0R OrElse scale <= 0.0R Then Return Double.NaN
        Dim logRatio As Double = Math.Log(x) - Math.Log(scale)
        Dim power As Double = SafeExp(shape * logRatio)
        If Not IsFinite(power) Then Return Double.NaN
        Return Math.Log(shape) - Math.Log(scale) + (shape - 1.0R) * logRatio - power
    End Function

    Private Shared Function LogisticStandardLogPdf(z As Double) As Double
        If z >= 0.0R Then
            Return -z - 2.0R * Log1p(Math.Exp(-z))
        Else
            Return z - 2.0R * Log1p(Math.Exp(z))
        End If
    End Function

    Private Shared Function EvaluateCdf(x As Double,
                                       kind As CumulativeDistributionKind,
                                       p() As Double) As Double
        If p Is Nothing OrElse Not ParametersAreValid(kind, p) Then Return Double.NaN

        Select Case kind
            Case CumulativeDistributionKind.Normal
                Return Clamp(distributions.PNorm(x, p(0), p(1)), 0.0R, 1.0R)

            Case CumulativeDistributionKind.Lognormal
                If x <= 0.0R Then Return 0.0R
                Return Clamp(distributions.PNorm(Math.Log(x), p(0), p(1)), 0.0R, 1.0R)

            Case CumulativeDistributionKind.ThreeParameterLognormal
                Dim y As Double = x - p(2)
                If y <= 0.0R Then Return 0.0R
                Return Clamp(distributions.PNorm(Math.Log(y), p(0), p(1)), 0.0R, 1.0R)

            Case CumulativeDistributionKind.Gamma
                If x <= 0.0R Then Return 0.0R
                Return Clamp(distributions.ChiSquareCDF(2.0R * x / p(1), 2.0R * p(0)), 0.0R, 1.0R)

            Case CumulativeDistributionKind.ThreeParameterGamma
                Dim y As Double = x - p(2)
                If y <= 0.0R Then Return 0.0R
                Return Clamp(distributions.ChiSquareCDF(2.0R * y / p(1), 2.0R * p(0)), 0.0R, 1.0R)

            Case CumulativeDistributionKind.Exponential
                If x <= 0.0R Then Return 0.0R
                Return Clamp(1.0R - Math.Exp(-x / p(0)), 0.0R, 1.0R)

            Case CumulativeDistributionKind.TwoParameterExponential
                Dim y As Double = x - p(1)
                If y <= 0.0R Then Return 0.0R
                Return Clamp(1.0R - Math.Exp(-y / p(0)), 0.0R, 1.0R)

            Case CumulativeDistributionKind.SmallestExtremeValue
                Dim z As Double = (x - p(0)) / p(1)
                If z >= 40.0R Then Return 1.0R
                If z <= -36.0R Then Return Math.Exp(z)
                Return Clamp(1.0R - Math.Exp(-Math.Exp(z)), 0.0R, 1.0R)

            Case CumulativeDistributionKind.Weibull
                If x <= 0.0R Then Return 0.0R
                Return WeibullCdf(x, p(0), p(1))

            Case CumulativeDistributionKind.ThreeParameterWeibull
                Dim y As Double = x - p(2)
                If y <= 0.0R Then Return 0.0R
                Return WeibullCdf(y, p(0), p(1))

            Case CumulativeDistributionKind.LargestExtremeValue
                Dim z As Double = (x - p(0)) / p(1)
                If z >= 36.0R Then Return 1.0R
                If z <= -40.0R Then Return 0.0R
                Return Clamp(Math.Exp(-Math.Exp(-z)), 0.0R, 1.0R)

            Case CumulativeDistributionKind.Logistic
                Return LogisticCdf((x - p(0)) / p(1))

            Case CumulativeDistributionKind.Loglogistic
                If x <= 0.0R Then Return 0.0R
                Return LogisticCdf((Math.Log(x) - p(0)) / p(1))

            Case CumulativeDistributionKind.ThreeParameterLoglogistic
                Dim y As Double = x - p(2)
                If y <= 0.0R Then Return 0.0R
                Return LogisticCdf((Math.Log(y) - p(0)) / p(1))
        End Select

        Return Double.NaN
    End Function

    Private Shared Function WeibullCdf(x As Double, shape As Double, scale As Double) As Double
        Dim exponent As Double = shape * (Math.Log(x) - Math.Log(scale))
        If exponent >= 40.0R Then Return 1.0R
        If exponent <= -36.0R Then Return Math.Exp(exponent)
        Dim power As Double = Math.Exp(exponent)
        Return Clamp(1.0R - Math.Exp(-power), 0.0R, 1.0R)
    End Function

    Private Shared Function LogisticCdf(z As Double) As Double
        If z >= 0.0R Then
            Dim e As Double = Math.Exp(-z)
            Return 1.0R / (1.0R + e)
        Else
            Dim e As Double = Math.Exp(z)
            Return e / (1.0R + e)
        End If
    End Function

    Private Shared Function GetParameterNames(kind As CumulativeDistributionKind) As String()
        Select Case kind
            Case CumulativeDistributionKind.Normal
                Return {"Mean", "Standard deviation"}
            Case CumulativeDistributionKind.Lognormal
                Return {"Location", "Scale"}
            Case CumulativeDistributionKind.ThreeParameterLognormal
                Return {"Location", "Scale", "Threshold"}
            Case CumulativeDistributionKind.Gamma
                Return {"Shape", "Scale"}
            Case CumulativeDistributionKind.ThreeParameterGamma
                Return {"Shape", "Scale", "Threshold"}
            Case CumulativeDistributionKind.Exponential
                Return {"Scale"}
            Case CumulativeDistributionKind.TwoParameterExponential
                Return {"Scale", "Threshold"}
            Case CumulativeDistributionKind.SmallestExtremeValue,
                 CumulativeDistributionKind.LargestExtremeValue,
                 CumulativeDistributionKind.Logistic,
                 CumulativeDistributionKind.Loglogistic
                Return {"Location", "Scale"}
            Case CumulativeDistributionKind.Weibull
                Return {"Shape", "Scale"}
            Case CumulativeDistributionKind.ThreeParameterWeibull
                Return {"Shape", "Scale", "Threshold"}
            Case CumulativeDistributionKind.ThreeParameterLoglogistic
                Return {"Location", "Scale", "Threshold"}
        End Select
        Return Array.Empty(Of String)()
    End Function

    Private Shared Function DistributionDisplayName(kind As CumulativeDistributionKind) As String
        Select Case kind
            Case CumulativeDistributionKind.ThreeParameterLognormal : Return "Three-parameter lognormal distribution"
            Case CumulativeDistributionKind.ThreeParameterGamma : Return "Three-parameter gamma distribution"
            Case CumulativeDistributionKind.TwoParameterExponential : Return "Two-parameter exponential distribution"
            Case CumulativeDistributionKind.SmallestExtremeValue : Return "Smallest extreme value distribution"
            Case CumulativeDistributionKind.ThreeParameterWeibull : Return "Three-parameter Weibull distribution"
            Case CumulativeDistributionKind.LargestExtremeValue : Return "Largest extreme value distribution"
            Case CumulativeDistributionKind.ThreeParameterLoglogistic : Return "Three-parameter loglogistic distribution"
            Case Else : Return kind.ToString() & " distribution"
        End Select
    End Function

    Private Shared Function HasThreshold(kind As CumulativeDistributionKind) As Boolean
        Return kind = CumulativeDistributionKind.ThreeParameterLognormal OrElse
               kind = CumulativeDistributionKind.ThreeParameterGamma OrElse
               kind = CumulativeDistributionKind.TwoParameterExponential OrElse
               kind = CumulativeDistributionKind.ThreeParameterWeibull OrElse
               kind = CumulativeDistributionKind.ThreeParameterLoglogistic
    End Function

    Private Shared Function HasZeroLowerSupport(kind As CumulativeDistributionKind) As Boolean
        Return kind = CumulativeDistributionKind.Lognormal OrElse
               kind = CumulativeDistributionKind.Gamma OrElse
               kind = CumulativeDistributionKind.Exponential OrElse
               kind = CumulativeDistributionKind.Weibull OrElse
               kind = CumulativeDistributionKind.Loglogistic
    End Function

    Private Shared Function IsLocationFirst(kind As CumulativeDistributionKind) As Boolean
        Return kind = CumulativeDistributionKind.SmallestExtremeValue OrElse
               kind = CumulativeDistributionKind.LargestExtremeValue OrElse
               kind = CumulativeDistributionKind.Logistic OrElse
               kind = CumulativeDistributionKind.Loglogistic OrElse
               kind = CumulativeDistributionKind.ThreeParameterLognormal OrElse
               kind = CumulativeDistributionKind.ThreeParameterLoglogistic
    End Function

    Private Shared Function TryGetThreshold(fit As CumulativeDistributionFit,
                                            ByRef threshold As Double) As Boolean
        threshold = Double.NaN
        If fit Is Nothing OrElse Not fit.Succeeded OrElse Not HasThreshold(fit.Distribution) Then Return False
        Dim values() As Double = fit.ParameterValues
        If values.Length = 0 Then Return False
        threshold = values(values.Length - 1)
        Return IsFinite(threshold)
    End Function

    Private Shared Function MedianOfSorted(sortedData() As Double) As Double
        Dim n As Integer = sortedData.Length
        If n = 0 Then Return Double.NaN
        If (n Mod 2) = 1 Then Return sortedData(n \ 2)
        Return 0.5R * (sortedData(n \ 2 - 1) + sortedData(n \ 2))
    End Function

    Private Shared Function SafeExp(value As Double) As Double
        If value > 700.0R Then Return Double.PositiveInfinity
        If value < -745.0R Then Return 0.0R
        Return Math.Exp(value)
    End Function

    Private Shared Function Log1p(value As Double) As Double
        If value <= -1.0R Then Return Double.NaN
        If Math.Abs(value) < 1.0E-8R Then
            Return value - 0.5R * value * value + value * value * value / 3.0R
        End If
        Return Math.Log(1.0R + value)
    End Function

    Private Shared Function Clamp(value As Double, minimum As Double, maximum As Double) As Double
        If Double.IsNaN(value) Then Return Double.NaN
        If value < minimum Then Return minimum
        If value > maximum Then Return maximum
        Return value
    End Function

    Private Shared Function BuildGroupKey(value As Object) As String
        If value Is Nothing Then Return "<NULL>"
        If TypeOf value Is String Then Return "S:" & DirectCast(value, String).Trim()
        If TypeOf value Is DateTime Then Return "D:" & DirectCast(value, DateTime).ToOADate().ToString("R", CultureInfo.InvariantCulture)
        If TypeOf value Is IFormattable Then
            Return "V:" & DirectCast(value, IFormattable).ToString(Nothing, CultureInfo.InvariantCulture)
        End If
        Return "O:" & value.ToString()
    End Function

    Private Shared Function FormatGroupLabel(value As Object) As String
        If value Is Nothing Then Return String.Empty
        If TypeOf value Is DateTime Then Return DirectCast(value, DateTime).ToString(CultureInfo.CurrentCulture)
        Return Convert.ToString(value, CultureInfo.CurrentCulture).Trim()
    End Function

    Private NotInheritable Class OptimizationVertex
        Public Theta() As Double
        Public Objective As Double

        Public Sub New(theta() As Double, objective As Double)
            Me.Theta = DirectCast(theta.Clone(), Double())
            Me.Objective = objective
        End Sub
    End Class

    Private NotInheritable Class OptimizationResult
        Public Theta() As Double
        Public Objective As Double
        Public Converged As Boolean
        Public Iterations As Integer
    End Class

    ''' <summary>
    ''' Small unconstrained Nelder-Mead implementation used only for distribution fitting.
    ''' Distribution-specific transformations keep scale/shape parameters positive and
    ''' threshold parameters below the smallest observation.
    ''' </summary>
    Private Shared Function NelderMead(objective As Func(Of Double(), Double),
                                      start() As Double,
                                      initialSteps() As Double,
                                      maximumIterations As Integer,
                                      tolerance As Double) As OptimizationResult
        Dim dimension As Integer = start.Length
        Dim simplex As New List(Of OptimizationVertex)(dimension + 1)

        Dim f0 As Double = SafeObjective(objective, start)
        simplex.Add(New OptimizationVertex(start, f0))

        For j As Integer = 0 To dimension - 1
            Dim point() As Double = DirectCast(start.Clone(), Double())
            point(j) += initialSteps(j)
            simplex.Add(New OptimizationVertex(point, SafeObjective(objective, point)))
        Next

        Dim iteration As Integer = 0
        Dim converged As Boolean = False

        While iteration < maximumIterations
            iteration += 1
            simplex.Sort(Function(a, b) a.Objective.CompareTo(b.Objective))

            Dim objectiveSpread As Double = Math.Abs(simplex(dimension).Objective - simplex(0).Objective)
            Dim parameterSpread As Double = 0.0R
            For i As Integer = 1 To dimension
                For j As Integer = 0 To dimension - 1
                    parameterSpread = Math.Max(parameterSpread,
                                               Math.Abs(simplex(i).Theta(j) - simplex(0).Theta(j)))
                Next
            Next

            Dim objectiveScale As Double = Math.Max(1.0R, Math.Abs(simplex(0).Objective))
            Dim parameterScale As Double = 1.0R
            For j As Integer = 0 To dimension - 1
                parameterScale = Math.Max(parameterScale, Math.Abs(simplex(0).Theta(j)))
            Next

            If objectiveSpread <= tolerance * objectiveScale AndAlso
               parameterSpread <= Math.Sqrt(tolerance) * parameterScale Then
                converged = True
                Exit While
            End If

            Dim centroid(dimension - 1) As Double
            For i As Integer = 0 To dimension - 1
                For j As Integer = 0 To dimension - 1
                    centroid(j) += simplex(i).Theta(j)
                Next
            Next
            For j As Integer = 0 To dimension - 1
                centroid(j) /= CDbl(dimension)
            Next

            Dim worst As OptimizationVertex = simplex(dimension)
            Dim reflected() As Double = Combine(centroid, worst.Theta, 1.0R)
            Dim fReflected As Double = SafeObjective(objective, reflected)

            If fReflected < simplex(0).Objective Then
                Dim expanded() As Double = Combine(centroid, worst.Theta, 2.0R)
                Dim fExpanded As Double = SafeObjective(objective, expanded)
                If fExpanded < fReflected Then
                    simplex(dimension) = New OptimizationVertex(expanded, fExpanded)
                Else
                    simplex(dimension) = New OptimizationVertex(reflected, fReflected)
                End If
                Continue While
            End If

            If fReflected < simplex(dimension - 1).Objective Then
                simplex(dimension) = New OptimizationVertex(reflected, fReflected)
                Continue While
            End If

            Dim contracted() As Double
            If fReflected < worst.Objective Then
                contracted = ContractOutside(centroid, reflected)
            Else
                contracted = ContractInside(centroid, worst.Theta)
            End If
            Dim fContracted As Double = SafeObjective(objective, contracted)

            If fContracted < Math.Min(worst.Objective, fReflected) Then
                simplex(dimension) = New OptimizationVertex(contracted, fContracted)
                Continue While
            End If

            Dim bestTheta() As Double = DirectCast(simplex(0).Theta.Clone(), Double())
            For i As Integer = 1 To dimension
                Dim shrunk(dimension - 1) As Double
                For j As Integer = 0 To dimension - 1
                    shrunk(j) = bestTheta(j) + 0.5R * (simplex(i).Theta(j) - bestTheta(j))
                Next
                simplex(i) = New OptimizationVertex(shrunk, SafeObjective(objective, shrunk))
            Next
        End While

        simplex.Sort(Function(a, b) a.Objective.CompareTo(b.Objective))
        Return New OptimizationResult With {
            .Theta = DirectCast(simplex(0).Theta.Clone(), Double()),
            .Objective = simplex(0).Objective,
            .Converged = converged,
            .Iterations = iteration
        }
    End Function

    Private Shared Function SafeObjective(objective As Func(Of Double(), Double), theta() As Double) As Double
        Try
            Dim value As Double = objective(theta)
            If Not IsFinite(value) Then Return HugeObjective
            If value > HugeObjective Then Return HugeObjective
            Return value
        Catch
            Return HugeObjective
        End Try
    End Function

    Private Shared Function Combine(centroid() As Double,
                                    worst() As Double,
                                    factor As Double) As Double()
        'factor=1: reflection c + (c-w); factor=2: expansion c + 2(c-w)
        Dim output(centroid.Length - 1) As Double
        For i As Integer = 0 To output.Length - 1
            output(i) = centroid(i) + factor * (centroid(i) - worst(i))
        Next
        Return output
    End Function

    Private Shared Function ContractOutside(centroid() As Double, reflected() As Double) As Double()
        Dim output(centroid.Length - 1) As Double
        For i As Integer = 0 To output.Length - 1
            output(i) = centroid(i) + 0.5R * (reflected(i) - centroid(i))
        Next
        Return output
    End Function

    Private Shared Function ContractInside(centroid() As Double, worst() As Double) As Double()
        Dim output(centroid.Length - 1) As Double
        For i As Integer = 0 To output.Length - 1
            output(i) = centroid(i) + 0.5R * (worst(i) - centroid(i))
        Next
        Return output
    End Function
End Class
