Option Explicit On
Option Strict On
Imports System
Imports System.Collections.Generic
Imports System.Globalization
Imports System.IO
Imports System.Linq
Imports System.Text
Imports BESHStatNG
Imports BESHStatNG.regression
Imports Microsoft.VisualStudio.TestTools.UnitTesting

' Source-derived Core-backed fixtures captured before production edits on c25c695.
' Not native observations or promoted scientific references. No historical writer.
Public NotInheritable Class Regression09Fixture
    Public Property Data As Double(,)
    Public Property Names As String()
    Public Property Ids As Integer()
    Public Property Offset As Double()
    Public Property Weights As Double()
    Public Property Tables As List(Of ResultTable)
    Public Property Fitted As Double(,)
    Public Property Residual As Object(,)
    Public Property Converged As Nullable(Of Boolean)
    Public Property Categories As Integer()

    Public Shared Function Root() As String
        Dim p As New DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory)
        While p IsNot Nothing
            If File.Exists(Path.Combine(p.FullName, "planning", "BESHStatNG_CrossPlatform_MasterPlan.md")) Then Return p.FullName
            p = p.Parent
        End While
        Throw New DirectoryNotFoundException("Repository fixture root required.")
    End Function

    Public Shared Function Load(name As String) As Regression09Fixture
        Dim lines = File.ReadAllLines(Path.Combine(Root(), "BESHStat.Core.Tests", "TestData", name))
        Dim header = lines(0).Split(","c)
        Dim names = header.Where(Function(h) h <> "id" AndAlso h <> "offset" AndAlso h <> "w").ToArray()
        Dim data(lines.Length - 2, names.Length - 1) As Double
        Dim ids = Enumerable.Range(0, lines.Length - 1).Select(Function(i) 7 + 3 * i).ToArray()
        Dim offs(lines.Length - 2), weights(lines.Length - 2) As Double
        For i = 1 To lines.Length - 1
            Dim cells = lines(i).Split(","c)
            For j = 0 To names.Length - 1
                data(i - 1, j) = Double.Parse(cells(Array.IndexOf(header, names(j))), CultureInfo.InvariantCulture)
            Next
            offs(i - 1) = If(header.Contains("offset"), Double.Parse(cells(Array.IndexOf(header, "offset")), CultureInfo.InvariantCulture), 0.03 * (i Mod 3))
            weights(i - 1) = If(header.Contains("w"), Double.Parse(cells(Array.IndexOf(header, "w")), CultureInfo.InvariantCulture), 1.0 + (i Mod 2))
        Next
        Return New Regression09Fixture With {.Data = data, .Names = names, .Ids = ids, .Offset = offs, .Weights = weights}
    End Function

    Public Shared Function Column(v() As Double) As Double(,)
        If v Is Nothing Then Return Nothing
        Dim m(v.Length - 1, 0) As Double
        For i = 0 To v.Length - 1 : m(i, 0) = v(i) : Next
        Return m
    End Function

    Public Shared Function Fit(kind As String, alternate As Boolean) As Regression09Fixture
        Dim file As String
        Select Case kind
            Case "LM" : file = "glm_gaussian_full.csv"
            Case "GLM" : file = If(alternate, "glm_binomial_full.csv", "glm_poisson_full.csv")
            Case "NB2" : file = "glm_nb2_full.csv"
            Case "ZIP" : file = "zip_dataset_basic_count.csv"
            Case "Ordinal" : file = "ordinal_logit_dataset_basic.csv"
            Case Else : file = "mlogit_dataset_grouped_basic.csv"
        End Select
        Dim f = Load(file)
        Select Case kind
            Case "LM"
                Dim model As New LinearModel With {.bReturnCov = alternate, .bComputeResiduals = alternate}
                model.Data(f.Data, f.Names, f.Ids, If(alternate, f.Weights, Nothing))
                model.Fit(Not alternate)
                f.Tables = model.wrapResults() : f.Fitted = Column(model.Fitted)
                If alternate Then f.Residual = model.AllResiduals_toPrint
            Case "GLM", "NB2"
                Dim model As GLM = If(kind = "NB2", CType(New GLM_NB(New Log()), GLM), New GLM(If(alternate, CType(New Binomial(), Family), New Poisson()), If(alternate, CType(New Logit(), Link), New Log())))
                model.bReturnCov = True : model.bComputeResiduals = Not (kind = "NB2" AndAlso alternate)
                model.bIterationDetails = alternate
                model.data(f.Data, f.Ids, f.Offset, f.Weights) : model.setVarNames(DirectCast(f.Names.Clone(), String()))
                model.settingInputs(0.05, 100, 0.000001)
                model.Fit(1, False)
                f.Tables = model.wrapResults("offset", "weight") : f.Fitted = Column(model.PredictedResponses) : f.Converged = model.Converged
                If model.bComputeResiduals Then f.Residual = model.AllResiduals
                If kind = "GLM" AndAlso alternate Then
                    Dim y = model.ObservedResponses, p = model.PredictedResponses, w = model.ObservationWeights
                    f.Tables.AddRange(BinaryClassificationCore.WrapResults(BinaryClassificationCore.ComputeBinarySummary(y, p, 0.5, w), BinaryClassificationCore.BuildThresholdTable(y, p, Nothing, w), BinaryClassificationCore.BuildCalibrationBins(y, p, 5, w), BinaryClassificationCore.ComputeBrierScore(y, p, w), Double.NaN, "GLM Binary Classification"))
                End If
            Case "ZIP"
                Dim z = Load("zip_dataset_basic_zero.csv")
                Dim model As New ZeroInflatedPoisson With {.bComputeResiduals = Not alternate, .bReturnCov = True, .bIterationDetails = alternate}
                model.dataInputs(f.Data, z.Data, f.Names, z.Names, f.Ids, f.Offset, "offset")
                model.settingInputs(0.05, 100, 100, 0.000001)
                model.Fit(1, If(alternate, 0, 1), False, False)
                f.Tables = model.wrapResults("offset", Nothing) : f.Fitted = Column(model.Predicted) : f.Converged = model.Converged
                If model.bComputeResiduals Then f.Residual = model.AllResiduals
            Case "Ordinal"
                Dim model As New OrdinalLogitModel With {.bComputeResiduals = True, .bReturnCov = alternate, .bIterationDetails = alternate}
                model.Data(f.Data, f.Names, f.Ids, f.Offset, f.Weights) : model.SettingInputs(0.05, 100, 0.000001)
                model.Fit(If(alternate, ReferenceCategory.First, ReferenceCategory.Last), False)
                f.Tables = model.wrapResults("offset", "weight") : f.Fitted = model.Residuals.Probabilities : f.Residual = model.wrapResiduals() : f.Categories = model.Residuals.Categories
            Case "Multinomial"
                Dim model As New MultinomialLogitModel With {.bComputeResiduals = True, .bReturnCov = alternate, .bIterationDetails = alternate}
                model.data(f.Data, f.Names, f.Ids, f.Offset, f.Weights) : model.settingInputs(0.05, 100, 0.000001)
                model.Fit(If(alternate, 0, 1), If(alternate, ReferenceCategory.First, ReferenceCategory.Last), False)
                f.Tables = model.wrapResults("offset", "weight") : f.Fitted = model.ResidualDiagnostics.Probabilities : f.Residual = model.wrapResiduals() : f.Categories = model.ResidualDiagnostics.Categories
        End Select
        Return f
    End Function

    ' Typed line encoding preserves null/NaN and exact strings; only wall-clock time is masked.
    Public Shared Function Canonical(f As Regression09Fixture) As String()
        Dim lines As New List(Of String) From {"04A09-host-free-v1", String.Join(",", f.Ids), If(f.Converged.HasValue, f.Converged.Value.ToString(), "UNKNOWN")}
        For Each table In f.Tables
            Dim m = table.ToOutputModel()
            lines.Add($"TABLE {m.RowCount} {m.ColumnCount} {m.TitleRows} {m.HeaderTopRows} {m.HeaderLeftColumns} {m.FooterRows} {String.Join(",", m.PvalueColumns)} {String.Join(";", m.PvalueCells.Select(Function(c) c.BodyRow.ToString() & "," & c.BodyColumn.ToString()))}")
            AppendCells(lines, m.Values)
        Next
        lines.Add("FITTED")
        If f.Fitted IsNot Nothing Then
            lines.Add($"{f.Fitted.GetLength(0)} {f.Fitted.GetLength(1)}")
            For Each v In f.Fitted : lines.Add("D:" & v.ToString("R", CultureInfo.InvariantCulture)) : Next
        End If
        lines.Add("RESIDUAL") : AppendCells(lines, f.Residual)
        Return lines.ToArray()
    End Function

    Private Shared Sub AppendCells(lines As List(Of String), values(,) As Object)
        If values Is Nothing Then
            lines.Add("NULL") : Return
        End If
        lines.Add($"{values.GetLength(0)} {values.GetLength(1)}")
        For Each v As Object In values
            If v Is Nothing Then
                lines.Add("NULL")
            ElseIf TypeOf v Is Double Then
                lines.Add("D:" & CDbl(v).ToString("R", CultureInfo.InvariantCulture))
            Else
                Dim s = Convert.ToString(v, CultureInfo.InvariantCulture)
                If s.StartsWith("Computational time", StringComparison.OrdinalIgnoreCase) Then s = "Computational time <elapsed> seconds."
                lines.Add(v.GetType().Name & ":" & Convert.ToBase64String(Encoding.UTF8.GetBytes(s)))
            End If
        Next
    End Sub
