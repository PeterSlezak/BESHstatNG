Option Explicit On
Option Strict On
Option Infer On

Imports System
Imports System.Collections.Generic
Imports System.Globalization

Namespace ExcelCommon
    Public NotInheritable Class UdfCoxInputPreparation38
        Private Sub New()
        End Sub

        Public Shared Function PrepareRecords(imported As CoxPHData, rowCount As Integer, fitColCount As Integer, fitX As Double(,)) As List(Of survival.SurvivalRecord)
            Dim records As New List(Of survival.SurvivalRecord)(rowCount)
            For i As Integer = 0 To rowCount - 1
                Dim rec As New survival.SurvivalRecord()
                rec.Time = imported.TimeData(i)
                rec.Censorship = imported.CensorData(i)
                rec.Index = imported.RowIds(i)

                Dim cov(fitColCount - 1) As Double
                For j As Integer = 0 To fitColCount - 1
                    cov(j) = fitX(i, j)
                Next
                rec.Covariates = cov

                If imported.bStrata Then
                    rec.Stratum = imported.StrataData(i)
                    rec.strStratum = imported.StrataData(i)
                Else
                    rec.Stratum = "__ALL__"
                    rec.strStratum = "__ALL__"
                End If

                records.Add(rec)
            Next
            Return records
        End Function
    End Class
End Namespace
