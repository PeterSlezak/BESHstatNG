Option Explicit On
Option Strict On

Imports System
Imports System.Collections.Generic
Imports BESHStatNG.ExcelCommon
Imports Microsoft.Office.Interop.Excel

''' <summary>Read-only Windows observations and a pure pre-creation validation seam.</summary>
Friend NotInheritable Class ExcelDnaOutputTargetAdapter
    Private Sub New()
    End Sub

    Friend Shared Function Inspect(workbook As Workbook, worksheet As Worksheet,
                                   destination As SpreadsheetOutputDestination,
                                   Optional bounds As OutputRectangle = Nothing) As OutputTargetFacts
        If destination = SpreadsheetOutputDestination.NewWorkbook AndAlso worksheet Is Nothing Then
            Return New OutputTargetFacts() ' Workbooks.Add does not depend on the active workbook.
        End If
        If workbook Is Nothing Then Return New OutputTargetFacts(workbookAvailable:=False)
        If destination = SpreadsheetOutputDestination.NewWorksheet AndAlso worksheet Is Nothing Then
            Return New OutputTargetFacts(workbookAvailable:=True, writable:=Not workbook.ReadOnly,
                                         canCreateWorksheet:=Not workbook.ProtectStructure)
        End If
        If worksheet Is Nothing Then Return New OutputTargetFacts(workbookAvailable:=True, sheetExists:=False)
        Dim merged As Boolean? = Nothing
        If bounds IsNot Nothing Then
            Dim region As Range = worksheet.Range(worksheet.Cells(bounds.StartRow, bounds.StartColumn),
                                                  worksheet.Cells(bounds.EndRow, bounds.EndColumn))
            Dim value As Object = region.MergeCells
            ' Mixed merged/unmerged ranges return Null; any mixed range is also a conflict.
            merged = If(TypeOf value Is Boolean, CBool(value), True)
        End If
        Return New OutputTargetFacts(workbookAvailable:=True, sheetExists:=True, identityUnambiguous:=True,
            writable:=Not workbook.ReadOnly, protectedSheet:=worksheet.ProtectContents,
            mergedConflict:=merged, resolvedWorksheetName:=worksheet.Name,
            maxRows:=worksheet.Rows.Count, maxColumns:=worksheet.Columns.Count)
    End Function

    Friend Shared Function Preflight(target As SpreadsheetOutputTarget, facts As OutputTargetFacts,
                                     document As OutputDocument,
                                     supplementary As IEnumerable(Of ResultTableOutputModel),
                                     Optional host As HostCapabilitySnapshot = Nothing) As OutputRectangle
        Dim decision = TargetPlanner.Create(target, If(host, ExcelDnaHostCapabilities.GetSnapshot()), facts)
        decision.EnsureSupported()
        Dim limits As New OutputLayoutOptions(maxRows:=decision.Facts.MaxRows, maxColumns:=decision.Facts.MaxColumns)
        Dim layout = OutputLayoutPlanner.Create(document, decision.ToTarget(), limits)
        Return ExcelDnaSharedOutputPlanAdapter.PreflightBounds(layout, supplementary)
    End Function
End Class
