Option Explicit On
Option Strict On

Imports System
Imports System.Collections.Generic
Imports System.Globalization

Namespace ExcelCommon

    Public Enum OutputArtifactKind
        Table = 1
        Diagnostic = 2
        Plot = 3
    End Enum

    Public Enum OutputDiagnosticSeverity
        Warning = 1
        Fault = 2
    End Enum

    ''' <summary>Closed immutable presentation family. IDs are ordinal, document-local keys.</summary>
    Public MustInherit Class OutputArtifact
        Friend Sub New(id As String, kind As OutputArtifactKind, caption As String)
            If String.IsNullOrWhiteSpace(id) Then Throw New ArgumentException("Artifact ID is required.", NameOf(id))
            Me.Id = id
            Me.Kind = kind
            Me.Caption = If(caption, String.Empty)
        End Sub

        Public ReadOnly Property Id As String
        Public ReadOnly Property Kind As OutputArtifactKind
        ''' <summary>Optional document-level caption; does not replace table title cells.</summary>
        Public ReadOnly Property Caption As String
    End Class

    ''' <summary>
    ''' In-memory snapshot of the entire assembled Core matrix and its metadata.
    ''' Model returns an independent copy. Only immutable scalar cell payloads are supported;
    ''' arbitrary objects cannot be made safe by a shallow array clone.
    ''' </summary>
    Public NotInheritable Class OutputTableArtifact
        Inherits OutputArtifact

        Private ReadOnly _model As Global.BESHStatNG.ResultTableOutputModel

        Public Sub New(id As String, model As Global.BESHStatNG.ResultTableOutputModel,
                       Optional caption As String = Nothing)
            MyBase.New(id, OutputArtifactKind.Table, caption)
            If model Is Nothing Then Throw New ArgumentNullException(NameOf(model))
            If model.Values Is Nothing Then Throw New ArgumentException("Table values are required.", NameOf(model))
            If model.Values.GetLowerBound(0) <> 0 OrElse model.Values.GetLowerBound(1) <> 0 Then
                Throw New ArgumentException("Table values must have zero-based array bounds.", NameOf(model))
            End If
            If model.TitleRows < 0 OrElse model.HeaderTopRows < 0 OrElse model.FooterRows < 0 OrElse
               model.HeaderLeftColumns < 0 OrElse model.HeaderLeftColumns > model.ColumnCount OrElse
               CLng(model.TitleRows) + model.HeaderTopRows + model.FooterRows > model.RowCount Then
                Throw New ArgumentException("Table section counts exceed the matrix dimensions or are negative.", NameOf(model))
            End If
            For Each value As Object In model.Values
                ValidateValue(value)
            Next
            _model = CopyModel(model)
        End Sub

        Public ReadOnly Property RowCount As Integer
            Get
                Return _model.RowCount
            End Get
        End Property

        Public ReadOnly Property ColumnCount As Integer
            Get
                Return _model.ColumnCount
            End Get
        End Property

        ''' <summary>
        ''' Includes actual title/footer rows. P-value annotations retain the Core DTO's exact
        ''' order, duplicates and values, including out-of-range entries; no placement is inferred.
        ''' Core ResultTable's AddPvalue* methods already filter invalid/duplicate annotations.
        ''' </summary>
        Public ReadOnly Property Model As Global.BESHStatNG.ResultTableOutputModel
            Get
                Return CopyModel(_model)
            End Get
        End Property

        Public Shared Function FromTable(id As String, table As Global.BESHStatNG.ResultTable,
                                         Optional caption As String = Nothing) As OutputTableArtifact
            If table Is Nothing Then Throw New ArgumentNullException(NameOf(table))
            Return New OutputTableArtifact(id, table.ToOutputModel(), caption)
        End Function

        Private Shared Function CopyModel(source As Global.BESHStatNG.ResultTableOutputModel) As Global.BESHStatNG.ResultTableOutputModel
            Return New Global.BESHStatNG.ResultTableOutputModel(
                DirectCast(source.Values.Clone(), Object(,)), source.HeaderTopRows, source.HeaderLeftColumns,
                source.FooterRows, source.PvalueColumns, source.TitleRows, source.IsResultTable, source.PvalueCells)
        End Function

        Private Shared Sub ValidateValue(value As Object)
            If value Is Nothing Then Return
            Dim valueType As Type = value.GetType()
            If Not valueType.IsEnum Then
                Select Case Type.GetTypeCode(valueType)
                    Case TypeCode.String, TypeCode.Boolean,
                         TypeCode.SByte, TypeCode.Byte, TypeCode.Int16, TypeCode.UInt16,
                         TypeCode.Int32, TypeCode.UInt32, TypeCode.Int64, TypeCode.UInt64,
                         TypeCode.Single, TypeCode.Double, TypeCode.Decimal
                        Return
                End Select
            End If
            Throw New ArgumentException("Unsupported output cell payload type: " & valueType.FullName & ".", NameOf(value))
        End Sub
    End Class

    Public NotInheritable Class OutputDiagnosticArtifact
        Inherits OutputArtifact

        Public Sub New(id As String, severity As OutputDiagnosticSeverity, code As String,
                       message As String, Optional fieldPath As String = Nothing)
            MyBase.New(id, OutputArtifactKind.Diagnostic, Nothing)
            If severity <> OutputDiagnosticSeverity.Warning AndAlso severity <> OutputDiagnosticSeverity.Fault Then
                Throw New ArgumentOutOfRangeException(NameOf(severity))
            End If
            If String.IsNullOrWhiteSpace(code) Then Throw New ArgumentException("Diagnostic code is required.", NameOf(code))
            If String.IsNullOrWhiteSpace(message) Then Throw New ArgumentException("Diagnostic message is required.", NameOf(message))
            Me.Severity = severity
            Me.Code = code
            Me.Message = message
            Me.FieldPath = If(fieldPath, String.Empty)
        End Sub

        Public ReadOnly Property Severity As OutputDiagnosticSeverity
        Public ReadOnly Property Code As String
        Public ReadOnly Property Message As String
        Public ReadOnly Property FieldPath As String
    End Class

    ''' <summary>
    ''' Opaque host-neutral reference and purpose only. No plot data, geometry or rendering promise.
    ''' ReferenceKey is a logical key, not a file path, URL, workbook address or host object.
    ''' </summary>
    Public NotInheritable Class OutputPlotArtifact
        Inherits OutputArtifact

        Public Sub New(id As String, referenceKey As String, purpose As String,
                       Optional caption As String = Nothing, Optional description As String = Nothing)
            MyBase.New(id, OutputArtifactKind.Plot, caption)
            If String.IsNullOrWhiteSpace(referenceKey) Then Throw New ArgumentException("Plot reference key is required.", NameOf(referenceKey))
            If String.IsNullOrWhiteSpace(purpose) Then Throw New ArgumentException("Plot purpose is required.", NameOf(purpose))
            Me.ReferenceKey = referenceKey
            Me.Purpose = purpose
            Me.Description = If(description, String.Empty)
        End Sub

        Public ReadOnly Property ReferenceKey As String
        Public ReadOnly Property Purpose As String
        Public ReadOnly Property Description As String
    End Class

    ''' <summary>
    ''' In-memory ordered presentation snapshot, intentionally without a serialization contract.
    ''' Analysis identity/status, metrics and typed detail remain in AnalysisResultDocument v1.
    ''' No cell normalization, semantic styles, destination or layout is performed here.
    ''' </summary>
    Public NotInheritable Class OutputDocument
        Private ReadOnly _artifacts As IReadOnlyList(Of OutputArtifact)

        Public Sub New(artifacts As IEnumerable(Of OutputArtifact))
            If artifacts Is Nothing Then Throw New ArgumentNullException(NameOf(artifacts))
            Dim items As New List(Of OutputArtifact)()
            Dim ids As New HashSet(Of String)(StringComparer.Ordinal)
            For Each artifact As OutputArtifact In artifacts
                If artifact Is Nothing Then Throw New ArgumentException("Null output artifact.", NameOf(artifacts))
                If Not ids.Add(artifact.Id) Then Throw New ArgumentException("Duplicate output artifact ID: " & artifact.Id, NameOf(artifacts))
                items.Add(artifact)
            Next
            _artifacts = items.AsReadOnly()
        End Sub

        Public ReadOnly Property Artifacts As IReadOnlyList(Of OutputArtifact)
            Get
                Return _artifacts
            End Get
        End Property

        Public Shared Function FromTable(table As Global.BESHStatNG.ResultTable) As OutputDocument
            Return New OutputDocument(New OutputArtifact() {OutputTableArtifact.FromTable("table-0", table)})
        End Function

        Public Shared Function FromModel(model As Global.BESHStatNG.ResultTableOutputModel) As OutputDocument
            Return New OutputDocument(New OutputArtifact() {New OutputTableArtifact("table-0", model)})
        End Function

        ''' <summary>Uses the captured analysis envelope, not subsequently mutated legacy Tables.</summary>
        Public Shared Function FromResult(result As SpreadsheetAnalysisResult) As OutputDocument
            If result Is Nothing Then Throw New ArgumentNullException(NameOf(result))
            Return FromAnalysis(result.Document)
        End Function

        ''' <summary>
        ''' Tables in source order, then warnings, then faults, then explicitly supplied trailing
        ''' artifacts. Each collection retains source order. Metrics/detail are not presentation.
        ''' For interleaving, construct OutputDocument directly with the desired sequence.
        ''' </summary>
        Public Shared Function FromAnalysis(source As AnalysisResultDocument,
                                            Optional trailingArtifacts As IEnumerable(Of OutputArtifact) = Nothing) As OutputDocument
            If source Is Nothing Then Throw New ArgumentNullException(NameOf(source))
            Dim items As New List(Of OutputArtifact)()
            For Each table As AnalysisTableArtifact In source.Tables
                items.Add(New OutputTableArtifact("table-" & table.Index.ToString(CultureInfo.InvariantCulture), table.Model))
            Next
            Dim index As Integer = 0
            For Each warning As AnalysisWarning In source.Warnings
                items.Add(New OutputDiagnosticArtifact("warning-" & index.ToString(CultureInfo.InvariantCulture),
                    OutputDiagnosticSeverity.Warning, warning.Code, warning.Message, warning.FieldPath))
                index += 1
            Next
            index = 0
            For Each fault As AnalysisFault In source.Faults
                items.Add(New OutputDiagnosticArtifact("fault-" & index.ToString(CultureInfo.InvariantCulture),
                    OutputDiagnosticSeverity.Fault, fault.Code, fault.Message, fault.FieldPath))
                index += 1
            Next
            If trailingArtifacts IsNot Nothing Then items.AddRange(trailingArtifacts)
            Return New OutputDocument(items)
        End Function
    End Class
End Namespace
