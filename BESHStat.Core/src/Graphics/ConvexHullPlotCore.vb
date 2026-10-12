Option Explicit On
Option Strict On
Option Infer On

Imports System
Imports System.Collections.Generic
Imports System.Globalization
Imports System.Linq

''' <summary>
''' Numerical options used when calculating one or more two-dimensional convex hulls.
''' </summary>
''' <remarks>
''' A zero padding percentage produces a tight hull. Positive padding reproduces the
''' behavior of the supplied VBA workbook: the tight hull is calculated first, each
''' hull vertex is expanded in the positive and negative X/Y directions, and the hull
''' of those generated points is then calculated again.
''' </remarks>
Public Class ConvexHullPlotOptions
    ''' <summary>
    ''' Gets or sets whether collinear points lying on a hull edge are retained as vertices.
    ''' </summary>
    Public Property IncludeCollinearBoundaryPoints As Boolean = True

    ''' <summary>
    ''' Gets or sets the percentage of the within-group X range added on each horizontal side.
    ''' </summary>
    Public Property PaddingPercentX As Double = 0.0R

    ''' <summary>
    ''' Gets or sets the percentage of the within-group Y range added on each vertical side.
    ''' </summary>
    Public Property PaddingPercentY As Double = 0.0R

    ''' <summary>
    ''' Gets or sets the cross-product tolerance used when deciding whether three points are collinear.
    ''' </summary>
    ''' <remarks>
    ''' Zero uses exact floating-point comparison. A small positive value can be useful when
    ''' coordinates are produced by earlier numerical calculations. The tolerance is scaled
    ''' by the square of the largest coordinate magnitude within a group.
    ''' </remarks>
    Public Property CollinearityTolerance As Double = 0.0R

    Friend Function Copy() As ConvexHullPlotOptions
        Return New ConvexHullPlotOptions With {
            .IncludeCollinearBoundaryPoints = IncludeCollinearBoundaryPoints,
            .PaddingPercentX = PaddingPercentX,
            .PaddingPercentY = PaddingPercentY,
            .CollinearityTolerance = CollinearityTolerance
        }
    End Function
End Class

''' <summary>
''' Represents one valid source observation or one synthetic padding vertex.
''' </summary>
Public NotInheritable Class ConvexHullPoint2D
    Private ReadOnly _x As Double
    Private ReadOnly _y As Double
    Private ReadOnly _sourceIndex As Integer
    Private ReadOnly _isSynthetic As Boolean

    Friend Sub New(x As Double, y As Double, sourceIndex As Integer, isSynthetic As Boolean)
        _x = x
        _y = y
        _sourceIndex = sourceIndex
        _isSynthetic = isSynthetic
    End Sub

    Public ReadOnly Property X As Double
        Get
            Return _x
        End Get
    End Property

    Public ReadOnly Property Y As Double
        Get
            Return _y
        End Get
    End Property

    ''' <summary>
    ''' Gets the zero-based source row index, or -1 for a synthetic padding point.
    ''' </summary>
    Public ReadOnly Property SourceIndex As Integer
        Get
            Return _sourceIndex
        End Get
    End Property

    Public ReadOnly Property IsSynthetic As Boolean
        Get
            Return _isSynthetic
        End Get
    End Property
End Class

