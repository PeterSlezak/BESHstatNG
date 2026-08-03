Option Explicit On
Option Infer On

Imports System
Imports System.Collections.Generic
Imports System.Globalization
Imports Microsoft.Office.Interop.Excel

''' <summary>
''' Specifies the unit used by the angle input of a polar plot.
''' </summary>
''' <remarks>
''' The selected unit affects only the interpretation of the angle variable. It
''' does not change the radius scale.
''' </remarks>
Public Enum PolarAngleUnit
    ''' <summary>
    ''' Angles are expressed in radians; one complete turn equals <c>2 * PI</c>.
    ''' </summary>
    Radians

    ''' <summary>
    ''' Angles are expressed in degrees; one complete turn equals 360 degrees.
    ''' </summary>
    Degrees

    ''' <summary>
    ''' Angles are expressed as percentages of a complete turn; one complete
    ''' turn equals 100 and a quarter turn equals 25.
    ''' </summary>
    Percentage
End Enum

''' <summary>
''' Specifies the direction in which positive polar angles increase.
''' </summary>
Public Enum PolarRotation
    ''' <summary>
    ''' Positive angles proceed clockwise from the selected zero-angle direction.
    ''' </summary>
    Clockwise

    ''' <summary>
    ''' Positive angles proceed counterclockwise from the selected zero-angle direction.
    ''' </summary>
    Counterclockwise
End Enum

''' <summary>
''' Specifies the compass direction at which an angle of zero is drawn.
''' </summary>
Public Enum PolarZeroAngle
    ''' <summary>
    ''' Zero is drawn at the top of the plot.
    ''' </summary>
    North

    ''' <summary>
    ''' Zero is drawn at the right-hand side of the plot.
    ''' </summary>
    East

    ''' <summary>
    ''' Zero is drawn at the bottom of the plot.
    ''' </summary>
    South

    ''' <summary>
    ''' Zero is drawn at the left-hand side of the plot.
    ''' </summary>
    West
End Enum

''' <summary>
''' Specifies how grouped polar-plot series are distinguished in the Excel renderer.
''' </summary>
Public Enum PolarGroupStyleMode
    ''' <summary>
    ''' Every group uses a different color while retaining the configured marker shape.
    ''' </summary>
    Color

    ''' <summary>
    ''' Every group uses a different marker shape while retaining the configured data color.
    ''' </summary>
    Marker

    ''' <summary>
    ''' Every group uses both a different color and a different marker shape.
    ''' </summary>
    ColorAndMarker
End Enum

''' <summary>
''' Contains the numerical and directional options used to compute a polar plot.
''' </summary>
''' <remarks>
''' The defaults follow the conventional mathematical polar coordinate system:
''' degrees, counterclockwise rotation, and zero at East. The object is copied by
''' <see cref="PolarPlot"/> when the model is constructed, so changing an options
''' object later does not change an existing plot model.
''' </remarks>
Public Class PolarPlotOptions
    ''' <summary>
    ''' Gets or sets the unit used by the supplied angle values.
    ''' </summary>
    Public Property AngleUnit As PolarAngleUnit = PolarAngleUnit.Degrees

    ''' <summary>
    ''' Gets or sets the direction in which positive angles increase.
    ''' </summary>
    Public Property Rotation As PolarRotation = PolarRotation.Counterclockwise

    ''' <summary>
    ''' Gets or sets the compass direction used for an angle of zero.
    ''' </summary>
    Public Property ZeroAngle As PolarZeroAngle = PolarZeroAngle.East

    ''' <summary>
    ''' Gets or sets whether consecutive observations are connected by straight lines.
    ''' </summary>
    ''' <remarks>
    ''' Missing observations always break a line. The final point is not connected
    ''' automatically to the first point; repeat the first observation explicitly
    ''' when a closed curve is required.
    ''' </remarks>
    Public Property ConnectPoints As Boolean = True

    ''' <summary>
    ''' Gets or sets the optional radial-axis value mapped to the centre of the plot.
    ''' </summary>
    ''' <remarks>
    ''' <see langword="Nothing"/> requests an automatic lower limit. A supplied
    ''' value must be finite and smaller than <see cref="RadialMaximum"/> when
    ''' both limits are supplied. Observations below the resolved limit are
    ''' retained in <see cref="PolarPlotResult.Points"/> but are not rendered.
    ''' </remarks>
    Public Property RadialMinimum As Nullable(Of Double) = Nothing

    ''' <summary>
    ''' Gets or sets the optional radial-axis value represented by the outer circle.
    ''' </summary>
    ''' <remarks>
    ''' <see langword="Nothing"/> requests an automatic upper limit. A supplied
    ''' value must be finite and greater than <see cref="RadialMinimum"/> when
    ''' both limits are supplied. Observations above the resolved limit are
    ''' retained in <see cref="PolarPlotResult.Points"/> but are not rendered.
    ''' </remarks>
    Public Property RadialMaximum As Nullable(Of Double) = Nothing

    ''' <summary>
    ''' Gets or sets the optional interval between radial grid circles and labels.
    ''' </summary>
    ''' <remarks>
    ''' <see langword="Nothing"/> selects a readable automatic 1-2-5 interval.
    ''' A supplied value must be finite and strictly positive. The resolved outer
    ''' limit is always drawn even when the interval does not divide the radial span.
    ''' </remarks>
    Public Property RadialTickInterval As Nullable(Of Double) = Nothing

    ''' <summary>
    ''' Gets or sets the optional interval between angular spokes and labels.
    ''' </summary>
    ''' <remarks>
    ''' The value uses <see cref="AngleUnit"/>: for example, 30 means 30 degrees
    ''' when <see cref="PolarAngleUnit.Degrees"/> is selected. <see langword="Nothing"/>
    ''' preserves the original 45-degree spacing. A supplied interval must be
    ''' finite, strictly positive, and no larger than one complete turn.
    ''' </remarks>
    Public Property AngularTickInterval As Nullable(Of Double) = Nothing

    ''' <summary>
    ''' Creates an independent copy of this options object.
    ''' </summary>
    ''' <returns>A new <see cref="PolarPlotOptions"/> containing the same settings.</returns>
    Friend Function Copy() As PolarPlotOptions
        Return New PolarPlotOptions With {
            .AngleUnit = AngleUnit,
            .Rotation = Rotation,
            .ZeroAngle = ZeroAngle,
            .ConnectPoints = ConnectPoints,
            .RadialMinimum = RadialMinimum,
            .RadialMaximum = RadialMaximum,
            .RadialTickInterval = RadialTickInterval,
            .AngularTickInterval = AngularTickInterval
        }
    End Function
End Class

''' <summary>
''' Represents one source observation after conversion to polar and Cartesian coordinates.
''' </summary>
''' <remarks>
''' Instances are immutable. For a missing observation, <see cref="IsMissing"/> is
''' <see langword="True"/> and all derived numerical properties contain
''' <see cref="Double.NaN"/>. The original radius and angle are retained so callers
''' can associate the computed geometry with the source row. A complete observation
''' outside manual radial limits has <see cref="IsOutsideRadialLimits"/> set and is
''' excluded from marker and line series.
''' </remarks>
Public NotInheritable Class PolarPlotPoint
    Private ReadOnly _sourceIndex As Integer
    Private ReadOnly _originalRadius As Double
    Private ReadOnly _originalAngle As Double
    Private ReadOnly _normalizedAngleRadians As Double
    Private ReadOnly _plotAngleRadians As Double
    Private ReadOnly _plotRadius As Double
    Private ReadOnly _x As Double
    Private ReadOnly _y As Double
    Private ReadOnly _isMissing As Boolean
    Private ReadOnly _isOutsideRadialLimits As Boolean

    ''' <summary>
    ''' Initializes one immutable polar-plot observation.
    ''' </summary>
    ''' <param name="sourceIndex">Zero-based position in the supplied arrays.</param>
    ''' <param name="originalRadius">Unmodified source radius.</param>
    ''' <param name="originalAngle">Unmodified source angle.</param>
    ''' <param name="normalizedAngleRadians">Input angle normalized to the interval [0, 2 PI).</param>
    ''' <param name="plotAngleRadians">Normalized Cartesian drawing angle after direction and zero-position conversion.</param>
    ''' <param name="plotRadius">Nonnegative distance from the centre of the rendered plot.</param>
    ''' <param name="x">Computed Cartesian X coordinate.</param>
    ''' <param name="y">Computed Cartesian Y coordinate.</param>
    ''' <param name="isMissing">Whether the observation represents a missing radius-angle pair.</param>
    ''' <param name="isOutsideRadialLimits">Whether a complete observation lies outside the resolved radial limits.</param>
    Friend Sub New(sourceIndex As Integer,
                   originalRadius As Double,
                   originalAngle As Double,
                   normalizedAngleRadians As Double,
                   plotAngleRadians As Double,
                   plotRadius As Double,
                   x As Double,
                   y As Double,
                   isMissing As Boolean,
                   isOutsideRadialLimits As Boolean)
        _sourceIndex = sourceIndex
        _originalRadius = originalRadius
        _originalAngle = originalAngle
        _normalizedAngleRadians = normalizedAngleRadians
        _plotAngleRadians = plotAngleRadians
        _plotRadius = plotRadius
        _x = x
        _y = y
        _isMissing = isMissing
        _isOutsideRadialLimits = isOutsideRadialLimits
    End Sub

    ''' <summary>
    ''' Gets the zero-based position of the observation in the supplied arrays.
    ''' </summary>
    Public ReadOnly Property SourceIndex As Integer
        Get
            Return _sourceIndex
        End Get
    End Property

    ''' <summary>
    ''' Gets the unmodified source radius.
    ''' </summary>
    Public ReadOnly Property OriginalRadius As Double
        Get
            Return _originalRadius
        End Get
    End Property

    ''' <summary>
    ''' Gets the unmodified source angle in the unit specified by the plot options.
    ''' </summary>
    Public ReadOnly Property OriginalAngle As Double
        Get
            Return _originalAngle
        End Get
    End Property

    ''' <summary>
    ''' Gets the source angle converted to radians and normalized to [0, 2 PI),
    ''' before rotation direction and zero-angle position are applied.
    ''' </summary>
    Public ReadOnly Property NormalizedAngleRadians As Double
        Get
            Return _normalizedAngleRadians
        End Get
    End Property

    ''' <summary>
    ''' Gets the final mathematical drawing angle in radians, measured
    ''' counterclockwise from East and normalized to [0, 2 PI).
    ''' </summary>
    Public ReadOnly Property PlotAngleRadians As Double
        Get
            Return _plotAngleRadians
        End Get
    End Property

    ''' <summary>
    ''' Gets the nonnegative distance from the centre used to draw this observation.
    ''' </summary>
    ''' <remarks>
    ''' For an included observation the value equals
    ''' <c>OriginalRadius - RadialMinimum</c>. Consequently, negative radius values
    ''' remain ordered and are displayed against a shifted radial origin instead of
    ''' being reflected through the plot centre. The value is <see cref="Double.NaN"/>
    ''' when the observation lies outside manual radial limits.
    ''' </remarks>
    Public ReadOnly Property PlotRadius As Double
        Get
            Return _plotRadius
        End Get
    End Property

    ''' <summary>
    ''' Gets the computed Cartesian X coordinate.
    ''' </summary>
    Public ReadOnly Property X As Double
        Get
            Return _x
        End Get
    End Property

    ''' <summary>
    ''' Gets the computed Cartesian Y coordinate.
    ''' </summary>
    Public ReadOnly Property Y As Double
        Get
            Return _y
        End Get
    End Property

    ''' <summary>
    ''' Gets whether either member of the source radius-angle pair was missing.
    ''' </summary>
    Public ReadOnly Property IsMissing As Boolean
        Get
            Return _isMissing
        End Get
    End Property

    ''' <summary>
    ''' Gets whether a complete source observation is below the resolved radial
    ''' minimum or above the resolved radial maximum.
    ''' </summary>
    Public ReadOnly Property IsOutsideRadialLimits As Boolean
        Get
            Return _isOutsideRadialLimits
        End Get
    End Property

    ''' <summary>
    ''' Gets whether this observation has finite coordinates that may be rendered.
    ''' </summary>
    Public ReadOnly Property IsPlottable As Boolean
        Get
            Return Not _isMissing AndAlso Not _isOutsideRadialLimits
        End Get
    End Property
End Class

''' <summary>
''' Stores an immutable sequence of paired Cartesian coordinates.
''' </summary>
''' <remarks>
''' This type is used for marker coordinates, connected data sections, grid
''' circles, and spokes. Array properties return copies so a caller cannot alter
''' a previously computed <see cref="PolarPlotResult"/>.
''' </remarks>
Public NotInheritable Class PolarPlotSeries
    Private ReadOnly _xValues As Double()
    Private ReadOnly _yValues As Double()

    ''' <summary>
    ''' Initializes a paired Cartesian coordinate sequence.
    ''' </summary>
    ''' <param name="xValues">X coordinates.</param>
    ''' <param name="yValues">Y coordinates.</param>
    ''' <exception cref="ArgumentNullException">Either coordinate array is <see langword="Nothing"/>.</exception>
    ''' <exception cref="ArgumentException">The coordinate arrays have different lengths.</exception>
    Friend Sub New(xValues As Double(), yValues As Double())
        If xValues Is Nothing Then Throw New ArgumentNullException(NameOf(xValues))
        If yValues Is Nothing Then Throw New ArgumentNullException(NameOf(yValues))
        If xValues.Length <> yValues.Length Then
            Throw New ArgumentException("The X and Y coordinate arrays must have the same length.")
        End If

        _xValues = DirectCast(xValues.Clone(), Double())
        _yValues = DirectCast(yValues.Clone(), Double())
    End Sub

    ''' <summary>
    ''' Gets a copy of the X-coordinate array.
    ''' </summary>
    Public ReadOnly Property XValues As Double()
        Get
            Return DirectCast(_xValues.Clone(), Double())
        End Get
    End Property

    ''' <summary>
    ''' Gets a copy of the Y-coordinate array.
    ''' </summary>
    Public ReadOnly Property YValues As Double()
        Get
            Return DirectCast(_yValues.Clone(), Double())
        End Get
    End Property

    ''' <summary>
    ''' Gets the number of paired coordinates in the sequence.
    ''' </summary>
    Public ReadOnly Property Count As Integer
        Get
            Return _xValues.Length
        End Get
    End Property
End Class

''' <summary>
''' Contains the immutable marker and connected-line geometry for one grouping level.
''' </summary>
''' <remarks>
''' Group levels are returned in first-plotted-observation order. Rows belonging to another
''' group do not interrupt a connected line. A missing or radially excluded row that
''' belongs to this group does interrupt its line, and a completely blank source row
''' interrupts every group.
''' </remarks>
Public NotInheritable Class PolarPlotGroupSeries
    Private ReadOnly _groupValue As Object
    Private ReadOnly _name As String
    Private ReadOnly _markerSeries As PolarPlotSeries
    Private ReadOnly _dataSegments As PolarPlotSeries()
    Private ReadOnly _sourceIndices As Integer()

    ''' <summary>
    ''' Initializes one immutable grouping-level result.
    ''' </summary>
    ''' <param name="groupValue">First source value that identified the group.</param>
    ''' <param name="name">Culture-aware display name used by chart legends.</param>
    ''' <param name="markerSeries">All plottable marker coordinates for the group.</param>
    ''' <param name="dataSegments">Contiguous connected-line sections for the group.</param>
    ''' <param name="sourceIndices">Zero-based source indices of the plotted observations.</param>
    Friend Sub New(groupValue As Object,
                   name As String,
                   markerSeries As PolarPlotSeries,
                   dataSegments As PolarPlotSeries(),
                   sourceIndices As Integer())
        If markerSeries Is Nothing Then Throw New ArgumentNullException(NameOf(markerSeries))
        If dataSegments Is Nothing Then Throw New ArgumentNullException(NameOf(dataSegments))
        If sourceIndices Is Nothing Then Throw New ArgumentNullException(NameOf(sourceIndices))
        If markerSeries.Count <> sourceIndices.Length Then
            Throw New ArgumentException("The source-index count must match the marker-coordinate count.", NameOf(sourceIndices))
        End If

        _groupValue = groupValue
        _name = If(name, String.Empty)
        _markerSeries = markerSeries
        _dataSegments = DirectCast(dataSegments.Clone(), PolarPlotSeries())
        _sourceIndices = DirectCast(sourceIndices.Clone(), Integer())
    End Sub

    ''' <summary>
    ''' Gets the first text or numeric source value that identified this group.
    ''' </summary>
    Public ReadOnly Property GroupValue As Object
        Get
            Return _groupValue
        End Get
    End Property

    ''' <summary>
    ''' Gets the display name used for the group's Excel series and legend entry.
    ''' </summary>
    Public ReadOnly Property Name As String
        Get
            Return _name
        End Get
    End Property

    ''' <summary>
    ''' Gets all marker coordinates belonging to this group in source order.
    ''' </summary>
    Public ReadOnly Property MarkerSeries As PolarPlotSeries
        Get
            Return _markerSeries
        End Get
    End Property

    ''' <summary>
    ''' Gets the connected-line sections belonging to this group.
    ''' </summary>
    Public ReadOnly Property DataSegments As PolarPlotSeries()
        Get
            Return DirectCast(_dataSegments.Clone(), PolarPlotSeries())
        End Get
    End Property

    ''' <summary>
    ''' Gets the zero-based source index corresponding to each marker coordinate.
    ''' </summary>
    Public ReadOnly Property SourceIndices As Integer()
        Get
            Return DirectCast(_sourceIndices.Clone(), Integer())
        End Get
    End Property

    ''' <summary>
    ''' Gets the number of plotted observations in this group.
    ''' </summary>
    Public ReadOnly Property Count As Integer
        Get
            Return _markerSeries.Count
        End Get
    End Property
