Option Explicit On
Option Strict On
Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports BESHStatNG.ExcelCommon

''' <summary>Post-import, host-free shipping bridge. Acquisition and all native operations
''' remain in the forms; original physical-area transport evidence is retained intact.</summary>
Public NotInheritable Class WindowsSurvivalWorkflowAdapter
    Private Sub New()
    End Sub
    Friend Shared Function Provenance(data As DataObj) As SpreadsheetTransportFrame
        Dim evidence = ExcelDnaDataImporter.GetPhysicalImportEvidence(data.CoreTable)
        Return If(evidence Is Nothing, Nothing, evidence.Data)
    End Function
    Public Shared Function Capture(data As DataObj, timeColumn As Integer, groups() As String, strata() As String,
                                   hasGroup As Boolean, hasStrata As Boolean, mode As SurvivalAnalysisMode,
                                   options As SurvivalOptions) As SurvivalWorkflowIntent
        Dim ids = data.RowIds, values = data.FinalData
        ' Preserve the raw numeric status for diagnostic observation, before the historical
        ' KM Array2intArray rounding. The already converted Core records execute unchanged.
        Dim input As New SurvivalInput(New RegressionVector("time", Matrix.Array2dblArray(Matrix.GetColumnFrom2Darray(values, timeColumn)), ids),
            New RegressionVector("status", Matrix.Array2dblArray(Matrix.GetColumnFrom2Darray(values, timeColumn + 1)), ids),
            If(hasGroup, New SurvivalLabels("group", groups, ids), Nothing),
            If(hasStrata, New SurvivalLabels("stratum", strata, ids), Nothing), Provenance(data),
            New SurvivalRoleColumns(timeColumn, timeColumn + 1, If(hasGroup, 0, -1), If(hasStrata, If(hasGroup, 1, 0), -1)))
        Return New SurvivalWorkflowIntent(mode, input, options)
    End Function
End Class

Public NotInheritable Class WindowsCoxWorkflowAdapter
    Private Sub New()
    End Sub
    Public Shared Function Input(data As CoxPHData) As SurvivalInput
        Return New SurvivalInput(New RegressionVector(data.TimeVarName, data.TimeData, data.RowIds),
            New RegressionVector(data.CensorVarName, data.CensorData.Select(Function(s) CDbl(s)).ToArray(), data.RowIds),
            strata:=If(data.bStrata, New SurvivalLabels(data.StrataVarName, data.StrataData, data.RowIds), Nothing),
            provenance:=WindowsSurvivalWorkflowAdapter.Provenance(data),
            roleColumns:=New SurvivalRoleColumns(0, 1, stratum:=If(data.bStrata, 2, -1)))
    End Function
    Public Shared Function Capture(data As CoxPHData, expanded(,) As Double, names() As String,
                                   effects As RegressionEffectModel, rawKeys() As String, options As CoxOptions,
                                   Optional initial() As Double = Nothing) As CoxWorkflowIntent
        Return New CoxWorkflowIntent(Input(data), New RegressionComponent(expanded, names, data.RowIds, effects, rawKeys, initial), options)
    End Function
End Class
