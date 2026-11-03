Option Explicit On
Option Strict On
Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports BESHStatNG.regression

Namespace ExcelCommon
    ''' <summary>Family-specific intent over the accepted request, after Core design expansion.
    ''' Validation remains diagnostic; creating intent never activates Windows strict refusal.</summary>
    Public NotInheritable Class LinearBinomialFitIntent
        Private ReadOnly _groups As Dictionary(Of String, Integer())
        Private ReadOnly _coding As IReadOnlyList(Of String)
        Friend Sub New(request As RegressionRequest, sums As TermSumOfSquaresType,
                       groups As Dictionary(Of String, Integer()), coding As IEnumerable(Of String))
            Me.Request = request
            Me.Validation = RegressionRequestPolicy.Validate(request)
            Me.SumOfSquares = sums
            _groups = CopyGroups(groups)
            _coding = If(coding Is Nothing, Nothing, New List(Of String)(coding).AsReadOnly())
        End Sub
        Public ReadOnly Property Request As RegressionRequest
        Public ReadOnly Property Validation As RegressionValidation
        Public ReadOnly Property SumOfSquares As TermSumOfSquaresType
        Public ReadOnly Property IsLinearModel As Boolean
            Get
                Return Request.Kind = RegressionAnalysisKind.LinearModel
            End Get
        End Property
        Public ReadOnly Property IncludeIntercept As Boolean
            Get
                Return Request.Primary.Effects.IncludeIntercept
            End Get
        End Property
        Public ReadOnly Property RequestedAlpha As Double
            Get
                Return Request.Options.Alpha
            End Get
        End Property
        ''' <summary>Native LM never sets Alpha. This is Core's existing default, not GUI alpha.
        ''' Response composition also records the actual fitted model's Alpha.</summary>
        Public ReadOnly Property EffectiveFitAlpha As Double
            Get
                Return If(IsLinearModel, 0.05, RequestedAlpha)
            End Get
        End Property
        Public ReadOnly Property ResponseScale As String
            Get
                Return If(Not IsLinearModel AndAlso Request.Options.Family.HasValue AndAlso Request.Options.Family.Value = RegressionFamily.Binomial, "probability", "response")
            End Get
        End Property
        Public ReadOnly Property TermGroups As Dictionary(Of String, Integer())
            Get
                Return CopyGroups(_groups)
            End Get
        End Property
        Public ReadOnly Property PredictorCodingFootnotes As IReadOnlyList(Of String)
            Get
                Return _coding
            End Get
        End Property
        Public Function CreateFamily() As Family
            If IsLinearModel Then Throw New InvalidOperationException("LM uses LinearModel, not a GLM family instance.")
            ' Gaussian dispersion is estimated by the existing GLM. Do not inject NB pdAlpha.
            Return FamilyUtils.createFamily(Request.Options.Family.Value.ToString())
        End Function
        Public Function CreateLink() As Link
            If IsLinearModel Then Throw New InvalidOperationException("LM has no selectable GLM link.")
            Dim o = Request.Options
            Return If(o.Link = RegressionLink.Power, LinkUtils.createLink(o.Link.Value.ToString(), o.Power.GetValueOrDefault()), LinkUtils.createLink(o.Link.Value.ToString()))
        End Function
        Private Shared Function CopyGroups(groups As Dictionary(Of String, Integer())) As Dictionary(Of String, Integer())
            If groups Is Nothing Then Return Nothing
            Dim result As New Dictionary(Of String, Integer())(groups.Comparer)
            For Each pair In groups
                result.Add(pair.Key, If(pair.Value Is Nothing, Nothing, DirectCast(pair.Value.Clone(), Integer())))
            Next
            Return result
        End Function
    End Class

    Public NotInheritable Class LinearBinomialRegressionPolicy
        Private Sub New()
        End Sub
        Public Shared Function Supports(request As RegressionRequest) As Boolean
            Return request IsNot Nothing AndAlso (request.Kind = RegressionAnalysisKind.LinearModel OrElse
                (request.Kind = RegressionAnalysisKind.GeneralizedLinearModel AndAlso
                 request.Options.Family.HasValue AndAlso
                 (request.Options.Family.Value = RegressionFamily.Gaussian OrElse request.Options.Family.Value = RegressionFamily.Binomial)))
        End Function
        Public Shared Function Project(request As RegressionRequest,
                                       Optional sums As TermSumOfSquaresType = TermSumOfSquaresType.TypeIII,
                                       Optional groups As Dictionary(Of String, Integer()) = Nothing,
                                       Optional codingFootnotes As IEnumerable(Of String) = Nothing) As LinearBinomialFitIntent
            If Not Supports(request) Then Throw New ArgumentException("regression.linear-binomial.family: Only LM, Gaussian GLM and binomial GLM are projected.")
            Return New LinearBinomialFitIntent(request, sums, groups, codingFootnotes)
        End Function

        ''' <summary>Snapshot real Core responses once, before any native write. No solver, inverse
        ''' link, sorting or replacement of nonfinite/missing cells occurs here.</summary>
        Public Shared Function Compose(intent As LinearBinomialFitIntent, tables As IEnumerable(Of ResultTable),
                                       fitted() As Double, residuals(,) As Object,
                                       Optional converged As Nullable(Of Boolean) = Nothing,
                                       Optional actualFitAlpha As Nullable(Of Double) = Nothing) As LinearBinomialReport
            If intent Is Nothing Then Throw New ArgumentNullException(NameOf(intent))
            Dim matrix(,) As Double = Nothing
            If fitted IsNot Nothing Then
                ReDim matrix(fitted.Length - 1, 0)
                For i = 0 To fitted.Length - 1 : matrix(i, 0) = fitted(i) : Next
            End If
            If residuals IsNot Nothing Then
                If residuals.GetLowerBound(0) <> 0 OrElse residuals.GetLowerBound(1) <> 0 OrElse residuals.GetLength(1) = 0 Then Throw New ArgumentException("regression.output.residuals.axes: Zero-based residual columns are required.")
                ' Core residual matrices do not embed RowIds; their axis follows the exact fit
                ' input order. Identity is carried separately by RegressionRowOutput.
                If residuals.GetLength(1) <> If(intent.IsLinearModel, 6, 7) Then Throw New ArgumentException("regression.output.residuals.axes: Unexpected Core LM/GLM residual axis.")
                For c = 0 To residuals.GetLength(1) - 1
                    If residuals.GetLength(0) = 0 OrElse Not TypeOf residuals(0, c) Is String Then Throw New ArgumentException("regression.output.residuals.headers: Core residual headers are required.")
                Next
            End If
            Dim plan = RegressionReportPolicy.Compose(intent.Request, tables,
                New RegressionRowOutput(intent.Request.Primary.RowIds, matrix, residuals), converged)
            Return New LinearBinomialReport(intent, plan, actualFitAlpha.GetValueOrDefault(intent.EffectiveFitAlpha))
        End Function
    End Class
End Namespace
