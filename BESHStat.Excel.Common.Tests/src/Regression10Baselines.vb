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

' Source-derived real Core observations on unedited 49fcc30. NOT_GOLDEN.
Public NotInheritable Class Regression10Fixture
    Public Shared Function Fit(name As String) As Regression09Fixture
        Dim linear = name.StartsWith("LM", StringComparison.Ordinal)
        Dim binomial = name.StartsWith("Binomial", StringComparison.Ordinal)
        Dim alternate = name.EndsWith("alternate", StringComparison.Ordinal)
        Dim f = Regression09Fixture.Load(If(binomial, "glm_binomial_full.csv", "glm_gaussian_full.csv"))
        ' Preserve physical identities in deliberately reversed order; never sort the fit.
        Array.Reverse(f.Ids)
        If linear Then
            Dim lm As New LinearModel With {.bReturnCov = True, .bComputeResiduals = True}
            lm.Data(f.Data, f.Names, f.Ids, If(alternate, f.Weights, Nothing))
            lm.SetPredictorCodingFootnotes(New List(Of String) From {"Coding note first", "Coding note second"})
            lm.Fit(Not alternate, Nothing, If(alternate, TermSumOfSquaresType.TypeI, TermSumOfSquaresType.TypeIII))
            f.Tables = lm.wrapResults() : f.Fitted = Regression09Fixture.Column(lm.Fitted) : f.Residual = lm.AllResiduals_toPrint
        Else
            Dim family As Family = If(binomial, CType(New Binomial(), Family), New Gaussian())
            Dim link As Link = If(binomial, If(alternate, CType(New Probit(), Link), New Logit()), If(alternate, CType(New Log(), Link), New Identity()))
            Dim glm As New GLM(family, link) With {.bReturnCov = True, .bComputeResiduals = True, .bIterationDetails = True}
            glm.data(f.Data, f.Ids, f.Offset, f.Weights) : glm.setVarNames(DirectCast(f.Names.Clone(), String()))
            glm.settingInputs(0.05, 100, 0.000001)
            glm.Fit(1, False)
            f.Tables = glm.wrapResults("offset", "weight") : f.Fitted = Regression09Fixture.Column(glm.PredictedResponses)
            f.Residual = glm.AllResiduals : f.Converged = glm.Converged
        End If
        Return f
    End Function

    Public Shared Sub AssertCapture(expected() As String, actual() As String, name As String)
        Assert.AreEqual(expected.Length, actual.Length, name)
        For i = 0 To expected.Length - 1
            If expected(i).StartsWith("D:", StringComparison.Ordinal) AndAlso actual(i).StartsWith("D:", StringComparison.Ordinal) Then
                Dim e = Double.Parse(expected(i).Substring(2), CultureInfo.InvariantCulture), a = Double.Parse(actual(i).Substring(2), CultureInfo.InvariantCulture)
                If Double.IsNaN(e) Then
                    Assert.IsTrue(Double.IsNaN(a), name & " cell " & i)
                Else
                    Assert.AreEqual(e, a, Math.Max(0.000000001, Math.Abs(e) * 0.000000001), name & " cell " & i)
                End If
            Else
                Assert.AreEqual(expected(i), actual(i), name & " cell " & i)
            End If
        Next
    End Sub
End Class

<TestClass>
Public Class Regression10Baselines
    <DataTestMethod>
    <DataRow("LM-ordinary")><DataRow("LM-alternate")>
    <DataRow("Gaussian-ordinary")><DataRow("Gaussian-alternate")>
    <DataRow("Binomial-ordinary")><DataRow("Binomial-alternate")>
    Public Sub FrozenPrechangeCoreTablesAndVectors(name As String)
        Dim saved = CultureInfo.CurrentCulture
        Try
            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture
            Dim actual = Regression09Fixture.Canonical(Regression10Fixture.Fit(name))
            Dim path = IO.Path.Combine(Regression09Fixture.Root(), "evidence", "04A10", "baselines", name & If(Environment.Version.Major < 5, "-net48", "") & ".txt")
            If Environment.GetEnvironmentVariable("BESHSTAT_04A10_CAPTURE") = "PRECHANGE-49fcc30" Then
                Directory.CreateDirectory(IO.Path.GetDirectoryName(path))
                Using stream As New FileStream(path, FileMode.CreateNew)
                    Using writer As New StreamWriter(stream, New UTF8Encoding(False))
                        For Each line In actual : writer.WriteLine(line) : Next
                    End Using
                End Using
            End If
            Regression10Fixture.AssertCapture(File.ReadAllLines(path), actual, name)
        Finally
            CultureInfo.CurrentCulture = saved
        End Try
    End Sub
End Class
