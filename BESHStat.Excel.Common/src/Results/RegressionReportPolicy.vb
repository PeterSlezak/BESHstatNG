Option Explicit On
Option Strict On
Imports System
Imports System.Collections.Generic
Imports System.Linq

Namespace ExcelCommon
    Public Enum RegressionConvergence
        Unknown
        Converged
        NotConverged
        NotIterative
    End Enum

    ''' <summary>Fitted columns and legacy residual payload tied to exact source identities. Header rows
    ''' are explicit, never mistaken for observations. NaN/missing cells are retained, not imputed.</summary>
    Public NotInheritable Class RegressionRowOutput
        Private ReadOnly _rows As Integer()
        Private ReadOnly _fitted As Double(,)
        Private ReadOnly _residuals As Object(,)
        Private ReadOnly _categories As Integer()
        Public Sub New(rows() As Integer, fitted(,) As Double, residuals(,) As Object,
                       Optional residualHeaderRows As Integer = 1, Optional categories() As Integer = Nothing)
            If rows Is Nothing OrElse rows.Distinct().Count() <> rows.Length Then Throw New ArgumentException("regression.output.rowIds: Unique source identities are required.")
            If fitted IsNot Nothing AndAlso (fitted.GetLowerBound(0) <> 0 OrElse fitted.GetLowerBound(1) <> 0) Then Throw New ArgumentException("regression.output.fitted.bounds: Fitted arrays must be zero based.")
            If fitted IsNot Nothing AndAlso fitted.GetLength(0) <> rows.Length Then Throw New ArgumentException("regression.output.fitted.alignment: Fitted rows must match source RowIds.")
            If residualHeaderRows < 0 OrElse (residuals IsNot Nothing AndAlso residuals.GetLength(0) <> rows.Length + residualHeaderRows) Then Throw New ArgumentException("regression.output.residuals.alignment: Residual rows and header metadata must match source RowIds.")
            If categories IsNot Nothing AndAlso (categories.Distinct().Count() <> categories.Length OrElse (fitted IsNot Nothing AndAlso fitted.GetLength(1) <> categories.Length)) Then Throw New ArgumentException("regression.output.categories: Category identities must match fitted columns.")
            If residuals IsNot Nothing Then
                ' Reuse the established immutable scalar whitelist; do not retain host error objects.
                Dim checked = New OutputTableArtifact("residuals", New ResultTableOutputModel(residuals, headerTopRows:=residualHeaderRows))
            End If
            _rows = DirectCast(rows.Clone(), Integer())
            _fitted = If(fitted Is Nothing, Nothing, DirectCast(fitted.Clone(), Double(,)))
            _residuals = If(residuals Is Nothing, Nothing, DirectCast(residuals.Clone(), Object(,)))
            _categories = If(categories Is Nothing, Nothing, DirectCast(categories.Clone(), Integer()))
            Me.ResidualHeaderRows = residualHeaderRows
        End Sub
        Public ReadOnly Property ResidualHeaderRows As Integer
        Public ReadOnly Property RowIds As Integer()
            Get
                Return DirectCast(_rows.Clone(), Integer())
            End Get
        End Property
        Public ReadOnly Property Fitted As Double(,)
            Get
                Return If(_fitted Is Nothing, Nothing, DirectCast(_fitted.Clone(), Double(,)))
            End Get
        End Property
        Public ReadOnly Property Residuals As Object(,)
            Get
                Return If(_residuals Is Nothing, Nothing, DirectCast(_residuals.Clone(), Object(,)))
            End Get
        End Property
        Public ReadOnly Property Categories As Integer()
            Get
                Return If(_categories Is Nothing, Nothing, DirectCast(_categories.Clone(), Integer()))
            End Get
        End Property
    End Class

    Public NotInheritable Class RegressionReportPlan
        Public Sub New(request As RegressionRequest, document As AnalysisResultDocument, rows As RegressionRowOutput,
                       convergence As RegressionConvergence, diagnostics As IEnumerable(Of RegressionDiagnostic))
            Me.Request = request : Me.Document = document : Me.Rows = rows : Me.Convergence = convergence
            Me.Diagnostics = New List(Of RegressionDiagnostic)(diagnostics).AsReadOnly()
        End Sub
        Public ReadOnly Property Request As RegressionRequest
        Public ReadOnly Property Document As AnalysisResultDocument
        Public ReadOnly Property Rows As RegressionRowOutput
        Public ReadOnly Property Convergence As RegressionConvergence
        Public ReadOnly Property Diagnostics As IReadOnlyList(Of RegressionDiagnostic)
        ''' <summary>Versioned metadata snapshot reuses the established document snapshot.
        ''' Cell payloads remain in Document/Rows; this does not replace output-cell serialization.</summary>
        Public Function ToContractSnapshot() As RegressionReportSnapshot
            Dim o = Request.Options
            Return New RegressionReportSnapshot With {
                .SchemaVersion = 1, .Kind = Request.Kind.ToString(), .ResponseKey = Request.ResponseKey,
                .PrimaryEffects = RegressionEffectSerialization.Serialize(Request.Primary.Effects),
                .LogisticEffects = If(Request.Logistic Is Nothing, Nothing, RegressionEffectSerialization.Serialize(Request.Logistic.Effects)),
                .RowIds = Rows.RowIds, .Categories = Rows.Categories,
                .OffsetKey = If(Request.Offset Is Nothing, Nothing, Request.Offset.Key),
                .WeightsKey = If(Request.Weights Is Nothing, Nothing, Request.Weights.Key),
                .Family = If(o.Family.HasValue, o.Family.Value.ToString(), Nothing),
                .Link = If(o.Link.HasValue, o.Link.Value.ToString(), Nothing),
                .Alpha = o.Alpha, .Epsilon = o.Epsilon, .MaximumIterations = o.MaximumIterations,
                .EmIterations = o.EmIterations, .Covariance = o.Covariance, .Residuals = o.Residuals,
                .IterationDetails = o.IterationDetails, .Reference = o.Reference.ToString(),
                .Power = o.Power, .Dispersion = o.Dispersion, .Classification = o.Classification,
                .Threshold = o.Threshold, .ThresholdTable = o.ThresholdTable, .Calibration = o.Calibration,
                .CalibrationBins = o.CalibrationBins, .Brier = o.Brier,
                .PValueDecimalPlaces = o.PValuePresentation.DecimalPlaces,
                .PValueSmallValueDisplay = o.PValuePresentation.SmallValueDisplay.ToString(),
                .PValueUseUpperBound = o.PValuePresentation.UseUpperBound,
                .Convergence = Convergence.ToString(), .DiagnosticCodes = Diagnostics.Select(Function(d) d.Code).ToArray(),
                .Document = Document.ToContractSnapshot()}
        End Function
    End Class

    Public NotInheritable Class RegressionReportSnapshot
        Public Property SchemaVersion As Integer
        Public Property Kind As String
        Public Property ResponseKey As String
        Public Property PrimaryEffects As String
        Public Property LogisticEffects As String
        Public Property RowIds As Integer()
        Public Property Categories As Integer()
        Public Property OffsetKey As String
        Public Property WeightsKey As String
        Public Property Family As String
        Public Property Link As String
        Public Property Alpha As Double
        Public Property Epsilon As Double
        Public Property MaximumIterations As Integer
        Public Property EmIterations As Integer
        Public Property Covariance As Boolean
        Public Property Residuals As Boolean
        Public Property IterationDetails As Boolean
        Public Property Reference As String
        Public Property Power As Nullable(Of Double)
        Public Property Dispersion As Nullable(Of Double)
        Public Property Classification As Boolean
        Public Property Threshold As Double
        Public Property ThresholdTable As Boolean
        Public Property Calibration As Boolean
        Public Property CalibrationBins As Integer
        Public Property Brier As Boolean
        Public Property PValueDecimalPlaces As Integer
        Public Property PValueSmallValueDisplay As String
        Public Property PValueUseUpperBound As Boolean
        Public Property Convergence As String
        Public Property DiagnosticCodes As String()
        Public Property Document As AnalysisResultDocumentSnapshot
    End Class

    ''' <summary>Composition only: Core owns every number and section. Document success describes
    ''' composition, not optimizer convergence. No inferred success from missing metadata.</summary>
    Public NotInheritable Class RegressionReportPolicy
        Private Sub New()
        End Sub
        Public Shared Function Compose(request As RegressionRequest, tables As IEnumerable(Of ResultTable),
                                       rows As RegressionRowOutput, Optional converged As Nullable(Of Boolean) = Nothing) As RegressionReportPlan
            If request Is Nothing OrElse request.Primary Is Nothing Then Throw New ArgumentNullException(NameOf(request))
            If tables Is Nothing Then Throw New ArgumentNullException(NameOf(tables))
            If rows Is Nothing OrElse request.Primary.RowIds Is Nothing OrElse Not rows.RowIds.SequenceEqual(request.Primary.RowIds) Then Throw New ArgumentException("regression.output.rowIds: Fitted order differs from source identity order.")
            Dim models As New List(Of ResultTableOutputModel)
            For Each table In tables
                If table Is Nothing Then Throw New ArgumentException("regression.output.table: Null result section.")
                Dim model = New OutputTableArtifact("table-" & models.Count.ToString(Globalization.CultureInfo.InvariantCulture), table.ToOutputModel())
                models.Add(model.Model)
            Next
            Dim issues As New List(Of RegressionDiagnostic)
            Dim state = RegressionConvergence.Unknown
            If request.Kind = RegressionAnalysisKind.LinearModel Then
                state = RegressionConvergence.NotIterative
            Else
                ' Ordinal/multinomial expose convergence only in their existing diagnostic table.
                ' Read its exact Core label; do not derive convergence from the existence of results.
                If Not converged.HasValue Then
                    For Each m In models
                        For r = 0 To m.RowCount - 1
                            For c = 0 To m.ColumnCount - 2
                                If TypeOf m.Values(r, c) Is String AndAlso CStr(m.Values(r, c)) = "Converged?" Then
                                    Dim flag As Boolean
                                    If Boolean.TryParse(Convert.ToString(m.Values(r, c + 1), Globalization.CultureInfo.InvariantCulture), flag) Then converged = flag
                                End If
                            Next
                        Next
                    Next
                End If
                If converged.HasValue Then state = If(converged.Value, RegressionConvergence.Converged, RegressionConvergence.NotConverged)
                issues.Add(New RegressionDiagnostic(If(state = RegressionConvergence.Converged, "regression.fit.converged", If(state = RegressionConvergence.NotConverged, "regression.fit.not-converged", "regression.fit.convergence-unknown")),
                    "fit.convergence", If(state = RegressionConvergence.Converged, "Core reported convergence.", If(state = RegressionConvergence.NotConverged, "Core did not converge; results may be misleading.", "Core convergence metadata was not supplied.")), blocking:=False))
            End If
            If models.Count = 0 Then issues.Add(New RegressionDiagnostic("regression.output.no-tables", "output.tables", "No result tables were supplied.", blocking:=False))
            If request.Options.Residuals AndAlso rows.Residuals Is Nothing Then issues.Add(New RegressionDiagnostic("regression.output.no-residuals", "output.residuals", "Residual output was requested but Core supplied none.", blocking:=False))
            Dim warnings = issues.Where(Function(d) d.Code <> "regression.fit.converged").Select(Function(d) New AnalysisWarning(d.Code, d.Message, d.FieldPath))
            Dim document As New AnalysisResultDocument("regression." & request.Kind.ToString(), models, Array.Empty(Of KeyValuePair(Of String, Double))(), Nothing, warnings)
            Return New RegressionReportPlan(request, document, rows, state, issues)
        End Function
    End Class
End Namespace
