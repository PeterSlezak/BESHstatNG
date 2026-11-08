Option Explicit On
Option Strict On
Option Compare Text
Imports System
Imports System.Linq
Imports System.Collections.Generic
Imports BESHStatNG
Imports BESHStatNG.DataManagement
Imports BESHStatNG.Multivariate
Namespace Frozen37
Public Class Original37
        Public Shared Function AxisPadding(minValue As Double,
                                    maxValue As Double,
                                    referenceSize As Double) As Double
            Dim span As Double = maxValue - minValue
            If span > 0 Then Return 0.05 * span
            If referenceSize > 0 Then Return 0.1 * referenceSize
            Return 0.5
        End Function
Public Shared Function PcaLoadingPlot2D(model As PCA) As Object
If model Is Nothing Then Throw New ArgumentNullException(NameOf(model))
            If model.NoExtractComponents < 2 Then Return Nothing

            Dim loadings As Double(,) = model.GetLoadings
            Dim varNames() As String = model.VariableNames
            Dim p As Integer = model.VariableCount
            Dim pct() As Double = model.PercentExpl

            Dim pc1() As Double = ArrayUtilities.GetColumn(loadings, 0)
            Dim pc2() As Double = ArrayUtilities.GetColumn(loadings, 1)
            Dim scl1 As Double = Math.Max(Math.Abs(pc1.Min()), Math.Abs(pc1.Max()))
            Dim scl2 As Double = Math.Max(Math.Abs(pc2.Min()), Math.Abs(pc2.Max()))
            Dim udAxisX As ChartAxisScaleResult = ChartAxisScalingCore.Compute(-scl1, scl1)
            Dim udAxisY As ChartAxisScaleResult = ChartAxisScalingCore.Compute(-scl2, scl2)

            Return New Object(){pc1, pc2, varNames, pct, udAxisX.Min, udAxisX.Max, udAxisX.Scale, udAxisY.Min, udAxisY.Max, udAxisY.Scale}
End Function

Public Shared Function PcaLoadingPlot3D(model As PCA) As Object
If model Is Nothing Then Throw New ArgumentNullException(NameOf(model))
            If model.NoExtractComponents < 3 Then Return Nothing

            Dim loadings As Double(,) = model.GetLoadings
            Dim pc1() As Double = ArrayUtilities.GetColumn(loadings, 0)
            Dim pc2() As Double = ArrayUtilities.GetColumn(loadings, 1)
            Dim pc3() As Double = ArrayUtilities.GetColumn(loadings, 2)
            Dim pct() As Double = model.PercentExpl

            Return New Object(){pc1, pc2, pc3, model.VariableNames, pct}
End Function

Public Shared Function PcaScorePlot2D(model As PCA, rowLabels() As String, groupLabels() As String) As Object
If model Is Nothing Then Throw New ArgumentNullException(NameOf(model))
            If model.NoExtractComponents < 2 Then Return Nothing

            Dim scores As Double(,) = model.ReducedDataset
            Dim n As Integer = model.ObservationCount
            Dim pct() As Double = model.PercentExpl
            Dim scoreLabels() As String = ResolveScoreLabels(model, rowLabels)
            Dim normalizedGroups() As String = ResolveGroupLabels(groupLabels, n)

            Dim pc1() As Double = ArrayUtilities.GetColumn(scores, 0)
            Dim pc2() As Double = ArrayUtilities.GetColumn(scores, 1)
            Dim udAxisX As ChartAxisScaleResult = ChartAxisScalingCore.Compute(pc1.Min(), pc1.Max())
            Dim udAxisY As ChartAxisScaleResult = ChartAxisScalingCore.Compute(pc2.Min(), pc2.Max())

            Return New Object(){pc1, pc2, scoreLabels, normalizedGroups, pct, udAxisX.Min, udAxisX.Max, udAxisX.Scale, udAxisY.Min, udAxisY.Max, udAxisY.Scale}
End Function

Public Shared Function PcaScorePlot3D(model As PCA, rowLabels() As String, groupLabels() As String) As Object
If model Is Nothing Then Throw New ArgumentNullException(NameOf(model))
            If model.NoExtractComponents < 3 Then Return Nothing

            Dim scores As Double(,) = model.ReducedDataset
            Dim pc1() As Double = ArrayUtilities.GetColumn(scores, 0)
            Dim pc2() As Double = ArrayUtilities.GetColumn(scores, 1)
            Dim pc3() As Double = ArrayUtilities.GetColumn(scores, 2)
            Dim pct() As Double = model.PercentExpl
            Dim n As Integer = model.ObservationCount

            Dim scoreLabels() As String = ResolveScoreLabels(model, rowLabels)
            Dim normalizedGroups() As String = ResolveGroupLabels(groupLabels, n)

            Return New Object(){pc1, pc2, pc3, scoreLabels, normalizedGroups, pct}
