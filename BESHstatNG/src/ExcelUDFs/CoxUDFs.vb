Option Explicit On
Option Strict On

Imports System
Imports System.Collections.Concurrent
Imports System.Collections.Generic
Imports System.Linq
Imports BESHStatNG.AppInfrastructure
Imports ExcelDna.Integration

Namespace BESHStatNG.WorksheetFunctions

    ''' <summary>
    ''' Worksheet functions for Cox proportional hazards regression.
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' These functions fit and interrogate semi-parametric survival models in which the hazard for subject <c>i</c> at time <c>t</c> is
    ''' <c>h(t | x_i) = h_0(t) exp(x_i'β)</c>, where <c>h_0(t)</c> is an unspecified baseline hazard and <c>β</c> is a vector of regression coefficients.
    ''' </para>
    ''' <para>
    ''' The fitted model is identified by a handle returned by <c>BESH.SURV.COX_FIT</c>. This design allows the model to be estimated once and then reused
    ''' by other worksheet functions to obtain coefficient tables, global tests, diagnostics, baseline quantities, and predictions without refitting.
    ''' </para>
    ''' <para>
    ''' Supported options include different methods for handling tied event times, optional stratification, and optional robust standard errors.
    ''' Stratified models estimate separate baseline hazards by stratum while keeping a common coefficient vector across strata.
    ''' </para>
    ''' </remarks>
    Public Module CoxUDFs

        ' Simple in-process cache of fitted models so we can "fit once, query many".
        ' Key = handle string returned by COX_FIT.
        Private ReadOnly _coxCache As New ConcurrentDictionary(Of String, CoxModelHandle)(StringComparer.OrdinalIgnoreCase)

        Private Class CoxModelHandle
            Public Property Handle As String
            Public Property Model As CoxPH
            Public Property Result As CoxResult
            Public Property VarNames As String()
            Public Property RawVarNames As String()
            Public Property RawPredictorKeys As String()
            Public Property RawPredictorAbsoluteLetters As String()
            Public Property DesignSpec As RegressionFormulaDesignSpec
            Public Property OmitCategoricalReference As Boolean
            Public Property TieMethod As TieMethod
            Public Property Robust As Boolean
        End Class

        ''' <summary>
        ''' Fits a Cox proportional hazards regression model and returns a reusable model handle.
        ''' </summary>
        ''' <param name="time">
        ''' A single-column range containing observed follow-up times.
        ''' Values must be numeric and greater than or equal to 0.
        ''' Each row corresponds to one subject or observational unit.
        ''' </param>
        ''' <param name="status">
        ''' A single-column range containing event indicators.
        ''' Use 1 for an observed event and 0 for a right-censored observation.
        ''' The number of rows must match <paramref name="time"/> and the predictor matrix.
        ''' </param>
        ''' <param name="x">
        ''' A numeric predictor matrix with one row per subject and one column per covariate.
        ''' All rows must align with <paramref name="time"/> and <paramref name="status"/>.
        ''' Each coefficient in the fitted model represents the effect of a one-unit increase in the corresponding covariate on the log hazard.
        ''' </param>
        ''' <param name="varNames">
        ''' Optional predictor names.
        ''' This may be supplied as a comma-separated text string or as a one-row or one-column range containing one name per predictor.
        ''' If omitted, default names such as X1, X2, … are assigned automatically.
        ''' </param>
        ''' <param name="strata">
        ''' Optional stratification variable supplied as a single-column range.
        ''' When used, the model allows each stratum to have its own baseline hazard function while estimating a common set of regression coefficients across strata.
        ''' Stratification is useful when baseline risk differs between groups but proportional effects of the covariates are still assumed within strata.
        ''' </param>
        ''' <param name="ties">
        ''' Optional method used to handle tied event times.
        ''' Accepted values are typically <c>breslow</c>, <c>efron</c>, and <c>exact</c>.
        ''' The Breslow approximation is simple and fast, Efron is usually more accurate when ties are present, and the exact method is most computationally intensive.
        ''' </param>
        ''' <param name="robust">
        ''' Optional logical flag indicating whether robust (sandwich) standard errors should be computed.
        ''' Robust standard errors can be useful when model assumptions are mildly violated or when greater protection against variance misspecification is desired.
        ''' </param>
        ''' <param name="formula">
        ''' Optional right-hand-side model formula used to construct the design matrix from the raw predictor matrix <paramref name="x"/>.
        ''' Supported syntax currently includes additive terms (<c>A + B</c>), polynomial terms (<c>A^2</c>),
        ''' continuous-variable interactions (<c>A:B</c>, <c>A:B:C</c>), and categorical main effects such as <c>factor(C)</c> or <c>factor(C, ref=2)</c>.
        ''' If omitted or blank, all predictor columns are used as continuous main effects.
        ''' </param>
        ''' <param name="formulaAddressing">
        ''' Optional formula-addressing mode that controls how bare column-letter tokens are interpreted.
        ''' Accepted values are <c>relative</c> (default), <c>absolute</c>, and <c>names</c>.
        ''' In <c>relative</c> mode, <c>A</c>, <c>B</c>, <c>AA</c>, … refer to columns 1, 2, 27, … of <paramref name="x"/>.
        ''' In <c>absolute</c> mode, bare letters refer to worksheet columns of the supplied <paramref name="x"/> range.
        ''' In <c>names</c> mode, bare letters are disabled and variables should be referenced using single-quoted names such as <c>'Age'</c>.
        ''' Quoted variable names are also allowed in the other two modes.
        ''' </param>
        ''' <param name="maxIter">
        ''' Optional maximum number of iterations allowed in the numerical optimization.
        ''' Increase this value if convergence is slow for more complex models.
        ''' </param>
        ''' <param name="tol">
        ''' Optional convergence tolerance controlling when the iterative fitting procedure stops.
        ''' Smaller values require a tighter convergence criterion but may increase computation time.
        ''' </param>
        ''' <returns>
        ''' A text handle identifying the fitted Cox model within the current Excel session.
        ''' This handle can be passed to other Cox-related worksheet functions to retrieve summaries, tests, and diagnostics without refitting the model.
        ''' </returns>
        ''' <remarks>
        ''' <para>
        ''' The Cox model estimates regression effects by maximizing the partial likelihood rather than specifying a full parametric distribution for survival times.
        ''' As a result, the method is flexible and widely used for time-to-event data with right censoring.
        ''' </para>
        ''' <para>
        ''' The sign of a coefficient indicates the direction of association with the hazard:
        ''' positive coefficients increase the hazard and therefore correspond to shorter expected survival, whereas negative coefficients decrease the hazard.
        ''' Exponentiating a coefficient gives the hazard ratio associated with a one-unit increase in the covariate.
        ''' </para>
        ''' <para>
        ''' When a formula is supplied, the model matrix is built internally from the raw predictor columns.
        ''' If <c>formulaAddressing="absolute"</c> is used, the <paramref name="x"/> argument should be passed as a direct worksheet range so its absolute worksheet column letters can be determined.
        ''' </para>
        ''' <para>
        ''' Rows with invalid values, such as non-numeric times or predictors, are excluded before fitting.
        ''' If too few valid rows remain after filtering, the function returns an Excel error.
        ''' </para>
        ''' <para>
        ''' The returned handle is valid only for the current Excel session and should be treated as a temporary identifier rather than a permanent stored result.
        ''' </para>
        ''' </remarks>
        ''' <example>
        ''' <code>
        ''' =BESH.SURV.COX_FIT(A2:A101,B2:B101,C2:D101,"Age,Treatment")
        ''' =BESH.SURV.COX_FIT(A2:A101,B2:B101,C2:F101,"Age,BMI,Stage,Treat",,,"efron",FALSE,100,1E-8,"A + A^2 + factor(C, ref=1) + B:D","relative")
        ''' </code>
        ''' </example>
        <ExcelFunction(
            Name:="BESH.SURV.COX_FIT",
            Category:="BESHStatNG - Survival",
            Description:="Fits a Cox proportional hazards model and returns a handle for use with other COX_* functions.",
            HelpTopic:=HelpLinks.FallbackBaseUrl & "/udf/survival/"
        )>
        Public Function COX_FIT(
            <ExcelArgument(Name:="time", Description:="Follow-up time for each subject (>=0). One column.")> time As Object,
            <ExcelArgument(Name:="status", Description:="Event indicator (1=event, 0=censored). One column.")> status As Object,
            <ExcelArgument(AllowReference:=True, Name:="x", Description:="Predictor matrix. Rows align with time/status; columns are predictors.")> x As Object,
            <ExcelArgument(Name:="varNames", Description:="Optional predictor names (one row) or comma-separated list.")> Optional varNames As Object = Nothing,
            <ExcelArgument(Name:="strata", Description:="Optional stratification variable (one column).")> Optional strata As Object = Nothing,
            <ExcelArgument(Name:="ties", Description:="Ties method: ""breslow"" (default), ""efron"", ""exact"".")> Optional ties As Object = Nothing,
            <ExcelArgument(Name:="robust", Description:="TRUE to compute robust (sandwich) standard errors.")> Optional robust As Object = Nothing,
            <ExcelArgument(Name:="formula", Description:="Optional RHS model formula, e.g. ""A + A^2 + factor(C, ref=1) + B:D"".")> Optional formula As Object = Nothing,
            <ExcelArgument(Name:="formulaAddressing", Description:="Formula-addressing mode: ""relative"" (default), ""absolute"", or ""names"".")> Optional formulaAddressing As Object = Nothing,
            <ExcelArgument(Name:="maxIter", Description:="Maximum iterations (default 100).")> Optional maxIter As Object = Nothing,
            <ExcelArgument(Name:="tol", Description:="Convergence tolerance (default 1E-8).")> Optional tol As Object = Nothing
            ) As Object

            If ExcelDnaUtil.IsInFunctionWizard() Then Return "COX_FIT (editing...)"

            Try

                Dim imported As CoxPHData = Nothing
                If Not UDFhelpers.TryBuildCoxDataFromUdfArgs(time, status, x, varNames, strata, imported) Then
                    Return ExcelError.ExcelErrorValue
                End If

                Dim rowCount As Integer = imported.nRows
                Dim colCount As Integer = imported.nCols
                If colCount < 1 Then Return ExcelError.ExcelErrorNum

                Dim rawVNames As String() = DirectCast(imported.varNames.Clone(), String())

                Dim formulaText As String = UDFhelpers.AsString(formula)
                If String.IsNullOrWhiteSpace(formulaText) Then formulaText = Nothing

                Dim addressingMode As String = UDFhelpers.ParseFormulaAddressingMode(formulaAddressing, "relative")
                Dim allowRelativeColumnLetters As Boolean = False
                Dim allowAbsoluteColumnLetters As Boolean = False
                Dim allowQuotedVariableNames As Boolean = True

                Select Case addressingMode
                    Case "absolute"
                        allowAbsoluteColumnLetters = True
                    Case "names"
                        allowQuotedVariableNames = True
                    Case Else
                        allowRelativeColumnLetters = True
                        addressingMode = "relative"
                End Select

                Dim absoluteColumnLetters As String() = Nothing
                If allowAbsoluteColumnLetters AndAlso Not String.IsNullOrWhiteSpace(formulaText) Then
                    If Not UDFhelpers.TryGetAbsoluteColumnLettersFromRange(x, colCount, absoluteColumnLetters) Then
                        Return ExcelError.ExcelErrorValue
                    End If
                End If

                Dim designBuild As RegressionFormulaMatrixBuildResult = Nothing
                Dim designErr As String = Nothing
                If Not RegressionFormulaDesignService.TryBuildExpandedPredictorMatrixFromFormula(rawX:=imported.DataDbl,
                                                                                result:=designBuild,
                                                                                errorMessage:=designErr,
                                                                                predictorNames:=rawVNames,
                                                                                formulaText:=formulaText,
                                                                                absoluteColumnLetters:=absoluteColumnLetters,
                                                                                allowRelativeColumnLetters:=allowRelativeColumnLetters,
                                                                                allowAbsoluteColumnLetters:=allowAbsoluteColumnLetters,
                                                                                allowQuotedVariableNames:=allowQuotedVariableNames,
                                                                                omitCategoricalReference:=True) Then
                    Return ExcelError.ExcelErrorValue
                End If

                Dim fitX(,) As Double = designBuild.ExpandedPredictorMatrix
                Dim fitVarNames As String() = designBuild.ExpandedPredictorNames
                Dim fitColCount As Integer = If(fitVarNames Is Nothing, 0, fitVarNames.Length)
                If fitX Is Nothing OrElse fitColCount < 1 Then Return ExcelError.ExcelErrorValue

                Dim records As New List(Of survival.SurvivalRecord)(rowCount)
                For i As Integer = 0 To rowCount - 1
                    Dim rec As New survival.SurvivalRecord()
                    rec.Time = imported.TimeData(i)
                    rec.Censorship = imported.CensorData(i)
                    rec.Index = imported.RowIds(i)

                    Dim cov(fitColCount - 1) As Double
                    For j As Integer = 0 To fitColCount - 1
                        cov(j) = fitX(i, j)
                    Next
                    rec.Covariates = cov

                    If imported.bStrata Then
                        rec.Stratum = imported.StrataData(i)
                        rec.strStratum = imported.StrataData(i)
                    Else
                        rec.Stratum = "__ALL__"
                        rec.strStratum = "__ALL__"
                    End If

                    records.Add(rec)
                Next

                If records.Count < 3 Then Return ExcelError.ExcelErrorNum

                Dim mi As Integer = UDFhelpers.GetOptionalInt(maxIter, 100)
                Dim eps As Double = UDFhelpers.GetOptionalDouble(tol, 0.00000001)
                Dim method As TieMethod = UDFhelpers.ParseTieMethod(ties, TieMethod.Breslow)
                Dim useRobust As Boolean = UDFhelpers.GetOptionalBool(robust, False)

                Dim model As New CoxPH(records, fitVarNames, mi, eps)
                model.bRobustVariance = useRobust

                Dim res As CoxResult = model.Fit(method)

                Dim handle As String = "COX:" & Guid.NewGuid().ToString("N")
                Dim h As New CoxModelHandle With {
                    .Handle = handle,
                    .Model = model,
                    .Result = res,
                    .VarNames = fitVarNames,
                    .RawVarNames = If(designBuild.FullRawPredictorNames, rawVNames),
                    .RawPredictorKeys = designBuild.FullRawPredictorKeys,
                    .RawPredictorAbsoluteLetters = designBuild.FullRawPredictorAbsoluteLetters,
                    .DesignSpec = designBuild.DesignSpec,
                    .OmitCategoricalReference = True,
                    .TieMethod = method,
                    .Robust = useRobust
                }
                _coxCache(handle) = h
                Return handle

            Catch ex As Exception
                Return LoggedUdfError("BESH.SURV.COX_FIT", ex, ExcelError.ExcelErrorValue)
            End Try
        End Function

        ''' <summary>
        ''' Returns a coefficient table for a fitted Cox proportional hazards model.
        ''' </summary>
        ''' <param name="handle">
        ''' A model handle previously returned by <c>BESH.SURV.COX_FIT</c>.
        ''' </param>
        ''' <param name="includeHeader">
        ''' Optional logical flag indicating whether a header row should be included in the spilled output.
        ''' If omitted, a header row is included.
        ''' </param>
        ''' <param name="alpha">
        ''' Optional two-sided significance level used for the hazard-ratio confidence interval.
        ''' The default is <c>0.05</c>, corresponding to a 95% confidence interval.
        ''' </param>
        ''' <returns>
        ''' A spilled array with one row per predictor.
        ''' The output includes the variable name, regression coefficient, standard error, Wald z statistic, two-sided p-value,
        ''' hazard ratio, and a two-sided hazard-ratio confidence interval at level <c>1 - alpha</c>.
        ''' </returns>
        ''' <remarks>
        ''' <para>
        ''' The regression coefficient is the estimated change in log hazard associated with a one-unit increase in the predictor while holding the other predictors fixed.
        ''' The hazard ratio is obtained by exponentiating the coefficient and is often easier to interpret in applied work.
        ''' </para>
        ''' <para>
        ''' A hazard ratio greater than 1 indicates increased hazard, a hazard ratio less than 1 indicates reduced hazard, and a hazard ratio equal to 1 indicates no change.
        ''' For example, a hazard ratio of 1.25 corresponds to an estimated 25% increase in hazard per one-unit increase in the predictor.
        ''' </para>
        ''' <para>
        ''' The z statistic is formed by dividing the estimated coefficient by its standard error.
        ''' The reported p-value is the usual two-sided Wald p-value for testing whether the coefficient equals 0.
        ''' </para>
        ''' <para>
        ''' If robust standard errors were requested during model fitting, the summary uses those robust standard errors in place of the model-based standard errors.
        ''' </para>
        ''' </remarks>
        ''' <example>
        ''' <code>
        ''' =BESH.SURV.COX_SUMMARY(F2)
        ''' =BESH.SURV.COX_SUMMARY(F2, TRUE, 0.1)
        ''' </code>
        ''' </example>
        <ExcelFunction(
            Name:="BESH.SURV.COX_SUMMARY",
            Category:="BESHStatNG - Survival",
            Description:="Returns coefficient table (beta, SE, z, p, HR, CI) for a fitted Cox model handle.",
            HelpTopic:=HelpLinks.FallbackBaseUrl & "/udf/survival/"
        )>
        Public Function COX_SUMMARY(
            <ExcelArgument(Name:="handle", Description:="Handle returned by BESH.SURV.COX_FIT.")> handle As Object,
            <ExcelArgument(Name:="includeHeader", Description:="TRUE to include header row (default TRUE).")> Optional includeHeader As Object = Nothing,
            <ExcelArgument(Name:="alpha", Description:="Optional two-sided alpha for HR confidence intervals (default 0.05).")> Optional alpha As Object = Nothing
        ) As Object

            Dim key As String = UDFhelpers.AsString(handle)
            If String.IsNullOrWhiteSpace(key) Then Return ExcelError.ExcelErrorValue

            Dim h As CoxModelHandle = Nothing
            If Not _coxCache.TryGetValue(key, h) Then Return ExcelError.ExcelErrorNA

            Dim alphaValue As Double = 0.05
            If Not TryParseAlpha(alpha, alphaValue) Then Return ExcelError.ExcelErrorNum

            Dim zCrit As Double = distributions.ZCritTwoSided(alphaValue)
            Dim ciPct As String = $"{100.0 * (1.0 - alphaValue):0.##}%"

            Dim hdr As Boolean = UDFhelpers.GetOptionalBool(includeHeader, True)
            Dim p As Integer = h.VarNames.Length
            Dim rows As Integer = If(hdr, p + 1, p)
            Dim out(rows - 1, 7) As Object

            Dim r0 As Integer = 0
            If hdr Then
                out(0, 0) = "Variable"
                out(0, 1) = "Coef"
                out(0, 2) = "SE"
                out(0, 3) = "Z"
                out(0, 4) = "P-value"
                out(0, 5) = "HR"
                out(0, 6) = "HR " & ciPct & " LCL"
                out(0, 7) = "HR " & ciPct & " UCL"
                r0 = 1
            End If

            For i As Integer = 0 To p - 1
                Dim beta As Double = h.Result.Coefficients(i)
                Dim se As Double
                If h.Robust AndAlso h.Result.VarCovRobust IsNot Nothing AndAlso h.Result.VarCovRobust.GetLength(0) = p Then
                    se = Math.Sqrt(h.Result.VarCovRobust(i, i))
                Else
                    se = Math.Sqrt(h.Result.VarCov(i, i))
                End If

                Dim z As Double = beta / se
                Dim pv As Double = 2.0 * distributions.PNorm(-Math.Abs(z))
                Dim hr As Object = ExpForDisplay(beta)
                Dim lcl As Object = ExpForDisplay(beta - zCrit * se)
                Dim ucl As Object = ExpForDisplay(beta + zCrit * se)

                out(r0 + i, 0) = h.VarNames(i)
                out(r0 + i, 1) = beta
                out(r0 + i, 2) = se
                out(r0 + i, 3) = z
                out(r0 + i, 4) = pv
                out(r0 + i, 5) = hr
                out(r0 + i, 6) = lcl
                out(r0 + i, 7) = ucl
            Next

            Return out
        End Function

        ''' <summary>
        ''' Returns global significance tests and fit statistics for a fitted Cox proportional hazards model.
        ''' </summary>
        ''' <param name="handle">
        ''' A model handle previously returned by <c>BESH.SURV.COX_FIT</c>.
        ''' </param>
        ''' <param name="includeHeader">
        ''' Optional logical flag indicating whether a header row should be included in the spilled output.
        ''' If omitted, a header row is included.
        ''' </param>
        ''' <returns>
        ''' A spilled array containing global likelihood-ratio and Wald chi-square tests, their degrees of freedom and p-values,
        ''' together with additional fit information such as the null and fitted log-likelihoods, number of iterations, and convergence status.
        ''' </returns>
        ''' <remarks>
        ''' <para>
        ''' The likelihood-ratio test compares the fitted model with a model containing no predictors by comparing their log partial likelihood values.
        ''' It assesses whether the predictors, taken together, improve model fit.
        ''' </para>
        ''' <para>
        ''' The Wald test assesses the joint null hypothesis that all regression coefficients are equal to 0.
        ''' It is based on the estimated coefficient vector and its covariance matrix.
        ''' </para>
        ''' <para>
        ''' In many applications the likelihood-ratio, Wald, and score tests give similar conclusions, but they are not identical and may differ somewhat in small samples
        ''' or when the model is close to numerical instability.
        ''' </para>
        ''' <para>
        ''' Log-likelihood values are useful for model comparison. Larger fitted log-likelihood values indicate better agreement with the observed event ordering,
        ''' although comparisons are most meaningful between models fit to the same data.
        ''' </para>
        ''' <para>
        ''' The convergence indicator reports whether the iterative estimation procedure satisfied the stopping criterion before reaching the iteration limit.
        ''' Lack of convergence may indicate separation, collinearity, sparse information, or an overly complex model relative to the data.
        ''' </para>
        ''' </remarks>
        ''' <example>
        ''' <code>
        ''' =BESH.SURV.COX_TESTS(F2)
        ''' </code>
        ''' </example>
        <ExcelFunction(
            Name:="BESH.SURV.COX_TESTS",
            Category:="BESHStatNG - Survival",
            Description:="Returns global tests (LR, Wald) and fit statistics for a fitted Cox model handle.",
            HelpTopic:=HelpLinks.FallbackBaseUrl & "/udf/survival/"
        )>
        Public Function COX_TESTS(
            <ExcelArgument(Name:="handle", Description:="Handle returned by BESH.SURV.COX_FIT.")> handle As Object,
            <ExcelArgument(Name:="includeHeader", Description:="TRUE to include header row (default TRUE).")> Optional includeHeader As Object = Nothing
        ) As Object

            Try
                Dim key As String = UDFhelpers.AsString(handle)
                If String.IsNullOrWhiteSpace(key) Then Return ExcelError.ExcelErrorValue

                Dim h As CoxModelHandle = Nothing
                If Not _coxCache.TryGetValue(key, h) Then Return ExcelError.ExcelErrorNA
                If h Is Nothing OrElse h.Result Is Nothing Then Return ExcelError.ExcelErrorNA
                If h.VarNames Is Nothing Then Return ExcelError.ExcelErrorNA

                Dim hdr As Boolean = UDFhelpers.GetOptionalBool(includeHeader, True)
                Dim p As Integer = h.VarNames.Length
                If p < 1 Then Return ExcelError.ExcelErrorNA

                If h.Result.Coefficients Is Nothing OrElse h.Result.Coefficients.Length < p Then
                    Return "Invalid coefficient vector length."
                End If

                Dim lr As Object = ExcelError.ExcelErrorNA
                Dim lrP As Object = ExcelError.ExcelErrorNA

                If Not Double.IsNaN(h.Result.LogLikelihoodNull) AndAlso Not Double.IsInfinity(h.Result.LogLikelihoodNull) AndAlso
                    Not Double.IsNaN(h.Result.LogLikelihood) AndAlso Not Double.IsInfinity(h.Result.LogLikelihood) Then
                    Dim lrVal As Double = -2.0 * (h.Result.LogLikelihoodNull - h.Result.LogLikelihood)
                    If Not Double.IsNaN(lrVal) AndAlso Not Double.IsInfinity(lrVal) AndAlso lrVal >= 0 Then
                        lr = lrVal
                        Try
                            lrP = 1.0 - distributions.ChiSquareCDF(lrVal, p)
                        Catch
                            lrP = ExcelError.ExcelErrorNA
                        End Try
                    End If
                End If

                Dim wald As Object = ExcelError.ExcelErrorNA
                Dim waldP As Object = ExcelError.ExcelErrorNA

                Dim v As Double(,) = Nothing
                If h.Robust AndAlso
                   h.Result.VarCovRobust IsNot Nothing AndAlso
                   h.Result.VarCovRobust.GetLength(0) = p AndAlso
                   h.Result.VarCovRobust.GetLength(1) = p Then

                    v = h.Result.VarCovRobust

                ElseIf h.Result.VarCov IsNot Nothing AndAlso
                   h.Result.VarCov.GetLength(0) = p AndAlso
                   h.Result.VarCov.GetLength(1) = p Then

                    v = h.Result.VarCov
                End If

                If v IsNot Nothing Then
                    Dim invV As Double(,) = Nothing

                    If UDFhelpers.TryInvertMatrix(v, invV) Then
                        If invV IsNot Nothing AndAlso invV.GetLength(0) = p AndAlso invV.GetLength(1) = p Then

                            Dim tmp(p - 1) As Double

                            For i As Integer = 0 To p - 1
                                Dim s As Double = 0.0
                                For j As Integer = 0 To p - 1
                                    s += invV(i, j) * h.Result.Coefficients(j)
                                Next
                                tmp(i) = s
                            Next

                            Dim w As Double = 0.0
                            For i As Integer = 0 To p - 1
                                w += h.Result.Coefficients(i) * tmp(i)
                            Next

                            If Not Double.IsNaN(w) AndAlso Not Double.IsInfinity(w) AndAlso w >= 0 Then
                                wald = w
                                Try
                                    waldP = 1.0 - distributions.ChiSquareCDF(w, p)
                                Catch
                                    waldP = ExcelError.ExcelErrorNA
                                End Try
                            End If
                        End If
                    End If
                End If

                Dim nRows As Integer = If(hdr, 7, 5)
                Dim out(nRows - 1, 3) As Object
                Dim r As Integer = 0

                If hdr Then
                    out(0, 0) = "Item"
                    out(0, 1) = "Value"
                    out(0, 2) = "df"
                    out(0, 3) = "P-value"
                    r = 1
                End If

                out(r + 0, 0) = "Likelihood-ratio chi-square"
                out(r + 0, 1) = lr
                out(r + 0, 2) = p
                out(r + 0, 3) = lrP

                out(r + 1, 0) = "Wald chi-square"
                out(r + 1, 1) = wald
                out(r + 1, 2) = p
                out(r + 1, 3) = waldP

                out(r + 2, 0) = "Log-likelihood (null)"
                out(r + 2, 1) = If(Not Double.IsNaN(h.Result.LogLikelihoodNull) AndAlso Not Double.IsInfinity(h.Result.LogLikelihoodNull), CType(h.Result.LogLikelihoodNull, Object), ExcelError.ExcelErrorNA)
                out(r + 2, 2) = ""
                out(r + 2, 3) = ""

                out(r + 3, 0) = "Log-likelihood (fitted)"
                out(r + 3, 1) = If(Not Double.IsNaN(h.Result.LogLikelihood) AndAlso Not Double.IsInfinity(h.Result.LogLikelihood), CType(h.Result.LogLikelihood, Object), ExcelError.ExcelErrorNA)
                out(r + 3, 2) = ""
                out(r + 3, 3) = ""

                out(r + 4, 0) = "Iterations"
                out(r + 4, 1) = h.Result.Iterations
                out(r + 4, 2) = ""
                out(r + 4, 3) = ""

                If hdr Then
                    out(r + 5, 0) = "Converged"
                    out(r + 5, 1) = h.Result.Converged
                    out(r + 5, 2) = ""
                    out(r + 5, 3) = ""
                End If

                Return out

            Catch ex As Exception
                Return LoggedUdfExceptionText("BESH.SURV.COX_TESTS", ex)
            End Try
        End Function

        ''' <summary>
        ''' Returns residual-based diagnostics for a fitted Cox proportional hazards model.
        ''' </summary>
        ''' <param name="handle">
        ''' A model handle previously returned by <c>BESH.SURV.COX_FIT</c>.
        ''' </param>
        ''' <param name="residType">
        ''' The residual type to return.
        ''' Supported values commonly include <c>martingale</c>, <c>deviance</c>, <c>schoenfeld</c>, and <c>dfbeta</c>.
        ''' </param>
        ''' <returns>
        ''' A spilled array containing the requested residual output.
        ''' For observation-level residuals such as martingale or deviance residuals, the result is usually one row per subject.
        ''' For coefficient-specific diagnostics such as Schoenfeld residuals or DFBETA values, the result may contain one column per coefficient.
        ''' </returns>
        ''' <remarks>
        ''' <para>
        ''' Residuals provide diagnostic information about model fit, influential observations, and possible departures from assumptions.
        ''' Different residuals answer different questions and should be interpreted accordingly.
        ''' </para>
        ''' <para>
        ''' Martingale residuals are useful for assessing functional form and identifying outlying event patterns, but they are often highly skewed.
        ''' Deviance residuals are a transformation of martingale residuals that tends to be more symmetric and easier to inspect graphically.
        ''' </para>
        ''' <para>
        ''' Schoenfeld residuals are tied to event times and are especially useful for assessing the proportional hazards assumption.
        ''' DFBETA diagnostics quantify how strongly each observation influences each coefficient estimate.
        ''' </para>
        ''' <para>
        ''' Residuals should generally be interpreted together with the fitted model, subject-matter knowledge, and graphical inspection.
        ''' A single unusual residual value does not automatically imply model failure, but systematic patterns can indicate misspecification or influential data points.
        ''' </para>
        ''' </remarks>
        ''' <example>
        ''' <code>
        ''' =BESH.SURV.COX_RESID(F2,"martingale")
        ''' </code>
        ''' </example>
        <ExcelFunction(Name:="BESH.SURV.COX_RESID",
                       Category:="BESHStatNG - Survival",
                       Description:="Returns residual diagnostics for a fitted Cox model.",
                       HelpTopic:=HelpLinks.FallbackBaseUrl & "/udf/survival/"
                       )>
        Public Function COX_RESID(
            <ExcelArgument(Name:="handle", Description:="Handle returned by BESH.SURV.COX_FIT.")> handle As Object,
            <ExcelArgument(Name:="type", Description:="Residual type: martingale, deviance, schoenfeld, or dfbeta.")> residType As Object
        ) As Object
            Try
                Dim h As CoxModelHandle = Nothing
                If Not TryGetHandle(handle, h) Then Return ExcelError.ExcelErrorNA
                Dim rt As ResidualType = ParseResidualType(residType)
                If Not EnsureResiduals(h) Then Return ExcelError.ExcelErrorValue
                Dim d As Dictionary(Of Integer, Double()) = h.Model.Residuals(rt)
                If d Is Nothing OrElse d.Count = 0 Then Return ExcelError.ExcelErrorNA
                Dim keys = d.Keys.OrderBy(Function(x) x).ToList()
                Dim width As Integer = d(keys(0)).Length
                Dim out(keys.Count, width) As Object
                out(0, 0) = "Row ID"
                If width = 1 Then
                    out(0, 1) = ResidualLabel(rt)
                Else
                    For j As Integer = 0 To width - 1
                        out(0, j + 1) = If(j < h.VarNames.Length, h.VarNames(j), "V" & (j + 1).ToString())
                    Next
                End If
                For i As Integer = 0 To keys.Count - 1
                    out(i + 1, 0) = keys(i)
                    Dim vals = d(keys(i))
                    For j As Integer = 0 To vals.Length - 1
                        out(i + 1, j + 1) = vals(j)
                    Next
                Next
                Return out
            Catch ex As Exception
                Return LoggedUdfExceptionText("BESH.SURV.COX_RESID", ex)
            End Try
        End Function

        ''' <summary>
        ''' Returns baseline quantities derived from a fitted Cox proportional hazards model.
        ''' </summary>
        ''' <param name="handle">
        ''' A model handle previously returned by <c>BESH.SURV.COX_FIT</c>.
        ''' </param>
        ''' <param name="baselineType">
        ''' Optional output type.
        ''' Common choices include <c>table</c>, <c>survival</c>, <c>cumhaz</c>, or a plot-ready representation, depending on the supported outputs in the implementation.
        ''' </param>
        ''' <returns>
        ''' A spilled array containing baseline output evaluated over event times.
        ''' Depending on the requested type, the result may include event times together with the baseline survival function,
        ''' cumulative baseline hazard, or a fuller tabular representation.
        ''' </returns>
        ''' <remarks>
        ''' <para>
        ''' In the Cox model, the regression coefficients describe relative hazards, while the baseline hazard or baseline survival function captures the underlying time pattern
        ''' for a reference covariate pattern with linear predictor equal to 0.
        ''' </para>
        ''' <para>
        ''' The baseline cumulative hazard is often estimated using a Breslow-type estimator, and the baseline survival function is then obtained as
        ''' <c>S_0(t) = exp(-H_0(t))</c>.
        ''' These quantities form the basis for individual survival predictions once a subject's linear predictor has been calculated.
        ''' </para>
        ''' <para>
        ''' In stratified models, separate baseline functions are estimated for each stratum.
        ''' As a result, baseline output should always be interpreted together with the stratum structure used during fitting.
        ''' </para>
        ''' <para>
        ''' Baseline quantities are model-based estimates and therefore inherit the assumptions of the fitted Cox model, including the proportional hazards structure.
        ''' </para>
        ''' </remarks>
        ''' <example>
        ''' <code>
        ''' =BESH.SURV.COX_BASELINE(F2,"survival")
        ''' </code>
        ''' </example>
        <ExcelFunction(Name:="BESH.SURV.COX_BASELINE",
                       Category:="BESHStatNG - Survival",
                       Description:="Returns baseline survival or cumulative hazard from a fitted Cox model.",
                       HelpTopic:=HelpLinks.FallbackBaseUrl & "/udf/survival/")>
        Public Function COX_BASELINE(
            <ExcelArgument(Name:="handle", Description:="Handle returned by BESH.SURV.COX_FIT.")> handle As Object,
            <ExcelArgument(Name:="type", Description:="Output type: table (default), survival, cumhaz, or plot.")> Optional baselineType As Object = Nothing
        ) As Object
            Try
                Dim h As CoxModelHandle = Nothing
                If Not TryGetHandle(handle, h) Then Return ExcelError.ExcelErrorNA
                Dim tp As String = UDFhelpers.AsString(baselineType)
                If String.IsNullOrWhiteSpace(tp) Then tp = "table"
                tp = tp.ToLowerInvariant()
                Dim base = h.Model.ComputeBaseline(False)
                If base Is Nothing OrElse base.Count = 0 Then Return ExcelError.ExcelErrorNA
                Dim total As Integer = 1
                For Each kv In base
                    Dim arr = kv.Value
                    If tp = "plot" Then
                        total += h.Model.BaseSurvivalForPloting(arr).GetLength(0)
                    Else
                        total += arr.GetLength(0)
                    End If
                Next
                Dim cols As Integer = If(tp = "survival" OrElse tp = "cumhaz", 3, 4)
                Dim out(total - 1, cols - 1) As Object
                If cols = 4 Then
                    out(0, 0) = "Stratum" : out(0, 1) = "Time" : out(0, 2) = "Survival" : out(0, 3) = "CumHazard"
                Else
                    out(0, 0) = "Stratum" : out(0, 1) = "Time" : out(0, 2) = If(tp = "survival", "Survival", "CumHazard")
                End If
                Dim r As Integer = 1
                For Each kv In base
                    Dim arr As Double(,) = kv.Value
                    If tp = "plot" Then arr = h.Model.BaseSurvivalForPloting(arr)
                    For i As Integer = 0 To arr.GetLength(0) - 1
                        out(r, 0) = Convert.ToString(kv.Key)
                        out(r, 1) = arr(i, 0)
                        If tp = "survival" Then
                            out(r, 2) = arr(i, 1)
                        ElseIf tp = "cumhaz" OrElse tp = "hazard" Then
                            out(r, 2) = arr(i, 2)
                        Else
                            out(r, 2) = arr(i, 1)
                            out(r, 3) = arr(i, 2)
                        End If
                        r += 1
                    Next
                Next
                Return out
            Catch ex As Exception
                Return LoggedUdfExceptionText("BESH.SURV.COX_BASELINE", ex)
            End Try
        End Function

        ''' <summary>
        ''' Returns predictions from a fitted Cox proportional hazards model for new covariate values.
        ''' </summary>
        ''' <param name="handle">
        ''' A model handle previously returned by <c>BESH.SURV.COX_FIT</c>.
        ''' </param>
        ''' <param name="newX">
        ''' A numeric matrix containing one row per subject and one column per predictor.
        ''' The number and ordering of columns must match the predictor matrix used when the model was fitted.
        ''' </param>
        ''' <param name="predType">
        ''' Optional prediction type.
        ''' Common choices include <c>lp</c> for the linear predictor, <c>risk</c> for relative risk, <c>survival</c> for predicted survival probabilities,
        ''' and <c>cumhaz</c> for predicted cumulative hazard values.
        ''' </param>
        ''' <param name="timeGrid">
        ''' Optional time grid used for time-dependent predictions such as survival probabilities or cumulative hazards.
        ''' This should usually be supplied as a single-column numeric range.
        ''' </param>
        ''' <returns>
        ''' A spilled array containing the requested predictions.
        ''' For scalar predictions such as the linear predictor or relative risk, the output typically contains one row per subject.
        ''' For time-dependent predictions, the output may contain one row per time point or one block of results per subject, depending on the selected type.
        ''' </returns>
        ''' <remarks>
        ''' <para>
        ''' The linear predictor is the quantity <c>x'β</c>. It summarizes the combined effect of the subject's covariates on the log hazard scale.
        ''' Exponentiating the linear predictor gives the relative risk <c>exp(x'β)</c>, which compares a subject's hazard with the baseline hazard.
        ''' </para>
        ''' <para>
        ''' Predicted survival for subject <c>i</c> at time <c>t</c> is typically obtained as
        ''' <c>S_i(t) = S_0(t) ^ exp(x_i'β)</c>, where <c>S_0(t)</c> is the estimated baseline survival function.
        ''' Likewise, the predicted cumulative hazard is obtained by scaling the baseline cumulative hazard by <c>exp(x_i'β)</c>.
        ''' </para>
        ''' <para>
        ''' Predictions are conditional on the fitted model and therefore depend on the chosen ties method, any stratification, and the observed event structure in the estimation sample.
        ''' In stratified models, time-dependent predictions require the appropriate stratum-specific baseline function.
        ''' </para>
        ''' <para>
        ''' The <c>newX</c> argument should contain the raw predictor columns in the same order as the original model input.
        ''' When the fitted model uses internally expanded terms such as factors, polynomials, or interactions, the prediction path rebuilds the required design matrix automatically from those raw inputs.
        ''' </para>
        ''' <para>
        ''' Prediction functions are most meaningful when the new covariate values are within the practical range of the data used to fit the model.
        ''' Strong extrapolation beyond the observed predictor region should be interpreted cautiously.
        ''' </para>
        ''' </remarks>
        ''' <example>
        ''' <code>
        ''' =BESH.SURV.COX_PRED(F2,H2:I6,"risk")
        ''' </code>
        ''' </example>
        <ExcelFunction(Name:="BESH.SURV.COX_PRED",
                       Category:="BESHStatNG - Survival",
                       Description:="Computes predictions from a fitted Cox model (linear predictor, risk, survival, or cumulative hazard).",
                       HelpTopic:=HelpLinks.FallbackBaseUrl & "/udf/survival/")>
        Public Function COX_PRED(
            <ExcelArgument(Name:="handle", Description:="Handle returned by BESH.SURV.COX_FIT.")> handle As Object,
            <ExcelArgument(AllowReference:=True, Name:="newX", Description:="Numeric matrix of raw predictor columns in the same order as used when fitting the model.")> newX As Object,
            <ExcelArgument(Name:="type", Description:="Prediction type: lp, risk, survival, or cumhaz.")> Optional predType As Object = Nothing,
            <ExcelArgument(Name:="timeGrid", Description:="Optional single-column vector of time points for survival/cumhaz predictions.")> Optional timeGrid As Object = Nothing
        ) As Object
            Try
                Dim h As CoxModelHandle = Nothing
                If Not TryGetHandle(handle, h) Then Return ExcelError.ExcelErrorNA

                Dim rawPredictorKeys As String() = If(h.RawPredictorKeys, h.RawVarNames)
                If rawPredictorKeys Is Nothing OrElse rawPredictorKeys.Length < 1 Then Return ExcelError.ExcelErrorValue

                Dim imported As glmData = Nothing
                If Not UDFhelpers.TryBuildPredictorDataFromUdfArgs(newX, rawPredictorKeys, Nothing, False, imported) Then
                    Return ExcelError.ExcelErrorValue
                End If

                If imported.nCols <> rawPredictorKeys.Length Then Return ExcelError.ExcelErrorValue

                Dim xMat As Double(,) = imported.DataDbl
                Dim predNames As String() = rawPredictorKeys

                If h.DesignSpec IsNot Nothing AndAlso rawPredictorKeys.Length > 0 Then
                    Dim expandedX(,) As Double = Nothing
                    Dim expandedNames() As String = Nothing
                    Dim designErr As String = Nothing
                    If Not RegressionFormulaDesignService.TryBuildExpandedPredictorMatrixFromDesignSpec(rawX:=imported.DataDbl,
                                                                                                fullRawPredictorKeys:=rawPredictorKeys,
                                                                                                designSpec:=h.DesignSpec,
                                                                                                expandedX:=expandedX,
                                                                                                expandedPredictorNames:=expandedNames,
                                                                                                errorMessage:=designErr,
                                                                                                omitCategoricalReference:=h.OmitCategoricalReference) Then
                        Return ExcelError.ExcelErrorValue
                    End If
                    xMat = expandedX
                    predNames = expandedNames
                End If

                Dim p As Integer = If(predNames Is Nothing, 0, predNames.Length)
                If xMat Is Nothing OrElse p < 1 OrElse h.Result Is Nothing OrElse h.Result.Coefficients Is Nothing Then Return ExcelError.ExcelErrorValue

                Dim tp As String = UDFhelpers.AsString(predType)
                If String.IsNullOrWhiteSpace(tp) Then tp = "lp"
                tp = tp.ToLowerInvariant()

                Dim beta = h.Result.Coefficients
                If beta.Length <> p Then Return ExcelError.ExcelErrorValue

                Dim n As Integer = imported.nRows

                If tp = "lp" OrElse tp = "risk" Then
                    Dim out(n, 1) As Object
                    out(0, 0) = "Subject"
                    out(0, 1) = If(tp = "lp", "LinearPredictor", "Risk")

                    For i As Integer = 0 To n - 1
                        Dim row(p - 1) As Double
                        For j As Integer = 0 To p - 1
                            row(j) = xMat(i, j)
                        Next

                        Dim lp As Double = Global.BESHStatNG.Matrix.Matrix.DotProduct(row, beta)
                        out(i + 1, 0) = i + 1
                        out(i + 1, 1) = If(tp = "lp", lp, Math.Exp(lp))
                    Next

                    Return out
                End If

                Dim base = h.Model.ComputeBaseline(False)
                If base Is Nothing OrElse base.Count <> 1 Then Return ExcelError.ExcelErrorValue

                Dim arr As Double(,) = base.Values.First()
                Dim times As New List(Of Double)
                If timeGrid IsNot Nothing AndAlso Not TypeOf timeGrid Is ExcelMissing AndAlso Not TypeOf timeGrid Is ExcelEmpty Then
                    Dim tVals As List(Of Double) = Nothing
                    If Not UDFhelpers.TryReadNumericColumn(timeGrid, tVals) Then Return ExcelError.ExcelErrorValue
                    For Each tt In tVals
                        If Not Double.IsNaN(tt) AndAlso Not Double.IsInfinity(tt) AndAlso tt >= 0 Then times.Add(tt)
                    Next
                Else
                    For i As Integer = 0 To arr.GetLength(0) - 1
                        times.Add(arr(i, 0))
                    Next
                End If

                If times.Count = 0 Then Return ExcelError.ExcelErrorNum

                Dim out2(n * times.Count, 2) As Object
                out2(0, 0) = "Subject"
                out2(0, 1) = "Time"
                out2(0, 2) = If(tp = "cumhaz", "CumHazard", "Survival")

                Dim r As Integer = 1
                For i As Integer = 0 To n - 1
                    Dim row(p - 1) As Double
                    For j As Integer = 0 To p - 1
                        row(j) = xMat(i, j)
                    Next

                    Dim risk As Double = Math.Exp(Global.BESHStatNG.Matrix.Matrix.DotProduct(row, beta))

                    For Each tt In times
                        Dim baseCumHaz As Double = LookupStepValue(arr, tt, 2)
                        out2(r, 0) = i + 1
                        out2(r, 1) = tt
                        If tp = "cumhaz" OrElse tp = "hazard" Then
                            out2(r, 2) = baseCumHaz * risk
                        Else
                            out2(r, 2) = Math.Exp(-baseCumHaz * risk)
                        End If
                        r += 1
                    Next
                Next

                Return out2

            Catch ex As Exception
                Return LoggedUdfExceptionText("BESH.SURV.COX_PRED", ex)
            End Try
        End Function

        ''' <summary>
        ''' Removes a fitted Cox model handle from memory.
        ''' </summary>
        ''' <param name="handle">
        ''' A model handle previously returned by <c>BESH.SURV.COX_FIT</c>.
        ''' </param>
        ''' <returns>
        ''' TRUE if the handle existed and was removed successfully; otherwise FALSE.
        ''' </returns>
        ''' <remarks>
        ''' <para>
        ''' Cox model handles are stored only for the current Excel session so that related worksheet functions can reuse the fitted model without repeating the estimation step.
        ''' This function removes the cached model when it is no longer needed.
        ''' </para>
        ''' <para>
        ''' Removing unused handles can help reduce memory use in large workbooks or after repeated model fitting during exploratory analysis.
        ''' Once a handle has been removed, it can no longer be used by other Cox-related worksheet functions.
        ''' </para>
        ''' </remarks>
        ''' <example>
        ''' <code>
        ''' =BESH.SURV.COX_DROP(F2)
        ''' </code>
        ''' </example>
        <ExcelFunction(
            Name:="BESH.SURV.COX_DROP",
            Category:="BESHStatNG - Survival",
            Description:="Removes a fitted Cox model handle from memory.",
            HelpTopic:=HelpLinks.FallbackBaseUrl & "/udf/survival/"
        )>
        Public Function COX_DROP(
            <ExcelArgument(Name:="handle",
                           Description:="Handle returned by BESH.SURV.COX_FIT.")> handle As Object
        ) As Object
            Dim key As String = UDFhelpers.AsString(handle)
            If String.IsNullOrWhiteSpace(key) Then Return ExcelError.ExcelErrorValue
            Dim removed As CoxModelHandle = Nothing
            Return _coxCache.TryRemove(key, removed)
        End Function

        Private Function TryGetHandle(handle As Object, ByRef h As CoxModelHandle) As Boolean
            h = Nothing
            Dim key As String = UDFhelpers.AsString(handle)
            If String.IsNullOrWhiteSpace(key) Then Return False
            Return _coxCache.TryGetValue(key, h)
        End Function

        Private Function EnsureResiduals(h As CoxModelHandle) As Boolean
            Try
                Dim d = h.Model.Residuals(ResidualType.Martingale)
                If d IsNot Nothing AndAlso d.Count > 0 Then Return True
            Catch ex As Exception
                AppGlobals.BSlogg.Debug($"BESH.SURV.COX_RESID cache probe failed: {ex.Message}")
            End Try
            Try
                h.Model.bComputeAllResiduals = True
                h.Result = h.Model.Fit(h.TieMethod)
                Return True
            Catch ex As Exception
                AppGlobals.BSlogg.Error(ex, "BESH.SURV.COX_RESID failed while computing residuals.")
                Return False
            End Try
        End Function

        Private Function ParseResidualType(v As Object) As ResidualType
            Dim s As String = UDFhelpers.AsString(v)
            If String.IsNullOrWhiteSpace(s) Then Return ResidualType.Martingale
            Select Case s.Trim().ToLowerInvariant()
                Case "martingale" : Return ResidualType.Martingale
                Case "deviance" : Return ResidualType.Deviance
                Case "schoenfeld" : Return ResidualType.Schoenfeld
                Case "dfbeta" : Return ResidualType.Dfbeta
                Case Else : Return ResidualType.Martingale
            End Select
        End Function

        Private Function ResidualLabel(rt As ResidualType) As String
            Select Case rt
                Case ResidualType.Martingale : Return "Martingale"
                Case ResidualType.Deviance : Return "Deviance"
                Case ResidualType.Schoenfeld : Return "Schoenfeld"
                Case ResidualType.Dfbeta : Return "Dfbeta"
                Case Else : Return "Residual"
            End Select
        End Function

        Private Function LookupStepValue(arr As Double(,), t As Double, col As Integer) As Double
            Dim last As Double = If(col = 1, 1.0, 0.0)
            For i As Integer = 0 To arr.GetLength(0) - 1
                If arr(i, 0) > t Then Exit For
                last = arr(i, col)
            Next
            Return last
        End Function
    End Module

End Namespace
