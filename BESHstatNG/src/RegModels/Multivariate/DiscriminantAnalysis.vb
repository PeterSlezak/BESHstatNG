Option Explicit On
Option Strict On
Imports System
Imports System.Collections.Generic
Imports System.Globalization
Imports System.Linq
Imports BESHStatNG.AppInfrastructure

Namespace Multivariate

    ''' <summary>
    ''' Specifies whether a common pooled covariance matrix or group-specific covariance matrices are used.
    ''' </summary>
    Public Enum DiscriminantAnalysisMethod
        ''' <summary>
        ''' Linear discriminant analysis using a common pooled within-group covariance matrix.
        ''' </summary>
        Linear = 0

        ''' <summary>
        ''' Quadratic discriminant analysis using a separate covariance matrix for each group.
        ''' </summary>
        Quadratic = 1
    End Enum

    ''' <summary>
    ''' Specifies how prior group probabilities are determined.
    ''' </summary>
    Public Enum DiscriminantPriorMode
        ''' <summary>
        ''' Set priors proportional to the observed training-group sizes.
        ''' </summary>
        ProportionalToGroupSizes = 0

        ''' <summary>
        ''' Give every group the same prior probability.
        ''' </summary>
        Equal = 1

        ''' <summary>
        ''' Use user-specified prior probabilities supplied through <see cref="DiscriminantAnalysis.priorInputs(Object(), Double())"/>.
        ''' </summary>
        UserSpecified = 2
    End Enum

    ''' <summary>
    ''' Specifies the optional validation strategy applied after the model is fitted.
    ''' </summary>
    Public Enum DiscriminantValidationMode
        ''' <summary>
        ''' Do not run an extra validation pass beyond the apparent (resubstitution) classification table.
        ''' </summary>
        None = 0

        ''' <summary>
        ''' Perform exact leave-one-out cross-validation.
        ''' </summary>
        LeaveOneOut = 1

        ''' <summary>
        ''' Perform stratified or unstratified k-fold cross-validation.
        ''' </summary>
        KFold = 2

        ''' <summary>
        ''' Perform a single train/test holdout split.
        ''' </summary>
        Holdout = 3
    End Enum

    ''' <summary>
    ''' Stores the prepared active analysis dataset after preprocessing.
    ''' </summary>
    Public Class DiscriminantPreparedData
        Public Property WorkingData As Double(,)
        Public Property ActiveOriginalData As Double(,)
        Public Property ActiveGroupLabels As String()
        Public Property ActiveRowLabels As String()
        Public Property ActiveOriginalIndices As Integer()
        Public Property RemovedOriginalIndices As Integer()
        Public Property RemovedRowLabels As String()
        Public Property VariableNames As String()
        Public Property ColumnLocations As Double()
        Public Property ColumnScales As Double()
        Public Property Standardization As ClusterStandardizationMode
    End Class

    ''' <summary>
    ''' Stores group-level summaries for a fitted discriminant-analysis model.
    ''' </summary>
    Public Class DiscriminantGroupStatistics
        Public Property GroupLabel As String
        Public Property Count As Integer
        Public Property PriorProbability As Double
        Public Property MeanOriginal As Double()
        Public Property MeanWorking As Double()
        Public Property CovarianceWorking As Double(,)
        Public Property InverseCovarianceWorking As Double(,)
        Public Property LogDeterminantWorking As Double
        Public Property RegularizationUsed As Double
    End Class

    ''' <summary>
    ''' Stores an observed-versus-predicted classification table and related diagnostics.
    ''' </summary>
    Public Class DiscriminantConfusionMatrix
        Public Property ClassLabels As String()
        Public Property Counts As Double(,)
        Public Property RowTotals As Double()
        Public Property ColumnTotals As Double()
        Public Property OverallAccuracy As Double
        Public Property OverallAccuracyPct As Double
        Public Property RecallPct As Double()
        Public Property PrecisionPct As Double()

        Public Function ToObjectTable() As Object(,)
            If ClassLabels Is Nothing OrElse Counts Is Nothing Then Return Nothing

            Dim g As Integer = ClassLabels.Length
            Dim out(g + 2, g + 3) As Object
            out(0, 0) = "Observed"
            For j As Integer = 0 To g - 1
                out(0, j + 1) = ClassLabels(j)
            Next
            out(0, g + 1) = "Row Total"
            out(0, g + 2) = "Recall %"
            out(0, g + 3) = String.Empty

            For i As Integer = 0 To g - 1
                out(i + 1, 0) = ClassLabels(i)
                For j As Integer = 0 To g - 1
                    out(i + 1, j + 1) = Counts(i, j)
                Next
                out(i + 1, g + 1) = RowTotals(i)
                out(i + 1, g + 2) = RecallPct(i)
                out(i + 1, g + 3) = String.Empty
            Next

            out(g + 1, 0) = "Column Total"
            For j As Integer = 0 To g - 1
                out(g + 1, j + 1) = ColumnTotals(j)
            Next
            out(g + 1, g + 1) = RowTotals.Sum()
            out(g + 1, g + 2) = String.Empty
            out(g + 1, g + 3) = String.Empty

            out(g + 2, 0) = "Precision %"
            For j As Integer = 0 To g - 1
                out(g + 2, j + 1) = PrecisionPct(j)
            Next
            out(g + 2, g + 1) = String.Empty
            out(g + 2, g + 2) = "Overall %"
            out(g + 2, g + 3) = OverallAccuracyPct

            Return out
        End Function
    End Class

    ''' <summary>
    ''' Stores row-level classification results for either the training data or a validation split.
    ''' </summary>
    Public Class DiscriminantPredictionResult
        Public Property ValidationMode As DiscriminantValidationMode = DiscriminantValidationMode.None
        Public Property RowIndices As Integer()
        Public Property RowLabels As String()
        Public Property ActualGroupLabels As String()
        Public Property PredictedGroupLabels As String()
        Public Property AssignedPosteriorProbability As Double()
        Public Property PosteriorProbabilities As Double(,)
        Public Property RawScores As Double(,)
        Public Property SquaredDistances As Double(,)
        Public Property CorrectClassification As Boolean()
        Public Property FoldAssignments As Integer()
        Public Property Confusion As DiscriminantConfusionMatrix

        Public Function ToCasewiseTable(classLabels() As String,
                                        Optional maxRows As Integer = -1) As Object(,)
            If PredictedGroupLabels Is Nothing Then Return Nothing

            Dim n As Integer = PredictedGroupLabels.Length
            If maxRows > 0 Then n = Math.Min(n, maxRows)

            Dim g As Integer = If(classLabels Is Nothing, 0, classLabels.Length)
            Dim out(n, 4 + g + g) As Object
            out(0, 0) = "Row"
            out(0, 1) = "Label"
            out(0, 2) = "Actual"
            out(0, 3) = "Predicted"
            out(0, 4) = "Assigned Posterior"
            For j As Integer = 0 To g - 1
                out(0, 5 + j) = $"Posterior[{classLabels(j)}]"
            Next
            For j As Integer = 0 To g - 1
                out(0, 5 + g + j) = $"Distance2[{classLabels(j)}]"
            Next

            For i As Integer = 0 To n - 1
                out(i + 1, 0) = If(RowIndices Is Nothing, i + 1, RowIndices(i))
                out(i + 1, 1) = If(RowLabels Is Nothing, String.Empty, RowLabels(i))
                out(i + 1, 2) = If(ActualGroupLabels Is Nothing, String.Empty, ActualGroupLabels(i))
                out(i + 1, 3) = PredictedGroupLabels(i)
                out(i + 1, 4) = If(AssignedPosteriorProbability Is Nothing, Double.NaN, AssignedPosteriorProbability(i))
                For j As Integer = 0 To g - 1
                    If PosteriorProbabilities IsNot Nothing Then out(i + 1, 5 + j) = PosteriorProbabilities(i, j)
                    If SquaredDistances IsNot Nothing Then out(i + 1, 5 + g + j) = SquaredDistances(i, j)
                Next
            Next

            Return out
        End Function
    End Class

    ''' <summary>
    ''' Fits classical linear and quadratic discriminant-analysis models, performs casewise classification,
    ''' and optionally runs common validation strategies such as leave-one-out, k-fold, and holdout validation.
    ''' </summary>
    Public Class DiscriminantAnalysis

        Private pRawData(,) As Double
        Private pRawGroupLabels() As Object
        Private pRowLabels() As String
        Private pVarNames() As String

        Private pMethod As DiscriminantAnalysisMethod = DiscriminantAnalysisMethod.Linear
        Private pStandardization As ClusterStandardizationMode = ClusterStandardizationMode.None
        Private pMissingPolicy As ClusterMissingValuePolicy = ClusterMissingValuePolicy.ErrorOnMissing
        Private pPriorMode As DiscriminantPriorMode = DiscriminantPriorMode.ProportionalToGroupSizes
        Private pUserPriorLabels() As String
        Private pUserPriorValues() As Double
        Private pValidationMode As DiscriminantValidationMode = DiscriminantValidationMode.None
        Private pValidationFolds As Integer = 5
        Private pHoldoutFraction As Double = 0.3
        Private pValidationStratified As Boolean = True
        Private pRandomSeed As Integer = Integer.MinValue
        Private pCovarianceRegularization As Double = 0.00000001

        Private pPrepared As DiscriminantPreparedData
        Private pGroupStats As List(Of DiscriminantGroupStatistics)
        Private pGroupLabels() As String
        Private pOverallMeanWorking() As Double
        Private pPooledCovarianceWorking(,) As Double
        Private pPooledInverseWorking(,) As Double
        Private pPooledLogDeterminant As Double
        Private pPooledRegularization As Double
        Private pLinearCoefficientsWorking(,) As Double
        Private pLinearConstantsWorking() As Double
        Private pLinearCoefficientsOriginal(,) As Double
        Private pLinearConstantsOriginal() As Double
        Private pCanonicalEigenvalues() As Double
        Private pCanonicalCorrelations() As Double
        Private pCanonicalProportions() As Double
        Private pCanonicalWilksLambda() As Double
        Private pCanonicalCoefficients(,) As Double
        Private pCanonicalScores(,) As Double
        Private pCanonicalGroupCentroids(,) As Double
        Private pTrainingClassification As DiscriminantPredictionResult
        Private pValidationClassification As DiscriminantPredictionResult
        Private pRandomSeedUsed As Integer = Integer.MinValue

        Public Sub dataInputs(arData(,) As Double,
                              groupLabels() As Object,
                              Optional arRowLabels() As String = Nothing,
                              Optional arVarNames() As String = Nothing)
            pRawData = arData
            pRawGroupLabels = groupLabels
            pRowLabels = arRowLabels
            pVarNames = arVarNames
        End Sub

        Public Sub settingsInputs(Optional method As DiscriminantAnalysisMethod = DiscriminantAnalysisMethod.Linear,
                                  Optional standardization As ClusterStandardizationMode = ClusterStandardizationMode.None,
                                  Optional missingPolicy As ClusterMissingValuePolicy = ClusterMissingValuePolicy.ErrorOnMissing,
                                  Optional priorMode As DiscriminantPriorMode = DiscriminantPriorMode.ProportionalToGroupSizes,
                                  Optional covarianceRegularization As Double = 0.00000001)
            pMethod = method
            pStandardization = standardization
            pMissingPolicy = missingPolicy
            pPriorMode = priorMode
            pCovarianceRegularization = Math.Max(0.0, covarianceRegularization)
        End Sub

        Public Sub priorInputs(categoryLabels() As Object, priorProbabilities() As Double)
            If categoryLabels Is Nothing OrElse priorProbabilities Is Nothing Then
                CoreServices.Errors.LogAndThrow(New ArgumentNullException("User priors require both category labels and probabilities."))
            End If
            If categoryLabels.Length <> priorProbabilities.Length Then
                CoreServices.Errors.LogAndThrow(New ArgumentException("The user-prior label and probability arrays must have the same length."))
            End If
            ReDim pUserPriorLabels(categoryLabels.Length - 1)
            ReDim pUserPriorValues(priorProbabilities.Length - 1)
            For i As Integer = 0 To categoryLabels.Length - 1
                pUserPriorLabels(i) = NormalizeGroupLabel(categoryLabels(i))
                pUserPriorValues(i) = priorProbabilities(i)
            Next
            pPriorMode = DiscriminantPriorMode.UserSpecified
        End Sub

        Public Sub validationInputs(Optional mode As DiscriminantValidationMode = DiscriminantValidationMode.None,
                                    Optional numberOfFolds As Integer = 5,
                                    Optional holdoutFraction As Double = 0.3,
                                    Optional randomSeed As Integer = Integer.MinValue,
                                    Optional stratified As Boolean = True)
            pValidationMode = mode
            pValidationFolds = Math.Max(2, numberOfFolds)
            pHoldoutFraction = holdoutFraction
            pRandomSeed = randomSeed
            pValidationStratified = stratified
        End Sub

        Public ReadOnly Property PreparedData As DiscriminantPreparedData
            Get
                Return pPrepared
            End Get
        End Property

        Public ReadOnly Property GroupStatistics As List(Of DiscriminantGroupStatistics)
            Get
                Return pGroupStats
            End Get
        End Property

        Public ReadOnly Property GroupLabels As String()
            Get
                Return pGroupLabels
            End Get
        End Property

        Public ReadOnly Property TrainingClassification As DiscriminantPredictionResult
            Get
                Return pTrainingClassification
            End Get
        End Property

        Public ReadOnly Property ValidationClassification As DiscriminantPredictionResult
            Get
                Return pValidationClassification
            End Get
        End Property

        Public ReadOnly Property CanonicalEigenvalues As Double()
            Get
                Return pCanonicalEigenvalues
            End Get
        End Property

        Public Sub Fit()
            ValidateInputs()
            pPrepared = PrepareData(pRawData, pRawGroupLabels, pRowLabels, pVarNames, pStandardization, pMissingPolicy)
            FitFromPreparedData(pPrepared)
            pTrainingClassification = PredictInternal(pPrepared.ActiveOriginalData,
                                                      pPrepared.ActiveRowLabels,
                                                      pPrepared.ActiveOriginalIndices,
                                                      pPrepared.ActiveGroupLabels,
                                                      DiscriminantValidationMode.None,
                                                      Nothing)
            pValidationClassification = Nothing
            If pValidationMode <> DiscriminantValidationMode.None Then
                pValidationClassification = RunValidation()
            End If
        End Sub

        Public Function Predict(newData(,) As Double,
                                Optional rowLabels() As String = Nothing) As String()
            Dim details = PredictDetailed(newData, rowLabels)
            Return details.PredictedGroupLabels
        End Function

        Public Function PredictDetailed(newData(,) As Double,
                                        Optional rowLabels() As String = Nothing,
                                        Optional actualGroupLabels As Object() = Nothing) As DiscriminantPredictionResult
            MultivariateInputHelpers.ValidateRectangularData(newData, nullParamName:=NameOf(newData),
                                                             rankMessage:="The data matrix must be two-dimensional.",
                                                             emptyMessage:="The data matrix must contain at least one row and one column.")

            If pPrepared Is Nothing OrElse pGroupStats Is Nothing Then
                CoreServices.Errors.LogAndThrow(New InvalidOperationException("Model is not fitted."))
            End If

            Dim actualLabels() As String = Nothing
            If actualGroupLabels IsNot Nothing Then
                ReDim actualLabels(actualGroupLabels.Length - 1)
                For i As Integer = 0 To actualGroupLabels.Length - 1
                    actualLabels(i) = NormalizeGroupLabel(actualGroupLabels(i))
                Next
            End If

            Return PredictInternal(newData,
                                   MultivariateInputHelpers.NormalizeRowLabels(rowLabels, newData.GetUpperBound(0) + 1, defaultPrefix:="Row", allowDefaultOnLengthMismatch:=True),
                                   Enumerable.Range(1, newData.GetUpperBound(0) + 1).ToArray(),
                                   actualLabels,
                                   DiscriminantValidationMode.None,
                                   Nothing)
        End Function

        Public Function wrapResults() As List(Of ResultTable)
            If pPrepared Is Nothing OrElse pGroupStats Is Nothing Then
                CoreServices.Errors.LogAndThrow(New InvalidOperationException("Model is not fitted."))
            End If

            Dim out As New List(Of ResultTable)
            Dim t As ResultTable

            t = ClusterAnalysisHelpers.BuildResultTableFromObjectMatrix("Discriminant Analysis Settings", BuildSettingsTable())
            If pMethod = DiscriminantAnalysisMethod.Linear Then
                t.AddFootnote("Linear discriminant analysis uses a common pooled within-group covariance matrix.")
            Else
                t.AddFootnote("Quadratic discriminant analysis uses a separate covariance matrix for each group.")
            End If
            out.Add(t)

            t = ClusterAnalysisHelpers.BuildResultTableFromObjectMatrix("Group Summary", BuildGroupSummaryTable())
            out.Add(t)

            t = ClusterAnalysisHelpers.BuildResultTableFromObjectMatrix("Group Means (Original Scale)", BuildGroupMeansTable(False))
            out.Add(t)

            If pPrepared.Standardization <> ClusterStandardizationMode.None Then
                t = ClusterAnalysisHelpers.BuildResultTableFromObjectMatrix("Group Means (Working Analysis Scale)", BuildGroupMeansTable(True))
                out.Add(t)

                t = ClusterAnalysisHelpers.BuildResultTableFromObjectMatrix("Preprocessing Constants", BuildPreprocessingTable())
                out.Add(t)
            End If

            If pMethod = DiscriminantAnalysisMethod.Linear Then
                t = ClusterAnalysisHelpers.BuildResultTableFromObjectMatrix("Pooled Covariance Matrix (Working Scale)", BuildCovarianceTable(pPooledCovarianceWorking))
                out.Add(t)

                t = ClusterAnalysisHelpers.BuildResultTableFromObjectMatrix("Linear Classification Functions (Original Input Scale)", BuildLinearFunctionTable())
                t.AddFootnote("For each group g, classify to the largest value of Constant[g] + sum_j Coef[j,g] * x_j.")
                out.Add(t)

                If pCanonicalEigenvalues IsNot Nothing AndAlso pCanonicalEigenvalues.Length > 0 Then
                    t = ClusterAnalysisHelpers.BuildResultTableFromObjectMatrix("Canonical Discriminant Functions Summary", BuildCanonicalSummaryTable())
                    out.Add(t)

                    t = ClusterAnalysisHelpers.BuildResultTableFromObjectMatrix("Canonical Coefficients (Working Scale)", BuildCanonicalCoefficientTable())
                    out.Add(t)

                    t = ClusterAnalysisHelpers.BuildResultTableFromObjectMatrix("Group Centroids in Canonical Space", BuildCanonicalCentroidTable())
                    out.Add(t)
                End If
            Else
                For Each gs In pGroupStats
                    t = ClusterAnalysisHelpers.BuildResultTableFromObjectMatrix($"Within-Group Covariance Matrix [{gs.GroupLabel}] (Working Scale)", BuildCovarianceTable(gs.CovarianceWorking))
                    out.Add(t)
                Next
            End If

            t = ClusterAnalysisHelpers.BuildResultTableFromObjectMatrix("Training Classification Matrix (Resubstitution)", pTrainingClassification.Confusion.ToObjectTable())
            out.Add(t)

            t = ClusterAnalysisHelpers.BuildResultTableFromObjectMatrix("Training Casewise Classification", pTrainingClassification.ToCasewiseTable(pGroupLabels))
            out.Add(t)

            If pValidationClassification IsNot Nothing Then
                Dim validationTitle As String = "Validation Classification Matrix"
                Select Case pValidationClassification.ValidationMode
                    Case DiscriminantValidationMode.LeaveOneOut
                        validationTitle = "Validation Classification Matrix (Leave-One-Out)"
                    Case DiscriminantValidationMode.KFold
                        validationTitle = $"Validation Classification Matrix ({pValidationFolds}-Fold)"
                    Case DiscriminantValidationMode.Holdout
                        validationTitle = "Validation Classification Matrix (Holdout)"
                End Select

                t = ClusterAnalysisHelpers.BuildResultTableFromObjectMatrix(validationTitle, pValidationClassification.Confusion.ToObjectTable())
                out.Add(t)

                t = ClusterAnalysisHelpers.BuildResultTableFromObjectMatrix("Validation Casewise Classification", pValidationClassification.ToCasewiseTable(pGroupLabels))
                out.Add(t)
            End If

            Dim removedTable = BuildRemovedRowsTable()
            If removedTable IsNot Nothing Then
                t = ClusterAnalysisHelpers.BuildResultTableFromObjectMatrix("Rows Removed by Missing-Value Policy", removedTable)
                out.Add(t)
            End If

            Return out
        End Function

        Private Sub ValidateInputs()
            If pRawData Is Nothing Then CoreServices.Errors.LogAndThrow(New ArgumentNullException("Input predictor data were not supplied."))
            If pRawGroupLabels Is Nothing Then CoreServices.Errors.LogAndThrow(New ArgumentNullException("The grouping variable was not supplied."))
            MultivariateInputHelpers.ValidateRectangularData(pRawData, nullParamName:=NameOf(pRawData), rankMessage:="The data matrix must be two-dimensional.", emptyMessage:="The data matrix must contain at least one row and one column.")
            Dim n As Integer = pRawData.GetUpperBound(0) + 1
            If pRawGroupLabels.Length <> n Then
                CoreServices.Errors.LogAndThrow(New ArgumentException("The grouping variable length must match the number of data rows."))
            End If
        End Sub

        Private Sub FitFromPreparedData(prepared As DiscriminantPreparedData)
            Dim working As Double(,) = prepared.WorkingData
            Dim original As Double(,) = prepared.ActiveOriginalData
            Dim n As Integer = working.GetUpperBound(0) + 1
            Dim p As Integer = working.GetUpperBound(1) + 1

            Dim groupIndexByLabel As New Dictionary(Of String, Integer)(StringComparer.Ordinal)
            Dim labels As New List(Of String)
            For i As Integer = 0 To prepared.ActiveGroupLabels.Length - 1
                Dim lbl As String = prepared.ActiveGroupLabels(i)
                If Not groupIndexByLabel.ContainsKey(lbl) Then
                    groupIndexByLabel(lbl) = labels.Count
                    labels.Add(lbl)
                End If
            Next

            pGroupLabels = labels.ToArray()
            Dim g As Integer = pGroupLabels.Length
            If g < 2 Then CoreServices.Errors.LogAndThrow(New ArgumentException("Discriminant analysis requires at least two groups."))
            If n <= g Then CoreServices.Errors.LogAndThrow(New ArgumentException("The number of complete observations must exceed the number of groups."))

            pGroupStats = New List(Of DiscriminantGroupStatistics)
            pOverallMeanWorking = ColumnMeans(working)

            Dim counts(g - 1) As Integer
            For Each lbl In prepared.ActiveGroupLabels
                counts(groupIndexByLabel(lbl)) += 1
            Next
            For i As Integer = 0 To g - 1
                If counts(i) < 2 Then
                    CoreServices.Errors.LogAndThrow(New ArgumentException($"Group '{pGroupLabels(i)}' must contain at least two complete observations."))
                End If
            Next

            Dim priors() As Double = ResolvePriors(pGroupLabels, counts)
            Dim pooledNumerator(p - 1, p - 1) As Double
            Dim pooledDf As Double = 0.0

            For groupIdx As Integer = 0 To g - 1
                Dim rowIds As List(Of Integer) = RowsForGroup(prepared.ActiveGroupLabels, pGroupLabels(groupIdx))
                Dim groupWorking = ExtractRows(working, rowIds.ToArray())
                Dim groupOriginal = ExtractRows(original, rowIds.ToArray())
                Dim cov As Double(,) = Matrix.MatCovar(groupWorking)
                Dim meanWorking() As Double = ColumnMeans(groupWorking)
                Dim meanOriginal() As Double = ColumnMeans(groupOriginal)
                Dim regularized = RegularizeSymmetricMatrix(cov, pCovarianceRegularization)

                Dim gs As New DiscriminantGroupStatistics
                gs.GroupLabel = pGroupLabels(groupIdx)
                gs.Count = counts(groupIdx)
                gs.PriorProbability = priors(groupIdx)
                gs.MeanOriginal = meanOriginal
                gs.MeanWorking = meanWorking
                gs.CovarianceWorking = regularized.Matrix
                gs.InverseCovarianceWorking = regularized.Inverse
                gs.LogDeterminantWorking = regularized.LogDeterminant
                gs.RegularizationUsed = regularized.RidgeUsed
                pGroupStats.Add(gs)

                If pMethod = DiscriminantAnalysisMethod.Linear Then
                    AddScaledMatrixInPlace(pooledNumerator, cov, counts(groupIdx) - 1)
                    pooledDf += counts(groupIdx) - 1
                End If
            Next

            If pMethod = DiscriminantAnalysisMethod.Linear Then
                If pooledDf <= 0 Then CoreServices.Errors.LogAndThrow(New ArgumentException("The pooled within-group degrees of freedom must be positive for linear discriminant analysis."))
                pPooledCovarianceWorking = Matrix.MatrixMult(pooledNumerator, 1.0 / pooledDf)
                Dim pooledPrepared = RegularizeSymmetricMatrix(pPooledCovarianceWorking, pCovarianceRegularization)
                pPooledCovarianceWorking = pooledPrepared.Matrix
                pPooledInverseWorking = pooledPrepared.Inverse
                pPooledLogDeterminant = pooledPrepared.LogDeterminant
                pPooledRegularization = pooledPrepared.RidgeUsed
                ComputeLinearFunctions(prepared)
                ComputeCanonicalFunctions(prepared)
            Else
                pPooledCovarianceWorking = Nothing
                pPooledInverseWorking = Nothing
                pCanonicalEigenvalues = Nothing
                pCanonicalCorrelations = Nothing
                pCanonicalProportions = Nothing
                pCanonicalWilksLambda = Nothing
                pCanonicalCoefficients = Nothing
                pCanonicalScores = Nothing
                pCanonicalGroupCentroids = Nothing
                pLinearCoefficientsWorking = Nothing
                pLinearConstantsWorking = Nothing
                pLinearCoefficientsOriginal = Nothing
                pLinearConstantsOriginal = Nothing
            End If
        End Sub

        Private Sub ComputeLinearFunctions(prepared As DiscriminantPreparedData)
            Dim p As Integer = prepared.WorkingData.GetUpperBound(1) + 1
            Dim g As Integer = pGroupStats.Count
            ReDim pLinearCoefficientsWorking(p - 1, g - 1)
            ReDim pLinearConstantsWorking(g - 1)
            ReDim pLinearCoefficientsOriginal(p - 1, g - 1)
            ReDim pLinearConstantsOriginal(g - 1)

            For groupIdx As Integer = 0 To g - 1
                Dim mu() As Double = pGroupStats(groupIdx).MeanWorking
                Dim coeffWorking() As Double = MatVec(pPooledInverseWorking, mu)
                For j As Integer = 0 To p - 1
                    pLinearCoefficientsWorking(j, groupIdx) = coeffWorking(j)
                    Dim scale As Double = prepared.ColumnScales(j)
                    If scale = 0 Then scale = 1.0
                    pLinearCoefficientsOriginal(j, groupIdx) = coeffWorking(j) / scale
                Next
                Dim constantWorking As Double = -0.5 * Matrix.DotProduct(mu, coeffWorking) + Math.Log(Math.Max(pGroupStats(groupIdx).PriorProbability, 1.0E-300))
                pLinearConstantsWorking(groupIdx) = constantWorking

                Dim adjust As Double = 0.0
                For j As Integer = 0 To p - 1
                    Dim scale As Double = prepared.ColumnScales(j)
                    If scale = 0 Then scale = 1.0
                    adjust += coeffWorking(j) * prepared.ColumnLocations(j) / scale
                Next
                pLinearConstantsOriginal(groupIdx) = constantWorking - adjust
            Next
        End Sub

        Private Sub ComputeCanonicalFunctions(prepared As DiscriminantPreparedData)
            Dim p As Integer = prepared.WorkingData.GetUpperBound(1) + 1
            Dim g As Integer = pGroupStats.Count
            Dim n As Integer = prepared.WorkingData.GetUpperBound(0) + 1
            Dim nRoots As Integer = Math.Min(p, g - 1)
            If nRoots <= 0 Then
                pCanonicalEigenvalues = Nothing
                pCanonicalCorrelations = Nothing
                pCanonicalProportions = Nothing
                pCanonicalWilksLambda = Nothing
                pCanonicalCoefficients = Nothing
                pCanonicalScores = Nothing
                pCanonicalGroupCentroids = Nothing
                Return
            End If

            Dim between(p - 1, p - 1) As Double
            For Each gs In pGroupStats
                Dim delta() As Double = Matrix.M_SUB(gs.MeanWorking, pOverallMeanWorking)
                Dim outer = Matrix.M_OUTERPRODUCT(delta, delta)
                AddScaledMatrixInPlace(between, outer, gs.Count / Math.Max(n - 1.0, 1.0))
            Next

            Dim eigW = Matrix.EIGEN_JK(pPooledCovarianceWorking)
            Dim sortedW = MultivariateShared.SortEigenpairsDescending(eigW.Item1, eigW.Item2)
            Dim invSqrtW(p - 1, p - 1) As Double
            Dim tol As Double = 0.000000000001
            For k As Integer = 0 To p - 1
                Dim ev As Double = sortedW.Item1(k)
                Dim w As Double = If(ev > tol, 1.0 / Math.Sqrt(ev), 0.0)
                For i As Integer = 0 To p - 1
                    For j As Integer = 0 To p - 1
                        invSqrtW(i, j) += sortedW.Item2(i, k) * w * sortedW.Item2(j, k)
                    Next
                Next
            Next

            Dim tmp = Matrix.MatrixMult(invSqrtW, between)
            Dim a = Matrix.MatrixMult(tmp, invSqrtW)
            a = Symmetrize(a)

            Dim eigA = Matrix.EIGEN_JK(a)
            Dim sortedA = MultivariateShared.SortEigenpairsDescending(eigA.Item1, eigA.Item2)

            Dim positiveRoots As New List(Of Integer)
            For i As Integer = 0 To sortedA.Item1.Length - 1
                If sortedA.Item1(i) > 0.0000000001 Then positiveRoots.Add(i)
                If positiveRoots.Count = nRoots Then Exit For
            Next

            If positiveRoots.Count = 0 Then
                pCanonicalEigenvalues = Nothing
                pCanonicalCorrelations = Nothing
                pCanonicalProportions = Nothing
                pCanonicalWilksLambda = Nothing
                pCanonicalCoefficients = Nothing
                pCanonicalScores = Nothing
                pCanonicalGroupCentroids = Nothing
                Return
            End If

            nRoots = positiveRoots.Count
            ReDim pCanonicalEigenvalues(nRoots - 1)
            ReDim pCanonicalCorrelations(nRoots - 1)
            ReDim pCanonicalProportions(nRoots - 1)
            ReDim pCanonicalWilksLambda(nRoots - 1)
            ReDim pCanonicalCoefficients(p - 1, nRoots - 1)

            Dim totalEig As Double = 0.0
            For root As Integer = 0 To nRoots - 1
                pCanonicalEigenvalues(root) = sortedA.Item1(positiveRoots(root))
                totalEig += pCanonicalEigenvalues(root)
            Next

            For root As Integer = 0 To nRoots - 1
                Dim idx As Integer = positiveRoots(root)
                Dim v(p - 1, 0) As Double
                For i As Integer = 0 To p - 1
                    v(i, 0) = sortedA.Item2(i, idx)
                Next
                Dim coeff = Matrix.MatrixMult(invSqrtW, v)
                For i As Integer = 0 To p - 1
                    pCanonicalCoefficients(i, root) = coeff(i, 0)
                Next
                pCanonicalCorrelations(root) = Math.Sqrt(pCanonicalEigenvalues(root) / (1.0 + pCanonicalEigenvalues(root)))
                pCanonicalProportions(root) = If(totalEig > 0.0, pCanonicalEigenvalues(root) / totalEig, 0.0)
            Next

            For root As Integer = 0 To nRoots - 1
                Dim wilks As Double = 1.0
                For j As Integer = root To nRoots - 1
                    wilks *= 1.0 / (1.0 + pCanonicalEigenvalues(j))
                Next
                pCanonicalWilksLambda(root) = wilks
            Next

            pCanonicalScores = Matrix.MatrixMult(prepared.WorkingData, pCanonicalCoefficients)
            ReDim pCanonicalGroupCentroids(g - 1, nRoots - 1)
            For groupIdx As Integer = 0 To g - 1
                Dim rows = RowsForGroup(prepared.ActiveGroupLabels, pGroupLabels(groupIdx))
                For root As Integer = 0 To nRoots - 1
                    Dim s As Double = 0.0
                    For Each r In rows
                        s += pCanonicalScores(r, root)
                    Next
                    pCanonicalGroupCentroids(groupIdx, root) = s / Math.Max(rows.Count, 1)
                Next
            Next

            For root As Integer = 0 To nRoots - 1
                Dim bestValue As Double = 0.0
                Dim bestGroup As Integer = 0
                For groupIdx As Integer = 0 To g - 1
                    If Math.Abs(pCanonicalGroupCentroids(groupIdx, root)) > Math.Abs(bestValue) Then
                        bestValue = pCanonicalGroupCentroids(groupIdx, root)
                        bestGroup = groupIdx
                    End If
                Next
                If pCanonicalGroupCentroids(bestGroup, root) < 0.0 Then
                    For i As Integer = 0 To p - 1
                        pCanonicalCoefficients(i, root) *= -1.0
                    Next
                    For i As Integer = 0 To pCanonicalScores.GetUpperBound(0)
                        pCanonicalScores(i, root) *= -1.0
                    Next
                    For groupIdx As Integer = 0 To g - 1
                        pCanonicalGroupCentroids(groupIdx, root) *= -1.0
                    Next
                End If
            Next
        End Sub

        Private Function PredictInternal(newData(,) As Double,
                                         rowLabels() As String,
                                         rowIndices() As Integer,
                                         actualLabels() As String,
                                         validationMode As DiscriminantValidationMode,
                                         foldAssignments() As Integer) As DiscriminantPredictionResult
            MultivariateInputHelpers.ValidateRectangularData(newData, nullParamName:=NameOf(newData), rankMessage:="The data matrix must be two-dimensional.", emptyMessage:="The data matrix must contain at least one row and one column.")
            Dim n As Integer = newData.GetUpperBound(0) + 1
            Dim p As Integer = newData.GetUpperBound(1) + 1
            If p <> pPrepared.ActiveOriginalData.GetUpperBound(1) + 1 Then
                CoreServices.Errors.LogAndThrow(New ArgumentException("The supplied prediction data do not have the expected number of variables."))
            End If
            If actualLabels IsNot Nothing AndAlso actualLabels.Length <> n Then
                CoreServices.Errors.LogAndThrow(New ArgumentException("The actual-group label array must match the number of prediction rows."))
            End If

            Dim working As Double(,) = TransformExternalData(newData, pPrepared)
            Dim g As Integer = pGroupStats.Count
            Dim predicted(n - 1) As String
            Dim assignedPosterior(n - 1) As Double
            Dim scores(n - 1, g - 1) As Double
            Dim distances(n - 1, g - 1) As Double
            Dim posterior(n - 1, g - 1) As Double
            Dim correct() As Boolean = Nothing
            If actualLabels IsNot Nothing Then ReDim correct(n - 1)

            For i As Integer = 0 To n - 1
                Dim x() As Double = Matrix.rowFromArray(working, i)
                Dim raw(g - 1) As Double
                Dim d2(g - 1) As Double
                For groupIdx As Integer = 0 To g - 1
                    If pMethod = DiscriminantAnalysisMethod.Linear Then
                        Dim mu() As Double = pGroupStats(groupIdx).MeanWorking
                        Dim delta = Matrix.M_SUB(x, mu)
                        d2(groupIdx) = QuadraticForm(delta, pPooledInverseWorking)
                        Dim coeff() As Double = Matrix.GetColumnFrom2Darray(pLinearCoefficientsWorking, groupIdx)
                        raw(groupIdx) = Matrix.DotProduct(x, coeff) + pLinearConstantsWorking(groupIdx)
                    Else
                        Dim gs = pGroupStats(groupIdx)
                        Dim delta = Matrix.M_SUB(x, gs.MeanWorking)
                        d2(groupIdx) = QuadraticForm(delta, gs.InverseCovarianceWorking)
                        raw(groupIdx) = -0.5 * (gs.LogDeterminantWorking + d2(groupIdx)) + Math.Log(Math.Max(gs.PriorProbability, 1.0E-300))
                    End If
                Next

                Dim probs() As Double = Softmax(raw)
                Dim bestIdx As Integer = ArgMax(probs)
                predicted(i) = pGroupLabels(bestIdx)
                assignedPosterior(i) = probs(bestIdx)
                If actualLabels IsNot Nothing Then correct(i) = String.Equals(actualLabels(i), predicted(i), StringComparison.Ordinal)
                For j As Integer = 0 To g - 1
                    scores(i, j) = raw(j)
                    distances(i, j) = d2(j)
                    posterior(i, j) = probs(j)
                Next
            Next

            Dim result As New DiscriminantPredictionResult
            result.ValidationMode = validationMode
            result.RowLabels = rowLabels
            result.RowIndices = rowIndices
            result.ActualGroupLabels = actualLabels
            result.PredictedGroupLabels = predicted
            result.AssignedPosteriorProbability = assignedPosterior
            result.PosteriorProbabilities = posterior
            result.RawScores = scores
            result.SquaredDistances = distances
            result.CorrectClassification = correct
            result.FoldAssignments = foldAssignments
            If actualLabels IsNot Nothing Then
                result.Confusion = BuildConfusion(actualLabels, predicted, pGroupLabels)
            End If
            Return result
        End Function

        Private Function RunValidation() As DiscriminantPredictionResult
            Select Case pValidationMode
                Case DiscriminantValidationMode.LeaveOneOut
                    Return RunLeaveOneOutValidation()
                Case DiscriminantValidationMode.KFold
                    Return RunKFoldValidation()
                Case DiscriminantValidationMode.Holdout
                    Return RunHoldoutValidation()
                Case Else
                    Return Nothing
            End Select
        End Function

        Private Function RunLeaveOneOutValidation() As DiscriminantPredictionResult
            Dim n As Integer = pPrepared.ActiveOriginalData.GetUpperBound(0) + 1
            Dim predicted(n - 1) As String
            Dim assignedPosterior(n - 1) As Double
            Dim g As Integer = pGroupLabels.Length
            Dim posterior(n - 1, g - 1) As Double
            Dim scores(n - 1, g - 1) As Double
            Dim distances(n - 1, g - 1) As Double
            Dim foldAssign(n - 1) As Integer

            Dim countsByGroup = CountByLabel(pPrepared.ActiveGroupLabels)
            For Each lbl In pGroupLabels
                If countsByGroup(lbl) < 3 Then
                    CoreServices.Errors.LogAndThrow(New ArgumentException($"Leave-one-out validation requires at least three complete observations in group '{lbl}'."))
                End If
            Next

            For i As Integer = 0 To n - 1
                Dim iIn As Integer = i
                foldAssign(i) = i + 1
                Dim trainRows = Enumerable.Range(0, n).Where(Function(idx) idx <> iIn).ToArray()
                Dim testRows = New Integer() {i}
                Dim model = CreateSubmodel(trainRows)
                Dim pred = model.PredictDetailed(ExtractRows(pPrepared.ActiveOriginalData, testRows),
                                                New String() {pPrepared.ActiveRowLabels(i)},
                                                New Object() {pPrepared.ActiveGroupLabels(i)})
                predicted(i) = pred.PredictedGroupLabels(0)
                assignedPosterior(i) = pred.AssignedPosteriorProbability(0)
                For j As Integer = 0 To g - 1
                    posterior(i, j) = pred.PosteriorProbabilities(0, j)
                    scores(i, j) = pred.RawScores(0, j)
                    distances(i, j) = pred.SquaredDistances(0, j)
                Next
            Next

            Dim actual = CType(pPrepared.ActiveGroupLabels.Clone(), String())
            Dim result As New DiscriminantPredictionResult
            result.ValidationMode = DiscriminantValidationMode.LeaveOneOut
            result.RowIndices = CType(pPrepared.ActiveOriginalIndices.Clone(), Integer())
            result.RowLabels = CType(pPrepared.ActiveRowLabels.Clone(), String())
            result.ActualGroupLabels = actual
            result.PredictedGroupLabels = predicted
            result.AssignedPosteriorProbability = assignedPosterior
            result.PosteriorProbabilities = posterior
            result.RawScores = scores
            result.SquaredDistances = distances
            result.FoldAssignments = foldAssign
            ReDim result.CorrectClassification(n - 1)
            For i As Integer = 0 To n - 1
                result.CorrectClassification(i) = String.Equals(actual(i), predicted(i), StringComparison.Ordinal)
            Next
            result.Confusion = BuildConfusion(actual, predicted, pGroupLabels)
            Return result
        End Function

        Private Function RunKFoldValidation() As DiscriminantPredictionResult
            Dim n As Integer = pPrepared.ActiveOriginalData.GetUpperBound(0) + 1
            Dim actual = CType(pPrepared.ActiveGroupLabels.Clone(), String())
            Dim foldAssignments = BuildFoldAssignments(actual, pValidationFolds, pValidationStratified)
            Dim predicted(n - 1) As String
            Dim assignedPosterior(n - 1) As Double
            Dim g As Integer = pGroupLabels.Length
            Dim posterior(n - 1, g - 1) As Double
            Dim scores(n - 1, g - 1) As Double
            Dim distances(n - 1, g - 1) As Double

            For fold As Integer = 1 To pValidationFolds
                Dim foldi As Integer = fold
                Dim testRows = Enumerable.Range(0, n).Where(Function(i) foldAssignments(i) = foldi).ToArray()
                If testRows.Length = 0 Then Continue For
                Dim trainRows = Enumerable.Range(0, n).Where(Function(i) foldAssignments(i) <> foldi).ToArray()
                Dim model = CreateSubmodel(trainRows)
                Dim pred = model.PredictDetailed(ExtractRows(pPrepared.ActiveOriginalData, testRows),
                                                ExtractRows(pPrepared.ActiveRowLabels, testRows),
                                                ExtractObjects(actual, testRows))
                For localIdx As Integer = 0 To testRows.Length - 1
                    Dim rowIdx As Integer = testRows(localIdx)
                    predicted(rowIdx) = pred.PredictedGroupLabels(localIdx)
                    assignedPosterior(rowIdx) = pred.AssignedPosteriorProbability(localIdx)
                    For j As Integer = 0 To g - 1
                        posterior(rowIdx, j) = pred.PosteriorProbabilities(localIdx, j)
                        scores(rowIdx, j) = pred.RawScores(localIdx, j)
                        distances(rowIdx, j) = pred.SquaredDistances(localIdx, j)
                    Next
                Next
            Next

            Dim result As New DiscriminantPredictionResult
            result.ValidationMode = DiscriminantValidationMode.KFold
            result.RowIndices = CType(pPrepared.ActiveOriginalIndices.Clone(), Integer())
            result.RowLabels = CType(pPrepared.ActiveRowLabels.Clone(), String())
            result.ActualGroupLabels = actual
            result.PredictedGroupLabels = predicted
            result.AssignedPosteriorProbability = assignedPosterior
            result.PosteriorProbabilities = posterior
            result.RawScores = scores
            result.SquaredDistances = distances
            result.FoldAssignments = foldAssignments
            ReDim result.CorrectClassification(n - 1)
            For i As Integer = 0 To n - 1
                result.CorrectClassification(i) = String.Equals(actual(i), predicted(i), StringComparison.Ordinal)
            Next
            result.Confusion = BuildConfusion(actual, predicted, pGroupLabels)
            Return result
        End Function

        Private Function RunHoldoutValidation() As DiscriminantPredictionResult
            Dim n As Integer = pPrepared.ActiveOriginalData.GetUpperBound(0) + 1
            If pHoldoutFraction <= 0.0 OrElse pHoldoutFraction >= 1.0 Then
                CoreServices.Errors.LogAndThrow(New ArgumentException("Holdout validation requires a fraction strictly between 0 and 1."))
            End If

            Dim rng As Random = CreateRandomForValidation()
            Dim allGroups = CType(pPrepared.ActiveGroupLabels.Clone(), String())
            Dim testRows As New List(Of Integer)
            Dim trainRows As New List(Of Integer)

            If pValidationStratified Then
                For Each lbl In pGroupLabels
                    Dim rows = RowsForGroup(allGroups, lbl)
                    ShuffleInPlace(rows, rng)
                    Dim nTest As Integer = Math.Max(1, CInt(Math.Round(rows.Count * pHoldoutFraction)))
                    nTest = Math.Min(nTest, rows.Count - 2)
                    If nTest <= 0 Then
                        CoreServices.Errors.LogAndThrow(New ArgumentException($"Group '{lbl}' does not have enough rows for the requested holdout validation."))
                    End If
                    testRows.AddRange(rows.Take(nTest))
                    trainRows.AddRange(rows.Skip(nTest))
                Next
            Else
                Dim all = Enumerable.Range(0, n).ToList()
                ShuffleInPlace(all, rng)
                Dim nTest As Integer = Math.Max(1, CInt(Math.Round(n * pHoldoutFraction)))
                testRows.AddRange(all.Take(nTest))
                trainRows.AddRange(all.Skip(nTest))
            End If

            Dim trainArray = trainRows.OrderBy(Function(x) x).ToArray()
            Dim testArray = testRows.OrderBy(Function(x) x).ToArray()
            Dim trainCounts = CountByLabel(ExtractRows(allGroups, trainArray))
            For Each lbl In pGroupLabels
                If Not trainCounts.ContainsKey(lbl) OrElse trainCounts(lbl) < 2 Then
                    CoreServices.Errors.LogAndThrow(New ArgumentException($"The requested holdout split leaves fewer than two training observations in group '{lbl}'."))
                End If
            Next

            Dim model = CreateSubmodel(trainArray)
            Dim pred = model.PredictDetailed(ExtractRows(pPrepared.ActiveOriginalData, testArray),
                                            ExtractRows(pPrepared.ActiveRowLabels, testArray),
                                            ExtractObjects(allGroups, testArray))

            Dim result As New DiscriminantPredictionResult
            result.ValidationMode = DiscriminantValidationMode.Holdout
            result.RowIndices = ExtractRows(pPrepared.ActiveOriginalIndices, testArray)
            result.RowLabels = ExtractRows(pPrepared.ActiveRowLabels, testArray)
            result.ActualGroupLabels = ExtractRows(allGroups, testArray)
            result.PredictedGroupLabels = pred.PredictedGroupLabels
            result.AssignedPosteriorProbability = pred.AssignedPosteriorProbability
            result.PosteriorProbabilities = pred.PosteriorProbabilities
            result.RawScores = pred.RawScores
            result.SquaredDistances = pred.SquaredDistances
            result.FoldAssignments = Enumerable.Repeat(1, testArray.Length).ToArray()
            result.CorrectClassification = pred.CorrectClassification
            result.Confusion = BuildConfusion(result.ActualGroupLabels, result.PredictedGroupLabels, pGroupLabels)
            Return result
        End Function

        Private Function CreateSubmodel(trainRows() As Integer) As DiscriminantAnalysis
            Dim model As New DiscriminantAnalysis
            model.dataInputs(ExtractRows(pPrepared.ActiveOriginalData, trainRows),
                             ExtractObjects(pPrepared.ActiveGroupLabels, trainRows),
                             ExtractRows(pPrepared.ActiveRowLabels, trainRows),
                             CType(pPrepared.VariableNames.Clone(), String()))
            model.settingsInputs(pMethod, pStandardization, ClusterMissingValuePolicy.ErrorOnMissing, pPriorMode, pCovarianceRegularization)
            If pPriorMode = DiscriminantPriorMode.UserSpecified Then
                model.priorInputs(ConvertStringsToObjects(pUserPriorLabels), pUserPriorValues)
            End If
            model.validationInputs(DiscriminantValidationMode.None)
            model.Fit()
            Return model
        End Function

        Private Function BuildSettingsTable() As Object(,)
            Dim out(1, 12) As Object
            out(0, 0) = "Method"
            out(0, 1) = "Validation"
            out(0, 2) = "ValidationParameter"
            out(0, 3) = "StratifiedValidation"
            out(0, 4) = "Standardization"
            out(0, 5) = "MissingValuePolicy"
            out(0, 6) = "PriorMode"
            out(0, 7) = "ActiveObservations"
            out(0, 8) = "RemovedObservations"
            out(0, 9) = "Variables"
            out(0, 10) = "Groups"
            out(0, 11) = "CovarianceRegularization"
            out(0, 12) = "RandomSeed"

            out(1, 0) = pMethod.ToString()
            out(1, 1) = pValidationMode.ToString()
            Select Case pValidationMode
                Case DiscriminantValidationMode.KFold
                    out(1, 2) = pValidationFolds
                Case DiscriminantValidationMode.Holdout
                    out(1, 2) = pHoldoutFraction
                Case Else
                    out(1, 2) = String.Empty
            End Select
            out(1, 3) = pValidationStratified
            out(1, 4) = pPrepared.Standardization.ToString()
            out(1, 5) = pMissingPolicy.ToString()
            out(1, 6) = pPriorMode.ToString()
            out(1, 7) = pPrepared.ActiveGroupLabels.Length
            out(1, 8) = If(pPrepared.RemovedOriginalIndices Is Nothing, 0, pPrepared.RemovedOriginalIndices.Length)
            out(1, 9) = pPrepared.VariableNames.Length
            out(1, 10) = pGroupLabels.Length
            out(1, 11) = pCovarianceRegularization
            out(1, 12) = If(pRandomSeed = Integer.MinValue, CType(String.Empty, Object), pRandomSeed)
            Return out
        End Function

        Private Function BuildGroupSummaryTable() As Object(,)
            Dim out(pGroupStats.Count, 5) As Object
            out(0, 0) = "Group"
            out(0, 1) = "Count"
            out(0, 2) = "Prior"
            out(0, 3) = "LogDet(Cov)"
            out(0, 4) = "RegularizationUsed"
            out(0, 5) = "PctOfActive"

            Dim n As Double = pPrepared.ActiveGroupLabels.Length
            For i As Integer = 0 To pGroupStats.Count - 1
                out(i + 1, 0) = pGroupStats(i).GroupLabel
                out(i + 1, 1) = pGroupStats(i).Count
                out(i + 1, 2) = pGroupStats(i).PriorProbability
                out(i + 1, 3) = pGroupStats(i).LogDeterminantWorking
                out(i + 1, 4) = pGroupStats(i).RegularizationUsed
                out(i + 1, 5) = 100.0 * pGroupStats(i).Count / Math.Max(n, 1.0)
            Next

            Return out
        End Function

        Private Function BuildGroupMeansTable(useWorkingScale As Boolean) As Object(,)
            Dim p As Integer = pPrepared.VariableNames.Length
            Dim out(pGroupStats.Count, p) As Object
            out(0, 0) = "Group"
            For j As Integer = 0 To p - 1
                out(0, j + 1) = pPrepared.VariableNames(j)
            Next
            For i As Integer = 0 To pGroupStats.Count - 1
                out(i + 1, 0) = pGroupStats(i).GroupLabel
                Dim means() As Double = If(useWorkingScale, pGroupStats(i).MeanWorking, pGroupStats(i).MeanOriginal)
                For j As Integer = 0 To p - 1
                    out(i + 1, j + 1) = means(j)
                Next
            Next
            Return out
        End Function

        Private Function BuildPreprocessingTable() As Object(,)
            Dim p As Integer = pPrepared.VariableNames.Length
            Dim out(p, 2) As Object
            out(0, 0) = "Variable"
            out(0, 1) = "Location"
            out(0, 2) = "Scale"
            For j As Integer = 0 To p - 1
                out(j + 1, 0) = pPrepared.VariableNames(j)
                out(j + 1, 1) = pPrepared.ColumnLocations(j)
                out(j + 1, 2) = pPrepared.ColumnScales(j)
            Next
            Return out
        End Function

        Private Function BuildCovarianceTable(cov(,) As Double) As Object(,)
            Dim p As Integer = cov.GetUpperBound(0) + 1
            Dim out(p, p) As Object
            out(0, 0) = "Variable"
            For j As Integer = 0 To p - 1
                out(0, j + 1) = pPrepared.VariableNames(j)
            Next
            For i As Integer = 0 To p - 1
                out(i + 1, 0) = pPrepared.VariableNames(i)
                For j As Integer = 0 To p - 1
                    out(i + 1, j + 1) = cov(i, j)
                Next
            Next
            Return out
        End Function

        Private Function BuildLinearFunctionTable() As Object(,)
            Dim p As Integer = pPrepared.VariableNames.Length
            Dim g As Integer = pGroupLabels.Length
            Dim out(p + 1, g) As Object
            out(0, 0) = "Term"
            For j As Integer = 0 To g - 1
                out(0, j + 1) = pGroupLabels(j)
            Next
            out(1, 0) = "Constant"
            For j As Integer = 0 To g - 1
                out(1, j + 1) = pLinearConstantsOriginal(j)
            Next
            For i As Integer = 0 To p - 1
                out(i + 2, 0) = pPrepared.VariableNames(i)
                For j As Integer = 0 To g - 1
                    out(i + 2, j + 1) = pLinearCoefficientsOriginal(i, j)
                Next
            Next
            Return out
        End Function

        Private Function BuildCanonicalSummaryTable() As Object(,)
            Dim m As Integer = pCanonicalEigenvalues.Length
            Dim out(m, 4) As Object
            out(0, 0) = "Function"
            out(0, 1) = "Eigenvalue"
            out(0, 2) = "CanonicalCorrelation"
            out(0, 3) = "Proportion"
            out(0, 4) = "WilksLambda(step-down)"
            For i As Integer = 0 To m - 1
                out(i + 1, 0) = $"Function {i + 1}"
                out(i + 1, 1) = pCanonicalEigenvalues(i)
                out(i + 1, 2) = pCanonicalCorrelations(i)
                out(i + 1, 3) = pCanonicalProportions(i)
                out(i + 1, 4) = pCanonicalWilksLambda(i)
            Next
            Return out
        End Function

        Private Function BuildCanonicalCoefficientTable() As Object(,)
            Dim p As Integer = pPrepared.VariableNames.Length
            Dim m As Integer = pCanonicalEigenvalues.Length
            Dim out(p, m) As Object
            out(0, 0) = "Variable"
            For j As Integer = 0 To m - 1
                out(0, j + 1) = $"Function {j + 1}"
            Next
            For i As Integer = 0 To p - 1
                out(i + 1, 0) = pPrepared.VariableNames(i)
                For j As Integer = 0 To m - 1
                    out(i + 1, j + 1) = pCanonicalCoefficients(i, j)
                Next
            Next
            Return out
        End Function

        Private Function BuildCanonicalCentroidTable() As Object(,)
            Dim g As Integer = pGroupLabels.Length
            Dim m As Integer = pCanonicalEigenvalues.Length
            Dim out(g, m) As Object
            out(0, 0) = "Group"
            For j As Integer = 0 To m - 1
                out(0, j + 1) = $"Function {j + 1}"
            Next
            For i As Integer = 0 To g - 1
                out(i + 1, 0) = pGroupLabels(i)
                For j As Integer = 0 To m - 1
                    out(i + 1, j + 1) = pCanonicalGroupCentroids(i, j)
                Next
            Next
            Return out
        End Function

        Private Function BuildRemovedRowsTable() As Object(,)
            If pPrepared.RemovedOriginalIndices Is Nothing OrElse pPrepared.RemovedOriginalIndices.Length = 0 Then Return Nothing
            Dim out(pPrepared.RemovedOriginalIndices.Length, 1) As Object
            out(0, 0) = "OriginalRow"
            out(0, 1) = "Label"
            For i As Integer = 0 To pPrepared.RemovedOriginalIndices.Length - 1
                out(i + 1, 0) = pPrepared.RemovedOriginalIndices(i)
                out(i + 1, 1) = pPrepared.RemovedRowLabels(i)
            Next
            Return out
        End Function

        Private Function PrepareData(data(,) As Double,
                                     groupLabels() As Object,
                                     rowLabels() As String,
                                     varNames() As String,
                                     standardization As ClusterStandardizationMode,
                                     missingPolicy As ClusterMissingValuePolicy) As DiscriminantPreparedData
            MultivariateInputHelpers.ValidateRectangularData(data, nullParamName:=NameOf(data), rankMessage:="The data matrix must be two-dimensional.", emptyMessage:="The data matrix must contain at least one row and one column.")
            Dim n As Integer = data.GetUpperBound(0) + 1
            Dim p As Integer = data.GetUpperBound(1) + 1
            Dim finalRowLabels() As String = MultivariateInputHelpers.NormalizeRowLabels(rowLabels, n, defaultPrefix:="Row", allowDefaultOnLengthMismatch:=True)
            Dim finalVarNames() As String = MultivariateInputHelpers.NormalizeVarNames(varNames, p, defaultPrefix:="X", allowDefaultOnLengthMismatch:=True)


            Dim keepRow(n - 1) As Boolean
            Dim removedIndices As New List(Of Integer)
            Dim removedLabels As New List(Of String)
            Dim activeCount As Integer = 0

            For i As Integer = 0 To n - 1
                Dim hasMissing As Boolean = False
                For j As Integer = 0 To p - 1
                    If Double.IsNaN(data(i, j)) OrElse Double.IsInfinity(data(i, j)) Then
                        hasMissing = True
                        Exit For
                    End If
                Next
                Dim groupLabel As String = NormalizeGroupLabel(groupLabels(i))
                If groupLabel.Length = 0 Then hasMissing = True

                If hasMissing Then
                    If missingPolicy = ClusterMissingValuePolicy.ErrorOnMissing Then
                        CoreServices.Errors.LogAndThrow(New ArgumentException($"Missing or non-finite value found in row {i + 1}."))
                    End If
                    keepRow(i) = False
                    removedIndices.Add(i + 1)
                    removedLabels.Add(finalRowLabels(i))
                Else
                    keepRow(i) = True
                    activeCount += 1
                End If
            Next

            If activeCount = 0 Then
                CoreServices.Errors.LogAndThrow(New ArgumentException("No complete observations remain after preprocessing."))
            End If

            Dim activeOriginalData(activeCount - 1, p - 1) As Double
            Dim workingData(activeCount - 1, p - 1) As Double
            Dim activeGroups(activeCount - 1) As String
            Dim activeLabels(activeCount - 1) As String
            Dim activeIndices(activeCount - 1) As Integer
            Dim rowOut As Integer = 0

            For i As Integer = 0 To n - 1
                If Not keepRow(i) Then Continue For
                activeGroups(rowOut) = NormalizeGroupLabel(groupLabels(i))
                activeLabels(rowOut) = finalRowLabels(i)
                activeIndices(rowOut) = i + 1
                For j As Integer = 0 To p - 1
                    activeOriginalData(rowOut, j) = data(i, j)
                Next
                rowOut += 1
            Next

            Dim locations(p - 1) As Double
            Dim scales(p - 1) As Double
            ComputeStandardizationParameters(activeOriginalData, standardization, locations, scales)
            For i As Integer = 0 To activeCount - 1
                For j As Integer = 0 To p - 1
                    workingData(i, j) = TransformValue(activeOriginalData(i, j), standardization, locations(j), scales(j))
                Next
            Next

            Dim prepared As New DiscriminantPreparedData
            prepared.WorkingData = workingData
            prepared.ActiveOriginalData = activeOriginalData
            prepared.ActiveGroupLabels = activeGroups
            prepared.ActiveRowLabels = activeLabels
            prepared.ActiveOriginalIndices = activeIndices
            prepared.RemovedOriginalIndices = removedIndices.ToArray()
            prepared.RemovedRowLabels = removedLabels.ToArray()
            prepared.VariableNames = finalVarNames
            prepared.ColumnLocations = locations
            prepared.ColumnScales = scales
            prepared.Standardization = standardization
            Return prepared
        End Function

        Private Function ResolvePriors(groupLabels() As String, counts() As Integer) As Double()
            Dim g As Integer = groupLabels.Length
            Dim priors(g - 1) As Double
            Select Case pPriorMode
                Case DiscriminantPriorMode.ProportionalToGroupSizes
                    Dim total As Double = counts.Sum()
                    For i As Integer = 0 To g - 1
                        priors(i) = counts(i) / total
                    Next
                Case DiscriminantPriorMode.Equal
                    For i As Integer = 0 To g - 1
                        priors(i) = 1.0 / g
                    Next
                Case DiscriminantPriorMode.UserSpecified
                    If pUserPriorLabels Is Nothing OrElse pUserPriorValues Is Nothing Then
                        CoreServices.Errors.LogAndThrow(New ArgumentException("User-specified priors were requested but no prior values were supplied."))
                    End If
                    Dim priorMap As New Dictionary(Of String, Double)(StringComparer.Ordinal)
                    For i As Integer = 0 To pUserPriorLabels.Length - 1
                        If pUserPriorValues(i) <= 0 Then
                            CoreServices.Errors.LogAndThrow(New ArgumentException("All user-specified prior probabilities must be positive."))
                        End If
                        priorMap(pUserPriorLabels(i)) = pUserPriorValues(i)
                    Next
                    For i As Integer = 0 To g - 1
                        If Not priorMap.ContainsKey(groupLabels(i)) Then
                            CoreServices.Errors.LogAndThrow(New ArgumentException($"No user prior was supplied for group '{groupLabels(i)}'."))
                        End If
                        priors(i) = priorMap(groupLabels(i))
                    Next
                    Dim s As Double = priors.Sum()
                    If s <= 0 Then CoreServices.Errors.LogAndThrow(New ArgumentException("The user-specified priors must sum to a positive value."))
                    For i As Integer = 0 To g - 1
                        priors(i) /= s
                    Next
            End Select
            Return priors
        End Function

        Private Function BuildConfusion(actual() As String,
                                        predicted() As String,
                                        classLabels() As String) As DiscriminantConfusionMatrix
            Dim g As Integer = classLabels.Length
            Dim counts(g - 1, g - 1) As Double
            Dim indexOf As New Dictionary(Of String, Integer)(StringComparer.Ordinal)
            For i As Integer = 0 To g - 1
                indexOf(classLabels(i)) = i
            Next
            For i As Integer = 0 To actual.Length - 1
                Dim obs As Integer = indexOf(actual(i))
                Dim pred As Integer = indexOf(predicted(i))
                counts(obs, pred) += 1.0
            Next

            Dim rowTotals(g - 1) As Double
            Dim colTotals(g - 1) As Double
            Dim recall(g - 1) As Double
            Dim precision(g - 1) As Double
            Dim total As Double = 0.0
            Dim diag As Double = 0.0
            For i As Integer = 0 To g - 1
                For j As Integer = 0 To g - 1
                    rowTotals(i) += counts(i, j)
                    colTotals(j) += counts(i, j)
                    total += counts(i, j)
                    If i = j Then diag += counts(i, j)
                Next
            Next
            For i As Integer = 0 To g - 1
                recall(i) = If(rowTotals(i) > 0, 100.0 * counts(i, i) / rowTotals(i), Double.NaN)
                precision(i) = If(colTotals(i) > 0, 100.0 * counts(i, i) / colTotals(i), Double.NaN)
            Next

            Dim result As New DiscriminantConfusionMatrix
            result.ClassLabels = CType(classLabels.Clone(), String())
            result.Counts = counts
            result.RowTotals = rowTotals
            result.ColumnTotals = colTotals
            result.RecallPct = recall
            result.PrecisionPct = precision
            result.OverallAccuracy = If(total > 0.0, diag / total, Double.NaN)
            result.OverallAccuracyPct = 100.0 * result.OverallAccuracy
            Return result
        End Function

        Private Function BuildFoldAssignments(actual() As String,
                                              k As Integer,
                                              stratified As Boolean) As Integer()
            Dim n As Integer = actual.Length
            Dim folds(n - 1) As Integer
            Dim rng As Random = CreateRandomForValidation()

            If stratified Then
                For Each lbl In pGroupLabels
                    Dim rows = RowsForGroup(actual, lbl)
                    If rows.Count < k Then
                        CoreServices.Errors.LogAndThrow(New ArgumentException($"{k}-fold validation requires at least {k} complete observations in group '{lbl}'."))
                    End If
                    ShuffleInPlace(rows, rng)
                    For i As Integer = 0 To rows.Count - 1
                        folds(rows(i)) = (i Mod k) + 1
                    Next
                Next
            Else
                Dim rows = Enumerable.Range(0, n).ToList()
                ShuffleInPlace(rows, rng)
                For i As Integer = 0 To n - 1
                    folds(rows(i)) = (i Mod k) + 1
                Next
            End If

            For fold As Integer = 1 To k
                Dim foldi As Integer = fold
                Dim trainRows = Enumerable.Range(0, n).Where(Function(i) folds(i) <> foldi).ToArray()
                Dim trainCounts = CountByLabel(ExtractRows(actual, trainRows))
                For Each lbl In pGroupLabels
                    If Not trainCounts.ContainsKey(lbl) OrElse trainCounts(lbl) < 2 Then
                        CoreServices.Errors.LogAndThrow(New ArgumentException($"The requested validation split leaves fewer than two training observations in group '{lbl}'."))
                    End If
                Next
            Next
            Return folds
        End Function

        Private Function TransformExternalData(data(,) As Double, prepared As DiscriminantPreparedData) As Double(,)
            MultivariateInputHelpers.ValidateRectangularData(data, nullParamName:=NameOf(data), rankMessage:="The data matrix must be two-dimensional.", emptyMessage:="The data matrix must contain at least one row and one column.")
            Dim n As Integer = data.GetUpperBound(0) + 1
            Dim p As Integer = data.GetUpperBound(1) + 1
            Dim out(n - 1, p - 1) As Double
            For i As Integer = 0 To n - 1
                For j As Integer = 0 To p - 1
                    If Double.IsNaN(data(i, j)) OrElse Double.IsInfinity(data(i, j)) Then
                        CoreServices.Errors.LogAndThrow(New ArgumentException($"Missing or non-finite value found in prediction row {i + 1}."))
                    End If
                    out(i, j) = TransformValue(data(i, j), prepared.Standardization, prepared.ColumnLocations(j), prepared.ColumnScales(j))
                Next
            Next
            Return out
        End Function

        Private Function CreateRandomForValidation() As Random
            If pRandomSeed <> Integer.MinValue Then
                pRandomSeedUsed = pRandomSeed
                Return New Random(pRandomSeed)
            End If
            Dim seed As Integer = Environment.TickCount Xor Guid.NewGuid().GetHashCode()
            pRandomSeedUsed = seed
            Return New Random(seed)
        End Function

        Private Function CountByLabel(labels() As String) As Dictionary(Of String, Integer)
            Dim out As New Dictionary(Of String, Integer)(StringComparer.Ordinal)
            For Each lbl In labels
                If Not out.ContainsKey(lbl) Then out(lbl) = 0
                out(lbl) += 1
            Next
            Return out
        End Function

        Private Function RegularizeSymmetricMatrix(mat(,) As Double,
                                                   baseRidge As Double) As RegularizedMatrixInfo
            Dim p As Integer = mat.GetUpperBound(0) + 1
            Dim ridge As Double = Math.Max(baseRidge, 0.0)
            Dim trace As Double = 0.0
            For i As Integer = 0 To p - 1
                trace += Math.Abs(mat(i, i))
            Next
            Dim unitRidge As Double = Math.Max(0.000000000001, If(trace > 0.0, trace / p * 0.000000000001, 0.000000000001))
            If ridge = 0.0 Then ridge = unitRidge

            For attempt As Integer = 0 To 10
                Dim candidate As Double(,)
                If attempt = 0 AndAlso baseRidge = 0.0 Then
                    candidate = CType(mat.Clone(), Double(,))
                Else
                    candidate = AddDiagonalRidge(mat, ridge)
                End If
                candidate = Symmetrize(candidate)
                Dim eig = Matrix.EIGEN_JK(candidate)
                Dim sorted = MultivariateShared.SortEigenpairsDescending(eig.Item1, eig.Item2)
                Dim minEigen As Double = sorted.Item1.Min()
                If minEigen > 0.000000000001 Then
                    Dim inv = MultivariateShared.SafeInverse(candidate, preferCholesky:=True)
                    Dim logdet As Double = 0.0
                    For Each ev In sorted.Item1
                        logdet += Math.Log(ev)
                    Next
                    Dim info As New RegularizedMatrixInfo
                    info.Matrix = candidate
                    info.Inverse = inv
                    info.LogDeterminant = logdet
                    info.RidgeUsed = If(attempt = 0 AndAlso baseRidge = 0.0, 0.0, ridge)
                    Return info
                End If
                ridge = If(ridge <= 0.0, unitRidge, ridge * 10.0)
            Next

            Dim fallback = AddDiagonalRidge(mat, Math.Max(ridge, unitRidge))
            fallback = Symmetrize(fallback)
            Dim out As New RegularizedMatrixInfo
            out.Matrix = fallback
            out.Inverse = MultivariateShared.SafeInverse(fallback, preferCholesky:=False)
            Dim eig2 = Matrix.EIGEN_JK(fallback)
            Dim sorted2 = MultivariateShared.SortEigenpairsDescending(eig2.Item1, eig2.Item2)
            Dim logdet2 As Double = 0.0
            For Each ev In sorted2.Item1
                logdet2 += Math.Log(Math.Max(ev, 0.000000000001))
            Next
            out.LogDeterminant = logdet2
            out.RidgeUsed = Math.Max(ridge, unitRidge)
            Return out
        End Function

        Private Function NormalizeGroupLabel(value As Object) As String
            If value Is Nothing Then Return String.Empty
            Dim s As String = Convert.ToString(value, CultureInfo.InvariantCulture)
            If s Is Nothing Then Return String.Empty
            Return s.Trim()
        End Function

        Private Sub ComputeStandardizationParameters(data(,) As Double, mode As ClusterStandardizationMode,
                                                     ByRef locations() As Double, ByRef scales() As Double)
            Dim p As Integer = data.GetUpperBound(1) + 1
            For j As Integer = 0 To p - 1
                Dim col = Matrix.GetColumnFrom2Darray(data, j)
                Select Case mode
                    Case ClusterStandardizationMode.None
                        locations(j) = 0.0
                        scales(j) = 1.0
                    Case ClusterStandardizationMode.ZScores
                        locations(j) = col.Average()
                        scales(j) = stDev(col)
                        If scales(j) <= 0.0 OrElse Double.IsNaN(scales(j)) OrElse Double.IsInfinity(scales(j)) Then
                            CoreServices.Errors.LogAndThrow(New ArgumentException($"Variable '{j + 1}' cannot be standardized because its sample standard deviation is zero or invalid."))
                        End If
                    Case ClusterStandardizationMode.RangeZeroToOne
                        locations(j) = col.Min()
                        scales(j) = col.Max() - col.Min()
                        If scales(j) <= 0.0 OrElse Double.IsNaN(scales(j)) OrElse Double.IsInfinity(scales(j)) Then
                            CoreServices.Errors.LogAndThrow(New ArgumentException($"Variable '{j + 1}' cannot be range-standardized because its observed range is zero or invalid."))
                        End If
                    Case Else
                        CoreServices.Errors.LogAndThrow(New ArgumentException("Unsupported standardization mode."))
                End Select
            Next
        End Sub

        Private Function TransformValue(x As Double, mode As ClusterStandardizationMode, location As Double, scale As Double) As Double
            Select Case mode
                Case ClusterStandardizationMode.None
                    Return x
                Case Else
                    Return (x - location) / scale
            End Select
        End Function

        Private Function ColumnMeans(data(,) As Double) As Double()
            Dim n As Integer = data.GetUpperBound(0) + 1
            Dim p As Integer = data.GetUpperBound(1) + 1
            Dim out(p - 1) As Double
            For j As Integer = 0 To p - 1
                Dim s As Double = 0.0
                For i As Integer = 0 To n - 1
                    s += data(i, j)
                Next
                out(j) = s / n
            Next
            Return out
        End Function

        Private Function ExtractRows(data(,) As Double, rows() As Integer) As Double(,)
            Dim p As Integer = data.GetUpperBound(1) + 1
            Dim out(rows.Length - 1, p - 1) As Double
            For i As Integer = 0 To rows.Length - 1
                For j As Integer = 0 To p - 1
                    out(i, j) = data(rows(i), j)
                Next
            Next
            Return out
        End Function

        Private Function ExtractRows(values() As String, rows() As Integer) As String()
            Dim out(rows.Length - 1) As String
            For i As Integer = 0 To rows.Length - 1
                out(i) = values(rows(i))
            Next
            Return out
        End Function

        Private Function ExtractRows(values() As Integer, rows() As Integer) As Integer()
            Dim out(rows.Length - 1) As Integer
            For i As Integer = 0 To rows.Length - 1
                out(i) = values(rows(i))
            Next
            Return out
        End Function

        Private Function ExtractObjects(values() As String, rows() As Integer) As Object()
            Dim out(rows.Length - 1) As Object
            For i As Integer = 0 To rows.Length - 1
                out(i) = values(rows(i))
            Next
            Return out
        End Function

        Private Function ConvertStringsToObjects(values() As String) As Object()
            Dim out(values.Length - 1) As Object
            For i As Integer = 0 To values.Length - 1
                out(i) = values(i)
            Next
            Return out
        End Function

        Private Function RowsForGroup(labels() As String, groupLabel As String) As List(Of Integer)
            Dim out As New List(Of Integer)
            For i As Integer = 0 To labels.Length - 1
                If String.Equals(labels(i), groupLabel, StringComparison.Ordinal) Then out.Add(i)
            Next
            Return out
        End Function

        Private Function Softmax(values() As Double) As Double()
            Dim maxv As Double = values.Max()
            Dim out(values.Length - 1) As Double
            Dim s As Double = 0.0
            For i As Integer = 0 To values.Length - 1
                out(i) = Math.Exp(values(i) - maxv)
                s += out(i)
            Next
            If s <= 0.0 Then
                Dim uniform As Double = 1.0 / values.Length
                For i As Integer = 0 To values.Length - 1
                    out(i) = uniform
                Next
                Return out
            End If
            For i As Integer = 0 To values.Length - 1
                out(i) /= s
            Next
            Return out
        End Function

        Private Function ArgMax(values() As Double) As Integer
            Dim idx As Integer = 0
            Dim best As Double = values(0)
            For i As Integer = 1 To values.Length - 1
                If values(i) > best Then
                    best = values(i)
                    idx = i
                End If
            Next
            Return idx
        End Function

        Friend Shared Function MatVec(mat(,) As Double, v() As Double) As Double()
            Dim n As Integer = mat.GetUpperBound(0) + 1
            Dim p As Integer = mat.GetUpperBound(1) + 1
            Dim out(n - 1) As Double
            For i As Integer = 0 To n - 1
                Dim s As Double = 0.0
                For j As Integer = 0 To p - 1
                    s += mat(i, j) * v(j)
                Next
                out(i) = s
            Next
            Return out
        End Function

        Friend Shared Function QuadraticForm(v() As Double, mat(,) As Double) As Double
            Dim tmp() As Double = MatVec(mat, v)
            Return Matrix.DotProduct(v, tmp)
        End Function

        Private Function Symmetrize(mat(,) As Double) As Double(,)
            Dim p As Integer = mat.GetUpperBound(0) + 1
            Dim out(p - 1, p - 1) As Double
            For i As Integer = 0 To p - 1
                For j As Integer = 0 To p - 1
                    out(i, j) = 0.5 * (mat(i, j) + mat(j, i))
                Next
            Next
            Return out
        End Function

        Private Function AddDiagonalRidge(mat(,) As Double, ridge As Double) As Double(,)
            Dim p As Integer = mat.GetUpperBound(0) + 1
            Dim out(p - 1, p - 1) As Double
            For i As Integer = 0 To p - 1
                For j As Integer = 0 To p - 1
                    out(i, j) = mat(i, j)
                Next
                out(i, i) += ridge
            Next
            Return out
        End Function

        Private Sub AddScaledMatrixInPlace(ByRef target(,) As Double, addend(,) As Double, scale As Double)
            Dim n As Integer = target.GetUpperBound(0) + 1
            Dim p As Integer = target.GetUpperBound(1) + 1
            For i As Integer = 0 To n - 1
                For j As Integer = 0 To p - 1
                    target(i, j) += scale * addend(i, j)
                Next
            Next
        End Sub

        Private Sub ShuffleInPlace(Of T)(items As IList(Of T), rng As Random)
            For i As Integer = items.Count - 1 To 1 Step -1
                Dim j As Integer = rng.Next(i + 1)
                Dim tmp As T = items(i)
                items(i) = items(j)
                items(j) = tmp
            Next
        End Sub

        Private Class RegularizedMatrixInfo
            Public Property Matrix As Double(,)
            Public Property Inverse As Double(,)
            Public Property LogDeterminant As Double
            Public Property RidgeUsed As Double
        End Class

    End Class

End Namespace
