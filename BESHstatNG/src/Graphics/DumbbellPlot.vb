Option Explicit On
Option Strict Off
Option Infer On

Imports System
Imports System.Collections.Generic
Imports System.Globalization
Imports System.Linq

''' <summary>
''' Controls how dumbbell categories are ordered from top to bottom.
''' </summary>
Public Enum DumbbellPlotSortMode
    ''' <summary>Preserves the order of the usable source rows.</summary>
    InputOrder

    ''' <summary>Orders categories by the first endpoint, smallest first.</summary>
    FirstAscending

    ''' <summary>Orders categories by the first endpoint, largest first.</summary>
    FirstDescending

    ''' <summary>Orders categories by the second endpoint, smallest first.</summary>
    SecondAscending

    ''' <summary>Orders categories by the second endpoint, largest first.</summary>
    SecondDescending

    ''' <summary>Orders by Second - First, smallest change first.</summary>
    DifferenceAscending

    ''' <summary>Orders by Second - First, largest change first.</summary>
    DifferenceDescending

    ''' <summary>Orders by |Second - First|, smallest absolute change first.</summary>
    AbsoluteDifferenceAscending

    ''' <summary>Orders by |Second - First|, largest absolute change first.</summary>
    AbsoluteDifferenceDescending

    ''' <summary>Orders category display labels alphabetically.</summary>
    CategoryAscending

    ''' <summary>Orders category display labels in reverse alphabetical order.</summary>
    CategoryDescending
End Enum

''' <summary>
''' Describes the source representation detected for dumbbell axis values.
''' This is only a formatting hint for a renderer; all calculated X values are Double.
''' </summary>
Public Enum DumbbellAxisValueKind
    Numeric
    DateValue
    Mixed
End Enum

