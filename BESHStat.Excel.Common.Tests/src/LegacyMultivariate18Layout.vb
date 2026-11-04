Option Explicit On
Option Strict On
Imports System
Imports BESHStatNG
Imports BESHStatNG.Multivariate

' Frozen prechange Data-sheet write statements; only method/parameter boundaries added.
Public NotInheritable Class LegacyMultivariate18Layout
    Public Shared Sub PcaData(WriteRes As ResultTableWriterBase, MyData As DataObj, objPCA As PCA, rowLabels() As String, groupLabels() As String)
        Dim metadataColumnCount As Integer = 1
        If rowLabels IsNot Nothing Then
            WriteRes.write({"Record ID"})
            WriteRes.setRowPointer(2)
            WriteRes.write(rowLabels, bTall:=True)
        Else
            WriteRes.write({"Row ID"})
            WriteRes.setRowPointer(2)
            WriteRes.write(MyData.RowIds, bTall:=True)
        End If

        If groupLabels IsNot Nothing Then
            metadataColumnCount += 1
            WriteRes.setRowPointer()
            WriteRes.setColumnPointer(2)
            WriteRes.write({"Group ID"})
            WriteRes.setRowPointer(2)
            WriteRes.write(groupLabels, bTall:=True)
        End If
        WriteRes.setRowPointer()
        WriteRes.setColumnPointer(metadataColumnCount + 1)
        WriteRes.write(MyData.varNames)
        WriteRes.write(MyData.FinalData)
        WriteRes.shiftColumnPointer(MyData.varNames.Length)
        WriteRes.setRowPointer()
        'Standardized values
        WriteRes.write(objPCA.StandardizedVarNames(MyData.varNames))
        WriteRes.write(objPCA.StandardizedData)
        WriteRes.shiftColumnPointer(MyData.varNames.Length)
        WriteRes.setRowPointer()
        'Final Reduced Dataset
        WriteRes.write(objPCA.StandardizedVarNames(objPCA.PCnames("Reduced_Data_PC")))
        WriteRes.write(objPCA.ReducedDataset)

    End Sub
    Public Shared Sub McaData(WriteRes As ResultTableWriterBase, MyData As DataObj, mca As CA)
        WriteRes.write({"Row ID"})
        WriteRes.setRowPointer(3) 'second row is blank because of Design matrix having two header rows
        WriteRes.write(MyData.RowIds, bTall:=True)
        WriteRes.setRowPointer()
        WriteRes.setColumnPointer(2)
        WriteRes.write(MyData.varNames)
        WriteRes.setRowPointer(3) 'second row is blank
        WriteRes.write(MyData.FinalData)
        WriteRes.shiftColumnPointer(MyData.varNames.Length)
        WriteRes.setRowPointer()
        WriteRes.write({"Design MatrixType:"})
        WriteRes.setRowPointer()
        WriteRes.shiftColumnPointer(1)
        WriteRes.write(mca.BurtVarNames)
        WriteRes.write(mca.rowNames)
        WriteRes.write(mca.DesignMatrix)

    End Sub
    Public Shared Function LabelledCounts(data As DataObj) As Integer(,)
        Dim TableData(data.nRows - 1, data.nCols - 2) As Integer
        For i = 0 To UBound(data.FinalData, 1)
            For j = 1 To UBound(data.FinalData, 2)
                TableData(i, j - 1) = CInt(data.FinalData(i, j))
            Next
        Next
        Return TableData
    End Function
    Public Shared Function LegacyUnlabelledCounts(data As DataObj) As Integer(,)
        Dim TableData(data.nRows - 1, data.nCols - 1) As Integer
        For i = 0 To UBound(data.FinalData, 1)
            For j = 0 To UBound(data.FinalData, 2)
                TableData(i, j - 1) = CInt(data.FinalData(i, j))
            Next
        Next
        Return TableData
    End Function
End Class
