Option Explicit On
Option Strict On
Imports System
Imports System.Collections.Generic
Imports System.Globalization
Imports System.Linq
Imports BESHStatNG
Imports BESHStatNG.ExcelCommon
Imports BESHStatNG.regression
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass>
Public Class Mixed16AdapterTests
    Private NotInheritable Class MemoryWriter
        Inherits ResultTableWriterBase
        Public ReadOnly Blocks As New List(Of ResultTableOutputBlock)
        Protected Overrides Sub WriteOutputBlock(block As ResultTableOutputBlock)
            Blocks.Add(block)
        End Sub
    End Class
    <DataTestMethod><DynamicData(NameOf(Mixed14Fixture.Cases), GetType(Mixed14Fixture), DynamicDataSourceType.Method)>
    Public Sub ActualAttachmentAndWriterPreserveCoreOrderAndRepeatedProjection(key As String)
        Dim saved = CultureInfo.CurrentCulture
        Try
            For Each culture In {"", "sk-SK"}
                CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(culture)
                Dim f = Mixed14Fixture.Load(key), req = Mixed14Fixture.LegacyRequest(key, f), result = Mixed14Fixture.Fit(key, req)
                Dim mmrm = key.StartsWith("MMRM|", StringComparison.Ordinal), kind = If(mmrm, MixedModelWorkflowKind.MMRM, MixedModelWorkflowKind.LMM)
                Dim supplemental As MixedModelSupplementalReport
                If mmrm Then
                    Dim intent As New MixedModelPostEstimationIntent(kind, f.FixedNames, "visit", f.Visit, False, Nothing, Nothing, New MixedModelReportOptions, f.Rows)
                    supplemental = MixedModelPostEstimationPolicy.ProjectMMRM(result, f.X, intent,
                        Function() MixedModelPostEstimationPolicy.ClassInformation(kind, {New MixedModelClassLevel("subject", f.Subjects.Distinct().Count(), "synthetic subjects")}))
                    CollectionAssert.AreEqual(f.Rows, intent.SourceRowIds)
                    CollectionAssert.AreEqual(req.Data.Blocks.SelectMany(Function(b) b.RowIndices).ToArray(), Mixed14Requests.Request(key).Data.BlockRowIds)
                Else
                    supplemental = MixedModelPostEstimationPolicy.LmmClassReport({})
                End If
                Dim originalList = result.AdditionalResultTables, originalTables = originalList.ToArray()
                originalList.AddRange(supplemental.LegacyTables())
                Dim expected = result.wrapResults(0.05, True, Not mmrm, False)
                originalList.RemoveRange(originalTables.Length, originalList.Count - originalTables.Length)
                WindowsMixedModelWorkflowAdapter.ApplySupplemental(result, supplemental)
                WindowsMixedModelWorkflowAdapter.ApplySupplemental(result, supplemental)
                Assert.AreSame(originalList, result.AdditionalResultTables)
                Assert.AreEqual(originalTables.Length + supplemental.Document.Tables.Count, result.AdditionalResultTables.Count)
                Dim projected = MixedModelReportPolicy.Project(kind, result, New MixedModelReportOptions With {.IterationDetails = True, .Diagnostics = False})
                Mixed16Fixture.EqualTables(expected, projected.LegacyTables(), key)
                Dim before As New MemoryWriter, after As New MemoryWriter
                ' Same native writer-consumer class, with a host-free memory sink.
                Dim p As New ProcessListofResultTables(expected), q As New ProcessListofResultTables(projected.LegacyTables())
                p.writeToSheet(before, mmrm) : q.writeToSheet(after, mmrm)
                Assert.AreEqual(before.Blocks.Count, after.Blocks.Count)
                For i = 0 To before.Blocks.Count - 1
                    Assert.AreEqual(before.Blocks(i).StartRow, after.Blocks(i).StartRow)
                    Assert.AreEqual(before.Blocks(i).StartColumn, after.Blocks(i).StartColumn)
                    CollectionAssert.AreEqual(before.Blocks(i).Model.PvalueColumns, after.Blocks(i).Model.PvalueColumns)
                    CollectionAssert.AreEqual(before.Blocks(i).Model.PvalueCells, after.Blocks(i).Model.PvalueCells)
                Next
                WindowsMixedModelWorkflowAdapter.ApplySupplemental(result, MixedModelPostEstimationPolicy.EmptyReport())
                CollectionAssert.AreEqual(originalTables, result.AdditionalResultTables.ToArray())
                Dim frozen = Mixed14Fixture.CanonicalTables(projected.LegacyTables())
                result.ReleaseLargePostEstimationWorkspaces()
                Regression10Fixture.AssertCapture(frozen, Mixed14Fixture.CanonicalTables(projected.LegacyTables()), key & " released")
            Next
        Finally
            CultureInfo.CurrentCulture = saved
        End Try
    End Sub

    <TestMethod>
    Public Sub LmmClassMetadataHasOriginalLayoutAndNoInventedMeansEditor()
        Dim rows As New List(Of MixedModelClassLevel) From {New MixedModelClassLevel("site", 3, "0, 1, 2", "Fixed"), New MixedModelClassLevel("group", 2, "0, 1", "Random")}
        Dim report = MixedModelPostEstimationPolicy.LmmClassReport(rows), original As New ResultTable
        original.AddTitle("Class level information")
        original.SetBody(New Object(,) {{"Fixed", "site", "Categorical main effect", "0, 1, 2", 3}, {"Random", "group", "Categorical main effect", "0, 1", 2}})
        original.AddHeaderTopRow({"Model part", "Variable", "Term kind", "Levels", "No. levels"})
        original.AddFootnote("Levels are observed numeric/coded values in the cleaned analysis data for variables used as categorical effects.")
        Mixed16Fixture.EqualTables(New List(Of ResultTable) From {original}, report.LegacyTables(), "LMM class")
        Assert.AreEqual(1, MixedModelReportPolicy.Plan(MixedModelWorkflowKind.LMM, New MixedModelReportOptions, True).Sections.Count)
        rows.Clear() : Assert.AreEqual(1, report.Document.Tables.Count)
    End Sub
End Class
