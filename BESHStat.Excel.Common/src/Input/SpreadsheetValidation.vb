Option Explicit On
Option Strict On

Imports System.Collections.Generic
Imports System.Collections.ObjectModel
Imports System.Linq

Namespace ExcelCommon

    ''' <summary>Stable severity for host-neutral spreadsheet input diagnostics.</summary>
    Public Enum SpreadsheetValidationSeverity
        Information = 0
        Warning = 1
        [Error] = 2
    End Enum

    ''' <summary>
    ''' Describes a specific validation problem. SelectedRow/Column are 1-based offsets within
    ''' the supplied range; WorksheetRow/Column are 1-based Excel coordinates or zero if unknown.
    ''' No host objects or mutable spreadsheet cell references are retained.
    ''' </summary>
    Public NotInheritable Class SpreadsheetValidationIssue
        Public Sub New(code As String, severity As SpreadsheetValidationSeverity,
                       message As String, fieldPath As String,
                       Optional selectedRow As Integer = 0, Optional selectedColumn As Integer = 0,
                       Optional worksheetRow As Integer = 0, Optional worksheetColumn As Integer = 0)
            If String.IsNullOrWhiteSpace(code) Then Throw New ArgumentException("An issue code is required.", NameOf(code))
            If selectedRow < 0 OrElse selectedColumn < 0 OrElse worksheetRow < 0 OrElse worksheetColumn < 0 Then
                Throw New ArgumentOutOfRangeException(NameOf(selectedRow), "Validation coordinates must be nonnegative.")
            End If
            Me.Code = code
            Me.Severity = severity
            Me.Message = If(message, String.Empty)
            Me.FieldPath = If(fieldPath, String.Empty)
            Me.SelectedRow = selectedRow
            Me.SelectedColumn = selectedColumn
            Me.WorksheetRow = worksheetRow
            Me.WorksheetColumn = worksheetColumn
        End Sub

        Public ReadOnly Property Code As String
        Public ReadOnly Property Severity As SpreadsheetValidationSeverity
        Public ReadOnly Property Message As String
        Public ReadOnly Property FieldPath As String
        Public ReadOnly Property SelectedRow As Integer
        Public ReadOnly Property SelectedColumn As Integer
        Public ReadOnly Property WorksheetRow As Integer
        Public ReadOnly Property WorksheetColumn As Integer
    End Class

    ''' <summary>Immutable snapshot of diagnostic issues returned by a parser.</summary>
    Public NotInheritable Class SpreadsheetValidationReport
        Private ReadOnly _issues As ReadOnlyCollection(Of SpreadsheetValidationIssue)

        Public Sub New(issues As IEnumerable(Of SpreadsheetValidationIssue))
            If issues Is Nothing Then Throw New ArgumentNullException(NameOf(issues))
            Dim items As New List(Of SpreadsheetValidationIssue)()
            For Each item As SpreadsheetValidationIssue In issues
                If item Is Nothing Then Throw New ArgumentException("Null validation issues are not allowed.", NameOf(issues))
                items.Add(item)
            Next
            _issues = items.AsReadOnly()
        End Sub

        Public ReadOnly Property Issues As IReadOnlyList(Of SpreadsheetValidationIssue)
            Get
                Return _issues
            End Get
        End Property
        Public ReadOnly Property HasErrors As Boolean
            Get
                Return _issues.Any(Function(issue) issue.Severity = SpreadsheetValidationSeverity.[Error])
            End Get
        End Property
        Public ReadOnly Property HasWarnings As Boolean
            Get
                Return _issues.Any(Function(issue) issue.Severity = SpreadsheetValidationSeverity.Warning)
            End Get
        End Property
        Public ReadOnly Property IsValid As Boolean
            Get
                Return Not HasErrors
            End Get
        End Property
    End Class

    ''' <summary>An opt-in throwing facade for consumers that cannot use diagnostics results.</summary>
    Public NotInheritable Class SpreadsheetInputValidationException
        Inherits ArgumentException

        Public Sub New(report As SpreadsheetValidationReport)
            MyBase.New(If(report IsNot Nothing AndAlso report.Issues.Count > 0,
                          report.Issues(0).Message, "Spreadsheet input validation failed."), "input")
            If report Is Nothing Then Throw New ArgumentNullException(NameOf(report))
            Me.Report = report
        End Sub
        Public ReadOnly Property Report As SpreadsheetValidationReport
    End Class

End Namespace
