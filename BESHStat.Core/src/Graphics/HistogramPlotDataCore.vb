Option Explicit On
Option Strict On
Imports System
Imports System.Linq

''' <summary>Histogram-only Tukey summary, using QuartilesComp/ResolveTukeyWhiskers.
''' This is not the standalone box whisker-loop policy.</summary>
Public NotInheritable Class HistogramTukeyData
    Private ReadOnly outlierData() As Double
    Public Sub New(values() As Double)
        Dim sorted = DirectCast(values.Clone(), Double())
        Array.Sort(sorted)
        Dim quartiles = StatFunc.QuartilesComp(DirectCast(sorted.Clone(), Double()))
        Q1 = quartiles.Q1 : Median = quartiles.Median : Q3 = quartiles.Q3
        Dim low As Double, high As Double
        StatFunc.ResolveTukeyWhiskers(sorted, Q1, Q3, low, high)
        WhiskerLow = low : WhiskerHigh = high
        outlierData = sorted.Where(Function(value) value < low OrElse value > high).ToArray()
    End Sub
    Public ReadOnly Property Q1 As Double
    Public ReadOnly Property Median As Double
    Public ReadOnly Property Q3 As Double
    Public ReadOnly Property WhiskerLow As Double
    Public ReadOnly Property WhiskerHigh As Double
    Public ReadOnly Property Outliers As Double()
        Get
            Return DirectCast(outlierData.Clone(), Double())
        End Get
    End Property
End Class

Public NotInheritable Class HistogramAxisData
    Public Sub New(midpoints() As Double, frequencies() As Integer, overlayY() As Double, drawBox As Boolean)
        Dim bounds = HistogramDataCore.HorizontalBounds(midpoints)
        XMinimum = bounds.Minimum : XMaximum = bounds.Maximum
        Dim positiveMax As Double = 1.0
        For Each f As Integer In frequencies
            If f > positiveMax Then positiveMax = f
        Next
        If overlayY IsNot Nothing Then
            For Each y As Double In overlayY
                If Not Double.IsNaN(y) AndAlso Not Double.IsInfinity(y) AndAlso y > positiveMax Then positiveMax = y
            Next
        End If
        Dim scale = ChartAxisScalingCore.Compute(0.0, positiveMax)
        Dim ymax = scale.Max, major = scale.Scale
        If ymax <= 0 OrElse Double.IsNaN(ymax) OrElse Double.IsInfinity(ymax) Then ymax = 1.0
        If major <= 0 OrElse Double.IsNaN(major) OrElse Double.IsInfinity(major) Then major = 1.0
        YMaximum = ymax : YMajorUnit = major
        YMinimum = If(drawBox, -0.32 * ymax, 0.0)
        BoxCentreY = -0.18 * ymax : BoxHalfHeight = 0.055 * ymax
    End Sub
    Public ReadOnly Property XMinimum As Double
    Public ReadOnly Property XMaximum As Double
    Public ReadOnly Property YMinimum As Double
    Public ReadOnly Property YMaximum As Double
    Public ReadOnly Property YMajorUnit As Double
    Public ReadOnly Property BoxCentreY As Double
    Public ReadOnly Property BoxHalfHeight As Double
End Class

