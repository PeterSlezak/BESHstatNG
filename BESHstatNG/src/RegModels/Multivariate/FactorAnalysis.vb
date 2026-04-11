Option Explicit On
Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports BESHStatNG.AppInfrastructure
Imports Microsoft.Office.Interop.Excel

Namespace Multivariate

    ''' <summary>
    ''' Specifies whether factor analysis is performed on a correlation matrix or on a covariance matrix.
    ''' </summary>
    Public Enum FactorAnalysisMatrixType
        ''' <summary>
        ''' Standardizes variables to unit variance and analyzes their correlation matrix.
        ''' </summary>
        Correlation = 0

        ''' <summary>
        ''' Centers variables but keeps their original scale and analyzes the covariance matrix.
        ''' </summary>
        Covariance = 1
    End Enum

    ''' <summary>
    ''' Specifies how rows containing missing or non-finite values are handled before factor extraction.
    ''' </summary>
    Public Enum FactorAnalysisMissingValuePolicy
        ''' <summary>
        ''' Stops the analysis as soon as a missing or non-finite value is encountered.
        ''' </summary>
        ErrorOnMissing = 0

        ''' <summary>
        ''' Removes any row containing at least one missing or non-finite value before fitting the model.
        ''' </summary>
        ListwiseDeletion = 1
    End Enum

    ''' <summary>
    ''' Specifies the common-factor extraction algorithm.
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' The implementation supports the major extraction families typically offered by standard statistical software
    ''' for exploratory factor analysis on continuous numeric data.
    ''' </para>
    ''' <list type="bullet">
    '''   <item><description><see cref="PrincipalComponents"/> computes component-style loadings from the full analysis matrix.</description></item>
    '''   <item><description><see cref="PrincipalAxis"/> performs iterative principal-axis factoring using reduced diagonals.</description></item>
    '''   <item><description><see cref="MaximumLikelihood"/> estimates uniquenesses by maximizing the Gaussian common-factor likelihood.</description></item>
    '''   <item><description><see cref="GeneralizedLeastSquares"/> minimizes a weighted residual criterion for the common-factor model.</description></item>
    '''   <item><description><see cref="Image"/> extracts factors from the image-correlation structure implied by the inverse correlation matrix.</description></item>
    '''   <item><description><see cref="Alpha"/> performs alpha factoring using iterative communality updates on the correlation scale.</description></item>
    ''' </list>
    ''' </remarks>
    Public Enum FactorAnalysisExtractionMethod
        ''' <summary>
        ''' Extracts factors/components directly from the full analysis matrix using its leading eigenpairs.
        ''' </summary>
        PrincipalComponents = 0

        ''' <summary>
        ''' Performs iterative principal-axis factoring using communalities on the reduced diagonal.
        ''' </summary>
        PrincipalAxis = 1

        ''' <summary>
        ''' Performs maximum-likelihood common-factor extraction.
        ''' </summary>
        MaximumLikelihood = 2

        ''' <summary>
        ''' Performs generalized-least-squares common-factor extraction.
        ''' </summary>
        GeneralizedLeastSquares = 3

        ''' <summary>
        ''' Performs image factor analysis using the image-correlation matrix.
        ''' </summary>
        Image = 4

        ''' <summary>
        ''' Performs alpha factor extraction.
        ''' </summary>
        Alpha = 5
    End Enum

    ''' <summary>
    ''' Specifies how the number of extracted factors is chosen.
    ''' </summary>
    Public Enum FactorAnalysisRetentionMethod
        ''' <summary>
        ''' Uses the exact number supplied through the retention parameter.
        ''' </summary>
        Fixed = 0

        ''' <summary>
        ''' Retains factors whose initial eigenvalue is greater than or equal to the supplied cutoff.
        ''' </summary>
        Eigenvalue = 1

        ''' <summary>
        ''' Retains the smallest number of factors whose cumulative initial variance explained reaches the supplied percentage.
        ''' </summary>
        Variance = 2
    End Enum

    ''' <summary>
    ''' Specifies how starting communalities are initialized for principal-axis factoring.
    ''' </summary>
    Public Enum FactorAnalysisCommunalityInitialization
        ''' <summary>
        ''' Uses squared multiple correlations as the starting communalities.
        ''' </summary>
        SquaredMultipleCorrelation = 0

        ''' <summary>
        ''' Uses the full variable variance on the working analysis scale as the starting communality.
        ''' </summary>
        One = 1
    End Enum

    ''' <summary>
    ''' Specifies the post-extraction rotation applied to the unrotated loading matrix.
    ''' </summary>
    Public Enum FactorAnalysisRotationMethod
        ''' <summary>
        ''' Leaves the factor solution unrotated.
        ''' </summary>
        None = 0

        ''' <summary>
        ''' Applies orthogonal varimax rotation.
        ''' </summary>
        Varimax = 1

        ''' <summary>
        ''' Applies orthogonal quartimax rotation.
        ''' </summary>
        Quartimax = 2

        ''' <summary>
        ''' Applies orthogonal equamax rotation.
        ''' </summary>
        Equamax = 3

        ''' <summary>
        ''' Applies oblique promax rotation after an initial varimax rotation.
        ''' </summary>
        Promax = 4
    End Enum

    ''' <summary>
    ''' Specifies how factor scores are estimated from the fitted factor model.
    ''' </summary>
    Public Enum FactorAnalysisScoreMethod
        ''' <summary>
        ''' Does not compute factor-score coefficients or observation-level factor scores.
        ''' </summary>
        None = 0

        ''' <summary>
        ''' Computes Thomson regression factor scores.
        ''' </summary>
        Regression = 1

        ''' <summary>
        ''' Computes Bartlett weighted least-squares factor scores.
        ''' </summary>
        Bartlett = 2
    End Enum

    ''' <summary>
    ''' Performs exploratory factor analysis on a rectangular numeric data matrix.
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' This class is designed to fit into the existing BESH-Stat-NG multivariate stack in the same spirit as
    ''' <see cref="PCA"/> and the clustering classes. It focuses on reusable analysis machinery rather than UI code.
    ''' </para>
    ''' <para>
    ''' End-to-end workflow:
    ''' </para>
    ''' <list type="number">
    '''   <item><description>Provide the raw matrix and row/column labels through <see cref="dataInputs(Double(,), Integer(), String(), String)"/>.</description></item>
    '''   <item><description>Configure extraction, retention, rotation, scoring, and missing-data behavior through <see cref="settingsInputs(Integer, Double, FactorAnalysisMatrixType, FactorAnalysisExtractionMethod, FactorAnalysisRetentionMethod, Double, FactorAnalysisRotationMethod, FactorAnalysisScoreMethod, FactorAnalysisCommunalityInitialization, FactorAnalysisMissingValuePolicy, Boolean, Double)"/>.</description></item>
    '''   <item><description>Run <see cref="Calculate"/>.</description></item>
    '''   <item><description>Consume the structured tabular output through <see cref="wrapResults"/> or use the exposed read-only properties directly.</description></item>
    ''' </list>
    ''' <para>
    ''' Mathematical conventions used by the implementation:
    ''' </para>
    ''' <list type="bullet">
    '''   <item><description>Rows are observations, columns are variables.</description></item>
    '''   <item><description>The working analysis matrix is either the sample correlation matrix or the sample covariance matrix.</description></item>
    '''   <item><description>Principal-axis factoring iteratively replaces the diagonal by communalities until convergence.</description></item>
    '''   <item><description>Orthogonal rotations preserve factor independence; promax returns both a pattern matrix and a factor-correlation matrix.</description></item>
    '''   <item><description>Reproduced matrices are formed as <c>Λ Φ Λ' + Ψ</c>, where <c>Λ</c> is the pattern matrix, <c>Φ</c> is the factor-correlation matrix, and <c>Ψ</c> is the diagonal uniqueness matrix.</description></item>
    ''' </list>
    ''' </remarks>
    Public Class FactorAnalysis

        ''' <summary>
        ''' Holds the outcome of a loading-rotation routine.
        ''' </summary>
        ''' <remarks>
        ''' The container carries the rotated pattern and structure matrices together with the
        ''' factor-correlation matrix, the accumulated transformation matrix, and convergence metadata.
        ''' </remarks>
        Private Class FactorRotationResult
            Public Loadings(,) As Double
            Public StructureMatrix(,) As Double
            Public Phi(,) As Double
            Public Transform(,) As Double
            Public Iterations As Integer
            Public Converged As Boolean
            Public Label As String
        End Class

        Private pData(,) As Double
        Private pRowNums() As Integer
        Private pVarNames() As String
        Private pAnalysisLabel As String

        Private pAnalysisData(,) As Double
        Private pAnalysisRowNums() As Integer
        Private pCenteredData(,) As Double
        Private pStandardizedData(,) As Double

        Private pn As Integer
        Private pp As Integer
        Private pRemovedRowCount As Integer

        Private pMatrixType As FactorAnalysisMatrixType = FactorAnalysisMatrixType.Correlation
        Private pExtractionMethod As FactorAnalysisExtractionMethod = FactorAnalysisExtractionMethod.PrincipalAxis
        Private pRetentionMethod As FactorAnalysisRetentionMethod = FactorAnalysisRetentionMethod.Fixed
        Private pRetentionValue As Double = 1.0
        Private pRotationMethod As FactorAnalysisRotationMethod = FactorAnalysisRotationMethod.None
        Private pScoreMethod As FactorAnalysisScoreMethod = FactorAnalysisScoreMethod.Regression
        Private pCommunalityInitialization As FactorAnalysisCommunalityInitialization = FactorAnalysisCommunalityInitialization.SquaredMultipleCorrelation
        Private pMissingValuePolicy As FactorAnalysisMissingValuePolicy = FactorAnalysisMissingValuePolicy.ErrorOnMissing
        Private pUseKaiserNormalization As Boolean = True
        Private pPromaxPower As Double = 4.0
        Private pMaxiter As Integer = 250
        Private pEps As Double = 0.000001

        Private pWorkingMatrix(,) As Double
        Private pCorrelationMatrix(,) As Double
        Private pInitialEigenvalues() As Double
        Private pInitialEigenvectors(,) As Double
        Private pExtractionEigenvalues() As Double
        Private pUnrotatedLoadings(,) As Double
        Private pPatternMatrix(,) As Double
        Private pStructureMatrix(,) As Double
        Private pPhi(,) As Double
        Private pRotationTransform(,) As Double
        Private pCommunalities() As Double
        Private pInitialCommunalities() As Double
        Private pUniquenesses() As Double
        Private pCommonVarianceMatrix(,) As Double
        Private pReproducedMatrix(,) As Double
        Private pResidualMatrix(,) As Double
        Private pScoreCoefficientMatrix(,) As Double
        Private pScores(,) As Double
        Private pNoFactors As Integer
        Private pIterationsUsed As Integer
        Private pConverged As Boolean
        Private pRotationConverged As Boolean
        Private pRotationIterations As Integer

        Private pKmoOverall As Double
        Private pKmoPerVariable() As Double
        Private pAntiImageCorrelation(,) As Double
        Private pBartlettChiSquare As Double
        Private pBartlettDf As Integer
        Private pBartlettPValue As Double
        Private pDeterminantCorrelation As Double
        Private pRmsr As Double
        Private pRotationLabel As String

        ''' <summary>
        ''' Stores the raw data matrix and labels that will be used when the analysis is run.
        ''' </summary>
        ''' <param name="arData">Numeric input matrix with observations in rows and variables in columns.</param>
        ''' <param name="arRowIds">Optional observation identifiers used in score tables. If missing or inconsistent, sequential identifiers are generated.</param>
        ''' <param name="arVarNames">Variable names corresponding to the matrix columns.</param>
        ''' <param name="strAnalysisLabel">Optional descriptive label echoed in output titles.</param>
        Public Sub dataInputs(arData(,) As Double,
                              arRowIds() As Integer,
                              arVarNames() As String,
                              Optional strAnalysisLabel As String = "")
            pData = arData
            pRowNums = arRowIds
            pVarNames = arVarNames
            pAnalysisLabel = strAnalysisLabel
        End Sub

        ''' <summary>
        ''' Configures how the factor analysis is fitted.
        ''' </summary>
        ''' <param name="maximumIteration">Maximum number of iterations used by extraction and rotation routines.</param>
        ''' <param name="dEps">Convergence tolerance used by iterative procedures.</param>
        ''' <param name="analyzedMatrixType">Determines whether the analysis is based on a correlation or covariance matrix.</param>
        ''' <param name="extractionMethod">Common-factor extraction algorithm.</param>
        ''' <param name="retentionMethod">Rule used to determine how many factors are retained.</param>
        ''' <param name="retentionValue">Parameter attached to the retention rule. It represents a count, an eigenvalue cutoff, or a target cumulative percent depending on <paramref name="retentionMethod"/>.</param>
        ''' <param name="rotationMethod">Post-extraction rotation applied to the loading matrix.</param>
        ''' <param name="scoreMethod">Factor-score estimator used for observation-level scores.</param>
        ''' <param name="communalityInitialization">Starting communality rule used by principal-axis factoring.</param>
        ''' <param name="missingValuePolicy">Policy used when missing or non-finite values are found in the raw matrix.</param>
        ''' <param name="useKaiserNormalization">If <c>True</c>, Kaiser row normalization is applied before orthomax-family rotations.</param>
        ''' <param name="promaxPower">Power used when constructing the promax target matrix. Standard software often defaults to 4.</param>
        Public Sub settingsInputs(Optional maximumIteration As Integer = 250,
                                  Optional dEps As Double = 0.000001,
                                  Optional analyzedMatrixType As FactorAnalysisMatrixType = FactorAnalysisMatrixType.Correlation,
                                  Optional extractionMethod As FactorAnalysisExtractionMethod = FactorAnalysisExtractionMethod.PrincipalAxis,
                                  Optional retentionMethod As FactorAnalysisRetentionMethod = FactorAnalysisRetentionMethod.Fixed,
                                  Optional retentionValue As Double = 1.0,
                                  Optional rotationMethod As FactorAnalysisRotationMethod = FactorAnalysisRotationMethod.None,
                                  Optional scoreMethod As FactorAnalysisScoreMethod = FactorAnalysisScoreMethod.Regression,
                                  Optional communalityInitialization As FactorAnalysisCommunalityInitialization = FactorAnalysisCommunalityInitialization.SquaredMultipleCorrelation,
                                  Optional missingValuePolicy As FactorAnalysisMissingValuePolicy = FactorAnalysisMissingValuePolicy.ErrorOnMissing,
                                  Optional useKaiserNormalization As Boolean = True,
                                  Optional promaxPower As Double = 4.0)

            pMaxiter = maximumIteration
            pEps = dEps
            pMatrixType = analyzedMatrixType
            pExtractionMethod = extractionMethod
            pRetentionMethod = retentionMethod
            pRetentionValue = retentionValue
            pRotationMethod = rotationMethod
            pScoreMethod = scoreMethod
            pCommunalityInitialization = communalityInitialization
            pMissingValuePolicy = missingValuePolicy
            pUseKaiserNormalization = useKaiserNormalization
            pPromaxPower = promaxPower
        End Sub

        ''' <summary>
        ''' Gets the numeric data matrix actually analyzed after missing-value handling.
        ''' </summary>
        Public ReadOnly Property AnalysisData() As Double(,)
            Get
                Return pAnalysisData
            End Get
        End Property

        ''' <summary>
        ''' Gets the observation identifiers corresponding to <see cref="AnalysisData"/>.
        ''' </summary>
        Public ReadOnly Property AnalysisRowIds() As Integer()
            Get
                Return pAnalysisRowNums
            End Get
        End Property

        ''' <summary>
        ''' Gets the centered analysis matrix used when the model is fitted on covariances.
        ''' </summary>
        Public ReadOnly Property CenteredData() As Double(,)
            Get
                Return pCenteredData
            End Get
        End Property

        ''' <summary>
        ''' Gets the standardized analysis matrix used when the model is fitted on correlations.
        ''' </summary>
        Public ReadOnly Property StandardizedData() As Double(,)
            Get
                Return pStandardizedData
            End Get
        End Property

        ''' <summary>
        ''' Gets the working correlation/covariance matrix that is factor-analyzed.
        ''' </summary>
        Public ReadOnly Property WorkingMatrix() As Double(,)
            Get
                Return pWorkingMatrix
            End Get
        End Property

        ''' <summary>
        ''' Gets the sample correlation matrix of the cleaned dataset.
        ''' </summary>
        ''' <remarks>
        ''' This matrix is always computed because several factorability diagnostics are defined on correlations,
        ''' even when extraction is performed on a covariance matrix.
        ''' </remarks>
        Public ReadOnly Property CorrelationMatrix() As Double(,)
            Get
                Return pCorrelationMatrix
            End Get
        End Property

        ''' <summary>
        ''' Gets the initial eigenvalues of the full working analysis matrix, sorted descending.
        ''' </summary>
        Public ReadOnly Property InitialEigenvalues() As Double()
            Get
                Return pInitialEigenvalues
            End Get
        End Property

        ''' <summary>
        ''' Gets the initial eigenvectors of the full working analysis matrix, ordered to match <see cref="InitialEigenvalues"/>.
        ''' </summary>
        Public ReadOnly Property InitialEigenvectors() As Double(,)
            Get
                Return pInitialEigenvectors
            End Get
        End Property

        ''' <summary>
        ''' Gets the unrotated loading matrix for the retained factors.
        ''' </summary>
        Public ReadOnly Property UnrotatedLoadings() As Double(,)
            Get
                Return pUnrotatedLoadings
            End Get
        End Property

        ''' <summary>
        ''' Gets the final factor-pattern matrix after the requested rotation.
        ''' </summary>
        Public ReadOnly Property PatternMatrix() As Double(,)
            Get
                Return pPatternMatrix
            End Get
        End Property

        ''' <summary>
        ''' Gets the final factor-structure matrix.
        ''' </summary>
        ''' <remarks>
        ''' For orthogonal solutions the structure matrix is identical to the pattern matrix.
        ''' For oblique promax solutions it equals <c>Pattern × Phi</c>.
        ''' </remarks>
        Public ReadOnly Property StructureMatrix() As Double(,)
            Get
                Return pStructureMatrix
            End Get
        End Property

        ''' <summary>
        ''' Gets the factor-correlation matrix Φ.
        ''' </summary>
        Public ReadOnly Property FactorCorrelationMatrix() As Double(,)
            Get
                Return pPhi
            End Get
        End Property

        ''' <summary>
        ''' Gets the linear transformation that maps the unrotated loadings to the final pattern matrix.
        ''' </summary>
        Public ReadOnly Property RotationTransform() As Double(,)
            Get
                Return pRotationTransform
            End Get
        End Property

        ''' <summary>
        ''' Gets the final communality estimates for each variable on the working analysis scale.
        ''' </summary>
        Public ReadOnly Property Communalities() As Double()
            Get
                Return pCommunalities
            End Get
        End Property

        ''' <summary>
        ''' Gets the starting communality values used by the extraction step.
        ''' </summary>
        Public ReadOnly Property InitialCommunalities() As Double()
            Get
                Return pInitialCommunalities
            End Get
        End Property

        ''' <summary>
        ''' Gets the final uniqueness estimates for each variable on the working analysis scale.
        ''' </summary>
        Public ReadOnly Property Uniquenesses() As Double()
            Get
                Return pUniquenesses
            End Get
        End Property

        ''' <summary>
        ''' Gets the common-variance matrix <c>Λ Φ Λ'</c>.
        ''' </summary>
        Public ReadOnly Property CommonVarianceMatrix() As Double(,)
            Get
                Return pCommonVarianceMatrix
            End Get
        End Property

        ''' <summary>
        ''' Gets the model-implied reproduced matrix <c>Λ Φ Λ' + Ψ</c>.
        ''' </summary>
        Public ReadOnly Property ReproducedMatrix() As Double(,)
            Get
                Return pReproducedMatrix
            End Get
        End Property

        ''' <summary>
        ''' Gets the residual matrix equal to observed working matrix minus reproduced matrix.
        ''' </summary>
        Public ReadOnly Property ResidualMatrix() As Double(,)
            Get
                Return pResidualMatrix
            End Get
        End Property

        ''' <summary>
        ''' Gets the factor-score coefficient matrix.
        ''' </summary>
        ''' <remarks>
        ''' If factor scores are disabled, this property remains <c>Nothing</c>.
        ''' </remarks>
        Public ReadOnly Property ScoreCoefficientMatrix() As Double(,)
            Get
                Return pScoreCoefficientMatrix
            End Get
        End Property

        ''' <summary>
        ''' Gets the observation-level factor scores.
        ''' </summary>
        Public ReadOnly Property Scores() As Double(,)
            Get
                Return pScores
            End Get
        End Property

        ''' <summary>
        ''' Gets the number of retained factors.
        ''' </summary>
        Public ReadOnly Property NumberOfFactors() As Integer
            Get
                Return pNoFactors
            End Get
        End Property

        ''' <summary>
        ''' Gets the extraction sum of squares for the retained factors.
        ''' </summary>
        Public ReadOnly Property ExtractionSumsOfSquares() As Double()
            Get
                Return pExtractionEigenvalues
            End Get
        End Property

        ''' <summary>
        ''' Gets the total number of rows removed by the missing-data policy.
        ''' </summary>
        Public ReadOnly Property RemovedRowCount() As Integer
            Get
                Return pRemovedRowCount
            End Get
        End Property

        ''' <summary>
        ''' Gets the maximum absolute off-diagonal residual root mean square (RMSR) summary of model misfit.
        ''' </summary>
        Public ReadOnly Property RMSR() As Double
            Get
                Return pRmsr
            End Get
        End Property

        ''' <summary>
        ''' Gets the overall Kaiser-Meyer-Olkin sampling adequacy statistic.
        ''' </summary>
        Public ReadOnly Property KmoOverall() As Double
            Get
                Return pKmoOverall
            End Get
        End Property

        ''' <summary>
        ''' Gets variable-wise KMO / MSA values.
        ''' </summary>
        Public ReadOnly Property KmoPerVariable() As Double()
            Get
                Return pKmoPerVariable
            End Get
        End Property

        ''' <summary>
        ''' Gets the anti-image correlation matrix derived from the inverse correlation matrix.
        ''' </summary>
        Public ReadOnly Property AntiImageCorrelationMatrix() As Double(,)
            Get
                Return pAntiImageCorrelation
            End Get
        End Property

        ''' <summary>
        ''' Gets Bartlett's test statistic for the null hypothesis that the correlation matrix is the identity matrix.
        ''' </summary>
        Public ReadOnly Property BartlettChiSquare() As Double
            Get
                Return pBartlettChiSquare
            End Get
        End Property

        ''' <summary>
        ''' Gets the degrees of freedom used by Bartlett's sphericity test.
        ''' </summary>
        Public ReadOnly Property BartlettDegreesOfFreedom() As Integer
            Get
                Return pBartlettDf
            End Get
        End Property

        ''' <summary>
        ''' Gets the upper-tail p-value for Bartlett's sphericity test.
        ''' </summary>
        Public ReadOnly Property BartlettPValue() As Double
            Get
                Return pBartlettPValue
            End Get
        End Property

        ''' <summary>
        ''' Gets the determinant of the cleaned-data correlation matrix.
        ''' </summary>
        Public ReadOnly Property DeterminantCorrelation() As Double
            Get
                Return pDeterminantCorrelation
            End Get
        End Property

        ''' <summary>
        ''' Gets a flag indicating whether the extraction step converged.
        ''' </summary>
        Public ReadOnly Property ExtractionConverged() As Boolean
            Get
                Return pConverged
            End Get
        End Property

        ''' <summary>
        ''' Gets a flag indicating whether the rotation step converged.
        ''' </summary>
        Public ReadOnly Property RotationConverged() As Boolean
            Get
                Return pRotationConverged
            End Get
        End Property

        ''' <summary>
        ''' Gets the number of extraction iterations used.
        ''' </summary>
        Public ReadOnly Property ExtractionIterations() As Integer
            Get
                Return pIterationsUsed
            End Get
        End Property

        ''' <summary>
        ''' Gets the number of rotation iterations used.
        ''' </summary>
        Public ReadOnly Property RotationIterations() As Integer
            Get
                Return pRotationIterations
            End Get
        End Property

        ''' <summary>
        ''' Gets display names for the retained factors.
        ''' </summary>
        ''' <param name="prefix">Optional factor-name prefix.</param>
        Public Function FactorNames(Optional prefix As String = "Factor ") As String()
            If pNoFactors <= 0 Then Return New String() {}
            Dim out(pNoFactors - 1) As String
            For i As Integer = 0 To pNoFactors - 1
                out(i) = prefix & CStr(i + 1)
            Next
            Return out
        End Function

        ''' <summary>
        ''' Creates a scree plot of the initial factor-analysis eigenvalue profile.
        ''' </summary>
        ''' <remarks>
        ''' The chart mirrors the default PCA scree output and labels each point by its percentage of total variance.
        ''' Run <see cref="Calculate"/> before calling this method.
        ''' </remarks>
        Public Sub screePlot()

            If pInitialEigenvalues Is Nothing OrElse pInitialEigenvalues.Length = 0 Then Exit Sub

            Dim factorAxis(pInitialEigenvalues.Length - 1) As Integer
            For i As Integer = 0 To pInitialEigenvalues.Length - 1
                factorAxis(i) = i + 1
            Next
            Dim initialPct() As Double = PercentOfTotal(pInitialEigenvalues, TotalVariance(pWorkingMatrix))

            AppGlobals.app.Charts.Add()
            With AppGlobals.app.ActiveWorkbook.ActiveChart
                .Name = "Scree Plot"
                .ChartType = XlChartType.xlXYScatter

                Do Until .SeriesCollection.Count = 0
                    .SeriesCollection(1).Delete
                Loop

                .SeriesCollection.NewSeries
                With .SeriesCollection(1)
                    .XValues = factorAxis
                    .Values = initialPct
                    .Name = "Initial Variance Explained"
                    .Format.Line.Weight = 1.5
                    .MarkerStyle = 8
                    .MarkerSize = 5
                    .Border.Color = RGB(100, 100, 100)
                    .MarkerForegroundColor = RGB(100, 100, 100)
                    .MarkerBackgroundColor = RGB(100, 100, 100)

                    For i As Integer = 0 To pInitialEigenvalues.Length - 1
                        .Points(i + 1).HasDataLabel = True
                        .Points(i + 1).DataLabel.Text = Format$(initialPct(i), "#0.0#")
                        .Points(i + 1).DataLabel.Position = XlDataLabelPosition.xlLabelPositionAbove
                        .Points(i + 1).DataLabel.Font.Size = 12
                    Next
                End With

                Try
                    .Legend.Delete()
                Catch
                End Try

                .Axes(XlAxisType.xlValue, XlAxisGroup.xlPrimary).HasTitle = False
                .Axes(XlAxisType.xlValue, XlAxisGroup.xlPrimary).HasTitle = True
                .Axes(XlAxisType.xlValue, XlAxisGroup.xlPrimary).AxisTitle.Text = "Variance explained [%]"
                .Axes(XlAxisType.xlValue, XlAxisGroup.xlPrimary).AxisTitle.Font.Size = 16
                .Axes(XlAxisType.xlValue, XlAxisGroup.xlPrimary).TickLabels.Font.Size = 14
                .Axes(XlAxisType.xlCategory, XlAxisGroup.xlPrimary).HasTitle = False
                .Axes(XlAxisType.xlCategory, XlAxisGroup.xlPrimary).HasTitle = True
                .Axes(XlAxisType.xlCategory, XlAxisGroup.xlPrimary).AxisTitle.Text = "Factor"
                .Axes(XlAxisType.xlCategory, XlAxisGroup.xlPrimary).AxisTitle.Font.Size = 16
                .Axes(XlAxisType.xlCategory, XlAxisGroup.xlPrimary).TickLabels.Font.Size = 14
                .HasTitle = False
                .HasTitle = True
                .ChartTitle.Text = "Scree Plot"
                .ChartTitle.Font.Size = 18
                .ChartTitle.Font.Bold = True
            End With

        End Sub

        ''' <summary>
        ''' Creates a 2D scatter plot of variable loadings on the first two retained factors.
        ''' </summary>
        ''' <remarks>
        ''' The plot uses the rotated pattern matrix and mirrors the PCA loading-plot style.
        ''' Run <see cref="Calculate"/> before calling this method.
        ''' </remarks>
        Public Sub loadingPlot2D()

            If pPatternMatrix Is Nothing OrElse pNoFactors < 2 Then Exit Sub

            Dim f1() As Double = Matrix.GetColumnFrom2Darray(pPatternMatrix, 0)
            Dim f2() As Double = Matrix.GetColumnFrom2Darray(pPatternMatrix, 1)
            Dim factorPct() As Double = PercentOfTotal(ColumnSumsOfSquares(pPatternMatrix, pStructureMatrix), TotalVariance(pWorkingMatrix))
            Dim factorNames() As String = Me.FactorNames()

            Dim scl1 As Double = Math.Max(Math.Abs(f1.Min()), Math.Abs(f1.Max()))
            Dim scl2 As Double = Math.Max(Math.Abs(f2.Min()), Math.Abs(f2.Max()))
            Dim udAxisX As graphics.CHARTscale = graphics.ChartScaling(-scl1, scl1)
            Dim udAxisY As graphics.CHARTscale = graphics.ChartScaling(-scl2, scl2)

            AppGlobals.app.Charts.Add()
            With AppGlobals.app.ActiveWorkbook.ActiveChart
                .Name = "Factor Loadings Plot2D"
                .ChartType = XlChartType.xlXYScatter

                Do Until .SeriesCollection.Count = 0
                    .SeriesCollection(1).Delete
                Loop

                With .Axes(XlAxisType.xlCategory)
                    .MinimumScale = udAxisX.Min
                    .MaximumScale = udAxisX.Max
                    .MajorUnit = udAxisX.Scale
                    .CrossesAt = -1.0E+100
                    .MajorTickMark = XlTickMark.xlTickMarkOutside
                    .MajorGridlines.Delete
                End With
                With .Axes(XlAxisType.xlValue)
                    .CrossesAt = -1.0E+100
                    .MinimumScale = udAxisY.Min
                    .MaximumScale = udAxisY.Max
                    .MajorUnit = udAxisY.Scale
                    .MajorTickMark = XlTickMark.xlTickMarkOutside
                    .MajorGridlines.Delete
                End With

                Dim seriesId As Integer = 0
                For id As Integer = 0 To pp - 1
                    .SeriesCollection.NewSeries
                    seriesId += 1
                    With .SeriesCollection(seriesId)
                        .ChartType = XlChartType.xlXYScatterLinesNoMarkers
                        .XValues = {0, f1(id)}
                        .Values = {0, f2(id)}
                        .Name = "Loading_" & CStr(id)
                        .Format.Line.Weight = 1
                        .Format.Line.Visible = True
                        .Format.Line.ForeColor.RGB = RGB(0, 0, 150)
                        .Format.Line.EndArrowheadStyle = 2

                        .Points(2).HasDataLabel = True
                        .Points(2).DataLabel.Text = CStr(pVarNames(id))
                        .Points(2).DataLabel.Position = XlDataLabelPosition.xlLabelPositionAbove
                        .Points(2).DataLabel.Font.Size = 11
                        .Points(2).DataLabel.Font.Color = RGB(0, 0, 150)
                    End With
                Next

                .SeriesCollection.NewSeries
                seriesId += 1
                With .SeriesCollection(seriesId)
                    .XValues = {udAxisX.Min, udAxisX.Max}
                    .Values = {0, 0}
                    .Name = "Y Zero Line"
                    .MarkerStyle = -4142
                    .Border.Color = RGB(0, 0, 0)
                    With .Format.Line
                        .Visible = True
                        .Weight = 1
                    End With
                End With
                .SeriesCollection.NewSeries
                seriesId += 1
                With .SeriesCollection(seriesId)
                    .XValues = {0, 0}
                    .Values = {udAxisY.Min, udAxisY.Max}
                    .Name = "X Zero Line"
                    .MarkerStyle = -4142
                    .Border.Color = RGB(0, 0, 0)
                    With .Format.Line
                        .Visible = True
                        .Weight = 1
                    End With
                End With

                Try
                    .Legend.Delete()
                Catch
                End Try

                .Axes(XlAxisType.xlValue, XlAxisGroup.xlPrimary).HasTitle = False
                .Axes(XlAxisType.xlValue, XlAxisGroup.xlPrimary).HasTitle = True
                .Axes(XlAxisType.xlValue, XlAxisGroup.xlPrimary).AxisTitle.Text = $"{factorNames(1)} [{Format$(factorPct(1), "#0.0#")}%]"
                .Axes(XlAxisType.xlValue, XlAxisGroup.xlPrimary).AxisTitle.Font.Size = 16
                .Axes(XlAxisType.xlValue, XlAxisGroup.xlPrimary).TickLabels.Font.Size = 14
                .Axes(XlAxisType.xlCategory, XlAxisGroup.xlPrimary).HasTitle = False
                .Axes(XlAxisType.xlCategory, XlAxisGroup.xlPrimary).HasTitle = True
                .Axes(XlAxisType.xlCategory, XlAxisGroup.xlPrimary).AxisTitle.Text = $"{factorNames(0)} [{Format$(factorPct(0), "#0.0#")}%]"
                .Axes(XlAxisType.xlCategory, XlAxisGroup.xlPrimary).AxisTitle.Font.Size = 16
                .Axes(XlAxisType.xlCategory, XlAxisGroup.xlPrimary).TickLabels.Font.Size = 14
                .HasTitle = False
                .HasTitle = True
                .ChartTitle.Text = "Factor Loadings Plot"
                .ChartTitle.Font.Size = 18
                .ChartTitle.Font.Bold = True
            End With

        End Sub

        ''' <summary>
        ''' Creates a 3D scatter plot of variable loadings on the first three retained factors.
        ''' </summary>
        ''' <remarks>
        ''' The plot uses the rotated pattern matrix and mirrors the PCA loading-plot style.
        ''' Run <see cref="Calculate"/> before calling this method.
        ''' </remarks>
        Public Sub loadingPlot3D()

            If pPatternMatrix Is Nothing OrElse pNoFactors < 3 Then Exit Sub

            Dim factorPct() As Double = PercentOfTotal(ColumnSumsOfSquares(pPatternMatrix, pStructureMatrix), TotalVariance(pWorkingMatrix))
            Dim factorNames() As String = Me.FactorNames()
            Dim XYZ As New graphics.XYZscatter
            Dim f1() As Double = Matrix.GetColumnFrom2Darray(pPatternMatrix, 0)
            Dim f2() As Double = Matrix.GetColumnFrom2Darray(pPatternMatrix, 1)
            Dim f3() As Double = Matrix.GetColumnFrom2Darray(pPatternMatrix, 2)

            With XYZ
                .ChartName = "Factor Loadings Plot3D"
                .dataInputs(f1, f2, f3)
                .axesLabelInputs($"{factorNames(0)} [{Format$(factorPct(0), "#0.0#")}%]",
                                 $"{factorNames(1)} [{Format$(factorPct(1), "#0.0#")}%]",
                                 $"{factorNames(2)} [{Format$(factorPct(2), "#0.0#")}%]")
                .showPlanePointInputs(True, True, True, 3, 3, 3)
                .ScaleAxis(False)
                .settingsInputs(True, True, True)
                .SetDataLabels(pVarNames)
                .draw()
            End With

        End Sub

        ''' <summary>
        ''' Returns the per-factor contributions that sum to each variable communality.
        ''' </summary>
        ''' <remarks>
        ''' For orthogonal solutions this reduces to squared loadings. For oblique solutions it uses the
        ''' element-wise product of the pattern and structure matrices so that the row sums equal the extracted communalities.
        ''' </remarks>
        Public Function CommunalityContributionsByFactor() As Double(,)
            If pPatternMatrix Is Nothing OrElse pStructureMatrix Is Nothing Then Return Nothing

            Dim out(pPatternMatrix.GetLength(0) - 1, pPatternMatrix.GetLength(1) - 1) As Double
            For i As Integer = 0 To pPatternMatrix.GetLength(0) - 1
                For j As Integer = 0 To pPatternMatrix.GetLength(1) - 1
                    out(i, j) = pPatternMatrix(i, j) * pStructureMatrix(i, j)
                Next
            Next
            Return out
        End Function

        ''' <summary>
        ''' Fits the configured exploratory factor model.
        ''' </summary>
        ''' <remarks>
        ''' <para>
        ''' This method validates inputs, handles missing data, builds centered and standardized working representations,
        ''' computes factorability diagnostics, determines the number of factors, extracts the unrotated solution,
        ''' applies the requested rotation, reconstructs the reproduced matrix, derives residuals, and optionally
        ''' computes factor-score coefficients and scores.
        ''' </para>
        ''' </remarks>
        Public Sub Calculate()
            ValidateSettings()
            PrepareAnalysisDataset()
            BuildWorkingRepresentations()
            ComputeFactorabilityDiagnostics()
            ComputeInitialEigenpairs()
            DetermineNumberOfFactors()
            ExtractFactors()
            RotateFactors()
            BuildModelMatrices()
            ComputeFactorScores()
        End Sub

        ''' <summary>
        ''' Wraps the current analysis into report-oriented result tables suitable for the existing output pipeline.
        ''' </summary>
        ''' <returns>A list of <see cref="ResultTable"/> objects containing diagnostics, loadings, matrices, and optional scores.</returns>
        Public Function wrapResults() As List(Of ResultTable)
            Dim out As New List(Of ResultTable)
            Dim t As ResultTable
            Dim factorNames() As String = Me.FactorNames()
            Dim titleSuffix As String = If(String.IsNullOrWhiteSpace(pAnalysisLabel), "", $" - {pAnalysisLabel}")

            ' Summary
            t = New ResultTable
            t.AddTitle($"Factor Analysis Summary{titleSuffix}")
            t.AddHeaderTopRow({"Statistic", "Value"})
            Dim summary(15, 1) As Object
            summary(0, 0) = "Analysis matrix"
            summary(0, 1) = pMatrixType.ToString()
            summary(1, 0) = "Extraction method"
            summary(1, 1) = pExtractionMethod.ToString()
            summary(2, 0) = "Rotation method"
            summary(2, 1) = pRotationMethod.ToString()
            summary(3, 0) = "Score method"
            summary(3, 1) = pScoreMethod.ToString()
            summary(4, 0) = "Rows analyzed"
            summary(4, 1) = pn
            summary(5, 0) = "Rows removed"
            summary(5, 1) = pRemovedRowCount
            summary(6, 0) = "Variables"
            summary(6, 1) = pp
            summary(7, 0) = "Retained factors"
            summary(7, 1) = pNoFactors
            summary(8, 0) = "Extraction converged"
            summary(8, 1) = pConverged
            summary(9, 0) = "Extraction iterations"
            summary(9, 1) = pIterationsUsed
            summary(10, 0) = "Rotation converged"
            summary(10, 1) = pRotationConverged
            summary(11, 0) = "Rotation iterations"
            summary(11, 1) = pRotationIterations
            summary(12, 0) = "Overall KMO"
            summary(12, 1) = pKmoOverall
            summary(13, 0) = "Bartlett Chi-square"
            summary(13, 1) = pBartlettChiSquare
            summary(14, 0) = "Bartlett p-value"
            summary(14, 1) = pBartlettPValue
            summary(15, 0) = "RMSR"
            summary(15, 1) = pRmsr
            t.SetBody(summary)
            t.AddPvalueToFormat(1)
            out.Add(t)

            ' Working matrix
            t = New ResultTable
            t.AddTitle(If(pMatrixType = FactorAnalysisMatrixType.Correlation, "Correlation Matrix", "Covariance Matrix"))
            t.AddHeaderTopRow(pVarNames)
            t.AddHeaderLeftRow(pVarNames)
            t.SetBody(pWorkingMatrix)
            out.Add(t)

            ' Factorability diagnostics
            t = New ResultTable
            t.AddTitle("Factorability Diagnostics")
            t.AddHeaderTopRow({"Statistic", "Value"})
            Dim factDiag(4, 1) As Object
            factDiag(0, 0) = "Determinant of correlation matrix"
            factDiag(0, 1) = pDeterminantCorrelation
            factDiag(1, 0) = "Overall KMO"
            factDiag(1, 1) = pKmoOverall
            factDiag(2, 0) = "Bartlett Chi-square"
            factDiag(2, 1) = pBartlettChiSquare
            factDiag(3, 0) = "Bartlett df"
            factDiag(3, 1) = pBartlettDf
            factDiag(4, 0) = "Bartlett p-value"
            factDiag(4, 1) = pBartlettPValue
            t.SetBody(factDiag)
            t.AddPvalueToFormat(1)
            out.Add(t)

            ' MSA / anti-image diagonal table
            t = New ResultTable
            t.AddTitle("KMO / Measure of Sampling Adequacy by Variable")
            t.AddHeaderTopRow({"Variable", "MSA"})
            Dim msa(pp - 1, 1) As Object
            For i As Integer = 0 To pp - 1
                msa(i, 0) = pVarNames(i)
                msa(i, 1) = pKmoPerVariable(i)
            Next
            t.SetBody(msa)
            out.Add(t)

            ' Anti-image
            t = New ResultTable
            t.AddTitle("Anti-Image Correlation Matrix")
            t.AddHeaderTopRow(pVarNames)
            t.AddHeaderLeftRow(pVarNames)
            t.SetBody(pAntiImageCorrelation)
            out.Add(t)

            ' Variance explained
            t = New ResultTable
            t.AddTitle("Variance Explained")
            t.AddHeaderTopRow({"Factor #", "Initial Eigenvalue", "Initial %", "Initial Cumulative %", "Extraction SS Loadings", "Extraction %", "Extraction Cumulative %", "Rotation SS Loadings", "Rotation %", "Rotation Cumulative %"})
            Dim initPct() As Double = PercentOfTotal(pInitialEigenvalues, TotalVariance(pWorkingMatrix))
            Dim initCum() As Double = Cumulative(initPct)
            Dim extractionSs() As Double = ColumnSumsOfSquares(pUnrotatedLoadings, pUnrotatedLoadings)
            Dim extractionPct() As Double = PercentOfTotal(extractionSs, TotalVariance(pWorkingMatrix))
            Dim extractionCum() As Double = Cumulative(extractionPct)
            Dim rotatedSs() As Double = ColumnSumsOfSquares(pPatternMatrix, pStructureMatrix)
            Dim rotatedPct() As Double = PercentOfTotal(rotatedSs, TotalVariance(pWorkingMatrix))
            Dim rotatedCum() As Double = Cumulative(rotatedPct)
            Dim varianceTable(Math.Max(pp, pNoFactors) - 1, 9) As Object
            For i As Integer = 0 To varianceTable.GetUpperBound(0)
                varianceTable(i, 0) = i + 1
                If i < pInitialEigenvalues.Length Then
                    varianceTable(i, 1) = pInitialEigenvalues(i)
                    varianceTable(i, 2) = initPct(i)
                    varianceTable(i, 3) = initCum(i)
                End If
                If i < pNoFactors Then
                    varianceTable(i, 4) = extractionSs(i)
                    varianceTable(i, 5) = extractionPct(i)
                    varianceTable(i, 6) = extractionCum(i)
                    varianceTable(i, 7) = rotatedSs(i)
                    varianceTable(i, 8) = rotatedPct(i)
                    varianceTable(i, 9) = rotatedCum(i)
                End If
            Next
            t.SetBody(varianceTable)
            out.Add(t)

            ' Communalities
            t = New ResultTable
            t.AddTitle("Communalities")
            Dim factorContributionHeader As New List(Of String)
            factorContributionHeader.Add("Variable")
            factorContributionHeader.Add("Initial")
            For Each fname As String In factorNames
                factorContributionHeader.Add($"{fname} Contribution")
            Next
            factorContributionHeader.Add("Extracted")
            factorContributionHeader.Add("Uniqueness")
            t.AddHeaderTopRow(factorContributionHeader.ToArray())
            Dim factorContrib(,) As Double = Me.CommunalityContributionsByFactor()
            Dim comm(pp - 1, pNoFactors + 3) As Object
            For i As Integer = 0 To pp - 1
                comm(i, 0) = pVarNames(i)
                comm(i, 1) = pInitialCommunalities(i)
                For j As Integer = 0 To pNoFactors - 1
                    comm(i, j + 2) = factorContrib(i, j)
                Next
                comm(i, pNoFactors + 2) = pCommunalities(i)
                comm(i, pNoFactors + 3) = pUniquenesses(i)
            Next
            t.SetBody(comm)
            out.Add(t)

            ' Unrotated loadings
            t = New ResultTable
            t.AddTitle("Unrotated Loadings")
            t.AddHeaderTopRow(Matrix.ConcatArrays({"Variable"}, factorNames))
            Dim unl(pp - 1, pNoFactors) As Object
            For i As Integer = 0 To pp - 1
                unl(i, 0) = pVarNames(i)
                For j As Integer = 0 To pNoFactors - 1
                    unl(i, j + 1) = pUnrotatedLoadings(i, j)
                Next
            Next
            t.SetBody(unl)
            out.Add(t)

            ' Pattern matrix
            t = New ResultTable
            t.AddTitle($"Rotated Pattern Matrix ({pRotationLabel})")
            t.AddHeaderTopRow(Matrix.ConcatArrays({"Variable"}, factorNames))
            Dim pat(pp - 1, pNoFactors) As Object
            For i As Integer = 0 To pp - 1
                pat(i, 0) = pVarNames(i)
                For j As Integer = 0 To pNoFactors - 1
                    pat(i, j + 1) = pPatternMatrix(i, j)
                Next
            Next
            t.SetBody(pat)
            out.Add(t)

            ' Structure matrix (only distinct for oblique rotation, but emitting it consistently simplifies downstream UI wiring)
            t = New ResultTable
            t.AddTitle($"Structure Matrix ({pRotationLabel})")
            t.AddHeaderTopRow(Matrix.ConcatArrays({"Variable"}, factorNames))
            Dim structTbl(pp - 1, pNoFactors) As Object
            For i As Integer = 0 To pp - 1
                structTbl(i, 0) = pVarNames(i)
                For j As Integer = 0 To pNoFactors - 1
                    structTbl(i, j + 1) = pStructureMatrix(i, j)
                Next
            Next
            t.SetBody(structTbl)
            out.Add(t)

            ' Factor correlation matrix
            t = New ResultTable
            t.AddTitle("Factor Correlation Matrix")
            t.AddHeaderTopRow(factorNames)
            t.AddHeaderLeftRow(factorNames)
            t.SetBody(pPhi)
            out.Add(t)

            ' Rotation transform
            t = New ResultTable
            t.AddTitle("Rotation Transformation Matrix")
            t.AddHeaderTopRow(factorNames)
            t.AddHeaderLeftRow(factorNames)
            t.SetBody(pRotationTransform)
            out.Add(t)

            ' Score coefficients
            If pScoreCoefficientMatrix IsNot Nothing Then
                t = New ResultTable
                t.AddTitle($"Factor Score Coefficients ({pScoreMethod})")
                t.AddHeaderTopRow(factorNames)
                t.AddHeaderLeftRow(pVarNames)
                t.SetBody(pScoreCoefficientMatrix)
                out.Add(t)
            End If

            ' Scores
            If pScores IsNot Nothing Then
                t = New ResultTable
                t.AddTitle($"Factor Scores ({pScoreMethod})")
                t.AddHeaderTopRow(Matrix.ConcatArrays({"Observation"}, factorNames))
                Dim scoreTbl(pn - 1, pNoFactors) As Object
                For i As Integer = 0 To pn - 1
                    scoreTbl(i, 0) = pAnalysisRowNums(i)
                    For j As Integer = 0 To pNoFactors - 1
                        scoreTbl(i, j + 1) = pScores(i, j)
                    Next
                Next
                t.SetBody(scoreTbl)
                out.Add(t)
            End If

            ' Reproduced matrix
            t = New ResultTable
            t.AddTitle("Reproduced Matrix")
            t.AddHeaderTopRow(pVarNames)
            t.AddHeaderLeftRow(pVarNames)
            t.SetBody(pReproducedMatrix)
            out.Add(t)

            ' Residual matrix
            t = New ResultTable
            t.AddTitle("Residual Matrix")
            t.AddHeaderTopRow(pVarNames)
            t.AddHeaderLeftRow(pVarNames)
            t.SetBody(pResidualMatrix)
            out.Add(t)

            Return out
        End Function

        ''' <summary>
        ''' Validates that the supplied inputs and configuration are internally consistent before fitting begins.
        ''' </summary>
        Private Sub ValidateSettings()
            If pData Is Nothing Then AppGlobals.BSerr.LogAndThrow(New InvalidOperationException("No input data supplied to FactorAnalysis.dataInputs."))
            If pVarNames Is Nothing Then AppGlobals.BSerr.LogAndThrow(New InvalidOperationException("No variable names supplied to FactorAnalysis.dataInputs."))
            If pData.GetLength(1) <> pVarNames.Length Then AppGlobals.BSerr.LogAndThrow(New ArgumentException("The number of variable names must match the number of columns in the input matrix."))
            If pMaxiter < 1 Then AppGlobals.BSerr.LogAndThrow(New ArgumentOutOfRangeException(NameOf(pMaxiter), "maximumIteration must be >= 1."))
            If pEps <= 0 Then AppGlobals.BSerr.LogAndThrow(New ArgumentOutOfRangeException(NameOf(pEps), "dEps must be > 0."))
            If pPromaxPower <= 1.0 Then AppGlobals.BSerr.LogAndThrow(New ArgumentOutOfRangeException(NameOf(pPromaxPower), "promaxPower must be > 1."))
        End Sub

        ''' <summary>
        ''' Applies the missing-value policy and constructs the cleaned numeric matrix that will actually be analyzed.
        ''' </summary>
        ''' <remarks>
        ''' Row identifiers are carried forward so that downstream score tables stay aligned with the surviving cases.
        ''' </remarks>
        Private Sub PrepareAnalysisDataset()
            Dim nRaw As Integer = pData.GetLength(0)
            Dim pRaw As Integer = pData.GetLength(1)
            Dim rowIds() As Integer = PrepareRowIds(nRaw)
            Dim keptRows As New List(Of Integer)

            For i As Integer = 0 To nRaw - 1
                Dim rowHasMissing As Boolean = False
                For j As Integer = 0 To pRaw - 1
                    Dim v As Double = pData(i, j)
                    If Double.IsNaN(v) OrElse Double.IsInfinity(v) Then
                        rowHasMissing = True
                        Exit For
                    End If
                Next

                If rowHasMissing Then
                    If pMissingValuePolicy = FactorAnalysisMissingValuePolicy.ErrorOnMissing Then
                        AppGlobals.BSerr.LogAndThrow(New ArgumentException($"Missing or non-finite value detected in row {i + 1}."))
                    End If
                Else
                    keptRows.Add(i)
                End If
            Next

            If keptRows.Count = 0 Then AppGlobals.BSerr.LogAndThrow(New ArgumentException("No complete rows remain after applying the missing-value policy."))
            If keptRows.Count < 3 Then AppGlobals.BSerr.LogAndThrow(New ArgumentException("Factor analysis requires at least three complete observations."))

            ReDim pAnalysisData(keptRows.Count - 1, pRaw - 1)
            ReDim pAnalysisRowNums(keptRows.Count - 1)
            For i As Integer = 0 To keptRows.Count - 1
                Dim srcRow As Integer = keptRows(i)
                pAnalysisRowNums(i) = rowIds(srcRow)
                For j As Integer = 0 To pRaw - 1
                    pAnalysisData(i, j) = pData(srcRow, j)
                Next
            Next

            pRemovedRowCount = nRaw - keptRows.Count
            pn = keptRows.Count
            pp = pRaw
        End Sub

        ''' <summary>
        ''' Normalizes the observation identifiers used by the analysis tables.
        ''' </summary>
        ''' <param name="nRows">Number of rows in the raw input matrix.</param>
        ''' <returns>The supplied identifiers when valid; otherwise a sequential 1-based identifier vector.</returns>
        Private Function PrepareRowIds(nRows As Integer) As Integer()
            Dim out(nRows - 1) As Integer
            If pRowNums IsNot Nothing AndAlso pRowNums.Length = nRows Then
                For i As Integer = 0 To nRows - 1
                    out(i) = pRowNums(i)
                Next
            Else
                For i As Integer = 0 To nRows - 1
                    out(i) = i + 1
                Next
            End If
            Return out
        End Function

        ''' <summary>
        ''' Builds centered and standardized versions of the cleaned data and derives the working analysis matrix.
        ''' </summary>
        ''' <remarks>
        ''' The correlation matrix is always computed because several diagnostics are defined on correlations even
        ''' when the extraction itself is requested on the covariance scale.
        ''' </remarks>
        Private Sub BuildWorkingRepresentations()
            ReDim pCenteredData(pn - 1, pp - 1)
            ReDim pStandardizedData(pn - 1, pp - 1)

            For j As Integer = 0 To pp - 1
                Dim col() As Double = Matrix.GetColumnFrom2Darray(pAnalysisData, j)
                Dim centered() As Double = MultivariateShared.Center(col)
                Dim standardized() As Double = MultivariateShared.Standardize(col)
                For i As Integer = 0 To pn - 1
                    pCenteredData(i, j) = centered(i)
                    pStandardizedData(i, j) = standardized(i)
                Next
            Next

            pCorrelationMatrix = Matrix.MatCovar(pStandardizedData)
            If pMatrixType = FactorAnalysisMatrixType.Correlation Then
                pWorkingMatrix = DirectCast(pCorrelationMatrix.Clone(), Double(,))
            Else
                pWorkingMatrix = Matrix.MatCovar(pCenteredData)
            End If
        End Sub

        ''' <summary>
        ''' Computes correlation-based factorability diagnostics such as KMO, anti-image correlations, and Bartlett's test.
        ''' </summary>
        Private Sub ComputeFactorabilityDiagnostics()
            Dim invR(,) As Double = MultivariateShared.SafeInverse(pCorrelationMatrix, preferCholesky:=True)
            Dim pDiag() As Double = New Double(pp - 1) {}
            ReDim pAntiImageCorrelation(pp - 1, pp - 1)
            ReDim pKmoPerVariable(pp - 1)

            For i As Integer = 0 To pp - 1
                pDiag(i) = Math.Max(Math.Abs(invR(i, i)), pEps)
            Next

            Dim sumR2 As Double = 0.0
            Dim sumP2 As Double = 0.0

            For i As Integer = 0 To pp - 1
                Dim rowR2 As Double = 0.0
                Dim rowP2 As Double = 0.0
                For j As Integer = 0 To pp - 1
                    If i = j Then
                        pAntiImageCorrelation(i, j) = 1.0
                    Else
                        Dim part As Double = -invR(i, j) / Math.Sqrt(pDiag(i) * pDiag(j))
                        pAntiImageCorrelation(i, j) = part
                        Dim r2 As Double = pCorrelationMatrix(i, j) * pCorrelationMatrix(i, j)
                        Dim p2 As Double = part * part
                        rowR2 += r2
                        rowP2 += p2
                        If j > i Then
                            sumR2 += r2
                            sumP2 += p2
                        End If
                    End If
                Next
                pKmoPerVariable(i) = If(rowR2 + rowP2 <= 0.0, 0.0, rowR2 / (rowR2 + rowP2))
            Next

            pKmoOverall = If(sumR2 + sumP2 <= 0.0, 0.0, sumR2 / (sumR2 + sumP2))
            pDeterminantCorrelation = Math.Max(Matrix.MDeterm(DirectCast(pCorrelationMatrix.Clone(), Double(,))), 0.0)
            pBartlettDf = CInt(pp * (pp - 1) / 2)
            If pDeterminantCorrelation <= 0.0 Then
                pBartlettChiSquare = Double.PositiveInfinity
                pBartlettPValue = 0.0
            Else
                pBartlettChiSquare = -(pn - 1 - (2 * pp + 5) / 6.0) * Math.Log(pDeterminantCorrelation)
                pBartlettPValue = 1.0 - distributions.ChiSquareCDF(pBartlettChiSquare, pBartlettDf)
            End If
        End Sub

        ''' <summary>
        ''' Eigen-decomposes the full working analysis matrix and stores the eigenpairs in descending order.
        ''' </summary>
        Private Sub ComputeInitialEigenpairs()
            Dim raw = Matrix.EIGEN_JK(pWorkingMatrix, pMaxiter, pEps)
            Dim sorted = MultivariateShared.SortEigenpairsDescending(raw.Item1, raw.Item2)
            pInitialEigenvalues = sorted.Item1
            pInitialEigenvectors = sorted.Item2
        End Sub

        ''' <summary>
        ''' Applies the configured retention rule to determine how many factors are extracted.
        ''' </summary>
        Private Sub DetermineNumberOfFactors()
            Select Case pRetentionMethod
                Case FactorAnalysisRetentionMethod.Fixed
                    pNoFactors = CInt(Math.Round(pRetentionValue))

                Case FactorAnalysisRetentionMethod.Eigenvalue
                    pNoFactors = pInitialEigenvalues.TakeWhile(Function(x) x >= pRetentionValue).Count()

                Case FactorAnalysisRetentionMethod.Variance
                    Dim target As Double = pRetentionValue
                    If target <= 0 OrElse target > 100 Then AppGlobals.BSerr.LogAndThrow(New ArgumentOutOfRangeException(NameOf(pRetentionValue), "Variance retention targets must be in (0, 100]."))
                    Dim pct() As Double = PercentOfTotal(pInitialEigenvalues, TotalVariance(pWorkingMatrix))
                    Dim cum As Double = 0.0
                    pNoFactors = 0
                    For i As Integer = 0 To pct.Length - 1
                        cum += pct(i)
                        pNoFactors += 1
                        If cum >= target Then Exit For
                    Next
            End Select

            pNoFactors = Math.Max(1, Math.Min(pp, pNoFactors))
        End Sub

        ''' <summary>
        ''' Dispatches to the requested extraction routine and stores the unrotated solution.
        ''' </summary>
        Private Sub ExtractFactors()
            Select Case pExtractionMethod
                Case FactorAnalysisExtractionMethod.PrincipalComponents
                    pInitialCommunalities = MultivariateShared.DiagonalValues(pWorkingMatrix)
                    pUnrotatedLoadings = MultivariateShared.BuildLoadingsFromEigenpairs(pInitialEigenvalues, pInitialEigenvectors, pNoFactors)
                    pExtractionEigenvalues = ColumnSumsOfSquares(pUnrotatedLoadings, pUnrotatedLoadings)
                    pIterationsUsed = 1
                    pConverged = True

                Case FactorAnalysisExtractionMethod.PrincipalAxis
                    ExtractPrincipalAxisFactors()

                Case FactorAnalysisExtractionMethod.MaximumLikelihood,
                     FactorAnalysisExtractionMethod.GeneralizedLeastSquares,
                     FactorAnalysisExtractionMethod.Image,
                     FactorAnalysisExtractionMethod.Alpha
                    ExtractAdvancedFactorsInternal()

                Case Else
                    AppGlobals.BSerr.LogAndThrow(New NotSupportedException($"Unsupported extraction method: {pExtractionMethod}."))
            End Select
        End Sub

        ''' <summary>
        ''' Dispatches to the advanced extraction helper embedded in this source file and copies its results into the
        ''' state fields used by the rest of the factor-analysis workflow.
        ''' </summary>
        Private Sub ExtractAdvancedFactorsInternal()
            Dim helperMethod As AdvancedFactorExtractionFamily
            Select Case pExtractionMethod
                Case FactorAnalysisExtractionMethod.MaximumLikelihood
                    helperMethod = AdvancedFactorExtractionFamily.MaximumLikelihood
                Case FactorAnalysisExtractionMethod.GeneralizedLeastSquares
                    helperMethod = AdvancedFactorExtractionFamily.GeneralizedLeastSquares
                Case FactorAnalysisExtractionMethod.Image
                    helperMethod = AdvancedFactorExtractionFamily.Image
                Case FactorAnalysisExtractionMethod.Alpha
                    helperMethod = AdvancedFactorExtractionFamily.Alpha
                Case Else
                    AppGlobals.BSerr.LogAndThrow(New InvalidOperationException($"Unsupported advanced extraction method: {pExtractionMethod}."))
                    Exit Sub
            End Select

            Dim initialCommunalities() As Double = Nothing
            If pExtractionMethod <> FactorAnalysisExtractionMethod.Image Then
                initialCommunalities = BuildInitialCommunalityVector(MultivariateShared.DiagonalValues(pWorkingMatrix))
            End If

            Dim adv = FactorAnalysisAdvancedExtraction.ExtractAdvancedFactors(
                analysisMatrix:=pWorkingMatrix,
                correlationMatrix:=pCorrelationMatrix,
                numberOfFactors:=pNoFactors,
                method:=helperMethod,
                initialCommunalities:=initialCommunalities,
                maxIterations:=pMaxiter,
                epsilon:=pEps)

            pInitialCommunalities = If(adv.InitialCommunalities Is Nothing, Nothing, CType(adv.InitialCommunalities.Clone(), Double()))
            pUnrotatedLoadings = DirectCast(adv.Loadings.Clone(), Double(,))
            pExtractionEigenvalues = If(adv.ExtractionEigenvalues Is Nothing, Nothing, CType(adv.ExtractionEigenvalues.Clone(), Double()))
            pIterationsUsed = adv.Iterations
            pConverged = adv.Converged
        End Sub

        ''' <summary>
        ''' Performs iterative principal-axis factoring by repeatedly replacing the diagonal with updated communalities.
        ''' </summary>
        ''' <remarks>
        ''' At each iteration the reduced matrix is eigen-decomposed, factor loadings are rebuilt from the leading
        ''' eigenpairs, and communalities are updated from the row-wise sums of squared loadings.
        ''' </remarks>
        Private Sub ExtractPrincipalAxisFactors()
            Dim targetDiag() As Double = MultivariateShared.DiagonalValues(pWorkingMatrix)
            Dim h2() As Double = BuildInitialCommunalityVector(targetDiag)
            pInitialCommunalities = CType(h2.Clone(), Double())
            Dim loadings(,) As Double = Nothing
            Dim iter As Integer
            Dim converged As Boolean = False

            For iter = 1 To pMaxiter
                Dim reduced(,) As Double = DirectCast(pWorkingMatrix.Clone(), Double(,))
                For i As Integer = 0 To pp - 1
                    reduced(i, i) = h2(i)
                Next

                Dim raw = Matrix.EIGEN_JK(reduced, pMaxiter, pEps)
                Dim sorted = MultivariateShared.SortEigenpairsDescending(raw.Item1, raw.Item2)
                loadings = MultivariateShared.BuildLoadingsFromEigenpairs(sorted.Item1, sorted.Item2, pNoFactors)
                Dim newH2() As Double = RowSumsOfSquares(loadings)
                For i As Integer = 0 To pp - 1
                    newH2(i) = MultivariateShared.Clamp(newH2(i), 0.0, Math.Max(0.0, targetDiag(i) - pEps))
                Next

                Dim delta As Double = MultivariateShared.MaxAbsDifference(h2, newH2)
                h2 = newH2
                If delta <= pEps Then
                    converged = True
                    Exit For
                End If
            Next

            pUnrotatedLoadings = loadings
            pExtractionEigenvalues = ColumnSumsOfSquares(loadings, loadings)
            pIterationsUsed = Math.Min(iter, pMaxiter)
            pConverged = converged

            If Not converged Then
                AppGlobals.BSlogg.Log("Principal-axis factoring did not meet the convergence tolerance before the maximum iteration count.", AppGlobals.LogMsgType.Warn)
            End If
        End Sub

        ''' <summary>
        ''' Creates the starting communality vector used by principal-axis factoring.
        ''' </summary>
        ''' <param name="targetDiag">Diagonal variances of the working analysis matrix.</param>
        ''' <returns>A vector of starting communalities on the same scale as the working matrix diagonal.</returns>
        Private Function BuildInitialCommunalityVector(targetDiag() As Double) As Double()
            Dim out(pp - 1) As Double
            If pCommunalityInitialization = FactorAnalysisCommunalityInitialization.One Then
                For i As Integer = 0 To pp - 1
                    out(i) = targetDiag(i)
                Next
                Return out
            End If

            Dim invR(,) As Double = MultivariateShared.SafeInverse(pCorrelationMatrix, preferCholesky:=True)
            For i As Integer = 0 To pp - 1
                Dim smc As Double = 1.0 - 1.0 / Math.Max(invR(i, i), pEps)
                smc = MultivariateShared.Clamp(smc, 0.0, 0.999999)
                out(i) = smc * targetDiag(i)
            Next
            Return out
        End Function

        ''' <summary>
        ''' Applies the requested post-extraction rotation and stores the final pattern/structure representation.
        ''' </summary>
        Private Sub RotateFactors()
            Dim rotation As FactorRotationResult
            Select Case pRotationMethod
                Case FactorAnalysisRotationMethod.None
                    rotation = New FactorRotationResult With {
                        .Loadings = DirectCast(pUnrotatedLoadings.Clone(), Double(,)),
                        .StructureMatrix = DirectCast(pUnrotatedLoadings.Clone(), Double(,)),
                        .Phi = Matrix.IdentityMat(pNoFactors - 1),
                        .Transform = Matrix.IdentityMat(pNoFactors - 1),
                        .Iterations = 0,
                        .Converged = True,
                        .Label = "None"
                    }

                Case FactorAnalysisRotationMethod.Varimax
                    rotation = OrthomaxRotate(pUnrotatedLoadings, 1.0, "Varimax")

                Case FactorAnalysisRotationMethod.Quartimax
                    rotation = OrthomaxRotate(pUnrotatedLoadings, 0.0, "Quartimax")

                Case FactorAnalysisRotationMethod.Equamax
                    rotation = OrthomaxRotate(pUnrotatedLoadings, pNoFactors / 2.0, "Equamax")

                Case FactorAnalysisRotationMethod.Promax
                    rotation = PromaxRotate(pUnrotatedLoadings)

                Case Else
                    AppGlobals.BSerr.LogAndThrow(New NotSupportedException($"Unsupported rotation method: {pRotationMethod}."))
                    Return
            End Select

            pPatternMatrix = rotation.Loadings
            pStructureMatrix = rotation.StructureMatrix
            pPhi = rotation.Phi
            pRotationTransform = rotation.Transform
            pRotationIterations = rotation.Iterations
            pRotationConverged = rotation.Converged
            pRotationLabel = rotation.Label
        End Sub

        ''' <summary>
        ''' Applies an orthomax-family rotation to an unrotated loading matrix.
        ''' </summary>
        ''' <param name="loadings">Unrotated loading matrix.</param>
        ''' <param name="gamma">Orthomax family parameter. Common values are 0 for quartimax and 1 for varimax.</param>
        ''' <param name="label">Human-readable name stored alongside the rotation result.</param>
        ''' <returns>A fully populated <see cref="FactorRotationResult"/> describing the rotated orthogonal solution.</returns>
        Private Function OrthomaxRotate(loadings(,) As Double, gamma As Double, label As String) As FactorRotationResult
            Dim m As Integer = loadings.GetLength(1)
            Dim p As Integer = loadings.GetLength(0)
            Dim rot(m - 1, m - 1) As Double
            For i As Integer = 0 To m - 1
                rot(i, i) = 1.0
            Next

            Dim norms() As Double = Nothing
            Dim work(,) As Double
            If pUseKaiserNormalization Then
                work = KaiserNormalize(loadings, norms)
            Else
                work = DirectCast(loadings.Clone(), Double(,))
                ReDim norms(p - 1)
                For i As Integer = 0 To p - 1
                    norms(i) = 1.0
                Next
            End If

            Dim converged As Boolean = False
            Dim iter As Integer
            For iter = 1 To pMaxiter
                Dim maxAngle As Double = 0.0
                For j As Integer = 0 To m - 2
                    For k As Integer = j + 1 To m - 1
                        Dim x() As Double = Matrix.GetColumnFrom2Darray(work, j)
                        Dim y() As Double = Matrix.GetColumnFrom2Darray(work, k)
                        Dim u(p - 1) As Double
                        Dim v(p - 1) As Double
                        Dim sumU As Double = 0.0
                        Dim sumV As Double = 0.0
                        Dim sumUUminusVV As Double = 0.0
                        Dim sumUV As Double = 0.0

                        For i As Integer = 0 To p - 1
                            u(i) = x(i) * x(i) - y(i) * y(i)
                            v(i) = 2.0 * x(i) * y(i)
                            sumU += u(i)
                            sumV += v(i)
                            sumUUminusVV += u(i) * u(i) - v(i) * v(i)
                            sumUV += u(i) * v(i)
                        Next

                        Dim c As Double = sumUUminusVV - (2.0 * gamma / p) * (sumU * sumU - sumV * sumV)
                        Dim d As Double = 2.0 * sumUV - (4.0 * gamma / p) * sumU * sumV
                        Dim angle As Double = 0.25 * Math.Atan2(d, c)
                        maxAngle = Math.Max(maxAngle, Math.Abs(angle))

                        If Math.Abs(angle) > 0.0 Then
                            ApplyOrthogonalPairRotation(work, j, k, angle)
                            ApplyOrthogonalPairRotation(rot, j, k, angle)
                        End If
                    Next
                Next

                If maxAngle <= pEps Then
                    converged = True
                    Exit For
                End If
            Next

            Dim finalLoadings(,) As Double = KaiserDenormalize(work, norms)
            Dim struct(,) As Double = DirectCast(finalLoadings.Clone(), Double(,))
            Return New FactorRotationResult With {
                .Loadings = finalLoadings,
                .StructureMatrix = struct,
                .Phi = Matrix.IdentityMat(m - 1),
                .Transform = rot,
                .Iterations = Math.Min(iter, pMaxiter),
                .Converged = converged,
                .Label = label
            }
        End Function

        ''' <summary>
        ''' Applies promax rotation by first obtaining a varimax solution and then fitting an oblique target transformation.
        ''' </summary>
        ''' <param name="loadings">Unrotated loading matrix.</param>
        ''' <returns>A rotation result containing the oblique pattern matrix, structure matrix, factor correlations, and transformation matrix.</returns>
        Private Function PromaxRotate(loadings(,) As Double) As FactorRotationResult
            Dim orth As FactorRotationResult = OrthomaxRotate(loadings, 1.0, "Varimax")
            Dim a(,) As Double = orth.Loadings
            Dim p As Integer = a.GetLength(0)
            Dim m As Integer = a.GetLength(1)

            Dim target(p - 1, m - 1) As Double
            For i As Integer = 0 To p - 1
                For j As Integer = 0 To m - 1
                    Dim v As Double = a(i, j)
                    target(i, j) = Math.Sign(v) * Math.Pow(Math.Abs(v), pPromaxPower)
                Next
            Next

            Dim ata As Double(,) = Matrix.MatrixMult(Matrix.trans(a), a)
            Dim atq As Double(,) = Matrix.MatrixMult(Matrix.trans(a), target)
            Dim u As Double(,) = Matrix.MatrixMult(MultivariateShared.SafeInverse(ata, preferCholesky:=True), atq)
            Dim phi As Double(,) = MultivariateShared.SafeInverse(Matrix.MatrixMult(Matrix.trans(u), u), preferCholesky:=True)

            Dim d(m - 1) As Double
            For i As Integer = 0 To m - 1
                d(i) = Math.Sqrt(Math.Max(phi(i, i), pEps))
            Next
            Dim dMat(,) As Double = MultivariateShared.DiagonalMatrix(d)
            Dim transform(,) As Double = Matrix.MatrixMult(u, dMat)
            Dim pattern(,) As Double = Matrix.MatrixMult(a, transform)
            Dim phiStd(,) As Double = MultivariateShared.SafeInverse(Matrix.MatrixMult(Matrix.trans(transform), transform), preferCholesky:=True)
            Dim struct(,) As Double = Matrix.MatrixMult(pattern, phiStd)
            Dim totalTransform(,) As Double = Matrix.MatrixMult(orth.Transform, transform)

            Return New FactorRotationResult With {
                .Loadings = pattern,
                .StructureMatrix = struct,
                .Phi = phiStd,
                .Transform = totalTransform,
                .Iterations = orth.Iterations + 1,
                .Converged = orth.Converged,
                .Label = "Promax"
            }
        End Function

        ''' <summary>
        ''' Reconstructs the common, reproduced, and residual matrices implied by the fitted factor model.
        ''' </summary>
        Private Sub BuildModelMatrices()
            pCommonVarianceMatrix = Matrix.MatrixMult(Matrix.MatrixMult(pPatternMatrix, pPhi), Matrix.trans(pPatternMatrix))
            ReDim pCommunalities(pp - 1)
            ReDim pUniquenesses(pp - 1)
            For i As Integer = 0 To pp - 1
                pCommunalities(i) = MultivariateShared.Clamp(pCommonVarianceMatrix(i, i), 0.0, pWorkingMatrix(i, i))
                pCommonVarianceMatrix(i, i) = pCommunalities(i)
                pUniquenesses(i) = Math.Max(0.0, pWorkingMatrix(i, i) - pCommunalities(i))
            Next

            pReproducedMatrix = DirectCast(pCommonVarianceMatrix.Clone(), Double(,))
            For i As Integer = 0 To pp - 1
                pReproducedMatrix(i, i) += pUniquenesses(i)
            Next
            pResidualMatrix = Matrix.M_SUB(pWorkingMatrix, pReproducedMatrix)
            pRmsr = ComputeRmsrOffDiagonal(pResidualMatrix)
        End Sub

        ''' <summary>
        ''' Computes factor-score coefficients and observation-level scores using the requested scoring rule.
        ''' </summary>
        Private Sub ComputeFactorScores()
            pScoreCoefficientMatrix = Nothing
            pScores = Nothing
            If pScoreMethod = FactorAnalysisScoreMethod.None Then Exit Sub

            Dim psiInv(pp - 1, pp - 1) As Double
            For i As Integer = 0 To pp - 1
                psiInv(i, i) = 1.0 / Math.Max(pUniquenesses(i), pEps)
            Next

            Select Case pScoreMethod
                Case FactorAnalysisScoreMethod.Regression
                    Dim sigmaInv As Double(,) = MultivariateShared.SafeInverse(pWorkingMatrix, preferCholesky:=True)
                    pScoreCoefficientMatrix = Matrix.MatrixMult(Matrix.MatrixMult(sigmaInv, pPatternMatrix), pPhi)

                Case FactorAnalysisScoreMethod.Bartlett
                    Dim ptPsiInv As Double(,) = Matrix.MatrixMult(Matrix.trans(pPatternMatrix), psiInv)
                    Dim mid As Double(,) = Matrix.MatrixMult(ptPsiInv, pPatternMatrix)
                    pScoreCoefficientMatrix = Matrix.MatrixMult(psiInv, Matrix.MatrixMult(pPatternMatrix, MultivariateShared.SafeInverse(mid, preferCholesky:=True)))

                Case Else
                    AppGlobals.BSerr.LogAndThrow(New NotSupportedException($"Unsupported factor score method: {pScoreMethod}."))
            End Select

            Dim scoreInput(,) As Double = If(pMatrixType = FactorAnalysisMatrixType.Correlation,
                                             pStandardizedData,
                                             pCenteredData)
            pScores = Matrix.MatrixMult(scoreInput, pScoreCoefficientMatrix)
        End Sub

        ''' <summary>
        ''' Computes factor-wise sums of squares from a pattern/structure representation.
        ''' </summary>
        ''' <param name="pattern">Pattern matrix.</param>
        ''' <param name="struct">Structure matrix.</param>
        ''' <returns>A vector of factor-wise sums of squares.</returns>
        Private Function ColumnSumsOfSquares(pattern(,) As Double, struct(,) As Double) As Double()
            Dim out(pattern.GetLength(1) - 1) As Double
            For j As Integer = 0 To pattern.GetLength(1) - 1
                Dim s As Double = 0.0
                For i As Integer = 0 To pattern.GetLength(0) - 1
                    s += pattern(i, j) * struct(i, j)
                Next
                out(j) = s
            Next
            Return out
        End Function

        ''' <summary>
        ''' Expresses a vector of values as percentages of a supplied total.
        ''' </summary>
        ''' <param name="values">Values to rescale.</param>
        ''' <param name="total">Denominator used for the percentage calculation.</param>
        ''' <returns>A percentage vector.</returns>
        Private Function PercentOfTotal(values() As Double, total As Double) As Double()
            Dim out(values.Length - 1) As Double
            If total <= 0.0 Then Return out
            For i As Integer = 0 To values.Length - 1
                out(i) = 100.0 * values(i) / total
            Next
            Return out
        End Function

        ''' <summary>
        ''' Computes the cumulative running totals of a numeric vector.
        ''' </summary>
        ''' <param name="values">Input vector.</param>
        ''' <returns>Cumulative sums with the same length as <paramref name="values"/>.</returns>
        Private Function Cumulative(values() As Double) As Double()
            Dim out(values.Length - 1) As Double
            Dim s As Double = 0.0
            For i As Integer = 0 To values.Length - 1
                s += values(i)
                out(i) = s
            Next
            Return out
        End Function

        ''' <summary>
        ''' Computes total variance as the trace of the working analysis matrix.
        ''' </summary>
        ''' <param name="mat">Square covariance or correlation matrix.</param>
        ''' <returns>The sum of diagonal entries.</returns>
        Private Function TotalVariance(mat(,) As Double) As Double
            Dim s As Double = 0.0
            For i As Integer = 0 To Math.Min(mat.GetLength(0), mat.GetLength(1)) - 1
                s += mat(i, i)
            Next
            Return s
        End Function

        ''' <summary>
        ''' Applies a planar orthogonal rotation to two selected columns of a matrix.
        ''' </summary>
        ''' <param name="mat">Matrix whose columns are rotated in place.</param>
        ''' <param name="col1">Index of the first column.</param>
        ''' <param name="col2">Index of the second column.</param>
        ''' <param name="angle">Rotation angle in radians.</param>
        Private Sub ApplyOrthogonalPairRotation(ByRef mat(,) As Double, col1 As Integer, col2 As Integer, angle As Double)
            Dim c As Double = Math.Cos(angle)
            Dim s As Double = Math.Sin(angle)
            For i As Integer = 0 To mat.GetLength(0) - 1
                Dim x As Double = mat(i, col1)
                Dim y As Double = mat(i, col2)
                mat(i, col1) = x * c + y * s
                mat(i, col2) = -x * s + y * c
            Next
        End Sub

        ''' <summary>
        ''' Applies Kaiser row normalization to a loading matrix.
        ''' </summary>
        ''' <param name="loadings">Input loading matrix.</param>
        ''' <param name="norms">Returns the row norms used for normalization.</param>
        ''' <returns>The normalized loading matrix.</returns>
        Private Function KaiserNormalize(loadings(,) As Double, ByRef norms() As Double) As Double(,)
            Dim p As Integer = loadings.GetLength(0)
            Dim m As Integer = loadings.GetLength(1)
            ReDim norms(p - 1)
            Dim out(p - 1, m - 1) As Double
            For i As Integer = 0 To p - 1
                Dim norm As Double = 0.0
                For j As Integer = 0 To m - 1
                    norm += loadings(i, j) * loadings(i, j)
                Next
                norm = Math.Sqrt(Math.Max(norm, pEps))
                norms(i) = norm
                For j As Integer = 0 To m - 1
                    out(i, j) = loadings(i, j) / norm
                Next
            Next
            Return out
        End Function

        ''' <summary>
        ''' Reverses a previously applied Kaiser row normalization.
        ''' </summary>
        ''' <param name="loadings">Normalized loading matrix.</param>
        ''' <param name="norms">Row norms returned by <see cref="KaiserNormalize"/>.</param>
        ''' <returns>The de-normalized loading matrix.</returns>
        Private Function KaiserDenormalize(loadings(,) As Double, norms() As Double) As Double(,)
            Dim p As Integer = loadings.GetLength(0)
            Dim m As Integer = loadings.GetLength(1)
            Dim out(p - 1, m - 1) As Double
            For i As Integer = 0 To p - 1
                For j As Integer = 0 To m - 1
                    out(i, j) = loadings(i, j) * norms(i)
                Next
            Next
            Return out
        End Function

        ''' <summary>
        ''' Computes the root mean square of the off-diagonal entries of a residual matrix.
        ''' </summary>
        ''' <param name="mat">Residual matrix.</param>
        ''' <returns>The off-diagonal RMSR value.</returns>
        Private Function ComputeRmsrOffDiagonal(mat(,) As Double) As Double
            Dim s As Double = 0.0
            Dim nOff As Integer = 0
            For i As Integer = 0 To mat.GetLength(0) - 1
                For j As Integer = 0 To mat.GetLength(1) - 1
                    If i <> j Then
                        s += mat(i, j) * mat(i, j)
                        nOff += 1
                    End If
                Next
            Next
            If nOff = 0 Then Return 0.0
            Return Math.Sqrt(s / nOff)
        End Function

    End Class

''' <summary>
    ''' Enumerates the additional exploratory-factor-extraction families implemented by
    ''' <see cref="FactorAnalysisAdvancedExtraction"/>.
    ''' </summary>
    Public Enum AdvancedFactorExtractionFamily
        ''' <summary>
        ''' Maximum-likelihood common-factor extraction based on optimization of the unique variances.
        ''' </summary>
        MaximumLikelihood = 0

        ''' <summary>
        ''' Generalized-least-squares extraction based on weighted residual minimization.
        ''' </summary>
        GeneralizedLeastSquares = 1

        ''' <summary>
        ''' Image factoring based on the image-correlation matrix implied by the inverse correlation matrix.
        ''' </summary>
        Image = 2

        ''' <summary>
        ''' Alpha factoring based on iterative re-estimation of communalities on the correlation scale.
        ''' </summary>
        Alpha = 3
    End Enum

    ''' <summary>
    ''' Stores the result of one advanced extraction run.
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' The object is intentionally lightweight so it can be copied into the existing
    ''' <c>FactorAnalysis</c> workflow with minimal changes.
    ''' </para>
    ''' <para>
    ''' Typical integration points in the current factor-analysis class are:
    ''' </para>
    ''' <list type="bullet">
    '''   <item><description><c>InitialCommunalities</c> -> <c>pInitialCommunalities</c></description></item>
    '''   <item><description><c>Loadings</c> -> <c>pUnrotatedLoadings</c></description></item>
    '''   <item><description><c>ExtractionEigenvalues</c> -> <c>pExtractionEigenvalues</c></description></item>
    '''   <item><description><c>Iterations</c> -> <c>pIterationsUsed</c></description></item>
    '''   <item><description><c>Converged</c> -> <c>pConverged</c></description></item>
    ''' </list>
    ''' </remarks>
    Public Class AdvancedFactorExtractionResult

        ''' <summary>
        ''' Gets or sets the family used to obtain the solution.
        ''' </summary>
        Public Property Method As AdvancedFactorExtractionFamily

        ''' <summary>
        ''' Gets or sets a human-readable method label.
        ''' </summary>
        Public Property MethodLabel As String

        ''' <summary>
        ''' Gets or sets the unrotated loading matrix on the metric of the supplied analysis matrix.
        ''' </summary>
        Public Property Loadings As Double(,)

        ''' <summary>
        ''' Gets or sets the factor-wise sums of squares derived from the loading matrix.
        ''' </summary>
        Public Property ExtractionEigenvalues As Double()

        ''' <summary>
        ''' Gets or sets the starting communalities used by the extraction routine.
        ''' </summary>
        Public Property InitialCommunalities As Double()

        ''' <summary>
        ''' Gets or sets the final communalities reproduced by the extracted loading matrix.
        ''' </summary>
        Public Property Communalities As Double()

        ''' <summary>
        ''' Gets or sets the final uniqueness estimates on the metric of the supplied analysis matrix.
        ''' </summary>
        Public Property Uniquenesses As Double()

        ''' <summary>
        ''' Gets or sets the optimizer objective value, when the extraction method is formulated as an optimization problem.
        ''' </summary>
        Public Property ObjectiveValue As Double

        ''' <summary>
        ''' Gets or sets the number of iterations used by the extraction routine.
        ''' </summary>
        Public Property Iterations As Integer

        ''' <summary>
        ''' Gets or sets a value indicating whether the iterative procedure satisfied the requested convergence criterion.
        ''' </summary>
        Public Property Converged As Boolean

        ''' <summary>
        ''' Gets or sets the correlation-scale working matrix that was directly factored by the method.
        ''' </summary>
        Public Property CorrelationScaleWorkingMatrix As Double(,)

        ''' <summary>
        ''' Gets or sets the metric-scale working matrix that was directly factored by the method.
        ''' </summary>
        Public Property MetricScaleWorkingMatrix As Double(,)

    End Class

    ''' <summary>
    ''' Provides reusable extraction engines for maximum-likelihood, generalized-least-squares,
    ''' image, and alpha factor analysis.
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' The existing <c>FactorAnalysis</c> class already handles data preparation, rotation, scoring,
    ''' diagnostics, and result wrapping. This module focuses only on the extraction step so it can be
    ''' dropped into that workflow without duplicating the surrounding machinery.
    ''' </para>
    ''' <para>
    ''' Design choices used here:
    ''' </para>
    ''' <list type="bullet">
    '''   <item><description>Maximum-likelihood extraction follows the classic uniqueness optimization used by <c>factanal</c> and related implementations.</description></item>
    '''   <item><description>Generalized-least-squares extraction minimizes a weighted residual criterion close to the one used by the <c>psych</c> package.</description></item>
    '''   <item><description>Image and alpha factoring are implemented on the correlation scale and are then rescaled back to the metric of the supplied analysis matrix.</description></item>
    '''   <item><description>All methods rely only on the matrix algebra already present in the project.</description></item>
    ''' </list>
    ''' </remarks>
    Public Module FactorAnalysisAdvancedExtraction

        ''' <summary>
        ''' Stores the internal state returned by the generic uniqueness optimizer.
        ''' </summary>
        Private Class OptimizationState
            Public Psi() As Double
            Public ObjectiveValue As Double
            Public Iterations As Integer
            Public Converged As Boolean
        End Class

        ''' <summary>
        ''' Runs one of the advanced extraction families and returns a standardized extraction result object.
        ''' </summary>
        ''' <param name="analysisMatrix">
        ''' Square covariance or correlation matrix on the user-selected analysis metric.
        ''' This matrix defines the scale of the returned loadings, communalities, and uniquenesses.
        ''' </param>
        ''' <param name="correlationMatrix">
        ''' Correlation matrix associated with <paramref name="analysisMatrix"/>.
        ''' It is used for squared-multiple-correlation starts and for the image/alpha families.
        ''' </param>
        ''' <param name="numberOfFactors">Requested number of common factors.</param>
        ''' <param name="method">Advanced extraction family to run.</param>
        ''' <param name="initialCommunalities">
        ''' Optional starting communalities on the metric of <paramref name="analysisMatrix"/>.
        ''' When omitted, squared-multiple-correlation starts are used and scaled to the matrix diagonal.
        ''' </param>
        ''' <param name="maxIterations">Maximum number of optimizer or communality-update iterations.</param>
        ''' <param name="epsilon">Absolute convergence tolerance.</param>
        ''' <returns>A fully populated extraction-result object.</returns>
        Public Function ExtractAdvancedFactors(analysisMatrix(,) As Double,
                                               correlationMatrix(,) As Double,
                                               numberOfFactors As Integer,
                                               method As AdvancedFactorExtractionFamily,
                                               Optional initialCommunalities() As Double = Nothing,
                                               Optional maxIterations As Integer = 250,
                                               Optional epsilon As Double = 0.000001) As AdvancedFactorExtractionResult

            ValidateCommonInputs(analysisMatrix, correlationMatrix, numberOfFactors, maxIterations, epsilon)

            Select Case method
                Case AdvancedFactorExtractionFamily.MaximumLikelihood
                    Return ExtractMaximumLikelihoodFactors(analysisMatrix, correlationMatrix, numberOfFactors, initialCommunalities, maxIterations, epsilon)

                Case AdvancedFactorExtractionFamily.GeneralizedLeastSquares
                    Return ExtractGeneralizedLeastSquaresFactors(analysisMatrix, correlationMatrix, numberOfFactors, initialCommunalities, maxIterations, epsilon)

                Case AdvancedFactorExtractionFamily.Image
                    Return ExtractImageFactors(analysisMatrix, correlationMatrix, numberOfFactors, initialCommunalities, maxIterations, epsilon)

                Case AdvancedFactorExtractionFamily.Alpha
                    Return ExtractAlphaFactors(analysisMatrix, correlationMatrix, numberOfFactors, initialCommunalities, maxIterations, epsilon)

                Case Else
                    AppGlobals.BSerr.LogAndThrow(New NotSupportedException($"Unsupported advanced extraction family: {method}."))
                    Return Nothing
            End Select
        End Function

        ''' <summary>
        ''' Performs maximum-likelihood common-factor extraction.
        ''' </summary>
        ''' <param name="analysisMatrix">Square covariance or correlation matrix on the target metric.</param>
        ''' <param name="correlationMatrix">Correlation matrix associated with <paramref name="analysisMatrix"/>.</param>
        ''' <param name="numberOfFactors">Requested number of common factors.</param>
        ''' <param name="initialCommunalities">Optional starting communalities on the target metric.</param>
        ''' <param name="maxIterations">Maximum number of uniqueness-optimization iterations.</param>
        ''' <param name="epsilon">Absolute convergence tolerance for the projected-gradient optimizer.</param>
        ''' <returns>An extraction result containing the unrotated ML loading matrix and related quantities.</returns>
        Public Function ExtractMaximumLikelihoodFactors(analysisMatrix(,) As Double,
                                                        correlationMatrix(,) As Double,
                                                        numberOfFactors As Integer,
                                                        Optional initialCommunalities() As Double = Nothing,
                                                        Optional maxIterations As Integer = 250,
                                                        Optional epsilon As Double = 0.000001) As AdvancedFactorExtractionResult

            ValidateCommonInputs(analysisMatrix, correlationMatrix, numberOfFactors, maxIterations, epsilon)

            Dim diagS() As Double = MultivariateShared.DiagonalValues(analysisMatrix)
            Dim h2Start() As Double = GetStartingCommunalities(analysisMatrix, correlationMatrix, initialCommunalities)
            Dim psiStart() As Double = BuildUniquenessVector(diagS, h2Start, epsilon)
            Dim lower() As Double = BuildLowerUniquenessBounds(diagS, epsilon)
            Dim upper() As Double = BuildUpperUniquenessBounds(diagS, epsilon)

            Dim opt = OptimizeUniqueVariances(psiStart,
                                              lower,
                                              upper,
                                              Function(psi) MaximumLikelihoodObjective(psi, analysisMatrix, numberOfFactors),
                                              Function(psi) MaximumLikelihoodGradient(psi, analysisMatrix, numberOfFactors),
                                              maxIterations,
                                              epsilon)

            Dim loadings(,) As Double = BuildMaximumLikelihoodLoadings(opt.Psi, analysisMatrix, numberOfFactors)
            Dim communalities() As Double = RowSumsOfSquares(loadings)
            communalities = ClampCommunalities(communalities, diagS, epsilon)

            Dim result As New AdvancedFactorExtractionResult With {
                .Method = AdvancedFactorExtractionFamily.MaximumLikelihood,
                .MethodLabel = "Maximum Likelihood",
                .Loadings = loadings,
                .ExtractionEigenvalues = ColumnSumsOfSquares(loadings),
                .InitialCommunalities = h2Start,
                .Communalities = communalities,
                .Uniquenesses = BuildUniquenessVector(diagS, communalities, epsilon),
                .ObjectiveValue = opt.ObjectiveValue,
                .Iterations = opt.Iterations,
                .Converged = opt.Converged,
                .CorrelationScaleWorkingMatrix = DirectCast(correlationMatrix.Clone(), Double(,)),
                .MetricScaleWorkingMatrix = DirectCast(analysisMatrix.Clone(), Double(,))
            }

            If Not result.Converged Then
                AppGlobals.BSlogg.Log("Maximum-likelihood factor extraction did not satisfy the convergence tolerance before the iteration limit.", AppGlobals.LogMsgType.Warn)
            End If

            Return result
        End Function

        ''' <summary>
        ''' Performs generalized-least-squares common-factor extraction.
        ''' </summary>
        ''' <param name="analysisMatrix">Square covariance or correlation matrix on the target metric.</param>
        ''' <param name="correlationMatrix">Correlation matrix associated with <paramref name="analysisMatrix"/>.</param>
        ''' <param name="numberOfFactors">Requested number of common factors.</param>
        ''' <param name="initialCommunalities">Optional starting communalities on the target metric.</param>
        ''' <param name="maxIterations">Maximum number of uniqueness-optimization iterations.</param>
        ''' <param name="epsilon">Absolute convergence tolerance for the projected-gradient optimizer.</param>
        ''' <returns>An extraction result containing the unrotated GLS loading matrix and related quantities.</returns>
        Public Function ExtractGeneralizedLeastSquaresFactors(analysisMatrix(,) As Double,
                                                              correlationMatrix(,) As Double,
                                                              numberOfFactors As Integer,
                                                              Optional initialCommunalities() As Double = Nothing,
                                                              Optional maxIterations As Integer = 250,
                                                              Optional epsilon As Double = 0.000001) As AdvancedFactorExtractionResult

            ValidateCommonInputs(analysisMatrix, correlationMatrix, numberOfFactors, maxIterations, epsilon)

            Dim diagS() As Double = MultivariateShared.DiagonalValues(analysisMatrix)
            Dim h2Start() As Double = GetStartingCommunalities(analysisMatrix, correlationMatrix, initialCommunalities)
            Dim psiStart() As Double = BuildUniquenessVector(diagS, h2Start, epsilon)
            Dim lower() As Double = BuildLowerUniquenessBounds(diagS, epsilon)
            Dim upper() As Double = BuildUpperUniquenessBounds(diagS, epsilon)
            Dim sInv(,) As Double = MultivariateShared.SafeInverse(analysisMatrix, preferCholesky:=True)

            Dim opt = OptimizeUniqueVariances(psiStart,
                                              lower,
                                              upper,
                                              Function(psi) GeneralizedLeastSquaresObjective(psi, analysisMatrix, sInv, numberOfFactors),
                                              Function(psi) NumericalGradient(Function(v) GeneralizedLeastSquaresObjective(v, analysisMatrix, sInv, numberOfFactors), psi, epsilon),
                                              maxIterations,
                                              epsilon)

            Dim loadings(,) As Double = BuildReducedMatrixLoadings(opt.Psi, analysisMatrix, numberOfFactors)
            Dim communalities() As Double = RowSumsOfSquares(loadings)
            communalities = ClampCommunalities(communalities, diagS, epsilon)

            Dim result As New AdvancedFactorExtractionResult With {
                .Method = AdvancedFactorExtractionFamily.GeneralizedLeastSquares,
                .MethodLabel = "Generalized Least Squares",
                .Loadings = loadings,
                .ExtractionEigenvalues = ColumnSumsOfSquares(loadings),
                .InitialCommunalities = h2Start,
                .Communalities = communalities,
                .Uniquenesses = BuildUniquenessVector(diagS, communalities, epsilon),
                .ObjectiveValue = opt.ObjectiveValue,
                .Iterations = opt.Iterations,
                .Converged = opt.Converged,
                .CorrelationScaleWorkingMatrix = DirectCast(correlationMatrix.Clone(), Double(,)),
                .MetricScaleWorkingMatrix = DirectCast(analysisMatrix.Clone(), Double(,))
            }

            If Not result.Converged Then
                AppGlobals.BSlogg.Log("Generalized-least-squares factor extraction did not satisfy the convergence tolerance before the iteration limit.", AppGlobals.LogMsgType.Warn)
            End If

            Return result
        End Function

        ''' <summary>
        ''' Performs image factor analysis on the image-correlation matrix implied by the inverse correlation matrix.
        ''' </summary>
        ''' <param name="analysisMatrix">Square covariance or correlation matrix on the target metric.</param>
        ''' <param name="correlationMatrix">Correlation matrix associated with <paramref name="analysisMatrix"/>.</param>
        ''' <param name="numberOfFactors">Requested number of common factors.</param>
        ''' <param name="initialCommunalities">
        ''' Optional starting communalities on the target metric. They are recorded in the result object but are not required by the image-factor extraction itself.
        ''' </param>
        ''' <param name="maxIterations">
        ''' Present for API symmetry with the other extraction engines. The image-factor implementation itself is non-iterative.
        ''' </param>
        ''' <param name="epsilon">Small positive constant used for numerical safeguards.</param>
        ''' <returns>An extraction result containing the unrotated image-factor loading matrix and related quantities.</returns>
        Public Function ExtractImageFactors(analysisMatrix(,) As Double,
                                            correlationMatrix(,) As Double,
                                            numberOfFactors As Integer,
                                            Optional initialCommunalities() As Double = Nothing,
                                            Optional maxIterations As Integer = 250,
                                            Optional epsilon As Double = 0.000001) As AdvancedFactorExtractionResult

            ValidateCommonInputs(analysisMatrix, correlationMatrix, numberOfFactors, maxIterations, epsilon)

            Dim diagS() As Double = MultivariateShared.DiagonalValues(analysisMatrix)
            Dim h2Start() As Double = GetStartingCommunalities(analysisMatrix, correlationMatrix, initialCommunalities)

            Dim invR(,) As Double = MultivariateShared.SafeInverse(correlationMatrix, preferCholesky:=True)
            Dim u2(invR.GetLength(0) - 1) As Double
            For i As Integer = 0 To u2.Length - 1
                u2(i) = 1.0 / Math.Max(invR(i, i), epsilon)
            Next

            Dim antiImageCov(,) As Double = Matrix.MatrixMult(Matrix.MatrixMult(MultivariateShared.DiagonalMatrix(u2), invR), MultivariateShared.DiagonalMatrix(u2))
            Dim imageCov(,) As Double = DirectCast(correlationMatrix.Clone(), Double(,))
            For i As Integer = 0 To imageCov.GetLength(0) - 1
                For j As Integer = 0 To imageCov.GetLength(1) - 1
                    imageCov(i, j) -= antiImageCov(i, j)
                Next
            Next

            Dim imageCorr(,) As Double = CovarianceToCorrelation(imageCov, epsilon)
            Dim sorted = SortedEigen(imageCorr)
            Dim loadCorr(,) As Double = MultivariateShared.BuildLoadingsFromEigenpairs(sorted.Item1, sorted.Item2, numberOfFactors)
            Dim loadings(,) As Double = RescaleLoadingsToMetric(loadCorr, diagS)

            Dim communalitiesCorr() As Double = RowSumsOfSquares(loadCorr)
            communalitiesCorr = ClampCommunalities(communalitiesCorr, Enumerable.Repeat(1.0, communalitiesCorr.Length).ToArray(), epsilon)
            Dim communalities() As Double = ScaleVectorByDiagonal(communalitiesCorr, diagS)

            Dim result As New AdvancedFactorExtractionResult With {
                .Method = AdvancedFactorExtractionFamily.Image,
                .MethodLabel = "Image Factoring",
                .Loadings = loadings,
                .ExtractionEigenvalues = ColumnSumsOfSquares(loadings),
                .InitialCommunalities = h2Start,
                .Communalities = communalities,
                .Uniquenesses = BuildUniquenessVector(diagS, communalities, epsilon),
                .ObjectiveValue = 0.0,
                .Iterations = 1,
                .Converged = True,
                .CorrelationScaleWorkingMatrix = imageCorr,
                .MetricScaleWorkingMatrix = RescaleCovarianceLikeMatrix(imageCov, diagS)
            }

            Return result
        End Function

        ''' <summary>
        ''' Performs alpha factor extraction by iteratively updating communalities on the correlation scale.
        ''' </summary>
        ''' <param name="analysisMatrix">Square covariance or correlation matrix on the target metric.</param>
        ''' <param name="correlationMatrix">Correlation matrix associated with <paramref name="analysisMatrix"/>.</param>
        ''' <param name="numberOfFactors">Requested number of common factors.</param>
        ''' <param name="initialCommunalities">Optional starting communalities on the target metric.</param>
        ''' <param name="maxIterations">Maximum number of communality-update iterations.</param>
        ''' <param name="epsilon">Absolute convergence tolerance for the communality updates.</param>
        ''' <returns>An extraction result containing the unrotated alpha-factor loading matrix and related quantities.</returns>
        Public Function ExtractAlphaFactors(analysisMatrix(,) As Double,
                                            correlationMatrix(,) As Double,
                                            numberOfFactors As Integer,
                                            Optional initialCommunalities() As Double = Nothing,
                                            Optional maxIterations As Integer = 250,
                                            Optional epsilon As Double = 0.000001) As AdvancedFactorExtractionResult

            ValidateCommonInputs(analysisMatrix, correlationMatrix, numberOfFactors, maxIterations, epsilon)

            Dim diagS() As Double = MultivariateShared.DiagonalValues(analysisMatrix)
            Dim initialMetric() As Double = GetStartingCommunalities(analysisMatrix, correlationMatrix, initialCommunalities)
            Dim h2() As Double = ConvertCommunalitiesToCorrelationScale(initialMetric, diagS, epsilon)
            Dim h2Start() As Double = CType(initialMetric.Clone(), Double())

            Dim loadCorr(,) As Double = Nothing
            Dim lastWorking(,) As Double = Nothing
            Dim iter As Integer
            Dim converged As Boolean = False

            For iter = 1 To maxIterations
                Dim reduced(,) As Double = DirectCast(correlationMatrix.Clone(), Double(,))
                For i As Integer = 0 To reduced.GetLength(0) - 1
                    reduced(i, i) = h2(i)
                Next

                Dim working(,) As Double = CovarianceToCorrelation(reduced, epsilon)
                Dim sorted = SortedEigen(working)
                Dim provisional(,) As Double = MultivariateShared.BuildLoadingsFromEigenpairs(sorted.Item1, sorted.Item2, numberOfFactors)
                Dim model(,) As Double = Matrix.MatrixMult(provisional, Matrix.trans(provisional))
                Dim newH2(h2.Length - 1) As Double

                For i As Integer = 0 To h2.Length - 1
                    newH2(i) = MultivariateShared.Clamp(h2(i) * model(i, i), 0.0, 0.999999)
                Next

                lastWorking = working
                loadCorr = provisional

                If MultivariateShared.MaxAbsDifference(h2, newH2) <= epsilon Then
                    h2 = newH2
                    converged = True
                    Exit For
                End If

                h2 = newH2
            Next

            If loadCorr Is Nothing Then
                AppGlobals.BSerr.LogAndThrow(New InvalidOperationException("Alpha factor extraction did not produce a loading matrix."))
            End If

            Dim sqrtH2() As Double = h2.Select(Function(x) Math.Sqrt(Math.Max(x, 0.0))).ToArray()
            loadCorr = Matrix.MatrixMult(MultivariateShared.DiagonalMatrix(sqrtH2), loadCorr)

            Dim communalitiesCorr() As Double = RowSumsOfSquares(loadCorr)
            communalitiesCorr = ClampCommunalities(communalitiesCorr, Enumerable.Repeat(1.0, communalitiesCorr.Length).ToArray(), epsilon)
            Dim communalities() As Double = ScaleVectorByDiagonal(communalitiesCorr, diagS)
            Dim loadings(,) As Double = RescaleLoadingsToMetric(loadCorr, diagS)

            Dim result As New AdvancedFactorExtractionResult With {
                .Method = AdvancedFactorExtractionFamily.Alpha,
                .MethodLabel = "Alpha Factoring",
                .Loadings = loadings,
                .ExtractionEigenvalues = ColumnSumsOfSquares(loadings),
                .InitialCommunalities = h2Start,
                .Communalities = communalities,
                .Uniquenesses = BuildUniquenessVector(diagS, communalities, epsilon),
                .ObjectiveValue = 0.0,
                .Iterations = Math.Min(iter, maxIterations),
                .Converged = converged,
                .CorrelationScaleWorkingMatrix = If(lastWorking Is Nothing, DirectCast(correlationMatrix.Clone(), Double(,)), lastWorking),
                .MetricScaleWorkingMatrix = DirectCast(analysisMatrix.Clone(), Double(,))
            }

            If Not result.Converged Then
                AppGlobals.BSlogg.Log("Alpha factor extraction did not satisfy the convergence tolerance before the iteration limit.", AppGlobals.LogMsgType.Warn)
            End If

            Return result
        End Function

        ''' <summary>
        ''' Computes default starting communalities from squared multiple correlations and scales them to the metric of the supplied analysis matrix.
        ''' </summary>
        ''' <param name="analysisMatrix">Square covariance or correlation matrix on the target metric.</param>
        ''' <param name="correlationMatrix">Correlation matrix associated with <paramref name="analysisMatrix"/>.</param>
        ''' <param name="userSuppliedCommunalities">Optional user-supplied communalities on the target metric.</param>
        ''' <returns>A communality vector on the target metric.</returns>
        Public Function GetStartingCommunalities(analysisMatrix(,) As Double,
                                                 correlationMatrix(,) As Double,
                                                 Optional userSuppliedCommunalities() As Double = Nothing) As Double()

            Dim diagS() As Double = MultivariateShared.DiagonalValues(analysisMatrix)
            If userSuppliedCommunalities IsNot Nothing Then
                If userSuppliedCommunalities.Length <> diagS.Length Then
                    AppGlobals.BSerr.LogAndThrow(New ArgumentException("The supplied communality vector length does not match the matrix dimension."))
                End If
                Return ClampCommunalities(CType(userSuppliedCommunalities.Clone(), Double()), diagS, 0.000001)
            End If

            Dim invR(,) As Double = MultivariateShared.SafeInverse(correlationMatrix, preferCholesky:=True)
            Dim out(diagS.Length - 1) As Double
            For i As Integer = 0 To out.Length - 1
                Dim smc As Double = 1.0 - 1.0 / Math.Max(invR(i, i), 0.000001)
                smc = MultivariateShared.Clamp(smc, 0.0, 0.999999)
                out(i) = smc * diagS(i)
            Next

            Return out
        End Function

        ''' <summary>
        ''' Validates the common matrix and tuning parameters used by all advanced extraction methods.
        ''' </summary>
        Private Sub ValidateCommonInputs(analysisMatrix(,) As Double,
                                         correlationMatrix(,) As Double,
                                         numberOfFactors As Integer,
                                         maxIterations As Integer,
                                         epsilon As Double)

            ValidateSquareMatrix(analysisMatrix, NameOf(analysisMatrix))
            ValidateSquareMatrix(correlationMatrix, NameOf(correlationMatrix))

            If analysisMatrix.GetLength(0) <> correlationMatrix.GetLength(0) Then
                AppGlobals.BSerr.LogAndThrow(New ArgumentException("The analysis matrix and correlation matrix must have the same order."))
            End If

            If numberOfFactors < 1 OrElse numberOfFactors > analysisMatrix.GetLength(0) Then
                AppGlobals.BSerr.LogAndThrow(New ArgumentOutOfRangeException(NameOf(numberOfFactors), "The requested number of factors must be between 1 and the number of variables."))
            End If

            If maxIterations < 1 Then
                AppGlobals.BSerr.LogAndThrow(New ArgumentOutOfRangeException(NameOf(maxIterations), "The maximum number of iterations must be at least 1."))
            End If

            If epsilon <= 0.0 OrElse Double.IsNaN(epsilon) OrElse Double.IsInfinity(epsilon) Then
                AppGlobals.BSerr.LogAndThrow(New ArgumentOutOfRangeException(NameOf(epsilon), "The convergence tolerance must be a finite positive number."))
            End If
        End Sub

        ''' <summary>
        ''' Validates that a matrix is square and finite.
        ''' </summary>
        Private Sub ValidateSquareMatrix(mat(,) As Double, paramName As String)
            If mat Is Nothing Then
                AppGlobals.BSerr.LogAndThrow(New ArgumentNullException(paramName))
            End If

            If mat.GetLength(0) <> mat.GetLength(1) Then
                AppGlobals.BSerr.LogAndThrow(New ArgumentException("The supplied matrix must be square.", paramName))
            End If

            For i As Integer = 0 To mat.GetLength(0) - 1
                For j As Integer = 0 To mat.GetLength(1) - 1
                    If Double.IsNaN(mat(i, j)) OrElse Double.IsInfinity(mat(i, j)) Then
                        AppGlobals.BSerr.LogAndThrow(New ArgumentException("The supplied matrix contains a missing or non-finite value.", paramName))
                    End If
                Next
            Next
        End Sub

        ''' <summary>
        ''' Builds maximum-likelihood loadings for a fixed uniqueness vector.
        ''' </summary>
        Private Function BuildMaximumLikelihoodLoadings(psi() As Double, s(,) As Double, q As Integer) As Double(,)
            Dim sc(psi.Length - 1, psi.Length - 1) As Double
            Dim sqrtPsi(psi.Length - 1) As Double
            For i As Integer = 0 To psi.Length - 1
                sc(i, i) = 1.0 / Math.Sqrt(Math.Max(psi(i), 0.0000000001))
                sqrtPsi(i) = Math.Sqrt(Math.Max(psi(i), 0.0))
            Next

            Dim sStar(,) As Double = Matrix.MatrixMult(Matrix.MatrixMult(sc, s), sc)
            Dim sorted = SortedEigen(sStar)
            Dim nVar As Integer = s.GetLength(0)
            Dim loadingScale(q - 1, q - 1) As Double
            Dim left(nVar - 1, q - 1) As Double

            For j As Integer = 0 To q - 1
                loadingScale(j, j) = Math.Sqrt(Math.Max(sorted.Item1(j) - 1.0, 0.0))
                For i As Integer = 0 To nVar - 1
                    left(i, j) = sorted.Item2(i, j)
                Next
            Next

            Return Matrix.MatrixMult(Matrix.MatrixMult(MultivariateShared.DiagonalMatrix(sqrtPsi), left), loadingScale)
        End Function

        ''' <summary>
        ''' Builds loadings from the reduced matrix used by least-squares-type factor extraction methods.
        ''' </summary>
        Private Function BuildReducedMatrixLoadings(psi() As Double, s(,) As Double, q As Integer) As Double(,)
            Dim reduced(,) As Double = DirectCast(s.Clone(), Double(,))
            For i As Integer = 0 To reduced.GetLength(0) - 1
                reduced(i, i) -= psi(i)
            Next

            Dim sorted = SortedEigen(reduced)
            Return MultivariateShared.BuildLoadingsFromEigenpairs(sorted.Item1, sorted.Item2, q)
        End Function

        ''' <summary>
        ''' Evaluates the negative log-likelihood discrepancy used by maximum-likelihood factor analysis.
        ''' </summary>
        Private Function MaximumLikelihoodObjective(psi() As Double, s(,) As Double, q As Integer) As Double
            Dim sc(psi.Length - 1, psi.Length - 1) As Double
            For i As Integer = 0 To psi.Length - 1
                sc(i, i) = 1.0 / Math.Sqrt(Math.Max(psi(i), 0.0000000001))
            Next

            Dim sStar(,) As Double = Matrix.MatrixMult(Matrix.MatrixMult(sc, s), sc)
            Dim eig = SortedEigen(sStar).Item1

            Dim total As Double = 0.0
            For i As Integer = q To eig.Length - 1
                Dim e As Double = Math.Max(eig(i), 0.0000000001)
                total += Math.Log(e) - e
            Next

            Return -total - q + s.GetLength(0)
        End Function

        ''' <summary>
        ''' Evaluates the generalized-least-squares residual criterion used by the GLS extraction engine.
        ''' </summary>
        Private Function GeneralizedLeastSquaresObjective(psi() As Double, s(,) As Double, sInv(,) As Double, q As Integer) As Double
            Dim loadings(,) As Double = BuildReducedMatrixLoadings(psi, s, q)
            Dim model(,) As Double = Matrix.MatrixMult(loadings, Matrix.trans(loadings))
            Dim residual(s.GetLength(0) - 1, s.GetLength(1) - 1) As Double

            For i As Integer = 0 To s.GetLength(0) - 1
                For j As Integer = 0 To s.GetLength(1) - 1
                    residual(i, j) = s(i, j) - model(i, j)
                Next
            Next

            Dim weighted(,) As Double = Matrix.MatrixMult(sInv, residual)
            Return SumSquares(weighted)
        End Function

        ''' <summary>
        ''' Computes the analytic gradient of the ML discrepancy with respect to the uniqueness vector.
        ''' </summary>
        Private Function MaximumLikelihoodGradient(psi() As Double, s(,) As Double, q As Integer) As Double()
            Dim loadings(,) As Double = BuildMaximumLikelihoodLoadings(psi, s, q)
            Dim model(,) As Double = Matrix.MatrixMult(loadings, Matrix.trans(loadings))
            Dim out(psi.Length - 1) As Double

            For i As Integer = 0 To psi.Length - 1
                out(i) = (model(i, i) + psi(i) - s(i, i)) / Math.Max(psi(i) * psi(i), 0.0000000001)
            Next

            Return out
        End Function

        ''' <summary>
        ''' Optimizes the unique variances under simple box constraints using projected gradient descent with backtracking.
        ''' </summary>
        Private Function OptimizeUniqueVariances(startPsi() As Double,
                                                 lower() As Double,
                                                 upper() As Double,
                                                 objective As Func(Of Double(), Double),
                                                 gradient As Func(Of Double(), Double()),
                                                 maxIterations As Integer,
                                                 epsilon As Double) As OptimizationState

            Dim psi() As Double = ClampVector(CType(startPsi.Clone(), Double()), lower, upper)
            Dim f As Double = objective(psi)
            Dim stepSize As Double = 1.0

            For iter As Integer = 1 To maxIterations
                Dim g() As Double = gradient(psi)
                If g Is Nothing OrElse g.Length <> psi.Length OrElse g.Any(Function(x) Double.IsNaN(x) OrElse Double.IsInfinity(x)) Then
                    g = NumericalGradient(objective, psi, epsilon)
                End If

                Dim gradInf As Double = MaxAbs(g)
                If gradInf <= epsilon Then
                    Return New OptimizationState With {.Psi = psi, .ObjectiveValue = f, .Iterations = iter, .Converged = True}
                End If

                Dim grad2 As Double = Matrix.DotProduct(g, g)
                Dim alpha As Double = stepSize
                Dim improved As Boolean = False
                Dim bestPsi() As Double = psi
                Dim bestF As Double = f

                For bt As Integer = 1 To 35
                    Dim candidate(psi.Length - 1) As Double
                    For i As Integer = 0 To psi.Length - 1
                        candidate(i) = psi(i) - alpha * g(i)
                    Next
                    candidate = ClampVector(candidate, lower, upper)

                    Dim candF As Double = objective(candidate)
                    If candF < bestF - 0.0001 * alpha * grad2 OrElse candF < bestF Then
                        bestPsi = candidate
                        bestF = candF
                        improved = True
                        Exit For
                    End If

                    alpha *= 0.5
                Next

                If Not improved Then
                    Return New OptimizationState With {.Psi = psi, .ObjectiveValue = f, .Iterations = iter, .Converged = False}
                End If

                psi = bestPsi
                f = bestF
                stepSize = Math.Min(alpha * 1.5, 1.0)
            Next

            Return New OptimizationState With {.Psi = psi, .ObjectiveValue = f, .Iterations = maxIterations, .Converged = False}
        End Function

        ''' <summary>
        ''' Computes a simple central-difference numerical gradient.
        ''' </summary>
        Private Function NumericalGradient(objective As Func(Of Double(), Double), point() As Double, epsilon As Double) As Double()
            Dim out(point.Length - 1) As Double
            For i As Integer = 0 To point.Length - 1
                Dim h As Double = Math.Max(0.0001, Math.Abs(point(i)) * 0.0001)
                h = Math.Max(h, epsilon * 10.0)

                Dim plus() As Double = CType(point.Clone(), Double())
                Dim minus() As Double = CType(point.Clone(), Double())
                plus(i) += h
                minus(i) = Math.Max(minus(i) - h, epsilon * 0.1)

                Dim fp As Double = objective(plus)
                Dim fm As Double = objective(minus)
                out(i) = (fp - fm) / Math.Max(plus(i) - minus(i), 0.0000000001)
            Next
            Return out
        End Function

        ''' <summary>
        ''' Sorts an eigen decomposition in descending eigenvalue order.
        ''' </summary>
        Private Function SortedEigen(mat(,) As Double) As Tuple(Of Double(), Double(,))
            Dim raw = Matrix.EIGEN_JK(mat, 250, 0.000000001)
            Dim sorted = MultivariateShared.SortEigenpairsDescending(raw.Item1, raw.Item2)
            Return Tuple.Create(sorted.Item1, sorted.Item2)
        End Function

        ''' <summary>
        ''' Converts a covariance-like matrix into a correlation matrix.
        ''' </summary>
        Private Function CovarianceToCorrelation(mat(,) As Double, epsilon As Double) As Double(,)
            Dim n As Integer = mat.GetLength(0)
            Dim out(n - 1, n - 1) As Double
            Dim sds(n - 1) As Double

            For i As Integer = 0 To n - 1
                sds(i) = Math.Sqrt(Math.Max(mat(i, i), epsilon))
            Next

            For i As Integer = 0 To n - 1
                For j As Integer = 0 To n - 1
                    out(i, j) = mat(i, j) / Math.Max(sds(i) * sds(j), epsilon)
                Next
                out(i, i) = 1.0
            Next

            Return out
        End Function

        ''' <summary>
        ''' Rescales a correlation-scale loading matrix back to the metric of the supplied analysis-matrix diagonal.
        ''' </summary>
        Private Function RescaleLoadingsToMetric(loadingsCorr(,) As Double, diagMetric() As Double) As Double(,)
            Dim out(loadingsCorr.GetLength(0) - 1, loadingsCorr.GetLength(1) - 1) As Double
            For i As Integer = 0 To loadingsCorr.GetLength(0) - 1
                Dim sd As Double = Math.Sqrt(Math.Max(diagMetric(i), 0.0))
                For j As Integer = 0 To loadingsCorr.GetLength(1) - 1
                    out(i, j) = loadingsCorr(i, j) * sd
                Next
            Next
            Return out
        End Function

        ''' <summary>
        ''' Rescales a correlation-scale covariance-like matrix to the metric implied by the target diagonal.
        ''' </summary>
        Private Function RescaleCovarianceLikeMatrix(matCorrScale(,) As Double, diagMetric() As Double) As Double(,)
            Dim out(matCorrScale.GetLength(0) - 1, matCorrScale.GetLength(1) - 1) As Double
            For i As Integer = 0 To matCorrScale.GetLength(0) - 1
                Dim sdi As Double = Math.Sqrt(Math.Max(diagMetric(i), 0.0))
                For j As Integer = 0 To matCorrScale.GetLength(1) - 1
                    Dim sdj As Double = Math.Sqrt(Math.Max(diagMetric(j), 0.0))
                    out(i, j) = matCorrScale(i, j) * sdi * sdj
                Next
            Next
            Return out
        End Function

        ''' <summary>
        ''' Converts metric-scale communalities to the correlation scale.
        ''' </summary>
        Private Function ConvertCommunalitiesToCorrelationScale(metricCommunalities() As Double, diagMetric() As Double, epsilon As Double) As Double()
            Dim out(metricCommunalities.Length - 1) As Double
            For i As Integer = 0 To out.Length - 1
                out(i) = MultivariateShared.Clamp(metricCommunalities(i) / Math.Max(diagMetric(i), epsilon), 0.0, 0.999999)
            Next
            Return out
        End Function

        ''' <summary>
        ''' Scales correlation-scale communalities back to the target metric diagonal.
        ''' </summary>
        Private Function ScaleVectorByDiagonal(values() As Double, diagMetric() As Double) As Double()
            Dim out(values.Length - 1) As Double
            For i As Integer = 0 To out.Length - 1
                out(i) = values(i) * diagMetric(i)
            Next
            Return out
        End Function

        ''' <summary>
        ''' Computes row-wise sums of squares of a loading matrix.
        ''' </summary>
        Friend Function RowSumsOfSquares(loadings(,) As Double) As Double()
            Dim out(loadings.GetLength(0) - 1) As Double
            For i As Integer = 0 To loadings.GetLength(0) - 1
                Dim s As Double = 0.0
                For j As Integer = 0 To loadings.GetLength(1) - 1
                    s += loadings(i, j) * loadings(i, j)
                Next
                out(i) = s
            Next
            Return out
        End Function

        ''' <summary>
        ''' Computes factor-wise sums of squares of a loading matrix.
        ''' </summary>
        Private Function ColumnSumsOfSquares(loadings(,) As Double) As Double()
            Dim out(loadings.GetLength(1) - 1) As Double
            For j As Integer = 0 To loadings.GetLength(1) - 1
                Dim s As Double = 0.0
                For i As Integer = 0 To loadings.GetLength(0) - 1
                    s += loadings(i, j) * loadings(i, j)
                Next
                out(j) = s
            Next
            Return out
        End Function

        ''' <summary>
        ''' Clamps communalities to the legal diagonal range of the target metric.
        ''' </summary>
        Private Function ClampCommunalities(values() As Double, diagS() As Double, epsilon As Double) As Double()
            Dim out(values.Length - 1) As Double
            For i As Integer = 0 To values.Length - 1
                out(i) = MultivariateShared.Clamp(values(i), 0.0, Math.Max(0.0, diagS(i) - epsilon))
            Next
            Return out
        End Function

        ''' <summary>
        ''' Builds uniquenesses as diagonal minus communalities.
        ''' </summary>
        Private Function BuildUniquenessVector(diagS() As Double, communalities() As Double, epsilon As Double) As Double()
            Dim out(diagS.Length - 1) As Double
            For i As Integer = 0 To diagS.Length - 1
                out(i) = MultivariateShared.Clamp(diagS(i) - communalities(i), epsilon, diagS(i))
            Next
            Return out
        End Function

        ''' <summary>
        ''' Builds lower bounds for uniqueness optimization.
        ''' </summary>
        Private Function BuildLowerUniquenessBounds(diagS() As Double, epsilon As Double) As Double()
            Dim out(diagS.Length - 1) As Double
            For i As Integer = 0 To diagS.Length - 1
                out(i) = Math.Max(epsilon * 0.1, diagS(i) * 0.000001)
            Next
            Return out
        End Function

        ''' <summary>
        ''' Builds upper bounds for uniqueness optimization.
        ''' </summary>
        Private Function BuildUpperUniquenessBounds(diagS() As Double, epsilon As Double) As Double()
            Dim out(diagS.Length - 1) As Double
            For i As Integer = 0 To diagS.Length - 1
                out(i) = Math.Max(diagS(i) - Math.Max(epsilon * 0.1, diagS(i) * 0.000001), Math.Max(epsilon, diagS(i) * 0.5))
            Next
            Return out
        End Function

        ''' <summary>
        ''' Clamps each component of a vector to a corresponding interval.
        ''' </summary>
        Private Function ClampVector(values() As Double, lower() As Double, upper() As Double) As Double()
            Dim out(values.Length - 1) As Double
            For i As Integer = 0 To values.Length - 1
                out(i) = MultivariateShared.Clamp(values(i), lower(i), upper(i))
            Next
            Return out
        End Function

        ''' <summary>
        ''' Computes the maximum absolute element in a vector.
        ''' </summary>
        Private Function MaxAbs(values() As Double) As Double
            Dim out As Double = 0.0
            For i As Integer = 0 To values.Length - 1
                out = Math.Max(out, Math.Abs(values(i)))
            Next
            Return out
        End Function

        ''' <summary>
        ''' Computes the sum of squared matrix elements.
        ''' </summary>
        Private Function SumSquares(mat(,) As Double) As Double
            Dim out As Double = 0.0
            For i As Integer = 0 To mat.GetLength(0) - 1
                For j As Integer = 0 To mat.GetLength(1) - 1
                    out += mat(i, j) * mat(i, j)
                Next
            Next
            Return out
        End Function

    End Module

End Namespace
