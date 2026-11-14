Option Explicit On
Option Strict On

Imports System
Imports System.Collections.Concurrent
Imports System.Collections.Generic
Imports System.Linq
Imports System.Reflection
Imports System.Reflection.Emit
Imports System.Globalization
Imports BESHStatNG.ExcelCommon
Imports BESHStatNG.WorksheetFunctions
Imports ExcelDna.Integration
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass>
Public Class UdfPreparation38AdapterTests
    Private Shared ReadOnly Opcodes As Dictionary(Of Short, OpCode) = GetType(OpCodes).GetFields(BindingFlags.Public Or BindingFlags.Static).
        Where(Function(field) field.FieldType Is GetType(OpCode)).Select(Function(field) DirectCast(field.GetValue(Nothing), OpCode)).ToDictionary(Function(code) code.Value)

    <DataTestMethod>
    <DataRow("BESHStatNG.WorksheetFunctions.ExcelArgNumeric", "GetOptionalBool", "BESHStatNG.ExcelCommon.UdfScalarChoice38")>
    <DataRow("BESHStatNG.WorksheetFunctions.ExcelArgNumeric", "TryGetBinary01", "BESHStatNG.ExcelCommon.UdfScalarChoice38")>
    <DataRow("BESHStatNG.UdfDataImport", "HasNumericColumnHeader", "BESHStatNG.ExcelCommon.UdfRangePreparation38")>
    <DataRow("BESHStatNG.UdfDataImport", "TryExtractNumericGroupsFromMatrix", "BESHStatNG.ExcelCommon.UdfNumericImportPreparation38")>
    <DataRow("BESHStatNG.UdfLinearAlgebra", "TryInvertMatrix", "BESHStatNG.WorksheetCompatibility.UdfNumericPreparation38")>
    <DataRow("BESHStatNG.WorksheetFunctions.AssumptionsUDFs", "BuildTwoValueTable", "BESHStatNG.ExcelCommon.UdfSpillPreparation38")>
    <DataRow("BESHStatNG.WorksheetFunctions.PropensityScoreUDFs", "ApplyOption", "BESHStatNG.ExcelCommon.UdfPsmPreparation38")>
    <DataRow("BESHStatNG.WorksheetFunctions.GEEUDFs", "AverageGeeFittedDesignRowForLsmProfile", "BESHStatNG.WorksheetCompatibility.UdfProfilePreparation38")>
    <DataRow("BESHStatNG.WorksheetFunctions.MixedModelUDFs", "AverageFittedDesignRowForLsmProfile", "BESHStatNG.WorksheetCompatibility.UdfProfilePreparation38")>
    <DataRow("BESHStatNG.WorksheetFunctions.RegressionFormulaUDFs", "BuildValidationFailureMessage", "BESHStatNG.ExcelCommon.RegressionFormulaUDFsMetadata38")>
    <DataRow("BESHStatNG.UdfOutputTables", "PrepareWrappedResultTableForUdf", "BESHStatNG.ExcelCommon.UdfSpillLayout38")>
    <DataRow("BESHStatNG.UdfDataImport", "TryGetGroupedCompleteMatrix", "BESHStatNG.ExcelCommon.UdfNumericImportPreparation38")>
    <DataRow("BESHStatNG.UdfDataImport", "TryGetNestedThreeColumnData", "BESHStatNG.ExcelCommon.UdfNumericImportPreparation38")>
    <DataRow("BESHStatNG.WorksheetFunctions.GEEUDFs", "ApplyGeeLsmEstimateAtProfile", "BESHStatNG.ExcelCommon.UdfModelProfilePreparation38")>
    <DataRow("BESHStatNG.WorksheetFunctions.MixedModelUDFs", "ApplyMmrmLsmEstimateAtProfile", "BESHStatNG.ExcelCommon.UdfModelProfilePreparation38")>
    <DataRow("BESHStatNG.WorksheetFunctions.PlotDataUDFs", "BuildCalibrationPointsTable", "BESHStatNG.ExcelCommon.PlotDataUDFsReports38")>
    <DataRow("BESHStatNG.WorksheetFunctions.PlotDataUDFs", "BuildRocPointsTable", "BESHStatNG.ExcelCommon.PlotDataUDFsReports38")>
    <DataRow("BESHStatNG.WorksheetFunctions.PlotDataUDFs", "BuildRocStatsTable", "BESHStatNG.ExcelCommon.PlotDataUDFsReports38")>
    <DataRow("BESHStatNG.WorksheetFunctions.AssumptionsUDFs", "TryParseLeveneCenter", "BESHStatNG.ExcelCommon.AssumptionsUDFsOptions38")>
    <DataRow("BESHStatNG.WorksheetFunctions.AssumptionsUDFs", "TryParseSymmetryMethod", "BESHStatNG.ExcelCommon.AssumptionsUDFsOptions38")>
    <DataRow("BESHStatNG.WorksheetFunctions.DistributionUDFs", "PRTRNG", "BESHStatNG.WorksheetCompatibility.UdfNumericPreparation38")>
    <DataRow("BESHStatNG.WorksheetFunctions.DistributionUDFs", "PRTRNG_TAIL", "BESHStatNG.WorksheetCompatibility.UdfNumericPreparation38")>
    <DataRow("BESHStatNG.WorksheetFunctions.DistributionUDFs", "F_PDF", "BESHStatNG.WorksheetCompatibility.UdfNumericPreparation38")>
    <DataRow("BESHStatNG.WorksheetFunctions.ParametricUDFs", "StackWithBlankRow", "BESHStatNG.ExcelCommon.UdfSpillPreparation38")>
    <DataRow("BESHStatNG.WorksheetFunctions.ParametricUDFs", "ResolveNames", "BESHStatNG.ExcelCommon.UdfNamePreparation38")>
    <DataRow("BESHStatNG.UdfDataImport", "GetVariableNames", "BESHStatNG.ExcelCommon.UdfNamePreparation38")>
    Public Sub CompiledShippingAdapterCallsItsPortablePreparation(owner As String, member As String, portableOwner As String)
        Dim assembly As Assembly = GetType(UdfLinearAlgebra).Assembly
        Dim type As Type = assembly.GetType(owner, True)
        Dim method As MethodInfo = type.GetMethod(member, BindingFlags.Public Or BindingFlags.NonPublic Or BindingFlags.Static)
        Assert.IsNotNull(method)
        Assert.IsTrue(Calls(method).Any(Function(target) target.DeclaringType.FullName = portableOwner), owner & "." & member)
    End Sub

    Private Shared Iterator Function Calls(method As MethodBase) As IEnumerable(Of MethodBase)
        Dim bytes As Byte() = method.GetMethodBody().GetILAsByteArray()
        Dim i As Integer = 0
        While i < bytes.Length
            Dim key As Short = CShort(bytes(i))
            i += 1
            If key = &HFE Then
                key = CShort(-512 Or CInt(bytes(i)))
                i += 1
            End If
            Dim code As OpCode = Opcodes(key)
            Dim size As Integer
            Select Case code.OperandType
                Case OperandType.InlineNone : size = 0
                Case OperandType.ShortInlineBrTarget, OperandType.ShortInlineI, OperandType.ShortInlineVar : size = 1
                Case OperandType.InlineVar : size = 2
                Case OperandType.InlineI8, OperandType.InlineR : size = 8
                Case OperandType.InlineSwitch : size = 4 + 4 * BitConverter.ToInt32(bytes, i)
                Case Else : size = 4
            End Select
            If code.OperandType = OperandType.InlineMethod Then
                Yield method.Module.ResolveMethod(BitConverter.ToInt32(bytes, i), method.DeclaringType.GetGenericArguments(), If(method.IsGenericMethod, method.GetGenericArguments(), Nothing))
            End If
            i += size
        End While
    End Function

    <TestMethod>
    Public Sub ShippingSentinelAdapterPreservesNumericAndBooleanDistinctions()
        Assert.IsTrue(ExcelArgNumeric.GetOptionalBool(ExcelMissing.Value, True))
        Assert.IsFalse(ExcelArgNumeric.GetOptionalBool(ExcelEmpty.Value, False))
        Assert.IsFalse(ExcelArgNumeric.TryGetDouble(True).HasValue)
        Assert.IsFalse(ExcelArgNumeric.TryGetDouble("1").HasValue)
        Dim result As Integer
        Assert.IsTrue(ExcelArgNumeric.TryGetBinary01(0.75, result))
        Assert.AreEqual(0, result)
        Assert.IsTrue(ExcelArgNumeric.TryGetBinary01(True, result))
        Assert.AreEqual(1, result)
    End Sub

    Private Delegate Function LeveneParser(argument As Object, ByRef useMedian As Boolean, ByRef title As String) As Boolean
    Private Delegate Function SymmetryParser(argument As Object, ByRef method As String, ByRef title As String) As Boolean

    Private NotInheritable Class ParserState
        Public UseMedian As Boolean = True
        Public Title As String = "before title"
        Public Method As String = "before method"
    End Class

    Private NotInheritable Class ConversionProbe
        Private ReadOnly _read As Func(Of String)
        Public Sub New(read As Func(Of String))
            _read = read
        End Sub
        Public Overrides Function ToString() As String
            Return _read()
        End Function
    End Class

    Private Shared Function ShippingParser(Of T As Class)(name As String) As T
        Dim owner = GetType(UdfLinearAlgebra).Assembly.GetType("BESHStatNG.WorksheetFunctions.AssumptionsUDFs", True)
        Dim method = owner.GetMethod(name, BindingFlags.NonPublic Or BindingFlags.Static)
        Return DirectCast(DirectCast([Delegate].CreateDelegate(GetType(T), method), Object), T)
    End Function

    <DataTestMethod, DataRow(""), DataRow("sk-SK")>
    Public Sub ShippingLeveneOrdinaryAliasesAndSentinelsKeepOriginalOutputs(cultureName As String)
        Dim prior = CultureInfo.CurrentCulture, priorUi = CultureInfo.CurrentUICulture
        Try
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(cultureName)
            CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo(cultureName)
            Dim parse = ShippingParser(Of LeveneParser)("TryParseLeveneCenter")
            For Each value As Object In New Object() {Nothing, " ", "MEAN", "LEVENE", "CLASSICAL", "mean", ExcelMissing.Value, ExcelEmpty.Value}
                Dim state As New ParserState
                Assert.IsTrue(parse(value, state.UseMedian, state.Title), Convert.ToString(value))
                Assert.IsFalse(state.UseMedian)
                Assert.AreEqual("Levene Test", state.Title)
            Next
            For Each value As String In {"MEDIAN", "BF", "Brown-Forsythe", "BROWNFORSYTHE"}
                Dim state As New ParserState
                Assert.IsTrue(parse(value, state.UseMedian, state.Title), value)
                Assert.IsTrue(state.UseMedian)
                Assert.AreEqual("Brown-Forsythe Test", state.Title)
            Next
            For Each value As Object In New Object() {"unknown", ExcelError.ExcelErrorNA}
                Dim state As New ParserState
                Assert.IsFalse(parse(value, state.UseMedian, state.Title))
                Assert.IsFalse(state.UseMedian)
                Assert.AreEqual("Levene Test", state.Title)
            Next
        Finally
            CultureInfo.CurrentCulture = prior : CultureInfo.CurrentUICulture = priorUi
        End Try
    End Sub

    <DataTestMethod, DataRow(""), DataRow("sk-SK")>
    Public Sub ShippingLeveneResetsBeforeThrowingConversionAndKeepsItsMutations(cultureName As String)
        CheckLeveneConversion(cultureName, True, False)
    End Sub

    <DataTestMethod, DataRow(""), DataRow("sk-SK")>
    Public Sub ShippingLeveneInvalidConversionKeepsCallbackByRefMutations(cultureName As String)
        CheckLeveneConversion(cultureName, False, False)
    End Sub

    <DataTestMethod, DataRow(""), DataRow("sk-SK")>
    Public Sub ShippingLeveneAcceptedConversionOverwritesCallbackValuesInOriginalOrder(cultureName As String)
        CheckLeveneConversion(cultureName, False, True)
    End Sub

    Private Shared Sub CheckLeveneConversion(cultureName As String, throws As Boolean, accepted As Boolean)
        Dim prior = CultureInfo.CurrentCulture, priorUi = CultureInfo.CurrentUICulture
        Try
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(cultureName)
            CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo(cultureName)
            Dim parse = ShippingParser(Of LeveneParser)("TryParseLeveneCenter")
            Dim state As New ParserState
            Dim seenMedian As Boolean = True, seenTitle As String = Nothing, calls As Integer = 0
            Dim input As New ConversionProbe(Function()
                                                 calls += 1
                                                 seenMedian = state.UseMedian : seenTitle = state.Title
                                                 state.UseMedian = True : state.Title = "callback title"
                                                 If throws Then Throw New InvalidOperationException("conversion stopped")
                                                 Return If(accepted, "MEDIAN", "unknown")
                                             End Function)
            If throws Then
                Dim ex = Assert.ThrowsException(Of InvalidOperationException)(Sub() parse(input, state.UseMedian, state.Title))
                Assert.AreEqual("conversion stopped", ex.Message)
            Else
                Assert.AreEqual(accepted, parse(input, state.UseMedian, state.Title))
            End If
            Assert.AreEqual(1, calls)
            Assert.IsFalse(seenMedian, "useMedian must be reset before ToString")
            Assert.AreEqual("Levene Test", seenTitle, "title must be reset before ToString")
            Assert.IsTrue(state.UseMedian)
            Assert.AreEqual(If(accepted, "Brown-Forsythe Test", "callback title"), state.Title)
        Finally
            CultureInfo.CurrentCulture = prior : CultureInfo.CurrentUICulture = priorUi
        End Try
    End Sub

    <DataTestMethod, DataRow(""), DataRow("sk-SK")>
    Public Sub ShippingSymmetryAliasesAndInvalidInputKeepOriginalByRefRules(cultureName As String)
        Dim prior = CultureInfo.CurrentCulture, priorUi = CultureInfo.CurrentUICulture
        Try
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(cultureName)
            CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo(cultureName)
            Dim parse = ShippingParser(Of SymmetryParser)("TryParseSymmetryMethod")
            For Each value As Object In New Object() {Nothing, " ", "MGG", "MIAO", ExcelMissing.Value}
                Dim state As New ParserState
                Assert.IsTrue(parse(value, state.Method, state.Title))
                Assert.AreEqual("Miao-Gel-Gastwirth", state.Method)
                Assert.AreEqual("Miao-Gel-Gastwirth Symmetry Test", state.Title)
            Next
            For Each value As String In {"CM", "CABILIO", "MASARO"}
                Dim state As New ParserState
                Assert.IsTrue(parse(value, state.Method, state.Title))
                Assert.AreEqual("Cabilio-Masaro", state.Method)
                Assert.AreEqual("Cabilio-Masaro Symmetry Test", state.Title)
            Next
            For Each value As Object In New Object() {"unknown", "MIAO-GEL-GASTWIRTH", "CABILIO-MASARO", ExcelError.ExcelErrorNA}
                Dim state As New ParserState
                Assert.IsFalse(parse(value, state.Method, state.Title))
                Assert.AreEqual("before method", state.Method) : Assert.AreEqual("before title", state.Title)
            Next
        Finally
            CultureInfo.CurrentCulture = prior : CultureInfo.CurrentUICulture = priorUi
        End Try
    End Sub

    <DataTestMethod, DataRow(""), DataRow("sk-SK")>
    Public Sub ShippingSymmetryThrowingConversionDoesNotInitializeOutputs(cultureName As String)
        CheckSymmetryConversion(cultureName, True)
    End Sub

    <DataTestMethod, DataRow(""), DataRow("sk-SK")>
    Public Sub ShippingSymmetryInvalidConversionKeepsItsMutatedOutputs(cultureName As String)
        CheckSymmetryConversion(cultureName, False)
    End Sub

    Private Shared Sub CheckSymmetryConversion(cultureName As String, throws As Boolean)
        Dim prior = CultureInfo.CurrentCulture, priorUi = CultureInfo.CurrentUICulture
        Try
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(cultureName)
            CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo(cultureName)
            Dim parse = ShippingParser(Of SymmetryParser)("TryParseSymmetryMethod")
            Dim state As New ParserState
            Dim seenMethod As String = Nothing, seenTitle As String = Nothing, calls As Integer = 0
            Dim input As New ConversionProbe(Function()
                                                 calls += 1 : seenMethod = state.Method : seenTitle = state.Title
                                                 state.Method = "callback method" : state.Title = "callback title"
                                                 If throws Then Throw New InvalidOperationException("conversion stopped")
                                                 Return "unknown"
                                             End Function)
            If throws Then
                Dim ex = Assert.ThrowsException(Of InvalidOperationException)(Sub() parse(input, state.Method, state.Title))
                Assert.AreEqual("conversion stopped", ex.Message)
            Else
                Assert.IsFalse(parse(input, state.Method, state.Title))
            End If
            Assert.AreEqual(1, calls)
            Assert.AreEqual("before method", seenMethod) : Assert.AreEqual("before title", seenTitle)
            Assert.AreEqual("callback method", state.Method) : Assert.AreEqual("callback title", state.Title)
        Finally
            CultureInfo.CurrentCulture = prior : CultureInfo.CurrentUICulture = priorUi
        End Try
    End Sub

    <DataTestMethod, DataRow(""), DataRow("sk-SK")>
    Public Sub ShippingGroupedAndNestedImportsUseNativePoliciesWithPortablePreparation(cultureName As String)
        Dim previous = CultureInfo.CurrentCulture
        Try
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(cultureName)
            Dim owner = GetType(UdfLinearAlgebra).Assembly.GetType("BESHStatNG.UdfDataImport", True)
            Dim grouped = owner.GetMethod("TryGetGroupedCompleteMatrix", BindingFlags.NonPublic Or BindingFlags.Static)
            Dim args As Object() = {New Object(,) {{"X"}, {1.0R}, {Double.NaN}, {2.0R}}, New Object(,) {{"G"}, {" a "}, {"bad"}, {"A"}}, Nothing, Nothing}
            Assert.IsTrue(DirectCast(grouped.Invoke(Nothing, args), Boolean))
            Dim matrices = DirectCast(args(2), List(Of Double(,)))
            CollectionAssert.AreEqual(New String() {"a"}, DirectCast(args(3), List(Of String)).ToArray())
            Assert.AreEqual(2, matrices(0).GetLength(0))
            Dim nested = owner.GetMethod("TryGetNestedThreeColumnData", BindingFlags.NonPublic Or BindingFlags.Static)
            args = {New Object(,) {{"G", "S", "Y"}, {"A", "a", 2.0R}, {"B", "b", ExcelError.ExcelErrorNA}, {"C", "c", Double.NaN}}, Nothing, Nothing}
            Assert.IsTrue(DirectCast(nested.Invoke(Nothing, args), Boolean))
            Dim data = DirectCast(args(1), Object(,))
            Assert.AreEqual(1, data.GetLength(0))
            Assert.AreEqual(2.0R, DirectCast(data(0, 2), Double))
        Finally
            CultureInfo.CurrentCulture = previous
        End Try
    End Sub

    <DataTestMethod, DataRow(""), DataRow("sk-SK")>
    Public Sub ShippingCalibrationReportKeepsRawNonfiniteAndLegacyNumericErrorCoercion(cultureName As String)
        Dim previous = CultureInfo.CurrentCulture
        Try
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(cultureName)
            Dim owner = GetType(UdfLinearAlgebra).Assembly.GetType("BESHStatNG.WorksheetFunctions.PlotDataUDFs", True)
            Dim method = owner.GetMethod("BuildCalibrationPointsTable", BindingFlags.NonPublic Or BindingFlags.Static)
            Dim rows As New List(Of regression.CalibrationBinSummary) From {New regression.CalibrationBinSummary With {.BinIndex = 1, .N = 2.0R, .LowerCI = Double.NaN, .UpperCI = Double.PositiveInfinity}}
            Dim cells = DirectCast(method.Invoke(Nothing, New Object() {rows}), Object(,))
            Assert.IsTrue(Double.IsNaN(DirectCast(cells(1, 4), Double)))
            Assert.IsTrue(Double.IsPositiveInfinity(DirectCast(cells(1, 5), Double)))
            Assert.AreEqual(CDbl(ExcelError.ExcelErrorNA), DirectCast(cells(1, 6), Double))
            Assert.IsTrue(Double.IsPositiveInfinity(DirectCast(cells(1, 7), Double)))
        Finally
            CultureInfo.CurrentCulture = previous
        End Try
    End Sub

    <TestMethod>
    Public Sub ShippingReportMaterializerRetainsNativeSourceCellsAndErrorCategory()
        Dim native As ExcelError = ExcelError.ExcelErrorNA
        Dim projection As New UdfReportProjection38(New ResultTableOutputModel(New Object(,) {{New UdfSourceCellReference38(0), UdfReportProjection38.SafeNumber(Double.NaN)}}))
        Dim result As Object(,) = DirectCast(UdfOutputTables.MaterializePortableReport38(projection, {native}), Object(,))
        Assert.AreEqual(native, DirectCast(result(0, 0), ExcelError))
        Assert.AreEqual(ExcelError.ExcelErrorNum, DirectCast(result(0, 1), ExcelError))
        Assert.AreEqual(1, result.GetLength(0))
        Assert.AreEqual(2, result.GetLength(1))
    End Sub

    <TestMethod>
    Public Sub ExistingCacheHelperRetainsDictionaryComparerAndMissingLookupBehavior()
        Dim cache As New ConcurrentDictionary(Of String, String)(StringComparer.OrdinalIgnoreCase)
        cache.TryAdd("pca:sample", "model")
        Dim result As String = "previous"
        Assert.IsTrue(UdfCacheHelpers.TryGetCachedHandle(" PCA:SAMPLE ", cache, result))
        Assert.AreEqual("model", result)
        Assert.IsFalse(UdfCacheHelpers.TryGetCachedHandle(ExcelMissing.Value, cache, result))
        Assert.IsNull(result)
        Assert.AreEqual(1, cache.Count)
    End Sub

    <DataTestMethod, DataRow(""), DataRow("sk-SK")>
    Public Sub ShippingMmrmDescriptionDelegatesToPortablePolicyWithNativeDto(cultureName As String)
        Dim original = CultureInfo.CurrentCulture
        Dim originalUi = CultureInfo.CurrentUICulture
        Try
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(cultureName)
            CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo(cultureName)
            Dim assembly = GetType(UdfLinearAlgebra).Assembly
            Dim owner = assembly.GetType("BESHStatNG.WorksheetFunctions.MixedModelUDFs", True)
            Dim method = owner.GetMethod("FormatMmrmAtProfileDescription", BindingFlags.NonPublic Or BindingFlags.Static)
            Dim nativeType = assembly.GetType("BESHStatNG.WorksheetFunctions.MixedModelUDFs+MmrmLsmEstimateAtProfile", True)
            Dim profile = Activator.CreateInstance(nativeType)
            nativeType.GetProperty("VisitSpecified").SetValue(profile, True)
            nativeType.GetProperty("VisitValue").SetValue(profile, 1.23456789R)
            Assert.AreEqual("visit=1.23457", method.Invoke(Nothing, New Object() {profile}))
            Assert.IsTrue(Calls(method).Any(Function(target) target.DeclaringType.FullName = "BESHStatNG.ExcelCommon.UdfModelProfilePreparation38" AndAlso target.Name = "FormatMmrmAtProfileDescription"))
        Finally
            CultureInfo.CurrentCulture = original
            CultureInfo.CurrentUICulture = originalUi
        End Try
    End Sub

    <TestMethod>
    Public Sub RegistrationMetadataHasExactly268DistinctWindowsExports()
        Dim names As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)
        For Each type As Type In GetType(UdfLinearAlgebra).Assembly.GetTypes()
            For Each method As MethodInfo In type.GetMethods(BindingFlags.Public Or BindingFlags.Static Or BindingFlags.DeclaredOnly)
                Dim attribute As ExcelFunctionAttribute = method.GetCustomAttribute(Of ExcelFunctionAttribute)()
                If attribute Is Nothing Then Continue For
                Assert.IsTrue(names.Add(attribute.Name), attribute.Name)
                Assert.IsTrue(attribute.Name.StartsWith("BESH.", StringComparison.Ordinal))
            Next
        Next
        Assert.AreEqual(268, names.Count)
    End Sub

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
            CollectionAssert.AreEqual(New String() {"X1"}, parse("a,b", 1))
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
            CollectionAssert.AreEqual(New String() {"X1", "X2", "X3", Convert.ToString(CObj(ExcelError.ExcelErrorNA)).Trim(), "padded"}, parse(row, 5))
            Assert.IsNull(row(0, 0)) : Assert.AreEqual(" padded ", row(0, 4))
            CollectionAssert.AreEqual(New String() {"first", "X2"}, parse(New Object(,) {{" first "}, {ExcelEmpty.Value}}, 2))
            CollectionAssert.AreEqual(New String() {"X1"}, parse(New Object(,) {{1, 2}, {3, 4}}, 1))
            Dim shifted = DirectCast(Array.CreateInstance(GetType(Object), {1, 2}, {1, 1}), Object(,))
            shifted(1, 1) = " left " : shifted(1, 2) = "right"
            CollectionAssert.AreEqual(New String() {"left", "right"}, parse(shifted, 2))
            Assert.AreEqual(1, shifted.GetLowerBound(0)) : Assert.AreEqual(" left ", shifted(1, 1))
            Dim shiftedNegative = DirectCast(Array.CreateInstance(GetType(Object), {1, 2}, {-1, -1}), Object(,))
            shiftedNegative(-1, -1) = "n1" : shiftedNegative(-1, 0) = "n2"
            CollectionAssert.AreEqual(New String() {"n1", "n2"}, parse(shiftedNegative, 2))
            Dim stamp As New DateTime(2020, 1, 2, 3, 4, 5)
            CollectionAssert.AreEqual(New String() {If(cultureName = "sk-SK", "1,25", "1.25"), Convert.ToString(CObj(stamp)).Trim()}, parse(New Object(,) {{1.25, stamp}}, 2))
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
        CollectionAssert.AreEqual(New String() {"first", "before", "third"}, parse(input, 3))
        Assert.AreEqual("changed", input(0, 1), "Conversion mutates original outer array, while acquired snapshot is detached")
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

    <TestMethod>
    Public Sub InversionHostCatchRetainsFrameworkErrAndByRefContract()
        Dim regular As Double(,) = {{4.0, 1.0}, {1.0, 3.0}}
        For Each a As Double(,) In New List(Of Double(,)) From {Nothing, New Double(-1, -1) {}, New Double(1, 2) {}, regular}
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
        For Each a As Double(,) In New List(Of Double(,)) From {New Double(,) {{1.0, 2.0}, {2.0, 4.0}}, New Double(,) {{1.0, 0.0}, {0.0, 0.00000000000000000001}}, DirectCast(Array.CreateInstance(GetType(Double), {2, 2}, {1, 1}), Double(,))}
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

    <DataTestMethod>
    <DataRow("")>
    <DataRow("sk-SK")>
    Public Sub NativeCellConversion38KeepsSentinelsAndRealCatch(culture As String)
        Dim old As CultureInfo = CultureInfo.CurrentCulture
        Try
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(culture)
            For Each value As Object In New Object() {Nothing, ExcelEmpty.Value, ExcelMissing.Value}
                Assert.AreEqual("", ExcelArgReaders.CellToTrimmedText(value))
                Assert.IsFalse(ExcelArgNumeric.TryGetDouble(value).HasValue)
            Next
            For Each value As Object In New Object() {ExcelError.ExcelErrorNA, True, "1.5", "", DBNull.Value, New Object(,) {{1.0R}}}
                Assert.IsFalse(ExcelArgNumeric.TryGetDouble(value).HasValue)
            Next
            Assert.AreEqual("ExcelErrorNA", ExcelArgReaders.CellToTrimmedText(ExcelError.ExcelErrorNA))
            Assert.AreEqual("1.5", ExcelArgReaders.CellToTrimmedText(1.5R))
            Assert.AreEqual(1.5R, ExcelArgNumeric.TryGetDouble(1.5D).Value)
            Assert.ThrowsException(Of OverflowException)(Sub() ExcelArgReaders.CellToTrimmedText(Long.MaxValue))
            Microsoft.VisualBasic.Err().Raise(5)
        Catch ex As Exception
            Assert.AreEqual(5, Microsoft.VisualBasic.Err().Number)
            Assert.IsFalse(ExcelArgNumeric.TryGetDouble(DBNull.Value).HasValue)
            Assert.AreEqual(0, Microsoft.VisualBasic.Err().Number)
        Finally
            Microsoft.VisualBasic.Err().Clear()
            CultureInfo.CurrentCulture = old
        End Try
    End Sub

    <DataTestMethod>
    <DataRow("")>
    <DataRow("sk-SK")>
    Public Sub GroupedImport38RetainsBoundsNamesAndCallerArrays(culture As String)
        Dim old As CultureInfo = CultureInfo.CurrentCulture
        Try
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(culture)
            Dim source As Object(,) = {{" A ", "empty", " C "}, {1.0R, Nothing, 3.0R}, {2.0R, ExcelError.ExcelErrorNA, "4"}}
            Dim groups()() As Double = Nothing
            Dim names() As String = Nothing
            Assert.IsTrue(UdfDataImport.TryGetGroupedNumericColumns(source, groups, names))
            ' An empty source column prevents automatic header detection in the original policy.
            CollectionAssert.AreEqual(New String() {"Group 1", "Group 2"}, names)
            Assert.IsTrue(UdfDataImport.TryExtractNumericGroupsFromMatrix(source, 1, True, "", groups, names))
            CollectionAssert.AreEqual(New String() {"A", "C"}, names)
            CollectionAssert.AreEqual(New Double() {1.0R, 2.0R}, groups(0))
            CollectionAssert.AreEqual(New Double() {3.0R}, groups(1))
            Assert.AreEqual(" A ", source(0, 0))
            Assert.AreEqual("4", source(2, 2))
            groups(0)(0) = 99
            names(0) = "changed"
            Assert.AreEqual(1.0R, source(1, 0))
            Assert.AreEqual(" A ", source(0, 0))
            Dim bounded As Object(,) = DirectCast(Array.CreateInstance(GetType(Object), {3, 3}, {1, -1}), Object(,))
            For row As Integer = 0 To 2
                For column As Integer = 0 To 2
                    bounded(row + 1, column - 1) = source(row, column)
                Next
            Next
            Assert.IsTrue(UdfDataImport.TryGetGroupedNumericColumns(bounded, groups, names))
            Assert.AreEqual(1.0R, groups(0)(0))
            Assert.AreEqual(1, bounded.GetLowerBound(0))
            Assert.AreEqual(-1, bounded.GetLowerBound(1))
            Assert.ThrowsException(Of IndexOutOfRangeException)(Sub() UdfDataImport.TryExtractNumericGroupsFromMatrix(bounded, 1, True, "", groups, names))
            Assert.IsNull(groups)
            Assert.IsNull(names)
            Assert.IsFalse(UdfDataImport.TryGetGroupedNumericColumns(1.0R, groups, names))
            Assert.IsNull(groups)
            Assert.IsNull(names)
        Finally
            CultureInfo.CurrentCulture = old
        End Try
    End Sub

    <DataTestMethod>
    <DataRow("")>
    <DataRow("sk-SK")>
    Public Sub NativeAlpha38RetainsDefaultParsePrecedenceAndByRef(culture As String)
        Dim old As CultureInfo = CultureInfo.CurrentCulture
        Try
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(culture)
            Dim alpha As Double = 99
            Assert.IsTrue(UdfDataImport.TryParseAlpha(ExcelMissing.Value, alpha))
            Assert.AreEqual(0.05R, alpha)
            Assert.IsTrue(UdfDataImport.TryParseAlpha(" 0.1 ", alpha))
            Assert.AreEqual(0.1R, alpha)
            Assert.IsFalse(UdfDataImport.TryParseAlpha("0,1", alpha))
            Assert.AreEqual(1.0R, alpha)
            Assert.IsFalse(UdfDataImport.TryParseAlpha("bad", alpha))
            Assert.AreEqual(0.0R, alpha)
            Assert.IsFalse(UdfDataImport.TryParseAlpha(DBNull.Value, alpha))
            Assert.AreEqual(0.05R, alpha)
            Assert.IsFalse(UdfDataImport.TryParseAlpha(True, alpha))
            Assert.AreEqual(1.0R, alpha)
            Assert.IsFalse(UdfDataImport.TryParseAlpha(Double.NaN, alpha))
            Assert.IsTrue(Double.IsNaN(alpha))
        Finally
            CultureInfo.CurrentCulture = old
        End Try
    End Sub

    Private NotInheritable Class AliasBox38
        Public Value As Integer = 42
    End Class

    Private NotInheritable Class AliasNumber38
        Implements IConvertible
        Private ReadOnly output As AliasBox38
        Private ReadOnly number As Double
        Public Sub New(target As AliasBox38, input As Double)
            output = target
            number = input
        End Sub
        Public Function ToDouble(provider As IFormatProvider) As Double Implements IConvertible.ToDouble
            output.Value = 17
            Return number
        End Function
        Public Function GetTypeCode() As TypeCode Implements IConvertible.GetTypeCode
            Return TypeCode.Double
        End Function
        Public Function ToBoolean(provider As IFormatProvider) As Boolean Implements IConvertible.ToBoolean
            Throw New NotSupportedException()
        End Function
        Public Function ToByte(provider As IFormatProvider) As Byte Implements IConvertible.ToByte
            Throw New NotSupportedException()
        End Function
        Public Function ToChar(provider As IFormatProvider) As Char Implements IConvertible.ToChar
            Throw New NotSupportedException()
        End Function
        Public Function ToDateTime(provider As IFormatProvider) As DateTime Implements IConvertible.ToDateTime
            Throw New NotSupportedException()
        End Function
        Public Function ToDecimal(provider As IFormatProvider) As Decimal Implements IConvertible.ToDecimal
            Throw New NotSupportedException()
        End Function
        Public Function ToInt16(provider As IFormatProvider) As Short Implements IConvertible.ToInt16
            Throw New NotSupportedException()
        End Function
        Public Function ToInt32(provider As IFormatProvider) As Integer Implements IConvertible.ToInt32
            Throw New NotSupportedException()
        End Function
        Public Function ToInt64(provider As IFormatProvider) As Long Implements IConvertible.ToInt64
            Throw New NotSupportedException()
        End Function
        Public Function ToSByte(provider As IFormatProvider) As SByte Implements IConvertible.ToSByte
            Throw New NotSupportedException()
        End Function
        Public Function ToSingle(provider As IFormatProvider) As Single Implements IConvertible.ToSingle
            Throw New NotSupportedException()
        End Function
        Public Function ToString(provider As IFormatProvider) As String Implements IConvertible.ToString
            Throw New NotSupportedException()
        End Function
        Public Function ToUInt16(provider As IFormatProvider) As UShort Implements IConvertible.ToUInt16
            Throw New NotSupportedException()
        End Function
        Public Function ToUInt32(provider As IFormatProvider) As UInteger Implements IConvertible.ToUInt32
            Throw New NotSupportedException()
        End Function
        Public Function ToUInt64(provider As IFormatProvider) As ULong Implements IConvertible.ToUInt64
            Throw New NotSupportedException()
        End Function
        Public Function ToType(conversionType As Type, provider As IFormatProvider) As Object Implements IConvertible.ToType
            Throw New NotSupportedException()
        End Function
    End Class

    <DataTestMethod>
    <DataRow("")>
    <DataRow("sk-SK")>
    Public Sub NativeScalar38PreservesAliasedConverterOutputOnFailedValidation(culture As String)
        Dim old = CultureInfo.CurrentCulture
        Try
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(culture)
            Dim box As New AliasBox38()
            Assert.IsFalse(ExcelArgNumeric.TryGetBinary01(New AliasNumber38(box, 2.25R), box.Value))
            Assert.AreEqual(17, box.Value)
            box.Value = 42
            Assert.IsFalse(ExcelArgNumeric.TryGetWholeNumber(New AliasNumber38(box, 2.25R), box.Value))
            Assert.AreEqual(17, box.Value)
            box.Value = 42
            Assert.IsFalse(ExcelArgNumeric.TryGetWholeNumber(New AliasNumber38(box, Double.NaN), box.Value))
            Assert.AreEqual(17, box.Value)
            box.Value = 42
            Assert.IsFalse(ExcelArgNumeric.TryGetOptionalPositiveInteger(New AliasNumber38(box, 2.25R), box.Value))
            Assert.AreEqual(17, box.Value)
            box.Value = 42
            Assert.ThrowsException(Of OverflowException)(Sub() ExcelArgNumeric.TryGetBinary01(New AliasNumber38(box, Double.MaxValue), box.Value))
            Assert.AreEqual(17, box.Value)
        Finally
            CultureInfo.CurrentCulture = old
        End Try
    End Sub


    <DataTestMethod>
    <DataRow("")>
    <DataRow("sk-SK")>
    Public Sub IndependentAndPaired38KeepLowerBoundsSourceRowsAndOwnership(culture As String)
        Dim old = CultureInfo.CurrentCulture
        Try
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(culture)
            Dim x = Array.CreateInstance(GetType(Object), {4, 1}, {4, 7})
            Dim y = Array.CreateInstance(GetType(Object), {4, 1}, {9, 2})
            For row As Integer = 0 To 3
                x.SetValue(New Object() {"x", 1.0R, Nothing, 3.0R}(row), 4 + row, 7)
                y.SetValue(New Object() {"y", 10.0R, 20.0R, 30.0R}(row), 9 + row, 2)
            Next
            Dim groups()() As Double = Nothing, names() As String = Nothing
            Assert.IsTrue(UdfDataImport.TryGetIndependentNumericColumns(x, y, groups, names))
            CollectionAssert.AreEqual(New String() {"x", "y"}, names)
            CollectionAssert.AreEqual(New Double() {1.0R, 3.0R}, groups(0))
            CollectionAssert.AreEqual(New Double() {10.0R, 20.0R, 30.0R}, groups(1))
            Dim pairs As Double(,) = Nothing
            Assert.IsTrue(UdfDataImport.TryGetPairedNumericColumns(x, y, pairs, names))
            Assert.AreEqual(2, pairs.GetLength(0))
            Assert.AreEqual(1.0R, pairs(0, 0))
            Assert.AreEqual(10.0R, pairs(0, 1))
            Assert.AreEqual(3.0R, pairs(1, 0))
            Assert.AreEqual(30.0R, pairs(1, 1))
            groups(0)(0) = 99.0R
            Assert.AreEqual(1.0R, x.GetValue(5, 7))
            Assert.IsNull(x.GetValue(6, 7))
            Dim thresholds() As Double = {99.0R}
            Assert.IsFalse(ExcelArgNumeric.TryGetOptionalThresholdVector(New Object(,) {{0.1R, ExcelError.ExcelErrorNA}}, thresholds))
            Assert.IsNull(thresholds)
            Assert.IsTrue(ExcelArgNumeric.TryGetOptionalThresholdVector(New Object(,) {{Nothing, 0.2R, 0.2R, 0.1R}}, thresholds))
            CollectionAssert.AreEqual(New Double() {0.1R, 0.2R}, thresholds)
        Finally
            CultureInfo.CurrentCulture = old
        End Try
    End Sub


    <DataTestMethod>
    <DataRow("")>
    <DataRow("sk-SK")>
    Public Sub Inference38ShippingFamiliesRetainErrorsShapesAndPairedRows(culture As String)
        Dim old = CultureInfo.CurrentCulture
        Try
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(culture)
            Dim data As Object(,) = {{"A", "B", "C"}, {1.0R, 4.0R, 3.0R}, {2.0R, 3.0R, 5.0R}, {4.0R, 8.0R, 6.0R}, {3.0R, 6.0R, 9.0R}}
            Assert.IsInstanceOfType(ParametricUDFs.RMANOVA1(data, correction:="both"), GetType(Object(,)))
            Assert.AreEqual(ExcelError.ExcelErrorValue, ParametricUDFs.RMANOVA1(data, correction:="unknown"))
            Assert.IsInstanceOfType(ParametricUDFs.RMANOVA1_MCP(data, method:="all"), GetType(Object(,)))
            Assert.IsInstanceOfType(NonparametricUDFs.KW_MCP(data), GetType(Object(,)))
            Assert.IsInstanceOfType(NonparametricUDFs.FRIEDMAN_MCP(data, method:="all"), GetType(Object(,)))
            Dim x As Object(,) = {{"X"}, {1.0R}, {2.0R}, {4.0R}, {8.0R}}
            Dim y As Object(,) = {{"Y"}, {2.0R}, {4.0R}, {7.0R}, {11.0R}}
            Assert.IsInstanceOfType(ParametricUDFs.TTEST_PAIRED(x, y), GetType(Object(,)))
            Assert.IsInstanceOfType(ParametricUDFs.TTEST_UNPAIRED(x, y, outputType:="welch"), GetType(Object(,)))
            Dim ni = DirectCast(ParametricUDFs.TTEST_UNPAIRED_NI(x, y, 1.0R), Object(,))
            Assert.AreEqual(ExcelError.ExcelErrorValue, ParametricUDFs.TTEST_UNPAIRED_EQUIV(x, y, 2.0R))
            Dim eq = DirectCast(ParametricUDFs.TTEST_UNPAIRED_EQUIV(x, y, 4.0R), Object(,))
            Assert.AreEqual(23, ni.GetLength(0))
            Assert.AreEqual(26, eq.GetLength(0))
            Assert.AreEqual("One-sided p-value", ni(15, 0))
            Assert.IsInstanceOfType(NonparametricUDFs.SPEARMAN_RHO(x, y), GetType(Double))
            Assert.IsInstanceOfType(NonparametricUDFs.KENDALL_P(x, y), GetType(Double))
            Assert.AreEqual(ExcelError.ExcelErrorNum, NonparametricUDFs.KENDALL_P(New Object(,) {{1.0R}, {2.0R}, {3.0R}}, New Object(,) {{2.0R}, {4.0R}, {3.0R}}))
        Finally
            CultureInfo.CurrentCulture = old
        End Try
    End Sub

    <DataTestMethod>
    <DataRow("")>
    <DataRow("sk-SK")>
    Public Sub Trimmed38ShippingReadersRetainShapesNaNsOffsetsAndResets(culture As String)
        Dim previous = CultureInfo.CurrentCulture
        Try
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(culture)
            Dim column As Object(,) = Nothing, name As String = Nothing
            Dim raw As Object(,) = {{" Label "}, {1.0R}, {ExcelError.ExcelErrorNA}, {Double.NaN}, {Double.PositiveInfinity}, {""}}
            Assert.IsTrue(UdfDataImport.TryGetTrimmedColumnObject(raw, column, name))
            Assert.AreEqual("Label", name)
            Assert.AreEqual(4, column.GetLength(0))
            Assert.AreEqual(ExcelError.ExcelErrorNA, column(1, 0))
            Dim values As System.Collections.Generic.List(Of Double) = Nothing
            Assert.IsTrue(UdfDataImport.TryReadNumericColumn(raw, values))
            Assert.AreEqual(4, values.Count)
            Assert.AreEqual(1.0R, values(0))
            Assert.IsTrue(Double.IsNaN(values(1)))
            Assert.IsTrue(Double.IsNaN(values(2)))
            Assert.IsTrue(Double.IsNaN(values(3)))
            Dim offsets() As Double = {99.0R}, rows As Integer = 99
            Assert.IsFalse(UdfDataImport.TryGetRequiredOffsetPredictionInputs(raw, offsets, rows))
            Assert.AreEqual(4, offsets.Length)
            Assert.AreEqual(0, rows)
            Assert.IsTrue(UdfDataImport.TryGetRequiredOffsetPredictionInputs(New Object(,) {{-2.0R}, {0.0R}}, offsets, rows))
            CollectionAssert.AreEqual(New Double() {-2.0R, 0.0R}, offsets)
            Assert.AreEqual(2, rows)
            Assert.IsFalse(UdfDataImport.TryGetRequiredOffsetPredictionInputs(ExcelMissing.Value, offsets, rows))
            Assert.IsNull(offsets)
            Assert.AreEqual(0, rows)
            Assert.IsFalse(UdfDataImport.TryGetTrimmedColumnObject(New Object(,) {{1.0R, 2.0R}}, column, name))
            Assert.IsNull(column)
            Assert.IsNull(name)
            Dim lower = DirectCast(Array.CreateInstance(GetType(Object), {3, 1}, {4, 7}), Object(,))
            lower(4, 7) = "Header" : lower(5, 7) = 2.0R : lower(6, 7) = 3.0R
            Assert.IsTrue(UdfDataImport.TryGetTrimmedColumnObject(lower, column, name))
            Assert.AreEqual("Header", name)
            Assert.AreEqual(2.0R, column(0, 0))
            Dim matrix(,) As Double = Nothing, columns As Integer = 99
            Assert.IsTrue(UdfDataImport.TryReadNumericMatrix(New Object(,) {{"A", "B"}, {1.0R, 2.0R}, {3.0R, 4.0R}, {Nothing, Nothing}}, matrix, rows, columns))
            Assert.AreEqual(2, rows) : Assert.AreEqual(2, columns)
            Assert.AreEqual(4.0R, matrix(1, 1))
            Assert.IsFalse(UdfDataImport.TryReadNumericMatrix(New Object(,) {{Nothing}}, matrix, rows, columns))
            Assert.IsNull(matrix) : Assert.AreEqual(0, rows) : Assert.AreEqual(0, columns)
            Assert.IsFalse(UdfDataImport.TryGetTrimmedColumnObject(New Object(-1, 0) {}, column, name))
            Assert.IsNull(column)
            Assert.ThrowsException(Of NullReferenceException)(Sub() UdfDataImport.TryGetTrimmedColumnObject(New Object(,) {{1.0R}}, column, name, Nothing))
            Assert.IsNull(column)
        Finally
            CultureInfo.CurrentCulture = previous
        End Try
    End Sub

    <TestMethod>
    Public Sub Assumptions38ShippingBoundariesAndSizingStayDistinct()
        Assert.AreEqual(ExcelError.ExcelErrorNum, AssumptionsUDFs.SHAPIRO_WILK(New Object(,) {{1.0R}, {2.0R}}))
        Assert.AreEqual(ExcelError.ExcelErrorNum, AssumptionsUDFs.DAGOSTINO_PEARSON(New Object(,) {{1.0R}, {2.0R}, {3.0R}}))
        Assert.AreEqual(ExcelError.ExcelErrorNum, AssumptionsUDFs.GRUBBS(New Object(,) {{1.0R}, {2.0R}}))
        Assert.AreEqual(ExcelError.ExcelErrorNum, AssumptionsUDFs.ROSNER(New Object(,) {{1.0R}, {2.0R}, {3.0R}}))
        Assert.AreEqual(ExcelError.ExcelErrorNum, AssumptionsUDFs.BOX_M(New Object(,) {{1.0R, 2.0R}, {3.0R, 4.0R}}, New Object(,) {{"a"}, {"b"}}))
        Assert.AreEqual(ExcelError.ExcelErrorValue, AssumptionsUDFs.LEVENE(New Object(,) {{1.0R, 2.0R}, {3.0R, 4.0R}}, "unknown"))
        Assert.AreEqual(ExcelError.ExcelErrorNum, SampleSizeUDFs.SSIZE_TTEST_PAIRED(0.0R, 2.0R, 0.05R, 0.2R))
        Assert.AreEqual(ExcelError.ExcelErrorValue, SampleSizeUDFs.SSIZE_TTEST_PAIRED(ExcelError.ExcelErrorNA, 2.0R, 0.05R, 0.2R))
        Assert.IsInstanceOfType(SampleSizeUDFs.SSIZE_TTEST_PAIRED(2.0R, 5.0R, 0.05R, 0.2R), GetType(Object(,)))
    End Sub


    Private NotInheritable Class Continuation8TextProbe
        Public Reads As Integer
        Public Overrides Function ToString() As String
            Reads += 1
            Return "NI"
        End Function
    End Class

    <DataTestMethod>
    <DataRow("")>
    <DataRow("sk-SK")>
    Public Sub Continuation8PlanningAdaptersKeepErrorPrecedenceAndReportShapes(culture As String)
        Dim previous = CultureInfo.CurrentCulture
        Try
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(culture)
            Dim probe As New Continuation8TextProbe()
            Assert.AreEqual(ExcelError.ExcelErrorNum, SampleSizeUDFs.SSIZE_TTEST_UNPAIRED(1.0R, 0.0R, 1.0R, 0.05R, 0.2R, probe, 0.0R))
            Assert.AreEqual(0, probe.Reads)
            Assert.AreEqual(ExcelError.ExcelErrorNum, SampleSizeUDFs.SSIZE_TTEST_UNPAIRED(1.0R, 2.0R, 1.0R, 0.05R, 0.2R, probe, 0.0R))
            Assert.AreEqual(1, probe.Reads)
            Assert.AreEqual(ExcelError.ExcelErrorValue, SampleSizeUDFs.SSIZE_TTEST_UNPAIRED(1.0R, 2.0R, 1.0R, 0.05R, 0.2R, "unknown", 0.0R))
            Dim unpaired = DirectCast(SampleSizeUDFs.SSIZE_TTEST_UNPAIRED(0.2R, 1.0R, 1.0R, 0.05R, 0.2R, "eq", 1.0R), Object(,))
            Assert.AreEqual(4, unpaired.GetLength(0)) : Assert.AreEqual(4, unpaired.GetLength(1))
            Dim proportions = DirectCast(SampleSizeUDFs.SSIZE_PROP_INDEP(0.4R, 0.45R, 1.0R, 0.05R, 0.2R, "NI", 0.2R), Object(,))
            Assert.AreEqual(3, proportions.GetLength(0)) : Assert.AreEqual(3, proportions.GetLength(1))
            Assert.AreEqual(ExcelError.ExcelErrorValue, SampleSizeUDFs.SSIZE_PROP_SINGLE(Double.NaN, 0.5R, 0.05R, 0.2R))
            Assert.AreEqual(ExcelError.ExcelErrorValue, SampleSizeUDFs.SSIZE_PROP_SINGLE(Double.PositiveInfinity, 0.5R, 0.05R, 0.2R))
            Assert.AreEqual(ExcelError.ExcelErrorValue, SampleSizeUDFs.SSIZE_PROP_SINGLE(True, 0.5R, 0.05R, 0.2R))
            Assert.AreEqual(ExcelError.ExcelErrorValue, SampleSizeUDFs.SSIZE_PROP_SINGLE("0.6", 0.5R, 0.05R, 0.2R))
            Assert.AreEqual(ExcelError.ExcelErrorValue, SampleSizeUDFs.SSIZE_LOGRANK(ExcelError.ExcelErrorNA, 0.3R, 0.2R, 1.0R, 0.05R, 0.2R))
            Assert.AreEqual(ExcelError.ExcelErrorNum, SampleSizeUDFs.SSIZE_LOGRANK(0.7R, 0.0R, 0.2R, 1.0R, 0.05R, 0.2R))
            Dim binary = DirectCast(SampleSizeUDFs.SSIZE_COX_BINARY(0.7R, 1.0R, 0.05R, 0.2R, ExcelMissing.Value, ExcelEmpty.Value, False), Object(,))
            Assert.AreEqual("Binary covariate", binary(1, 1)) : Assert.AreEqual("Not supplied", binary(4, 1))
            Dim continuous = DirectCast(SampleSizeUDFs.SSIZE_COX_CONTINUOUS(0.8R, 1.0R, 0.05R, 0.2R, 0.0R, 0.3R), Object(,))
            Assert.AreEqual("Continuous covariate", continuous(1, 1))
            Assert.AreEqual(ExcelError.ExcelErrorNum, SampleSizeUDFs.SSIZE_COX_BINARY(0.7R, 1.0R, 0.05R, 0.2R, 1.0R, ExcelError.ExcelErrorNA))
            Assert.AreEqual(ExcelError.ExcelErrorNum, SampleSizeUDFs.SSIZE_ICC(0.5R, 0.75R, 2.5R, 0.05R, 0.2R))
            Dim icc = DirectCast(SampleSizeUDFs.SSIZE_ICC(0.5R, 0.75R, 3.0R, 0.05R, 0.2R), Object(,))
            Assert.AreEqual(6, icc.GetLength(0))
            Dim bland = DirectCast(SampleSizeUDFs.SSIZE_BLANDALTMAN(2.0R, 1.0R, 0.05R, ExcelMissing.Value), Object(,))
            Assert.AreEqual(7, bland.GetLength(0)) : Assert.AreEqual(1.96R, bland(6, 1))
        Finally
            CultureInfo.CurrentCulture = previous
        End Try
    End Sub

    <TestMethod>
    Public Sub Continuation8ImportParentsKeepAlignmentFlagsAndFailureResets()
        Dim y As Object(,) = {{"Y"}, {1.0R}, {2.0R}, {3.0R}, {4.0R}}
        Dim x As Object(,) = {{"X"}, {2.0R}, {4.0R}, {6.0R}, {8.0R}}
        Dim offset As Object(,) = {{"Offset"}, {-1.0R}, {0.0R}, {1.0R}, {2.0R}}
        Dim weights As Object(,) = {{"Weight"}, {1.0R}, {1.0R}, {2.0R}, {2.0R}}
        Dim groups As Object(,) = {{"ID"}, {"a"}, {"b"}, {"a"}, {"b"}}
        Dim time As Object(,) = {{"Time"}, {1.0R}, {2.0R}, {3.0R}, {4.0R}}
        Dim glm As BESHStatNG.glmData = Nothing
        Assert.IsTrue(UdfDataImport.TryBuildGlmDataFromUdfArgs(y, x, Nothing, offset, weights, glm))
        Assert.AreEqual(4, glm.nRows) : Assert.IsTrue(glm.bOffset) : Assert.IsTrue(glm.bWeights)
        CollectionAssert.AreEqual(New Double() {-1.0R, 0.0R, 1.0R, 2.0R}, glm.OffsetData)
        Assert.AreEqual("Offset", glm.OffsetVarName) : Assert.AreEqual("Weight", glm.WeightVarName)
        Assert.IsFalse(UdfDataImport.TryBuildGlmDataFromUdfArgs(y, New Object(,) {{1.0R}, {2.0R}}, Nothing, Nothing, Nothing, glm))
        Assert.IsNull(glm)
        Assert.IsTrue(UdfDataImport.TryBuildPredictorDataFromUdfArgs(x, {"X"}, offset, True, glm))
        Assert.IsTrue(glm.bOffset) : Assert.IsFalse(glm.bWeights)
        Assert.IsFalse(UdfDataImport.TryBuildPredictorDataFromUdfArgs(x, {"X"}, ExcelMissing.Value, True, glm))
        Assert.IsNull(glm)
        Dim gee As BESHStatNG.geeData = Nothing
        Assert.IsTrue(UdfDataImport.TryBuildGeeDataFromUdfArgs(y, x, groups, time, Nothing, offset, weights, gee))
        Assert.AreEqual(4, gee.nRows) : Assert.IsTrue(gee.bTime) : Assert.IsTrue(gee.bOffset) : Assert.IsTrue(gee.bWeights)
        Assert.AreEqual(1.0R, Convert.ToDouble(gee.ClusterIdData(0)))
        CollectionAssert.AreEqual(New Double() {1.0R, 1.0R, 2.0R, 2.0R}, gee.ClusterIdData.Select(Function(value) Convert.ToDouble(value)).ToArray())
        CollectionAssert.AreEqual(New Integer() {1, 3, 2, 4}, gee.RowIds)
        CollectionAssert.AreEqual(New Double() {-1.0R, 1.0R, 0.0R, 2.0R}, gee.OffsetData)
        CollectionAssert.AreEqual(New Double() {1.0R, 3.0R, 2.0R, 4.0R}, gee.TimeData)
        Dim cox As BESHStatNG.CoxPHData = Nothing
        Assert.IsFalse(UdfDataImport.TryBuildCoxDataFromUdfArgs(time, New Object(,) {{"Event"}, {0.0R}, {1.0R}, {0.0R}, {1.0R}}, x, Nothing, groups, cox))
        Assert.IsNull(cox)
        Assert.IsTrue(UdfDataImport.TryBuildCoxDataFromUdfArgs(time, New Object(,) {{"Event"}, {0.0R}, {1.0R}, {0.0R}, {1.0R}}, x, Nothing, New Object(,) {{"a"}, {"b"}, {"a"}, {"b"}}, cox))
        Assert.IsTrue(cox.bStrata) : Assert.AreEqual(4, cox.nRows)
        Assert.AreEqual("a", cox.StrataData(0)) : Assert.AreEqual(1, cox.CensorData(1))
        Dim column(,) As Object = Nothing, name As String = Nothing
        Assert.IsTrue(UdfDataImport.TryGetAlignedClusterIdColumnObject(New Object(,) {{"ID"}, {"a"}, {"b"}}, 2, column, name))
        Assert.AreEqual("ID", name) : Assert.AreEqual("b", column(1, 0))
        Assert.IsFalse(UdfDataImport.TryGetAlignedClusterIdColumnObject(New Object(,) {{"a"}, {"b"}}, 3, column, name))
        Assert.IsNull(column) : Assert.IsNull(name)
        Assert.IsTrue(UdfDataImport.TryGetAlignedClusterIdColumnObject(New Object(,) {{"ID"}}, 0, column, name))
        Assert.AreEqual(0, column.GetLength(0))
        Dim inferred As String() = {"duplicate", "duplicate"}
        Assert.AreSame(inferred, UdfDataImport.ResolveImportedPredictorNames(ExcelMissing.Value, inferred))
    End Sub

    <TestMethod>
    Public Sub Continuation8CompiledShippingConsumersReachTheirPreparedApis()
        For Each member In New String() {"SSIZE_TTEST_UNPAIRED", "SSIZE_PROP_SINGLE", "SSIZE_PROP_INDEP", "SSIZE_LOGRANK", "SSIZE_COX_BINARY", "SSIZE_COX_CONTINUOUS", "SSIZE_ICC", "SSIZE_BLANDALTMAN"}
            Dim method = GetType(SampleSizeUDFs).GetMethod(member, BindingFlags.Public Or BindingFlags.Static)
            Assert.IsTrue(Calls(method).Any(Function(target) target.DeclaringType Is GetType(UdfSampleSizePreparation38)), member)
        Next
        For Each member In New String() {"TryBuildGeeDataFromUdfArgs", "TryBuildGlmDataFromUdfArgs", "TryBuildPredictorDataFromUdfArgs", "TryBuildCoxDataFromUdfArgs"}
            Dim method = GetType(UdfDataImport).GetMethod(member, BindingFlags.NonPublic Or BindingFlags.Static)
            Assert.IsTrue(Calls(method).Any(Function(target) target.DeclaringType Is GetType(UdfModelImportPreparation38)), member)
        Next
    End Sub

    <TestMethod>
    Public Sub Continuation9CoefficientAdaptersKeepRowsAndTypedEstimates()
        Dim x As Object(,) = {{1.0R}, {2.0R}, {4.0R}, {5.0R}, {7.0R}, {8.0R}}
        Dim y As Object(,) = {{2.0R}, {3.0R}, {3.5R}, {5.6R}, {6.0R}, {8.0R}}
        For Each result In New Object() {AgreementMethodUDFs.PASSINGBABLOK_COEF(x, y), AgreementMethodUDFs.DEMING_COEF(x, y, ciMethod:="analytical")}
            Assert.IsInstanceOfType(result, GetType(Object(,)))
            Dim table = DirectCast(result, Object(,)), found As Boolean = False
            For row = 0 To table.GetLength(0) - 2
                If Object.Equals(table(row, 0), "Slope") Then
                    Assert.AreEqual("Intercept", CStr(table(row + 1, 0)))
                    Assert.IsInstanceOfType(table(row, 1), GetType(Double))
                    found = True
                End If
            Next
            Assert.IsTrue(found)
        Next
    End Sub

    <TestMethod>
    Public Sub Continuation9BlandAltmanAndRepeatabilityKeepNativeMaterialization()
        Dim x As Object(,) = {{1.0R}, {2.0R}, {4.0R}, {5.0R}, {7.0R}, {8.0R}}
        Dim y As Object(,) = {{2.0R}, {3.0R}, {3.5R}, {5.6R}, {6.0R}, {8.0R}}
        Dim stats = DirectCast(AgreementMethodUDFs.BLANDALTMAN_STATS(x, y), Object(,))
        Assert.IsTrue(stats.Cast(Of Object)().Any(Function(value) Object.Equals(value, "Repeatability coefficient")))
        Dim rc = DirectCast(AgreementMethodUDFs.ICC_RC(New Object(,) {{1.0R, 2.0R}, {2.0R, 3.2R}, {4.0R, 4.4R}, {5.0R, 5.7R}}), Object(,))
        Assert.AreEqual("SEM (standard error of measurement)", CStr(rc(rc.GetLength(0) - 1, 0)))
        Assert.IsInstanceOfType(rc(rc.GetLength(0) - 1, 1), GetType(Double))
    End Sub

    <TestMethod>
    Public Sub Continuation9ClassificationKeepsRowVectorsHeadersAndMissingErrors()
        Dim y As Object(,) = {{"Y", 0.0R, 1.0R, 1.0R, 0.0R}}
        Dim p As Object(,) = {{"P", 0.2R, 0.5R, 0.8R, 0.4R}}
        Dim table = DirectCast(ClassificationReportUDFs.CLASS_CONFUSION(y, p), Object(,))
        Assert.AreEqual(5, table.GetLength(0))
        Assert.AreEqual(4, table.GetLength(1))
        Assert.AreEqual(0, CInt(table(0, 1)))
        Assert.AreEqual(1, CInt(table(0, 2)))
        Dim missing = DirectCast(ClassificationReportUDFs.CLASS_CONFUSION(New Object(,) {{0.0R, 0.0R}}, New Object(,) {{0.1R, 0.2R}}), Object(,))
        Assert.AreEqual(ExcelError.ExcelErrorNA, missing(2, 3))
        Assert.IsInstanceOfType(ClassificationReportUDFs.CLASS_THRESH(y, p), GetType(Object(,)))
        Assert.IsInstanceOfType(ClassificationReportUDFs.CLASS_CALIB(y, p), GetType(Object(,)))
        Assert.IsInstanceOfType(ClassificationReportUDFs.CLASS_BRIER(y, p), GetType(Object(,)))
    End Sub

    <TestMethod>
    Public Sub Continuation9BrierValidationPrecedesHeaderAndLowerBoundsStayNative()
        Dim probe As New Continuation8TextProbe
        Dim result = ClassificationReportUDFs.CLASS_BRIER(New Object(,) {{0.0R, 1.0R}}, New Object(,) {{0.2R, 0.8R}}, New Object(,) {{-1.0R, 1.0R}}, probe)
        Assert.IsInstanceOfType(result, GetType(String))
        Assert.IsTrue(CStr(result).StartsWith("ArgumentException:", StringComparison.Ordinal))
        Assert.AreEqual(0, probe.Reads)
        Dim lower = DirectCast(Array.CreateInstance(GetType(Object), New Integer() {2, 1}, New Integer() {1, 1}), Object(,))
        lower(1, 1) = 0.0R : lower(2, 1) = 1.0R
        result = ClassificationReportUDFs.CLASS_BRIER(lower, New Object(,) {{0.2R}, {0.8R}})
        Assert.IsInstanceOfType(result, GetType(Object(,)))
        Dim expected = DirectCast(ClassificationReportUDFs.CLASS_BRIER(New Object(,) {{0.0R}, {1.0R}}, New Object(,) {{0.2R}, {0.8R}}), Object(,))
        CollectionAssert.AreEqual(expected.Cast(Of Object)().ToArray(), DirectCast(result, Object(,)).Cast(Of Object)().ToArray())
    End Sub

    <TestMethod>
    Public Sub Continuation9ContingencyKeepsShapeFailuresAndPValueTypes()
        Dim oneRow As Object(,) = {{1.0R, 2.0R, 3.0R}}
        Assert.AreEqual(ExcelError.ExcelErrorNum, ContingencyTableUDFs.CT_CHI2(oneRow))
        Assert.AreEqual(ExcelError.ExcelErrorNum, ContingencyTableUDFs.CT_NOMINAL_ASSOC(oneRow))
        Assert.AreEqual(ExcelError.ExcelErrorValue, ContingencyTableUDFs.CT_FISHER_2X2(oneRow))
        Assert.AreEqual(ExcelError.ExcelErrorNum, ContingencyTableUDFs.CT_FFH_EXACT(oneRow))
        Dim tab As Object(,) = {{8.0R, 2.0R}, {3.0R, 7.0R}}
        For Each result In New Object() {ContingencyTableUDFs.CT_CHI2(tab), ContingencyTableUDFs.CT_NOMINAL_ASSOC(tab), ContingencyTableUDFs.CT_FISHER_2X2(tab), ContingencyTableUDFs.CT_FFH_EXACT(tab)}
            Assert.IsInstanceOfType(result, GetType(Object(,)))
        Next
        Assert.AreEqual(ExcelError.ExcelErrorValue, ContingencyTableUDFs.CT_CHI2(New Object(,) {{ExcelError.ExcelErrorNA, 1.0R}, {2.0R, 3.0R}}))
    End Sub

    <TestMethod>
    Public Sub Continuation9RealCompiledConsumersCallEachPortableFamily()
        For Each name In New String() {"PASSINGBABLOK_COEF", "DEMING_COEF", "BLANDALTMAN_STATS", "ICC_RC"}
            Assert.IsTrue(Calls(GetType(AgreementMethodUDFs).GetMethod(name)).Any(Function(target) target.DeclaringType Is GetType(UdfAgreementReportPreparation38)), name)
        Next
        For Each name In New String() {"CT_CHI2", "CT_NOMINAL_ASSOC", "CT_FISHER_2X2", "CT_FFH_EXACT"}
            Assert.IsTrue(Calls(GetType(ContingencyTableUDFs).GetMethod(name)).Any(Function(target) target.DeclaringType Is GetType(UdfContingencyPreparation38)), name)
        Next
        For Each name In New String() {"BuildBinaryCrosstabUdfOutput", "BuildThresholdTableUdfOutput", "BuildCalibrationTableUdfOutput", "BuildBrierScoreUdfOutput"}
            Assert.IsTrue(Calls(GetType(BESHStatNG.regression.BinaryClassificationReporting).GetMethod(name)).Any(Function(target) target.DeclaringType Is GetType(UdfClassificationReportPreparation38)), name)
        Next
    End Sub

    <TestMethod>
    Public Sub Continuation10ContingencyImportAndAlphaPrecedenceRemainNative()
        Dim probe As New Continuation8TextProbe
        Assert.AreEqual(ExcelError.ExcelErrorValue, ContingencyTableUDFs.CT_SINGLE_PROPORTION("invalid", 10, probe))
        Assert.AreEqual(0, probe.Reads)
        Assert.AreEqual(ExcelError.ExcelErrorValue, ContingencyTableUDFs.CT_MCNEMAR_EXACT(New Object(,) {{1.0R, 2.0R, 3.0R}}, probe))
        Assert.AreEqual(0, probe.Reads)
        Assert.AreEqual(ExcelError.ExcelErrorValue, ContingencyTableUDFs.CT_TWO_INDEPENDENT_PROPORTIONS_NI("invalid", 10, 2, 10, probe))
        Assert.AreEqual(0, probe.Reads)
        Assert.AreEqual(ExcelError.ExcelErrorNum, ContingencyTableUDFs.CT_SINGLE_PROPORTION(1, 10, 1.0R))
    End Sub

    <TestMethod>
    Public Sub Continuation10ContingencyHeaderLowerBoundsAndErrorCellsRemainNative()
        Dim lower = DirectCast(Array.CreateInstance(GetType(Object), New Integer() {2, 2}, New Integer() {1, 1}), Object(,))
        lower(1, 1) = 8.0R : lower(1, 2) = 2.0R : lower(2, 1) = 3.0R : lower(2, 2) = 7.0R
        Dim expected = DirectCast(ContingencyTableUDFs.CT_ODDS_RATIO(New Object(,) {{8.0R, 2.0R}, {3.0R, 7.0R}}), Object(,))
        Dim actual = DirectCast(ContingencyTableUDFs.CT_ODDS_RATIO(lower), Object(,))
        CollectionAssert.AreEqual(expected.Cast(Of Object)().ToArray(), actual.Cast(Of Object)().ToArray())
        Assert.AreEqual(ExcelError.ExcelErrorValue, ContingencyTableUDFs.CT_RISK_RATIO(New Object(,) {{ExcelError.ExcelErrorNA, 1.0R}, {2.0R, 3.0R}}))
        Assert.AreEqual(ExcelError.ExcelErrorValue, ContingencyTableUDFs.CT_MANTEL_HAENSZEL(New Object(,) {{1.5R, 1.0R}, {2.0R, 3.0R}}))
    End Sub

    <TestMethod>
    Public Sub Continuation10ProportionOverflowAndMarginBoundariesRemainNative()
        Assert.AreEqual(ExcelError.ExcelErrorValue, ContingencyTableUDFs.CT_PAIRED_PROPORTIONS(Integer.MaxValue, Integer.MaxValue, 0, 1))
        Assert.AreEqual(ExcelError.ExcelErrorNum, ContingencyTableUDFs.CT_TWO_INDEPENDENT_PROPORTIONS_NI(3, 10, 4, 10, 0.0R))
        Assert.AreEqual(ExcelError.ExcelErrorNum, ContingencyTableUDFs.CT_TWO_INDEPENDENT_PROPORTIONS_EQUIV(3, 10, 4, 10, 0.2R, -0.2R))
        Assert.IsInstanceOfType(ContingencyTableUDFs.CT_TWO_INDEPENDENT_PROPORTIONS_EQUIV(30, 100, 35, 100, 0.2R), GetType(Object(,)))
        Assert.IsInstanceOfType(ContingencyTableUDFs.CT_PAIRED_PROPORTIONS(10, 1, 2, 3), GetType(Object(,)))
    End Sub

    <TestMethod>
    Public Sub Continuation10RealContingencyConsumersReachAllElevenPreparedPhases()
        For Each name In New String() {"CT_MCNEMAR_EXACT", "CT_ODDS_RATIO", "CT_RISK_RATIO", "CT_TREND", "CT_ORDINAL_ASSOC", "CT_MANTEL_HAENSZEL", "CT_SINGLE_PROPORTION", "CT_TWO_INDEPENDENT_PROPORTIONS", "CT_PAIRED_PROPORTIONS", "CT_TWO_INDEPENDENT_PROPORTIONS_NI", "CT_TWO_INDEPENDENT_PROPORTIONS_EQUIV"}
            Assert.IsTrue(Calls(GetType(ContingencyTableUDFs).GetMethod(name)).Any(Function(target) target.DeclaringType Is GetType(UdfContingencyPreparation38)), name)
        Next
    End Sub

    Private Shared Function Continuation10NativeData() As Double(,)
        Return New Double(,) {{1, 2, 5}, {2, 1, 7}, {3, 4, 6}, {4, 3, 8}, {5, 6, 9}, {6, 5, 10}, {7, 8, 11}, {8, 7, 12}}
    End Function

    Private Shared Sub WithContinuation10Handle(family As String, model As Object, action As Action(Of String))
        Dim owner = GetType(MultivariateAnalysisUDFs), isPca = (family = "PCA")
        Dim handleType = owner.GetNestedType(If(isPca, "PcaHandle", "FactorAnalysisHandle"), BindingFlags.NonPublic)
        Dim info = Activator.CreateInstance(handleType, True), key = family & ":" & Guid.NewGuid().ToString("N")
        handleType.GetProperty("Handle").SetValue(info, key, Nothing)
        handleType.GetProperty("Model").SetValue(info, model, Nothing)
        handleType.GetProperty("VariableNames").SetValue(info, New String() {"duplicate", "duplicate", "Z"}, Nothing)
        handleType.GetProperty("MatrixType").SetValue(info, "Correlation", Nothing)
        If isPca Then
            handleType.GetProperty("RowIds").SetValue(info, New Integer() {17, 3, 9, 4, 5, 6, 7, 8}, Nothing)
            handleType.GetProperty("RetentionMethod").SetValue(info, "Fixed", Nothing)
            handleType.GetProperty("RetentionValue").SetValue(info, 2.0R, Nothing)
        Else
            handleType.GetProperty("ExtractionMethod").SetValue(info, "PrincipalComponents", Nothing)
            handleType.GetProperty("RotationMethod").SetValue(info, "None", Nothing)
            handleType.GetProperty("ScoreMethod").SetValue(info, "Regression", Nothing)
        End If
        Dim cache = owner.GetField(If(isPca, "_pcaCache", "_faCache"), BindingFlags.NonPublic Or BindingFlags.Static).GetValue(Nothing)
        cache.GetType().GetProperty("Item").SetValue(cache, info, New Object() {key})
        Try
            action(key)
        Finally
            Dim remove = cache.GetType().GetMethods().Single(Function(m) m.Name = "TryRemove" AndAlso m.GetParameters().Length = 2)
            remove.Invoke(cache, New Object() {key, Nothing})
        End Try
    End Sub

    <TestMethod>
    Public Sub Continuation10PcaCachedConsumersKeepNamesRowOrderAndShapes()
        Dim model As New BESHStatNG.Multivariate.PCA
        model.dataInputs(Continuation10NativeData(), New Integer() {17, 3, 9, 4, 5, 6, 7, 8}, New String() {"duplicate", "duplicate", "Z"})
        model.settingsInputs(250, 0.000001R, "Correlation") : model.Calculate("Fixed", 2)
        WithContinuation10Handle("PCA", model,
            Sub(key)
                Dim summary = DirectCast(MultivariateAnalysisUDFs.PCA_SUMMARY(" " & key.ToUpperInvariant() & " ", False), Object(,))
                Assert.AreEqual(6, summary.GetLength(0)) : Assert.AreEqual(8, CInt(summary(1, 1)))
                Dim eigen = DirectCast(MultivariateAnalysisUDFs.PCA_EIGEN(key, True), Object(,))
                Assert.AreEqual(4, eigen.GetLength(0)) : Assert.AreEqual("PC1", eigen(1, 0))
                Dim loadings = DirectCast(MultivariateAnalysisUDFs.PCA_LOADINGS(key, False), Object(,))
                Assert.AreEqual("duplicate", loadings(0, 0)) : Assert.AreEqual("duplicate", loadings(1, 0))
                Dim scores = DirectCast(MultivariateAnalysisUDFs.PCA_SCORES(key, False), Object(,))
                Assert.AreEqual(17, CInt(scores(0, 0))) : Assert.AreEqual(3, CInt(scores(1, 0)))
                Dim matrix = DirectCast(MultivariateAnalysisUDFs.PCA_MATRIX(key, False), Object(,))
                Assert.AreEqual(3, matrix.GetLength(0)) : Assert.AreEqual(4, matrix.GetLength(1))
            End Sub)
    End Sub

    <TestMethod>
    Public Sub Continuation10FactorCachedConsumersKeepMatricesAndSummaryBounds()
        Dim model As New BESHStatNG.Multivariate.FactorAnalysis
        model.dataInputs(Continuation10NativeData(), New Integer() {17, 3, 9, 4, 5, 6, 7, 8}, New String() {"duplicate", "duplicate", "Z"})
        model.settingsInputs(extractionMethod:=BESHStatNG.Multivariate.FactorAnalysisExtractionMethod.PrincipalComponents,
                             retentionMethod:=BESHStatNG.Multivariate.FactorAnalysisRetentionMethod.Fixed, retentionValue:=2)
        model.Calculate()
        WithContinuation10Handle("FA", model,
            Sub(key)
                Dim summary = DirectCast(MultivariateAnalysisUDFs.FA_SUMMARY(key, False), Object(,))
                Assert.AreEqual(13, summary.GetLength(0)) : Assert.AreEqual("RMSR", summary(12, 0))
                Dim eigen = DirectCast(MultivariateAnalysisUDFs.FA_EIGEN(key), Object(,))
                Assert.AreEqual(4, eigen.GetLength(0)) : Assert.AreEqual(10, eigen.GetLength(1))
                For Each value In New Object() {MultivariateAnalysisUDFs.FA_MATRIX(key, False), MultivariateAnalysisUDFs.FA_LOADINGS(key, False), MultivariateAnalysisUDFs.FA_STRUCTURE(key, False)}
                    Dim table = DirectCast(value, Object(,))
                    Assert.AreEqual(3, table.GetLength(0)) : Assert.AreEqual("duplicate", table(0, 0))
                Next
                Dim correlations = DirectCast(MultivariateAnalysisUDFs.FA_FACTORCORR(key, False), Object(,))
                Assert.AreEqual(2, correlations.GetLength(0)) : Assert.AreEqual(3, correlations.GetLength(1))
            End Sub)
    End Sub

    <TestMethod>
    Public Sub Continuation10PcaAndFactorUnknownHandlesPrecedeHeaderReads()
        Dim probe As New Continuation8TextProbe
        Assert.AreEqual(ExcelError.ExcelErrorNA, MultivariateAnalysisUDFs.PCA_SUMMARY("C10 missing handle", probe))
        Assert.AreEqual(ExcelError.ExcelErrorNA, MultivariateAnalysisUDFs.FA_EIGEN("C10 missing handle", probe))
        Assert.AreEqual(0, probe.Reads)
    End Sub

    <TestMethod>
    Public Sub Continuation10FactorDerivedFailurePrecedesNativeHeaderConversion()
        Dim probe As New Continuation8TextProbe
        WithContinuation10Handle("FA", New BESHStatNG.Multivariate.FactorAnalysis(),
            Sub(key)
                Dim result = MultivariateAnalysisUDFs.FA_EIGEN(key, probe)
                Assert.IsInstanceOfType(result, GetType(String))
                Assert.AreEqual(0, probe.Reads)
            End Sub)
    End Sub

    <TestMethod>
    Public Sub Continuation10PcaFactorCompiledConsumersKeepPortableAndHeaderTargets()
        For Each name In New String() {"PCA_SUMMARY", "PCA_EIGEN", "FA_SUMMARY", "FA_EIGEN"}
            Assert.IsTrue(Calls(GetType(MultivariateAnalysisUDFs).GetMethod(name)).Any(Function(target) target.DeclaringType Is GetType(UdfMultivariateReportPreparation38)), name)
        Next
        Dim fa = GetType(MultivariateAnalysisUDFs).GetMethod("FA_EIGEN")
        Assert.IsTrue(Calls(fa).Any(Function(target) target.Name = "GetOptionalBool" AndAlso target.DeclaringType Is GetType(ExcelArgNumeric)))
        Dim api = GetType(UdfMultivariateReportPreparation38).GetMethod("BuildFactorEigen")
        Assert.IsTrue(api.GetParameters()(1).ParameterType.IsByRef)
        Assert.AreEqual(1, Calls(fa).Count(Function(target) target.Name = "GetObjectValue"), "The original handle-argument copy stays native.")
        Dim portableCalls = Calls(api).ToList()
        Assert.AreEqual(1, portableCalls.Where(Function(target) target.Name = "GetObjectValue").Count(), "The original header copy occurs only at late delegate invocation.")
        Assert.IsTrue(portableCalls.FindIndex(Function(target) target.Name = "GetObjectValue") > portableCalls.FindLastIndex(Function(target) target.Name = "Cumulative"))
    End Sub

    Private Shared Sub WithContinuation11Handle(family As String, model As Object, action As Action(Of String))
        Dim owner = GetType(MultivariateAnalysisUDFs)
        Dim typeName = If(family = "KMEANS", "KMeansHandle", If(family = "HCLUST", "HierarchicalHandle", "CorrespondenceHandle"))
        Dim fieldName = If(family = "KMEANS", "_kmeansCache", If(family = "HCLUST", "_hclustCache", "_caCache"))
        Dim handleType = owner.GetNestedType(typeName, BindingFlags.NonPublic)
        Dim info = Activator.CreateInstance(handleType, True), key = family & ":" & Guid.NewGuid().ToString("N")
        handleType.GetProperty("Handle").SetValue(info, key, Nothing)
        handleType.GetProperty("Model").SetValue(info, model, Nothing)
        If family <> "CA" Then
            handleType.GetProperty("VariableNames").SetValue(info, New String() {"same", "same"}, Nothing)
        End If
        Dim cache = owner.GetField(fieldName, BindingFlags.NonPublic Or BindingFlags.Static).GetValue(Nothing)
        cache.GetType().GetProperty("Item").SetValue(cache, info, New Object() {key})
        Try
            action(key)
        Finally
            Dim remove = cache.GetType().GetMethods().Single(Function(method) method.Name = "TryRemove" AndAlso method.GetParameters().Length = 2)
            remove.Invoke(cache, New Object() {key, Nothing})
        End Try
    End Sub

    <TestMethod>
    Public Sub Continuation11UnknownHandlesPrecedeAllHeaderAndSelectorReads()
        Dim probe As New Continuation8TextProbe
        For Each name In New String() {"FA_COMMUNALITIES", "FA_FACTORABILITY", "FA_SCORES", "KMEANS_SUMMARY", "KMEANS_ASSIGNMENTS", "KMEANS_PREPROCESS", "KMEANS_REMOVED", "HCLUST_SUMMARY", "HCLUST_AGGLOM", "HCLUST_LEAFORDER", "HCLUST_PREPROCESS", "HCLUST_REMOVED", "CA_SUMMARY", "CA_EIGEN", "CA_ROWS", "CA_ROW_COORD", "CA_ROW_COS2"}
            Dim method = GetType(MultivariateAnalysisUDFs).GetMethod(name)
            Assert.AreEqual(ExcelError.ExcelErrorNA, method.Invoke(Nothing, New Object() {"C11 missing", probe}), name)
        Next
        Assert.AreEqual(ExcelError.ExcelErrorNA, MultivariateAnalysisUDFs.KMEANS_CENTERS(ExcelError.ExcelErrorNA, probe, probe))
        Assert.AreEqual(ExcelError.ExcelErrorNA, MultivariateAnalysisUDFs.HCLUST_MEMBERSHIP(Nothing, probe, probe, probe))
        Assert.AreEqual(0, probe.Reads)
    End Sub

    <TestMethod>
    Public Sub Continuation11FactorCommunalitiesAndScoresKeepSourceRows()
        Dim model As New BESHStatNG.Multivariate.FactorAnalysis
        model.dataInputs(Continuation10NativeData(), New Integer() {17, 3, 9, 4, 5, 6, 7, 8}, New String() {"duplicate", "duplicate", "Z"})
        model.settingsInputs(extractionMethod:=BESHStatNG.Multivariate.FactorAnalysisExtractionMethod.PrincipalComponents,
            retentionMethod:=BESHStatNG.Multivariate.FactorAnalysisRetentionMethod.Fixed, retentionValue:=2)
        model.Calculate()
        WithContinuation10Handle("FA", model,
            Sub(key)
                Dim table = DirectCast(MultivariateAnalysisUDFs.FA_COMMUNALITIES(key, False), Object(,))
                Assert.AreEqual(3, table.GetLength(0)) : Assert.AreEqual(6, table.GetLength(1))
                Assert.AreEqual("duplicate", table(0, 0))
                Dim scores = DirectCast(MultivariateAnalysisUDFs.FA_SCORES(key, False), Object(,))
                Assert.AreEqual(17, scores(0, 0)) : Assert.AreEqual(3, scores(1, 0))
                Dim failure = MultivariateAnalysisUDFs.FA_FACTORABILITY(key, False)
                Assert.IsInstanceOfType(failure, GetType(String))
                StringAssert.StartsWith(CStr(failure), "IndexOutOfRangeException:")
            End Sub)
    End Sub

    <TestMethod>
    Public Sub Continuation11MissingScoresPrecedeHeaderCoercion()
        Dim probe As New Continuation8TextProbe
        WithContinuation10Handle("FA", New BESHStatNG.Multivariate.FactorAnalysis(),
            Sub(key)
                Assert.AreEqual(ExcelError.ExcelErrorNA, MultivariateAnalysisUDFs.FA_SCORES(key, probe))
                Assert.AreEqual(0, probe.Reads)
            End Sub)
    End Sub

    <TestMethod>
    Public Sub Continuation11KMeansReportsKeepListwiseIdsAndCacheReference()
        Dim model As New BESHStatNG.Multivariate.KMeans
        model.dataInputs(New Double(,) {{1, 2}, {Double.NaN, 3}, {2, 1}, {9, 8}, {8, 9}},
            New String() {"A", "removed", "A", "D", "E"}, New String() {"same", "same"})
        model.settingsInputs(numberOfClusters:=2, nStarts:=1, randomSeed:=38,
            standardization:=BESHStatNG.Multivariate.ClusterStandardizationMode.ZScores,
            missingValuePolicy:=BESHStatNG.Multivariate.ClusterMissingValuePolicy.ListwiseDeletion)
        model.Fit()
        Dim before = model.Result
        WithContinuation11Handle("KMEANS", model,
            Sub(key)
                Dim summary = DirectCast(MultivariateAnalysisUDFs.KMEANS_SUMMARY(" " & key.ToUpperInvariant() & " ", False), Object(,))
                Assert.AreEqual(4, summary(1, 1)) : Assert.AreEqual(1, summary(2, 1))
                Dim assignments = DirectCast(MultivariateAnalysisUDFs.KMEANS_ASSIGNMENTS(key, False), Object(,))
                Assert.AreEqual(1, assignments(0, 0)) : Assert.AreEqual(3, assignments(1, 0))
                Dim removed = DirectCast(MultivariateAnalysisUDFs.KMEANS_REMOVED(key, False), Object(,))
                Assert.AreEqual(2, removed(0, 0)) : Assert.AreEqual("removed", removed(0, 1))
                Assert.IsInstanceOfType(MultivariateAnalysisUDFs.KMEANS_CENTERS(key, "working", False), GetType(Object(,)))
                Assert.IsInstanceOfType(MultivariateAnalysisUDFs.KMEANS_PREPROCESS(key, False), GetType(Object(,)))
                Assert.AreSame(before, model.Result)
            End Sub)
    End Sub

    <TestMethod>
    Public Sub Continuation11HierarchicalReportsKeepBothCutModesAndLeafIds()
        Dim model As New BESHStatNG.Multivariate.HierarchicalClustering
        model.dataInputs(New Double(,) {{1, 2}, {Double.NaN, 3}, {2, 1}, {9, 8}, {8, 9}},
            New String() {"A", "removed", "A", "D", "E"}, New String() {"same", "same"})
        model.settingsInputs(missingValuePolicy:=BESHStatNG.Multivariate.ClusterMissingValuePolicy.ListwiseDeletion)
        model.Fit()
        Dim before = model.Result
        WithContinuation11Handle("HCLUST", model,
            Sub(key)
                Dim summary = DirectCast(MultivariateAnalysisUDFs.HCLUST_SUMMARY(key, False), Object(,))
                Assert.AreEqual(4, summary(5, 1)) : Assert.AreEqual(1, summary(6, 1))
                Dim leaves = DirectCast(MultivariateAnalysisUDFs.HCLUST_LEAFORDER(key, False), Object(,))
                Assert.AreEqual(4, leaves.GetLength(0))
                CollectionAssert.AreEquivalent(New Integer() {1, 3, 4, 5}, Enumerable.Range(0, 4).Select(Function(i) CInt(leaves(i, 1))).ToArray())
                Assert.IsInstanceOfType(MultivariateAnalysisUDFs.HCLUST_MEMBERSHIP(key, "count", 2, False), GetType(Object(,)))
                Assert.IsInstanceOfType(MultivariateAnalysisUDFs.HCLUST_MEMBERSHIP(key, "height", 0.0R, False), GetType(Object(,)))
                Assert.IsInstanceOfType(MultivariateAnalysisUDFs.HCLUST_AGGLOM(key, False), GetType(Object(,)))
                Assert.IsInstanceOfType(MultivariateAnalysisUDFs.HCLUST_PREPROCESS(key, False), GetType(Object(,)))
                Assert.AreSame(before, model.Result)
            End Sub)
    End Sub

    <TestMethod>
    Public Sub Continuation11CaDistinctCallbacksKeepAxisValuesAndNames()
        Dim model As New BESHStatNG.Multivariate.CA
        model.data(New Integer(,) {{8, 2, 1}, {2, 7, 3}, {1, 3, 9}}, New String() {"a", "a", "c"}, New String() {"x", "y", "z"})
        model.Calculate()
        WithContinuation11Handle("CA", model,
            Sub(key)
                Dim coords = DirectCast(MultivariateAnalysisUDFs.CA_ROW_COORD(key, False), Object(,))
                Dim cos2 = DirectCast(MultivariateAnalysisUDFs.CA_ROW_COS2(key, False), Object(,))
                Assert.AreEqual("a", coords(0, 0)) : Assert.AreEqual("a", coords(1, 0))
                Assert.AreEqual(model.RowFactors(0)(0), CDbl(coords(0, 1)))
                Assert.AreEqual(model.RowCorr(0)(0), CDbl(cos2(0, 1)))
                Assert.IsInstanceOfType(MultivariateAnalysisUDFs.CA_SUMMARY(key, False), GetType(Object(,)))
                Assert.IsInstanceOfType(MultivariateAnalysisUDFs.CA_EIGEN(key, False), GetType(Object(,)))
                Assert.IsInstanceOfType(MultivariateAnalysisUDFs.CA_ROWS(key, False), GetType(Object(,)))
            End Sub)
    End Sub

    <TestMethod>
    Public Sub Continuation11ShippingConsumersCallFiveExactLayouts()
        For Each name In New String() {"FA_COMMUNALITIES", "FA_FACTORABILITY", "KMEANS_SUMMARY", "HCLUST_SUMMARY", "CA_SUMMARY"}
            Assert.IsTrue(Calls(GetType(MultivariateAnalysisUDFs).GetMethod(name)).Any(Function(target) target.DeclaringType Is GetType(UdfMultivariateReportPreparation38)), name)
        Next
        Assert.IsTrue(GetType(UdfMultivariateReportPreparation38).GetMethod("BuildHierarchicalSummary").GetParameters()(8).ParameterType.IsByRef)
    End Sub

    Private Shared Sub WithContinuation12Handle(family As String, model As Object, action As Action(Of String), Optional validation As String = "KFold")
        Dim owner = GetType(MultivariateAnalysisUDFs)
        Dim typeName = If(family = "CA", "CorrespondenceHandle", If(family = "MCA", "MultipleCorrespondenceHandle", "DiscriminantHandle"))
        Dim fieldName = If(family = "CA", "_caCache", If(family = "MCA", "_mcaCache", "_daCache"))
        Dim handleType = owner.GetNestedType(typeName, BindingFlags.NonPublic)
        Dim info = Activator.CreateInstance(handleType, True), key = family & ":" & Guid.NewGuid().ToString("N")
        Dim settings As New Dictionary(Of String, Object) From {{"Handle", key}, {"Model", model}}
        If family = "MCA" Then
            settings.Add("VariableNames", New String() {"First", "Second"})
            settings.Add("ObservationCount", 4)
        ElseIf family = "DA" Then
            settings.Add("VariableNames", New String() {"X", "Y"})
            settings.Add("GroupVariableName", "Class")
            settings.Add("Method", "Linear")
            settings.Add("Standardization", "None")
            settings.Add("MissingValuePolicy", "ErrorOnMissing")
            settings.Add("PriorMode", "Equal")
            settings.Add("ValidationMode", validation)
            settings.Add("NumberOfFolds", 3)
            settings.Add("HoldoutFraction", 0.25R)
            settings.Add("Stratified", True)
            settings.Add("CovarianceRegularization", 0.00000001R)
        End If
        For Each pair In settings
            handleType.GetProperty(pair.Key).SetValue(info, pair.Value, Nothing)
        Next
        Dim cache = owner.GetField(fieldName, BindingFlags.NonPublic Or BindingFlags.Static).GetValue(Nothing)
        cache.GetType().GetProperty("Item").SetValue(cache, info, New Object() {key})
        Try
            action(key)
        Finally
            Dim remove = cache.GetType().GetMethods().Single(Function(method) method.Name = "TryRemove" AndAlso method.GetParameters().Length = 2)
            remove.Invoke(cache, New Object() {key, Nothing})
        End Try
    End Sub

    Private Shared Function Continuation12Import(name As String, arguments() As Object) As Boolean
        Dim owner = GetType(MultivariateAnalysisUDFs).Assembly.GetType("BESHStatNG.UdfDataImport", True)
        Return CBool(owner.GetMethod(name, BindingFlags.Static Or BindingFlags.Public Or BindingFlags.NonPublic).Invoke(Nothing, arguments))
    End Function

    <TestMethod>
    Public Sub Continuation12MissingHandlesPrecedeHeaderAndModelReads()
        Dim probe As New Continuation8TextProbe
        For Each name In New String() {"CA_ROW_CONTRIB", "CA_COLUMNS", "CA_COL_COORD", "CA_COL_COS2", "CA_COL_CONTRIB", "MCA_SUMMARY", "MCA_EIGEN", "MCA_BURT", "MCA_INDICATOR", "MCA_CATEGORIES", "MCA_COORD", "MCA_COS2", "MCA_CONTRIB", "DA_SUMMARY", "DA_GROUPSUMMARY"}
            Assert.AreEqual(ExcelError.ExcelErrorNA, GetType(MultivariateAnalysisUDFs).GetMethod(name).Invoke(Nothing, New Object() {"C12 absent", probe}), name)
        Next
        Assert.AreEqual(0, probe.Reads)
    End Sub

    <TestMethod>
    Public Sub Continuation12CaCallbacksKeepSeparateRowAndColumnMetrics()
        Dim model As New BESHStatNG.Multivariate.CA()
        model.data(New Integer(,) {{8, 2, 1}, {2, 7, 3}, {1, 3, 9}}, New String() {"r", "r", "s"}, New String() {"c", "c", "d"})
        model.Calculate()
        WithContinuation12Handle("CA", model,
            Sub(key)
                Dim rows = DirectCast(MultivariateAnalysisUDFs.CA_ROW_CONTRIB(key, False), Object(,))
                Dim columns = DirectCast(MultivariateAnalysisUDFs.CA_COLUMNS(key, False), Object(,))
                Dim coord = DirectCast(MultivariateAnalysisUDFs.CA_COL_COORD(key, False), Object(,))
                Dim cos2 = DirectCast(MultivariateAnalysisUDFs.CA_COL_COS2(key, False), Object(,))
                Dim contribution = DirectCast(MultivariateAnalysisUDFs.CA_COL_CONTRIB(key, False), Object(,))
                Assert.AreEqual(model.RowContribution(0)(0), CDbl(rows(0, 1)))
                Assert.AreEqual(model.ColFactors(0)(0), CDbl(coord(0, 1)))
                Assert.AreEqual(model.ColCorr(0)(0), CDbl(cos2(0, 1)))
                Assert.AreEqual(model.ColContribution(0)(0), CDbl(contribution(0, 1)))
                Assert.AreEqual("c", coord(0, 0))
                Assert.AreEqual("c", coord(1, 0))
                Assert.AreEqual(3, columns.GetLength(0))
            End Sub)
    End Sub

    <TestMethod>
    Public Sub Continuation12McaReportsKeepCategoryOrderAndCacheModel()
        Dim model As New BESHStatNG.Multivariate.CA()
        model.DataMultiple(New String(,) {{"a", "x"}, {"a", "y"}, {"b", "x"}, {"b", "y"}}, New String() {"First", "Second"})
        model.Calculate()
        Dim before = model.Eigenvalues
        WithContinuation12Handle("MCA", model,
            Sub(key)
                Dim summary = DirectCast(MultivariateAnalysisUDFs.MCA_SUMMARY(" " & key.ToUpperInvariant() & " ", False), Object(,))
                Assert.AreEqual(4, summary(1, 1))
                Assert.AreEqual(2, summary(2, 1))
                Assert.AreEqual(4, summary(3, 1))
                For Each name In New String() {"MCA_EIGEN", "MCA_BURT", "MCA_INDICATOR", "MCA_CATEGORIES", "MCA_COORD", "MCA_COS2", "MCA_CONTRIB"}
                    Assert.IsInstanceOfType(GetType(MultivariateAnalysisUDFs).GetMethod(name).Invoke(Nothing, New Object() {key, False}), GetType(Object(,)), name)
                Next
                Dim coord = DirectCast(MultivariateAnalysisUDFs.MCA_COORD(key, False), Object(,))
                Dim cos2 = DirectCast(MultivariateAnalysisUDFs.MCA_COS2(key, False), Object(,))
                Dim contribution = DirectCast(MultivariateAnalysisUDFs.MCA_CONTRIB(key, False), Object(,))
                Assert.AreEqual(model.ColFactors(0)(0), CDbl(coord(0, 2)))
                Assert.AreEqual(model.ColCorr(0)(0), CDbl(cos2(0, 2)))
                Assert.AreEqual(model.ColContribution(0)(0), CDbl(contribution(0, 2)))
                Assert.AreSame(before, model.Eigenvalues)
            End Sub)
    End Sub

    <TestMethod>
    Public Sub Continuation12DaSummaryKeepsHostValidationDetailAndGroupReport()
        Dim model As New BESHStatNG.Multivariate.DiscriminantAnalysis()
        model.dataInputs(New Double(,) {{1, 2}, {2, 1}, {3, 3}, {4, 2}, {8, 9}, {9, 8}, {10, 10}, {11, 9}},
            New Object() {"A", "A", "A", "A", "B", "B", "B", "B"},
            New String() {"r1", "r2", "r3", "r4", "r5", "r6", "r7", "r8"}, New String() {"X", "Y"})
        model.settingsInputs()
        model.Fit()
        Dim before = model.TrainingClassification
        WithContinuation12Handle("DA", model,
            Sub(key)
                Dim summary = DirectCast(MultivariateAnalysisUDFs.DA_SUMMARY(key, False), Object(,))
                Assert.AreEqual(14, summary.GetLength(0))
                Assert.AreEqual(8, summary(4, 1))
                Assert.AreEqual("3 folds; Stratified=True", summary(13, 1))
                Assert.IsInstanceOfType(MultivariateAnalysisUDFs.DA_GROUPSUMMARY(key, False), GetType(Object(,)))
                Assert.AreSame(before, model.TrainingClassification)
            End Sub)
    End Sub

    <TestMethod>
    Public Sub Continuation12DaModelFailureAndModeFailureBothPrecedeHeader()
        Dim probe As New Continuation8TextProbe
        WithContinuation12Handle("DA", Nothing,
            Sub(key)
                StringAssert.StartsWith(CStr(MultivariateAnalysisUDFs.DA_SUMMARY(key, probe)), "NullReferenceException:")
            End Sub, "not a validation mode")
        WithContinuation12Handle("DA", New BESHStatNG.Multivariate.DiscriminantAnalysis(),
            Sub(key)
                StringAssert.StartsWith(CStr(MultivariateAnalysisUDFs.DA_SUMMARY(key, probe)), "ArgumentException:")
            End Sub, "not a validation mode")
        Assert.AreEqual(0, probe.Reads)
    End Sub

    <DataTestMethod, DataRow(""), DataRow("sk-SK")>
    Public Sub Continuation12CategoricalImportKeepsExcelSentinelsAndInvariantNumbers(culture As String)
        Dim previous = CultureInfo.CurrentCulture
        Try
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(culture)
            Dim arguments As Object() = {New Object(,) {{" First ", ExcelError.ExcelErrorNA}, {1.5R, ExcelEmpty.Value}, {"tail", "z"}}, Nothing, True, Nothing, Nothing}
            Assert.IsTrue(Continuation12Import("TryGetCategoricalMatrix", arguments))
            Dim data = DirectCast(arguments(3), String(,)), names = DirectCast(arguments(4), String())
            CollectionAssert.AreEqual(New String() {"First", "ExcelErrorNA"}, names)
            Assert.AreEqual("1.5", data(0, 0))
            Assert.AreEqual("", data(0, 1))
            Assert.AreEqual("tail", data(1, 0))
        Finally
            CultureInfo.CurrentCulture = previous
        End Try
    End Sub

    <DataTestMethod, DataRow(""), DataRow("sk-SK")>
    Public Sub Continuation12VectorsKeepDelimitedPriorityAndNativeNumericErrorPolicy(culture As String)
        Dim previous = CultureInfo.CurrentCulture
        Try
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(culture)
            Dim arguments As Object() = {"1,5;2,5", Nothing}
            Assert.IsTrue(Continuation12Import("TryGetDoubleVector", arguments))
            CollectionAssert.AreEqual(New Double() {15, 25}, DirectCast(arguments(1), Double()))
            arguments = New Object() {"NaN", Nothing}
            Assert.IsTrue(Continuation12Import("TryGetDoubleVector", arguments))
            Assert.IsTrue(Double.IsNaN(DirectCast(arguments(1), Double())(0)))
            arguments = New Object() {New Object(,) {{Double.NaN}}, New Double() {17}}
            Assert.IsFalse(Continuation12Import("TryGetDoubleVector", arguments))
            Assert.IsNull(arguments(1))
            arguments = New Object() {New Object(,) {{1, ExcelError.ExcelErrorValue, 3}}, New Double() {17}}
            Assert.IsFalse(Continuation12Import("TryGetDoubleVector", arguments))
            Assert.IsNull(arguments(1))
            arguments = New Object() {New Object(,) {{" a ", ExcelEmpty.Value, ExcelError.ExcelErrorNA}}, Nothing}
            Assert.IsTrue(Continuation12Import("TryGetStringVector", arguments))
            CollectionAssert.AreEqual(New String() {"a", "", "ExcelErrorNA"}, DirectCast(arguments(1), String()))
        Finally
            CultureInfo.CurrentCulture = previous
        End Try
    End Sub

    <TestMethod>
    Public Sub Continuation12ShippingConsumersCallExactPreparedOwners()
        For Each name In New String() {"MCA_SUMMARY", "DA_SUMMARY"}
            Assert.IsTrue(Calls(GetType(MultivariateAnalysisUDFs).GetMethod(name)).Any(Function(target) target.DeclaringType Is GetType(UdfMultivariateReportPreparation38)), name)
        Next
        Dim importer = GetType(MultivariateAnalysisUDFs).Assembly.GetType("BESHStatNG.UdfDataImport", True)
        For Each name In New String() {"TryGetCategoricalMatrix", "TryGetStringVector", "TryGetDoubleVector"}
            Assert.IsTrue(Calls(importer.GetMethod(name, BindingFlags.NonPublic Or BindingFlags.Public Or BindingFlags.Static)).Any(Function(target) target.DeclaringType Is GetType(UdfModelImportPreparation38)), name)
        Next
        Assert.IsTrue(GetType(UdfModelImportPreparation38).GetMethod("CompleteCategoricalMatrixPrepared").GetParameters()(1).ParameterType.IsByRef)
        Assert.IsTrue(Calls(GetType(MultivariateAnalysisUDFs).GetMethod("DA_SUMMARY")).Any(Function(target) target.Name = "ParseDiscriminantValidationMode"))
    End Sub

    <DataTestMethod, DataRow(""), DataRow("sk-SK")>
    Public Sub Continuation13RocImportKeepsErrorRowsDirectionAndNativeNumberPolicy(culture As String)
        Dim previous = CultureInfo.CurrentCulture
        Try
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(culture)
            Dim arguments As Object() = {New Object(,) {{10}, {ExcelError.ExcelErrorValue}, {30}, {40}}, New Object(,) {{1}, {0}, {1}, {0}}, Nothing, "lower", Nothing, Nothing, Nothing, False}
            Assert.IsTrue(Continuation12Import("TryGetRocInput", arguments))
            Dim values = DirectCast(arguments(4), Double()())
            CollectionAssert.AreEqual(New Double() {-10, -30}, values(0))
            CollectionAssert.AreEqual(New Double() {-40}, values(1))
            Assert.AreEqual("1", arguments(6))
            Assert.AreEqual(True, arguments(7))
            arguments = New Object() {New Object(,) {{"1,5"}, {"2,5"}}, New Object(,) {{1}, {0}}, Nothing, "lower", Nothing, Nothing, Nothing, False}
            Assert.IsTrue(Continuation12Import("TryGetRocInput", arguments))
            values = DirectCast(arguments(4), Double()())
            CollectionAssert.AreEqual(New Double() {-15}, values(0))
            CollectionAssert.AreEqual(New Double() {-25}, values(1))
        Finally
            CultureInfo.CurrentCulture = previous
        End Try
    End Sub

    <TestMethod>
    Public Sub Continuation13FailedAcquisitionRetainsByRefResetAndDirectionPrecedence()
        Dim arguments As Object() = {Nothing, Nothing, Nothing, "lower", New Double()() {New Double() {17}}, "old marker", "old positive", False}
        Assert.IsFalse(Continuation12Import("TryGetRocInput", arguments))
        Assert.IsNull(arguments(4))
        Assert.IsNull(arguments(5))
        Assert.IsNull(arguments(6))
        Assert.AreEqual(True, arguments(7))
    End Sub

    <DataTestMethod, DataRow(""), DataRow("sk-SK")>
    Public Sub Continuation13ActualPlotConsumersKeepHeadersShapesAndExcelErrors(culture As String)
        Dim previous = CultureInfo.CurrentCulture
        Try
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(culture)
            Dim source As Object(,) = {{1}, {2}, {3}, {4}}
            Dim histogram = DirectCast(PlotDataUDFs.HIST_BINS(source), Object(,))
            Assert.AreEqual(3, histogram.GetLength(1))
            Assert.AreEqual("BinMidpoint", histogram(0, 1))
            Dim normal = DirectCast(PlotDataUDFs.HIST_NORMAL(source), Object(,))
            Assert.AreEqual(3, normal.GetLength(1))
            Assert.AreEqual("NormalFrequency", normal(0, 2))
            Dim roc = DirectCast(PlotDataUDFs.ROC_POINTS(New Object(,) {{1}, {2}, {3}, {4}}, New Object(,) {{0}, {0}, {1}, {1}}), Object(,))
            Assert.AreEqual(5, roc.GetLength(1))
            Assert.AreEqual("Threshold", roc(0, 0))
            Dim stats = DirectCast(PlotDataUDFs.ROC_STATS(New Object(,) {{1}, {2}, {3}, {4}}, New Object(,) {{0}, {0}, {1}, {1}}), Object(,))
            Assert.AreEqual(2, stats.GetLength(1))
            Assert.AreEqual("Metric", stats(0, 0))
            Dim curve = DirectCast(PlotDataUDFs.KM_CURVE(New Object(,) {{2}, {5}}, New Object(,) {{1}, {0}}), Object(,))
            Assert.AreEqual(7, curve.GetLength(1))
            Assert.AreEqual("PlotOrder", curve(0, 1))
            Dim calibration = DirectCast(PlotDataUDFs.CALIB_POINTS(New Object(,) {{0}, {1}, {0}, {1}}, New Object(,) {{0.1R}, {0.8R}, {0.3R}, {0.9R}}, 2, "equalwidth"), Object(,))
            Assert.AreEqual(8, calibration.GetLength(1))
            Assert.AreEqual("Bin", calibration(0, 0))
            Assert.AreEqual(ExcelError.ExcelErrorNum, PlotDataUDFs.ROC_POINTS(Nothing, Nothing, Nothing, Nothing, -1))
            Assert.AreEqual(ExcelError.ExcelErrorNum, PlotDataUDFs.KM_CURVE(Nothing, Nothing, Nothing, -1))
        Finally
            CultureInfo.CurrentCulture = previous
        End Try
    End Sub

    <TestMethod>
    Public Sub Continuation13AllDaMissingHandlesPrecedeEveryOtherArgument()
        Dim probe As New Continuation8TextProbe
        For Each name In New String() {"DA_MEANS", "DA_PREPROCESS", "DA_COVARIANCE", "DA_FUNCTIONS", "DA_CANONICAL", "DA_CANONCOEF", "DA_CENTROIDS", "DA_CONFUSION", "DA_CASEWISE", "DA_PREDICT", "DA_REMOVED"}
            Dim method = GetType(MultivariateAnalysisUDFs).GetMethod(name)
            Dim arguments = Enumerable.Repeat(Of Object)(probe, method.GetParameters().Length).ToArray()
            arguments(0) = "C13 absent"
            Assert.AreEqual(ExcelError.ExcelErrorNA, method.Invoke(Nothing, arguments), name)
        Next
        Assert.AreEqual(0, probe.Reads)
    End Sub

    <DataTestMethod, DataRow(""), DataRow("sk-SK")>
    Public Sub Continuation13DaReportsAndPredictionKeepCacheModelAndSourceRows(culture As String)
        Dim previous = CultureInfo.CurrentCulture
        Try
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(culture)
            Dim model As New BESHStatNG.Multivariate.DiscriminantAnalysis()
            model.dataInputs(New Double(,) {{1, 2}, {2, 1}, {3, 3}, {4, 2}, {8, 9}, {9, 8}, {10, 10}, {11, 9}},
                New Object() {"A", "A", "A", "A", "B", "B", "B", "B"},
                New String() {"r1", "r2", "r3", "r4", "r5", "r6", "r7", "r8"}, New String() {"X", "Y"})
            model.settingsInputs()
            model.Fit()
            Dim training = model.TrainingClassification, prepared = model.PreparedData, first = model.GroupStatistics(0)
            WithContinuation12Handle("DA", model,
                Sub(key)
                    Dim means = DirectCast(MultivariateAnalysisUDFs.DA_MEANS(key, "original", True), Object(,))
                    Assert.AreEqual("Group", means(0, 0))
                    Assert.AreEqual(first.MeanOriginal(0), means(1, 1))
                    Dim covariance = DirectCast(MultivariateAnalysisUDFs.DA_COVARIANCE(key, "a", False), Object(,))
                    Assert.AreEqual(first.CovarianceWorking(0, 0), covariance(0, 1))
                    Dim prediction = DirectCast(MultivariateAnalysisUDFs.DA_PREDICT(key, New Object(,) {{2, 2}, {9, 9}},
                        New Object(,) {{"caseB"}, {"caseA"}}, New Object(,) {{"A"}, {"B"}}, True), Object(,))
                    Assert.AreEqual("caseB", prediction(1, 1))
                    Assert.AreEqual("caseA", prediction(2, 1))
                    Assert.AreEqual("A", prediction(1, 2))
                    Assert.AreEqual("B", prediction(2, 2))
                    Dim probe As New Continuation8TextProbe
                    StringAssert.StartsWith(CStr(MultivariateAnalysisUDFs.DA_COVARIANCE(key, "absent", probe)), "ArgumentException:")
                    StringAssert.StartsWith(CStr(MultivariateAnalysisUDFs.DA_CONFUSION(key, "bad source", probe)), "ArgumentException:")
                    Assert.AreEqual(0, probe.Reads)
                    Assert.AreSame(training, model.TrainingClassification)
                    Assert.AreSame(prepared, model.PreparedData)
                    Assert.AreSame(first, model.GroupStatistics(0))
                End Sub)
        Finally
            CultureInfo.CurrentCulture = previous
        End Try
    End Sub

    <DataTestMethod>
    <DataRow("BESHStatNG.WorksheetFunctions.MultivariateAnalysisUDFs", "FindWrappedDiscriminantTable", "BESHStatNG.ExcelCommon.MultivariateAnalysisUDFsReports38")>
    <DataRow("BESHStatNG.WorksheetFunctions.MultivariateAnalysisUDFs", "DA_COVARIANCE", "BESHStatNG.ExcelCommon.MultivariateAnalysisUDFsReports38")>
    <DataRow("BESHStatNG.WorksheetFunctions.PlotDataUDFs", "HIST_BINS", "BESHStatNG.ExcelCommon.PlotDataUDFsReports38")>
    <DataRow("BESHStatNG.WorksheetFunctions.PlotDataUDFs", "HIST_NORMAL", "BESHStatNG.ExcelCommon.PlotDataUDFsReports38")>
    <DataRow("BESHStatNG.WorksheetFunctions.PlotDataUDFs", "BuildKaplanMeierStepTable", "BESHStatNG.ExcelCommon.PlotDataUDFsReports38")>
    <DataRow("BESHStatNG.WorksheetFunctions.PlotDataUDFs", "StackHomogeneousTables", "BESHStatNG.ExcelCommon.PlotDataUDFsReports38")>
    <DataRow("BESHStatNG.UdfDataImport", "TryGetRocInput", "BESHStatNG.ExcelCommon.UdfPlotDataPreparation38")>
    <DataRow("BESHStatNG.UdfDataImport", "TryGetSurvivalRecords", "BESHStatNG.ExcelCommon.UdfPlotDataPreparation38")>
    Public Sub Continuation13ShippingBodiesBindActualPreparedOwners(owner As String, name As String, portable As String)
        Dim assembly = GetType(MultivariateAnalysisUDFs).Assembly
        Dim method = assembly.GetType(owner, True).GetMethod(name, BindingFlags.Static Or BindingFlags.Public Or BindingFlags.NonPublic)
        Assert.IsTrue(Calls(method).Any(Function(target) target.DeclaringType.FullName = portable), name)
        Assert.IsTrue(GetType(UdfPlotFiniteDoubleReader38).GetMethod("Invoke").GetParameters()(1).ParameterType.IsByRef)
        Assert.IsTrue(GetType(UdfPlotStatusReader38).GetMethod("Invoke").GetParameters()(1).ParameterType.IsByRef)
    End Sub


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
        il.Emit(Global.System.Reflection.Emit.OpCodes.Ldarg_0) : il.Emit(Global.System.Reflection.Emit.OpCodes.Ldarg_1) : il.Emit(Global.System.Reflection.Emit.OpCodes.Ldarg_2)
        il.Emit(Global.System.Reflection.Emit.OpCodes.Call, method) : il.Emit(Global.System.Reflection.Emit.OpCodes.Ret)
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
            Assert.AreEqual("ExcelErrorValue", Continuation14Label)
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

    <DataTestMethod, DataRow(False), DataRow(True)>
    Public Sub Continuation15CoxSummaryTestsHandleAlphaAndDropBoundaries(header As Boolean)
        Dim owner = GetType(CoxUDFs), nested = owner.GetNestedType("CoxModelHandle", BindingFlags.NonPublic)
        Dim handle = Activator.CreateInstance(nested, True)
        Dim result As New Global.BESHStatNG.CoxResult With {
            .Coefficients = New Double() {1}, .VarCov = New Double(,) {{1}},
            .LogLikelihoodNull = -10, .LogLikelihood = -8, .Iterations = 3, .Converged = True}
        nested.GetProperty("Result").SetValue(handle, result)
        nested.GetProperty("VarNames").SetValue(handle, New String() {"Dose"})
        Dim cache = DirectCast(owner.GetField("_coxCache", BindingFlags.NonPublic Or BindingFlags.Static).GetValue(Nothing), System.Collections.IDictionary)
        Dim key = "COX:" & Guid.NewGuid().ToString("N")
        cache.Add(key, handle)
        Try
            Assert.AreEqual(ExcelError.ExcelErrorValue, CoxUDFs.COX_SUMMARY("", Nothing, -1))
            Assert.AreEqual(ExcelError.ExcelErrorNA, CoxUDFs.COX_SUMMARY("absent", Nothing, -1))
            Assert.AreEqual(ExcelError.ExcelErrorNum, CoxUDFs.COX_SUMMARY(key, Nothing, -1))
            Dim summary = DirectCast(CoxUDFs.COX_SUMMARY(key.ToUpperInvariant(), header, 0.1R), Object(,))
            Assert.AreEqual(If(header, 2, 1), summary.GetLength(0))
            Assert.AreEqual("Dose", summary(If(header, 1, 0), 0))
            Dim tests = DirectCast(CoxUDFs.COX_TESTS(key, header), Object(,))
            Assert.AreEqual(If(header, 7, 5), tests.GetLength(0))
            Assert.AreEqual("Likelihood-ratio chi-square", tests(If(header, 1, 0), 0))
            result.Coefficients = New Double() {}
            Assert.AreEqual("Invalid coefficient vector length.", CoxUDFs.COX_TESTS(key, header))
            Assert.AreEqual(True, CoxUDFs.COX_DROP(key))
            Assert.AreEqual(ExcelError.ExcelErrorNA, CoxUDFs.COX_SUMMARY(key))
        Finally
            If cache.Contains(key) Then cache.Remove(key)
        End Try
    End Sub

    <DataTestMethod, DataRow(""), DataRow("sk-SK")>
    Public Sub Continuation15GeeSummaryTestsKeepNativeCacheAndInvariantLabels(culture As String)
        Dim previous = CultureInfo.CurrentCulture
        Dim owner = GetType(GEEUDFs), nested = owner.GetNestedType("GeeHandle", BindingFlags.NonPublic)
        Dim handle = Activator.CreateInstance(nested, True)
        Dim model As New Global.BESHStatNG.GEE(Nothing, Nothing, Nothing)
        model.results = New Global.BESHStatNG.LMresult With {
            .Coeffs_est = New Double() {1, 2}, .Coeffs_SEs = New Double() {1, 1},
            .ModelTableLabels = New String() {"QIC"}, .ModelTableVals = New Object(,) {{12.0R}}}
        nested.GetProperty("Model").SetValue(handle, model)
        nested.GetProperty("VarNames").SetValue(handle, New String() {"Dose"})
        nested.GetProperty("Alpha").SetValue(handle, 0.055R)
        Dim cache = DirectCast(owner.GetField("_geeCache", BindingFlags.NonPublic Or BindingFlags.Static).GetValue(Nothing), System.Collections.IDictionary)
        Dim key = "GEE:" & Guid.NewGuid().ToString("N")
        cache.Add(key, handle)
        Try
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(culture)
            Assert.AreEqual(ExcelError.ExcelErrorNA, GEEUDFs.GEE_SUMMARY("absent", Nothing, -1))
            Assert.AreEqual(ExcelError.ExcelErrorNum, GEEUDFs.GEE_SUMMARY(key, Nothing, -1))
            Dim summary = DirectCast(GEEUDFs.GEE_SUMMARY(key.ToUpperInvariant()), Object(,))
            Assert.AreEqual("94.5% CI Lower", summary(0, 6))
            Assert.AreEqual("Dose", summary(2, 0))
            Dim tests = DirectCast(GEEUDFs.GEE_TESTS(key), Object(,))
            Assert.AreEqual(3, tests.GetLength(0))
            Assert.AreEqual("QIC", tests(1, 0))
            Assert.AreEqual("Computational time (s)", tests(2, 0))
            Assert.AreEqual(0.0R, tests(2, 1))
        Finally
            CultureInfo.CurrentCulture = previous
            cache.Remove(key)
        End Try
    End Sub

    <DataTestMethod>
    <DataRow("GeeLsmEstimateComponent", "ProfileValues", "UdfGeeComponent38")>
    <DataRow("GeeLsmEstimateAtProfile", "TimeSpecified", "UdfGeeAtProfile38")>
    <DataRow("GeeLsmEstimateAtProfile", "TimeValue", "UdfGeeAtProfile38")>
    <DataRow("GeeLsmEstimateAtProfile", "ProfileValues", "UdfGeeAtProfile38")>
    Public Sub Continuation15FourExactMovedGeePropertiesUseActualInheritedAccessors(owner As String, name As String, portableOwner As String)
        Dim nested = GetType(GEEUDFs).GetNestedType(owner, BindingFlags.NonPublic)
        Dim first = Activator.CreateInstance(nested, True), second = Activator.CreateInstance(nested, True)
        Dim prop = nested.GetProperty(name)
        Assert.IsTrue(prop.DeclaringType.FullName.StartsWith("BESHStatNG.ExcelCommon." & portableOwner & "`1", StringComparison.Ordinal))
        If name = "ProfileValues" Then
            Assert.AreNotSame(prop.GetValue(first), prop.GetValue(second))
            Dim value = Activator.CreateInstance(prop.PropertyType)
            prop.SetValue(first, value)
            Assert.AreSame(value, prop.GetValue(first))
        ElseIf name = "TimeSpecified" Then
            Assert.AreEqual(False, prop.GetValue(first))
            prop.SetValue(first, True)
            Assert.AreEqual(True, prop.GetValue(first))
        Else
            Assert.IsTrue(Double.IsNaN(DirectCast(prop.GetValue(first), Double)))
            prop.SetValue(first, 2.5R)
            Assert.AreEqual(2.5R, prop.GetValue(first))
        End If
    End Sub

    <TestMethod>
    Public Sub Continuation15ExactCoxLambdaKeepsItsCompiledIdentityFunction()
        Dim parent = GetType(CoxUDFs).GetMethod("COX_RESID")
        Dim targets = Calls(parent).Where(Function(method) method.Name.StartsWith("_Lambda$__", StringComparison.Ordinal)).ToArray()
        Assert.AreEqual(1, targets.Length)
        Dim target = targets(0)
        Dim instance = If(target.IsStatic, Nothing, Activator.CreateInstance(target.DeclaringType, True))
        Assert.AreEqual(42, target.Invoke(instance, New Object() {42}))
    End Sub

    <DataTestMethod>
    <DataRow("BESHStatNG.WorksheetFunctions.CoxUDFs", "COX_FIT", "BESHStatNG.ExcelCommon.UdfCoxInputPreparation38")>
    <DataRow("BESHStatNG.WorksheetFunctions.CoxUDFs", "COX_SUMMARY", "BESHStatNG.ExcelCommon.UdfCoxGeeReportPreparation38")>
    <DataRow("BESHStatNG.WorksheetFunctions.CoxUDFs", "COX_TESTS", "BESHStatNG.ExcelCommon.UdfCoxGeeReportPreparation38")>
    <DataRow("BESHStatNG.WorksheetFunctions.CoxUDFs", "COX_RESID", "BESHStatNG.ExcelCommon.UdfCoxGeeReportPreparation38")>
    <DataRow("BESHStatNG.WorksheetFunctions.CoxUDFs", "COX_BASELINE", "BESHStatNG.ExcelCommon.UdfCoxGeeReportPreparation38")>
    <DataRow("BESHStatNG.WorksheetFunctions.GEEUDFs", "GEE_SUMMARY", "BESHStatNG.ExcelCommon.UdfCoxGeeReportPreparation38")>
    <DataRow("BESHStatNG.WorksheetFunctions.GEEUDFs", "GEE_TESTS", "BESHStatNG.ExcelCommon.UdfCoxGeeReportPreparation38")>
    <DataRow("BESHStatNG.WorksheetFunctions.GEEUDFs", "GEE_RESID", "BESHStatNG.ExcelCommon.UdfCoxGeeReportPreparation38")>
    <DataRow("BESHStatNG.WorksheetFunctions.GEEUDFs", "GEE_PRED", "BESHStatNG.ExcelCommon.UdfCoxGeeReportPreparation38")>
    <DataRow("BESHStatNG.WorksheetFunctions.GEEUDFs", "MaterializeRows", "BESHStatNG.ExcelCommon.UdfCoxGeeReportPreparation38")>
    Public Sub Continuation15ActualShippingConsumersReachCommonPreparation(owner As String, name As String, prepared As String)
        Dim method = GetType(CoxUDFs).Assembly.GetType(owner, True).GetMethod(name, BindingFlags.Public Or BindingFlags.NonPublic Or BindingFlags.Static)
        Assert.IsTrue(Calls(method).Any(Function(target) target.DeclaringType.FullName = prepared))
    End Sub

    <DataTestMethod, DataRow(""), DataRow("sk-SK")>
    Public Sub Continuation15LooseNumericImportKeepsNativeResetAndCulture(culture As String)
        Dim previous = CultureInfo.CurrentCulture
        Dim method = GetType(GEEUDFs).Assembly.GetType("BESHStatNG.UdfDataImport", True).
            GetMethod("TryGetLooseNumericVector", BindingFlags.NonPublic Or BindingFlags.Static)
        Try
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(culture)
            For Each input As Object In New Object() {ExcelMissing.Value, ExcelEmpty.Value, ExcelError.ExcelErrorValue, "", "1;bad", "NaN", New Object(,) {{1, 2}, {3, 4}}}
                Dim rejected As Object() = {input, New Double() {99}}
                Assert.AreEqual(False, method.Invoke(Nothing, rejected))
                Assert.IsNull(rejected(1))
            Next
            Dim accepted As Object() = {"1,5;2.5", New Double() {99}}
            Assert.AreEqual(True, method.Invoke(Nothing, accepted))
            CollectionAssert.AreEqual(New Double() {1, 5, 2.5}, DirectCast(accepted(1), Double()))
            Assert.IsTrue(Calls(method).Any(Function(target) target.DeclaringType Is GetType(UdfModelImportPreparation38) AndAlso target.Name = "TryGetLooseNumericTextPrepared"))
        Finally
            CultureInfo.CurrentCulture = previous
        End Try
    End Sub

    <DataTestMethod, DataRow(""), DataRow("sk-SK")>
    Public Sub Continuation15CoxBaselineTextPolicyUsesWindowsWidthAndCase(culture As String)
        Dim previous = CultureInfo.CurrentCulture
        Try
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(culture)
            Dim table = UdfCoxGeeReportPreparation38.CreateCoxBaselineTable(2, "ｓｕｒｖｉｖａｌ")
            Dim row As Integer = 1
            UdfCoxGeeReportPreparation38.AppendCoxBaselineRows(table, New Double(,) {{7, 0.8, 0.2}}, "stratum", "ｓｕｒｖｉｖａｌ", row)
            Assert.AreEqual(3, table.GetLength(1))
            Assert.AreEqual(0.8R, table(1, 2))
        Finally
            CultureInfo.CurrentCulture = previous
        End Try
    End Sub


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


    Private NotInheritable Class CovarianceConversionProbe17
        Public Calls As Integer
        Public First As String
        Public Second As String
        Public FailureAt As Integer
        Public Failure As Exception
        Public ReadOnly Cultures As New List(Of String)()
        Public Overrides Function ToString() As String
            Calls += 1
            Cultures.Add(CultureInfo.CurrentCulture.Name)
            If Calls = FailureAt Then Throw Failure
            Return If(Calls = 1, First, Second)
        End Function
    End Class

    Private Shared Function NativeCovariance17(value As Object, fallback As String) As String
        Dim owner = GetType(UdfLinearAlgebra).Assembly.GetType("BESHStatNG.UdfScalarUtilities", True)
        Dim method = owner.GetMethod("NormalizeMixedModelResidualCovarianceName", BindingFlags.NonPublic Or BindingFlags.Static)
        Return DirectCast(method.Invoke(Nothing, New Object() {value, fallback}), String)
    End Function

    <DataTestMethod>
    <DataRow("", "default")>
    <DataRow("   ", "default")>
    <DataRow("identity", "Identity")>
    <DataRow(" hetero_geneous AR(1) ", "Heterogeneous AR(1)")>
    <DataRow("toep-h", "Heterogeneous Toeplitz (TOEPH)")>
    <DataRow("unstructured", "UN")>
    Public Sub Covariance17KnownOrBlankConvertsExactlyOnce(first As String, expected As String)
        Dim probe As New CovarianceConversionProbe17 With {.First = first, .Second = "wrong", .FailureAt = 2, .Failure = New ApplicationException("second conversion must not run")}
        Assert.AreEqual(expected, NativeCovariance17(probe, "default"))
        Assert.AreEqual(1, probe.Calls)
    End Sub

    <TestMethod>
    Public Sub Covariance17UnknownReevaluatesAndReturnsSecondTrimmedValueWithoutRemapping()
        Dim probe As New CovarianceConversionProbe17 With {.First = "unknown-token", .Second = "  HCS  "}
        Assert.AreEqual("HCS", NativeCovariance17(probe, "default"))
        Assert.AreEqual(2, probe.Calls)
    End Sub

    <DataTestMethod>
    <DataRow(1)>
    <DataRow(2)>
    Public Sub Covariance17ConversionExceptionPropagatesFromTheOriginalStage(stage As Integer)
        Dim failure As New ApplicationException("covariance-conversion-stage")
        Dim probe As New CovarianceConversionProbe17 With {.First = "unknown-token", .Second = "unused", .FailureAt = stage, .Failure = failure}
        Dim caught As TargetInvocationException = Assert.ThrowsException(Of TargetInvocationException)(Sub() NativeCovariance17(probe, "default"))
        Assert.AreSame(failure, caught.InnerException)
        Assert.AreEqual(stage, probe.Calls)
    End Sub

    <TestMethod>
    Public Sub Covariance17MissingNullAndExcelErrorKeepHostConversionPolicy()
        For Each missing As Object In New Object() {Nothing, ExcelMissing.Value, ExcelEmpty.Value}
            Assert.AreEqual("fallback", NativeCovariance17(missing, "fallback"))
            Assert.IsNull(NativeCovariance17(missing, Nothing))
        Next
        Assert.AreEqual(Convert.ToString(ExcelError.ExcelErrorNA).Trim(), NativeCovariance17(ExcelError.ExcelErrorNA, "fallback"))
    End Sub

    <DataTestMethod>
    <DataRow("")>
    <DataRow("sk-SK")>
    Public Sub Covariance17BothConversionsObserveTheNativeCurrentCulture(cultureName As String)
        Dim prior = CultureInfo.CurrentCulture
        Try
            CultureInfo.CurrentCulture = If(cultureName = "", CultureInfo.InvariantCulture, CultureInfo.GetCultureInfo(cultureName))
            Dim probe As New CovarianceConversionProbe17 With {.First = "unknown", .Second = " final "}
            Assert.AreEqual("final", NativeCovariance17(probe, "UN"))
            CollectionAssert.AreEqual(New String() {cultureName, cultureName}, probe.Cultures.ToArray())
        Finally
            CultureInfo.CurrentCulture = prior
        End Try
    End Sub

    <TestMethod>
    Public Sub Mmrm17NoInterceptRetainsOriginalArrayAliasAndNullPrecedence()
        Dim native = GetType(Global.BESHStatNG.WorksheetFunctions.MixedModelUDFs).GetMethod("AddInterceptIfRequested", System.Reflection.BindingFlags.NonPublic Or System.Reflection.BindingFlags.Static)
        Assert.IsNotNull(native)
        Dim data(,) As Double = {{1.0, 2.0}, {3.0, 4.0}}
        Assert.AreSame(data, native.Invoke(Nothing, New Object() {data, False}))
        Dim withIntercept = DirectCast(native.Invoke(Nothing, New Object() {data, True}), Double(,))
        Assert.AreNotSame(data, withIntercept)
        Assert.AreEqual(3, withIntercept.GetLength(1))
        Assert.AreEqual(1.0, withIntercept(0, 0))
        For Each includeIntercept As Boolean In New Boolean() {False, True}
            Dim ex = Assert.ThrowsException(Of System.Reflection.TargetInvocationException)(Sub() native.Invoke(Nothing, New Object() {Nothing, includeIntercept}))
            Assert.IsInstanceOfType(ex.InnerException, GetType(ArgumentNullException))
            Assert.AreEqual("x", DirectCast(ex.InnerException, ArgumentNullException).ParamName)
        Next
    End Sub

End Class
