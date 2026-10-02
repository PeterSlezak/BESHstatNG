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

''' <summary>
''' Host-neutral options used by the shared DataObj raw-matrix import path.
''' Source provenance is carried as <see cref="DataSourceInfo"/> rather than a host worksheet object.
''' </summary>
Public Class DataImportOptions
    Public Property FirstSourceRow As Integer = 1
    Public Property SourceInfo As DataSourceInfo = Nothing
    Public Property CharCols As Integer = -1
    Public Property SkipRows As Integer = 0
    Public Property AllowMissing As Boolean = False
    Public Property SourceKind As String = "RawMatrix"
    Public Property SourceAddress As String = Nothing
End Class
