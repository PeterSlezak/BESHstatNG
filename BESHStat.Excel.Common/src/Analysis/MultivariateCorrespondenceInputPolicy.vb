Option Explicit On
Option Strict On
Imports System
Imports System.Collections.Generic
Imports System.Linq

Namespace ExcelCommon
    ''' <summary>Count-axis intent only. Excel Object-to-CInt coercion remains in
    ''' Windows. The unlabelled identity mapping is a dormant candidate, not a route
    ''' around C17-CA-NOLABEL-INDEX-01.</summary>
    Public NotInheritable Class MultivariateContingencyInput
        Private ReadOnly _names, _labels As String()
        Private ReadOnly _rows As Integer()
        Friend Sub New(rowCount As Integer, sourceColumns As Integer, labelled As Boolean,
                       names() As String, sourceRows() As Integer, labels() As String)
            Me.RowCount = rowCount : Me.SourceColumnCount = sourceColumns : Me.HasLabelColumn = labelled
            _names = If(names Is Nothing, Nothing, DirectCast(names.Clone(), String()))
            _rows = If(sourceRows Is Nothing, Nothing, DirectCast(sourceRows.Clone(), Integer()))
            _labels = If(labels Is Nothing, Nothing, DirectCast(labels.Clone(), String()))
            Dim issues As New List(Of RegressionDiagnostic)
            If rowCount < 1 OrElse CountColumnCount < 1 Then issues.Add(New RegressionDiagnostic("multivariate.ca.empty", "counts", "Count axes are empty or invalid.", blocking:=False))
            If _rows IsNot Nothing AndAlso _rows.Length <> rowCount Then issues.Add(New RegressionDiagnostic("multivariate.ca.rows.shape", "rows", "Supplied source RowIds do not match the count-row axis.", blocking:=False))
            If _names IsNot Nothing AndAlso _names.Length <> sourceColumns Then issues.Add(New RegressionDiagnostic("multivariate.ca.names.shape", "columns", "Source names do not match the acquired column axis.", blocking:=False))
            If labelled AndAlso _labels IsNot Nothing AndAlso _labels.Length <> rowCount Then issues.Add(New RegressionDiagnostic("multivariate.ca.labels.shape", "labels", "Supplied row labels do not match count rows.", blocking:=False))
            If Not labelled Then issues.Add(New RegressionDiagnostic("C17-CA-NOLABEL-INDEX-01", "activation", "Correct unlabelled column identity mapping is dormant; the existing Windows failure remains active.", blocking:=False))
            Validation = New RegressionValidation(issues, 0, 0, {})
        End Sub
        Public ReadOnly Property RowCount As Integer
        Public ReadOnly Property SourceColumnCount As Integer
        Public ReadOnly Property HasLabelColumn As Boolean
        Public ReadOnly Property CandidateOnly As Boolean
            Get
                Return Not HasLabelColumn
            End Get
        End Property
        Public ReadOnly Property CountColumnCount As Integer
            Get
                Return SourceColumnCount - If(HasLabelColumn, 1, 0)
            End Get
        End Property
        Public ReadOnly Property FirstCountSourceColumn As Integer
            Get
                Return If(HasLabelColumn, 1, 0)
            End Get
        End Property
        Public Function ResultColumn(sourceColumn As Integer) As Integer
            Return sourceColumn - FirstCountSourceColumn
        End Function
        Public ReadOnly Property SourceNames As String()
            Get
                Return If(_names Is Nothing, Nothing, DirectCast(_names.Clone(), String()))
            End Get
        End Property
        Public ReadOnly Property SourceRowIds As Integer()
            Get
                Return If(_rows Is Nothing, Nothing, DirectCast(_rows.Clone(), Integer()))
            End Get
        End Property
        Public ReadOnly Property RowLabels As String()
            Get
                Return If(_labels Is Nothing, Nothing, DirectCast(_labels.Clone(), String()))
            End Get
        End Property
        Public ReadOnly Property Validation As RegressionValidation
    End Class
    Public NotInheritable Class MultivariateMcaTransformIntent
        Public Sub New(input As MultivariateInputIntent, firstAcquiredRowRemoved As Boolean)
            Me.Input = input : Me.FirstAcquiredRowRemoved = firstAcquiredRowRemoved
        End Sub
        Public ReadOnly Property Input As MultivariateInputIntent
        Public ReadOnly Property FirstAcquiredRowRemoved As Boolean
    End Class
    Public NotInheritable Class MultivariateCorrespondenceInputPolicy
        Private Sub New()
        End Sub
        Public Shared Function Labelled(rowCount As Integer, sourceColumns As Integer, names() As String,
                                        rows() As Integer, labels() As String) As MultivariateContingencyInput
            Return New MultivariateContingencyInput(rowCount, sourceColumns, True, names, rows, labels)
        End Function
        Public Shared Function DormantUnlabelled(rowCount As Integer, sourceColumns As Integer, names() As String,
                                               Optional rows() As Integer = Nothing) As MultivariateContingencyInput
            Return New MultivariateContingencyInput(rowCount, sourceColumns, False, names, rows, Nothing)
        End Function
    End Class
End Namespace
