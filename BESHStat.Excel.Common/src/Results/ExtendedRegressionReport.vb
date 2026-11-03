Option Explicit On
Option Strict On
Imports System.Collections.Generic
Imports System.Linq

Namespace ExcelCommon
    ''' <summary>Independent, immutable Core-response/document projection. Class axes are supplied
    ''' by Core, never sorted for display. Dispersion is an actual Core estimate, not CI alpha.</summary>
    Public NotInheritable Class ExtendedRegressionReport
        Friend Sub New(intent As ExtendedRegressionIntent, report As RegressionReportPlan, dispersion As Nullable(Of Double))
            Me.Intent = intent : Me.Report = report : Me.EstimatedDispersion = dispersion
        End Sub
        Public ReadOnly Property Intent As ExtendedRegressionIntent
        Public ReadOnly Property Report As RegressionReportPlan
        Public ReadOnly Property EstimatedDispersion As Nullable(Of Double)
        Public ReadOnly Property Rows As RegressionRowOutput
            Get
                Return Report.Rows
            End Get
        End Property
        Public ReadOnly Property Document As AnalysisResultDocument
            Get
                Return Report.Document
            End Get
        End Property
        Public ReadOnly Property Convergence As RegressionConvergence
            Get
                Return Report.Convergence
            End Get
        End Property
        Public Function LegacyTables() As List(Of ResultTable)
            Return Document.Tables.Select(Function(t) LinearBinomialReport.Rehydrate(t.Model)).ToList()
        End Function
    End Class
End Namespace
