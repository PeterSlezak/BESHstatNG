Option Explicit On
Option Strict Off
Imports System
Imports System.Linq
Imports System.Collections.Generic
Imports BESHStatNG
Namespace Global.Batch32Fixtures
Public Class LegacyBox32
    Inherits Batch30Fixtures.LegacyBox30
    Friend pArOutliersSmall(,),pArOutliersBig(,) As Double
    Public Sub New(groups As Double()())
        pData=groups : pNoGroups=groups.Length
        pNs=groups.Select(Function(g) g.Length).ToArray()
    End Sub
        Sub Calculate()
            ReDim pArOutliersBig(pNs.Max() / 2, pNoGroups - 1)
            ReDim pArOutliersSmall(pNs.Max() / 2, pNoGroups - 1)
            ReDim pArNOutliersBig(pNoGroups - 1), pArNOutliersSmall(pNoGroups - 1)

            For i = 0 To pNoGroups - 1
                Dim arTemporal() As Double = DirectCast(pData(i).Clone(), Double())

                Dim DescriptiveS As DescriptiveStat = New DescriptiveStat(arTemporal)
                DescriptiveS.compute(False)
                pDSCollection.Add(DescriptiveS) 'Add to Collection

                With DescriptiveS
                    For j = 0 To pNs(i) - 1
                        If arTemporal(j) < (.LQuartile - 1.5 * .IQR) Then
                            pArNOutliersSmall(i) += 1
                            pArOutliersSmall(pArNOutliersSmall(i) - 1, i) = arTemporal(j)
                        End If
                        If arTemporal(j) > (.UQuartile + 1.5 * .IQR) Then
                            pArNOutliersBig(i) += 1
                            pArOutliersBig(pArNOutliersBig(i) - 1, i) = arTemporal(j)
                        End If
                    Next
                End With
            Next  'next group

        End Sub
End Class
Public Class LegacyHistogram32
    Friend data(), arBinMidVal(), arXi(), arGauss(), arOutliers() As Double
    Friend arFreq() As Integer
    Friend pQ1,pMedian,pQ3,pWhiskerLow,pWhiskerHigh As Double
    Friend pHasBoxSummary As Boolean
    Public Sub New(values As Double())
        data=values
    End Sub
        Public Function Compute(bOveraly As Boolean,
                                Optional strType As String = "",
                                Optional bBoxPlot As Boolean = False) As Object(,)

            Dim bins = HistogramDataCore.BinsCopy(Me.data, strType)
            Me.arBinMidVal = DataManagement.ArrayUtilities.GetColumn(bins, 0).Select(Function(x) Convert.ToDouble(x)).ToArray()
            Me.arFreq = DataManagement.ArrayUtilities.GetColumn(bins, 1).Select(Function(x) Convert.ToInt32(x)).ToArray()

            Me.arXi = Nothing
            Me.arGauss = Nothing

            If bOveraly Then
                Dim overlayData = HistogramDataCore.OverlayCopy(Me.data, Me.arBinMidVal)
                Me.arXi = DataManagement.ArrayUtilities.GetColumn(overlayData, 0)
                Me.arGauss = DataManagement.ArrayUtilities.GetColumn(overlayData, 1)
            End If

            Me.pHasBoxSummary = False
            Me.arOutliers = Nothing
            If bBoxPlot AndAlso Me.data IsNot Nothing AndAlso Me.data.Length > 0 Then
                Dim sorted() As Double = DirectCast(Me.data.Clone(), Double())
                Array.Sort(sorted)

                Dim quartileInput() As Double = DirectCast(sorted.Clone(), Double())
                Dim quartiles As udQuartiles = StatFunc.QuartilesComp(quartileInput)

                Me.pQ1 = quartiles.Q1
                Me.pMedian = quartiles.Median
                Me.pQ3 = quartiles.Q3
                StatFunc.ResolveTukeyWhiskers(sorted,
                                              Me.pQ1,
                                              Me.pQ3,
                                              Me.pWhiskerLow,
                                              Me.pWhiskerHigh)

                Dim outlierList As New List(Of Double)
                For Each value As Double In sorted
                    If value < Me.pWhiskerLow OrElse value > Me.pWhiskerHigh Then
                        outlierList.Add(value)
                    End If
                Next
                Me.arOutliers = outlierList.ToArray()
                Me.pHasBoxSummary = True
            End If

            Return bins
        End Function
    Public Function Axes(bBoxPlot As Boolean) As Double()
            Dim bOverlay As Boolean = (Me.arGauss IsNot Nothing AndAlso Me.arXi IsNot Nothing)
            Dim bDrawBoxPlot As Boolean = bBoxPlot AndAlso Me.pHasBoxSummary
            Dim bNeedsSecondaryAxes As Boolean = bOverlay

            Dim iNoBins As Integer = Me.arBinMidVal.Length
            If iNoBins = 0 Then Return Nothing

            Dim horizontalBounds = HistogramDataCore.HorizontalBounds(Me.arBinMidVal)
            Dim dXMin As Double = horizontalBounds.Minimum
            Dim dXMax As Double = horizontalBounds.Maximum

            Dim dPositiveMax As Double = 1.0R
            For Each f As Integer In Me.arFreq
                If f > dPositiveMax Then dPositiveMax = f
            Next
            If bOverlay Then
                For Each y As Double In Me.arGauss
                    If Not Double.IsNaN(y) AndAlso Not Double.IsInfinity(y) AndAlso y > dPositiveMax Then
                        dPositiveMax = y
                    End If
                Next
            End If

            Dim yScale As ChartAxisScaleResult = ChartAxisScalingCore.Compute(0.0R, dPositiveMax)
            Dim dYMax As Double = yScale.Max
            If dYMax <= 0.0R OrElse Double.IsNaN(dYMax) OrElse Double.IsInfinity(dYMax) Then dYMax = 1.0R
            Dim dYMajor As Double = yScale.Scale
            If dYMajor <= 0.0R OrElse Double.IsNaN(dYMajor) OrElse Double.IsInfinity(dYMajor) Then dYMajor = 1.0R

            Dim dYMin As Double = If(bDrawBoxPlot, -0.32R * dYMax, 0.0R)
            Dim dBoxY As Double = -0.18R * dYMax
            Dim dBoxHalfHeight As Double = 0.055R * dYMax

        Return {dXMin,dXMax,dYMin,dYMax,dYMajor,dBoxY,dBoxHalfHeight}
    End Function
