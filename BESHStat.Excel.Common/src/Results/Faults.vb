Option Explicit On
Option Strict On

Imports System
Imports System.Collections.Generic
Imports System.Globalization

Namespace ExcelCommon
    Public Enum UserFaultSeverity
        Information
        Warning
        [Error]
    End Enum

    Public Enum UserFaultCategory
        Input
        Analysis
        Host
        Output
        Cancellation
        Unexpected
    End Enum

    ''' <summary>Presentation-only codes. Existing analysis.* and output.* values are reused.
    ''' These codes are never diagnostic event codes or additions to output wire schemas.</summary>
    Public NotInheritable Class UserFaultCodes
        Private Sub New()
        End Sub
        Public Const InvalidInput As String = "user.input-invalid"
        Public Const InputWarning As String = "user.input-warning"
        Public Const InputInformation As String = "user.input-information"
        Public Const Cancelled As String = "user.cancelled"
        Public Const HostUnavailable As String = "user.host-unavailable"
        Public Const Unexpected As String = "user.unexpected"
    End Class

    ''' <summary>Immutable safe projection, constructed only by the fixed mapper. No native
    ''' messages, worksheet addresses, cell values, exceptions or absolute coordinates retained.</summary>
    Public NotInheritable Class UserFault
        Friend Sub New(code As String, category As UserFaultCategory, severity As UserFaultSeverity,
                       title As String, summary As String, action As String,
                       Optional originalCode As String = Nothing, Optional fieldPath As String = Nothing,
                       Optional selectedRow As Integer = 0, Optional selectedColumn As Integer = 0)
            Me.Code = code : Me.Category = category : Me.Severity = severity
            Me.Title = title : Me.Summary = summary : Me.Action = action
            Me.OriginalCode = originalCode : Me.FieldPath = fieldPath
            Me.SelectedRow = selectedRow : Me.SelectedColumn = selectedColumn
        End Sub
        Public ReadOnly Property Code As String
        Public ReadOnly Property Category As UserFaultCategory
        Public ReadOnly Property Severity As UserFaultSeverity
        Public ReadOnly Property Title As String
        Public ReadOnly Property Summary As String
        Public ReadOnly Property Action As String
        Public ReadOnly Property OriginalCode As String
        Public ReadOnly Property FieldPath As String
        Public ReadOnly Property SelectedRow As Integer
        Public ReadOnly Property SelectedColumn As Integer
    End Class

    ''' <summary>Pure, ordinal mapping. Unknown text is discarded, never scrubbed or echoed.
    ''' A cancellation request or an arbitrary OCE is not evidence of terminal cancellation.</summary>
    Public NotInheritable Class UserFaultMapper
        Private Sub New()
        End Sub

        Public Shared Function FromAnalysisFault(fault As AnalysisFault) As UserFault
            If fault Is Nothing Then Return Unexpected()
            Select Case fault.Code
                Case AnalysisFaultCodes.InvalidRequest
                    Return New UserFault(fault.Code, UserFaultCategory.Input, UserFaultSeverity.Error,
                        "Check analysis options", "The analysis request is invalid.",
                        "Check the grouped numeric input and select options supported by this analysis.", fault.Code, SafeField(fault.FieldPath))
                Case AnalysisFaultCodes.UnsupportedAnalysis
                    Return New UserFault(fault.Code, UserFaultCategory.Analysis, UserFaultSeverity.Error,
                        "Analysis unavailable", "This analysis is not supported by the current host.",
                        "Select a supported analysis and try again.", fault.Code, SafeField(fault.FieldPath))
                Case AnalysisFaultCodes.ExecutionFailed
                    ' This signal proves only an analysis failure, not singularity/convergence.
                    Return New UserFault(fault.Code, UserFaultCategory.Analysis, UserFaultSeverity.Error,
                        "Analysis could not complete", "No usable analysis result was produced.",
                        "Review the input and model options. If this repeats, report the code and run reference.", fault.Code)
                Case Else
                    Return Unexpected()
            End Select
        End Function

        Public Shared Function FromValidation(issue As SpreadsheetValidationIssue) As UserFault
            If issue Is Nothing Then Return Unexpected()
            Dim severity = UserFaultSeverity.Error
            Dim code = UserFaultCodes.InvalidInput
            If issue.Severity = SpreadsheetValidationSeverity.Warning Then
                severity = UserFaultSeverity.Warning : code = UserFaultCodes.InputWarning
            ElseIf issue.Severity = SpreadsheetValidationSeverity.Information Then
                severity = UserFaultSeverity.Information : code = UserFaultCodes.InputInformation
            End If
            Dim action = "Check the selected range, group layout and numeric observations, then retry."
            Dim original As String = Nothing
            Select Case issue.Code
                Case "cell.not-numeric", "cell.invalid-value", "cell.excel-error"
                    action = "Correct the numeric observation in the selected location, then retry."
                    original = issue.Code
                Case "groups.too-few", "range.too-few-groups", "groups.too-few-observations"
                    action = "Select at least two groups with enough numeric observations in each group."
                    original = issue.Code
                Case "options.invalid", "options.long-columns"
                    action = "Choose a supported input orientation and distinct category and value columns."
                    original = issue.Code
                Case "header.invalid-cell", "header.excel-error", "header.ambiguous"
                    action = "Check the header setting and use valid group headers."
                    original = issue.Code
                Case "groups.category-error", "groups.category-invalid", "groups.category-missing", "groups.category-collision"
                    action = "Check the category column and provide distinct valid group identifiers."
                    original = issue.Code
                Case "cell.missing-skipped", "rows.blank-skipped"
                    action = "Review the skipped observations before using the results."
                    original = issue.Code
                Case "header.inferred"
                    action = "Review the inferred header setting before using the results."
                    original = issue.Code
                Case "range.null", "range.shape", "range.not-rectangular"
                    original = issue.Code
            End Select
            Return New UserFault(code, UserFaultCategory.Input, severity,
                If(severity = UserFaultSeverity.Error, "Check analysis input", "Review analysis input"),
                If(severity = UserFaultSeverity.Error, "The selected input needs correction.", "The input parser reported a nonfatal notice."),
                action, original, SafeField(issue.FieldPath), SafeOffset(issue.SelectedRow, 1048576), SafeOffset(issue.SelectedColumn, 16384))
        End Function

        Public Shared Function FromReport(report As SpreadsheetValidationReport) As IReadOnlyList(Of UserFault)
            Dim result As New List(Of UserFault)()
            If report IsNot Nothing Then
                For Each issue In report.Issues
                    result.Add(FromValidation(issue))
                Next
            End If
            Return result.AsReadOnly()
        End Function

        Public Shared Function FromWarning(warning As AnalysisWarning) As UserFault
            Dim known = warning IsNot Nothing AndAlso String.Equals(warning.Code, AnalysisWarningCodes.AlphaDefaulted, StringComparison.Ordinal)
            Return New UserFault(UserFaultCodes.InputWarning, UserFaultCategory.Input, UserFaultSeverity.Warning,
                "Review analysis options", If(known, "The default significance level was used.", "The analysis reported a nonfatal warning."),
                "Review the analysis options before using the results.", If(known, AnalysisWarningCodes.AlphaDefaulted, Nothing),
                If(warning Is Nothing, Nothing, SafeField(warning.FieldPath)))
        End Function

        Public Shared Function FromException(failureException As Exception) As UserFault
            ' Deliberately closed whitelist. No base ArgumentException/InvalidOperationException,
            ' JS exception, message matching, inner exception inspection or inferred convergence.
            Dim validation = TryCast(failureException, SpreadsheetInputValidationException)
            If validation IsNot Nothing Then
                Dim primary = PrimaryIssue(validation.Report)
                If primary IsNot Nothing Then Return FromValidation(primary)
            End If
            Dim host = TryCast(failureException, HostCapabilityException)
            If host IsNot Nothing Then
                Select Case host.Code
                    Case CapabilityPreflightCodes.UnknownAnalysis, CapabilityPreflightCodes.UnsupportedAnalysis
                        Return FromAnalysisFault(New AnalysisFault(AnalysisFaultCodes.UnsupportedAnalysis, "Unsupported"))
                    Case CapabilityPreflightCodes.InvalidTarget, CapabilityPreflightCodes.SheetNameCollision,
                         CapabilityPreflightCodes.UnwritableTarget, CapabilityPreflightCodes.MergedTarget,
                         CapabilityPreflightCodes.UnsupportedDestination
                        Return FromWriteState(OutputWriteState.RejectedBeforeWrite, WriteCodes.Rejected)
                    Case CapabilityPreflightCodes.HostUnavailable, CapabilityPreflightCodes.MissingCapability
                        Return New UserFault(UserFaultCodes.HostUnavailable, UserFaultCategory.Host, UserFaultSeverity.Error,
                            "Spreadsheet operation unavailable", "The host cannot perform this operation.",
                            "Open a supported Excel host and check the required operation is available.")
                End Select
            End If
            Return Unexpected()
        End Function

        Public Shared Function FromWriteResult(result As WriteResult) As UserFault
            If result Is Nothing Then Return Unexpected()
            Return FromWriteState(result.State, result.Code, result.MutationMayHaveOccurred)
        End Function

        ''' <summary>Host adapters may project an existing writer envelope here without changing it.
        ''' State takes precedence over code. Uncertain mutation takes precedence over a safe failure.</summary>
        Public Shared Function FromWriteState(state As OutputWriteState, code As String,
                                              Optional mutationMayHaveOccurred As Boolean = False) As UserFault
            If state = OutputWriteState.Completed Then Return Nothing
            If state = OutputWriteState.PartiallyWritten OrElse mutationMayHaveOccurred Then
                Return New UserFault(WriteCodes.MutationUncertain, UserFaultCategory.Output, UserFaultSeverity.Warning,
                    "Output may remain", "Output may be partially written. No rollback was attempted.",
                    "Inspect the destination for remaining output before retrying.")
            End If
            If state = OutputWriteState.CancelledByUser Then
                Return New UserFault(WriteCodes.Cancelled, UserFaultCategory.Cancellation, UserFaultSeverity.Information,
                    "Overwrite cancelled", "No output was written.", "Choose another destination or run again to review overwrite consent.")
            End If
            If state = OutputWriteState.FailedBeforeWrite Then
                Return New UserFault(WriteCodes.Failed, UserFaultCategory.Output, UserFaultSeverity.Error,
                    "Output could not start", "The host failed before writing output.", "Check the destination and host availability, then retry.")
            End If
            If state <> OutputWriteState.RejectedBeforeWrite Then Return Unexpected()
            Select Case code
                Case WriteCodes.Changed
                    Return New UserFault(code, UserFaultCategory.Output, UserFaultSeverity.Warning,
                        "Destination changed", "The previous overwrite consent is no longer valid. No output was written.",
                        "Inspect the destination and confirm again.")
                Case WriteCodes.Unverified
                    Return New UserFault(code, UserFaultCategory.Output, UserFaultSeverity.Warning,
                        "Destination not verified", "Destination occupancy could not be verified. No output was written.",
                        "Check the destination or choose a new empty worksheet.")
                Case WriteCodes.ConfirmationRequired, WriteCodes.Occupied
                    Return AwaitingConfirmation()
                Case Else
                    Return New UserFault(WriteCodes.Rejected, UserFaultCategory.Output, UserFaultSeverity.Error,
                        "Choose another destination", "The destination was rejected before writing.",
                        "Select a supported writable destination without merged cells, then retry.")
            End Select
        End Function

        Public Shared Function AwaitingConfirmation() As UserFault
            Return New UserFault(WriteCodes.ConfirmationRequired, UserFaultCategory.Output, UserFaultSeverity.Information,
                "Confirm overwrite", "The output destination contains existing content.",
                "Choose Overwrite to approve this destination, or Cancel to keep its content.")
        End Function

        ''' <summary>Authoritative write outcome first, then confirmed terminal state, errors,
        ''' mapped fault, unexpected failure, and finally nonfatal validation notices.</summary>
        Public Shared Function FromOutcome(snapshot As ExecutionSnapshot, Optional writeResult As WriteResult = Nothing,
                                           Optional failureException As Exception = Nothing, Optional fault As AnalysisFault = Nothing,
                                           Optional validation As SpreadsheetValidationReport = Nothing) As UserFault
            If writeResult IsNot Nothing Then Return FromWriteResult(writeResult)
            If snapshot IsNot Nothing Then
                If snapshot.State = ExecutionState.Completed Then Return Nothing
                If snapshot.State = ExecutionState.Cancelled Then
                    Return New UserFault(UserFaultCodes.Cancelled, UserFaultCategory.Cancellation, UserFaultSeverity.Information,
                        "Execution cancelled", "No further analysis or output will start.", "Run again when ready.")
                End If
            End If
            Dim primary = PrimaryIssue(validation)
            If validation IsNot Nothing AndAlso validation.HasErrors Then Return FromValidation(primary)
            If fault IsNot Nothing Then Return FromAnalysisFault(fault)
            If failureException IsNot Nothing Then Return FromException(failureException)
            If snapshot IsNot Nothing AndAlso snapshot.State = ExecutionState.Failed Then Return Unexpected()
            If primary IsNot Nothing Then Return FromValidation(primary)
            Return Nothing
        End Function

        Public Shared Function FormatDisplay(fault As UserFault, Optional runId As String = Nothing) As String
            If fault Is Nothing Then Return String.Empty
            Dim result = fault.Title & " [" & fault.Code & "] " & fault.Summary & " " & fault.Action
            If fault.SelectedRow > 0 Then result &= " Selected row " & fault.SelectedRow.ToString(CultureInfo.InvariantCulture) & "."
            If fault.SelectedColumn > 0 Then result &= " Selected column " & fault.SelectedColumn.ToString(CultureInfo.InvariantCulture) & "."
            Dim run As Guid
            If Guid.TryParseExact(runId, "N", run) AndAlso run <> Guid.Empty Then result &= " Run " & run.ToString("N") & "."
            Return result
        End Function

        Private Shared Function Unexpected() As UserFault
            Return New UserFault(UserFaultCodes.Unexpected, UserFaultCategory.Unexpected, UserFaultSeverity.Error,
                "Operation could not complete", "An unexpected failure occurred.",
                "Retry the operation. If it repeats, report the code and run reference.")
        End Function
        Private Shared Function PrimaryIssue(report As SpreadsheetValidationReport) As SpreadsheetValidationIssue
            If report Is Nothing OrElse report.Issues.Count = 0 Then Return Nothing
            Dim primary = report.Issues(0)
            For Each issue In report.Issues
                If CInt(issue.Severity) > CInt(primary.Severity) Then primary = issue
            Next
            Return primary
        End Function
        Private Shared Function SafeOffset(value As Integer, maximum As Integer) As Integer
            Return If(value > 0 AndAlso value <= maximum, value, 0)
        End Function
        Private Shared Function SafeField(value As String) As String
            Select Case value
                Case "input", "request", "analysisId", "options", "options.columns", "options.alpha", "observations", "groups", "rows", "header", "category", "execution", "output"
                    Return value
            End Select
            If value IsNot Nothing AndAlso value.Length <= 13 AndAlso value.StartsWith("groups[", StringComparison.Ordinal) AndAlso value.EndsWith("]", StringComparison.Ordinal) Then
                Dim index As Integer
                Dim digits = value.Substring(7, value.Length - 8)
                If Integer.TryParse(digits, NumberStyles.None, CultureInfo.InvariantCulture, index) AndAlso index >= 0 AndAlso index < 16384 AndAlso
                    String.Equals(digits, index.ToString(CultureInfo.InvariantCulture), StringComparison.Ordinal) Then Return value
            End If
            Return Nothing
        End Function
    End Class
End Namespace
