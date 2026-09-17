Option Explicit On
Option Strict Off
Option Infer On

Imports System
Imports System.Collections.Generic
Imports System.Globalization
Imports System.Linq

''' <summary>
''' Controls how nodes are ordered vertically within each Sankey stage.
''' </summary>
Public Enum SankeyNodeOrderMode
    ''' <summary>
    ''' Starts with first-appearance order and performs weighted barycentric
    ''' left-to-right/right-to-left sweeps to reduce ribbon crossings.
    ''' </summary>
    Automatic

    ''' <summary>Preserves the order in which categorical levels first appear.</summary>
    FirstAppearance

    ''' <summary>Orders node labels alphabetically within each stage.</summary>
    Alphabetical

    ''' <summary>Orders nodes from largest to smallest node weight within each stage.</summary>
    WeightDescending
End Enum

''' <summary>
''' Controls vertical placement when a stage does not consume the full plot height.
''' </summary>
Public Enum SankeyVerticalAlignment
    Top
    Center
    Bottom
End Enum

''' <summary>
''' Numerical/layout options for <see cref="SankeyPlot"/>.
''' All geometric results are returned in normalized coordinates from 0 to 1.
''' </summary>
Public Class SankeyPlotOptions
    ''' <summary>
    ''' Vertical gap between neighboring nodes, expressed as a fraction of the
    ''' available plot height. Default: 0.02.
    ''' </summary>
    Public Property NodeGapFraction As Double = 0.02R

    ''' <summary>Vertical node-ordering strategy. Default: Automatic.</summary>
    Public Property NodeOrder As SankeyNodeOrderMode = SankeyNodeOrderMode.Automatic

    ''' <summary>
    ''' Number of paired barycentric crossing-reduction passes used by Automatic
    ''' ordering. Default: 8. Values from 1 to 50 are accepted.
    ''' </summary>
    Public Property CrossingReductionPasses As Integer = 8

    ''' <summary>
    ''' Placement of a stage whose nodes use less than the full height. Default: Center.
    ''' </summary>
    Public Property VerticalAlignment As SankeyVerticalAlignment = SankeyVerticalAlignment.Center

    ''' <summary>
    ''' For staged/wide data, when True a row such as A, blank, C is represented by
    ''' a direct A-to-C link spanning the missing stage. When False (default), an
    ''' intermediate missing value breaks that row's flow. Leading missing stages do
    ''' not prevent a later-stage source from being used.
    ''' </summary>
    Public Property ConnectAcrossMissingStages As Boolean = False

    Friend Function Copy() As SankeyPlotOptions
        Return New SankeyPlotOptions With {
            .NodeGapFraction = NodeGapFraction,
            .NodeOrder = NodeOrder,
            .CrossingReductionPasses = CrossingReductionPasses,
            .VerticalAlignment = VerticalAlignment,
            .ConnectAcrossMissingStages = ConnectAcrossMissingStages
        }
    End Function
End Class

''' <summary>
''' Immutable node returned by <see cref="SankeyPlot.Compute"/> or
''' <see cref="SankeyPlot.ComputeStaged"/>.
''' </summary>
Public NotInheritable Class SankeyNode
    Private ReadOnly _id As String
    Private ReadOnly _label As String
    Private ReadOnly _stageIndex As Integer
    Private ReadOnly _orderInStage As Integer
    Private ReadOnly _incomingWeight As Double
    Private ReadOnly _outgoingWeight As Double
    Private ReadOnly _value As Double
    Private ReadOnly _x As Double
    Private ReadOnly _y As Double
    Private ReadOnly _height As Double
    Private ReadOnly _firstAppearanceIndex As Integer
    Private ReadOnly _colorKey As String

    Friend Sub New(id As String,
                   label As String,
                   stageIndex As Integer,
                   orderInStage As Integer,
                   incomingWeight As Double,
                   outgoingWeight As Double,
                   value As Double,
                   x As Double,
                   y As Double,
                   height As Double,
                   firstAppearanceIndex As Integer,
                   colorKey As String)
        _id = id
        _label = label
        _stageIndex = stageIndex
        _orderInStage = orderInStage
        _incomingWeight = incomingWeight
        _outgoingWeight = outgoingWeight
        _value = value
        _x = x
        _y = y
        _height = height
        _firstAppearanceIndex = firstAppearanceIndex
        _colorKey = colorKey
    End Sub

    ''' <summary>Stable identifier used by <see cref="SankeyLink"/>.</summary>
    Public ReadOnly Property Id As String
        Get
            Return _id
        End Get
    End Property

    ''' <summary>Display label for the categorical level.</summary>
    Public ReadOnly Property Label As String
        Get
            Return _label
        End Get
    End Property

    ''' <summary>Zero-based left-to-right stage index.</summary>
    Public ReadOnly Property StageIndex As Integer
        Get
            Return _stageIndex
        End Get
    End Property

    ''' <summary>Zero-based top-to-bottom order within the stage.</summary>
    Public ReadOnly Property OrderInStage As Integer
        Get
            Return _orderInStage
        End Get
    End Property

    Public ReadOnly Property IncomingWeight As Double
        Get
            Return _incomingWeight
        End Get
    End Property

    Public ReadOnly Property OutgoingWeight As Double
        Get
            Return _outgoingWeight
        End Get
    End Property

    ''' <summary>
    ''' Node thickness basis. This is max(IncomingWeight, OutgoingWeight), allowing
    ''' new flow to enter at an intermediate node and allowing attrition/termination.
    ''' </summary>
    Public ReadOnly Property Value As Double
        Get
            Return _value
        End Get
    End Property

    ''' <summary>Normalized horizontal centre of the node, in [0, 1].</summary>
    Public ReadOnly Property X As Double
        Get
            Return _x
        End Get
    End Property

    ''' <summary>Normalized top edge of the node, in [0, 1].</summary>
    Public ReadOnly Property Y As Double
        Get
            Return _y
        End Get
    End Property

    ''' <summary>Normalized node height using the common chart-wide weight scale.</summary>
    Public ReadOnly Property Height As Double
        Get
            Return _height
        End Get
    End Property

    ''' <summary>Original level discovery order, useful for deterministic formatting.</summary>
    Public ReadOnly Property FirstAppearanceIndex As Integer
        Get
            Return _firstAppearanceIndex
        End Get
    End Property

    ''' <summary>
    ''' Stable category key. Equal labels/types use the same key even when the same
    ''' category appears in several stages, allowing a renderer to reuse its color.
    ''' </summary>
    Public ReadOnly Property ColorKey As String
        Get
            Return _colorKey
        End Get
    End Property
End Class

''' <summary>
''' Immutable aggregated ribbon returned by the Sankey backend.
''' </summary>
Public NotInheritable Class SankeyLink
    Private ReadOnly _sourceNodeId As String
    Private ReadOnly _targetNodeId As String
    Private ReadOnly _value As Double
    Private ReadOnly _thickness As Double
    Private ReadOnly _sourceTop As Double
    Private ReadOnly _targetTop As Double
    Private ReadOnly _firstAppearanceIndex As Integer

    Friend Sub New(sourceNodeId As String,
                   targetNodeId As String,
                   value As Double,
                   thickness As Double,
                   sourceTop As Double,
                   targetTop As Double,
                   firstAppearanceIndex As Integer)
        _sourceNodeId = sourceNodeId
        _targetNodeId = targetNodeId
        _value = value
        _thickness = thickness
        _sourceTop = sourceTop
        _targetTop = targetTop
        _firstAppearanceIndex = firstAppearanceIndex
    End Sub

    Public ReadOnly Property SourceNodeId As String
        Get
            Return _sourceNodeId
        End Get
    End Property

    Public ReadOnly Property TargetNodeId As String
        Get
            Return _targetNodeId
        End Get
    End Property

    Public ReadOnly Property Value As Double
        Get
            Return _value
        End Get
    End Property

    ''' <summary>Ribbon thickness as a normalized fraction of plot height.</summary>
    Public ReadOnly Property Thickness As Double
        Get
            Return _thickness
        End Get
    End Property

    ''' <summary>Normalized top edge of the ribbon at the source node.</summary>
    Public ReadOnly Property SourceTop As Double
        Get
            Return _sourceTop
        End Get
    End Property

    ''' <summary>Normalized top edge of the ribbon at the target node.</summary>
    Public ReadOnly Property TargetTop As Double
        Get
            Return _targetTop
        End Get
    End Property

    Public ReadOnly Property FirstAppearanceIndex As Integer
        Get
            Return _firstAppearanceIndex
        End Get
    End Property
End Class

''' <summary>
''' Immutable result from the Excel-independent Sankey calculation/layout engine.
''' </summary>
Public NotInheritable Class SankeyPlotResult
    Private ReadOnly _nodes As SankeyNode()
    Private ReadOnly _links As SankeyLink()
    Private ReadOnly _stageNames As String()
    Private ReadOnly _stageTotals As Double()
    Private ReadOnly _sourceObservationCount As Integer
    Private ReadOnly _usableObservationCount As Integer
    Private ReadOnly _excludedObservationCount As Integer
    Private ReadOnly _rawLinkCount As Integer
    Private ReadOnly _weightScale As Double

    Friend Sub New(nodes As SankeyNode(),
                   links As SankeyLink(),
                   stageNames As String(),
                   stageTotals As Double(),
                   sourceObservationCount As Integer,
                   usableObservationCount As Integer,
                   excludedObservationCount As Integer,
                   rawLinkCount As Integer,
                   weightScale As Double)
        _nodes = DirectCast(nodes.Clone(), SankeyNode())
        _links = DirectCast(links.Clone(), SankeyLink())
        _stageNames = DirectCast(stageNames.Clone(), String())
        _stageTotals = DirectCast(stageTotals.Clone(), Double())
        _sourceObservationCount = sourceObservationCount
        _usableObservationCount = usableObservationCount
        _excludedObservationCount = excludedObservationCount
        _rawLinkCount = rawLinkCount
        _weightScale = weightScale
    End Sub

    Public ReadOnly Property Nodes As SankeyNode()
        Get
            Return DirectCast(_nodes.Clone(), SankeyNode())
        End Get
    End Property

    Public ReadOnly Property Links As SankeyLink()
        Get
            Return DirectCast(_links.Clone(), SankeyLink())
        End Get
    End Property

    Public ReadOnly Property StageNames As String()
        Get
            Return DirectCast(_stageNames.Clone(), String())
        End Get
    End Property

    ''' <summary>Sum of node values at each stage.</summary>
    Public ReadOnly Property StageTotals As Double()
        Get
            Return DirectCast(_stageTotals.Clone(), Double())
        End Get
    End Property

    Public ReadOnly Property StageCount As Integer
        Get
            Return _stageNames.Length
        End Get
    End Property

    Public ReadOnly Property NodeCount As Integer
        Get
            Return _nodes.Length
        End Get
    End Property

    ''' <summary>Number of aggregated Source-to-Target ribbons.</summary>
    Public ReadOnly Property LinkCount As Integer
        Get
            Return _links.Length
        End Get
    End Property

    ''' <summary>
    ''' Number of usable links before equal Source/Target pairs were aggregated.
    ''' </summary>
    Public ReadOnly Property RawLinkCount As Integer
        Get
            Return _rawLinkCount
        End Get
    End Property

    Public ReadOnly Property SourceObservationCount As Integer
        Get
            Return _sourceObservationCount
        End Get
    End Property

    Public ReadOnly Property UsableObservationCount As Integer
        Get
            Return _usableObservationCount
        End Get
    End Property

    Public ReadOnly Property ExcludedObservationCount As Integer
        Get
            Return _excludedObservationCount
        End Get
    End Property

    ''' <summary>
    ''' Common normalized height per unit of weight. Using one global scale keeps
    ''' ribbon thickness visually identical at its source and target stages.
    ''' </summary>
    Public ReadOnly Property WeightScale As Double
        Get
            Return _weightScale
        End Get
    End Property

    ''' <summary>
    ''' Largest stage total. This is a useful default denominator for optional
    ''' percentage labels in the Excel renderer.
    ''' </summary>
    Public ReadOnly Property ReferenceTotal As Double
        Get
            If _stageTotals.Length = 0 Then Return 0.0R
            Return _stageTotals.Max()
        End Get
    End Property
End Class

''' <summary>
''' Excel-independent Sankey/alluvial backend.
'''
''' The primary representation is a directed acyclic graph of weighted links. It can
''' be created directly from Source/Target/Weight vectors or adapted from a wide
''' staged table. Nodes may start at any stage. With automatically inferred stages,
''' shorter branches are right-aligned where possible, so a new source feeding an
''' already-late target is not unnecessarily forced into the left-most column.
''' </summary>
Public NotInheritable Class SankeyPlot
    Private Sub New()
    End Sub

    Private NotInheritable Class DirectInputRow
        Friend SourceValue As Object
        Friend TargetValue As Object
        Friend Weight As Double
        Friend SourceStage As Nullable(Of Integer)
        Friend TargetStage As Nullable(Of Integer)
        Friend AppearanceIndex As Integer
    End Class

    Private NotInheritable Class WorkingNode
        Friend Id As String
        Friend Key As String
        Friend ColorKey As String
        Friend Label As String
        Friend StageIndex As Integer
        Friend StageIsExplicit As Boolean
        Friend FirstAppearanceIndex As Integer
        Friend Incoming As New List(Of WorkingLink)()
        Friend Outgoing As New List(Of WorkingLink)()
        Friend IncomingWeight As Double
        Friend OutgoingWeight As Double
        Friend Value As Double
        Friend OrderInStage As Integer
        Friend X As Double
        Friend Y As Double
        Friend Height As Double
    End Class

    Private NotInheritable Class WorkingLink
        Friend Source As WorkingNode
        Friend Target As WorkingNode
        Friend Weight As Double
        Friend FirstAppearanceIndex As Integer
        Friend SourceTop As Double
        Friend TargetTop As Double
        Friend Thickness As Double
    End Class

    Private NotInheritable Class BuildContext
        Friend Nodes As New List(Of WorkingNode)()
        Friend NodesByKey As New Dictionary(Of String, WorkingNode)(StringComparer.Ordinal)
        Friend Links As New List(Of WorkingLink)()
        Friend LinksByKey As New Dictionary(Of String, WorkingLink)(StringComparer.Ordinal)
        Friend NextNodeNumber As Integer = 1
        Friend NextNodeAppearance As Integer = 0
        Friend NextLinkAppearance As Integer = 0
    End Class

    ''' <summary>
    ''' Computes a Sankey graph from row-aligned Source and Target vectors.
    ''' </summary>
    ''' <param name="source">Source categories. Text and numeric values are supported.</param>
    ''' <param name="target">Target categories aligned with <paramref name="source"/>.</param>
    ''' <param name="weights">
    ''' Optional positive weights. Nothing means one unit per usable row. NaN is treated
    ''' as missing; zero-weight rows are ignored; negative/infinite weights are rejected.
    ''' </param>
    ''' <param name="sourceStages">
    ''' Optional zero-based (or consistently offset) source stage positions. If supplied,
    ''' targetStages must also be supplied. The smallest supplied stage is normalized to
    ''' zero while gaps are preserved.
    ''' </param>
    ''' <param name="targetStages">Optional target stage positions aligned with sourceStages.</param>
    ''' <param name="stageNames">
    ''' Optional names for the resolved stages. In explicit-stage mode its length must
    ''' equal the resolved stage span; in automatic mode it must equal the inferred count.
    ''' </param>
    Public Shared Function Compute(source As Array,
                                   target As Array,
                                   Optional weights As Double() = Nothing,
                                   Optional sourceStages As Integer() = Nothing,
                                   Optional targetStages As Integer() = Nothing,
                                   Optional stageNames As String() = Nothing,
                                   Optional options As SankeyPlotOptions = Nothing) As SankeyPlotResult
        If source Is Nothing Then Throw New ArgumentNullException(NameOf(source))
        If target Is Nothing Then Throw New ArgumentNullException(NameOf(target))
        If source.Rank <> 1 Then Throw New ArgumentException("Source must be a one-dimensional array.", NameOf(source))
        If target.Rank <> 1 Then Throw New ArgumentException("Target must be a one-dimensional array.", NameOf(target))
        If source.Length <> target.Length Then
            Throw New ArgumentException("Source and Target must contain the same number of rows.", NameOf(target))
        End If
        If source.Length = 0 Then Throw New ArgumentException("At least one Source/Target row is required.", NameOf(source))
        If weights IsNot Nothing AndAlso weights.Length <> source.Length Then
            Throw New ArgumentException("Weights must contain the same number of rows as Source and Target.", NameOf(weights))
        End If

        Dim hasSourceStages As Boolean = sourceStages IsNot Nothing
        Dim hasTargetStages As Boolean = targetStages IsNot Nothing
        If hasSourceStages Xor hasTargetStages Then
            Throw New ArgumentException("Source stages and Target stages must either both be supplied or both be omitted.")
        End If
        If hasSourceStages Then
            If sourceStages.Length <> source.Length Then
                Throw New ArgumentException("Source stages must contain the same number of rows as Source.", NameOf(sourceStages))
            End If
            If targetStages.Length <> source.Length Then
                Throw New ArgumentException("Target stages must contain the same number of rows as Source.", NameOf(targetStages))
            End If
        End If

        Dim resolvedOptions As SankeyPlotOptions = If(options, New SankeyPlotOptions()).Copy()
        ValidateOptions(resolvedOptions)

        Dim inputRows As New List(Of DirectInputRow)()
        Dim excludedCount As Integer = 0
        Dim sourceLower As Integer = source.GetLowerBound(0)
        Dim targetLower As Integer = target.GetLowerBound(0)
        Dim minimumExplicitStage As Integer = Integer.MaxValue
        Dim maximumExplicitStage As Integer = Integer.MinValue

        For i As Integer = 0 To source.Length - 1
            Dim sourceValue As Object = source.GetValue(sourceLower + i)
            Dim targetValue As Object = target.GetValue(targetLower + i)

            If IsMissingCategoryValue(sourceValue) OrElse IsMissingCategoryValue(targetValue) Then
                excludedCount += 1
                Continue For
            End If

            Dim weight As Double = If(weights Is Nothing, 1.0R, weights(i))
            If Double.IsNaN(weight) Then
                excludedCount += 1
                Continue For
            End If
            ValidateWeight(weight, i)
            If weight = 0.0R Then
                excludedCount += 1
                Continue For
            End If

            Dim row As New DirectInputRow With {
                .SourceValue = sourceValue,
                .TargetValue = targetValue,
                .Weight = weight,
                .AppearanceIndex = i
            }

            If hasSourceStages Then
                Dim sourceStage As Integer = sourceStages(i)
                Dim targetStage As Integer = targetStages(i)
                If sourceStage < 0 OrElse targetStage < 0 Then
                    Throw New ArgumentOutOfRangeException(NameOf(sourceStages),
                                                          "Stage values must be non-negative. Invalid row " &
                                                          (i + 1).ToString(CultureInfo.CurrentCulture) & ".")
                End If
                If targetStage <= sourceStage Then
                    Throw New ArgumentException("Target stage must be greater than Source stage at row " &
                                                (i + 1).ToString(CultureInfo.CurrentCulture) & ".")
                End If
                row.SourceStage = sourceStage
                row.TargetStage = targetStage
                minimumExplicitStage = Math.Min(minimumExplicitStage, sourceStage)
                minimumExplicitStage = Math.Min(minimumExplicitStage, targetStage)
                maximumExplicitStage = Math.Max(maximumExplicitStage, sourceStage)
                maximumExplicitStage = Math.Max(maximumExplicitStage, targetStage)
            End If

            inputRows.Add(row)
        Next

        If inputRows.Count = 0 Then
            Throw New ArgumentException("No usable positive-weight Source/Target rows were found.")
        End If

        Dim explicitStageCount As Nullable(Of Integer) = Nothing
        If hasSourceStages Then
            For Each row As DirectInputRow In inputRows
                row.SourceStage = row.SourceStage.Value - minimumExplicitStage
                row.TargetStage = row.TargetStage.Value - minimumExplicitStage
            Next
            explicitStageCount = maximumExplicitStage - minimumExplicitStage + 1
        End If

        Return ComputeFromInputRows(inputRows,
                                    source.Length,
                                    inputRows.Count,
                                    excludedCount,
                                    inputRows.Count,
                                    explicitStageCount,
                                    stageNames,
                                    resolvedOptions)
    End Function

    ''' <summary>
    ''' Computes a Sankey/alluvial graph from a wide row-by-stage categorical table.
    ''' Each row is one path/observation. Leading blank stages are allowed, so a path
    ''' may enter the diagram at any later stage rather than only at the left-most node.
    ''' </summary>
    ''' <param name="stages">Rows are observations; columns are successive stages.</param>
    ''' <param name="weights">Optional positive row weights; Nothing means one per row.</param>
    ''' <param name="stageNames">Optional column/stage names.</param>
    Public Shared Function ComputeStaged(stages As Object(,),
                                         Optional weights As Double() = Nothing,
                                         Optional stageNames As String() = Nothing,
                                         Optional options As SankeyPlotOptions = Nothing) As SankeyPlotResult
        If stages Is Nothing Then Throw New ArgumentNullException(NameOf(stages))

        Dim rowCount As Integer = stages.GetLength(0)
        Dim stageCount As Integer = stages.GetLength(1)
        If rowCount = 0 Then Throw New ArgumentException("At least one observation row is required.", NameOf(stages))
        If stageCount < 2 Then Throw New ArgumentException("At least two stage columns are required.", NameOf(stages))
        If weights IsNot Nothing AndAlso weights.Length <> rowCount Then
            Throw New ArgumentException("Weights must contain the same number of rows as the staged data.", NameOf(weights))
        End If

        Dim resolvedOptions As SankeyPlotOptions = If(options, New SankeyPlotOptions()).Copy()
        ValidateOptions(resolvedOptions)
        ValidateStageNames(stageNames, stageCount)

        Dim rowLower As Integer = stages.GetLowerBound(0)
        Dim columnLower As Integer = stages.GetLowerBound(1)
        Dim inputRows As New List(Of DirectInputRow)()
        Dim usableObservationCount As Integer = 0
        Dim excludedObservationCount As Integer = 0
        Dim rawLinkCount As Integer = 0

        For rowOffset As Integer = 0 To rowCount - 1
            Dim weight As Double = If(weights Is Nothing, 1.0R, weights(rowOffset))
            If Double.IsNaN(weight) Then
                excludedObservationCount += 1
                Continue For
            End If
            ValidateWeight(weight, rowOffset)
            If weight = 0.0R Then
                excludedObservationCount += 1
                Continue For
            End If

            Dim generatedForRow As Integer = 0

            If resolvedOptions.ConnectAcrossMissingStages Then
                Dim previousStage As Integer = -1
                Dim previousValue As Object = Nothing

                For stageOffset As Integer = 0 To stageCount - 1
                    Dim value As Object = stages(rowLower + rowOffset, columnLower + stageOffset)
                    If IsMissingCategoryValue(value) Then Continue For

                    If previousStage >= 0 Then
                        inputRows.Add(New DirectInputRow With {
                            .SourceValue = previousValue,
                            .TargetValue = value,
                            .Weight = weight,
                            .SourceStage = previousStage,
                            .TargetStage = stageOffset,
                            .AppearanceIndex = rawLinkCount
                        })
                        rawLinkCount += 1
                        generatedForRow += 1
                    End If

                    previousStage = stageOffset
                    previousValue = value
                Next
            Else
                For stageOffset As Integer = 0 To stageCount - 2
                    Dim sourceValue As Object = stages(rowLower + rowOffset, columnLower + stageOffset)
                    Dim targetValue As Object = stages(rowLower + rowOffset, columnLower + stageOffset + 1)
                    If IsMissingCategoryValue(sourceValue) OrElse IsMissingCategoryValue(targetValue) Then Continue For

                    inputRows.Add(New DirectInputRow With {
                        .SourceValue = sourceValue,
                        .TargetValue = targetValue,
                        .Weight = weight,
                        .SourceStage = stageOffset,
                        .TargetStage = stageOffset + 1,
                        .AppearanceIndex = rawLinkCount
                    })
                    rawLinkCount += 1
                    generatedForRow += 1
                Next
            End If

            If generatedForRow > 0 Then
                usableObservationCount += 1
            Else
                excludedObservationCount += 1
            End If
        Next

        If inputRows.Count = 0 Then
            Throw New ArgumentException("No usable transitions were found in the staged data.", NameOf(stages))
        End If

        Return ComputeFromInputRows(inputRows,
                                    rowCount,
                                    usableObservationCount,
                                    excludedObservationCount,
                                    rawLinkCount,
                                    stageCount,
                                    stageNames,
                                    resolvedOptions)
    End Function

    Private Shared Function ComputeFromInputRows(inputRows As List(Of DirectInputRow),
                                                  sourceObservationCount As Integer,
                                                  usableObservationCount As Integer,
                                                  excludedObservationCount As Integer,
                                                  rawLinkCount As Integer,
                                                  explicitStageCount As Nullable(Of Integer),
                                                  stageNames As String(),
                                                  options As SankeyPlotOptions) As SankeyPlotResult
        Dim context As New BuildContext()
        Dim hasExplicitStages As Boolean = explicitStageCount.HasValue

        For Each row As DirectInputRow In inputRows
            Dim sourceStage As Nullable(Of Integer) = row.SourceStage
            Dim targetStage As Nullable(Of Integer) = row.TargetStage

            Dim sourceNode As WorkingNode = GetOrCreateNode(context,
                                                            row.SourceValue,
                                                            sourceStage,
                                                            hasExplicitStages)
            Dim targetNode As WorkingNode = GetOrCreateNode(context,
                                                            row.TargetValue,
                                                            targetStage,
                                                            hasExplicitStages)

            If hasExplicitStages AndAlso targetNode.StageIndex <= sourceNode.StageIndex Then
                Throw New ArgumentException("Every Sankey link must move to a later stage.")
            End If

            AddOrAggregateLink(context, sourceNode, targetNode, row.Weight)
        Next

        If context.Links.Count = 0 Then
            Throw New ArgumentException("No usable Sankey links were created.")
        End If

        Dim stageCount As Integer
        If hasExplicitStages Then
            stageCount = explicitStageCount.Value
            ValidateExplicitStageGraph(context)
        Else
            stageCount = InferStages(context)
        End If

        ValidateStageNames(stageNames, stageCount)
        Dim resolvedStageNames() As String = ResolveStageNames(stageNames, stageCount)

        ComputeNodeWeights(context)
        Dim stageNodes As List(Of WorkingNode)() = BuildStageNodeLists(context, stageCount)
        OrderNodes(stageNodes, context.Links, options)

        Dim stageTotals(stageCount - 1) As Double
        Dim weightScale As Double = ComputeNormalizedLayout(stageNodes, stageTotals, options)
        ComputeLinkOffsets(context.Links, weightScale)

        Dim outputNodes As SankeyNode() = context.Nodes _
            .OrderBy(Function(n) n.StageIndex) _
            .ThenBy(Function(n) n.OrderInStage) _
            .Select(Function(n) New SankeyNode(n.Id,
                                              n.Label,
                                              n.StageIndex,
                                              n.OrderInStage,
                                              n.IncomingWeight,
                                              n.OutgoingWeight,
                                              n.Value,
                                              n.X,
                                              n.Y,
                                              n.Height,
                                              n.FirstAppearanceIndex,
                                              n.ColorKey)).ToArray()

        Dim outputLinks As SankeyLink() = context.Links _
            .OrderBy(Function(l) l.FirstAppearanceIndex) _
            .Select(Function(l) New SankeyLink(l.Source.Id,
                                              l.Target.Id,
                                              l.Weight,
                                              l.Thickness,
                                              l.SourceTop,
                                              l.TargetTop,
                                              l.FirstAppearanceIndex)).ToArray()

        Return New SankeyPlotResult(outputNodes,
                                    outputLinks,
                                    resolvedStageNames,
                                    stageTotals,
                                    sourceObservationCount,
                                    usableObservationCount,
                                    excludedObservationCount,
                                    rawLinkCount,
                                    weightScale)
    End Function

    Private Shared Function GetOrCreateNode(context As BuildContext,
                                            value As Object,
                                            stage As Nullable(Of Integer),
                                            stageIsExplicit As Boolean) As WorkingNode
        Dim categoryKey As String = BuildCategoryKey(value)
        Dim nodeKey As String
        If stageIsExplicit Then
            If Not stage.HasValue Then Throw New ArgumentException("An explicit stage is required for every node.")
            nodeKey = stage.Value.ToString(CultureInfo.InvariantCulture) & "|" & categoryKey
        Else
            nodeKey = categoryKey
        End If

        Dim node As WorkingNode = Nothing
        If context.NodesByKey.TryGetValue(nodeKey, node) Then Return node

        node = New WorkingNode With {
            .Id = "N" & context.NextNodeNumber.ToString(CultureInfo.InvariantCulture),
            .Key = nodeKey,
            .ColorKey = categoryKey,
            .Label = FormatCategoryName(value),
            .StageIndex = If(stage.HasValue, stage.Value, 0),
            .StageIsExplicit = stageIsExplicit,
            .FirstAppearanceIndex = context.NextNodeAppearance
        }
        context.NextNodeNumber += 1
        context.NextNodeAppearance += 1
        context.Nodes.Add(node)
        context.NodesByKey.Add(nodeKey, node)
        Return node
    End Function

    Private Shared Sub AddOrAggregateLink(context As BuildContext,
                                          source As WorkingNode,
                                          target As WorkingNode,
                                          weight As Double)
        If source Is target Then
            Throw New ArgumentException("A Sankey link cannot connect a node to itself ('" & source.Label & "').")
        End If

        Dim key As String = source.Id & ">" & target.Id
        Dim link As WorkingLink = Nothing
        If context.LinksByKey.TryGetValue(key, link) Then
            link.Weight += weight
            Return
        End If

        link = New WorkingLink With {
            .Source = source,
            .Target = target,
            .Weight = weight,
            .FirstAppearanceIndex = context.NextLinkAppearance
        }
        context.NextLinkAppearance += 1
        context.Links.Add(link)
        context.LinksByKey.Add(key, link)
        source.Outgoing.Add(link)
        target.Incoming.Add(link)
    End Sub

    Private Shared Sub ValidateExplicitStageGraph(context As BuildContext)
        For Each link As WorkingLink In context.Links
            If link.Target.StageIndex <= link.Source.StageIndex Then
                Throw New ArgumentException("Link '" & link.Source.Label & "' -> '" & link.Target.Label &
                                            "' does not move to a later stage.")
            End If
        Next
    End Sub

    ''' <summary>
    ''' Assigns DAG layers using longest-path depth, then right-aligns nodes wherever
    ''' slack exists. The second pass is what allows a source with no predecessor to
    ''' enter at an intermediate stage when it feeds a node whose stage is already
    ''' established by a longer path.
    ''' </summary>
    Private Shared Function InferStages(context As BuildContext) As Integer
        Dim indegree As New Dictionary(Of String, Integer)(StringComparer.Ordinal)
        For Each node As WorkingNode In context.Nodes
            indegree(node.Id) = node.Incoming.Count
            node.StageIndex = 0
        Next

        Dim available As New List(Of WorkingNode)(context.Nodes.Where(Function(n) indegree(n.Id) = 0))
        available.Sort(Function(a, b) a.FirstAppearanceIndex.CompareTo(b.FirstAppearanceIndex))
        Dim topological As New List(Of WorkingNode)(context.Nodes.Count)

        While available.Count > 0
            Dim node As WorkingNode = available(0)
            available.RemoveAt(0)
            topological.Add(node)

            For Each link As WorkingLink In node.Outgoing.OrderBy(Function(l) l.FirstAppearanceIndex)
                Dim target As WorkingNode = link.Target
                target.StageIndex = Math.Max(target.StageIndex, node.StageIndex + 1)
                indegree(target.Id) -= 1
                If indegree(target.Id) = 0 Then
                    available.Add(target)
                    available.Sort(Function(a, b) a.FirstAppearanceIndex.CompareTo(b.FirstAppearanceIndex))
                End If
            Next
        End While

        If topological.Count <> context.Nodes.Count Then
            Throw New ArgumentException("Sankey links contain a directed cycle. The layered Sankey renderer requires an acyclic flow graph.")
        End If

        'Right-align nodes within the slack allowed by their already-positioned successors.
        'Example: A->C->F establishes F at stage 2. An independent X->F source can then
        'move from stage 0 to stage 1 instead of being forced into the left-most column.
        For i As Integer = topological.Count - 1 To 0 Step -1
            Dim node As WorkingNode = topological(i)
            If node.Outgoing.Count = 0 Then Continue For

            Dim maximumAllowedStage As Integer = Integer.MaxValue
            For Each link As WorkingLink In node.Outgoing
                maximumAllowedStage = Math.Min(maximumAllowedStage, link.Target.StageIndex - 1)
            Next
            If maximumAllowedStage > node.StageIndex Then node.StageIndex = maximumAllowedStage
        Next

        For Each link As WorkingLink In context.Links
            If link.Target.StageIndex <= link.Source.StageIndex Then
                Throw New InvalidOperationException("Automatic Sankey stage inference produced a non-forward link.")
            End If
        Next

        Return context.Nodes.Max(Function(n) n.StageIndex) + 1
    End Function

    Private Shared Sub ComputeNodeWeights(context As BuildContext)
        For Each node As WorkingNode In context.Nodes
            node.IncomingWeight = node.Incoming.Sum(Function(l) l.Weight)
            node.OutgoingWeight = node.Outgoing.Sum(Function(l) l.Weight)
            node.Value = Math.Max(node.IncomingWeight, node.OutgoingWeight)
            If Not IsFinitePositive(node.Value) Then
                Throw New InvalidOperationException("Sankey node '" & node.Label & "' has no positive flow weight.")
            End If
        Next
    End Sub

    Private Shared Function BuildStageNodeLists(context As BuildContext,
                                                stageCount As Integer) As List(Of WorkingNode)()
        If stageCount < 2 Then
            Throw New ArgumentException("A Sankey chart requires at least two stages.")
        End If

        Dim result(stageCount - 1) As List(Of WorkingNode)
        For i As Integer = 0 To stageCount - 1
            result(i) = New List(Of WorkingNode)()
        Next

        For Each node As WorkingNode In context.Nodes
            If node.StageIndex < 0 OrElse node.StageIndex >= stageCount Then
                Throw New InvalidOperationException("A Sankey node resolved outside the available stage range.")
            End If
            result(node.StageIndex).Add(node)
        Next

        Return result
    End Function

    Private Shared Sub OrderNodes(stageNodes As List(Of WorkingNode)(),
                                  links As List(Of WorkingLink),
                                  options As SankeyPlotOptions)
        For stage As Integer = 0 To stageNodes.Length - 1
            Select Case options.NodeOrder
                Case SankeyNodeOrderMode.FirstAppearance, SankeyNodeOrderMode.Automatic
                    stageNodes(stage).Sort(Function(a, b) a.FirstAppearanceIndex.CompareTo(b.FirstAppearanceIndex))
                Case SankeyNodeOrderMode.Alphabetical
                    stageNodes(stage).Sort(Function(a, b)
                                               Dim c As Integer = StringComparer.CurrentCultureIgnoreCase.Compare(a.Label, b.Label)
                                               If c <> 0 Then Return c
                                               Return a.FirstAppearanceIndex.CompareTo(b.FirstAppearanceIndex)
                                           End Function)
                Case SankeyNodeOrderMode.WeightDescending
                    stageNodes(stage).Sort(Function(a, b)
                                               Dim c As Integer = b.Value.CompareTo(a.Value)
                                               If c <> 0 Then Return c
                                               Return a.FirstAppearanceIndex.CompareTo(b.FirstAppearanceIndex)
                                           End Function)
            End Select
            AssignStageOrder(stageNodes(stage))
        Next

        If options.NodeOrder <> SankeyNodeOrderMode.Automatic Then Return

        For pass As Integer = 1 To options.CrossingReductionPasses
            For stage As Integer = 1 To stageNodes.Length - 1
                ReorderStageByBarycenter(stageNodes(stage), useIncoming:=True)
            Next
            For stage As Integer = stageNodes.Length - 2 To 0 Step -1
                ReorderStageByBarycenter(stageNodes(stage), useIncoming:=False)
            Next
        Next
    End Sub

    Private Shared Sub ReorderStageByBarycenter(nodes As List(Of WorkingNode),
                                                 useIncoming As Boolean)
        If nodes.Count <= 1 Then
            AssignStageOrder(nodes)
            Return
        End If

        Dim oldOrder As New Dictionary(Of String, Integer)(StringComparer.Ordinal)
        For Each node As WorkingNode In nodes
            oldOrder(node.Id) = node.OrderInStage
        Next

        nodes.Sort(Function(a, b)
                       Dim aHas As Boolean
                       Dim bHas As Boolean
                       Dim aBary As Double = WeightedNeighborBarycenter(a, useIncoming, aHas)
                       Dim bBary As Double = WeightedNeighborBarycenter(b, useIncoming, bHas)

                       If aHas AndAlso bHas Then
                           Dim c As Integer = aBary.CompareTo(bBary)
                           If c <> 0 Then Return c
                       ElseIf aHas <> bHas Then
                           'Keep unconnected-to-this-direction nodes stable rather than
                           'forcing them to an arbitrary chart edge.
                           Return oldOrder(a.Id).CompareTo(oldOrder(b.Id))
                       End If

                       Return oldOrder(a.Id).CompareTo(oldOrder(b.Id))
                   End Function)
        AssignStageOrder(nodes)
    End Sub

    Private Shared Function WeightedNeighborBarycenter(node As WorkingNode,
                                                        useIncoming As Boolean,
                                                        ByRef hasNeighbors As Boolean) As Double
        Dim relevant As IEnumerable(Of WorkingLink) = If(useIncoming, node.Incoming, node.Outgoing)
        Dim weightedSum As Double = 0.0R
        Dim totalWeight As Double = 0.0R

        For Each link As WorkingLink In relevant
            Dim neighbor As WorkingNode = If(useIncoming, link.Source, link.Target)
            weightedSum += link.Weight * CDbl(neighbor.OrderInStage)
            totalWeight += link.Weight
        Next

        hasNeighbors = totalWeight > 0.0R
        If Not hasNeighbors Then Return 0.0R
        Return weightedSum / totalWeight
    End Function

    Private Shared Sub AssignStageOrder(nodes As List(Of WorkingNode))
        For i As Integer = 0 To nodes.Count - 1
            nodes(i).OrderInStage = i
        Next
    End Sub

    Private Shared Function ComputeNormalizedLayout(stageNodes As List(Of WorkingNode)(),
                                                    ByRef stageTotals As Double(),
                                                    options As SankeyPlotOptions) As Double
        Dim stageCount As Integer = stageNodes.Length
        Dim globalWeightScale As Double = Double.PositiveInfinity

        For stage As Integer = 0 To stageCount - 1
            Dim nodes As List(Of WorkingNode) = stageNodes(stage)
            Dim total As Double = nodes.Sum(Function(n) n.Value)
            stageTotals(stage) = total
            If nodes.Count = 0 OrElse total <= 0.0R Then Continue For

            Dim totalGap As Double = options.NodeGapFraction * CDbl(Math.Max(0, nodes.Count - 1))
            Dim availableForNodes As Double = 1.0R - totalGap
            If availableForNodes <= 0.0R Then
                Throw New ArgumentException("NodeGapFraction is too large for the number of nodes in stage " &
                                            (stage + 1).ToString(CultureInfo.CurrentCulture) & ".")
            End If
            globalWeightScale = Math.Min(globalWeightScale, availableForNodes / total)
        Next

        If Double.IsInfinity(globalWeightScale) OrElse Not IsFinitePositive(globalWeightScale) Then
            Throw New InvalidOperationException("Unable to calculate a positive Sankey weight scale.")
        End If

        For stage As Integer = 0 To stageCount - 1
            Dim nodes As List(Of WorkingNode) = stageNodes(stage)
            If nodes.Count = 0 Then Continue For

            Dim usedHeight As Double = nodes.Sum(Function(n) n.Value * globalWeightScale) +
                                       options.NodeGapFraction * CDbl(Math.Max(0, nodes.Count - 1))
            Dim startY As Double
            Select Case options.VerticalAlignment
                Case SankeyVerticalAlignment.Top
                    startY = 0.0R
                Case SankeyVerticalAlignment.Center
                    startY = Math.Max(0.0R, (1.0R - usedHeight) / 2.0R)
                Case SankeyVerticalAlignment.Bottom
                    startY = Math.Max(0.0R, 1.0R - usedHeight)
                Case Else
                    Throw New ArgumentOutOfRangeException(NameOf(options.VerticalAlignment))
            End Select

            Dim currentY As Double = startY
            For Each node As WorkingNode In nodes.OrderBy(Function(n) n.OrderInStage)
                node.X = If(stageCount = 1, 0.5R, CDbl(stage) / CDbl(stageCount - 1))
                node.Y = currentY
                node.Height = node.Value * globalWeightScale
                currentY += node.Height + options.NodeGapFraction
            Next
        Next

        Return globalWeightScale
    End Function

    Private Shared Sub ComputeLinkOffsets(links As List(Of WorkingLink),
                                          weightScale As Double)
        Dim allNodes As WorkingNode() = links _
            .SelectMany(Function(l) New WorkingNode() {l.Source, l.Target}) _
            .Distinct().ToArray()

        For Each node As WorkingNode In allNodes
            Dim outgoingOrdered As WorkingLink() = node.Outgoing _
                .OrderBy(Function(l) l.Target.StageIndex) _
                .ThenBy(Function(l) l.Target.OrderInStage) _
                .ThenBy(Function(l) l.FirstAppearanceIndex).ToArray()
            Dim outgoingHeight As Double = node.OutgoingWeight * weightScale
            Dim outgoingY As Double = node.Y + Math.Max(0.0R, (node.Height - outgoingHeight) / 2.0R)
            For Each link As WorkingLink In outgoingOrdered
                link.Thickness = link.Weight * weightScale
                link.SourceTop = outgoingY
                outgoingY += link.Thickness
            Next

            Dim incomingOrdered As WorkingLink() = node.Incoming _
                .OrderBy(Function(l) l.Source.StageIndex) _
                .ThenBy(Function(l) l.Source.OrderInStage) _
                .ThenBy(Function(l) l.FirstAppearanceIndex).ToArray()
            Dim incomingHeight As Double = node.IncomingWeight * weightScale
            Dim incomingY As Double = node.Y + Math.Max(0.0R, (node.Height - incomingHeight) / 2.0R)
            For Each link As WorkingLink In incomingOrdered
                link.Thickness = link.Weight * weightScale
                link.TargetTop = incomingY
                incomingY += link.Thickness
            Next
        Next
    End Sub

    Private Shared Sub ValidateOptions(options As SankeyPlotOptions)
        If Not [Enum].IsDefined(GetType(SankeyNodeOrderMode), options.NodeOrder) Then
            Throw New ArgumentOutOfRangeException(NameOf(options.NodeOrder), "The Sankey node-order mode is not defined.")
        End If
        If Not [Enum].IsDefined(GetType(SankeyVerticalAlignment), options.VerticalAlignment) Then
            Throw New ArgumentOutOfRangeException(NameOf(options.VerticalAlignment), "The Sankey vertical alignment is not defined.")
        End If
        If Not IsFinite(options.NodeGapFraction) OrElse options.NodeGapFraction < 0.0R OrElse options.NodeGapFraction >= 0.5R Then
            Throw New ArgumentOutOfRangeException(NameOf(options.NodeGapFraction),
                                                  "NodeGapFraction must be finite and in the interval [0, 0.5).")
        End If
        If options.CrossingReductionPasses < 1 OrElse options.CrossingReductionPasses > 50 Then
            Throw New ArgumentOutOfRangeException(NameOf(options.CrossingReductionPasses),
                                                  "CrossingReductionPasses must be between 1 and 50.")
        End If
    End Sub

    Private Shared Sub ValidateWeight(weight As Double, rowIndex As Integer)
        If Double.IsInfinity(weight) Then
            Throw New ArgumentOutOfRangeException("weights",
                                                  "Weight at row " &
                                                  (rowIndex + 1).ToString(CultureInfo.CurrentCulture) &
                                                  " is infinite.")
        End If
        If weight < 0.0R Then
            Throw New ArgumentOutOfRangeException("weights",
                                                  "Weight at row " &
                                                  (rowIndex + 1).ToString(CultureInfo.CurrentCulture) &
                                                  " is negative. Sankey weights must be non-negative.")
        End If
    End Sub

    Private Shared Sub ValidateStageNames(stageNames As String(), stageCount As Integer)
        If stageNames Is Nothing Then Return
        If stageNames.Length <> stageCount Then
            Throw New ArgumentException("StageNames must contain exactly " &
                                        stageCount.ToString(CultureInfo.CurrentCulture) &
                                        " entries.", NameOf(stageNames))
        End If
    End Sub

    Private Shared Function ResolveStageNames(stageNames As String(), stageCount As Integer) As String()
        If stageNames IsNot Nothing Then
            Return stageNames.Select(Function(s) If(s, String.Empty).Trim()).ToArray()
        End If
        Dim result(stageCount - 1) As String
        For i As Integer = 0 To result.Length - 1
            result(i) = String.Empty
        Next
        Return result
    End Function

    Private Shared Function IsMissingCategoryValue(value As Object) As Boolean
        If value Is Nothing OrElse Convert.IsDBNull(value) Then Return True
        If TypeOf value Is String Then Return String.IsNullOrWhiteSpace(CStr(value))
        Return False
    End Function

    Private Shared Function BuildCategoryKey(value As Object) As String
        If IsNumeric(value) Then
            Dim numericValue As Double = Convert.ToDouble(value, CultureInfo.InvariantCulture)
            If Double.IsNaN(numericValue) OrElse Double.IsInfinity(numericValue) Then
                Throw New ArgumentException("Numeric Sankey categories must be finite.")
            End If
            Return "N:" & numericValue.ToString("R", CultureInfo.InvariantCulture)
        End If
        Return "S:" & Convert.ToString(value, CultureInfo.InvariantCulture).Trim()
    End Function

    Private Shared Function FormatCategoryName(value As Object) As String
        If IsNumeric(value) Then
            Return Convert.ToDouble(value, CultureInfo.CurrentCulture).ToString(CultureInfo.CurrentCulture)
        End If
        Return Convert.ToString(value, CultureInfo.CurrentCulture).Trim()
    End Function

    Private Shared Function IsFinite(value As Double) As Boolean
        Return Not Double.IsNaN(value) AndAlso Not Double.IsInfinity(value)
    End Function

    Private Shared Function IsFinitePositive(value As Double) As Boolean
        Return IsFinite(value) AndAlso value > 0.0R
    End Function
End Class