End Class

''' <summary>
''' Describes one circular radial gridline in computed plot coordinates.
''' </summary>
Public NotInheritable Class PolarGridCircle
    Private ReadOnly _radialValue As Double
    Private ReadOnly _plotRadius As Double
    Private ReadOnly _coordinates As PolarPlotSeries

    ''' <summary>
    ''' Initializes one radial grid circle.
    ''' </summary>
    ''' <param name="radialValue">Radius-axis value represented by the circle.</param>
    ''' <param name="plotRadius">Distance of the circle from the plot centre.</param>
    ''' <param name="coordinates">Closed Cartesian coordinate sequence describing the circle.</param>
    Friend Sub New(radialValue As Double, plotRadius As Double, coordinates As PolarPlotSeries)
        If coordinates Is Nothing Then Throw New ArgumentNullException(NameOf(coordinates))
        _radialValue = radialValue
        _plotRadius = plotRadius
        _coordinates = coordinates
    End Sub

    ''' <summary>
    ''' Gets the radius-axis value represented by this circle.
    ''' </summary>
    Public ReadOnly Property RadialValue As Double
        Get
            Return _radialValue
        End Get
    End Property

    ''' <summary>
    ''' Gets the nonnegative distance of this circle from the plot centre.
    ''' </summary>
    Public ReadOnly Property PlotRadius As Double
        Get
            Return _plotRadius
        End Get
    End Property

    ''' <summary>
    ''' Gets the immutable Cartesian coordinate sequence for this circle.
    ''' </summary>
    Public ReadOnly Property Coordinates As PolarPlotSeries
        Get
            Return _coordinates
        End Get
    End Property
End Class

''' <summary>
''' Describes one angular spoke extending from the centre to the outer radial limit.
''' </summary>
Public NotInheritable Class PolarSpoke
    Private ReadOnly _inputAngleRadians As Double
    Private ReadOnly _plotAngleRadians As Double
    Private ReadOnly _coordinates As PolarPlotSeries

    ''' <summary>
    ''' Initializes one angular spoke.
    ''' </summary>
    ''' <param name="inputAngleRadians">Angle relative to the configured polar zero and direction.</param>
    ''' <param name="plotAngleRadians">Final Cartesian drawing angle in radians.</param>
    ''' <param name="coordinates">Two-point Cartesian line sequence.</param>
    Friend Sub New(inputAngleRadians As Double,
                   plotAngleRadians As Double,
                   coordinates As PolarPlotSeries)
        If coordinates Is Nothing Then Throw New ArgumentNullException(NameOf(coordinates))
        _inputAngleRadians = inputAngleRadians
        _plotAngleRadians = plotAngleRadians
        _coordinates = coordinates
    End Sub

    ''' <summary>
    ''' Gets the spoke angle in radians relative to the configured polar zero and direction.
    ''' </summary>
    Public ReadOnly Property InputAngleRadians As Double
        Get
            Return _inputAngleRadians
        End Get
    End Property

    ''' <summary>
    ''' Gets the final mathematical drawing angle in radians, measured counterclockwise from East.
    ''' </summary>
    Public ReadOnly Property PlotAngleRadians As Double
        Get
            Return _plotAngleRadians
        End Get
    End Property

    ''' <summary>
    ''' Gets the immutable two-point Cartesian coordinate sequence for this spoke.
    ''' </summary>
    Public ReadOnly Property Coordinates As PolarPlotSeries
        Get
            Return _coordinates
        End Get
    End Property
End Class

''' <summary>
''' Represents a text label and its Cartesian anchor position.
''' </summary>
Public NotInheritable Class PolarPlotLabel
    Private ReadOnly _text As String
    Private ReadOnly _x As Double
    Private ReadOnly _y As Double

    ''' <summary>
    ''' Initializes a plot label.
    ''' </summary>
    ''' <param name="text">Text displayed by the renderer.</param>
    ''' <param name="x">Cartesian X coordinate of the label anchor.</param>
    ''' <param name="y">Cartesian Y coordinate of the label anchor.</param>
    Friend Sub New(text As String, x As Double, y As Double)
        _text = If(text, String.Empty)
        _x = x
        _y = y
    End Sub

    ''' <summary>
    ''' Gets the text displayed for this label.
    ''' </summary>
    Public ReadOnly Property Text As String
        Get
            Return _text
        End Get
    End Property

    ''' <summary>
    ''' Gets the Cartesian X coordinate of the label anchor.
    ''' </summary>
    Public ReadOnly Property X As Double
        Get
            Return _x
        End Get
    End Property

    ''' <summary>
    ''' Gets the Cartesian Y coordinate of the label anchor.
    ''' </summary>
    Public ReadOnly Property Y As Double
        Get
            Return _y
        End Get
    End Property
End Class

''' <summary>
''' Contains all host-independent geometry and resolved scaling needed to render a polar plot.
''' </summary>
''' <remarks>
''' The result contains no worksheet or Excel chart references. It can therefore be
''' tested without Excel and can later be consumed by another renderer, such as an
''' Office.js canvas or SVG renderer. All returned arrays are defensive copies.
''' </remarks>
Public NotInheritable Class PolarPlotResult
    Private ReadOnly _points As PolarPlotPoint()
    Private ReadOnly _markerSeries As PolarPlotSeries
    Private ReadOnly _dataSegments As PolarPlotSeries()
    Private ReadOnly _groupSeries As PolarPlotGroupSeries()
    Private ReadOnly _gridCircles As PolarGridCircle()
    Private ReadOnly _spokes As PolarSpoke()
    Private ReadOnly _angularLabels As PolarPlotLabel()
    Private ReadOnly _radialLabels As PolarPlotLabel()
    Private ReadOnly _radialMinimum As Double
    Private ReadOnly _radialMaximum As Double
    Private ReadOnly _radialMajorInterval As Double
    Private ReadOnly _angularMajorInterval As Double
    Private ReadOnly _angularMajorIntervalRadians As Double
    Private ReadOnly _cartesianExtent As Double
    Private ReadOnly _angleUnit As PolarAngleUnit
    Private ReadOnly _rotation As PolarRotation
    Private ReadOnly _zeroAngle As PolarZeroAngle
    Private ReadOnly _connectPoints As Boolean
    Private ReadOnly _hasGrouping As Boolean
    Private ReadOnly _missingPointCount As Integer
    Private ReadOnly _outsideRadialLimitCount As Integer
    Private ReadOnly _missingGroupCount As Integer

    ''' <summary>
    ''' Initializes an immutable computed polar-plot result.
    ''' </summary>
    ''' <param name="points">One transformed point for each source row.</param>
    ''' <param name="markerSeries">All rendered marker coordinates in source order.</param>
    ''' <param name="dataSegments">All connected-line sections, flattened across grouping levels.</param>
    ''' <param name="groupSeries">One marker/line result for each rendered grouping level.</param>
    ''' <param name="gridCircles">Computed circular gridline geometry.</param>
    ''' <param name="spokes">Computed angular spoke geometry.</param>
    ''' <param name="angularLabels">Angular label text and positions.</param>
    ''' <param name="radialLabels">Radial label text and positions.</param>
    ''' <param name="radialMinimum">Resolved inner radial-axis limit.</param>
    ''' <param name="radialMaximum">Resolved outer radial-axis limit.</param>
    ''' <param name="radialMajorInterval">Resolved major radial tick interval.</param>
    ''' <param name="angularMajorInterval">Resolved angular tick interval in the configured input unit.</param>
    ''' <param name="angularMajorIntervalRadians">Resolved angular tick interval in radians.</param>
    ''' <param name="cartesianExtent">Symmetric positive X/Y axis extent including label space.</param>
    ''' <param name="hasGrouping">Whether a grouping array was supplied to the model.</param>
    ''' <param name="missingGroupCount">Number of complete in-range observations omitted because their group was missing.</param>
    ''' <param name="options">Validated options snapshot used for the computation.</param>
    Friend Sub New(points As PolarPlotPoint(),
                   markerSeries As PolarPlotSeries,
                   dataSegments As PolarPlotSeries(),
                   groupSeries As PolarPlotGroupSeries(),
                   gridCircles As PolarGridCircle(),
                   spokes As PolarSpoke(),
                   angularLabels As PolarPlotLabel(),
                   radialLabels As PolarPlotLabel(),
                   radialMinimum As Double,
                   radialMaximum As Double,
                   radialMajorInterval As Double,
                   angularMajorInterval As Double,
                   angularMajorIntervalRadians As Double,
                   cartesianExtent As Double,
                   hasGrouping As Boolean,
                   missingGroupCount As Integer,
                   options As PolarPlotOptions)
        If points Is Nothing Then Throw New ArgumentNullException(NameOf(points))
        If markerSeries Is Nothing Then Throw New ArgumentNullException(NameOf(markerSeries))
        If dataSegments Is Nothing Then Throw New ArgumentNullException(NameOf(dataSegments))
        If groupSeries Is Nothing Then Throw New ArgumentNullException(NameOf(groupSeries))
        If gridCircles Is Nothing Then Throw New ArgumentNullException(NameOf(gridCircles))
        If spokes Is Nothing Then Throw New ArgumentNullException(NameOf(spokes))
        If angularLabels Is Nothing Then Throw New ArgumentNullException(NameOf(angularLabels))
        If radialLabels Is Nothing Then Throw New ArgumentNullException(NameOf(radialLabels))
        If options Is Nothing Then Throw New ArgumentNullException(NameOf(options))

        _points = DirectCast(points.Clone(), PolarPlotPoint())
        _markerSeries = markerSeries
        _dataSegments = DirectCast(dataSegments.Clone(), PolarPlotSeries())
        _groupSeries = DirectCast(groupSeries.Clone(), PolarPlotGroupSeries())
        _gridCircles = DirectCast(gridCircles.Clone(), PolarGridCircle())
        _spokes = DirectCast(spokes.Clone(), PolarSpoke())
        _angularLabels = DirectCast(angularLabels.Clone(), PolarPlotLabel())
        _radialLabels = DirectCast(radialLabels.Clone(), PolarPlotLabel())
        _radialMinimum = radialMinimum
        _radialMaximum = radialMaximum
        _radialMajorInterval = radialMajorInterval
        _angularMajorInterval = angularMajorInterval
        _angularMajorIntervalRadians = angularMajorIntervalRadians
        _cartesianExtent = cartesianExtent
        _angleUnit = options.AngleUnit
        _rotation = options.Rotation
        _zeroAngle = options.ZeroAngle
        _connectPoints = options.ConnectPoints
        _hasGrouping = hasGrouping
        _missingGroupCount = Math.Max(0, missingGroupCount)

        For Each point As PolarPlotPoint In _points
            If point.IsMissing Then _missingPointCount += 1
            If point.IsOutsideRadialLimits Then _outsideRadialLimitCount += 1
        Next
    End Sub

    ''' <summary>
    ''' Gets a transformed observation for every source row, including missing rows.
    ''' </summary>
    Public ReadOnly Property Points As PolarPlotPoint()
        Get
            Return DirectCast(_points.Clone(), PolarPlotPoint())
        End Get
    End Property

    ''' <summary>
    ''' Gets all rendered marker coordinates in the original worksheet order.
    ''' </summary>
    Public ReadOnly Property MarkerSeries As PolarPlotSeries
        Get
            Return _markerSeries
        End Get
    End Property

    ''' <summary>
    ''' Gets all connected data sections, flattened in group order.
    ''' </summary>
    ''' <remarks>
    ''' Sections are never sorted by angle and the last section is not joined to the
    ''' first. Use <see cref="GroupSeries"/> when group identity must be retained.
    ''' </remarks>
    Public ReadOnly Property DataSegments As PolarPlotSeries()
        Get
            Return DirectCast(_dataSegments.Clone(), PolarPlotSeries())
        End Get
    End Property

    ''' <summary>
    ''' Gets one immutable data-series result for each rendered grouping level.
    ''' </summary>
    ''' <remarks>
    ''' When no grouping variable was supplied, the array contains one unnamed
    ''' series so renderers can use the same code path for grouped and ungrouped data.
    ''' </remarks>
    Public ReadOnly Property GroupSeries As PolarPlotGroupSeries()
        Get
            Return DirectCast(_groupSeries.Clone(), PolarPlotGroupSeries())
        End Get
    End Property

    ''' <summary>
    ''' Gets the circular radial gridlines, from the first major tick through the outer limit.
    ''' </summary>
    Public ReadOnly Property GridCircles As PolarGridCircle()
        Get
            Return DirectCast(_gridCircles.Clone(), PolarGridCircle())
        End Get
    End Property

    ''' <summary>
    ''' Gets the angular spokes drawn at the resolved angular tick interval.
    ''' </summary>
    Public ReadOnly Property Spokes As PolarSpoke()
        Get
            Return DirectCast(_spokes.Clone(), PolarSpoke())
        End Get
    End Property

    ''' <summary>
    ''' Gets the angular tick labels and their Cartesian positions.
    ''' </summary>
    Public ReadOnly Property AngularLabels As PolarPlotLabel()
        Get
            Return DirectCast(_angularLabels.Clone(), PolarPlotLabel())
        End Get
    End Property

    ''' <summary>
    ''' Gets the radial tick labels and their Cartesian positions.
    ''' </summary>
    Public ReadOnly Property RadialLabels As PolarPlotLabel()
        Get
            Return DirectCast(_radialLabels.Clone(), PolarPlotLabel())
        End Get
    End Property

    ''' <summary>
    ''' Gets the resolved radial value mapped to the centre of the plot.
    ''' </summary>
    Public ReadOnly Property RadialMinimum As Double
        Get
            Return _radialMinimum
        End Get
    End Property

    ''' <summary>
    ''' Gets the resolved radial value mapped to the outer grid circle.
    ''' </summary>
    Public ReadOnly Property RadialMaximum As Double
        Get
            Return _radialMaximum
        End Get
    End Property

    ''' <summary>
    ''' Gets the major interval used for radial circles and labels.
    ''' </summary>
    Public ReadOnly Property RadialMajorInterval As Double
        Get
            Return _radialMajorInterval
        End Get
    End Property

    ''' <summary>
    ''' Gets the resolved interval between radial grid circles and labels.
    ''' </summary>
    ''' <remarks>
    ''' This is a terminology alias for <see cref="RadialMajorInterval"/>.
    ''' </remarks>
    Public ReadOnly Property RadialTickInterval As Double
        Get
            Return _radialMajorInterval
        End Get
    End Property

    ''' <summary>
    ''' Gets the resolved angular tick interval expressed in <see cref="AngleUnit"/>.
    ''' </summary>
    Public ReadOnly Property AngularMajorInterval As Double
        Get
            Return _angularMajorInterval
        End Get
    End Property

    ''' <summary>
    ''' Gets the resolved angular tick interval expressed in <see cref="AngleUnit"/>.
    ''' </summary>
    ''' <remarks>
    ''' This is a terminology alias for <see cref="AngularMajorInterval"/>.
    ''' </remarks>
    Public ReadOnly Property AngularTickInterval As Double
        Get
            Return _angularMajorInterval
        End Get
    End Property

    ''' <summary>
    ''' Gets the resolved angular tick interval expressed in radians.
    ''' </summary>
    Public ReadOnly Property AngularMajorIntervalRadians As Double
        Get
            Return _angularMajorIntervalRadians
        End Get
    End Property

    ''' <summary>
    ''' Gets the distance from the plot centre to the outer radial grid circle.
    ''' </summary>
    Public ReadOnly Property RadialSpan As Double
        Get
            Return _radialMaximum - _radialMinimum
        End Get
    End Property

    ''' <summary>
    ''' Gets the positive symmetric limit recommended for both Cartesian chart axes.
    ''' </summary>
    ''' <remarks>
    ''' The extent is larger than <see cref="RadialSpan"/> so angular labels are not clipped.
    ''' Renderers should use <c>-CartesianExtent</c> and <c>+CartesianExtent</c>
    ''' for both axes to preserve circular geometry.
    ''' </remarks>
    Public ReadOnly Property CartesianExtent As Double
        Get
            Return _cartesianExtent
        End Get
    End Property

    ''' <summary>
    ''' Gets the number of observations included in rendered data series.
    ''' </summary>
    Public ReadOnly Property ValidPointCount As Integer
        Get
            Return _markerSeries.Count
        End Get
    End Property

    ''' <summary>
    ''' Gets the number of source rows containing a missing radius or angle.
    ''' </summary>
    Public ReadOnly Property MissingPointCount As Integer
        Get
            Return _missingPointCount
        End Get
    End Property

    ''' <summary>
    ''' Gets the number of complete observations omitted because they fall outside
    ''' the resolved radial limits.
    ''' </summary>
    Public ReadOnly Property OutsideRadialLimitCount As Integer
        Get
            Return _outsideRadialLimitCount
        End Get
    End Property

    ''' <summary>
    ''' Gets the number of complete in-range observations omitted because the
    ''' supplied grouping variable was missing.
    ''' </summary>
    Public ReadOnly Property MissingGroupCount As Integer
        Get
            Return _missingGroupCount
        End Get
    End Property

    ''' <summary>
    ''' Gets whether the model was constructed with a grouping array.
    ''' </summary>
    Public ReadOnly Property HasGrouping As Boolean
        Get
            Return _hasGrouping
        End Get
    End Property

    ''' <summary>
    ''' Gets the number of rendered grouping levels.
    ''' </summary>
    Public ReadOnly Property GroupCount As Integer
        Get
            Return _groupSeries.Length
        End Get
    End Property

    ''' <summary>
    ''' Gets the angle unit used for this computation.
    ''' </summary>
    Public ReadOnly Property AngleUnit As PolarAngleUnit
        Get
            Return _angleUnit
        End Get
    End Property

    ''' <summary>
    ''' Gets the angular rotation direction used for this computation.
    ''' </summary>
    Public ReadOnly Property Rotation As PolarRotation
        Get
            Return _rotation
        End Get
    End Property

    ''' <summary>
    ''' Gets the zero-angle compass position used for this computation.
    ''' </summary>
    Public ReadOnly Property ZeroAngle As PolarZeroAngle
        Get
            Return _zeroAngle
        End Get
    End Property

    ''' <summary>
    ''' Gets whether the renderer should connect consecutive observations.
    ''' </summary>
    Public ReadOnly Property ConnectPoints As Boolean
        Get
            Return _connectPoints
        End Get
    End Property
