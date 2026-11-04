Option Explicit On
Option Strict On
Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports BESHStatNG.Multivariate

Namespace ExcelCommon
    ''' <summary>All ordinals are explicitly one-based. A source RowId is never a
    ''' Core input ordinal, active leaf ID or internal merge ID. Missing axes are Nothing.</summary>
    Public NotInheritable Class MultivariateClusterObservation
        Friend Sub New(inputOrdinal As Integer, sourceRowId As Integer?, activeOrdinal As Integer?,
                       removedOrdinal As Integer?, sourceLabel As String, displayLabel As String,
                       membership As Integer?, leafId As Integer?, leafPosition As Integer?)
            Me.InputOrdinal = inputOrdinal : Me.SourceRowId = sourceRowId : Me.ActiveOrdinal = activeOrdinal
            Me.RemovedOrdinal = removedOrdinal : Me.SourceLabel = sourceLabel : Me.DisplayLabel = displayLabel
            Me.Membership = membership : Me.LeafId = leafId : Me.LeafPosition = leafPosition
        End Sub
        Public ReadOnly Property InputOrdinal As Integer
        Public ReadOnly Property SourceRowId As Integer?
        Public ReadOnly Property ActiveOrdinal As Integer?
        Public ReadOnly Property RemovedOrdinal As Integer?
        Public ReadOnly Property SourceLabel As String
        Public ReadOnly Property DisplayLabel As String
        Public ReadOnly Property Membership As Integer?
        Public ReadOnly Property LeafId As Integer?
        Public ReadOnly Property LeafPosition As Integer?
    End Class

    Public NotInheritable Class MultivariateClusterMerge
        Friend Sub New(stepNumber As Integer, internalId As Integer, left As Integer, right As Integer, size As Integer, height As Double)
            Me.StepNumber = stepNumber : Me.InternalId = internalId : Me.LeftId = left : Me.RightId = right
            Me.Size = size : Me.Height = height
        End Sub
        Public ReadOnly Property StepNumber As Integer
        Public ReadOnly Property InternalId As Integer
        Public ReadOnly Property LeftId As Integer
        Public ReadOnly Property RightId As Integer
        Public ReadOnly Property Size As Integer
        Public ReadOnly Property Height As Double
    End Class

    ''' <summary>One ordered existing wrapResults section. Report remains the sole
    ''' presentation/rehydration owner, including types, captions, footnotes and formats.</summary>
    Public NotInheritable Class MultivariateClusterSection
        Friend Sub New(table As AnalysisTableArtifact)
            Index = table.Index : Shape = New MultivariateArrayShape(table.RowCount, table.ColumnCount)
            Dim model = table.Model
            HeaderDepth = model.HeaderTopRows : TitleDepth = model.TitleRows : FooterDepth = model.FooterRows
            Title = If(model.TitleRows > 0 AndAlso model.RowCount > 0 AndAlso model.ColumnCount > 0, TryCast(model.Values(0, 0), String), Nothing)
        End Sub
        Public ReadOnly Property Index As Integer
        Public ReadOnly Property Title As String
        Public ReadOnly Property Shape As MultivariateArrayShape
        Public ReadOnly Property HeaderDepth As Integer
        Public ReadOnly Property TitleDepth As Integer
        Public ReadOnly Property FooterDepth As Integer
    End Class

    ''' <summary>Post-fit metadata only; no mutable Core model/arrays or raw N x P matrix
    ''' is retained. Membership and merge/leaf order are copied from actual Core outcomes.
    ''' This never changes legacy OriginalRow cells to physical source RowIds.</summary>
    Public NotInheritable Class MultivariateClusteringProjection
        Friend Sub New(input As MultivariateInputIntent, report As MultivariateReport,
                       active() As Integer, activeLabels() As String, removed() As Integer, removedLabels() As String,
                       membership() As Integer, leafOrder() As Integer, variables() As String,
                       centers As MultivariateArrayShape, merges As IEnumerable(Of MultivariateClusterMerge),
                       Optional seed As Integer? = Nothing, Optional starts As Integer? = Nothing,
                       Optional converged As Boolean? = Nothing, Optional iterations As Integer? = Nothing,
                       Optional requestedClusters As Integer? = Nothing, Optional workingCenters As MultivariateArrayShape = Nothing)
            Me.Input = input : Me.Report = report : Me.CenterShape = centers
            Me.EffectiveSeed = seed : Me.StartsEvaluated = starts : Me.Converged = converged : Me.Iterations = iterations
            Me.RequestedClusterCount = requestedClusters : Me.WorkingCenterShape = workingCenters
            Me.Centers = If(centers Is Nothing, New List(Of MultivariateAxisItem),
                Enumerable.Range(0, centers.Rows).Select(Function(i) New MultivariateAxisItem(i, "Cluster " & (i + 1).ToString(Globalization.CultureInfo.CurrentCulture))).ToList()).AsReadOnly()
            Me.Merges = New List(Of MultivariateClusterMerge)(merges).AsReadOnly()
            Me.Sections = report.Document.Tables.Select(Function(t) New MultivariateClusterSection(t)).ToList().AsReadOnly()
            Dim columns = input.Columns
            Me.Variables = variables.Select(Function(name, j) New MultivariateAxisItem(j, name,
                sourcePosition:=j, sourceKey:=If(j < columns.Count, columns(j).SourceKey, Nothing))).ToList().AsReadOnly()
            Dim rows = input.SourceRowIds, labels = input.RowLabels
            Dim activePositions As New Dictionary(Of Integer, Integer), removedPositions As New Dictionary(Of Integer, Integer), leafPositions As New Dictionary(Of Integer, Integer)
            For j = 0 To active.Length - 1 : activePositions(active(j)) = j : Next
            For j = 0 To removed.Length - 1 : removedPositions(removed(j)) = j : Next
            If leafOrder IsNot Nothing Then
                For j = 0 To leafOrder.Length - 1 : leafPositions(leafOrder(j)) = j : Next
            End If
            Dim observations As New List(Of MultivariateClusterObservation)
            For i = 1 To input.RowCount
                Dim a = If(activePositions.ContainsKey(i), activePositions(i), -1), r = If(removedPositions.ContainsKey(i), removedPositions(i), -1)
                Dim display = If(a >= 0, activeLabels(a), If(r >= 0, removedLabels(r), Nothing))
                Dim position = If(leafPositions.ContainsKey(i), leafPositions(i), -1)
                observations.Add(New MultivariateClusterObservation(i,
                    If(rows IsNot Nothing AndAlso i <= rows.Length, CType(rows(i - 1), Integer?), Nothing),
                    If(a >= 0, CType(a + 1, Integer?), Nothing), If(r >= 0, CType(r + 1, Integer?), Nothing),
                    If(labels IsNot Nothing AndAlso i <= labels.Length, labels(i - 1), Nothing), display,
                    If(a >= 0 AndAlso a < membership.Length, CType(membership(a), Integer?), Nothing),
                    If(leafOrder IsNot Nothing AndAlso a >= 0, CType(a + 1, Integer?), Nothing),
                    If(position >= 0, CType(position + 1, Integer?), Nothing)))
            Next
            Me.Observations = observations.AsReadOnly()
            Dim diagnostics As New List(Of RegressionDiagnostic)
            If observations.Any(Function(o) o.SourceRowId.HasValue AndAlso o.SourceRowId.Value <> o.InputOrdinal) Then
                diagnostics.Add(New RegressionDiagnostic("C19-CLUSTER-ROWID-DISPLAY-01", "rows", "Data OriginalRow uses source RowIds; legacy result OriginalRow uses Core input ordinals. No output correction is activated.", blocking:=False))
            End If
            If activeLabels.Distinct(StringComparer.Ordinal).Count() <> activeLabels.Length Then
                diagnostics.Add(New RegressionDiagnostic("clustering.labels.duplicate", "labels", "Duplicate display labels are preserved as labels, never identities.", blocking:=False))
            End If
            Validation = New RegressionValidation(diagnostics.Take(32), 0, 0, {})
        End Sub
        Public ReadOnly Property Input As MultivariateInputIntent
        Public ReadOnly Property Report As MultivariateReport
        Public ReadOnly Property Observations As IReadOnlyList(Of MultivariateClusterObservation)
        Public ReadOnly Property Variables As IReadOnlyList(Of MultivariateAxisItem)
        Public ReadOnly Property Merges As IReadOnlyList(Of MultivariateClusterMerge)
        Public ReadOnly Property Sections As IReadOnlyList(Of MultivariateClusterSection)
        Public ReadOnly Property CenterShape As MultivariateArrayShape
        Public ReadOnly Property WorkingCenterShape As MultivariateArrayShape
        Public ReadOnly Property RequestedClusterCount As Integer?
        Public ReadOnly Property Centers As IReadOnlyList(Of MultivariateAxisItem)
        Public ReadOnly Property EffectiveSeed As Integer?
        Public ReadOnly Property StartsEvaluated As Integer?
        Public ReadOnly Property Converged As Boolean?
        Public ReadOnly Property Iterations As Integer?
        Public ReadOnly Property Validation As RegressionValidation
        Public ReadOnly Property ResultsSheetName As String
            Get
                Return If(Input.Kind = MultivariateAnalysisKind.KMeans, "KMeans results", "Hierarchical results")
            End Get
        End Property
        Public Function LegacyTables() As List(Of ResultTable)
            Return Report.LegacyTables()
        End Function
    End Class

    Public NotInheritable Class MultivariateClusteringProjectionPolicy
        Private Sub New()
        End Sub
        Public Shared Function KMeans(request As MultivariateKMeansRequest, result As KMeansClusterResult,
                                      tables As IEnumerable(Of ResultTable)) As MultivariateClusteringProjection
            Return New MultivariateClusteringProjection(request.Input, MultivariateReportPolicy.Project(request.Input, tables),
                result.ActiveRowIndices, result.ActiveRowLabels, result.RemovedRowIndices, result.RemovedRowLabels,
                result.ClusterAssignments, Nothing, result.VariableNames, MultivariateArrayShape.Matrix(result.CentersOriginalScale),
                Array.Empty(Of MultivariateClusterMerge)(), result.RandomSeedUsed, result.StartsEvaluated, result.Converged, result.Iterations,
                request.Options.Clusters, MultivariateArrayShape.Matrix(result.CentersWorkingScale))
        End Function
        Public Shared Function Hierarchical(request As MultivariateHierarchicalRequest, cut As MultivariateClusterCutRequest,
                                            result As HierarchicalClusterResult, tables As IEnumerable(Of ResultTable)) As MultivariateClusteringProjection
            Dim merges As New List(Of MultivariateClusterMerge)
            For j = 0 To result.MergeHeights.Length - 1
                merges.Add(New MultivariateClusterMerge(j + 1, result.ActiveRowIndices.Length + j + 1,
                    result.MergeLeftClusterIds(j), result.MergeRightClusterIds(j), result.MergeClusterSizes(j), result.MergeHeights(j)))
            Next
            Return New MultivariateClusteringProjection(request.Input, MultivariateReportPolicy.Project(request.Input, tables),
                result.ActiveRowIndices, result.ActiveRowLabels, result.RemovedRowIndices, result.RemovedRowLabels,
                cut.Membership(result), result.LeafOrder, result.VariableNames, Nothing, merges)
        End Function
    End Class
End Namespace
