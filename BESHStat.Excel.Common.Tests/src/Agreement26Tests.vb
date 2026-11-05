Option Explicit On
Option Strict On
Option Infer On
Imports System
Imports System.Collections.Generic
Imports System.Globalization
Imports System.Linq
Imports System.Xml.Linq
Imports BESHStatNG
Imports BESHStatNG.Agreement
Imports BESHStatNG.ExcelCommon
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass>
Public Class Agreement26Tests
    Public Shared Iterator Function OptionCases() As IEnumerable(Of Object())
        For mask = 0 To 15
            For model = -1 To 4 : Yield New Object() {mask, model} : Next
        Next
    End Function
    <DataTestMethod, DynamicData(NameOf(OptionCases), DynamicDataSourceType.Method)>
    Public Sub DemingOriginalPrecedenceVarianceAndExecution(mask As Integer, model As Integer)
        Dim p = (mask And 1) <> 0, b = (mask And 2) <> 0, a = (mask And 4) <> 0, m = (mask And 8) <> 0
        Dim expected = If(p, AgreementCiMethod.BootstrapPercentile, If(b, AgreementCiMethod.BootstrapBCa, If(a OrElse m, AgreementCiMethod.Analytical, AgreementCiMethod.Jackknife)))
        Dim parsed As New DemingOptions With {.Alpha = 0.1R, .VarianceModel = DemingRequestSelection.VarianceForIndex(model), .SDx = {0.2R, Double.NaN}, .SDy = {0.3R, 0.4R}, .FitIntercept = False}
        Dim request As New DemingRequestSelection(parsed, p, b, a, m), options = request.CreateOptions()
        Assert.AreEqual(expected, options.CiMethod)
        Assert.AreEqual(If(model = 1, DemingVarianceModel.ConstantCV, If(model = 2, DemingVarianceModel.KnownPointwiseSD, DemingVarianceModel.ConstantLambda)), options.VarianceModel)
        Assert.AreEqual(If(expected = AgreementCiMethod.Analytical, If(m, DemingExecutionIntent.AnalyticalMcr, DemingExecutionIntent.AnalyticalClosedForm), If(expected = AgreementCiMethod.Jackknife, DemingExecutionIntent.Jackknife, DemingExecutionIntent.Bootstrap)), request.Execution)
        parsed.SDx(0) = 88 : options.SDy(0) = 99
        Assert.AreEqual(0.2R, request.CreateOptions().SDx(0)) : Assert.AreEqual(0.3R, request.CreateOptions().SDy(0))
        Assert.IsTrue(Double.IsNaN(request.CreateOptions().SDx(1)))
    End Sub
    Public Shared Iterator Function KappaCases() As IEnumerable(Of Object())
        For mask = 0 To 15 : For weight = -1 To 5 : Yield New Object() {mask, weight} : Next : Next
    End Function
    <DataTestMethod, DynamicData(NameOf(KappaCases), DynamicDataSourceType.Method)>
    Public Sub KappaAllWeightsAndConflictingRadioPrecedence(mask As Integer, weight As Integer)
        Dim a = (mask And 1) <> 0, j = (mask And 2) <> 0, p = (mask And 4) <> 0, b = (mask And 8) <> 0
        Dim request As New KappaRequestSelection(New KappaOptions With {.Alpha = 0.1R, .BootstrapReplicates = 17}, weight, a, j, p, b), options = request.CreateOptions()
        Assert.AreEqual(If(a, AgreementCiMethod.Analytical, If(j, AgreementCiMethod.Jackknife, If(p, AgreementCiMethod.BootstrapPercentile, If(b, AgreementCiMethod.BootstrapBCa, AgreementCiMethod.Analytical)))), options.CiMethod)
        Assert.AreEqual(If(weight >= 0 AndAlso weight <= 4, CType(weight, KappaWeightingScheme), KappaWeightingScheme.Unweighted), options.Weighting)
        Assert.AreEqual(options.CiMethod <> AgreementCiMethod.Analytical, request.UsesResampling)
        Assert.AreEqual(17, options.BootstrapReplicates)
    End Sub
    <DataTestMethod><DataRow(0)><DataRow(1)><DataRow(2)><DataRow(3)><DataRow(4)><DataRow(5)><DataRow(6)><DataRow(7)>
    Public Sub LinOriginalRadioPriorityAndDetachedSubjects(mask As Integer)
        Dim a = (mask And 1) <> 0, p = (mask And 2) <> 0, b = (mask And 4) <> 0
        Dim source As New LinConcordanceOptions With {.SubjectIds = {"Ω", 4}, .NullConcordance = 0.4R}
        Dim request As New LinRequestSelection(source, a, p, b), options = request.CreateOptions()
        Assert.AreEqual(If(a, AgreementCiMethod.Analytical, If(p, AgreementCiMethod.BootstrapPercentile, If(b, AgreementCiMethod.BootstrapBCa, AgreementCiMethod.Analytical))), options.CiMethod)
        Assert.AreEqual(options.CiMethod <> AgreementCiMethod.Analytical, request.UsesBootstrap)
        source.SubjectIds(0) = "changed" : options.SubjectIds(1) = -1
        Assert.AreEqual("Ω", CStr(request.CreateOptions().SubjectIds(0)))
        Assert.AreEqual(4, CInt(request.CreateOptions().SubjectIds(1)))
    End Sub
    Public Shared Iterator Function IccCases() As IEnumerable(Of Object())
        For mask = 0 To 63 : Yield New Object() {mask} : Next
    End Function
    <DataTestMethod, DynamicData(NameOf(IccCases), DynamicDataSourceType.Method)>
    Public Sub IccAllSixChoicesAndNoSelectionRetainFirstCheckedAndLabels(mask As Integer)
        Dim flags = Enumerable.Range(0, 6).Select(Function(i) (mask And (1 << i)) <> 0).ToArray()
        Dim intent As New IccRequestIntent(flags(0), flags(1), flags(2), flags(3), flags(4), flags(5), 0.1R, True)
        Dim expected = Array.FindIndex(flags, Function(v) v)
        Assert.AreEqual(expected, CInt(intent.Choice))
        Dim labels = {"ICC(1,1)", "ICC(1,k)", "ICC(2,1)", "ICC(2,4)", "ICC(3,1)", "ICC(3,4)"}
        Assert.AreEqual(If(expected < 0, "", labels(Math.Max(0, expected))), intent.Label(4))
    End Sub
    Public Shared Iterator Function FitCases() As IEnumerable(Of Object())
        For Each culture In {"", "sk-SK"} : For kind = 0 To 4 : For profile = 0 To 7 : Yield New Object() {kind, profile, culture} : Next : Next : Next
    End Function
    <DataTestMethod, DynamicData(NameOf(FitCases), DynamicDataSourceType.Method)>
    Public Sub SameCoreTablesAndStoredAxesMatchOriginalPresentation(kind As Integer, profile As Integer, culture As String)
        Dim saved = CultureInfo.CurrentCulture
        Try
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(culture)
            Dim x = Enumerable.Range(0, 12).Select(Function(i) 10.0R + i * 2).ToArray()
            Dim y = x.Select(Function(v, i) 1.1R * v + 1 + Math.Sin(i)).ToArray()
            Dim rows = Enumerable.Range(0, 12).Select(Function(i) 20 + i * 3).Reverse().ToArray()
            Dim subjects As Object() = Enumerable.Range(0, 12).Select(Function(i) CObj("subject Ω " & (i \ 3).ToString())).ToArray()
            If profile = 7 Then x(0) = Double.NaN
            Dim axis As New AgreementInputAxis(12, {"reference", "test"}, rows, x, y, subjects)
            Dim effectiveInput As Object = New Object() {x, y}, effectiveRows = rows
            Dim tables As List(Of ResultTable) = Nothing, fitResult As Object = Nothing, options As Object = Nothing
            Dim failure = Planning26Tests.Failure(Sub()
                Select Case kind
                    Case 0
                        Dim opts As New DemingOptions With {.VarianceModel = CType(profile Mod 3, DemingVarianceModel), .SDx = Enumerable.Repeat(0.5R, 12).ToArray(), .SDy = Enumerable.Repeat(0.6R, 12).ToArray(), .CVx = 0.05R, .CVy = 0.06R, .BootstrapReplicates = 200, .FitIntercept = profile Mod 2 = 0}
                        If profile = 6 Then opts.SDx(0) = 0 : opts.VarianceModel = DemingVarianceModel.KnownPointwiseSD
                        Dim request As New DemingRequestSelection(opts, profile = 3, profile = 4, profile = 0, profile = 1)
                        opts = request.CreateOptions() : options = opts
                        Dim engine As New WeightedDemingRegression(axis.First(), axis.Second(), "reference", "test") With {.Options = opts}
                        Select Case request.Execution
                            Case DemingExecutionIntent.AnalyticalClosedForm : fitResult = engine.DemingAnalyticalCI()
                            Case DemingExecutionIntent.AnalyticalMcr : fitResult = engine.DemingAnalyticalCI_MCR()
                            Case DemingExecutionIntent.Jackknife : fitResult = engine.FitJackknifeCI()
                            Case Else : fitResult = engine.Fit(randomSeed:=12345)
                        End Select
                        tables = engine.wrapResults()
                    Case 1
                        Dim request As New LinRequestSelection(New LinConcordanceOptions With {.Alpha = 0.1R, .NullConcordance = 0.3R, .BootstrapReplicates = If(profile = 7, 30, 200)}, profile Mod 3 = 0, profile Mod 3 = 1, profile Mod 3 = 2)
                        Dim opts = request.CreateOptions() : options = opts
                        Dim engine As New LinConcordanceCorrelation(axis.First(), axis.Second(), "reference", "test") With {.Options = opts}
                        fitResult = engine.Fit(randomSeed:=12345) : tables = engine.wrapResults()
                    Case 2
                        Dim first As Object() = {"a", "b", "Ω", "a", "b", "Ω", "a", "b", "Ω", "a", "b", "Ω"}
                        Dim second As Object() = {"a", "b", "Ω", "b", "b", "Ω", "a", "a", "Ω", "a", "b", "Ω"}
                        If profile = 7 Then first(0) = Nothing : second(1) = ""
                        effectiveInput = New Object() {first, second}
                        axis = New AgreementInputAxis(12, {"rater1", "rater2"}, rows, categoricalFirst:=first, categoricalSecond:=second, header:=True)
                        Dim request As New KappaRequestSelection(New KappaOptions With {.BootstrapReplicates = 200}, profile Mod 5, profile < 5, profile = 5, profile = 6, profile = 7)
                        Dim opts = request.CreateOptions() : options = opts
                        Dim engine As New WeightedKappaAgreement(axis.FirstCategories(), axis.SecondCategories(), "rater1", "rater2") With {.Options = opts}
                        fitResult = engine.Fit(randomSeed:=12345) : tables = engine.wrapResults()
                    Case 3
                        Dim raw = PairedOptionPolicy.ProjectBland(profile Mod 3, profile Mod 5, profile = 4, profile = 5, profile = 6)
                        raw.SubjectIds = If(profile Mod 2 = 0, subjects, Nothing) : raw.BootstrapReplicates = If(profile = 7, 30, 200)
                        Dim request As New BlandRequestSelection(raw, profile Mod 3, profile Mod 3), opts = request.CreateOptions()
                        options = opts
                        Dim engine As New BlandAltmanAgreement(axis.First(), axis.Second(), "reference", "test", opts)
                        fitResult = engine.Fit(randomSeed:=12345) : tables = engine.wrapResults()
                    Case Else
                        Dim matrix As Double(,) = {{10, 11, 12}, {20, 19, 21}, {30, 29, 28}, {40, 41, 39}, {50, 49, 48}}
                        effectiveRows = rows.Take(matrix.GetLength(0)).ToArray()
                        axis = New AgreementInputAxis(matrix.GetLength(0), {"rater1", "rater2", "rater3"}, effectiveRows,
                            subjectIds:=Enumerable.Range(0, matrix.GetLength(0)).Select(Function(i) CObj("ICC subject " & i.ToString())).ToArray())
                        Dim engine As New IntraclassCorrelation, ci As ConfidenceIntervalResult = Nothing
                        Dim choice = profile Mod 6, flags = Enumerable.Range(0, 6).Select(Function(i) i = choice).ToArray()
                        Dim intent As New IccRequestIntent(flags(0), flags(1), flags(2), flags(3), flags(4), flags(5), 0.05R, profile Mod 2 = 0)
                        options = intent
                        Dim jagged = Enumerable.Range(0, 5).Select(Function(i) New Double() {matrix(i, 0), matrix(i, 1), matrix(i, 2)}).ToArray()
                        effectiveInput = If(choice < 2, CObj(jagged), CObj(matrix))
                        Select Case intent.Choice
                            Case IccEstimatorChoice.OneOne : ci = engine.ICC11(jagged, intent.Alpha)
                            Case IccEstimatorChoice.OneAverage : ci = engine.ICC1k(jagged, intent.Alpha)
                            Case IccEstimatorChoice.TwoOne : ci = engine.ICC21(matrix, intent.Alpha)
                            Case IccEstimatorChoice.TwoAverage : ci = engine.ICC2k(matrix, intent.Alpha)
                            Case IccEstimatorChoice.ThreeOne : ci = engine.ICC31(matrix, intent.Alpha)
                            Case IccEstimatorChoice.ThreeAverage : ci = engine.ICC3k(matrix, intent.Alpha)
                        End Select
                        If intent.Repeatability Then
                            If choice < 2 Then
                                engine.RepeatabilityCoefficient_OneWay(jagged, choice = 1, intent.Alpha)
                            Else
                                engine.RepeatabilityCoefficient_TwoWay(matrix, choice < 4, choice = 3 OrElse choice = 5, intent.Alpha)
                            End If
                        End If
                        fitResult = ci : tables = engine.wrapResults(ci, intent.Label(3))
                End Select
            End Sub)
            Dim reference As (Result As Object, Tables As List(Of ResultTable), Options As Object)
            Dim originalFailure = Planning26Tests.Failure(Sub() reference = Agreement26Reference.Run(kind, profile, x, y, rows, subjects))
            Assert.AreEqual(If(originalFailure Is Nothing, "", originalFailure.GetType().FullName), If(failure Is Nothing, "", failure.GetType().FullName))
            Assert.AreEqual(If(originalFailure Is Nothing, "", originalFailure.Message), If(failure Is Nothing, "", failure.Message))
            Dim node As New XElement("comparison", New XAttribute("family", "agreement"), New XAttribute("method", kind), New XAttribute("profile", profile), New XAttribute("culture", culture), New XAttribute("runtime", Environment.Version.ToString()), New XAttribute("status", "NOT_GOLDEN"), New XAttribute("purpose", "SAME_CORE_COMPATIBILITY_ONLY"), New XElement("fixture_sha256", Spc23Fixture.Hash(New Object() {effectiveInput, effectiveRows, axis.Names, axis.SubjectIds()})), New XElement("options_sha256", Spc23Fixture.Hash(options)), New XElement("seed_test_only", 12345), New XElement("input", Snapshot(effectiveInput)), New XElement("input_axis", Snapshot(axis)), New XElement("effective_options", Snapshot(options)))
            If failure IsNot Nothing Then
                node.Add(New XAttribute("classification", "PRESERVED_CORE_REJECTION"), New XElement("exception", New XAttribute("type", failure.GetType().FullName), failure.Message))
            Else
                Assert.AreEqual(Spc23Fixture.Hash(reference.Result), Spc23Fixture.Hash(fitResult), "Existing seeded Core calculation with original options/dispatch")
                If kind = 4 Then
                    Dim oldIntent = DirectCast(reference.Options, Agreement26Legacy.Icc), intent = DirectCast(options, IccRequestIntent)
                    Assert.AreEqual(oldIntent.Choice, intent.Choice) : Assert.AreEqual(oldIntent.Alpha, intent.Alpha)
                    Assert.AreEqual(oldIntent.Repeatability, intent.Repeatability) : Assert.AreEqual(oldIntent.Label(3), intent.Label(3))
                Else
                    Assert.AreEqual(Spc23Fixture.Hash(reference.Options), Spc23Fixture.Hash(options), "Exact original option fields")
                End If
                Dim method = CType(kind, AgreementMethod), plan As New AgreementReportPlan(method, tables, axis)
                Assert.AreEqual(Spc23Fixture.Hash(tables), Spc23Fixture.Hash(plan.TablesForWriter()))
                Assert.AreEqual(kind = 0 OrElse kind = 1 OrElse kind = 3, plan.PlotRequested)
                Assert.AreEqual(If(kind = 3, 10 + tables.Count + tables.Count - 1, 10 + tables.Count - 1), plan.NativeRowSpan(10))
                Assert.AreEqual(If(kind = 4, 5, 12), plan.Axis.Count)
                For i = 0 To axis.Count - 1 : Assert.AreEqual(effectiveRows(i), plan.Axis.PhysicalRows(i).Value) : Next
                Dim copied = plan.TablesForWriter() : copied.Clear()
                Assert.AreEqual(tables.Count, plan.TablesForWriter().Count)
                node.Add(New XAttribute("classification", "POSITIVE_PARITY"), New XElement("result_sha256", Spc23Fixture.Hash(fitResult)), New XElement("axis_sha256", Spc23Fixture.Hash(axis)), New XElement("table_count", tables.Count), New XElement("tables_sha256", Spc23Fixture.Hash(tables)), New XElement("plot", plan.PlotRequested))
                node.Add(New XElement("core_result", Snapshot(fitResult)), New XElement("core_tables", Snapshot(tables.Select(Function(t) t.ToOutputModel()).ToArray())))
            End If
            Planning26Tests.Emit(node)
        Finally
            CultureInfo.CurrentCulture = saved
        End Try
    End Sub
    Public Shared Function Snapshot(value As Object, Optional depth As Integer = 0) As XElement
        If depth > 24 Then Throw New InvalidOperationException("Unexpected cyclic fixture value.")
        Dim node As New XElement("value")
        If value Is Nothing Then
            node.Add(New XAttribute("missing", True)) : Return node
        End If
        Dim kind = value.GetType()
        node.Add(New XAttribute("type", kind.FullName))
        If TypeOf value Is Double Then
            node.Add(New XAttribute("ieee_bits", Spc23Fixture.Bits(CDbl(value))))
            node.Value = CDbl(value).ToString("R", CultureInfo.InvariantCulture)
        ElseIf kind.IsEnum OrElse kind.IsPrimitive OrElse TypeOf value Is String OrElse TypeOf value Is Decimal Then
            node.Value = Convert.ToString(value, CultureInfo.InvariantCulture)
        ElseIf TypeOf value Is Array Then
            Dim array = DirectCast(value, Array)
            node.Add(New XAttribute("dimensions", String.Join(",", Enumerable.Range(0, array.Rank).Select(Function(i) array.GetLength(i)))))
            For Each item As Object In array : node.Add(Snapshot(item, depth + 1)) : Next
        ElseIf TypeOf value Is System.Collections.IEnumerable Then
            For Each item As Object In DirectCast(value, System.Collections.IEnumerable) : node.Add(Snapshot(item, depth + 1)) : Next
        Else
            For Each propertyInfo In kind.GetProperties(System.Reflection.BindingFlags.Public Or System.Reflection.BindingFlags.Instance).Where(Function(p) p.CanRead AndAlso p.GetIndexParameters().Length = 0).OrderBy(Function(p) p.Name, StringComparer.Ordinal)
                node.Add(New XElement("property", New XAttribute("name", propertyInfo.Name), Snapshot(propertyInfo.GetValue(value, Nothing), depth + 1)))
            Next
            For Each field In kind.GetFields(System.Reflection.BindingFlags.Public Or System.Reflection.BindingFlags.Instance).OrderBy(Function(f) f.Name, StringComparer.Ordinal)
                node.Add(New XElement("field", New XAttribute("name", field.Name), Snapshot(field.GetValue(value), depth + 1)))
            Next
        End If
        Return node
    End Function
    <TestMethod>
    Public Sub AxisCopiesValuesLabelsSubjectsAndUnknownSourceIsNeverInvented()
        Dim x = {1.0R, Double.NaN, 3.0R}, y = {2.0R, 4.0R, 6.0R}, ids As Object() = {"same", "same", Nothing}, rows = {30, 10, 90}, names = {"Ω", "Ω"}
        Dim axis As New AgreementInputAxis(3, names, rows, x, y, ids)
        x(0) = 99 : rows(0) = 1 : names(0) = "changed" : ids(0) = "changed"
        Assert.AreEqual(1.0R, axis.First()(0)) : Assert.IsTrue(Double.IsNaN(axis.First()(1)))
        Assert.AreEqual(30, axis.PhysicalRows(0).Value) : Assert.AreEqual("Ω", axis.Names(0))
        Assert.AreEqual("same", CStr(axis.SubjectIds()(0))) : Assert.IsNull(axis.SubjectIds()(2))
        Dim unknown As New AgreementInputAxis(3, Nothing, Nothing)
        Assert.IsFalse(unknown.PhysicalRows(0).HasValue) : Assert.IsNull(unknown.Alignment)
        Dim copied = axis.First() : copied(0) = 100 : Assert.AreEqual(1.0R, axis.First()(0))
    End Sub
    <TestMethod>
    Public Sub AdditionalNestedOptionsBlankSubjectsAndLegacyBlandReplicateFallbackAreDetached()
        Dim source As New KappaOptions With {.CustomWeights = New Double(,) {{1, 0}, {0, 1}}, .Categories = {1, "1", "Ω"}}
        Dim request As New KappaRequestSelection(source, 2, True, False, False, False)
        source.CustomWeights(0, 0) = 99 : source.Categories(0) = "changed"
        Dim first = request.CreateOptions() : first.CustomWeights(1, 1) = 88
        Assert.AreEqual(1.0R, request.CreateOptions().CustomWeights(0, 0))
        Assert.AreEqual(1.0R, request.CreateOptions().CustomWeights(1, 1))
        Assert.IsInstanceOfType(request.CreateOptions().Categories(0), GetType(Integer))
        Dim raw = PairedOptionPolicy.ProjectBland(2, 4, True, True, True)
        raw.SubjectIds = {Nothing, "", "same", "same"} : raw.BootstrapReplicates = 0
        Dim bland As New BlandRequestSelection(raw, 7, 7), options = bland.CreateOptions()
        raw.SubjectIds(2) = "changed" : options.SubjectIds(3) = "changed"
        Assert.AreEqual(2000, bland.CreateOptions().BootstrapReplicates)
        Assert.AreEqual(AgreementCiMethod.BootstrapPercentile, options.CiMethod)
        Assert.AreEqual(BlandAltmanXAxisMode.MeanOfMethods, options.XAxisMode)
        Assert.AreEqual(RepeatedBlandAltmanPlotMode.AllObservations, options.PlotMode)
        Assert.AreEqual("same", CStr(bland.CreateOptions().SubjectIds(2)))
        Assert.IsNull(bland.CreateOptions().SubjectIds(0))
        Dim shape As New AgreementInputAxis(3, {"x", "y"}, {50}, categoricalFirst:=New Object() {1, "1", "Ω"}, categoricalSecond:=New Object() {1, "1", "Ω"})
        Assert.AreEqual(1, shape.CandidateDiagnostics.Issues.Count)
        Assert.IsFalse(shape.PhysicalRows(1).HasValue)
    End Sub
End Class
