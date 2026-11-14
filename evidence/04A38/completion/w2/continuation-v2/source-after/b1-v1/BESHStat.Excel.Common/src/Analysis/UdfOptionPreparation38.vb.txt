Option Explicit On
Option Strict On
Option Infer On

Imports System
Imports System.Collections.Generic
Imports System.Globalization
Imports System.Linq
Imports BESHStatNG
Imports BESHStatNG.regression
Imports BESHStatNG.CausalInference
Imports BESHStatNG.ExcelCommon.UdfScalarUtilitiesOptions38

Namespace ExcelCommon
    Public NotInheritable Class AssumptionsUDFsOptions38
        Private Sub New()
        End Sub

        ''' <summary>
        ''' Initialize caller outputs before acquiring host text. Parsing uses
        ''' local outputs so an invalid/throwing conversion retains any changes
        ''' the caller's reader made to its original ByRef storage.
        ''' </summary>
        Public Shared Function TryParseLeveneCenterFromReader(readText As Func(Of String), ByRef useMedian As Boolean, ByRef title As String) As Boolean
            useMedian = False
            title = "Levene Test"

            Dim text As String = readText()
            Dim parsedMedian As Boolean = False
            Dim parsedTitle As String = Nothing
            If Not TryParseLeveneCenter(text, parsedMedian, parsedTitle) Then Return False
            useMedian = parsedMedian
            title = parsedTitle
            Return True
        End Function

        Public Shared Function TryParseLeveneCenter(arg As String, ByRef useMedian As Boolean, ByRef title As String) As Boolean
            useMedian = False
            title = "Levene Test"

            Dim s As String = NormalizeText(arg)
            If LegacyUdfText38.EqualText(s, "") OrElse LegacyUdfText38.EqualText(s, "MEAN") OrElse LegacyUdfText38.EqualText(s, "LEVENE") OrElse LegacyUdfText38.EqualText(s, "CLASSICAL") Then
                useMedian = False
                title = "Levene Test"
                Return True
            End If

            Select Case LegacyUdfText38.CaseToken(s, New String() {"MEDIAN", "BROWN-FORSYTHE", "BROWNFORSYTHE", "BF"})
                Case "MEDIAN", "BROWN-FORSYTHE", "BROWNFORSYTHE", "BF"
                    useMedian = True
                    title = "Brown-Forsythe Test"
                    Return True
                Case Else
                    Return False
            End Select
        End Function

        Public Shared Function TryParseSymmetryMethod(arg As String, ByRef methodInternal As String, ByRef title As String) As Boolean
            Dim s As String = NormalizeText(arg)
            If LegacyUdfText38.EqualText(s, "") OrElse LegacyUdfText38.EqualText(s, "MGG") OrElse LegacyUdfText38.EqualText(s, "MIAO-GEL-GASTWIRTH") OrElse LegacyUdfText38.EqualText(s, "MIAO") Then
                methodInternal = "Miao-Gel-Gastwirth"
                title = "Miao-Gel-Gastwirth Symmetry Test"
                Return True
            End If

            Select Case LegacyUdfText38.CaseToken(s, New String() {"CM", "CABILIO-MASARO", "CABILIO", "MASARO"})
                Case "CM", "CABILIO-MASARO", "CABILIO", "MASARO"
                    methodInternal = "Cabilio-Masaro"
                    title = "Cabilio-Masaro Symmetry Test"
                    Return True
                Case Else
                    Return False
            End Select
        End Function
    End Class

    Public NotInheritable Class MultivariateAnalysisUDFsOptions38
        Private Sub New()
        End Sub

        Public Shared Function ParsePcaMatrixType(v As String) As String
            Dim token As String = NormalizeToken(v)
            Select Case LegacyUdfText38.CaseToken(token, New String() {"", "CORRELATION", "CORR", "STANDARDIZED", "STANDARDISED", "COVARIANCE", "COV", "RAW", "ORIGINAL"})
                Case "", "CORRELATION", "CORR", "STANDARDIZED", "STANDARDISED"
                    Return "Correlation"
                Case "COVARIANCE", "COV", "RAW", "ORIGINAL"
                    Return "Covariance"
                Case Else
                    Throw New ArgumentException("matrixType must be either correlation or covariance.")
            End Select
        End Function

        Public Shared Function ParsePcaRetentionMethod(v As String) As String
            Dim token As String = NormalizeToken(v)
            Select Case LegacyUdfText38.CaseToken(token, New String() {"", "EIGENVALUE", "KAISER", "MINEIGEN", "FIXED", "COUNT", "NUMBER", "VARIANCE", "PERCENT", "PCT", "CUMULATIVE"})
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

        Public Shared Function ParseFaMatrixType(v As String) As Multivariate.FactorAnalysisMatrixType
            Dim token As String = NormalizeToken(v)
            Select Case LegacyUdfText38.CaseToken(token, New String() {"", "CORRELATION", "CORR", "STANDARDIZED", "STANDARDISED", "COVARIANCE", "COV", "RAW", "ORIGINAL"})
                Case "", "CORRELATION", "CORR", "STANDARDIZED", "STANDARDISED"
                    Return Multivariate.FactorAnalysisMatrixType.Correlation
                Case "COVARIANCE", "COV", "RAW", "ORIGINAL"
                    Return Multivariate.FactorAnalysisMatrixType.Covariance
                Case Else
                    Throw New ArgumentException("matrixType must be either correlation or covariance.")
            End Select
        End Function

        Public Shared Function ParseFaExtractionMethod(v As String) As Multivariate.FactorAnalysisExtractionMethod
            Dim token As String = NormalizeToken(v)
            Select Case LegacyUdfText38.CaseToken(token, New String() {"", "PRINCIPALAXIS", "PAF", "PRINCIPALFACTOR", "PRINCIPALAXISFACTORING", "PRINCIPALCOMPONENTS", "PC", "PCA", "COMPONENTS", "MAXIMUMLIKELIHOOD", "ML", "GENERALIZEDLEASTSQUARES", "GLS", "IMAGE", "ALPHA"})
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

        Public Shared Function ParseFaRetentionMethod(v As String) As Multivariate.FactorAnalysisRetentionMethod
            Dim token As String = NormalizeToken(v)
            Select Case LegacyUdfText38.CaseToken(token, New String() {"", "EIGENVALUE", "KAISER", "MINEIGEN", "FIXED", "COUNT", "NUMBER", "VARIANCE", "PERCENT", "PCT", "CUMULATIVE"})
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

        Public Shared Function ParseFaRotationMethod(v As String) As Multivariate.FactorAnalysisRotationMethod
            Dim token As String = NormalizeToken(v)
            Select Case LegacyUdfText38.CaseToken(token, New String() {"", "NONE", "UNROTATED", "VARIMAX", "QUARTIMAX", "EQUAMAX", "PROMAX"})
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

        Public Shared Function ParseFaScoreMethod(v As String) As Multivariate.FactorAnalysisScoreMethod
            Dim token As String = NormalizeToken(v)
            Select Case LegacyUdfText38.CaseToken(token, New String() {"", "REGRESSION", "THOMSON", "NONE", "BARTLETT"})
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

        Public Shared Function ParseFaCommunalityInitialization(v As String) As Multivariate.FactorAnalysisCommunalityInitialization
            Dim token As String = NormalizeToken(v)
            Select Case LegacyUdfText38.CaseToken(token, New String() {"", "SMC", "SQUAREDMULTIPLECORRELATION", "SQUAREDMULTIPLECORRELATIONS", "ONE", "UNIT", "ONES"})
                Case "", "SMC", "SQUAREDMULTIPLECORRELATION", "SQUAREDMULTIPLECORRELATIONS"
                    Return Multivariate.FactorAnalysisCommunalityInitialization.SquaredMultipleCorrelation
                Case "ONE", "UNIT", "ONES"
                    Return Multivariate.FactorAnalysisCommunalityInitialization.One
                Case Else
                    Throw New ArgumentException("communalityInitialization must be smc or one.")
            End Select
        End Function

        Public Shared Function ParseFaMissingPolicy(v As String) As Multivariate.FactorAnalysisMissingValuePolicy
            Dim token As String = NormalizeToken(v)
            Select Case LegacyUdfText38.CaseToken(token, New String() {"", "ERROR", "ERRORONMISSING", "STOP", "LISTWISE", "LISTWISEDELETION", "DELETE", "DROP"})
                Case "", "ERROR", "ERRORONMISSING", "STOP"
                    Return Multivariate.FactorAnalysisMissingValuePolicy.ErrorOnMissing
                Case "LISTWISE", "LISTWISEDELETION", "DELETE", "DROP"
                    Return Multivariate.FactorAnalysisMissingValuePolicy.ListwiseDeletion
                Case Else
                    Throw New ArgumentException("missingValuePolicy must be error or listwise.")
            End Select
        End Function

        Public Shared Function ParseClusterStandardizationMode(v As String) As Multivariate.ClusterStandardizationMode
            Dim token As String = NormalizeToken(v)
            Select Case LegacyUdfText38.CaseToken(token, New String() {"", "NONE", "RAW", "ORIGINAL", "ZSCORES", "ZSCORE", "STANDARDIZE", "STANDARDISE", "STANDARDIZED", "STANDARDISED", "RANGE01", "RANGE0TO1", "ZEROONE", "RANGEZEROTOONE", "MINMAX", "UNITINTERVAL"})
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

        Public Shared Function ParseClusterMissingValuePolicy(v As String) As Multivariate.ClusterMissingValuePolicy
            Dim token As String = NormalizeToken(v)
            Select Case LegacyUdfText38.CaseToken(token, New String() {"", "ERROR", "ERRORONMISSING", "STOP", "LISTWISE", "LISTWISEDELETION", "DELETE", "DROP"})
                Case "", "ERROR", "ERRORONMISSING", "STOP"
                    Return Multivariate.ClusterMissingValuePolicy.ErrorOnMissing
                Case "LISTWISE", "LISTWISEDELETION", "DELETE", "DROP"
                    Return Multivariate.ClusterMissingValuePolicy.ListwiseDeletion
                Case Else
                    Throw New ArgumentException("missingValuePolicy must be error or listwise.")
            End Select
        End Function

        Public Shared Function ParseKMeansInitialization(v As String) As Multivariate.KMeansInitializationMethod
            Dim token As String = NormalizeToken(v)
            Select Case LegacyUdfText38.CaseToken(token, New String() {"", "KMEANSPLUSPLUS", "KMEANS++", "PLUSPLUS", "KPP", "FORGY", "RANDOMOBSERVATIONS", "RANDOMPARTITION", "PARTITION", "USERSPECIFIED", "USERSPECIFIEDCENTERS", "CENTERS", "STARTINGCENTERS"})
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

        Public Shared Function ParseKMeansDistanceMetric(v As String) As Multivariate.KMeansDistanceMetric
            Dim token As String = NormalizeToken(v)
            Select Case LegacyUdfText38.CaseToken(token, New String() {"", "SQUAREDEUCLIDEAN", "SQEUCLIDEAN", "SQUARED", "EUCLIDEAN"})
                Case "", "SQUAREDEUCLIDEAN", "SQEUCLIDEAN", "SQUARED"
                    Return Multivariate.KMeansDistanceMetric.SquaredEuclidean
                Case "EUCLIDEAN"
                    Return Multivariate.KMeansDistanceMetric.Euclidean
                Case Else
                    Throw New ArgumentException("distanceMetric must be squaredeuclidean or euclidean.")
            End Select
        End Function

        Public Shared Function ParseEmptyClusterHandling(v As String) As Multivariate.EmptyClusterHandlingStrategy
            Dim token As String = NormalizeToken(v)
            Select Case LegacyUdfText38.CaseToken(token, New String() {"", "FARTHESTOBSERVATION", "FARTHEST", "RANDOMOBSERVATION", "RANDOM", "KEEPPREVIOUSCENTER", "KEEP", "PREVIOUS"})
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

        Public Shared Function ParseHierarchicalLinkage(v As String) As Multivariate.HierarchicalLinkageMethod
            Dim token As String = NormalizeToken(v)
            Select Case LegacyUdfText38.CaseToken(token, New String() {"", "WARD", "WARDS", "WARDMINIMUMVARIANCE", "SINGLE", "SINGLELINKAGE", "COMPLETE", "COMPLETELINKAGE", "AVERAGE", "AVERAGELINKAGE", "UPGMA", "WEIGHTEDAVERAGE", "WEIGHTED", "WPGMA", "CENTROID", "UPGMC", "MEDIAN", "WPGMC"})
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

        Public Shared Function ParseHierarchicalDistanceMetric(v As String) As Multivariate.HierarchicalDistanceMetric
            Dim token As String = NormalizeToken(v)
            Select Case LegacyUdfText38.CaseToken(token, New String() {"", "SQUAREDEUCLIDEAN", "SQEUCLIDEAN", "SQUARED", "EUCLIDEAN", "MANHATTAN", "CITYBLOCK", "CHEBYSHEV", "MAXIMUM", "MINKOWSKI", "COSINE", "CORRELATION", "PEARSON"})
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

        Public Shared Function ParseHierarchicalMembershipDisplayMode(v As String) As Multivariate.HierarchicalMembershipDisplayMode
            Dim token As String = NormalizeToken(v)
            Select Case LegacyUdfText38.CaseToken(token, New String() {"", "CLUSTERS", "COUNT", "K", "BYCLUSTERCOUNT", "HEIGHT", "CUTHEIGHT", "BYHEIGHT"})
                Case "", "CLUSTERS", "COUNT", "K", "BYCLUSTERCOUNT"
                    Return Multivariate.HierarchicalMembershipDisplayMode.ByClusterCount
                Case "HEIGHT", "CUTHEIGHT", "BYHEIGHT"
                    Return Multivariate.HierarchicalMembershipDisplayMode.ByHeight
                Case Else
                    Throw New ArgumentException("mode must be clusters/count or height.")
            End Select
        End Function

        Public Shared Function ParseOutputScaleUseOriginal(v As String) As Boolean
            Dim token As String = NormalizeToken(v)
            Select Case LegacyUdfText38.CaseToken(token, New String() {"", "ORIGINAL", "RAW", "WORKING", "STANDARDIZED", "STANDARDISED", "ANALYSIS"})
                Case "", "ORIGINAL", "RAW"
                    Return True
                Case "WORKING", "STANDARDIZED", "STANDARDISED", "ANALYSIS"
                    Return False
                Case Else
                    Throw New ArgumentException("scale must be original or working.")
            End Select
        End Function

        Public Shared Function ParseDiscriminantMethod(v As String) As Multivariate.DiscriminantAnalysisMethod
            Dim token As String = NormalizeToken(v)
            Select Case LegacyUdfText38.CaseToken(token, New String() {"", "LINEAR", "LDA", "QUADRATIC", "QDA"})
                Case "", "LINEAR", "LDA"
                    Return Multivariate.DiscriminantAnalysisMethod.Linear
                Case "QUADRATIC", "QDA"
                    Return Multivariate.DiscriminantAnalysisMethod.Quadratic
                Case Else
                    Throw New ArgumentException("method must be linear or quadratic.")
            End Select
        End Function

        Public Shared Function ParseDiscriminantPriorMode(v As String) As Multivariate.DiscriminantPriorMode
            Dim token As String = NormalizeToken(v)
            Select Case LegacyUdfText38.CaseToken(token, New String() {"", "PROPORTIONAL", "PROPORTIONALTOGROUPSIZES", "PROPORTIONALTOGROUPSIZE", "OBSERVED", "DEFAULT", "EQUAL", "UNIFORM", "USER", "USERSPECIFIED", "CUSTOM"})
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

        Public Shared Function ParseDiscriminantValidationMode(v As String) As Multivariate.DiscriminantValidationMode
            Dim token As String = NormalizeToken(v)
            Select Case LegacyUdfText38.CaseToken(token, New String() {"", "NONE", "APPARENT", "RESUBSTITUTION", "LEAVEONEOUT", "LOO", "JACKKNIFE", "KFOLD", "K-FOLD", "CV", "CROSSVALIDATION", "HOLDOUT", "TEST", "TRAINTEST", "SPLIT"})
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
    End Class

    Public NotInheritable Class ParametricUDFsOptions38
        Private Sub New()
        End Sub

        Public Shared Function ParseEqualVarianceMode(arg As String, defaultEqual As Boolean) As Boolean
            Dim s As String = NormalizeToken(arg)
            If LegacyUdfText38.EqualText(s, "") Then Return defaultEqual
            Select Case LegacyUdfText38.CaseToken(s, New String() {"WELCH", "UNEQUAL", "ASSUMEUNEQUAL", "ASSUME-UNEQUAL", "EQUAL", "POOLED", "STUDENT", "ASSUMEEQUAL", "ASSUME-EQUAL"})
                Case "WELCH", "UNEQUAL", "ASSUMEUNEQUAL", "ASSUME-UNEQUAL"
                    Return False
                Case "EQUAL", "POOLED", "STUDENT", "ASSUMEEQUAL", "ASSUME-EQUAL"
                    Return True
                Case Else
                    Throw New ArgumentException("Unsupported variance method. Use welch or equal/pooled/student.")
            End Select
        End Function
    End Class

    Public NotInheritable Class PropensityScoreUDFsOptions38
        Private Sub New()
        End Sub

        Public Shared Function ParseRunMethod(value As String, defaultValue As PsmBackendRunMethod) As PsmBackendRunMethod
            Dim s As String = NormalizeOptionKey(value)
            Select Case LegacyUdfText38.CaseToken(s, New String() {"matching", "nearest", "nearestneighbor", "nn", "standardnearestneighbor", "weighting", "weights", "iptw", "overlap", "weightingonly", "subclassification", "subclass", "strata", "standardsubclassification", "optimal", "optimalpair", "optimalpairmatching", "cem", "coarsenedexact", "coarsenedexactmatching"})
                Case "matching", "nearest", "nearestneighbor", "nn", "standardnearestneighbor"
                    Return PsmBackendRunMethod.StandardNearestNeighbor
                Case "weighting", "weights", "iptw", "overlap", "weightingonly"
                    Return PsmBackendRunMethod.WeightingOnly
                Case "subclassification", "subclass", "strata", "standardsubclassification"
                    Return PsmBackendRunMethod.StandardSubclassification
                Case "optimal", "optimalpair", "optimalpairmatching"
                    Return PsmBackendRunMethod.OptimalPairMatching
                Case "cem", "coarsenedexact", "coarsenedexactmatching"
                    Return PsmBackendRunMethod.CoarsenedExactMatching
                Case Else
                    Return defaultValue
            End Select
        End Function

        Public Shared Function ParseScoreMethod(value As String, defaultValue As PsmScoreMethod) As PsmScoreMethod
            Dim s As String = NormalizeOptionKey(value)
            Select Case LegacyUdfText38.CaseToken(s, New String() {"supplied", "existing", "score", "propensityscore", "logit", "logistic", "logisticregression", "glm"})
                Case "supplied", "existing", "score", "propensityscore"
                    Return PsmScoreMethod.Supplied
                Case "logit", "logistic", "logisticregression", "glm"
                    Return PsmScoreMethod.LogisticRegression
                Case Else
                    Return defaultValue
            End Select
        End Function

        Public Shared Function ParseDistance(value As String, defaultValue As PsmDistanceMetric) As PsmDistanceMetric
            Dim s As String = NormalizeOptionKey(value)
            Select Case LegacyUdfText38.CaseToken(s, New String() {"ps", "propensity", "propensityscore", "logit", "logitps", "logitpropensity", "logitpropensityscore", "mahalanobis", "mahalanobiswithpropensitycaliper", "mahalanobiswithpscaliper", "mahalanobiswithinpropensitycaliper"})
                Case "ps", "propensity", "propensityscore"
                    Return PsmDistanceMetric.PropensityScore
                Case "logit", "logitps", "logitpropensity", "logitpropensityscore"
                    Return PsmDistanceMetric.LogitPropensityScore
                Case "mahalanobis"
                    Return PsmDistanceMetric.Mahalanobis
                Case "mahalanobiswithpropensitycaliper", "mahalanobiswithpscaliper", "mahalanobiswithinpropensitycaliper"
                    Return PsmDistanceMetric.MahalanobisWithinPropensityCaliper
                Case Else
                    Return defaultValue
            End Select
        End Function

        Public Shared Function ParseCaliperScale(value As String, defaultValue As PsmCaliperScale) As PsmCaliperScale
            Dim s As String = NormalizeOptionKey(value)
            Select Case LegacyUdfText38.CaseToken(s, New String() {"none", "no", "false", "0", "raw", "ps", "rawps", "rawpropensity", "rawpropensityscore", "sd", "sdps", "standardized", "standardizedps", "standardizedpropensityscore", "logit", "logitps", "logitpropensityscore", "sdlogit", "standardizedlogit", "standardizedlogitpropensityscore"})
                Case "none", "no", "false", "0"
                    Return PsmCaliperScale.None
                Case "raw", "ps", "rawps", "rawpropensity", "rawpropensityscore"
                    Return PsmCaliperScale.RawPropensityScore
                Case "sd", "sdps", "standardized", "standardizedps", "standardizedpropensityscore"
                    Return PsmCaliperScale.StandardizedPropensityScore
                Case "logit", "logitps", "logitpropensityscore"
                    Return PsmCaliperScale.LogitPropensityScore
                Case "sdlogit", "standardizedlogit", "standardizedlogitpropensityscore"
                    Return PsmCaliperScale.StandardizedLogitPropensityScore
                Case Else
                    Return defaultValue
            End Select
        End Function

        Public Shared Function ParseOrder(value As String, defaultValue As PsmMatchingOrder) As PsmMatchingOrder
            Dim s As String = NormalizeOptionKey(value)
            Select Case LegacyUdfText38.CaseToken(s, New String() {"asinput", "input", "original", "ascending", "psascending", "propensityascending", "descending", "psdescending", "propensitydescending", "random", "hardest", "hardestfirst", "bestfirst"})
                Case "asinput", "input", "original"
                    Return PsmMatchingOrder.AsInput
                Case "ascending", "psascending", "propensityascending"
                    Return PsmMatchingOrder.PropensityAscending
                Case "descending", "psdescending", "propensitydescending"
                    Return PsmMatchingOrder.PropensityDescending
                Case "random"
                    Return PsmMatchingOrder.Random
                Case "hardest", "hardestfirst", "bestfirst"
                    Return PsmMatchingOrder.HardestFirst
                Case Else
                    Return defaultValue
            End Select
        End Function

        Public Shared Function ParseSupport(value As String, defaultValue As PsmCommonSupportMode) As PsmCommonSupportMode
            Dim s As String = NormalizeOptionKey(value)
            Select Case LegacyUdfText38.CaseToken(s, New String() {"none", "no", "false", "0", "overlap", "minmax", "dropoutsideoverlaprange", "treated", "dropoutsidetreated", "dropcontrolsoutsidetreatedrange", "control", "controls", "dropoutsidcontrol", "droptreatedoutsidecontrolrange"})
                Case "none", "no", "false", "0"
                    Return PsmCommonSupportMode.None
                Case "overlap", "minmax", "dropoutsideoverlaprange"
                    Return PsmCommonSupportMode.DropOutsideOverlapRange
                Case "treated", "dropoutsidetreated", "dropcontrolsoutsidetreatedrange"
                    Return PsmCommonSupportMode.DropControlsOutsideTreatedRange
                Case "control", "controls", "dropoutsidcontrol", "droptreatedoutsidecontrolrange"
                    Return PsmCommonSupportMode.DropTreatedOutsideControlRange
                Case Else
                    Return defaultValue
            End Select
        End Function

        Public Shared Function ParseBoolean(value As String, defaultValue As Boolean) As Boolean
            If String.IsNullOrWhiteSpace(value) Then Return defaultValue
            Select Case LegacyUdfText38.CaseToken(value.Trim().ToLowerInvariant(), New String() {"true", "t", "yes", "y", "1", "on", "false", "f", "no", "n", "0", "off"})
                Case "true", "t", "yes", "y", "1", "on"
                    Return True
                Case "false", "f", "no", "n", "0", "off"
                    Return False
                Case Else
                    Return defaultValue
            End Select
        End Function

        Public Shared Function ParseInteger(value As String, defaultValue As Integer) As Integer
            Dim parsed As Integer
            If Integer.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, parsed) Then Return parsed
            Return defaultValue
        End Function

        Public Shared Function ParseDouble(value As String, defaultValue As Double) As Double
            Dim parsed As Double
            If Double.TryParse(value, NumberStyles.Float Or NumberStyles.AllowThousands, CultureInfo.InvariantCulture, parsed) Then Return parsed
            If Double.TryParse(value, NumberStyles.Float Or NumberStyles.AllowThousands, CultureInfo.CurrentCulture, parsed) Then Return parsed
            Return defaultValue
        End Function

        Public Shared Function NormalizeOptionKey(value As String) As String
            If value Is Nothing Then Return String.Empty
            Dim chars As New List(Of Char)()
            For Each ch As Char In value.Trim().ToLowerInvariant()
                If Char.IsLetterOrDigit(ch) Then chars.Add(ch)
            Next
            Return New String(chars.ToArray())
        End Function
    End Class

    Public NotInheritable Class CoxUDFsOptions38
        Private Sub New()
        End Sub

        Public Shared Function ParseResidualType(v As String) As ResidualType
            Dim s As String = v
            If String.IsNullOrWhiteSpace(s) Then Return ResidualType.Martingale
            Select Case LegacyUdfText38.CaseToken(s.Trim().ToLowerInvariant(), New String() {"martingale", "deviance", "schoenfeld", "dfbeta"})
                Case "martingale" : Return ResidualType.Martingale
                Case "deviance" : Return ResidualType.Deviance
                Case "schoenfeld" : Return ResidualType.Schoenfeld
                Case "dfbeta" : Return ResidualType.Dfbeta
                Case Else : Return ResidualType.Martingale
            End Select
        End Function
    End Class

    Public NotInheritable Class GEEUDFsOptions38
        Private Sub New()
        End Sub

        Public Shared Function ParseGeeLsmEstimateScale(v As String) As Boolean
            Dim s As String = v
            If String.IsNullOrWhiteSpace(s) Then Return False
            Select Case LegacyUdfText38.CaseToken(NormalizeKey(s), New String() {"response", "mean", "probability", "mu"})
                Case "response", "mean", "probability", "mu"
                    Return True
                Case Else
                    Return False
            End Select
        End Function

        Public Shared Function ParseGeeCovarianceName(v As String) As String
            Dim s As String = v
            If String.IsNullOrWhiteSpace(s) Then Return "Independence"

            Select Case LegacyUdfText38.CaseToken(NormalizeKey(s), New String() {"independence", "independent", "id", "identity", "exchangeable", "exchangable", "compoundsymmetry", "cs", "exch", "exchangeablecs", "compoundsymmetrycs", "autoregressive", "ar1", "ar", "autoregressivear1", "ar1autoregressive", "toeplitz", "toep", "toeplitztoep", "toeptoeplitz", "stationary", "stationarymdependent", "unstructured", "un", "uns", "unstructuredun"})
                Case "independence", "independent", "id", "identity"
                    Return "Independence"
                Case "exchangeable", "exchangable", "compoundsymmetry", "cs", "exch", "exchangeablecs", "compoundsymmetrycs"
                    Return "Exchangable"
                Case "autoregressive", "ar1", "ar", "autoregressivear1", "ar1autoregressive"
                    Return "Autoregressive"
                Case "toeplitz", "toep", "toeplitztoep", "toeptoeplitz", "stationary", "stationarymdependent"
                    Return "Toeplitz"
                Case "unstructured", "un", "uns", "unstructuredun"
                    Return "Unstructured"
                Case Else
                    Return Nothing
            End Select
        End Function

        Public Shared Function ParseGeeStandardErrorType(v As String) As String
            Dim s As String = v
            If String.IsNullOrWhiteSpace(s) Then Return "Robust"

            Select Case LegacyUdfText38.CaseToken(NormalizeKey(s), New String() {"robust", "sandwich", "empirical", "naive", "modelbased", "model", "biasreduced", "biascorrected", "biascorr", "manclderouen"})
                Case "robust", "sandwich", "empirical"
                    Return "Robust"
                Case "naive", "modelbased", "model"
                    Return "Naive"
                Case "biasreduced", "biascorrected", "biascorr", "manclderouen"
                    Return "Bias Reduced"
                Case Else
                    Return Nothing
            End Select
        End Function

        Public Shared Function ParseGeeResidualType(v As String) As String
            Dim s As String = v
            If String.IsNullOrWhiteSpace(s) Then Return "all"

            Select Case LegacyUdfText38.CaseToken(NormalizeKey(s), New String() {"all", "raw", "response", "deviance", "dev", "pearson", "stdpearson", "standardizedpearson", "scaledpearson", "stddeviance", "standardizeddeviance", "scaleddeviance", "working", "work"})
                Case "all"
                    Return "all"
                Case "raw", "response"
                    Return "raw"
                Case "deviance", "dev"
                    Return "deviance"
                Case "pearson"
                    Return "pearson"
                Case "stdpearson", "standardizedpearson", "scaledpearson"
                    Return "stdpearson"
                Case "stddeviance", "standardizeddeviance", "scaleddeviance"
                    Return "stddeviance"
                Case "working", "work"
                    Return "working"
                Case Else
                    Return "all"
            End Select
        End Function
    End Class

    Public NotInheritable Class GLMUDFsOptions38
        Private Sub New()
        End Sub

        Public Shared Function ParseGlmResidualType(v As String) As String
            Dim s As String = v
            If String.IsNullOrWhiteSpace(s) Then Return "all"

            Select Case LegacyUdfText38.CaseToken(NormalizeKey(s), New String() {"all", "raw", "response", "deviance", "dev", "pearson", "stdpearson", "standardizedpearson", "stddeviance", "standardizeddeviance", "leverage", "hat", "cook", "cooksdistance", "cookdistance"})
                Case "all"
                    Return "all"
                Case "raw", "response"
                    Return "raw"
                Case "deviance", "dev"
                    Return "deviance"
                Case "pearson"
                    Return "pearson"
                Case "stdpearson", "standardizedpearson"
                    Return "stdpearson"
                Case "stddeviance", "standardizeddeviance"
                    Return "stddeviance"
                Case "leverage", "hat"
                    Return "leverage"
                Case "cook", "cooksdistance", "cookdistance"
                    Return "cook"
                Case Else
                    Return "all"
            End Select
        End Function
    End Class

    Public NotInheritable Class LinearModelUDFsOptions38
        Private Sub New()
        End Sub

        Public Shared Function ParseLmAnovaScope(v As String) As String
            Dim s As String = v
            If String.IsNullOrWhiteSpace(s) Then Return "overall"

            Select Case LegacyUdfText38.CaseToken(s.Trim().ToLowerInvariant(), New String() {"type1", "typei", "i", "seq", "sequential", "type3", "typeiii", "iii", "partial"})
                Case "type1", "typei", "i", "seq", "sequential"
                    Return "type1"
                Case "type3", "typeiii", "iii", "partial"
                    Return "type3"
                Case Else
                    Return "overall"
            End Select
        End Function

        Public Shared Function ParseLmResidualType(v As String) As String
            Dim s As String = v
            If String.IsNullOrWhiteSpace(s) Then Return "all"

            Select Case LegacyUdfText38.CaseToken(s.Trim().ToLowerInvariant(), New String() {"fitted", "fit", "mean", "pred", "prediction", "residual", "resid", "raw", "leverage", "hat", "stdresid", "standardized", "standardizedresidual", "std", "studentized", "cooksd", "cook", "cooks", "cooksdistance", "jackknife", "press", "deleted"})
                Case "fitted", "fit", "mean", "pred", "prediction"
                    Return "fitted"
                Case "residual", "resid", "raw"
                    Return "residual"
                Case "leverage", "hat"
                    Return "leverage"
                Case "stdresid", "standardized", "standardizedresidual", "std", "studentized"
                    Return "stdresid"
                Case "cooksd", "cook", "cooks", "cooksdistance"
                    Return "cooksd"
                Case "jackknife", "press", "deleted"
                    Return "jackknife"
                Case Else
                    Return "all"
            End Select
        End Function
    End Class

    Public NotInheritable Class MixedModelUDFsOptions38
        Private Sub New()
        End Sub

        Public Shared Function NormalizeDesignColumnName(s As String) As String
            If s Is Nothing Then Return String.Empty
            Return New String(s.Trim().ToLowerInvariant().Where(Function(ch) Char.IsLetterOrDigit(ch)).ToArray())
        End Function
    End Class

    Public NotInheritable Class MultinomialLogitUDFsOptions38
        Private Sub New()
        End Sub

        Public Shared Function ParseMultinomialResidualType(v As String) As String
            Dim s As String = v
            If String.IsNullOrWhiteSpace(s) Then Return "all"

            Select Case LegacyUdfText38.CaseToken(s.Trim().ToLowerInvariant(), New String() {"all", "observed", "fittedmean", "mean", "fittedmeans", "prob", "probability", "probabilities", "fittedprob", "fittedprobability", "response", "raw", "pearson", "stdpearson", "standardizedpearson", "deviance", "dev", "stddeviance", "standardizeddeviance", "leverage", "hat"})
                Case "all"
                    Return "all"
                Case "observed"
                    Return "observed"
                Case "fittedmean", "mean", "fittedmeans"
                    Return "fittedmean"
                Case "prob", "probability", "probabilities", "fittedprob", "fittedprobability"
                    Return "prob"
                Case "response", "raw"
                    Return "response"
                Case "pearson"
                    Return "pearson"
                Case "stdpearson", "standardizedpearson"
                    Return "stdpearson"
                Case "deviance", "dev"
                    Return "deviance"
                Case "stddeviance", "standardizeddeviance"
                    Return "stddeviance"
                Case "leverage", "hat"
                    Return "leverage"
                Case Else
                    Return "all"
            End Select
        End Function
    End Class

    Public NotInheritable Class OrdinalLogitUDFsOptions38
        Private Sub New()
        End Sub

        Public Shared Function ParseOrdinalResidualType(v As String) As String
            Dim s As String = v
            If String.IsNullOrWhiteSpace(s) Then Return "all"

            Select Case LegacyUdfText38.CaseToken(s.Trim().ToLowerInvariant(), New String() {"all", "fittedmean", "mean", "fittedmeans", "prob", "probability", "probabilities", "fittedprob", "fittedprobability", "response", "raw", "pearson", "stdpearson", "standardizedpearson", "deviance", "dev", "stddeviance", "standardizeddeviance", "leverage", "hat"})
                Case "all"
                    Return "all"
                Case "fittedmean", "mean", "fittedmeans"
                    Return "fittedmean"
                Case "prob", "probability", "probabilities", "fittedprob", "fittedprobability"
                    Return "prob"
                Case "response", "raw"
                    Return "response"
                Case "pearson"
                    Return "pearson"
                Case "stdpearson", "standardizedpearson"
                    Return "stdpearson"
                Case "deviance", "dev"
                    Return "deviance"
                Case "stddeviance", "standardizeddeviance"
                    Return "stddeviance"
                Case "leverage", "hat"
                    Return "leverage"
                Case Else
                    Return "all"
            End Select
        End Function
    End Class

    Public NotInheritable Class ZIPUDFsOptions38
        Private Sub New()
        End Sub

        Public Shared Function ParseZipComponent(v As String) As String
            Dim s As String = v
            If String.IsNullOrWhiteSpace(s) Then Return "all"

            Select Case LegacyUdfText38.CaseToken(NormalizeKey(s), New String() {"all", "count", "poisson", "countpart", "poissonpart", "zero", "logistic", "zeropart", "inflation", "zeroinflation"})
                Case "all"
                    Return "all"
                Case "count", "poisson", "countpart", "poissonpart"
                    Return "count"
                Case "zero", "logistic", "zeropart", "inflation", "zeroinflation"
                    Return "zero"
                Case Else
                    Return Nothing
            End Select
        End Function

        Public Shared Function ParseZipResidualType(v As String) As String
            Dim s As String = v
            If String.IsNullOrWhiteSpace(s) Then Return "all"

            Select Case LegacyUdfText38.CaseToken(NormalizeKey(s), New String() {"all", "raw", "response", "pearson"})
                Case "all"
                    Return "all"
                Case "raw", "response"
                    Return "raw"
                Case "pearson"
                    Return "pearson"
                Case Else
                    Return "all"
            End Select
        End Function
    End Class

    Public NotInheritable Class SampleSizeUDFsOptions38
        Private Sub New()
        End Sub
        Private Const HypothesisSuperiority As String = "superiority"
        Private Const HypothesisNonInferiority As String = "noninferiority"
        Private Const HypothesisEquivalence As String = "equivalence"

        Public Shared Function ResolveSampleSizeHypothesisType(value As String) As String
            Dim s As String = value
            If String.IsNullOrWhiteSpace(s) Then Return HypothesisSuperiority

            Select Case LegacyUdfText38.CaseToken(s.Trim().ToLowerInvariant(), New String() {"superiority", "superior", "sup", "noninferiority", "non-inferiority", "noninferior", "non-inferior", "ni", "equivalence", "equivalent", "equiv", "eq"})
                Case "superiority", "superior", "sup"
                    Return HypothesisSuperiority
                Case "noninferiority", "non-inferiority", "noninferior", "non-inferior", "ni"
                    Return HypothesisNonInferiority
                Case "equivalence", "equivalent", "equiv", "eq"
                    Return HypothesisEquivalence
                Case Else
                    Return Nothing
            End Select
        End Function
    End Class

    Public NotInheritable Class UdfDataImportOptions38
        Private Sub New()
        End Sub

        Public Shared Function GetCoxTieMethod(arg As String, defaultValue As TieMethod) As TieMethod
            Dim s As String = arg
            If String.IsNullOrWhiteSpace(s) Then Return defaultValue

            Select Case LegacyUdfText38.CaseToken(s.Trim().ToLowerInvariant(), New String() {"breslow", "efron", "exact"})
                Case "breslow"
                    Return TieMethod.Breslow
                Case "efron"
                    Return TieMethod.Efron
                Case "exact"
                    Return TieMethod.Exact
                Case Else
                    Return defaultValue
            End Select
        End Function

        Public Shared Function ParseAgreementCiMethod(arg As String, defaultValue As Agreement.AgreementCiMethod) As Agreement.AgreementCiMethod
            Dim s As String = NormalizeToken(arg)
            If LegacyUdfText38.EqualText(s, "") Then Return defaultValue
            Select Case LegacyUdfText38.CaseToken(s, New String() {"ANALYTICAL", "ANALYTIC", "JACKKNIFE", "JACK", "BOOTSTRAP", "PERCENTILE", "BOOTSTRAPPERCENTILE", "BOOTSTRAP_PERCENTILE", "BCA", "BOOTSTRAPBCA", "BOOTSTRAP_BCA"})
                Case "ANALYTICAL", "ANALYTIC"
                    Return Agreement.AgreementCiMethod.Analytical
                Case "JACKKNIFE", "JACK"
                    Return Agreement.AgreementCiMethod.Jackknife
                Case "BOOTSTRAP", "PERCENTILE", "BOOTSTRAPPERCENTILE", "BOOTSTRAP_PERCENTILE"
                    Return Agreement.AgreementCiMethod.BootstrapPercentile
                Case "BCA", "BOOTSTRAPBCA", "BOOTSTRAP_BCA"
                    Return Agreement.AgreementCiMethod.BootstrapBCa
                Case Else
                    Throw New ArgumentException("Unsupported ciMethod. Use analytical, jackknife, bootstrap, or bca.")
            End Select
        End Function

        Public Shared Function ParseBlandAltmanMode(arg As String, defaultValue As Agreement.RepeatedBlandAltmanMode) As Agreement.RepeatedBlandAltmanMode
            Dim s As String = NormalizeToken(arg)
            If LegacyUdfText38.EqualText(s, "") Then Return defaultValue
            Select Case LegacyUdfText38.CaseToken(s, New String() {"AUTO", "SIMPLE", "PAIRS", "SIMPLEPAIRS", "REPEATED", "REPEATEDBYSUBJECT", "SUBJECT"})
                Case "AUTO"
                    Return Agreement.RepeatedBlandAltmanMode.Auto
                Case "SIMPLE", "PAIRS", "SIMPLEPAIRS"
                    Return Agreement.RepeatedBlandAltmanMode.SimplePairs
                Case "REPEATED", "REPEATEDBYSUBJECT", "SUBJECT"
                    Return Agreement.RepeatedBlandAltmanMode.RepeatedBySubject
                Case Else
                    Throw New ArgumentException("Unsupported Bland–Altman mode. Use auto, simple, or repeated.")
            End Select
        End Function

        Public Shared Function ParseBlandAltmanScale(arg As String, defaultValue As Agreement.BlandAltmanScale) As Agreement.BlandAltmanScale
            Dim s As String = NormalizeToken(arg)
            If LegacyUdfText38.EqualText(s, "") Then Return defaultValue
            Select Case LegacyUdfText38.CaseToken(s, New String() {"RAW", "RAWDIFFERENCE", "DIFF", "DIFFERENCE", "MEANPCT", "PERCENTOFMEAN", "PCTMEAN", "PERCENTMEAN", "REFPCT", "PERCENTOFREFERENCE", "PCTREF", "PERCENTREF", "REFERENCE", "TESTPCT", "PERCENTOFTEST", "PCTTEST", "PERCENTTEST", "LOGRATIO", "LOG", "RATIO"})
                Case "RAW", "RAWDIFFERENCE", "DIFF", "DIFFERENCE"
                    Return Agreement.BlandAltmanScale.RawDifference
                Case "MEANPCT", "PERCENTOFMEAN", "PCTMEAN", "PERCENTMEAN"
                    Return Agreement.BlandAltmanScale.PercentOfMean
                Case "REFPCT", "PERCENTOFREFERENCE", "PCTREF", "PERCENTREF", "REFERENCE"
                    Return Agreement.BlandAltmanScale.PercentOfReference
                Case "TESTPCT", "PERCENTOFTEST", "PCTTEST", "PERCENTTEST"
                    Return Agreement.BlandAltmanScale.PercentOfTest
                Case "LOGRATIO", "LOG", "RATIO"
                    Return Agreement.BlandAltmanScale.LogRatio
                Case Else
                    Throw New ArgumentException("Unsupported Bland–Altman scale.")
            End Select
        End Function

        Public Shared Function ParseBlandAltmanXAxisMode(arg As String, defaultValue As Agreement.BlandAltmanXAxisMode) As Agreement.BlandAltmanXAxisMode
            Dim s As String = NormalizeToken(arg)
            If LegacyUdfText38.EqualText(s, "") Then Return defaultValue
            Select Case LegacyUdfText38.CaseToken(s, New String() {"MEAN", "MEANOFMETHODS", "REFERENCE", "REF", "X", "TEST", "Y"})
                Case "MEAN", "MEANOFMETHODS"
                    Return Agreement.BlandAltmanXAxisMode.MeanOfMethods
                Case "REFERENCE", "REF", "X"
                    Return Agreement.BlandAltmanXAxisMode.ReferenceMethod
                Case "TEST", "Y"
                    Return Agreement.BlandAltmanXAxisMode.TestMethod
                Case Else
                    Throw New ArgumentException("Unsupported Bland–Altman xAxis.")
            End Select
        End Function

        Public Shared Function ParseBlandAltmanPlotMode(arg As String, defaultValue As Agreement.RepeatedBlandAltmanPlotMode) As Agreement.RepeatedBlandAltmanPlotMode
            Dim s As String = NormalizeToken(arg)
            If LegacyUdfText38.EqualText(s, "") Then Return defaultValue
            Select Case LegacyUdfText38.CaseToken(s, New String() {"ALL", "OBS", "ALLOBSERVATIONS", "MEANS", "SUBJECTMEANS", "MEANSONLY", "BOTH", "ALLANDMEANS", "ALLOBSERVATIONSANDSUBJECTMEANS"})
                Case "ALL", "OBS", "ALLOBSERVATIONS"
                    Return Agreement.RepeatedBlandAltmanPlotMode.AllObservations
                Case "MEANS", "SUBJECTMEANS", "MEANSONLY"
                    Return Agreement.RepeatedBlandAltmanPlotMode.SubjectMeansOnly
                Case "BOTH", "ALLANDMEANS", "ALLOBSERVATIONSANDSUBJECTMEANS"
                    Return Agreement.RepeatedBlandAltmanPlotMode.AllObservationsAndSubjectMeans
                Case Else
                    Throw New ArgumentException("Unsupported Bland–Altman plotMode.")
            End Select
        End Function

        Public Shared Function ParseKappaWeighting(arg As String, defaultValue As Agreement.KappaWeightingScheme) As Agreement.KappaWeightingScheme
            Dim s As String = NormalizeToken(arg)
            If LegacyUdfText38.EqualText(s, "") Then Return defaultValue
            Select Case LegacyUdfText38.CaseToken(s, New String() {"UNWEIGHTED", "COHEN", "NONE", "LINEAR", "QUADRATIC", "CICCHETTI", "CICCHETTIALLISON", "CA", "FLEISS", "FLEISSCOHEN", "FC", "CUSTOM"})
                Case "UNWEIGHTED", "COHEN", "NONE"
                    Return Agreement.KappaWeightingScheme.Unweighted
                Case "LINEAR"
                    Return Agreement.KappaWeightingScheme.Linear
                Case "QUADRATIC"
                    Return Agreement.KappaWeightingScheme.Quadratic
                Case "CICCHETTI", "CICCHETTIALLISON", "CA"
                    Return Agreement.KappaWeightingScheme.CicchettiAllison
                Case "FLEISS", "FLEISSCOHEN", "FC"
                    Return Agreement.KappaWeightingScheme.FleissCohen
                Case "CUSTOM"
                    Return Agreement.KappaWeightingScheme.Custom
                Case Else
                    Throw New ArgumentException("Unsupported kappa weighting scheme.")
            End Select
        End Function

        Public Shared Function ParseDemingVarianceModel(arg As String, defaultValue As Agreement.DemingVarianceModel) As Agreement.DemingVarianceModel
            Dim s As String = NormalizeToken(arg)
            If LegacyUdfText38.EqualText(s, "") Then Return defaultValue
            Select Case LegacyUdfText38.CaseToken(s, New String() {"LAMBDA", "CONSTANTLAMBDA", "CONSTANT", "POINTWISE", "KNOWNPOINTWISESD", "SD", "POINTWISESD", "CV", "CONSTANTCV"})
                Case "LAMBDA", "CONSTANTLAMBDA", "CONSTANT"
                    Return Agreement.DemingVarianceModel.ConstantLambda
                Case "POINTWISE", "KNOWNPOINTWISESD", "SD", "POINTWISESD"
                    Return Agreement.DemingVarianceModel.KnownPointwiseSD
                Case "CV", "CONSTANTCV"
                    Return Agreement.DemingVarianceModel.ConstantCV
                Case Else
                    Throw New ArgumentException("Unsupported Deming varianceModel.")
            End Select
        End Function

        Public Shared Function ParseIccModel(arg As String, defaultValue As String) As String
            Dim s As String = NormalizeToken(arg)
            If LegacyUdfText38.EqualText(s, "") Then Return defaultValue
            Select Case LegacyUdfText38.CaseToken(s, New String() {"ICC11", "11", "ICC1K", "1K", "ICC21", "21", "ICC2K", "2K", "ICC31", "31", "ICC3K", "3K"})
                Case "ICC11", "11"
                    Return "ICC11"
                Case "ICC1K", "1K"
                    Return "ICC1K"
                Case "ICC21", "21"
                    Return "ICC21"
                Case "ICC2K", "2K"
                    Return "ICC2K"
                Case "ICC31", "31"
                    Return "ICC31"
                Case "ICC3K", "3K"
                    Return "ICC3K"
                Case Else
                    Throw New ArgumentException("Unsupported ICC model. Use ICC11, ICC1K, ICC21, ICC2K, ICC31, or ICC3K.")
            End Select
        End Function

        Public Shared Function GetFormulaAddressingMode(arg As String, defaultMode As String) As String
            Dim s As String = arg
            If String.IsNullOrWhiteSpace(s) Then Return defaultMode

            Select Case LegacyUdfText38.CaseToken(s.Trim().ToLowerInvariant(), New String() {"relative", "rel", "x", "absolute", "abs", "worksheet", "names", "name", "quoted", "variables", "varnames"})
                Case "relative", "rel", "x"
                    Return "relative"
                Case "absolute", "abs", "worksheet"
                    Return "absolute"
                Case "names", "name", "quoted", "variables", "varnames"
                    Return "names"
                Case Else
                    Return defaultMode
            End Select
        End Function

        Public Shared Function NormalizeOptionKey(value As String) As String
            If value Is Nothing Then Return String.Empty

            Return value.Trim().ToLowerInvariant().Replace(" ", String.Empty).Replace("-", String.Empty).Replace("_", String.Empty)
        End Function

        Public Shared Function GetHistogramBinRule(arg As String) As String
            Dim token As String = NormalizeOptionKey(arg)
            If String.IsNullOrWhiteSpace(token) Then Return "(Sturges)"

            Select Case LegacyUdfText38.CaseToken(token, New String() {"sturges", "sturge", "doane", "doan", "scott", "freedmandiaconis", "fd"})
                Case "sturges", "sturge"
                    Return "(Sturges)"
                Case "doane", "doan"
                    Return "(Doane)"
                Case "scott"
                    Return "(Scott)"
                Case "freedmandiaconis", "fd"
                    Return "(Freedman-Diaconis)"
                Case Else
                    Return "(Sturges)"
            End Select
        End Function

        Public Shared Function ParseRocDirection(direction As String) As Boolean
            Dim token As String = NormalizeOptionKey(direction)
            If String.IsNullOrWhiteSpace(token) Then Return False
            Return (LegacyUdfText38.EqualText(token, "lower") OrElse LegacyUdfText38.EqualText(token, "low") OrElse LegacyUdfText38.EqualText(token, "smaller") OrElse LegacyUdfText38.EqualText(token, "decreasing"))
        End Function

        Public Shared Function GetRegressionFamilyCode(arg As String) As String
            Dim s As String = arg
            If String.IsNullOrWhiteSpace(s) Then Return "Gaussian"

            Select Case LegacyUdfText38.CaseToken(NormalizeRegressionOptionKey(s), New String() {"binomial", "binary", "logistic", "poisson", "count", "negativebinomial", "negativebinom", "negativebin", "negbin", "nb", "nb2", "gaussian", "normal", "gamma"})
                Case "binomial", "binary", "logistic"
                    Return "Binomial"
                Case "poisson", "count"
                    Return "Poisson"
                Case "negativebinomial", "negativebinom", "negativebin", "negbin", "nb", "nb2"
                    Return "NegativeBinomial"
                Case "gaussian", "normal"
                    Return "Gaussian"
                Case "gamma"
                    Return "Gamma"
                Case Else
                    Return Nothing
            End Select
        End Function

        Public Shared Function GetRegressionLinkName(arg As String, familyDisplayName As String) As String
            Dim s As String = arg
            If String.IsNullOrWhiteSpace(s) Then
                Return Global.BESHStatNG.regression.GetCanonicalLinkFromDisplayName(familyDisplayName)
            End If

            Select Case LegacyUdfText38.CaseToken(NormalizeRegressionOptionKey(s), New String() {"logit", "probit", "log", "identity", "id", "sqrt", "squareroot", "inverse", "reciprocal", "power"})
                Case "logit"
                    Return "Logit"
                Case "probit"
                    Return "Probit"
                Case "log"
                    Return "Log"
                Case "identity", "id"
                    Return "Identity"
                Case "sqrt", "squareroot"
                    Return "Sqrt"
                Case "inverse", "reciprocal"
                    Return "Inverse"
                Case "power"
                    Return "Power"
                Case Else
                    Return Nothing
            End Select
        End Function

        Public Shared Function NormalizeRegressionOptionKey(value As String) As String
            If value Is Nothing Then Return String.Empty

            Dim chars As New List(Of Char)()
            For Each ch As Char In value.Trim().ToLowerInvariant()
                If Char.IsLetterOrDigit(ch) Then chars.Add(ch)
            Next

            Return New String(chars.ToArray())
        End Function
    End Class

    Public NotInheritable Class UdfOutputTablesOptions38
        Private Sub New()
        End Sub

        Public Shared Function NormalizeResultTableTitle(title As String) As String
            If title Is Nothing Then Return String.Empty
            Return New String(title.Trim().ToUpperInvariant().Where(Function(ch) Char.IsLetterOrDigit(ch)).ToArray())
        End Function
    End Class

    Public NotInheritable Class UdfScalarUtilitiesOptions38
        Private Sub New()
        End Sub

        Public Shared Function NormalizeMatchToken(value As String, Optional toUpper As Boolean = True, Optional removeUnderscore As Boolean = False) As String
            If value Is Nothing Then Return String.Empty

            Dim normalized As String = value.Trim()
            normalized = If(toUpper, normalized.ToUpperInvariant(), normalized.ToLowerInvariant())
            normalized = normalized.Replace(" ", String.Empty).Replace("-", String.Empty)
            If removeUnderscore Then normalized = normalized.Replace("_", String.Empty)
            Return normalized
        End Function

        Public Shared Function NormalizeText(v As String) As String
            Return NormalizeMatchToken(v, toUpper:=True, removeUnderscore:=False)
        End Function

        Public Shared Function NormalizeToken(arg As String) As String
            Return NormalizeMatchToken(arg, toUpper:=True, removeUnderscore:=False)
        End Function

        Public Shared Function NormalizeKey(value As String) As String
            Return NormalizeMatchToken(value, toUpper:=False, removeUnderscore:=True)
        End Function

        Public Shared Function NormalizeMixedModelOptionToken(arg As String) As String
            Dim raw As String = arg
            If String.IsNullOrWhiteSpace(raw) Then Return String.Empty
            Return New String(raw.Trim().ToUpperInvariant().Where(Function(ch) Char.IsLetterOrDigit(ch)).ToArray())
        End Function

        Public Shared Function ParseMixedModelFitMethodStrict(arg As String, analysisLabel As String) As Global.BESHStatNG.regression.MixedModelFitMethod
            Dim token As String = NormalizeMixedModelOptionToken(arg)
            If LegacyUdfText38.EqualText(token, String.Empty) OrElse LegacyUdfText38.EqualText(token, "REML") OrElse LegacyUdfText38.EqualText(token, "RESTRICTEDML") OrElse LegacyUdfText38.EqualText(token, "RESTRICTEDMAXIMUMLIKELIHOOD") Then
                Return Global.BESHStatNG.regression.MixedModelFitMethod.REML
            End If
            If LegacyUdfText38.EqualText(token, "ML") OrElse LegacyUdfText38.EqualText(token, "MAXIMUMLIKELIHOOD") Then Return Global.BESHStatNG.regression.MixedModelFitMethod.ML

            Throw New ArgumentException("Unsupported " & analysisLabel & " fitMethod. Use REML or ML.")
        End Function

        Public Shared Function ParseMixedModelInferenceMethodStrict(arg As String, analysisLabel As String) As Global.BESHStatNG.regression.MixedModelFixedInferenceMethod
            Dim token As String = NormalizeMixedModelOptionToken(arg)
            If LegacyUdfText38.EqualText(token, String.Empty) OrElse LegacyUdfText38.EqualText(token, "KR") OrElse LegacyUdfText38.EqualText(token, "KENWARDROGER") OrElse LegacyUdfText38.EqualText(token, "KENWARDROGERDF") OrElse LegacyUdfText38.EqualText(token, "KENWARDROGERF") Then
                Return Global.BESHStatNG.regression.MixedModelFixedInferenceMethod.KenwardRoger
            End If
            If LegacyUdfText38.EqualText(token, "SAT") OrElse LegacyUdfText38.EqualText(token, "SATTERTHWAITE") OrElse LegacyUdfText38.EqualText(token, "SATTERTHWAITEDF") Then Return Global.BESHStatNG.regression.MixedModelFixedInferenceMethod.Satterthwaite
            If LegacyUdfText38.EqualText(token, "BW") OrElse LegacyUdfText38.EqualText(token, "BETWEENWITHIN") OrElse LegacyUdfText38.EqualText(token, "BETWEENWITHINDF") Then Return Global.BESHStatNG.regression.MixedModelFixedInferenceMethod.BetweenWithin
            If LegacyUdfText38.EqualText(token, "RESID") OrElse LegacyUdfText38.EqualText(token, "RESIDUAL") OrElse LegacyUdfText38.EqualText(token, "RESIDUALDF") Then Return Global.BESHStatNG.regression.MixedModelFixedInferenceMethod.ResidualDF
            If LegacyUdfText38.EqualText(token, "WALD") OrElse LegacyUdfText38.EqualText(token, "NORMAL") OrElse LegacyUdfText38.EqualText(token, "WALDNORMAL") OrElse LegacyUdfText38.EqualText(token, "Z") OrElse LegacyUdfText38.EqualText(token, "LARGESAMPLENORMAL") Then Return Global.BESHStatNG.regression.MixedModelFixedInferenceMethod.WaldNormal

            Throw New ArgumentException("Unsupported " & analysisLabel & " inference method. Use KR, Satterthwaite, BetweenWithin, ResidualDF, or Wald.")
        End Function

        Public Shared Function ParseMixedModelCovarianceOptimizerModeStrict(arg As String, analysisLabel As String) As Global.BESHStatNG.regression.MixedModelCovarianceOptimizerMode
            Dim token As String = NormalizeMixedModelOptionToken(arg)
            If LegacyUdfText38.EqualText(token, String.Empty) OrElse LegacyUdfText38.EqualText(token, "AI") OrElse LegacyUdfText38.EqualText(token, "AIREML") OrElse LegacyUdfText38.EqualText(token, "AVERAGEINFORMATION") OrElse
               LegacyUdfText38.EqualText(token, "AVERAGEINFORMATIONREML") OrElse LegacyUdfText38.EqualText(token, "FISHER") OrElse LegacyUdfText38.EqualText(token, "FISHERSCORING") OrElse
               LegacyUdfText38.EqualText(token, "FISHERSCORE") OrElse LegacyUdfText38.EqualText(token, "SAS") OrElse LegacyUdfText38.EqualText(token, "PROCMIXED") OrElse LegacyUdfText38.EqualText(token, "PROCMIXEDSTYLE") OrElse
               LegacyUdfText38.EqualText(token, "AIFISHERSCORINGDEFAULT") Then
                Return Global.BESHStatNG.regression.MixedModelCovarianceOptimizerMode.AverageInformationReml
            End If

            If LegacyUdfText38.EqualText(token, "BFGS") OrElse LegacyUdfText38.EqualText(token, "PROJECTEDBFGS") OrElse LegacyUdfText38.EqualText(token, "PROJECTEDBFGSAUTO") OrElse
               LegacyUdfText38.EqualText(token, "BFGSAUTOGRADIENT") OrElse LegacyUdfText38.EqualText(token, "PROJECTEDBFGSAUTOGRADIENT") Then
                Return Global.BESHStatNG.regression.MixedModelCovarianceOptimizerMode.ProjectedBfgs
            End If

            If LegacyUdfText38.EqualText(token, "BFGSANALYTIC") OrElse LegacyUdfText38.EqualText(token, "PROJECTEDBFGSANALYTIC") OrElse
               LegacyUdfText38.EqualText(token, "PROJECTEDBFGSANALYTICGRADIENT") OrElse LegacyUdfText38.EqualText(token, "ANALYTICBFGS") Then
                Return Global.BESHStatNG.regression.MixedModelCovarianceOptimizerMode.ProjectedBfgsAnalyticGradient
            End If

            If LegacyUdfText38.EqualText(token, "BFGSNUMERICAL") OrElse LegacyUdfText38.EqualText(token, "PROJECTEDBFGSNUMERICAL") OrElse
               LegacyUdfText38.EqualText(token, "BFGSFINITE") OrElse LegacyUdfText38.EqualText(token, "BFGSFINITEDIFFERENCE") OrElse LegacyUdfText38.EqualText(token, "NUMERICALBFGS") Then
                Return Global.BESHStatNG.regression.MixedModelCovarianceOptimizerMode.ProjectedBfgs
            End If

            Throw New ArgumentException("Unsupported " & analysisLabel & " covOptimizerMode. Use AI, AverageInformation, FisherScoring, SAS, BFGS, BFGS_ANALYTIC, or BFGS_NUMERICAL.")
        End Function

        Public Shared Function ParseMixedModelCovarianceGradientModeStrict(arg As String, analysisLabel As String) As Global.BESHStatNG.regression.MixedModelCovarianceGradientMode
            Dim token As String = NormalizeMixedModelOptionToken(arg)
            If LegacyUdfText38.EqualText(token, String.Empty) OrElse LegacyUdfText38.EqualText(token, "AUTO") OrElse LegacyUdfText38.EqualText(token, "AUTOMATIC") OrElse LegacyUdfText38.EqualText(token, "AUTOANALYTICWHEREAVAILABLE") Then
                Return Global.BESHStatNG.regression.MixedModelCovarianceGradientMode.Auto
            End If
            If LegacyUdfText38.EqualText(token, "ANALYTIC") OrElse LegacyUdfText38.EqualText(token, "ANALYTICSCORE") OrElse LegacyUdfText38.EqualText(token, "SCORE") Then
                Return Global.BESHStatNG.regression.MixedModelCovarianceGradientMode.AnalyticScore
            End If
            If LegacyUdfText38.EqualText(token, "ANALYTICVALIDATION") OrElse LegacyUdfText38.EqualText(token, "ANALYTICSCOREVALIDATION") OrElse LegacyUdfText38.EqualText(token, "ANALYTICWITHVALIDATION") OrElse
               LegacyUdfText38.EqualText(token, "VALIDATE") OrElse LegacyUdfText38.EqualText(token, "VALIDATION") OrElse LegacyUdfText38.EqualText(token, "ANALYTICSCOREFINITEDIFFERENCEVALIDATION") OrElse
               LegacyUdfText38.EqualText(token, "ANALYTICSCOREWITHFINITEDIFFERENCEVALIDATION") Then
                Return Global.BESHStatNG.regression.MixedModelCovarianceGradientMode.AnalyticScoreWithFiniteDifferenceValidation
            End If
            If LegacyUdfText38.EqualText(token, "NUMERICAL") OrElse LegacyUdfText38.EqualText(token, "FINITE") OrElse LegacyUdfText38.EqualText(token, "FINITEDIFFERENCE") OrElse
               LegacyUdfText38.EqualText(token, "FD") OrElse LegacyUdfText38.EqualText(token, "NUMERICALFINITEDIFFERENCE") Then
                Return Global.BESHStatNG.regression.MixedModelCovarianceGradientMode.NumericalFiniteDifference
            End If

            Throw New ArgumentException("Unsupported " & analysisLabel & " covGradientMode. Use Auto, Analytic, AnalyticValidation, Validate, Numerical, or FiniteDifference.")
        End Function

        Public Shared Sub ApplyMixedModelOptimizerShortcutToGradient(optimizerArg As String,
                                                             ByRef gradientMode As Global.BESHStatNG.regression.MixedModelCovarianceGradientMode)
            Dim token As String = NormalizeMixedModelOptionToken(optimizerArg)
            If LegacyUdfText38.EqualText(token, "BFGSANALYTIC") OrElse LegacyUdfText38.EqualText(token, "PROJECTEDBFGSANALYTIC") OrElse
               LegacyUdfText38.EqualText(token, "PROJECTEDBFGSANALYTICGRADIENT") OrElse LegacyUdfText38.EqualText(token, "ANALYTICBFGS") Then
                gradientMode = Global.BESHStatNG.regression.MixedModelCovarianceGradientMode.AnalyticScore
            ElseIf LegacyUdfText38.EqualText(token, "BFGSNUMERICAL") OrElse LegacyUdfText38.EqualText(token, "PROJECTEDBFGSNUMERICAL") OrElse
                   LegacyUdfText38.EqualText(token, "BFGSFINITE") OrElse LegacyUdfText38.EqualText(token, "BFGSFINITEDIFFERENCE") OrElse LegacyUdfText38.EqualText(token, "NUMERICALBFGS") Then
                gradientMode = Global.BESHStatNG.regression.MixedModelCovarianceGradientMode.NumericalFiniteDifference
            End If
        End Sub

        Public Shared Function NormalizeMixedModelResidualCovarianceName(arg As String, defaultValue As String) As String
            Dim token As String = NormalizeMixedModelOptionToken(arg)
            If LegacyUdfText38.EqualText(token, String.Empty) Then Return defaultValue
            If LegacyUdfText38.EqualText(token, "ID") OrElse LegacyUdfText38.EqualText(token, "IDENTITY") OrElse LegacyUdfText38.EqualText(token, "INDEPENDENCE") Then Return "Identity"
            If LegacyUdfText38.EqualText(token, "DIAG") OrElse LegacyUdfText38.EqualText(token, "DIAGONAL") OrElse LegacyUdfText38.EqualText(token, "DIAGONALHETEROGENEOUS") OrElse LegacyUdfText38.EqualText(token, "HETEROGENEOUSDIAGONAL") Then Return "Diagonal"
            If LegacyUdfText38.EqualText(token, "CS") OrElse LegacyUdfText38.EqualText(token, "COMPOUNDSYMMETRY") OrElse LegacyUdfText38.EqualText(token, "EXCHANGEABLE") Then Return "CS"
            If LegacyUdfText38.EqualText(token, "HCS") OrElse LegacyUdfText38.EqualText(token, "CSH") OrElse LegacyUdfText38.EqualText(token, "HETEROGENEOUSCS") OrElse LegacyUdfText38.EqualText(token, "HETEROGENEOUSCOMPOUNDSYMMETRY") Then Return "Heterogeneous CS"
            If LegacyUdfText38.EqualText(token, "AR1") OrElse LegacyUdfText38.EqualText(token, "AR") OrElse LegacyUdfText38.EqualText(token, "AUTOREGRESSIVE") Then Return "AR(1)"
            If LegacyUdfText38.EqualText(token, "HAR1") OrElse LegacyUdfText38.EqualText(token, "HAR") OrElse LegacyUdfText38.EqualText(token, "ARH1") OrElse LegacyUdfText38.EqualText(token, "ARH") OrElse LegacyUdfText38.EqualText(token, "HETEROGENEOUSAR1") OrElse LegacyUdfText38.EqualText(token, "HETEROGENEOUSAR") OrElse LegacyUdfText38.EqualText(token, "HETEROGENEOUSAUTOREGRESSIVE") Then Return "Heterogeneous AR(1)"
            If LegacyUdfText38.EqualText(token, "TOEP") OrElse LegacyUdfText38.EqualText(token, "TOEPLITZ") Then Return "Toeplitz (TOEP)"
            If LegacyUdfText38.EqualText(token, "TOEPH") OrElse LegacyUdfText38.EqualText(token, "TOEPHETEROGENEOUS") OrElse LegacyUdfText38.EqualText(token, "HETEROGENEOUSTOEP") OrElse LegacyUdfText38.EqualText(token, "HETEROGENEOUSTOEPLITZ") OrElse LegacyUdfText38.EqualText(token, "TOEPLITZHETEROGENEOUS") Then Return "Heterogeneous Toeplitz (TOEPH)"
            If LegacyUdfText38.EqualText(token, "UN") OrElse LegacyUdfText38.EqualText(token, "UNSTRUCTURED") Then Return "UN"

            Return arg
        End Function
    End Class
End Namespace
