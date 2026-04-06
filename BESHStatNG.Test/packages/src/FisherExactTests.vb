Option Explicit On
Imports Microsoft.VisualStudio.TestTools.UnitTesting
Imports System

' Fisher exact tests for FisherExactEngine (Mehta–Patel network algorithm port)
' These tests compute exact reference values by enumerating all contingency tables
' with fixed margins (small examples), and compare PObserved and PValue.

<TestClass()>
Public Class FisherExactEngine_Tests

    Private Const TOL As Double = 0.000000000001

    ' ---------- Helpers (independent of production code) ----------

    Private Shared Function LogFact(n As Integer) As Double
        If n < 2 Then Return 0.0#
        Dim s As Double = 0.0#
        For k As Integer = 2 To n
            s += Math.Log(k)
        Next
        Return s
    End Function

    Private Shared Function RowSums(t(,) As Integer) As Integer()
        Dim r As Integer = UBound(t, 1)
        Dim c As Integer = UBound(t, 2)
        Dim rs(r) As Integer
        For i As Integer = 0 To r
            Dim s As Integer = 0
            For j As Integer = 0 To c
                s += t(i, j)
            Next
            rs(i) = s
        Next
        Return rs
    End Function

    Private Shared Function ColSums(t(,) As Integer) As Integer()
        Dim r As Integer = UBound(t, 1)
        Dim c As Integer = UBound(t, 2)
        Dim cs(c) As Integer
        For j As Integer = 0 To c
            Dim s As Integer = 0
            For i As Integer = 0 To r
                s += t(i, j)
            Next
            cs(j) = s
        Next
        Return cs
    End Function

    Private Shared Function TotalSum(t(,) As Integer) As Integer
        Dim r As Integer = UBound(t, 1)
        Dim c As Integer = UBound(t, 2)
        Dim s As Integer = 0
        For i As Integer = 0 To r
            For j As Integer = 0 To c
                s += t(i, j)
            Next
        Next
        Return s
    End Function

    Private Shared Function LogProbTable(t(,) As Integer, rs() As Integer, cs() As Integer, nTot As Integer, logConst As Double) As Double
        ' log P = logConst - sum log(cell!)
        Dim r As Integer = UBound(t, 1)
        Dim c As Integer = UBound(t, 2)
        Dim s As Double = 0.0#
        For i As Integer = 0 To r
            For j As Integer = 0 To c
                s += LogFact(t(i, j))
            Next
        Next
        Return logConst - s
    End Function

    Private Shared Sub EnumerateTables(ByVal r As Integer,
                                      ByVal c As Integer,
                                      ByVal i As Integer,
                                      ByVal j As Integer,
                                      ByVal cur(,) As Integer,
                                      ByVal rowRem() As Integer,
                                      ByVal colRem() As Integer,
                                      ByVal logConst As Double,
                                      ByVal nTot As Integer,
                                      ByVal logPObs As Double,
                                      ByRef pSum As Double)
        ' Recursively enumerate all nonnegative integer tables with given margins.
        If i = r AndAlso j = c Then
            ' completed (should not hit)
            Return
        End If

        If i = r - 1 AndAlso j = c - 1 Then
            Dim v As Integer = rowRem(i)
            If v <> colRem(j) Then Return
            cur(i, j) = v
            Dim lp As Double = LogProbTable(cur, Nothing, Nothing, nTot, logConst)
            If lp <= logPObs + 0.000000000000001 Then
                pSum += Math.Exp(lp)
            End If
            Return
        End If

        If j = c - 1 Then
            ' last column in row i fixed
            Dim v As Integer = rowRem(i)
            If v > colRem(j) Then Return
            cur(i, j) = v
            Dim savedCol As Integer = colRem(j)
            colRem(j) -= v
            rowRem(i) = 0
            EnumerateTables(r, c, i + 1, 0, cur, rowRem, colRem, logConst, nTot, logPObs, pSum)
            ' backtrack
            rowRem(i) = v
            colRem(j) = savedCol
            Return
        End If

        If i = r - 1 Then
            ' last row: value fixed by remaining column sums
            Dim v As Integer = colRem(j)
            If v > rowRem(i) Then Return
            cur(i, j) = v
            Dim savedRow As Integer = rowRem(i)
            rowRem(i) -= v
            colRem(j) = 0
            EnumerateTables(r, c, i, j + 1, cur, rowRem, colRem, logConst, nTot, logPObs, pSum)
            ' backtrack
            rowRem(i) = savedRow
            colRem(j) = v
            Return
        End If

        Dim maxVal As Integer = Math.Min(rowRem(i), colRem(j))
        Dim savedRow2 As Integer = rowRem(i)
        Dim savedCol2 As Integer = colRem(j)

        For v As Integer = 0 To maxVal
            cur(i, j) = v
            rowRem(i) = savedRow2 - v
            colRem(j) = savedCol2 - v
            EnumerateTables(r, c, i, j + 1, cur, rowRem, colRem, logConst, nTot, logPObs, pSum)
        Next

        ' backtrack
        rowRem(i) = savedRow2
        colRem(j) = savedCol2
    End Sub

    Private Shared Sub ComputeExactFisherReference(t(,) As Integer, ByRef pObserved As Double, ByRef pValue As Double)
        Dim rs() As Integer = RowSums(t)
        Dim cs() As Integer = ColSums(t)
        Dim nTot As Integer = TotalSum(t)

        If nTot = 0 Then
            pObserved = Double.NaN
            pValue = Double.NaN
            Return
        End If

        Dim logConst As Double = 0.0#
        For Each v As Integer In rs
            logConst += LogFact(v)
        Next
        For Each v As Integer In cs
            logConst += LogFact(v)
        Next
        logConst -= LogFact(nTot)

        Dim logPObs As Double = LogProbTable(t, rs, cs, nTot, logConst)
        pObserved = Math.Exp(logPObs)

        Dim r As Integer = rs.Length
        Dim c As Integer = cs.Length

        Dim cur(r - 1, c - 1) As Integer
        Dim rowRem(r - 1) As Integer
        Dim colRem(c - 1) As Integer
        Array.Copy(rs, rowRem, r)
        Array.Copy(cs, colRem, c)

        Dim sumP As Double = 0.0#
        EnumerateTables(r, c, 0, 0, cur, rowRem, colRem, logConst, nTot, logPObs, sumP)
        pValue = sumP
    End Sub

    ' ---------- Tests ----------

    <TestMethod>
    Public Sub FisherExact_2x2_matches_exact_enumeration()
        Dim t(,) As Integer = {{1, 9},
                               {11, 3}}

        Dim expObs As Double, expP As Double
        ComputeExactFisherReference(t, expObs, expP)

        Dim eng As New contingencytable.FisherExactEngine(t)
        eng.Run()

        Assert.AreEqual(expObs, eng.PObserved, 0.000000001, "PObserved mismatch.")
        Assert.AreEqual(expP, eng.PValue, TOL, "PValue mismatch.")
    End Sub

    <TestMethod>
    Public Sub FisherExact_2x3_matches_exact_enumeration()
        ' Small table so enumeration is fast
        Dim t(,) As Integer = {{1, 2, 1},
                               {0, 1, 2}}

        Dim expObs As Double, expP As Double
        ComputeExactFisherReference(t, expObs, expP)

        Dim eng As New contingencytable.FisherExactEngine(t)
        eng.Run()

        Assert.AreEqual(expObs, eng.PObserved, 0.0000001, "PObserved mismatch.")
        Assert.AreEqual(expP, eng.PValue, TOL, "PValue mismatch.")
    End Sub

    <TestMethod>
    Public Sub FisherExact_3x3_matches_exact_enumeration()
        ' Small 3x3 example (kept tiny to keep enumeration feasible)
        Dim t(,) As Integer = {{1, 0, 1},
                               {0, 2, 0},
                               {1, 0, 1}}

        Dim expObs As Double, expP As Double
        ComputeExactFisherReference(t, expObs, expP)

        Dim eng As New contingencytable.FisherExactEngine(t)
        eng.Run()

        Assert.AreEqual(expObs, eng.PObserved, 0.0000001, "PObserved mismatch.")
        Assert.AreEqual(expP, eng.PValue, TOL, "PValue mismatch.")
    End Sub

    <TestMethod>
    Public Sub FisherExact_all_zero_table_returns_NaN()
        Dim t(,) As Integer = {{0, 0},
                               {0, 0}}

        Dim eng As New contingencytable.FisherExactEngine(t)
        eng.Run()

        Assert.IsTrue(Double.IsNaN(eng.PObserved), "Expected PObserved NaN for all-zero table.")
        Assert.IsTrue(Double.IsNaN(eng.PValue), "Expected PValue NaN for all-zero table.")
    End Sub

    <TestMethod>
    Public Sub FisherExact_negative_entry_throws()
        Dim t(,) As Integer = {{1, -1},
                               {0, 1}}
        Assert.ThrowsException(Of ArgumentException)(
            Sub()
                Dim eng As New contingencytable.FisherExactEngine(t)
                eng.Run()
            End Sub)
    End Sub

    <TestMethod>
    Public Sub FisherExact_FEXACT_Clarkson_5x3_case1()
        Dim t(,) As Integer = {{24, 7, 3, 8, 1}, {9, 5, 5, 0, 3}, {2, 0, 2, 0, 1}}
        Dim expectedP As Double = 0.01993  ' from Clarkson (1993)

        Dim eng As New contingencytable.FisherExactEngine(t)
        eng.Run()

        Assert.AreEqual(expectedP, eng.PValue, 0.000001)
    End Sub

    <TestMethod>
    Public Sub FisherExact_FEXACT_Clarkson_8x2_case1()
        Dim t(,) As Integer = {{22, 13, 5, 4, 5, 3, 2, 1}, {7, 1, 4, 3, 1, 2, 3, 4}}
        Dim expectedP As Double = 0.035954  ' from Clarkson (1993)

        Dim eng As New contingencytable.FisherExactEngine(t)
        eng.Run()

        Assert.AreEqual(expectedP, eng.PValue, 0.000001)
    End Sub

    <TestMethod>
    Public Sub FisherExact_FEXACT_Clarkson_7x3_case1()
        Dim t(,) As Integer = {{1, 8, 5, 4, 4, 2, 2}, {5, 3, 3, 4, 3, 1, 0}, {10, 1, 4, 0, 0, 0, 0}}
        Dim expectedP As Double = 0.00355998  ' from Clarkson (1993)

        Dim eng As New contingencytable.FisherExactEngine(t)
        eng.Run()

        Assert.AreEqual(expectedP, eng.PValue, 0.000001)
    End Sub

    <TestMethod>
    Public Sub FisherExact_Clarkson1993_TableI_case11_3x6()
        ' Contingency table (3 x 6):
        Dim t(,) As Integer = {
            {12, 6, 12, 1, 1, 0},
            {5, 12, 4, 4, 0, 1},
            {5, 12, 10, 1, 1, 0}
        }
        ' Published exact p-value = 0.049480 (Table I)
        Const expectedP As Double = 0.04948
        Const tolP As Double = 0.000001

        Dim eng As New contingencytable.FisherExactEngine(t)
        eng.Run()

        Assert.AreEqual(expectedP, eng.PValue, tolP, "PValue mismatch vs Clarkson (1993) Table I.")
    End Sub

    <TestMethod>
    Public Sub FisherExact_Clarkson1993_TableI_case6_2x5_with_multidigit()
        ' Contingency table (2 x 5), includes multi-digit cells (11, 10, 11):
        Dim t(,) As Integer = {
            {2, 3, 4, 8, 9},
            {0, 0, 11, 10, 11}
        }
        ' Published exact p-value = 0.085524 (Table I)
        Const expectedP As Double = 0.085524
        Const tolP As Double = 0.000001

        Dim eng As New contingencytable.FisherExactEngine(t)
        eng.Run()

        Assert.AreEqual(expectedP, eng.PValue, tolP, "PValue mismatch vs Clarkson (1993) Table I.")
    End Sub

    <TestMethod>
    Public Sub FisherExact_Clarkson1993_TableI_case2_2x10()
        ' Contingency table (2 x 10):
        Dim t(,) As Integer = {
            {20, 3, 6, 4, 7, 6, 6, 2, 2, 2},
            {8, 8, 4, 5, 2, 1, 0, 2, 1, 1}
        }
        ' Published exact p-value = 0.082538 (Table I)
        Const expectedP As Double = 0.082538
        Const tolP As Double = 0.000001

        Dim eng As New contingencytable.FisherExactEngine(t)
        eng.Run()

        Assert.AreEqual(expectedP, eng.PValue, tolP, "PValue mismatch vs Clarkson (1993) Table I.")
    End Sub
End Class
