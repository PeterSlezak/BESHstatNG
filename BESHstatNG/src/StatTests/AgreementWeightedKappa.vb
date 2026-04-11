Option Explicit On

Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports BESHStatNG.AppInfrastructure
Imports BESHStatNG.Multivariate

Namespace Agreement

    ''' <summary>
    ''' Computes Cohen's kappa and weighted kappa for paired categorical or ordinal ratings.
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' This class implements a native agreement-analysis workflow for two raters, methods, or scoring systems
    ''' that classify the same items into a common set of categories.
    ''' </para>
    ''' <para>
    ''' Supported weighting schemes are:
    ''' </para>
    ''' <list type="bullet">
    '''   <item><description>unweighted Cohen's kappa</description></item>
    '''   <item><description>linear weighted kappa</description></item>
    '''   <item><description>quadratic weighted kappa</description></item>
    '''   <item><description>Cicchetti–Allison weights (implemented as linear weights for ordered categories)</description></item>
    '''   <item><description>Fleiss–Cohen weights (implemented as quadratic weights for ordered categories)</description></item>
    '''   <item><description>custom user-supplied weights</description></item>
    ''' </list>
    ''' <para>
    ''' The class accepts either paired raw ratings or a pre-aggregated square confusion matrix.
    ''' For paired raw ratings, non-missing paired observations are filtered by pairwise complete-case logic.
    ''' </para>
    ''' <para>
    ''' Weighted kappa is computed as:
    ''' </para>
    ''' <para>
    ''' <c>κ_w = (P_o(w) − P_e(w)) / (1 − P_e(w))</c>
    ''' </para>
    ''' <para>
    ''' where <c>P_o(w)</c> is the observed weighted agreement and <c>P_e(w)</c> is the expected weighted agreement
    ''' under independence of the two raters. The same expression reduces to ordinary Cohen's kappa when the
    ''' weight matrix contains 1 on the diagonal and 0 off the diagonal.
    ''' </para>
    ''' <para>
    ''' The analytical standard error is obtained from a first-order delta-method approximation on the multinomial
    ''' cell probabilities. This provides a practical general-purpose approximation for weighted as well as
    ''' unweighted kappa in the first version.
    ''' </para>
    ''' <para>
    ''' Bootstrap percentile confidence intervals are supported for paired raw ratings, and also for square
    ''' contingency tables when all counts are non-negative integers so that the table can be expanded into item-level
    ''' pairs internally.
    ''' </para>
    ''' </remarks>
    Public Class WeightedKappaAgreement

        Private ReadOnly pRater1Raw As Object()
        Private ReadOnly pRater2Raw As Object()
        Private ReadOnly pVarR1 As String
        Private ReadOnly pVarR2 As String

        Private ReadOnly pInputTable As Double(,)
        Private ReadOnly pInputLabels As Object()
        Private ReadOnly pConstructedFromTable As Boolean

        Private pOptions As KappaOptions
        Private pResult As KappaResult
        Private pIsFitted As Boolean = False
        Private pDroppedPairCount As Integer = 0
        Private pSampleSize As Integer = 0
        Private pComputationNotes As New List(Of String)
        Private pUsedBootstrapCi As Boolean = False
        Private pBootstrapSeedUsed As Integer = Integer.MinValue

        ''' <summary>
        ''' Initializes a new weighted-kappa analysis object from paired raw ratings.
        ''' </summary>
        ''' <param name="r1">
        ''' Ratings from the first rater, method, or classification system.
        ''' </param>
        ''' <param name="r2">
        ''' Ratings from the second rater, method, or classification system.
        ''' </param>
        ''' <param name="varR1">
        ''' Display label for the first rating source.
        ''' </param>
        ''' <param name="varR2">
        ''' Display label for the second rating source.
        ''' </param>
        ''' <param name="opts">
        ''' Optional kappa-analysis options. If <c>Nothing</c>, a new <see cref="KappaOptions"/> instance is used.
        ''' </param>
        ''' <remarks>
        ''' <para>
        ''' The paired-rating constructor is the preferred entry point because it preserves the item-level data and
        ''' therefore supports bootstrap confidence intervals directly.
        ''' </para>
        ''' </remarks>
        Public Sub New(r1 As Object(),
                       r2 As Object(),
                       varR1 As String,
                       varR2 As String,
                       Optional opts As KappaOptions = Nothing)

            If r1 Is Nothing Then AppGlobals.BSerr.LogAndThrow(New ArgumentNullException(NameOf(r1)))
            If r2 Is Nothing Then AppGlobals.BSerr.LogAndThrow(New ArgumentNullException(NameOf(r2)))
            If r1.Length <> r2.Length Then
                AppGlobals.BSerr.LogAndThrow(New ArgumentException("The two rating arrays must have the same length."))
            End If
            If r1.Length < 2 Then
                AppGlobals.BSerr.LogAndThrow(New ArgumentException("At least two paired observations are required for kappa analysis."))
            End If

            Me.pRater1Raw = DirectCast(r1.Clone(), Object())
            Me.pRater2Raw = DirectCast(r2.Clone(), Object())
            Me.pVarR1 = If(String.IsNullOrWhiteSpace(varR1), "Rater 1", varR1.Trim())
            Me.pVarR2 = If(String.IsNullOrWhiteSpace(varR2), "Rater 2", varR2.Trim())
            Me.pOptions = If(opts, New KappaOptions())
            Me.pConstructedFromTable = False

            ValidateOptions(Me.pOptions)
        End Sub

        ''' <summary>
        ''' Initializes a new weighted-kappa analysis object from a square confusion matrix.
        ''' </summary>
        ''' <param name="table">
        ''' A square matrix of non-negative cell counts whose rows correspond to categories of the first rater and
        ''' whose columns correspond to categories of the second rater.
        ''' </param>
        ''' <param name="categoryLabels">
        ''' Labels of the categories corresponding to the rows and columns of <paramref name="table"/>.
        ''' </param>
        ''' <param name="opts">
        ''' Optional kappa-analysis options. If <c>Nothing</c>, a new <see cref="KappaOptions"/> instance is used.
        ''' </param>
        ''' <remarks>
        ''' <para>
        ''' The table constructor is useful when only an aggregated confusion matrix is available. Analytical standard
        ''' errors are always available. Bootstrap confidence intervals are available only when all table entries are
        ''' non-negative integers so that the table can be expanded into item-level pairs internally.
        ''' </para>
        ''' </remarks>
        Public Sub New(table As Double(,),
                       categoryLabels As Object(),
                       Optional opts As KappaOptions = Nothing)

            If table Is Nothing Then AppGlobals.BSerr.LogAndThrow(New ArgumentNullException(NameOf(table)))
            If categoryLabels Is Nothing Then AppGlobals.BSerr.LogAndThrow(New ArgumentNullException(NameOf(categoryLabels)))
            If table.GetLength(0) <> table.GetLength(1) Then
                AppGlobals.BSerr.LogAndThrow(New ArgumentException("The confusion matrix for weighted kappa must be square."))
            End If
            If table.GetLength(0) <> categoryLabels.Length Then
                AppGlobals.BSerr.LogAndThrow(New ArgumentException("The number of category labels must match the dimension of the square confusion matrix."))
            End If
            If table.GetLength(0) < 2 Then
                AppGlobals.BSerr.LogAndThrow(New ArgumentException("At least two categories are required for kappa analysis."))
            End If

            For i As Integer = 0 To table.GetLength(0) - 1
                For j As Integer = 0 To table.GetLength(1) - 1
                    If Double.IsNaN(table(i, j)) OrElse Double.IsInfinity(table(i, j)) OrElse table(i, j) < 0.0 Then
                        AppGlobals.BSerr.LogAndThrow(New ArgumentException("The confusion matrix must contain only finite non-negative counts."))
                    End If
                Next
            Next

            Me.pInputTable = DirectCast(table.Clone(), Double(,))
            Me.pInputLabels = DirectCast(categoryLabels.Clone(), Object())
            Me.pVarR1 = "Rater 1"
            Me.pVarR2 = "Rater 2"
            Me.pOptions = If(opts, New KappaOptions())
            Me.pConstructedFromTable = True

            ValidateOptions(Me.pOptions)
        End Sub

        ''' <summary>
        ''' Gets or sets the kappa-analysis options used by the class.
        ''' </summary>
        ''' <remarks>
        ''' <para>
        ''' Replacing the options invalidates any previously fitted result. The analysis will be recomputed the next
        ''' time <see cref="Fit"/> is called.
        ''' </para>
        ''' </remarks>
        Public Property Options As KappaOptions
            Get
                Return Me.pOptions
            End Get
            Set(value As KappaOptions)
                If value Is Nothing Then AppGlobals.BSerr.LogAndThrow(New ArgumentNullException(NameOf(value)))
                ValidateOptions(value)
                Me.pOptions = value
                Me.pIsFitted = False
                Me.pResult = Nothing
            End Set
        End Property

        ''' <summary>
        ''' Gets the fitted weighted-kappa result.
        ''' </summary>
        ''' <returns>
        ''' The current <see cref="KappaResult"/> instance if the analysis has been fitted; otherwise <c>Nothing</c>.
        ''' </returns>
        Public ReadOnly Property Result As KappaResult
            Get
                Return Me.pResult
            End Get
        End Property

        ''' <summary>
        ''' Fits the weighted-kappa model and returns the agreement result.
        ''' </summary>
        ''' <returns>
        ''' A populated <see cref="KappaResult"/> containing the kappa estimate, confidence interval, test summary,
        ''' observed agreement, expected agreement, the confusion matrix, and the weight matrix used in the analysis.
        ''' </returns>
        ''' <remarks>
        ''' <para>
        ''' The fit process performs these steps:
        ''' </para>
        ''' <list type="number">
        '''   <item><description>validate the options and the input structure</description></item>
        '''   <item><description>build or validate the square confusion matrix and category order</description></item>
        '''   <item><description>construct the requested weight matrix</description></item>
        '''   <item><description>compute weighted observed and expected agreement and the corresponding kappa coefficient</description></item>
        '''   <item><description>compute a confidence interval using the configured method</description></item>
        '''   <item><description>compute an approximate z-test for <c>H0: κ = 0</c></description></item>
        ''' </list>
        ''' </remarks>
        Public Function Fit(Optional progressBar As System.Windows.Forms.ProgressBar = Nothing,
                            Optional randomSeed As Integer = Integer.MinValue) As KappaResult
            If Me.pIsFitted AndAlso Me.pResult IsNot Nothing Then Return Me.pResult

            ValidateOptions(Me.pOptions)
            Me.pComputationNotes.Clear()
            Me.pUsedBootstrapCi = False
            Me.pBootstrapSeedUsed = Integer.MinValue

            Dim labels As Object()
            Dim table As Double(,)
            If Me.pConstructedFromTable Then
                labels = DirectCast(Me.pInputLabels.Clone(), Object())
                table = DirectCast(Me.pInputTable.Clone(), Double(,))
                If Me.pOptions.Categories IsNot Nothing AndAlso Me.pOptions.Categories.Length > 0 Then
                    Me.pComputationNotes.Add("Explicit category labels supplied to the table constructor take precedence over KappaOptions.Categories.")
                End If
            Else
                Dim built = BuildConfusionMatrixFromPairs(Me.pRater1Raw, Me.pRater2Raw, Me.pOptions.Categories)
                labels = built.Labels
                table = built.Table
                Me.pDroppedPairCount = built.DroppedCount
            End If

            Dim n As Double = SumMatrix(table)
            If n <= 0.0 Then
                AppGlobals.BSerr.LogAndThrow(New InvalidOperationException("The confusion matrix contains no observations."))
            End If
            Me.pSampleSize = CInt(Math.Round(n))

            Dim weights = BuildWeightMatrix(table.GetLength(0), Me.pOptions)
            Dim metrics = ComputeKappaMetrics(table, weights)

            Dim ci As ConfidenceIntervalResult
            If Me.pOptions.CiMethod = AgreementCiMethod.BootstrapPercentile OrElse Me.pOptions.CiMethod = AgreementCiMethod.BootstrapBCa Then
                Me.pUsedBootstrapCi = True
                Me.pBootstrapSeedUsed = ResolveRandomSeed(randomSeed)
                ci = ComputeBootstrapConfidenceInterval(table, labels, Me.pOptions, metrics.Kappa, progressBar, Me.pBootstrapSeedUsed)
            Else
                ci = ComputeAnalyticalConfidenceInterval(table, weights, metrics.Kappa, Me.pOptions)
            End If

            Dim seForTest As Double = ci.StdErr
            If Double.IsNaN(seForTest) OrElse seForTest <= 0.0 Then
                seForTest = ComputeAnalyticalStandardError(table, weights, metrics.Kappa)
            End If
            Dim ht = ComputeHypothesisTest(metrics.Kappa, seForTest)

            If Me.pConstructedFromTable Then
                Me.pDroppedPairCount = 0
            End If
            If Me.pOptions.CiMethod = AgreementCiMethod.Jackknife Then
                Me.pComputationNotes.Add("Jackknife confidence intervals are not yet implemented separately; the current version uses the analytical delta-method interval.")
            End If
            If Me.pOptions.CiMethod = AgreementCiMethod.BootstrapBCa Then
                Me.pComputationNotes.Add("BCa bootstrap is not yet implemented separately; the current version uses percentile bootstrap limits.")
            End If
            If Me.pConstructedFromTable AndAlso (Me.pOptions.CiMethod = AgreementCiMethod.BootstrapPercentile OrElse Me.pOptions.CiMethod = AgreementCiMethod.BootstrapBCa) Then
                Me.pComputationNotes.Add("Bootstrap interval from a contingency table is available only because the table could be expanded into item-level pairs.")
            End If
            If Me.pUsedBootstrapCi Then
                Me.pComputationNotes.Add($"Bootstrap seed = {Me.pBootstrapSeedUsed}.")
            End If

            Me.pResult = New KappaResult With {
                .KappaCI = ci,
                .ObservedAgreement = metrics.UnweightedObservedAgreement,
                .ExpectedAgreement = metrics.UnweightedExpectedAgreement,
                .WeightedObservedAgreement = metrics.WeightedObservedAgreement,
                .WeightedExpectedAgreement = metrics.WeightedExpectedAgreement,
                .HypothesisTest = ht,
                .Categories = labels,
                .ConfusionMatrix = table,
                .WeightMatrix = weights
            }

            If progressBar IsNot Nothing Then
                progressBar.Invoke(Sub() progressBar.Value = 100)
            End If

            Me.pIsFitted = True
            Return Me.pResult
        End Function

        ''' <summary>
        ''' Creates formatted result tables suitable for worksheet or report output.
        ''' </summary>
        ''' <returns>
        ''' A list of <see cref="ResultTable"/> objects summarizing the weighted-kappa analysis.
        ''' </returns>
        ''' <remarks>
        ''' <para>
        ''' If the object has not been fitted yet, <see cref="Fit"/> is called automatically.
        ''' </para>
        ''' </remarks>
        Public Function wrapResults() As List(Of ResultTable)
            If Not Me.pIsFitted OrElse Me.pResult Is Nothing Then Me.Fit()

            Dim out As New List(Of ResultTable), categoryHeaders As String()

            Dim t As New ResultTable
            t.AddTitle("Weighted Kappa Agreement")
            Dim summaryRows = {{"Source 1", Me.pVarR1},
                                {"Source 2", Me.pVarR2},
                                {"Number of categories", Me.pResult.Categories.Length},
                                {"Number of observations", Me.pSampleSize},
                                {"Dropped missing pairs", Me.pDroppedPairCount},
                                {"Weighting scheme", Me.pOptions.Weighting.ToString()}}
            t.SetBody(summaryRows)
            out.Add(t)

            t = New ResultTable
            t.AddTitle("Kappa Summary")
            t.AddHeaderLeftRow({"Kappa", "Observed agreement", "Expected agreement", "Weighted observed agreement", "Weighted expected agreement"})
            t.AddHeaderTopRow({"Estimate", Me.pResult.KappaCI.CIlabel, "Meaning"})
            t.SetBody(New Object(,) {
                {Me.pResult.KappaCI.Estimate, Me.pResult.KappaCI.strConfidenceInterval(CIformat.LL_to_UL), "Chance-corrected agreement under the selected weighting scheme."},
                {Me.pResult.ObservedAgreement, "", "Unweighted observed agreement proportion."},
                {Me.pResult.ExpectedAgreement, "", "Unweighted expected agreement under independence."},
                {Me.pResult.WeightedObservedAgreement, "", "Observed weighted agreement proportion."},
                {Me.pResult.WeightedExpectedAgreement, "", "Expected weighted agreement under independence."}
            })
            out.Add(t)

            t = New ResultTable
            t.AddTitle("Hypothesis Test")
            t.AddHeaderTopRow({"z statistic", "Two-sided p-value", "Meaning"})
            t.SetBody({
                {Me.pResult.HypothesisTest.TestStatistics1, Me.pResult.HypothesisTest.Pvalue, "Approximate test of H0: kappa = 0."}
            })
            out.Add(t)

            t = New ResultTable
            t.AddTitle("Confusion Matrix")
            categoryHeaders = UIprocedures.ConvertCategoriesToStrings(Me.pResult.Categories)
            t.AddHeaderLeftRow(categoryHeaders)
            t.AddHeaderTopRow(categoryHeaders)
            t.SetBody(Me.pResult.ConfusionMatrix)
            out.Add(t)

            If Me.pResult.WeightMatrix.GetLength(0) <= 10 Then
                t = New ResultTable
                t.AddTitle("Weight Matrix")
                t.AddHeaderLeftRow(categoryHeaders)
                t.AddHeaderTopRow(categoryHeaders)
                t.SetBody(Me.pResult.WeightMatrix)
                out.Add(t)
            End If

            If Me.pComputationNotes.Count > 0 Then
                t = New ResultTable
                t.AddTitle("Notes")
                Dim body(Me.pComputationNotes.Count - 1, 0) As Object
                For i As Integer = 0 To Me.pComputationNotes.Count - 1
                    body(i, 0) = Me.pComputationNotes(i)
                Next
                t.SetBody(body)
                out.Add(t)
            End If

            Return out
        End Function

        Private Sub ValidateOptions(opts As KappaOptions)
            If opts Is Nothing Then AppGlobals.BSerr.LogAndThrow(New ArgumentNullException(NameOf(opts)))
            If Double.IsNaN(opts.Alpha) OrElse opts.Alpha <= 0.0 OrElse opts.Alpha >= 1.0 Then
                AppGlobals.BSerr.LogAndThrow(New ArgumentException("Alpha must be strictly between 0 and 1."))
            End If
            If opts.BootstrapReplicates < 200 Then
                opts.BootstrapReplicates = Math.Max(200, opts.BootstrapReplicates)
            End If
            If opts.Weighting = KappaWeightingScheme.Custom Then
                If opts.CustomWeights Is Nothing Then
                    AppGlobals.BSerr.LogAndThrow(New ArgumentException("Custom weights were requested, but KappaOptions.CustomWeights is Nothing."))
                End If
                If opts.CustomWeights.GetLength(0) <> opts.CustomWeights.GetLength(1) Then
                    AppGlobals.BSerr.LogAndThrow(New ArgumentException("The custom weight matrix must be square."))
                End If
            End If
        End Sub

        Private Function BuildConfusionMatrixFromPairs(r1 As Object(), r2 As Object(), explicitCategories As Object()) As (Table As Double(,), Labels As Object(), DroppedCount As Integer)
            Dim pairs As New List(Of Tuple(Of Object, Object))
            Dim dropped As Integer = 0

            For i As Integer = 0 To r1.Length - 1
                If IsMissingCategoryValue(r1(i)) OrElse IsMissingCategoryValue(r2(i)) Then
                    dropped += 1
                Else
                    pairs.Add(Tuple.Create(NormalizeCategoryValue(r1(i)), NormalizeCategoryValue(r2(i))))
                End If
            Next

            If pairs.Count < 2 Then
                AppGlobals.BSerr.LogAndThrow(New InvalidOperationException("Fewer than two complete rating pairs remain after filtering missing observations."))
            End If

            Dim labels As Object()
            Dim keyToIndex As Dictionary(Of String, Integer)
            If explicitCategories IsNot Nothing AndAlso explicitCategories.Length > 0 Then
                labels = DirectCast(explicitCategories.Clone(), Object())
                keyToIndex = BuildCategoryIndex(labels)
            Else
                Dim encountered As New List(Of Object)
                Dim seen As New HashSet(Of String)(StringComparer.Ordinal)
                For Each pr As Tuple(Of Object, Object) In pairs
                    Dim k1 = CategoryKey(pr.Item1)
                    If Not seen.Contains(k1) Then
                        encountered.Add(pr.Item1)
                        seen.Add(k1)
                    End If
                    Dim k2 = CategoryKey(pr.Item2)
                    If Not seen.Contains(k2) Then
                        encountered.Add(pr.Item2)
                        seen.Add(k2)
                    End If
                Next
                labels = encountered.ToArray()
                keyToIndex = BuildCategoryIndex(labels)
            End If

            If labels.Length < 2 Then
                AppGlobals.BSerr.LogAndThrow(New InvalidOperationException("At least two categories are required for kappa analysis."))
            End If

            Dim table(labels.Length - 1, labels.Length - 1) As Double
            For Each pr As Tuple(Of Object, Object) In pairs
                Dim k1 = CategoryKey(pr.Item1)
                Dim k2 = CategoryKey(pr.Item2)
                If Not keyToIndex.ContainsKey(k1) OrElse Not keyToIndex.ContainsKey(k2) Then
                    AppGlobals.BSerr.LogAndThrow(New ArgumentException("Observed categories are not fully covered by the category order supplied in KappaOptions.Categories."))
                End If
                table(keyToIndex(k1), keyToIndex(k2)) += 1.0
            Next

            Return (table, labels, dropped)
        End Function

        Private Function BuildCategoryIndex(labels As Object()) As Dictionary(Of String, Integer)
            Dim dict As New Dictionary(Of String, Integer)(StringComparer.Ordinal)
            For i As Integer = 0 To labels.Length - 1
                If IsMissingCategoryValue(labels(i)) Then
                    AppGlobals.BSerr.LogAndThrow(New ArgumentException("Category labels must not contain missing values."))
                End If
                Dim normalized = NormalizeCategoryValue(labels(i))
                Dim key = CategoryKey(normalized)
                If dict.ContainsKey(key) Then
                    AppGlobals.BSerr.LogAndThrow(New ArgumentException("Category labels must be unique after normalization to string keys."))
                End If
                labels(i) = normalized
                dict.Add(key, i)
            Next
            Return dict
        End Function

        Private Function IsMissingCategoryValue(value As Object) As Boolean
            If value Is Nothing OrElse Convert.IsDBNull(value) Then Return True
            If TypeOf value Is String Then Return String.IsNullOrWhiteSpace(CStr(value))
            If TypeOf value Is Double Then Return Double.IsNaN(CDbl(value)) OrElse Double.IsInfinity(CDbl(value))
            If TypeOf value Is Single Then Return Single.IsNaN(CSng(value)) OrElse Single.IsInfinity(CSng(value))
            Return False
        End Function

        Private Function NormalizeCategoryValue(value As Object) As Object
            If TypeOf value Is String Then Return CStr(value).Trim()
            Return value
        End Function

        Private Function CategoryKey(value As Object) As String
            If TypeOf value Is Double Then Return CDbl(value).ToString("R", Globalization.CultureInfo.InvariantCulture)
            If TypeOf value Is Single Then Return CSng(value).ToString("R", Globalization.CultureInfo.InvariantCulture)
            If TypeOf value Is IFormattable Then
                Return DirectCast(value, IFormattable).ToString(Nothing, Globalization.CultureInfo.InvariantCulture)
            End If
            Return Convert.ToString(value, Globalization.CultureInfo.InvariantCulture)
        End Function

        Private Function BuildWeightMatrix(k As Integer, opts As KappaOptions) As Double(,)
            Dim w(k - 1, k - 1) As Double
            If k = 1 Then
                w(0, 0) = 1.0
                Return w
            End If

            Select Case opts.Weighting
                Case KappaWeightingScheme.Unweighted
                    For i As Integer = 0 To k - 1
                        For j As Integer = 0 To k - 1
                            w(i, j) = If(i = j, 1.0, 0.0)
                        Next
                    Next

                Case KappaWeightingScheme.Linear, KappaWeightingScheme.CicchettiAllison
                    For i As Integer = 0 To k - 1
                        For j As Integer = 0 To k - 1
                            w(i, j) = 1.0 - (Math.Abs(i - j) / CDbl(k - 1))
                        Next
                    Next

                Case KappaWeightingScheme.Quadratic, KappaWeightingScheme.FleissCohen
                    For i As Integer = 0 To k - 1
                        For j As Integer = 0 To k - 1
                            Dim d As Double = Math.Abs(i - j) / CDbl(k - 1)
                            w(i, j) = 1.0 - d * d
                        Next
                    Next

                Case KappaWeightingScheme.Custom
                    If opts.CustomWeights.GetLength(0) <> k OrElse opts.CustomWeights.GetLength(1) <> k Then
                        AppGlobals.BSerr.LogAndThrow(New ArgumentException("The custom weight matrix dimension must match the number of categories."))
                    End If
                    w = DirectCast(opts.CustomWeights.Clone(), Double(,))

                Case Else
                    AppGlobals.BSerr.LogAndThrow(New NotSupportedException("Unsupported weighting scheme."))
            End Select

            For i As Integer = 0 To k - 1
                For j As Integer = 0 To k - 1
                    If Double.IsNaN(w(i, j)) OrElse Double.IsInfinity(w(i, j)) Then
                        AppGlobals.BSerr.LogAndThrow(New ArgumentException("The weight matrix must contain only finite values."))
                    End If
                Next
            Next
            Return w
        End Function

        Private Function ComputeKappaMetrics(table As Double(,), weights As Double(,)) As (Kappa As Double, WeightedObservedAgreement As Double, WeightedExpectedAgreement As Double, UnweightedObservedAgreement As Double, UnweightedExpectedAgreement As Double)
            Dim k As Integer = table.GetLength(0)
            Dim n As Double = SumMatrix(table)
            Dim probs = MatrixToProbabilities(table, n)
            Dim row = RowMarginals(probs)
            Dim col = ColumnMarginals(probs)

            Dim poW As Double = 0.0
            Dim peW As Double = 0.0
            Dim po As Double = 0.0
            Dim pe As Double = 0.0
            For i As Integer = 0 To k - 1
                po += probs(i, i)
                pe += row(i) * col(i)
                For j As Integer = 0 To k - 1
                    poW += weights(i, j) * probs(i, j)
                    peW += weights(i, j) * row(i) * col(j)
                Next
            Next

            If Math.Abs(1.0 - peW) < 0.000000000001 Then
                AppGlobals.BSerr.LogAndThrow(New InvalidOperationException("Weighted expected agreement is equal to 1, so kappa is not estimable."))
            End If

            Dim kappa As Double = (poW - peW) / (1.0 - peW)
            Return (kappa, poW, peW, po, pe)
        End Function

        Private Function ComputeAnalyticalConfidenceInterval(table As Double(,), weights As Double(,), kappa As Double, opts As KappaOptions) As ConfidenceIntervalResult
            Dim se As Double = ComputeAnalyticalStandardError(table, weights, kappa)
            Dim z As Double = distributions.ZCritTwoSided(opts.Alpha)
            Dim ci As New ConfidenceIntervalResult With {
                .alpha = opts.Alpha,
                .Estimate = kappa,
                .StdErr = se,
                .LowerLimit = Math.Max(-1.0, kappa - z * se),
                .UpperLimit = Math.Min(1.0, kappa + z * se)
            }
            Return ci
        End Function

        Private Function ComputeAnalyticalStandardError(table As Double(,), weights As Double(,), observedKappa As Double) As Double
            Dim n As Double = SumMatrix(table)
            If n <= 1.0 Then Return Double.NaN

            Dim probs = MatrixToProbabilities(table, n)
            Dim pVec = FlattenMatrix(probs)
            Dim grad = NumericalGradientForKappa(pVec, table.GetLength(0), weights)

            Dim var As Double = 0.0
            For i As Integer = 0 To pVec.Length - 1
                For j As Integer = 0 To pVec.Length - 1
                    Dim cov As Double = If(i = j, pVec(i), 0.0) - pVec(i) * pVec(j)
                    var += grad(i) * cov * grad(j)
                Next
            Next
            var /= n
            If var < 0.0 AndAlso var > -0.000000000001 Then var = 0.0
            If var < 0.0 Then Return Double.NaN
            Return Math.Sqrt(var)
        End Function

        Private Function NumericalGradientForKappa(pVec As Double(), k As Integer, weights As Double(,)) As Double()
            Dim g(pVec.Length - 1) As Double
            Const h As Double = 0.0000001

            For idx As Integer = 0 To pVec.Length - 1
                Dim pPlus = DirectCast(pVec.Clone(), Double())
                Dim pMinus = DirectCast(pVec.Clone(), Double())
                pPlus(idx) += h
                pMinus(idx) = Math.Max(0.0, pMinus(idx) - h)
                RenormalizeProbabilities(pPlus)
                RenormalizeProbabilities(pMinus)
                Dim fPlus As Double = KappaFromProbabilityVector(pPlus, k, weights)
                Dim fMinus As Double = KappaFromProbabilityVector(pMinus, k, weights)
                Dim stepSize As Double = 2.0 * h
                If pVec(idx) < h Then stepSize = h
                g(idx) = (fPlus - fMinus) / stepSize
            Next

            Return g
        End Function

        Private Sub RenormalizeProbabilities(ByRef p As Double())
            For i As Integer = 0 To p.Length - 1
                If p(i) < 0.0 Then p(i) = 0.0
            Next
            Dim s As Double = p.Sum()
            If s <= 0.0 Then
                Dim uniform As Double = 1.0 / p.Length
                For i As Integer = 0 To p.Length - 1
                    p(i) = uniform
                Next
            Else
                For i As Integer = 0 To p.Length - 1
                    p(i) /= s
                Next
            End If
        End Sub

        Private Function KappaFromProbabilityVector(pVec As Double(), k As Integer, weights As Double(,)) As Double
            Dim probs(k - 1, k - 1) As Double
            Dim idx As Integer = 0
            For i As Integer = 0 To k - 1
                For j As Integer = 0 To k - 1
                    probs(i, j) = pVec(idx)
                    idx += 1
                Next
            Next
            Return ComputeKappaMetrics(ProbabilitiesToCounts(probs), weights).Kappa
        End Function

        Private Function ProbabilitiesToCounts(probs As Double(,)) As Double(,)
            Return probs
        End Function

        Private Function ComputeBootstrapConfidenceInterval(table As Double(,),
                                                           labels As Object(),
                                                           opts As KappaOptions,
                                                           observedKappa As Double,
                                                           Optional progressBar As System.Windows.Forms.ProgressBar = Nothing,
                                                           Optional randomSeed As Integer = Integer.MinValue) As ConfidenceIntervalResult
            Dim pairs = ExpandTableToPairs(table, labels)
            If pairs Is Nothing OrElse pairs.Item1 Is Nothing OrElse pairs.Item1.Length < 2 Then
                AppGlobals.BSerr.LogAndThrow(New InvalidOperationException("Bootstrap confidence interval requires paired item-level data or an integer-valued contingency table that can be expanded to pairs."))
            End If

            Dim n As Integer = pairs.Item1.Length
            Dim reps As Integer = opts.BootstrapReplicates
            Dim boot(reps - 1) As Double
            Dim rnd As Random = AppGlobals.CreateRandom(randomSeed)

            If progressBar IsNot Nothing Then
                progressBar.Invoke(Sub() progressBar.Value = 0)
            End If

            For b As Integer = 0 To reps - 1
                Dim s1(n - 1) As Object
                Dim s2(n - 1) As Object
                For i As Integer = 0 To n - 1
                    Dim take As Integer = rnd.Next(0, n)
                    s1(i) = pairs.Item1(take)
                    s2(i) = pairs.Item2(take)
                Next
                Dim built = BuildConfusionMatrixFromPairs(s1, s2, labels)
                Dim w = BuildWeightMatrix(built.Labels.Length, opts)
                boot(b) = ComputeKappaMetrics(built.Table, w).Kappa

                If progressBar IsNot Nothing Then
                    Dim progressValue As Integer = CInt(Math.Min(100.0, Math.Round(100.0 * (b + 1) / reps)))
                    progressBar.Invoke(Sub() progressBar.Value = progressValue)
                End If
            Next

            Array.Sort(boot)
            Dim lower As Double = QuantileFromSorted(boot, opts.Alpha / 2.0)
            Dim upper As Double = QuantileFromSorted(boot, 1.0 - opts.Alpha / 2.0)

            Dim ci As New ConfidenceIntervalResult With {
                .alpha = opts.Alpha,
                .Estimate = observedKappa,
                .LowerLimit = Math.Max(-1.0, lower),
                .UpperLimit = Math.Min(1.0, upper),
                .StdErr = SampleStandardDeviation(boot)
            }
            Return ci
        End Function

        ''' <summary>
        ''' Resolves the actual pseudo-random seed that will be used for bootstrap resampling.
        ''' </summary>
        ''' <param name="requestedSeed">
        ''' Explicit seed requested by the caller. Use <see cref="Integer.MinValue"/> to indicate that no explicit seed was supplied.
        ''' </param>
        ''' <returns>
        ''' The explicit seed if one was supplied; otherwise the global default seed; otherwise a time-based seed captured from <see cref="Environment.TickCount"/>.
        ''' </returns>
        Friend Shared Function ResolveRandomSeed(requestedSeed As Integer) As Integer
            If requestedSeed <> Integer.MinValue Then Return requestedSeed

            Dim globalSeed As Integer = AppGlobals.DefaultRandomSeed
            If globalSeed <> Integer.MinValue Then Return globalSeed

            Return Environment.TickCount
        End Function

        Private Function ExpandTableToPairs(table As Double(,), labels As Object()) As Tuple(Of Object(), Object())
            Dim n As Integer = CInt(Math.Round(SumMatrix(table)))
            If n <= 0 Then Return Nothing

            For i As Integer = 0 To table.GetLength(0) - 1
                For j As Integer = 0 To table.GetLength(1) - 1
                    Dim rounded As Double = Math.Round(table(i, j))
                    If Math.Abs(table(i, j) - rounded) > 0.000000001 Then Return Nothing
                Next
            Next

            Dim r1 As New List(Of Object)(n)
            Dim r2 As New List(Of Object)(n)
            For i As Integer = 0 To table.GetLength(0) - 1
                For j As Integer = 0 To table.GetLength(1) - 1
                    Dim count As Integer = CInt(Math.Round(table(i, j)))
                    For c As Integer = 1 To count
                        r1.Add(labels(i))
                        r2.Add(labels(j))
                    Next
                Next
            Next

            Return Tuple.Create(r1.ToArray(), r2.ToArray())
        End Function

        Private Function ComputeHypothesisTest(kappa As Double, se As Double) As TestResult
            Dim out As New TestResult
            If Double.IsNaN(se) OrElse se <= 0.0 Then
                out.TestStatistics1 = Double.NaN
                out.Pvalue = Double.NaN
                out.strSpecialInformation = "Approximate z-test could not be computed because the kappa standard error was not available."
                Return out
            End If

            Dim z As Double = kappa / se
            out.TestStatistics1 = z
            out.Pvalue = 2.0 * (1.0 - distributions.PNorm(Math.Abs(z), 0, 1))
            Return out
        End Function

        Private Function MatrixToProbabilities(table As Double(,), n As Double) As Double(,)
            Dim k As Integer = table.GetLength(0)
            Dim probs(k - 1, k - 1) As Double
            For i As Integer = 0 To k - 1
                For j As Integer = 0 To k - 1
                    probs(i, j) = table(i, j) / n
                Next
            Next
            Return probs
        End Function

        Private Function RowMarginals(probs As Double(,)) As Double()
            Dim k As Integer = probs.GetLength(0)
            Dim out(k - 1) As Double
            For i As Integer = 0 To k - 1
                For j As Integer = 0 To k - 1
                    out(i) += probs(i, j)
                Next
            Next
            Return out
        End Function

        Private Function ColumnMarginals(probs As Double(,)) As Double()
            Dim k As Integer = probs.GetLength(0)
            Dim out(k - 1) As Double
            For j As Integer = 0 To k - 1
                For i As Integer = 0 To k - 1
                    out(j) += probs(i, j)
                Next
            Next
            Return out
        End Function

        Private Function FlattenMatrix(m As Double(,)) As Double()
            Dim out(m.Length - 1) As Double
            Dim idx As Integer = 0
            For i As Integer = 0 To m.GetLength(0) - 1
                For j As Integer = 0 To m.GetLength(1) - 1
                    out(idx) = m(i, j)
                    idx += 1
                Next
            Next
            Return out
        End Function

        Private Function SumMatrix(m As Double(,)) As Double
            Dim s As Double = 0.0
            For i As Integer = 0 To m.GetLength(0) - 1
                For j As Integer = 0 To m.GetLength(1) - 1
                    s += m(i, j)
                Next
            Next
            Return s
        End Function

        Private Function QuantileFromSorted(sortedValues As Double(), p As Double) As Double
            If sortedValues Is Nothing OrElse sortedValues.Length = 0 Then Return Double.NaN
            If p <= 0.0 Then Return sortedValues(0)
            If p >= 1.0 Then Return sortedValues(sortedValues.Length - 1)
            Dim pos As Double = (sortedValues.Length - 1) * p
            Dim lo As Integer = CInt(Math.Floor(pos))
            Dim hi As Integer = CInt(Math.Ceiling(pos))
            If lo = hi Then Return sortedValues(lo)
            Dim frac As Double = pos - lo
            Return sortedValues(lo) + frac * (sortedValues(hi) - sortedValues(lo))
        End Function

        Private Function SampleStandardDeviation(values As Double()) As Double
            If values Is Nothing OrElse values.Length < 2 Then Return Double.NaN
            Dim m As Double = values.Average()
            Dim ss As Double = 0.0
            For Each v As Double In values
                ss += (v - m) * (v - m)
            Next
            Return Math.Sqrt(ss / (values.Length - 1))
        End Function

    End Class

End Namespace
