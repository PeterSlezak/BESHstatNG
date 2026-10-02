Option Explicit On
Option Strict On

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
