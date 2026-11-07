Option Explicit On
Option Strict Off

Imports Microsoft.Office.Interop.Excel
Imports System.Runtime.CompilerServices
Imports BESHStatNG.ExcelCommon

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
            Plot(analysis.GetKaplanMeierPlotData(alpha), ws, bPlotCI, bLegend, sTitle, sXaxisUnit)
        End Sub

        ' Payload overload only. The shipping renderer below retains its original series
        ' indexing, clipping, markers and native operations; Common performs no rendering.
        Public Shared Sub Plot(plotData As survival.KaplanMeierPlotData,
                               ws As Worksheet, bPlotCI As Boolean, bLegend As Boolean,
                               sTitle As String, sXaxisUnit As String)
            If ws Is Nothing Then Throw New ArgumentNullException(NameOf(ws))
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
                        .MaximumScale = SurvivalChartPresentation36.MaximumProbability
                        .MajorUnit = SurvivalChartPresentation36.ProbabilityMajorUnit
                        .MajorGridlines.Delete
                    End With
                    .Axes(XlAxisType.xlCategory).MinimumScale = 0

                    For i As Integer = 0 To noGroups - 1
                        Dim prepared = KaplanMeierSeriesGeometry36.Survival(plotData, i)
                        time = prepared.XValues
                        probability = prepared.YValues

                        .SeriesCollection.NewSeries
                        With .SeriesCollection(i + 1)
                            .Name = SurvivalChartPresentation36.SeriesName(plotData, i, KaplanMeierSeriesRole36.Survival)
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
                        Dim prepared = KaplanMeierSeriesGeometry36.Censor(plotData, groupIndex)
                        cenTimes = prepared.XValues
                        cenProbability = prepared.YValues
                        bCen = prepared.AssignValues

                        .SeriesCollection.NewSeries
                        If bCen Then
                            With .SeriesCollection(i + 1)
                                .XValues = cenTimes
                                .Values = cenProbability
                                .Name = SurvivalChartPresentation36.SeriesName(plotData, groupIndex, KaplanMeierSeriesRole36.Censor)
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
                        Dim prepared = KaplanMeierSeriesGeometry36.Confidence(plotData, groupIndex, True)
                        time = prepared.XValues
                            upCI = prepared.YValues

                        .SeriesCollection.NewSeries
                            With .SeriesCollection(i + 1)
                                .Name = SurvivalChartPresentation36.SeriesName(plotData, groupIndex, KaplanMeierSeriesRole36.UpperConfidence)
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
                        Dim prepared = KaplanMeierSeriesGeometry36.Confidence(plotData, groupIndex, False)
                        time = prepared.XValues
                            lowCI = prepared.YValues

                        .SeriesCollection.NewSeries
                            With .SeriesCollection(i + 1)
                                .Name = SurvivalChartPresentation36.SeriesName(plotData, groupIndex, KaplanMeierSeriesRole36.LowerConfidence)
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
                        .Axes(XlAxisType.xlValue, XlAxisGroup.xlPrimary).AxisTitle.Text = SurvivalChartPresentation36.ProbabilityTitle
                        .Axes(XlAxisType.xlCategory, XlAxisGroup.xlPrimary).HasTitle = False
                        .Axes(XlAxisType.xlCategory, XlAxisGroup.xlPrimary).HasTitle = True
                        .Axes(XlAxisType.xlCategory, XlAxisGroup.xlPrimary).AxisTitle.Text = SurvivalChartPresentation36.KmTimeTitle(sXaxisUnit)
                    Catch
                    End Try

                    For i As Integer = .SeriesCollection.Count To noGroups + 1 Step -1
                        .Legend.LegendEntries(i).Delete
                    Next

                    If SurvivalChartPresentation36.DeleteLegend(noGroups, bLegend) Then
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

    ''' <summary>
    ''' Windows-host compatibility extension for the historical <c>Survival_KM_LR.AddKMplot</c> call pattern.
    ''' The survival calculation type itself lives in BESHStat.Core; all Excel Interop remains here.
    ''' </summary>
    Public Module KaplanMeierPlotExcelCompatibility

        <Extension()>
        Public Sub AddKMplot(analysis As Survival_KM_LR,
                         ws As Worksheet,
                         bPlotCI As Boolean,
                         bLegend As Boolean,
                         sTitle As String,
                         sXaxisUnit As String,
                         Optional alpha As Double = 0.05)
            graphics.KaplanMeierPlotExcel.Plot(analysis, ws, bPlotCI, bLegend, sTitle, sXaxisUnit, alpha)
        End Sub

    End Module

End Namespace
