from common import *

def append(path,body):
 s=(ROOT/path).read_text(encoding='utf-8-sig');assert body.splitlines()[2].strip() not in s
 write_source(path,s.rsplit('End Class',1)[0]+body+'End Class\n')

append('BESHStat.Core.Tests/src/UdfNumericPreparation38Tests.vb','''
    <TestMethod>
    Public Sub DistributionPreparationRetainsEngineBitsAndTailOrder()
        For Each tuple As Double() In New Double()() {New Double() {0.0, 20.0, 5.0}, New Double() {3.5, 20.0, 5.0}, New Double() {0.2, 2.5, 2.0}}
            Dim fault As Integer
            Dim raw As Double = BESHStatNG.distributions.Distributions.PRTRNG(tuple(0), tuple(1), tuple(2), fault)
            Assert.AreEqual(0, fault)
            Dim cdf = UdfNumericPreparation38.PrepareStudentizedRangeCdf(tuple(0), tuple(1), tuple(2))
            Assert.IsTrue(cdf.HasValue)
            Assert.AreEqual(BitConverter.DoubleToInt64Bits(raw), BitConverter.DoubleToInt64Bits(cdf.Value))
            Dim expected As Double = 1.0 - raw
            If expected < 0 Then expected = 0.0
            If expected > 1 Then expected = 1.0
            Assert.AreEqual(BitConverter.DoubleToInt64Bits(expected), BitConverter.DoubleToInt64Bits(UdfNumericPreparation38.PrepareStudentizedRangeTail(tuple(0), tuple(1), tuple(2)).Value))
        Next
        Dim density = BESHStatNG.distributions.Distributions.F_PDF(1.25, 5.0, 10.0)
        Assert.AreEqual(BitConverter.DoubleToInt64Bits(density), BitConverter.DoubleToInt64Bits(UdfNumericPreparation38.PrepareFDistributionDensity(1.25, 5.0, 10.0).Value))
    End Sub

    <TestMethod>
    Public Sub DistributionPreparationKeepsZeroOverridesAndEngineFaults()
        For Each invalid As Double() In New Double()() {New Double() {Double.NaN, 20.0, 5.0}, New Double() {1.0, Double.NaN, 5.0}, New Double() {1.0, 20.0, Double.NaN}, New Double() {-1.0, 20.0, 5.0}, New Double() {1.0, 0.0, 5.0}, New Double() {1.0, 20.0, 1.0}, New Double() {1.0, 0.25, 5.0}}
            Assert.IsFalse(UdfNumericPreparation38.PrepareStudentizedRangeCdf(invalid(0), invalid(1), invalid(2)).HasValue)
            Assert.IsFalse(UdfNumericPreparation38.PrepareStudentizedRangeTail(invalid(0), invalid(1), invalid(2)).HasValue)
        Next
        Assert.AreEqual(1.0, UdfNumericPreparation38.PrepareFDistributionDensity(-0.0, 2.0, Double.PositiveInfinity).Value)
        Assert.AreEqual(0.0, UdfNumericPreparation38.PrepareFDistributionDensity(0.0, Double.PositiveInfinity, 10.0).Value)
        Assert.IsFalse(UdfNumericPreparation38.PrepareFDistributionDensity(0.0, 1.0, 10.0).HasValue)
        Assert.IsFalse(UdfNumericPreparation38.PrepareFDistributionDensity(0.0, 2.0, Double.NaN).HasValue)
        Assert.IsFalse(UdfNumericPreparation38.PrepareFDistributionDensity(Double.PositiveInfinity, 2.0, 10.0).HasValue)
    End Sub
''')
append('BESHStat.Excel.Common.Tests/src/UdfPreparation38Tests.vb','''
    <TestMethod>
    Public Sub SpillStackKeepsIdentityPaddingAndCellReferences()
        Dim marker As New Object()
        Dim a As Object(,) = {{marker, 2.0}}
        Dim b As Object(,) = {{"B"}, {DBNull.Value}}
        Assert.AreSame(a, UdfSpillPreparation38.StackWithBlankRow(a, Nothing))
        Assert.AreSame(b, UdfSpillPreparation38.StackWithBlankRow(Nothing, b))
        Assert.IsNull(UdfSpillPreparation38.StackWithBlankRow(Nothing, Nothing))
        Dim combined = UdfSpillPreparation38.StackWithBlankRow(a, b)
        Assert.AreEqual(4, combined.GetLength(0))
        Assert.AreEqual(2, combined.GetLength(1))
        Assert.AreEqual(0, combined.GetLowerBound(0))
        Assert.AreSame(marker, combined(0, 0))
        Assert.IsNull(combined(1, 0)) : Assert.IsNull(combined(1, 1))
        Assert.AreEqual("B", combined(2, 0)) : Assert.IsNull(combined(2, 1))
        Assert.AreSame(DBNull.Value, combined(3, 0))
        combined(0, 0) = "changed"
        Assert.AreSame(marker, a(0, 0))
        Dim empty = UdfSpillPreparation38.StackWithBlankRow(New Object(-1, 0) {}, New Object(-1, 1) {})
        Assert.AreEqual(1, empty.GetLength(0)) : Assert.AreEqual(2, empty.GetLength(1))
    End Sub

    <TestMethod>
    Public Sub BlankProjectionDoesNotCoerceCellsAndKeepsOriginalBoundsFailure()
        Dim marker As New Object()
        Dim negativeZero As Double = BitConverter.Int64BitsToDouble(Long.MinValue)
        Dim original As Object(,) = {{Nothing, DBNull.Value, marker, negativeZero, Double.NaN}}
        Dim result = UdfSpillPreparation38.NormalizeBlanks(original)
        Assert.AreNotSame(original, result)
        Assert.AreEqual(String.Empty, result(0, 0)) : Assert.AreEqual(String.Empty, result(0, 1))
        Assert.AreSame(marker, result(0, 2))
        Assert.AreEqual(Long.MinValue, BitConverter.DoubleToInt64Bits(CDbl(result(0, 3))))
        Assert.IsTrue(Double.IsNaN(CDbl(result(0, 4))))
        Assert.IsNull(original(0, 0)) : Assert.AreSame(DBNull.Value, original(0, 1))
        Assert.IsNull(UdfSpillPreparation38.NormalizeBlanks(New Object() {1.0}))
        Assert.IsNull(UdfSpillPreparation38.NormalizeBlanks(Nothing))
        Dim shifted = DirectCast(Array.CreateInstance(GetType(Object), {1, 1}, {1, 1}), Object(,))
        Assert.ThrowsException(Of IndexOutOfRangeException)(Sub() UdfSpillPreparation38.NormalizeBlanks(shifted))
        Assert.AreSame(shifted, UdfSpillPreparation38.StackWithBlankRow(shifted, Nothing))
        Assert.ThrowsException(Of IndexOutOfRangeException)(Sub() UdfSpillPreparation38.StackWithBlankRow(shifted, original))
    End Sub
''')
path='BESHStatNG.Test/src/UDFs/UdfPreparation38AdapterTests.vb';s=(ROOT/path).read_text(encoding='utf-8-sig');needle='    Public Sub CompiledShippingAdapterCallsItsPortablePreparation'
for owner,member,target in [('DistributionUDFs','PRTRNG','BESHStatNG.WorksheetCompatibility.UdfNumericPreparation38'),('DistributionUDFs','PRTRNG_TAIL','BESHStatNG.WorksheetCompatibility.UdfNumericPreparation38'),('DistributionUDFs','F_PDF','BESHStatNG.WorksheetCompatibility.UdfNumericPreparation38'),('ParametricUDFs','StackWithBlankRow','BESHStatNG.ExcelCommon.UdfSpillPreparation38')]:
 s=s.replace(needle,'    <DataRow("BESHStatNG.WorksheetFunctions.'+owner+'", "'+member+'", "'+target+'")>\n'+needle)