''' <summary>
''' Excel-independent data preparation options for <see cref="DumbbellPlot"/>.
''' Appearance settings such as colors, marker size and connector width belong in the
''' Excel renderer rather than in this class.
''' </summary>
Public Class DumbbellPlotOptions
    ''' <summary>Top-to-bottom ordering strategy. Default: InputOrder.</summary>
    Public Property SortMode As DumbbellPlotSortMode = DumbbellPlotSortMode.InputOrder

    ''' <summary>
    ''' Reverses the final top-to-bottom order after applying <see cref="SortMode"/>.
    ''' Default: False.
    ''' </summary>
    Public Property ReverseCategoryOrder As Boolean = False

    ''' <summary>
    ''' When False (default), a missing category or endpoint causes validation to fail.
    ''' When True, incomplete endpoint rows are omitted and counted in the result.
    ''' </summary>
    Public Property OmitIncompleteRows As Boolean = False

    ''' <summary>
    ''' Maximum vertical displacement applied to optional auxiliary observations,
    ''' expressed in units of one category spacing. Zero disables jitter.
    ''' A value such as 0.15 is useful when many event observations overlap.
    ''' Accepted range: 0 to 0.45. Default: 0.
    ''' </summary>
    Public Property AuxiliaryJitterFraction As Double = 0.0R

    ''' <summary>
    ''' Seed used for deterministic auxiliary-observation jitter. Default: 1729.
    ''' </summary>
    Public Property AuxiliaryJitterSeed As Integer = 1729

    Friend Function Copy() As DumbbellPlotOptions
        Return New DumbbellPlotOptions With {
            .SortMode = SortMode,
            .ReverseCategoryOrder = ReverseCategoryOrder,
            .OmitIncompleteRows = OmitIncompleteRows,
            .AuxiliaryJitterFraction = AuxiliaryJitterFraction,
            .AuxiliaryJitterSeed = AuxiliaryJitterSeed
        }
    End Function
End Class

''' <summary>
''' Immutable endpoint row returned by <see cref="DumbbellPlot.Compute"/>.
''' </summary>
Public NotInheritable Class DumbbellPlotCategory
    Private ReadOnly _sourceIndex As Integer
    Private ReadOnly _categoryValue As Object
    Private ReadOnly _categoryKey As String
    Private ReadOnly _categoryLabel As String
    Private ReadOnly _displayIndex As Integer
    Private ReadOnly _yPosition As Double
    Private ReadOnly _firstValue As Double
    Private ReadOnly _secondValue As Double

    Friend Sub New(sourceIndex As Integer,
                   categoryValue As Object,
                   categoryKey As String,
                   categoryLabel As String,
                   displayIndex As Integer,
                   yPosition As Double,
                   firstValue As Double,
                   secondValue As Double)
        _sourceIndex = sourceIndex
        _categoryValue = categoryValue
        _categoryKey = categoryKey
        _categoryLabel = categoryLabel
        _displayIndex = displayIndex
        _yPosition = yPosition
        _firstValue = firstValue
        _secondValue = secondValue
    End Sub

    ''' <summary>Zero-based row position in the source endpoint arrays.</summary>
    Public ReadOnly Property SourceIndex As Integer
        Get
            Return _sourceIndex
        End Get
    End Property

    ''' <summary>Original category value supplied by the caller.</summary>
    Public ReadOnly Property CategoryValue As Object
        Get
            Return _categoryValue
        End Get
    End Property

    ''' <summary>Stable normalized key used to match optional auxiliary observations.</summary>
    Public ReadOnly Property CategoryKey As String
        Get
            Return _categoryKey
        End Get
    End Property

    ''' <summary>Display text for the category.</summary>
    Public ReadOnly Property CategoryLabel As String
        Get
            Return _categoryLabel
        End Get
    End Property

    ''' <summary>Zero-based top-to-bottom position after sorting and optional reversal.</summary>
    Public ReadOnly Property DisplayIndex As Integer
        Get
            Return _displayIndex
        End Get
    End Property

    ''' <summary>
    ''' Numeric Y coordinate intended for an XY-scatter renderer. The first displayed
    ''' category receives the largest Y value so it appears at the top of a normal Excel axis.
    ''' </summary>
    Public ReadOnly Property YPosition As Double
        Get
            Return _yPosition
        End Get
    End Property

    Public ReadOnly Property FirstValue As Double
        Get
            Return _firstValue
        End Get
    End Property

    Public ReadOnly Property SecondValue As Double
        Get
            Return _secondValue
        End Get
    End Property

    ''' <summary>Signed change, calculated as SecondValue - FirstValue.</summary>
    Public ReadOnly Property Difference As Double
        Get
            Return _secondValue - _firstValue
        End Get
    End Property

    Public ReadOnly Property AbsoluteDifference As Double
        Get
            Return Math.Abs(_secondValue - _firstValue)
        End Get
    End Property

    ''' <summary>Left-most endpoint, independent of first/second series semantics.</summary>
    Public ReadOnly Property ConnectorStart As Double
        Get
            Return Math.Min(_firstValue, _secondValue)
        End Get
    End Property

    ''' <summary>Right-most endpoint, independent of first/second series semantics.</summary>
    Public ReadOnly Property ConnectorEnd As Double
        Get
            Return Math.Max(_firstValue, _secondValue)
        End Get
    End Property

    ''' <summary>Non-negative horizontal connector length.</summary>
    Public ReadOnly Property ConnectorLength As Double
        Get
            Return Math.Abs(_secondValue - _firstValue)
        End Get
    End Property
End Class

''' <summary>
''' Immutable optional event/observation point associated with one dumbbell category.
''' </summary>
Public NotInheritable Class DumbbellAuxiliaryObservation
    Private ReadOnly _sourceIndex As Integer
    Private ReadOnly _categoryKey As String
    Private ReadOnly _categoryLabel As String
    Private ReadOnly _categoryDisplayIndex As Integer
    Private ReadOnly _value As Double
    Private ReadOnly _baseYPosition As Double
    Private ReadOnly _jitterOffset As Double

    Friend Sub New(sourceIndex As Integer,
                   categoryKey As String,
                   categoryLabel As String,
                   categoryDisplayIndex As Integer,
                   value As Double,
                   baseYPosition As Double,
                   jitterOffset As Double)
        _sourceIndex = sourceIndex
        _categoryKey = categoryKey
        _categoryLabel = categoryLabel
        _categoryDisplayIndex = categoryDisplayIndex
        _value = value
        _baseYPosition = baseYPosition
        _jitterOffset = jitterOffset
    End Sub

    ''' <summary>Zero-based position in the source auxiliary arrays.</summary>
    Public ReadOnly Property SourceIndex As Integer
        Get
            Return _sourceIndex
        End Get
    End Property

    Public ReadOnly Property CategoryKey As String
        Get
            Return _categoryKey
        End Get
    End Property

    Public ReadOnly Property CategoryLabel As String
        Get
            Return _categoryLabel
        End Get
    End Property

    ''' <summary>Zero-based top-to-bottom category position in the prepared result.</summary>
    Public ReadOnly Property CategoryDisplayIndex As Integer
        Get
            Return _categoryDisplayIndex
        End Get
    End Property

    ''' <summary>Numeric/date-serial X value of the auxiliary observation.</summary>
    Public ReadOnly Property Value As Double
        Get
            Return _value
        End Get
    End Property

    ''' <summary>Unjittered Y coordinate of the owning category.</summary>
    Public ReadOnly Property BaseYPosition As Double
        Get
            Return _baseYPosition
        End Get
    End Property

    ''' <summary>Deterministic vertical offset applied to the observation.</summary>
    Public ReadOnly Property JitterOffset As Double
        Get
            Return _jitterOffset
        End Get
    End Property

    ''' <summary>Final Y coordinate for the auxiliary scatter series.</summary>
    Public ReadOnly Property YPosition As Double
        Get
            Return _baseYPosition + _jitterOffset
        End Get
    End Property
