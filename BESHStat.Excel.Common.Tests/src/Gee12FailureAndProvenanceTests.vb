Option Explicit On
Option Strict On
Imports System
Imports System.Collections.Generic
Imports System.Globalization
Imports System.IO
Imports System.Linq
Imports System.Text
Imports BESHStatNG
Imports BESHStatNG.ExcelCommon
Imports BESHStatNG.regression
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass>
Public Class Gee12FailureAndProvenanceTests
    Public Property TestContext As TestContext
    <DataTestMethod><DataRow("zero")><DataRow("one")><DataRow("singular")><DataRow("single-cluster")><DataRow("singleton-ar")><DataRow("duplicate-unstructured")>
    Public Sub UnstableFitsMatchDirectCoreOutcomeWithoutInventedStatistics(kind As String)
        Dim raw = Gee12Fixture.Raw("Gaussian|Identity|Independence|Robust|time"), r = Gee12PolicyTests.Request("Gaussian|Identity|Independence|Robust|time", raw)
        Dim data = r.MeanRequest.Primary.Data, clusters = r.Cluster.Values, times = r.Time.Values, o = r.MeanRequest.Options, go = r.Options
        o.MaximumIterations = 3
        For i = 0 To raw.nRows - 1
            Select Case kind
                Case "zero", "one" : data(i, 0) = If(kind = "zero", 0.0, 1.0)
                Case "singular" : data(i, 1) = 1.0
                Case "single-cluster" : clusters(i) = "only"
                Case "singleton-ar" : clusters(i) = i
                Case "duplicate-unstructured" : times(i) = 0
            End Select
        Next
        If kind = "zero" OrElse kind = "one" Then o.Family = RegressionFamily.Binomial : o.Link = RegressionLink.Logit
        If kind = "singleton-ar" Then go.Correlation = "Autoregressive"
        If kind = "duplicate-unstructured" Then go.Correlation = "Unstructured"
        Dim mean As New RegressionRequest(RegressionAnalysisKind.GeneralizedLinearModel, "y",
            New RegressionComponent(data, raw.varNames, raw.RowIds, r.MeanRequest.Primary.Effects, {"y", "x"}), o,
            r.MeanRequest.Offset, r.MeanRequest.Weights)
        Dim intent = GeeWorkflowPolicy.Project(mean, New GeeClusterVector("cluster", clusters, raw.RowIds), New RegressionVector("visit", times, raw.RowIds), go)
        Dim oldFamily = FamilyUtils.createFamily(o.Family.Value.ToString()) : oldFamily.pdAlpha = 0.8
        Dim direct As New GEE(oldFamily, LinkUtils.createLink(o.Link.Value.ToString()), GEEcovStructUtils.createGEEcovMat(go.Correlation), "Robust")
        direct.data(data, clusters, raw.RowIds, raw.OffsetData, raw.WeightData, times)
        direct.setVarNames(raw.varNames, "cluster", "offset", "weight", "visit")
        direct.settingInputs(o.Alpha, o.MaximumIterations, o.Epsilon, go.UseP)
        direct.bComputeResiduals = True : direct.bIterationDetails = True
        Dim oldOutcome = Outcome(direct, raw.RowIds), newOutcome = Outcome(intent.CreateCoreModel(), raw.RowIds)
        Assert.AreEqual(oldOutcome, newOutcome, kind)
        TestContext.WriteLine("GEE12_DEGENERATE|" & kind & "|" & If(oldOutcome.StartsWith("CORE_REPORT|", StringComparison.Ordinal), "CORE_REPORT", oldOutcome))
        If Not oldOutcome.StartsWith("CORE_REPORT|", StringComparison.Ordinal) Then
            Dim missing = GeeReportPolicy.Compose(intent, New List(Of ResultTable), Nothing, Nothing, Nothing, Nothing)
            Assert.AreEqual(AnalysisDocumentStatus.Failed, missing.Document.Status)
            Assert.AreEqual(RegressionConvergence.Unknown, missing.Convergence)
            Assert.AreEqual(0, missing.Document.Tables.Count)
        End If
    End Sub

    Private Shared Function Outcome(fit As GEE, rowIds() As Integer) As String
        Try
            fit.Fit(False)
            Dim tables = fit.wrapResults()
            Assert.IsTrue(tables.Count >= 5)
            Dim f As New Regression09Fixture With {.Ids = rowIds, .Tables = tables,
                .Fitted = Regression09Fixture.Column(fit.PredictedResponses), .Residual = fit.AllResiduals}
            Return "CORE_REPORT|" & String.Join(ChrW(10), Regression09Fixture.Canonical(f))
        Catch ex As Exception
            If TypeOf ex Is AssertFailedException Then Throw
            Return ex.GetType().FullName
        End Try
    End Function

    <DataTestMethod><DataRow("time")><DataRow("notime")><DataRow("irregular")>
    Public Sub RawFilteredSortedAndActualCoreInputsHaveReproducibleTypedEvidence(mode As String)
        Dim saved = CultureInfo.CurrentCulture
        Try
            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture
            Dim key = "Gaussian|Identity|Independence|Robust|" & mode, raw = Gee12Fixture.Raw(key)
            Dim input As New ResultTable : input.SetBody(raw.CoreTable.ObjectMatrix)
            input.AddHeaderTopRow(raw.CoreTable.ColumnNames) : input.AddTitle("Raw synthetic acquisition before filtering/sorting")
            Dim mean As New ResultTable : mean.SetBody(Gee12Fixture.Matrix(raw))
            mean.AddHeaderTopRow(raw.varNames) : mean.AddTitle("Actual expanded mean matrix passed to Core")
            Dim roles(raw.nRows - 1, 4) As Object
            For i = 0 To raw.nRows - 1
                roles(i, 0) = raw.RowIds(i) : roles(i, 1) = raw.ClusterIdData(i)
                roles(i, 2) = If(raw.bTime, CType(raw.TimeData(i), Object), Nothing)
                roles(i, 3) = raw.OffsetData(i) : roles(i, 4) = raw.WeightData(i)
            Next
            Dim sidecars As New ResultTable : sidecars.SetBody(roles)
            sidecars.AddHeaderTopRow({"Actual Core Row ID", "Cluster", "Time supplied (Nothing if absent)", "Offset", "Weights (not estimating-equation weights)"})
            Dim f As New Regression09Fixture With {.Ids = raw.RowIds, .Tables = New List(Of ResultTable) From {input, mean, sidecars}}
            Dim actual = Regression09Fixture.Canonical(f), path = IO.Path.Combine(Regression09Fixture.Root(), "evidence", "04A12", "input-" & mode & ".txt")
            If Environment.GetEnvironmentVariable("BESHSTAT_04A12_INPUT_CAPTURE") = "UNCHANGED-FIXTURE-90d990c" Then
                Using stream As New FileStream(path, FileMode.CreateNew)
                    Using writer As New StreamWriter(stream, New UTF8Encoding(False)) With {.NewLine = ChrW(10)}
                        For Each line In actual : writer.WriteLine(line) : Next
                    End Using
                End Using
            End If
            Regression10Fixture.AssertCapture(File.ReadAllLines(path), actual, mode)
            Assert.AreEqual(51, raw.CoreTable.ObjectMatrix.GetLength(0)) : Assert.AreEqual(48, raw.nRows)
            Assert.IsFalse(raw.RowIds.Contains(22)) : Assert.IsFalse(raw.RowIds.Contains(35)) : Assert.IsFalse(raw.RowIds.Contains(51))
            Dim intent = Gee12PolicyTests.Request(key, raw)
            CollectionAssert.AreEqual(Gee12Fixture.Matrix(raw).Cast(Of Double)().ToArray(), intent.MeanRequest.Primary.Data.Cast(Of Double)().ToArray())
            CollectionAssert.AreEqual(raw.RowIds, intent.MeanRequest.Primary.RowIds)
            CollectionAssert.AreEqual(raw.OffsetData, intent.MeanRequest.Offset.Values)
            CollectionAssert.AreEqual(raw.WeightData, intent.MeanRequest.Weights.Values)
        Finally
            CultureInfo.CurrentCulture = saved
        End Try
    End Sub
End Class
