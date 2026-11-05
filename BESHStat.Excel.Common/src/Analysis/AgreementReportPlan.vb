Option Explicit On
Option Strict On
Option Infer On
Imports System
Imports System.Collections.Generic
Imports System.Linq

Namespace ExcelCommon
    Public Enum AgreementMethod
        Deming
        LinConcordance
        Kappa
        BlandAltman
        Icc
    End Enum
    ''' <summary>Ordered existing Core tables and host plot/area intent. Core table objects remain
    ''' caller-owned; fresh list snapshots prevent stale section-list reuse. No table math or drawing.</summary>
    Public NotInheritable Class AgreementReportPlan
        Private ReadOnly tables As ResultTable()
        Public Sub New(method As AgreementMethod, coreTables As IEnumerable(Of ResultTable), axis As AgreementInputAxis)
            Me.Method = method : Me.Axis = axis : tables = coreTables.ToArray()
            PlotRequested = method = AgreementMethod.Deming OrElse method = AgreementMethod.LinConcordance OrElse method = AgreementMethod.BlandAltman
        End Sub
        Public ReadOnly Property Method As AgreementMethod
        Public ReadOnly Property Axis As AgreementInputAxis
        Public ReadOnly Property PlotRequested As Boolean
        Public Function TablesForWriter() As List(Of ResultTable)
            Return New List(Of ResultTable)(tables)
        End Function
        Public Function NativeRowSpan(tableRows As Integer) As Integer
            If Method = AgreementMethod.BlandAltman Then Return tableRows + tables.Length + tables.Length - 1
            Return tableRows + tables.Length - 1
        End Function
        Public Shared Sub AppendDecisionTables(destination As List(Of ResultTable), bias As ResultTable, limits As ResultTable)
            destination.Add(bias) : destination.Add(limits)
        End Sub
    End Class
    Public Enum IccEstimatorChoice
        None = -1
        OneOne = 0
        OneAverage = 1
        TwoOne = 2
        TwoAverage = 3
        ThreeOne = 4
        ThreeAverage = 5
    End Enum
    Public NotInheritable Class IccRequestIntent
        Public Sub New(oneOne As Boolean, oneAverage As Boolean, twoOne As Boolean, twoAverage As Boolean,
                       threeOne As Boolean, threeAverage As Boolean, alpha As Double, repeatability As Boolean)
            Choice = If(oneOne, IccEstimatorChoice.OneOne, If(oneAverage, IccEstimatorChoice.OneAverage,
                If(twoOne, IccEstimatorChoice.TwoOne, If(twoAverage, IccEstimatorChoice.TwoAverage,
                If(threeOne, IccEstimatorChoice.ThreeOne, If(threeAverage, IccEstimatorChoice.ThreeAverage, IccEstimatorChoice.None))))))
            Me.Alpha = alpha : Me.Repeatability = repeatability
        End Sub
        Public ReadOnly Property Choice As IccEstimatorChoice
        Public ReadOnly Property Alpha As Double
        Public ReadOnly Property Repeatability As Boolean
        Public Function Label(measurements As Integer) As String
            Select Case Choice
                Case IccEstimatorChoice.OneOne : Return "ICC(1,1)"
                Case IccEstimatorChoice.OneAverage : Return "ICC(1,k)"
                Case IccEstimatorChoice.TwoOne : Return "ICC(2,1)"
                Case IccEstimatorChoice.TwoAverage : Return $"ICC(2,{measurements})"
                Case IccEstimatorChoice.ThreeOne : Return "ICC(3,1)"
                Case IccEstimatorChoice.ThreeAverage : Return $"ICC(3,{measurements})"
                Case Else : Return String.Empty
            End Select
        End Function
    End Class
End Namespace
