Option Explicit On
Option Strict On
Imports System
Imports System.Collections.Generic
Imports System.Globalization
Imports System.IO
Imports System.Linq
Imports System.Text
Imports BESHStatNG
Imports BESHStatNG.regression
Imports Microsoft.VisualStudio.TestTools.UnitTesting

''' <summary>Pre-edit real Core execution, with the unchanged geeData import/sort.
''' Sanitized deterministic synthetic rows; no reimplementation of GEE equations.</summary>
Public NotInheritable Class Gee12Fixture
    Public Shared Function Cases() As IEnumerable(Of Object())
        Dim keys As New List(Of String)
        For Each cov In GEEcovStruct.CovStructsList
            For Each se In {"Robust", "Naive", "Bias Reduced"}
                keys.Add("Gaussian|Identity|" & cov & "|" & se & "|time")
            Next
            keys.Add("Gaussian|Identity|" & cov & "|Robust|irregular")
        Next
        keys.AddRange({"Gaussian|Identity|Independence|Robust|notime", "Gaussian|Identity|Independence|Robust|intercept",
            "Gaussian|Identity|Independence|Robust|start", "Gaussian|Identity|Independence|Robust|limit",
            "Gaussian|Power|Independence|Robust|time", "Binomial|Logit|Independence|Robust|time",
            "Binomial|Probit|Independence|Naive|time", "Poisson|Log|Independence|Robust|time",
            "NegativeBinomial|Log|Independence|Robust|time", "Gamma|Log|Independence|Robust|time"})
        Return keys.Select(Function(k) New Object() {k})
    End Function

    Public Shared Function Raw(key As String) As geeData
        Dim parts = key.Split("|"c), hasTime = parts(4) <> "notime", interceptOnly = parts(4) = "intercept"
        Dim columns As New List(Of String) From {"y"}
        If Not interceptOnly Then columns.Add("x")
        columns.Add("cluster")
        If hasTime Then columns.Add("visit")
        columns.AddRange({"offset", "weight"})
        Dim input(50, columns.Count - 1) As Object
        Dim valid As Integer = 0
        For row = 0 To 50
            Dim g = valid Mod 12, visit = valid \ 12
            Dim x = (g - 5.5) / 7.0 + visit * 0.5
            Dim y = 2.0 + 0.3 * x + 0.15 * Math.Sin(2.0 * g + visit) + 0.07 * (g Mod 3)
            Select Case parts(0)
                Case "Binomial" : y = If((g * 7 + visit * 3) Mod 11 < 5, 1.0, 0.0)
                Case "Poisson", "NegativeBinomial" : y = 1 + (g Mod 4) + ((g + visit * 3) Mod 3)
            End Select
            input(row, 0) = If(row = 5 OrElse row = 18 OrElse row = 34, Nothing, CType(y, Object))
            Dim c = 1
            If Not interceptOnly Then input(row, c) = x : c += 1
            input(row, c) = CDbl((g * 5) Mod 12) : c += 1
            If hasTime Then
                input(row, c) = If(parts(4) = "irregular", New Double() {0, 2, 7, 11}(visit), CDbl(visit))
                c += 1
            End If
            input(row, c) = 0.02 * (visit - 1.5) : input(row, c + 1) = CDbl(1 + ((g + visit) Mod 3))
            If input(row, 0) IsNot Nothing Then valid += 1
        Next
        Dim imported As New geeData With {.bTime = hasTime, .bOffset = True, .bWeights = True}
        imported.DataImportRawMatrix(input, columns.ToArray(), firstSourceRow:=17)
        Return imported
    End Function

    Public Shared Function Matrix(raw As geeData) As Double(,)
        Dim values(raw.nRows - 1, raw.nCols - 1) As Double
        For i = 0 To raw.nRows - 1
            For j = 0 To raw.nCols - 1 : values(i, j) = CDbl(raw.FinalData(i, j)) : Next
        Next
        Return values
    End Function

    Public Shared Function Model(key As String, raw As geeData) As GEE
        Dim parts = key.Split("|"c), family = FamilyUtils.createFamily(parts(0))
        family.pdAlpha = 0.8
        Dim link = If(parts(1) = "Power", LinkUtils.createLink(parts(1), 0.5), LinkUtils.createLink(parts(1)))
        Dim fit As New GEE(family, link, GEEcovStructUtils.createGEEcovMat(parts(2)), parts(3))
        fit.data(Matrix(raw), raw.ClusterIdData, raw.RowIds, raw.OffsetData, raw.WeightData, If(raw.bTime, raw.TimeData, Nothing))
        fit.setVarNames(raw.varNames, raw.ClusterIdVarName, raw.OffsetVarName, raw.WeightVarName, If(raw.bTime, raw.TimeVarName, Nothing))
        fit.bComputeResiduals = True : fit.bIterationDetails = True
        fit.settingInputs(0.1, If(parts(4) = "limit", 0, 50), 0.000001, False)
        If parts(4) = "start" Then fit.startParams = {2.0, 0.3}
        Return fit
    End Function

    Public Shared Function Execute(key As String, Optional model As GEE = Nothing, Optional raw As geeData = Nothing) As String()
        raw = If(raw, Gee12Fixture.Raw(key))
        Dim lines As New List(Of String) From {"04A12-real-Core-NOT_GOLDEN", key,
            "IDS|" & String.Join(",", raw.RowIds), "CLUSTERS|" & String.Join(",", raw.ClusterIdData),
            "TIME|" & If(raw.bTime, String.Join(",", raw.TimeData.Select(Function(t) t.ToString("R", CultureInfo.InvariantCulture))), "ABSENT")}
        Try
            Dim fit = If(model, Gee12Fixture.Model(key, raw))
            fit.Fit(key.EndsWith("|start", StringComparison.Ordinal))
            lines.AddRange(Snapshot(raw, fit))
        Catch ex As OperationCanceledException
            Throw
        Catch ex As Exception
            lines.Add("CORE_FAILURE|" & ex.GetType().FullName)
        End Try
        Return lines.ToArray()
    End Function

    Public Shared Function Snapshot(raw As geeData, fit As GEE) As String()
        Dim f As New Regression09Fixture With {.Data = Matrix(raw), .Names = raw.varNames, .Ids = raw.RowIds,
            .Tables = fit.wrapResults(), .Fitted = Regression09Fixture.Column(fit.PredictedResponses),
            .Residual = If(fit.bComputeResiduals, fit.AllResiduals, Nothing)}
        Dim lines As New List(Of String)(Regression09Fixture.Canonical(f))
        For Each name In {"Robust", "Naive", "Bias Reduced"}
            Dim cov = fit.GetParameterCovarianceMatrix(name)
            lines.Add("COVARIANCE|" & name)
            AddMatrix(lines, cov)
        Next
        lines.Add("WORKING_CORRELATION") : AddMatrix(lines, fit.WorkingCorrelationMatrix)
        lines.Add("OBSERVED|" & String.Join(",", fit.ObservedResponses.Select(Function(v) v.ToString("R", CultureInfo.InvariantCulture))))
        lines.Add("LINEAR|" & String.Join(",", fit.LinearPredictors.Select(Function(v) v.ToString("R", CultureInfo.InvariantCulture))))
        lines.Add("WEIGHTS|" & String.Join(",", fit.ObservationWeights.Select(Function(v) v.ToString("R", CultureInfo.InvariantCulture))))
        Return lines.ToArray()
    End Function

    Private Shared Sub AddMatrix(lines As List(Of String), matrix(,) As Double)
        If matrix Is Nothing Then lines.Add("NOT_AVAILABLE") : Return
        lines.Add($"AXIS|{matrix.GetLength(0)}|{matrix.GetLength(1)}")
        For i = 0 To matrix.GetLength(0) - 1
            Dim row = i
            lines.Add(String.Join(",", Enumerable.Range(0, matrix.GetLength(1)).Select(Function(j) matrix(row, j).ToString("R", CultureInfo.InvariantCulture))))
        Next
    End Sub

    Public Shared Function FileName(key As String, culture As String) As String
        Return key.Replace("|", "-").Replace(" ", "") & "-" & If(culture = "", "iv", culture) & If(Environment.Version.Major < 5, "-net48", "") & ".txt"
    End Function

    Public Shared Function BaselineRoot() As String
        Return Path.Combine(Regression09Fixture.Root(), "evidence", "04A12", "baselines")
    End Function