End Function

Public Shared Function PcaBiplot(model As PCA, c As Double, rowLabels() As String, groupLabels() As String) As Object
If model Is Nothing Then Throw New ArgumentNullException(NameOf(model))
            If model.NoExtractComponents < 2 Then Return Nothing
            If c < 0.0 Or c > 1.0 Then Throw New ArgumentException("biplot 'scale' is outside of range [0, 1]")

            Dim titl As String = String.Empty
            If c = 0.0 Then
                titl = "GH, or column-metric preserving"
            ElseIf c = 1.0 Then
                titl = "JK, or row-metric preserving"
            ElseIf c = 0.5 Then
                titl = "SQ, or symmetric"
            End If

            Dim scores As Double(,) = model.ReducedDataset
            Dim loadings As Double(,) = model.GetLoadings
            Dim varNames() As String = model.VariableNames
            Dim eigenvalues() As Double = model.Eigenvalues
            Dim pct() As Double = model.PercentExpl
            Dim n As Integer = model.ObservationCount
            Dim p As Integer = model.VariableCount
            Dim scoreLabels() As String = ResolveScoreLabels(model, rowLabels)
            Dim normalizedGroups() As String = ResolveGroupLabels(groupLabels, n)

            Dim pc1() As Double = ArrayUtilities.GetColumn(scores, 0)
            Dim pc2() As Double = ArrayUtilities.GetColumn(scores, 1)
            Dim Load1() As Double = ArrayUtilities.GetColumn(loadings, 0)
            Dim Load2() As Double = ArrayUtilities.GetColumn(loadings, 1)

            Dim lam(1) As Double
            For i = 0 To 1
                lam(i) = Math.Sqrt(eigenvalues(i)) * Math.Sqrt(n)
                lam(i) = lam(i) ^ (1.0 - c)
            Next

            For i = 0 To n - 1
                pc1(i) /= lam(0)
                pc2(i) /= lam(1)
            Next

            For i = 0 To p - 1
                Load1(i) *= lam(0)
                Load2(i) *= lam(1)
            Next

            Dim udAxisX As ChartAxisScaleResult = ChartAxisScalingCore.Compute(Math.Min(pc1.Min(), Load1.Min()), Math.Max(pc1.Max(), Load1.Max()))
            Dim udAxisY As ChartAxisScaleResult = ChartAxisScalingCore.Compute(Math.Min(pc2.Min(), Load2.Min()), Math.Max(pc2.Max(), Load2.Max()))

            Return New Object(){pc1, pc2, Load1, Load2, scoreLabels, normalizedGroups, pct, titl, udAxisX.Min, udAxisX.Max, udAxisX.Scale, udAxisY.Min, udAxisY.Max, udAxisY.Scale}
End Function

Public Shared Function PcaScreePlot(model As PCA) As Object
If model Is Nothing Then Throw New ArgumentNullException(NameOf(model))

            Dim pct() As Double = model.PercentExpl
            Dim p As Integer = model.VariableCount

            Return New Object(){model.XaxisComponents, pct}
End Function


        Private Shared Function ResolveScoreLabels(model As Multivariate.PCA, rowLabels() As String) As String()
            Dim n As Integer = model.ObservationCount
            Dim rowIds() As Integer = model.RowIds
            Dim out(n - 1) As String

            If rowLabels IsNot Nothing AndAlso rowLabels.Length <> n Then
                Throw New ArgumentException("PCA row-label count must match the number of observations.", NameOf(rowLabels))
            End If

            For i As Integer = 0 To n - 1
                Dim text As String = String.Empty
                If rowLabels IsNot Nothing AndAlso rowLabels(i) IsNot Nothing Then text = rowLabels(i).Trim()
                If text = String.Empty Then text = CStr(rowIds(i))
                out(i) = text
            Next

            Return out
        End Function


        Private Shared Function ResolveGroupLabels(groupLabels() As String, expectedCount As Integer) As String()
            If groupLabels Is Nothing Then Return Nothing
            If groupLabels.Length <> expectedCount Then
                Throw New ArgumentException("PCA grouping-label count must match the number of observations.", NameOf(groupLabels))
            End If

            Dim out(expectedCount - 1) As String
            For i As Integer = 0 To expectedCount - 1
                Dim text As String = If(groupLabels(i), String.Empty).Trim()
                If text = String.Empty Then text = "(missing)"
                out(i) = text
            Next
            Return out
        End Function

Public Shared Function Groups(xValues() As Double, yValues() As Double, pointLabels() As String, groupLabels() As String, ungroupedSeriesName As String) As Object
If groupLabels Is Nothing Then Return New Object(){New Object(){ungroupedSeriesName, xValues, yValues, pointLabels}}
Dim groupIds As New List(Of String)
            Dim seen As New HashSet(Of String)(StringComparer.Ordinal)
            For Each groupId As String In groupLabels
                If seen.Add(groupId) Then groupIds.Add(groupId)
            Next

            
