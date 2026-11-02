Option Explicit On

Option Strict Off

Imports System

Imports System.Collections.Generic

Imports BESHStatNG

' Frozen main b984e20 source projection; no native or independent scientific claim.

Friend NotInheritable Class LegacyAnovaWorkflow07

    Public Shared Function Repeated(data As MultiGroupsPairedData, alpha As Double, gg As Boolean, hf As Boolean, tukey As Boolean, mauchly As Boolean, descriptive As Boolean) As List(Of ResultTable)
        Dim Mtest As TestResult, Mout As ResultTable

        Dim anova = New parametric.OneWayRmANOVA(data.X, data.varNames)
        anova.compute()
        If gg Then anova.GreenhouseGeisser()
        If hf Then anova.HuyhnFeldt()
        If tukey Then
            anova.TukeyKramerRM2(alpha) 'no sphericity asumption
            anova.Tukey(alpha)
        End If
        Dim res = anova.wrapResults()

        If mauchly Then
            Mtest = assumptions.MauchlyTest(data.X)
            Mout = New ResultTable
            Mout.SetBody({{"Chi2", Mtest.TestStatistics1}, {"P-value", Mtest.Pvalue}})
            Mout.AddHeaderTopRow({"Mauchly 's Test of Sphericity", ""})
            Mout.AddPvalueCellToFormat(2, 2)
            res.Add(Mout)
        End If

        'Compute descriptive statistics
        If descriptive Then res.Add(ComputeDescriptiveStatistics(data))

        Return res
    End Function

    Public Shared Function ComputeDescriptiveStatistics(data As MultiGroupsPairedData) As ResultTable
        Dim descTable = New ResultTable
        Dim ds1 = New DescriptiveStat(Matrix.GetColumnFrom2Darray(data.X, 0))
        ds1.compute()
        Dim tableBody = ds1.wrapSelf(True)
        For i = 1 To UBound(data.X, 2)
            Dim ds2 = New DescriptiveStat(Matrix.GetColumnFrom2Darray(data.X, i))
            ds2.compute()
            tableBody = Matrix.VerticalStackArrays(tableBody, ds2.wrapSelf(False))
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

    Public Shared Function Nested(d As MultiGroupsPairedDataObj) As Tuple(Of List(Of ResultTable), Boolean)
        Dim nest = New parametric.TwoWayNestedANOVA(d.X, d.varNames)
        nest.compute()
        Dim res = nest.wrapResults()
        Return Tuple.Create(res, nest.balancedDesign)
    End Function

End Class