''' <summary>
''' Contains all source points and the computed hull for one grouping level.
''' </summary>
Public NotInheritable Class ConvexHullPlotGroup
    Private ReadOnly _groupValue As Object
    Private ReadOnly _name As String
    Private ReadOnly _points As ConvexHullPoint2D()
    Private ReadOnly _hullVertices As ConvexHullPoint2D()
    Private ReadOnly _closedHullX As Double()
    Private ReadOnly _closedHullY As Double()
    Private ReadOnly _area As Double
    Private ReadOnly _perimeter As Double

    Friend Sub New(groupValue As Object,
                   name As String,
                   points As ConvexHullPoint2D(),
                   hullVertices As ConvexHullPoint2D(),
                   closedHullX As Double(),
                   closedHullY As Double(),
                   area As Double,
                   perimeter As Double)
        _groupValue = groupValue
        _name = If(name, String.Empty)
        _points = DirectCast(points.Clone(), ConvexHullPoint2D())
        _hullVertices = DirectCast(hullVertices.Clone(), ConvexHullPoint2D())
        _closedHullX = DirectCast(closedHullX.Clone(), Double())
        _closedHullY = DirectCast(closedHullY.Clone(), Double())
        _area = area
        _perimeter = perimeter
    End Sub

    Public ReadOnly Property GroupValue As Object
        Get
            Return _groupValue
        End Get
    End Property

    Public ReadOnly Property Name As String
        Get
            Return _name
        End Get
    End Property

    Public ReadOnly Property Points As ConvexHullPoint2D()
        Get
            Return DirectCast(_points.Clone(), ConvexHullPoint2D())
        End Get
    End Property

    ''' <summary>
    ''' Gets hull vertices in counterclockwise order without repeating the first vertex.
    ''' </summary>
    Public ReadOnly Property HullVertices As ConvexHullPoint2D()
        Get
            Return DirectCast(_hullVertices.Clone(), ConvexHullPoint2D())
        End Get
    End Property

    ''' <summary>
    ''' Gets X coordinates suitable for a closed polygon line series. The first point is repeated at the end when there are at least three vertices.
    ''' </summary>
    Public ReadOnly Property ClosedHullX As Double()
        Get
            Return DirectCast(_closedHullX.Clone(), Double())
        End Get
    End Property

    Public ReadOnly Property ClosedHullY As Double()
        Get
            Return DirectCast(_closedHullY.Clone(), Double())
        End Get
    End Property

    Public ReadOnly Property Area As Double
        Get
            Return _area
        End Get
    End Property

    ''' <summary>
    ''' Gets the closed-boundary perimeter. A two-point degenerate hull has twice the endpoint distance.
    ''' </summary>
    Public ReadOnly Property Perimeter As Double
        Get
            Return _perimeter
        End Get
    End Property

    Public ReadOnly Property IsDegenerate As Boolean
        Get
            Return _hullVertices.Length < 3
        End Get
    End Property
End Class

