Option Explicit On
Option Strict On

Imports System
Imports System.Collections.Concurrent
Imports System.Collections.Generic
Imports System.Globalization
Imports ExcelDna.Integration

Namespace WorksheetFunctions

    ''' <summary>
    ''' Worksheet functions for principal component analysis and exploratory factor analysis.
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' These functions expose multivariate methods that are also available through the graphical interface.
    ''' The workflow follows the same handle-based pattern used by the regression worksheet functions:
    ''' first fit a model and capture the returned handle, then call separate functions to spill the
    ''' specific output table you need.
    ''' </para>
    ''' <para>
    ''' The handle exists only for the current Excel session. If the workbook is reopened, the model
    ''' must be fitted again.
    ''' </para>
    ''' </remarks>
    Public Module MultivariateAnalysisUDFs

        Private ReadOnly _pcaCache As New ConcurrentDictionary(Of String, PcaHandle)(StringComparer.OrdinalIgnoreCase)
        Private ReadOnly _faCache As New ConcurrentDictionary(Of String, FactorAnalysisHandle)(StringComparer.OrdinalIgnoreCase)
        Private ReadOnly _kmeansCache As New ConcurrentDictionary(Of String, KMeansHandle)(StringComparer.OrdinalIgnoreCase)
        Private ReadOnly _hclustCache As New ConcurrentDictionary(Of String, HierarchicalHandle)(StringComparer.OrdinalIgnoreCase)
        Private ReadOnly _caCache As New ConcurrentDictionary(Of String, CorrespondenceHandle)(StringComparer.OrdinalIgnoreCase)
        Private ReadOnly _mcaCache As New ConcurrentDictionary(Of String, MultipleCorrespondenceHandle)(StringComparer.OrdinalIgnoreCase)
        Private ReadOnly _daCache As New ConcurrentDictionary(Of String, DiscriminantHandle)(StringComparer.OrdinalIgnoreCase)

        Private Class PcaHandle
            Public Property Handle As String
            Public Property Model As Multivariate.PCA
            Public Property VariableNames As String()
            Public Property RowIds As Integer()
            Public Property MatrixType As String
            Public Property RetentionMethod As String
            Public Property RetentionValue As Double
        End Class

        Private Class FactorAnalysisHandle
            Public Property Handle As String
            Public Property Model As Multivariate.FactorAnalysis
            Public Property VariableNames As String()
            Public Property MatrixType As String
            Public Property ExtractionMethod As String
            Public Property RotationMethod As String
            Public Property ScoreMethod As String
        End Class

        Private Class KMeansHandle
            Public Property Handle As String
            Public Property Model As Multivariate.KMeans
            Public Property VariableNames As String()
            Public Property RowLabels As String()
            Public Property NumberOfClusters As Integer
            Public Property InitializationMethod As String
            Public Property DistanceMetric As String
            Public Property Standardization As String
            Public Property MissingValuePolicy As String
            Public Property EmptyClusterHandling As String
            Public Property RandomStarts As Integer
            Public Property MaxIterations As Integer
            Public Property Tolerance As Double
            Public Property RequestedRandomSeed As Integer
        End Class

        Private Class HierarchicalHandle
            Public Property Handle As String
            Public Property Model As Multivariate.HierarchicalClustering
            Public Property VariableNames As String()
            Public Property RowLabels As String()
            Public Property Linkage As String
            Public Property DistanceMetric As String
            Public Property MinkowskiPower As Double
            Public Property Standardization As String
            Public Property MissingValuePolicy As String
        End Class

        Private Class CorrespondenceHandle
            Public Property Handle As String
            Public Property Model As Multivariate.CA
            Public Property RowNames As String()
            Public Property ColumnNames As String()
        End Class

        Private Class MultipleCorrespondenceHandle
            Public Property Handle As String
            Public Property Model As Multivariate.CA
            Public Property VariableNames As String()
            Public Property ObservationCount As Integer
        End Class

        Private Class DiscriminantHandle
            Public Property Handle As String
            Public Property Model As Multivariate.DiscriminantAnalysis
            Public Property VariableNames As String()
            Public Property GroupVariableName As String
            Public Property Method As String
            Public Property Standardization As String
            Public Property MissingValuePolicy As String
            Public Property PriorMode As String
            Public Property ValidationMode As String
            Public Property NumberOfFolds As Integer
            Public Property HoldoutFraction As Double
            Public Property Stratified As Boolean
            Public Property RequestedRandomSeed As Integer
            Public Property CovarianceRegularization As Double
        End Class

        ''' <summary>
        ''' Fits a principal component analysis model and returns a reusable handle.
        ''' </summary>
        ''' <param name="x">
        ''' Numeric data matrix with observations in rows and variables in columns.
        ''' A single header row is detected automatically when the first row is nonnumeric and the rows below are numeric.
        ''' Rows containing invalid or missing numeric values are removed before fitting, so the returned PCA handle always
        ''' represents a complete numeric matrix.
        ''' </param>
        ''' <param name="varNames">
        ''' Optional variable names supplied either as a comma-separated list or as a one-row or one-column range.
        ''' When omitted, names are taken from the detected header row when available, otherwise default names X1, X2, … are generated.
        ''' </param>
        ''' <param name="matrixType">
        ''' Optional matrix type. Accepted values include <c>"correlation"</c> and <c>"covariance"</c>.
        ''' Choose correlation when variables are on different scales and you want each variable to contribute on a standardized basis.
        ''' Choose covariance when the original measurement units are directly meaningful and comparable.
        ''' Default: <c>"correlation"</c>.
        ''' </param>
        ''' <param name="retentionMethod">
        ''' Optional component-retention rule. Accepted values include <c>"eigenvalue"</c>, <c>"fixed"</c>, and <c>"variance"</c>.
        ''' The eigenvalue rule retains components whose eigenvalue meets the cutoff. The fixed rule keeps an exact number of components.
        ''' The variance rule keeps the smallest number of components that reaches the requested cumulative percentage.
        ''' Default: <c>"eigenvalue"</c>.
        ''' </param>
        ''' <param name="retentionValue">
        ''' Optional value paired with <paramref name="retentionMethod"/>.
        ''' Use 1.0 for the common Kaiser rule under the eigenvalue method, an integer count for the fixed method,
        ''' or a cumulative percentage such as 80 for the variance method.
        ''' Default: 1.0.
        ''' </param>
        ''' <param name="maxIterations">
        ''' Optional maximum number of iterations for the eigenvalue solver. Larger values may help for numerically difficult matrices.
        ''' Default: 250.
        ''' </param>
        ''' <param name="epsilon">
        ''' Optional numerical convergence tolerance for the eigenvalue solver. Smaller values request stricter convergence.
        ''' Default: 0.000001.
        ''' </param>
        ''' <returns>
        ''' A text handle for the fitted principal component model. Pass the handle to the other <c>PCA_*</c> worksheet functions
        ''' to retrieve only the result table you need, such as the working matrix, eigenvalue table, loading matrix, or scores.
        ''' </returns>
        ''' <remarks>
        ''' <para>
        ''' PCA decomposes either the covariance matrix or the correlation matrix into orthogonal linear combinations of the original
        ''' variables. The retained components are ordered from largest to smallest explained variance.
        ''' </para>
        ''' <para>
        ''' The handle-based design is especially helpful when you want to use the same fitted PCA solution in several worksheet
        ''' locations without recomputing the decomposition each time.
        ''' </para>
        ''' </remarks>
        ''' <example>
        ''' <code>
        ''' =BESH.MULTI.PCA_FIT(A1:H31)
        ''' =BESH.MULTI.PCA_FIT(A1:H31,,"correlation","variance",80)
        ''' =BESH.MULTI.PCA_FIT(A1:H31,,"covariance","fixed",3,500,1E-08)
        ''' </code>
        ''' </example>
        <ExcelFunction(
            Name:="BESH.MULTI.PCA_FIT",
            Category:="BESHStatNG - Multivariate Analysis",
            Description:="Fits a principal component analysis model and returns a reusable handle.",
            HelpTopic:=HelpLinks.FallbackBaseUrl & "/udf/multivariate-analysis/"
        )>
        Public Function PCA_FIT(
            <ExcelArgument(AllowReference:=True, Name:="x", Description:="Numeric data matrix with observations in rows and variables in columns.")> x As Object,
            <ExcelArgument(Name:="varNames", Description:="Optional variable names as a comma-separated list or a one-row/one-column range.")> Optional varNames As Object = Nothing,
            <ExcelArgument(Name:="matrixType", Description:="Optional matrix type: correlation (default) or covariance.")> Optional matrixType As Object = Nothing,
            <ExcelArgument(Name:="retentionMethod", Description:="Optional retention rule: eigenvalue (default), fixed, or variance.")> Optional retentionMethod As Object = Nothing,
            <ExcelArgument(Name:="retentionValue", Description:="Optional value paired with the retention rule. Default 1.0.")> Optional retentionValue As Object = Nothing,
            <ExcelArgument(Name:="maxIterations", Description:="Optional maximum iterations for the eigenvalue solver. Default 250.")> Optional maxIterations As Object = Nothing,
            <ExcelArgument(Name:="epsilon", Description:="Optional convergence tolerance for the eigenvalue solver. Default 0.000001.")> Optional epsilon As Object = Nothing
        ) As Object

            If ExcelDnaUtil.IsInFunctionWizard() Then Return "PCA_FIT (editing...)"

            Try
                Dim imported As DataObj = Nothing
                If Not Global.BESHStatNG.UdfDataImport.TryGetNumericData(x, varNames, False, imported) Then Return ExcelError.ExcelErrorValue
                If imported.nRows < 2 OrElse imported.nCols < 1 Then Return ExcelError.ExcelErrorNum

                Dim matrixLabel As String = ParsePcaMatrixType(matrixType)
                Dim retentionLabel As String = ParsePcaRetentionMethod(retentionMethod)
                Dim keepValue As Double = GetOptionalDouble(retentionValue, 1.0R)
                Dim iter As Integer = GetOptionalInt(maxIterations, 250)
                Dim eps As Double = GetOptionalDouble(epsilon, 0.000001R)

                Dim fit As New Multivariate.PCA()
                fit.dataInputs(imported.DataDbl, imported.RowIds, imported.varNames,
                               String.Format(CultureInfo.InvariantCulture, "{0}={1}", retentionLabel, keepValue))
                fit.settingsInputs(iter, eps, matrixLabel)
                fit.Calculate(retentionLabel, keepValue)

                Dim handleKey As String = "PCA:" & Guid.NewGuid().ToString("N")
                Dim info As New PcaHandle With {
                    .Handle = handleKey,
                    .Model = fit,
                    .VariableNames = CloneStringArray(imported.varNames),
                    .RowIds = DirectCast(imported.RowIds.Clone(), Integer()),
                    .MatrixType = matrixLabel,
                    .RetentionMethod = retentionLabel,
                    .RetentionValue = keepValue
                }
                _pcaCache(handleKey) = info

                Return handleKey
            Catch ex As Exception
                Return LoggedUdfExceptionText("BESH.MULTI.PCA_FIT", ex)
            End Try
        End Function

        ''' <summary>
        ''' Returns a compact settings summary for a fitted principal component analysis model.
        ''' </summary>
        ''' <param name="handle">Handle returned by <c>BESH.MULTI.PCA_FIT</c>.</param>
        ''' <param name="includeHeader">TRUE to include a header row. Default TRUE.</param>
        ''' <returns>A spilled two-column table listing the matrix analyzed, dimensions, retained components, and retention rule.</returns>
        <ExcelFunction(
            Name:="BESH.MULTI.PCA_SUMMARY",
            Category:="BESHStatNG - Multivariate Analysis",
            Description:="Returns a compact settings summary for a fitted principal component analysis model.",
            HelpTopic:=HelpLinks.FallbackBaseUrl & "/udf/multivariate-analysis/"
        )>
        Public Function PCA_SUMMARY(
            <ExcelArgument(Name:="handle", Description:="Handle returned by BESH.MULTI.PCA_FIT.")> handle As Object,
            <ExcelArgument(Name:="includeHeader", Description:="TRUE to include a header row (default TRUE).")> Optional includeHeader As Object = Nothing
        ) As Object
            Try
                Dim h As PcaHandle = Nothing
                If Not TryGetPcaHandle(handle, h) Then Return ExcelError.ExcelErrorNA

                Dim hdr As Boolean = GetOptionalBool(includeHeader, True)
                Dim out(If(hdr, 6, 5), 1) As Object
                Dim r0 As Integer = 0
                If hdr Then
                    out(0, 0) = "Setting"
                    out(0, 1) = "Value"
                    r0 = 1
                End If

                out(r0 + 0, 0) = "Matrix analyzed"
                out(r0 + 0, 1) = h.MatrixType
                out(r0 + 1, 0) = "Rows analyzed"
                out(r0 + 1, 1) = h.RowIds.Length
                out(r0 + 2, 0) = "Variables"
                out(r0 + 2, 1) = h.VariableNames.Length
                out(r0 + 3, 0) = "Retained components"
                out(r0 + 3, 1) = h.Model.NoExtractComponents
                out(r0 + 4, 0) = "Retention rule"
                out(r0 + 4, 1) = h.RetentionMethod
                out(r0 + 5, 0) = "Retention value"
                out(r0 + 5, 1) = h.RetentionValue

                Return PrepareResultTableForUdf(out)
            Catch ex As Exception
                Return LoggedUdfExceptionText("BESH.MULTI.PCA_SUMMARY", ex)
            End Try
        End Function

        ''' <summary>
        ''' Returns the analyzed covariance or correlation matrix for a fitted principal component model.
        ''' </summary>
        ''' <param name="handle">Handle returned by <c>BESH.MULTI.PCA_FIT</c>.</param>
        ''' <param name="includeHeader">TRUE to include a header row with variable names. Default TRUE.</param>
        ''' <returns>
        ''' A labeled square matrix. The first column contains variable names. When <paramref name="includeHeader"/> is TRUE,
        ''' the first row contains the same variable names as column headers.
        ''' </returns>
        <ExcelFunction(
            Name:="BESH.MULTI.PCA_MATRIX",
            Category:="BESHStatNG - Multivariate Analysis",
            Description:="Returns the analyzed covariance or correlation matrix for a fitted principal component model.",
            HelpTopic:=HelpLinks.FallbackBaseUrl & "/udf/multivariate-analysis/"
        )>
        Public Function PCA_MATRIX(
            <ExcelArgument(Name:="handle", Description:="Handle returned by BESH.MULTI.PCA_FIT.")> handle As Object,
            <ExcelArgument(Name:="includeHeader", Description:="TRUE to include a header row (default TRUE).")> Optional includeHeader As Object = Nothing
        ) As Object
            Try
                Dim h As PcaHandle = Nothing
                If Not TryGetPcaHandle(handle, h) Then Return ExcelError.ExcelErrorNA
                Return BuildNamedMatrixOutput("Variable", h.VariableNames, h.VariableNames, h.Model.VarCovarMat, GetOptionalBool(includeHeader, True))
            Catch ex As Exception
                Return LoggedUdfExceptionText("BESH.MULTI.PCA_MATRIX", ex)
            End Try
        End Function

        ''' <summary>
        ''' Returns the eigenvalue table for a fitted principal component model.
        ''' </summary>
        ''' <param name="handle">Handle returned by <c>BESH.MULTI.PCA_FIT</c>.</param>
        ''' <param name="includeHeader">TRUE to include a header row. Default TRUE.</param>
        ''' <returns>
        ''' A spilled table with one row per component showing the eigenvalue, percentage of variance explained,
        ''' cumulative percentage, and whether the component was retained by the requested rule.
        ''' </returns>
        <ExcelFunction(
            Name:="BESH.MULTI.PCA_EIGEN",
            Category:="BESHStatNG - Multivariate Analysis",
            Description:="Returns eigenvalues and explained-variance summaries for a fitted principal component model.",
            HelpTopic:=HelpLinks.FallbackBaseUrl & "/udf/multivariate-analysis/"
        )>
        Public Function PCA_EIGEN(
            <ExcelArgument(Name:="handle", Description:="Handle returned by BESH.MULTI.PCA_FIT.")> handle As Object,
            <ExcelArgument(Name:="includeHeader", Description:="TRUE to include a header row (default TRUE).")> Optional includeHeader As Object = Nothing
        ) As Object
            Try
                Dim h As PcaHandle = Nothing
                If Not TryGetPcaHandle(handle, h) Then Return ExcelError.ExcelErrorNA

                Dim eigen() As Double = h.Model.Eigenvalues
                If eigen Is Nothing Then Return ExcelError.ExcelErrorNA

                Dim pct() As Double = h.Model.PercentExpl
                Dim cum() As Double = h.Model.PercentExplCum
                Dim hdr As Boolean = GetOptionalBool(includeHeader, True)
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
                    out(r0 + i, 4) = (i < h.Model.NoExtractComponents)
                Next

                Return PrepareResultTableForUdf(out)
            Catch ex As Exception
                Return LoggedUdfExceptionText("BESH.MULTI.PCA_EIGEN", ex)
            End Try
        End Function

        ''' <summary>
        ''' Returns the retained principal-component loading matrix.
        ''' </summary>
        ''' <param name="handle">Handle returned by <c>BESH.MULTI.PCA_FIT</c>.</param>
        ''' <param name="includeHeader">TRUE to include a header row. Default TRUE.</param>
        ''' <returns>
        ''' A spilled table with one row per variable and one column per retained component.
        ''' The values are the retained loading directions used to compute component scores.
        ''' </returns>
        <ExcelFunction(
            Name:="BESH.MULTI.PCA_LOADINGS",
            Category:="BESHStatNG - Multivariate Analysis",
            Description:="Returns the retained loading matrix for a fitted principal component model.",
            HelpTopic:=HelpLinks.FallbackBaseUrl & "/udf/multivariate-analysis/"
        )>
        Public Function PCA_LOADINGS(
            <ExcelArgument(Name:="handle", Description:="Handle returned by BESH.MULTI.PCA_FIT.")> handle As Object,
            <ExcelArgument(Name:="includeHeader", Description:="TRUE to include a header row (default TRUE).")> Optional includeHeader As Object = Nothing
        ) As Object
            Try
                Dim h As PcaHandle = Nothing
                If Not TryGetPcaHandle(handle, h) Then Return ExcelError.ExcelErrorNA
                Return BuildNamedMatrixOutput("Variable", h.VariableNames, h.Model.PCnames("PC"), h.Model.GetLoadings, GetOptionalBool(includeHeader, True))
            Catch ex As Exception
                Return LoggedUdfExceptionText("BESH.MULTI.PCA_LOADINGS", ex)
            End Try
        End Function

        ''' <summary>
        ''' Returns principal-component scores for the analyzed observations.
        ''' </summary>
        ''' <param name="handle">Handle returned by <c>BESH.MULTI.PCA_FIT</c>.</param>
        ''' <param name="includeHeader">TRUE to include a header row. Default TRUE.</param>
        ''' <returns>
        ''' A spilled table with one row per analyzed observation and one column per retained component.
        ''' The first column contains row identifiers relative to the supplied input range after rows with invalid values were removed.
        ''' </returns>
        <ExcelFunction(
            Name:="BESH.MULTI.PCA_SCORES",
            Category:="BESHStatNG - Multivariate Analysis",
            Description:="Returns principal-component scores for the analyzed observations.",
            HelpTopic:=HelpLinks.FallbackBaseUrl & "/udf/multivariate-analysis/"
        )>
        Public Function PCA_SCORES(
            <ExcelArgument(Name:="handle", Description:="Handle returned by BESH.MULTI.PCA_FIT.")> handle As Object,
            <ExcelArgument(Name:="includeHeader", Description:="TRUE to include a header row (default TRUE).")> Optional includeHeader As Object = Nothing
        ) As Object
            Try
                Dim h As PcaHandle = Nothing
                If Not TryGetPcaHandle(handle, h) Then Return ExcelError.ExcelErrorNA
                Return BuildCaseMatrixOutput("Row", h.RowIds, h.Model.PCnames("PC"), h.Model.ReducedDataset, GetOptionalBool(includeHeader, True))
            Catch ex As Exception
                Return LoggedUdfExceptionText("BESH.MULTI.PCA_SCORES", ex)
            End Try
        End Function

        ''' <summary>
        ''' Removes a principal component analysis handle from memory.
        ''' </summary>
        ''' <param name="handle">Handle returned by <c>BESH.MULTI.PCA_FIT</c>.</param>
        ''' <returns>TRUE when the handle was removed; FALSE when the handle was not found.</returns>
        <ExcelFunction(
            Name:="BESH.MULTI.PCA_DROP",
            Category:="BESHStatNG - Multivariate Analysis",
            Description:="Removes a principal component analysis handle from memory.",
            HelpTopic:=HelpLinks.FallbackBaseUrl & "/udf/multivariate-analysis/"
        )>
        Public Function PCA_DROP(
            <ExcelArgument(Name:="handle", Description:="Handle returned by BESH.MULTI.PCA_FIT.")> handle As Object
        ) As Object
            Try
                Dim key As String = AsString(handle)
                If String.IsNullOrWhiteSpace(key) Then Return False
                Dim removed As PcaHandle = Nothing
                Return _pcaCache.TryRemove(key.Trim(), removed)
            Catch ex As Exception
                Return LoggedUdfExceptionText("BESH.MULTI.PCA_DROP", ex)
            End Try
        End Function

        ''' <summary>
        ''' Fits an exploratory factor-analysis model and returns a reusable handle.
        ''' </summary>
        ''' <param name="x">
        ''' Numeric data matrix with observations in rows and variables in columns.
        ''' If the first row contains labels, that row is detected automatically and used as variable names.
        ''' Missing or invalid cells are passed through to the factor-analysis engine so that the requested missing-value policy can be applied.
        ''' </param>
        ''' <param name="varNames">
        ''' Optional variable names supplied either as a comma-separated list or as a one-row or one-column range.
        ''' When omitted, names are taken from the detected header row when available, otherwise default names X1, X2, … are generated.
        ''' </param>
        ''' <param name="matrixType">
        ''' Optional working matrix: <c>"correlation"</c> (default) or <c>"covariance"</c>.
        ''' Correlation analysis standardizes variables first and is the usual choice when variables are measured on different scales.
        ''' Covariance analysis preserves the original measurement scale.
        ''' </param>
        ''' <param name="extractionMethod">
        ''' Optional extraction method. Accepted values include <c>"principalaxis"</c>, <c>"principalcomponents"</c>, <c>"ml"</c>,
        ''' <c>"gls"</c>, <c>"image"</c>, and <c>"alpha"</c>. Default: <c>"principalaxis"</c>.
        ''' </param>
        ''' <param name="retentionMethod">
        ''' Optional retention rule: <c>"fixed"</c>, <c>"eigenvalue"</c>, or <c>"variance"</c>. Default: <c>"eigenvalue"</c>.
        ''' </param>
        ''' <param name="retentionValue">
        ''' Optional parameter paired with the retention rule.
        ''' Use an integer count for the fixed rule, an eigenvalue cutoff such as 1.0 for the eigenvalue rule,
        ''' or a target cumulative percentage such as 70 for the variance rule. Default: 1.0.
        ''' </param>
        ''' <param name="rotationMethod">
        ''' Optional post-extraction rotation. Accepted values include <c>"none"</c>, <c>"varimax"</c>, <c>"quartimax"</c>, <c>"equamax"</c>, and <c>"promax"</c>.
        ''' Rotation is used to obtain a loading pattern that is often easier to interpret than the raw extraction output.
        ''' Default: <c>"none"</c>.
        ''' </param>
        ''' <param name="scoreMethod">
        ''' Optional factor-score estimator. Accepted values include <c>"none"</c>, <c>"regression"</c>, and <c>"bartlett"</c>.
        ''' Default: <c>"regression"</c>.
        ''' </param>
        ''' <param name="communalityInitialization">
        ''' Optional starting communality rule used by principal-axis factoring.
        ''' Accepted values include <c>"smc"</c> for squared multiple correlations and <c>"one"</c> for unit communalities.
        ''' Default: <c>"smc"</c>.
        ''' </param>
        ''' <param name="missingValuePolicy">
        ''' Optional missing-data policy. Accepted values include <c>"error"</c> and <c>"listwise"</c>.
        ''' Use <c>"error"</c> when any missing value should stop the analysis. Use <c>"listwise"</c> to delete incomplete rows before fitting.
        ''' Default: <c>"error"</c>.
        ''' </param>
        ''' <param name="useKaiserNormalization">
        ''' Optional TRUE/FALSE flag controlling Kaiser normalization before orthomax-family rotation.
        ''' This is commonly left TRUE for varimax, quartimax, equamax, and promax. Default TRUE.
        ''' </param>
        ''' <param name="promaxPower">
        ''' Optional power used when promax rotation is requested. Larger values typically encourage a simpler, more polarized loading pattern.
        ''' Standard software often uses 4. Default 4.
        ''' </param>
        ''' <param name="maxIterations">
        ''' Optional maximum number of iterations used by extraction and rotation routines. Default 250.
        ''' </param>
        ''' <param name="epsilon">
        ''' Optional convergence tolerance for iterative fitting routines. Default 0.000001.
        ''' </param>
        ''' <returns>
        ''' A text handle for the fitted factor-analysis model. Pass the handle to the other <c>FA_*</c> worksheet functions
        ''' to retrieve the specific output needed for reporting, interpretation, or downstream analysis.
        ''' </returns>
        ''' <remarks>
        ''' <para>
        ''' Unlike PCA, exploratory factor analysis focuses on shared variance and separates common variance from uniqueness.
        ''' Rotation choices influence interpretability, and oblique rotation additionally allows the retained factors to correlate.
        ''' </para>
        ''' <para>
        ''' The returned handle is especially useful when you want to inspect several tables from the same fitted solution,
        ''' such as communalities, the rotated pattern matrix, factor correlations, and factor scores.
        ''' </para>
        ''' </remarks>
        ''' <example>
        ''' <code>
        ''' =BESH.MULTI.FA_FIT(A1:H31)
        ''' =BESH.MULTI.FA_FIT(A1:H31,,"correlation","ml","eigenvalue",1,"varimax")
        ''' =BESH.MULTI.FA_FIT(A1:H31,,"correlation","principalaxis","fixed",3,"promax","regression","smc","listwise",TRUE,4)
        ''' </code>
        ''' </example>
        <ExcelFunction(
            Name:="BESH.MULTI.FA_FIT",
            Category:="BESHStatNG - Multivariate Analysis",
            Description:="Fits an exploratory factor-analysis model and returns a reusable handle.",
            HelpTopic:=HelpLinks.FallbackBaseUrl & "/udf/multivariate-analysis/"
        )>
        Public Function FA_FIT(
            <ExcelArgument(AllowReference:=True, Name:="x", Description:="Numeric data matrix with observations in rows and variables in columns.")> x As Object,
            <ExcelArgument(Name:="varNames", Description:="Optional variable names as a comma-separated list or a one-row/one-column range.")> Optional varNames As Object = Nothing,
            <ExcelArgument(Name:="matrixType", Description:="Optional matrix type: correlation (default) or covariance.")> Optional matrixType As Object = Nothing,
            <ExcelArgument(Name:="extractionMethod", Description:="Optional extraction method. Default principalaxis.")> Optional extractionMethod As Object = Nothing,
            <ExcelArgument(Name:="retentionMethod", Description:="Optional retention rule: eigenvalue (default), fixed, or variance.")> Optional retentionMethod As Object = Nothing,
            <ExcelArgument(Name:="retentionValue", Description:="Optional parameter paired with the retention rule. Default 1.0.")> Optional retentionValue As Object = Nothing,
            <ExcelArgument(Name:="rotationMethod", Description:="Optional rotation method. Default none.")> Optional rotationMethod As Object = Nothing,
            <ExcelArgument(Name:="scoreMethod", Description:="Optional score method: regression (default), bartlett, or none.")> Optional scoreMethod As Object = Nothing,
            <ExcelArgument(Name:="communalityInitialization", Description:="Optional communality initialization: smc (default) or one.")> Optional communalityInitialization As Object = Nothing,
            <ExcelArgument(Name:="missingValuePolicy", Description:="Optional missing-value policy: error (default) or listwise.")> Optional missingValuePolicy As Object = Nothing,
            <ExcelArgument(Name:="useKaiserNormalization", Description:="TRUE to use Kaiser normalization before orthomax-family rotation (default TRUE).")> Optional useKaiserNormalization As Object = Nothing,
            <ExcelArgument(Name:="promaxPower", Description:="Optional promax power. Default 4.")> Optional promaxPower As Object = Nothing,
            <ExcelArgument(Name:="maxIterations", Description:="Optional maximum iterations used by extraction and rotation routines. Default 250.")> Optional maxIterations As Object = Nothing,
            <ExcelArgument(Name:="epsilon", Description:="Optional convergence tolerance for iterative fitting. Default 0.000001.")> Optional epsilon As Object = Nothing
        ) As Object

            If ExcelDnaUtil.IsInFunctionWizard() Then Return "FA_FIT (editing...)"

            Try
                Dim imported As DataObj = Nothing
                If Not Global.BESHStatNG.UdfDataImport.TryGetNumericData(x, varNames, True, imported) Then Return ExcelError.ExcelErrorValue
                If imported.nRows < 2 OrElse imported.nCols < 2 Then Return ExcelError.ExcelErrorNum

                Dim matrixChoice As Multivariate.FactorAnalysisMatrixType = ParseFaMatrixType(matrixType)
                Dim extractionChoice As Multivariate.FactorAnalysisExtractionMethod = ParseFaExtractionMethod(extractionMethod)
                Dim retentionChoice As Multivariate.FactorAnalysisRetentionMethod = ParseFaRetentionMethod(retentionMethod)
                Dim rotationChoice As Multivariate.FactorAnalysisRotationMethod = ParseFaRotationMethod(rotationMethod)
                Dim scoreChoice As Multivariate.FactorAnalysisScoreMethod = ParseFaScoreMethod(scoreMethod)
                Dim communalityChoice As Multivariate.FactorAnalysisCommunalityInitialization = ParseFaCommunalityInitialization(communalityInitialization)
                Dim missingChoice As Multivariate.FactorAnalysisMissingValuePolicy = ParseFaMissingPolicy(missingValuePolicy)

                Dim fit As New Multivariate.FactorAnalysis()
                fit.dataInputs(imported.DataDbl, imported.RowIds, imported.varNames)
                fit.settingsInputs(
                    maximumIteration:=GetOptionalInt(maxIterations, 250),
                    dEps:=GetOptionalDouble(epsilon, 0.000001R),
                    analyzedMatrixType:=matrixChoice,
                    extractionMethod:=extractionChoice,
                    retentionMethod:=retentionChoice,
                    retentionValue:=GetOptionalDouble(retentionValue, 1.0R),
                    rotationMethod:=rotationChoice,
                    scoreMethod:=scoreChoice,
                    communalityInitialization:=communalityChoice,
                    missingValuePolicy:=missingChoice,
                    useKaiserNormalization:=GetOptionalBool(useKaiserNormalization, True),
                    promaxPower:=GetOptionalDouble(promaxPower, 4.0R))
                fit.Calculate()

                Dim handleKey As String = "FA:" & Guid.NewGuid().ToString("N")
                Dim info As New FactorAnalysisHandle With {
                    .Handle = handleKey,
                    .Model = fit,
                    .VariableNames = CloneStringArray(imported.varNames),
                    .MatrixType = matrixChoice.ToString(),
                    .ExtractionMethod = extractionChoice.ToString(),
                    .RotationMethod = rotationChoice.ToString(),
                    .ScoreMethod = scoreChoice.ToString()
                }
                _faCache(handleKey) = info
                Return handleKey
            Catch ex As Exception
                Return LoggedUdfExceptionText("BESH.MULTI.FA_FIT", ex)
            End Try
        End Function

        ''' <summary>
        ''' Returns a compact settings and convergence summary for a fitted factor-analysis model.
        ''' </summary>
        ''' <param name="handle">Handle returned by <c>BESH.MULTI.FA_FIT</c>.</param>
        ''' <param name="includeHeader">TRUE to include a header row. Default TRUE.</param>
        ''' <returns>
        ''' A spilled two-column table describing the working matrix, extraction and rotation choices,
        ''' sample size after missing-data handling, retained factors, convergence flags, and RMSR.
        ''' </returns>
        <ExcelFunction(
                Name:="BESH.MULTI.FA_SUMMARY",
                Category:="BESHStatNG - Multivariate Analysis",
                Description:="Returns a compact settings and convergence summary for a fitted factor-analysis model.",
                HelpTopic:=HelpLinks.FallbackBaseUrl & "/udf/multivariate-analysis/"
            )>
        Public Function FA_SUMMARY(
                <ExcelArgument(Name:="handle", Description:="Handle returned by BESH.MULTI.FA_FIT.")> handle As Object,
                <ExcelArgument(Name:="includeHeader", Description:="TRUE to include a header row (default TRUE).")> Optional includeHeader As Object = Nothing
            ) As Object
            Try
                Dim h As FactorAnalysisHandle = Nothing
                If Not TryGetFaHandle(handle, h) Then Return ExcelError.ExcelErrorNA

                Dim hdr As Boolean = GetOptionalBool(includeHeader, True)
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
                out(r0 + 0, 1) = h.MatrixType
                out(r0 + 1, 0) = "Extraction method"
                out(r0 + 1, 1) = h.ExtractionMethod
                out(r0 + 2, 0) = "Rotation method"
                out(r0 + 2, 1) = h.RotationMethod
                out(r0 + 3, 0) = "Score method"
                out(r0 + 3, 1) = h.ScoreMethod
                out(r0 + 4, 0) = "Rows analyzed"
                out(r0 + 4, 1) = h.Model.AnalysisData.GetLength(0)
                out(r0 + 5, 0) = "Rows removed"
                out(r0 + 5, 1) = h.Model.RemovedRowCount
                out(r0 + 6, 0) = "Variables"
                out(r0 + 6, 1) = h.VariableNames.Length
                out(r0 + 7, 0) = "Retained factors"
                out(r0 + 7, 1) = h.Model.NumberOfFactors
                out(r0 + 8, 0) = "Extraction converged"
                out(r0 + 8, 1) = h.Model.ExtractionConverged
                out(r0 + 9, 0) = "Rotation converged"
                out(r0 + 9, 1) = h.Model.RotationConverged
                out(r0 + 10, 0) = "Extraction iterations"
                out(r0 + 10, 1) = h.Model.ExtractionIterations
                out(r0 + 11, 0) = "Rotation iterations"
                out(r0 + 11, 1) = h.Model.RotationIterations
                out(r0 + 12, 0) = "RMSR"
                out(r0 + 12, 1) = h.Model.RMSR

                Return PrepareResultTableForUdf(out)
            Catch ex As Exception
                Return LoggedUdfExceptionText("BESH.MULTI.FA_SUMMARY", ex)
            End Try
        End Function

        ''' <summary>
        ''' Returns the working covariance or correlation matrix for a fitted factor-analysis model.
        ''' </summary>
        ''' <param name="handle">Handle returned by <c>BESH.MULTI.FA_FIT</c>.</param>
        ''' <param name="includeHeader">TRUE to include a header row with variable names. Default TRUE.</param>
        ''' <returns>A labeled square matrix containing the working analysis matrix.</returns>
        <ExcelFunction(
            Name:="BESH.MULTI.FA_MATRIX",
            Category:="BESHStatNG - Multivariate Analysis",
            Description:="Returns the working covariance or correlation matrix for a fitted factor-analysis model.",
            HelpTopic:=HelpLinks.FallbackBaseUrl & "/udf/multivariate-analysis/"
        )>
        Public Function FA_MATRIX(
            <ExcelArgument(Name:="handle", Description:="Handle returned by BESH.MULTI.FA_FIT.")> handle As Object,
            <ExcelArgument(Name:="includeHeader", Description:="TRUE to include a header row (default TRUE).")> Optional includeHeader As Object = Nothing
        ) As Object
            Try
                Dim h As FactorAnalysisHandle = Nothing
                If Not TryGetFaHandle(handle, h) Then Return ExcelError.ExcelErrorNA
                Return BuildNamedMatrixOutput("Variable", h.VariableNames, h.VariableNames, h.Model.WorkingMatrix, GetOptionalBool(includeHeader, True))
            Catch ex As Exception
                Return LoggedUdfExceptionText("BESH.MULTI.FA_MATRIX", ex)
            End Try
        End Function

        ''' <summary>
        ''' Returns the initial and retained variance table for a fitted factor-analysis model.
        ''' </summary>
        ''' <param name="handle">Handle returned by <c>BESH.MULTI.FA_FIT</c>.</param>
        ''' <param name="includeHeader">TRUE to include a header row. Default TRUE.</param>
        ''' <returns>
        ''' A spilled table showing initial eigenvalues and percentages for all variables, plus the extraction and rotation sums of squares for the retained factors.
        ''' This table is useful for deciding how many factors to keep and for reporting how much common variance is represented by the final solution.
        ''' </returns>
        <ExcelFunction(
            Name:="BESH.MULTI.FA_EIGEN",
            Category:="BESHStatNG - Multivariate Analysis",
            Description:="Returns the initial and retained variance table for a fitted factor-analysis model.",
            HelpTopic:=HelpLinks.FallbackBaseUrl & "/udf/multivariate-analysis/"
        )>
        Public Function FA_EIGEN(
            <ExcelArgument(Name:="handle", Description:="Handle returned by BESH.MULTI.FA_FIT.")> handle As Object,
            <ExcelArgument(Name:="includeHeader", Description:="TRUE to include a header row (default TRUE).")> Optional includeHeader As Object = Nothing
        ) As Object
            Try
                Dim h As FactorAnalysisHandle = Nothing
                If Not TryGetFaHandle(handle, h) Then Return ExcelError.ExcelErrorNA

                Dim initialEigen() As Double = h.Model.InitialEigenvalues
                Dim unrotatedSs() As Double = ColumnSumsOfSquares(h.Model.UnrotatedLoadings, h.Model.UnrotatedLoadings)
                Dim rotatedSs() As Double = ColumnSumsOfSquares(h.Model.PatternMatrix, h.Model.StructureMatrix)
                Dim totalVar As Double = Matrix.MatrixTrace(h.Model.WorkingMatrix)
                Dim initPct() As Double = PercentOfTotal(initialEigen, totalVar)
                Dim initCum() As Double = Cumulative(initPct)
                Dim extrPct() As Double = PercentOfTotal(unrotatedSs, totalVar)
                Dim extrCum() As Double = Cumulative(extrPct)
                Dim rotPct() As Double = PercentOfTotal(rotatedSs, totalVar)
                Dim rotCum() As Double = Cumulative(rotPct)

                Dim rows As Integer = Math.Max(initialEigen.Length, h.Model.NumberOfFactors)
                Dim hdr As Boolean = GetOptionalBool(includeHeader, True)
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
                    If i < h.Model.NumberOfFactors Then
                        out(r0 + i, 4) = unrotatedSs(i)
                        out(r0 + i, 5) = extrPct(i)
                        out(r0 + i, 6) = extrCum(i)
                        out(r0 + i, 7) = rotatedSs(i)
                        out(r0 + i, 8) = rotPct(i)
                        out(r0 + i, 9) = rotCum(i)
                    End If
                Next

                Return PrepareResultTableForUdf(out)
            Catch ex As Exception
                Return LoggedUdfExceptionText("BESH.MULTI.FA_EIGEN", ex)
            End Try
        End Function

        ''' <summary>
        ''' Returns the rotated loading matrix for a fitted factor-analysis model.
        ''' </summary>
        ''' <param name="handle">Handle returned by <c>BESH.MULTI.FA_FIT</c>.</param>
        ''' <param name="includeHeader">TRUE to include a header row. Default TRUE.</param>
        ''' <returns>
        ''' A spilled table with one row per variable and one column per retained factor.
        ''' For orthogonal rotations this is the usual rotated loading matrix. For oblique rotation it is the pattern matrix.
        ''' </returns>
        <ExcelFunction(
            Name:="BESH.MULTI.FA_LOADINGS",
            Category:="BESHStatNG - Multivariate Analysis",
            Description:="Returns the rotated loading matrix for a fitted factor-analysis model.",
            HelpTopic:=HelpLinks.FallbackBaseUrl & "/udf/multivariate-analysis/"
        )>
        Public Function FA_LOADINGS(
            <ExcelArgument(Name:="handle", Description:="Handle returned by BESH.MULTI.FA_FIT.")> handle As Object,
            <ExcelArgument(Name:="includeHeader", Description:="TRUE to include a header row (default TRUE).")> Optional includeHeader As Object = Nothing
        ) As Object
            Try
                Dim h As FactorAnalysisHandle = Nothing
                If Not TryGetFaHandle(handle, h) Then Return ExcelError.ExcelErrorNA
                Return BuildNamedMatrixOutput("Variable", h.VariableNames, h.Model.FactorNames("Factor "), h.Model.PatternMatrix, GetOptionalBool(includeHeader, True))
            Catch ex As Exception
                Return LoggedUdfExceptionText("BESH.MULTI.FA_LOADINGS", ex)
            End Try
        End Function

        ''' <summary>
        ''' Returns the strctre matrix for a fitted factor-analysis model.
        ''' </summary>
        ''' <param name="handle">Handle returned by <c>BESH.MULTI.FA_FIT</c>.</param>
        ''' <param name="includeHeader">TRUE to include a header row. Default TRUE.</param>
        ''' <returns>
        ''' A spilled table with one row per variable and one column per retained factor.
        ''' Under orthogonal rotation this matrix equals the loading matrix. Under oblique rotation it contains variable–factor correlations.
        ''' </returns>
        <ExcelFunction(
            Name:="BESH.MULTI.FA_STRUCTURE",
            Category:="BESHStatNG - Multivariate Analysis",
            Description:="Returns the strctre matrix for a fitted factor-analysis model.",
            HelpTopic:=HelpLinks.FallbackBaseUrl & "/udf/multivariate-analysis/"
        )>
        Public Function FA_STRUCTURE(
            <ExcelArgument(Name:="handle", Description:="Handle returned by BESH.MULTI.FA_FIT.")> handle As Object,
            <ExcelArgument(Name:="includeHeader", Description:="TRUE to include a header row (default TRUE).")> Optional includeHeader As Object = Nothing
        ) As Object
            Try
                Dim h As FactorAnalysisHandle = Nothing
                If Not TryGetFaHandle(handle, h) Then Return ExcelError.ExcelErrorNA
                Return BuildNamedMatrixOutput("Variable", h.VariableNames, h.Model.FactorNames("Factor "), h.Model.StructureMatrix, GetOptionalBool(includeHeader, True))
            Catch ex As Exception
                Return LoggedUdfExceptionText("BESH.MULTI.FA_STRUCTURE", ex)
            End Try
        End Function

        ''' <summary>
        ''' Returns the factor-correlation matrix for a fitted factor-analysis model.
        ''' </summary>
        ''' <param name="handle">Handle returned by <c>BESH.MULTI.FA_FIT</c>.</param>
        ''' <param name="includeHeader">TRUE to include a header row. Default TRUE.</param>
        ''' <returns>
        ''' A labeled square matrix of factor correlations. Under orthogonal rotation this is the identity matrix.
        ''' Under oblique rotation the off-diagonal values show how strongly the retained factors correlate with one another.
        ''' </returns>
        <ExcelFunction(
            Name:="BESH.MULTI.FA_FACTORCORR",
            Category:="BESHStatNG - Multivariate Analysis",
            Description:="Returns the factor-correlation matrix for a fitted factor-analysis model.",
            HelpTopic:=HelpLinks.FallbackBaseUrl & "/udf/multivariate-analysis/"
        )>
        Public Function FA_FACTORCORR(
            <ExcelArgument(Name:="handle", Description:="Handle returned by BESH.MULTI.FA_FIT.")> handle As Object,
            <ExcelArgument(Name:="includeHeader", Description:="TRUE to include a header row (default TRUE).")> Optional includeHeader As Object = Nothing
        ) As Object
            Try
                Dim h As FactorAnalysisHandle = Nothing
                If Not TryGetFaHandle(handle, h) Then Return ExcelError.ExcelErrorNA
                Dim factorNames() As String = h.Model.FactorNames("Factor ")
                Return BuildNamedMatrixOutput("Factor", factorNames, factorNames, h.Model.FactorCorrelationMatrix, GetOptionalBool(includeHeader, True))
            Catch ex As Exception
                Return LoggedUdfExceptionText("BESH.MULTI.FA_FACTORCORR", ex)
            End Try
        End Function

        ''' <summary>
        ''' Returns the communalities table for a fitted factor-analysis model.
        ''' </summary>
        ''' <param name="handle">Handle returned by <c>BESH.MULTI.FA_FIT</c>.</param>
        ''' <param name="includeHeader">TRUE to include a header row. Default TRUE.</param>
        ''' <returns>
        ''' A spilled table giving the initial communality, each factor’s contribution, the final extracted communality, and the uniqueness for every variable.
        ''' Large communalities indicate that the retained factors explain most of a variable’s variance. Large uniqueness values indicate that much of the variance remains specific or residual.
        ''' </returns>
        <ExcelFunction(
            Name:="BESH.MULTI.FA_COMMUNALITIES",
            Category:="BESHStatNG - Multivariate Analysis",
            Description:="Returns communalities and uniquenesses for a fitted factor-analysis model.",
            HelpTopic:=HelpLinks.FallbackBaseUrl & "/udf/multivariate-analysis/"
        )>
        Public Function FA_COMMUNALITIES(
            <ExcelArgument(Name:="handle", Description:="Handle returned by BESH.MULTI.FA_FIT.")> handle As Object,
            <ExcelArgument(Name:="includeHeader", Description:="TRUE to include a header row (default TRUE).")> Optional includeHeader As Object = Nothing
        ) As Object
            Try
                Dim h As FactorAnalysisHandle = Nothing
                If Not TryGetFaHandle(handle, h) Then Return ExcelError.ExcelErrorNA

                Dim factorNames() As String = h.Model.FactorNames("Factor ")
                Dim contrib(,) As Double = h.Model.CommunalityContributionsByFactor()
                Dim p As Integer = h.VariableNames.Length
                Dim k As Integer = h.Model.NumberOfFactors
                Dim hdr As Boolean = GetOptionalBool(includeHeader, True)
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
                    out(r0 + i, 0) = h.VariableNames(i)
                    out(r0 + i, 1) = h.Model.InitialCommunalities(i)
                    For j As Integer = 0 To k - 1
                        out(r0 + i, j + 2) = contrib(i, j)
                    Next
                    out(r0 + i, k + 2) = h.Model.Communalities(i)
                    out(r0 + i, k + 3) = h.Model.Uniquenesses(i)
                Next

                Return PrepareResultTableForUdf(out)
            Catch ex As Exception
                Return LoggedUdfExceptionText("BESH.MULTI.FA_COMMUNALITIES", ex)
            End Try
        End Function

        ''' <summary>
        ''' Returns factorability diagnostics for a fitted factor-analysis model.
        ''' </summary>
        ''' <param name="handle">Handle returned by <c>BESH.MULTI.FA_FIT</c>.</param>
        ''' <param name="includeHeader">TRUE to include a header row. Default TRUE.</param>
        ''' <returns>
        ''' A spilled table containing the determinant of the correlation matrix, overall KMO, Bartlett’s test,
        ''' its degrees of freedom and p-value, and RMSR. These diagnostics help judge whether factor analysis is appropriate
        ''' and how well the retained-factor solution reproduces the observed association matrix.
        ''' </returns>
        <ExcelFunction(
            Name:="BESH.MULTI.FA_FACTORABILITY",
            Category:="BESHStatNG - Multivariate Analysis",
            Description:="Returns factorability diagnostics for a fitted factor-analysis model.",
            HelpTopic:=HelpLinks.FallbackBaseUrl & "/udf/multivariate-analysis/"
        )>
        Public Function FA_FACTORABILITY(
            <ExcelArgument(Name:="handle", Description:="Handle returned by BESH.MULTI.FA_FIT.")> handle As Object,
            <ExcelArgument(Name:="includeHeader", Description:="TRUE to include a header row (default TRUE).")> Optional includeHeader As Object = Nothing
        ) As Object
            Try
                Dim h As FactorAnalysisHandle = Nothing
                If Not TryGetFaHandle(handle, h) Then Return ExcelError.ExcelErrorNA

                Dim hdr As Boolean = GetOptionalBool(includeHeader, True)
                Dim out(If(hdr, 5, 4), 1) As Object
                Dim r0 As Integer = 0
                If hdr Then
                    out(0, 0) = "Statistic"
                    out(0, 1) = "Value"
                    r0 = 1
                End If

                out(r0 + 0, 0) = "Determinant of correlation matrix"
                out(r0 + 0, 1) = h.Model.DeterminantCorrelation
                out(r0 + 1, 0) = "Overall KMO"
                out(r0 + 1, 1) = h.Model.KmoOverall
                out(r0 + 2, 0) = "Bartlett Chi-square"
                out(r0 + 2, 1) = h.Model.BartlettChiSquare
                out(r0 + 3, 0) = "Bartlett df"
                out(r0 + 3, 1) = h.Model.BartlettDegreesOfFreedom
                out(r0 + 4, 0) = "Bartlett p-value"
                out(r0 + 4, 1) = h.Model.BartlettPValue
                out(r0 + 5, 0) = "RMSR"
                out(r0 + 5, 1) = h.Model.RMSR

                Return PrepareResultTableForUdf(out)
            Catch ex As Exception
                Return LoggedUdfExceptionText("BESH.MULTI.FA_FACTORABILITY", ex)
            End Try
        End Function

        ''' <summary>
        ''' Returns factor scores for the analyzed observations.
        ''' </summary>
        ''' <param name="handle">Handle returned by <c>BESH.MULTI.FA_FIT</c>.</param>
        ''' <param name="includeHeader">TRUE to include a header row. Default TRUE.</param>
        ''' <returns>
        ''' A spilled table with one row per analyzed observation and one column per retained factor.
        ''' The first column contains row identifiers after any listwise deletion requested by the missing-value policy.
        ''' If factor scores were not requested when the model was fitted, this function returns <c>#N/A</c>.
        ''' </returns>
        <ExcelFunction(
            Name:="BESH.MULTI.FA_SCORES",
            Category:="BESHStatNG - Multivariate Analysis",
            Description:="Returns factor scores for the analyzed observations.",
            HelpTopic:=HelpLinks.FallbackBaseUrl & "/udf/multivariate-analysis/"
        )>
        Public Function FA_SCORES(
            <ExcelArgument(Name:="handle", Description:="Handle returned by BESH.MULTI.FA_FIT.")> handle As Object,
            <ExcelArgument(Name:="includeHeader", Description:="TRUE to include a header row (default TRUE).")> Optional includeHeader As Object = Nothing
        ) As Object
            Try
                Dim h As FactorAnalysisHandle = Nothing
                If Not TryGetFaHandle(handle, h) Then Return ExcelError.ExcelErrorNA
                If h.Model.Scores Is Nothing OrElse h.Model.AnalysisRowIds Is Nothing Then Return ExcelError.ExcelErrorNA
                Return BuildCaseMatrixOutput("Row", h.Model.AnalysisRowIds, h.Model.FactorNames("Factor "), h.Model.Scores, GetOptionalBool(includeHeader, True))
            Catch ex As Exception
                Return LoggedUdfExceptionText("BESH.MULTI.FA_SCORES", ex)
            End Try
        End Function

        ''' <summary>
        ''' Removes a factor-analysis handle from memory.
        ''' </summary>
        ''' <param name="handle">Handle returned by <c>BESH.MULTI.FA_FIT</c>.</param>
        ''' <returns>TRUE when the handle was removed; FALSE when the handle was not found.</returns>
        <ExcelFunction(
            Name:="BESH.MULTI.FA_DROP",
            Category:="BESHStatNG - Multivariate Analysis",
            Description:="Removes a factor-analysis handle from memory.",
            HelpTopic:=HelpLinks.FallbackBaseUrl & "/udf/multivariate-analysis/"
        )>
        Public Function FA_DROP(
            <ExcelArgument(Name:="handle", Description:="Handle returned by BESH.MULTI.FA_FIT.")> handle As Object
        ) As Object
            Try
                Dim key As String = AsString(handle)
                If String.IsNullOrWhiteSpace(key) Then Return False
                Dim removed As FactorAnalysisHandle = Nothing
                Return _faCache.TryRemove(key.Trim(), removed)
            Catch ex As Exception
                Return LoggedUdfExceptionText("BESH.MULTI.FA_DROP", ex)
            End Try
        End Function

        ''' <summary>
        ''' Fits a k-means clustering model and returns a reusable handle.
        ''' </summary>
        ''' <param name="x">
        ''' Numeric data matrix with observations in rows and variables in columns.
        ''' A single header row is detected automatically when present. Missing values are passed through to the clustering engine
        ''' so the requested missing-value policy can either stop the analysis or remove incomplete rows listwise.
        ''' </param>
        ''' <param name="varNames">
        ''' Optional variable names supplied either as a comma-separated list or as a one-row or one-column range.
        ''' When omitted, names are taken from the detected header row when available, otherwise default names X1, X2, … are generated.
        ''' </param>
        ''' <param name="rowLabels">
        ''' Optional one-column range of observation labels aligned with <paramref name="x"/>.
        ''' These labels are carried into the assignment and removed-row tables. When omitted, generic observation labels are used.
        ''' </param>
        ''' <param name="numberOfClusters">Requested number of clusters <c>k</c>. Default 3.</param>
        ''' <param name="initializationMethod">
        ''' Optional initialization strategy: <c>"kmeans++"</c> (default), <c>"forgy"</c>, <c>"randompartition"</c>, or <c>"userspecified"</c>.
        ''' When <paramref name="startingCenters"/> is supplied, user-specified centers are used automatically.
        ''' </param>
        ''' <param name="distanceMetric">
        ''' Optional reporting distance: <c>"squaredeuclidean"</c> (default) or <c>"euclidean"</c>.
        ''' Classical k-means still minimizes the sum of squared Euclidean distances either way; this option mainly affects the displayed distances.
        ''' </param>
        ''' <param name="nStarts">Optional number of random starts. Default 10.</param>
        ''' <param name="maxIterations">Optional maximum number of update iterations per start. Default 100.</param>
        ''' <param name="tolerance">Optional convergence tolerance for center movement on the working analysis scale. Default 0.000001.</param>
        ''' <param name="standardization">
        ''' Optional preprocessing mode: <c>"none"</c> (default), <c>"zscores"</c>, or <c>"range01"</c>.
        ''' Standardization is helpful when variables are measured on very different scales.
        ''' </param>
        ''' <param name="missingValuePolicy">Optional missing-data policy: <c>"error"</c> (default) or <c>"listwise"</c>.</param>
        ''' <param name="emptyClusterHandling">
        ''' Optional strategy for a temporarily empty cluster: <c>"farthestobservation"</c> (default), <c>"randomobservation"</c>, or <c>"keeppreviouscenter"</c>.
        ''' </param>
        ''' <param name="randomSeed">
        ''' Optional deterministic random seed. Leave blank to use a time-based seed. Supplying a seed improves reproducibility across recalculations.
        ''' </param>
        ''' <param name="startingCenters">
        ''' Optional matrix of user-specified starting centers, with one row per cluster and one column per variable.
        ''' When supplied, the matrix must have exactly <paramref name="numberOfClusters"/> rows and the same number of columns as <paramref name="x"/>.
        ''' </param>
        ''' <returns>
        ''' A text handle for the fitted k-means solution. Pass the handle to the other <c>KMEANS_*</c> worksheet functions
        ''' to retrieve the fit summary, centers, assignments, preprocessing constants, or removed-row report.
        ''' </returns>
        ''' <remarks>
        ''' <para>
        ''' K-means partitions observations into <c>k</c> compact centroid-based clusters. Because the objective is non-convex,
        ''' the final partition can depend on the starting centers. Multiple random starts and k-means++ seeding often improve the solution.
        ''' </para>
        ''' <para>
        ''' The handle-based design is useful when you want to inspect several outputs from the same fitted partition without repeating the fit.
        ''' </para>
        ''' </remarks>
        ''' <example>
        ''' <code>
        ''' =BESH.MULTI.KMEANS_FIT(A1:F101)
        ''' =BESH.MULTI.KMEANS_FIT(A1:F101,,G1:G101,4,"kmeans++","squaredeuclidean",25,100,1E-06,"zscores","listwise","farthestobservation",12345)
        ''' </code>
        ''' </example>
        <ExcelFunction(
            Name:="BESH.MULTI.KMEANS_FIT",
            Category:="BESHStatNG - Multivariate Analysis",
            Description:="Fits a k-means clustering model and returns a reusable handle.",
            HelpTopic:=HelpLinks.FallbackBaseUrl & "/udf/multivariate-analysis/"
        )>
        Public Function KMEANS_FIT(
            <ExcelArgument(AllowReference:=True, Name:="x", Description:="Numeric data matrix with observations in rows and variables in columns.")> x As Object,
            <ExcelArgument(Name:="varNames", Description:="Optional variable names as a comma-separated list or a one-row/one-column range.")> Optional varNames As Object = Nothing,
            <ExcelArgument(Name:="rowLabels", Description:="Optional one-column range of observation labels aligned with the data matrix.")> Optional rowLabels As Object = Nothing,
            <ExcelArgument(Name:="numberOfClusters", Description:="Requested number of clusters k. Default 3.")> Optional numberOfClusters As Object = Nothing,
            <ExcelArgument(Name:="initializationMethod", Description:="Optional initialization: kmeans++ (default), forgy, randompartition, or userspecified.")> Optional initializationMethod As Object = Nothing,
            <ExcelArgument(Name:="distanceMetric", Description:="Optional reporting distance: squaredeuclidean (default) or euclidean.")> Optional distanceMetric As Object = Nothing,
            <ExcelArgument(Name:="nStarts", Description:="Optional number of random starts. Default 10.")> Optional nStarts As Object = Nothing,
            <ExcelArgument(Name:="maxIterations", Description:="Optional maximum number of update iterations per start. Default 100.")> Optional maxIterations As Object = Nothing,
            <ExcelArgument(Name:="tolerance", Description:="Optional convergence tolerance for center movement. Default 0.000001.")> Optional tolerance As Object = Nothing,
            <ExcelArgument(Name:="standardization", Description:="Optional preprocessing: none (default), zscores, or range01.")> Optional standardization As Object = Nothing,
            <ExcelArgument(Name:="missingValuePolicy", Description:="Optional missing-data policy: error (default) or listwise.")> Optional missingValuePolicy As Object = Nothing,
            <ExcelArgument(Name:="emptyClusterHandling", Description:="Optional empty-cluster strategy: farthestobservation (default), randomobservation, or keeppreviouscenter.")> Optional emptyClusterHandling As Object = Nothing,
            <ExcelArgument(Name:="randomSeed", Description:="Optional deterministic random seed.")> Optional randomSeed As Object = Nothing,
            <ExcelArgument(AllowReference:=True, Name:="startingCenters", Description:="Optional user-specified starting-center matrix with one row per cluster and one column per variable.")> Optional startingCenters As Object = Nothing
        ) As Object

            If ExcelDnaUtil.IsInFunctionWizard() Then Return "KMEANS_FIT (editing...)"

            Try
                Dim imported As DataObj = Nothing
                If Not Global.BESHStatNG.UdfDataImport.TryGetNumericData(x, varNames, True, imported) Then Return ExcelError.ExcelErrorValue
                If imported.nRows < 1 OrElse imported.nCols < 1 Then Return ExcelError.ExcelErrorNum

                Dim labels() As String = Nothing
                If Not TryResolveOptionalClusterRowLabels(rowLabels, imported.nRows, labels) Then Return ExcelError.ExcelErrorValue

                Dim k As Integer = GetOptionalInt(numberOfClusters, 3)
                Dim initChoice As Multivariate.KMeansInitializationMethod = ParseKMeansInitialization(initializationMethod)
                Dim userCentersProvided As Boolean = Not (startingCenters Is Nothing OrElse TypeOf startingCenters Is ExcelEmpty OrElse TypeOf startingCenters Is ExcelMissing)

                Dim fit As New Multivariate.KMeans()
                fit.dataInputs(imported.DataDbl, labels, imported.varNames)

                If userCentersProvided Then
                    Dim centersData As DataObj = Nothing
                    If Not Global.BESHStatNG.UdfDataImport.TryGetNumericData(startingCenters, imported.varNames, False, centersData) Then Return ExcelError.ExcelErrorValue
                    If centersData.nCols <> imported.nCols Then Throw New ArgumentException("startingCenters must have the same number of columns as x.")
                    If centersData.nRows <> k Then Throw New ArgumentException("startingCenters must have exactly numberOfClusters rows.")
                    fit.startingCentersInputs(centersData.DataDbl)
                    initChoice = Multivariate.KMeansInitializationMethod.UserSpecifiedCenters
                End If

                Dim seed As Integer = If(randomSeed Is Nothing OrElse TypeOf randomSeed Is ExcelEmpty OrElse TypeOf randomSeed Is ExcelMissing,
                                         Integer.MinValue,
                                         GetOptionalInt(randomSeed, Integer.MinValue))

                fit.settingsInputs(numberOfClusters:=k,
                                   initialization:=initChoice,
                                   distanceMetric:=ParseKMeansDistanceMetric(distanceMetric),
                                   nStarts:=GetOptionalInt(nStarts, 10),
                                   maxIterations:=GetOptionalInt(maxIterations, 100),
                                   convergenceTolerance:=GetOptionalDouble(tolerance, 0.000001R),
                                   standardization:=ParseClusterStandardizationMode(standardization),
                                   missingValuePolicy:=ParseClusterMissingValuePolicy(missingValuePolicy),
                                   emptyClusterHandling:=ParseEmptyClusterHandling(emptyClusterHandling),
                                   randomSeed:=seed)
                fit.Fit()

                Dim handleKey As String = "KMEANS:" & Guid.NewGuid().ToString("N")
                Dim info As New KMeansHandle With {
                    .Handle = handleKey,
                    .Model = fit,
                    .VariableNames = CloneStringArray(imported.varNames),
                    .RowLabels = CloneStringArray(labels),
                    .NumberOfClusters = k,
                    .InitializationMethod = initChoice.ToString(),
                    .DistanceMetric = ParseKMeansDistanceMetric(distanceMetric).ToString(),
                    .Standardization = ParseClusterStandardizationMode(standardization).ToString(),
                    .MissingValuePolicy = ParseClusterMissingValuePolicy(missingValuePolicy).ToString(),
                    .EmptyClusterHandling = ParseEmptyClusterHandling(emptyClusterHandling).ToString(),
                    .RandomStarts = If(initChoice = Multivariate.KMeansInitializationMethod.UserSpecifiedCenters, 1, GetOptionalInt(nStarts, 10)),
                    .MaxIterations = GetOptionalInt(maxIterations, 100),
                    .Tolerance = GetOptionalDouble(tolerance, 0.000001R),
                    .RequestedRandomSeed = seed
                }
                _kmeansCache(handleKey) = info
                Return handleKey
            Catch ex As Exception
                Return LoggedUdfExceptionText("BESH.MULTI.KMEANS_FIT", ex)
            End Try
        End Function

        ''' <summary>
        ''' Returns a compact settings and fit summary for a fitted k-means model.
        ''' </summary>
        ''' <param name="handle">Handle returned by <c>BESH.MULTI.KMEANS_FIT</c>.</param>
        ''' <param name="includeHeader">TRUE to include a header row. Default TRUE.</param>
        ''' <returns>
        ''' A spilled two-column table listing the requested and realized clustering settings together with key fit diagnostics,
        ''' including the number of active observations, removed observations, convergence, and sums of squares.
        ''' </returns>
        <ExcelFunction(
            Name:="BESH.MULTI.KMEANS_SUMMARY",
            Category:="BESHStatNG - Multivariate Analysis",
            Description:="Returns a compact settings and fit summary for a fitted k-means model.",
            HelpTopic:=HelpLinks.FallbackBaseUrl & "/udf/multivariate-analysis/"
        )>
        Public Function KMEANS_SUMMARY(
            <ExcelArgument(Name:="handle", Description:="Handle returned by BESH.MULTI.KMEANS_FIT.")> handle As Object,
            <ExcelArgument(Name:="includeHeader", Description:="TRUE to include a header row (default TRUE).")> Optional includeHeader As Object = Nothing
        ) As Object
            Try
                Dim h As KMeansHandle = Nothing
                If Not TryGetKMeansHandle(handle, h) Then Return ExcelError.ExcelErrorNA

                Dim hdr As Boolean = GetOptionalBool(includeHeader, True)
                Dim out(If(hdr, 19, 18), 1) As Object
                Dim r0 As Integer = 0
                If hdr Then
                    out(0, 0) = "Setting"
                    out(0, 1) = "Value"
                    r0 = 1
                End If

                out(r0 + 0, 0) = "Number of clusters"
                out(r0 + 0, 1) = h.Model.Result.NumberOfClusters
                out(r0 + 1, 0) = "Rows analyzed"
                out(r0 + 1, 1) = If(h.Model.Result.ClusterAssignments Is Nothing, 0, h.Model.Result.ClusterAssignments.Length)
                out(r0 + 2, 0) = "Rows removed"
                out(r0 + 2, 1) = If(h.Model.Result.RemovedRowIndices Is Nothing, 0, h.Model.Result.RemovedRowIndices.Length)
                out(r0 + 3, 0) = "Variables"
                out(r0 + 3, 1) = h.VariableNames.Length
                out(r0 + 4, 0) = "Initialization"
                out(r0 + 4, 1) = h.InitializationMethod
                out(r0 + 5, 0) = "Distance metric"
                out(r0 + 5, 1) = h.DistanceMetric
                out(r0 + 6, 0) = "Standardization"
                out(r0 + 6, 1) = h.Standardization
                out(r0 + 7, 0) = "Missing-value policy"
                out(r0 + 7, 1) = h.MissingValuePolicy
                out(r0 + 8, 0) = "Empty-cluster handling"
                out(r0 + 8, 1) = h.EmptyClusterHandling
                out(r0 + 9, 0) = "Random starts evaluated"
                out(r0 + 9, 1) = h.Model.Result.StartsEvaluated
                out(r0 + 10, 0) = "Max iterations per start"
                out(r0 + 10, 1) = h.MaxIterations
                out(r0 + 11, 0) = "Tolerance"
                out(r0 + 11, 1) = h.Tolerance
                out(r0 + 12, 0) = "Converged"
                out(r0 + 12, 1) = h.Model.Result.Converged
                out(r0 + 13, 0) = "Iterations used by best solution"
                out(r0 + 13, 1) = h.Model.Result.Iterations
                out(r0 + 14, 0) = "Total within-cluster sum of squares"
                out(r0 + 14, 1) = h.Model.Result.TotalWithinClusterSS
                out(r0 + 15, 0) = "Between-cluster sum of squares"
                out(r0 + 15, 1) = h.Model.Result.BetweenClusterSS
                out(r0 + 16, 0) = "Total sum of squares"
                out(r0 + 16, 1) = h.Model.Result.TotalSS
                out(r0 + 17, 0) = "Objective value"
                out(r0 + 17, 1) = h.Model.Result.ObjectiveValue
                out(r0 + 18, 0) = "Random seed used"
                out(r0 + 18, 1) = h.Model.Result.RandomSeedUsed

                Return PrepareResultTableForUdf(out)
            Catch ex As Exception
                Return LoggedUdfExceptionText("BESH.MULTI.KMEANS_SUMMARY", ex)
            End Try
        End Function

        ''' <summary>
        ''' Returns the fitted k-means cluster centers.
        ''' </summary>
        ''' <param name="handle">Handle returned by <c>BESH.MULTI.KMEANS_FIT</c>.</param>
        ''' <param name="scale">
        ''' Optional center scale: <c>"original"</c> (default) or <c>"working"</c>.
        ''' Use original to report centers back on the original measurement scale, or working to inspect the centers after preprocessing.
        ''' </param>
        ''' <param name="includeHeader">TRUE to include a header row. Default TRUE.</param>
        ''' <returns>
        ''' A spilled table with one row per cluster containing the cluster size, the within-cluster sum of squares for that cluster,
        ''' and the fitted center coordinates.
        ''' </returns>
        <ExcelFunction(
            Name:="BESH.MULTI.KMEANS_CENTERS",
            Category:="BESHStatNG - Multivariate Analysis",
            Description:="Returns the fitted k-means cluster centers.",
            HelpTopic:=HelpLinks.FallbackBaseUrl & "/udf/multivariate-analysis/"
        )>
        Public Function KMEANS_CENTERS(
            <ExcelArgument(Name:="handle", Description:="Handle returned by BESH.MULTI.KMEANS_FIT.")> handle As Object,
            <ExcelArgument(Name:="scale", Description:="Optional center scale: original (default) or working.")> Optional scale As Object = Nothing,
            <ExcelArgument(Name:="includeHeader", Description:="TRUE to include a header row (default TRUE).")> Optional includeHeader As Object = Nothing
        ) As Object
            Try
                Dim h As KMeansHandle = Nothing
                If Not TryGetKMeansHandle(handle, h) Then Return ExcelError.ExcelErrorNA
                Dim useOriginalScale As Boolean = ParseOutputScaleUseOriginal(scale)
                Return PrepareExistingObjectTableForUdf(h.Model.Result.GetCentersTable(useOriginalScale), GetOptionalBool(includeHeader, True))
            Catch ex As Exception
                Return LoggedUdfExceptionText("BESH.MULTI.KMEANS_CENTERS", ex)
            End Try
        End Function

        ''' <summary>
        ''' Returns the active-observation assignment table for a fitted k-means model.
        ''' </summary>
        ''' <param name="handle">Handle returned by <c>BESH.MULTI.KMEANS_FIT</c>.</param>
        ''' <param name="includeHeader">TRUE to include a header row. Default TRUE.</param>
        ''' <returns>
        ''' A spilled table containing the original row number, optional row label, assigned cluster, and point-to-center distance
        ''' for every active observation retained in the fitted k-means analysis.
        ''' </returns>
        <ExcelFunction(
            Name:="BESH.MULTI.KMEANS_ASSIGNMENTS",
            Category:="BESHStatNG - Multivariate Analysis",
            Description:="Returns the active-observation assignment table for a fitted k-means model.",
            HelpTopic:=HelpLinks.FallbackBaseUrl & "/udf/multivariate-analysis/"
        )>
        Public Function KMEANS_ASSIGNMENTS(
            <ExcelArgument(Name:="handle", Description:="Handle returned by BESH.MULTI.KMEANS_FIT.")> handle As Object,
            <ExcelArgument(Name:="includeHeader", Description:="TRUE to include a header row (default TRUE).")> Optional includeHeader As Object = Nothing
        ) As Object
            Try
                Dim h As KMeansHandle = Nothing
                If Not TryGetKMeansHandle(handle, h) Then Return ExcelError.ExcelErrorNA
                Return PrepareExistingObjectTableForUdf(h.Model.Result.GetObservationAssignmentsTable(), GetOptionalBool(includeHeader, True))
            Catch ex As Exception
                Return LoggedUdfExceptionText("BESH.MULTI.KMEANS_ASSIGNMENTS", ex)
            End Try
        End Function

        ''' <summary>
        ''' Returns the preprocessing constants used by a fitted k-means model.
        ''' </summary>
        ''' <param name="handle">Handle returned by <c>BESH.MULTI.KMEANS_FIT</c>.</param>
        ''' <param name="includeHeader">TRUE to include a header row. Default TRUE.</param>
        ''' <returns>
        ''' A spilled table of variable-wise location and scale constants used during preprocessing.
        ''' For z-score standardization the location is the mean and the scale is the sample standard deviation.
        ''' For range standardization the location is the minimum and the scale is the observed range. When no preprocessing was applied,
        ''' the function returns a compact note table rather than an error.
        ''' </returns>
        <ExcelFunction(
            Name:="BESH.MULTI.KMEANS_PREPROCESS",
            Category:="BESHStatNG - Multivariate Analysis",
            Description:="Returns the preprocessing constants used by a fitted k-means model.",
            HelpTopic:=HelpLinks.FallbackBaseUrl & "/udf/multivariate-analysis/"
        )>
        Public Function KMEANS_PREPROCESS(
            <ExcelArgument(Name:="handle", Description:="Handle returned by BESH.MULTI.KMEANS_FIT.")> handle As Object,
            <ExcelArgument(Name:="includeHeader", Description:="TRUE to include a header row (default TRUE).")> Optional includeHeader As Object = Nothing
        ) As Object
            Try
                Dim h As KMeansHandle = Nothing
                If Not TryGetKMeansHandle(handle, h) Then Return ExcelError.ExcelErrorNA
                Return PrepareExistingObjectTableForUdf(BuildClusterPreprocessingTable(h.Model.Result.VariableNames,
                                                                                       h.Model.Result.StandardizationLocations,
                                                                                       h.Model.Result.StandardizationScales,
                                                                                       h.Model.Result.Standardization),
                                                       GetOptionalBool(includeHeader, True))
            Catch ex As Exception
                Return LoggedUdfExceptionText("BESH.MULTI.KMEANS_PREPROCESS", ex)
            End Try
        End Function

        ''' <summary>
        ''' Returns the rows removed by the missing-value policy before k-means fitting.
        ''' </summary>
        ''' <param name="handle">Handle returned by <c>BESH.MULTI.KMEANS_FIT</c>.</param>
        ''' <param name="includeHeader">TRUE to include a header row. Default TRUE.</param>
        ''' <returns>
        ''' A spilled table listing the original row numbers and optional row labels removed before fitting because at least one analysis variable
        ''' was missing or non-finite. When no rows were removed, the function returns a short note table instead of an error.
        ''' </returns>
        <ExcelFunction(
            Name:="BESH.MULTI.KMEANS_REMOVED",
            Category:="BESHStatNG - Multivariate Analysis",
            Description:="Returns the rows removed by the missing-value policy before k-means fitting.",
            HelpTopic:=HelpLinks.FallbackBaseUrl & "/udf/multivariate-analysis/"
        )>
        Public Function KMEANS_REMOVED(
            <ExcelArgument(Name:="handle", Description:="Handle returned by BESH.MULTI.KMEANS_FIT.")> handle As Object,
            <ExcelArgument(Name:="includeHeader", Description:="TRUE to include a header row (default TRUE).")> Optional includeHeader As Object = Nothing
        ) As Object
            Try
                Dim h As KMeansHandle = Nothing
                If Not TryGetKMeansHandle(handle, h) Then Return ExcelError.ExcelErrorNA
                Return PrepareExistingObjectTableForUdf(BuildRemovedRowsOutput(h.Model.Result.RemovedRowIndices, h.Model.Result.RemovedRowLabels),
                                                       GetOptionalBool(includeHeader, True))
            Catch ex As Exception
                Return LoggedUdfExceptionText("BESH.MULTI.KMEANS_REMOVED", ex)
            End Try
        End Function

        ''' <summary>
        ''' Removes a k-means handle from memory.
        ''' </summary>
        ''' <param name="handle">Handle returned by <c>BESH.MULTI.KMEANS_FIT</c>.</param>
        ''' <returns>TRUE when the handle was removed; FALSE when the handle was not found.</returns>
        <ExcelFunction(
            Name:="BESH.MULTI.KMEANS_DROP",
            Category:="BESHStatNG - Multivariate Analysis",
            Description:="Removes a k-means handle from memory.",
            HelpTopic:=HelpLinks.FallbackBaseUrl & "/udf/multivariate-analysis/"
        )>
        Public Function KMEANS_DROP(
            <ExcelArgument(Name:="handle", Description:="Handle returned by BESH.MULTI.KMEANS_FIT.")> handle As Object
        ) As Object
            Try
                Dim key As String = AsString(handle)
                If String.IsNullOrWhiteSpace(key) Then Return False
                Dim removed As KMeansHandle = Nothing
                Return _kmeansCache.TryRemove(key.Trim(), removed)
            Catch ex As Exception
                Return LoggedUdfExceptionText("BESH.MULTI.KMEANS_DROP", ex)
            End Try
        End Function

        ''' <summary>
        ''' Fits an agglomerative hierarchical clustering model and returns a reusable handle.
        ''' </summary>
        ''' <param name="x">
        ''' Numeric data matrix with observations in rows and variables in columns.
        ''' A single header row is detected automatically when present. Missing values are passed through to the clustering engine
        ''' so the requested missing-value policy can either stop the analysis or remove incomplete rows listwise.
        ''' </param>
        ''' <param name="varNames">
        ''' Optional variable names supplied either as a comma-separated list or as a one-row or one-column range.
        ''' When omitted, names are taken from the detected header row when available, otherwise default names X1, X2, … are generated.
        ''' </param>
        ''' <param name="rowLabels">
        ''' Optional one-column range of observation labels aligned with <paramref name="x"/>.
        ''' These labels are carried into the leaf-order, membership, and removed-row tables. When omitted, generic labels are used.
        ''' </param>
        ''' <param name="linkage">
        ''' Optional agglomeration rule: <c>"ward"</c> (default), <c>"single"</c>, <c>"complete"</c>, <c>"average"</c>,
        ''' <c>"weightedaverage"</c>, <c>"centroid"</c>, or <c>"median"</c>.
        ''' </param>
        ''' <param name="distanceMetric">
        ''' Optional base observation-level distance: <c>"squaredeuclidean"</c> (default), <c>"euclidean"</c>, <c>"manhattan"</c>,
        ''' <c>"chebyshev"</c>, <c>"minkowski"</c>, <c>"cosine"</c>, or <c>"correlation"</c>.
        ''' Some linkages impose restrictions: centroid, median, and Ward linkage require Euclidean or squared Euclidean distance.
        ''' </param>
        ''' <param name="minkowskiPower">Optional Minkowski power parameter used only when <paramref name="distanceMetric"/> is <c>"minkowski"</c>. Default 2.</param>
        ''' <param name="standardization">Optional preprocessing mode: none (default), zscores, or range01.</param>
        ''' <param name="missingValuePolicy">Optional missing-data policy: error (default) or listwise.</param>
        ''' <returns>
        ''' A text handle for the fitted hierarchical clustering solution. Pass the handle to the other <c>HCLUST_*</c> worksheet functions
        ''' to retrieve the fit summary, agglomeration schedule, leaf order, membership tables, preprocessing constants, or removed-row report.
        ''' </returns>
        ''' <remarks>
        ''' <para>
        ''' Hierarchical clustering builds a full merge tree rather than a single partition. You can later cut that tree either to a requested number
        ''' of clusters or at a chosen merge height without refitting the model.
        ''' </para>
        ''' <para>
        ''' The handle-based design is therefore especially convenient: fit the tree once, then inspect several alternative cut levels using
        ''' repeated calls to <c>HCLUST_MEMBERSHIP</c>.
        ''' </para>
        ''' </remarks>
        ''' <example>
        ''' <code>
        ''' =BESH.MULTI.HCLUST_FIT(A1:F101)
        ''' =BESH.MULTI.HCLUST_FIT(A1:F101,,G1:G101,"ward","squaredeuclidean",2,"zscores","listwise")
        ''' </code>
        ''' </example>
        <ExcelFunction(
            Name:="BESH.MULTI.HCLUST_FIT",
            Category:="BESHStatNG - Multivariate Analysis",
            Description:="Fits an agglomerative hierarchical clustering model and returns a reusable handle.",
            HelpTopic:=HelpLinks.FallbackBaseUrl & "/udf/multivariate-analysis/"
        )>
        Public Function HCLUST_FIT(
            <ExcelArgument(AllowReference:=True, Name:="x", Description:="Numeric data matrix with observations in rows and variables in columns.")> x As Object,
            <ExcelArgument(Name:="varNames", Description:="Optional variable names as a comma-separated list or a one-row/one-column range.")> Optional varNames As Object = Nothing,
            <ExcelArgument(Name:="rowLabels", Description:="Optional one-column range of observation labels aligned with the data matrix.")> Optional rowLabels As Object = Nothing,
            <ExcelArgument(Name:="linkage", Description:="Optional linkage: ward (default), single, complete, average, weightedaverage, centroid, or median.")> Optional linkage As Object = Nothing,
            <ExcelArgument(Name:="distanceMetric", Description:="Optional distance: squaredeuclidean (default), euclidean, manhattan, chebyshev, minkowski, cosine, or correlation.")> Optional distanceMetric As Object = Nothing,
            <ExcelArgument(Name:="minkowskiPower", Description:="Optional Minkowski power used only with Minkowski distance. Default 2.")> Optional minkowskiPower As Object = Nothing,
            <ExcelArgument(Name:="standardization", Description:="Optional preprocessing: none (default), zscores, or range01.")> Optional standardization As Object = Nothing,
            <ExcelArgument(Name:="missingValuePolicy", Description:="Optional missing-data policy: error (default) or listwise.")> Optional missingValuePolicy As Object = Nothing
        ) As Object

            If ExcelDnaUtil.IsInFunctionWizard() Then Return "HCLUST_FIT (editing...)"

            Try
                Dim imported As DataObj = Nothing
                If Not Global.BESHStatNG.UdfDataImport.TryGetNumericData(x, varNames, True, imported) Then Return ExcelError.ExcelErrorValue
                If imported.nRows < 2 OrElse imported.nCols < 1 Then Return ExcelError.ExcelErrorNum

                Dim labels() As String = Nothing
                If Not TryResolveOptionalClusterRowLabels(rowLabels, imported.nRows, labels) Then Return ExcelError.ExcelErrorValue

                Dim linkageChoice As Multivariate.HierarchicalLinkageMethod = ParseHierarchicalLinkage(linkage)
                Dim distanceChoice As Multivariate.HierarchicalDistanceMetric = ParseHierarchicalDistanceMetric(distanceMetric)
                Dim fit As New Multivariate.HierarchicalClustering()
                fit.dataInputs(imported.DataDbl, labels, imported.varNames)
                fit.settingsInputs(linkage:=linkageChoice,
                                   distanceMetric:=distanceChoice,
                                   minkowskiPower:=GetOptionalDouble(minkowskiPower, 2.0R),
                                   standardization:=ParseClusterStandardizationMode(standardization),
                                   missingValuePolicy:=ParseClusterMissingValuePolicy(missingValuePolicy))
                fit.Fit()

                Dim handleKey As String = "HCLUST:" & Guid.NewGuid().ToString("N")
                Dim info As New HierarchicalHandle With {
                    .Handle = handleKey,
                    .Model = fit,
                    .VariableNames = CloneStringArray(imported.varNames),
                    .RowLabels = CloneStringArray(labels),
                    .Linkage = linkageChoice.ToString(),
                    .DistanceMetric = distanceChoice.ToString(),
                    .MinkowskiPower = GetOptionalDouble(minkowskiPower, 2.0R),
                    .Standardization = ParseClusterStandardizationMode(standardization).ToString(),
                    .MissingValuePolicy = ParseClusterMissingValuePolicy(missingValuePolicy).ToString()
                }
                _hclustCache(handleKey) = info
                Return handleKey
            Catch ex As Exception
                Return LoggedUdfExceptionText("BESH.MULTI.HCLUST_FIT", ex)
            End Try
        End Function

        ''' <summary>
        ''' Returns a compact settings and fit summary for a fitted hierarchical clustering model.
        ''' </summary>
        ''' <param name="handle">Handle returned by <c>BESH.MULTI.HCLUST_FIT</c>.</param>
        ''' <param name="includeHeader">TRUE to include a header row. Default TRUE.</param>
        ''' <returns>
        ''' A spilled two-column table listing the linkage rule, distance metric, preprocessing choices, active and removed observation counts,
        ''' total merge steps, and the final merge height of the fitted tree.
        ''' </returns>
        <ExcelFunction(
            Name:="BESH.MULTI.HCLUST_SUMMARY",
            Category:="BESHStatNG - Multivariate Analysis",
            Description:="Returns a compact settings and fit summary for a fitted hierarchical clustering model.",
            HelpTopic:=HelpLinks.FallbackBaseUrl & "/udf/multivariate-analysis/"
        )>
        Public Function HCLUST_SUMMARY(
            <ExcelArgument(Name:="handle", Description:="Handle returned by BESH.MULTI.HCLUST_FIT.")> handle As Object,
            <ExcelArgument(Name:="includeHeader", Description:="TRUE to include a header row (default TRUE).")> Optional includeHeader As Object = Nothing
        ) As Object
            Try
                Dim h As HierarchicalHandle = Nothing
                If Not TryGetHierarchicalHandle(handle, h) Then Return ExcelError.ExcelErrorNA

                Dim mergeSteps As Integer = If(h.Model.Result.MergeHeights Is Nothing, 0, h.Model.Result.MergeHeights.Length)
                Dim finalHeight As Object = If(mergeSteps > 0, CType(h.Model.Result.MergeHeights(mergeSteps - 1), Object), CType(Nothing, Object))

                Dim hdr As Boolean = GetOptionalBool(includeHeader, True)
                Dim out(If(hdr, 11, 10), 1) As Object
                Dim r0 As Integer = 0
                If hdr Then
                    out(0, 0) = "Setting"
                    out(0, 1) = "Value"
                    r0 = 1
                End If

                out(r0 + 0, 0) = "Linkage"
                out(r0 + 0, 1) = h.Linkage
                out(r0 + 1, 0) = "Distance metric"
                out(r0 + 1, 1) = h.DistanceMetric
                out(r0 + 2, 0) = "Minkowski power"
                out(r0 + 2, 1) = h.MinkowskiPower
                out(r0 + 3, 0) = "Standardization"
                out(r0 + 3, 1) = h.Standardization
                out(r0 + 4, 0) = "Missing-value policy"
                out(r0 + 4, 1) = h.MissingValuePolicy
                out(r0 + 5, 0) = "Rows analyzed"
                out(r0 + 5, 1) = If(h.Model.Result.ActiveRowIndices Is Nothing, 0, h.Model.Result.ActiveRowIndices.Length)
                out(r0 + 6, 0) = "Rows removed"
                out(r0 + 6, 1) = If(h.Model.Result.RemovedRowIndices Is Nothing, 0, h.Model.Result.RemovedRowIndices.Length)
                out(r0 + 7, 0) = "Variables"
                out(r0 + 7, 1) = h.VariableNames.Length
                out(r0 + 8, 0) = "Merge steps"
                out(r0 + 8, 1) = mergeSteps
                out(r0 + 9, 0) = "Leaf count"
                out(r0 + 9, 1) = If(h.Model.Result.ActiveRowIndices Is Nothing, 0, h.Model.Result.ActiveRowIndices.Length)
                out(r0 + 10, 0) = "Final merge height"
                out(r0 + 10, 1) = finalHeight

                Return PrepareResultTableForUdf(out)
            Catch ex As Exception
                Return LoggedUdfExceptionText("BESH.MULTI.HCLUST_SUMMARY", ex)
            End Try
        End Function

        ''' <summary>
        ''' Returns the agglomeration schedule for a fitted hierarchical clustering model.
        ''' </summary>
        ''' <param name="handle">Handle returned by <c>BESH.MULTI.HCLUST_FIT</c>.</param>
        ''' <param name="includeHeader">TRUE to include a header row. Default TRUE.</param>
        ''' <returns>
        ''' A spilled table with one row per merge showing the left and right cluster ids merged at each step, the merge height,
        ''' and the size of the newly formed cluster.
        ''' </returns>
        <ExcelFunction(
            Name:="BESH.MULTI.HCLUST_AGGLOM",
            Category:="BESHStatNG - Multivariate Analysis",
            Description:="Returns the agglomeration schedule for a fitted hierarchical clustering model.",
            HelpTopic:=HelpLinks.FallbackBaseUrl & "/udf/multivariate-analysis/"
        )>
        Public Function HCLUST_AGGLOM(
            <ExcelArgument(Name:="handle", Description:="Handle returned by BESH.MULTI.HCLUST_FIT.")> handle As Object,
            <ExcelArgument(Name:="includeHeader", Description:="TRUE to include a header row (default TRUE).")> Optional includeHeader As Object = Nothing
        ) As Object
            Try
                Dim h As HierarchicalHandle = Nothing
                If Not TryGetHierarchicalHandle(handle, h) Then Return ExcelError.ExcelErrorNA
                Return PrepareExistingObjectTableForUdf(h.Model.Result.GetAgglomerationSchedule(), GetOptionalBool(includeHeader, True))
            Catch ex As Exception
                Return LoggedUdfExceptionText("BESH.MULTI.HCLUST_AGGLOM", ex)
            End Try
        End Function

        ''' <summary>
        ''' Returns the leaf order used to display the fitted hierarchical tree.
        ''' </summary>
        ''' <param name="handle">Handle returned by <c>BESH.MULTI.HCLUST_FIT</c>.</param>
        ''' <param name="includeHeader">TRUE to include a header row. Default TRUE.</param>
        ''' <returns>
        ''' A spilled table giving the display order from left to right in the dendrogram together with the original row numbers and row labels.
        ''' </returns>
        <ExcelFunction(
            Name:="BESH.MULTI.HCLUST_LEAFORDER",
            Category:="BESHStatNG - Multivariate Analysis",
            Description:="Returns the leaf order used to display the fitted hierarchical tree.",
            HelpTopic:=HelpLinks.FallbackBaseUrl & "/udf/multivariate-analysis/"
        )>
        Public Function HCLUST_LEAFORDER(
            <ExcelArgument(Name:="handle", Description:="Handle returned by BESH.MULTI.HCLUST_FIT.")> handle As Object,
            <ExcelArgument(Name:="includeHeader", Description:="TRUE to include a header row (default TRUE).")> Optional includeHeader As Object = Nothing
        ) As Object
            Try
                Dim h As HierarchicalHandle = Nothing
                If Not TryGetHierarchicalHandle(handle, h) Then Return ExcelError.ExcelErrorNA
                Return PrepareExistingObjectTableForUdf(BuildHierarchicalLeafOrderTable(h.Model.Result), GetOptionalBool(includeHeader, True))
            Catch ex As Exception
                Return LoggedUdfExceptionText("BESH.MULTI.HCLUST_LEAFORDER", ex)
            End Try
        End Function

        ''' <summary>
        ''' Returns a cluster-membership table obtained by cutting the fitted hierarchical tree.
        ''' </summary>
        ''' <param name="handle">Handle returned by <c>BESH.MULTI.HCLUST_FIT</c>.</param>
        ''' <param name="mode">
        ''' Optional cut mode: <c>"clusters"</c> or <c>"count"</c> to cut the tree to a requested number of clusters, or <c>"height"</c>
        ''' to cut the tree at a merge-height threshold. Default: <c>"clusters"</c>.
        ''' </param>
        ''' <param name="value">
        ''' Optional parameter paired with <paramref name="mode"/>. Supply an integer cluster count when the mode is by clusters,
        ''' or a numeric merge-height threshold when the mode is by height. Default: 3 when mode is by clusters, otherwise 0.
        ''' </param>
        ''' <param name="includeHeader">TRUE to include a header row. Default TRUE.</param>
        ''' <returns>
        ''' A spilled membership table for the active observations. Because the tree is already fitted, you can call this function repeatedly
        ''' with different cut values to compare alternative cluster solutions without refitting the hierarchy.
        ''' </returns>
        <ExcelFunction(
            Name:="BESH.MULTI.HCLUST_MEMBERSHIP",
            Category:="BESHStatNG - Multivariate Analysis",
            Description:="Returns a cluster-membership table obtained by cutting the fitted hierarchical tree.",
            HelpTopic:=HelpLinks.FallbackBaseUrl & "/udf/multivariate-analysis/"
        )>
        Public Function HCLUST_MEMBERSHIP(
            <ExcelArgument(Name:="handle", Description:="Handle returned by BESH.MULTI.HCLUST_FIT.")> handle As Object,
            <ExcelArgument(Name:="mode", Description:="Optional cut mode: clusters/count (default) or height.")> Optional mode As Object = Nothing,
            <ExcelArgument(Name:="value", Description:="Optional cluster count or cut height paired with the selected mode.")> Optional value As Object = Nothing,
            <ExcelArgument(Name:="includeHeader", Description:="TRUE to include a header row (default TRUE).")> Optional includeHeader As Object = Nothing
        ) As Object
            Try
                Dim h As HierarchicalHandle = Nothing
                If Not TryGetHierarchicalHandle(handle, h) Then Return ExcelError.ExcelErrorNA

                Dim cutMode As Multivariate.HierarchicalMembershipDisplayMode = ParseHierarchicalMembershipDisplayMode(mode)
                Dim table As Object(,)
                If cutMode = Multivariate.HierarchicalMembershipDisplayMode.ByHeight Then
                    table = h.Model.Result.GetMembershipTableByHeight(GetOptionalDouble(value, 0.0R))
                Else
                    table = h.Model.Result.GetMembershipTable(GetOptionalInt(value, 3))
                End If

                Return PrepareExistingObjectTableForUdf(table, GetOptionalBool(includeHeader, True))
            Catch ex As Exception
                Return LoggedUdfExceptionText("BESH.MULTI.HCLUST_MEMBERSHIP", ex)
            End Try
        End Function

        ''' <summary>
        ''' Returns the preprocessing constants used by a fitted hierarchical clustering model.
        ''' </summary>
        ''' <param name="handle">Handle returned by <c>BESH.MULTI.HCLUST_FIT</c>.</param>
        ''' <param name="includeHeader">TRUE to include a header row. Default TRUE.</param>
        ''' <returns>
        ''' A spilled table of variable-wise location and scale constants used during preprocessing.
        ''' When no preprocessing was applied, the function returns a compact note table rather than an error.
        ''' </returns>
        <ExcelFunction(
            Name:="BESH.MULTI.HCLUST_PREPROCESS",
            Category:="BESHStatNG - Multivariate Analysis",
            Description:="Returns the preprocessing constants used by a fitted hierarchical clustering model.",
            HelpTopic:=HelpLinks.FallbackBaseUrl & "/udf/multivariate-analysis/"
        )>
        Public Function HCLUST_PREPROCESS(
            <ExcelArgument(Name:="handle", Description:="Handle returned by BESH.MULTI.HCLUST_FIT.")> handle As Object,
            <ExcelArgument(Name:="includeHeader", Description:="TRUE to include a header row (default TRUE).")> Optional includeHeader As Object = Nothing
        ) As Object
            Try
                Dim h As HierarchicalHandle = Nothing
                If Not TryGetHierarchicalHandle(handle, h) Then Return ExcelError.ExcelErrorNA
                Return PrepareExistingObjectTableForUdf(BuildClusterPreprocessingTable(h.Model.Result.VariableNames,
                                                                                       h.Model.Result.StandardizationLocations,
                                                                                       h.Model.Result.StandardizationScales,
                                                                                       h.Model.Result.Standardization),
                                                       GetOptionalBool(includeHeader, True))
            Catch ex As Exception
                Return LoggedUdfExceptionText("BESH.MULTI.HCLUST_PREPROCESS", ex)
            End Try
        End Function

        ''' <summary>
        ''' Returns the rows removed by the missing-value policy before hierarchical clustering was fitted.
        ''' </summary>
        ''' <param name="handle">Handle returned by <c>BESH.MULTI.HCLUST_FIT</c>.</param>
        ''' <param name="includeHeader">TRUE to include a header row. Default TRUE.</param>
        ''' <returns>
        ''' A spilled table listing the original row numbers and optional row labels removed before fitting because at least one analysis variable
        ''' was missing or non-finite. When no rows were removed, the function returns a short note table instead of an error.
        ''' </returns>
        <ExcelFunction(
            Name:="BESH.MULTI.HCLUST_REMOVED",
            Category:="BESHStatNG - Multivariate Analysis",
            Description:="Returns the rows removed by the missing-value policy before hierarchical clustering was fitted.",
            HelpTopic:=HelpLinks.FallbackBaseUrl & "/udf/multivariate-analysis/"
        )>
        Public Function HCLUST_REMOVED(
            <ExcelArgument(Name:="handle", Description:="Handle returned by BESH.MULTI.HCLUST_FIT.")> handle As Object,
            <ExcelArgument(Name:="includeHeader", Description:="TRUE to include a header row (default TRUE).")> Optional includeHeader As Object = Nothing
        ) As Object
            Try
                Dim h As HierarchicalHandle = Nothing
                If Not TryGetHierarchicalHandle(handle, h) Then Return ExcelError.ExcelErrorNA
                Return PrepareExistingObjectTableForUdf(BuildRemovedRowsOutput(h.Model.Result.RemovedRowIndices, h.Model.Result.RemovedRowLabels),
                                                       GetOptionalBool(includeHeader, True))
            Catch ex As Exception
                Return LoggedUdfExceptionText("BESH.MULTI.HCLUST_REMOVED", ex)
            End Try
        End Function

        ''' <summary>
        ''' Removes a hierarchical clustering handle from memory.
        ''' </summary>
        ''' <param name="handle">Handle returned by <c>BESH.MULTI.HCLUST_FIT</c>.</param>
        ''' <returns>TRUE when the handle was removed; FALSE when the handle was not found.</returns>
        <ExcelFunction(
            Name:="BESH.MULTI.HCLUST_DROP",
            Category:="BESHStatNG - Multivariate Analysis",
            Description:="Removes a hierarchical clustering handle from memory.",
            HelpTopic:=HelpLinks.FallbackBaseUrl & "/udf/multivariate-analysis/"
        )>
        Public Function HCLUST_DROP(
            <ExcelArgument(Name:="handle", Description:="Handle returned by BESH.MULTI.HCLUST_FIT.")> handle As Object
        ) As Object
            Try
                Dim key As String = AsString(handle)
                If String.IsNullOrWhiteSpace(key) Then Return False
                Dim removed As HierarchicalHandle = Nothing
                Return _hclustCache.TryRemove(key.Trim(), removed)
            Catch ex As Exception
                Return LoggedUdfExceptionText("BESH.MULTI.HCLUST_DROP", ex)
            End Try
        End Function

        ''' <summary>
        ''' Fits a simple correspondence-analysis model to a contingency table and returns a reusable handle.
        ''' </summary>
        ''' <param name="table">
        ''' Numeric contingency table of non-negative counts with row categories in rows and column categories in columns.
        ''' A single top header row containing column labels is detected automatically and skipped when present.
        ''' Embedded row-label columns are not supported in the supplied range; pass <paramref name="rowNames"/> separately when you want row labels.
        ''' </param>
        ''' <param name="rowNames">
        ''' Optional row-category names as a comma-separated list or as a one-row or one-column range.
        ''' When omitted, default labels Row 1, Row 2, … are generated.
        ''' </param>
        ''' <param name="colNames">
        ''' Optional column-category names as a comma-separated list or as a one-row or one-column range.
        ''' When omitted, names are taken from a detected header row when available; otherwise default labels Col 1, Col 2, … are generated.
        ''' </param>
        ''' <returns>
        ''' A text handle for the fitted correspondence-analysis solution. Pass the handle to the other <c>CA_*</c> worksheet functions
        ''' to retrieve inertia summaries, row and column overview tables, coordinates, cos² tables, and contribution tables.
        ''' </returns>
        ''' <remarks>
        ''' <para>
        ''' Correspondence analysis decomposes the departure from independence in a contingency table into orthogonal latent axes.
        ''' It is especially useful when the Pearson chi-square test shows association but you also want to understand which row and column
        ''' categories drive that association and how categories are arranged in a low-dimensional map.
        ''' </para>
        ''' <para>
        ''' If <c>N</c> is the contingency table, the analysis is based on the matrix of standardized residuals
        ''' <c>S = D_r^(-1/2) (P - r cᵀ) D_c^(-1/2)</c>, where <c>P = N / n</c>, <c>r</c> and <c>c</c> are row and column masses,
        ''' and <c>D_r</c> and <c>D_c</c> are diagonal mass matrices. The singular values of <c>S</c> produce the principal inertias (eigenvalues),
        ''' while the left and right singular vectors produce row and column principal coordinates.
        ''' </para>
        ''' <para>
        ''' Axis signs are arbitrary. If the same table is compared with another software package, the coordinates can differ only by a sign reversal,
        ''' while distances, inertias, cos² values, and contributions remain unchanged.
        ''' </para>
        ''' </remarks>
        ''' <example>
        ''' <code>
        ''' =BESH.MULTI.CA_FIT(A1:D5)
        ''' =BESH.MULTI.CA_FIT(A1:D5,{"Low";"Medium";"High"},{"Control","Treatment A","Treatment B"})
        ''' </code>
        ''' </example>
        <ExcelFunction(
            Name:="BESH.MULTI.CA_FIT",
            Category:="BESHStatNG - Multivariate Analysis",
            Description:="Fits a simple correspondence-analysis model to a contingency table and returns a reusable handle.",
            HelpTopic:=HelpLinks.FallbackBaseUrl & "/udf/multivariate-analysis/"
        )>
        Public Function CA_FIT(
            <ExcelArgument(AllowReference:=True, Name:="table", Description:="Numeric contingency table of non-negative counts. A single top header row may be included.")> table As Object,
            <ExcelArgument(Name:="rowNames", Description:="Optional row-category names as a comma-separated list or a one-row/one-column range.")> Optional rowNames As Object = Nothing,
            <ExcelArgument(Name:="colNames", Description:="Optional column-category names as a comma-separated list or a one-row/one-column range.")> Optional colNames As Object = Nothing
        ) As Object

            If ExcelDnaUtil.IsInFunctionWizard() Then Return "CA_FIT (editing...)"

            Try
                Dim counts(,) As Integer = Nothing
                Dim rows() As String = Nothing
                Dim cols() As String = Nothing
                If Not Global.BESHStatNG.UdfDataImport.TryGetCorrespondenceInput(table, rowNames, colNames, counts, rows, cols) Then Return ExcelError.ExcelErrorValue
                If counts.GetLength(0) < 2 OrElse counts.GetLength(1) < 2 Then Return ExcelError.ExcelErrorNum

                Dim fit As New Multivariate.CA()
                fit.data(counts, rows, cols)
                fit.Calculate()

                Dim handleKey As String = "CA:" & Guid.NewGuid().ToString("N")
                Dim info As New CorrespondenceHandle With {
                    .Handle = handleKey,
                    .Model = fit,
                    .RowNames = CloneStringArray(rows),
                    .ColumnNames = CloneStringArray(cols)
                }
                _caCache(handleKey) = info
                Return handleKey
            Catch ex As Exception
                Return LoggedUdfExceptionText("BESH.MULTI.CA_FIT", ex)
            End Try
        End Function

        ''' <summary>
        ''' Returns a compact settings summary for a fitted correspondence-analysis model.
        ''' </summary>
        ''' <param name="handle">Handle returned by <c>BESH.MULTI.CA_FIT</c>.</param>
        ''' <param name="includeHeader">TRUE to include a header row. Default TRUE.</param>
        ''' <returns>
        ''' A spilled two-column table listing the analyzed table dimensions, the number of row and column categories,
        ''' the number of available axes, and the total inertia of the fitted correspondence-analysis solution.
        ''' </returns>
        <ExcelFunction(
            Name:="BESH.MULTI.CA_SUMMARY",
            Category:="BESHStatNG - Multivariate Analysis",
            Description:="Returns a compact settings summary for a fitted correspondence-analysis model.",
            HelpTopic:=HelpLinks.FallbackBaseUrl & "/udf/multivariate-analysis/"
        )>
        Public Function CA_SUMMARY(
            <ExcelArgument(Name:="handle", Description:="Handle returned by BESH.MULTI.CA_FIT.")> handle As Object,
            <ExcelArgument(Name:="includeHeader", Description:="TRUE to include a header row (default TRUE).")> Optional includeHeader As Object = Nothing
        ) As Object
            Try
                Dim h As CorrespondenceHandle = Nothing
                If Not TryGetCaHandle(handle, h) Then Return ExcelError.ExcelErrorNA

                Dim eigen() As Double = h.Model.Eigenvalues
                Dim hdr As Boolean = GetOptionalBool(includeHeader, True)
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
                out(r0 + 1, 1) = h.Model.rowNames.Length
                out(r0 + 2, 0) = "Column categories"
                out(r0 + 2, 1) = h.Model.ColumNames.Length
                out(r0 + 3, 0) = "Available axes"
                out(r0 + 3, 1) = If(eigen Is Nothing, 0, eigen.Length)
                out(r0 + 4, 0) = "Total inertia"
                out(r0 + 4, 1) = If(eigen Is Nothing, CType(Nothing, Object), CType(eigen.Sum(), Object))
                out(r0 + 5, 0) = "Interpretation"
                out(r0 + 5, 1) = "Rows and columns are analyzed jointly through chi-square distances."

                Return PrepareResultTableForUdf(out)
            Catch ex As Exception
                Return LoggedUdfExceptionText("BESH.MULTI.CA_SUMMARY", ex)
            End Try
        End Function

        ''' <summary>
        ''' Returns the inertia (eigenvalue) table for a fitted correspondence-analysis model.
        ''' </summary>
        ''' <param name="handle">Handle returned by <c>BESH.MULTI.CA_FIT</c>.</param>
        ''' <param name="includeHeader">TRUE to include a header row. Default TRUE.</param>
        ''' <returns>
        ''' A spilled table with one row per axis showing the principal inertia, percentage inertia,
        ''' and cumulative percentage inertia. Large early axes indicate that most of the association structure
        ''' can be summarized in a low-dimensional map.
        ''' </returns>
        <ExcelFunction(
            Name:="BESH.MULTI.CA_EIGEN",
            Category:="BESHStatNG - Multivariate Analysis",
            Description:="Returns inertia and explained-percentage summaries for a fitted correspondence-analysis model.",
            HelpTopic:=HelpLinks.FallbackBaseUrl & "/udf/multivariate-analysis/"
        )>
        Public Function CA_EIGEN(
            <ExcelArgument(Name:="handle", Description:="Handle returned by BESH.MULTI.CA_FIT.")> handle As Object,
            <ExcelArgument(Name:="includeHeader", Description:="TRUE to include a header row (default TRUE).")> Optional includeHeader As Object = Nothing
        ) As Object
            Try
                Dim h As CorrespondenceHandle = Nothing
                If Not TryGetCaHandle(handle, h) Then Return ExcelError.ExcelErrorNA
                Return BuildCaEigenOutput(h.Model, GetOptionalBool(includeHeader, True))
            Catch ex As Exception
                Return LoggedUdfExceptionText("BESH.MULTI.CA_EIGEN", ex)
            End Try
        End Function

        ''' <summary>
        ''' Returns row-category overview statistics for a fitted correspondence-analysis model.
        ''' </summary>
        ''' <param name="handle">Handle returned by <c>BESH.MULTI.CA_FIT</c>.</param>
        ''' <param name="includeHeader">TRUE to include a header row. Default TRUE.</param>
        ''' <returns>
        ''' A spilled table with one row per row category showing quality of representation, mass, chi-square distance,
        ''' and inertia. Mass is the row marginal proportion. Distance measures how far the row profile is from the average profile.
        ''' Inertia combines mass and distance, so rare but very unusual categories and common moderately unusual categories can both be influential.
        ''' </returns>
        <ExcelFunction(
            Name:="BESH.MULTI.CA_ROWS",
            Category:="BESHStatNG - Multivariate Analysis",
            Description:="Returns row-category overview statistics for a fitted correspondence-analysis model.",
            HelpTopic:=HelpLinks.FallbackBaseUrl & "/udf/multivariate-analysis/"
        )>
        Public Function CA_ROWS(
            <ExcelArgument(Name:="handle", Description:="Handle returned by BESH.MULTI.CA_FIT.")> handle As Object,
            <ExcelArgument(Name:="includeHeader", Description:="TRUE to include a header row (default TRUE).")> Optional includeHeader As Object = Nothing
        ) As Object
            Try
                Dim h As CorrespondenceHandle = Nothing
                If Not TryGetCaHandle(handle, h) Then Return ExcelError.ExcelErrorNA
                Return BuildCaOverviewOutput("Row", h.Model.rowNames, h.Model.RowQuality, h.Model.RowMass, h.Model.RowDistance, h.Model.RowInertia, GetOptionalBool(includeHeader, True))
            Catch ex As Exception
                Return LoggedUdfExceptionText("BESH.MULTI.CA_ROWS", ex)
            End Try
        End Function

        ''' <summary>
        ''' Returns the row principal-coordinate matrix for a fitted correspondence-analysis model.
        ''' </summary>
        ''' <param name="handle">Handle returned by <c>BESH.MULTI.CA_FIT</c>.</param>
        ''' <param name="includeHeader">TRUE to include a header row. Default TRUE.</param>
        ''' <returns>
        ''' A spilled matrix with one row per row category and one column per available axis.
        ''' These principal coordinates are the row points typically shown on a correspondence-analysis map.
        ''' Categories with similar coordinates have similar conditional profiles across the table columns.
        ''' </returns>
        <ExcelFunction(
            Name:="BESH.MULTI.CA_ROW_COORD",
            Category:="BESHStatNG - Multivariate Analysis",
            Description:="Returns the row principal-coordinate matrix for a fitted correspondence-analysis model.",
            HelpTopic:=HelpLinks.FallbackBaseUrl & "/udf/multivariate-analysis/"
        )>
        Public Function CA_ROW_COORD(
            <ExcelArgument(Name:="handle", Description:="Handle returned by BESH.MULTI.CA_FIT.")> handle As Object,
            <ExcelArgument(Name:="includeHeader", Description:="TRUE to include a header row (default TRUE).")> Optional includeHeader As Object = Nothing
        ) As Object
            Try
                Dim h As CorrespondenceHandle = Nothing
                If Not TryGetCaHandle(handle, h) Then Return ExcelError.ExcelErrorNA
                Return BuildCaAxisMetricOutput("Row", h.Model.rowNames, "Dim ", h.Model.Eigenvalues.Length, Function(axis As Integer) h.Model.RowFactors(axis), GetOptionalBool(includeHeader, True))
            Catch ex As Exception
                Return LoggedUdfExceptionText("BESH.MULTI.CA_ROW_COORD", ex)
            End Try
        End Function

        ''' <summary>
        ''' Returns row cos² values for each axis of a fitted correspondence-analysis model.
        ''' </summary>
        ''' <param name="handle">Handle returned by <c>BESH.MULTI.CA_FIT</c>.</param>
        ''' <param name="includeHeader">TRUE to include a header row. Default TRUE.</param>
        ''' <returns>
        ''' A spilled matrix of row cos² values (squared cosines) by available axis.
        ''' Cos² values measure how well an axis represents a row category. Large values indicate that the row lies mainly along that axis.
        ''' </returns>
        <ExcelFunction(
            Name:="BESH.MULTI.CA_ROW_COS2",
            Category:="BESHStatNG - Multivariate Analysis",
            Description:="Returns row cos² values for each axis of a fitted correspondence-analysis model.",
            HelpTopic:=HelpLinks.FallbackBaseUrl & "/udf/multivariate-analysis/"
        )>
        Public Function CA_ROW_COS2(
            <ExcelArgument(Name:="handle", Description:="Handle returned by BESH.MULTI.CA_FIT.")> handle As Object,
            <ExcelArgument(Name:="includeHeader", Description:="TRUE to include a header row (default TRUE).")> Optional includeHeader As Object = Nothing
        ) As Object
            Try
                Dim h As CorrespondenceHandle = Nothing
                If Not TryGetCaHandle(handle, h) Then Return ExcelError.ExcelErrorNA
                Return BuildCaAxisMetricOutput("Row", h.Model.rowNames, "Dim ", h.Model.Eigenvalues.Length, Function(axis As Integer) h.Model.RowCorr(axis), GetOptionalBool(includeHeader, True))
            Catch ex As Exception
                Return LoggedUdfExceptionText("BESH.MULTI.CA_ROW_COS2", ex)
            End Try
        End Function

        ''' <summary>
        ''' Returns row contributions for each axis of a fitted correspondence-analysis model.
        ''' </summary>
        ''' <param name="handle">Handle returned by <c>BESH.MULTI.CA_FIT</c>.</param>
        ''' <param name="includeHeader">TRUE to include a header row. Default TRUE.</param>
        ''' <returns>
        ''' A spilled matrix of row contributions by available axis.
        ''' Contributions identify which row categories define each axis. High-contribution rows help anchor the interpretation of that dimension.
        ''' </returns>
        <ExcelFunction(
            Name:="BESH.MULTI.CA_ROW_CONTRIB",
            Category:="BESHStatNG - Multivariate Analysis",
            Description:="Returns row contributions for each axis of a fitted correspondence-analysis model.",
            HelpTopic:=HelpLinks.FallbackBaseUrl & "/udf/multivariate-analysis/"
        )>
        Public Function CA_ROW_CONTRIB(
            <ExcelArgument(Name:="handle", Description:="Handle returned by BESH.MULTI.CA_FIT.")> handle As Object,
            <ExcelArgument(Name:="includeHeader", Description:="TRUE to include a header row (default TRUE).")> Optional includeHeader As Object = Nothing
        ) As Object
            Try
                Dim h As CorrespondenceHandle = Nothing
                If Not TryGetCaHandle(handle, h) Then Return ExcelError.ExcelErrorNA
                Return BuildCaAxisMetricOutput("Row", h.Model.rowNames, "Dim ", h.Model.Eigenvalues.Length, Function(axis As Integer) h.Model.RowContribution(axis), GetOptionalBool(includeHeader, True))
            Catch ex As Exception
                Return LoggedUdfExceptionText("BESH.MULTI.CA_ROW_CONTRIB", ex)
            End Try
        End Function

        ''' <summary>
        ''' Returns column-category overview statistics for a fitted correspondence-analysis model.
        ''' </summary>
        ''' <param name="handle">Handle returned by <c>BESH.MULTI.CA_FIT</c>.</param>
        ''' <param name="includeHeader">TRUE to include a header row. Default TRUE.</param>
        ''' <returns>
        ''' A spilled table with one row per column category showing quality of representation, mass, chi-square distance,
        ''' and inertia. This output is the column-side counterpart of <c>BESH.MULTI.CA_ROWS</c>.
        ''' </returns>
        <ExcelFunction(
            Name:="BESH.MULTI.CA_COLUMNS",
            Category:="BESHStatNG - Multivariate Analysis",
            Description:="Returns column-category overview statistics for a fitted correspondence-analysis model.",
            HelpTopic:=HelpLinks.FallbackBaseUrl & "/udf/multivariate-analysis/"
        )>
        Public Function CA_COLUMNS(
            <ExcelArgument(Name:="handle", Description:="Handle returned by BESH.MULTI.CA_FIT.")> handle As Object,
            <ExcelArgument(Name:="includeHeader", Description:="TRUE to include a header row (default TRUE).")> Optional includeHeader As Object = Nothing
        ) As Object
            Try
                Dim h As CorrespondenceHandle = Nothing
                If Not TryGetCaHandle(handle, h) Then Return ExcelError.ExcelErrorNA
                Return BuildCaOverviewOutput("Column", h.Model.ColumNames, h.Model.ColQuality, h.Model.ColMass, h.Model.ColDistance, h.Model.ColInertia, GetOptionalBool(includeHeader, True))
            Catch ex As Exception
                Return LoggedUdfExceptionText("BESH.MULTI.CA_COLUMNS", ex)
            End Try
        End Function

        ''' <summary>
        ''' Returns the column principal-coordinate matrix for a fitted correspondence-analysis model.
        ''' </summary>
        ''' <param name="handle">Handle returned by <c>BESH.MULTI.CA_FIT</c>.</param>
        ''' <param name="includeHeader">TRUE to include a header row. Default TRUE.</param>
        ''' <returns>
        ''' A spilled matrix with one row per column category and one column per available axis.
        ''' These are the column points on the correspondence-analysis map.
        ''' </returns>
        <ExcelFunction(
            Name:="BESH.MULTI.CA_COL_COORD",
            Category:="BESHStatNG - Multivariate Analysis",
            Description:="Returns the column principal-coordinate matrix for a fitted correspondence-analysis model.",
            HelpTopic:=HelpLinks.FallbackBaseUrl & "/udf/multivariate-analysis/"
        )>
        Public Function CA_COL_COORD(
            <ExcelArgument(Name:="handle", Description:="Handle returned by BESH.MULTI.CA_FIT.")> handle As Object,
            <ExcelArgument(Name:="includeHeader", Description:="TRUE to include a header row (default TRUE).")> Optional includeHeader As Object = Nothing
        ) As Object
            Try
                Dim h As CorrespondenceHandle = Nothing
                If Not TryGetCaHandle(handle, h) Then Return ExcelError.ExcelErrorNA
                Return BuildCaAxisMetricOutput("Column", h.Model.ColumNames, "Dim ", h.Model.Eigenvalues.Length, Function(axis As Integer) h.Model.ColFactors(axis), GetOptionalBool(includeHeader, True))
            Catch ex As Exception
                Return LoggedUdfExceptionText("BESH.MULTI.CA_COL_COORD", ex)
            End Try
        End Function

        ''' <summary>
        ''' Returns column cos² values for each axis of a fitted correspondence-analysis model.
        ''' </summary>
        ''' <param name="handle">Handle returned by <c>BESH.MULTI.CA_FIT</c>.</param>
        ''' <param name="includeHeader">TRUE to include a header row. Default TRUE.</param>
        ''' <returns>
        ''' A spilled matrix of column cos² values (squared cosines) by available axis.
        ''' </returns>
        <ExcelFunction(
                Name:="BESH.MULTI.CA_COL_COS2",
                Category:="BESHStatNG - Multivariate Analysis",
                Description:="Returns column cos² values for each axis of a fitted correspondence-analysis model.",
                HelpTopic:=HelpLinks.FallbackBaseUrl & "/udf/multivariate-analysis/"
            )>
        Public Function CA_COL_COS2(
                <ExcelArgument(Name:="handle", Description:="Handle returned by BESH.MULTI.CA_FIT.")> handle As Object,
                <ExcelArgument(Name:="includeHeader", Description:="TRUE to include a header row (default TRUE).")> Optional includeHeader As Object = Nothing
            ) As Object
            Try
                Dim h As CorrespondenceHandle = Nothing
                If Not TryGetCaHandle(handle, h) Then Return ExcelError.ExcelErrorNA
                Return BuildCaAxisMetricOutput("Column", h.Model.ColumNames, "Dim ", h.Model.Eigenvalues.Length, Function(axis As Integer) h.Model.ColCorr(axis), GetOptionalBool(includeHeader, True))
            Catch ex As Exception
                Return LoggedUdfExceptionText("BESH.MULTI.CA_COL_COS2", ex)
            End Try
        End Function

        ''' <summary>
        ''' Returns column contributions for each axis of a fitted correspondence-analysis model.
        ''' </summary>
        ''' <param name="handle">Handle returned by <c>BESH.MULTI.CA_FIT</c>.</param>
        ''' <param name="includeHeader">TRUE to include a header row. Default TRUE.</param>
        ''' <returns>
        ''' A spilled matrix of column contributions by available axis.
        ''' Contributions identify which column categories define each axis.
        ''' </returns>
        <ExcelFunction(
                Name:="BESH.MULTI.CA_COL_CONTRIB",
                Category:="BESHStatNG - Multivariate Analysis",
                Description:="Returns column contributions for each axis of a fitted correspondence-analysis model.",
                HelpTopic:=HelpLinks.FallbackBaseUrl & "/udf/multivariate-analysis/"
            )>
        Public Function CA_COL_CONTRIB(
                <ExcelArgument(Name:="handle", Description:="Handle returned by BESH.MULTI.CA_FIT.")> handle As Object,
                <ExcelArgument(Name:="includeHeader", Description:="TRUE to include a header row (default TRUE).")> Optional includeHeader As Object = Nothing
            ) As Object
            Try
                Dim h As CorrespondenceHandle = Nothing
                If Not TryGetCaHandle(handle, h) Then Return ExcelError.ExcelErrorNA
                Return BuildCaAxisMetricOutput("Column", h.Model.ColumNames, "Dim ", h.Model.Eigenvalues.Length, Function(axis As Integer) h.Model.ColContribution(axis), GetOptionalBool(includeHeader, True))
            Catch ex As Exception
                Return LoggedUdfExceptionText("BESH.MULTI.CA_COL_CONTRIB", ex)
            End Try
        End Function

        ''' <summary>
        ''' Removes a correspondence-analysis handle from memory.
        ''' </summary>
        ''' <param name="handle">Handle returned by <c>BESH.MULTI.CA_FIT</c>.</param>
        ''' <returns>TRUE when the handle was removed; FALSE when the handle was not found.</returns>
        <ExcelFunction(
            Name:="BESH.MULTI.CA_DROP",
            Category:="BESHStatNG - Multivariate Analysis",
            Description:="Removes a correspondence-analysis handle from memory.",
            HelpTopic:=HelpLinks.FallbackBaseUrl & "/udf/multivariate-analysis/"
        )>
        Public Function CA_DROP(
            <ExcelArgument(Name:="handle", Description:="Handle returned by BESH.MULTI.CA_FIT.")> handle As Object
        ) As Object
            Try
                Dim key As String = AsString(handle)
                If String.IsNullOrWhiteSpace(key) Then Return False
                Dim removed As CorrespondenceHandle = Nothing
                Return _caCache.TryRemove(key.Trim(), removed)
            Catch ex As Exception
                Return LoggedUdfExceptionText("BESH.MULTI.CA_DROP", ex)
            End Try
        End Function

        ''' <summary>
        ''' Fits a multiple correspondence-analysis model to a matrix of categorical variables and returns a reusable handle.
        ''' </summary>
        ''' <param name="x">
        ''' Categorical data matrix with one observation per row and one categorical variable per column.
        ''' Cells are converted to trimmed text. Numbers are allowed and are treated as category labels.
        ''' Blank cells are treated as an empty-string category unless you recode them beforehand.
        ''' </param>
        ''' <param name="varNames">
        ''' Optional variable names as a comma-separated list or a one-row or one-column range.
        ''' When omitted, names are taken from the first row when <paramref name="hasHeader"/> is TRUE; otherwise default names Variable 1, Variable 2, … are generated.
        ''' </param>
        ''' <param name="hasHeader">
        ''' Optional flag indicating whether the first row of <paramref name="x"/> contains variable names rather than observations.
        ''' Default: TRUE when <paramref name="varNames"/> is omitted, otherwise FALSE.
        ''' </param>
        ''' <returns>
        ''' A text handle for the fitted MCA solution. Pass the handle to the other <c>MCA_*</c> worksheet functions
        ''' to retrieve eigen summaries, Burt and indicator matrices, category overview tables, coordinates, cos² values, and contributions.
        ''' </returns>
        ''' <remarks>
        ''' <para>
        ''' Multiple correspondence analysis extends simple correspondence analysis from a two-way table to several categorical variables.
        ''' Internally, the method constructs an indicator matrix with one binary column per category level across all variables, and it also forms
        ''' the Burt table containing all pairwise cross-tabulations between category levels.
        ''' </para>
        ''' <para>
        ''' MCA is useful when you want to explore association structure in survey-like data, questionnaire items, or collections of coded categorical variables.
        ''' Categories that tend to occur together appear near each other in the coordinate map, while categories with very different association profiles
        ''' are separated along the dominant axes.
        ''' </para>
        ''' <para>
        ''' Because the implementation treats raw cell text as categories, it is usually best to clean spelling and capitalization first.
        ''' For example, <c>"yes"</c>, <c>"Yes"</c>, and <c>"YES"</c> are different categories unless standardized before fitting.
        ''' </para>
        ''' </remarks>
        ''' <example>
        ''' <code>
        ''' =BESH.MULTI.MCA_FIT(A1:F101)
        ''' =BESH.MULTI.MCA_FIT(A2:F101,{"Sex","Smoke","Diet","Exercise","Region","Outcome"},FALSE)
        ''' </code>
        ''' </example>
        <ExcelFunction(
            Name:="BESH.MULTI.MCA_FIT",
            Category:="BESHStatNG - Multivariate Analysis",
            Description:="Fits a multiple correspondence-analysis model to a categorical data matrix and returns a reusable handle.",
            HelpTopic:=HelpLinks.FallbackBaseUrl & "/udf/multivariate-analysis/"
        )>
        Public Function MCA_FIT(
            <ExcelArgument(AllowReference:=True, Name:="x", Description:="Categorical data matrix with observations in rows and variables in columns.")> x As Object,
            <ExcelArgument(Name:="varNames", Description:="Optional variable names as a comma-separated list or a one-row/one-column range.")> Optional varNames As Object = Nothing,
            <ExcelArgument(Name:="hasHeader", Description:="Optional TRUE/FALSE flag indicating whether the first row contains variable names.")> Optional hasHeader As Object = Nothing
        ) As Object

            If ExcelDnaUtil.IsInFunctionWizard() Then Return "MCA_FIT (editing...)"

            Try
                Dim raw(,) As String = Nothing
                Dim names() As String = Nothing
                If Not Global.BESHStatNG.UdfDataImport.TryGetCategoricalMatrix(x, varNames, hasHeader, raw, names) Then Return ExcelError.ExcelErrorValue
                If raw.GetLength(0) < 1 OrElse raw.GetLength(1) < 2 Then Return ExcelError.ExcelErrorNum

                Dim fit As New Multivariate.CA()
                fit.DataMultiple(raw, names)
                fit.Calculate()

                Dim handleKey As String = "MCA:" & Guid.NewGuid().ToString("N")
                Dim info As New MultipleCorrespondenceHandle With {
                    .Handle = handleKey,
                    .Model = fit,
                    .VariableNames = CloneStringArray(names),
                    .ObservationCount = raw.GetLength(0)
                }
                _mcaCache(handleKey) = info
                Return handleKey
            Catch ex As Exception
                Return LoggedUdfExceptionText("BESH.MULTI.MCA_FIT", ex)
            End Try
        End Function

        ''' <summary>
        ''' Returns a compact settings summary for a fitted multiple correspondence-analysis model.
        ''' </summary>
        ''' <param name="handle">Handle returned by <c>BESH.MULTI.MCA_FIT</c>.</param>
        ''' <param name="includeHeader">TRUE to include a header row. Default TRUE.</param>
        ''' <returns>
        ''' A spilled two-column table listing the number of observations, variables, total category levels,
        ''' available axes, and total inertia of the fitted MCA solution.
        ''' </returns>
        <ExcelFunction(
            Name:="BESH.MULTI.MCA_SUMMARY",
            Category:="BESHStatNG - Multivariate Analysis",
            Description:="Returns a compact settings summary for a fitted multiple correspondence-analysis model.",
            HelpTopic:=HelpLinks.FallbackBaseUrl & "/udf/multivariate-analysis/"
        )>
        Public Function MCA_SUMMARY(
            <ExcelArgument(Name:="handle", Description:="Handle returned by BESH.MULTI.MCA_FIT.")> handle As Object,
            <ExcelArgument(Name:="includeHeader", Description:="TRUE to include a header row (default TRUE).")> Optional includeHeader As Object = Nothing
        ) As Object
            Try
                Dim h As MultipleCorrespondenceHandle = Nothing
                If Not TryGetMcaHandle(handle, h) Then Return ExcelError.ExcelErrorNA

                Dim eigen() As Double = h.Model.Eigenvalues
                Dim hdr As Boolean = GetOptionalBool(includeHeader, True)
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
                out(r0 + 1, 1) = h.ObservationCount
                out(r0 + 2, 0) = "Variables"
                out(r0 + 2, 1) = h.VariableNames.Length
                out(r0 + 3, 0) = "Category levels"
                out(r0 + 3, 1) = h.Model.ColumNames.Length
                out(r0 + 4, 0) = "Available axes"
                out(r0 + 4, 1) = If(eigen Is Nothing, 0, eigen.Length)
                out(r0 + 5, 0) = "Total inertia"
                out(r0 + 5, 1) = If(eigen Is Nothing, CType(Nothing, Object), CType(eigen.Sum(), Object))

                Return PrepareResultTableForUdf(out)
            Catch ex As Exception
                Return LoggedUdfExceptionText("BESH.MULTI.MCA_SUMMARY", ex)
            End Try
        End Function

        ''' <summary>
        ''' Returns the inertia (eigenvalue) table for a fitted multiple correspondence-analysis model.
        ''' </summary>
        ''' <param name="handle">Handle returned by <c>BESH.MULTI.MCA_FIT</c>.</param>
        ''' <param name="includeHeader">TRUE to include a header row. Default TRUE.</param>
        ''' <returns>
        ''' A spilled table with one row per axis showing principal inertia, percentage inertia, and cumulative percentage inertia.
        ''' </returns>
        <ExcelFunction(
            Name:="BESH.MULTI.MCA_EIGEN",
            Category:="BESHStatNG - Multivariate Analysis",
            Description:="Returns inertia and explained-percentage summaries for a fitted multiple correspondence-analysis model.",
            HelpTopic:=HelpLinks.FallbackBaseUrl & "/udf/multivariate-analysis/"
        )>
        Public Function MCA_EIGEN(
            <ExcelArgument(Name:="handle", Description:="Handle returned by BESH.MULTI.MCA_FIT.")> handle As Object,
            <ExcelArgument(Name:="includeHeader", Description:="TRUE to include a header row (default TRUE).")> Optional includeHeader As Object = Nothing
        ) As Object
            Try
                Dim h As MultipleCorrespondenceHandle = Nothing
                If Not TryGetMcaHandle(handle, h) Then Return ExcelError.ExcelErrorNA
                Return BuildCaEigenOutput(h.Model, GetOptionalBool(includeHeader, True))
            Catch ex As Exception
                Return LoggedUdfExceptionText("BESH.MULTI.MCA_EIGEN", ex)
            End Try
        End Function

        ''' <summary>
        ''' Returns the Burt table for a fitted multiple correspondence-analysis model.
        ''' </summary>
        ''' <param name="handle">Handle returned by <c>BESH.MULTI.MCA_FIT</c>.</param>
        ''' <param name="includeHeader">TRUE to include a header row. Default TRUE.</param>
        ''' <returns>
        ''' A labeled square matrix containing all pairwise cross-tabulations between category levels.
        ''' Diagonal blocks contain one-variable category counts. Off-diagonal blocks contain contingency tables for pairs of variables.
        ''' </returns>
        <ExcelFunction(
            Name:="BESH.MULTI.MCA_BURT",
            Category:="BESHStatNG - Multivariate Analysis",
            Description:="Returns the Burt table for a fitted multiple correspondence-analysis model.",
            HelpTopic:=HelpLinks.FallbackBaseUrl & "/udf/multivariate-analysis/"
        )>
        Public Function MCA_BURT(
            <ExcelArgument(Name:="handle", Description:="Handle returned by BESH.MULTI.MCA_FIT.")> handle As Object,
            <ExcelArgument(Name:="includeHeader", Description:="TRUE to include a header row (default TRUE).")> Optional includeHeader As Object = Nothing
        ) As Object
            Try
                Dim h As MultipleCorrespondenceHandle = Nothing
                If Not TryGetMcaHandle(handle, h) Then Return ExcelError.ExcelErrorNA
                Return BuildMcaBurtOutput(h.Model, GetOptionalBool(includeHeader, True))
            Catch ex As Exception
                Return LoggedUdfExceptionText("BESH.MULTI.MCA_BURT", ex)
            End Try
        End Function

        ''' <summary>
        ''' Returns the indicator (design) matrix used internally by a fitted multiple correspondence-analysis model.
        ''' </summary>
        ''' <param name="handle">Handle returned by <c>BESH.MULTI.MCA_FIT</c>.</param>
        ''' <param name="includeHeader">TRUE to include a header row. Default TRUE.</param>
        ''' <returns>
        ''' A spilled binary matrix with one row per observation and one column per category level.
        ''' Each row contains one active category for each original variable.
        ''' </returns>
        <ExcelFunction(
            Name:="BESH.MULTI.MCA_INDICATOR",
            Category:="BESHStatNG - Multivariate Analysis",
            Description:="Returns the indicator (design) matrix used internally by a fitted multiple correspondence-analysis model.",
            HelpTopic:=HelpLinks.FallbackBaseUrl & "/udf/multivariate-analysis/"
        )>
        Public Function MCA_INDICATOR(
            <ExcelArgument(Name:="handle", Description:="Handle returned by BESH.MULTI.MCA_FIT.")> handle As Object,
            <ExcelArgument(Name:="includeHeader", Description:="TRUE to include a header row (default TRUE).")> Optional includeHeader As Object = Nothing
        ) As Object
            Try
                Dim h As MultipleCorrespondenceHandle = Nothing
                If Not TryGetMcaHandle(handle, h) Then Return ExcelError.ExcelErrorNA
                Return BuildMcaIndicatorOutput(h.Model, GetOptionalBool(includeHeader, True))
            Catch ex As Exception
                Return LoggedUdfExceptionText("BESH.MULTI.MCA_INDICATOR", ex)
            End Try
        End Function

        ''' <summary>
        ''' Returns category-level overview statistics for a fitted multiple correspondence-analysis model.
        ''' </summary>
        ''' <param name="handle">Handle returned by <c>BESH.MULTI.MCA_FIT</c>.</param>
        ''' <param name="includeHeader">TRUE to include a header row. Default TRUE.</param>
        ''' <returns>
        ''' A spilled table with one row per category level showing the originating variable, the category label,
        ''' quality of representation, mass, chi-square distance, and inertia.
        ''' </returns>
        <ExcelFunction(
            Name:="BESH.MULTI.MCA_CATEGORIES",
            Category:="BESHStatNG - Multivariate Analysis",
            Description:="Returns category-level overview statistics for a fitted multiple correspondence-analysis model.",
            HelpTopic:=HelpLinks.FallbackBaseUrl & "/udf/multivariate-analysis/"
        )>
        Public Function MCA_CATEGORIES(
            <ExcelArgument(Name:="handle", Description:="Handle returned by BESH.MULTI.MCA_FIT.")> handle As Object,
            <ExcelArgument(Name:="includeHeader", Description:="TRUE to include a header row (default TRUE).")> Optional includeHeader As Object = Nothing
        ) As Object
            Try
                Dim h As MultipleCorrespondenceHandle = Nothing
                If Not TryGetMcaHandle(handle, h) Then Return ExcelError.ExcelErrorNA
                Return BuildMcaCategoryOverviewOutput(h.Model.BurtVarNames, h.Model.rowNames, h.Model.ColQuality, h.Model.ColMass, h.Model.ColDistance, h.Model.ColInertia, GetOptionalBool(includeHeader, True))
            Catch ex As Exception
                Return LoggedUdfExceptionText("BESH.MULTI.MCA_CATEGORIES", ex)
            End Try
        End Function

        ''' <summary>
        ''' Returns category principal coordinates for a fitted multiple correspondence-analysis model.
        ''' </summary>
        ''' <param name="handle">Handle returned by <c>BESH.MULTI.MCA_FIT</c>.</param>
        ''' <param name="includeHeader">TRUE to include a header row. Default TRUE.</param>
        ''' <returns>
        ''' A spilled matrix with one row per category and one column per available axis.
        ''' Categories with similar coordinates tend to co-occur across observations.
        ''' </returns>
        <ExcelFunction(
                Name:="BESH.MULTI.MCA_COORD",
                Category:="BESHStatNG - Multivariate Analysis",
                Description:="Returns category principal coordinates for a fitted multiple correspondence-analysis model.",
                HelpTopic:=HelpLinks.FallbackBaseUrl & "/udf/multivariate-analysis/"
            )>
        Public Function MCA_COORD(
                <ExcelArgument(Name:="handle", Description:="Handle returned by BESH.MULTI.MCA_FIT.")> handle As Object,
                <ExcelArgument(Name:="includeHeader", Description:="TRUE to include a header row (default TRUE).")> Optional includeHeader As Object = Nothing
            ) As Object
            Try
                Dim h As MultipleCorrespondenceHandle = Nothing
                If Not TryGetMcaHandle(handle, h) Then Return ExcelError.ExcelErrorNA
                Return BuildMcaCategoryAxisMetricOutput(h.Model.BurtVarNames, h.Model.rowNames, "Dim ", h.Model.Eigenvalues.Length, Function(axis As Integer) h.Model.ColFactors(axis), GetOptionalBool(includeHeader, True))
            Catch ex As Exception
                Return LoggedUdfExceptionText("BESH.MULTI.MCA_COORD", ex)
            End Try
        End Function

        ''' <summary>
        ''' Returns category cos² values for each axis of a fitted multiple correspondence-analysis model.
        ''' </summary>
        ''' <param name="handle">Handle returned by <c>BESH.MULTI.MCA_FIT</c>.</param>
        ''' <param name="includeHeader">TRUE to include a header row. Default TRUE.</param>
        ''' <returns>
        ''' A spilled matrix of category cos² values (squared cosines) by available axis.
        ''' Large values indicate that the category is well represented by that axis.
        ''' </returns>
        <ExcelFunction(
                Name:="BESH.MULTI.MCA_COS2",
                Category:="BESHStatNG - Multivariate Analysis",
                Description:="Returns category cos² values for each axis of a fitted multiple correspondence-analysis model.",
                HelpTopic:=HelpLinks.FallbackBaseUrl & "/udf/multivariate-analysis/"
            )>
        Public Function MCA_COS2(
                <ExcelArgument(Name:="handle", Description:="Handle returned by BESH.MULTI.MCA_FIT.")> handle As Object,
                <ExcelArgument(Name:="includeHeader", Description:="TRUE to include a header row (default TRUE).")> Optional includeHeader As Object = Nothing
            ) As Object
            Try
                Dim h As MultipleCorrespondenceHandle = Nothing
                If Not TryGetMcaHandle(handle, h) Then Return ExcelError.ExcelErrorNA
                Return BuildMcaCategoryAxisMetricOutput(h.Model.BurtVarNames, h.Model.rowNames, "Dim ", h.Model.Eigenvalues.Length, Function(axis As Integer) h.Model.ColCorr(axis), GetOptionalBool(includeHeader, True))
            Catch ex As Exception
                Return LoggedUdfExceptionText("BESH.MULTI.MCA_COS2", ex)
            End Try
        End Function

        ''' <summary>
        ''' Returns category contributions for each axis of a fitted multiple correspondence-analysis model.
        ''' </summary>
        ''' <param name="handle">Handle returned by <c>BESH.MULTI.MCA_FIT</c>.</param>
        ''' <param name="includeHeader">TRUE to include a header row. Default TRUE.</param>
        ''' <returns>
        ''' A spilled matrix of category contributions by available axis.
        ''' High-contribution categories are the ones that primarily define the orientation of each MCA dimension.
        ''' </returns>
        <ExcelFunction(
                Name:="BESH.MULTI.MCA_CONTRIB",
                Category:="BESHStatNG - Multivariate Analysis",
                Description:="Returns category contributions for each axis of a fitted multiple correspondence-analysis model.",
                HelpTopic:=HelpLinks.FallbackBaseUrl & "/udf/multivariate-analysis/"
            )>
        Public Function MCA_CONTRIB(
                <ExcelArgument(Name:="handle", Description:="Handle returned by BESH.MULTI.MCA_FIT.")> handle As Object,
                <ExcelArgument(Name:="includeHeader", Description:="TRUE to include a header row (default TRUE).")> Optional includeHeader As Object = Nothing
            ) As Object
            Try
                Dim h As MultipleCorrespondenceHandle = Nothing
                If Not TryGetMcaHandle(handle, h) Then Return ExcelError.ExcelErrorNA
                Return BuildMcaCategoryAxisMetricOutput(h.Model.BurtVarNames, h.Model.rowNames, "Dim ", h.Model.Eigenvalues.Length, Function(axis As Integer) h.Model.ColContribution(axis), GetOptionalBool(includeHeader, True))
            Catch ex As Exception
                Return LoggedUdfExceptionText("BESH.MULTI.MCA_CONTRIB", ex)
            End Try
        End Function

        ''' <summary>
        ''' Removes a multiple correspondence-analysis handle from memory.
        ''' </summary>
        ''' <param name="handle">Handle returned by <c>BESH.MULTI.MCA_FIT</c>.</param>
        ''' <returns>TRUE when the handle was removed; FALSE when the handle was not found.</returns>
        <ExcelFunction(
            Name:="BESH.MULTI.MCA_DROP",
            Category:="BESHStatNG - Multivariate Analysis",
            Description:="Removes a multiple correspondence-analysis handle from memory.",
            HelpTopic:=HelpLinks.FallbackBaseUrl & "/udf/multivariate-analysis/"
        )>
        Public Function MCA_DROP(
            <ExcelArgument(Name:="handle", Description:="Handle returned by BESH.MULTI.MCA_FIT.")> handle As Object
        ) As Object
            Try
                Dim key As String = AsString(handle)
                If String.IsNullOrWhiteSpace(key) Then Return False
                Dim removed As MultipleCorrespondenceHandle = Nothing
                Return _mcaCache.TryRemove(key.Trim(), removed)
            Catch ex As Exception
                Return LoggedUdfExceptionText("BESH.MULTI.MCA_DROP", ex)
            End Try
        End Function

        ''' <summary>
        ''' Fits a discriminant-analysis classification model and returns a reusable handle.
        ''' </summary>
        ''' <param name="x">
        ''' Numeric predictor matrix with observations in rows and analysis variables in columns.
        ''' A single header row is detected automatically when the first row is nonnumeric and the rows below are numeric.
        ''' Use one row per case and one numeric predictor per column.
        ''' </param>
        ''' <param name="groups">
        ''' One-column grouping variable aligned with <paramref name="x"/>. The grouping variable defines the known classes used to fit the classifier.
        ''' Text or numeric labels are accepted. A single top header cell is detected automatically when the supplied range is a whole-column style reference.
        ''' </param>
        ''' <param name="varNames">
        ''' Optional predictor names supplied either as a comma-separated list or as a one-row or one-column range.
        ''' When omitted, names are taken from the detected header row when available; otherwise default names X1, X2, … are generated.
        ''' </param>
        ''' <param name="rowLabels">
        ''' Optional one-column range of case labels aligned with <paramref name="x"/>.
        ''' These labels are carried into the casewise classification tables and removed-row report. When omitted, generic labels are generated.
        ''' </param>
        ''' <param name="method">
        ''' Optional discriminant method: <c>"linear"</c> (default) or <c>"quadratic"</c>.
        ''' Linear discriminant analysis assumes all groups share one pooled within-group covariance matrix.
        ''' Quadratic discriminant analysis allows each group to have its own covariance matrix and can model curved decision boundaries.
        ''' </param>
        ''' <param name="standardization">
        ''' Optional preprocessing mode: <c>"none"</c> (default), <c>"zscores"</c>, or <c>"range01"</c>.
        ''' Standardization is useful when predictors are measured on very different scales and you do not want variables with larger units to dominate the covariance structure.
        ''' </param>
        ''' <param name="missingValuePolicy">
        ''' Optional missing-data policy: <c>"error"</c> (default) or <c>"listwise"</c>.
        ''' The error policy stops the fit when any case contains a missing or non-finite predictor value.
        ''' The listwise policy removes incomplete rows before the model is estimated.
        ''' </param>
        ''' <param name="priorMode">
        ''' Optional prior-probability mode: <c>"proportional"</c> (default), <c>"equal"</c>, or <c>"user"</c>.
        ''' Priors affect posterior probabilities and therefore the final classification rule, especially when groups are imbalanced or when misclassification costs are conceptually asymmetric.
        ''' </param>
        ''' <param name="priorLabels">
        ''' Optional group labels used only when <paramref name="priorMode"/> is <c>"user"</c>.
        ''' Supply a comma-separated list or a one-row or one-column range with one label per training group.
        ''' </param>
        ''' <param name="priorProbabilities">
        ''' Optional prior probabilities used only when <paramref name="priorMode"/> is <c>"user"</c>.
        ''' Supply a one-row or one-column numeric range, or a comma-separated numeric list, aligned with <paramref name="priorLabels"/>.
        ''' The values are internally normalized to sum to 1.
        ''' </param>
        ''' <param name="covarianceRegularization">
        ''' Optional non-negative ridge constant added to covariance diagonals when inversion is numerically difficult.
        ''' Default: 0.00000001. Increase this slightly when near-singular covariance matrices are expected, for example with highly collinear predictors or very small groups.
        ''' </param>
        ''' <param name="validationMode">
        ''' Optional validation strategy: <c>"none"</c> (default), <c>"leaveoneout"</c>, <c>"kfold"</c>, or <c>"holdout"</c>.
        ''' Validation does not change the fitted final training model; it adds an extra out-of-sample style assessment.
        ''' </param>
        ''' <param name="numberOfFolds">Optional number of folds for k-fold validation. Default 5.</param>
        ''' <param name="holdoutFraction">Optional test-set fraction for holdout validation. Default 0.3.</param>
        ''' <param name="stratified">
        ''' Optional TRUE/FALSE flag controlling whether k-fold and holdout validation preserve the observed group proportions as closely as possible.
        ''' Default TRUE.
        ''' </param>
        ''' <param name="randomSeed">
        ''' Optional deterministic random seed for k-fold or holdout validation.
        ''' Leave blank to use a time-based seed. Supplying a seed improves reproducibility across recalculations.
        ''' </param>
        ''' <returns>
        ''' A text handle for the fitted discriminant-analysis model. Pass the handle to the other <c>DA_*</c> worksheet functions
        ''' to retrieve settings, group summaries, mean tables, covariance matrices, classification tables, canonical summaries, or prediction output.
        ''' </returns>
        ''' <remarks>
        ''' <para>
        ''' Discriminant analysis is a supervised classification method. It starts from observations whose group membership is already known,
        ''' estimates one score function per group, and then classifies each case to the group with the largest posterior support.
        ''' It is often used both for practical prediction and for understanding which groups are well separated in multivariate space.
        ''' </para>
        ''' <para>
        ''' Use the linear method when the groups can reasonably share one within-group covariance matrix and you want a stable, interpretable model.
        ''' Use the quadratic method when group covariance patterns are meaningfully different and you have enough observations in each group to estimate them reliably.
        ''' The quadratic method is more flexible but usually needs more data.
        ''' </para>
        ''' <para>
        ''' When validation is requested, the model also stores a second classification table based on leave-one-out, k-fold, or holdout validation.
        ''' The apparent training table is still available separately because it answers a different question: how well the fitted rule classifies the same data used to estimate it.
        ''' </para>
        ''' </remarks>
        ''' <example>
        ''' <code>
        ''' =BESH.MULTI.DA_FIT(B1:F101,A1:A101)
        ''' =BESH.MULTI.DA_FIT(B1:F101,A1:A101,,G1:G101,"linear","zscores","listwise","equal")
        ''' =BESH.MULTI.DA_FIT(B1:F101,A1:A101,,,"quadratic","none","listwise","user",{"Control";"Case"},{0.4;0.6},1E-06,"kfold",10,,TRUE,12345)
        ''' </code>
        ''' </example>
        <ExcelFunction(
            Name:="BESH.MULTI.DA_FIT",
            Category:="BESHStatNG - Multivariate Analysis",
            Description:="Fits a discriminant-analysis model and returns a reusable handle.",
            HelpTopic:=HelpLinks.FallbackBaseUrl & "/udf/multivariate-analysis/"
        )>
        Public Function DA_FIT(
            <ExcelArgument(AllowReference:=True, Name:="x", Description:="Numeric predictor matrix with observations in rows and variables in columns.")> x As Object,
            <ExcelArgument(AllowReference:=True, Name:="groups", Description:="One-column grouping variable aligned with the predictor matrix.")> groups As Object,
            <ExcelArgument(Name:="varNames", Description:="Optional predictor names as a comma-separated list or a one-row/one-column range.")> Optional varNames As Object = Nothing,
            <ExcelArgument(Name:="rowLabels", Description:="Optional one-column range of case labels aligned with the predictor matrix.")> Optional rowLabels As Object = Nothing,
            <ExcelArgument(Name:="method", Description:="Optional method: linear (default) or quadratic.")> Optional method As Object = Nothing,
            <ExcelArgument(Name:="standardization", Description:="Optional preprocessing: none (default), zscores, or range01.")> Optional standardization As Object = Nothing,
            <ExcelArgument(Name:="missingValuePolicy", Description:="Optional missing-data policy: error (default) or listwise.")> Optional missingValuePolicy As Object = Nothing,
            <ExcelArgument(Name:="priorMode", Description:="Optional prior mode: proportional (default), equal, or user.")> Optional priorMode As Object = Nothing,
            <ExcelArgument(Name:="priorLabels", Description:="Optional user-prior group labels, required only when priorMode=user.")> Optional priorLabels As Object = Nothing,
            <ExcelArgument(Name:="priorProbabilities", Description:="Optional user-prior probabilities, required only when priorMode=user.")> Optional priorProbabilities As Object = Nothing,
            <ExcelArgument(Name:="covarianceRegularization", Description:="Optional non-negative covariance ridge constant. Default 0.00000001.")> Optional covarianceRegularization As Object = Nothing,
            <ExcelArgument(Name:="validationMode", Description:="Optional validation: none (default), leaveoneout, kfold, or holdout.")> Optional validationMode As Object = Nothing,
            <ExcelArgument(Name:="numberOfFolds", Description:="Optional number of folds for k-fold validation. Default 5.")> Optional numberOfFolds As Object = Nothing,
            <ExcelArgument(Name:="holdoutFraction", Description:="Optional test-set fraction for holdout validation. Default 0.3.")> Optional holdoutFraction As Object = Nothing,
            <ExcelArgument(Name:="stratified", Description:="TRUE to preserve group proportions during validation splits where possible. Default TRUE.")> Optional stratified As Object = Nothing,
            <ExcelArgument(Name:="randomSeed", Description:="Optional deterministic random seed for validation splitting.")> Optional randomSeed As Object = Nothing
        ) As Object

            If ExcelDnaUtil.IsInFunctionWizard() Then Return "DA_FIT (editing...)"

            Try
                Dim imported As DataObj = Nothing
                If Not Global.BESHStatNG.UdfDataImport.TryGetNumericData(x, varNames, True, imported) Then Return ExcelError.ExcelErrorValue
                If imported.nRows < 2 OrElse imported.nCols < 1 Then Return ExcelError.ExcelErrorNum

                Dim groupCol(,) As Object = Nothing
                Dim inferredGroupName As String = Nothing
                If Not Global.BESHStatNG.UdfDataImport.TryGetTextColumn(groups, groupCol, inferredGroupName) Then Return ExcelError.ExcelErrorValue
                If groupCol.GetLength(0) <> imported.nRows Then Throw New ArgumentException("groups must contain one value per data row after header detection.")

                Dim labels() As String = Nothing
                If Not TryResolveOptionalClusterRowLabels(rowLabels, imported.nRows, labels) Then Return ExcelError.ExcelErrorValue

                Dim groupValues(imported.nRows - 1) As Object
                For i As Integer = 0 To imported.nRows - 1
                    groupValues(i) = groupCol(i, 0)
                Next

                Dim methodChoice As Multivariate.DiscriminantAnalysisMethod = ParseDiscriminantMethod(method)
                Dim standardizationChoice As Multivariate.ClusterStandardizationMode = ParseClusterStandardizationMode(standardization)
                Dim missingChoice As Multivariate.ClusterMissingValuePolicy = ParseClusterMissingValuePolicy(missingValuePolicy)
                Dim priorChoice As Multivariate.DiscriminantPriorMode = ParseDiscriminantPriorMode(priorMode)
                Dim validationChoice As Multivariate.DiscriminantValidationMode = ParseDiscriminantValidationMode(validationMode)
                Dim requestedSeed As Integer = If(IsMissingArg(randomSeed), Integer.MinValue, GetOptionalInt(randomSeed, Integer.MinValue))

                Dim fit As New Multivariate.DiscriminantAnalysis()
                fit.dataInputs(imported.DataDbl, groupValues, labels, imported.varNames)
                fit.settingsInputs(method:=methodChoice,
                                   standardization:=standardizationChoice,
                                   missingPolicy:=missingChoice,
                                   priorMode:=priorChoice,
                                   covarianceRegularization:=GetOptionalDouble(covarianceRegularization, 0.00000001R))

                If priorChoice = Multivariate.DiscriminantPriorMode.UserSpecified Then
                    Dim parsedLabels() As String = Nothing
                    Dim parsedProbabilities() As Double = Nothing
                    If Not Global.BESHStatNG.UdfDataImport.TryGetStringVector(priorLabels, parsedLabels) Then Throw New ArgumentException("priorLabels must be supplied when priorMode=user.")
                    If Not Global.BESHStatNG.UdfDataImport.TryGetDoubleVector(priorProbabilities, parsedProbabilities) Then Throw New ArgumentException("priorProbabilities must be supplied when priorMode=user.")
                    If parsedLabels.Length <> parsedProbabilities.Length Then Throw New ArgumentException("priorLabels and priorProbabilities must have the same length.")
                    Dim priorLabelObjects(parsedLabels.Length - 1) As Object
                    For i As Integer = 0 To parsedLabels.Length - 1
                        priorLabelObjects(i) = parsedLabels(i)
                    Next
                    fit.priorInputs(priorLabelObjects, parsedProbabilities)
                End If

                fit.validationInputs(mode:=validationChoice,
                                     numberOfFolds:=GetOptionalInt(numberOfFolds, 5),
                                     holdoutFraction:=GetOptionalDouble(holdoutFraction, 0.3R),
                                     randomSeed:=requestedSeed,
                                     stratified:=GetOptionalBool(stratified, True))
                fit.Fit()

                Dim handleKey As String = "DA:" & Guid.NewGuid().ToString("N")
                Dim info As New DiscriminantHandle With {
                    .Handle = handleKey,
                    .Model = fit,
                    .VariableNames = CloneStringArray(imported.varNames),
                    .GroupVariableName = If(String.IsNullOrWhiteSpace(inferredGroupName), "Group", inferredGroupName),
                    .Method = methodChoice.ToString(),
                    .Standardization = standardizationChoice.ToString(),
                    .MissingValuePolicy = missingChoice.ToString(),
                    .PriorMode = priorChoice.ToString(),
                    .ValidationMode = validationChoice.ToString(),
                    .NumberOfFolds = GetOptionalInt(numberOfFolds, 5),
                    .HoldoutFraction = GetOptionalDouble(holdoutFraction, 0.3R),
                    .Stratified = GetOptionalBool(stratified, True),
                    .RequestedRandomSeed = requestedSeed,
                    .CovarianceRegularization = GetOptionalDouble(covarianceRegularization, 0.00000001R)
                }
                _daCache(handleKey) = info
                Return handleKey
            Catch ex As Exception
                Return LoggedUdfExceptionText("BESH.MULTI.DA_FIT", ex)
            End Try
        End Function

        ''' <summary>
        ''' Returns a compact settings and performance summary for a fitted discriminant-analysis model.
        ''' </summary>
        ''' <param name="handle">Handle returned by <c>BESH.MULTI.DA_FIT</c>.</param>
        ''' <param name="includeHeader">TRUE to include a header row. Default TRUE.</param>
        ''' <returns>
        ''' A spilled two-column summary describing the analysis method, preprocessing, priors, validation settings,
        ''' sample size after missing-data handling, number of groups, and apparent and validation classification accuracy when available.
        ''' </returns>
        <ExcelFunction(
            Name:="BESH.MULTI.DA_SUMMARY",
            Category:="BESHStatNG - Multivariate Analysis",
            Description:="Returns a compact settings and performance summary for a fitted discriminant-analysis model.",
            HelpTopic:=HelpLinks.FallbackBaseUrl & "/udf/multivariate-analysis/"
        )>
        Public Function DA_SUMMARY(
            <ExcelArgument(Name:="handle", Description:="Handle returned by BESH.MULTI.DA_FIT.")> handle As Object,
            <ExcelArgument(Name:="includeHeader", Description:="TRUE to include a header row (default TRUE).")> Optional includeHeader As Object = Nothing
        ) As Object
            Try
                Dim h As DiscriminantHandle = Nothing
                If Not TryGetDaHandle(handle, h) Then Return ExcelError.ExcelErrorNA
                Dim model = h.Model
                Dim out(14, 1) As Object
                out(0, 0) = "Setting"
                out(0, 1) = "Value"
                out(1, 0) = "Method"
                out(1, 1) = h.Method
                out(2, 0) = "Grouping variable"
                out(2, 1) = h.GroupVariableName
                out(3, 0) = "Predictors"
                out(3, 1) = h.VariableNames.Length
                out(4, 0) = "Groups"
                out(4, 1) = If(model.GroupLabels Is Nothing, 0, model.GroupLabels.Length)
                out(5, 0) = "Rows analyzed"
                out(5, 1) = If(model.PreparedData Is Nothing OrElse model.PreparedData.ActiveOriginalData Is Nothing, 0, model.PreparedData.ActiveOriginalData.GetLength(0))
                out(6, 0) = "Rows removed"
                out(6, 1) = If(model.PreparedData Is Nothing OrElse model.PreparedData.RemovedOriginalIndices Is Nothing, 0, model.PreparedData.RemovedOriginalIndices.Length)
                out(7, 0) = "Standardization"
                out(7, 1) = h.Standardization
                out(8, 0) = "Missing-value policy"
                out(8, 1) = h.MissingValuePolicy
                out(9, 0) = "Prior mode"
                out(9, 1) = h.PriorMode
                out(10, 0) = "Validation mode"
                out(10, 1) = h.ValidationMode
                out(11, 0) = "Training accuracy %"
                out(11, 1) = If(model.TrainingClassification Is Nothing OrElse model.TrainingClassification.Confusion Is Nothing, CType(Nothing, Object), CType(model.TrainingClassification.Confusion.OverallAccuracyPct, Object))
                out(12, 0) = "Validation accuracy %"
                out(12, 1) = If(model.ValidationClassification Is Nothing OrElse model.ValidationClassification.Confusion Is Nothing, CType(Nothing, Object), CType(model.ValidationClassification.Confusion.OverallAccuracyPct, Object))
                out(13, 0) = "Covariance regularization"
                out(13, 1) = h.CovarianceRegularization
                out(14, 0) = "Validation detail"
                Select Case ParseDiscriminantValidationMode(h.ValidationMode)
                    Case Multivariate.DiscriminantValidationMode.KFold
                        out(14, 1) = h.NumberOfFolds.ToString(CultureInfo.InvariantCulture) & " folds; Stratified=" & h.Stratified.ToString()
                    Case Multivariate.DiscriminantValidationMode.Holdout
                        out(14, 1) = "Test fraction=" & h.HoldoutFraction.ToString(CultureInfo.InvariantCulture) & "; Stratified=" & h.Stratified.ToString()
                    Case Else
                        out(14, 1) = h.ValidationMode
                End Select
                Return PrepareExistingObjectTableForUdf(out, GetOptionalBool(includeHeader, True))
            Catch ex As Exception
                Return LoggedUdfExceptionText("BESH.MULTI.DA_SUMMARY", ex)
            End Try
        End Function

        ''' <summary>
        ''' Returns group counts, prior probabilities, and covariance diagnostics for a fitted discriminant-analysis model.
        ''' </summary>
        ''' <param name="handle">Handle returned by <c>BESH.MULTI.DA_FIT</c>.</param>
        ''' <param name="includeHeader">TRUE to include a header row. Default TRUE.</param>
        ''' <returns>
        ''' A spilled table with one row per group showing the number of training cases retained for that group,
        ''' the prior probability used by the classifier, and the group-specific covariance diagnostics on the working analysis scale.
        ''' </returns>
        <ExcelFunction(
            Name:="BESH.MULTI.DA_GROUPSUMMARY",
            Category:="BESHStatNG - Multivariate Analysis",
            Description:="Returns group counts, prior probabilities, and covariance diagnostics for a fitted discriminant-analysis model.",
            HelpTopic:=HelpLinks.FallbackBaseUrl & "/udf/multivariate-analysis/"
        )>
        Public Function DA_GROUPSUMMARY(
            <ExcelArgument(Name:="handle", Description:="Handle returned by BESH.MULTI.DA_FIT.")> handle As Object,
            <ExcelArgument(Name:="includeHeader", Description:="TRUE to include a header row (default TRUE).")> Optional includeHeader As Object = Nothing
        ) As Object
            Try
                Dim h As DiscriminantHandle = Nothing
                If Not TryGetDaHandle(handle, h) Then Return ExcelError.ExcelErrorNA
                Return PrepareExistingObjectTableForUdf(BuildDiscriminantGroupSummaryTable(h.Model), GetOptionalBool(includeHeader, True))
            Catch ex As Exception
                Return LoggedUdfExceptionText("BESH.MULTI.DA_GROUPSUMMARY", ex)
            End Try
        End Function

        ''' <summary>
        ''' Returns the group mean table for a fitted discriminant-analysis model.
        ''' </summary>
        ''' <param name="handle">Handle returned by <c>BESH.MULTI.DA_FIT</c>.</param>
        ''' <param name="scale">
        ''' Optional output scale: <c>"original"</c> (default) or <c>"working"</c>.
        ''' The original scale reports means in the original measurement units.
        ''' The working scale reports means after any requested standardization and is therefore the scale used by the fitted covariance matrices and classification functions.
        ''' </param>
        ''' <param name="includeHeader">TRUE to include a header row. Default TRUE.</param>
        ''' <returns>A labeled matrix of group means with groups in rows and predictors in columns.</returns>
        <ExcelFunction(
            Name:="BESH.MULTI.DA_MEANS",
            Category:="BESHStatNG - Multivariate Analysis",
            Description:="Returns the group mean table for a fitted discriminant-analysis model.",
            HelpTopic:=HelpLinks.FallbackBaseUrl & "/udf/multivariate-analysis/"
        )>
        Public Function DA_MEANS(
            <ExcelArgument(Name:="handle", Description:="Handle returned by BESH.MULTI.DA_FIT.")> handle As Object,
            <ExcelArgument(Name:="scale", Description:="Optional scale: original (default) or working.")> Optional scale As Object = Nothing,
            <ExcelArgument(Name:="includeHeader", Description:="TRUE to include a header row (default TRUE).")> Optional includeHeader As Object = Nothing
        ) As Object
            Try
                Dim h As DiscriminantHandle = Nothing
                If Not TryGetDaHandle(handle, h) Then Return ExcelError.ExcelErrorNA
                Return PrepareExistingObjectTableForUdf(BuildDiscriminantMeansTable(h.Model, ParseOutputScaleUseOriginal(scale)), GetOptionalBool(includeHeader, True))
            Catch ex As Exception
                Return LoggedUdfExceptionText("BESH.MULTI.DA_MEANS", ex)
            End Try
        End Function

        ''' <summary>
        ''' Returns the preprocessing constants used by a fitted discriminant-analysis model.
        ''' </summary>
        ''' <param name="handle">Handle returned by <c>BESH.MULTI.DA_FIT</c>.</param>
        ''' <param name="includeHeader">TRUE to include a header row. Default TRUE.</param>
        ''' <returns>
        ''' A spilled table of variable-wise location and scale constants used during preprocessing.
        ''' When no preprocessing was applied, the function returns a compact note table rather than an error.
        ''' </returns>
        <ExcelFunction(
            Name:="BESH.MULTI.DA_PREPROCESS",
            Category:="BESHStatNG - Multivariate Analysis",
            Description:="Returns the preprocessing constants used by a fitted discriminant-analysis model.",
            HelpTopic:=HelpLinks.FallbackBaseUrl & "/udf/multivariate-analysis/"
        )>
        Public Function DA_PREPROCESS(
            <ExcelArgument(Name:="handle", Description:="Handle returned by BESH.MULTI.DA_FIT.")> handle As Object,
            <ExcelArgument(Name:="includeHeader", Description:="TRUE to include a header row (default TRUE).")> Optional includeHeader As Object = Nothing
        ) As Object
            Try
                Dim h As DiscriminantHandle = Nothing
                If Not TryGetDaHandle(handle, h) Then Return ExcelError.ExcelErrorNA
                Dim prepared = h.Model.PreparedData
                Return PrepareExistingObjectTableForUdf(BuildClusterPreprocessingTable(prepared.VariableNames,
                                                                                       prepared.ColumnLocations,
                                                                                       prepared.ColumnScales,
                                                                                       prepared.Standardization),
                                                       GetOptionalBool(includeHeader, True))
            Catch ex As Exception
                Return LoggedUdfExceptionText("BESH.MULTI.DA_PREPROCESS", ex)
            End Try
        End Function

        ''' <summary>
        ''' Returns the covariance matrix used by a fitted discriminant-analysis model.
        ''' </summary>
        ''' <param name="handle">Handle returned by <c>BESH.MULTI.DA_FIT</c>.</param>
        ''' <param name="groupLabel">
        ''' Optional group label. Leave blank to request the pooled covariance matrix used by linear discriminant analysis.
        ''' Supply a specific group label when you want that group's within-group covariance matrix.
        ''' For quadratic discriminant analysis, a group label is normally required because the model uses one covariance matrix per group.
        ''' </param>
        ''' <param name="includeHeader">TRUE to include a header row. Default TRUE.</param>
        ''' <returns>A labeled covariance matrix on the working analysis scale.</returns>
        <ExcelFunction(
            Name:="BESH.MULTI.DA_COVARIANCE",
            Category:="BESHStatNG - Multivariate Analysis",
            Description:="Returns the covariance matrix used by a fitted discriminant-analysis model.",
            HelpTopic:=HelpLinks.FallbackBaseUrl & "/udf/multivariate-analysis/"
        )>
        Public Function DA_COVARIANCE(
            <ExcelArgument(Name:="handle", Description:="Handle returned by BESH.MULTI.DA_FIT.")> handle As Object,
            <ExcelArgument(Name:="groupLabel", Description:="Optional group label. Leave blank for the pooled LDA covariance matrix, or supply a specific group label for a group-specific matrix.")> Optional groupLabel As Object = Nothing,
            <ExcelArgument(Name:="includeHeader", Description:="TRUE to include a header row (default TRUE).")> Optional includeHeader As Object = Nothing
        ) As Object
            Try
                Dim h As DiscriminantHandle = Nothing
                If Not TryGetDaHandle(handle, h) Then Return ExcelError.ExcelErrorNA
                Dim requested As String = CellToTrimmedText(groupLabel)
                If String.IsNullOrWhiteSpace(requested) Then
                    If ParseDiscriminantMethod(h.Method) = Multivariate.DiscriminantAnalysisMethod.Quadratic Then
                        Return PrepareExistingObjectTableForUdf(BuildSimpleNoteTable("Message", "For quadratic discriminant analysis, supply groupLabel to choose one of the group-specific covariance matrices."), GetOptionalBool(includeHeader, True))
                    End If
                    Dim pooled As Object(,) = FindWrappedDiscriminantTable(h.Model, "Pooled Covariance Matrix (Working Scale)")
                    If pooled Is Nothing Then Return ExcelError.ExcelErrorNA
                    Return PrepareWrappedResultTableForUdf(pooled, GetOptionalBool(includeHeader, True))
                End If
                Dim gs As Multivariate.DiscriminantGroupStatistics = Nothing
                For Each item As Multivariate.DiscriminantGroupStatistics In h.Model.GroupStatistics
                    If String.Equals(item.GroupLabel, requested, StringComparison.OrdinalIgnoreCase) Then
                        gs = item
                        Exit For
                    End If
                Next
                If gs Is Nothing Then Throw New ArgumentException("groupLabel was not found in the fitted model.")
                Return BuildNamedMatrixOutput("Variable", h.VariableNames, h.VariableNames, gs.CovarianceWorking, GetOptionalBool(includeHeader, True))
            Catch ex As Exception
                Return LoggedUdfExceptionText("BESH.MULTI.DA_COVARIANCE", ex)
            End Try
        End Function

        ''' <summary>
        ''' Returns the linear classification-function table for a fitted linear discriminant-analysis model.
        ''' </summary>
        ''' <param name="handle">Handle returned by <c>BESH.MULTI.DA_FIT</c>.</param>
        ''' <param name="includeHeader">TRUE to include a header row. Default TRUE.</param>
        ''' <returns>
        ''' A spilled table containing the linear classification constants and coefficients on the original input scale.
        ''' These coefficients are available only for the linear method. For each group, calculate the displayed linear score and assign the case to the group with the largest score.
        ''' </returns>
        <ExcelFunction(
            Name:="BESH.MULTI.DA_FUNCTIONS",
            Category:="BESHStatNG - Multivariate Analysis",
            Description:="Returns the linear classification-function table for a fitted linear discriminant-analysis model.",
            HelpTopic:=HelpLinks.FallbackBaseUrl & "/udf/multivariate-analysis/"
        )>
        Public Function DA_FUNCTIONS(
            <ExcelArgument(Name:="handle", Description:="Handle returned by BESH.MULTI.DA_FIT.")> handle As Object,
            <ExcelArgument(Name:="includeHeader", Description:="TRUE to include a header row (default TRUE).")> Optional includeHeader As Object = Nothing
        ) As Object
            Try
                Dim h As DiscriminantHandle = Nothing
                If Not TryGetDaHandle(handle, h) Then Return ExcelError.ExcelErrorNA
                Dim table As Object(,) = FindWrappedDiscriminantTable(h.Model, "Linear Classification Functions (Original Input Scale)")
                If table Is Nothing Then
                    Return PrepareExistingObjectTableForUdf(BuildSimpleNoteTable("Message", "Linear classification functions are available only for the linear discriminant method."), GetOptionalBool(includeHeader, True))
                End If
                Return PrepareWrappedResultTableForUdf(table, GetOptionalBool(includeHeader, True))
            Catch ex As Exception
                Return LoggedUdfExceptionText("BESH.MULTI.DA_FUNCTIONS", ex)
            End Try
        End Function

        ''' <summary>
        ''' Returns the canonical discriminant-functions summary for a fitted linear discriminant-analysis model.
        ''' </summary>
        ''' <param name="handle">Handle returned by <c>BESH.MULTI.DA_FIT</c>.</param>
        ''' <param name="includeHeader">TRUE to include a header row. Default TRUE.</param>
        ''' <returns>
        ''' A spilled table containing the eigenvalues, canonical correlations, explained proportions, and Wilks' lambda values for the canonical functions.
        ''' This output is available only for the linear method and only when at least one canonical function exists.
        ''' </returns>
        <ExcelFunction(
            Name:="BESH.MULTI.DA_CANONICAL",
            Category:="BESHStatNG - Multivariate Analysis",
            Description:="Returns the canonical discriminant-functions summary for a fitted linear discriminant-analysis model.",
            HelpTopic:=HelpLinks.FallbackBaseUrl & "/udf/multivariate-analysis/"
        )>
        Public Function DA_CANONICAL(
            <ExcelArgument(Name:="handle", Description:="Handle returned by BESH.MULTI.DA_FIT.")> handle As Object,
            <ExcelArgument(Name:="includeHeader", Description:="TRUE to include a header row (default TRUE).")> Optional includeHeader As Object = Nothing
        ) As Object
            Try
                Dim h As DiscriminantHandle = Nothing
                If Not TryGetDaHandle(handle, h) Then Return ExcelError.ExcelErrorNA
                Dim table As Object(,) = FindWrappedDiscriminantTable(h.Model, "Canonical Discriminant Functions Summary")
                If table Is Nothing Then
                    Return PrepareExistingObjectTableForUdf(BuildSimpleNoteTable("Message", "Canonical-function output is available only for linear discriminant analysis with at least one canonical function."), GetOptionalBool(includeHeader, True))
                End If
                Return PrepareWrappedResultTableForUdf(table, GetOptionalBool(includeHeader, True))
            Catch ex As Exception
                Return LoggedUdfExceptionText("BESH.MULTI.DA_CANONICAL", ex)
            End Try
        End Function

        ''' <summary>
        ''' Returns the canonical coefficient matrix for a fitted linear discriminant-analysis model.
        ''' </summary>
        ''' <param name="handle">Handle returned by <c>BESH.MULTI.DA_FIT</c>.</param>
        ''' <param name="includeHeader">TRUE to include a header row. Default TRUE.</param>
        ''' <returns>
        ''' A spilled coefficient matrix showing how each predictor contributes to each canonical discriminant function on the working analysis scale.
        ''' Large absolute coefficients indicate variables that contribute strongly to the corresponding discriminant dimension.
        ''' </returns>
        <ExcelFunction(
            Name:="BESH.MULTI.DA_CANONCOEF",
            Category:="BESHStatNG - Multivariate Analysis",
            Description:="Returns the canonical coefficient matrix for a fitted linear discriminant-analysis model.",
            HelpTopic:=HelpLinks.FallbackBaseUrl & "/udf/multivariate-analysis/"
        )>
        Public Function DA_CANONCOEF(
            <ExcelArgument(Name:="handle", Description:="Handle returned by BESH.MULTI.DA_FIT.")> handle As Object,
            <ExcelArgument(Name:="includeHeader", Description:="TRUE to include a header row (default TRUE).")> Optional includeHeader As Object = Nothing
        ) As Object
            Try
                Dim h As DiscriminantHandle = Nothing
                If Not TryGetDaHandle(handle, h) Then Return ExcelError.ExcelErrorNA
                Dim table As Object(,) = FindWrappedDiscriminantTable(h.Model, "Canonical Coefficients (Working Scale)")
                If table Is Nothing Then
                    Return PrepareExistingObjectTableForUdf(BuildSimpleNoteTable("Message", "Canonical coefficients are available only for linear discriminant analysis with at least one canonical function."), GetOptionalBool(includeHeader, True))
                End If
                Return PrepareWrappedResultTableForUdf(table, GetOptionalBool(includeHeader, True))
            Catch ex As Exception
                Return LoggedUdfExceptionText("BESH.MULTI.DA_CANONCOEF", ex)
            End Try
        End Function

        ''' <summary>
        ''' Returns the group centroids in canonical discriminant space for a fitted linear discriminant-analysis model.
        ''' </summary>
        ''' <param name="handle">Handle returned by <c>BESH.MULTI.DA_FIT</c>.</param>
        ''' <param name="includeHeader">TRUE to include a header row. Default TRUE.</param>
        ''' <returns>
        ''' A spilled table of group centroids on the canonical axes.
        ''' Centroids that are far apart indicate strong between-group separation on the corresponding discriminant functions.
        ''' </returns>
        <ExcelFunction(
            Name:="BESH.MULTI.DA_CENTROIDS",
            Category:="BESHStatNG - Multivariate Analysis",
            Description:="Returns the group centroids in canonical discriminant space for a fitted linear discriminant-analysis model.",
            HelpTopic:=HelpLinks.FallbackBaseUrl & "/udf/multivariate-analysis/"
        )>
        Public Function DA_CENTROIDS(
            <ExcelArgument(Name:="handle", Description:="Handle returned by BESH.MULTI.DA_FIT.")> handle As Object,
            <ExcelArgument(Name:="includeHeader", Description:="TRUE to include a header row (default TRUE).")> Optional includeHeader As Object = Nothing
        ) As Object
            Try
                Dim h As DiscriminantHandle = Nothing
                If Not TryGetDaHandle(handle, h) Then Return ExcelError.ExcelErrorNA
                Dim table As Object(,) = FindWrappedDiscriminantTable(h.Model, "Group Centroids in Canonical Space")
                If table Is Nothing Then
                    Return PrepareExistingObjectTableForUdf(BuildSimpleNoteTable("Message", "Canonical centroids are available only for linear discriminant analysis with at least one canonical function."), GetOptionalBool(includeHeader, True))
                End If
                Return PrepareWrappedResultTableForUdf(table, GetOptionalBool(includeHeader, True))
            Catch ex As Exception
                Return LoggedUdfExceptionText("BESH.MULTI.DA_CENTROIDS", ex)
            End Try
        End Function

        ''' <summary>
        ''' Returns an observed-versus-predicted classification table for the training or validation pass.
        ''' </summary>
        ''' <param name="handle">Handle returned by <c>BESH.MULTI.DA_FIT</c>.</param>
        ''' <param name="source">
        ''' Optional result source: <c>"training"</c> (default) or <c>"validation"</c>.
        ''' The training table is the apparent or resubstitution table based on the fitted model.
        ''' The validation table is available only when validation was requested during fitting.
        ''' </param>
        ''' <param name="includeHeader">TRUE to include a header row. Default TRUE.</param>
        ''' <returns>
        ''' A spilled confusion matrix including row totals, column totals, per-group recall, per-group precision, and overall classification accuracy.
        ''' </returns>
        <ExcelFunction(
            Name:="BESH.MULTI.DA_CONFUSION",
            Category:="BESHStatNG - Multivariate Analysis",
            Description:="Returns an observed-versus-predicted classification table for the training or validation pass.",
            HelpTopic:=HelpLinks.FallbackBaseUrl & "/udf/multivariate-analysis/"
        )>
        Public Function DA_CONFUSION(
            <ExcelArgument(Name:="handle", Description:="Handle returned by BESH.MULTI.DA_FIT.")> handle As Object,
            <ExcelArgument(Name:="source", Description:="Optional source: training (default) or validation.")> Optional source As Object = Nothing,
            <ExcelArgument(Name:="includeHeader", Description:="TRUE to include a header row (default TRUE).")> Optional includeHeader As Object = Nothing
        ) As Object
            Try
                Dim h As DiscriminantHandle = Nothing
                If Not TryGetDaHandle(handle, h) Then Return ExcelError.ExcelErrorNA
                Dim result As Multivariate.DiscriminantPredictionResult = GetDiscriminantPredictionResult(h.Model, source)
                If result Is Nothing OrElse result.Confusion Is Nothing Then
                    Return PrepareExistingObjectTableForUdf(BuildSimpleNoteTable("Message", "The requested classification table is not available. Validation output exists only when validationMode was requested during DA_FIT."), GetOptionalBool(includeHeader, True))
                End If
                Return PrepareExistingObjectTableForUdf(result.Confusion.ToObjectTable(), GetOptionalBool(includeHeader, True))
            Catch ex As Exception
                Return LoggedUdfExceptionText("BESH.MULTI.DA_CONFUSION", ex)
            End Try
        End Function

        ''' <summary>
        ''' Returns the casewise classification table for the training or validation pass.
        ''' </summary>
        ''' <param name="handle">Handle returned by <c>BESH.MULTI.DA_FIT</c>.</param>
        ''' <param name="source">Optional source: training (default) or validation.</param>
        ''' <param name="maxRows">
        ''' Optional maximum number of rows to return. Leave blank to spill the full table.
        ''' This is useful when you want to inspect the first portion of a large classification result without filling a large worksheet area.
        ''' </param>
        ''' <param name="includeHeader">TRUE to include a header row. Default TRUE.</param>
        ''' <returns>
        ''' A spilled casewise table containing observed and predicted groups, assigned posterior probability, per-group posterior probabilities,
        ''' and squared distances for each case.
        ''' </returns>
        <ExcelFunction(
            Name:="BESH.MULTI.DA_CASEWISE",
            Category:="BESHStatNG - Multivariate Analysis",
            Description:="Returns the casewise classification table for the training or validation pass.",
            HelpTopic:=HelpLinks.FallbackBaseUrl & "/udf/multivariate-analysis/"
        )>
        Public Function DA_CASEWISE(
            <ExcelArgument(Name:="handle", Description:="Handle returned by BESH.MULTI.DA_FIT.")> handle As Object,
            <ExcelArgument(Name:="source", Description:="Optional source: training (default) or validation.")> Optional source As Object = Nothing,
            <ExcelArgument(Name:="maxRows", Description:="Optional maximum number of rows to return. Leave blank for all rows.")> Optional maxRows As Object = Nothing,
            <ExcelArgument(Name:="includeHeader", Description:="TRUE to include a header row (default TRUE).")> Optional includeHeader As Object = Nothing
        ) As Object
            Try
                Dim h As DiscriminantHandle = Nothing
                If Not TryGetDaHandle(handle, h) Then Return ExcelError.ExcelErrorNA
                Dim result As Multivariate.DiscriminantPredictionResult = GetDiscriminantPredictionResult(h.Model, source)
                If result Is Nothing Then
                    Return PrepareExistingObjectTableForUdf(BuildSimpleNoteTable("Message", "The requested casewise table is not available. Validation output exists only when validationMode was requested during DA_FIT."), GetOptionalBool(includeHeader, True))
                End If
                Return PrepareExistingObjectTableForUdf(result.ToCasewiseTable(h.Model.GroupLabels, GetOptionalInt(maxRows, -1)), GetOptionalBool(includeHeader, True))
            Catch ex As Exception
                Return LoggedUdfExceptionText("BESH.MULTI.DA_CASEWISE", ex)
            End Try
        End Function

        ''' <summary>
        ''' Applies a fitted discriminant-analysis model to a new predictor matrix and returns casewise predictions.
        ''' </summary>
        ''' <param name="handle">Handle returned by <c>BESH.MULTI.DA_FIT</c>.</param>
        ''' <param name="newX">New numeric predictor matrix with the same columns and column order used when the model was fitted.</param>
        ''' <param name="rowLabels">Optional one-column range of case labels aligned with <paramref name="newX"/>.</param>
        ''' <param name="actualGroups">Optional one-column range of known groups for the new cases, used only to populate the Actual column in the output.</param>
        ''' <param name="includeHeader">TRUE to include a header row. Default TRUE.</param>
        ''' <returns>
        ''' A spilled casewise prediction table containing predicted groups, assigned posterior probabilities, per-group posterior probabilities,
        ''' and squared distances for each new case.
        ''' </returns>
        <ExcelFunction(
            Name:="BESH.MULTI.DA_PREDICT",
            Category:="BESHStatNG - Multivariate Analysis",
            Description:="Applies a fitted discriminant-analysis model to a new predictor matrix and returns casewise predictions.",
            HelpTopic:=HelpLinks.FallbackBaseUrl & "/udf/multivariate-analysis/"
        )>
        Public Function DA_PREDICT(
            <ExcelArgument(Name:="handle", Description:="Handle returned by BESH.MULTI.DA_FIT.")> handle As Object,
            <ExcelArgument(AllowReference:=True, Name:="newX", Description:="New numeric predictor matrix with the same columns used during fitting.")> newX As Object,
            <ExcelArgument(Name:="rowLabels", Description:="Optional one-column range of case labels aligned with the new predictor matrix.")> Optional rowLabels As Object = Nothing,
            <ExcelArgument(Name:="actualGroups", Description:="Optional one-column range of known groups for the new cases.")> Optional actualGroups As Object = Nothing,
            <ExcelArgument(Name:="includeHeader", Description:="TRUE to include a header row (default TRUE).")> Optional includeHeader As Object = Nothing
        ) As Object
            Try
                Dim h As DiscriminantHandle = Nothing
                If Not TryGetDaHandle(handle, h) Then Return ExcelError.ExcelErrorNA
                Dim imported As DataObj = Nothing
                If Not Global.BESHStatNG.UdfDataImport.TryGetNumericData(newX, h.VariableNames, False, imported) Then Return ExcelError.ExcelErrorValue
                If imported.nCols <> h.VariableNames.Length Then Throw New ArgumentException("newX must have the same number of predictor columns used when the model was fitted.")
                Dim labels() As String = Nothing
                If Not TryResolveOptionalClusterRowLabels(rowLabels, imported.nRows, labels) Then Return ExcelError.ExcelErrorValue
                Dim actualObjects() As Object = Nothing
                If Not IsMissingArg(actualGroups) Then
                    Dim actualCol(,) As Object = Nothing
                    Dim actualName As String = Nothing
                    If Not TryGetAlignedClusterIdColumnObject(actualGroups, imported.nRows, actualCol, actualName) Then Return ExcelError.ExcelErrorValue
                    ReDim actualObjects(imported.nRows - 1)
                    For i As Integer = 0 To imported.nRows - 1
                        actualObjects(i) = actualCol(i, 0)
                    Next
                End If
                Dim result As Multivariate.DiscriminantPredictionResult = h.Model.PredictDetailed(imported.DataDbl, labels, actualObjects)
                Return PrepareExistingObjectTableForUdf(result.ToCasewiseTable(h.Model.GroupLabels), GetOptionalBool(includeHeader, True))
            Catch ex As Exception
                Return LoggedUdfExceptionText("BESH.MULTI.DA_PREDICT", ex)
            End Try
        End Function

        ''' <summary>
        ''' Returns the rows removed by the missing-value policy before discriminant analysis was fitted.
        ''' </summary>
        ''' <param name="handle">Handle returned by <c>BESH.MULTI.DA_FIT</c>.</param>
        ''' <param name="includeHeader">TRUE to include a header row. Default TRUE.</param>
        ''' <returns>
        ''' A spilled table listing the original case indices and optional case labels removed before fitting because at least one predictor
        ''' was missing or non-finite. When no rows were removed, the function returns a short note table instead of an error.
        ''' </returns>
        <ExcelFunction(
            Name:="BESH.MULTI.DA_REMOVED",
            Category:="BESHStatNG - Multivariate Analysis",
            Description:="Returns the rows removed by the missing-value policy before discriminant analysis was fitted.",
            HelpTopic:=HelpLinks.FallbackBaseUrl & "/udf/multivariate-analysis/"
        )>
        Public Function DA_REMOVED(
            <ExcelArgument(Name:="handle", Description:="Handle returned by BESH.MULTI.DA_FIT.")> handle As Object,
            <ExcelArgument(Name:="includeHeader", Description:="TRUE to include a header row (default TRUE).")> Optional includeHeader As Object = Nothing
        ) As Object
            Try
                Dim h As DiscriminantHandle = Nothing
                If Not TryGetDaHandle(handle, h) Then Return ExcelError.ExcelErrorNA
                Dim prepared = h.Model.PreparedData
                Return PrepareExistingObjectTableForUdf(BuildRemovedRowsOutput(prepared.RemovedOriginalIndices, prepared.RemovedRowLabels), GetOptionalBool(includeHeader, True))
            Catch ex As Exception
                Return LoggedUdfExceptionText("BESH.MULTI.DA_REMOVED", ex)
            End Try
        End Function

        ''' <summary>
        ''' Removes a discriminant-analysis handle from memory.
        ''' </summary>
        ''' <param name="handle">Handle returned by <c>BESH.MULTI.DA_FIT</c>.</param>
        ''' <returns>TRUE when the handle was removed; FALSE when the handle was not found.</returns>
        <ExcelFunction(
            Name:="BESH.MULTI.DA_DROP",
            Category:="BESHStatNG - Multivariate Analysis",
            Description:="Removes a discriminant-analysis handle from memory.",
            HelpTopic:=HelpLinks.FallbackBaseUrl & "/udf/multivariate-analysis/"
        )>
        Public Function DA_DROP(
            <ExcelArgument(Name:="handle", Description:="Handle returned by BESH.MULTI.DA_FIT.")> handle As Object
        ) As Object
            Try
                Dim key As String = AsString(handle)
                If String.IsNullOrWhiteSpace(key) Then Return False
                Dim removed As DiscriminantHandle = Nothing
                Return _daCache.TryRemove(key.Trim(), removed)
            Catch ex As Exception
                Return LoggedUdfExceptionText("BESH.MULTI.DA_DROP", ex)
            End Try
        End Function

        '----------------------------------------------------------------------
        ' Helpers
        '----------------------------------------------------------------------
        Private Function TryGetKMeansHandle(handle As Object, ByRef found As KMeansHandle) As Boolean
            found = Nothing
            Dim key As String = AsString(handle)
            If String.IsNullOrWhiteSpace(key) Then Return False
            Return _kmeansCache.TryGetValue(key.Trim(), found)
        End Function

        Private Function TryGetHierarchicalHandle(handle As Object, ByRef found As HierarchicalHandle) As Boolean
            found = Nothing
            Dim key As String = AsString(handle)
            If String.IsNullOrWhiteSpace(key) Then Return False
            Return _hclustCache.TryGetValue(key.Trim(), found)
        End Function

        Private Function TryGetPcaHandle(handle As Object, ByRef found As PcaHandle) As Boolean
            found = Nothing
            Dim key As String = AsString(handle)
            If String.IsNullOrWhiteSpace(key) Then Return False
            Return _pcaCache.TryGetValue(key.Trim(), found)
        End Function

        Private Function TryGetFaHandle(handle As Object, ByRef found As FactorAnalysisHandle) As Boolean
            found = Nothing
            Dim key As String = AsString(handle)
            If String.IsNullOrWhiteSpace(key) Then Return False
            Return _faCache.TryGetValue(key.Trim(), found)
        End Function

        Private Function TryGetCaHandle(handle As Object, ByRef found As CorrespondenceHandle) As Boolean
            found = Nothing
            Dim key As String = AsString(handle)
            If String.IsNullOrWhiteSpace(key) Then Return False
            Return _caCache.TryGetValue(key.Trim(), found)
        End Function

        Private Function TryGetMcaHandle(handle As Object, ByRef found As MultipleCorrespondenceHandle) As Boolean
            found = Nothing
            Dim key As String = AsString(handle)
            If String.IsNullOrWhiteSpace(key) Then Return False
            Return _mcaCache.TryGetValue(key.Trim(), found)
        End Function

        Private Function TryGetDaHandle(handle As Object, ByRef found As DiscriminantHandle) As Boolean
            found = Nothing
            Dim key As String = AsString(handle)
            If String.IsNullOrWhiteSpace(key) Then Return False
            Return _daCache.TryGetValue(key.Trim(), found)
        End Function

        Private Function TryResolveOptionalClusterRowLabels(arg As Object,
                                                            expectedRows As Integer,
                                                            ByRef rowLabels() As String) As Boolean
            rowLabels = Nothing
            If arg Is Nothing OrElse TypeOf arg Is ExcelEmpty OrElse TypeOf arg Is ExcelMissing Then Return True

            Dim col(,) As Object = Nothing
            Dim inferredName As String = Nothing
            If Not TryGetAlignedClusterIdColumnObject(arg, expectedRows, col, inferredName) Then Return False

            ReDim rowLabels(expectedRows - 1)
            For i As Integer = 0 To expectedRows - 1
                Dim txt As String = CellToTrimmedText(col(i, 0))
                If String.IsNullOrWhiteSpace(txt) Then txt = "Obs " & (i + 1).ToString(CultureInfo.InvariantCulture)
                rowLabels(i) = txt
            Next
            Return True
        End Function

        Private Function ParsePcaMatrixType(v As Object) As String
            Dim token As String = NormalizeToken(v)
            Select Case token
                Case "", "CORRELATION", "CORR", "STANDARDIZED", "STANDARDISED"
                    Return "Correlation"
                Case "COVARIANCE", "COV", "RAW", "ORIGINAL"
                    Return "Covariance"
                Case Else
                    Throw New ArgumentException("matrixType must be either correlation or covariance.")
            End Select
        End Function

        Private Function ParsePcaRetentionMethod(v As Object) As String
            Dim token As String = NormalizeToken(v)
            Select Case token
                Case "", "EIGENVALUE", "KAISER", "MINEIGEN"
                    Return "Eigenvalue"
                Case "FIXED", "COUNT", "NUMBER"
                    Return "Fixed"
                Case "VARIANCE", "PERCENT", "PCT", "CUMULATIVE"
                    Return "Variance"
                Case Else
                    Throw New ArgumentException("retentionMethod must be eigenvalue, fixed, or variance.")
            End Select
        End Function

        Private Function ParseFaMatrixType(v As Object) As Multivariate.FactorAnalysisMatrixType
            Dim token As String = NormalizeToken(v)
            Select Case token
                Case "", "CORRELATION", "CORR", "STANDARDIZED", "STANDARDISED"
                    Return Multivariate.FactorAnalysisMatrixType.Correlation
                Case "COVARIANCE", "COV", "RAW", "ORIGINAL"
                    Return Multivariate.FactorAnalysisMatrixType.Covariance
                Case Else
                    Throw New ArgumentException("matrixType must be either correlation or covariance.")
            End Select
        End Function

        Private Function ParseFaExtractionMethod(v As Object) As Multivariate.FactorAnalysisExtractionMethod
            Dim token As String = NormalizeToken(v)
            Select Case token
                Case "", "PRINCIPALAXIS", "PAF", "PRINCIPALFACTOR", "PRINCIPALAXISFACTORING"
                    Return Multivariate.FactorAnalysisExtractionMethod.PrincipalAxis
                Case "PRINCIPALCOMPONENTS", "PC", "PCA", "COMPONENTS"
                    Return Multivariate.FactorAnalysisExtractionMethod.PrincipalComponents
                Case "MAXIMUMLIKELIHOOD", "ML"
                    Return Multivariate.FactorAnalysisExtractionMethod.MaximumLikelihood
                Case "GENERALIZEDLEASTSQUARES", "GLS"
                    Return Multivariate.FactorAnalysisExtractionMethod.GeneralizedLeastSquares
                Case "IMAGE"
                    Return Multivariate.FactorAnalysisExtractionMethod.Image
                Case "ALPHA"
                    Return Multivariate.FactorAnalysisExtractionMethod.Alpha
                Case Else
                    Throw New ArgumentException("extractionMethod must be principalaxis, principalcomponents, ml, gls, image, or alpha.")
            End Select
        End Function

        Private Function ParseFaRetentionMethod(v As Object) As Multivariate.FactorAnalysisRetentionMethod
            Dim token As String = NormalizeToken(v)
            Select Case token
                Case "", "EIGENVALUE", "KAISER", "MINEIGEN"
                    Return Multivariate.FactorAnalysisRetentionMethod.Eigenvalue
                Case "FIXED", "COUNT", "NUMBER"
                    Return Multivariate.FactorAnalysisRetentionMethod.Fixed
                Case "VARIANCE", "PERCENT", "PCT", "CUMULATIVE"
                    Return Multivariate.FactorAnalysisRetentionMethod.Variance
                Case Else
                    Throw New ArgumentException("retentionMethod must be fixed, eigenvalue, or variance.")
            End Select
        End Function

        Private Function ParseFaRotationMethod(v As Object) As Multivariate.FactorAnalysisRotationMethod
            Dim token As String = NormalizeToken(v)
            Select Case token
                Case "", "NONE", "UNROTATED"
                    Return Multivariate.FactorAnalysisRotationMethod.None
                Case "VARIMAX"
                    Return Multivariate.FactorAnalysisRotationMethod.Varimax
                Case "QUARTIMAX"
                    Return Multivariate.FactorAnalysisRotationMethod.Quartimax
                Case "EQUAMAX"
                    Return Multivariate.FactorAnalysisRotationMethod.Equamax
                Case "PROMAX"
                    Return Multivariate.FactorAnalysisRotationMethod.Promax
                Case Else
                    Throw New ArgumentException("rotationMethod must be none, varimax, quartimax, equamax, or promax.")
            End Select
        End Function

        Private Function ParseFaScoreMethod(v As Object) As Multivariate.FactorAnalysisScoreMethod
            Dim token As String = NormalizeToken(v)
            Select Case token
                Case "", "REGRESSION", "THOMSON"
                    Return Multivariate.FactorAnalysisScoreMethod.Regression
                Case "NONE"
                    Return Multivariate.FactorAnalysisScoreMethod.None
                Case "BARTLETT"
                    Return Multivariate.FactorAnalysisScoreMethod.Bartlett
                Case Else
                    Throw New ArgumentException("scoreMethod must be regression, bartlett, or none.")
            End Select
        End Function

        Private Function ParseFaCommunalityInitialization(v As Object) As Multivariate.FactorAnalysisCommunalityInitialization
            Dim token As String = NormalizeToken(v)
            Select Case token
                Case "", "SMC", "SQUAREDMULTIPLECORRELATION", "SQUAREDMULTIPLECORRELATIONS"
                    Return Multivariate.FactorAnalysisCommunalityInitialization.SquaredMultipleCorrelation
                Case "ONE", "UNIT", "ONES"
                    Return Multivariate.FactorAnalysisCommunalityInitialization.One
                Case Else
                    Throw New ArgumentException("communalityInitialization must be smc or one.")
            End Select
        End Function

        Private Function ParseFaMissingPolicy(v As Object) As Multivariate.FactorAnalysisMissingValuePolicy
            Dim token As String = NormalizeToken(v)
            Select Case token
                Case "", "ERROR", "ERRORONMISSING", "STOP"
                    Return Multivariate.FactorAnalysisMissingValuePolicy.ErrorOnMissing
                Case "LISTWISE", "LISTWISEDELETION", "DELETE", "DROP"
                    Return Multivariate.FactorAnalysisMissingValuePolicy.ListwiseDeletion
                Case Else
                    Throw New ArgumentException("missingValuePolicy must be error or listwise.")
            End Select
        End Function

        Private Function ParseClusterStandardizationMode(v As Object) As Multivariate.ClusterStandardizationMode
            Dim token As String = NormalizeToken(v)
            Select Case token
                Case "", "NONE", "RAW", "ORIGINAL"
                    Return Multivariate.ClusterStandardizationMode.None
                Case "ZSCORES", "ZSCORE", "STANDARDIZE", "STANDARDISE", "STANDARDIZED", "STANDARDISED"
                    Return Multivariate.ClusterStandardizationMode.ZScores
                Case "RANGE01", "RANGE0TO1", "ZEROONE", "RANGEZEROTOONE", "MINMAX", "UNITINTERVAL"
                    Return Multivariate.ClusterStandardizationMode.RangeZeroToOne
                Case Else
                    Throw New ArgumentException("standardization must be none, zscores, or range01.")
            End Select
        End Function

        Private Function ParseClusterMissingValuePolicy(v As Object) As Multivariate.ClusterMissingValuePolicy
            Dim token As String = NormalizeToken(v)
            Select Case token
                Case "", "ERROR", "ERRORONMISSING", "STOP"
                    Return Multivariate.ClusterMissingValuePolicy.ErrorOnMissing
                Case "LISTWISE", "LISTWISEDELETION", "DELETE", "DROP"
                    Return Multivariate.ClusterMissingValuePolicy.ListwiseDeletion
                Case Else
                    Throw New ArgumentException("missingValuePolicy must be error or listwise.")
            End Select
        End Function

        Private Function ParseKMeansInitialization(v As Object) As Multivariate.KMeansInitializationMethod
            Dim token As String = NormalizeToken(v)
            Select Case token
                Case "", "KMEANSPLUSPLUS", "KMEANS++", "PLUSPLUS", "KPP"
                    Return Multivariate.KMeansInitializationMethod.KMeansPlusPlus
                Case "FORGY", "RANDOMOBSERVATIONS"
                    Return Multivariate.KMeansInitializationMethod.Forgy
                Case "RANDOMPARTITION", "PARTITION"
                    Return Multivariate.KMeansInitializationMethod.RandomPartition
                Case "USERSPECIFIED", "USERSPECIFIEDCENTERS", "CENTERS", "STARTINGCENTERS"
                    Return Multivariate.KMeansInitializationMethod.UserSpecifiedCenters
                Case Else
                    Throw New ArgumentException("initializationMethod must be kmeans++, forgy, randompartition, or userspecified.")
            End Select
        End Function

        Private Function ParseKMeansDistanceMetric(v As Object) As Multivariate.KMeansDistanceMetric
            Dim token As String = NormalizeToken(v)
            Select Case token
                Case "", "SQUAREDEUCLIDEAN", "SQEUCLIDEAN", "SQUARED"
                    Return Multivariate.KMeansDistanceMetric.SquaredEuclidean
                Case "EUCLIDEAN"
                    Return Multivariate.KMeansDistanceMetric.Euclidean
                Case Else
                    Throw New ArgumentException("distanceMetric must be squaredeuclidean or euclidean.")
            End Select
        End Function

        Private Function ParseEmptyClusterHandling(v As Object) As Multivariate.EmptyClusterHandlingStrategy
            Dim token As String = NormalizeToken(v)
            Select Case token
                Case "", "FARTHESTOBSERVATION", "FARTHEST"
                    Return Multivariate.EmptyClusterHandlingStrategy.FarthestObservation
                Case "RANDOMOBSERVATION", "RANDOM"
                    Return Multivariate.EmptyClusterHandlingStrategy.RandomObservation
                Case "KEEPPREVIOUSCENTER", "KEEP", "PREVIOUS"
                    Return Multivariate.EmptyClusterHandlingStrategy.KeepPreviousCenter
                Case Else
                    Throw New ArgumentException("emptyClusterHandling must be farthestobservation, randomobservation, or keeppreviouscenter.")
            End Select
        End Function

        Private Function ParseHierarchicalLinkage(v As Object) As Multivariate.HierarchicalLinkageMethod
            Dim token As String = NormalizeToken(v)
            Select Case token
                Case "", "WARD", "WARDS", "WARDMINIMUMVARIANCE"
                    Return Multivariate.HierarchicalLinkageMethod.Ward
                Case "SINGLE", "SINGLELINKAGE"
                    Return Multivariate.HierarchicalLinkageMethod.SingleLinkage
                Case "COMPLETE", "COMPLETELINKAGE"
                    Return Multivariate.HierarchicalLinkageMethod.Complete
                Case "AVERAGE", "AVERAGELINKAGE", "UPGMA"
                    Return Multivariate.HierarchicalLinkageMethod.Average
                Case "WEIGHTEDAVERAGE", "WEIGHTED", "WPGMA"
                    Return Multivariate.HierarchicalLinkageMethod.WeightedAverage
                Case "CENTROID", "UPGMC"
                    Return Multivariate.HierarchicalLinkageMethod.Centroid
                Case "MEDIAN", "WPGMC"
                    Return Multivariate.HierarchicalLinkageMethod.Median
                Case Else
                    Throw New ArgumentException("linkage must be ward, single, complete, average, weightedaverage, centroid, or median.")
            End Select
        End Function

        Private Function ParseHierarchicalDistanceMetric(v As Object) As Multivariate.HierarchicalDistanceMetric
            Dim token As String = NormalizeToken(v)
            Select Case token
                Case "", "SQUAREDEUCLIDEAN", "SQEUCLIDEAN", "SQUARED"
                    Return Multivariate.HierarchicalDistanceMetric.SquaredEuclidean
                Case "EUCLIDEAN"
                    Return Multivariate.HierarchicalDistanceMetric.Euclidean
                Case "MANHATTAN", "CITYBLOCK"
                    Return Multivariate.HierarchicalDistanceMetric.Manhattan
                Case "CHEBYSHEV", "MAXIMUM"
                    Return Multivariate.HierarchicalDistanceMetric.Chebyshev
                Case "MINKOWSKI"
                    Return Multivariate.HierarchicalDistanceMetric.Minkowski
                Case "COSINE"
                    Return Multivariate.HierarchicalDistanceMetric.Cosine
                Case "CORRELATION", "PEARSON"
                    Return Multivariate.HierarchicalDistanceMetric.Correlation
                Case Else
                    Throw New ArgumentException("distanceMetric must be squaredeuclidean, euclidean, manhattan, chebyshev, minkowski, cosine, or correlation.")
            End Select
        End Function

        Private Function ParseHierarchicalMembershipDisplayMode(v As Object) As Multivariate.HierarchicalMembershipDisplayMode
            Dim token As String = NormalizeToken(v)
            Select Case token
                Case "", "CLUSTERS", "COUNT", "K", "BYCLUSTERCOUNT"
                    Return Multivariate.HierarchicalMembershipDisplayMode.ByClusterCount
                Case "HEIGHT", "CUTHEIGHT", "BYHEIGHT"
                    Return Multivariate.HierarchicalMembershipDisplayMode.ByHeight
                Case Else
                    Throw New ArgumentException("mode must be clusters/count or height.")
            End Select
        End Function

        Private Function ParseOutputScaleUseOriginal(v As Object) As Boolean
            Dim token As String = NormalizeToken(v)
            Select Case token
                Case "", "ORIGINAL", "RAW"
                    Return True
                Case "WORKING", "STANDARDIZED", "STANDARDISED", "ANALYSIS"
                    Return False
                Case Else
                    Throw New ArgumentException("scale must be original or working.")
            End Select
        End Function

        Private Function ParseDiscriminantMethod(v As Object) As Multivariate.DiscriminantAnalysisMethod
            Dim token As String = NormalizeToken(v)
            Select Case token
                Case "", "LINEAR", "LDA"
                    Return Multivariate.DiscriminantAnalysisMethod.Linear
                Case "QUADRATIC", "QDA"
                    Return Multivariate.DiscriminantAnalysisMethod.Quadratic
                Case Else
                    Throw New ArgumentException("method must be linear or quadratic.")
            End Select
        End Function

        Private Function ParseDiscriminantPriorMode(v As Object) As Multivariate.DiscriminantPriorMode
            Dim token As String = NormalizeToken(v)
            Select Case token
                Case "", "PROPORTIONAL", "PROPORTIONALTOGROUPSIZES", "PROPORTIONALTOGROUPSIZE", "OBSERVED", "DEFAULT"
                    Return Multivariate.DiscriminantPriorMode.ProportionalToGroupSizes
                Case "EQUAL", "UNIFORM"
                    Return Multivariate.DiscriminantPriorMode.Equal
                Case "USER", "USERSPECIFIED", "CUSTOM"
                    Return Multivariate.DiscriminantPriorMode.UserSpecified
                Case Else
                    Throw New ArgumentException("priorMode must be proportional, equal, or user.")
            End Select
        End Function

        Private Function ParseDiscriminantValidationMode(v As Object) As Multivariate.DiscriminantValidationMode
            Dim token As String = NormalizeToken(v)
            Select Case token
                Case "", "NONE", "APPARENT", "RESUBSTITUTION"
                    Return Multivariate.DiscriminantValidationMode.None
                Case "LEAVEONEOUT", "LOO", "JACKKNIFE"
                    Return Multivariate.DiscriminantValidationMode.LeaveOneOut
                Case "KFOLD", "K-FOLD", "CV", "CROSSVALIDATION"
                    Return Multivariate.DiscriminantValidationMode.KFold
                Case "HOLDOUT", "TEST", "TRAINTEST", "SPLIT"
                    Return Multivariate.DiscriminantValidationMode.Holdout
                Case Else
                    Throw New ArgumentException("validationMode must be none, leaveoneout, kfold, or holdout.")
            End Select
        End Function

        Private Function BuildClusterPreprocessingTable(varNames() As String, locations() As Double, scales() As Double,
                                                        mode As Multivariate.ClusterStandardizationMode) As Object(,)
            Dim varCount As Integer = 0
            If varNames IsNot Nothing Then
                varCount = varNames.Length
            ElseIf locations IsNot Nothing Then
                varCount = locations.Length
            ElseIf scales IsNot Nothing Then
                varCount = scales.Length
            End If

            If mode = Multivariate.ClusterStandardizationMode.None OrElse varCount <= 0 Then
                Dim note(1, 1) As Object
                note(0, 0) = "Setting"
                note(0, 1) = "Value"
                note(1, 0) = "Standardization"
                note(1, 1) = "None"
                Return note
            End If

            Dim out(varCount, 2) As Object
            out(0, 0) = "Variable"
            out(0, 1) = "Location"
            out(0, 2) = "Scale"
            For i As Integer = 0 To varCount - 1
                out(i + 1, 0) = If(varNames IsNot Nothing AndAlso i < varNames.Length, CType(varNames(i), Object), CType("X" & (i + 1).ToString(CultureInfo.InvariantCulture), Object))
                out(i + 1, 1) = If(locations IsNot Nothing AndAlso i < locations.Length, CType(locations(i), Object), CType(Nothing, Object))
                out(i + 1, 2) = If(scales IsNot Nothing AndAlso i < scales.Length, CType(scales(i), Object), CType(Nothing, Object))
            Next
            Return out
        End Function

        Private Function BuildRemovedRowsOutput(rowIds() As Integer, rowLabels() As String) As Object(,)
            If rowIds Is Nothing OrElse rowIds.Length = 0 Then
                Dim note(1, 1) As Object
                note(0, 0) = "Setting"
                note(0, 1) = "Value"
                note(1, 0) = "Removed rows"
                note(1, 1) = "None"
                Return note
            End If

            Dim hasLabels As Boolean = (rowLabels IsNot Nothing AndAlso rowLabels.Length = rowIds.Length)
            Dim out(rowIds.Length, If(hasLabels, 2, 1)) As Object
            out(0, 0) = "OriginalRow"
            If hasLabels Then
                out(0, 1) = "RowLabel"
                out(0, 2) = "Reason"
            Else
                out(0, 1) = "Reason"
            End If

            For i As Integer = 0 To rowIds.Length - 1
                out(i + 1, 0) = rowIds(i)
                If hasLabels Then
                    out(i + 1, 1) = rowLabels(i)
                    out(i + 1, 2) = "Removed before fitting because at least one analysis variable was missing or non-finite."
                Else
                    out(i + 1, 1) = "Removed before fitting because at least one analysis variable was missing or non-finite."
                End If
            Next
            Return out
        End Function

        Private Function BuildHierarchicalLeafOrderTable(result As Multivariate.HierarchicalClusterResult) As Object(,)
            If result Is Nothing Then Return Nothing

            Dim n As Integer = If(result.ActiveRowIndices Is Nothing, 0, result.ActiveRowIndices.Length)
            Dim out(Math.Max(n, 1), 2) As Object
            out(0, 0) = "DisplayOrder"
            out(0, 1) = "OriginalRow"
            out(0, 2) = "RowLabel"
            If n = 0 Then
                out(1, 0) = 1
                out(1, 1) = Nothing
                out(1, 2) = Nothing
                Return out
            End If

            Dim labelByRow As New Dictionary(Of Integer, String)
            If result.ActiveRowLabels IsNot Nothing AndAlso result.ActiveRowLabels.Length = n Then
                For i As Integer = 0 To n - 1
                    labelByRow(result.ActiveRowIndices(i)) = result.ActiveRowLabels(i)
                Next
            End If

            Dim displayOrder() As Integer = If(result.LeafOrder IsNot Nothing AndAlso result.LeafOrder.Length = n,
                                               result.LeafOrder,
                                               result.ActiveRowIndices)
            For i As Integer = 0 To n - 1
                Dim rowId As Integer = displayOrder(i)
                out(i + 1, 0) = i + 1
                out(i + 1, 1) = rowId
                out(i + 1, 2) = If(labelByRow.ContainsKey(rowId), CType(labelByRow(rowId), Object), CType("Obs " & rowId.ToString(CultureInfo.InvariantCulture), Object))
            Next
            Return out
        End Function

        Private Function PercentOfTotal(values() As Double, total As Double) As Double()
            If values Is Nothing Then Return Nothing
            Dim out(values.Length - 1) As Double
            If total = 0.0R OrElse Double.IsNaN(total) OrElse Double.IsInfinity(total) Then Return out
            For i As Integer = 0 To values.Length - 1
                out(i) = 100.0R * values(i) / total
            Next
            Return out
        End Function

        Private Function Cumulative(values() As Double) As Double()
            If values Is Nothing Then Return Nothing
            Dim out(values.Length - 1) As Double
            Dim running As Double = 0.0R
            For i As Integer = 0 To values.Length - 1
                running += values(i)
                out(i) = running
            Next
            Return out
        End Function

        Private Function ColumnSumsOfSquares(pattern(,) As Double, strctre(,) As Double) As Double()
            If pattern Is Nothing OrElse strctre Is Nothing Then Return Nothing
            Dim p As Integer = pattern.GetLength(0)
            Dim k As Integer = pattern.GetLength(1)
            Dim out(k - 1) As Double
            For j As Integer = 0 To k - 1
                Dim sum As Double = 0.0R
                For i As Integer = 0 To p - 1
                    sum += pattern(i, j) * strctre(i, j)
                Next
                out(j) = sum
            Next
            Return out
        End Function

        Private Function BuildCaEigenOutput(model As Multivariate.CA, includeHeader As Boolean) As Object
            If model Is Nothing OrElse model.Eigenvalues Is Nothing Then Return ExcelError.ExcelErrorNA
            Dim eigen() As Double = model.Eigenvalues
            Dim pct(,) As Double = model.Percents
            Dim out(eigen.Length - 1 + If(includeHeader, 1, 0), 3) As Object
            Dim r0 As Integer = 0
            If includeHeader Then
                out(0, 0) = "Axis"
                out(0, 1) = "Inertia"
                out(0, 2) = "% Inertia"
                out(0, 3) = "Cumulative % Inertia"
                r0 = 1
            End If

            For i As Integer = 0 To eigen.Length - 1
                out(r0 + i, 0) = "Dim " & (i + 1).ToString(CultureInfo.InvariantCulture)
                out(r0 + i, 1) = eigen(i)
                out(r0 + i, 2) = pct(i, 0)
                out(r0 + i, 3) = pct(i, 1)
            Next

            Return PrepareResultTableForUdf(out)
        End Function

        Private Function BuildCaOverviewOutput(idHeader As String,
                                               names() As String,
                                               quality() As Double,
                                               mass() As Double,
                                               distance() As Double,
                                               inertia() As Double,
                                               includeHeader As Boolean) As Object
            Dim n As Integer = names.Length
            Dim out(n - 1 + If(includeHeader, 1, 0), 4) As Object
            Dim r0 As Integer = 0
            If includeHeader Then
                out(0, 0) = idHeader
                out(0, 1) = "Quality"
                out(0, 2) = "Mass"
                out(0, 3) = "Distance"
                out(0, 4) = "Inertia"
                r0 = 1
            End If

            For i As Integer = 0 To n - 1
                out(r0 + i, 0) = names(i)
                out(r0 + i, 1) = quality(i)
                out(r0 + i, 2) = mass(i)
                out(r0 + i, 3) = distance(i)
                out(r0 + i, 4) = inertia(i)
            Next

            Return PrepareResultTableForUdf(out)
        End Function

        ''' <summary>
        ''' Determines how many axes are actually available from a metric selector.
        ''' </summary>
        ''' <param name="preferredAxisCount">
        ''' The nominal number of axes requested by the caller, usually <c>model.Eigenvalues.Length</c>.
        ''' </param>
        ''' <param name="valueSelector">
        ''' Function that returns the metric vector for a zero-based axis index.
        ''' </param>
        ''' <returns>
        ''' The number of axes that can actually be retrieved without error.
        ''' </returns>
        ''' <remarks>
        ''' <para>
        ''' This helper protects the UDF output builders from mismatches between
        ''' the number of stored eigenvalues and the number of metric arrays
        ''' that are actually available in the model.
        ''' </para>
        ''' <para>
        ''' Example:
        ''' coordinates may be available for all axes, while cos² or contributions
        ''' may only be available for a subset of axes. In that case the output
        ''' should stop at the last retrievable axis instead of throwing.
        ''' </para>
        ''' </remarks>
        Private Function ResolveAvailableAxisCount(preferredAxisCount As Integer, valueSelector As Func(Of Integer, Double())) As Integer
            If preferredAxisCount < 1 OrElse valueSelector Is Nothing Then Return 0
            Dim available As Integer = 0
            For axisIdx As Integer = 0 To preferredAxisCount - 1
                Try
                    Dim values() As Double = valueSelector(axisIdx)
                    If values Is Nothing Then Exit For
                    available += 1
                Catch ex As Exception
                    Exit For
                End Try
            Next
            Return available
        End Function

        Private Function BuildCaAxisMetricOutput(idHeader As String, names() As String, axisPrefix As String,
                                                 axisCount As Integer, valueSelector As Func(Of Integer, Double()),
                                                 includeHeader As Boolean) As Object
            Dim effectiveAxisCount As Integer = ResolveAvailableAxisCount(axisCount, valueSelector)
            If effectiveAxisCount < 1 Then Return ExcelError.ExcelErrorNA

            Dim n As Integer = names.Length
            Dim out(n - 1 + If(includeHeader, 1, 0), effectiveAxisCount) As Object
            Dim r0 As Integer = 0

            If includeHeader Then
                out(0, 0) = idHeader
                For j As Integer = 0 To effectiveAxisCount - 1
                    out(0, j + 1) = axisPrefix & (j + 1).ToString(CultureInfo.InvariantCulture)
                Next
                r0 = 1
            End If

            For i As Integer = 0 To n - 1
                out(r0 + i, 0) = names(i)
            Next

            For j As Integer = 0 To effectiveAxisCount - 1
                Dim values() As Double = valueSelector(j)
                For i As Integer = 0 To n - 1
                    out(r0 + i, j + 1) = values(i)
                Next
            Next
            Return PrepareResultTableForUdf(out)
        End Function

        Private Function BuildMcaCategoryOverviewOutput(varNames() As String,
                                                        categories() As String,
                                                        quality() As Double,
                                                        mass() As Double,
                                                        distance() As Double,
                                                        inertia() As Double,
                                                        includeHeader As Boolean) As Object
            Dim n As Integer = categories.Length
            Dim out(n - 1 + If(includeHeader, 1, 0), 5) As Object
            Dim r0 As Integer = 0
            If includeHeader Then
                out(0, 0) = "Variable"
                out(0, 1) = "Category"
                out(0, 2) = "Quality"
                out(0, 3) = "Mass"
                out(0, 4) = "Distance"
                out(0, 5) = "Inertia"
                r0 = 1
            End If

            For i As Integer = 0 To n - 1
                out(r0 + i, 0) = varNames(i)
                out(r0 + i, 1) = categories(i)
                out(r0 + i, 2) = quality(i)
                out(r0 + i, 3) = mass(i)
                out(r0 + i, 4) = distance(i)
                out(r0 + i, 5) = inertia(i)
            Next

            Return PrepareResultTableForUdf(out)
        End Function

        Private Function BuildMcaCategoryAxisMetricOutput(varNames() As String, categories() As String, axisPrefix As String, axisCount As Integer,
                                                          valueSelector As Func(Of Integer, Double()), includeHeader As Boolean) As Object
            Dim effectiveAxisCount As Integer = ResolveAvailableAxisCount(axisCount, valueSelector)
            If effectiveAxisCount < 1 Then Return ExcelError.ExcelErrorNA

            Dim n As Integer = categories.Length
            Dim out(n - 1 + If(includeHeader, 1, 0), effectiveAxisCount + 1) As Object
            Dim r0 As Integer = 0

            If includeHeader Then
                out(0, 0) = "Variable"
                out(0, 1) = "Category"
                For j As Integer = 0 To effectiveAxisCount - 1
                    out(0, j + 2) = axisPrefix & (j + 1).ToString(CultureInfo.InvariantCulture)
                Next
                r0 = 1
            End If

            For i As Integer = 0 To n - 1
                out(r0 + i, 0) = varNames(i)
                out(r0 + i, 1) = categories(i)
            Next

            For j As Integer = 0 To effectiveAxisCount - 1
                Dim values() As Double = valueSelector(j)
                For i As Integer = 0 To n - 1
                    out(r0 + i, j + 2) = values(i)
                Next
            Next
            Return PrepareResultTableForUdf(out)
        End Function

        Private Function CombinedCategoryHeaders(varNames() As String, categories() As String) As String()
            Dim n As Integer = Math.Min(varNames.Length, categories.Length)
            Dim out(n - 1) As String
            For i As Integer = 0 To n - 1
                out(i) = varNames(i) & ": " & categories(i)
            Next
            Return out
        End Function

        Private Function BuildMcaBurtOutput(model As Multivariate.CA, includeHeader As Boolean) As Object
            If model Is Nothing OrElse model.BurtTable Is Nothing Then Return ExcelError.ExcelErrorNA
            Dim headers() As String = CombinedCategoryHeaders(model.BurtVarNames, model.rowNames)
            Dim n As Integer = model.BurtTable.GetLength(0)
            Dim out(n - 1 + If(includeHeader, 1, 0), n + 1) As Object
            Dim r0 As Integer = 0
            If includeHeader Then
                out(0, 0) = "Variable"
                out(0, 1) = "Category"
                For j As Integer = 0 To n - 1
                    out(0, j + 2) = headers(j)
                Next
                r0 = 1
            End If

            For i As Integer = 0 To n - 1
                out(r0 + i, 0) = model.BurtVarNames(i)
                out(r0 + i, 1) = model.rowNames(i)
                For j As Integer = 0 To n - 1
                    out(r0 + i, j + 2) = model.BurtTable(i, j)
                Next
            Next
            Return PrepareResultTableForUdf(out)
        End Function

        Private Function BuildMcaIndicatorOutput(model As Multivariate.CA, includeHeader As Boolean) As Object
            If model Is Nothing OrElse model.DesignMatrix Is Nothing Then Return ExcelError.ExcelErrorNA
            Dim headers() As String = CombinedCategoryHeaders(model.BurtVarNames, model.rowNames)
            Dim n As Integer = model.DesignMatrix.GetLength(0)
            Dim p As Integer = model.DesignMatrix.GetLength(1)
            Dim out(n - 1 + If(includeHeader, 1, 0), p) As Object
            Dim r0 As Integer = 0
            If includeHeader Then
                out(0, 0) = "Case"
                For j As Integer = 0 To p - 1
                    out(0, j + 1) = headers(j)
                Next
                r0 = 1
            End If

            For i As Integer = 0 To n - 1
                out(r0 + i, 0) = i + 1
                For j As Integer = 0 To p - 1
                    out(r0 + i, j + 1) = model.DesignMatrix(i, j)
                Next
            Next
            Return PrepareResultTableForUdf(out)
        End Function

        Private Function FindWrappedDiscriminantTable(model As Multivariate.DiscriminantAnalysis,
                                                      title As String,
                                                      Optional startsWith As Boolean = False) As Object(,)
            If model Is Nothing Then Return Nothing
            Dim tables As List(Of ResultTable) = model.wrapResults()
            If tables Is Nothing Then Return Nothing
            For Each t As ResultTable In tables
                Dim arr As Object(,) = t.returnSelf()
                If arr Is Nothing OrElse arr.GetLength(0) < 1 OrElse arr.GetLength(1) < 1 Then Continue For
                Dim candidate As String = CellToTrimmedText(arr(0, 0))
                If startsWith Then
                    If candidate.StartsWith(title, StringComparison.OrdinalIgnoreCase) Then Return arr
                Else
                    If String.Equals(candidate, title, StringComparison.OrdinalIgnoreCase) Then Return arr
                End If
            Next
            Return Nothing
        End Function

        Private Function BuildDiscriminantGroupSummaryTable(model As Multivariate.DiscriminantAnalysis) As Object(,)
            If model Is Nothing OrElse model.GroupStatistics Is Nothing Then Return Nothing
            Dim n As Integer = model.GroupStatistics.Count
            Dim out(n, 5) As Object
            out(0, 0) = "Group"
            out(0, 1) = "Count"
            out(0, 2) = "Prior"
            out(0, 3) = "Prior %"
            out(0, 4) = "LogDet (Working)"
            out(0, 5) = "Ridge Used"
            For i As Integer = 0 To n - 1
                Dim g = model.GroupStatistics(i)
                out(i + 1, 0) = g.GroupLabel
                out(i + 1, 1) = g.Count
                out(i + 1, 2) = g.PriorProbability
                out(i + 1, 3) = g.PriorProbability * 100.0R
                out(i + 1, 4) = g.LogDeterminantWorking
                out(i + 1, 5) = g.RegularizationUsed
            Next
            Return out
        End Function

        Private Function BuildDiscriminantMeansTable(model As Multivariate.DiscriminantAnalysis,
                                                     useOriginalScale As Boolean) As Object(,)
            If model Is Nothing OrElse model.GroupStatistics Is Nothing Then Return Nothing
            Dim g As Integer = model.GroupStatistics.Count
            If g < 1 Then Return Nothing
            Dim p As Integer = model.GroupStatistics(0).MeanOriginal.Length
            Dim out(g, p) As Object
            out(0, 0) = "Group"
            For j As Integer = 0 To p - 1
                out(0, j + 1) = model.PreparedData.VariableNames(j)
            Next
            For i As Integer = 0 To g - 1
                out(i + 1, 0) = model.GroupStatistics(i).GroupLabel
                Dim values() As Double = If(useOriginalScale, model.GroupStatistics(i).MeanOriginal, model.GroupStatistics(i).MeanWorking)
                For j As Integer = 0 To p - 1
                    out(i + 1, j + 1) = values(j)
                Next
            Next
            Return out
        End Function

        Private Function GetDiscriminantPredictionResult(model As Multivariate.DiscriminantAnalysis,
                                                         source As Object) As Multivariate.DiscriminantPredictionResult
            If model Is Nothing Then Return Nothing
            Dim token As String = NormalizeToken(source)
            Select Case token
                Case "", "TRAINING", "APPARENT", "RESUBSTITUTION"
                    Return model.TrainingClassification
                Case "VALIDATION", "CV", "CROSSVALIDATION"
                    Return model.ValidationClassification
                Case Else
                    Throw New ArgumentException("source must be training or validation.")
            End Select
        End Function

    End Module
End Namespace
