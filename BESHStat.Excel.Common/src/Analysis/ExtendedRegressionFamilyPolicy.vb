Option Explicit On
Option Strict On
Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports BESHStatNG.regression

Namespace ExcelCommon
    Public Enum RegressionDispersionMode
        NotApplicable
        EstimatedNegativeBinomial2
    End Enum

    ''' <summary>Four-family application intent over the accepted immutable request. ZIP owns
    ''' two independent components. Core expansion, fitting and category coding stay authoritative.</summary>
    Public NotInheritable Class ExtendedRegressionIntent
        Friend Sub New(request As RegressionRequest)
            Me.Request = request : Me.Validation = RegressionRequestPolicy.Validate(request)
        End Sub
        Public ReadOnly Property Request As RegressionRequest
        Public ReadOnly Property Validation As RegressionValidation
        Public ReadOnly Property Primary As RegressionComponent
            Get
                Return Request.Primary
            End Get
        End Property
        Public ReadOnly Property Logistic As RegressionComponent
            Get
                Return Request.Logistic
            End Get
        End Property
        Public ReadOnly Property Options As RegressionOptions
            Get
                Return Request.Options
            End Get
        End Property
        Public ReadOnly Property PrimaryParameterCount As Integer
            Get
                Return Validation.PrimaryParameterCount
            End Get
        End Property
        Public ReadOnly Property LogisticParameterCount As Integer
            Get
                Return Validation.LogisticParameterCount
            End Get
        End Property
        Public ReadOnly Property PrimaryIntercept As Integer
            Get
                Return If(Primary.Effects.IncludeIntercept, 1, 0)
            End Get
        End Property
        Public ReadOnly Property LogisticIntercept As Integer
            Get
                Return If(Logistic IsNot Nothing AndAlso Logistic.Effects.IncludeIntercept, 1, 0)
            End Get
        End Property
        Public ReadOnly Property DispersionMode As RegressionDispersionMode
            Get
                Return If(Request.Kind = RegressionAnalysisKind.NegativeBinomial2, RegressionDispersionMode.EstimatedNegativeBinomial2, RegressionDispersionMode.NotApplicable)
            End Get
        End Property
        Public ReadOnly Property Reference As ReferenceCategory
            Get
                Return Options.Reference
            End Get
        End Property
        Public Function CreateLink() As Link
            If Request.Kind <> RegressionAnalysisKind.NegativeBinomial2 Then Throw New InvalidOperationException("Only the NB2 route selects a GLM link.")
            Dim o = Options
            Return If(o.Link = RegressionLink.Power, LinkUtils.createLink(o.Link.Value.ToString(), o.Power.GetValueOrDefault()), LinkUtils.createLink(o.Link.Value.ToString()))
        End Function
    End Class

    Public NotInheritable Class ExtendedRegressionFamilyPolicy
        Private Sub New()
        End Sub
        Public Shared Function Project(request As RegressionRequest) As ExtendedRegressionIntent
            If request Is Nothing OrElse (request.Kind <> RegressionAnalysisKind.NegativeBinomial2 AndAlso request.Kind <> RegressionAnalysisKind.ZeroInflatedPoisson AndAlso request.Kind <> RegressionAnalysisKind.Ordinal AndAlso request.Kind <> RegressionAnalysisKind.Multinomial) Then Throw New ArgumentException("regression.extended.family: Expected NB2, ZIP, ordinal or multinomial request.")
            ' Observation only in Windows: do not turn these diagnostics into a new refusal.
            Return New ExtendedRegressionIntent(request)
        End Function

        Public Shared Function Compose(intent As ExtendedRegressionIntent, tables As IEnumerable(Of ResultTable), rows As RegressionRowOutput,
                                       Optional converged As Nullable(Of Boolean) = Nothing,
                                       Optional estimatedDispersion As Nullable(Of Double) = Nothing) As ExtendedRegressionReport
            If intent Is Nothing Then Throw New ArgumentNullException(NameOf(intent))
            If rows Is Nothing Then Throw New ArgumentNullException(NameOf(rows))
            Dim categorical = intent.Request.Kind = RegressionAnalysisKind.Ordinal OrElse intent.Request.Kind = RegressionAnalysisKind.Multinomial
            Dim fitted = rows.Fitted, categories = rows.Categories, residual = rows.Residuals
            If categorical Then
                If fitted IsNot Nothing AndAlso (categories Is Nothing OrElse fitted.GetLength(1) <> categories.Length) Then Throw New ArgumentException("regression.output.categories: Actual Core probability axes are required.")
                If categories IsNot Nothing AndAlso intent.Validation.IsValid AndAlso Not categories.OrderBy(Function(c) c).SequenceEqual(intent.Validation.ObservedClasses.OrderBy(Function(c) c)) Then Throw New ArgumentException("regression.output.categories: Core class identities differ from the observed response.")
            ElseIf fitted IsNot Nothing AndAlso fitted.GetLength(1) <> 1 Then
                Throw New ArgumentException("regression.output.fitted.axis: Count response requires one fitted column.")
            End If
            If residual IsNot Nothing Then
                If residual.GetLowerBound(0) <> 0 OrElse residual.GetLowerBound(1) <> 0 OrElse rows.ResidualHeaderRows <> 1 Then Throw New ArgumentException("regression.output.residuals.axes: Core residual arrays require zero bounds and one header row.")
                Dim width As Integer = If(categorical, If(categories Is Nothing, 0, 5 * categories.Length + 3), If(intent.Request.Kind = RegressionAnalysisKind.ZeroInflatedPoisson, 2, 7))
                If residual.GetLength(1) <> width Then Throw New ArgumentException("regression.output.residuals.axes: Residual columns differ from the actual family/class axis.")
                For c = 0 To width - 1
                    If Not TypeOf residual(0, c) Is String Then Throw New ArgumentException("regression.output.residuals.headers: Explicit Core headers are required.")
                Next
            End If
            Dim report = RegressionReportPolicy.Compose(intent.Request, tables, rows, converged)
            Return New ExtendedRegressionReport(intent, report, estimatedDispersion)
        End Function
    End Class
End Namespace
