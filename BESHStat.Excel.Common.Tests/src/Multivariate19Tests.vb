Option Explicit On
Option Strict On
Imports System
Imports System.Collections.Generic
Imports System.Globalization
Imports System.Linq
Imports System.Text
Imports System.Xml.Linq
Imports BESHStatNG
Imports BESHStatNG.ExcelCommon
Imports BESHStatNG.Multivariate
Imports Microsoft.VisualStudio.TestTools.UnitTesting

Public NotInheritable Class Multivariate19Fixture
    Public Shared Function Data(Optional edge As Integer = -1) As Double(,)
        Dim x(,) As Double = {{0, 0}, {0, 1}, {2, 0}, {10, 10}, {10, 12}, {13, 10}}
        Select Case edge
            Case 0, 1, 2 : x(1, 0) = {Double.NaN, Double.PositiveInfinity, Double.NegativeInfinity}(edge)
            Case 3 : For i = 0 To 5 : x(i, 0) = Double.NaN : Next
            Case 4 : Return New Double(,) {{1, 1}}
            Case 5 : Return New Double(,) {{1, 1}, {2, 2}}
            Case 6 : Return New Double(,) {{0, 0}, {0, 0}, {0, 0}, {0, 0}}
            Case 7 : For i = 0 To 5 : x(i, 1) = 1 : Next
            Case 8 : For i = 0 To 5 : x(i, 1) = 1 + i * 0.00000000000001 : Next
            Case 9 : Return New Double(,) {{0}, {1}, {2}, {3}}
            Case 10 : Return New Double(-1, 1) {}
            Case 11 : Return New Double(2, -1) {}
            Case 12 : Return New Double(,) {{1, 1}, {1, 1}, {2, 2}, {3, 3}, {4, 4}, {4, 4}}
        End Select
        Return x
    End Function
    Public Shared Function Input(kind As MultivariateAnalysisKind, x(,) As Double, labelMode As Integer) As MultivariateInputIntent
        Dim n = x.GetLength(0), p = x.GetLength(1)
        Dim rows = Enumerable.Range(0, n).Select(Function(i) 101 + i * 7).Reverse().ToArray()
        Dim labels = If(labelMode = 0, Nothing, Enumerable.Range(0, n).Select(Function(i) MultivariateWorkflowPolicy.ObservationLabel(If(i Mod 3 = 0, " ", "重复 α"), rows(i))).ToArray())
        Return New MultivariateInputIntent(kind, n, p, Enumerable.Range(0, p).Select(Function(i) "X&μ").ToArray(), rows, labels,
            sourceColumnKeys:=Enumerable.Range(0, p).Select(Function(i) "selected-" & (p - i)).ToArray())
    End Function
    Public Shared Function Cells(x(,) As Double) As Object(,)
        Dim result(x.GetLength(0) - 1, x.GetLength(1) - 1) As Object
        For i = 0 To x.GetLength(0) - 1
            For j = 0 To x.GetLength(1) - 1 : result(i, j) = x(i, j) : Next
        Next
        Return result
    End Function
    Public Shared Function KOptions(id As Integer) As MultivariateKMeansOptions
        Dim o As New MultivariateKMeansOptions With {.Clusters = If(id Mod 4 = 0, 1, If(id Mod 4 = 1, 2, If(id Mod 4 = 2, 3, 6))),
            .Initialization = CType(id Mod 4, KMeansInitializationMethod), .Standardization = CType((id \ 4) Mod 3, ClusterStandardizationMode),
            .Distance = CType((id \ 12) Mod 2, KMeansDistanceMetric), .EmptyCluster = CType((id \ 24) Mod 3, EmptyClusterHandlingStrategy),
            .Starts = If(id Mod 2 = 0, 1, 7), .Seed = If(id Mod 2 = 0, 0, 123), .Missing = ClusterMissingValuePolicy.ListwiseDeletion}
        If id >= 72 Then
            o.Clusters = 2 : o.Initialization = KMeansInitializationMethod.UserSpecifiedCenters
            Select Case id
                Case 72 To 85 : o.Clusters = If(id = 76, 1, 2)
                Case 86 : o.Clusters = 7
                Case 87 : o.Clusters = 0
                Case 88 : o.Starts = 0
                Case 89 : o.MaximumIterations = 0
                Case 90 : o.Tolerance = -1
                Case 91 : o.MaximumIterations = 1 : o.Tolerance = 0 : o.Initialization = KMeansInitializationMethod.RandomPartition
                Case 92 : o.Tolerance = 100
                Case 93 To 95 : o.EmptyCluster = CType(id - 93, EmptyClusterHandlingStrategy) : o.Clusters = 4
                Case 96 To 98 : o.Missing = ClusterMissingValuePolicy.ErrorOnMissing
                Case 99 To 101 : o.Standardization = CType(id - 99, ClusterStandardizationMode)
            End Select
        End If
        Return o
    End Function
    Public Shared Function StartingCenters(x(,) As Double, o As MultivariateKMeansOptions, id As Integer) As Double(,)
        If o.Initialization <> KMeansInitializationMethod.UserSpecifiedCenters Then Return Nothing
        Dim r = o.Clusters, p = x.GetLength(1)
        If id = 102 Then r += 1
        If id = 103 Then p += 1
        If id = 104 Then Return New Double(-1, p - 1) {}
        Dim centers(Math.Max(0, r) - 1, p - 1) As Double
        For i = 0 To centers.GetLength(0) - 1
            For j = 0 To p - 1 : centers(i, j) = If(id >= 93 AndAlso id <= 95, 0, i * 10) : Next
        Next
        If id = 105 Then centers(0, 0) = Double.NaN
        If id = 106 Then centers(0, 0) = Double.PositiveInfinity
        If id = 107 Then centers(0, 0) = Double.NegativeInfinity
        Return centers
    End Function
    Public Shared Function HOptions(id As Integer) As MultivariateHierarchicalOptions
        Dim o As New MultivariateHierarchicalOptions With {.Linkage = CType(id Mod 7, HierarchicalLinkageMethod),
            .Distance = CType((id \ 7) Mod 7, HierarchicalDistanceMetric), .Standardization = CType((id \ 49) Mod 3, ClusterStandardizationMode),
            .Missing = ClusterMissingValuePolicy.ListwiseDeletion, .Clusters = 2,
            .Cut = If(id Mod 2 = 0, HierarchicalMembershipDisplayMode.ByClusterCount, HierarchicalMembershipDisplayMode.ByHeight), .CutHeight = 3.5,
            .MinkowskiPower = {1.0, 2.0, 1.5}(id Mod 3)}
        If id >= 147 Then
            o.Linkage = HierarchicalLinkageMethod.Complete : o.Distance = HierarchicalDistanceMetric.Euclidean
            Select Case id
                Case 151 : o.Clusters = 1
                Case 161 : o.Clusters = 0
                Case 162 : o.Clusters = 7 : o.Cut = HierarchicalMembershipDisplayMode.ByClusterCount
                Case 163 To 167 : o.Distance = HierarchicalDistanceMetric.Minkowski : o.MinkowskiPower = {0.0, -1.0, Double.NaN, Double.PositiveInfinity, Double.NegativeInfinity}(id - 163)
                Case 168 To 170 : o.Missing = ClusterMissingValuePolicy.ErrorOnMissing
                Case 171 : o.Cut = HierarchicalMembershipDisplayMode.ByHeight : o.CutHeight = Double.NaN
                Case 172 : o.Cut = HierarchicalMembershipDisplayMode.ByHeight : o.CutHeight = Double.PositiveInfinity
            End Select
        End If
        Return o
    End Function
    Public Shared Function Failure(action As Action) As Exception
        Try
            action() : Return Nothing
        Catch ex As Exception
            Return ex
        End Try
    End Function
    Public Shared Sub SameFailure(a As Exception, b As Exception)
        Assert.AreEqual(If(a Is Nothing, Nothing, a.GetType().FullName), If(b Is Nothing, Nothing, b.GetType().FullName))
        Assert.AreEqual(If(a Is Nothing, Nothing, a.Message), If(b Is Nothing, Nothing, b.Message))
    End Sub
    Public Shared Sub Record(family As String, id As Integer, input As MultivariateInputIntent, x(,) As Double,
                             options As Object, projection As MultivariateClusteringProjection, failure As Exception,
                             numerical As String, Optional centers(,) As Double = Nothing,
                             Optional fitAvailable As Boolean = False, Optional phase As String = "Completed",
                             Optional fittedK As KMeansClusterResult = Nothing, Optional fittedH As HierarchicalClusterResult = Nothing)
        Dim optionValues = options.GetType().GetProperties().Select(Function(p) New XElement(p.Name, Convert.ToString(p.GetValue(options, Nothing), CultureInfo.InvariantCulture)))
        Dim root As New XElement("profile", New XAttribute("family", family), New XAttribute("fixture", id),
            New XAttribute("status", "NOT_GOLDEN"), New XAttribute("classification", If(failure IsNot Nothing, If(fitAvailable, "EXISTING_CORE_REPORT_REJECTED", "EXISTING_CORE_REJECTED"), If(projection.Converged.HasValue AndAlso Not projection.Converged.Value, "CORE_NONCONVERGED", "CORE_RETURNED"))),
            New XAttribute("phase", phase), New XAttribute("fit_available", fitAvailable OrElse projection IsNot Nothing),
            New XAttribute("culture", CultureInfo.CurrentCulture.Name), New XAttribute("runtime", Environment.Version.ToString()),
            New XAttribute("fixture_sha256", Multivariate18Fixture.Digest(x, input.SourceRowIds, input.VariableNames, input.RowLabels)),
            New XElement("options", optionValues), New XElement("source_rows", input.SourceRowIds.Select(Function(v) New XElement("row", v))),
            New XElement("names", input.VariableNames.Select(Function(v) New XElement("name", v))),
            New XElement("centres", New XAttribute("source", If(centers Is Nothing, "NotSelected", "HostImportedOriginalScale")),
                New XAttribute("rows", If(centers Is Nothing, 0, centers.GetLength(0))), New XAttribute("columns", If(centers Is Nothing, 0, centers.GetLength(1))),
                New XAttribute("sha256", Multivariate18Fixture.Digest(centers))), New XElement("numerical_sha256", numerical))
        If failure IsNot Nothing Then root.Add(New XElement("exception", New XAttribute("type", failure.GetType().FullName), failure.Message))
        If failure IsNot Nothing Then
            root.Add(New XElement("result_availability", If(fitAvailable, "FittedButReportRejected", "UnavailableAtRejection")))
            If fittedK IsNot Nothing Then
                root.Add(New XElement("active_input_ordinals", fittedK.ActiveRowIndices.Select(Function(v) New XElement("ordinal", v))),
                    New XElement("removed_input_ordinals", fittedK.RemovedRowIndices.Select(Function(v) New XElement("ordinal", v))),
                    New XElement("active_labels", fittedK.ActiveRowLabels.Select(Function(v) New XElement("label", v))),
                    New XElement("effective_seed", fittedK.RandomSeedUsed), New XElement("converged", fittedK.Converged),
                    New XElement("fitted_result_sha256", Multivariate18Fixture.Digest(fittedK.ClusterAssignments, fittedK.CentersOriginalScale, fittedK.StandardizationLocations, fittedK.StandardizationScales)))
            End If
            If fittedH IsNot Nothing Then
                root.Add(New XElement("active_input_ordinals", fittedH.ActiveRowIndices.Select(Function(v) New XElement("ordinal", v))),
                    New XElement("removed_input_ordinals", fittedH.RemovedRowIndices.Select(Function(v) New XElement("ordinal", v))),
                    New XElement("active_labels", fittedH.ActiveRowLabels.Select(Function(v) New XElement("label", v))),
                    New XElement("leaf_order", fittedH.LeafOrder.Select(Function(v) New XElement("ordinal", v))),
                    New XElement("fitted_result_sha256", Multivariate18Fixture.Digest(fittedH.MergeLeftClusterIds, fittedH.MergeRightClusterIds, fittedH.MergeHeights, fittedH.MergeClusterSizes, fittedH.StandardizationLocations, fittedH.StandardizationScales)))
            End If
        End If
        If projection IsNot Nothing Then
            root.Add(New XElement("table_sha256", Multivariate18Fixture.Digest(projection.LegacyTables().Select(Function(t) CType(t.ToOutputModel().Values, Array)).ToArray())),
                New XElement("plot_sequence", "None"))
            If projection.CenterShape IsNot Nothing Then
                root.Add(New XElement("centre_axes", New XAttribute("requested_clusters", projection.RequestedClusterCount.Value),
                    New XAttribute("original_rows", projection.CenterShape.Rows), New XAttribute("original_columns", projection.CenterShape.Columns),
                    New XAttribute("working_rows", projection.WorkingCenterShape.Rows), New XAttribute("working_columns", projection.WorkingCenterShape.Columns)))
            End If
            root.Add(New XElement("effective_seed", projection.EffectiveSeed), New XElement("starts_evaluated", projection.StartsEvaluated), New XElement("converged", projection.Converged), New XElement("iterations", projection.Iterations),
                New XElement("observations", projection.Observations.Select(Function(r) New XElement("row", New XAttribute("input", r.InputOrdinal),
                    New XElement("source", r.SourceRowId), New XElement("active", r.ActiveOrdinal), New XElement("removed", r.RemovedOrdinal),
                    New XElement("label", r.DisplayLabel), New XElement("membership", r.Membership), New XElement("leaf", r.LeafId), New XElement("leaf_position", r.LeafPosition)))),
                New XElement("merges", projection.Merges.Select(Function(m) New XElement("merge", New XAttribute("step", m.StepNumber), New XAttribute("id", m.InternalId),
                    New XAttribute("left", m.LeftId), New XAttribute("right", m.RightId), New XAttribute("size", m.Size), New XAttribute("height", m.Height.ToString("R", CultureInfo.InvariantCulture))))),
                New XElement("sections", projection.Sections.Select(Function(s) New XElement("section", New XAttribute("index", s.Index), New XAttribute("rows", s.Shape.Rows), New XAttribute("columns", s.Shape.Columns), New XAttribute("header_depth", s.HeaderDepth), New XAttribute("footer_depth", s.FooterDepth), s.Title))),
                New XElement("variables", projection.Variables.Select(Function(v) New XElement("variable", New XAttribute("index", v.ResultIndex), New XAttribute("source_key", v.SourceKey), v.Name))))
        End If
        Console.WriteLine("04A19_XML|" & Convert.ToBase64String(Encoding.UTF8.GetBytes(root.ToString(SaveOptions.DisableFormatting))))
    End Sub
    Public Shared Function KProfile(id As Integer, Optional host As Func(Of MultivariateInputIntent, MultivariateKMeansOptions, Double(,), MultivariateKMeansRequest) = Nothing) As MultivariateClusteringProjection
        Dim edge = If(id >= 72 AndAlso id <= 85, id - 72, If(id >= 93 AndAlso id <= 95, 6, If(id >= 96 AndAlso id <= 98, id - 96, If(id >= 99 AndAlso id <= 101, 8, -1))))
        Dim x = Data(edge), input = Multivariate19Fixture.Input(MultivariateAnalysisKind.KMeans, x, id Mod 2), o = KOptions(id), centers = Multivariate19Fixture.StartingCenters(x, o, id)
        Dim a As New KMeans, b As New KMeans
        Dim request = If(host Is Nothing, New MultivariateKMeansRequest(input, o, If(centers Is Nothing, Nothing, New MultivariateStartingCentersIntent(centers.GetLength(0), centers.GetLength(1)))), host(input, o, centers))
        Dim ea = Failure(Sub()
                             a.dataInputs(x, input.RowLabels, input.VariableNames)
                             If centers IsNot Nothing Then a.startingCentersInputs(centers)
                             a.settingsInputs(o.Clusters, o.Initialization, o.Distance, o.Starts, o.MaximumIterations, o.Tolerance, o.Standardization, o.Missing, o.EmptyCluster, o.Seed)
                             a.Fit()
                             a.wrapResults()
                         End Sub)
        Dim phase As String = "Data"
        Dim eb = Failure(Sub()
                             b.dataInputs(x, input.RowLabels, input.VariableNames)
                             phase = "StartingCenters"
                             If centers IsNot Nothing Then b.startingCentersInputs(centers)
                             phase = "Settings" : request.Configure(b)
                             phase = "Fit" : b.Fit()
                             phase = "Report" : b.wrapResults()
                         End Sub)
        SameFailure(ea, eb)
        If id < 72 Then Assert.IsNull(eb)
        If eb IsNot Nothing Then
            Record("KMeans", id, input, x, o, Nothing, eb, "", centers, b.Result IsNot Nothing, phase, fittedK:=b.Result) : Return Nothing
        End If
        Dim r = b.Result, digest = Multivariate18Fixture.Digest(r.ClusterAssignments, r.ClusterSizes, r.CentersOriginalScale, r.CentersWorkingScale, r.DistanceToAssignedCenter,
            r.WithinClusterSSByCluster, r.StandardizationLocations, r.StandardizationScales, New Double() {r.TotalWithinClusterSS, r.TotalSS, r.BetweenClusterSS, r.ObjectiveValue})
        Dim ar = a.Result
        Assert.AreEqual(Multivariate18Fixture.Digest(ar.ClusterAssignments, ar.ClusterSizes, ar.CentersOriginalScale, ar.CentersWorkingScale, ar.DistanceToAssignedCenter,
            ar.WithinClusterSSByCluster, ar.StandardizationLocations, ar.StandardizationScales, New Double() {ar.TotalWithinClusterSS, ar.TotalSS, ar.BetweenClusterSS, ar.ObjectiveValue}), digest)
        Dim tables = b.wrapResults(), projection = MultivariateClusteringProjectionPolicy.KMeans(request, r, tables)
        Multivariate17Fixture.Tables(a.wrapResults(), projection.LegacyTables())
        Multivariate17Fixture.Tables(tables, MultivariateClusteringProjectionPolicy.KMeans(request, r, tables).LegacyTables())
        Multivariate17Fixture.Cells(MultivariateWorkflowPolicy.ClusterInputTable(Cells(x), input.VariableNames, input.SourceRowIds, input.RowLabels), request.DataTable(Cells(x)))
        CollectionAssert.AreEqual(r.ClusterAssignments, projection.Observations.Where(Function(v) v.ActiveOrdinal.HasValue).OrderBy(Function(v) v.ActiveOrdinal).Select(Function(v) v.Membership.Value).ToArray())
        Assert.AreEqual(r.RandomSeedUsed, projection.EffectiveSeed.Value) : Assert.AreEqual(r.Converged, projection.Converged.Value)
        Assert.AreEqual(If(o.Initialization = KMeansInitializationMethod.UserSpecifiedCenters, 1, o.Starts), r.StartsEvaluated)
        Assert.AreEqual(r.CentersOriginalScale.GetLength(0), projection.CenterShape.Rows)
        Assert.AreEqual(o.Clusters, projection.RequestedClusterCount.Value)
        Assert.AreEqual(r.CentersWorkingScale.GetLength(1), projection.WorkingCenterShape.Columns)
        If id = 91 Then Assert.IsFalse(r.Converged, "Existing one-iteration random-partition fixture is nonconverged.")
        Assert.AreEqual(0, projection.Report.Plots.Count) : Assert.AreEqual("KMeans results", projection.ResultsSheetName)
        Record("KMeans", id, input, x, o, projection, Nothing, digest, centers)
        Return projection
    End Function
    Public Shared Function HProfile(id As Integer) As MultivariateClusteringProjection
        Dim edge = If(id >= 147 AndAlso id <= 160, id - 147, If(id >= 168 AndAlso id <= 170, id - 168, -1))
        Dim x = Data(edge), input = Multivariate19Fixture.Input(MultivariateAnalysisKind.Hierarchical, x, id Mod 2), o = HOptions(id)
        Dim request As New MultivariateHierarchicalRequest(input, o), cut As New MultivariateClusterCutRequest(o.Cut, o.Clusters, o.CutHeight)
        Dim a As New HierarchicalClustering, b As New HierarchicalClustering
        Dim ea = Failure(Sub()
                             a.dataInputs(x, input.RowLabels, input.VariableNames)
                             a.settingsInputs(o.Linkage, o.Distance, o.MinkowskiPower, o.Standardization, o.Missing)
                             a.reportInputs(o.Cut, o.Clusters, o.CutHeight) : a.Fit() : a.wrapResults()
                         End Sub)
        Dim phase As String = "Data"
        Dim eb = Failure(Sub()
                             b.dataInputs(x, input.RowLabels, input.VariableNames)
                             phase = "Settings" : request.Configure(b)
                             phase = "CutSettings" : cut.Configure(b)
                             phase = "Fit" : b.Fit()
                             phase = "Report" : b.wrapResults()
                         End Sub)
        SameFailure(ea, eb)
        If id < 147 AndAlso (o.Distance = HierarchicalDistanceMetric.Euclidean OrElse o.Distance = HierarchicalDistanceMetric.SquaredEuclidean OrElse
            (o.Linkage <> HierarchicalLinkageMethod.Ward AndAlso o.Linkage <> HierarchicalLinkageMethod.Centroid AndAlso o.Linkage <> HierarchicalLinkageMethod.Median)) Then Assert.IsNull(eb)
        If eb IsNot Nothing Then
            Record("Hierarchical", id, input, x, o, Nothing, eb, "", fitAvailable:=b.Result IsNot Nothing, phase:=phase, fittedH:=b.Result) : Return Nothing
        End If
        Dim r = b.Result, ar = a.Result
        Dim digest = Multivariate18Fixture.Digest(r.MergeLeftClusterIds, r.MergeRightClusterIds, r.MergeHeights, r.MergeClusterSizes, r.LeafOrder, r.StandardizationLocations, r.StandardizationScales)
        Assert.AreEqual(Multivariate18Fixture.Digest(ar.MergeLeftClusterIds, ar.MergeRightClusterIds, ar.MergeHeights, ar.MergeClusterSizes, ar.LeafOrder, ar.StandardizationLocations, ar.StandardizationScales), digest)
        Dim tables = b.wrapResults(), projection = MultivariateClusteringProjectionPolicy.Hierarchical(request, cut, r, tables)
        Multivariate17Fixture.Tables(a.wrapResults(), projection.LegacyTables())
        Multivariate17Fixture.Tables(tables, MultivariateClusteringProjectionPolicy.Hierarchical(request, cut, r, tables).LegacyTables())
        Multivariate17Fixture.Cells(MultivariateWorkflowPolicy.ClusterInputTable(Cells(x), input.VariableNames, input.SourceRowIds, input.RowLabels), request.DataTable(Cells(x)))
        CollectionAssert.AreEqual(r.LeafOrder, projection.Observations.Where(Function(v) v.LeafPosition.HasValue).OrderBy(Function(v) v.LeafPosition).Select(Function(v) v.InputOrdinal).ToArray())
        Assert.AreEqual(r.MergeHeights.Length, projection.Merges.Count)
        For Each m In projection.Merges
            Assert.AreEqual(r.ActiveRowIndices.Length + m.StepNumber, m.InternalId)
            Assert.AreEqual(r.MergeHeights(m.StepNumber - 1), m.Height)
        Next
        CollectionAssert.AreEqual(cut.Membership(r), projection.Observations.Where(Function(v) v.ActiveOrdinal.HasValue).OrderBy(Function(v) v.ActiveOrdinal).Select(Function(v) v.Membership.Value).ToArray())
        Assert.AreEqual("Hierarchical results", projection.ResultsSheetName)
        Record("Hierarchical", id, input, x, o, projection, Nothing, digest)
        Return projection
    End Function
