Option Explicit On
Option Strict On

Imports System
Imports System.Collections.Generic
Imports System.Linq

Namespace ExcelCommon
    Public Enum OutputOccupancy
        Empty = 0
        Occupied = 1
        Unknown = 2
        Rejected = 3
    End Enum

    Public Enum OutputWriteState
        Completed = 0
        CancelledByUser = 1
        RejectedBeforeWrite = 2
        FailedBeforeWrite = 3
        PartiallyWritten = 4
    End Enum

    Public NotInheritable Class WriteCodes
        Public Const Completed As String = "output.completed"
        Public Const Cancelled As String = "output.cancelled"
        Public Const Empty As String = "output.occupancy-empty"
        Public Const Occupied As String = "output.occupancy-occupied"
        Public Const Unverified As String = "output.occupancy-unverified"
        Public Const Rejected As String = "output.rejected"
        Public Const Changed As String = "output.consent-stale"
        Public Const ConfirmationRequired As String = "output.confirmation-required"
        Public Const Failed As String = "output.host-failed"
        Public Const MutationUncertain As String = "output.mutation-uncertain"
        Private Sub New()
        End Sub
    End Class

    ''' <summary>Ordered artifact identity; Nothing bounds means no cell mutation (e.g. a logical plot).
    ''' Addresses are host-neutral one-based inclusive rectangles, never envelopes.</summary>
    Public NotInheritable Class WriteRegion
        Public Sub New(artifactId As String, bounds As OutputRectangle, Optional nonCellMutation As Boolean = False)
            Me.ArtifactId = WriteIntent.CheckedText(artifactId, 256)
            If bounds IsNot Nothing AndAlso (bounds.EndRow > 1048576 OrElse bounds.EndColumn > 16384) Then Throw New ArgumentException("Write region outside Excel grid.")
            Me.Bounds = bounds
            MayMutate = bounds IsNot Nothing OrElse nonCellMutation
        End Sub
        Public ReadOnly Property ArtifactId As String
        Public ReadOnly Property Bounds As OutputRectangle
        Public ReadOnly Property MayMutate As Boolean
    End Class

    ''' <summary>One execution snapshot. PlanIdentity identifies the complete payload, including styles,
    ''' supplied by the adapter, not just its geometry. Unknown physical identities remain Nothing.
    ''' Guards are additional conservative inspection areas for host-positioned charts; never cleared.
    ''' An adapter must reject uncertainty it cannot safely inspect.</summary>
    Public NotInheritable Class WriteIntent
        Public Sub New(planIdentity As String, targetIdentity As String, target As SpreadsheetOutputTarget,
                       regions As IEnumerable(Of WriteRegion), Optional reservations As IEnumerable(Of OutputRectangle) = Nothing,
                       Optional guards As IEnumerable(Of OutputRectangle) = Nothing,
                       Optional workbookIdentity As String = Nothing, Optional worksheetIdentity As String = Nothing)
            Me.PlanIdentity = CheckedText(planIdentity, 256)
            Me.TargetIdentity = CheckedText(targetIdentity, 1024)
            If target Is Nothing OrElse Not [Enum].IsDefined(GetType(SpreadsheetOutputDestination), target.Destination) OrElse
                target.StartRow < 1 OrElse target.StartRow > 1048576 OrElse target.StartColumn < 1 OrElse target.StartColumn > 16384 Then Throw New ArgumentException("Invalid write target.")
            WorksheetName = CheckedText(target.WorksheetName, 256)
            Destination = target.Destination : StartRow = target.StartRow : StartColumn = target.StartColumn
            Me.WorkbookIdentity = If(workbookIdentity Is Nothing, Nothing, CheckedText(workbookIdentity, 1024))
            Me.WorksheetIdentity = If(worksheetIdentity Is Nothing, Nothing, CheckedText(worksheetIdentity, 1024))
            If regions Is Nothing Then Throw New ArgumentNullException(NameOf(regions))
            Dim items As New List(Of WriteRegion)()
            Dim ids As New HashSet(Of String)(StringComparer.Ordinal)
            For Each item In regions
                If item Is Nothing OrElse items.Count >= 256 OrElse Not ids.Add(item.ArtifactId) Then Throw New ArgumentException("Invalid ordered write identities.")
                If item.Bounds IsNot Nothing AndAlso items.Any(Function(p) p.Bounds IsNot Nothing AndAlso p.Bounds.Intersects(item.Bounds)) Then Throw New ArgumentException("Overlapping writes.")
                items.Add(item)
            Next
            Me.Regions = items.AsReadOnly()
            Me.Reservations = CopyRectangles(reservations)
            Me.Guards = CopyRectangles(guards)
            If Me.Reservations.Any(Function(r) items.Any(Function(p) p.Bounds IsNot Nothing AndAlso p.Bounds.Intersects(r))) Then Throw New ArgumentException("Write intersects reservation.")
        End Sub
        Friend Shared Function CheckedText(value As String, maximum As Integer) As String
            If String.IsNullOrWhiteSpace(value) OrElse value.Length > maximum OrElse value.Any(Function(c) Char.IsControl(c)) Then Throw New ArgumentException("Invalid bounded output identifier/message.")
            Return value
        End Function
        Private Shared Function CopyRectangles(source As IEnumerable(Of OutputRectangle)) As IReadOnlyList(Of OutputRectangle)
            Dim result As New List(Of OutputRectangle)()
            If source IsNot Nothing Then
                For Each item In source
                    If item Is Nothing OrElse result.Count >= 256 Then Throw New ArgumentException("Invalid rectangle list.")
                    Dim checked = New WriteRegion("bounds", item)
                    result.Add(checked.Bounds)
                Next
            End If
            Return result.AsReadOnly()
        End Function
        Public ReadOnly Property PlanIdentity As String
        Public ReadOnly Property TargetIdentity As String
        Public ReadOnly Property WorkbookIdentity As String
        Public ReadOnly Property WorksheetIdentity As String
        Public ReadOnly Property WorksheetName As String
        Public ReadOnly Property Destination As SpreadsheetOutputDestination
        Public ReadOnly Property StartRow As Integer
        Public ReadOnly Property StartColumn As Integer
        Public ReadOnly Property Regions As IReadOnlyList(Of WriteRegion)
        Public ReadOnly Property Reservations As IReadOnlyList(Of OutputRectangle)
        Public ReadOnly Property Guards As IReadOnlyList(Of OutputRectangle)
    End Class

    ''' <summary>ObservationIdentity is an opaque adapter snapshot token, never workbook cell data.
    ''' It must change whenever inspected values, formulas, target facts or identity change.</summary>
    Public NotInheritable Class WriteInspection
        Public Sub New(intent As WriteIntent, state As OutputOccupancy, observationIdentity As String,
                       occupiedRegionIds As IEnumerable(Of String), Optional hasValues As Boolean = False,
                       Optional hasFormulas As Boolean = False, Optional explanation As String = Nothing)
            If intent Is Nothing Then Throw New ArgumentNullException(NameOf(intent))
            If Not [Enum].IsDefined(GetType(OutputOccupancy), state) Then Throw New ArgumentOutOfRangeException(NameOf(state))
            Me.Intent = intent : Me.State = state
            Message = WriteIntent.CheckedText(If(explanation, DefaultMessage(state)), 1024)
            Me.ObservationIdentity = WriteIntent.CheckedText(observationIdentity, 256)
            Dim ids As New List(Of String)()
            If occupiedRegionIds IsNot Nothing Then
                For Each id In occupiedRegionIds
                    If ids.Count >= 256 OrElse ids.Contains(id) OrElse Not intent.Regions.Any(Function(r) r.ArtifactId = id AndAlso r.MayMutate) Then Throw New ArgumentException("Invalid occupied identities.")
                    ids.Add(id)
                Next
            End If
            If (state = OutputOccupancy.Empty AndAlso (ids.Count > 0 OrElse hasValues OrElse hasFormulas)) OrElse
               (state = OutputOccupancy.Occupied AndAlso ids.Count = 0) Then Throw New ArgumentException("Contradictory occupancy.")
            Me.OccupiedRegionIds = ids.AsReadOnly() : Me.HasValues = hasValues : Me.HasFormulas = hasFormulas
        End Sub
        Public ReadOnly Property Intent As WriteIntent
        Public ReadOnly Property State As OutputOccupancy
        Public ReadOnly Property ObservationIdentity As String
        Public ReadOnly Property OccupiedRegionIds As IReadOnlyList(Of String)
        Public ReadOnly Property HasValues As Boolean
        Public ReadOnly Property HasFormulas As Boolean
        Public ReadOnly Property Message As String
        Private Shared Function DefaultMessage(state As OutputOccupancy) As String
            Select Case state
                Case OutputOccupancy.Empty : Return "No values or formulas observed. Output formatting may change."
                Case OutputOccupancy.Occupied : Return "Existing values or formulas require explicit overwrite confirmation."
                Case OutputOccupancy.Unknown : Return "Destination occupancy could not be verified."
                Case Else : Return "The host rejected the destination before writing."
            End Select
        End Function
        Public ReadOnly Property ConfirmationRequired As Boolean
            Get
                Return State = OutputOccupancy.Occupied
            End Get
        End Property
        Public ReadOnly Property Code As String
            Get
                Select Case State
                    Case OutputOccupancy.Empty : Return WriteCodes.Empty
                    Case OutputOccupancy.Occupied : Return WriteCodes.Occupied
                    Case OutputOccupancy.Unknown : Return WriteCodes.Unverified
                    Case Else : Return WriteCodes.Rejected
                End Select
            End Get
        End Property
    End Class

    ''' <summary>Consent is tied to this exact immutable intent AND observation, not a preference.</summary>
    Public NotInheritable Class WriteConsent
        Public Sub New(inspection As WriteInspection, approved As Boolean)
            If inspection Is Nothing OrElse inspection.State <> OutputOccupancy.Occupied Then Throw New ArgumentException("Consent requires known occupancy.")
            Me.Inspection = inspection : Me.Approved = approved
        End Sub
        Public ReadOnly Property Inspection As WriteInspection
        Public ReadOnly Property Approved As Boolean
        Public Function Authorizes(current As WriteInspection) As Boolean
            Return Approved AndAlso current IsNot Nothing AndAlso Object.ReferenceEquals(Inspection.Intent, current.Intent) AndAlso
                current.State = OutputOccupancy.Occupied AndAlso Inspection.ObservationIdentity = current.ObservationIdentity AndAlso
                Inspection.OccupiedRegionIds.SequenceEqual(current.OccupiedRegionIds) AndAlso
                Inspection.HasValues = current.HasValues AndAlso Inspection.HasFormulas = current.HasFormulas
        End Function
    End Class

    ''' <summary>Additive outcome envelope v1; existing analysis/table/composite wire contracts are unchanged.
    ''' ConfirmedWritten includes only operations whose completion the adapter actually observed.
    ''' No adapter in 02.08 supplies undo or rollback. Partial includes mutation-uncertain failures.</summary>
    Public NotInheritable Class WriteResult
        Public Sub New(state As OutputWriteState, code As String, message As String, intent As WriteIntent,
                       mutationMayHaveOccurred As Boolean, Optional confirmedWrittenIds As IEnumerable(Of String) = Nothing,
                       Optional confirmationRequired As Boolean? = Nothing, Optional overwriteApproved As Boolean? = Nothing)
            If Not [Enum].IsDefined(GetType(OutputWriteState), state) Then Throw New ArgumentOutOfRangeException(NameOf(state))
            Me.Code = WriteIntent.CheckedText(code, 128) : Me.Message = WriteIntent.CheckedText(message, 1024)
            If intent Is Nothing Then Throw New ArgumentNullException(NameOf(intent))
            If (overwriteApproved.HasValue AndAlso Not confirmationRequired.GetValueOrDefault()) OrElse
                (state = OutputWriteState.CancelledByUser AndAlso (Not confirmationRequired.GetValueOrDefault() OrElse Not overwriteApproved.HasValue OrElse overwriteApproved.Value)) OrElse
                (state = OutputWriteState.Completed AndAlso confirmationRequired.GetValueOrDefault() AndAlso Not overwriteApproved.GetValueOrDefault()) Then Throw New ArgumentException("Contradictory consent outcome.")
            Dim confirmed As New List(Of WriteRegion)()
            Dim previous As Integer = -1
            If confirmedWrittenIds IsNot Nothing Then
                For Each id In confirmedWrittenIds
                    Dim index As Integer = -1
                    For i As Integer = 0 To intent.Regions.Count - 1
                        If intent.Regions(i).ArtifactId = id Then index = i
                    Next
                    If index <= previous OrElse confirmed.Count >= 256 OrElse Not intent.Regions(index).MayMutate Then Throw New ArgumentException("Invalid confirmed artifact order.")
                    confirmed.Add(intent.Regions(index)) : previous = index
                Next
            End If
            Dim safe As Boolean = state = OutputWriteState.CancelledByUser OrElse state = OutputWriteState.RejectedBeforeWrite OrElse state = OutputWriteState.FailedBeforeWrite
            If (safe AndAlso (mutationMayHaveOccurred OrElse confirmed.Count > 0)) OrElse
               (state = OutputWriteState.PartiallyWritten AndAlso Not mutationMayHaveOccurred) OrElse
               (state = OutputWriteState.Completed AndAlso (confirmed.Count <> intent.Regions.Where(Function(r) r.MayMutate).Count() OrElse (confirmed.Count > 0 AndAlso Not mutationMayHaveOccurred))) Then Throw New ArgumentException("Contradictory write outcome.")
            Me.State = state : Me.Intent = intent : Me.MutationMayHaveOccurred = mutationMayHaveOccurred
            Me.ConfirmedWritten = confirmed.AsReadOnly()
            Me.ConfirmationRequired = confirmationRequired : Me.OverwriteApproved = overwriteApproved
        End Sub
        Public ReadOnly Property SchemaVersion As Integer = 1
        Public ReadOnly Property State As OutputWriteState
        Public ReadOnly Property Code As String
        Public ReadOnly Property Message As String
        Public ReadOnly Property Intent As WriteIntent
        Public ReadOnly Property ConfirmedWritten As IReadOnlyList(Of WriteRegion)
        Public ReadOnly Property MutationMayHaveOccurred As Boolean
        Public ReadOnly Property ConfirmationRequired As Boolean?
        Public ReadOnly Property OverwriteApproved As Boolean?
        Public ReadOnly Property UndoSupported As Boolean = False
        Public ReadOnly Property RollbackAttempted As Boolean = False
        Public ReadOnly Property RollbackSucceeded As Boolean = False
    End Class

    ''' <summary>Small synchronous host-neutral gate. Inspection/revalidation are read-only delegates.
    ''' Each action is treated as possibly mutated BEFORE invocation. Never retries or rolls back.</summary>
    Public NotInheritable Class SafeWrite
        Private Sub New()
        End Sub
        Public Shared Function Execute(intent As WriteIntent, inspect As Func(Of WriteInspection),
                                       confirm As Func(Of WriteInspection, Boolean), writes As IReadOnlyList(Of Action),
                                       Optional destinationCreated As Boolean = False) As WriteResult
            If intent Is Nothing OrElse inspect Is Nothing OrElse confirm Is Nothing OrElse writes Is Nothing Then Throw New ArgumentNullException("write seam")
            If writes.Count <> intent.Regions.Count OrElse writes.Any(Function(a) a Is Nothing) Then Throw New ArgumentException("Write actions must match ordered intent.")
            Dim mutation As Boolean = destinationCreated
            Dim confirmed As New List(Of String)()
            Dim required As Boolean? = Nothing, approved As Boolean? = Nothing
            Try
                Dim observed = inspect()
                If observed Is Nothing OrElse Not Object.ReferenceEquals(observed.Intent, intent) OrElse observed.State = OutputOccupancy.Unknown OrElse observed.State = OutputOccupancy.Rejected Then
                    Return New WriteResult(If(mutation, OutputWriteState.PartiallyWritten, OutputWriteState.RejectedBeforeWrite),
                        If(observed Is Nothing, WriteCodes.Unverified, observed.Code),
                        If(observed Is Nothing, "Destination occupancy could not be verified.", observed.Message), intent, mutation)
                End If
                Dim consent As WriteConsent = Nothing
                required = observed.ConfirmationRequired
                If observed.ConfirmationRequired Then
                    consent = New WriteConsent(observed, confirm(observed))
                    approved = consent.Approved
                    If Not consent.Approved Then Return New WriteResult(If(mutation, OutputWriteState.PartiallyWritten, OutputWriteState.CancelledByUser), WriteCodes.Cancelled, If(mutation, "Overwrite cancelled; created destination may remain.", "Output overwrite cancelled. No writes performed."), intent, mutation, confirmationRequired:=required, overwriteApproved:=approved)
                End If
                Dim current = inspect()
                If current Is Nothing OrElse Not Object.ReferenceEquals(current.Intent, intent) OrElse
                    current.ObservationIdentity <> observed.ObservationIdentity OrElse current.State <> observed.State OrElse
                    (consent IsNot Nothing AndAlso Not consent.Authorizes(current)) Then
                    Return New WriteResult(If(mutation, OutputWriteState.PartiallyWritten, OutputWriteState.RejectedBeforeWrite), WriteCodes.Changed, "Destination changed. Inspect and confirm again.", intent, mutation, confirmationRequired:=required, overwriteApproved:=approved)
                End If
                For i As Integer = 0 To writes.Count - 1
                    If Not intent.Regions(i).MayMutate Then Continue For
                    mutation = True : writes(i)() : confirmed.Add(intent.Regions(i).ArtifactId)
                Next
                Return New WriteResult(OutputWriteState.Completed, WriteCodes.Completed, "Output completed.", intent, mutation, confirmed, required, approved)
            Catch ex As HostCapabilityException
                Return New WriteResult(If(mutation, OutputWriteState.PartiallyWritten, OutputWriteState.RejectedBeforeWrite),
                    ex.Code, ex.Message, intent, mutation, confirmed, required, approved)
            Catch ex As Exception
                ' Native exception text may contain workbook data; keep it out of the portable result.
                Return New WriteResult(If(mutation, OutputWriteState.PartiallyWritten, OutputWriteState.FailedBeforeWrite),
                    If(mutation, WriteCodes.MutationUncertain, WriteCodes.Failed),
                    If(mutation, "Output may be partially written. No rollback was attempted.", "Host inspection failed before writing."), intent, mutation, confirmed, required, approved)
            End Try
        End Function
    End Class
End Namespace
