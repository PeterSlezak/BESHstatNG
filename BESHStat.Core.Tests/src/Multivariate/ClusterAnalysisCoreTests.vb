Option Explicit On
Option Strict On
Option Infer On

Imports System
Imports System.Linq
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports BESHStatNG
Imports BESHStatNG.Multivariate

''' <summary>
''' Direct .NET 8 reference tests for Core k-means and hierarchical clustering.
''' The same historical reference checks remain in the Framework test assembly
''' to cover the Framework-to-Core boundary. Test inputs match the legacy CSV
''' datasets but are embedded to keep Core tests independent of Excel/host files.
''' </summary>
<TestClass>
Public Class ClusterAnalysisCoreTests

    Private Shared Function LoadClusterData(Optional includeMissing As Boolean = False) As (data As Double(,), rowLabels As String(), varNames As String())
        Dim basic As Double(,) = {
            {0.0R, 0.0R},
            {0.0R, 1.0R},
            {2.0R, 0.0R},
            {10.0R, 10.0R},
            {10.0R, 12.0R},
            {13.0R, 10.0R}
        }
        Dim labels As String() = {"A", "B", "C", "D", "E", "F"}
        If Not includeMissing Then Return (basic, labels, New String() {"x1", "x2"})

        Dim withMissing(6, 1) As Double
        For i As Integer = 0 To basic.GetLength(0) - 1
            For j As Integer = 0 To basic.GetLength(1) - 1
                withMissing(i, j) = basic(i, j)
            Next
        Next
        withMissing(6, 0) = Double.NaN
        withMissing(6, 1) = 99.0R
        Return (withMissing, New String() {"A", "B", "C", "D", "E", "F", "G"}, New String() {"x1", "x2"})
    End Function

    Private Shared Sub AssertClose(expected As Double, actual As Double, absTol As Double, Optional relTol As Double = 0.0, Optional msg As String = "")
        Dim diff As Double = Math.Abs(expected - actual)
        Dim ok As Boolean = diff <= absTol
        If Not ok AndAlso relTol > 0 Then
            Dim denom As Double = Math.Max(Math.Abs(expected), Math.Abs(actual))
            If denom > 0 Then ok = (diff / denom) <= relTol
        End If
        If Not ok Then
            Assert.Fail($"{msg} Expected {expected:R}, got {actual:R}, diff={diff:R}")
        End If
    End Sub

    Private Shared Sub AssertVectorClose(expected() As Double, actual() As Double, absTol As Double, Optional relTol As Double = 0.0, Optional msg As String = "")
        Assert.AreEqual(expected.Length, actual.Length, $"{msg} Length mismatch")
        For i As Integer = 0 To expected.Length - 1
            AssertClose(expected(i), actual(i), absTol, relTol, $"{msg} [i={i}]")
        Next
    End Sub

    Private Shared Sub AssertMatrixClose(expected(,) As Double, actual(,) As Double, absTol As Double, Optional relTol As Double = 0.0, Optional msg As String = "")
        Assert.AreEqual(expected.GetLength(0), actual.GetLength(0), $"{msg} RowUdfs count mismatch")
        Assert.AreEqual(expected.GetLength(1), actual.GetLength(1), $"{msg} ColUdfs count mismatch")
        For i As Integer = 0 To expected.GetLength(0) - 1
            For j As Integer = 0 To expected.GetLength(1) - 1
                AssertClose(expected(i, j), actual(i, j), absTol, relTol, $"{msg} [i={i},j={j}]")
            Next
        Next
    End Sub

    Private Shared Function GetTitleText(tbl As ResultTable) As String
        Dim m(,) As Object = tbl.returnSelf()
        If m Is Nothing Then Return String.Empty
        If m.GetLength(0) = 0 OrElse m.GetLength(1) = 0 Then Return String.Empty
        Return If(m(0, 0), String.Empty).ToString()
    End Function

    <TestMethod>
    Public Sub KMeans_UserSpecifiedCenters_FitsExpectedPartitionAndCenters()
        Dim loaded = LoadClusterData()

        Dim km As New KMeans()
        km.dataInputs(loaded.data, loaded.rowLabels, loaded.varNames)
        km.startingCentersInputs(New Double(,) {{0.0, 0.0}, {10.0, 10.0}})
        km.settingsInputs(numberOfClusters:=2,
                          initialization:=KMeansInitializationMethod.UserSpecifiedCenters,
                          distanceMetric:=KMeansDistanceMetric.SquaredEuclidean,
                          nStarts:=10,
                          maxIterations:=100,
                          convergenceTolerance:=0.000001,
                          standardization:=ClusterStandardizationMode.None,
                          missingValuePolicy:=ClusterMissingValuePolicy.ErrorOnMissing,
                          emptyClusterHandling:=EmptyClusterHandlingStrategy.FarthestObservation,
                          randomSeed:=123)
        km.Fit()

        Dim res = km.Result
        CollectionAssert.AreEqual(New Integer() {1, 1, 1, 2, 2, 2}, res.ClusterAssignments)
        AssertMatrixClose(New Double(,) {{2.0 / 3.0, 1.0 / 3.0}, {11.0, 32.0 / 3.0}}, res.CentersOriginalScale, 0.000000001, 0.0, "Centers")
        AssertClose(12.0, res.TotalWithinClusterSS, 0.000000001, 0.0, "TotalWithinClusterSS")
        AssertClose(10.0 / 3.0, res.WithinClusterSSByCluster(0), 0.000000001, 0.0, "Cluster1 SSE")
        AssertClose(26.0 / 3.0, res.WithinClusterSSByCluster(1), 0.000000001, 0.0, "Cluster2 SSE")
        CollectionAssert.AreEqual(New Integer() {1, 2, 3, 4, 5, 6}, res.ActiveRowIndices)
        CollectionAssert.AreEqual(New String() {"A", "B", "C", "D", "E", "F"}, res.ActiveRowLabels)
        Assert.AreEqual(0, res.RemovedRowIndices.Length)
    End Sub

    <TestMethod>
    Public Sub KMeans_ListwiseDeletion_TracksRemovedRowsAndLabels()
        Dim loaded = LoadClusterData(includeMissing:=True)

        Dim km As New KMeans()
        km.dataInputs(loaded.data, loaded.rowLabels, loaded.varNames)
        km.startingCentersInputs(New Double(,) {{0.0, 0.0}, {10.0, 10.0}})
        km.settingsInputs(numberOfClusters:=2,
                          initialization:=KMeansInitializationMethod.UserSpecifiedCenters,
                          missingValuePolicy:=ClusterMissingValuePolicy.ListwiseDeletion,
                          randomSeed:=123)
        km.Fit()

        Dim res = km.Result
        CollectionAssert.AreEqual(New Integer() {1, 1, 1, 2, 2, 2}, res.ClusterAssignments)
        CollectionAssert.AreEqual(New Integer() {7}, res.RemovedRowIndices)
        CollectionAssert.AreEqual(New String() {"G"}, res.RemovedRowLabels)
        CollectionAssert.AreEqual(New String() {"A", "B", "C", "D", "E", "F"}, res.ActiveRowLabels)
        AssertClose(12.0, res.TotalWithinClusterSS, 0.000000001)
    End Sub

    <TestMethod>
    Public Sub KMeans_Predict_AssignsNewPointsToNearestCluster()
        Dim loaded = LoadClusterData()

        Dim km As New KMeans()
        km.dataInputs(loaded.data, loaded.rowLabels, loaded.varNames)
        km.startingCentersInputs(New Double(,) {{0.0, 0.0}, {10.0, 10.0}})
        km.settingsInputs(numberOfClusters:=2,
                          initialization:=KMeansInitializationMethod.UserSpecifiedCenters,
                          distanceMetric:=KMeansDistanceMetric.Euclidean,
                          randomSeed:=123)
        km.Fit()

        Dim newData(,) As Double = {{0.1, 0.2}, {11.8, 10.3}}
        Dim clusters() As Integer = km.Predict(newData)
        Dim distances() As Double = km.DistanceToNearestCluster(newData)

        CollectionAssert.AreEqual(New Integer() {1, 2}, clusters)
        AssertClose(Math.Sqrt((0.1 - 2.0 / 3.0) ^ 2 + (0.2 - 1.0 / 3.0) ^ 2), distances(0), 0.000000001)
        AssertClose(Math.Sqrt((11.8 - 11.0) ^ 2 + (10.3 - 32.0 / 3.0) ^ 2), distances(1), 0.000000001)
    End Sub

    <TestMethod>
    Public Sub KMeans_WrapResults_ReturnsProjectStyleTables()
        Dim loaded = LoadClusterData()

        Dim km As New KMeans()
        km.dataInputs(loaded.data, loaded.rowLabels, loaded.varNames)
        km.startingCentersInputs(New Double(,) {{0.0, 0.0}, {10.0, 10.0}})
        km.settingsInputs(numberOfClusters:=2,
                          initialization:=KMeansInitializationMethod.UserSpecifiedCenters,
                          standardization:=ClusterStandardizationMode.ZScores,
                          randomSeed:=123)
        km.Fit()

        Dim tables = km.wrapResults()
        Assert.AreEqual(6, tables.Count, "Expected settings, summary, original centers, working centers, preprocessing constants, and assignments.")
        Assert.AreEqual("K-Means Settings", GetTitleText(tables(0)))
        Assert.AreEqual("K-Means Fit Summary", GetTitleText(tables(1)))
        Assert.AreEqual("Cluster Centers (Original Scale)", GetTitleText(tables(2)))
        Assert.AreEqual("Cluster Centers (Working Analysis Scale)", GetTitleText(tables(3)))
        Assert.AreEqual("Preprocessing Constants", GetTitleText(tables(4)))
        Assert.AreEqual("Observation Assignments", GetTitleText(tables(5)))
    End Sub

    <TestMethod>
    Public Sub Hierarchical_CompleteEuclidean_MatchesReferenceScheduleAndMembership()
        Dim loaded = LoadClusterData()

        Dim hc As New HierarchicalClustering()
        hc.dataInputs(loaded.data, loaded.rowLabels, loaded.varNames)
        hc.settingsInputs(linkage:=HierarchicalLinkageMethod.Complete,
                          distanceMetric:=HierarchicalDistanceMetric.Euclidean,
                          standardization:=ClusterStandardizationMode.None,
                          missingValuePolicy:=ClusterMissingValuePolicy.ErrorOnMissing)
        hc.reportInputs(cutMode:=HierarchicalMembershipDisplayMode.ByClusterCount,
                        membershipClusterCount:=2)
        hc.Fit()

        Dim res = hc.Result
        AssertVectorClose(New Double() {1.0, 2.0, Math.Sqrt(5.0), Math.Sqrt(13.0), Math.Sqrt(269.0)}, res.MergeHeights, 0.000000001, 0.0, "Merge heights")
        CollectionAssert.AreEqual(New Integer() {1, 1, 1, 2, 2, 2}, res.GetMembershipByClusterCount(2))
        CollectionAssert.AreEqual(New Integer() {1, 2, 3, 4, 5, 6}, res.LeafOrder)
    End Sub

    <TestMethod>
    Public Sub Hierarchical_ByHeight_ListwiseDeletion_TracksRemovedRows_AndWrapResults()
        Dim loaded = LoadClusterData(includeMissing:=True)

        Dim hc As New HierarchicalClustering()
        hc.dataInputs(loaded.data, loaded.rowLabels, loaded.varNames)
        hc.settingsInputs(linkage:=HierarchicalLinkageMethod.Complete,
                          distanceMetric:=HierarchicalDistanceMetric.Euclidean,
                          standardization:=ClusterStandardizationMode.None,
                          missingValuePolicy:=ClusterMissingValuePolicy.ListwiseDeletion)
        hc.reportInputs(cutMode:=HierarchicalMembershipDisplayMode.ByHeight,
                        membershipCutHeight:=4.0)
        hc.Fit()

        Dim res = hc.Result
        CollectionAssert.AreEqual(New Integer() {1, 1, 1, 2, 2, 2}, res.GetMembershipByHeight(4.0))
        CollectionAssert.AreEqual(New Integer() {7}, res.RemovedRowIndices)
        CollectionAssert.AreEqual(New String() {"G"}, res.RemovedRowLabels)

        Dim tables = hc.wrapResults()
        Assert.AreEqual(6, tables.Count, "Expected settings, fit summary, agglomeration schedule, leaf order, membership, and removed rows.")
        Assert.AreEqual("Hierarchical Clustering Settings", GetTitleText(tables(0)))
        Assert.AreEqual("Hierarchical Clustering Fit Summary", GetTitleText(tables(1)))
        Assert.AreEqual("Agglomeration Schedule", GetTitleText(tables(2)))
        Assert.AreEqual("Leaf Order", GetTitleText(tables(3)))
        StringAssert.StartsWith(GetTitleText(tables(4)), "Cluster Membership (Cut Height <=")
        Assert.AreEqual("Rows Removed by Missing-Value Policy", GetTitleText(tables(5)))
    End Sub

    <TestMethod>
    Public Sub Hierarchical_DendrogramLayout_LeftOrientation_UsesDistanceOnXCoordinate()
        Dim loaded = LoadClusterData()

        Dim hc As New HierarchicalClustering()
        hc.dataInputs(loaded.data, loaded.rowLabels, loaded.varNames)
        hc.settingsInputs(linkage:=HierarchicalLinkageMethod.Complete,
                          distanceMetric:=HierarchicalDistanceMetric.Euclidean)
        hc.Fit()

        Dim layout = hc.Result.CreateDendrogramLayout(heightMode:=DendrogramHeightMode.MergeDistance,
                                                      orientation:=DendrogramOrientation.Left,
                                                      cutMode:=HierarchicalMembershipDisplayMode.ByClusterCount,
                                                      membershipClusterCount:=2)

        Assert.IsNotNull(layout.CutLineX)
        Assert.AreEqual(2, layout.CutLineX.Length)
        Assert.AreEqual(layout.LeafCount, layout.LeafX.Length)
        For i As Integer = 0 To layout.LeafX.Length - 1
            AssertClose(layout.MaximumHeight, layout.LeafX(i), 0.000000001, 0.0, "Leaves should lie on the x-axis distance baseline in Left orientation.")
        Next
    End Sub

    <TestMethod>
    <TestCategory("Multivariate-Core-Clustering")>
    Public Sub KMeans_InvalidClusterCountRejected()
        Dim km As New KMeans()
        Assert.ThrowsException(Of ArgumentException)(Sub() km.settingsInputs(numberOfClusters:=0))
    End Sub

    <TestMethod>
    <TestCategory("Multivariate-Core-Clustering")>
    Public Sub KMeans_RejectsMissingObservationsUnderErrorPolicy()
        Dim loaded = LoadClusterData(includeMissing:=True)
        Dim km As New KMeans()
        km.dataInputs(loaded.data, loaded.rowLabels, loaded.varNames)
        km.settingsInputs(numberOfClusters:=2,
                          missingValuePolicy:=ClusterMissingValuePolicy.ErrorOnMissing,
                          randomSeed:=123)
        Assert.ThrowsException(Of ArgumentException)(Sub() km.Fit())
    End Sub

    <TestMethod>
    <TestCategory("Multivariate-Core-Clustering")>
    Public Sub KMeans_SeededKMeansPlusPlusIsReproducible()
        Dim loaded = LoadClusterData()
        Dim first As New KMeans()
        first.dataInputs(loaded.data, loaded.rowLabels, loaded.varNames)
        first.settingsInputs(numberOfClusters:=2,
                             initialization:=KMeansInitializationMethod.KMeansPlusPlus,
                             nStarts:=5, randomSeed:=947)
        first.Fit()

        Dim second As New KMeans()
        second.dataInputs(loaded.data, loaded.rowLabels, loaded.varNames)
        second.settingsInputs(numberOfClusters:=2,
                              initialization:=KMeansInitializationMethod.KMeansPlusPlus,
                              nStarts:=5, randomSeed:=947)
        second.Fit()

        Assert.AreEqual(947, first.Result.RandomSeedUsed)
        Assert.AreEqual(947, second.Result.RandomSeedUsed)
        AssertVectorClose(first.Result.WithinClusterSSByCluster,
                          second.Result.WithinClusterSSByCluster, 0.0R)
        CollectionAssert.AreEqual(first.Result.ClusterAssignments, second.Result.ClusterAssignments)
        AssertMatrixClose(first.Result.CentersOriginalScale, second.Result.CentersOriginalScale, 0.0R)
    End Sub

    <TestMethod>
    <TestCategory("Multivariate-Core-Clustering")>
    Public Sub Hierarchical_WardRejectsManhattanDistance()
        Dim loaded = LoadClusterData()
        Dim hc As New HierarchicalClustering()
        hc.dataInputs(loaded.data, loaded.rowLabels, loaded.varNames)
        hc.settingsInputs(linkage:=HierarchicalLinkageMethod.Ward,
                          distanceMetric:=HierarchicalDistanceMetric.Manhattan)
        Assert.ThrowsException(Of ArgumentException)(Sub() hc.Fit())
    End Sub

    <TestMethod>
    <TestCategory("Multivariate-Core-Clustering")>
    Public Sub Hierarchical_ClusterCountOutsideActiveRowsIsRejected()
        Dim loaded = LoadClusterData()
        Dim hc As New HierarchicalClustering()
        hc.dataInputs(loaded.data, loaded.rowLabels, loaded.varNames)
        hc.settingsInputs(linkage:=HierarchicalLinkageMethod.Complete,
                          distanceMetric:=HierarchicalDistanceMetric.Euclidean)
        hc.Fit()
        Assert.ThrowsException(Of ArgumentException)(Sub() hc.Result.GetMembershipByClusterCount(0))
        Assert.ThrowsException(Of ArgumentException)(Sub() hc.Result.GetMembershipByClusterCount(7))
    End Sub

    <TestMethod>
    <TestCategory("Multivariate-Core-Clustering")>
    Public Sub RendererColorPalette_PreservesOleColorEncoding()
        Assert.AreEqual(0 Or (114 << 8) Or (178 << 16), ClusterAnalysisHelpers.GetClusterSeriesColor(0))
        Assert.AreEqual(213 Or (94 << 8), ClusterAnalysisHelpers.GetClusterSeriesColor(1))
        Assert.AreEqual(ClusterAnalysisHelpers.GetClusterSeriesColor(0),
                        ClusterAnalysisHelpers.GetClusterSeriesColor(8))
    End Sub

End Class
