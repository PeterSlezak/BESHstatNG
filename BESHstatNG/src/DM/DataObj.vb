Option Explicit On

Imports BESHStatNG.DataManagement

''' <summary>
''' Represents a data container and utility class for importing, cleaning,
''' and transforming imported data into structured arrays suitable for statistical analysis.
''' </summary>
''' <remarks>
''' <para>
''' The <c>DataObj</c> class is a host-neutral statistical data container. Front-end importers
''' convert host-specific ranges or values to <see cref="CoreDataTable"/> before loading them here. It provides functionality for:
''' </para>
''' <list type="bullet">
'''   <item><description>Importing raw matrices and host-neutral <c>CoreDataTable</c> instances.</description></item>
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
'''   <item><description><c>SourceInfo</c>: Host-neutral source metadata such as address and sheet name.</description></item>
'''   <item><description><c>varNames()</c>: Names of variables (columns).</description></item>
'''   <item><description><c>RawData(,)</c>: Imported raw values indexed by source row.</description></item>
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
'''   <item><description>Load a <c>CoreDataTable</c> or call <c>DataImportRawMatrix</c> with host-neutral values.</description></item>
'''   <item><description>Use <c>RemoveMissing</c> to clean rows with missing values.</description></item>
'''   <item><description>Access <c>FinalData</c>, <c>DataDbl</c>, or <c>DataByID2ByColumn</c> for analysis.</description></item>
'''   <item><description>Optionally subset rows with <c>SubsetByRowIdValues</c>.</description></item>
''' </list>
''' </remarks>
''' <example>
''' ' Example: Import and clean a host-neutral raw matrix
''' Dim dObj As New DataObj()
''' Dim raw(,) As Object = {{1.0, 2.0}, {3.0, 4.0}}
''' dObj.DataImportRawMatrix(raw, New String() {"Y", "X"})
''' Dim cleanData As Double(,) = dObj.DataDbl
''' Console.WriteLine("Rows: " + dObj.nRows + ", Cols: " + dObj.nCols)
''' </example>
Public Class DataObj
    Public varNames() As String 'array that contains the variable names
    Public RawData(,) As Object 'raw imported values indexed by source row
    Public RowIds() As Integer = Nothing 'source row numbers where we have valid data
    Public FinalData(,) As Object
    Public nCols As Integer
    Public nRows As Integer
    Public bZeroValid As Boolean
    Public SourceInfo As DataSourceInfo = Nothing
    Public CoreTable As CoreDataTable = Nothing
    Private StartRow As Integer
    Private pbAllowMissing As Boolean = False

    Public ReadOnly Property AllowMissing As Boolean
        Get
            Return pbAllowMissing
        End Get
    End Property


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
            groupIDs = ArrayUtilities.GetColumn(Me.FinalData, 0).Distinct().ToArray()
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
                out(j) = ArrayUtilities.Slice(ArrayUtilities.GetColumn(arDataColumn, j), 0, arN(j) - 1)
            Next

            Return out
        End Get

    End Property

    ''' <summary>
    ''' Loads this DataObj from a host-neutral table model.  Excel/Office.js/Google Sheets importers should convert
    ''' their host-specific ranges into CoreDataTable first, then call this method.
    ''' </summary>
    ''' <param name="cloneTable">
    ''' If <c>True</c> (default), stores a defensive clone in <see cref="CoreTable"/>.  Pass <c>False</c> only when
    ''' the caller created the <see cref="CoreDataTable"/> specifically for this <see cref="DataObj"/> and will not
    ''' mutate it afterwards.  This avoids one full-size copy of ObjectMatrix/NumericMatrix/MissingMask for large data.
    ''' </param>
    Public Overridable Sub LoadCoreDataTable(table As CoreDataTable,
                                            Optional CharCols As Integer = -1,
                                            Optional SkipRow As Integer = 0,
                                            Optional cloneTable As Boolean = True)
        If table Is Nothing Then Throw New ArgumentNullException(NameOf(table))
        If table.ObjectMatrix Is Nothing Then Throw New ArgumentException("CoreDataTable.ObjectMatrix cannot be Nothing.")
        If table.ColumnNames Is Nothing Then Throw New ArgumentException("CoreDataTable.ColumnNames cannot be Nothing.")

        Dim rows As Integer = table.ObjectMatrix.GetLength(0)
        Dim cols As Integer = table.ObjectMatrix.GetLength(1)
        If rows < 1 OrElse cols < 1 Then Throw New ArgumentException("CoreDataTable must contain at least one row and one column.")
        If table.ColumnNames.Length <> cols Then Throw New ArgumentException($"ColumnNames length ({table.ColumnNames.Length}) must match the number of columns ({cols}).")

        Me.CoreTable = If(cloneTable, table.Clone(), table)
        Me.nRows = rows
        Me.nCols = cols
        Me.StartRow = Math.Max(1, table.FirstSourceRow)

        ReDim Me.varNames(cols - 1)
        For j As Integer = 0 To cols - 1
            Me.varNames(j) = If(table.ColumnNames(j), String.Empty)
        Next

        ReDim Me.RawData(Me.StartRow + rows - 1, cols - 1)
        For i As Integer = 0 To rows - 1
            For j As Integer = 0 To cols - 1
                Me.RawData(Me.StartRow + i, j) = table.ObjectMatrix(i, j)
            Next
        Next

        If table.SourceInfo IsNot Nothing Then
            Me.SourceInfo = New DataSourceInfo With {
                .SourceKind = table.SourceInfo.SourceKind,
                .Address = table.SourceInfo.Address,
                .SheetName = table.SourceInfo.SheetName,
                .FirstSourceRow = table.SourceInfo.FirstSourceRow,
                .FirstSourceColumn = table.SourceInfo.FirstSourceColumn,
                .ColumnNames = CoreDataTable.CopyColumnNames(table.SourceInfo.ColumnNames)
            }
        Else
            Me.SourceInfo = New DataSourceInfo With {
                .SourceKind = "CoreDataTable",
                .FirstSourceRow = Me.StartRow,
                .ColumnNames = CoreDataTable.CopyColumnNames(Me.varNames)
            }
        End If

        Me.RemoveMissing(CharCols, SkipRow)
        If Not Me.bZeroValid Then OnDataImported()
    End Sub

    Public Overridable Sub DataImportRawMatrix(rawInput(,) As Object,
                                           variableNames() As String,
                                           Optional firstSourceRow As Integer = 1,
                                           Optional sourceInfo As DataSourceInfo = Nothing,
                                           Optional CharCols As Integer = -1,
                                           Optional SkipRow As Integer = 0)

        Dim source As DataSourceInfo
        If sourceInfo Is Nothing Then
            source = New DataSourceInfo With {
                .SourceKind = "RawMatrix",
                .FirstSourceRow = Math.Max(1, firstSourceRow),
                .FirstSourceColumn = 1,
                .ColumnNames = CoreDataTable.CopyColumnNames(variableNames)
            }
        Else
            source = New DataSourceInfo With {
                .SourceKind = If(String.IsNullOrWhiteSpace(sourceInfo.SourceKind), "RawMatrix", sourceInfo.SourceKind),
                .Address = sourceInfo.Address,
                .SheetName = sourceInfo.SheetName,
                .FirstSourceRow = Math.Max(1, firstSourceRow),
                .FirstSourceColumn = Math.Max(1, sourceInfo.FirstSourceColumn),
                .ColumnNames = CoreDataTable.CopyColumnNames(variableNames)
            }
        End If

        Me.LoadCoreDataTable(CoreDataTable.FromObjectMatrix(rawInput, variableNames, firstSourceRow:=firstSourceRow, sourceInfo:=source),
                             CharCols:=CharCols,
                             SkipRow:=SkipRow,
                             cloneTable:=False)
    End Sub

    Public Overridable Sub DataImportRawMatrixWithOptions(rawInput(,) As Object,
                                                         variableNames() As String,
                                                         Optional options As DataImportOptions = Nothing)
        If options Is Nothing Then options = New DataImportOptions()

        Me.pbAllowMissing = options.AllowMissing
        Me.DataImportRawMatrix(rawInput,
                               variableNames,
                               firstSourceRow:=options.FirstSourceRow,
                               sourceInfo:=options.SourceInfo,
                               CharCols:=options.CharCols,
                               SkipRow:=options.SkipRows)

        If Me.SourceInfo Is Nothing Then Me.SourceInfo = New DataSourceInfo()
        Me.SourceInfo.SourceKind = If(String.IsNullOrWhiteSpace(options.SourceKind), "RawMatrix", options.SourceKind)
        Me.SourceInfo.Address = options.SourceAddress
    End Sub

    Protected Overridable Sub OnDataImported()
        'Specialized data objects can split weights, offsets, time, strata, etc. after the common import/cleaning path.
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
        Dim NoMissing As Integer, cnt As Integer, i As Integer, j As Integer
        Dim haveChar = New List(Of Integer)
        'Dimension temporal matrix and return missing obs vector
        Me.bZeroValid = False
        Me.FinalData = Nothing

        'Dimension return missing obs vector
        ReDim RowIds(Me.nRows + Me.StartRow - 1)

        For i = (Me.StartRow + SkipRow) To (Me.nRows + Me.StartRow - 1)
            cnt += 1
            Dim tmpChars = New List(Of Integer)
            Dim currentMiss As Integer = 0
            For j = 0 To Me.nCols - 1
                'If TypeOf Me.RawData(i, j) Is ExcelEmpty Or TypeOf Me.RawData(i, j) Is ExcelMissing Or TypeOf Me.RawData(i, j) Is ExcelError Then
                If CoreDataTable.IsMissingValue(Me.RawData(i, j)) Then
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
                    If CharCols > -1 Or tmpChars.Count > 0 Then
                        For Each xxx In tmpChars
                            If Not haveChar.Contains(xxx) Then haveChar.Add(xxx)
                        Next
                    End If
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

        If cnt = 0 Then 'zero valid data
            AppInfrastructure.CoreServices.Log("Zero valid matched data!")
            Me.bZeroValid = True
            Me.nRows = 0
            Me.FinalData = Nothing
            Me.RowIds = New Integer() {}
            Exit Sub
        End If

        ReDim Preserve RowIds(cnt - 1)
        Me.nRows = cnt

        'check if any variable have all values missing
        Dim originalCols As Integer = Me.nCols
        Dim ColToDrop = New List(Of Integer)
        If Me.pbAllowMissing Then
            For j = 0 To originalCols - 1
                Dim nMiss As Integer = 0
                For i = 0 To cnt - 1
                    If Me.RawData(RowIds(i), j) Is Nothing Then nMiss += 1
                Next
                If nMiss = cnt Then ColToDrop.Add(j) 'delete whole column
            Next
        End If

        Dim retainedCols As Integer = originalCols - ColToDrop.Count
        If retainedCols <= 0 Then
            AppInfrastructure.CoreServices.Log("Zero valid variables after dropping all-missing columns!")
            Me.bZeroValid = True
            Me.nCols = 0
            Me.FinalData = Nothing
            Me.varNames = New String() {}
            Exit Sub
        End If

        ReDim Me.FinalData(cnt - 1, retainedCols - 1)

        'Dump the matrix with deleted rows into the resized array
        For i = 0 To cnt - 1
            Dim jj As Integer = 0
            For j = 0 To originalCols - 1
                If Not ColToDrop.Contains(j) Then
                    If (CharCols > -1 AndAlso j <= CharCols) OrElse haveChar.Contains(j) Then 'convert only declared/text columns to text
                        If Me.RawData(RowIds(i), j) Is Nothing Then
                            Me.FinalData(i, jj) = Nothing
                        Else
                            Me.FinalData(i, jj) = CStr(Me.RawData(RowIds(i), j))
                        End If
                    Else
                        Me.FinalData(i, jj) = Me.RawData(RowIds(i), j)
                    End If
                    jj += 1
                End If
            Next
        Next i

        'If we dropped any variable then update the variable name list also
        If ColToDrop.Count > 0 Then
            Dim newNames(retainedCols - 1) As String
            Dim jj As Integer = 0
            For j = 0 To originalCols - 1
                If Not ColToDrop.Contains(j) Then
                    newNames(jj) = Me.varNames(j)
                    jj += 1
                End If
            Next
            Me.varNames = newNames
            Me.nCols = retainedCols
        Else
            Me.nCols = originalCols
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
        Me.FinalData = RowArrayUtilities.SubsetRowsByIds(Me.FinalData, rIds.Keys)
        Me.RowIds = rIds.Values.ToArray()
    End Sub
End Class
