Option Explicit On
Option Strict On
Imports System
Imports System.Collections.Generic
Imports System.Globalization
Imports System.Linq
Imports System.Security.Cryptography
Imports System.Text
Imports System.Xml.Linq
Imports BESHStatNG
Imports BESHStatNG.ExcelCommon
Imports BESHStatNG.Multivariate
Imports Microsoft.VisualStudio.TestTools.UnitTesting

Public NotInheritable Class Multivariate18Fixture
    Public NotInheritable Class MemoryWriter
        Inherits ResultTableWriterBase
        Public ReadOnly Blocks As New List(Of ResultTableOutputBlock)
        Public Sub New(Optional row As Integer = 1, Optional column As Integer = 1)
            MyBase.New(row, column)
        End Sub
        Protected Overrides Sub WriteOutputBlock(block As ResultTableOutputBlock)
            Blocks.Add(block)
        End Sub
    End Class
    Public Shared Function Acquired(raw(,) As Object, names() As String, Optional rows() As Integer = Nothing) As DataObj
        Return New DataObj With {.FinalData = raw, .varNames = names, .nRows = raw.GetLength(0), .nCols = raw.GetLength(1),
            .RowIds = If(rows, Enumerable.Range(0, raw.GetLength(0)).Select(Function(i) 101 + 3 * i).ToArray())}
    End Function
    Public Shared Function PcaData(caseNumber As Integer) As DataObj
        Dim family = caseNumber \ 24, n = {12, 9, 16}(family), p = {3, 2, 4}(family), raw(n - 1, p - 1) As Object
        For i = 0 To n - 1
            For j = 0 To p - 1
                If family = 2 Then
                    Dim angle = (i Mod 8) * Math.PI / 4
                    raw(i, j) = If(j Mod 2 = 0, Math.Cos(angle * (1 + j \ 2)), Math.Sin(angle * (1 + j \ 2))) * If(j = 3, 1.0000000001, 1.0)
                Else
                    raw(i, j) = (i + 1) * (j + 0.5) + Math.Sin((i + 1) * (j + 1) * 0.7)
                End If
            Next
        Next
        Dim names = If(family = 2, New String() {"X", "X", "μ", "A&B"}, Enumerable.Range(0, p).Select(Function(j) "x" & j).ToArray())
        Dim rows = Enumerable.Range(0, n).Select(Function(i) 101 + i * 3).ToArray()
        If caseNumber Mod 2 = 1 Then Array.Reverse(rows)
        Dim data = Acquired(raw, names, rows)
        If family = 1 Then
            Dim keep As New Dictionary(Of Integer, Integer)
            For i = 0 To n - 1
                If i Mod 3 <> 1 Then keep.Add(i, rows(i))
            Next
            data.SubsetByRowIdValues(keep)
        End If
        Return data
    End Function
    Public Shared Function PcaOptions(caseNumber As Integer) As MultivariatePcaOptions
        Dim family = caseNumber \ 24, mode = (caseNumber \ 8) Mod 3
        Return New MultivariatePcaOptions((caseNumber \ 4) Mod 2 = 0, mode = 0, mode = 1, mode = 2,
            If(family = 1, 1D, 0.4D), If(family = 2, 3D, If(family = 1, 1D, 2D)), 80D)
    End Function
    Public Shared Function RowLabels(data As DataObj, caseNumber As Integer) As String()
        If caseNumber Mod 4 <> 1 AndAlso caseNumber Mod 4 <> 3 Then Return Nothing
        Return Enumerable.Range(0, data.nRows).Select(Function(i) MultivariateWorkflowPolicy.PcaAuxiliaryLabel(If(i Mod 3 = 0, " ", " dup "), data.RowIds(i), True)).ToArray()
    End Function
    Public Shared Function Groups(data As DataObj, caseNumber As Integer) As String()
        If caseNumber Mod 4 <> 2 AndAlso caseNumber Mod 4 <> 3 Then Return Nothing
        Return Enumerable.Range(0, data.nRows).Select(Function(i) MultivariateWorkflowPolicy.PcaAuxiliaryLabel(If(i Mod 3 = 0, "", If(i Mod 2 = 0, "a", "A")), data.RowIds(i), False)).ToArray()
    End Function
    Public Shared Function McaData(caseNumber As Integer) As DataObj
        Dim q = 1 + (caseNumber \ 2) Mod 3, raw(23, q - 1) As Object
        For i = 0 To 23
            For j = 0 To q - 1
                Dim level = (i + j) Mod (2 + j Mod 2)
                raw(i, j) = If(caseNumber >= 12, If(level = 0, " α ", If(i Mod 5 = 0, Nothing, "β")), If(level = 0, "same", "z" & level))
            Next
        Next
        If caseNumber = 0 OrElse caseNumber = 10 Then
            For i = 0 To 23
                For j = 0 To q - 1
                    raw(i, j) = "only"
                Next
            Next
        End If
        Dim names = Enumerable.Range(0, q).Select(Function(j) If(caseNumber = 16, "duplicate", "variable" & j)).ToArray()
        Dim data = Acquired(raw, names)
        If caseNumber Mod 2 = 1 Then
            Dim keep As New Dictionary(Of Integer, Integer)
            For i = 1 To data.nRows - 1
                keep.Add(i, data.RowIds(i))
            Next
            data.SubsetByRowIdValues(keep)
        End If
        Return data
    End Function
    Public Shared Function CaData(caseNumber As Integer) As DataObj
        Dim r = If(caseNumber = 8, 2, If(caseNumber = 9, 1, 3)), c = If(caseNumber = 8, 2, 3), raw(r - 1, c) As Object
        For i = 0 To r - 1
            raw(i, 0) = If(i = 0, "", "dup")
            For j = 1 To c
                raw(i, j) = CDbl(1 + ((i + 1) * (j + 3)) Mod 13)
            Next
        Next
        Select Case caseNumber
            Case 1 : raw(0, 1) = 1.5
            Case 2 : raw(0, 1) = "1,5"
            Case 3 : raw(0, 1) = Nothing
            Case 4 : raw(0, 1) = -20.0
            Case 5
                For i = 0 To r - 1
                    For j = 1 To c
                        raw(i, j) = 0.0
                    Next
                Next
            Case 6 : raw(0, 1) = Double.MaxValue
            Case 7 : raw(0, 1) = "bad"
            Case 10
                For j = 1 To c
                    raw(0, j) = 0.0
                Next
            Case 11 : raw(0, 1) = Double.NaN
        End Select
        Return Acquired(raw, (New String() {"label"}).Concat(Enumerable.Range(0, c).Select(Function(j) If(j < 2, "duplicate", "Ω"))).ToArray(), Enumerable.Range(0, r).Select(Function(i) 31 + i * i * 7).ToArray())
    End Function
    Public Shared Function Input(kind As MultivariateAnalysisKind, data As DataObj, Optional labels() As String = Nothing, Optional groups() As String = Nothing) As MultivariateInputIntent
        Return New MultivariateInputIntent(kind, data.nRows, data.nCols, data.varNames, data.RowIds, labels, groups,
            Enumerable.Range(0, data.nCols).Select(Function(j) "source-column-" & j).ToArray())
    End Function
    Public Shared Sub Consume(writer As ResultTableWriterBase, section As MultivariateResultSection, payload As Object, tall As Boolean)
        writer.setRowPointer(section.StartRow) : writer.setColumnPointer(section.StartColumn) : writer.write(payload, tall)
    End Sub
    Public Shared Sub Blocks(expected As MemoryWriter, actual As MemoryWriter)
        Assert.AreEqual(expected.RowID, actual.RowID) : Assert.AreEqual(expected.ColID, actual.ColID)
        Assert.AreEqual(expected.Blocks.Count, actual.Blocks.Count)
        For i = 0 To expected.Blocks.Count - 1
            Dim a = expected.Blocks(i), b = actual.Blocks(i)
            Assert.AreEqual(a.StartRow, b.StartRow) : Assert.AreEqual(a.StartColumn, b.StartColumn)
            Assert.AreEqual(a.Model.IsResultTable, b.Model.IsResultTable)
            Multivariate17Fixture.Cells(a.Model.Values, b.Model.Values)
            Assert.AreEqual(a.Model.TitleRows, b.Model.TitleRows) : Assert.AreEqual(a.Model.HeaderTopRows, b.Model.HeaderTopRows)
            Assert.AreEqual(a.Model.HeaderLeftColumns, b.Model.HeaderLeftColumns) : Assert.AreEqual(a.Model.FooterRows, b.Model.FooterRows)
            CollectionAssert.AreEqual(a.Model.PvalueColumns, b.Model.PvalueColumns) : CollectionAssert.AreEqual(a.Model.PvalueCells, b.Model.PvalueCells)
        Next
    End Sub
    Public Shared Function Digest(ParamArray arrays() As Array) As String
        Dim text As New StringBuilder
        For Each value In arrays
            If value Is Nothing Then
                text.Append("NULL;") : Continue For
            End If
            text.Append(value.GetType().FullName).Append(":"c)
            For d = 0 To value.Rank - 1
                text.Append(value.GetLength(d).ToString(CultureInfo.InvariantCulture)).Append(","c)
            Next
            For Each cell As Object In value
                If cell Is Nothing Then
                    text.Append("NULL;")
                Else
                    Dim s = If(TypeOf cell Is Double, CDbl(If(TypeOf cell Is Double, cell, 0.0)).ToString("R", CultureInfo.InvariantCulture), Convert.ToString(cell, CultureInfo.InvariantCulture))
                    text.Append(cell.GetType().FullName).Append(":"c).Append(s.Length.ToString(CultureInfo.InvariantCulture)).Append(":"c).Append(s).Append(";"c)
                End If
            Next
        Next
        Using hash = SHA256.Create()
            Return BitConverter.ToString(hash.ComputeHash(Encoding.UTF8.GetBytes(text.ToString()))).Replace("-", "").ToLowerInvariant()
        End Using
    End Function
    Public Shared Sub Evidence(family As String, caseNumber As Integer, data As DataObj, projection As MultivariateComponentProjection,
                               outcome As String, Optional failure As Exception = Nothing, Optional options As String = Nothing,
                               Optional numericalDigest As String = Nothing, Optional tables As List(Of ResultTable) = Nothing)
        Dim root As New XElement("profile", New XAttribute("family", family), New XAttribute("case", caseNumber),
            New XAttribute("culture", If(CultureInfo.CurrentCulture.Name, "")), New XAttribute("outcome", outcome),
            New XAttribute("dataset_sha256", Digest(data.FinalData, data.RowIds, data.varNames)),
            New XAttribute("raw_rows", data.nRows), New XAttribute("raw_columns", data.nCols), New XElement("options", options),
            New XElement("numerical_sha256", numericalDigest), New XElement("source_rows", data.RowIds.Select(Function(v) New XElement("row", v))))
        If failure IsNot Nothing Then root.Add(New XElement("exception", New XAttribute("type", failure.GetType().FullName), failure.Message))
        If projection IsNot Nothing Then
            root.Add(New XElement("axes", projection.Axes.Select(Function(a) New XElement("axis", New XAttribute("kind", a.Kind.ToString()),
                a.Items.Select(Function(i) New XElement("item", New XAttribute("index", i.ResultIndex), New XAttribute("state", i.State.ToString()),
                    If(i.SourcePosition.HasValue, New XAttribute("source_position", i.SourcePosition.Value), Nothing),
                    If(i.SourceRowId.HasValue, New XAttribute("source_row_id", i.SourceRowId.Value), Nothing),
                    If(i.Name IsNot Nothing, New XAttribute("name", i.Name), Nothing), If(i.DisplayLabel IsNot Nothing, New XAttribute("display", i.DisplayLabel), Nothing),
                    If(i.SourceKey IsNot Nothing, New XAttribute("source_key", i.SourceKey), Nothing)))))))
            root.Add(New XElement("sections", projection.Sections.Select(Function(v) New XElement("section", New XAttribute("role", v.Role.ToString()),
                New XAttribute("row", v.StartRow), New XAttribute("column", v.StartColumn), New XAttribute("rows", v.Shape.Rows), New XAttribute("columns", v.Shape.Columns),
                New XAttribute("available", v.Shape.Available), New XAttribute("row_axis", v.RowAxis.ToString()), New XAttribute("column_axis", v.ColumnAxis.ToString())))))
            root.Add(New XElement("plots", projection.PlotData.Select(Function(v) New XElement("plot", New XAttribute("kind", v.Request.Kind.ToString()),
                New XAttribute("scaling", v.Request.Scaling.ToString("R", CultureInfo.InvariantCulture)), New XAttribute("axis", v.Request.Axis), New XAttribute("row_contribution", v.Request.RowContribution),
                New XAttribute("row_points", v.RowPointShape.Rows), New XAttribute("row_dimensions", v.RowPointShape.Columns), New XAttribute("column_points", v.ColumnPointShape.Rows), New XAttribute("column_dimensions", v.ColumnPointShape.Columns), New XAttribute("display_rows", v.DisplayRows)))))
            If projection.BurtShape IsNot Nothing Then root.Add(New XElement("burt", New XAttribute("rows", projection.BurtShape.Rows), New XAttribute("columns", projection.BurtShape.Columns)))
        End If
        If tables IsNot Nothing Then
            Dim nonfinite = tables.Sum(Function(t) t.ToOutputModel().Values.Cast(Of Object)().Count(Function(v) TypeOf v Is Double AndAlso (Double.IsNaN(CDbl(v)) OrElse Double.IsInfinity(CDbl(v)))))
            root.Add(New XElement("tables", New XAttribute("count", tables.Count), New XAttribute("nonfinite_cells", nonfinite)))
        End If
        Console.WriteLine("04A18_XML|" & Convert.ToBase64String(Encoding.UTF8.GetBytes(root.ToString(SaveOptions.DisableFormatting))))
    End Sub
