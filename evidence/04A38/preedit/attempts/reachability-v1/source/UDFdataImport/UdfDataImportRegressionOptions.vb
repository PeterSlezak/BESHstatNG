Option Explicit On
Option Strict On

Imports System
Imports System.Collections.Generic

' Regression option and response-shape import helpers for worksheet UDFs.
' Keeps family/link/reference parsing and categorical-response validation behind the shared UdfDataImport facade.
Partial Friend Module UdfDataImport

    ''' <summary>
    ''' Parses the GLM/GEE family argument into the canonical display code expected by the model layer.
    ''' </summary>
    Friend Function GetRegressionFamilyCode(arg As Object) As String
        Global.BESHStatNG.UdfTrace38.Hit("BESHstatNG/src/ExcelUDFs/UDFdataImport/UdfDataImportRegressionOptions.vb|BESHStatNG.UdfDataImport|FunctionBlock|a2960f8b44479538@471:794")
        Dim s As String = Global.BESHStatNG.WorksheetFunctions.ExcelArgReaders.AsString(arg)
        If String.IsNullOrWhiteSpace(s) Then Return "Gaussian"

        Select Case NormalizeRegressionOptionKey(s)
            Case "binomial", "binary", "logistic"
                Return "Binomial"
            Case "poisson", "count"
                Return "Poisson"
            Case "negativebinomial", "negativebinom", "negativebin", "negbin", "nb", "nb2"
                Return "NegativeBinomial"
            Case "gaussian", "normal"
                Return "Gaussian"
            Case "gamma"
                Return "Gamma"
            Case Else
                Return Nothing
        End Select
    End Function

    ''' <summary>
    ''' Parses a GLM/GEE link argument into the canonical link name expected by the model layer.
    ''' </summary>
    Friend Function GetRegressionLinkName(arg As Object, familyDisplayName As String) As String
        Global.BESHStatNG.UdfTrace38.Hit("BESHstatNG/src/ExcelUDFs/UDFdataImport/UdfDataImportRegressionOptions.vb|BESHStatNG.UdfDataImport|FunctionBlock|a4e37404006b505f@1410:923")
        Dim s As String = Global.BESHStatNG.WorksheetFunctions.ExcelArgReaders.AsString(arg)
        If String.IsNullOrWhiteSpace(s) Then
            Return regression.GetCanonicalLinkFromDisplayName(familyDisplayName)
        End If

        Select Case NormalizeRegressionOptionKey(s)
            Case "logit"
                Return "Logit"
            Case "probit"
                Return "Probit"
            Case "log"
                Return "Log"
            Case "identity", "id"
                Return "Identity"
            Case "sqrt", "squareroot"
                Return "Sqrt"
            Case "inverse", "reciprocal"
                Return "Inverse"
            Case "power"
                Return "Power"
            Case Else
                Return Nothing
        End Select
    End Function

    ''' <summary>
    ''' Counts distinct integer response categories in the first column of a fitted regression data matrix.
    ''' </summary>
    Friend Function CountDistinctOutcomeCategories(fitData(,) As Double) As Integer
        Global.BESHStatNG.UdfTrace38.Hit("BESHstatNG/src/ExcelUDFs/UDFdataImport/UdfDataImportRegressionOptions.vb|BESHStatNG.UdfDataImport|FunctionBlock|e5543fdb717ba6d1@2489:377")
        If fitData Is Nothing Then Return 0

        Dim n As Integer = fitData.GetLength(0)
        Dim cats As New HashSet(Of Integer)()
        For i As Integer = 0 To n - 1
            cats.Add(CInt(Math.Round(fitData(i, 0))))
        Next

        Return cats.Count
    End Function

    ''' <summary>
    ''' Parses the reference-category option supplied to multinomial/ordinal logit fit functions.
    ''' </summary>
    Friend Function GetReferenceCategory(arg As Object) As regression.ReferenceCategory
        Global.BESHStatNG.UdfTrace38.Hit("BESHstatNG/src/ExcelUDFs/UDFdataImport/UdfDataImportRegressionOptions.vb|BESHStatNG.UdfDataImport|FunctionBlock|f9047c8fd46bb8ca@3012:538")
        Dim s As String = Global.BESHStatNG.WorksheetFunctions.ExcelArgReaders.AsString(arg)
        If String.IsNullOrWhiteSpace(s) Then Return regression.ReferenceCategory.Last

        Select Case s.Trim().ToLowerInvariant()
            Case "first", "smallest", "min"
                Return regression.ReferenceCategory.First
            Case Else
                Return regression.ReferenceCategory.Last
        End Select
    End Function

    Private Function NormalizeRegressionOptionKey(value As String) As String
        Global.BESHStatNG.UdfTrace38.Hit("BESHstatNG/src/ExcelUDFs/UDFdataImport/UdfDataImportRegressionOptions.vb|BESHStatNG.UdfDataImport|FunctionBlock|9ffb0f0c5deb7bfe@3558:372")
        If value Is Nothing Then Return String.Empty

        Dim chars As New List(Of Char)()
        For Each ch As Char In value.Trim().ToLowerInvariant()
            If Char.IsLetterOrDigit(ch) Then chars.Add(ch)
        Next

        Return New String(chars.ToArray())
    End Function

End Module