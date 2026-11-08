Option Explicit On
Option Strict On
Imports System
Namespace Frozen37
Public Class ArchivedLateExpressions37
Public Shared Function Component(pct As Double(), ordinal As Integer, score As Boolean) As String
If ordinal = 0 Then
If score Then Return $"1St Component Scores [{ Format$(pct(0), "#0.0#") }%]"
Return $"1st Component Scores [{ Format$(pct(0), "#0.0#") }%]"
End If
If ordinal = 1 Then Return $"2nd Component Scores [{ Format$(pct(1), "#0.0#") }%]"
If ordinal = 2 Then
If score Then Return $"3Rd Component Scores [{ Format$(pct(2), "#0.0#") }%]"
Return $"3rd Component Scores [{ Format$(pct(2), "#0.0#") }%]"
End If
Throw New ArgumentOutOfRangeException(NameOf(ordinal))
End Function
Public Shared Function Factor(factorNames As String(), factorPct As Double(), ordinal As Integer) As String
If ordinal = 0 Then Return $"{factorNames(0)} [{Format$(factorPct(0), "#0.0#")}%]"
If ordinal = 1 Then Return $"{factorNames(1)} [{Format$(factorPct(1), "#0.0#")}%]"
If ordinal = 2 Then Return $"{factorNames(2)} [{Format$(factorPct(2), "#0.0#")}%]"
Throw New ArgumentOutOfRangeException(NameOf(ordinal))
End Function
Public Shared Function Correspondence(pct As Double(,), ordinal As Integer) As String
If ordinal = 1 Then Return $"Factor 1 [{ Format$(pct(1, 1), "#0.0#") }%]"
If ordinal = 2 Then Return $"Factor 2 [{ Format$(pct(2, 1), "#0.0#") }%]"
Throw New ArgumentOutOfRangeException(NameOf(ordinal))
End Function
Public Shared Function Percent(pct As Double(), i As Integer) As String
Return Format$(pct(i), "#0.0#")
End Function
Public Shared Function BiplotName(c As Double) As String
Return "Biplot scale=" & CStr(c)
End Function
Public Shared Function LoadingName(id As Integer) As String
Return "Loading_" & CStr(id)
End Function
Public Shared Function VariableLabel(varNames As String(), id As Integer) As String
Return CStr(varNames(id))
End Function
Public Shared Function ContributionName(lAxis As Integer) As String
Return "Contribution" & CStr(lAxis + 1)
End Function
Public Shared Function ContributionTitle(lAxis As Integer) As String
Return "Contribution Plot: Axis " & CStr(lAxis + 1)
End Function
Public Shared Function GroupIndices(groupLabels As String(), expectedCount As Integer) As Object()
If groupLabels Is Nothing Then Return New Object(){System.Linq.Enumerable.Range(0,expectedCount).ToArray()}
Dim groupIds As New Collections.Generic.List(Of String)
Dim seen As New Collections.Generic.HashSet(Of String)(StringComparer.Ordinal)
For Each groupId As String In groupLabels
If seen.Add(groupId) Then groupIds.Add(groupId)
Next
Dim result As New Collections.Generic.List(Of Object)
For g As Integer=0 To groupIds.Count-1
Dim indices As New Collections.Generic.List(Of Integer)
For i As Integer=0 To groupLabels.Length-1
If groupLabels(i) = groupIds(g) Then indices.Add(i)
Next
result.Add(indices.ToArray())
Next
Return result.ToArray()
End Function
End Class
End Namespace
