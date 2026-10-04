Option Explicit On

Imports System.Runtime.CompilerServices
Imports System.Linq
Imports Microsoft.Office.Interop.Excel

''' <summary>
''' Windows-host renderers for agreement plots. Statistical calculations and neutral plot payloads live in BESHStat.Core.
''' </summary>
Public Module AgreementPlotExcelCompatibility

    <Extension()>
    Public Sub AddPlot(analysis As Agreement.PassinbBablok, ws As Worksheet)
        If analysis Is Nothing Then Throw New ArgumentNullException(NameOf(analysis))
        If ws Is Nothing Then Throw New ArgumentNullException(NameOf(ws))

        Dim plotData As Agreement.PassingBablokPlotData = analysis.GetPlotData()
        Dim ch = graphics.GeneralScatterPlot(plotData.XValues, plotData.YValues, plotData.YName, plotData.XName, ws, plotData.Title)

        With ch
            .HasLegend = True

            .SeriesCollection.NewSeries()
            With .SeriesCollection(2)
                .XValues = {plotData.MinX, plotData.MaxX}
                .Values = {plotData.Intercept + plotData.Slope * plotData.MinX,
                           plotData.Intercept + plotData.Slope * plotData.MaxX}
                .Name = "Regression line"
                .MarkerStyle = -4142
                .Border.Color = RGB(255, 0, 0)
                With .Format.Line
                    .Visible = True
                    .Weight = 1.5
                End With
            End With

            .SeriesCollection.NewSeries()
            With .SeriesCollection(3)
                .XValues = {plotData.MinX, plotData.MaxX}
                .Values = {plotData.MinX, plotData.MaxX}
                .Name = "Unity line (y = x)"
                .MarkerStyle = -4142
                .Border.Color = RGB(0, 0, 255)
                With .Format.Line
                    .Visible = True
                    .DashStyle = 4
                    .Weight = 0.5
                End With
            End With
        End With
    End Sub

    <Extension()>
    Public Sub AddPlot(analysis As Agreement.BlandAltmanAgreement, ws As Worksheet)
        If analysis Is Nothing Then Throw New ArgumentNullException(NameOf(analysis))
        If ws Is Nothing Then Throw New ArgumentNullException(NameOf(ws))

        Dim plotData As Agreement.BlandAltmanPlotData = analysis.GetPlotData()
        Dim ch As Chart = graphics.GeneralScatterPlot(plotData.XValues, plotData.YValues, plotData.YLabel, plotData.XLabel, ws, plotData.Title)
        Dim xMin As Double = plotData.XValues.Min()
        Dim xMax As Double = plotData.XValues.Max()
        If xMin = xMax Then
            xMin -= 0.5
            xMax += 0.5
        End If

        Dim lineX As Double() = {xMin, xMax}
        AddHorizontalReferenceLine(ch, lineX, plotData.Bias, "Bias", RGB(31, 119, 180))
        AddHorizontalReferenceLine(ch, lineX, plotData.LowerLoA, "Lower LoA", RGB(214, 39, 40))
        AddHorizontalReferenceLine(ch, lineX, plotData.UpperLoA, "Upper LoA", RGB(214, 39, 40))

        If plotData.ShowSubjectMeans AndAlso plotData.SubjectMeanX IsNot Nothing AndAlso plotData.SubjectMeanY IsNot Nothing Then
            ch.SeriesCollection.NewSeries()
            With ch.SeriesCollection(ch.SeriesCollection.Count)
                .XValues = plotData.SubjectMeanX
                .Values = plotData.SubjectMeanY
                .Name = "Subject means"
                .MarkerStyle = XlMarkerStyle.xlMarkerStyleDiamond
                .MarkerSize = 7
                .Format.Line.Visible = False
            End With
        End If
    End Sub

    <Extension()>
    Public Function AddPlot(analysis As Agreement.LinConcordanceCorrelation,
                            ws As Worksheet,
                            Optional chartTitle As String = "Lin concordance plot") As Chart
        If analysis Is Nothing Then Throw New ArgumentNullException(NameOf(analysis))
        If ws Is Nothing Then Throw New ArgumentNullException(NameOf(ws))

        Dim plotData As Agreement.LinConcordancePlotData = analysis.GetPlotData(chartTitle)
        Dim chartObj As Chart = graphics.GeneralScatterPlot(plotData.XValues, plotData.YValues, plotData.YName, plotData.XName, ws, plotData.Title)

        With chartObj
            .Axes(XlAxisType.xlCategory).MinimumScale = plotData.MinValue
            .Axes(XlAxisType.xlCategory).MaximumScale = plotData.MaxValue
            .Axes(XlAxisType.xlValue).MinimumScale = plotData.MinValue
            .Axes(XlAxisType.xlValue).MaximumScale = plotData.MaxValue

            .SeriesCollection.NewSeries()
            With .SeriesCollection(.SeriesCollection.Count)
                .Name = "Identity"
                .XValues = New Double() {plotData.MinValue, plotData.MaxValue}
                .Values = New Double() {plotData.MinValue, plotData.MaxValue}
                .MarkerStyle = XlMarkerStyle.xlMarkerStyleNone
                .Format.Line.Visible = True
            End With
        End With

        Return chartObj
    End Function

    <Extension()>
    Public Sub AddPlot(analysis As Agreement.WeightedDemingRegression, ws As Worksheet)
        If analysis Is Nothing Then Throw New ArgumentNullException(NameOf(analysis))
        If ws Is Nothing Then Throw New ArgumentNullException(NameOf(ws))

        Dim plotData As Agreement.DemingPlotData = analysis.GetPlotData()
        Dim ch = graphics.GeneralScatterPlot(plotData.XValues, plotData.YValues, plotData.YName, plotData.XName, ws, plotData.Title)

        With ch
            .HasLegend = True

            .SeriesCollection.NewSeries()
            With .SeriesCollection(2)
                .XValues = {plotData.MinX, plotData.MaxX}
                .Values = {plotData.Intercept + plotData.Slope * plotData.MinX,
                           plotData.Intercept + plotData.Slope * plotData.MaxX}
                .Name = "Regression line"
                .MarkerStyle = -4142
                .Border.Color = RGB(255, 0, 0)
                With .Format.Line
                    .Visible = True
                    .Weight = 1.5
                End With
            End With

            .SeriesCollection.NewSeries()
            With .SeriesCollection(3)
                .XValues = {plotData.MinX, plotData.MaxX}
                .Values = {plotData.MinX, plotData.MaxX}
                .Name = "Unity line (y = x)"
                .MarkerStyle = -4142
                .Border.Color = RGB(0, 0, 255)
                With .Format.Line
                    .Visible = True
                    .DashStyle = 4
                    .Weight = 0.5
                End With
            End With
        End With
    End Sub

    Private Sub AddHorizontalReferenceLine(ch As Chart,
                                           lineX As Double(),
                                           yValue As Double,
                                           seriesName As String,
                                           lineColor As Integer)
        If ch Is Nothing Then Exit Sub
        Dim lineY As Double() = {yValue, yValue}
        ch.SeriesCollection.NewSeries()
        With ch.SeriesCollection(ch.SeriesCollection.Count)
            .XValues = lineX
            .Values = lineY
            .Name = seriesName
            .MarkerStyle = XlMarkerStyle.xlMarkerStyleNone
            .Format.Line.Visible = True
            .Format.Line.ForeColor.RGB = lineColor
            .Format.Line.Weight = 1.5
        End With
    End Sub

End Module
