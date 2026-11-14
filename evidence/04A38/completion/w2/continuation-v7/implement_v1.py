"""Prepared copying and covariance orchestration only; preserve native policies."""
from common import *

plan = json.loads((HERE/'implementation-plan-v1.json').read_bytes())
for path in ['BESHStat.Core/src/WorksheetFunctions/UdfNumericPreparation38.vb','BESHStat.Core.Tests/src/UdfNumericPreparation38Tests.vb']:
    target=HERE/'source-before'/(path+'.txt');target.parent.mkdir(parents=True,exist_ok=True)
    with target.open('xb') as stream:stream.write((ROOT/path).read_bytes())
    plan['source_before'].append(dict(live=pin(ROOT/path),preimage=pin(target)))
save('implementation-plan-v2.json',plan)
for row in plan['source_before']:assert pin(ROOT/row['live']['path'])==row['live']

def edit(path, before, after):
    source=(ROOT/path).read_text(encoding='utf-8-sig')
    assert source.count(before)==1,(path,before[:90])
    write_source(path,source.replace(before,after))

def append_class(path,code):
    source=(ROOT/path).read_text(encoding='utf-8-sig')
    index=source.rindex('    End Class')
    write_source(path,source[:index]+code+'\n'+source[index:])

numeric='BESHStat.Excel.Common/src/Analysis/UdfNumericImportPreparation38.vb'
append_class(numeric,'''        ' Prepared phases: acquisition, resets and lazy header conversions stay in Windows.
        Public Shared Sub CopyTrimmedColumnPrepared(source As Object(,), startRow As Integer, rows As Integer,
                                                    ByRef column As Object(,))
            ReDim column(rows - 1, 0)
            For i As Integer = 0 To rows - 1
                column(i, 0) = source(startRow + i, 0)
            Next
        End Sub

        Public Shared Sub CopyTrimmedMatrixPrepared(source As Object(,), startRow As Integer, rows As Integer, columns As Integer,
                                                    ByRef matrix As Object(,))
            ReDim matrix(rows - 1, columns - 1)
            For i As Integer = 0 To rows - 1
                For j As Integer = 0 To columns - 1
                    matrix(i, j) = source(startRow + i, j)
                Next
            Next
        End Sub
''')
readers='BESHstatNG/src/ExcelUDFs/UDFdataImport/UdfDataImportRangeReaders.vb'
edit(readers,'''        ReDim col(rows - 1, 0)
        For i As Integer = 0 To rows - 1
            col(i, 0) = arr(startRow + i, 0)
        Next''','''        Global.BESHStatNG.ExcelCommon.UdfNumericImportPreparation38.CopyTrimmedColumnPrepared(arr, startRow, rows, col)''')
edit(readers,'''        ReDim mat(rows - 1, cols - 1)
        For i As Integer = 0 To rows - 1
            For j As Integer = 0 To cols - 1
                mat(i, j) = arr(startRow + i, j)
            Next
        Next''','''        Global.BESHStatNG.ExcelCommon.UdfNumericImportPreparation38.CopyTrimmedMatrixPrepared(arr, startRow, rows, cols, mat)''')

assumptions='BESHstatNG/src/ExcelUDFs/AssumptionsUDFs.vb'
before='''                Dim g As Integer = groupMats.Count
                Dim p As Integer = groupMats(0).GetLength(1)
                Dim covCube(g - 1, p - 1, p - 1) As Double
                Dim sampleSizes(g - 1) As Integer

                For i As Integer = 0 To g - 1
                    Dim mat = groupMats(i)
                    Dim n As Integer = mat.GetLength(0)
                    If n < 2 Then Return ExcelError.ExcelErrorNum
                    sampleSizes(i) = n
                    Dim cov = Matrix.MatCovar(mat)
                    For r As Integer = 0 To p - 1
                        For c As Integer = 0 To p - 1
                            covCube(i, r, c) = cov(r, c)
                        Next
                    Next
                Next'''
core='BESHStat.Core/src/WorksheetFunctions/UdfNumericPreparation38.vb'
body=before.replace('                ','            ').replace('Dim covCube(g - 1, p - 1, p - 1) As Double','ReDim covCube(g - 1, p - 1, p - 1)').replace('Dim sampleSizes(g - 1) As Integer','ReDim sampleSizes(g - 1)').replace('Return ExcelError.ExcelErrorNum','Return False').replace('Matrix.MatCovar(mat)','Global.BESHStatNG.Matrix.MatrixStatisticsCore.SampleCovariance(mat)')
append_class(core,'''        ' Original BOX_M preparation; the existing Core covariance engine is unchanged.
        Public Shared Function TryBuildBoxCovariances(groupMats As List(Of Double(,)),
                                                      ByRef covCube As Double(,,), ByRef sampleSizes As Integer()) As Boolean
'''+body+'''
            Return True
        End Function
''')
edit(assumptions,before,'''                Dim covCube(,,) As Double = Nothing
                Dim sampleSizes() As Integer = Nothing
                If Not Global.BESHStatNG.WorksheetCompatibility.UdfNumericPreparation38.TryBuildBoxCovariances(groupMats, covCube, sampleSizes) Then Return ExcelError.ExcelErrorNum''')

