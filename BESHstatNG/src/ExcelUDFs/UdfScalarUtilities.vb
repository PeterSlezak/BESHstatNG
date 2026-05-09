Option Explicit On
Option Strict On

Imports BESHStatNG.WorksheetFunctions

Module UdfScalarUtilities

    ''' <summary>
    ''' Normalizes a string for token or key matching by trimming whitespace, optionally changing case,
    ''' and removing selected separator characters.
    ''' </summary>
    Friend Function NormalizeMatchToken(value As String, Optional toUpper As Boolean = True, Optional removeUnderscore As Boolean = False) As String
        If value Is Nothing Then Return String.Empty

        Dim normalized As String = value.Trim()
        normalized = If(toUpper, normalized.ToUpperInvariant(), normalized.ToLowerInvariant())
        normalized = normalized.Replace(" ", String.Empty).Replace("-", String.Empty)
        If removeUnderscore Then normalized = normalized.Replace("_", String.Empty)
        Return normalized
    End Function

    ''' <summary>
    ''' Normalizes an optional text argument for case-insensitive method matching.
    ''' </summary>
    Friend Function NormalizeText(v As Object) As String
        Return NormalizeMatchToken(AsString(v), toUpper:=True, removeUnderscore:=False)
    End Function

    ''' <summary>
    ''' Normalizes an optional token-style worksheet argument for case-insensitive option parsing.
    ''' </summary>
    Friend Function NormalizeToken(arg As Object) As String
        Return NormalizeMatchToken(AsString(arg), toUpper:=True, removeUnderscore:=False)
    End Function

    ''' <summary>
    ''' Normalizes a key for case-insensitive dictionary or option matching.
    ''' </summary>
    Friend Function NormalizeKey(value As String) As String
        Return NormalizeMatchToken(value, toUpper:=False, removeUnderscore:=True)
    End Function

    Friend Function CloneStringArray(values() As String) As String()
        If values Is Nothing Then Return Nothing
        Return DirectCast(values.Clone(), String())
    End Function

End Module