End Class

<TestClass>
Public Class Gee12Baselines
    <DataTestMethod><DynamicData(NameOf(Gee12Fixture.Cases), GetType(Gee12Fixture), DynamicDataSourceType.Method)>
    Public Sub FrozenCoreImportFitTablesAndAllVectors(key As String)
        Dim saved = CultureInfo.CurrentCulture
        Try
            For Each culture In {"", "sk-SK"}
                CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(culture)
                Dim lines = Gee12Fixture.Execute(key), path = IO.Path.Combine(Gee12Fixture.BaselineRoot(), Gee12Fixture.FileName(key, culture))
                If Environment.GetEnvironmentVariable("BESHSTAT_04A12_CAPTURE") = "PRECHANGE-90d990c" Then
                    Directory.CreateDirectory(Gee12Fixture.BaselineRoot())
                    Using stream As New FileStream(path, FileMode.CreateNew)
                        Using writer As New StreamWriter(stream, New UTF8Encoding(False)) With {.NewLine = ChrW(10)}
                            For Each line In lines : writer.WriteLine(line) : Next
                        End Using
                    End Using
                End If
                Regression10Fixture.AssertCapture(File.ReadAllLines(path), lines, key)
            Next
        Finally
            CultureInfo.CurrentCulture = saved
        End Try
    End Sub
End Class