End Class

<TestClass>
Public Class Multivariate18Tests
    Public Shared Iterator Function Profiles() As IEnumerable(Of Object())
        For Each family In {"PCA", "MCA", "CA"}
            Dim count = If(family = "PCA", 72, If(family = "MCA", 18, 12))
            For i = 0 To count - 1
                Yield {family, i}
            Next
        Next
    End Function
    <DataTestMethod><DynamicData(NameOf(Profiles), DynamicDataSourceType.Method)>
    Public Sub SourceBackedAxesBlocksAndReports(family As String, caseNumber As Integer)
        Dim saved = CultureInfo.CurrentCulture
        Try
            For Each culture In {"", "sk-SK"}
                CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(culture)
                Select Case family
                    Case "PCA" : PcaProfile(caseNumber)
                    Case "MCA" : McaProfile(caseNumber)
                    Case "CA" : CaProfile(caseNumber)
                End Select
            Next
        Finally
            CultureInfo.CurrentCulture = saved
        End Try
    End Sub
    Private Shared Sub PcaProfile(caseNumber As Integer)
        Dim data = Multivariate18Fixture.PcaData(caseNumber), options = Multivariate18Fixture.PcaOptions(caseNumber)
        Dim labels = Multivariate18Fixture.RowLabels(data, caseNumber), groups = Multivariate18Fixture.Groups(data, caseNumber)
        Dim input = Multivariate18Fixture.Input(MultivariateAnalysisKind.PCA, data, labels, groups), model As New PCA
        model.dataInputs(data.DataDbl, input.SourceRowIds, input.VariableNames, options.Label)
        options.Configure(model, 1000, 0.000000000001)
        Try
            model.Calculate(options.CoreRetention, options.Value)
        Catch ex As Exception
            Multivariate18Fixture.Evidence("PCA", caseNumber, data, Nothing, "CORE_REJECTED", ex, options.MatrixType & ";" & options.CoreRetention & ";" & options.Value.ToString("R", CultureInfo.InvariantCulture)) : Return
        End Try
        Dim digest = Multivariate18Fixture.Digest(model.StandardizedData, model.ReducedDataset, model.GetLoadings, model.Eigenvalues, model.Eigenvectors)
        Dim projection = MultivariateComponentProjectionPolicy.Pca(input, model, options, data.FinalData)
        Dim before As New Multivariate18Fixture.MemoryWriter, after As New Multivariate18Fixture.MemoryWriter
        LegacyMultivariate18Layout.PcaData(before, data, model, labels, groups)
        MultivariateComponentProjectionPolicy.VisitPca(projection, data.FinalData, model, Sub(section, payload, tall) Multivariate18Fixture.Consume(after, section, payload, tall))
        Multivariate18Fixture.Blocks(before, after)
        Dim expected = model.wrapResults(), report = projection.ToReport(expected)
        Multivariate17Fixture.Tables(expected, report.LegacyTables())
        Assert.AreEqual(data.nRows, projection.Axis(MultivariateSemanticAxis.ScoreObservations).Items.Count)
        Assert.AreEqual(model.ReducedDataset.GetLength(1), projection.Axis(MultivariateSemanticAxis.RetainedComponents).Items.Count)
        Assert.AreEqual(data.nCols, projection.Axis(MultivariateSemanticAxis.LoadingVariables).Items.Count)
        Assert.AreEqual(model.Eigenvalues.Length, projection.Axis(MultivariateSemanticAxis.EigenComponents).Items.Count)
        CollectionAssert.AreEqual(data.RowIds, projection.Axis(MultivariateSemanticAxis.Observations).Items.Select(Function(i) i.SourceRowId.Value).ToArray())
        CollectionAssert.AreEqual(New Double() {0, 0.5, 1}, projection.PlotData.Where(Function(v) v.Request.Kind = MultivariatePlotKind.Biplot).Select(Function(v) v.Request.Scaling).ToArray())
        Assert.AreEqual(digest, Multivariate18Fixture.Digest(model.StandardizedData, model.ReducedDataset, model.GetLoadings, model.Eigenvalues, model.Eigenvectors))
        Multivariate18Fixture.Evidence("PCA", caseNumber, data, projection, "CORE_RETURNED", options:=options.MatrixType & ";" & options.CoreRetention & ";" & options.Value.ToString("R", CultureInfo.InvariantCulture) & ";" & options.Label, numericalDigest:=digest, tables:=expected)
    End Sub
    Private Shared Sub McaProfile(caseNumber As Integer)
        Dim data = Multivariate18Fixture.McaData(caseNumber), input = Multivariate18Fixture.Input(MultivariateAnalysisKind.MCA, data), model As New CA
        Try
            model.DataMultiple(Global.BESHStatNG.DataManagement.ArrayUtilities.Array2strArray(data.FinalData), input.VariableNames) : model.Calculate()
        Catch ex As Exception
            Multivariate18Fixture.Evidence("MCA", caseNumber, data, Nothing, "CORE_REJECTED", ex, "first_acquired_row_removed=" & (caseNumber Mod 2 = 1).ToString()) : Return
        End Try
        Dim digest = Multivariate18Fixture.Digest(model.DesignMatrix, model.BurtTable, model.rowNames, model.ColumNames, model.BurtVarNames)
        Dim projection = MultivariateComponentProjectionPolicy.Mca(New MultivariateMcaTransformIntent(input, caseNumber Mod 2 = 1), model, data.FinalData)
        Dim before As New Multivariate18Fixture.MemoryWriter, after As New Multivariate18Fixture.MemoryWriter
        LegacyMultivariate18Layout.McaData(before, data, model)
        MultivariateComponentProjectionPolicy.VisitMca(projection, data.FinalData, model, Sub(section, payload, tall) Multivariate18Fixture.Consume(after, section, payload, tall))
        Multivariate18Fixture.Blocks(before, after)
        Dim expected As List(Of ResultTable)
        Try
            expected = model.wrapResults()
        Catch ex As Exception
            Multivariate18Fixture.Evidence("MCA", caseNumber, data, projection, "CORE_REPORT_REJECTED", ex, numericalDigest:=digest) : Return
        End Try
        Multivariate17Fixture.Tables(expected, projection.ToReport(expected).LegacyTables())
        Assert.AreEqual(data.nRows, projection.Axis(MultivariateSemanticAxis.Observations).Items.Count)
        Assert.AreEqual(model.DesignMatrix.GetLength(1), projection.Axis(MultivariateSemanticAxis.Categories).Items.Count)
        Assert.AreEqual(model.BurtTable.GetLength(0), projection.BurtShape.Rows)
        CollectionAssert.AreEqual(model.rowNames, projection.Axis(MultivariateSemanticAxis.VariableCategoryBlocks).Items.Select(Function(i) i.DisplayLabel).ToArray())
        For Each item In projection.Axis(MultivariateSemanticAxis.Categories).Items
            Assert.IsFalse(item.SourceRowId.HasValue, "Category index is not a worksheet row.")
        Next
        If caseNumber = 16 AndAlso data.nCols > 1 Then
            Assert.IsTrue(projection.Axis(MultivariateSemanticAxis.Categories).Items.All(Function(i) i.State = MultivariateMappingState.Ambiguous AndAlso Not i.SourcePosition.HasValue))
        End If
        Assert.AreEqual(3, projection.PlotData.Count)
        Assert.IsFalse(projection.PlotData(0).DisplayRows)
        Assert.AreEqual(digest, Multivariate18Fixture.Digest(model.DesignMatrix, model.BurtTable, model.rowNames, model.ColumNames, model.BurtVarNames))
        Multivariate18Fixture.Evidence("MCA", caseNumber, data, projection, "CORE_RETURNED", options:="first_acquired_row_removed=" & (caseNumber Mod 2 = 1).ToString(), numericalDigest:=digest, tables:=expected)
    End Sub
    Private Shared Sub CaProfile(caseNumber As Integer)
        Dim data = Multivariate18Fixture.CaData(caseNumber), counts(,) As Integer
        Try
            counts = LegacyMultivariate18Layout.LabelledCounts(data)
        Catch ex As Exception
            Multivariate18Fixture.Evidence("CA", caseNumber, data, Nothing, "HOST_CINT_REJECTED", ex) : Return
        End Try
        Dim labels = Global.BESHStatNG.DataManagement.ArrayUtilities.Array2strArray(Global.BESHStatNG.DataManagement.ArrayUtilities.GetColumn(data.FinalData, 0))
        Dim input = MultivariateCorrespondenceInputPolicy.Labelled(data.nRows, data.nCols, data.varNames, data.RowIds, labels), model As New CA
        Try
            model.data(counts, labels, data.varNames.Skip(1).ToArray()) : model.Calculate()
        Catch ex As Exception
            Multivariate18Fixture.Evidence("CA", caseNumber, data, Nothing, "CORE_REJECTED", ex) : Return
        End Try
        Dim expected As List(Of ResultTable)
        Try
            expected = model.wrapResults()
        Catch ex As Exception
            Multivariate18Fixture.Evidence("CA", caseNumber, data, Nothing, "CORE_REPORT_REJECTED", ex) : Return
        End Try
        Dim projection = MultivariateComponentProjectionPolicy.SimpleCa(input, model, counts)
        Dim prefix As New ResultTable : prefix.AddTitle("Analyzed contingency table") : prefix.SetBody(counts)
        prefix.AddHeaderLeftRow(model.rowNames) : prefix.AddHeaderTopRow(model.ColumNames)
        Dim allExpected As New List(Of ResultTable)(expected) : allExpected.Insert(0, prefix)
        Dim originalCount = expected.Count, actual = MultivariateComponentProjectionPolicy.CaReport(projection, counts, expected)
        Multivariate17Fixture.Tables(allExpected, actual.LegacyTables())
        Multivariate17Fixture.Tables(allExpected, MultivariateComponentProjectionPolicy.CaReport(projection, counts, expected).LegacyTables())
        Assert.AreEqual(originalCount, expected.Count)
        CollectionAssert.AreEqual(data.RowIds, projection.Axis(MultivariateSemanticAxis.ContingencyRows).Items.Select(Function(i) i.SourceRowId.Value).ToArray())
        Assert.AreEqual(1, projection.Axis(MultivariateSemanticAxis.ContingencyColumns).Items(0).SourcePosition.Value)
        Assert.AreEqual(5, projection.PlotData.Count)
        Multivariate18Fixture.Evidence("CA", caseNumber, data, projection, "CORE_RETURNED", numericalDigest:=Multivariate18Fixture.Digest(counts, model.Eigenvalues), tables:=allExpected)
    End Sub
    <TestMethod>
    Public Sub MetadataCopiesBorrowedPayloadsAndRepeatedProjectionStaySeparate()
        Dim data = Multivariate18Fixture.PcaData(35), labels = Multivariate18Fixture.RowLabels(data, 35), groups = Multivariate18Fixture.Groups(data, 35)
        Dim input = Multivariate18Fixture.Input(MultivariateAnalysisKind.PCA, data, labels, groups), model As New PCA, options = Multivariate18Fixture.PcaOptions(35)
        model.dataInputs(data.DataDbl, input.SourceRowIds, input.VariableNames, options.Label) : options.Configure(model, 1000, 0.000000000001) : model.Calculate(options.CoreRetention, options.Value)
        Dim projection = MultivariateComponentProjectionPolicy.Pca(input, model, options, data.FinalData)
        Dim oldName = projection.Sections.First(Function(s) s.Role = MultivariateSectionRole.OriginalHeader).Header(0)
        data.varNames(0) = "changed" : data.RowIds(0) = -1 : labels(0) = "changed" : groups(0) = "changed"
        Dim names = projection.Sections.First(Function(s) s.Role = MultivariateSectionRole.OriginalHeader).Header
        names(0) = "mutated getter"
        Assert.AreEqual(oldName, projection.Sections.First(Function(s) s.Role = MultivariateSectionRole.OriginalHeader).Header(0))
        Assert.AreNotEqual(-1, projection.Axis(MultivariateSemanticAxis.Observations).Items(0).SourceRowId.Value)
        Assert.AreNotEqual("changed", projection.Input.RowLabels(0)) : Assert.AreNotEqual("changed", projection.Input.GroupLabels(0))
        Dim seen As New HashSet(Of MultivariateSectionRole)
        MultivariateComponentProjectionPolicy.VisitPca(projection, data.FinalData, model,
            Sub(section, payload, tall)
                seen.Add(section.Role)
                If section.Role = MultivariateSectionRole.OriginalData Then Assert.AreSame(data.FinalData, payload)
                If section.Role = MultivariateSectionRole.StandardizedData Then Assert.AreSame(model.StandardizedData, payload)
                If section.Role = MultivariateSectionRole.ReducedScores Then Assert.AreSame(model.ReducedDataset, payload)
            End Sub)
        Assert.AreEqual(projection.Sections.Count, seen.Count)
        Assert.ThrowsException(Of NotSupportedException)(Sub() DirectCast(projection.Sections, IList(Of MultivariateResultSection)).Clear())
        Assert.ThrowsException(Of NotSupportedException)(Sub() DirectCast(projection.Axis(MultivariateSemanticAxis.Observations).Items, IList(Of MultivariateAxisItem)).Clear())
        Dim noRows As New MultivariateInputIntent(MultivariateAnalysisKind.PCA, data.nRows, data.nCols, input.VariableNames)
        Dim absent = MultivariateComponentProjectionPolicy.Pca(noRows, model, options, data.FinalData)
        Assert.IsTrue(absent.Axis(MultivariateSemanticAxis.ScoreObservations).Items.All(Function(i) Not i.SourceRowId.HasValue))
    End Sub
    <TestMethod>
    Public Sub PartialCoreAndLargeShapesStayMetadataOnlyAndCandidateOnly()
        Dim input As New MultivariateInputIntent(MultivariateAnalysisKind.PCA, Integer.MaxValue, Integer.MaxValue, {"x"}, {}, {}, {})
        Dim partialProjection = MultivariateComponentProjectionPolicy.Pca(input, New PCA, Nothing, Nothing)
        Assert.IsTrue(partialProjection.Validation.Diagnostics.Count <= 32)
        Assert.IsTrue(partialProjection.Validation.Diagnostics.All(Function(d) Not d.Blocking))
        Assert.AreEqual(0, partialProjection.Axis(MultivariateSemanticAxis.ScoreObservations).Items.Count)
        Assert.IsFalse(partialProjection.Sections.First(Function(s) s.Role = MultivariateSectionRole.ReducedScores).Shape.Available)
        Dim raw(2047, 15) As Object
        For i = 0 To 2047
            For j = 0 To 15
                raw(i, j) = Math.Sin((i + 1) * (j + 1) * 0.017) + i * 0.001
            Next
        Next
        Dim data = Multivariate18Fixture.Acquired(raw, Enumerable.Range(0, 16).Select(Function(i) "x" & i).ToArray()), model As New PCA
        model.dataInputs(data.DataDbl, data.RowIds, data.varNames) : model.settingsInputs(1000, 0.00000001, "Correlation") : model.Calculate("Fixed", 3)
        Dim projection = MultivariateComponentProjectionPolicy.Pca(Multivariate18Fixture.Input(MultivariateAnalysisKind.PCA, data), model, Nothing, raw)
        Assert.AreEqual(2048, projection.Axis(MultivariateSemanticAxis.ScoreObservations).Items.Count)
        Assert.AreEqual(16, projection.Axis(MultivariateSemanticAxis.LoadingVariables).Items.Count)
        For Each field In GetType(MultivariateComponentProjection).GetFields(Reflection.BindingFlags.NonPublic Or Reflection.BindingFlags.Instance)
            Assert.IsFalse(GetType(PCA).IsAssignableFrom(field.FieldType)) : Assert.IsFalse(GetType(CA).IsAssignableFrom(field.FieldType))
            Assert.IsFalse(field.FieldType.IsArray AndAlso field.FieldType.GetArrayRank() > 1)
            Assert.IsFalse(GetType(DataObj).IsAssignableFrom(field.FieldType))
            Assert.IsFalse(GetType([Delegate]).IsAssignableFrom(field.FieldType))
        Next
    End Sub
    <DataTestMethod><DataRow(0)><DataRow(1)><DataRow(2)><DataRow(3)>
    Public Sub UnlabelledFailureAndDormantIdentityMappingAreDifferent(caseNumber As Integer)
        Dim raw As Object(,) = {{2.0, 3.0, 5.0}, {7.0, 11.0, 13.0}, {17.0, 19.0, 23.0}}
        If caseNumber = 1 Then raw(0, 0) = Nothing
        If caseNumber = 2 Then raw(0, 0) = "bad"
        If caseNumber = 3 Then raw(0, 0) = Double.MaxValue
        Dim data = Multivariate18Fixture.Acquired(raw, {"a", "b", "c"}), legacyError As Exception = Nothing
        Try : LegacyMultivariate18Layout.LegacyUnlabelledCounts(data) : Catch ex As Exception : legacyError = ex : End Try
        Assert.IsNotNull(legacyError)
        If caseNumber < 2 Then Assert.IsInstanceOfType(legacyError, GetType(IndexOutOfRangeException))
        Dim dormant = MultivariateCorrespondenceInputPolicy.DormantUnlabelled(data.nRows, data.nCols, data.varNames, data.RowIds)
        Assert.IsTrue(dormant.CandidateOnly) : Assert.IsTrue(dormant.Validation.Diagnostics.All(Function(d) Not d.Blocking))
        Assert.AreEqual(data.nCols, dormant.CountColumnCount)
        For j = 0 To data.nCols - 1
            Assert.AreEqual(j, dormant.ResultColumn(j))
        Next
        Multivariate18Fixture.Evidence("CA-UNLABELLED", caseNumber, data, Nothing, "LEGACY_FAILURE_RETAINED", legacyError, "DORMANT_CORRECTION_NOT_ACTIVATED")
        If caseNumber = 0 Then
            Dim counts(data.nRows - 1, data.nCols - 1) As Integer
            For i = 0 To data.nRows - 1
                For j = 0 To data.nCols - 1
                    counts(i, dormant.ResultColumn(j)) = CInt(data.FinalData(i, j))
                    Assert.AreEqual(CInt(raw(i, j)), counts(i, j))
                Next
            Next
            Dim model As New CA : model.data(counts) : model.Calculate()
            Dim projection = MultivariateComponentProjectionPolicy.SimpleCa(dormant, model, counts)
            Assert.IsTrue(projection.Contingency.CandidateOnly)
            Assert.IsFalse(projection.Axis(MultivariateSemanticAxis.ContingencyColumns).Items(0).SourceRowId.HasValue)
            Multivariate18Fixture.Evidence("CA-DORMANT", caseNumber, data, projection, "DORMANT_CORE_RETURNED_NOT_NATIVE", numericalDigest:=Multivariate18Fixture.Digest(counts, model.Eigenvalues))
        End If
    End Sub
    <DataTestMethod><DataRow(0)><DataRow(1)><DataRow(2)><DataRow(3)><DataRow(4)><DataRow(5)><DataRow(6)><DataRow(7)>
    Public Sub ExistingPcaNegativeInputsAreNotNewRefusals(caseNumber As Integer)
        Dim raw(5, 1) As Object
        For i = 0 To 5
            raw(i, 0) = CDbl(i + 1) : raw(i, 1) = CDbl(i + 1) + i * 0.000000001
        Next
        Select Case caseNumber
            Case 0
                For i = 0 To 5
                    raw(i, 0) = 1.0
                Next
            Case 1 : raw(0, 0) = Double.NaN
            Case 2 : raw(0, 0) = Double.PositiveInfinity
            Case 3 : raw(0, 0) = Nothing
            Case 4 : raw = New Object(,) {{1.0}}
        End Select
        Dim data = Multivariate18Fixture.Acquired(raw, Enumerable.Range(0, raw.GetLength(1)).Select(Function(j) "x" & j).ToArray()), model As New PCA, failure As Exception = Nothing
        Try
            model.dataInputs(data.DataDbl, data.RowIds, data.varNames)
            model.settingsInputs(If(caseNumber = 5, 1, 100), If(caseNumber = 6, Double.NaN, 0.000001), "Correlation")
            model.Calculate("Fixed", If(caseNumber = 7, 0, 1))
        Catch ex As Exception
            failure = ex
        End Try
        If failure IsNot Nothing Then
            Multivariate18Fixture.Evidence("PCA-NEGATIVE", caseNumber, data, Nothing, "EXISTING_CORE_REJECTED", failure) : Return
        End If
        Dim projection = MultivariateComponentProjectionPolicy.Pca(Multivariate18Fixture.Input(MultivariateAnalysisKind.PCA, data), model, Nothing, data.FinalData)
        Dim oldWriter As New Multivariate18Fixture.MemoryWriter, newWriter As New Multivariate18Fixture.MemoryWriter
        LegacyMultivariate18Layout.PcaData(oldWriter, data, model, Nothing, Nothing)
        MultivariateComponentProjectionPolicy.VisitPca(projection, data.FinalData, model, Sub(s, v, t) Multivariate18Fixture.Consume(newWriter, s, v, t))
        Multivariate18Fixture.Blocks(oldWriter, newWriter)
        Multivariate18Fixture.Evidence("PCA-NEGATIVE", caseNumber, data, projection, "EXISTING_CORE_RETURNED_NOT_SCIENTIFICALLY_VALIDATED")
    End Sub
    <DataTestMethod><DataRow(0)><DataRow(1)><DataRow(2)><DataRow(3)><DataRow(4)><DataRow(5)>
    Public Sub McaEmptySmallNullAndMalformedAxesRetainCoreBoundary(caseNumber As Integer)
        Dim saved = CultureInfo.CurrentCulture
        Try
            For Each culture In {"", "sk-SK"}
                CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(culture)
                Dim raw(7, 1) As Object, names As String() = {"a", "b"}
                For i = 0 To 7
                    raw(i, 0) = If(i Mod 2 = 0, " ρ ", "σ") : raw(i, 1) = If(i Mod 3 = 0, Nothing, "ρ")
                Next
                Select Case caseNumber
                    Case 0 : raw = DirectCast(Array.CreateInstance(GetType(Object), 0, 1), Object(,)) : names = {"a"}
                    Case 1 : raw = New Object(,) {{"a", "b", "c"}, {"d", "e", "f"}} : names = {"a", "b", "c"}
                    Case 2 : raw = New Object(,) {{Nothing}, {Nothing}, {Nothing}, {Nothing}} : names = {"a"}
                    Case 3 : names = {"a"}
                    Case 4 : names = {Nothing, "b"}
                    Case 5 : raw(0, 0) = " a" & vbLf & "b "
                End Select
                Dim data = Multivariate18Fixture.Acquired(raw, names), input = Multivariate18Fixture.Input(MultivariateAnalysisKind.MCA, data), model As New CA, failure As Exception = Nothing
                Try
                    model.DataMultiple(Global.BESHStatNG.DataManagement.ArrayUtilities.Array2strArray(data.FinalData), names) : model.Calculate()
                Catch ex As Exception
                    failure = ex
                End Try
                If failure IsNot Nothing Then
                    Multivariate18Fixture.Evidence("MCA-NEGATIVE", caseNumber, data, Nothing, "EXISTING_CORE_REJECTED", failure) : Continue For
                End If
                Dim projection = MultivariateComponentProjectionPolicy.Mca(New MultivariateMcaTransformIntent(input, False), model, raw)
                Dim before As New Multivariate18Fixture.MemoryWriter, after As New Multivariate18Fixture.MemoryWriter
                LegacyMultivariate18Layout.McaData(before, data, model)
                MultivariateComponentProjectionPolicy.VisitMca(projection, raw, model, Sub(s, v, t) Multivariate18Fixture.Consume(after, s, v, t))
                Multivariate18Fixture.Blocks(before, after)
                Assert.IsTrue(projection.Validation.Diagnostics.All(Function(d) Not d.Blocking))
                Multivariate18Fixture.Evidence("MCA-NEGATIVE", caseNumber, data, projection, "EXISTING_CORE_RETURNED_NOT_SCIENTIFICALLY_VALIDATED")
            Next
        Finally
            CultureInfo.CurrentCulture = saved
        End Try
    End Sub
End Class
