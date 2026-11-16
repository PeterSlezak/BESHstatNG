Option Strict On
Option Explicit On
Imports System
Imports System.Collections.Generic
Imports BESHStatNG.ExcelCommon
Namespace Harness46
 Friend Module FakePlanning46
  Friend ReadOnly Requests As New List(Of SampleSizePlanningRequest)
  Friend Function Calculate(request As SampleSizePlanningRequest) As SampleSizePlanningReport
   Requests.Add(request)
   Return New SampleSizePlanningReport(request, Nothing, "synthetic planning response")
  End Function
 End Module
 Friend Module FakeFaults46
  Friend ReadOnly Errors As New List(Of Exception)
  Friend Sub Record(failure As Exception, Optional first As Boolean = False, Optional second As Boolean = True)
   Errors.Add(failure)
  End Sub
 End Module
 Friend Module FakeGlobals46
  Friend Const gsAPP_TITLE As String = "synthetic application"
  Friend app As New FakeApp46()
 End Module
 Friend Class FakeApp46
  Friend ActiveWorkbook As New FakeBook46()
  Friend Workbooks As New FakeBooks46(Me)
 End Class
 Friend Class FakeBooks46
  Private ReadOnly app As FakeApp46
  Friend Sub New(value As FakeApp46)
   app = value
  End Sub
  Friend Function Add() As FakeBook46
   FakeOutput46.Events.Add("workbook-add") : app.ActiveWorkbook = New FakeBook46() : Return app.ActiveWorkbook
  End Function
 End Class
 Friend Class FakeBook46
  Friend ActiveSheet As New FakeSheet46()
  Friend Worksheets As New FakeSheets46(Me)
 End Class
 Friend Class FakeSheets46
  Private ReadOnly book As FakeBook46
  Friend Sub New(value As FakeBook46)
   book = value
  End Sub
  Friend Function Add() As FakeSheet46
   FakeOutput46.Events.Add("worksheet-add") : book.ActiveSheet = New FakeSheet46() : Return book.ActiveSheet
  End Function
 End Class
 Friend Class FakeSheet46
  Friend Function Range(address As String) As FakeRange46
   FakeOutput46.Events.Add("range:" & address) : Return New FakeRange46()
  End Function
 End Class
 Friend Class FakeRange46
  Friend Row As Integer = 7, Column As Integer = 3
 End Class
 Friend Class FakeResultWriter46
  Friend wb As FakeBook46, ws As FakeSheet46, RowID As Integer, ColID As Integer
  Friend Sub setRowPointer(value As Integer)
   RowID = value
  End Sub
  Friend Sub setColumnPointer(value As Integer)
   ColID = value
  End Sub
 End Class
 Friend Class FakeTables46
  Private ReadOnly original As Global.BESHStatNG.ProcessListofResultTables
  Friend Sub New(tables As List(Of ResultTable))
   original = New Global.BESHStatNG.ProcessListofResultTables(tables)
   FakeOutput46.TableCount = tables.Count
  End Sub
  Friend ReadOnly Property TotRows As Integer
   Get
    Return original.TotRows
   End Get
  End Property
  Friend ReadOnly Property TotCols As Integer
   Get
    Return original.TotCols
   End Get
  End Property
  Friend Sub writeToSheet(writer As FakeResultWriter46, stacked As Boolean)
   FakeOutput46.Writes += 1 : FakeOutput46.Events.Add("write")
   FakeOutput46.LastRow = writer.RowID : FakeOutput46.LastColumn = writer.ColID
  End Sub
 End Class
 Friend Module FakeOutput46
  Friend ReadOnly Events As New List(Of String)
  Friend ImportCalls As Integer, Writes As Integer, TableCount As Integer, LastRow As Integer, LastColumn As Integer
  Friend Occupied As Boolean
  Friend Imported As DataObj
  Friend Function WorksheetFromRefAdress(address As String) As FakeSheet46
   Events.Add("sheet:" & address) : Return FakeGlobals46.app.ActiveWorkbook.ActiveSheet
  End Function
  Friend Function AreaCheck(row As Integer, column As Integer, rows As Integer, columns As Integer, sheet As FakeSheet46) As Boolean
   Events.Add("area") : Return Occupied
  End Function
  Friend Sub Reset()
   Events.Clear() : ImportCalls = 0 : Writes = 0 : TableCount = 0 : Occupied = False : Imported = Nothing
   FakeGlobals46.app = New FakeApp46() : FakeFaults46.Errors.Clear() : FakeMessages46.Messages.Clear() : FakeMessages46.Response = 1 : FakePlanning46.Requests.Clear()
  End Sub
 End Module
End Namespace
