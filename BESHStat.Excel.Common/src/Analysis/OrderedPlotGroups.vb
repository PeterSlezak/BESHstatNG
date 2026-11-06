Option Explicit On
Option Strict On

Imports System
Imports System.Collections.Generic
Imports System.Collections.ObjectModel
Imports System.Linq

Namespace ExcelCommon

    ''' <summary>Identity for one plotted slot. Indices are zero-based; physical row is
    ''' nullable and comes only from an actual UnivariateObservationSource.</summary>
    Public NotInheritable Class OrderedPlotPoint

        Friend Sub New(groupOrdinal As Integer, observationIndex As Integer, chartIndex As Integer, source As UnivariateObservationSource)
            Me.GroupOrdinal = groupOrdinal : Me.ObservationIndex = observationIndex : Me.ChartIndex = chartIndex
            Me.Source = source
            If source IsNot Nothing AndAlso source.WorksheetRow > 0 Then Me.PhysicalRowId = source.WorksheetRow
        End Sub

        Public ReadOnly Property GroupOrdinal As Integer

        Public ReadOnly Property ObservationIndex As Integer

        Public ReadOnly Property ChartIndex As Integer

        Public ReadOnly Property PhysicalRowId As Integer?

        Public ReadOnly Property Source As UnivariateObservationSource
    End Class

    ''' <summary>Ordered presentation snapshot over existing grouped-input contracts.
    ''' Does not acquire, regroup, trim, sort, deduplicate, filter or format labels/data.
    ''' Numeric and text raw labels remain distinct. The host supplies display labels.
    ''' Only immutable scalar raw labels are accepted; no workbook objects are retained.</summary>
    Public NotInheritable Class OrderedPlotGroups

        Private ReadOnly names() As String, raw() As Object, groups()() As Double

        Private ReadOnly sources As Dictionary(Of Tuple(Of Integer, Integer), UnivariateObservationSource)

        Public Sub New(data As GroupedNumericData, Optional rawLabels As Object() = Nothing, Optional observationSources As IEnumerable(Of UnivariateObservationSource) = Nothing)
            If data Is Nothing Then Throw New ArgumentNullException(NameOf(data))
            If data.GroupNames.Length <> data.Groups.Length Then Throw New ArgumentException("Group and label counts must agree.", NameOf(data))
            If rawLabels IsNot Nothing AndAlso rawLabels.Length <> data.Groups.Length Then Throw New ArgumentException("Raw label count must agree.", NameOf(rawLabels))
            names = DirectCast(data.GroupNames.Clone(), String())
            raw = If(rawLabels Is Nothing, names.Cast(Of Object)().ToArray(), DirectCast(rawLabels.Clone(), Object()))
            For Each label As Object In raw
                If label IsNot Nothing AndAlso Not(TypeOf label Is String OrElse TypeOf label Is Boolean OrElse TypeOf label Is Char OrElse TypeOf label Is Byte OrElse TypeOf label Is SByte OrElse TypeOf label Is Short OrElse TypeOf label Is UShort OrElse TypeOf label Is Integer OrElse TypeOf label Is UInteger OrElse TypeOf label Is Long OrElse TypeOf label Is ULong OrElse TypeOf label Is Single OrElse TypeOf label Is Double OrElse TypeOf label Is Decimal) Then
                    Throw New ArgumentException("Raw labels must be immutable text/numeric/Boolean scalars.", NameOf(rawLabels))
                End If
            Next

            ReDim groups(data.Groups.Length - 1)
            For i As Integer = 0 To groups.Length - 1
                groups(i) = If(data.Groups(i) Is Nothing, Nothing, DirectCast(data.Groups(i).Clone(), Double()))
            Next

            sources = New Dictionary(Of Tuple(Of Integer, Integer), UnivariateObservationSource)()
            If observationSources IsNot Nothing Then
                For Each source In observationSources
                    If source Is Nothing OrElse source.GroupIndex < 0 OrElse source.GroupIndex >= groups.Length OrElse groups(source.GroupIndex) Is Nothing OrElse source.ObservationIndex < 0 OrElse source.ObservationIndex >= groups(source.GroupIndex).Length Then
                        Throw New ArgumentException("Source coordinates must identify an existing observation.", NameOf(observationSources))
                    End If

                    Dim key = Tuple.Create(source.GroupIndex, source.ObservationIndex)
                    If sources.ContainsKey(key) Then Throw New ArgumentException("Duplicate observation source.", NameOf(observationSources))
                    sources.Add(key, source)
                Next
            End If
        End Sub

        Public ReadOnly Property DisplayLabels As String()
            Get
                Return DirectCast(names.Clone(), String())
            End Get
        End Property

        Public ReadOnly Property RawLabels As Object()
            Get
                Return DirectCast(raw.Clone(), Object())
            End Get
        End Property

        Public ReadOnly Property Data As GroupedNumericData
            Get
                Dim copy(groups.Length - 1)() As Double
                For i As Integer = 0 To groups.Length - 1
                    copy(i) = If(groups(i) Is Nothing, Nothing, DirectCast(groups(i).Clone(), Double()))
                Next

                Return New GroupedNumericData(DisplayLabels, copy)
            End Get
        End Property

        ''' <summary>Explicit permutation from chart slot to original observation, with no
        ''' inferred physical row. A missing group vector has zero plotted observations.</summary>
        Public Function PointOrder(groupOrdinal As Integer, Optional observationOrder As Integer() = Nothing) As IReadOnlyList(Of OrderedPlotPoint)
            If groupOrdinal < 0 OrElse groupOrdinal >= groups.Length Then Throw New ArgumentOutOfRangeException(NameOf(groupOrdinal))
            Dim count = If(groups(groupOrdinal) Is Nothing, 0, groups(groupOrdinal).Length)
            Dim order = If(observationOrder Is Nothing, Enumerable.Range(0, count).ToArray(), DirectCast(observationOrder.Clone(), Integer()))
            If order.Length <> count OrElse order.Distinct().Count() <> count OrElse order.Any(Function(i) i < 0 OrElse i >= count) Then
                Throw New ArgumentException("Order must be a permutation of the group observations.", NameOf(observationOrder))
            End If

            Dim points As New List(Of OrderedPlotPoint)()
            For chartIndex As Integer = 0 To order.Length - 1
                Dim source As UnivariateObservationSource = Nothing
                sources.TryGetValue(Tuple.Create(groupOrdinal, order(chartIndex)), source)
                points.Add(New OrderedPlotPoint(groupOrdinal, order(chartIndex), chartIndex, source))
            Next

            Return New ReadOnlyCollection(Of OrderedPlotPoint)(points)
        End Function
    End Class
End Namespace

