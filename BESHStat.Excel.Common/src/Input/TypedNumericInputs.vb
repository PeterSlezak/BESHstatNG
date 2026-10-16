Option Explicit On
Option Strict On

Imports System
Imports System.Collections.Generic
Imports System.Collections.ObjectModel

Namespace ExcelCommon

    ''' <summary>Immutable numeric observation and its source position. Zero absolute coordinates mean unknown.</summary>
    Public NotInheritable Class SpreadsheetNumericObservation
        Public Sub New(value As Double, selectedRow As Integer, selectedColumn As Integer,
                       worksheetRow As Integer, worksheetColumn As Integer)
            If Double.IsNaN(value) OrElse Double.IsInfinity(value) Then Throw New ArgumentOutOfRangeException(NameOf(value))
            If selectedRow < 1 OrElse selectedColumn < 1 OrElse worksheetRow < 0 OrElse worksheetColumn < 0 Then
                Throw New ArgumentOutOfRangeException(NameOf(selectedRow), "Source coordinates are invalid.")
            End If
            Me.Value = value
            Me.SelectedRow = selectedRow
            Me.SelectedColumn = selectedColumn
            Me.WorksheetRow = worksheetRow
            Me.WorksheetColumn = worksheetColumn
        End Sub

        Public ReadOnly Property Value As Double
        Public ReadOnly Property SelectedRow As Integer
        Public ReadOnly Property SelectedColumn As Integer
        Public ReadOnly Property WorksheetRow As Integer
        Public ReadOnly Property WorksheetColumn As Integer
    End Class

    ''' <summary>Single numeric input, with its original worksheet provenance.</summary>
    Public NotInheritable Class SpreadsheetScalarNumericInput
        Inherits SpreadsheetAnalysisInput

        Public Sub New(observation As SpreadsheetNumericObservation)
            If observation Is Nothing Then Throw New ArgumentNullException(NameOf(observation))
            Me.Observation = observation
        End Sub

        Public ReadOnly Property Observation As SpreadsheetNumericObservation
        Public ReadOnly Property Value As Double
            Get
                Return Observation.Value
            End Get
        End Property
    End Class

    ''' <summary>Numeric series with stable observation order and per-cell source coordinates.</summary>
    Public NotInheritable Class SpreadsheetVectorNumericInput
        Inherits SpreadsheetAnalysisInput

        Private ReadOnly _observations As ReadOnlyCollection(Of SpreadsheetNumericObservation)

        Public Sub New(name As String, observations As IEnumerable(Of SpreadsheetNumericObservation))
            If observations Is Nothing Then Throw New ArgumentNullException(NameOf(observations))
            Dim copied As New List(Of SpreadsheetNumericObservation)()
            For Each observation As SpreadsheetNumericObservation In observations
                If observation Is Nothing Then Throw New ArgumentException("Null observations are unsupported.", NameOf(observations))
                copied.Add(observation)
            Next
            Me.Name = If(name, String.Empty)
            _observations = copied.AsReadOnly()
        End Sub

        Public ReadOnly Property Name As String
        Public ReadOnly Property Observations As IReadOnlyList(Of SpreadsheetNumericObservation)
            Get
                Return _observations
            End Get
        End Property
        Public ReadOnly Property Count As Integer
            Get
                Return _observations.Count
            End Get
        End Property
        Public ReadOnly Property Values As Double()
            Get
                If _observations.Count = 0 Then Return Array.Empty(Of Double)()
                Dim result(_observations.Count - 1) As Double
                For i As Integer = 0 To _observations.Count - 1
                    result(i) = _observations(i).Value
                Next
                Return result
            End Get
        End Property
    End Class

    Public NotInheritable Class SpreadsheetNumericPair
        Public Sub New(first As SpreadsheetNumericObservation, second As SpreadsheetNumericObservation)
            If first Is Nothing Then Throw New ArgumentNullException(NameOf(first))
            If second Is Nothing Then Throw New ArgumentNullException(NameOf(second))
            Me.First = first
            Me.Second = second
        End Sub

        Public ReadOnly Property First As SpreadsheetNumericObservation
        Public ReadOnly Property Second As SpreadsheetNumericObservation
    End Class

    ''' <summary>Aligned complete numeric pairs. Incomplete source pairs are never independently shifted.</summary>
    Public NotInheritable Class SpreadsheetPairedNumericInput
        Inherits SpreadsheetAnalysisInput

        Private ReadOnly _pairs As ReadOnlyCollection(Of SpreadsheetNumericPair)

        Public Sub New(firstName As String, secondName As String, pairs As IEnumerable(Of SpreadsheetNumericPair))
            If pairs Is Nothing Then Throw New ArgumentNullException(NameOf(pairs))
            Dim copied As New List(Of SpreadsheetNumericPair)()
            For Each item As SpreadsheetNumericPair In pairs
                If item Is Nothing Then Throw New ArgumentException("Null pairs are unsupported.", NameOf(pairs))
                copied.Add(item)
            Next
            Me.FirstName = If(firstName, String.Empty)
            Me.SecondName = If(secondName, String.Empty)
            _pairs = copied.AsReadOnly()
        End Sub

        Public ReadOnly Property FirstName As String
        Public ReadOnly Property SecondName As String
        Public ReadOnly Property Pairs As IReadOnlyList(Of SpreadsheetNumericPair)
            Get
                Return _pairs
            End Get
        End Property
        Public ReadOnly Property Count As Integer
            Get
                Return _pairs.Count
            End Get
        End Property
        Public ReadOnly Property FirstValues As Double()
            Get
                If _pairs.Count = 0 Then Return Array.Empty(Of Double)()
                Dim values(_pairs.Count - 1) As Double
                For i As Integer = 0 To _pairs.Count - 1
                    values(i) = _pairs(i).First.Value
                Next
                Return values
            End Get
        End Property
        Public ReadOnly Property SecondValues As Double()
            Get
                If _pairs.Count = 0 Then Return Array.Empty(Of Double)()
                Dim values(_pairs.Count - 1) As Double
                For i As Integer = 0 To _pairs.Count - 1
                    values(i) = _pairs(i).Second.Value
                Next
                Return values
            End Get
        End Property
    End Class

    ''' <summary>Common diagnostics-first parse result for numeric input families.</summary>
    Public NotInheritable Class SpreadsheetTypedParseResult(Of T As SpreadsheetAnalysisInput)
        Public Sub New(data As T, validation As SpreadsheetValidationReport)
            If validation Is Nothing Then Throw New ArgumentNullException(NameOf(validation))
            If validation.HasErrors AndAlso data IsNot Nothing Then
                Throw New ArgumentException("Invalid parses cannot carry typed data.", NameOf(data))
            End If
            Me.Data = data
            Me.Validation = validation
        End Sub
        Public ReadOnly Property Data As T
        Public ReadOnly Property Validation As SpreadsheetValidationReport
        Public ReadOnly Property Success As Boolean
            Get
                Return Data IsNot Nothing AndAlso Validation.IsValid
            End Get
        End Property
    End Class

    Public Enum SpreadsheetVectorOrientation
        Auto = 0
        Column = 1
        Row = 2
    End Enum

    Public Enum SpreadsheetPairOrientation
        TwoColumns = 0
        TwoRows = 1
    End Enum

    Public Enum SpreadsheetIncompleteObservationPolicy
        Reject = 0
        Skip = 1
    End Enum

    Public NotInheritable Class SpreadsheetVectorParseOptions
        Public Property Orientation As SpreadsheetVectorOrientation = SpreadsheetVectorOrientation.Auto
        Public Property Header As SpreadsheetHeaderMode = SpreadsheetHeaderMode.None
        Public Property ColumnIndex As Integer = 0
        Public Property RowIndex As Integer = 0
        Public Property MinimumObservations As Integer = 1
        Public Property Missing As SpreadsheetIncompleteObservationPolicy = SpreadsheetIncompleteObservationPolicy.Skip
        Public Property WarnOnSkippedObservations As Boolean = False
        Public Property CellPolicy As SpreadsheetCellPolicyOptions = New SpreadsheetCellPolicyOptions()
    End Class

    Public NotInheritable Class SpreadsheetPairedParseOptions
        Public Property Orientation As SpreadsheetPairOrientation = SpreadsheetPairOrientation.TwoColumns
        Public Property Header As SpreadsheetHeaderMode = SpreadsheetHeaderMode.None
        Public Property FirstIndex As Integer = 0
        Public Property SecondIndex As Integer = 1
        Public Property MinimumPairs As Integer = 1
        Public Property Missing As SpreadsheetIncompleteObservationPolicy = SpreadsheetIncompleteObservationPolicy.Reject
        Public Property WarnOnSkippedPairs As Boolean = True
        Public Property CellPolicy As SpreadsheetCellPolicyOptions = New SpreadsheetCellPolicyOptions()
    End Class
End Namespace
