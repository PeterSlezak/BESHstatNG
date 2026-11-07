Option Explicit On
Option Strict On
Option Infer On
Imports System
Imports System.Linq
Imports System.Globalization
Imports System.Collections.Generic
Imports BESHStatNG
Imports BESHStatNG.StatisticalProcessControl
Namespace Global.Legacy36
Public Module Original36
Public Function KM0(plotData As survival.KaplanMeierPlotData, group As Integer) As Object()
Dim probability(),time(),upCI(),lowCI(),cenProbability(),cenTimes() As Double
Dim bCen As Boolean
Dim plotUpper As Integer = plotData.SurvivalProbability.GetUpperBound(0)
                        ReDim probability(plotUpper + 1)
                        ReDim time(plotUpper + 1)
                        probability(0) = 1
                        time(0) = 0

                        For j As Integer = 0 To plotUpper
                            If plotData.SurvivalTime(j) <= plotData.MaximumTimeByGroup(group) Then
                                probability(j + 1) = plotData.SurvivalProbability(j, group)
                                time(j + 1) = plotData.SurvivalTime(j)
                            Else
                                ReDim Preserve probability(j)
                                ReDim Preserve time(j)
                                Exit For
                            End If
                        Next

Return New Object(){time,probability,True}
End Function
Public Function KM1(plotData As survival.KaplanMeierPlotData, group As Integer) As Object()
Dim probability(),time(),upCI(),lowCI(),cenProbability(),cenTimes() As Double
Dim bCen As Boolean
Dim groupIndex As Integer = group
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

Return New Object(){cenTimes,cenProbability,bCen}
End Function
Public Function KM2(plotData As survival.KaplanMeierPlotData, group As Integer) As Object()
Dim probability(),time(),upCI(),lowCI(),cenProbability(),cenTimes() As Double
Dim bCen As Boolean
Dim groupIndex As Integer = group
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

    Return New Object(){time,upCI,True}
End Function
Public Function KM3(plotData As survival.KaplanMeierPlotData, group As Integer) As Object()
Dim probability(),time(),upCI(),lowCI(),cenProbability(),cenTimes() As Double
Dim bCen As Boolean
Dim groupIndex As Integer = group
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

    Return New Object(){time,lowCI,True}
End Function
Public Function Bands(points As SpcPointResult(), minimum As Double) As Object()
Dim categories(points.Length-1) As Object
            Dim baseValues(categories.Length - 1) As Object
            Dim lowerOuter(categories.Length - 1) As Object
            Dim lowerMiddle(categories.Length - 1) As Object
            Dim lowerCenter(categories.Length - 1) As Object
            Dim upperCenter(categories.Length - 1) As Object
            Dim upperMiddle(categories.Length - 1) As Object
            Dim upperOuter(categories.Length - 1) As Object
            Dim validBandCount As Integer = 0

            For i As Integer = 0 To categories.Length - 1
                Dim point As SpcPointResult = points(i)
                If point IsNot Nothing AndAlso HasValidZones(point) Then
                    baseValues(i) = point.LowerControlLimit - minimum
                    lowerOuter(i) = point.LowerTwoSigmaLimit - point.LowerControlLimit
                    lowerMiddle(i) = point.LowerOneSigmaLimit - point.LowerTwoSigmaLimit
                    lowerCenter(i) = point.CenterLine - point.LowerOneSigmaLimit
                    upperCenter(i) = point.UpperOneSigmaLimit - point.CenterLine
                    upperMiddle(i) = point.UpperTwoSigmaLimit - point.UpperOneSigmaLimit
                    upperOuter(i) = point.UpperControlLimit - point.UpperTwoSigmaLimit
                    validBandCount += 1
                Else
                    baseValues(i) = MissingChartValue()
                    lowerOuter(i) = MissingChartValue()
                    lowerMiddle(i) = MissingChartValue()
                    lowerCenter(i) = MissingChartValue()
                    upperCenter(i) = MissingChartValue()
                    upperMiddle(i) = MissingChartValue()
                    upperOuter(i) = MissingChartValue()
                End If
            Next


Return New Object(){validBandCount,baseValues,lowerOuter,lowerMiddle,lowerCenter,upperCenter,upperMiddle,upperOuter}
End Function

        Public Function HasValidZones(point As SpcPointResult) As Boolean
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

        Public Function IsFinite(value As Double) As Boolean
            Return Not Double.IsNaN(value) AndAlso Not Double.IsInfinity(value)
        End Function





        Public Function MissingChartValue() As Object
            'Nothing marshals as a blank VARIANT and is accepted by Series.Values.
            'Chart.DisplayBlanksAs controls whether the point is plotted.
            Return Nothing
        End Function

        Public Function IsNumericChartValue(value As Object) As Boolean
            If value Is Nothing OrElse Not IsNumeric(value) Then Return False
            Dim numericValue As Double
            Try
                numericValue = Convert.ToDouble(value, CultureInfo.InvariantCulture)
            Catch
                Return False
            End Try
            Return IsFinite(numericValue)
        End Function

        Public Function HasAnyValue(values As Object()) As Boolean
            For i As Integer = 0 To values.Length - 1
                If IsNumericChartValue(values(i)) Then Return True
            Next
            Return False
        End Function
Public Function LastIndex(values As Object()) As Integer
For i As Integer=values.Length-1 To 0 Step -1
If IsNumericChartValue(values(i)) Then Return i
Next
Return -1
End Function
Public Function Cox(time As Double(), probability As Double()) As Object()
If time Is Nothing Then Throw New ArgumentNullException("survTime")
If probability Is Nothing Then Throw New ArgumentNullException("survProb")
If time.Length<>probability.Length Then Throw New ArgumentException("survTime and survProb must have the same length.")
If time.Length=0 Then Return Nothing
Dim scale=ChartAxisScalingCore.Compute(0,time.Max())
Return New Object(){time,probability,scale.Min,scale.Max,scale.Scale,0.0,1.0,0.2,"Baseline","Cox - Survival plot","Survival Probability","Time"}
End Function
End Module
End Namespace
