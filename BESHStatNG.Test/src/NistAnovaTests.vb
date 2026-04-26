Option Explicit On
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports System
Imports System.Collections.Generic
Imports System.Globalization
Imports System.IO
Imports System.Linq
Imports BESHStatNG

<TestClass()>
Public Class NistAnova_Tests

    Private Const ABS_TOL As Double = 1.0E-12
    Private Const REL_TOL_DEFAULT As Double = 1.0E-8
    Private Const REL_TOL_HIGHER As Double = 2.0E-4

    <DataTestMethod>
    <DataRow("SiRstv")>
    <DataRow("SmLs01")>
    <DataRow("SmLs02")>
    <DataRow("SmLs03")>
    <DataRow("AtmWtAg")>
    <DataRow("SmLs04")>
    <DataRow("SmLs05")>
    <DataRow("SmLs06")>
    <DataRow("SmLs07")>
    <DataRow("SmLs08")>
    <DataRow("SmLs09")>
    Public Sub OneWayANOVA_matches_nist_anova_reference(datasetName As String)
        RunDatasetCheck(datasetName)
    End Sub

    Private Shared Sub RunDatasetCheck(datasetName As String)
        Dim grouped As Double()() = Nothing
        Dim groupNames As String() = Nothing
        Dim shiftConstant As Double = 0.0
        LoadDataset(datasetName, grouped, groupNames, shiftConstant)

        Dim expected As Dictionary(Of String, Double) = LoadReference(datasetName)
        Dim relTol As Double = GetRelativeTolerance(datasetName)

        ' NIST notes that subtracting the leading constant from all observations can
        ' improve accuracy on stiff ANOVA datasets while leaving the ANOVA table invariant.
        If shiftConstant <> 0.0 Then
            For i As Integer = 0 To grouped.Length - 1
                For j As Integer = 0 To grouped(i).Length - 1
                    grouped(i)(j) -= shiftConstant
                Next
            Next
        End If

        Dim mdl As New parametric.OneWayANOVA(grouped, groupNames)
        Dim tbl(,) As Object = mdl.compute()

        Dim betweenSS As Double = Convert.ToDouble(tbl(0, 0), CultureInfo.InvariantCulture)
        Dim betweenDf As Integer = Convert.ToInt32(tbl(0, 1), CultureInfo.InvariantCulture)
        Dim betweenMS As Double = Convert.ToDouble(tbl(0, 2), CultureInfo.InvariantCulture)
        Dim betweenF As Double = Convert.ToDouble(tbl(0, 3), CultureInfo.InvariantCulture)

        Dim withinSS As Double = Convert.ToDouble(tbl(1, 0), CultureInfo.InvariantCulture)
        Dim withinDf As Integer = Convert.ToInt32(tbl(1, 1), CultureInfo.InvariantCulture)
        Dim withinMS As Double = Convert.ToDouble(tbl(1, 2), CultureInfo.InvariantCulture)

        Dim totalSS As Double = Convert.ToDouble(tbl(2, 0), CultureInfo.InvariantCulture)
        Dim totalDf As Integer = Convert.ToInt32(tbl(2, 1), CultureInfo.InvariantCulture)

        Dim rSquared As Double = betweenSS / totalSS
        Dim residualSd As Double = Math.Sqrt(withinMS)

        Assert.AreEqual(CInt(expected("between_df")), betweenDf, datasetName & " between df")
        Assert.AreEqual(CInt(expected("within_df")), withinDf, datasetName & " within df")
        Assert.AreEqual(CInt(expected("total_df")), totalDf, datasetName & " total df")

        AssertClose(expected("between_ss"), betweenSS, relTol, ABS_TOL, datasetName & " between SS")
        AssertClose(expected("between_ms"), betweenMS, relTol, ABS_TOL, datasetName & " between MS")
        AssertClose(expected("between_f"), betweenF, relTol, ABS_TOL, datasetName & " F")
        AssertClose(expected("within_ss"), withinSS, relTol, ABS_TOL, datasetName & " within SS")
        AssertClose(expected("within_ms"), withinMS, relTol, ABS_TOL, datasetName & " within MS")
        AssertClose(expected("total_ss"), totalSS, relTol, ABS_TOL, datasetName & " total SS")
        AssertClose(expected("r_squared"), rSquared, relTol, ABS_TOL, datasetName & " R^2")
        AssertClose(expected("residual_sd"), residualSd, relTol, ABS_TOL, datasetName & " residual SD")

        Dim pVal As Double = Convert.ToDouble(tbl(0, 4), CultureInfo.InvariantCulture)
        Assert.IsTrue(Not Double.IsNaN(pVal) AndAlso pVal >= 0.0 AndAlso pVal <= 1.0, datasetName & " p-value out of range")
    End Sub

    Private Shared Function GetRelativeTolerance(datasetName As String) As Double
        Select Case datasetName
            Case "SmLs07", "SmLs08", "SmLs09"
                Return REL_TOL_HIGHER
            Case Else
                Return REL_TOL_DEFAULT
        End Select
    End Function

    Private Shared Sub LoadDataset(datasetName As String,
                                   ByRef grouped As Double()(),
                                   ByRef groupNames As String(),
                                   ByRef shiftConstant As Double)
        Dim path As String = GetFixturePath(datasetName & ".csv")
        Dim lines() As String = File.ReadAllLines(path)
        If lines.Length < 2 Then Throw New InvalidOperationException("Dataset CSV must have header + rows.")

        Dim groups As New SortedDictionary(Of Integer, List(Of Double))()
        For i As Integer = 1 To lines.Length - 1
            Dim parts() As String = lines(i).Split(","c)
            Dim trt As Integer = Integer.Parse(parts(0).Trim(), CultureInfo.InvariantCulture)
            Dim y As Double = Double.Parse(parts(1).Trim(), NumberStyles.Float Or NumberStyles.AllowExponent, CultureInfo.InvariantCulture)
            If Not groups.ContainsKey(trt) Then groups(trt) = New List(Of Double)()
            groups(trt).Add(y)
        Next

        grouped = groups.Values.Select(Function(v) v.ToArray()).ToArray()
        groupNames = groups.Keys.Select(Function(k) "T" & k.ToString(CultureInfo.InvariantCulture)).ToArray()

        Dim refVals As Dictionary(Of String, Double) = LoadReference(datasetName)
        shiftConstant = refVals("shift_constant")
    End Sub

    Private Shared Function LoadReference(datasetName As String) As Dictionary(Of String, Double)
        Dim path As String = GetFixturePath(datasetName & "_reference.csv")
        Dim lines() As String = File.ReadAllLines(path)
        If lines.Length < 2 Then Throw New InvalidOperationException("Reference CSV must have header + rows.")

        Dim out As New Dictionary(Of String, Double)(StringComparer.OrdinalIgnoreCase)
        For i As Integer = 1 To lines.Length - 1
            Dim parts() As String = lines(i).Split(","c)
            If parts.Length < 2 Then Continue For
            Dim key As String = parts(0).Trim()
            Dim valueText As String = parts(1).Trim()
            Dim parsed As Double
            If Double.TryParse(valueText, NumberStyles.Float Or NumberStyles.AllowExponent, CultureInfo.InvariantCulture, parsed) Then
                out(key) = parsed
            End If
        Next
        Return out
    End Function

    Private Shared Function GetFixturePath(fileName As String) As String
        Dim baseDir As String = AppDomain.CurrentDomain.BaseDirectory
        Dim candidates As String() = {
            Path.Combine(baseDir, fileName),
            Path.Combine(baseDir, "TestData", "NIST_ANOVA", fileName),
            Path.GetFullPath(Path.Combine(baseDir, "..\..\TestData\NIST_ANOVA", fileName)),
            Path.GetFullPath(Path.Combine(baseDir, "..\..\..\TestData\NIST_ANOVA", fileName))
        }

        For Each c As String In candidates
            If File.Exists(c) Then Return c
        Next

        Throw New FileNotFoundException("NIST ANOVA fixture not found", fileName)
    End Function

    Private Shared Sub AssertClose(expected As Double, actual As Double, relTol As Double, absTol As Double, message As String)
        Dim diff As Double = Math.Abs(expected - actual)
        Dim tol As Double = Math.Max(absTol, relTol * Math.Max(Math.Abs(expected), Math.Abs(actual)))
        Assert.IsTrue(diff <= tol, message & ": expected=" & expected.ToString("R", CultureInfo.InvariantCulture) &
                                      ", actual=" & actual.ToString("R", CultureInfo.InvariantCulture) &
                                      ", diff=" & diff.ToString("R", CultureInfo.InvariantCulture) &
                                      ", tol=" & tol.ToString("R", CultureInfo.InvariantCulture))
    End Sub
End Class