''' <summary>Immutable current histogram payload. ClassificationEdges are actual
''' PrettyBreaks;DisplayBounds are the distinct legacy midpoint-derived chart span.
''' NormalY is already frequency-scaled by the existing engine;never rescale it.</summary>
Public NotInheritable Class HistogramPlotSeriesData
    Private ReadOnly binsData(,) As Object, edgesData() As Double, mids() As Double, freq() As Integer
    Private ReadOnly normalXData() As Double, normalYData() As Double, outlierData() As Double
    Public Sub New(bins(,) As Object, edges() As Double, midpoints() As Double, frequencies() As Integer, normalX() As Double, normalY() As Double, hasBox As Boolean, boxValues() As Double, outliers() As Double, drawBox As Boolean)
        binsData = If(bins Is Nothing, Nothing, DirectCast(bins.Clone(), Object(,)))
        edgesData = Copy(edges) : mids = Copy(midpoints)
        freq = If(frequencies Is Nothing, Nothing, DirectCast(frequencies.Clone(), Integer()))
        normalXData = Copy(normalX) : normalYData = Copy(normalY) : outlierData = Copy(outliers)
        HasBoxSummary = hasBox
        HasOverlay = normalX IsNot Nothing AndAlso normalY IsNot Nothing
        DrawBoxSummary = drawBox AndAlso hasBox
        Q1 = boxValues(0) : Median = boxValues(1) : Q3 = boxValues(2) : WhiskerLow = boxValues(3) : WhiskerHigh = boxValues(4)
    End Sub
    Private Shared Function Copy(v() As Double) As Double()
        Return If(v Is Nothing, Nothing, DirectCast(v.Clone(), Double()))
    End Function
    Public ReadOnly Property HasOverlay As Boolean
    Public ReadOnly Property HasBoxSummary As Boolean
    Public ReadOnly Property DrawBoxSummary As Boolean
    Public ReadOnly Property Q1 As Double
    Public ReadOnly Property Median As Double
    Public ReadOnly Property Q3 As Double
    Public ReadOnly Property WhiskerLow As Double
    Public ReadOnly Property WhiskerHigh As Double
    Public ReadOnly Property Bins As Object(,)
        Get
            Return If(binsData Is Nothing, Nothing, DirectCast(binsData.Clone(), Object(,)))
        End Get
    End Property
    Public ReadOnly Property ClassificationEdges As Double()
        Get
            Return Copy(edgesData)
        End Get
    End Property
    Public ReadOnly Property Midpoints As Double()
        Get
            Return Copy(mids)
        End Get
    End Property
    Public ReadOnly Property Frequencies As Integer()
        Get
            Return If(freq Is Nothing, Nothing, DirectCast(freq.Clone(), Integer()))
        End Get
    End Property
    Public ReadOnly Property NormalX As Double()
        Get
            Return Copy(normalXData)
        End Get
    End Property
    Public ReadOnly Property NormalY As Double()
        Get
            Return Copy(normalYData)
        End Get
    End Property
    Public ReadOnly Property Outliers As Double()
        Get
            Return Copy(outlierData)
        End Get
    End Property
    Public ReadOnly Property Axis As HistogramAxisData
        Get
            Return New HistogramAxisData(mids, freq, If(HasOverlay, normalYData, Nothing), DrawBoxSummary)
        End Get
    End Property
End Class

Public NotInheritable Class HistogramPlotDataCore
    Private Sub New()
    End Sub
    Public Shared Function Compute(values() As Double, overlay As Boolean, Optional rule As String = "", Optional box As Boolean = False) As HistogramPlotSeriesData
        Dim b = HistogramDataCore.BinsAndEdgesCopy(values, rule), bins = b.Bins
        Dim mids = DataManagement.ArrayUtilities.GetColumn(bins, 0).Select(Function(x) Convert.ToDouble(x)).ToArray()
        Dim freq = DataManagement.ArrayUtilities.GetColumn(bins, 1).Select(Function(x) Convert.ToInt32(x)).ToArray()
        Dim normalX() As Double = Nothing, normalY() As Double = Nothing
        If overlay Then
            Dim normal = HistogramDataCore.OverlayCopy(values, mids)
            normalX = DataManagement.ArrayUtilities.GetColumn(normal, 0)
            normalY = DataManagement.ArrayUtilities.GetColumn(normal, 1)
        End If
        Dim summary As HistogramTukeyData = Nothing
        If box AndAlso values IsNot Nothing AndAlso values.Length > 0 Then summary = New HistogramTukeyData(values)
        Dim q() As Double = {0, 0, 0, 0, 0}
        If summary IsNot Nothing Then q = {summary.Q1, summary.Median, summary.Q3, summary.WhiskerLow, summary.WhiskerHigh}
        Return New HistogramPlotSeriesData(bins, b.Edges, mids, freq, normalX, normalY, summary IsNot Nothing, q, If(summary Is Nothing, Nothing, summary.Outliers), box)
    End Function
End Class