End Class

''' <summary>
''' Immutable result from the Excel-independent dumbbell data-preparation engine.
''' </summary>
Public NotInheritable Class DumbbellPlotResult
    Private ReadOnly _categories As DumbbellPlotCategory()
    Private ReadOnly _auxiliaryObservations As DumbbellAuxiliaryObservation()
    Private ReadOnly _sourceCategoryCount As Integer
    Private ReadOnly _omittedCategoryCount As Integer
    Private ReadOnly _sourceAuxiliaryObservationCount As Integer
    Private ReadOnly _omittedAuxiliaryObservationCount As Integer
    Private ReadOnly _minimumValue As Double
    Private ReadOnly _maximumValue As Double
    Private ReadOnly _axisValueKind As DumbbellAxisValueKind
    Private ReadOnly _options As DumbbellPlotOptions

    Friend Sub New(categories As DumbbellPlotCategory(),
                   auxiliaryObservations As DumbbellAuxiliaryObservation(),
                   sourceCategoryCount As Integer,
                   omittedCategoryCount As Integer,
                   sourceAuxiliaryObservationCount As Integer,
                   omittedAuxiliaryObservationCount As Integer,
                   minimumValue As Double,
                   maximumValue As Double,
                   axisValueKind As DumbbellAxisValueKind,
                   options As DumbbellPlotOptions)
        _categories = DirectCast(categories.Clone(), DumbbellPlotCategory())
        _auxiliaryObservations = DirectCast(auxiliaryObservations.Clone(), DumbbellAuxiliaryObservation())
        _sourceCategoryCount = sourceCategoryCount
        _omittedCategoryCount = omittedCategoryCount
        _sourceAuxiliaryObservationCount = sourceAuxiliaryObservationCount
        _omittedAuxiliaryObservationCount = omittedAuxiliaryObservationCount
        _minimumValue = minimumValue
        _maximumValue = maximumValue
        _axisValueKind = axisValueKind
        _options = options.Copy()
    End Sub

    Public ReadOnly Property Categories As DumbbellPlotCategory()
        Get
            Return DirectCast(_categories.Clone(), DumbbellPlotCategory())
        End Get
    End Property

    Public ReadOnly Property AuxiliaryObservations As DumbbellAuxiliaryObservation()
        Get
            Return DirectCast(_auxiliaryObservations.Clone(), DumbbellAuxiliaryObservation())
        End Get
    End Property

    Public ReadOnly Property CategoryCount As Integer
        Get
            Return _categories.Length
        End Get
    End Property

    Public ReadOnly Property HasAuxiliaryObservations As Boolean
        Get
            Return _auxiliaryObservations.Length > 0
        End Get
    End Property

    Public ReadOnly Property SourceCategoryCount As Integer
        Get
            Return _sourceCategoryCount
        End Get
    End Property

    Public ReadOnly Property OmittedCategoryCount As Integer
        Get
            Return _omittedCategoryCount
        End Get
    End Property

    Public ReadOnly Property SourceAuxiliaryObservationCount As Integer
        Get
            Return _sourceAuxiliaryObservationCount
        End Get
    End Property

    Public ReadOnly Property UsableAuxiliaryObservationCount As Integer
        Get
            Return _auxiliaryObservations.Length
        End Get
    End Property

    Public ReadOnly Property OmittedAuxiliaryObservationCount As Integer
        Get
            Return _omittedAuxiliaryObservationCount
        End Get
    End Property

    ''' <summary>Minimum finite endpoint/auxiliary X value in the prepared result.</summary>
    Public ReadOnly Property MinimumValue As Double
        Get
            Return _minimumValue
        End Get
    End Property

    ''' <summary>Maximum finite endpoint/auxiliary X value in the prepared result.</summary>
    Public ReadOnly Property MaximumValue As Double
        Get
            Return _maximumValue
        End Get
    End Property

    ''' <summary>
    ''' Formatting hint inferred from values supplied as numeric types and/or DateTime values.
    ''' Excel date serials supplied as Double are necessarily classified as numeric.
    ''' </summary>
    Public ReadOnly Property AxisValueKind As DumbbellAxisValueKind
        Get
            Return _axisValueKind
        End Get
    End Property

    ''' <summary>Validated options snapshot used to prepare this result.</summary>
    Public ReadOnly Property Options As DumbbellPlotOptions
        Get
            Return _options.Copy()
        End Get
    End Property
