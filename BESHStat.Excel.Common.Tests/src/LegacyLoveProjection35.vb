Option Explicit On
Option Strict On
Option Infer On
Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports BESHStatNG.CausalInference

Namespace Global.Legacy35
    ''' <summary>One frozen ordered axis for the source table and native chart. Chart missing values
    ''' are NaN; table missing values are Nothing. Ordinals are one-based plot positions, not source IDs.</summary>
    Public NotInheritable Class LegacyLoveProjection35
        Private ReadOnly rows As PsmLovePlotRow()
        Public Sub New(source As IList(Of PsmLovePlotRow))
            rows = If(source, New List(Of PsmLovePlotRow)()).
                Where(Function(r) r IsNot Nothing AndAlso Not String.IsNullOrWhiteSpace(r.VariableName)).
                OrderByDescending(Function(r) MaxFiniteAbs(r.SmdBefore, r.SmdAfterMatching, r.SmdAfterWeighting)).
                ThenBy(Function(r) r.VariableName, StringComparer.OrdinalIgnoreCase).
                Select(Function(r) New PsmLovePlotRow With {.VariableName = r.VariableName, .SmdBefore = r.SmdBefore,
                    .SmdAfterMatching = r.SmdAfterMatching, .SmdAfterWeighting = r.SmdAfterWeighting,
                    .Threshold = r.Threshold, .Flag = r.Flag}).ToArray()
            Dim value = rows.Select(Function(r) r.Threshold).FirstOrDefault(Function(v) Finite(v) AndAlso v > 0)
            Threshold = If(Not Finite(value) OrElse value <= 0, 0.1R, value)
            Dim maxValue = Double.NaN
            For Each v In Before.Concat(AfterMatching).Concat(AfterWeighting)
                If Finite(v) AndAlso (Not Finite(maxValue) OrElse v > maxValue) Then maxValue = v
            Next
            value = Math.Max(Threshold * 1.25R, maxValue * 1.15R)
            If Not Finite(value) OrElse value <= 0 Then value = 0.25R
            If value < 0.25R Then value = 0.25R
            MaximumX = value
        End Sub
        Public ReadOnly Property Count As Integer
            Get
                Return rows.Length
            End Get
        End Property
        Public ReadOnly Property Threshold As Double
        Public ReadOnly Property MaximumX As Double
        Public ReadOnly Property Labels As String()
            Get
                Return rows.Select(Function(r) r.VariableName).ToArray()
            End Get
        End Property
        Public ReadOnly Property YValues As Double()
            Get
                Return Enumerable.Range(1, Count).Select(Function(i) CDbl(i)).ToArray()
            End Get
        End Property
        Public ReadOnly Property Before As Double()
            Get
                Return rows.Select(Function(r) ChartValue(r.SmdBefore)).ToArray()
            End Get
        End Property
        Public ReadOnly Property AfterMatching As Double()
            Get
                Return rows.Select(Function(r) ChartValue(r.SmdAfterMatching)).ToArray()
            End Get
        End Property
        Public ReadOnly Property AfterWeighting As Double()
            Get
                Return rows.Select(Function(r) ChartValue(r.SmdAfterWeighting)).ToArray()
            End Get
        End Property
        Public Function BuildDataTable() As Object(,)
            If Count = 0 Then Return PsmResult.EmptyTable("No Love plot data available")
            Dim table(Count, 6) As Object
            Dim headings = {"Plot Row", "Variable", "|SMD| before", "|SMD| after matching", "|SMD| after weighting", "Threshold", "Flag"}
            For j = 0 To 6 : table(0, j) = headings(j) : Next
            For i = 0 To Count - 1
                Dim r = rows(i)
                table(i + 1, 0) = i + 1 : table(i + 1, 1) = r.VariableName
                table(i + 1, 2) = TableValue(r.SmdBefore)
                table(i + 1, 3) = TableValue(r.SmdAfterMatching)
                table(i + 1, 4) = TableValue(r.SmdAfterWeighting)
                table(i + 1, 5) = If(Finite(r.Threshold), r.Threshold, Nothing)
                table(i + 1, 6) = r.Flag
            Next
            Return table
        End Function
        Private Shared Function Finite(value As Double) As Boolean
            Return Not Double.IsNaN(value) AndAlso Not Double.IsInfinity(value)
        End Function
        Private Shared Function ChartValue(value As Double) As Double
            Return If(Finite(value), Math.Abs(value), Double.NaN)
        End Function
        Private Shared Function TableValue(value As Double) As Object
            If Not Finite(value) Then Return Nothing
            Return Math.Abs(value)
        End Function
        Private Shared Function MaxFiniteAbs(ParamArray values As Double()) As Double
            Dim maximum = Double.NaN
            For Each v In values
                If Finite(v) Then
                    Dim absolute = Math.Abs(v)
                    If Not Finite(maximum) OrElse absolute > maximum Then maximum = absolute
                End If
            Next
            Return maximum
        End Function
    End Class
End Namespace
