Option Explicit On
Option Strict On

Imports System
Imports System.Collections.Generic
Imports System.Linq

''' <summary>
''' Plot-data import helpers for worksheet UDFs.
''' Kept in a separate partial module so plot UDFs call the shared UdfDataImport facade instead of owning range parsing.
''' </summary>
Partial Friend Module UdfDataImport
    Private Function IsStatusBinaryCell38(value As Object) As Boolean
        Dim result As Integer
        Return Global.BESHStatNG.WorksheetFunctions.ExcelArgNumeric.TryGetStatus01Flexible(value, result)
    End Function


    ''' <summary>
    ''' Imports aligned marker/status inputs and builds the positive/negative sample arrays required by the ROC engine.
    ''' </summary>
    Friend Function TryGetRocInput(marker As Object,
                                   status As Object,
                                   positiveClass As Object,
                                   direction As Object,
                                   ByRef rocInput()() As Double,
                                   ByRef markerName As String,
                                   ByRef positiveLabel As String,
                                   ByRef isLowerDirection As Boolean) As Boolean
        rocInput = Nothing
        markerName = Nothing
        positiveLabel = Nothing
        isLowerDirection = ParseRocDirection(direction)

        Dim markerCol(,) As Object = Nothing
        Dim statusCol(,) As Object = Nothing
        Dim inferredMarker As String = Nothing
        Dim inferredStatus As String = Nothing

        If Not TryGetTrimmedColumnObject(marker, markerCol, inferredMarker, "numeric") Then Return False
        If Not TryGetTrimmedColumnObject(status, statusCol, inferredStatus, "binary") Then
            If Not TryGetTrimmedColumnObject(status, statusCol, inferredStatus, "text") Then Return False
        End If

        If markerCol.GetLength(0) <> statusCol.GetLength(0) Then
            If statusCol.GetLength(0) = markerCol.GetLength(0) + 1 Then
                Dim trimmed(statusCol.GetLength(0) - 2, 0) As Object
                For i As Integer = 1 To statusCol.GetLength(0) - 1
                    trimmed(i - 1, 0) = statusCol(i, 0)
                Next
                statusCol = trimmed
            End If
        End If

        If markerCol.GetLength(0) <> statusCol.GetLength(0) Then Return False

        markerName = If(String.IsNullOrWhiteSpace(inferredMarker), "Marker", inferredMarker)
        If Not GuessPositiveClass(statusCol, positiveClass, positiveLabel) Then Return False

        Return Global.BESHStatNG.ExcelCommon.UdfPlotDataPreparation38.CompleteRocInputPrepared(markerCol, statusCol, positiveLabel, isLowerDirection,
            AddressOf Global.BESHStatNG.WorksheetFunctions.ExcelArgNumeric.TryGetFiniteDoubleFlexible,
            AddressOf Global.BESHStatNG.WorksheetFunctions.ExcelArgNumeric.TryGetStatus01Flexible,
            AddressOf Global.BESHStatNG.WorksheetFunctions.ExcelArgReaders.CellToTrimmedText, AddressOf NormalizeOptionKey, rocInput)
    End Function

    ''' <summary>
    ''' Imports aligned time/status/group columns and builds survival records for Kaplan-Meier plotting.
    ''' </summary>
    Friend Function TryGetSurvivalRecords(time As Object,
                                          status As Object,
                                          group As Object,
                                          ByRef records As List(Of survival.SurvivalRecord)) As Boolean
        records = Nothing
        Dim timeCol(,) As Object = Nothing
        Dim statusCol(,) As Object = Nothing
        Dim groupCol(,) As Object = Nothing
        Dim timeName As String = Nothing
        Dim statusName As String = Nothing
        Dim groupName As String = Nothing

        If Not TryGetTrimmedColumnObject(time, timeCol, timeName, "numeric") Then Return False
        If Not TryGetTrimmedColumnObject(status, statusCol, statusName, "binary") Then Return False
        If timeCol.GetLength(0) <> statusCol.GetLength(0) Then Return False

        Dim hasGroup As Boolean = Not Global.BESHStatNG.WorksheetFunctions.ExcelArgPredicates.IsMissingArg(group)
        If hasGroup Then
            If Not TryGetTrimmedColumnObject(group, groupCol, groupName, "text") Then Return False
            If groupCol.GetLength(0) <> timeCol.GetLength(0) Then Return False
        End If

        Return Global.BESHStatNG.ExcelCommon.UdfPlotDataPreparation38.CompleteSurvivalRecordsPrepared(timeCol, statusCol, groupCol, hasGroup,
            AddressOf Global.BESHStatNG.WorksheetFunctions.ExcelArgNumeric.TryGetFiniteDoubleFlexible,
            AddressOf Global.BESHStatNG.WorksheetFunctions.ExcelArgNumeric.TryGetStatus01Flexible,
            AddressOf Global.BESHStatNG.WorksheetFunctions.ExcelArgReaders.CellToTrimmedText, records)
    End Function

    Private Function ParseRocDirection(direction As Object) As Boolean
        Return Global.BESHStatNG.ExcelCommon.UdfDataImportOptions38.ParseRocDirection(Global.BESHStatNG.WorksheetFunctions.ExcelArgReaders.AsString(direction))
    End Function

    Private Function GuessPositiveClass(statusCol(,) As Object,
                                        explicitPositive As Object,
                                        ByRef positiveLabel As String) As Boolean
        positiveLabel = Nothing
        Dim explicitText As String = Global.BESHStatNG.WorksheetFunctions.ExcelArgReaders.CellToTrimmedText(explicitPositive)
        Return Global.BESHStatNG.ExcelCommon.UdfClassLabelPreparation38.GuessPositiveClassAfterConversion(
            explicitText, Function() statusCol.GetLength(0),
            Function(row) Global.BESHStatNG.WorksheetFunctions.ExcelArgReaders.CellToTrimmedText(statusCol(row, 0)),
            Function(row) IsStatusBinaryCell38(statusCol(row, 0)), positiveLabel)
    End Function

End Module