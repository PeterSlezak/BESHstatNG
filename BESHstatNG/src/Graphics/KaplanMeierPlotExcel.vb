Option Explicit On
Option Strict Off

Imports Microsoft.Office.Interop.Excel

Namespace graphics

    ''' <summary>
    ''' Excel-specific renderer for Kaplan-Meier survival curves.
    ''' The statistical calculation layer supplies a host-neutral plotting payload.
    ''' </summary>
    Public NotInheritable Class KaplanMeierPlotExcel

        Private Sub New()
        End Sub

        Public Shared Sub Plot(analysis As survival.Survival_KM_LR,
                               ws As Worksheet,
                               bPlotCI As Boolean,
                               bLegend As Boolean,
                               sTitle As String,
                               sXaxisUnit As String,
                               Optional alpha As Double = 0.05)
            If analysis Is Nothing Then Throw New ArgumentNullException(NameOf(analysis))
            If ws Is Nothing Then Throw New ArgumentNullException(NameOf(ws))

            Dim plotData As survival.KaplanMeierPlotData = analysis.GetKaplanMeierPlotData(alpha)
            Dim noGroups As Integer = plotData.GroupLabels.Length
            Dim bCen As Boolean
            Dim cenProbability() As Double = Nothing
            Dim cenTimes() As Double = Nothing
            Dim upCI() As Double = Nothing
            Dim lowCI() As Double = Nothing
            Dim time() As Double = Nothing
            Dim probability() As Double = Nothing

            With ws.Shapes.AddChart
                With .Chart
                    .ChartType = XlChartType.xlXYScatterLinesNoMarkers

                    Do Until .SeriesCollection.Count = 0
                        .SeriesCollection(1).Delete
                    Loop

                    With .Axes(XlAxisType.xlValue)
                        .MinimumScale = 0
                        .MaximumScale = 1
                        .MajorUnit = 0.2
                        .MajorGridlines.Delete
                    End With
                    .Axes(XlAxisType.xlCategory).MinimumScale = 0

                    For i As Integer = 0 To noGroups - 1
                        Dim plotUpper As Integer = plotData.SurvivalProbability.GetUpperBound(0)
                        ReDim probability(plotUpper + 1)
                        ReDim time(plotUpper + 1)
                        probability(0) = 1
                        time(0) = 0

                        For j As Integer = 0 To plotUpper
                            If plotData.SurvivalTime(j) <= plotData.MaximumTimeByGroup(i) Then
                                probability(j + 1) = plotData.SurvivalProbability(j, i)
                                time(j + 1) = plotData.SurvivalTime(j)
                            Else
                                ReDim Preserve probability(j)
                                ReDim Preserve time(j)
                                Exit For
                            End If
                        Next

                        .SeriesCollection.NewSeries
                        With .SeriesCollection(i + 1)
                            .Name = plotData.GroupLabels(i)
                            .XValues = time
                            .Values = probability
                            .Border.Color = graphics.GetColor(i + 1)
                            With .Format.Line
                                .Visible = True
                                .ForeColor.TintAndShade = 0
                                .Weight = 2.25
                                .ForeColor.Brightness = 0
                            End With
                        End With
                    Next

                    For i As Integer = noGroups To 2 * noGroups - 1
                        Dim groupIndex As Integer = i - noGroups
                        If plotData.CensoredCountByGroup(groupIndex) > 0 Then
                            ReDim cenProbability(plotData.CensoredCountByGroup(groupIndex) - 1)
                            ReDim cenTimes(plotData.CensoredCountByGroup(groupIndex) - 1)
                            bCen = True
                        Else
                            ReDim cenProbability(0)
                            ReDim cenTimes(0)
                            bCen = False
                        End If

                        For j As Integer = 0 To plotData.CensoredCountByGroup(groupIndex) - 1
                            cenProbability(j) = plotData.CensorMarkerProbability(j, groupIndex)
                            cenTimes(j) = plotData.CensorMarkerTime(j, groupIndex)
                        Next

                        .SeriesCollection.NewSeries
                        If bCen Then
                            With .SeriesCollection(i + 1)
                                .XValues = cenTimes
                                .Values = cenProbability
                                .Name = "Censored " + plotData.GroupLabels(groupIndex)
                                With .Format.Line
                                    .Visible = True
                                    .ForeColor.TintAndShade = 0
                                    .ForeColor.Brightness = 0
                                End With
                                .MarkerStyle = 9
                                .MarkerSize = 5
                                .ChartType = XlChartType.xlXYScatter
                                .MarkerForegroundColor = graphics.GetColor(groupIndex + 1)
                            End With
                        End If
                    Next

                    If bPlotCI Then
                        For i As Integer = 2 * noGroups To 3 * noGroups - 1
                            Dim groupIndex As Integer = i - 2 * noGroups
                            Dim plotUpper As Integer = plotData.SurvivalProbability.GetUpperBound(0)
                            ReDim upCI(plotUpper + 1)
                            ReDim time(plotData.SurvivalTime.Length)
                            upCI(0) = 1
                            time(0) = 0

                            For j As Integer = 0 To plotUpper
                                If plotData.SurvivalTime(j) <= plotData.MaximumTimeByGroup(groupIndex) Then
                                    upCI(j + 1) = plotData.UpperConfidenceLimit(j, groupIndex)
                                    time(j + 1) = plotData.SurvivalTime(j)
                                Else
                                    ReDim Preserve upCI(j)
                                    ReDim Preserve time(j)
                                    Exit For
                                End If
                            Next

                            .SeriesCollection.NewSeries
                            With .SeriesCollection(i + 1)
                                .Name = $"{100.0 * (1.0 - plotData.Alpha):0.##}% CI " + plotData.GroupLabels(groupIndex)
                                .XValues = time
                                .Values = upCI
                                .Border.Color = graphics.GetColor(groupIndex + 1)
                                With .Format.Line
                                    .Visible = True
                                    .ForeColor.TintAndShade = 0
                                    .ForeColor.Brightness = 0
                                    .Weight = 1
                                    .DashStyle = 4
                                End With
                            End With
                        Next

                        For i As Integer = 3 * noGroups To 4 * noGroups - 1
                            Dim groupIndex As Integer = i - 3 * noGroups
                            Dim plotUpper As Integer = plotData.SurvivalProbability.GetUpperBound(0)
                            ReDim lowCI(plotUpper + 1)
                            ReDim time(plotData.SurvivalTime.Length)
                            lowCI(0) = 1
                            time(0) = 0

                            For j As Integer = 0 To plotUpper
                                If plotData.SurvivalTime(j) <= plotData.MaximumTimeByGroup(groupIndex) Then
                                    lowCI(j + 1) = plotData.LowerConfidenceLimit(j, groupIndex)
                                    time(j + 1) = plotData.SurvivalTime(j)
                                Else
                                    ReDim Preserve lowCI(j)
                                    ReDim Preserve time(j)
                                    Exit For
                                End If
                            Next

                            .SeriesCollection.NewSeries
                            With .SeriesCollection(i + 1)
                                .Name = $"{100.0 * (1.0 - plotData.Alpha):0.##}% CI " + plotData.GroupLabels(groupIndex)
                                .XValues = time
                                .Values = lowCI
                                .Border.Color = graphics.GetColor(groupIndex + 1)
                                With .Format.Line
                                    .Visible = True
                                    .ForeColor.TintAndShade = 0
                                    .ForeColor.Brightness = 0
                                    .Weight = 1
                                    .DashStyle = 4
                                End With
                            End With
                        Next
                    End If

                    Try
                        .HasTitle = False
                        .HasTitle = True
                        If sTitle <> String.Empty Then .ChartTitle.Text = sTitle
                        If sTitle = String.Empty Then .HasTitle = False
                        .Axes(XlAxisType.xlValue, XlAxisGroup.xlPrimary).HasTitle = False
                        .Axes(XlAxisType.xlValue, XlAxisGroup.xlPrimary).HasTitle = True
                        .Axes(XlAxisType.xlValue, XlAxisGroup.xlPrimary).AxisTitle.Text = "Survival Probability"
                        .Axes(XlAxisType.xlCategory, XlAxisGroup.xlPrimary).HasTitle = False
                        .Axes(XlAxisType.xlCategory, XlAxisGroup.xlPrimary).HasTitle = True
                        .Axes(XlAxisType.xlCategory, XlAxisGroup.xlPrimary).AxisTitle.Text = $"Time ({sXaxisUnit})"
                    Catch
                    End Try

                    For i As Integer = .SeriesCollection.Count To noGroups + 1 Step -1
                        .Legend.LegendEntries(i).Delete
                    Next

                    If noGroups = 1 OrElse Not bLegend Then
                        Try
                            .Legend.Delete()
                        Catch
                        End Try
                    End If
                End With
            End With
        End Sub

    End Class

End Namespace

Namespace survival

    'Windows-host compatibility surface for the historical Survival_KM_LR.AddKMplot API.
    'Keeping this partial declaration in the Excel project isolates Interop from the calculation source file.
    Partial Public Class Survival_KM_LR

        Public Sub AddKMplot(ws As Worksheet,
                             bPlotCI As Boolean,
                             bLegend As Boolean,
                             sTitle As String,
                             sXaxisUnit As String,
                             Optional alpha As Double = 0.05)
            graphics.KaplanMeierPlotExcel.Plot(Me, ws, bPlotCI, bLegend, sTitle, sXaxisUnit, alpha)
        End Sub

    End Class

End Namespace
