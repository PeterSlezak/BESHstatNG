"""Single canonical spill implementation and typed portable name policies."""
import re
from common import *
p='BESHStat.Excel.Common/src/Results/UdfSpillPreparation38.vb';s=(ROOT/p).read_text(encoding='utf-8-sig');start=s.index('        Public Shared Function StackWithBlankRow');end=s.index('    End Class',start)
removed=s[start:end];assert removed.count('Public Shared Function')==2
write_source(p,s[:start]+s[end:])

p='BESHStat.Excel.Common/src/Analysis/UdfMetadataPreparation38.vb';s=(ROOT/p).read_text(encoding='utf-8-sig');assert 'Class UdfNamePreparation38' not in s
code='''
    '''+"'''"+''' <summary>Host-acquired shape and string accessor; no worksheet cell or error object crosses this boundary.</summary>
    Public NotInheritable Class UdfVariableNameRange38
        Private ReadOnly _read As Func(Of Integer, Integer, String)
        Public Sub New(rows As Integer, columns As Integer, readText As Func(Of Integer, Integer, String))
            RowCount = rows
            ColumnCount = columns
            _read = readText
        End Sub
        Public ReadOnly Property RowCount As Integer
        Public ReadOnly Property ColumnCount As Integer
        Public Function ReadText(row As Integer, column As Integer) As String
            Return _read(row, column)
        End Function
    End Class

    '''+"'''"+''' <summary>Original UDF variable-name precedence, ownership, ordinal formatting and range traversal.</summary>
    Public NotInheritable Class UdfNamePreparation38
        Private Sub New()
        End Sub

        Public Shared Function FillExplicitNames(names As String(), prefix As String) As String()
            For i As Integer = 0 To names.Length - 1
                If String.IsNullOrWhiteSpace(names(i)) Then names(i) = prefix & " " & (i + 1).ToString()
            Next
            Return names
        End Function

        Public Shared Function ResolveDetectedNames(detectedNames As String(), expectedCount As Integer, prefix As String) As String()
            If detectedNames IsNot Nothing AndAlso detectedNames.Length = expectedCount Then
                Dim names(expectedCount - 1) As String
                For i As Integer = 0 To expectedCount - 1
                    names(i) = detectedNames(i)
                    If String.IsNullOrWhiteSpace(names(i)) Then names(i) = prefix & " " & (i + 1).ToString()
                Next
                Return names
            End If

            Dim fallback(expectedCount - 1) As String
            For i As Integer = 0 To expectedCount - 1
                fallback(i) = prefix & " " & (i + 1).ToString()
            Next
            Return fallback
        End Function

        Public Shared Function GetVariableNames(expectedCount As Integer,
                                                isMissing As Func(Of Boolean),
                                                scalarText As Func(Of String),
                                                acquireRange As Func(Of UdfVariableNameRange38)) As String()
            If expectedCount <= 0 Then Return New String() {}

            Dim fallback(expectedCount - 1) As String
            For i As Integer = 0 To expectedCount - 1
                fallback(i) = "X" & (i + 1).ToString()
            Next

            If isMissing() Then Return fallback

            Dim s As String = scalarText()
            If s IsNot Nothing Then
                Dim parts As String() = s.Split({","c}, StringSplitOptions.RemoveEmptyEntries)
                If parts.Length = expectedCount Then
                    For i As Integer = 0 To expectedCount - 1
                        parts(i) = parts(i).Trim()
                        If String.IsNullOrWhiteSpace(parts(i)) Then parts(i) = fallback(i)
                    Next
                    Return parts
                End If
            End If

            Dim arr As UdfVariableNameRange38 = acquireRange()
            If arr Is Nothing Then Return fallback

            Dim rows As Integer = arr.RowCount
            Dim cols As Integer = arr.ColumnCount
            Dim list As New List(Of String)()

            If rows = 1 AndAlso cols >= 1 Then
                For j As Integer = 0 To cols - 1
                    list.Add(arr.ReadText(0, j))
                Next
            ElseIf cols = 1 AndAlso rows >= 1 Then
                For i As Integer = 0 To rows - 1
                    list.Add(arr.ReadText(i, 0))
                Next
            End If

            If list.Count = expectedCount Then
                For i As Integer = 0 To expectedCount - 1
                    If String.IsNullOrWhiteSpace(list(i)) Then list(i) = fallback(i)
                Next
                Return list.ToArray()
            End If

            Return fallback
        End Function
    End Class
'''
head,tail=s.rsplit('End Namespace',1);write_source(p,head+code+'End Namespace'+tail)

def replace(p,name,signature,newbody):
 s=(ROOT/p).read_text(encoding='utf-8-sig');m=re.search(signature,s);assert m;end=s.index('        End Function' if m[0].startswith('        ') else '    End Function',m.end())
 write_source(p,s[:m.end()]+'\n'+newbody+'\n'+s[end:])
replace('BESHstatNG/src/ExcelUDFs/ParametricUDFs.vb','ResolveNames',r'(?m)^        Friend Function ResolveNames[^\n]+','''            If Not IsMissingArg(explicitNames) Then
                Dim names = Global.BESHStatNG.UdfDataImport.GetVariableNames(explicitNames, expectedCount)
                Return Global.BESHStatNG.ExcelCommon.UdfNamePreparation38.FillExplicitNames(names, prefix)
            End If
            Return Global.BESHStatNG.ExcelCommon.UdfNamePreparation38.ResolveDetectedNames(detectedNames, expectedCount, prefix)''')
replace('BESHstatNG/src/ExcelUDFs/UDFdataImport/UdfDataImportFormula.vb','GetVariableNames',r'(?m)^    Friend Function GetVariableNames[^\n]+','''        If expectedCount <= 0 Then Return New String() {}
        Return Global.BESHStatNG.ExcelCommon.UdfNamePreparation38.GetVariableNames(expectedCount,
            Function() Global.BESHStatNG.WorksheetFunctions.ExcelArgPredicates.IsMissingArg(arg),
            Function() TryCast(arg, String),
            Function() As Global.BESHStatNG.ExcelCommon.UdfVariableNameRange38
                Dim arr As Object(,) = Get2D(arg)
                If arr Is Nothing Then Return Nothing
                Dim rows As Integer = arr.GetLength(0)
                Dim cols As Integer = arr.GetLength(1)
                Return New Global.BESHStatNG.ExcelCommon.UdfVariableNameRange38(rows, cols,
                    Function(row As Integer, column As Integer) As String
                        Dim cell As Object = arr(row, column)
                        If Global.BESHStatNG.WorksheetFunctions.ExcelArgPredicates.IsBlankCell(cell) Then Return String.Empty
                        Return Convert.ToString(cell).Trim()
                    End Function)
            End Function)''')
save('implementation-names.json',dict(status='SOURCE_EDITED_AWAITING_COMPILATION_AND_PROOF',removed_coordinate_methods=['StackWithBlankRow','NormalizeBlanks'],canonical_coordinate_properties=['RowOrdinal','ColumnOrdinal'],removed_source_sha256=sha(removed.encode()),new_portable_methods=['FillExplicitNames','ResolveDetectedNames','GetVariableNames'],native_cell_policy='Get2D/IsBlankCell/Convert.ToString.Trim only in native callbacks; Common receives strings/dimensions, never ExcelError',original_queries=0))
