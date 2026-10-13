Option Explicit On
Option Strict Off
Option Infer On

Imports System
Imports System.Collections.Generic
Imports System.Globalization
Imports BESHStatNG.AppInfrastructure
Imports Microsoft.Office.Interop.Excel

''' <summary>
''' Windows compatibility entry point for portable kite-chart geometry.
''' Result contracts and calculations now reside in BESHStat.Core.
''' Appearance, Excel chart construction and the UI remain in Framework.
''' </summary>
Public Module KiteChart
    Public Function Compute(values As Double(,),
                            Optional seriesNames As String() = Nothing,
                            Optional categoryLabels As Array = Nothing,
                            Optional options As KiteChartOptions = Nothing) As KiteChartResult
        Return KiteChartCore.Compute(values, seriesNames, categoryLabels, options)
    End Function
End Module

''' <summary>
''' Optional per-series Excel appearance override. Only populated properties replace
''' the corresponding chart defaults.
''' </summary>
Public Class KiteSeriesAppearance
    Public Property SeriesName As String
    Public Property FillColor As Nullable(Of Integer)
    Public Property FillTransparency As Nullable(Of Single)
    Public Property OutlineColor As Nullable(Of Integer)
    Public Property OutlineWeight As Nullable(Of Single)
    Public Property CenterLineColor As Nullable(Of Integer)
    Public Property CenterLineWeight As Nullable(Of Single)
    Public Property CenterLineStyle As Nullable(Of XlLineStyle)
End Class

''' <summary>
''' Excel chart appearance settings used by <see cref="KiteChartExcel"/>.
''' </summary>
''' <remarks>Colors are OLE RGB integers used by the Excel object model.</remarks>
Public Class KiteChartAppearance
    Public Property ChartTitle As String = "Kite chart"
    Public Property XAxisTitle As String = "Position"
    Public Property SeriesAxisTitle As String = String.Empty

    ''' <summary>
    ''' Shows one legend entry for each visible kite.
    ''' </summary>
    Public Property ShowLegend As Boolean = False

    ''' <summary>
    ''' Places each series name beside its centre line at the left of the plot.
    ''' </summary>
    Public Property ShowSeriesLabels As Boolean = True

    Public Property ShowCenterLines As Boolean = True
    Public Property ShowVerticalGridlines As Boolean = False
    Public Property ShowHorizontalGridlines As Boolean = False
    Public Property ShowValueAxisLabels As Boolean = False
    Public Property ShowOutline As Boolean = True

    Public Property FillTransparency As Single = 0.08F
    Public Property OutlineWeight As Single = 1.0F
    Public Property CenterLineWeight As Single = 0.75F
    Public Property CenterLineStyle As XlLineStyle = XlLineStyle.xlDot

    Public Property SeriesLabelFontSize As Single = 9.0F
    Public Property SeriesLabelBold As Boolean = True

    ''' <summary>
    ''' Gets or sets the category-label rotation in degrees from -90 to 90.
    ''' </summary>
    Public Property CategoryLabelRotation As Integer = 0

    Public Property LegendPosition As XlLegendPosition = XlLegendPosition.xlLegendPositionBottom

    Public Property SeriesColors As Integer() = {
        &HB4771F, &HE7FFF, &H2CA02C, &H2827D6, &HBD6794,
        &H4B568C, &HC277E3, &H7F7F7F, &H22BDBC, &HCFBE17
    }

    Public Property BackgroundColor As Integer = &HFFFFFF
    Public Property GridlineColor As Integer = &HE6E6E6
    Public Property OutlineColor As Nullable(Of Integer) = Nothing
    Public Property CenterLineColor As Integer = &H808080
    Public Property TextColor As Integer = &H333333

    ''' <summary>
    ''' Gets or sets optional per-series overrides matched case-insensitively by name.
    ''' </summary>
    Public Property SeriesOverrides As KiteSeriesAppearance() = New KiteSeriesAppearance() {}
End Class

''' <summary>
''' Renders a <see cref="KiteChartResult"/> as an embedded Excel area chart.
''' </summary>
''' <remarks>
''' Every kite is drawn as a visible upper area followed by an opaque background-colored
''' lower area. This creates a filled band between the two symmetric boundaries without
''' writing helper columns to the worksheet. Pairs are ordered from the top lane to the
''' bottom lane so the masks do not interfere when the default non-overlapping geometry
''' is used.
''' </remarks>
Public NotInheritable Class KiteChartExcel
    Private Sub New()
    End Sub

    Private NotInheritable Class ResolvedSeriesStyle
        Friend FillColor As Integer
        Friend FillTransparency As Single
        Friend OutlineColor As Integer
        Friend OutlineWeight As Single
        Friend CenterLineColor As Integer
        Friend CenterLineWeight As Single
        Friend CenterLineStyle As XlLineStyle
    End Class

    ''' <summary>
    ''' Creates an embedded kite chart on the supplied worksheet.
    ''' </summary>
    Public Shared Function AddChart(ws As Worksheet,
                                    result As KiteChartResult,
                                    Optional appearance As KiteChartAppearance = Nothing,
                                    Optional left As Double = 20.0R,
                                    Optional top As Double = 20.0R,
                                    Optional width As Double = 720.0R,
                                    Optional height As Double = 440.0R) As Chart
        If ws Is Nothing Then Throw New ArgumentNullException(NameOf(ws))
        If result Is Nothing Then Throw New ArgumentNullException(NameOf(result))
        If Not IsFinite(left) OrElse Not IsFinite(top) Then
            Throw New ArgumentOutOfRangeException(NameOf(left), "Chart position must be finite.")
        End If
        If Not IsFinitePositive(width) OrElse Not IsFinitePositive(height) Then
            Throw New ArgumentOutOfRangeException(NameOf(width),
                                                  "Chart width and height must be finite and positive.")
        End If

        Dim resolvedAppearance As KiteChartAppearance = If(appearance, New KiteChartAppearance())
        ValidateAppearance(resolvedAppearance)

        Dim chartShape As Shape = Nothing
        Try
            chartShape = ws.Shapes.AddChart(XlChartType.xlArea, left, top, width, height)
            Dim chart As Chart = chartShape.Chart
            chart.ChartType = XlChartType.xlArea
            chart.DisplayBlanksAs = XlDisplayBlanksAs.xlNotPlotted
            chart.PlotVisibleOnly = False
            chart.ChartArea.AutoScaleFont = False

            Dim seriesCollection As SeriesCollection = DirectCast(chart.SeriesCollection(), SeriesCollection)
            DeleteAllSeries(seriesCollection)
            ConfigureBackground(chart, resolvedAppearance)
            ConfigureTitle(chart, resolvedAppearance.ChartTitle)

            Dim legendSeriesIndices As New List(Of Integer)()
            Dim categories As Object() = result.Categories
            Dim kiteSeries As KiteChartSeries() = result.Series

            For seriesIndex As Integer = 0 To kiteSeries.Length - 1
                Dim kite As KiteChartSeries = kiteSeries(seriesIndex)
                Dim style As ResolvedSeriesStyle = ResolveStyle(kite.Name,
                                                                seriesIndex,
                                                                resolvedAppearance)

                Dim visibleSeriesIndex As Integer = AddAreaSeries(seriesCollection,
                                                                  kite.Name,
                                                                  categories,
                                                                  kite.UpperBoundary,
                                                                  style.FillColor,
                                                                  style.FillTransparency,
                                                                  resolvedAppearance.ShowOutline,
                                                                  style.OutlineColor,
                                                                  style.OutlineWeight)
                legendSeriesIndices.Add(visibleSeriesIndex)

                AddMaskSeries(seriesCollection,
                              kite.Name & " mask",
                              categories,
                              kite.LowerBoundary,
                              resolvedAppearance.BackgroundColor)

                If resolvedAppearance.ShowOutline Then
                    AddBoundaryLineSeries(seriesCollection,
                                          kite.Name & " lower outline",
                                          categories,
                                          kite.LowerBoundary,
                                          style.OutlineColor,
                                          style.OutlineWeight)
                End If

                If resolvedAppearance.ShowCenterLines Then
                    AddCenterLineSeries(seriesCollection,
                                        kite.Name & " centre line",
                                        categories,
                                        kite.CenterLineValues,
                                        style.CenterLineColor,
                                        style.CenterLineWeight,
                                        style.CenterLineStyle)
                End If

                If resolvedAppearance.ShowSeriesLabels Then
                    AddSeriesLabel(seriesCollection,
                                   kite.Name,
                                   categories,
                                   kite.CenterLine,
                                   style.FillColor,
                                   resolvedAppearance)
                End If
            Next

            ConfigureAxes(chart, result, resolvedAppearance)
            ConfigureLegend(chart,
                            legendSeriesIndices,
                            resolvedAppearance.ShowLegend,
                            resolvedAppearance.LegendPosition)
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

    Private Shared Sub ValidateAppearance(appearance As KiteChartAppearance)
        If appearance.SeriesColors Is Nothing OrElse appearance.SeriesColors.Length = 0 Then
            Throw New ArgumentException("SeriesColors must contain at least one color.",
                                        NameOf(appearance.SeriesColors))
        End If
        If Not IsFiniteFraction(appearance.FillTransparency) Then
            Throw New ArgumentOutOfRangeException(NameOf(appearance.FillTransparency),
                                                  "Fill transparency must be between zero and one.")
        End If
        If Not IsFinitePositive(appearance.OutlineWeight) Then
            Throw New ArgumentOutOfRangeException(NameOf(appearance.OutlineWeight),
                                                  "Outline weight must be finite and positive.")
        End If
        If Not IsFinitePositive(appearance.CenterLineWeight) Then
            Throw New ArgumentOutOfRangeException(NameOf(appearance.CenterLineWeight),
                                                  "Centre-line weight must be finite and positive.")
        End If
        If appearance.CenterLineStyle = XlLineStyle.xlLineStyleNone AndAlso appearance.ShowCenterLines Then
            Throw New ArgumentException("CenterLineStyle must be visible when ShowCenterLines is enabled.",
                                        NameOf(appearance.CenterLineStyle))
        End If
        If Not IsFinitePositive(appearance.SeriesLabelFontSize) Then
            Throw New ArgumentOutOfRangeException(NameOf(appearance.SeriesLabelFontSize),
                                                  "Series-label font size must be finite and positive.")
        End If
        If appearance.CategoryLabelRotation < -90 OrElse appearance.CategoryLabelRotation > 90 Then
            Throw New ArgumentOutOfRangeException(NameOf(appearance.CategoryLabelRotation),
                                                  "Category-label rotation must be between -90 and 90 degrees.")
        End If

        If appearance.SeriesOverrides IsNot Nothing Then
            For Each item As KiteSeriesAppearance In appearance.SeriesOverrides
                If item Is Nothing Then Continue For
                If item.FillTransparency.HasValue AndAlso Not IsFiniteFraction(item.FillTransparency.Value) Then
                    Throw New ArgumentOutOfRangeException(NameOf(item.FillTransparency),
                                                          "Series fill transparency must be between zero and one.")
                End If
                If item.OutlineWeight.HasValue AndAlso Not IsFinitePositive(item.OutlineWeight.Value) Then
                    Throw New ArgumentOutOfRangeException(NameOf(item.OutlineWeight),
                                                          "Series outline weight must be finite and positive.")
                End If
                If item.CenterLineWeight.HasValue AndAlso Not IsFinitePositive(item.CenterLineWeight.Value) Then
                    Throw New ArgumentOutOfRangeException(NameOf(item.CenterLineWeight),
                                                          "Series centre-line weight must be finite and positive.")
                End If
                If item.CenterLineStyle.HasValue AndAlso
                   appearance.ShowCenterLines AndAlso
                   item.CenterLineStyle.Value = XlLineStyle.xlLineStyleNone Then
                    Throw New ArgumentException("A series centre-line style must be visible when centre lines are enabled.",
                                                NameOf(item.CenterLineStyle))
                End If
            Next
        End If
    End Sub

    Private Shared Sub DeleteAllSeries(seriesCollection As SeriesCollection)
        Do While seriesCollection.Count > 0
            DirectCast(seriesCollection.Item(1), Series).Delete()
        Loop
    End Sub

    Private Shared Function AddAreaSeries(seriesCollection As SeriesCollection,
                                          seriesName As String,
                                          categories As Object(),
                                          values As Double(),
                                          fillColor As Integer,
                                          fillTransparency As Single,
                                          showOutline As Boolean,
                                          outlineColor As Integer,
                                          outlineWeight As Single) As Integer
        'Dim series As Series = DirectCast(seriesCollection.NewSeries(), Series)
        seriesCollection.NewSeries()
        With seriesCollection(seriesCollection.Count - 1)
            .Name = seriesName
            .ChartType = XlChartType.xlArea
            .XValues = categories
            .Values = ToChartValues(values)
            .Format.Fill.Visible = True
            .Format.Fill.Solid()
            .Format.Fill.ForeColor.RGB = fillColor
            .Format.Fill.Transparency = fillTransparency

            If showOutline Then
                .Format.Line.Visible = True
                .Format.Line.ForeColor.RGB = outlineColor
                .Format.Line.Weight = outlineWeight
                .Border.Color = outlineColor
            Else
                .Format.Line.Visible = False
                .Border.LineStyle = XlLineStyle.xlLineStyleNone
            End If
        End With

        Return seriesCollection.Count
    End Function

    Private Shared Sub AddMaskSeries(seriesCollection As SeriesCollection,
                                     seriesName As String,
                                     categories As Object(),
                                     values As Double(),
                                     backgroundColor As Integer)
        'Dim series As Series = DirectCast(seriesCollection.NewSeries(), Series)
        seriesCollection.NewSeries()
        With seriesCollection(seriesCollection.Count - 1)
            .Name = seriesName
            .ChartType = XlChartType.xlArea
            .XValues = categories
            .Values = ToChartValues(values)
            .Format.Fill.Visible = True
            .Format.Fill.Solid()
            .Format.Fill.ForeColor.RGB = backgroundColor
            .Format.Fill.Transparency = 0.0F
            .Format.Line.Visible = False
            .Border.LineStyle = XlLineStyle.xlLineStyleNone
        End With
    End Sub


    Private Shared Sub AddBoundaryLineSeries(seriesCollection As SeriesCollection,
                                             seriesName As String,
                                             categories As Object(),
                                             values As Double(),
                                             color As Integer,
                                             weight As Single)
        'Dim series As Series = DirectCast(seriesCollection.NewSeries(), Series)
        seriesCollection.NewSeries()
        With seriesCollection(seriesCollection.Count - 1)
            .Name = seriesName
            .ChartType = XlChartType.xlLine
            .XValues = categories
            .Values = ToChartValues(values)
            .MarkerStyle = XlMarkerStyle.xlMarkerStyleNone
            .Format.Fill.Visible = False
            .Format.Line.Visible = True
            .Format.Line.ForeColor.RGB = color
            .Format.Line.Weight = weight
            .Border.Color = color
            .Border.LineStyle = XlLineStyle.xlContinuous
        End With
    End Sub

    Private Shared Sub AddCenterLineSeries(seriesCollection As SeriesCollection,
                                           seriesName As String,
                                           categories As Object(),
                                           values As Double(),
                                           color As Integer,
                                           weight As Single,
                                           lineStyle As XlLineStyle)
        'Dim series As Series = DirectCast(seriesCollection.NewSeries(), Series)
        seriesCollection.NewSeries()
        With seriesCollection(seriesCollection.Count - 1)
            .Name = seriesName
            .ChartType = XlChartType.xlLine
            .XValues = categories
            .Values = ToChartValues(values)
            .MarkerStyle = XlMarkerStyle.xlMarkerStyleNone
            .Format.Fill.Visible = False
            .Format.Line.Visible = True
            .Format.Line.ForeColor.RGB = color
            .Format.Line.Weight = weight
            .Border.Color = color
            .Border.LineStyle = lineStyle
        End With
    End Sub

    Private Shared Sub AddSeriesLabel(seriesCollection As SeriesCollection,
                                      seriesName As String,
                                      categories As Object(),
                                      centerLine As Double,
                                      color As Integer,
                                      appearance As KiteChartAppearance)
        'A label helper needs only one plotted point. Using a full-length array
        'with #N/A placeholders causes DISP_E_TYPEMISMATCH when assigned through
        'the Excel Interop Series.Values property.
        Dim labelCategories(0) As Object
        labelCategories(0) = categories(0)

        Dim labelValues(0) As Double
        labelValues(0) = centerLine

        'Dim series As Series = DirectCast(seriesCollection.NewSeries(), Series)
        seriesCollection.NewSeries()
        With seriesCollection(seriesCollection.Count - 1)
            .Name = seriesName & " label"
            .ChartType = XlChartType.xlLine
            .XValues = labelCategories
            .Values = labelValues
            .MarkerStyle = XlMarkerStyle.xlMarkerStyleNone
            .Format.Fill.Visible = False
            .Format.Line.Visible = False
            .Border.LineStyle = XlLineStyle.xlLineStyleNone

            Try
                .ApplyDataLabels()
                Dim point As Point = DirectCast(.Points(1), Point)
                point.DataLabel.Text = seriesName
                point.DataLabel.Position = XlDataLabelPosition.xlLabelPositionAbove ' XlDataLabelPosition.xlLabelPositionLeft
                point.DataLabel.Font.Size = appearance.SeriesLabelFontSize
                point.DataLabel.Font.Bold = appearance.SeriesLabelBold
                point.DataLabel.Font.Color = If(color = appearance.BackgroundColor,
                                            appearance.TextColor,
                                            color)
            Catch
                'Series labels are helpful but should never prevent chart creation.
            End Try
        End With
    End Sub

    Private Shared Function ToChartValues(values As Double()) As Object()
        Dim result(values.Length - 1) As Object
        For i As Integer = 0 To values.Length - 1
            If IsFinite(values(i)) Then
                result(i) = values(i)
            Else
                'Nothing marshals as a blank VARIANT and is accepted by Series.Values.
                'Chart.DisplayBlanksAs controls whether the point is plotted.
                result(i) = Nothing
            End If
        Next
        Return result
    End Function

    Private Shared Function ResolveStyle(seriesName As String,
                                         seriesIndex As Integer,
                                         appearance As KiteChartAppearance) As ResolvedSeriesStyle
        Dim fillColor As Integer = appearance.SeriesColors(seriesIndex Mod appearance.SeriesColors.Length)
        Dim result As New ResolvedSeriesStyle With {
            .FillColor = fillColor,
            .FillTransparency = appearance.FillTransparency,
            .OutlineColor = If(appearance.OutlineColor.HasValue,
                               appearance.OutlineColor.Value,
                               fillColor),
            .OutlineWeight = appearance.OutlineWeight,
            .CenterLineColor = appearance.CenterLineColor,
            .CenterLineWeight = appearance.CenterLineWeight,
            .CenterLineStyle = appearance.CenterLineStyle
        }

        Dim overrideStyle As KiteSeriesAppearance = FindOverride(seriesName, appearance.SeriesOverrides)
        If overrideStyle IsNot Nothing Then
            If overrideStyle.FillColor.HasValue Then result.FillColor = overrideStyle.FillColor.Value
            If overrideStyle.FillTransparency.HasValue Then result.FillTransparency = overrideStyle.FillTransparency.Value
            If overrideStyle.OutlineColor.HasValue Then result.OutlineColor = overrideStyle.OutlineColor.Value
            If overrideStyle.OutlineWeight.HasValue Then result.OutlineWeight = overrideStyle.OutlineWeight.Value
            If overrideStyle.CenterLineColor.HasValue Then result.CenterLineColor = overrideStyle.CenterLineColor.Value
            If overrideStyle.CenterLineWeight.HasValue Then result.CenterLineWeight = overrideStyle.CenterLineWeight.Value
            If overrideStyle.CenterLineStyle.HasValue Then result.CenterLineStyle = overrideStyle.CenterLineStyle.Value
        End If
        Return result
    End Function

    Private Shared Function FindOverride(seriesName As String,
                                         ovrrides As KiteSeriesAppearance()) As KiteSeriesAppearance
        If ovrrides Is Nothing Then Return Nothing
        For Each item As KiteSeriesAppearance In ovrrides
            If item IsNot Nothing AndAlso
               Not String.IsNullOrWhiteSpace(item.SeriesName) AndAlso
               String.Equals(item.SeriesName.Trim(),
                             seriesName,
                             StringComparison.CurrentCultureIgnoreCase) Then
                Return item
            End If
        Next
        Return Nothing
    End Function

    Private Shared Sub ConfigureTitle(chart As Chart, title As String)
        If String.IsNullOrWhiteSpace(title) Then
            chart.HasTitle = False
        Else
            chart.HasTitle = True
            chart.ChartTitle.Text = title.Trim()
        End If
    End Sub

    Private Shared Sub ConfigureBackground(chart As Object, appearance As KiteChartAppearance)
        chart.ChartArea.Format.Fill.Visible = True
        chart.ChartArea.Format.Fill.Solid()
        chart.ChartArea.Format.Fill.ForeColor.RGB = appearance.BackgroundColor
        chart.PlotArea.Format.Fill.Visible = True
        chart.PlotArea.Format.Fill.Solid()
        chart.PlotArea.Format.Fill.ForeColor.RGB = appearance.BackgroundColor
    End Sub

    Private Shared Sub ConfigureAxes(chart As Chart,
                                     result As KiteChartResult,
                                     appearance As KiteChartAppearance)
        Dim categoryAxis As Axis = DirectCast(chart.Axes(XlAxisType.xlCategory,
                                                         XlAxisGroup.xlPrimary), Axis)
        categoryAxis.HasTitle = Not String.IsNullOrWhiteSpace(appearance.XAxisTitle)
        If categoryAxis.HasTitle Then categoryAxis.AxisTitle.Text = appearance.XAxisTitle.Trim()
        categoryAxis.AxisBetweenCategories = False
        categoryAxis.HasMajorGridlines = appearance.ShowVerticalGridlines
        categoryAxis.TickLabels.Orientation = appearance.CategoryLabelRotation
        If appearance.ShowVerticalGridlines Then
            Try
                categoryAxis.MajorGridlines.Border.Color = appearance.GridlineColor
            Catch
            End Try
        End If

        Dim valueAxis As Axis = DirectCast(chart.Axes(XlAxisType.xlValue,
                                                      XlAxisGroup.xlPrimary), Axis)
        valueAxis.MinimumScale = result.AxisMinimum
        valueAxis.MaximumScale = result.AxisMaximum
        valueAxis.MajorUnit = result.Options.LaneSpacing
        valueAxis.HasTitle = Not String.IsNullOrWhiteSpace(appearance.SeriesAxisTitle)
        If valueAxis.HasTitle Then valueAxis.AxisTitle.Text = appearance.SeriesAxisTitle.Trim()
        valueAxis.HasMajorGridlines = appearance.ShowHorizontalGridlines
        valueAxis.TickLabelPosition = If(appearance.ShowValueAxisLabels,
                                         XlTickLabelPosition.xlTickLabelPositionNextToAxis,
                                         XlTickLabelPosition.xlTickLabelPositionNone)
        If appearance.ShowHorizontalGridlines Then
            Try
                valueAxis.MajorGridlines.Border.Color = appearance.GridlineColor
            Catch
            End Try
        End If
    End Sub

    Private Shared Sub ConfigureLegend(chart As Chart,
                                       keepSeriesIndices As IList(Of Integer),
                                       showLegend As Boolean,
                                       legendPosition As XlLegendPosition)
        If Not showLegend OrElse keepSeriesIndices Is Nothing OrElse keepSeriesIndices.Count = 0 Then
            chart.HasLegend = False
            Return
        End If

        chart.HasLegend = True
        chart.Legend.Position = legendPosition
        Dim keep As New HashSet(Of Integer)(keepSeriesIndices)
        Dim entries As LegendEntries = DirectCast(chart.Legend.LegendEntries(), LegendEntries)
        For i As Integer = entries.Count To 1 Step -1
            If Not keep.Contains(i) Then
                DirectCast(entries.Item(i), LegendEntry).Delete()
            End If
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

    Private Shared Function IsFiniteFraction(value As Single) As Boolean
        Return Not Single.IsNaN(value) AndAlso
               Not Single.IsInfinity(value) AndAlso
               value >= 0.0F AndAlso value <= 1.0F
    End Function
End Class
