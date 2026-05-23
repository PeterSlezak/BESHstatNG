Option Explicit On
Option Strict On

Imports System.Collections.Generic
Imports System.Linq
Imports BESHStatNG.AppInfrastructure

Namespace Multivariate

    ''' <summary>
    ''' Specifies how numeric variables are rescaled before cluster distances are computed.
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' Standardization is applied column-by-column to the active analysis dataset after any row deletions
    ''' required by the missing-value policy.
    ''' </para>
    ''' <list type="bullet">
    '''   <item><description><see cref="None"/>: use the original measurement scale.</description></item>
    '''   <item><description><see cref="ZScores"/>: subtract the column mean and divide by the sample standard deviation.</description></item>
    '''   <item><description><see cref="RangeZeroToOne"/>: subtract the column minimum and divide by the observed range.</description></item>
    ''' </list>
    ''' <para>
    ''' Standardization changes the geometry of the clustering problem. In particular, z-score standardization
    ''' gives each variable equal variance in the working analysis scale and is often used when variables are
    ''' measured in different units.
    ''' </para>
    ''' </remarks>
    Public Enum ClusterStandardizationMode
        ''' <summary>
        ''' Analyze the supplied numeric values without any rescaling.
        ''' </summary>
        None = 0

        ''' <summary>
        ''' Standardize each variable to mean 0 and sample standard deviation 1.
        ''' </summary>
        ZScores = 1

        ''' <summary>
        ''' Rescale each variable to the unit interval [0, 1] using the observed minimum and maximum.
        ''' </summary>
        RangeZeroToOne = 2
    End Enum

    ''' <summary>
    ''' Specifies how rows containing missing or non-finite values are handled before clustering.
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' Missing values are recognized when a cell contains <see cref="Double.NaN"/> or an infinite numeric value.
    ''' </para>
    ''' <list type="bullet">
    '''   <item><description><see cref="ErrorOnMissing"/>: stop immediately and raise an exception if any row contains missing data.</description></item>
    '''   <item><description><see cref="ListwiseDeletion"/>: remove any row that contains one or more missing values before fitting the model.</description></item>
    ''' </list>
    ''' </remarks>
    Public Enum ClusterMissingValuePolicy
        ''' <summary>
        ''' Reject datasets that contain missing or non-finite values.
        ''' </summary>
        ErrorOnMissing = 0

        ''' <summary>
        ''' Remove entire rows that contain missing or non-finite values before clustering.
        ''' </summary>
        ListwiseDeletion = 1
    End Enum

    ''' <summary>
    ''' Specifies how initial centers are chosen for k-means clustering.
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' Good initialization matters because the k-means objective is non-convex and different starts may converge
    ''' to different local minima.
    ''' </para>
    ''' </remarks>
    Public Enum KMeansInitializationMethod
        ''' <summary>
        ''' Choose <c>k</c> distinct observations uniformly at random as the initial cluster centers.
        ''' </summary>
        Forgy = 0

        ''' <summary>
        ''' Randomly allocate observations to clusters and then compute the implied initial centers.
        ''' </summary>
        RandomPartition = 1

        ''' <summary>
        ''' Use the k-means++ seeding heuristic, which spreads initial centers apart using distance-weighted sampling.
        ''' </summary>
        KMeansPlusPlus = 2

        ''' <summary>
        ''' Use centers supplied earlier through <see cref="KMeans.startingCentersInputs(Double(,))"/>.
        ''' </summary>
        UserSpecifiedCenters = 3
    End Enum

    ''' <summary>
    ''' Specifies the point-to-center distance reported by the k-means model.
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' Classical k-means minimizes a sum of squared Euclidean distances. Euclidean and squared Euclidean distances
    ''' always induce the same cluster assignment because one is a monotone transformation of the other. This option
    ''' therefore mainly affects the reported distances rather than the fitted partition.
    ''' </para>
    ''' </remarks>
    Public Enum KMeansDistanceMetric
        ''' <summary>
        ''' Report ordinary Euclidean distances.
        ''' </summary>
        Euclidean = 0

        ''' <summary>
        ''' Report squared Euclidean distances.
        ''' </summary>
        SquaredEuclidean = 1
    End Enum

    ''' <summary>
    ''' Specifies how k-means handles a cluster that temporarily receives no observations during an iteration.
    ''' </summary>
    Public Enum EmptyClusterHandlingStrategy
        ''' <summary>
        ''' Keep the previous center for the empty cluster and continue iterating.
        ''' </summary>
        KeepPreviousCenter = 0

        ''' <summary>
        ''' Re-seed the empty cluster using a randomly chosen observation.
        ''' </summary>
        RandomObservation = 1

        ''' <summary>
        ''' Re-seed the empty cluster using the observation that is farthest from its currently assigned center.
        ''' </summary>
        FarthestObservation = 2
    End Enum

    ''' <summary>
    ''' Specifies the base dissimilarity used by agglomerative hierarchical clustering.
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' The selected distance is first computed between individual observations. Depending on the linkage method,
    ''' cluster-to-cluster dissimilarities are then updated either from these base distances or from cluster centroids.
    ''' </para>
    ''' <para>
    ''' Centroid, median, and Ward linkage require Euclidean geometry. This implementation therefore restricts them
    ''' to <see cref="Euclidean"/> and <see cref="SquaredEuclidean"/>.
    ''' </para>
    ''' </remarks>
    Public Enum HierarchicalDistanceMetric
        ''' <summary>
        ''' Euclidean distance.
        ''' </summary>
        Euclidean = 0

        ''' <summary>
        ''' Squared Euclidean distance.
        ''' </summary>
        SquaredEuclidean = 1

        ''' <summary>
        ''' Manhattan (city-block) distance.
        ''' </summary>
        Manhattan = 2

        ''' <summary>
        ''' Chebyshev distance, defined as the maximum absolute coordinate difference.
        ''' </summary>
        Chebyshev = 3

        ''' <summary>
        ''' Minkowski distance with user-specified power parameter <c>p</c>.
        ''' </summary>
        Minkowski = 4

        ''' <summary>
        ''' Cosine distance, defined as <c>1 - cosine similarity</c>.
        ''' </summary>
        Cosine = 5

        ''' <summary>
        ''' Correlation distance, defined as <c>1 - Pearson correlation</c>.
        ''' </summary>
        Correlation = 6
    End Enum

    ''' <summary>
    ''' Specifies the agglomeration rule used by hierarchical clustering.
    ''' </summary>
    Public Enum HierarchicalLinkageMethod
        ''' <summary>
        ''' Single linkage (nearest-neighbor): the smallest distance between any pair of points across two clusters.
        ''' </summary>
        SingleLinkage = 0

        ''' <summary>
        ''' Complete linkage (farthest-neighbor): the largest distance between any pair of points across two clusters.
        ''' </summary>
        Complete = 1

        ''' <summary>
        ''' Average linkage (UPGMA): the size-weighted average dissimilarity between clusters.
        ''' </summary>
        Average = 2

        ''' <summary>
        ''' Weighted average linkage (WPGMA): the simple average of the two predecessor cluster distances.
        ''' </summary>
        WeightedAverage = 3

        ''' <summary>
        ''' Centroid linkage (UPGMC): distance between cluster centroids.
        ''' </summary>
        Centroid = 4

        ''' <summary>
        ''' Median linkage (WPGMC): distance between recursively averaged cluster representatives.
        ''' </summary>
        Median = 5

        ''' <summary>
        ''' Ward minimum-variance linkage: merge the pair that produces the smallest increase in within-cluster sum of squares.
        ''' </summary>
        Ward = 6
    End Enum

    ''' <summary>
    ''' Specifies how the vertical scale of a dendrogram is computed.
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' <see cref="StepLevels"/> reproduces the chapter-8 teaching style from the supplied Excel text,
    ''' where the first merge is drawn at height 1, the second at height 2, and so on, regardless of the
    ''' actual numerical merge distances.
    ''' </para>
    ''' <para>
    ''' <see cref="MergeDistance"/> uses the fitted merge heights produced by the clustering algorithm.
    ''' This is the standard proportional dendrogram used by most statistical software.
    ''' </para>
    ''' </remarks>
    Public Enum DendrogramHeightMode
        ''' <summary>
        ''' Draw one level per merge step, regardless of the numerical merge distance.
        ''' </summary>
        StepLevels = 0

        ''' <summary>
        ''' Draw branch heights using the fitted merge distances.
        ''' </summary>
        MergeDistance = 1
    End Enum

    ''' <summary>
    ''' Specifies the orientation of a dendrogram in the final chart layout.
    ''' </summary>
    ''' <remarks>
    ''' <list type="bullet">
    '''   <item><description><see cref="Top"/>: leaves are arranged left-to-right and branches rise upward.</description></item>
    '''   <item><description><see cref="Bottom"/>: leaves are arranged left-to-right and branches hang downward.</description></item>
    '''   <item><description><see cref="Left"/>: leaves are arranged top-to-bottom and branches extend leftward.</description></item>
    '''   <item><description><see cref="Right"/>: leaves are arranged top-to-bottom and branches extend rightward.</description></item>
    ''' </list>
    ''' </remarks>
    Public Enum DendrogramOrientation
        ''' <summary>
        ''' Leaves are positioned on the lower side of the chart and the tree grows upward.
        ''' </summary>
        Top = 0

        ''' <summary>
        ''' Leaves are positioned on the upper side of the chart and the tree grows downward.
        ''' </summary>
        Bottom = 1

        ''' <summary>
        ''' Leaves are positioned on the right side of the chart and the tree grows leftward.
        ''' </summary>
        Left = 2

        ''' <summary>
        ''' Leaves are positioned on the left side of the chart and the tree grows rightward.
        ''' </summary>
        Right = 3
    End Enum

    ''' <summary>
    ''' Specifies how leaf labels are rendered in an Excel dendrogram chart.
    ''' </summary>
    Public Enum DendrogramLabelMode
        ''' <summary>
        ''' Do not render leaf labels automatically.
        ''' </summary>
        None = 0

        ''' <summary>
        ''' Render leaf labels as data labels attached to a separate label series.
        ''' </summary>
        DataLabels = 1

        ''' <summary>
        ''' Render leaf labels in the x-axis title as a single spaced text string.
        ''' </summary>
        AxisTitle = 2
    End Enum

    ''' <summary>
    ''' Specifies how the hierarchical cluster-membership table is produced for reporting.
    ''' </summary>
    Public Enum HierarchicalMembershipDisplayMode
        ''' <summary>
        ''' Cut the fitted tree to a requested number of clusters.
        ''' </summary>
        ByClusterCount = 0

        ''' <summary>
        ''' Cut the fitted tree at a requested merge-height threshold.
        ''' </summary>
        ByHeight = 1
    End Enum

    ''' <summary>
    ''' Stores the fitted output of a k-means clustering analysis.
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' The result object contains the fitted partition, cluster centers, distance summaries, and the preprocessing
    ''' metadata needed to interpret the solution. Rows removed by listwise deletion are reported separately and do not
    ''' appear in the active-row arrays.
    ''' </para>
    ''' <para>
    ''' Cluster labels are numbered from 1 to <c>k</c>.
    ''' </para>
    ''' </remarks>
    Public Class KMeansClusterResult

        Public Property RandomSeedUsed As Integer = Integer.MinValue

        ''' <summary>
        ''' Gets or sets the original 1-based row numbers retained in the clustering analysis.
        ''' </summary>
        Public Property ActiveRowIndices As Integer()

        ''' <summary>
        ''' Gets or sets the labels for the rows retained in the clustering analysis.
        ''' </summary>
        Public Property ActiveRowLabels As String()

        ''' <summary>
        ''' Gets or sets the original 1-based row numbers removed by listwise deletion.
        ''' </summary>
        Public Property RemovedRowIndices As Integer()

        ''' <summary>
        ''' Gets or sets the labels for rows removed by listwise deletion.
        ''' </summary>
        Public Property RemovedRowLabels As String()

        ''' <summary>
        ''' Gets or sets the variable names used in the clustering analysis.
        ''' </summary>
        Public Property VariableNames As String()

        ''' <summary>
        ''' Gets or sets the standardization mode used during fitting.
        ''' </summary>
        Public Property Standardization As ClusterStandardizationMode

        ''' <summary>
        ''' Gets or sets the missing-value policy used during fitting.
        ''' </summary>
        Public Property MissingValuePolicy As ClusterMissingValuePolicy

        ''' <summary>
        ''' Gets or sets the initialization method that produced the selected best solution.
        ''' </summary>
        Public Property InitializationMethod As KMeansInitializationMethod

        ''' <summary>
        ''' Gets or sets the distance metric used for reporting point-to-center distances.
        ''' </summary>
        Public Property DistanceMetric As KMeansDistanceMetric

        ''' <summary>
        ''' Gets or sets the number of clusters <c>k</c>.
        ''' </summary>
        Public Property NumberOfClusters As Integer

        ''' <summary>
        ''' Gets or sets the total number of random starts evaluated.
        ''' </summary>
        Public Property StartsEvaluated As Integer

        ''' <summary>
        ''' Gets or sets the number of iterations used by the selected best solution.
        ''' </summary>
        Public Property Iterations As Integer

        ''' <summary>
        ''' Gets or sets a value indicating whether the selected best solution met the convergence criterion.
        ''' </summary>
        Public Property Converged As Boolean

        ''' <summary>
        ''' Gets or sets the cluster assignment for each active observation.
        ''' </summary>
        Public Property ClusterAssignments As Integer()

        ''' <summary>
        ''' Gets or sets the size of each fitted cluster.
        ''' </summary>
        Public Property ClusterSizes As Integer()

        ''' <summary>
        ''' Gets or sets the fitted cluster centers on the working analysis scale.
        ''' </summary>
        Public Property CentersWorkingScale As Double(,)

        ''' <summary>
        ''' Gets or sets the fitted cluster centers back-transformed to the original variable scale.
        ''' </summary>
        Public Property CentersOriginalScale As Double(,)

        ''' <summary>
        ''' Gets or sets the analysis-scale grand mean of the active data.
        ''' </summary>
        Public Property GrandMeanWorkingScale As Double()

        ''' <summary>
        ''' Gets or sets the original-scale grand mean of the active data.
        ''' </summary>
        Public Property GrandMeanOriginalScale As Double()

        ''' <summary>
        ''' Gets or sets the distance from each active observation to its assigned cluster center.
        ''' </summary>
        Public Property DistanceToAssignedCenter As Double()

        ''' <summary>
        ''' Gets or sets the within-cluster sum of squares for each fitted cluster, computed on the working analysis scale.
        ''' </summary>
        Public Property WithinClusterSSByCluster As Double()

        ''' <summary>
        ''' Gets or sets the total within-cluster sum of squares, computed on the working analysis scale.
        ''' </summary>
        Public Property TotalWithinClusterSS As Double

        ''' <summary>
        ''' Gets or sets the between-cluster sum of squares, computed as total minus within-cluster SS on the working analysis scale.
        ''' </summary>
        Public Property BetweenClusterSS As Double

        ''' <summary>
        ''' Gets or sets the total sum of squares around the grand mean on the working analysis scale.
        ''' </summary>
        Public Property TotalSS As Double

        ''' <summary>
        ''' Gets or sets the objective function value of the selected best solution.
        ''' </summary>
        ''' <remarks>
        ''' For k-means this equals the total within-cluster sum of squared Euclidean distances on the working analysis scale.
        ''' </remarks>
        Public Property ObjectiveValue As Double

        ''' <summary>
        ''' Gets or sets the column locations used for preprocessing.
        ''' </summary>
        ''' <remarks>
        ''' For <see cref="ClusterStandardizationMode.ZScores"/> this stores variable means; for
        ''' <see cref="ClusterStandardizationMode.RangeZeroToOne"/> this stores variable minima; for
        ''' <see cref="ClusterStandardizationMode.None"/> the values are 0.
        ''' </remarks>
        Public Property StandardizationLocations As Double()

        ''' <summary>
        ''' Gets or sets the column scales used for preprocessing.
        ''' </summary>
        ''' <remarks>
        ''' For <see cref="ClusterStandardizationMode.ZScores"/> this stores sample standard deviations; for
        ''' <see cref="ClusterStandardizationMode.RangeZeroToOne"/> this stores variable ranges; for
        ''' <see cref="ClusterStandardizationMode.None"/> the values are 1.
        ''' </remarks>
        Public Property StandardizationScales As Double()

        ''' <summary>
        ''' Returns a row-wise table of active observations, their cluster labels, and their assigned-center distances.
        ''' </summary>
        ''' <returns>
        ''' A two-dimensional <see cref="Object"/> array with columns:
        ''' <c>OriginalRow</c>, <c>RowLabel</c>, <c>Cluster</c>, and <c>DistanceToAssignedCenter</c>.
        ''' </returns>
        Public Function GetObservationAssignmentsTable() As Object(,)
            Dim n As Integer = 0
            If ClusterAssignments IsNot Nothing Then n = ClusterAssignments.Length
            Dim out(Math.Max(n, 1), 3) As Object
            out(0, 0) = "OriginalRow"
            out(0, 1) = "RowLabel"
            out(0, 2) = "Cluster"
            out(0, 3) = "DistanceToAssignedCenter"

            For i As Integer = 0 To n - 1
                out(i + 1, 0) = If(ActiveRowIndices Is Nothing, CType(i + 1, Object), ActiveRowIndices(i))
                out(i + 1, 1) = If(ActiveRowLabels Is Nothing, CType($"Obs {i + 1}", Object), ActiveRowLabels(i))
                out(i + 1, 2) = ClusterAssignments(i)
                out(i + 1, 3) = If(DistanceToAssignedCenter Is Nothing, CType(Nothing, Object), DistanceToAssignedCenter(i))
            Next

            Return out
        End Function

        ''' <summary>
        ''' Returns a table of cluster centers together with cluster sizes and cluster-level within-cluster sums of squares.
        ''' </summary>
        ''' <param name="useOriginalScale">
        ''' If <c>True</c>, return centers on the original variable scale; otherwise return centers on the working analysis scale.
        ''' </param>
        ''' <returns>
        ''' A two-dimensional <see cref="Object"/> array containing one row per cluster.
        ''' </returns>
        Public Function GetCentersTable(Optional useOriginalScale As Boolean = True) As Object(,)
            Dim centers(,) As Double = If(useOriginalScale, CentersOriginalScale, CentersWorkingScale)
            If centers Is Nothing Then
                Dim emptyOut(0, 0) As Object
                emptyOut(0, 0) = "No centers available."
                Return emptyOut
            End If

            Dim k As Integer = centers.GetUpperBound(0) + 1
            Dim p As Integer = centers.GetUpperBound(1) + 1
            Dim out(k, p + 2) As Object

            out(0, 0) = "Cluster"
            out(0, 1) = "Size"
            out(0, 2) = "WithinClusterSS"
            For j As Integer = 0 To p - 1
                out(0, j + 3) = If(VariableNames Is Nothing, CType($"Var {j + 1}", Object), VariableNames(j))
            Next

            For i As Integer = 0 To k - 1
                out(i + 1, 0) = i + 1
                out(i + 1, 1) = If(ClusterSizes Is Nothing, CType(Nothing, Object), ClusterSizes(i))
                out(i + 1, 2) = If(WithinClusterSSByCluster Is Nothing, CType(Nothing, Object), WithinClusterSSByCluster(i))
                For j As Integer = 0 To p - 1
                    out(i + 1, j + 3) = centers(i, j)
                Next
            Next

            Return out
        End Function

        ''' <summary>
        ''' Returns a compact one-row summary of the fitted partition.
        ''' </summary>
        ''' <returns>
        ''' A two-dimensional <see cref="Object"/> array containing basic fit diagnostics.
        ''' </returns>
        Public Function GetSummaryTable() As Object(,)
            Dim out(1, 7) As Object
            out(0, 0) = "NumberOfClusters"
            out(0, 1) = "ActiveObservations"
            out(0, 2) = "RemovedObservations"
            out(0, 3) = "Iterations"
            out(0, 4) = "Converged"
            out(0, 5) = "TotalWithinClusterSS"
            out(0, 6) = "BetweenClusterSS"
            out(0, 7) = "TotalSS"

            out(1, 0) = NumberOfClusters
            out(1, 1) = If(ClusterAssignments Is Nothing, 0, ClusterAssignments.Length)
            out(1, 2) = If(RemovedRowIndices Is Nothing, 0, RemovedRowIndices.Length)
            out(1, 3) = Iterations
            out(1, 4) = Converged
            out(1, 5) = TotalWithinClusterSS
            out(1, 6) = BetweenClusterSS
            out(1, 7) = TotalSS
            Return out
        End Function
    End Class

    ''' <summary>
    ''' Performs classical k-means clustering for numeric data.
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' k-means partitions <c>n</c> observations into <c>k</c> clusters by minimizing the within-cluster sum of squared
    ''' Euclidean distances. This implementation supports multiple common initialization strategies, repeated random starts,
    ''' preprocessing by standardization, and explicit handling of temporarily empty clusters.
    ''' </para>
    ''' <para>
    ''' The fitted result is available through <see cref="Result"/> after calling <see cref="Fit"/>.
    ''' </para>
    ''' <para>
    ''' Mathematical criterion:
    ''' </para>
    ''' <code>
    ''' minimize   Σ_{g=1..k} Σ_{i in C_g} ||x_i - μ_g||²
    ''' </code>
    ''' <para>
    ''' where <c>μ_g</c> is the centroid (mean vector) of cluster <c>g</c>.
    ''' </para>
    ''' </remarks>
    Public Class KMeans

        Private pData(,) As Double
        Private pRowLabels() As String
        Private pVarNames() As String
        Private pUserStartingCenters(,) As Double
        Private pHasUserStartingCenters As Boolean

        Private pNumberOfClusters As Integer = 3
        Private pInitializationMethod As KMeansInitializationMethod = KMeansInitializationMethod.KMeansPlusPlus
        Private pDistanceMetric As KMeansDistanceMetric = KMeansDistanceMetric.SquaredEuclidean
        Private pStarts As Integer = 10
        Private pMaxIterations As Integer = 100
        Private pTolerance As Double = 0.000001
        Private pStandardization As ClusterStandardizationMode = ClusterStandardizationMode.None
        Private pMissingValuePolicy As ClusterMissingValuePolicy = ClusterMissingValuePolicy.ErrorOnMissing
        Private pEmptyClusterHandling As EmptyClusterHandlingStrategy = EmptyClusterHandlingStrategy.FarthestObservation
        Private pRandomSeed As Integer = Integer.MinValue
        Private pRandomSeedUsed As Integer = Integer.MinValue

        Private pResult As KMeansClusterResult

        ''' <summary>
        ''' Supplies the numeric data matrix and optional row and variable labels used by k-means.
        ''' </summary>
        ''' <param name="arData">Numeric input matrix with observations in rows and variables in columns.</param>
        ''' <param name="arRowLabels">Optional observation labels. When omitted, default labels <c>Obs 1</c>, <c>Obs 2</c>, ... are generated.</param>
        ''' <param name="arVarNames">Optional variable names. When omitted, default names <c>Var 1</c>, <c>Var 2</c>, ... are generated.</param>
        ''' <remarks>
        ''' This method stores the raw inputs only. Call <see cref="settingsInputs"/> to configure the algorithm and <see cref="Fit"/> to estimate the model.
        ''' </remarks>
        Public Sub dataInputs(arData(,) As Double,
                              Optional arRowLabels() As String = Nothing,
                              Optional arVarNames() As String = Nothing)
            pData = arData
            pRowLabels = arRowLabels
            pVarNames = arVarNames
            pResult = Nothing
        End Sub

        ''' <summary>
        ''' Supplies user-defined starting centers for k-means.
        ''' </summary>
        ''' <param name="arCenters">
        ''' Matrix of starting centers on the original variable scale. Rows correspond to clusters and columns correspond to variables.
        ''' </param>
        ''' <remarks>
        ''' <para>
        ''' To use these centers, set the initialization method to <see cref="KMeansInitializationMethod.UserSpecifiedCenters"/>
        ''' when calling <see cref="settingsInputs"/>.
        ''' </para>
        ''' <para>
        ''' The number of supplied centers must match the requested number of clusters and the number of columns must match the data matrix.
        ''' </para>
        ''' </remarks>
        Public Sub startingCentersInputs(arCenters(,) As Double)
            pUserStartingCenters = arCenters
            pHasUserStartingCenters = True
            pResult = Nothing
        End Sub

        ''' <summary>
        ''' Configures the k-means fitting procedure.
        ''' </summary>
        ''' <param name="numberOfClusters">Requested number of clusters <c>k</c>. Must be at least 1.</param>
        ''' <param name="initialization">Initialization strategy for the first centers.</param>
        ''' <param name="distanceMetric">Distance reported between each observation and its assigned center.</param>
        ''' <param name="nStarts">Number of random starts. Ignored for user-specified centers, which always use exactly one start.</param>
        ''' <param name="maxIterations">Maximum number of Lloyd/Forgy update iterations per start.</param>
        ''' <param name="convergenceTolerance">Convergence tolerance for center movement on the working analysis scale.</param>
        ''' <param name="standardization">Optional variable standardization applied before clustering.</param>
        ''' <param name="missingValuePolicy">Policy for rows that contain missing or non-finite numeric values.</param>
        ''' <param name="emptyClusterHandling">Strategy for handling a cluster that becomes empty during an iteration.</param>
        ''' <param name="randomSeed">
        ''' Optional deterministic seed for the pseudo-random number generator. Pass <see cref="Integer.MinValue"/> to use a time-based seed.
        ''' </param>
        Public Sub settingsInputs(Optional numberOfClusters As Integer = 3,
                                  Optional initialization As KMeansInitializationMethod = KMeansInitializationMethod.KMeansPlusPlus,
                                  Optional distanceMetric As KMeansDistanceMetric = KMeansDistanceMetric.SquaredEuclidean,
                                  Optional nStarts As Integer = 10,
                                  Optional maxIterations As Integer = 100,
                                  Optional convergenceTolerance As Double = 0.000001,
                                  Optional standardization As ClusterStandardizationMode = ClusterStandardizationMode.None,
                                  Optional missingValuePolicy As ClusterMissingValuePolicy = ClusterMissingValuePolicy.ErrorOnMissing,
                                  Optional emptyClusterHandling As EmptyClusterHandlingStrategy = EmptyClusterHandlingStrategy.FarthestObservation,
                                  Optional randomSeed As Integer = Integer.MinValue)

            If numberOfClusters < 1 Then CoreServices.Errors.LogAndThrow(New ArgumentException("numberOfClusters must be at least 1."))
            If nStarts < 1 Then CoreServices.Errors.LogAndThrow(New ArgumentException("nStarts must be at least 1."))
            If maxIterations < 1 Then CoreServices.Errors.LogAndThrow(New ArgumentException("maxIterations must be at least 1."))
            If convergenceTolerance < 0 Then CoreServices.Errors.LogAndThrow(New ArgumentException("convergenceTolerance must be non-negative."))

            pNumberOfClusters = numberOfClusters
            pInitializationMethod = initialization
            pDistanceMetric = distanceMetric
            pStarts = nStarts
            pMaxIterations = maxIterations
            pTolerance = convergenceTolerance
            pStandardization = standardization
            pMissingValuePolicy = missingValuePolicy
            pEmptyClusterHandling = emptyClusterHandling
            pRandomSeed = randomSeed
            pResult = Nothing
        End Sub

        ''' <summary>
        ''' Gets the fitted k-means result object.
        ''' </summary>
        ''' <remarks>
        ''' The value is <c>Nothing</c> until <see cref="Fit"/> has completed successfully.
        ''' </remarks>
        Public ReadOnly Property Result As KMeansClusterResult
            Get
                Return pResult
            End Get
        End Property

        ''' <summary>
        ''' Wraps the fitted k-means output into a list of presentation-ready <see cref="ResultTable"/> objects.
        ''' </summary>
        ''' <returns>
        ''' A list of formatted tables describing the fitted k-means solution, including settings,
        ''' fit summary, cluster centers, optional preprocessing constants, observation assignments,
        ''' and rows removed by the selected missing-value policy.
        ''' </returns>
        ''' <remarks>
        ''' <para>
        ''' Call <see cref="Fit"/> before calling this method. The returned tables are intended to be written to an
        ''' Excel worksheet using <see cref="ProcessListofResultTables.writeToSheet"/> together with <see cref="ExcelDnaResultWriter"/>.
        ''' </para>
        ''' <para>
        ''' This method follows the same project pattern used by the other analysis classes that expose
        ''' <c>wrapResults()</c> for UI/reporting.
        ''' </para>
        ''' </remarks>
        Public Function wrapResults() As List(Of ResultTable)
            If pResult Is Nothing Then CoreServices.Errors.LogAndThrow(New InvalidOperationException("Model is not fitted."))

            Dim out As New List(Of ResultTable)
            Dim t As ResultTable = Nothing

            t = ClusterAnalysisHelpers.BuildResultTableFromObjectMatrix("K-Means Settings", BuildSettingsTable())
            t.AddFootnote("Cluster labels are numbered from 1 to k.")
            out.Add(t)

            t = ClusterAnalysisHelpers.BuildResultTableFromObjectMatrix("K-Means Fit Summary", BuildFitSummaryTable())
            If pDistanceMetric = KMeansDistanceMetric.Euclidean Then
                t.AddFootnote("Reported point-to-center distances are Euclidean, but the k-means objective remains the total within-cluster sum of squared Euclidean distances.")
            End If
            out.Add(t)

            t = ClusterAnalysisHelpers.BuildResultTableFromObjectMatrix("Cluster Centers (Original Scale)", pResult.GetCentersTable(True))
            out.Add(t)

            If pResult.Standardization <> ClusterStandardizationMode.None Then
                t = ClusterAnalysisHelpers.BuildResultTableFromObjectMatrix("Cluster Centers (Working Analysis Scale)", pResult.GetCentersTable(False))
                out.Add(t)

                t = ClusterAnalysisHelpers.BuildResultTableFromObjectMatrix("Preprocessing Constants", BuildPreprocessingTable())
                If pResult.Standardization = ClusterStandardizationMode.ZScores Then
                    t.AddFootnote("Location = variable mean; Scale = sample standard deviation used to standardize the active analysis data.")
                ElseIf pResult.Standardization = ClusterStandardizationMode.RangeZeroToOne Then
                    t.AddFootnote("Location = variable minimum; Scale = observed range (maximum minus minimum) used to rescale the active analysis data to [0, 1].")
                End If
                out.Add(t)
            End If

            t = ClusterAnalysisHelpers.BuildResultTableFromObjectMatrix("Observation Assignments", pResult.GetObservationAssignmentsTable())
            out.Add(t)

            Dim removedTable As Object(,) = BuildRemovedRowsTable()
            If removedTable IsNot Nothing Then
                t = ClusterAnalysisHelpers.BuildResultTableFromObjectMatrix("Rows Removed by Missing-Value Policy", removedTable)
                out.Add(t)
            End If

            Return out
        End Function

        Private Function BuildSettingsTable() As Object(,)
            Dim out(1, 9) As Object
            out(0, 0) = "NumberOfClusters"
            out(0, 1) = "Initialization"
            out(0, 2) = "DistanceMetric"
            out(0, 3) = "RandomStarts"
            out(0, 4) = "MaxIterations"
            out(0, 5) = "Tolerance"
            out(0, 6) = "Standardization"
            out(0, 7) = "MissingValuePolicy"
            out(0, 8) = "EmptyClusterHandling"
            out(0, 9) = "RandomSeed"

            out(1, 0) = pNumberOfClusters
            out(1, 1) = pInitializationMethod.ToString()
            out(1, 2) = pDistanceMetric.ToString()
            out(1, 3) = If(pInitializationMethod = KMeansInitializationMethod.UserSpecifiedCenters, CType(1, Object), pStarts)
            out(1, 4) = pMaxIterations
            out(1, 5) = pTolerance
            out(1, 6) = pStandardization.ToString()
            out(1, 7) = pMissingValuePolicy.ToString()
            out(1, 8) = pEmptyClusterHandling.ToString()
            If pResult IsNot Nothing AndAlso pResult.RandomSeedUsed <> Integer.MinValue Then
                out(1, 9) = pResult.RandomSeedUsed
            ElseIf pRandomSeed <> Integer.MinValue Then
                out(1, 9) = pRandomSeed
            ElseIf pRandomSeedUsed <> Integer.MinValue Then
                out(1, 9) = pRandomSeedUsed
            Else
                out(1, 9) = String.Empty
            End If

            Return out
        End Function

        Private Function BuildFitSummaryTable() As Object(,)
            Dim out(1, 8) As Object
            out(0, 0) = "NumberOfClusters"
            out(0, 1) = "ActiveObservations"
            out(0, 2) = "RemovedObservations"
            out(0, 3) = "Iterations"
            out(0, 4) = "Converged"
            out(0, 5) = "TotalWithinClusterSS"
            out(0, 6) = "BetweenClusterSS"
            out(0, 7) = "TotalSS"
            out(0, 8) = "ObjectiveValue"

            out(1, 0) = pResult.NumberOfClusters
            out(1, 1) = If(pResult.ClusterAssignments Is Nothing, 0, pResult.ClusterAssignments.Length)
            out(1, 2) = If(pResult.RemovedRowIndices Is Nothing, 0, pResult.RemovedRowIndices.Length)
            out(1, 3) = pResult.Iterations
            out(1, 4) = pResult.Converged
            out(1, 5) = pResult.TotalWithinClusterSS
            out(1, 6) = pResult.BetweenClusterSS
            out(1, 7) = pResult.TotalSS
            out(1, 8) = pResult.ObjectiveValue

            Return out
        End Function

        Private Function BuildPreprocessingTable() As Object(,)
            Dim varCount As Integer = 0
            If pResult.VariableNames IsNot Nothing Then
                varCount = pResult.VariableNames.Length
            ElseIf pResult.StandardizationLocations IsNot Nothing Then
                varCount = pResult.StandardizationLocations.Length
            ElseIf pResult.StandardizationScales IsNot Nothing Then
                varCount = pResult.StandardizationScales.Length
            End If

            If varCount <= 0 Then
                Dim emptyOut(1, 2) As Object
                emptyOut(0, 0) = "Variable"
                emptyOut(0, 1) = "Location"
                emptyOut(0, 2) = "Scale"
                emptyOut(1, 0) = "(none)"
                emptyOut(1, 1) = Nothing
                emptyOut(1, 2) = Nothing
                Return emptyOut
            End If

            Dim out(varCount, 2) As Object
            out(0, 0) = "Variable"
            out(0, 1) = "Location"
            out(0, 2) = "Scale"

            For i As Integer = 0 To varCount - 1
                out(i + 1, 0) = If(pResult.VariableNames Is Nothing, CType($"Var {i + 1}", Object), pResult.VariableNames(i))
                out(i + 1, 1) = If(pResult.StandardizationLocations Is Nothing, CType(Nothing, Object), pResult.StandardizationLocations(i))
                out(i + 1, 2) = If(pResult.StandardizationScales Is Nothing, CType(Nothing, Object), pResult.StandardizationScales(i))
            Next

            Return out
        End Function

        Private Function BuildRemovedRowsTable() As Object(,)
            If pResult Is Nothing OrElse pResult.RemovedRowIndices Is Nothing OrElse pResult.RemovedRowIndices.Length = 0 Then
                Return Nothing
            End If

            Dim hasLabels As Boolean = (pResult.RemovedRowLabels IsNot Nothing AndAlso pResult.RemovedRowLabels.Length = pResult.RemovedRowIndices.Length)
            Dim out(pResult.RemovedRowIndices.Length, If(hasLabels, 2, 1)) As Object

            out(0, 0) = "OriginalRow"
            If hasLabels Then
                out(0, 1) = "RowLabel"
                out(0, 2) = "Reason"
            Else
                out(0, 1) = "Reason"
            End If

            For i As Integer = 0 To pResult.RemovedRowIndices.Length - 1
                out(i + 1, 0) = pResult.RemovedRowIndices(i)
                If hasLabels Then
                    out(i + 1, 1) = pResult.RemovedRowLabels(i)
                    out(i + 1, 2) = "Removed before fitting because at least one analysis variable was missing or non-finite."
                Else
                    out(i + 1, 1) = "Removed before fitting because at least one analysis variable was missing or non-finite."
                End If
            Next

            Return out
        End Function

        ''' <summary>
        ''' Fits the k-means model using the current data and settings.
        ''' </summary>
        ''' <exception cref="System.ArgumentException">
        ''' Thrown when the inputs are inconsistent, when the requested number of clusters exceeds the number of active observations,
        ''' or when user-specified centers do not match the data dimensions.
        ''' </exception>
        Public Sub Fit()
            If pData Is Nothing Then CoreServices.Errors.LogAndThrow(New ArgumentException("No data supplied. Call dataInputs() first."))

            Dim prepared As ClusterPreparedData = ClusterAnalysisHelpers.PrepareData(pData, pRowLabels, pVarNames, pStandardization, pMissingValuePolicy)
            Dim n As Integer = prepared.WorkingData.GetUpperBound(0) + 1
            Dim p As Integer = prepared.WorkingData.GetUpperBound(1) + 1

            If n < pNumberOfClusters Then
                CoreServices.Errors.LogAndThrow(New ArgumentException("The number of active observations must be at least as large as the requested number of clusters."))
            End If

            If pInitializationMethod = KMeansInitializationMethod.UserSpecifiedCenters Then
                If Not pHasUserStartingCenters OrElse pUserStartingCenters Is Nothing Then
                    CoreServices.Errors.LogAndThrow(New ArgumentException("UserSpecifiedCenters was requested but no starting centers were provided through startingCentersInputs()."))
                End If
                If pUserStartingCenters.GetUpperBound(0) + 1 <> pNumberOfClusters Then
                    CoreServices.Errors.LogAndThrow(New ArgumentException("The number of supplied starting centers does not match numberOfClusters."))
                End If
                If pUserStartingCenters.GetUpperBound(1) + 1 <> p Then
                    CoreServices.Errors.LogAndThrow(New ArgumentException("The number of columns in the supplied starting centers does not match the number of variables."))
                End If
            End If

            pRandomSeedUsed = Integer.MinValue
            Dim effectiveSeed As Integer = ResolveEffectiveRandomSeed()
            Dim rng As Random = ClusterAnalysisHelpers.CreateRandom(effectiveSeed)
            Dim startsToRun As Integer = If(pInitializationMethod = KMeansInitializationMethod.UserSpecifiedCenters, 1, Math.Max(1, pStarts))

            Dim bestAssignments() As Integer = Nothing
            Dim bestCentersWorking(,) As Double = Nothing
            Dim bestDistances() As Double = Nothing
            Dim bestIterations As Integer = 0
            Dim bestConverged As Boolean = False
            Dim bestObjective As Double = Double.PositiveInfinity
            Dim bestClusterSS() As Double = Nothing

            For s As Integer = 1 To startsToRun
                Dim initialCenters(,) As Double
                If pInitializationMethod = KMeansInitializationMethod.UserSpecifiedCenters Then
                    initialCenters = ClusterAnalysisHelpers.StandardizeExternalCenters(pUserStartingCenters, prepared)
                Else
                    initialCenters = InitializeCenters(prepared.WorkingData, pNumberOfClusters, pInitializationMethod, rng)
                End If

                Dim fit = RunSingleStart(prepared.WorkingData, initialCenters, rng)

                If fit.ObjectiveValue < bestObjective Then
                    bestObjective = fit.ObjectiveValue
                    bestAssignments = fit.Assignments
                    bestCentersWorking = fit.Centers
                    bestDistances = fit.AssignedDistances
                    bestIterations = fit.Iterations
                    bestConverged = fit.Converged
                    bestClusterSS = fit.WithinClusterSSByCluster
                End If
            Next

            Dim totalSS As Double = ClusterAnalysisHelpers.ComputeTotalSS(prepared.WorkingData)
            Dim betweenSS As Double = totalSS - bestObjective
            If betweenSS < 0 AndAlso Math.Abs(betweenSS) < 0.000000001 Then betweenSS = 0

            Dim sizes(pNumberOfClusters - 1) As Integer
            For i As Integer = 0 To bestAssignments.Length - 1
                sizes(bestAssignments(i) - 1) += 1
            Next

            Dim centersOriginal(,) As Double = ClusterAnalysisHelpers.UnstandardizeCenters(bestCentersWorking, prepared)
            Dim grandMeanWorking() As Double = ClusterAnalysisHelpers.ColumnMeans(prepared.WorkingData)
            Dim grandMeanOriginal() As Double = ClusterAnalysisHelpers.ColumnMeans(prepared.ActiveOriginalData)

            Dim result As New KMeansClusterResult With {
                .ActiveRowIndices = prepared.ActiveOriginalIndices,
                .ActiveRowLabels = prepared.ActiveRowLabels,
                .RemovedRowIndices = prepared.RemovedOriginalIndices,
                .RemovedRowLabels = prepared.RemovedRowLabels,
                .VariableNames = prepared.VariableNames,
                .Standardization = pStandardization,
                .MissingValuePolicy = pMissingValuePolicy,
                .InitializationMethod = pInitializationMethod,
                .DistanceMetric = pDistanceMetric,
                .NumberOfClusters = pNumberOfClusters,
                .StartsEvaluated = startsToRun,
                .Iterations = bestIterations,
                .Converged = bestConverged,
                .ClusterAssignments = bestAssignments,
                .ClusterSizes = sizes,
                .CentersWorkingScale = bestCentersWorking,
                .CentersOriginalScale = centersOriginal,
                .GrandMeanWorkingScale = grandMeanWorking,
                .GrandMeanOriginalScale = grandMeanOriginal,
                .DistanceToAssignedCenter = bestDistances,
                .WithinClusterSSByCluster = bestClusterSS,
                .TotalWithinClusterSS = bestObjective,
                .BetweenClusterSS = betweenSS,
                .TotalSS = totalSS,
                .ObjectiveValue = bestObjective,
                .StandardizationLocations = prepared.ColumnLocations,
                .StandardizationScales = prepared.ColumnScales,
                .RandomSeedUsed = pRandomSeedUsed
            }

            pResult = result
        End Sub

        Private Function ResolveEffectiveRandomSeed() As Integer
            If pRandomSeed <> Integer.MinValue Then
                pRandomSeedUsed = pRandomSeed
            Else
                pRandomSeedUsed = CoreServices.AnalysisDefaults.ResolveRandomSeed(generateWhenMissing:=True)
            End If

            Return pRandomSeedUsed
        End Function

        ''' <summary>
        ''' Assigns new observations to the nearest fitted cluster center.
        ''' </summary>
        ''' <param name="arNewData">New data matrix with the same number and ordering of variables as the training data.</param>
        ''' <returns>An array of cluster labels numbered from 1 to <c>k</c>.</returns>
        ''' <remarks>
        ''' <para>
        ''' New observations are transformed using the preprocessing parameters estimated from the training data, then assigned
        ''' to the nearest fitted center.
        ''' </para>
        ''' <para>
        ''' This method requires a previously fitted model.
        ''' </para>
        ''' </remarks>
        Public Function Predict(arNewData(,) As Double) As Integer()
            Dim distances() As Double = Nothing
            Return PredictInternal(arNewData, distances)
        End Function

        ''' <summary>
        ''' Computes the distance from each new observation to its nearest fitted cluster center.
        ''' </summary>
        ''' <param name="arNewData">New data matrix with the same number and ordering of variables as the training data.</param>
        ''' <returns>An array of distances to the nearest fitted center.</returns>
        Public Function DistanceToNearestCluster(arNewData(,) As Double) As Double()
            Dim distances() As Double = Nothing
            PredictInternal(arNewData, distances)
            Return distances
        End Function

        Private Function PredictInternal(arNewData(,) As Double, ByRef distances() As Double) As Integer()
            If pResult Is Nothing Then CoreServices.Errors.LogAndThrow(New InvalidOperationException("The model has not been fitted yet."))
            If arNewData Is Nothing Then CoreServices.Errors.LogAndThrow(New ArgumentException("arNewData must not be Nothing."))

            Dim prepared As ClusterPreparedData = ClusterAnalysisHelpers.PreparePredictionData(arNewData, pResult.VariableNames, pResult.Standardization, pResult.StandardizationLocations, pResult.StandardizationScales)
            Dim n As Integer = prepared.WorkingData.GetUpperBound(0) + 1
            Dim assignments(n - 1) As Integer
            ReDim distances(n - 1)

            For i As Integer = 0 To n - 1
                Dim bestCluster As Integer = -1
                Dim bestSq As Double = Double.PositiveInfinity
                For c As Integer = 0 To pResult.NumberOfClusters - 1
                    Dim sq As Double = ClusterAnalysisHelpers.SquaredEuclideanRowToCenter(prepared.WorkingData, i, pResult.CentersWorkingScale, c)
                    If sq < bestSq Then
                        bestSq = sq
                        bestCluster = c
                    End If
                Next
                assignments(i) = bestCluster + 1
                distances(i) = If(pResult.DistanceMetric = KMeansDistanceMetric.Euclidean, Math.Sqrt(bestSq), bestSq)
            Next

            Return assignments
        End Function

        Private Function InitializeCenters(data(,) As Double,
                                           k As Integer,
                                           initialization As KMeansInitializationMethod,
                                           rng As Random) As Double(,)
            Select Case initialization
                Case KMeansInitializationMethod.Forgy
                    Return ClusterAnalysisHelpers.InitializeForgy(data, k, rng)
                Case KMeansInitializationMethod.RandomPartition
                    Return ClusterAnalysisHelpers.InitializeRandomPartition(data, k, rng)
                Case Else
                    Return ClusterAnalysisHelpers.InitializeKMeansPlusPlus(data, k, rng)
            End Select
        End Function

        Private Function RunSingleStart(data(,) As Double,
                                        initialCenters(,) As Double,
                                        rng As Random) As SingleKMeansRunResult

            Dim n As Integer = data.GetUpperBound(0) + 1
            Dim p As Integer = data.GetUpperBound(1) + 1
            Dim centers(,) As Double = CType(initialCenters.Clone(), Double(,))
            Dim assignments(n - 1) As Integer
            Dim prevAssignments(n - 1) As Integer
            For i As Integer = 0 To prevAssignments.Length - 1
                prevAssignments(i) = -1
            Next

            Dim converged As Boolean = False
            Dim finalIterations As Integer = 0

            For iter As Integer = 1 To pMaxIterations
                finalIterations = iter

                Dim counts(pNumberOfClusters - 1) As Integer
                Dim sqDistances(n - 1) As Double

                For i As Integer = 0 To n - 1
                    Dim bestCluster As Integer = 0
                    Dim bestSq As Double = Double.PositiveInfinity
                    For c As Integer = 0 To pNumberOfClusters - 1
                        Dim sq As Double = ClusterAnalysisHelpers.SquaredEuclideanRowToCenter(data, i, centers, c)
                        If sq < bestSq Then
                            bestSq = sq
                            bestCluster = c
                        End If
                    Next
                    assignments(i) = bestCluster + 1
                    counts(bestCluster) += 1
                    sqDistances(i) = bestSq
                Next

                If counts.Any(Function(v) v = 0) Then
                    ResolveEmptyClusters(assignments, counts, sqDistances, rng)
                End If

                Dim newCenters(pNumberOfClusters - 1, p - 1) As Double
                For i As Integer = 0 To n - 1
                    Dim c As Integer = assignments(i) - 1
                    For j As Integer = 0 To p - 1
                        newCenters(c, j) += data(i, j)
                    Next
                Next

                For c As Integer = 0 To pNumberOfClusters - 1
                    If counts(c) > 0 Then
                        For j As Integer = 0 To p - 1
                            newCenters(c, j) /= counts(c)
                        Next
                    Else
                        For j As Integer = 0 To p - 1
                            newCenters(c, j) = centers(c, j)
                        Next
                    End If
                Next

                Dim maxMoveSq As Double = 0
                For c As Integer = 0 To pNumberOfClusters - 1
                    Dim moveSq As Double = 0
                    For j As Integer = 0 To p - 1
                        Dim d As Double = newCenters(c, j) - centers(c, j)
                        moveSq += d * d
                    Next
                    If moveSq > maxMoveSq Then maxMoveSq = moveSq
                Next

                Dim assignmentsChanged As Boolean = False
                For i As Integer = 0 To n - 1
                    If assignments(i) <> prevAssignments(i) Then
                        assignmentsChanged = True
                        Exit For
                    End If
                Next

                Array.Copy(assignments, prevAssignments, assignments.Length)
                centers = newCenters

                If (Not assignmentsChanged) OrElse maxMoveSq <= pTolerance * pTolerance Then
                    converged = True
                    Exit For
                End If
            Next

            Dim finalSqDistances(n - 1) As Double
            Dim finalClusterSS(pNumberOfClusters - 1) As Double
            Dim objective As Double = 0
            For i As Integer = 0 To n - 1
                Dim c As Integer = assignments(i) - 1
                Dim sq As Double = ClusterAnalysisHelpers.SquaredEuclideanRowToCenter(data, i, centers, c)
                finalSqDistances(i) = sq
                finalClusterSS(c) += sq
                objective += sq
            Next

            Dim outputDistances(n - 1) As Double
            For i As Integer = 0 To n - 1
                outputDistances(i) = If(pDistanceMetric = KMeansDistanceMetric.Euclidean, Math.Sqrt(finalSqDistances(i)), finalSqDistances(i))
            Next

            Dim fit As New SingleKMeansRunResult
            fit.Assignments = CType(assignments.Clone(), Integer())
            fit.Centers = centers
            fit.AssignedDistances = outputDistances
            fit.Iterations = finalIterations
            fit.Converged = converged
            fit.ObjectiveValue = objective
            fit.WithinClusterSSByCluster = finalClusterSS
            Return fit
        End Function

        Private Sub ResolveEmptyClusters(ByRef assignments() As Integer,
                                         ByRef counts() As Integer,
                                         sqDistances() As Double,
                                         rng As Random)
            If Not counts.Any(Function(v) v = 0) Then Return

            Select Case pEmptyClusterHandling
                Case EmptyClusterHandlingStrategy.KeepPreviousCenter
                    Return
            End Select

            For emptyCluster As Integer = 0 To counts.Length - 1
                If counts(emptyCluster) <> 0 Then Continue For

                Dim donorIndex As Integer = -1

                If pEmptyClusterHandling = EmptyClusterHandlingStrategy.FarthestObservation Then
                    Dim bestDist As Double = Double.NegativeInfinity
                    For i As Integer = 0 To assignments.Length - 1
                        Dim currentCluster As Integer = assignments(i) - 1
                        If counts(currentCluster) <= 1 Then Continue For
                        If sqDistances(i) > bestDist Then
                            bestDist = sqDistances(i)
                            donorIndex = i
                        End If
                    Next
                ElseIf pEmptyClusterHandling = EmptyClusterHandlingStrategy.RandomObservation Then
                    Dim candidates As New List(Of Integer)
                    For i As Integer = 0 To assignments.Length - 1
                        Dim currentCluster As Integer = assignments(i) - 1
                        If counts(currentCluster) > 1 Then candidates.Add(i)
                    Next
                    If candidates.Count > 0 Then donorIndex = candidates(rng.Next(candidates.Count))
                End If

                If donorIndex = -1 Then
                    For i As Integer = 0 To assignments.Length - 1
                        Dim currentCluster As Integer = assignments(i) - 1
                        If counts(currentCluster) > 1 Then
                            donorIndex = i
                            Exit For
                        End If
                    Next
                End If

                If donorIndex = -1 Then Exit For

                Dim oldCluster As Integer = assignments(donorIndex) - 1
                counts(oldCluster) -= 1
                assignments(donorIndex) = emptyCluster + 1
                counts(emptyCluster) += 1
                sqDistances(donorIndex) = 0
            Next
        End Sub
    End Class

    ''' <summary>
    ''' Stores the fitted output of an agglomerative hierarchical clustering analysis.
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' The merge arrays define a binary clustering tree. Leaves are numbered 1 to <c>n</c> in the order of the active
    ''' observations after preprocessing. Each merge creates a new cluster with id <c>n + step</c>, where <c>step</c>
    ''' starts at 1.
    ''' </para>
    ''' <para>
    ''' Rows removed by listwise deletion are reported separately and do not appear in the active-row arrays.
    ''' </para>
    ''' </remarks>
    Public Class HierarchicalClusterResult

        ''' <summary>
        ''' Gets or sets the original 1-based row numbers retained in the clustering analysis.
        ''' </summary>
        Public Property ActiveRowIndices As Integer()

        ''' <summary>
        ''' Gets or sets the labels for the rows retained in the clustering analysis.
        ''' </summary>
        Public Property ActiveRowLabels As String()

        ''' <summary>
        ''' Gets or sets the original 1-based row numbers removed by listwise deletion.
        ''' </summary>
        Public Property RemovedRowIndices As Integer()

        ''' <summary>
        ''' Gets or sets the labels for rows removed by listwise deletion.
        ''' </summary>
        Public Property RemovedRowLabels As String()

        ''' <summary>
        ''' Gets or sets the variable names used in the clustering analysis.
        ''' </summary>
        Public Property VariableNames As String()

        ''' <summary>
        ''' Gets or sets the linkage method used by the fitted hierarchical clustering solution.
        ''' </summary>
        Public Property LinkageMethod As HierarchicalLinkageMethod

        ''' <summary>
        ''' Gets or sets the base observation-level distance metric used by the fitted solution.
        ''' </summary>
        Public Property DistanceMetric As HierarchicalDistanceMetric

        ''' <summary>
        ''' Gets or sets the Minkowski power parameter used when <see cref="DistanceMetric"/> is <see cref="HierarchicalDistanceMetric.Minkowski"/>.
        ''' </summary>
        Public Property MinkowskiPower As Double

        ''' <summary>
        ''' Gets or sets the standardization mode used during fitting.
        ''' </summary>
        Public Property Standardization As ClusterStandardizationMode

        ''' <summary>
        ''' Gets or sets the missing-value policy used during fitting.
        ''' </summary>
        Public Property MissingValuePolicy As ClusterMissingValuePolicy

        ''' <summary>
        ''' Gets or sets the left predecessor cluster id for each merge step.
        ''' </summary>
        Public Property MergeLeftClusterIds As Integer()

        ''' <summary>
        ''' Gets or sets the right predecessor cluster id for each merge step.
        ''' </summary>
        Public Property MergeRightClusterIds As Integer()

        ''' <summary>
        ''' Gets or sets the merge height for each agglomeration step.
        ''' </summary>
        ''' <remarks>
        ''' The meaning of height depends on the linkage method and distance definition. For Ward linkage the height is the
        ''' increase in within-cluster sum of squares on the working analysis scale.
        ''' </remarks>
        Public Property MergeHeights As Double()

        ''' <summary>
        ''' Gets or sets the size of the newly created cluster at each agglomeration step.
        ''' </summary>
        Public Property MergeClusterSizes As Integer()

        ''' <summary>
        ''' Gets or sets the left-to-right leaf order of the active observations, expressed as original 1-based row numbers.
        ''' </summary>
        Public Property LeafOrder As Integer()

        ''' <summary>
        ''' Gets or sets the column locations used for preprocessing.
        ''' </summary>
        Public Property StandardizationLocations As Double()

        ''' <summary>
        ''' Gets or sets the column scales used for preprocessing.
        ''' </summary>
        Public Property StandardizationScales As Double()

        ''' <summary>
        ''' Returns an agglomeration schedule table suitable for display or later export.
        ''' </summary>
        ''' <returns>
        ''' A two-dimensional <see cref="Object"/> array with one row per merge and the columns
        ''' <c>Step</c>, <c>LeftClusterId</c>, <c>RightClusterId</c>, <c>Height</c>, and <c>NewClusterSize</c>.
        ''' </returns>
        Public Function GetAgglomerationSchedule() As Object(,)
            Dim steps As Integer = If(MergeHeights Is Nothing, 0, MergeHeights.Length)
            Dim out(Math.Max(steps, 1), 4) As Object
            out(0, 0) = "Step"
            out(0, 1) = "LeftClusterId"
            out(0, 2) = "RightClusterId"
            out(0, 3) = "Height"
            out(0, 4) = "NewClusterSize"

            For i As Integer = 0 To steps - 1
                out(i + 1, 0) = i + 1
                out(i + 1, 1) = MergeLeftClusterIds(i)
                out(i + 1, 2) = MergeRightClusterIds(i)
                out(i + 1, 3) = MergeHeights(i)
                out(i + 1, 4) = MergeClusterSizes(i)
            Next

            Return out
        End Function

        ''' <summary>
        ''' Cuts the hierarchical tree to produce a requested number of clusters.
        ''' </summary>
        ''' <param name="numberOfClusters">Requested number of clusters.</param>
        ''' <returns>An array of cluster labels for the active observations, numbered from 1 to the requested number of clusters.</returns>
        Public Function GetMembershipByClusterCount(numberOfClusters As Integer) As Integer()
            If ActiveRowIndices Is Nothing Then CoreServices.Errors.LogAndThrow(New InvalidOperationException("No fitted solution is available."))
            Dim n As Integer = ActiveRowIndices.Length
            If numberOfClusters < 1 OrElse numberOfClusters > n Then
                CoreServices.Errors.LogAndThrow(New ArgumentException("numberOfClusters must be between 1 and the number of active observations."))
            End If
            Return ClusterAnalysisHelpers.BuildMembershipFromMerges(n, MergeLeftClusterIds, MergeRightClusterIds, n - numberOfClusters)
        End Function

        ''' <summary>
        ''' Cuts the hierarchical tree at a specified merge height.
        ''' </summary>
        ''' <param name="cutHeight">Merge-height threshold. Merges at or below this height are applied.</param>
        ''' <returns>An array of cluster labels for the active observations.</returns>
        Public Function GetMembershipByHeight(cutHeight As Double) As Integer()
            If ActiveRowIndices Is Nothing Then CoreServices.Errors.LogAndThrow(New InvalidOperationException("No fitted solution is available."))
            Dim mergesToApply As Integer = 0
            If MergeHeights IsNot Nothing Then
                For i As Integer = 0 To MergeHeights.Length - 1
                    If MergeHeights(i) <= cutHeight Then
                        mergesToApply += 1
                    Else
                        Exit For
                    End If
                Next
            End If
            Return ClusterAnalysisHelpers.BuildMembershipFromMerges(ActiveRowIndices.Length, MergeLeftClusterIds, MergeRightClusterIds, mergesToApply)
        End Function

        ''' <summary>
        ''' Returns a row-wise table of observation membership after cutting the tree to a requested number of clusters.
        ''' </summary>
        ''' <param name="numberOfClusters">Requested number of clusters.</param>
        ''' <returns>
        ''' A two-dimensional <see cref="Object"/> array with columns <c>OriginalRow</c>, <c>RowLabel</c>, and <c>Cluster</c>.
        ''' </returns>
        Public Function GetMembershipTable(numberOfClusters As Integer) As Object(,)
            Dim membership() As Integer = GetMembershipByClusterCount(numberOfClusters)
            Dim out(Math.Max(membership.Length, 1), 2) As Object
            out(0, 0) = "OriginalRow"
            out(0, 1) = "RowLabel"
            out(0, 2) = "Cluster"
            For i As Integer = 0 To membership.Length - 1
                out(i + 1, 0) = ActiveRowIndices(i)
                out(i + 1, 1) = ActiveRowLabels(i)
                out(i + 1, 2) = membership(i)
            Next
            Return out
        End Function

        ''' <summary>
        ''' Returns a row-wise table of observation membership after cutting the tree at a requested merge height.
        ''' </summary>
        ''' <param name="cutHeight">Merge-height threshold. Merges at or below this height are applied.</param>
        ''' <returns>
        ''' A two-dimensional <see cref="Object"/> array with columns <c>OriginalRow</c>, <c>RowLabel</c>, and <c>Cluster</c>.
        ''' </returns>
        Public Function GetMembershipTableByHeight(cutHeight As Double) As Object(,)
            Dim membership() As Integer = GetMembershipByHeight(cutHeight)
            Dim out(Math.Max(membership.Length, 1), 2) As Object
            out(0, 0) = "OriginalRow"
            out(0, 1) = "RowLabel"
            out(0, 2) = "Cluster"
            For i As Integer = 0 To membership.Length - 1
                out(i + 1, 0) = ActiveRowIndices(i)
                out(i + 1, 1) = ActiveRowLabels(i)
                out(i + 1, 2) = membership(i)
            Next
            Return out
        End Function


        ''' <summary>
        ''' Builds a reusable dendrogram layout from the fitted hierarchical clustering result.
        ''' </summary>
        ''' <param name="heightMode">
        ''' Controls whether branch heights follow merge step numbers or the fitted merge distances.
        ''' </param>
        ''' <param name="orientation">Requested display orientation for the final chart coordinates.</param>
        ''' <returns>
        ''' A <see cref="DendrogramLayout"/> object containing the polyline coordinates, leaf coordinates, and labels
        ''' required to draw the dendrogram as an Excel X/Y scatter chart.
        ''' </returns>
        ''' <remarks>
        ''' <para>
        ''' The generated polyline follows the “continuous pen path” teaching approach described in Chapter 8 of the
        ''' attached Excel text. Some segments are therefore intentionally retraced so that the entire dendrogram can be
        ''' drawn from a single scatter-line series.
        ''' </para>
        ''' </remarks>
        Public Function CreateDendrogramLayout(Optional heightMode As DendrogramHeightMode = DendrogramHeightMode.MergeDistance,
                                               Optional orientation As DendrogramOrientation = DendrogramOrientation.Top,
                                               Optional cutMode As HierarchicalMembershipDisplayMode = HierarchicalMembershipDisplayMode.ByClusterCount,
                                               Optional membershipClusterCount As Integer = 3,
                                               Optional membershipCutHeight As Double = 0.0) As DendrogramLayout
            If ActiveRowIndices Is Nothing OrElse MergeLeftClusterIds Is Nothing OrElse MergeRightClusterIds Is Nothing Then
                CoreServices.Errors.LogAndThrow(New InvalidOperationException("No fitted hierarchical clustering solution is available."))
            End If
            Return ClusterAnalysisHelpers.BuildDendrogramLayout(Me, heightMode, orientation, cutMode, membershipClusterCount, membershipCutHeight)
        End Function

    End Class

    ''' <summary>
    ''' Represents a fully prepared dendrogram drawing layout.
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' The layout stores the single-series polyline coordinates needed to reproduce the Chapter-8 scatter-plot
    ''' dendrogram technique, together with the leaf positions and labels used for annotation.
    ''' </para>
    ''' </remarks>
    Public Class DendrogramLayout

        ''' <summary>
        ''' Gets or sets the height scaling mode used by the layout.
        ''' </summary>
        Public Property HeightMode As DendrogramHeightMode

        ''' <summary>
        ''' Gets or sets the final display orientation.
        ''' </summary>
        Public Property Orientation As DendrogramOrientation

        ''' <summary>
        ''' Gets or sets the X coordinates of the continuous dendrogram polyline.
        ''' </summary>
        Public Property PolylineX As Double()

        ''' <summary>
        ''' Gets or sets the Y coordinates of the continuous dendrogram polyline.
        ''' </summary>
        Public Property PolylineY As Double()

        ''' <summary>
        ''' Gets or sets the X coordinates of the leaf positions used for labeling.
        ''' </summary>
        Public Property LeafX As Double()

        ''' <summary>
        ''' Gets or sets the Y coordinates of the leaf positions used for labeling.
        ''' </summary>
        Public Property LeafY As Double()

        ''' <summary>
        ''' Gets or sets the labels shown for the dendrogram leaves.
        ''' </summary>
        Public Property LeafLabels As String()

        ''' <summary>
        ''' Gets or sets the original 1-based row numbers in display order.
        ''' </summary>
        Public Property LeafOriginalRowOrder As Integer()

        ''' <summary>
        ''' Gets or sets the maximum branch height represented in the layout.
        ''' </summary>
        Public Property MaximumHeight As Double

        ''' <summary>
        ''' Gets or sets the number of displayed leaves.
        ''' </summary>
        Public Property LeafCount As Integer

        ''' <summary>
        ''' Gets or sets one colored polyline X-array for each cluster-specific subtree drawn below the selected cut.
        ''' </summary>
        Public Property ClusterPolylineX As List(Of Double())

        ''' <summary>
        ''' Gets or sets one colored polyline Y-array for each cluster-specific subtree drawn below the selected cut.
        ''' </summary>
        Public Property ClusterPolylineY As List(Of Double())

        ''' <summary>
        ''' Gets or sets the display height of the selected cut line, when available.
        ''' </summary>
        Public Property CutDisplayHeight As Double?

        ''' <summary>
        ''' Gets or sets the X coordinates of the dashed cut line, when available.
        ''' </summary>
        Public Property CutLineX As Double()

        ''' <summary>
        ''' Gets or sets the Y coordinates of the dashed cut line, when available.
        ''' </summary>
        Public Property CutLineY As Double()

        ''' <summary>
        ''' Builds a spaced axis-title string containing all leaf labels in display order.
        ''' </summary>
        ''' <param name="minimumSpacesBetweenLabels">Minimum number of spaces inserted between neighboring labels.</param>
        ''' <returns>A single string suitable for use as an axis title.</returns>
        ''' <remarks>
        ''' This matches the Chapter-8 idea of using the axis title as a compact label strip below the dendrogram.
        ''' </remarks>
        Public Function GetSuggestedAxisTitle(Optional minimumSpacesBetweenLabels As Integer = 3) As String
            If LeafLabels Is Nothing OrElse LeafLabels.Length = 0 Then Return String.Empty
            Return String.Join(New String(" "c, Math.Max(1, minimumSpacesBetweenLabels)), LeafLabels)
        End Function

    End Class

    ''' <summary>
    ''' Performs agglomerative hierarchical clustering for numeric data.
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' The algorithm starts with one singleton cluster per observation and repeatedly merges the closest pair until all
    ''' active observations belong to one cluster. The notion of “closest” is controlled jointly by the selected distance
    ''' metric and linkage method.
    ''' </para>
    ''' <para>
    ''' The fitted merge tree is available through <see cref="Result"/> after calling <see cref="Fit"/>.
    ''' </para>
    ''' </remarks>
    Public Class HierarchicalClustering

        Private pData(,) As Double
        Private pRowLabels() As String
        Private pVarNames() As String

        Private pLinkage As HierarchicalLinkageMethod = HierarchicalLinkageMethod.Ward
        Private pDistanceMetric As HierarchicalDistanceMetric = HierarchicalDistanceMetric.SquaredEuclidean
        Private pMinkowskiPower As Double = 2.0
        Private pStandardization As ClusterStandardizationMode = ClusterStandardizationMode.None
        Private pMissingValuePolicy As ClusterMissingValuePolicy = ClusterMissingValuePolicy.ErrorOnMissing

        Private pResult As HierarchicalClusterResult
        Private pMembershipDisplayMode As HierarchicalMembershipDisplayMode = HierarchicalMembershipDisplayMode.ByClusterCount
        Private pMembershipClusterCount As Integer = 3
        Private pMembershipCutHeight As Double = 0.0

        ''' <summary>
        ''' Supplies the numeric data matrix and optional row and variable labels used by hierarchical clustering.
        ''' </summary>
        ''' <param name="arData">Numeric input matrix with observations in rows and variables in columns.</param>
        ''' <param name="arRowLabels">Optional observation labels. When omitted, default labels <c>Obs 1</c>, <c>Obs 2</c>, ... are generated.</param>
        ''' <param name="arVarNames">Optional variable names. When omitted, default names <c>Var 1</c>, <c>Var 2</c>, ... are generated.</param>
        Public Sub dataInputs(arData(,) As Double,
                              Optional arRowLabels() As String = Nothing,
                              Optional arVarNames() As String = Nothing)
            pData = arData
            pRowLabels = arRowLabels
            pVarNames = arVarNames
            pResult = Nothing
        End Sub

        ''' <summary>
        ''' Configures the hierarchical clustering procedure.
        ''' </summary>
        ''' <param name="linkage">Agglomeration rule used to merge clusters.</param>
        ''' <param name="distanceMetric">Base observation-level distance metric.</param>
        ''' <param name="minkowskiPower">Power parameter for Minkowski distance. Must be positive.</param>
        ''' <param name="standardization">Optional variable standardization applied before clustering.</param>
        ''' <param name="missingValuePolicy">Policy for rows that contain missing or non-finite numeric values.</param>
        Public Sub settingsInputs(Optional linkage As HierarchicalLinkageMethod = HierarchicalLinkageMethod.Ward,
                                  Optional distanceMetric As HierarchicalDistanceMetric = HierarchicalDistanceMetric.SquaredEuclidean,
                                  Optional minkowskiPower As Double = 2.0,
                                  Optional standardization As ClusterStandardizationMode = ClusterStandardizationMode.None,
                                  Optional missingValuePolicy As ClusterMissingValuePolicy = ClusterMissingValuePolicy.ErrorOnMissing)
            If minkowskiPower <= 0 Then CoreServices.Errors.LogAndThrow(New ArgumentException("minkowskiPower must be greater than 0."))

            pLinkage = linkage
            pDistanceMetric = distanceMetric
            pMinkowskiPower = minkowskiPower
            pStandardization = standardization
            pMissingValuePolicy = missingValuePolicy
            pResult = Nothing
        End Sub

        ''' <summary>
        ''' Configures how formatted membership output should be produced by <see cref="wrapResults"/>.
        ''' </summary>
        ''' <param name="cutMode">Specifies whether the membership table is cut by cluster count or by merge height.</param>
        ''' <param name="membershipClusterCount">Requested number of clusters when <paramref name="cutMode"/> is <see cref="HierarchicalMembershipDisplayMode.ByClusterCount"/>.</param>
        ''' <param name="membershipCutHeight">Requested merge-height threshold when <paramref name="cutMode"/> is <see cref="HierarchicalMembershipDisplayMode.ByHeight"/>.</param>
        Public Sub reportInputs(Optional cutMode As HierarchicalMembershipDisplayMode = HierarchicalMembershipDisplayMode.ByClusterCount,
                                Optional membershipClusterCount As Integer = 3,
                                Optional membershipCutHeight As Double = 0.0)
            If membershipClusterCount < 1 Then CoreServices.Errors.LogAndThrow(New ArgumentException("membershipClusterCount must be at least 1."))
            pMembershipDisplayMode = cutMode
            pMembershipClusterCount = membershipClusterCount
            pMembershipCutHeight = membershipCutHeight
        End Sub

        ''' <summary>
        ''' Gets the fitted hierarchical clustering result object.
        ''' </summary>
        ''' <remarks>
        ''' The value is <c>Nothing</c> until <see cref="Fit"/> has completed successfully.
        ''' </remarks>
        Public ReadOnly Property Result As HierarchicalClusterResult
            Get
                Return pResult
            End Get
        End Property

        ''' <summary>
        ''' Wraps the fitted hierarchical-clustering output into a list of presentation-ready <see cref="ResultTable"/> objects.
        ''' </summary>
        ''' <returns>
        ''' A list of formatted tables describing the fitted hierarchical-clustering solution, including settings,
        ''' fit summary, agglomeration schedule, leaf order, the requested membership table, optional preprocessing
        ''' constants, and rows removed by the selected missing-value policy.
        ''' </returns>
        Public Function wrapResults() As List(Of ResultTable)
            If pResult Is Nothing Then CoreServices.Errors.LogAndThrow(New InvalidOperationException("Model is not fitted."))

            Dim out As New List(Of ResultTable)
            Dim t As ResultTable = Nothing

            t = ClusterAnalysisHelpers.BuildResultTableFromObjectMatrix("Hierarchical Clustering Settings", BuildHierarchicalSettingsTable())
            t.AddFootnote("Leaf nodes correspond to the active observations retained after preprocessing. Newly created internal cluster ids are numbered n+1, n+2, ... in merge order.")
            out.Add(t)

            t = ClusterAnalysisHelpers.BuildResultTableFromObjectMatrix("Hierarchical Clustering Fit Summary", BuildHierarchicalFitSummaryTable())
            out.Add(t)

            t = ClusterAnalysisHelpers.BuildResultTableFromObjectMatrix("Agglomeration Schedule", pResult.GetAgglomerationSchedule())
            out.Add(t)

            t = ClusterAnalysisHelpers.BuildResultTableFromObjectMatrix("Leaf Order", BuildLeafOrderTable())
            t.AddFootnote("The leaf order is the left-to-right observation order used when drawing the dendrogram.")
            out.Add(t)

            If pMembershipDisplayMode = HierarchicalMembershipDisplayMode.ByHeight Then
                t = ClusterAnalysisHelpers.BuildResultTableFromObjectMatrix($"Cluster Membership (Cut Height <= {pMembershipCutHeight})", pResult.GetMembershipTableByHeight(pMembershipCutHeight))
                t.AddFootnote("Clusters were obtained by applying all merges whose heights were less than or equal to the requested cut height.")
            Else
                t = ClusterAnalysisHelpers.BuildResultTableFromObjectMatrix($"Cluster Membership (k = {pMembershipClusterCount})", pResult.GetMembershipTable(pMembershipClusterCount))
                t.AddFootnote("Clusters were obtained by cutting the fitted tree to the requested number of clusters.")
            End If
            out.Add(t)

            If pResult.Standardization <> ClusterStandardizationMode.None Then
                t = ClusterAnalysisHelpers.BuildResultTableFromObjectMatrix("Preprocessing Constants", BuildHierarchicalPreprocessingTable())
                If pResult.Standardization = ClusterStandardizationMode.ZScores Then
                    t.AddFootnote("Location = variable mean; Scale = sample standard deviation used to standardize the active analysis data.")
                ElseIf pResult.Standardization = ClusterStandardizationMode.RangeZeroToOne Then
                    t.AddFootnote("Location = variable minimum; Scale = observed range (maximum minus minimum) used to rescale the active analysis data to [0, 1].")
                End If
                out.Add(t)
            End If

            Dim removedTable As Object(,) = BuildHierarchicalRemovedRowsTable()
            If removedTable IsNot Nothing Then
                t = ClusterAnalysisHelpers.BuildResultTableFromObjectMatrix("Rows Removed by Missing-Value Policy", removedTable)
                out.Add(t)
            End If

            Return out
        End Function

        Private Function BuildHierarchicalSettingsTable() As Object(,)
            Dim out(1, 7) As Object
            out(0, 0) = "Linkage"
            out(0, 1) = "DistanceMetric"
            out(0, 2) = "MinkowskiPower"
            out(0, 3) = "Standardization"
            out(0, 4) = "MissingValuePolicy"
            out(0, 5) = "MembershipDisplayMode"
            out(0, 6) = "MembershipClusterCount"
            out(0, 7) = "MembershipCutHeight"

            out(1, 0) = pLinkage.ToString()
            out(1, 1) = pDistanceMetric.ToString()
            out(1, 2) = pMinkowskiPower
            out(1, 3) = pStandardization.ToString()
            out(1, 4) = pMissingValuePolicy.ToString()
            out(1, 5) = pMembershipDisplayMode.ToString()
            out(1, 6) = pMembershipClusterCount
            out(1, 7) = pMembershipCutHeight
            Return out
        End Function

        Private Function BuildHierarchicalFitSummaryTable() As Object(,)
            Dim mergeSteps As Integer = If(pResult.MergeHeights Is Nothing, 0, pResult.MergeHeights.Length)
            Dim finalHeight As Object = Nothing
            If mergeSteps > 0 Then finalHeight = pResult.MergeHeights(mergeSteps - 1)

            Dim out(1, 4) As Object
            out(0, 0) = "ActiveObservations"
            out(0, 1) = "RemovedObservations"
            out(0, 2) = "MergeSteps"
            out(0, 3) = "FinalMergeHeight"
            out(0, 4) = "LeafCount"

            out(1, 0) = If(pResult.ActiveRowIndices Is Nothing, 0, pResult.ActiveRowIndices.Length)
            out(1, 1) = If(pResult.RemovedRowIndices Is Nothing, 0, pResult.RemovedRowIndices.Length)
            out(1, 2) = mergeSteps
            out(1, 3) = finalHeight
            out(1, 4) = If(pResult.LeafOrder Is Nothing, 0, pResult.LeafOrder.Length)
            Return out
        End Function

        Private Function BuildLeafOrderTable() As Object(,)
            Dim n As Integer = If(pResult.ActiveRowIndices Is Nothing, 0, pResult.ActiveRowIndices.Length)
            Dim out(Math.Max(n, 1), 2) As Object
            out(0, 0) = "DisplayPosition"
            out(0, 1) = "OriginalRow"
            out(0, 2) = "RowLabel"
            If n = 0 Then Return out

            Dim labelByRow As New Dictionary(Of Integer, String)
            If pResult.ActiveRowIndices IsNot Nothing AndAlso pResult.ActiveRowLabels IsNot Nothing Then
                For i As Integer = 0 To Math.Min(pResult.ActiveRowIndices.Length, pResult.ActiveRowLabels.Length) - 1
                    labelByRow(pResult.ActiveRowIndices(i)) = pResult.ActiveRowLabels(i)
                Next
            End If

            Dim displayOrder() As Integer = If(pResult.LeafOrder IsNot Nothing AndAlso pResult.LeafOrder.Length = n,
                                               pResult.LeafOrder,
                                               pResult.ActiveRowIndices)
            For i As Integer = 0 To n - 1
                Dim rowId As Integer = displayOrder(i)
                out(i + 1, 0) = i + 1
                out(i + 1, 1) = rowId
                out(i + 1, 2) = If(labelByRow.ContainsKey(rowId), CType(labelByRow(rowId), Object), CType($"Obs {rowId}", Object))
            Next
            Return out
        End Function

        Private Function BuildHierarchicalPreprocessingTable() As Object(,)
            Dim varCount As Integer = 0
            If pResult.VariableNames IsNot Nothing Then
                varCount = pResult.VariableNames.Length
            ElseIf pResult.StandardizationLocations IsNot Nothing Then
                varCount = pResult.StandardizationLocations.Length
            ElseIf pResult.StandardizationScales IsNot Nothing Then
                varCount = pResult.StandardizationScales.Length
            End If

            If varCount <= 0 Then
                Dim emptyOut(1, 2) As Object
                emptyOut(0, 0) = "Variable"
                emptyOut(0, 1) = "Location"
                emptyOut(0, 2) = "Scale"
                emptyOut(1, 0) = "(none)"
                emptyOut(1, 1) = Nothing
                emptyOut(1, 2) = Nothing
                Return emptyOut
            End If

            Dim out(varCount, 2) As Object
            out(0, 0) = "Variable"
            out(0, 1) = "Location"
            out(0, 2) = "Scale"
            For i As Integer = 0 To varCount - 1
                out(i + 1, 0) = If(pResult.VariableNames Is Nothing, CType($"Var {i + 1}", Object), pResult.VariableNames(i))
                out(i + 1, 1) = If(pResult.StandardizationLocations Is Nothing, CType(Nothing, Object), pResult.StandardizationLocations(i))
                out(i + 1, 2) = If(pResult.StandardizationScales Is Nothing, CType(Nothing, Object), pResult.StandardizationScales(i))
            Next
            Return out
        End Function

        Private Function BuildHierarchicalRemovedRowsTable() As Object(,)
            If pResult Is Nothing OrElse pResult.RemovedRowIndices Is Nothing OrElse pResult.RemovedRowIndices.Length = 0 Then
                Return Nothing
            End If

            Dim hasLabels As Boolean = (pResult.RemovedRowLabels IsNot Nothing AndAlso pResult.RemovedRowLabels.Length = pResult.RemovedRowIndices.Length)
            Dim out(pResult.RemovedRowIndices.Length, If(hasLabels, 2, 1)) As Object
            out(0, 0) = "OriginalRow"
            If hasLabels Then
                out(0, 1) = "RowLabel"
                out(0, 2) = "Reason"
            Else
                out(0, 1) = "Reason"
            End If
            For i As Integer = 0 To pResult.RemovedRowIndices.Length - 1
                out(i + 1, 0) = pResult.RemovedRowIndices(i)
                If hasLabels Then
                    out(i + 1, 1) = pResult.RemovedRowLabels(i)
                    out(i + 1, 2) = "Removed before fitting because at least one analysis variable was missing or non-finite."
                Else
                    out(i + 1, 1) = "Removed before fitting because at least one analysis variable was missing or non-finite."
                End If
            Next
            Return out
        End Function

        ''' <summary>
        ''' Fits the agglomerative hierarchical clustering model using the current data and settings.
        ''' </summary>
        ''' <exception cref="System.ArgumentException">
        ''' Thrown when the inputs are inconsistent or when the selected linkage and distance combination is not supported.
        ''' </exception>
        Public Sub Fit()
            If pData Is Nothing Then CoreServices.Errors.LogAndThrow(New ArgumentException("No data supplied. Call dataInputs() first."))

            If (pLinkage = HierarchicalLinkageMethod.Centroid OrElse
                pLinkage = HierarchicalLinkageMethod.Median OrElse
                pLinkage = HierarchicalLinkageMethod.Ward) AndAlso
               (pDistanceMetric <> HierarchicalDistanceMetric.Euclidean AndAlso pDistanceMetric <> HierarchicalDistanceMetric.SquaredEuclidean) Then

                CoreServices.Errors.LogAndThrow(New ArgumentException("Centroid, median, and Ward linkage require Euclidean or squared Euclidean distance."))
            End If

            Dim prepared As ClusterPreparedData = ClusterAnalysisHelpers.PrepareData(pData, pRowLabels, pVarNames, pStandardization, pMissingValuePolicy)
            Dim n As Integer = prepared.WorkingData.GetUpperBound(0) + 1
            If n < 2 Then CoreServices.Errors.LogAndThrow(New ArgumentException("Hierarchical clustering requires at least two active observations."))

            Dim mergeLeft(n - 2) As Integer
            Dim mergeRight(n - 2) As Integer
            Dim mergeHeights(n - 2) As Double
            Dim mergeSizes(n - 2) As Integer
            Dim leafOrder() As Integer = Nothing

            Select Case pLinkage
                Case HierarchicalLinkageMethod.SingleLinkage,
                     HierarchicalLinkageMethod.Complete,
                     HierarchicalLinkageMethod.Average,
                     HierarchicalLinkageMethod.WeightedAverage
                    FitViaDistanceUpdates(prepared.WorkingData, mergeLeft, mergeRight, mergeHeights, mergeSizes, leafOrder)
                Case Else
                    FitViaCentroidModels(prepared.WorkingData, mergeLeft, mergeRight, mergeHeights, mergeSizes, leafOrder)
            End Select

            Dim leafOrderOriginal() As Integer = Nothing
            If leafOrder IsNot Nothing Then
                ReDim leafOrderOriginal(leafOrder.Length - 1)
                For i As Integer = 0 To leafOrder.Length - 1
                    leafOrderOriginal(i) = prepared.ActiveOriginalIndices(leafOrder(i) - 1)
                Next
            End If

            Dim result As New HierarchicalClusterResult With {
                .ActiveRowIndices = prepared.ActiveOriginalIndices,
                .ActiveRowLabels = prepared.ActiveRowLabels,
                .RemovedRowIndices = prepared.RemovedOriginalIndices,
                .RemovedRowLabels = prepared.RemovedRowLabels,
                .VariableNames = prepared.VariableNames,
                .LinkageMethod = pLinkage,
                .DistanceMetric = pDistanceMetric,
                .MinkowskiPower = pMinkowskiPower,
                .Standardization = pStandardization,
                .MissingValuePolicy = pMissingValuePolicy,
                .MergeLeftClusterIds = mergeLeft,
                .MergeRightClusterIds = mergeRight,
                .MergeHeights = mergeHeights,
                .MergeClusterSizes = mergeSizes,
                .LeafOrder = leafOrderOriginal,
                .StandardizationLocations = prepared.ColumnLocations,
                .StandardizationScales = prepared.ColumnScales
            }

            pResult = result
        End Sub

        ''' <summary>
        ''' Cuts the fitted hierarchical tree to a requested number of clusters.
        ''' </summary>
        ''' <param name="numberOfClusters">Requested number of clusters.</param>
        ''' <returns>An array of cluster labels for the active observations.</returns>
        Public Function GetMembershipByClusterCount(numberOfClusters As Integer) As Integer()
            If pResult Is Nothing Then CoreServices.Errors.LogAndThrow(New InvalidOperationException("The model has not been fitted yet."))
            Return pResult.GetMembershipByClusterCount(numberOfClusters)
        End Function

        ''' <summary>
        ''' Cuts the fitted hierarchical tree at a specified merge height.
        ''' </summary>
        ''' <param name="cutHeight">Merge-height threshold. Merges at or below this height are applied.</param>
        ''' <returns>An array of cluster labels for the active observations.</returns>
        Public Function GetMembershipByHeight(cutHeight As Double) As Integer()
            If pResult Is Nothing Then CoreServices.Errors.LogAndThrow(New InvalidOperationException("The model has not been fitted yet."))
            Return pResult.GetMembershipByHeight(cutHeight)
        End Function

        Private Sub FitViaDistanceUpdates(data(,) As Double,
                                          ByRef mergeLeft() As Integer,
                                          ByRef mergeRight() As Integer,
                                          ByRef mergeHeights() As Double,
                                          ByRef mergeSizes() As Integer,
                                          ByRef leafOrder() As Integer)

            Dim n As Integer = data.GetUpperBound(0) + 1
            Dim maxId As Integer = 2 * n
            Dim distances(maxId, maxId) As Double
            For i As Integer = 0 To maxId
                For j As Integer = 0 To maxId
                    distances(i, j) = Double.PositiveInfinity
                Next
            Next

            Dim nodes As New Dictionary(Of Integer, ClusterNode)
            Dim active As New List(Of Integer)
            For i As Integer = 0 To n - 1
                Dim node As ClusterNode = ClusterAnalysisHelpers.CreateLeafNode(i + 1, data, i)
                nodes(node.Id) = node
                active.Add(node.Id)
            Next

            For i As Integer = 1 To n
                For j As Integer = i + 1 To n
                    Dim d As Double = ClusterAnalysisHelpers.ComputeObservationDistance(data, i - 1, j - 1, pDistanceMetric, pMinkowskiPower)
                    distances(i, j) = d
                    distances(j, i) = d
                Next
            Next

            For stepIndex As Integer = 0 To n - 2
                Dim bestI As Integer = -1
                Dim bestJ As Integer = -1
                Dim bestD As Double = Double.PositiveInfinity

                For a As Integer = 0 To active.Count - 2
                    For b As Integer = a + 1 To active.Count - 1
                        Dim idA As Integer = active(a)
                        Dim idB As Integer = active(b)
                        Dim d As Double = distances(idA, idB)
                        If d < bestD OrElse (d = bestD AndAlso ClusterAnalysisHelpers.IsPreferredPair(idA, idB, bestI, bestJ, nodes)) Then
                            bestD = d
                            bestI = idA
                            bestJ = idB
                        End If
                    Next
                Next

                Dim leftNode As ClusterNode = nodes(bestI)
                Dim rightNode As ClusterNode = nodes(bestJ)
                ClusterAnalysisHelpers.CanonicalizeNodes(leftNode, rightNode)
                bestI = leftNode.Id
                bestJ = rightNode.Id

                Dim newId As Integer = n + stepIndex + 1
                Dim mergedNode As ClusterNode = ClusterAnalysisHelpers.MergeNodes(leftNode, rightNode, newId, False)
                nodes(newId) = mergedNode

                mergeLeft(stepIndex) = bestI
                mergeRight(stepIndex) = bestJ
                mergeHeights(stepIndex) = bestD
                mergeSizes(stepIndex) = mergedNode.Size

                For Each otherId As Integer In active.ToArray()
                    If otherId = bestI OrElse otherId = bestJ Then Continue For
                    Dim newDist As Double
                    Dim dIK As Double = distances(bestI, otherId)
                    Dim dJK As Double = distances(bestJ, otherId)
                    Select Case pLinkage
                        Case HierarchicalLinkageMethod.SingleLinkage
                            newDist = Math.Min(dIK, dJK)
                        Case HierarchicalLinkageMethod.Complete
                            newDist = Math.Max(dIK, dJK)
                        Case HierarchicalLinkageMethod.Average
                            newDist = (leftNode.Size * dIK + rightNode.Size * dJK) / (leftNode.Size + rightNode.Size)
                        Case Else
                            newDist = 0.5 * (dIK + dJK)
                    End Select
                    distances(newId, otherId) = newDist
                    distances(otherId, newId) = newDist
                Next

                active.Remove(bestI)
                active.Remove(bestJ)
                active.Add(newId)
            Next

            Dim finalNode As ClusterNode = nodes(2 * n - 1)
            leafOrder = finalNode.LeafOrder.Select(Function(idx) idx + 1).ToArray()
        End Sub

        Private Sub FitViaCentroidModels(data(,) As Double,
                                         ByRef mergeLeft() As Integer,
                                         ByRef mergeRight() As Integer,
                                         ByRef mergeHeights() As Double,
                                         ByRef mergeSizes() As Integer,
                                         ByRef leafOrder() As Integer)

            Dim n As Integer = data.GetUpperBound(0) + 1
            Dim nodes As New Dictionary(Of Integer, ClusterNode)
            Dim active As New List(Of Integer)
            For i As Integer = 0 To n - 1
                Dim node As ClusterNode = ClusterAnalysisHelpers.CreateLeafNode(i + 1, data, i)
                nodes(node.Id) = node
                active.Add(node.Id)
            Next

            For stepIndex As Integer = 0 To n - 2
                Dim bestI As Integer = -1
                Dim bestJ As Integer = -1
                Dim bestD As Double = Double.PositiveInfinity

                For a As Integer = 0 To active.Count - 2
                    For b As Integer = a + 1 To active.Count - 1
                        Dim nodeA As ClusterNode = nodes(active(a))
                        Dim nodeB As ClusterNode = nodes(active(b))
                        Dim d As Double = ClusterAnalysisHelpers.ComputeClusterDistance(nodeA, nodeB, pLinkage, pDistanceMetric)
                        If d < bestD OrElse (d = bestD AndAlso ClusterAnalysisHelpers.IsPreferredPair(nodeA.Id, nodeB.Id, bestI, bestJ, nodes)) Then
                            bestD = d
                            bestI = nodeA.Id
                            bestJ = nodeB.Id
                        End If
                    Next
                Next

                Dim leftNode As ClusterNode = nodes(bestI)
                Dim rightNode As ClusterNode = nodes(bestJ)
                ClusterAnalysisHelpers.CanonicalizeNodes(leftNode, rightNode)
                bestI = leftNode.Id
                bestJ = rightNode.Id

                Dim newId As Integer = n + stepIndex + 1
                Dim mergedNode As ClusterNode = ClusterAnalysisHelpers.MergeNodes(leftNode, rightNode, newId, pLinkage = HierarchicalLinkageMethod.Median)
                nodes(newId) = mergedNode

                mergeLeft(stepIndex) = bestI
                mergeRight(stepIndex) = bestJ
                mergeHeights(stepIndex) = bestD
                mergeSizes(stepIndex) = mergedNode.Size

                active.Remove(bestI)
                active.Remove(bestJ)
                active.Add(newId)
            Next

            Dim finalNode As ClusterNode = nodes(2 * n - 1)
            leafOrder = finalNode.LeafOrder.Select(Function(idx) idx + 1).ToArray()
        End Sub
    End Class


    Friend Class DendrogramNode
        Public Id As Integer
        Public Height As Double
        Public AnchorX As Double
        Public Left As DendrogramNode
        Public Right As DendrogramNode
    End Class

    Friend Class SingleKMeansRunResult
        Public Assignments As Integer()
        Public Centers As Double(,)
        Public AssignedDistances As Double()
        Public Iterations As Integer
        Public Converged As Boolean
        Public ObjectiveValue As Double
        Public WithinClusterSSByCluster As Double()
    End Class

    Friend Class ClusterPreparedData
        Public WorkingData(,) As Double
        Public ActiveOriginalData(,) As Double
        Public ActiveRowLabels() As String
        Public VariableNames() As String
        Public ActiveOriginalIndices() As Integer
        Public RemovedOriginalIndices() As Integer
        Public RemovedRowLabels() As String
        Public ColumnLocations() As Double
        Public ColumnScales() As Double
        Public Standardization As ClusterStandardizationMode
    End Class

    Friend Class ClusterNode
        Public Id As Integer
        Public Size As Integer
        Public LeafOrder As List(Of Integer)
        Public TrueCentroid() As Double
        Public LinkageCentroid() As Double
    End Class

    Friend Module ClusterAnalysisHelpers

        Friend Function BuildResultTableFromObjectMatrix(title As String, table(,) As Object) As ResultTable
            Dim t As New ResultTable
            t.AddTitle(title)

            If table Is Nothing Then Return t

            Dim nRows As Integer = table.GetUpperBound(0) + 1
            Dim nCols As Integer = table.GetUpperBound(1) + 1
            Dim hdr(nCols - 1) As String

            For j As Integer = 0 To nCols - 1
                hdr(j) = If(table(0, j) Is Nothing, String.Empty, CStr(table(0, j)))
            Next
            t.AddHeaderTopRow(hdr)

            If nRows > 1 Then
                Dim body(nRows - 2, nCols - 1) As Object
                For i As Integer = 1 To nRows - 1
                    For j As Integer = 0 To nCols - 1
                        body(i - 1, j) = table(i, j)
                    Next
                Next
                t.SetBody(body)
            Else
                Dim body(0, nCols - 1) As Object
                For j As Integer = 0 To nCols - 1
                    body(0, j) = Nothing
                Next
                t.SetBody(body)
            End If

            Return t
        End Function

        Public Function BuildDendrogramLayout(result As HierarchicalClusterResult,
                                              heightMode As DendrogramHeightMode,
                                              orientation As DendrogramOrientation,
                                              Optional cutMode As HierarchicalMembershipDisplayMode = HierarchicalMembershipDisplayMode.ByClusterCount,
                                              Optional membershipClusterCount As Integer = 3,
                                              Optional membershipCutHeight As Double = 0.0) As DendrogramLayout

            If result Is Nothing Then CoreServices.Errors.LogAndThrow(New ArgumentNullException(NameOf(result)))
            If result.ActiveRowIndices Is Nothing OrElse result.ActiveRowLabels Is Nothing Then
                CoreServices.Errors.LogAndThrow(New ArgumentException("The hierarchical clustering result does not contain active rows."))
            End If

            Dim n As Integer = result.ActiveRowIndices.Length
            If n = 0 Then CoreServices.Errors.LogAndThrow(New ArgumentException("The hierarchical clustering result does not contain any active rows."))

            Dim originalToDisplayX As New Dictionary(Of Integer, Double)
            Dim leafOriginalOrder() As Integer
            Dim leafLabels() As String

            If result.LeafOrder IsNot Nothing AndAlso result.LeafOrder.Length = n Then
                leafOriginalOrder = CType(result.LeafOrder.Clone(), Integer())
                ReDim leafLabels(n - 1)
                For i As Integer = 0 To n - 1
                    originalToDisplayX(result.LeafOrder(i)) = i + 1
                    Dim idx As Integer = Array.IndexOf(result.ActiveRowIndices, result.LeafOrder(i))
                    If idx >= 0 AndAlso idx < result.ActiveRowLabels.Length Then
                        leafLabels(i) = result.ActiveRowLabels(idx)
                    Else
                        leafLabels(i) = CStr(result.LeafOrder(i))
                    End If
                Next
            Else
                ReDim leafOriginalOrder(n - 1)
                ReDim leafLabels(n - 1)
                For i As Integer = 0 To n - 1
                    leafOriginalOrder(i) = result.ActiveRowIndices(i)
                    leafLabels(i) = result.ActiveRowLabels(i)
                    originalToDisplayX(result.ActiveRowIndices(i)) = i + 1
                Next
            End If

            Dim nodes As New Dictionary(Of Integer, DendrogramNode)
            For leafId As Integer = 1 To n
                Dim originalRow As Integer = result.ActiveRowIndices(leafId - 1)
                nodes(leafId) = New DendrogramNode With {
                    .Id = leafId,
                    .Height = 0.0,
                    .AnchorX = originalToDisplayX(originalRow)
                }
            Next

            Dim maxHeight As Double = 0.0
            For stepIndex As Integer = 0 To n - 2
                Dim newId As Integer = n + stepIndex + 1
                Dim leftNode As DendrogramNode = nodes(result.MergeLeftClusterIds(stepIndex))
                Dim rightNode As DendrogramNode = nodes(result.MergeRightClusterIds(stepIndex))
                If leftNode.AnchorX > rightNode.AnchorX Then
                    Dim tmp As DendrogramNode = leftNode
                    leftNode = rightNode
                    rightNode = tmp
                End If

                Dim height As Double = If(heightMode = DendrogramHeightMode.StepLevels,
                                          CDbl(stepIndex + 1),
                                          If(result.MergeHeights Is Nothing, CDbl(stepIndex + 1), result.MergeHeights(stepIndex)))

                Dim node As New DendrogramNode With {
                    .Id = newId,
                    .Height = height,
                    .AnchorX = 0.5 * (leftNode.AnchorX + rightNode.AnchorX),
                    .Left = leftNode,
                    .Right = rightNode
                }
                nodes(newId) = node
                If height > maxHeight Then maxHeight = height
            Next

            Dim root As DendrogramNode = nodes(2 * n - 1)
            Dim xPath As New List(Of Double)
            Dim yPath As New List(Of Double)
            AppendForwardDendrogramPath(root, xPath, yPath)

            Dim transformedX As New List(Of Double)(xPath.Count)
            Dim transformedY As New List(Of Double)(yPath.Count)
            For i As Integer = 0 To xPath.Count - 1
                Dim tx As Double, ty As Double
                TransformDendrogramCoordinates(xPath(i), yPath(i), maxHeight, n, orientation, tx, ty)
                transformedX.Add(tx)
                transformedY.Add(ty)
            Next

            Dim leafX(n - 1) As Double
            Dim leafY(n - 1) As Double
            For i As Integer = 0 To n - 1
                Dim tx As Double, ty As Double
                TransformDendrogramCoordinates(i + 1, 0.0, maxHeight, n, orientation, tx, ty)
                leafX(i) = tx
                leafY(i) = ty
            Next

            Dim clusterPathsX As New List(Of Double())
            Dim clusterPathsY As New List(Of Double())
            Dim clusterRootIds As List(Of Integer) = BuildClusterRootIds(n,
                                                                         result.MergeLeftClusterIds,
                                                                         result.MergeRightClusterIds,
                                                                         cutMode,
                                                                         membershipClusterCount,
                                                                         membershipCutHeight,
                                                                         result.MergeHeights)
            If clusterRootIds IsNot Nothing Then
                For Each rootId As Integer In clusterRootIds
                    If Not nodes.ContainsKey(rootId) Then Continue For
                    Dim clusterRoot As DendrogramNode = nodes(rootId)
                    If clusterRoot.Left Is Nothing OrElse clusterRoot.Right Is Nothing Then Continue For

                    Dim cx As New List(Of Double)
                    Dim cy As New List(Of Double)
                    AppendClosedDendrogramPath(clusterRoot, cx, cy)

                    If cx.Count > 0 Then
                        Dim txArr(cx.Count - 1) As Double
                        Dim tyArr(cy.Count - 1) As Double
                        For i As Integer = 0 To cx.Count - 1
                            Dim tx As Double, ty As Double
                            TransformDendrogramCoordinates(cx(i), cy(i), maxHeight, n, orientation, tx, ty)
                            txArr(i) = tx
                            tyArr(i) = ty
                        Next
                        clusterPathsX.Add(txArr)
                        clusterPathsY.Add(tyArr)
                    End If
                Next
            End If

            Dim cutDisplayHeight As Double = ResolveCutDisplayHeight(heightMode,
                                                                     result.MergeHeights,
                                                                     maxHeight,
                                                                     n,
                                                                     cutMode,
                                                                     membershipClusterCount,
                                                                     membershipCutHeight)
            Dim cutLineX() As Double = Nothing
            Dim cutLineY() As Double = Nothing
            If Not Double.IsNaN(cutDisplayHeight) Then
                Dim tx1 As Double, ty1 As Double, tx2 As Double, ty2 As Double
                TransformDendrogramCoordinates(0.5, cutDisplayHeight, maxHeight, n, orientation, tx1, ty1)
                TransformDendrogramCoordinates(n + 0.5, cutDisplayHeight, maxHeight, n, orientation, tx2, ty2)
                cutLineX = New Double() {tx1, tx2}
                cutLineY = New Double() {ty1, ty2}
            End If

            Return New DendrogramLayout With {
                .HeightMode = heightMode,
                .Orientation = orientation,
                .PolylineX = transformedX.ToArray(),
                .PolylineY = transformedY.ToArray(),
                .LeafX = leafX,
                .LeafY = leafY,
                .LeafLabels = leafLabels,
                .LeafOriginalRowOrder = leafOriginalOrder,
                .MaximumHeight = maxHeight,
                .LeafCount = n,
                .ClusterPolylineX = clusterPathsX,
                .ClusterPolylineY = clusterPathsY,
                .CutDisplayHeight = If(Double.IsNaN(cutDisplayHeight), CType(Nothing, Double?), cutDisplayHeight),
                .CutLineX = cutLineX,
                .CutLineY = cutLineY
            }
        End Function

        Private Function BuildClusterRootIds(n As Integer,
                                             mergeLeft() As Integer,
                                             mergeRight() As Integer,
                                             cutMode As HierarchicalMembershipDisplayMode,
                                             membershipClusterCount As Integer,
                                             membershipCutHeight As Double,
                                             mergeHeights() As Double) As List(Of Integer)

            If n <= 0 Then Return New List(Of Integer)

            Dim mergesToApply As Integer
            If cutMode = HierarchicalMembershipDisplayMode.ByHeight Then
                mergesToApply = 0
                If mergeHeights IsNot Nothing Then
                    For i As Integer = 0 To mergeHeights.Length - 1
                        If mergeHeights(i) <= membershipCutHeight Then
                            mergesToApply += 1
                        Else
                            Exit For
                        End If
                    Next
                End If
            Else
                Dim k As Integer = Math.Max(1, Math.Min(membershipClusterCount, n))
                mergesToApply = n - k
            End If

            Dim active As New List(Of Integer)
            For i As Integer = 1 To n
                active.Add(i)
            Next

            For stepIndex As Integer = 0 To mergesToApply - 1
                Dim newId As Integer = n + stepIndex + 1
                active.Remove(mergeLeft(stepIndex))
                active.Remove(mergeRight(stepIndex))
                active.Add(newId)
            Next

            active.Sort()
            Return active
        End Function

        Private Function ResolveCutDisplayHeight(heightMode As DendrogramHeightMode,
                                         mergeHeights() As Double,
                                         maxHeight As Double,
                                         leafCount As Integer,
                                         cutMode As HierarchicalMembershipDisplayMode,
                                         membershipClusterCount As Integer,
                                         membershipCutHeight As Double) As Double

            If leafCount <= 0 Then Return Double.NaN

            If cutMode = HierarchicalMembershipDisplayMode.ByHeight Then
                If heightMode = DendrogramHeightMode.StepLevels Then
                    Dim mergeCountAtCut As Integer = 0
                    If mergeHeights IsNot Nothing Then
                        For i As Integer = 0 To mergeHeights.Length - 1
                            If mergeHeights(i) <= membershipCutHeight Then
                                mergeCountAtCut += 1
                            Else
                                Exit For
                            End If
                        Next
                    End If
                    Return mergeCountAtCut + 0.5
                End If
                Return membershipCutHeight
            End If

            Dim k As Integer = Math.Max(1, Math.Min(membershipClusterCount, leafCount))
            Dim mergesToApply As Integer = leafCount - k

            If heightMode = DendrogramHeightMode.StepLevels Then
                If k = 1 Then
                    Return maxHeight + 0.5
                End If
                Return mergesToApply + 0.5
            End If

            If mergeHeights Is Nothing OrElse mergeHeights.Length = 0 Then
                Return 0.5
            End If

            If k >= leafCount Then
                Return mergeHeights(0) / 2.0
            End If

            If k <= 1 Then
                If mergeHeights.Length = 1 Then Return mergeHeights(0) * 1.05
                Dim delta As Double = mergeHeights(mergeHeights.Length - 1) - mergeHeights(mergeHeights.Length - 2)
                If delta <= 0 Then delta = Math.Max(1.0, 0.05 * Math.Abs(mergeHeights(mergeHeights.Length - 1)))
                Return mergeHeights(mergeHeights.Length - 1) + 0.5 * delta
            End If

            Dim lowHeight As Double = If(mergesToApply > 0, mergeHeights(mergesToApply - 1), 0.0)
            Dim highHeight As Double = mergeHeights(mergesToApply)
            If highHeight < lowHeight Then
                Dim tmp As Double = lowHeight
                lowHeight = highHeight
                highHeight = tmp
            End If
            If highHeight = lowHeight Then Return highHeight
            Return 0.5 * (lowHeight + highHeight)
        End Function

        Public Function GetClusterSeriesColor(index As Integer) As Integer
            Dim palette() As Integer = {
                RGB(0, 114, 178),
                RGB(213, 94, 0),
                RGB(0, 158, 115),
                RGB(204, 121, 167),
                RGB(230, 159, 0),
                RGB(86, 180, 233),
                RGB(240, 228, 66),
                RGB(0, 0, 0)
            }
            Return palette(index Mod palette.Length)
        End Function

        Private Sub AppendForwardDendrogramPath(node As DendrogramNode,
                                                xPath As List(Of Double),
                                                yPath As List(Of Double))
            If node Is Nothing Then Return
            If node.Left Is Nothing OrElse node.Right Is Nothing Then
                AppendPoint(xPath, yPath, node.AnchorX, 0.0)
                Return
            End If

            AppendForwardDendrogramPath(node.Left, xPath, yPath)
            AppendPoint(xPath, yPath, node.Left.AnchorX, node.Height)
            AppendPoint(xPath, yPath, node.Right.AnchorX, node.Height)
            AppendPoint(xPath, yPath, node.Right.AnchorX, node.Right.Height)
            AppendClosedDendrogramPath(node.Right, xPath, yPath)
            AppendPoint(xPath, yPath, node.Right.AnchorX, node.Height)
            AppendPoint(xPath, yPath, node.AnchorX, node.Height)
        End Sub

        Private Sub AppendClosedDendrogramPath(node As DendrogramNode,
                                               xPath As List(Of Double),
                                               yPath As List(Of Double))
            If node Is Nothing Then Return
            If node.Left Is Nothing OrElse node.Right Is Nothing Then Return

            AppendPoint(xPath, yPath, node.Left.AnchorX, node.Height)
            AppendPoint(xPath, yPath, node.Left.AnchorX, node.Left.Height)
            AppendClosedDendrogramPath(node.Left, xPath, yPath)
            AppendPoint(xPath, yPath, node.Left.AnchorX, node.Height)
            AppendPoint(xPath, yPath, node.Right.AnchorX, node.Height)
            AppendPoint(xPath, yPath, node.Right.AnchorX, node.Right.Height)
            AppendClosedDendrogramPath(node.Right, xPath, yPath)
            AppendPoint(xPath, yPath, node.Right.AnchorX, node.Height)
            AppendPoint(xPath, yPath, node.AnchorX, node.Height)
        End Sub

        Private Sub AppendPoint(xPath As List(Of Double),
                                yPath As List(Of Double),
                                x As Double,
                                y As Double)
            If xPath.Count > 0 Then
                Dim lastIndex As Integer = xPath.Count - 1
                If xPath(lastIndex) = x AndAlso yPath(lastIndex) = y Then Return
            End If
            xPath.Add(x)
            yPath.Add(y)
        End Sub

        Private Sub TransformDendrogramCoordinates(sourceX As Double,
                                                   sourceY As Double,
                                                   maxHeight As Double,
                                                   leafCount As Integer,
                                                   orientation As DendrogramOrientation,
                                                   ByRef targetX As Double,
                                                   ByRef targetY As Double)
            Select Case orientation
                Case DendrogramOrientation.Top
                    targetX = sourceX
                    targetY = sourceY
                Case DendrogramOrientation.Bottom
                    targetX = sourceX
                    targetY = maxHeight - sourceY
                Case DendrogramOrientation.Left
                    targetX = maxHeight - sourceY
                    targetY = leafCount + 1 - sourceX
                Case Else
                    targetX = sourceY
                    targetY = leafCount + 1 - sourceX
            End Select
        End Sub

        Public Function AxisPadding(minValue As Double,
                                    maxValue As Double,
                                    referenceSize As Double) As Double
            Dim span As Double = maxValue - minValue
            If span > 0 Then Return 0.05 * span
            If referenceSize > 0 Then Return 0.1 * referenceSize
            Return 0.5
        End Function

        Public Function CreateRandom(seed As Integer) As Random
            Return CoreServices.AnalysisDefaults.CreateRandom(seed)
        End Function

        Public Function PrepareData(data(,) As Double,
                                    rowLabels() As String,
                                    varNames() As String,
                                    standardization As ClusterStandardizationMode,
                                    missingPolicy As ClusterMissingValuePolicy) As ClusterPreparedData

            MultivariateInputHelpers.ValidateRectangularData(data)
            Dim n As Integer = data.GetUpperBound(0) + 1
            Dim p As Integer = data.GetUpperBound(1) + 1

            Dim finalRowLabels() As String = MultivariateInputHelpers.NormalizeRowLabels(rowLabels, n, defaultPrefix:="Obs",
                                                                                         mismatchMessage:="The number of row labels does not match the number of observations.")
            Dim finalVarNames() As String = MultivariateInputHelpers.NormalizeVarNames(varNames, p, defaultPrefix:="Var",
                                                                                       mismatchMessage:="The number of variable names does not match the number of columns.", useSpaceSeparator:=True)


            Dim keepRow(n - 1) As Boolean
            Dim activeCount As Integer = 0
            Dim removedIndices As New List(Of Integer)
            Dim removedLabels As New List(Of String)

            For i As Integer = 0 To n - 1
                Dim hasMissing As Boolean = False
                For j As Integer = 0 To p - 1
                    If Double.IsNaN(data(i, j)) OrElse Double.IsInfinity(data(i, j)) Then
                        hasMissing = True
                        Exit For
                    End If
                Next

                If hasMissing Then
                    If missingPolicy = ClusterMissingValuePolicy.ErrorOnMissing Then
                        CoreServices.Errors.LogAndThrow(New ArgumentException($"Missing or non-finite numeric value found in row {i + 1}."))
                    End If
                    keepRow(i) = False
                    removedIndices.Add(i + 1)
                    removedLabels.Add(finalRowLabels(i))
                Else
                    keepRow(i) = True
                    activeCount += 1
                End If
            Next

            If activeCount = 0 Then CoreServices.Errors.LogAndThrow(New ArgumentException("No complete observations remain after preprocessing."))

            Dim activeOriginalData(activeCount - 1, p - 1) As Double
            Dim workingData(activeCount - 1, p - 1) As Double
            Dim activeRowLabels(activeCount - 1) As String
            Dim activeOriginalIndices(activeCount - 1) As Integer

            Dim outRow As Integer = 0
            For i As Integer = 0 To n - 1
                If Not keepRow(i) Then Continue For
                activeRowLabels(outRow) = finalRowLabels(i)
                activeOriginalIndices(outRow) = i + 1
                For j As Integer = 0 To p - 1
                    activeOriginalData(outRow, j) = data(i, j)
                Next
                outRow += 1
            Next

            Dim locations(p - 1) As Double
            Dim scales(p - 1) As Double
            ComputeStandardizationParameters(activeOriginalData, standardization, locations, scales)

            For i As Integer = 0 To activeCount - 1
                For j As Integer = 0 To p - 1
                    workingData(i, j) = TransformValue(activeOriginalData(i, j), standardization, locations(j), scales(j))
                Next
            Next

            Dim prepared As New ClusterPreparedData
            prepared.WorkingData = workingData
            prepared.ActiveOriginalData = activeOriginalData
            prepared.ActiveRowLabels = activeRowLabels
            prepared.VariableNames = finalVarNames
            prepared.ActiveOriginalIndices = activeOriginalIndices
            prepared.RemovedOriginalIndices = removedIndices.ToArray()
            prepared.RemovedRowLabels = removedLabels.ToArray()
            prepared.ColumnLocations = locations
            prepared.ColumnScales = scales
            prepared.Standardization = standardization
            Return prepared
        End Function

        Public Function PreparePredictionData(data(,) As Double,
                                             varNames() As String,
                                             standardization As ClusterStandardizationMode,
                                             locations() As Double,
                                             scales() As Double) As ClusterPreparedData
            ValidateRectangularData(data)
            Dim n As Integer = data.GetUpperBound(0) + 1
            Dim p As Integer = data.GetUpperBound(1) + 1
            If varNames IsNot Nothing AndAlso varNames.Length <> p Then
                CoreServices.Errors.LogAndThrow(New ArgumentException("The new data matrix does not have the expected number of variables."))
            End If

            Dim workingData(n - 1, p - 1) As Double
            For i As Integer = 0 To n - 1
                For j As Integer = 0 To p - 1
                    If Double.IsNaN(data(i, j)) OrElse Double.IsInfinity(data(i, j)) Then
                        CoreServices.Errors.LogAndThrow(New ArgumentException($"Missing or non-finite numeric value found in new-data row {i + 1}."))
                    End If
                    workingData(i, j) = TransformValue(data(i, j), standardization, locations(j), scales(j))
                Next
            Next

            Dim prepared As New ClusterPreparedData
            prepared.WorkingData = workingData
            prepared.VariableNames = NormalizeVarNames(varNames, p)
            prepared.ColumnLocations = CType(locations.Clone(), Double())
            prepared.ColumnScales = CType(scales.Clone(), Double())
            prepared.Standardization = standardization
            Return prepared
        End Function

        Public Function StandardizeExternalCenters(centersOriginal(,) As Double,
                                                  prepared As ClusterPreparedData) As Double(,)
            ValidateRectangularData(centersOriginal)
            Dim k As Integer = centersOriginal.GetUpperBound(0) + 1
            Dim p As Integer = centersOriginal.GetUpperBound(1) + 1
            If p <> prepared.WorkingData.GetUpperBound(1) + 1 Then
                CoreServices.Errors.LogAndThrow(New ArgumentException("The supplied centers do not have the expected number of variables."))
            End If
            Dim output(k - 1, p - 1) As Double
            For i As Integer = 0 To k - 1
                For j As Integer = 0 To p - 1
                    output(i, j) = TransformValue(centersOriginal(i, j), prepared.Standardization, prepared.ColumnLocations(j), prepared.ColumnScales(j))
                Next
            Next
            Return output
        End Function

        Public Function UnstandardizeCenters(centersWorking(,) As Double,
                                             prepared As ClusterPreparedData) As Double(,)
            ValidateRectangularData(centersWorking)
            Dim k As Integer = centersWorking.GetUpperBound(0) + 1
            Dim p As Integer = centersWorking.GetUpperBound(1) + 1
            Dim output(k - 1, p - 1) As Double
            For i As Integer = 0 To k - 1
                For j As Integer = 0 To p - 1
                    output(i, j) = InverseTransformValue(centersWorking(i, j), prepared.Standardization, prepared.ColumnLocations(j), prepared.ColumnScales(j))
                Next
            Next
            Return output
        End Function

        Public Function ColumnMeans(data(,) As Double) As Double()
            Dim n As Integer = data.GetUpperBound(0) + 1
            Dim p As Integer = data.GetUpperBound(1) + 1
            Dim means(p - 1) As Double
            For j As Integer = 0 To p - 1
                Dim s As Double = 0
                For i As Integer = 0 To n - 1
                    s += data(i, j)
                Next
                means(j) = s / n
            Next
            Return means
        End Function

        Public Function ComputeTotalSS(data(,) As Double) As Double
            Dim means() As Double = ColumnMeans(data)
            Dim n As Integer = data.GetUpperBound(0) + 1
            Dim p As Integer = data.GetUpperBound(1) + 1
            Dim total As Double = 0
            For i As Integer = 0 To n - 1
                For j As Integer = 0 To p - 1
                    Dim d As Double = data(i, j) - means(j)
                    total += d * d
                Next
            Next
            Return total
        End Function

        Public Function InitializeForgy(data(,) As Double, k As Integer, rng As Random) As Double(,)
            Dim n As Integer = data.GetUpperBound(0) + 1
            Dim p As Integer = data.GetUpperBound(1) + 1
            Dim picks As Integer() = Enumerable.Range(0, n).OrderBy(Function(x) rng.NextDouble()).Take(k).ToArray()
            Dim centers(k - 1, p - 1) As Double
            For c As Integer = 0 To k - 1
                For j As Integer = 0 To p - 1
                    centers(c, j) = data(picks(c), j)
                Next
            Next
            Return centers
        End Function

        Public Function InitializeRandomPartition(data(,) As Double, k As Integer, rng As Random) As Double(,)
            Dim n As Integer = data.GetUpperBound(0) + 1
            Dim p As Integer = data.GetUpperBound(1) + 1
            Dim assignments(n - 1) As Integer

            For i As Integer = 0 To n - 1
                assignments(i) = (i Mod k) + 1
            Next
            assignments = assignments.OrderBy(Function(x) rng.NextDouble()).ToArray()

            Dim centers(k - 1, p - 1) As Double
            Dim counts(k - 1) As Integer
            For i As Integer = 0 To n - 1
                Dim c As Integer = assignments(i) - 1
                counts(c) += 1
                For j As Integer = 0 To p - 1
                    centers(c, j) += data(i, j)
                Next
            Next
            For c As Integer = 0 To k - 1
                For j As Integer = 0 To p - 1
                    centers(c, j) /= counts(c)
                Next
            Next
            Return centers
        End Function

        Public Function InitializeKMeansPlusPlus(data(,) As Double, k As Integer, rng As Random) As Double(,)
            Dim n As Integer = data.GetUpperBound(0) + 1
            Dim p As Integer = data.GetUpperBound(1) + 1
            Dim chosen As New List(Of Integer)
            chosen.Add(rng.Next(n))

            While chosen.Count < k
                Dim minSq(n - 1) As Double
                Dim total As Double = 0
                For i As Integer = 0 To n - 1
                    Dim bestSq As Double = Double.PositiveInfinity
                    For Each idx As Integer In chosen
                        Dim sq As Double = SquaredEuclideanRows(data, i, data, idx)
                        If sq < bestSq Then bestSq = sq
                    Next
                    minSq(i) = bestSq
                    total += bestSq
                Next

                Dim nextIndex As Integer = -1
                If total <= 0 Then
                    Dim remaining = Enumerable.Range(0, n).Where(Function(idx) Not chosen.Contains(idx)).ToArray()
                    nextIndex = remaining(rng.Next(remaining.Length))
                Else
                    Dim threshold As Double = rng.NextDouble() * total
                    Dim cumulative As Double = 0
                    For i As Integer = 0 To n - 1
                        cumulative += minSq(i)
                        If cumulative >= threshold Then
                            nextIndex = i
                            Exit For
                        End If
                    Next
                    If nextIndex = -1 Then nextIndex = n - 1
                    If chosen.Contains(nextIndex) Then
                        Dim remaining = Enumerable.Range(0, n).Where(Function(idx) Not chosen.Contains(idx)).ToArray()
                        nextIndex = remaining(rng.Next(remaining.Length))
                    End If
                End If
                chosen.Add(nextIndex)
            End While

            Dim centers(k - 1, p - 1) As Double
            For c As Integer = 0 To k - 1
                For j As Integer = 0 To p - 1
                    centers(c, j) = data(chosen(c), j)
                Next
            Next
            Return centers
        End Function

        Public Function SquaredEuclideanRowToCenter(data(,) As Double, rowIndex As Integer, centers(,) As Double, centerIndex As Integer) As Double
            Dim p As Integer = data.GetUpperBound(1) + 1
            Dim s As Double = 0
            For j As Integer = 0 To p - 1
                Dim d As Double = data(rowIndex, j) - centers(centerIndex, j)
                s += d * d
            Next
            Return s
        End Function

        Public Function SquaredEuclideanRows(dataA(,) As Double, rowA As Integer, dataB(,) As Double, rowB As Integer) As Double
            Dim p As Integer = dataA.GetUpperBound(1) + 1
            Dim s As Double = 0
            For j As Integer = 0 To p - 1
                Dim d As Double = dataA(rowA, j) - dataB(rowB, j)
                s += d * d
            Next
            Return s
        End Function

        Public Function CreateLeafNode(id As Integer, data(,) As Double, rowIndex As Integer) As ClusterNode
            Dim p As Integer = data.GetUpperBound(1) + 1
            Dim centroid(p - 1) As Double
            For j As Integer = 0 To p - 1
                centroid(j) = data(rowIndex, j)
            Next
            Dim node As New ClusterNode
            node.Id = id
            node.Size = 1
            node.LeafOrder = New List(Of Integer) From {rowIndex}
            node.TrueCentroid = CType(centroid.Clone(), Double())
            node.LinkageCentroid = CType(centroid.Clone(), Double())
            Return node
        End Function

        Public Sub CanonicalizeNodes(ByRef leftNode As ClusterNode, ByRef rightNode As ClusterNode)
            Dim leftMin As Integer = leftNode.LeafOrder.Min()
            Dim rightMin As Integer = rightNode.LeafOrder.Min()
            If rightMin < leftMin Then
                Dim temp As ClusterNode = leftNode
                leftNode = rightNode
                rightNode = temp
            End If
        End Sub

        Public Function MergeNodes(leftNode As ClusterNode,
                                   rightNode As ClusterNode,
                                   newId As Integer,
                                   useMedianRepresentative As Boolean) As ClusterNode
            Dim p As Integer = leftNode.TrueCentroid.Length
            Dim merged As New ClusterNode
            merged.Id = newId
            merged.Size = leftNode.Size + rightNode.Size
            merged.LeafOrder = New List(Of Integer)(leftNode.LeafOrder)
            merged.LeafOrder.AddRange(rightNode.LeafOrder)

            ReDim merged.TrueCentroid(p - 1)
            ReDim merged.LinkageCentroid(p - 1)
            For j As Integer = 0 To p - 1
                merged.TrueCentroid(j) = (leftNode.Size * leftNode.TrueCentroid(j) + rightNode.Size * rightNode.TrueCentroid(j)) / merged.Size
                If useMedianRepresentative Then
                    merged.LinkageCentroid(j) = 0.5 * (leftNode.LinkageCentroid(j) + rightNode.LinkageCentroid(j))
                Else
                    merged.LinkageCentroid(j) = merged.TrueCentroid(j)
                End If
            Next
            Return merged
        End Function

        Public Function ComputeClusterDistance(nodeA As ClusterNode,
                                               nodeB As ClusterNode,
                                               linkage As HierarchicalLinkageMethod,
                                               distanceMetric As HierarchicalDistanceMetric) As Double
            Select Case linkage
                Case HierarchicalLinkageMethod.Centroid
                    Return ComputeVectorDistance(nodeA.TrueCentroid, nodeB.TrueCentroid, distanceMetric, 2.0)
                Case HierarchicalLinkageMethod.Median
                    Return ComputeVectorDistance(nodeA.LinkageCentroid, nodeB.LinkageCentroid, distanceMetric, 2.0)
                Case Else
                    Dim sq As Double = SquaredEuclideanVectors(nodeA.TrueCentroid, nodeB.TrueCentroid)
                    Return (nodeA.Size * nodeB.Size / CDbl(nodeA.Size + nodeB.Size)) * sq
            End Select
        End Function

        Public Function ComputeObservationDistance(data(,) As Double,
                                                   rowA As Integer,
                                                   rowB As Integer,
                                                   metric As HierarchicalDistanceMetric,
                                                   minkowskiPower As Double) As Double
            Dim p As Integer = data.GetUpperBound(1) + 1
            Dim a(p - 1) As Double
            Dim b(p - 1) As Double
            For j As Integer = 0 To p - 1
                a(j) = data(rowA, j)
                b(j) = data(rowB, j)
            Next
            Return ComputeVectorDistance(a, b, metric, minkowskiPower)
        End Function

        Public Function ComputeVectorDistance(a() As Double,
                                              b() As Double,
                                              metric As HierarchicalDistanceMetric,
                                              minkowskiPower As Double) As Double
            Select Case metric
                Case HierarchicalDistanceMetric.Euclidean
                    Return Math.Sqrt(SquaredEuclideanVectors(a, b))
                Case HierarchicalDistanceMetric.SquaredEuclidean
                    Return SquaredEuclideanVectors(a, b)
                Case HierarchicalDistanceMetric.Manhattan
                    Dim s As Double = 0
                    For i As Integer = 0 To a.Length - 1
                        s += Math.Abs(a(i) - b(i))
                    Next
                    Return s
                Case HierarchicalDistanceMetric.Chebyshev
                    Dim m As Double = 0
                    For i As Integer = 0 To a.Length - 1
                        m = Math.Max(m, Math.Abs(a(i) - b(i)))
                    Next
                    Return m
                Case HierarchicalDistanceMetric.Minkowski
                    Dim s As Double = 0
                    For i As Integer = 0 To a.Length - 1
                        s += Math.Pow(Math.Abs(a(i) - b(i)), minkowskiPower)
                    Next
                    Return Math.Pow(s, 1.0 / minkowskiPower)
                Case HierarchicalDistanceMetric.Cosine
                    Dim dot As Double = 0
                    Dim normA As Double = 0
                    Dim normB As Double = 0
                    For i As Integer = 0 To a.Length - 1
                        dot += a(i) * b(i)
                        normA += a(i) * a(i)
                        normB += b(i) * b(i)
                    Next
                    If normA <= 0 OrElse normB <= 0 Then Return 0
                    Dim cos As Double = dot / (Math.Sqrt(normA) * Math.Sqrt(normB))
                    If cos > 1 Then cos = 1
                    If cos < -1 Then cos = -1
                    Return 1 - cos
                Case Else
                    Return CorrelationDistance(a, b)
            End Select
        End Function

        Public Function BuildMembershipFromMerges(n As Integer,
                                                  mergeLeft() As Integer,
                                                  mergeRight() As Integer,
                                                  mergesToApply As Integer) As Integer()
            Dim members As New Dictionary(Of Integer, List(Of Integer))
            For i As Integer = 1 To n
                members(i) = New List(Of Integer) From {i - 1}
            Next

            Dim nextId As Integer = n + 1
            For stepIndex As Integer = 0 To mergesToApply - 1
                Dim leftId As Integer = mergeLeft(stepIndex)
                Dim rightId As Integer = mergeRight(stepIndex)
                Dim merged As New List(Of Integer)(members(leftId))
                merged.AddRange(members(rightId))
                members.Remove(leftId)
                members.Remove(rightId)
                members(nextId) = merged
                nextId += 1
            Next

            Dim assignment(n - 1) As Integer
            Dim clusterLabel As Integer = 1
            For Each kvp In members.OrderBy(Function(x) x.Key)
                For Each leafPos As Integer In kvp.Value
                    assignment(leafPos) = clusterLabel
                Next
                clusterLabel += 1
            Next
            Return assignment
        End Function

        Public Function IsPreferredPair(idA As Integer,
                                        idB As Integer,
                                        currentBestA As Integer,
                                        currentBestB As Integer,
                                        nodes As Dictionary(Of Integer, ClusterNode)) As Boolean
            If currentBestA = -1 OrElse currentBestB = -1 Then Return True
            Dim pair1 = CanonicalPairKey(idA, idB, nodes)
            Dim pair2 = CanonicalPairKey(currentBestA, currentBestB, nodes)
            Return String.CompareOrdinal(pair1, pair2) < 0
        End Function

        Private Function CanonicalPairKey(idA As Integer,
                                          idB As Integer,
                                          nodes As Dictionary(Of Integer, ClusterNode)) As String
            Dim nodeA As ClusterNode = nodes(idA)
            Dim nodeB As ClusterNode = nodes(idB)
            Dim minA As Integer = nodeA.LeafOrder.Min()
            Dim minB As Integer = nodeB.LeafOrder.Min()
            If minB < minA Then
                Dim temp As Integer = minA
                minA = minB
                minB = temp
            End If
            Return minA.ToString("D8") & "|" & minB.ToString("D8")
        End Function

        Private Sub ComputeStandardizationParameters(data(,) As Double, mode As ClusterStandardizationMode,
                                                     ByRef locations() As Double, ByRef scales() As Double)
            Dim n As Integer = data.GetUpperBound(0) + 1
            Dim p As Integer = data.GetUpperBound(1) + 1
            ReDim locations(p - 1)
            ReDim scales(p - 1)

            For j As Integer = 0 To p - 1
                Select Case mode
                    Case ClusterStandardizationMode.None
                        locations(j) = 0
                        scales(j) = 1
                    Case ClusterStandardizationMode.ZScores
                        Dim sum As Double = 0
                        For i As Integer = 0 To n - 1
                            sum += data(i, j)
                        Next
                        Dim mean As Double = sum / n
                        Dim ss As Double = 0
                        For i As Integer = 0 To n - 1
                            Dim d As Double = data(i, j) - mean
                            ss += d * d
                        Next
                        Dim sd As Double = If(n > 1, Math.Sqrt(ss / (n - 1)), 0)
                        locations(j) = mean
                        scales(j) = If(sd > 0, sd, 1.0)
                    Case Else
                        Dim minVal As Double = Double.PositiveInfinity
                        Dim maxVal As Double = Double.NegativeInfinity
                        For i As Integer = 0 To n - 1
                            Dim x As Double = data(i, j)
                            If x < minVal Then minVal = x
                            If x > maxVal Then maxVal = x
                        Next
                        locations(j) = minVal
                        Dim rangeVal As Double = maxVal - minVal
                        scales(j) = If(rangeVal > 0, rangeVal, 1.0)
                End Select
            Next
        End Sub

        Private Function TransformValue(x As Double, mode As ClusterStandardizationMode,
                                        location As Double, scale As Double) As Double
            Select Case mode
                Case ClusterStandardizationMode.None
                    Return x
                Case Else
                    Return (x - location) / scale
            End Select
        End Function

        Private Function InverseTransformValue(x As Double, mode As ClusterStandardizationMode,
                                               location As Double, scale As Double) As Double
            Select Case mode
                Case ClusterStandardizationMode.None
                    Return x
                Case Else
                    Return x * scale + location
            End Select
        End Function

        Private Function SquaredEuclideanVectors(a() As Double, b() As Double) As Double
            Dim s As Double = 0
            For i As Integer = 0 To a.Length - 1
                Dim d As Double = a(i) - b(i)
                s += d * d
            Next
            Return s
        End Function

        Private Function CorrelationDistance(a() As Double, b() As Double) As Double
            Dim n As Integer = a.Length
            Dim meanA As Double = a.Average()
            Dim meanB As Double = b.Average()
            Dim num As Double = 0
            Dim denA As Double = 0
            Dim denB As Double = 0
            For i As Integer = 0 To n - 1
                Dim da As Double = a(i) - meanA
                Dim db As Double = b(i) - meanB
                num += da * db
                denA += da * da
                denB += db * db
            Next
            If denA <= 0 OrElse denB <= 0 Then Return 0
            Dim corr As Double = num / Math.Sqrt(denA * denB)
            If corr > 1 Then corr = 1
            If corr < -1 Then corr = -1
            Return 1 - corr
        End Function
    End Module
End Namespace
