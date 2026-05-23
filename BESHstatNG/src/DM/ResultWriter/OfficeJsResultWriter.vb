Option Explicit On
Option Strict On

''' <summary>
''' Host-neutral writer that captures output blocks for a future Office.js front end.
''' </summary>
''' <remarks>
''' Office.js itself will write these blocks to Excel using TypeScript/JavaScript. This VB.NET
''' writer is intended for a server/API layer that prepares JSON-serializable values and table
''' metadata without referencing Excel Interop.
''' </remarks>
Public Class OfficeJsResultWriter
    Inherits ResultTableWriterBase

    Private ReadOnly mBlocks As New List(Of ResultTableOutputBlock)

    Public Sub New(Optional row As Integer = 1, Optional col As Integer = 1)
        MyBase.New(row, col)
    End Sub

    Public ReadOnly Property Blocks As List(Of ResultTableOutputBlock)
        Get
            Return mBlocks
        End Get
    End Property

    Protected Overrides Sub WriteOutputBlock(block As ResultTableOutputBlock)
        If block Is Nothing Then Exit Sub
        mBlocks.Add(block)
    End Sub
End Class

''' <summary>
''' Placeholder writer for a future Google Sheets adapter.
''' </summary>
''' <remarks>
''' Google Sheets and Office.js both need row/column/value blocks plus metadata. This class keeps
''' that future adapter explicit while reusing the same portable payload behavior for now.
''' </remarks>
Public Class GoogleSheetsResultWriter
    Inherits OfficeJsResultWriter

    Public Sub New(Optional row As Integer = 1, Optional col As Integer = 1)
        MyBase.New(row, col)
    End Sub
End Class
