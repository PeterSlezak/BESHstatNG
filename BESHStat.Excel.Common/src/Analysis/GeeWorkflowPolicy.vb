Option Explicit On
Option Strict On
Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports BESHStatNG.DataManagement
Imports BESHStatNG.regression

Namespace ExcelCommon
    Public Enum GeeStandardError
        Robust
        Naive
        BiasReduced
    End Enum

    ''' <summary>GEE-only options. Family/link/inference/start/section options remain in the
    ''' accepted RegressionRequest. The Windows intercept switch is recorded separately
    ''' from the implicit intercept always added by the existing Core GEE.</summary>
    Public NotInheritable Class GeeWorkflowOptions
        Public Property Correlation As String = "Independence"
        Public Property StandardError As GeeStandardError = GeeStandardError.Robust
        Public Property UseP As Boolean
        Public Property RequestedIntercept As Boolean = True
        Public Property TraceRequested As Boolean
        Public Function Copy() As GeeWorkflowOptions
            Return DirectCast(MemberwiseClone(), GeeWorkflowOptions)
        End Function
    End Class

    Public NotInheritable Class GeeClusterVector
        Private ReadOnly _values As Object()
        Private ReadOnly _rows As Integer()
        Public Sub New(key As String, values() As Object, rowIds() As Integer)
            Me.Key = key
            If values IsNot Nothing AndAlso values.Any(Function(v) v IsNot Nothing AndAlso Type.GetTypeCode(v.GetType()) = TypeCode.Object) Then Throw New ArgumentException("gee.cluster.scalar: Cluster identities must be immutable scalar values.")
            _values = If(values Is Nothing, Nothing, DirectCast(values.Clone(), Object()))
            _rows = If(rowIds Is Nothing, Nothing, DirectCast(rowIds.Clone(), Integer()))
        End Sub
        Public ReadOnly Property Key As String
        Public ReadOnly Property Values As Object()
            Get
                Return If(_values Is Nothing, Nothing, DirectCast(_values.Clone(), Object()))
            End Get
        End Property
        Public ReadOnly Property RowIds As Integer()
            Get
                Return If(_rows Is Nothing, Nothing, DirectCast(_rows.Clone(), Integer()))
            End Get
        End Property
    End Class

    ''' <summary>Observed import order, Core cluster-flattening map and within-cluster
    ''' position are different axes. Never sorts or mutates an imported model matrix.</summary>
    Public NotInheritable Class GeeClusterPlan
        Private ReadOnly _flattened, _positions, _rows As Integer()
        Private ReadOnly _visits As Double()
        Friend Sub New(rows() As Integer, flattened() As Integer, positions() As Integer, visits() As Double, count As Integer)
            _rows = DirectCast(rows.Clone(), Integer()) : _flattened = DirectCast(flattened.Clone(), Integer())
            _positions = DirectCast(positions.Clone(), Integer()) : _visits = DirectCast(visits.Clone(), Double())
            ClusterCount = count
        End Sub
        Public ReadOnly Property ClusterCount As Integer
        Public ReadOnly Property InputRowIds As Integer()
            Get
                Return DirectCast(_rows.Clone(), Integer())
            End Get
        End Property
        Public ReadOnly Property ClusteredToInputIndices As Integer()
            Get
                Return DirectCast(_flattened.Clone(), Integer())
            End Get
        End Property
        Public ReadOnly Property CorePredictionRowIds As Integer()
            Get
                Return _flattened.Select(Function(i) _rows(i)).ToArray()
            End Get
        End Property
        Public ReadOnly Property WithinClusterPositions As Integer()
            Get
                Return DirectCast(_positions.Clone(), Integer())
            End Get
        End Property
        Public ReadOnly Property DistinctVisitLabels As Double()
            Get
                Return DirectCast(_visits.Clone(), Double())
            End Get
        End Property
        Public ReadOnly Property PredictionOrderMatchesInput As Boolean
            Get
                Return _flattened.SequenceEqual(Enumerable.Range(0, _rows.Length))
            End Get
        End Property
    End Class

    Public NotInheritable Class GeeWorkflowIntent
        Private ReadOnly _options As GeeWorkflowOptions
        Friend Sub New(mean As RegressionRequest, cluster As GeeClusterVector, time As RegressionVector,
                       options As GeeWorkflowOptions, order As GeeClusterPlan, validation As RegressionValidation)
            MeanRequest = mean : Me.Cluster = cluster : Me.Time = time : _options = options.Copy()
            Me.Order = order : Me.Validation = validation
        End Sub
        ''' <summary>GLM describes the marginal mean only; the workflow/report identity is GEE.</summary>
        Public ReadOnly Property MeanRequest As RegressionRequest
        Public ReadOnly Property Cluster As GeeClusterVector
        Public ReadOnly Property Time As RegressionVector
        Public ReadOnly Property Order As GeeClusterPlan
        Public ReadOnly Property Validation As RegressionValidation
        Public ReadOnly Property Options As GeeWorkflowOptions
            Get
                Return _options.Copy()
            End Get
        End Property
        Public ReadOnly Property EffectiveIntercept As Boolean
            Get
                Return True
            End Get
        End Property
        Public ReadOnly Property ExpectedParameterCount As Integer
            Get
                Return If(MeanRequest.Primary.Data Is Nothing, 0, MeanRequest.Primary.Data.GetLength(1))
            End Get
        End Property
        Public ReadOnly Property RequestedAlpha As Double
            Get
                Return MeanRequest.Options.Alpha
            End Get
        End Property
        Public ReadOnly Property EffectiveFitAlpha As Double
            Get
                Return RequestedAlpha
            End Get
        End Property
        Public ReadOnly Property WeightsEnterEstimatingEquations As Boolean
            Get
                Return False
            End Get
        End Property
        Public ReadOnly Property CoreStandardErrorName As String
            Get
                Return GeeWorkflowPolicy.StandardErrorNames()(CInt(_options.StandardError))
            End Get
        End Property
        Public Function CreateFamily() As Family
            Dim o = MeanRequest.Options, family = FamilyUtils.createFamily(o.Family.Value.ToString())
            ' Exact legacy UI normalization: only positive supplied values are applied.
            ' Diagnostics remain observational; no new finite-value refusal here.
            If o.Dispersion.HasValue AndAlso o.Dispersion.Value > 0 Then family.pdAlpha = o.Dispersion.Value
            Return family
        End Function
        Public Function CreateLink() As Link
            Dim o = MeanRequest.Options
            Return If(o.Link = RegressionLink.Power, LinkUtils.createLink(o.Link.Value.ToString(), o.Power.GetValueOrDefault()), LinkUtils.createLink(o.Link.Value.ToString()))
        End Function
        Public Function CreateCorrelation() As GEEcovStruct
            ' Core owns every alias, including its retained Exchangable spelling.
            Return GEEcovStructUtils.createGEEcovMat(_options.Correlation)
        End Function
        Public Function CreateCoreModel() As GEE
            Dim o = MeanRequest.Options, primary = MeanRequest.Primary
            Dim fit As New GEE(CreateFamily(), CreateLink(), CreateCorrelation(), CoreStandardErrorName)
            fit.data(primary.Data, Cluster.Values, primary.RowIds,
                     If(MeanRequest.Offset Is Nothing, Nothing, MeanRequest.Offset.Values),
                     If(MeanRequest.Weights Is Nothing, Nothing, MeanRequest.Weights.Values),
                     If(Time Is Nothing, Nothing, Time.Values))
            fit.setVarNames(primary.Names, Cluster.Key,
                            If(MeanRequest.Offset Is Nothing, Nothing, MeanRequest.Offset.Key),
                            If(MeanRequest.Weights Is Nothing, Nothing, MeanRequest.Weights.Key), If(Time Is Nothing, Nothing, Time.Key))
            fit.bComputeResiduals = o.Residuals : fit.bIterationDetails = o.IterationDetails
            fit.settingInputs(EffectiveFitAlpha, o.MaximumIterations, o.Epsilon, _options.UseP)
            fit.startParams = primary.InitialValues
            Return fit
        End Function
    End Class

    Public NotInheritable Class GeeWorkflowPolicy
        Private Sub New()
        End Sub
        Public Shared ReadOnly Property OmitCategoricalReference As Boolean
            Get
                Return True
            End Get
        End Property
        Public Shared Function StandardErrorNames() As IReadOnlyList(Of String)
            Return Array.AsReadOnly({"Robust", "Naive", "Bias Reduced"})
        End Function
        Public Shared Function FamilyNames() As IReadOnlyList(Of String)
            Return Array.AsReadOnly(DirectCast(Family.FamiliesList.Clone(), String()))
        End Function
        Public Shared Function CorrelationNames() As IReadOnlyList(Of String)
            Return Array.AsReadOnly(DirectCast(GEEcovStruct.CovStructsList.Clone(), String()))
        End Function
        Public Shared Function FamilyCode(display As String) As String
            Dim index = Array.IndexOf(Family.FamiliesList, display)
            Return If(index < 0, String.Empty, Family.FamiliesCodes(index))
        End Function
        Public Shared Function SupportedLinks(familyCode As String) As IReadOnlyList(Of String)
            Dim selected = FamilyUtils.createFamily(familyCode)
            Return Link.LinkList.Values.Where(Function(value) selected.testLink(value)).ToList().AsReadOnly()
        End Function
        Public Shared Function ExpandedParameterCount(data(,) As Double) As Integer
            Return If(data Is Nothing, 0, data.GetLength(1))
        End Function

        Public Shared Function Project(mean As RegressionRequest, cluster As GeeClusterVector,
                                       Optional time As RegressionVector = Nothing,
                                       Optional options As GeeWorkflowOptions = Nothing) As GeeWorkflowIntent
            If mean Is Nothing OrElse mean.Primary Is Nothing OrElse mean.Kind <> RegressionAnalysisKind.GeneralizedLinearModel Then Throw New ArgumentException("gee.mean.required: A typed marginal GLM mean specification is required.")
            Dim o = If(options, New GeeWorkflowOptions()).Copy()
            ' Preserve authored terms; describe the intercept that Core actually fits.
            Dim p = mean.Primary
            Dim effective As New RegressionComponent(p.Data, p.Names, p.RowIds,
                If(p.Effects Is Nothing, Nothing, New RegressionEffectModel(p.Effects.Effects, True)), p.SourceKeys, p.InitialValues, p.ResponseSources)
            Dim request As New RegressionRequest(mean.Kind, mean.ResponseKey, effective, If(mean.OptionsDefaulted, Nothing, mean.Options), mean.Offset, mean.Weights)
            Dim generic = RegressionRequestPolicy.Validate(request), issues As New List(Of RegressionDiagnostic)
            For Each diagnostic In generic.Diagnostics
                If diagnostic.Code = "regression.options.dispersion" AndAlso request.Options.Family <> RegressionFamily.NegativeBinomial AndAlso
                    request.Options.Dispersion.HasValue AndAlso RegressionRequestPolicy.Finite(request.Options.Dispersion.Value) AndAlso request.Options.Dispersion.Value > 0 Then
                    issues.Add(New RegressionDiagnostic("gee.dispersion.unused-family", "options.dispersion", "The legacy positive dispersion property is retained; this family does not use the NB dispersion parameter.", blocking:=False))
                Else
                    issues.Add(diagnostic)
                End If
            Next
            Dim add = Sub(code As String, field As String, message As String, blocking As Boolean)
                          issues.Add(New RegressionDiagnostic("gee." & code, field, message, blocking:=blocking))
                      End Sub
            If options Is Nothing Then add("options.defaulted", "options", "GEE defaults were used.", False)
            If Not o.RequestedIntercept Then add("intercept.implicit", "options.intercept", "Core GEE always includes an intercept; the UI selection is retained as request metadata.", False)
            If request.Weights IsNot Nothing Then add("weights.estimating-equations", "weights", "Weights are retained/echoed and exposed for classification; they do not weight the GEE estimating equations.", False)
            If o.TraceRequested Then add("trace.ui-only", "options.trace", "The legacy trace checkbox has no per-model RunGEE assignment; Core logging remains authoritative.", False)
            If request.Options.MaximumIterations = Integer.MaxValue Then add("options.iterations.overflow", "options.maximumIterations", "Core iteration count plus one overflows Int32.", True)
            If Not [Enum].IsDefined(GetType(GeeStandardError), o.StandardError) Then add("options.standard-error", "options.standardError", "Unsupported GEE standard-error selection.", True)
            Try
                GEEcovStructUtils.createGEEcovMat(o.Correlation)
            Catch ex As Exception
                add("options.correlation", "options.correlation", "Unsupported Core working-correlation selection.", True)
            End Try
            Dim rows = If(p.RowIds, Array.Empty(Of Integer)()), ids = If(cluster Is Nothing, Nothing, cluster.Values)
            Dim validCluster = cluster IsNot Nothing AndAlso Not String.IsNullOrWhiteSpace(cluster.Key) AndAlso ids IsNot Nothing AndAlso
                cluster.RowIds IsNot Nothing AndAlso cluster.RowIds.SequenceEqual(rows) AndAlso ids.Length = rows.Length AndAlso ids.All(Function(v) v IsNot Nothing)
            If Not validCluster Then add("cluster.alignment", "cluster", "Cluster key, immutable identities and source RowIds must be present and aligned.", True)
            Dim times = If(time Is Nothing, Nothing, time.Values)
            If time IsNot Nothing Then
                If String.IsNullOrWhiteSpace(time.Key) OrElse times Is Nothing OrElse time.RowIds Is Nothing OrElse Not time.RowIds.SequenceEqual(rows) OrElse times.Length <> rows.Length Then
                    add("time.alignment", "time", "Time key, values and RowIds must match retained input order.", True)
                ElseIf times.Any(Function(v) Not RegressionRequestPolicy.Finite(v)) Then
                    add("time.nonfinite", "time", "Time values contain non-finite entries; nothing is dropped or coerced.", True)
                End If
            End If
            If p.Data IsNot Nothing AndAlso p.Data.GetLength(0) <= ExpandedParameterCount(p.Data) Then add("data.insufficient", "primary.data", "Core GEE requires more observations than mean coefficients.", True)
            Dim flattened As New List(Of Integer), positions(rows.Length - 1) As Integer, visits As New List(Of Double), groupCount As Integer = 0
            If validCluster Then
                Dim groups = ids.Select(Function(value, index) New With {.Value = value, .Index = index}).GroupBy(Function(v) v.Value).ToList()
                groupCount = groups.Count
                If groups.Select(Function(g) g.Key.ToString()).Distinct(StringComparer.Ordinal).Count() <> groupCount Then add("cluster.text-collision", "cluster", "Distinct cluster values share the same Core text label; Core grouping is ambiguous.", True)
                For Each group In groups
                    Dim at = 0, previous As Nullable(Of Double) = Nothing
                    For Each entry In group
                        flattened.Add(entry.Index) : positions(entry.Index) = at
                        Dim visit = If(time Is Nothing, CDbl(at), If(times IsNot Nothing AndAlso times.Length = rows.Length, times(entry.Index), Double.NaN))
                        If time Is Nothing OrElse (times IsNot Nothing AndAlso times.Length = rows.Length) Then visits.Add(visit)
                        If previous.HasValue AndAlso visit = previous.Value AndAlso time IsNot Nothing Then add("time.duplicate", "time", "Repeated visit labels retain the imported tie order; no stable tie sort is invented.", False)
                        previous = visit : at += 1
                    Next
                Next
                If groupCount = 1 Then add("cluster.single", "cluster", "One cluster cannot establish reliable sandwich inference; actual Core outputs are retained.", False)
                If groups.Any(Function(g) g.Count() = 1) Then add("cluster.singleton", "cluster", "Singleton clusters are retained; some association structures are undefined.", False)
            End If
            Dim order As New GeeClusterPlan(rows, flattened.ToArray(), positions, visits.Distinct().OrderBy(Function(v) v).ToArray(), groupCount)
            If validCluster AndAlso Not order.PredictionOrderMatchesInput Then add("order.cluster-concatenated", "output.prediction", "Core predictions use cluster-concatenated order; the explicit row map is required to align them with original input.", False)
            Return New GeeWorkflowIntent(request, cluster, time, o, order,
                New RegressionValidation(issues, ExpandedParameterCount(p.Data), 0, Array.Empty(Of Integer)()))
        End Function

        ''' <summary>Characterization/planning of the exact existing importer sort. The shipping
        ''' adapter receives an already-sorted geeData and does not call this to sort again.</summary>
        Public Shared Function ImportSortPermutation(cluster() As Object, rows() As Integer, Optional time() As Double = Nothing) As Integer()
            If cluster Is Nothing OrElse rows Is Nothing OrElse cluster.Length <> rows.Length OrElse (time IsNot Nothing AndAlso time.Length <> rows.Length) Then Throw New ArgumentException("gee.import.alignment")
            Dim values(rows.Length - 1, 2) As Object
            For i = 0 To rows.Length - 1
                values(i, 0) = cluster(i) : values(i, 1) = If(time Is Nothing, CDbl(rows(i)), time(i)) : values(i, 2) = i
            Next
            If rows.Length > 0 Then RowArrayUtilities.SortRows(values, "0,A,1,A", 0, rows.Length - 1)
            Return Enumerable.Range(0, rows.Length).Select(Function(i) DirectCast(values(i, 2), Integer)).ToArray()
        End Function
    End Class
End Namespace
