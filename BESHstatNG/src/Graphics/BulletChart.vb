Option Explicit On
Option Strict Off
Option Infer On

Imports System
Imports System.Collections.Generic
Imports System.Globalization

''' <summary>
''' Indicates whether larger or smaller values represent better performance for a bullet-chart item.
''' The setting affects only the desirability ordering of the qualitative bands; values are always
''' laid out from zero at the left to the resolved scale maximum at the right.
''' </summary>
Public Enum BulletChartDirection
    ''' <summary>Higher values represent better performance.</summary>
    HigherIsBetter = 0

    ''' <summary>Lower values represent better performance.</summary>
    LowerIsBetter = 1
End Enum

''' <summary>
''' Excel-independent preparation options for <see cref="BulletChart"/>.
''' Appearance settings such as colours, fonts, bar thicknesses and chart dimensions belong in the
''' Excel renderer rather than in this class.
''' </summary>
Public Class BulletChartOptions
    ''' <summary>
    ''' When False (default), a missing measure label, actual value, target value, or fewer than two
    ''' qualitative ranges causes validation to fail. When True, such incomplete rows are omitted.
    ''' Invalid non-missing values still raise an exception.
    ''' </summary>
    Public Property OmitIncompleteRows As Boolean = False

    ''' <summary>
    ''' Direction used when no row-specific direction is supplied. Default: HigherIsBetter.
    ''' </summary>
    Public Property DefaultDirection As BulletChartDirection = BulletChartDirection.HigherIsBetter

    ''' <summary>
    ''' Approximate number of major intervals used when the major interval is calculated automatically.
    ''' The actual number can differ because a human-readable 1/2/2.5/5/10 x power-of-ten interval is
    ''' selected. Accepted range: 3 to 20. Default: 6.
    ''' </summary>
    Public Property TargetMajorTickCount As Integer = 6

    Friend Function Copy() As BulletChartOptions
        Return New BulletChartOptions With {
            .OmitIncompleteRows = OmitIncompleteRows,
            .DefaultDirection = DefaultDirection,
            .TargetMajorTickCount = TargetMajorTickCount
        }
    End Function
End Class

''' <summary>
''' One qualitative background band in a bullet chart.
''' </summary>
Public NotInheritable Class BulletChartBand
    Private ReadOnly _bandIndex As Integer
    Private ReadOnly _desirabilityRank As Integer
    Private ReadOnly _lowerBound As Double
    Private ReadOnly _upperBound As Double
    Private ReadOnly _normalizedStart As Double
    Private ReadOnly _normalizedEnd As Double
    Private ReadOnly _isExtendedFinalBand As Boolean

    Friend Sub New(bandIndex As Integer,
                   desirabilityRank As Integer,
                   lowerBound As Double,
                   upperBound As Double,
                   normalizedStart As Double,
                   normalizedEnd As Double,
                   isExtendedFinalBand As Boolean)
        _bandIndex = bandIndex
        _desirabilityRank = desirabilityRank
        _lowerBound = lowerBound
        _upperBound = upperBound
        _normalizedStart = normalizedStart
        _normalizedEnd = normalizedEnd
        _isExtendedFinalBand = isExtendedFinalBand
    End Sub

    ''' <summary>Zero-based left-to-right band index.</summary>
    Public ReadOnly Property BandIndex As Integer
        Get
            Return _bandIndex
        End Get
    End Property

    ''' <summary>
    ''' Zero-based desirability rank where 0 is the least desirable band and RangeCount - 1 is the
    ''' most desirable band. This lets a renderer reverse the shading automatically for LowerIsBetter.
    ''' </summary>
    Public ReadOnly Property DesirabilityRank As Integer
        Get
            Return _desirabilityRank
        End Get
    End Property

    ''' <summary>Lower value represented by the band.</summary>
    Public ReadOnly Property LowerBound As Double
        Get
            Return _lowerBound
        End Get
    End Property

    ''' <summary>Upper value represented by the band.</summary>
    Public ReadOnly Property UpperBound As Double
        Get
            Return _upperBound
        End Get
    End Property

    ''' <summary>Band start as a fraction of the item's scale width, in [0, 1].</summary>
    Public ReadOnly Property NormalizedStart As Double
        Get
            Return _normalizedStart
        End Get
    End Property

    ''' <summary>Band end as a fraction of the item's scale width, in [0, 1].</summary>
    Public ReadOnly Property NormalizedEnd As Double
        Get
            Return _normalizedEnd
        End Get
    End Property

    ''' <summary>
    ''' True when the final qualitative band has been extended beyond the last supplied qualitative
    ''' boundary because the resolved scale maximum is larger.
    ''' </summary>
    Public ReadOnly Property IsExtendedFinalBand As Boolean
        Get
            Return _isExtendedFinalBand
        End Get
    End Property
End Class

''' <summary>
''' Immutable prepared row returned by <see cref="BulletChart.Compute"/>.
''' </summary>
Public NotInheritable Class BulletChartItem
    Private ReadOnly _sourceIndex As Integer
    Private ReadOnly _displayIndex As Integer
    Private ReadOnly _measureLabel As String
    Private ReadOnly _subtitle As String
    Private ReadOnly _actualValue As Double
    Private ReadOnly _targetValue As Double
    Private ReadOnly _direction As BulletChartDirection
    Private ReadOnly _qualitativeRangeBoundaries As Double()
    Private ReadOnly _bands As BulletChartBand()
    Private ReadOnly _scaleMaximum As Double
    Private ReadOnly _majorInterval As Double
    Private ReadOnly _scaleTicks As Double()
    Private ReadOnly _scaleMaximumWasAutomatic As Boolean
    Private ReadOnly _majorIntervalWasAutomatic As Boolean

    Friend Sub New(sourceIndex As Integer,
                   displayIndex As Integer,
                   measureLabel As String,
                   subtitle As String,
                   actualValue As Double,
                   targetValue As Double,
                   direction As BulletChartDirection,
                   qualitativeRangeBoundaries As Double(),
                   bands As BulletChartBand(),
                   scaleMaximum As Double,
                   majorInterval As Double,
                   scaleTicks As Double(),
                   scaleMaximumWasAutomatic As Boolean,
                   majorIntervalWasAutomatic As Boolean)
        _sourceIndex = sourceIndex
        _displayIndex = displayIndex
        _measureLabel = measureLabel
        _subtitle = subtitle
        _actualValue = actualValue
        _targetValue = targetValue
        _direction = direction
        _qualitativeRangeBoundaries = DirectCast(qualitativeRangeBoundaries.Clone(), Double())
        _bands = DirectCast(bands.Clone(), BulletChartBand())
        _scaleMaximum = scaleMaximum
        _majorInterval = majorInterval
        _scaleTicks = DirectCast(scaleTicks.Clone(), Double())
        _scaleMaximumWasAutomatic = scaleMaximumWasAutomatic
        _majorIntervalWasAutomatic = majorIntervalWasAutomatic
    End Sub

    ''' <summary>Zero-based position in the original source vectors.</summary>
    Public ReadOnly Property SourceIndex As Integer
        Get
            Return _sourceIndex
        End Get
    End Property

    ''' <summary>Zero-based position among retained bullet-chart rows.</summary>
    Public ReadOnly Property DisplayIndex As Integer
        Get
            Return _displayIndex
        End Get
    End Property

    ''' <summary>Main left-side label, for example Revenue.</summary>
    Public ReadOnly Property MeasureLabel As String
        Get
            Return _measureLabel
        End Get
    End Property

    ''' <summary>Optional secondary label such as a unit or explanatory caption.</summary>
    Public ReadOnly Property Subtitle As String
        Get
            Return _subtitle
        End Get
    End Property

    ''' <summary>True when a non-empty subtitle was supplied.</summary>
    Public ReadOnly Property HasSubtitle As Boolean
        Get
            Return _subtitle.Length > 0
        End Get
    End Property

    ''' <summary>Primary featured measure shown by the dark horizontal bar.</summary>
    Public ReadOnly Property ActualValue As Double
        Get
            Return _actualValue
        End Get
    End Property

    ''' <summary>Comparative/target value shown by the target marker.</summary>
    Public ReadOnly Property TargetValue As Double
        Get
            Return _targetValue
        End Get
    End Property

    ''' <summary>Whether high or low values are more desirable.</summary>
    Public ReadOnly Property Direction As BulletChartDirection
        Get
            Return _direction
        End Get
    End Property

    ''' <summary>Convenience flag equivalent to Direction = HigherIsBetter.</summary>
    Public ReadOnly Property HigherIsBetter As Boolean
        Get
            Return _direction = BulletChartDirection.HigherIsBetter
        End Get
    End Property

    ''' <summary>
    ''' Cumulative upper boundaries supplied for the qualitative ranges. Between two and five values
    ''' are retained. The last supplied value is the qualitative reference maximum; when the plotting
    ''' scale must extend farther, the final visual band is extended to ScaleMaximum.
    ''' </summary>
    Public ReadOnly Property QualitativeRangeBoundaries As Double()
        Get
            Return DirectCast(_qualitativeRangeBoundaries.Clone(), Double())
        End Get
    End Property

    ''' <summary>Number of qualitative bands, from 2 to 5.</summary>
    Public ReadOnly Property QualitativeRangeCount As Integer
        Get
            Return _qualitativeRangeBoundaries.Length
        End Get
    End Property

    ''' <summary>Last cumulative qualitative boundary supplied by the caller.</summary>
    Public ReadOnly Property QualitativeMaximum As Double
        Get
            Return _qualitativeRangeBoundaries(_qualitativeRangeBoundaries.Length - 1)
        End Get
    End Property

    ''' <summary>Prepared qualitative bands in left-to-right order.</summary>
    Public ReadOnly Property Bands As BulletChartBand()
        Get
            Return DirectCast(_bands.Clone(), BulletChartBand())
        End Get
    End Property

    ''' <summary>Scale minimum. The current bullet-chart backend uses a zero baseline.</summary>
    Public ReadOnly Property ScaleMinimum As Double
        Get
            Return 0.0R
        End Get
    End Property

    ''' <summary>Resolved right-side scale maximum for this item.</summary>
    Public ReadOnly Property ScaleMaximum As Double
        Get
            Return _scaleMaximum
        End Get
    End Property

    ''' <summary>Resolved major tick interval for this item.</summary>
    Public ReadOnly Property MajorInterval As Double
        Get
            Return _majorInterval
        End Get
    End Property

    ''' <summary>
    ''' Scale ticks beginning at zero. The final ScaleMaximum is included even when it is not an exact
    ''' multiple of MajorInterval.
    ''' </summary>
    Public ReadOnly Property ScaleTicks As Double()
        Get
            Return DirectCast(_scaleTicks.Clone(), Double())
        End Get
    End Property

    ''' <summary>True when the scale maximum was calculated rather than explicitly supplied.</summary>
    Public ReadOnly Property ScaleMaximumWasAutomatic As Boolean
        Get
            Return _scaleMaximumWasAutomatic
        End Get
    End Property

    ''' <summary>True when the major interval was calculated rather than explicitly supplied.</summary>
    Public ReadOnly Property MajorIntervalWasAutomatic As Boolean
        Get
            Return _majorIntervalWasAutomatic
        End Get
    End Property

    ''' <summary>Actual value as a fraction of the available plotting width.</summary>
    Public ReadOnly Property ActualNormalized As Double
        Get
            Return _actualValue / _scaleMaximum
        End Get
    End Property

    ''' <summary>Target value as a fraction of the available plotting width.</summary>
    Public ReadOnly Property TargetNormalized As Double
        Get
            Return _targetValue / _scaleMaximum
        End Get
    End Property

    ''' <summary>True when the actual value is beyond the last supplied qualitative boundary.</summary>
    Public ReadOnly Property ActualExceedsQualitativeMaximum As Boolean
        Get
            Return _actualValue > QualitativeMaximum
        End Get
    End Property

    ''' <summary>True when the target value is beyond the last supplied qualitative boundary.</summary>
    Public ReadOnly Property TargetExceedsQualitativeMaximum As Boolean
        Get
            Return _targetValue > QualitativeMaximum
        End Get
    End Property

    ''' <summary>
    ''' True when the plotting scale is larger than the last supplied qualitative boundary.
    ''' In that case the final qualitative band is deliberately continued to the scale maximum.
    ''' </summary>
    Public ReadOnly Property ScaleExtendedBeyondQualitativeMaximum As Boolean
        Get
            Return _scaleMaximum > QualitativeMaximum AndAlso
                   Not BulletChart.NearlyEqualForPublicUse(_scaleMaximum, QualitativeMaximum)
        End Get
    End Property
