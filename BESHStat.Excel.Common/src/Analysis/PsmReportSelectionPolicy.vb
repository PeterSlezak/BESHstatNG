Option Explicit On
Option Strict On
Option Infer On
Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports BESHStatNG.CausalInference

Namespace ExcelCommon
    Public Enum PsmReportSection
        InputData
        Results
        Diagnostics
        RowAudit
        LovePlot
        MatchedPairs
        MatchedData
        Cem
    End Enum
    ''' <summary>Ordered immutable sheet intent. Each build delegates to existing Core table APIs
    ''' and returns a fresh list. Excel creation, skip-empty handling and warning catches stay native.</summary>
    Public NotInheritable Class PsmReportSheet
        Friend Sub New(section As PsmReportSection, sheetName As String)
            Me.Section = section : Me.SheetName = sheetName
        End Sub
        Public ReadOnly Property Section As PsmReportSection
        Public ReadOnly Property SheetName As String
        Public Function BuildTables(input As PsmInputData, sourceRows As Integer(), result As PsmComprehensiveResult, options As PsmComprehensiveFitOptions, summary As Object(,)) As List(Of ResultTable)
            Select Case Section
                Case PsmReportSection.InputData
                    Return New List(Of ResultTable) From {PsmFormattedResultTables.AnalyzedInputDataTable(input, sourceRows)}
                Case PsmReportSection.Results
                    Return PsmFormattedResultTables.GeneralResultTables(result, options, summary)
                Case PsmReportSection.Diagnostics
                    Return PsmFormattedResultTables.DiagnosticsTables(input, result, includeDefaultSensitivity:=True)
                Case PsmReportSection.RowAudit
                    Return PsmFormattedResultTables.RowAuditTables(input, result, sourceRows)
                Case PsmReportSection.MatchedPairs
                    Return PsmFormattedResultTables.MatchedPairsTables(input, result)
                Case PsmReportSection.MatchedData
                    Return PsmFormattedResultTables.MatchedDatasetTables(input, result, sourceRows)
                Case PsmReportSection.Cem
                    Return PsmFormattedResultTables.CoarsenedExactTables(input, result)
                Case Else
                    Return New List(Of ResultTable)
            End Select
        End Function
    End Class
    Public NotInheritable Class PsmReportSelectionPolicy
        Private Sub New()
        End Sub
        ''' <summary>Phase selections allow the native handler to read checkboxes at its original
        ''' boundaries, including its second BuildFitOptions call. No re-entrant state fix is applied.</summary>
        Public Shared Function Plan(diagnostics As Boolean, matches As Boolean, love As Boolean, hasLoveRows As Boolean, hasCem As Boolean) As IReadOnlyList(Of PsmReportSheet)
            Dim sheets As New List(Of PsmReportSheet) From {New PsmReportSheet(PsmReportSection.InputData, "Input Data"), New PsmReportSheet(PsmReportSection.Results, "Results")}
            sheets.AddRange(DiagnosticSheets(diagnostics))
            sheets.AddRange(LoveSheets(love, hasLoveRows))
            sheets.AddRange(MatchSheets(matches))
            sheets.AddRange(CemSheets(hasCem))
            Return sheets.AsReadOnly()
        End Function
        Public Shared Function DiagnosticSheets(selected As Boolean) As IReadOnlyList(Of PsmReportSheet)
            Return Array.AsReadOnly(If(selected, {New PsmReportSheet(PsmReportSection.Diagnostics, "Diagnostics"), New PsmReportSheet(PsmReportSection.RowAudit, "Row Audit")}, New PsmReportSheet() {}))
        End Function
        Public Shared Function LoveSheets(selected As Boolean, available As Boolean) As IReadOnlyList(Of PsmReportSheet)
            Return Array.AsReadOnly(If(selected AndAlso available, {New PsmReportSheet(PsmReportSection.LovePlot, "Love Plot")}, New PsmReportSheet() {}))
        End Function
        Public Shared Function MatchSheets(selected As Boolean) As IReadOnlyList(Of PsmReportSheet)
            Return Array.AsReadOnly(If(selected, {New PsmReportSheet(PsmReportSection.MatchedPairs, "Matched Pairs"), New PsmReportSheet(PsmReportSection.MatchedData, "Matched Data")}, New PsmReportSheet() {}))
        End Function
        Public Shared Function CemSheets(available As Boolean) As IReadOnlyList(Of PsmReportSheet)
            Return Array.AsReadOnly(If(available, {New PsmReportSheet(PsmReportSection.Cem, "CEM")}, New PsmReportSheet() {}))
        End Function
    End Class
End Namespace
