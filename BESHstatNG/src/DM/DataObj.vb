Option Explicit On

Imports BESHStatNG.AppInfrastructure
Imports ExcelDna.Integration
Imports Microsoft.Office.Interop.Excel

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

''' <summary>
''' Represents a data container and utility class for importing, cleaning,
''' and transforming worksheet data into structured arrays suitable for statistical analysis.
''' </summary>
''' <remarks>
''' <para>
''' The <c>DataObj</c> class is designed to bridge Excel worksheet ranges with
''' statistical routines in VB.NET/VBA interop. It provides functionality for:
''' </para>
''' <list type="bullet">
'''   <item><description>Importing raw data from worksheet references (including non-contiguous ranges).</description></item>
'''   <item><description>Handling missing values with configurable rules (allow or drop missing observations).</description></item>
'''   <item><description>Maintaining variable names and row identifiers for traceability.</description></item>
'''   <item><description>Converting raw data into typed arrays (<c>Double(,)</c>, jagged arrays by group).</description></item>
'''   <item><description>Subsetting data by row IDs for composite models.</description></item>
'''   <item><description>Preparing regression-ready data structures (e.g., for RMANOVA, Hotelling’s T²).</description></item>
''' </list>
''' 
''' <para>
''' Key properties:
''' </para>
''' <list type="bullet">
'''   <item><description><c>ws</c>: The source worksheet.</description></item>
'''   <item><description><c>varNames()</c>: Names of variables (columns).</description></item>
'''   <item><description><c>RawData(,)</c>: Imported raw values from Excel.</description></item>
'''   <item><description><c>FinalData(,)</c>: Cleaned and processed data matrix.</description></item>
'''   <item><description><c>RowIds()</c>: Row indices of valid observations.</description></item>
'''   <item><description><c>DataDbl</c>: Strongly typed <c>Double(,)</c> view of <c>FinalData</c>.</description></item>
'''   <item><description><c>DataByID2ByColumn</c>: Jagged array grouped by ID (first column).</description></item>
''' </list>
''' 
''' <para>
''' Typical workflow:
''' </para>
''' <list type="number">
'''   <item><description>Call <c>DataInport</c> with a reference string to import worksheet data.</description></item>
'''   <item><description>Use <c>RemoveMissing</c> to clean rows with missing values.</description></item>
'''   <item><description>Access <c>FinalData</c>, <c>DataDbl</c>, or <c>DataByID2ByColumn</c> for analysis.</description></item>
'''   <item><description>Optionally subset rows with <c>SubsetByRowIdValues</c>.</description></item>
''' </list>
''' </remarks>
''' <example>
''' ' Example: Import and clean worksheet data
''' Dim dObj As New DataObj()
''' dObj.DataInport("Sheet1!A:C", bStartRow:=True)
''' Dim cleanData As Double(,) = dObj.DataDbl
''' Console.WriteLine("Rows: " + dObj.nRows + ", Cols: " + dObj.nCols)
''' </example>
Public Class DataObj
    Public ws As Worksheet
    Public varNames() As String 'array that contains the variable names
    Public RawData(,) As Object 'variant that returns the range specified in 'ref'
    Public RowIds() As Integer = Nothing 'Excel row numbers where we have valid data
    Public FinalData(,) As Object
    Public nCols As Integer
    Public nRows As Integer
    Public bZeroValid As Boolean
    Private StartRow As Integer
    Private pbAllowMissing As Boolean = False
    Public WriteOnly Property bAllowMissing() As Boolean
        Set(value As Boolean)
            pbAllowMissing = value
        End Set
    End Property

    ''' <summary>
    ''' Returns the <c>FinalData</c> matrix as a strongly typed <c>Double(,)</c> array.
    ''' </summary>
    ''' <returns>A two-dimensional array of doubles, with <c>Double.NaN</c> for missing values if allowed.</returns>
    ''' <remarks>
    ''' Converts each element of <c>FinalData</c> to <c>Double</c>.  
    ''' If <c>bAllowMissing</c> is <c>True</c>, missing entries are represented as <c>Double.NaN</c>.  
    ''' </remarks>
    ''' <example>
    ''' Dim d(,) As Double = Me.DataDbl
    ''' Console.WriteLine("Value at (0,0): " + d(0,0))
    ''' </example>
    Public ReadOnly Property DataDbl() As Double(,)
        Get
            Dim d(,) As Double
            ReDim d(UBound(Me.FinalData, 1), UBound(Me.FinalData, 2))
            For i = 0 To UBound(Me.FinalData, 1)
                For j = 0 To UBound(Me.FinalData, 2)
                    If Me.pbAllowMissing AndAlso Me.FinalData(i, j) Is Nothing Then
                        'when missing is allowed we set everything that is not a number to Nothing 
                        d(i, j) = Double.NaN
                    Else
                        d(i, j) = CDbl(Me.FinalData(i, j))
                    End If
                Next
            Next
            Return d
        End Get
    End Property

    ''' <summary>
    ''' Groups data by ID (first column) and returns values as a jagged array of doubles by column.
    ''' </summary>
    ''' <returns>A jagged array where each element contains the values for a group ID.</returns>
    ''' <remarks>
    ''' - Assumes the first column of <c>FinalData</c> contains group IDs.  
    ''' - Useful for grouped statistical analysis.  
    ''' </remarks>
    ''' <example>
    ''' Dim grouped()() As Double = Me.DataByID2ByColumn
    ''' Console.WriteLine("Group count: " + grouped.Length)
    ''' </example>
    Public ReadOnly Property DataByID2ByColumn() As Double()()
        Get
            Dim groupIDs() As Object, arDataColumn(,) As Double, NoGroups As Integer, arN() As Integer, n As Integer
            groupIDs = Matrix.GetColumnFrom2Darray(Me.FinalData, 0).Distinct().ToArray()
            NoGroups = UBound(groupIDs, 1)
            n = UBound(Me.FinalData, 1)

            'rewrite data to 2D array, by column
            Dim out()() As Double = New Double(NoGroups)() {}
            ReDim arDataColumn(n, NoGroups), arN(NoGroups)
            For i = 0 To n
                For j = 0 To NoGroups
                    If groupIDs(j) = Me.FinalData(i, 0) Then
                        arDataColumn(arN(j), j) = Me.FinalData(i, 1)
                        arN(j) += 1
                    End If
                Next
            Next
            For j = 0 To NoGroups
                out(j) = Matrix.SubsetArray(Matrix.GetColumnFrom2Darray(arDataColumn, j), 0, arN(j) - 1)
            Next

            Return out
        End Get

    End Property

    ''' <summary>
    ''' Imports data from a worksheet reference into the <c>DataObj</c>, preparing ranges and removing missing values.
    ''' </summary>
    ''' <param name="ref">The reference string specifying the worksheet range (may be non-contiguous).</param>
    ''' <param name="bStartRow">If <c>True</c>, determines the starting row from the reference string; otherwise defaults to 1.</param>
    ''' <param name="CharCols">Optional. Number of leading columns allowed to contain character data. Default = -1 (none).</param>
    ''' <param name="SkipRow">Optional. Number of rows to skip at the top of the range. Default = 0.</param>
    ''' <remarks>
    ''' Calls <c>RangePrep</c> to assemble the raw data matrix, then <c>RemoveMissing</c> to clean invalid rows.
    ''' </remarks>
    ''' <example>
    ''' Dim dObj As New DataObj()
    ''' dObj.DataInport("Sheet1!A:C", bStartRow:=True, CharCols:=1)
    ''' Console.WriteLine("Rows: " + dObj.nRows + ", Cols: " + dObj.nCols)
    ''' </example>
    Public Overridable Sub DataInport(ByRef ref As String, Optional bStartRow As Boolean = False,
                   Optional CharCols As Integer = -1, Optional SkipRow As Integer = 0)
        Me.RangePrep(ref, bStartRow)
        Me.RemoveMissing(CharCols, SkipRow)
    End Sub

    Public Overridable Sub DataImportRawMatrix(rawInput(,) As Object,
                                           variableNames() As String,
                                           Optional firstSourceRow As Integer = 1,
                                           Optional sourceWorksheet As Worksheet = Nothing,
                                           Optional CharCols As Integer = -1,
                                           Optional SkipRow As Integer = 0)

        If rawInput Is Nothing Then AppGlobals.BSerr.LogAndThrow(New ArgumentNullException(NameOf(rawInput)))
        If variableNames Is Nothing Then AppGlobals.BSerr.LogAndThrow(New ArgumentNullException(NameOf(variableNames)))
        Dim rows As Integer = rawInput.GetLength(0)
        Dim cols As Integer = rawInput.GetLength(1)

        If rows < 1 OrElse cols < 1 Then AppGlobals.BSerr.LogAndThrow(New ArgumentException("rawInput must contain at least one row and one column."))
        If variableNames.Length <> cols Then AppGlobals.BSerr.LogAndThrow(New ArgumentException($"variableNames length ({variableNames.Length}) must match the number of columns ({cols})."))

        Me.ws = sourceWorksheet
        Me.nRows = rows
        Me.nCols = cols
        Me.StartRow = Math.Max(1, firstSourceRow)

        ReDim Me.varNames(cols - 1)
        For j As Integer = 0 To cols - 1
            Me.varNames(j) = If(variableNames(j), String.Empty)
        Next

        ReDim Me.RawData(Me.StartRow + rows - 1, cols - 1)
        For i As Integer = 0 To rows - 1
            For j As Integer = 0 To cols - 1
                Me.RawData(Me.StartRow + i, j) = rawInput(i, j)
            Next
        Next

        Me.RemoveMissing(CharCols, SkipRow)
    End Sub

    ''' <summary>
    ''' Prepares the raw data matrix from a worksheet reference string, handling non-contiguous ranges.
    ''' </summary>
    ''' <param name="ref">The reference string specifying the worksheet range.</param>
    ''' <param name="bStartRow">If <c>True</c>, determines the starting row from the reference string; otherwise defaults to 1.</param>
    ''' <remarks>
    ''' - Determines number of columns and rows in the reference.  
    ''' - Populates <c>RawData</c> and <c>varNames</c>.  
    ''' - Handles missing or default variable names.  
    ''' </remarks>
    ''' <example>
    ''' Me.RangePrep("Sheet1!A:C", bStartRow:=True)
    ''' Console.WriteLine("StartRow=" + Me.StartRow)
    ''' </example>
    Private Sub RangePrep(ByRef ref As String, Optional bStartRow As Boolean = False)
        'Subroutine assembles a single return array from a (possibly) non-contiguous range reference.
        '  ref = a string that contains the range reference to be analyzed (may be non contiguous)

        Dim TestVal As Range, FullString() As String, j As Integer, i As Integer, PartString As Object, temp As Object
        Dim Str1 As Object, Str2 As Object, MaxRows As Integer, k As Integer
        Me.ws = WorksheetFromRefAdress(ref)

        With Me.ws
            FullString = ColumListFromRefAdress(ref)

            'Determine # of columns in the total range including non-contiguous ranges
            'Used to dimension return (Data) array later
            For i = 0 To UBound(FullString, 1)
                Me.nCols += .Range(FullString(i)).Columns.Count
            Next
            ReDim Me.varNames(Me.nCols - 1)

            'Determine rows by assigning a range object to a range on the worksheet
            'TestVal is range of the outcome variable
            TestVal = .Range(FullString(0))

            MaxRows = MaxRowsInSheet(WorksheetFromRefAdress(ref))

            'Find the last row of the data range (uses only the 1st column)
            If TestVal.Rows.Count = .Cells.Rows.Count Then
                Me.nRows = TestVal(MaxRows, 1).End(XlDirection.xlUp).row 'Start at bottom and go up
            Else
                Me.nRows = TestVal.Rows.Count
            End If

            'Dimension the return matrix and variable name array
            If bStartRow Then
                If InStr(FullString(0), ":") = 0 Then
                    If removeNonDigits(CStr(FullString(0))) = String.Empty Then
                        Me.StartRow = 1
                    Else
                        Me.StartRow = CLng(removeNonDigits(CStr(FullString(0))))
                    End If
                Else
                    temp = Split(FullString(0), ":")

                    If UBound(temp) >= 1 Then 'use row number
                        If removeNonDigits(CStr(temp(0))) = String.Empty Then
                            Me.StartRow = 1
                        Else
                            Me.StartRow = CLng(removeNonDigits(CStr(temp(0))))
                        End If
                    Else
                        Me.StartRow = 1
                    End If
                End If
            Else
                Me.StartRow = 1
            End If
            ReDim Me.RawData(Me.nRows + Me.StartRow - 1, Me.nCols - 1) ' we would need to move elements by number = StartRow

            'Loop once for each non-contiguous range
            For i = 0 To Me.nCols - 1
                If InStr(FullString(i), ":") = 0 Then
                    temp = FullString(i)
                Else
                    'Break the 1st range reference into two parts (column refs)
                    PartString = Split(FullString(i), ":")

                    'Get 1st part of range reference without row numbers
                    '(Val function does not recognize zeros as numbers)
                    Str1 = Split(PartString(0), StrReverse(Val(StrReverse(PartString(0)))))
                    Str1 = Str1(0)

                    'Get 2nd part of range reference without row numbers
                    Str2 = Split(PartString(1), StrReverse(Val(StrReverse(PartString(1)))))
                    Str2 = Str2(0)

                    'Assemble the entire reference with row numbers
                    temp = Str1 & CStr(Me.StartRow) & ":" & Str2 & CStr(Me.nRows + (Me.StartRow - 1))
                End If
                TestVal = .Range(temp) 'Set range object equal to range on worksheet

                'Iterate through the columns and add to data matrix
                For k = 1 To .Range(temp).Columns.Count
                    'Check for numeric variable names
                    If TypeOf TestVal(1, k).Value Is ExcelMissing Or TypeOf TestVal(1, k).Value Is ExcelEmpty Or TestVal(1, k).Value Is Nothing Then
                        Me.varNames(i) = "Var" + ColNumber2Letter(TestVal.Column) 'default name
                        For j = 1 To Me.nRows  'Load return matrix
                            Me.RawData(j + Me.StartRow - 1, i) = TestVal(j, k).value
                        Next
                    Else 'Load variable names
                        Me.varNames(i) = TestVal(1, k).value
                        For j = 1 To Me.nRows
                            Me.RawData(j + Me.StartRow - 1, i) = TestVal(j, k).value
                        Next
                    End If
                Next
            Next i
        End With
    End Sub

    ''' <summary>
    ''' Removes rows containing missing values from the raw data matrix and produces the cleaned <c>FinalData</c>.
    ''' </summary>
    ''' <param name="CharCols">Optional. Number of leading columns allowed to contain character data. Default = -1.</param>
    ''' <param name="SkipRow">Optional. Number of rows to skip at the top of the range. Default = 0.</param>
    ''' <remarks>
    ''' - Updates <c>FinalData</c>, <c>RowIds</c>, and <c>varNames</c>.  
    ''' - Handles missing values depending on <c>bAllowMissing</c>.  
    ''' - Drops columns entirely if all values are missing.  
    ''' </remarks>
    ''' <example>
    ''' Me.RemoveMissing(CharCols:=1, SkipRow:=1)
    ''' Console.WriteLine("Cleaned rows: " + Me.nRows)
    ''' </example>
    Private Sub RemoveMissing(Optional CharCols As Integer = -1, Optional SkipRow As Integer = 0)
        'Subroutine removes rows from the input matrix that contain missing values. The returned matrix is redimensioned.
        Dim temp(,) As Object, NoMissing As Integer, cnt As Integer, i As Integer, j As Integer
        Dim haveChar = New List(Of Integer)
        'Dimension temporal matrix and return missing obs vector
        ReDim temp(Me.nRows + Me.StartRow - 1, Me.nCols - 1), RowIds(Me.nRows + Me.StartRow - 1)

        For i = (Me.StartRow + SkipRow) To (Me.nRows + Me.StartRow - 1)
            cnt += 1
            Dim tmpChars = New List(Of Integer)
            Dim currentMiss As Integer = 0
            For j = 0 To Me.nCols - 1
                If TypeOf Me.RawData(i, j) Is ExcelEmpty Or TypeOf Me.RawData(i, j) Is ExcelMissing Or TypeOf Me.RawData(i, j) Is ExcelError Then
                    If Me.pbAllowMissing Then
                        currentMiss += 1
                        Me.RawData(i, j) = Nothing
                    Else
                        NoMissing += 1
                        cnt -= 1
                        Exit For 'Remove entire row
                    End If
                ElseIf IsNumeric(Me.RawData(i, j)) Then
                    If TypeOf Me.RawData(i, j) Is String Then tmpChars.Add(j) 'number stored as text. Convert everything to text because we may sort by this column
                    Continue For
                ElseIf Not IsNumeric(Me.RawData(i, j)) And CharCols = -1 Then
                    If Me.pbAllowMissing Then
                        currentMiss += 1
                        Me.RawData(i, j) = Nothing
                    Else
                        NoMissing += 1
                        cnt -= 1
                        Exit For 'Remove entire row
                    End If
                ElseIf Not IsNumeric(Me.RawData(i, j)) And CharCols > -1 And j <= CharCols Then 'accept char data but only in the 1st # of specified columns
                    tmpChars.Add(j)
                    Continue For
                ElseIf Not IsNumeric(Me.RawData(i, j)) And j > CharCols Then
                    If Me.pbAllowMissing Then
                        currentMiss += 1
                        Me.RawData(i, j) = Nothing
                    Else
                        NoMissing += 1
                        cnt -= 1
                        Exit For 'Remove entire row
                    End If
                End If
            Next j

            If Me.pbAllowMissing Then 'we can have missing
                If currentMiss = Me.nCols Then 'all columns are missing, even if we allow missing drop the record
                    NoMissing += 1
                    cnt -= 1
                Else
                    RowIds(cnt - 1) = i
                End If
            Else
                If j = Me.nCols Then 'check for character columns
                    RowIds(cnt - 1) = i
                    If CharCols > -1 Or tmpChars.Count > 0 Then
                        For Each xxx In tmpChars
                            If Not haveChar.Contains(xxx) Then haveChar.Add(xxx)
                        Next
                    End If
                End If
            End If
        Next

        'Redimension the temporary array to equal the ouput array
        If cnt = 0 Then 'zero valid data
            AppGlobals.BSlogg.Log("Zero valid matched data!")
            Exit Sub
        Else
            Me.bZeroValid = False
            ReDim Me.FinalData(cnt - 1, Me.nCols - 1)
            ReDim Preserve RowIds(cnt - 1)
            Me.nRows = cnt
        End If

        'check if any variable have all values missing
        Dim ColToDrop = New List(Of Integer)
        If Me.pbAllowMissing Then
            For j = 0 To nCols - 1
                Dim nMiss As Integer = 0
                For i = 0 To cnt - 1
                    If Me.RawData(RowIds(i), j) Is Nothing Then nMiss += 1
                Next
                If nMiss = cnt Then ColToDrop.Add(j) 'delete whole column
            Next
        End If

        'Dump the matrix with deleted rows into the resized array
        For i = 0 To cnt - 1
            Dim jj As Integer = 0
            For j = 0 To nCols - 1
                If Me.pbAllowMissing Then
                    If Not ColToDrop.Contains(j) Then 'if column contains no data then drop it
                        Me.FinalData(i, jj) = Me.RawData(RowIds(i), j)
                        jj += 1
                    End If
                Else
                    'If CharCols > -1 Or haveChar.Contains(j) Then 'we had some character value in this column so convert everything to char
                    If (CharCols > -1 AndAlso j <= CharCols) OrElse haveChar.Contains(j) Then 'convert only declared/text columns to text
                            Me.FinalData(i, j) = CStr(Me.RawData(RowIds(i), j))
                        Else
                            Me.FinalData(i, j) = Me.RawData(RowIds(i), j)
                    End If
                End If
            Next
        Next i

        'If we droped any variable then update the variable name list also
        If Me.pbAllowMissing AndAlso ColToDrop.Count > 0 Then
            Dim jj As Integer = 0
            For j = 0 To nCols - 1
                If Not ColToDrop.Contains(j) Then
                    Me.varNames(jj) = Me.varNames(j)
                    jj += 1
                End If
            Next
            ReDim Preserve Me.varNames(jj - 1)
        End If
    End Sub

    ''' <summary>
    ''' Subsets the <c>FinalData</c> matrix to include only rows matching the specified row IDs.
    ''' </summary>
    ''' <param name="rIds">An array of row IDs to retain.</param>
    ''' <remarks>
    ''' - Updates <c>FinalData</c> and <c>RowIds</c>.  
    ''' - Useful for composite models (e.g., Zero-Inflated Poisson) to align multiple data objects.  
    ''' </remarks>
    ''' <example>
    ''' Dim ids() As Integer = {2, 5, 7}
    ''' Me.SubsetByRowIdValues(ids)
    ''' Console.WriteLine("Subset rows: " + Me.nRows)
    ''' </example>
    Public Overridable Sub SubsetByRowIdValues(rIds As Dictionary(Of Integer, Integer))
        Me.FinalData = SubsetArrayByIds(Me.FinalData, rIds)
        Me.RowIds = rIds.Values.ToArray()
    End Sub