''' <summary>
''' Immutable result returned by <see cref="ConvexHullPlotCore.Compute"/>.
''' </summary>
Public NotInheritable Class ConvexHullPlotResult
    Private ReadOnly _groups As ConvexHullPlotGroup()
    Private ReadOnly _hasGrouping As Boolean
    Private ReadOnly _sourceObservationCount As Integer
    Private ReadOnly _validObservationCount As Integer
    Private ReadOnly _omittedObservationCount As Integer

    Friend Sub New(groups As ConvexHullPlotGroup(),
                   hasGrouping As Boolean,
                   sourceObservationCount As Integer,
                   validObservationCount As Integer,
                   omittedObservationCount As Integer)
        _groups = DirectCast(groups.Clone(), ConvexHullPlotGroup())
        _hasGrouping = hasGrouping
        _sourceObservationCount = sourceObservationCount
        _validObservationCount = validObservationCount
        _omittedObservationCount = omittedObservationCount
    End Sub

    Public ReadOnly Property Groups As ConvexHullPlotGroup()
        Get
            Return DirectCast(_groups.Clone(), ConvexHullPlotGroup())
        End Get
    End Property

    Public ReadOnly Property HasGrouping As Boolean
        Get
            Return _hasGrouping
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

    Public ReadOnly Property OmittedObservationCount As Integer
        Get
            Return _omittedObservationCount
        End Get
    End Property
End Class

''' <summary>
''' Computes tight or padded two-dimensional convex hulls for ungrouped or grouped data.
''' </summary>
''' <remarks>
''' The implementation uses Andrew's monotone-chain algorithm. Duplicate coordinates
''' are removed only for hull calculation; they remain in each group's <see cref="ConvexHullPlotGroup.Points"/>
''' collection so the renderer can preserve all source observations.
''' </remarks>
Public Module ConvexHullPlotCore
    Private NotInheritable Class GroupBuilder
        Friend Key As String
        Friend Name As String
        Friend Value As Object
        Friend ReadOnly Points As New List(Of ConvexHullPoint2D)()
    End Class

    Private NotInheritable Class CoordinateComparer
        Implements IComparer(Of ConvexHullPoint2D)

        Public Function Compare(left As ConvexHullPoint2D,
                                right As ConvexHullPoint2D) As Integer Implements IComparer(Of ConvexHullPoint2D).Compare
            Dim xCompare As Integer = left.X.CompareTo(right.X)
            If xCompare <> 0 Then Return xCompare
            Return left.Y.CompareTo(right.Y)
        End Function
    End Class

    ''' <summary>
    ''' Calculates a convex hull for all valid X/Y pairs.
    ''' </summary>
    Public Function Compute(xValues As Double(),
                            yValues As Double(),
                            Optional options As ConvexHullPlotOptions = Nothing,
                            Optional groupingValues As Array = Nothing) As ConvexHullPlotResult
        If xValues Is Nothing Then Throw New ArgumentNullException(NameOf(xValues))
        If yValues Is Nothing Then Throw New ArgumentNullException(NameOf(yValues))
        If xValues.Length <> yValues.Length Then
            Throw New ArgumentException("X and Y arrays must contain the same number of observations.")
        End If
        If xValues.Length = 0 Then
            Throw New ArgumentException("At least one X/Y observation is required.")
        End If

        Dim resolvedOptions As ConvexHullPlotOptions = If(options, New ConvexHullPlotOptions()).Copy()
        ValidateOptions(resolvedOptions)

        Dim hasGrouping As Boolean = groupingValues IsNot Nothing
        Dim groupsRaw As Object() = Nothing
        If hasGrouping Then
            groupsRaw = CopyGroupingValues(groupingValues)
            If groupsRaw.Length <> xValues.Length Then
                Throw New ArgumentException("The grouping array must contain the same number of observations as X and Y.",
                                            NameOf(groupingValues))
            End If
        End If

        Dim builders As New List(Of GroupBuilder)()
        Dim builderByKey As New Dictionary(Of String, GroupBuilder)(StringComparer.Ordinal)
        Dim validCount As Integer = 0
        Dim omittedCount As Integer = 0

        For i As Integer = 0 To xValues.Length - 1
            Dim x As Double = xValues(i)
            Dim y As Double = yValues(i)

            If Double.IsInfinity(x) Then
                Throw New ArgumentOutOfRangeException(NameOf(xValues), $"X observation {i + 1} is infinite.")
            End If
            If Double.IsInfinity(y) Then
                Throw New ArgumentOutOfRangeException(NameOf(yValues), $"Y observation {i + 1} is infinite.")
            End If
            If Double.IsNaN(x) OrElse Double.IsNaN(y) Then
                omittedCount += 1
                Continue For
            End If

            Dim key As String = "__ALL__"
            Dim displayName As String = String.Empty
            Dim normalizedValue As Object = Nothing
            If hasGrouping AndAlso Not TryNormalizeGroupValue(groupsRaw(i), key, displayName, normalizedValue, i) Then
                omittedCount += 1
                Continue For
            End If

            Dim builder As GroupBuilder = Nothing
            If Not builderByKey.TryGetValue(key, builder) Then
                builder = New GroupBuilder With {
                    .Key = key,
                    .Name = displayName,
                    .Value = normalizedValue
                }
                builderByKey.Add(key, builder)
                builders.Add(builder)
            End If

            builder.Points.Add(New ConvexHullPoint2D(x, y, i, False))
            validCount += 1
        Next

        If validCount = 0 Then
            Throw New ArgumentException("No complete finite X/Y observation with a usable group value is available.")
        End If

        Dim results As New List(Of ConvexHullPlotGroup)(builders.Count)
        For Each builder As GroupBuilder In builders
            Dim scale As Double = CoordinateScale(builder.Points)
            ValidateCoordinateScale(scale, builder.Name)
            Dim tolerance As Double = ResolveCrossTolerance(resolvedOptions.CollinearityTolerance, scale)
            Dim hull As List(Of ConvexHullPoint2D) = ComputeMonotoneChain(builder.Points,
                                                                          resolvedOptions.IncludeCollinearBoundaryPoints,
                                                                          tolerance)

            If resolvedOptions.PaddingPercentX > 0.0R OrElse resolvedOptions.PaddingPercentY > 0.0R Then
                Dim paddedPoints As List(Of ConvexHullPoint2D) = BuildPaddingPoints(hull,
                                                                                   builder.Points,
                                                                                   resolvedOptions.PaddingPercentX,
                                                                                   resolvedOptions.PaddingPercentY)
                scale = CoordinateScale(paddedPoints)
                ValidateCoordinateScale(scale, builder.Name)
                tolerance = ResolveCrossTolerance(resolvedOptions.CollinearityTolerance, scale)
                hull = ComputeMonotoneChain(paddedPoints,
                                             resolvedOptions.IncludeCollinearBoundaryPoints,
                                             tolerance)
            End If

            Dim closedX As Double() = Nothing
            Dim closedY As Double() = Nothing
            BuildClosedCoordinates(hull, closedX, closedY)

            results.Add(New ConvexHullPlotGroup(builder.Value,
                                                builder.Name,
                                                builder.Points.ToArray(),
                                                hull.ToArray(),
                                                closedX,
                                                closedY,
                                                CalculateArea(hull),
                                                CalculatePerimeter(hull)))
        Next

        Return New ConvexHullPlotResult(results.ToArray(),
                                        hasGrouping,
                                        xValues.Length,
                                        validCount,
                                        omittedCount)
    End Function

    ''' <summary>
    ''' Calculates grouped hulls using a grouping-first argument order convenient for UI callers.
    ''' </summary>
    Public Function ComputeGrouped(xValues As Double(),
                                   yValues As Double(),
                                   groupingValues As Array,
                                   Optional options As ConvexHullPlotOptions = Nothing) As ConvexHullPlotResult
        If groupingValues Is Nothing Then Throw New ArgumentNullException(NameOf(groupingValues))
        Return Compute(xValues, yValues, options, groupingValues)
    End Function

    Private Sub ValidateOptions(options As ConvexHullPlotOptions)
        ValidateFiniteNonnegative(options.PaddingPercentX, NameOf(options.PaddingPercentX))
        ValidateFiniteNonnegative(options.PaddingPercentY, NameOf(options.PaddingPercentY))
        ValidateFiniteNonnegative(options.CollinearityTolerance, NameOf(options.CollinearityTolerance))
    End Sub

    Private Sub ValidateFiniteNonnegative(value As Double, optionName As String)
        If Double.IsNaN(value) OrElse Double.IsInfinity(value) OrElse value < 0.0R Then
            Throw New ArgumentOutOfRangeException(optionName, "The value must be finite and nonnegative.")
        End If
    End Sub

    Private Function CopyGroupingValues(values As Array) As Object()
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
                Throw New ArgumentException("The grouping input must be one-dimensional or a single-row/single-column two-dimensional array.",
                                            NameOf(values))
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

        Throw New ArgumentException("The grouping input must be one-dimensional or a single-row/single-column two-dimensional array.",
                                    NameOf(values))
    End Function

    Private Function TryNormalizeGroupValue(value As Object,
                                            ByRef key As String,
                                            ByRef displayName As String,
                                            ByRef normalizedValue As Object,
                                            sourceIndex As Integer) As Boolean
        key = Nothing
        displayName = Nothing
        normalizedValue = Nothing

        If value Is Nothing OrElse value Is DBNull.Value Then Return False

        If TypeOf value Is String OrElse TypeOf value Is Char Then
            Dim text As String = Convert.ToString(value, CultureInfo.CurrentCulture).Trim()
            If text.Length = 0 Then Return False
            key = "S:" & text
            displayName = text
            normalizedValue = text
            Return True
        End If

        Select Case Type.GetTypeCode(value.GetType())
            Case TypeCode.Byte, TypeCode.SByte,
                 TypeCode.Int16, TypeCode.UInt16,
                 TypeCode.Int32, TypeCode.UInt32,
                 TypeCode.Int64, TypeCode.UInt64,
                 TypeCode.Decimal
                Dim decimalValue As Decimal = Convert.ToDecimal(value, CultureInfo.InvariantCulture)
                Dim canonical As String = decimalValue.ToString("G29", CultureInfo.InvariantCulture)
                key = "N:" & canonical
                displayName = decimalValue.ToString("G29", CultureInfo.CurrentCulture)
                normalizedValue = value
                Return True

            Case TypeCode.Single
                Dim singleValue As Single = Convert.ToSingle(value, CultureInfo.InvariantCulture)
                If Single.IsNaN(singleValue) Then Return False
                If Single.IsInfinity(singleValue) Then
                    Throw New ArgumentOutOfRangeException(NameOf(value), $"Grouping observation {sourceIndex + 1} is infinite.")
                End If
                key = "N:" & singleValue.ToString("R", CultureInfo.InvariantCulture)
                displayName = singleValue.ToString("0.########", CultureInfo.CurrentCulture)
                normalizedValue = value
                Return True

            Case TypeCode.Double
                Dim doubleValue As Double = Convert.ToDouble(value, CultureInfo.InvariantCulture)
                If Double.IsNaN(doubleValue) Then Return False
                If Double.IsInfinity(doubleValue) Then
                    Throw New ArgumentOutOfRangeException(NameOf(value), $"Grouping observation {sourceIndex + 1} is infinite.")
                End If
                key = "N:" & doubleValue.ToString("R", CultureInfo.InvariantCulture)
                displayName = doubleValue.ToString("0.###############", CultureInfo.CurrentCulture)
                normalizedValue = value
                Return True

            Case Else
                Throw New ArgumentException(
                    $"Grouping observation {sourceIndex + 1} has unsupported type '{value.GetType().Name}'. Only text and numeric values are supported.",
                    NameOf(value))
        End Select
    End Function

    Private Function ComputeMonotoneChain(source As IEnumerable(Of ConvexHullPoint2D),
                                          includeCollinear As Boolean,
                                          tolerance As Double) As List(Of ConvexHullPoint2D)
        Dim sorted As List(Of ConvexHullPoint2D) = source.OrderBy(Function(p) p, New CoordinateComparer()).ToList()
        Dim unique As New List(Of ConvexHullPoint2D)(sorted.Count)
        For Each point As ConvexHullPoint2D In sorted
            If unique.Count = 0 OrElse point.X <> unique(unique.Count - 1).X OrElse point.Y <> unique(unique.Count - 1).Y Then
                unique.Add(point)
            End If
        Next

        If unique.Count <= 2 Then Return unique

        If includeCollinear AndAlso AreAllCollinear(unique, tolerance) Then
            Return unique
        End If

        Dim lower As New List(Of ConvexHullPoint2D)()
        For Each point As ConvexHullPoint2D In unique
            While lower.Count >= 2 AndAlso ShouldRemoveTurn(lower(lower.Count - 2),
                                                            lower(lower.Count - 1),
                                                            point,
                                                            includeCollinear,
                                                            tolerance)
                lower.RemoveAt(lower.Count - 1)
            End While
            lower.Add(point)
        Next

        Dim upper As New List(Of ConvexHullPoint2D)()
        For i As Integer = unique.Count - 1 To 0 Step -1
            Dim point As ConvexHullPoint2D = unique(i)
            While upper.Count >= 2 AndAlso ShouldRemoveTurn(upper(upper.Count - 2),
                                                            upper(upper.Count - 1),
                                                            point,
                                                            includeCollinear,
                                                            tolerance)
                upper.RemoveAt(upper.Count - 1)
            End While
            upper.Add(point)
        Next

        lower.RemoveAt(lower.Count - 1)
        upper.RemoveAt(upper.Count - 1)
        lower.AddRange(upper)
        Return lower
    End Function

    Private Function ShouldRemoveTurn(a As ConvexHullPoint2D,
                                      b As ConvexHullPoint2D,
                                      c As ConvexHullPoint2D,
                                      includeCollinear As Boolean,
                                      tolerance As Double) As Boolean
        Dim cross_ As Double = Cross(a, b, c)
        If includeCollinear Then Return cross_ < -tolerance
        Return cross_ <= tolerance
    End Function

    Private Function AreAllCollinear(points As IList(Of ConvexHullPoint2D), tolerance As Double) As Boolean
        For i As Integer = 2 To points.Count - 1
            If Math.Abs(Cross(points(0), points(1), points(i))) > tolerance Then Return False
        Next
        Return True
    End Function

    Private Function Cross(origin As ConvexHullPoint2D,
                           a As ConvexHullPoint2D,
                           b As ConvexHullPoint2D) As Double
        Return (a.X - origin.X) * (b.Y - origin.Y) - (a.Y - origin.Y) * (b.X - origin.X)
    End Function

    Private Function CoordinateScale(points As IEnumerable(Of ConvexHullPoint2D)) As Double
        Dim scale As Double = 1.0R
        For Each point As ConvexHullPoint2D In points
            scale = Math.Max(scale, Math.Abs(point.X))
            scale = Math.Max(scale, Math.Abs(point.Y))
        Next
        Return scale
    End Function


    Private Sub ValidateCoordinateScale(scale As Double, groupName As String)
        Dim safeLimit As Double = Math.Sqrt(Double.MaxValue) / 4.0R
        If Not IsFiniteNumber(scale) OrElse scale > safeLimit Then
            Dim label As String = If(String.IsNullOrWhiteSpace(groupName), "the data", "group '" & groupName & "'")
            Throw New ArgumentOutOfRangeException(NameOf(scale),
                                                  "Coordinate magnitudes in " & label & " are too large for stable convex-hull arithmetic.")
        End If
    End Sub

    Private Function ResolveCrossTolerance(toleranceFactor As Double, scale As Double) As Double
        If toleranceFactor = 0.0R Then Return 0.0R
        Dim result As Double = toleranceFactor * scale * scale
        If Not IsFiniteNumber(result) Then
            Throw New ArgumentOutOfRangeException(NameOf(toleranceFactor),
                                                  "The collinearity tolerance is too large for the coordinate scale.")
        End If
        Return result
    End Function

    Private Function IsFiniteNumber(value As Double) As Boolean
        Return Not Double.IsNaN(value) AndAlso Not Double.IsInfinity(value)
    End Function

    Private Function BuildPaddingPoints(hull As IList(Of ConvexHullPoint2D),
                                        allPoints As IList(Of ConvexHullPoint2D),
                                        paddingPercentX As Double,
                                        paddingPercentY As Double) As List(Of ConvexHullPoint2D)
        If hull.Count = 0 Then Return New List(Of ConvexHullPoint2D)()

        Dim minX As Double = allPoints.Min(Function(p) p.X)
        Dim maxX As Double = allPoints.Max(Function(p) p.X)
        Dim minY As Double = allPoints.Min(Function(p) p.Y)
        Dim maxY As Double = allPoints.Max(Function(p) p.Y)
        Dim xPadding As Double = (maxX - minX) * paddingPercentX / 100.0R
        Dim yPadding As Double = (maxY - minY) * paddingPercentY / 100.0R
        If Not IsFiniteNumber(xPadding) OrElse Not IsFiniteNumber(yPadding) Then
            Throw New ArgumentOutOfRangeException(NameOf(paddingPercentX),
                                                  "The requested padding is too large for the coordinate range.")
        End If

        Dim expanded As New List(Of ConvexHullPoint2D)(hull.Count * 5)
        For Each point As ConvexHullPoint2D In hull
            expanded.Add(point)
            If xPadding > 0.0R Then
                AddSyntheticPoint(expanded, point.X + xPadding, point.Y)
                AddSyntheticPoint(expanded, point.X - xPadding, point.Y)
            End If
            If yPadding > 0.0R Then
                AddSyntheticPoint(expanded, point.X, point.Y + yPadding)
                AddSyntheticPoint(expanded, point.X, point.Y - yPadding)
            End If
        Next
        Return expanded
    End Function


    Private Sub AddSyntheticPoint(target As IList(Of ConvexHullPoint2D), x As Double, y As Double)
        If Not IsFiniteNumber(x) OrElse Not IsFiniteNumber(y) Then
            Throw New ArgumentOutOfRangeException(NameOf(x),
                                                  "The requested padding produces a non-finite hull coordinate.")
        End If
        target.Add(New ConvexHullPoint2D(x, y, -1, True))
    End Sub

    Private Sub BuildClosedCoordinates(hull As IList(Of ConvexHullPoint2D),
                                       ByRef xValues As Double(),
                                       ByRef yValues As Double())
        If hull.Count = 0 Then
            xValues = New Double() {}
            yValues = New Double() {}
            Return
        End If

        Dim closePolygon As Boolean = hull.Count >= 3
        Dim count As Integer = hull.Count + If(closePolygon, 1, 0)
        ReDim xValues(count - 1)
        ReDim yValues(count - 1)

        For i As Integer = 0 To hull.Count - 1
            xValues(i) = hull(i).X
            yValues(i) = hull(i).Y
        Next
        If closePolygon Then
            xValues(count - 1) = hull(0).X
            yValues(count - 1) = hull(0).Y
        End If
    End Sub

    Private Function CalculateArea(hull As IList(Of ConvexHullPoint2D)) As Double
        If hull.Count < 3 Then Return 0.0R
        Dim twiceArea As Double = 0.0R
        For i As Integer = 0 To hull.Count - 1
            Dim j As Integer = (i + 1) Mod hull.Count
            twiceArea += hull(i).X * hull(j).Y - hull(j).X * hull(i).Y
        Next
        Return Math.Abs(twiceArea) / 2.0R
    End Function

    Private Function CalculatePerimeter(hull As IList(Of ConvexHullPoint2D)) As Double
        If hull.Count <= 1 Then Return 0.0R
        If hull.Count = 2 Then Return 2.0R * Distance(hull(0), hull(1))

        Dim result As Double = 0.0R
        For i As Integer = 0 To hull.Count - 1
            result += Distance(hull(i), hull((i + 1) Mod hull.Count))
        Next
        Return result
    End Function

    Private Function Distance(a As ConvexHullPoint2D, b As ConvexHullPoint2D) As Double
        Dim dx As Double = b.X - a.X
        Dim dy As Double = b.Y - a.Y
        Return Math.Sqrt(dx * dx + dy * dy)
    End Function
End Module
