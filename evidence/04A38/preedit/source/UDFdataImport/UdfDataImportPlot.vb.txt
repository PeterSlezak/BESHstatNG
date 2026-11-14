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

        Dim pos As New List(Of Double)()
        Dim neg As New List(Of Double)()
        Dim normalizedPositive As String = NormalizeOptionKey(positiveLabel)

        For i As Integer = 0 To markerCol.GetLength(0) - 1
            Dim x As Double
            If Not Global.BESHStatNG.WorksheetFunctions.ExcelArgNumeric.TryGetFiniteDoubleFlexible(markerCol(i, 0), x) Then Continue For

            Dim label As String = Global.BESHStatNG.WorksheetFunctions.ExcelArgReaders.CellToTrimmedText(statusCol(i, 0))
            If String.IsNullOrWhiteSpace(label) Then Continue For

            Dim isPositive As Boolean = False
            Dim iv As Integer
            If normalizedPositive = "1" AndAlso Global.BESHStatNG.WorksheetFunctions.ExcelArgNumeric.TryGetStatus01Flexible(statusCol(i, 0), iv) Then
                isPositive = (iv = 1)
            Else
                isPositive = String.Equals(NormalizeOptionKey(label), normalizedPositive, StringComparison.OrdinalIgnoreCase)
            End If

            If isLowerDirection Then x = -x
            If isPositive Then
                pos.Add(x)
            Else
                neg.Add(x)
            End If
        Next

        If pos.Count < 1 OrElse neg.Count < 1 Then Return False
        rocInput = New Double()() {pos.ToArray(), neg.ToArray()}
        Return True
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

        Dim tList As New List(Of Double)()
        Dim sList As New List(Of Integer)()
        Dim gList As New List(Of String)()
        Dim stratList As New List(Of String)()

        For i As Integer = 0 To timeCol.GetLength(0) - 1
            Dim t As Double
            Dim s As Integer
            If Not Global.BESHStatNG.WorksheetFunctions.ExcelArgNumeric.TryGetFiniteDoubleFlexible(timeCol(i, 0), t) Then Continue For
            If t < 0.0R Then Return False
            If Not Global.BESHStatNG.WorksheetFunctions.ExcelArgNumeric.TryGetStatus01Flexible(statusCol(i, 0), s) Then Continue For

            Dim g As String = "ALL"
            If hasGroup Then
                g = Global.BESHStatNG.WorksheetFunctions.ExcelArgReaders.CellToTrimmedText(groupCol(i, 0))
                If String.IsNullOrWhiteSpace(g) Then Continue For
            End If

            tList.Add(t)
            sList.Add(s)
            gList.Add(g)
            stratList.Add("ALL")
        Next

        If tList.Count < 1 Then Return False

        Dim err As String = Nothing
        records = survival.Survival.CreatSurvivalData(tList.ToArray(), sList.ToArray(), gList.ToArray(), stratList.ToArray(), err)
        Return records IsNot Nothing AndAlso records.Count > 0
    End Function

    Private Function ParseRocDirection(direction As Object) As Boolean
        Dim token As String = NormalizeOptionKey(Global.BESHStatNG.WorksheetFunctions.ExcelArgReaders.AsString(direction))
        If String.IsNullOrWhiteSpace(token) Then Return False
        Return (token = "lower" OrElse token = "low" OrElse token = "smaller" OrElse token = "decreasing")
    End Function

    Private Function GuessPositiveClass(statusCol(,) As Object,
                                        explicitPositive As Object,
                                        ByRef positiveLabel As String) As Boolean
        positiveLabel = Nothing
        Dim explicitText As String = Global.BESHStatNG.WorksheetFunctions.ExcelArgReaders.CellToTrimmedText(explicitPositive)
        If Not String.IsNullOrWhiteSpace(explicitText) Then
            positiveLabel = explicitText
            Return True
        End If

        Dim distinctTokens As New List(Of String)()
        Dim distinctNormalized As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
        Dim allBinary01 As Boolean = True

        For i As Integer = 0 To statusCol.GetLength(0) - 1
            Dim s As String = Global.BESHStatNG.WorksheetFunctions.ExcelArgReaders.CellToTrimmedText(statusCol(i, 0))
            If String.IsNullOrWhiteSpace(s) Then Continue For

            Dim iv As Integer
            If Not Global.BESHStatNG.WorksheetFunctions.ExcelArgNumeric.TryGetStatus01Flexible(statusCol(i, 0), iv) Then
                allBinary01 = False
            End If

            Dim norm As String = NormalizeOptionKey(s)
            If Not distinctNormalized.Contains(norm) Then
                distinctNormalized.Add(norm)
                distinctTokens.Add(s)
            End If
        Next

        If allBinary01 Then
            positiveLabel = "1"
            Return True
        End If

        If distinctTokens.Count = 2 Then
            Dim norms = distinctTokens.Select(Function(x) NormalizeOptionKey(x)).ToArray()
            For i As Integer = 0 To norms.Length - 1
                Select Case norms(i)
                    Case "positive", "pos", "case", "cases", "event", "events", "yes", "true"
                        positiveLabel = distinctTokens(i)
                        Return True
                End Select
            Next
            positiveLabel = distinctTokens(0)
            Return True
        End If

        Return False
    End Function

End Module