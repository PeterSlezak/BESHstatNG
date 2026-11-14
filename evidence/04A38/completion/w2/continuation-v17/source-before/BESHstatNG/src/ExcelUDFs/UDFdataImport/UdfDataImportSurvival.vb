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

        Dim map As New Dictionary(Of String, Integer)(StringComparer.OrdinalIgnoreCase)
        Dim nextId As Integer = 0
        Dim out As New List(Of survival.SurvivalRecord)()

        For i As Integer = 0 To tArr.GetLength(0) - 1
            Dim tVal As Double
            Dim sVal As Integer

            If Not Global.BESHStatNG.WorksheetFunctions.ExcelArgNumeric.TryGetFiniteDoubleFlexible(tArr(i, 0), tVal) Then Continue For
            If tVal < 0.0R Then Continue For
            If Not Global.BESHStatNG.WorksheetFunctions.ExcelArgNumeric.TryGetStatus01Flexible(sArr(i, 0), sVal) Then Continue For

            Dim gKey As String = "All"
            If hasGroup Then
                gKey = Global.BESHStatNG.WorksheetFunctions.ExcelArgReaders.CellToTrimmedText(gArr(i, 0))
                If String.IsNullOrWhiteSpace(gKey) Then Continue For
                gKey = gKey.Trim()
            End If

            Dim gid As Integer
            If Not map.TryGetValue(gKey, gid) Then
                gid = nextId
                map(gKey) = gid
                nextId += 1
            End If

            out.Add(New survival.SurvivalRecord With {
                    .Time = tVal,
                    .Censorship = sVal,
                    .Group = gid,
                    .strGroup = gKey,
                    .Stratum = "",
                    .strStratum = ""
                })
        Next

        records = out
        Return records.Count > 0
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

        Dim tList As New List(Of Double)()
        Dim sList As New List(Of Integer)()
        Dim gList As New List(Of String)()

        For i As Integer = 0 To nRows - 1
            Dim t As Double
            Dim s As Integer

            If Not Global.BESHStatNG.WorksheetFunctions.ExcelArgNumeric.TryGetFiniteDoubleFlexible(timeArr(i, 0), t) Then Continue For
            If Not Global.BESHStatNG.WorksheetFunctions.ExcelArgNumeric.TryGetStatus01Flexible(statusArr(i, 0), s) Then Continue For
            If t < 0.0R Then Return False

            Dim g As String = "ALL"
            If hasGroup Then
                g = Global.BESHStatNG.WorksheetFunctions.ExcelArgReaders.CellToTrimmedText(groupArr(i, 0))
                If String.IsNullOrWhiteSpace(g) Then Continue For
                g = g.Trim()
            End If

            tList.Add(t)
            sList.Add(s)
            gList.Add(g)
        Next

        If tList.Count < 3 Then Return False

        Dim err As String = Nothing
        Dim stratAll() As String = Enumerable.Repeat("ALL", tList.Count).ToArray()
        Dim recs = survival.Survival.CreatSurvivalData(tList.ToArray(), sList.ToArray(), gList.ToArray(), stratAll, err)
        If recs Is Nothing Then Return False

        Dim lr As New survival.Survival_KM_LR(recs)
        Dim mci As Object(,) = lr.BrookmeyerCrowleyMedianSurvivalCI(alpha)
        If mci Is Nothing OrElse mci.GetLength(0) < 1 Then Return False

        Dim groupIds = gList.Distinct().ToList()
        Dim k As Integer = mci.GetLength(0)
        Dim out As Object(,) = New Object(k - 1, 3) {}

        For j As Integer = 0 To k - 1
            Dim med As Double = CDbl(mci(j, 0))
            Dim ll As Double = CDbl(mci(j, 1))
            Dim ul As Double = CDbl(mci(j, 2))

            out(j, 0) = groupIds(j)
            out(j, 1) = If(Double.IsNaN(med) OrElse Double.IsInfinity(med), CType(ExcelError.ExcelErrorNA, Object), CType(med, Object))
            out(j, 2) = If(Double.IsNaN(ll) OrElse Double.IsInfinity(ll), CType(ExcelError.ExcelErrorNA, Object), CType(ll, Object))
            out(j, 3) = If(Double.IsNaN(ul) OrElse Double.IsInfinity(ul), CType(ExcelError.ExcelErrorNA, Object), CType(ul, Object))
        Next

        outTable = out
        Return True
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

        Dim tList As New List(Of Double)()
        Dim sList As New List(Of Integer)()
        Dim gList As New List(Of String)()
        Dim stratList As New List(Of String)()

        For i As Integer = 0 To nRows - 1
            Dim t As Double
            Dim s As Integer
            Dim st As String = "ALL"

            If Not Global.BESHStatNG.WorksheetFunctions.ExcelArgNumeric.TryGetFiniteDoubleFlexible(timeArr(i, 0), t) Then Continue For
            If Not Global.BESHStatNG.WorksheetFunctions.ExcelArgNumeric.TryGetStatus01Flexible(statusArr(i, 0), s) Then Continue For

            Dim g As String = Global.BESHStatNG.WorksheetFunctions.ExcelArgReaders.CellToTrimmedText(groupArr(i, 0))
            If String.IsNullOrWhiteSpace(g) Then Continue For

            If hasStrata Then
                st = Global.BESHStatNG.WorksheetFunctions.ExcelArgReaders.CellToTrimmedText(strataArr(i, 0))
                If String.IsNullOrWhiteSpace(st) Then st = "ALL"
            End If

            If t < 0.0R Then Return False

            tList.Add(t)
            sList.Add(s)
            gList.Add(g.Trim())
            stratList.Add(st.Trim())
        Next

        If tList.Count < 3 Then Return False
        If gList.Distinct().Count() < 2 Then Return False

        Dim err As String = Nothing
        Dim recs = survival.Survival.CreatSurvivalData(tList.ToArray(), sList.ToArray(), gList.ToArray(), stratList.ToArray(), err)
        If recs Is Nothing Then
            errText = err
            Return False
        End If

        Dim lr As New survival.Survival_KM_LR(recs)
        result = lr.WeightedLogRankTest(weightMethod)
        Return result IsNot Nothing
    End Function

    Private Function ParseSurvivalWeightMethod(weight As Object) As String
        Dim w As String = "logrank"
        If Global.BESHStatNG.WorksheetFunctions.ExcelArgPredicates.IsMissingArg(weight) Then Return w

        Dim s As String = Global.BESHStatNG.WorksheetFunctions.ExcelArgReaders.CellToTrimmedText(weight)
        If String.IsNullOrWhiteSpace(s) Then Return w

        Select Case s.Trim().ToLowerInvariant()
            Case "logrank", "lr"
                Return "logrank"
            Case "gehan-breslow", "gehan", "breslow", "wilcoxon"
                Return "gehan-breslow"
            Case "tarone-ware", "tarone", "ware"
                Return "tarone-ware"
            Case "peto", "peto-peto"
                Return "peto"
            Case "modified peto", "modifiedpeto", "anderson", "modpeto"
                Return "modified peto"
            Case Else
                Return Nothing
        End Select
    End Function

    Private Function TryGetSurvival2D(input As Object, ByRef arr As Object(,)) As Boolean
        arr = Get2DOrScalar(input)
        Return arr IsNot Nothing
    End Function

End Module