tests='BESHStat.Excel.Common.Tests/src/UdfPreparation38Tests.vb'
source=(ROOT/tests).read_text(encoding='utf-8-sig');assert source.count('\nEnd Class')==1
new='''    <TestMethod>
    Public Sub TrimmedCopy38PreservesCellsAliasingAndFailureAssignments()
        Dim token As New Object()
        Dim source As Object(,) = {{"header", "other"}, {token, Double.NaN}, {Nothing, Double.PositiveInfinity}}
        Dim matrix As Object(,) = Nothing
        UdfNumericImportPreparation38.CopyTrimmedMatrixPrepared(source, 1, 2, 2, matrix)
        Assert.AreEqual(2, matrix.GetLength(0))
        Assert.AreSame(token, matrix(0, 0))
        Assert.IsTrue(Double.IsNaN(DirectCast(matrix(0, 1), Double)))
        Assert.IsNull(matrix(1, 0))
        Assert.AreEqual(Double.PositiveInfinity, DirectCast(matrix(1, 1), Double))
        Dim aliasSource As Object(,) = {{1.0R}, {2.0R}, {3.0R}}
        UdfNumericImportPreparation38.CopyTrimmedColumnPrepared(aliasSource, 1, 2, aliasSource)
        Assert.AreEqual(2.0R, aliasSource(0, 0))
        Assert.AreEqual(3.0R, aliasSource(1, 0))
        Dim lower = DirectCast(Array.CreateInstance(GetType(Object), {2, 1}, {1, 1}), Object(,))
        matrix = New Object(,) {{"sentinel"}}
        Assert.ThrowsException(Of IndexOutOfRangeException)(Sub() UdfNumericImportPreparation38.CopyTrimmedColumnPrepared(lower, 0, 2, matrix))
        Assert.AreEqual(2, matrix.GetLength(0))
        Assert.IsNull(matrix(0, 0))
        matrix = Nothing
        Assert.ThrowsException(Of IndexOutOfRangeException)(Sub() UdfNumericImportPreparation38.CopyTrimmedMatrixPrepared(source, 1, 3, 2, matrix))
        Assert.AreEqual(3, matrix.GetLength(0))
        Assert.AreSame(token, matrix(0, 0))
        Assert.AreEqual(Double.PositiveInfinity, DirectCast(matrix(1, 1), Double))
    End Sub

'''
write_source(tests,source.replace('\nEnd Class','\n'+new+'End Class'))

tests='BESHStat.Core.Tests/src/UdfNumericPreparation38Tests.vb'
source=(ROOT/tests).read_text(encoding='utf-8-sig');assert source.count('\nEnd Class')==1
new='''    <TestMethod>
    Public Sub BoxPreparation38RetainsCovariancesOrderAndEarlyFailure()
        Dim groups As New System.Collections.Generic.List(Of Double(,)) From {
            New Double(,) {{1.0R, 2.0R}, {3.0R, 6.0R}, {5.0R, 4.0R}},
            New Double(,) {{2.0R, 1.0R}, {6.0R, 5.0R}}}
        Dim cube(,,) As Double = Nothing, sizes() As Integer = Nothing
        Assert.IsTrue(UdfNumericPreparation38.TryBuildBoxCovariances(groups, cube, sizes))
        CollectionAssert.AreEqual(New Integer() {3, 2}, sizes)
        Assert.AreEqual(4.0R, cube(0, 0, 0))
        Assert.AreEqual(2.0R, cube(0, 0, 1))
        Assert.AreEqual(8.0R, cube(1, 1, 1))
        Assert.AreEqual(1.0R, groups(0)(0, 0))
        groups(1) = New Double(,) {{99.0R, 88.0R}}
        Assert.IsFalse(UdfNumericPreparation38.TryBuildBoxCovariances(groups, cube, sizes))
        CollectionAssert.AreEqual(New Integer() {3, 0}, sizes)
        Assert.AreEqual(4.0R, cube(0, 0, 0))
        Assert.AreEqual(0.0R, cube(1, 0, 0))
        groups(1) = New Double(,) {{1.0R}, {2.0R}}
        Assert.ThrowsException(Of IndexOutOfRangeException)(Sub() UdfNumericPreparation38.TryBuildBoxCovariances(groups, cube, sizes))
        CollectionAssert.AreEqual(New Integer() {3, 2}, sizes)
        Assert.AreEqual(0.5R, cube(1, 0, 0))
    End Sub

'''
write_source(tests,source.replace('\nEnd Class','\n'+new+'End Class'))

tests='BESHStatNG.Test/src/UDFs/UdfPreparation38AdapterTests.vb'
source=(ROOT/tests).read_text(encoding='utf-8-sig');assert source.count('\nEnd Class')==1
new='''    <DataTestMethod>
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

'''
write_source(tests,source.replace('\nEnd Class','\n'+new+'End Class'))
save('implementation-execution-v1.json',dict(status='PREPARED_COPY_AND_BOX_ORCHESTRATION_WRITTEN',reader=pin(HERE/'implement_v1.py'),sources=[pin(ROOT/r['live']['path']) for r in plan['source_before']],original_queries=0))
print('C7 coherent A/B/C preparation packet written; existing numerical engines unchanged')
