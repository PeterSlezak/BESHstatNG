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

' Pre-edit merged Core, original GUI request semantics; reference candidates only.
Public NotInheritable Class Mixed14Fixture
    Public Y, Visit As Double()
    Public X, Z As Double(,)
    Public Subjects As Object()
    Public Rows As Integer()
    Public FixedNames As String() = {"Intercept", "visit"}
    Public RandomNames As String()
    Public Shared Function Cases() As IEnumerable(Of Object())
        Dim keys As New List(Of String)
        For Each cov In {"Identity", "CS", "AR1", "UN", "CSH", "TOEP"}
            keys.Add("MMRM|" & cov & "|Large-sample normal|REML|toy")
        Next
        For Each inf In {"Residual DF", "Between-within DF", "Satterthwaite", "Kenward-Roger", "unknown"}
            keys.Add("MMRM|CS|" & inf & "|REML|toy")
        Next
        keys.AddRange({"MMRM|CS|Kenward-Roger|ML|toy", "MMRM|CS|Large-sample normal|ML|toy",
            "MMRM|AR1|Satterthwaite|REML|missing", "MMRM|CS|Kenward-Roger|REML|orthodont",
            "LMM|Random Intercept|Large-sample normal|REML|toy", "LMM|Random Intercept|Residual DF|ML|toy",
            "LMM|Random Intercept|Satterthwaite|REML|toy", "LMM|Random Intercept|Kenward-Roger|ML|toy",
            "LMM|Random Intercept + Slope|Kenward-Roger|REML|toy", "LMM|Variance Components (VC/Diag)|Large-sample normal|REML|toy",
            "LMM|Random Intercept|unknown|REML|novisit", "LMM|Random Intercept + Slope|Satterthwaite|REML|sleep-unbalanced",
            "LMM|Random Intercept + Slope|Large-sample normal|REML|sleep", "LMM|Random Intercept|Large-sample normal|REML|limit"})
        Return keys.Select(Function(k) New Object() {k})
    End Function
    Public Shared Function Load(key As String) As Mixed14Fixture
        Dim p = key.Split("|"c), mmrm = p(0) = "MMRM", f As New Mixed14Fixture
        Dim ys As New List(Of Double), vs As New List(Of Double), subjects As New List(Of Object), rows As New List(Of Integer)
        If p(4) = "orthodont" OrElse p(4).StartsWith("sleep", StringComparison.Ordinal) Then
            Dim csv = If(p(4) = "orthodont", "mmrm_orthodont_potthoffroy_long.csv", If(p(4) = "sleep", "mixedmodel_lmm_sleepstudy_random_slope_data.csv", "mixedmodel_lmm_sleepstudy_random_slope_unbalanced_data.csv"))
            Dim lines = File.ReadAllLines(Path.Combine(Regression09Fixture.Root(), "BESHStatNG.Test", "TestData", csv))
            For i = 1 To lines.Length - 1
                Dim cells = lines(i).Split(","c)
                ys.Add(Double.Parse(cells(If(p(4) = "orthodont", 6, 0)), CultureInfo.InvariantCulture))
                vs.Add(Double.Parse(cells(If(p(4) = "orthodont", 4, 1)), CultureInfo.InvariantCulture))
                subjects.Add(cells(If(p(4) = "orthodont", 0, 2))) : rows.Add(101 + i * 3)
            Next
        Else
            For i = 0 To 35
                Dim subject = i Mod 12, v = 2 - i \ 12
                If p(4) = "missing" AndAlso (i = 5 OrElse i = 21 OrElse i = 30) Then Continue For
                ys.Add(8 + 0.7 * v + 0.3 * Math.Sin(subject) + 0.18 * Math.Cos(i * 1.7) + 0.08 * (subject Mod 3) * v)
                vs.Add(CDbl(v)) : subjects.Add("S" & (11 - subject).ToString("D2", CultureInfo.InvariantCulture)) : rows.Add(101 + i * 3)
            Next
        End If
        f.Y = ys.ToArray() : f.Visit = If(p(4) = "novisit", Nothing, vs.ToArray())
        f.Subjects = subjects.ToArray() : f.Rows = rows.ToArray()
        ReDim f.X(f.Y.Length - 1, 1)
        Dim q = If(mmrm, 0, If(p(1) = "Random Intercept", 1, 2))
        If q > 0 Then ReDim f.Z(f.Y.Length - 1, q - 1)
        f.RandomNames = If(q = 1, New String() {"Intercept"}, If(q = 2, New String() {"Intercept", "visit"}, Nothing))
        For i = 0 To f.Y.Length - 1
            f.X(i, 0) = 1 : f.X(i, 1) = vs(i)
            If q > 0 Then f.Z(i, 0) = 1
            If q > 1 Then f.Z(i, 1) = vs(i)
        Next
        Return f
    End Function
    Public Shared Function LegacyRequest(key As String, f As Mixed14Fixture) As MixedModelFitRequest
        Dim p = key.Split("|"c), mmrm = p(0) = "MMRM"
        Dim blocks = MixedModelBlockData.FromArrays(f.Y, f.X, f.Subjects, f.Z, f.Visit, mmrm OrElse f.Visit IsNot Nothing, f.Rows)
        Dim method = If(p(3) = "ML", MixedModelFitMethod.ML, MixedModelFitMethod.REML)
        If String.Equals(p(2), "Kenward-Roger", StringComparison.OrdinalIgnoreCase) Then method = MixedModelFitMethod.REML
        Dim req = If(mmrm, MixedModelFitRequest.CreateMMRM(blocks, MixedModelRStructUtils.createMixedModelRStruct(p(1)), method),
            MixedModelFitRequest.CreateLMM(blocks, New IdentityR(), MixedModelGStructUtils.createMixedModelGStruct(p(1)), method))
        req.ResponseVarName = "response" : req.SubjectVarName = "subject" : req.VisitVarName = If(f.Visit Is Nothing, "", "visit")
        req.FixedEffectNames = f.FixedNames : req.RandomEffectNames = f.RandomNames
        req.FixedFormulaText = "visit" : req.RandomFormulaText = If(mmrm, "", If(f.RandomNames.Length = 1, "Random intercept only", "visit"))
        req.RequestLabel = p(0)
        Select Case p(2)
            Case "Large-sample normal" : req.FixedInferenceMethod = MixedModelFixedInferenceMethod.WaldNormal
            Case "Residual DF" : req.FixedInferenceMethod = MixedModelFixedInferenceMethod.ResidualDF
            Case "Satterthwaite" : req.FixedInferenceMethod = MixedModelFixedInferenceMethod.Satterthwaite : req.UseSatterthwaite = True
            Case "Kenward-Roger"
                If mmrm Then req.EnableFullKenwardRogerForMmrm() Else req.EnableFullKenwardRogerForLmm()
            Case Else : req.FixedInferenceMethod = If(mmrm, MixedModelFixedInferenceMethod.BetweenWithin, MixedModelFixedInferenceMethod.WaldNormal)
        End Select
        Dim ctl = req.Control
        ctl.MaxIter = If(p(4) = "limit", 1, 50) : ctl.Epsilon = 0.000001 : ctl.StepTolerance = ctl.Epsilon : ctl.FunctionTolerance = ctl.Epsilon
        ctl.CovarianceOptimizerMode = MixedModelCovarianceOptimizerMode.AverageInformationReml
        ctl.CovarianceGradientMode = MixedModelCovarianceGradientMode.Auto : ctl.Trace = True : req.Control = ctl
        Return req
    End Function
    Public Shared Function Fit(key As String, req As MixedModelFitRequest) As MixedModelResult
        Return If(key.StartsWith("MMRM|", StringComparison.Ordinal), New MMRM(req).Fit(), New LMM(req).Fit())
    End Function
    Public Shared Function Capture(key As String, Optional req As MixedModelFitRequest = Nothing, Optional result As MixedModelResult = Nothing,
                                   Optional tables As List(Of ResultTable) = Nothing) As String()
        Dim lines As New List(Of String) From {"04A14-Core-NOT_GOLDEN", key}
        Try
            Dim f = Load(key) : req = If(req, LegacyRequest(key, f))
            For Each block In req.Data.Blocks
                lines.Add("SUBJECT|" & block.SubjectKey) : AddArray(lines, block.RowIndices)
                AddArray(lines, block.Y) : AddArray(lines, block.X) : AddArray(lines, block.Z) : AddArray(lines, block.Visit)
            Next
            result = If(result, Fit(key, req))
            lines.Add("STATUS|" & result.Converged.ToString() & "|" & result.Cancelled.ToString() & "|" & result.Interrupted.ToString())
            lines.Add("INFERENCE|" & result.FixedInferenceMethod.ToString())
            AddArray(lines, result.Beta) : AddArray(lines, result.VarBeta) : AddArray(lines, result.BetaDF)
            AddArray(lines, result.BetaP) : AddArray(lines, result.Theta) : AddArray(lines, result.FittedMarginal) : AddArray(lines, result.ResidualRaw)
            lines.AddRange(CanonicalTables(If(tables, result.wrapResults(0.05, True, key.StartsWith("LMM|", StringComparison.Ordinal), False))))
        Catch ex As OperationCanceledException
            Throw
        Catch ex As Exception
            lines.Add("CORE_FAILURE|" & ex.GetType().FullName)
        End Try
        Return lines.ToArray()
    End Function
    Public Shared Sub AddArray(lines As List(Of String), array As Array)
        If array Is Nothing Then lines.Add("NULL") : Return
        lines.Add("ARRAY|" & String.Join(",", Enumerable.Range(0, array.Rank).Select(Function(i) array.GetLength(i))))
        For Each v As Object In array
            lines.Add(If(TypeOf v Is Double, "D:" & CDbl(v).ToString("R", CultureInfo.InvariantCulture), v.GetType().Name & ":" & Convert.ToBase64String(Encoding.UTF8.GetBytes(Convert.ToString(v, CultureInfo.InvariantCulture)))))
        Next
    End Sub
    Public Shared Function CanonicalTables(tables As List(Of ResultTable)) As String()
        Dim lines As New List(Of String)
        For Each t In tables
            Dim m = t.ToOutputModel(), cells = m.Values
            lines.Add($"TABLE {m.RowCount} {m.ColumnCount} {m.TitleRows} {m.HeaderTopRows} {m.HeaderLeftColumns} {m.FooterRows} {String.Join(",", m.PvalueColumns)} {String.Join(";", m.PvalueCells.Select(Function(c) c.BodyRow.ToString() & "," & c.BodyColumn.ToString()))}")
            For r = 0 To m.RowCount - 1
                For col = 0 To m.ColumnCount - 1
                    Dim v = cells(r, col)
                    If col > 0 AndAlso TypeOf cells(r, col - 1) Is String Then
                        Dim label = CStr(cells(r, col - 1))
                        If label = "Execution time" OrElse label.EndsWith("time ms", StringComparison.Ordinal) Then v = "<elapsed>"
                    End If
                    If v Is Nothing Then
                        lines.Add("NULL")
                    ElseIf TypeOf v Is Double Then
                        lines.Add("D:" & CDbl(v).ToString("R", CultureInfo.InvariantCulture))
                    Else
                        lines.Add(v.GetType().Name & ":" & Convert.ToBase64String(Encoding.UTF8.GetBytes(Convert.ToString(v, CultureInfo.InvariantCulture))))
                    End If
                Next
            Next
        Next
        Return lines.ToArray()
    End Function
    Public Shared Function Name(key As String, culture As String) As String
        Return String.Join("-", key.Split("|"c)).Replace(" ", "").Replace("/", "_") & "-" & If(culture = "", "iv", culture) & If(Environment.Version.Major < 5, "-net48", "") & ".txt"
    End Function