End Class

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
''' ' Example: import GLM data with weights
''' Dim glm As New glmData()
''' glm.bWeights = True
''' glm.DataInport("Sheet1!A:D", bStartRow:=True)
''' Console.WriteLine("Rows: " + glm.nRows + ", Cols: " + glm.nCols)
''' Console.WriteLine("Weight variable: " + glm.OffsetVarName)
''' </example>
Public Class glmData
    Inherits DataObj

    ''' <summary>Flag indicating whether weight data is present.</summary>
    Public bWeights As Boolean
    ''' <summary>Flag indicating whether offset data is present.</summary>
    Public bOffset As Boolean
    ''' <summary>Array of offset values extracted from the worksheet.</summary>
    Public OffsetData() As Double
    ''' <summary>Array of weight values extracted from the worksheet.</summary>
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
        Me.FinalData = SubsetArrayByIds(Me.FinalData, rIds)
        If Me.bOffset Then Me.OffsetData = Subset1DArrayByIds(Me.OffsetData, rIds)
        If Me.bWeights Then Me.WeightData = Subset1DArrayByIds(Me.WeightData, rIds)
        Me.RowIds = rIds.Values.ToArray()
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

    ''' <summary>
    ''' Imports data from a worksheet reference into the <c>glmData</c> object,
    ''' separating offset and weight columns if enabled.
    ''' </summary>
    ''' <param name="ref">The reference string specifying the worksheet range.</param>
    ''' <param name="bStartRow">If <c>True</c>, determines the starting row from the reference string; otherwise defaults to 1.</param>
    ''' <param name="CharCols">Optional. Number of leading columns allowed to contain character data. Default = -1.</param>
    ''' <param name="SkipRow">Optional. Number of rows to skip at the top of the range. Default = 0.</param>
    ''' <remarks>
    ''' - Calls <c>DataObj.DataInport</c> to import the base data.  
    ''' - If <c>bWeights</c> is <c>True</c>, the last column is extracted into <c>WeightData</c> and removed from <c>FinalData</c>.  
    ''' - If <c>bOffset</c> is <c>True</c>, the last column is extracted into <c>OffsetData</c> and removed from <c>FinalData</c>.  
    ''' - Updates <c>WeightVarName</c> and <c>OffsetVarName</c> accordingly.  
    ''' </remarks>
    ''' <example>
    ''' ' Example: import GLM data with offset
    ''' Dim glm As New glmData()
    ''' glm.bOffset = True
    ''' glm.DataInport("Sheet1!A:D", bStartRow:=True)
    ''' Console.WriteLine("Offset variable: " + glm.WeightVarName)
    ''' </example>
    Public Overrides Sub DataInport(ByRef ref As String, Optional bStartRow As Boolean = False,
                               Optional CharCols As Integer = -1, Optional SkipRow As Integer = 0)

        MyBase.DataInport(ref, bStartRow, CharCols, SkipRow)
        SplitOffsetAndWeights()
    End Sub

    Public Overrides Sub DataImportRawMatrix(rawInput(,) As Object,
                                         variableNames() As String,
                                         Optional firstSourceRow As Integer = 1,
                                         Optional sourceWorksheet As Worksheet = Nothing,
                                         Optional CharCols As Integer = -1,
                                         Optional SkipRow As Integer = 0)

        MyBase.DataImportRawMatrix(rawInput, variableNames, firstSourceRow, sourceWorksheet, CharCols, SkipRow)
        SplitOffsetAndWeights()
    End Sub
