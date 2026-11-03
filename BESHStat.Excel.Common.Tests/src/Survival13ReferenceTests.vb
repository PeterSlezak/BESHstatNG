Option Explicit On
Option Strict On
Imports System
Imports System.Globalization
Imports System.IO
Imports System.Linq
Imports BESHStatNG
Imports BESHStatNG.ExcelCommon
Imports BESHStatNG.survival
Imports Microsoft.VisualStudio.TestTools.UnitTesting

<TestClass>
Public Class Survival13ReferenceTests
    Private Shared Function Reference(caseKey As String, field As String) As Double
        Dim path = IO.Path.Combine(Regression09Fixture.Root(), "evidence", "04A13", "R-candidate-reference-v2.csv")
        Dim row = File.ReadAllLines(path).Skip(1).Select(Function(line) line.Replace("""", "").Split(","c)).
            Single(Function(parts) parts(0) = caseKey AndAlso parts(1) = field)
        Return Double.Parse(row(2), CultureInfo.InvariantCulture)
    End Function

    <DataTestMethod><DataRow("Breslow", False)><DataRow("Efron", False)><DataRow("Exact", False)>
    <DataRow("Breslow", True)><DataRow("Efron", True)><DataRow("Exact", True)>
    Public Sub CoxCoefficientsAndLogLikelihoodAgainstExecutedR451(tie As String, stratified As Boolean)
        Dim suffix = If(stratified, "strata", "plain"), intent = Survival13Requests.Cox("Cox|" & tie & "|" & suffix)
        Dim result = intent.CreateCoreModel().Fit(intent.Options.Ties)
        Assert.IsTrue(result.Converged)
        Dim key = "Cox|" & tie.ToLowerInvariant() & "|" & suffix
        Assert.AreEqual(Reference(key, "x1"), result.Coefficients(0), 0.0000005)
        Assert.AreEqual(Reference(key, "x2"), result.Coefficients(1), 0.0000005)
        Assert.AreEqual(Reference(key, "loglik"), result.LogLikelihood, 0.000001)
    End Sub

    <DataTestMethod><DataRow("b")><DataRow("A")>
    Public Sub KaplanMeierSurvivalAtObservedTimesAgainstExecutedR451(group As String)
        Dim intent = Survival13Requests.Km("KM|Logrank|groups"), fit = intent.Execute()
        Dim tables = fit.SurvivalCurveTabularOutput()
        Dim index = Array.IndexOf(intent.Input.GroupLabels, group)
        Dim records = DirectCast(tables(index), System.Collections.Generic.List(Of SurvivalTableRecord))
        For Each row In records
            Assert.AreEqual(Reference("KM|" & group, "S@" & row.Time.ToString("0", CultureInfo.InvariantCulture)), row.Prob, 0.000000000001)
        Next
    End Sub
End Class
