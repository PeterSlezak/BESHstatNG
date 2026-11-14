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
    End Class
End Namespace
