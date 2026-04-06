Option Explicit On
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports System
Imports System.IO
Imports System.Globalization
Imports System.Collections.Generic

<TestClass()>
Public Class Survival_Tests

    Private Shared ReadOnly Invariant As CultureInfo = CultureInfo.InvariantCulture
    Private Const TOL As Double = 0.0000000001

    ' Assumes CSV files are stored under TestData in the test project.
    Private Shared Function GetTestDataPath(fileName As String) As String
        Dim baseDir As String = AppDomain.CurrentDomain.BaseDirectory
        Dim c1 As String = Path.Combine(baseDir, fileName)
        If File.Exists(c1) Then Return c1

        Dim c2 As String = Path.Combine(baseDir, "TestData", fileName)
        If File.Exists(c2) Then Return c2

        Dim c3 As String = Path.GetFullPath(Path.Combine(baseDir, "..\..\TestData", fileName))
        If File.Exists(c3) Then Return c3

        Dim c4 As String = Path.GetFullPath(Path.Combine(baseDir, "..\..\..\TestData", fileName))
        If File.Exists(c4) Then Return c4

        Throw New FileNotFoundException("Test data file not found", fileName)
    End Function

    Private Shared Sub AssertAlmostEqual(expected As Double, actual As Double, tol As Double, msg As String)
        If Double.IsNaN(actual) OrElse Double.IsInfinity(actual) Then
            Assert.Fail($"{msg}: expected {expected} but got {actual}.")
        End If
        Dim diff As Double = Math.Abs(expected - actual)
        If diff > tol Then
            Assert.Fail($"{msg}: expected {expected} but got {actual}. |diff|={diff} > tol={tol}.")
        End If
    End Sub

    Private Shared Function ParseDouble(s As String) As Double
        Return Double.Parse(s.Trim(), NumberStyles.Float, Invariant)
    End Function

    ' CSV schema expected:
    ' id,time,status,group,stratum,x1,x2,x3
    Private Shared Function LoadSurvivalCsv(fileName As String) As List(Of survival.SurvivalRecord)
        Dim path As String = GetTestDataPath(fileName)
        Dim lines() As String = File.ReadAllLines(path)
        Dim out As New List(Of survival.SurvivalRecord)()

        ' Map group labels from CSV to the integer Group IDs used by the production code.
        Dim groupMap As New Dictionary(Of String, Integer)(StringComparer.OrdinalIgnoreCase)
        Dim nextGroupId As Integer = 0

        For i As Integer = 1 To lines.Length - 1 ' skip header
            Dim line As String = lines(i).Trim()
            If line.Length = 0 Then Continue For
            Dim parts() As String = line.Split(","c)

            Dim t As Double = ParseDouble(parts(1))
            Dim status As Integer = Integer.Parse(parts(2).Trim(), Invariant) ' 1=event, 0=censored
            Dim gLabel As String = parts(3).Trim()
            Dim sLabel As String = parts(4).Trim()

            If Not groupMap.ContainsKey(gLabel) Then
                groupMap(gLabel) = nextGroupId
                nextGroupId += 1
            End If

            Dim rec As New survival.SurvivalRecord()
            rec.Time = t
            rec.Censorship = If(status = 1, 1, 0)
            rec.Group = groupMap(gLabel)
            rec.strGroup = gLabel
            rec.Stratum = sLabel
            rec.strStratum = sLabel
            rec.Index = i
            out.Add(rec)
        Next

        Return out
    End Function

    Private Shared Function NewKm(records As List(Of survival.SurvivalRecord)) As survival.Survival_KM_LR
        ' In your updated production code you made Sub New Public.
        Return New survival.Survival_KM_LR(records)
    End Function

    <TestMethod>
    Public Sub KM_tabular_output_basic_matches_reference_points()
        Dim recs = LoadSurvivalCsv("survival_dataset_2group.csv")
        Dim km = NewKm(recs)

        Dim tables() As Object = km.SurvivalCurveTabularOutput()
        Assert.IsNotNull(tables)
        Assert.IsTrue(tables.Length >= 2, "Expected at least two group tables.")

        ' Each entry is List(Of SurvivalTableRecord) (not a 2D array).
        Dim tabList As List(Of survival.SurvivalTableRecord) =
            CType(tables(0), List(Of survival.SurvivalTableRecord))
        Assert.IsNotNull(tabList)
        Assert.IsTrue(tabList.Count > 3, "Expected multiple KM rows.")

        ' Spot-check monotonic decreasing survival and CI within [0,1]
        Dim prev As Double = 1.0
        For r = 0 To tabList.Count - 1
            Dim row = tabList(r)
            Dim p As Double = row.Prob
            Assert.IsTrue(p <= prev + 0.000000000001, $"Survival not non-increasing at row {r}.")
            prev = p
            Assert.IsTrue(row.ProbCILL >= 0 AndAlso row.ProbCILL <= 1, $"CI LL out of range row {r}.")
            Assert.IsTrue(row.ProbCIUL >= 0 AndAlso row.ProbCIUL <= 1, $"CI UL out of range row {r}.")
        Next

        ' Numeric anchor checks at second event time for group 0.
        Dim second = tabList(1)
        AssertAlmostEqual(2.0, second.Time, 0.0, "2nd time (group 0)")
        AssertAlmostEqual(0.88888888889, second.Prob, 0.00000000001, "S(t) at 2nd event (group 0)")
        AssertAlmostEqual(0.104756560176, second.SE, 0.0000000000005, "Greenwood SE at 2nd event (group 0)")
        Assert.AreEqual(9, second.AtRisk, "AtRisk at 2nd event (group 0)")
    End Sub


    <TestMethod>
    Public Sub WeightedLogRank_all_methods_match_reference()
        Dim recs = LoadSurvivalCsv("survival_dataset_2group.csv")
        Dim km = NewKm(recs)

        ' Reference values are stored in TestData\survival_weightedlogrank_reference.csv and can be regenerated by survival_reference.R.
        ' CSV columns: method,chisq,p
        Dim refPath As String = GetTestDataPath("survival_weightedlogrank_reference.csv")
        Dim lines() As String = File.ReadAllLines(refPath)
        Assert.IsTrue(lines.Length >= 2, "Reference CSV must contain a header and at least one row.")

        Dim refs As New Dictionary(Of String, Tuple(Of Double, Double))(StringComparer.OrdinalIgnoreCase)
        For i As Integer = 1 To lines.Length - 1
            Dim ln As String = lines(i).Trim()
            If ln.Length = 0 Then Continue For
            Dim p() As String = ln.Split(","c)
            Dim method As String = p(0).Trim()
            Dim chi As Double = ParseDouble(p(1))
            Dim pv As Double = ParseDouble(p(2))
            refs(method) = Tuple.Create(chi, pv)
        Next

        Dim methods() As String = {"logrank", "gehan-breslow", "tarone-ware", "peto", "modified peto"}
        For Each method In methods
            Assert.IsTrue(refs.ContainsKey(method), $"Missing reference row for method '{method}'.")
            Dim wantChi As Double = refs(method).Item1
            Dim wantP As Double = refs(method).Item2

            Dim res = km.WeightedLogRankTest(method)
            Assert.IsNotNull(res, $"Expected non-null TestResult for method {method}.")
            AssertAlmostEqual(wantChi, res.TestStatistics1, 0.000000001, $"Chi-square mismatch ({method})")
            AssertAlmostEqual(wantP, res.Pvalue, 0.000000001, $"p-value mismatch ({method})")
        Next
    End Sub


    <TestMethod>
    Public Sub WeightedLogRank_returns_Nothing_when_group_all_censored()
        Dim recs = LoadSurvivalCsv("survival_dataset_2group_allcensored.csv")
        Dim km = NewKm(recs)

        Assert.IsTrue(km.AllCenzoredInGroup(), "Expected AllCenzoredInGroup() = True.")
        Dim res = km.WeightedLogRankTest("logrank")
        Assert.IsNull(res, "Expected WeightedLogRankTest to return Nothing when a group is all censored.")
    End Sub

    <TestMethod>
    Public Sub BrookmeyerCrowley_median_CI_has_expected_shape_and_values()
        Dim recs = LoadSurvivalCsv("survival_dataset_2group.csv")
        Dim km = NewKm(recs)

        Dim ci As Object(,) = km.BrookmeyerCrowleyMedianSurvivalCI()
        Assert.IsNotNull(ci)
        Assert.IsTrue(ci.GetLength(1) >= 3, "Expected at least 3 columns: Median, LCL, UCL.")

        ' Ensure median is reached for both groups in this dataset
        For r = 0 To ci.GetLength(0) - 1
            Dim median As Double = CDbl(ci(r, 0))
            Assert.IsTrue(median > 0, $"Expected median > 0 for row {r}.")
            Dim lcl As Double = CDbl(ci(r, 1))
            Dim ucl As Double = CDbl(ci(r, 2))
            Assert.IsTrue(lcl <= median AndAlso median <= ucl, $"Median not within CI for row {r}.")
        Next
    End Sub

    <TestMethod>
    Public Sub CompareCurveFixTimePoint_two_group_returns_expected_rows()
        Dim recs = LoadSurvivalCsv("survival_dataset_2group.csv")
        Dim km = NewKm(recs)

        Dim out As Object(,) = km.CompareCurveFixTimePoint()
        Assert.IsNotNull(out)
        Assert.IsTrue(out.GetLength(1) >= 3, "Expected at least: time, Sdiff, p")
        Assert.IsTrue(out.GetLength(0) >= 2, "Expected multiple time points.")
        ' p-values should be within [0,1]
        For r = 0 To out.GetLength(0) - 1
            Dim p As Double = CDbl(out(r, 2))
            Assert.IsTrue(p >= 0 AndAlso p <= 1, $"p out of range at row {r}.")
        Next
    End Sub

    <TestMethod>
    Public Sub EqualityOfMedianTest_matches_reference()
        Dim recs = LoadSurvivalCsv("survival_dataset.csv")
        Dim km = NewKm(recs)

        Dim res = km.EqualityOfMedianTest()
        Assert.IsNotNull(res)

        ' Reference values from survival_reference.R (VB-aligned pseudo-count method)
        AssertAlmostEqual(1.6681857886633793, res.TestStatistics1, 0.000000001, "EqualityOfMedian chi-square")
        AssertAlmostEqual(0.43426822999321008, res.Pvalue, 0.000000002, "EqualityOfMedian p-value")

    End Sub

    <TestMethod>
    Public Sub wrapResults_returns_nonempty_tables()
        Dim recs = LoadSurvivalCsv("survival_dataset_2group.csv")
        Dim km = NewKm(recs)

        Dim tables = km.wrapResults()
        Assert.IsNotNull(tables)
        Assert.IsTrue(tables.Count > 0, "Expected at least one ResultTable.")
    End Sub

End Class
