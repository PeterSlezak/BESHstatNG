Option Explicit On
Option Strict On
Imports System
Imports System.Collections.Generic
Imports System.Globalization
Imports System.IO
Imports System.Linq
Imports System.Text.RegularExpressions
Imports BESHStatNG
Imports BESHStatNG.ExcelCommon
Imports BESHStatNG.regression
Imports Microsoft.VisualStudio.TestTools.UnitTesting

' Prechange 04A.14 control construction; parsers are the frozen source-derived helper.
' This is a compatibility oracle, not independent statistical validation.
Public NotInheritable Class Mixed15Legacy
    Public Shared Sub Apply(kind As MixedModelWorkflowKind, ByRef ctl As MixedModelControl, o As MixedModelWorkflowOptions)
        ctl.MaxIter = o.MaximumIterations : ctl.Epsilon = o.Epsilon
        ctl.StepTolerance = o.Epsilon : ctl.FunctionTolerance = o.Epsilon
        Dim text = If(String.IsNullOrWhiteSpace(o.Optimizer), "AI/Fisher scoring (default)", o.Optimizer)
        ctl.CovarianceOptimizerMode = LegacyMixed14Options.ParseCovarianceOptimizerMode(text,
            If(kind = MixedModelWorkflowKind.MMRM, MixedModelCovarianceOptimizerMode.ProjectedBfgs, MixedModelCovarianceOptimizerMode.AverageInformationReml))
        ctl.CovarianceGradientMode = LegacyMixed14Options.ParseCovarianceGradientMode(o.Gradient)
        If String.Equals(text, "Projected BFGS (analytic gradient)", StringComparison.OrdinalIgnoreCase) Then
            ctl.CovarianceOptimizerMode = MixedModelCovarianceOptimizerMode.ProjectedBfgsAnalyticGradient
            ctl.CovarianceGradientMode = MixedModelCovarianceGradientMode.AnalyticScore
        ElseIf String.Equals(text, "Projected BFGS (finite-difference gradient)", StringComparison.OrdinalIgnoreCase) Then
            ctl.CovarianceOptimizerMode = MixedModelCovarianceOptimizerMode.ProjectedBfgs
            ctl.CovarianceGradientMode = MixedModelCovarianceGradientMode.NumericalFiniteDifference
        End If
        ctl.Trace = o.Trace OrElse o.IterationDetails
    End Sub
End Class

