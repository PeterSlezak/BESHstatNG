Option Explicit On
Option Strict Off
Option Infer On

Imports System
Imports System.Collections.Generic
Imports System.Globalization
Imports System.Linq

''' <summary>
''' Excel-independent data preparation options for <see cref="LadderPlot"/>.
''' Appearance settings such as colours, line widths, markers and labels belong in the
''' Excel renderer rather than in this class.
''' </summary>
Public Class LadderPlotOptions
    ''' <summary>
    ''' When False (default), a missing First/Before value, Second/After value, or supplied
    ''' grouping value causes validation to fail. When True, incomplete rows are omitted.
    ''' Missing optional record labels do not cause a row to be omitted; a row-number label
    ''' is generated instead.
    ''' </summary>
    Public Property OmitIncompleteRows As Boolean = False

    ''' <summary>
    ''' Maximum horizontal displacement of points involved in a collision, expressed as a
    ''' fraction of the distance between the two ladder sides. The normal X positions are
    ''' 1 (First/Before) and 2 (Second/After), so a value of 0.08 allows a point to move by
    ''' at most +/-0.08. Only collision groups containing two or more points are displaced.
    ''' Zero disables jitter. Accepted range: 0 to 0.25. Default: 0.
    ''' </summary>
    Public Property HorizontalJitterFraction As Double = 0.0R

    ''' <summary>
    ''' Absolute Y-value tolerance used when detecting point collisions independently on the
    ''' First/Before and Second/After sides. Zero (default) means that only exactly equal
    ''' numeric values collide. A positive value can be used when nearly identical values
    ''' should also be separated. Accepted range: zero or any finite positive value.
    ''' </summary>
    Public Property CollisionTolerance As Double = 0.0R

    Friend Function Copy() As LadderPlotOptions
        Return New LadderPlotOptions With {
            .OmitIncompleteRows = OmitIncompleteRows,
            .HorizontalJitterFraction = HorizontalJitterFraction,
            .CollisionTolerance = CollisionTolerance
        }
    End Function
End Class

''' <summary>
''' Immutable grouping level retained by <see cref="LadderPlot.Compute"/>.
''' Levels are returned in order of first appearance among usable source rows.
''' </summary>
Public NotInheritable Class LadderPlotGroupLevel
    Private ReadOnly _levelIndex As Integer
    Private ReadOnly _groupValue As Object
    Private ReadOnly _groupKey As String
    Private ReadOnly _groupLabel As String

    Friend Sub New(levelIndex As Integer,
                   groupValue As Object,
                   groupKey As String,
                   groupLabel As String)
        _levelIndex = levelIndex
        _groupValue = groupValue
        _groupKey = groupKey
        _groupLabel = groupLabel
    End Sub

    ''' <summary>Zero-based level index in first-appearance order.</summary>
    Public ReadOnly Property LevelIndex As Integer
        Get
            Return _levelIndex
        End Get
    End Property

    ''' <summary>Original grouping value supplied by the caller.</summary>
    Public ReadOnly Property GroupValue As Object
        Get
            Return _groupValue
        End Get
    End Property

    ''' <summary>Stable normalized key suitable for renderer lookups.</summary>
    Public ReadOnly Property GroupKey As String
        Get
            Return _groupKey
        End Get
    End Property

    ''' <summary>Display text for the grouping level.</summary>
    Public ReadOnly Property GroupLabel As String
        Get
            Return _groupLabel
        End Get
    End Property
End Class

