Option Explicit On
Option Strict On
Option Infer On

Imports System
Imports System.Collections.Generic
Imports System.Globalization
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
            Dim rowCount As Integer = Me.FinalData.GetLength(0)
            Dim columnCount As Integer = Me.FinalData.GetLength(1)
            Dim d(rowCount - 1, columnCount - 1) As Double

            For i As Integer = 0 To rowCount - 1
                For j As Integer = 0 To columnCount - 1
                    If Me.pbAllowMissing AndAlso Me.FinalData(i, j) Is Nothing Then
                        d(i, j) = Double.NaN
                    Else
                        d(i, j) = Convert.ToDouble(Me.FinalData(i, j), CultureInfo.CurrentCulture)
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
            Dim rowCount As Integer = Me.FinalData.GetLength(0)
            Dim groupIds As New List(Of Object)()
            For i As Integer = 0 To rowCount - 1
                Dim groupId As Object = Me.FinalData(i, 0)
                If Not groupIds.Contains(groupId) Then groupIds.Add(groupId)
            Next

            Dim groupCount As Integer = groupIds.Count
            Dim arDataColumn(rowCount - 1, groupCount - 1) As Double
            Dim counts(groupCount - 1) As Integer

            For i As Integer = 0 To rowCount - 1
                For j As Integer = 0 To groupCount - 1
                    If Object.Equals(groupIds(j), Me.FinalData(i, 0)) Then
                        arDataColumn(counts(j), j) = Convert.ToDouble(Me.FinalData(i, 1), CultureInfo.CurrentCulture)
                        counts(j) += 1
                    End If
                Next
            Next
            Dim output()() As Double = New Double(groupCount - 1)() {}
            For j As Integer = 0 To groupCount - 1
                output(j) = ArrayUtilities.Slice(ArrayUtilities.GetColumn(arDataColumn, j), 0, counts(j) - 1)
            Next

            Return output
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
        Dim noMissing As Integer = 0
        Dim count As Integer = 0
        Dim haveChar As New List(Of Integer)()
        Me.bZeroValid = False
        Me.FinalData = Nothing

        ReDim Me.RowIds(Me.nRows + Me.StartRow - 1)

        For sourceRow As Integer = (Me.StartRow + SkipRow) To (Me.nRows + Me.StartRow - 1)
            count += 1
            Dim tmpChars As New List(Of Integer)()
            Dim currentMiss As Integer = 0
            Dim scannedAllColumns As Boolean = True

            For columnIndex As Integer = 0 To Me.nCols - 1
                Dim value As Object = Me.RawData(sourceRow, columnIndex)
                If CoreDataTable.IsMissingValue(value) Then
                    If Me.pbAllowMissing Then
                        currentMiss += 1
                        Me.RawData(sourceRow, columnIndex) = Nothing
                    Else
                        noMissing += 1
                        count -= 1
                        scannedAllColumns = False
                        Exit For
                    End If
                    Continue For
                End If

                Dim numericValue As Double
                Dim isNumericValue As Boolean = CoreDataTable.TryConvertToDouble(value, numericValue)

                If isNumericValue Then
                    If TypeOf value Is String Then tmpChars.Add(columnIndex)
                    Continue For
                End If

                If CharCols = -1 Then
                    If Me.pbAllowMissing Then
                        currentMiss += 1
                        Me.RawData(sourceRow, columnIndex) = Nothing
                    Else
                        noMissing += 1
                        count -= 1
                        scannedAllColumns = False
                        Exit For
                    End If
                ElseIf columnIndex <= CharCols Then
                    tmpChars.Add(columnIndex)
                Else
                    If Me.pbAllowMissing Then
                        currentMiss += 1
                        Me.RawData(sourceRow, columnIndex) = Nothing
                    Else
                        noMissing += 1
                        count -= 1
                        scannedAllColumns = False
                        Exit For
                    End If
                End If
            Next

            If Me.pbAllowMissing Then
                If currentMiss = Me.nCols Then
                    noMissing += 1
                    count -= 1
                Else

                    Me.RowIds(count - 1) = sourceRow
                    For Each charColumn As Integer In tmpChars
                        If Not haveChar.Contains(charColumn) Then haveChar.Add(charColumn)
                    Next
                End If
            ElseIf scannedAllColumns Then
                Me.RowIds(count - 1) = sourceRow
                For Each charColumn As Integer In tmpChars
                    If Not haveChar.Contains(charColumn) Then haveChar.Add(charColumn)
                Next
            End If
        Next

        If count = 0 Then 'zero valid data
            AppInfrastructure.CoreServices.Log("Zero valid matched data!")
            Me.bZeroValid = True
            Me.nRows = 0
            Me.FinalData = Nothing
            Me.RowIds = Array.Empty(Of Integer)()
            Exit Sub
        End If

        ReDim Preserve Me.RowIds(count - 1)
        Me.nRows = count

        'check if any variable have all values missing
        Dim originalCols As Integer = Me.nCols
        Dim columnsToDrop As New List(Of Integer)()
        If Me.pbAllowMissing Then
            For columnIndex As Integer = 0 To originalCols - 1
                Dim nMiss As Integer = 0
                For rowIndex As Integer = 0 To count - 1
                    If Me.RawData(Me.RowIds(rowIndex), columnIndex) Is Nothing Then nMiss += 1
                Next
                If nMiss = count Then columnsToDrop.Add(columnIndex) 'delete whole column
            Next
        End If

        Dim retainedCols As Integer = originalCols - columnsToDrop.Count
        If retainedCols <= 0 Then
            AppInfrastructure.CoreServices.Log("Zero valid variables after dropping all-missing columns!")
            Me.bZeroValid = True
            Me.nCols = 0
            Me.FinalData = Nothing
            Me.varNames = Array.Empty(Of String)()
            Exit Sub
        End If

        ReDim Me.FinalData(count - 1, retainedCols - 1)
        For rowIndex As Integer = 0 To count - 1
            Dim targetColumn As Integer = 0
            For columnIndex As Integer = 0 To originalCols - 1
                If Not columnsToDrop.Contains(columnIndex) Then
                    Dim value As Object = Me.RawData(Me.RowIds(rowIndex), columnIndex)
                    If (CharCols > -1 AndAlso columnIndex <= CharCols) OrElse haveChar.Contains(columnIndex) Then
                        Me.FinalData(rowIndex, targetColumn) = If(value Is Nothing,
                                                                 Nothing,
                                                                 Convert.ToString(value, CultureInfo.CurrentCulture))
                    Else
                        Me.FinalData(rowIndex, targetColumn) = value
                    End If
                    targetColumn += 1
                End If
            Next
        Next

        'If we dropped any variable then update the variable name list also
        If columnsToDrop.Count > 0 Then
            Dim newNames(retainedCols - 1) As String
            Dim targetColumn As Integer = 0
            For columnIndex As Integer = 0 To originalCols - 1
                If Not columnsToDrop.Contains(columnIndex) Then
                    newNames(targetColumn) = Me.varNames(columnIndex)
                    targetColumn += 1
                End If
            Next
            Me.varNames = newNames
        End If
        Me.nCols = retainedCols
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
        Me.RowIds = New List(Of Integer)(rIds.Values).ToArray()
        Me.nRows = Me.RowIds.Length
    End Sub
End Class
