Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports System
Imports System.IO
Imports System.Globalization
Imports System.Linq
Imports BESHStatNG
Imports BESHStatNG.Multivariate

<TestClass>
Public Class ClusterAnalysisTests

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

    Private Shared Function LoadClusterCsv(path As String) As (data As Double(,), rowLabels As String(), varNames As String())
        Dim lines = File.ReadAllLines(path)
        If lines.Length < 2 Then Throw New AssertFailedException($"CSV has no data rows: {path}")

        Dim header = lines(0).Split(","c).Select(Function(s) s.Trim()).ToArray()
        If header.Length < 4 Then Throw New AssertFailedException($"CSV must contain obs_id, row_label, and at least two variables: {path}")

        Dim varNames = header.Skip(2).ToArray()
        Dim n As Integer = lines.Length - 1
        Dim p As Integer = varNames.Length
        Dim rowLabels(n - 1) As String
        Dim data(n - 1, p - 1) As Double

        For i As Integer = 0 To n - 1
            Dim parts = lines(i + 1).Split(","c).Select(Function(s) s.Trim()).ToArray()
            If parts.Length <> p + 2 Then Throw New AssertFailedException($"RowUdfs {i + 2} has {parts.Length} columns; expected {p + 2}. File: {path}")

            rowLabels(i) = parts(1)
            For j As Integer = 0 To p - 1
                Dim token As String = parts(j + 2)
                If token = "" OrElse String.Equals(token, "NA", StringComparison.OrdinalIgnoreCase) Then
                    data(i, j) = Double.NaN
                Else
                    data(i, j) = Double.Parse(token, CultureInfo.InvariantCulture)
                End If
            Next
        Next

        Return (data, rowLabels, varNames)
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
        Dim loaded = LoadClusterCsv(GetTestDataPath("cluster_dataset_basic.csv"))

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
        AssertMatrixClose(New Double(,) {{2.0 / 3.0, 1.0 / 3.0}, {11.0, 32.0 / 3.0}}, res.CentersOriginalScale, 1.0E-9, 0.0, "Centers")
        AssertClose(12.0, res.TotalWithinClusterSS, 0.000000001, 0.0, "TotalWithinClusterSS")
        AssertClose(10.0 / 3.0, res.WithinClusterSSByCluster(0), 0.000000001, 0.0, "Cluster1 SSE")
        AssertClose(26.0 / 3.0, res.WithinClusterSSByCluster(1), 0.000000001, 0.0, "Cluster2 SSE")
        CollectionAssert.AreEqual(New Integer() {1, 2, 3, 4, 5, 6}, res.ActiveRowIndices)
        CollectionAssert.AreEqual(New String() {"A", "B", "C", "D", "E", "F"}, res.ActiveRowLabels)
        Assert.AreEqual(0, res.RemovedRowIndices.Length)
    End Sub

    <TestMethod>
    Public Sub KMeans_ListwiseDeletion_TracksRemovedRowsAndLabels()
        Dim loaded = LoadClusterCsv(GetTestDataPath("cluster_dataset_missing.csv"))

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
        Dim loaded = LoadClusterCsv(GetTestDataPath("cluster_dataset_basic.csv"))

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
        AssertClose(Math.Sqrt((0.1 - 2.0 / 3.0) ^ 2 + (0.2 - 1.0 / 3.0) ^ 2), distances(0), 1.0E-9)
        AssertClose(Math.Sqrt((11.8 - 11.0) ^ 2 + (10.3 - 32.0 / 3.0) ^ 2), distances(1), 1.0E-9)
    End Sub

    <TestMethod>
    Public Sub KMeans_WrapResults_ReturnsProjectStyleTables()
        Dim loaded = LoadClusterCsv(GetTestDataPath("cluster_dataset_basic.csv"))

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
        Dim loaded = LoadClusterCsv(GetTestDataPath("cluster_dataset_basic.csv"))

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
        AssertVectorClose(New Double() {1.0, 2.0, Math.Sqrt(5.0), Math.Sqrt(13.0), Math.Sqrt(269.0)}, res.MergeHeights, 1.0E-9, 0.0, "Merge heights")
        CollectionAssert.AreEqual(New Integer() {1, 1, 1, 2, 2, 2}, res.GetMembershipByClusterCount(2))
        CollectionAssert.AreEqual(New Integer() {1, 2, 3, 4, 5, 6}, res.LeafOrder)
    End Sub

    <TestMethod>
    Public Sub Hierarchical_ByHeight_ListwiseDeletion_TracksRemovedRows_AndWrapResults()
        Dim loaded = LoadClusterCsv(GetTestDataPath("cluster_dataset_missing.csv"))

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
        Dim loaded = LoadClusterCsv(GetTestDataPath("cluster_dataset_basic.csv"))

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
            AssertClose(layout.MaximumHeight, layout.LeafX(i), 1.0E-9, 0.0, "Leaves should lie on the x-axis distance baseline in Left orientation.")
        Next
    End Sub

End Class
