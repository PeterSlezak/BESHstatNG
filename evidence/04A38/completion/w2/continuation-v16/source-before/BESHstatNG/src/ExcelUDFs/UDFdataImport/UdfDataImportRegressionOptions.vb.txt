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
        Return Global.BESHStatNG.ExcelCommon.UdfDataImportOptions38.GetRegressionFamilyCode(Global.BESHStatNG.WorksheetFunctions.ExcelArgReaders.AsString(arg))
    End Function

    ''' <summary>
    ''' Parses a GLM/GEE link argument into the canonical link name expected by the model layer.
    ''' </summary>
    Friend Function GetRegressionLinkName(arg As Object, familyDisplayName As String) As String
        Return Global.BESHStatNG.ExcelCommon.UdfDataImportOptions38.GetRegressionLinkName(Global.BESHStatNG.WorksheetFunctions.ExcelArgReaders.AsString(arg), familyDisplayName)
    End Function

    ''' <summary>
    ''' Counts distinct integer response categories in the first column of a fitted regression data matrix.
    ''' </summary>
    Friend Function CountDistinctOutcomeCategories(fitData(,) As Double) As Integer
        Return Global.BESHStatNG.WorksheetCompatibility.UdfNumericPreparation38.CountDistinctOutcomeCategories(fitData)
    End Function

    ''' <summary>
    ''' Parses the reference-category option supplied to multinomial/ordinal logit fit functions.
    ''' </summary>
    Friend Function GetReferenceCategory(arg As Object) As regression.ReferenceCategory
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
        Return Global.BESHStatNG.ExcelCommon.UdfDataImportOptions38.NormalizeRegressionOptionKey(value)
    End Function

End Module