End Class
Public Class LegacyAxis32
    Public XMinimum,XMaximum,YMinimum,YMaximum As Double
End Class
Public Class LegacyViolinAppearance32
    Public YAxisMinimum,YAxisMaximum As Double?
End Class
Public Class LegacyDomain32
    Private Shared Function IsFinite(x As Double) As Boolean
        Return Not Double.IsNaN(x) AndAlso Not Double.IsInfinity(x)
    End Function
    Public Shared Function ResolveLegacyAxis32(result As ViolinPlotResult,
                                               appearance As LegacyViolinAppearance32) As LegacyAxis32
        Dim axis As New LegacyAxis32 With {
            .XMinimum = 0.5R,
            .XMaximum = result.GroupCount + 0.5R
        }

        If appearance.YAxisMinimum.HasValue Then
            axis.YMinimum = appearance.YAxisMinimum.Value
        End If
        If appearance.YAxisMaximum.HasValue Then
            axis.YMaximum = appearance.YAxisMaximum.Value
        End If

        If Not appearance.YAxisMinimum.HasValue OrElse Not appearance.YAxisMaximum.HasValue Then
            Dim autoMinimum As Double
            Dim autoMaximum As Double
            ResolveAutomaticYLimits(result.YMinimum, result.YMaximum, autoMinimum, autoMaximum)
            If Not appearance.YAxisMinimum.HasValue Then axis.YMinimum = autoMinimum
            If Not appearance.YAxisMaximum.HasValue Then axis.YMaximum = autoMaximum
        End If

        If axis.YMinimum >= axis.YMaximum Then
            Throw New ArgumentException("Resolved Y-axis minimum must be smaller than its maximum.")
        End If
        Return axis
    End Function
    Public Shared Sub ResolveAutomaticYLimits(dataMinimum As Double,
                                               dataMaximum As Double,
                                               ByRef axisMinimum As Double,
                                               ByRef axisMaximum As Double)
        If IsFinite(dataMinimum) AndAlso IsFinite(dataMaximum) AndAlso dataMaximum > dataMinimum Then
            Dim scale As ChartAxisScaleResult = ChartAxisScalingCore.Compute(dataMinimum, dataMaximum)
            If scale IsNot Nothing AndAlso IsFinite(scale.Min) AndAlso IsFinite(scale.Max) AndAlso scale.Max > scale.Min Then
                axisMinimum = scale.Min
                axisMaximum = scale.Max
                Return
            End If
        End If

        Dim centre As Double = If(IsFinite(dataMinimum), dataMinimum, 0.0R)
        Dim halfRange As Double = Math.Max(1.0R, Math.Abs(centre) * 0.1R)
        axisMinimum = centre - halfRange
        axisMaximum = centre + halfRange
    End Sub
        Public Shared Function DeterministicJitter(index As Integer, halfWidth As Double) As Double
            If halfWidth <= 0.0R Then Return 0.0R
            Const GoldenFraction As Double = 0.6180339887498949R
            Dim u As Double = ((index + 1) * GoldenFraction) Mod 1.0R
            Return (2.0R * u - 1.0R) * halfWidth
        End Function
End Class
End Namespace
