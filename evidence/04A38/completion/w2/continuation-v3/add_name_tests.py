from common import *
def append(path,body):
 s=(ROOT/path).read_text(encoding='utf-8-sig');write_source(path,s.rsplit('End Class',1)[0]+body+'End Class\n')
append('BESHStat.Excel.Common.Tests/src/UdfPreparation38Tests.vb','''
    <TestMethod>
    Public Sub SpillCoordinateSurfaceContainsOnlyCoordinates()
        Dim t = GetType(UdfSpillCell38)
        Assert.AreEqual(0, t.GetMethods(System.Reflection.BindingFlags.Public Or System.Reflection.BindingFlags.Static Or System.Reflection.BindingFlags.DeclaredOnly).Length)
        Assert.AreEqual(2, t.GetProperties().Length)
        Assert.IsFalse(t.GetProperty("RowOrdinal").CanWrite)
        Assert.IsFalse(t.GetProperty("ColumnOrdinal").CanWrite)
        Dim cell As New UdfSpillCell38(4, 7)
        Assert.AreEqual(4, cell.RowOrdinal) : Assert.AreEqual(7, cell.ColumnOrdinal)
        Assert.IsNotNull(GetType(UdfSpillPreparation38).GetMethod("StackWithBlankRow"))
        Assert.IsNotNull(GetType(UdfSpillPreparation38).GetMethod("NormalizeBlanks"))
    End Sub

    <DataTestMethod, DataRow(""), DataRow("sk-SK")>
    Public Sub NameOwnershipAndFallbackOrderStayDistinct(cultureName As String)
        Dim prior = CultureInfo.CurrentCulture, priorUi = CultureInfo.CurrentUICulture
        Try
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(cultureName)
            CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo(cultureName)
            Dim explicit As String() = {Nothing, " padded ", " "}
            Assert.AreSame(explicit, UdfNamePreparation38.FillExplicitNames(explicit, "Group"))
            CollectionAssert.AreEqual(New String() {"Group 1", " padded ", "Group 3"}, explicit)
            Dim detected As String() = {Nothing, " padded ", " "}
            Dim copied = UdfNamePreparation38.ResolveDetectedNames(detected, 3, "Condition")
            Assert.AreNotSame(detected, copied)
            CollectionAssert.AreEqual(New String() {"Condition 1", " padded ", "Condition 3"}, copied)
            Assert.IsNull(detected(0)) : Assert.AreEqual(" ", detected(2))
            CollectionAssert.AreEqual(New String() {" 1", " 2"}, UdfNamePreparation38.ResolveDetectedNames({"one"}, 2, Nothing))
            Dim empty As String() = {}
            Assert.AreSame(empty, UdfNamePreparation38.FillExplicitNames(empty, "unused"))
            Assert.AreNotSame(empty, UdfNamePreparation38.ResolveDetectedNames(empty, 0, "unused"))
            Assert.ThrowsException(Of NullReferenceException)(Sub() UdfNamePreparation38.FillExplicitNames(Nothing, "Group"))
            Assert.ThrowsException(Of OverflowException)(Sub() UdfNamePreparation38.ResolveDetectedNames(Nothing, -1, "Group"))
            Assert.ThrowsException(Of OverflowException)(Sub() UdfNamePreparation38.ResolveDetectedNames(Nothing, Integer.MinValue, "Group"))
        Finally
            CultureInfo.CurrentCulture = prior : CultureInfo.CurrentUICulture = priorUi
        End Try
    End Sub

    <TestMethod>
    Public Sub NameParsingDoesNotAcquireUnusedHostInputs()
        For Each count As Integer In {0, -1, Integer.MinValue}
            Assert.AreEqual(0, UdfNamePreparation38.GetVariableNames(count, Nothing, Nothing, Nothing).Length)
        Next
        CollectionAssert.AreEqual(New String() {"X1", "X2"}, UdfNamePreparation38.GetVariableNames(2, Function() True, Nothing, Nothing))
        CollectionAssert.AreEqual(New String() {"a", "b"}, UdfNamePreparation38.GetVariableNames(2, Function() False, Function() " a,, b ", Nothing))
        CollectionAssert.AreEqual(New String() {"a", "X2"}, UdfNamePreparation38.GetVariableNames(2, Function() False, Function() "a, ", Nothing))
    End Sub

    <TestMethod>
    Public Sub NameRangeTraversesBeforeMismatchAndKeepsRowPriority()
        Dim events As New System.Collections.Generic.List(Of String)()
        Dim range As New UdfVariableNameRange38(1, 3, Function(row, column)
                                                         events.Add(row.ToString() & ":" & column.ToString())
                                                         Return "name"
                                                     End Function)
        Dim result = UdfNamePreparation38.GetVariableNames(2, Function() False, Function() Nothing, Function() range)
        CollectionAssert.AreEqual(New String() {"X1", "X2"}, result)
        CollectionAssert.AreEqual(New String() {"0:0", "0:1", "0:2"}, events.ToArray())
        Dim column As New UdfVariableNameRange38(2, 1, Function(row, col) If(row = 0, "first", " "))
        CollectionAssert.AreEqual(New String() {"first", "X2"}, UdfNamePreparation38.GetVariableNames(2, Function() False, Function() Nothing, Function() column))
        Dim unreadable As New UdfVariableNameRange38(2, 2, Nothing)
        CollectionAssert.AreEqual(New String() {"X1"}, UdfNamePreparation38.GetVariableNames(1, Function() False, Function() Nothing, Function() unreadable))
        Dim throws As New UdfVariableNameRange38(1, 2, Function(row, col) ThrowNameConversion38())
        Assert.ThrowsException(Of InvalidOperationException)(Sub() UdfNamePreparation38.GetVariableNames(1, Function() False, Function() Nothing, Function() throws))
    End Sub

    Private Shared Function ThrowNameConversion38() As String
        Throw New InvalidOperationException("name conversion stopped")
    End Function
''')
p='BESHStatNG.Test/src/UDFs/UdfPreparation38AdapterTests.vb';s=(ROOT/p).read_text(encoding='utf-8-sig');needle='    Public Sub CompiledShippingAdapterCallsItsPortablePreparation'
for owner,name in [('BESHStatNG.WorksheetFunctions.ParametricUDFs','ResolveNames'),('BESHStatNG.UdfDataImport','GetVariableNames')]:s=s.replace(needle,'    <DataRow("'+owner+'", "'+name+'", "BESHStatNG.ExcelCommon.UdfNamePreparation38")>\n'+needle)
write_source(p,s)
append(p,'''
    Private Delegate Function NamesParser(argument As Object, expectedCount As Integer) As String()
    Private Delegate Function NameResolver(explicitNames As Object, detected As String(), expectedCount As Integer, prefix As String) As String()
    Private Shared Function ShippingNameParser38() As NamesParser
        Dim method = GetType(ParametricUDFs).Assembly.GetType("BESHStatNG.UdfDataImport").GetMethod("GetVariableNames", BindingFlags.NonPublic Or BindingFlags.Static)
        Return DirectCast([Delegate].CreateDelegate(GetType(NamesParser), method), NamesParser)
    End Function
    Private Shared Function ShippingNameResolver38() As NameResolver
        Dim method = GetType(ParametricUDFs).GetMethod("ResolveNames", BindingFlags.NonPublic Or BindingFlags.Static)
        Return DirectCast([Delegate].CreateDelegate(GetType(NameResolver), method), NameResolver)
    End Function

    <DataTestMethod, DataRow(""), DataRow("sk-SK")>
    Public Sub ShippingNamesKeepScalarCommaMissingAndEmptyPrecedence(cultureName As String)
        Dim prior = CultureInfo.CurrentCulture, priorUi = CultureInfo.CurrentUICulture
        Try
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(cultureName)
            CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo(cultureName)
            Dim parse = ShippingNameParser38(), resolve = ShippingNameResolver38()
            CollectionAssert.AreEqual(New String() {"a", "b"}, parse(" a,, b ", 2))
            CollectionAssert.AreEqual(New String() {"a", "X2"}, parse("a, ", 2))
            CollectionAssert.AreEqual(New String() {"a,b"}, parse("a,b", 1))
            CollectionAssert.AreEqual(New String() {"X1", "X2", "X3"}, parse("a,b", 3))
            CollectionAssert.AreEqual(New String() {"X1"}, parse(String.Empty, 1))
            For Each missing As Object In New Object() {Nothing, ExcelEmpty.Value, ExcelMissing.Value}
                CollectionAssert.AreEqual(New String() {"X1", "X2"}, parse(missing, 2))
                CollectionAssert.AreEqual(New String() {"detected"}, resolve(missing, {"detected"}, 1, "Group"))
            Next
            CollectionAssert.AreEqual(New String() {"X1"}, resolve(String.Empty, {"detected"}, 1, "Group"))
            Dim detected As String() = {Nothing, " padded "}
            Dim result = resolve(Nothing, detected, 2, "Group")
            CollectionAssert.AreEqual(New String() {"Group 1", " padded "}, result)
            Assert.AreNotSame(detected, result) : Assert.IsNull(detected(0))
            CollectionAssert.AreEqual(New String() {" 1", " 2"}, resolve(Nothing, {"wrong count"}, 2, Nothing))
        Finally
            CultureInfo.CurrentCulture = prior : CultureInfo.CurrentUICulture = priorUi
        End Try
    End Sub

    <DataTestMethod, DataRow(""), DataRow("sk-SK")>
    Public Sub ShippingNamesKeepRangeBlankErrorAndConversionPolicy(cultureName As String)
        Dim prior = CultureInfo.CurrentCulture, priorUi = CultureInfo.CurrentUICulture
        Try
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(cultureName)
            CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo(cultureName)
            Dim parse = ShippingNameParser38()
            Dim row As Object(,) = {{Nothing, "  ", DBNull.Value, ExcelError.ExcelErrorNA, " padded "}}
            CollectionAssert.AreEqual(New String() {"X1", "X2", "X3", Convert.ToString(ExcelError.ExcelErrorNA).Trim(), "padded"}, parse(row, 5))
            Assert.IsNull(row(0, 0)) : Assert.AreEqual(" padded ", row(0, 4))
            CollectionAssert.AreEqual(New String() {"first", "X2"}, parse(New Object(,) {{" first "}, {ExcelEmpty.Value}}, 2))
            CollectionAssert.AreEqual(New String() {"X1"}, parse(New Object(,) {{1, 2}, {3, 4}}, 1))
            Dim shifted = DirectCast(Array.CreateInstance(GetType(Object), {1, 2}, {1, 1}), Object(,))
            Assert.ThrowsException(Of IndexOutOfRangeException)(Sub() parse(shifted, 2))
            Dim shiftedNegative = DirectCast(Array.CreateInstance(GetType(Object), {1, 2}, {-1, -1}), Object(,))
            Assert.ThrowsException(Of IndexOutOfRangeException)(Sub() parse(shiftedNegative, 2))
        Finally
            CultureInfo.CurrentCulture = prior : CultureInfo.CurrentUICulture = priorUi
        End Try
    End Sub

    <TestMethod>
    Public Sub ShippingNamesConvertLazilyInSourceOrderEvenBeforeMismatch()
        Dim parse = ShippingNameParser38(), events As New List(Of String)()
        Dim input As Object(,) = {{Nothing, "before", Nothing}}
        input(0, 0) = New ConversionProbe(Function()
                                            events.Add("first") : input(0, 1) = "changed"
                                            Return " first "
                                        End Function)
        input(0, 2) = New ConversionProbe(Function()
                                            events.Add("third") : Return "third"
                                        End Function)
        CollectionAssert.AreEqual(New String() {"first", "changed", "third"}, parse(input, 3))
        CollectionAssert.AreEqual(New String() {"first", "third"}, events.ToArray())
        input(0, 2) = New ConversionProbe(Function()
                                            Throw New InvalidOperationException("third conversion stopped")
                                            Return "unreachable"
                                        End Function)
        Dim ex = Assert.ThrowsException(Of InvalidOperationException)(Sub() parse(input, 2))
        Assert.AreEqual("third conversion stopped", ex.Message)
        input(0, 0) = New ConversionProbe(Function() CType(Nothing, String))
        Assert.ThrowsException(Of NullReferenceException)(Sub() parse(input, 3))
    End Sub

    <TestMethod>
    Public Sub ShippingNameCountGuardsPreserveExplicitVersusMissingBehavior()
        Dim parse = ShippingNameParser38(), resolve = ShippingNameResolver38()
        Dim unreadable As New ConversionProbe(Function() ThrowUnexpectedNameRead38())
        For Each count As Integer In {0, -1, Integer.MinValue}
            Assert.AreEqual(0, parse(unreadable, count).Length)
            Assert.AreEqual(0, resolve(unreadable, Nothing, count, "Group").Length)
        Next
        Assert.AreEqual(0, resolve(Nothing, Nothing, 0, "Group").Length)
        Assert.ThrowsException(Of OverflowException)(Sub() resolve(Nothing, Nothing, -1, "Group"))
        Assert.ThrowsException(Of OverflowException)(Sub() resolve(Nothing, Nothing, Integer.MinValue, "Group"))
    End Sub

    Private Shared Function ThrowUnexpectedNameRead38() As String
        Throw New InvalidOperationException("must not read for nonpositive expected count")
    End Function
''')
save('test-source-names.json',dict(files=[pin(ROOT/p) for p in ['BESHStat.Excel.Common.Tests/src/UdfPreparation38Tests.vb',p]],original_queries=0))
