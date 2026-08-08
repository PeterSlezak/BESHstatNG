Option Explicit On
Option Strict Off
Option Infer On

Imports System
Imports System.Collections.Generic
Imports System.Globalization
Imports System.Linq
Imports Microsoft.Office.Interop.Excel

''' <summary>
''' Controls which visual attributes vary automatically between grouping levels.
''' </summary>
<Flags>
Public Enum ConvexHullGroupStyleMode
    None = 0
    Color = 1
    Marker = 2
    LineStyle = 4
    ColorAndMarker = Color Or Marker
    ColorAndLineStyle = Color Or LineStyle
    MarkerAndLineStyle = Marker Or LineStyle
    ColorMarkerAndLineStyle = Color Or Marker Or LineStyle
End Enum

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
    ''' Gets X coordinates suitable for an Excel line series. The first point is repeated at the end when there are at least three vertices.
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
''' Immutable result returned by <see cref="ConvexHullPlot.Compute"/>.
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
Public Module ConvexHullPlot
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

''' <summary>
''' Optional per-group display override. Set only the properties that should differ from the plot defaults.
''' </summary>
Public Class ConvexHullGroupAppearance
    Public Property GroupName As String
    Public Property MarkerStyle As Nullable(Of XlMarkerStyle)
    Public Property MarkerSize As Nullable(Of Integer)
    Public Property MarkerForegroundColor As Nullable(Of Integer)
    Public Property MarkerBackgroundColor As Nullable(Of Integer)
    Public Property HullLineColor As Nullable(Of Integer)
    Public Property HullLineWeight As Nullable(Of Single)
    Public Property HullLineStyle As Nullable(Of XlLineStyle)
End Class

