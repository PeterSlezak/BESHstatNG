Option Explicit On
Option Strict On
Imports System
Imports System.Collections.Generic
Imports System.Globalization
Imports System.Linq
Imports BESHStatNG.StatisticalProcessControl

Namespace ExcelCommon
    Public NotInheritable Class SpcImportedExclusion
        Friend Sub New(index As Integer, reason As String)
            DisplayPoint = index + 1 : Me.Reason = reason
        End Sub
        Public ReadOnly Property DisplayPoint As Integer
        Public ReadOnly Property Reason As String
    End Class
    Public NotInheritable Class SpcExclusionPolicy
        Private Sub New()
        End Sub
        Public Shared Function Import(map As SpcSourcePointMap, indicators() As Boolean, reasons() As String) As IReadOnlyList(Of SpcImportedExclusion)
            If map Is Nothing Then Throw New ArgumentNullException(NameOf(map))
            If indicators Is Nothing Then Throw New ArgumentNullException(NameOf(indicators))
            If indicators.Length <> map.PointIndexByRow.Count OrElse (reasons IsNot Nothing AndAlso reasons.Length <> indicators.Length) Then Throw New ArgumentException("Exclusion values must align with source rows.")
            Dim excluded(map.PointCount - 1) As Boolean, sets(map.PointCount - 1) As HashSet(Of String)
            For i = 0 To sets.Length - 1 : sets(i) = New HashSet(Of String)(StringComparer.Ordinal) : Next
            For row = 0 To indicators.Length - 1
                Dim point = map.PointIndexByRow(row)
                If indicators(row) Then excluded(point) = True
                If reasons IsNot Nothing AndAlso reasons(row).Trim().Length > 0 Then sets(point).Add(reasons(row).Trim())
            Next
            Dim result As New List(Of SpcImportedExclusion)
            For point = 0 To excluded.Length - 1
                If excluded(point) Then result.Add(New SpcImportedExclusion(point, String.Join("; ", sets(point).ToArray())))
            Next
            Return result.AsReadOnly()
        End Function
        Public Shared Function CoreIndex(displayPoint As Integer, logicalPointCount As Integer) As Integer
            If displayPoint < 1 Then Throw New ArgumentException("Exclusion point numbers are one-based and must be positive.")
            If displayPoint > logicalPointCount Then Throw New ArgumentException("Exclusion point " & displayPoint.ToString(CultureInfo.CurrentCulture) & " is beyond the last chart point (" & logicalPointCount.ToString(CultureInfo.CurrentCulture) & ").")
            Return displayPoint - 1
        End Function
    End Class
End Namespace