End Class

Public Class geeData
    Inherits DataObj


    ''' <summary>Flag indicating whether weight data is present.</summary>
    Public bWeights As Boolean
    ''' <summary>Flag indicating whether offset data is present.</summary>
    Public bOffset As Boolean
    ''' <summary>Array of offset values extracted from the worksheet.</summary>
    Public OffsetData() As Double
    ''' <summary>Array of weight values extracted from the worksheet.</summary>
    Public WeightData() As Double
    ''' <summary>Name of the offset variable (last column in the imported range).</summary>
    Public OffsetVarName As String
    ''' <summary>Name of the weight variable (last column in the imported range).</summary>
    Public WeightVarName As String
    ''' <summary>Flag indicating whether Time (within cluster orderig) data is present. It's used in the GEE.</summary>
    Public bTime As Boolean
    ''' <summary>Array of Time (within cluster orderig) data values extracted from the worksheet.</summary>
    Public TimeData() As Double
    ''' <summary>Array of Cluster ID data values extracted from the worksheet.</summary>
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
        Me.FinalData = SubsetArrayByIds(Me.FinalData, rIds)
        Me.ClusterIdData = Subset1DArrayByIds(Me.ClusterIdData, rIds)
        If Me.bOffset Then Me.OffsetData = Subset1DArrayByIds(Me.OffsetData, rIds)
        If Me.bWeights Then Me.WeightData = Subset1DArrayByIds(Me.WeightData, rIds)
        If Me.bTime Then Me.TimeData = Subset1DArrayByIds(Me.TimeData, rIds)
        Me.RowIds = rIds.Values.ToArray()
    End Sub

    ''' <summary>
    ''' Imports data from a worksheet reference into the <c>glmData</c> object,
    ''' separating offset and weight columns if enabled.
    ''' </summary>
    ''' <param name="ref">The reference string specifying the worksheet range.</param>
    ''' <param name="bStartRow">If <c>True</c>, determines the starting row from the reference string; otherwise defaults to 1.</param>
    ''' <param name="CharCols">Optional. Number of leading columns allowed to contain character data. Default = -1.</param>
    ''' <param name="SkipRow">Optional. Number of rows to skip at the top of the range. Default = 0.</param>
    ''' <remarks>
    ''' - Calls <c>DataObj.DataInport</c> to import the base data.  
    ''' - If <c>bWeights</c> is <c>True</c>, the last column is extracted into <c>WeightData</c> and removed from <c>FinalData</c>.  
    ''' - If <c>bOffset</c> is <c>True</c>, the last column is extracted into <c>OffsetData</c> and removed from <c>FinalData</c>.  
    ''' - Updates <c>WeightVarName</c> and <c>OffsetVarName</c> accordingly.  
    ''' </remarks>
    ''' <example>
    ''' ' Example: import GLM data with offset
    ''' Dim glm As New glmData()
    ''' glm.bOffset = True
    ''' glm.DataInport("Sheet1!A:D", bStartRow:=True)
    ''' Console.WriteLine("Offset variable: " + glm.WeightVarName)
    ''' </example>
    Public Overrides Sub DataInport(ByRef ref As String, Optional bStartRow As Boolean = False,
                                   Optional CharCols As Integer = -1, Optional SkipRow As Integer = 0)

        MyBase.DataInport(ref, bStartRow, CharCols, SkipRow)
        FinalizeGeeImport()
    End Sub

    Public Overrides Sub DataImportRawMatrix(rawInput(,) As Object,
                                         variableNames() As String,
                                         Optional firstSourceRow As Integer = 1,
                                         Optional sourceWorksheet As Worksheet = Nothing,
                                         Optional CharCols As Integer = -1,
                                         Optional SkipRow As Integer = 0)

        MyBase.DataImportRawMatrix(rawInput, variableNames, firstSourceRow, sourceWorksheet, CharCols, SkipRow)
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
            QuickSort2D(data2, $"{iClasterPos},A,{iClasterPos + 1},A", 0, Me.nRows - 1)
        Else
            'Order is get by source doata ordering (i.e. RowIds)
            QuickSort2D(data2, $"{iClasterPos},A,{Me.nCols},A", 0, Me.nRows - 1)
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



