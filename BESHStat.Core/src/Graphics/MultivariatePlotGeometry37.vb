Option Explicit On
Option Strict On
Option Infer On

Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports BESHStatNG.DataManagement
Imports BESHStatNG.Multivariate

''' <summary>Numerical axes only. The renderer owns axis-setting order and tick appearance.</summary>
Public NotInheritable Class MultivariateAxes37
    Public Sub New(x As ChartAxisScaleResult, y As ChartAxisScaleResult)
        Me.X = x
        Me.Y = y
    End Sub
    Public ReadOnly Property X As ChartAxisScaleResult
    Public ReadOnly Property Y As ChartAxisScaleResult
End Class

''' <summary>Detached biplot vectors, in original observation/variable order.</summary>
Public NotInheritable Class BiplotCoordinates37
    Private ReadOnly _scoreX As Double(), _scoreY As Double(), _loadingX As Double(), _loadingY As Double()
    Friend Sub New(scoreX As Double(), scoreY As Double(), loadingX As Double(), loadingY As Double())
        _scoreX = DirectCast(scoreX.Clone(), Double())
        _scoreY = DirectCast(scoreY.Clone(), Double())
        _loadingX = DirectCast(loadingX.Clone(), Double())
        _loadingY = DirectCast(loadingY.Clone(), Double())
    End Sub
    Public ReadOnly Property ScoreX As Double()
        Get
            Return DirectCast(_scoreX.Clone(), Double())
        End Get
    End Property
    Public ReadOnly Property ScoreY As Double()
        Get
            Return DirectCast(_scoreY.Clone(), Double())
        End Get
    End Property
    Public ReadOnly Property LoadingX As Double()
        Get
            Return DirectCast(_loadingX.Clone(), Double())
        End Get
    End Property
    Public ReadOnly Property LoadingY As Double()
        Get
            Return DirectCast(_loadingY.Clone(), Double())
        End Get
    End Property
End Class

