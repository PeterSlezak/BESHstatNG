Option Explicit On
Option Strict Off
Option Infer On

Imports System
Imports System.Collections.Generic
Imports System.Globalization
Imports BESHStatNG.AppInfrastructure
Imports Microsoft.Office.Interop.Excel

''' <summary>
''' Specifies how kite widths are scaled across the supplied series.
''' </summary>
Public Enum KiteScaleMode
    ''' <summary>
    ''' All series use one common maximum. Relative magnitudes are therefore comparable
    ''' both within and between series.
    ''' </summary>
    CommonMaximum

    ''' <summary>
    ''' Every series is scaled to its own maximum. This emphasizes the shape of each
    ''' distribution but removes between-series magnitude comparability.
    ''' </summary>
    PerSeriesMaximum
End Enum

''' <summary>
''' Specifies the optional transformation applied before kite widths are scaled.
''' </summary>
Public Enum KiteValueTransform
    ''' <summary>
    ''' Plot values without transformation.
    ''' </summary>
    Linear

    ''' <summary>
    ''' Plot the square root of each value. This is useful for moderately skewed counts.
    ''' </summary>
    SquareRoot

    ''' <summary>
    ''' Plot <c>Log(1 + value)</c>. This is useful when a few large observations would
    ''' otherwise dominate the chart.
    ''' </summary>
    LogOnePlus
End Enum

''' <summary>
''' Specifies how missing observations are represented in a kite chart.
''' </summary>
Public Enum KiteMissingValueMode
    ''' <summary>
    ''' Leave a gap at the missing position.
    ''' </summary>
    Gap

    ''' <summary>
    ''' Treat a missing observation as zero width.
    ''' </summary>
    Zero
End Enum

''' <summary>
''' Numerical options used to calculate the geometry of a kite chart.
''' </summary>
''' <remarks>
''' The implementation uses fixed centre lines and symmetric upper/lower boundaries.
''' Unlike spreadsheet recipes based on cumulative offsets, every plotted width is
''' derived directly from its corresponding source observation, so the kite cannot
''' drift away from the underlying data.
''' </remarks>
Public Class KiteChartOptions
    ''' <summary>
    ''' Gets or sets how values are normalized across series.
    ''' </summary>
    Public Property ScaleMode As KiteScaleMode = KiteScaleMode.CommonMaximum

    ''' <summary>
    ''' Gets or sets the transformation applied before normalization.
    ''' </summary>
    Public Property ValueTransform As KiteValueTransform = KiteValueTransform.Linear

    ''' <summary>
    ''' Gets or sets how missing observations are handled.
    ''' </summary>
    Public Property MissingValueMode As KiteMissingValueMode = KiteMissingValueMode.Gap

    ''' <summary>
    ''' Gets or sets the distance between adjacent series centre lines.
    ''' </summary>
    Public Property LaneSpacing As Double = 1.0R

    ''' <summary>
    ''' Gets or sets the maximum full kite width as a fraction of
    ''' <see cref="LaneSpacing"/>.
    ''' </summary>
    ''' <remarks>
    ''' A value of 0.8 leaves a 20% gap between adjacent lanes when both neighboring
    ''' series reach their maximum. The value must be greater than zero and no greater
    ''' than one.
    ''' </remarks>
    Public Property MaximumWidthFraction As Double = 0.8R

    ''' <summary>
    ''' Gets or sets whether a zero-width point is added before the first and after the
    ''' last source position so each filled area closes cleanly.
    ''' </summary>
    Public Property AddZeroEndpoints As Boolean = True

    ''' <summary>
    ''' Creates an independent copy of the options.
    ''' </summary>
    Friend Function Copy() As KiteChartOptions
        Return New KiteChartOptions With {
            .ScaleMode = ScaleMode,
            .ValueTransform = ValueTransform,
            .MissingValueMode = MissingValueMode,
            .LaneSpacing = LaneSpacing,
            .MaximumWidthFraction = MaximumWidthFraction,
            .AddZeroEndpoints = AddZeroEndpoints
        }
    End Function
End Class

