Option Explicit On
Option Strict On
Option Infer On
Imports System
Imports System.Collections.Generic
Imports System.Globalization
Imports System.Linq
Imports BESHStatNG.StatisticalProcessControl

Namespace ExcelCommon
    Public NotInheritable Class SpcMultivariateStageRow
        Public Sub New(stageId As String, firstPoint As Integer, lastPoint As Integer, phase As SpcPhase)
            Me.StageId = stageId
            Me.FirstPoint = firstPoint
            Me.LastPoint = lastPoint
            Me.Phase = phase
        End Sub
        Public ReadOnly Property StageId As String
        Public ReadOnly Property FirstPoint As Integer
        Public ReadOnly Property LastPoint As Integer
        Public ReadOnly Property Phase As SpcPhase
    End Class

    Public NotInheritable Class SpcMultivariatePointAssignments
        Friend Sub New(stages As String(), phases As SpcPhase())
            StageIds = Array.AsReadOnly(CType(stages.Clone(), String()))
            Me.Phases = Array.AsReadOnly(CType(phases.Clone(), SpcPhase()))
        End Sub
        Public ReadOnly Property StageIds As IReadOnlyList(Of String)
        Public ReadOnly Property Phases As IReadOnlyList(Of SpcPhase)
    End Class

    ''' <summary>Short-lived grid assembly; validation calls interleave with native cell parsing.</summary>
    Public NotInheritable Class SpcMultivariateStageBuilder
        Private ReadOnly _stages As String()
        Private ReadOnly _phases As SpcPhase()
        Private ReadOnly _assigned As Boolean()
        Private ReadOnly _used As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
        Private _count As Integer
        Public Sub New(pointCount As Integer)
            _stages = New String(pointCount - 1) {}
            _phases = New SpcPhase(pointCount - 1) {}
            _assigned = New Boolean(pointCount - 1) {}
        End Sub
        Public Sub BeginStage(id As String)
            _count += 1
            If id.Length = 0 Then Throw New ArgumentException("Every stage requires a nonblank stage ID.")
            If Not _used.Add(id) Then Throw New ArgumentException("Stage ID '" & id & "' is duplicated.")
        End Sub
        Public Sub ValidateRange(id As String, firstPoint As Integer, lastPoint As Integer)
            If firstPoint < 1 OrElse lastPoint < firstPoint OrElse lastPoint > _stages.Length Then
                Throw New ArgumentException("Stage '" & id & "' must use a valid one-based point range within 1 to " & _stages.Length.ToString() & ".")
            End If
        End Sub
        Public Sub Assign(id As String, firstPoint As Integer, lastPoint As Integer, phase As SpcPhase)
            For point As Integer = firstPoint - 1 To lastPoint - 1
                If _assigned(point) Then Throw New ArgumentException("Chart point " & (point + 1).ToString() & " belongs to more than one stage.")
                _assigned(point) = True
                _stages(point) = id
                _phases(point) = phase
            Next
        End Sub
        Public Function Finish() As SpcMultivariatePointAssignments
            If _count = 0 Then
                For point As Integer = 0 To _stages.Length - 1
                    _stages(point) = "Stage1"
                    _phases(point) = SpcPhase.PhaseI
                Next
            Else
                For point As Integer = 0 To _stages.Length - 1
                    If Not _assigned(point) Then Throw New ArgumentException("Chart point " & (point + 1).ToString() & " is not covered by the stage grid.")
                Next
            End If
            Return New SpcMultivariatePointAssignments(_stages, _phases)
        End Function
    End Class

    Public NotInheritable Class SpcMultivariatePointExclusions
        Friend Sub New(scopes As SpcExclusionScope(), reasons As String())
            Me.Scopes = Array.AsReadOnly(CType(scopes.Clone(), SpcExclusionScope()))
            Me.Reasons = Array.AsReadOnly(CType(reasons.Clone(), String()))
        End Sub
        Public ReadOnly Property Scopes As IReadOnlyList(Of SpcExclusionScope)
        Public ReadOnly Property Reasons As IReadOnlyList(Of String)
    End Class

    Public NotInheritable Class SpcMultivariateExclusionBuilder
        Private ReadOnly _scopes As SpcExclusionScope()
        Private ReadOnly _reasons As HashSet(Of String)()
        Public Sub New(pointCount As Integer)
            _scopes = New SpcExclusionScope(pointCount - 1) {}
            _reasons = New HashSet(Of String)(pointCount - 1) {}
            For i As Integer = 0 To pointCount - 1
                _reasons(i) = New HashSet(Of String)(StringComparer.Ordinal)
            Next
        End Sub
        Public Function CoreIndex(displayPoint As Integer) As Integer
            If displayPoint < 1 OrElse displayPoint > _scopes.Length Then Throw New ArgumentException("Exclusion point must be between 1 and " & _scopes.Length.ToString() & ".")
            Return displayPoint - 1
        End Function
        Public Sub IncludeScope(index As Integer, scope As SpcExclusionScope)
            _scopes(index) = _scopes(index) Or scope
        End Sub
        Public Sub IncludeReason(index As Integer, reason As String)
            If reason.Length > 0 Then _reasons(index).Add(reason)
        End Sub
        Public Function Finish() As SpcMultivariatePointExclusions
            Dim reasons(_reasons.Length - 1) As String
            For i As Integer = 0 To reasons.Length - 1
                reasons(i) = String.Join("; ", _reasons(i).ToArray())
            Next
            Return New SpcMultivariatePointExclusions(_scopes, reasons)
        End Function
    End Class

    Public NotInheritable Class SpcMultivariateImportedExclusion
        Friend Sub New(point As Integer, reason As String)
            DisplayPoint = point
            Me.Reason = reason
        End Sub
        Public ReadOnly Property DisplayPoint As Integer
        Public ReadOnly Property Reason As String
    End Class

    ''' <summary>Typed per-row import after Windows indicator coercion; no cell interpretation.</summary>
    Public NotInheritable Class SpcMultivariateExclusionImport
        Private ReadOnly _map As SpcMultivariatePointMap
        Private ReadOnly _excluded As Boolean()
        Private ReadOnly _reasons As HashSet(Of String)()
        Public Sub New(map As SpcMultivariatePointMap)
            _map = map
            _excluded = New Boolean(map.PointCount - 1) {}
            _reasons = New HashSet(Of String)(map.PointCount - 1) {}
            For i As Integer = 0 To _reasons.Length - 1
                _reasons(i) = New HashSet(Of String)(StringComparer.Ordinal)
            Next
        End Sub
        Public Sub IncludeRow(row As Integer, excluded As Boolean, reasons As String())
            Dim point = _map.PointIndexByRow(row)
            If excluded Then
                _excluded(point) = True
                If reasons IsNot Nothing AndAlso reasons(row).Length > 0 Then _reasons(point).Add(reasons(row))
            End If
        End Sub
        Public Function Rows() As IReadOnlyList(Of SpcMultivariateImportedExclusion)
            Dim values As New List(Of SpcMultivariateImportedExclusion)
            For point As Integer = 0 To _excluded.Length - 1
                If Not _excluded(point) Then Continue For
                values.Add(New SpcMultivariateImportedExclusion(point + 1, String.Join("; ", _reasons(point).ToArray())))
            Next
            Return values.AsReadOnly()
        End Function
    End Class

    Public Enum SpcMultivariateQuickPhase
        AllPhaseI
        PhaseIThenPhaseII
        AllPhaseII
    End Enum

    Public NotInheritable Class SpcMultivariatePhasePolicy
        Private Sub New()
        End Sub
        Public Shared Function ParsePhase(text As String) As SpcPhase
            Dim normalized = text.Trim().Replace(" ", String.Empty).ToUpperInvariant()
            If normalized = "PHASEI" OrElse normalized = "I" OrElse normalized = "1" Then Return SpcPhase.PhaseI
            If normalized = "PHASEII" OrElse normalized = "II" OrElse normalized = "2" Then Return SpcPhase.PhaseII
            Throw New ArgumentException("Phase must be 'Phase I' or 'Phase II'.")
        End Function
        Public Shared Function Quick(pointCount As Integer, mode As SpcMultivariateQuickPhase, Optional lastPhaseI As Integer = 1, Optional modelSource As SpcMultivariateModelSource = SpcMultivariateModelSource.EstimateFromPhaseI) As IReadOnlyList(Of SpcMultivariateStageRow)
            Dim rows As New List(Of SpcMultivariateStageRow)
            If mode = SpcMultivariateQuickPhase.AllPhaseI Then
                rows.Add(New SpcMultivariateStageRow("PhaseI", 1, pointCount, SpcPhase.PhaseI))
            ElseIf mode = SpcMultivariateQuickPhase.AllPhaseII Then
                If modelSource <> SpcMultivariateModelSource.UseHistoricalParameters Then Throw New ArgumentException("All-Phase-II monitoring requires historical parameters.")
                rows.Add(New SpcMultivariateStageRow("PhaseII", 1, pointCount, SpcPhase.PhaseII))
            Else
                If lastPhaseI < 1 OrElse lastPhaseI >= pointCount Then Throw New ArgumentException("The last Phase I point must be between 1 and " & (pointCount - 1).ToString(CultureInfo.CurrentCulture) & ".")
                rows.Add(New SpcMultivariateStageRow("PhaseI", 1, lastPhaseI, SpcPhase.PhaseI))
                rows.Add(New SpcMultivariateStageRow("PhaseII", lastPhaseI + 1, pointCount, SpcPhase.PhaseII))
            End If
            Return rows.AsReadOnly()
        End Function
        Public Shared Function CollapseImported(map As SpcMultivariatePointMap, rowStages As String(), rowPhases As String()) As SpcMultivariatePointAssignments
            Dim stages(map.PointCount - 1) As String
            Dim phases(map.PointCount - 1) As SpcPhase
            For point As Integer = 0 To map.PointCount - 1
                If rowPhases Is Nothing Then
                    phases(point) = SpcPhase.PhaseI
                Else
                    Dim found As SpcPhase? = Nothing
                    For Each row In map.Points(point).InputRowIndices
                        Dim current = ParsePhase(rowPhases(row))
                        If found.HasValue AndAlso found.Value <> current Then Throw New ArgumentException("Rows belonging to chart point " & (point + 1).ToString() & " contain inconsistent phase values.")
                        found = current
                    Next
                    If Not found.HasValue Then Throw New ArgumentException("No phase value is available for chart point " & (point + 1).ToString() & ".")
                    phases(point) = found.Value
                End If
                If rowStages IsNot Nothing Then
                    stages(point) = map.CollapsedText(rowStages, point, "stage identifier")
                    If stages(point).Length = 0 Then Throw New ArgumentException("The stage identifier is blank at chart point " & (point + 1).ToString() & ".")
                End If
            Next
            If rowStages Is Nothing Then GenerateStageIdsFromPhaseRuns(stages, phases)
            Return New SpcMultivariatePointAssignments(stages, phases)
        End Function
        Public Shared Sub GenerateStageIdsFromPhaseRuns(stageIds As String(), phases As SpcPhase())
            Dim phaseCounts As New Dictionary(Of SpcPhase, Integer)
            Dim start As Integer = 0
            While start < phases.Length
                Dim phase = phases(start), finish = start
                While finish + 1 < phases.Length AndAlso phases(finish + 1) = phase
                    finish += 1
                End While
                Dim number = If(phaseCounts.ContainsKey(phase), phaseCounts(phase) + 1, 1)
                phaseCounts(phase) = number
                Dim baseId = If(phase = SpcPhase.PhaseI, "PhaseI", "PhaseII")
                Dim id = If(number = 1, baseId, baseId & "-" & number.ToString(CultureInfo.InvariantCulture))
                For i As Integer = start To finish
                    stageIds(i) = id
                Next
                start = finish + 1
            End While
        End Sub
        ''' <summary>Lazy to preserve clearing/add-row chronology and partial grid state on failure.</summary>
        Public Shared Iterator Function ImportedRows(assignments As SpcMultivariatePointAssignments) As IEnumerable(Of SpcMultivariateStageRow)
            Dim used As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
            Dim runStart = 0, count = assignments.Phases.Count
            For point As Integer = 1 To count
                Dim atEnd = point = count
                Dim changed = Not atEnd AndAlso (Not String.Equals(assignments.StageIds(point), assignments.StageIds(runStart), StringComparison.Ordinal) OrElse assignments.Phases(point) <> assignments.Phases(runStart))
                If Not atEnd AndAlso Not changed Then Continue For
                Dim id = assignments.StageIds(runStart).Trim()
                If Not used.Add(id) Then Throw New ArgumentException("Stage '" & id & "' occurs in more than one non-contiguous block.")
                Yield New SpcMultivariateStageRow(id, runStart + 1, point, assignments.Phases(runStart))
                runStart = point
            Next
        End Function
    End Class
End Namespace