''' <summary>Original chart arithmetic, without estimation, normalization or finite filtering.
''' Null, empty and nonfinite behavior deliberately follows the frozen renderer expressions.</summary>
Public NotInheritable Class MultivariatePlotGeometry37
    Private Sub New()
    End Sub

    Public Shared Function Column(values(,) As Double, ordinal As Integer) As Double()
        Return ArrayUtilities.GetColumn(values, ordinal)
    End Function

    Public Shared Function Axes(x As Double(), y As Double()) As MultivariateAxes37
        Return New MultivariateAxes37(ChartAxisScalingCore.Compute(x.Min(), x.Max()),
                                      ChartAxisScalingCore.Compute(y.Min(), y.Max()))
    End Function

    Public Shared Function SymmetricAxes(x As Double(), y As Double()) As MultivariateAxes37
        Dim sx As Double = Math.Max(Math.Abs(x.Min()), Math.Abs(x.Max()))
        Dim sy As Double = Math.Max(Math.Abs(y.Min()), Math.Abs(y.Max()))
        Return New MultivariateAxes37(ChartAxisScalingCore.Compute(-sx, sx), ChartAxisScalingCore.Compute(-sy, sy))
    End Function

    Public Shared Function CombinedAxes(x As Double(), y As Double(), loadingX As Double(), loadingY As Double()) As MultivariateAxes37
        Return New MultivariateAxes37(
            ChartAxisScalingCore.Compute(Math.Min(x.Min(), loadingX.Min()), Math.Max(x.Max(), loadingX.Max())),
            ChartAxisScalingCore.Compute(Math.Min(y.Min(), loadingY.Min()), Math.Max(y.Max(), loadingY.Max())))
    End Function

    Public Shared Function Biplot(scores(,) As Double, loadings(,) As Double, eigenvalues() As Double,
                                  observationCount As Integer, variableCount As Integer, c As Double) As BiplotCoordinates37
        Dim pc1() As Double = Column(scores, 0)
        Dim pc2() As Double = Column(scores, 1)
        Dim load1() As Double = Column(loadings, 0)
        Dim load2() As Double = Column(loadings, 1)
        Dim lam(1) As Double
        For i As Integer = 0 To 1
            lam(i) = Math.Sqrt(eigenvalues(i)) * Math.Sqrt(observationCount)
            lam(i) = lam(i) ^ (1.0 - c)
        Next
        For i As Integer = 0 To observationCount - 1
            pc1(i) /= lam(0)
            pc2(i) /= lam(1)
        Next
        For i As Integer = 0 To variableCount - 1
            load1(i) *= lam(0)
            load2(i) *= lam(1)
        Next
        Return New BiplotCoordinates37(pc1, pc2, load1, load2)
    End Function

    Public Shared Function ComponentAxis(count As Integer) As Integer()
        Dim result(count - 1) As Integer
        For i As Integer = 0 To count - 1
            result(i) = i + 1
        Next
        Return result
    End Function

    Public Shared Function InitialFactorPercent(eigenvalues As Double(), workingMatrix As Double(,)) As Double()
        Return PercentOfTotal(eigenvalues, TotalVariance(workingMatrix))
    End Function

    Public Shared Function FactorPercent(pattern As Double(,), structureMatrix As Double(,), workingMatrix As Double(,)) As Double()
        Dim sums(pattern.GetLength(1) - 1) As Double
        For j As Integer = 0 To pattern.GetLength(1) - 1
            Dim s As Double = 0.0
            For i As Integer = 0 To pattern.GetLength(0) - 1
                s += pattern(i, j) * structureMatrix(i, j)
            Next
            sums(j) = s
        Next
        Return PercentOfTotal(sums, TotalVariance(workingMatrix))
    End Function

    Private Shared Function PercentOfTotal(values As Double(), total As Double) As Double()
        Dim result(values.Length - 1) As Double
        If total <= 0.0 Then Return result
        For i As Integer = 0 To values.Length - 1
            result(i) = 100.0 * values(i) / total
        Next
        Return result
    End Function

    Private Shared Function TotalVariance(values As Double(,)) As Double
        Dim s As Double = 0.0
        For i As Integer = 0 To Math.Min(values.GetLength(0), values.GetLength(1)) - 1
            s += values(i, i)
        Next
        Return s
    End Function

    Public Shared Function CorrespondenceAxes(model As CA) As MultivariateAxes37
        'Y precedes X, including the historically undisplayed MCA row coordinates.
        Dim y = ChartAxisScalingCore.Compute(Math.Min(model.RowFactors(1).Min(), model.ColFactors(1).Min()),
                                              Math.Max(model.RowFactors(1).Max(), model.ColFactors(1).Max()))
        Dim x = ChartAxisScalingCore.Compute(Math.Min(model.RowFactors(0).Min(), model.ColFactors(0).Min()),
                                              Math.Max(model.RowFactors(0).Max(), model.ColFactors(0).Max()))
        Return New MultivariateAxes37(x, y)
    End Function

    Public Shared Function Contribution(model As CA, axis As Integer, row As Boolean) As Double()
        Return If(row, model.RowContribution(axis), model.ColContribution(axis))
    End Function

    ''' <summary>One loading-arrow coordinate, prepared at the original native assignment phase.</summary>
    Public Shared Function LoadingVector(value As Double) As Double()
        Return New Double() {0, value}
    End Function

    Public Shared Function ZeroPair() As Integer()
        Return New Integer() {0, 0}
    End Function
End Class

Public NotInheritable Class DendrogramBounds37
    Friend Sub New(xMinimum As Double, xMaximum As Double, yMinimum As Double, yMaximum As Double)
        XMin = xMinimum : XMax = xMaximum : YMin = yMinimum : YMax = yMaximum
    End Sub
    Public ReadOnly Property XMin As Double
    Public ReadOnly Property XMax As Double
    Public ReadOnly Property YMin As Double
    Public ReadOnly Property YMax As Double
End Class

''' <summary>Snapshot of authoritative existing layout. No paths, cuts or leaf order are recomputed.
''' Bounds remain a separate late operation, after the original series phase.</summary>
Public NotInheritable Class DendrogramChartGeometry37
    Private ReadOnly _layout As DendrogramLayout
    Public Sub New(layout As DendrogramLayout)
        _layout = New DendrogramLayout With {
            .PolylineX = Copy(layout.PolylineX), .PolylineY = Copy(layout.PolylineY),
            .ClusterPolylineX = CopySegments(layout.ClusterPolylineX), .ClusterPolylineY = CopySegments(layout.ClusterPolylineY),
            .CutLineX = Copy(layout.CutLineX), .CutLineY = Copy(layout.CutLineY),
            .LeafX = Copy(layout.LeafX), .LeafY = Copy(layout.LeafY), .LeafLabels = Copy(layout.LeafLabels),
            .LeafOriginalRowOrder = Copy(layout.LeafOriginalRowOrder), .Orientation = layout.Orientation,
            .HeightMode = layout.HeightMode, .MaximumHeight = layout.MaximumHeight, .LeafCount = layout.LeafCount}
    End Sub
    Private Shared Function Copy(Of T)(values As T()) As T()
        Return If(values Is Nothing, Nothing, DirectCast(values.Clone(), T()))
    End Function
    Private Shared Function CopySegments(values As List(Of Double())) As List(Of Double())
        Return If(values Is Nothing, Nothing, values.Select(Function(v) Copy(v)).ToList())
    End Function
    Public ReadOnly Property PolylineX As Double()
        Get
            Return Copy(_layout.PolylineX)
        End Get
    End Property
    Public ReadOnly Property PolylineY As Double()
        Get
            Return Copy(_layout.PolylineY)
        End Get
    End Property
    Public ReadOnly Property ClusterPolylineX As List(Of Double())
        Get
            Return CopySegments(_layout.ClusterPolylineX)
        End Get
    End Property
    Public ReadOnly Property ClusterPolylineY As List(Of Double())
        Get
            Return CopySegments(_layout.ClusterPolylineY)
        End Get
    End Property
    Public ReadOnly Property CutLineX As Double()
        Get
            Return Copy(_layout.CutLineX)
        End Get
    End Property
    Public ReadOnly Property CutLineY As Double()
        Get
            Return Copy(_layout.CutLineY)
        End Get
    End Property
    Public ReadOnly Property LeafX As Double()
        Get
            Return Copy(_layout.LeafX)
        End Get
    End Property
    Public ReadOnly Property LeafY As Double()
        Get
            Return Copy(_layout.LeafY)
        End Get
    End Property
    Public ReadOnly Property LeafLabels As String()
        Get
            Return Copy(_layout.LeafLabels)
        End Get
    End Property
    ''' <summary>One-based original input ordinals, never physical worksheet rows.</summary>
    Public ReadOnly Property LeafOriginalRowOrder As Integer()
        Get
            Return Copy(_layout.LeafOriginalRowOrder)
        End Get
    End Property
    Public ReadOnly Property Orientation As DendrogramOrientation
        Get
            Return _layout.Orientation
        End Get
    End Property
    Public ReadOnly Property HeightMode As DendrogramHeightMode
        Get
            Return _layout.HeightMode
        End Get
    End Property
    Public Function SuggestedAxisTitle() As String
        Return _layout.GetSuggestedAxisTitle()
    End Function
    Public Function Bounds() As DendrogramBounds37
        Dim xMin As Double = _layout.PolylineX.Min()
        Dim xMax As Double = _layout.PolylineX.Max()
        Dim yMin As Double = _layout.PolylineY.Min()
        Dim yMax As Double = _layout.PolylineY.Max()
        If _layout.CutLineX IsNot Nothing AndAlso _layout.CutLineX.Length > 0 Then
            xMin = Math.Min(xMin, _layout.CutLineX.Min())
            xMax = Math.Max(xMax, _layout.CutLineX.Max())
        End If
        If _layout.CutLineY IsNot Nothing AndAlso _layout.CutLineY.Length > 0 Then
            yMin = Math.Min(yMin, _layout.CutLineY.Min())
            yMax = Math.Max(yMax, _layout.CutLineY.Max())
        End If
        Dim xReference As Double = If(_layout.Orientation = DendrogramOrientation.Left OrElse _layout.Orientation = DendrogramOrientation.Right,
            If(_layout.MaximumHeight <= 0, 1.0, _layout.MaximumHeight), _layout.LeafCount)
        Dim yReference As Double = If(_layout.Orientation = DendrogramOrientation.Left OrElse _layout.Orientation = DendrogramOrientation.Right,
            _layout.LeafCount, If(_layout.MaximumHeight <= 0, 1.0, _layout.MaximumHeight))
        Dim xPad As Double = ClusterAnalysisHelpers.AxisPadding(xMin, xMax, xReference)
        Dim yPad As Double = ClusterAnalysisHelpers.AxisPadding(yMin, yMax, yReference)
        Return New DendrogramBounds37(xMin - xPad, xMax + xPad, yMin - yPad, yMax + yPad)
    End Function
End Class
