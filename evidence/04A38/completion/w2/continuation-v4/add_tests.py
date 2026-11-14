from common import *
import shutil
p='BESHStat.Excel.Common/src/Results/UdfTypedReportPreparation38.vb';src=ROOT/p;dst=HERE/'attempts/report-compile-v1.vb.txt';dst.parent.mkdir(exist_ok=True);shutil.copyfile(src,dst);s=src.read_text(encoding='utf-8-sig');s=s.replace('    <summary>Existing Core ANOVA orchestration and report metadata; host output adaptation remains at the caller.</summary>',"    ' Existing Core ANOVA orchestration/report metadata; output adaptation remains at the host.");write_source(p,s)
def append(path,body):
 s=(ROOT/path).read_text(encoding='utf-8-sig');i=s.rindex('End Class');write_source(path,s[:i]+body+'\n'+s[i:])
append('BESHStatNG.Test/src/UDFs/UdfPreparation38AdapterTests.vb','''
    <TestMethod>
    Public Sub InversionHostCatchRetainsFrameworkErrAndByRefContract()
        Dim regular As Double(,) = {{4.0, 1.0}, {1.0, 3.0}}
        For Each a As Double(,) In New Double(,)() {Nothing, New Double(-1, -1) {}, New Double(1, 2) {}, regular}
            Microsoft.VisualBasic.Err.Clear()
            Microsoft.VisualBasic.Err.Number = 5
            Microsoft.VisualBasic.Err.Description = "current-seed"
            Dim inverse As Double(,) = {{42.0}}
            Dim ok As Boolean = UdfLinearAlgebra.TryInvertMatrix(a, inverse)
            Assert.AreEqual(5, Microsoft.VisualBasic.Err.Number)
            Assert.AreEqual("current-seed", Microsoft.VisualBasic.Err.Description)
            If a Is regular Then
                Assert.IsTrue(ok)
                Assert.AreEqual(3.0 / 11.0, inverse(0, 0), 0.0000000001)
                Assert.AreNotSame(a, inverse)
            Else
                Assert.IsFalse(ok)
                Assert.IsNull(inverse)
            End If
        Next
        For Each a As Double(,) In New Double(,)() {New Double(,) {{1.0, 2.0}, {2.0, 4.0}}, New Double(,) {{1.0, 0.0}, {0.0, 0.00000000000000000001}}, DirectCast(Array.CreateInstance(GetType(Double), {2, 2}, {1, 1}), Double(,))}
            Microsoft.VisualBasic.Err.Clear()
            Microsoft.VisualBasic.Err.Number = 5
            Dim inverse As Double(,) = {{42.0}}
            Assert.IsFalse(UdfLinearAlgebra.TryInvertMatrix(a, inverse))
            Assert.IsNull(inverse)
            Assert.AreEqual(0, Microsoft.VisualBasic.Err.Number)
            Assert.AreEqual(String.Empty, Microsoft.VisualBasic.Err.Description)
            Assert.IsNull(Microsoft.VisualBasic.Err.GetException())
        Next
        Dim aliasInput As Double(,) = DirectCast(regular.Clone(), Double(,))
        Assert.IsTrue(UdfLinearAlgebra.TryInvertMatrix(aliasInput, aliasInput))
        Assert.AreEqual(3.0 / 11.0, aliasInput(0, 0), 0.0000000001)
        Assert.AreEqual(4.0, regular(0, 0))
        Try
            Throw New InvalidOperationException("current-only outer catch")
        Catch
            Dim inverse As Double(,) = Nothing
            Assert.IsNotNull(Microsoft.VisualBasic.Err.GetException())
            Assert.IsTrue(UdfLinearAlgebra.TryInvertMatrix(regular, inverse))
            Assert.IsNotNull(Microsoft.VisualBasic.Err.GetException())
            Assert.IsFalse(UdfLinearAlgebra.TryInvertMatrix(New Double(,) {{0.0}}, inverse))
            Assert.IsNull(Microsoft.VisualBasic.Err.GetException())
        End Try
        Microsoft.VisualBasic.Err.Clear()
    End Sub

    Private Delegate Function EqualVarianceParser38(arg As Object, defaultEqual As Boolean) As Boolean
    Private NotInheritable Class VarianceText38
        Public ReadOnly Effect As Func(Of String)
        Public Sub New(effect As Func(Of String))
            Me.Effect = effect
        End Sub
        Public Overrides Function ToString() As String
            Return Effect()
        End Function
    End Class

    <DataTestMethod> <DataRow("")> <DataRow("sk-SK")>
    Public Sub VarianceAliasesAndExceptionalConversionUseShippingBoundary(cultureName As String)
        Dim old = CultureInfo.CurrentCulture
        Dim oldUi = CultureInfo.CurrentUICulture
        Try
            CultureInfo.CurrentCulture = If(cultureName = "", CultureInfo.InvariantCulture, New CultureInfo(cultureName))
            CultureInfo.CurrentUICulture = CultureInfo.CurrentCulture
            Dim method = GetType(ParametricUDFs).GetMethod("ParseEqualVarianceMode", BindingFlags.NonPublic Or BindingFlags.Static)
            Dim parse = DirectCast([Delegate].CreateDelegate(GetType(EqualVarianceParser38), method), EqualVarianceParser38)
            For Each blank As Object In New Object() {Nothing, ExcelMissing.Value, ExcelEmpty.Value, DBNull.Value, "", "  "}
                Assert.IsTrue(parse(blank, True))
                Assert.IsFalse(parse(blank, False))
            Next
            For Each text As String In {"welch", "UNEQUAL", "assume unequal", "assume-unequal"}
                Assert.IsFalse(parse(text, True))
            Next
            For Each text As String In {"EQUAL", "pooled", "Student", "assume equal", "assume-equal"}
                Assert.IsTrue(parse(text, False))
            Next
            For Each value As Object In New Object() {"assume_equal", "bad", ExcelError.ExcelErrorNA}
                Assert.ThrowsException(Of ArgumentException)(Sub() parse(value, True))
            Next
            Dim calls As Integer = 0
            Assert.IsTrue(parse(New VarianceText38(Function()
                                                     calls += 1
                                                     Return " pooled "
                                                 End Function), False))
            Assert.AreEqual(1, calls)
            Assert.ThrowsException(Of InvalidOperationException)(Sub() parse(New VarianceText38(Function() ThrowVarianceConversion38()), True))
            Assert.ThrowsException(Of NullReferenceException)(Sub() parse(New VarianceText38(Function() Nothing), True))
            For Each value As String In {ChrW(0), ChrW(&HAD)}
                Dim expected = ParametricUDFsOptions38.ParseEqualVarianceMode(value, False)
                Assert.AreEqual(expected, parse(value, False))
            Next
        Finally
            CultureInfo.CurrentCulture = old
            CultureInfo.CurrentUICulture = oldUi
        End Try
    End Sub

    Private Shared Function ThrowVarianceConversion38() As String
        Throw New InvalidOperationException("current conversion")
    End Function

    <DataTestMethod> <DataRow("")> <DataRow("sk-SK")>
    Public Sub AnovaShippingReportsPreserveGroupedLayoutAndFaultOrdering(cultureName As String)
        Dim old = CultureInfo.CurrentCulture
        Dim oldUi = CultureInfo.CurrentUICulture
        Try
            CultureInfo.CurrentCulture = If(cultureName = "", CultureInfo.InvariantCulture, New CultureInfo(cultureName))
            CultureInfo.CurrentUICulture = CultureInfo.CurrentCulture
            Dim input As Object(,) = {{"A", "B", "C"}, {1.0, 3.0, 5.0}, {2.0, 4.0, 8.0}, {4.0, 6.0, 9.0}, {5.0, 7.0, 12.0}}
            Dim groups As Double()() = Nothing
            Dim names As String() = Nothing
            Assert.IsTrue(Global.BESHStatNG.UdfDataImport.TryGetGroupedNumericColumns(input, groups, names))
            CollectionAssert.AreEqual(New String() {"A", "B", "C"}, names)
            Dim mdl As New Global.BESHStatNG.parametric.OneWayANOVA(groups, names)
            mdl.compute()
            AssertReport38(UdfOutputTables.PrepareResultTableForUdf(mdl.wrapResults()(0)), DirectCast(ParametricUDFs.ANOVA1(input), Object(,)))
            Dim welch = DirectCast(ParametricUDFs.ANOVA1_WELCH(input), Object(,))
            Assert.AreEqual(2, welch.GetLength(0))
            Assert.AreEqual(5, welch.GetLength(1))
            Assert.AreEqual("Welch ANOVA", welch(1, 0))
            Assert.AreEqual(2, welch(1, 1))
            Assert.AreEqual(GetType(Integer), welch(1, 1).GetType())
            Assert.AreEqual(GetType(Double), welch(1, 2).GetType())
            Assert.AreEqual(GetType(String), welch(1, 4).GetType())
            For Each selector As String In {"", "tukey", "tukey-kramer", "TK", "gh", "games-howell", "games", "lsd", "fisher", "fisher-lsd", "bonf", "bonferroni", "all", "both"}
                Dim result = ParametricUDFs.ANOVA1_MCP(input, Nothing, selector, Nothing)
                Assert.IsInstanceOfType(result, GetType(Object(,)))
                Assert.IsTrue(DirectCast(result, Object(,)).GetLength(0) > 1)
            Next
            Assert.AreEqual(ExcelError.ExcelErrorValue, ParametricUDFs.ANOVA1_MCP(input, Nothing, "invalid"))
            Dim conversion As Integer = 0
            Dim throwing As New VarianceText38(Function()
                                                 conversion += 1
                                                 Return ThrowVarianceConversion38()
                                             End Function)
            Assert.AreEqual(ExcelError.ExcelErrorNum, ParametricUDFs.ANOVA1_MCP(input, Nothing, throwing, 2.0))
            Assert.AreEqual(0, conversion)
            Assert.AreEqual(ExcelError.ExcelErrorNum, ParametricUDFs.ANOVA1(New Object(,) {{1.0}, {2.0}}))
            Assert.AreEqual(ExcelError.ExcelErrorValue, ParametricUDFs.ANOVA1(Nothing))
            Dim sparse As Object(,) = {{"A", "none", "C"}, {1.0, Nothing, 3.0}, {2.0, ExcelError.ExcelErrorNA, 6.0}, {4.0, "ignored", 8.0}}
            Assert.IsTrue(Global.BESHStatNG.UdfDataImport.TryGetGroupedNumericColumns(sparse, groups, names))
            Assert.AreEqual(2, groups.Length)
            CollectionAssert.AreEqual(New Double() {1, 2, 4}, groups(0))
            CollectionAssert.AreEqual(New Double() {3, 6, 8}, groups(1))
            Dim bounded = DirectCast(Array.CreateInstance(GetType(Object), {5, 3}, {2, 4}), Object(,))
            For r As Integer = 0 To 4
                For c As Integer = 0 To 2
                    bounded(r + 2, c + 4) = input(r, c)
                Next
            Next
            AssertReport38(DirectCast(ParametricUDFs.ANOVA1(input), Object(,)), DirectCast(ParametricUDFs.ANOVA1(bounded), Object(,)))
        Finally
            CultureInfo.CurrentCulture = old
            CultureInfo.CurrentUICulture = oldUi
        End Try
    End Sub

    Private Shared Sub AssertReport38(expected As Object(,), actual As Object(,))
        Assert.AreEqual(expected.GetLength(0), actual.GetLength(0))
        Assert.AreEqual(expected.GetLength(1), actual.GetLength(1))
        For r As Integer = 0 To expected.GetLength(0) - 1
            For c As Integer = 0 To expected.GetLength(1) - 1
                Assert.AreEqual(expected(r, c), actual(r, c))
                If expected(r, c) IsNot Nothing Then Assert.AreEqual(expected(r, c).GetType(), actual(r, c).GetType())
            Next
        Next
    End Sub
''')
append('BESHStat.Excel.Common.Tests/src/UdfPreparation38Tests.vb','''
    <DataTestMethod> <DataRow("")> <DataRow("sk-SK")>
    Public Sub AnovaReportContractsRetainEngineOrderAndMetadata(cultureName As String)
        Dim old = CultureInfo.CurrentCulture
        Try
            CultureInfo.CurrentCulture = If(cultureName = "", CultureInfo.InvariantCulture, New CultureInfo(cultureName))
            Dim data As Double()() = {New Double() {1, 2, 4, 5}, New Double() {3, 4, 6, 7}, New Double() {5, 8, 9, 12}}
            Dim names = New String() {" A ", "B", "C"}
            Dim model = ParametricUdfReports38.BuildWelchAnovaTable(data, names).ToOutputModel()
            Assert.AreEqual(1, model.HeaderRows)
            Assert.AreEqual(1, model.LeftHeaderColumns)
            Assert.AreEqual(2, model.Values(1, 1))
            Assert.AreEqual(GetType(Integer), model.Values(1, 1).GetType())
            Assert.AreEqual(GetType(Double), model.Values(1, 2).GetType())
            CollectionAssert.AreEqual(New Integer() {4}, model.PvalueColumns)
            Dim stack As Boolean = False
            Dim all = ParametricUdfReports38.BuildOneWayMcpTables(data, names, "ALL", 0.05, stack)
            Assert.IsTrue(stack)
            Assert.AreEqual(5, all.Count)
            Dim single = ParametricUdfReports38.BuildOneWayMcpTables(data, names, "GH", 0.05, stack)
            Assert.IsFalse(stack)
            Assert.AreEqual(2, single.Count)
            Assert.IsNull(ParametricUdfReports38.BuildOneWayMcpTables(data, names, "invalid", 0.05, stack))
            stack = True
            Assert.ThrowsException(Of NullReferenceException)(Sub() ParametricUdfReports38.BuildOneWayMcpTables(Nothing, names, "invalid", 0.05, stack))
            Assert.IsTrue(stack)
        Finally
            CultureInfo.CurrentCulture = old
        End Try
    End Sub
''')
append('BESHStat.Core.Tests/src/UdfNumericPreparation38Tests.vb','''
    <TestMethod>
    Public Sub InversionPreparedPhasePropagatesEngineFaultAndPreservesValidationOrder()
        Dim rows As Integer = 17
        Dim cols As Integer = 19
        Assert.IsFalse(UdfNumericPreparation38.TryGetSquareMatrixDimensions(Nothing, rows, cols))
        Assert.AreEqual(17, rows)
        Assert.AreEqual(19, cols)
        Assert.IsFalse(UdfNumericPreparation38.TryGetSquareMatrixDimensions(New Double(1, 2) {}, rows, cols))
        Assert.AreEqual(2, rows)
        Assert.AreEqual(3, cols)
        Dim singular As Double(,) = {{1.0, 2.0}, {2.0, 4.0}}
        Dim inverse As Double(,) = {{42.0}}
        Assert.ThrowsException(Of ApplicationException)(Sub() UdfNumericPreparation38.TryInvertPreparedSquareMatrix(singular, 2, 2, inverse))
        Assert.AreEqual(42.0, inverse(0, 0))
        Assert.IsFalse(UdfNumericPreparation38.TryInvertMatrix(singular, inverse))
        Assert.IsNull(inverse)
    End Sub
''')
save('tests-added.json',dict(classification='CURRENT_ONLY;NO_ORIGINAL_REFERENCE',source_files=[pin(ROOT/p) for p in ['BESHStatNG.Test/src/UDFs/UdfPreparation38AdapterTests.vb','BESHStat.Excel.Common.Tests/src/UdfPreparation38Tests.vb','BESHStat.Core.Tests/src/UdfNumericPreparation38Tests.vb']],failed_compile_source=pin(dst)))
