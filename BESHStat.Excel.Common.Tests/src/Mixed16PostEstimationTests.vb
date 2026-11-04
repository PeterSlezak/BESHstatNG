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

Public NotInheritable Class Mixed16Fixture
    Public Visit, Groups, Site, Age As Double()
    Public X As Double(,)
    Public Names As String() = {"Intercept", "visit", "group", "site", "age", "visit:group"}
    Public Rows As Integer()
    Public Shared Function Create() As Mixed16Fixture
        Dim f As New Mixed16Fixture, v As New List(Of Double), g As New List(Of Double), s As New List(Of Double), a As New List(Of Double)
        For i = 0 To 35
            If i = 5 OrElse i = 21 OrElse i = 30 Then Continue For
            v.Add(CDbl(2 - i \ 12)) : g.Add(CDbl((i Mod 12) Mod 3))
            s.Add(If(i Mod 4 = 0, 2.0, 0.0)) : a.Add(8.0 + (i Mod 12))
        Next
        f.Visit = v.ToArray() : f.Groups = g.ToArray() : f.Site = s.ToArray() : f.Age = a.ToArray()
        f.Rows = Enumerable.Range(0, v.Count).Select(Function(i) 101 + i * 3).ToArray()
        ReDim f.X(v.Count - 1, 5)
        For i = 0 To v.Count - 1
            f.X(i, 0) = 1 : f.X(i, 1) = v(i) : f.X(i, 2) = g(i) : f.X(i, 3) = s(i) : f.X(i, 4) = a(i) : f.X(i, 5) = v(i) * g(i)
        Next
        Return f
    End Function
    Public Function Result(Optional inference As MixedModelFixedInferenceMethod = MixedModelFixedInferenceMethod.WaldNormal) As MixedModelResult
        Dim variance(5, 5) As Double
        For i = 0 To 5 : variance(i, i) = 0.2 : Next
        Return New MixedModelResult With {.Converged = True, .P = 6, .Nobs = Visit.Length, .NoSubjects = 12,
            .FixedEffectNames = DirectCast(Names.Clone(), String()), .Beta = {1.0, 0.3, 0.4, 0.1, 0.05, 0.2}, .VarBeta = variance,
            .BetaSE = Enumerable.Repeat(Math.Sqrt(0.2), 6).ToArray(), .BetaDF = Enumerable.Repeat(20.0, 6).ToArray(),
            .FixedInferenceMethod = inference, .BetaStatisticLabel = If(inference = MixedModelFixedInferenceMethod.WaldNormal, "z", "t")}
    End Function
    Public Function Intent(o As MixedModelReportOptions, Optional grouped As Boolean = True) As MixedModelPostEstimationIntent
        Return New MixedModelPostEstimationIntent(MixedModelWorkflowKind.MMRM, Names, "visit | VarC", Visit,
            grouped, "group | VarD", Groups, o, Rows)
    End Function
    Public Function LegacySpec(o As MixedModelReportOptions, grouped As Boolean) As MixedModelReferenceGridSpec
        Dim spec As New MixedModelReferenceGridSpec With {.FixedEffectNames = DirectCast(Names.Clone(), String()), .Alpha = o.Alpha, .Weighting = o.Weighting, .MultiplicityAdjustment = o.Multiplicity}
        spec.AddByFactor("visit", MixedModelPostEstimation.UniqueSortedFiniteValues(Visit), Visit)
        If grouped Then
            spec.AddByFactor("group", MixedModelPostEstimation.UniqueSortedFiniteValues(Groups), Groups)
        Else
            spec.AddMarginalFactor("group", MixedModelPostEstimation.UniqueSortedFiniteValues(Groups), Groups)
        End If
        spec.AddMarginalFactor("site", MixedModelPostEstimation.UniqueSortedFiniteValues(Site), Site)
        If o.CovariateMode = MixedModelCovariateReferenceMode.ObservedMean Then spec.AddCovariateMean("age", Age) Else spec.AddCovariateValue("age", Age, 0)
        Return spec
    End Function
    Public Function CurrentSpec(intent As MixedModelPostEstimationIntent) As MixedModelReferenceGridSpec
        Dim spec = intent.Reference.CreateCoreSpec()
        If Not intent.HasGroup Then MixedModelPostEstimationPolicy.AddReferenceColumn(spec,
            New MixedModelReferenceColumn("group | VarD", MixedModelReferenceColumnRole.MarginalFactor, Groups), "visit", Nothing, intent.Options)
        For Each col In {New MixedModelReferenceColumn("site | VarE", MixedModelReferenceColumnRole.MarginalFactor, Site),
                         New MixedModelReferenceColumn("age | VarF", MixedModelReferenceColumnRole.ContinuousCovariate, Age)}
            MixedModelPostEstimationPolicy.AddReferenceColumn(spec, col, "visit", If(intent.HasGroup, "group", Nothing), intent.Options)
        Next
        Return spec
    End Function
    Public Shared Function ClassTable() As ResultTable
        ' Original Windows table layout, independent of the new Common builder.
        Dim t As New ResultTable
        t.AddTitle("Class Level Information") : t.SetBody(New Object(,) {{"subject", 12, "S0 S1"}, {"group", 3, "0 1 2"}})
        t.AddHeaderTopRow({"Class", "Levels", "Values"})
        t.AddFootnote("Class levels are shown for the subject ID and, when available, the grouping factor used by MMRM LS-means/contrast output.")
        Return t
    End Function
    Public Function LegacyTables(result As MixedModelResult, o As MixedModelReportOptions, grouped As Boolean,
                                 Optional xInput(,) As Double = Nothing, Optional visitInput() As Double = Nothing,
                                 Optional namesInput() As String = Nothing) As List(Of ResultTable)
        Dim tables As New List(Of ResultTable), xUse = If(xInput, X), visitsUse = If(visitInput, Visit)
        Dim legacy As New LegacyMixed16Selection(o)
        Dim add As Action(Of Func(Of ResultTable)) = Sub(factory)
            Try
                Dim t = factory()
                If t IsNot Nothing Then tables.Add(t)
            Catch ex As Exception
                ' Original AddOptionalMMRMTable policy: log and continue, no replacement row.
            End Try
        End Sub
        If o.ClassInformation Then add(Function() ClassTable())
        If Not o.EstimatedMeans Then Return tables
        Dim baseline = legacy.ResolveSelectedBaselineVisit(visitsUse), control = If(grouped, legacy.ResolveSelectedControlLevel(Groups), Double.NaN)
        Dim mode = MixedModelPostEstimationPolicy.ContrastText(o), direction = MixedModelPostEstimationPolicy.DirectionText(o)
        If o.MeansMode = MixedModelMeansMode.ReferenceGrid Then
            Dim spec = LegacySpec(o, grouped), rows = MixedModelReferenceGridService.BuildReferenceGridRows(spec)
            add(Function() MixedModelReferenceGridService.BuildEstimatedMeansTable("Reference-grid estimated marginal means", rows, result, spec))
            If grouped AndAlso o.Contrast <> MixedModelContrastSelection.None Then add(Function() legacy.BuildMMRMReferenceGridGroupContrastsTable(result, rows, spec, "group | VarD", mode, control, direction))
            Return tables
        End If
        add(Function() MMRMPostEstimation.BuildEstimatedMeansByVisitTable(result, xUse, visitsUse, o.Alpha))
        If o.ChangeFromBaseline Then add(Function() MMRMPostEstimation.BuildChangeFromBaselineTableControlled(result, xUse, visitsUse, baseline, o.Alpha))
        If grouped Then
            add(Function() MMRMPostEstimation.BuildEstimatedMeansByVisitAndGroupTable(result, xUse, visitsUse, Groups, "group | VarD", o.Alpha))
            If o.Contrast <> MixedModelContrastSelection.None Then add(Function() legacy.BuildMMRMVisitGroupDifferencesTableControlled(result, xUse, visitsUse, Groups, "group | VarD", o.Alpha, mode, control, direction))
            If o.ChangeFromBaseline Then
                add(Function() MMRMPostEstimation.BuildChangeFromBaselineByGroupTableControlled(result, xUse, visitsUse, Groups, "group | VarD", baseline, o.Alpha))
                If o.Contrast <> MixedModelContrastSelection.None AndAlso o.DifferenceInChange Then add(Function() legacy.BuildMMRMDifferenceInChangeFromBaselineTableControlled(result, xUse, visitsUse, Groups, "group | VarD", baseline, o.Alpha, mode, control, direction))
            End If
        End If
        Return tables
    End Function
    Public Shared Sub EqualTables(expected As List(Of ResultTable), actual As List(Of ResultTable), key As String)
        Regression10Fixture.AssertCapture(Mixed14Fixture.CanonicalTables(expected), Mixed14Fixture.CanonicalTables(actual), key)
    End Sub