End Class

''' <summary>
''' Excel-independent backend for horizontal dumbbell plots.
'''
''' The backend validates and normalizes category/end-point arrays, applies stable sorting,
''' assigns artificial numeric Y positions for an XY-scatter renderer, calculates connector
''' geometry, and optionally associates a second table of event/observation points with the
''' endpoint categories. The eventual Excel renderer can therefore use only a handful of
''' series even for large plots.
''' </summary>
Public NotInheritable Class DumbbellPlot
    Private Sub New()
    End Sub

    Private NotInheritable Class WorkingCategory
        Friend SourceIndex As Integer
        Friend CategoryValue As Object
        Friend CategoryKey As String
        Friend CategoryLabel As String
        Friend FirstValue As Double
        Friend SecondValue As Double
        Friend DisplayIndex As Integer
        Friend YPosition As Double
    End Class

    ''' <summary>
    ''' Prepares a dumbbell plot from row-aligned category, first-value and second-value vectors.
    ''' Numeric values and DateTime values are accepted for the X axis. DateTime values are
    ''' converted to OLE Automation/Excel serial values.
    ''' </summary>
    ''' <param name="categories">
    ''' Category identifiers. One-dimensional arrays and single-row/single-column two-dimensional
    ''' arrays are accepted. Text and numeric category identifiers are supported.
    ''' </param>
    ''' <param name="firstValues">First endpoint values aligned with categories.</param>
    ''' <param name="secondValues">Second endpoint values aligned with categories.</param>
    ''' <param name="options">Optional data preparation and ordering settings.</param>
    ''' <param name="auxiliaryCategories">
    ''' Optional category identifiers for additional event/observation points. Repeated categories
    ''' are expected and allowed. Every usable identifier must match a retained endpoint category.
    ''' </param>
    ''' <param name="auxiliaryValues">Optional X values aligned with auxiliaryCategories.</param>
    Public Shared Function Compute(categories As Array,
                                   firstValues As Array,
                                   secondValues As Array,
                                   Optional options As DumbbellPlotOptions = Nothing,
                                   Optional auxiliaryCategories As Array = Nothing,
                                   Optional auxiliaryValues As Array = Nothing) As DumbbellPlotResult
        If categories Is Nothing Then Throw New ArgumentNullException(NameOf(categories))
        If firstValues Is Nothing Then Throw New ArgumentNullException(NameOf(firstValues))
        If secondValues Is Nothing Then Throw New ArgumentNullException(NameOf(secondValues))

        Dim categoryRaw As Object() = CopyVector(categories, NameOf(categories))
        Dim firstRaw As Object() = CopyVector(firstValues, NameOf(firstValues))
        Dim secondRaw As Object() = CopyVector(secondValues, NameOf(secondValues))

        If categoryRaw.Length = 0 Then
            Throw New ArgumentException("At least one dumbbell category is required.", NameOf(categories))
        End If
        If firstRaw.Length <> categoryRaw.Length OrElse secondRaw.Length <> categoryRaw.Length Then
            Throw New ArgumentException("Categories, First values and Second values must contain the same number of rows.")
        End If

        Dim hasAuxiliaryCategories As Boolean = auxiliaryCategories IsNot Nothing
        Dim hasAuxiliaryValues As Boolean = auxiliaryValues IsNot Nothing
        If hasAuxiliaryCategories Xor hasAuxiliaryValues Then
            Throw New ArgumentException("Auxiliary categories and auxiliary values must either both be supplied or both be omitted.")
        End If

        Dim resolvedOptions As DumbbellPlotOptions = If(options, New DumbbellPlotOptions()).Copy()
        ValidateOptions(resolvedOptions)

        Dim anyNumericValue As Boolean = False
        Dim anyDateValue As Boolean = False
        Dim working As New List(Of WorkingCategory)(categoryRaw.Length)
        Dim byKey As New Dictionary(Of String, WorkingCategory)(StringComparer.Ordinal)
        Dim omittedCategoryCount As Integer = 0

        For i As Integer = 0 To categoryRaw.Length - 1
            Dim categoryValue As Object = categoryRaw(i)
            Dim missingCategory As Boolean = IsMissingCategoryValue(categoryValue)

            Dim firstValue As Double = Double.NaN
            Dim secondValue As Double = Double.NaN
            Dim firstWasDate As Boolean = False
            Dim secondWasDate As Boolean = False
            Dim hasFirst As Boolean = TryConvertAxisValue(firstRaw(i), firstValue, firstWasDate, NameOf(firstValues), i)
            Dim hasSecond As Boolean = TryConvertAxisValue(secondRaw(i), secondValue, secondWasDate, NameOf(secondValues), i)

            If missingCategory OrElse Not hasFirst OrElse Not hasSecond Then
                If resolvedOptions.OmitIncompleteRows Then
                    omittedCategoryCount += 1
                    Continue For
                End If

                If missingCategory Then
                    Throw New ArgumentException("Dumbbell category at row " &
                                                (i + 1).ToString(CultureInfo.CurrentCulture) &
                                                " is missing.", NameOf(categories))
                End If
                If Not hasFirst Then
                    Throw New ArgumentException("First endpoint at row " &
                                                (i + 1).ToString(CultureInfo.CurrentCulture) &
                                                " is missing.", NameOf(firstValues))
                End If
                Throw New ArgumentException("Second endpoint at row " &
                                            (i + 1).ToString(CultureInfo.CurrentCulture) &
                                            " is missing.", NameOf(secondValues))
            End If

            anyDateValue = anyDateValue OrElse firstWasDate OrElse secondWasDate
            anyNumericValue = anyNumericValue OrElse Not firstWasDate OrElse Not secondWasDate

            Dim categoryKey As String = BuildCategoryKey(categoryValue)
            Dim categoryLabel As String = FormatCategoryName(categoryValue)
            If byKey.ContainsKey(categoryKey) Then
                Throw New ArgumentException("Dumbbell category '" & categoryLabel &
                                            "' occurs more than once in the endpoint table. " &
                                            "Each endpoint category must be unique.", NameOf(categories))
            End If

            Dim item As New WorkingCategory With {
                .SourceIndex = i,
                .CategoryValue = categoryValue,
                .CategoryKey = categoryKey,
                .CategoryLabel = categoryLabel,
                .FirstValue = firstValue,
                .SecondValue = secondValue
            }
            byKey.Add(categoryKey, item)
            working.Add(item)
        Next

        If working.Count = 0 Then
            Throw New ArgumentException("No complete dumbbell category with two finite endpoint values is available.")
        End If

        Dim ordered As List(Of WorkingCategory) = OrderCategories(working, resolvedOptions.SortMode)
        If resolvedOptions.ReverseCategoryOrder Then ordered.Reverse()

        For i As Integer = 0 To ordered.Count - 1
            ordered(i).DisplayIndex = i
            'Largest Y coordinate is shown at the top of a standard Excel XY axis.
            ordered(i).YPosition = ordered.Count - i
        Next

        'Rebuild lookup against the retained categories. The WorkingCategory instances are
        'the same objects after sorting, so their final display positions are now available.
        byKey.Clear()
        For Each item As WorkingCategory In ordered
            byKey.Add(item.CategoryKey, item)
        Next

        Dim preparedCategories(ordered.Count - 1) As DumbbellPlotCategory
        Dim minimumValue As Double = Double.PositiveInfinity
        Dim maximumValue As Double = Double.NegativeInfinity
        For i As Integer = 0 To ordered.Count - 1
            Dim item As WorkingCategory = ordered(i)
            preparedCategories(i) = New DumbbellPlotCategory(item.SourceIndex,
                                                             item.CategoryValue,
                                                             item.CategoryKey,
                                                             item.CategoryLabel,
                                                             item.DisplayIndex,
                                                             item.YPosition,
                                                             item.FirstValue,
                                                             item.SecondValue)
            minimumValue = Math.Min(minimumValue, Math.Min(item.FirstValue, item.SecondValue))
            maximumValue = Math.Max(maximumValue, Math.Max(item.FirstValue, item.SecondValue))
        Next

        Dim preparedAuxiliary As DumbbellAuxiliaryObservation() = Array.Empty(Of DumbbellAuxiliaryObservation)()
        Dim sourceAuxiliaryCount As Integer = 0
        Dim omittedAuxiliaryCount As Integer = 0

        If hasAuxiliaryCategories Then
            Dim auxiliaryCategoryRaw As Object() = CopyVector(auxiliaryCategories, NameOf(auxiliaryCategories))
            Dim auxiliaryValueRaw As Object() = CopyVector(auxiliaryValues, NameOf(auxiliaryValues))
            If auxiliaryCategoryRaw.Length <> auxiliaryValueRaw.Length Then
                Throw New ArgumentException("Auxiliary categories and auxiliary values must contain the same number of rows.")
            End If

            sourceAuxiliaryCount = auxiliaryCategoryRaw.Length
            Dim observations As New List(Of DumbbellAuxiliaryObservation)(sourceAuxiliaryCount)
            Dim jitterRandom As Random = Nothing
            If resolvedOptions.AuxiliaryJitterFraction > 0.0R Then
                jitterRandom = New Random(resolvedOptions.AuxiliaryJitterSeed)
            End If

            For i As Integer = 0 To auxiliaryCategoryRaw.Length - 1
                Dim categoryValue As Object = auxiliaryCategoryRaw(i)
                If IsMissingCategoryValue(categoryValue) Then
                    omittedAuxiliaryCount += 1
                    Continue For
                End If

                Dim value As Double = Double.NaN
                Dim wasDate As Boolean = False
                If Not TryConvertAxisValue(auxiliaryValueRaw(i), value, wasDate, NameOf(auxiliaryValues), i) Then
                    omittedAuxiliaryCount += 1
                    Continue For
                End If

                anyDateValue = anyDateValue OrElse wasDate
                anyNumericValue = anyNumericValue OrElse Not wasDate

                Dim categoryKey As String = BuildCategoryKey(categoryValue)
                Dim endpointCategory As WorkingCategory = Nothing
                If Not byKey.TryGetValue(categoryKey, endpointCategory) Then
                    Throw New ArgumentException("Auxiliary category '" & FormatCategoryName(categoryValue) &
                                                "' at row " &
                                                (i + 1).ToString(CultureInfo.CurrentCulture) &
                                                " does not match a retained endpoint category.",
                                                NameOf(auxiliaryCategories))
                End If

                Dim jitterOffset As Double = 0.0R
                If jitterRandom IsNot Nothing Then
                    jitterOffset = (2.0R * jitterRandom.NextDouble() - 1.0R) *
                                   resolvedOptions.AuxiliaryJitterFraction
                End If

                observations.Add(New DumbbellAuxiliaryObservation(i,
                                                                   endpointCategory.CategoryKey,
                                                                   endpointCategory.CategoryLabel,
                                                                   endpointCategory.DisplayIndex,
                                                                   value,
                                                                   endpointCategory.YPosition,
                                                                   jitterOffset))
                minimumValue = Math.Min(minimumValue, value)
                maximumValue = Math.Max(maximumValue, value)
            Next

            preparedAuxiliary = observations.ToArray()
        End If

        Dim axisValueKind As DumbbellAxisValueKind
        If anyDateValue AndAlso anyNumericValue Then
            axisValueKind = DumbbellAxisValueKind.Mixed
        ElseIf anyDateValue Then
            axisValueKind = DumbbellAxisValueKind.DateValue
        Else
            axisValueKind = DumbbellAxisValueKind.Numeric
        End If

        Return New DumbbellPlotResult(preparedCategories,
                                      preparedAuxiliary,
                                      categoryRaw.Length,
                                      omittedCategoryCount,
                                      sourceAuxiliaryCount,
                                      omittedAuxiliaryCount,
                                      minimumValue,
                                      maximumValue,
                                      axisValueKind,
                                      resolvedOptions)
    End Function

    ''' <summary>
    ''' Positional convenience overload for the enhanced dumbbell form with additional
    ''' event/observation points.
    ''' </summary>
    Public Shared Function ComputeWithAuxiliary(categories As Array,
                                                firstValues As Array,
                                                secondValues As Array,
                                                auxiliaryCategories As Array,
                                                auxiliaryValues As Array,
                                                Optional options As DumbbellPlotOptions = Nothing) As DumbbellPlotResult
        If auxiliaryCategories Is Nothing Then Throw New ArgumentNullException(NameOf(auxiliaryCategories))
        If auxiliaryValues Is Nothing Then Throw New ArgumentNullException(NameOf(auxiliaryValues))
        Return Compute(categories, firstValues, secondValues, options, auxiliaryCategories, auxiliaryValues)
    End Function

    Private Shared Function OrderCategories(source As IEnumerable(Of WorkingCategory),
                                            sortMode As DumbbellPlotSortMode) As List(Of WorkingCategory)
        Dim ordered As IOrderedEnumerable(Of WorkingCategory)

        Select Case sortMode
            Case DumbbellPlotSortMode.InputOrder
                ordered = source.OrderBy(Function(x) x.SourceIndex)

            Case DumbbellPlotSortMode.FirstAscending
                ordered = source.OrderBy(Function(x) x.FirstValue).ThenBy(Function(x) x.SourceIndex)

            Case DumbbellPlotSortMode.FirstDescending
                ordered = source.OrderByDescending(Function(x) x.FirstValue).ThenBy(Function(x) x.SourceIndex)

            Case DumbbellPlotSortMode.SecondAscending
                ordered = source.OrderBy(Function(x) x.SecondValue).ThenBy(Function(x) x.SourceIndex)

            Case DumbbellPlotSortMode.SecondDescending
                ordered = source.OrderByDescending(Function(x) x.SecondValue).ThenBy(Function(x) x.SourceIndex)

            Case DumbbellPlotSortMode.DifferenceAscending
                ordered = source.OrderBy(Function(x) x.SecondValue - x.FirstValue).ThenBy(Function(x) x.SourceIndex)

            Case DumbbellPlotSortMode.DifferenceDescending
                ordered = source.OrderByDescending(Function(x) x.SecondValue - x.FirstValue).ThenBy(Function(x) x.SourceIndex)

            Case DumbbellPlotSortMode.AbsoluteDifferenceAscending
                ordered = source.OrderBy(Function(x) Math.Abs(x.SecondValue - x.FirstValue)).ThenBy(Function(x) x.SourceIndex)

            Case DumbbellPlotSortMode.AbsoluteDifferenceDescending
                ordered = source.OrderByDescending(Function(x) Math.Abs(x.SecondValue - x.FirstValue)).ThenBy(Function(x) x.SourceIndex)

            Case DumbbellPlotSortMode.CategoryAscending
                ordered = source.OrderBy(Function(x) x.CategoryLabel, StringComparer.CurrentCultureIgnoreCase).
                                 ThenBy(Function(x) x.SourceIndex)

            Case DumbbellPlotSortMode.CategoryDescending
                ordered = source.OrderByDescending(Function(x) x.CategoryLabel, StringComparer.CurrentCultureIgnoreCase).
                                 ThenBy(Function(x) x.SourceIndex)

            Case Else
                Throw New ArgumentOutOfRangeException(NameOf(sortMode), "The dumbbell sort mode is not defined.")
        End Select

        Return ordered.ToList()
    End Function

    Private Shared Sub ValidateOptions(options As DumbbellPlotOptions)
        If Not [Enum].IsDefined(GetType(DumbbellPlotSortMode), options.SortMode) Then
            Throw New ArgumentOutOfRangeException(NameOf(options.SortMode), "The dumbbell sort mode is not defined.")
        End If

        Dim jitter As Double = options.AuxiliaryJitterFraction
        If Double.IsNaN(jitter) OrElse Double.IsInfinity(jitter) OrElse jitter < 0.0R OrElse jitter > 0.45R Then
            Throw New ArgumentOutOfRangeException(NameOf(options.AuxiliaryJitterFraction),
                                                  "AuxiliaryJitterFraction must be finite and in the interval [0, 0.45].")
        End If
    End Sub

    ''' <summary>
    ''' Copies a one-dimensional or single-row/single-column two-dimensional array into
    ''' a zero-based Object vector. This makes the backend convenient for both core arrays
    ''' and Excel-range shaped Object(,) inputs.
    ''' </summary>
    Private Shared Function CopyVector(values As Array, parameterName As String) As Object()
        If values Is Nothing Then Throw New ArgumentNullException(parameterName)

        If values.Rank = 1 Then
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

    ''' <summary>
    ''' Converts an axis value to Double. Missing values return False. Infinite values and
    ''' unsupported types are rejected because an Excel XY series cannot plot them safely.
    ''' </summary>
    Private Shared Function TryConvertAxisValue(rawValue As Object,
                                                ByRef value As Double,
                                                ByRef wasDate As Boolean,
                                                parameterName As String,
                                                rowIndex As Integer) As Boolean
        value = Double.NaN
        wasDate = False

        If rawValue Is Nothing OrElse Convert.IsDBNull(rawValue) Then Return False
        If TypeOf rawValue Is String AndAlso String.IsNullOrWhiteSpace(CStr(rawValue)) Then Return False

        If TypeOf rawValue Is DateTime Then
            Dim dateValue As DateTime = DirectCast(rawValue, DateTime)
            value = dateValue.ToOADate()
            wasDate = True
        ElseIf TypeOf rawValue Is DateTimeOffset Then
            Dim dateValue As DateTimeOffset = DirectCast(rawValue, DateTimeOffset)
            value = dateValue.DateTime.ToOADate()
            wasDate = True
        ElseIf TypeOf rawValue Is Boolean Then
            Throw New ArgumentException("Axis value at row " &
                                        (rowIndex + 1).ToString(CultureInfo.CurrentCulture) &
                                        " in " & parameterName & " is Boolean rather than numeric/date.",
                                        parameterName)
        ElseIf IsNumeric(rawValue) Then
            value = Convert.ToDouble(rawValue, CultureInfo.InvariantCulture)
        ElseIf TypeOf rawValue Is String Then
            Dim text As String = CStr(rawValue).Trim()
            Dim parsed As Double
            If Double.TryParse(text, NumberStyles.Float Or NumberStyles.AllowThousands,
                               CultureInfo.CurrentCulture, parsed) OrElse
               Double.TryParse(text, NumberStyles.Float Or NumberStyles.AllowThousands,
                               CultureInfo.InvariantCulture, parsed) Then
                value = parsed
            Else
                Throw New ArgumentException("Axis value '" & text & "' at row " &
                                            (rowIndex + 1).ToString(CultureInfo.CurrentCulture) &
                                            " in " & parameterName & " is not numeric/date.",
                                            parameterName)
            End If
        Else
            Throw New ArgumentException("Axis value at row " &
                                        (rowIndex + 1).ToString(CultureInfo.CurrentCulture) &
                                        " in " & parameterName & " is not numeric/date.",
                                        parameterName)
        End If

        If Double.IsNaN(value) Then Return False
        If Double.IsInfinity(value) Then
            Throw New ArgumentOutOfRangeException(parameterName,
                                                  "Axis value at row " &
                                                  (rowIndex + 1).ToString(CultureInfo.CurrentCulture) &
                                                  " is infinite.")
        End If
        Return True
    End Function

    Private Shared Function IsMissingCategoryValue(value As Object) As Boolean
        If value Is Nothing OrElse Convert.IsDBNull(value) Then Return True
        If TypeOf value Is String Then Return String.IsNullOrWhiteSpace(CStr(value))
        Return False
    End Function

    Private Shared Function BuildCategoryKey(value As Object) As String
        If TypeOf value Is DateTime Then
            Return "D:" & DirectCast(value, DateTime).ToOADate().ToString("R", CultureInfo.InvariantCulture)
        End If
        If TypeOf value Is DateTimeOffset Then
            Return "D:" & DirectCast(value, DateTimeOffset).DateTime.ToOADate().ToString("R", CultureInfo.InvariantCulture)
        End If
        If IsNumeric(value) AndAlso Not TypeOf value Is Boolean Then
            Dim numericValue As Double = Convert.ToDouble(value, CultureInfo.InvariantCulture)
            If Double.IsNaN(numericValue) OrElse Double.IsInfinity(numericValue) Then
                Throw New ArgumentException("Numeric dumbbell categories must be finite.")
            End If
            Return "N:" & numericValue.ToString("R", CultureInfo.InvariantCulture)
        End If
        Return "S:" & Convert.ToString(value, CultureInfo.InvariantCulture).Trim()
    End Function

    Private Shared Function FormatCategoryName(value As Object) As String
        If TypeOf value Is DateTime Then
            Return DirectCast(value, DateTime).ToString(CultureInfo.CurrentCulture)
        End If
        If TypeOf value Is DateTimeOffset Then
            Return DirectCast(value, DateTimeOffset).ToString(CultureInfo.CurrentCulture)
        End If
        If IsNumeric(value) AndAlso Not TypeOf value Is Boolean Then
            Return Convert.ToDouble(value, CultureInfo.CurrentCulture).ToString(CultureInfo.CurrentCulture)
        End If
        Return Convert.ToString(value, CultureInfo.CurrentCulture).Trim()
    End Function
End Class