''' <summary>
''' Excel chart appearance settings for <see cref="ConvexHullPlotExcel"/>.
''' </summary>
''' <remarks>Colors are OLE RGB integers used by the Excel object model.</remarks>
Public Class ConvexHullPlotAppearance
    Public Property ChartTitle As String = "2D convex hull plot"
    Public Property XAxisTitle As String = "X"
    Public Property YAxisTitle As String = "Y"
    Public Property SeriesName As String = "Data"

    Public Property ShowPoints As Boolean = True
    Public Property ShowHullLine As Boolean = True
    Public Property ShowLegend As Boolean = False
    Public Property ShowGroupLegend As Boolean = True
    Public Property ShowMajorGridlines As Boolean = True

    Public Property GroupStyleMode As ConvexHullGroupStyleMode = ConvexHullGroupStyleMode.ColorAndMarker

    Public Property MarkerStyle As XlMarkerStyle = XlMarkerStyle.xlMarkerStyleCircle
    Public Property MarkerSize As Integer = 6
    Public Property MarkerForegroundColor As Integer = &HB4771F
    Public Property MarkerBackgroundColor As Integer = &HB4771F

    Public Property HullLineColor As Integer = &HB4771F
    Public Property HullLineWeight As Single = 1.5F
    Public Property HullLineStyle As XlLineStyle = XlLineStyle.xlContinuous

    Public Property GroupColors As Integer() = {
        &HB4771F, &HE7FFF, &H2CA02C, &H2827D6, &HBD6794,
        &H4B568C, &HC277E3, &H7F7F7F, &H22BDBC, &HCFBE17
    }

    Public Property GroupMarkerStyles As XlMarkerStyle() = {
        XlMarkerStyle.xlMarkerStyleCircle,
        XlMarkerStyle.xlMarkerStyleSquare,
        XlMarkerStyle.xlMarkerStyleTriangle,
        XlMarkerStyle.xlMarkerStyleDiamond,
        XlMarkerStyle.xlMarkerStyleX,
        XlMarkerStyle.xlMarkerStylePlus,
        XlMarkerStyle.xlMarkerStyleStar,
        XlMarkerStyle.xlMarkerStyleDash
    }

    Public Property GroupLineStyles As XlLineStyle() = {
        XlLineStyle.xlContinuous,
        XlLineStyle.xlDash,
        XlLineStyle.xlDot,
        XlLineStyle.xlDashDot,
        XlLineStyle.xlDashDotDot
    }

    ''' <summary>
    ''' Gets or sets optional group-specific overrides matched case-insensitively by displayed group name.
    ''' </summary>
    Public Property GroupOverrides As ConvexHullGroupAppearance() = New ConvexHullGroupAppearance() {}

    Public Property BackgroundColor As Integer = &HFFFFFF
    Public Property GridlineColor As Integer = &HE6E6E6

    Public Property XAxisMinimum As Nullable(Of Double)
    Public Property XAxisMaximum As Nullable(Of Double)
    Public Property YAxisMinimum As Nullable(Of Double)
    Public Property YAxisMaximum As Nullable(Of Double)
End Class

''' <summary>
''' Renders a <see cref="ConvexHullPlotResult"/> as an embedded Excel XY-scatter chart.
''' </summary>
Public NotInheritable Class ConvexHullPlotExcel
    Private Sub New()
    End Sub

    Private NotInheritable Class ResolvedGroupStyle
        Friend MarkerStyle As XlMarkerStyle
        Friend MarkerSize As Integer
        Friend MarkerForegroundColor As Integer
        Friend MarkerBackgroundColor As Integer
        Friend HullLineColor As Integer
        Friend HullLineWeight As Single
        Friend HullLineStyle As XlLineStyle
    End Class

    Public Shared Function AddChart(ws As Worksheet,
                                    result As ConvexHullPlotResult,
                                    Optional appearance As ConvexHullPlotAppearance = Nothing,
                                    Optional left As Double = 20.0R,
                                    Optional top As Double = 20.0R,
                                    Optional width As Double = 620.0R,
                                    Optional height As Double = 420.0R) As Chart
        If ws Is Nothing Then Throw New ArgumentNullException(NameOf(ws))
        If result Is Nothing Then Throw New ArgumentNullException(NameOf(result))
        If Not IsFinite(left) OrElse Not IsFinite(top) Then
            Throw New ArgumentOutOfRangeException(NameOf(left), "Chart position must be finite.")
        End If
        If Not IsFinitePositive(width) OrElse Not IsFinitePositive(height) Then
            Throw New ArgumentOutOfRangeException(NameOf(width), "Chart width and height must be finite and positive.")
        End If

        Dim resolvedAppearance As ConvexHullPlotAppearance = If(appearance, New ConvexHullPlotAppearance())
        ValidateAppearance(resolvedAppearance, result.HasGrouping)

        Dim chartShape As Shape = Nothing
        Try
            chartShape = ws.Shapes.AddChart(XlChartType.xlXYScatter, left, top, width, height)
            Dim chart As Chart = chartShape.Chart
            chart.ChartType = XlChartType.xlXYScatter
            chart.DisplayBlanksAs = XlDisplayBlanksAs.xlNotPlotted
            chart.PlotVisibleOnly = False
            chart.ChartArea.AutoScaleFont = False

            Dim seriesCollection As SeriesCollection = DirectCast(chart.SeriesCollection(), SeriesCollection)
            DeleteAllSeries(seriesCollection)
            ConfigureBackground(chart, resolvedAppearance)
            ConfigureTitle(chart, resolvedAppearance.ChartTitle)

            Dim legendSeriesIndices As New List(Of Integer)()
            Dim groups As ConvexHullPlotGroup() = result.Groups
            For groupIndex As Integer = 0 To groups.Length - 1
                Dim group As ConvexHullPlotGroup = groups(groupIndex)
                Dim style As ResolvedGroupStyle = ResolveStyle(group, groupIndex, result.HasGrouping, resolvedAppearance)
                Dim seriesName As String = ResolveSeriesName(group, groupIndex, result.HasGrouping, resolvedAppearance)

                If resolvedAppearance.ShowPoints Then
                    legendSeriesIndices.Add(AddMarkerSeries(seriesCollection, group, seriesName, style))
                End If

                If resolvedAppearance.ShowHullLine AndAlso group.HullVertices.Length >= 2 Then
                    Dim hullIndex As Integer = AddHullSeries(seriesCollection, group, seriesName & " hull", style)
                    If Not resolvedAppearance.ShowPoints Then legendSeriesIndices.Add(hullIndex)
                End If
            Next

            If seriesCollection.Count = 0 Then
                Throw New InvalidOperationException("No chart series can be rendered with the current data and display settings.")
            End If

            ConfigureAxes(chart, resolvedAppearance)
            ConfigureLegend(chart,
                            legendSeriesIndices,
                            resolvedAppearance.ShowLegend OrElse (result.HasGrouping AndAlso resolvedAppearance.ShowGroupLegend))
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

    Private Shared Sub ValidateAppearance(appearance As ConvexHullPlotAppearance, hasGrouping As Boolean)
        If Not appearance.ShowPoints AndAlso Not appearance.ShowHullLine Then
            Throw New ArgumentException("At least one of ShowPoints or ShowHullLine must be enabled.", NameOf(appearance))
        End If
        If Not [Enum].IsDefined(GetType(ConvexHullGroupStyleMode), appearance.GroupStyleMode) Then
            Throw New ArgumentOutOfRangeException(NameOf(appearance.GroupStyleMode), "The group style mode is not defined.")
        End If
        If appearance.MarkerSize < 2 OrElse appearance.MarkerSize > 72 Then
            Throw New ArgumentOutOfRangeException(NameOf(appearance.MarkerSize), "Marker size must be between 2 and 72 points.")
        End If
        If Not IsFinitePositive(appearance.HullLineWeight) Then
            Throw New ArgumentOutOfRangeException(NameOf(appearance.HullLineWeight), "Hull line weight must be finite and positive.")
        End If
        If appearance.ShowPoints AndAlso appearance.MarkerStyle = XlMarkerStyle.xlMarkerStyleNone Then
            Throw New ArgumentException("MarkerStyle must be visible when ShowPoints is enabled.", NameOf(appearance.MarkerStyle))
        End If
        If appearance.ShowHullLine AndAlso appearance.HullLineStyle = XlLineStyle.xlLineStyleNone Then
            Throw New ArgumentException("HullLineStyle must be visible when ShowHullLine is enabled.", NameOf(appearance.HullLineStyle))
        End If
        If hasGrouping AndAlso (appearance.GroupStyleMode And ConvexHullGroupStyleMode.Color) <> 0 AndAlso
           (appearance.GroupColors Is Nothing OrElse appearance.GroupColors.Length = 0) Then
            Throw New ArgumentException("GroupColors must contain at least one color.", NameOf(appearance.GroupColors))
        End If
        If hasGrouping AndAlso (appearance.GroupStyleMode And ConvexHullGroupStyleMode.Marker) <> 0 AndAlso
           (appearance.GroupMarkerStyles Is Nothing OrElse appearance.GroupMarkerStyles.Length = 0) Then
            Throw New ArgumentException("GroupMarkerStyles must contain at least one marker style.", NameOf(appearance.GroupMarkerStyles))
        End If
        If hasGrouping AndAlso (appearance.GroupStyleMode And ConvexHullGroupStyleMode.LineStyle) <> 0 AndAlso
           (appearance.GroupLineStyles Is Nothing OrElse appearance.GroupLineStyles.Length = 0) Then
            Throw New ArgumentException("GroupLineStyles must contain at least one line style.", NameOf(appearance.GroupLineStyles))
        End If

        If hasGrouping AndAlso appearance.ShowPoints AndAlso
           (appearance.GroupStyleMode And ConvexHullGroupStyleMode.Marker) <> 0 Then
            For Each marker As XlMarkerStyle In appearance.GroupMarkerStyles
                If marker = XlMarkerStyle.xlMarkerStyleNone Then
                    Throw New ArgumentException("Grouped marker styles must be visible when ShowPoints is enabled.",
                                                NameOf(appearance.GroupMarkerStyles))
                End If
            Next
        End If
        If hasGrouping AndAlso appearance.ShowHullLine AndAlso
           (appearance.GroupStyleMode And ConvexHullGroupStyleMode.LineStyle) <> 0 Then
            For Each lineStyle As XlLineStyle In appearance.GroupLineStyles
                If lineStyle = XlLineStyle.xlLineStyleNone Then
                    Throw New ArgumentException("Grouped line styles must be visible when ShowHullLine is enabled.",
                                                NameOf(appearance.GroupLineStyles))
                End If
            Next
        End If

        ValidateAxisLimits(appearance.XAxisMinimum, appearance.XAxisMaximum, "X")
        ValidateAxisLimits(appearance.YAxisMinimum, appearance.YAxisMaximum, "Y")
    End Sub

    Private Shared Sub ValidateAxisLimits(minimum As Nullable(Of Double),
                                          maximum As Nullable(Of Double),
                                          axisName As String)
        If minimum.HasValue AndAlso Not IsFinite(minimum.Value) Then
            Throw New ArgumentOutOfRangeException(axisName & "AxisMinimum", "Axis limits must be finite.")
        End If
        If maximum.HasValue AndAlso Not IsFinite(maximum.Value) Then
            Throw New ArgumentOutOfRangeException(axisName & "AxisMaximum", "Axis limits must be finite.")
        End If
        If minimum.HasValue AndAlso maximum.HasValue AndAlso minimum.Value >= maximum.Value Then
            Throw New ArgumentException(axisName & " axis minimum must be smaller than its maximum.")
        End If
    End Sub

    Private Shared Sub DeleteAllSeries(seriesCollection As SeriesCollection)
        Do While seriesCollection.Count > 0
            DirectCast(seriesCollection.Item(1), Series).Delete()
        Loop
    End Sub

    Private Shared Function AddMarkerSeries(seriesCollection As SeriesCollection,
                                            group As ConvexHullPlotGroup,
                                            seriesName As String,
                                            style As ResolvedGroupStyle) As Integer
        Dim points As ConvexHullPoint2D() = group.Points
        seriesCollection.NewSeries()
        'Dim series As Series = DirectCast(seriesCollection.Item(seriesCollection.Count), Series)
        With seriesCollection(seriesCollection.Count - 1)
            .Name = seriesName
            .ChartType = XlChartType.xlXYScatter
            .XValues = points.Select(Function(p) p.X).ToArray()
            .Values = points.Select(Function(p) p.Y).ToArray()
            .MarkerStyle = style.MarkerStyle
            .MarkerSize = style.MarkerSize
            .MarkerForegroundColor = style.MarkerForegroundColor
            .MarkerBackgroundColor = style.MarkerBackgroundColor
            .Format.Line.Visible = False
        End With
        Return seriesCollection.Count
    End Function

    Private Shared Function AddHullSeries(seriesCollection As SeriesCollection,
                                          group As ConvexHullPlotGroup,
                                          seriesName As String,
                                          style As ResolvedGroupStyle) As Integer
        seriesCollection.NewSeries()
        Dim series As Series = DirectCast(seriesCollection.Item(seriesCollection.Count), Series)
        With seriesCollection(seriesCollection.Count - 1)
            .Name = seriesName
            .ChartType = XlChartType.xlXYScatterLinesNoMarkers
            .XValues = group.ClosedHullX
            .Values = group.ClosedHullY
            .Smooth = False
            .MarkerStyle = XlMarkerStyle.xlMarkerStyleNone
            .Border.Color = style.HullLineColor
            .Border.LineStyle = style.HullLineStyle
            .Format.Line.Visible = True
            .Format.Line.ForeColor.RGB = style.HullLineColor
            .Format.Line.Weight = style.HullLineWeight
        End With

        Return seriesCollection.Count
    End Function

    Private Shared Function ResolveStyle(group As ConvexHullPlotGroup,
                                         groupIndex As Integer,
                                         hasGrouping As Boolean,
                                         appearance As ConvexHullPlotAppearance) As ResolvedGroupStyle
        Dim varyColor As Boolean = hasGrouping AndAlso (appearance.GroupStyleMode And ConvexHullGroupStyleMode.Color) <> 0
        Dim varyMarker As Boolean = hasGrouping AndAlso (appearance.GroupStyleMode And ConvexHullGroupStyleMode.Marker) <> 0
        Dim varyLine As Boolean = hasGrouping AndAlso (appearance.GroupStyleMode And ConvexHullGroupStyleMode.LineStyle) <> 0

        Dim color As Integer = If(varyColor,
                                  appearance.GroupColors(groupIndex Mod appearance.GroupColors.Length),
                                  appearance.HullLineColor)
        Dim markerColor As Integer = If(varyColor,
                                        appearance.GroupColors(groupIndex Mod appearance.GroupColors.Length),
                                        appearance.MarkerForegroundColor)

        Dim result As New ResolvedGroupStyle With {
            .MarkerStyle = If(varyMarker,
                              appearance.GroupMarkerStyles(groupIndex Mod appearance.GroupMarkerStyles.Length),
                              appearance.MarkerStyle),
            .MarkerSize = appearance.MarkerSize,
            .MarkerForegroundColor = markerColor,
            .MarkerBackgroundColor = If(varyColor, markerColor, appearance.MarkerBackgroundColor),
            .HullLineColor = color,
            .HullLineWeight = appearance.HullLineWeight,
            .HullLineStyle = If(varyLine,
                                appearance.GroupLineStyles(groupIndex Mod appearance.GroupLineStyles.Length),
                                appearance.HullLineStyle)
        }

        Dim overrideStyle As ConvexHullGroupAppearance = FindOverride(group.Name, appearance.GroupOverrides)
        If overrideStyle IsNot Nothing Then
            If overrideStyle.MarkerStyle.HasValue Then result.MarkerStyle = overrideStyle.MarkerStyle.Value
            If overrideStyle.MarkerSize.HasValue Then result.MarkerSize = overrideStyle.MarkerSize.Value
            If overrideStyle.MarkerForegroundColor.HasValue Then result.MarkerForegroundColor = overrideStyle.MarkerForegroundColor.Value
            If overrideStyle.MarkerBackgroundColor.HasValue Then result.MarkerBackgroundColor = overrideStyle.MarkerBackgroundColor.Value
            If overrideStyle.HullLineColor.HasValue Then result.HullLineColor = overrideStyle.HullLineColor.Value
            If overrideStyle.HullLineWeight.HasValue Then result.HullLineWeight = overrideStyle.HullLineWeight.Value
            If overrideStyle.HullLineStyle.HasValue Then result.HullLineStyle = overrideStyle.HullLineStyle.Value
        End If

        If appearance.ShowPoints AndAlso result.MarkerStyle = XlMarkerStyle.xlMarkerStyleNone Then
            Throw New ArgumentException($"Marker style override for group '{group.Name}' must be visible.", "MarkerStyle")
        End If
        If appearance.ShowHullLine AndAlso result.HullLineStyle = XlLineStyle.xlLineStyleNone Then
            Throw New ArgumentException($"Hull line style override for group '{group.Name}' must be visible.", "HullLineStyle")
        End If
        If result.MarkerSize < 2 OrElse result.MarkerSize > 72 Then
            Throw New ArgumentOutOfRangeException("MarkerSize",
                                                  $"Marker size override for group '{group.Name}' must be between 2 and 72 points.")
        End If
        If Not IsFinitePositive(result.HullLineWeight) Then
            Throw New ArgumentOutOfRangeException("HullLineWeight",
                                                  $"Hull line weight override for group '{group.Name}' must be finite and positive.")
        End If
        Return result
    End Function

    Private Shared Function FindOverride(groupName As String,
                                         ovrrides As ConvexHullGroupAppearance()) As ConvexHullGroupAppearance
        If ovrrides Is Nothing Then Return Nothing
        For Each item As ConvexHullGroupAppearance In ovrrides
            If item IsNot Nothing AndAlso
               Not String.IsNullOrWhiteSpace(item.GroupName) AndAlso
               String.Equals(item.GroupName.Trim(), groupName, StringComparison.CurrentCultureIgnoreCase) Then
                Return item
            End If
        Next
        Return Nothing
    End Function

    Private Shared Function ResolveSeriesName(group As ConvexHullPlotGroup,
                                              groupIndex As Integer,
                                              hasGrouping As Boolean,
                                              appearance As ConvexHullPlotAppearance) As String
        If hasGrouping Then
            If Not String.IsNullOrWhiteSpace(group.Name) Then Return group.Name
            Return "Group " & (groupIndex + 1).ToString(CultureInfo.CurrentCulture)
        End If
        Return If(String.IsNullOrWhiteSpace(appearance.SeriesName), "Data", appearance.SeriesName.Trim())
    End Function

    Private Shared Sub ConfigureTitle(chart As Chart, title As String)
        If String.IsNullOrWhiteSpace(title) Then
            chart.HasTitle = False
        Else
            chart.HasTitle = True
            chart.ChartTitle.Text = title.Trim()
        End If
    End Sub

    Private Shared Sub ConfigureBackground(chart As Object, appearance As ConvexHullPlotAppearance)
        chart.ChartArea.Format.Fill.Visible = True
        chart.ChartArea.Format.Fill.Solid()
        chart.ChartArea.Format.Fill.ForeColor.RGB = appearance.BackgroundColor
        chart.PlotArea.Format.Fill.Visible = True
        chart.PlotArea.Format.Fill.Solid()
        chart.PlotArea.Format.Fill.ForeColor.RGB = appearance.BackgroundColor
    End Sub

    Private Shared Sub ConfigureAxes(chart As Chart, appearance As ConvexHullPlotAppearance)
        ConfigureAxis(DirectCast(chart.Axes(XlAxisType.xlCategory), Axis),
                      appearance.XAxisTitle,
                      appearance.XAxisMinimum,
                      appearance.XAxisMaximum,
                      appearance.ShowMajorGridlines,
                      appearance.GridlineColor)
        ConfigureAxis(DirectCast(chart.Axes(XlAxisType.xlValue), Axis),
                      appearance.YAxisTitle,
                      appearance.YAxisMinimum,
                      appearance.YAxisMaximum,
                      appearance.ShowMajorGridlines,
                      appearance.GridlineColor)
    End Sub

    Private Shared Sub ConfigureAxis(axis As Axis,
                                     title As String,
                                     minimum As Nullable(Of Double),
                                     maximum As Nullable(Of Double),
                                     showGridlines As Boolean,
                                     gridlineColor As Integer)
        axis.HasTitle = Not String.IsNullOrWhiteSpace(title)
        If axis.HasTitle Then axis.AxisTitle.Text = title.Trim()

        If minimum.HasValue Then
            axis.MinimumScale = minimum.Value
        Else
            axis.MinimumScaleIsAuto = True
        End If
        If maximum.HasValue Then
            axis.MaximumScale = maximum.Value
        Else
            axis.MaximumScaleIsAuto = True
        End If

        axis.HasMajorGridlines = showGridlines
        If showGridlines Then
            Try
                axis.MajorGridlines.Border.Color = gridlineColor
            Catch
            End Try
        End If
    End Sub

    Private Shared Sub ConfigureLegend(chart As Chart,
                                       keepSeriesIndices As IList(Of Integer),
                                       showLegend As Boolean)
        If Not showLegend OrElse keepSeriesIndices Is Nothing OrElse keepSeriesIndices.Count = 0 Then
            chart.HasLegend = False
            Return
        End If

        chart.HasLegend = True
        chart.Legend.Position = XlLegendPosition.xlLegendPositionBottom
        Dim keep As New HashSet(Of Integer)(keepSeriesIndices)
        Dim entries As LegendEntries = DirectCast(chart.Legend.LegendEntries(), LegendEntries)
        For i As Integer = entries.Count To 1 Step -1
            If Not keep.Contains(i) Then DirectCast(entries.Item(i), LegendEntry).Delete()
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
End Class
