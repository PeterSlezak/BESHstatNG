Option Explicit On
Option Strict On

Imports System

Namespace ExcelCommon
    Public Enum DocumentInvalidationReason
        None = 0
        Unverifiable
        WorkbookClosed
        DocumentChanged
        ActiveDocumentSwitched
        WorksheetRemoved
        WorksheetRenamed
        WorksheetReplaced
        SelectionChanged
        ContextDisposed
    End Enum

    Public Enum DocumentLifecycleStatus
        VerifiedEqual = 0
        Unavailable
        Stale
        Disposed
    End Enum

    ''' <summary>Bounded metadata only. Empty identity/revision means unverified, never equal.
    ''' ContextIdentity is proof of a host session object, NOT a physical workbook ID.
    ''' A host with no workbook ID may instead prove continuity of that same context.
    ''' Coordinates are an immutable rectangle; names are display/name-addressing metadata.</summary>
    Public NotInheritable Class DocumentLifecycleSnapshot
        Public Sub New(workbookIdentity As String, worksheetIdentity As String, worksheetName As String,
                       Optional bounds As OutputRectangle = Nothing, Optional selectionRevision As String = Nothing,
                       Optional contextIdentity As String = Nothing, Optional available As Boolean = True)
            Me.WorkbookIdentity = Bounded(workbookIdentity, 1024)
            Me.ContextIdentity = Bounded(contextIdentity, 1024)
            Me.WorksheetIdentity = Bounded(worksheetIdentity, 1024)
            Me.WorksheetName = Bounded(worksheetName, 256)
            Me.SelectionRevision = Bounded(selectionRevision, 256)
            Me.Bounds = bounds
            Me.Available = available
        End Sub
        Private Shared Function Bounded(value As String, limit As Integer) As String
            If String.IsNullOrWhiteSpace(value) Then Return String.Empty
            If value.Length > limit Then Throw New ArgumentException("Lifecycle metadata exceeds its limit.")
            For Each character In value
                If Char.IsControl(character) Then Throw New ArgumentException("Invalid lifecycle metadata.")
            Next
            Return value
        End Function
        Public ReadOnly Property WorkbookIdentity As String
        Public ReadOnly Property ContextIdentity As String
        Public ReadOnly Property WorksheetIdentity As String
        Public ReadOnly Property WorksheetName As String
        Public ReadOnly Property Bounds As OutputRectangle
        Public ReadOnly Property SelectionRevision As String
        Public ReadOnly Property Available As Boolean
    End Class

    Public NotInheritable Class DocumentLifecycleException
        Inherits InvalidOperationException
        Public Sub New(reason As DocumentInvalidationReason)
            MyBase.New("The bound document or selection is unavailable or changed. Refresh the source and destination, then retry.")
            Me.Reason = reason
        End Sub
        Public ReadOnly Property Reason As DocumentInvalidationReason
    End Class

    ''' <summary>Operation-owned lease, not an execution state machine. Hosts supply fresh
    ''' observations at checkpoints. No event registry, worksheet values or host references.
    ''' Pinned references ignore selection changes; volatile inputs stop requiring selection
    ''' continuity only after their immutable input has been copied. Target leases are separate.
    ''' Invalidation is sticky; disposal is idempotent and late invalidations are ignored.</summary>
    Public NotInheritable Class DocumentLifecycleLease
        Implements IDisposable
        Private ReadOnly _gate As New Object()
        Private ReadOnly _selectionBound As Boolean
        Private ReadOnly _nameAddressed As Boolean
        Private ReadOnly _requiresWorksheet As Boolean
        Private _copied As Boolean
        Private _disposed As Boolean
        Private _reason As DocumentInvalidationReason
        Public Sub New(binding As DocumentLifecycleSnapshot, Optional selectionBound As Boolean = False,
                       Optional nameAddressed As Boolean = True, Optional requiresWorksheet As Boolean = True)
            If binding Is Nothing Then Throw New ArgumentNullException(NameOf(binding))
            Me.Binding = binding
            _selectionBound = selectionBound : _nameAddressed = nameAddressed : _requiresWorksheet = requiresWorksheet
        End Sub
        Public ReadOnly Property Binding As DocumentLifecycleSnapshot
        Public ReadOnly Property Reason As DocumentInvalidationReason
            Get
                SyncLock _gate
                    Return If(_disposed, DocumentInvalidationReason.ContextDisposed, _reason)
                End SyncLock
            End Get
        End Property
        Public Function Invalidate(reason As DocumentInvalidationReason) As Boolean
            If Not [Enum].IsDefined(GetType(DocumentInvalidationReason), reason) OrElse reason = DocumentInvalidationReason.None Then
                Throw New ArgumentOutOfRangeException(NameOf(reason))
            End If
            SyncLock _gate
                If _disposed OrElse _reason <> DocumentInvalidationReason.None Then Return False
                If reason = DocumentInvalidationReason.SelectionChanged AndAlso (Not _selectionBound OrElse _copied) Then Return False
                If reason = DocumentInvalidationReason.ActiveDocumentSwitched AndAlso Not _selectionBound Then Return False
                _reason = reason
                Return True
            End SyncLock
        End Function
        Public Sub MarkInputCopied()
            SyncLock _gate
                If _disposed OrElse _reason <> DocumentInvalidationReason.None Then Throw New DocumentLifecycleException(Reason)
                _copied = True
            End SyncLock
        End Sub
        Public Function Check(current As DocumentLifecycleSnapshot) As DocumentLifecycleStatus
            SyncLock _gate
                If _disposed Then Return DocumentLifecycleStatus.Disposed
                If _reason <> DocumentInvalidationReason.None Then Return DocumentLifecycleStatus.Stale
                If current Is Nothing OrElse Not Binding.Available OrElse Not current.Available OrElse
                    (Binding.WorkbookIdentity.Length = 0 AndAlso Binding.ContextIdentity.Length = 0) OrElse
                    (Binding.WorkbookIdentity.Length > 0 AndAlso current.WorkbookIdentity.Length = 0) OrElse
                    (Binding.WorkbookIdentity.Length = 0 AndAlso current.ContextIdentity.Length = 0) OrElse
                    (_requiresWorksheet AndAlso (Binding.WorksheetIdentity.Length = 0 OrElse current.WorksheetIdentity.Length = 0)) Then
                    _reason = DocumentInvalidationReason.Unverifiable
                    Return DocumentLifecycleStatus.Unavailable
                End If
                If If(Binding.WorkbookIdentity.Length > 0, Binding.WorkbookIdentity <> current.WorkbookIdentity,
                      Binding.ContextIdentity <> current.ContextIdentity) Then
                    _reason = DocumentInvalidationReason.DocumentChanged
                ElseIf _requiresWorksheet AndAlso Binding.WorksheetIdentity <> current.WorksheetIdentity Then
                    _reason = DocumentInvalidationReason.WorksheetReplaced
                ElseIf _requiresWorksheet AndAlso _nameAddressed AndAlso Binding.WorksheetName <> current.WorksheetName Then
                    _reason = DocumentInvalidationReason.WorksheetRenamed
                ElseIf Binding.Bounds IsNot Nothing AndAlso Not _copied AndAlso Not SameBounds(Binding.Bounds, current.Bounds) Then
                    _reason = DocumentInvalidationReason.SelectionChanged
                ElseIf _selectionBound AndAlso Not _copied Then
                    If Binding.SelectionRevision.Length = 0 OrElse current.SelectionRevision.Length = 0 Then
                        _reason = DocumentInvalidationReason.Unverifiable
                        Return DocumentLifecycleStatus.Unavailable
                    End If
                    If Binding.SelectionRevision <> current.SelectionRevision OrElse Not SameBounds(Binding.Bounds, current.Bounds) Then
                        _reason = DocumentInvalidationReason.SelectionChanged
                    End If
                End If
                Return If(_reason = DocumentInvalidationReason.None, DocumentLifecycleStatus.VerifiedEqual, DocumentLifecycleStatus.Stale)
            End SyncLock
        End Function
        Private Shared Function SameBounds(first As OutputRectangle, second As OutputRectangle) As Boolean
            If first Is Nothing OrElse second Is Nothing Then Return first Is second
            Return first.StartRow = second.StartRow AndAlso first.StartColumn = second.StartColumn AndAlso
                first.EndRow = second.EndRow AndAlso first.EndColumn = second.EndColumn
        End Function
        Public Sub EnsureCurrent(current As DocumentLifecycleSnapshot)
            If Check(current) <> DocumentLifecycleStatus.VerifiedEqual Then Throw New DocumentLifecycleException(Reason)
        End Sub
        Public Sub Dispose() Implements IDisposable.Dispose
            SyncLock _gate
                _disposed = True
            End SyncLock
        End Sub
    End Class
End Namespace