''' <summary>
''' Immutable paired observation returned by <see cref="LadderPlot.Compute"/>.
''' </summary>
Public NotInheritable Class LadderPlotObservation
    Private ReadOnly _sourceIndex As Integer
    Private ReadOnly _displayIndex As Integer
    Private ReadOnly _recordLabel As String
    Private ReadOnly _hasExplicitRecordLabel As Boolean
    Private ReadOnly _groupValue As Object
    Private ReadOnly _groupKey As String
    Private ReadOnly _groupLabel As String
    Private ReadOnly _groupLevelIndex As Integer
    Private ReadOnly _firstValue As Double
    Private ReadOnly _secondValue As Double
    Private ReadOnly _firstJitterOffset As Double
    Private ReadOnly _secondJitterOffset As Double

    Friend Sub New(sourceIndex As Integer,
                   displayIndex As Integer,
                   recordLabel As String,
                   hasExplicitRecordLabel As Boolean,
                   groupValue As Object,
                   groupKey As String,
                   groupLabel As String,
                   groupLevelIndex As Integer,
                   firstValue As Double,
                   secondValue As Double,
                   firstJitterOffset As Double,
                   secondJitterOffset As Double)
        _sourceIndex = sourceIndex
        _displayIndex = displayIndex
        _recordLabel = recordLabel
        _hasExplicitRecordLabel = hasExplicitRecordLabel
        _groupValue = groupValue
        _groupKey = groupKey
        _groupLabel = groupLabel
        _groupLevelIndex = groupLevelIndex
        _firstValue = firstValue
        _secondValue = secondValue
        _firstJitterOffset = firstJitterOffset
        _secondJitterOffset = secondJitterOffset
    End Sub

    ''' <summary>Zero-based position in the original source vectors.</summary>
    Public ReadOnly Property SourceIndex As Integer
        Get
            Return _sourceIndex
        End Get
    End Property

    ''' <summary>
    ''' Zero-based position among retained observations. Input order is preserved.
    ''' </summary>
    Public ReadOnly Property DisplayIndex As Integer
        Get
            Return _displayIndex
        End Get
    End Property

    ''' <summary>
    ''' Record display label. When no usable explicit label was supplied, this is the
    ''' one-based source row number formatted using the current culture.
    ''' </summary>
    Public ReadOnly Property RecordLabel As String
        Get
            Return _recordLabel
        End Get
    End Property

    ''' <summary>True when RecordLabel came from a non-missing supplied label value.</summary>
    Public ReadOnly Property HasExplicitRecordLabel As Boolean
        Get
            Return _hasExplicitRecordLabel
        End Get
    End Property

    ''' <summary>Original grouping value, or Nothing when no grouping vector was supplied.</summary>
    Public ReadOnly Property GroupValue As Object
        Get
            Return _groupValue
        End Get
    End Property

    ''' <summary>Normalized grouping key, or an empty string when grouping is not used.</summary>
    Public ReadOnly Property GroupKey As String
        Get
            Return _groupKey
        End Get
    End Property

    ''' <summary>Grouping display text, or an empty string when grouping is not used.</summary>
    Public ReadOnly Property GroupLabel As String
        Get
            Return _groupLabel
        End Get
    End Property

    ''' <summary>
    ''' Zero-based group-level index, or -1 when grouping is not used.
    ''' </summary>
    Public ReadOnly Property GroupLevelIndex As Integer
        Get
            Return _groupLevelIndex
        End Get
    End Property

    ''' <summary>First/Before Y value.</summary>
    Public ReadOnly Property FirstValue As Double
        Get
            Return _firstValue
        End Get
    End Property

    ''' <summary>Second/After Y value.</summary>
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

    ''' <summary>
    ''' Unjittered X coordinate of the First/Before side.
    ''' </summary>
    Public ReadOnly Property FirstBaseXPosition As Double
        Get
            Return LadderPlot.FirstBaseXPosition
        End Get
    End Property

    ''' <summary>
    ''' Unjittered X coordinate of the Second/After side.
    ''' </summary>
    Public ReadOnly Property SecondBaseXPosition As Double
        Get
            Return LadderPlot.SecondBaseXPosition
        End Get
    End Property

    ''' <summary>
    ''' Horizontal offset assigned on the First/Before side. It is zero unless this point
    ''' belongs to a detected collision group and horizontal jitter is enabled.
    ''' </summary>
    Public ReadOnly Property FirstJitterOffset As Double
        Get
            Return _firstJitterOffset
        End Get
    End Property

    ''' <summary>
    ''' Horizontal offset assigned on the Second/After side. It is zero unless this point
    ''' belongs to a detected collision group and horizontal jitter is enabled.
    ''' </summary>
    Public ReadOnly Property SecondJitterOffset As Double
        Get
            Return _secondJitterOffset
        End Get
    End Property

    ''' <summary>Final X coordinate to use for the First/Before point.</summary>
    Public ReadOnly Property FirstXPosition As Double
        Get
            Return LadderPlot.FirstBaseXPosition + _firstJitterOffset
        End Get
    End Property

    ''' <summary>Final X coordinate to use for the Second/After point.</summary>
    Public ReadOnly Property SecondXPosition As Double
        Get
            Return LadderPlot.SecondBaseXPosition + _secondJitterOffset
        End Get
    End Property

    Public ReadOnly Property IsFirstJittered As Boolean
        Get
            Return _firstJitterOffset <> 0.0R
        End Get
    End Property

    Public ReadOnly Property IsSecondJittered As Boolean
        Get
            Return _secondJitterOffset <> 0.0R
        End Get
    End Property
End Class

