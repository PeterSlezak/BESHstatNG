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

Public NotInheritable Class Regression11Fixture
    Public Shared Function Root() As String
        Return Path.Combine(Regression09Fixture.Root(), "evidence", "04A11", "baselines")
    End Function
    Public Shared Function DefectObservations() As String()
        Dim lines As New List(Of String) From {"04A11-prechange-defects-NOT_GOLDEN"}
        For Each alpha In {0.05, 0.1}
            Dim f = Regression09Fixture.Load("glm_gaussian_full.csv"), lm As New LinearModel With {.bReturnCov = True, .bComputeResiduals = True}
            lm.Data(f.Data, f.Names, f.Ids) : lm.Fit(True, Nothing, TermSumOfSquaresType.TypeIII)
            lines.Add("LM requested=" & alpha.ToString("R", CultureInfo.InvariantCulture) & " actual=" & lm.Alpha.ToString("R", CultureInfo.InvariantCulture))
            f.Tables = lm.wrapResults() : f.Fitted = Regression09Fixture.Column(lm.Fitted) : f.Residual = lm.AllResiduals_toPrint
            lines.AddRange(Regression09Fixture.Canonical(f))
        Next
        For Each intercept As Boolean In {False, True}
            For Each cov In {False, True}
                Dim f = Regression09Fixture.Load("glm_gaussian_full.csv"), names = DirectCast(f.Names.Clone(), String())
                Dim glm As New GLM(New Gaussian(), New Identity()) With {.bReturnCov = cov}
                glm.data(f.Data, f.Ids) : glm.setVarNames(names) : glm.settingInputs(0.05, 100, 0.000001) : glm.Fit(If(intercept, 1, 0), False)
                Dim before = String.Join("|", names)
                glm.wrapResults(Nothing, Nothing) : glm.wrapResults(Nothing, Nothing)
                lines.Add($"COV intercept={intercept} enabled={cov} before={before} after={String.Join("|", names)}")
            Next
        Next
        For Each response In {0.0, 1.0}
            For Each interceptOnly In {False, True}
                For Each hl In {False, True}
                    For Each sections In {False, True}
                        Dim data(23, If(interceptOnly, 0, 1)) As Double
                        For i = 0 To 23
                            data(i, 0) = response
                            If Not interceptOnly Then data(i, 1) = i - 11.5
                        Next
                        Dim glm As New GLM(New Binomial(), New Logit()) With {.bHosmerLemeshow = hl, .bReturnCov = sections, .bIterationDetails = sections, .bComputeResiduals = sections}
                        glm.data(data) : glm.setVarNames(If(interceptOnly, {"y"}, {"y", "x"})) : glm.settingInputs(0.05, 1, 0.00000000000001)
                        Dim status As String
                        Try
                            glm.Fit(1, False)
                            Dim tables = glm.wrapResults(Nothing, Nothing)
                            status = "tables=" & tables.Count.ToString(CultureInfo.InvariantCulture)
                        Catch ex As Exception
                            status = ex.GetType().FullName
                        End Try
                        lines.Add($"CONSTANT y={response} interceptOnly={interceptOnly} hl={hl} sections={sections} converged={glm.Converged} outcome={status}")
                    Next
                Next
            Next
        Next
        Return lines.ToArray()
    End Function
    Public Shared Sub Capture(name As String, lines() As String)
        Directory.CreateDirectory(Root())
        Using stream As New FileStream(Path.Combine(Root(), name), FileMode.CreateNew)
            Using writer As New StreamWriter(stream, New UTF8Encoding(False)) With {.NewLine = ChrW(10)}
                For Each line In lines : writer.WriteLine(line) : Next
            End Using
        End Using
    End Sub
End Class

<TestClass>
Public Class Regression11Baselines
    <DataTestMethod><DataRow("NB2", False)><DataRow("NB2", True)><DataRow("ZIP", False)><DataRow("ZIP", True)><DataRow("Ordinal", False)><DataRow("Ordinal", True)><DataRow("Multinomial", False)><DataRow("Multinomial", True)>
    Public Sub FrozenFourFamilyTablesAndVectors(kind As String, alternate As Boolean)
        Dim saved = CultureInfo.CurrentCulture
        Try
            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture
            Dim actual = Regression09Fixture.Canonical(Regression09Fixture.Fit(kind, alternate))
            Dim name = kind & If(alternate, "-alternate", "-ordinary") & If(Environment.Version.Major < 5, "-net48", "") & ".txt"
            If Environment.GetEnvironmentVariable("BESHSTAT_04A11_CAPTURE") = "PRECHANGE-463aaa1" Then Regression11Fixture.Capture(name, actual)
            Regression10Fixture.AssertCapture(File.ReadAllLines(Path.Combine(Regression11Fixture.Root(), name)), actual, name)
        Finally
            CultureInfo.CurrentCulture = saved
        End Try
    End Sub
    <TestMethod>
    Public Sub PreservedExecutablePrechangeDefectObservation()
        Dim saved = CultureInfo.CurrentCulture
        Try
            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture
            Dim name = "defects-before" & If(Environment.Version.Major < 5, "-net48", "") & ".txt"
            If Environment.GetEnvironmentVariable("BESHSTAT_04A11_CAPTURE") = "PRECHANGE-463aaa1" Then Regression11Fixture.Capture(name, Regression11Fixture.DefectObservations())
            Dim before = File.ReadAllLines(Path.Combine(Regression11Fixture.Root(), name))
            Assert.IsTrue(before.Any(Function(s) s.Contains("LM requested=0.1 actual=0.05")))
            Assert.IsTrue(before.Any(Function(s) s.Contains("after=Intercept|x1|x2")))
            Assert.IsTrue(before.Any(Function(s) s.Contains("System.NullReferenceException")))
        Finally
            CultureInfo.CurrentCulture = saved
        End Try
    End Sub
End Class
