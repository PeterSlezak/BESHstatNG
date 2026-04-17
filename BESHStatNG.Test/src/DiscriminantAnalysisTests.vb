Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports BESHStatNG
Imports BESHStatNG.Multivariate

<TestClass>
Public Class DiscriminantAnalysisTests

    Private Shared Function BuildEuropeanDietReferenceDataset() As (data As Double(,), rowLabels As String(), groupLabels As Object(), varNames As String())
        Dim data(,) As Double = New Double(,) {
            {10.1, 1.4, 0.5, 8.9, 0.2, 42.3, 0.6, 5.5, 1.7},
            {8.9, 14.0, 4.3, 19.9, 2.1, 28.0, 3.6, 1.3, 4.3},
            {13.5, 9.3, 4.1, 17.5, 4.5, 26.6, 5.7, 2.1, 4.0},
            {7.8, 6.0, 1.6, 8.3, 1.2, 56.7, 1.1, 3.7, 4.2},
            {9.7, 11.4, 2.8, 12.5, 2.0, 34.3, 5.0, 1.1, 4.0},
            {10.6, 10.8, 3.7, 25.0, 9.9, 21.9, 4.8, 0.7, 2.4},
            {8.4, 11.6, 3.7, 11.1, 5.4, 24.6, 6.5, 0.8, 3.6},
            {9.5, 4.9, 2.7, 33.7, 5.8, 26.3, 5.1, 1.0, 1.4},
            {18.0, 9.9, 3.3, 19.5, 5.7, 28.1, 4.8, 2.4, 6.5},
            {10.2, 3.0, 2.8, 17.6, 5.9, 41.7, 2.2, 7.8, 6.5},
            {5.3, 12.4, 2.9, 9.7, 0.3, 40.1, 4.0, 5.4, 4.2},
            {13.9, 10.0, 4.7, 25.8, 2.2, 24.0, 6.2, 1.6, 2.9},
            {9.0, 5.1, 2.9, 13.7, 3.4, 36.8, 2.1, 4.3, 6.7},
            {9.5, 13.6, 3.6, 23.4, 2.5, 22.4, 4.2, 1.8, 3.7},
            {9.4, 4.7, 2.7, 23.3, 9.7, 23.0, 4.6, 1.6, 2.7},
            {6.9, 10.2, 2.7, 19.3, 3.0, 36.1, 5.9, 2.0, 6.6},
            {6.2, 3.7, 1.1, 4.9, 14.2, 27.0, 5.9, 4.7, 7.9},
            {6.2, 6.3, 1.5, 11.1, 1.0, 49.6, 3.1, 5.3, 2.8},
            {7.1, 3.4, 3.1, 8.6, 7.0, 29.2, 5.7, 5.9, 7.2},
            {9.9, 7.8, 3.5, 24.7, 7.5, 19.5, 3.7, 1.4, 2.0},
            {13.1, 10.1, 3.1, 23.8, 2.3, 25.6, 2.8, 2.4, 4.9},
            {17.4, 5.7, 4.7, 20.6, 4.3, 24.3, 4.7, 3.4, 3.3},
            {9.3, 4.6, 2.1, 16.6, 3.0, 43.6, 6.4, 3.4, 2.9},
            {11.4, 12.5, 4.1, 18.8, 3.4, 18.6, 5.2, 1.5, 3.8},
            {4.4, 5.0, 1.2, 9.5, 0.6, 55.9, 3.0, 5.7, 3.2}
        }
        Dim rowLabels() As String = New String() {"A", "A", "A", "A", "A", "A", "A", "A", "A", "A", "B", "B", "B", "B", "B", "B", "B", "B", "B", "B", "C", "C", "C", "D", "D"}
        Dim groups() As String = New String() {"A", "A", "A", "A", "A", "A", "A", "A", "A", "A", "B", "B", "B", "B", "B", "B", "B", "B", "B", "B", "C", "C", "C", "D", "D"}
        Dim varNames() As String = New String() {"RedMeat", "WhiteMeat", "Eggs", "Milk", "Fish", "Cereals", "Starch", "Nuts", "Fr&Veg"}
        Return (data, rowLabels, ToObjectArray(groups), varNames)
    End Function

    Private Shared Function BuildValidationDataset() As (data As Double(,), rowLabels As String(), groupLabels As Object(), varNames As String())
        Dim data(,) As Double = New Double(,) {
            {-3.0, 0.0},
            {-2.5, 0.8},
            {-1.8, -0.5},
            {-0.7, 0.4},
            {-2.2, 1.1},
            {-1.1, -0.9},
            {3.0, 5.0},
            {3.4, 6.2},
            {4.1, 4.6},
            {5.0, 5.8},
            {4.5, 7.0},
            {2.8, 6.5},
            {-5.5, 6.2},
            {-4.8, 7.8},
            {-6.1, 8.5},
            {-5.2, 9.1},
            {-6.4, 7.1},
            {-4.6, 8.7}
        }
        Dim rowLabels() As String = New String() {
            "A1", "A2", "A3", "A4", "A5", "A6",
            "B1", "B2", "B3", "B4", "B5", "B6",
            "C1", "C2", "C3", "C4", "C5", "C6"
        }
        Dim groups() As String = New String() {
            "A", "A", "A", "A", "A", "A",
            "B", "B", "B", "B", "B", "B",
            "C", "C", "C", "C", "C", "C"
        }
        Dim varNames() As String = New String() {"X1", "X2"}
        Return (data, rowLabels, ToObjectArray(groups), varNames)
    End Function

    Private Shared Function BuildMissingDataset() As (data As Double(,), rowLabels As String(), groupLabels As Object(), varNames As String())
        Dim data(,) As Double = New Double(,) {
            {0.0, 0.1},
            {Double.NaN, 0.4},
            {0.3, -0.2},
            {4.8, 5.1},
            {5.2, 4.9},
            {5.4, 5.3}
        }
        Dim rowLabels() As String = New String() {"A1", "A2", "A3", "B1", "B2", "B3"}
        Dim groups() As String = New String() {"A", "A", "A", "B", "B", "B"}
        Dim varNames() As String = New String() {"X1", "X2"}
        Return (data, rowLabels, ToObjectArray(groups), varNames)
    End Function

    Private Shared Function ToObjectArray(values() As String) As Object()
        Dim out(values.Length - 1) As Object
        For i As Integer = 0 To values.Length - 1
            out(i) = values(i)
        Next
        Return out
    End Function

    Private Shared Function BuildAnalysis(loaded As (data As Double(,), rowLabels As String(), groupLabels As Object(), varNames As String()),
                                          Optional method As DiscriminantAnalysisMethod = DiscriminantAnalysisMethod.Linear,
                                          Optional standardization As ClusterStandardizationMode = ClusterStandardizationMode.None,
                                          Optional missingPolicy As ClusterMissingValuePolicy = ClusterMissingValuePolicy.ErrorOnMissing,
                                          Optional priorMode As DiscriminantPriorMode = DiscriminantPriorMode.ProportionalToGroupSizes,
                                          Optional covarianceRegularization As Double = 1.0E-8) As DiscriminantAnalysis
        Dim da As New DiscriminantAnalysis()
        da.dataInputs(loaded.data, loaded.groupLabels, loaded.rowLabels, loaded.varNames)
        da.settingsInputs(method:=method,
                          standardization:=standardization,
                          missingPolicy:=missingPolicy,
                          priorMode:=priorMode,
                          covarianceRegularization:=covarianceRegularization)
        Return da
    End Function

    Private Shared Sub AssertClose(expected As Double, actual As Double, absTol As Double, Optional relTol As Double = 0.0, Optional msg As String = "")
        Dim diff As Double = Math.Abs(expected - actual)
        Dim ok As Boolean = diff <= absTol
        If Not ok AndAlso relTol > 0 Then
            Dim denom As Double = Math.Max(Math.Abs(expected), Math.Abs(actual))
            If denom > 0 Then ok = (diff / denom) <= relTol
        End If
        If Not ok Then
            Assert.Fail(String.Format("{0} Expected {1:R}, got {2:R}, diff={3:R}", msg, expected, actual, diff))
        End If
    End Sub

    Private Shared Sub AssertVectorClose(expected() As Double, actual() As Double, absTol As Double, Optional relTol As Double = 0.0, Optional msg As String = "")
        Assert.AreEqual(expected.Length, actual.Length, msg & " Length mismatch")
        For i As Integer = 0 To expected.Length - 1
            AssertClose(expected(i), actual(i), absTol, relTol, msg & " [i=" & i & "]")
        Next
    End Sub

    Private Shared Sub AssertMatrixClose(expected(,) As Double, actual(,) As Double, absTol As Double, Optional relTol As Double = 0.0, Optional msg As String = "")
        Assert.AreEqual(expected.GetLength(0), actual.GetLength(0), msg & " Row count mismatch")
        Assert.AreEqual(expected.GetLength(1), actual.GetLength(1), msg & " Column count mismatch")
        For i As Integer = 0 To expected.GetLength(0) - 1
            For j As Integer = 0 To expected.GetLength(1) - 1
                AssertClose(expected(i, j), actual(i, j), absTol, relTol, msg & " [i=" & i & ",j=" & j & "]")
            Next
        Next
    End Sub

    Private Shared Sub AssertFiniteMatrix(values(,) As Double, Optional msg As String = "")
        Assert.IsNotNull(values, msg & " Matrix should not be null.")
        For i As Integer = 0 To values.GetLength(0) - 1
            For j As Integer = 0 To values.GetLength(1) - 1
                If Double.IsNaN(values(i, j)) OrElse Double.IsInfinity(values(i, j)) Then
                    Assert.Fail(msg & " Non-finite value at [" & i & "," & j & "]: " & values(i, j).ToString())
                End If
            Next
        Next
    End Sub

    Private Shared Function GetTitleText(tbl As ResultTable) As String
        Dim m(,) As Object = tbl.returnSelf()
        If m Is Nothing Then Return String.Empty
        If m.GetLength(0) = 0 OrElse m.GetLength(1) = 0 Then Return String.Empty
        Return If(m(0, 0), String.Empty).ToString()
    End Function

    Private Shared Function CountLabels(labels() As String) As Dictionary(Of String, Integer)
        Dim out As New Dictionary(Of String, Integer)(StringComparer.Ordinal)
        For Each lbl As String In labels
            If Not out.ContainsKey(lbl) Then out(lbl) = 0
            out(lbl) += 1
        Next
        Return out
    End Function

    <TestMethod>
    Public Sub LinearDiscriminant_EuropeanDiet_ReplicatesReferenceConfusionAndCanonicalRoots()
        Dim loaded = BuildEuropeanDietReferenceDataset()
        Dim da = BuildAnalysis(loaded,
                               method:=DiscriminantAnalysisMethod.Linear,
                               standardization:=ClusterStandardizationMode.None,
                               missingPolicy:=ClusterMissingValuePolicy.ErrorOnMissing,
                               priorMode:=DiscriminantPriorMode.ProportionalToGroupSizes,
                               covarianceRegularization:=1.0E-8)

        da.Fit()

        CollectionAssert.AreEqual(New String() {"A", "B", "C", "D"}, da.GroupLabels)

        Assert.AreEqual(4, da.GroupStatistics.Count)
        Assert.AreEqual(10, da.GroupStatistics(0).Count)
        Assert.AreEqual(10, da.GroupStatistics(1).Count)
        Assert.AreEqual(3, da.GroupStatistics(2).Count)
        Assert.AreEqual(2, da.GroupStatistics(3).Count)
        AssertClose(0.4, da.GroupStatistics(0).PriorProbability, 1.0E-12)
        AssertClose(0.4, da.GroupStatistics(1).PriorProbability, 1.0E-12)
        AssertClose(0.12, da.GroupStatistics(2).PriorProbability, 1.0E-12)
        AssertClose(0.08, da.GroupStatistics(3).PriorProbability, 1.0E-12)

        Dim expectedConfusion(,) As Double = New Double(,) {
            {7.0, 3.0, 0.0, 0.0},
            {1.0, 8.0, 1.0, 0.0},
            {1.0, 0.0, 2.0, 0.0},
            {0.0, 2.0, 0.0, 0.0}
        }
        AssertMatrixClose(expectedConfusion, da.TrainingClassification.Confusion.Counts, 1.0E-12, 0.0, "Training confusion")
        AssertClose(0.68, da.TrainingClassification.Confusion.OverallAccuracy, 1.0E-12)
        AssertClose(68.0, da.TrainingClassification.Confusion.OverallAccuracyPct, 1.0E-10)

        Dim expectedEig() As Double = New Double() {0.660129298, 0.237576878, 0.086472121}
        AssertVectorClose(expectedEig, da.CanonicalEigenvalues, 1.0E-6, 0.0, "Canonical eigenvalues")

        Dim expectedRecall() As Double = New Double() {70.0, 80.0, 66.666666667, 0.0}
        AssertVectorClose(expectedRecall, da.TrainingClassification.Confusion.RecallPct, 1.0E-6, 0.0, "Recall %")

        AssertFiniteMatrix(da.TrainingClassification.PosteriorProbabilities, "Posterior probabilities")
        AssertFiniteMatrix(da.TrainingClassification.SquaredDistances, "Squared distances")
        For i As Integer = 0 To da.TrainingClassification.PosteriorProbabilities.GetLength(0) - 1
            Dim s As Double = 0.0
            For j As Integer = 0 To da.TrainingClassification.PosteriorProbabilities.GetLength(1) - 1
                s += da.TrainingClassification.PosteriorProbabilities(i, j)
            Next
            AssertClose(1.0, s, 1.0E-9, 0.0, "Posterior probabilities should sum to 1 for row " & (i + 1).ToString())
        Next
    End Sub

    <TestMethod>
    Public Sub LinearDiscriminant_WrapResults_ReturnsProjectStyleTables()
        Dim loaded = BuildEuropeanDietReferenceDataset()
        Dim da = BuildAnalysis(loaded,
                               method:=DiscriminantAnalysisMethod.Linear,
                               standardization:=ClusterStandardizationMode.None,
                               missingPolicy:=ClusterMissingValuePolicy.ErrorOnMissing,
                               priorMode:=DiscriminantPriorMode.ProportionalToGroupSizes,
                               covarianceRegularization:=1.0E-8)

        da.Fit()

        Dim tables = da.wrapResults()
        Assert.AreEqual(10, tables.Count, "Expected settings, summaries, canonical tables, and training classification tables.")
        Assert.AreEqual("Discriminant Analysis Settings", GetTitleText(tables(0)))
        Assert.AreEqual("Group Summary", GetTitleText(tables(1)))
        Assert.AreEqual("Group Means (Original Scale)", GetTitleText(tables(2)))
        Assert.AreEqual("Pooled Covariance Matrix (Working Scale)", GetTitleText(tables(3)))
        Assert.AreEqual("Linear Classification Functions (Original Input Scale)", GetTitleText(tables(4)))
        Assert.AreEqual("Canonical Discriminant Functions Summary", GetTitleText(tables(5)))
        Assert.AreEqual("Canonical Coefficients (Working Scale)", GetTitleText(tables(6)))
        Assert.AreEqual("Group Centroids in Canonical Space", GetTitleText(tables(7)))
        Assert.AreEqual("Training Classification Matrix (Resubstitution)", GetTitleText(tables(8)))
        Assert.AreEqual("Training Casewise Classification", GetTitleText(tables(9)))
    End Sub

    <TestMethod>
    Public Sub DiscriminantAnalysis_ErrorOnMissing_ThrowsOnIncompleteRow()
        Dim loaded = BuildMissingDataset()
        Dim da = BuildAnalysis(loaded,
                               method:=DiscriminantAnalysisMethod.Linear,
                               missingPolicy:=ClusterMissingValuePolicy.ErrorOnMissing)

        Dim ex = Assert.ThrowsException(Of ArgumentException)(Sub() da.Fit())
        StringAssert.Contains(ex.Message, "row 2")
    End Sub

    <TestMethod>
    Public Sub DiscriminantAnalysis_ListwiseDeletion_RemovesIncompleteRowsAndTracksLabels()
        Dim loaded = BuildMissingDataset()
        Dim da = BuildAnalysis(loaded,
                               method:=DiscriminantAnalysisMethod.Linear,
                               missingPolicy:=ClusterMissingValuePolicy.ListwiseDeletion)

        da.Fit()

        CollectionAssert.AreEqual(New Integer() {2}, da.PreparedData.RemovedOriginalIndices)
        CollectionAssert.AreEqual(New String() {"A2"}, da.PreparedData.RemovedRowLabels)
        Assert.AreEqual(5, da.PreparedData.ActiveOriginalData.GetLength(0))
        CollectionAssert.AreEqual(New String() {"A1", "A3", "B1", "B2", "B3"}, da.PreparedData.ActiveRowLabels)
        AssertClose(1.0, da.TrainingClassification.Confusion.OverallAccuracy, 1.0E-12)
    End Sub

    <TestMethod>
    Public Sub DiscriminantAnalysis_UserSpecifiedPriors_AreNormalizedAndApplied()
        Dim loaded = BuildValidationDataset()
        Dim da = BuildAnalysis(loaded,
                               method:=DiscriminantAnalysisMethod.Linear,
                               priorMode:=DiscriminantPriorMode.UserSpecified)

        da.priorInputs(New Object() {"A", "B", "C"}, New Double() {2.0, 1.0, 1.0})
        da.Fit()

        Assert.AreEqual(3, da.GroupStatistics.Count)
        AssertClose(0.5, da.GroupStatistics(0).PriorProbability, 1.0E-12)
        AssertClose(0.25, da.GroupStatistics(1).PriorProbability, 1.0E-12)
        AssertClose(0.25, da.GroupStatistics(2).PriorProbability, 1.0E-12)
    End Sub

    <TestMethod>
    Public Sub QuadraticDiscriminant_PredictsGroupMeans_AndProducesQuadraticTables()
        Dim loaded = BuildValidationDataset()
        Dim da = BuildAnalysis(loaded,
                               method:=DiscriminantAnalysisMethod.Quadratic,
                               covarianceRegularization:=1.0E-8)

        da.Fit()

        Assert.IsTrue(da.CanonicalEigenvalues Is Nothing, "QDA should not produce linear canonical roots in the current implementation.")
        Assert.AreEqual(3, da.GroupStatistics.Count)

        For Each gs As DiscriminantGroupStatistics In da.GroupStatistics
            Dim oneRow(0, gs.MeanOriginal.Length - 1) As Double
            For j As Integer = 0 To gs.MeanOriginal.Length - 1
                oneRow(0, j) = gs.MeanOriginal(j)
            Next
            Dim pred() As String = da.Predict(oneRow, New String() {"centroid"})
            Assert.AreEqual(gs.GroupLabel, pred(0), "Group mean should classify back to its own group.")
        Next

        Dim tables = da.wrapResults()
        Assert.AreEqual(8, tables.Count, "Expected settings, summaries, one covariance table per group, and training classification tables.")
        Assert.AreEqual("Discriminant Analysis Settings", GetTitleText(tables(0)))
        Assert.AreEqual("Group Summary", GetTitleText(tables(1)))
        Assert.AreEqual("Group Means (Original Scale)", GetTitleText(tables(2)))
        Assert.AreEqual("Within-Group Covariance Matrix [A] (Working Scale)", GetTitleText(tables(3)))
        Assert.AreEqual("Within-Group Covariance Matrix [B] (Working Scale)", GetTitleText(tables(4)))
        Assert.AreEqual("Within-Group Covariance Matrix [C] (Working Scale)", GetTitleText(tables(5)))
        Assert.AreEqual("Training Classification Matrix (Resubstitution)", GetTitleText(tables(6)))
        Assert.AreEqual("Training Casewise Classification", GetTitleText(tables(7)))
    End Sub

    <TestMethod>
    Public Sub Validation_KFold_WithFixedSeed_IsReproducible()
        Dim loaded = BuildValidationDataset()

        Dim da1 = BuildAnalysis(loaded, method:=DiscriminantAnalysisMethod.Linear)
        da1.validationInputs(mode:=DiscriminantValidationMode.KFold,
                             numberOfFolds:=3,
                             holdoutFraction:=0.3,
                             randomSeed:=12345,
                             stratified:=True)
        da1.Fit()

        Dim da2 = BuildAnalysis(loaded, method:=DiscriminantAnalysisMethod.Linear)
        da2.validationInputs(mode:=DiscriminantValidationMode.KFold,
                             numberOfFolds:=3,
                             holdoutFraction:=0.3,
                             randomSeed:=12345,
                             stratified:=True)
        da2.Fit()

        Assert.IsNotNull(da1.ValidationClassification)
        Assert.AreEqual(DiscriminantValidationMode.KFold, da1.ValidationClassification.ValidationMode)
        Assert.AreEqual(loaded.rowLabels.Length, da1.ValidationClassification.PredictedGroupLabels.Length)
        CollectionAssert.AreEqual(da1.ValidationClassification.FoldAssignments, da2.ValidationClassification.FoldAssignments)
        CollectionAssert.AreEqual(da1.ValidationClassification.PredictedGroupLabels, da2.ValidationClassification.PredictedGroupLabels)
        AssertClose(1.0, da1.ValidationClassification.Confusion.OverallAccuracy, 1.0E-12)
    End Sub

    <TestMethod>
    Public Sub Validation_LeaveOneOut_ProducesFullLengthCasewiseResults()
        Dim loaded = BuildValidationDataset()
        Dim da = BuildAnalysis(loaded, method:=DiscriminantAnalysisMethod.Linear)
        da.validationInputs(mode:=DiscriminantValidationMode.LeaveOneOut,
                            numberOfFolds:=5,
                            holdoutFraction:=0.3,
                            randomSeed:=2468,
                            stratified:=True)

        da.Fit()

        Assert.IsNotNull(da.ValidationClassification)
        Assert.AreEqual(DiscriminantValidationMode.LeaveOneOut, da.ValidationClassification.ValidationMode)
        Assert.AreEqual(loaded.rowLabels.Length, da.ValidationClassification.RowIndices.Length)
        Assert.AreEqual(loaded.rowLabels.Length, da.ValidationClassification.PredictedGroupLabels.Length)
        CollectionAssert.AreEqual(Enumerable.Range(1, loaded.rowLabels.Length).ToArray(), da.ValidationClassification.FoldAssignments)
        AssertClose(1.0, da.ValidationClassification.Confusion.OverallAccuracy, 1.0E-12)
    End Sub

    <TestMethod>
    Public Sub Validation_Holdout_WithFixedSeed_IsReproducible_AndStratified()
        Dim loaded = BuildValidationDataset()

        Dim da1 = BuildAnalysis(loaded, method:=DiscriminantAnalysisMethod.Linear)
        da1.validationInputs(mode:=DiscriminantValidationMode.Holdout,
                             numberOfFolds:=5,
                             holdoutFraction:=0.33,
                             randomSeed:=54321,
                             stratified:=True)
        da1.Fit()

        Dim da2 = BuildAnalysis(loaded, method:=DiscriminantAnalysisMethod.Linear)
        da2.validationInputs(mode:=DiscriminantValidationMode.Holdout,
                             numberOfFolds:=5,
                             holdoutFraction:=0.33,
                             randomSeed:=54321,
                             stratified:=True)
        da2.Fit()

        Assert.IsNotNull(da1.ValidationClassification)
        Assert.AreEqual(DiscriminantValidationMode.Holdout, da1.ValidationClassification.ValidationMode)
        Assert.AreEqual(6, da1.ValidationClassification.PredictedGroupLabels.Length, "Stratified holdout with 6 rows per group and fraction 0.33 should allocate 2 test rows per group.")
        CollectionAssert.AreEqual(da1.ValidationClassification.RowIndices, da2.ValidationClassification.RowIndices)
        CollectionAssert.AreEqual(da1.ValidationClassification.PredictedGroupLabels, da2.ValidationClassification.PredictedGroupLabels)
        CollectionAssert.AreEqual(Enumerable.Repeat(1, 6).ToArray(), da1.ValidationClassification.FoldAssignments)

        Dim counts = CountLabels(da1.ValidationClassification.ActualGroupLabels)
        Assert.AreEqual(2, counts("A"))
        Assert.AreEqual(2, counts("B"))
        Assert.AreEqual(2, counts("C"))
        AssertClose(1.0, da1.ValidationClassification.Confusion.OverallAccuracy, 1.0E-12)
    End Sub

End Class
