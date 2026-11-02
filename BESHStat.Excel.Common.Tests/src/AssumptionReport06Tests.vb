Option Explicit On
Option Strict On
Imports System
Imports System.Collections.Generic
Imports System.Globalization
Imports System.Linq
Imports System.Threading
Imports System.Threading.Tasks
Imports BESHStatNG
Imports BESHStatNG.ExcelCommon
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass>
Public Class AssumptionReport06Tests
    Public Shared Function Sample(Optional n As Integer = 30) As Double()
        Return Enumerable.Range(1, n).Select(Function(i) Math.Sin(i * 1.7) + i * 0.11 + (i Mod 5) * 0.2).Reverse().ToArray()
    End Function
    Private Shared Function Input(Optional n As Integer = 30) As AssumptionReportInput
        Return New AssumptionReportInput({Sample(n), Sample(n + 2)}, {"Z'eta", "0"})
    End Function
    <DataTestMethod>
    <DataRow(2)> <DataRow(3)> <DataRow(4)> <DataRow(8)> <DataRow(9)>
    <DataRow(4999)> <DataRow(5000)> <DataRow(5001)>
    Public Sub ShippingEligibilityAndNineRows(n As Integer)
        Dim original = Sample(n)
        Dim result = AssumptionReportPolicy.Build(New AssumptionReportInput({original}, {"Name"}), New AssumptionReportOptions(AssumptionReportKind.Normality))
        Dim eligible As New NormalityReportEligibility(n)
        Assert.AreEqual(n > 3 AndAlso n < 5000, eligible.Shapiro)
        Assert.AreEqual(n >= 9, eligible.DAgostino)
        Assert.AreEqual(n > 1, eligible.AndersonDarling)
        If n >= 4999 Then
            'Existing Core integer denominator overflows; no arithmetic correction is authorized.
            Assert.ThrowsException(Of OverflowException)(Sub() assumptions.DAgostino(DirectCast(original.Clone(), Double()), ""))
            Assert.IsFalse(result.Success)
            Assert.AreEqual(0, result.Tables.Count)
            Assert.AreEqual("ASSUMPTION_CORE_FAILURE", result.Validation.Issues.Last().Code)
            Assert.ThrowsException(Of OverflowException)(Sub() result.ThrowIfFailed())
            Return
        End If
        Assert.IsTrue(result.Success, String.Join("; ", result.Validation.Issues.Select(Function(i) i.Code & ": " & i.Message)))
        Dim table = result.Tables(0).ToLegacyTable().ToOutputModel()
        Assert.AreEqual(10, table.RowCount)
        Assert.AreEqual(2, table.ColumnCount)
        Assert.AreEqual("Normality Tests", table.Values(0, 0))
        Assert.AreEqual("Name", table.Values(0, 1))
        CollectionAssert.AreEqual(New Integer() {3, 6, 9}, table.PvalueCells.Select(Function(c) c.BodyRow).ToArray())
        Assert.IsTrue(table.PvalueCells.All(Function(c) c.BodyColumn = 2))
        If n > 3 AndAlso n < 5000 Then
            Dim err As String = Nothing
            Dim expected = assumptions.ShapiroWilk(DirectCast(original.Clone(), Double()), err)
            Assert.AreEqual(expected.TestStatistics1, CDbl(table.Values(2, 1)), 1.0E-12)
            Assert.AreEqual(expected.Pvalue, CDbl(table.Values(3, 1)), 1.0E-12)
        Else
            Assert.AreEqual("NA n<4 or n>5000", table.Values(1, 1))
            Assert.IsTrue(result.Validation.Issues.Any(Function(i) i.Code = "NORMALITY_SW_NOT_APPLICABLE"))
        End If
        If n >= 9 Then
            Dim expected = assumptions.DAgostino(DirectCast(original.Clone(), Double()), "")
            Assert.AreEqual(expected.Pvalue, CDbl(table.Values(6, 1)), 1.0E-12)
        Else
            Assert.AreEqual("NA n<9", table.Values(4, 1))
        End If
        Dim ad = assumptions.AndersonDarlingTEST(DirectCast(original.Clone(), Double()))
        Assert.AreEqual(ad.Pvalue, CDbl(table.Values(9, 1)), 1.0E-12)
        CollectionAssert.AreEqual(Sample(n), original)
    End Sub

    <DataTestMethod>
    <DataRow(False, False)> <DataRow(True, False)> <DataRow(False, True)> <DataRow(True, True)>
    Public Sub SupplementPlanAndImmutability(descriptive As Boolean, box As Boolean)
        Dim source = Input()
        Dim before = source.Groups
        Dim result = AssumptionReportPolicy.Build(source, New AssumptionReportOptions(AssumptionReportKind.Normality, descriptive:=descriptive, boxSummary:=box))
        Assert.IsTrue(result.Success)
        Assert.AreEqual(1 + If(descriptive, 1, 0), result.Tables.Count)
        Assert.AreEqual(1 + If(descriptive, 1, 0) + If(box, 1, 0), result.Stages.Count)
        Assert.AreEqual(1, result.BlankRowsBetweenTables)
        If box Then Assert.AreEqual(AssumptionOutputStage.WindowsBoxSummaryAndChart, result.Stages.Last())
        Dim artifact = result.Tables(0).ToArtifact(0)
        Dim copy = artifact.Model
        copy.Values(0, 0) = "mutated"
        Assert.AreEqual("Normality Tests", artifact.Model.Values(0, 0))
        Dim scratch = result.LegacyWorkingGroups
        scratch(0)(0) = 10000
        CollectionAssert.AreEqual(before(0), source.Groups(0))
        Assert.AreNotEqual(10000.0, result.LegacyWorkingGroups(0)(0))
    End Sub

    <DataTestMethod>
    <DataRow("n")> <DataRow("mean")> <DataRow("median")> <DataRow("cv")>
    <DataRow("variance")> <DataRow("sd")> <DataRow("sem")> <DataRow("skewness")>
    <DataRow("kurtosis")> <DataRow("q1")> <DataRow("q3")> <DataRow("iqr")>
    <DataRow("minimum")> <DataRow("maximum")> <DataRow("range")> <DataRow("swstat")> <DataRow("swpvalue")>
    Public Sub EveryDescriptiveSelectionUsesCore(key As String)
        Dim inputData = Input()
        Dim result = AssumptionReportPolicy.Build(inputData, New AssumptionReportOptions(AssumptionReportKind.Descriptive, statistics:={key}))
        Assert.IsTrue(result.Success)
        Dim table = result.Tables(0).ToLegacyTable().ToOutputModel()
        Dim core As New DescriptiveStat(inputData.Groups(0))
        core.compute()
        Dim expected = core.wrapSelf(True, New List(Of String) From {key})
        Assert.AreEqual(expected(0, 0), table.Values(2, 0))
        Assert.AreEqual(expected(0, 1), table.Values(2, 1))
        Assert.AreEqual("Z'eta", table.Values(1, 1))
        Assert.AreEqual(If(key = "swpvalue", 2, 0), table.PvalueCells.Count)
    End Sub

    <DataTestMethod>
    <DataRow(1)> <DataRow(2)> <DataRow(4)> <DataRow(8)> <DataRow(15)>
    Public Sub HomogeneityCoreMethodsAndOrdering(bits As Integer)
        Dim inputData = Input()
        Dim result = AssumptionReportPolicy.Build(inputData, New AssumptionReportOptions(AssumptionReportKind.Homogeneity, homogeneity:=CType(bits, HomogeneityMethods)))
        Assert.IsTrue(result.Success)
        Dim table = result.Tables(0).ToLegacyTable().ToOutputModel()
        Dim index = 0
        For Each flag In {1, 2, 4, 8}
            If (bits And flag) = 0 Then Continue For
            Dim expected As BESHStatNG.TestResult
            Dim label As String
            Select Case flag
                Case 1 : expected = assumptions.FlignerKilleenTEST(inputData.Groups) : label = "Fligner-Killeen"
                Case 2 : expected = assumptions.LeveneTEST(inputData.Groups, True) : label = "Levene"
                Case 4 : expected = assumptions.SquaredRanksTestVARIANCE(inputData.Groups) : label = "Squared Ranks"
                Case Else : expected = assumptions.BartlettTEST(inputData.Groups) : label = "Bartlett"
            End Select
            Assert.AreEqual(label, table.Values(1 + 3 * index, 0))
            Assert.AreEqual(expected.TestStatistics1, CDbl(table.Values(2 + 3 * index, 1)), 1.0E-12)
            Assert.AreEqual(expected.Pvalue, CDbl(table.Values(3 + 3 * index, 1)), 1.0E-12)
            Assert.AreEqual(3 * (index + 1), table.PvalueCells(index).BodyRow)
            index += 1
        Next
    End Sub

    <DataTestMethod> <DataRow(AssumptionOutlierMethod.Grubbs, 30)> <DataRow(AssumptionOutlierMethod.Rosner, 30)>
    <DataRow(AssumptionOutlierMethod.Rosner, 14)> <DataRow(AssumptionOutlierMethod.Rosner, 20)>
    Public Sub OutlierValuesAndFixedPadding(method As AssumptionOutlierMethod, n As Integer)
        Dim values = Sample(n)
        values(0) = 1000
        Dim inputData As New AssumptionReportInput({values}, {"outlier"})
        Dim result = AssumptionReportPolicy.Build(inputData, New AssumptionReportOptions(AssumptionReportKind.Outliers, outlierMethod:=method))
        Assert.IsTrue(result.Success)
        Dim model = result.Tables(0).ToLegacyTable().ToOutputModel()
        Assert.AreEqual(0.05D, model.Values(1, 1))
        If method = AssumptionOutlierMethod.Grubbs Then
            Dim core = assumptions.Grubbs(DirectCast(values.Clone(), Double()), 0.05)
            Assert.AreEqual(core.TestStatistics1, model.Values(2, 1))
            Assert.AreEqual(core.TestStatistics2, model.Values(3, 1))
            Assert.AreEqual(core.strSpecialInformation, model.Values(4, 1))
        Else
            Assert.AreEqual(14, model.RowCount)
            Dim core = assumptions.Rosner(DirectCast(values.Clone(), Double()), 0.05)
            Assert.AreEqual(If(core Is Nothing, 0, core.Length), model.Values(2, 1))
            Assert.AreEqual("List of Outliers:", model.Values(3, 1))
            If core IsNot Nothing Then Assert.AreEqual(core(0), model.Values(4, 1))
            If n < 15 Then Assert.IsTrue(result.Validation.HasWarnings)
        End If
        CollectionAssert.AreEqual(values, inputData.Groups(0))
    End Sub

    <DataTestMethod> <DataRow(AssumptionSymmetryMethod.MiaoGelGastwirth)> <DataRow(AssumptionSymmetryMethod.CabilioMasaro)>
    Public Sub SymmetryCoreAndConstantZeroAreNumeric(method As AssumptionSymmetryMethod)
        Dim result = AssumptionReportPolicy.Build(New AssumptionReportInput({New Double() {3, 3, 3, 3}}, {"constant"}),
            New AssumptionReportOptions(AssumptionReportKind.Symmetry, symmetry:=method))
        Assert.IsTrue(result.Success)
        Dim model = result.Tables(0).ToLegacyTable().ToOutputModel()
        Assert.AreEqual(0.0, CDbl(model.Values(2, 1)))
        Assert.AreEqual(1.0, CDbl(model.Values(3, 1)))
    End Sub

    <DataTestMethod> <DataRow(0)> <DataRow(1)> <DataRow(2)> <DataRow(3)> <DataRow(4)> <DataRow(5)>
    Public Sub InvalidInputsAreAtomicAndStructured(which As Integer)
        Dim inputData = Input()
        Dim options As New AssumptionReportOptions(AssumptionReportKind.Normality)
        Dim code As String
        Select Case which
            Case 0 : inputData = Nothing : code = "ASSUMPTION_SHAPE"
            Case 1 : inputData = New AssumptionReportInput({Array.Empty(Of Double)()}, {"x"}) : code = "ASSUMPTION_EMPTY_GROUP"
            Case 2 : inputData = New AssumptionReportInput({New Double() {1, Double.NaN}}, {"x"}) : code = "ASSUMPTION_NONFINITE"
            Case 3 : options = New AssumptionReportOptions(CType(99, AssumptionReportKind)) : code = "ASSUMPTION_OPTIONS"
            Case 4 : options = New AssumptionReportOptions(AssumptionReportKind.Outliers, alpha:=0D) : code = "ASSUMPTION_ALPHA"
            Case Else : inputData = New AssumptionReportInput({New Double() {1}}, {"x", "y"}) : code = "ASSUMPTION_SHAPE"
        End Select
        Dim result = AssumptionReportPolicy.Build(inputData, options)
        Assert.IsFalse(result.Success)
        Assert.AreEqual(0, result.Tables.Count)
        Assert.IsTrue(result.Validation.Issues.Any(Function(i) i.Code = code))
    End Sub

    <TestMethod>
    Public Sub SourceCoordinatesStayWithOriginalValuesAndFailureDoesNotWrite()
        Dim sources = {New UnivariateObservationSource(0, 0, 2, 1, 103, 5, "book", "sheet", "S", "E101:E107"),
            New UnivariateObservationSource(0, 1, 4, 1, 107, 5, "book", "sheet", "S", "E101:E107")}
        Dim inputData As New AssumptionReportInput({New Double() {2, Double.PositiveInfinity}}, {"x"}, sources)
        Dim result = AssumptionReportPolicy.Build(inputData, New AssumptionReportOptions(AssumptionReportKind.Normality))
        Assert.IsFalse(result.Success)
        Dim issue = result.Validation.Issues.Single()
        Assert.AreEqual("ASSUMPTION_NONFINITE", issue.Code)
        Assert.AreEqual(107, issue.WorksheetRow)
        Assert.AreEqual(4, issue.SelectedRow)
        Assert.AreEqual(103, inputData.Sources(0).WorksheetRow)
        Assert.AreEqual(0, result.Tables.Count) 'only successful tables can reach a writer
    End Sub

    <TestMethod>
    Public Sub CoreNullFailureCancellationAndNoOutputAreDistinct()
        Dim constantData As New AssumptionReportInput({New Double() {4, 4, 4, 4}}, {"x"})
        Dim failed = AssumptionReportPolicy.Build(constantData, New AssumptionReportOptions(AssumptionReportKind.Normality))
        Assert.IsFalse(failed.Success)
        Assert.AreEqual("ASSUMPTION_CORE_FAILURE", failed.Validation.Issues.Last().Code)
        Assert.AreEqual(0, failed.Tables.Count)
        Assert.ThrowsException(Of NullReferenceException)(Sub() failed.ThrowIfFailed())
        Dim cancelled = AssumptionReportPolicy.Build(Input(), New AssumptionReportOptions(AssumptionReportKind.Normality), New CancellationToken(True))
        Assert.AreEqual("ASSUMPTION_CANCELLED", cancelled.Validation.Issues.Last().Code)
        Assert.AreEqual(0, cancelled.Tables.Count)
        Dim empty = AssumptionReportPolicy.Build(Input(), New AssumptionReportOptions(AssumptionReportKind.Homogeneity))
        Assert.IsTrue(empty.Success)
        Assert.AreEqual("ASSUMPTION_NO_OUTPUT", empty.Validation.Issues.Single().Code)
    End Sub

    <DataTestMethod> <DataRow(1)> <DataRow(2)> <DataRow(4)> <DataRow(8)>
    Public Sub ConstantHomogeneityAndSingleGroupExposeRawCoreOutcome(bits As Integer)
        Dim groups = New Double()() {New Double() {2, 2, 2, 2}, New Double() {3, 3, 3, 3, 3}}
        Dim result = AssumptionReportPolicy.Build(New AssumptionReportInput(groups, {"same", "same"}),
            New AssumptionReportOptions(AssumptionReportKind.Homogeneity, homogeneity:=CType(bits, HomogeneityMethods)))
        'Core may fail or return undefined numeric cells; neither becomes a valid zero/NA.
        If result.Success Then
            Dim values = result.Tables(0).ToArtifact(0).Model.Values
            If Double.IsNaN(CDbl(values(2, 1))) OrElse Double.IsNaN(CDbl(values(3, 1))) OrElse Double.IsInfinity(CDbl(values(2, 1))) Then
                Assert.IsTrue(result.Validation.Issues.Any(Function(i) i.Code = "ASSUMPTION_UNDEFINED_RESULT"))
            End If
        Else
            Assert.AreEqual(0, result.Tables.Count)
            Assert.AreEqual("ASSUMPTION_CORE_FAILURE", result.Validation.Issues.Last().Code)
        End If
        Dim singleGroup = AssumptionReportPolicy.Build(New AssumptionReportInput({Sample()}, {"single"}),
            New AssumptionReportOptions(AssumptionReportKind.Homogeneity, homogeneity:=CType(bits, HomogeneityMethods)))
        Assert.IsTrue(singleGroup.Success OrElse singleGroup.Tables.Count = 0)
    End Sub

    <TestMethod>
    Public Sub SingleObservationNAAndFreshGroupDiagnosticIsolation()
        Dim one = AssumptionReportPolicy.Build(New AssumptionReportInput({New Double() {5}}, {"one"}), New AssumptionReportOptions(AssumptionReportKind.Normality))
        Assert.IsTrue(one.Success)
        Assert.AreEqual(3, one.Validation.Issues.Count)
        Assert.AreEqual("NA", one.Tables(0).ToArtifact(0).Model.Values(9, 1))
        Dim failed = AssumptionReportPolicy.Build(New AssumptionReportInput({New Double() {2, 2, 2, 2}, Sample()}, {"bad", "good"}), New AssumptionReportOptions(AssumptionReportKind.Normality))
        Assert.IsFalse(failed.Success)
        Assert.IsTrue(failed.Validation.Issues.Any(Function(i) i.Code = "ASSUMPTION_METHOD_FAILURE" AndAlso i.FieldPath = "groups[0].shapiro"))
        Dim fresh = AssumptionReportPolicy.Build(Input(), New AssumptionReportOptions(AssumptionReportKind.Normality))
        Assert.IsTrue(fresh.Success)
        Assert.IsFalse(fresh.Validation.HasErrors)
    End Sub

    <DataTestMethod> <DataRow(0)> <DataRow(1)> <DataRow(2)>
    Public Sub InvalidSourceMapsFailWithoutFabricatingCoordinates(which As Integer)
        Dim src = New UnivariateObservationSource(If(which = 0, 1, 0), 0, If(which = 1, -2, 1), 1, 101, 4, "book", "sheet", "S", "D101:D107")
        Dim sources = If(which = 2, New UnivariateObservationSource() {src, src}, {src})
        Dim result = AssumptionReportPolicy.Build(New AssumptionReportInput({New Double() {1, 2, 3}}, {"x"}, sources), New AssumptionReportOptions(AssumptionReportKind.Normality))
        Assert.IsFalse(result.Success)
        Assert.IsTrue(result.Validation.Issues.Any(Function(i) i.Code = "ASSUMPTION_SOURCE_MAP"))
    End Sub

    <TestMethod>
    Public Sub NoncontiguousSourcesStayOriginalAfterScratchSortAndNamesNeverReorder()
        Dim values = New Double() {10, 2, 8, 3, 4}
        Dim rows = New Integer() {101, 103, 107, 109, 111}
        Dim sources = rows.Select(Function(row, index) New UnivariateObservationSource(0, index, index + 1, 1, row, 4, "book", "sheet", "S", "D101:D111")).ToArray()
        Dim inputData As New AssumptionReportInput({values, Sample()}, {"same", "same"}, sources)
        Dim result = AssumptionReportPolicy.Build(inputData, New AssumptionReportOptions(AssumptionReportKind.Normality))
        Assert.IsTrue(result.Success)
        CollectionAssert.AreEqual(rows, inputData.Sources.Select(Function(s) s.WorksheetRow).ToArray())
        CollectionAssert.AreEqual(values, inputData.Groups(0))
        CollectionAssert.AreEqual(values.OrderBy(Function(v) v).ToArray(), result.LegacyWorkingGroups(0))
        Dim model = result.Tables(0).ToArtifact(0).Model
        Assert.AreEqual("same", model.Values(0, 1))
        Assert.AreEqual("same", model.Values(0, 2))
    End Sub

    <DataTestMethod> <DataRow(-0.05)> <DataRow(0.0)> <DataRow(1.0)> <DataRow(2.0)>
    Public Sub AlphaIsExplicitAndNotLocalized(value As Double)
        Dim result = AssumptionReportPolicy.Build(Input(), New AssumptionReportOptions(AssumptionReportKind.Outliers, outlierMethod:=AssumptionOutlierMethod.Grubbs, alpha:=CDec(value)))
        Assert.IsFalse(result.Success)
        Assert.AreEqual("ASSUMPTION_ALPHA", result.Validation.Issues.Single().Code)
    End Sub

    <DataTestMethod> <DataRow("")> <DataRow("sk-SK")>
    Public Sub CulturesAndFreshConcurrentInstances(culture As String)
        Dim saved = Thread.CurrentThread.CurrentCulture
        Try
            Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo(culture)
            Dim requests = Enumerable.Range(1, 6).Select(Function(i) Task.Run(Function() AssumptionReportPolicy.Build(Input(), New AssumptionReportOptions(AssumptionReportKind.Normality, descriptive:=True)))).ToArray()
            Task.WaitAll(requests)
            Dim model = requests(0).Result.Tables(0).ToLegacyTable().ToOutputModel()
            For Each request In requests
                Assert.IsTrue(request.Result.Success)
                Assert.AreEqual(model.Values(2, 1), request.Result.Tables(0).ToLegacyTable().ToOutputModel().Values(2, 1))
                Assert.IsInstanceOfType(model.Values(2, 1), GetType(Double))
            Next
        Finally
            Thread.CurrentThread.CurrentCulture = saved
        End Try
    End Sub
End Class
