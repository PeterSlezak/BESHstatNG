Option Explicit On

Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports Microsoft.Office.Interop.Excel
Imports BESHStatNG.CausalInference

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
            If rows Is Nothing OrElse rows.Count = 0 Then Return PsmResult.EmptyTable("No Love plot data available")

            Dim cleanRows As List(Of PsmLovePlotRow) = rows.
                Where(Function(r) r IsNot Nothing AndAlso Not String.IsNullOrWhiteSpace(r.VariableName)).
                OrderByDescending(Function(r) MaxFiniteAbs(r.SmdBefore, r.SmdAfterMatching, r.SmdAfterWeighting)).
                ThenBy(Function(r) r.VariableName, StringComparer.OrdinalIgnoreCase).
                ToList()

            If cleanRows.Count = 0 Then Return PsmResult.EmptyTable("No Love plot data available")

            Dim table(cleanRows.Count, 6) As Object
            table(0, 0) = "Plot Row"
            table(0, 1) = "Variable"
            table(0, 2) = "|SMD| before"
            table(0, 3) = "|SMD| after matching"
            table(0, 4) = "|SMD| after weighting"
            table(0, 5) = "Threshold"
            table(0, 6) = "Flag"

            For i As Integer = 0 To cleanRows.Count - 1
                Dim r As PsmLovePlotRow = cleanRows(i)
                table(i + 1, 0) = i + 1
                table(i + 1, 1) = r.VariableName
                table(i + 1, 2) = AbsOrMissing(r.SmdBefore)
                table(i + 1, 3) = AbsOrMissing(r.SmdAfterMatching)
                table(i + 1, 4) = AbsOrMissing(r.SmdAfterWeighting)
                table(i + 1, 5) = If(AppInfrastructure.IsFinite(r.Threshold), r.Threshold, Nothing)
                table(i + 1, 6) = r.Flag
            Next

            Return table
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

            Dim cleanRows As List(Of PsmLovePlotRow) = rows.
                Where(Function(r) r IsNot Nothing AndAlso Not String.IsNullOrWhiteSpace(r.VariableName)).
                OrderByDescending(Function(r) MaxFiniteAbs(r.SmdBefore, r.SmdAfterMatching, r.SmdAfterWeighting)).
                ThenBy(Function(r) r.VariableName, StringComparer.OrdinalIgnoreCase).
                ToList()

            If cleanRows.Count = 0 Then Return

            Dim yValues As Double() = Enumerable.Range(1, cleanRows.Count).Select(Function(i) CDbl(i)).ToArray()
            Dim xBefore As Double() = cleanRows.Select(Function(r) AbsForChart(r.SmdBefore)).ToArray()
            Dim xAfterMatching As Double() = cleanRows.Select(Function(r) AbsForChart(r.SmdAfterMatching)).ToArray()
            Dim xAfterWeighting As Double() = cleanRows.Select(Function(r) AbsForChart(r.SmdAfterWeighting)).ToArray()
            Dim labels As String() = cleanRows.Select(Function(r) r.VariableName).ToArray()
            Dim threshold As Double = cleanRows.Select(Function(r) r.Threshold).FirstOrDefault(Function(v) AppInfrastructure.IsFinite(v) AndAlso v > 0)
            If Not AppInfrastructure.IsFinite(threshold) OrElse threshold <= 0 Then threshold = 0.1R

            Dim maxX As Double = Math.Max(threshold * 1.25R, MaxFinite(xBefore.Concat(xAfterMatching).Concat(xAfterWeighting)) * 1.15R)
            If Not AppInfrastructure.IsFinite(maxX) OrElse maxX <= 0 Then maxX = 0.25R
            If maxX < 0.25R Then maxX = 0.25R

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

                AddThresholdSeries(ch, threshold, cleanRows.Count)

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
                    .MaximumScale = cleanRows.Count + 0.5R
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

        Private Shared Function AbsForChart(value As Double) As Double
            If Not AppInfrastructure.IsFinite(value) Then Return Double.NaN
            Return Math.Abs(value)
        End Function

        Private Shared Function AbsOrMissing(value As Double) As Object
            If Not AppInfrastructure.IsFinite(value) Then Return Nothing
            Return Math.Abs(value)
        End Function

        Private Shared Function MaxFiniteAbs(ParamArray values As Double()) As Double
            Dim maxValue As Double = Double.NaN
            If values Is Nothing Then Return maxValue
            For Each v As Double In values
                If AppInfrastructure.IsFinite(v) Then
                    Dim av As Double = Math.Abs(v)
                    If Not AppInfrastructure.IsFinite(maxValue) OrElse av > maxValue Then maxValue = av
                End If
            Next
            Return maxValue
        End Function

        Private Shared Function MaxFinite(values As IEnumerable(Of Double)) As Double
            Dim maxValue As Double = Double.NaN
            If values Is Nothing Then Return maxValue
            For Each v As Double In values
                If AppInfrastructure.IsFinite(v) Then
                    If Not AppInfrastructure.IsFinite(maxValue) OrElse v > maxValue Then maxValue = v
                End If
            Next
            Return maxValue
        End Function

        Private Shared Function HasAnyFinite(values As IEnumerable(Of Double)) As Boolean
            If values Is Nothing Then Return False
            Return values.Any(Function(v) AppInfrastructure.IsFinite(v))
        End Function

    End Class

End Namespace
