Option Explicit On
Option Strict On
Option Infer On
Imports System
Imports System.Linq
Imports System.Collections.Generic
Imports BESHStatNG.regression


''' <summary>Ordered calibration bins. Historical admission rejects NaN only; infinities are retained.</summary>
Public NotInheritable Class CalibrationSeriesData
    Private ReadOnly _x As Double()
    Private ReadOnly _y As Double()
    Private ReadOnly _ordinals As Integer()
    Public Sub New(rows As IList(Of CalibrationBinSummary))
        Dim xs As New List(Of Double)()
        Dim ys As New List(Of Double)()
        Dim ordinals As New List(Of Integer)()
        If rows IsNot Nothing Then
            Dim inputOrdinal As Integer = 0
            For Each row As CalibrationBinSummary In rows
                If Not Double.IsNaN(row.MeanPredicted) AndAlso Not Double.IsNaN(row.ObservedRate) Then
                    xs.Add(row.MeanPredicted)
                    ys.Add(row.ObservedRate)
                    ordinals.Add(inputOrdinal)
                End If
                inputOrdinal += 1
            Next
        End If
        _x = xs.ToArray()
        _y = ys.ToArray()
        _ordinals = ordinals.ToArray()
    End Sub
    Public ReadOnly Property XValues As Double()
        Get
            Return DirectCast(_x.Clone(), Double())
        End Get
    End Property
    Public ReadOnly Property YValues As Double()
        Get
            Return DirectCast(_y.Clone(), Double())
        End Get
    End Property
    ''' <summary>Zero-based input-bin ordinals, without worksheet-row provenance.</summary>
    Public ReadOnly Property SourceOrdinals As Integer()
        Get
            Return DirectCast(_ordinals.Clone(), Integer())
        End Get
    End Property
    Public ReadOnly Property Count As Integer
        Get
            Return _x.Length
        End Get
    End Property
    Public ReadOnly Property Reference As ChartReferenceSeries
        Get
            Return ChartReferenceSeries.Identity(0.0R, 1.0R)
        End Get
    End Property
    Public ReadOnly Property Axis As ChartAxisScaleResult
        Get
            Return New ChartAxisScaleResult(0.0R, 1.0R, 0.2R)
        End Get
    End Property
End Class