''' <summary>
''' Contains the source and transformed geometry for one kite.
''' </summary>
Public NotInheritable Class KiteChartSeries
    Private ReadOnly _sourceIndex As Integer
    Private ReadOnly _name As String
    Private ReadOnly _sourceValues As Double()
    Private ReadOnly _transformedValues As Double()
    Private ReadOnly _halfWidths As Double()
    Private ReadOnly _upperBoundary As Double()
    Private ReadOnly _lowerBoundary As Double()
    Private ReadOnly _centerLineValues As Double()
    Private ReadOnly _centerLine As Double
    Private ReadOnly _sourceMaximum As Double
    Private ReadOnly _transformedMaximum As Double
    Private ReadOnly _validObservationCount As Integer
    Private ReadOnly _missingObservationCount As Integer

    Friend Sub New(sourceIndex As Integer,
                   name As String,
                   sourceValues As Double(),
                   transformedValues As Double(),
                   halfWidths As Double(),
                   upperBoundary As Double(),
                   lowerBoundary As Double(),
                   centerLineValues As Double(),
                   centerLine As Double,
                   sourceMaximum As Double,
                   transformedMaximum As Double,
                   validObservationCount As Integer,
                   missingObservationCount As Integer)
        _sourceIndex = sourceIndex
        _name = If(name, String.Empty)
        _sourceValues = DirectCast(sourceValues.Clone(), Double())
        _transformedValues = DirectCast(transformedValues.Clone(), Double())
        _halfWidths = DirectCast(halfWidths.Clone(), Double())
        _upperBoundary = DirectCast(upperBoundary.Clone(), Double())
        _lowerBoundary = DirectCast(lowerBoundary.Clone(), Double())
        _centerLineValues = DirectCast(centerLineValues.Clone(), Double())
        _centerLine = centerLine
        _sourceMaximum = sourceMaximum
        _transformedMaximum = transformedMaximum
        _validObservationCount = validObservationCount
        _missingObservationCount = missingObservationCount
    End Sub

    ''' <summary>
    ''' Gets the zero-based source column index.
    ''' </summary>
    Public ReadOnly Property SourceIndex As Integer
        Get
            Return _sourceIndex
        End Get
    End Property

    ''' <summary>
    ''' Gets the displayed series name.
    ''' </summary>
    Public ReadOnly Property Name As String
        Get
            Return _name
        End Get
    End Property

    ''' <summary>
    ''' Gets the unmodified source observations. Missing values are represented by
    ''' <see cref="Double.NaN"/>.
    ''' </summary>
    Public ReadOnly Property SourceValues As Double()
        Get
            Return DirectCast(_sourceValues.Clone(), Double())
        End Get
    End Property

    ''' <summary>
    ''' Gets observations after applying the selected value transformation but before
    ''' width normalization.
    ''' </summary>
    Public ReadOnly Property TransformedValues As Double()
        Get
            Return DirectCast(_transformedValues.Clone(), Double())
        End Get
    End Property

    ''' <summary>
    ''' Gets the plotted half-width at every plotted category, including optional
    ''' zero-width endpoints.
    ''' </summary>
    Public ReadOnly Property HalfWidths As Double()
        Get
            Return DirectCast(_halfWidths.Clone(), Double())
        End Get
    End Property

    ''' <summary>
    ''' Gets the upper boundary supplied to the visible Excel area series.
    ''' </summary>
    Public ReadOnly Property UpperBoundary As Double()
        Get
            Return DirectCast(_upperBoundary.Clone(), Double())
        End Get
    End Property

    ''' <summary>
    ''' Gets the lower boundary supplied to the masking Excel area series.
    ''' </summary>
    Public ReadOnly Property LowerBoundary As Double()
        Get
            Return DirectCast(_lowerBoundary.Clone(), Double())
        End Get
    End Property

    ''' <summary>
    ''' Gets an array containing the fixed centre-line value at every plotted category.
    ''' </summary>
    Public ReadOnly Property CenterLineValues As Double()
        Get
            Return DirectCast(_centerLineValues.Clone(), Double())
        End Get
    End Property

    ''' <summary>
    ''' Gets the fixed vertical centre line for this kite.
    ''' </summary>
    Public ReadOnly Property CenterLine As Double
        Get
            Return _centerLine
        End Get
    End Property

    ''' <summary>
    ''' Gets the largest finite source value, or zero when the series contains only
    ''' zeros and missing observations.
    ''' </summary>
    Public ReadOnly Property SourceMaximum As Double
        Get
            Return _sourceMaximum
        End Get
    End Property

    ''' <summary>
    ''' Gets the largest transformed value used for scaling.
    ''' </summary>
    Public ReadOnly Property TransformedMaximum As Double
        Get
            Return _transformedMaximum
        End Get
    End Property

    Public ReadOnly Property ValidObservationCount As Integer
        Get
            Return _validObservationCount
        End Get
    End Property

    Public ReadOnly Property MissingObservationCount As Integer
        Get
            Return _missingObservationCount
        End Get
    End Property
End Class

''' <summary>
''' Immutable numerical result returned by <see cref="KiteChart.Compute"/>.
''' </summary>
Public NotInheritable Class KiteChartResult
    Private ReadOnly _categories As Object()
    Private ReadOnly _series As KiteChartSeries()
    Private ReadOnly _options As KiteChartOptions
    Private ReadOnly _sourceCategoryCount As Integer
    Private ReadOnly _plottedCategoryCount As Integer
    Private ReadOnly _sourceObservationCount As Integer
    Private ReadOnly _validObservationCount As Integer
    Private ReadOnly _missingObservationCount As Integer
    Private ReadOnly _globalSourceMaximum As Double
    Private ReadOnly _globalTransformedMaximum As Double
    Private ReadOnly _maximumHalfWidth As Double
    Private ReadOnly _axisMinimum As Double
    Private ReadOnly _axisMaximum As Double

    Friend Sub New(categories As Object(),
                   series As KiteChartSeries(),
                   options As KiteChartOptions,
                   sourceCategoryCount As Integer,
                   plottedCategoryCount As Integer,
                   sourceObservationCount As Integer,
                   validObservationCount As Integer,
                   missingObservationCount As Integer,
                   globalSourceMaximum As Double,
                   globalTransformedMaximum As Double,
                   maximumHalfWidth As Double,
                   axisMinimum As Double,
                   axisMaximum As Double)
        _categories = DirectCast(categories.Clone(), Object())
        _series = DirectCast(series.Clone(), KiteChartSeries())
        _options = options.Copy()
        _sourceCategoryCount = sourceCategoryCount
        _plottedCategoryCount = plottedCategoryCount
        _sourceObservationCount = sourceObservationCount
        _validObservationCount = validObservationCount
        _missingObservationCount = missingObservationCount
        _globalSourceMaximum = globalSourceMaximum
        _globalTransformedMaximum = globalTransformedMaximum
        _maximumHalfWidth = maximumHalfWidth
        _axisMinimum = axisMinimum
        _axisMaximum = axisMaximum
    End Sub

    ''' <summary>
    ''' Gets plotted category labels, including blank labels for optional zero endpoints.
    ''' </summary>
    Public ReadOnly Property Categories As Object()
        Get
            Return DirectCast(_categories.Clone(), Object())
        End Get
    End Property

    ''' <summary>
    ''' Gets kites in source-column order. The first source column is placed at the top
    ''' of the chart.
    ''' </summary>
    Public ReadOnly Property Series As KiteChartSeries()
        Get
            Return DirectCast(_series.Clone(), KiteChartSeries())
        End Get
    End Property

    Public ReadOnly Property Options As KiteChartOptions
        Get
            Return _options.Copy()
        End Get
    End Property

    Public ReadOnly Property SourceCategoryCount As Integer
        Get
            Return _sourceCategoryCount
        End Get
    End Property

    Public ReadOnly Property PlottedCategoryCount As Integer
        Get
            Return _plottedCategoryCount
        End Get
    End Property

    Public ReadOnly Property SourceObservationCount As Integer
        Get
            Return _sourceObservationCount
        End Get
    End Property

    Public ReadOnly Property ValidObservationCount As Integer
        Get
            Return _validObservationCount
        End Get
    End Property

    Public ReadOnly Property MissingObservationCount As Integer
        Get
            Return _missingObservationCount
        End Get
    End Property

    Public ReadOnly Property GlobalSourceMaximum As Double
        Get
            Return _globalSourceMaximum
        End Get
    End Property

    Public ReadOnly Property GlobalTransformedMaximum As Double
        Get
            Return _globalTransformedMaximum
        End Get
    End Property

    Public ReadOnly Property MaximumHalfWidth As Double
        Get
            Return _maximumHalfWidth
        End Get
    End Property

    Public ReadOnly Property AxisMinimum As Double
        Get
            Return _axisMinimum
        End Get
    End Property

    Public ReadOnly Property AxisMaximum As Double
        Get
            Return _axisMaximum
        End Get
    End Property
