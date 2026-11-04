Option Explicit On
Option Strict On
Imports System
Imports System.Collections.Generic
Imports System.Globalization
Imports System.Linq
Imports BESHStatNG.StatisticalProcessControl

Namespace ExcelCommon
    Public NotInheritable Class SpcStageRowIntent
        Public Sub New(id As String, name As String, first As Integer, last As Integer, phase As SpcPhase, mode As SpcStageLimitMode, reference As String)
            StageId = id : DisplayName = name : FirstPoint = first : LastPoint = last
            Me.Phase = phase : LimitMode = mode : ReferenceStageId = reference
        End Sub
        Public ReadOnly Property StageId As String
        Public ReadOnly Property DisplayName As String
        Public ReadOnly Property FirstPoint As Integer
        Public ReadOnly Property LastPoint As Integer
        Public ReadOnly Property Phase As SpcPhase
        Public ReadOnly Property LimitMode As SpcStageLimitMode
        Public ReadOnly Property ReferenceStageId As String
    End Class
    Public NotInheritable Class SpcImportedStageIntent
        Friend Sub New(stages() As String, phases() As SpcPhase)
            StageIds = Array.AsReadOnly(CType(stages.Clone(), String()))
            Me.Phases = Array.AsReadOnly(CType(phases.Clone(), SpcPhase()))
        End Sub
        Public ReadOnly Property StageIds As IReadOnlyList(Of String)
        Public ReadOnly Property Phases As IReadOnlyList(Of SpcPhase)
    End Class
    Public NotInheritable Class SpcPhaseStagePolicy
        Private Sub New()
        End Sub
        Public Shared Iterator Function Quick(pointCount As Integer, singlePhaseI As Boolean, lastPhaseI As Integer, source As SpcParameterSource) As IEnumerable(Of SpcStageRowIntent)
            Dim mode = If(source = SpcParameterSource.UseHistoricalParameters, SpcStageLimitMode.UseHistoricalParameters, SpcStageLimitMode.EstimateFromStageData)
            If singlePhaseI Then
                Yield New SpcStageRowIntent("PhaseI", "Phase I", 1, pointCount, SpcPhase.PhaseI, mode, String.Empty)
            Else
                If lastPhaseI < 1 OrElse lastPhaseI >= pointCount Then Throw New ArgumentException("The last Phase I point must be between 1 and " & (pointCount - 1).ToString(CultureInfo.CurrentCulture) & ".")
                Yield New SpcStageRowIntent("PhaseI", "Phase I", 1, lastPhaseI, SpcPhase.PhaseI, mode, String.Empty)
                Yield New SpcStageRowIntent("PhaseII", "Phase II", lastPhaseI + 1, pointCount, SpcPhase.PhaseII,
                    If(source = SpcParameterSource.UseHistoricalParameters, SpcStageLimitMode.UseHistoricalParameters, SpcStageLimitMode.UseReferenceStage), If(source = SpcParameterSource.UseHistoricalParameters, String.Empty, "PhaseI"))
            End If
        End Function
    Public Shared Function ParsePhaseText(text As String) As SpcPhase
        Dim normalized As String = text.Trim().Replace(" ", String.Empty).ToUpperInvariant()
        If normalized = "PHASEI" OrElse normalized = "I" OrElse normalized = "1" Then Return SpcPhase.PhaseI
        If normalized = "PHASEII" OrElse normalized = "II" OrElse normalized = "2" Then Return SpcPhase.PhaseII
        Throw New ArgumentException("Phase must be 'Phase I' or 'Phase II'.")
    End Function
        Public Shared Function Collapse(map As SpcSourcePointMap, rowStages() As String, rowPhases() As String) As SpcImportedStageIntent
            If map Is Nothing Then Throw New ArgumentNullException(NameOf(map))
            If rowStages Is Nothing AndAlso rowPhases Is Nothing Then Throw New ArgumentException("Select a stage identifier column, a Phase I/Phase II column, or both.")
            If rowStages IsNot Nothing AndAlso rowStages.Length <> map.PointIndexByRow.Count Then Throw New ArgumentException("Stage values must align with source rows.", NameOf(rowStages))
            If rowPhases IsNot Nothing AndAlso rowPhases.Length <> map.PointIndexByRow.Count Then Throw New ArgumentException("Phase values must align with source rows.", NameOf(rowPhases))
            Dim stages(map.PointCount - 1) As String, phases(map.PointCount - 1) As SpcPhase
            For point = 0 To map.PointCount - 1
                If rowStages IsNot Nothing Then
                    Dim found As String = Nothing
                    For Each row In map.Points(point).InputRowIndices
                        Dim current = rowStages(row).Trim()
                        If found Is Nothing Then
                            found = current
                        ElseIf Not String.Equals(found, current, StringComparison.Ordinal) Then
                            Throw New ArgumentException("Rows belonging to chart point " & (point + 1).ToString() & " contain inconsistent stage identifier values.")
                        End If
                    Next
                    stages(point) = If(found, String.Empty)
                    If stages(point).Trim().Length = 0 Then Throw New ArgumentException("The stage identifier is blank at chart point " & (point + 1).ToString() & ".")
                End If
                If rowPhases Is Nothing Then
                    phases(point) = SpcPhase.PhaseI
                Else
                    Dim found As SpcPhase? = Nothing
                    For Each row In map.Points(point).InputRowIndices
                        Dim current = ParsePhaseText(rowPhases(row))
                        If found.HasValue AndAlso found.Value <> current Then Throw New ArgumentException("Rows belonging to chart point " & (point + 1).ToString() & " contain inconsistent phase values.")
                        found = current
                    Next
                    If Not found.HasValue Then Throw New ArgumentException("No phase value is available for chart point " & (point + 1).ToString() & ".")
                    phases(point) = found.Value
                End If
            Next
            If rowStages Is Nothing Then
                Dim phaseRunCounts As New Dictionary(Of SpcPhase, Integer)()
                Dim runStartPoint As Integer = 0
                While runStartPoint < map.PointCount
                    Dim phase As SpcPhase = phases(runStartPoint)
                    Dim runEndPoint As Integer = runStartPoint
                    While runEndPoint + 1 < map.PointCount AndAlso phases(runEndPoint + 1) = phase
                        runEndPoint += 1
                    End While
                    Dim runNumber As Integer = 1
                    If phaseRunCounts.ContainsKey(phase) Then runNumber = phaseRunCounts(phase) + 1
                    phaseRunCounts(phase) = runNumber
                    Dim baseId As String = If(phase = SpcPhase.PhaseI, "PhaseI", "PhaseII")
                    Dim generatedId As String = If(runNumber = 1, baseId, baseId & "-" & runNumber.ToString(CultureInfo.InvariantCulture))
                    For point As Integer = runStartPoint To runEndPoint
                        stages(point) = generatedId
                    Next
                    runStartPoint = runEndPoint + 1
                End While
            End If

            Return New SpcImportedStageIntent(stages, phases)
        End Function
        Public Shared Iterator Function ImportedRows(intent As SpcImportedStageIntent, source As SpcParameterSource) As IEnumerable(Of SpcStageRowIntent)
            If intent Is Nothing Then Throw New ArgumentNullException(NameOf(intent))
            Dim pointStages = intent.StageIds, pointPhases = intent.Phases
            Dim usedIds As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
            Dim runStart As Integer = 0
            Dim lastPhaseIId As String = String.Empty
            For point As Integer = 1 To intent.StageIds.Count
                Dim atEnd As Boolean = point = intent.StageIds.Count
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

                Yield New SpcStageRowIntent(stageId, stageId, runStart + 1, point, phase, limitMode, referenceId)
                runStart = point
            Next
        End Function
        Public Shared Function Manual(row As SpcStageRowIntent, logicalPointCount As Integer) As SpcStageDefinition
            If row Is Nothing Then Throw New ArgumentNullException(NameOf(row))
            If row.LastPoint > logicalPointCount Then Throw New ArgumentException("Stage '" & row.StageId & "' ends after the last chart point (" & logicalPointCount.ToString(CultureInfo.CurrentCulture) & ").")
            Return New SpcStageDefinition(row.StageId, row.FirstPoint - 1, row.LastPoint - 1, row.Phase, row.LimitMode, If(row.ReferenceStageId.Length = 0, Nothing, row.ReferenceStageId), row.DisplayName)
        End Function
        Public Shared Function FinishManual(stages As IEnumerable(Of SpcStageDefinition), source As SpcParameterSource) As SpcStageDefinition()
            Dim result = stages.ToArray()
            If result.Length = 0 AndAlso source = SpcParameterSource.DefinedByStage Then Throw New ArgumentException("Define at least one stage when 'Defined by stage' is selected.")
            Return result
        End Function
    End Class
End Namespace
