Option Explicit On
Option Strict On

Namespace Global.BESHStatCoreSmoke

    ''' <summary>
    ''' Host-neutral payload returned by the GLM WebAssembly smoke test.
    ''' </summary>
    Public NotInheritable Class GlmWasmSmokeResult

        Public Sub New(coefficients As Global.BESHStatNG.ResultTableOutputModel,
                       diagnostics As Global.BESHStatNG.ResultTableOutputModel,
                       iterationDetails As Global.BESHStatNG.ResultTableOutputModel,
                       covariance As Global.BESHStatNG.ResultTableOutputModel,
                       residuals As Object(,),
                       coefficientEstimates As Double(),
                       standardErrors As Double(),
                       pValues As Double(),
                       aic As Double,
                       logLikelihood As Double,
                       pseudoR2 As Double,
                       devianceGofP As Double,
                       pearsonGofP As Double,
                       observationCount As Integer)
            Me.Coefficients = coefficients
            Me.Diagnostics = diagnostics
            Me.IterationDetails = iterationDetails
            Me.Covariance = covariance
            Me.Residuals = residuals
            Me.CoefficientEstimates = coefficientEstimates
            Me.StandardErrors = standardErrors
            Me.PValues = pValues
            Me.AIC = aic
            Me.LogLikelihood = logLikelihood
            Me.PseudoR2 = pseudoR2
            Me.DevianceGOFP = devianceGofP
            Me.PearsonGOFP = pearsonGofP
            Me.ObservationCount = observationCount
        End Sub

        Public ReadOnly Property Coefficients As Global.BESHStatNG.ResultTableOutputModel
        Public ReadOnly Property Diagnostics As Global.BESHStatNG.ResultTableOutputModel
        Public ReadOnly Property IterationDetails As Global.BESHStatNG.ResultTableOutputModel
        Public ReadOnly Property Covariance As Global.BESHStatNG.ResultTableOutputModel
        Public ReadOnly Property Residuals As Object(,)
        Public ReadOnly Property CoefficientEstimates As Double()
        Public ReadOnly Property StandardErrors As Double()
        Public ReadOnly Property PValues As Double()
        Public ReadOnly Property AIC As Double
        Public ReadOnly Property LogLikelihood As Double
        Public ReadOnly Property PseudoR2 As Double
        Public ReadOnly Property DevianceGOFP As Double
        Public ReadOnly Property PearsonGOFP As Double
        Public ReadOnly Property ObservationCount As Integer
    End Class

    ''' <summary>
    ''' Host-neutral result returned by the Excel/Office.js one-way ANOVA bridge.
    ''' </summary>
    Public NotInheritable Class AnovaExcelBridgeResult

        Public Sub New(table As Global.BESHStatNG.ResultTableOutputModel,
                       fStatistic As Double,
                       pValue As Double,
                       groupNames As String(),
                       groupCounts As Integer())
            Me.Table = table
            Me.FStatistic = fStatistic
            Me.PValue = pValue
            Me.GroupNames = groupNames
            Me.GroupCounts = groupCounts
        End Sub

        Public ReadOnly Property Table As Global.BESHStatNG.ResultTableOutputModel
        Public ReadOnly Property FStatistic As Double
        Public ReadOnly Property PValue As Double
        Public ReadOnly Property GroupNames As String()
        Public ReadOnly Property GroupCounts As Integer()
    End Class

    Public NotInheritable Class WasmSmokeProbe

        Private Sub New()
        End Sub

        ''' <summary>
        ''' Runs a classical one-way ANOVA on grouped values supplied by a host such as
        ''' Office.js and returns the host-neutral ResultTable output.
        ''' </summary>
        Public Shared Function RunOneWayAnova(groupedData As Double()(),
                                              groupNames As String()) As AnovaExcelBridgeResult
            If groupedData Is Nothing Then
                Throw New ArgumentNullException(NameOf(groupedData))
            End If
            If groupNames Is Nothing Then
                Throw New ArgumentNullException(NameOf(groupNames))
            End If
            If groupedData.Length < 2 Then
                Throw New ArgumentException("At least two groups are required.", NameOf(groupedData))
            End If
            If groupedData.Length <> groupNames.Length Then
                Throw New ArgumentException("The number of groups and group names must match.")
            End If

            Dim counts(groupedData.Length - 1) As Integer
            For i As Integer = 0 To groupedData.Length - 1
                If groupedData(i) Is Nothing OrElse groupedData(i).Length < 2 Then
                    Throw New ArgumentException(
                        "Each group must contain at least two numeric observations. Group index: " &
                        i.ToString(Globalization.CultureInfo.InvariantCulture) & ".",
                        NameOf(groupedData))
                End If

                counts(i) = groupedData(i).Length

                For j As Integer = 0 To groupedData(i).Length - 1
                    Dim value As Double = groupedData(i)(j)
                    If Double.IsNaN(value) OrElse Double.IsInfinity(value) Then
                        Throw New ArgumentException("ANOVA input contains a non-finite value.", NameOf(groupedData))
                    End If
                Next
            Next

            Dim model As New Global.BESHStatNG.parametric.OneWayANOVA(groupedData, groupNames)
            Dim raw As Object(,) = model.compute()
            Dim tables As System.Collections.Generic.List(Of Global.BESHStatNG.ResultTable) = model.wrapResults()

            If tables Is Nothing OrElse tables.Count = 0 Then
                Throw New InvalidOperationException("One-way ANOVA did not produce a ResultTable.")
            End If

            Dim output As Global.BESHStatNG.ResultTableOutputModel = tables(0).ToOutputModel()
            Dim fStatistic As Double = Convert.ToDouble(raw(0, 3), Globalization.CultureInfo.InvariantCulture)
            Dim pValue As Double = Convert.ToDouble(raw(0, 4), Globalization.CultureInfo.InvariantCulture)

            Return New AnovaExcelBridgeResult(
                output,
                fStatistic,
                pValue,
                DirectCast(groupNames.Clone(), String()),
                counts)
        End Function

        ''' <summary>
        ''' Fits the same 250-observation Poisson/log GLM used by the Core GLM
        ''' regression tests. The method deliberately enables residuals,
        ''' iteration details and covariance output so the WASM run exercises
        ''' IRLS, matrix algebra, distributions, residual diagnostics and the
        ''' host-neutral ResultTable pipeline.
        ''' </summary>
        Public Shared Function RunPoissonLogGlm() As GlmWasmSmokeResult
            Dim design As Double(,) = CreatePoissonLogData()
            Dim n As Integer = design.GetLength(0)

            Dim weights(n - 1) As Double
            Dim offsets(n - 1) As Double
            For i As Integer = 0 To n - 1
                weights(i) = 1.0R
                offsets(i) = 0.0R
            Next

            Dim model As New Global.BESHStatNG.GLM(
                New Global.BESHStatNG.regression.Poisson(),
                New Global.BESHStatNG.regression.Log())

            model.bHosmerLemeshow = False
            model.bComputeResiduals = True
            model.bIterationDetails = True
            model.bReturnCov = True
            model.setVarNames(New String() {"y", "x1", "x2"})
            model.data(design, Offset:=offsets, Weights:=weights)
            model.settingInputs(0.05R, 500, 0.000000000000001R)
            model.Fit(intercept:=1, bStartParams:=False)

            If Not model.Converged Then
                Throw New InvalidOperationException("Poisson/log GLM did not converge in WebAssembly.")
            End If
            If model.results Is Nothing Then
                Throw New InvalidOperationException("GLM results were not populated.")
            End If

            Dim coefficients() As Double = model.results.Coeffs_est
            Dim standardErrors() As Double = model.results.Coeffs_SEs
            Dim pValues() As Double = model.results.Coeffs_PvaluesZ

            If coefficients Is Nothing OrElse coefficients.Length <> 3 Then
                Throw New InvalidOperationException("Expected three GLM coefficients: Intercept, x1 and x2.")
            End If

            ' Reference values from BESHStat.Core.Tests/TestData/glm_expected_outputs.csv
            ' for Poisson_Log_Full.
            AssertClose(0.244548451325666R, coefficients(0), 0.0000001R, "Intercept coefficient")
            AssertClose(0.668389169387848R, coefficients(1), 0.0000001R, "x1 coefficient")
            AssertClose(-0.234556819791767R, coefficients(2), 0.0000001R, "x2 coefficient")

            AssertClose(0.0609498744133044R, standardErrors(0), 0.0000001R, "Intercept SE")
            AssertClose(0.0531997617602967R, standardErrors(1), 0.0000001R, "x1 SE")
            AssertClose(0.0514760048757186R, standardErrors(2), 0.0000001R, "x2 SE")

            AssertClose(0.0000601330675937817R, pValues(0), 0.00000001R, "Intercept p-value")
            AssertClose(0.0R, pValues(1), 0.00000001R, "x1 p-value")
            AssertClose(0.00000519822575939521R, pValues(2), 0.00000001R, "x2 p-value")

            AssertClose(713.482331282461R, model.AIC, 0.00001R, "AIC")
            AssertClose(-353.741165641231R, model.LogLikelihood, 0.00001R, "log-likelihood")
            AssertClose(0.418808253246421R, model.PseudoR2, 0.0000001R, "pseudo R2")
            AssertClose(0.583756533787331R, model.DevianceGOFpvalue, 0.0000001R, "deviance GOF p-value")
            AssertClose(0.906400317089388R, model.PearsonGOFpvalue, 0.0000001R, "Pearson GOF p-value")

            ' Exercise fitted values and residual calculations as well.
            Dim fitted() As Double = model.PredictedResponses
            If fitted Is Nothing OrElse fitted.Length <> n Then
                Throw New InvalidOperationException("Unexpected fitted-response vector length.")
            End If
            AssertClose(0.379420138762691R, fitted(0), 0.000001R, "first fitted response")

            Dim residuals As Object(,) = model.AllResiduals
            If residuals Is Nothing OrElse residuals.GetLength(0) <> n + 1 OrElse residuals.GetLength(1) <> 7 Then
                Throw New InvalidOperationException("Unexpected GLM residual table dimensions.")
            End If
            AssertClose(0.620579861237309R, Convert.ToDouble(residuals(1, 0)), 0.000001R, "first raw residual")
            AssertClose(0.834902726187081R, Convert.ToDouble(residuals(1, 1)), 0.000001R, "first deviance residual")
            AssertClose(1.00748244785321R, Convert.ToDouble(residuals(1, 2)), 0.000001R, "first Pearson residual")

            ' Force covariance computation and verify the matrix is finite.
            Dim vcov As Double(,) = model.VarCovar
            If vcov Is Nothing OrElse vcov.GetLength(0) <> 3 OrElse vcov.GetLength(1) <> 3 Then
                Throw New InvalidOperationException("Unexpected GLM covariance matrix dimensions.")
            End If
            For i As Integer = 0 To vcov.GetLength(0) - 1
                For j As Integer = 0 To vcov.GetLength(1) - 1
                    If Double.IsNaN(vcov(i, j)) OrElse Double.IsInfinity(vcov(i, j)) Then
                        Throw New InvalidOperationException("Covariance matrix contains a non-finite value.")
                    End If
                Next
            Next

            Dim tables As System.Collections.Generic.List(Of Global.BESHStatNG.ResultTable) =
                model.wrapResults(Nothing, Nothing)

            If tables Is Nothing OrElse tables.Count <> 4 Then
                Throw New InvalidOperationException(
                    "Expected coefficient, diagnostic, iteration and covariance ResultTables; got " &
                    If(tables Is Nothing, "Nothing", tables.Count.ToString(Globalization.CultureInfo.InvariantCulture)) & ".")
            End If

            Dim coefficientTable As Global.BESHStatNG.ResultTableOutputModel = tables(0).ToOutputModel()
            Dim diagnosticTable As Global.BESHStatNG.ResultTableOutputModel = tables(1).ToOutputModel()
            Dim iterationTable As Global.BESHStatNG.ResultTableOutputModel = tables(2).ToOutputModel()
            Dim covarianceTable As Global.BESHStatNG.ResultTableOutputModel = tables(3).ToOutputModel()

            If coefficientTable.HeaderTopRows <> 1 OrElse coefficientTable.HeaderLeftColumns <> 1 Then
                Throw New InvalidOperationException("Unexpected coefficient ResultTable header metadata.")
            End If
            If coefficientTable.PvalueColumns.Count <> 1 OrElse coefficientTable.PvalueColumns(0) <> 4 Then
                Throw New InvalidOperationException("Coefficient p-value column metadata was not preserved.")
            End If
            If diagnosticTable.PvalueColumns.Count <> 1 OrElse diagnosticTable.PvalueColumns(0) <> 3 Then
                Throw New InvalidOperationException("Diagnostic p-value column metadata was not preserved.")
            End If
            If iterationTable.RowCount = 0 OrElse iterationTable.ColumnCount = 0 Then
                Throw New InvalidOperationException("Iteration-detail ResultTable is empty.")
            End If
            If covarianceTable.RowCount = 0 OrElse covarianceTable.ColumnCount = 0 Then
                Throw New InvalidOperationException("Covariance ResultTable is empty.")
            End If

            Return New GlmWasmSmokeResult(
                coefficientTable,
                diagnosticTable,
                iterationTable,
                covarianceTable,
                residuals,
                coefficients,
                standardErrors,
                pValues,
                model.AIC,
                model.LogLikelihood,
                model.PseudoR2,
                model.DevianceGOFpvalue,
                model.PearsonGOFpvalue,
                n)
        End Function

        Private Shared Sub AssertClose(expected As Double,
                                       actual As Double,
                                       tolerance As Double,
                                       label As String)
            If Double.IsNaN(actual) OrElse Double.IsInfinity(actual) OrElse
               Math.Abs(expected - actual) > tolerance Then
                Throw New InvalidOperationException(
                    label & " mismatch. Expected " &
                    expected.ToString("G17", Globalization.CultureInfo.InvariantCulture) &
                    ", got " & actual.ToString("G17", Globalization.CultureInfo.InvariantCulture) & ".")
            End If
        End Sub

        Private Shared Function CreatePoissonLogData() As Double(,)
            Dim data(249, 2) As Double
            data(0, 0) = 1R : data(0, 1) = -1.423825036R : data(0, 2) = 1.116959041R
            data(1, 0) = 6R : data(1, 1) = 1.263728458R : data(1, 2) = -0.9440005415R
            data(2, 0) = 1R : data(2, 1) = -0.870661738R : data(2, 2) = 0.531406704R
            data(3, 0) = 2R : data(3, 1) = -0.2591732349R : data(3, 2) = 0.1933459969R
            data(4, 0) = 4R : data(4, 1) = -0.07534330701R : data(4, 2) = -1.118263583R
            data(5, 0) = 1R : data(5, 1) = -0.7408846521R : data(5, 2) = 0.5118455818R
            data(6, 0) = 1R : data(6, 1) = -1.367792702R : data(6, 2) = -2.270566289R
            data(7, 0) = 2R : data(7, 1) = 0.6488928022R : data(7, 2) = 0.263163546R
            data(8, 0) = 1R : data(8, 1) = 0.3610581131R : data(8, 2) = 2.471313494R
            data(9, 0) = 0R : data(9, 1) = -1.952863063R : data(9, 2) = -1.019885202R
            data(10, 0) = 4R : data(10, 1) = 2.347409654R : data(10, 2) = 0.01875261497R
            data(11, 0) = 7R : data(11, 1) = 0.9684969058R : data(11, 2) = -1.894264224R
            data(12, 0) = 0R : data(12, 1) = -0.7593871804R : data(12, 2) = -0.7550016558R
            data(13, 0) = 0R : data(13, 1) = 0.9021982742R : data(13, 2) = 0.7561977396R
            data(14, 0) = 0R : data(14, 1) = -0.4669531733R : data(14, 2) = -1.042462006R
            data(15, 0) = 1R : data(15, 1) = -0.06068951874R : data(15, 2) = -0.03425814296R
            data(16, 0) = 1R : data(16, 1) = 0.7888443445R : data(16, 2) = -0.3551682991R
            data(17, 0) = 1R : data(17, 1) = -1.256668133R : data(17, 2) = -0.3784283679R
            data(18, 0) = 4R : data(18, 1) = 0.5758575144R : data(18, 2) = 0.1906486885R
            data(19, 0) = 2R : data(19, 1) = 1.398978995R : data(19, 2) = 0.4843962939R
            data(20, 0) = 2R : data(20, 1) = 1.322298061R : data(20, 2) = 1.230267754R
            data(21, 0) = 0R : data(21, 1) = -0.2996985153R : data(21, 2) = 0.8329706229R
            data(22, 0) = 4R : data(22, 1) = 0.9029193414R : data(22, 2) = -0.5649417518R
            data(23, 0) = 0R : data(23, 1) = -1.621582734R : data(23, 2) = 1.414696013R
            data(24, 0) = 0R : data(24, 1) = -0.1581892607R : data(24, 2) = 1.248281217R
            data(25, 0) = 3R : data(25, 1) = 0.4494839321R : data(25, 2) = -1.558948099R
            data(26, 0) = 1R : data(26, 1) = -1.343601072R : data(26, 2) = 0.6652325922R
            data(27, 0) = 0R : data(27, 1) = -0.0816875907R : data(27, 2) = 0.8255951718R
            data(28, 0) = 4R : data(28, 1) = 1.724739932R : data(28, 2) = 0.9663188326R
            data(29, 0) = 10R : data(29, 1) = 2.618159426R : data(29, 2) = 0.5471752989R
            data(30, 0) = 4R : data(30, 1) = 0.7773613438R : data(30, 2) = -1.297183265R
            data(31, 0) = 4R : data(31, 1) = 0.8286331956R : data(31, 2) = -0.2676343332R
            data(32, 0) = 1R : data(32, 1) = -0.958988313R : data(32, 2) = -2.070206556R
            data(33, 0) = 0R : data(33, 1) = -1.209388287R : data(33, 2) = -0.1578925536R
            data(34, 0) = 0R : data(34, 1) = -1.412292013R : data(34, 2) = 2.028942624R
            data(35, 0) = 2R : data(35, 1) = 0.5415468299R : data(35, 2) = 0.681570885R
            data(36, 0) = 1R : data(36, 1) = 0.7519393956R : data(36, 2) = 0.8362035825R
            data(37, 0) = 1R : data(37, 1) = -0.6587603196R : data(37, 2) = -0.5134880466R
            data(38, 0) = 1R : data(38, 1) = -1.228674986R : data(38, 2) = -1.015736606R
            data(39, 0) = 3R : data(39, 1) = 0.2575577684R : data(39, 2) = -1.0702709R
            data(40, 0) = 2R : data(40, 1) = 0.3129029184R : data(40, 2) = 0.1239126576R
            data(41, 0) = 1R : data(41, 1) = -0.1308116902R : data(41, 2) = -0.8030158823R
            data(42, 0) = 4R : data(42, 1) = 1.26998312R : data(42, 2) = -0.6376357468R
            data(43, 0) = 2R : data(43, 1) = -0.09296245773R : data(43, 2) = 1.14449564R
            data(44, 0) = 2R : data(44, 1) = -0.066150889R : data(44, 2) = -1.640901914R
            data(45, 0) = 1R : data(45, 1) = -1.108214467R : data(45, 2) = -1.314003581R
            data(46, 0) = 1R : data(46, 1) = 0.1359568506R : data(46, 2) = -0.5103525097R
            data(47, 0) = 5R : data(47, 1) = 1.347077764R : data(47, 2) = -0.09986088394R
            data(48, 0) = 1R : data(48, 1) = 0.06114402098R : data(48, 2) = -0.1295067638R
            data(49, 0) = 2R : data(49, 1) = 0.07091460028R : data(49, 2) = -1.396705059R
            data(50, 0) = 0R : data(50, 1) = 0.4336545371R : data(50, 2) = 0.1938093509R
            data(51, 0) = 1R : data(51, 1) = 0.2774836599R : data(51, 2) = -0.1292927282R
            data(52, 0) = 1R : data(52, 1) = 0.5302523866R : data(52, 2) = 0.3544790943R
            data(53, 0) = 0R : data(53, 1) = 0.5367209691R : data(53, 2) = -1.082872637R
            data(54, 0) = 2R : data(54, 1) = 0.6183500148R : data(54, 2) = 0.2449392348R
            data(55, 0) = 1R : data(55, 1) = -0.7950174561R : data(55, 2) = 0.2208207909R
            data(56, 0) = 3R : data(56, 1) = 0.3000309463R : data(56, 2) = -0.6604324845R
            data(57, 0) = 0R : data(57, 1) = -1.602701592R : data(57, 2) = -0.2185351507R
            data(58, 0) = 2R : data(58, 1) = 0.2667988297R : data(58, 2) = -0.5559990124R
            data(59, 0) = 0R : data(59, 1) = -1.261623782R : data(59, 2) = 1.356381012R
            data(60, 0) = 1R : data(60, 1) = -0.07127080619R : data(60, 2) = -3.119608626R
            data(61, 0) = 4R : data(61, 1) = 0.4740497303R : data(61, 2) = -1.447671697R
            data(62, 0) = 0R : data(62, 1) = -0.4148537611R : data(62, 2) = -1.705785395R
            data(63, 0) = 2R : data(63, 1) = 0.09771650001R : data(63, 2) = -0.3781199227R
            data(64, 0) = 0R : data(64, 1) = -1.640417837R : data(64, 2) = -0.6784939159R
            data(65, 0) = 0R : data(65, 1) = -0.8572588239R : data(65, 2) = -0.4112459345R
            data(66, 0) = 0R : data(66, 1) = 0.6882817881R : data(66, 2) = 0.7190126437R
            data(67, 0) = 1R : data(67, 1) = -1.154529583R : data(67, 2) = -1.632519477R
            data(68, 0) = 2R : data(68, 1) = 0.6504523891R : data(68, 2) = -0.845693631R
            data(69, 0) = 1R : data(69, 1) = -1.388359953R : data(69, 2) = -0.2231000585R
            data(70, 0) = 1R : data(70, 1) = -0.9073824574R : data(70, 2) = -1.20221832R
            data(71, 0) = 0R : data(71, 1) = -1.095425307R : data(71, 2) = 0.3699876832R
            data(72, 0) = 1R : data(72, 1) = 0.00714569494R : data(72, 2) = 0.3299973578R
            data(73, 0) = 4R : data(73, 1) = 0.534359903R : data(73, 2) = -0.7104466108R
            data(74, 0) = 0R : data(74, 1) = -1.065807847R : data(74, 2) = -1.771107162R
            data(75, 0) = 0R : data(75, 1) = -0.1814727402R : data(75, 2) = 1.600835847R
            data(76, 0) = 10R : data(76, 1) = 1.621951799R : data(76, 2) = 0.4710980653R
            data(77, 0) = 2R : data(77, 1) = -0.3173919457R : data(77, 2) = 0.4237506251R
            data(78, 0) = 0R : data(78, 1) = -0.8158149669R : data(78, 2) = 0.07226048142R
            data(79, 0) = 1R : data(79, 1) = 0.3865790173R : data(79, 2) = -0.007346966624R
            data(80, 0) = 1R : data(80, 1) = -0.2236389259R : data(80, 2) = 0.8310756729R
            data(81, 0) = 1R : data(81, 1) = -0.7016908087R : data(81, 2) = -0.01560552491R
            data(82, 0) = 2R : data(82, 1) = -1.795713176R : data(82, 2) = -0.6309063384R
            data(83, 0) = 2R : data(83, 1) = 0.8183256215R : data(83, 2) = 0.7095013848R
            data(84, 0) = 2R : data(84, 1) = -0.5710329018R : data(84, 2) = 0.05969517076R
            data(85, 0) = 1R : data(85, 1) = 0.0007855250626R : data(85, 2) = -0.7724757474R
            data(86, 0) = 0R : data(86, 1) = -1.063642717R : data(86, 2) = -0.9062390936R
            data(87, 0) = 2R : data(87, 1) = 1.3017145R : data(87, 2) = 1.165479282R
            data(88, 0) = 1R : data(88, 1) = 0.7478729421R : data(88, 2) = 0.6310474021R
            data(89, 0) = 0R : data(89, 1) = 0.9808759092R : data(89, 2) = 1.97551858R
            data(90, 0) = 1R : data(90, 1) = -0.1104186881R : data(90, 2) = -0.5266551964R
            data(91, 0) = 0R : data(91, 1) = 0.4679185306R : data(91, 2) = -0.7435405866R
            data(92, 0) = 1R : data(92, 1) = 0.8906071497R : data(92, 2) = 0.4038905714R
            data(93, 0) = 1R : data(93, 1) = 1.023009366R : data(93, 2) = 1.379406853R
            data(94, 0) = 2R : data(94, 1) = 0.3123833894R : data(94, 2) = 0.5967524014R
            data(95, 0) = 0R : data(95, 1) = -0.06190468566R : data(95, 2) = 0.4331530724R
            data(96, 0) = 1R : data(96, 1) = -0.3594796473R : data(96, 2) = 1.886988066R
            data(97, 0) = 1R : data(97, 1) = -0.7486439843R : data(97, 2) = -0.7433909389R
            data(98, 0) = 1R : data(98, 1) = -0.9654789075R : data(98, 2) = -1.24794902R
            data(99, 0) = 3R : data(99, 1) = 0.3600346574R : data(99, 2) = -0.7446183165R
            data(100, 0) = 1R : data(100, 1) = -0.2445525323R : data(100, 2) = -0.3168569887R
            data(101, 0) = 0R : data(101, 1) = -1.995856611R : data(101, 2) = 0.373351314R
            data(102, 0) = 1R : data(102, 1) = -0.1552476168R : data(102, 2) = 1.021960352R
            data(103, 0) = 1R : data(103, 1) = 1.063830875R : data(103, 2) = 0.4056097228R
            data(104, 0) = 0R : data(104, 1) = -0.2751715673R : data(104, 2) = 1.52331375R
            data(105, 0) = 0R : data(105, 1) = -1.85333593R : data(105, 2) = -0.8275081282R
            data(106, 0) = 0R : data(106, 1) = -0.1243419278R : data(106, 2) = 1.905495461R
            data(107, 0) = 1R : data(107, 1) = 0.7849745223R : data(107, 2) = -0.8120216932R
            data(108, 0) = 3R : data(108, 1) = 0.2019985967R : data(108, 2) = 1.170736237R
            data(109, 0) = 1R : data(109, 1) = -0.4280744428R : data(109, 2) = -0.9378429667R
            data(110, 0) = 6R : data(110, 1) = 1.848288896R : data(110, 2) = 0.3432670067R
            data(111, 0) = 3R : data(111, 1) = 1.899952893R : data(111, 2) = 0.05089841017R
            data(112, 0) = 1R : data(112, 1) = -0.09842503478R : data(112, 2) = 0.5175596454R
            data(113, 0) = 3R : data(113, 1) = 0.8134454398R : data(113, 2) = 0.7514247641R
            data(114, 0) = 3R : data(114, 1) = 0.3924943886R : data(114, 2) = -0.2131991712R
            data(115, 0) = 2R : data(115, 1) = 0.7814429005R : data(115, 2) = -0.06682367759R
            data(116, 0) = 4R : data(116, 1) = 1.453271517R : data(116, 2) = 0.2391117183R
            data(117, 0) = 3R : data(117, 1) = 0.8201860453R : data(117, 2) = -1.305008144R
            data(118, 0) = 1R : data(118, 1) = 0.08770534456R : data(118, 2) = -0.7312364398R
            data(119, 0) = 1R : data(119, 1) = -0.6535056482R : data(119, 2) = -1.624406842R
            data(120, 0) = 0R : data(120, 1) = -0.8118868793R : data(120, 2) = 0.005250509615R
            data(121, 0) = 3R : data(121, 1) = -0.0255381724R : data(121, 2) = 2.222156961R
            data(122, 0) = 2R : data(122, 1) = 1.158184543R : data(122, 2) = 0.7413092237R
            data(123, 0) = 2R : data(123, 1) = 0.3005208697R : data(123, 2) = -0.8654297067R
            data(124, 0) = 3R : data(124, 1) = 0.0530566461R : data(124, 2) = -0.7076846959R
            data(125, 0) = 0R : data(125, 1) = 0.2572715238R : data(125, 2) = 0.9758987637R
            data(126, 0) = 1R : data(126, 1) = 0.0357428624R : data(126, 2) = -0.4744093827R
            data(127, 0) = 2R : data(127, 1) = 0.5472366859R : data(127, 2) = -1.608482025R
            data(128, 0) = 0R : data(128, 1) = -1.122961579R : data(128, 2) = -1.022915552R
            data(129, 0) = 2R : data(129, 1) = -1.975247668R : data(129, 2) = -1.054256424R
            data(130, 0) = 0R : data(130, 1) = -0.4251500496R : data(130, 2) = -1.021398712R
            data(131, 0) = 0R : data(131, 1) = -1.149073821R : data(131, 2) = 0.6759508198R
            data(132, 0) = 2R : data(132, 1) = 1.615138054R : data(132, 2) = 0.07070287188R
            data(133, 0) = 2R : data(133, 1) = -0.1584768602R : data(133, 2) = -1.488639453R
            data(134, 0) = 5R : data(134, 1) = -0.2528733453R : data(134, 2) = -0.5397969927R
            data(135, 0) = 0R : data(135, 1) = -1.538154035R : data(135, 2) = -1.138321024R
            data(136, 0) = 0R : data(136, 1) = 0.2820860329R : data(136, 2) = -1.208420523R
            data(137, 0) = 1R : data(137, 1) = -0.6236121343R : data(137, 2) = -1.506674916R
            data(138, 0) = 0R : data(138, 1) = 1.121822263R : data(138, 2) = 1.747162038R
            data(139, 0) = 3R : data(139, 1) = 0.841221031R : data(139, 2) = -0.3073295207R
            data(140, 0) = 1R : data(140, 1) = -0.7758961005R : data(140, 2) = 0.5762305842R
            data(141, 0) = 1R : data(141, 1) = 0.4107164402R : data(141, 2) = 0.868743678R
            data(142, 0) = 0R : data(142, 1) = -2.722416104R : data(142, 2) = -0.3842716659R
            data(143, 0) = 1R : data(143, 1) = -0.6733047988R : data(143, 2) = 0.5349195393R
            data(144, 0) = 3R : data(144, 1) = 1.246221529R : data(144, 2) = 0.5406158791R
            data(145, 0) = 2R : data(145, 1) = 0.7902080282R : data(145, 2) = 0.2224565209R
            data(146, 0) = 2R : data(146, 1) = 0.1753408885R : data(146, 2) = 2.169532129R
            data(147, 0) = 3R : data(147, 1) = -0.02929460165R : data(147, 2) = 0.283928102R
            data(148, 0) = 0R : data(148, 1) = -1.419514257R : data(148, 2) = -0.2284905136R
            data(149, 0) = 0R : data(149, 1) = -1.359966324R : data(149, 2) = 0.6461644796R
            data(150, 0) = 3R : data(150, 1) = 0.2234115586R : data(150, 2) = 1.77796354R
            data(151, 0) = 1R : data(151, 1) = 1.761779432R : data(151, 2) = 0.9943080003R
            data(152, 0) = 0R : data(152, 1) = -2.170889847R : data(152, 2) = -0.478299892R
            data(153, 0) = 2R : data(153, 1) = 0.6284881711R : data(153, 2) = -0.8587645744R
            data(154, 0) = 3R : data(154, 1) = 0.601196526R : data(154, 2) = -0.9308502857R
            data(155, 0) = 3R : data(155, 1) = 0.9507578581R : data(155, 2) = -0.6618993556R
            data(156, 0) = 0R : data(156, 1) = -0.869246667R : data(156, 2) = 0.3865009026R
            data(157, 0) = 1R : data(157, 1) = -0.5290070709R : data(157, 2) = 0.8459882923R
            data(158, 0) = 2R : data(158, 1) = 0.04568409556R : data(158, 2) = -2.269947874R
            data(159, 0) = 1R : data(159, 1) = -1.027551811R : data(159, 2) = -0.01866816541R
            data(160, 0) = 1R : data(160, 1) = -1.229289299R : data(160, 2) = -1.105018883R
            data(161, 0) = 1R : data(161, 1) = -0.8833584719R : data(161, 2) = 0.6828997317R
            data(162, 0) = 2R : data(162, 1) = -0.07089346369R : data(162, 2) = 0.9145184925R
            data(163, 0) = 3R : data(163, 1) = 0.3740533509R : data(163, 2) = -0.7783521151R
            data(164, 0) = 1R : data(164, 1) = -0.02459373836R : data(164, 2) = -0.449423204R
            data(165, 0) = 3R : data(165, 1) = 0.07726065834R : data(165, 2) = -0.9516186303R
            data(166, 0) = 2R : data(166, 1) = -0.683913218R : data(166, 2) = 0.3910785348R
            data(167, 0) = 1R : data(167, 1) = -0.7208376678R : data(167, 2) = -1.609961814R
            data(168, 0) = 5R : data(168, 1) = 1.120622815R : data(168, 2) = -1.125575245R
            data(169, 0) = 1R : data(169, 1) = -0.05481416027R : data(169, 2) = -0.6846238369R
            data(170, 0) = 2R : data(170, 1) = -0.08241372418R : data(170, 2) = 1.226518956R
            data(171, 0) = 0R : data(171, 1) = 0.9359865008R : data(171, 2) = 2.402472801R
            data(172, 0) = 2R : data(172, 1) = 1.238537125R : data(172, 2) = 0.019958419R
            data(173, 0) = 2R : data(173, 1) = 1.272795525R : data(173, 2) = -1.231571245R
            data(174, 0) = 2R : data(174, 1) = 0.4058922175R : data(174, 2) = 0.07418646851R
            data(175, 0) = 0R : data(175, 1) = -0.05032522141R : data(175, 2) = -0.8690495928R
            data(176, 0) = 1R : data(176, 1) = 0.2893175389R : data(176, 2) = 0.4598195677R
            data(177, 0) = 0R : data(177, 1) = 0.1793056768R : data(177, 2) = -1.113704507R
            data(178, 0) = 4R : data(178, 1) = 1.397480556R : data(178, 2) = -0.3694873502R
            data(179, 0) = 4R : data(179, 1) = 0.2920467917R : data(179, 2) = -0.01410552787R
            data(180, 0) = 1R : data(180, 1) = 0.6384056744R : data(180, 2) = 0.519943798R
            data(181, 0) = 0R : data(181, 1) = -0.02788771315R : data(181, 2) = -0.07688849331R
            data(182, 0) = 4R : data(182, 1) = 1.37105185R : data(182, 2) = 1.265888584R
            data(183, 0) = 1R : data(183, 1) = -2.052807629R : data(183, 2) = 0.1242327616R
            data(184, 0) = 3R : data(184, 1) = 0.3805090784R : data(184, 2) = -1.813814914R
            data(185, 0) = 3R : data(185, 1) = 0.7553906693R : data(185, 2) = 0.9240590308R
            data(186, 0) = 0R : data(186, 1) = -1.159125851R : data(186, 2) = -0.2911134059R
            data(187, 0) = 7R : data(187, 1) = 2.150310115R : data(187, 2) = -0.6697622415R
            data(188, 0) = 2R : data(188, 1) = -0.1502702186R : data(188, 2) = 1.507684682R
            data(189, 0) = 0R : data(189, 1) = -0.1611642798R : data(189, 2) = -0.9936061885R
            data(190, 0) = 2R : data(190, 1) = -1.079442475R : data(190, 2) = 0.07266743711R
            data(191, 0) = 1R : data(191, 1) = 0.8779661845R : data(191, 2) = -0.712974484R
            data(192, 0) = 2R : data(192, 1) = 0.2244674139R : data(192, 2) = 1.352428933R
            data(193, 0) = 0R : data(193, 1) = -0.5915934535R : data(193, 2) = -0.3640172856R
            data(194, 0) = 1R : data(194, 1) = 0.2262628022R : data(194, 2) = 0.2356994777R
            data(195, 0) = 4R : data(195, 1) = 0.686182507R : data(195, 2) = -1.392284646R
            data(196, 0) = 1R : data(196, 1) = 1.215004608R : data(196, 2) = 1.434733987R
            data(197, 0) = 3R : data(197, 1) = 0.2160594414R : data(197, 2) = -1.069520424R
            data(198, 0) = 0R : data(198, 1) = -0.9648235573R : data(198, 2) = 0.9104593232R
            data(199, 0) = 0R : data(199, 1) = -0.5566078047R : data(199, 2) = 1.113705592R
            data(200, 0) = 0R : data(200, 1) = -2.29838764R : data(200, 2) = 1.232315379R
            data(201, 0) = 0R : data(201, 1) = -0.7320821261R : data(201, 2) = 1.814112808R
            data(202, 0) = 4R : data(202, 1) = 0.7364691004R : data(202, 2) = -1.17073802R
            data(203, 0) = 1R : data(203, 1) = 0.4657167185R : data(203, 2) = 0.4019821781R
            data(204, 0) = 2R : data(204, 1) = -0.1078760488R : data(204, 2) = 0.1379696495R
            data(205, 0) = 1R : data(205, 1) = -0.3414362942R : data(205, 2) = 0.419915493R
            data(206, 0) = 1R : data(206, 1) = 1.584533793R : data(206, 2) = 1.098319538R
            data(207, 0) = 3R : data(207, 1) = 0.2822412062R : data(207, 2) = 0.1093617683R
            data(208, 0) = 3R : data(208, 1) = 0.9095463908R : data(208, 2) = -0.8169192121R
            data(209, 0) = 1R : data(209, 1) = 0.3950715661R : data(209, 2) = -0.09725296817R
            data(210, 0) = 0R : data(210, 1) = -0.6693765192R : data(210, 2) = 1.539236434R
            data(211, 0) = 3R : data(211, 1) = 1.555368981R : data(211, 2) = -1.178987468R
            data(212, 0) = 0R : data(212, 1) = -1.238139031R : data(212, 2) = 0.07968424618R
            data(213, 0) = 0R : data(213, 1) = -1.196173461R : data(213, 2) = -0.9813006656R
            data(214, 0) = 2R : data(214, 1) = -0.4291495058R : data(214, 2) = 1.108779407R
            data(215, 0) = 1R : data(215, 1) = -0.7296598932R : data(215, 2) = -1.648448621R
            data(216, 0) = 4R : data(216, 1) = -0.5574688991R : data(216, 2) = -0.3410864686R
            data(217, 0) = 1R : data(217, 1) = -0.5999530643R : data(217, 2) = -0.1241949656R
            data(218, 0) = 5R : data(218, 1) = 0.9868272044R : data(218, 2) = 0.1169157395R
            data(219, 0) = 0R : data(219, 1) = 0.05419467513R : data(219, 2) = 2.3612187R
            data(220, 0) = 0R : data(220, 1) = 0.3519074427R : data(220, 2) = 1.55516878R
            data(221, 0) = 0R : data(221, 1) = -1.587969509R : data(221, 2) = -1.029859405R
            data(222, 0) = 0R : data(222, 1) = -0.8469513494R : data(222, 2) = 2.159404453R
            data(223, 0) = 2R : data(223, 1) = 1.084570262R : data(223, 2) = -1.192533426R
            data(224, 0) = 2R : data(224, 1) = -1.203826645R : data(224, 2) = -1.870890503R
            data(225, 0) = 3R : data(225, 1) = 1.178530894R : data(225, 2) = -1.498482052R
            data(226, 0) = 0R : data(226, 1) = -1.030665854R : data(226, 2) = 0.2034504706R
            data(227, 0) = 0R : data(227, 1) = 0.299218328R : data(227, 2) = 0.5348605862R
            data(228, 0) = 1R : data(228, 1) = -0.8462399711R : data(228, 2) = -2.211085873R
            data(229, 0) = 0R : data(229, 1) = 0.1966203005R : data(229, 2) = 0.4359741911R
            data(230, 0) = 1R : data(230, 1) = -0.8996378247R : data(230, 2) = -0.2670521689R
            data(231, 0) = 3R : data(231, 1) = -0.2566054287R : data(231, 2) = 0.7063895323R
            data(232, 0) = 3R : data(232, 1) = 1.672547766R : data(232, 2) = -0.3846068147R
            data(233, 0) = 1R : data(233, 1) = -0.3752696462R : data(233, 2) = 0.9000817112R
            data(234, 0) = 3R : data(234, 1) = 2.03679478R : data(234, 2) = 0.5643451297R
            data(235, 0) = 0R : data(235, 1) = -0.4587933977R : data(235, 2) = -1.172644994R
            data(236, 0) = 0R : data(236, 1) = -1.175769659R : data(236, 2) = 1.445591995R
            data(237, 0) = 0R : data(237, 1) = 0.0750524988R : data(237, 2) = 0.2124271284R
            data(238, 0) = 1R : data(238, 1) = -0.408990215R : data(238, 2) = 0.7300488205R
            data(239, 0) = 2R : data(239, 1) = 1.756531983R : data(239, 2) = 1.261771936R
            data(240, 0) = 2R : data(240, 1) = 0.8609230686R : data(240, 2) = -0.9747583323R
            data(241, 0) = 3R : data(241, 1) = 1.181273983R : data(241, 2) = -0.08473721063R
            data(242, 0) = 3R : data(242, 1) = 0.6316701713R : data(242, 2) = -0.8216979687R
            data(243, 0) = 6R : data(243, 1) = 2.470997111R : data(243, 2) = -0.9423727322R
            data(244, 0) = 2R : data(244, 1) = 0.7943025166R : data(244, 2) = 0.2659795462R
            data(245, 0) = 2R : data(245, 1) = 0.5313528781R : data(245, 2) = 0.3397911075R
            data(246, 0) = 1R : data(246, 1) = -0.8293985972R : data(246, 2) = 1.331403494R
            data(247, 0) = 0R : data(247, 1) = -0.9093038343R : data(247, 2) = 1.385540642R
            data(248, 0) = 2R : data(248, 1) = 0.1842359336R : data(248, 2) = -0.02135830344R
            data(249, 0) = 4R : data(249, 1) = 0.9977381754R : data(249, 2) = -0.8370647257R
            Return data
        End Function

    End Class

End Namespace