''' <summary>
''' Specialized data container for Cox proportional hazards (CoxPH) survival models,
''' extending <see cref="DataObj"/>.
''' </summary>
''' <remarks>
''' <para>
''' The <c>CoxPHData</c> class inherits all functionality from <c>DataObj</c> (importing, cleaning, subsetting)
''' and adds support for CoxPH-specific features:
''' </para>
''' <list type="bullet">
'''   <item><description>Mandatory time-to-event variable (first column).</description></item>
'''   <item><description>Mandatory censoring indicator (second column, must be 0 or 1).</description></item>
'''   <item><description>Optional strata variable (third column) for stratified Cox models.</description></item>
'''   <item><description>Automatic separation of time, censoring, and strata columns from covariates.</description></item>
'''   <item><description>Creation of <c>SurvivalRecord</c> objects for each observation.</description></item>
''' </list>
''' </remarks>
''' <example>
''' ' Example: import CoxPH data with strata
''' Dim cox As New CoxPHData()
''' cox.bStrata = True
''' cox.DataInport("Sheet1!A:D", bStartRow:=True)
''' Console.WriteLine("Rows: " + cox.nRows + ", Cols: " + cox.nCols)
''' Console.WriteLine("Time variable: " + cox.TimeVarName)
''' Console.WriteLine("Censor variable: " + cox.CensorVarName)
''' Console.WriteLine("Strata variable: " + cox.StrataVarName)
''' </example>
Public Class CoxPHData
    Inherits DataObj

    ''' <summary>Flag indicating whether a strata variable is present.</summary>
    Public bStrata As Boolean

    ''' <summary>Array of strata values extracted from the worksheet (optional).</summary>
    Public StrataData() As String

    ''' <summary>Array of censoring indicators (0 = event, 1 = censored).</summary>
    Public CensorData() As Integer

    ''' <summary>Array of time-to-event values.</summary>
    Public TimeData() As Double

    ''' <summary>Name of the strata variable (third column if present).</summary>
    Public StrataVarName As String

    ''' <summary>Name of the censoring variable (second column).</summary>
    Public CensorVarName As String

    ''' <summary>Name of the time variable (first column).</summary>
    Public TimeVarName As String

    ''' <summary>List of <see cref="survival.SurvivalRecord"/> objects representing each observation.</summary>
    Public SurvRecordsList = New List(Of survival.SurvivalRecord)

    ''' <summary>
    ''' Initializes a new instance of the <c>CoxPHData</c> class with strata disabled.
    ''' </summary>
    Sub New()
        MyBase.New()
        Me.bStrata = False
    End Sub

    ''' <summary>
    ''' Imports data from a worksheet reference into the <c>CoxPHData</c> object,
    ''' separating time, censoring, and optional strata variables from covariates.
    ''' </summary>
    ''' <param name="ref">The reference string specifying the worksheet range.</param>
    ''' <param name="bStartRow">If <c>True</c>, determines the starting row from the reference string; otherwise defaults to 1.</param>
    ''' <param name="CharCols">Optional. Number of leading columns allowed to contain character data. Default = -1.</param>
    ''' <param name="SkipRow">Optional. Number of rows to skip at the top of the range. Default = 0.</param>
    ''' <remarks>
    ''' - Calls <c>DataObj.DataInport</c> to import the base data.  
    ''' - First column is always treated as time-to-event.  
    ''' - Second column must be a censoring indicator (0 or 1).  
    ''' - If <c>bStrata</c> is <c>True</c>, third column is treated as strata.  
    ''' - Remaining columns are treated as covariates.  
    ''' - Populates <c>SurvRecordsList</c> with <c>SurvivalRecord</c> objects.  
    ''' </remarks>
    ''' <example>
    ''' ' Example: import CoxPH data without strata
    ''' Dim cox As New CoxPHData()
    ''' cox.DataInport("Sheet1!A:C", bStartRow:=True)
    ''' Console.WriteLine("Time variable: " + cox.TimeVarName)
    ''' Console.WriteLine("Censor variable: " + cox.CensorVarName)
    ''' Console.WriteLine("Covariates: " + String.Join(",", cox.varNames))
    ''' </example>
    Public Overrides Sub DataInport(ByRef ref As String, Optional bStartRow As Boolean = False,
                                Optional CharCols As Integer = -1, Optional SkipRow As Integer = 0)

        Dim effectiveCharCols As Integer = CharCols
        If Me.bStrata AndAlso effectiveCharCols < 2 Then
            effectiveCharCols = 2
        End If

        MyBase.DataInport(ref, bStartRow, effectiveCharCols, SkipRow)
        FinalizeCoxImport()
    End Sub

    Public Overrides Sub DataImportRawMatrix(rawInput(,) As Object,
                                         variableNames() As String,
                                         Optional firstSourceRow As Integer = 1,
                                         Optional sourceWorksheet As Worksheet = Nothing,
                                         Optional CharCols As Integer = -1,
                                         Optional SkipRow As Integer = 0)

        Dim effectiveCharCols As Integer = CharCols
        If Me.bStrata AndAlso effectiveCharCols < 2 Then
            effectiveCharCols = 2
        End If

        MyBase.DataImportRawMatrix(rawInput, variableNames, firstSourceRow, sourceWorksheet, effectiveCharCols, SkipRow)
        FinalizeCoxImport()
    End Sub

    Private Sub FinalizeCoxImport()
        ReDim TimeData(Me.nRows - 1), CensorData(Me.nRows - 1)

        For i = 0 To Me.nRows - 1
            Me.TimeData(i) = CDbl(Me.FinalData(i, 0))

            If Not (Me.FinalData(i, 1) = 0 Or Me.FinalData(i, 1) = 1) Then
                AppGlobals.BSerr.LogAndThrow(New ArgumentException($"Censorting value is not 1/0. Value ={Me.FinalData(i, 1)}"))
            End If

            Me.CensorData(i) = CInt(Me.FinalData(i, 1))
            Me.TimeVarName = Me.varNames(0)
            Me.CensorVarName = Me.varNames(1)
        Next

        If Me.bStrata Then
            ReDim Me.StrataData(Me.nRows - 1)
            For i = 0 To Me.nRows - 1
                Me.StrataData(i) = CStr(Me.FinalData(i, 2))
            Next
            Me.StrataVarName = Me.varNames(2)

            For i = 0 To Me.nRows - 1
                For j = 3 To Me.nCols - 1
                    Me.FinalData(i, j - 3) = Me.FinalData(i, j)
                    If i = 0 Then Me.varNames(j - 3) = Me.varNames(j)
                Next
            Next

            ReDim Preserve Me.FinalData(Me.nRows - 1, Me.nCols - 4)
            ReDim Preserve Me.varNames(Me.nCols - 4)
            Me.nCols -= 3
        Else
            For i = 0 To Me.nRows - 1
                For j = 2 To Me.nCols - 1
                    Me.FinalData(i, j - 2) = Me.FinalData(i, j)
                    If i = 0 Then Me.varNames(j - 2) = Me.varNames(j)
                Next
            Next

            ReDim Preserve Me.FinalData(Me.nRows - 1, Me.nCols - 3)
            ReDim Preserve Me.varNames(Me.nCols - 3)
            Me.nCols -= 2
        End If

        SurvRecordsList.Clear()
        For i = 0 To Me.nRows - 1
            Dim Xs(Me.nCols - 1) As Double
            For j = 0 To Me.nCols - 1
                Xs(j) = CDbl(Me.FinalData(i, j))
            Next

            Dim sr = New survival.SurvivalRecord
            sr.Censorship = Me.CensorData(i)
            sr.Stratum = If(Me.bStrata, Me.StrataData(i), "0")
            sr.Time = Me.TimeData(i)
            sr.Index = Me.RowIds(i)
            sr.Covariates = Xs
            SurvRecordsList.Add(sr)
        Next
    End Sub
End Class