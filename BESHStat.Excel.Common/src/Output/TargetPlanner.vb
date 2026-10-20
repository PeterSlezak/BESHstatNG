Option Explicit On
Option Strict On

Imports System

Namespace ExcelCommon
    ''' <summary>Optional read-only observations supplied by the host for this exact target.
    ''' Nothing means unverified, never permission to overwrite. Names alone are not identity.</summary>
    Public NotInheritable Class OutputTargetFacts
        Public Sub New(Optional workbookAvailable As Boolean? = Nothing,
                       Optional sheetExists As Boolean? = Nothing,
                       Optional identityUnambiguous As Boolean? = Nothing,
                       Optional writable As Boolean? = Nothing,
                       Optional protectedSheet As Boolean? = Nothing,
                       Optional mergedConflict As Boolean? = Nothing,
                       Optional canCreateWorksheet As Boolean? = Nothing,
                       Optional resolvedWorksheetName As String = Nothing,
                       Optional worksheetNameValid As Boolean? = Nothing,
                       Optional newWorksheetNameCollision As Boolean? = Nothing,
                       Optional maxRows As Integer = 1048576, Optional maxColumns As Integer = 16384)
            Me.WorkbookAvailable = workbookAvailable
            Me.SheetExists = sheetExists
            Me.IdentityUnambiguous = identityUnambiguous
            Me.Writable = writable
            Me.ProtectedSheet = protectedSheet
            Me.MergedConflict = mergedConflict
            Me.CanCreateWorksheet = canCreateWorksheet
            Me.ResolvedWorksheetName = resolvedWorksheetName
            Me.WorksheetNameValid = worksheetNameValid
            Me.NewWorksheetNameCollision = newWorksheetNameCollision
            Me.MaxRows = maxRows
            Me.MaxColumns = maxColumns
        End Sub
        Public ReadOnly Property WorkbookAvailable As Boolean?
        Public ReadOnly Property SheetExists As Boolean?
        Public ReadOnly Property IdentityUnambiguous As Boolean?
        Public ReadOnly Property Writable As Boolean?
        Public ReadOnly Property ProtectedSheet As Boolean?
        Public ReadOnly Property MergedConflict As Boolean?
        Public ReadOnly Property CanCreateWorksheet As Boolean?
        Public ReadOnly Property ResolvedWorksheetName As String
        Public ReadOnly Property WorksheetNameValid As Boolean?
        Public ReadOnly Property NewWorksheetNameCollision As Boolean?
        Public ReadOnly Property MaxRows As Integer
        Public ReadOnly Property MaxColumns As Integer
    End Class

    ''' <summary>Immutable decision. No fallback: effective destination always equals the request.
    ''' ToTarget returns a fresh transport DTO; changing it cannot change this decision.</summary>
    Public NotInheritable Class OutputTargetDecision
        Friend Sub New(target As SpreadsheetOutputTarget, host As HostCapabilitySnapshot,
                       facts As OutputTargetFacts, result As CapabilityPreflightResult)
            If target IsNot Nothing Then
                RequestedDestination = target.Destination
                StartRow = target.StartRow
                StartColumn = target.StartColumn
                RequestedWorksheetName = target.WorksheetName
            End If
            WorksheetName = If(facts.ResolvedWorksheetName, RequestedWorksheetName)
            HostName = If(host Is Nothing, Nothing, host.Host)
            Platform = If(host Is Nothing, Nothing, host.Platform)
            SelectedCapability = HostCapabilityIds.ForDestination(RequestedDestination)
            Me.Facts = facts
            Me.Result = result
        End Sub
        Public ReadOnly Property RequestedDestination As SpreadsheetOutputDestination
        Public ReadOnly Property EffectiveDestination As SpreadsheetOutputDestination
            Get
                Return RequestedDestination
            End Get
        End Property
        Public ReadOnly Property RequestedWorksheetName As String
        Public ReadOnly Property WorksheetName As String
        Public ReadOnly Property StartRow As Integer
        Public ReadOnly Property StartColumn As Integer
        Public ReadOnly Property HostName As String
        Public ReadOnly Property Platform As String
        Public ReadOnly Property SelectedCapability As String
        Public ReadOnly Property Facts As OutputTargetFacts
        Public ReadOnly Property Result As CapabilityPreflightResult
        Public Sub EnsureSupported()
            Result.EnsureSupported()
        End Sub
        Public Function ToTarget() As SpreadsheetOutputTarget
            EnsureSupported()
            Return New SpreadsheetOutputTarget With {.Destination = EffectiveDestination,
                .WorksheetName = WorksheetName, .StartRow = StartRow, .StartColumn = StartColumn}
        End Function
    End Class

    Public NotInheritable Class TargetPlanner
        Private Sub New()
        End Sub

        Public Shared Function Create(requested As SpreadsheetOutputTarget, host As HostCapabilitySnapshot,
                                      Optional facts As OutputTargetFacts = Nothing) As OutputTargetDecision
            Dim observed = If(facts, New OutputTargetFacts())
            Dim check = HostCapabilityPreflight.CheckOutput(host, requested)
            If check.IsSupported Then
                Dim message As String = Nothing
                Dim code As String = CapabilityPreflightCodes.InvalidTarget
                If observed.MaxRows < 1 OrElse observed.MaxColumns < 1 OrElse
                    requested.StartRow > observed.MaxRows OrElse requested.StartColumn > observed.MaxColumns Then
                    message = "The requested start cell is outside the host grid. Choose a valid row and column."
                ElseIf requested.Destination <> SpreadsheetOutputDestination.NewWorkbook AndAlso observed.WorkbookAvailable = False Then
                    message = "The target workbook is missing or inaccessible. Open a writable workbook and retry."
                ElseIf requested.Destination = SpreadsheetOutputDestination.CurrentWorksheet AndAlso
                    (observed.SheetExists = False OrElse observed.IdentityUnambiguous = False) Then
                    message = "The intended worksheet is missing, inaccessible or ambiguous. Select it explicitly and retry."
                ElseIf observed.WorksheetNameValid = False Then
                    message = "The host rejected the worksheet name. Choose a valid name."
                ElseIf requested.Destination = SpreadsheetOutputDestination.NewWorksheet AndAlso observed.NewWorksheetNameCollision = True Then
                    code = CapabilityPreflightCodes.SheetNameCollision
                    message = "No unused worksheet name could be resolved. Choose another new worksheet name."
                ElseIf requested.Destination = SpreadsheetOutputDestination.NewWorksheet AndAlso observed.CanCreateWorksheet = False Then
                    code = CapabilityPreflightCodes.UnwritableTarget
                    message = "The workbook does not allow adding a worksheet. Choose another writable destination."
                ElseIf observed.ProtectedSheet = True OrElse observed.Writable = False Then
                    code = CapabilityPreflightCodes.UnwritableTarget
                    message = "The target is protected or known to be unwritable. Choose an unprotected writable destination."
                ElseIf observed.MergedConflict = True Then
                    code = CapabilityPreflightCodes.MergedTarget
                    message = "The planned output intersects merged cells. Choose an unmerged destination."
                End If
                If message IsNot Nothing Then check = CapabilityPreflightResult.Rejected(code, message)
            End If
            Return New OutputTargetDecision(requested, host, observed, check)
        End Function
    End Class
End Namespace
