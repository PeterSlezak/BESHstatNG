Option Explicit On
Option Strict On
Imports System
Imports System.Collections.Generic
Imports System.Runtime.CompilerServices
Imports BESHStatNG.ExcelCommon
Imports BESHStatNG.regression

''' <summary>Post-acquisition bridge; no form construction, fitting, callbacks or writer.
''' Original Core blocks are immutable snapshots and are reused without copying X/Z again.</summary>
Public NotInheritable Class WindowsMixedModelWorkflowAdapter
    Private Sub New()
    End Sub
    Private NotInheritable Class AttachedTables
        Public Tables As New List(Of ResultTable)
    End Class
    Private Shared ReadOnly SupplementalTables As New ConditionalWeakTable(Of MixedModelResult, AttachedTables)
    ' Preserve the existing Core additional-table list and its original insertion point.
    ' Track only table identities, with a weak fit key; never retain X/Z or a KR workspace.
    Public Shared Sub ApplySupplemental(result As MixedModelResult, report As MixedModelSupplementalReport)
        If result Is Nothing OrElse report Is Nothing Then Return
        SyncLock result
            Dim attached = SupplementalTables.GetValue(result, Function(key) New AttachedTables())
            If result.AdditionalResultTables Is Nothing Then result.AdditionalResultTables = New List(Of ResultTable)
            Dim position = result.AdditionalResultTables.Count
            For Each table In attached.Tables
                Dim index = result.AdditionalResultTables.IndexOf(table)
                If index >= 0 Then
                    position = Math.Min(position, index)
                    result.AdditionalResultTables.RemoveAt(index)
                End If
            Next
            attached.Tables = report.LegacyTables()
            result.AdditionalResultTables.InsertRange(position, attached.Tables)
        End SyncLock
    End Sub
    Public Shared Sub LogPostEstimation(notice As MixedModelPostEstimationNotice)
        If notice.IsWarning Then
            AppInfrastructure.CoreServices.Logger.Warn(notice.Message)
        Else
            AppInfrastructure.CoreServices.Logger.Debug(notice.Message)
        End If
    End Sub
    ' Called at the original form factory sites, before the KR information message.
    ' Preserve Core factory side effects and exceptions; never activate strict validation here.
    Public Shared Function CreateResidualStructure(selectedToken As String) As MixedModelRStruct
        Return MixedModelFittingOptions.CreateResidualStructure(selectedToken)
    End Function
    Public Shared Function CreateRandomStructure(selectedToken As String) As MixedModelGStruct
        Return MixedModelFittingOptions.CreateRandomStructure(selectedToken)
    End Function
    Private Shared Function Snapshot(raw As DataObj, blocks As MixedModelBlockData, subjects() As Object) As MixedModelWorkflowData
        Dim evidence = ExcelDnaDataImporter.GetPhysicalImportEvidence(raw.CoreTable)
        Return MixedModelWorkflowData.FromAcquiredBlocks(blocks, raw.RowIds, subjects, If(evidence Is Nothing, Nothing, evidence.Data))
    End Function
    Public Shared Function CaptureMMRM(raw As DataObj, blocks As MixedModelBlockData, subjects() As Object,
                                       response As String, subject As String, visit As String, fixedNames() As String,
                                       effects As RegressionEffectModel, fixedText As String, options As MixedModelWorkflowOptions) As MixedModelWorkflowRequest
        Return New MixedModelWorkflowRequest(MixedModelWorkflowKind.MMRM, Snapshot(raw, blocks, subjects),
            RegressionDesignCore.GetCoefBaseName(response), RegressionDesignCore.GetCoefBaseName(subject), RegressionDesignCore.GetCoefBaseName(visit),
            fixedNames, Nothing, effects, Nothing, fixedText, String.Empty, options, response, subject, visit)
    End Function
    Public Shared Function CaptureLMM(raw As DataObj, blocks As MixedModelBlockData, subjects() As Object,
                                      response As String, subject As String, visit As String, fixedNames() As String, randomNames() As String,
                                      fixedEffects As RegressionEffectModel, randomEffects As RegressionEffectModel,
                                      fixedText As String, randomText As String, options As MixedModelWorkflowOptions) As MixedModelWorkflowRequest
        Return New MixedModelWorkflowRequest(MixedModelWorkflowKind.LMM, Snapshot(raw, blocks, subjects),
            RegressionDesignCore.GetCoefBaseName(response), RegressionDesignCore.GetCoefBaseName(subject),
            If(String.IsNullOrWhiteSpace(visit), String.Empty, RegressionDesignCore.GetCoefBaseName(visit)),
            fixedNames, randomNames, fixedEffects, randomEffects, fixedText, randomText, options, response, subject, visit)
    End Function
End Class
