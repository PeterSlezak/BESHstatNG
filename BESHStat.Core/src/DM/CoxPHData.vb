Option Explicit On
Option Strict On
Option Infer On

Imports System
Imports System.Collections.Generic
Imports System.Globalization
Imports BESHStatNG.DataManagement

''' <summary>
''' Specialized host-neutral data container for Cox proportional hazards models.
''' </summary>
Public Class CoxPHData
    Inherits DataObj

    Public bStrata As Boolean
    Public StrataData() As String
    Public CensorData() As Integer
    Public TimeData() As Double
    Public StrataVarName As String
    Public CensorVarName As String
    Public TimeVarName As String
    Public SurvRecordsList As New List(Of survival.SurvivalRecord)()

    Public Sub New()
        MyBase.New()
        Me.bStrata = False
    End Sub

    Public Overrides Sub DataImportRawMatrix(rawInput(,) As Object,
                                             variableNames() As String,
                                             Optional firstSourceRow As Integer = 1,
                                             Optional sourceInfo As DataSourceInfo = Nothing,
                                             Optional CharCols As Integer = -1,
                                             Optional SkipRow As Integer = 0)
        Dim effectiveCharCols As Integer = CharCols
        If Me.bStrata AndAlso effectiveCharCols < 2 Then effectiveCharCols = 2

        MyBase.DataImportRawMatrix(rawInput,
                                   variableNames,
                                   firstSourceRow,
                                   sourceInfo,
                                   effectiveCharCols,
                                   SkipRow)
    End Sub

    Protected Overrides Sub OnDataImported()
        If Me.bZeroValid OrElse Me.FinalData Is Nothing Then Return
        FinalizeCoxImport()
    End Sub

    Private Shared Function RequireDouble(value As Object, fieldName As String) As Double
        Dim result As Double
        If Not CoreDataTable.TryConvertToDouble(value, result) Then
            Throw New ArgumentException($"{fieldName} must be numeric.")
        End If
        Return result
    End Function

    Private Sub FinalizeCoxImport()
        ReDim TimeData(Me.nRows - 1)
        ReDim CensorData(Me.nRows - 1)

        Me.TimeVarName = Me.varNames(0)
        Me.CensorVarName = Me.varNames(1)

        For i As Integer = 0 To Me.nRows - 1
            Me.TimeData(i) = RequireDouble(Me.FinalData(i, 0), "Time")

            Dim censorValue As Double = RequireDouble(Me.FinalData(i, 1), "Censoring value")
            If censorValue <> 0.0 AndAlso censorValue <> 1.0 Then
                Throw New ArgumentException($"Censorting value is not 1/0. Value ={Convert.ToString(Me.FinalData(i, 1), CultureInfo.CurrentCulture)}")
            End If
            Me.CensorData(i) = If(censorValue = 0.0, 0, 1)
        Next

        If Me.bStrata Then
            ReDim Me.StrataData(Me.nRows - 1)
            Me.StrataVarName = Me.varNames(2)

            For i As Integer = 0 To Me.nRows - 1
                Me.StrataData(i) = Convert.ToString(Me.FinalData(i, 2), CultureInfo.CurrentCulture)
            Next

            For i As Integer = 0 To Me.nRows - 1
                For j As Integer = 3 To Me.nCols - 1
                    Me.FinalData(i, j - 3) = Me.FinalData(i, j)
                    If i = 0 Then Me.varNames(j - 3) = Me.varNames(j)
                Next
            Next

            ReDim Preserve Me.FinalData(Me.nRows - 1, Me.nCols - 4)
            ReDim Preserve Me.varNames(Me.nCols - 4)
            Me.nCols -= 3
        Else
            For i As Integer = 0 To Me.nRows - 1
                For j As Integer = 2 To Me.nCols - 1
                    Me.FinalData(i, j - 2) = Me.FinalData(i, j)
                    If i = 0 Then Me.varNames(j - 2) = Me.varNames(j)
                Next
            Next

            ReDim Preserve Me.FinalData(Me.nRows - 1, Me.nCols - 3)
            ReDim Preserve Me.varNames(Me.nCols - 3)
            Me.nCols -= 2
        End If

        SurvRecordsList.Clear()
        For i As Integer = 0 To Me.nRows - 1
            Dim covariates(Me.nCols - 1) As Double
            For j As Integer = 0 To Me.nCols - 1
                covariates(j) = RequireDouble(Me.FinalData(i, j), $"Covariate {j + 1}")
            Next

            Dim sr As New survival.SurvivalRecord With {
                .Censorship = Me.CensorData(i),
                .Stratum = If(Me.bStrata, Me.StrataData(i), "0"),
                .Time = Me.TimeData(i),
                .Index = Me.RowIds(i),
                .covariates = covariates
            }
            SurvRecordsList.Add(sr)
        Next
    End Sub
End Class
