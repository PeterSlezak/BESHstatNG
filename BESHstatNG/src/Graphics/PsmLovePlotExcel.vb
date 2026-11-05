Option Explicit On

Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports Microsoft.Office.Interop.Excel
Imports BESHStatNG.CausalInference
Imports BESHStatNG.ExcelCommon

Namespace graphics

    ''' <summary>
    ''' Excel chart helper for propensity-score-matching Love plots.
    ''' </summary>
    ''' <remarks>
    ''' The GUI writes the source table using the standard ResultTable / ExcelDnaResultWriter
    ''' pipeline, then calls this helper to add an embedded Excel scatter chart on the
    ''' same worksheet.  Keeping the chart helper separate from the PSM backend avoids
    ''' adding Excel COM dependencies to backend classes.
    ''' </remarks>
    Public NotInheritable Class PsmLovePlotExcel
        Private Sub New()
        End Sub

        ''' <summary>
        ''' Builds a compact chart-source table with absolute standardized mean
        ''' differences.  The first row is the header row expected by ResultTable.
        ''' </summary>
        Public Shared Function BuildPlotDataTable(rows As IList(Of PsmLovePlotRow)) As Object(,)
            Return BuildProjectedDataTable(New PsmLovePlotProjection(rows))
        End Function

        Public Shared Function BuildProjectedDataTable(projection As PsmLovePlotProjection) As Object(,)
            Return projection.BuildDataTable()
        End Function

        ''' <summary>
        ''' Adds a Love plot chart to an existing worksheet.  The chart is built from
        ''' the supplied rows, so it does not depend on exact cell locations produced
        ''' by ExcelDnaResultWriter formatting.
        ''' </summary>
        Public Shared Sub AddChart(ws As Worksheet,
                                   rows As IList(Of PsmLovePlotRow),
                                   Optional left As Double = 420,
                                   Optional top As Double = 20,
                                   Optional width As Double = 720,
                                   Optional height As Double = 420)
            If ws Is Nothing OrElse rows Is Nothing OrElse rows.Count = 0 Then Return

            AddProjectedChart(ws, New PsmLovePlotProjection(rows), left, top, width, height)
        End Sub

        Public Shared Sub AddProjectedChart(ws As Worksheet, projection As PsmLovePlotProjection,
                                            Optional left As Double = 420, Optional top As Double = 20,
                                            Optional width As Double = 720, Optional height As Double = 420)
            If ws Is Nothing OrElse projection Is Nothing OrElse projection.Count = 0 Then Return
            Dim yValues = projection.YValues
            Dim xBefore = projection.Before
            Dim xAfterMatching = projection.AfterMatching
            Dim xAfterWeighting = projection.AfterWeighting
            Dim labels = projection.Labels
            Dim threshold = projection.Threshold
            Dim maxX = projection.MaximumX

            Dim shp As Shape = ws.Shapes.AddChart(Left:=left, Top:=top, Width:=width, Height:=height)
            Dim ch As Chart = shp.Chart
            With ch
                .ChartType = XlChartType.xlXYScatter

                Do Until .SeriesCollection.Count = 0
                    .SeriesCollection(1).Delete()
                Loop

                Dim plottedSeries As New List(Of Integer)()
                If HasAnyFinite(xBefore) Then plottedSeries.Add(AddScatterSeries(ch, "Before", xBefore, yValues, RGB(120, 120, 120), XlMarkerStyle.xlMarkerStyleCircle))
                If HasAnyFinite(xAfterMatching) Then plottedSeries.Add(AddScatterSeries(ch, "After matching", xAfterMatching, yValues, RGB(31, 119, 180), XlMarkerStyle.xlMarkerStyleDiamond))
                If HasAnyFinite(xAfterWeighting) Then plottedSeries.Add(AddScatterSeries(ch, "After weighting", xAfterWeighting, yValues, RGB(44, 160, 44), XlMarkerStyle.xlMarkerStyleSquare))

                AddThresholdSeries(ch, threshold, projection.Count)

                If plottedSeries.Count > 0 Then
                    AddVariableLabels(CType(.SeriesCollection(plottedSeries(plottedSeries.Count - 1)), Series), labels)
                End If

                .HasTitle = True
                .ChartTitle.Text = "Love plot: absolute standardized mean differences"
                .HasLegend = True
                .Legend.Position = XlLegendPosition.xlLegendPositionBottom

                With .Axes(XlAxisType.xlCategory)
                    .MinimumScale = 0
                    .MaximumScale = maxX
                    .MajorUnitIsAuto = True
                    .HasTitle = True
                    .AxisTitle.Text = "Absolute standardized mean difference"
                    Try
                        .MajorGridlines.Border.Color = RGB(230, 230, 230)
                    Catch
                    End Try
                End With

                With .Axes(XlAxisType.xlValue)
                    .MinimumScale = 0.5R
                    .MaximumScale = projection.Count + 0.5R
                    .MajorUnit = 1
                    .ReversePlotOrder = True
                    .HasTitle = False
                    Try
                        .TickLabelPosition = XlTickLabelPosition.xlTickLabelPositionNone
                    Catch
                    End Try
                    Try
                        .MajorGridlines.Delete()
                    Catch
                    End Try
                End With
            End With
        End Sub

        Private Shared Function AddScatterSeries(ch As Chart,
                                                 name As String,
                                                 xValues As Double(),
                                                 yValues As Double(),
                                                 color As Integer,
                                                 markerStyle As XlMarkerStyle) As Integer
            ch.SeriesCollection.NewSeries()
            Dim index As Integer = ch.SeriesCollection.Count
            With ch.SeriesCollection(index)
                .Name = name
                .XValues = xValues
                .Values = yValues
                .ChartType = XlChartType.xlXYScatter
                .MarkerStyle = markerStyle
                .MarkerSize = 6
                .MarkerForegroundColor = color
                .MarkerBackgroundColor = color
                .Format.Line.Visible = False
            End With
            Return index
        End Function

        Private Shared Sub AddThresholdSeries(ch As Chart, threshold As Double, n As Integer)
            ch.SeriesCollection.NewSeries()
            Dim s As Series = CType(ch.SeriesCollection(ch.SeriesCollection.Count), Series)
            With s
                .Name = "Threshold"
                .XValues = New Double() {threshold, threshold}
                .Values = New Double() {0.5R, n + 0.5R}
                .ChartType = XlChartType.xlXYScatterLinesNoMarkers
                Try
                    .Border.Color = RGB(180, 0, 0)
                    .Border.Weight = XlBorderWeight.xlMedium
                Catch
                End Try
            End With
        End Sub

        Private Shared Sub AddVariableLabels(series As Series, labels As String())
            If series Is Nothing OrElse labels Is Nothing OrElse labels.Length = 0 Then Return
            Try
                series.ApplyDataLabels()
                For i As Integer = 1 To Math.Min(labels.Length, series.Points.Count)
                    Dim pt As Point = CType(series.Points(i), Point)
                    pt.DataLabel.Text = labels(i - 1)
                    pt.DataLabel.Position = XlDataLabelPosition.xlLabelPositionRight
                    pt.DataLabel.Font.Size = 8
                Next
            Catch
                'Data labels are helpful but not essential; keep chart creation robust.
            End Try
        End Sub





        Private Shared Function HasAnyFinite(values As IEnumerable(Of Double)) As Boolean
            If values Is Nothing Then Return False
            Return values.Any(Function(v) AppInfrastructure.IsFinite(v))
        End Function

    End Class

End Namespace