Dim result As New List(Of Object)
For g As Integer = 0 To groupIds.Count - 1
                Dim xs As New List(Of Double)
                Dim ys As New List(Of Double)
                Dim labels As New List(Of String)

                For i As Integer = 0 To groupLabels.Length - 1
                    If groupLabels(i) = groupIds(g) Then
                        xs.Add(xValues(i))
                        ys.Add(yValues(i))
                        labels.Add(pointLabels(i))
                    End If
                Next

                result.Add(New Object(){groupIds(g), xs.ToArray(), ys.ToArray(), labels.ToArray()})
Next
Return result.ToArray()
End Function


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

Public Shared Function FactorScreePlot(model As FactorAnalysis) As Object
If model Is Nothing Then Throw New ArgumentNullException(NameOf(model))

            Dim initialEigenvalues() As Double = model.InitialEigenvalues
            If initialEigenvalues Is Nothing OrElse initialEigenvalues.Length = 0 Then Return Nothing

            Dim factorAxis(initialEigenvalues.Length - 1) As Integer
            For i As Integer = 0 To initialEigenvalues.Length - 1
                factorAxis(i) = i + 1
            Next
            Dim initialPct() As Double = PercentOfTotal(initialEigenvalues, TotalVariance(model.WorkingMatrix))

            Return New Object(){factorAxis, initialPct}
End Function

Public Shared Function FactorLoadingPlot2D(model As FactorAnalysis) As Object
If model Is Nothing Then Throw New ArgumentNullException(NameOf(model))
            If model.PatternMatrix Is Nothing OrElse model.NumberOfFactors < 2 Then Return Nothing

            Dim patternMatrix As Double(,) = model.PatternMatrix
            Dim structureMatrix As Double(,) = model.StructureMatrix
            Dim varNames() As String = model.VariableNames
            Dim p As Integer = model.VariableCount
            Dim f1() As Double = ArrayUtilities.GetColumn(patternMatrix, 0)
            Dim f2() As Double = ArrayUtilities.GetColumn(patternMatrix, 1)
            Dim factorPct() As Double = PercentOfTotal(ColumnSumsOfSquares(patternMatrix, structureMatrix), TotalVariance(model.WorkingMatrix))
            Dim factorNames() As String = model.FactorNames()

            Dim scl1 As Double = Math.Max(Math.Abs(f1.Min()), Math.Abs(f1.Max()))
            Dim scl2 As Double = Math.Max(Math.Abs(f2.Min()), Math.Abs(f2.Max()))
            Dim udAxisX As ChartAxisScaleResult = ChartAxisScalingCore.Compute(-scl1, scl1)
            Dim udAxisY As ChartAxisScaleResult = ChartAxisScalingCore.Compute(-scl2, scl2)

            Return New Object(){f1, f2, varNames, factorPct, factorNames, udAxisX.Min, udAxisX.Max, udAxisX.Scale, udAxisY.Min, udAxisY.Max, udAxisY.Scale}
End Function

Public Shared Function FactorLoadingPlot3D(model As FactorAnalysis) As Object
If model Is Nothing Then Throw New ArgumentNullException(NameOf(model))
            If model.PatternMatrix Is Nothing OrElse model.NumberOfFactors < 3 Then Return Nothing

            Dim patternMatrix As Double(,) = model.PatternMatrix
            Dim structureMatrix As Double(,) = model.StructureMatrix
            Dim factorPct() As Double = PercentOfTotal(ColumnSumsOfSquares(patternMatrix, structureMatrix), TotalVariance(model.WorkingMatrix))
            Dim factorNames() As String = model.FactorNames()
            Dim f1() As Double = ArrayUtilities.GetColumn(patternMatrix, 0)
            Dim f2() As Double = ArrayUtilities.GetColumn(patternMatrix, 1)
            Dim f3() As Double = ArrayUtilities.GetColumn(patternMatrix, 2)

            Return New Object(){f1, f2, f3, model.VariableNames, factorPct, factorNames}
End Function

Public Shared Function CaContribution(model As CA, lAxis As Integer, bRow As Boolean) As Object
If model Is Nothing Then Throw New ArgumentNullException(NameOf(model))

            
            
            Dim contrib() As Double

            If bRow Then
                contrib = model.RowContribution(lAxis)
            Else
                contrib = model.ColContribution(lAxis)
            End If

            Return contrib
End Function

