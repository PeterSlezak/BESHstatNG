Option Explicit On
Option Strict On
Imports System
Imports System.Collections.Generic
Imports System.Globalization
Imports System.IO
Imports System.Linq
Imports System.Text
Imports BESHStatNG
Imports BESHStatNG.survival
Imports Microsoft.VisualStudio.TestTools.UnitTesting

' Executable transcription of the merged shipping calculation order. NOT_GOLDEN.
' This fixture deliberately does not reference the new Common survival services.
Public NotInheritable Class Survival13Fixture
    Public Shared Function Cases() As IEnumerable(Of Object())
        Dim keys As New List(Of String)
        For Each weight In {"Logrank", "Tarone-Ware", "Gehan-Breslow", "Peto", "modified Peto"}
            For Each stratified In {False, True}
                keys.Add("KM|" & weight & "|" & If(stratified, "strata", "groups"))
            Next
        Next
        keys.AddRange({"KM|Logrank|single", "KM|Logrank|censored", "KM|Logrank|events", "KM|Logrank|mixed",
            "KM|Logrank|three", "KM|Logrank|optional-off", "KM|Logrank|alpha10", "KM|Logrank|zero"})
        For Each tie In {"Breslow", "Efron", "Exact"}
            For Each scenario In {"plain", "strata", "robust", "starts", "limit"}
                keys.Add("Cox|" & tie & "|" & scenario)
            Next
        Next
        keys.AddRange({"Cox|Breslow|noevents", "Cox|Breslow|emptystratum", "Cox|Breslow|singular", "Cox|Breslow|separated"})
        Return keys.Select(Function(k) New Object() {k})
    End Function
    Public Shared Function Records(key As String) As List(Of SurvivalRecord)
        Dim parts = key.Split("|"c), scenario = parts(2), cox = parts(0) = "Cox"
        Dim times As New List(Of Double), status As New List(Of Integer), groups As New List(Of String), strata As New List(Of String)
        For i = 0 To 23
            times.Add(CDbl(1 + (i * 7 Mod 11)))
            If scenario = "zero" AndAlso i < 3 Then times(i) = 0
            status.Add(If(i Mod 4 = 0, 0, 1))
            If scenario = "censored" OrElse scenario = "noevents" Then status(i) = 0
            If scenario = "events" Then status(i) = 1
            groups.Add(If(scenario = "single" OrElse scenario = "censored" OrElse scenario = "events" OrElse cox, "0", If(i Mod 3 = 0, "b", "A")))
            If scenario = "three" Then groups(i) = New String() {"Z", "a", "A"}(i Mod 3)
            If scenario = "mixed" AndAlso groups(i) = "b" Then status(i) = 0
            strata.Add(If(scenario = "strata" OrElse scenario = "emptystratum", If(i Mod 2 = 0, "S z", "S a"), "0"))
            If scenario = "emptystratum" AndAlso strata(i) = "S z" Then status(i) = 0
        Next
        Dim err = String.Empty
        Dim recs = Survival.CreatSurvivalData(times.ToArray(), status.ToArray(), groups.ToArray(), strata.ToArray(), err)
        Assert.AreEqual("", err)
        For i = 0 To recs.Count - 1
            Dim r = recs(i)
            r.Index = 101 + i * 3
            If cox Then
                r.Stratum = strata(i) ' Cox uses literal strata, unlike CreatSurvivalData.
                r.Covariates = {CDbl((i * 5 Mod 13) - 6) / 5, CDbl(i Mod 3)}
                If scenario = "singular" Then r.Covariates(1) = r.Covariates(0)
                If scenario = "separated" Then r.Covariates(0) = CDbl(r.Censorship)
            End If
            recs(i) = r
        Next
        Return recs
    End Function
    Public Shared Function Km(key As String, records As List(Of SurvivalRecord)) As Survival_KM_LR
        Dim p = key.Split("|"c), fit As New Survival_KM_LR(records), alpha = If(p(2) = "alpha10", 0.1, 0.05)
        Dim ng = records.Select(Function(r) r.strGroup).Distinct().Count()
        If ng > 1 Then fit.WeightedLogRankTest(p(1), alpha)
        fit.BrookmeyerCrowleyMedianSurvivalCI(alpha)
        If p(2) <> "optional-off" Then fit.SurvivalCurveTabularOutput(alpha)
        If p(2) <> "optional-off" AndAlso ng > 1 Then fit.EqualityOfMedianTest()
        If p(2) <> "optional-off" AndAlso ng = 2 Then fit.CompareCurveFixTimePoint()
        Return fit
    End Function
    Public Shared Function Cox(key As String, records As List(Of SurvivalRecord)) As CoxPH
        Dim p = key.Split("|"c), fit As New CoxPH(records, {"x1", "x2"}, If(p(2) = "limit", 1, 50), 0.000001)
        fit.bRobustVariance = p(2) = "robust"
        fit.bReturnCov = True : fit.bComputeAllResiduals = True : fit.bComputePHScoreTest = True
        fit.bIterationDetails = True : fit.bTrace = False
        If p(2) = "starts" Then fit.startParams = {0.1, -0.1}
        Return fit
    End Function
    Public Shared Function Snapshot(key As String, Optional kmModel As Survival_KM_LR = Nothing, Optional coxModel As CoxPH = Nothing,
                                    Optional projectedTables As List(Of ResultTable) = Nothing) As String()
        Dim lines As New List(Of String) From {"04A13-Core-NOT_GOLDEN", key}, records = Survival13Fixture.Records(key), p = key.Split("|"c)
        Try
            If p(0) = "KM" Then
                Dim model = If(kmModel, Km(key, records))
                lines.AddRange(Tables(If(projectedTables, model.wrapResults()), records.Select(Function(r) r.Index).ToArray()))
                Dim plot = model.GetKaplanMeierPlotData(If(p(2) = "alpha10", 0.1, 0.05))
                AddPlot(lines, plot)
            Else
                Dim model = If(coxModel, Cox(key, records))
                If coxModel Is Nothing Then model.Fit(DirectCast([Enum].Parse(GetType(TieMethod), p(1)), TieMethod))
                lines.AddRange(Tables(If(projectedTables, model.wrapResults(If(p(2) = "strata" OrElse p(2) = "emptystratum", "stratum", Nothing), 0.05)), records.Select(Function(r) r.Index).ToArray()))
                For Each pair In model.ComputeBaseline(False)
                    lines.Add("STRATUM|" & CStr(pair.Key)) : AddArray(lines, pair.Value)
                    lines.Add("STEP") : AddArray(lines, model.BaseSurvivalForPloting(pair.Value))
                Next
                For Each residual In model.wrapResiduals()
                    lines.AddRange(Regression09Fixture.Canonical(New Regression09Fixture With {.Tables = New List(Of ResultTable), .Ids = {}, .Residual = residual}))
                Next
            End If
        Catch ex As OperationCanceledException
            Throw
        Catch ex As Exception
            lines.Add("CORE_FAILURE|" & ex.GetType().FullName)
        End Try
        Return lines.ToArray()
    End Function
    Public Shared Function Tables(values As List(Of ResultTable), ids() As Integer) As String()
        Return Regression09Fixture.Canonical(New Regression09Fixture With {.Tables = values, .Ids = ids})
    End Function
    Public Shared Sub AddArray(lines As List(Of String), values As Array)
        If values Is Nothing Then lines.Add("NULL") : Return
        lines.Add("ARRAY|" & String.Join(",", Enumerable.Range(0, values.Rank).Select(Function(i) values.GetLength(i))))
        For Each v As Object In values
            If TypeOf v Is Double Then
                lines.Add("D:" & CDbl(v).ToString("R", CultureInfo.InvariantCulture))
            Else
                lines.Add(v.GetType().Name & ":" & Convert.ToBase64String(Encoding.UTF8.GetBytes(Convert.ToString(v, CultureInfo.InvariantCulture))))
            End If
        Next
    End Sub
    Public Shared Sub AddPlot(lines As List(Of String), p As KaplanMeierPlotData)
        AddArray(lines, p.GroupLabels) : AddArray(lines, p.MaximumTimeByGroup) : AddArray(lines, p.CensoredCountByGroup)
        AddArray(lines, p.SurvivalTime) : AddArray(lines, p.SurvivalProbability)
        AddArray(lines, p.LowerConfidenceLimit) : AddArray(lines, p.UpperConfidenceLimit)
        AddArray(lines, p.CensorMarkerTime) : AddArray(lines, p.CensorMarkerProbability)
    End Sub
    Public Shared Function FileName(key As String, culture As String) As String
        Return key.Replace("|", "-").Replace(" ", "") & "-" & If(culture = "", "iv", culture) & If(Environment.Version.Major < 5, "-net48", "") & ".txt"
    End Function
