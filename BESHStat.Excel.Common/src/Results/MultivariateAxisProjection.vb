Option Explicit On
Option Strict On
Imports System
Imports System.Collections.Generic
Imports System.Linq

Namespace ExcelCommon
    Public Enum MultivariateSemanticAxis
        Observations
        ScoreObservations
        InputVariables
        LoadingVariables
        RetainedComponents
        Categories
        VariableCategoryBlocks
        CorrespondenceFactors
        ContingencyRows
        ContingencyColumns
        EigenComponents
        PresentationHeaders
        ObservationIdentity
        ObservationGrouping
    End Enum
    Public Enum MultivariateMappingState
        Known
        Unavailable
        Ambiguous
    End Enum
    ''' <summary>Array positions are zero-based; physical RowId is an optional, distinct
    ''' host-supplied identity. A category/factor index is never a worksheet row.</summary>
    Public NotInheritable Class MultivariateAxisItem
        Public Sub New(resultIndex As Integer, name As String, Optional sourcePosition As Nullable(Of Integer) = Nothing,
                       Optional sourceRowId As Nullable(Of Integer) = Nothing, Optional sourceKey As String = Nothing,
                       Optional displayLabel As String = Nothing, Optional state As MultivariateMappingState = MultivariateMappingState.Known)
            Me.ResultIndex = resultIndex : Me.Name = name : Me.SourcePosition = sourcePosition
            Me.SourceRowId = sourceRowId : Me.SourceKey = sourceKey : Me.DisplayLabel = displayLabel : Me.State = state
        End Sub
        Public ReadOnly Property ResultIndex As Integer
        Public ReadOnly Property Name As String
        Public ReadOnly Property SourcePosition As Nullable(Of Integer)
        Public ReadOnly Property SourceRowId As Nullable(Of Integer)
        Public ReadOnly Property SourceKey As String
        Public ReadOnly Property DisplayLabel As String
        Public ReadOnly Property State As MultivariateMappingState
    End Class
    Public NotInheritable Class MultivariateAxisMap
        Public Sub New(kind As MultivariateSemanticAxis, items As IEnumerable(Of MultivariateAxisItem))
            Me.Kind = kind : Me.Items = New List(Of MultivariateAxisItem)(items).AsReadOnly()
        End Sub
        Public ReadOnly Property Kind As MultivariateSemanticAxis
        Public ReadOnly Property Items As IReadOnlyList(Of MultivariateAxisItem)
    End Class
    Public NotInheritable Class MultivariateArrayShape
        Public Sub New(rows As Integer, columns As Integer, Optional available As Boolean = True)
            Me.Rows = rows : Me.Columns = columns : Me.Available = available
        End Sub
        Public ReadOnly Property Rows As Integer
        Public ReadOnly Property Columns As Integer
        Public ReadOnly Property Available As Boolean
        Public Shared Function Matrix(value As Array) As MultivariateArrayShape
            Return If(value Is Nothing, New MultivariateArrayShape(0, 0, False), New MultivariateArrayShape(value.GetLength(0), value.GetLength(1)))
        End Function
    End Class
    Public Enum MultivariateSectionRole
        IdentifierHeader
        Identifiers
        GroupHeader
        Groups
        OriginalHeader
        OriginalData
        StandardizedHeader
        StandardizedData
        ReducedHeader
        ReducedScores
        DesignCaption
        DesignVariableHeader
        DesignCategoryHeader
        IndicatorData
        ContingencyBody
    End Enum
    ''' <summary>One existing Data-document block; positions are one-based relative
    ''' output offsets, not source addresses. Payload matrices are not retained.</summary>
    Public NotInheritable Class MultivariateResultSection
        Private ReadOnly _header As String()
        Friend Sub New(role As MultivariateSectionRole, row As Integer, column As Integer,
                       shape As MultivariateArrayShape, rowAxis As MultivariateSemanticAxis,
                       columnAxis As MultivariateSemanticAxis, Optional header() As String = Nothing)
            Me.Role = role : Me.StartRow = row : Me.StartColumn = column : Me.Shape = shape
            Me.RowAxis = rowAxis : Me.ColumnAxis = columnAxis
            _header = If(header Is Nothing, Nothing, DirectCast(header.Clone(), String()))
        End Sub
        Public ReadOnly Property Role As MultivariateSectionRole
        Public ReadOnly Property StartRow As Integer
        Public ReadOnly Property StartColumn As Integer
        Public ReadOnly Property Shape As MultivariateArrayShape
        Public ReadOnly Property RowAxis As MultivariateSemanticAxis
        Public ReadOnly Property ColumnAxis As MultivariateSemanticAxis
        Public ReadOnly Property HeaderDepth As Integer
            Get
                Return If(_header Is Nothing, 0, 1)
            End Get
        End Property
        Public ReadOnly Property Header As String()
            Get
                Return If(_header Is Nothing, Nothing, DirectCast(_header.Clone(), String()))
            End Get
        End Property
    End Class
    ''' <summary>Existing Core point-set shapes and identities. No coordinates or
    ''' biplot/dendrogram geometry are recalculated, cached or normalized here.</summary>
    Public NotInheritable Class MultivariatePlotDataProjection
        Friend Sub New(request As MultivariatePlotIntent, rowAxis As MultivariateSemanticAxis,
                       columnAxis As MultivariateSemanticAxis, rowShape As MultivariateArrayShape,
                       columnShape As MultivariateArrayShape, displayRows As Boolean)
            Me.Request = request : Me.RowAxis = rowAxis : Me.ColumnAxis = columnAxis
            Me.RowPointShape = rowShape : Me.ColumnPointShape = columnShape : Me.DisplayRows = displayRows
        End Sub
        Public ReadOnly Property Request As MultivariatePlotIntent
        Public ReadOnly Property RowAxis As MultivariateSemanticAxis
        Public ReadOnly Property ColumnAxis As MultivariateSemanticAxis
        Public ReadOnly Property RowPointShape As MultivariateArrayShape
        Public ReadOnly Property ColumnPointShape As MultivariateArrayShape
        Public ReadOnly Property DisplayRows As Boolean
    End Class
    Public NotInheritable Class MultivariateComponentProjection
        Friend Sub New(input As MultivariateInputIntent, axes As IEnumerable(Of MultivariateAxisMap),
                       sections As IEnumerable(Of MultivariateResultSection), plots As IEnumerable(Of MultivariatePlotDataProjection),
                       diagnostics As IEnumerable(Of RegressionDiagnostic), Optional pcaOptions As MultivariatePcaOptions = Nothing,
                       Optional mcaTransform As MultivariateMcaTransformIntent = Nothing,
                       Optional contingency As MultivariateContingencyInput = Nothing,
                       Optional burtShape As MultivariateArrayShape = Nothing)
            Me.Input = input : Me.Axes = New List(Of MultivariateAxisMap)(axes).AsReadOnly()
            Me.Sections = New List(Of MultivariateResultSection)(sections).AsReadOnly()
            Me.PlotData = New List(Of MultivariatePlotDataProjection)(plots).AsReadOnly() : Me.PcaSelection = pcaOptions
            Me.Validation = New RegressionValidation(diagnostics.Take(32), 0, 0, {})
            Me.McaTransform = mcaTransform : Me.Contingency = contingency : Me.BurtShape = burtShape
        End Sub
        Public ReadOnly Property Input As MultivariateInputIntent
        Public ReadOnly Property Axes As IReadOnlyList(Of MultivariateAxisMap)
        Public ReadOnly Property Sections As IReadOnlyList(Of MultivariateResultSection)
        Public ReadOnly Property PlotData As IReadOnlyList(Of MultivariatePlotDataProjection)
        Public ReadOnly Property PcaSelection As MultivariatePcaOptions
        Public ReadOnly Property Validation As RegressionValidation
        Public ReadOnly Property McaTransform As MultivariateMcaTransformIntent
        Public ReadOnly Property Contingency As MultivariateContingencyInput
        Public ReadOnly Property BurtShape As MultivariateArrayShape
        Public Function Axis(kind As MultivariateSemanticAxis) As MultivariateAxisMap
            Return Axes.FirstOrDefault(Function(a) a.Kind = kind)
        End Function
        Public Function ToReport(tables As IEnumerable(Of ResultTable)) As MultivariateReport
            Return MultivariateReportPolicy.Project(Input, tables)
        End Function
    End Class
End Namespace
