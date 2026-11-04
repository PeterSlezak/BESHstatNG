Option Explicit On
Option Strict On
Imports System
Imports System.Collections.Generic
Imports System.Globalization
Imports System.Linq
Imports System.Reflection
Imports BESHStatNG
Imports BESHStatNG.ExcelCommon
Imports BESHStatNG.Multivariate
Imports Microsoft.VisualStudio.TestTools.UnitTesting

Public NotInheritable Class Multivariate17Fixture
    Public Shared Function Data(Optional missing As Boolean = False) As Double(,)
        Dim x(29, 2) As Double
        For i = 0 To 29
            x(i, 0) = i * 0.7 + Math.Sin(i)
            x(i, 1) = (i Mod 6) * 1.3 + Math.Cos(i * 2)
            x(i, 2) = i * 0.2 + Math.Sin(i * 0.4)
        Next
        If missing Then x(4, 1) = Double.NaN
        Return x
    End Function
    Public Shared ReadOnly Names As String() = {"A", "B", "C"}
    Public Shared Function Rows() As Integer()
        Return Enumerable.Range(0, 30).Select(Function(i) 101 + i * 3).ToArray()
    End Function
    Public Shared Sub Cells(expected(,) As Object, actual(,) As Object)
        Assert.AreEqual(expected.GetLength(0), actual.GetLength(0)) : Assert.AreEqual(expected.GetLength(1), actual.GetLength(1))
        For i = 0 To expected.GetLength(0) - 1
            For j = 0 To expected.GetLength(1) - 1
                Dim a = expected(i, j), b = actual(i, j)
                If a Is Nothing Then
                    Assert.IsNull(b) : Continue For
                End If
                Assert.IsNotNull(b) : Assert.AreEqual(a.GetType(), b.GetType(), $"CLR kind {i},{j}")
                If TypeOf a Is Double Then
                    Dim d = CDbl(a), e = CDbl(b)
                    If Double.IsNaN(d) Then
                        Assert.IsTrue(Double.IsNaN(e))
                    ElseIf Double.IsInfinity(d) Then
                        Assert.AreEqual(d, e)
                    Else
                        Assert.AreEqual(d, e, Math.Max(0.000000001, Math.Abs(d) * 0.000000001), $"cell {i},{j}")
                    End If
                Else
                    Assert.AreEqual(a, b, $"cell {i},{j}")
                End If
            Next
        Next
    End Sub
    Public Shared Sub Tables(expected As List(Of ResultTable), actual As List(Of ResultTable))
        Assert.AreEqual(expected.Count, actual.Count)
        For i = 0 To expected.Count - 1
            Dim a = expected(i).ToOutputModel(), b = actual(i).ToOutputModel()
            Cells(a.Values, b.Values)
            Assert.AreEqual(a.TitleRows, b.TitleRows) : Assert.AreEqual(a.HeaderTopRows, b.HeaderTopRows)
            Assert.AreEqual(a.HeaderLeftColumns, b.HeaderLeftColumns) : Assert.AreEqual(a.FooterRows, b.FooterRows)
            CollectionAssert.AreEqual(a.PvalueColumns, b.PvalueColumns)
            Assert.AreEqual(a.PvalueCells.Count, b.PvalueCells.Count)
            For j = 0 To a.PvalueCells.Count - 1
                Assert.AreEqual(a.PvalueCells(j).BodyRow, b.PvalueCells(j).BodyRow) : Assert.AreEqual(a.PvalueCells(j).BodyColumn, b.PvalueCells(j).BodyColumn)
            Next
        Next
    End Sub
    Public Shared Function Fit(kind As MultivariateAnalysisKind, caseNumber As Integer, current As Boolean, Optional ByRef state As String = Nothing, Optional ByRef payload As List(Of Array) = Nothing) As List(Of ResultTable)
        Dim x = Data(caseNumber Mod 7 = 6), rows = Multivariate17Fixture.Rows(), names = DirectCast(Multivariate17Fixture.Names.Clone(), String())
        Select Case kind
            Case MultivariateAnalysisKind.PCA
                Dim model As New PCA, corr = caseNumber Mod 2 = 0, mode = (caseNumber \ 2) Mod 3
                Dim band = (caseNumber \ 6) Mod 4
                Dim eigen = {0.0D, 1.0D, 1.000001D, 20D}(band), count = {1D, 2D, 3D, 0D}(band), percent = {50D, 80D, 99D, 100D}(band)
                Dim o As New MultivariatePcaOptions(corr, mode = 0, mode = 1, mode = 2, eigen, count, percent)
                model.dataInputs(Data(), rows, names, o.Label)
                If current Then
                    o.Configure(model, 2000, 0.000000000001)
                Else
                    model.settingsInputs(2000, 0.000000000001, If(corr, "Correlation", "Covariance"))
                End If
                model.Calculate(If(current, o.CoreRetention, {"Eigenvalue", "Fixed", "Variance"}(mode)), {CDbl(eigen), CDbl(count), CDbl(percent)}(mode))
                payload = New List(Of Array) From {model.Eigenvalues, model.Eigenvectors, model.ReducedDataset, model.StandardizedData, model.GetLoadings}
                state = "COMPLETED_CORE_CALL" : Return model.wrapResults()
            Case MultivariateAnalysisKind.MCA, MultivariateAnalysisKind.CA
                Dim model As New CA
                If kind = MultivariateAnalysisKind.MCA Then
                    Dim cells(11, 2) As String
                    For i = 0 To 11
                        cells(i, 0) = If(i Mod 2 = 0, "a", "A")
                        cells(i, 1) = "B" & (i Mod 3).ToString(CultureInfo.InvariantCulture)
                        cells(i, 2) = If(i Mod 4 = 0, "", "two")
                    Next
                    model.DataMultiple(cells, names)
                Else
                    model.data(New Integer(,) {{10, 6, 8}, {5, 11, 9}, {7, 9, 15}}, {"r", "r", "R"}, names)
                End If
                model.Calculate() : payload = New List(Of Array) From {model.RowFactors(0), model.ColFactors(0), model.ColContribution(0), model.PlotLabels(False, vbLf)}
                state = "COMPLETED_CORE_CALL" : Return model.wrapResults()
            Case MultivariateAnalysisKind.KMeans
                Dim model As New KMeans, o As New MultivariateKMeansOptions With {
                    .Clusters = 3, .Initialization = CType(caseNumber Mod 4, KMeansInitializationMethod),
                    .Distance = CType((caseNumber \ 4) Mod 2, KMeansDistanceMetric), .Starts = 3,
                    .Standardization = CType((caseNumber \ 2) Mod 3, ClusterStandardizationMode),
                    .Missing = ClusterMissingValuePolicy.ListwiseDeletion, .EmptyCluster = CType(caseNumber Mod 3, EmptyClusterHandlingStrategy), .Seed = 1729}
                model.dataInputs(x, rows.Select(Function(v) "Row " & v.ToString(CultureInfo.CurrentCulture)).ToArray(), names)
                If o.Initialization = KMeansInitializationMethod.UserSpecifiedCenters Then model.startingCentersInputs(New Double(,) {{0, 0, 0}, {9, 2, 1}, {18, 5, 4}})
                If current Then
                    o.Configure(model)
                Else
                    model.settingsInputs(o.Clusters, o.Initialization, o.Distance, o.Starts, o.MaximumIterations, o.Tolerance, o.Standardization, o.Missing, o.EmptyCluster, o.Seed)
                End If
                model.Fit() : payload = New List(Of Array) From {model.Result.ActiveRowIndices, model.Result.RemovedRowIndices, model.Result.ClusterAssignments, model.Result.ActiveRowLabels}
                state = "KMEANS_CONVERGED=" & model.Result.Converged.ToString() : Return model.wrapResults()
            Case MultivariateAnalysisKind.Hierarchical
                Dim model As New HierarchicalClustering, o As New MultivariateHierarchicalOptions With {
                    .Linkage = CType(caseNumber Mod 7, HierarchicalLinkageMethod), .Distance = CType((caseNumber \ 7) Mod 7, HierarchicalDistanceMetric),
                    .Standardization = CType(caseNumber Mod 3, ClusterStandardizationMode), .Missing = ClusterMissingValuePolicy.ListwiseDeletion,
                    .Cut = CType(caseNumber Mod 2, HierarchicalMembershipDisplayMode), .CutHeight = 5}
                model.dataInputs(x, Nothing, names)
                If current Then
                    o.ConfigureModel(model) : o.ConfigureReport(model)
                Else
                    model.settingsInputs(o.Linkage, o.Distance, o.MinkowskiPower, o.Standardization, o.Missing)
                    model.reportInputs(o.Cut, o.Clusters, o.CutHeight)
                End If
                model.Fit() : state = "COMPLETED_CORE_CALL" : Return model.wrapResults()
            Case MultivariateAnalysisKind.Factor
                Dim model As New FactorAnalysis, o As New MultivariateFactorOptions With {
                    .Extraction = CType(caseNumber Mod 6, FactorAnalysisExtractionMethod), .Rotation = CType((caseNumber \ 6) Mod 5, FactorAnalysisRotationMethod),
                    .MatrixType = CType(caseNumber Mod 2, FactorAnalysisMatrixType), .Retention = CType(caseNumber Mod 3, FactorAnalysisRetentionMethod),
                    .RetentionValue = {2.0, 0.7, 80.0}(caseNumber Mod 3), .MaximumIterations = 100, .PromaxPower = 2 + caseNumber Mod 3,
                    .Scores = CType(caseNumber Mod 3, FactorAnalysisScoreMethod), .Missing = FactorAnalysisMissingValuePolicy.ListwiseDeletion,
                    .Communality = CType(caseNumber Mod 2, FactorAnalysisCommunalityInitialization), .KaiserNormalization = caseNumber Mod 2 = 0}
                model.dataInputs(x, rows, names)
                If current Then
                    o.Configure(model)
                Else
                    model.settingsInputs(o.MaximumIterations, o.Epsilon, o.MatrixType, o.Extraction, o.Retention, o.RetentionValue, o.Rotation, o.Scores, o.Communality, o.Missing, o.KaiserNormalization, o.PromaxPower)
                End If
                model.Calculate() : payload = New List(Of Array) From {model.InitialEigenvalues, model.PatternMatrix, model.StructureMatrix, model.Scores, model.AnalysisRowIds}
                state = "FA_EXTRACTION=" & model.ExtractionConverged.ToString() & ";ROTATION=" & model.RotationConverged.ToString() : Return model.wrapResults()
            Case MultivariateAnalysisKind.Discriminant
                Dim model As New DiscriminantAnalysis, o As New MultivariateDiscriminantOptions With {
                    .Method = CType(caseNumber Mod 2, DiscriminantAnalysisMethod), .Priors = CType((caseNumber \ 2) Mod 3, DiscriminantPriorMode),
                    .Validation = CType((caseNumber \ 6) Mod 4, DiscriminantValidationMode), .Seed = 1729, .Folds = 3,
                    .Standardization = CType(caseNumber Mod 3, ClusterStandardizationMode), .Missing = ClusterMissingValuePolicy.ListwiseDeletion}
                Dim groups = Enumerable.Range(0, 30).Select(Function(i) CObj(If(i < 15, "a", "A"))).ToArray()
                model.dataInputs(x, groups, rows.Select(Function(v) "Row " & v.ToString(CultureInfo.CurrentCulture)).ToArray(), names)
                If current Then
                    o.ConfigureModel(model)
                Else
                    model.settingsInputs(o.Method, o.Standardization, o.Missing, o.Priors, o.Regularization)
                End If
                If o.Priors = DiscriminantPriorMode.UserSpecified Then model.priorInputs({CObj("a"), CObj("A")}, {0.4, 0.6})
                If current Then
                    o.ConfigureValidation(model)
                Else
                    model.validationInputs(o.Validation, o.Folds, o.HoldoutFraction, o.Seed, o.Stratified)
                End If
                model.Fit() : payload = New List(Of Array) From {model.PreparedData.ActiveOriginalIndices, model.PreparedData.RemovedOriginalIndices, model.PreparedData.ActiveRowLabels, model.GroupLabels}
                state = "COMPLETED_CORE_CALL" : Return model.wrapResults()
            Case Else : Throw New ArgumentOutOfRangeException(NameOf(kind))
        End Select
    End Function