End Class

<TestClass>
Public Class Multivariate19Tests
    <DataTestMethod><DataRow(False)><DataRow(True)>
    Public Sub PhysicalRowsInputOrdinalsAndLeavesStayDistinct(labelsSelected As Boolean)
        Dim rows = New Integer() {101, 105, 118, 131}, names = New String() {"x", "y"}
        Dim labels = If(labelsSelected, New String() {"Row 101", "Row 105", "dup α", "dup α"}, Nothing)
        Dim x(,) As Double = {{0, 0}, {Double.NaN, 1}, {10, 10}, {12, 10}}
        Dim input As New MultivariateInputIntent(MultivariateAnalysisKind.Hierarchical, 4, 2, names, rows, labels)
        Dim options As New MultivariateHierarchicalOptions With {.Linkage = HierarchicalLinkageMethod.Complete, .Missing = ClusterMissingValuePolicy.ListwiseDeletion, .Clusters = 2}
        Dim request As New MultivariateHierarchicalRequest(input, options), cut As New MultivariateClusterCutRequest(options.Cut, 2, 0), model As New HierarchicalClustering
        rows(0) = -100 : names(0) = "changed" : If labels IsNot Nothing Then labels(0) = "changed"
        model.dataInputs(x, input.RowLabels, input.VariableNames) : request.Configure(model) : cut.Configure(model) : model.Fit()
        Dim tables = model.wrapResults(), projection = MultivariateClusteringProjectionPolicy.Hierarchical(request, cut, model.Result, tables)
        CollectionAssert.AreEqual(New Integer() {101, 105, 118, 131}, projection.Observations.Select(Function(o) o.SourceRowId.Value).ToArray())
        CollectionAssert.AreEqual(New Integer() {1, 3, 4}, projection.Observations.Where(Function(o) o.ActiveOrdinal.HasValue).Select(Function(o) o.InputOrdinal).ToArray())
        CollectionAssert.AreEqual(New Integer() {1, 2, 3}, projection.Observations.Where(Function(o) o.LeafId.HasValue).Select(Function(o) o.LeafId.Value).ToArray())
        Assert.AreEqual(1, projection.Observations(1).RemovedOrdinal.Value) : Assert.IsFalse(projection.Observations(1).LeafId.HasValue)
        Assert.AreEqual(4, projection.Merges(0).InternalId)
        Dim data = request.DataTable(Multivariate19Fixture.Cells(x)), membership = model.Result.GetMembershipTable(2)
        Assert.AreEqual(101, CInt(data(1, 0))) : Assert.AreEqual(1, CInt(membership(1, 0)))
        Assert.AreEqual(118, CInt(data(3, 0))) : Assert.AreEqual(3, CInt(membership(2, 0)))
        Assert.AreEqual(If(labelsSelected, "Row 101", "Obs 1"), projection.Observations(0).DisplayLabel)
        Assert.AreEqual(If(labelsSelected, "RowLabel", "x"), CStr(data(0, 1)))
        Dim kinput As New MultivariateInputIntent(MultivariateAnalysisKind.KMeans, 4, 2, {"x", "y"}, input.SourceRowIds, input.RowLabels)
        Dim krequest As New MultivariateKMeansRequest(kinput, New MultivariateKMeansOptions With {.Clusters = 3, .Seed = 0, .Missing = ClusterMissingValuePolicy.ListwiseDeletion}), kmodel As New KMeans
        kmodel.dataInputs(x, kinput.RowLabels, kinput.VariableNames) : krequest.Configure(kmodel) : kmodel.Fit()
        Dim kp = MultivariateClusteringProjectionPolicy.KMeans(krequest, kmodel.Result, kmodel.wrapResults())
        CollectionAssert.AreEqual(New Integer() {1, 3, 4}, kp.Observations.Where(Function(o) o.ActiveOrdinal.HasValue).Select(Function(o) o.InputOrdinal).ToArray())
        CollectionAssert.AreEqual(New Integer() {101, 118, 131}, kp.Observations.Where(Function(o) o.ActiveOrdinal.HasValue).Select(Function(o) o.SourceRowId.Value).ToArray())
        Assert.AreEqual(3, kp.CenterShape.Rows) : Assert.AreEqual(3, kmodel.Result.ClusterSizes.Sum())
        Assert.AreEqual(1, CInt(kmodel.Result.GetObservationAssignmentsTable()(1, 0)))
        Assert.IsTrue(kp.Observations.All(Function(o) Not o.LeafId.HasValue))
        Dim oldLabel = projection.Observations(0).DisplayLabel
        model.Result.ActiveRowIndices(0) = 999 : model.Result.ActiveRowLabels(0) = "mutated"
        model.Result.MergeHeights(0) = -123 : model.Result.LeafOrder(0) = -999
        Assert.AreEqual(1, projection.Observations(0).InputOrdinal) : Assert.AreEqual(oldLabel, projection.Observations(0).DisplayLabel)
        Assert.AreNotEqual(-123.0, projection.Merges(0).Height)
        Multivariate17Fixture.Tables(tables, projection.LegacyTables())
    End Sub

    <TestMethod>
    Public Sub RequestsAndReportViewsDefendCopiesAndDoNotClaimFit()
        Dim x = Multivariate19Fixture.Data(), input = Multivariate19Fixture.Input(MultivariateAnalysisKind.KMeans, x, 1)
        Dim o As New MultivariateKMeansOptions With {.Clusters = 2, .Seed = 0}, request As New MultivariateKMeansRequest(input, o), model As New KMeans
        o.Seed = 987 : o.Clusters = 99 : request.Options.Clusters = 12
        Assert.AreEqual(2, request.Options.Clusters) : Assert.AreEqual(0, request.Options.Seed) : Assert.IsTrue(request.HasExplicitSeed)
        Assert.IsFalse(New MultivariateKMeansRequest(input, New MultivariateKMeansOptions).HasExplicitSeed)
        Dim h As New MultivariateHierarchicalOptions, hr As New MultivariateHierarchicalRequest(input, h)
        h.MinkowskiPower = -9 : hr.Options.MinkowskiPower = -1 : Assert.AreEqual(2.0, hr.Options.MinkowskiPower)
        model.dataInputs(x, input.RowLabels, input.VariableNames) : request.Configure(model) : Assert.IsNull(model.Result) : model.Fit()
        Dim projection = MultivariateClusteringProjectionPolicy.KMeans(request, model.Result, model.wrapResults())
        Dim expected = model.wrapResults(), list = projection.LegacyTables()
        list.Clear() : projection.Report.Document.Tables(0).Model.Values(0, 0) = "mutated"
        Multivariate17Fixture.Tables(expected, projection.LegacyTables())
        Assert.ThrowsException(Of NotSupportedException)(Sub() DirectCast(projection.Observations, IList(Of MultivariateClusterObservation)).Clear())
    End Sub

    <TestMethod>
    Public Sub BlankSeedIsResolvedOnlyByCoreAndCanBeReplayedWithItsReturnedSeed()
        Dim x = Multivariate19Fixture.Data(), input = Multivariate19Fixture.Input(MultivariateAnalysisKind.KMeans, x, 0)
        Dim options As New MultivariateKMeansOptions With {.Clusters = 2}, request As New MultivariateKMeansRequest(input, options), model As New KMeans
        Assert.AreEqual(Integer.MinValue, request.Options.Seed) : Assert.IsFalse(request.HasExplicitSeed)
        model.dataInputs(x) : request.Configure(model) : Assert.IsNull(model.Result) : model.Fit()
        options.Seed = model.Result.RandomSeedUsed
        Dim replay As New KMeans
        replay.dataInputs(x) : Dim replayRequest As New MultivariateKMeansRequest(input, options) : replayRequest.Configure(replay) : replay.Fit()
        CollectionAssert.AreEqual(model.Result.ClusterAssignments, replay.Result.ClusterAssignments)
        Assert.AreEqual(model.Result.TotalWithinClusterSS, replay.Result.TotalWithinClusterSS)
        Dim projection = MultivariateClusteringProjectionPolicy.KMeans(request, model.Result, model.wrapResults())
        Assert.AreEqual(model.Result.RandomSeedUsed, projection.EffectiveSeed.Value)
        Multivariate19Fixture.Record("KMeansUnspecifiedSeed", 0, input, x, request.Options, projection, Nothing, Multivariate18Fixture.Digest(model.Result.ClusterAssignments))
    End Sub

    <DataTestMethod><DataRow(0)><DataRow(1)><DataRow(123)>
    Public Sub FixedSeedReplayKeepsAssignmentsCentresBestStartAndIterationState(seed As Integer)
        Dim x = Multivariate19Fixture.Data(), input = Multivariate19Fixture.Input(MultivariateAnalysisKind.KMeans, x, 0)
        Dim request As New MultivariateKMeansRequest(input, New MultivariateKMeansOptions With {.Clusters = 3, .Seed = seed, .Starts = 9, .Initialization = KMeansInitializationMethod.RandomPartition})
        Dim a As New KMeans, b As New KMeans
        For Each model In {a, b} : model.dataInputs(x) : request.Configure(model) : model.Fit() : Next
        Assert.AreEqual(seed, a.Result.RandomSeedUsed) : Assert.AreEqual(seed, b.Result.RandomSeedUsed)
        Assert.AreEqual(a.Result.Iterations, b.Result.Iterations) : Assert.AreEqual(a.Result.Converged, b.Result.Converged)
        Assert.AreEqual(a.Result.StartsEvaluated, b.Result.StartsEvaluated)
        Assert.AreEqual(Multivariate18Fixture.Digest(a.Result.ClusterAssignments, a.Result.CentersWorkingScale, a.Result.CentersOriginalScale), Multivariate18Fixture.Digest(b.Result.ClusterAssignments, b.Result.CentersWorkingScale, b.Result.CentersOriginalScale))
    End Sub

    <DataTestMethod><DataRow(0)><DataRow(1)><DataRow(2)>
    Public Sub ReportedDistanceDoesNotChangeSseAssignmentsOrStandardizedOriginalCentres(scale As Integer)
        Dim x = Multivariate19Fixture.Data(), input = Multivariate19Fixture.Input(MultivariateAnalysisKind.KMeans, x, 0)
        Dim a As New KMeans, b As New KMeans, o As New MultivariateKMeansOptions With {.Clusters = 2, .Seed = 0, .Standardization = CType(scale, ClusterStandardizationMode), .Initialization = KMeansInitializationMethod.UserSpecifiedCenters}
        For Each model In {a, b}
            model.dataInputs(x) : model.startingCentersInputs(New Double(,) {{0, 0}, {10, 10}})
            Dim distanceRequest As New MultivariateKMeansRequest(input, o) : distanceRequest.Configure(model) : model.Fit() : o.Distance = KMeansDistanceMetric.Euclidean
        Next
        CollectionAssert.AreEqual(a.Result.ClusterAssignments, b.Result.ClusterAssignments)
        Assert.AreEqual(a.Result.TotalWithinClusterSS, b.Result.TotalWithinClusterSS)
        Assert.AreEqual(a.Result.ObjectiveValue, b.Result.ObjectiveValue)
        For i = 0 To a.Result.DistanceToAssignedCenter.Length - 1
            Assert.AreEqual(Math.Sqrt(a.Result.DistanceToAssignedCenter(i)), b.Result.DistanceToAssignedCenter(i), 0.000000001)
        Next
    End Sub

    <DataTestMethod><DataRow(0)><DataRow(1)><DataRow(2)><DataRow(3)><DataRow(4)><DataRow(5)><DataRow(6)><DataRow(7)>
    Public Sub DendrogramRequestsDelegateExactCoreGeometryAndPreserveNativeArguments(mode As Integer)
        Dim x = Multivariate19Fixture.Data(), input = Multivariate19Fixture.Input(MultivariateAnalysisKind.Hierarchical, x, 1)
        Dim model As New HierarchicalClustering, options As New MultivariateHierarchicalOptions With {.Linkage = HierarchicalLinkageMethod.Complete, .Clusters = 2}
        model.dataInputs(x, input.RowLabels, input.VariableNames) : Dim hierarchicalRequest As New MultivariateHierarchicalRequest(input, options) : hierarchicalRequest.Configure(model) : model.Fit()
        For Each labelMode As DendrogramLabelMode In {DendrogramLabelMode.DataLabels, DendrogramLabelMode.AxisTitle, DendrogramLabelMode.None}
            For Each cutMode In {HierarchicalMembershipDisplayMode.ByClusterCount, HierarchicalMembershipDisplayMode.ByHeight}
                Dim cut As New MultivariateClusterCutRequest(cutMode, 2, model.Result.MergeHeights(1)), request As New MultivariateDendrogramRequest(CType(mode \ 4, DendrogramHeightMode), CType(mode Mod 4, DendrogramOrientation), labelMode, cut)
                Dim a = model.Result.CreateDendrogramLayout(request.HeightMode, request.Orientation, cut.Mode, cut.Clusters, cut.Height), b = request.CreateLayout(model.Result)
                Assert.AreEqual(Multivariate18Fixture.Digest(a.PolylineX, a.PolylineY, a.LeafX, a.LeafY, a.LeafLabels, a.LeafOriginalRowOrder, a.CutLineX, a.CutLineY),
                    Multivariate18Fixture.Digest(b.PolylineX, b.PolylineY, b.LeafX, b.LeafY, b.LeafLabels, b.LeafOriginalRowOrder, b.CutLineX, b.CutLineY))
                Assert.AreEqual(a.CutDisplayHeight, b.CutDisplayHeight) : Assert.AreEqual(a.MaximumHeight, b.MaximumHeight)
                Assert.AreEqual(a.ClusterPolylineX.Count, b.ClusterPolylineX.Count)
                For i = 0 To a.ClusterPolylineX.Count - 1
                    CollectionAssert.AreEqual(a.ClusterPolylineX(i), b.ClusterPolylineX(i)) : CollectionAssert.AreEqual(a.ClusterPolylineY(i), b.ClusterPolylineY(i))
                Next
                Assert.AreEqual("A1", request.Anchor) : Assert.AreEqual("Dendrogram", request.SheetName)
                Assert.AreEqual("Hierarchical Clustering Dendrogram", request.Title) : Assert.AreEqual(680.0, request.Width) : Assert.AreEqual(420.0, request.Height)
                Dim record As New XElement("profile", New XAttribute("family", "Dendrogram"), New XAttribute("fixture", mode),
                    New XAttribute("classification", "CORE_LAYOUT_RETURNED"), New XAttribute("status", "NOT_GOLDEN"),
                    New XAttribute("culture", CultureInfo.CurrentCulture.Name), New XAttribute("runtime", Environment.Version.ToString()),
                    New XAttribute("fixture_sha256", Multivariate18Fixture.Digest(x, input.SourceRowIds, input.RowLabels, input.VariableNames)),
                    New XElement("source_rows", input.SourceRowIds.Select(Function(v) New XElement("row", v))),
                    New XElement("names", input.VariableNames.Select(Function(v) New XElement("name", v))),
                    New XElement("request", New XAttribute("height_mode", request.HeightMode), New XAttribute("orientation", request.Orientation), New XAttribute("label_mode", request.LabelMode),
                        New XAttribute("cut_mode", cut.Mode), New XAttribute("clusters", cut.Clusters), New XAttribute("cut_height", cut.Height), New XAttribute("role", request.Plot.DataRole),
                        New XAttribute("title", request.Title), New XAttribute("sheet", request.SheetName), New XAttribute("anchor", request.Anchor), New XAttribute("width", request.Width), New XAttribute("height", request.Height)),
                    New XElement("leaf_order", b.LeafOriginalRowOrder.Select(Function(v) New XElement("ordinal", v))),
                    New XElement("leaf_labels", b.LeafLabels.Select(Function(v) New XElement("label", v))),
                    New XElement("layout_sha256", Multivariate18Fixture.Digest(b.PolylineX, b.PolylineY, b.LeafX, b.LeafY, b.CutLineX, b.CutLineY)),
                    New XElement("cluster_colour_request_sha256", Multivariate18Fixture.Digest(b.ClusterPolylineX.Cast(Of Array)().Concat(b.ClusterPolylineY.Cast(Of Array)()).ToArray())),
                    New XElement("cut_display_height", b.CutDisplayHeight), New XElement("maximum_height", b.MaximumHeight), New XElement("native_execution", "NOT_RUN"))
                Console.WriteLine("04A19_XML|" & Convert.ToBase64String(Encoding.UTF8.GetBytes(record.ToString(SaveOptions.DisableFormatting))))
            Next
        Next
        Assert.AreEqual(0, MultivariateReportPolicy.PlotPlan(MultivariateAnalysisKind.Hierarchical, False).Count)
        Assert.AreEqual(1, MultivariateReportPolicy.PlotPlan(MultivariateAnalysisKind.Hierarchical, True).Count)
    End Sub

    <TestMethod>
    Public Sub CutsUseActiveCountAndIncludeEqualHeightsWithoutRelabelling()
        Dim model As New HierarchicalClustering
        model.dataInputs(New Double(,) {{0}, {1}, {3}, {7}}) : model.settingsInputs(HierarchicalLinkageMethod.Complete) : model.Fit()
        For Each k In {1, 2, 4}
            CollectionAssert.AreEqual(model.Result.GetMembershipByClusterCount(k), New MultivariateClusterCutRequest(HierarchicalMembershipDisplayMode.ByClusterCount, k, 0).Membership(model.Result))
        Next
        For Each k In {0, 5}
            Dim cut As New MultivariateClusterCutRequest(HierarchicalMembershipDisplayMode.ByClusterCount, k, 0)
            Multivariate19Fixture.SameFailure(Multivariate19Fixture.Failure(Sub() model.Result.GetMembershipByClusterCount(k)), Multivariate19Fixture.Failure(Sub() cut.Membership(model.Result)))
            Assert.IsNotNull(Multivariate19Fixture.Failure(Sub() cut.Membership(model.Result)))
        Next
        For Each h In New Double() {-1, model.Result.MergeHeights(0), model.Result.MergeHeights(1), model.Result.MergeHeights.Last(), Double.PositiveInfinity}
            CollectionAssert.AreEqual(model.Result.GetMembershipByHeight(h), New MultivariateClusterCutRequest(HierarchicalMembershipDisplayMode.ByHeight, 2, h).Membership(model.Result))
        Next
        Assert.AreEqual(3, model.Result.GetMembershipByHeight(model.Result.MergeHeights(0)).Distinct().Count())
    End Sub

    <DataTestMethod><DataRow(4)><DataRow(5)>
    Public Sub ExistingInversionAndPrefixHeightCutRemainCandidateOnly(linkage As Integer)
        Dim x(,) As Double = {{-1, 0}, {1, 0}, {0, 1.8}}, model As New HierarchicalClustering
        Dim input = Multivariate19Fixture.Input(MultivariateAnalysisKind.Hierarchical, x, 1)
        Dim options As New MultivariateHierarchicalOptions With {.Linkage = CType(linkage, HierarchicalLinkageMethod), .Distance = HierarchicalDistanceMetric.Euclidean, .Clusters = 2, .Cut = HierarchicalMembershipDisplayMode.ByHeight, .CutHeight = 1.9}
        Dim request As New MultivariateHierarchicalRequest(input, options), cut As New MultivariateClusterCutRequest(options.Cut, 2, 1.9)
        model.dataInputs(x, input.RowLabels, input.VariableNames) : request.Configure(model) : cut.Configure(model) : model.Fit()
        Assert.IsTrue(model.Result.MergeHeights(1) < model.Result.MergeHeights(0), "Existing centroid/median inversion retained.")
        CollectionAssert.AreEqual(New Integer() {1, 2, 3}, cut.Membership(model.Result), "Core stops at the first merge above the cut, despite a later smaller merge height.")
        Dim projection = MultivariateClusteringProjectionPolicy.Hierarchical(request, cut, model.Result, model.wrapResults())
        Assert.IsTrue(projection.Merges(1).Height < projection.Merges(0).Height)
        Multivariate19Fixture.Record("HierarchicalInversionCandidate", linkage, input, x, options, projection, Nothing, Multivariate18Fixture.Digest(model.Result.MergeHeights, cut.Membership(model.Result)))
    End Sub
    Public Shared Iterator Function Profiles() As IEnumerable(Of Object())
        For Each family In {"KMeans", "Hierarchical"}
            For id = 0 To If(family = "KMeans", 107, 172) : Yield {family, id} : Next
        Next
    End Function
    <DataTestMethod><DynamicData(NameOf(Profiles), DynamicDataSourceType.Method)>
    Public Sub ExactCoreRequestAndProjectionParity(family As String, id As Integer)
        Dim saved = CultureInfo.CurrentCulture
        Try
            For Each culture In {"", "sk-SK"}
                CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(culture)
                If family = "KMeans" Then
                    Multivariate19Fixture.KProfile(id)
                Else
                    Multivariate19Fixture.HProfile(id)
                End If
            Next
        Finally
            CultureInfo.CurrentCulture = saved
        End Try
    End Sub
End Class
