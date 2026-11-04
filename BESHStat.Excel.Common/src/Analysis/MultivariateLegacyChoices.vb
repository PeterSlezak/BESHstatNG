Option Explicit On
Option Strict On
Imports System
Imports System.Collections.Generic

Namespace ExcelCommon
    ' Compatibility boundary only: indexes/text belong to the historical Windows choices.
    ' Typed family options below this boundary carry Core enums, never UI indices.
    Public NotInheritable Class MultivariateLegacyChoices
        Private Sub New()
        End Sub
    Public Shared Function GetSelectedKMeansInitialization(index As Integer) As Multivariate.KMeansInitializationMethod
        Select Case index
            Case 1
                Return Multivariate.KMeansInitializationMethod.Forgy
            Case 2
                Return Multivariate.KMeansInitializationMethod.RandomPartition
            Case 3
                Return Multivariate.KMeansInitializationMethod.UserSpecifiedCenters
            Case Else
                Return Multivariate.KMeansInitializationMethod.KMeansPlusPlus
        End Select
    End Function

    Public Shared Function GetSelectedKMeansDistanceMetric(index As Integer) As Multivariate.KMeansDistanceMetric
        If index = 1 Then
            Return Multivariate.KMeansDistanceMetric.Euclidean
        End If
        Return Multivariate.KMeansDistanceMetric.SquaredEuclidean
    End Function

    Public Shared Function GetSelectedKMeansStandardization(index As Integer) As Multivariate.ClusterStandardizationMode
        Select Case index
            Case 1
                Return Multivariate.ClusterStandardizationMode.ZScores
            Case 2
                Return Multivariate.ClusterStandardizationMode.RangeZeroToOne
            Case Else
                Return Multivariate.ClusterStandardizationMode.None
        End Select
    End Function

    Public Shared Function GetSelectedKMeansMissingPolicy(index As Integer) As Multivariate.ClusterMissingValuePolicy
        If index = 1 Then
            Return Multivariate.ClusterMissingValuePolicy.ListwiseDeletion
        End If
        Return Multivariate.ClusterMissingValuePolicy.ErrorOnMissing
    End Function

    Public Shared Function GetSelectedKMeansEmptyClusterHandling(index As Integer) As Multivariate.EmptyClusterHandlingStrategy
        Select Case index
            Case 1
                Return Multivariate.EmptyClusterHandlingStrategy.RandomObservation
            Case 2
                Return Multivariate.EmptyClusterHandlingStrategy.KeepPreviousCenter
            Case Else
                Return Multivariate.EmptyClusterHandlingStrategy.FarthestObservation
        End Select
    End Function

    Public Shared Function GetSelectedHierarchicalLinkage(index As Integer) As Multivariate.HierarchicalLinkageMethod
        Select Case index
            Case 1
                Return Multivariate.HierarchicalLinkageMethod.Complete
            Case 2
                Return Multivariate.HierarchicalLinkageMethod.Average
            Case 3
                Return Multivariate.HierarchicalLinkageMethod.WeightedAverage
            Case 4
                Return Multivariate.HierarchicalLinkageMethod.SingleLinkage
            Case 5
                Return Multivariate.HierarchicalLinkageMethod.Centroid
            Case 6
                Return Multivariate.HierarchicalLinkageMethod.Median
            Case Else
                Return Multivariate.HierarchicalLinkageMethod.Ward
        End Select
    End Function

    Public Shared Function GetSelectedHierarchicalDistanceMetric(index As Integer) As Multivariate.HierarchicalDistanceMetric
        Select Case index
            Case 1
                Return Multivariate.HierarchicalDistanceMetric.Euclidean
            Case 2
                Return Multivariate.HierarchicalDistanceMetric.Manhattan
            Case 3
                Return Multivariate.HierarchicalDistanceMetric.Chebyshev
            Case 4
                Return Multivariate.HierarchicalDistanceMetric.Minkowski
            Case 5
                Return Multivariate.HierarchicalDistanceMetric.Cosine
            Case 6
                Return Multivariate.HierarchicalDistanceMetric.Correlation
            Case Else
                Return Multivariate.HierarchicalDistanceMetric.SquaredEuclidean
        End Select
    End Function

    Public Shared Function GetSelectedHierarchicalStandardization(index As Integer) As Multivariate.ClusterStandardizationMode
        Return GetSelectedKMeansStandardization(index)
    End Function

    Public Shared Function GetSelectedHierarchicalMissingPolicy(index As Integer) As Multivariate.ClusterMissingValuePolicy
        Return GetSelectedKMeansMissingPolicy(index)
    End Function

    Public Shared Function GetSelectedHierarchicalDendrogramHeightMode(index As Integer) As Multivariate.DendrogramHeightMode
        If index = 1 Then
            Return Multivariate.DendrogramHeightMode.StepLevels
        End If
        Return Multivariate.DendrogramHeightMode.MergeDistance
    End Function

    Public Shared Function GetSelectedHierarchicalDendrogramOrientation(index As Integer) As Multivariate.DendrogramOrientation
        Select Case index
            Case 1
                Return Multivariate.DendrogramOrientation.Bottom
            Case 2
                Return Multivariate.DendrogramOrientation.Left
            Case 3
                Return Multivariate.DendrogramOrientation.Right
            Case Else
                Return Multivariate.DendrogramOrientation.Top
        End Select
    End Function

    Public Shared Function GetSelectedHierarchicalDendrogramLabelMode(index As Integer) As Multivariate.DendrogramLabelMode
        Select Case index
            Case 1
                Return Multivariate.DendrogramLabelMode.AxisTitle
            Case 2
                Return Multivariate.DendrogramLabelMode.None
            Case Else
                Return Multivariate.DendrogramLabelMode.DataLabels
        End Select
    End Function

    Public Shared Function GetSelectedFAExtractionMethod(index As Integer) As Multivariate.FactorAnalysisExtractionMethod
        Select Case index
            Case 1
                Return Multivariate.FactorAnalysisExtractionMethod.PrincipalAxis
            Case 2
                Return Multivariate.FactorAnalysisExtractionMethod.MaximumLikelihood ' MaximumLikelihood after enum extension
            Case 3
                Return Multivariate.FactorAnalysisExtractionMethod.GeneralizedLeastSquares ' GeneralizedLeastSquares after enum extension
            Case 4
                Return Multivariate.FactorAnalysisExtractionMethod.Image ' Image after enum extension
            Case 5
                Return Multivariate.FactorAnalysisExtractionMethod.Alpha ' Alpha after enum extension
            Case Else
                Return Multivariate.FactorAnalysisExtractionMethod.PrincipalComponents
        End Select
    End Function

    Public Shared Function GetSelectedFARotationMethod(index As Integer) As Multivariate.FactorAnalysisRotationMethod
        Select Case index
            Case 1
                Return Multivariate.FactorAnalysisRotationMethod.Varimax
            Case 2
                Return Multivariate.FactorAnalysisRotationMethod.Quartimax
            Case 3
                Return Multivariate.FactorAnalysisRotationMethod.Equamax
            Case 4
                Return Multivariate.FactorAnalysisRotationMethod.Promax
            Case Else
                Return Multivariate.FactorAnalysisRotationMethod.None
        End Select
    End Function

    Public Shared Function GetSelectedFAScoreMethod(index As Integer) As Multivariate.FactorAnalysisScoreMethod
        Select Case index
            Case 1
                Return Multivariate.FactorAnalysisScoreMethod.Regression
            Case 2
                Return Multivariate.FactorAnalysisScoreMethod.Bartlett
            Case Else
                Return Multivariate.FactorAnalysisScoreMethod.None
        End Select
    End Function

    Public Shared Function GetSelectedFACommunalityInitialization(index As Integer) As Multivariate.FactorAnalysisCommunalityInitialization
        If index = 1 Then
            Return Multivariate.FactorAnalysisCommunalityInitialization.One
        End If
        Return Multivariate.FactorAnalysisCommunalityInitialization.SquaredMultipleCorrelation
    End Function

    Public Shared Function GetSelectedFAMissingPolicy(index As Integer) As Multivariate.FactorAnalysisMissingValuePolicy
        If index = 1 Then
            Return Multivariate.FactorAnalysisMissingValuePolicy.ListwiseDeletion
        End If
        Return Multivariate.FactorAnalysisMissingValuePolicy.ErrorOnMissing
    End Function

    Public Shared Function GetSelectedFAMatrixType(covariance As Boolean) As Multivariate.FactorAnalysisMatrixType
        If covariance Then
            Return Multivariate.FactorAnalysisMatrixType.Covariance
        End If
        Return Multivariate.FactorAnalysisMatrixType.Correlation
    End Function

    Public Shared Function GetSelectedHierarchicalMembershipMode(byHeight As Boolean) As Multivariate.HierarchicalMembershipDisplayMode
        If byHeight Then
            Return Multivariate.HierarchicalMembershipDisplayMode.ByHeight
        End If
        Return Multivariate.HierarchicalMembershipDisplayMode.ByClusterCount
    End Function

    Public Shared Function GetSelectedFARetentionMethod(eigen As Boolean, variance As Boolean) As Multivariate.FactorAnalysisRetentionMethod
        If eigen Then
            Return Multivariate.FactorAnalysisRetentionMethod.Eigenvalue
        End If
        If variance Then
            Return Multivariate.FactorAnalysisRetentionMethod.Variance
        End If
        Return Multivariate.FactorAnalysisRetentionMethod.Fixed
    End Function

    Public Shared Function GetSelectedDiscriminantMethod(txt As String) As Multivariate.DiscriminantAnalysisMethod
        If txt.IndexOf("quadratic", StringComparison.OrdinalIgnoreCase) >= 0 OrElse txt.IndexOf("qda", StringComparison.OrdinalIgnoreCase) >= 0 Then
            Return Multivariate.DiscriminantAnalysisMethod.Quadratic
        End If
        Return Multivariate.DiscriminantAnalysisMethod.Linear
    End Function

    Public Shared Function GetSelectedDiscriminantMissingPolicy(txt As String) As Multivariate.ClusterMissingValuePolicy
        If txt.IndexOf("listwise", StringComparison.OrdinalIgnoreCase) >= 0 Then
            Return Multivariate.ClusterMissingValuePolicy.ListwiseDeletion
        End If
        Return Multivariate.ClusterMissingValuePolicy.ErrorOnMissing
    End Function

    Public Shared Function GetSelectedDiscriminantStandardization(txt As String) As Multivariate.ClusterStandardizationMode
        If txt.IndexOf("z", StringComparison.OrdinalIgnoreCase) >= 0 Then
            Return Multivariate.ClusterStandardizationMode.ZScores
        End If
        If txt.IndexOf("range", StringComparison.OrdinalIgnoreCase) >= 0 OrElse txt.IndexOf("0 to 1", StringComparison.OrdinalIgnoreCase) >= 0 Then
            Return Multivariate.ClusterStandardizationMode.RangeZeroToOne
        End If
        Return Multivariate.ClusterStandardizationMode.None
    End Function

    Public Shared Function GetSelectedDiscriminantPriorMode(txt As String) As Multivariate.DiscriminantPriorMode

        If txt.IndexOf("user", StringComparison.OrdinalIgnoreCase) >= 0 Then
            Return Multivariate.DiscriminantPriorMode.UserSpecified
        End If
        If txt.IndexOf("equal", StringComparison.OrdinalIgnoreCase) >= 0 Then
            Return Multivariate.DiscriminantPriorMode.Equal
        End If
        Return Multivariate.DiscriminantPriorMode.ProportionalToGroupSizes
    End Function

    Public Shared Function GetSelectedDiscriminantValidationMode(txt As String) As Multivariate.DiscriminantValidationMode

        If txt.IndexOf("leave", StringComparison.OrdinalIgnoreCase) >= 0 Then
            Return Multivariate.DiscriminantValidationMode.LeaveOneOut
        End If
        If txt.IndexOf("k-fold", StringComparison.OrdinalIgnoreCase) >= 0 OrElse txt.IndexOf("k fold", StringComparison.OrdinalIgnoreCase) >= 0 OrElse txt.IndexOf("kfold", StringComparison.OrdinalIgnoreCase) >= 0 Then
            Return Multivariate.DiscriminantValidationMode.KFold
        End If
        If txt.IndexOf("holdout", StringComparison.OrdinalIgnoreCase) >= 0 Then
            Return Multivariate.DiscriminantValidationMode.Holdout
        End If
        Return Multivariate.DiscriminantValidationMode.None
    End Function

    Public Shared Sub ParsePriors(txt As String, ByRef categoryLabels() As Object, ByRef priorProbabilities() As Double)
        If txt.Trim() = String.Empty Then
            Throw New ArgumentException("User-specified priors were selected, but no prior definitions were supplied. Use the format GroupA=0.4; GroupB=0.6 or place each definition on a new line.")
        End If

        Dim normalized As String = txt.Replace(New String({Convert.ToChar(13), Convert.ToChar(10)}), Convert.ToChar(10).ToString()).Replace(Convert.ToChar(13).ToString(), Convert.ToChar(10).ToString())
        Dim chunks() As String = normalized.Split(New String() {Convert.ToChar(10).ToString(), ";"}, StringSplitOptions.RemoveEmptyEntries)

        Dim labelList As New List(Of Object)
        Dim valueList As New List(Of Double)

        For Each rawChunk As String In chunks
            Dim chunk As String = rawChunk.Trim()
            If chunk = String.Empty Then Continue For

            Dim splitPos As Integer = chunk.IndexOf("="c)
            If splitPos < 0 Then splitPos = chunk.IndexOf(":"c)
            If splitPos <= 0 OrElse splitPos >= chunk.Length - 1 Then
                Throw New ArgumentException("Each user prior must be written as label=probability (for example: GroupA=0.4). Separate multiple definitions with semicolons or new lines.")
            End If

            Dim label As String = chunk.Substring(0, splitPos).Trim()
            Dim valueText As String = chunk.Substring(splitPos + 1).Trim()

            If label = String.Empty Then
                Throw New ArgumentException("Each user prior must include a non-empty group label.")
            End If

            Dim p As Double
            If Not Double.TryParse(valueText,
                               System.Globalization.NumberStyles.Float Or System.Globalization.NumberStyles.AllowThousands,
                               System.Globalization.CultureInfo.CurrentCulture,
                               p) Then
                If Not Double.TryParse(valueText,
                                   System.Globalization.NumberStyles.Float Or System.Globalization.NumberStyles.AllowThousands,
                                   System.Globalization.CultureInfo.InvariantCulture,
                                   p) Then
                    Throw New ArgumentException("Could not parse the user-specified prior probability '" & valueText & "'.")
                End If
            End If

            labelList.Add(label)
            valueList.Add(p)
        Next

        If labelList.Count = 0 Then
            Throw New ArgumentException("No valid user-specified priors were found.")
        End If

        categoryLabels = labelList.ToArray()
        priorProbabilities = valueList.ToArray()
    End Sub
    End Class
End Namespace