End Class

<TestClass>
Public Class Survival13Baselines
    <DataTestMethod><DynamicData(NameOf(Survival13Fixture.Cases), GetType(Survival13Fixture), DynamicDataSourceType.Method)>
    Public Sub FrozenCoreTablesCurvesResidualsAndFaults(key As String)
        Dim saved = CultureInfo.CurrentCulture
        Try
            For Each culture In {"", "sk-SK"}
                CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(culture)
                Dim actual = Survival13Fixture.Snapshot(key)
                Dim folder = Path.Combine(Regression09Fixture.Root(), "evidence", "04A13", "baselines")
                Dim file = Path.Combine(folder, Survival13Fixture.FileName(key, culture))
                If Environment.GetEnvironmentVariable("BESHSTAT_04A13_CAPTURE") = "PRECHANGE-f7d1c69" Then
                    Directory.CreateDirectory(folder)
                    Using stream As New FileStream(file, FileMode.CreateNew)
                        Using writer As New StreamWriter(stream, New UTF8Encoding(False)) With {.NewLine = ChrW(10)}
                            For Each line In actual : writer.WriteLine(line) : Next
                        End Using
                    End Using
                End If
                Regression10Fixture.AssertCapture(IO.File.ReadAllLines(file), actual, key)
            Next
        Finally
            CultureInfo.CurrentCulture = saved
        End Try
    End Sub
End Class
