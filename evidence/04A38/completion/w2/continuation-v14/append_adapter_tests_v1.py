from common import *
path='BESHStatNG.Test/src/UDFs/UdfPreparation38AdapterTests.vb';text=(ROOT/path).read_text(encoding='utf-8-sig');assert text.rstrip().endswith('End Class')
addition=r'''
    Private Delegate Function Continuation14Guess(cells As Object(,), explicitPositive As Object, ByRef label As String) As Boolean
    Private Shared Continuation14Label As String
    Private Class Continuation14Cell
        Public Reads As Integer
        Public Value As String = ""
        Public Failure As Exception
        Public Overrides Function ToString() As String
            Reads += 1
            Continuation14Label = "converted"
            If Failure IsNot Nothing Then Throw Failure
            Return Value
        End Function
    End Class

    Private Shared Function Continuation14NativeGuess() As Continuation14Guess
        Dim owner = GetType(MultivariateAnalysisUDFs).Assembly.GetType("BESHStatNG.UdfDataImport", True)
        Dim method = owner.GetMethod("GuessPositiveClass", BindingFlags.Static Or BindingFlags.NonPublic)
        Dim emitted As New DynamicMethod("CurrentOnlyContinuation14Guess", GetType(Boolean),
            New Type() {GetType(Object(,)), GetType(Object), GetType(String).MakeByRefType()}, GetType(UdfPreparation38AdapterTests).Module, True)
        Dim il = emitted.GetILGenerator()
        il.Emit(OpCodes.Ldarg_0) : il.Emit(OpCodes.Ldarg_1) : il.Emit(OpCodes.Ldarg_2)
        il.Emit(OpCodes.Call, method) : il.Emit(OpCodes.Ret)
        Return DirectCast(emitted.CreateDelegate(GetType(Continuation14Guess)), Continuation14Guess)
    End Function

    <DataTestMethod, DoNotParallelize, DataRow("", False), DataRow("sk-SK", False), DataRow("", True), DataRow("sk-SK", True)>
    Public Sub Continuation14ActualNativeAliasIsRetainedOnFalse(culture As String, whitespace As Boolean)
        Dim oldCulture = CultureInfo.CurrentCulture
        Try
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(culture)
            Dim cell As New Continuation14Cell With {.Value = If(whitespace, " ", "")}
            Continuation14Label = "before"
            Assert.IsFalse(Continuation14NativeGuess()(New Object(,) {{"red"}, {"green"}, {"blue"}}, cell, Continuation14Label))
            Assert.AreEqual("converted", Continuation14Label)
            Assert.AreEqual(1, cell.Reads)
        Finally
            CultureInfo.CurrentCulture = oldCulture
        End Try
    End Sub

    <TestMethod, DoNotParallelize>
    Public Sub Continuation14NativeFailureKeepsConversionEffectsAndExactException()
        Dim expected As New InvalidOperationException("conversion exception"), cell As New Continuation14Cell With {.Failure = expected}
        Dim guess = Continuation14NativeGuess()
        Continuation14Label = "before"
        Dim actual = Assert.ThrowsException(Of InvalidOperationException)(Sub() guess(Nothing, cell, Continuation14Label))
        Assert.AreSame(expected, actual)
        Assert.AreEqual("converted", Continuation14Label)
        Assert.AreEqual(1, cell.Reads)
        Continuation14Label = "before"
        Assert.ThrowsException(Of NullReferenceException)(Sub() guess(Nothing, Nothing, Continuation14Label))
        Assert.IsNull(Continuation14Label)
    End Sub

    <TestMethod, DoNotParallelize>
    Public Sub Continuation14NativeStatusConversionAndLowerBoundsKeepAlias()
        Dim guess = Continuation14NativeGuess(), explicitCell As New Continuation14Cell(), statusCell As New Continuation14Cell With {.Value = "red"}
        Continuation14Label = "before"
        Assert.IsFalse(guess(New Object(,) {{statusCell}, {"green"}, {"blue"}}, explicitCell, Continuation14Label))
        Assert.AreEqual("converted", Continuation14Label)
        Assert.IsTrue(statusCell.Reads >= 1)
        Dim cells = DirectCast(Array.CreateInstance(GetType(Object), New Integer() {1, 1}, New Integer() {1, 1}), Object(,))
        Continuation14Label = "before"
        Assert.ThrowsException(Of IndexOutOfRangeException)(Sub() guess(cells, explicitCell, Continuation14Label))
        Assert.AreEqual("converted", Continuation14Label)
    End Sub

    <DataTestMethod, DoNotParallelize, DataRow(""), DataRow("sk-SK")>
    Public Sub Continuation14NativeBinaryAndExplicitErrorsKeepPolicy(culture As String)
        Dim oldCulture = CultureInfo.CurrentCulture
        Try
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(culture)
            Dim guess = Continuation14NativeGuess()
            Assert.IsTrue(guess(New Object(,) {{0}, {1}, {ExcelError.ExcelErrorNA}}, "event", Continuation14Label))
            Assert.AreEqual("event", Continuation14Label)
            Assert.IsTrue(guess(New Object(,) {{0}, {1}}, Nothing, Continuation14Label))
            Assert.AreEqual("1", Continuation14Label)
            Assert.IsTrue(guess(New Object(,) {{"no"}, {"yes"}}, ExcelError.ExcelErrorValue, Continuation14Label))
            Assert.AreEqual("yes", Continuation14Label)
        Finally
            CultureInfo.CurrentCulture = oldCulture
        End Try
    End Sub

    <DataTestMethod, DataRow(""), DataRow("sk-SK")>
    Public Sub Continuation14PsmNativeOptionsKeepCoercionAndDiagnosticPrecedence(culture As String)
        Dim oldCulture = CultureInfo.CurrentCulture
        Try
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(culture)
            Dim method = GetType(PropensityScoreUDFs).GetMethod("BuildFitOptions", BindingFlags.NonPublic Or BindingFlags.Static)
            Dim options = DirectCast(method.Invoke(Nothing, New Object() {"matching", "ATT", "supplied", "ratio=4;seed=7", "ratio=2"}), Global.BESHStatNG.CausalInference.PsmComprehensiveFitOptions)
            Assert.AreEqual(2, options.StandardOptions.MatchingRatio)
            Assert.AreEqual(7, options.StandardOptions.RandomSeed)
            Assert.AreEqual(Global.BESHStatNG.CausalInference.PsmScoreMethod.Supplied, options.StandardOptions.ScoreMethod)
            options = DirectCast(method.Invoke(Nothing, New Object() {"optimal", Nothing, Nothing, "ratio=4;replacement=true", Nothing}), Global.BESHStatNG.CausalInference.PsmComprehensiveFitOptions)
            Assert.AreEqual(1, options.StandardOptions.MatchingRatio)
            Assert.IsFalse(options.StandardOptions.WithReplacement)
            Dim invalid As New Continuation14Cell With {.Failure = New InvalidOperationException("method conversion first")}
            Dim caught = Assert.ThrowsException(Of TargetInvocationException)(Sub() method.Invoke(Nothing, New Object() {invalid, Nothing, Nothing, Nothing, Nothing}))
            Assert.AreSame(invalid.Failure, caught.InnerException)
        Finally
            CultureInfo.CurrentCulture = oldCulture
        End Try
    End Sub

    <TestMethod>
    Public Sub Continuation14PsmUnknownTableDoesNotReadMissingHandleState()
        Dim method = GetType(PropensityScoreUDFs).GetMethod("ResolveNamedTable", BindingFlags.NonPublic Or BindingFlags.Static)
        Dim table = DirectCast(method.Invoke(Nothing, New Object() {Nothing, "unknown"}), Object(,))
        Assert.AreEqual("Unknown PSM table: unknown", table(0, 0))
        Assert.AreEqual(1, table.GetLength(0))
        Dim failure = Assert.ThrowsException(Of TargetInvocationException)(Sub() method.Invoke(Nothing, New Object() {Nothing, "summary"}))
        Assert.IsInstanceOfType(failure.InnerException, GetType(NullReferenceException))
    End Sub

    <TestMethod, DoNotParallelize>
    Public Sub Continuation14PsmHandleAlphaCacheReferenceAndSpecificCleanup()
        Dim owner = GetType(PropensityScoreUDFs), nested = owner.GetNestedType("PsmUdfHandle", BindingFlags.NonPublic)
        Dim value = Activator.CreateInstance(nested, True)
        Assert.AreEqual(0.05R, DirectCast(nested.GetProperty("Alpha").GetValue(value), Double))
        nested.GetProperty("Alpha").SetValue(value, 0.2R)
        Dim cache = DirectCast(owner.GetField("_psmCache", BindingFlags.NonPublic Or BindingFlags.Static).GetValue(Nothing), System.Collections.IDictionary)
        Dim key As String = "PSM:" & Guid.NewGuid().ToString("N")
        cache.Add(key, value)
        Try
            Dim find = owner.GetMethod("TryGetPsmHandle", BindingFlags.NonPublic Or BindingFlags.Static), args = New Object() {key.ToUpperInvariant(), Nothing}
            Assert.AreEqual(True, find.Invoke(Nothing, args))
            Assert.AreSame(value, args(1))
            Assert.AreEqual(0.2R, DirectCast(nested.GetProperty("Alpha").GetValue(args(1)), Double))
            Dim absent = New Object() {ExcelError.ExcelErrorValue, value}
            Assert.AreEqual(False, find.Invoke(Nothing, absent))
            Assert.IsNull(absent(1))
            Assert.AreEqual(True, PropensityScoreUDFs.PS_CLEANUP(key))
            Assert.AreEqual(False, PropensityScoreUDFs.PS_CLEANUP(key))
        Finally
            If cache.Contains(key) Then cache.Remove(key)
        End Try
    End Sub

    <DataTestMethod>
    <DataRow("BESHStatNG.UdfDataImport", "GuessPositiveClass", "BESHStatNG.ExcelCommon.UdfClassLabelPreparation38")>
    <DataRow("BESHStatNG.UdfDataImport", "TryGetPsmData", "BESHStatNG.ExcelCommon.UdfPsmDataPreparation38")>
    <DataRow("BESHStatNG.WorksheetFunctions.PropensityScoreUDFs", "BuildFitOptions", "BESHStatNG.ExcelCommon.UdfPsmDataPreparation38")>
    <DataRow("BESHStatNG.WorksheetFunctions.PropensityScoreUDFs", "ResolveNamedTable", "BESHStatNG.ExcelCommon.UdfPsmDataPreparation38")>
    <DataRow("BESHStatNG.WorksheetFunctions.PropensityScoreUDFs", "DataImportSummaryTable", "BESHStatNG.ExcelCommon.UdfPsmDataPreparation38")>
    Public Sub Continuation14ShippingConsumersReachActualPreparedApis(owner As String, name As String, prepared As String)
        Dim method = GetType(PropensityScoreUDFs).Assembly.GetType(owner, True).GetMethod(name, BindingFlags.Public Or BindingFlags.NonPublic Or BindingFlags.Static)
        Assert.IsTrue(Calls(method).Any(Function(target) target.DeclaringType.FullName = prepared))
    End Sub
'''
text=text[:text.rfind('End Class')]+addition+'\nEnd Class\n';write_source(path,text)
save('new-adapter-test-source-v1.json',dict(path=path,after=pin(ROOT/path),scope='Append only: current managed native alias/reflection-emitted direct call, lower bounds/culture/errors, typed PSM precedence, handle/cache/Alpha and actual compiled consumer assertions. No original oracle or Office execution.'))
