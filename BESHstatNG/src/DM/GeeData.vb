Option Explicit On

Imports BESHStatNG.DataManagement

Public Class geeData
    Inherits DataObj


    ''' <summary>Flag indicating whether weight data is present.</summary>
    Public bWeights As Boolean
    ''' <summary>Flag indicating whether offset data is present.</summary>
    Public bOffset As Boolean
    ''' <summary>Array of imported offset values.</summary>
    Public OffsetData() As Double
    ''' <summary>Array of imported weight values.</summary>
    Public WeightData() As Double
    ''' <summary>Name of the offset variable (last column in the imported range).</summary>
    Public OffsetVarName As String
    ''' <summary>Name of the weight variable (last column in the imported range).</summary>
    Public WeightVarName As String
    ''' <summary>Flag indicating whether Time (within cluster orderig) data is present. It's used in the GEE.</summary>
    Public bTime As Boolean
    ''' <summary>Array of imported Time (within-cluster ordering) values.</summary>
    Public TimeData() As Double
    ''' <summary>Array of imported Cluster ID values.</summary>
    Public ClusterIdData() As Object
    ''' <summary>Name of the Time variable.</summary>
    Public TimeVarName As String
    ''' <summary>Name of the Cluster ID variable.</summary>
    Public ClusterIdVarName As String

    ''' <summary>
    ''' Initializes a new instance of the <c>glmData</c> class with weights and offsets disabled.
    ''' </summary>
    Sub New()
        MyBase.New()
        Me.bWeights = False
        Me.bOffset = False
        Me.bTime = False
    End Sub

    Public Overrides Sub SubsetByRowIdValues(rIds As Dictionary(Of Integer, Integer))
        Me.FinalData = RowArrayUtilities.SubsetRowsByIds(Me.FinalData, rIds.Keys)
        Me.ClusterIdData = RowArrayUtilities.SubsetItemsByIds(Me.ClusterIdData, rIds.Keys)
        If Me.bOffset Then Me.OffsetData = RowArrayUtilities.SubsetItemsByIds(Me.OffsetData, rIds.Keys)
        If Me.bWeights Then Me.WeightData = RowArrayUtilities.SubsetItemsByIds(Me.WeightData, rIds.Keys)
        If Me.bTime Then Me.TimeData = RowArrayUtilities.SubsetItemsByIds(Me.TimeData, rIds.Keys)
        Me.RowIds = rIds.Values.ToArray()
    End Sub

    Public Overrides Sub DataImportRawMatrix(rawInput(,) As Object,
                                         variableNames() As String,
                                         Optional firstSourceRow As Integer = 1,
                                         Optional sourceInfo As DataSourceInfo = Nothing,
                                         Optional CharCols As Integer = -1,
                                         Optional SkipRow As Integer = 0)

        MyBase.DataImportRawMatrix(rawInput, variableNames, firstSourceRow, sourceInfo, CharCols, SkipRow)
    End Sub

    Protected Overrides Sub OnDataImported()
        If Me.bZeroValid OrElse Me.FinalData Is Nothing Then Return
        FinalizeGeeImport()
    End Sub

    Private Sub FinalizeGeeImport()

        'Sort by repeats clusterid (subject) and within cluster ordering varianble (if provided)
        'It is required by some GEE algorithms logic
        Dim data2(Me.nRows - 1, Me.nCols) As Object
        For i = 0 To Me.nRows - 1
            For j = 0 To Me.nCols - 1
                data2(i, j) = Me.FinalData(i, j)
            Next
            data2(i, Me.nCols) = Me.RowIds(i)
        Next

        Dim iClasterPos = Me.nCols - 1  'will be the clusterID column position in the array. Time is right after it
        If bWeights Then iClasterPos -= 1
        If bOffset Then iClasterPos -= 1
        If bTime Then iClasterPos -= 1

        If bTime Then 'Sort by Claster ID only and by time
            RowArrayUtilities.SortRows(data2, $"{iClasterPos},A,{iClasterPos + 1},A", 0, Me.nRows - 1)
        Else
            'Order is get by source doata ordering (i.e. RowIds)
            RowArrayUtilities.SortRows(data2, $"{iClasterPos},A,{Me.nCols},A", 0, Me.nRows - 1)
        End If
        'move back to original arrays
        For i = 0 To Me.nRows - 1
            For j = 0 To Me.nCols - 1
                Me.FinalData(i, j) = data2(i, j)
            Next
            Me.RowIds(i) = data2(i, Me.nCols)
        Next

        'process offset and weights
        If Me.bWeights Then
            'Last column is offset. Put it to separate array
            ReDim Me.WeightData(Me.nRows - 1)
            For i = 0 To Me.nRows - 1
                Me.WeightData(i) = Me.FinalData(i, Me.nCols - 1)
            Next
            Me.WeightVarName = Me.varNames(Me.nCols - 1)
            ReDim Preserve Me.FinalData(Me.nRows - 1, Me.nCols - 2)
            ReDim Preserve Me.varNames(Me.nCols - 2)
            Me.nCols -= 1
        End If
        If Me.bOffset Then
            'Last column is offset. Put it to separate array
            ReDim Me.OffsetData(Me.nRows - 1)
            For i = 0 To Me.nRows - 1
                Me.OffsetData(i) = Me.FinalData(i, Me.nCols - 1)
            Next
            Me.OffsetVarName = Me.varNames(Me.nCols - 1)
            ReDim Preserve Me.FinalData(Me.nRows - 1, Me.nCols - 2)
            ReDim Preserve Me.varNames(Me.nCols - 2)
            Me.nCols -= 1
        End If
        'process Time and ClusterID
        If Me.bTime Then
            'Last column now is Time. Put it to separate array
            ReDim Me.TimeData(Me.nRows - 1)
            For i = 0 To Me.nRows - 1
                Me.TimeData(i) = Me.FinalData(i, Me.nCols - 1)
            Next
            Me.TimeVarName = Me.varNames(Me.nCols - 1)
            ReDim Preserve Me.FinalData(Me.nRows - 1, Me.nCols - 2)
            ReDim Preserve Me.varNames(Me.nCols - 2)
            Me.nCols -= 1
        End If
        'Cluster ID - now it should be the last column in the Final Data
        ReDim Me.ClusterIdData(Me.nRows - 1)
        For i = 0 To Me.nRows - 1
            Me.ClusterIdData(i) = Me.FinalData(i, Me.nCols - 1)
        Next
        Me.ClusterIdVarName = Me.varNames(Me.nCols - 1)
        ReDim Preserve Me.FinalData(Me.nRows - 1, Me.nCols - 2)
        ReDim Preserve Me.varNames(Me.nCols - 2)
        Me.nCols -= 1

    End Sub

End Class