End Class

''' <summary>
''' Converts paired radius and angle arrays into host-independent polar-plot geometry.
''' </summary>
''' <remarks>
''' <para>
''' Observations remain in their original order. Angles may be negative or may
''' exceed one complete turn; they are normalized automatically. Duplicate angles
''' are retained.
''' </para>
''' <para>
''' A <see cref="Double.NaN"/> in either input marks a missing observation and
''' creates a line gap. Infinite values are rejected. Fully automatic radial
''' scaling includes zero; either limit and either major tick interval may instead
''' be supplied explicitly. The resolved radial minimum is mapped to the plot
''' centre and radius-axis values increase outwards.
''' </para>
''' <para>
''' An optional one-dimensional text or numeric grouping array creates independent
''' marker and line geometry for each first-occurring group level. Rows belonging
''' to other groups do not interrupt a group's line. Missing group IDs are omitted,
''' while a completely blank row interrupts every grouped line.
''' </para>
''' </remarks>
Public Class PolarPlot
    Private Const FullTurnRadians As Double = 2.0R * Math.PI
    Private Const DefaultAngularIntervalRadians As Double = Math.PI / 4.0R
    Private Const CircleSegmentCount As Integer = 72
    Private Const TargetRadialIntervals As Integer = 5
    Private Const MaximumGeneratedTicks As Integer = 1000
    Private Const AngularLabelRadiusFactor As Double = 1.075R
    Private Const CartesianExtentFactor As Double = 1.16R
    Private Const ScaleToleranceFactor As Double = 0.0000000001R

    Private ReadOnly _radius As Double()
    Private ReadOnly _angle As Double()
    Private ReadOnly _groupValues As Object()
    Private ReadOnly _hasGrouping As Boolean
    Private ReadOnly _options As PolarPlotOptions

    ''' <summary>
    ''' Initializes a polar-plot computation for paired radius and angle data.
    ''' </summary>
    ''' <param name="radius">Radius observations. A <see cref="Double.NaN"/> value denotes a missing observation.</param>
    ''' <param name="angle">Angle observations paired row-for-row with <paramref name="radius"/>.</param>
    ''' <param name="options">Optional plot settings. When omitted, conventional mathematical defaults are used.</param>
    ''' <param name="groupValues">
    ''' Optional one-dimensional text or numeric grouping array aligned row-for-row
    ''' with radius and angle. Blank text, <see langword="Nothing"/>,
    ''' <see cref="DBNull.Value"/>, and numeric <see cref="Double.NaN"/> values are
    ''' treated as missing group IDs.
    ''' </param>
    ''' <exception cref="ArgumentNullException">Either input array is <see langword="Nothing"/>.</exception>
    ''' <exception cref="ArgumentException">An array is multidimensional, lengths differ, or no rows were supplied.</exception>
    Public Sub New(radius As Double(),
                   angle As Double(),
                   Optional options As PolarPlotOptions = Nothing,
                   Optional groupValues As Array = Nothing)
        Me.New(radius,
               angle,
               If(groupValues Is Nothing,
                  CType(Nothing, Object()),
                  CopyGroupValues(groupValues)),
               options,
               groupValues IsNot Nothing)
    End Sub

    ''' <summary>
    ''' Creates a grouped polar-plot model using the grouping-first argument order.
    ''' </summary>
    ''' <param name="radius">Radius observations. A <see cref="Double.NaN"/> value denotes a missing observation.</param>
    ''' <param name="angle">Angle observations paired row-for-row with <paramref name="radius"/>.</param>
    ''' <param name="groupValues">
    ''' One-dimensional text or numeric grouping array aligned row-for-row with the
    ''' radius and angle arrays. Blank text, <see langword="Nothing"/>,
    ''' <see cref="DBNull.Value"/>, and numeric <see cref="Double.NaN"/> values are
    ''' treated as missing group IDs.
    ''' </param>
    ''' <param name="options">Optional plot settings. When omitted, conventional mathematical defaults are used.</param>
    ''' <returns>A defensively copied grouped polar-plot model.</returns>
    ''' <exception cref="ArgumentNullException"><paramref name="groupValues"/> or either numerical input is <see langword="Nothing"/>.</exception>
    ''' <exception cref="ArgumentException">An array is multidimensional, lengths differ, or no rows were supplied.</exception>
    Public Shared Function CreateGrouped(radius As Double(),
                                         angle As Double(),
                                         groupValues As Array,
                                         Optional options As PolarPlotOptions = Nothing) As PolarPlot
        If groupValues Is Nothing Then Throw New ArgumentNullException(NameOf(groupValues))
        Return New PolarPlot(radius, angle, options, groupValues)
    End Function

    ''' <summary>
    ''' Performs common defensive copying and length validation for grouped and ungrouped constructors.
    ''' </summary>
    Private Sub New(radius As Double(),
                    angle As Double(),
                    groupValues As Object(),
                    options As PolarPlotOptions,
                    hasGrouping As Boolean)
        If radius Is Nothing Then Throw New ArgumentNullException(NameOf(radius))
        If angle Is Nothing Then Throw New ArgumentNullException(NameOf(angle))
        If radius.Length <> angle.Length Then
            Throw New ArgumentException("Radius and angle arrays must contain the same number of observations.")
        End If
        If radius.Length = 0 Then
            Throw New ArgumentException("At least one radius-angle observation is required.")
        End If
        If hasGrouping AndAlso groupValues Is Nothing Then
            Throw New ArgumentNullException(NameOf(groupValues))
        End If
        If hasGrouping AndAlso groupValues.Length <> radius.Length Then
            Throw New ArgumentException("The grouping array must contain the same number of observations as radius and angle.",
                                        NameOf(groupValues))
        End If

        _radius = DirectCast(radius.Clone(), Double())
        _angle = DirectCast(angle.Clone(), Double())
        _groupValues = If(groupValues Is Nothing, Nothing, DirectCast(groupValues.Clone(), Object()))
        _hasGrouping = hasGrouping
        _options = If(options Is Nothing, New PolarPlotOptions(), options.Copy())
    End Sub

    ''' <summary>
    ''' Converts any one-dimensional CLR array into a boxed defensive copy.
    ''' </summary>
    ''' <param name="values">Source grouping array.</param>
    ''' <returns>A boxed one-dimensional copy.</returns>
    Private Shared Function CopyGroupValues(values As Array) As Object()
        If values Is Nothing Then Throw New ArgumentNullException(NameOf(values))
        If values.Rank <> 1 Then
            Throw New ArgumentException("The grouping input must be a one-dimensional array.", NameOf(values))
        End If

        Dim result(values.Length - 1) As Object
        Dim lowerBound As Integer = values.GetLowerBound(0)
        For i As Integer = 0 To values.Length - 1
            result(i) = values.GetValue(lowerBound + i)
        Next
        Return result
    End Function

    ''' <summary>
    ''' Computes transformed data points, missing-value line sections, radial scale,
    ''' grid circles, spokes, and label coordinates.
    ''' </summary>
    ''' <returns>An immutable <see cref="PolarPlotResult"/> ready for an Excel or non-Excel renderer.</returns>
    ''' <exception cref="ArgumentOutOfRangeException">
    ''' An option is not a defined enumeration value, an input contains infinity,
    ''' or the numerical magnitude is too large to form finite plot geometry.
    ''' </exception>
    ''' <exception cref="ArgumentException">No complete, finite radius-angle pair is available.</exception>
    Public Function Compute() As PolarPlotResult
        ValidateOptions(_options)

        Dim dataMinimum As Double = Double.PositiveInfinity
        Dim dataMaximum As Double = Double.NegativeInfinity
        Dim validCount As Integer = 0
        Dim groupKeys() As String = Nothing
        Dim groupNames() As String = Nothing
        Dim normalizedGroupValues() As Object = Nothing

        If _hasGrouping Then
            ReDim groupKeys(_radius.Length - 1)
            ReDim groupNames(_radius.Length - 1)
            ReDim normalizedGroupValues(_radius.Length - 1)
        End If

        For i As Integer = 0 To _radius.Length - 1
            If Double.IsInfinity(_radius(i)) Then
                Throw New ArgumentOutOfRangeException(NameOf(_radius),
                                                      $"Radius observation {i + 1} is infinite. Infinite values cannot be plotted.")
            End If
            If Double.IsInfinity(_angle(i)) Then
                Throw New ArgumentOutOfRangeException(NameOf(_angle),
                                                      $"Angle observation {i + 1} is infinite. Infinite values cannot be plotted.")
            End If

            Dim hasUsableGroup As Boolean = True
            If _hasGrouping Then
                hasUsableGroup = TryNormalizeGroupValue(_groupValues(i),
                                                        groupKeys(i),
                                                        groupNames(i),
                                                        normalizedGroupValues(i),
                                                        i)
            End If

            If Not Double.IsNaN(_radius(i)) AndAlso
               Not Double.IsNaN(_angle(i)) AndAlso
               hasUsableGroup Then
                dataMinimum = Math.Min(dataMinimum, _radius(i))
                dataMaximum = Math.Max(dataMaximum, _radius(i))
                validCount += 1
            End If
        Next

        If validCount = 0 Then
            If _hasGrouping Then
                Throw New ArgumentException(
                    "At least one complete radius-angle observation with a nonmissing text or numeric group ID is required.")
            End If
            Throw New ArgumentException("At least one complete, finite radius-angle pair is required.")
        End If

        Dim radialMinimum As Double
        Dim radialMaximum As Double
        Dim radialMajorInterval As Double
        ResolveRadialScale(dataMinimum,
                           dataMaximum,
                           _options,
                           radialMinimum,
                           radialMaximum,
                           radialMajorInterval)

        Dim angularMajorIntervalRadians As Double = ResolveAngularTickIntervalRadians(_options)
        Dim angularMajorInterval As Double = ConvertRadiansToAngleUnit(angularMajorIntervalRadians,
                                                                       _options.AngleUnit)

        Dim radialSpan As Double = radialMaximum - radialMinimum
        Dim cartesianExtent As Double = radialSpan * CartesianExtentFactor
        If Not IsFinitePositive(radialSpan) OrElse Not IsFinitePositive(cartesianExtent) Then
            Throw New ArgumentOutOfRangeException(NameOf(_radius),
                                                  "The radius range is too large to create finite polar-plot coordinates.")
        End If

        Dim points(_radius.Length - 1) As PolarPlotPoint
        Dim markerX As New List(Of Double)(validCount)
        Dim markerY As New List(Of Double)(validCount)
        Dim missingGroupCount As Integer = 0
        Dim inRangeCount As Integer = 0

        For i As Integer = 0 To _radius.Length - 1
            Dim sourceRadius As Double = _radius(i)
            Dim sourceAngle As Double = _angle(i)
            If Double.IsNaN(sourceRadius) OrElse Double.IsNaN(sourceAngle) Then
                points(i) = New PolarPlotPoint(i,
                                               sourceRadius,
                                               sourceAngle,
                                               Double.NaN,
                                               Double.NaN,
                                               Double.NaN,
                                               Double.NaN,
                                               Double.NaN,
                                               True,
                                               False)
                Continue For
            End If

            Dim normalizedAngle As Double = ConvertAndNormalizeAngle(sourceAngle, _options.AngleUnit)
            Dim plotAngle As Double = ToPlotAngle(normalizedAngle, _options.Rotation, _options.ZeroAngle)

            Dim belowMinimum As Boolean = sourceRadius < radialMinimum AndAlso
                                          Not NearlyEqual(sourceRadius, radialMinimum, radialSpan)
            Dim aboveMaximum As Boolean = sourceRadius > radialMaximum AndAlso
                                          Not NearlyEqual(sourceRadius, radialMaximum, radialSpan)
            If belowMinimum OrElse aboveMaximum Then
                points(i) = New PolarPlotPoint(i,
                                               sourceRadius,
                                               sourceAngle,
                                               normalizedAngle,
                                               plotAngle,
                                               Double.NaN,
                                               Double.NaN,
                                               Double.NaN,
                                               False,
                                               True)
                Continue For
            End If

            Dim effectiveRadius As Double = sourceRadius
            If NearlyEqual(effectiveRadius, radialMinimum, radialSpan) Then effectiveRadius = radialMinimum
            If NearlyEqual(effectiveRadius, radialMaximum, radialSpan) Then effectiveRadius = radialMaximum
            Dim plotRadius As Double = effectiveRadius - radialMinimum
            Dim x As Double = plotRadius * Math.Cos(plotAngle)
            Dim y As Double = plotRadius * Math.Sin(plotAngle)

            If Not IsFinite(x) OrElse Not IsFinite(y) OrElse Not IsFinite(plotRadius) Then
                Throw New ArgumentOutOfRangeException(NameOf(_radius),
                                                      $"Observation {i + 1} produces non-finite plot coordinates.")
            End If

            points(i) = New PolarPlotPoint(i,
                                           sourceRadius,
                                           sourceAngle,
                                           normalizedAngle,
                                           plotAngle,
                                           plotRadius,
                                           x,
                                           y,
                                           False,
                                           False)

            If _hasGrouping AndAlso groupKeys(i) Is Nothing Then
                missingGroupCount += 1
                Continue For
            End If

            markerX.Add(x)
            markerY.Add(y)
            inRangeCount += 1
        Next

        If inRangeCount = 0 Then
            Dim suffix As String = If(_hasGrouping,
                                      " that has a usable group ID",
                                      String.Empty)
            Throw New ArgumentException(
                "The resolved radial limits exclude every complete observation" & suffix & ".")
        End If

        Dim markerSeries As New PolarPlotSeries(markerX.ToArray(), markerY.ToArray())
        Dim groupedSeries As PolarPlotGroupSeries() = BuildGroupSeries(points,
                                                                      groupKeys,
                                                                      groupNames,
                                                                      normalizedGroupValues,
                                                                      _hasGrouping)
        Dim segments As PolarPlotSeries() = FlattenDataSegments(groupedSeries)
        Dim circles As PolarGridCircle() = BuildGridCircles(radialMinimum,
                                                            radialMaximum,
                                                            radialMajorInterval)
        Dim spokes As PolarSpoke() = BuildSpokes(radialSpan,
                                                 angularMajorIntervalRadians,
                                                 _options.Rotation,
                                                 _options.ZeroAngle)
        Dim angularLabels As PolarPlotLabel() = BuildAngularLabels(radialSpan,
                                                                  angularMajorIntervalRadians,
                                                                  _options.AngleUnit,
                                                                  _options.Rotation,
                                                                  _options.ZeroAngle)
        Dim radialLabels As PolarPlotLabel() = BuildRadialLabels(radialMinimum,
                                                                 radialMaximum,
                                                                 radialMajorInterval,
                                                                 radialSpan,
                                                                 _options.Rotation,
                                                                 _options.ZeroAngle)

        Return New PolarPlotResult(points,
                                   markerSeries,
                                   segments,
                                   groupedSeries,
                                   circles,
                                   spokes,
                                   angularLabels,
                                   radialLabels,
                                   radialMinimum,
                                   radialMaximum,
                                   radialMajorInterval,
                                   angularMajorInterval,
                                   angularMajorIntervalRadians,
                                   cartesianExtent,
                                   _hasGrouping,
                                   missingGroupCount,
                                   _options)
    End Function

    ''' <summary>
    ''' Verifies that every enumeration option contains a declared value.
    ''' </summary>
    ''' <param name="options">Options to validate.</param>
    ''' <exception cref="ArgumentOutOfRangeException">An enumeration value is undefined.</exception>
    Private Shared Sub ValidateOptions(options As PolarPlotOptions)
        If options Is Nothing Then Throw New ArgumentNullException(NameOf(options))
        If Not [Enum].IsDefined(GetType(PolarAngleUnit), options.AngleUnit) Then
            Throw New ArgumentOutOfRangeException(NameOf(options.AngleUnit), options.AngleUnit, "Unknown polar angle unit.")
        End If
        If Not [Enum].IsDefined(GetType(PolarRotation), options.Rotation) Then
            Throw New ArgumentOutOfRangeException(NameOf(options.Rotation), options.Rotation, "Unknown polar rotation direction.")
        End If
        If Not [Enum].IsDefined(GetType(PolarZeroAngle), options.ZeroAngle) Then
            Throw New ArgumentOutOfRangeException(NameOf(options.ZeroAngle), options.ZeroAngle, "Unknown polar zero-angle position.")
        End If

        ValidateOptionalFinite(options.RadialMinimum, NameOf(options.RadialMinimum))
        ValidateOptionalFinite(options.RadialMaximum, NameOf(options.RadialMaximum))
        ValidateOptionalPositive(options.RadialTickInterval, NameOf(options.RadialTickInterval))
        ValidateOptionalPositive(options.AngularTickInterval, NameOf(options.AngularTickInterval))

        If options.RadialMinimum.HasValue AndAlso
           options.RadialMaximum.HasValue AndAlso
           options.RadialMaximum.Value <= options.RadialMinimum.Value Then
            Throw New ArgumentException("RadialMaximum must be greater than RadialMinimum.", NameOf(options))
        End If
    End Sub

    ''' <summary>
    ''' Validates an optional finite numerical setting.
    ''' </summary>
    Private Shared Sub ValidateOptionalFinite(value As Nullable(Of Double), optionName As String)
        If value.HasValue AndAlso Not IsFinite(value.Value) Then
            Throw New ArgumentOutOfRangeException(optionName,
                                                  value.Value,
                                                  optionName & " must be finite when supplied.")
        End If
    End Sub

    ''' <summary>
    ''' Validates an optional finite, strictly positive interval.
    ''' </summary>
    Private Shared Sub ValidateOptionalPositive(value As Nullable(Of Double), optionName As String)
        If value.HasValue AndAlso Not IsFinitePositive(value.Value) Then
            Throw New ArgumentOutOfRangeException(optionName,
                                                  value.Value,
                                                  optionName & " must be finite and greater than zero when supplied.")
        End If
    End Sub

    ''' <summary>
    ''' Resolves the angular tick interval and converts it to radians without wrapping.
    ''' </summary>
    Private Shared Function ResolveAngularTickIntervalRadians(options As PolarPlotOptions) As Double
        Dim radians As Double
        If options.AngularTickInterval.HasValue Then
            Select Case options.AngleUnit
                Case PolarAngleUnit.Radians
                    radians = options.AngularTickInterval.Value
                Case PolarAngleUnit.Degrees
                    radians = options.AngularTickInterval.Value * Math.PI / 180.0R
                Case PolarAngleUnit.Percentage
                    radians = options.AngularTickInterval.Value * FullTurnRadians / 100.0R
                Case Else
                    Throw New ArgumentOutOfRangeException(NameOf(options.AngleUnit))
            End Select
        Else
            radians = DefaultAngularIntervalRadians
        End If

        If Not IsFinitePositive(radians) OrElse radians > FullTurnRadians * (1.0R + ScaleToleranceFactor) Then
            Throw New ArgumentOutOfRangeException(NameOf(options.AngularTickInterval),
                                                  "AngularTickInterval must be greater than zero and no larger than one complete turn.")
        End If

        If radians > FullTurnRadians Then radians = FullTurnRadians
        Dim tickCount As Integer = CountAngularTicks(radians)
        If tickCount > MaximumGeneratedTicks Then
            Throw New ArgumentOutOfRangeException(NameOf(options.AngularTickInterval),
                                                  $"AngularTickInterval creates {tickCount} ticks; at most {MaximumGeneratedTicks} are supported.")
        End If
        Return radians
    End Function

    ''' <summary>
    ''' Converts a radian interval to the configured display unit.
    ''' </summary>
    Private Shared Function ConvertRadiansToAngleUnit(radians As Double,
                                                      unit As PolarAngleUnit) As Double
        Select Case unit
            Case PolarAngleUnit.Radians
                Return radians
            Case PolarAngleUnit.Degrees
                Return radians * 180.0R / Math.PI
            Case PolarAngleUnit.Percentage
                Return radians * 100.0R / FullTurnRadians
            Case Else
                Throw New ArgumentOutOfRangeException(NameOf(unit), unit, "Unknown polar angle unit.")
        End Select
    End Function

    ''' <summary>
    ''' Counts angular ticks from zero up to, but excluding, a duplicate full-turn tick.
    ''' </summary>
    Private Shared Function CountAngularTicks(intervalRadians As Double) As Integer
        Dim adjustedTurn As Double = FullTurnRadians * (1.0R - ScaleToleranceFactor)
        Dim rawCount As Double = Math.Floor(adjustedTurn / intervalRadians) + 1.0R
        If rawCount > CDbl(MaximumGeneratedTicks) Then Return MaximumGeneratedTicks + 1
        Return Math.Max(1, CInt(rawCount))
    End Function

    ''' <summary>
    ''' Converts an input angle to radians and normalizes it to [0, 2 PI).
    ''' </summary>
    ''' <param name="value">Source angle.</param>
    ''' <param name="unit">Unit of the source angle.</param>
    ''' <returns>The normalized source angle in radians.</returns>
    Private Shared Function ConvertAndNormalizeAngle(value As Double, unit As PolarAngleUnit) As Double
        Select Case unit
            Case PolarAngleUnit.Radians
                Return NormalizeRadians(value)
            Case PolarAngleUnit.Degrees
                Return NormalizeCycleValue(value, 360.0R) * FullTurnRadians / 360.0R
            Case PolarAngleUnit.Percentage
                Return NormalizeCycleValue(value, 100.0R) * FullTurnRadians / 100.0R
            Case Else
                Throw New ArgumentOutOfRangeException(NameOf(unit), unit, "Unknown polar angle unit.")
        End Select
    End Function

    ''' <summary>
    ''' Normalizes a value expressed in arbitrary cycle units to [0, cycleLength).
    ''' </summary>
    ''' <param name="value">Finite value to normalize.</param>
    ''' <param name="cycleLength">Positive length of one complete cycle.</param>
    ''' <returns>The normalized value.</returns>
    Private Shared Function NormalizeCycleValue(value As Double, cycleLength As Double) As Double
        Dim normalized As Double = value Mod cycleLength
        If normalized < 0.0R Then normalized += cycleLength
        If normalized >= cycleLength Then normalized = 0.0R
        Return normalized
    End Function

    ''' <summary>
    ''' Normalizes a radian angle to [0, 2 PI).
    ''' </summary>
    ''' <param name="radians">Finite radian value.</param>
    ''' <returns>The normalized radian value.</returns>
    Private Shared Function NormalizeRadians(radians As Double) As Double
        Return NormalizeCycleValue(radians, FullTurnRadians)
    End Function

    ''' <summary>
    ''' Applies the configured direction and zero position to a normalized input angle.
    ''' </summary>
    ''' <param name="normalizedAngle">Input angle in radians within [0, 2 PI).</param>
    ''' <param name="rotation">Direction in which the input angle increases.</param>
    ''' <param name="zeroAngle">Compass direction corresponding to zero.</param>
    ''' <returns>A normalized Cartesian drawing angle measured counterclockwise from East.</returns>
    Private Shared Function ToPlotAngle(normalizedAngle As Double,
                                        rotation As PolarRotation,
                                        zeroAngle As PolarZeroAngle) As Double
        Dim direction As Double = If(rotation = PolarRotation.Counterclockwise, 1.0R, -1.0R)
        Return NormalizeRadians(GetZeroOffset(zeroAngle) + direction * normalizedAngle)
    End Function

    ''' <summary>
    ''' Converts a zero-angle compass position to the standard Cartesian radian offset.
    ''' </summary>
    ''' <param name="zeroAngle">Configured zero-angle compass direction.</param>
    ''' <returns>Cartesian radian offset measured counterclockwise from East.</returns>
    Private Shared Function GetZeroOffset(zeroAngle As PolarZeroAngle) As Double
        Select Case zeroAngle
            Case PolarZeroAngle.East
                Return 0.0R
            Case PolarZeroAngle.North
                Return Math.PI / 2.0R
            Case PolarZeroAngle.West
                Return Math.PI
            Case PolarZeroAngle.South
                Return -Math.PI / 2.0R
            Case Else
                Throw New ArgumentOutOfRangeException(NameOf(zeroAngle), zeroAngle, "Unknown polar zero-angle position.")
        End Select
    End Function

    ''' <summary>
    ''' Resolves automatic or user-specified radial limits and a major interval.
    ''' </summary>
    ''' <param name="dataMinimum">Smallest nonmissing radius.</param>
    ''' <param name="dataMaximum">Largest nonmissing radius.</param>
    ''' <param name="options">Validated scale options.</param>
    ''' <param name="radialMinimum">Receives the resolved inner radial limit.</param>
    ''' <param name="radialMaximum">Receives the resolved outer radial limit.</param>
    ''' <param name="majorInterval">Receives the resolved major radial interval.</param>
    Private Shared Sub ResolveRadialScale(dataMinimum As Double,
                                          dataMaximum As Double,
                                          options As PolarPlotOptions,
                                          ByRef radialMinimum As Double,
                                          ByRef radialMaximum As Double,
                                          ByRef majorInterval As Double)
        Dim hasMinimum As Boolean = options.RadialMinimum.HasValue
        Dim hasMaximum As Boolean = options.RadialMaximum.HasValue
        Dim automaticMinimum As Double = Math.Min(0.0R, dataMinimum)
        Dim automaticMaximum As Double = Math.Max(0.0R, dataMaximum)

        If automaticMinimum = automaticMaximum Then automaticMaximum = automaticMinimum + 1.0R

        Dim provisionalMinimum As Double = If(hasMinimum,
                                               options.RadialMinimum.Value,
                                               automaticMinimum)
        Dim provisionalMaximum As Double = If(hasMaximum,
                                               options.RadialMaximum.Value,
                                               automaticMaximum)

        If provisionalMaximum <= provisionalMinimum Then
            If hasMinimum AndAlso Not hasMaximum Then
                provisionalMaximum = provisionalMinimum + 1.0R
            ElseIf hasMaximum AndAlso Not hasMinimum Then
                provisionalMinimum = provisionalMaximum - 1.0R
            End If
        End If

        Dim rawRange As Double = provisionalMaximum - provisionalMinimum
        If Not IsFinitePositive(rawRange) Then
            Throw New ArgumentOutOfRangeException(NameOf(dataMaximum),
                                                  "The radius range is too large to scale.")
        End If

        majorInterval = If(options.RadialTickInterval.HasValue,
                           options.RadialTickInterval.Value,
                           NiceStep(rawRange / CDbl(TargetRadialIntervals)))

        If hasMinimum AndAlso hasMaximum Then
            radialMinimum = options.RadialMinimum.Value
            radialMaximum = options.RadialMaximum.Value
        ElseIf hasMinimum Then
            radialMinimum = options.RadialMinimum.Value
            Dim targetMaximum As Double = Math.Max(dataMaximum, radialMinimum + majorInterval)
            If radialMinimum < 0.0R Then targetMaximum = Math.Max(targetMaximum, 0.0R)
            Dim intervals As Double = Math.Ceiling((targetMaximum - radialMinimum) / majorInterval)
            radialMaximum = radialMinimum + Math.Max(1.0R, intervals) * majorInterval
        ElseIf hasMaximum Then
            radialMaximum = options.RadialMaximum.Value
            Dim targetMinimum As Double = Math.Min(dataMinimum, radialMaximum - majorInterval)
            If radialMaximum > 0.0R Then targetMinimum = Math.Min(targetMinimum, 0.0R)
            Dim intervals As Double = Math.Ceiling((radialMaximum - targetMinimum) / majorInterval)
            radialMinimum = radialMaximum - Math.Max(1.0R, intervals) * majorInterval
        Else
            radialMinimum = Math.Floor(automaticMinimum / majorInterval) * majorInterval
            radialMaximum = Math.Ceiling(automaticMaximum / majorInterval) * majorInterval
            If radialMinimum > 0.0R Then radialMinimum = 0.0R
            If radialMaximum < 0.0R Then radialMaximum = 0.0R
            If radialMaximum <= radialMinimum Then radialMaximum = radialMinimum + majorInterval
        End If

        If Not IsFinite(radialMinimum) OrElse
           Not IsFinite(radialMaximum) OrElse
           Not IsFinitePositive(majorInterval) Then
            Throw New ArgumentOutOfRangeException(NameOf(dataMaximum),
                                                  "The radius magnitude is too large to form a finite scale.")
        End If

        Dim tickCount As Integer = CountRadialTicks(radialMinimum,
                                                    radialMaximum,
                                                    majorInterval)
        If tickCount > MaximumGeneratedTicks Then
            Throw New ArgumentOutOfRangeException(NameOf(options.RadialTickInterval),
                                                  $"RadialTickInterval creates {tickCount} ticks; at most {MaximumGeneratedTicks} are supported.")
        End If
    End Sub

    ''' <summary>
    ''' Counts the centre label, regular radial ticks, and a possible non-aligned outer-limit label.
    ''' </summary>
    Private Shared Function CountRadialTicks(radialMinimum As Double,
                                             radialMaximum As Double,
                                             majorInterval As Double) As Integer
        Dim span As Double = radialMaximum - radialMinimum
        Dim rawIntervals As Double = Math.Floor(span / majorInterval + ScaleToleranceFactor)
        If rawIntervals > CDbl(MaximumGeneratedTicks) Then Return MaximumGeneratedTicks + 1
        Dim regularIntervals As Integer = CInt(rawIntervals)
        Dim lastRegular As Double = radialMinimum + CDbl(regularIntervals) * majorInterval
        Dim includeOuter As Boolean = Not NearlyEqual(lastRegular, radialMaximum, span)
        Return 1 + regularIntervals + If(includeOuter, 1, 0)
    End Function

    ''' <summary>
    ''' Rounds a positive raw interval to a nearby 1, 2, 5, or 10 multiple of a power of ten.
    ''' </summary>
    ''' <param name="rawStep">Positive unrounded interval.</param>
    ''' <returns>A positive human-readable interval.</returns>
    Private Shared Function NiceStep(rawStep As Double) As Double
        If Not IsFinitePositive(rawStep) Then Return 1.0R

        Dim exponent As Double = Math.Floor(Math.Log10(rawStep))
        Dim magnitude As Double = Math.Pow(10.0R, exponent)
        Dim fraction As Double = rawStep / magnitude
        Dim niceFraction As Double

        If fraction <= 1.5R Then
            niceFraction = 1.0R
        ElseIf fraction <= 3.5R Then
            niceFraction = 2.0R
        ElseIf fraction <= 7.5R Then
            niceFraction = 5.0R
        Else
            niceFraction = 10.0R
        End If

        Return niceFraction * magnitude
    End Function

    ''' <summary>
    ''' Stores mutable construction buffers for one normalized grouping level.
    ''' </summary>
    Private NotInheritable Class GroupDescriptor
        Friend Key As String
        Friend Name As String
        Friend Value As Object
        Friend MarkerX As New List(Of Double)()
        Friend MarkerY As New List(Of Double)()
        Friend SourceIndices As New List(Of Integer)()
    End Class

    ''' <summary>
    ''' Normalizes a supported text or numeric group value into a stable dictionary key.
    ''' </summary>
    ''' <returns><see langword="False"/> for a missing group value; otherwise <see langword="True"/>.</returns>
    Private Shared Function TryNormalizeGroupValue(value As Object,
                                                   ByRef key As String,
                                                   ByRef displayName As String,
                                                   ByRef normalizedValue As Object,
                                                   sourceIndex As Integer) As Boolean
        key = Nothing
        displayName = Nothing
        normalizedValue = Nothing

        If value Is Nothing OrElse value Is DBNull.Value Then Return False

        Dim textValue As String = TryCast(value, String)
        If textValue IsNot Nothing Then
            textValue = textValue.Trim()
            If textValue.Length = 0 Then Return False
            key = "S:" & textValue
            displayName = textValue
            normalizedValue = textValue
            Return True
        End If

        If TypeOf value Is Char Then
            displayName = CChar(value).ToString()
            key = "S:" & displayName
            normalizedValue = displayName
            Return True
        End If

        Dim valueTypeCode As TypeCode = Type.GetTypeCode(value.GetType())
        Select Case valueTypeCode
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
                    Throw New ArgumentOutOfRangeException(NameOf(value),
                                                          $"Grouping observation {sourceIndex + 1} is infinite.")
                End If
                If singleValue = 0.0F Then singleValue = 0.0F
                key = "N:" & singleValue.ToString("R", CultureInfo.InvariantCulture)
                displayName = singleValue.ToString("0.########", CultureInfo.CurrentCulture)
                normalizedValue = value
                Return True

            Case TypeCode.Double
                Dim floatingValue As Double = Convert.ToDouble(value, CultureInfo.InvariantCulture)
                If Double.IsNaN(floatingValue) Then Return False
                If Double.IsInfinity(floatingValue) Then
                    Throw New ArgumentOutOfRangeException(NameOf(value),
                                                          $"Grouping observation {sourceIndex + 1} is infinite.")
                End If
                If floatingValue = 0.0R Then floatingValue = 0.0R
                key = "N:" & floatingValue.ToString("R", CultureInfo.InvariantCulture)
                displayName = FormatNumber(floatingValue)
                normalizedValue = value
                Return True

            Case Else
                Throw New ArgumentException(
                    $"Grouping observation {sourceIndex + 1} has unsupported type '{value.GetType().Name}'. " &
                    "Only text and numeric group values are supported.",
                    NameOf(value))
        End Select
    End Function

    ''' <summary>
    ''' Builds one independent marker and line result for each grouping level.
    ''' </summary>
    Private Shared Function BuildGroupSeries(points As PolarPlotPoint(),
                                             groupKeys As String(),
                                             groupNames As String(),
                                             groupValues As Object(),
                                             hasGrouping As Boolean) As PolarPlotGroupSeries()
        If Not hasGrouping Then
            Dim markerX As New List(Of Double)()
            Dim markerY As New List(Of Double)()
            Dim sourceIndices As New List(Of Integer)()
            For Each point As PolarPlotPoint In points
                If point.IsPlottable Then
                    markerX.Add(point.X)
                    markerY.Add(point.Y)
                    sourceIndices.Add(point.SourceIndex)
                End If
            Next

            Return {New PolarPlotGroupSeries(Nothing,
                                             String.Empty,
                                             New PolarPlotSeries(markerX.ToArray(), markerY.ToArray()),
                                             BuildSegmentsForGroup(points, Nothing, Nothing, False),
                                             sourceIndices.ToArray())}
        End If

        Dim descriptors As New List(Of GroupDescriptor)()
        Dim descriptorByKey As New Dictionary(Of String, GroupDescriptor)(StringComparer.Ordinal)

        For i As Integer = 0 To points.Length - 1
            If Not points(i).IsPlottable OrElse groupKeys(i) Is Nothing Then Continue For

            Dim descriptor As GroupDescriptor = Nothing
            If Not descriptorByKey.TryGetValue(groupKeys(i), descriptor) Then
                descriptor = New GroupDescriptor With {
                    .Key = groupKeys(i),
                    .Name = groupNames(i),
                    .Value = groupValues(i)
                }
                descriptorByKey.Add(descriptor.Key, descriptor)
                descriptors.Add(descriptor)
            End If

            descriptor.MarkerX.Add(points(i).X)
            descriptor.MarkerY.Add(points(i).Y)
            descriptor.SourceIndices.Add(points(i).SourceIndex)
        Next

        Dim result(descriptors.Count - 1) As PolarPlotGroupSeries
        For i As Integer = 0 To descriptors.Count - 1
            Dim descriptor As GroupDescriptor = descriptors(i)
            result(i) = New PolarPlotGroupSeries(
                descriptor.Value,
                descriptor.Name,
                New PolarPlotSeries(descriptor.MarkerX.ToArray(), descriptor.MarkerY.ToArray()),
                BuildSegmentsForGroup(points, groupKeys, descriptor.Key, True),
                descriptor.SourceIndices.ToArray())
        Next
        Return result
    End Function

    ''' <summary>
    ''' Builds connected sections for one group without allowing other groups to interrupt it.
    ''' </summary>
    Private Shared Function BuildSegmentsForGroup(points As PolarPlotPoint(),
                                                  groupKeys As String(),
                                                  targetKey As String,
                                                  hasGrouping As Boolean) As PolarPlotSeries()
        Dim result As New List(Of PolarPlotSeries)()
        Dim currentX As New List(Of Double)()
        Dim currentY As New List(Of Double)()

        For i As Integer = 0 To points.Length - 1
            If hasGrouping Then
                If groupKeys(i) Is Nothing Then
                    If points(i).IsMissing Then FlushSegment(currentX, currentY, result)
                    Continue For
                End If
                If Not String.Equals(groupKeys(i), targetKey, StringComparison.Ordinal) Then Continue For
            End If

            If Not points(i).IsPlottable Then
                FlushSegment(currentX, currentY, result)
            Else
                currentX.Add(points(i).X)
                currentY.Add(points(i).Y)
            End If
        Next
        FlushSegment(currentX, currentY, result)
        Return result.ToArray()
    End Function

    ''' <summary>
    ''' Flattens grouping-level sections for backward-compatible access through
    ''' <see cref="PolarPlotResult.DataSegments"/>.
    ''' </summary>
    Private Shared Function FlattenDataSegments(groups As PolarPlotGroupSeries()) As PolarPlotSeries()
        Dim result As New List(Of PolarPlotSeries)()
        For Each group As PolarPlotGroupSeries In groups
            result.AddRange(group.DataSegments)
        Next
        Return result.ToArray()
    End Function

    ''' <summary>
    ''' Adds the current nonempty coordinate section to a result list and clears its buffers.
    ''' </summary>
    ''' <param name="xValues">Current X-coordinate buffer.</param>
    ''' <param name="yValues">Current Y-coordinate buffer.</param>
    ''' <param name="segments">Destination segment list.</param>
    Private Shared Sub FlushSegment(xValues As List(Of Double),
                                    yValues As List(Of Double),
                                    segments As List(Of PolarPlotSeries))
        If xValues.Count = 0 Then Return
        segments.Add(New PolarPlotSeries(xValues.ToArray(), yValues.ToArray()))
        xValues.Clear()
        yValues.Clear()
    End Sub

    ''' <summary>
    ''' Creates circular gridline geometry for every resolved major radial tick above the centre.
    ''' </summary>
    ''' <param name="radialMinimum">Resolved radial minimum.</param>
    ''' <param name="radialMaximum">Resolved radial maximum.</param>
    ''' <param name="majorInterval">Resolved major radial interval.</param>
    ''' <returns>Closed 5-degree-resolution circle sequences.</returns>
    Private Shared Function BuildGridCircles(radialMinimum As Double,
                                             radialMaximum As Double,
                                             majorInterval As Double) As PolarGridCircle()
        Dim tickValues As Double() = BuildRadialTickValues(radialMinimum,
                                                           radialMaximum,
                                                           majorInterval)
        Dim result As New List(Of PolarGridCircle)(Math.Max(0, tickValues.Length - 1))

        For i As Integer = 1 To tickValues.Length - 1
            Dim radialValue As Double = tickValues(i)
            Dim plotRadius As Double = radialValue - radialMinimum
            result.Add(New PolarGridCircle(radialValue,
                                           plotRadius,
                                           CreateCircleSeries(plotRadius)))
        Next

        Return result.ToArray()
    End Function

    ''' <summary>
    ''' Creates radial values containing the inner limit, regular major ticks, and
    ''' the outer limit when it is not aligned with the major interval.
    ''' </summary>
    Private Shared Function BuildRadialTickValues(radialMinimum As Double,
                                                  radialMaximum As Double,
                                                  majorInterval As Double) As Double()
        Dim result As New List(Of Double) From {radialMinimum}
        Dim span As Double = radialMaximum - radialMinimum
        Dim regularIntervals As Integer = CInt(Math.Floor(span / majorInterval + ScaleToleranceFactor))

        For i As Integer = 1 To regularIntervals
            Dim value As Double = radialMinimum + CDbl(i) * majorInterval
            If value < radialMaximum AndAlso Not NearlyEqual(value, radialMaximum, span) Then
                result.Add(value)
            Else
                Exit For
            End If
        Next

        If Not NearlyEqual(result(result.Count - 1), radialMaximum, span) Then
            result.Add(radialMaximum)
        End If
        Return result.ToArray()
    End Function

    ''' <summary>
    ''' Creates one closed Cartesian circle using 72 equal segments, or one point every 5 degrees.
    ''' </summary>
    ''' <param name="radius">Nonnegative plotted circle radius.</param>
    ''' <returns>A 73-point series whose final point repeats its first point.</returns>
    Private Shared Function CreateCircleSeries(radius As Double) As PolarPlotSeries
        Dim xValues(CircleSegmentCount) As Double
        Dim yValues(CircleSegmentCount) As Double

        For i As Integer = 0 To CircleSegmentCount
            Dim angle As Double = FullTurnRadians * CDbl(i) / CDbl(CircleSegmentCount)
            xValues(i) = radius * Math.Cos(angle)
            yValues(i) = radius * Math.Sin(angle)
        Next

        Return New PolarPlotSeries(xValues, yValues)
    End Function

    ''' <summary>
    ''' Creates angular spokes using the resolved interval and configured orientation.
    ''' </summary>
    ''' <param name="radialSpan">Distance from the centre to the outer circle.</param>
    ''' <param name="angularIntervalRadians">Positive interval between spokes.</param>
    ''' <param name="rotation">Positive-angle direction.</param>
    ''' <param name="zeroAngle">Zero-angle compass position.</param>
    ''' <returns>Two-point spoke sequences beginning at an input angle of zero.</returns>
    Private Shared Function BuildSpokes(radialSpan As Double,
                                        angularIntervalRadians As Double,
                                        rotation As PolarRotation,
                                        zeroAngle As PolarZeroAngle) As PolarSpoke()
        Dim tickCount As Integer = CountAngularTicks(angularIntervalRadians)
        Dim result(tickCount - 1) As PolarSpoke

        For i As Integer = 0 To tickCount - 1
            Dim inputAngle As Double = CDbl(i) * angularIntervalRadians
            Dim plotAngle As Double = ToPlotAngle(inputAngle, rotation, zeroAngle)
            Dim xValues As Double() = {0.0R, radialSpan * Math.Cos(plotAngle)}
            Dim yValues As Double() = {0.0R, radialSpan * Math.Sin(plotAngle)}
            result(i) = New PolarSpoke(inputAngle,
                                       plotAngle,
                                       New PolarPlotSeries(xValues, yValues))
        Next

        Return result
    End Function

    ''' <summary>
    ''' Creates unit-aware angular labels at the ends of the resolved spokes.
    ''' </summary>
    ''' <param name="radialSpan">Distance from the centre to the outer circle.</param>
    ''' <param name="angularIntervalRadians">Positive interval between labels.</param>
    ''' <param name="angleUnit">Unit used to format the label text.</param>
    ''' <param name="rotation">Positive-angle direction.</param>
    ''' <param name="zeroAngle">Zero-angle compass position.</param>
    ''' <returns>Angular labels positioned just outside the outer circle.</returns>
    Private Shared Function BuildAngularLabels(radialSpan As Double,
                                               angularIntervalRadians As Double,
                                               angleUnit As PolarAngleUnit,
                                               rotation As PolarRotation,
                                               zeroAngle As PolarZeroAngle) As PolarPlotLabel()
        Dim tickCount As Integer = CountAngularTicks(angularIntervalRadians)
        Dim result(tickCount - 1) As PolarPlotLabel
        Dim labelRadius As Double = radialSpan * AngularLabelRadiusFactor

        For i As Integer = 0 To tickCount - 1
            Dim inputAngle As Double = CDbl(i) * angularIntervalRadians
            Dim plotAngle As Double = ToPlotAngle(inputAngle, rotation, zeroAngle)
            result(i) = New PolarPlotLabel(FormatAngularLabel(inputAngle, angleUnit),
                                           labelRadius * Math.Cos(plotAngle),
                                           labelRadius * Math.Sin(plotAngle))
        Next

        Return result
    End Function

    ''' <summary>
    ''' Formats one angular tick label in the selected unit.
    ''' </summary>
    ''' <param name="inputAngleRadians">Tick angle relative to the configured polar zero.</param>
    ''' <param name="angleUnit">Requested display unit.</param>
    ''' <returns>A degrees, radians, or percentage label.</returns>
    Private Shared Function FormatAngularLabel(inputAngleRadians As Double,
                                               angleUnit As PolarAngleUnit) As String
        Select Case angleUnit
            Case PolarAngleUnit.Degrees
                Return FormatNumber(inputAngleRadians * 180.0R / Math.PI) & ChrW(&HB0)
            Case PolarAngleUnit.Percentage
                Dim percentage As Double = inputAngleRadians * 100.0R / FullTurnRadians
                Return FormatNumber(percentage) & "%"
            Case PolarAngleUnit.Radians
                Return FormatRadianLabel(inputAngleRadians)
            Case Else
                Throw New ArgumentOutOfRangeException(NameOf(angleUnit), angleUnit, "Unknown polar angle unit.")
        End Select
    End Function

    ''' <summary>
    ''' Formats common rational multiples of PI symbolically and other radians numerically.
    ''' </summary>
    Private Shared Function FormatRadianLabel(radians As Double) As String
        If NearlyEqual(radians, 0.0R, FullTurnRadians) Then Return "0"

        Dim ratio As Double = radians / Math.PI
        For denominator As Integer = 1 To 64
            Dim numerator As Integer = CInt(Math.Round(ratio * CDbl(denominator),
                                                       MidpointRounding.AwayFromZero))
            If NearlyEqual(ratio,
                           CDbl(numerator) / CDbl(denominator),
                           Math.Max(1.0R, Math.Abs(ratio))) Then
                Dim divisor As Integer = GreatestCommonDivisor(Math.Abs(numerator), denominator)
                Dim reducedNumerator As Integer = numerator \ divisor
                Dim reducedDenominator As Integer = denominator \ divisor

                If reducedDenominator = 1 Then
                    If reducedNumerator = 1 Then Return "π"
                    Return reducedNumerator.ToString(CultureInfo.CurrentCulture) & "π"
                End If
                If reducedNumerator = 1 Then Return "π/" & reducedDenominator.ToString(CultureInfo.CurrentCulture)
                Return reducedNumerator.ToString(CultureInfo.CurrentCulture) & "π/" &
                       reducedDenominator.ToString(CultureInfo.CurrentCulture)
            End If
        Next

        Return FormatNumber(radians)
    End Function

    ''' <summary>
    ''' Calculates the greatest common divisor of two nonnegative integers.
    ''' </summary>
    Private Shared Function GreatestCommonDivisor(left As Integer, right As Integer) As Integer
        While right <> 0
            Dim remainder As Integer = left Mod right
            left = right
            right = remainder
        End While
        Return Math.Max(1, left)
    End Function

    ''' <summary>
    ''' Creates radial tick labels along the configured zero-angle spoke.
    ''' </summary>
    ''' <param name="radialMinimum">Resolved radial minimum.</param>
    ''' <param name="radialMaximum">Resolved radial maximum.</param>
    ''' <param name="majorInterval">Resolved major radial interval.</param>
    ''' <param name="radialSpan">Distance from the centre to the outer circle.</param>
    ''' <param name="rotation">Positive-angle direction.</param>
    ''' <param name="zeroAngle">Zero-angle compass position.</param>
    ''' <returns>Labels from the inner radial limit through the outer radial limit.</returns>
    Private Shared Function BuildRadialLabels(radialMinimum As Double,
                                              radialMaximum As Double,
                                              majorInterval As Double,
                                              radialSpan As Double,
                                              rotation As PolarRotation,
                                              zeroAngle As PolarZeroAngle) As PolarPlotLabel()
        Dim tickValues As Double() = BuildRadialTickValues(radialMinimum,
                                                           radialMaximum,
                                                           majorInterval)
        Dim result(tickValues.Length - 1) As PolarPlotLabel
        Dim plotAngle As Double = ToPlotAngle(0.0R, rotation, zeroAngle)
        Dim perpendicularAngle As Double = plotAngle + Math.PI / 2.0R
        Dim offset As Double = radialSpan * 0.018R

        For i As Integer = 0 To tickValues.Length - 1
            Dim radialValue As Double = tickValues(i)
            Dim plotRadius As Double = radialValue - radialMinimum
            Dim x As Double = plotRadius * Math.Cos(plotAngle) + offset * Math.Cos(perpendicularAngle)
            Dim y As Double = plotRadius * Math.Sin(plotAngle) + offset * Math.Sin(perpendicularAngle)
            result(i) = New PolarPlotLabel(FormatNumber(radialValue), x, y)
        Next

        Return result
    End Function

    ''' <summary>
    ''' Formats a radial or percentage value without unnecessary trailing zeroes.
    ''' </summary>
    ''' <param name="value">Finite number to format.</param>
    ''' <returns>A concise culture-aware numeric label.</returns>
    Private Shared Function FormatNumber(value As Double) As String
        If Math.Abs(value) < 0.00000000000001R Then value = 0.0R
        Return value.ToString("0.###############", CultureInfo.CurrentCulture)
    End Function

    ''' <summary>
    ''' Compares finite scale values using a tolerance proportional to their span.
    ''' </summary>
    Private Shared Function NearlyEqual(left As Double,
                                        right As Double,
                                        scale As Double) As Boolean
        Dim tolerance As Double = Math.Max(1.0R, Math.Abs(scale)) * ScaleToleranceFactor
        Return Math.Abs(left - right) <= tolerance
    End Function

    ''' <summary>
    ''' Tests whether a number is neither NaN nor positive or negative infinity.
    ''' </summary>
    ''' <param name="value">Number to test.</param>
    ''' <returns><see langword="True"/> when the number is finite.</returns>
    Private Shared Function IsFinite(value As Double) As Boolean
        Return Not Double.IsNaN(value) AndAlso Not Double.IsInfinity(value)
    End Function

    ''' <summary>
    ''' Tests whether a number is finite and strictly positive.
    ''' </summary>
    ''' <param name="value">Number to test.</param>
    ''' <returns><see langword="True"/> when the number is finite and greater than zero.</returns>
    Private Shared Function IsFinitePositive(value As Double) As Boolean
        Return IsFinite(value) AndAlso value > 0.0R
    End Function
