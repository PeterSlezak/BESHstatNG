Option Explicit On
Option Strict On

Imports System
Imports System.Globalization
Imports System.Linq
Imports BESHStatNG

Namespace ExcelCommon
    Public NotInheritable Class UdfMultivariateReportPreparation38
        Private Sub New()
        End Sub

        Public Shared Function BuildPcaSummary(model As Multivariate.PCA, rowIds() As Integer, variableNames() As String, matrixLabel As String, retentionLabel As String, retentionValue As Double, hdr As Boolean) As Object(,)
            Dim out(If(hdr, 6, 5), 1) As Object
            Dim r0 As Integer = 0
            If hdr Then
                out(0, 0) = "Setting"
                out(0, 1) = "Value"
                r0 = 1
            End If

            out(r0 + 0, 0) = "Matrix analyzed"
            out(r0 + 0, 1) = matrixLabel
            out(r0 + 1, 0) = "Rows analyzed"
            out(r0 + 1, 1) = rowIds.Length
            out(r0 + 2, 0) = "Variables"
            out(r0 + 2, 1) = variableNames.Length
            out(r0 + 3, 0) = "Retained components"
            out(r0 + 3, 1) = model.NoExtractComponents
            out(r0 + 4, 0) = "Retention rule"
            out(r0 + 4, 1) = retentionLabel
            out(r0 + 5, 0) = "Retention value"
            out(r0 + 5, 1) = retentionValue
            Return out
        End Function

        Public Shared Function BuildPcaEigen(eigen() As Double, pct() As Double, cum() As Double, model As Multivariate.PCA, hdr As Boolean) As Object(,)
            Dim out(eigen.Length - 1 + If(hdr, 1, 0), 4) As Object
            Dim r0 As Integer = 0
            If hdr Then
                out(0, 0) = "Component"
                out(0, 1) = "Eigenvalue"
                out(0, 2) = "% Variance Explained"
                out(0, 3) = "Cumulative % Explained"
                out(0, 4) = "Retained"
                r0 = 1
            End If

            For i As Integer = 0 To eigen.Length - 1
                out(r0 + i, 0) = "PC" & (i + 1).ToString(CultureInfo.InvariantCulture)
                out(r0 + i, 1) = eigen(i)
                out(r0 + i, 2) = pct(i)
                out(r0 + i, 3) = cum(i)
                out(r0 + i, 4) = (i < model.NoExtractComponents)
            Next
            Return out
        End Function

        Public Shared Function BuildFactorSummary(model As Multivariate.FactorAnalysis, variableNames() As String, matrixLabel As String, extractionLabel As String, rotationLabel As String, scoreLabel As String, hdr As Boolean) As Object(,)
            Dim dataRowCount As Integer = 13

            ' VB array bounds are inclusive, so the upper bound must be:
            '   header row + data rows - 1
            Dim rowUpperBound As Integer = If(hdr, dataRowCount, dataRowCount - 1)

            Dim out(rowUpperBound, 1) As Object
            Dim r0 As Integer = 0

            If hdr Then
                out(0, 0) = "Setting"
                out(0, 1) = "Value"
                r0 = 1
            End If

            out(r0 + 0, 0) = "Matrix analyzed"
            out(r0 + 0, 1) = matrixLabel
            out(r0 + 1, 0) = "Extraction method"
            out(r0 + 1, 1) = extractionLabel
            out(r0 + 2, 0) = "Rotation method"
            out(r0 + 2, 1) = rotationLabel
            out(r0 + 3, 0) = "Score method"
            out(r0 + 3, 1) = scoreLabel
            out(r0 + 4, 0) = "Rows analyzed"
            out(r0 + 4, 1) = model.AnalysisData.GetLength(0)
            out(r0 + 5, 0) = "Rows removed"
            out(r0 + 5, 1) = model.RemovedRowCount
            out(r0 + 6, 0) = "Variables"
            out(r0 + 6, 1) = variableNames.Length
            out(r0 + 7, 0) = "Retained factors"
            out(r0 + 7, 1) = model.NumberOfFactors
            out(r0 + 8, 0) = "Extraction converged"
            out(r0 + 8, 1) = model.ExtractionConverged
            out(r0 + 9, 0) = "Rotation converged"
            out(r0 + 9, 1) = model.RotationConverged
            out(r0 + 10, 0) = "Extraction iterations"
            out(r0 + 10, 1) = model.ExtractionIterations
            out(r0 + 11, 0) = "Rotation iterations"
            out(r0 + 11, 1) = model.RotationIterations
            out(r0 + 12, 0) = "RMSR"
            out(r0 + 12, 1) = model.RMSR
            Return out
        End Function

        Public Shared Function BuildFactorEigen(model As Multivariate.FactorAnalysis, ByRef includeHeader As Object, matrixTrace As Func(Of Double(,), Double), readHeader As Func(Of Object, Boolean, Boolean)) As Object(,)
            Dim initialEigen() As Double = model.InitialEigenvalues
            Dim unrotatedSs() As Double = Global.BESHStatNG.WorksheetCompatibility.UdfNumericPreparation38.ColumnSumsOfSquares(model.UnrotatedLoadings, model.UnrotatedLoadings)
            Dim rotatedSs() As Double = Global.BESHStatNG.WorksheetCompatibility.UdfNumericPreparation38.ColumnSumsOfSquares(model.PatternMatrix, model.StructureMatrix)
            Dim totalVar As Double = matrixTrace(model.WorkingMatrix)
            Dim initPct() As Double = Global.BESHStatNG.WorksheetCompatibility.UdfNumericPreparation38.PercentOfTotal(initialEigen, totalVar)
            Dim initCum() As Double = Global.BESHStatNG.WorksheetCompatibility.UdfNumericPreparation38.Cumulative(initPct)
            Dim extrPct() As Double = Global.BESHStatNG.WorksheetCompatibility.UdfNumericPreparation38.PercentOfTotal(unrotatedSs, totalVar)
            Dim extrCum() As Double = Global.BESHStatNG.WorksheetCompatibility.UdfNumericPreparation38.Cumulative(extrPct)
            Dim rotPct() As Double = Global.BESHStatNG.WorksheetCompatibility.UdfNumericPreparation38.PercentOfTotal(rotatedSs, totalVar)
            Dim rotCum() As Double = Global.BESHStatNG.WorksheetCompatibility.UdfNumericPreparation38.Cumulative(rotPct)

            Dim rows As Integer = Math.Max(initialEigen.Length, model.NumberOfFactors)
            Dim hdr As Boolean = readHeader(includeHeader, True)
            Dim out(rows - 1 + If(hdr, 1, 0), 9) As Object
            Dim r0 As Integer = 0
            If hdr Then
                out(0, 0) = "Factor"
                out(0, 1) = "Initial Eigenvalue"
                out(0, 2) = "Initial %"
                out(0, 3) = "Initial Cumulative %"
                out(0, 4) = "Extraction SS Loadings"
                out(0, 5) = "Extraction %"
                out(0, 6) = "Extraction Cumulative %"
                out(0, 7) = "Rotation SS Loadings"
                out(0, 8) = "Rotation %"
                out(0, 9) = "Rotation Cumulative %"
                r0 = 1
            End If

            For i As Integer = 0 To rows - 1
                out(r0 + i, 0) = i + 1
                If i < initialEigen.Length Then
                    out(r0 + i, 1) = initialEigen(i)
                    out(r0 + i, 2) = initPct(i)
                    out(r0 + i, 3) = initCum(i)
                End If
                If i < model.NumberOfFactors Then
                    out(r0 + i, 4) = unrotatedSs(i)
                    out(r0 + i, 5) = extrPct(i)
                    out(r0 + i, 6) = extrCum(i)
                    out(r0 + i, 7) = rotatedSs(i)
                    out(r0 + i, 8) = rotPct(i)
                    out(r0 + i, 9) = rotCum(i)
                End If
            Next
            Return out
        End Function

        Public Shared Function BuildFactorCommunalities(model As Multivariate.FactorAnalysis, variableNames() As String, factorNames() As String, contrib(,) As Double, p As Integer, k As Integer, hdr As Boolean) As Object(,)
            Dim out(p - 1 + If(hdr, 1, 0), k + 3) As Object
            Dim r0 As Integer = 0
            If hdr Then
                out(0, 0) = "Variable"
                out(0, 1) = "Initial"
                For j As Integer = 0 To k - 1
                    out(0, j + 2) = factorNames(j) & " Contribution"
                Next
                out(0, k + 2) = "Extracted"
                out(0, k + 3) = "Uniqueness"
                r0 = 1
            End If

            For i As Integer = 0 To p - 1
                out(r0 + i, 0) = variableNames(i)
                out(r0 + i, 1) = model.InitialCommunalities(i)
                For j As Integer = 0 To k - 1
                    out(r0 + i, j + 2) = contrib(i, j)
                Next
                out(r0 + i, k + 2) = model.Communalities(i)
                out(r0 + i, k + 3) = model.Uniquenesses(i)
            Next
            Return out
        End Function

        Public Shared Function BuildFactorability(model As Multivariate.FactorAnalysis, hdr As Boolean) As Object(,)
            Dim out(If(hdr, 5, 4), 1) As Object
            Dim r0 As Integer = 0
            If hdr Then
                out(0, 0) = "Statistic"
                out(0, 1) = "Value"
                r0 = 1
            End If

            out(r0 + 0, 0) = "Determinant of correlation matrix"
            out(r0 + 0, 1) = model.DeterminantCorrelation
            out(r0 + 1, 0) = "Overall KMO"
            out(r0 + 1, 1) = model.KmoOverall
            out(r0 + 2, 0) = "Bartlett Chi-square"
            out(r0 + 2, 1) = model.BartlettChiSquare
            out(r0 + 3, 0) = "Bartlett df"
            out(r0 + 3, 1) = model.BartlettDegreesOfFreedom
            out(r0 + 4, 0) = "Bartlett p-value"
            out(r0 + 4, 1) = model.BartlettPValue
            out(r0 + 5, 0) = "RMSR"
            out(r0 + 5, 1) = model.RMSR
            Return out
        End Function

        Public Shared Function BuildKMeansSummary(model As Multivariate.KMeans, variableNames() As String, initializationMethod As String, distanceMetric As String, standardization As String, missingValuePolicy As String, emptyClusterHandling As String, maxIterations As Integer, tolerance As Double, hdr As Boolean) As Object(,)
            Dim out(If(hdr, 19, 18), 1) As Object
            Dim r0 As Integer = 0
            If hdr Then
                out(0, 0) = "Setting"
                out(0, 1) = "Value"
                r0 = 1
            End If

            out(r0 + 0, 0) = "Number of clusters"
            out(r0 + 0, 1) = model.Result.NumberOfClusters
            out(r0 + 1, 0) = "Rows analyzed"
            out(r0 + 1, 1) = If(model.Result.ClusterAssignments Is Nothing, 0, model.Result.ClusterAssignments.Length)
            out(r0 + 2, 0) = "Rows removed"
            out(r0 + 2, 1) = If(model.Result.RemovedRowIndices Is Nothing, 0, model.Result.RemovedRowIndices.Length)
            out(r0 + 3, 0) = "Variables"
            out(r0 + 3, 1) = variableNames.Length
            out(r0 + 4, 0) = "Initialization"
            out(r0 + 4, 1) = initializationMethod
            out(r0 + 5, 0) = "Distance metric"
            out(r0 + 5, 1) = distanceMetric
            out(r0 + 6, 0) = "Standardization"
            out(r0 + 6, 1) = standardization
            out(r0 + 7, 0) = "Missing-value policy"
            out(r0 + 7, 1) = missingValuePolicy
            out(r0 + 8, 0) = "Empty-cluster handling"
            out(r0 + 8, 1) = emptyClusterHandling
            out(r0 + 9, 0) = "Random starts evaluated"
            out(r0 + 9, 1) = model.Result.StartsEvaluated
            out(r0 + 10, 0) = "Max iterations per start"
            out(r0 + 10, 1) = maxIterations
            out(r0 + 11, 0) = "Tolerance"
            out(r0 + 11, 1) = tolerance
            out(r0 + 12, 0) = "Converged"
            out(r0 + 12, 1) = model.Result.Converged
            out(r0 + 13, 0) = "Iterations used by best solution"
            out(r0 + 13, 1) = model.Result.Iterations
            out(r0 + 14, 0) = "Total within-cluster sum of squares"
            out(r0 + 14, 1) = model.Result.TotalWithinClusterSS
            out(r0 + 15, 0) = "Between-cluster sum of squares"
            out(r0 + 15, 1) = model.Result.BetweenClusterSS
            out(r0 + 16, 0) = "Total sum of squares"
            out(r0 + 16, 1) = model.Result.TotalSS
            out(r0 + 17, 0) = "Objective value"
            out(r0 + 17, 1) = model.Result.ObjectiveValue
            out(r0 + 18, 0) = "Random seed used"
            out(r0 + 18, 1) = model.Result.RandomSeedUsed
            Return out
        End Function

        Public Shared Function BuildHierarchicalSummary(model As Multivariate.HierarchicalClustering, variableNames() As String, linkage As String, distanceMetric As String, minkowskiPower As Double, standardization As String, missingValuePolicy As String, mergeSteps As Integer, ByRef finalHeight As Object, hdr As Boolean) As Object(,)
            Dim out(If(hdr, 11, 10), 1) As Object
            Dim r0 As Integer = 0
            If hdr Then
                out(0, 0) = "Setting"
                out(0, 1) = "Value"
                r0 = 1
            End If

            out(r0 + 0, 0) = "Linkage"
            out(r0 + 0, 1) = linkage
            out(r0 + 1, 0) = "Distance metric"
            out(r0 + 1, 1) = distanceMetric
            out(r0 + 2, 0) = "Minkowski power"
            out(r0 + 2, 1) = minkowskiPower
            out(r0 + 3, 0) = "Standardization"
            out(r0 + 3, 1) = standardization
            out(r0 + 4, 0) = "Missing-value policy"
            out(r0 + 4, 1) = missingValuePolicy
            out(r0 + 5, 0) = "Rows analyzed"
            out(r0 + 5, 1) = If(model.Result.ActiveRowIndices Is Nothing, 0, model.Result.ActiveRowIndices.Length)
            out(r0 + 6, 0) = "Rows removed"
            out(r0 + 6, 1) = If(model.Result.RemovedRowIndices Is Nothing, 0, model.Result.RemovedRowIndices.Length)
            out(r0 + 7, 0) = "Variables"
            out(r0 + 7, 1) = variableNames.Length
            out(r0 + 8, 0) = "Merge steps"
            out(r0 + 8, 1) = mergeSteps
            out(r0 + 9, 0) = "Leaf count"
            out(r0 + 9, 1) = If(model.Result.ActiveRowIndices Is Nothing, 0, model.Result.ActiveRowIndices.Length)
            out(r0 + 10, 0) = "Final merge height"
            out(r0 + 10, 1) = finalHeight
            Return out
        End Function

        Public Shared Function BuildCaSummary(model As Multivariate.CA, eigen() As Double, hdr As Boolean) As Object(,)
            Dim out(If(hdr, 6, 5), 1) As Object
            Dim r0 As Integer = 0
            If hdr Then
                out(0, 0) = "Setting"
                out(0, 1) = "Value"
                r0 = 1
            End If

            out(r0 + 0, 0) = "Analysis type"
            out(r0 + 0, 1) = "Simple correspondence analysis"
            out(r0 + 1, 0) = "Row categories"
            out(r0 + 1, 1) = model.rowNames.Length
            out(r0 + 2, 0) = "Column categories"
            out(r0 + 2, 1) = model.ColumNames.Length
            out(r0 + 3, 0) = "Available axes"
            out(r0 + 3, 1) = If(eigen Is Nothing, 0, eigen.Length)
            out(r0 + 4, 0) = "Total inertia"
            out(r0 + 4, 1) = If(eigen Is Nothing, CType(Nothing, Object), CType(eigen.Sum(), Object))
            out(r0 + 5, 0) = "Interpretation"
            out(r0 + 5, 1) = "Rows and columns are analyzed jointly through chi-square distances."
            Return out
        End Function
        Public Shared Function BuildMcaSummary(model As Multivariate.CA, observationCount As Integer, variableNames() As String, eigen() As Double, hdr As Boolean) As Object(,)
            Dim out(If(hdr, 6, 5), 1) As Object
            Dim r0 As Integer = 0
            If hdr Then
                out(0, 0) = "Setting"
                out(0, 1) = "Value"
                r0 = 1
            End If

            out(r0 + 0, 0) = "Analysis type"
            out(r0 + 0, 1) = "Multiple correspondence analysis"
            out(r0 + 1, 0) = "Observations"
            out(r0 + 1, 1) = observationCount
            out(r0 + 2, 0) = "Variables"
            out(r0 + 2, 1) = variableNames.Length
            out(r0 + 3, 0) = "Category levels"
            out(r0 + 3, 1) = model.ColumNames.Length
            out(r0 + 4, 0) = "Available axes"
            out(r0 + 4, 1) = If(eigen Is Nothing, 0, eigen.Length)
            out(r0 + 5, 0) = "Total inertia"
            out(r0 + 5, 1) = If(eigen Is Nothing, CType(Nothing, Object), CType(eigen.Sum(), Object))

            Return out
        End Function

        Public Shared Function BuildDiscriminantSummary(model As Multivariate.DiscriminantAnalysis, method As String, groupVariableName As String, variableNames() As String, standardization As String, missingValuePolicy As String, priorMode As String, validationMode As String, covarianceRegularization As Double) As Object(,)
            Dim out(14, 1) As Object
            out(0, 0) = "Setting"
            out(0, 1) = "Value"
            out(1, 0) = "Method"
            out(1, 1) = method
            out(2, 0) = "Grouping variable"
            out(2, 1) = groupVariableName
            out(3, 0) = "Predictors"
            out(3, 1) = variableNames.Length
            out(4, 0) = "Groups"
            out(4, 1) = If(model.GroupLabels Is Nothing, 0, model.GroupLabels.Length)
            out(5, 0) = "Rows analyzed"
            out(5, 1) = If(model.PreparedData Is Nothing OrElse model.PreparedData.ActiveOriginalData Is Nothing, 0, model.PreparedData.ActiveOriginalData.GetLength(0))
            out(6, 0) = "Rows removed"
            out(6, 1) = If(model.PreparedData Is Nothing OrElse model.PreparedData.RemovedOriginalIndices Is Nothing, 0, model.PreparedData.RemovedOriginalIndices.Length)
            out(7, 0) = "Standardization"
            out(7, 1) = standardization
            out(8, 0) = "Missing-value policy"
            out(8, 1) = missingValuePolicy
            out(9, 0) = "Prior mode"
            out(9, 1) = priorMode
            out(10, 0) = "Validation mode"
            out(10, 1) = validationMode
            out(11, 0) = "Training accuracy %"
            out(11, 1) = If(model.TrainingClassification Is Nothing OrElse model.TrainingClassification.Confusion Is Nothing, CType(Nothing, Object), CType(model.TrainingClassification.Confusion.OverallAccuracyPct, Object))
            out(12, 0) = "Validation accuracy %"
            out(12, 1) = If(model.ValidationClassification Is Nothing OrElse model.ValidationClassification.Confusion Is Nothing, CType(Nothing, Object), CType(model.ValidationClassification.Confusion.OverallAccuracyPct, Object))
            out(13, 0) = "Covariance regularization"
            out(13, 1) = covarianceRegularization
            out(14, 0) = "Validation detail"
            Return out
        End Function

    End Class
End Namespace