<TestClass>
Public Class Mixed15FittingTests
    Public Shared Function Aliases() As IEnumerable(Of Object())
        Dim rows As New List(Of Object())
        For Each random In {False, True}
            Dim sourcePath = Path.Combine(Regression09Fixture.Root(), "BESHStat.Core/src/RegModels/MixedModels", If(random, "MixedModelGStruct.vb", "MixedModelRStruct.vb"))
            Dim source = File.ReadAllText(sourcePath).Split(New String() {"Public MustInherit Class"}, StringSplitOptions.None)(0)
            For Each m As Match In Regex.Matches(source, "normalized = ""([^""]*)""")
                rows.Add({If(random, "G", "R"), m.Groups(1).Value})
            Next
        Next
        Return rows
    End Function
    <DataTestMethod><DynamicData(NameOf(Aliases), DynamicDataSourceType.Method)>
    Public Sub EveryOriginalFactoryAliasHasLosslessSilentMetadata(side As String, token As String)
        Dim saved = CultureInfo.CurrentCulture
        Try
            For Each culture In {"", "sk-SK"}
                CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(culture)
                For Each spelling In {token, "  " & token.ToUpperInvariant() & "  "}
                    Dim choice = If(side = "G", MixedModelFittingOptions.SelectRandom(spelling), MixedModelFittingOptions.SelectResidual(spelling))
                    Assert.AreEqual(spelling, choice.RequestedToken)
                    Assert.IsTrue(choice.IsSupported)
                    If side = "G" Then
                        Dim original = MixedModelGStructUtils.createMixedModelGStruct(spelling)
                        Assert.AreEqual(original.GetType().Name, choice.EffectiveCoreType)
                        Assert.AreEqual(original.ToString(), choice.EffectiveName)
                        Assert.AreEqual(original.IsDegenerateZeroG(), choice.IsZeroRandom)
                        Assert.AreEqual(original.GetType(), choice.CreateRandomStructure().GetType())
                        For Each q In {0, 1, 2, 3}
                            Dim count As Nullable(Of Integer) = Nothing
                            Try
                                count = original.ParamCount(q)
                            Catch ex As ApplicationException
                            End Try
                            Assert.AreEqual(count, choice.RandomParameterCount(q))
                        Next
                    Else
                        Dim original = MixedModelRStructUtils.createMixedModelRStruct(spelling)
                        Assert.AreEqual(original.GetType().Name, choice.EffectiveCoreType)
                        Assert.AreEqual(original.ToString(), choice.EffectiveName)
                        Assert.AreEqual(original.UsesVisitIndex(), choice.UsesVisitIndex)
                        Assert.AreEqual(original.GetType(), choice.CreateResidualStructure().GetType())
                    End If
                Next
            Next
        Finally
            CultureInfo.CurrentCulture = saved
        End Try
    End Sub

    <TestMethod>
    Public Sub UnknownNullAndInteriorSpacingDoNotAcquireNewScientificAliases()
        For Each token In New String() {Nothing, "", "  ", "unknown", "Compound  Symmetry", "AR 1", "UNstructured Random Effects"}
            Dim choice = MixedModelFittingOptions.SelectResidual(token)
            Assert.IsFalse(choice.IsSupported, If(token, "<null>"))
            Dim original = Assert.ThrowsException(Of ApplicationException)(Sub() MixedModelRStructUtils.createMixedModelRStruct(token))
            Dim current = Assert.ThrowsException(Of ApplicationException)(Sub() choice.CreateResidualStructure())
            Assert.AreEqual(original.Message, current.Message)
        Next
        For Each token In New String() {"unknown", "Random  Intercept", "har1", "compound-symmetry"}
            Dim choice = MixedModelFittingOptions.SelectRandom(token)
            Assert.IsFalse(choice.IsSupported)
            Dim original = Assert.ThrowsException(Of ApplicationException)(Sub() MixedModelGStructUtils.createMixedModelGStruct(token))
            Dim current = Assert.ThrowsException(Of ApplicationException)(Sub() choice.CreateRandomStructure())
            Assert.AreEqual(original.Message, current.Message)
        Next
        Assert.IsTrue(MixedModelFittingOptions.SelectRandom(Nothing).IsZeroRandom)
        Assert.IsNull(MixedModelFittingOptions.SelectRandom("Unstructured").RandomParameterCount(Integer.MaxValue))
        Assert.IsNull(MixedModelFittingOptions.SelectRandom("RI").RandomParameterCount(-1))
    End Sub

    Public Shared Function Combinations() As IEnumerable(Of Object())
        Return From r In MixedModelRStruct.RStructsList From g In MixedModelGStruct.GStructsList Select New Object() {r, g}
    End Function
    <DataTestMethod><DynamicData(NameOf(Combinations), DynamicDataSourceType.Method)>
    Public Sub EachDisplayedRAndGCombinationDelegatesShapeMetadata(r As String, g As String)
        Dim f = Mixed14Fixture.Load("LMM|Variance Components (VC/Diag)|Large-sample normal|REML|toy")
        For Each withVisit In {False, True}
            Dim data = MixedModelBlockData.FromArrays(f.Y, f.X, f.Subjects, f.Z, If(withVisit, f.Visit, Nothing), withVisit, f.Rows)
            Dim selection As New MixedModelFittingOptions(MixedModelWorkflowKind.LMM, New MixedModelWorkflowOptions With {.ResidualStructure = r, .RandomStructure = g})
            Dim originalR = MixedModelRStructUtils.createMixedModelRStruct(r), originalG = MixedModelGStructUtils.createMixedModelGStruct(g)
            Assert.AreEqual(CInt(originalR.ParamCount(data)), selection.Residual.ParameterCount(data).Value)
            Dim expectedG As Nullable(Of Integer) = Nothing
            Try
                expectedG = originalG.ParamCount(data.Q)
            Catch ex As ApplicationException
            End Try
            Assert.AreEqual(expectedG, selection.Random.ParameterCount(data))
            Dim issues = selection.DesignDiagnostics(data)
            Assert.AreEqual(Not expectedG.HasValue AndAlso Not selection.Random.IsZeroRandom, issues.Any(Function(d) d.Code = "mixed.random.dimension"))
        Next
        ' Selection/metadata coverage only: this method does not claim a successful fit.
    End Sub

    Public Shared Function AllOptionTokens() As String()
        Dim tokens = Mixed14WorkflowTests.ParserTokens().Select(Function(row) CStr(row(0))).ToList()
        ' Every literal alias in the frozen prechange parser, plus shipping labels and
        ' case/outer-spacing variants. Expected outcomes still come from that parser.
        Dim source = File.ReadAllText(Path.Combine(Regression09Fixture.Root(), "BESHStat.Excel.Common.Tests/src/LegacyMixed14Options.vb"))
        For Each m As Match In Regex.Matches(source, "token = ""([^""]*)""")
            tokens.Add(m.Groups(1).Value)
        Next
        For Each token In tokens.ToArray()
            If token IsNot Nothing Then tokens.Add("  " & token.ToLowerInvariant() & "  ")
        Next
        Return tokens.Distinct(StringComparer.Ordinal).ToArray()
    End Function
    Public Shared Function Optimizers() As IEnumerable(Of Object())
        Return From token In AllOptionTokens() Select New Object() {If(token Is Nothing, "<null>", token)}
    End Function
    <DataTestMethod><DynamicData(NameOf(Optimizers), DynamicDataSourceType.Method)>
    Public Sub OptimizerGradientCrossProductPreservesPrechangeControls(token As String)
        Dim saved = CultureInfo.CurrentCulture
        Try
            For Each culture In {"", "sk-SK"}
                CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(culture)
                For Each kind In {MixedModelWorkflowKind.MMRM, MixedModelWorkflowKind.LMM}
                    For Each gradient In AllOptionTokens()
                        Dim o = MixedModelWorkflowOptions.Defaults(kind)
                        o.Optimizer = If(token = "<null>", Nothing, token) : o.Gradient = gradient
                        o.Epsilon = 0.0001 : o.MaximumIterations = 17 : o.IterationDetails = True
                        Dim expected = MixedModelControl.CreateDefault(), actual = expected
                        Mixed15Legacy.Apply(kind, expected, o)
                        Dim selection As New MixedModelFittingOptions(kind, o)
                        selection.ApplyControl(actual)
                        Assert.AreEqual(expected.CovarianceOptimizerMode, actual.CovarianceOptimizerMode)
                        Assert.AreEqual(expected.CovarianceGradientMode, actual.CovarianceGradientMode)
                        Assert.AreEqual(expected.MaxIter, actual.MaxIter)
                        Assert.AreEqual(expected.Epsilon, actual.Epsilon)
                        Assert.AreEqual(expected.StepTolerance, actual.StepTolerance)
                        Assert.AreEqual(expected.FunctionTolerance, actual.FunctionTolerance)
                        Assert.AreEqual(expected.Trace, actual.Trace)
                        Assert.AreEqual(expected.FallbackToNumericalGradientOnAnalyticFailure, actual.FallbackToNumericalGradientOnAnalyticFailure)
                    Next
                Next
            Next
        Finally
            CultureInfo.CurrentCulture = saved
        End Try
    End Sub

    <TestMethod>
    Public Sub ImmutableSelectionsAndPortableDiagnosticsKeepAcquiredBoundary()
        Dim key = "LMM|Random Intercept|Large-sample normal|REML|toy", o = Mixed14Requests.Options(key)
        o.RandomStructure = "RI" : o.Epsilon = Double.NaN : o.MaximumIterations = 0
        o.Optimizer = "unknown" : o.Gradient = "unknown" : o.FitMethod = "unknown"
        Dim request = Mixed14Requests.Request(key, o)
        o.RandomStructure = "unknown"
        request.Fitting.RequestedOptions.RandomStructure = "None"
        Assert.AreEqual("RI", request.Fitting.Random.RequestedToken)
        Assert.AreEqual("RI", request.Fitting.RequestedOptions.RandomStructure)
        Assert.IsFalse(request.Validation.IsValid)
        For Each code In {"mixed.epsilon", "mixed.iterations", "mixed.optimizer.fallback", "mixed.gradient.fallback", "mixed.fit.fallback"}
            Assert.IsTrue(request.Validation.Diagnostics.Any(Function(d) d.Code = code), code)
        Next
        Assert.ThrowsException(Of ArgumentException)(Sub() request.CreateCoreRequest())
        Assert.IsNotNull(request.CreateAcquiredCoreRequest(), "Diagnostic-only checks must not become new Windows refusals.")
        Dim choices = MixedModelFittingOptions.ResidualChoices(), original = MixedModelRStruct.RStructsList(0)
        Try
            MixedModelRStruct.RStructsList(0) = "changed"
            Assert.AreEqual(original, choices(0), "Supported arrays are independent snapshots.")
        Finally
            MixedModelRStruct.RStructsList(0) = original
        End Try
        Assert.ThrowsException(Of NotSupportedException)(Sub() DirectCast(choices, IList(Of String))(0) = "changed")
        Dim codes = request.Validation.Diagnostics.Select(Function(d) d.Code).ToArray()
        CollectionAssert.AreEqual(codes, MixedModelWorkflowPolicy.Validate(request).Diagnostics.Select(Function(d) d.Code).ToArray())
        Assert.IsTrue(codes.Length <= MixedModelWorkflowPolicy.MaximumDiagnostics)
        Assert.IsFalse(MixedModelWorkflowPolicy.Validate(Nothing).IsValid)
    End Sub

    <DataTestMethod><DataRow("Identity")><DataRow("AR1")><DataRow("Unstructured")><DataRow("Diagonal")>
    Public Sub OptionalVisitIsNotConfusedWithMissingVisitRole(token As String)
        Dim key = "LMM|Random Intercept|Large-sample normal|REML|novisit", o = Mixed14Requests.Options(key)
        o.ResidualStructure = token
        Dim request = Mixed14Requests.Request(key, o), acquired = request.CreateAcquiredCoreRequest()
        Assert.AreEqual(MixedModelFittingOptions.SelectResidual(token).UsesVisitIndex, Not request.Validation.IsValid)
        acquired.Validate() ' Core intentionally warns and retains sequential order for AR1/UN/Diag.
        Assert.IsFalse(acquired.Data.HasVisit)
        Assert.AreEqual("", acquired.VisitVarName)
    End Sub

    <TestMethod>
    Public Sub ToleranceIterationAndTraceBoundariesAreProjectedWithoutSanitizing()
        For Each kind In {MixedModelWorkflowKind.MMRM, MixedModelWorkflowKind.LMM}
            For Each epsilon In {Double.NaN, Double.PositiveInfinity, Double.NegativeInfinity, -1.0, 0.0, Double.Epsilon, 0.000001}
                For Each maximum In {-1, 0, 1, Integer.MaxValue}
                    For Each trace In {False, True}
                        For Each details In {False, True}
                            Dim o = MixedModelWorkflowOptions.Defaults(kind)
                            o.Epsilon = epsilon : o.MaximumIterations = maximum : o.Trace = trace : o.IterationDetails = details
                            Dim selection As New MixedModelFittingOptions(kind, o), ctl = MixedModelControl.CreateDefault()
                            ctl.AnalyticGradientValidationTolerance = 0.0123 : ctl.EnableStructuredRestarts = False
                            selection.ApplyControl(ctl)
                            Assert.AreEqual(epsilon, ctl.Epsilon) : Assert.AreEqual(epsilon, ctl.StepTolerance) : Assert.AreEqual(epsilon, ctl.FunctionTolerance)
                            Assert.AreEqual(maximum, ctl.MaxIter) : Assert.AreEqual(trace OrElse details, ctl.Trace)
                            Assert.AreEqual(0.0123, ctl.AnalyticGradientValidationTolerance) : Assert.IsFalse(ctl.EnableStructuredRestarts)
                        Next
                    Next
                Next
            Next
        Next
        Dim choices = MixedModelFittingOptions.RandomChoices()
        Assert.ThrowsException(Of NotSupportedException)(Sub() DirectCast(choices, IList(Of String))(0) = "changed")
        Dim mmrm As New MixedModelFittingOptions(MixedModelWorkflowKind.MMRM, New MixedModelWorkflowOptions With {.RandomStructure = "unsupported"})
        Assert.IsTrue(mmrm.Random.IsZeroRandom) : Assert.AreEqual("unsupported", mmrm.RequestedOptions.RandomStructure)
    End Sub
End Class
