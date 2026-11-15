Option Explicit On
Option Strict On

Imports BESHStatNG

Namespace ExcelCommon
    Public NotInheritable Class UdfAgreementReportPreparation38
        Private Sub New()
        End Sub

        Public Shared Function BuildRegressionCoefficients(slope As ConfidenceIntervalResult, intercept As ConfidenceIntervalResult, title As String) As ResultTable
                Dim t As New ResultTable
                t.AddTitle(title)
                t.AddHeaderLeftRow({"Slope", "Intercept"})
                t.AddHeaderTopRow({"Estimate", slope.CIlabel, "Lower", "Upper"})
                t.SetBody(New Object(,) {
                    {slope.Estimate, slope.strConfidenceInterval(CIformat.LL_to_UL), slope.LowerLimit, slope.UpperLimit},
                    {intercept.Estimate, intercept.strConfidenceInterval(CIformat.LL_to_UL), intercept.LowerLimit, intercept.UpperLimit}
                })
                Return t
        End Function

        Public Shared Function BuildBlandAltmanStatistics(res As Agreement.BlandAltmanResult) As ResultTable
                Dim t As New ResultTable
                t.AddTitle("Bland–Altman Statistics")
                t.AddHeaderLeftRow({"Bias", "Lower LoA", "Upper LoA", "SD(diff)", "Repeatability coefficient"})
                t.AddHeaderTopRow({"Estimate", res.BiasCI.CIlabel, "Lower", "Upper"})
                t.SetBody(New Object(,) {
                    {res.BiasCI.Estimate, res.BiasCI.strConfidenceInterval(CIformat.LL_to_UL), res.BiasCI.LowerLimit, res.BiasCI.UpperLimit},
                    {res.LowerLoACI.Estimate, res.LowerLoACI.strConfidenceInterval(CIformat.LL_to_UL), res.LowerLoACI.LowerLimit, res.LowerLoACI.UpperLimit},
                    {res.UpperLoACI.Estimate, res.UpperLoACI.strConfidenceInterval(CIformat.LL_to_UL), res.UpperLoACI.LowerLimit, res.UpperLoACI.UpperLimit},
                    {res.SdDifference, "", "", ""},
                    {res.RepeatabilityCoefficient, "", "", ""}
                })
                Return t
        End Function

        Public Shared Function BuildRepeatability(rc As ConfidenceIntervalResult, title As String) As ResultTable
                Dim t As New ResultTable
                t.SetBody(New Object(,) {
                        {"Repeatability coefficient", rc.Estimate},
                        {rc.CIlabel, rc.strConfidenceInterval(CIformat.LL_to_UL)},
                        {"SEM (standard error of measurement)", rc.StdErr}
                    })
                t.AddHeaderTopRow({title, ""})
                Return t
        End Function

        ''' <summary>Fitted typed model; host option conversion and its ordering remain at the caller.</summary>
        Public Shared Function FitDeming(first() As Double, second() As Double, firstName As String, secondName As String,
                                         options As Agreement.DemingOptions, seed As Integer, configureLegacyFields As Boolean) As Agreement.WeightedDemingRegression
            Dim model As New Agreement.WeightedDemingRegression(first, second, firstName, secondName, options)
            If configureLegacyFields Then
                model.alpha = options.Alpha
                If options.VarianceModel = Agreement.DemingVarianceModel.ConstantLambda Then model.Lambda = options.Lambda
            End If
            model.Fit(Nothing, seed)
            Return model
        End Function

        ''' <summary>Deferred seed readers preserve constructor-before-reader exception precedence.</summary>
        Public Shared Function FitBlandAltman(first() As Double, second() As Double, firstName As String, secondName As String,
                                             options As Agreement.BlandAltmanOptions, readSeed As Func(Of Integer),
                                             ByRef result As Agreement.BlandAltmanResult) As Agreement.BlandAltmanAgreement
            Dim model As New Agreement.BlandAltmanAgreement(first, second, firstName, secondName, options)
            result = model.Fit(readSeed())
            Return model
        End Function

        Public Shared Function FitLinConcordance(first() As Double, second() As Double, firstName As String, secondName As String,
                                                 options As Agreement.LinConcordanceOptions, readSeed As Func(Of Integer),
                                                 ByRef result As Agreement.LinConcordanceResult) As Agreement.LinConcordanceCorrelation
            Dim model As New Agreement.LinConcordanceCorrelation(first, second, firstName, secondName, options)
            result = model.Fit(Nothing, readSeed())
            Return model
        End Function

        Public Shared Function FitKappa(first() As Object, second() As Object, firstName As String, secondName As String,
                                       options As Agreement.KappaOptions, readSeed As Func(Of Integer),
                                       ByRef result As Agreement.KappaResult) As Agreement.WeightedKappaAgreement
            Dim model As New Agreement.WeightedKappaAgreement(first, second, firstName, secondName, options)
            result = model.Fit(Nothing, readSeed())
            Return model
        End Function
    End Class
End Namespace
