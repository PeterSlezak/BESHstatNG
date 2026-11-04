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
Public Class Multivariate18AdapterTests
    <DataTestMethod><DataRow(0)><DataRow(1)><DataRow(2)><DataRow(3)><DataRow(4)><DataRow(5)><DataRow(6)><DataRow(7)><DataRow(24)><DataRow(27)><DataRow(48)><DataRow(59)><DataRow(71)>
    Public Sub ActualPcaDataWriterConsumesCommonPositionsAndCorePayloads(caseNumber As Integer)
        Dim saved = CultureInfo.CurrentCulture
        Try
            For Each culture In {"", "sk-SK"}
                CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(culture)
                Dim data = Multivariate18Fixture.PcaData(caseNumber), options = Multivariate18Fixture.PcaOptions(caseNumber)
                Dim labels = Multivariate18Fixture.RowLabels(data, caseNumber), groups = Multivariate18Fixture.Groups(data, caseNumber)
                Dim input = WindowsMultivariateWorkflowAdapter.Capture(MultivariateAnalysisKind.PCA, data, labels, groups), model As New PCA
                model.dataInputs(data.DataDbl, input.SourceRowIds, input.VariableNames, options.Label) : options.Configure(model, 1000, 0.000000000001) : model.Calculate(options.CoreRetention, options.Value)
                Dim projection = MultivariateComponentProjectionPolicy.Pca(input, model, options, data.FinalData)
                Dim before As New Multivariate18Fixture.MemoryWriter, after As New Multivariate18Fixture.MemoryWriter
                LegacyMultivariate18Layout.PcaData(before, data, model, labels, groups)
                WindowsMultivariateWorkflowAdapter.WritePcaData(after, data.FinalData, model, projection)
                Multivariate18Fixture.Blocks(before, after)
                Dim repeated As New Multivariate18Fixture.MemoryWriter
                WindowsMultivariateWorkflowAdapter.WritePcaData(repeated, data.FinalData, model, projection)
                Multivariate18Fixture.Blocks(after, repeated)
            Next
        Finally
            CultureInfo.CurrentCulture = saved
        End Try
    End Sub
    <DataTestMethod><DataRow(0)><DataRow(1)><DataRow(2)><DataRow(5)><DataRow(12)><DataRow(16)>
    Public Sub ActualMcaDataWriterKeepsTwoHeadersAndPostSubsetRows(caseNumber As Integer)
        Dim saved = CultureInfo.CurrentCulture
        Try
            For Each culture In {"", "sk-SK"}
                CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(culture)
                Dim data = Multivariate18Fixture.McaData(caseNumber), model As New CA
                Dim input = WindowsMultivariateWorkflowAdapter.Capture(MultivariateAnalysisKind.MCA, data)
                model.DataMultiple(Global.BESHStatNG.DataManagement.ArrayUtilities.Array2strArray(data.FinalData), input.VariableNames) : model.Calculate()
                Dim projection = MultivariateComponentProjectionPolicy.Mca(New MultivariateMcaTransformIntent(input, caseNumber Mod 2 = 1), model, data.FinalData)
                Dim before As New Multivariate18Fixture.MemoryWriter, after As New Multivariate18Fixture.MemoryWriter
                LegacyMultivariate18Layout.McaData(before, data, model)
                WindowsMultivariateWorkflowAdapter.WriteMcaData(after, data.FinalData, model, projection)
                Multivariate18Fixture.Blocks(before, after)
                Assert.AreEqual(3, after.Blocks(1).StartRow) : Assert.AreEqual(data.nCols + 3, after.Blocks(5).StartColumn)
                Assert.AreEqual(1, after.Blocks(5).StartRow) : Assert.AreEqual(2, after.Blocks(6).StartRow) : Assert.AreEqual(3, after.Blocks(7).StartRow)
                CollectionAssert.AreEqual(data.RowIds, projection.Input.SourceRowIds)
            Next
        Finally
            CultureInfo.CurrentCulture = saved
        End Try
    End Sub
    <DataTestMethod><DataRow(0)><DataRow(1)><DataRow(2)><DataRow(3)><DataRow(4)><DataRow(5)><DataRow(6)><DataRow(7)><DataRow(8)><DataRow(9)><DataRow(10)><DataRow(11)>
    Public Sub LabelledWindowsCoercionAndBothWriterDestinationsRemainOriginal(caseNumber As Integer)
        Dim saved = CultureInfo.CurrentCulture
        Try
            For Each culture In {"", "sk-SK"}
                CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(culture)
                Dim data = Multivariate18Fixture.CaData(caseNumber), before(,) As Integer = Nothing, after(,) As Integer = Nothing
                Dim labels = Global.BESHStatNG.DataManagement.ArrayUtilities.Array2strArray(Matrix.GetColumnFrom2Darray(data.FinalData, 0))
                Dim input = WindowsMultivariateWorkflowAdapter.CaptureLabelledCa(data, labels), oldError As Exception = Nothing, newError As Exception = Nothing
                Try : before = LegacyMultivariate18Layout.LabelledCounts(data) : Catch ex As Exception : oldError = ex : End Try
                Try : after = WindowsMultivariateWorkflowAdapter.ConvertLabelledCaCounts(data, input) : Catch ex As Exception : newError = ex : End Try
                If oldError IsNot Nothing Then
                    Assert.IsNotNull(newError) : Assert.AreEqual(oldError.GetType(), newError.GetType()) : Assert.AreEqual(oldError.Message, newError.Message) : Continue For
                End If
                Assert.IsNull(newError) : CollectionAssert.AreEqual(before, after)
                Dim model As New CA
                Try
                    model.data(before, labels, data.varNames.Skip(1).ToArray()) : model.Calculate()
                Catch ex As Exception
                    ' This test covers conversion parity; identical already-converted Core inputs
                    ' retain their existing failures (full classification is in Common profiles).
                    Continue For
                End Try
                Dim coreTables = model.wrapResults(), prefix As New ResultTable
                prefix.AddTitle("Analyzed contingency table") : prefix.SetBody(before) : prefix.AddHeaderLeftRow(model.rowNames) : prefix.AddHeaderTopRow(model.ColumNames)
                Dim expected As New List(Of ResultTable)(coreTables) : expected.Insert(0, prefix)
                Dim projection = MultivariateComponentProjectionPolicy.SimpleCa(input, model, after)
                Dim report = MultivariateComponentProjectionPolicy.CaReport(projection, after, coreTables)
                For Each anchor In {New Integer() {1, 1}, New Integer() {7, 9}}
                    Dim oldWriter As New Multivariate18Fixture.MemoryWriter(anchor(0), anchor(1)), newWriter As New Multivariate18Fixture.MemoryWriter(anchor(0), anchor(1))
                    Dim p As New ProcessListofResultTables(expected), q As New ProcessListofResultTables(report.LegacyTables())
                    Assert.AreEqual(p.TotRows, q.TotRows) : Assert.AreEqual(p.TotCols, q.TotCols)
                    p.writeToSheet(oldWriter, True) : q.writeToSheet(newWriter, True) : Multivariate18Fixture.Blocks(oldWriter, newWriter)
                Next
            Next
        Finally
            CultureInfo.CurrentCulture = saved
        End Try
    End Sub
    <TestMethod>
    Public Sub DormantMappingCannotBePassedToTheShippingLabelledConverter()
        Dim data = Multivariate18Fixture.Acquired(New Object(,) {{1.0, 2.0}, {3.0, 4.0}}, {"a", "b"})
        Dim dormant = MultivariateCorrespondenceInputPolicy.DormantUnlabelled(data.nRows, data.nCols, data.varNames, data.RowIds)
        Assert.ThrowsException(Of InvalidOperationException)(Sub() WindowsMultivariateWorkflowAdapter.ConvertLabelledCaCounts(data, dormant))
        Assert.ThrowsException(Of IndexOutOfRangeException)(Sub() LegacyMultivariate18Layout.LegacyUnlabelledCounts(data))
    End Sub
End Class
