Option Explicit On

Imports Microsoft.Office.Interop.Excel

Namespace graphics

    ''' <summary>
    ''' Excel-specific renderer for Cox proportional hazards survival plots.
    ''' </summary>
    ''' <remarks>
    ''' This class intentionally lives in the graphics/Excel adapter layer rather than in
    ''' <c>CoxPH</c>.  The Cox model class should compute survival data only; worksheet
    ''' and chart creation belongs to the Excel-DNA front end.
    ''' </remarks>
    Public NotInheritable Class CoxSurvivalPlotExcel

        Private Sub New()
        End Sub

        ''' <summary>
        ''' Adds a Cox baseline survival plot to the supplied Excel worksheet.
        ''' </summary>
        Public Shared Sub PlotCox(ws As Worksheet,
                                  survTime() As Double,
                                  survProb() As Double,
                                  Optional lTop As Long = 100,
                                  Optional lLeft As Long = 100)

            If ws Is Nothing Then Throw New ArgumentNullException(NameOf(ws))
            If survTime Is Nothing Then Throw New ArgumentNullException(NameOf(survTime))
            If survProb Is Nothing Then Throw New ArgumentNullException(NameOf(survProb))
            If survTime.Length <> survProb.Length Then Throw New ArgumentException("survTime and survProb must have the same length.")
            If survTime.Length = 0 Then Exit Sub

            'subroutine adds Adjusted survival and Cumulative hazards plots
            'inputs:
            '   survTime()      - array of distinct survival times
            '   survProb()      - corresponding array of survival probabilities
            '   lTop            - top coordinate of the plot
            '   lLeft           - left coordinate of the plot

            'compute optimal scaling
            Dim udPlotAxisX = ChartScaling(0, survTime.Max())
            Dim yMax As Double = 1.0
            Dim majorUnit As Double = 0.2

            With ws.Shapes.AddChart(Left:=lLeft, Top:=lTop)
                With .Chart
                    .ChartType = XlChartType.xlXYScatterLinesNoMarkers

                    'delete extra series
                    Do Until .SeriesCollection.Count = 0
                        .SeriesCollection(1).Delete
                    Loop

                    With .Axes(XlAxisType.xlValue)
                        .MinimumScale = 0
                        .MaximumScale = yMax
                        .MajorUnit = majorUnit
                        .MajorGridlines.Delete
                    End With
                    .Axes(XlAxisType.xlCategory).MinimumScale = 0
                    .Axes(XlAxisType.xlCategory).MaximumScale = udPlotAxisX.Max

                    .SeriesCollection.NewSeries
                    With .SeriesCollection(1)
                        .Name = "Baseline"
                        .XValues = survTime
                        .Values = survProb
                        .Border.Color = RGB(155, 0, 0)
                        With .Format.Line
                            .Visible = True
                            .ForeColor.TintAndShade = 0
                            .ForeColor.Brightness = 0
                        End With
                    End With

                    Try 'add title and axis labels
                        .HasTitle = False
                        .HasTitle = True
                        .ChartTitle.Text = "Cox - Survival plot"
                        .Axes(XlAxisType.xlValue, XlAxisGroup.xlPrimary).HasTitle = False
                        .Axes(XlAxisType.xlValue, XlAxisGroup.xlPrimary).HasTitle = True
                        .Axes(XlAxisType.xlValue, XlAxisGroup.xlPrimary).AxisTitle.Text = "Survival Probability"
                        .Axes(XlAxisType.xlCategory, XlAxisGroup.xlPrimary).HasTitle = False
                        .Axes(XlAxisType.xlCategory, XlAxisGroup.xlPrimary).HasTitle = True
                        .Axes(XlAxisType.xlCategory, XlAxisGroup.xlPrimary).AxisTitle.Text = "Time"
                    Catch
                    End Try
                End With
            End With
        End Sub

    End Class

End Namespace