Public Shared Function CaBounds(model As CA) As Object
If model Is Nothing Then Throw New ArgumentNullException(NameOf(model))
            If model.FactorCount < 2 Then Return Nothing

            
            

            Dim udPlotAxisY As ChartAxisScaleResult = ChartAxisScalingCore.Compute(Math.Min(model.RowFactors(1).Min(), model.ColFactors(1).Min()),
                                                         Math.Max(model.RowFactors(1).Max(), model.ColFactors(1).Max()))
            Dim udPlotAxisX As ChartAxisScaleResult = ChartAxisScalingCore.Compute(Math.Min(model.RowFactors(0).Min(), model.ColFactors(0).Min()),
                                                         Math.Max(model.RowFactors(0).Max(), model.ColFactors(0).Max()))

            Return New Object(){udPlotAxisX.Min,udPlotAxisX.Max,udPlotAxisX.Scale,udPlotAxisY.Min,udPlotAxisY.Max,udPlotAxisY.Scale}
End Function

Public Shared Function CaPoints(model As CA, bRow As Boolean) As Object
Return New Object(){If(bRow,model.RowFactors(0),model.ColFactors(0)), If(bRow,model.RowFactors(1),model.ColFactors(1)), model.PlotLabels(bRow, Nothing)}
End Function

Public Shared Function CaLabels(model As CA, bRow As Boolean) As Object
Return model.PlotLabels(bRow, vbNewLine)
End Function

Public Shared Function CaCaptions(model As CA) As Object
Dim pct As Double(,) = model.Percents
Return New Object(){$"Factor 2 [{ Format$(pct(2, 1), "#0.0#") }%]", $"Factor 1 [{ Format$(pct(1, 1), "#0.0#") }%]"}
End Function

Public Shared Function DendrogramBounds(layout As DendrogramLayout) As Object
Dim xMin As Double = layout.PolylineX.Min()
                Dim xMax As Double = layout.PolylineX.Max()
                Dim yMin As Double = layout.PolylineY.Min()
                Dim yMax As Double = layout.PolylineY.Max()

                If layout.CutLineX IsNot Nothing AndAlso layout.CutLineX.Length > 0 Then
                    xMin = Math.Min(xMin, layout.CutLineX.Min())
                    xMax = Math.Max(xMax, layout.CutLineX.Max())
                End If
                If layout.CutLineY IsNot Nothing AndAlso layout.CutLineY.Length > 0 Then
                    yMin = Math.Min(yMin, layout.CutLineY.Min())
                    yMax = Math.Max(yMax, layout.CutLineY.Max())
                End If

                Dim xReference As Double = If(layout.Orientation = DendrogramOrientation.Left OrElse layout.Orientation = DendrogramOrientation.Right,
                                              If(layout.MaximumHeight <= 0, 1.0, layout.MaximumHeight),
                                              layout.LeafCount)
                Dim yReference As Double = If(layout.Orientation = DendrogramOrientation.Left OrElse layout.Orientation = DendrogramOrientation.Right,
                                              layout.LeafCount,
                                              If(layout.MaximumHeight <= 0, 1.0, layout.MaximumHeight))
                Dim xPad As Double = AxisPadding(xMin, xMax, xReference)
                Dim yPad As Double = AxisPadding(yMin, yMax, yReference)

                Return New Object(){xMin-xPad,xMax+xPad,yMin-yPad,yMax+yPad}
End Function

Public Shared Function DendrogramTitles(layout As DendrogramLayout, distanceAxisTitle As String) As Object
Dim resolvedDistanceTitle As String = distanceAxisTitle
            If String.IsNullOrWhiteSpace(resolvedDistanceTitle) Then
                resolvedDistanceTitle = If(layout.HeightMode = DendrogramHeightMode.StepLevels,
                                           "Distance not in proportion",
                                           "Distance")
            End If

            Return New Object(){resolvedDistanceTitle, layout.GetSuggestedAxisTitle()}
End Function

Public Shared Function Captions(pct() As Double, factorPct() As Double, factorNames() As String) As Object
Return New Object(){$"1st Component Scores [{ Format$(pct(0), "#0.0#") }%]",
$"2nd Component Scores [{ Format$(pct(1), "#0.0#") }%]",
$"3rd Component Scores [{ Format$(pct(2), "#0.0#") }%]",
$"1St Component Scores [{ Format$(pct(0), "#0.0#") }%]",
$"3Rd Component Scores [{ Format$(pct(2), "#0.0#") }%]",
$"1st Component Scores [{ Format$(pct(0), "#0.0#")}%]",
$"2nd Component Scores [{ Format$(pct(1), "#0.0#")}%]",
$"{factorNames(1)} [{Format$(factorPct(1), "#0.0#")}%]",
$"{factorNames(0)} [{Format$(factorPct(0), "#0.0#")}%]",
$"{factorNames(2)} [{Format$(factorPct(2), "#0.0#")}%]"}
End Function

End Class
End Namespace
