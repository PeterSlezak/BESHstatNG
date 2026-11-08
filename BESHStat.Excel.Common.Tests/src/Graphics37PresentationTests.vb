Option Explicit On
Option Strict On
Option Infer On
Imports System
Imports System.Linq
Imports System.Reflection
Imports System.Globalization
Imports BESHStatNG
Imports BESHStatNG.ExcelCommon
Imports BESHStatNG.Multivariate
Imports Frozen37
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass><DoNotParallelize>
Public Class Graphics37PresentationTests
    Private Shared Function Capture(type As Type, method As String, args As Object()) As String
        Try
            Return Cases37.Snapshot(type.GetMethod(method).Invoke(Nothing, args))
        Catch ex As TargetInvocationException
            Dim e = ex.InnerException, a = TryCast(e, ArgumentException)
            Return e.GetType().FullName & ":" & If(a Is Nothing, "", a.ParamName) & ":" & e.Message
        End Try
    End Function
    Private Shared Sub Compare(name As String, args As Object(), id As Integer)
        Assert.AreEqual(Capture(GetType(Original37), name, args), Capture(GetType(Current37), name, args), name & " case " & id)
    End Sub

    <DataTestMethod><DataRow("")><DataRow("sk-SK")>
    Public Sub EveryCurrentShippingPrefixMatchesFrozenOriginal(culture As String)
        Dim saved = CultureInfo.CurrentCulture
        Try
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(culture)
            For id As Integer = 0 To 127
                Dim p = Cases37.PcaModel(id), f = Cases37.FactorModel(id), ca = Cases37.CaModel(id), labels = Cases37.Labels(id), groups = Cases37.Groups(id)
                For Each name In New String(){"LoadingPlot2D", "LoadingPlot3D", "ScreePlot"}
                    Compare("Pca" & name, {p}, id)
                    Compare("Factor" & name, {f}, id)
                Next
                For Each name In New String(){"ScorePlot2D", "ScorePlot3D"}
                    Compare("Pca" & name, {p, labels, groups}, id)
                Next
                Compare("PcaBiplot", {p, Cases37.Scale(id), labels, groups}, id)
                For Each row In New Boolean(){True, False}
                    Compare("CaContribution", {ca, id Mod 6 - 1, row}, id)
                    Compare("CaPoints", {ca, row}, id)
                    Compare("CaLabels", {ca, row}, id)
                Next
                Compare("CaBounds", {ca}, id) : Compare("CaCaptions", {ca}, id)
                Dim layout = Cases37.Layout(id)
                Compare("DendrogramBounds", {layout}, id)
                Compare("DendrogramTitles", {layout, If(id Mod 3 = 0, " custom ", If(id Mod 3 = 1, " ", Nothing))}, id)
                Dim pct = New Double(){id, 33.33333333333333, -0.0}
                Compare("Captions", {pct, pct, New String(){"F1", "F2", "F3"}}, id)
                Dim xs = Enumerable.Range(0, id Mod 7).Select(Function(i) CDbl(i)).ToArray(), ys = xs.Select(Function(v) -v).ToArray()
                Dim ls = Enumerable.Range(0, xs.Length).Select(Function(i) "L" & i).ToArray(), gs = New String(){"B", "a", "A", "(missing)", "Ｂ", "b", ""}.Take(xs.Length).ToArray()
                Compare("Groups", {xs, ys, ls, If(id Mod 3 = 0, Nothing, gs), "Scores"}, id)
            Next
        Finally
            CultureInfo.CurrentCulture = saved
        End Try
    End Sub

    <DataTestMethod><DataRow("")><DataRow("sk-SK")>
    Public Sub AllDendrogramSeriesModesRetainSourceLeafAndClusterOrdinals(culture As String)
        Dim saved = CultureInfo.CurrentCulture
        Try
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(culture)
            For source As Integer = 0 To 1
                For id As Integer = 0 To 127
                    Dim layout = If(source = 0, Cases37.Layout(id), OriginalSeries37.FittedLayout(id))
                    For Each mode As DendrogramLabelMode In {DendrogramLabelMode.DataLabels, DendrogramLabelMode.AxisTitle, DendrogramLabelMode.None}
                        Dim p As New DendrogramChartPresentation37(New DendrogramChartGeometry37(layout), mode)
                        Dim rows As New Collections.Generic.List(Of Object), b = p.BaseSeries()
                        rows.Add(New Object(){b.Name, b.XValues, b.YValues, -1})
                        For Each s In p.Clusters()
                            rows.Add(New Object(){s.Name, s.XValues, s.YValues, s.SourceOrdinals(0)})
                        Next
                        Dim cut = p.CutSeries(), leaf = p.LeafSeries()
                        If cut IsNot Nothing Then rows.Add(New Object(){cut.Name, cut.XValues, cut.YValues, -1})
                        If leaf IsNot Nothing Then rows.Add(New Object(){leaf.Name, leaf.XValues, leaf.YValues, leaf.Labels, leaf.SourceOrdinals, CInt(layout.Orientation)})
                        Assert.AreEqual(Cases37.Snapshot(OriginalSeries37.DendrogramSeries(layout, mode)), Cases37.Snapshot(rows.ToArray()), "case " & id)
                    Next
                Next
            Next
        Finally
            CultureInfo.CurrentCulture = saved
        End Try
    End Sub

    <TestMethod>
    Public Sub GroupsPreserveFirstSeenOrderingAndOriginalModelPositionsWithDetachedArrays()
        Dim p = MultivariateChartPresentation37.ScoreSeries({1, 2, 3, 4}, {5, 6, 7, 8}, {"501", "484", "467", "450"}, {"b", "A", "a", "b"})
        CollectionAssert.AreEqual(New String(){"b", "A", "a"}, p.Select(Function(s) s.Name).ToArray())
        CollectionAssert.AreEqual(New Integer(){0, 3}, p(0).SourceOrdinals)
        CollectionAssert.AreEqual(New Integer(){1, 2}, p(1).SourceOrdinals)
        CollectionAssert.AreEqual(New String(){"484", "467"}, p(1).Labels)
        p(1).Labels(0) = "mutate" : p(1).XValues(0) = 999 : p(1).SourceOrdinals(0) = 999
        Assert.AreEqual("484", p(1).Labels(0)) : Assert.AreEqual(2.0, p(1).XValues(0)) : Assert.AreEqual(1, p(1).SourceOrdinals(0))
    End Sub

    <DataTestMethod><DataRow("")><DataRow("sk-SK")>
    Public Sub LatePercentTextMatchesArchivedExpressionsForExceptionalValues(culture As String)
        Dim saved = CultureInfo.CurrentCulture
        Try
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(culture)
            For Each value In New Double(){-0.0, 0.0, -0.00001, -0.0049, -0.005, 0.005, Double.NaN, Double.PositiveInfinity, Double.NegativeInfinity, Double.Epsilon, Double.MaxValue}
                Compare("Captions", {New Double(){value,value,value},New Double(){value,value,value},New String(){"F1","F2","F3"}}, 0)
            Next
        Finally
            CultureInfo.CurrentCulture = saved
        End Try
    End Sub

    <TestMethod>
    Public Sub ModelRowIdsInputLeafOrdinalsAndPhysicalRowsRemainDifferentIdentities()
        Dim model = Cases37.PcaModel(3), names = New String(){"dup", "dup", "custom"}
        Dim labels = MultivariateChartPresentation37.ScoreLabels(model, names)
        Dim points = MultivariateChartPresentation37.ScoreSeries({1,2,3},{4,5,6},labels,{"B","a","B"},modelRowIds:=model.RowIds)
        CollectionAssert.AreEqual(New Integer?(){501,467}, points(0).ModelRowIds)
        Assert.AreEqual(MultivariateOrdinalKind37.ModelPositionZeroBased, points(0).OrdinalKind)
        Assert.IsTrue(points(0).SourceAxis.Items.All(Function(v) Not v.SourceRowId.HasValue AndAlso Not v.SourcePosition.HasValue))
        points(0).ModelRowIds(0)=999 : Assert.AreEqual(501,points(0).ModelRowIds(0).Value)
        Dim hc As New HierarchicalClustering
        hc.dataInputs(New Double(,){{0,0},{Double.NaN,0},{1,1},{7,5}}, {"dup","excluded","dup","long"}, {"x","y"})
        hc.settingsInputs(missingValuePolicy:=ClusterMissingValuePolicy.ListwiseDeletion) : hc.Fit()
        Dim layout = hc.Result.CreateDendrogramLayout(membershipClusterCount:=2)
        Dim p As New DendrogramChartPresentation37(New DendrogramChartGeometry37(layout),DendrogramLabelMode.DataLabels), leaf = p.LeafSeries()
        Assert.AreEqual(MultivariateOrdinalKind37.InputOrdinalOneBased,leaf.OrdinalKind)
        CollectionAssert.AreEqual(layout.LeafOriginalRowOrder,leaf.SourceOrdinals)
        CollectionAssert.AreEqual(layout.LeafLabels,leaf.Labels)
        For i As Integer=0 To leaf.SourceOrdinals.Length-1
            Assert.AreEqual(leaf.SourceOrdinals(i)-1,leaf.SourceAxis.Items(i).SourcePosition.Value)
            Assert.IsFalse(leaf.SourceAxis.Items(i).SourceRowId.HasValue)
        Next
        Dim ca=MultivariateChartPresentation37.CorrespondenceSeries(Cases37.CaModel(2),False)
        Assert.AreEqual(MultivariateSemanticAxis.Categories,ca.SourceAxis.Kind)
    End Sub
End Class
