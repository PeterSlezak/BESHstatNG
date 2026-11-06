Option Explicit On
Option Strict On

Imports System
Imports System.Collections.Generic

''' <summary>Legacy sorted coordinates plus a reproducible, zero-based permutation.
''' Uses SortedCopy unchanged. For each sorted IEEE value, assign the earliest unused
''' input with identical bits. Identical-bit ties are stable by input ordinal.
''' Signed zeros and distinct NaN payloads retain the legacy sort's coordinate slots;
''' no historical Array.Sort stability is asserted. No physical row is inferred.</summary>
Public NotInheritable Class NormalPlotObservationOrder
    Private ReadOnly values() As Double, sortedSource() As Integer, sourceSorted() As Integer

    Public Sub New(input() As Double)
        values = NormalPlotDataCore.SortedCopy(input)
        ReDim sortedSource(values.Length - 1), sourceSorted(values.Length - 1)
        Dim positions As New Dictionary(Of Long, Queue(Of Integer))()
        For i As Integer = 0 To input.Length - 1
            Dim bits = BitConverter.DoubleToInt64Bits(input(i))
            Dim queue As Queue(Of Integer) = Nothing
            If Not positions.TryGetValue(bits, queue) Then
                queue = New Queue(Of Integer)()
                positions.Add(bits, queue)
            End If
            queue.Enqueue(i)
        Next
        For i As Integer = 0 To values.Length - 1
            Dim source = positions(BitConverter.DoubleToInt64Bits(values(i))).Dequeue()
            sortedSource(i) = source : sourceSorted(source) = i
        Next
    End Sub

    Public ReadOnly Property Data As Double()
        Get
            Return DirectCast(values.Clone(), Double())
        End Get
    End Property

    Public ReadOnly Property SortedToSource As Integer()
        Get
            Return DirectCast(sortedSource.Clone(), Integer())
        End Get
    End Property

    Public ReadOnly Property SourceToSorted As Integer()
        Get
            Return DirectCast(sourceSorted.Clone(), Integer())
        End Get
    End Property
End Class

''' <summary>Ordinary normal Q-Q snapshot: observed X, expected normal Y;
''' reference matrix rows are endpoints, columns X/Y. Calculations remain in
''' NormalPlotDataCore. Project also captures legacy pre-score and mutable Windows
''' state without changing its sequencing, aliases, errors or numeric policies.</summary>
Public NotInheritable Class NormalPlotSeriesData
    Private ReadOnly order As NormalPlotObservationOrder
    Private ReadOnly yData() As Double, probabilitiesData() As Double, lineData(,) As Double

    Private Sub New(observationOrder As NormalPlotObservationOrder, probabilities() As Double, normalScores() As Double, scoreMethod As String, referenceLine(,) As Double, lineMethod As String)
        If observationOrder Is Nothing Then Throw New ArgumentNullException(NameOf(observationOrder))
        order = observationOrder
        probabilitiesData = Copy(probabilities) : yData = Copy(normalScores)
        lineData = If(referenceLine Is Nothing, Nothing, DirectCast(referenceLine.Clone(), Double(,)))
        Me.ScoreMethod = scoreMethod : Me.LineMethod = lineMethod
    End Sub

    Public Shared Function Compute(values() As Double, scoreMethod As String, lineMethod As String) As NormalPlotSeriesData
        Dim observationOrder As New NormalPlotObservationOrder(values)
        Dim scores = NormalPlotDataCore.Scores(observationOrder.Data, scoreMethod)
        Return Project(observationOrder, scores.Probabilities, scores.NormalScores, scoreMethod, NormalPlotDataCore.ReferenceLine(scores, lineMethod), lineMethod)
    End Function

    Public Shared Function Project(observationOrder As NormalPlotObservationOrder, probabilities() As Double, normalScores() As Double, scoreMethod As String, referenceLine(,) As Double, Optional lineMethod As String = Nothing) As NormalPlotSeriesData
        Return New NormalPlotSeriesData(observationOrder, probabilities, normalScores, scoreMethod, referenceLine, lineMethod)
    End Function

    Private Shared Function Copy(values() As Double) As Double()
        Return If(values Is Nothing, Nothing, DirectCast(values.Clone(), Double()))
    End Function

    Public ReadOnly Property ScoreMethod As String
    Public ReadOnly Property LineMethod As String

    Public ReadOnly Property X As Double()
        Get
            Return order.Data
        End Get
    End Property

    Public ReadOnly Property Y As Double()
        Get
            Return Copy(yData)
        End Get
    End Property

    Public ReadOnly Property Probabilities As Double()
        Get
            Return Copy(probabilitiesData)
        End Get
    End Property

    Public ReadOnly Property PointCount As Integer
        Get
            Return order.SortedToSource.Length
        End Get
    End Property

    Public ReadOnly Property SortedToSource As Integer()
        Get
            Return order.SortedToSource
        End Get
    End Property

    Public ReadOnly Property SourceToSorted As Integer()
        Get
            Return order.SourceToSorted
        End Get
    End Property

    Public ReadOnly Property ReferenceLine As Double(,)
        Get
            Return If(lineData Is Nothing, Nothing, DirectCast(lineData.Clone(), Double(,)))
        End Get
    End Property

    Private Function ReferenceColumn(column As Integer) As Double()
        If lineData Is Nothing Then Return Nothing
        Return DataManagement.ArrayUtilities.GetColumn(lineData, column)
    End Function

    Public ReadOnly Property ReferenceX As Double()
        Get
            Return ReferenceColumn(0)
        End Get
    End Property

    Public ReadOnly Property ReferenceY As Double()
        Get
            Return ReferenceColumn(1)
        End Get
    End Property
End Class
