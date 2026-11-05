Option Strict On
Option Explicit On
Imports System
Imports System.Collections.Generic
Imports System.Globalization
Imports BESHStatNG.StatisticalProcessControl

' Frozen source-derived host-free 04A.22 characterization. NOT_GOLDEN.
Public Class Spc22LegacyLabel
    Public ReadOnly Index As Integer
    Public ReadOnly Text As String
    Public Sub New(i As Integer, t As String)
        Index = i : Text = t
    End Sub
End Class
Public Class Spc22LegacyRenderer
    Public Class SpcControlChartAppearanceOptions
        Public Property UseSequenceValuesForHorizontalAxis As Boolean
        Public StatisticColor As Integer
        Public SignalColor As Integer
        Public ExclusionColor As Integer
        Public DataLabelFontSize As Double
        Public ShowSpecificationLimits As Boolean
        Public ShowTargetLine As Boolean
    End Class
    Public Enum XlDataLabelPosition
        xlLabelPositionAbove
        xlLabelPositionBelow
    End Enum
    Private Shared Sub SetPointLabel(series As List(Of Spc22LegacyLabel), i As Integer, text As String, color As Integer, size As Double, position As XlDataLabelPosition)
        If String.IsNullOrWhiteSpace(text) Then Return
        series.Add(New Spc22LegacyLabel(i, text))
    End Sub
        Public Shared Function BuildSharedAxisContext(
            result As SpcFitResult,
            options As SpcControlChartAppearanceOptions) As SharedAxisContext

            Dim representativePoints As New Dictionary(Of Integer, SpcPointResult)()
            Dim panels As SpcPanelResult() = result.Panels

            For panelIndex As Integer = 0 To panels.Length - 1
                Dim points As SpcPointResult() = panels(panelIndex).Points
                For pointIndex As Integer = 0 To points.Length - 1
                    Dim point As SpcPointResult = points(pointIndex)
                    If Not representativePoints.ContainsKey(point.PointIndex) Then
                        representativePoints.Add(point.PointIndex, point)
                    End If
                Next
            Next

            Dim orderedIndices As New List(Of Integer)(representativePoints.Keys)
            orderedIndices.Sort()
            Dim categories(orderedIndices.Count - 1) As Object
            Dim stageIds(orderedIndices.Count - 1) As String

            For i As Integer = 0 To orderedIndices.Count - 1
                Dim point As SpcPointResult = representativePoints(orderedIndices(i))
                If options.UseSequenceValuesForHorizontalAxis AndAlso
                   point.SequenceValue.HasValue Then
                    categories(i) = point.SequenceValue.Value
                Else
                    categories(i) = point.Label
                End If
                stageIds(i) = point.StageId
            Next

            Return New SharedAxisContext(orderedIndices.ToArray(), categories, stageIds)
        End Function

        Public Shared Function BuildPanelChartData(
            panel As SpcPanelResult,
            axisContext As SharedAxisContext) As PanelChartData

            Dim pointByIndex As New Dictionary(Of Integer, SpcPointResult)()
            Dim sourcePoints As SpcPointResult() = panel.Points
            For i As Integer = 0 To sourcePoints.Length - 1
                pointByIndex.Add(sourcePoints(i).PointIndex, sourcePoints(i))
            Next

            Dim n As Integer = axisContext.PointIndices.Length
            Dim points(n - 1) As SpcPointResult
            Dim statistic(n - 1) As Object
            Dim center(n - 1) As Object
            Dim lcl(n - 1) As Object
            Dim ucl(n - 1) As Object
            Dim lowerOne(n - 1) As Object
            Dim upperOne(n - 1) As Object
            Dim lowerTwo(n - 1) As Object
            Dim upperTwo(n - 1) As Object
            Dim signals(n - 1) As Object
            Dim exclusions(n - 1) As Object

            For i As Integer = 0 To n - 1
                Dim point As SpcPointResult = Nothing
                If pointByIndex.TryGetValue(axisContext.PointIndices(i), point) Then
                    points(i) = point
                    statistic(i) = ChartValue(point.Value)
                    center(i) = ChartValue(point.CenterLine)
                    lcl(i) = ChartValue(point.LowerControlLimit)
                    ucl(i) = ChartValue(point.UpperControlLimit)
                    lowerOne(i) = ChartValue(point.LowerOneSigmaLimit)
                    upperOne(i) = ChartValue(point.UpperOneSigmaLimit)
                    lowerTwo(i) = ChartValue(point.LowerTwoSigmaLimit)
                    upperTwo(i) = ChartValue(point.UpperTwoSigmaLimit)
                    signals(i) = If(point.IsSignalled,
                                    ChartValue(point.Value),
                                    MissingChartValue())
                    exclusions(i) = If(point.IsExplicitlyExcluded,
                                       ChartValue(point.Value),
                                       MissingChartValue())
                Else
                    statistic(i) = MissingChartValue()
                    center(i) = MissingChartValue()
                    lcl(i) = MissingChartValue()
                    ucl(i) = MissingChartValue()
                    lowerOne(i) = MissingChartValue()
                    upperOne(i) = MissingChartValue()
                    lowerTwo(i) = MissingChartValue()
                    upperTwo(i) = MissingChartValue()
                    signals(i) = MissingChartValue()
                    exclusions(i) = MissingChartValue()
                End If
            Next

            Return New PanelChartData(points,
                                      statistic,
                                      center,
                                      lcl,
                                      ucl,
                                      lowerOne,
                                      upperOne,
                                      lowerTwo,
                                      upperTwo,
                                      signals,
                                      exclusions)
        End Function

        Public Shared Sub AddPointLabels(series As List(Of Spc22LegacyLabel),
                                          points As SpcPointResult(),
                                          options As SpcControlChartAppearanceOptions)
            For i As Integer = 0 To points.Length - 1
                Dim pointResult As SpcPointResult = points(i)
                If pointResult IsNot Nothing AndAlso pointResult.HasFiniteValue Then
                    SetPointLabel(series,
                                  i,
                                  pointResult.Label,
                                  options.StatisticColor,
                                  options.DataLabelFontSize,
                                  XlDataLabelPosition.xlLabelPositionAbove)
                End If
            Next
        End Sub

        Public Shared Sub AddSignalLabels(series As List(Of Spc22LegacyLabel),
                                           points As SpcPointResult(),
                                           options As SpcControlChartAppearanceOptions)
            For i As Integer = 0 To points.Length - 1
                Dim pointResult As SpcPointResult = points(i)
                If pointResult IsNot Nothing AndAlso
                   pointResult.HasFiniteValue AndAlso
                   pointResult.IsSignalled Then
                    SetPointLabel(series,
                                  i,
                                  FormatRuleNumbers(pointResult.SignalRuleNumbers),
                                  options.SignalColor,
                                  options.DataLabelFontSize,
                                  XlDataLabelPosition.xlLabelPositionAbove)
                End If
            Next
        End Sub

        Public Shared Sub AddExclusionLabels(series As List(Of Spc22LegacyLabel),
                                              points As SpcPointResult(),
                                              options As SpcControlChartAppearanceOptions)
            For i As Integer = 0 To points.Length - 1
                Dim pointResult As SpcPointResult = points(i)
                If pointResult IsNot Nothing AndAlso
                   pointResult.HasFiniteValue AndAlso
                   pointResult.IsExplicitlyExcluded Then
                    Dim text As String = pointResult.ExclusionReason
                    If text.Length = 0 Then text = "Excluded"
                    SetPointLabel(series,
                                  i,
                                  text,
                                  options.ExclusionColor,
                                  options.DataLabelFontSize,
                                  XlDataLabelPosition.xlLabelPositionBelow)
                End If
            Next
        End Sub

        Public Shared Function FormatRuleNumbers(ruleNumbers As Integer()) As String
            If ruleNumbers Is Nothing OrElse ruleNumbers.Length = 0 Then Return String.Empty
            Dim parts(ruleNumbers.Length - 1) As String
            For i As Integer = 0 To ruleNumbers.Length - 1
                parts(i) = ruleNumbers(i).ToString(CultureInfo.InvariantCulture)
            Next
            Return "R" & String.Join(",", parts)
        End Function

        Public Shared Function ChartValue(value As Double) As Object
            If IsFinite(value) Then Return value
            Return MissingChartValue()
        End Function

        Public Shared Function MissingChartValue() As Object
            'Nothing marshals as a blank VARIANT and is accepted by Series.Values.
            'Chart.DisplayBlanksAs controls whether the point is plotted.
            Return Nothing
        End Function

        Public Shared Function IsFinite(value As Double) As Boolean
            Return Not Double.IsNaN(value) AndAlso Not Double.IsInfinity(value)
        End Function

        Public Shared Function HasValidZones(point As SpcPointResult) As Boolean
            If point Is Nothing Then Return False
            Dim values As Double() = {
                point.LowerControlLimit,
                point.LowerTwoSigmaLimit,
                point.LowerOneSigmaLimit,
                point.CenterLine,
                point.UpperOneSigmaLimit,
                point.UpperTwoSigmaLimit,
                point.UpperControlLimit
            }
            For i As Integer = 0 To values.Length - 1
                If Not IsFinite(values(i)) Then Return False
                If i > 0 AndAlso values(i) < values(i - 1) Then Return False
            Next
            Return True
        End Function

        Public Shared Function ComputeYAxisBounds(
            panel As SpcPanelResult,
            data As PanelChartData,
            specifications As SpcSpecificationLimits,
            options As SpcControlChartAppearanceOptions) As AxisBounds

            Dim minimum As Double = Double.PositiveInfinity
            Dim maximum As Double = Double.NegativeInfinity
            Dim valueSets As Object()() = {
                data.StatisticValues,
                data.CenterLineValues,
                data.LowerControlLimitValues,
                data.UpperControlLimitValues,
                data.LowerOneSigmaValues,
                data.UpperOneSigmaValues,
                data.LowerTwoSigmaValues,
                data.UpperTwoSigmaValues
            }

            For setIndex As Integer = 0 To valueSets.Length - 1
                IncludeValuesInBounds(valueSets(setIndex), minimum, maximum)
            Next

            If PanelSupportsSpecificationLimits(panel.PanelType) Then
                If options.ShowSpecificationLimits Then
                    IncludeNullableInBounds(specifications.LowerSpecificationLimit,
                                            minimum,
                                            maximum)
                    IncludeNullableInBounds(specifications.UpperSpecificationLimit,
                                            minimum,
                                            maximum)
                End If
                If options.ShowTargetLine Then
                    IncludeNullableInBounds(specifications.Target, minimum, maximum)
                End If
            End If

            If Double.IsPositiveInfinity(minimum) OrElse
               Double.IsNegativeInfinity(maximum) Then
                Return New AxisBounds(0.0R, 1.0R)
            End If

            If minimum = maximum Then
                Dim scale As Double = Math.Max(1.0R, Math.Abs(minimum))
                minimum -= 0.1R * scale
                maximum += 0.1R * scale
            Else
                Dim padding As Double = 0.07R * (maximum - minimum)
                minimum -= padding
                maximum += padding
            End If

            If HasNaturalZero(panel.PanelType) AndAlso minimum >= -0.15R * Math.Max(1.0R, maximum) Then
                minimum = 0.0R
            End If
            If panel.PanelType = SpcPanelType.Proportion AndAlso
               maximum > 1.0R AndAlso maximum <= 1.1R Then
                maximum = 1.0R
            End If
            If maximum <= minimum Then maximum = minimum + 1.0R

            Return New AxisBounds(minimum, maximum)
        End Function

        Public Shared Sub IncludeValuesInBounds(values As Object(),
                                                 ByRef minimum As Double,
                                                 ByRef maximum As Double)
            For i As Integer = 0 To values.Length - 1
                If IsNumericChartValue(values(i)) Then
                    Dim value As Double = Convert.ToDouble(values(i), CultureInfo.InvariantCulture)
                    If value < minimum Then minimum = value
                    If value > maximum Then maximum = value
                End If
            Next
        End Sub

        Public Shared Sub IncludeNullableInBounds(value As Nullable(Of Double),
                                                   ByRef minimum As Double,
                                                   ByRef maximum As Double)
            If Not value.HasValue Then Return
            If value.Value < minimum Then minimum = value.Value
            If value.Value > maximum Then maximum = value.Value
        End Sub

        Public Shared Function PanelSupportsSpecificationLimits(
            panelType As SpcPanelType) As Boolean
            Select Case panelType
                Case SpcPanelType.Run,
                     SpcPanelType.IndividualValue,
                     SpcPanelType.SubgroupMean,
                     SpcPanelType.Ewma,
                     SpcPanelType.MovingAverage
                    Return True
                Case Else
                    Return False
            End Select
        End Function

        Public Shared Function HasNaturalZero(panelType As SpcPanelType) As Boolean
            Select Case panelType
                Case SpcPanelType.MovingRange,
                     SpcPanelType.SubgroupRange,
                     SpcPanelType.SubgroupStandardDeviation,
                     SpcPanelType.Proportion,
                     SpcPanelType.NumberNonconforming,
                     SpcPanelType.DefectCount,
                     SpcPanelType.DefectRate,
                     SpcPanelType.EventsBetweenOccurrences,
                     SpcPanelType.TimeBetweenOccurrences,
                     SpcPanelType.UpperCusum,
                     SpcPanelType.LowerCusum,
                     SpcPanelType.HotellingT2,
                     SpcPanelType.GeneralizedVariance,
                     SpcPanelType.PcaT2,
                     SpcPanelType.PcaQ,
                     SpcPanelType.Mewma,
                     SpcPanelType.Mcusum
                    Return True
                Case Else
                    Return False
            End Select
        End Function

        Public Shared Function IsNumericChartValue(value As Object) As Boolean
            If value Is Nothing OrElse Not IsNumeric(value) Then Return False
            Dim numericValue As Double
            Try
                numericValue = Convert.ToDouble(value, CultureInfo.InvariantCulture)
            Catch
                Return False
            End Try
            Return IsFinite(numericValue)
        End Function
        Public NotInheritable Class SharedAxisContext
            Public Sub New(pointIndices As Integer(),
                           categories As Object(),
                           stageIds As String())
                Me.PointIndices = pointIndices
                Me.Categories = categories
                Me.StageIds = stageIds
            End Sub

            Public ReadOnly PointIndices As Integer()
            Public ReadOnly Categories As Object()
            Public ReadOnly StageIds As String()
        End Class

        Public NotInheritable Class PanelChartData
            Public Sub New(points As SpcPointResult(),
                           statisticValues As Object(),
                           centerLineValues As Object(),
                           lowerControlLimitValues As Object(),
                           upperControlLimitValues As Object(),
                           lowerOneSigmaValues As Object(),
                           upperOneSigmaValues As Object(),
                           lowerTwoSigmaValues As Object(),
                           upperTwoSigmaValues As Object(),
                           signalValues As Object(),
                           excludedValues As Object())
                Me.Points = points
                Me.StatisticValues = statisticValues
                Me.CenterLineValues = centerLineValues
                Me.LowerControlLimitValues = lowerControlLimitValues
                Me.UpperControlLimitValues = upperControlLimitValues
                Me.LowerOneSigmaValues = lowerOneSigmaValues
                Me.UpperOneSigmaValues = upperOneSigmaValues
                Me.LowerTwoSigmaValues = lowerTwoSigmaValues
                Me.UpperTwoSigmaValues = upperTwoSigmaValues
                Me.SignalValues = signalValues
                Me.ExcludedValues = excludedValues
            End Sub

            Public ReadOnly Points As SpcPointResult()
            Public ReadOnly StatisticValues As Object()
            Public ReadOnly CenterLineValues As Object()
            Public ReadOnly LowerControlLimitValues As Object()
            Public ReadOnly UpperControlLimitValues As Object()
            Public ReadOnly LowerOneSigmaValues As Object()
            Public ReadOnly UpperOneSigmaValues As Object()
            Public ReadOnly LowerTwoSigmaValues As Object()
            Public ReadOnly UpperTwoSigmaValues As Object()
            Public ReadOnly SignalValues As Object()
            Public ReadOnly ExcludedValues As Object()
        End Class

        Public NotInheritable Class AxisBounds
            Public Sub New(minimum As Double, maximum As Double)
                Me.Minimum = minimum
                Me.Maximum = maximum
            End Sub

            Public ReadOnly Minimum As Double
            Public ReadOnly Maximum As Double
        End Class
End Class
