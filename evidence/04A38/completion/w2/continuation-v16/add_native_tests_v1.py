"""Add host-free semantic adapter regressions; do not execute them before freeze."""
from common import *
path=ROOT/'BESHStatNG.Test/src/UDFs/UdfPreparation38AdapterTests.vb'
raw=path.read_bytes();text=raw.decode('utf-8-sig').replace('\r\n','\n')
assert text.rstrip().endswith('End Class') and 'Continuation16ParameterName' not in text
addition='''
    Private Shared Function Continuation16PrivateMember(owner As String, name As String) As MethodInfo
        Return GetType(LmmUDFs).Assembly.GetType("BESHStatNG.WorksheetFunctions." & owner, True).
            GetMethod(name, BindingFlags.Public Or BindingFlags.NonPublic Or BindingFlags.Static)
    End Function

    <DataTestMethod, DataRow("GLMUDFs"), DataRow("GLMNbUDFs")>
    Public Sub Continuation16ParameterNameAllocationKeepsErrorPrecedence(owner As String)
        Dim method = Continuation16PrivateMember(owner, "BuildParameterNames")
        Dim overflow = Assert.ThrowsException(Of TargetInvocationException)(Sub() method.Invoke(Nothing, New Object() {Nothing, Integer.MinValue}))
        Assert.IsInstanceOfType(overflow.InnerException, GetType(OverflowException))
        Dim missing = Assert.ThrowsException(Of TargetInvocationException)(Sub() method.Invoke(Nothing, New Object() {Nothing, 0}))
        Assert.IsInstanceOfType(missing.InnerException, GetType(NullReferenceException))
    End Sub

    <DataTestMethod>
    <DataRow("GLMUDFs", False), DataRow("GLMUDFs", True), DataRow("GLMNbUDFs", False), DataRow("GLMNbUDFs", True)>
    Public Sub Continuation16ParameterNamesPreserveSourceAliasesAndDefaults(owner As String, intercept As Boolean)
        Dim method = Continuation16PrivateMember(owner, "BuildParameterNames")
        Dim handleType = method.GetParameters()(0).ParameterType
        Dim handle = Activator.CreateInstance(handleType, True)
        Dim source As String() = {" A ", ""}
        handleType.GetProperty("InterceptIncluded").SetValue(handle, intercept)
        handleType.GetProperty("VarNames").SetValue(handle, source)
        Dim names = DirectCast(method.Invoke(Nothing, New Object() {handle, 3}), String())
        CollectionAssert.AreEqual(If(intercept, New String() {"Intercept", " A ", "Param3"}, New String() {" A ", "Param2", "Param3"}), names)
        Assert.AreSame(source, handleType.GetProperty("VarNames").GetValue(handle))
        Assert.AreNotSame(source, names)
    End Sub

    <TestMethod>
    Public Sub Continuation16NativeFootnotesKeepNoOpAliasAndPartialWrites()
        Dim append = Continuation16PrivateMember("LinearModelUDFs", "AppendFootnotes")
        Dim drop = Continuation16PrivateMember("LinearModelUDFs", "DropTopRows")
        Dim inplace = Continuation16PrivateMember("LinearModelUDFs", "AppendFootnotesInPlace")
        Dim table As Object(,) = {{1}, {2}}
        Assert.AreSame(table, append.Invoke(Nothing, New Object() {table, Nothing}))
        Assert.AreSame(table, drop.Invoke(Nothing, New Object() {table, 0}))
        Dim empty = DirectCast(drop.Invoke(Nothing, New Object() {table, 2}), Object(,))
        Assert.AreEqual(1, empty.GetLength(0))
        Assert.IsNull(empty(0, 0))
        Dim failure = Assert.ThrowsException(Of TargetInvocationException)(Sub() inplace.Invoke(Nothing, New Object() {table, 1, New String() {"first", "second"}}))
        Assert.IsInstanceOfType(failure.InnerException, GetType(IndexOutOfRangeException))
        Assert.AreEqual("first", table(1, 0))
        Assert.AreEqual(1, table(0, 0))
    End Sub

    <DataTestMethod, DataRow(""), DataRow("sk-SK"), DataRow("tr-TR")>
    Public Sub Continuation16GeeNormalizationAndScaleKeepOriginalPolicy(culture As String)
        Dim previous = CultureInfo.CurrentCulture
        Try
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(culture)
            Dim normalize = Continuation16PrivateMember("GEEUDFs", "GeeNormalizeDesignColumnName")
            Assert.AreEqual("å1β", normalize.Invoke(Nothing, New Object() {" Å_1-β "}))
            Assert.AreEqual(String.Empty, normalize.Invoke(Nothing, New Object() {Nothing}))
            Dim scale = Continuation16PrivateMember("GEEUDFs", "ParseGeeLsmEstimateScale")
            For Each value As Object In New Object() {Nothing, ExcelMissing.Value, ExcelEmpty.Value, "link", "unsupported"}
                Assert.AreEqual(False, scale.Invoke(Nothing, New Object() {value}))
            Next
            For Each value As String In New String() {"response", "MEAN", "probability", "mu"}
                Assert.AreEqual(True, scale.Invoke(Nothing, New Object() {value}))
            Next
        Finally
            CultureInfo.CurrentCulture = previous
        End Try
    End Sub

    <TestMethod>
    Public Sub Continuation16LmmImportedCountKeepsNullAndArrayLengthContract()
        Dim inputType = GetType(LmmUDFs).Assembly.GetType("BESHStatNG.WorksheetFunctions.LmmUDFs+LmmImportedInputs", True)
        Dim value = Activator.CreateInstance(inputType, True)
        Dim count = inputType.GetProperty("N")
        Assert.AreEqual(0, count.GetValue(value))
        inputType.GetProperty("Y").SetValue(value, New Double() {1, 2, 3})
        Assert.AreEqual(3, count.GetValue(value))
        inputType.GetProperty("Y").SetValue(value, Array.Empty(Of Double)())
        Assert.AreEqual(0, count.GetValue(value))
    End Sub

    <TestMethod>
    Public Sub Continuation16LmmExpandedDesignKeepsNativeByRefAliasesAndFailureReset()
        Dim method = Continuation16PrivateMember("LmmUDFs", "TryBuildLmmExpandedDesign")
        Dim raw As Double(,) = {{1, 2}, {3, 4}}
        Dim args As Object() = {raw, New String() {" X1 ", " "}, Nothing, Nothing, True, False, True, "fixedFormula", " Z ", New Double(,) {{99}}, New String() {"prior"}, "prior"}
        Assert.AreEqual(True, method.Invoke(Nothing, args))
        Assert.AreSame(raw, args(9))
        CollectionAssert.AreEqual(New String() {"Z1", "Z2"}, DirectCast(args(10), String()))
        Assert.IsNull(args(11))
        args(0) = Nothing
        Assert.AreEqual(False, method.Invoke(Nothing, args))
        Assert.IsNull(args(9))
        Assert.AreEqual(0, DirectCast(args(10), String()).Length)
        Assert.AreEqual("fixedFormula requires a non-empty predictor matrix.", args(11))
    End Sub

    <TestMethod>
    Public Sub Continuation16LmmNamesKeepNativeNullGuardAndIndependentCopies()
        Dim method = Continuation16PrivateMember("LmmUDFs", "EnsureLmmResultUsesHandleNames")
        method.Invoke(Nothing, New Object() {Nothing})
        Dim handleType = method.GetParameters()(0).ParameterType
        Dim handle = Activator.CreateInstance(handleType, True)
        handleType.GetProperty("FixedEffectNames").SetValue(handle, New String() {"F"})
        method.Invoke(Nothing, New Object() {handle})
        Dim result As New regression.MixedModelResult
        Dim source As String() = {"R"}
        handleType.GetProperty("Result").SetValue(handle, result)
        handleType.GetProperty("RandomEffectNames").SetValue(handle, source)
        method.Invoke(Nothing, New Object() {handle})
        Assert.AreEqual("F", result.FixedEffectNames(0))
        Assert.AreEqual("R", result.RandomCovarianceLabels(0))
        Assert.AreNotSame(source, result.RandomCovarianceLabels)
    End Sub
'''
text=text.rstrip()[:-len('End Class')]+addition+'\nEnd Class\n'
newline='\r\n' if b'\r\n' in raw else '\n'
path.write_bytes((b'\xef\xbb\xbf' if raw.startswith(b'\xef\xbb\xbf') else b'')+text.replace('\n',newline).encode())
save('new-test-source-receipt-v1.json',dict(status='COMBINED_PACKET_SEMANTIC_TESTS_ADDED_NOT_EXECUTED',sources=[pin(path),pin(ROOT/'BESHStat.Excel.Common.Tests/src/UdfContinuation16Tests.vb')],contracts=['Report shapes/culture/p-value placement','Exception identity and partial-write/ByRef state','Native allocation-before-handle failure precedence','Input row order, missingness, native callback order, no partial publication','N/Get count, design aliases/reset and copy-safe result names'],managed_campaigns_executed=0,Python_campaigns_executed=0,Node_campaigns_executed=0,Release=DEFERRED,original_queries=0))
print('Combined semantic Common/native tests added; none executed',flush=True)
