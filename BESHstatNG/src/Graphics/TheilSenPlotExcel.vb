Option Explicit On

Imports System.Runtime.CompilerServices
Imports Microsoft.Office.Interop.Excel

Namespace nonparametric

    ''' <summary>
    ''' Windows-host compatibility extension for the historical <c>TheilSen.AddPlot</c> call pattern.
    ''' The statistical calculation and plot payload live in BESHStat.Core; all Excel Interop remains here.
    ''' </summary>
    Public Module TheilSenPlotExcelCompatibility

        <Extension()>
        Public Sub AddPlot(analysis As TheilSen, ws As Worksheet)
            If analysis Is Nothing Then Throw New ArgumentNullException(NameOf(analysis))
            If ws Is Nothing Then Throw New ArgumentNullException(NameOf(ws))

            Dim plotData As TheilSenPlotData = analysis.GetPlotData()
            Dim ch = graphics.GeneralScatterPlot(
                plotData.XValues,
                plotData.YValues,
                plotData.YName,
                plotData.XName,
                ws)

            With ch
                .SeriesCollection.NewSeries
                With .SeriesCollection(2)
                    .XValues = {plotData.MinX, plotData.MaxX}
                    .Values = {plotData.FittedYAtMinX, plotData.FittedYAtMaxX}
                    .Name = "Nonparametric Fit"
                    .MarkerStyle = -4142
                    .Border.Color = RGB(255, 0, 0)
                    With .Format.Line
                        .Visible = True
                        .Weight = 1.5
                    End With
                End With
            End With
        End Sub

    End Module

End Namespace
