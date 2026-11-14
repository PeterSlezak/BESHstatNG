Option Explicit On
Option Strict On

Imports System
Imports System.Globalization
Imports System.Collections.Generic
Imports BESHStatNG
Imports BESHStatNG.ExcelCommon
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass>
Public Class UdfContinuation11Tests
    Private Shared Function Factor() As Multivariate.FactorAnalysis
        Dim model As New Multivariate.FactorAnalysis
        model.dataInputs(New Double(,) {{1, 2, 5}, {2, 1, 7}, {3, 4, 6}, {4, 3, 8}, {5, 6, 9}, {6, 5, 10}, {7, 8, 11}, {8, 7, 12}},
            New Integer() {17, 3, 9, 4, 5, 6, 7, 8}, New String() {"same", "same", "Z"})
        model.settingsInputs(extractionMethod:=Multivariate.FactorAnalysisExtractionMethod.PrincipalComponents,
            retentionMethod:=Multivariate.FactorAnalysisRetentionMethod.Fixed, retentionValue:=2)
        model.Calculate()
        Return model
    End Function

    Private Shared Function KMeans() As Multivariate.KMeans
        Dim model As New Multivariate.KMeans
        model.dataInputs(New Double(,) {{1, 2}, {Double.NaN, 3}, {2, 1}, {9, 8}, {8, 9}},
            New String() {"A", "removed", "A", "D", "E"}, New String() {"same", "same"})
        model.settingsInputs(numberOfClusters:=2, nStarts:=1, randomSeed:=38,
            standardization:=Multivariate.ClusterStandardizationMode.ZScores,
            missingValuePolicy:=Multivariate.ClusterMissingValuePolicy.ListwiseDeletion)
        model.Fit()
        Return model
    End Function

    Private Shared Function Hierarchical() As Multivariate.HierarchicalClustering
        Dim model As New Multivariate.HierarchicalClustering
        model.dataInputs(New Double(,) {{1, 2}, {Double.NaN, 3}, {2, 1}, {9, 8}, {8, 9}},
            New String() {"A", "removed", "A", "D", "E"}, New String() {"same", "same"})
        model.settingsInputs(standardization:=Multivariate.ClusterStandardizationMode.ZScores,
            missingValuePolicy:=Multivariate.ClusterMissingValuePolicy.ListwiseDeletion)
        model.Fit()
        Return model
    End Function

    <DataTestMethod, DataRow(True, ""), DataRow(False, ""), DataRow(True, "sk-SK"), DataRow(False, "sk-SK")>
    Public Sub FactorCommunalitiesKeepColumnsValuesAndDuplicateNames(header As Boolean, culture As String)
        Dim previous = CultureInfo.CurrentCulture
        Try
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(culture)
            Dim model = Factor(), names = New String() {"same", "same", "Z"}
            Dim output = UdfMultivariateReportPreparation38.BuildFactorCommunalities(model, names,
                model.FactorNames("Factor "), model.CommunalityContributionsByFactor(), 3, 2, header)
            Dim start = If(header, 1, 0)
            Assert.AreEqual(3 + start, output.GetLength(0))
            Assert.AreEqual(6, output.GetLength(1))
            Assert.AreEqual("same", output(start, 0))
            Assert.AreEqual("same", output(start + 1, 0))
            Assert.AreEqual(model.Communalities(0), CDbl(output(start, 4)))
            Assert.AreEqual(model.Uniquenesses(0), CDbl(output(start, 5)))
            If header Then Assert.AreEqual("Factor 1 Contribution", output(0, 2))
        Finally
            CultureInfo.CurrentCulture = previous
        End Try
    End Sub

    <DataTestMethod, DataRow(True), DataRow(False)>
    Public Sub FactorabilityRetainsOriginalInclusiveBoundFailure(header As Boolean)
        Assert.ThrowsException(Of IndexOutOfRangeException)(Sub() UdfMultivariateReportPreparation38.BuildFactorability(Factor(), header))
    End Sub

    <TestMethod>
    Public Sub FactorabilityNullModelPrecedesFinalBoundFailure()
        Assert.ThrowsException(Of NullReferenceException)(Sub() UdfMultivariateReportPreparation38.BuildFactorability(Nothing, False))
    End Sub

    <TestMethod>
    Public Sub CommunalitiesRetainHeaderFactorNameFailureBeforeModelRead()
        Assert.ThrowsException(Of IndexOutOfRangeException)(Sub() UdfMultivariateReportPreparation38.BuildFactorCommunalities(
            Nothing, Nothing, New String() {}, Nothing, 1, 1, True))
    End Sub

    <DataTestMethod, DataRow(True, ""), DataRow(False, ""), DataRow(True, "sk-SK"), DataRow(False, "sk-SK")>
    Public Sub KMeansSummaryKeepsTypedCountsAndListwiseRows(header As Boolean, culture As String)
        Dim previous = CultureInfo.CurrentCulture
        Try
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(culture)
            Dim model = KMeans()
            Dim output = UdfMultivariateReportPreparation38.BuildKMeansSummary(model, New String() {"same", "same"},
                "KMeansPlusPlus", "SquaredEuclidean", "ZScores", "ListwiseDeletion", "FarthestObservation", 100, 0.000001R, header)
            Dim start = If(header, 1, 0)
            Assert.AreEqual(19 + start, output.GetLength(0))
            Assert.AreEqual(4, output(start + 1, 1))
            Assert.AreEqual(1, output(start + 2, 1))
            Assert.AreEqual(38, output(start + 18, 1))
            Assert.AreEqual(model.Result.ObjectiveValue, CDbl(output(start + 17, 1)))
            CollectionAssert.AreEqual(New Integer() {1, 3, 4, 5}, model.Result.ActiveRowIndices)
            CollectionAssert.AreEqual(New String() {"removed"}, model.Result.RemovedRowLabels)
        Finally
            CultureInfo.CurrentCulture = previous
        End Try
    End Sub

    <TestMethod>
    Public Sub KMeansUnfittedResultRemainsAnError()
        Assert.ThrowsException(Of NullReferenceException)(Sub() UdfMultivariateReportPreparation38.BuildKMeansSummary(
            New Multivariate.KMeans(), Nothing, Nothing, Nothing, Nothing, Nothing, Nothing, 100, 0.000001R, False))
    End Sub

    <DataTestMethod, DataRow(True), DataRow(False)>
    Public Sub HierarchicalSummaryKeepsFinalHeightAndReferenceSlot(header As Boolean)
        Dim model = Hierarchical(), finalHeight As Object = model.Result.MergeHeights(model.Result.MergeHeights.Length - 1)
        Dim before = finalHeight
        Dim output = UdfMultivariateReportPreparation38.BuildHierarchicalSummary(model, New String() {"same", "same"},
            "Ward", "SquaredEuclidean", 2, "ZScores", "ListwiseDeletion", model.Result.MergeHeights.Length, finalHeight, header)
        Dim start = If(header, 1, 0)
        Assert.AreEqual(11 + start, output.GetLength(0))
        Assert.AreEqual(4, output(start + 5, 1))
        Assert.AreEqual(1, output(start + 6, 1))
        Assert.AreEqual(CDbl(finalHeight), CDbl(output(start + 10, 1)))
        Assert.AreSame(before, finalHeight)
        CollectionAssert.AreEqual(New Integer() {1, 3, 4, 5}, model.Result.ActiveRowIndices)
    End Sub

    <TestMethod>
    Public Sub HierarchicalBlankHeightDoesNotInventAValue()
        Dim model = Hierarchical(), slot As Object = Nothing
        Dim output = UdfMultivariateReportPreparation38.BuildHierarchicalSummary(model, New String() {"x", "y"},
            "Ward", "SquaredEuclidean", 2, "ZScores", "ListwiseDeletion", 0, slot, False)
        Assert.IsNull(output(10, 1))
        Assert.IsNull(slot)
    End Sub

    <DataTestMethod, DataRow(True), DataRow(False)>
    Public Sub CaSummaryKeepsLabelledAxesAndInertia(header As Boolean)
        Dim model As New Multivariate.CA
        model.data(New Integer(,) {{8, 2, 1}, {2, 7, 3}, {1, 3, 9}}, New String() {"a", "a", "c"}, New String() {"x", "y", "z"})
        model.Calculate()
        Dim output = UdfMultivariateReportPreparation38.BuildCaSummary(model, model.Eigenvalues, header)
        Dim start = If(header, 1, 0)
        Assert.AreEqual(6 + start, output.GetLength(0))
        Assert.AreEqual("Simple correspondence analysis", output(start, 1))
        Assert.AreEqual(3, output(start + 1, 1))
        Assert.AreEqual(model.Eigenvalues.Length, output(start + 3, 1))
        Assert.IsInstanceOfType(output(start + 4, 1), GetType(Double))
    End Sub

    <TestMethod>
    Public Sub CaMissingEigenvaluesRemainBlankInSummary()
        Dim model As New Multivariate.CA
        model.data(New Integer(,) {{8, 2}, {2, 7}}, New String() {"a", "b"}, New String() {"x", "y"})
        model.Calculate()
        Dim output = UdfMultivariateReportPreparation38.BuildCaSummary(model, Nothing, False)
        Assert.AreEqual(0, output(3, 1))
        Assert.IsNull(output(4, 1))
    End Sub

    <TestMethod>
    Public Sub AxisAvailabilityStopsAtFirstThrowWithoutReadingLaterAxes()
        Dim seen As New List(Of Integer)
        Dim count = MultivariateAnalysisUDFsReports38.ResolveAvailableAxisCount(4,
            Function(axis)
                seen.Add(axis)
                If axis = 1 Then Throw New InvalidOperationException("axis boundary")
                Return New Double() {1, 2}
            End Function)
        Assert.AreEqual(1, count)
        CollectionAssert.AreEqual(New Integer() {0, 1}, seen.ToArray())
    End Sub

    <TestMethod>
    Public Sub RemovedRowsRetainSourceIdsAndMismatchedLabelFallback()
        Dim body = MultivariateAnalysisUDFsReports38.BuildRemovedRowsOutput(New Integer() {17, 3}, New String() {"only one"}).ToOutputModel().Values
        Assert.AreEqual(2, body.GetLength(1))
        Assert.AreEqual(17, body(1, 0))
        Assert.AreEqual(3, body(2, 0))
    End Sub

    <TestMethod>
    Public Sub PreprocessingRetainsPartialArrayBlanksAndDuplicateNames()
        Dim body = MultivariateAnalysisUDFsReports38.BuildClusterPreprocessingTable(New String() {"same", "same"},
            New Double() {2}, Nothing, Multivariate.ClusterStandardizationMode.ZScores).ToOutputModel().Values
        Assert.AreEqual(3, body.GetLength(0))
        Assert.AreEqual(2.0R, body(1, 1))
        Assert.IsNull(body(2, 1))
        Assert.IsNull(body(1, 2))
    End Sub
End Class
