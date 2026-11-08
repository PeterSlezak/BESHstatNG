Option Explicit On
Option Strict On
Option Infer On

Imports System
Imports System.Collections.Generic
Imports System.Globalization
Imports System.Linq
Imports BESHStatNG.Multivariate

Namespace ExcelCommon
    Public Enum MultivariateOrdinalKind37
        ModelPositionZeroBased
        ClusterSlotZeroBased
        InputOrdinalOneBased
    End Enum

    ''' <summary>One ordered point set. OrdinalKind explicitly distinguishes model vector
    ''' positions, cluster slots and original input ordinals. None is an Excel point/row index.</summary>
    Public NotInheritable Class MultivariatePointSeries37
        Private ReadOnly _x As Double(), _y As Double(), _labels As String(), _ordinals As Integer(), _modelRowIds As Integer?()
        Private ReadOnly _axisKind As MultivariateSemanticAxis
        Friend Sub New(name As String, x As Double(), y As Double(), labels As String(), ordinals As Integer(), role As MultivariatePlotDataRole,
                       Optional ordinalKind As MultivariateOrdinalKind37 = MultivariateOrdinalKind37.ModelPositionZeroBased,
                       Optional axisKind As MultivariateSemanticAxis = MultivariateSemanticAxis.ScoreObservations,
                       Optional modelRowIds As Integer?() = Nothing)
            Me.Name = name : Me.Role = role : Me.OrdinalKind = ordinalKind
            _x = Clone(x) : _y = Clone(y) : _labels = Clone(labels) : _ordinals = Clone(ordinals)
            _axisKind = axisKind : _modelRowIds = Clone(modelRowIds)
        End Sub
        Private Shared Function Clone(Of T)(values As T()) As T()
            Return If(values Is Nothing, Nothing, DirectCast(values.Clone(), T()))
        End Function
        Public ReadOnly Property Name As String
        Public ReadOnly Property Role As MultivariatePlotDataRole
        Public ReadOnly Property OrdinalKind As MultivariateOrdinalKind37
        Public ReadOnly Property XValues As Double()
            Get
                Return Clone(_x)
            End Get
        End Property
        ''' <summary>Actual Core model RowIds where supplied. These are separately identified;
        ''' this payload makes no claim that a model ID is a physical worksheet row.</summary>
        Public ReadOnly Property ModelRowIds As Integer?()
            Get
                Return Clone(_modelRowIds)
            End Get
        End Property
        Public ReadOnly Property YValues As Double()
            Get
                Return Clone(_y)
            End Get
        End Property
        Public ReadOnly Property Labels As String()
            Get
                Return Clone(_labels)
            End Get
        End Property
        Public ReadOnly Property SourceOrdinals As Integer()
            Get
                Return Clone(_ordinals)
            End Get
        End Property
        ''' <summary>Reuse the accepted semantic axis contract. Model positions remain
        ''' model result indices with unavailable input/physical mapping. Leaf input
        ''' ordinals are known; physical worksheet rows are never synthesized.</summary>
        Public ReadOnly Property SourceAxis As MultivariateAxisMap
            Get
                If _ordinals Is Nothing OrElse OrdinalKind = MultivariateOrdinalKind37.ClusterSlotZeroBased Then Return Nothing
                Dim leaf As Boolean = OrdinalKind = MultivariateOrdinalKind37.InputOrdinalOneBased
                Dim kind = If(leaf, MultivariateSemanticAxis.Observations, _axisKind)
                Dim items As New List(Of MultivariateAxisItem)
                For i As Integer = 0 To _ordinals.Length - 1
                    items.Add(New MultivariateAxisItem(If(leaf, i, _ordinals(i)), Nothing,
                        sourcePosition:=If(leaf, CType(_ordinals(i) - 1, Integer?), Nothing),
                        displayLabel:=If(_labels IsNot Nothing AndAlso i < _labels.Length, _labels(i), Nothing),
                        state:=If(leaf, MultivariateMappingState.Known, MultivariateMappingState.Unavailable)))
                Next
                Return New MultivariateAxisMap(kind, items)
            End Get
        End Property
    End Class

    ''' <summary>Presentation over existing model vectors and existing PlotPlan roles. Call
    ''' formatting/label functions at their original renderer phase, rather than eagerly
    ''' converting a whole chart and changing late failures into pre-creation failures.</summary>
    Public NotInheritable Class MultivariateChartPresentation37
        Private Sub New()
        End Sub
        Public Const PcaLoading2DName As String = "Loadings Plot2D"
        Public Const PcaLoading3DName As String = "Loadings Plot3D"
        Public Const PcaLoadingTitle As String = "Component Loadings Plot"
        Public Const PcaScore2DName As String = "Score Plot2D"
        Public Const PcaScore3DName As String = "Score Plot3D"
        Public Const PcaScoreTitle As String = "Scores Plot"
        Public Const ScreeName As String = "Scree Plot"
        Public Const ComponentAxisTitle As String = "Principal Component"
        Public Const VarianceAxisTitle As String = "Variance explained [%]"
        Public Const PercentSeriesName As String = "Percent Explained"
        Public Const InitialVarianceSeriesName As String = "Initial Variance Explained"
        Public Const FactorAxisTitle As String = "Factor"
        Public Const FactorLoading2DName As String = "Factor Loadings Plot2D"
        Public Const FactorLoading3DName As String = "Factor Loadings Plot3D"
        Public Const FactorLoadingTitle As String = "Factor Loadings Plot"
        Public Const CorrespondenceName As String = "CA plot"
        Public Const CorrespondenceTitle As String = "Correspondence Plot"
        Public Const ContributionAxisTitle As String = "Contribution"
        Public Const BiplotTitlePrefix As String = "Biplot: "
        Public Const YZeroName As String = "Y Zero Line"
        Public Const XZeroName As String = "X Zero Line"

        Public Shared Function BiplotName(c As Double) As String
            Return "Biplot scale=" & c.ToString(CultureInfo.CurrentCulture)
        End Function
        Public Shared Function LoadingSeriesName(ordinal As Integer) As String
            Return "Loading_" & ordinal.ToString(CultureInfo.CurrentCulture)
        End Function
        Public Shared Function ShowScoreLegend(groupSeriesCount As Integer) As Boolean
            Return groupSeriesCount > 1
        End Function
        Public Shared Function ShowCorrespondenceRows(model As CA) As Boolean
            Return Not model.IsMultiple
        End Function
        Public Shared Function ContributionName(axis As Integer) As String
            Return "Contribution" & (axis + 1).ToString(CultureInfo.CurrentCulture)
        End Function
        Public Shared Function PercentLabel(value As Double) As String
            'VB Format$ suppresses the sign of zero, including on modern runtimes whose
            'Double custom formatting otherwise retains it. Numerical vectors keep their bits.
            If value = 0.0 Then value = 0.0
            Return value.ToString("#0.0#", CultureInfo.CurrentCulture)
        End Function
        Public Shared Function ComponentCaption(percent As Double, component As Integer, scoreCapitalization As Boolean) As String
            Dim prefix As String = If(component = 0, If(scoreCapitalization, "1St", "1st"), If(component = 1, "2nd", If(scoreCapitalization, "3Rd", "3rd")))
            Return prefix & " Component Scores [" & PercentLabel(percent) & "%]"
        End Function
        Public Shared Function FactorCaption(name As String, percent As Double) As String
            Return name & " [" & PercentLabel(percent) & "%]"
        End Function
        Public Shared Function BiplotDescription(c As Double) As String
            If c = 0.0 Then Return "GH, or column-metric preserving"
            If c = 1.0 Then Return "JK, or row-metric preserving"
            If c = 0.5 Then Return "SQ, or symmetric"
            Return String.Empty
        End Function
        Public Shared Function ScoreLabels(model As PCA, rowLabels As String()) As String()
            Dim n As Integer = model.ObservationCount
            Dim rowIds() As Integer = model.RowIds
            Dim result(n - 1) As String
            If rowLabels IsNot Nothing AndAlso rowLabels.Length <> n Then
                Throw New ArgumentException("PCA row-label count must match the number of observations.", NameOf(rowLabels))
            End If
            For i As Integer = 0 To n - 1
                Dim text As String = String.Empty
                If rowLabels IsNot Nothing AndAlso rowLabels(i) IsNot Nothing Then text = rowLabels(i).Trim()
                If text = String.Empty Then text = rowIds(i).ToString(CultureInfo.CurrentCulture)
                result(i) = text
            Next
            Return result
        End Function
        Public Shared Function NormalizeGroups(groupLabels As String(), expectedCount As Integer) As String()
            If groupLabels Is Nothing Then Return Nothing
            If groupLabels.Length <> expectedCount Then
                Throw New ArgumentException("PCA grouping-label count must match the number of observations.", NameOf(groupLabels))
            End If
            Dim result(expectedCount - 1) As String
            For i As Integer = 0 To expectedCount - 1
                Dim text As String = If(groupLabels(i), String.Empty).Trim()
                If text = String.Empty Then text = "(missing)"
                result(i) = text
            Next
            Return result
        End Function
        Public Shared Function ScoreSeries(x As Double(), y As Double(), labels As String(), groups As String(),
                                           Optional ungroupedName As String = "Score plot", Optional modelRowIds As Integer() = Nothing) As IReadOnlyList(Of MultivariatePointSeries37)
            Dim result As New List(Of MultivariatePointSeries37)
            If groups Is Nothing Then
                Dim ordinals = Enumerable.Range(0, x.Length).ToArray()
                result.Add(New MultivariatePointSeries37(ungroupedName, x, y, labels, ordinals, MultivariatePlotDataRole.CoreScores,
                    modelRowIds:=ModelIds(modelRowIds, ordinals)))
                Return result.AsReadOnly()
            End If
            Dim names As New List(Of String), seen As New HashSet(Of String)(StringComparer.Ordinal)
            For Each name As String In groups
                If seen.Add(name) Then names.Add(name)
            Next
            For Each name As String In names
                Dim xs As New List(Of Double), ys As New List(Of Double), texts As New List(Of String), ordinals As New List(Of Integer)
                For i As Integer = 0 To groups.Length - 1
                    'Original /optioncompare:text membership, with ordinal distinct-name discovery.
                    If CultureInfo.CurrentCulture.CompareInfo.Compare(groups(i), name,
                        CompareOptions.IgnoreCase Or CompareOptions.IgnoreKanaType Or CompareOptions.IgnoreWidth) = 0 Then
                        xs.Add(x(i)) : ys.Add(y(i)) : texts.Add(labels(i)) : ordinals.Add(i)
                    End If
                Next
                result.Add(New MultivariatePointSeries37(name, xs.ToArray(), ys.ToArray(), texts.ToArray(), ordinals.ToArray(), MultivariatePlotDataRole.CoreScores,
                    modelRowIds:=ModelIds(modelRowIds, ordinals.ToArray())))
            Next
            Return result.AsReadOnly()
        End Function
        Private Shared Function ModelIds(ids As Integer(), ordinals As Integer()) As Integer?()
            If ids Is Nothing Then Return Nothing
            Return ordinals.Select(Function(i) If(i >= 0 AndAlso i < ids.Length, CType(ids(i), Integer?), Nothing)).ToArray()
        End Function
        Public Shared Function ContributionTitle(axis As Integer) As String
            Return "Contribution Plot: Axis " & (axis + 1).ToString(CultureInfo.CurrentCulture)
        End Function
        Public Shared Function ContributionLabels(model As CA, row As Boolean) As String()
            Return model.PlotLabels(row, String.Concat(Convert.ToChar(13), Convert.ToChar(10)))
        End Function
        Public Shared Function CorrespondenceLabel(model As CA, row As Boolean, ordinal As Integer) As String
            Return model.PlotLabels(row, Nothing)(ordinal)
        End Function
        Public Shared Function CorrespondenceSeries(model As CA, row As Boolean) As MultivariatePointSeries37
            Dim x = If(row, model.RowFactors(0), model.ColFactors(0))
            Dim y = If(row, model.RowFactors(1), model.ColFactors(1))
            Return New MultivariatePointSeries37(If(row, "Rows", "Columns"), x, y, Nothing,
                Enumerable.Range(0, x.Length).ToArray(), MultivariatePlotDataRole.CoreCorrespondence,
                axisKind:=If(model.IsMultiple, MultivariateSemanticAxis.Categories, If(row, MultivariateSemanticAxis.ContingencyRows, MultivariateSemanticAxis.ContingencyColumns)))
        End Function
        Public Shared Function CorrespondenceCaption(percent As Double, factor As Integer) As String
            Return "Factor " & factor.ToString(CultureInfo.CurrentCulture) & " [" & PercentLabel(percent) & "%]"
        End Function
    End Class

    ''' <summary>Existing layout presentation, with lazy phase-specific series admission.
    ''' Leaf identities retain Core original input ordinals; no physical rows are inferred.</summary>
    Public NotInheritable Class DendrogramChartPresentation37
        Public Sub New(geometry As DendrogramChartGeometry37, labelMode As DendrogramLabelMode)
            Me.Geometry = geometry : Me.LabelMode = labelMode
        End Sub
        Public ReadOnly Property Geometry As DendrogramChartGeometry37
        Public ReadOnly Property LabelMode As DendrogramLabelMode
        Public Function BaseSeries() As MultivariatePointSeries37
            Return New MultivariatePointSeries37("Dendrogram", Geometry.PolylineX, Geometry.PolylineY, Nothing, Nothing, MultivariatePlotDataRole.CoreDendrogram)
        End Function
        ''' <summary>SourceOrdinals contains the original zero-based cluster slot, not series position.</summary>
        Public Function Clusters() As IReadOnlyList(Of MultivariatePointSeries37)
            Dim result As New List(Of MultivariatePointSeries37), xs = Geometry.ClusterPolylineX, ys = Geometry.ClusterPolylineY
            If xs IsNot Nothing AndAlso ys IsNot Nothing Then
                For i As Integer = 0 To Math.Min(xs.Count, ys.Count) - 1
                    If xs(i) Is Nothing OrElse ys(i) Is Nothing Then Continue For
                    If xs(i).Length = 0 OrElse ys(i).Length = 0 Then Continue For
                    result.Add(New MultivariatePointSeries37("Cluster " & (i + 1).ToString(CultureInfo.CurrentCulture), xs(i), ys(i), Nothing, {i}, MultivariatePlotDataRole.CoreDendrogram, MultivariateOrdinalKind37.ClusterSlotZeroBased))
                Next
            End If
            Return result.AsReadOnly()
        End Function
        Public Function CutSeries() As MultivariatePointSeries37
            Dim x = Geometry.CutLineX, y = Geometry.CutLineY
            If x Is Nothing OrElse y Is Nothing OrElse x.Length < 2 OrElse y.Length < 2 Then Return Nothing
            Return New MultivariatePointSeries37("Cut", x, y, Nothing, Nothing, MultivariatePlotDataRole.CoreDendrogram)
        End Function
        Public Function LeafSeries() As MultivariatePointSeries37
            Dim x = Geometry.LeafX, labels = Geometry.LeafLabels
            If LabelMode <> DendrogramLabelMode.DataLabels OrElse x Is Nothing OrElse labels Is Nothing OrElse labels.Length = 0 Then Return Nothing
            Return New MultivariatePointSeries37("LeafLabels", x, Geometry.LeafY, labels, Geometry.LeafOriginalRowOrder, MultivariatePlotDataRole.CoreDendrogram, MultivariateOrdinalKind37.InputOrdinalOneBased)
        End Function
        Public Function DistanceTitle(title As String) As String
            If Not String.IsNullOrWhiteSpace(title) Then Return title
            Return If(Geometry.HeightMode = DendrogramHeightMode.StepLevels, "Distance not in proportion", "Distance")
        End Function
    End Class
End Namespace