End Class

''' <summary>
''' Immutable result from the Excel-independent bullet-chart preparation engine.
''' </summary>
Public NotInheritable Class BulletChartResult
    Private ReadOnly _items As BulletChartItem()
    Private ReadOnly _sourceItemCount As Integer
    Private ReadOnly _omittedItemCount As Integer
    Private ReadOnly _options As BulletChartOptions
    Private ReadOnly _maximumQualitativeRangeCount As Integer
    Private ReadOnly _hasSubtitles As Boolean
    Private ReadOnly _hasExtendedScales As Boolean

    Friend Sub New(items As BulletChartItem(),
                   sourceItemCount As Integer,
                   omittedItemCount As Integer,
                   options As BulletChartOptions)
        _items = DirectCast(items.Clone(), BulletChartItem())
        _sourceItemCount = sourceItemCount
        _omittedItemCount = omittedItemCount
        _options = options.Copy()

        Dim maxRanges As Integer = 0
        Dim subtitles As Boolean = False
        Dim extended As Boolean = False
        For Each item As BulletChartItem In _items
            maxRanges = Math.Max(maxRanges, item.QualitativeRangeCount)
            subtitles = subtitles OrElse item.HasSubtitle
            extended = extended OrElse item.ScaleExtendedBeyondQualitativeMaximum
        Next

        _maximumQualitativeRangeCount = maxRanges
        _hasSubtitles = subtitles
        _hasExtendedScales = extended
    End Sub

    ''' <summary>Prepared rows in source order after optional omission of incomplete rows.</summary>
    Public ReadOnly Property Items As BulletChartItem()
        Get
            Return DirectCast(_items.Clone(), BulletChartItem())
        End Get
    End Property

    ''' <summary>Number of retained bullet-chart rows.</summary>
    Public ReadOnly Property ItemCount As Integer
        Get
            Return _items.Length
        End Get
    End Property

    ''' <summary>Number of rows presented to the backend before omission.</summary>
    Public ReadOnly Property SourceItemCount As Integer
        Get
            Return _sourceItemCount
        End Get
    End Property

    ''' <summary>Number of incomplete rows omitted when OmitIncompleteRows is enabled.</summary>
    Public ReadOnly Property OmittedItemCount As Integer
        Get
            Return _omittedItemCount
        End Get
    End Property

    ''' <summary>Largest qualitative range count among retained rows.</summary>
    Public ReadOnly Property MaximumQualitativeRangeCount As Integer
        Get
            Return _maximumQualitativeRangeCount
        End Get
    End Property

    ''' <summary>True when at least one retained row contains a subtitle.</summary>
    Public ReadOnly Property HasSubtitles As Boolean
        Get
            Return _hasSubtitles
        End Get
    End Property

    ''' <summary>True when at least one final qualitative band was extended to a larger plot scale.</summary>
    Public ReadOnly Property HasExtendedScales As Boolean
        Get
            Return _hasExtendedScales
        End Get
    End Property

    ''' <summary>Validated options used to prepare the result.</summary>
    Public ReadOnly Property Options As BulletChartOptions
        Get
            Return _options.Copy()
        End Get
    End Property
End Class

''' <summary>
''' Excel-independent backend for horizontal bullet charts.
'''
''' The backend validates row-aligned measure labels, actual values, targets and cumulative
''' qualitative-range boundaries; resolves an independent zero-based scale and readable major tick
''' interval for every item; and returns normalized geometry that an Excel renderer can draw with
''' chart-local Shapes. It contains no Excel or Excel-DNA dependencies.
''' </summary>
Public NotInheritable Class BulletChart
    ''' <summary>Minimum supported number of qualitative bands per item.</summary>
    Public Const MinimumQualitativeRanges As Integer = 2

    ''' <summary>Maximum supported number of qualitative bands per item.</summary>
    Public Const MaximumQualitativeRanges As Integer = 5

    Private Const MaximumGeneratedTicks As Integer = 50
    Private Const ScaleToleranceFactor As Double = 1.0E-10R

    Private Sub New()
    End Sub

    ''' <summary>
    ''' Prepares a multi-row bullet chart.
    ''' </summary>
    ''' <param name="measureLabels">
    ''' Main labels, one per bullet row. One-dimensional arrays and single-row/single-column
    ''' two-dimensional arrays are accepted.
    ''' </param>
    ''' <param name="actualValues">Featured/actual values aligned with measureLabels.</param>
    ''' <param name="targetValues">Target/comparative values aligned with measureLabels.</param>
    ''' <param name="qualitativeRanges">
    ''' Two-dimensional matrix whose rows align with measureLabels and whose columns contain cumulative
    ''' qualitative upper boundaries. Supply 2 to 5 columns. Trailing missing cells permit different
    ''' rows to use different range counts; gaps before a later non-missing boundary are not allowed.
    ''' Example: 200, 300, 400 creates three qualitative bands 0-200, 200-300 and 300-scaleMaximum.
    ''' </param>
    ''' <param name="subtitles">
    ''' Optional secondary labels/units aligned with measureLabels. Missing values become empty text.
    ''' </param>
    ''' <param name="scaleMaximums">
    ''' Optional row-aligned explicit scale maxima. Missing cells request automatic scaling. An explicit
    ''' maximum must contain the actual value, target value and last qualitative boundary.
    ''' </param>
    ''' <param name="majorIntervals">
    ''' Optional row-aligned major tick intervals. Missing cells request a readable automatic interval.
    ''' </param>
    ''' <param name="directions">
    ''' Optional row-aligned desirability direction. Values may be BulletChartDirection, Boolean
    ''' (True = HigherIsBetter), 0/1, or text such as "Higher is better" / "Lower is better".
    ''' Missing cells use options.DefaultDirection.
    ''' </param>
    ''' <param name="options">Optional preparation settings.</param>
    Public Shared Function Compute(measureLabels As Array,
                                   actualValues As Array,
                                   targetValues As Array,
                                   qualitativeRanges As Array,
                                   Optional subtitles As Array = Nothing,
                                   Optional scaleMaximums As Array = Nothing,
                                   Optional majorIntervals As Array = Nothing,
                                   Optional directions As Array = Nothing,
                                   Optional options As BulletChartOptions = Nothing) As BulletChartResult
        If measureLabels Is Nothing Then Throw New ArgumentNullException(NameOf(measureLabels))
        If actualValues Is Nothing Then Throw New ArgumentNullException(NameOf(actualValues))
        If targetValues Is Nothing Then Throw New ArgumentNullException(NameOf(targetValues))
        If qualitativeRanges Is Nothing Then Throw New ArgumentNullException(NameOf(qualitativeRanges))

        Dim resolvedOptions As BulletChartOptions = If(options Is Nothing,
                                                        New BulletChartOptions(),
                                                        options.Copy())
        ValidateOptions(resolvedOptions)

        Dim labelRaw As Object() = CopyVector(measureLabels, NameOf(measureLabels))
        Dim actualRaw As Object() = CopyVector(actualValues, NameOf(actualValues))
        Dim targetRaw As Object() = CopyVector(targetValues, NameOf(targetValues))

        If labelRaw.Length = 0 Then
            Throw New ArgumentException("At least one bullet-chart row is required.", NameOf(measureLabels))
        End If
        RequireSameLength(actualRaw, labelRaw.Length, NameOf(actualValues))
        RequireSameLength(targetRaw, labelRaw.Length, NameOf(targetValues))

        ValidateQualitativeRangeMatrix(qualitativeRanges, labelRaw.Length)

        Dim subtitleRaw As Object() = CopyOptionalVector(subtitles, NameOf(subtitles), labelRaw.Length)
        Dim scaleMaximumRaw As Object() = CopyOptionalVector(scaleMaximums, NameOf(scaleMaximums), labelRaw.Length)
        Dim majorIntervalRaw As Object() = CopyOptionalVector(majorIntervals, NameOf(majorIntervals), labelRaw.Length)
        Dim directionRaw As Object() = CopyOptionalVector(directions, NameOf(directions), labelRaw.Length)

        Dim rangeRowLower As Integer = qualitativeRanges.GetLowerBound(0)
        Dim rangeColumnLower As Integer = qualitativeRanges.GetLowerBound(1)
        Dim rangeColumns As Integer = qualitativeRanges.GetLength(1)

        Dim prepared As New List(Of BulletChartItem)()
        Dim omittedCount As Integer = 0

        For rowIndex As Integer = 0 To labelRaw.Length - 1
            Dim measureLabel As String = String.Empty
            Dim hasLabel As Boolean = TryConvertLabel(labelRaw(rowIndex), measureLabel)

            Dim actualValue As Double
            Dim targetValue As Double
            Dim hasActual As Boolean = TryConvertNumericValue(actualRaw(rowIndex),
                                                              actualValue,
                                                              NameOf(actualValues),
                                                              rowIndex)
            Dim hasTarget As Boolean = TryConvertNumericValue(targetRaw(rowIndex),
                                                              targetValue,
                                                              NameOf(targetValues),
                                                              rowIndex)

            Dim boundaries As Double() = ReadQualitativeBoundaries(qualitativeRanges,
                                                                   rangeRowLower + rowIndex,
                                                                   rangeColumnLower,
                                                                   rangeColumns,
                                                                   rowIndex)
            ValidateBoundaries(boundaries, rowIndex)

            If Not hasLabel OrElse Not hasActual OrElse Not hasTarget OrElse
               boundaries.Length < MinimumQualitativeRanges Then
                If resolvedOptions.OmitIncompleteRows Then
                    omittedCount += 1
                    Continue For
                End If

                ThrowIncompleteRow(rowIndex,
                                   hasLabel,
                                   hasActual,
                                   hasTarget,
                                   boundaries.Length)
            End If

            If actualValue < 0.0R Then
                Throw New ArgumentOutOfRangeException(NameOf(actualValues),
                                                      "Actual value at row " & RowText(rowIndex) &
                                                      " is negative. The current bullet-chart scale starts at zero.")
            End If
            If targetValue < 0.0R Then
                Throw New ArgumentOutOfRangeException(NameOf(targetValues),
                                                      "Target value at row " & RowText(rowIndex) &
                                                      " is negative. The current bullet-chart scale starts at zero.")
            End If

            Dim subtitle As String = String.Empty
            If subtitleRaw IsNot Nothing Then
                subtitle = ConvertOptionalText(subtitleRaw(rowIndex))
            End If

            Dim explicitScaleMaximum As Nullable(Of Double) = Nothing
            If scaleMaximumRaw IsNot Nothing Then
                explicitScaleMaximum = ReadOptionalPositiveValue(scaleMaximumRaw(rowIndex),
                                                                 NameOf(scaleMaximums),
                                                                 rowIndex)
            End If

            Dim explicitMajorInterval As Nullable(Of Double) = Nothing
            If majorIntervalRaw IsNot Nothing Then
                explicitMajorInterval = ReadOptionalPositiveValue(majorIntervalRaw(rowIndex),
                                                                  NameOf(majorIntervals),
                                                                  rowIndex)
            End If

            Dim direction As BulletChartDirection = resolvedOptions.DefaultDirection
            If directionRaw IsNot Nothing Then
                direction = ParseDirection(directionRaw(rowIndex),
                                           resolvedOptions.DefaultDirection,
                                           NameOf(directions),
                                           rowIndex)
            End If

            Dim qualitativeMaximum As Double = boundaries(boundaries.Length - 1)
            Dim requiredMaximum As Double = Math.Max(qualitativeMaximum,
                                                     Math.Max(actualValue, targetValue))

            If explicitScaleMaximum.HasValue AndAlso explicitScaleMaximum.Value < requiredMaximum Then
                If NearlyEqual(explicitScaleMaximum.Value, requiredMaximum) Then
                    'Avoid a microscopic floating-point under-run that could otherwise place an
                    'actual/target/boundary fraction just above 1.0.
                    explicitScaleMaximum = requiredMaximum
                Else
                    Throw New ArgumentOutOfRangeException(NameOf(scaleMaximums),
                                                          "Scale maximum at row " & RowText(rowIndex) &
                                                          " must be greater than or equal to the actual value, target value, " &
                                                          "and final qualitative boundary (required minimum " &
                                                          requiredMaximum.ToString("G", CultureInfo.CurrentCulture) & ").")
                End If
            End If

            Dim scaleMaximum As Double
            Dim majorInterval As Double
            ResolveScale(requiredMaximum,
                         explicitScaleMaximum,
                         explicitMajorInterval,
                         resolvedOptions.TargetMajorTickCount,
                         scaleMaximum,
                         majorInterval)

            Dim ticks As Double() = BuildScaleTicks(scaleMaximum,
                                                    majorInterval,
                                                    NameOf(majorIntervals),
                                                    rowIndex)
            Dim bands As BulletChartBand() = BuildBands(boundaries,
                                                        scaleMaximum,
                                                        direction)

            prepared.Add(New BulletChartItem(rowIndex,
                                             prepared.Count,
                                             measureLabel,
                                             subtitle,
                                             actualValue,
                                             targetValue,
                                             direction,
                                             boundaries,
                                             bands,
                                             scaleMaximum,
                                             majorInterval,
                                             ticks,
                                             Not explicitScaleMaximum.HasValue,
                                             Not explicitMajorInterval.HasValue))
        Next

        If prepared.Count = 0 Then
            Throw New ArgumentException("No usable bullet-chart rows remain after applying the missing-value rules.",
                                        NameOf(measureLabels))
        End If

        Return New BulletChartResult(prepared.ToArray(),
                                     labelRaw.Length,
                                     omittedCount,
                                     resolvedOptions)
    End Function

    ''' <summary>
    ''' Convenience overload for the common case with labels, actual values, targets and qualitative
    ''' ranges only. All scales and major intervals are resolved automatically.
    ''' </summary>
    Public Shared Function ComputeBasic(measureLabels As Array,
                                        actualValues As Array,
                                        targetValues As Array,
                                        qualitativeRanges As Array,
                                        Optional options As BulletChartOptions = Nothing) As BulletChartResult
        Return Compute(measureLabels,
                       actualValues,
                       targetValues,
                       qualitativeRanges,
                       Nothing,
                       Nothing,
                       Nothing,
                       Nothing,
                       options)
    End Function

    Private Shared Sub ValidateOptions(options As BulletChartOptions)
        If Not [Enum].IsDefined(GetType(BulletChartDirection), options.DefaultDirection) Then
            Throw New ArgumentOutOfRangeException(NameOf(options.DefaultDirection),
                                                  "The default bullet-chart direction is not defined.")
        End If

        If options.TargetMajorTickCount < 3 OrElse options.TargetMajorTickCount > 20 Then
            Throw New ArgumentOutOfRangeException(NameOf(options.TargetMajorTickCount),
                                                  "TargetMajorTickCount must be between 3 and 20.")
        End If
    End Sub

    Private Shared Sub ValidateQualitativeRangeMatrix(qualitativeRanges As Array,
                                                      expectedRows As Integer)
        If qualitativeRanges.Rank <> 2 Then
            Throw New ArgumentException("Qualitative ranges must be supplied as a two-dimensional matrix.",
                                        NameOf(qualitativeRanges))
        End If

        Dim rows As Integer = qualitativeRanges.GetLength(0)
        Dim columns As Integer = qualitativeRanges.GetLength(1)

        If rows <> expectedRows Then
            Throw New ArgumentException("Qualitative-range row count must match the measure-label row count.",
                                        NameOf(qualitativeRanges))
        End If
        If columns < MinimumQualitativeRanges OrElse columns > MaximumQualitativeRanges Then
            Throw New ArgumentException("Qualitative ranges must contain between " &
                                        MinimumQualitativeRanges.ToString(CultureInfo.InvariantCulture) & " and " &
                                        MaximumQualitativeRanges.ToString(CultureInfo.InvariantCulture) & " columns.",
                                        NameOf(qualitativeRanges))
        End If
    End Sub

    Private Shared Function ReadQualitativeBoundaries(values As Array,
                                                      absoluteRow As Integer,
                                                      columnLower As Integer,
                                                      columnCount As Integer,
                                                      sourceRowIndex As Integer) As Double()
        Dim boundaries As New List(Of Double)(columnCount)
        Dim missingSeen As Boolean = False

        For columnOffset As Integer = 0 To columnCount - 1
            Dim rawValue As Object = values.GetValue(absoluteRow, columnLower + columnOffset)
            Dim value As Double
            Dim hasValue As Boolean = TryConvertNumericValue(rawValue,
                                                             value,
                                                             NameOf(values),
                                                             sourceRowIndex,
                                                             columnOffset)

            If Not hasValue Then
                missingSeen = True
                Continue For
            End If

            If missingSeen Then
                Throw New ArgumentException("Qualitative ranges at row " & RowText(sourceRowIndex) &
                                            " contain a gap. Missing range cells are allowed only after the final supplied boundary.",
                                            NameOf(values))
            End If

            boundaries.Add(value)
        Next

        Return boundaries.ToArray()
    End Function

    Private Shared Sub ValidateBoundaries(boundaries As Double(), sourceRowIndex As Integer)
        'The minimum count is handled by the incomplete-row rule so that OmitIncompleteRows can
        'discard genuinely incomplete rows. Non-missing boundaries are still validated here.
        Dim previous As Double = 0.0R
        For i As Integer = 0 To boundaries.Length - 1
            Dim current As Double = boundaries(i)
            If current <= 0.0R Then
                Throw New ArgumentOutOfRangeException("qualitativeRanges",
                                                      "Qualitative boundary " & (i + 1).ToString(CultureInfo.CurrentCulture) &
                                                      " at row " & RowText(sourceRowIndex) &
                                                      " must be greater than zero.")
            End If
            If i > 0 AndAlso current <= previous Then
                Throw New ArgumentException("Qualitative boundaries at row " & RowText(sourceRowIndex) &
                                            " must be strictly increasing cumulative upper limits.",
                                            "qualitativeRanges")
            End If
            previous = current
        Next
    End Sub

    Private Shared Sub ResolveScale(requiredMaximum As Double,
                                    explicitScaleMaximum As Nullable(Of Double),
                                    explicitMajorInterval As Nullable(Of Double),
                                    targetMajorTickCount As Integer,
                                    ByRef scaleMaximum As Double,
                                    ByRef majorInterval As Double)
        If Not IsFinitePositive(requiredMaximum) Then
            Throw New ArgumentOutOfRangeException(NameOf(requiredMaximum),
                                                  "The bullet-chart range is too large to form a finite positive scale.")
        End If

        If explicitScaleMaximum.HasValue Then
            scaleMaximum = explicitScaleMaximum.Value
        Else
            Dim provisionalInterval As Double
            If explicitMajorInterval.HasValue Then
                provisionalInterval = explicitMajorInterval.Value
            Else
                provisionalInterval = NiceStep(requiredMaximum / CDbl(targetMajorTickCount))
            End If

            scaleMaximum = CeilingToMultiple(requiredMaximum, provisionalInterval)
        End If

        If explicitMajorInterval.HasValue Then
            majorInterval = explicitMajorInterval.Value
        Else
            majorInterval = NiceStep(scaleMaximum / CDbl(targetMajorTickCount))
        End If

        If Not IsFinitePositive(scaleMaximum) OrElse Not IsFinitePositive(majorInterval) Then
            Throw New ArgumentOutOfRangeException(NameOf(requiredMaximum),
                                                  "The bullet-chart values are too large to form a finite scale.")
        End If
    End Sub

    ''' <summary>
    ''' Rounds a positive raw interval to a nearby 1, 2, 2.5, 5, or 10 multiple of a power of ten.
    ''' </summary>
    Private Shared Function NiceStep(rawStep As Double) As Double
        If Not IsFinitePositive(rawStep) Then Return 1.0R

        Dim exponent As Double = Math.Floor(Math.Log10(rawStep))
        Dim magnitude As Double = Math.Pow(10.0R, exponent)
        If Not IsFinitePositive(magnitude) Then Return rawStep

        Dim fraction As Double = rawStep / magnitude
        Dim niceFraction As Double

        If fraction <= 1.5R Then
            niceFraction = 1.0R
        ElseIf fraction <= 2.25R Then
            niceFraction = 2.0R
        ElseIf fraction <= 3.5R Then
            niceFraction = 2.5R
        ElseIf fraction <= 7.5R Then
            niceFraction = 5.0R
        Else
            niceFraction = 10.0R
        End If

        Dim result As Double = niceFraction * magnitude
        If Not IsFinitePositive(result) Then Return rawStep
        Return result
    End Function

    Private Shared Function CeilingToMultiple(value As Double, interval As Double) As Double
        Dim quotient As Double = value / interval
        If Double.IsNaN(quotient) OrElse Double.IsInfinity(quotient) Then
            Throw New ArgumentOutOfRangeException(NameOf(value),
                                                  "The bullet-chart values are too large to scale.")
        End If

        Dim tolerance As Double = ScaleTolerance(quotient)
        Dim intervalCount As Double = Math.Ceiling(quotient - tolerance)
        If intervalCount < 1.0R Then intervalCount = 1.0R

        Dim result As Double = intervalCount * interval
        If result + ScaleTolerance(value) < value Then result += interval
        Return result
    End Function

    Private Shared Function BuildScaleTicks(scaleMaximum As Double,
                                            majorInterval As Double,
                                            parameterName As String,
                                            rowIndex As Integer) As Double()
        Dim ticks As New List(Of Double)()
        ticks.Add(0.0R)

        Dim tolerance As Double = ScaleTolerance(scaleMaximum)
        Dim estimatedIntervals As Double = scaleMaximum / majorInterval
        If Double.IsNaN(estimatedIntervals) OrElse Double.IsInfinity(estimatedIntervals) OrElse
           estimatedIntervals > CDbl(MaximumGeneratedTicks) + 1.0R Then
            Throw New ArgumentOutOfRangeException(parameterName,
                                                  "Major interval at row " & RowText(rowIndex) &
                                                  " creates too many scale ticks; at most " &
                                                  MaximumGeneratedTicks.ToString(CultureInfo.CurrentCulture) &
                                                  " are supported.")
        End If

        Dim i As Integer = 1
        Do
            Dim value As Double = CDbl(i) * majorInterval
            If Not IsFinite(value) Then
                Throw New ArgumentOutOfRangeException(parameterName,
                                                      "Major interval at row " & RowText(rowIndex) &
                                                      " produces non-finite scale ticks.")
            End If

            If value >= scaleMaximum - tolerance Then Exit Do
            ticks.Add(value)

            If ticks.Count >= MaximumGeneratedTicks Then
                Throw New ArgumentOutOfRangeException(parameterName,
                                                      "Major interval at row " & RowText(rowIndex) &
                                                      " creates too many scale ticks; at most " &
                                                      MaximumGeneratedTicks.ToString(CultureInfo.CurrentCulture) &
                                                      " are supported.")
            End If
            i += 1
        Loop

        If Not NearlyEqual(ticks(ticks.Count - 1), scaleMaximum) Then
            ticks.Add(scaleMaximum)
        End If

        If ticks.Count > MaximumGeneratedTicks Then
            Throw New ArgumentOutOfRangeException(parameterName,
                                                  "Major interval at row " & RowText(rowIndex) &
                                                  " creates too many scale ticks; at most " &
                                                  MaximumGeneratedTicks.ToString(CultureInfo.CurrentCulture) &
                                                  " are supported.")
        End If

        Return ticks.ToArray()
    End Function

    Private Shared Function BuildBands(boundaries As Double(),
                                       scaleMaximum As Double,
                                       direction As BulletChartDirection) As BulletChartBand()
        Dim result(boundaries.Length - 1) As BulletChartBand

        For i As Integer = 0 To boundaries.Length - 1
            Dim lower As Double = If(i = 0, 0.0R, boundaries(i - 1))
            Dim upper As Double = If(i = boundaries.Length - 1,
                                     scaleMaximum,
                                     boundaries(i))
            Dim desirabilityRank As Integer
            If direction = BulletChartDirection.HigherIsBetter Then
                desirabilityRank = i
            Else
                desirabilityRank = boundaries.Length - 1 - i
            End If

            Dim isExtendedFinalBand As Boolean =
                (i = boundaries.Length - 1 AndAlso
                 scaleMaximum > boundaries(boundaries.Length - 1) AndAlso
                 Not NearlyEqual(scaleMaximum, boundaries(boundaries.Length - 1)))

            result(i) = New BulletChartBand(i,
                                            desirabilityRank,
                                            lower,
                                            upper,
                                            lower / scaleMaximum,
                                            upper / scaleMaximum,
                                            isExtendedFinalBand)
        Next

        Return result
    End Function

    Private Shared Function ParseDirection(rawValue As Object,
                                           defaultDirection As BulletChartDirection,
                                           parameterName As String,
                                           rowIndex As Integer) As BulletChartDirection
        If IsMissingValue(rawValue) Then Return defaultDirection

        If TypeOf rawValue Is BulletChartDirection Then
            Dim enumValue As BulletChartDirection = DirectCast(rawValue, BulletChartDirection)
            If [Enum].IsDefined(GetType(BulletChartDirection), enumValue) Then Return enumValue
        End If

        If TypeOf rawValue Is Boolean Then
            Return If(CBool(rawValue),
                      BulletChartDirection.HigherIsBetter,
                      BulletChartDirection.LowerIsBetter)
        End If

        If IsNumeric(rawValue) Then
            Dim numericValue As Double = Convert.ToDouble(rawValue, CultureInfo.InvariantCulture)
            If IsFinite(numericValue) AndAlso Math.Truncate(numericValue) = numericValue Then
                Dim integerValue As Integer
                If numericValue >= Integer.MinValue AndAlso numericValue <= Integer.MaxValue Then
                    integerValue = CInt(numericValue)
                    If [Enum].IsDefined(GetType(BulletChartDirection), integerValue) Then
                        Return CType(integerValue, BulletChartDirection)
                    End If
                End If
            End If
        End If

        Dim text As String = Convert.ToString(rawValue, CultureInfo.CurrentCulture)
        If text Is Nothing Then text = String.Empty
        Dim key As String = text.Trim().ToLowerInvariant().
                            Replace(" ", String.Empty).
                            Replace("-", String.Empty).
                            Replace("_", String.Empty)

        Select Case key
            Case "higher", "higherisbetter", "high", "maximize", "maximise", "up"
                Return BulletChartDirection.HigherIsBetter
            Case "lower", "lowerisbetter", "low", "minimize", "minimise", "down"
                Return BulletChartDirection.LowerIsBetter
        End Select

        Throw New ArgumentException("Direction at row " & RowText(rowIndex) &
                                    " is not recognized. Use HigherIsBetter/LowerIsBetter, True/False, 0/1, " &
                                    "or equivalent text.",
                                    parameterName)
    End Function

    Private Shared Function ReadOptionalPositiveValue(rawValue As Object,
                                                      parameterName As String,
                                                      rowIndex As Integer) As Nullable(Of Double)
        Dim value As Double
        If Not TryConvertNumericValue(rawValue, value, parameterName, rowIndex) Then Return Nothing

        If value <= 0.0R Then
            Throw New ArgumentOutOfRangeException(parameterName,
                                                  "Value at row " & RowText(rowIndex) &
                                                  " must be greater than zero.")
        End If
        Return value
    End Function

    Private Shared Sub ThrowIncompleteRow(rowIndex As Integer,
                                          hasLabel As Boolean,
                                          hasActual As Boolean,
                                          hasTarget As Boolean,
                                          rangeCount As Integer)
        Dim missing As New List(Of String)()
        If Not hasLabel Then missing.Add("measure label")
        If Not hasActual Then missing.Add("actual value")
        If Not hasTarget Then missing.Add("target value")
        If rangeCount < MinimumQualitativeRanges Then missing.Add("at least two qualitative ranges")

        Throw New ArgumentException("Bullet-chart row " & RowText(rowIndex) &
                                    " is incomplete. Missing: " & String.Join(", ", missing.ToArray()) & ".")
    End Sub

    Private Shared Function CopyOptionalVector(values As Array,
                                               parameterName As String,
                                               expectedLength As Integer) As Object()
        If values Is Nothing Then Return Nothing
        Dim result As Object() = CopyVector(values, parameterName)
        RequireSameLength(result, expectedLength, parameterName)
        Return result
    End Function

    Private Shared Sub RequireSameLength(values As Object(),
                                         expectedLength As Integer,
                                         parameterName As String)
        If values.Length <> expectedLength Then
            Throw New ArgumentException(parameterName & " must contain the same number of rows as measureLabels.",
                                        parameterName)
        End If
    End Sub

    ''' <summary>
    ''' Copies a one-dimensional or single-row/single-column two-dimensional array into a zero-based
    ''' Object vector. This accepts both normal core arrays and Excel-range shaped Object(,) values.
    ''' </summary>
    Private Shared Function CopyVector(values As Array, parameterName As String) As Object()
        If values Is Nothing Then Throw New ArgumentNullException(parameterName)

        If values.Rank = 1 Then
            If values.Length = 0 Then Return Array.Empty(Of Object)()
            Dim result(values.Length - 1) As Object
            Dim lower As Integer = values.GetLowerBound(0)
            For i As Integer = 0 To values.Length - 1
                result(i) = values.GetValue(lower + i)
            Next
            Return result
        End If

        If values.Rank = 2 Then
            Dim rows As Integer = values.GetLength(0)
            Dim columns As Integer = values.GetLength(1)
            If rows = 0 OrElse columns = 0 Then Return Array.Empty(Of Object)()
            If rows <> 1 AndAlso columns <> 1 Then
                Throw New ArgumentException("The input must be one-dimensional or a single-row/single-column two-dimensional array.",
                                            parameterName)
            End If

            Dim result(values.Length - 1) As Object
            Dim rowLower As Integer = values.GetLowerBound(0)
            Dim columnLower As Integer = values.GetLowerBound(1)
            If columns = 1 Then
                For i As Integer = 0 To rows - 1
                    result(i) = values.GetValue(rowLower + i, columnLower)
                Next
            Else
                For i As Integer = 0 To columns - 1
                    result(i) = values.GetValue(rowLower, columnLower + i)
                Next
            End If
            Return result
        End If

        Throw New ArgumentException("The input must be one-dimensional or a single-row/single-column two-dimensional array.",
                                    parameterName)
    End Function

    Private Shared Function TryConvertLabel(rawValue As Object, ByRef text As String) As Boolean
        text = String.Empty
        If IsMissingValue(rawValue) Then Return False

        text = Convert.ToString(rawValue, CultureInfo.CurrentCulture)
        If text Is Nothing Then text = String.Empty
        text = text.Trim()
        Return text.Length > 0
    End Function

    Private Shared Function ConvertOptionalText(rawValue As Object) As String
        If IsMissingValue(rawValue) Then Return String.Empty
        Dim text As String = Convert.ToString(rawValue, CultureInfo.CurrentCulture)
        If text Is Nothing Then Return String.Empty
        Return text.Trim()
    End Function

    ''' <summary>
    ''' Converts a plotted value to Double. Missing values return False. Numeric strings are accepted
    ''' using current-culture and invariant-culture parsing. Boolean and infinite values are rejected.
    ''' </summary>
    Private Shared Function TryConvertNumericValue(rawValue As Object,
                                                   ByRef value As Double,
                                                   parameterName As String,
                                                   rowIndex As Integer,
                                                   Optional columnIndex As Nullable(Of Integer) = Nothing) As Boolean
        value = Double.NaN

        If IsMissingValue(rawValue) Then Return False

        Dim location As String = "row " & RowText(rowIndex)
        If columnIndex.HasValue Then
            location &= ", range " & (columnIndex.Value + 1).ToString(CultureInfo.CurrentCulture)
        End If

        If TypeOf rawValue Is Boolean Then
            Throw New ArgumentException("Value at " & location & " in " & parameterName &
                                        " is Boolean rather than numeric.",
                                        parameterName)
        ElseIf IsNumeric(rawValue) Then
            value = Convert.ToDouble(rawValue, CultureInfo.InvariantCulture)
        ElseIf TypeOf rawValue Is String Then
            Dim text As String = CStr(rawValue).Trim()
            Dim parsed As Double
            If Double.TryParse(text,
                               NumberStyles.Float Or NumberStyles.AllowThousands,
                               CultureInfo.CurrentCulture,
                               parsed) OrElse
               Double.TryParse(text,
                               NumberStyles.Float Or NumberStyles.AllowThousands,
                               CultureInfo.InvariantCulture,
                               parsed) Then
                value = parsed
            Else
                Throw New ArgumentException("Value '" & text & "' at " & location & " in " &
                                            parameterName & " is not numeric.",
                                            parameterName)
            End If
        Else
            Throw New ArgumentException("Value at " & location & " in " & parameterName &
                                        " is not numeric.",
                                        parameterName)
        End If

        If Double.IsNaN(value) Then Return False
        If Double.IsInfinity(value) Then
            Throw New ArgumentOutOfRangeException(parameterName,
                                                  "Value at " & location & " is infinite.")
        End If
        Return True
    End Function

    Private Shared Function IsMissingValue(value As Object) As Boolean
        If value Is Nothing OrElse Convert.IsDBNull(value) Then Return True
        If TypeOf value Is String Then Return String.IsNullOrWhiteSpace(CStr(value))
        If TypeOf value Is Double Then Return Double.IsNaN(CDbl(value))
        If TypeOf value Is Single Then Return Single.IsNaN(CSng(value))
        Return False
    End Function

    Private Shared Function IsFinite(value As Double) As Boolean
        Return Not Double.IsNaN(value) AndAlso Not Double.IsInfinity(value)
    End Function

    Private Shared Function IsFinitePositive(value As Double) As Boolean
        Return IsFinite(value) AndAlso value > 0.0R
    End Function

    Private Shared Function ScaleTolerance(value As Double) As Double
        Return ScaleToleranceFactor * Math.Max(1.0R, Math.Abs(value))
    End Function

    Private Shared Function NearlyEqual(firstValue As Double, secondValue As Double) As Boolean
        Dim scale As Double = Math.Max(1.0R, Math.Max(Math.Abs(firstValue), Math.Abs(secondValue)))
        Return Math.Abs(firstValue - secondValue) <= ScaleToleranceFactor * scale
    End Function

    ''' <summary>
    ''' Internal/public-model bridge used by BulletChartItem without exposing numerical helpers as part
    ''' of the user-facing API.
    ''' </summary>
    Friend Shared Function NearlyEqualForPublicUse(firstValue As Double, secondValue As Double) As Boolean
        Return NearlyEqual(firstValue, secondValue)
    End Function

    Private Shared Function RowText(zeroBasedRow As Integer) As String
        Return (zeroBasedRow + 1).ToString(CultureInfo.CurrentCulture)
    End Function
End Class
