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
