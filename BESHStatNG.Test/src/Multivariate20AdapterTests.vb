Option Explicit On
Option Strict On
Option Infer On
Imports System
Imports System.Collections.Generic
Imports System.Globalization
Imports System.Linq
Imports BESHStatNG.ExcelCommon
Imports BESHStatNG.Multivariate
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass>
Public Class Multivariate20AdapterTests
    <DataTestMethod><DataRow(0)><DataRow(1)><DataRow(2)><DataRow(3)><DataRow(4)><DataRow(5)><DataRow(24)><DataRow(31)><DataRow(68)><DataRow(100)><DataRow(198)><DataRow(208)>
    Public Sub ActualFactorWriterUsesPostfitProjectionAndLegacyResultSequence(id As Integer)
        Dim edge = If(id = 208, 0, -1), x = Multivariate20Fixture.FactorData(edge), originalInput = Multivariate20Fixture.Input(MultivariateAnalysisKind.Factor, x)
        Dim data = Multivariate18Fixture.Acquired(Multivariate20Fixture.Raw(x), originalInput.VariableNames, originalInput.SourceRowIds)
        Dim input = WindowsMultivariateWorkflowAdapter.Capture(MultivariateAnalysisKind.Factor, data, selectedKeys:=originalInput.Columns.Select(Function(v) v.SourceKey).ToArray())
        Dim request As New MultivariateFactorRequest(input, Multivariate20Fixture.FactorOptions(id)), model As New FactorAnalysis
        model.dataInputs(data.DataDbl, data.RowIds, data.varNames) : request.Configure(model) : model.Calculate()
        Dim projection = MultivariateFactorProjectionPolicy.Project(request, model), before As New Multivariate18Fixture.MemoryWriter, after As New Multivariate18Fixture.MemoryWriter
        before.write(MultivariateWorkflowPolicy.ClusterInputTable(data.FinalData, data.varNames, data.RowIds, Nothing))
        WindowsMultivariateWorkflowAdapter.WriteFactorData(after, data.FinalData, projection) : Multivariate18Fixture.Blocks(before, after)
        Dim oldResults As New Multivariate18Fixture.MemoryWriter, newResults As New Multivariate18Fixture.MemoryWriter
        Dim a As New ProcessListofResultTables(model.wrapResults()), report = projection.ProjectReport(model.wrapResults()), b As New ProcessListofResultTables(report.LegacyTables())
        a.writeToSheet(oldResults, True) : b.writeToSheet(newResults, True) : Multivariate18Fixture.Blocks(oldResults, newResults)
        Assert.AreEqual("Factor analysis results", request.ResultsSheetName) : Assert.AreEqual("Data", request.DataSection.SheetName)
        CollectionAssert.AreEqual(New MultivariatePlotKind() {MultivariatePlotKind.Scree, MultivariatePlotKind.Loadings2D, MultivariatePlotKind.Loadings3D}, projection.Plots.Select(Function(p) p.Request.Kind).ToArray())
    End Sub

    <DataTestMethod><DataRow(0)><DataRow(1)><DataRow(2)><DataRow(3)><DataRow(4)><DataRow(5)><DataRow(36)><DataRow(55)><DataRow(120)><DataRow(144)><DataRow(147)><DataRow(156)>
    Public Sub ActualDiscriminantWriterRetainsGroupFirstCellsLabelsAndNoRowIdColumn(id As Integer)
        Dim loaded = Multivariate20Fixture.DiscriminantData(2 + id Mod 3, If(id >= 144, id - 144, -1)), x = loaded.x
        Dim originalInput = Multivariate20Fixture.Input(MultivariateAnalysisKind.Discriminant, x, id Mod 2 = 1)
        Dim data = Multivariate18Fixture.Acquired(Multivariate20Fixture.Raw(x, loaded.groups), originalInput.VariableNames, originalInput.SourceRowIds)
        Dim input = WindowsMultivariateWorkflowAdapter.Capture(MultivariateAnalysisKind.Discriminant, data, originalInput.RowLabels, selectedKeys:=originalInput.Columns.Select(Function(v) v.SourceKey).ToArray())
        Dim o = Multivariate20Fixture.DiscriminantOptions(id), request As New MultivariateDiscriminantRequest(input, o, id Mod 2 = 1), validation As New MultivariateDiscriminantValidationRequest(o), model As New DiscriminantAnalysis
        model.dataInputs(LegacyMultivariate20HostConversion.Predictors(data), loaded.groups, input.RowLabels, LegacyMultivariate20HostConversion.Names(data))
        request.ConfigureModel(model)
        Dim priors As MultivariateUserPriorIntent = Nothing
        If o.Priors = DiscriminantPriorMode.UserSpecified Then
            Dim labels() As Object = Nothing, values() As Double = Nothing
            MultivariateLegacyChoices.ParsePriors(Multivariate20Fixture.PriorText(id, loaded.groups), labels, values)
            priors = request.ApplyUserPriors(model, labels, values)
        End If
        validation.Configure(model)
        Dim failure = Multivariate19Fixture.Failure(Sub() model.Fit())
        If failure IsNot Nothing Then
            Assert.IsNull(Multivariate20Fixture.DiscriminantProfile(id))
            Return
        End If
        Dim projection = MultivariateDiscriminantProjectionPolicy.Project(request, validation, priors, model), before As New Multivariate18Fixture.MemoryWriter, after As New Multivariate18Fixture.MemoryWriter
        before.write(MultivariateWorkflowPolicy.DiscriminantInputTable(data.FinalData, data.varNames, loaded.groups, input.RowLabels))
        WindowsMultivariateWorkflowAdapter.WriteDiscriminantData(after, data.FinalData, loaded.groups, projection) : Multivariate18Fixture.Blocks(before, after)
        Dim a As New ProcessListofResultTables(model.wrapResults()), b As New ProcessListofResultTables(projection.ProjectReport(model.wrapResults()).LegacyTables()), oldResults As New Multivariate18Fixture.MemoryWriter, newResults As New Multivariate18Fixture.MemoryWriter
        a.writeToSheet(oldResults, True) : b.writeToSheet(newResults, True) : Multivariate18Fixture.Blocks(oldResults, newResults)
        Assert.IsFalse(request.DataSection.Headers.Contains("OriginalRow")) : Assert.AreEqual("Discriminant results", request.ResultsSheetName)
        Assert.AreEqual(id Mod 2 = 1, request.FirstRowSkipped) : Assert.AreEqual(1, request.FirstPredictorSourceColumn)
        Assert.AreEqual(input.Columns(1).SourceKey, projection.Variables(0).SourceKey) : Assert.AreEqual(0, projection.Plots.Count)
    End Sub

    <DataTestMethod><DataRow(0)><DataRow(1)><DataRow(2)><DataRow(3)><DataRow(4)>
    Public Sub FrozenHostConversionKeepsCDblNothingCultureAndSourceColumnOffset(id As Integer)
        Dim saved = CultureInfo.CurrentCulture
        Try
            For Each culture In {"", "sk-SK", "de-DE"}
                CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(culture)
                Dim raw(,) As Object = {{"a", 1, CDec(2.5)}, {"A", Nothing, "1,5"}, {"a", Double.NaN, 3.0}}
                If id = 1 Then raw(0, 1) = "bad"
                If id = 2 Then raw(0, 1) = Double.PositiveInfinity
                If id = 3 Then raw(0, 1) = Double.NegativeInfinity
                Dim data = Multivariate18Fixture.Acquired(raw, {"group", "second", "first"})
                If id = 4 Then data.nCols = 1
                Dim converted(,) As Double = Nothing, errorValue = Multivariate19Fixture.Failure(Sub() converted = LegacyMultivariate20HostConversion.Predictors(data))
                If id = 1 OrElse id = 4 Then
                    Assert.IsNotNull(errorValue) : Continue For
                End If
                Assert.IsNull(errorValue) : Assert.AreEqual(CDbl(raw(0, 1)), converted(0, 0))
                Assert.AreEqual(CDbl("1,5"), converted(1, 1)) : Assert.IsTrue(Double.IsNaN(converted(1, 0)))
                CollectionAssert.AreEqual(New String() {"second", "first"}, LegacyMultivariate20HostConversion.Names(data))
                Dim input = WindowsMultivariateWorkflowAdapter.Capture(MultivariateAnalysisKind.Discriminant, data), request As New MultivariateDiscriminantRequest(input, New MultivariateDiscriminantOptions)
                Assert.AreEqual("second", request.Predictors(0).Name) : Assert.AreEqual(1, request.Predictors(0).SourcePosition.Value)
            Next
        Finally
            CultureInfo.CurrentCulture = saved
        End Try
    End Sub
End Class

Public NotInheritable Class LegacyMultivariate20HostConversion
    Public Shared Function Predictors(MyData As DataObj) As Double(,)
        If MyData.nCols < 2 Then
            Throw New ArgumentException("Discriminant analysis requires at least one predictor variable.")
        End If

        Dim out(MyData.nRows - 1, MyData.nCols - 2) As Double
        For i As Integer = 0 To MyData.nRows - 1
            For j As Integer = 1 To MyData.nCols - 1
                If MyData.FinalData(i, j) Is Nothing Then
                    out(i, j - 1) = Double.NaN
                Else
                    out(i, j - 1) = CDbl(MyData.FinalData(i, j))
                End If
            Next
        Next
        Return out
    End Function
    Public Shared Function Names(MyData As DataObj) As String()
        If MyData.nCols < 2 Then
            Throw New ArgumentException("Discriminant analysis requires at least one predictor variable.")
        End If

        Dim out(MyData.nCols - 2) As String
        For j As Integer = 1 To MyData.nCols - 1
            out(j - 1) = MyData.varNames(j)
        Next
        Return out
    End Function
End Class