''' <summary>
''' Immutable result from the Excel-independent ladder-plot data-preparation engine.
''' </summary>
Public NotInheritable Class LadderPlotResult
    Private ReadOnly _observations As LadderPlotObservation()
    Private ReadOnly _groupLevels As LadderPlotGroupLevel()
    Private ReadOnly _sourceObservationCount As Integer
    Private ReadOnly _omittedObservationCount As Integer
    Private ReadOnly _minimumValue As Double
    Private ReadOnly _maximumValue As Double
    Private ReadOnly _recordLabelsSupplied As Boolean
    Private ReadOnly _groupingSupplied As Boolean
    Private ReadOnly _firstCollisionGroupCount As Integer
    Private ReadOnly _secondCollisionGroupCount As Integer
    Private ReadOnly _firstCollisionPointCount As Integer
    Private ReadOnly _secondCollisionPointCount As Integer
    Private ReadOnly _firstJitteredPointCount As Integer
    Private ReadOnly _secondJitteredPointCount As Integer
    Private ReadOnly _options As LadderPlotOptions

    Friend Sub New(observations As LadderPlotObservation(),
                   groupLevels As LadderPlotGroupLevel(),
                   sourceObservationCount As Integer,
                   omittedObservationCount As Integer,
                   minimumValue As Double,
                   maximumValue As Double,
                   recordLabelsSupplied As Boolean,
                   groupingSupplied As Boolean,
                   firstCollisionGroupCount As Integer,
                   secondCollisionGroupCount As Integer,
                   firstCollisionPointCount As Integer,
                   secondCollisionPointCount As Integer,
                   firstJitteredPointCount As Integer,
                   secondJitteredPointCount As Integer,
                   options As LadderPlotOptions)
        _observations = DirectCast(observations.Clone(), LadderPlotObservation())
        _groupLevels = DirectCast(groupLevels.Clone(), LadderPlotGroupLevel())
        _sourceObservationCount = sourceObservationCount
        _omittedObservationCount = omittedObservationCount
        _minimumValue = minimumValue
        _maximumValue = maximumValue
        _recordLabelsSupplied = recordLabelsSupplied
        _groupingSupplied = groupingSupplied
        _firstCollisionGroupCount = firstCollisionGroupCount
        _secondCollisionGroupCount = secondCollisionGroupCount
        _firstCollisionPointCount = firstCollisionPointCount
        _secondCollisionPointCount = secondCollisionPointCount
        _firstJitteredPointCount = firstJitteredPointCount
        _secondJitteredPointCount = secondJitteredPointCount
        _options = options.Copy()
    End Sub

    Public ReadOnly Property Observations As LadderPlotObservation()
        Get
            Return DirectCast(_observations.Clone(), LadderPlotObservation())
        End Get
    End Property

    Public ReadOnly Property GroupLevels As LadderPlotGroupLevel()
        Get
            Return DirectCast(_groupLevels.Clone(), LadderPlotGroupLevel())
        End Get
    End Property

    Public ReadOnly Property ObservationCount As Integer
        Get
            Return _observations.Length
        End Get
    End Property

    Public ReadOnly Property SourceObservationCount As Integer
        Get
            Return _sourceObservationCount
        End Get
    End Property

    Public ReadOnly Property OmittedObservationCount As Integer
        Get
            Return _omittedObservationCount
        End Get
    End Property

    Public ReadOnly Property RecordLabelsSupplied As Boolean
        Get
            Return _recordLabelsSupplied
        End Get
    End Property

    Public ReadOnly Property GroupingSupplied As Boolean
        Get
            Return _groupingSupplied
        End Get
    End Property

    Public ReadOnly Property HasGroups As Boolean
        Get
            Return _groupLevels.Length > 0
        End Get
    End Property

    Public ReadOnly Property GroupCount As Integer
        Get
            Return _groupLevels.Length
        End Get
    End Property

    ''' <summary>Minimum finite Y value among both sides of all retained observations.</summary>
    Public ReadOnly Property MinimumValue As Double
        Get
            Return _minimumValue
        End Get
    End Property

    ''' <summary>Maximum finite Y value among both sides of all retained observations.</summary>
    Public ReadOnly Property MaximumValue As Double
        Get
            Return _maximumValue
        End Get
    End Property

    ''' <summary>Number of duplicate/near-duplicate value groups detected on the First side.</summary>
    Public ReadOnly Property FirstCollisionGroupCount As Integer
        Get
            Return _firstCollisionGroupCount
        End Get
    End Property

    ''' <summary>Number of duplicate/near-duplicate value groups detected on the Second side.</summary>
    Public ReadOnly Property SecondCollisionGroupCount As Integer
        Get
            Return _secondCollisionGroupCount
        End Get
    End Property

    ''' <summary>Total number of First-side points belonging to collision groups.</summary>
    Public ReadOnly Property FirstCollisionPointCount As Integer
        Get
            Return _firstCollisionPointCount
        End Get
    End Property

    ''' <summary>Total number of Second-side points belonging to collision groups.</summary>
    Public ReadOnly Property SecondCollisionPointCount As Integer
        Get
            Return _secondCollisionPointCount
        End Get
    End Property

    ''' <summary>
    ''' Number of First-side points whose final X position differs from the normal First X
    ''' position. This is zero when horizontal jitter is disabled.
    ''' </summary>
    Public ReadOnly Property FirstJitteredPointCount As Integer
        Get
            Return _firstJitteredPointCount
        End Get
    End Property

    ''' <summary>
    ''' Number of Second-side points whose final X position differs from the normal Second X
    ''' position. This is zero when horizontal jitter is disabled.
    ''' </summary>
    Public ReadOnly Property SecondJitteredPointCount As Integer
        Get
            Return _secondJitteredPointCount
        End Get
    End Property

    Public ReadOnly Property HasCollisions As Boolean
        Get
            Return _firstCollisionGroupCount > 0 OrElse _secondCollisionGroupCount > 0
        End Get
    End Property

    Public ReadOnly Property HasHorizontalJitter As Boolean
        Get
            Return _firstJitteredPointCount > 0 OrElse _secondJitteredPointCount > 0
        End Get
    End Property

    ''' <summary>Validated options snapshot used to prepare this result.</summary>
    Public ReadOnly Property Options As LadderPlotOptions
        Get
            Return _options.Copy()
        End Get
    End Property
