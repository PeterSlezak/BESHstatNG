Option Explicit On
Option Strict On

Imports System.Collections.Generic
Imports BESHStatNG.ExcelCommon

''' <summary>Pure conversion seam used by UibyID after Windows-owned range import.
''' It has no Excel state, controls, acquisition, dialog or write dependency.
''' </summary>
Public NotInheritable Class WindowsUnivariateInputAdapter
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
        Return New MultiGroupsUnpairedData With {.X = result.Data.Groups, .varNames = result.Data.GroupNames}
    End Function
End Class