End Class

''' <summary>
''' Contains Excel-rendering appearance settings for <see cref="PolarPlotExcel"/>.
''' </summary>
''' <remarks>
''' Colors are OLE RGB integers, as expected by the Excel object model. These
''' settings do not affect the numerical geometry stored in <see cref="PolarPlotResult"/>.
''' </remarks>
Public Class PolarPlotAppearance
    ''' <summary>
    ''' Gets or sets the chart title. Set an empty string to suppress the title.
    ''' </summary>
    Public Property ChartTitle As String = "Polar plot"

    ''' <summary>
    ''' Gets or sets the name of the observation series.
    ''' </summary>
    Public Property SeriesName As String = "Data"

    ''' <summary>
    ''' Gets or sets the OLE RGB color used for the data line and markers.
    ''' </summary>
    Public Property DataColor As Integer = &HB4771F

    ''' <summary>
    ''' Gets or sets whether grouped data are distinguished by color, marker, or both.
    ''' </summary>
    Public Property GroupStyleMode As PolarGroupStyleMode = PolarGroupStyleMode.ColorAndMarker

    ''' <summary>
    ''' Gets or sets the OLE RGB palette cycled across grouping levels.
    ''' </summary>
    ''' <remarks>
    ''' The default is the ten-color Tableau palette expressed in Excel's OLE RGB
    ''' byte order. At least one color is required when <see cref="GroupStyleMode"/>
    ''' uses color. When there are more groups than colors, the palette repeats.
    ''' </remarks>
    Public Property GroupColors As Integer() = {
        &HB4771F, &HE7FFF, &H2CA02C, &H2827D6, &HBD6794,
        &H4B568C, &HC277E3, &H7F7F7F, &H22BDBC, &HCFBE17
    }

    ''' <summary>
    ''' Gets or sets the OLE RGB color used for radial grid circles.
    ''' </summary>
    Public Property GridColor As Integer = &HDCDCDC

    ''' <summary>
    ''' Gets or sets the OLE RGB color used for angular spokes.
    ''' </summary>
    Public Property SpokeColor As Integer = &HBEBEBE

    ''' <summary>
    ''' Gets or sets the OLE RGB color used for angular and radial labels.
    ''' </summary>
    Public Property TextColor As Integer = &H505050

    ''' <summary>
    ''' Gets or sets the OLE RGB color used for the chart and plot backgrounds.
    ''' </summary>
    Public Property BackgroundColor As Integer = &HFFFFFF

    ''' <summary>
    ''' Gets or sets the Excel marker style used for observations.
    ''' </summary>
    Public Property MarkerStyle As XlMarkerStyle = XlMarkerStyle.xlMarkerStyleCircle

    ''' <summary>
    ''' Gets or sets the Excel marker-style palette cycled across grouping levels.
    ''' </summary>
    ''' <remarks>
    ''' At least one marker is required when <see cref="GroupStyleMode"/> uses
    ''' marker shapes. When there are more groups than marker styles, the palette repeats.
    ''' </remarks>
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

    ''' <summary>
    ''' Gets or sets the marker size in points.
    ''' </summary>
    Public Property MarkerSize As Integer = 6

    ''' <summary>
    ''' Gets or sets the data-line width in points.
    ''' </summary>
    Public Property DataLineWeight As Single = 1.5F

    ''' <summary>
    ''' Gets or sets the circular-gridline width in points.
    ''' </summary>
    Public Property GridLineWeight As Single = 0.75F

    ''' <summary>
    ''' Gets or sets the angular-spoke width in points.
    ''' </summary>
    Public Property SpokeLineWeight As Single = 0.75F

    ''' <summary>
    ''' Gets or sets the label font size in points.
    ''' </summary>
    Public Property LabelFontSize As Single = 9.0F

    ''' <summary>
    ''' Gets or sets whether angular labels are drawn.
    ''' </summary>
    Public Property ShowAngularLabels As Boolean = True

    ''' <summary>
    ''' Gets or sets whether radial labels are drawn.
    ''' </summary>
    Public Property ShowRadialLabels As Boolean = True

    ''' <summary>
    ''' Gets or sets whether a legend containing the data-series name is displayed.
    ''' </summary>
    Public Property ShowLegend As Boolean = False

    ''' <summary>
    ''' Gets or sets whether a legend is shown automatically when a grouping array was supplied.
    ''' </summary>
    ''' <remarks>
    ''' This setting is independent of <see cref="ShowLegend"/>, which controls the
    ''' single-series legend. The default makes group colors and shapes interpretable
    ''' without changing the original ungrouped chart appearance.
    ''' </remarks>
    Public Property ShowGroupLegend As Boolean = True
End Class

''' <summary>
''' Renders a computed <see cref="PolarPlotResult"/> as an embedded Excel XY-scatter chart.
''' </summary>
''' <remarks>
''' The renderer writes no helper cells. Circular rings, spokes, labels, and data
''' are assigned directly to Excel series. Ordinary Cartesian axes are hidden and
''' given identical symmetric limits so the chart behaves as a polar coordinate system.
''' </remarks>
Public NotInheritable Class PolarPlotExcel
    Private Const MaximumExcelSeries As Integer = 255

    ''' <summary>
    ''' Prevents construction of this shared utility class.
    ''' </summary>
    Private Sub New()
    End Sub

    ''' <summary>
    ''' Adds a square embedded polar chart to a worksheet.
    ''' </summary>
    ''' <param name="ws">Worksheet that receives the chart.</param>
    ''' <param name="result">Host-independent geometry returned by <see cref="PolarPlot.Compute"/>.</param>
    ''' <param name="appearance">Optional appearance settings; <see langword="Nothing"/> uses defaults.</param>
    ''' <param name="left">Horizontal chart position in points.</param>
    ''' <param name="top">Vertical chart position in points.</param>
    ''' <param name="width">Requested chart width in points.</param>
    ''' <param name="height">Requested chart height in points.</param>
    ''' <returns>The created embedded Excel <see cref="Chart"/>.</returns>
    ''' <remarks>
    ''' The smaller of <paramref name="width"/> and <paramref name="height"/> is
    ''' used for both dimensions. This preserves the equal physical X/Y scale
    ''' required for circular rings.
    ''' </remarks>
    ''' <exception cref="ArgumentNullException"><paramref name="ws"/> or <paramref name="result"/> is <see langword="Nothing"/>.</exception>
    ''' <exception cref="ArgumentOutOfRangeException">A chart dimension or an appearance size is invalid.</exception>
    ''' <exception cref="InvalidOperationException">The missing-value pattern would exceed Excel's series limit.</exception>
    Public Shared Function AddChart(ws As Worksheet,
                                    result As PolarPlotResult,
                                    appearance As PolarPlotAppearance,
                                    left As Double,
                                    top As Double,
                                    width As Double,
                                    height As Double) As Chart
        If ws Is Nothing Then Throw New ArgumentNullException(NameOf(ws))
        If result Is Nothing Then Throw New ArgumentNullException(NameOf(result))
        If Not IsFinite(left) OrElse Not IsFinite(top) Then
            Throw New ArgumentOutOfRangeException(NameOf(left), "Chart position must be finite.")
        End If
        If Not IsFinitePositive(width) OrElse Not IsFinitePositive(height) Then
            Throw New ArgumentOutOfRangeException(NameOf(width), "Chart width and height must be finite and positive.")
        End If

        Dim resolvedAppearance As PolarPlotAppearance = If(appearance, New PolarPlotAppearance())
        ValidateAppearance(resolvedAppearance, result.HasGrouping)
        ValidateSeriesCount(result, resolvedAppearance)

        Dim side As Double = Math.Min(width, height)
        Dim chartShape As Shape = Nothing

        Try
            chartShape = ws.Shapes.AddChart(XlChartType.xlXYScatter,
                                            left,
                                            top,
                                            side,
                                            side)
            'Use late binding here because LockAspectRatio is typed as Office.MsoTriState,
            'while the project intentionally references only the Excel interop assembly.
            Dim chartShapeObject As Object = chartShape
            chartShapeObject.LockAspectRatio = True

            Dim chart As Chart = chartShape.Chart
            chart.ChartType = XlChartType.xlXYScatter
            chart.DisplayBlanksAs = XlDisplayBlanksAs.xlNotPlotted
            chart.PlotVisibleOnly = False
            chart.ChartArea.AutoScaleFont = False

            Dim seriesCollection As SeriesCollection = DirectCast(chart.SeriesCollection(), SeriesCollection)
            DeleteAllSeries(seriesCollection)
            ConfigureChartBackground(chart, resolvedAppearance)
            ConfigureAxes(chart, result)
            ConfigureTitle(chart, resolvedAppearance)

            For Each circle As PolarGridCircle In result.GridCircles
                AddLineSeries(seriesCollection,
                              circle.Coordinates,
                              "Polar grid",
                              resolvedAppearance.GridColor,
                              resolvedAppearance.GridLineWeight)
            Next

            For Each spoke As PolarSpoke In result.Spokes
                AddLineSeries(seriesCollection,
                              spoke.Coordinates,
                              "Polar spoke",
                              resolvedAppearance.SpokeColor,
                              resolvedAppearance.SpokeLineWeight)
            Next

            If resolvedAppearance.ShowAngularLabels Then
                AddLabelSeries(seriesCollection,
                               result.AngularLabels,
                               "Angular labels",
                               resolvedAppearance)
            End If

            If resolvedAppearance.ShowRadialLabels Then
                AddLabelSeries(seriesCollection,
                               result.RadialLabels,
                               "Radial labels",
                               resolvedAppearance)
            End If

            Dim legendSeriesIndices As New List(Of Integer)()
            Dim groups As PolarPlotGroupSeries() = result.GroupSeries
            For groupIndex As Integer = 0 To groups.Length - 1
                Dim group As PolarPlotGroupSeries = groups(groupIndex)
                Dim seriesName As String = ResolveSeriesName(group, groupIndex, result, resolvedAppearance)
                Dim seriesColor As Integer = ResolveGroupColor(groupIndex, result, resolvedAppearance)
                Dim markerStyle As XlMarkerStyle = ResolveGroupMarkerStyle(groupIndex,
                                                                          result,
                                                                          resolvedAppearance)

                If result.ConnectPoints Then
                    Dim segments As PolarPlotSeries() = group.DataSegments
                    For segmentIndex As Integer = 0 To segments.Length - 1
                        Dim newIndex As Integer = AddDataSeries(seriesCollection,
                                                               segments(segmentIndex),
                                                               seriesName,
                                                               seriesColor,
                                                               markerStyle,
                                                               resolvedAppearance,
                                                               True,
                                                               segmentIndex + 1)
                        If segmentIndex = 0 Then legendSeriesIndices.Add(newIndex)
                    Next
                Else
                    legendSeriesIndices.Add(AddDataSeries(seriesCollection,
                                                          group.MarkerSeries,
                                                          seriesName,
                                                          seriesColor,
                                                          markerStyle,
                                                          resolvedAppearance,
                                                          False,
                                                          1))
                End If
            Next

            Dim showLegend As Boolean = resolvedAppearance.ShowLegend OrElse
                                        (result.HasGrouping AndAlso resolvedAppearance.ShowGroupLegend)
            ConfigureLegend(chart, legendSeriesIndices, showLegend)
            MakePlotAreaSquare(chart)
            chart.Refresh()
            Return chart
        Catch
            If chartShape IsNot Nothing Then
                Try
                    chartShape.Delete()
                Catch
                    'Retain the original rendering exception if chart cleanup fails.
                End Try
            End If
            Throw
        End Try
    End Function

    ''' <summary>
    ''' Validates appearance dimensions and marker settings before creating a worksheet object.
    ''' </summary>
    ''' <param name="appearance">Appearance settings to validate.</param>
    ''' <param name="hasGrouping">Whether grouped palettes will be consumed.</param>
    Private Shared Sub ValidateAppearance(appearance As PolarPlotAppearance,
                                         hasGrouping As Boolean)
        If Not [Enum].IsDefined(GetType(PolarGroupStyleMode), appearance.GroupStyleMode) Then
            Throw New ArgumentOutOfRangeException(NameOf(appearance.GroupStyleMode),
                                                  appearance.GroupStyleMode,
                                                  "Unknown grouped-series style mode.")
        End If
        If Not [Enum].IsDefined(GetType(XlMarkerStyle), appearance.MarkerStyle) Then
            Throw New ArgumentOutOfRangeException(NameOf(appearance.MarkerStyle),
                                                  appearance.MarkerStyle,
                                                  "Unknown Excel marker style.")
        End If

        Dim usesGroupColor As Boolean = hasGrouping AndAlso
                                        (appearance.GroupStyleMode = PolarGroupStyleMode.Color OrElse
                                         appearance.GroupStyleMode = PolarGroupStyleMode.ColorAndMarker)
        If usesGroupColor AndAlso
           (appearance.GroupColors Is Nothing OrElse appearance.GroupColors.Length = 0) Then
            Throw New ArgumentException("GroupColors must contain at least one color when grouped series use color.",
                                        NameOf(appearance.GroupColors))
        End If

        Dim usesGroupMarker As Boolean = hasGrouping AndAlso
                                         (appearance.GroupStyleMode = PolarGroupStyleMode.Marker OrElse
                                          appearance.GroupStyleMode = PolarGroupStyleMode.ColorAndMarker)
        If usesGroupMarker AndAlso
           (appearance.GroupMarkerStyles Is Nothing OrElse appearance.GroupMarkerStyles.Length = 0) Then
            Throw New ArgumentException("GroupMarkerStyles must contain at least one marker when grouped series use marker shapes.",
                                        NameOf(appearance.GroupMarkerStyles))
        End If
        If usesGroupMarker Then
            For Each marker As XlMarkerStyle In appearance.GroupMarkerStyles
                If Not [Enum].IsDefined(GetType(XlMarkerStyle), marker) OrElse
                   marker = XlMarkerStyle.xlMarkerStyleNone Then
                    Throw New ArgumentOutOfRangeException(NameOf(appearance.GroupMarkerStyles),
                                                          marker,
                                                          "Every grouped marker style must be a visible Excel marker.")
                End If
            Next
        End If
        If appearance.MarkerSize < 2 OrElse appearance.MarkerSize > 72 Then
            Throw New ArgumentOutOfRangeException(NameOf(appearance.MarkerSize),
                                                  "Marker size must be between 2 and 72 points.")
        End If
        If Not IsFinitePositive(appearance.DataLineWeight) Then
            Throw New ArgumentOutOfRangeException(NameOf(appearance.DataLineWeight),
                                                  "Data-line weight must be positive and finite.")
        End If
        If Not IsFinitePositive(appearance.GridLineWeight) Then
            Throw New ArgumentOutOfRangeException(NameOf(appearance.GridLineWeight),
                                                  "Gridline weight must be positive and finite.")
        End If
        If Not IsFinitePositive(appearance.SpokeLineWeight) Then
            Throw New ArgumentOutOfRangeException(NameOf(appearance.SpokeLineWeight),
                                                  "Spoke-line weight must be positive and finite.")
        End If
        If Not IsFinitePositive(appearance.LabelFontSize) Then
            Throw New ArgumentOutOfRangeException(NameOf(appearance.LabelFontSize),
                                                  "Label font size must be positive and finite.")
        End If
    End Sub

    ''' <summary>
    ''' Checks that grid, label, and data sections fit within Excel's 255-series chart limit.
    ''' </summary>
    ''' <param name="result">Computed plot result.</param>
    ''' <param name="appearance">Resolved appearance settings.</param>
    Private Shared Sub ValidateSeriesCount(result As PolarPlotResult,
                                           appearance As PolarPlotAppearance)
        Dim required As Integer = result.GridCircles.Length + result.Spokes.Length
        If appearance.ShowAngularLabels Then required += 1
        If appearance.ShowRadialLabels Then required += 1
        If result.ConnectPoints Then
            required += result.DataSegments.Length
        Else
            required += result.GroupSeries.Length
        End If

        If required > MaximumExcelSeries Then
            Throw New InvalidOperationException(
                $"The polar plot requires {required} Excel series, exceeding Excel's {MaximumExcelSeries}-series limit. " &
                "Increase the radial or angular tick interval, clear 'Connect points', or reduce missing-value gaps.")
        End If
    End Sub

    ''' <summary>
    ''' Removes all automatically generated series from a newly created chart.
    ''' </summary>
    ''' <param name="seriesCollection">Target Excel series collection.</param>
    Private Shared Sub DeleteAllSeries(seriesCollection As SeriesCollection)
        Do While seriesCollection.Count > 0
            DirectCast(seriesCollection.Item(1), Series).Delete()
        Loop
    End Sub

    ''' <summary>
    ''' Configures chart-area and plot-area fills and removes their borders.
    ''' </summary>
    ''' <param name="chart">Chart to format.</param>
    ''' <param name="appearance">Resolved appearance settings.</param>
    Private Shared Sub ConfigureChartBackground(chart As Object, appearance As PolarPlotAppearance)
        'chart is a "Chart" object
        With chart
            .ChartArea.Format.Fill.Visible = True
            .ChartArea.Format.Fill.Solid()
            .ChartArea.Format.Fill.ForeColor.RGB = appearance.BackgroundColor
            .ChartArea.Format.Line.Visible = False

            .PlotArea.Format.Fill.Visible = True
            .PlotArea.Format.Fill.Solid()
            .PlotArea.Format.Fill.ForeColor.RGB = appearance.BackgroundColor
            .PlotArea.Format.Line.Visible = False
        End With
    End Sub

    ''' <summary>
    ''' Applies equal symmetric scales to both Cartesian axes and hides their visual elements.
    ''' </summary>
    ''' <param name="chart">Chart to configure.</param>
    ''' <param name="result">Computed plot geometry containing the recommended extent.</param>
    Private Shared Sub ConfigureAxes(chart As Chart, result As PolarPlotResult)
        Dim xAxis As Axis = DirectCast(chart.Axes(XlAxisType.xlCategory,
                                                  XlAxisGroup.xlPrimary), Axis)
        Dim yAxis As Axis = DirectCast(chart.Axes(XlAxisType.xlValue,
                                                  XlAxisGroup.xlPrimary), Axis)

        ConfigureHiddenAxis(xAxis, result.CartesianExtent, result.RadialMajorInterval)
        ConfigureHiddenAxis(yAxis, result.CartesianExtent, result.RadialMajorInterval)
    End Sub

    ''' <summary>
    ''' Configures one hidden Cartesian axis used only to position polar geometry.
    ''' </summary>
    ''' <param name="axis">Excel axis to configure.</param>
    ''' <param name="extent">Positive symmetric axis extent.</param>
    ''' <param name="majorInterval">Major interval used internally by Excel.</param>
    Private Shared Sub ConfigureHiddenAxis(axis As Axis,
                                           extent As Double,
                                           majorInterval As Double)
        With axis
            .MinimumScale = -extent
            .MaximumScale = extent
            .MajorUnit = majorInterval
            .CrossesAt = 0.0R
            .HasTitle = False
            .HasMajorGridlines = False
            .HasMinorGridlines = False
            .TickLabelPosition = XlTickLabelPosition.xlTickLabelPositionNone
            .MajorTickMark = XlTickMark.xlTickMarkNone
            .MinorTickMark = XlTickMark.xlTickMarkNone
        End With

        'Avoid a visible Cartesian cross while keeping Microsoft.Office.Core out
        'of the project's compile-time references.
        Dim axisObject As Object = axis
        axisObject.Format.Line.Visible = False
    End Sub

    ''' <summary>
    ''' Applies or suppresses the chart title.
    ''' </summary>
    ''' <param name="chart">Chart to configure.</param>
    ''' <param name="appearance">Resolved appearance settings.</param>
    Private Shared Sub ConfigureTitle(chart As Chart,
                                      appearance As PolarPlotAppearance)
        If String.IsNullOrWhiteSpace(appearance.ChartTitle) Then
            chart.HasTitle = False
        Else
            chart.HasTitle = True
            chart.ChartTitle.Text = appearance.ChartTitle
            chart.ChartTitle.Font.Size = 12
        End If
    End Sub

    ''' <summary>
    ''' Adds one marker-free line series for a grid circle or angular spoke.
    ''' </summary>
    ''' <param name="seriesCollection">Target Excel series collection.</param>
    ''' <param name="coordinates">Cartesian coordinates to draw.</param>
    ''' <param name="seriesName">Internal descriptive series name.</param>
    ''' <param name="color">OLE RGB line color.</param>
    ''' <param name="weight">Line width in points.</param>
    ''' <returns>The one-based index of the new Excel series.</returns>
    Private Shared Function AddLineSeries(seriesCollection As SeriesCollection,
                                          coordinates As PolarPlotSeries,
                                          seriesName As String,
                                          color As Integer,
                                          weight As Single) As Integer
        seriesCollection.NewSeries()
        'Dim series As Object = DirectCast(seriesCollection.Item(seriesCollection.Count), Series)
        With seriesCollection(seriesCollection.Count - 1)
            .Name = seriesName
            .ChartType = XlChartType.xlXYScatterLinesNoMarkers
            .XValues = coordinates.XValues
            .Values = coordinates.YValues
            .Smooth = False
            .MarkerStyle = XlMarkerStyle.xlMarkerStyleNone
            .Format.Line.Visible = True
            .Format.Line.ForeColor.RGB = color
            .Format.Line.Weight = weight
        End With

        Return seriesCollection.Count
    End Function

    ''' <summary>
    ''' Adds an invisible scatter series and uses its points as anchors for custom text labels.
    ''' </summary>
    ''' <param name="seriesCollection">Target Excel series collection.</param>
    ''' <param name="labels">Text and anchor coordinates.</param>
    ''' <param name="seriesName">Internal descriptive series name.</param>
    ''' <param name="appearance">Resolved label appearance.</param>
    ''' <returns>The one-based index of the new Excel series, or zero when no labels were supplied.</returns>
    Private Shared Function AddLabelSeries(seriesCollection As SeriesCollection,
                                           labels As PolarPlotLabel(),
                                           seriesName As String,
                                           appearance As PolarPlotAppearance) As Integer
        If labels Is Nothing OrElse labels.Length = 0 Then Return 0

        Dim xValues(labels.Length - 1) As Double
        Dim yValues(labels.Length - 1) As Double
        For i As Integer = 0 To labels.Length - 1
            xValues(i) = labels(i).X
            yValues(i) = labels(i).Y
        Next

        seriesCollection.NewSeries()
        'Dim series As Series = DirectCast(seriesCollection.Item(seriesCollection.Count), Series)
        With seriesCollection(seriesCollection.Count - 1)
            .Name = seriesName
            .ChartType = XlChartType.xlXYScatter
            .XValues = xValues
            .Values = yValues
            .MarkerStyle = XlMarkerStyle.xlMarkerStyleNone
            .Format.Line.Visible = False
            .ApplyDataLabels()

            For i As Integer = 1 To labels.Length
                Dim point As Point = DirectCast(.Points(i), Point)
                point.HasDataLabel = True
                point.DataLabel.Text = labels(i - 1).Text
                point.DataLabel.Position = XlDataLabelPosition.xlLabelPositionCenter
                point.DataLabel.Font.Size = appearance.LabelFontSize
                point.DataLabel.Font.Color = appearance.TextColor
            Next
        End With

        Return seriesCollection.Count
    End Function

    ''' <summary>
    ''' Resolves the visible series name for grouped or ungrouped data.
    ''' </summary>
    Private Shared Function ResolveSeriesName(group As PolarPlotGroupSeries,
                                              groupIndex As Integer,
                                              result As PolarPlotResult,
                                              appearance As PolarPlotAppearance) As String
        If result.HasGrouping Then
            If Not String.IsNullOrWhiteSpace(group.Name) Then Return group.Name
            Return "Group " & (groupIndex + 1).ToString(CultureInfo.CurrentCulture)
        End If
        Return If(String.IsNullOrWhiteSpace(appearance.SeriesName),
                  "Data",
                  appearance.SeriesName.Trim())
    End Function

    ''' <summary>
    ''' Resolves a grouped-series color, cycling the configured palette when necessary.
    ''' </summary>
    Private Shared Function ResolveGroupColor(groupIndex As Integer,
                                              result As PolarPlotResult,
                                              appearance As PolarPlotAppearance) As Integer
        Dim varyColor As Boolean = result.HasGrouping AndAlso
                                  (appearance.GroupStyleMode = PolarGroupStyleMode.Color OrElse
                                   appearance.GroupStyleMode = PolarGroupStyleMode.ColorAndMarker)
        If Not varyColor Then Return appearance.DataColor
        Return appearance.GroupColors(groupIndex Mod appearance.GroupColors.Length)
    End Function

    ''' <summary>
    ''' Resolves a grouped-series marker, cycling the configured palette when necessary.
    ''' </summary>
    Private Shared Function ResolveGroupMarkerStyle(groupIndex As Integer,
                                                    result As PolarPlotResult,
                                                    appearance As PolarPlotAppearance) As XlMarkerStyle
        Dim varyMarker As Boolean = result.HasGrouping AndAlso
                                   (appearance.GroupStyleMode = PolarGroupStyleMode.Marker OrElse
                                    appearance.GroupStyleMode = PolarGroupStyleMode.ColorAndMarker)
        If Not varyMarker Then Return appearance.MarkerStyle
        Return appearance.GroupMarkerStyles(groupIndex Mod appearance.GroupMarkerStyles.Length)
    End Function

    ''' <summary>
    ''' Adds one data section with visible markers and an optional connecting line.
    ''' </summary>
    ''' <param name="seriesCollection">Target Excel series collection.</param>
    ''' <param name="coordinates">Data coordinates to plot.</param>
    ''' <param name="seriesName">Legend name shared by every section of one group.</param>
    ''' <param name="seriesColor">OLE RGB line and marker color.</param>
    ''' <param name="markerStyle">Visible Excel marker style.</param>
    ''' <param name="appearance">Resolved series appearance.</param>
    ''' <param name="connectPoints">Whether the section uses a connecting line.</param>
    ''' <param name="segmentNumber">One-based section number used to create a unique internal name.</param>
    ''' <returns>The one-based index of the new Excel series.</returns>
    Private Shared Function AddDataSeries(seriesCollection As SeriesCollection,
                                          coordinates As PolarPlotSeries,
                                          seriesName As String,
                                          seriesColor As Integer,
                                          markerStyle As XlMarkerStyle,
                                          appearance As PolarPlotAppearance,
                                          connectPoints As Boolean,
                                          segmentNumber As Integer) As Integer
        seriesCollection.NewSeries()
        'Dim series As Series = DirectCast(seriesCollection.Item(seriesCollection.Count), Series)
        With seriesCollection(seriesCollection.Count - 1)
            Dim baseName As String = If(String.IsNullOrWhiteSpace(seriesName), "Data", seriesName)
            .Name = If(segmentNumber = 1, baseName, $"{baseName} {segmentNumber}")
            .ChartType = If(connectPoints,
                                  XlChartType.xlXYScatterLines,
                                  XlChartType.xlXYScatter)
            .XValues = coordinates.XValues
            .Values = coordinates.YValues
            .Smooth = False
            .MarkerStyle = markerStyle
            .MarkerSize = appearance.MarkerSize
            .MarkerForegroundColor = seriesColor
            .MarkerBackgroundColor = seriesColor

            If connectPoints Then
                .Format.Line.Visible = True
                .Format.Line.ForeColor.RGB = seriesColor
                .Format.Line.Weight = appearance.DataLineWeight
            Else
                .Format.Line.Visible = False
            End If
        End With

        Return seriesCollection.Count
    End Function

    ''' <summary>
    ''' Keeps one legend entry per rendered group and removes grid/label/extra-section entries.
    ''' </summary>
    ''' <param name="chart">Chart to configure.</param>
    ''' <param name="dataSeriesIndices">One-based chart-series indices for observation sections.</param>
    ''' <param name="showLegend">Whether the filtered legend should be displayed.</param>
    Private Shared Sub ConfigureLegend(chart As Chart,
                                       dataSeriesIndices As IList(Of Integer),
                                       showLegend As Boolean)
        If Not showLegend OrElse dataSeriesIndices Is Nothing OrElse dataSeriesIndices.Count = 0 Then
            chart.HasLegend = False
            Return
        End If

        chart.HasLegend = True
        chart.Legend.Position = XlLegendPosition.xlLegendPositionBottom
        Dim keepIndices As New HashSet(Of Integer)(dataSeriesIndices)
        Dim entries As LegendEntries = DirectCast(chart.Legend.LegendEntries(), LegendEntries)

        For i As Integer = entries.Count To 1 Step -1
            If Not keepIndices.Contains(i) Then
                DirectCast(entries.Item(i), LegendEntry).Delete()
            End If
        Next
    End Sub

    ''' <summary>
    ''' Makes the internal plot area square after Excel has laid out the title and legend.
    ''' </summary>
    ''' <param name="chart">Chart whose plot area is adjusted.</param>
    ''' <remarks>
    ''' Some Excel versions postpone plot-area measurements until the chart is
    ''' painted. Failure to apply the refinement is therefore nonfatal because the
    ''' enclosing chart shape is already square and both axes use identical limits.
    ''' </remarks>
    Private Shared Sub MakePlotAreaSquare(chart As Chart)
        Try
            Dim plotArea As PlotArea = chart.PlotArea
            Dim currentWidth As Double = plotArea.InsideWidth
            Dim currentHeight As Double = plotArea.InsideHeight
            Dim side As Double = Math.Min(currentWidth, currentHeight)
            Dim adjustedLeft As Double = plotArea.InsideLeft + (currentWidth - side) / 2.0R
            Dim adjustedTop As Double = plotArea.InsideTop + (currentHeight - side) / 2.0R

            plotArea.InsideLeft = adjustedLeft
            plotArea.InsideTop = adjustedTop
            plotArea.InsideWidth = side
            plotArea.InsideHeight = side
        Catch
            'The square chart shape and equal axes remain a safe fallback.
        End Try
    End Sub

    ''' <summary>
    ''' Tests whether a number is neither NaN nor infinity.
    ''' </summary>
    ''' <param name="value">Number to test.</param>
    ''' <returns><see langword="True"/> when the number is finite.</returns>
    Private Shared Function IsFinite(value As Double) As Boolean
        Return Not Double.IsNaN(value) AndAlso Not Double.IsInfinity(value)
    End Function

    ''' <summary>
    ''' Tests whether a number is finite and strictly positive.
    ''' </summary>
    ''' <param name="value">Number to test.</param>
    ''' <returns><see langword="True"/> when the number is finite and greater than zero.</returns>
    Private Shared Function IsFinitePositive(value As Double) As Boolean
        Return IsFinite(value) AndAlso value > 0.0R
    End Function

    ''' <summary>
    ''' Tests whether a single-precision number is finite and strictly positive.
    ''' </summary>
    ''' <param name="value">Number to test.</param>
    ''' <returns><see langword="True"/> when the number is finite and greater than zero.</returns>
    Private Shared Function IsFinitePositive(value As Single) As Boolean
        Return Not Single.IsNaN(value) AndAlso Not Single.IsInfinity(value) AndAlso value > 0.0F
    End Function
End Class
