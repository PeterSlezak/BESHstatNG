Option Explicit On

Public Class TwoGroupsData
    Public X1() As Double
    Public X2() As Double
    Public name1 As String
    Public name2 As String
End Class

Public Class MultiGroupsUnpairedData
    Public X()() As Double
    Public varNames() As String
End Class

Public Class TwoGroupsPairedData
    Public X(,) As Double
    Public name1 As String
    Public name2 As String
End Class

Public Class MultiGroupsPairedData
    Public X(,) As Double
    Public varNames() As String
End Class

Public Class MultiGroupsPairedDataObj
    Public X(,) As Object
    Public varNames() As String
End Class

Public Class MmrmData
    Public Raw As DataObj
    Public SubjectKey As String
    Public ResponseKey As String
    Public VisitKey As String
End Class
