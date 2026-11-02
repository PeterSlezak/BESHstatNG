Option Explicit On
Option Strict On

Imports System.Collections.Generic
Imports BESHStatNG.ExcelCommon

''' <summary>Pure conversion seam used by UibyID after Windows-owned range import.
''' It has no Excel state, controls, acquisition, dialog or write dependency.
''' </summary>
Public NotInheritable Class WindowsUnivariateInputAdapter
    Private Shared ReadOnly originalSelections As New System.Runtime.CompilerServices.ConditionalWeakTable(Of MultiGroupsUnpairedData, AssumptionReportInput)
    Private Sub New()
    End Sub

    Public Shared Function SelectColumns(columns As IReadOnlyList(Of DataObj), ByRef errorText As String) As MultiGroupsUnpairedData
        Return ToWindowsData(UnivariateSelectionPolicy.SelectImportedColumns(columns), errorText)
    End Function

    Public Shared Function CaptureColumn(imported As DataObj) As UnivariateImportedColumn
        Return UnivariateSelectionPolicy.CaptureImportedColumn(imported)
    End Function

    Public Shared Function SelectCapturedColumns(columns As IReadOnlyList(Of UnivariateImportedColumn), ByRef errorText As String) As MultiGroupsUnpairedData
        Return ToWindowsData(UnivariateSelectionPolicy.SelectCapturedColumns(columns), errorText)
    End Function

    Public Shared Function SelectById(imported As DataObj, ByRef errorText As String) As MultiGroupsUnpairedData
        Return ToWindowsData(UnivariateSelectionPolicy.SelectImportedById(imported), errorText)
    End Function

    Private Shared Function ToWindowsData(result As UnivariateSelectionResult, ByRef errorText As String) As MultiGroupsUnpairedData
        If Not result.Success Then
            errorText = result.Validation.Issues(0).Message
            Return Nothing
        End If
        ' Do not clear a pre-existing worksheet diagnostic from UibyID.
        Dim legacy = New MultiGroupsUnpairedData With {.X = result.Data.Groups, .varNames = result.Data.GroupNames}
        originalSelections.Add(legacy, New AssumptionReportInput(result.Data.Groups, result.Data.GroupNames, result.Sources))
        Return legacy
    End Function

    ''' <summary>Immutable original selection, independent of later Core scratch sorting.
    ''' Does not claim these row IDs still describe a subsequently modified legacy vector.</summary>
    Public Shared Function GetOriginalSelection(data As MultiGroupsUnpairedData) As AssumptionReportInput
        Dim result As AssumptionReportInput = Nothing
        If data IsNot Nothing Then originalSelections.TryGetValue(data, result)
        Return result
    End Function
End Class
