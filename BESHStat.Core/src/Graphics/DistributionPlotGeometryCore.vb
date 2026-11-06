Option Explicit On
Option Strict On
Imports System
Imports System.Linq

Public NotInheritable Class PlotCoordinatesData
    Private ReadOnly xs(), ys() As Double
    Public Sub New(x() As Double, y() As Double)
        xs = DirectCast(x.Clone(), Double()) : ys = DirectCast(y.Clone(), Double())
    End Sub
    Public ReadOnly Property X As Double()
        Get
            Return DirectCast(xs.Clone(), Double())
        End Get
    End Property
    Public ReadOnly Property Y As Double()
        Get
            Return DirectCast(ys.Clone(), Double())
        End Get
    End Property
End Class

Public NotInheritable Class DistributionPlotGeometryCore
    Private Sub New()
    End Sub
    Public Shared Function DeterministicJitter(index As Integer, halfWidth As Double) As Double
        If halfWidth <= 0.0R Then Return 0.0R
        Const GoldenFraction As Double = 0.6180339887498949R
        Dim u As Double = ((index + 1) * GoldenFraction) Mod 1.0R
        Return (2.0R * u - 1.0R) * halfWidth
    End Function
    Public Shared Function ObservationX(groupX As Double, count As Integer, halfWidth As Double) As Double()
        Dim xs(count - 1) As Double
        For i As Integer = 0 To count - 1
            xs(i) = groupX + DeterministicJitter(i, halfWidth)
        Next
        Return xs
    End Function
    Public Shared Function ViolinOutline(groupX As Double, evaluationPoints() As Double, halfWidths() As Double) As PlotCoordinatesData
        If evaluationPoints.Length <> halfWidths.Length OrElse evaluationPoints.Length < 2 Then Throw New ArgumentException("Violin evaluation points and widths are inconsistent.")
        Dim xs(evaluationPoints.Length * 2 - 1), ys(evaluationPoints.Length * 2 - 1) As Double
        Dim k As Integer = 0
        For i As Integer = 0 To evaluationPoints.Length - 1
            xs(k) = groupX + halfWidths(i) : ys(k) = evaluationPoints(i) : k += 1
        Next
        For i As Integer = evaluationPoints.Length - 1 To 0 Step -1
            xs(k) = groupX - halfWidths(i) : ys(k) = evaluationPoints(i) : k += 1
        Next
        Return New PlotCoordinatesData(xs, ys)
    End Function
    ''' <summary>Left/right box and cap coordinates in data units. Pixel widths stay host-owned.</summary>
    Public Shared Function ViolinInnerX(groupX As Double, halfWidths() As Double, boxHalfWidth As Double, capHalfWidth As Double) As Double()
        Dim displayedMaximum = halfWidths.Max()
        Dim effectiveBox = Math.Min(boxHalfWidth, displayedMaximum * 0.45R)
        Dim effectiveCap = Math.Min(capHalfWidth, displayedMaximum * 0.35R)
        Return {groupX - effectiveBox, groupX + effectiveBox, groupX - effectiveCap, groupX + effectiveCap}
    End Function
    Public Shared Function VisibleObservations(values() As Double, minimum As Double, maximum As Double) As Boolean()
        Return values.Select(Function(value) Not (value < minimum OrElse value > maximum)).ToArray()
    End Function
End Class

''' <summary>Data-domain axis policy for violin only. Automatic major-unit stays
''' unavailable unless supplied: the renderer historically leaves it to the host.</summary>
Public NotInheritable Class ViolinPlotDomainData
    Private ReadOnly positions(), anchors() As Double, names() As String, seriesData() As ViolinSeriesDomainData
    Public Sub New(result As ViolinPlotResult, Optional minimum As Double? = Nothing, Optional maximum As Double? = Nothing, Optional majorUnit As Double? = Nothing, Optional jitterHalfWidth As Double = 0.12)
        XMinimum = 0.5R : XMaximum = result.GroupCount + 0.5R
        Dim low = If(minimum, 0.0), high = If(maximum, 0.0)
        If Not minimum.HasValue OrElse Not maximum.HasValue Then
            Dim auto = AutomaticLimits(result.YMinimum, result.YMaximum)
            If Not minimum.HasValue Then low = auto(0)
            If Not maximum.HasValue Then high = auto(1)
        End If
        If low >= high Then Throw New ArgumentException("Resolved Y-axis minimum must be smaller than its maximum.")
        YMinimum = low : YMaximum = high : YMajorUnit = majorUnit
        Dim sources = result.Series
        ReDim positions(sources.Length - 1), anchors(sources.Length - 1), names(sources.Length - 1), seriesData(sources.Length - 1)
        Dim anchorY = low + 0.012R * (high - low)
        For i As Integer = 0 To sources.Length - 1
            positions(i) = i + 1.0R : anchors(i) = anchorY : names(i) = sources(i).Name
            seriesData(i) = New ViolinSeriesDomainData(sources(i), i, jitterHalfWidth)
        Next
    End Sub
    Private Shared Function Finite(value As Double) As Boolean
        Return Not Double.IsNaN(value) AndAlso Not Double.IsInfinity(value)
    End Function
    Public Shared Function AutomaticLimits(dataMinimum As Double, dataMaximum As Double) As Double()
        If Finite(dataMinimum) AndAlso Finite(dataMaximum) AndAlso dataMaximum > dataMinimum Then
            Dim scale = ChartAxisScalingCore.Compute(dataMinimum, dataMaximum)
            If scale IsNot Nothing AndAlso Finite(scale.Min) AndAlso Finite(scale.Max) AndAlso scale.Max > scale.Min Then Return {scale.Min, scale.Max}
        End If
        Dim centre As Double = If(Finite(dataMinimum), dataMinimum, 0.0R)
        Dim halfRange As Double = Math.Max(1.0R, Math.Abs(centre) * 0.1R)
        Return {centre - halfRange, centre + halfRange}
    End Function
    Public ReadOnly Property XMinimum As Double
    Public ReadOnly Property XMaximum As Double
    Public ReadOnly Property YMinimum As Double
    Public ReadOnly Property YMaximum As Double
    Public ReadOnly Property YMajorUnit As Double?
    Public ReadOnly Property GroupPositions As Double()
        Get
            Return DirectCast(positions.Clone(), Double())
        End Get
    End Property
    Public ReadOnly Property CategoryLabelY As Double()
        Get
            Return DirectCast(anchors.Clone(), Double())
        End Get
    End Property
    Public ReadOnly Property Labels As String()
        Get
            Return DirectCast(names.Clone(), String())
        End Get
    End Property
    Public ReadOnly Property Series As ViolinSeriesDomainData()
        Get
            Return DirectCast(seriesData.Clone(), ViolinSeriesDomainData())
        End Get
    End Property
End Class

Public NotInheritable Class ViolinSeriesDomainData
    Private ReadOnly source As ViolinPlotSeries, observationXs() As Double
    Public ReadOnly Property GroupOrdinal As Integer
    Public ReadOnly Property Outline As PlotCoordinatesData
    Friend Sub New(series As ViolinPlotSeries, ordinal As Integer, jitterHalfWidth As Double)
        source = series : GroupOrdinal = ordinal
        Outline = DistributionPlotGeometryCore.ViolinOutline(ordinal + 1.0, series.EvaluationPoints, series.ScaledHalfWidths)
        observationXs = DistributionPlotGeometryCore.ObservationX(ordinal + 1.0, series.SampleSize, jitterHalfWidth)
    End Sub
    Public ReadOnly Property SourceObservationOrdinals As Integer()
        Get
            Return source.SourceObservationOrdinals
        End Get
    End Property
    Public ReadOnly Property Observations As Double()
        Get
            Return source.Observations
        End Get
    End Property
    Public ReadOnly Property ObservationX As Double()
        Get
            Return DirectCast(observationXs.Clone(), Double())
        End Get
    End Property
End Class
