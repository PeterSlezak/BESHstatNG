Option Explicit On
Imports Microsoft.Office.Interop.Excel

''' <summary>
''' Likelihood‑model result container providing coefficient estimates, standard errors,
''' Z‑statistics, T‑statistics, p‑values, confidence intervals, odds ratios, and
''' formatted output tables for regression models.
''' </summary>
''' <remarks>
''' 
''' ============================
'''  MATHEMATICAL APPENDIX
''' ============================
''' 
''' <para id="LM_Estimates"><b>1. Parameter Estimates</b><br/>
''' The estimated coefficient vector is β̂ = (β̂₀, β̂₁, …, β̂ₖ). These values are supplied
''' externally and stored in <c>Coeffs_est()</c>.
''' </para>
''' 
''' <para id="LM_SE_Z"><b>2. Standard Errors (Normal‑based)</b><br/>
''' For large‑sample likelihood models, standard errors are asymptotically normal:
''' SE(β̂ᵢ) = sqrt(Var(β̂ᵢ)). These are provided in <c>Coeffs_SEs()</c>.
''' </para>
''' 
''' <para id="LM_SE_T"><b>3. Standard Errors (T‑based)</b><br/>
''' For linear‑model or small‑sample contexts, T‑based SEs are used:
''' SEₜ(β̂ᵢ) = sqrt(σ̂² (X'X)⁻¹ᵢᵢ). These are stored in <c>Coeffs_SEsT()</c>.
''' </para>
''' 
''' <para id="LM_Zstat"><b>4. Z‑Statistics</b><br/>
''' Zᵢ = β̂ᵢ / SE(β̂ᵢ). Under H₀: βᵢ = 0, Zᵢ ~ N(0,1). Implemented in <c>Coeffs_Zstat</c>.
''' </para>
''' 
''' <para id="LM_Tstat"><b>5. T‑Statistics</b><br/>
''' Tᵢ = β̂ᵢ / SEₜ(β̂ᵢ). Under H₀: βᵢ = 0, Tᵢ ~ t(df), where df = n − p. Implemented in <c>Coeffs_Tstat</c>.
''' </para>
''' 
''' <para id="LM_PvaluesZ"><b>6. Two‑Sided P‑values (Z)</b><br/>
''' pᵢ = 2(1 − Φ(|Zᵢ|)), where Φ is the standard normal CDF. Implemented in <c>Coeffs_PvaluesZ</c>.
''' </para>
''' 
''' <para id="LM_PvaluesT"><b>7. Two‑Sided P‑values (T)</b><br/>
''' pᵢ = 2(1 − Fₜ(|Tᵢ|; df)), where Fₜ is the t‑distribution CDF. Implemented in <c>Coeffs_PvaluesT</c>.
''' </para>
''' 
''' <para id="LM_CI_Z"><b>8. 95% Confidence Intervals (Normal‑based)</b><br/>
''' CIᵢ = β̂ᵢ ± z_{1−α/2}·SE(β̂ᵢ), where z_{1−α/2} = Φ⁻¹(1 − α/2). Implemented in
''' <c>Coeffs_95CIlowZ</c> and <c>Coeffs_95CIhighZ</c>.
''' </para>
''' 
''' <para id="LM_CI_T"><b>9. 95% Confidence Intervals (T‑based)</b><br/>
''' CIᵢ = β̂ᵢ ± t_{1−α/2,df}·SEₜ(β̂ᵢ). Implemented in <c>Coeffs_95CIlowT</c> and <c>Coeffs_95CIhighT</c>.
''' </para>
''' 
''' <para id="LM_OR"><b>10. Odds Ratios</b><br/>
''' For logistic‑type models, ORᵢ = exp(β̂ᵢ). Wald χ² = Zᵢ². Confidence limits:
''' OR_L = exp(CI_low), OR_U = exp(CI_high). Implemented in <c>ParameterOdds</c>.
''' </para>
''' 
''' <para id="LM_Tables"><b>11. Output Tables</b><br/>
''' The class produces formatted <c>ResultTable</c> objects for:
''' <list type="bullet">
'''   <item><description>Z‑based coefficient tables (<c>CoeffsZ_toPrint</c>)</description></item>
'''   <item><description>T‑based coefficient tables (<c>CoeffsT_toPrint</c>)</description></item>
'''   <item><description>Odds‑ratio tables (<c>OR_toPrint</c>)</description></item>
'''   <item><description>Model‑diagnostic tables (<c>getModelDiagnasticTable_toPrint</c>)</description></item>
''' </list>
''' </para>
''' 
''' ============================
'''  DEVELOPER MANUAL
''' ============================
''' 
''' <para id="Dev_Usage"><b>12. Usage Notes</b><br/>
''' Populate <c>Coeffs_est</c>, <c>Coeffs_SEs</c>, <c>Coeffs_SEsT</c>, <c>varNames</c>, and <c>n</c>.
''' Then call the read‑only properties to obtain statistics or formatted tables.
''' </para>
''' 
''' <para id="Dev_Intercept"><b>13. Intercept Handling</b><br/>
''' If <c>bIntercept = True</c>, the first coefficient is treated as the intercept and excluded
''' from odds‑ratio computation.
''' </para>
''' 
''' <para id="Dev_Overflow"><b>14. Overflow Protection</b><br/>
''' If β̂ or CI bounds exceed 600, OR values are set to "Inf" to avoid floating‑point overflow.
''' </para>
''' 
''' <para id="Dev_Tables"><b>15. Table Construction</b><br/>
''' All tables are built as 2D arrays and wrapped in <c>ResultTable</c> objects with
''' header rows and optional left‑side variable labels.
''' </para>
''' 
''' </remarks>

Public Class LMresult
    'Likelihood model results
    Public Shared CoeffsZ_table_labels() As String = {"Coefficient", "Std. Error", "Z", "P-value", "95% CI Lower", "95% CI Upper"}
    Public Shared CoeffsT_table_labels() As String = {"Coefficient", "Std. Error", "T", "P-value", "95% CI Lower", "95% CI Upper"}
    Public Shared OR_table_labels() As String = {"OR", "Wald Chi2", "P-value", "95% CI Lower", "95% CI Upper"}

    Public alpha As Double = 0.05
    Public Coeffs_est() As Double 'Parameter estimates must be provided by procedure
    Public Coeffs_SEs() As Double 'Normal distribution SEs must be provided by procedure
    Public Coeffs_SEsT() As Double 'T distribution SEs must be provided by procedure
    Public n As Double 'sample size
    'Optional: set residual degrees of freedom explicitly (recommended for linear models).
    'If not set, df is derived as n - p where p = number of coefficients.
    Public dfResid As Double = Double.NaN
    Public bIntercept As Boolean = True
    Public varNames() As String
    Public ModelTableVals(,) As Object
    Public ModelTableLabels() As String
    Public ModelTableTopRow() As String = Nothing

    ''' <summary>
    ''' Gets the degrees of freedom used for t-based inference on the estimated coefficients.
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' This property is intended for computations that rely on the Student's t distribution,
    ''' such as coefficient p-values and confidence intervals.
    ''' </para>
    ''' <para>
    ''' Resolution order:
    ''' <list type="number">
    '''   <item>
    '''     <description>
    '''     If <see cref="dfResid"/> is set to a positive value, it is used directly.
    '''     This allows the caller (e.g., a fitted model class) to explicitly provide the residual
    '''     degrees of freedom (typically <c>n - p</c>).
    '''     </description>
    '''   </item>
    '''   <item>
    '''     <description>
    '''     Otherwise, the residual degrees of freedom is computed as <c>n - p</c>, where
    '''     <c>p</c> is the number of estimated coefficients (<c>UBound(Coeffs_est) + 1</c>).
    '''     </description>
    '''   </item>
    ''' </list>
    ''' </para>
    ''' <para>
    ''' If <see cref="Coeffs_est"/> is <c>Nothing</c>, the degrees of freedom cannot be determined
    ''' and <see cref="Double.NaN"/> is returned.
    ''' </para>
    ''' </remarks>
    ''' <returns>
    ''' A positive residual degrees of freedom value used for t-based inference, or <see cref="Double.NaN"/>
    ''' if it cannot be determined.
    ''' </returns>
    Private ReadOnly Property DF_T As Double
        Get
            If Not Double.IsNaN(Me.dfResid) AndAlso Me.dfResid > 0 Then Return Me.dfResid
            If Me.Coeffs_est Is Nothing Then Return Double.NaN
            Dim p As Integer = UBound(Me.Coeffs_est) + 1
            Return Me.n - p
        End Get
    End Property


    ''' <summary>
    ''' Computes Z‑statistics β̂ᵢ / SE(β̂ᵢ).
    ''' </summary>
    ''' <remarks>
    ''' See <a href="#LM_Zstat">Mathematical Appendix §4</a>.
    ''' </remarks>
    ReadOnly Property Coeffs_Zstat() As Double()
        Get
            Dim out(UBound(Me.Coeffs_est)) As Double
            For i = 0 To UBound(Me.Coeffs_est)
                out(i) = Me.Coeffs_est(i) / Me.Coeffs_SEs(i)
            Next
            Return out
        End Get
    End Property

    ''' <summary>
    ''' Computes T‑statistics β̂ᵢ / SEₜ(β̂ᵢ) using T‑based standard errors.
    ''' </summary>
    ''' <remarks>
    ''' See <a href="#LM_Tstat">Mathematical Appendix §5</a>.
    ''' </remarks>

    ReadOnly Property Coeffs_Tstat() As Double()
        Get
            Dim out(UBound(Me.Coeffs_est)) As Double
            For i = 0 To UBound(Me.Coeffs_est)
                out(i) = Me.Coeffs_est(i) / Me.Coeffs_SEsT(i)
            Next
            Return out
        End Get
    End Property

    ''' <summary>
    ''' Computes two‑sided p‑values for Z‑statistics using the normal distribution.
    ''' </summary>
    ''' <remarks>
    ''' See <a href="#LM_PvaluesZ">Mathematical Appendix §6</a>.
    ''' </remarks>
    ReadOnly Property Coeffs_PvaluesZ() As Double()
        Get
            Dim out(UBound(Me.Coeffs_est)) As Double
            For i = 0 To UBound(Me.Coeffs_est)
                out(i) = (1.0 - distributions.PNorm(Math.Abs(Me.Coeffs_Zstat(i)))) * 2.0
            Next
            Return out
        End Get
    End Property

    ''' <summary>
    ''' Computes two‑sided p‑values for T‑statistics using the t‑distribution.
    ''' </summary>
    ''' <remarks>
    ''' See <a href="#LM_PvaluesT">Mathematical Appendix §7</a>.
    ''' </remarks>
    ReadOnly Property Coeffs_PvaluesT() As Double()
        Get
            Dim out(UBound(Me.Coeffs_est)) As Double
            For i = 0 To UBound(Me.Coeffs_est)
                out(i) = distributions.T_2T(Math.Abs(Coeffs_Tstat(i)), Me.DF_T)
            Next
            Return out
        End Get
    End Property

    ''' <summary>
    ''' Computes lower bounds of 95% confidence intervals using normal‑based SEs.
    ''' </summary>
    ''' <remarks>
    ''' See <a href="#LM_CI_Z">Mathematical Appendix §8</a>.
    ''' </remarks>

    ReadOnly Property Coeffs_95CIlowZ() As Double()
        Get
            Dim out(UBound(Me.Coeffs_est)) As Double
            Dim tmp1 As Double = distributions.NormSInv(1.0 - Me.alpha / 2.0)
            For i = 0 To UBound(Me.Coeffs_est)
                out(i) = Me.Coeffs_est(i) - Me.Coeffs_SEs(i) * tmp1
            Next
            Return out
        End Get
    End Property

    ''' <summary>
    ''' Computes lower bounds of 95% confidence intervals using T‑based SEs.
    ''' </summary>
    ''' <remarks>
    ''' See <a href="#LM_CI_T">Mathematical Appendix §9</a>.
    ''' </remarks>

    ReadOnly Property Coeffs_95CIlowT() As Double()
        Get
            Dim out(UBound(Me.Coeffs_est)) As Double
            For i = 0 To UBound(Me.Coeffs_est)
                out(i) = Me.Coeffs_est(i) - Me.Coeffs_SEsT(i) * distributions.T_Inv_2T(Me.alpha, Me.DF_T)
            Next
            Return out
        End Get
    End Property

    ''' <summary>
    ''' Computes upper bounds of 95% confidence intervals using normal‑based SEs.
    ''' </summary>
    ''' <remarks>
    ''' See <a href="#LM_CI_Z">Mathematical Appendix §8</a>.
    ''' </remarks>

    ReadOnly Property Coeffs_95CIhighZ() As Double()
        Get
            Dim out(UBound(Me.Coeffs_est)) As Double
            Dim tmp1 As Double = distributions.NormSInv(1.0 - Me.alpha / 2.0)
            For i = 0 To UBound(Me.Coeffs_est)
                out(i) = Me.Coeffs_est(i) + Me.Coeffs_SEs(i) * tmp1
            Next
            Return out
        End Get
    End Property

    ''' <summary>
    ''' Computes upper bounds of 95% confidence intervals using T‑based SEs.
    ''' </summary>
    ''' <remarks>
    ''' See <a href="#LM_CI_T">Mathematical Appendix §9</a>.
    ''' </remarks>

    ReadOnly Property Coeffs_95CIhighT() As Double()
        Get
            Dim out(UBound(Me.Coeffs_est)) As Double
            For i = 0 To UBound(Me.Coeffs_est)
                out(i) = Me.Coeffs_est(i) + Me.Coeffs_SEsT(i) * distributions.T_Inv_2T(Me.alpha, Me.DF_T)
            Next
            Return out
        End Get
    End Property

    ''' <summary>
    ''' Returns a matrix of Z‑based coefficient statistics:
    ''' estimate, SE, Z, p‑value, CI‑lower, CI‑upper.
    ''' </summary>
    ''' <remarks>
    ''' See <a href="#LM_Tables">Mathematical Appendix §11</a>.
    ''' </remarks>

    ReadOnly Property CoeffsZ_vals() As Double(,)
        Get
            Dim out(UBound(Me.Coeffs_est), 5) As Double
            For i = 0 To UBound(Me.Coeffs_est)
                out(i, 0) = Me.Coeffs_est(i)
                out(i, 1) = Me.Coeffs_SEs(i)
                out(i, 2) = Me.Coeffs_Zstat(i)
                out(i, 3) = Me.Coeffs_PvaluesZ(i)
                out(i, 4) = Me.Coeffs_95CIlowZ(i)
                out(i, 5) = Me.Coeffs_95CIhighZ(i)
            Next
            Return out
        End Get
    End Property

    ''' <summary>
    ''' Returns a formatted <c>ResultTable</c> containing Z‑based coefficient statistics.
    ''' </summary>
    ''' <remarks>
    ''' See <a href="#LM_Tables">Mathematical Appendix §11</a>.
    ''' </remarks>

    ReadOnly Property CoeffsZ_toPrint() As ResultTable
        Get
            Dim out(UBound(Me.Coeffs_est), 5) As Object, rowLbls() As String
            Dim resTab As ResultTable = New ResultTable
            For i = 0 To UBound(Me.Coeffs_est)
                out(i, 0) = Me.Coeffs_est(i)
                out(i, 1) = Me.Coeffs_SEs(i)
                out(i, 2) = Me.Coeffs_Zstat(i)
                out(i, 3) = Me.Coeffs_PvaluesZ(i)
                out(i, 4) = Me.Coeffs_95CIlowZ(i)
                out(i, 5) = Me.Coeffs_95CIhighZ(i)
            Next
            resTab.SetBody(out)
            resTab.AddHeaderTopRow(LMresult.CoeffsZ_table_labels)
            If Me.bIntercept Then
                rowLbls = Matrix.ConcatArrays({"Variable", "Intercept"}, Me.varNames)
            Else
                rowLbls = Me.varNames
            End If
            resTab.AddHeaderLeftRow(rowLbls)
            Return resTab
        End Get
    End Property

    ''' <summary>
    ''' Returns a formatted <c>ResultTable</c> containing T‑based coefficient statistics.
    ''' </summary>
    ''' <remarks>
    ''' See <a href="#LM_Tables">Mathematical Appendix §11</a>.
    ''' </remarks>

    ReadOnly Property CoeffsT_toPrint() As ResultTable
        Get
            Dim out(UBound(Me.Coeffs_est), 5) As Object, rowLbls() As String
            Dim resTab As ResultTable = New ResultTable

            For i = 0 To UBound(Me.Coeffs_est)
                out(i, 0) = Me.Coeffs_est(i)
                out(i, 1) = Me.Coeffs_SEsT(i)
                out(i, 2) = Me.Coeffs_Tstat(i)
                out(i, 3) = Me.Coeffs_PvaluesT(i)
                out(i, 4) = Me.Coeffs_95CIlowT(i)
                out(i, 5) = Me.Coeffs_95CIhighT(i)
            Next
            resTab.SetBody(out)
            resTab.AddHeaderTopRow(LMresult.CoeffsT_table_labels)
            If Me.bIntercept Then
                rowLbls = Matrix.ConcatArrays({"Intercept"}, Me.varNames)
            Else
                rowLbls = Me.varNames
            End If
            resTab.AddHeaderLeftRow(rowLbls)
            Return resTab
        End Get
    End Property

    ''' <summary>
    ''' Returns a matrix of T‑based coefficient statistics:
    ''' estimate, SEₜ, T, p‑value, CI‑lower, CI‑upper.
    ''' </summary>
    ''' <remarks>
    ''' See <a href="#LM_Tables">Mathematical Appendix §11</a>.
    ''' </remarks>

    ReadOnly Property CoeffsT_vals() As Double(,)
        Get
            Dim out(UBound(Me.Coeffs_est), 5) As Double
            For i = 0 To UBound(Me.Coeffs_est)
                out(i, 0) = Me.Coeffs_est(i)
                out(i, 1) = Me.Coeffs_SEsT(i)
                out(i, 2) = Me.Coeffs_Tstat(i)
                out(i, 3) = Me.Coeffs_PvaluesT(i)
                out(i, 4) = Me.Coeffs_95CIlowT(i)
                out(i, 5) = Me.Coeffs_95CIhighT(i)
            Next

            Return out
        End Get
    End Property

    ''' <summary>
    ''' Computes odds ratios, Wald χ², p‑values, and CI bounds for non‑intercept parameters.
    ''' </summary>
    ''' <remarks>
    ''' See <a href="#LM_OR">Mathematical Appendix §10</a>.
    ''' </remarks>

    ReadOnly Property ParameterOdds() As Object(,)
        'Odds Rations exp(parameter estimate)
        Get
            Dim out(,) As Object
            Dim NoParams As Integer = UBound(Me.Coeffs_est)
            If Me.bIntercept Then NoParams -= 1 'Do not compute odds ration for the intercept
            If NoParams < 0 Then
                ReDim out(0, 4)
                Return out
            End If
            ReDim out(NoParams, 4)

            For i = 0 To NoParams
                Dim k = If(Me.bIntercept, i + 1, i)
                If Me.Coeffs_est(k) > 600 Or Me.Coeffs_95CIhighZ(k) > 600 Or Me.Coeffs_95CIlowZ(k) > 600 Then 'Avoid overflow. Most likely a linear combination of vars
                    out(i, 0) = "Inf"
                    out(i, 1) = Me.Coeffs_Zstat(k)
                    out(i, 2) = Me.Coeffs_PvaluesZ(k)
                    out(i, 3) = "Inf"
                    out(i, 4) = "Inf"
                Else
                    out(i, 0) = Math.Exp(Me.Coeffs_est(k))       ': Odds Ratio
                    out(i, 1) = Me.Coeffs_Zstat(k) ^ 2           ': Chi-Square
                    out(i, 2) = Me.Coeffs_PvaluesZ(k)            ': pvalue
                    out(i, 3) = Math.Exp(Me.Coeffs_95CIlowZ(k))  ': LL
                    out(i, 4) = Math.Exp(Me.Coeffs_95CIhighZ(k)) ': UL
                End If
            Next
            Return out
        End Get
    End Property

    ''' <summary>
    ''' Returns a formatted <c>ResultTable</c> containing odds ratios and Wald statistics.
    ''' </summary>
    ''' <remarks>
    ''' See <a href="#LM_OR">Mathematical Appendix §10</a>.
    ''' </remarks>

    ReadOnly Property OR_toPrint() As ResultTable
        Get
            Dim resTab As ResultTable = New ResultTable
            resTab.SetBody(Me.ParameterOdds)
            resTab.AddHeaderTopRow(LMresult.OR_table_labels)
            resTab.AddHeaderLeftRow(Me.varNames)
            Return resTab
        End Get
    End Property

    ''' <summary>
    ''' Returns a formatted <c>ResultTable</c> containing model‑level diagnostic statistics
    ''' such as likelihood ratio tests, Wald tests, or score tests.
    ''' </summary>
    ''' <remarks>
    ''' See <a href="#Dev_Tables">Developer Manual §15</a>.
    ''' </remarks>

    Public Function getModelDiagnasticTable_toPrint() As ResultTable
        Dim resTab As ResultTable = New ResultTable
        If Me.ModelTableTopRow Is Nothing Then
            resTab.AddHeaderTopRow({"Model Analysis", "", "df", "p-value"})
        Else
            resTab.AddHeaderTopRow(Me.ModelTableTopRow)
        End If
        resTab.AddHeaderLeftRow(Me.ModelTableLabels)
        resTab.SetBody(Me.ModelTableVals)
        Return resTab
    End Function
End Class
