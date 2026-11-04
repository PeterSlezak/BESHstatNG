Option Explicit On
Option Strict On
Imports System
Imports System.Collections.Generic
Imports System.Linq

Namespace ExcelCommon
    Public Enum MultivariateModelAxis
        InputObservations
        ActiveObservations
        InputVariables
        Factors
        Eigenvalues
        Classes
        CanonicalFunctions
    End Enum
    ''' <summary>Actual Core shape and semantic axes, never a retained numerical matrix.</summary>
    Public NotInheritable Class MultivariateModelArray
        Friend Sub New(role As String, value As Array, rows As MultivariateModelAxis, columns As MultivariateModelAxis)
            Me.Role = role : Me.Shape = MultivariateArrayShape.Matrix(value)
            Me.RowAxis = rows : Me.ColumnAxis = columns
        End Sub
        Public ReadOnly Property Role As String
        Public ReadOnly Property Shape As MultivariateArrayShape
        Public ReadOnly Property RowAxis As MultivariateModelAxis
        Public ReadOnly Property ColumnAxis As MultivariateModelAxis
    End Class
    Public NotInheritable Class MultivariateModelSection
        Friend Sub New(table As AnalysisTableArtifact)
            Index = table.Index : Shape = New MultivariateArrayShape(table.RowCount, table.ColumnCount)
            Dim m = table.Model
            HeaderDepth = m.HeaderTopRows : LeftHeaderDepth = m.HeaderLeftColumns
            TitleDepth = m.TitleRows : FooterDepth = m.FooterRows
            Title = If(m.TitleRows > 0 AndAlso m.RowCount > 0 AndAlso m.ColumnCount > 0, TryCast(m.Values(0, 0), String), Nothing)
        End Sub
        Public ReadOnly Property Index As Integer
        Public ReadOnly Property Title As String
        Public ReadOnly Property Shape As MultivariateArrayShape
        Public ReadOnly Property HeaderDepth As Integer
        Public ReadOnly Property LeftHeaderDepth As Integer
        Public ReadOnly Property TitleDepth As Integer
        Public ReadOnly Property FooterDepth As Integer
    End Class
    ''' <summary>Ordered section traversal over the established report document, preserving
    ''' its cells/types/headers/footnotes/p-value metadata and rehydration implementation.</summary>
    Public NotInheritable Class MultivariateModelReport
        Friend Sub New(input As MultivariateInputIntent, tables As IEnumerable(Of ResultTable))
            Report = MultivariateReportPolicy.Project(input, tables)
            Sections = Report.Document.Tables.Select(Function(t) New MultivariateModelSection(t)).ToList().AsReadOnly()
        End Sub
        Public ReadOnly Property Report As MultivariateReport
        Public ReadOnly Property Sections As IReadOnlyList(Of MultivariateModelSection)
        Public Function LegacyTables() As List(Of ResultTable)
            Dim source = Report.LegacyTables()
            Return Sections.Select(Function(s) source(s.Index)).ToList()
        End Function
    End Class
    Public NotInheritable Class MultivariateModelPlot
        Friend Sub New(request As MultivariatePlotIntent, shape As MultivariateArrayShape)
            Me.Request = request : Me.Shape = shape
        End Sub
        Public ReadOnly Property Request As MultivariatePlotIntent
        Public ReadOnly Property Shape As MultivariateArrayShape
    End Class
End Namespace
