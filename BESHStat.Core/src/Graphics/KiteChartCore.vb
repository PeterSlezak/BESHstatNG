Option Explicit On
Option Strict On
Option Infer On

Imports System
Imports System.Collections.Generic
Imports System.Globalization

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
''' Immutable numerical result returned by <see cref="KiteChartCore.Compute"/>.
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
Public Module KiteChartCore

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
