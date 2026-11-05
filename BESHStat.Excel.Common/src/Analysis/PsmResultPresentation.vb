Option Explicit On
Option Strict On
Option Infer On
Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports BESHStatNG.CausalInference

Namespace ExcelCommon
    ''' <summary>Detached Core link identities, retaining order and multiplicity. SetId is never a source row.</summary>
    Public NotInheritable Class PsmMatchIdentity
        Friend Sub New(link As PsmMatchLink)
            SetId = link.SetId : FocalOrdinal = link.FocalRowIndex : MatchedOrdinal = link.MatchedRowIndex
            TreatedOrdinal = link.TreatedRowIndex : ControlOrdinal = link.ControlRowIndex
        End Sub
        Public ReadOnly Property SetId As Integer
        Public ReadOnly Property FocalOrdinal As Integer
        Public ReadOnly Property MatchedOrdinal As Integer
        Public ReadOnly Property TreatedOrdinal As Integer
        Public ReadOnly Property ControlOrdinal As Integer
    End Class
    Public NotInheritable Class PsmStratumIdentity
        Friend Sub New(stratum As PsmCemStratumRow)
            Id = stratum.StratumId : Key = stratum.Key : Retained = stratum.Retained
        End Sub
        Public ReadOnly Property Id As Integer
        Public ReadOnly Property Key As String
        Public ReadOnly Property Retained As Boolean
    End Class
    Public NotInheritable Class PsmRowStatus
        Friend Sub New(ordinal As Integer, result As PsmResult, cem As PsmCoarsenedExactResult, observation As PsmObservation)
            Me.Ordinal = ordinal
            MatchingWeight = At(result.MatchingWeights, ordinal) : BalancingWeight = At(result.BalancingWeights, ordinal)
            If cem IsNot Nothing AndAlso cem.RowStratumIds IsNot Nothing AndAlso ordinal < cem.RowStratumIds.Length Then CemStratum = cem.RowStratumIds(ordinal)
            If observation IsNot Nothing Then
                IncludedBySupport = observation.IncludedByCommonSupport : IncludedByTrimming = observation.IncludedByTrimming
            End If
        End Sub
        Private Shared Function At(values As Double(), ordinal As Integer) As Double?
            If values Is Nothing OrElse ordinal >= values.Length Then Return Nothing
            Return values(ordinal)
        End Function
        Public ReadOnly Property Ordinal As Integer
        Public ReadOnly Property MatchingWeight As Double?
        Public ReadOnly Property BalancingWeight As Double?
        Public ReadOnly Property CemStratum As Integer?
        Public ReadOnly Property IncludedBySupport As Boolean?
        Public ReadOnly Property IncludedByTrimming As Boolean?
    End Class
    ''' <summary>Post-fit metadata only, from one existing Core result. Does not compute an estimator.</summary>
    Public NotInheritable Class PsmResultPresentation
        Public Sub New(inputRowCount As Integer, result As PsmComprehensiveResult)
            Dim links As New List(Of PsmMatchIdentity), statuses As New List(Of PsmRowStatus), notes As New List(Of String)
            If result IsNot Nothing Then
                RunMethod = result.RunMethod : HasCem = result.CoarsenedExactResult IsNot Nothing
                HasLoveRows = result.LovePlotRows IsNot Nothing AndAlso result.LovePlotRows.Count > 0
                HasOverlapDiagnostics = result.OverlapDiagnostics IsNot Nothing
                HasDoublyRobust = result.DoublyRobustResult IsNot Nothing
                HasWeightDiagnostics = result.WeightDiagnostics IsNot Nothing AndAlso result.WeightDiagnostics.Count > 0
                If HasCem Then CemStrata = Array.AsReadOnly(If(result.CoarsenedExactResult.Strata, New List(Of PsmCemStratumRow)).Where(Function(s) s IsNot Nothing).Select(Function(s) New PsmStratumIdentity(s)).ToArray())
                Warnings = Array.AsReadOnly(If(result.Warnings, New List(Of String)).ToArray())
                Dim core = result.Result
                If core IsNot Nothing Then
                    If core.Options IsNot Nothing Then Estimand = core.Options.Estimand
                    If core.ScoreModel IsNot Nothing Then
                        ScoreConverged = core.ScoreModel.Converged : ScoreMethod = core.ScoreModel.Method
                    End If
                    BalanceRowCount = If(core.Balance Is Nothing, 0, core.Balance.Count)
                    For Each link In If(core.Matches, New List(Of PsmMatchLink))
                        If link Is Nothing Then
                            notes.Add("C25-MATCH-AXIS: missing link; candidate only.") : Continue For
                        End If
                        links.Add(New PsmMatchIdentity(link))
                        If {link.FocalRowIndex, link.MatchedRowIndex, link.TreatedRowIndex, link.ControlRowIndex}.Any(Function(i) i < 0 OrElse i >= inputRowCount) Then notes.Add("C25-MATCH-AXIS: out-of-range Core ordinal; candidate only.")
                    Next
                    'Index explicit Core ordinals once; duplicate observations retain first occurrence.
                    Dim byRow = If(core.Observations, New List(Of PsmObservation)).Where(Function(o) o IsNot Nothing).GroupBy(Function(o) o.RowIndex).ToDictionary(Function(g) g.Key, Function(g) g.First())
                    For i = 0 To inputRowCount - 1
                        Dim observation As PsmObservation = Nothing
                        byRow.TryGetValue(i, observation)
                        statuses.Add(New PsmRowStatus(i, core, result.CoarsenedExactResult, observation))
                    Next
                    SubclassIds = Array.AsReadOnly(If(core.Subclasses, New List(Of PsmSubclassRow)).Select(Function(s) s.Stratum).ToArray())
                End If
            End If
            If Warnings Is Nothing Then Warnings = Array.AsReadOnly(New String() {})
            If SubclassIds Is Nothing Then SubclassIds = Array.AsReadOnly(New Integer() {})
            If CemStrata Is Nothing Then CemStrata = Array.AsReadOnly(New PsmStratumIdentity() {})
            Matches = links.AsReadOnly() : Rows = statuses.AsReadOnly() : CandidateDiagnostics = notes.AsReadOnly()
        End Sub
        Public ReadOnly Property RunMethod As PsmBackendRunMethod
        Public ReadOnly Property Estimand As PsmEstimand?
        Public ReadOnly Property ScoreConverged As Boolean?
        Public ReadOnly Property ScoreMethod As PsmScoreMethod?
        Public ReadOnly Property BalanceRowCount As Integer
        Public ReadOnly Property HasCem As Boolean
        Public ReadOnly Property HasLoveRows As Boolean
        Public ReadOnly Property HasOverlapDiagnostics As Boolean
        Public ReadOnly Property HasDoublyRobust As Boolean
        Public ReadOnly Property HasWeightDiagnostics As Boolean
        Public ReadOnly Property Matches As IReadOnlyList(Of PsmMatchIdentity)
        Public ReadOnly Property Rows As IReadOnlyList(Of PsmRowStatus)
        Public ReadOnly Property SubclassIds As IReadOnlyList(Of Integer)
        Public ReadOnly Property CemStrata As IReadOnlyList(Of PsmStratumIdentity)
        Public ReadOnly Property Warnings As IReadOnlyList(Of String)
        Public ReadOnly Property CandidateDiagnostics As IReadOnlyList(Of String)
    End Class
End Namespace
