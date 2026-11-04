Option Explicit On
Option Strict On
Imports System
Imports System.Collections.Generic
Imports System.Globalization
Imports System.Linq
Imports BESHStatNG.ExcelCommon
Imports BESHStatNG.Multivariate
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass>
Public Class Multivariate19AdapterTests
    <DataTestMethod><DataRow(0)><DataRow(1)><DataRow(2)><DataRow(3)><DataRow(4)><DataRow(5)><DataRow(6)>
    Public Sub FrozenHostImportedCentreValidationKeepsErrorPrecedenceAndOriginalScale(id As Integer)
        Dim raw(,) As Object = {{1.5, 2.0}, {10.0, 12.0}}
        Dim data = Multivariate18Fixture.Acquired(raw, {"second", "first"})
        Select Case id
            Case 1 : data.bZeroValid = True : data.nRows = 3 : data.nCols = 3
            Case 2 : data.nRows = 3 : data.nCols = 3
            Case 3 : data.nCols = 3
            Case 4 : data.FinalData(0, 0) = "not numeric"
            Case 5 : data.FinalData(0, 0) = Double.NaN
            Case 6 : data.FinalData(0, 0) = Double.PositiveInfinity
        End Select
        Dim centers(,) As Double = Nothing
        Dim failure = Multivariate19Fixture.Failure(Sub() centers = LegacyMultivariate19Centers.ValidateImported(data, 2, 2))
        Select Case id
            Case 1 : Assert.AreEqual("The user-specified starting-centers range does not contain any valid numeric observations.", failure.Message)
            Case 2 : Assert.AreEqual("The user-specified starting-centers range must contain exactly 2 row(s), one for each requested cluster.", failure.Message)
            Case 3 : Assert.AreEqual("The user-specified starting-centers range must contain exactly 2 column(s), one for each selected analysis variable.", failure.Message)
            Case 4 : Assert.IsInstanceOfType(failure, GetType(FormatException))
            Case Else
                Assert.IsNull(failure)
                Assert.AreEqual(10.0, centers(1, 0)) : Assert.AreEqual(12.0, centers(1, 1))
                Dim intent As New MultivariateStartingCentersIntent(centers.GetLength(0), centers.GetLength(1))
                Assert.AreEqual(2, intent.Shape.Rows) : Assert.AreEqual("HostImportedOriginalScale", intent.Source)
        End Select
    End Sub
    <DataTestMethod><DataRow(False, False)><DataRow(False, True)><DataRow(True, False)><DataRow(True, True)>
    Public Sub ActualClusterWriterUsesDetachedDataSectionsAndRetainsRawCellTypes(hierarchical As Boolean, labelled As Boolean)
        Dim saved = CultureInfo.CurrentCulture
        Try
            For Each culture In {"", "sk-SK"}
                CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(culture)
                Dim x = Multivariate19Fixture.Data(), raw = Multivariate19Fixture.Cells(x)
                raw(0, 0) = CInt(0) : raw(1, 1) = CDec(1) : raw(2, 1) = "0"
                Dim rows = New Integer() {101, 105, 118, 131, 151, 177}
                Dim data = Multivariate18Fixture.Acquired(raw, {"X", "μ"}, rows)
                Dim labels = If(labelled, New String() {"Row 101", "", "same", "same", "μ", "α"}, Nothing)
                Dim input = WindowsMultivariateWorkflowAdapter.Capture(If(hierarchical, MultivariateAnalysisKind.Hierarchical, MultivariateAnalysisKind.KMeans), data, labels, selectedKeys:={"second", "first"})
                Dim before As New Multivariate18Fixture.MemoryWriter, after As New Multivariate18Fixture.MemoryWriter
                before.write(MultivariateWorkflowPolicy.ClusterInputTable(raw, data.varNames, data.RowIds, labels))
                If hierarchical Then
                    Dim request As New MultivariateHierarchicalRequest(input, New MultivariateHierarchicalOptions)
                    WindowsMultivariateWorkflowAdapter.WriteClusterData(after, raw, request)
                    Assert.AreEqual(1, request.DataSection.HeaderRow) : Assert.AreEqual(2, request.DataSection.BodyRow)
                Else
                    Dim request As New MultivariateKMeansRequest(input, New MultivariateKMeansOptions)
                    WindowsMultivariateWorkflowAdapter.WriteClusterData(after, raw, request)
                    Assert.AreEqual("Data", request.DataSection.SheetName)
                End If
                Multivariate18Fixture.Blocks(before, after)
                CollectionAssert.AreEqual(rows, input.SourceRowIds)
                Dim repeated As New Multivariate18Fixture.MemoryWriter
                WindowsMultivariateWorkflowAdapter.WriteClusterData(repeated, raw, New MultivariateKMeansRequest(input, New MultivariateKMeansOptions))
                Multivariate18Fixture.Blocks(before, repeated)
                Assert.AreEqual("second", input.Columns(0).SourceKey)
            Next
        Finally
            CultureInfo.CurrentCulture = saved
        End Try
    End Sub

    <DataTestMethod><DataRow(0)><DataRow(3)><DataRow(15)><DataRow(27)><DataRow(39)><DataRow(51)><DataRow(63)><DataRow(72)><DataRow(86)><DataRow(91)><DataRow(93)><DataRow(102)><DataRow(105)>
    Public Sub CapturedWindowsInputFeedsSameCoreFitAndPortableOutcome(id As Integer)
        Multivariate19Fixture.KProfile(id, Function(input, options, centers)
                                              Dim data = Multivariate18Fixture.Acquired(Multivariate19Fixture.Cells(Multivariate19Fixture.Data(If(id = 72, 0, If(id = 93, 6, -1)))), input.VariableNames, input.SourceRowIds)
                                              Dim captured = WindowsMultivariateWorkflowAdapter.Capture(MultivariateAnalysisKind.KMeans, data, input.RowLabels, selectedKeys:=input.Columns.Select(Function(v) v.SourceKey).ToArray())
                                              Return New MultivariateKMeansRequest(captured, options, If(centers Is Nothing, Nothing, New MultivariateStartingCentersIntent(centers.GetLength(0), centers.GetLength(1))))
                                          End Function)
    End Sub

    <DataTestMethod><DataRow(0)><DataRow(1)><DataRow(2)><DataRow(3)><DataRow(4)><DataRow(5)><DataRow(6)>
    Public Sub WindowsCapturedHierarchicalAxesAndReportAreConsumedByWriter(linkage As Integer)
        Dim x = Multivariate19Fixture.Data(0), data = Multivariate18Fixture.Acquired(Multivariate19Fixture.Cells(x), {"x", "y"}, {101, 105, 118, 131, 151, 177})
        Dim input = WindowsMultivariateWorkflowAdapter.Capture(MultivariateAnalysisKind.Hierarchical, data)
        Dim options As New MultivariateHierarchicalOptions With {.Linkage = CType(linkage, HierarchicalLinkageMethod), .Missing = ClusterMissingValuePolicy.ListwiseDeletion, .Clusters = 2}
        Dim request As New MultivariateHierarchicalRequest(input, options), cut As New MultivariateClusterCutRequest(options.Cut, 2, 0), model As New HierarchicalClustering
        model.dataInputs(data.DataDbl, input.RowLabels, input.VariableNames) : request.Configure(model) : cut.Configure(model) : model.Fit()
        Dim projection = MultivariateClusteringProjectionPolicy.Hierarchical(request, cut, model.Result, model.wrapResults())
        Dim before As New Multivariate18Fixture.MemoryWriter, after As New Multivariate18Fixture.MemoryWriter
        Dim a As New ProcessListofResultTables(model.wrapResults()), b As New ProcessListofResultTables(projection.LegacyTables())
        a.writeToSheet(before, True) : b.writeToSheet(after, True) : Multivariate18Fixture.Blocks(before, after)
        Assert.AreEqual("Hierarchical results", projection.ResultsSheetName)
        Assert.AreEqual(105, projection.Observations(1).SourceRowId.Value) : Assert.AreEqual(1, projection.Observations(1).RemovedOrdinal.Value)
    End Sub
End Class


Public NotInheritable Class LegacyMultivariate19Centers
    Public Shared Function ValidateImported(centersData As DataObj, expectedClusters As Integer, expectedVariables As Integer) As Double(,)
        If centersData.bZeroValid Then
            Throw New ArgumentException("The user-specified starting-centers range does not contain any valid numeric observations.")
        End If

        If centersData.nRows <> expectedClusters Then
            Throw New ArgumentException(
                $"The user-specified starting-centers range must contain exactly {expectedClusters} row(s), one for each requested cluster.")
        End If

        If centersData.nCols <> expectedVariables Then
            Throw New ArgumentException(
                $"The user-specified starting-centers range must contain exactly {expectedVariables} column(s), one for each selected analysis variable.")
        End If

        Return centersData.DataDbl
    End Function
End Class
