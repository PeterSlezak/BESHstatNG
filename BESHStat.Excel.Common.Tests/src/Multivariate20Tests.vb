Option Explicit On
Option Strict On
Option Infer On
Imports System
Imports System.Collections.Generic
Imports System.Globalization
Imports System.Linq
Imports System.Text
Imports System.Xml.Linq
Imports BESHStatNG
Imports BESHStatNG.ExcelCommon
Imports BESHStatNG.Multivariate
Imports Microsoft.VisualStudio.TestTools.UnitTesting

Public NotInheritable Class Multivariate20Fixture
    Public Shared Function FactorData(Optional edge As Integer = -1) As Double(,)
        Dim x(35, 5) As Double
        For i = 0 To 35
            Dim f1 = (i - 17.5) / 6.0 + 0.15 * Math.Cos(i * 0.3)
            Dim f2 = 0.35 * f1 + 0.9 * (Math.Sin((i + 1) * 0.55) + ((i Mod 5) - 2) / 3.0)
            x(i, 0) = 0.88 * f1 + 0.1 * f2 + (((i * 5) Mod 7) - 3) / 40.0
            x(i, 1) = 0.78 * f1 - 0.12 * f2 + (((i * 3) Mod 11) - 5) / 45.0
            x(i, 2) = 0.18 * f1 + 0.86 * f2 + (((i * 4) Mod 9) - 4) / 42.0
            x(i, 3) = -0.08 * f1 + 0.76 * f2 + (((i * 6) Mod 10) - 5) / 38.0
            x(i, 4) = 0.58 * f1 + 0.46 * f2 + (((i * 2) Mod 8) - 4) / 44.0
            x(i, 5) = 0.52 * f1 - 0.4 * f2 + (((i * 7) Mod 12) - 6) / 48.0
        Next
        Select Case edge
            Case 0 To 2 : x(1, 0) = {Double.NaN, Double.PositiveInfinity, Double.NegativeInfinity}(edge)
            Case 3 : For i = 0 To 35 : x(i, 0) = Double.NaN : Next
            Case 4 : Return New Double(,) {{1, 2}, {3, 4}}
            Case 5 : Return New Double(,) {{1, 2}, {3, 4}, {5, 6}}
            Case 6 : For i = 0 To 35 : x(i, 1) = 1.0 : Next
            Case 7 : For i = 0 To 35 : x(i, 1) = 1.0 + i * 0.00000000000001 : Next
            Case 8 : For i = 0 To 35 : x(i, 1) = x(i, 0) : Next
            Case 9 : Return New Double(-1, 5) {}
            Case 10 : Return New Double(3, -1) {}
            Case 11 : Return New Double(,) {{1}, {2}, {3}, {4}}
        End Select
        Return x
    End Function
    Public Shared Function FactorOptions(id As Integer) As MultivariateFactorOptions
        Dim o As New MultivariateFactorOptions With {.Extraction = CType(id Mod 6, FactorAnalysisExtractionMethod), .Rotation = CType((id \ 6) Mod 5, FactorAnalysisRotationMethod),
            .Scores = CType((id \ 30) Mod 3, FactorAnalysisScoreMethod), .MatrixType = CType((id \ 90) Mod 2, FactorAnalysisMatrixType),
            .RetentionValue = 2, .MaximumIterations = 120, .Epsilon = 0.0000001, .KaiserNormalization = id Mod 2 = 0,
            .Communality = CType((id \ 3) Mod 2, FactorAnalysisCommunalityInitialization), .Missing = FactorAnalysisMissingValuePolicy.ListwiseDeletion}
        If id >= 180 Then
            Select Case id
                Case 180 To 185 : o.RetentionValue = {0.0, 1.0, 3.0, 6.0, 20.0, -1.0}(id - 180)
                Case 186 To 191 : o.Retention = FactorAnalysisRetentionMethod.Eigenvalue : o.RetentionValue = {0.0, 1.0, 1.000000001, 100.0, Double.NaN, Double.PositiveInfinity}(id - 186)
                Case 192 To 197 : o.Retention = FactorAnalysisRetentionMethod.Variance : o.RetentionValue = {70.0, 100.0, 0.0, 101.0, Double.NaN, Double.NegativeInfinity}(id - 192)
                Case 198 : o.MaximumIterations = 1 : o.Epsilon = 0.00000000000001 : o.Extraction = FactorAnalysisExtractionMethod.PrincipalAxis
                Case 199 : o.MaximumIterations = 0
                Case 200 To 203 : o.Epsilon = {0.0, -1.0, Double.NaN, Double.PositiveInfinity}(id - 200)
                Case 204 To 207 : o.PromaxPower = {1.0, 1.000000001, Double.NaN, Double.PositiveInfinity}(id - 204) : o.Rotation = FactorAnalysisRotationMethod.Promax
                Case 208 To 219
                Case 220 To 222 : o.Missing = FactorAnalysisMissingValuePolicy.ErrorOnMissing
                Case 223 : o.Rotation = CType(99, FactorAnalysisRotationMethod)
                Case 224 : o.Extraction = CType(99, FactorAnalysisExtractionMethod)
                Case 225 : o.RetentionValue = Double.NaN
            End Select
        End If
        Return o
    End Function
    Public Shared Function DiscriminantData(classCount As Integer, Optional edge As Integer = -1) As (x As Double(,), groups As Object())
        Dim sizes = New Integer() {9, 7, 5, 3}, n = sizes.Take(classCount).Sum(), x(n - 1, 2) As Double, groups(n - 1) As Object
        Dim labels = New Object() {"a", " A ", "Ω", "2"}, row = 0
        For g = 0 To classCount - 1
            For i = 0 To sizes(g) - 1
                groups(row) = If(edge = 12, CType(g + 0.5, Object), labels(g))
                For j = 0 To 2 : x(row, j) = g * 0.45 + Math.Sin((i + 1) * (j + 1) * 0.7) + i * (j + 1) * 0.04 : Next
                row += 1
            Next
        Next
        Select Case edge
            Case 0 To 2 : x(If(edge = 0, 0, If(edge = 1, n \ 2, n - 1)), 0) = {Double.NaN, Double.PositiveInfinity, Double.NegativeInfinity}(edge)
            Case 3 : groups(2) = Nothing
            Case 4 : groups(2) = " "
            Case 5 : For i = 0 To n - 1 : x(i, 0) = Double.NaN : Next
            Case 6 : For i = 0 To n - 1 : groups(i) = "only" : Next
            Case 7 : For i = 0 To n - 1 : x(i, 1) = 1 : Next
            Case 8 : For i = 0 To n - 1 : x(i, 1) = x(i, 0) : Next
            Case 9 : For i = 1 To n - 1 : groups(i) = "A" : Next
            Case 10 : Return (New Double(-1, 2) {}, New Object() {})
            Case 11 : Return (New Double(2, -1) {}, New Object() {"a", "A", "A"})
        End Select
        Return (x, groups)
    End Function
    Public Shared Function DiscriminantOptions(id As Integer) As MultivariateDiscriminantOptions
        Dim o As New MultivariateDiscriminantOptions With {.Method = CType(id Mod 2, DiscriminantAnalysisMethod), .Standardization = CType((id \ 2) Mod 3, ClusterStandardizationMode),
            .Priors = CType((id \ 6) Mod 3, DiscriminantPriorMode), .Validation = CType((id \ 18) Mod 4, DiscriminantValidationMode), .Stratified = (id \ 72) Mod 2 = 0,
            .Folds = {2, 3, 5}(id Mod 3), .Seed = {0, 1, 1729, -4}((id \ 2) Mod 4), .Missing = ClusterMissingValuePolicy.ListwiseDeletion}
        If id >= 144 Then
            o.Validation = DiscriminantValidationMode.None
            Select Case id
                Case 144 To 156
                Case 157 To 161 : o.Regularization = {0.0, 0.001, -1.0, Double.NaN, Double.PositiveInfinity}(id - 157)
                Case 162 To 165 : o.Validation = DiscriminantValidationMode.KFold : o.Folds = {-1, 1, 50, 100}(id - 162)
                Case 166 To 171 : o.Validation = DiscriminantValidationMode.Holdout : o.HoldoutFraction = {0.0, 0.01, 0.99, 1.0, -0.2, Double.NaN}(id - 166)
                Case 172 To 179 : o.Priors = DiscriminantPriorMode.UserSpecified
                Case 180 To 184 : o.Missing = ClusterMissingValuePolicy.ErrorOnMissing
                Case 185 : o.Validation = DiscriminantValidationMode.LeaveOneOut
                Case 186 : o.Validation = DiscriminantValidationMode.KFold : o.Folds = 5
                Case 187 : o.Validation = DiscriminantValidationMode.Holdout : o.HoldoutFraction = 0.8
            End Select
        End If
        Return o
    End Function
    Public Shared Function Input(kind As MultivariateAnalysisKind, x(,) As Double, Optional labelled As Boolean = False) As MultivariateInputIntent
        Dim n = x.GetLength(0), p = x.GetLength(1) + If(kind = MultivariateAnalysisKind.Discriminant, 1, 0)
        Dim rows = Enumerable.Range(0, n).Select(Function(i) If(i < 4, New Integer() {101, 105, 118, 131}(i), 140 + i * 7)).Reverse().ToArray()
        Return New MultivariateInputIntent(kind, n, p, Enumerable.Range(0, p).Select(Function(i) If(i = 0 AndAlso kind = MultivariateAnalysisKind.Discriminant, "group", If(i Mod 2 = 0, "μ & x", " duplicate "))).ToArray(), rows,
            If(labelled, rows.Select(Function(v, i) MultivariateWorkflowPolicy.ObservationLabel(If(i Mod 3 = 0, " ", "same Ω"), v)).ToArray(), Nothing),
            sourceColumnKeys:=Enumerable.Range(0, p).Select(Function(i) "selected-" & (p - i)).ToArray())
    End Function
    Public Shared Function Raw(x(,) As Double, Optional groups() As Object = Nothing) As Object(,)
        Dim extra = If(groups Is Nothing, 0, 1), output(x.GetLength(0) - 1, x.GetLength(1) + extra - 1) As Object
        For i = 0 To x.GetLength(0) - 1
            If groups IsNot Nothing Then output(i, 0) = groups(i)
            For j = 0 To x.GetLength(1) - 1 : output(i, j + extra) = x(i, j) : Next
        Next
        Return output
    End Function
    Public Shared Function FactorDigest(m As FactorAnalysis) As String
        Return Multivariate18Fixture.Digest(m.InitialEigenvalues, m.InitialEigenvectors, m.AnalysisRowIds, m.InitialCommunalities, m.Communalities, m.Uniquenesses, m.UnrotatedLoadings, m.PatternMatrix, m.StructureMatrix,
            m.FactorCorrelationMatrix, m.RotationTransform, m.ScoreCoefficientMatrix, m.Scores, m.WorkingMatrix, m.CorrelationMatrix, m.AntiImageCorrelationMatrix, m.CommonVarianceMatrix, m.ReproducedMatrix, m.ResidualMatrix,
            New Object() {m.NumberOfFactors, m.ExtractionConverged, m.ExtractionIterations, m.RotationConverged, m.RotationIterations})
    End Function
    Public Shared Function PredictionDigest(r As DiscriminantPredictionResult) As String
        If r Is Nothing Then Return "UNAVAILABLE"
        Return Multivariate18Fixture.Digest(r.RowIndices, r.RowLabels, r.ActualGroupLabels, r.PredictedGroupLabels, r.FoldAssignments, r.AssignedPosteriorProbability, r.PosteriorProbabilities, r.RawScores, r.SquaredDistances,
            r.CorrectClassification, r.Confusion.Counts, r.Confusion.RowTotals, r.Confusion.ColumnTotals, r.Confusion.RecallPct, r.Confusion.PrecisionPct, New Double() {r.Confusion.OverallAccuracy, r.Confusion.OverallAccuracyPct})
    End Function
    Public Shared Function DiscriminantDigest(m As DiscriminantAnalysis) As String
        If m.PreparedData Is Nothing Then Return "UNAVAILABLE"
        Dim p = m.PreparedData
        Return Multivariate18Fixture.Digest(p.WorkingData, p.ActiveOriginalData, p.ActiveOriginalIndices, p.RemovedOriginalIndices, p.ActiveGroupLabels, p.ActiveRowLabels, p.RemovedRowLabels, p.VariableNames,
            p.ColumnLocations, p.ColumnScales, m.GroupLabels, m.CanonicalEigenvalues,
            m.GroupStatistics.Select(Function(g) g.PriorProbability).ToArray(), m.GroupStatistics.Select(Function(g) g.Count).ToArray(),
            New String() {PredictionDigest(m.TrainingClassification), PredictionDigest(m.ValidationClassification)},
            m.GroupStatistics.SelectMany(Function(g) g.CovarianceWorking.Cast(Of Double)()).ToArray())
    End Function
    Private Shared Function OptionsXml(options As Object) As XElement
        Return New XElement("options", options.GetType().GetProperties().Select(Function(p) New XElement(p.Name, Convert.ToString(p.GetValue(options, Nothing), CultureInfo.InvariantCulture))))
    End Function
    Private Shared Function SectionsXml(report As MultivariateModelReport) As XElement
        Return New XElement("sections", report.Sections.Select(Function(s) New XElement("section", New XAttribute("index", s.Index), New XAttribute("rows", s.Shape.Rows), New XAttribute("columns", s.Shape.Columns),
            New XAttribute("headers", s.HeaderDepth), New XAttribute("left_headers", s.LeftHeaderDepth), New XAttribute("footer_rows", s.FooterDepth), s.Title)))
    End Function
    Private Shared Sub Emit(root As XElement)
        Console.WriteLine("04A20_XML|" & Convert.ToBase64String(Encoding.UTF8.GetBytes(root.ToString(SaveOptions.DisableFormatting))))
    End Sub
    Private Shared Function ProfileRoot(family As String, id As Integer, input As MultivariateInputIntent, x(,) As Double, options As Object, classification As String, phase As String, failure As Exception, Optional groups() As Object = Nothing) As XElement
        Dim root As New XElement("profile", New XAttribute("family", family), New XAttribute("fixture", id), New XAttribute("classification", classification), New XAttribute("status", "NOT_GOLDEN"),
            New XAttribute("culture", CultureInfo.CurrentCulture.Name), New XAttribute("runtime", Environment.Version.ToString()), New XAttribute("phase", phase),
            New XAttribute("fixture_sha256", Multivariate18Fixture.Digest(x, groups, input.SourceRowIds, input.VariableNames, input.RowLabels)), OptionsXml(options),
            New XElement("source_rows", input.SourceRowIds.Select(Function(v) New XElement("row", v))), New XElement("names", input.VariableNames.Select(Function(v) New XElement("name", v))),
            New XElement("input_shape", New XAttribute("rows", x.GetLength(0)), New XAttribute("columns", x.GetLength(1))))
        If failure IsNot Nothing Then root.Add(New XElement("exception", New XAttribute("type", failure.GetType().FullName), failure.Message))
        Return root
    End Function
    Public Shared Function FactorProfile(id As Integer, Optional capture As Func(Of MultivariateInputIntent, MultivariateFactorRequest) = Nothing) As MultivariateFactorProjection
        Dim edge = If(id >= 208 AndAlso id <= 219, id - 208, If(id >= 220 AndAlso id <= 222, id - 220, -1))
        Dim x = FactorData(edge), input = Multivariate20Fixture.Input(MultivariateAnalysisKind.Factor, x), o = FactorOptions(id)
        Dim request = If(capture Is Nothing, New MultivariateFactorRequest(input, o), capture(input)), a As New FactorAnalysis, b As New FactorAnalysis
        Dim expected = Multivariate19Fixture.Failure(Sub()
            a.dataInputs(x, input.SourceRowIds, input.VariableNames)
            a.settingsInputs(o.MaximumIterations, o.Epsilon, o.MatrixType, o.Extraction, o.Retention, o.RetentionValue, o.Rotation, o.Scores, o.Communality, o.Missing, o.KaiserNormalization, o.PromaxPower)
            a.Calculate()
        End Sub)
        Dim actual = Multivariate19Fixture.Failure(Sub()
            b.dataInputs(x, input.SourceRowIds, input.VariableNames) : request.Configure(b) : b.Calculate()
        End Sub)
        Multivariate19Fixture.SameFailure(expected, actual)
        If id < 180 Then Assert.IsNull(actual, "Ordinary factorial matrix must complete; nonconvergence is observed separately.")
        Dim root = ProfileRoot("FA", id, input, x, o, If(actual IsNot Nothing, "EXISTING_CORE_REJECTED", If(Not b.ExtractionConverged OrElse Not b.RotationConverged, "CORE_NONCONVERGED", "CORE_RETURNED")), "Calculate", actual)
        If actual IsNot Nothing Then
            root.Add(New XElement("calculate_completed", False))
            If b.AnalysisRowIds IsNot Nothing Then root.Add(New XElement("prepared_analysis_row_ids", b.AnalysisRowIds.Select(Function(v) New XElement("row", v))), New XElement("removed_count", b.RemovedRowCount))
            Emit(root) : Return Nothing
        End If
        Assert.AreEqual(FactorDigest(a), FactorDigest(b))
        Dim projection = MultivariateFactorProjectionPolicy.Project(request, b), tables = b.wrapResults(), report = projection.ProjectReport(tables)
        Multivariate17Fixture.Tables(a.wrapResults(), report.LegacyTables())
        Multivariate17Fixture.Tables(tables, projection.ProjectReport(tables).LegacyTables())
        Multivariate17Fixture.Cells(MultivariateWorkflowPolicy.ClusterInputTable(Raw(x), input.VariableNames, input.SourceRowIds, Nothing), request.DataTable(Raw(x)))
        Assert.AreEqual(b.NumberOfFactors, projection.ActualFactorCount)
        CollectionAssert.AreEqual(b.AnalysisRowIds, projection.ActiveObservations.Select(Function(r) r.CoreRowId).ToArray())
        Assert.AreEqual(b.Scores IsNot Nothing, projection.Arrays.Single(Function(v) v.Role = "Scores").Shape.Available)
        Assert.AreEqual(b.PatternMatrix.GetLength(0), projection.Variables.Count)
        CollectionAssert.AreEqual(b.FactorNames(), projection.Factors.Select(Function(v) v.Name).ToArray())
        CollectionAssert.AreEqual(New MultivariatePlotKind() {MultivariatePlotKind.Scree, MultivariatePlotKind.Loadings2D, MultivariatePlotKind.Loadings3D}, projection.Plots.Select(Function(p) p.Request.Kind).ToArray())
        Assert.AreEqual(FactorDigest(a), FactorDigest(b), "Projection must not mutate Core numerical arrays.")
        root.Add(New XElement("numerical_sha256", FactorDigest(b)), New XElement("extraction_converged", b.ExtractionConverged), New XElement("rotation_converged", b.RotationConverged),
            New XElement("extraction_iterations", b.ExtractionIterations), New XElement("rotation_iterations", b.RotationIterations), New XElement("actual_factors", b.NumberOfFactors), New XElement("removed_count", b.RemovedRowCount),
            New XElement("observations", projection.ActiveObservations.Select(Function(v) New XElement("row", New XAttribute("active", v.ActiveOrdinal), New XAttribute("core_id", v.CoreRowId), New XElement("input_ordinal", v.InputOrdinal), New XElement("source_row", v.SourceRowId), New XAttribute("state", v.State)))),
            New XElement("arrays", projection.Arrays.Select(Function(v) New XElement("array", New XAttribute("role", v.Role), New XAttribute("rows", v.Shape.Rows), New XAttribute("columns", v.Shape.Columns), New XAttribute("available", v.Shape.Available), New XAttribute("row_axis", v.RowAxis), New XAttribute("column_axis", v.ColumnAxis)))),
            New XElement("factors", projection.Factors.Select(Function(v) New XElement("factor", v.Name))), SectionsXml(report), New XElement("plots", projection.Plots.Select(Function(v) New XElement("plot", v.Request.Kind))))
        Emit(root) : Return projection
    End Function
    Public Shared Function PriorText(id As Integer, groups() As Object) As String
        Dim labels = groups.Select(Function(v) Convert.ToString(v, CultureInfo.CurrentCulture).Trim()).Distinct(StringComparer.Ordinal).Reverse().ToArray()
        Dim text = String.Join(";", labels.Select(Function(v, i) v & "=" & (i + 1).ToString(CultureInfo.InvariantCulture)))
        Select Case id
            Case 172 : text &= ";a=9"
            Case 173 : text = "a=0;A=1"
            Case 174 : text = "a=-1;A=2"
            Case 175 : text = text.Substring(0, text.IndexOf("="c) + 1) & "NaN" & text.Substring(text.IndexOf(";"c))
            Case 176 : text = text.Substring(0, text.IndexOf("="c) + 1) & "Infinity" & text.Substring(text.IndexOf(";"c))
            Case 177 : text = "a=1"
            Case 178 : text = "a wrong"
            Case 179 : text = ""
        End Select
        Return text
    End Function
    Public Shared Function DiscriminantProfile(id As Integer, Optional capture As Func(Of MultivariateInputIntent, MultivariateDiscriminantRequest) = Nothing) As MultivariateDiscriminantProjection
        Dim edge = If(id >= 144 AndAlso id <= 156, id - 144, If(id >= 180 AndAlso id <= 184, id - 180, If(id >= 185, 9, -1)))
        Dim loaded = DiscriminantData(2 + id Mod 3, edge), x = loaded.x, groups = loaded.groups
        Dim input = Multivariate20Fixture.Input(MultivariateAnalysisKind.Discriminant, x, id Mod 2 = 1), o = DiscriminantOptions(id)
        Dim request = If(capture Is Nothing, New MultivariateDiscriminantRequest(input, o, id Mod 2 = 1), capture(input))
        Dim a As New DiscriminantAnalysis, b As New DiscriminantAnalysis, priors As MultivariateUserPriorIntent = Nothing
        Dim priorLabels() As Object = Nothing, weights() As Double = Nothing, priorTextValue = PriorText(id, groups), phase = "Data", validation As MultivariateDiscriminantValidationRequest = Nothing
        Dim expected = Multivariate19Fixture.Failure(Sub()
            a.dataInputs(x, groups, input.RowLabels, input.VariableNames.Skip(1).ToArray())
            a.settingsInputs(o.Method, o.Standardization, o.Missing, o.Priors, o.Regularization)
            If o.Priors = DiscriminantPriorMode.UserSpecified Then
                MultivariateLegacyChoices.ParsePriors(priorTextValue, priorLabels, weights) : a.priorInputs(priorLabels, weights)
            End If
            a.validationInputs(o.Validation, o.Folds, o.HoldoutFraction, o.Seed, o.Stratified) : a.Fit()
        End Sub)
        Dim actual = Multivariate19Fixture.Failure(Sub()
            b.dataInputs(x, groups, input.RowLabels, input.VariableNames.Skip(1).ToArray())
            phase = "ModelOptions" : request.ConfigureModel(b)
            If o.Priors = DiscriminantPriorMode.UserSpecified Then
                phase = "PriorParse" : MultivariateLegacyChoices.ParsePriors(priorTextValue, priorLabels, weights)
                phase = "PriorInputs" : priors = request.ApplyUserPriors(b, priorLabels, weights)
            End If
            phase = "ValidationInputs" : validation = New MultivariateDiscriminantValidationRequest(o) : validation.Configure(b)
            phase = "Fit" : b.Fit()
        End Sub)
        Multivariate19Fixture.SameFailure(expected, actual)
        Dim root = ProfileRoot("DA", id, input, x, o, If(actual Is Nothing, "CORE_RETURNED", "EXISTING_CORE_REJECTED"), phase, actual, groups)
        root.Add(New XElement("prior_text", priorTextValue), New XElement("effective_seed", "NOT_EXPOSED_BY_PUBLIC_CORE_API"))
        If priors IsNot Nothing Then root.Add(New XElement("prior_entries", priors.Entries.Select(Function(v) New XElement("prior", New XAttribute("index", v.Index), New XAttribute("type", v.SourceType),
            New XAttribute("weight", v.Weight.ToString("R", CultureInfo.InvariantCulture)), Convert.ToString(v.SourceValue, CultureInfo.CurrentCulture)))))
        If actual IsNot Nothing Then
            If b.PreparedData IsNot Nothing Then root.Add(New XElement("prepared_active_ordinals", b.PreparedData.ActiveOriginalIndices.Select(Function(v) New XElement("row", v))), New XElement("prepared_removed_ordinals", b.PreparedData.RemovedOriginalIndices.Select(Function(v) New XElement("row", v))))
            root.Add(New XElement("fit_completed", False), New XElement("training_available", b.TrainingClassification IsNot Nothing))
            If b.TrainingClassification IsNot Nothing Then root.Add(New XElement("training_sha256", PredictionDigest(b.TrainingClassification)))
            Emit(root) : Return Nothing
        End If
        Assert.AreEqual(DiscriminantDigest(a), DiscriminantDigest(b))
        Dim projection = MultivariateDiscriminantProjectionPolicy.Project(request, validation, priors, b), tables = b.wrapResults(), report = projection.ProjectReport(tables)
        Multivariate17Fixture.Tables(a.wrapResults(), report.LegacyTables()) : Multivariate17Fixture.Tables(tables, projection.ProjectReport(tables).LegacyTables())
        Multivariate17Fixture.Cells(MultivariateWorkflowPolicy.DiscriminantInputTable(Raw(x, groups), input.VariableNames, groups, input.RowLabels), request.DataTable(Raw(x, groups), groups))
        CollectionAssert.AreEqual(b.GroupLabels, projection.Classes.ToArray())
        CollectionAssert.AreEqual(b.PreparedData.ActiveOriginalIndices, projection.Observations.Where(Function(v) v.ActiveOrdinal.HasValue).Select(Function(v) v.InputOrdinal).ToArray())
        Assert.AreEqual(b.ValidationClassification IsNot Nothing, projection.Validation IsNot Nothing)
        Assert.IsTrue(projection.Training.IsTrainingResubstitution) : Assert.AreEqual(0, projection.Plots.Count)
        Dim predictions = New List(Of MultivariateDiscriminantPrediction) From {projection.Training}
        If projection.Validation IsNot Nothing Then predictions.Add(projection.Validation)
        For Each prediction In predictions
            Dim core = If(prediction.IsTrainingResubstitution, b.TrainingClassification, b.ValidationClassification)
            CollectionAssert.AreEqual(core.RowIndices, prediction.Cases.Select(Function(v) v.CoreInputOrdinal).ToArray())
            CollectionAssert.AreEqual(core.ActualGroupLabels, prediction.Cases.Select(Function(v) v.ActualClass).ToArray())
            CollectionAssert.AreEqual(core.PredictedGroupLabels, prediction.Cases.Select(Function(v) v.PredictedClass).ToArray())
            CollectionAssert.AreEqual(core.Confusion.RowTotals, prediction.Confusion.RowTotals) : CollectionAssert.AreEqual(core.Confusion.ColumnTotals, prediction.Confusion.ColumnTotals)
            CollectionAssert.AreEqual(core.Confusion.RecallPct, prediction.Confusion.RecallPercent) : CollectionAssert.AreEqual(core.Confusion.PrecisionPct, prediction.Confusion.PrecisionPercent)
            Assert.AreEqual(core.Confusion.OverallAccuracyPct, prediction.Confusion.AccuracyPercent)
            Assert.AreEqual(Multivariate18Fixture.Digest(core.Confusion.Counts), Multivariate18Fixture.Digest(prediction.Confusion.Counts))
        Next
        root.Add(New XElement("nonfinite_report_cells", tables.Sum(Function(t) t.ToOutputModel().Values.Cast(Of Object)().Count(Function(v) TypeOf v Is Double AndAlso (Double.IsNaN(CDbl(v)) OrElse Double.IsInfinity(CDbl(v)))))),
            New XElement("numerical_sha256", DiscriminantDigest(b)), New XElement("classes", projection.Classes.Select(Function(v) New XElement("class", v))),
            New XElement("class_statistics", projection.GroupStatistics.Select(Function(v) New XElement("class", New XAttribute("label", v.Label), New XAttribute("count", v.Count), New XAttribute("prior", v.Prior.ToString("R", CultureInfo.InvariantCulture))))),
            New XElement("observations", projection.Observations.Select(Function(v) New XElement("row", New XAttribute("input", v.InputOrdinal), New XElement("source", v.SourceRowId), New XElement("active", v.ActiveOrdinal), New XElement("removed", v.RemovedOrdinal), New XElement("label", v.DisplayLabel), New XElement("actual", v.ActualClass)))),
            New XElement("predictions", predictions.Select(Function(v) New XElement("population", New XAttribute("training", v.IsTrainingResubstitution), New XAttribute("validation_mode", v.ValidationMode), New XAttribute("posterior_rows", v.PosteriorShape.Rows), New XAttribute("posterior_columns", v.PosteriorShape.Columns),
                New XAttribute("confusion_rows", v.Confusion.TableShape.Rows), New XAttribute("confusion_columns", v.Confusion.TableShape.Columns),
                New XElement("cases", v.Cases.Select(Function(r) New XElement("case", New XAttribute("core_row", r.CoreInputOrdinal), New XElement("source", r.SourceRowId), New XElement("label", r.DisplayLabel), New XElement("actual", r.ActualClass), New XElement("predicted", r.PredictedClass), New XElement("fold", r.Fold)))),
                New XElement("score_classes", v.ScoreClasses.Select(Function(c) New XElement("class", c))), New XElement("observed_classes", v.Confusion.ObservedClasses.Select(Function(c) New XElement("class", c))),
                New XElement("predicted_classes", v.Confusion.PredictedClasses.Select(Function(c) New XElement("class", c))), New XElement("accuracy_percent", v.Confusion.AccuracyPercent)))), SectionsXml(report))
        Assert.AreEqual(DiscriminantDigest(a), DiscriminantDigest(b))
        Emit(root) : Return projection
    End Function
    Public Shared Sub UnspecifiedSeedRecord(input As MultivariateInputIntent, x(,) As Double, groups() As Object, options As MultivariateDiscriminantOptions, model As DiscriminantAnalysis)
        Dim root = ProfileRoot("DA", -1, input, x, options, "UNSPECIFIED_SEED_OBSERVATION", "Fit", Nothing, groups)
        root.Add(New XElement("effective_seed", "NOT_EXPOSED_BY_PUBLIC_CORE_API"), New XElement("comparison", "One actual Core execution and projection; independent unspecified splits are not asserted equal."),
            New XElement("numerical_sha256", DiscriminantDigest(model)), New XElement("validation_input_ordinals", model.ValidationClassification.RowIndices.Select(Function(v) New XElement("row", v))),
            New XElement("folds", model.ValidationClassification.FoldAssignments.Select(Function(v) New XElement("fold", v))),
            New XElement("predictions", model.ValidationClassification.PredictedGroupLabels.Select(Function(v) New XElement("class", v))))
        Emit(root)
    End Sub
