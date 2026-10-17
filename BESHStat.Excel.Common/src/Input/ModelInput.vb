Option Explicit On
Option Strict On

Imports System
Imports System.Collections.Generic
Imports System.Collections.ObjectModel

Namespace ExcelCommon

    ''' <summary>Input-to-design choices. Column indices are zero-based within the selected rectangle.
    ''' FormulaText uses the existing Core RHS-only grammar (factor(), :, ^ and quoted names).
    ''' Intercept selection is a separate semantic option; y ~ x and +/- 1 are not accepted here.</summary>
    Public NotInheritable Class SpreadsheetModelParseOptions
        Public Property Header As SpreadsheetHeaderMode = SpreadsheetHeaderMode.Required
        Public Property ResponseColumnIndex As Integer = 0
        ''' <summary>Nothing means all columns other than the response, in source order.</summary>
        Public Property PredictorColumnIndices As Integer() = Nothing
        ''' <summary>Explicitly typed categorical predictor indices (within the entire input rectangle).</summary>
        Public Property CategoricalColumnIndices As Integer() = Array.Empty(Of Integer)()
        Public Property FormulaText As String = Nothing
        Public Property IncludeIntercept As Boolean = True
        Public Property Missing As SpreadsheetIncompleteObservationPolicy = SpreadsheetIncompleteObservationPolicy.Reject
        Public Property CellPolicy As SpreadsheetCellPolicyOptions = New SpreadsheetCellPolicyOptions()
        Public Property IgnoreCategoryCase As Boolean = False
        Public Property MinimumCompleteRows As Integer = 2
        Public Property MaximumExpandedColumns As Integer = 2048
        Public Property RejectRankDeficiency As Boolean = False
        Public Property AllowRelativeColumnLetters As Boolean = True
        Public Property AllowAbsoluteColumnLetters As Boolean = False
        Public Property AllowQuotedVariableNames As Boolean = True
    End Class

    Public NotInheritable Class SpreadsheetModelSourceRow
        Public Sub New(selectedRow As Integer, worksheetRow As Integer)
            If selectedRow < 1 OrElse worksheetRow < 0 Then Throw New ArgumentOutOfRangeException(NameOf(selectedRow))
            Me.SelectedRow = selectedRow
            Me.WorksheetRow = worksheetRow
        End Sub
        Public ReadOnly Property SelectedRow As Integer
        Public ReadOnly Property WorksheetRow As Integer
    End Class

    ''' <summary>One exact typed category level and its Core-only numeric encoding.
    ''' Numeric/Boolean-only level sets retain source values. Text/mixed sets
    ''' receive distinct 1-based codes in complete-case first-appearance order.</summary>
    Public NotInheritable Class SpreadsheetModelCategoryLevel
        Public Sub New(value As SpreadsheetCategoryValue, code As Double)
            If value Is Nothing Then Throw New ArgumentNullException(NameOf(value))
            Me.Value = value
            Me.Code = code
        End Sub
        Public ReadOnly Property Value As SpreadsheetCategoryValue
        Public ReadOnly Property Code As Double
    End Class

    Public NotInheritable Class SpreadsheetModelCategoricalColumn
        Private ReadOnly _levels As ReadOnlyCollection(Of SpreadsheetModelCategoryLevel)
        Public Sub New(columnName As String, selectedColumn As Integer,
                       levels As IEnumerable(Of SpreadsheetModelCategoryLevel))
            If levels Is Nothing Then Throw New ArgumentNullException(NameOf(levels))
            Me.ColumnName = If(columnName, String.Empty)
            Me.SelectedColumn = selectedColumn
            _levels = New List(Of SpreadsheetModelCategoryLevel)(levels).AsReadOnly()
        End Sub
        Public ReadOnly Property ColumnName As String
        Public ReadOnly Property SelectedColumn As Integer
        Public ReadOnly Property Levels As IReadOnlyList(Of SpreadsheetModelCategoryLevel)
            Get
                Return _levels
            End Get
        End Property
    End Class

    ''' <summary>Host-neutral compiled model input, not a fitted Core model.
    ''' Public array accessors return copies: later host mutation cannot corrupt a compiled request.
    ''' Predictor column names and matrix columns are aligned, including the optional intercept.</summary>
    Public NotInheritable Class SpreadsheetModelInput
        Inherits SpreadsheetAnalysisInput
        Private ReadOnly _response As Double()
        Private ReadOnly _rawPredictors As Double(,)
        Private ReadOnly _design As Double(,)
        Private ReadOnly _names As ReadOnlyCollection(Of String)
        Private ReadOnly _rawNames As ReadOnlyCollection(Of String)
        Private ReadOnly _requiredNames As ReadOnlyCollection(Of String)
        Private ReadOnly _rows As ReadOnlyCollection(Of SpreadsheetModelSourceRow)
        Private ReadOnly _categories As ReadOnlyCollection(Of SpreadsheetModelCategoricalColumn)

        Public Sub New(responseName As String, response As Double(), rawPredictors As Double(,),
                       design As Double(,), predictorNames As IEnumerable(Of String),
                       rawPredictorNames As IEnumerable(Of String), requiredPredictorNames As IEnumerable(Of String),
                       rows As IEnumerable(Of SpreadsheetModelSourceRow),
                       categoricalColumns As IEnumerable(Of SpreadsheetModelCategoricalColumn),
                       originalFormula As String, normalizedFormula As String, includesIntercept As Boolean,
                       matrixRank As Integer, sourceWorkbookId As String, sourceWorksheetId As String,
                       selectionRevision As String)
            If response Is Nothing OrElse rawPredictors Is Nothing OrElse design Is Nothing Then Throw New ArgumentNullException(NameOf(response))
            If predictorNames Is Nothing OrElse rawPredictorNames Is Nothing OrElse requiredPredictorNames Is Nothing OrElse rows Is Nothing OrElse categoricalColumns Is Nothing Then
                Throw New ArgumentNullException(NameOf(predictorNames))
            End If
            Dim items As New List(Of SpreadsheetModelSourceRow)(rows)
            Dim columnNames As New List(Of String)(predictorNames)
            Dim rawNames As New List(Of String)(rawPredictorNames)
            If items.Count <> response.Length OrElse rawPredictors.GetLength(0) <> response.Length OrElse
               design.GetLength(0) <> response.Length OrElse columnNames.Count <> design.GetLength(1) OrElse
               rawNames.Count <> rawPredictors.GetLength(1) Then
                Throw New ArgumentException("Model arrays and schema have inconsistent dimensions.")
            End If
            _response = DirectCast(response.Clone(), Double())
            _rawPredictors = DirectCast(rawPredictors.Clone(), Double(,))
            _design = DirectCast(design.Clone(), Double(,))
            _names = columnNames.AsReadOnly()
            _rawNames = rawNames.AsReadOnly()
            _requiredNames = New List(Of String)(requiredPredictorNames).AsReadOnly()
            _rows = items.AsReadOnly()
            _categories = New List(Of SpreadsheetModelCategoricalColumn)(categoricalColumns).AsReadOnly()
            Me.ResponseName = If(responseName, String.Empty)
            Me.OriginalFormula = If(originalFormula, String.Empty)
            Me.NormalizedFormula = If(normalizedFormula, String.Empty)
            Me.IncludesIntercept = includesIntercept
            Me.MatrixRank = matrixRank
            Me.WorkbookId = If(sourceWorkbookId, String.Empty)
            Me.WorksheetId = If(sourceWorksheetId, String.Empty)
            Me.SelectionRevision = If(selectionRevision, String.Empty)
        End Sub
        Public ReadOnly Property ResponseName As String
        Public ReadOnly Property OriginalFormula As String
        Public ReadOnly Property NormalizedFormula As String
        Public ReadOnly Property IncludesIntercept As Boolean
        Public ReadOnly Property MatrixRank As Integer
        Public ReadOnly Property WorkbookId As String
        Public ReadOnly Property WorksheetId As String
        Public ReadOnly Property SelectionRevision As String
        Public ReadOnly Property ObservationCount As Integer
            Get
                Return _response.Length
            End Get
        End Property
        Public ReadOnly Property DesignColumnCount As Integer
            Get
                Return _design.GetLength(1)
            End Get
        End Property
        Public ReadOnly Property Response As Double()
            Get
                Return DirectCast(_response.Clone(), Double())
            End Get
        End Property
        Public ReadOnly Property RawPredictorMatrix As Double(,)
            Get
                Return DirectCast(_rawPredictors.Clone(), Double(,))
            End Get
        End Property
        Public ReadOnly Property DesignMatrix As Double(,)
            Get
                Return DirectCast(_design.Clone(), Double(,))
            End Get
        End Property
        Public ReadOnly Property PredictorNames As IReadOnlyList(Of String)
            Get
                Return _names
            End Get
        End Property
        Public ReadOnly Property RawPredictorNames As IReadOnlyList(Of String)
            Get
                Return _rawNames
            End Get
        End Property
        Public ReadOnly Property RequiredPredictorNames As IReadOnlyList(Of String)
            Get
                Return _requiredNames
            End Get
        End Property
        Public ReadOnly Property SourceRows As IReadOnlyList(Of SpreadsheetModelSourceRow)
            Get
                Return _rows
            End Get
        End Property
        Public ReadOnly Property CategoricalColumns As IReadOnlyList(Of SpreadsheetModelCategoricalColumn)
            Get
                Return _categories
            End Get
        End Property
    End Class
End Namespace
