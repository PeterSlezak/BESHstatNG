Option Explicit On

Option Strict Off

Imports System

Imports System.Collections.Generic

Imports BESHStatNG

' Frozen source projection of main 4a3d2e1, before extraction. No native/scientific reference claim.

Friend NotInheritable Class LegacyAssumption06Characterization

    Public Shared Function RunNormality(data As MultiGroupsUnpairedData) As ResultTable
        Dim results(,) As Object, sw_r(,) As Object, da_r(,) As Object, ad_r(,) As Object
        Dim res = New List(Of ResultTable)
        Dim strErr As String = Nothing
        Dim rTable = New ResultTable

        results = {{"Shapiro-Wilk test"}, {"Test statistics"}, {"Two-sided P-value"},
                   {"D'Agostino-Pearson K2 test"}, {"Test statistics"}, {"Two-sided P-value"},
                   {"Anderson Darling test"}, {"Test statistics"}, {"Two-sided P-value"}}
        For i = 0 To data.X.Length - 1
            If data.X(i).Length > 3 And data.X(i).Length < 5000 Then
                Dim sw_res = assumptions.ShapiroWilk(data.X(i), strErr)
                sw_r = {{""}, {sw_res.TestStatistics1}, {sw_res.Pvalue}}
            Else
                AppInfrastructure.CoreServices.Log("N not in range between 3 and 5000")
                sw_r = {{"NA n<4 or n>5000"}, {"NA n<4 or n>5000"}, {"NA n<4 or n>5000"}}
            End If


            If data.X(i).Length >= 9 Then
                Dim da_res = assumptions.DAgostino(data.X(i), strErr)
                da_r = {{""}, {da_res.TestStatistics1}, {da_res.Pvalue}}
            Else
                AppInfrastructure.CoreServices.Log("N not >= 9")
                da_r = {{"NA n<9"}, {"NA n<9"}, {"NA n<9"}}
            End If

            If data.X(i).Length > 1 Then
                Dim ad_res = assumptions.AndersonDarlingTEST(data.X(i))
                ad_r = {{""}, {ad_res.TestStatistics1}, {ad_res.Pvalue}}
            Else
                AppInfrastructure.CoreServices.Log("N not > 1")
                ad_r = {{"NA"}, {"NA"}, {"NA"}}
            End If

            results = Matrix.VerticalStackArrays(results, Matrix.HorizontalStackArrays(Matrix.HorizontalStackArrays(sw_r, da_r), ad_r))
        Next
        rTable.SetBody(results)
        For i = 0 To data.X.Length - 1
            rTable.AddPvalueCellToFormat(3, i + 2)
            rTable.AddPvalueCellToFormat(6, i + 2)
            rTable.AddPvalueCellToFormat(9, i + 2)
        Next
        rTable.AddHeaderTopRow(Global.BESHStatNG.DataManagement.ArrayUtilities.ConcatArrays({"Normality Tests"}, data.varNames))
        Return rTable
    End Function

    Public Shared Function RunOutliers(data As MultiGroupsUnpairedData, grubbs As Boolean, rosner As Boolean, alpha As Decimal) As ResultTable
        Dim res = New List(Of ResultTable), out(,) As Object = Nothing
        Dim outlierTable = New ResultTable

        Dim k = data.X.Length
        If grubbs Then
            outlierTable.AddHeaderLeftRow({"Sample", "Alpha", "Critical Test Statistic", "Sample Test Statistic", "Result"})
            ReDim out(3, k - 1)
        ElseIf rosner Then
            outlierTable.AddHeaderLeftRow({"Sample", "Alpha", "Number of Outliers"})
            ReDim out(12, k - 1) 'rosner can detect up to 10 outliers + two rows for "number of outliers information" and additional text
        End If

        Dim nMaxOutliers As Integer = 0
        For i = 0 To k - 1
            If grubbs Then
                Dim t = assumptions.Grubbs(data.X(i), alpha)
                out(0, i) = alpha
                out(1, i) = t.TestStatistics1
                out(2, i) = t.TestStatistics2
                out(3, i) = t.strSpecialInformation

            ElseIf rosner Then

                Dim t = assumptions.Rosner(data.X(i), alpha)

                out(0, i) = alpha
                out(2, i) = "List of Outliers:"
                If t Is Nothing Then
                    out(1, i) = 0
                Else
                    out(1, i) = t.Length
                    For j = 0 To t.Length - 1
                        out(j + 3, i) = t(j)
                    Next
                End If
                nMaxOutliers = Math.Max(nMaxOutliers, out(1, i))
            End If
        Next

        outlierTable.SetBody(out)
        outlierTable.AddHeaderTopRow(data.varNames)
        outlierTable.bLeftHeaderAdjustUp = True
        Return outlierTable
    End Function

    Public Shared Function RunSymmetry(data As MultiGroupsUnpairedData, mgg As Boolean) As ResultTable
        Dim res = New List(Of ResultTable), out(,) As Object
        Dim asymTable = New ResultTable

        Dim strTest As String = If(mgg, "Miao-Gel-Gastwirth", "Cabilio-Masaro")
        Dim k = data.X.Length
        ReDim out(1, k - 1)
        For i = 0 To k - 1
            Dim t = assumptions.SymmetryTest(data.X(i), strTest)
            out(0, i) = t.TestStatistics1
            out(1, i) = t.Pvalue
        Next
        asymTable.SetBody(out)
        For i = 0 To k - 1
            asymTable.AddPvalueCellToFormat(2, i + 1)
        Next
        Dim tmp(UBound(data.varNames)) As String
        tmp(0) = strTest
        asymTable.AddHeaderTopRow(tmp)
        asymTable.AddHeaderTopRow(data.varNames)
        asymTable.AddHeaderLeftRow({"Asymetry test", "Test Statistic", "Two-sided P-value"})
        Return asymTable
    End Function

    Public Shared Function ComputeVarianceHomogeneity(data As MultiGroupsUnpairedData, fligner As Boolean, levene As Boolean, squared As Boolean, bartlett As Boolean) As ResultTable
        Dim varResult As TestResult, o(,) As Object = Nothing
        Dim t = New ResultTable

        If bartlett Or fligner Or levene Or squared Then
            If fligner Then
                varResult = assumptions.FlignerKilleenTEST(data.X)
                o = {{"Fligner-Killeen", ""}, {"Test statistics", varResult.TestStatistics1}, {"P-value", varResult.Pvalue}}
            End If
            If levene Then
                varResult = assumptions.LeveneTEST(data.X, True)
                If o Is Nothing Then
                    o = {{"Levene", ""}, {"Test statistics", varResult.TestStatistics1}, {"P-value", varResult.Pvalue}}
                Else
                    o = Matrix.HorizontalStackArrays(o, {{"Levene", ""}, {"Test statistics", varResult.TestStatistics1}, {"P-value", varResult.Pvalue}})
                End If
            End If
            If squared Then
                varResult = assumptions.SquaredRanksTestVARIANCE(data.X)
                If o Is Nothing Then
                    o = {{"Squared Ranks", ""}, {"Test statistics", varResult.TestStatistics1}, {"P-value", varResult.Pvalue}}
                Else
                    o = Matrix.HorizontalStackArrays(o, {{"Squared Ranks", ""}, {"Test statistics", varResult.TestStatistics1}, {"P-value", varResult.Pvalue}})
                End If
            End If

            If bartlett Then
                varResult = assumptions.BartlettTEST(data.X)
                If o Is Nothing Then
                    o = {{"Bartlett", ""}, {"Test statistics", varResult.TestStatistics1}, {"P-value", varResult.Pvalue}}
                Else
                    o = Matrix.HorizontalStackArrays(o, {{"Bartlett", ""}, {"Test statistics", varResult.TestStatistics1}, {"P-value", varResult.Pvalue}})
                End If
            End If
            t.AddHeaderTopRow({"Homogeneity of Variances", ""})
            t.SetBody(o)
            For rowIndex = 0 To o.GetLength(0) - 1
                If String.Equals(CStr(o(rowIndex, 0)), "P-value", StringComparison.OrdinalIgnoreCase) Then
                    t.AddPvalueCellToFormat(rowIndex + 1, 2)
                End If
            Next
        End If

                Return t
    End Function

    Public Shared Function ComputeDescriptiveStats(data As MultiGroupsUnpairedData, Optional statsToReturn As List(Of String) = Nothing) As ResultTable
        Dim descTable = New ResultTable
        Dim ds1 = New DescriptiveStat(data.X(0))
        ds1.compute()
        Dim tableBody = ds1.wrapSelf(True, statsToReturn)
        For i = 1 To data.X.Length - 1
            Dim ds2 = New DescriptiveStat(data.X(i))
            ds2.compute()
            tableBody = Matrix.VerticalStackArrays(tableBody, ds2.wrapSelf(False, statsToReturn))
        Next
        descTable.SetBody(tableBody)
        For rowIndex = 0 To tableBody.GetLength(0) - 1
            If String.Equals(CStr(tableBody(rowIndex, 0)), "Two-sided p-value", StringComparison.OrdinalIgnoreCase) Then
                For columnIndex = 1 To tableBody.GetLength(1) - 1
                    descTable.AddPvalueCellToFormat(rowIndex + 1, columnIndex + 1)
                Next
            End If
        Next
        descTable.AddTitle("Descriptive Statistics")
        descTable.AddHeaderTopRow(Global.BESHStatNG.DataManagement.ArrayUtilities.ConcatArrays({""}, data.varNames))

        Return descTable
    End Function

End Class
