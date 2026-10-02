Option Explicit On

Imports BESHStatNG.DataManagement

''' <summary>
''' Specialized data container for generalized linear models (GLMs), extending <see cref="DataObj"/>.
''' </summary>
''' <remarks>
''' <para>
''' The <c>glmData</c> class inherits all functionality from <c>DataObj</c> (importing, cleaning, subsetting)
''' and adds support for GLM-specific features:
''' </para>
''' <list type="bullet">
'''   <item><description>Optional weights for weighted regression.</description></item>
'''   <item><description>Optional offsets for log-linear models.</description></item>
'''   <item><description>Automatic separation of weight/offset columns from the main data matrix.</description></item>
'''   <item><description>Row subsetting that preserves alignment of weights and offsets.</description></item>
''' </list>
''' </remarks>
''' <example>
''' ' Example: import GLM data with weights from a raw matrix
''' Dim glm As New glmData()
''' glm.bWeights = True
''' glm.DataImportRawMatrix(raw, variableNames)
''' Console.WriteLine("Rows: " + glm.nRows + ", Cols: " + glm.nCols)
''' Console.WriteLine("Weight variable: " + glm.WeightVarName)
''' </example>
Public Class glmData
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

    ''' <summary>
    ''' Initializes a new instance of the <c>glmData</c> class with weights and offsets disabled.
    ''' </summary>
    Sub New()
        MyBase.New()
        Me.bWeights = False
        Me.bOffset = False
    End Sub

    ''' <summary>
    ''' Subsets the <c>FinalData</c> matrix to include only rows matching the specified row IDs,
    ''' preserving alignment of weights and offsets if present.
    ''' </summary>
    ''' <param name="rIds">An array of row IDs to retain.</param>
    ''' <remarks>
    ''' - Overrides <c>DataObj.SubsetByRowIdValues</c>.
    ''' - Ensures that <c>OffsetData</c> and <c>WeightData</c> are subset consistently with <c>FinalData</c>.
    ''' - Useful for composite models (e.g., Zero-Inflated Poisson) requiring matched records across multiple data objects.
    ''' </remarks>
    ''' <example>
    ''' Dim ids() As Integer = {2, 5, 7}
    ''' glm.SubsetByRowIdValues(ids)
    ''' Console.WriteLine("Subset rows: " + glm.nRows)
    ''' </example>
    Public Overrides Sub SubsetByRowIdValues(rIds As Dictionary(Of Integer, Integer))
        Me.FinalData = RowArrayUtilities.SubsetRowsByIds(Me.FinalData, rIds.Keys)
        If Me.bOffset Then Me.OffsetData = RowArrayUtilities.SubsetItemsByIds(Me.OffsetData, rIds.Keys)
        If Me.bWeights Then Me.WeightData = RowArrayUtilities.SubsetItemsByIds(Me.WeightData, rIds.Keys)
        Me.RowIds = rIds.Values.ToArray()
    End Sub

    Protected Overrides Sub OnDataImported()
        If Me.bZeroValid OrElse Me.FinalData Is Nothing Then Return
        SplitOffsetAndWeights()
    End Sub

    Private Sub SplitOffsetAndWeights()
        If Me.bWeights Then
            ReDim Me.WeightData(Me.nRows - 1)
            For i = 0 To Me.nRows - 1
                Me.WeightData(i) = CDbl(Me.FinalData(i, Me.nCols - 1))
            Next
            Me.WeightVarName = Me.varNames(Me.nCols - 1)
            ReDim Preserve Me.FinalData(Me.nRows - 1, Me.nCols - 2)
            ReDim Preserve Me.varNames(Me.nCols - 2)
            Me.nCols -= 1
        End If

        If Me.bOffset Then
            ReDim Me.OffsetData(Me.nRows - 1)
            For i = 0 To Me.nRows - 1
                Me.OffsetData(i) = CDbl(Me.FinalData(i, Me.nCols - 1))
            Next
            Me.OffsetVarName = Me.varNames(Me.nCols - 1)
            ReDim Preserve Me.FinalData(Me.nRows - 1, Me.nCols - 2)
            ReDim Preserve Me.varNames(Me.nCols - 2)
            Me.nCols -= 1
        End If
    End Sub

    Public Overrides Sub DataImportRawMatrix(rawInput(,) As Object,
                                         variableNames() As String,
                                         Optional firstSourceRow As Integer = 1,
                                         Optional sourceInfo As DataSourceInfo = Nothing,
                                         Optional CharCols As Integer = -1,
                                         Optional SkipRow As Integer = 0)

        MyBase.DataImportRawMatrix(rawInput, variableNames, firstSourceRow, sourceInfo, CharCols, SkipRow)
    End Sub
End Class
