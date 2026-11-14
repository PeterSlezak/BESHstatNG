Option Strict On
Public Module UdfTrace38
    Private ReadOnly Seen As New System.Collections.Generic.HashSet(Of String)(System.StringComparer.Ordinal)
    Public Sub Hit(identity As String)
        Seen.Add(identity)
    End Sub
    Public Sub Clear()
        Seen.Clear()
    End Sub
    Public Function Snapshot() As String()
        Dim values(Seen.Count - 1) As String
        Seen.CopyTo(values)
        System.Array.Sort(values, System.StringComparer.Ordinal)
        Return values
    End Function
End Module