End Class

<TestClass>
Public Class Multivariate20Tests
    <TestMethod>
    Public Sub RequestsRejectOnlyNewNullContractsAndKeepMismatchDiagnosticsNonblocking()
        Dim input As New MultivariateInputIntent(MultivariateAnalysisKind.Factor, 4, 2, {"only"}, {101}, {"short"})
        Assert.ThrowsException(Of ArgumentNullException)(Sub() NewFactor(Nothing, New MultivariateFactorOptions))
        Assert.ThrowsException(Of ArgumentNullException)(Sub() NewFactor(input, Nothing))
        Assert.ThrowsException(Of ArgumentNullException)(Sub() NewDiscriminant(Nothing, New MultivariateDiscriminantOptions))
        Assert.ThrowsException(Of ArgumentNullException)(Sub() NewDiscriminant(input, Nothing))
        Assert.ThrowsException(Of ArgumentNullException)(Sub() NewValidation(Nothing))
        Dim request As New MultivariateFactorRequest(input, New MultivariateFactorOptions)
        Assert.IsTrue(input.Validation.IsValid) : Assert.IsTrue(input.Validation.Diagnostics.Count > 0)
        Dim direct As New FactorAnalysis, projected As New FactorAnalysis, x = Multivariate20Fixture.FactorData()
        direct.dataInputs(x, input.SourceRowIds, input.VariableNames) : projected.dataInputs(x, input.SourceRowIds, input.VariableNames)
        Dim o = request.Options : direct.settingsInputs(o.MaximumIterations, o.Epsilon, o.MatrixType, o.Extraction, o.Retention, o.RetentionValue, o.Rotation, o.Scores, o.Communality, o.Missing, o.KaiserNormalization, o.PromaxPower) : request.Configure(projected)
        Multivariate19Fixture.SameFailure(Multivariate19Fixture.Failure(Sub() direct.Calculate()), Multivariate19Fixture.Failure(Sub() projected.Calculate()))
    End Sub
    Private Shared Sub NewFactor(input As MultivariateInputIntent, o As MultivariateFactorOptions)
        Dim request As New MultivariateFactorRequest(input, o)
    End Sub
    Private Shared Sub NewDiscriminant(input As MultivariateInputIntent, o As MultivariateDiscriminantOptions)
        Dim request As New MultivariateDiscriminantRequest(input, o)
    End Sub
    Private Shared Sub NewValidation(o As MultivariateDiscriminantOptions)
        Dim request As New MultivariateDiscriminantValidationRequest(o)
    End Sub

    <TestMethod>
    Public Sub FactorRetainedAxesAndReportsAreDetachedFromOptionsAndCoreArrays()
        Dim x = Multivariate20Fixture.FactorData(), input = Multivariate20Fixture.Input(MultivariateAnalysisKind.Factor, x), o = Multivariate20Fixture.FactorOptions(23)
        o.RetentionValue = 3 : o.Scores = FactorAnalysisScoreMethod.Bartlett : o.Rotation = FactorAnalysisRotationMethod.Promax
        Dim request As New MultivariateFactorRequest(input, o), model As New FactorAnalysis
        o.RetentionValue = 99 : request.Options.RetentionValue = 100
        model.dataInputs(x, input.SourceRowIds, input.VariableNames) : request.Configure(model) : model.Calculate()
        Dim projection = MultivariateFactorProjectionPolicy.Project(request, model), tables = MultivariateReportPolicy.Project(input, model.wrapResults()).LegacyTables(), report = projection.ProjectReport(tables)
        Assert.AreEqual(3, projection.ActualFactorCount) : Assert.AreEqual(3.0, request.Options.RetentionValue)
        Assert.AreEqual(x.GetLength(0), projection.Arrays.Single(Function(a) a.Role = "Scores").Shape.Rows)
        Assert.AreEqual(x.GetLength(1), projection.Arrays.Single(Function(a) a.Role = "PatternMatrix").Shape.Rows)
        Assert.AreEqual(3, projection.Arrays.Single(Function(a) a.Role = "Phi").Shape.Rows)
        Dim name = projection.Variables(0).Name, row = projection.ActiveObservations(0).CoreRowId
        model.VariableNames(0) = "mutated" : model.AnalysisRowIds(0) = -7 : model.PatternMatrix(0, 0) = 999
        input.VariableNames(0) = "returned array mutation"
        Assert.AreEqual(name, projection.Variables(0).Name) : Assert.AreEqual(row, projection.ActiveObservations(0).CoreRowId)
        report.LegacyTables().Clear() : report.Report.Document.Tables(0).Model.Values(0, 0) = "changed"
        Multivariate17Fixture.Tables(tables, report.LegacyTables())
        Assert.ThrowsException(Of NotSupportedException)(Sub() DirectCast(projection.Factors, IList(Of MultivariateAxisItem)).Clear())
    End Sub

    <DataTestMethod><DataRow(False)><DataRow(True)>
    Public Sub FactorDeletionUsesCorePhysicalScoreIdsAndDoesNotGuessDuplicateSources(duplicates As Boolean)
        Dim x = Multivariate20Fixture.FactorData(0), rows = Enumerable.Range(0, x.GetLength(0)).Select(Function(i) If(duplicates, 101, 101 + i * 17)).ToArray()
        Dim input As New MultivariateInputIntent(MultivariateAnalysisKind.Factor, x.GetLength(0), 6, {"a", "b", "c", "d", "e", "f"}, rows)
        Dim request As New MultivariateFactorRequest(input, New MultivariateFactorOptions With {.Missing = FactorAnalysisMissingValuePolicy.ListwiseDeletion}), model As New FactorAnalysis
        model.dataInputs(x, input.SourceRowIds, input.VariableNames) : request.Configure(model) : model.Calculate()
        Dim projection = MultivariateFactorProjectionPolicy.Project(request, model)
        Assert.AreEqual(1, projection.RemovedRowCount) : Assert.AreEqual(35, projection.ActiveObservations.Count)
        If duplicates Then
            Assert.IsTrue(projection.ActiveObservations.All(Function(v) v.State = MultivariateMappingState.Ambiguous))
            Assert.AreEqual(0, projection.RemovedInputOrdinals.Count) : Assert.IsTrue(projection.Validation.IsValid)
        Else
            CollectionAssert.AreEqual(New Integer() {2}, projection.RemovedInputOrdinals.ToArray())
            Assert.AreEqual(3, projection.ActiveObservations(1).InputOrdinal.Value) : Assert.AreEqual(135, projection.ActiveObservations(1).SourceRowId.Value)
        End If
        Assert.IsFalse(request.DataSection.IncludesRowLabel) : Assert.AreEqual("OriginalRow", request.DataSection.Headers(0))
    End Sub

    <TestMethod>
    Public Sub FactorEigenvalueBoundaryAndDisabledPromaxValueRemainCoreOwned()
        Dim x = Multivariate20Fixture.FactorData(), input = Multivariate20Fixture.Input(MultivariateAnalysisKind.Factor, x), model As New FactorAnalysis
        Dim o As New MultivariateFactorOptions With {.Extraction = FactorAnalysisExtractionMethod.PrincipalComponents, .Rotation = FactorAnalysisRotationMethod.None}
        model.dataInputs(x, input.SourceRowIds, input.VariableNames) : o.Configure(model) : model.Calculate()
        o.Retention = FactorAnalysisRetentionMethod.Eigenvalue : o.RetentionValue = model.InitialEigenvalues(1)
        Dim boundary As New FactorAnalysis, request As New MultivariateFactorRequest(input, o)
        boundary.dataInputs(x, input.SourceRowIds, input.VariableNames) : request.Configure(boundary) : boundary.Calculate()
        Assert.AreEqual(model.InitialEigenvalues.Count(Function(v) v >= o.RetentionValue), boundary.NumberOfFactors)
        o.PromaxPower = 1 : o.Rotation = FactorAnalysisRotationMethod.None
        Dim bad As New FactorAnalysis : bad.dataInputs(x, input.SourceRowIds, input.VariableNames) : o.Configure(bad)
        Assert.IsInstanceOfType(Multivariate19Fixture.Failure(Sub() bad.Calculate()), GetType(ArgumentOutOfRangeException))
    End Sub

    <DataTestMethod><DataRow(0)><DataRow(1)><DataRow(2)><DataRow(3)><DataRow(4)><DataRow(5)><DataRow(6)><DataRow(7)><DataRow(8)><DataRow(9)><DataRow(10)><DataRow(11)><DataRow(12)><DataRow(13)><DataRow(14)><DataRow(15)>
    Public Sub PriorGrammarRetainsFrozenCultureParsingDuplicatesAndFailureTiming(id As Integer)
        Dim cases = New String() {"", " ", "a=.4; A:.6", "a=0,4; A=0,6", "a=1.5;A=2.5", "a=1;a=9;A=2", "a=2" & ChrW(13) & ChrW(10) & "A:3", "bad", "; ;", "=1", "a:", ":1", "a=NaN;A=1", "a=Infinity;A=1", "a=1,234; A=2", Nothing}
        Dim saved = CultureInfo.CurrentCulture
        Try
            For Each culture In {"", "sk-SK", "de-DE"}
                CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(culture)
                Dim labels() As Object = Nothing, oldLabels() As Object = Nothing, values() As Double = Nothing, oldValues() As Double = Nothing
                Dim a = Multivariate19Fixture.Failure(Sub() LegacyMultivariate17Selection.ParsePriors(cases(id), oldLabels, oldValues)), b = Multivariate19Fixture.Failure(Sub() MultivariateLegacyChoices.ParsePriors(cases(id), labels, values))
                Multivariate19Fixture.SameFailure(a, b)
                If b IsNot Nothing Then Continue For
                CollectionAssert.AreEqual(oldLabels, labels) : CollectionAssert.AreEqual(oldValues, values)
                Dim input As New MultivariateInputIntent(MultivariateAnalysisKind.Discriminant, 0, 2, {"group", "x"}), request As New MultivariateDiscriminantRequest(input, New MultivariateDiscriminantOptions)
                Dim model As New DiscriminantAnalysis, priors = request.ApplyUserPriors(model, labels, values)
                For i = 0 To labels.Length - 1
                    Assert.AreEqual(labels(i), priors.Entries(i).SourceValue) : Assert.AreEqual(values(i), priors.Entries(i).Weight)
                Next
                If labels.Length > 0 Then
                    Dim first = priors.Entries(0).SourceValue : labels(0) = "mutated" : values(0) = -999
                    Assert.AreEqual(first, priors.Entries(0).SourceValue) : Assert.AreNotEqual(-999.0, priors.Entries(0).Weight)
                End If
            Next
        Finally
            CultureInfo.CurrentCulture = saved
        End Try
    End Sub

    <TestMethod>
    Public Sub PriorInputsNullLengthExceptionsStayInCoreBeforeMetadata()
        Dim input As New MultivariateInputIntent(MultivariateAnalysisKind.Discriminant, 1, 2, {"g", "x"}), request As New MultivariateDiscriminantRequest(input, New MultivariateDiscriminantOptions)
        For id = 0 To 2
            Dim labels = If(id = 0, Nothing, New Object() {"a"}), weights = If(id = 1, Nothing, If(id = 2, New Double() {}, New Double() {1}))
            Dim a As New DiscriminantAnalysis, b As New DiscriminantAnalysis
            Multivariate19Fixture.SameFailure(Multivariate19Fixture.Failure(Sub() a.priorInputs(labels, weights)), Multivariate19Fixture.Failure(Sub() request.ApplyUserPriors(b, labels, weights)))
        Next
    End Sub

    <DataTestMethod><DataRow(False)><DataRow(True)>
    Public Sub DiscriminantSourceOrdinalsConfusionAndCopiesStayIndependent(labelled As Boolean)
        Dim loaded = Multivariate20Fixture.DiscriminantData(3, 0), x = loaded.x, groups = loaded.groups
        Dim input = Multivariate20Fixture.Input(MultivariateAnalysisKind.Discriminant, x, labelled), o As New MultivariateDiscriminantOptions With {.Missing = ClusterMissingValuePolicy.ListwiseDeletion, .Validation = DiscriminantValidationMode.Holdout, .Seed = 0}
        Dim request As New MultivariateDiscriminantRequest(input, o), validation As New MultivariateDiscriminantValidationRequest(o), model As New DiscriminantAnalysis
        o.Seed = 999 : o.Method = DiscriminantAnalysisMethod.Quadratic : validation.Options.Seed = -9
        model.dataInputs(x, groups, input.RowLabels, input.VariableNames.Skip(1).ToArray()) : request.ConfigureModel(model) : validation.Configure(model) : model.Fit()
        Dim projection = MultivariateDiscriminantProjectionPolicy.Project(request, validation, Nothing, model), expected = MultivariateReportPolicy.Project(input, model.wrapResults()).LegacyTables(), report = projection.ProjectReport(expected)
        Assert.AreEqual(0, validation.Options.Seed) : Assert.IsTrue(validation.HasExplicitSeed) : Assert.IsFalse(validation.EffectiveSeed.HasValue)
        Assert.AreEqual(1, projection.Observations(0).RemovedOrdinal.Value) : Assert.AreEqual(2, projection.Training.Cases(0).CoreInputOrdinal)
        Assert.AreEqual(input.SourceRowIds(1), projection.Training.Cases(0).SourceRowId.Value)
        Assert.IsTrue(projection.Validation.Cases.Count < projection.Training.Cases.Count) : Assert.IsFalse(projection.Validation.IsTrainingResubstitution)
        Assert.AreEqual(DiscriminantValidationMode.Holdout, projection.Validation.ValidationMode)
        Assert.AreEqual(If(labelled, "Row label", "group"), request.DataSection.Headers(0)) : Assert.IsFalse(request.DataSection.Headers.Contains("OriginalRow"))
        CollectionAssert.AreEqual(model.GroupLabels, projection.Training.ScoreClasses.ToArray())
        Dim oldName = projection.Classes(0), oldLabel = projection.Training.Cases(0).DisplayLabel, counts = projection.Training.Confusion.Counts
        counts(0, 0) = -888 : projection.Training.Confusion.RowTotals(0) = -888
        model.GroupLabels(0) = "changed" : model.PreparedData.ActiveRowLabels(0) = "changed" : model.TrainingClassification.Confusion.Counts(0, 0) = -999
        Assert.AreEqual(oldName, projection.Classes(0)) : Assert.AreEqual(oldLabel, projection.Training.Cases(0).DisplayLabel)
        Assert.AreNotEqual(-888.0, projection.Training.Confusion.Counts(0, 0)) : Assert.AreNotEqual(-999.0, projection.Training.Confusion.Counts(0, 0))
        Multivariate17Fixture.Tables(expected, report.LegacyTables())
    End Sub

    <DataTestMethod><DataRow(0)><DataRow(1)><DataRow(1729)><DataRow(-4)>
    Public Sub FixedValidationSeedsReplayExactCoreFoldsAndPredictions(seed As Integer)
        Dim loaded = Multivariate20Fixture.DiscriminantData(3), input = Multivariate20Fixture.Input(MultivariateAnalysisKind.Discriminant, loaded.x)
        For Each mode In {DiscriminantValidationMode.KFold, DiscriminantValidationMode.Holdout}
            Dim o As New MultivariateDiscriminantOptions With {.Validation = mode, .Folds = 3, .Seed = seed}, request As New MultivariateDiscriminantRequest(input, o), validation As New MultivariateDiscriminantValidationRequest(o)
            Dim a As New DiscriminantAnalysis, b As New DiscriminantAnalysis
            For Each model In {a, b} : model.dataInputs(loaded.x, loaded.groups) : request.ConfigureModel(model) : validation.Configure(model) : model.Fit() : Next
            Assert.AreEqual(Multivariate20Fixture.PredictionDigest(a.ValidationClassification), Multivariate20Fixture.PredictionDigest(b.ValidationClassification))
        Next
    End Sub

    <TestMethod>
    Public Sub UnspecifiedValidationSeedIsObservedWithoutInventingAnEffectiveSeed()
        Dim loaded = Multivariate20Fixture.DiscriminantData(3), input = Multivariate20Fixture.Input(MultivariateAnalysisKind.Discriminant, loaded.x)
        Dim o As New MultivariateDiscriminantOptions With {.Validation = DiscriminantValidationMode.KFold, .Folds = 3, .Seed = Integer.MinValue}, request As New MultivariateDiscriminantRequest(input, o), validation As New MultivariateDiscriminantValidationRequest(o), model As New DiscriminantAnalysis
        model.dataInputs(loaded.x, loaded.groups, input.RowLabels, input.VariableNames.Skip(1).ToArray()) : request.ConfigureModel(model) : validation.Configure(model) : model.Fit()
        Dim projection = MultivariateDiscriminantProjectionPolicy.Project(request, validation, Nothing, model)
        Assert.IsFalse(validation.HasExplicitSeed) : Assert.IsFalse(validation.EffectiveSeed.HasValue)
        Assert.AreEqual(model.ValidationClassification.RowIndices.Length, projection.Validation.Cases.Count)
        Multivariate20Fixture.UnspecifiedSeedRecord(input, loaded.x, loaded.groups, o, model)
    End Sub
    Public Shared Iterator Function Profiles() As IEnumerable(Of Object())
        For Each family In {"FA", "DA"}
            For id = 0 To If(family = "FA", 225, 187) : Yield {family, id} : Next
        Next
    End Function
    <DataTestMethod><DynamicData(NameOf(Profiles), DynamicDataSourceType.Method)>
    Public Sub CoreConfiguredChoicesAndDetachedAxesReports(family As String, id As Integer)
        Dim saved = CultureInfo.CurrentCulture
        Try
            For Each culture In If(family = "FA", New String() {"", "sk-SK"}, New String() {"", "sk-SK", "de-DE"})
                CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(culture)
                If family = "FA" Then
                    Multivariate20Fixture.FactorProfile(id)
                Else
                    Multivariate20Fixture.DiscriminantProfile(id)
                End If
            Next
        Finally
            CultureInfo.CurrentCulture = saved
        End Try
    End Sub
End Class
