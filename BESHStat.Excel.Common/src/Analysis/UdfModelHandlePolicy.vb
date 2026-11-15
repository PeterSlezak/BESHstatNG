Option Explicit On
Option Strict On

Imports System

Namespace ExcelCommon
    Public Enum UdfHandleFamily
        Unknown
        LM
        GLM
        GLMNB
        ZIP
        GEE
        COX
        ORDLOGIT
        MNLOGIT
        LMM
        MMRM
        PCA
        FA
        KMEANS
        HCLUST
        CA
        MCA
        DA
        PSM
    End Enum

    Public Enum UdfHandleSyntax
        Blank
        GeneratedShape
        UnknownFamily
        Malformed
    End Enum

    Public Enum UdfHandleScopeEvidence
        NotEstablished
        VerifiedByHost
    End Enum

    Public Enum UdfHandleState
        Unobserved
        PresentObserved
        Removed
        FamilyCleared
        SessionEnded
        WorkbookClosed
    End Enum

    Public Enum UdfHandleValidity
        HostLookupRequired
        Blank
        Available
        NotFound
        ExplicitlyInvalidated
        WrongVerifiedSession
        WrongVerifiedWorkbook
        FamilyMismatchReportOnly
    End Enum

    Public Enum UdfHandleEvent
        ExplicitDrop
        FamilyClear
        SessionEnd
        WorkbookClose
        WorkbookSwitch
        WorkbookReopen
        RepeatedFit
    End Enum

    Public Enum UdfHandleAction
        None
        RemoveKey
        ClearFamily
        CreateIndependentHandle
        RecommendInvalidation
        NativeScopeNotVerified
    End Enum

    ''' <summary>Immutable host-provided metadata. Contains no fitted object or worksheet data.
    ''' Keys are opaque and must not be logged. Epochs are host-issued opaque generation numbers,
    ''' not workbook paths or inferred identity. Existing Windows adapters establish neither epoch.</summary>
    Public NotInheritable Class UdfModelHandleDescriptor
        Public Sub New(convertedKey As String, family As UdfHandleFamily,
                       Optional state As UdfHandleState = UdfHandleState.Unobserved,
                       Optional sessionEpoch As Long? = Nothing,
                       Optional sessionEvidence As UdfHandleScopeEvidence = UdfHandleScopeEvidence.NotEstablished,
                       Optional workbookEpoch As Long? = Nothing,
                       Optional workbookEvidence As UdfHandleScopeEvidence = UdfHandleScopeEvidence.NotEstablished,
                       Optional resultTypeVersion As String = Nothing)
            Me.ConvertedKey = convertedKey : Me.Family = family : Me.State = state
            Me.SessionEpoch = sessionEpoch : Me.SessionEvidence = sessionEvidence
            Me.WorkbookEpoch = workbookEpoch : Me.WorkbookEvidence = workbookEvidence
            Me.ResultTypeVersion = resultTypeVersion
        End Sub
        Public ReadOnly Property ConvertedKey As String
        Public ReadOnly Property Family As UdfHandleFamily
        Public ReadOnly Property State As UdfHandleState
        Public ReadOnly Property SessionEpoch As Long?
        Public ReadOnly Property SessionEvidence As UdfHandleScopeEvidence
        Public ReadOnly Property WorkbookEpoch As Long?
        Public ReadOnly Property WorkbookEvidence As UdfHandleScopeEvidence
        Public ReadOnly Property ResultTypeVersion As String

        Public Function WithState(nextState As UdfHandleState) As UdfModelHandleDescriptor
            Return New UdfModelHandleDescriptor(ConvertedKey, Family, nextState, SessionEpoch, SessionEvidence,
                                                WorkbookEpoch, WorkbookEvidence, ResultTypeVersion)
        End Function
    End Class

    ''' <summary>Pure decision only. The host must perform any storage operation at its existing point.
    ''' Scope-driven recommendations are not registered events or an activated invalidation policy.</summary>
    Public NotInheritable Class UdfHandleLifecycleDecision
        Friend Sub New(action As UdfHandleAction, nextState As UdfHandleState)
            Me.Action = action : Me.NextState = nextState
        End Sub
        Public ReadOnly Property Action As UdfHandleAction
        Public ReadOnly Property NextState As UdfHandleState
        Public ReadOnly Property PerformsStorage As Boolean
            Get
                Return False
            End Get
        End Property
    End Class

    Public NotInheritable Class UdfModelHandlePolicy
        Private Sub New()
        End Sub

        ''' <summary>Receives text after host-specific conversion. No grammar/family/session restriction
        ''' is added to legacy dictionary lookup. In particular arbitrary nonblank stored keys remain usable.</summary>
        Public Shared Function HasLookupKey(convertedKey As String) As Boolean
            Return Not String.IsNullOrWhiteSpace(convertedKey)
        End Function

        ''' <summary>The caller retains conversion, Count-then-Clear, removal, catches and return mapping.</summary>
        Public Shared Function IsClearAllRequest(convertedKey As String, allToken As String) As Boolean
            Return String.IsNullOrWhiteSpace(convertedKey) OrElse String.Equals(convertedKey, allToken, StringComparison.OrdinalIgnoreCase)
        End Function

        Public Shared Function IdentifyFamily(convertedKey As String) As UdfHandleFamily
            If convertedKey Is Nothing Then Return UdfHandleFamily.Unknown
            Dim separator = convertedKey.IndexOf(":"c)
            If separator <= 0 Then Return UdfHandleFamily.Unknown
            Select Case convertedKey.Substring(0, separator).ToUpperInvariant()
                Case "LM" : Return UdfHandleFamily.LM
                Case "GLM" : Return UdfHandleFamily.GLM
                Case "GLMNB" : Return UdfHandleFamily.GLMNB
                Case "ZIP" : Return UdfHandleFamily.ZIP
                Case "GEE" : Return UdfHandleFamily.GEE
                Case "COX" : Return UdfHandleFamily.COX
                Case "ORDLOGIT" : Return UdfHandleFamily.ORDLOGIT
                Case "MNLOGIT" : Return UdfHandleFamily.MNLOGIT
                Case "LMM" : Return UdfHandleFamily.LMM
                Case "MMRM" : Return UdfHandleFamily.MMRM
                Case "PCA" : Return UdfHandleFamily.PCA
                Case "FA" : Return UdfHandleFamily.FA
                Case "KMEANS" : Return UdfHandleFamily.KMEANS
                Case "HCLUST" : Return UdfHandleFamily.HCLUST
                Case "CA" : Return UdfHandleFamily.CA
                Case "MCA" : Return UdfHandleFamily.MCA
                Case "DA" : Return UdfHandleFamily.DA
                Case "PSM" : Return UdfHandleFamily.PSM
                Case Else : Return UdfHandleFamily.Unknown
            End Select
        End Function

        ''' <summary>Describes the generated shape, never authorizes or rejects host lookup.</summary>
        Public Shared Function InspectSyntax(convertedKey As String) As UdfHandleSyntax
            If Not HasLookupKey(convertedKey) Then Return UdfHandleSyntax.Blank
            If IdentifyFamily(convertedKey) = UdfHandleFamily.Unknown Then Return UdfHandleSyntax.UnknownFamily
            Dim separator = convertedKey.IndexOf(":"c), parsed As Guid
            Dim suffix = convertedKey.Substring(separator + 1)
            If suffix.Length = 32 AndAlso Guid.TryParseExact(suffix, "N", parsed) Then Return UdfHandleSyntax.GeneratedShape
            Return UdfHandleSyntax.Malformed
        End Function

        Public Shared Function Validity(descriptor As UdfModelHandleDescriptor, expectedFamily As UdfHandleFamily,
                                        Optional found As Boolean? = Nothing,
                                        Optional currentSessionEpoch As Long? = Nothing,
                                        Optional currentWorkbookEpoch As Long? = Nothing) As UdfHandleValidity
            If descriptor Is Nothing OrElse Not HasLookupKey(descriptor.ConvertedKey) Then Return UdfHandleValidity.Blank
            If descriptor.State = UdfHandleState.Removed OrElse descriptor.State = UdfHandleState.FamilyCleared OrElse
                descriptor.State = UdfHandleState.SessionEnded OrElse descriptor.State = UdfHandleState.WorkbookClosed Then Return UdfHandleValidity.ExplicitlyInvalidated
            If descriptor.SessionEvidence = UdfHandleScopeEvidence.VerifiedByHost AndAlso descriptor.SessionEpoch.HasValue AndAlso currentSessionEpoch.HasValue AndAlso descriptor.SessionEpoch.Value <> currentSessionEpoch.Value Then Return UdfHandleValidity.WrongVerifiedSession
            If descriptor.WorkbookEvidence = UdfHandleScopeEvidence.VerifiedByHost AndAlso descriptor.WorkbookEpoch.HasValue AndAlso currentWorkbookEpoch.HasValue AndAlso descriptor.WorkbookEpoch.Value <> currentWorkbookEpoch.Value Then Return UdfHandleValidity.WrongVerifiedWorkbook
            If expectedFamily <> UdfHandleFamily.Unknown AndAlso descriptor.Family <> UdfHandleFamily.Unknown AndAlso descriptor.Family <> expectedFamily Then Return UdfHandleValidity.FamilyMismatchReportOnly
            If Not found.HasValue Then Return UdfHandleValidity.HostLookupRequired
            Return If(found.Value, UdfHandleValidity.Available, UdfHandleValidity.NotFound)
        End Function

        Public Shared Function Plan(descriptor As UdfModelHandleDescriptor, signal As UdfHandleEvent,
                                    Optional signalEpoch As Long? = Nothing,
                                    Optional verifiedSignal As Boolean = False) As UdfHandleLifecycleDecision
            If descriptor Is Nothing Then Throw New ArgumentNullException(NameOf(descriptor))
            Select Case signal
                Case UdfHandleEvent.ExplicitDrop
                    Return New UdfHandleLifecycleDecision(UdfHandleAction.RemoveKey, UdfHandleState.Removed)
                Case UdfHandleEvent.FamilyClear
                    Return New UdfHandleLifecycleDecision(UdfHandleAction.ClearFamily, UdfHandleState.FamilyCleared)
                Case UdfHandleEvent.RepeatedFit
                    Return New UdfHandleLifecycleDecision(UdfHandleAction.CreateIndependentHandle, descriptor.State)
                Case UdfHandleEvent.WorkbookSwitch
                    Return New UdfHandleLifecycleDecision(UdfHandleAction.None, descriptor.State)
                Case UdfHandleEvent.SessionEnd
                    If verifiedSignal AndAlso signalEpoch.HasValue AndAlso descriptor.SessionEvidence = UdfHandleScopeEvidence.VerifiedByHost AndAlso descriptor.SessionEpoch.HasValue AndAlso signalEpoch.Value = descriptor.SessionEpoch.Value Then Return New UdfHandleLifecycleDecision(UdfHandleAction.RecommendInvalidation, UdfHandleState.SessionEnded)
                Case UdfHandleEvent.WorkbookClose
                    If verifiedSignal AndAlso signalEpoch.HasValue AndAlso descriptor.WorkbookEvidence = UdfHandleScopeEvidence.VerifiedByHost AndAlso descriptor.WorkbookEpoch.HasValue AndAlso signalEpoch.Value = descriptor.WorkbookEpoch.Value Then Return New UdfHandleLifecycleDecision(UdfHandleAction.RecommendInvalidation, UdfHandleState.WorkbookClosed)
                Case UdfHandleEvent.WorkbookReopen
                    If verifiedSignal AndAlso signalEpoch.HasValue AndAlso descriptor.WorkbookEvidence = UdfHandleScopeEvidence.VerifiedByHost AndAlso descriptor.WorkbookEpoch.HasValue AndAlso signalEpoch.Value <> descriptor.WorkbookEpoch.Value Then Return New UdfHandleLifecycleDecision(UdfHandleAction.RecommendInvalidation, UdfHandleState.WorkbookClosed)
                Case Else
                    Throw New ArgumentOutOfRangeException(NameOf(signal))
            End Select
            Return New UdfHandleLifecycleDecision(UdfHandleAction.NativeScopeNotVerified, descriptor.State)
        End Function

        ''' <summary>Relational reference validation for host-provided observations;not a model store.</summary>
        Public Shared Function ReferenceMatches(descriptor As UdfModelHandleDescriptor, requestedConvertedKey As String,
                                                observedFamily As UdfHandleFamily, sameCapturedObject As Boolean) As Boolean
            Return descriptor IsNot Nothing AndAlso HasLookupKey(requestedConvertedKey) AndAlso sameCapturedObject AndAlso
                descriptor.Family = observedFamily AndAlso String.Equals(descriptor.ConvertedKey, requestedConvertedKey, StringComparison.OrdinalIgnoreCase)
        End Function
    End Class
End Namespace