End Class

<TestClass>
Public Class Mixed16PostEstimationTests
    Public Shared Function OptionsMatrix() As IEnumerable(Of Object())
        Return From reference In {False, True} From grouped In {False, True} From contrast In {0, 1, 2, 3}
               From direction In {0, 1, 2} From multiplicity In {0, 1, 2, 3} Select New Object() {reference, grouped, contrast, direction, multiplicity}
    End Function
    <DataTestMethod><DynamicData(NameOf(OptionsMatrix), DynamicDataSourceType.Method)>
    Public Sub OrderedObservedAndReferenceOutputsMatchOriginalCalls(reference As Boolean, grouped As Boolean, contrast As Integer, direction As Integer, multiplicity As Integer)
        Dim saved = CultureInfo.CurrentCulture
        Try
            For Each culture In {"", "sk-SK"}
                CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(culture)
                Dim f = Mixed16Fixture.Create(), result = f.Result(If(multiplicity Mod 2 = 0, MixedModelFixedInferenceMethod.WaldNormal, MixedModelFixedInferenceMethod.ResidualDF))
                Dim o As New MixedModelReportOptions With {.MeansMode = If(reference, MixedModelMeansMode.ReferenceGrid, MixedModelMeansMode.ObservedDesign),
                    .Contrast = CType(contrast, MixedModelContrastSelection), .Direction = CType(direction, MixedModelContrastDirection),
                    .Multiplicity = CType(multiplicity, MixedModelMultiplicityAdjustment), .BaselineSelection = If(direction = 0, "(Smallest)", "1"),
                    .ControlSelection = "1", .ComparisonSelection = If(direction = 2, "0", "(Auto)"), .Alpha = If(multiplicity = 0, 0.05, 0.1),
                    .Weighting = If(multiplicity Mod 2 = 0, MixedModelReferenceGridWeighting.EqualCells, MixedModelReferenceGridWeighting.ObservedCellFrequency),
                    .CovariateMode = If(direction = 1, MixedModelCovariateReferenceMode.UserSpecified, MixedModelCovariateReferenceMode.ObservedMean)}
                Dim intent = f.Intent(o, grouped)
                Dim actual = MixedModelPostEstimationPolicy.ProjectMMRM(result, f.X, intent, Function() Mixed16Fixture.ClassTable(),
                    Function() MixedModelPostEstimationPolicy.ProjectReference(result, f.CurrentSpec(intent), intent))
                Mixed16Fixture.EqualTables(f.LegacyTables(result, o, grouped), actual.LegacyTables(), "matrix")
                Dim repeated = MixedModelPostEstimationPolicy.ProjectMMRM(result, f.X, intent, Function() Mixed16Fixture.ClassTable(), Function() MixedModelPostEstimationPolicy.ProjectReference(result, f.CurrentSpec(intent), intent))
                Mixed16Fixture.EqualTables(actual.LegacyTables(), repeated.LegacyTables(), "repeat")
                Assert.AreEqual(0, result.AdditionalResultTables.Count)
                Assert.AreEqual(actual.Document.Tables.Count, actual.Sections.Count)
                Dim payload = System.Text.Encoding.UTF8.GetBytes(String.Join(ChrW(10), Mixed14Fixture.CanonicalTables(actual.LegacyTables())))
                Using algorithm = System.Security.Cryptography.SHA256.Create()
                    Dim digest = BitConverter.ToString(algorithm.ComputeHash(payload)).Replace("-", "").ToLowerInvariant()
                    Console.WriteLine("04A16_POST|" & String.Join("|", {reference.ToString(), grouped.ToString(), contrast.ToString(CultureInfo.InvariantCulture), direction.ToString(CultureInfo.InvariantCulture),
                        multiplicity.ToString(CultureInfo.InvariantCulture), culture, actual.Document.Tables.Count.ToString(CultureInfo.InvariantCulture), digest}))
                End Using
            Next
        Finally
            CultureInfo.CurrentCulture = saved
        End Try
    End Sub

    Public Shared Function SelectionTexts() As IEnumerable(Of Object())
        Return New String() {"<null>", "", "  ", "(Auto)", "(Smallest)", "(First)", "0", "1", "1.5", "1,5", " 2 ", "-1", "999", "NaN", "Infinity", "unknown"}.Select(Function(s) New Object() {s})
    End Function
    <DataTestMethod><DynamicData(NameOf(SelectionTexts), DynamicDataSourceType.Method)>
    Public Sub SelectedLevelsPreserveOriginalParserOrderAndMissingLevelBehavior(display As String)
        Dim saved = CultureInfo.CurrentCulture, text = If(display = "<null>", Nothing, display)
        Try
            For Each culture In {"", "sk-SK"}
                CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(culture)
                Dim o As New MixedModelReportOptions With {.BaselineSelection = text, .ControlSelection = text, .ComparisonSelection = text}
                Dim original As New LegacyMixed16Selection(o), levels = New Double() {0, 1, 1.5, 2, 2, Double.NaN}
                Assert.AreEqual(original.ResolveSelectedBaselineVisit(levels), MixedModelPostEstimationPolicy.ResolveLevel(levels, text, "(Smallest)"))
                Assert.AreEqual(original.ResolveSelectedControlLevel(levels), MixedModelPostEstimationPolicy.ResolveLevel(levels, text, "(First)"))
                For Each control In {0.0, 1.0, Double.NaN}
                    Dim a, b As Double
                    Assert.AreEqual(original.ResolveSelectedComparisonLevel(levels, control, a), MixedModelPostEstimationPolicy.ResolveComparison(levels, control, text, b))
                    Assert.AreEqual(a, b)
                Next
            Next
        Finally
            CultureInfo.CurrentCulture = saved
        End Try
    End Sub

    <DataTestMethod><DataRow(False, False, False)><DataRow(True, False, True)><DataRow(False, True, False)><DataRow(True, True, True)>
    Public Sub DisabledSectionsAndCopyIsolationAreDetached(means As Boolean, changes As Boolean, classes As Boolean)
        Dim f = Mixed16Fixture.Create(), o As New MixedModelReportOptions With {.EstimatedMeans = means, .ChangeFromBaseline = changes, .ClassInformation = classes}
        Dim intent = f.Intent(o), originalRows = DirectCast(f.Rows.Clone(), Integer())
        o.Alpha = -1 : o.EstimatedMeans = Not means : intent.Options.ClassInformation = Not classes
        f.Rows(0) = -1 : intent.SourceRowIds(0) = -2 : intent.Reference.FixedNames(0) = "mutated"
        CollectionAssert.AreEqual(originalRows, intent.SourceRowIds)
        Assert.AreEqual("Intercept", intent.Reference.FixedNames(0)) : Assert.AreEqual(0.05, intent.Options.Alpha)
        Dim report = MixedModelPostEstimationPolicy.ProjectMMRM(f.Result(), f.X, intent, Function() Mixed16Fixture.ClassTable())
        Assert.AreEqual(classes, report.Sections.Contains(MixedModelSupplementalSection.ClassInformation))
        Assert.AreEqual(means, report.Sections.Contains(MixedModelSupplementalSection.MeansByVisit))
        Assert.AreEqual(means AndAlso changes, report.Sections.Contains(MixedModelSupplementalSection.ChangeByVisit))
        If report.Document.Tables.Count > 0 Then
            Dim table = report.LegacyTables()(0), saved = report.Document.Tables(0).Model.Values(0, 0)
            table.ToOutputModel().Values(0, 0) = "changed"
            report.Document.Tables(0).Model.Values(0, 0) = "changed"
            Assert.AreEqual(saved, report.Document.Tables(0).Model.Values(0, 0))
        End If
    End Sub

    <DataTestMethod><DataRow("class")><DataRow("spec")><DataRow("selected-control")><DataRow("bad-alpha")><DataRow("nonfinite-beta")><DataRow("nonfinite-var")><DataRow("negative-var")><DataRow("no-beta")><DataRow("no-var")><DataRow("cancelled")><DataRow("interrupted")><DataRow("nonconverged")>
    Public Sub OptionalFailuresAndInvalidResultsRemainExplicitWithoutNewNativeRefusal(scenario As String)
        Dim f = Mixed16Fixture.Create(), result = f.Result(), o As New MixedModelReportOptions
        Dim factory As Func(Of ResultTable) = Function() Mixed16Fixture.ClassTable()
        Select Case scenario
            Case "class" : factory = Function() ThrowTable()
            Case "spec" : o.MeansMode = MixedModelMeansMode.ReferenceGrid
            Case "selected-control" : o.Contrast = MixedModelContrastSelection.SelectedComparison : o.ControlSelection = "1" : o.ComparisonSelection = "1"
            Case "bad-alpha" : o.Alpha = Double.NaN
            Case "nonfinite-beta" : result.Beta(0) = Double.NaN
            Case "nonfinite-var" : result.VarBeta(0, 0) = Double.PositiveInfinity
            Case "negative-var" : result.VarBeta(0, 0) = -1
            Case "no-beta" : result.Beta = Nothing
            Case "no-var" : result.VarBeta = Nothing
            Case "cancelled" : result.Converged = False : result.Cancelled = True
            Case "interrupted" : result.Converged = False : result.Interrupted = True
            Case "nonconverged" : result.Converged = False
        End Select
        Dim intent = f.Intent(o), notices As New List(Of MixedModelPostEstimationNotice)
        Dim report = MixedModelPostEstimationPolicy.ProjectMMRM(result, f.X, intent, factory,
            Function() ThrowReference(), Sub(n) notices.Add(n))
        Assert.AreEqual(0, result.AdditionalResultTables.Count)
        If scenario = "class" OrElse scenario = "selected-control" Then
            Assert.IsTrue(report.Document.Warnings.Any(Function(w) w.Code = "mixed.post.table.failed"))
            Assert.IsTrue(report.Sections.Contains(MixedModelSupplementalSection.MeansByVisit))
        ElseIf scenario = "spec" Then
            Assert.AreEqual(1, report.Document.Tables.Count) : Assert.IsTrue(report.Document.Warnings.Any(Function(w) w.Code = "mixed.post.reference.failed"))
        ElseIf scenario = "no-beta" OrElse scenario = "no-var" Then
            Assert.AreEqual(0, report.Document.Tables.Count)
        Else
            Mixed16Fixture.EqualTables(f.LegacyTables(result, o, True), report.LegacyTables(), scenario)
        End If
        Dim status = MixedModelDiagnosticPolicy.Capture(result, o.Alpha)
        If scenario = "bad-alpha" Then Assert.AreEqual(0.05, status.EffectiveAlpha)
        If scenario.StartsWith("nonfinite", StringComparison.Ordinal) OrElse scenario = "negative-var" Then Assert.IsTrue(status.Warnings.Count > 0)
        If scenario = "cancelled" Then Assert.AreEqual(MixedModelFitState.Cancelled, status.State)
        If scenario = "interrupted" Then Assert.AreEqual(MixedModelFitState.Interrupted, status.State)
    End Sub
    Private Shared Function ThrowTable() As ResultTable
        Throw New ApplicationException("Synthetic optional failure")
    End Function
    Private Shared Function ThrowReference() As MixedModelSupplementalReport
        Throw New ArgumentException("Synthetic reference failure")
    End Function

    <DataTestMethod><DynamicData(NameOf(Mixed14Fixture.Cases), GetType(Mixed14Fixture), DynamicDataSourceType.Method)>
    Public Sub ExistingFitsKeepInferenceTablesStatusAndDetachedWorkspaceLifetime(key As String)
        Dim saved = CultureInfo.CurrentCulture
        Try
            For Each culture In {"", "sk-SK"}
                CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(culture)
                CheckExistingFit(key, culture)
            Next
        Finally
            CultureInfo.CurrentCulture = saved
        End Try
    End Sub
    Private Shared Sub CheckExistingFit(key As String, culture As String)
        Dim f = Mixed14Fixture.Load(key), coreRequest = Mixed14Fixture.LegacyRequest(key, f), result = Mixed14Fixture.Fit(key, coreRequest)
        Dim kind = If(key.StartsWith("MMRM|", StringComparison.Ordinal), MixedModelWorkflowKind.MMRM, MixedModelWorkflowKind.LMM)
        Dim before = Mixed14Fixture.CanonicalTables(result.wrapResults(0.05, True, kind = MixedModelWorkflowKind.LMM, True))
        Dim decision = Mixed14Requests.Request(key).Decision
        Dim report = MixedModelReportPolicy.Project(kind, result, New MixedModelReportOptions With {.IterationDetails = True}, decision)
        Assert.AreSame(decision, report.RequestedDecision)
        Regression10Fixture.AssertCapture(before, Mixed14Fixture.CanonicalTables(report.LegacyTables()), key)
        Assert.AreEqual(result.Converged, report.Diagnostics.State = MixedModelFitState.Converged)
        Assert.AreEqual(result.FixedInferenceMethod, report.Diagnostics.Inference.Value)
        Assert.AreEqual(result.FitMethod, report.Diagnostics.FitMethod.Value)
        Dim names = report.Sections.Select(Function(s) s.Label).ToArray()
        Assert.AreEqual("Fixed effects", names(0)) : Assert.IsTrue(names.Contains("Convergence"))
        Dim saved = Mixed14Fixture.CanonicalTables(report.LegacyTables())
        Dim postOptions As New MixedModelReportOptions With {.ClassInformation = False, .Contrast = MixedModelContrastSelection.Pairwise}
        ' Actual fits, including KR/Satt, use the original Core post-fit services before
        ' workspace release. LMM reference estimates are portable Core requests only;
        ' no new LMM GUI report/editor is introduced.
        For Each reference In {False, True}
            If kind = MixedModelWorkflowKind.LMM AndAlso Not reference Then Continue For
            postOptions.MeansMode = If(reference, MixedModelMeansMode.ReferenceGrid, MixedModelMeansMode.ObservedDesign)
            Dim intent As New MixedModelPostEstimationIntent(kind, f.FixedNames, "visit", f.Visit, False, Nothing, Nothing, postOptions, f.Rows, decision)
            If f.Visit Is Nothing Then Continue For
            Dim expectedPost As New List(Of ResultTable), actualPost As MixedModelSupplementalReport
            If reference Then
                Dim spec As New MixedModelReferenceGridSpec With {.FixedEffectNames = f.FixedNames}
                spec.AddByFactor("visit", MixedModelPostEstimation.UniqueSortedFiniteValues(f.Visit), f.Visit)
                Dim rows = MixedModelReferenceGridService.BuildReferenceGridRows(spec)
                expectedPost.Add(MixedModelReferenceGridService.BuildEstimatedMeansTable("Reference-grid estimated marginal means", rows, result, spec))
                actualPost = MixedModelPostEstimationPolicy.ProjectReference(result, intent.Reference.CreateCoreSpec(), intent)
            Else
                expectedPost.Add(MMRMPostEstimation.BuildEstimatedMeansByVisitTable(result, f.X, f.Visit, 0.05))
                expectedPost.Add(MMRMPostEstimation.BuildChangeFromBaselineTableControlled(result, f.X, f.Visit, MixedModelPostEstimation.UniqueSortedFiniteValues(f.Visit)(0), 0.05))
                actualPost = MixedModelPostEstimationPolicy.ProjectMMRM(result, f.X, intent)
            End If
            Mixed16Fixture.EqualTables(expectedPost.Where(Function(t) t IsNot Nothing).ToList(), actualPost.LegacyTables(), key & " post")
        Next
        result.ReleaseLargePostEstimationWorkspaces()
        Regression10Fixture.AssertCapture(saved, Mixed14Fixture.CanonicalTables(report.LegacyTables()), key & " detached")
        Console.WriteLine("04A16_FIT|" & key & "|" & culture & "|" & report.Diagnostics.State.ToString() & "|" & report.Document.Tables.Count.ToString(CultureInfo.InvariantCulture))
    End Sub

    <TestMethod>
    Public Sub ReferenceMetadataDuplicateProfilesAndAuthoringStayCoreOwned()
        Dim f = Mixed16Fixture.Create(), o As New MixedModelReportOptions With {.CovariateMode = MixedModelCovariateReferenceMode.UserSpecified, .CovariateValue = 7, .Alpha = -1}
        Dim intent = f.Intent(o)
        Assert.IsFalse(intent.Validation.IsValid)
        Assert.IsTrue(intent.Validation.Diagnostics.Any(Function(d) d.Code = "mixed.post.covariate.zero-only"))
        Dim spec = f.CurrentSpec(intent)
        Assert.AreEqual(0.0, spec.Covariates(0).UserValue)
        Dim col As New MixedModelReferenceColumn("age", MixedModelReferenceColumnRole.ContinuousCovariate, f.Age)
        col.Values(0) = -999 : Assert.AreNotEqual(-999.0, col.Values(0))
        MixedModelPostEstimationPolicy.AddReferenceColumn(spec, New MixedModelReferenceColumn("visit | VarC", MixedModelReferenceColumnRole.MarginalFactor, {5.0, 6.0}), "visit", "group", o)
        Assert.AreEqual(2, spec.ByFactors.Count)
        spec.AddMarginalFactor("visit", {0.0, 1.0}, f.Visit)
        Assert.ThrowsException(Of ArgumentException)(Sub() MixedModelReferenceGridService.BuildReferenceGridRows(spec))
        Dim specs As New Dictionary(Of String, TermSpec) From {
            {"factor", New TermSpec With {.Kind = "MainEffect", .Scale = PredictorScale.Categorical, .BaseVarKeys = New List(Of String) From {"group | VarD"}}},
            {"orphan", New TermSpec With {.Kind = "MainEffect", .Scale = PredictorScale.Categorical, .BaseVarKeys = New List(Of String) From {"site"}}},
            {"interaction", New TermSpec With {.Kind = "Interaction", .Scale = PredictorScale.Categorical, .BaseVarKeys = New List(Of String) From {"site", "group"}}}}
        CollectionAssert.AreEqual({"group | VarD"}, MixedModelPostEstimationPolicy.MainEffectKeys({"factor", "interaction"}, specs, PredictorScale.Categorical).ToArray())
        CollectionAssert.AreEqual({"group | VarD", "site"}, MixedModelPostEstimationPolicy.MainEffectKeys(specs.Keys, specs, PredictorScale.Categorical, singleBaseOnly:=False).ToArray())
        Assert.IsTrue(MixedModelPostEstimationPolicy.BetterGroupingCandidate(9, 10), "Legacy implementation has no six-level cap.")
        Assert.IsFalse(MixedModelPostEstimationPolicy.BetterGroupingCandidate(2, 2), "First equal-sized categorical candidate wins.")
    End Sub

    <DataTestMethod><DataRow(False, False)><DataRow(False, True)><DataRow(True, False)><DataRow(True, True)>
    Public Sub CoreReferenceProfilesAndWeightsStayInExactOriginalOrder(observedWeights As Boolean, zeroCovariate As Boolean)
        Dim f = Mixed16Fixture.Create(), o As New MixedModelReportOptions With {
            .Weighting = If(observedWeights, MixedModelReferenceGridWeighting.ObservedCellFrequency, MixedModelReferenceGridWeighting.EqualCells),
            .CovariateMode = If(zeroCovariate, MixedModelCovariateReferenceMode.UserSpecified, MixedModelCovariateReferenceMode.ObservedMean)}
        For Each grouped In {False, True}
            Dim expected = MixedModelReferenceGridService.BuildReferenceGridRows(f.LegacySpec(o, grouped)), actual = MixedModelReferenceGridService.BuildReferenceGridRows(f.CurrentSpec(f.Intent(o, grouped)))
            Assert.AreEqual(expected.Count, actual.Count)
            For i = 0 To actual.Count - 1
                Assert.AreEqual(expected(i).Label, actual(i).Label) : Assert.AreEqual(expected(i).Count, actual(i).Count)
                Assert.AreEqual(expected(i).WeightSum, actual(i).WeightSum)
                CollectionAssert.AreEqual(expected(i).L, actual(i).L)
                CollectionAssert.AreEqual(expected(i).Profile.ToArray(), actual(i).Profile.ToArray())
            Next
        Next
    End Sub

    <DataTestMethod><DataRow("unknown")><DataRow("pairwise among group levels")><DataRow(" Pairwise among group levels ")><DataRow("Selected comparison only")>
    Public Sub RawContrastFallbacksAndAbsentControlsMatchOriginalFactories(text As String)
        Dim f = Mixed16Fixture.Create()
        For Each reference In {False, True}
            Dim o As New MixedModelReportOptions With {.RequestedContrastText = text, .RequestedDirectionText = "unknown", .Contrast = MixedModelReportPolicy.Contrast(text),
                .ControlSelection = "999", .ComparisonSelection = "0", .MeansMode = If(reference, MixedModelMeansMode.ReferenceGrid, MixedModelMeansMode.ObservedDesign)}
            Dim intent = f.Intent(o), result = f.Result()
            Dim current = MixedModelPostEstimationPolicy.ProjectMMRM(result, f.X, intent, Function() Mixed16Fixture.ClassTable(), Function() MixedModelPostEstimationPolicy.ProjectReference(result, f.CurrentSpec(intent), intent))
            Mixed16Fixture.EqualTables(f.LegacyTables(result, o, True), current.LegacyTables(), text)
        Next
    End Sub

    <TestMethod>
    Public Sub MissingInputsStatusesBoundedWarningsAndDetachedScalarsAreExplicit()
        Assert.IsNotNull(GetType(MixedModelReportPolicy).GetMethod("Project", {GetType(MixedModelWorkflowKind), GetType(MixedModelResult), GetType(MixedModelReportOptions)}), "Keep the accepted three-argument binary entry point.")
        Dim f = Mixed16Fixture.Create(), o As New MixedModelReportOptions, result = f.Result(), intent = f.Intent(o)
        Assert.AreEqual(0, MixedModelPostEstimationPolicy.ProjectMMRM(Nothing, f.X, intent).Document.Tables.Count)
        Assert.AreEqual(0, MixedModelPostEstimationPolicy.ProjectMMRM(result, Nothing, intent).Document.Tables.Count)
        Assert.AreEqual(0, MixedModelPostEstimationPolicy.ProjectMMRM(result, f.X, Nothing).Document.Tables.Count)
        Assert.AreEqual(0, MixedModelPostEstimationPolicy.ProjectMMRM(result, New Double(,) {{1}}, intent).Document.Tables.Count)
        Assert.AreEqual(0, MixedModelPostEstimationPolicy.ProjectReference(result, Nothing, intent).Document.Tables.Count)
        Assert.AreEqual(0, MixedModelPostEstimationPolicy.ProjectReference(Nothing, Nothing, Nothing).Document.Tables.Count)
        Assert.AreEqual(0, MixedModelPostEstimationPolicy.LmmClassReport(Nothing).Document.Tables.Count)
        Dim lmm As New MixedModelPostEstimationIntent(MixedModelWorkflowKind.LMM, f.Names, "visit", f.Visit, False, Nothing, Nothing, o)
        Assert.IsTrue(MixedModelPostEstimationPolicy.ProjectMMRM(result, f.X, lmm).Document.Warnings.Any(Function(w) w.Code = "mixed.post.kind.unsupported"))
        result.UserWarnings.AddRange(Enumerable.Range(0, 1000).Select(Function(i) "warning " & i.ToString(CultureInfo.InvariantCulture)))
        Dim snapshot = MixedModelDiagnosticPolicy.Capture(result, Double.NaN)
        Assert.AreEqual(MixedModelWorkflowPolicy.MaximumDiagnostics, snapshot.Warnings.Count)
        result.UserWarnings.Clear() : result.Beta(0) = Double.NaN : result.Converged = False
        Assert.AreEqual(MixedModelFitState.Converged, snapshot.State) : Assert.AreEqual(128, snapshot.Warnings.Count)
        Assert.IsFalse(snapshot.HasKrWorkspace)
        Dim unavailable = MixedModelReportPolicy.Project(MixedModelWorkflowKind.MMRM, Nothing, o)
        Assert.AreEqual(AnalysisDocumentStatus.Failed, unavailable.Document.Status)
        Assert.AreEqual(MixedModelFitState.Unavailable, unavailable.Diagnostics.State)
        Assert.IsFalse(MixedModelPostEstimationPolicy.HasGroupingLevels(1))
        Assert.AreEqual(MixedModelGroupingChoice.Automatic, MixedModelPostEstimationPolicy.GroupingChoice("(auto)"))
        Assert.AreEqual(MixedModelGroupingChoice.None, MixedModelPostEstimationPolicy.GroupingChoice("(None)"))
        Assert.AreEqual(MixedModelGroupingChoice.Explicit, MixedModelPostEstimationPolicy.GroupingChoice(" (Auto) "))
    End Sub
End Class
