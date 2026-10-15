Option Explicit On
Option Strict On

Imports System
Imports System.Collections.Generic

Namespace ExcelCommon

    ''' <summary>Result-document contract version 1. Independent of the future Excel placement contract.</summary>
    Public Enum AnalysisDocumentStatus
        Succeeded = 0
        Failed = 1
    End Enum

    Public Enum AnalysisArtifactKind
        Table = 1
        Metric = 2
        TypedDetail = 3
    End Enum

    Public Enum AnalysisMetricRole
        Statistic = 0
        PValue = 1
    End Enum

    Public NotInheritable Class AnalysisFaultCodes
        Private Sub New()
        End Sub
        Public Const InvalidRequest As String = "analysis.invalid-request"
        Public Const UnsupportedAnalysis As String = "analysis.unsupported"
        Public Const ExecutionFailed As String = "analysis.execution-failed"
    End Class

    Public NotInheritable Class AnalysisWarningCodes
        Private Sub New()
        End Sub
        Public Const AlphaDefaulted As String = "options.alpha.defaulted"
    End Class

    ''' <summary>Safe, structured diagnostics. Codes are stable; messages are for display.</summary>
    Public NotInheritable Class AnalysisWarning
        Public Sub New(code As String, message As String, Optional fieldPath As String = Nothing)
            If String.IsNullOrWhiteSpace(code) Then Throw New ArgumentException("Warning code is required.", NameOf(code))
            If String.IsNullOrWhiteSpace(message) Then Throw New ArgumentException("Warning message is required.", NameOf(message))
            Me.Code = code
            Me.Message = message
            Me.FieldPath = If(fieldPath, String.Empty)
        End Sub
        Public ReadOnly Property Code As String
        Public ReadOnly Property Message As String
        Public ReadOnly Property FieldPath As String
    End Class

    Public NotInheritable Class AnalysisFault
        Public Sub New(code As String, message As String, Optional fieldPath As String = Nothing)
            If String.IsNullOrWhiteSpace(code) Then Throw New ArgumentException("Fault code is required.", NameOf(code))
            If String.IsNullOrWhiteSpace(message) Then Throw New ArgumentException("Fault message is required.", NameOf(message))
            Me.Code = code
            Me.Message = message
            Me.FieldPath = If(fieldPath, String.Empty)
        End Sub
        Public ReadOnly Property Code As String
        Public ReadOnly Property Message As String
        Public ReadOnly Property FieldPath As String
    End Class

    ''' <summary>Closed v1 artifact family. Additional kinds require an explicit contract revision.</summary>
    Public MustInherit Class AnalysisResultArtifact
        Protected Sub New(kind As AnalysisArtifactKind)
            Me.Kind = kind
        End Sub
        Public ReadOnly Property Kind As AnalysisArtifactKind
    End Class

    ''' <summary>
    ''' Table position is its zero-based sequence in the statistical result, NOT an Excel cell.
    ''' The retained table is copied; access returns another copy so it cannot be mutated by callers.
    ''' </summary>
    Public NotInheritable Class AnalysisTableArtifact
        Inherits AnalysisResultArtifact

        Private ReadOnly _model As Global.BESHStatNG.ResultTableOutputModel

        Public Sub New(index As Integer, model As Global.BESHStatNG.ResultTableOutputModel)
            MyBase.New(AnalysisArtifactKind.Table)
            If index < 0 Then Throw New ArgumentOutOfRangeException(NameOf(index))
            If model Is Nothing Then Throw New ArgumentNullException(NameOf(model))
            Me.Index = index
            _model = CloneTable(model)
        End Sub

        Public ReadOnly Property Index As Integer
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
        Public ReadOnly Property Model As Global.BESHStatNG.ResultTableOutputModel
            Get
                Return CloneTable(_model)
            End Get
        End Property

        Private Shared Function CloneTable(source As Global.BESHStatNG.ResultTableOutputModel) As Global.BESHStatNG.ResultTableOutputModel
            Dim clonedValues As Object(,) = Nothing
            If source.Values IsNot Nothing Then clonedValues = DirectCast(source.Values.Clone(), Object(,))
            Return New Global.BESHStatNG.ResultTableOutputModel(
                clonedValues, source.HeaderTopRows, source.HeaderLeftColumns,
                source.FooterRows, source.PvalueColumns, source.TitleRows,
                source.IsResultTable, source.PvalueCells)
        End Function
    End Class

    Public NotInheritable Class AnalysisMetricArtifact
        Inherits AnalysisResultArtifact

        Public Sub New(name As String, value As Double, role As AnalysisMetricRole, Optional unit As String = Nothing)
            MyBase.New(AnalysisArtifactKind.Metric)
            If String.IsNullOrWhiteSpace(name) Then Throw New ArgumentException("Metric name is required.", NameOf(name))
            Me.Name = name
            Me.Value = value
            Me.Role = role
            Me.Unit = If(unit, String.Empty)
        End Sub
        Public ReadOnly Property Name As String
        Public ReadOnly Property Value As Double
        Public ReadOnly Property Role As AnalysisMetricRole
        Public ReadOnly Property Unit As String
    End Class

    ''' <summary>
    ''' A typed access boundary around the existing analysis-specific detail. The raw object
    ''' never appears in the serializable snapshot; consumers explicitly request its type.
    ''' </summary>
    Public NotInheritable Class AnalysisDetailArtifact
        Inherits AnalysisResultArtifact
        Private ReadOnly _detail As Object

        Private Sub New(value As Object)
            MyBase.New(AnalysisArtifactKind.TypedDetail)
            If value Is Nothing Then Throw New ArgumentNullException(NameOf(value))
            _detail = value
            DetailTypeName = value.GetType().FullName
        End Sub

        Public ReadOnly Property DetailTypeName As String

        Public Shared Function FromValue(Of TDetail As Class)(value As TDetail) As AnalysisDetailArtifact
            If value Is Nothing Then Throw New ArgumentNullException(NameOf(value))
            Return New AnalysisDetailArtifact(value)
        End Function

        Public Function TryGet(Of TDetail As Class)(ByRef detail As TDetail) As Boolean
            detail = TryCast(_detail, TDetail)
            Return detail IsNot Nothing
        End Function
    End Class

    ''' <summary>
    ''' Portable, ordered, read-only result envelope. Tables/metrics are snapshots; typed details
    ''' are reference semantics until the family-specific DTO contract is implemented in 02.xx.
    ''' No workbook placement, formatting, COM or Office.js state appears here.
    ''' </summary>
    Public NotInheritable Class AnalysisResultDocument
        Public Const SchemaVersion As Integer = 1
        Private ReadOnly _artifacts As IReadOnlyList(Of AnalysisResultArtifact)
        Private ReadOnly _warnings As IReadOnlyList(Of AnalysisWarning)
        Private ReadOnly _faults As IReadOnlyList(Of AnalysisFault)
        Private ReadOnly _tables As IReadOnlyList(Of AnalysisTableArtifact)
        Private ReadOnly _metrics As IReadOnlyList(Of AnalysisMetricArtifact)
        Private ReadOnly _detail As AnalysisDetailArtifact

        Public Sub New(analysisId As String,
                       tables As IEnumerable(Of Global.BESHStatNG.ResultTableOutputModel),
                       metrics As IEnumerable(Of KeyValuePair(Of String, Double)),
                       detail As AnalysisDetailArtifact,
                       Optional warnings As IEnumerable(Of AnalysisWarning) = Nothing,
                       Optional faults As IEnumerable(Of AnalysisFault) = Nothing)
            If String.IsNullOrWhiteSpace(analysisId) Then Throw New ArgumentException("Analysis ID is required.", NameOf(analysisId))
            If tables Is Nothing Then Throw New ArgumentNullException(NameOf(tables))
            If metrics Is Nothing Then Throw New ArgumentNullException(NameOf(metrics))
            Me.AnalysisId = analysisId

            Dim warningsList As New List(Of AnalysisWarning)()
            If warnings IsNot Nothing Then
                For Each item As AnalysisWarning In warnings
                    If item Is Nothing Then Throw New ArgumentException("Null warning.", NameOf(warnings))
                    warningsList.Add(item)
                Next
            End If
            Dim faultList As New List(Of AnalysisFault)()
            If faults IsNot Nothing Then
                For Each item As AnalysisFault In faults
                    If item Is Nothing Then Throw New ArgumentException("Null fault.", NameOf(faults))
                    faultList.Add(item)
                Next
            End If
            _warnings = warningsList.AsReadOnly()
            _faults = faultList.AsReadOnly()
            Status = If(faultList.Count > 0, AnalysisDocumentStatus.Failed, AnalysisDocumentStatus.Succeeded)

            Dim outputTables As New List(Of AnalysisTableArtifact)()
            For Each model As Global.BESHStatNG.ResultTableOutputModel In tables
                If model Is Nothing Then Throw New ArgumentException("Null result table.", NameOf(tables))
                outputTables.Add(New AnalysisTableArtifact(outputTables.Count, model))
            Next
            _tables = outputTables.AsReadOnly()

            Dim outputMetrics As New List(Of AnalysisMetricArtifact)()
            Dim names As New HashSet(Of String)(StringComparer.Ordinal)
            For Each pair As KeyValuePair(Of String, Double) In metrics
                If Not names.Add(pair.Key) Then Throw New ArgumentException("Duplicate metric: " & pair.Key, NameOf(metrics))
                Dim role As AnalysisMetricRole = If(IsKnownPValue(pair.Key), AnalysisMetricRole.PValue, AnalysisMetricRole.Statistic)
                outputMetrics.Add(New AnalysisMetricArtifact(pair.Key, pair.Value, role))
            Next
            outputMetrics.Sort(Function(a, b) StringComparer.Ordinal.Compare(a.Name, b.Name))
            _metrics = outputMetrics.AsReadOnly()
            _detail = detail

            Dim allArtifacts As New List(Of AnalysisResultArtifact)()
            For Each table As AnalysisTableArtifact In outputTables
                allArtifacts.Add(table)
            Next
            For Each metric As AnalysisMetricArtifact In outputMetrics
                allArtifacts.Add(metric)
            Next
            If detail IsNot Nothing Then allArtifacts.Add(detail)
            _artifacts = allArtifacts.AsReadOnly()
        End Sub

        Public ReadOnly Property AnalysisId As String
        Public ReadOnly Property Status As AnalysisDocumentStatus
        Public ReadOnly Property Artifacts As IReadOnlyList(Of AnalysisResultArtifact)
            Get
                Return _artifacts
            End Get
        End Property
        Public ReadOnly Property Tables As IReadOnlyList(Of AnalysisTableArtifact)
            Get
                Return _tables
            End Get
        End Property
        Public ReadOnly Property Metrics As IReadOnlyList(Of AnalysisMetricArtifact)
            Get
                Return _metrics
            End Get
        End Property
        Public ReadOnly Property Warnings As IReadOnlyList(Of AnalysisWarning)
            Get
                Return _warnings
            End Get
        End Property
        Public ReadOnly Property Faults As IReadOnlyList(Of AnalysisFault)
            Get
                Return _faults
            End Get
        End Property
        Public ReadOnly Property TypedDetail As AnalysisDetailArtifact
            Get
                Return _detail
            End Get
        End Property

        Public Function TryGetDetail(Of TDetail As Class)(ByRef value As TDetail) As Boolean
            value = Nothing
            Return _detail IsNot Nothing AndAlso _detail.TryGet(Of TDetail)(value)
        End Function

        ''' <summary>Produces a JSON-safe structural snapshot, not a replacement for 02.xx output-cell serialization.</summary>
        Public Function ToContractSnapshot() As AnalysisResultDocumentSnapshot
            Dim snapshot As New AnalysisResultDocumentSnapshot With {
                .SchemaVersion = SchemaVersion,
                .AnalysisId = AnalysisId,
                .Status = Status,
                .DetailTypeName = If(_detail Is Nothing, String.Empty, _detail.DetailTypeName)
            }
            For Each table As AnalysisTableArtifact In _tables
                Dim model As Global.BESHStatNG.ResultTableOutputModel = table.Model
                snapshot.Tables.Add(New AnalysisTableSnapshot With {
                    .Index = table.Index, .RowCount = table.RowCount, .ColumnCount = table.ColumnCount,
                    .TitleRows = model.TitleRows, .HeaderTopRows = model.HeaderTopRows,
                    .HeaderLeftColumns = model.HeaderLeftColumns, .FooterRows = model.FooterRows,
                    .IsResultTable = model.IsResultTable,
                    .PvalueColumns = New List(Of Integer)(model.PvalueColumns)
                })
                Dim lastTable As AnalysisTableSnapshot = snapshot.Tables(snapshot.Tables.Count - 1)
                For Each cell As Global.BESHStatNG.ResultTableCellAddress In model.PvalueCells
                    lastTable.PvalueCells.Add(New AnalysisCellAddressSnapshot With {
                        .BodyRow = cell.BodyRow, .BodyColumn = cell.BodyColumn})
                Next
            Next
            For Each metric As AnalysisMetricArtifact In _metrics
                Dim marker As String = "Finite"
                Dim nullableValue As Nullable(Of Double) = metric.Value
                If Double.IsNaN(metric.Value) Then
                    marker = "NaN"
                    nullableValue = Nothing
                ElseIf Double.IsPositiveInfinity(metric.Value) Then
                    marker = "PositiveInfinity"
                    nullableValue = Nothing
                ElseIf Double.IsNegativeInfinity(metric.Value) Then
                    marker = "NegativeInfinity"
                    nullableValue = Nothing
                End If
                snapshot.Metrics.Add(New AnalysisMetricSnapshot With {
                    .Name = metric.Name, .Value = nullableValue, .NumericKind = marker,
                    .Role = metric.Role, .Unit = metric.Unit
                })
            Next
            For Each warning As AnalysisWarning In _warnings
                snapshot.Warnings.Add(New AnalysisIssueSnapshot With {
                    .Code = warning.Code, .Message = warning.Message, .FieldPath = warning.FieldPath
                })
            Next
            For Each fault As AnalysisFault In _faults
                snapshot.Faults.Add(New AnalysisIssueSnapshot With {
                    .Code = fault.Code, .Message = fault.Message, .FieldPath = fault.FieldPath
                })
            Next
            Return snapshot
        End Function

        Public Shared Function Failure(analysisId As String, fault As AnalysisFault) As AnalysisResultDocument
            If fault Is Nothing Then Throw New ArgumentNullException(NameOf(fault))
            Return New AnalysisResultDocument(analysisId,
                Array.Empty(Of Global.BESHStatNG.ResultTableOutputModel)(),
                Array.Empty(Of KeyValuePair(Of String, Double))(), Nothing,
                faults:=New AnalysisFault() {fault})
        End Function

        Private Shared Function IsKnownPValue(name As String) As Boolean
            Return String.Equals(name, AnalysisMetricNames.PValue, StringComparison.Ordinal) OrElse
                   String.Equals(name, AnalysisMetricNames.WelchPValue, StringComparison.Ordinal) OrElse
                   String.Equals(name, AnalysisMetricNames.TieCorrectedPValue, StringComparison.Ordinal)
        End Function
    End Class

    ''' <summary>
    ''' JSON-friendly data transfer representation for contract diagnostics. The ordered result
    ''' table cell payload is intentionally deferred to the output contract in Phase 02.
    ''' </summary>
    Public NotInheritable Class AnalysisResultDocumentSnapshot
        Public Property SchemaVersion As Integer
        Public Property AnalysisId As String
        Public Property Status As AnalysisDocumentStatus
        Public Property DetailTypeName As String
        Public Property Tables As List(Of AnalysisTableSnapshot) = New List(Of AnalysisTableSnapshot)()
        Public Property Metrics As List(Of AnalysisMetricSnapshot) = New List(Of AnalysisMetricSnapshot)()
        Public Property Warnings As List(Of AnalysisIssueSnapshot) = New List(Of AnalysisIssueSnapshot)()
        Public Property Faults As List(Of AnalysisIssueSnapshot) = New List(Of AnalysisIssueSnapshot)()
    End Class

    Public NotInheritable Class AnalysisTableSnapshot
        Public Property Index As Integer
        Public Property RowCount As Integer
        Public Property ColumnCount As Integer
        Public Property TitleRows As Integer
        Public Property HeaderTopRows As Integer
        Public Property HeaderLeftColumns As Integer
        Public Property FooterRows As Integer
        Public Property IsResultTable As Boolean
        Public Property PvalueColumns As List(Of Integer) = New List(Of Integer)()
        Public Property PvalueCells As List(Of AnalysisCellAddressSnapshot) = New List(Of AnalysisCellAddressSnapshot)()
    End Class

    Public NotInheritable Class AnalysisCellAddressSnapshot
        Public Property BodyRow As Integer
        Public Property BodyColumn As Integer
    End Class

    Public NotInheritable Class AnalysisMetricSnapshot
        Public Property Name As String
        Public Property Value As Nullable(Of Double)
        Public Property NumericKind As String
        Public Property Role As AnalysisMetricRole
        Public Property Unit As String
    End Class

    Public NotInheritable Class AnalysisIssueSnapshot
        Public Property Code As String
        Public Property Message As String
        Public Property FieldPath As String
    End Class

End Namespace
