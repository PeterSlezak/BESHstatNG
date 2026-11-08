Option Explicit On

Imports System.Linq
Imports BESHStatNG.AppInfrastructure
Imports Microsoft.Office.Interop.Excel

Namespace graphics

    ''' <summary>
    ''' Excel-specific renderer for factor-analysis plots.
    ''' </summary>
    ''' <remarks>
    ''' This adapter keeps Excel chart creation out of the FactorAnalysis statistical model class.
    ''' The FactorAnalysis class computes eigenvalues, loadings, matrices, and result tables only;
    ''' Excel chart rendering belongs to the Excel-DNA graphics/front-end layer.
    ''' </remarks>
    Public NotInheritable Class FactorAnalysisPlotExcel

        Private Sub New()
        End Sub

        ''' <summary>
        ''' Creates a scree plot of the initial factor-analysis eigenvalue profile.
        ''' </summary>
        Public Shared Sub ScreePlot(model As Multivariate.FactorAnalysis)
            If model Is Nothing Then Throw New ArgumentNullException(NameOf(model))

            Dim initialEigenvalues() As Double = model.InitialEigenvalues
            If initialEigenvalues Is Nothing OrElse initialEigenvalues.Length = 0 Then Exit Sub

            Dim factorAxis(initialEigenvalues.Length - 1) As Integer
            For i As Integer = 0 To initialEigenvalues.Length - 1
                factorAxis(i) = i + 1
            Next
            Dim initialPct() As Double = PercentOfTotal(initialEigenvalues, TotalVariance(model.WorkingMatrix))

            AppGlobals.app.Charts.Add()
            With AppGlobals.app.ActiveWorkbook.ActiveChart
                .Name = "Scree Plot"
                .ChartType = XlChartType.xlXYScatter

                Do Until .SeriesCollection.Count = 0
                    .SeriesCollection(1).Delete
                Loop

                .SeriesCollection.NewSeries
                With .SeriesCollection(1)
                    .XValues = factorAxis
                    .Values = initialPct
                    .Name = "Initial Variance Explained"
                    .Format.Line.Weight = 1.5
                    .MarkerStyle = 8
                    .MarkerSize = 5
                    .Border.Color = RGB(100, 100, 100)
                    .MarkerForegroundColor = RGB(100, 100, 100)
                    .MarkerBackgroundColor = RGB(100, 100, 100)

                    For i As Integer = 0 To initialEigenvalues.Length - 1
                        .Points(i + 1).HasDataLabel = True
                        .Points(i + 1).DataLabel.Text = Format$(initialPct(i), "#0.0#")
                        .Points(i + 1).DataLabel.Position = XlDataLabelPosition.xlLabelPositionAbove
                        .Points(i + 1).DataLabel.Font.Size = 12
                    Next
                End With

                Try
                    .Legend.Delete()
                Catch
                End Try

                .Axes(XlAxisType.xlValue, XlAxisGroup.xlPrimary).HasTitle = False
                .Axes(XlAxisType.xlValue, XlAxisGroup.xlPrimary).HasTitle = True
                .Axes(XlAxisType.xlValue, XlAxisGroup.xlPrimary).AxisTitle.Text = "Variance explained [%]"
                .Axes(XlAxisType.xlValue, XlAxisGroup.xlPrimary).AxisTitle.Font.Size = 16
                .Axes(XlAxisType.xlValue, XlAxisGroup.xlPrimary).TickLabels.Font.Size = 14
                .Axes(XlAxisType.xlCategory, XlAxisGroup.xlPrimary).HasTitle = False
                .Axes(XlAxisType.xlCategory, XlAxisGroup.xlPrimary).HasTitle = True
                .Axes(XlAxisType.xlCategory, XlAxisGroup.xlPrimary).AxisTitle.Text = "Factor"
                .Axes(XlAxisType.xlCategory, XlAxisGroup.xlPrimary).AxisTitle.Font.Size = 16
                .Axes(XlAxisType.xlCategory, XlAxisGroup.xlPrimary).TickLabels.Font.Size = 14
                .HasTitle = False
                .HasTitle = True
                .ChartTitle.Text = "Scree Plot"
                .ChartTitle.Font.Size = 18
                .ChartTitle.Font.Bold = True
            End With
        End Sub

        ''' <summary>
        ''' Creates a 2D scatter plot of variable loadings on the first two retained factors.
        ''' </summary>
        Public Shared Sub LoadingPlot2D(model As Multivariate.FactorAnalysis)
            If model Is Nothing Then Throw New ArgumentNullException(NameOf(model))
            If model.PatternMatrix Is Nothing OrElse model.NumberOfFactors < 2 Then Exit Sub

            Dim patternMatrix As Double(,) = model.PatternMatrix
            Dim structureMatrix As Double(,) = model.StructureMatrix
            Dim varNames() As String = model.VariableNames
            Dim p As Integer = model.VariableCount
            Dim f1() As Double = Matrix.GetColumnFrom2Darray(patternMatrix, 0)
            Dim f2() As Double = Matrix.GetColumnFrom2Darray(patternMatrix, 1)
            Dim factorPct() As Double = PercentOfTotal(ColumnSumsOfSquares(patternMatrix, structureMatrix), TotalVariance(model.WorkingMatrix))
            Dim factorNames() As String = model.FactorNames()

            Dim scl1 As Double = Math.Max(Math.Abs(f1.Min()), Math.Abs(f1.Max()))
            Dim scl2 As Double = Math.Max(Math.Abs(f2.Min()), Math.Abs(f2.Max()))
            Dim udAxisX As CHARTscale = ChartScaling(-scl1, scl1)
            Dim udAxisY As CHARTscale = ChartScaling(-scl2, scl2)

            AppGlobals.app.Charts.Add()
            With AppGlobals.app.ActiveWorkbook.ActiveChart
                .Name = "Factor Loadings Plot2D"
                .ChartType = XlChartType.xlXYScatter

                Do Until .SeriesCollection.Count = 0
                    .SeriesCollection(1).Delete
                Loop

                With .Axes(XlAxisType.xlCategory)
                    .MinimumScale = udAxisX.Min
                    .MaximumScale = udAxisX.Max
                    .MajorUnit = udAxisX.Scale
                    .CrossesAt = -1.0E+100
                    .MajorTickMark = XlTickMark.xlTickMarkOutside
                    .MajorGridlines.Delete
                End With
                With .Axes(XlAxisType.xlValue)
                    .CrossesAt = -1.0E+100
                    .MinimumScale = udAxisY.Min
                    .MaximumScale = udAxisY.Max
                    .MajorUnit = udAxisY.Scale
                    .MajorTickMark = XlTickMark.xlTickMarkOutside
                    .MajorGridlines.Delete
                End With

                Dim seriesId As Integer = 0
                For id As Integer = 0 To p - 1
                    .SeriesCollection.NewSeries
                    seriesId += 1
                    With .SeriesCollection(seriesId)
                        .ChartType = XlChartType.xlXYScatterLinesNoMarkers
                        .XValues = {0, f1(id)}
                        .Values = {0, f2(id)}
                        .Name = "Loading_" & CStr(id)
                        .Format.Line.Weight = 1
                        .Format.Line.Visible = True
                        .Format.Line.ForeColor.RGB = RGB(0, 0, 150)
                        .Format.Line.EndArrowheadStyle = 2

                        .Points(2).HasDataLabel = True
                        .Points(2).DataLabel.Text = CStr(varNames(id))
                        .Points(2).DataLabel.Position = XlDataLabelPosition.xlLabelPositionAbove
                        .Points(2).DataLabel.Font.Size = 11
                        .Points(2).DataLabel.Font.Color = RGB(0, 0, 150)
                    End With
                Next

                .SeriesCollection.NewSeries
                seriesId += 1
                With .SeriesCollection(seriesId)
                    .XValues = {udAxisX.Min, udAxisX.Max}
                    .Values = {0, 0}
                    .Name = "Y Zero Line"
                    .MarkerStyle = -4142
                    .Border.Color = RGB(0, 0, 0)
                    With .Format.Line
                        .Visible = True
                        .Weight = 1
                    End With
                End With
                .SeriesCollection.NewSeries
                seriesId += 1
                With .SeriesCollection(seriesId)
                    .XValues = {0, 0}
                    .Values = {udAxisY.Min, udAxisY.Max}
                    .Name = "X Zero Line"
                    .MarkerStyle = -4142
                    .Border.Color = RGB(0, 0, 0)
                    With .Format.Line
                        .Visible = True
                        .Weight = 1
                    End With
                End With

                Try
                    .Legend.Delete()
                Catch
                End Try

                .Axes(XlAxisType.xlValue, XlAxisGroup.xlPrimary).HasTitle = False
                .Axes(XlAxisType.xlValue, XlAxisGroup.xlPrimary).HasTitle = True
                .Axes(XlAxisType.xlValue, XlAxisGroup.xlPrimary).AxisTitle.Text = $"{factorNames(1)} [{Format$(factorPct(1), "#0.0#")}%]"
                .Axes(XlAxisType.xlValue, XlAxisGroup.xlPrimary).AxisTitle.Font.Size = 16
                .Axes(XlAxisType.xlValue, XlAxisGroup.xlPrimary).TickLabels.Font.Size = 14
                .Axes(XlAxisType.xlCategory, XlAxisGroup.xlPrimary).HasTitle = False
                .Axes(XlAxisType.xlCategory, XlAxisGroup.xlPrimary).HasTitle = True
                .Axes(XlAxisType.xlCategory, XlAxisGroup.xlPrimary).AxisTitle.Text = $"{factorNames(0)} [{Format$(factorPct(0), "#0.0#")}%]"
                .Axes(XlAxisType.xlCategory, XlAxisGroup.xlPrimary).AxisTitle.Font.Size = 16
                .Axes(XlAxisType.xlCategory, XlAxisGroup.xlPrimary).TickLabels.Font.Size = 14
                .HasTitle = False
                .HasTitle = True
                .ChartTitle.Text = "Factor Loadings Plot"
                .ChartTitle.Font.Size = 18
                .ChartTitle.Font.Bold = True
            End With
        End Sub

        ''' <summary>
        ''' Creates a 3D scatter plot of variable loadings on the first three retained factors.
        ''' </summary>
        Public Shared Sub LoadingPlot3D(model As Multivariate.FactorAnalysis)
            If model Is Nothing Then Throw New ArgumentNullException(NameOf(model))
            If model.PatternMatrix Is Nothing OrElse model.NumberOfFactors < 3 Then Exit Sub

            Dim patternMatrix As Double(,) = model.PatternMatrix
            Dim structureMatrix As Double(,) = model.StructureMatrix
            Dim factorPct() As Double = PercentOfTotal(ColumnSumsOfSquares(patternMatrix, structureMatrix), TotalVariance(model.WorkingMatrix))
            Dim factorNames() As String = model.FactorNames()
            Dim f1() As Double = Matrix.GetColumnFrom2Darray(patternMatrix, 0)
            Dim f2() As Double = Matrix.GetColumnFrom2Darray(patternMatrix, 1)
            Dim f3() As Double = Matrix.GetColumnFrom2Darray(patternMatrix, 2)

            Dim XYZ As New XYZscatter
            With XYZ
                .ChartName = "Factor Loadings Plot3D"
                .dataInputs(f1, f2, f3)
                .axesLabelInputs($"{factorNames(0)} [{Format$(factorPct(0), "#0.0#")}%]",
                                 $"{factorNames(1)} [{Format$(factorPct(1), "#0.0#")}%]",
                                 $"{factorNames(2)} [{Format$(factorPct(2), "#0.0#")}%]")
                .showPlanePointInputs(True, True, True, 3, 3, 3)
                .ScaleAxis(False)
                .settingsInputs(True, True, True)
                .SetDataLabels(model.VariableNames)
                .draw()
            End With
        End Sub

        Private Shared Function ColumnSumsOfSquares(pattern(,) As Double, struct(,) As Double) As Double()
            Dim out(pattern.GetLength(1) - 1) As Double
            For j As Integer = 0 To pattern.GetLength(1) - 1
                Dim s As Double = 0.0
                For i As Integer = 0 To pattern.GetLength(0) - 1
                    s += pattern(i, j) * struct(i, j)
                Next
                out(j) = s
            Next
            Return out
        End Function

        Private Shared Function PercentOfTotal(values() As Double, total As Double) As Double()
            Dim out(values.Length - 1) As Double
            If total <= 0.0 Then Return out
            For i As Integer = 0 To values.Length - 1
                out(i) = 100.0 * values(i) / total
            Next
            Return out
        End Function

        Private Shared Function TotalVariance(mat(,) As Double) As Double
            Dim s As Double = 0.0
            For i As Integer = 0 To Math.Min(mat.GetLength(0), mat.GetLength(1)) - 1
                s += mat(i, i)
            Next
            Return s
        End Function

    End Class

End Namespace