End Class

''' <summary>
''' Computes symmetric kite-chart geometry from a position-by-series data matrix.
''' </summary>
''' <remarks>
''' Rows represent ordered sample positions (for example quadrats along a transect) and
''' columns represent species or other variables. Values must be nonnegative. Missing
''' observations are supplied as <see cref="Double.NaN"/>.
''' </remarks>
Public Module KiteChart

    ''' <summary>
    ''' Calculates one kite for every column of <paramref name="values"/>.
    ''' </summary>
    ''' <param name="values">
    ''' A two-dimensional matrix in which rows are ordered positions and columns are
    ''' plotted series.
    ''' </param>
    ''' <param name="seriesNames">
    ''' Optional names matching the matrix columns. Blank entries receive automatic names.
    ''' </param>
    ''' <param name="categoryLabels">
    ''' Optional one-dimensional labels matching the matrix rows. When omitted, row numbers
    ''' starting at one are used. Numeric positions are accepted but, because Excel area
    ''' charts use a category axis, they are displayed at equal horizontal spacing.
    ''' </param>
    ''' <param name="options">Optional calculation settings.</param>
    Public Function Compute(values As Double(,),
                            Optional seriesNames As String() = Nothing,
                            Optional categoryLabels As Array = Nothing,
                            Optional options As KiteChartOptions = Nothing) As KiteChartResult
        If values Is Nothing Then Throw New ArgumentNullException(NameOf(values))

        Dim rowCount As Integer = values.GetLength(0)
        Dim seriesCount As Integer = values.GetLength(1)
        If rowCount < 2 Then
            Throw New ArgumentException("A kite chart requires at least two ordered positions.", NameOf(values))
        End If
        If seriesCount < 1 Then
            Throw New ArgumentException("A kite chart requires at least one data series.", NameOf(values))
        End If

        Dim resolvedOptions As KiteChartOptions = If(options, New KiteChartOptions()).Copy()
        ValidateOptions(resolvedOptions)

        Dim resolvedNames As String() = ResolveSeriesNames(seriesNames, seriesCount)
        Dim sourceCategories As Object() = ResolveCategories(categoryLabels, rowCount)

        Dim sourceBySeries(seriesCount - 1)() As Double
        Dim transformedBySeries(seriesCount - 1)() As Double
        Dim sourceMaxima(seriesCount - 1) As Double
        Dim transformedMaxima(seriesCount - 1) As Double
        Dim validCounts(seriesCount - 1) As Integer
        Dim missingCounts(seriesCount - 1) As Integer

        Dim globalSourceMaximum As Double = 0.0R
        Dim globalTransformedMaximum As Double = 0.0R
        Dim totalValid As Integer = 0
        Dim totalMissing As Integer = 0
        Dim hasPositiveValue As Boolean = False

        For seriesIndex As Integer = 0 To seriesCount - 1
            Dim sourceValues(rowCount - 1) As Double
            Dim transformedValues(rowCount - 1) As Double
            Dim sourceMaximum As Double = 0.0R
            Dim transformedMaximum As Double = 0.0R
            Dim validCount As Integer = 0
            Dim missingCount As Integer = 0

            For rowIndex As Integer = 0 To rowCount - 1
                Dim value As Double = values(rowIndex, seriesIndex)
                If Double.IsNaN(value) Then
                    sourceValues(rowIndex) = Double.NaN
                    transformedValues(rowIndex) = Double.NaN
                    missingCount += 1
                    Continue For
                End If
                If Double.IsInfinity(value) Then
                    Throw New ArgumentOutOfRangeException(NameOf(values),
                                                          BuildCellMessage(rowIndex,
                                                                           seriesIndex,
                                                                           "contains an infinite value"))
                End If
                If value < 0.0R Then
                    Throw New ArgumentOutOfRangeException(NameOf(values),
                                                          BuildCellMessage(rowIndex,
                                                                           seriesIndex,
                                                                           "contains a negative value"))
                End If

                Dim transformed As Double = TransformValue(value, resolvedOptions.ValueTransform)
                sourceValues(rowIndex) = value
                transformedValues(rowIndex) = transformed
                validCount += 1

                If value > sourceMaximum Then sourceMaximum = value
                If transformed > transformedMaximum Then transformedMaximum = transformed
                If value > globalSourceMaximum Then globalSourceMaximum = value
                If transformed > globalTransformedMaximum Then globalTransformedMaximum = transformed
                If value > 0.0R Then hasPositiveValue = True
            Next

            sourceBySeries(seriesIndex) = sourceValues
            transformedBySeries(seriesIndex) = transformedValues
            sourceMaxima(seriesIndex) = sourceMaximum
            transformedMaxima(seriesIndex) = transformedMaximum
            validCounts(seriesIndex) = validCount
            missingCounts(seriesIndex) = missingCount
            totalValid += validCount
            totalMissing += missingCount
        Next

        If totalValid = 0 Then
            Throw New ArgumentException("The selected matrix contains no finite observations.", NameOf(values))
        End If
        If Not hasPositiveValue Then
            Throw New ArgumentException("A kite chart requires at least one positive observation.", NameOf(values))
        End If

        Dim maximumHalfWidth As Double = resolvedOptions.LaneSpacing *
                                         resolvedOptions.MaximumWidthFraction / 2.0R
        Dim plottedCategories As Object() = BuildPlottedCategories(sourceCategories,
                                                                    resolvedOptions.AddZeroEndpoints)
        Dim plottedCount As Integer = plottedCategories.Length
        Dim outputSeries(seriesCount - 1) As KiteChartSeries

        For seriesIndex As Integer = 0 To seriesCount - 1
            Dim centerLine As Double = (seriesCount - seriesIndex) * resolvedOptions.LaneSpacing
            Dim halfWidths(plottedCount - 1) As Double
            Dim upperBoundary(plottedCount - 1) As Double
            Dim lowerBoundary(plottedCount - 1) As Double
            Dim centerValues(plottedCount - 1) As Double
            Dim sourceOffset As Integer = If(resolvedOptions.AddZeroEndpoints, 1, 0)
            Dim denominator As Double = If(resolvedOptions.ScaleMode = KiteScaleMode.CommonMaximum,
                                           globalTransformedMaximum,
                                           transformedMaxima(seriesIndex))

            For plotIndex As Integer = 0 To plottedCount - 1
                centerValues(plotIndex) = centerLine
            Next

            If resolvedOptions.AddZeroEndpoints Then
                halfWidths(0) = 0.0R
                upperBoundary(0) = centerLine
                lowerBoundary(0) = centerLine
                halfWidths(plottedCount - 1) = 0.0R
                upperBoundary(plottedCount - 1) = centerLine
                lowerBoundary(plottedCount - 1) = centerLine
            End If

            For rowIndex As Integer = 0 To rowCount - 1
                Dim plotIndex As Integer = rowIndex + sourceOffset
                Dim transformed As Double = transformedBySeries(seriesIndex)(rowIndex)
                If Double.IsNaN(transformed) Then
                    If resolvedOptions.MissingValueMode = KiteMissingValueMode.Zero Then
                        halfWidths(plotIndex) = 0.0R
                        upperBoundary(plotIndex) = centerLine
                        lowerBoundary(plotIndex) = centerLine
                    Else
                        halfWidths(plotIndex) = Double.NaN
                        upperBoundary(plotIndex) = Double.NaN
                        lowerBoundary(plotIndex) = Double.NaN
                    End If
                Else
                    Dim halfWidth As Double = 0.0R
                    If denominator > 0.0R Then
                        halfWidth = maximumHalfWidth * transformed / denominator
                    End If
                    halfWidths(plotIndex) = halfWidth
                    upperBoundary(plotIndex) = centerLine + halfWidth
                    lowerBoundary(plotIndex) = centerLine - halfWidth
                End If
            Next

            outputSeries(seriesIndex) = New KiteChartSeries(seriesIndex,
                                                             resolvedNames(seriesIndex),
                                                             sourceBySeries(seriesIndex),
                                                             transformedBySeries(seriesIndex),
                                                             halfWidths,
                                                             upperBoundary,
                                                             lowerBoundary,
                                                             centerValues,
                                                             centerLine,
                                                             sourceMaxima(seriesIndex),
                                                             transformedMaxima(seriesIndex),
                                                             validCounts(seriesIndex),
                                                             missingCounts(seriesIndex))
        Next

        Dim axisMinimum As Double = 0.0R
        Dim axisMaximum As Double = (seriesCount + 1) * resolvedOptions.LaneSpacing
        Return New KiteChartResult(plottedCategories,
                                   outputSeries,
                                   resolvedOptions,
                                   rowCount,
                                   plottedCount,
                                   rowCount * seriesCount,
                                   totalValid,
                                   totalMissing,
                                   globalSourceMaximum,
                                   globalTransformedMaximum,
                                   maximumHalfWidth,
                                   axisMinimum,
                                   axisMaximum)
    End Function

    Private Sub ValidateOptions(options As KiteChartOptions)
        If Not [Enum].IsDefined(GetType(KiteScaleMode), options.ScaleMode) Then
            Throw New ArgumentOutOfRangeException(NameOf(options.ScaleMode), "The scale mode is not defined.")
        End If
        If Not [Enum].IsDefined(GetType(KiteValueTransform), options.ValueTransform) Then
            Throw New ArgumentOutOfRangeException(NameOf(options.ValueTransform), "The value transform is not defined.")
        End If
        If Not [Enum].IsDefined(GetType(KiteMissingValueMode), options.MissingValueMode) Then
            Throw New ArgumentOutOfRangeException(NameOf(options.MissingValueMode), "The missing-value mode is not defined.")
        End If
        If Not IsFinitePositive(options.LaneSpacing) Then
            Throw New ArgumentOutOfRangeException(NameOf(options.LaneSpacing),
                                                  "Lane spacing must be finite and positive.")
        End If
        If Not IsFinite(options.MaximumWidthFraction) OrElse
           options.MaximumWidthFraction <= 0.0R OrElse
           options.MaximumWidthFraction > 1.0R Then
            Throw New ArgumentOutOfRangeException(NameOf(options.MaximumWidthFraction),
                                                  "Maximum width fraction must be greater than zero and no greater than one.")
        End If
    End Sub

    Private Function ResolveSeriesNames(seriesNames As String(), seriesCount As Integer) As String()
        If seriesNames IsNot Nothing AndAlso seriesNames.Length <> seriesCount Then
            Throw New ArgumentException("The number of series names must equal the number of matrix columns.",
                                        NameOf(seriesNames))
        End If

        Dim result(seriesCount - 1) As String
        For i As Integer = 0 To seriesCount - 1
            Dim supplied As String = If(seriesNames Is Nothing, Nothing, seriesNames(i))
            If String.IsNullOrWhiteSpace(supplied) Then
                result(i) = "Series " & (i + 1).ToString(CultureInfo.CurrentCulture)
            Else
                result(i) = supplied.Trim()
            End If
        Next
        Return result
    End Function

    Private Function ResolveCategories(categoryLabels As Array, rowCount As Integer) As Object()
        Dim result(rowCount - 1) As Object
        If categoryLabels Is Nothing Then
            For i As Integer = 0 To rowCount - 1
                result(i) = i + 1
            Next
            Return result
        End If

        If categoryLabels.Rank <> 1 Then
            Throw New ArgumentException("Category labels must be a one-dimensional array.", NameOf(categoryLabels))
        End If
        If categoryLabels.Length <> rowCount Then
            Throw New ArgumentException("The number of category labels must equal the number of matrix rows.",
                                        NameOf(categoryLabels))
        End If

        Dim lowerBound As Integer = categoryLabels.GetLowerBound(0)
        For i As Integer = 0 To rowCount - 1
            Dim value As Object = categoryLabels.GetValue(lowerBound + i)
            If value Is Nothing OrElse value Is DBNull.Value Then
                result(i) = i + 1
            Else
                result(i) = value
            End If
        Next
        Return result
    End Function

    Private Function BuildPlottedCategories(sourceCategories As Object(), addEndpoints As Boolean) As Object()
        If Not addEndpoints Then Return DirectCast(sourceCategories.Clone(), Object())

        Dim result(sourceCategories.Length + 1) As Object
        result(0) = String.Empty
        For i As Integer = 0 To sourceCategories.Length - 1
            result(i + 1) = sourceCategories(i)
        Next
        result(result.Length - 1) = String.Empty
        Return result
    End Function

    Private Function TransformValue(value As Double, transform As KiteValueTransform) As Double
        Select Case transform
            Case KiteValueTransform.Linear
                Return value
            Case KiteValueTransform.SquareRoot
                Return Math.Sqrt(value)
            Case KiteValueTransform.LogOnePlus
                Return Math.Log(1.0R + value)
            Case Else
                Throw New ArgumentOutOfRangeException(NameOf(transform))
        End Select
    End Function

    Private Function BuildCellMessage(rowIndex As Integer,
                                      seriesIndex As Integer,
                                      problem As String) As String
        Return "Observation at row " & (rowIndex + 1).ToString(CultureInfo.CurrentCulture) &
               ", series " & (seriesIndex + 1).ToString(CultureInfo.CurrentCulture) &
               " " & problem & "."
    End Function

    Private Function IsFinite(value As Double) As Boolean
        Return Not Double.IsNaN(value) AndAlso Not Double.IsInfinity(value)
    End Function

    Private Function IsFinitePositive(value As Double) As Boolean
        Return IsFinite(value) AndAlso value > 0.0R
    End Function
