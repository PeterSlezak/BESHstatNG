Option Explicit On
Option Strict On

Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports ExcelDna.Integration

' Survival-analysis data import helpers for worksheet UDFs.
' Keeps survival UDF modules focused on analysis/output while this facade owns range parsing and row alignment.
Partial Friend Module UdfDataImport

    ''' <summary>
    ''' Imports aligned time/status/group inputs and builds records for Kaplan-Meier table output.
    ''' Invalid numeric/status rows are skipped; negative times are skipped to preserve the existing KM_TABLE behavior.
    ''' </summary>
    Friend Function TryGetKaplanMeierRecords(time As Object,
                                             status As Object,
                                             group As Object,
                                             ByRef records As List(Of survival.SurvivalRecord)) As Boolean
        records = Nothing

        Dim tArr As Object(,) = Nothing
        Dim sArr As Object(,) = Nothing
        If Not TryGetSurvival2D(time, tArr) Then Return False
        If Not TryGetSurvival2D(status, sArr) Then Return False
        If tArr.GetLength(1) <> 1 OrElse sArr.GetLength(1) <> 1 Then Return False
        If tArr.GetLength(0) <> sArr.GetLength(0) Then Return False

        Dim hasGroup As Boolean = Not Global.BESHStatNG.WorksheetFunctions.ExcelArgPredicates.IsMissingArg(group)
        Dim gArr As Object(,) = Nothing
        If hasGroup Then
            If Not TryGetSurvival2D(group, gArr) Then Return False
            If gArr.GetLength(1) <> 1 Then Return False
            If gArr.GetLength(0) <> tArr.GetLength(0) Then Return False
        End If

        Return Global.BESHStatNG.ExcelCommon.UdfW2CompletionPreparation38.PrepareKaplanMeierRecords(tArr, sArr, gArr, hasGroup, records, AddressOf Global.BESHStatNG.WorksheetFunctions.ExcelArgNumeric.TryGetFiniteDoubleFlexible, AddressOf Global.BESHStatNG.WorksheetFunctions.ExcelArgNumeric.TryGetStatus01Flexible, AddressOf Global.BESHStatNG.WorksheetFunctions.ExcelArgReaders.CellToTrimmedText)
    End Function

    ''' <summary>
    ''' Imports survival median-CI inputs, builds survival records, runs the engine, and returns the Excel-ready table.
    ''' </summary>
    Friend Function TryComputeSurvivalMedianCi(timeRange As Object,
                                               statusRange As Object,
                                               groupRange As Object,
                                               alpha As Double,
                                               ByRef outTable As Object(,)) As Boolean
        outTable = Nothing

        Dim timeArr As Object(,) = Nothing
        Dim statusArr As Object(,) = Nothing
        Dim groupArr As Object(,) = Nothing

        If Not TryGetSurvival2D(timeRange, timeArr) Then Return False
        If Not TryGetSurvival2D(statusRange, statusArr) Then Return False

        If timeArr.GetLength(1) <> 1 OrElse statusArr.GetLength(1) <> 1 Then Return False

        Dim nRows As Integer = timeArr.GetLength(0)
        If statusArr.GetLength(0) <> nRows Then Return False

        Dim hasGroup As Boolean = Not Global.BESHStatNG.WorksheetFunctions.ExcelArgPredicates.IsMissingArg(groupRange)
        If hasGroup Then
            If Not TryGetSurvival2D(groupRange, groupArr) Then Return False
            If groupArr.GetLength(1) <> 1 OrElse groupArr.GetLength(0) <> nRows Then Return False
        End If

        Return Global.BESHStatNG.ExcelCommon.UdfW2CompletionPreparation38.ComputeSurvivalMedianCi(timeArr, statusArr, groupArr, hasGroup, alpha, outTable, ExcelError.ExcelErrorNA, AddressOf Global.BESHStatNG.WorksheetFunctions.ExcelArgNumeric.TryGetFiniteDoubleFlexible, AddressOf Global.BESHStatNG.WorksheetFunctions.ExcelArgNumeric.TryGetStatus01Flexible, AddressOf Global.BESHStatNG.WorksheetFunctions.ExcelArgReaders.CellToTrimmedText)
    End Function

    ''' <summary>
    ''' Imports aligned log-rank inputs, builds survival records, and runs the selected weighted log-rank test.
    ''' </summary>
    Friend Function TryComputeSurvivalLogRank(timeRange As Object,
                                              statusRange As Object,
                                              groupRange As Object,
                                              strataRange As Object,
                                              weight As Object,
                                              ByRef result As TestResult,
                                              ByRef errText As String) As Boolean
        errText = Nothing
        result = Nothing

        Dim timeArr As Object(,) = Nothing
        Dim statusArr As Object(,) = Nothing
        Dim groupArr As Object(,) = Nothing
        Dim strataArr As Object(,) = Nothing

        If Not TryGetSurvival2D(timeRange, timeArr) Then Return False
        If Not TryGetSurvival2D(statusRange, statusArr) Then Return False
        If Not TryGetSurvival2D(groupRange, groupArr) Then Return False

        If timeArr.GetLength(1) <> 1 OrElse statusArr.GetLength(1) <> 1 OrElse groupArr.GetLength(1) <> 1 Then Return False

        Dim nRows As Integer = timeArr.GetLength(0)
        If statusArr.GetLength(0) <> nRows OrElse groupArr.GetLength(0) <> nRows Then Return False

        Dim hasStrata As Boolean = Not Global.BESHStatNG.WorksheetFunctions.ExcelArgPredicates.IsMissingArg(strataRange)
        If hasStrata Then
            If Not TryGetSurvival2D(strataRange, strataArr) Then Return False
            If strataArr.GetLength(1) <> 1 OrElse strataArr.GetLength(0) <> nRows Then Return False
        End If

        Dim weightMethod As String = ParseSurvivalWeightMethod(weight)
        If weightMethod Is Nothing Then Return False

        Return Global.BESHStatNG.ExcelCommon.UdfW2CompletionPreparation38.ComputeSurvivalLogRank(timeArr, statusArr, groupArr, strataArr, hasStrata, weightMethod, result, errText, AddressOf Global.BESHStatNG.WorksheetFunctions.ExcelArgNumeric.TryGetFiniteDoubleFlexible, AddressOf Global.BESHStatNG.WorksheetFunctions.ExcelArgNumeric.TryGetStatus01Flexible, AddressOf Global.BESHStatNG.WorksheetFunctions.ExcelArgReaders.CellToTrimmedText)
    End Function

    Private Function ParseSurvivalWeightMethod(weight As Object) As String
        Dim w As String = "logrank"
        If Global.BESHStatNG.WorksheetFunctions.ExcelArgPredicates.IsMissingArg(weight) Then Return w

        Dim s As String = Global.BESHStatNG.WorksheetFunctions.ExcelArgReaders.CellToTrimmedText(weight)
        If String.IsNullOrWhiteSpace(s) Then Return w

        Return Global.BESHStatNG.ExcelCommon.UdfW2CompletionPreparation38.ParseSurvivalWeightText(s)
    End Function

    Private Function TryGetSurvival2D(input As Object, ByRef arr As Object(,)) As Boolean
        arr = Get2DOrScalar(input)
        Return arr IsNot Nothing
    End Function

End Module