Option Explicit On
Option Strict On
Imports System
Imports System.Collections.Generic
Imports System.Threading
Imports BESHStatNG.ExcelCommon
Imports Excel = Microsoft.Office.Interop.Excel

Namespace AppInfrastructure
    ''' <summary>Run-scoped UI-thread observer. Native subscriptions and observations are
    ''' injected at the same seam used in tests. Dispose never releases borrowed RCWs.</summary>
    Friend NotInheritable Class WindowsDocumentLifecycle
        Implements IDisposable
        Private ReadOnly _thread As Integer = Thread.CurrentThread.ManagedThreadId
        Private ReadOnly _gate As New Object()
        Private ReadOnly _watches As New List(Of Tuple(Of DocumentLifecycleLease, Func(Of DocumentLifecycleSnapshot)))()
        Private _detach As Action
        Private _disposed As Boolean

        Friend Sub New(Optional subscribe As Func(Of Action(Of DocumentInvalidationReason, String, String), Action) = Nothing)
            If subscribe IsNot Nothing Then _detach = subscribe(AddressOf Notice)
        End Sub
        Private Sub EnsureThread()
            If Thread.CurrentThread.ManagedThreadId <> _thread Then Throw New DocumentLifecycleException(DocumentInvalidationReason.Unverifiable)
        End Sub
        Friend Function Watch(observe As Func(Of DocumentLifecycleSnapshot), Optional requiresWorksheet As Boolean = True) As DocumentLifecycleLease
            EnsureThread()
            If _disposed OrElse _watches.Count >= 8 Then Throw New DocumentLifecycleException(DocumentInvalidationReason.ContextDisposed)
            Dim binding = observe()
            Dim lease As New DocumentLifecycleLease(binding, requiresWorksheet:=requiresWorksheet)
            lease.EnsureCurrent(binding)
            SyncLock _gate
                If _disposed Then Throw New DocumentLifecycleException(DocumentInvalidationReason.ContextDisposed)
                _watches.Add(Tuple.Create(lease, observe))
            End SyncLock
            Return lease
        End Function
        Friend Sub EnsureCurrent()
            EnsureThread()
            If _disposed Then Throw New DocumentLifecycleException(DocumentInvalidationReason.ContextDisposed)
            For Each entry In CopyWatches()
                entry.Item1.EnsureCurrent(entry.Item1.Binding) ' Sticky invalidation avoids touching a closed COM object.
                entry.Item1.EnsureCurrent(entry.Item2.Invoke())
            Next
        End Sub
        Private Sub Notice(reason As DocumentInvalidationReason, book As String, sheet As String)
            If _disposed Then Return
            ' Never perform COM from an unexpected event thread.
            If Thread.CurrentThread.ManagedThreadId <> _thread Then
                For Each entry In CopyWatches()
                    entry.Item1.Invalidate(DocumentInvalidationReason.Unverifiable)
                Next
                Return
            End If
            For Each entry In CopyWatches()
                Dim binding = entry.Item1.Binding
                If (book Is Nothing OrElse binding.WorkbookIdentity = book) AndAlso
                   (sheet Is Nothing OrElse binding.WorksheetIdentity = sheet) Then entry.Item1.Invalidate(reason)
            Next
        End Sub
        Private Function CopyWatches() As Tuple(Of DocumentLifecycleLease, Func(Of DocumentLifecycleSnapshot))()
            SyncLock _gate
                Return If(_disposed, New Tuple(Of DocumentLifecycleLease, Func(Of DocumentLifecycleSnapshot))() {}, _watches.ToArray())
            End SyncLock
        End Function
        Public Sub Dispose() Implements IDisposable.Dispose
            EnsureThread()
            Dim entries As Tuple(Of DocumentLifecycleLease, Func(Of DocumentLifecycleSnapshot))()
            Dim detach As Action
            SyncLock _gate
                If _disposed Then Return
                entries = _watches.ToArray()
                _watches.Clear()
                _disposed = True
                detach = _detach : _detach = Nothing
            End SyncLock
            Try
                detach?.Invoke()
            Finally
                For Each entry In entries
                    entry.Item1.Dispose()
                Next
            End Try
        End Sub

        Friend Shared Function Observe(app As Excel.Application, book As Excel.Workbook, sheet As Excel.Worksheet,
                                       Optional bounds As OutputRectangle = Nothing) As DocumentLifecycleSnapshot
            Try
                Dim bookId = ExcelDnaOutputTargetAdapter.TargetIdentity(book)
                Dim foundBook As Boolean = False
                If app.Workbooks.Count > 256 Then Throw New InvalidOperationException()
                For Each candidate As Excel.Workbook In app.Workbooks
                    If ExcelDnaOutputTargetAdapter.TargetIdentity(candidate) = bookId Then foundBook = True : Exit For
                Next
                If Not foundBook Then Return New DocumentLifecycleSnapshot(bookId, Nothing, Nothing, available:=False)
                If sheet Is Nothing Then Return New DocumentLifecycleSnapshot(bookId, Nothing, Nothing)
                Dim sheetId = ExcelDnaOutputTargetAdapter.TargetIdentity(sheet)
                Dim foundSheet As Boolean = False
                If book.Worksheets.Count > 4096 Then Throw New InvalidOperationException()
                For Each candidate As Excel.Worksheet In book.Worksheets
                    If ExcelDnaOutputTargetAdapter.TargetIdentity(candidate) = sheetId Then foundSheet = True : Exit For
                Next
                Return New DocumentLifecycleSnapshot(bookId, sheetId, sheet.Name, bounds, available:=foundSheet)
            Catch
                Return New DocumentLifecycleSnapshot(Nothing, Nothing, Nothing, available:=False)
            End Try
        End Function

        Friend Shared Function Create(app As Excel.Application) As WindowsDocumentLifecycle
            Dim ownerThread = Thread.CurrentThread.ManagedThreadId
            Return New WindowsDocumentLifecycle(Function(notify)
                Dim events = DirectCast(app, Excel.AppEvents_Event)
                Dim listening As Boolean = True
                Dim closeHandler As Excel.AppEvents_WorkbookBeforeCloseEventHandler =
                    Sub(book As Excel.Workbook, ByRef cancel As Boolean)
                        If Not Volatile.Read(listening) Then Return
                        If Thread.CurrentThread.ManagedThreadId <> ownerThread Then
                            notify(DocumentInvalidationReason.Unverifiable, Nothing, Nothing) : Return
                        End If
                        SafeNotice(notify, DocumentInvalidationReason.WorkbookClosed, book, Nothing)
                    End Sub
                Dim deleteHandler As Excel.AppEvents_SheetBeforeDeleteEventHandler =
                    Sub(sheet As Object)
                        If Not Volatile.Read(listening) Then Return
                        If Thread.CurrentThread.ManagedThreadId <> ownerThread Then
                            notify(DocumentInvalidationReason.Unverifiable, Nothing, Nothing) : Return
                        End If
                        Try
                            Dim worksheet = TryCast(sheet, Excel.Worksheet)
                            If worksheet IsNot Nothing Then SafeNotice(notify, DocumentInvalidationReason.WorksheetRemoved, worksheet.Parent, worksheet)
                        Catch
                            notify(DocumentInvalidationReason.Unverifiable, Nothing, Nothing)
                        End Try
                    End Sub
                Dim deactivateHandler As Excel.AppEvents_WorkbookDeactivateEventHandler =
                    Sub(book)
                        If Not Volatile.Read(listening) Then Return
                        If Thread.CurrentThread.ManagedThreadId <> ownerThread Then
                            notify(DocumentInvalidationReason.Unverifiable, Nothing, Nothing) : Return
                        End If
                        SafeNotice(notify, DocumentInvalidationReason.ActiveDocumentSwitched, book, Nothing)
                    End Sub
                Dim selectionHandler As Excel.AppEvents_SheetSelectionChangeEventHandler =
                    Sub(sheet As Object, target As Excel.Range)
                        If Not Volatile.Read(listening) Then Return
                        If Thread.CurrentThread.ManagedThreadId <> ownerThread Then
                            notify(DocumentInvalidationReason.Unverifiable, Nothing, Nothing) : Return
                        End If
                        Try
                            Dim worksheet = TryCast(sheet, Excel.Worksheet)
                            If worksheet IsNot Nothing Then SafeNotice(notify, DocumentInvalidationReason.SelectionChanged, worksheet.Parent, worksheet)
                        Catch
                            notify(DocumentInvalidationReason.Unverifiable, Nothing, Nothing)
                        End Try
                    End Sub
                Dim detach As Action = Sub()
                    Volatile.Write(listening, False)
                    ' Attempt every detach even if Excel is already closing.
                    Try : RemoveHandler events.WorkbookBeforeClose, closeHandler : Catch : End Try
                    Try : RemoveHandler events.SheetBeforeDelete, deleteHandler : Catch : End Try
                    Try : RemoveHandler events.WorkbookDeactivate, deactivateHandler : Catch : End Try
                    Try : RemoveHandler events.SheetSelectionChange, selectionHandler : Catch : End Try
                End Sub
                Try
                    AddHandler events.WorkbookBeforeClose, closeHandler
                    AddHandler events.SheetBeforeDelete, deleteHandler
                    AddHandler events.WorkbookDeactivate, deactivateHandler
                    AddHandler events.SheetSelectionChange, selectionHandler
                Catch
                    detach()
                    ' Full object reconciliation remains mandatory if event hookup is unavailable.
                End Try
                Return detach
            End Function)
        End Function
        Private Shared Sub SafeNotice(notify As Action(Of DocumentInvalidationReason, String, String), reason As DocumentInvalidationReason,
                                      book As Object, sheet As Object)
            Try
                notify(reason, ExcelDnaOutputTargetAdapter.TargetIdentity(book),
                       If(sheet Is Nothing, Nothing, ExcelDnaOutputTargetAdapter.TargetIdentity(sheet)))
            Catch
                notify(DocumentInvalidationReason.Unverifiable, Nothing, Nothing)
            End Try
        End Sub
    End Class
End Namespace