End Class

<TestClass>
Public Class Mixed14Baselines
    <DataTestMethod><DynamicData(NameOf(Mixed14Fixture.Cases), GetType(Mixed14Fixture), DynamicDataSourceType.Method)>
    Public Sub FrozenOriginalCoreRequestsFitsAndReports(key As String)
        Dim saved = CultureInfo.CurrentCulture
        Try
            For Each culture In {"", "sk-SK"}
                CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(culture)
                Dim actual = Mixed14Fixture.Capture(key), folder = IO.Path.Combine(Regression09Fixture.Root(), "evidence", "04A14", "baselines")
                Dim path = IO.Path.Combine(folder, Mixed14Fixture.Name(key, culture))
                If Environment.GetEnvironmentVariable("BESHSTAT_04A14_CAPTURE") = "PRECHANGE-b02480a" Then
                    Directory.CreateDirectory(folder)
                    Using stream As New FileStream(path, FileMode.CreateNew)
                        Using writer As New StreamWriter(stream, New UTF8Encoding(False)) With {.NewLine = ChrW(10)}
                            For Each line In actual : writer.WriteLine(line) : Next
                        End Using
                    End Using
                End If
                Regression10Fixture.AssertCapture(File.ReadAllLines(path), actual, key)
            Next
        Finally
            CultureInfo.CurrentCulture = saved
        End Try
    End Sub
End Class
