Option Explicit On
Option Strict On
Option Infer On

Imports System
Imports System.Collections.Generic

Namespace ExcelCommon
    Public NotInheritable Class UdfLmmReportPreparation38
        Private Sub New()
        End Sub

        Public Shared Sub ApplyHandleNames(result As regression.MixedModelResult, fixedEffectNames As String(), randomEffectNames As String())
            If fixedEffectNames IsNot Nothing AndAlso fixedEffectNames.Length > 0 Then
                result.FixedEffectNames = CType(fixedEffectNames.Clone(), String())
            End If
            If randomEffectNames IsNot Nothing AndAlso randomEffectNames.Length > 0 Then
                result.RandomCovarianceLabels = CType(randomEffectNames.Clone(), String())
            End If
        End Sub

        Public Shared Function BuildFittedTable(fitted As Double(), hdr As Boolean) As Object(,)
            Dim out(fitted.Length - 1 + If(hdr, 1, 0), 1) As Object
            Dim r As Integer = 0

            If hdr Then
                out(0, 0) = "Row"
                out(0, 1) = "Fitted"
                r = 1
            End If

            For i As Integer = 0 To fitted.Length - 1
                out(r + i, 0) = i + 1
                out(r + i, 1) = fitted(i)
            Next
            Return out
        End Function

        Public Shared Function BuildResidualTable(fitted As Double(), resid As Double(), n As Integer, hdr As Boolean) As Object(,)
            Dim out(n - 1 + If(hdr, 1, 0), 2) As Object
            Dim r As Integer = 0
            If hdr Then
                out(0, 0) = "Row"
                out(0, 1) = "Fitted"
                out(0, 2) = "Residual"
                r = 1
            End If

            For i As Integer = 0 To n - 1
                out(r + i, 0) = i + 1
                out(r + i, 1) = If(i < fitted.Length, CType(fitted(i), Object), String.Empty)
                out(r + i, 2) = If(i < resid.Length, CType(resid(i), Object), String.Empty)
            Next
            Return out
        End Function

        Public Shared Function ObservationCount(values As Double()) As Integer
            Return If(values Is Nothing, 0, values.Length)
        End Function
    End Class
End Namespace
