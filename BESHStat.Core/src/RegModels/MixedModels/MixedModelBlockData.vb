Option Explicit On
Option Strict On

Imports System
Imports System.Collections.Generic
Imports System.Linq

Namespace regression

    ''' <summary>
    ''' Specifies the likelihood criterion used when fitting Gaussian mixed models.
    ''' </summary>
    Public Enum MixedModelFitMethod
        ML = 0
        REML = 1
    End Enum

    Public Enum MixedModelFixedInferenceMethod
        ''' <summary>
        ''' Large-sample Wald normal inference.  Reports z statistics and normal p-values.
        ''' </summary>
        WaldNormal = 0

        ''' <summary>
        ''' Student-t inference using residual degrees of freedom n - p.
        ''' </summary>
        ResidualDF = 1

        ''' <summary>
        ''' MMRM-focused R mmrm-style between-within denominator-df approximation.
        ''' The intercept and fixed-effect columns that vary within subject receive within-subject df;
        ''' subject-constant fixed-effect columns receive between-subject df.
        ''' </summary>
        BetweenWithin = 2

        ''' <summary>
        ''' First-order Satterthwaite denominator degrees of freedom for fixed effects.
        ''' The covariance of covariance parameters is approximated from the finite-difference
        ''' Hessian of the profiled ML/REML criterion, and the variance of each fixed-effect
        ''' variance estimate is obtained by finite differences.
        ''' </summary>
        Satterthwaite = 3

        ''' <summary>
        ''' Kenward-Roger fixed-effect inference.
        ''' Uses the KR-adjusted coefficient covariance matrix plus R mmrm-style
        ''' denominator degrees of freedom for one-dimensional coefficient/contrast
        ''' tests and KR F scaling for multi-row hypotheses.
        ''' </summary>
        KenwardRoger = 4

    End Enum

    ''' <summary>
    ''' Selects the covariance-parameter optimizer used after fixed effects are profiled out.
    ''' </summary>
    Public Enum MixedModelCovarianceOptimizerMode
        ''' <summary>Use the existing projected BFGS optimizer and the selected covariance-gradient mode.</summary>
        ProjectedBfgs = 0

        ''' <summary>Use projected BFGS with an analytic covariance gradient when available.</summary>
        ProjectedBfgsAnalyticGradient = 1

        ''' <summary>Use the experimental Average Information / Fisher-scoring optimizer for REML fits when available.</summary>
        AverageInformationReml = 2
    End Enum

    ''' <summary>
    ''' Runtime/control settings for the mixed-model optimizer.
    ''' </summary>
    Public Structure MixedModelControl
        Public MaxIter As Integer
        Public Epsilon As Double
        Public StepTolerance As Double
        Public FunctionTolerance As Double
        Public Trace As Boolean
        Public ProfileFixedEffects As Boolean
        Public EnableStructuredRestarts As Boolean
        Public WeakSupportMinimumPairCount As Integer
        Public UseBfgsCovarianceOptimization As Boolean
        Public UseKrPqrDesignPatternCache As Boolean
        Public UseKrPqrFastFactorization As Boolean
        Public UseAnalyticGradientDerivativePatternCache As Boolean
        Public CovarianceGradientMode As MixedModelCovarianceGradientMode
        Public CovarianceOptimizerMode As MixedModelCovarianceOptimizerMode
        Public AnalyticGradientValidationTolerance As Double
        Public FallbackToNumericalGradientOnAnalyticFailure As Boolean

        ''' <summary>
        ''' Returns defaults aligned with SAS PROC MIXED-style REML covariance optimization. Auto covariance gradients use analytic scores for validated structures and finite differences otherwise.
        ''' </summary>
        Public Shared Function CreateDefault() As MixedModelControl
            Return New MixedModelControl With {
                .MaxIter = 100,
                .Epsilon = 0.00000001,
                .StepTolerance = 0.0000001,
                .FunctionTolerance = 0.000000001,
                .Trace = False,
                .ProfileFixedEffects = True,
                .EnableStructuredRestarts = True,
                .WeakSupportMinimumPairCount = 5,
                .UseBfgsCovarianceOptimization = True,
                .UseKrPqrDesignPatternCache = True,
                .UseKrPqrFastFactorization = True,
                .UseAnalyticGradientDerivativePatternCache = True,
                .CovarianceGradientMode = MixedModelCovarianceGradientMode.Auto,
                .CovarianceOptimizerMode = MixedModelCovarianceOptimizerMode.AverageInformationReml,
                .AnalyticGradientValidationTolerance = 0.0001,
                .FallbackToNumericalGradientOnAnalyticFailure = True
            }
        End Function
    End Structure

    ''' <summary>
    ''' Represents one subject/cluster block used by the LMM/MMRM likelihood.
    ''' </summary>
    Public Class MixedModelSubjectBlock

        Private pSubjectKey As String
        Private pRowIndices() As Integer
        Private pY() As Double
        Private pX(,) As Double
        Private pZ(,) As Double
        Private pVisit() As Double
        Private pVisitIndex() As Integer

        Public Sub New(subjectKey As String,
                       rowIndices() As Integer,
                       y() As Double,
                       x(,) As Double,
                       Optional z(,) As Double = Nothing,
                       Optional visit() As Double = Nothing,
                       Optional visitIndex() As Integer = Nothing)

            If String.IsNullOrWhiteSpace(subjectKey) Then subjectKey = String.Empty
            If y Is Nothing Then Throw New ArgumentNullException(NameOf(y))
            If x Is Nothing Then Throw New ArgumentNullException(NameOf(x))
            If rowIndices Is Nothing Then Throw New ArgumentNullException(NameOf(rowIndices))
            If rowIndices.Length <> y.Length Then Throw New ArgumentException("rowIndices must have the same length as y.")
            If x.GetLength(0) <> y.Length Then Throw New ArgumentException("X row count must match y length.")
            If z IsNot Nothing AndAlso z.GetLength(0) <> y.Length Then Throw New ArgumentException("Z row count must match y length.")
            If visit IsNot Nothing AndAlso visit.Length <> y.Length Then Throw New ArgumentException("visit must have the same length as y.")
            If visitIndex IsNot Nothing AndAlso visitIndex.Length <> y.Length Then Throw New ArgumentException("visitIndex must have the same length as y.")

            pSubjectKey = subjectKey
            pRowIndices = CType(rowIndices.Clone(), Integer())
            pY = CType(y.Clone(), Double())
            pX = CType(x.Clone(), Double(,))
            pZ = If(z Is Nothing, Nothing, CType(z.Clone(), Double(,)))
            pVisit = If(visit Is Nothing, Nothing, CType(visit.Clone(), Double()))
            pVisitIndex = If(visitIndex Is Nothing, Nothing, CType(visitIndex.Clone(), Integer()))
        End Sub

        Public ReadOnly Property SubjectKey As String
            Get
                Return pSubjectKey
            End Get
        End Property

        Public ReadOnly Property RowIndices As Integer()
            Get
                Return CType(pRowIndices.Clone(), Integer())
            End Get
        End Property

        Public ReadOnly Property Y As Double()
            Get
                Return CType(pY.Clone(), Double())
            End Get
        End Property

        Public ReadOnly Property X As Double(,)
            Get
                Return CType(pX.Clone(), Double(,))
            End Get
        End Property

        Public ReadOnly Property Z As Double(,)
            Get
                Return If(pZ Is Nothing, Nothing, CType(pZ.Clone(), Double(,)))
            End Get
        End Property

        Public ReadOnly Property Visit As Double()
            Get
                Return If(pVisit Is Nothing, Nothing, CType(pVisit.Clone(), Double()))
            End Get
        End Property

        Public ReadOnly Property VisitIndex As Integer()
            Get
                Return If(pVisitIndex Is Nothing, Nothing, CType(pVisitIndex.Clone(), Integer()))
            End Get
        End Property

        Public ReadOnly Property Nobs As Integer
            Get
                Return pY.Length
            End Get
        End Property

        Public ReadOnly Property P As Integer
            Get
                Return pX.GetLength(1)
            End Get
        End Property

        Public ReadOnly Property Q As Integer
            Get
                If pZ Is Nothing Then Return 0
                Return pZ.GetLength(1)
            End Get
        End Property

        Public Function HasRandomEffectsDesign() As Boolean
            Return pZ IsNot Nothing AndAlso pZ.GetLength(1) > 0
        End Function

        Public Function HasVisit() As Boolean
            Return pVisit IsNot Nothing AndAlso pVisit.Length = pY.Length
        End Function

        Public Function ToTraceString() As String
            Return $"Subject='{pSubjectKey}', n={Nobs}, p={P}, q={Q}, hasVisit={HasVisit()}"
        End Function
    End Class

    ''' <summary>
    ''' Immutable subject-block view of the mixed-model dataset.
    ''' This is the primary input to the Gaussian mixed-model likelihood.
    ''' </summary>
    Public Class MixedModelBlockData

        Private ReadOnly pBlocks As List(Of MixedModelSubjectBlock)
        Private ReadOnly pUniqueVisitValues As Double()
        Private ReadOnly pVisitIndexMap As Dictionary(Of Double, Integer)
        Private ReadOnly pHasVisit As Boolean
        Private ReadOnly pP As Integer
        Private ReadOnly pQ As Integer
        Private ReadOnly pNobs As Integer

        Private Sub New(blocks As List(Of MixedModelSubjectBlock),
                        uniqueVisitValues() As Double,
                        visitIndexMap As Dictionary(Of Double, Integer),
                        hasVisit As Boolean,
                        p As Integer,
                        q As Integer,
                        nobs As Integer)
            pBlocks = blocks
            pUniqueVisitValues = If(uniqueVisitValues Is Nothing, Array.Empty(Of Double)(), CType(uniqueVisitValues.Clone(), Double()))
            pVisitIndexMap = If(visitIndexMap Is Nothing,
                                New Dictionary(Of Double, Integer)(),
                                New Dictionary(Of Double, Integer)(visitIndexMap))
            pHasVisit = hasVisit
            pP = p
            pQ = q
            pNobs = nobs
        End Sub

        Public ReadOnly Property Blocks As List(Of MixedModelSubjectBlock)
            Get
                Return New List(Of MixedModelSubjectBlock)(pBlocks)
            End Get
        End Property

        Public ReadOnly Property NoSubjects As Integer
            Get
                Return pBlocks.Count
            End Get
        End Property

        Public ReadOnly Property Nobs As Integer
            Get
                Return pNobs
            End Get
        End Property

        Public ReadOnly Property P As Integer
            Get
                Return pP
            End Get
        End Property

        Public ReadOnly Property Q As Integer
            Get
                Return pQ
            End Get
        End Property

        Public ReadOnly Property HasVisit As Boolean
            Get
                Return pHasVisit
            End Get
        End Property

        Public ReadOnly Property UniqueVisitValues As Double()
            Get
                Return CType(pUniqueVisitValues.Clone(), Double())
            End Get
        End Property

        Public ReadOnly Property VisitIndexMap As Dictionary(Of Double, Integer)
            Get
                Return New Dictionary(Of Double, Integer)(pVisitIndexMap)
            End Get
        End Property

        Public Function GetBlock(index As Integer) As MixedModelSubjectBlock
            Return pBlocks(index)
        End Function

        Public Function MaxClusterSize() As Integer
            If pBlocks.Count = 0 Then Return 0
            Return pBlocks.Max(Function(b) b.Nobs)
        End Function

        Public Function SubjectKeys() As String()
            Return pBlocks.Select(Function(b) b.SubjectKey).ToArray()
        End Function

        Public Shared Function FromArrays(y() As Double,
                                          x(,) As Double,
                                          subjectId() As Object,
                                          Optional z(,) As Double = Nothing,
                                          Optional visit() As Double = Nothing,
                                          Optional sortWithinSubjectByVisit As Boolean = True,
                                          Optional rowNumbers() As Integer = Nothing) As MixedModelBlockData

            ValidateInputs(y, x, subjectId, z, visit, rowNumbers)

            Dim n As Integer = y.Length
            Dim p As Integer = x.GetLength(1)
            Dim q As Integer = If(z Is Nothing, 0, z.GetLength(1))

            Dim effectiveRowNumbers() As Integer = If(rowNumbers Is Nothing,
                                                      Enumerable.Range(1, n).ToArray(),
                                                      CType(rowNumbers.Clone(), Integer()))

            Dim visitIndexMap As New Dictionary(Of Double, Integer)
            Dim uniqueVisitValues() As Double = Array.Empty(Of Double)()
            If visit IsNot Nothing Then
                uniqueVisitValues = visit.Distinct().OrderBy(Function(v) v).ToArray()
                For i = 0 To uniqueVisitValues.Length - 1
                    visitIndexMap(uniqueVisitValues(i)) = i
                Next
            End If

            Dim groups As New Dictionary(Of String, List(Of Integer))(StringComparer.Ordinal)
            For i = 0 To n - 1
                Dim key As String = NormalizeSubjectKey(subjectId(i))
                If Not groups.ContainsKey(key) Then groups.Add(key, New List(Of Integer))
                groups(key).Add(i)
            Next

            Dim blocks As New List(Of MixedModelSubjectBlock)(groups.Count)
            For Each kvp In groups
                Dim idx As List(Of Integer) = kvp.Value

                If sortWithinSubjectByVisit AndAlso visit IsNot Nothing Then
                    idx = idx.OrderBy(Function(i) visit(i)).ThenBy(Function(i) effectiveRowNumbers(i)).ToList()
                Else
                    idx = idx.OrderBy(Function(i) effectiveRowNumbers(i)).ToList()
                End If

                Dim blockRows() As Integer = idx.Select(Function(i) effectiveRowNumbers(i)).ToArray()
                Dim blockY() As Double = idx.Select(Function(i) y(i)).ToArray()
                Dim blockX(,) As Double = SliceRows(x, idx)
                Dim blockZ(,) As Double = If(z Is Nothing, Nothing, SliceRows(z, idx))
                Dim blockVisit() As Double = If(visit Is Nothing, Nothing, idx.Select(Function(i) visit(i)).ToArray())
                Dim blockVisitIndex() As Integer = Nothing
                If blockVisit IsNot Nothing Then
                    blockVisitIndex = blockVisit.Select(Function(v) visitIndexMap(v)).ToArray()
                End If

                blocks.Add(New MixedModelSubjectBlock(subjectKey:=kvp.Key,
                                                      rowIndices:=blockRows,
                                                      y:=blockY,
                                                      x:=blockX,
                                                      z:=blockZ,
                                                      visit:=blockVisit,
                                                      visitIndex:=blockVisitIndex))
            Next

            blocks = blocks.OrderBy(Function(b) b.SubjectKey, StringComparer.Ordinal).ToList()

            Return New MixedModelBlockData(blocks:=blocks,
                                           uniqueVisitValues:=uniqueVisitValues,
                                           visitIndexMap:=visitIndexMap,
                                           hasVisit:=(visit IsNot Nothing),
                                           p:=p,
                                           q:=q,
                                           nobs:=n)
        End Function

        Private Shared Sub ValidateInputs(y() As Double,
                                          x(,) As Double,
                                          subjectId() As Object,
                                          z(,) As Double,
                                          visit() As Double,
                                          rowNumbers() As Integer)

            If y Is Nothing Then Throw New ArgumentNullException(NameOf(y))
            If x Is Nothing Then Throw New ArgumentNullException(NameOf(x))
            If subjectId Is Nothing Then Throw New ArgumentNullException(NameOf(subjectId))
            If y.Length = 0 Then Throw New ArgumentException("y must be non-empty.")
            If x.GetLength(0) <> y.Length Then Throw New ArgumentException("X row count must equal y length.")
            If subjectId.Length <> y.Length Then Throw New ArgumentException("subjectId length must equal y length.")
            If z IsNot Nothing AndAlso z.GetLength(0) <> y.Length Then Throw New ArgumentException("Z row count must equal y length.")
            If visit IsNot Nothing AndAlso visit.Length <> y.Length Then Throw New ArgumentException("visit length must equal y length.")
            If rowNumbers IsNot Nothing AndAlso rowNumbers.Length <> y.Length Then Throw New ArgumentException("rowNumbers length must equal y length.")
            If x.GetLength(1) = 0 Then Throw New ArgumentException("X must contain at least one fixed-effect column.")
            If z IsNot Nothing AndAlso z.GetLength(1) = 0 Then Throw New ArgumentException("If Z is supplied it must contain at least one random-effect column.")

            For i = 0 To y.Length - 1
                If Double.IsNaN(y(i)) OrElse Double.IsInfinity(y(i)) Then
                    Throw New ArgumentException("y contains invalid numeric values.")
                End If
                For j = 0 To x.GetLength(1) - 1
                    If Double.IsNaN(x(i, j)) OrElse Double.IsInfinity(x(i, j)) Then
                        Throw New ArgumentException("X contains invalid numeric values.")
                    End If
                Next
                If z IsNot Nothing Then
                    For j = 0 To z.GetLength(1) - 1
                        If Double.IsNaN(z(i, j)) OrElse Double.IsInfinity(z(i, j)) Then
                            Throw New ArgumentException("Z contains invalid numeric values.")
                        End If
                    Next
                End If
                If visit IsNot Nothing AndAlso (Double.IsNaN(visit(i)) OrElse Double.IsInfinity(visit(i))) Then
                    Throw New ArgumentException("visit contains invalid numeric values.")
                End If
            Next
        End Sub

        Private Shared Function NormalizeSubjectKey(value As Object) As String
            If value Is Nothing Then Return String.Empty
            Return Convert.ToString(value, Globalization.CultureInfo.InvariantCulture)
        End Function

        Private Shared Function SliceRows(mat(,) As Double, rows As List(Of Integer)) As Double(,)
            Dim out(rows.Count - 1, mat.GetLength(1) - 1) As Double
            For i = 0 To rows.Count - 1
                Dim srcRow As Integer = rows(i)
                For j = 0 To mat.GetLength(1) - 1
                    out(i, j) = mat(srcRow, j)
                Next
            Next
            Return out
        End Function
    End Class

End Namespace
