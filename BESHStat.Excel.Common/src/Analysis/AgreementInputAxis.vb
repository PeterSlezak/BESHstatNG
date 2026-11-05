Option Explicit On
Option Strict On
Option Infer On
Imports System
Imports System.Collections.Generic
Imports System.Linq

Namespace ExcelCommon
    ''' <summary>Post-conversion observation axis. Never parses, filters, coerces, sorts or re-pairs.
    ''' Cell/subject values are detached primitive host values; arrays are copied without string conversion.</summary>
    Public NotInheritable Class AgreementInputAxis
        Private ReadOnly firstValues As Double(), secondValues As Double(), categoryFirst As Object(), categorySecond As Object(), subjects As Object()
        Public Sub New(count As Integer, names As IEnumerable(Of String), sourceRows As IEnumerable(Of Integer),
                       Optional first As Double() = Nothing, Optional second As Double() = Nothing,
                       Optional subjectIds As Object() = Nothing, Optional categoricalFirst As Object() = Nothing,
                       Optional categoricalSecond As Object() = Nothing, Optional header As Boolean? = Nothing)
            Me.Count = count : Me.Names = Array.AsReadOnly(If(names, Enumerable.Empty(Of String)()).ToArray()) : Me.Header = header
            firstValues = Clone(first) : secondValues = Clone(second)
            categoryFirst = CloneObjects(categoricalFirst) : categorySecond = CloneObjects(categoricalSecond) : subjects = CloneObjects(subjectIds)
            IsCategorical = categoricalFirst IsNot Nothing OrElse categoricalSecond IsNot Nothing
            Dim rows = If(sourceRows Is Nothing, Nothing, sourceRows.ToArray())
            Dim mapped As New List(Of Integer?), issues As New List(Of SpreadsheetValidationIssue)
            For i = 0 To count - 1
                Dim physical As Integer? = Nothing
                If rows IsNot Nothing AndAlso i < rows.Length Then physical = rows(i)
                mapped.Add(physical)
            Next
            If rows IsNot Nothing Then
                'Identity-only reuse of the accepted alignment contract, no new range grammar.
                Alignment = New SpreadsheetRowAlignment(rows, rows)
                If rows.Length <> count Then issues.Add(New SpreadsheetValidationIssue("agreement.axis.shape", SpreadsheetValidationSeverity.Warning, "Source row count differs from observations; candidate only.", "input.rows"))
            End If
            PhysicalRows = mapped.AsReadOnly() : CandidateDiagnostics = New SpreadsheetValidationReport(issues)
        End Sub
        Public ReadOnly Property Count As Integer
        Public ReadOnly Property Names As IReadOnlyList(Of String)
        Public ReadOnly Property PhysicalRows As IReadOnlyList(Of Integer?)
        Public ReadOnly Property Alignment As SpreadsheetRowAlignment
        Public ReadOnly Property Header As Boolean?
        Public ReadOnly Property IsCategorical As Boolean
        Public ReadOnly Property CandidateDiagnostics As SpreadsheetValidationReport
        Public Function First() As Double()
            Return Clone(firstValues)
        End Function
        Public Function Second() As Double()
            Return Clone(secondValues)
        End Function
        Public Function FirstCategories() As Object()
            Return CloneObjects(categoryFirst)
        End Function
        Public Function SecondCategories() As Object()
            Return CloneObjects(categorySecond)
        End Function
        Public Function SubjectIds() As Object()
            Return CloneObjects(subjects)
        End Function
        Private Shared Function Clone(value As Double()) As Double()
            Return If(value Is Nothing, Nothing, CType(value.Clone(), Double()))
        End Function
        Private Shared Function CloneObjects(value As Object()) As Object()
            Return If(value Is Nothing, Nothing, CType(value.Clone(), Object()))
        End Function
        Public Shared Function FromBlocks(blocks As SpreadsheetNumericBlocks) As AgreementInputAxis
            If blocks Is Nothing Then Return New AgreementInputAxis(0, Nothing, Nothing)
            Return New AgreementInputAxis(blocks.Blocks.Count, blocks.Names, blocks.Blocks.Select(Function(b) b(0).WorksheetRow))
        End Function
    End Class
End Namespace