# The Object and ResultTable overloads are tested with explicit typed delegate below.
write_source(path,s)
append(path,'''
    Private Delegate Function BlankProjector(table As Object) As Object(,)
    Private Delegate Function StackProjector(a As Object(,), b As Object(,)) As Object(,)

    <DataTestMethod, DataRow(""), DataRow("sk-SK")>
    Public Sub ShippingDistributionRoutePreservesBoxedErrorsAndZeroPolicy(cultureName As String)
        Dim prior = CultureInfo.CurrentCulture, priorUi = CultureInfo.CurrentUICulture
        Try
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(cultureName)
            CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo(cultureName)
            For Each result As Object In {DistributionUDFs.PRTRNG(Double.NaN, 20, 5), DistributionUDFs.PRTRNG(1, 0.25, 5), DistributionUDFs.PRTRNG_TAIL(-1, 20, 5), DistributionUDFs.F_PDF(0, 1, 10), DistributionUDFs.F_PDF(0, 2, Double.NaN)}
                Assert.IsInstanceOfType(result, GetType(ExcelError))
                Assert.AreEqual(ExcelError.ExcelErrorNum, DirectCast(result, ExcelError))
            Next
            Assert.AreEqual(1.0, DirectCast(DistributionUDFs.F_PDF(-0.0, 2, Double.PositiveInfinity), Double))
            Assert.AreEqual(0.0, DirectCast(DistributionUDFs.F_PDF(0, Double.PositiveInfinity, 10), Double))
            Dim cdf = DirectCast(DistributionUDFs.PRTRNG(3.5, 20, 5), Double)
            Assert.AreEqual(1.0 - cdf, DirectCast(DistributionUDFs.PRTRNG_TAIL(3.5, 20, 5), Double))
            Dim density = BESHStatNG.distributions.Distributions.F_PDF(1.25, 5, 10)
            Assert.AreEqual(BitConverter.DoubleToInt64Bits(density), BitConverter.DoubleToInt64Bits(DirectCast(DistributionUDFs.F_PDF(1.25, 5, 10), Double)))
        Finally
            CultureInfo.CurrentCulture = prior : CultureInfo.CurrentUICulture = priorUi
        End Try
    End Sub

    <TestMethod>
    Public Sub ShippingSpillRoutesRetainCellIdentityBoundsAndExcelErrors()
        Dim host = GetType(DistributionUDFs).Assembly
        Dim method = host.GetType("BESHStatNG.UdfOutputTables", True).GetMethod("PrepareResultTableForUdf", BindingFlags.NonPublic Or BindingFlags.Static, Nothing, {GetType(Object)}, Nothing)
        Assert.IsTrue(Calls(method).Any(Function(target) target.DeclaringType Is GetType(UdfSpillPreparation38)))
        Dim project = DirectCast([Delegate].CreateDelegate(GetType(BlankProjector), method), BlankProjector)
        Dim stackMethod = GetType(ParametricUDFs).GetMethod("StackWithBlankRow", BindingFlags.NonPublic Or BindingFlags.Static)
        Dim stack = DirectCast([Delegate].CreateDelegate(GetType(StackProjector), stackMethod), StackProjector)
        Dim errorCell As Object = ExcelError.ExcelErrorNA, marker As New Object()
        Dim a As Object(,) = {{Nothing, errorCell, marker}}, b As Object(,) = {{DBNull.Value}}
        Dim combined = stack(a, b), prepared = project(combined)
        Assert.AreSame(a, stack(a, Nothing))
        Assert.AreSame(errorCell, prepared(0, 1)) : Assert.AreSame(marker, prepared(0, 2))
        Assert.AreEqual(String.Empty, prepared(0, 0)) : Assert.AreEqual(String.Empty, prepared(1, 2))
        Assert.AreEqual(String.Empty, prepared(2, 0))
        Assert.IsNull(combined(1, 2)) : Assert.AreSame(DBNull.Value, combined(2, 0))
        Dim shifted = DirectCast(Array.CreateInstance(GetType(Object), {1, 1}, {1, 1}), Object(,))
        Assert.ThrowsException(Of IndexOutOfRangeException)(Sub() project(shifted))
        Assert.ThrowsException(Of IndexOutOfRangeException)(Sub() stack(shifted, b))
    End Sub
''')
save('test-source-b1.json',dict(files=[pin(ROOT/p) for p in ['BESHStat.Core.Tests/src/UdfNumericPreparation38Tests.vb','BESHStat.Excel.Common.Tests/src/UdfPreparation38Tests.vb',path]],original_queries=0))
