Option Explicit On
Option Strict On
Imports System
Imports System.Linq

''' <summary>Standalone box policy: unchanged DescriptiveStat route on a clone.
''' Sorted outlier order is distinct from raw observation order. No finite filter.
''' Windows retains collection accumulation and fixed constructor counts.</summary>
Public NotInheritable Class BoxGroupAggregateData
    Private ReadOnly sorted() As Double, raw() As Double, small() As Double, large() As Double
    Private ReadOnly smallFlags() As Boolean, largeFlags() As Boolean
    Private ReadOnly order As NormalPlotObservationOrder
    Friend Sub New(values() As Double)
        raw = DirectCast(values.Clone(), Double())
        sorted = DirectCast(values.Clone(), Double())
        Dim ds As New DescriptiveStat(sorted)
        ds.compute(False)
        N = values.Length : LQuartile = ds.LQuartile : Median = ds.Median : UQuartile = ds.UQuartile
        IQR = ds.IQR : Mean = ds.Mean : Minimum = ds.Minimum : Maximum = ds.Maximum
        ReDim smallFlags(sorted.Length - 1), largeFlags(sorted.Length - 1)
        Dim lows As New Collections.Generic.List(Of Double), highs As New Collections.Generic.List(Of Double)
        For i As Integer = 0 To sorted.Length - 1
            smallFlags(i) = sorted(i) < (LQuartile - 1.5 * IQR)
            largeFlags(i) = sorted(i) > (UQuartile + 1.5 * IQR)
            If smallFlags(i) Then lows.Add(sorted(i))
            If largeFlags(i) Then highs.Add(sorted(i))
        Next
        small = lows.ToArray() : large = highs.ToArray()
        order = New NormalPlotObservationOrder(raw)
    End Sub
    Public ReadOnly Property N As Integer
    Public ReadOnly Property LQuartile As Double
    Public ReadOnly Property Median As Double
    Public ReadOnly Property UQuartile As Double
    Public ReadOnly Property IQR As Double
    Public ReadOnly Property Mean As Double
    Public ReadOnly Property Minimum As Double
    Public ReadOnly Property Maximum As Double
    Public ReadOnly Property RawValues As Double()
        Get
            Return DirectCast(raw.Clone(), Double())
        End Get
    End Property
    Public ReadOnly Property SortedValues As Double()
        Get
            Return DirectCast(sorted.Clone(), Double())
        End Get
    End Property
    Public ReadOnly Property SmallOutliers As Double()
        Get
            Return DirectCast(small.Clone(), Double())
        End Get
    End Property
    Public ReadOnly Property LargeOutliers As Double()
        Get
            Return DirectCast(large.Clone(), Double())
        End Get
    End Property
    Public ReadOnly Property SmallOutlierFlags As Boolean()
        Get
            Return DirectCast(smallFlags.Clone(), Boolean())
        End Get
    End Property
    Public ReadOnly Property LargeOutlierFlags As Boolean()
        Get
            Return DirectCast(largeFlags.Clone(), Boolean())
        End Get
    End Property
    Public ReadOnly Property SortedToInput As Integer()
        Get
            Return order.SortedToSource
        End Get
    End Property
End Class

Public NotInheritable Class BoxGroupPlotData
    Public ReadOnly Property Aggregate As BoxGroupAggregateData
    Public ReadOnly Property Segments As BoxSegmentData
    Public ReadOnly Property LowerWhiskerLength As Double
    Public ReadOnly Property UpperWhiskerLength As Double
    Public ReadOnly Property GroupOrdinal As Integer
    Private ReadOnly xs() As Double
    Friend Sub New(values() As Double, ordinal As Integer, halfWidth As Double)
        Aggregate = BoxPlotDataCore.Aggregate(values)
        GroupOrdinal = ordinal
        Segments = BoxSegmentsCore.Compute(Aggregate.LQuartile, Aggregate.Median, Aggregate.UQuartile)
        UpperWhiskerLength = BoxPlotDataCore.UpperWhisker(Aggregate, values, values.Length, Aggregate.LargeOutliers.Length)
        LowerWhiskerLength = BoxPlotDataCore.LowerWhisker(Aggregate, values, values.Length, Aggregate.SmallOutliers.Length)
        xs = DistributionPlotGeometryCore.ObservationX(ordinal + 1.0, values.Length, halfWidth)
    End Sub
    Public ReadOnly Property ObservationX As Double()
        Get
            Return DirectCast(xs.Clone(), Double())
        End Get
    End Property
End Class

Public NotInheritable Class BoxPlotSeriesData
    Private ReadOnly items() As BoxGroupPlotData, names() As String
    Friend Sub New(groups()() As Double, labels() As String, halfWidth As Double)
        names = If(labels Is Nothing, Nothing, DirectCast(labels.Clone(), String()))
        ReDim items(groups.Length - 1)
        For i As Integer = 0 To groups.Length - 1
            items(i) = New BoxGroupPlotData(groups(i), i, halfWidth)
        Next
    End Sub
    Public ReadOnly Property Groups As BoxGroupPlotData()
        Get
            Return DirectCast(items.Clone(), BoxGroupPlotData())
        End Get
    End Property
    Public ReadOnly Property Labels As String()
        Get
            Return If(names Is Nothing, Nothing, DirectCast(names.Clone(), String()))
        End Get
    End Property
    Public ReadOnly Property GroupPositions As Double()
        Get
            Return Enumerable.Range(1, items.Length).Select(Function(i) CDbl(i)).ToArray()
        End Get
    End Property
    Public ReadOnly Property Axis As ChartAxisScaleResult
        Get
            Return ChartAxisScalingCore.Compute(items.Min(Function(g) g.Aggregate.Minimum), items.Max(Function(g) g.Aggregate.Maximum))
        End Get
    End Property
End Class

Public NotInheritable Class BoxPlotDataCore
    Private Sub New()
    End Sub
    Public Shared Function Aggregate(values() As Double) As BoxGroupAggregateData
        Return New BoxGroupAggregateData(values)
    End Function
    Public Shared Function Compute(groups()() As Double, labels() As String, Optional jitterHalfWidth As Double = 0.12) As BoxPlotSeriesData
        Return New BoxPlotSeriesData(groups, labels, jitterHalfWidth)
    End Function
    Public Shared Function Compute(values(,) As Double, labels() As String, Optional jitterHalfWidth As Double = 0.12) As BoxPlotSeriesData
        Dim groups(values.GetLength(1) - 1)() As Double
        For i As Integer = 0 To groups.Length - 1
            groups(i) = DataManagement.ArrayUtilities.GetColumn(values, i)
        Next
        Return Compute(groups, labels, jitterHalfWidth)
    End Function
    Public Shared Function Axis(minima() As Double, maxima() As Double) As ChartAxisScaleResult
        Return ChartAxisScalingCore.Compute(minima.Min(), maxima.Max())
    End Function
    Public Shared Function UpperWhisker(summary As BoxGroupAggregateData, currentValues() As Double, observationCount As Integer, largeCount As Long) As Double
        Dim sorted = DirectCast(currentValues.Clone(), Double())
        Array.Sort(sorted)
        If summary.Maximum > summary.UQuartile Then
            If largeCount = 0 Then Return summary.Maximum - summary.UQuartile
            Dim j As Long
            For j = observationCount - 1 To CLng(observationCount / 2 - 1) Step -1
                If (sorted(CInt(j)) - (summary.UQuartile + 1.5 * summary.IQR)) <= 0 Then Exit For
            Next
            Return sorted(CInt(j)) - summary.UQuartile
        End If
        Return 0
    End Function
    Public Shared Function LowerWhisker(summary As BoxGroupAggregateData, currentValues() As Double, observationCount As Integer, smallCount As Long) As Double
        Dim sorted = DirectCast(currentValues.Clone(), Double())
        Array.Sort(sorted)
        If summary.LQuartile > summary.Minimum Then
            If smallCount = 0 Then Return summary.LQuartile - summary.Minimum
            Dim j As Long
            For j = 0 To CLng(observationCount / 2 - 1)
                If (sorted(CInt(j)) - (summary.LQuartile - 1.5 * summary.IQR)) >= 0 Then Exit For
            Next
            Return summary.LQuartile - sorted(CInt(j))
        End If
        Return 0
    End Function
End Class
