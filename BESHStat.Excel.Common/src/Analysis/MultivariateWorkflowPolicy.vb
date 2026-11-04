Option Explicit On
Option Strict On
Imports System
Imports System.Collections.Generic
Imports System.Globalization
Imports System.Linq
Imports BESHStatNG.Multivariate

Namespace ExcelCommon
    Public Enum MultivariateAnalysisKind
        PCA
        CA
        MCA
        KMeans
        Hierarchical
        Factor
        Discriminant
    End Enum
    Public Enum MultivariateCellSemantics
        NumericAnalysis
        CategoricalAnalysis
        GroupThenNumericPredictors
        ContingencyCounts
    End Enum
    Public Enum MultivariatePcaRetention
        Unselected
        Eigenvalue
        Fixed
        Variance
    End Enum
    Public Enum MultivariateColumnRole
        AnalysisVariable
        GroupingVariable
    End Enum
    Public NotInheritable Class MultivariateColumnIntent
        Friend Sub New(index As Integer, name As String, sourceKey As String, role As MultivariateColumnRole)
            Me.Index = index : Me.Name = name : Me.SourceKey = sourceKey : Me.Role = role
        End Sub
        Public ReadOnly Property Index As Integer
        Public ReadOnly Property Name As String
        Public ReadOnly Property SourceKey As String
        Public ReadOnly Property Role As MultivariateColumnRole
    End Class

    ''' <summary>Detached acquired identity/shape, not a second matrix store. Source rows,
    ''' Core retained positions, user labels and class labels are deliberately separate.
    ''' Numerical/categorical matrices are borrowed only for immediate Core calls.</summary>
    Public NotInheritable Class MultivariateInputIntent
        Private ReadOnly _names As String(), _labels As String(), _groups As String(), _rows As Integer(), _keys As String()
        Public Sub New(kind As MultivariateAnalysisKind, rowCount As Integer, columnCount As Integer,
                       names() As String, Optional sourceRowIds() As Integer = Nothing,
                       Optional rowLabels() As String = Nothing, Optional groupLabels() As String = Nothing,
                       Optional sourceColumnKeys() As String = Nothing)
            Me.Kind = kind : Me.RowCount = rowCount : Me.ColumnCount = columnCount
            _names = Copy(names) : _rows = Copy(sourceRowIds) : _labels = Copy(rowLabels) : _groups = Copy(groupLabels) : _keys = Copy(sourceColumnKeys)
            Me.CellSemantics = If(kind = MultivariateAnalysisKind.MCA, MultivariateCellSemantics.CategoricalAnalysis,
                If(kind = MultivariateAnalysisKind.CA, MultivariateCellSemantics.ContingencyCounts,
                If(kind = MultivariateAnalysisKind.Discriminant, MultivariateCellSemantics.GroupThenNumericPredictors, MultivariateCellSemantics.NumericAnalysis)))
            Dim issues As New List(Of RegressionDiagnostic)
            If rowCount < 1 OrElse columnCount < 1 Then issues.Add(Candidate("dimensions", "Input has no analysis rows or columns."))
            If _names Is Nothing OrElse _names.Length <> columnCount Then issues.Add(Candidate("names.shape", "Column names do not match the acquired column axis."))
            If _names IsNot Nothing AndAlso _names.Distinct(StringComparer.Ordinal).Count() <> _names.Length Then issues.Add(Candidate("names.duplicate", "Duplicate column display names are retained."))
            If _rows IsNot Nothing AndAlso _rows.Length <> rowCount Then issues.Add(Candidate("rows.shape", "Source RowIds do not match the acquired row axis."))
            If _labels IsNot Nothing AndAlso _labels.Length <> rowCount Then issues.Add(Candidate("labels.shape", "User labels do not match the acquired row axis."))
            If _groups IsNot Nothing AndAlso _groups.Length <> rowCount Then issues.Add(Candidate("groups.shape", "Display groups do not match the acquired row axis."))
            If _keys IsNot Nothing AndAlso _keys.Length <> columnCount Then issues.Add(Candidate("keys.shape", "Selected source keys do not match the acquired column axis."))
            Validation = New RegressionValidation(issues, 0, 0, {})
        End Sub
        Private Shared Function Candidate(code As String, message As String) As RegressionDiagnostic
            Return New RegressionDiagnostic("multivariate." & code, "input", message, blocking:=False)
        End Function
        Private Shared Function Copy(Of T)(values() As T) As T()
            Return If(values Is Nothing, Nothing, DirectCast(values.Clone(), T()))
        End Function
        Public ReadOnly Property Kind As MultivariateAnalysisKind
        Public ReadOnly Property CellSemantics As MultivariateCellSemantics
        Public ReadOnly Property RowCount As Integer
        Public ReadOnly Property ColumnCount As Integer
        Public ReadOnly Property Validation As RegressionValidation
        Public ReadOnly Property Columns As IReadOnlyList(Of MultivariateColumnIntent)
            Get
                Dim output As New List(Of MultivariateColumnIntent)
                For j = 0 To Math.Min(Math.Max(0, ColumnCount), If(_names Is Nothing, 0, _names.Length)) - 1
                    output.Add(New MultivariateColumnIntent(j, If(_names IsNot Nothing AndAlso j < _names.Length, _names(j), Nothing),
                        If(_keys IsNot Nothing AndAlso j < _keys.Length, _keys(j), Nothing),
                        If(Kind = MultivariateAnalysisKind.Discriminant AndAlso j = 0, MultivariateColumnRole.GroupingVariable, MultivariateColumnRole.AnalysisVariable)))
                Next
                Return output.AsReadOnly()
            End Get
        End Property
        Public ReadOnly Property VariableNames As String()
            Get
                Return Copy(_names)
            End Get
        End Property
        Public ReadOnly Property SourceRowIds As Integer()
            Get
                Return Copy(_rows)
            End Get
        End Property
        Public ReadOnly Property RowLabels As String()
            Get
                Return Copy(_labels)
            End Get
        End Property
        Public ReadOnly Property GroupLabels As String()
            Get
                Return Copy(_groups)
            End Get
        End Property
    End Class

    ''' <summary>Options are explicit Core values. Copy before configuration; no fitting,
    ''' initialization, scaling, retention or validation mathematics is implemented here.</summary>
    Public NotInheritable Class MultivariatePcaOptions
        Public Sub New(correlation As Boolean, eigen As Boolean, fixed As Boolean, variance As Boolean,
                       eigenValue As Decimal, fixedCount As Decimal, variancePercent As Decimal)
            MatrixType = If(correlation, "Correlation", "Covariance")
            If eigen Then
                Retention = MultivariatePcaRetention.Eigenvalue : Value = CDbl(eigenValue)
                Label = "Eigenvalue > " & eigenValue.ToString(CultureInfo.CurrentCulture)
            ElseIf fixed Then
                Retention = MultivariatePcaRetention.Fixed : Value = CDbl(fixedCount)
                Label = "Fixed Number of Components = " & fixedCount.ToString(CultureInfo.CurrentCulture)
            ElseIf variance Then
                Retention = MultivariatePcaRetention.Variance : Value = CDbl(variancePercent)
                Label = "Explained Variance > " & variancePercent.ToString(CultureInfo.CurrentCulture)
            Else
                Label = String.Empty
            End If
        End Sub
        Public ReadOnly Property MatrixType As String
        Public ReadOnly Property Retention As MultivariatePcaRetention
        Public ReadOnly Property Value As Double
        Public ReadOnly Property Label As String
        Public ReadOnly Property CoreRetention As String
            Get
                Return If(Retention = MultivariatePcaRetention.Unselected, String.Empty, Retention.ToString())
            End Get
        End Property
        Public Sub Configure(model As PCA, maximumIterations As Integer, epsilon As Double)
            model.settingsInputs(maximumIterations, epsilon, MatrixType)
        End Sub
    End Class

    Public NotInheritable Class MultivariateKMeansOptions
        Public Property Clusters As Integer = 3
        Public Property Initialization As KMeansInitializationMethod = KMeansInitializationMethod.KMeansPlusPlus
        Public Property Distance As KMeansDistanceMetric = KMeansDistanceMetric.SquaredEuclidean
        Public Property Starts As Integer = 10
        Public Property MaximumIterations As Integer = 100
        Public Property Tolerance As Double = 0.000001
        Public Property Standardization As ClusterStandardizationMode = ClusterStandardizationMode.None
        Public Property Missing As ClusterMissingValuePolicy = ClusterMissingValuePolicy.ErrorOnMissing
        Public Property EmptyCluster As EmptyClusterHandlingStrategy = EmptyClusterHandlingStrategy.FarthestObservation
        Public Property Seed As Integer = Integer.MinValue
        Public Function Copy() As MultivariateKMeansOptions
            Return DirectCast(MemberwiseClone(), MultivariateKMeansOptions)
        End Function
        Public Sub Configure(model As KMeans)
            Dim o = Copy()
            model.settingsInputs(o.Clusters, o.Initialization, o.Distance, o.Starts, o.MaximumIterations, o.Tolerance, o.Standardization, o.Missing, o.EmptyCluster, o.Seed)
        End Sub
    End Class

    Public NotInheritable Class MultivariateHierarchicalOptions
        Public Property Linkage As HierarchicalLinkageMethod = HierarchicalLinkageMethod.Ward
        Public Property Distance As HierarchicalDistanceMetric = HierarchicalDistanceMetric.SquaredEuclidean
        Public Property MinkowskiPower As Double = 2
        Public Property Standardization As ClusterStandardizationMode = ClusterStandardizationMode.None
        Public Property Missing As ClusterMissingValuePolicy = ClusterMissingValuePolicy.ErrorOnMissing
        Public Property Cut As HierarchicalMembershipDisplayMode = HierarchicalMembershipDisplayMode.ByClusterCount
        Public Property Clusters As Integer = 3
        Public Property CutHeight As Double
        Public Function Copy() As MultivariateHierarchicalOptions
            Return DirectCast(MemberwiseClone(), MultivariateHierarchicalOptions)
        End Function
        Public Sub ConfigureModel(model As HierarchicalClustering)
            Dim o = Copy()
            model.settingsInputs(o.Linkage, o.Distance, o.MinkowskiPower, o.Standardization, o.Missing)
        End Sub
        Public Sub ConfigureReport(model As HierarchicalClustering)
            Dim o = Copy()
            model.reportInputs(o.Cut, o.Clusters, o.CutHeight)
        End Sub
    End Class

    Public NotInheritable Class MultivariateFactorOptions
        Public Property MaximumIterations As Integer = 250
        Public Property Epsilon As Double = 0.000001
        Public Property MatrixType As FactorAnalysisMatrixType = FactorAnalysisMatrixType.Correlation
        Public Property Extraction As FactorAnalysisExtractionMethod = FactorAnalysisExtractionMethod.PrincipalAxis
        Public Property Retention As FactorAnalysisRetentionMethod = FactorAnalysisRetentionMethod.Fixed
        Public Property RetentionValue As Double = 1
        Public Property Rotation As FactorAnalysisRotationMethod = FactorAnalysisRotationMethod.Varimax
        Public Property Scores As FactorAnalysisScoreMethod = FactorAnalysisScoreMethod.Regression
        Public Property Communality As FactorAnalysisCommunalityInitialization = FactorAnalysisCommunalityInitialization.SquaredMultipleCorrelation
        Public Property Missing As FactorAnalysisMissingValuePolicy = FactorAnalysisMissingValuePolicy.ErrorOnMissing
        Public Property KaiserNormalization As Boolean = True
        Public Property PromaxPower As Double = 4
        Public Function Copy() As MultivariateFactorOptions
            Return DirectCast(MemberwiseClone(), MultivariateFactorOptions)
        End Function
        Public Sub Configure(model As FactorAnalysis)
            Dim o = Copy()
            model.settingsInputs(o.MaximumIterations, o.Epsilon, o.MatrixType, o.Extraction, o.Retention, o.RetentionValue, o.Rotation, o.Scores, o.Communality, o.Missing, o.KaiserNormalization, o.PromaxPower)
        End Sub
    End Class

    Public NotInheritable Class MultivariateDiscriminantOptions
        Public Property Method As DiscriminantAnalysisMethod = DiscriminantAnalysisMethod.Linear
        Public Property Standardization As ClusterStandardizationMode = ClusterStandardizationMode.None
        Public Property Missing As ClusterMissingValuePolicy = ClusterMissingValuePolicy.ErrorOnMissing
        Public Property Priors As DiscriminantPriorMode = DiscriminantPriorMode.ProportionalToGroupSizes
        Public Property Regularization As Double = 0.00000001
        Public Property Validation As DiscriminantValidationMode = DiscriminantValidationMode.None
        Public Property Folds As Integer = 5
        Public Property HoldoutFraction As Double = 0.3
        Public Property Seed As Integer = Integer.MinValue
        Public Property Stratified As Boolean = True
        Public Function Copy() As MultivariateDiscriminantOptions
            Return DirectCast(MemberwiseClone(), MultivariateDiscriminantOptions)
        End Function
        Public Sub ConfigureModel(model As DiscriminantAnalysis)
            Dim o = Copy()
            model.settingsInputs(o.Method, o.Standardization, o.Missing, o.Priors, o.Regularization)
        End Sub
        Public Sub ConfigureValidation(model As DiscriminantAnalysis)
            Dim o = Copy()
            model.validationInputs(o.Validation, o.Folds, o.HoldoutFraction, o.Seed, o.Stratified)
        End Sub
    End Class

    Public NotInheritable Class MultivariateWorkflowPolicy
        Private Sub New()
        End Sub
        ' Called only after the original Windows conversion/catch, one acquired cell at a time.
        Public Shared Function PcaAuxiliaryLabel(text As String, sourceRowId As Integer, rowLabel As Boolean) As String
            text = If(text, String.Empty).Trim()
            If text <> String.Empty Then Return text
            Return If(rowLabel, sourceRowId.ToString(CultureInfo.CurrentCulture), "(missing)")
        End Function
        Public Shared Function ObservationLabel(text As String, sourceRowId As Integer) As String
            Return If(String.IsNullOrWhiteSpace(text), "Row " & sourceRowId.ToString(CultureInfo.CurrentCulture), text)
        End Function
        Public Shared Function Seed(parsed As Nullable(Of Integer)) As Integer
            Return parsed.GetValueOrDefault(Integer.MinValue)
        End Function
        Public Shared Sub ValidateAuxiliaryRoles(keys As IEnumerable(Of String), rowLabelKey As String, groupingKey As String, distinctAuxiliaries As Boolean)
            If rowLabelKey <> String.Empty AndAlso keys.Contains(rowLabelKey, StringComparer.Ordinal) Then Throw New ArgumentException("The optional row label variable must not also be selected as an analysis variable.")
            If groupingKey <> String.Empty AndAlso keys.Contains(groupingKey, StringComparer.Ordinal) Then Throw New ArgumentException("The grouping variable must not also be selected as an analysis variable.")
            If distinctAuxiliaries AndAlso rowLabelKey <> String.Empty AndAlso groupingKey <> String.Empty AndAlso rowLabelKey = groupingKey Then Throw New ArgumentException("The optional row label variable and grouping variable must be different columns.")
        End Sub
        Public Shared Function FactorRetentionValue(eigen As Boolean, variance As Boolean, eigenValue As Double, variancePercent As Double, fixedCount As Double) As Double
            Return If(eigen, eigenValue, If(variance, variancePercent, fixedCount))
        End Function
        Public Shared Function ClusterInputTable(data(,) As Object, names() As String, rows() As Integer, labels() As String) As Object(,)
            Dim n = data.GetLength(0), p = data.GetLength(1), include = labels IsNot Nothing AndAlso labels.Length = n
            Dim extra = If(include, 2, 1), output(n, p + extra - 1) As Object
            output(0, 0) = "OriginalRow"
            If include Then output(0, 1) = "RowLabel"
            For j = 0 To p - 1
                output(0, j + extra) = names(j)
            Next
            For i = 0 To n - 1
                output(i + 1, 0) = rows(i)
                If include Then output(i + 1, 1) = labels(i)
                For j = 0 To p - 1
                    output(i + 1, j + extra) = data(i, j)
                Next
            Next
            Return output
        End Function
        Public Shared Function DiscriminantInputTable(data(,) As Object, names() As String, groups() As Object, labels() As String) As Object(,)
            Dim n = data.GetLength(0), p = data.GetLength(1), include = labels IsNot Nothing AndAlso labels.Length = n
            If p < 2 Then Throw New ArgumentException("Discriminant analysis requires at least one predictor variable.")
            Dim extra = If(include, 1, 0), output(n, p + extra - 1) As Object
            If include Then output(0, 0) = "Row label"
            For j = 0 To p - 1
                output(0, j + extra) = names(j)
            Next
            For i = 0 To n - 1
                If include Then output(i + 1, 0) = labels(i)
                output(i + 1, extra) = groups(i)
                For j = 1 To p - 1
                    output(i + 1, j + extra) = data(i, j)
                Next
            Next
            Return output
        End Function
    End Class
End Namespace