End Class

<TestClass>
Public Class Regression09Baselines
    <DataTestMethod>
    <DataRow("LM", False)><DataRow("LM", True)>
    <DataRow("GLM", False)><DataRow("GLM", True)>
    <DataRow("NB2", False)><DataRow("NB2", True)>
    <DataRow("ZIP", False)><DataRow("ZIP", True)>
    <DataRow("Ordinal", False)><DataRow("Ordinal", True)>
    <DataRow("Multinomial", False)><DataRow("Multinomial", True)>
    Public Sub FrozenCoreTablesAndVectors(kind As String, alternate As Boolean)
        Dim savedCulture = CultureInfo.CurrentCulture
        Try
            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture
            Dim actual = Regression09Fixture.Canonical(Regression09Fixture.Fit(kind, alternate))
            Dim root = Path.Combine(Regression09Fixture.Root(), "evidence", "04A09", "baselines")
            Dim name = kind & If(alternate, "-alternate", "-ordinary") & If(Environment.Version.Major < 5, "-net48", "") & ".txt"
            If Environment.GetEnvironmentVariable("BESHSTAT_04A09_CAPTURE") = "PRECHANGE-c25c695" Then
                Directory.CreateDirectory(root)
                Using stream As New FileStream(Path.Combine(root, name), FileMode.CreateNew)
                    Using w As New StreamWriter(stream, New UTF8Encoding(False))
                        For Each line In actual : w.WriteLine(line) : Next
                    End Using
                End Using
            End If
            Dim expected = File.ReadAllLines(Path.Combine(root, name))
            Assert.AreEqual(expected.Length, actual.Length, kind)
            For i = 0 To expected.Length - 1
                If expected(i).StartsWith("D:", StringComparison.Ordinal) AndAlso actual(i).StartsWith("D:", StringComparison.Ordinal) Then
                    Dim e = Double.Parse(expected(i).Substring(2), CultureInfo.InvariantCulture), a = Double.Parse(actual(i).Substring(2), CultureInfo.InvariantCulture)
                    If Double.IsNaN(e) Then
                        Assert.IsTrue(Double.IsNaN(a), $"{kind} cell {i}")
                    Else
                        Assert.AreEqual(e, a, Math.Max(0.000000001, Math.Abs(e) * 0.000000001), $"{kind} cell {i}")
                    End If
                Else
                    Assert.AreEqual(expected(i), actual(i), $"{kind} cell {i}")
                End If
            Next
        Finally
            CultureInfo.CurrentCulture = savedCulture
        End Try
    End Sub
End Class
