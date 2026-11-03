Option Explicit On
Option Strict On
Imports System
Imports BESHStatNG.ExcelCommon
Imports BESHStatNG.regression

''' <summary>Post-acquisition bridge; no form construction, fitting, callbacks or writer.
''' Original Core blocks are immutable snapshots and are reused without copying X/Z again.</summary>
Public NotInheritable Class WindowsMixedModelWorkflowAdapter
    Private Sub New()
    End Sub
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