End Module

''' <summary>
''' Optional per-series Excel appearance override. Only populated properties replace
''' the corresponding chart defaults.
''' </summary>
Public Class KiteSeriesAppearance
    Public Property SeriesName As String
    Public Property FillColor As Nullable(Of Integer)
    Public Property FillTransparency As Nullable(Of Single)
    Public Property OutlineColor As Nullable(Of Integer)
    Public Property OutlineWeight As Nullable(Of Single)
    Public Property CenterLineColor As Nullable(Of Integer)
    Public Property CenterLineWeight As Nullable(Of Single)
    Public Property CenterLineStyle As Nullable(Of XlLineStyle)
End Class

''' <summary>
''' Excel chart appearance settings used by <see cref="KiteChartExcel"/>.
''' </summary>
''' <remarks>Colors are OLE RGB integers used by the Excel object model.</remarks>
Public Class KiteChartAppearance
    Public Property ChartTitle As String = "Kite chart"
    Public Property XAxisTitle As String = "Position"
    Public Property SeriesAxisTitle As String = String.Empty

    ''' <summary>
    ''' Shows one legend entry for each visible kite.
    ''' </summary>
    Public Property ShowLegend As Boolean = False

    ''' <summary>
    ''' Places each series name beside its centre line at the left of the plot.
    ''' </summary>
    Public Property ShowSeriesLabels As Boolean = True

    Public Property ShowCenterLines As Boolean = True
    Public Property ShowVerticalGridlines As Boolean = False
    Public Property ShowHorizontalGridlines As Boolean = False
    Public Property ShowValueAxisLabels As Boolean = False
    Public Property ShowOutline As Boolean = True

    Public Property FillTransparency As Single = 0.08F
    Public Property OutlineWeight As Single = 1.0F
    Public Property CenterLineWeight As Single = 0.75F
    Public Property CenterLineStyle As XlLineStyle = XlLineStyle.xlDot

    Public Property SeriesLabelFontSize As Single = 9.0F
    Public Property SeriesLabelBold As Boolean = True

    ''' <summary>
    ''' Gets or sets the category-label rotation in degrees from -90 to 90.
    ''' </summary>
    Public Property CategoryLabelRotation As Integer = 0

    Public Property LegendPosition As XlLegendPosition = XlLegendPosition.xlLegendPositionBottom

    Public Property SeriesColors As Integer() = {
        &HB4771F, &HE7FFF, &H2CA02C, &H2827D6, &HBD6794,
        &H4B568C, &HC277E3, &H7F7F7F, &H22BDBC, &HCFBE17
    }

    Public Property BackgroundColor As Integer = &HFFFFFF
    Public Property GridlineColor As Integer = &HE6E6E6
    Public Property OutlineColor As Nullable(Of Integer) = Nothing
    Public Property CenterLineColor As Integer = &H808080
    Public Property TextColor As Integer = &H333333

    ''' <summary>
    ''' Gets or sets optional per-series overrides matched case-insensitively by name.
    ''' </summary>
    Public Property SeriesOverrides As KiteSeriesAppearance() = New KiteSeriesAppearance() {}
