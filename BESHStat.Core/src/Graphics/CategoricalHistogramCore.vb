Option Explicit On
Option Strict On
Option Infer On

Imports System
Imports System.Collections.Generic
Imports System.Globalization
Imports System.Linq

''' <summary>
''' Portable categorical histogram contracts and grouped histogram calculations.
''' Uses the existing HistogramBinningCore for shared bin selection; Excel rendering
''' and display appearance stay exclusively in the Windows Excel-DNA host.
''' </summary>
Public Enum CategoricalHistogramPlotType
    ''' <summary>
    ''' Side-by-side bars with one independently normalised density histogram per group.
    ''' This corresponds to the Matplotlib "bars with legend" example.
    ''' </summary>
    BarsWithLegend

    ''' <summary>
    ''' Stacked bars. Group contributions are normalised together so that the total
    ''' stacked histogram integrates to one. This corresponds to the Matplotlib
    ''' "stacked bar" example with density=True and stacked=True.
    ''' </summary>
    StackedBar

    ''' <summary>
    ''' Side-by-side raw frequency counts. This is useful when the groups have different
    ''' sample sizes and corresponds to the Matplotlib "different sample sizes" example.
    ''' </summary>
    DifferentSampleSizes

    ''' <summary>
    ''' Mirrored percentages for exactly two groups. The two groups are normalised
    ''' independently and drawn on opposite sides of zero.
    ''' </summary>
    Comparative
End Enum

''' <summary>
''' Orientation of the mirrored comparative histogram.
''' </summary>
Public Enum CategoricalHistogramComparativeOrientation
    ''' <summary>Vertical columns, with group 1 above and group 2 below zero.</summary>
    Vertical

    ''' <summary>Horizontal bars, with group 1 left and group 2 right of zero.</summary>
    Horizontal
End Enum

''' <summary>
''' Histogram bin-size rules supported by the ordinary BESHStatNG histogram.
''' </summary>
Public Enum CategoricalHistogramBinningRule
    Sturges
    Doane
    Scott
    FreedmanDiaconis
End Enum

''' <summary>
''' Numerical options for a categorical histogram.
''' </summary>
Public Class CategoricalHistogramOptions
    Public Property PlotType As CategoricalHistogramPlotType = CategoricalHistogramPlotType.BarsWithLegend
    Public Property BinningRule As CategoricalHistogramBinningRule = CategoricalHistogramBinningRule.Sturges
    Public Property ComparativeOrientation As CategoricalHistogramComparativeOrientation = CategoricalHistogramComparativeOrientation.Vertical

    ''' <summary>
    ''' Creates an independent copy of the options.
    ''' </summary>
    Friend Function Copy() As CategoricalHistogramOptions
        Return New CategoricalHistogramOptions With {
            .PlotType = PlotType,
            .BinningRule = BinningRule,
            .ComparativeOrientation = ComparativeOrientation
        }
    End Function
End Class

''' <summary>
''' Histogram values for one categorical level.
''' </summary>
Public NotInheritable Class CategoricalHistogramSeries
    Private ReadOnly _name As String
    Private ReadOnly _groupValue As Object
    Private ReadOnly _sampleSize As Integer
    Private ReadOnly _counts As Double()
    Private ReadOnly _perGroupDensity As Double()
    Private ReadOnly _pooledDensityContribution As Double()
    Private ReadOnly _perGroupPercent As Double()
    Private ReadOnly _plotValues As Double()

    Friend Sub New(name As String,
                   groupValue As Object,
                   sampleSize As Integer,
                   counts As Double(),
                   perGroupDensity As Double(),
                   pooledDensityContribution As Double(),
                   perGroupPercent As Double(),
                   plotValues As Double())
        _name = name
        _groupValue = groupValue
        _sampleSize = sampleSize
        _counts = DirectCast(counts.Clone(), Double())
        _perGroupDensity = DirectCast(perGroupDensity.Clone(), Double())
        _pooledDensityContribution = DirectCast(pooledDensityContribution.Clone(), Double())
        _perGroupPercent = DirectCast(perGroupPercent.Clone(), Double())
        _plotValues = DirectCast(plotValues.Clone(), Double())
    End Sub

    Public ReadOnly Property Name As String
        Get
            Return _name
        End Get
    End Property

    Public ReadOnly Property GroupValue As Object
        Get
            Return _groupValue
        End Get
    End Property

    Public ReadOnly Property SampleSize As Integer
        Get
            Return _sampleSize
        End Get
    End Property

    ''' <summary>Raw frequency in each common bin.</summary>
    Public ReadOnly Property Counts As Double()
        Get
            Return DirectCast(_counts.Clone(), Double())
        End Get
    End Property

    ''' <summary>
    ''' Density values normalised within this group: count / (group n * bin width).
    ''' </summary>
    Public ReadOnly Property PerGroupDensity As Double()
        Get
            Return DirectCast(_perGroupDensity.Clone(), Double())
        End Get
    End Property

    ''' <summary>
    ''' Density contribution normalised against all usable observations:
    ''' count / (pooled n * bin width). Summing these contributions across groups gives
    ''' a density histogram with total area one.
    ''' </summary>
    Public ReadOnly Property PooledDensityContribution As Double()
        Get
            Return DirectCast(_pooledDensityContribution.Clone(), Double())
        End Get
    End Property

    ''' <summary>
    ''' Percentage of this group's usable observations in each common bin. The values
    ''' sum to 100 (apart from floating-point round-off) and are used by comparative
    ''' histograms so groups with different sample sizes remain directly comparable.
    ''' </summary>
    Public ReadOnly Property PerGroupPercent As Double()
        Get
            Return DirectCast(_perGroupPercent.Clone(), Double())
        End Get
    End Property


    ''' <summary>Values selected by the requested plot type and supplied to Excel.</summary>
    Public ReadOnly Property PlotValues As Double()
        Get
            Return DirectCast(_plotValues.Clone(), Double())
        End Get
    End Property
