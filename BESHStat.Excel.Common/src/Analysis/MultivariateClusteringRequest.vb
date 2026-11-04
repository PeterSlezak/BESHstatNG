Option Explicit On
Option Strict On
Imports System
Imports BESHStatNG.Multivariate

Namespace ExcelCommon
    Public NotInheritable Class MultivariateClusterDataSection
        Public Sub New(input As MultivariateInputIntent)
            Dim labels = input.RowLabels
            IncludesRowLabel = labels IsNot Nothing AndAlso labels.Length = input.RowCount
            Shape = New MultivariateArrayShape(input.RowCount + 1, input.ColumnCount + If(IncludesRowLabel, 2, 1))
        End Sub
        Public ReadOnly Property SheetName As String = "Data"
        Public ReadOnly Property HeaderRow As Integer = 1
        Public ReadOnly Property BodyRow As Integer = 2
        Public ReadOnly Property FirstColumn As Integer = 1
        Public ReadOnly Property IncludesRowLabel As Boolean
        Public ReadOnly Property Shape As MultivariateArrayShape
    End Class
    ''' <summary>Provenance only: the Windows host has already imported original-scale
    ''' numeric centres in selected-variable order. No range, values or transform is stored.</summary>
    Public NotInheritable Class MultivariateStartingCentersIntent
        Public Sub New(rows As Integer, columns As Integer)
            Shape = New MultivariateArrayShape(rows, columns)
        End Sub
        Public ReadOnly Property Shape As MultivariateArrayShape
        Public ReadOnly Property Source As String = "HostImportedOriginalScale"
    End Class

    ''' <summary>Detached pre-fit intent. Configuring never fits or resolves a seed.
    ''' Existing mutable options remain compatible; every exposed options value is a copy.</summary>
    Public NotInheritable Class MultivariateKMeansRequest
        Private ReadOnly _options As MultivariateKMeansOptions
        Public Sub New(input As MultivariateInputIntent, options As MultivariateKMeansOptions,
                       Optional startingCenters As MultivariateStartingCentersIntent = Nothing)
            Me.Input = input : _options = options.Copy() : Me.StartingCenters = startingCenters
            DataSection = New MultivariateClusterDataSection(input)
        End Sub
        Public ReadOnly Property Input As MultivariateInputIntent
        Public ReadOnly Property StartingCenters As MultivariateStartingCentersIntent
        Public ReadOnly Property DataSection As MultivariateClusterDataSection
        Public ReadOnly Property Options As MultivariateKMeansOptions
            Get
                Return _options.Copy()
            End Get
        End Property
        Public ReadOnly Property HasExplicitSeed As Boolean
            Get
                Return _options.Seed <> Integer.MinValue
            End Get
        End Property
        Public ReadOnly Property Objective As String = "WithinClusterSquaredEuclideanSSE"
        Public Sub Configure(model As KMeans)
            _options.Configure(model)
        End Sub
        ''' <summary>Immediate synchronous raw-cell borrowing. No matrix is retained or
        ''' converted; the established Data-sheet contract owns all cell types/order.</summary>
        Public Function DataTable(original(,) As Object) As Object(,)
            Return MultivariateWorkflowPolicy.ClusterInputTable(original, Input.VariableNames, Input.SourceRowIds, Input.RowLabels)
        End Function
    End Class

    ''' <summary>Fit choices only. Cut and display choices are captured at their original
    ''' host parsing boundaries, so Core validation/Windows exception precedence stays intact.</summary>
    Public NotInheritable Class MultivariateHierarchicalRequest
        Private ReadOnly _options As MultivariateHierarchicalOptions
        Public Sub New(input As MultivariateInputIntent, options As MultivariateHierarchicalOptions)
            Me.Input = input : _options = options.Copy()
            DataSection = New MultivariateClusterDataSection(input)
        End Sub
        Public ReadOnly Property Input As MultivariateInputIntent
        Public ReadOnly Property DataSection As MultivariateClusterDataSection
        Public ReadOnly Property Options As MultivariateHierarchicalOptions
            Get
                Return _options.Copy()
            End Get
        End Property
        Public Sub Configure(model As HierarchicalClustering)
            _options.ConfigureModel(model)
        End Sub
        Public Function DataTable(original(,) As Object) As Object(,)
            Return MultivariateWorkflowPolicy.ClusterInputTable(original, Input.VariableNames, Input.SourceRowIds, Input.RowLabels)
        End Function
    End Class

    Public NotInheritable Class MultivariateClusterCutRequest
        Public Sub New(mode As HierarchicalMembershipDisplayMode, clusters As Integer, height As Double)
            Me.Mode = mode : Me.Clusters = clusters : Me.Height = height
        End Sub
        Public ReadOnly Property Mode As HierarchicalMembershipDisplayMode
        Public ReadOnly Property Clusters As Integer
        Public ReadOnly Property Height As Double
        Public Sub Configure(model As HierarchicalClustering)
            model.reportInputs(Mode, Clusters, Height)
        End Sub
        Public Function Membership(result As HierarchicalClusterResult) As Integer()
            Return If(Mode = HierarchicalMembershipDisplayMode.ByHeight, result.GetMembershipByHeight(Height), result.GetMembershipByClusterCount(Clusters))
        End Function
    End Class

    ''' <summary>A semantic render request, never chart coordinates. Core builds layout;
    ''' the native renderer owns COM, style and lifetime. Construct only when plotting is on.</summary>
    Public NotInheritable Class MultivariateDendrogramRequest
        Public Sub New(heightMode As DendrogramHeightMode, orientation As DendrogramOrientation,
                       labelMode As DendrogramLabelMode, cut As MultivariateClusterCutRequest)
            Me.HeightMode = heightMode : Me.Orientation = orientation : Me.LabelMode = labelMode : Me.Cut = cut
            Me.Plot = MultivariateReportPolicy.PlotPlan(MultivariateAnalysisKind.Hierarchical, True)(0)
        End Sub
        Public ReadOnly Property HeightMode As DendrogramHeightMode
        Public ReadOnly Property Orientation As DendrogramOrientation
        Public ReadOnly Property LabelMode As DendrogramLabelMode
        Public ReadOnly Property Cut As MultivariateClusterCutRequest
        Public ReadOnly Property Plot As MultivariatePlotIntent
        Public ReadOnly Property SheetName As String = "Dendrogram"
        Public ReadOnly Property Anchor As String = "A1"
        Public ReadOnly Property Width As Double = 680.0
        Public ReadOnly Property Height As Double = 420.0
        Public ReadOnly Property Title As String = "Hierarchical Clustering Dendrogram"
        Public Function CreateLayout(result As HierarchicalClusterResult) As DendrogramLayout
            Return result.CreateDendrogramLayout(HeightMode, Orientation, Cut.Mode, Cut.Clusters, Cut.Height)
        End Function
    End Class
End Namespace
