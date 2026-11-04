Option Explicit On
Option Strict On
Imports System
Imports System.Collections
Imports System.Collections.Generic
Imports System.Globalization
Imports System.IO
Imports System.Linq
Imports System.Security.Cryptography
Imports System.Text
Imports System.Xml.Linq
Imports BESHStatNG
Imports BESHStatNG.ExcelCommon
Imports BESHStatNG.StatisticalProcessControl
Imports Microsoft.VisualStudio.TestTools.UnitTesting

Public NotInheritable Class Spc21LegacyPreparation
    Public Shared Function ParsePhaseText(text As String) As SpcPhase
        Dim normalized As String = text.Trim().Replace(" ", String.Empty).ToUpperInvariant()
        If normalized = "PHASEI" OrElse normalized = "I" OrElse normalized = "1" Then Return SpcPhase.PhaseI
        If normalized = "PHASEII" OrElse normalized = "II" OrElse normalized = "2" Then Return SpcPhase.PhaseII
        Throw New ArgumentException("Phase must be 'Phase I' or 'Phase II'.")
    End Function
    Public Shared Function ParseCollapsedPhase(values As String(), pointIndices As Integer(), point As Integer) As SpcPhase
        Dim found As Nullable(Of SpcPhase) = Nothing
        For i As Integer = 0 To values.Length - 1
            If pointIndices(i) <> point Then Continue For
            Dim current As SpcPhase = ParsePhaseText(values(i))
            If found.HasValue AndAlso found.Value <> current Then
                Throw New ArgumentException("Rows belonging to chart point " & (point + 1).ToString() & " contain inconsistent phase values.")
            End If
            found = current
        Next
        If Not found.HasValue Then Throw New ArgumentException("No phase value is available for chart point " & (point + 1).ToString() & ".")
        Return found.Value
    End Function
    Public Shared Function CollapsedTextValue(values As String(),
                                               pointIndices As Integer(),
                                               point As Integer,
                                               label As String) As String
        Dim found As String = Nothing
        For i As Integer = 0 To values.Length - 1
            If pointIndices(i) <> point Then Continue For
            Dim current As String = values(i).Trim()
            If found Is Nothing Then
                found = current
            ElseIf Not String.Equals(found, current, StringComparison.Ordinal) Then
                Throw New ArgumentException("Rows belonging to chart point " & (point + 1).ToString() &
                                            " contain inconsistent " & label & " values.")
            End If
        Next
        Return If(found, String.Empty)
    End Function
    Public Shared Function IsAttributeChart(chartType As SpcChartType) As Boolean
        Return chartType = SpcChartType.PChart OrElse
               chartType = SpcChartType.NpChart OrElse
               chartType = SpcChartType.CChart OrElse
               chartType = SpcChartType.UChart
    End Function
    Public Shared Function IsSubgroupChart(chartType As SpcChartType) As Boolean
        Return chartType = SpcChartType.XBar OrElse
               chartType = SpcChartType.SubgroupRange OrElse
               chartType = SpcChartType.SubgroupStandardDeviation OrElse
               chartType = SpcChartType.XBarR OrElse
               chartType = SpcChartType.XBarS
    End Function
    Public Shared Function IsTimeWeightedChart(chartType As SpcChartType) As Boolean
        Return chartType = SpcChartType.Cusum OrElse
               chartType = SpcChartType.Ewma OrElse
               chartType = SpcChartType.MovingAverage
    End Function
    Public Shared Function ChartCanUseSpecificationLines(chartType As SpcChartType) As Boolean
        Return chartType = SpcChartType.Individuals OrElse
               chartType = SpcChartType.IndividualsMovingRange OrElse
               chartType = SpcChartType.XBar OrElse
               chartType = SpcChartType.XBarR OrElse
               chartType = SpcChartType.XBarS OrElse
               chartType = SpcChartType.Ewma OrElse
               chartType = SpcChartType.MovingAverage
    End Function
    Public Shared Function ParseRuleKind(text As String) As SpcRuleKind
        Select Case text.Trim().ToLowerInvariant()
            Case "beyond sigma" : Return SpcRuleKind.BeyondSigma
            Case "k of m beyond sigma" : Return SpcRuleKind.KOfMConsecutiveBeyondSigma
            Case "run on one side" : Return SpcRuleKind.RunOnOneSide
            Case "monotonic trend" : Return SpcRuleKind.MonotonicTrend
            Case "alternating" : Return SpcRuleKind.Alternating
            Case "all within sigma" : Return SpcRuleKind.AllWithinSigma
            Case "all beyond sigma on both sides" : Return SpcRuleKind.AllBeyondSigmaOnBothSides
            Case Else : Throw New ArgumentException("Select a valid rule pattern.")
        End Select
    End Function
    Public Shared Function ParseRuleSide(text As String) As SpcRuleSide
        Select Case text.Trim().ToLowerInvariant()
            Case "either side" : Return SpcRuleSide.EitherSide
            Case "upper side only" : Return SpcRuleSide.UpperSideOnly
            Case "lower side only" : Return SpcRuleSide.LowerSideOnly
            Case Else : Throw New ArgumentException("Select a valid rule side.")
        End Select
    End Function
    Public Shared Function ParseRuleScope(text As String) As SpcRuleScope
        Select Case text.Trim().ToLowerInvariant()
            Case "location panels" : Return SpcRuleScope.LocationPanels
            Case "dispersion panels" : Return SpcRuleScope.DispersionPanels
            Case "attribute panels" : Return SpcRuleScope.AttributePanels
            Case "time-weighted panels" : Return SpcRuleScope.TimeWeightedPanels
            Case "location and attribute panels" : Return SpcRuleScope.LocationAndAttributePanels
            Case "all shewhart panels" : Return SpcRuleScope.AllShewhartPanels
            Case "all panels" : Return SpcRuleScope.All
            Case Else : Throw New ArgumentException("Select a valid rule scope.")
        End Select
    End Function
    Public Shared Function IsExclusionIndicator(value As Object) As Boolean
        If value Is Nothing Then Return False
        If TypeOf value Is Boolean Then Return DirectCast(value, Boolean)
        Dim numeric As Double
        If TryParseDouble(Convert.ToString(value, CultureInfo.CurrentCulture), numeric) Then Return numeric <> 0.0R
        Select Case Convert.ToString(value, CultureInfo.CurrentCulture).Trim().ToLowerInvariant()
            Case "true", "yes", "y", "x", "exclude", "excluded" : Return True
            Case "false", "no", "n", "include", "included", "" : Return False
            Case Else : Throw New ArgumentException("Exclusion indicators must be blank/0/No or nonzero/Yes/True/X.")
        End Select
    End Function
    Public Shared Function TryParseDouble(text As String, ByRef value As Double) As Boolean
        If Double.TryParse(text, NumberStyles.Float Or NumberStyles.AllowThousands, CultureInfo.CurrentCulture, value) Then Return True
        Return Double.TryParse(text, NumberStyles.Float Or NumberStyles.AllowThousands, CultureInfo.InvariantCulture, value)
    End Function
    Public Shared Function PointIndices(layout As SpcDataLayout, sources() As Integer, subgroupIds() As String) As Integer()
        Dim rowCount As Integer = sources.Length
        Dim pointIndexByRow(rowCount - 1) As Integer
        Dim pointCount As Integer
        If layout = SpcDataLayout.StackedObservations Then
            Dim groups As New Dictionary(Of String, Integer)(StringComparer.Ordinal)
            For row As Integer = 0 To subgroupIds.Length - 1
                Dim id As String = subgroupIds(row).Trim()
                If id.Length = 0 Then Throw New ArgumentException("A subgroup ID is blank at worksheet row " & sources(row).ToString() & ".")
                Dim point As Integer
                If Not groups.TryGetValue(id, point) Then
                    point = groups.Count
                    groups.Add(id, point)
                End If
                pointIndexByRow(row) = point
            Next
            pointCount = groups.Count
        Else
            For row As Integer = 0 To pointIndexByRow.Length - 1
                pointIndexByRow(row) = row
            Next
            pointCount = rowCount
        End If

        Return pointIndexByRow
    End Function
    Public Shared Function Quick(pointCount As Integer, singlePhaseI As Boolean, lastPhaseI As Integer, source As SpcParameterSource) As List(Of SpcStageRowIntent)
        Dim rows As New List(Of SpcStageRowIntent)
        Dim phaseILimitMode = If(source = SpcParameterSource.UseHistoricalParameters, SpcStageLimitMode.UseHistoricalParameters, SpcStageLimitMode.EstimateFromStageData)
            If singlePhaseI Then
                rows.Add(New SpcStageRowIntent("PhaseI", "Phase I", 1, pointCount, SpcPhase.PhaseI,
                            phaseILimitMode, String.Empty))
            Else
                If lastPhaseI < 1 OrElse lastPhaseI >= pointCount Then
                    Throw New ArgumentException("The last Phase I point must be between 1 and " &
                                                (pointCount - 1).ToString(CultureInfo.CurrentCulture) & ".")
                End If
                rows.Add(New SpcStageRowIntent("PhaseI", "Phase I", 1, lastPhaseI, SpcPhase.PhaseI,
                            phaseILimitMode, String.Empty))
                If source = SpcParameterSource.UseHistoricalParameters Then
                    rows.Add(New SpcStageRowIntent("PhaseII", "Phase II", lastPhaseI + 1, pointCount, SpcPhase.PhaseII,
                                SpcStageLimitMode.UseHistoricalParameters, String.Empty))
                Else
                    rows.Add(New SpcStageRowIntent("PhaseII", "Phase II", lastPhaseI + 1, pointCount, SpcPhase.PhaseII,
                                SpcStageLimitMode.UseReferenceStage, "PhaseI"))
                End If
            End If
        Return rows
    End Function
    Public Shared Sub Imported(pointIndices() As Integer, pointCount As Integer, rowStages() As String, rowPhases() As String, source As SpcParameterSource, rows As List(Of SpcStageRowIntent), ByRef cleared As Boolean)
            Dim pointStages(pointCount - 1) As String
            Dim pointPhases(pointCount - 1) As SpcPhase
            For point As Integer = 0 To pointCount - 1
                If rowStages IsNot Nothing Then
                    pointStages(point) = CollapsedTextValue(rowStages, pointIndices, point, "stage identifier")
                End If
                If rowStages IsNot Nothing AndAlso pointStages(point).Trim().Length = 0 Then
                    Throw New ArgumentException("The stage identifier is blank at chart point " & (point + 1).ToString() & ".")
                End If
                pointPhases(point) = If(rowPhases Is Nothing, SpcPhase.PhaseI,
                                        ParseCollapsedPhase(rowPhases, pointIndices, point))
            Next

            If rowStages Is Nothing Then
                Dim phaseRunCounts As New Dictionary(Of SpcPhase, Integer)()
                Dim runStartPoint As Integer = 0
                While runStartPoint < pointCount
                    Dim phase As SpcPhase = pointPhases(runStartPoint)
                    Dim runEndPoint As Integer = runStartPoint
                    While runEndPoint + 1 < pointCount AndAlso pointPhases(runEndPoint + 1) = phase
                        runEndPoint += 1
                    End While
                    Dim runNumber As Integer = 1
                    If phaseRunCounts.ContainsKey(phase) Then runNumber = phaseRunCounts(phase) + 1
                    phaseRunCounts(phase) = runNumber
                    Dim baseId As String = If(phase = SpcPhase.PhaseI, "PhaseI", "PhaseII")
                    Dim generatedId As String = If(runNumber = 1, baseId, baseId & "-" & runNumber.ToString(CultureInfo.InvariantCulture))
                    For point As Integer = runStartPoint To runEndPoint
                        pointStages(point) = generatedId
                    Next
                    runStartPoint = runEndPoint + 1
                End While
            End If

            rows.Clear() : cleared = True
            Dim usedIds As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
            Dim runStart As Integer = 0
            Dim lastPhaseIId As String = String.Empty
            For point As Integer = 1 To pointCount
                Dim atEnd As Boolean = point = pointCount
                Dim changed As Boolean = Not atEnd AndAlso
                    (Not String.Equals(pointStages(point), pointStages(runStart), StringComparison.Ordinal) OrElse
                     pointPhases(point) <> pointPhases(runStart))
                If Not atEnd AndAlso Not changed Then Continue For

                Dim stageId As String = pointStages(runStart).Trim()
                If Not usedIds.Add(stageId) Then
                    Throw New ArgumentException("Stage '" & stageId & "' occurs in more than one non-contiguous block.")
                End If

                Dim phase As SpcPhase = pointPhases(runStart)
                Dim limitMode As SpcStageLimitMode
                Dim referenceId As String = String.Empty
                If source = SpcParameterSource.UseHistoricalParameters Then
                    limitMode = SpcStageLimitMode.UseHistoricalParameters
                    If phase = SpcPhase.PhaseI Then lastPhaseIId = stageId
                ElseIf phase = SpcPhase.PhaseI Then
                    limitMode = SpcStageLimitMode.EstimateFromStageData
                    lastPhaseIId = stageId
                Else
                    If lastPhaseIId.Length = 0 Then
                        Throw New ArgumentException("A Phase II stage requires an earlier Phase I reference stage or historical parameters.")
                    End If
                    limitMode = SpcStageLimitMode.UseReferenceStage
                    referenceId = lastPhaseIId
                End If

                rows.Add(New SpcStageRowIntent(stageId, stageId, runStart + 1, point, phase, limitMode, referenceId))
                runStart = point
            Next
    End Sub
    Public Shared Function Exclusions(pointIndices() As Integer, pointCount As Integer, indicators() As Boolean, reasons() As String) As List(Of KeyValuePair(Of Integer, String))
            Dim excluded(pointCount - 1) As Boolean
            Dim reasonSets(pointCount - 1) As HashSet(Of String)
            For i As Integer = 0 To reasonSets.Length - 1
                reasonSets(i) = New HashSet(Of String)(StringComparer.Ordinal)
            Next
            For row As Integer = 0 To indicators.Length - 1
                Dim point As Integer = pointIndices(row)
                If indicators(row) Then excluded(point) = True
                If reasons IsNot Nothing AndAlso reasons(row).Trim().Length > 0 Then reasonSets(point).Add(reasons(row).Trim())
            Next

        Dim result As New List(Of KeyValuePair(Of Integer, String))
        For point = 0 To excluded.Length - 1
            If excluded(point) Then result.Add(New KeyValuePair(Of Integer, String)(point + 1, String.Join("; ", reasonSets(point).ToArray())))
        Next
        Return result
    End Function
    Public Shared Function ManualStage(row As SpcStageRowIntent, count As Integer) As SpcStageDefinition
        If row.LastPoint > count Then Throw New ArgumentException("Stage '" & row.StageId & "' ends after the last chart point (" & count.ToString(CultureInfo.CurrentCulture) & ").")
        Return New SpcStageDefinition(row.StageId, row.FirstPoint - 1, row.LastPoint - 1, row.Phase, row.LimitMode, If(row.ReferenceStageId.Length = 0, Nothing, row.ReferenceStageId), row.DisplayName)
    End Function
    Public Shared Function LogicalPointCount(input As SpcInputData) As Integer
        If input.Layout <> SpcDataLayout.StackedObservations Then Return input.RowCount
        Dim ids As String() = input.SubgroupIds
        Dim distinct As New HashSet(Of String)(StringComparer.Ordinal)
        For Each id As String In ids
            distinct.Add(If(id, String.Empty).Trim())
        Next
        Return distinct.Count
    End Function
End Class