End Class

''' <summary>
''' Renders a <see cref="KiteChartResult"/> as an embedded Excel area chart.
''' </summary>
''' <remarks>
''' Every kite is drawn as a visible upper area followed by an opaque background-colored
''' lower area. This creates a filled band between the two symmetric boundaries without
''' writing helper columns to the worksheet. Pairs are ordered from the top lane to the
''' bottom lane so the masks do not interfere when the default non-overlapping geometry
''' is used.
''' </remarks>
Public NotInheritable Class KiteChartExcel
    Private Sub New()
    End Sub

    Private NotInheritable Class ResolvedSeriesStyle
        Friend FillColor As Integer
        Friend FillTransparency As Single
        Friend OutlineColor As Integer
        Friend OutlineWeight As Single
        Friend CenterLineColor As Integer
        Friend CenterLineWeight As Single
        Friend CenterLineStyle As XlLineStyle
    End Class

    ''' <summary>
    ''' Creates an embedded kite chart on the supplied worksheet.
    ''' </summary>
    Public Shared Function AddChart(ws As Worksheet,
                                    result As KiteChartResult,
                                    Optional appearance As KiteChartAppearance = Nothing,
                                    Optional left As Double = 20.0R,
                                    Optional top As Double = 20.0R,
                                    Optional width As Double = 720.0R,
                                    Optional height As Double = 440.0R) As Chart
        If ws Is Nothing Then Throw New ArgumentNullException(NameOf(ws))
        If result Is Nothing Then Throw New ArgumentNullException(NameOf(result))
        If Not IsFinite(left) OrElse Not IsFinite(top) Then
            Throw New ArgumentOutOfRangeException(NameOf(left), "Chart position must be finite.")
        End If
        If Not IsFinitePositive(width) OrElse Not IsFinitePositive(height) Then
            Throw New ArgumentOutOfRangeException(NameOf(width),
                                                  "Chart width and height must be finite and positive.")
        End If

        Dim resolvedAppearance As KiteChartAppearance = If(appearance, New KiteChartAppearance())
        ValidateAppearance(resolvedAppearance)

        Dim chartShape As Shape = Nothing
        Try
            chartShape = ws.Shapes.AddChart(XlChartType.xlArea, left, top, width, height)
            Dim chart As Chart = chartShape.Chart
            chart.ChartType = XlChartType.xlArea
            chart.DisplayBlanksAs = XlDisplayBlanksAs.xlNotPlotted
            chart.PlotVisibleOnly = False
            chart.ChartArea.AutoScaleFont = False

            Dim seriesCollection As SeriesCollection = DirectCast(chart.SeriesCollection(), SeriesCollection)
            DeleteAllSeries(seriesCollection)
            ConfigureBackground(chart, resolvedAppearance)
            ConfigureTitle(chart, resolvedAppearance.ChartTitle)

            Dim legendSeriesIndices As New List(Of Integer)()
            Dim categories As Object() = result.Categories
            Dim kiteSeries As KiteChartSeries() = result.Series

            For seriesIndex As Integer = 0 To kiteSeries.Length - 1
                Dim kite As KiteChartSeries = kiteSeries(seriesIndex)
                Dim style As ResolvedSeriesStyle = ResolveStyle(kite.Name,
                                                                seriesIndex,
                                                                resolvedAppearance)

                Dim visibleSeriesIndex As Integer = AddAreaSeries(seriesCollection,
                                                                  kite.Name,
                                                                  categories,
                                                                  kite.UpperBoundary,
                                                                  style.FillColor,
                                                                  style.FillTransparency,
                                                                  resolvedAppearance.ShowOutline,
                                                                  style.OutlineColor,
                                                                  style.OutlineWeight)
                legendSeriesIndices.Add(visibleSeriesIndex)

                AddMaskSeries(seriesCollection,
                              kite.Name & " mask",
                              categories,
                              kite.LowerBoundary,
                              resolvedAppearance.BackgroundColor)

                If resolvedAppearance.ShowOutline Then
                    AddBoundaryLineSeries(seriesCollection,
                                          kite.Name & " lower outline",
                                          categories,
                                          kite.LowerBoundary,
                                          style.OutlineColor,
                                          style.OutlineWeight)
                End If

                If resolvedAppearance.ShowCenterLines Then
                    AddCenterLineSeries(seriesCollection,
                                        kite.Name & " centre line",
                                        categories,
                                        kite.CenterLineValues,
                                        style.CenterLineColor,
                                        style.CenterLineWeight,
                                        style.CenterLineStyle)
                End If

                If resolvedAppearance.ShowSeriesLabels Then
                    AddSeriesLabel(seriesCollection,
                                   kite.Name,
                                   categories,
                                   kite.CenterLine,
                                   style.FillColor,
                                   resolvedAppearance)
                End If
            Next

            ConfigureAxes(chart, result, resolvedAppearance)
            ConfigureLegend(chart,
                            legendSeriesIndices,
                            resolvedAppearance.ShowLegend,
                            resolvedAppearance.LegendPosition)
            chart.Refresh()
            Return chart
        Catch
            If chartShape IsNot Nothing Then
                Try
                    chartShape.Delete()
                Catch
                End Try
            End If
            Throw
        End Try
    End Function

    Private Shared Sub ValidateAppearance(appearance As KiteChartAppearance)
        If appearance.SeriesColors Is Nothing OrElse appearance.SeriesColors.Length = 0 Then
            Throw New ArgumentException("SeriesColors must contain at least one color.",
                                        NameOf(appearance.SeriesColors))
        End If
        If Not IsFiniteFraction(appearance.FillTransparency) Then
            Throw New ArgumentOutOfRangeException(NameOf(appearance.FillTransparency),
                                                  "Fill transparency must be between zero and one.")
        End If
        If Not IsFinitePositive(appearance.OutlineWeight) Then
            Throw New ArgumentOutOfRangeException(NameOf(appearance.OutlineWeight),
                                                  "Outline weight must be finite and positive.")
        End If
        If Not IsFinitePositive(appearance.CenterLineWeight) Then
            Throw New ArgumentOutOfRangeException(NameOf(appearance.CenterLineWeight),
                                                  "Centre-line weight must be finite and positive.")
        End If
        If appearance.CenterLineStyle = XlLineStyle.xlLineStyleNone AndAlso appearance.ShowCenterLines Then
            Throw New ArgumentException("CenterLineStyle must be visible when ShowCenterLines is enabled.",
                                        NameOf(appearance.CenterLineStyle))
        End If
        If Not IsFinitePositive(appearance.SeriesLabelFontSize) Then
            Throw New ArgumentOutOfRangeException(NameOf(appearance.SeriesLabelFontSize),
                                                  "Series-label font size must be finite and positive.")
        End If
        If appearance.CategoryLabelRotation < -90 OrElse appearance.CategoryLabelRotation > 90 Then
            Throw New ArgumentOutOfRangeException(NameOf(appearance.CategoryLabelRotation),
                                                  "Category-label rotation must be between -90 and 90 degrees.")
        End If

        If appearance.SeriesOverrides IsNot Nothing Then
            For Each item As KiteSeriesAppearance In appearance.SeriesOverrides
                If item Is Nothing Then Continue For
                If item.FillTransparency.HasValue AndAlso Not IsFiniteFraction(item.FillTransparency.Value) Then
                    Throw New ArgumentOutOfRangeException(NameOf(item.FillTransparency),
                                                          "Series fill transparency must be between zero and one.")
                End If
                If item.OutlineWeight.HasValue AndAlso Not IsFinitePositive(item.OutlineWeight.Value) Then
                    Throw New ArgumentOutOfRangeException(NameOf(item.OutlineWeight),
                                                          "Series outline weight must be finite and positive.")
                End If
                If item.CenterLineWeight.HasValue AndAlso Not IsFinitePositive(item.CenterLineWeight.Value) Then
                    Throw New ArgumentOutOfRangeException(NameOf(item.CenterLineWeight),
                                                          "Series centre-line weight must be finite and positive.")
                End If
                If item.CenterLineStyle.HasValue AndAlso
                   appearance.ShowCenterLines AndAlso
                   item.CenterLineStyle.Value = XlLineStyle.xlLineStyleNone Then
                    Throw New ArgumentException("A series centre-line style must be visible when centre lines are enabled.",
                                                NameOf(item.CenterLineStyle))
                End If
            Next
        End If
    End Sub

    Private Shared Sub DeleteAllSeries(seriesCollection As SeriesCollection)
        Do While seriesCollection.Count > 0
            DirectCast(seriesCollection.Item(1), Series).Delete()
        Loop
    End Sub

    Private Shared Function AddAreaSeries(seriesCollection As SeriesCollection,
                                          seriesName As String,
                                          categories As Object(),
                                          values As Double(),
                                          fillColor As Integer,
                                          fillTransparency As Single,
                                          showOutline As Boolean,
                                          outlineColor As Integer,
                                          outlineWeight As Single) As Integer
        'Dim series As Series = DirectCast(seriesCollection.NewSeries(), Series)
        seriesCollection.NewSeries()
        With seriesCollection(seriesCollection.Count - 1)
            .Name = seriesName
            .ChartType = XlChartType.xlArea
            .XValues = categories
            .Values = ToChartValues(values)
            .Format.Fill.Visible = True
            .Format.Fill.Solid()
            .Format.Fill.ForeColor.RGB = fillColor
            .Format.Fill.Transparency = fillTransparency

            If showOutline Then
                .Format.Line.Visible = True
                .Format.Line.ForeColor.RGB = outlineColor
                .Format.Line.Weight = outlineWeight
                .Border.Color = outlineColor
            Else
                .Format.Line.Visible = False
                .Border.LineStyle = XlLineStyle.xlLineStyleNone
            End If
        End With

        Return seriesCollection.Count
    End Function

    Private Shared Sub AddMaskSeries(seriesCollection As SeriesCollection,
                                     seriesName As String,
                                     categories As Object(),
                                     values As Double(),
                                     backgroundColor As Integer)
        'Dim series As Series = DirectCast(seriesCollection.NewSeries(), Series)
        seriesCollection.NewSeries()
        With seriesCollection(seriesCollection.Count - 1)
            .Name = seriesName
            .ChartType = XlChartType.xlArea
            .XValues = categories
            .Values = ToChartValues(values)
            .Format.Fill.Visible = True
            .Format.Fill.Solid()
            .Format.Fill.ForeColor.RGB = backgroundColor
            .Format.Fill.Transparency = 0.0F
            .Format.Line.Visible = False
            .Border.LineStyle = XlLineStyle.xlLineStyleNone
        End With
    End Sub


    Private Shared Sub AddBoundaryLineSeries(seriesCollection As SeriesCollection,
                                             seriesName As String,
                                             categories As Object(),
                                             values As Double(),
                                             color As Integer,
                                             weight As Single)
        'Dim series As Series = DirectCast(seriesCollection.NewSeries(), Series)
        seriesCollection.NewSeries()
        With seriesCollection(seriesCollection.Count - 1)
            .Name = seriesName
            .ChartType = XlChartType.xlLine
            .XValues = categories
            .Values = ToChartValues(values)
            .MarkerStyle = XlMarkerStyle.xlMarkerStyleNone
            .Format.Fill.Visible = False
            .Format.Line.Visible = True
            .Format.Line.ForeColor.RGB = color
            .Format.Line.Weight = weight
            .Border.Color = color
            .Border.LineStyle = XlLineStyle.xlContinuous
        End With
    End Sub

    Private Shared Sub AddCenterLineSeries(seriesCollection As SeriesCollection,
                                           seriesName As String,
                                           categories As Object(),
                                           values As Double(),
                                           color As Integer,
                                           weight As Single,
                                           lineStyle As XlLineStyle)
        'Dim series As Series = DirectCast(seriesCollection.NewSeries(), Series)
        seriesCollection.NewSeries()
        With seriesCollection(seriesCollection.Count - 1)
            .Name = seriesName
            .ChartType = XlChartType.xlLine
            .XValues = categories
            .Values = ToChartValues(values)
            .MarkerStyle = XlMarkerStyle.xlMarkerStyleNone
            .Format.Fill.Visible = False
            .Format.Line.Visible = True
            .Format.Line.ForeColor.RGB = color
            .Format.Line.Weight = weight
            .Border.Color = color
            .Border.LineStyle = lineStyle
        End With
    End Sub

    Private Shared Sub AddSeriesLabel(seriesCollection As SeriesCollection,
                                      seriesName As String,
                                      categories As Object(),
                                      centerLine As Double,
                                      color As Integer,
                                      appearance As KiteChartAppearance)
        'A label helper needs only one plotted point. Using a full-length array
        'with #N/A placeholders causes DISP_E_TYPEMISMATCH when assigned through
        'the Excel Interop Series.Values property.
        Dim labelCategories(0) As Object
        labelCategories(0) = categories(0)

        Dim labelValues(0) As Double
        labelValues(0) = centerLine

        'Dim series As Series = DirectCast(seriesCollection.NewSeries(), Series)
        seriesCollection.NewSeries()
        With seriesCollection(seriesCollection.Count - 1)
            .Name = seriesName & " label"
            .ChartType = XlChartType.xlLine
            .XValues = labelCategories
            .Values = labelValues
            .MarkerStyle = XlMarkerStyle.xlMarkerStyleNone
            .Format.Fill.Visible = False
            .Format.Line.Visible = False
            .Border.LineStyle = XlLineStyle.xlLineStyleNone

            Try
                .ApplyDataLabels()
                Dim point As Point = DirectCast(.Points(1), Point)
                point.DataLabel.Text = seriesName
                point.DataLabel.Position = XlDataLabelPosition.xlLabelPositionAbove ' XlDataLabelPosition.xlLabelPositionLeft
                point.DataLabel.Font.Size = appearance.SeriesLabelFontSize
                point.DataLabel.Font.Bold = appearance.SeriesLabelBold
                point.DataLabel.Font.Color = If(color = appearance.BackgroundColor,
                                            appearance.TextColor,
                                            color)
            Catch
                'Series labels are helpful but should never prevent chart creation.
            End Try
        End With
    End Sub

    Private Shared Function ToChartValues(values As Double()) As Object()
        Dim result(values.Length - 1) As Object
        For i As Integer = 0 To values.Length - 1
            If IsFinite(values(i)) Then
                result(i) = values(i)
            Else
                'Nothing marshals as a blank VARIANT and is accepted by Series.Values.
                'Chart.DisplayBlanksAs controls whether the point is plotted.
                result(i) = Nothing
            End If
        Next
        Return result
    End Function

    Private Shared Function ResolveStyle(seriesName As String,
                                         seriesIndex As Integer,
                                         appearance As KiteChartAppearance) As ResolvedSeriesStyle
        Dim fillColor As Integer = appearance.SeriesColors(seriesIndex Mod appearance.SeriesColors.Length)
        Dim result As New ResolvedSeriesStyle With {
            .FillColor = fillColor,
            .FillTransparency = appearance.FillTransparency,
            .OutlineColor = If(appearance.OutlineColor.HasValue,
                               appearance.OutlineColor.Value,
                               fillColor),
            .OutlineWeight = appearance.OutlineWeight,
            .CenterLineColor = appearance.CenterLineColor,
            .CenterLineWeight = appearance.CenterLineWeight,
            .CenterLineStyle = appearance.CenterLineStyle
        }

        Dim overrideStyle As KiteSeriesAppearance = FindOverride(seriesName, appearance.SeriesOverrides)
        If overrideStyle IsNot Nothing Then
            If overrideStyle.FillColor.HasValue Then result.FillColor = overrideStyle.FillColor.Value
            If overrideStyle.FillTransparency.HasValue Then result.FillTransparency = overrideStyle.FillTransparency.Value
            If overrideStyle.OutlineColor.HasValue Then result.OutlineColor = overrideStyle.OutlineColor.Value
            If overrideStyle.OutlineWeight.HasValue Then result.OutlineWeight = overrideStyle.OutlineWeight.Value
            If overrideStyle.CenterLineColor.HasValue Then result.CenterLineColor = overrideStyle.CenterLineColor.Value
            If overrideStyle.CenterLineWeight.HasValue Then result.CenterLineWeight = overrideStyle.CenterLineWeight.Value
            If overrideStyle.CenterLineStyle.HasValue Then result.CenterLineStyle = overrideStyle.CenterLineStyle.Value
        End If
        Return result
    End Function

    Private Shared Function FindOverride(seriesName As String,
                                         ovrrides As KiteSeriesAppearance()) As KiteSeriesAppearance
        If ovrrides Is Nothing Then Return Nothing
        For Each item As KiteSeriesAppearance In ovrrides
            If item IsNot Nothing AndAlso
               Not String.IsNullOrWhiteSpace(item.SeriesName) AndAlso
               String.Equals(item.SeriesName.Trim(),
                             seriesName,
                             StringComparison.CurrentCultureIgnoreCase) Then
                Return item
            End If
        Next
        Return Nothing
    End Function

    Private Shared Sub ConfigureTitle(chart As Chart, title As String)
        If String.IsNullOrWhiteSpace(title) Then
            chart.HasTitle = False
        Else
            chart.HasTitle = True
            chart.ChartTitle.Text = title.Trim()
        End If
    End Sub

    Private Shared Sub ConfigureBackground(chart As Object, appearance As KiteChartAppearance)
        chart.ChartArea.Format.Fill.Visible = True
        chart.ChartArea.Format.Fill.Solid()
        chart.ChartArea.Format.Fill.ForeColor.RGB = appearance.BackgroundColor
        chart.PlotArea.Format.Fill.Visible = True
        chart.PlotArea.Format.Fill.Solid()
        chart.PlotArea.Format.Fill.ForeColor.RGB = appearance.BackgroundColor
    End Sub

    Private Shared Sub ConfigureAxes(chart As Chart,
                                     result As KiteChartResult,
                                     appearance As KiteChartAppearance)
        Dim categoryAxis As Axis = DirectCast(chart.Axes(XlAxisType.xlCategory,
                                                         XlAxisGroup.xlPrimary), Axis)
        categoryAxis.HasTitle = Not String.IsNullOrWhiteSpace(appearance.XAxisTitle)
        If categoryAxis.HasTitle Then categoryAxis.AxisTitle.Text = appearance.XAxisTitle.Trim()
        categoryAxis.AxisBetweenCategories = False
        categoryAxis.HasMajorGridlines = appearance.ShowVerticalGridlines
        categoryAxis.TickLabels.Orientation = appearance.CategoryLabelRotation
        If appearance.ShowVerticalGridlines Then
            Try
                categoryAxis.MajorGridlines.Border.Color = appearance.GridlineColor
            Catch
            End Try
        End If

        Dim valueAxis As Axis = DirectCast(chart.Axes(XlAxisType.xlValue,
                                                      XlAxisGroup.xlPrimary), Axis)
        valueAxis.MinimumScale = result.AxisMinimum
        valueAxis.MaximumScale = result.AxisMaximum
        valueAxis.MajorUnit = result.Options.LaneSpacing
        valueAxis.HasTitle = Not String.IsNullOrWhiteSpace(appearance.SeriesAxisTitle)
        If valueAxis.HasTitle Then valueAxis.AxisTitle.Text = appearance.SeriesAxisTitle.Trim()
        valueAxis.HasMajorGridlines = appearance.ShowHorizontalGridlines
        valueAxis.TickLabelPosition = If(appearance.ShowValueAxisLabels,
                                         XlTickLabelPosition.xlTickLabelPositionNextToAxis,
                                         XlTickLabelPosition.xlTickLabelPositionNone)
        If appearance.ShowHorizontalGridlines Then
            Try
                valueAxis.MajorGridlines.Border.Color = appearance.GridlineColor
            Catch
            End Try
        End If
    End Sub

    Private Shared Sub ConfigureLegend(chart As Chart,
                                       keepSeriesIndices As IList(Of Integer),
                                       showLegend As Boolean,
                                       legendPosition As XlLegendPosition)
        If Not showLegend OrElse keepSeriesIndices Is Nothing OrElse keepSeriesIndices.Count = 0 Then
            chart.HasLegend = False
            Return
        End If

        chart.HasLegend = True
        chart.Legend.Position = legendPosition
        Dim keep As New HashSet(Of Integer)(keepSeriesIndices)
        Dim entries As LegendEntries = DirectCast(chart.Legend.LegendEntries(), LegendEntries)
        For i As Integer = entries.Count To 1 Step -1
            If Not keep.Contains(i) Then
                DirectCast(entries.Item(i), LegendEntry).Delete()
            End If
        Next
    End Sub

    Private Shared Function IsFinite(value As Double) As Boolean
        Return Not Double.IsNaN(value) AndAlso Not Double.IsInfinity(value)
    End Function

    Private Shared Function IsFinitePositive(value As Double) As Boolean
        Return IsFinite(value) AndAlso value > 0.0R
    End Function

    Private Shared Function IsFinitePositive(value As Single) As Boolean
        Return Not Single.IsNaN(value) AndAlso Not Single.IsInfinity(value) AndAlso value > 0.0F
    End Function

    Private Shared Function IsFiniteFraction(value As Single) As Boolean
        Return Not Single.IsNaN(value) AndAlso
               Not Single.IsInfinity(value) AndAlso
               value >= 0.0F AndAlso value <= 1.0F
    End Function
End Class