End Class

<TestClass>
Public Class Multivariate17Tests
    Public Shared Iterator Function Choices() As IEnumerable(Of Object())
        For Each method In GetType(LegacyMultivariate17Selection).GetMethods(BindingFlags.Public Or BindingFlags.Static)
            If method.GetParameters().Length <> 1 Then Continue For
            Dim parameter = method.GetParameters()(0).ParameterType
            If parameter = GetType(Integer) Then
                For i = -2 To 9
                    Yield {method.Name, CObj(i)}
                Next
            ElseIf parameter = GetType(String) Then
                For Each s In {Nothing, "", "unknown", "LINEAR", "QDA", "Quadratic", "listwise deletion", "Z-scores", "Range 0 to 1", "USER-specified", "equal", "leave-one-out", "kfold", "k fold", "holdout"}
                    Yield {method.Name, CObj(s)}
                Next
            ElseIf parameter = GetType(Boolean) Then
                Yield {method.Name, CObj(False)} : Yield {method.Name, CObj(True)}
            End If
        Next
    End Function
    <DataTestMethod><DynamicData(NameOf(Choices), DynamicDataSourceType.Method)>
    Public Sub ChoiceMatchesFrozenPrechange(name As String, value As Object)
        Dim saved = CultureInfo.CurrentCulture
        Try
            For Each culture In {"", "sk-SK"}
                CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(culture)
                Dim oldMethod = GetType(LegacyMultivariate17Selection).GetMethod(name), newMethod = GetType(MultivariateLegacyChoices).GetMethod(name)
                Dim expected As Object = Nothing, actual As Object = Nothing, oldError As Exception = Nothing, newError As Exception = Nothing
                Try : expected = oldMethod.Invoke(Nothing, {value}) : Catch ex As TargetInvocationException : oldError = ex.InnerException : End Try
                Try : actual = newMethod.Invoke(Nothing, {value}) : Catch ex As TargetInvocationException : newError = ex.InnerException : End Try
                If oldError IsNot Nothing Then
                    Assert.IsNotNull(newError) : Assert.AreEqual(oldError.GetType(), newError.GetType()) : Assert.AreEqual(oldError.Message, newError.Message)
                Else
                    Assert.IsNull(newError) : Assert.AreEqual(expected, actual)
                End If
            Next
        Finally
            CultureInfo.CurrentCulture = saved
        End Try
    End Sub
    Public Shared Iterator Function Fits() As IEnumerable(Of Object())
        For Each pair In New KeyValuePair(Of MultivariateAnalysisKind, Integer)() {
            New KeyValuePair(Of MultivariateAnalysisKind, Integer)(MultivariateAnalysisKind.PCA, 24),
            New KeyValuePair(Of MultivariateAnalysisKind, Integer)(MultivariateAnalysisKind.CA, 1),
            New KeyValuePair(Of MultivariateAnalysisKind, Integer)(MultivariateAnalysisKind.MCA, 1),
            New KeyValuePair(Of MultivariateAnalysisKind, Integer)(MultivariateAnalysisKind.KMeans, 24),
            New KeyValuePair(Of MultivariateAnalysisKind, Integer)(MultivariateAnalysisKind.Hierarchical, 49),
            New KeyValuePair(Of MultivariateAnalysisKind, Integer)(MultivariateAnalysisKind.Factor, 30),
            New KeyValuePair(Of MultivariateAnalysisKind, Integer)(MultivariateAnalysisKind.Discriminant, 24)}
            For i = 0 To pair.Value - 1
                Yield {CInt(pair.Key), i}
            Next
        Next
    End Function
    <DataTestMethod><DynamicData(NameOf(Fits), DynamicDataSourceType.Method)>
    Public Sub CoreBackedConfigurationAndDetachedReports(kindValue As Integer, caseNumber As Integer)
        Dim saved = CultureInfo.CurrentCulture, kind = CType(kindValue, MultivariateAnalysisKind)
        Try
            For Each culture In {"", "sk-SK"}
                CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(culture)
                Dim expected As List(Of ResultTable) = Nothing, actual As List(Of ResultTable) = Nothing, oldError As Exception = Nothing, newError As Exception = Nothing
                Dim oldState As String = Nothing, newState As String = Nothing
                Dim oldPayload As List(Of Array) = Nothing, newPayload As List(Of Array) = Nothing
                Try : expected = Multivariate17Fixture.Fit(kind, caseNumber, False, oldState, oldPayload) : Catch ex As Exception : oldError = ex : End Try
                Try : actual = Multivariate17Fixture.Fit(kind, caseNumber, True, newState, newPayload) : Catch ex As Exception : newError = ex : End Try
                If oldError IsNot Nothing Then
                    Assert.IsNotNull(newError) : Assert.AreEqual(oldError.GetType(), newError.GetType()) : Assert.AreEqual(oldError.Message, newError.Message)
                    Console.WriteLine($"04A17_FIT|{kind}|{caseNumber}|{culture}|REJECTED:{oldError.GetType().Name}|0")
                Else
                    Assert.IsNull(newError) : Assert.AreEqual(oldState, newState)
                    Multivariate17Fixture.Tables(expected, actual)
                    If oldPayload IsNot Nothing Then
                        Assert.AreEqual(oldPayload.Count, newPayload.Count)
                        For k = 0 To oldPayload.Count - 1
                            Dim a = oldPayload(k), b = newPayload(k)
                            If a Is Nothing Then
                                Assert.IsNull(b) : Continue For
                            End If
                            Assert.AreEqual(a.Rank, b.Rank) : Assert.AreEqual(a.Length, b.Length)
                            For d = 0 To a.Rank - 1
                                Assert.AreEqual(a.GetLength(d), b.GetLength(d))
                            Next
                            Dim ac(0, a.Length - 1) As Object, bc(0, b.Length - 1) As Object
                            Dim av = a.Cast(Of Object)().ToArray(), bv = b.Cast(Of Object)().ToArray()
                            For j = 0 To a.Length - 1
                                ac(0, j) = av(j) : bc(0, j) = bv(j)
                            Next
                            Multivariate17Fixture.Cells(ac, bc)
                        Next
                    End If
                    Dim input As New MultivariateInputIntent(kind, 30, 3, Multivariate17Fixture.Names, Multivariate17Fixture.Rows())
                    Dim report = MultivariateReportPolicy.Project(input, actual, True)
                    Multivariate17Fixture.Tables(expected, report.LegacyTables())
                    Multivariate17Fixture.Tables(expected, report.LegacyTables())
                    If actual.Count > 0 Then actual(0).AddTitle("mutated caller")
                    Multivariate17Fixture.Tables(expected, report.LegacyTables())
                    Console.WriteLine($"04A17_FIT|{kind}|{caseNumber}|{culture}|{oldState}|{expected.Count}")
                End If
            Next
        Finally
            CultureInfo.CurrentCulture = saved
        End Try
    End Sub
    <DataTestMethod><DataRow(0)><DataRow(1)><DataRow(2)><DataRow(3)><DataRow(4)><DataRow(5)><DataRow(6)>
    Public Sub PlotOrderIdentityAndNoInventedCharts(kind As Integer)
        Dim p = MultivariateReportPolicy.PlotPlan(CType(kind, MultivariateAnalysisKind), True)
        Assert.AreEqual({8, 5, 3, 0, 1, 3, 0}(kind), p.Count)
        For Each plot In p
            If plot.Kind = MultivariatePlotKind.Correspondence Then Assert.AreEqual(2, plot.MinimumDimensions)
            If plot.Kind = MultivariatePlotKind.Contribution Then Assert.AreEqual(plot.Axis + 1, plot.MinimumDimensions)
        Next
        If kind = 0 Then
            CollectionAssert.AreEqual(New Double() {0, 0.5, 1}, p.Where(Function(v) v.Kind = MultivariatePlotKind.Biplot).Select(Function(v) v.Scaling).ToArray())
            CollectionAssert.AreEqual(New MultivariatePlotKind() {MultivariatePlotKind.Scree, MultivariatePlotKind.Scores2D, MultivariatePlotKind.Loadings2D, MultivariatePlotKind.Biplot, MultivariatePlotKind.Biplot, MultivariatePlotKind.Biplot, MultivariatePlotKind.Scores3D, MultivariatePlotKind.Loadings3D}, p.Select(Function(v) v.Kind).ToArray())
        End If
        Assert.AreEqual(0, MultivariateReportPolicy.PlotPlan(MultivariateAnalysisKind.Hierarchical, False).Count)
    End Sub
    <TestMethod>
    Public Sub IdentityCellsAndOptionsAreDetachedWithoutInventingCoordinates()
        Dim names = {"x", "x"}, rows = {101, 109}, labels = {"dup", "dup"}, groups = {"a", "A"}
        Dim input As New MultivariateInputIntent(MultivariateAnalysisKind.PCA, 2, 2, names, rows, labels, groups)
        names(0) = "other" : rows(0) = 0 : labels(0) = "other" : groups(0) = "other"
        Assert.AreEqual("x", input.VariableNames(0)) : Assert.AreEqual(101, input.SourceRowIds(0))
        input.SourceRowIds(0) = 0 : Assert.AreEqual(101, input.SourceRowIds(0))
        Assert.AreEqual("dup", input.RowLabels(0)) : Assert.AreEqual("a", input.GroupLabels(0))
        Assert.IsTrue(input.Validation.Diagnostics.All(Function(d) Not d.Blocking))
        Assert.AreEqual("101", MultivariateWorkflowPolicy.PcaAuxiliaryLabel(" ", 101, True))
        Assert.AreEqual("(missing)", MultivariateWorkflowPolicy.PcaAuxiliaryLabel(Nothing, 101, False))
        Assert.AreEqual("text", MultivariateWorkflowPolicy.PcaAuxiliaryLabel(" text ", 101, True))
        Assert.AreEqual("Row 109", MultivariateWorkflowPolicy.ObservationLabel(" ", 109))
        Assert.AreEqual(" x ", MultivariateWorkflowPolicy.ObservationLabel(" x ", 109))
        Assert.AreEqual(Integer.MinValue, MultivariateWorkflowPolicy.Seed(Nothing)) : Assert.AreEqual(0, MultivariateWorkflowPolicy.Seed(0))
        Dim o As New MultivariateKMeansOptions, copy = o.Copy() : copy.Seed = 2 : Assert.AreEqual(Integer.MinValue, o.Seed)
        Dim data As Object(,) = {{1.0, Nothing}, {Double.NaN, "text"}}
        Dim table = MultivariateWorkflowPolicy.ClusterInputTable(data, {"x", "y"}, {101, 109}, {"dup", "dup"})
        Assert.AreEqual("OriginalRow", table(0, 0)) : Assert.AreEqual("RowLabel", table(0, 1))
        Assert.AreEqual(109, table(2, 0)) : Assert.IsNull(table(1, 3)) : Assert.AreEqual("text", table(2, 3))
        table(1, 2) = 9.0 : Assert.AreEqual(1.0, data(0, 0))
        Dim da = MultivariateWorkflowPolicy.DiscriminantInputTable(data, {"group", "x"}, {CObj("a"), CObj("A")}, Nothing)
        Assert.AreEqual("group", da(0, 0)) : Assert.AreEqual("a", da(1, 0)) : Assert.AreEqual("A", da(2, 0))
        Assert.IsNull(New MultivariateInputIntent(MultivariateAnalysisKind.CA, 2, 2, names).SourceRowIds)
    End Sub
    <DataTestMethod><DataRow(0)><DataRow(1)><DataRow(2)><DataRow(3)><DataRow(4)><DataRow(5)><DataRow(6)><DataRow(7)><DataRow(8)><DataRow(9)>
    Public Sub PriorsKeepCultureCaseSeparatorsAndFailureBoundaries(index As Integer)
        Dim texts = {Nothing, "", "a=0.4;A:0.6", "a=0,4" & vbCrLf & "A:0,6", "a=0.4" & vbCr & "a=0.6", "a=NaN;A=Infinity", "a=1e-1;A=9e-1", "a", "=0.3", "a=bad"}
        Dim saved = CultureInfo.CurrentCulture
        Try
            For Each culture In {"", "sk-SK"}
                CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(culture)
                Dim a() As Object = Nothing, b() As Object = Nothing, av() As Double = Nothing, bv() As Double = Nothing
                Dim ae As Exception = Nothing, be As Exception = Nothing
                Try : LegacyMultivariate17Selection.ParsePriors(texts(index), a, av) : Catch ex As Exception : ae = ex : End Try
                Try : MultivariateLegacyChoices.ParsePriors(texts(index), b, bv) : Catch ex As Exception : be = ex : End Try
                If ae IsNot Nothing Then
                    Assert.IsNotNull(be) : Assert.AreEqual(ae.GetType(), be.GetType()) : Assert.AreEqual(ae.Message, be.Message)
                Else
                    Assert.IsNull(be) : CollectionAssert.AreEqual(a, b) : CollectionAssert.AreEqual(av, bv)
                End If
            Next
        Finally
            CultureInfo.CurrentCulture = saved
        End Try
    End Sub
    <TestMethod>
    Public Sub RetentionDefaultsPrioritiesAndCandidateDiagnostics()
        For Each eigen In {False, True}
            For Each fixed In {False, True}
                For Each varianceChoice In {False, True}
                    Dim saved = CultureInfo.CurrentCulture
                    Try
                        For Each culture In {"", "sk-SK"}
                            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(culture)
                            Dim p As New MultivariatePcaOptions(True, eigen, fixed, varianceChoice, 1.25D, 2D, 80D)
                            Assert.AreEqual(If(eigen, "Eigenvalue", If(fixed, "Fixed", If(varianceChoice, "Variance", ""))), p.CoreRetention)
                            Assert.AreEqual(If(eigen, 1.25, If(fixed, 2.0, If(varianceChoice, 80.0, 0.0))), p.Value)
                            Assert.AreEqual(If(eigen, "Eigenvalue > " & CStr(1.25D), If(fixed, "Fixed Number of Components = 2", If(varianceChoice, "Explained Variance > 80", ""))), p.Label)
                        Next
                    Finally
                        CultureInfo.CurrentCulture = saved
                    End Try
                    Assert.AreEqual(LegacyMultivariate17Selection.GetSelectedFARetentionMethod(eigen, varianceChoice), MultivariateLegacyChoices.GetSelectedFARetentionMethod(eigen, varianceChoice))
                Next
            Next
        Next
        Dim empty As New MultivariateInputIntent(MultivariateAnalysisKind.MCA, 0, 0, Nothing, {}, {}, {})
        Assert.IsTrue(empty.Validation.Diagnostics.Count > 0) : Assert.IsTrue(empty.Validation.Diagnostics.All(Function(d) Not d.Blocking))
        Assert.ThrowsException(Of ArgumentException)(Sub() MultivariateWorkflowPolicy.ValidateAuxiliaryRoles({"x"}, "x", "x", True))
        Assert.ThrowsException(Of ArgumentException)(Sub() MultivariateWorkflowPolicy.ValidateAuxiliaryRoles({"x"}, "", "x", False))
        MultivariateWorkflowPolicy.ValidateAuxiliaryRoles({"X"}, "x", "g", True)
    End Sub
    <DataTestMethod><DataRow(0)><DataRow(1)><DataRow(2)><DataRow(3)><DataRow(4)><DataRow(5)><DataRow(6)><DataRow(7)><DataRow(8)><DataRow(9)><DataRow(10)><DataRow(11)>
    Public Sub NegativeAndBoundaryCallsPreserveCoreOutcomes(caseNumber As Integer)
        Dim expected As Exception = Nothing, actual As Exception = Nothing
        Dim callCore As Action(Of Boolean) =
            Sub(current)
                Select Case caseNumber
                    Case 0, 1, 2, 3, 4
                        Dim data = If(caseNumber = 0, New Double(,) {{1.0}}, Multivariate17Fixture.Data())
                        If caseNumber = 1 Then
                            For i = 0 To data.GetLength(0) - 1
                                data(i, 0) = 1
                            Next
                        ElseIf caseNumber = 2 Then
                            data(0, 0) = Double.NaN
                        ElseIf caseNumber = 3 Then
                            data(0, 0) = Double.PositiveInfinity
                        ElseIf caseNumber = 4 Then
                            For i = 0 To data.GetLength(0) - 1
                                data(i, 1) = data(i, 0) + 0.000000001 * i
                            Next
                        End If
                        Dim model As New PCA, o As New MultivariatePcaOptions(True, False, True, False, 1D, 1D, 80D)
                        model.dataInputs(data, Enumerable.Range(101, data.GetLength(0)).ToArray(), Multivariate17Fixture.Names)
                        If current Then o.Configure(model, 100, 0.000001) Else model.settingsInputs(100, 0.000001, "Correlation")
                        model.Calculate("Fixed", 1)
                    Case 5, 6, 7
                        Dim model As New KMeans, o As New MultivariateKMeansOptions With {.Clusters = If(caseNumber = 5, 0, 3), .Tolerance = If(caseNumber = 6, -1, Double.NaN), .Seed = 1729}
                        model.dataInputs(Multivariate17Fixture.Data(True))
                        If current Then o.Configure(model) Else model.settingsInputs(o.Clusters, o.Initialization, o.Distance, o.Starts, o.MaximumIterations, o.Tolerance, o.Standardization, o.Missing, o.EmptyCluster, o.Seed)
                        model.Fit()
                    Case 8
                        Dim model As New KMeans
                        model.dataInputs(Multivariate17Fixture.Data())
                        model.startingCentersInputs(New Double(,) {{1.0}})
                        Dim o As New MultivariateKMeansOptions With {.Initialization = KMeansInitializationMethod.UserSpecifiedCenters, .Seed = 1729}
                        If current Then o.Configure(model) Else model.settingsInputs(initialization:=o.Initialization, randomSeed:=1729)
                        model.Fit()
                    Case 9
                        Dim model As New CA : model.DataMultiple(New String(0, 0) {}, {"x"}) : model.Calculate()
                    Case 10
                        Dim model As New DiscriminantAnalysis, o As New MultivariateDiscriminantOptions With {.Priors = DiscriminantPriorMode.UserSpecified}
                        model.dataInputs(Multivariate17Fixture.Data(), Enumerable.Repeat(CObj("a"), 30).ToArray())
                        If current Then o.ConfigureModel(model) Else model.settingsInputs(priorMode:=DiscriminantPriorMode.UserSpecified)
                        model.priorInputs({CObj("missing")}, {-1.0}) : model.Fit()
                    Case 11
                        Dim model As New FactorAnalysis, o As New MultivariateFactorOptions With {.MaximumIterations = 1, .Epsilon = Double.NaN}
                        model.dataInputs(Multivariate17Fixture.Data(True), Multivariate17Fixture.Rows(), Multivariate17Fixture.Names)
                        If current Then o.Configure(model) Else model.settingsInputs(1, Double.NaN, rotationMethod:=FactorAnalysisRotationMethod.Varimax)
                        model.Calculate()
                End Select
            End Sub
        Try : callCore(False) : Catch ex As Exception : expected = ex : End Try
        Try : callCore(True) : Catch ex As Exception : actual = ex : End Try
        If expected Is Nothing Then
            Assert.IsNull(actual, "A completed boundary call is not independent scientific validation.")
        Else
            Assert.IsNotNull(actual) : Assert.AreEqual(expected.GetType(), actual.GetType()) : Assert.AreEqual(expected.Message, actual.Message)
        End If
        Console.WriteLine($"04A17_NEGATIVE|{caseNumber}|{If(expected Is Nothing, "RETURNED", expected.GetType().Name)}")
    End Sub
End Class
