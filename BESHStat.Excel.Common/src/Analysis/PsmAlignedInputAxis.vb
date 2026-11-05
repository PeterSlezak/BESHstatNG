Option Explicit On
Option Strict On
Option Infer On
Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports BESHStatNG.CausalInference

Namespace ExcelCommon
    ''' <summary>Detached post-alignment row. Source row, input ordinal, display ID and match set
    ''' identity are distinct. Nothing source row records unavailable provenance, never a fabricated row.</summary>
    Public NotInheritable Class PsmAlignedRow
        Friend Sub New(ordinal As Integer, sourceRow As Integer?, id As String, effectiveId As String, treatment As Double, outcome As Double?, score As Double?, exactLabel As String)
            Me.Ordinal = ordinal : Me.SourceRow = sourceRow : Me.Id = id : Me.EffectiveId = effectiveId
            Me.Treatment = treatment : Me.Outcome = outcome : Me.SuppliedScore = score : Me.ExactLabel = exactLabel
        End Sub
        Public ReadOnly Property Ordinal As Integer
        Public ReadOnly Property SourceRow As Integer?
        Public ReadOnly Property Id As String
        Public ReadOnly Property EffectiveId As String
        Public ReadOnly Property Treatment As Double
        Public ReadOnly Property Outcome As Double?
        Public ReadOnly Property SuppliedScore As Double?
        Public ReadOnly Property ExactLabel As String
    End Class

    ''' <summary>Observation-only identity snapshot after native alignment/design expansion.
    ''' Does not retain or copy covariate matrices; diagnostics never reject a Windows request.</summary>
    Public NotInheritable Class PsmAlignedInputAxis
        Private ReadOnly sourceRows As Integer()
        Public Sub New(input As PsmInputData, sourceRowIds As Integer(), rawNames As IEnumerable(Of String), effectNames As IEnumerable(Of String), droppedRows As Integer)
            sourceRows = If(sourceRowIds Is Nothing, Nothing, CType(sourceRowIds.Clone(), Integer()))
            Me.DroppedRows = droppedRows
            RawCovariates = Array.AsReadOnly(If(rawNames, Enumerable.Empty(Of String)()).ToArray())
            Effects = Array.AsReadOnly(If(effectNames, Enumerable.Empty(Of String)()).ToArray())
            ExpandedCovariates = Array.AsReadOnly(If(input Is Nothing OrElse input.CovariateNames Is Nothing, New String() {}, CType(input.CovariateNames.Clone(), String())))
            HasExactGroups = input IsNot Nothing AndAlso input.ExactGroupLabels IsNot Nothing
            HasSuppliedScores = input IsNot Nothing AndAlso input.SuppliedPropensityScores IsNot Nothing
            Dim mapped As New List(Of PsmAlignedRow), notes As New List(Of String)
            If input IsNot Nothing Then
                If sourceRows Is Nothing OrElse sourceRows.Length <> input.RowCount Then notes.Add("C25-SOURCE-AXIS-SHAPE: source-row count differs from input; candidate only.")
                If sourceRows IsNot Nothing AndAlso sourceRows.Distinct().Count() <> sourceRows.Length Then notes.Add("C25-SOURCE-AXIS-DUPLICATE: repeated physical row identity; candidate only.")
                For i = 0 To input.RowCount - 1
                    Dim source As Integer? = Nothing
                    If sourceRows IsNot Nothing AndAlso i < sourceRows.Length Then source = sourceRows(i)
                    Dim id As String = Nothing, exact As String = Nothing, score As Double? = Nothing, outcome As Double? = Nothing
                    If input.Ids IsNot Nothing AndAlso i < input.Ids.Length Then id = input.Ids(i)
                    If input.ExactGroupLabels IsNot Nothing AndAlso i < input.ExactGroupLabels.Length Then exact = input.ExactGroupLabels(i)
                    If input.SuppliedPropensityScores IsNot Nothing AndAlso i < input.SuppliedPropensityScores.Length Then score = input.SuppliedPropensityScores(i)
                    If input.Outcome IsNot Nothing AndAlso i < input.Outcome.Length Then outcome = input.Outcome(i)
                    mapped.Add(New PsmAlignedRow(i, source, id, input.GetId(i), input.Treatment(i), outcome, score, exact))
                Next
            End If
            Rows = mapped.AsReadOnly() : CandidateDiagnostics = notes.AsReadOnly()
        End Sub
        Public ReadOnly Property Rows As IReadOnlyList(Of PsmAlignedRow)
        Public ReadOnly Property RawCovariates As IReadOnlyList(Of String)
        Public ReadOnly Property Effects As IReadOnlyList(Of String)
        Public ReadOnly Property ExpandedCovariates As IReadOnlyList(Of String)
        Public ReadOnly Property HasExactGroups As Boolean
        Public ReadOnly Property HasSuppliedScores As Boolean
        Public ReadOnly Property DroppedRows As Integer
        Public ReadOnly Property CandidateDiagnostics As IReadOnlyList(Of String)
        Public Function SourceRowIds() As Integer()
            Return If(sourceRows Is Nothing, Nothing, CType(sourceRows.Clone(), Integer()))
        End Function
    End Class
End Namespace