End Class

''' <summary>
''' Excel-independent backend for two-column ladder/slope plots.
'''
''' Each retained source row represents one paired observation. The renderer can draw a
''' straight line from (FirstXPosition, FirstValue) to (SecondXPosition, SecondValue),
''' optionally colouring observations by record or by grouping level.
'''
''' Horizontal jitter is collision-aware and is intentionally applied independently on the
''' two sides. Non-colliding points always stay exactly at X=1 or X=2. Within each collision
''' group, points are spread symmetrically and deterministically, so repeated runs produce
''' the same geometry without requiring a random seed.
''' </summary>
Public NotInheritable Class LadderPlot
    Private Sub New()
    End Sub

    ''' <summary>Normal X coordinate for the First/Before side.</summary>
    Public Const FirstBaseXPosition As Double = 1.0R

    ''' <summary>Normal X coordinate for the Second/After side.</summary>
    Public Const SecondBaseXPosition As Double = 2.0R

    Private NotInheritable Class WorkingObservation
        Friend SourceIndex As Integer
        Friend DisplayIndex As Integer
        Friend RecordLabel As String
        Friend HasExplicitRecordLabel As Boolean
        Friend GroupValue As Object
        Friend GroupKey As String
        Friend GroupLabel As String
        Friend GroupLevelIndex As Integer = -1
        Friend FirstValue As Double
        Friend SecondValue As Double
        Friend FirstJitterOffset As Double
        Friend SecondJitterOffset As Double
    End Class

    Private NotInheritable Class WorkingGroupLevel
        Friend LevelIndex As Integer
        Friend GroupValue As Object
        Friend GroupKey As String
        Friend GroupLabel As String
    End Class

    Private Structure JitterSummary
        Friend CollisionGroupCount As Integer
        Friend CollisionPointCount As Integer
        Friend JitteredPointCount As Integer
    End Structure

    ''' <summary>
    ''' Prepares a ladder plot from row-aligned First/Before and Second/After values.
    ''' Optional record labels identify individual paired observations, while an optional
    ''' grouping vector can be used by the renderer for group colours and a legend.
    ''' </summary>
    ''' <param name="firstValues">First/Before numeric values.</param>
    ''' <param name="secondValues">Second/After numeric values aligned with firstValues.</param>
    ''' <param name="recordLabels">
    ''' Optional row-aligned identifiers such as subject, country or topic. Missing labels are
    ''' replaced by the one-based source row number and do not cause row omission.
    ''' </param>
    ''' <param name="groupingValues">
    ''' Optional row-aligned categorical grouping values such as Control/Treated. Text, numeric
    ''' and date-like identifiers are supported. If supplied, every retained row must have a
    ''' non-missing group unless OmitIncompleteRows=True.
    ''' </param>
    ''' <param name="options">Optional data-preparation and collision-jitter settings.</param>
    Public Shared Function Compute(firstValues As Array,
                                   secondValues As Array,
                                   Optional recordLabels As Array = Nothing,
                                   Optional groupingValues As Array = Nothing,
                                   Optional options As LadderPlotOptions = Nothing) As LadderPlotResult
        If firstValues Is Nothing Then Throw New ArgumentNullException(NameOf(firstValues))
        If secondValues Is Nothing Then Throw New ArgumentNullException(NameOf(secondValues))

        Dim firstRaw As Object() = CopyVector(firstValues, NameOf(firstValues))
        Dim secondRaw As Object() = CopyVector(secondValues, NameOf(secondValues))

        If firstRaw.Length = 0 Then
            Throw New ArgumentException("At least one paired ladder-plot observation is required.", NameOf(firstValues))
        End If
        If secondRaw.Length <> firstRaw.Length Then
            Throw New ArgumentException("First/Before and Second/After values must contain the same number of rows.")
        End If

        Dim labelsSupplied As Boolean = recordLabels IsNot Nothing
        Dim groupsSupplied As Boolean = groupingValues IsNot Nothing
        Dim labelRaw As Object() = Nothing
        Dim groupRaw As Object() = Nothing

        If labelsSupplied Then
            labelRaw = CopyVector(recordLabels, NameOf(recordLabels))
            If labelRaw.Length <> firstRaw.Length Then
                Throw New ArgumentException("Record labels must contain the same number of rows as the paired values.",
                                            NameOf(recordLabels))
            End If
        End If

        If groupsSupplied Then
            groupRaw = CopyVector(groupingValues, NameOf(groupingValues))
            If groupRaw.Length <> firstRaw.Length Then
                Throw New ArgumentException("Grouping values must contain the same number of rows as the paired values.",
                                            NameOf(groupingValues))
            End If
        End If

        Dim resolvedOptions As LadderPlotOptions = If(options, New LadderPlotOptions()).Copy()
        ValidateOptions(resolvedOptions)

        Dim working As New List(Of WorkingObservation)(firstRaw.Length)
        Dim omittedCount As Integer = 0
        Dim minimumValue As Double = Double.PositiveInfinity
        Dim maximumValue As Double = Double.NegativeInfinity

        Dim groupLookup As Dictionary(Of String, WorkingGroupLevel) = Nothing
        Dim workingGroups As List(Of WorkingGroupLevel) = Nothing
        If groupsSupplied Then
            groupLookup = New Dictionary(Of String, WorkingGroupLevel)(StringComparer.Ordinal)
            workingGroups = New List(Of WorkingGroupLevel)()
        End If

        For i As Integer = 0 To firstRaw.Length - 1
            Dim firstValue As Double = Double.NaN
            Dim secondValue As Double = Double.NaN
            Dim hasFirst As Boolean = TryConvertNumericValue(firstRaw(i), firstValue, NameOf(firstValues), i)
            Dim hasSecond As Boolean = TryConvertNumericValue(secondRaw(i), secondValue, NameOf(secondValues), i)
            Dim missingGroup As Boolean = groupsSupplied AndAlso IsMissingIdentifierValue(groupRaw(i))

            If Not hasFirst OrElse Not hasSecond OrElse missingGroup Then
                If resolvedOptions.OmitIncompleteRows Then
                    omittedCount += 1
                    Continue For
                End If

                If Not hasFirst Then
                    Throw New ArgumentException("First/Before value at row " &
                                                (i + 1).ToString(CultureInfo.CurrentCulture) &
                                                " is missing.", NameOf(firstValues))
                End If
                If Not hasSecond Then
                    Throw New ArgumentException("Second/After value at row " &
                                                (i + 1).ToString(CultureInfo.CurrentCulture) &
                                                " is missing.", NameOf(secondValues))
                End If
                Throw New ArgumentException("Grouping value at row " &
                                            (i + 1).ToString(CultureInfo.CurrentCulture) &
                                            " is missing.", NameOf(groupingValues))
            End If

            Dim recordLabel As String
            Dim hasExplicitRecordLabel As Boolean = False
            If labelsSupplied AndAlso Not IsMissingIdentifierValue(labelRaw(i)) Then
                recordLabel = FormatIdentifier(labelRaw(i))
                hasExplicitRecordLabel = True
            Else
                recordLabel = (i + 1).ToString(CultureInfo.CurrentCulture)
            End If

            Dim item As New WorkingObservation With {
                .SourceIndex = i,
                .DisplayIndex = working.Count,
                .RecordLabel = recordLabel,
                .HasExplicitRecordLabel = hasExplicitRecordLabel,
                .FirstValue = firstValue,
                .SecondValue = secondValue,
                .FirstJitterOffset = 0.0R,
                .SecondJitterOffset = 0.0R
            }

            If groupsSupplied Then
                Dim groupValue As Object = groupRaw(i)
                Dim groupKey As String = BuildIdentifierKey(groupValue, "grouping")
                Dim groupLevel As WorkingGroupLevel = Nothing
                If Not groupLookup.TryGetValue(groupKey, groupLevel) Then
                    groupLevel = New WorkingGroupLevel With {
                        .LevelIndex = workingGroups.Count,
                        .GroupValue = groupValue,
                        .GroupKey = groupKey,
                        .GroupLabel = FormatIdentifier(groupValue)
                    }
                    groupLookup.Add(groupKey, groupLevel)
                    workingGroups.Add(groupLevel)
                End If

                item.GroupValue = groupLevel.GroupValue
                item.GroupKey = groupLevel.GroupKey
                item.GroupLabel = groupLevel.GroupLabel
                item.GroupLevelIndex = groupLevel.LevelIndex
            Else
                item.GroupValue = Nothing
                item.GroupKey = String.Empty
                item.GroupLabel = String.Empty
                item.GroupLevelIndex = -1
            End If

            working.Add(item)
            minimumValue = Math.Min(minimumValue, Math.Min(firstValue, secondValue))
            maximumValue = Math.Max(maximumValue, Math.Max(firstValue, secondValue))
        Next

        If working.Count = 0 Then
            Throw New ArgumentException("No complete paired observation with two finite numeric values is available.")
        End If

        Dim firstJitterSummary As JitterSummary = ApplyCollisionJitter(working,
                                                                      True,
                                                                      resolvedOptions.HorizontalJitterFraction,
                                                                      resolvedOptions.CollisionTolerance)
        Dim secondJitterSummary As JitterSummary = ApplyCollisionJitter(working,
                                                                       False,
                                                                       resolvedOptions.HorizontalJitterFraction,
                                                                       resolvedOptions.CollisionTolerance)

        Dim prepared(working.Count - 1) As LadderPlotObservation
        For i As Integer = 0 To working.Count - 1
            Dim item As WorkingObservation = working(i)
            prepared(i) = New LadderPlotObservation(item.SourceIndex,
                                                    item.DisplayIndex,
                                                    item.RecordLabel,
                                                    item.HasExplicitRecordLabel,
                                                    item.GroupValue,
                                                    item.GroupKey,
                                                    item.GroupLabel,
                                                    item.GroupLevelIndex,
                                                    item.FirstValue,
                                                    item.SecondValue,
                                                    item.FirstJitterOffset,
                                                    item.SecondJitterOffset)
        Next

        Dim preparedGroups As LadderPlotGroupLevel() = Array.Empty(Of LadderPlotGroupLevel)()
        If groupsSupplied Then
            ReDim preparedGroups(workingGroups.Count - 1)
            For i As Integer = 0 To workingGroups.Count - 1
                Dim groupLevel As WorkingGroupLevel = workingGroups(i)
                preparedGroups(i) = New LadderPlotGroupLevel(groupLevel.LevelIndex,
                                                             groupLevel.GroupValue,
                                                             groupLevel.GroupKey,
                                                             groupLevel.GroupLabel)
            Next
        End If

        Return New LadderPlotResult(prepared,
                                    preparedGroups,
                                    firstRaw.Length,
                                    omittedCount,
                                    minimumValue,
                                    maximumValue,
                                    labelsSupplied,
                                    groupsSupplied,
                                    firstJitterSummary.CollisionGroupCount,
                                    secondJitterSummary.CollisionGroupCount,
                                    firstJitterSummary.CollisionPointCount,
                                    secondJitterSummary.CollisionPointCount,
                                    firstJitterSummary.JitteredPointCount,
                                    secondJitterSummary.JitteredPointCount,
                                    resolvedOptions)
    End Function

    ''' <summary>
    ''' Convenience overload for paired values without record labels or grouping.
    ''' </summary>
    Public Shared Function Compute(firstValues As Array,
                                   secondValues As Array,
                                   options As LadderPlotOptions) As LadderPlotResult
        Return Compute(firstValues, secondValues, Nothing, Nothing, options)
    End Function

    ''' <summary>
    ''' Convenience overload for paired values and record labels without grouping.
    ''' </summary>
    Public Shared Function ComputeWithLabels(firstValues As Array,
                                             secondValues As Array,
                                             recordLabels As Array,
                                             Optional options As LadderPlotOptions = Nothing) As LadderPlotResult
        If recordLabels Is Nothing Then Throw New ArgumentNullException(NameOf(recordLabels))
        Return Compute(firstValues, secondValues, recordLabels, Nothing, options)
    End Function

    ''' <summary>
    ''' Convenience overload for paired values, record labels and a grouping variable.
    ''' </summary>
    Public Shared Function ComputeWithGroups(firstValues As Array,
                                             secondValues As Array,
                                             recordLabels As Array,
                                             groupingValues As Array,
                                             Optional options As LadderPlotOptions = Nothing) As LadderPlotResult
        If groupingValues Is Nothing Then Throw New ArgumentNullException(NameOf(groupingValues))
        Return Compute(firstValues, secondValues, recordLabels, groupingValues, options)
    End Function

    ''' <summary>
    ''' Detects collisions on one side of the ladder and, when enabled, spreads only the
    ''' colliding points across the available horizontal jitter band. The spread is symmetric
    ''' about the normal side position and deterministic by SourceIndex.
    ''' </summary>
    Private Shared Function ApplyCollisionJitter(source As List(Of WorkingObservation),
                                                 useFirstSide As Boolean,
                                                 maximumOffset As Double,
                                                 collisionTolerance As Double) As JitterSummary
        Dim summary As New JitterSummary()
        If source Is Nothing OrElse source.Count < 2 Then Return summary

        Dim ordered As List(Of WorkingObservation)
        If useFirstSide Then
            ordered = source.OrderBy(Function(x) x.FirstValue).
                             ThenBy(Function(x) x.SourceIndex).
                             ToList()
        Else
            ordered = source.OrderBy(Function(x) x.SecondValue).
                             ThenBy(Function(x) x.SourceIndex).
                             ToList()
        End If

        Dim group As New List(Of WorkingObservation)()
        Dim groupAnchorValue As Double = 0.0R

        For i As Integer = 0 To ordered.Count - 1
            Dim item As WorkingObservation = ordered(i)
            Dim currentValue As Double = If(useFirstSide, item.FirstValue, item.SecondValue)

            If group.Count = 0 Then
                group.Add(item)
                groupAnchorValue = currentValue
            ElseIf ValuesCollide(groupAnchorValue, currentValue, collisionTolerance) Then
                group.Add(item)
            Else
                SpreadCollisionGroup(group, useFirstSide, maximumOffset, summary)
                group.Clear()
                group.Add(item)
                groupAnchorValue = currentValue
            End If
        Next

        SpreadCollisionGroup(group, useFirstSide, maximumOffset, summary)
        Return summary
    End Function

    Private Shared Sub SpreadCollisionGroup(group As List(Of WorkingObservation),
                                            useFirstSide As Boolean,
                                            maximumOffset As Double,
                                            ByRef summary As JitterSummary)
        If group Is Nothing OrElse group.Count < 2 Then Return

        summary.CollisionGroupCount += 1
        summary.CollisionPointCount += group.Count

        'No geometry change is required when jitter is disabled, but collision diagnostics
        'are still retained in the result.
        If maximumOffset <= 0.0R Then Return

        'SourceIndex ordering makes the assignment deterministic. The offsets span the whole
        'requested band: e.g. n=2 -> {-J,+J}; n=3 -> {-J,0,+J}.
        group.Sort(Function(a, b) a.SourceIndex.CompareTo(b.SourceIndex))

        Dim stepSize As Double = (2.0R * maximumOffset) / CDbl(group.Count - 1)
        For i As Integer = 0 To group.Count - 1
            Dim offset As Double = -maximumOffset + stepSize * CDbl(i)

            'Avoid carrying tiny floating-point residuals for the centre point of odd groups.
            If Math.Abs(offset) < 1.0E-14R Then offset = 0.0R

            If useFirstSide Then
                group(i).FirstJitterOffset = offset
            Else
                group(i).SecondJitterOffset = offset
            End If

            If offset <> 0.0R Then summary.JitteredPointCount += 1
        Next
    End Sub

    Private Shared Function ValuesCollide(firstValue As Double,
                                          secondValue As Double,
                                          tolerance As Double) As Boolean
        If tolerance <= 0.0R Then Return firstValue = secondValue
        Return Math.Abs(secondValue - firstValue) <= tolerance
    End Function

    Private Shared Sub ValidateOptions(options As LadderPlotOptions)
        Dim jitter As Double = options.HorizontalJitterFraction
        If Double.IsNaN(jitter) OrElse Double.IsInfinity(jitter) OrElse jitter < 0.0R OrElse jitter > 0.25R Then
            Throw New ArgumentOutOfRangeException(NameOf(options.HorizontalJitterFraction),
                                                  "HorizontalJitterFraction must be finite and in the interval [0, 0.25].")
        End If

        Dim tolerance As Double = options.CollisionTolerance
        If Double.IsNaN(tolerance) OrElse Double.IsInfinity(tolerance) OrElse tolerance < 0.0R Then
            Throw New ArgumentOutOfRangeException(NameOf(options.CollisionTolerance),
                                                  "CollisionTolerance must be finite and greater than or equal to zero.")
        End If
    End Sub

    ''' <summary>
    ''' Copies a one-dimensional or single-row/single-column two-dimensional array into a
    ''' zero-based Object vector. This accepts both normal core arrays and Excel-range shaped
    ''' Object(,) values.
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

    ''' <summary>
    ''' Converts a plotted Y value to Double. Missing values return False. Infinite values and
    ''' unsupported types are rejected because an Excel XY series cannot plot them safely.
    ''' Numeric strings are accepted using current-culture and invariant-culture parsing.
    ''' </summary>
    Private Shared Function TryConvertNumericValue(rawValue As Object,
                                                   ByRef value As Double,
                                                   parameterName As String,
                                                   rowIndex As Integer) As Boolean
        value = Double.NaN

        If rawValue Is Nothing OrElse Convert.IsDBNull(rawValue) Then Return False
        If TypeOf rawValue Is String AndAlso String.IsNullOrWhiteSpace(CStr(rawValue)) Then Return False

        If TypeOf rawValue Is Boolean Then
            Throw New ArgumentException("Value at row " &
                                        (rowIndex + 1).ToString(CultureInfo.CurrentCulture) &
                                        " in " & parameterName & " is Boolean rather than numeric.",
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
                Throw New ArgumentException("Value '" & text & "' at row " &
                                            (rowIndex + 1).ToString(CultureInfo.CurrentCulture) &
                                            " in " & parameterName & " is not numeric.",
                                            parameterName)
            End If
        Else
            Throw New ArgumentException("Value at row " &
                                        (rowIndex + 1).ToString(CultureInfo.CurrentCulture) &
                                        " in " & parameterName & " is not numeric.",
                                        parameterName)
        End If

        If Double.IsNaN(value) Then Return False
        If Double.IsInfinity(value) Then
            Throw New ArgumentOutOfRangeException(parameterName,
                                                  "Value at row " &
                                                  (rowIndex + 1).ToString(CultureInfo.CurrentCulture) &
                                                  " is infinite.")
        End If
        Return True
    End Function

    Private Shared Function IsMissingIdentifierValue(value As Object) As Boolean
        If value Is Nothing OrElse Convert.IsDBNull(value) Then Return True
        If TypeOf value Is String Then Return String.IsNullOrWhiteSpace(CStr(value))
        Return False
    End Function

    ''' <summary>
    ''' Builds a type-aware key so, for example, numeric 1 and text "1" remain distinct
    ''' categorical group levels.
    ''' </summary>
    Private Shared Function BuildIdentifierKey(value As Object, identifierName As String) As String
        If value Is Nothing OrElse Convert.IsDBNull(value) Then
            Throw New ArgumentException("A " & identifierName & " value is missing.")
        End If

        If TypeOf value Is DateTime Then
            Return "D:" & DirectCast(value, DateTime).ToOADate().ToString("R", CultureInfo.InvariantCulture)
        End If
        If TypeOf value Is DateTimeOffset Then
            Return "D:" & DirectCast(value, DateTimeOffset).DateTime.ToOADate().ToString("R", CultureInfo.InvariantCulture)
        End If
        If TypeOf value Is Boolean Then
            Return "B:" & If(CBool(value), "1", "0")
        End If
        If IsNumeric(value) Then
            Dim numericValue As Double = Convert.ToDouble(value, CultureInfo.InvariantCulture)
            If Double.IsNaN(numericValue) OrElse Double.IsInfinity(numericValue) Then
                Throw New ArgumentException("Numeric " & identifierName & " values must be finite.")
            End If
            Return "N:" & numericValue.ToString("R", CultureInfo.InvariantCulture)
        End If

        Dim text As String = Convert.ToString(value, CultureInfo.InvariantCulture)
        If text Is Nothing Then text = String.Empty
        text = text.Trim()
        If text.Length = 0 Then
            Throw New ArgumentException("A " & identifierName & " value is missing.")
        End If
        Return "S:" & text
    End Function

    Private Shared Function FormatIdentifier(value As Object) As String
        If value Is Nothing OrElse Convert.IsDBNull(value) Then Return String.Empty
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
