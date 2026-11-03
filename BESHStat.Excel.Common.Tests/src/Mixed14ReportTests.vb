Option Explicit On
Option Strict On
Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports BESHStatNG
Imports BESHStatNG.ExcelCommon
Imports BESHStatNG.regression
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass>
Public Class Mixed14ReportTests
    <DataTestMethod><DataRow(False, False)><DataRow(False, True)><DataRow(True, False)><DataRow(True, True)>
    Public Sub SupplementalOrderPreservesObservedVersusReferenceMeaning(reference As Boolean, grouped As Boolean)
        Dim options As New MixedModelReportOptions With {.MeansMode = If(reference, MixedModelMeansMode.ReferenceGrid, MixedModelMeansMode.ObservedDesign)}
        Dim plan = MixedModelReportPolicy.Plan(MixedModelWorkflowKind.MMRM, options, grouped)
        Dim expected As New List(Of MixedModelSupplementalSection) From {MixedModelSupplementalSection.ClassInformation}
        If reference Then
            expected.Add(MixedModelSupplementalSection.ReferenceMeans)
            If grouped Then expected.Add(MixedModelSupplementalSection.ReferenceGroupContrasts)
            Assert.IsFalse(plan.Includes(MixedModelSupplementalSection.ChangeByVisit), "Reference-grid mode does not invent observed change tables.")
        Else
            expected.AddRange({MixedModelSupplementalSection.MeansByVisit, MixedModelSupplementalSection.ChangeByVisit})
            If grouped Then expected.AddRange({MixedModelSupplementalSection.MeansByVisitAndGroup, MixedModelSupplementalSection.GroupDifferences,
                MixedModelSupplementalSection.ChangeByGroup, MixedModelSupplementalSection.DifferenceInChange})
        End If
        CollectionAssert.AreEqual(expected.ToArray(), plan.Sections.ToArray())
        options.EstimatedMeans = False
        CollectionAssert.AreEqual(New MixedModelSupplementalSection() {MixedModelSupplementalSection.ClassInformation}, MixedModelReportPolicy.Plan(MixedModelWorkflowKind.MMRM, options, grouped).Sections.ToArray())
        Assert.AreEqual(1, MixedModelReportPolicy.Plan(MixedModelWorkflowKind.LMM, New MixedModelReportOptions, grouped).Sections.Count)
        Assert.AreEqual(0, MixedModelReportPolicy.Plan(MixedModelWorkflowKind.MMRM, New MixedModelReportOptions, grouped, False).Sections.Count)
    End Sub

    <DataTestMethod><DataRow(0)><DataRow(1)><DataRow(2)><DataRow(3)>
    Public Sub ContrastSelectionsAndOptionalChangeTablesStaySeparate(code As Integer)
        Dim o As New MixedModelReportOptions With {.Contrast = CType(code, MixedModelContrastSelection), .ChangeFromBaseline = False}
        Dim plan = MixedModelReportPolicy.Plan(MixedModelWorkflowKind.MMRM, o, True)
        Assert.AreEqual(code <> 0, plan.Includes(MixedModelSupplementalSection.GroupDifferences))
        Assert.IsFalse(plan.Includes(MixedModelSupplementalSection.DifferenceInChange))
        Assert.IsFalse(plan.Includes(MixedModelSupplementalSection.ChangeByGroup))
        Assert.IsFalse(plan.Includes(MixedModelSupplementalSection.ChangeByVisit))
        o.ChangeFromBaseline = True : o.DifferenceInChange = False
        Assert.IsFalse(MixedModelReportPolicy.Plan(MixedModelWorkflowKind.MMRM, o, True).Includes(MixedModelSupplementalSection.DifferenceInChange))
    End Sub

    <DataTestMethod><DataRow(False, False)><DataRow(False, True)><DataRow(True, False)><DataRow(True, True)>
    Public Sub ReferenceGridOptionsAndLinearEstimatesUseOriginalCore(observedWeights As Boolean, userValue As Boolean)
        Dim visits = New Double() {0, 1, 0, 1, 0, 1, 0, 1}, groups = New Double() {0, 0, 1, 1, 1, 1, 1, 1}
        Dim site = New Double() {0, 2, 0, 0, 0, 0, 2, 2}, age = New Double() {8, 9, 10, 11, 12, 13, 14, 15}
        Dim names = New String() {"Intercept", "visit", "group", "site", "age", "visit:group"}
        Dim o As New MixedModelReportOptions With {.Alpha = 0.1,
            .Weighting = If(observedWeights, MixedModelReferenceGridWeighting.ObservedCellFrequency, MixedModelReferenceGridWeighting.EqualCells),
            .CovariateMode = If(userValue, MixedModelCovariateReferenceMode.UserSpecified, MixedModelCovariateReferenceMode.ObservedMean),
            .Multiplicity = MixedModelMultiplicityAdjustment.Holm}
        Dim intent As New MixedModelReferenceIntent(names, "visit", visits, "group", groups, o), actual = intent.CreateCoreSpec()
        Dim expected As New MixedModelReferenceGridSpec With {.FixedEffectNames = names, .Alpha = 0.1, .Weighting = o.Weighting, .MultiplicityAdjustment = o.Multiplicity}
        expected.AddByFactor("visit", {0.0, 1.0}, visits) : expected.AddByFactor("group", {0.0, 1.0}, groups)
        For Each spec In {expected, actual}
            spec.AddMarginalFactor("site", {0.0, 2.0}, site)
            If userValue Then spec.AddCovariateValue("age", age, 0.0) Else spec.AddCovariateMean("age", age)
        Next
        Dim a = MixedModelReferenceGridService.BuildReferenceGridRows(expected), b = MixedModelReferenceGridService.BuildReferenceGridRows(actual)
        Assert.AreEqual(a.Count, b.Count)
        For i = 0 To a.Count - 1
            Assert.AreEqual(a(i).Label, b(i).Label) : CollectionAssert.AreEqual(a(i).L, b(i).L)
        Next
        Dim variance(5, 5) As Double
        For i = 0 To 5 : variance(i, i) = 0.2 : Next
        Dim result As New MixedModelResult With {.P = 6, .Nobs = 8, .NoSubjects = 4, .FixedEffectNames = names,
            .Beta = {1.0, 0.3, 0.4, 0.1, 0.05, 0.2}, .VarBeta = variance, .FixedInferenceMethod = MixedModelFixedInferenceMethod.WaldNormal}
        Dim original = MixedModelReferenceGridService.BuildEstimatedMeansTable("Reference-grid estimated marginal means", a, result, expected)
        Dim projected = MixedModelReferenceGridService.BuildEstimatedMeansTable("Reference-grid estimated marginal means", b, result, actual)
        CollectionAssert.AreEqual(Mixed14Fixture.CanonicalTables(New List(Of ResultTable) From {original}), Mixed14Fixture.CanonicalTables(New List(Of ResultTable) From {projected}))
        names(1) = "mutated" : visits(0) = 999 : o.Alpha = -1
        Assert.AreEqual(0.1, intent.CreateCoreSpec().Alpha)
        Assert.AreEqual("visit", intent.CreateCoreSpec().FixedEffectNames(1))
    End Sub

    <DataTestMethod><DataRow("Higher level - lower level", 0, 1.0, 0.0)><DataRow("Treatment - control", 1, 0.0, 1.0)><DataRow("Control - treatment", 2, 1.0, 0.0)>
    Public Sub DirectedContrastSignMatchesOriginalControlVersusComparison(text As String, code As Integer, first As Double, second As Double)
        Assert.AreEqual(CType(code, MixedModelContrastDirection), MixedModelReportPolicy.Direction(text))
        CollectionAssert.AreEqual({first, second}, MixedModelReportPolicy.DirectedPair(CType(code, MixedModelContrastDirection), 1, 0))
        Dim beta = New Double() {1, 2}, firstL = New Double() {1, first}, secondL = New Double() {1, second}
        Dim delta = firstL.Zip(secondL, Function(x, y) x - y).ToArray()
        Assert.AreEqual(2 * (first - second), MixedModelPostEstimation.LinearEstimate(delta, beta), 0.000000000001)
    End Sub

    <TestMethod>
    Public Sub ReportValidationDoesNotSilentlyDefaultInvalidTypedSelections()
        Dim o As New MixedModelReportOptions With {.Alpha = Double.NaN, .MeansMode = CType(99, MixedModelMeansMode),
            .Weighting = CType(99, MixedModelReferenceGridWeighting), .CovariateMode = MixedModelCovariateReferenceMode.UserSpecified,
            .CovariateValue = Double.PositiveInfinity, .Multiplicity = CType(99, MixedModelMultiplicityAdjustment),
            .Contrast = MixedModelContrastSelection.SelectedComparison, .BaselineVisit = 7, .ControlLevel = 2, .ComparisonLevel = 2}
        Dim validation = MixedModelReportPolicy.Validate(o, {0.0, 1.0}, {0.0, 1.0})
        Assert.IsFalse(validation.IsValid)
        For Each field In {"alpha", "option", "covariate", "baseline", "control", "comparison"}
            Assert.IsTrue(validation.Diagnostics.Any(Function(d) d.Code = "mixed.report." & field))
        Next
    End Sub

    <TestMethod>
    Public Sub DisplayChoiceMappingsDefaultsAndFreshReportFlagsAreExact()
        Assert.AreEqual(MixedModelMeansMode.ObservedDesign, MixedModelReportPolicy.MeansMode(Nothing))
        Assert.AreEqual(MixedModelReferenceGridWeighting.EqualCells, MixedModelReportPolicy.Weighting("unknown"))
        Assert.AreEqual(MixedModelReferenceGridWeighting.ObservedCellFrequency, MixedModelReportPolicy.Weighting("Observed class-cell weights"))
        Assert.AreEqual(MixedModelCovariateReferenceMode.ObservedMean, MixedModelReportPolicy.CovariateMode(Nothing))
        For Each name In {"None", "Bonferroni", "Holm", "Sidak"}
            Assert.AreEqual(DirectCast([Enum].Parse(GetType(MixedModelMultiplicityAdjustment), name), MixedModelMultiplicityAdjustment), MixedModelReportPolicy.Multiplicity(name))
        Next
        Assert.IsFalse(MixedModelReportPolicy.ShouldWriteTrace(True, True, " "))
        Assert.IsTrue(MixedModelReportPolicy.ShouldWriteTrace(False, True, "Trace"))
        Assert.IsFalse(MixedModelReportPolicy.ShouldWriteResidual(True, {1.0}, 2))
        Assert.IsTrue(MixedModelReportPolicy.ShouldWriteResidual(True, {1.0}, 1))
        Dim before As New MixedModelReportOptions With {.EstimatedMeans = False}
        Dim saved = MixedModelReportPolicy.Plan(MixedModelWorkflowKind.MMRM, before, True)
        before.EstimatedMeans = True
        Assert.IsFalse(saved.Options.EstimatedMeans)
        Assert.IsTrue(MixedModelReportPolicy.Plan(MixedModelWorkflowKind.MMRM, before, True).Includes(MixedModelSupplementalSection.MeansByVisit))
    End Sub
End Class