End Class

''' <summary>
''' Immutable numerical result returned by <see cref="CategoricalHistogramCore.Compute"/>.
''' </summary>
Public NotInheritable Class CategoricalHistogramResult
    Private ReadOnly _binMidpoints As Double()
    Private ReadOnly _series As CategoricalHistogramSeries()
    Private ReadOnly _options As CategoricalHistogramOptions
    Private ReadOnly _binWidth As Double
    Private ReadOnly _binMinimum As Double
    Private ReadOnly _binMaximum As Double
    Private ReadOnly _sourceObservationCount As Integer
    Private ReadOnly _usableObservationCount As Integer
    Private ReadOnly _excludedObservationCount As Integer

    Friend Sub New(binMidpoints As Double(),
                   series As CategoricalHistogramSeries(),
                   options As CategoricalHistogramOptions,
                   binWidth As Double,
                   binMinimum As Double,
                   binMaximum As Double,
                   sourceObservationCount As Integer,
                   usableObservationCount As Integer,
                   excludedObservationCount As Integer)
        _binMidpoints = DirectCast(binMidpoints.Clone(), Double())
        _series = DirectCast(series.Clone(), CategoricalHistogramSeries())
        _options = options.Copy()
        _binWidth = binWidth
        _binMinimum = binMinimum
        _binMaximum = binMaximum
        _sourceObservationCount = sourceObservationCount
        _usableObservationCount = usableObservationCount
        _excludedObservationCount = excludedObservationCount
    End Sub

    Public ReadOnly Property BinMidpoints As Double()
        Get
            Return DirectCast(_binMidpoints.Clone(), Double())
        End Get
    End Property

    Public ReadOnly Property Series As CategoricalHistogramSeries()
        Get
            Return DirectCast(_series.Clone(), CategoricalHistogramSeries())
        End Get
    End Property

    Public ReadOnly Property Options As CategoricalHistogramOptions
        Get
            Return _options.Copy()
        End Get
    End Property

    Public ReadOnly Property BinWidth As Double
        Get
            Return _binWidth
        End Get
    End Property

    Public ReadOnly Property BinMinimum As Double
        Get
            Return _binMinimum
        End Get
    End Property

    Public ReadOnly Property BinMaximum As Double
        Get
            Return _binMaximum
        End Get
    End Property

    Public ReadOnly Property SourceObservationCount As Integer
        Get
            Return _sourceObservationCount
        End Get
    End Property

    Public ReadOnly Property UsableObservationCount As Integer
        Get
            Return _usableObservationCount
        End Get
    End Property

    Public ReadOnly Property ExcludedObservationCount As Integer
        Get
            Return _excludedObservationCount
        End Get
    End Property

    Public ReadOnly Property GroupCount As Integer
        Get
            Return _series.Length
        End Get
    End Property

    Public ReadOnly Property BinCount As Integer
        Get
            Return _binMidpoints.Length
        End Get
    End Property
End Class

''' <summary>
''' Computes grouped histograms from one continuous variable and one categorical variable.
''' All groups always share one set of bins calculated from the pooled continuous data.
''' </summary>
Public NotInheritable Class CategoricalHistogramCore
    Private Sub New()
    End Sub

    Private NotInheritable Class WorkingGroup
        Friend Key As String
        Friend Name As String
        Friend Value As Object
        Friend Values As New List(Of Double)()
    End Class

    ''' <summary>
    ''' Computes a categorical histogram.
    ''' </summary>
    ''' <param name="values">
    ''' Continuous observations. Missing continuous values can be represented by NaN.
    ''' </param>
    ''' <param name="groups">
    ''' Categorical observations aligned row-by-row with <paramref name="values"/>.
    ''' Text and numeric group values are supported. Missing/blank group values are omitted.
    ''' </param>
    ''' <param name="options">Plot and binning options.</param>
    Public Shared Function Compute(values As Double(),
                                   groups As Array,
                                   Optional options As CategoricalHistogramOptions = Nothing) As CategoricalHistogramResult
        If values Is Nothing Then Throw New ArgumentNullException(NameOf(values))
        If groups Is Nothing Then Throw New ArgumentNullException(NameOf(groups))
        If groups.Rank <> 1 Then
            Throw New ArgumentException("The categorical variable must be a one-dimensional array.", NameOf(groups))
        End If
        If groups.Length <> values.Length Then
            Throw New ArgumentException("The continuous and categorical variables must contain the same number of rows.",
                                        NameOf(groups))
        End If
        If values.Length = 0 Then
            Throw New ArgumentException("At least one observation is required.", NameOf(values))
        End If

        Dim resolvedOptions As CategoricalHistogramOptions = If(options, New CategoricalHistogramOptions()).Copy()
        ValidateOptions(resolvedOptions)

        Dim orderedGroups As New List(Of WorkingGroup)()
        Dim groupsByKey As New Dictionary(Of String, WorkingGroup)(StringComparer.Ordinal)
        Dim pooledValues As New List(Of Double)()
        Dim excludedCount As Integer = 0
        Dim groupLowerBound As Integer = groups.GetLowerBound(0)

        For i As Integer = 0 To values.Length - 1
            Dim x As Double = values(i)
            Dim groupValue As Object = groups.GetValue(groupLowerBound + i)

            If Double.IsNaN(x) OrElse IsMissingGroupValue(groupValue) Then
                excludedCount += 1
                Continue For
            End If
            If Double.IsInfinity(x) Then
                Throw New ArgumentOutOfRangeException(NameOf(values),
                                                      "Continuous observation at row " & (i + 1).ToString(CultureInfo.CurrentCulture) &
                                                      " is infinite.")
            End If

            Dim key As String = BuildGroupKey(groupValue)
            Dim working As WorkingGroup = Nothing
            If Not groupsByKey.TryGetValue(key, working) Then
                working = New WorkingGroup With {
                    .Key = key,
                    .Name = FormatGroupName(groupValue),
                    .Value = groupValue
                }
                groupsByKey.Add(key, working)
                orderedGroups.Add(working)
            End If

            working.Values.Add(x)
            pooledValues.Add(x)
        Next

        If pooledValues.Count = 0 Then
            Throw New ArgumentException("No usable paired continuous/categorical observations were found.")
        End If
        If orderedGroups.Count = 0 Then
            Throw New ArgumentException("No usable categorical levels were found.", NameOf(groups))
        End If
        If resolvedOptions.PlotType = CategoricalHistogramPlotType.Comparative AndAlso orderedGroups.Count <> 2 Then
            Throw New ArgumentException("Comparative histograms require exactly two usable groups. " &
                                        "The current input contains " & orderedGroups.Count.ToString(CultureInfo.CurrentCulture) & " usable groups.",
                                        NameOf(groups))
        End If

        Dim pooled() As Double = pooledValues.ToArray()
        Dim binTable As Object(,) = HistogramBinningCore.HistogramBinsComputation(pooled,
                                                                      BinningRuleToLegacyText(resolvedOptions.BinningRule))
        Dim binCount As Integer = binTable.GetLength(0)
        If binCount < 1 Then Throw New InvalidOperationException("Histogram binning returned no bins.")

        Dim midpoints(binCount - 1) As Double
        For binIndex As Integer = 0 To binCount - 1
            midpoints(binIndex) = Convert.ToDouble(binTable(binIndex, 0), CultureInfo.InvariantCulture)
        Next

        Dim binWidth As Double = ResolveBinWidth(midpoints, pooled)
        Dim binMinimum As Double = midpoints(0) - binWidth / 2.0R
        Dim binMaximum As Double = midpoints(midpoints.Length - 1) + binWidth / 2.0R
        Dim totalN As Integer = pooled.Length

        Dim outputSeries(orderedGroups.Count - 1) As CategoricalHistogramSeries
        For groupIndex As Integer = 0 To orderedGroups.Count - 1
            Dim working As WorkingGroup = orderedGroups(groupIndex)
            Dim counts As Double() = CountBins(working.Values, binMinimum, binMaximum, binWidth, binCount)
            Dim perGroupDensity(binCount - 1) As Double
            Dim pooledDensityContribution(binCount - 1) As Double
            Dim perGroupPercent(binCount - 1) As Double
            Dim plotValues(binCount - 1) As Double

            For binIndex As Integer = 0 To binCount - 1
                perGroupDensity(binIndex) = counts(binIndex) / (Convert.ToDouble(working.Values.Count) * binWidth)
                pooledDensityContribution(binIndex) = counts(binIndex) / (Convert.ToDouble(totalN) * binWidth)
                perGroupPercent(binIndex) = 100.0R * counts(binIndex) / Convert.ToDouble(working.Values.Count)

                Select Case resolvedOptions.PlotType
                    Case CategoricalHistogramPlotType.BarsWithLegend
                        plotValues(binIndex) = perGroupDensity(binIndex)
                    Case CategoricalHistogramPlotType.StackedBar
                        plotValues(binIndex) = pooledDensityContribution(binIndex)
                    Case CategoricalHistogramPlotType.DifferentSampleSizes
                        plotValues(binIndex) = counts(binIndex)
                    Case CategoricalHistogramPlotType.Comparative
                        'Keep numerical results positive. The Excel renderer applies the
                        'orientation-specific sign so presentation does not leak into
                        'the statistical computation.
                        plotValues(binIndex) = perGroupPercent(binIndex)
                    Case Else
                        Throw New ArgumentOutOfRangeException(NameOf(resolvedOptions.PlotType))
                End Select
            Next

            outputSeries(groupIndex) = New CategoricalHistogramSeries(working.Name,
                                                                       working.Value,
                                                                       working.Values.Count,
                                                                       counts,
                                                                       perGroupDensity,
                                                                       pooledDensityContribution,
                                                                       perGroupPercent,
                                                                       plotValues)
        Next

        Return New CategoricalHistogramResult(midpoints,
                                              outputSeries,
                                              resolvedOptions,
                                              binWidth,
                                              binMinimum,
                                              binMaximum,
                                              values.Length,
                                              totalN,
                                              excludedCount)
    End Function

    Private Shared Sub ValidateOptions(options As CategoricalHistogramOptions)
        If Not [Enum].IsDefined(GetType(CategoricalHistogramPlotType), options.PlotType) Then
            Throw New ArgumentOutOfRangeException(NameOf(options.PlotType), "The categorical histogram plot type is not defined.")
        End If
        If Not [Enum].IsDefined(GetType(CategoricalHistogramBinningRule), options.BinningRule) Then
            Throw New ArgumentOutOfRangeException(NameOf(options.BinningRule), "The histogram binning rule is not defined.")
        End If
        If Not [Enum].IsDefined(GetType(CategoricalHistogramComparativeOrientation), options.ComparativeOrientation) Then
            Throw New ArgumentOutOfRangeException(NameOf(options.ComparativeOrientation), "The comparative histogram orientation is not defined.")
        End If
    End Sub

    Private Shared Function BinningRuleToLegacyText(rule As CategoricalHistogramBinningRule) As String
        Select Case rule
            Case CategoricalHistogramBinningRule.Sturges
                Return "(Sturges)"
            Case CategoricalHistogramBinningRule.Doane
                Return "(Doane)"
            Case CategoricalHistogramBinningRule.Scott
                Return "(Scott)"
            Case CategoricalHistogramBinningRule.FreedmanDiaconis
                Return "(Freedman-Diaconis)"
            Case Else
                Throw New ArgumentOutOfRangeException(NameOf(rule))
        End Select
    End Function

    Private Shared Function CountBins(values As IEnumerable(Of Double),
                                      binMinimum As Double,
                                      binMaximum As Double,
                                      binWidth As Double,
                                      binCount As Integer) As Double()
        Dim counts(binCount - 1) As Double

        For Each x As Double In values
            Dim index As Integer
            If x >= binMaximum Then
                index = binCount - 1
            Else
                index = Convert.ToInt32(Math.Floor((x - binMinimum) / binWidth))
                If index < 0 Then index = 0
                If index > binCount - 1 Then index = binCount - 1
            End If
            counts(index) += 1.0R
        Next

        Return counts
    End Function

    Private Shared Function ResolveBinWidth(midpoints As Double(), pooled As Double()) As Double
        If midpoints.Length > 1 Then
            Dim width As Double = midpoints(1) - midpoints(0)
            If IsFinitePositive(width) Then Return width
        End If

        'The ordinary histogram exposes midpoints/frequencies rather than the breaks.
        'For the uncommon one-bin case, reconstruct the same pretty-step logic closely
        'enough to preserve the current histogram behaviour and obtain a valid density.
        Dim dMin As Double = pooled.Min()
        Dim dMax As Double = pooled.Max()
        If dMax > dMin Then
            Return NiceStep125(dMax - dMin)
        End If

        Dim stepSize As Double = NiceStep125(Math.Abs(dMin))
        If Not IsFinitePositive(stepSize) Then stepSize = 1.0R
        Return 2.0R * stepSize
    End Function

    Private Shared Function NiceStep125(rawStep As Double) As Double
        rawStep = Math.Abs(rawStep)
        If rawStep = 0.0R OrElse Double.IsNaN(rawStep) OrElse Double.IsInfinity(rawStep) Then Return 1.0R

        Dim exponent As Double = Math.Floor(Math.Log10(rawStep))
        Dim fraction As Double = rawStep / (10.0R ^ exponent)
        Dim niceFraction As Double

        If fraction <= 1.0R Then
            niceFraction = 1.0R
        ElseIf fraction <= 2.0R Then
            niceFraction = 2.0R
        ElseIf fraction <= 5.0R Then
            niceFraction = 5.0R
        Else
            niceFraction = 10.0R
        End If

        Return niceFraction * (10.0R ^ exponent)
    End Function

    Private Shared Function IsMissingGroupValue(value As Object) As Boolean
        If value Is Nothing OrElse Convert.IsDBNull(value) Then Return True
        If TypeOf value Is String AndAlso String.IsNullOrWhiteSpace(DirectCast(value, String)) Then Return True

        Dim number As Double
        If TryGetNumericGroupValue(value, number) Then
            Return Double.IsNaN(number) OrElse Double.IsInfinity(number)
        End If
        Return False
    End Function

    ' CLR numeric categories and parseable worksheet text share a canonical key.
    ' Culture-aware text is parsed without requiring Microsoft.VisualBasic.IsNumeric.
    ' Dates and Booleans continue to be treated as ordinary categorical labels.
    Private Shared Function TryGetNumericGroupValue(value As Object, ByRef number As Double) As Boolean
        number = 0.0R
        If value Is Nothing OrElse Convert.IsDBNull(value) Then Return False

        Select Case Type.GetTypeCode(value.GetType())
            Case TypeCode.Byte, TypeCode.SByte, TypeCode.Int16, TypeCode.UInt16,
                 TypeCode.Int32, TypeCode.UInt32, TypeCode.Int64, TypeCode.UInt64,
                 TypeCode.Single, TypeCode.Double, TypeCode.Decimal
                Try
                    number = Convert.ToDouble(value, CultureInfo.InvariantCulture)
                    Return True
                Catch ex As OverflowException
                    Return False
                End Try
            Case TypeCode.String
                Dim numericText As String = DirectCast(value, String)
                Dim style As NumberStyles = NumberStyles.Float Or NumberStyles.AllowThousands
                Return Double.TryParse(numericText, style, CultureInfo.CurrentCulture, number) OrElse
                       Double.TryParse(numericText, style, CultureInfo.InvariantCulture, number)
            Case Else
                Return False
        End Select
    End Function

    Private Shared Function BuildGroupKey(value As Object) As String
        Dim numericValue As Double
        If TryGetNumericGroupValue(value, numericValue) Then
            Return "N:" & numericValue.ToString("R", CultureInfo.InvariantCulture)
        End If
        Return "S:" & Convert.ToString(value, CultureInfo.InvariantCulture).Trim()
    End Function

    Private Shared Function FormatGroupName(value As Object) As String
        Dim numericValue As Double
        If TryGetNumericGroupValue(value, numericValue) Then
            Return numericValue.ToString(CultureInfo.CurrentCulture)
        End If
        Return Convert.ToString(value, CultureInfo.CurrentCulture).Trim()
    End Function

    Private Shared Function IsFinitePositive(value As Double) As Boolean
        Return Not Double.IsNaN(value) AndAlso Not Double.IsInfinity(value) AndAlso value > 0.0R
    End Function
End Class
